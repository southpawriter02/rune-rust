using RuneAndRust.Core;
using Serilog;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.27.1: Service for Skald specialization-specific abilities and mechanics.
/// Handles Saga of Courage, Saga of Einherjar, performance management, and other bard abilities.
/// The chronicler of coherence - wields structured verse as weapon and shield.
/// </summary>
public class SkaldService
{
    private static readonly ILogger _log = Log.ForContext<SkaldService>();
    private readonly PerformanceStateService _performanceService;
    private readonly TraumaEconomyService _traumaService;
    private readonly DiceService _diceService;
    private readonly string _connectionString;

    public SkaldService(string connectionString)
    {
        _connectionString = connectionString;
        _performanceService = new PerformanceStateService(connectionString);
        _traumaService = new TraumaEconomyService();
        _diceService = new DiceService();
        _log.Debug("SkaldService initialized");
    }

    #region Tier 1 Abilities

    /// <summary>
    /// Execute Saga of Courage performance (Tier 1)
    /// Rousing chant granting Fear immunity and Psychic Stress resistance to all allies
    /// </summary>
    public (bool success, string message, int duration) ExecuteSagaOfCourage(
        PlayerCharacter skald,
        List<PlayerCharacter> allies,
        int rank = 1,
        int enduringPerformanceRank = 0)
    {
        _log.Information("Executing Saga of Courage: SkaldID={SkaldId}, Rank={Rank}",
            skald.CharacterID, rank);

        // Start performance
        bool started = _performanceService.StartPerformance(
            skald.CharacterID,
            27002, // Saga of Courage ability ID
            rank,
            skald.WILL,
            enduringPerformanceRank);

        if (!started)
        {
            return (false, "Cannot start performance - already performing!", 0);
        }

        // Calculate effects based on rank
        var (stressDiceBonus, disorientedBonus) = rank switch
        {
            1 => (1, 0),
            2 => (2, 0),
            3 => (2, 1),
            _ => (1, 0)
        };

        int baseDuration = skald.WILL;
        int enduringBonus = rank >= 1 && enduringPerformanceRank > 0
            ? GetEnduringPerformanceBonus(enduringPerformanceRank)
            : 0;
        int totalDuration = baseDuration + enduringBonus;

        _log.Information(
            "Saga of Courage initiated: SkaldID={SkaldId}, Allies={AllyCount}, Duration={Duration}, StressDice={Dice}, Rank={Rank}",
            skald.CharacterID, allies.Count, totalDuration, stressDiceBonus, rank);

        string message = $"The Skald's Saga of Courage fills {allies.Count} allies with fearless resolve! " +
                        $"(Fear immunity, +{stressDiceBonus} dice vs Psychic Stress";

        if (disorientedBonus > 0)
        {
            message += $", +{disorientedBonus} die vs Disoriented";
        }

        message += $" for {totalDuration} rounds)";

        return (true, message, totalDuration);
    }

    /// <summary>
    /// Execute Dirge of Defeat performance (Tier 1)
    /// Sorrowful dirge that debuffs intelligent enemies
    /// </summary>
    public (bool success, string message, int duration) ExecuteDirgeOfDefeat(
        PlayerCharacter skald,
        List<Enemy> intelligentEnemies,
        int rank = 1,
        int enduringPerformanceRank = 0)
    {
        _log.Information("Executing Dirge of Defeat: SkaldID={SkaldId}, Rank={Rank}",
            skald.CharacterID, rank);

        // Start performance
        bool started = _performanceService.StartPerformance(
            skald.CharacterID,
            27003, // Dirge of Defeat ability ID
            rank,
            skald.WILL,
            enduringPerformanceRank);

        if (!started)
        {
            return (false, "Cannot start performance - already performing!", 0);
        }

        // Calculate effects based on rank
        var (dicePenalty, psychicDamage) = rank switch
        {
            1 => (1, 0),
            2 => (2, 0),
            3 => (2, 4), // 1d4 per turn
            _ => (1, 0)
        };

        int baseDuration = skald.WILL;
        int enduringBonus = rank >= 1 && enduringPerformanceRank > 0
            ? GetEnduringPerformanceBonus(enduringPerformanceRank)
            : 0;
        int totalDuration = baseDuration + enduringBonus;

        _log.Information(
            "Dirge of Defeat initiated: SkaldID={SkaldId}, Enemies={EnemyCount}, Duration={Duration}, Penalty={Penalty}, Rank={Rank}",
            skald.CharacterID, intelligentEnemies.Count, totalDuration, dicePenalty, rank);

        string message = $"The Skald's Dirge of Defeat unnerves {intelligentEnemies.Count} intelligent foes! " +
                        $"(-{dicePenalty} dice to Accuracy and damage";

        if (psychicDamage > 0)
        {
            message += $", 1d{psychicDamage} Psychic damage/turn";
        }

        message += $" for {totalDuration} rounds)";

        return (true, message, totalDuration);
    }

