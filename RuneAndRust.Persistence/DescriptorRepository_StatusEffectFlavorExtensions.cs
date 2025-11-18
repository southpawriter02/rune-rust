using Microsoft.Data.Sqlite;
using RuneAndRust.Core.StatusEffectFlavor;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38.8: Status effect flavor text extensions for DescriptorRepository
/// Provides database access to status effect descriptors, severity profiles, and interaction descriptors
/// </summary>
public partial class DescriptorRepository
{
    private static readonly ILogger _statusEffectFlavorLog = Log.ForContext<DescriptorRepository>();

    #region Status Effect Descriptors

    /// <summary>
    /// Gets status effect descriptors with optional filtering
    /// </summary>
    public List<StatusEffectDescriptor> GetStatusEffectDescriptors(
        string? effectType = null,
        string? applicationContext = null,
        string? severity = null,
        string? sourceType = null,
        string? targetType = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, effect_type, application_context, severity,
                source_type, source_detail, target_type, descriptor_text,
                weight, is_active, tags
            FROM Status_Effect_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(effectType))
        {
            sql += " AND effect_type = $effectType";
            command.Parameters.AddWithValue("$effectType", effectType);
        }

        if (!string.IsNullOrEmpty(applicationContext))
        {
            sql += " AND application_context = $applicationContext";
            command.Parameters.AddWithValue("$applicationContext", applicationContext);
        }

        if (!string.IsNullOrEmpty(severity))
        {
            sql += " AND (severity = $severity OR severity IS NULL)";
            command.Parameters.AddWithValue("$severity", severity);
        }

        if (!string.IsNullOrEmpty(sourceType))
        {
            sql += " AND (source_type = $sourceType OR source_type IS NULL)";
            command.Parameters.AddWithValue("$sourceType", sourceType);
        }

        if (!string.IsNullOrEmpty(targetType))
        {
            sql += " AND (target_type = $targetType OR target_type IS NULL)";
            command.Parameters.AddWithValue("$targetType", targetType);
        }

        command.CommandText = sql;

