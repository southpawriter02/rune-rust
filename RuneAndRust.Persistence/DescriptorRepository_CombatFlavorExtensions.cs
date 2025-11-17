using Microsoft.Data.Sqlite;
using RuneAndRust.Core.CombatFlavor;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38.6: Combat flavor text extensions for DescriptorRepository
/// Provides database access to combat action descriptors, enemy voice profiles, and environmental combat modifiers
/// </summary>
public partial class DescriptorRepository
{
    private static readonly ILogger _combatFlavorLog = Log.ForContext<DescriptorRepository>();

    #region Combat Action Descriptors

    /// <summary>
    /// Gets combat action descriptors with optional filtering
    /// </summary>
    public List<CombatActionDescriptor> GetCombatActionDescriptors(
        CombatActionCategory? category = null,
        string? weaponType = null,
        string? enemyArchetype = null,
        CombatOutcome? outcomeType = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, category, weapon_type, enemy_archetype,
                outcome_type, descriptor_text, tags
            FROM Combat_Action_Descriptors
            WHERE 1=1
        ";

        if (category.HasValue)
        {
            sql += " AND category = $category";
            command.Parameters.AddWithValue("$category", category.Value.ToString());
        }

        if (!string.IsNullOrEmpty(weaponType))
        {
            sql += " AND weapon_type = $weaponType";
            command.Parameters.AddWithValue("$weaponType", weaponType);
        }

        if (!string.IsNullOrEmpty(enemyArchetype))
        {
            sql += " AND enemy_archetype = $enemyArchetype";
            command.Parameters.AddWithValue("$enemyArchetype", enemyArchetype);
        }

        if (outcomeType.HasValue)
        {
            sql += " AND outcome_type = $outcomeType";
            command.Parameters.AddWithValue("$outcomeType", outcomeType.Value.ToString());
        }

        command.CommandText = sql;

        var descriptors = new List<CombatActionDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapCombatActionDescriptor(reader));
        }

        _combatFlavorLog.Debug("Loaded {Count} combat action descriptors (Category: {Category}, Weapon: {Weapon}, Outcome: {Outcome})",
            descriptors.Count, category, weaponType, outcomeType);

        return descriptors;
    }

    /// <summary>
    /// Gets a combat action descriptor by ID
    /// </summary>
    public CombatActionDescriptor? GetCombatActionDescriptorById(int descriptorId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                descriptor_id, category, weapon_type, enemy_archetype,
                outcome_type, descriptor_text, tags
            FROM Combat_Action_Descriptors
            WHERE descriptor_id = $descriptorId
        ";
        command.Parameters.AddWithValue("$descriptorId", descriptorId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapCombatActionDescriptor(reader);
        }

        _combatFlavorLog.Warning("Combat action descriptor not found: ID {DescriptorId}", descriptorId);
        return null;
    }

    /// <summary>
    /// Gets combat action descriptors by IDs
    /// </summary>
    public List<CombatActionDescriptor> GetCombatActionDescriptorsByIds(List<int> descriptorIds)
    {
        if (descriptorIds == null || descriptorIds.Count == 0)
            return new List<CombatActionDescriptor>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var idParams = string.Join(",", descriptorIds.Select((_, i) => $"$id{i}"));
        command.CommandText = $@"
            SELECT
                descriptor_id, category, weapon_type, enemy_archetype,
                outcome_type, descriptor_text, tags
            FROM Combat_Action_Descriptors
            WHERE descriptor_id IN ({idParams})
        ";

        for (int i = 0; i < descriptorIds.Count; i++)
        {
            command.Parameters.AddWithValue($"$id{i}", descriptorIds[i]);
        }

        var descriptors = new List<CombatActionDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapCombatActionDescriptor(reader));
        }

        _combatFlavorLog.Debug("Loaded {Count} combat action descriptors by IDs", descriptors.Count);
        return descriptors;
    }

    private CombatActionDescriptor MapCombatActionDescriptor(SqliteDataReader reader)
    {
        return new CombatActionDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            Category = reader.GetString(1),
            WeaponType = reader.IsDBNull(2) ? null : reader.GetString(2),
            EnemyArchetype = reader.IsDBNull(3) ? null : reader.GetString(3),
            OutcomeType = reader.IsDBNull(4) ? null : reader.GetString(4),
            DescriptorText = reader.GetString(5),
            Tags = reader.IsDBNull(6) ? null : reader.GetString(6)
        };
    }

    #endregion

    #region Enemy Voice Profiles

    /// <summary>
    /// Gets all enemy voice profiles
    /// </summary>
    public List<EnemyVoiceProfile> GetAllEnemyVoiceProfiles()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                profile_id, enemy_archetype, voice_description, setting_context,
                attack_descriptors, reaction_damage, reaction_death, special_attacks
            FROM Enemy_Voice_Profiles
            ORDER BY enemy_archetype
        ";

        var profiles = new List<EnemyVoiceProfile>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            profiles.Add(MapEnemyVoiceProfile(reader));
        }

        _combatFlavorLog.Debug("Loaded {Count} enemy voice profiles", profiles.Count);
        return profiles;
    }

    /// <summary>
    /// Gets an enemy voice profile by archetype
    /// </summary>
    public EnemyVoiceProfile? GetEnemyVoiceProfile(string archetype)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                profile_id, enemy_archetype, voice_description, setting_context,
                attack_descriptors, reaction_damage, reaction_death, special_attacks
            FROM Enemy_Voice_Profiles
            WHERE enemy_archetype = $archetype
        ";
        command.Parameters.AddWithValue("$archetype", archetype);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapEnemyVoiceProfile(reader);
        }

        _combatFlavorLog.Warning("Enemy voice profile not found: {Archetype}", archetype);
        return null;
    }

    /// <summary>
    /// Updates enemy voice profile descriptor arrays
    /// Used to populate descriptor IDs after initial content insertion
    /// </summary>
    public void UpdateEnemyVoiceProfileDescriptors(
        string archetype,
        List<int> attackDescriptors,
        List<int> reactionDamage,
        List<int> reactionDeath,
        List<int>? specialAttacks = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Enemy_Voice_Profiles
            SET
                attack_descriptors = $attackDescriptors,
                reaction_damage = $reactionDamage,
                reaction_death = $reactionDeath,
                special_attacks = $specialAttacks
            WHERE enemy_archetype = $archetype
        ";

        command.Parameters.AddWithValue("$archetype", archetype);
        command.Parameters.AddWithValue("$attackDescriptors", JsonSerializer.Serialize(attackDescriptors));
        command.Parameters.AddWithValue("$reactionDamage", JsonSerializer.Serialize(reactionDamage));
        command.Parameters.AddWithValue("$reactionDeath", JsonSerializer.Serialize(reactionDeath));
        command.Parameters.AddWithValue("$specialAttacks",
            specialAttacks != null ? JsonSerializer.Serialize(specialAttacks) : "[]");

        var rowsAffected = command.ExecuteNonQuery();
        _combatFlavorLog.Information("Updated enemy voice profile: {Archetype} ({RowsAffected} rows)",
            archetype, rowsAffected);
    }

    private EnemyVoiceProfile MapEnemyVoiceProfile(SqliteDataReader reader)
    {
        return new EnemyVoiceProfile
        {
            ProfileId = reader.GetInt32(0),
            EnemyArchetype = reader.GetString(1),
            VoiceDescription = reader.GetString(2),
            SettingContext = reader.GetString(3),
            AttackDescriptors = reader.GetString(4),
            ReactionDamage = reader.GetString(5),
            ReactionDeath = reader.GetString(6),
            SpecialAttacks = reader.IsDBNull(7) ? null : reader.GetString(7)
        };
    }

    #endregion

    #region Environmental Combat Modifiers

    /// <summary>
    /// Gets environmental combat modifiers for a specific biome
    /// </summary>
    public List<EnvironmentalCombatModifier> GetEnvironmentalCombatModifiers(
        string biomeName,
        EnvironmentalModifierType? modifierType = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                modifier_id, biome_name, modifier_type,
                descriptor_text, trigger_chance
            FROM Environmental_Combat_Modifiers
            WHERE biome_name = $biomeName
        ";

        command.Parameters.AddWithValue("$biomeName", biomeName);

        if (modifierType.HasValue)
        {
            sql += " AND modifier_type = $modifierType";
            command.Parameters.AddWithValue("$modifierType", modifierType.Value.ToString());
        }

        command.CommandText = sql;

        var modifiers = new List<EnvironmentalCombatModifier>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            modifiers.Add(MapEnvironmentalCombatModifier(reader));
        }

        _combatFlavorLog.Debug("Loaded {Count} environmental combat modifiers for biome {Biome}",
            modifiers.Count, biomeName);

        return modifiers;
    }

    /// <summary>
    /// Gets all environmental combat modifiers
    /// </summary>
    public List<EnvironmentalCombatModifier> GetAllEnvironmentalCombatModifiers()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                modifier_id, biome_name, modifier_type,
                descriptor_text, trigger_chance
            FROM Environmental_Combat_Modifiers
            ORDER BY biome_name, modifier_type
        ";

        var modifiers = new List<EnvironmentalCombatModifier>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            modifiers.Add(MapEnvironmentalCombatModifier(reader));
        }

        _combatFlavorLog.Debug("Loaded {Count} environmental combat modifiers", modifiers.Count);
        return modifiers;
    }

    private EnvironmentalCombatModifier MapEnvironmentalCombatModifier(SqliteDataReader reader)
    {
        return new EnvironmentalCombatModifier
        {
            ModifierId = reader.GetInt32(0),
            BiomeName = reader.GetString(1),
            ModifierType = reader.GetString(2),
            DescriptorText = reader.GetString(3),
            TriggerChance = (float)reader.GetDouble(4)
        };
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets statistics about the combat flavor text library
    /// </summary>
    public CombatFlavorTextStats GetCombatFlavorTextStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var stats = new CombatFlavorTextStats
        {
            TotalCombatDescriptors = GetCount(connection, "Combat_Action_Descriptors"),
            TotalEnemyVoiceProfiles = GetCount(connection, "Enemy_Voice_Profiles"),
            TotalEnvironmentalModifiers = GetCount(connection, "Environmental_Combat_Modifiers"),
            DescriptorsByCategory = GetGroupCount(connection, "Combat_Action_Descriptors", "category"),
            ModifiersByBiome = GetGroupCount(connection, "Environmental_Combat_Modifiers", "biome_name")
        };

        _combatFlavorLog.Information("Combat flavor text stats: {Descriptors} descriptors, {Profiles} voice profiles, {Modifiers} environmental modifiers",
            stats.TotalCombatDescriptors, stats.TotalEnemyVoiceProfiles, stats.TotalEnvironmentalModifiers);

        return stats;
    }

    #endregion
}

/// <summary>
/// Statistics about the combat flavor text library
/// </summary>
public class CombatFlavorTextStats
{
    public int TotalCombatDescriptors { get; set; }
    public int TotalEnemyVoiceProfiles { get; set; }
    public int TotalEnvironmentalModifiers { get; set; }
    public Dictionary<string, int> DescriptorsByCategory { get; set; } = new();
    public Dictionary<string, int> ModifiersByBiome { get; set; } = new();
}
