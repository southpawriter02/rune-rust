using Microsoft.Data.Sqlite;
using Serilog;
using System;
using System.Threading.Tasks;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a Fury operation
/// </summary>
public class FuryResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CurrentFury { get; set; }
    public int FuryChange { get; set; }
}

/// <summary>
/// v0.26.1: Service for managing the Berserkr's Fury resource system.
/// Fury is gained by dealing damage and taking damage, and spent on powerful abilities.
/// Fury decays to 0 upon leaving combat or taking a Sanctuary Rest.
/// </summary>
public class FuryService
{
    private static readonly ILogger _log = Log.ForContext<FuryService>();
    private readonly string _connectionString;

    public const int MAX_FURY = 100;
    public const int FURY_PER_HP_DAMAGE_TAKEN = 1; // Base value, doubled with Blood-Fueled
    public const int FURY_PER_DAMAGE_DEALT_BASE = 1; // Per 10 damage dealt

    public FuryService(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("FuryService initialized");
    }

    /// <summary>
    /// Initialize Fury tracking for a character (creates entry if doesn't exist)
    /// </summary>
    public FuryResult InitializeFury(int characterId)
    {
        _log.Debug("Initializing Fury for character {CharacterId}", characterId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR IGNORE INTO Characters_Fury (character_id, current_fury, max_fury, in_combat, created_at, updated_at)
                VALUES (@CharacterId, 0, 100, 0, datetime('now'), datetime('now'))
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.ExecuteNonQuery();

            _log.Information("Fury initialized for character {CharacterId}", characterId);

            return new FuryResult
            {
                Success = true,
                Message = "Fury tracking initialized",
                CurrentFury = 0
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize Fury for character {CharacterId}", characterId);
            return new FuryResult
            {
                Success = false,
                Message = $"Error initializing Fury: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Generates Fury when the Berserkr takes damage.
    /// </summary>
    /// <param name="characterId">Character ID</param>
    /// <param name="damageAmount">Amount of HP damage taken</param>
    /// <param name="hasBloodFueled">Whether Blood-Fueled passive is active (doubles Fury gain)</param>
    /// <param name="bloodFueledRank">Rank of Blood-Fueled (1=2x, 2=2.5x, 3=3x)</param>
    public FuryResult GenerateFuryFromDamageTaken(int characterId, int damageAmount, bool hasBloodFueled = false, int bloodFueledRank = 1)
    {
        _log.Debug("Generating Fury from damage taken: CharacterId={CharacterId}, Damage={Damage}", characterId, damageAmount);

        try
        {
            // Calculate base Fury gain
            int furyGain = damageAmount * FURY_PER_HP_DAMAGE_TAKEN;

            // Apply Blood-Fueled multiplier if active
            if (hasBloodFueled)
            {
                float multiplier = bloodFueledRank switch
                {
                    1 => 2.0f,
                    2 => 2.5f,
                    3 => 3.0f,
                    _ => 2.0f
                };
                furyGain = (int)(furyGain * multiplier);
                _log.Information("Blood-Fueled Rank {Rank} active: Fury generation multiplied by {Multiplier}x for CharacterId={CharacterId}",
                    bloodFueledRank, multiplier, characterId);
            }

            // Get current Fury and calculate new value
            var currentFury = GetCurrentFury(characterId);
            int newFury = Math.Min(currentFury + furyGain, MAX_FURY);
            int actualGain = newFury - currentFury;

            // Update database
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_Fury
                SET current_fury = @NewFury,
                    last_fury_gain_timestamp = datetime('now'),
                    total_fury_generated = total_fury_generated + @ActualGain,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.Parameters.AddWithValue("@NewFury", newFury);
            command.Parameters.AddWithValue("@ActualGain", actualGain);
            command.ExecuteNonQuery();

            _log.Information("Fury generated from damage: CharacterId={CharacterId}, DamageTaken={Damage}, FuryGain={FuryGain}, NewFury={NewFury}, BloodFueled={BloodFueled}",
                characterId, damageAmount, actualGain, newFury, hasBloodFueled);

            return new FuryResult
            {
                Success = true,
                Message = $"+{actualGain} Fury from taking {damageAmount} damage",
                CurrentFury = newFury,
                FuryChange = actualGain
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to generate Fury from damage: CharacterId={CharacterId}, Damage={Damage}",
                characterId, damageAmount);
            return new FuryResult
            {
                Success = false,
                Message = $"Error generating Fury: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Generates Fury when the Berserkr deals damage.
    /// </summary>
    /// <param name="characterId">Character ID</param>
    /// <param name="damageDealt">Amount of damage dealt</param>
    /// <param name="abilityFuryBonus">Specific ability Fury bonus (e.g., Wild Swing +5 per hit)</param>
    /// <param name="isBloodied">Whether character is Bloodied (below 50% HP) for Death or Glory</param>
    /// <param name="deathOrGloryRank">Rank of Death or Glory (1=+50%, 2=+75%, 3=+100%)</param>
    public FuryResult GenerateFuryFromDamageDealt(
        int characterId,
        int damageDealt,
        int abilityFuryBonus = 0,
        bool isBloodied = false,
        int deathOrGloryRank = 0)
    {
        _log.Debug("Generating Fury from damage dealt: CharacterId={CharacterId}, Damage={Damage}, AbilityBonus={Bonus}",
            characterId, damageDealt, abilityFuryBonus);

        try
        {
            // Calculate base Fury gain from damage dealt
            int baseFuryGain = (damageDealt / 10) * FURY_PER_DAMAGE_DEALT_BASE;

            // Add ability-specific bonus
            int totalFuryGain = baseFuryGain + abilityFuryBonus;

            // Apply Death or Glory bonus if Bloodied
            if (isBloodied && deathOrGloryRank > 0)
            {
                float bonusMultiplier = deathOrGloryRank switch
                {
                    1 => 1.5f,  // +50%
                    2 => 1.75f, // +75%
                    3 => 2.0f,  // +100%
                    _ => 1.0f
                };
                totalFuryGain = (int)(totalFuryGain * bonusMultiplier);
                _log.Information("Death or Glory Rank {Rank} active: Fury generation increased by {Bonus}% for CharacterId={CharacterId}",
                    deathOrGloryRank, (bonusMultiplier - 1) * 100, characterId);
            }

            // Get current Fury and calculate new value
            var currentFury = GetCurrentFury(characterId);
            int newFury = Math.Min(currentFury + totalFuryGain, MAX_FURY);
            int actualGain = newFury - currentFury;

            // Update database
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_Fury
                SET current_fury = @NewFury,
                    last_fury_gain_timestamp = datetime('now'),
                    total_fury_generated = total_fury_generated + @ActualGain,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.Parameters.AddWithValue("@NewFury", newFury);
            command.Parameters.AddWithValue("@ActualGain", actualGain);
            command.ExecuteNonQuery();

            _log.Information("Fury generated from damage dealt: CharacterId={CharacterId}, DamageDealt={Damage}, AbilityBonus={Bonus}, FuryGain={FuryGain}, NewFury={NewFury}, Bloodied={Bloodied}",
                characterId, damageDealt, abilityFuryBonus, actualGain, newFury, isBloodied);

            return new FuryResult
            {
                Success = true,
                Message = $"+{actualGain} Fury",
                CurrentFury = newFury,
                FuryChange = actualGain
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to generate Fury from damage dealt: CharacterId={CharacterId}, Damage={Damage}",
                characterId, damageDealt);
            return new FuryResult
            {
                Success = false,
                Message = $"Error generating Fury: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Spends Fury to activate an ability.
    /// </summary>
    public FuryResult SpendFury(int characterId, int furyCost)
    {
        _log.Debug("Spending Fury: CharacterId={CharacterId}, Cost={Cost}", characterId, furyCost);

        try
        {
            var currentFury = GetCurrentFury(characterId);

            if (currentFury < furyCost)
            {
                _log.Warning("Insufficient Fury: CharacterId={CharacterId}, Required={Required}, Current={Current}",
                    characterId, furyCost, currentFury);
                return new FuryResult
                {
                    Success = false,
                    Message = $"Insufficient Fury (need {furyCost}, have {currentFury})",
                    CurrentFury = currentFury
                };
            }

            int newFury = currentFury - furyCost;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_Fury
                SET current_fury = @NewFury,
                    total_fury_spent = total_fury_spent + @FuryCost,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.Parameters.AddWithValue("@NewFury", newFury);
            command.Parameters.AddWithValue("@FuryCost", furyCost);
            command.ExecuteNonQuery();

            _log.Information("Fury spent: CharacterId={CharacterId}, FurySpent={Cost}, RemainingFury={Remaining}",
                characterId, furyCost, newFury);

            return new FuryResult
            {
                Success = true,
                Message = $"Spent {furyCost} Fury",
                CurrentFury = newFury,
                FuryChange = -furyCost
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to spend Fury: CharacterId={CharacterId}, Cost={Cost}",
                characterId, furyCost);
            return new FuryResult
            {
                Success = false,
                Message = $"Error spending Fury: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Gets current Fury for a character.
    /// </summary>
    public int GetCurrentFury(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT current_fury FROM Characters_Fury WHERE character_id = @CharacterId";
            command.Parameters.AddWithValue("@CharacterId", characterId);

            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get current Fury: CharacterId={CharacterId}", characterId);
            return 0;
        }
    }

    /// <summary>
    /// Resets Fury to 0 when leaving combat or resting.
    /// </summary>
    public FuryResult ResetFury(int characterId, string reason)
    {
        _log.Debug("Resetting Fury: CharacterId={CharacterId}, Reason={Reason}", characterId, reason);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_Fury
                SET current_fury = 0,
                    in_combat = 0,
                    unstoppable_fury_triggered = 0,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.ExecuteNonQuery();

            _log.Information("Fury reset: CharacterId={CharacterId}, Reason={Reason}", characterId, reason);

            return new FuryResult
            {
                Success = true,
                Message = $"Fury reset: {reason}",
                CurrentFury = 0,
                FuryChange = -GetCurrentFury(characterId)
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to reset Fury: CharacterId={CharacterId}, Reason={Reason}",
                characterId, reason);
            return new FuryResult
            {
                Success = false,
                Message = $"Error resetting Fury: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Triggers Unstoppable Fury capstone once per combat.
    /// </summary>
    public FuryResult TriggerUnstoppableFury(int characterId)
    {
        _log.Debug("Triggering Unstoppable Fury: CharacterId={CharacterId}", characterId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Check if already triggered this combat
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT unstoppable_fury_triggered FROM Characters_Fury WHERE character_id = @CharacterId";
            checkCommand.Parameters.AddWithValue("@CharacterId", characterId);
            var alreadyTriggered = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

            if (alreadyTriggered)
            {
                _log.Warning("Unstoppable Fury already triggered this combat: CharacterId={CharacterId}", characterId);
                return new FuryResult
                {
                    Success = false,
                    Message = "Unstoppable Fury already used this combat"
                };
            }

            // Set HP to 1, grant 100 Fury, mark as triggered
            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = @"
                UPDATE Characters_Fury
                SET current_fury = 100,
                    unstoppable_fury_triggered = 1,
                    total_fury_generated = total_fury_generated + 100,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            updateCommand.Parameters.AddWithValue("@CharacterId", characterId);
            updateCommand.ExecuteNonQuery();

            _log.Information("Unstoppable Fury triggered: CharacterId={CharacterId}, HP set to 1, Fury set to 100",
                characterId);

            return new FuryResult
            {
                Success = true,
                Message = "UNSTOPPABLE FURY! You refuse to die! (+100 Fury)",
                CurrentFury = 100,
                FuryChange = 100
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to trigger Unstoppable Fury: CharacterId={CharacterId}", characterId);
            return new FuryResult
            {
                Success = false,
                Message = $"Error triggering Unstoppable Fury: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Calculates Stamina regeneration bonus from Primal Vigor passive.
    /// </summary>
    /// <param name="currentFury">Current Fury amount</param>
    /// <param name="rank">Rank of Primal Vigor (1=+2 per 25, 2=+3 per 25, 3=+4 per 25)</param>
    public int CalculatePrimalVigorBonus(int currentFury, int rank = 1)
    {
        // +2/3/4 Stamina regen per 25 Fury depending on rank
        int bonusPerBreakpoint = rank switch
        {
            1 => 2,
            2 => 3,
            3 => 4,
            _ => 2
        };

        int breakpoints = currentFury / 25;
        int bonus = breakpoints * bonusPerBreakpoint;

        return bonus;
    }

    /// <summary>
    /// Sets combat state for Fury tracking
    /// </summary>
    public void SetCombatState(int characterId, bool inCombat)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Characters_Fury
                SET in_combat = @InCombat,
                    updated_at = datetime('now')
                WHERE character_id = @CharacterId
            ";
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.Parameters.AddWithValue("@InCombat", inCombat ? 1 : 0);
            command.ExecuteNonQuery();

            _log.Information("Combat state updated: CharacterId={CharacterId}, InCombat={InCombat}",
                characterId, inCombat);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to set combat state: CharacterId={CharacterId}", characterId);
        }
    }
}
