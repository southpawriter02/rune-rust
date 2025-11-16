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

    #region Environmental Hazards (v0.32.2 - Placeholder)

    // TODO v0.32.2: Implement environmental hazards
    // - [Live Power Conduit] (signature hazard)
    // - [High-Pressure Steam Vent]
    // - [Unstable Ceiling/Wall]
    // - [Flooded] terrain (coolant spills)
    // - [Toxic Haze]
    // - [Cover] (shipping containers, engine blocks)
    // - [Jötun Corpse Terrain]
    // - [Assembly Line]
    // - [Scrap Heap]
    // - [Coolant Reservoir]

    #endregion

    #region Enemy Spawns (v0.32.3 - Placeholder)

    // TODO v0.32.3: Implement enemy spawns
    // - Rusted Servitor (Common Undying)
    // - Rusted Warden (Medium Undying)
    // - Draugr Juggernaut (Rare Undying)
    // - God-Sleeper Cultist (Medium Humanoid)
    // - Scrap-Tinker (Rare Humanoid)
    // - Iron-Husked Boar (Low Beast)

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
