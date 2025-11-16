using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.31.2: Data repository for Alfheim biome.
/// Provides database-driven access to room templates, enemies, hazards, and resources.
/// </summary>
public class AlfheimDataRepository
{
    private static readonly ILogger _log = Log.ForContext<AlfheimDataRepository>();
    private readonly string _connectionString;
    private const int ALFHEIM_BIOME_ID = 6;

    public AlfheimDataRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("AlfheimDataRepository initialized");
    }

    #region Room Templates

    /// <summary>
    /// Get all room templates for Alfheim biome (all Canopy-exclusive)
    /// </summary>
    public List<AlfheimRoomTemplate> GetRoomTemplates()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                template_id,
                template_name,
                template_description,
                room_size_category,
                min_connections,
                max_connections,
                can_be_entrance,
                can_be_exit,
                can_be_hub,
                hazard_density,
                enemy_spawn_weight,
                resource_spawn_chance,
                wfc_adjacency_rules,
                z_level
            FROM Biome_RoomTemplates
            WHERE biome_id = $biomeId
        ";

        command.Parameters.AddWithValue("$biomeId", ALFHEIM_BIOME_ID);

        var templates = new List<AlfheimRoomTemplate>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            templates.Add(new AlfheimRoomTemplate
            {
                TemplateId = reader.GetInt32(0),
                TemplateName = reader.GetString(1),
                Description = reader.GetString(2),
                RoomSize = reader.GetString(3),
                MinConnections = reader.GetInt32(4),
                MaxConnections = reader.GetInt32(5),
                CanBeEntrance = reader.GetInt32(6) == 1,
                CanBeExit = reader.GetInt32(7) == 1,
                CanBeHub = reader.GetInt32(8) == 1,
                HazardDensity = reader.GetString(9),
                EnemySpawnWeight = reader.GetInt32(10),
                ResourceSpawnChance = reader.GetDouble(11),
                WfcAdjacencyRules = reader.GetString(12),
                VerticalityTier = reader.GetString(13)
            });
        }

        _log.Information("Loaded {Count} room templates for Alfheim (all Canopy)", templates.Count);
        return templates;
    }

    /// <summary>
    /// Get room templates that can be entrance rooms
    /// </summary>
    public List<AlfheimRoomTemplate> GetEntranceTemplates()
    {
        return GetRoomTemplates().Where(t => t.CanBeEntrance).ToList();
    }

    /// <summary>
    /// Get room templates that can be exit rooms
    /// </summary>
    public List<AlfheimRoomTemplate> GetExitTemplates()
    {
        return GetRoomTemplates().Where(t => t.CanBeExit).ToList();
    }

    /// <summary>
    /// Get boss room template (All-Rune Proving Ground)
    /// </summary>
    public AlfheimRoomTemplate? GetBossRoomTemplate()
    {
        return GetRoomTemplates().FirstOrDefault(t => t.TemplateName == "All-Rune Proving Ground");
    }

    #endregion

    #region Environmental Hazards

    /// <summary>
    /// Get all environmental hazards for Alfheim
    /// </summary>
    public List<AlfheimHazard> GetEnvironmentalHazards(string? hazardDensity = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                feature_id,
                feature_name,
                feature_type,
                feature_description,
                damage_per_turn,
                damage_type,
                tile_coverage_percent,
                is_destructible,
                blocks_movement,
                blocks_line_of_sight,
                hazard_density_category,
                special_rules
            FROM Biome_EnvironmentalFeatures
            WHERE biome_id = $biomeId
        ";

        if (!string.IsNullOrEmpty(hazardDensity))
        {
            command.CommandText += @"
                AND (
                    hazard_density_category = $density
                    OR hazard_density_category = 'Low'
                    OR hazard_density_category = 'None'
                    OR (hazard_density_category = 'Medium' AND $density IN ('Medium', 'High', 'Extreme'))
                    OR (hazard_density_category = 'High' AND $density IN ('High', 'Extreme'))
                )
            ";
            command.Parameters.AddWithValue("$density", hazardDensity);
        }

        command.Parameters.AddWithValue("$biomeId", ALFHEIM_BIOME_ID);

        var hazards = new List<AlfheimHazard>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            hazards.Add(new AlfheimHazard
            {
                FeatureId = reader.GetInt32(0),
                FeatureName = reader.GetString(1),
                FeatureType = reader.GetString(2),
                Description = reader.GetString(3),
                DamagePerTurn = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                DamageType = reader.IsDBNull(5) ? null : reader.GetString(5),
                TileCoveragePercent = reader.GetDouble(6),
                IsDestructible = reader.GetInt32(7) == 1,
                BlocksMovement = reader.GetInt32(8) == 1,
                BlocksLineOfSight = reader.GetInt32(9) == 1,
                HazardDensity = reader.GetString(10),
                SpecialRules = reader.GetString(11)
            });
        }

        _log.Information("Loaded {Count} environmental hazards for Alfheim (density: {Density})",
            hazards.Count, hazardDensity ?? "All");
        return hazards;
    }

    /// <summary>
    /// Get Reality Tear hazard specifically
    /// </summary>
    public AlfheimHazard? GetRealityTear()
    {
        return GetEnvironmentalHazards().FirstOrDefault(h => h.FeatureName == "Reality Tear");
    }

    /// <summary>
    /// Get Energy Conduit hazards (interactive)
    /// </summary>
    public List<AlfheimHazard> GetEnergyConduits()
    {
        return GetEnvironmentalHazards().Where(h => h.FeatureName == "Energy Conduit").ToList();
    }

    /// <summary>
    /// Get Crystalline Spire cover objects
    /// </summary>
    public List<AlfheimHazard> GetCrystallineSpires()
    {
        return GetEnvironmentalHazards().Where(h => h.FeatureName == "Crystalline Spire").ToList();
    }

    #endregion

    #region Resources

    /// <summary>
    /// Get all resource drops for Alfheim
    /// </summary>
    public List<AlfheimResource> GetResourceDrops(int? minTier = null, int? maxTier = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                resource_drop_id,
                resource_name,
                resource_description,
                resource_tier,
                rarity,
                base_drop_chance,
                min_quantity,
                max_quantity,
                requires_special_node,
                weight
            FROM Biome_ResourceDrops
            WHERE biome_id = $biomeId
        ";

        if (minTier.HasValue)
        {
            command.CommandText += " AND resource_tier >= $minTier";
            command.Parameters.AddWithValue("$minTier", minTier.Value);
        }

        if (maxTier.HasValue)
        {
            command.CommandText += " AND resource_tier <= $maxTier";
            command.Parameters.AddWithValue("$maxTier", maxTier.Value);
        }

        command.Parameters.AddWithValue("$biomeId", ALFHEIM_BIOME_ID);

        var resources = new List<AlfheimResource>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            resources.Add(new AlfheimResource
            {
                ResourceDropId = reader.GetInt32(0),
                ResourceName = reader.GetString(1),
                Description = reader.GetString(2),
                ResourceTier = reader.GetInt32(3),
                Rarity = reader.GetString(4),
                BaseDropChance = reader.GetDouble(5),
                MinQuantity = reader.GetInt32(6),
                MaxQuantity = reader.GetInt32(7),
                RequiresSpecialNode = reader.GetInt32(8) == 1,
                Weight = reader.GetInt32(9)
            });
        }

        _log.Information("Loaded {Count} resource drops for Alfheim (tier: {MinTier}-{MaxTier})",
            resources.Count, minTier, maxTier);
        return resources;
    }

    /// <summary>
    /// Get legendary resources (tier 5)
    /// </summary>
    public List<AlfheimResource> GetLegendaryResources()
    {
        return GetResourceDrops(minTier: 5, maxTier: 5);
    }

    /// <summary>
    /// Get Fragment of the All-Rune (boss drop)
    /// </summary>
    public AlfheimResource? GetAllRuneFragment()
    {
        return GetLegendaryResources().FirstOrDefault(r => r.ResourceName == "Fragment of the All-Rune");
    }

    #endregion

    #region Conditions

    /// <summary>
    /// Get Runic Instability condition data
    /// </summary>
    public RunicInstabilityCondition? GetRunicInstabilityCondition()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                c.condition_id,
                c.condition_name,
                c.description
            FROM Conditions c
            WHERE c.condition_id = 107
        ";

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var condition = new RunicInstabilityCondition
            {
                ConditionId = reader.GetInt32(0),
                ConditionName = reader.GetString(1),
                Description = reader.GetString(2)
            };

            _log.Debug("Loaded Runic Instability condition (id: {Id})", condition.ConditionId);
            return condition;
        }

        _log.Warning("Runic Instability condition not found in database");
        return null;
    }

    /// <summary>
    /// Get Runic Instability effect values
    /// </summary>
    public Dictionary<string, int> GetRunicInstabilityEffects()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                effect_type,
                effect_value
            FROM Condition_Effects
            WHERE condition_id = 107
        ";

        var effects = new Dictionary<string, int>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var effectType = reader.GetString(0);
            var effectValue = reader.GetInt32(1);
            effects[effectType] = effectValue;
        }

        _log.Debug("Loaded {Count} Runic Instability effects", effects.Count);
        return effects;
    }

    #endregion
}

