using Microsoft.Data.Sqlite;
using RuneAndRust.Core.NPCFlavor;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38.11: NPC Descriptors & Dialogue Barks extensions for DescriptorRepository
/// Provides database access to NPC physical descriptors, ambient barks, and reaction descriptors
/// </summary>
public partial class DescriptorRepository
{
    private static readonly ILogger _npcFlavorLog = Log.ForContext<DescriptorRepository>();

    #region NPC Physical Descriptors

    /// <summary>
    /// Gets NPC physical descriptors with optional filtering
    /// </summary>
    public List<NPCPhysicalDescriptor> GetNPCPhysicalDescriptors(
        string? npcArchetype = null,
        string? npcSubtype = null,
        string? descriptorType = null,
        string? condition = null,
        string? biomeContext = null,
        string? ageCategory = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, npc_archetype, npc_subtype, descriptor_type,
                condition, biome_context, age_category,
                descriptor_text, weight, is_active, tags
            FROM NPC_Physical_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(npcArchetype))
        {
            sql += " AND npc_archetype = $npcArchetype";
            command.Parameters.AddWithValue("$npcArchetype", npcArchetype);
        }

        if (!string.IsNullOrEmpty(npcSubtype))
        {
            sql += " AND npc_subtype = $npcSubtype";
            command.Parameters.AddWithValue("$npcSubtype", npcSubtype);
        }

        if (!string.IsNullOrEmpty(descriptorType))
        {
            sql += " AND descriptor_type = $descriptorType";
            command.Parameters.AddWithValue("$descriptorType", descriptorType);
        }

        if (!string.IsNullOrEmpty(condition))
        {
            sql += " AND (condition = $condition OR condition IS NULL)";
            command.Parameters.AddWithValue("$condition", condition);
        }

        if (!string.IsNullOrEmpty(biomeContext))
        {
            sql += " AND (biome_context = $biomeContext OR biome_context IS NULL)";
            command.Parameters.AddWithValue("$biomeContext", biomeContext);
        }

        if (!string.IsNullOrEmpty(ageCategory))
        {
            sql += " AND (age_category = $ageCategory OR age_category IS NULL)";
            command.Parameters.AddWithValue("$ageCategory", ageCategory);
        }

        command.CommandText = sql;

