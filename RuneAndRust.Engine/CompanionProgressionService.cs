using RuneAndRust.Core;
using Microsoft.Data.Sqlite;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.34.3: Companion Progression Service
/// Handles leveling, stat scaling, equipment management, and ability unlocks
/// </summary>
public class CompanionProgressionService
{
    private static readonly ILogger _log = Log.ForContext<CompanionProgressionService>();
    private readonly string _connectionString;

    // Ability unlock levels
    private static readonly int[] ABILITY_UNLOCK_LEVELS = { 3, 5, 7 };

    // Legend scaling (matches player progression)
    private const int BASE_LEGEND_TO_NEXT_LEVEL = 100;
    private const float LEGEND_SCALING_FACTOR = 1.1f;

    public CompanionProgressionService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Awards Legend (XP) to a companion and triggers level ups if thresholds met
    /// </summary>
    public void AwardLegend(int characterId, int companionId, int legendAmount)
    {
        if (legendAmount <= 0)
        {
            _log.Debug("Legend amount is 0 or negative, skipping award");
            return;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var progression = GetCompanionProgression(characterId, companionId, connection);
        if (progression == null)
        {
            _log.Warning("Companion progression not found: CharacterId={CharacterId}, CompanionId={CompanionId}",
                characterId, companionId);
            return;
        }

        var oldLevel = progression.CurrentLevel;
        var oldLegend = progression.CurrentLegend;
        var newLegend = oldLegend + legendAmount;

        _log.Information("Awarding Legend: CharacterId={CharacterId}, CompanionId={CompanionId}, Amount={Amount}, OldLegend={OldLegend}, NewLegend={NewLegend}",
            characterId, companionId, legendAmount, oldLegend, newLegend);

        // Check for level ups
        var levelsGained = 0;
        var currentLevel = oldLevel;
        var remainingLegend = newLegend;
        var legendToNext = progression.LegendToNextLevel;

        while (remainingLegend >= legendToNext)
        {
            remainingLegend -= legendToNext;
            currentLevel++;
            levelsGained++;

            // Calculate next level's legend requirement
            legendToNext = CalculateLegendToNextLevel(currentLevel);

            _log.Information("Level up! CharacterId={CharacterId}, CompanionId={CompanionId}, NewLevel={NewLevel}",
                characterId, companionId, currentLevel);

            // Check for ability unlocks
            if (ABILITY_UNLOCK_LEVELS.Contains(currentLevel))
            {
                UnlockAbilityAtLevel(characterId, companionId, currentLevel, connection);
            }
        }

        // Update progression in database
        var updateCommand = connection.CreateCommand();
        updateCommand.CommandText = @"
            UPDATE Companion_Progression
            SET current_level = @level,
                current_legend = @legend,
                legend_to_next_level = @legendToNext,
                updated_at = @updatedAt
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        updateCommand.Parameters.AddWithValue("@level", currentLevel);
        updateCommand.Parameters.AddWithValue("@legend", remainingLegend);
        updateCommand.Parameters.AddWithValue("@legendToNext", legendToNext);
        updateCommand.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
        updateCommand.Parameters.AddWithValue("@charId", characterId);
        updateCommand.Parameters.AddWithValue("@companionId", companionId);
        updateCommand.ExecuteNonQuery();

        if (levelsGained > 0)
        {
            _log.Information("Companion gained {Levels} level(s): CharacterId={CharacterId}, CompanionId={CompanionId}, OldLevel={OldLevel}, NewLevel={NewLevel}",
                levelsGained, characterId, companionId, oldLevel, currentLevel);
        }
    }

    /// <summary>
    /// Calculates scaled stats for a companion based on their current level
    /// Scaling formulas:
    /// - HP/Stamina/Aether: Base × (1 + 0.1 × (Level-1))
    /// - Attributes (MIGHT/FINESSE/etc): Base + 2 × (Level-1)
    /// </summary>
    public CompanionScaledStats CalculateScaledStats(int characterId, int companionId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Get base stats from Companions table
        var baseCommand = connection.CreateCommand();
        baseCommand.CommandText = @"
            SELECT base_might, base_finesse, base_sturdiness, base_wits, base_will,
                   base_max_hp, base_defense, base_soak, base_max_resource
            FROM Companions
            WHERE companion_id = @companionId
        ";
        baseCommand.Parameters.AddWithValue("@companionId", companionId);

        var baseStats = new CompanionScaledStats();

        using (var reader = baseCommand.ExecuteReader())
        {
            if (!reader.Read())
            {
                _log.Warning("Companion not found: CompanionId={CompanionId}", companionId);
                return baseStats;
            }

            baseStats.BaseMight = reader.GetInt32(0);
            baseStats.BaseFinesse = reader.GetInt32(1);
            baseStats.BaseSturdiness = reader.GetInt32(2);
            baseStats.BaseWits = reader.GetInt32(3);
            baseStats.BaseWill = reader.GetInt32(4);
            baseStats.BaseMaxHP = reader.GetInt32(5);
            baseStats.BaseDefense = reader.GetInt32(6);
            baseStats.BaseSoak = reader.GetInt32(7);
            baseStats.BaseMaxResource = reader.GetInt32(8);
        }

        // Get current level from Companion_Progression
        var progression = GetCompanionProgression(characterId, companionId, connection);
        if (progression == null)
        {
            _log.Warning("Companion progression not found: CharacterId={CharacterId}, CompanionId={CompanionId}",
                characterId, companionId);
            return baseStats;
        }

        var level = progression.CurrentLevel;

        // Apply scaling formulas
        var levelBonus = level - 1;

        // Attributes: Base + 2 × (Level-1)
        baseStats.Might = baseStats.BaseMight + (2 * levelBonus);
        baseStats.Finesse = baseStats.BaseFinesse + (2 * levelBonus);
        baseStats.Sturdiness = baseStats.BaseSturdiness + (2 * levelBonus);
        baseStats.Wits = baseStats.BaseWits + (2 * levelBonus);
        baseStats.Will = baseStats.BaseWill + (2 * levelBonus);

        // HP/Resources: Base × (1 + 0.1 × (Level-1))
        var resourceScaling = 1.0 + (0.1 * levelBonus);
        baseStats.MaxHP = (int)(baseStats.BaseMaxHP * resourceScaling);
        baseStats.MaxResource = (int)(baseStats.BaseMaxResource * resourceScaling);

        // Defense and Soak scale with level (simplified)
        baseStats.Defense = baseStats.BaseDefense + levelBonus;
        baseStats.Soak = baseStats.BaseSoak + (levelBonus / 2); // Slower scaling

        baseStats.Level = level;

        _log.Debug("Calculated scaled stats: CompanionId={CompanionId}, Level={Level}, HP={HP}, MIGHT={MIGHT}",
            companionId, level, baseStats.MaxHP, baseStats.Might);

        return baseStats;
    }

    /// <summary>
    /// Equips an item to a companion's equipment slot
    /// </summary>
    public bool EquipCompanionItem(int characterId, int companionId, int itemId, string slot)
    {
        if (!new[] { "weapon", "armor", "accessory" }.Contains(slot.ToLower()))
        {
            _log.Warning("Invalid equipment slot: {Slot}", slot);
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var columnName = slot.ToLower() switch
        {
            "weapon" => "equipped_weapon_id",
            "armor" => "equipped_armor_id",
            "accessory" => "equipped_accessory_id",
            _ => null
        };

        if (columnName == null)
        {
            return false;
        }

        var command = connection.CreateCommand();
        command.CommandText = $@"
            UPDATE Companion_Progression
            SET {columnName} = @itemId,
                updated_at = @updatedAt
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        command.Parameters.AddWithValue("@itemId", itemId);
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@companionId", companionId);

        var rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            _log.Information("Item equipped: CharacterId={CharacterId}, CompanionId={CompanionId}, ItemId={ItemId}, Slot={Slot}",
                characterId, companionId, itemId, slot);
            return true;
        }

        _log.Warning("Failed to equip item: CharacterId={CharacterId}, CompanionId={CompanionId}, ItemId={ItemId}, Slot={Slot}",
            characterId, companionId, itemId, slot);
        return false;
    }

    /// <summary>
    /// Unequips an item from a companion's equipment slot
    /// </summary>
    public bool UnequipCompanionItem(int characterId, int companionId, string slot)
    {
        if (!new[] { "weapon", "armor", "accessory" }.Contains(slot.ToLower()))
        {
            _log.Warning("Invalid equipment slot: {Slot}", slot);
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var columnName = slot.ToLower() switch
        {
            "weapon" => "equipped_weapon_id",
            "armor" => "equipped_armor_id",
            "accessory" => "equipped_accessory_id",
            _ => null
        };

        if (columnName == null)
        {
            return false;
        }

        var command = connection.CreateCommand();
        command.CommandText = $@"
            UPDATE Companion_Progression
            SET {columnName} = NULL,
                updated_at = @updatedAt
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@companionId", companionId);

        var rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            _log.Information("Item unequipped: CharacterId={CharacterId}, CompanionId={CompanionId}, Slot={Slot}",
                characterId, companionId, slot);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Manually unlocks an ability for a companion
    /// Used for quest rewards or special unlocks
    /// </summary>
    public bool UnlockAbility(int characterId, int companionId, int abilityId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var progression = GetCompanionProgression(characterId, companionId, connection);
        if (progression == null)
        {
            _log.Warning("Companion progression not found: CharacterId={CharacterId}, CompanionId={CompanionId}",
                characterId, companionId);
            return false;
        }

        // Parse existing abilities
        var abilities = JsonSerializer.Deserialize<List<int>>(progression.UnlockedAbilities) ?? new List<int>();

        if (abilities.Contains(abilityId))
        {
            _log.Debug("Ability already unlocked: AbilityId={AbilityId}", abilityId);
            return false;
        }

        abilities.Add(abilityId);

        // Update progression
        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Companion_Progression
            SET unlocked_abilities = @abilities,
                updated_at = @updatedAt
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        command.Parameters.AddWithValue("@abilities", JsonSerializer.Serialize(abilities));
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@companionId", companionId);

        var rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            _log.Information("Ability unlocked: CharacterId={CharacterId}, CompanionId={CompanionId}, AbilityId={AbilityId}",
                characterId, companionId, abilityId);
            return true;
        }

        return false;
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private CompanionProgressionData? GetCompanionProgression(int characterId, int companionId, SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT current_level, current_legend, legend_to_next_level, unlocked_abilities
            FROM Companion_Progression
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@companionId", companionId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new CompanionProgressionData
            {
                CurrentLevel = reader.GetInt32(0),
                CurrentLegend = reader.GetInt32(1),
                LegendToNextLevel = reader.GetInt32(2),
                UnlockedAbilities = reader.GetString(3)
            };
        }

        return null;
    }

    private int CalculateLegendToNextLevel(int currentLevel)
    {
        // Exponential scaling matching player progression
        return (int)(BASE_LEGEND_TO_NEXT_LEVEL * Math.Pow(LEGEND_SCALING_FACTOR, currentLevel - 1));
    }

    private void UnlockAbilityAtLevel(int characterId, int companionId, int level, SqliteConnection connection)
    {
        _log.Information("Unlocking ability at level {Level} for CompanionId={CompanionId}",
            level, companionId);

        // Note: Actual ability IDs would need to be defined per companion
        // This is a placeholder - actual implementation would query companion-specific abilities
        // from database or configuration

        // For now, just log that an ability should be unlocked
        _log.Information("Ability unlock triggered: CharacterId={CharacterId}, CompanionId={CompanionId}, Level={Level}",
            characterId, companionId, level);
    }
}

/// <summary>
/// Internal DTO for companion progression data
/// </summary>
internal class CompanionProgressionData
{
    public int CurrentLevel { get; set; }
    public int CurrentLegend { get; set; }
    public int LegendToNextLevel { get; set; }
    public string UnlockedAbilities { get; set; } = "[]";
}

/// <summary>
/// Scaled stats for a companion at their current level
/// </summary>
public class CompanionScaledStats
{
    public int Level { get; set; } = 1;

    // Base stats (from database)
    public int BaseMight { get; set; }
    public int BaseFinesse { get; set; }
    public int BaseSturdiness { get; set; }
    public int BaseWits { get; set; }
    public int BaseWill { get; set; }
    public int BaseMaxHP { get; set; }
    public int BaseDefense { get; set; }
    public int BaseSoak { get; set; }
    public int BaseMaxResource { get; set; }

    // Scaled stats (calculated)
    public int Might { get; set; }
    public int Finesse { get; set; }
    public int Sturdiness { get; set; }
    public int Wits { get; set; }
    public int Will { get; set; }
    public int MaxHP { get; set; }
    public int Defense { get; set; }
    public int Soak { get; set; }
    public int MaxResource { get; set; }
}
