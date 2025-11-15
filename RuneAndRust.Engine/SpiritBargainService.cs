using Microsoft.Data.Sqlite;
using Serilog;
using System;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a Spirit Bargain operation
/// </summary>
public class BargainResult
{
    public bool Success { get; set; }
    public bool Guaranteed { get; set; }
    public float Roll { get; set; }
    public float FinalChance { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// v0.28.1: Service for managing the Seidkona's Spirit Bargain system.
/// Spirit Bargains are unpredictable bonus effects that trigger on spell casts.
/// Base chances are modified by Fickle Fortune passive, Psychic Resonance zones, and Moment of Clarity.
/// </summary>
public class SpiritBargainService
{
    private static readonly ILogger _log = Log.ForContext<SpiritBargainService>();
    private readonly string _connectionString;
    private readonly Random _random;

    public SpiritBargainService(string connectionString, Random? random = null)
    {
        _connectionString = connectionString;
        _random = random ?? new Random();
        _log.Debug("SpiritBargainService initialized");
    }

    /// <summary>
    /// Initialize Spirit Bargain tracking for a character (creates entry if doesn't exist)
    /// </summary>
    public bool InitializeSpiritBargains(int characterId)
    {
        _log.Debug("Initializing Spirit Bargains for character {CharacterId}", characterId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR IGNORE INTO Characters_SpiritBargains (
                    character_id, total_bargains_triggered, total_bargains_attempted,
                    bargain_success_rate, fickle_fortune_rank, in_moment_of_clarity,
                    clarity_turns_remaining, clarity_uses_this_combat,
                    forced_bargain_used_this_combat, psychic_resonance_bonus_active,
                    created_at, updated_at
                ) VALUES (
                    @CharacterId, 0, 0, 0.0, 0, 0, 0, 0, 0, 0,
                    datetime('now'), datetime('now')
                )
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.ExecuteNonQuery();

            _log.Information("Spirit Bargains initialized for character {CharacterId}", characterId);
            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize Spirit Bargains for character {CharacterId}", characterId);
            return false;
        }
    }

    /// <summary>
    /// Attempt a Spirit Bargain with base probability and modifiers
    /// </summary>
    /// <param name="characterId">Character ID</param>
    /// <param name="abilityName">Name of the ability triggering the bargain</param>
    /// <param name="baseChance">Base probability (0.0-1.0)</param>
    /// <returns>Bargain result with success status and details</returns>
    public BargainResult AttemptSpiritBargain(int characterId, string abilityName, float baseChance)
    {
        _log.Debug("Attempting Spirit Bargain: CharacterId={CharacterId}, Ability={Ability}, BaseChance={Chance}%",
            characterId, abilityName, baseChance * 100);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Check if in Moment of Clarity
            var inClarity = GetMomentOfClarityStatus(connection, characterId);

            if (inClarity)
            {
                // Guaranteed success during Clarity
                RecordBargainSuccess(connection, characterId);
                _log.Information("Spirit Bargain GUARANTEED (Clarity): CharacterId={CharacterId}, Ability={Ability}",
                    characterId, abilityName);

                return new BargainResult
                {
                    Success = true,
                    Guaranteed = true,
                    Roll = 0.0f,
                    FinalChance = 1.0f,
                    Message = "[Moment of Clarity] Spirit Bargain guaranteed!"
                };
            }

            // Get Fickle Fortune rank
            int fickleRank = GetFickleFortuneRank(connection, characterId);
            float fickleBonus = fickleRank switch
            {
                1 => 0.15f,  // +15%
                2 => 0.20f,  // +20%
                3 => 0.25f,  // +25%
                _ => 0.0f
            };

            // Check Psychic Resonance zone bonus
            bool inResonance = GetPsychicResonanceStatus(connection, characterId);
            float resonanceBonus = inResonance ? 0.15f : 0.0f;

            // Calculate final chance
            float finalChance = Math.Clamp(baseChance + fickleBonus + resonanceBonus, 0.0f, 1.0f);

            // Check if forced bargain is available (Fickle Fortune Rank 3)
            bool canForceBargain = fickleRank >= 3 && !IsForcedBargainUsed(connection, characterId);

            // Roll for bargain
            float roll = (float)_random.NextDouble();
            bool success = roll <= finalChance;

            // Apply forced bargain if needed
            if (!success && canForceBargain)
            {
                _log.Warning("Spirit Bargain FORCED (Fickle Fortune Rank 3): CharacterId={CharacterId}, Roll={Roll} vs Chance={Chance}%",
                    characterId, roll, finalChance * 100);
                success = true;
                MarkForcedBargainUsed(connection, characterId);
            }

            // Record result
            if (success)
            {
                RecordBargainSuccess(connection, characterId);
            }
            else
            {
                RecordBargainFailure(connection, characterId);
            }

            _log.Information("Spirit Bargain attempt: CharacterId={CharacterId}, Ability={Ability}, Success={Success}, Roll={Roll}, FinalChance={Chance}%",
                characterId, abilityName, success, roll, finalChance * 100);

            return new BargainResult
            {
                Success = success,
                Guaranteed = false,
                Roll = roll,
                FinalChance = finalChance,
                Message = success
                    ? $"[Spirit Bargain] Bonus effect triggered! (rolled {roll:F2} vs {finalChance:F2})"
                    : $"Spirit Bargain failed (rolled {roll:F2} vs {finalChance:F2})"
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to attempt Spirit Bargain: CharacterId={CharacterId}, Ability={Ability}",
                characterId, abilityName);
            return new BargainResult
            {
                Success = false,
                Guaranteed = false,
                Message = $"Error attempting Spirit Bargain: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Activate Moment of Clarity for specified duration
    /// </summary>
    public bool ActivateMomentOfClarity(int characterId, int duration, int rank)
    {
        _log.Information("Activating Moment of Clarity: CharacterId={CharacterId}, Duration={Duration}, Rank={Rank}",
            characterId, duration, rank);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Check usage limit
            int usesThisCombat = GetClarityUsesThisCombat(connection, characterId);
            int maxUses = rank >= 3 ? 2 : 1;

            if (usesThisCombat >= maxUses)
            {
                _log.Warning("Moment of Clarity already used maximum times this combat: CharacterId={CharacterId}, Uses={Uses}/{Max}",
                    characterId, usesThisCombat, maxUses);
                return false;
            }

            // Activate Clarity
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_SpiritBargains
                SET in_moment_of_clarity = 1,
                    clarity_turns_remaining = @Duration,
                    clarity_uses_this_combat = clarity_uses_this_combat + 1,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.Parameters.AddWithValue("@Duration", duration);
            command.ExecuteNonQuery();

            _log.Information("MOMENT OF CLARITY ACTIVATED: CharacterId={CharacterId}, Duration={Duration} turns, Rank={Rank}",
                characterId, duration, rank);

            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to activate Moment of Clarity: CharacterId={CharacterId}", characterId);
            return false;
        }
    }

    /// <summary>
    /// Decrement Moment of Clarity duration at end of turn
    /// </summary>
    public bool DecrementClarityDuration(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_SpiritBargains
                SET clarity_turns_remaining = clarity_turns_remaining - 1,
                    in_moment_of_clarity = CASE WHEN clarity_turns_remaining - 1 <= 0 THEN 0 ELSE 1 END,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId AND in_moment_of_clarity = 1
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                _log.Debug("Clarity duration decremented: CharacterId={CharacterId}", characterId);
            }

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to decrement Clarity duration: CharacterId={CharacterId}", characterId);
            return false;
        }
    }

    /// <summary>
    /// End Moment of Clarity (used when clarity expires or is manually ended)
    /// </summary>
    public bool EndMomentOfClarity(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_SpiritBargains
                SET in_moment_of_clarity = 0,
                    clarity_turns_remaining = 0,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.ExecuteNonQuery();

            _log.Information("Moment of Clarity ended: CharacterId={CharacterId}", characterId);
            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to end Moment of Clarity: CharacterId={CharacterId}", characterId);
            return false;
        }
    }

