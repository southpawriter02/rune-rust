using Microsoft.Data.Sqlite;
using RuneAndRust.Core.SkillUsageFlavor;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38.10: Skill Usage Flavor Text extensions for DescriptorRepository
/// Provides database access to skill check descriptors and fumble descriptors
/// </summary>
public partial class DescriptorRepository
{
    private static readonly ILogger _skillUsageFlavorLog = Log.ForContext<DescriptorRepository>();

    #region Skill Check Descriptors

    /// <summary>
    /// Gets skill check descriptors with optional filtering
    /// </summary>
    public List<SkillCheckDescriptor> GetSkillCheckDescriptors(
        string? skillType = null,
        string? actionType = null,
        string? checkPhase = null,
        string? resultDegree = null,
        string? environmentalContext = null,
        string? biomeContext = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, skill_type, action_type, check_phase,
                result_degree, environmental_context, biome_context,
                descriptor_text, weight, is_active, tags
            FROM Skill_Check_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(skillType))
        {
            sql += " AND skill_type = $skillType";
            command.Parameters.AddWithValue("$skillType", skillType);
        }

        if (!string.IsNullOrEmpty(actionType))
        {
            sql += " AND action_type = $actionType";
            command.Parameters.AddWithValue("$actionType", actionType);
        }

        if (!string.IsNullOrEmpty(checkPhase))
        {
            sql += " AND check_phase = $checkPhase";
            command.Parameters.AddWithValue("$checkPhase", checkPhase);
        }

        if (!string.IsNullOrEmpty(resultDegree))
        {
            sql += " AND (result_degree = $resultDegree OR result_degree IS NULL)";
            command.Parameters.AddWithValue("$resultDegree", resultDegree);
        }

        if (!string.IsNullOrEmpty(environmentalContext))
        {
            sql += " AND (environmental_context = $environmentalContext OR environmental_context IS NULL)";
            command.Parameters.AddWithValue("$environmentalContext", environmentalContext);
        }

        if (!string.IsNullOrEmpty(biomeContext))
        {
            sql += " AND (biome_context = $biomeContext OR biome_context IS NULL)";
            command.Parameters.AddWithValue("$biomeContext", biomeContext);
        }

        command.CommandText = sql;

