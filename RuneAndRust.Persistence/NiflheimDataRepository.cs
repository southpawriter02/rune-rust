using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.30.4: Data repository for Niflheim biome.
/// Provides database-driven access to room templates, enemies, hazards, and resources.
/// </summary>
public class NiflheimDataRepository
{
    private static readonly ILogger _log = Log.ForContext<NiflheimDataRepository>();
    private readonly string _connectionString;
    private const int NIFLHEIM_BIOME_ID = 5;

    public NiflheimDataRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("NiflheimDataRepository initialized");
    }

    #region Room Templates

    /// <summary>
    /// Get all room templates for Niflheim biome
    /// </summary>
    public List<NiflheimRoomTemplate> GetRoomTemplates(string? verticalityTier = null)
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

        // Filter by verticality tier if specified
        if (!string.IsNullOrEmpty(verticalityTier))
        {
            command.CommandText += " AND z_level = $verticalityTier";
            command.Parameters.AddWithValue("$verticalityTier", verticalityTier);
        }

        command.Parameters.AddWithValue("$biomeId", NIFLHEIM_BIOME_ID);

        var templates = new List<NiflheimRoomTemplate>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            templates.Add(new NiflheimRoomTemplate
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

        _log.Information("Loaded {Count} room templates for Niflheim (tier: {Tier})",
            templates.Count, verticalityTier ?? "All");
        return templates;
    }

    /// <summary>
    /// Get room templates that can be entrance rooms
    /// </summary>
    public List<NiflheimRoomTemplate> GetEntranceTemplates(string? verticalityTier = null)
    {
        return GetRoomTemplates(verticalityTier).Where(t => t.CanBeEntrance).ToList();
    }

    /// <summary>
    /// Get room templates that can be exit rooms
    /// </summary>
    public List<NiflheimRoomTemplate> GetExitTemplates(string? verticalityTier = null)
    {
        return GetRoomTemplates(verticalityTier).Where(t => t.CanBeExit).ToList();
    }

    #endregion

    #region Enemy Spawns

    /// <summary>
    /// Get all enemy spawn definitions for Niflheim
    /// </summary>
    public List<NiflheimEnemySpawn> GetEnemySpawns(int? minLevel = null, int? maxLevel = null, string? verticalityTier = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                spawn_id,
                enemy_name,
                enemy_type,
                min_level,
                max_level,
                spawn_weight,
                spawn_rules_json,
                verticality_tier
            FROM Biome_EnemySpawns
            WHERE biome_id = $biomeId
        ";

        if (minLevel.HasValue)
        {
            command.CommandText += " AND max_level >= $minLevel";
            command.Parameters.AddWithValue("$minLevel", minLevel.Value);
        }

        if (maxLevel.HasValue)
        {
            command.CommandText += " AND min_level <= $maxLevel";
            command.Parameters.AddWithValue("$maxLevel", maxLevel.Value);
        }

        if (!string.IsNullOrEmpty(verticalityTier))
        {
            command.CommandText += " AND (verticality_tier = $verticalityTier OR verticality_tier = 'Both')";
            command.Parameters.AddWithValue("$verticalityTier", verticalityTier);
        }

        command.Parameters.AddWithValue("$biomeId", NIFLHEIM_BIOME_ID);

        var spawns = new List<NiflheimEnemySpawn>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            spawns.Add(new NiflheimEnemySpawn
            {
                SpawnId = reader.GetInt32(0),
                EnemyName = reader.GetString(1),
                EnemyType = reader.GetString(2),
                MinLevel = reader.GetInt32(3),
                MaxLevel = reader.GetInt32(4),
                SpawnWeight = reader.GetInt32(5),
                SpawnRulesJson = reader.GetString(6),
                VerticalityTier = reader.GetString(7)
            });
        }

        _log.Information("Loaded {Count} enemy spawns for Niflheim (level: {MinLevel}-{MaxLevel}, tier: {Tier})",
            spawns.Count, minLevel, maxLevel, verticalityTier ?? "All");
        return spawns;
    }

    /// <summary>
    /// Get boss enemy spawn (Frost-Giant)
    /// </summary>
    public NiflheimEnemySpawn? GetBossSpawn()
    {
        return GetEnemySpawns().FirstOrDefault(e => e.EnemyType == "Boss");
    }

    /// <summary>
    /// Get resistances from spawn rules JSON
    /// </summary>
    public Dictionary<string, int> GetEnemyResistances(string spawnRulesJson)
    {
        try
        {
            var rules = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(spawnRulesJson);
            var resistances = new Dictionary<string, int>();

            if (rules != null)
            {
                // Check for resistances object first (v0.30.3 structure)
                if (rules.ContainsKey("resistances") && rules["resistances"].ValueKind == JsonValueKind.Object)
                {
                    var resistancesObj = rules["resistances"];
                    foreach (var prop in resistancesObj.EnumerateObject())
                    {
                        resistances[prop.Name] = prop.Value.GetInt32();
                    }
                }
                // Fallback to flat structure (backward compatibility)
                else
                {
                    if (rules.ContainsKey("ice_resistance"))
                    {
                        resistances["Ice"] = rules["ice_resistance"].GetInt32();
                    }
                    if (rules.ContainsKey("fire_resistance"))
                    {
                        resistances["Fire"] = rules["fire_resistance"].GetInt32();
                    }
                    if (rules.ContainsKey("physical_resistance"))
                    {
                        resistances["Physical"] = rules["physical_resistance"].GetInt32();
                    }
                    if (rules.ContainsKey("psychic_resistance"))
                    {
                        resistances["Psychic"] = rules["Psychic"].GetInt32();
                    }
                }
            }

            return resistances;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to parse spawn rules JSON: {Json}", spawnRulesJson);
            return new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Get tags from spawn rules JSON
    /// </summary>
    public List<string> GetEnemyTags(string spawnRulesJson)
    {
        try
        {
            var rules = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(spawnRulesJson);
            var tags = new List<string>();

            if (rules != null && rules.ContainsKey("tags"))
            {
                var tagsElement = rules["tags"];
                if (tagsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tag in tagsElement.EnumerateArray())
                    {
                        tags.Add(tag.GetString() ?? "");
                    }
                }
            }

            return tags;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to parse spawn rules JSON for tags: {Json}", spawnRulesJson);
            return new List<string>();
        }
    }

    #endregion

    #region Environmental Hazards

    /// <summary>
    /// Get all environmental hazards for Niflheim
    /// </summary>
    public List<NiflheimHazard> GetEnvironmentalHazards(string? hazardDensity = null)
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

        command.Parameters.AddWithValue("$biomeId", NIFLHEIM_BIOME_ID);

        var hazards = new List<NiflheimHazard>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            hazards.Add(new NiflheimHazard
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

        _log.Information("Loaded {Count} environmental hazards for Niflheim (density: {Density})",
            hazards.Count, hazardDensity ?? "All");
        return hazards;
    }

    /// <summary>
    /// Get slippery terrain feature specifically
    /// </summary>
    public NiflheimHazard? GetSlipperyTerrain()
    {
        return GetEnvironmentalHazards().FirstOrDefault(h => h.FeatureName == "Slippery Terrain");
    }

    #endregion

    #region Resources

    /// <summary>
    /// Get all resource drops for Niflheim
    /// </summary>
    public List<NiflheimResource> GetResourceDrops(int? minTier = null, int? maxTier = null)
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

        command.Parameters.AddWithValue("$biomeId", NIFLHEIM_BIOME_ID);

        var resources = new List<NiflheimResource>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            resources.Add(new NiflheimResource
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

        _log.Information("Loaded {Count} resource drops for Niflheim (tier: {MinTier}-{MaxTier})",
            resources.Count, minTier, maxTier);
        return resources;
    }

    /// <summary>
    /// Get legendary resources (tier 5)
    /// </summary>
    public List<NiflheimResource> GetLegendaryResources()
    {
        return GetResourceDrops(minTier: 5, maxTier: 5);
    }

    #endregion
}