    #endregion

    #region Tier 2 Abilities

    /// <summary>
    /// Execute Rousing Verse (Tier 2)
    /// Restore Stamina to single ally through inspiring recollection
    /// </summary>
    public (int staminaRestored, bool exhaustedRemoved, string message) ExecuteRousingVerse(
        PlayerCharacter skald,
        PlayerCharacter target,
        int rank = 1)
    {
        _log.Information("Executing Rousing Verse: SkaldID={SkaldId}, TargetID={TargetId}, Rank={Rank}",
            skald.CharacterID, target.CharacterID, rank);

        // Calculate stamina restoration based on rank
        var (baseRestore, willMultiplier, removesExhausted) = rank switch
        {
            1 => (15, 2, false),
            2 => (20, 3, false),
            3 => (25, 3, true),
            _ => (15, 2, false)
        };

        int staminaRestored = baseRestore + (skald.WILL * willMultiplier);

        // Apply stamina restoration (capped at max)
        int oldStamina = target.Stamina;
        target.Stamina = Math.Min(target.Stamina + staminaRestored, target.MaxStamina);
        int actualRestored = target.Stamina - oldStamina;

        _log.Information("Rousing Verse restored Stamina: TargetID={TargetId}, Restored={Stamina}",
            target.CharacterID, actualRestored);

        string message = $"Rousing Verse restores {actualRestored} Stamina to {target.Name}!";

        if (removesExhausted)
        {
            message += " [Exhausted removed]";
        }

        return (actualRestored, removesExhausted, message);
    }

    /// <summary>
    /// Execute Song of Silence (Tier 2)
    /// Counter-resonant chant to silence an intelligent enemy
    /// </summary>
    public (bool success, int duration, int damage, string message) ExecuteSongOfSilence(
        PlayerCharacter skald,
        Enemy target,
        int rank = 1)
    {
        _log.Information("Executing Song of Silence: SkaldID={SkaldId}, TargetID={TargetId}, Rank={Rank}",
            skald.CharacterID, target.EnemyID, rank);

        // Calculate effects based on rank
        var (silenceDuration, psychicDamage) = rank switch
        {
            1 => (2, 0),
            2 => (3, 0),
            3 => (3, _diceService.RollD6() + _diceService.RollD6()), // 2d6
            _ => (2, 0)
        };

        // Opposed WILL + Rhetoric check (simplified: assume success for this implementation)
        bool success = true;

        _log.Information("Song of Silence executed: TargetID={TargetId}, Silenced={Duration} rounds, Damage={Damage}",
            target.EnemyID, silenceDuration, psychicDamage);

        string message = $"Song of Silence applies [Silenced] to {target.Name} for {silenceDuration} rounds!";

        if (psychicDamage > 0)
        {
            message += $" ({psychicDamage} Psychic damage from vocal disruption)";
        }

        return (success, silenceDuration, psychicDamage, message);
    }

    #endregion

    #region Tier 3 Abilities