#region Data Transfer Objects

public class AlfheimRoomTemplate
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RoomSize { get; set; } = string.Empty;
    public int MinConnections { get; set; }
    public int MaxConnections { get; set; }
    public bool CanBeEntrance { get; set; }
    public bool CanBeExit { get; set; }
    public bool CanBeHub { get; set; }
    public string HazardDensity { get; set; } = string.Empty;
    public int EnemySpawnWeight { get; set; }
    public double ResourceSpawnChance { get; set; }
    public string WfcAdjacencyRules { get; set; } = string.Empty;
    public string VerticalityTier { get; set; } = string.Empty; // Always "Canopy"
}

public class AlfheimHazard
{
    public int FeatureId { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public string FeatureType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DamagePerTurn { get; set; }
    public string? DamageType { get; set; }
    public double TileCoveragePercent { get; set; }
    public bool IsDestructible { get; set; }
    public bool BlocksMovement { get; set; }
    public bool BlocksLineOfSight { get; set; }
    public string HazardDensity { get; set; } = string.Empty;
    public string SpecialRules { get; set; } = string.Empty;
}

public class AlfheimResource
{
    public int ResourceDropId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ResourceTier { get; set; }
    public string Rarity { get; set; } = string.Empty;
    public double BaseDropChance { get; set; }
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public bool RequiresSpecialNode { get; set; }
    public int Weight { get; set; }
}

public class RunicInstabilityCondition
{
    public int ConditionId { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

#endregion