        var descriptors = new List<NPCPhysicalDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapNPCPhysicalDescriptor(reader));
        }

        _npcFlavorLog.Debug(
            "Loaded {Count} NPC physical descriptors (Archetype: {Archetype}, Subtype: {Subtype}, Type: {Type})",
            descriptors.Count, npcArchetype, npcSubtype, descriptorType);

        return descriptors;
    }

    /// <summary>
    /// Gets an NPC physical descriptor by ID
    /// </summary>
    public NPCPhysicalDescriptor? GetNPCPhysicalDescriptorById(int descriptorId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                descriptor_id, npc_archetype, npc_subtype, descriptor_type,
                condition, biome_context, age_category,
                descriptor_text, weight, is_active, tags
            FROM NPC_Physical_Descriptors
            WHERE descriptor_id = $descriptorId
        ";
        command.Parameters.AddWithValue("$descriptorId", descriptorId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapNPCPhysicalDescriptor(reader);
        }

        _npcFlavorLog.Warning("NPC physical descriptor not found: ID {DescriptorId}", descriptorId);
        return null;
    }

    /// <summary>
    /// Gets a random NPC physical descriptor matching the criteria
    /// </summary>
    public NPCPhysicalDescriptor? GetRandomNPCPhysicalDescriptor(
        string npcArchetype,
        string npcSubtype,
        string descriptorType,
        string? condition = null,
        string? biomeContext = null,
        string? ageCategory = null)
    {
        var descriptors = GetNPCPhysicalDescriptors(
            npcArchetype, npcSubtype, descriptorType, condition, biomeContext, ageCategory);

        if (descriptors.Count == 0)
        {
            // Fallback: try without condition
            if (!string.IsNullOrEmpty(condition))
            {
                descriptors = GetNPCPhysicalDescriptors(
                    npcArchetype, npcSubtype, descriptorType, null, biomeContext, ageCategory);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without biome context
            if (!string.IsNullOrEmpty(biomeContext))
            {
                descriptors = GetNPCPhysicalDescriptors(
                    npcArchetype, npcSubtype, descriptorType, condition, null, ageCategory);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without age category
            if (!string.IsNullOrEmpty(ageCategory))
            {
                descriptors = GetNPCPhysicalDescriptors(
                    npcArchetype, npcSubtype, descriptorType, condition, biomeContext, null);
            }
        }

        if (descriptors.Count == 0)
        {
            _npcFlavorLog.Warning(
                "No NPC physical descriptors found for: {Archetype} {Subtype} {Type}",
                npcArchetype, npcSubtype, descriptorType);
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

    private NPCPhysicalDescriptor MapNPCPhysicalDescriptor(SqliteDataReader reader)
    {
        return new NPCPhysicalDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            NPCArchetype = reader.GetString(1),
            NPCSubtype = reader.GetString(2),
            DescriptorType = reader.GetString(3),
            Condition = reader.IsDBNull(4) ? null : reader.GetString(4),
            BiomeContext = reader.IsDBNull(5) ? null : reader.GetString(5),
            AgeCategory = reader.IsDBNull(6) ? null : reader.GetString(6),
            DescriptorText = reader.GetString(7),
            Weight = (float)reader.GetDouble(8),
            IsActive = reader.GetInt32(9) == 1,
            Tags = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }

    #endregion

    #region NPC Ambient Bark Descriptors

    /// <summary>
    /// Gets NPC ambient bark descriptors with optional filtering
    /// </summary>
    public List<NPCAmbientBarkDescriptor> GetNPCAmbientBarkDescriptors(
        string? npcArchetype = null,
        string? npcSubtype = null,
        string? barkType = null,
        string? activityContext = null,
        string? dispositionContext = null,
        string? biomeContext = null,
        string? triggerCondition = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, npc_archetype, npc_subtype, bark_type,
                activity_context, disposition_context, biome_context, trigger_condition,
                dialogue_text, weight, is_active, tags
            FROM NPC_Ambient_Bark_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(npcArchetype))
        {
            sql += " AND npc_archetype = $npcArchetype";
            command.Parameters.AddWithValue("$npcArchetype", npcArchetype);
        }

        if (!string.IsNullOrEmpty(npcSubtype))
        {
            sql += " AND npc_subtype = $npcSubtype";
            command.Parameters.AddWithValue("$npcSubtype", npcSubtype);
        }

        if (!string.IsNullOrEmpty(barkType))
        {
            sql += " AND bark_type = $barkType";
            command.Parameters.AddWithValue("$barkType", barkType);
        }

        if (!string.IsNullOrEmpty(activityContext))
        {
            sql += " AND (activity_context = $activityContext OR activity_context IS NULL)";
            command.Parameters.AddWithValue("$activityContext", activityContext);
        }

        if (!string.IsNullOrEmpty(dispositionContext))
        {
            sql += " AND (disposition_context = $dispositionContext OR disposition_context IS NULL)";
            command.Parameters.AddWithValue("$dispositionContext", dispositionContext);
        }

        if (!string.IsNullOrEmpty(biomeContext))
        {
            sql += " AND (biome_context = $biomeContext OR biome_context IS NULL)";
            command.Parameters.AddWithValue("$biomeContext", biomeContext);
        }

        if (!string.IsNullOrEmpty(triggerCondition))
        {
            sql += " AND (trigger_condition = $triggerCondition OR trigger_condition IS NULL)";
            command.Parameters.AddWithValue("$triggerCondition", triggerCondition);
        }

        command.CommandText = sql;

        var descriptors = new List<NPCAmbientBarkDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapNPCAmbientBarkDescriptor(reader));
        }

        _npcFlavorLog.Debug(
            "Loaded {Count} NPC ambient bark descriptors (Archetype: {Archetype}, Subtype: {Subtype}, BarkType: {BarkType})",
            descriptors.Count, npcArchetype, npcSubtype, barkType);

        return descriptors;
    }

    /// <summary>
    /// Gets an NPC ambient bark descriptor by ID
    /// </summary>
    public NPCAmbientBarkDescriptor? GetNPCAmbientBarkDescriptorById(int descriptorId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                descriptor_id, npc_archetype, npc_subtype, bark_type,
                activity_context, disposition_context, biome_context, trigger_condition,
                dialogue_text, weight, is_active, tags
            FROM NPC_Ambient_Bark_Descriptors
            WHERE descriptor_id = $descriptorId
        ";
        command.Parameters.AddWithValue("$descriptorId", descriptorId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapNPCAmbientBarkDescriptor(reader);
        }

        _npcFlavorLog.Warning("NPC ambient bark descriptor not found: ID {DescriptorId}", descriptorId);
        return null;
    }

    /// <summary>
    /// Gets a random NPC ambient bark descriptor matching the criteria
    /// </summary>
    public NPCAmbientBarkDescriptor? GetRandomNPCAmbientBarkDescriptor(
        string npcArchetype,
        string npcSubtype,
        string barkType,
        string? activityContext = null,
        string? dispositionContext = null,
        string? biomeContext = null,
        string? triggerCondition = null)
    {
        var descriptors = GetNPCAmbientBarkDescriptors(
            npcArchetype, npcSubtype, barkType, activityContext, dispositionContext, biomeContext, triggerCondition);

        if (descriptors.Count == 0)
        {
            // Fallback: try without activity context
            if (!string.IsNullOrEmpty(activityContext))
            {
                descriptors = GetNPCAmbientBarkDescriptors(
                    npcArchetype, npcSubtype, barkType, null, dispositionContext, biomeContext, triggerCondition);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without disposition context
            if (!string.IsNullOrEmpty(dispositionContext))
            {
                descriptors = GetNPCAmbientBarkDescriptors(
                    npcArchetype, npcSubtype, barkType, activityContext, null, biomeContext, triggerCondition);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without biome context
            if (!string.IsNullOrEmpty(biomeContext))
            {
                descriptors = GetNPCAmbientBarkDescriptors(
                    npcArchetype, npcSubtype, barkType, activityContext, dispositionContext, null, triggerCondition);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without trigger condition
            if (!string.IsNullOrEmpty(triggerCondition))
            {
                descriptors = GetNPCAmbientBarkDescriptors(
                    npcArchetype, npcSubtype, barkType, activityContext, dispositionContext, biomeContext, null);
            }
        }

        if (descriptors.Count == 0)
        {
            _npcFlavorLog.Warning(
                "No NPC ambient bark descriptors found for: {Archetype} {Subtype} {BarkType}",
                npcArchetype, npcSubtype, barkType);
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

    private NPCAmbientBarkDescriptor MapNPCAmbientBarkDescriptor(SqliteDataReader reader)
    {
        return new NPCAmbientBarkDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            NPCArchetype = reader.GetString(1),
            NPCSubtype = reader.GetString(2),
            BarkType = reader.GetString(3),
            ActivityContext = reader.IsDBNull(4) ? null : reader.GetString(4),
            DispositionContext = reader.IsDBNull(5) ? null : reader.GetString(5),
            BiomeContext = reader.IsDBNull(6) ? null : reader.GetString(6),
            TriggerCondition = reader.IsDBNull(7) ? null : reader.GetString(7),
            DialogueText = reader.GetString(8),
            Weight = (float)reader.GetDouble(9),
            IsActive = reader.GetInt32(10) == 1,
            Tags = reader.IsDBNull(11) ? null : reader.GetString(11)
        };
    }

    #endregion

    #region NPC Reaction Descriptors

    /// <summary>
    /// Gets NPC reaction descriptors with optional filtering
    /// </summary>
    public List<NPCReactionDescriptor> GetNPCReactionDescriptors(
        string? npcArchetype = null,
        string? npcSubtype = null,
        string? reactionType = null,
        string? triggerEvent = null,
        string? intensity = null,
        string? priorDisposition = null,
        string? actionTendency = null,
        string? biomeContext = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = @"
            SELECT
                descriptor_id, npc_archetype, npc_subtype, reaction_type,
                trigger_event, intensity, prior_disposition, action_tendency,
                biome_context, reaction_text, weight, is_active, tags
            FROM NPC_Reaction_Descriptors
            WHERE is_active = 1
        ";

        if (!string.IsNullOrEmpty(npcArchetype))
        {
            sql += " AND npc_archetype = $npcArchetype";
            command.Parameters.AddWithValue("$npcArchetype", npcArchetype);
        }

        if (!string.IsNullOrEmpty(npcSubtype))
        {
            sql += " AND npc_subtype = $npcSubtype";
            command.Parameters.AddWithValue("$npcSubtype", npcSubtype);
        }

        if (!string.IsNullOrEmpty(reactionType))
        {
            sql += " AND reaction_type = $reactionType";
            command.Parameters.AddWithValue("$reactionType", reactionType);
        }

        if (!string.IsNullOrEmpty(triggerEvent))
        {
            sql += " AND trigger_event = $triggerEvent";
            command.Parameters.AddWithValue("$triggerEvent", triggerEvent);
        }

        if (!string.IsNullOrEmpty(intensity))
        {
            sql += " AND (intensity = $intensity OR intensity IS NULL)";
            command.Parameters.AddWithValue("$intensity", intensity);
        }

        if (!string.IsNullOrEmpty(priorDisposition))
        {
            sql += " AND (prior_disposition = $priorDisposition OR prior_disposition IS NULL)";
            command.Parameters.AddWithValue("$priorDisposition", priorDisposition);
        }

        if (!string.IsNullOrEmpty(actionTendency))
        {
            sql += " AND (action_tendency = $actionTendency OR action_tendency IS NULL)";
            command.Parameters.AddWithValue("$actionTendency", actionTendency);
        }

        if (!string.IsNullOrEmpty(biomeContext))
        {
            sql += " AND (biome_context = $biomeContext OR biome_context IS NULL)";
            command.Parameters.AddWithValue("$biomeContext", biomeContext);
        }

        command.CommandText = sql;

        var descriptors = new List<NPCReactionDescriptor>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            descriptors.Add(MapNPCReactionDescriptor(reader));
        }

        _npcFlavorLog.Debug(
            "Loaded {Count} NPC reaction descriptors (Archetype: {Archetype}, Subtype: {Subtype}, Reaction: {Reaction}, Trigger: {Trigger})",
            descriptors.Count, npcArchetype, npcSubtype, reactionType, triggerEvent);

        return descriptors;
    }

    /// <summary>
    /// Gets an NPC reaction descriptor by ID
    /// </summary>
    public NPCReactionDescriptor? GetNPCReactionDescriptorById(int descriptorId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                descriptor_id, npc_archetype, npc_subtype, reaction_type,
                trigger_event, intensity, prior_disposition, action_tendency,
                biome_context, reaction_text, weight, is_active, tags
            FROM NPC_Reaction_Descriptors
            WHERE descriptor_id = $descriptorId
        ";
        command.Parameters.AddWithValue("$descriptorId", descriptorId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapNPCReactionDescriptor(reader);
        }

        _npcFlavorLog.Warning("NPC reaction descriptor not found: ID {DescriptorId}", descriptorId);
        return null;
    }

    /// <summary>
    /// Gets a random NPC reaction descriptor matching the criteria
    /// </summary>
    public NPCReactionDescriptor? GetRandomNPCReactionDescriptor(
        string npcArchetype,
        string npcSubtype,
        string reactionType,
        string triggerEvent,
        string? intensity = null,
        string? priorDisposition = null,
        string? actionTendency = null,
        string? biomeContext = null)
    {
        var descriptors = GetNPCReactionDescriptors(
            npcArchetype, npcSubtype, reactionType, triggerEvent, intensity, priorDisposition, actionTendency, biomeContext);

        if (descriptors.Count == 0)
        {
            // Fallback: try without intensity
            if (!string.IsNullOrEmpty(intensity))
            {
                descriptors = GetNPCReactionDescriptors(
                    npcArchetype, npcSubtype, reactionType, triggerEvent, null, priorDisposition, actionTendency, biomeContext);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without prior disposition
            if (!string.IsNullOrEmpty(priorDisposition))
            {
                descriptors = GetNPCReactionDescriptors(
                    npcArchetype, npcSubtype, reactionType, triggerEvent, intensity, null, actionTendency, biomeContext);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without action tendency
            if (!string.IsNullOrEmpty(actionTendency))
            {
                descriptors = GetNPCReactionDescriptors(
                    npcArchetype, npcSubtype, reactionType, triggerEvent, intensity, priorDisposition, null, biomeContext);
            }
        }

        if (descriptors.Count == 0)
        {
            // Fallback: try without biome context
            if (!string.IsNullOrEmpty(biomeContext))
            {
                descriptors = GetNPCReactionDescriptors(
                    npcArchetype, npcSubtype, reactionType, triggerEvent, intensity, priorDisposition, actionTendency, null);
            }
        }

        if (descriptors.Count == 0)
        {
            _npcFlavorLog.Warning(
                "No NPC reaction descriptors found for: {Archetype} {Subtype} {Reaction} {Trigger}",
                npcArchetype, npcSubtype, reactionType, triggerEvent);
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

    private NPCReactionDescriptor MapNPCReactionDescriptor(SqliteDataReader reader)
    {
        return new NPCReactionDescriptor
        {
            DescriptorId = reader.GetInt32(0),
            NPCArchetype = reader.GetString(1),
            NPCSubtype = reader.GetString(2),
            ReactionType = reader.GetString(3),
            TriggerEvent = reader.GetString(4),
            Intensity = reader.IsDBNull(5) ? null : reader.GetString(5),
            PriorDisposition = reader.IsDBNull(6) ? null : reader.GetString(6),
            ActionTendency = reader.IsDBNull(7) ? null : reader.GetString(7),
            BiomeContext = reader.IsDBNull(8) ? null : reader.GetString(8),
            ReactionText = reader.GetString(9),
            Weight = (float)reader.GetDouble(10),
            IsActive = reader.GetInt32(11) == 1,
            Tags = reader.IsDBNull(12) ? null : reader.GetString(12)
        };
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets statistics about the NPC flavor text library
    /// </summary>
    public NPCFlavorTextStats GetNPCFlavorTextStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var stats = new NPCFlavorTextStats
        {
            TotalPhysicalDescriptors = GetCount(connection, "NPC_Physical_Descriptors"),
            TotalAmbientBarkDescriptors = GetCount(connection, "NPC_Ambient_Bark_Descriptors"),
            TotalReactionDescriptors = GetCount(connection, "NPC_Reaction_Descriptors"),
            DescriptorsByArchetype = GetGroupCount(connection, "NPC_Physical_Descriptors", "npc_archetype"),
            BarksByType = GetGroupCount(connection, "NPC_Ambient_Bark_Descriptors", "bark_type"),
            ReactionsByType = GetGroupCount(connection, "NPC_Reaction_Descriptors", "reaction_type")
        };

        _npcFlavorLog.Information(
            "NPC flavor text stats: {Physical} physical descriptors, {Barks} ambient barks, {Reactions} reaction descriptors",
            stats.TotalPhysicalDescriptors, stats.TotalAmbientBarkDescriptors, stats.TotalReactionDescriptors);

        return stats;
    }

    #endregion
}

/// <summary>
/// Statistics about the NPC flavor text library
/// </summary>
public class NPCFlavorTextStats
{
    public int TotalPhysicalDescriptors { get; set; }
    public int TotalAmbientBarkDescriptors { get; set; }
    public int TotalReactionDescriptors { get; set; }
    public Dictionary<string, int> DescriptorsByArchetype { get; set; } = new();
    public Dictionary<string, int> BarksByType { get; set; } = new();
    public Dictionary<string, int> ReactionsByType { get; set; } = new();
}