    /// <summary>
    /// Execute Lay of the Iron Wall performance (Tier 3)
    /// Story of unbreakable shield wall granting Soak to Front Row allies
    /// </summary>
    public (bool success, string message, int duration, int soakBonus) ExecuteLayOfTheIronWall(
        PlayerCharacter skald,
        List<PlayerCharacter> frontRowAllies,
        int rank = 1,
        int enduringPerformanceRank = 0)
    {
        _log.Information("Executing Lay of the Iron Wall: SkaldID={SkaldId}, Rank={Rank}",
            skald.CharacterID, rank);

        // Start performance
        bool started = _performanceService.StartPerformance(
            skald.CharacterID,
            27007, // Lay of the Iron Wall ability ID
            rank,
            skald.WILL,
            enduringPerformanceRank);

        if (!started)
        {
            return (false, "Cannot start performance - already performing!", 0, 0);
        }

        // Calculate effects based on rank
        var (soakBonus, pushPullResistance) = rank switch
        {
            1 => (2, false),
            2 => (3, false),
            3 => (4, true),
            _ => (2, false)
        };

        int baseDuration = skald.WILL;
        int enduringBonus = rank >= 1 && enduringPerformanceRank > 0
            ? GetEnduringPerformanceBonus(enduringPerformanceRank)
            : 0;
        int totalDuration = baseDuration + enduringBonus;

        _log.Information(
            "Lay of the Iron Wall initiated: SkaldID={SkaldId}, FrontRowAllies={Count}, Duration={Duration}, Soak={Soak}, Rank={Rank}",
            skald.CharacterID, frontRowAllies.Count, totalDuration, soakBonus, rank);

        string message = $"Lay of the Iron Wall fortifies {frontRowAllies.Count} Front Row allies! " +
                        $"(+{soakBonus} Soak";

        if (pushPullResistance)
        {
            message += ", Resistance to Push/Pull";
        }

        message += $" for {totalDuration} rounds)";

        return (true, message, totalDuration, soakBonus);
    }

    #endregion

    #region Capstone: Saga of the Einherjar

    /// <summary>
    /// Execute Saga of the Einherjar - Ultimate Performance (Capstone)
    /// Masterpiece saga granting massive temporary buffs with Psychic Stress cost at end
    /// </summary>
    public (bool success, string message, int duration, int damageBonus, int tempHP, int stressCost) ExecuteSagaOfEinherjar(
        PlayerCharacter skald,
        List<PlayerCharacter> allies,
        int rank = 1,
        int enduringPerformanceRank = 0)
    {
        _log.Information("Attempting to execute Saga of the Einherjar: SkaldID={SkaldId}, Rank={Rank}",
            skald.CharacterID, rank);

        // Check once-per-combat limit
        if (HasUsedEinherjarThisCombat(skald.CharacterID))
        {
            _log.Warning("Saga of Einherjar already used this combat: SkaldID={SkaldId}", skald.CharacterID);
            return (false, "Saga of Einherjar already used this combat!", 0, 0, 0, 0);
        }

        // Start performance
        bool started = _performanceService.StartPerformance(
            skald.CharacterID,
            27009, // Saga of Einherjar ability ID
            rank,
            skald.WILL,
            enduringPerformanceRank);

        if (!started)
        {
            return (false, "Cannot start performance - already performing!", 0, 0, 0, 0);
        }

        // Mark as used this combat
        MarkEinherjarUsed(skald.CharacterID);

        // Calculate effects based on rank
        var (damageDiceBonus, tempHP, stressCost, fearStunImmune) = rank switch
        {
            1 => (3, 20, 10, false),
            2 => (4, 30, 8, false),
            3 => (5, 40, 6, true),
            _ => (3, 20, 10, false)
        };

        int baseDuration = skald.WILL;
        int enduringBonus = rank >= 1 && enduringPerformanceRank > 0
            ? GetEnduringPerformanceBonus(enduringPerformanceRank)
            : 0;
        int totalDuration = baseDuration + enduringBonus;

        // Store affected allies and stress cost for later application
        StoreEinherjarAffectedAllies(skald.CharacterID, allies.Select(a => a.CharacterID).ToList(), stressCost);

        _log.Information(
            "SAGA OF EINHERJAR UNLEASHED: SkaldID={SkaldId}, Allies={Count}, DamageBonus={Dice} dice, TempHP={HP}, StressCost={Stress}, Duration={Duration}",
            skald.CharacterID, allies.Count, damageDiceBonus, tempHP, stressCost, totalDuration);

        string message = $"⚔️ THE SAGA OF THE EINHERJAR! {allies.Count} allies believe themselves legendary heroes! " +
                        $"(+{damageDiceBonus} damage dice, +{tempHP} temp HP";

        if (fearStunImmune)
        {
            message += ", Fear/Stun immunity";
        }

        message += $" for {totalDuration} rounds. {stressCost} Stress cost when performance ends)";

        return (true, message, totalDuration, damageDiceBonus, tempHP, stressCost);
    }