        var descriptors = new List<StatusEffectDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapStatusEffectDescriptor(reader));
        }

        _statusEffectFlavorLog.Debug(
            "Loaded {Count} status effect descriptors (Effect: {Effect}, Context: {Context}, Severity: {Severity})",
            descriptors.Count, effectType, applicationContext, severity);

        return descriptors;
    }

    /// <summary>
    /// Gets a status effect descriptor by ID
    /// </summary>
    public StatusEffectDescriptor? GetStatusEffectDescriptorById(int descriptorId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                descriptor_id, effect_type, application_context, severity,
                source_type, source_detail, target_type, descriptor_text,
                weight, is_active, tags
            FROM Status_Effect_Descriptors
            WHERE descriptor_id = $descriptorId
        ";
        command.Parameters.AddWithValue("$descriptorId", descriptorId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapStatusEffectDescriptor(reader);
        }

        _statusEffectFlavorLog.Warning("Status effect descriptor not found: ID {DescriptorId}", descriptorId);
        return null;
    }

    /// <summary>
    /// Gets a random status effect descriptor matching the criteria
    /// </summary>
    public StatusEffectDescriptor? GetRandomStatusEffectDescriptor(
        string effectType,
        string applicationContext,
        string? severity = null,
        string? sourceType = null,
        string? targetType = null)
    {
        var descriptors = GetStatusEffectDescriptors(
            effectType, applicationContext, severity, sourceType, targetType);

        if (descriptors.Count == 0)
        {
            // Fallback: try without source_type filter
            if (!string.IsNullOrEmpty(sourceType))
            {
                descriptors = GetStatusEffectDescriptors(
                    effectType, applicationContext, severity, null, targetType);
            }
        }

        if (descriptors.Count == 0)
        {
            _statusEffectFlavorLog.Warning(
                "No status effect descriptors found for: {Effect} {Context} {Severity}",
                effectType, applicationContext, severity);
            return null;
        }

        // Weighted random selection
        var totalWeight = descriptors.Sum(d => d.Weight);
        var random = new Random().NextDouble() * totalWeight;
        var cumulativeWeight = 0.0;

        foreach (var descriptor in descriptors)
        {
            cumulativeWeight += descriptor.Weight;
            if (random <= cumulativeWeight)
            {
                return descriptor;
            }
        }

        return descriptors.Last(); // Fallback
    }

    private StatusEffectDescriptor MapStatusEffectDescriptor(SqliteDataReader reader)
    {
        return new StatusEffectDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            EffectType = reader.GetString(1),
            ApplicationContext = reader.GetString(2),
            Severity = reader.IsDBNull(3) ? null : reader.GetString(3),
            SourceType = reader.IsDBNull(4) ? null : reader.GetString(4),
            SourceDetail = reader.IsDBNull(5) ? null : reader.GetString(5),
            TargetType = reader.IsDBNull(6) ? null : reader.GetString(6),
            DescriptorText = reader.GetString(7),
            Weight = (float)reader.GetDouble(8),
            IsActive = reader.GetInt32(9) == 1,
            Tags = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }

    #endregion

    #region Status Effect Severity Profiles

    /// <summary>
    /// Gets severity profiles for a specific effect type
    /// </summary>
    public List<StatusEffectSeverityProfile> GetStatusEffectSeverityProfiles(string? effectType = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                profile_id, effect_type, severity,
                damage_per_turn_min, damage_per_turn_max,
                stack_count_min, stack_count_max,
                duration_min, duration_max,
                intensity_description, urgency_level,
                is_active
            FROM Status_Effect_Severity_Profiles
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(effectType))
        {
            sql += " AND effect_type = $effectType";
            command.Parameters.AddWithValue("$effectType", effectType);
        }

        sql += " ORDER BY effect_type, severity";
        command.CommandText = sql;

        var profiles = new List<StatusEffectSeverityProfile>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            profiles.Add(MapStatusEffectSeverityProfile(reader));
        }

        _statusEffectFlavorLog.Debug("Loaded {Count} severity profiles for effect {Effect}",
            profiles.Count, effectType ?? "all");

        return profiles;
    }

    /// <summary>
    /// Determines severity level based on damage amount
    /// </summary>
    public string? DetermineSeverityByDamage(string effectType, int damagePerTurn)
    {
        var profiles = GetStatusEffectSeverityProfiles(effectType);

        foreach (var profile in profiles)
        {
            if (profile.DamagePerTurnMin.HasValue && profile.DamagePerTurnMax.HasValue)
            {
                if (damagePerTurn >= profile.DamagePerTurnMin.Value &&
                    damagePerTurn <= profile.DamagePerTurnMax.Value)
                {
                    return profile.Severity;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Determines severity level based on stack count
    /// </summary>
    public string? DetermineSeverityByStacks(string effectType, int stackCount)
    {
        var profiles = GetStatusEffectSeverityProfiles(effectType);

        foreach (var profile in profiles)
        {
            if (profile.StackCountMin.HasValue && profile.StackCountMax.HasValue)
            {
                if (stackCount >= profile.StackCountMin.Value &&
                    stackCount <= profile.StackCountMax.Value)
                {
                    return profile.Severity;
                }
            }
        }

        return null;
    }

    private StatusEffectSeverityProfile MapStatusEffectSeverityProfile(SqliteDataReader reader)
    {
        return new StatusEffectSeverityProfile
        {
            ProfileId = reader.GetInt32(0),
            EffectType = reader.GetString(1),
            Severity = reader.GetString(2),
            DamagePerTurnMin = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            DamagePerTurnMax = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            StackCountMin = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            StackCountMax = reader.IsDBNull(6) ? null : reader.GetInt32(6),
            DurationMin = reader.IsDBNull(7) ? null : reader.GetInt32(7),
            DurationMax = reader.IsDBNull(8) ? null : reader.GetInt32(8),
            IntensityDescription = reader.IsDBNull(9) ? null : reader.GetString(9),
            UrgencyLevel = reader.IsDBNull(10) ? null : reader.GetString(10),
            IsActive = reader.GetInt32(11) == 1
        };
    }

    #endregion

    #region Status Effect Interaction Descriptors

    /// <summary>
    /// Gets interaction descriptors for two status effects
    /// </summary>
    public List<StatusEffectInteractionDescriptor> GetStatusEffectInteractionDescriptors(
        string? effectType1 = null,
        string? effectType2 = null,
        string? interactionType = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                interaction_id, effect_type_1, effect_type_2,
                interaction_type, result_effect, descriptor_text,
                weight, is_active, tags
            FROM Status_Effect_Interaction_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(effectType1))
        {
            sql += " AND effect_type_1 = $effectType1";
            command.Parameters.AddWithValue("$effectType1", effectType1);
        }

        if (!string.IsNullOrEmpty(effectType2))
        {
            sql += " AND effect_type_2 = $effectType2";
            command.Parameters.AddWithValue("$effectType2", effectType2);
        }

        if (!string.IsNullOrEmpty(interactionType))
        {
            sql += " AND interaction_type = $interactionType";
            command.Parameters.AddWithValue("$interactionType", interactionType);
        }

        command.CommandText = sql;

        var descriptors = new List<StatusEffectInteractionDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapStatusEffectInteractionDescriptor(reader));
        }

        _statusEffectFlavorLog.Debug("Loaded {Count} interaction descriptors", descriptors.Count);

        return descriptors;
    }

    /// <summary>
    /// Gets a random interaction descriptor
    /// </summary>
    public StatusEffectInteractionDescriptor? GetRandomInteractionDescriptor(
        string effectType1,
        string effectType2,
        string? interactionType = null)
    {
        var descriptors = GetStatusEffectInteractionDescriptors(effectType1, effectType2, interactionType);

        if (descriptors.Count == 0)
            return null;

        // Weighted random selection
        var totalWeight = descriptors.Sum(d => d.Weight);
        var random = new Random().NextDouble() * totalWeight;
        var cumulativeWeight = 0.0;

        foreach (var descriptor in descriptors)
        {
            cumulativeWeight += descriptor.Weight;
            if (random <= cumulativeWeight)
            {
                return descriptor;
            }
        }

        return descriptors.Last();
    }

    private StatusEffectInteractionDescriptor MapStatusEffectInteractionDescriptor(SqliteDataReader reader)
    {
        return new StatusEffectInteractionDescriptor
        {
            InteractionId = reader.GetInt32(0),
            EffectType1 = reader.GetString(1),
            EffectType2 = reader.GetString(2),
            InteractionType = reader.GetString(3),
            ResultEffect = reader.IsDBNull(4) ? null : reader.GetString(4),
            DescriptorText = reader.GetString(5),
            Weight = (float)reader.GetDouble(6),
            IsActive = reader.GetInt32(7) == 1,
            Tags = reader.IsDBNull(8) ? null : reader.GetString(8)
        };
    }

    #endregion

    #region Status Effect Source Modifiers

    /// <summary>
    /// Gets source-specific modifiers for status effects
    /// </summary>
    public List<StatusEffectSourceModifier> GetStatusEffectSourceModifiers(
        string? effectType = null,
        string? sourceType = null,
        string? biomeName = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                modifier_id, effect_type, source_type,
                enemy_archetype, weapon_type, environmental_hazard,
                biome_name, modifier_prefix, modifier_suffix,
                replacement_text, weight, is_active, tags
            FROM Status_Effect_Source_Modifiers
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(effectType))
        {
            sql += " AND effect_type = $effectType";
            command.Parameters.AddWithValue("$effectType", effectType);
        }

        if (!string.IsNullOrEmpty(sourceType))
        {
            sql += " AND source_type = $sourceType";
            command.Parameters.AddWithValue("$sourceType", sourceType);
        }

        if (!string.IsNullOrEmpty(biomeName))
        {
            sql += " AND (biome_name = $biomeName OR biome_name IS NULL)";
            command.Parameters.AddWithValue("$biomeName", biomeName);
        }

        command.CommandText = sql;

        var modifiers = new List<StatusEffectSourceModifier>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            modifiers.Add(MapStatusEffectSourceModifier(reader));
        }

        _statusEffectFlavorLog.Debug("Loaded {Count} source modifiers", modifiers.Count);

        return modifiers;
    }

    private StatusEffectSourceModifier MapStatusEffectSourceModifier(SqliteDataReader reader)
    {
        return new StatusEffectSourceModifier
        {
            ModifierId = reader.GetInt32(0),
            EffectType = reader.GetString(1),
            SourceType = reader.GetString(2),
            EnemyArchetype = reader.IsDBNull(3) ? null : reader.GetString(3),
            WeaponType = reader.IsDBNull(4) ? null : reader.GetString(4),
            EnvironmentalHazard = reader.IsDBNull(5) ? null : reader.GetString(5),
            BiomeName = reader.IsDBNull(6) ? null : reader.GetString(6),
            ModifierPrefix = reader.IsDBNull(7) ? null : reader.GetString(7),
            ModifierSuffix = reader.IsDBNull(8) ? null : reader.GetString(8),
            ReplacementText = reader.IsDBNull(9) ? null : reader.GetString(9),
            Weight = (float)reader.GetDouble(10),
            IsActive = reader.GetInt32(11) == 1,
            Tags = reader.IsDBNull(12) ? null : reader.GetString(12)
        };
    }

    #endregion

    #region Status Effect Environmental Context

    /// <summary>
    /// Gets environmental context descriptors for status effects
    /// </summary>
    public List<StatusEffectEnvironmentalContext> GetStatusEffectEnvironmentalContext(
        string? effectType = null,
        string? biomeName = null,
        string? applicationContext = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                context_id, effect_type, biome_name, application_context,
                environmental_descriptor, duration_modifier, damage_modifier,
                trigger_chance, weight, is_active, tags
            FROM Status_Effect_Environmental_Context
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(effectType))
        {
            sql += " AND effect_type = $effectType";
            command.Parameters.AddWithValue("$effectType", effectType);
        }

        if (!string.IsNullOrEmpty(biomeName))
        {
            sql += " AND biome_name = $biomeName";
            command.Parameters.AddWithValue("$biomeName", biomeName);
        }

        if (!string.IsNullOrEmpty(applicationContext))
        {
            sql += " AND (application_context = $applicationContext OR application_context IS NULL)";
            command.Parameters.AddWithValue("$applicationContext", applicationContext);
        }

        command.CommandText = sql;

        var contexts = new List<StatusEffectEnvironmentalContext>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            contexts.Add(MapStatusEffectEnvironmentalContext(reader));
        }

        _statusEffectFlavorLog.Debug("Loaded {Count} environmental contexts", contexts.Count);

        return contexts;
    }

    /// <summary>
    /// Gets a random environmental context descriptor with trigger chance
    /// </summary>
    public StatusEffectEnvironmentalContext? GetRandomEnvironmentalContext(
        string effectType,
        string biomeName,
        string? applicationContext = null)
    {
        var contexts = GetStatusEffectEnvironmentalContext(effectType, biomeName, applicationContext);

        if (contexts.Count == 0)
            return null;

        // Check trigger chance
        var random = new Random();
        var selectedContext = contexts[random.Next(contexts.Count)];

        if (random.NextDouble() <= selectedContext.TriggerChance)
        {
            return selectedContext;
        }

        return null; // Trigger chance failed
    }

    private StatusEffectEnvironmentalContext MapStatusEffectEnvironmentalContext(SqliteDataReader reader)
    {
        return new StatusEffectEnvironmentalContext
        {
            ContextId = reader.GetInt32(0),
            EffectType = reader.GetString(1),
            BiomeName = reader.GetString(2),
            ApplicationContext = reader.IsDBNull(3) ? null : reader.GetString(3),
            EnvironmentalDescriptor = reader.GetString(4),
            DurationModifier = reader.IsDBNull(5) ? null : (float?)reader.GetDouble(5),
            DamageModifier = reader.IsDBNull(6) ? null : (float?)reader.GetDouble(6),
            TriggerChance = (float)reader.GetDouble(7),
            Weight = (float)reader.GetDouble(8),
            IsActive = reader.GetInt32(9) == 1,
            Tags = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets statistics about the status effect flavor text library
    /// </summary>
    public StatusEffectFlavorTextStats GetStatusEffectFlavorTextStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var stats = new StatusEffectFlavorTextStats
        {
            TotalStatusEffectDescriptors = GetCount(connection, "Status_Effect_Descriptors"),
            TotalSeverityProfiles = GetCount(connection, "Status_Effect_Severity_Profiles"),
            TotalInteractionDescriptors = GetCount(connection, "Status_Effect_Interaction_Descriptors"),
            TotalSourceModifiers = GetCount(connection, "Status_Effect_Source_Modifiers"),
            TotalEnvironmentalContexts = GetCount(connection, "Status_Effect_Environmental_Context"),
            DescriptorsByEffectType = GetGroupCount(connection, "Status_Effect_Descriptors", "effect_type"),
            DescriptorsByContext = GetGroupCount(connection, "Status_Effect_Descriptors", "application_context")
        };

        _statusEffectFlavorLog.Information(
            "Status effect flavor text stats: {Descriptors} descriptors, {Profiles} profiles, {Interactions} interactions",
            stats.TotalStatusEffectDescriptors, stats.TotalSeverityProfiles, stats.TotalInteractionDescriptors);

        return stats;
    }

    #endregion
}

/// <summary>
/// Statistics about the status effect flavor text library
/// </summary>
public class StatusEffectFlavorTextStats
{
    public int TotalStatusEffectDescriptors { get; set; }
    public int TotalSeverityProfiles { get; set; }
    public int TotalInteractionDescriptors { get; set; }
    public int TotalSourceModifiers { get; set; }
    public int TotalEnvironmentalContexts { get; set; }
    public Dictionary<string, int> DescriptorsByEffectType { get; set; } = new();
    public Dictionary<string, int> DescriptorsByContext { get; set; } = new();
}
