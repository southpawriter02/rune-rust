using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.32.1: Data repository for Jötunheim biome.
/// Provides database-driven access to room templates, enemies, hazards, and resources.
/// </summary>
public class JotunheimDataRepository
{
    private static readonly ILogger _log = Log.ForContext<JotunheimDataRepository>();
    private readonly string _connectionString;
    private const int JOTUNHEIM_BIOME_ID = 7;

    public JotunheimDataRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("JotunheimDataRepository initialized");
    }

    #region Room Templates

    /// <summary>
    /// Get all room templates for Jötunheim biome (Trunk and Roots)
    /// </summary>
    public List<JotunheimRoomTemplate> GetRoomTemplates(string? verticalityTier = null)
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

        if (!string.IsNullOrEmpty(verticalityTier))
        {
            command.CommandText += " AND z_level = $verticalityTier";
            command.Parameters.AddWithValue("$verticalityTier", verticalityTier);
        }

        command.Parameters.AddWithValue("$biomeId", JOTUNHEIM_BIOME_ID);

        var templates = new List<JotunheimRoomTemplate>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            templates.Add(new JotunheimRoomTemplate
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

        _log.Information("Loaded {Count} room templates for Jötunheim (tier: {Tier})",
            templates.Count, verticalityTier ?? "All");
        return templates;
    }

    /// <summary>
    /// Get room templates for Trunk (factory floor level) - 70% of spawn weight
    /// </summary>
    public List<JotunheimRoomTemplate> GetTrunkTemplates()
    {
        return GetRoomTemplates("Trunk");
    }

    /// <summary>
    /// Get room templates for Roots (maintenance/utility level) - 30% of spawn weight
    /// </summary>
    public List<JotunheimRoomTemplate> GetRootsTemplates()
    {
        return GetRoomTemplates("Roots");
    }

    /// <summary>
    /// Get room templates that can be entrance rooms
    /// </summary>
    public List<JotunheimRoomTemplate> GetEntranceTemplates()
    {
        return GetRoomTemplates().Where(t => t.CanBeEntrance).ToList();
    }

    /// <summary>
    /// Get room templates that can be exit rooms
    /// </summary>
    public List<JotunheimRoomTemplate> GetExitTemplates()
    {
        return GetRoomTemplates().Where(t => t.CanBeExit).ToList();
    }

    /// <summary>
    /// Get special Fallen Einherjar Torso-Cave room template (inside dormant Jötun-Forged)
    /// </summary>
    public JotunheimRoomTemplate? GetFallenEinherjarTemplate()
    {
        return GetRoomTemplates().FirstOrDefault(t => t.TemplateName == "Fallen Einherjar Torso-Cave");
    }

    /// <summary>
    /// Get Power Distribution Core template (deepest, most dangerous Roots room)
    /// </summary>
    public JotunheimRoomTemplate? GetPowerDistributionCore()
    {
        return GetRoomTemplates("Roots").FirstOrDefault(t => t.TemplateName == "Power Distribution Core");
    }

    /// <summary>
    /// Get verticality distribution for room generation (70% Trunk, 30% Roots)
    /// </summary>
    public Dictionary<string, int> GetVerticalityWeights()
    {
        var trunkWeight = GetTrunkTemplates().Sum(t => t.EnemySpawnWeight);
        var rootsWeight = GetRootsTemplates().Sum(t => t.EnemySpawnWeight);

        _log.Debug("Jötunheim verticality weights - Trunk: {TrunkWeight}, Roots: {RootsWeight}",
            trunkWeight, rootsWeight);

        return new Dictionary<string, int>
        {
            { "Trunk", trunkWeight },
            { "Roots", rootsWeight }
        };
    }

    #endregion

    #region Resources

    /// <summary>
    /// Get all resource drops for Jötunheim (mechanical components focus)
    /// </summary>
    public List<JotunheimResource> GetResourceDrops(int? minTier = null, int? maxTier = null)
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

        command.Parameters.AddWithValue("$biomeId", JOTUNHEIM_BIOME_ID);

        var resources = new List<JotunheimResource>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            resources.Add(new JotunheimResource
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

        _log.Information("Loaded {Count} resource drops for Jötunheim (tier: {MinTier}-{MaxTier})",
            resources.Count, minTier, maxTier);
        return resources;
    }

    /// <summary>
    /// Get common mechanical components (Tier 1-2)
    /// </summary>
    public List<JotunheimResource> GetCommonComponents()
    {
        return GetResourceDrops(minTier: 1, maxTier: 2);
    }

    /// <summary>
    /// Get advanced components (Tier 3)
    /// </summary>
    public List<JotunheimResource> GetAdvancedComponents()
    {
        return GetResourceDrops(minTier: 3, maxTier: 3);
    }

    /// <summary>
    /// Get legendary resources (Tier 4)
    /// </summary>
    public List<JotunheimResource> GetLegendaryResources()
    {
        return GetResourceDrops(minTier: 4, maxTier: 4);
    }

    /// <summary>
    /// Get Unblemished Jötun Plating (signature Tier 3 resource)
    /// </summary>
    public JotunheimResource? GetUnblemishedPlating()
    {
        return GetResourceDrops().FirstOrDefault(r => r.ResourceName == "Unblemished Jotun Plating");
    }

    /// <summary>
    /// Get Uncorrupted Power Coil (legendary drop from Power Distribution Core)
    /// </summary>
    public JotunheimResource? GetUncorruptedPowerCoil()
    {
        return GetLegendaryResources().FirstOrDefault(r => r.ResourceName == "Uncorrupted Power Coil");
    }

    /// <summary>
    /// Get Jötun Logic Core Fragment (ultra-rare legendary)
    /// </summary>
    public JotunheimResource? GetLogicCoreFragment()
    {
        return GetLegendaryResources().FirstOrDefault(r => r.ResourceName == "Jotun Logic Core Fragment");
    }

    /// <summary>
    /// Get weighted resource table for loot generation
    /// </summary>
    public Dictionary<string, int> GetResourceWeights()
    {
        var resources = GetResourceDrops();
        return resources.ToDictionary(r => r.ResourceName, r => r.Weight);
    }

    #endregion

    #region Environmental Hazards (v0.32.2)

    /// <summary>
    /// Get all environmental hazards for Jötunheim
    /// </summary>
    public List<JotunheimHazard> GetEnvironmentalHazards(string? hazardDensity = null)
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

        command.Parameters.AddWithValue("$biomeId", JOTUNHEIM_BIOME_ID);

        var hazards = new List<JotunheimHazard>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            hazards.Add(new JotunheimHazard
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

        _log.Information("Loaded {Count} environmental hazards for Jötunheim (density: {Density})",
            hazards.Count, hazardDensity ?? "All");
        return hazards;
    }

    /// <summary>
    /// Get Live Power Conduit hazard (signature hazard)
    /// </summary>
    public JotunheimHazard? GetLivePowerConduit()
    {
        return GetEnvironmentalHazards().FirstOrDefault(h => h.FeatureName == "Live Power Conduit");
    }

    /// <summary>
    /// Get High-Pressure Steam Vent hazard
    /// </summary>
    public JotunheimHazard? GetSteamVent()
    {
        return GetEnvironmentalHazards().FirstOrDefault(h => h.FeatureName == "High-Pressure Steam Vent");
    }

    /// <summary>
    /// Get Unstable Ceiling/Wall hazard
    /// </summary>
    public JotunheimHazard? GetUnstableCeiling()
    {
        return GetEnvironmentalHazards().FirstOrDefault(h => h.FeatureName == "Unstable Ceiling/Wall");
    }

    /// <summary>
    /// Get Jötun Corpse Terrain (special terrain type)
    /// </summary>
    public JotunheimHazard? GetJotunCorpseTerrain()
    {
        return GetEnvironmentalHazards().FirstOrDefault(h => h.FeatureName == "Jotun Corpse Terrain");
    }

    /// <summary>
    /// Get Flooded (Coolant) terrain
    /// </summary>
    public JotunheimHazard? GetFloodedTerrain()
    {
        return GetEnvironmentalHazards().FirstOrDefault(h => h.FeatureName == "Flooded (Coolant)");
    }

    /// <summary>
    /// Get Cover (Industrial) objects
    /// </summary>
    public List<JotunheimHazard> GetIndustrialCover()
    {
        return GetEnvironmentalHazards().Where(h => h.FeatureName == "Cover (Industrial)").ToList();
    }

    #endregion

    #region Enemy Spawns (v0.32.3)

    /// <summary>
    /// Get all enemy spawns for Jötunheim biome
    /// </summary>
    public List<JotunheimEnemy> GetEnemySpawns(string? enemyType = null, string? verticalityTier = null)
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

        if (!string.IsNullOrEmpty(enemyType))
        {
            command.CommandText += " AND enemy_type LIKE $enemyType";
            command.Parameters.AddWithValue("$enemyType", $"%{enemyType}%");
        }

        if (!string.IsNullOrEmpty(verticalityTier))
        {
            command.CommandText += " AND (verticality_tier = $verticalityTier OR verticality_tier = 'Both')";
            command.Parameters.AddWithValue("$verticalityTier", verticalityTier);
        }

        command.Parameters.AddWithValue("$biomeId", JOTUNHEIM_BIOME_ID);

        var enemies = new List<JotunheimEnemy>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            enemies.Add(new JotunheimEnemy
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

        _log.Information("Loaded {Count} enemy spawns for Jötunheim (type: {Type}, tier: {Tier})",
            enemies.Count, enemyType ?? "All", verticalityTier ?? "All");
        return enemies;
    }

    /// <summary>
    /// Get all Undying enemy spawns (~60% of spawns)
    /// </summary>
    public List<JotunheimEnemy> GetUndeadSpawns()
    {
        return GetEnemySpawns(enemyType: "Undying");
    }

    /// <summary>
    /// Get all Humanoid enemy spawns (~29% of spawns)
    /// </summary>
    public List<JotunheimEnemy> GetHumanoidSpawns()
    {
        return GetEnemySpawns(enemyType: "Humanoid");
    }

    /// <summary>
    /// Get all Beast enemy spawns (~12% of spawns)
    /// </summary>
    public List<JotunheimEnemy> GetBeastSpawns()
    {
        return GetEnemySpawns(enemyType: "Beast");
    }

    /// <summary>
    /// Get Rusted Servitor (most common enemy - ~29% spawn weight)
    /// </summary>
    public JotunheimEnemy? GetRustedServitor()
    {
        return GetEnemySpawns().FirstOrDefault(e => e.EnemyName == "Rusted Servitor");
    }

    /// <summary>
    /// Get Rusted Warden (common Undying - ~22% spawn weight)
    /// </summary>
    public JotunheimEnemy? GetRustedWarden()
    {
        return GetEnemySpawns().FirstOrDefault(e => e.EnemyName == "Rusted Warden");
    }

    /// <summary>
    /// Get Draugr Juggernaut (rare elite teaching enemy - Physical Soak 10)
    /// TEACHING MECHANIC: Requires armor-shredding or high damage attacks
    /// </summary>
    public JotunheimEnemy? GetDraugrJuggernaut()
    {
        return GetEnemySpawns().FirstOrDefault(e => e.EnemyName == "Draugr Juggernaut");
    }

    /// <summary>
    /// Get God-Sleeper Cultist (cargo cult fanatic - ~19% spawn weight)
    /// </summary>
    public JotunheimEnemy? GetGodSleeperCultist()
    {
        return GetEnemySpawns().FirstOrDefault(e => e.EnemyName == "God-Sleeper Cultist");
    }

    /// <summary>
    /// Get Scrap-Tinker (uncommon scavenger - ~10% spawn weight)
    /// </summary>
    public JotunheimEnemy? GetScrapTinker()
    {
        return GetEnemySpawns().FirstOrDefault(e => e.EnemyName == "Scrap-Tinker");
    }

    /// <summary>
    /// Get Iron-Husked Boar (mutated beast - ~12% spawn weight)
    /// </summary>
    public JotunheimEnemy? GetIronHuskedBoar()
    {
        return GetEnemySpawns().FirstOrDefault(e => e.EnemyName == "Iron-Husked Boar");
    }

    /// <summary>
    /// Get weighted enemy spawn table for procedural generation
    /// </summary>
    public Dictionary<string, int> GetEnemySpawnWeights(string? verticalityTier = null)
    {
        var enemies = GetEnemySpawns(verticalityTier: verticalityTier);
        var weights = enemies.ToDictionary(e => e.EnemyName, e => e.SpawnWeight);

        _log.Debug("Enemy spawn weights for {Tier}: {Weights}",
            verticalityTier ?? "All tiers", string.Join(", ", weights.Select(kvp => $"{kvp.Key}:{kvp.Value}")));

        return weights;
    }

    /// <summary>
    /// Get enemy type distribution statistics
    /// </summary>
    public JotunheimEnemyDistribution GetEnemyDistribution()
    {
        var allEnemies = GetEnemySpawns();
        var totalWeight = allEnemies.Sum(e => e.SpawnWeight);

        var distribution = new JotunheimEnemyDistribution
        {
            TotalEnemyTypes = allEnemies.Count,
            TotalSpawnWeight = totalWeight,
            UndeadWeight = GetUndeadSpawns().Sum(e => e.SpawnWeight),
            HumanoidWeight = GetHumanoidSpawns().Sum(e => e.SpawnWeight),
            BeastWeight = GetBeastSpawns().Sum(e => e.SpawnWeight)
        };

        distribution.UndeadPercentage = (distribution.UndeadWeight * 100.0) / totalWeight;
        distribution.HumanoidPercentage = (distribution.HumanoidWeight * 100.0) / totalWeight;
        distribution.BeastPercentage = (distribution.BeastWeight * 100.0) / totalWeight;

        _log.Information("Jötunheim enemy distribution - Undying: {Undead:F1}%, Humanoid: {Humanoid:F1}%, Beast: {Beast:F1}%",
            distribution.UndeadPercentage, distribution.HumanoidPercentage, distribution.BeastPercentage);

        return distribution;
    }

    #endregion

    #region Biome Info

    /// <summary>
    /// Get biome metadata for Jötunheim
    /// </summary>
    public JotunheimBiomeInfo GetBiomeInfo()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                biome_id,
                biome_name,
                biome_description,
                z_level_restriction,
                ambient_condition_id,
                min_character_level,
                max_character_level,
                is_active
            FROM Biomes
            WHERE biome_id = $biomeId
        ";

        command.Parameters.AddWithValue("$biomeId", JOTUNHEIM_BIOME_ID);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var info = new JotunheimBiomeInfo
            {
                BiomeId = reader.GetInt32(0),
                BiomeName = reader.GetString(1),
                Description = reader.GetString(2),
                ZLevelRestriction = reader.GetString(3),
                AmbientConditionId = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                MinCharacterLevel = reader.GetInt32(5),
                MaxCharacterLevel = reader.GetInt32(6),
                IsActive = reader.GetInt32(7) == 1
            };

            _log.Debug("Loaded Jötunheim biome info: {Name}, no ambient condition: {NoAmbient}",
                info.BiomeName, info.AmbientConditionId == null);

            return info;
        }

        _log.Error("Jötunheim biome not found in database (biome_id: {BiomeId})", JOTUNHEIM_BIOME_ID);
        throw new InvalidOperationException($"Jötunheim biome not found (biome_id: {JOTUNHEIM_BIOME_ID})");
    }

    #endregion

    #region Database Integrity Validation

    /// <summary>
    /// Validate database integrity for Jötunheim biome
    /// </summary>
    public JotunheimIntegrityReport ValidateDatabaseIntegrity()
    {
        var report = new JotunheimIntegrityReport();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check biome exists
        var biomeCheck = connection.CreateCommand();
        biomeCheck.CommandText = "SELECT COUNT(*) FROM Biomes WHERE biome_id = $biomeId";
        biomeCheck.Parameters.AddWithValue("$biomeId", JOTUNHEIM_BIOME_ID);
        report.BiomeExists = Convert.ToInt32(biomeCheck.ExecuteScalar()) == 1;

        // Check room template count
        var roomCheck = connection.CreateCommand();
        roomCheck.CommandText = "SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = $biomeId";
        roomCheck.Parameters.AddWithValue("$biomeId", JOTUNHEIM_BIOME_ID);
        report.RoomTemplateCount = Convert.ToInt32(roomCheck.ExecuteScalar());
        report.ExpectedRoomTemplateCount = 10;

        // Check resource drop count
        var resourceCheck = connection.CreateCommand();
        resourceCheck.CommandText = "SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = $biomeId";
        resourceCheck.Parameters.AddWithValue("$biomeId", JOTUNHEIM_BIOME_ID);
        report.ResourceDropCount = Convert.ToInt32(resourceCheck.ExecuteScalar());
        report.ExpectedResourceDropCount = 10;

        // Check verticality distribution
        var verticalityCheck = connection.CreateCommand();
        verticalityCheck.CommandText = @"
            SELECT z_level, COUNT(*), SUM(enemy_spawn_weight)
            FROM Biome_RoomTemplates
            WHERE biome_id = $biomeId
            GROUP BY z_level
        ";
        verticalityCheck.Parameters.AddWithValue("$biomeId", JOTUNHEIM_BIOME_ID);

        using var vertReader = verticalityCheck.ExecuteReader();
        while (vertReader.Read())
        {
            var tier = vertReader.GetString(0);
            var count = vertReader.GetInt32(1);
            var weight = vertReader.GetInt32(2);

            if (tier == "Trunk")
            {
                report.TrunkTemplateCount = count;
                report.TrunkSpawnWeight = weight;
            }
            else if (tier == "Roots")
            {
                report.RootsTemplateCount = count;
                report.RootsSpawnWeight = weight;
            }
        }

        report.ExpectedTrunkTemplateCount = 7;
        report.ExpectedRootsTemplateCount = 3;

        // Calculate integrity score
        report.IsValid = report.BiomeExists
            && report.RoomTemplateCount == report.ExpectedRoomTemplateCount
            && report.ResourceDropCount == report.ExpectedResourceDropCount
            && report.TrunkTemplateCount == report.ExpectedTrunkTemplateCount
            && report.RootsTemplateCount == report.ExpectedRootsTemplateCount;

        _log.Information("Jötunheim database integrity: {IsValid}", report.IsValid);
        _log.Debug("Integrity report: {@Report}", report);

        return report;
    }

    #endregion
}