    /// <summary>
    /// Update Fickle Fortune rank when ability is learned or ranked up
    /// </summary>
    public void UpdateFickleFortuneRank(int characterId, int rank)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_SpiritBargains
                SET fickle_fortune_rank = @Rank,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.Parameters.AddWithValue("@Rank", rank);
            command.ExecuteNonQuery();

            _log.Information("Fickle Fortune rank updated: CharacterId={CharacterId}, Rank={Rank}", characterId, rank);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update Fickle Fortune rank: CharacterId={CharacterId}", characterId);
        }
    }

    /// <summary>
    /// Reset combat-specific flags (forced bargain, clarity uses) when combat starts
    /// </summary>
    public void ResetCombatState(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_SpiritBargains
                SET forced_bargain_used_this_combat = 0,
                    clarity_uses_this_combat = 0,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.ExecuteNonQuery();

            _log.Debug("Combat state reset for Spirit Bargains: CharacterId={CharacterId}", characterId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to reset combat state: CharacterId={CharacterId}", characterId);
        }
    }

    /// <summary>
    /// Get Spirit Bargain statistics for a character
    /// </summary>
    public (int attempted, int triggered, float successRate) GetBargainStatistics(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT total_bargains_attempted, total_bargains_triggered, bargain_success_rate
                FROM Characters_SpiritBargains
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return (
                    reader.GetInt32(0),  // attempted
                    reader.GetInt32(1),  // triggered
                    reader.GetFloat(2)   // success rate
                );
            }

            return (0, 0, 0.0f);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get bargain statistics: CharacterId={CharacterId}", characterId);
            return (0, 0, 0.0f);
        }
    }

    #region Private Helper Methods

    private bool GetMomentOfClarityStatus(SqliteConnection connection, int characterId)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT in_moment_of_clarity
            FROM Characters_SpiritBargains
            WHERE character_id = @CharacterId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);

        var result = command.ExecuteScalar();
        return result != null && Convert.ToInt32(result) == 1;
    }

    private int GetFickleFortuneRank(SqliteConnection connection, int characterId)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT fickle_fortune_rank
            FROM Characters_SpiritBargains
            WHERE character_id = @CharacterId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);

        var result = command.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    private bool GetPsychicResonanceStatus(SqliteConnection connection, int characterId)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT psychic_resonance_bonus_active
            FROM Characters_SpiritBargains
            WHERE character_id = @CharacterId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);

        var result = command.ExecuteScalar();
        return result != null && Convert.ToInt32(result) == 1;
    }

    private bool IsForcedBargainUsed(SqliteConnection connection, int characterId)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT forced_bargain_used_this_combat
            FROM Characters_SpiritBargains
            WHERE character_id = @CharacterId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);

        var result = command.ExecuteScalar();
        return result != null && Convert.ToInt32(result) == 1;
    }

    private void MarkForcedBargainUsed(SqliteConnection connection, int characterId)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters_SpiritBargains
            SET forced_bargain_used_this_combat = 1,
                updated_at = datetime('now')
            WHERE character_id = @CharacterId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.ExecuteNonQuery();
    }

    private int GetClarityUsesThisCombat(SqliteConnection connection, int characterId)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT clarity_uses_this_combat
            FROM Characters_SpiritBargains
            WHERE character_id = @CharacterId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);

        var result = command.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    private void RecordBargainSuccess(SqliteConnection connection, int characterId)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters_SpiritBargains
            SET total_bargains_triggered = total_bargains_triggered + 1,
                total_bargains_attempted = total_bargains_attempted + 1,
                bargain_success_rate = CAST(total_bargains_triggered + 1 AS REAL) / CAST(total_bargains_attempted + 1 AS REAL),
                updated_at = datetime('now')
            WHERE character_id = @CharacterId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.ExecuteNonQuery();
    }

    private void RecordBargainFailure(SqliteConnection connection, int characterId)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters_SpiritBargains
            SET total_bargains_attempted = total_bargains_attempted + 1,
                bargain_success_rate = CAST(total_bargains_triggered AS REAL) / CAST(total_bargains_attempted + 1 AS REAL),
                updated_at = datetime('now')
            WHERE character_id = @CharacterId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.ExecuteNonQuery();
    }

    #endregion
}
