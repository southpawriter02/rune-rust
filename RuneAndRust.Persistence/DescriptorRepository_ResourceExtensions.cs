using Microsoft.Data.Sqlite;
using RuneAndRust.Core.Descriptors;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38.5: Resource node extensions for DescriptorRepository
/// Provides database access to resource nodes and biome resource profiles
/// </summary>
public partial class DescriptorRepository
{
    private static readonly ILogger _resourceLog = Log.ForContext<DescriptorRepository>();

    #region Biome Resource Profiles

    /// <summary>
    /// Gets all biome resource profiles
    /// </summary>
    public List<BiomeResourceProfile> GetAllBiomeResourceProfiles()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                profile_id, biome_name,
                common_resources, uncommon_resources,
                rare_resources, legendary_resources,
                spawn_density_small, spawn_density_medium, spawn_density_large,
                unique_resources, notes
            FROM Biome_Resource_Profiles
            ORDER BY biome_name
        ";

        var profiles = new List<BiomeResourceProfile>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            profiles.Add(MapBiomeResourceProfile(reader));
        }

        _resourceLog.Debug("Loaded {Count} biome resource profiles", profiles.Count);
        return profiles;
    }

    /// <summary>
    /// Gets biome resource profile by biome name
    /// </summary>
    public BiomeResourceProfile? GetBiomeResourceProfile(string biomeName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                profile_id, biome_name,
                common_resources, uncommon_resources,
                rare_resources, legendary_resources,
                spawn_density_small, spawn_density_medium, spawn_density_large,
                unique_resources, notes
            FROM Biome_Resource_Profiles
            WHERE biome_name = $biomeName
        ";
        command.Parameters.AddWithValue("$biomeName", biomeName);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapBiomeResourceProfile(reader);
        }

        _resourceLog.Warning("Biome resource profile not found: {BiomeName}", biomeName);
        return null;
    }

    private BiomeResourceProfile MapBiomeResourceProfile(SqliteDataReader reader)
    {
        return new BiomeResourceProfile
        {
            ProfileId = reader.GetInt32(0),
            BiomeName = reader.GetString(1),
            CommonResources = reader.GetString(2),
            UncommonResources = reader.GetString(3),
            RareResources = reader.GetString(4),
            LegendaryResources = reader.IsDBNull(5) ? null : reader.GetString(5),
            SpawnDensitySmall = reader.GetInt32(6),
            SpawnDensityMedium = reader.GetInt32(7),
            SpawnDensityLarge = reader.GetInt32(8),
            UniqueResources = reader.IsDBNull(9) ? null : reader.GetString(9),
            Notes = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }

    #endregion

    #region Resource Nodes

    /// <summary>
    /// Gets all resource nodes for a room
    /// </summary>
    public List<ResourceNode> GetResourceNodesForRoom(int roomId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                node_id, room_id, composite_descriptor_id,
                node_name, description, node_type,
                extraction_type, extraction_dc, extraction_time,
                requires_tool, required_tool,
                yield_min, yield_max, resource_type, rarity_tier,
                depleted, uses_remaining, max_uses,
                hazardous, hazard_description, trap_chance,
                hidden, detection_dc, unstable, requires_galdr,
                biome_restriction, tags
            FROM Resource_Nodes
            WHERE room_id = $roomId
        ";
        command.Parameters.AddWithValue("$roomId", roomId);

        var nodes = new List<ResourceNode>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            nodes.Add(MapResourceNode(reader));
        }

        _resourceLog.Debug("Loaded {Count} resource nodes for room {RoomId}", nodes.Count, roomId);
        return nodes;
    }

    /// <summary>
    /// Gets a resource node by ID
    /// </summary>
    public ResourceNode? GetResourceNodeById(int nodeId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                node_id, room_id, composite_descriptor_id,
                node_name, description, node_type,
                extraction_type, extraction_dc, extraction_time,
                requires_tool, required_tool,
                yield_min, yield_max, resource_type, rarity_tier,
                depleted, uses_remaining, max_uses,
                hazardous, hazard_description, trap_chance,
                hidden, detection_dc, unstable, requires_galdr,
                biome_restriction, tags
            FROM Resource_Nodes
            WHERE node_id = $nodeId
        ";
        command.Parameters.AddWithValue("$nodeId", nodeId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapResourceNode(reader);
        }

        _resourceLog.Warning("Resource node not found: ID {NodeId}", nodeId);
        return null;
    }

    /// <summary>
    /// Saves a resource node to the database
    /// </summary>
    public int SaveResourceNode(ResourceNode node)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Resource_Nodes (
                room_id, composite_descriptor_id,
                node_name, description, node_type,
                extraction_type, extraction_dc, extraction_time,
                requires_tool, required_tool,
                yield_min, yield_max, resource_type, rarity_tier,
                depleted, uses_remaining, max_uses,
                hazardous, hazard_description, trap_chance,
                hidden, detection_dc, unstable, requires_galdr,
                biome_restriction, tags
            ) VALUES (
                $roomId, $compositeDescriptorId,
                $nodeName, $description, $nodeType,
                $extractionType, $extractionDC, $extractionTime,
                $requiresTool, $requiredTool,
                $yieldMin, $yieldMax, $resourceType, $rarityTier,
                $depleted, $usesRemaining, $maxUses,
                $hazardous, $hazardDescription, $trapChance,
                $hidden, $detectionDC, $unstable, $requiresGaldr,
                $biomeRestriction, $tags
            );
            SELECT last_insert_rowid();
        ";

        command.Parameters.AddWithValue("$roomId", node.RoomId);
        command.Parameters.AddWithValue("$compositeDescriptorId", (object?)node.CompositeDescriptorId ?? DBNull.Value);
        command.Parameters.AddWithValue("$nodeName", node.Name);
        command.Parameters.AddWithValue("$description", node.Description);
        command.Parameters.AddWithValue("$nodeType", node.NodeType.ToString());
        command.Parameters.AddWithValue("$extractionType", node.ExtractionType.ToString());
        command.Parameters.AddWithValue("$extractionDC", node.ExtractionDC);
        command.Parameters.AddWithValue("$extractionTime", node.ExtractionTime);
        command.Parameters.AddWithValue("$requiresTool", node.RequiresTool ? 1 : 0);
        command.Parameters.AddWithValue("$requiredTool", (object?)node.RequiredTool ?? DBNull.Value);
        command.Parameters.AddWithValue("$yieldMin", node.YieldMin);
        command.Parameters.AddWithValue("$yieldMax", node.YieldMax);
        command.Parameters.AddWithValue("$resourceType", node.ResourceType);
        command.Parameters.AddWithValue("$rarityTier", node.RarityTier.ToString());
        command.Parameters.AddWithValue("$depleted", node.Depleted ? 1 : 0);
        command.Parameters.AddWithValue("$usesRemaining", node.UsesRemaining);
        command.Parameters.AddWithValue("$maxUses", node.MaxUses);
        command.Parameters.AddWithValue("$hazardous", node.Hazardous ? 1 : 0);
        command.Parameters.AddWithValue("$hazardDescription", (object?)node.HazardDescription ?? DBNull.Value);
        command.Parameters.AddWithValue("$trapChance", node.TrapChance);
        command.Parameters.AddWithValue("$hidden", node.Hidden ? 1 : 0);
        command.Parameters.AddWithValue("$detectionDC", node.DetectionDC);
        command.Parameters.AddWithValue("$unstable", node.Unstable ? 1 : 0);
        command.Parameters.AddWithValue("$requiresGaldr", node.RequiresGaldr ? 1 : 0);
        command.Parameters.AddWithValue("$biomeRestriction", (object?)node.BiomeRestriction ?? DBNull.Value);
        command.Parameters.AddWithValue("$tags", (object?)node.Tags ?? DBNull.Value);

        var nodeId = Convert.ToInt32(command.ExecuteScalar());
        _resourceLog.Debug("Saved resource node: {NodeName} (ID: {NodeId})", node.Name, nodeId);
        return nodeId;
    }

    /// <summary>
    /// Updates a resource node in the database
    /// </summary>
    public void UpdateResourceNode(ResourceNode node)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Resource_Nodes SET
                depleted = $depleted,
                uses_remaining = $usesRemaining
            WHERE node_id = $nodeId
        ";

        command.Parameters.AddWithValue("$depleted", node.Depleted ? 1 : 0);
        command.Parameters.AddWithValue("$usesRemaining", node.UsesRemaining);
        command.Parameters.AddWithValue("$nodeId", node.NodeId);

        command.ExecuteNonQuery();
        _resourceLog.Debug("Updated resource node: {NodeId}", node.NodeId);
    }

    private ResourceNode MapResourceNode(SqliteDataReader reader)
    {
        return new ResourceNode
        {
            NodeId = reader.GetInt32(0),
            RoomId = reader.GetInt32(1),
            CompositeDescriptorId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
            Name = reader.GetString(3),
            Description = reader.GetString(4),
            NodeType = Enum.Parse<ResourceNodeType>(reader.GetString(5)),
            ExtractionType = Enum.Parse<ExtractionType>(reader.GetString(6)),
            ExtractionDC = reader.GetInt32(7),
            ExtractionTime = reader.GetInt32(8),
            RequiresTool = reader.GetInt32(9) == 1,
            RequiredTool = reader.IsDBNull(10) ? null : reader.GetString(10),
            YieldMin = reader.GetInt32(11),
            YieldMax = reader.GetInt32(12),
            ResourceType = reader.GetString(13),
            RarityTier = Enum.Parse<RarityTier>(reader.GetString(14)),
            Depleted = reader.GetInt32(15) == 1,
            UsesRemaining = reader.GetInt32(16),
            MaxUses = reader.GetInt32(17),
            Hazardous = reader.GetInt32(18) == 1,
            HazardDescription = reader.IsDBNull(19) ? null : reader.GetString(19),
            TrapChance = (float)reader.GetDouble(20),
            Hidden = reader.GetInt32(21) == 1,
            DetectionDC = reader.GetInt32(22),
            Unstable = reader.GetInt32(23) == 1,
            RequiresGaldr = reader.GetInt32(24) == 1,
            BiomeRestriction = reader.IsDBNull(25) ? null : reader.GetString(25),
            Tags = reader.IsDBNull(26) ? null : reader.GetString(26)
        };
    }

    #endregion

    #region Resource Statistics

    /// <summary>
    /// Gets statistics about resource nodes
    /// </summary>
    public Dictionary<string, int> GetResourceNodeStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT node_type, COUNT(*) as count
            FROM Resource_Nodes
            GROUP BY node_type
            ORDER BY node_type
        ";

        var stats = new Dictionary<string, int>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            stats[reader.GetString(0)] = reader.GetInt32(1);
        }

        _resourceLog.Information("Resource node stats: {Stats}",
            string.Join(", ", stats.Select(kvp => $"{kvp.Key}={kvp.Value}")));

        return stats;
    }

    #endregion
}