#region Data Transfer Objects

public class JotunheimRoomTemplate
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
    public string VerticalityTier { get; set; } = string.Empty; // "Trunk" or "Roots"
}

public class JotunheimResource
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

public class JotunheimHazard
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

public class JotunheimBiomeInfo
{
    public int BiomeId { get; set; }
    public string BiomeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ZLevelRestriction { get; set; } = string.Empty;
    public int? AmbientConditionId { get; set; }
    public int MinCharacterLevel { get; set; }
    public int MaxCharacterLevel { get; set; }
    public bool IsActive { get; set; }
}

public class JotunheimEnemy
{
    public int SpawnId { get; set; }
    public string EnemyName { get; set; } = string.Empty;
    public string EnemyType { get; set; } = string.Empty;
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public int SpawnWeight { get; set; }
    public string SpawnRulesJson { get; set; } = string.Empty;
    public string VerticalityTier { get; set; } = string.Empty; // "Trunk", "Roots", or "Both"

    /// <summary>
    /// Deserialize spawn rules JSON to typed object
    /// </summary>
    public T? GetSpawnRules<T>() where T : class
    {
        if (string.IsNullOrEmpty(SpawnRulesJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(SpawnRulesJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

public class JotunheimEnemyDistribution
{
    public int TotalEnemyTypes { get; set; }
    public int TotalSpawnWeight { get; set; }
    public int UndeadWeight { get; set; }
    public int HumanoidWeight { get; set; }
    public int BeastWeight { get; set; }
    public double UndeadPercentage { get; set; }
    public double HumanoidPercentage { get; set; }
    public double BeastPercentage { get; set; }

    public override string ToString()
    {
        return $"Enemy Distribution:\n" +
               $"  Total Types: {TotalEnemyTypes} (weight: {TotalSpawnWeight})\n" +
               $"  Undying: {UndeadPercentage:F1}% (weight: {UndeadWeight})\n" +
               $"  Humanoid: {HumanoidPercentage:F1}% (weight: {HumanoidWeight})\n" +
               $"  Beast: {BeastPercentage:F1}% (weight: {BeastWeight})";
    }
}

public class JotunheimIntegrityReport
{
    public bool BiomeExists { get; set; }
    public int RoomTemplateCount { get; set; }
    public int ExpectedRoomTemplateCount { get; set; }
    public int ResourceDropCount { get; set; }
    public int ExpectedResourceDropCount { get; set; }
    public int TrunkTemplateCount { get; set; }
    public int ExpectedTrunkTemplateCount { get; set; }
    public int RootsTemplateCount { get; set; }
    public int ExpectedRootsTemplateCount { get; set; }
    public int TrunkSpawnWeight { get; set; }
    public int RootsSpawnWeight { get; set; }
    public bool IsValid { get; set; }

    public override string ToString()
    {
        return $"Jötunheim Integrity Report:\n" +
               $"  Biome Exists: {BiomeExists}\n" +
               $"  Room Templates: {RoomTemplateCount}/{ExpectedRoomTemplateCount}\n" +
               $"  Resources: {ResourceDropCount}/{ExpectedResourceDropCount}\n" +
               $"  Trunk Templates: {TrunkTemplateCount}/{ExpectedTrunkTemplateCount} (weight: {TrunkSpawnWeight})\n" +
               $"  Roots Templates: {RootsTemplateCount}/{ExpectedRootsTemplateCount} (weight: {RootsSpawnWeight})\n" +
               $"  Valid: {IsValid}";
    }
}

#endregion
