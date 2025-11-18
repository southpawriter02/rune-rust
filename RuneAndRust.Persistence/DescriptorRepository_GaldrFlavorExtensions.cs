// ==============================================================================
// v0.38.7: Ability & Galdr Flavor Text
// DescriptorRepository_GaldrFlavorExtensions.cs
// ==============================================================================
// Purpose: Repository extensions for Galdr and ability flavor text
// Pattern: Follows DescriptorRepository_CombatFlavorExtensions structure
// Integration: Used by GaldrFlavorTextService
// ==============================================================================

using Microsoft.Data.Sqlite;
using RuneAndRust.Core.GaldrFlavor;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38.7: Galdr flavor text extensions for DescriptorRepository.
/// Provides database access to Galdr action descriptors, manifestations, outcomes, miscasts, and caster voices.
/// </summary>
public partial class DescriptorRepository
{
    private static readonly ILogger _galdrFlavorLog = Log.ForContext<DescriptorRepository>();

    #region Galdr Action Descriptors

    /// <summary>
    /// Gets Galdr action descriptors with optional filtering.
    /// </summary>
    public List<GaldrActionDescriptor> GetGaldrActionDescriptors(
        string? category = null,
        string? runeSchool = null,
        string? abilityName = null,
        string? successLevel = null,
        string? biomeName = null,
        string? actionType = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, category, action_type, rune_school, ability_name,
                success_level, ability_category, weapon_type, biome_name,
                descriptor_text, weight, is_active, tags
            FROM Galdr_Action_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(category))
        {
            sql += " AND category = $category";
            command.Parameters.AddWithValue("$category", category);
        }

        if (!string.IsNullOrEmpty(runeSchool))
        {
            sql += " AND (rune_school = $runeSchool OR rune_school IS NULL)";
            command.Parameters.AddWithValue("$runeSchool", runeSchool);
        }

        if (!string.IsNullOrEmpty(abilityName))
        {
            sql += " AND (ability_name = $abilityName OR ability_name IS NULL)";
            command.Parameters.AddWithValue("$abilityName", abilityName);
        }

        if (!string.IsNullOrEmpty(successLevel))
        {
            sql += " AND (success_level = $successLevel OR success_level IS NULL)";
            command.Parameters.AddWithValue("$successLevel", successLevel);
        }

        if (!string.IsNullOrEmpty(biomeName))
        {
            sql += " AND (biome_name = $biomeName OR biome_name IS NULL)";
            command.Parameters.AddWithValue("$biomeName", biomeName);
        }

        if (!string.IsNullOrEmpty(actionType))
        {
            sql += " AND (action_type = $actionType OR action_type IS NULL)";
            command.Parameters.AddWithValue("$actionType", actionType);
        }

        command.CommandText = sql;