        var descriptors = new List<SkillCheckDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapSkillCheckDescriptor(reader));
        }

        _skillUsageFlavorLog.Debug(
            "Loaded {Count} skill check descriptors (Skill: {Skill}, Action: {Action}, Phase: {Phase}, Degree: {Degree})",
            descriptors.Count, skillType, actionType, checkPhase, resultDegree);

        return descriptors;
    }

    /// <summary>
    /// Gets a skill check descriptor by ID
    /// </summary>
    public SkillCheckDescriptor? GetSkillCheckDescriptorById(int descriptorId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                descriptor_id, skill_type, action_type, check_phase,
                result_degree, environmental_context, biome_context,
                descriptor_text, weight, is_active, tags
            FROM Skill_Check_Descriptors
            WHERE descriptor_id = $descriptorId
        ";
        command.Parameters.AddWithValue("$descriptorId", descriptorId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapSkillCheckDescriptor(reader);
        }

        _skillUsageFlavorLog.Warning("Skill check descriptor not found: ID {DescriptorId}", descriptorId);
        return null;
    }

    /// <summary>
    /// Gets a random skill check descriptor matching the criteria
    /// </summary>
    public SkillCheckDescriptor? GetRandomSkillCheckDescriptor(
        string skillType,
        string actionType,
        string checkPhase,
        string? resultDegree = null,
        string? environmentalContext = null,
        string? biomeContext = null)
    {
        var descriptors = GetSkillCheckDescriptors(
            skillType, actionType, checkPhase, resultDegree, environmentalContext, biomeContext);

        if (descriptors.Count == 0)
        {
            // Fallback: try without environmental context
            if (!string.IsNullOrEmpty(environmentalContext))
            {
                descriptors = GetSkillCheckDescriptors(
                    skillType, actionType, checkPhase, resultDegree, null, biomeContext);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without biome context
            if (!string.IsNullOrEmpty(biomeContext))
            {
                descriptors = GetSkillCheckDescriptors(
                    skillType, actionType, checkPhase, resultDegree, environmentalContext, null);
            }
        }

        if (descriptors.Count == 0)
        {
            _skillUsageFlavorLog.Warning(
                "No skill check descriptors found for: {Skill} {Action} {Phase} {Degree}",
                skillType, actionType, checkPhase, resultDegree);
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

    private SkillCheckDescriptor MapSkillCheckDescriptor(SqliteDataReader reader)
    {
        return new SkillCheckDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            SkillType = reader.GetString(1),
            ActionType = reader.GetString(2),
            CheckPhase = reader.GetString(3),
            ResultDegree = reader.IsDBNull(4) ? null : reader.GetString(4),
            EnvironmentalContext = reader.IsDBNull(5) ? null : reader.GetString(5),
            BiomeContext = reader.IsDBNull(6) ? null : reader.GetString(6),
            DescriptorText = reader.GetString(7),
            Weight = (float)reader.GetDouble(8),
            IsActive = reader.GetInt32(9) == 1,
            Tags = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }

    #endregion

    #region Skill Fumble Descriptors

    /// <summary>
    /// Gets skill fumble descriptors with optional filtering
    /// </summary>
    public List<SkillFumbleDescriptor> GetSkillFumbleDescriptors(
        string? skillType = null,
        string? actionType = null,
        string? consequenceType = null,
        string? severity = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                fumble_id, skill_type, action_type, consequence_type,
                severity, damage_formula, status_effect_applied,
                next_attempt_dc_modifier, time_penalty_minutes, prevents_retry,
                descriptor_text, weight, is_active, tags
            FROM Skill_Fumble_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(skillType))
        {
            sql += " AND skill_type = $skillType";
            command.Parameters.AddWithValue("$skillType", skillType);
        }

        if (!string.IsNullOrEmpty(actionType))
        {
            sql += " AND action_type = $actionType";
            command.Parameters.AddWithValue("$actionType", actionType);
        }

        if (!string.IsNullOrEmpty(consequenceType))
        {
            sql += " AND consequence_type = $consequenceType";
            command.Parameters.AddWithValue("$consequenceType", consequenceType);
        }

        if (!string.IsNullOrEmpty(severity))
        {
            sql += " AND severity = $severity";
            command.Parameters.AddWithValue("$severity", severity);
        }

        command.CommandText = sql;

        var fumbles = new List<SkillFumbleDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            fumbles.Add(MapSkillFumbleDescriptor(reader));
        }

        _skillUsageFlavorLog.Debug(
            "Loaded {Count} skill fumble descriptors (Skill: {Skill}, Action: {Action}, Type: {Type})",
            fumbles.Count, skillType, actionType, consequenceType);

        return fumbles;
    }

    /// <summary>
    /// Gets a skill fumble descriptor by ID
    /// </summary>
    public SkillFumbleDescriptor? GetSkillFumbleDescriptorById(int fumbleId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                fumble_id, skill_type, action_type, consequence_type,
                severity, damage_formula, status_effect_applied,
                next_attempt_dc_modifier, time_penalty_minutes, prevents_retry,
                descriptor_text, weight, is_active, tags
            FROM Skill_Fumble_Descriptors
            WHERE fumble_id = $fumbleId
        ";
        command.Parameters.AddWithValue("$fumbleId", fumbleId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapSkillFumbleDescriptor(reader);
        }

        _skillUsageFlavorLog.Warning("Skill fumble descriptor not found: ID {FumbleId}", fumbleId);
        return null;
    }

    /// <summary>
    /// Gets a random fumble descriptor matching the criteria
    /// </summary>
    public SkillFumbleDescriptor? GetRandomSkillFumbleDescriptor(
        string skillType,
        string actionType,
        string? consequenceType = null,
        string? severity = null)
    {
        var fumbles = GetSkillFumbleDescriptors(skillType, actionType, consequenceType, severity);

        if (fumbles.Count == 0)
        {
            _skillUsageFlavorLog.Warning(
                "No fumble descriptors found for: {Skill} {Action} {Type}",
                skillType, actionType, consequenceType);
            return null;
        }

        // Weighted random selection
        var totalWeight = fumbles.Sum(f => f.Weight);
        var random = new Random().NextDouble() * totalWeight;
        var cumulativeWeight = 0.0;

        foreach (var fumble in fumbles)
        {
            cumulativeWeight += fumble.Weight;
            if (random <= cumulativeWeight)
            {
                return fumble;
            }
        }

        return fumbles.Last(); // Fallback
    }

    private SkillFumbleDescriptor MapSkillFumbleDescriptor(SqliteDataReader reader)
    {
        return new SkillFumbleDescriptor
        {
            FumbleId = reader.GetInt32(0),
            SkillType = reader.GetString(1),
            ActionType = reader.GetString(2),
            ConsequenceType = reader.GetString(3),
            Severity = reader.GetString(4),
            DamageFormula = reader.IsDBNull(5) ? null : reader.GetString(5),
            StatusEffectApplied = reader.IsDBNull(6) ? null : reader.GetString(6),
            NextAttemptDCModifier = reader.IsDBNull(7) ? null : reader.GetInt32(7),
            TimePenaltyMinutes = reader.IsDBNull(8) ? null : reader.GetInt32(8),
            PreventsRetry = reader.GetInt32(9) == 1,
            DescriptorText = reader.GetString(10),
            Weight = (float)reader.GetDouble(11),
            IsActive = reader.GetInt32(12) == 1,
            Tags = reader.IsDBNull(13) ? null : reader.GetString(13)
        };
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets statistics about the skill usage flavor text library
    /// </summary>
    public SkillUsageFlavorTextStats GetSkillUsageFlavorTextStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var stats = new SkillUsageFlavorTextStats
        {
            TotalSkillCheckDescriptors = GetCount(connection, "Skill_Check_Descriptors"),
            TotalFumbleDescriptors = GetCount(connection, "Skill_Fumble_Descriptors"),
            DescriptorsBySkillType = GetGroupCount(connection, "Skill_Check_Descriptors", "skill_type"),
            DescriptorsByActionType = GetGroupCount(connection, "Skill_Check_Descriptors", "action_type"),
            DescriptorsByCheckPhase = GetGroupCount(connection, "Skill_Check_Descriptors", "check_phase"),
            FumblesByConsequenceType = GetGroupCount(connection, "Skill_Fumble_Descriptors", "consequence_type")
        };

        _skillUsageFlavorLog.Information(
            "Skill usage flavor text stats: {SkillCheck} skill check descriptors, {Fumbles} fumble descriptors",
            stats.TotalSkillCheckDescriptors, stats.TotalFumbleDescriptors);

        return stats;
    }

    #endregion
}

/// <summary>
/// Statistics about the skill usage flavor text library
/// </summary>
public class SkillUsageFlavorTextStats
{
    public int TotalSkillCheckDescriptors { get; set; }
    public int TotalFumbleDescriptors { get; set; }
    public Dictionary<string, int> DescriptorsBySkillType { get; set; } = new();
    public Dictionary<string, int> DescriptorsByActionType { get; set; } = new();
    public Dictionary<string, int> DescriptorsByCheckPhase { get; set; } = new();
    public Dictionary<string, int> FumblesByConsequenceType { get; set; } = new();
}