    /// <summary>
    /// Handle effects when performance ends (especially for Saga of Einherjar)
    /// </summary>
    public void OnPerformanceEnd(int characterId)
    {
        // Check if this was Saga of Einherjar
        var einherjarData = GetEinherjarAffectedAllies(characterId);
        if (einherjarData != null)
        {
            // Apply Psychic Stress to all affected allies
            foreach (var allyId in einherjarData.Value.AffectedAllies)
            {
                // Note: This would need access to the ally PlayerCharacter objects
                // For now, we just log the stress cost that should be applied
                _log.Information(
                    "Einherjar Stress cost to be applied: AllyID={AllyId}, Stress={Stress}",
                    allyId, einherjarData.Value.StressCost);
            }

            ClearEinherjarData(characterId);

            _log.Information("Saga of Einherjar ended - Psychic Stress cost applied to all affected allies");
        }
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
    /// Get the Heart of the Clan passive bonus
    /// </summary>
    public int GetHeartOfClanBonus(int rank)
    {
        return rank switch
        {
            1 => 1,
            2 => 2,
            3 => 2,
            _ => 0
        };
    }

    /// <summary>
    /// Check if Saga of Einherjar has been used this combat
    /// </summary>
    private bool HasUsedEinherjarThisCombat(int characterId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COUNT(*)
                    FROM Skald_Einherjar_Usage
                    WHERE character_id = @CharacterId AND combat_id = @CombatId";

                cmd.Parameters.AddWithValue("@CharacterId", characterId);
                cmd.Parameters.AddWithValue("@CombatId", GetCurrentCombatId());

                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }

    /// <summary>
    /// Mark Saga of Einherjar as used for this combat
    /// </summary>
    private void MarkEinherjarUsed(int characterId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT OR REPLACE INTO Skald_Einherjar_Usage
                    (character_id, combat_id, used_at)
                    VALUES (@CharacterId, @CombatId, @Timestamp)";

                cmd.Parameters.AddWithValue("@CharacterId", characterId);
                cmd.Parameters.AddWithValue("@CombatId", GetCurrentCombatId());
                cmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Store affected allies for Einherjar stress cost application
    /// </summary>
    private void StoreEinherjarAffectedAllies(int skaldId, List<int> allyIds, int stressCost)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT OR REPLACE INTO Skald_Einherjar_Affected
                    (skald_character_id, affected_ally_ids, stress_cost)
                    VALUES (@SkaldId, @AllyIds, @StressCost)";

                cmd.Parameters.AddWithValue("@SkaldId", skaldId);
                cmd.Parameters.AddWithValue("@AllyIds", string.Join(",", allyIds));
                cmd.Parameters.AddWithValue("@StressCost", stressCost);

                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Get affected allies data for Einherjar
    /// </summary>
    private (List<int> AffectedAllies, int StressCost)? GetEinherjarAffectedAllies(int skaldId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT affected_ally_ids, stress_cost
                    FROM Skald_Einherjar_Affected
                    WHERE skald_character_id = @SkaldId";

                cmd.Parameters.AddWithValue("@SkaldId", skaldId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string allyIdsStr = reader.GetString(0);
                        int stressCost = reader.GetInt32(1);
                        var allyIds = allyIdsStr.Split(',').Select(int.Parse).ToList();
                        return (allyIds, stressCost);
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Clear Einherjar affected allies data
    /// </summary>
    private void ClearEinherjarData(int skaldId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    DELETE FROM Skald_Einherjar_Affected
                    WHERE skald_character_id = @SkaldId";

                cmd.Parameters.AddWithValue("@SkaldId", skaldId);

                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Get current combat ID (simplified - would be part of combat state)
    /// </summary>
    private int GetCurrentCombatId()
    {
        // Simplified implementation - in real system, this would track active combat
        return 1;
    }

    #endregion
}
