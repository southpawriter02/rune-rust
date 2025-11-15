using RuneAndRust.Core;
using Serilog;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.27.1: Service for managing performance state for Skald specialization
/// Tracks active performances, durations, interruptions, and performance-specific mechanics
/// </summary>
public class PerformanceStateService
{
    private static readonly ILogger _log = Log.ForContext<PerformanceStateService>();
    private readonly string _connectionString;

    public PerformanceStateService(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("PerformanceStateService initialized");
    }

    #region Performance Management

    /// <summary>
    /// Start a new performance for a character
    /// </summary>
    /// <param name="characterId">The Skald character ID</param>
    /// <param name="abilityId">The performance ability ID</param>
    /// <param name="rank">Ability rank (1-3)</param>
    /// <param name="willScore">Character's WILL score</param>
    /// <param name="enduringPerformanceRank">Rank of Enduring Performance ability (0 if not learned)</param>
    /// <returns>True if performance started successfully</returns>
    public bool StartPerformance(int characterId, int abilityId, int rank, int willScore, int enduringPerformanceRank = 0)
    {
        using var operation = _log.BeginOperation("StartPerformance");

        try
        {
            // Check if already performing
            if (IsPerforming(characterId))
            {
                _log.Warning("Cannot start performance - already performing: CharacterID={CharacterId}", characterId);
                return false;
            }

            // Calculate duration based on WILL + Enduring Performance bonuses
            int baseDuration = willScore;
            int enduringBonus = GetEnduringPerformanceBonus(enduringPerformanceRank);
            int totalDuration = baseDuration + enduringBonus;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // First, ensure the character has a performance state record
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = @"
                        INSERT OR IGNORE INTO Characters_Performances
                        (character_id, is_performing)
                        VALUES (@CharacterId, 0)";
                    checkCmd.Parameters.AddWithValue("@CharacterId", characterId);
                    checkCmd.ExecuteNonQuery();
                }

                // Start performance
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE Characters_Performances
                        SET is_performing = 1,
                            current_performance_ability_id = @AbilityId,
                            performance_duration_remaining = @Duration,
                            performance_rank = @Rank,
                            can_use_standard_action = 0,
                            updated_at = @Timestamp
                        WHERE character_id = @CharacterId";

                    cmd.Parameters.AddWithValue("@CharacterId", characterId);
                    cmd.Parameters.AddWithValue("@AbilityId", abilityId);
                    cmd.Parameters.AddWithValue("@Duration", totalDuration);
                    cmd.Parameters.AddWithValue("@Rank", rank);
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                    cmd.ExecuteNonQuery();
                }
            }

            _log.Information(
                "Performance started: CharacterID={CharacterId}, AbilityID={AbilityId}, Duration={Duration} rounds",
                characterId, abilityId, totalDuration);

            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Failed to start performance: CharacterID={CharacterId}, AbilityID={AbilityId}",
                characterId, abilityId);
            throw;
        }
    }

    /// <summary>
    /// Interrupt an active performance
    /// </summary>
    /// <param name="characterId">The character whose performance to interrupt</param>
    /// <param name="reason">Reason for interruption (e.g., "[Stunned]", "voluntarily ended")</param>
    /// <returns>True if performance was interrupted</returns>
    public bool InterruptPerformance(int characterId, string reason)
    {
        using var operation = _log.BeginOperation("InterruptPerformance");

        try
        {
            if (!IsPerforming(characterId))
                return false;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE Characters_Performances
                        SET is_performing = 0,
                            current_performance_ability_id = NULL,
                            performance_duration_remaining = 0,
                            interrupted_this_combat = 1,
                            can_use_standard_action = 1,
                            updated_at = @Timestamp
                        WHERE character_id = @CharacterId";

                    cmd.Parameters.AddWithValue("@CharacterId", characterId);
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                    cmd.ExecuteNonQuery();
                }
            }

            _log.Warning("Performance interrupted: CharacterID={CharacterId}, Reason={Reason}", characterId, reason);

            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to interrupt performance: CharacterID={CharacterId}", characterId);
            throw;
        }
    }

    /// <summary>
    /// Check if a character is currently performing
    /// </summary>
    public bool IsPerforming(int characterId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COALESCE(is_performing, 0)
                    FROM Characters_Performances
                    WHERE character_id = @CharacterId";

                cmd.Parameters.AddWithValue("@CharacterId", characterId);

                var result = cmd.ExecuteScalar();
                return result != null && Convert.ToInt32(result) == 1;
            }
        }
    }

    /// <summary>
    /// Decrement the duration of an active performance at the end of a turn
    /// </summary>
    public void DecrementPerformanceDuration(int characterId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            // Get current duration
            int remaining;
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COALESCE(performance_duration_remaining, 0)
                    FROM Characters_Performances
                    WHERE character_id = @CharacterId";

                cmd.Parameters.AddWithValue("@CharacterId", characterId);

                var result = cmd.ExecuteScalar();
                remaining = result != null ? Convert.ToInt32(result) : 0;
            }

            if (remaining <= 1)
            {
                // Performance ends naturally
                EndPerformance(characterId, "duration expired");
            }
            else
            {
                // Decrement duration
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE Characters_Performances
                        SET performance_duration_remaining = performance_duration_remaining - 1,
                            updated_at = @Timestamp
                        WHERE character_id = @CharacterId";

                    cmd.Parameters.AddWithValue("@CharacterId", characterId);
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    /// <summary>
    /// End a performance (naturally or voluntarily)
    /// </summary>
    public void EndPerformance(int characterId, string reason)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE Characters_Performances
                    SET is_performing = 0,
                        current_performance_ability_id = NULL,
                        performance_duration_remaining = 0,
                        can_use_standard_action = 1,
                        updated_at = @Timestamp
                    WHERE character_id = @CharacterId";

                cmd.Parameters.AddWithValue("@CharacterId", characterId);
                cmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                cmd.ExecuteNonQuery();
            }
        }

        _log.Information("Performance ended: CharacterID={CharacterId}, Reason={Reason}", characterId, reason);
    }

    /// <summary>
    /// Reset performance state at the start of combat
    /// </summary>
    public void ResetCombatState(int characterId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE Characters_Performances
                    SET is_performing = 0,
                        current_performance_ability_id = NULL,
                        performance_duration_remaining = 0,
                        interrupted_this_combat = 0,
                        can_use_standard_action = 1,
                        updated_at = @Timestamp
                    WHERE character_id = @CharacterId";

                cmd.Parameters.AddWithValue("@CharacterId", characterId);
                cmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                cmd.ExecuteNonQuery();
            }
        }

        _log.Debug("Performance state reset for combat: CharacterID={CharacterId}", characterId);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get the duration bonus from Enduring Performance ability
    /// </summary>
    private int GetEnduringPerformanceBonus(int rank)
    {
        return rank switch
        {
            1 => 2,
            2 => 3,
            3 => 4,
            _ => 0
        };
    }

    /// <summary>
    /// Get current performance info for a character
    /// </summary>
    public (int abilityId, int rank, int durationRemaining)? GetCurrentPerformance(int characterId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT current_performance_ability_id, performance_rank, performance_duration_remaining
                    FROM Characters_Performances
                    WHERE character_id = @CharacterId AND is_performing = 1";

                cmd.Parameters.AddWithValue("@CharacterId", characterId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int abilityId = reader.GetInt32(0);
                        int rank = reader.GetInt32(1);
                        int duration = reader.GetInt32(2);
                        return (abilityId, rank, duration);
                    }
                }
            }
        }

        return null;
    }

    #endregion
}