        var descriptors = new List<GaldrActionDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapGaldrActionDescriptor(reader));
        }

        _galdrFlavorLog.Debug("Loaded {Count} Galdr action descriptors (Category: {Category}, Rune: {Rune}, Ability: {Ability}, Success: {Success})",
            descriptors.Count, category, runeSchool, abilityName, successLevel);

        return descriptors;
    }

    /// <summary>
    /// Gets a Galdr action descriptor by ID.
    /// </summary>
    public GaldrActionDescriptor? GetGaldrActionDescriptorById(int descriptorId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                descriptor_id, category, action_type, rune_school, ability_name,
                success_level, ability_category, weapon_type, biome_name,
                descriptor_text, weight, is_active, tags
            FROM Galdr_Action_Descriptors
            WHERE descriptor_id = $descriptorId
        ";
        command.Parameters.AddWithValue("$descriptorId", descriptorId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapGaldrActionDescriptor(reader);
        }

        _galdrFlavorLog.Warning("Galdr action descriptor not found: ID {DescriptorId}", descriptorId);
        return null;
    }

    private GaldrActionDescriptor MapGaldrActionDescriptor(SqliteDataReader reader)
    {
        return new GaldrActionDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            Category = reader.GetString(1),
            ActionType = reader.IsDBNull(2) ? null : reader.GetString(2),
            RuneSchool = reader.IsDBNull(3) ? null : reader.GetString(3),
            AbilityName = reader.IsDBNull(4) ? null : reader.GetString(4),
            SuccessLevel = reader.IsDBNull(5) ? null : reader.GetString(5),
            AbilityCategory = reader.IsDBNull(6) ? null : reader.GetString(6),
            WeaponType = reader.IsDBNull(7) ? null : reader.GetString(7),
            BiomeName = reader.IsDBNull(8) ? null : reader.GetString(8),
            DescriptorText = reader.GetString(9),
            Weight = (float)reader.GetDouble(10),
            IsActive = reader.GetInt32(11) == 1,
            Tags = reader.IsDBNull(12) ? null : reader.GetString(12)
        };
    }

    #endregion

    #region Galdr Manifestation Descriptors

    /// <summary>
    /// Gets Galdr manifestation descriptors (visual/sensory effects).
    /// </summary>
    public List<GaldrManifestationDescriptor> GetGaldrManifestationDescriptors(
        string? runeSchool = null,
        string? manifestationType = null,
        string? element = null,
        string? powerLevel = null,
        string? biomeName = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                manifestation_id, rune_school, manifestation_type, element,
                power_level, biome_name, environmental_interaction,
                descriptor_text, weight, is_active, tags
            FROM Galdr_Manifestation_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(runeSchool))
        {
            sql += " AND rune_school = $runeSchool";
            command.Parameters.AddWithValue("$runeSchool", runeSchool);
        }

        if (!string.IsNullOrEmpty(manifestationType))
        {
            sql += " AND manifestation_type = $manifestationType";
            command.Parameters.AddWithValue("$manifestationType", manifestationType);
        }

        if (!string.IsNullOrEmpty(element))
        {
            sql += " AND (element = $element OR element IS NULL)";
            command.Parameters.AddWithValue("$element", element);
        }

        if (!string.IsNullOrEmpty(powerLevel))
        {
            sql += " AND (power_level = $powerLevel OR power_level IS NULL)";
            command.Parameters.AddWithValue("$powerLevel", powerLevel);
        }

        if (!string.IsNullOrEmpty(biomeName))
        {
            sql += " AND (biome_name = $biomeName OR biome_name IS NULL)";
            command.Parameters.AddWithValue("$biomeName", biomeName);
        }

        command.CommandText = sql;

        var descriptors = new List<GaldrManifestationDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapGaldrManifestationDescriptor(reader));
        }

        _galdrFlavorLog.Debug("Loaded {Count} Galdr manifestation descriptors", descriptors.Count);
        return descriptors;
    }

    private GaldrManifestationDescriptor MapGaldrManifestationDescriptor(SqliteDataReader reader)
    {
        return new GaldrManifestationDescriptor
        {
            ManifestationId = reader.GetInt32(0),
            RuneSchool = reader.GetString(1),
            ManifestationType = reader.GetString(2),
            Element = reader.IsDBNull(3) ? null : reader.GetString(3),
            PowerLevel = reader.IsDBNull(4) ? null : reader.GetString(4),
            BiomeName = reader.IsDBNull(5) ? null : reader.GetString(5),
            EnvironmentalInteraction = reader.IsDBNull(6) ? null : reader.GetString(6),
            DescriptorText = reader.GetString(7),
            Weight = (float)reader.GetDouble(8),
            IsActive = reader.GetInt32(9) == 1,
            Tags = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }

    #endregion

    #region Galdr Outcome Descriptors

    /// <summary>
    /// Gets Galdr outcome descriptors (ability resolution).
    /// </summary>
    public List<GaldrOutcomeDescriptor> GetGaldrOutcomeDescriptors(
        string? abilityName = null,
        string? outcomeType = null,
        string? targetType = null,
        string? enemyArchetype = null,
        string? effectCategory = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                outcome_id, ability_name, outcome_type, success_count,
                target_type, enemy_archetype, effect_category,
                descriptor_text, weight, is_active, tags
            FROM Galdr_Outcome_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(abilityName))
        {
            sql += " AND ability_name = $abilityName";
            command.Parameters.AddWithValue("$abilityName", abilityName);
        }

        if (!string.IsNullOrEmpty(outcomeType))
        {
            sql += " AND outcome_type = $outcomeType";
            command.Parameters.AddWithValue("$outcomeType", outcomeType);
        }

        if (!string.IsNullOrEmpty(targetType))
        {
            sql += " AND (target_type = $targetType OR target_type IS NULL)";
            command.Parameters.AddWithValue("$targetType", targetType);
        }

        if (!string.IsNullOrEmpty(enemyArchetype))
        {
            sql += " AND (enemy_archetype = $enemyArchetype OR enemy_archetype IS NULL)";
            command.Parameters.AddWithValue("$enemyArchetype", enemyArchetype);
        }

        if (!string.IsNullOrEmpty(effectCategory))
        {
            sql += " AND (effect_category = $effectCategory OR effect_category IS NULL)";
            command.Parameters.AddWithValue("$effectCategory", effectCategory);
        }

        command.CommandText = sql;

        var descriptors = new List<GaldrOutcomeDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapGaldrOutcomeDescriptor(reader));
        }

        _galdrFlavorLog.Debug("Loaded {Count} Galdr outcome descriptors", descriptors.Count);
        return descriptors;
    }

    private GaldrOutcomeDescriptor MapGaldrOutcomeDescriptor(SqliteDataReader reader)
    {
        return new GaldrOutcomeDescriptor
        {
            OutcomeId = reader.GetInt32(0),
            AbilityName = reader.GetString(1),
            OutcomeType = reader.GetString(2),
            SuccessCount = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            TargetType = reader.IsDBNull(4) ? null : reader.GetString(4),
            EnemyArchetype = reader.IsDBNull(5) ? null : reader.GetString(5),
            EffectCategory = reader.IsDBNull(6) ? null : reader.GetString(6),
            DescriptorText = reader.GetString(7),
            Weight = (float)reader.GetDouble(8),
            IsActive = reader.GetInt32(9) == 1,
            Tags = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }

    #endregion

    #region Galdr Miscast Descriptors

    /// <summary>
    /// Gets Galdr miscast descriptors (paradox, Blight corruption).
    /// </summary>
    public List<GaldrMiscastDescriptor> GetGaldrMiscastDescriptors(
        string? miscastType = null,
        string? severity = null,
        string? runeSchool = null,
        string? abilityName = null,
        string? biomeName = null,
        string? corruptionSource = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                miscast_id, miscast_type, severity, rune_school, ability_name,
                corruption_source, biome_name, descriptor_text, mechanical_effect,
                weight, is_active, tags
            FROM Galdr_Miscast_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(miscastType))
        {
            sql += " AND miscast_type = $miscastType";
            command.Parameters.AddWithValue("$miscastType", miscastType);
        }

        if (!string.IsNullOrEmpty(severity))
        {
            sql += " AND severity = $severity";
            command.Parameters.AddWithValue("$severity", severity);
        }

        if (!string.IsNullOrEmpty(runeSchool))
        {
            sql += " AND (rune_school = $runeSchool OR rune_school IS NULL)";
            command.Parameters.AddWithValue("$runeSchool", runeSchool);
        }

        if (!string.IsNullOrEmpty(abilityName))
        {
            sql += " AND (ability_name = $abilityName OR ability_name IS NULL)";
            command.Parameters.AddWithValue("$abilityName", abilityName);
        }

        if (!string.IsNullOrEmpty(biomeName))
        {
            sql += " AND (biome_name = $biomeName OR biome_name IS NULL)";
            command.Parameters.AddWithValue("$biomeName", biomeName);
        }

        if (!string.IsNullOrEmpty(corruptionSource))
        {
            sql += " AND (corruption_source = $corruptionSource OR corruption_source IS NULL)";
            command.Parameters.AddWithValue("$corruptionSource", corruptionSource);
        }

        command.CommandText = sql;

        var descriptors = new List<GaldrMiscastDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapGaldrMiscastDescriptor(reader));
        }

        _galdrFlavorLog.Debug("Loaded {Count} Galdr miscast descriptors", descriptors.Count);
        return descriptors;
    }

    private GaldrMiscastDescriptor MapGaldrMiscastDescriptor(SqliteDataReader reader)
    {
        return new GaldrMiscastDescriptor
        {
            MiscastId = reader.GetInt32(0),
            MiscastType = reader.GetString(1),
            Severity = reader.GetString(2),
            RuneSchool = reader.IsDBNull(3) ? null : reader.GetString(3),
            AbilityName = reader.IsDBNull(4) ? null : reader.GetString(4),
            CorruptionSource = reader.IsDBNull(5) ? null : reader.GetString(5),
            BiomeName = reader.IsDBNull(6) ? null : reader.GetString(6),
            DescriptorText = reader.GetString(7),
            MechanicalEffect = reader.IsDBNull(8) ? null : reader.GetString(8),
            Weight = (float)reader.GetDouble(9),
            IsActive = reader.GetInt32(10) == 1,
            Tags = reader.IsDBNull(11) ? null : reader.GetString(11)
        };
    }

    #endregion

    #region Galdr Caster Voice Profiles

    /// <summary>
    /// Gets all Galdr caster voice profiles.
    /// </summary>
    public List<GaldrCasterVoiceProfile> GetAllGaldrCasterVoiceProfiles()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                voice_id, caster_archetype, voice_name, voice_description,
                casting_style, setting_context, invocation_descriptors,
                manifestation_descriptors, miscast_descriptors, preferred_schools,
                is_active, tags
            FROM Galdr_Caster_Voices
            WHERE is_active = 1
            ORDER BY caster_archetype
        ";

        var profiles = new List<GaldrCasterVoiceProfile>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            profiles.Add(MapGaldrCasterVoiceProfile(reader));
        }

        _galdrFlavorLog.Debug("Loaded {Count} Galdr caster voice profiles", profiles.Count);
        return profiles;
    }

    /// <summary>
    /// Gets a Galdr caster voice profile by archetype.
    /// </summary>
    public GaldrCasterVoiceProfile? GetGaldrCasterVoiceProfile(string casterArchetype)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                voice_id, caster_archetype, voice_name, voice_description,
                casting_style, setting_context, invocation_descriptors,
                manifestation_descriptors, miscast_descriptors, preferred_schools,
                is_active, tags
            FROM Galdr_Caster_Voices
            WHERE caster_archetype = $casterArchetype
            AND is_active = 1
        ";
        command.Parameters.AddWithValue("$casterArchetype", casterArchetype);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapGaldrCasterVoiceProfile(reader);
        }

        _galdrFlavorLog.Warning("Galdr caster voice profile not found: {Archetype}", casterArchetype);
        return null;
    }

    /// <summary>
    /// Updates Galdr caster voice profile descriptor arrays.
    /// </summary>
    public void UpdateGaldrCasterVoiceProfileDescriptors(
        string casterArchetype,
        List<int> invocationDescriptors,
        List<int> manifestationDescriptors,
        List<int> miscastDescriptors,
        List<string> preferredSchools)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Galdr_Caster_Voices
            SET
                invocation_descriptors = $invocationDescriptors,
                manifestation_descriptors = $manifestationDescriptors,
                miscast_descriptors = $miscastDescriptors,
                preferred_schools = $preferredSchools
            WHERE caster_archetype = $casterArchetype
        ";

        command.Parameters.AddWithValue("$casterArchetype", casterArchetype);
        command.Parameters.AddWithValue("$invocationDescriptors", JsonSerializer.Serialize(invocationDescriptors));
        command.Parameters.AddWithValue("$manifestationDescriptors", JsonSerializer.Serialize(manifestationDescriptors));
        command.Parameters.AddWithValue("$miscastDescriptors", JsonSerializer.Serialize(miscastDescriptors));
        command.Parameters.AddWithValue("$preferredSchools", JsonSerializer.Serialize(preferredSchools));

        var rowsAffected = command.ExecuteNonQuery();
        _galdrFlavorLog.Information("Updated Galdr caster voice profile: {Archetype} ({RowsAffected} rows)",
            casterArchetype, rowsAffected);
    }

    private GaldrCasterVoiceProfile MapGaldrCasterVoiceProfile(SqliteDataReader reader)
    {
        return new GaldrCasterVoiceProfile
        {
            VoiceId = reader.GetInt32(0),
            CasterArchetype = reader.GetString(1),
            VoiceName = reader.GetString(2),
            VoiceDescription = reader.GetString(3),
            CastingStyle = reader.GetString(4),
            SettingContext = reader.IsDBNull(5) ? null : reader.GetString(5),
            InvocationDescriptors = reader.IsDBNull(6) ? null : reader.GetString(6),
            ManifestationDescriptors = reader.IsDBNull(7) ? null : reader.GetString(7),
            MiscastDescriptors = reader.IsDBNull(8) ? null : reader.GetString(8),
            PreferredSchools = reader.IsDBNull(9) ? null : reader.GetString(9),
            IsActive = reader.GetInt32(10) == 1,
            Tags = reader.IsDBNull(11) ? null : reader.GetString(11)
        };
    }

    #endregion

    #region Galdr Environmental Reactions

    /// <summary>
    /// Gets Galdr environmental reactions for a specific biome.
    /// </summary>
    public List<GaldrEnvironmentalReaction> GetGaldrEnvironmentalReactions(
        string? biomeName = null,
        string? runeSchool = null,
        string? element = null,
        string? reactionType = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                reaction_id, biome_name, reaction_type, rune_school, element,
                trigger_chance, power_level_min, descriptor_text,
                weight, is_active, tags
            FROM Galdr_Environmental_Reactions
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(biomeName))
        {
            sql += " AND biome_name = $biomeName";
            command.Parameters.AddWithValue("$biomeName", biomeName);
        }

        if (!string.IsNullOrEmpty(runeSchool))
        {
            sql += " AND (rune_school = $runeSchool OR rune_school IS NULL)";
            command.Parameters.AddWithValue("$runeSchool", runeSchool);
        }

        if (!string.IsNullOrEmpty(element))
        {
            sql += " AND (element = $element OR element IS NULL)";
            command.Parameters.AddWithValue("$element", element);
        }

        if (!string.IsNullOrEmpty(reactionType))
        {
            sql += " AND reaction_type = $reactionType";
            command.Parameters.AddWithValue("$reactionType", reactionType);
        }

        command.CommandText = sql;

        var reactions = new List<GaldrEnvironmentalReaction>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            reactions.Add(MapGaldrEnvironmentalReaction(reader));
        }

        _galdrFlavorLog.Debug("Loaded {Count} Galdr environmental reactions for biome {Biome}",
            reactions.Count, biomeName);

        return reactions;
    }

    private GaldrEnvironmentalReaction MapGaldrEnvironmentalReaction(SqliteDataReader reader)
    {
        return new GaldrEnvironmentalReaction
        {
            ReactionId = reader.GetInt32(0),
            BiomeName = reader.GetString(1),
            ReactionType = reader.GetString(2),
            RuneSchool = reader.IsDBNull(3) ? null : reader.GetString(3),
            Element = reader.IsDBNull(4) ? null : reader.GetString(4),
            TriggerChance = (float)reader.GetDouble(5),
            PowerLevelMin = reader.IsDBNull(6) ? null : reader.GetString(6),
            DescriptorText = reader.GetString(7),
            Weight = (float)reader.GetDouble(8),
            IsActive = reader.GetInt32(9) == 1,
            Tags = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }

    #endregion

    #region Ability Flavor Descriptors

    /// <summary>
    /// Gets ability flavor descriptors (non-Galdr abilities).
    /// </summary>
    public List<AbilityFlavorDescriptor> GetAbilityFlavorDescriptors(
        string? abilityCategory = null,
        string? abilityName = null,
        string? weaponType = null,
        string? specialization = null,
        string? successLevel = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, ability_category, ability_name, weapon_type,
                specialization, success_level, descriptor_text,
                weight, is_active, tags
            FROM Ability_Flavor_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(abilityCategory))
        {
            sql += " AND ability_category = $abilityCategory";
            command.Parameters.AddWithValue("$abilityCategory", abilityCategory);
        }

        if (!string.IsNullOrEmpty(abilityName))
        {
            sql += " AND ability_name = $abilityName";
            command.Parameters.AddWithValue("$abilityName", abilityName);
        }

        if (!string.IsNullOrEmpty(weaponType))
        {
            sql += " AND (weapon_type = $weaponType OR weapon_type IS NULL)";
            command.Parameters.AddWithValue("$weaponType", weaponType);
        }

        if (!string.IsNullOrEmpty(specialization))
        {
            sql += " AND (specialization = $specialization OR specialization IS NULL)";
            command.Parameters.AddWithValue("$specialization", specialization);
        }

        if (!string.IsNullOrEmpty(successLevel))
        {
            sql += " AND (success_level = $successLevel OR success_level IS NULL)";
            command.Parameters.AddWithValue("$successLevel", successLevel);
        }

        command.CommandText = sql;

        var descriptors = new List<AbilityFlavorDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapAbilityFlavorDescriptor(reader));
        }

        _galdrFlavorLog.Debug("Loaded {Count} ability flavor descriptors", descriptors.Count);
        return descriptors;
    }

    private AbilityFlavorDescriptor MapAbilityFlavorDescriptor(SqliteDataReader reader)
    {
        return new AbilityFlavorDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            AbilityCategory = reader.GetString(1),
            AbilityName = reader.GetString(2),
            WeaponType = reader.IsDBNull(3) ? null : reader.GetString(3),
            Specialization = reader.IsDBNull(4) ? null : reader.GetString(4),
            SuccessLevel = reader.IsDBNull(5) ? null : reader.GetString(5),
            DescriptorText = reader.GetString(6),
            Weight = (float)reader.GetDouble(7),
            IsActive = reader.GetInt32(8) == 1,
            Tags = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets statistics about the Galdr flavor text library.
    /// </summary>
    public GaldrFlavorTextStats GetGaldrFlavorTextStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var stats = new GaldrFlavorTextStats
        {
            TotalGaldrActionDescriptors = GetCount(connection, "Galdr_Action_Descriptors"),
            TotalManifestationDescriptors = GetCount(connection, "Galdr_Manifestation_Descriptors"),
            TotalOutcomeDescriptors = GetCount(connection, "Galdr_Outcome_Descriptors"),
            TotalMiscastDescriptors = GetCount(connection, "Galdr_Miscast_Descriptors"),
            TotalCasterVoiceProfiles = GetCount(connection, "Galdr_Caster_Voices"),
            TotalEnvironmentalReactions = GetCount(connection, "Galdr_Environmental_Reactions"),
            TotalAbilityFlavorDescriptors = GetCount(connection, "Ability_Flavor_Descriptors"),
            DescriptorsByRuneSchool = GetGroupCount(connection, "Galdr_Action_Descriptors", "rune_school"),
            ReactionsByBiome = GetGroupCount(connection, "Galdr_Environmental_Reactions", "biome_name")
        };

        _galdrFlavorLog.Information("Galdr flavor text stats: {Actions} actions, {Manifestations} manifestations, {Outcomes} outcomes, {Miscasts} miscasts, {Voices} voices, {Reactions} reactions, {Abilities} abilities",
            stats.TotalGaldrActionDescriptors, stats.TotalManifestationDescriptors, stats.TotalOutcomeDescriptors,
            stats.TotalMiscastDescriptors, stats.TotalCasterVoiceProfiles, stats.TotalEnvironmentalReactions,
            stats.TotalAbilityFlavorDescriptors);

        return stats;
    }

    #endregion
}

/// <summary>
/// Statistics about the Galdr flavor text library.
/// </summary>
public class GaldrFlavorTextStats
{
    public int TotalGaldrActionDescriptors { get; set; }
    public int TotalManifestationDescriptors { get; set; }
    public int TotalOutcomeDescriptors { get; set; }
    public int TotalMiscastDescriptors { get; set; }
    public int TotalCasterVoiceProfiles { get; set; }
    public int TotalEnvironmentalReactions { get; set; }
    public int TotalAbilityFlavorDescriptors { get; set; }
    public Dictionary<string, int> DescriptorsByRuneSchool { get; set; } = new();
    public Dictionary<string, int> ReactionsByBiome { get; set; } = new();
}
