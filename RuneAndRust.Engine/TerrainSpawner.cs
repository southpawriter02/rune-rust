using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.11 Static Terrain Spawner
/// Places cover, obstacles, and tactical terrain features
/// </summary>
public class TerrainSpawner
{
    private static readonly ILogger _log = Log.ForContext<TerrainSpawner>();

    /// <summary>
    /// Populates a room with static terrain based on biome elements
    /// </summary>
    public void PopulateRoom(Room room, BiomeDefinition biome, Random rng)
    {
        // Skip if handcrafted
        if (room.IsHandcrafted)
        {
            _log.Debug("Skipping terrain population for handcrafted room {RoomId}", room.RoomId);
            return;
        }

        // Determine terrain count (1-3 terrain features per room)
        int terrainCount = DetermineTerrainCount(room, rng);
        if (terrainCount == 0)
        {
            _log.Debug("Room {RoomId}: No terrain features", room.RoomId);
            return;
        }

        _log.Debug("Room {RoomId}: Spawning {Count} terrain features", room.RoomId, terrainCount);

        // Get eligible terrain
        if (biome.Elements == null)
        {
            _log.Warning("Biome {BiomeName} has no Elements table", biome.Name);
            return;
        }

        var availableTerrain = biome.Elements.GetEligibleElements(
            BiomeElementType.StaticTerrain, room, rng);

        if (availableTerrain.Count == 0)
        {
            _log.Debug("No eligible terrain for room {RoomId}", room.RoomId);
            return;
        }

        // Coherent Glitch: If Unstable Ceiling hazard present, MUST spawn Rubble Pile
        bool hasUnstableCeiling = room.DynamicHazards.Cast<DynamicHazard>().Any(h => h.Type == DynamicHazardType.UnstableCeiling);
        if (hasUnstableCeiling)
        {
            var rubblePile = CreateRubblePile();
            room.StaticTerrain.Add(rubblePile);
            _log.Debug("Coherent Glitch: Spawned mandatory Rubble Pile due to Unstable Ceiling in room {RoomId}", room.RoomId);
            terrainCount--; // One slot used
        }

        // Spawn remaining terrain
        for (int i = 0; i < terrainCount; i++)
        {
            var selected = biome.Elements.WeightedRandomSelection(availableTerrain, rng);
            if (selected == null) break;

            var terrain = CreateTerrainFromElement(selected, room, rng);
            if (terrain != null)
            {
                room.StaticTerrain.Add(terrain);
                _log.Debug("Spawned terrain {TerrainName} in room {RoomId}", terrain.Name, room.RoomId);
            }

            // Remove from pool to avoid duplicates
            availableTerrain = availableTerrain.Where(t => t.ElementName != selected.ElementName).ToList();
        }

        _log.Information("Room {RoomId}: Spawned {Count} terrain features", room.RoomId, room.StaticTerrain.Count);
    }

    /// <summary>
    /// Determines how many terrain features to spawn
    /// </summary>
    private int DetermineTerrainCount(Room room, Random rng)
    {
        // Entry halls: 1-2 (some cover)
        if (room.IsStartRoom || room.GeneratedNodeType == NodeType.Start)
        {
            return rng.Next(1, 3);
        }

        // Boss arenas: 2-3 (tactical terrain)
        if (room.IsBossRoom)
        {
            return rng.Next(2, 4);
        }

        // Secret rooms: 1-2
        if (room.GeneratedNodeType == NodeType.Secret)
        {
            return rng.Next(1, 3);
        }

        // Normal rooms: 1-3
        return rng.Next(1, 4);
    }

    /// <summary>
    /// Creates StaticTerrain from a BiomeElement
    /// </summary>
    private StaticTerrain? CreateTerrainFromElement(BiomeElement element, Room room, Random rng)
    {
        var terrainType = MapElementToTerrainType(element.AssociatedDataId);
        if (terrainType == null)
        {
            _log.Warning("Could not map element {ElementName} to terrain type", element.ElementName);
            return null;
        }

        return terrainType.Value switch
        {
            StaticTerrainType.CollapsedPillar => CreateCollapsedPillar(),
            StaticTerrainType.RubblePile => CreateRubblePile(),
            StaticTerrainType.RustedBulkhead => CreateRustedBulkhead(),
            StaticTerrainType.Chasm => CreateChasm(),
            StaticTerrainType.ElevatedPlatform => CreateElevatedPlatform(),
            _ => null
        };
    }

    private StaticTerrainType? MapElementToTerrainType(string? dataId)
    {
        return dataId switch
        {
            "collapsed_pillar" => StaticTerrainType.CollapsedPillar,
            "rubble_pile" => StaticTerrainType.RubblePile,
            "rusted_bulkhead" => StaticTerrainType.RustedBulkhead,
            "chasm" => StaticTerrainType.Chasm,
            "elevated_platform" => StaticTerrainType.ElevatedPlatform,
            _ => null
        };
    }

    // Terrain creation methods
    private StaticTerrain CreateCollapsedPillar()
    {
        return new CorrodedPillar
        {
            Id = $"collapsed_pillar_{Guid.NewGuid():N}",
            TerrainId = $"collapsed_pillar_{Guid.NewGuid():N}",
            Name = "Collapsed Pillar",
            Description = "A massive support pillar lies in ruins, providing substantial cover."
        };
    }

    private StaticTerrain CreateRubblePile()
    {
        return new RubblePile
        {
            Id = $"rubble_pile_{Guid.NewGuid():N}",
            TerrainId = $"rubble_pile_{Guid.NewGuid():N}",
            Name = "Rubble Pile",
            Description = "Debris and broken masonry create difficult terrain and partial cover."
        };
    }

    private StaticTerrain CreateRustedBulkhead()
    {
        return new RustedBulkhead
        {
            Id = $"rusted_bulkhead_{Guid.NewGuid():N}",
            TerrainId = $"rusted_bulkhead_{Guid.NewGuid():N}",
            Name = "Rusted Bulkhead",
            Description = "A corroded blast door provides solid cover despite centuries of decay."
        };
    }

    private StaticTerrain CreateChasm()
    {
        return new ChasmTerrain
        {
            Id = $"chasm_{Guid.NewGuid():N}",
            TerrainId = $"chasm_{Guid.NewGuid():N}",
            Name = "Chasm",
            Description = "A gaping hole in the floor plunges into darkness. The edges are unstable.",
            RequiresSkillCheck = true,
            SkillCheckDC = 12
        };
    }

    private StaticTerrain CreateElevatedPlatform()
    {
        return new ElevatedPlatform
        {
            Id = $"elevated_platform_{Guid.NewGuid():N}",
            TerrainId = $"elevated_platform_{Guid.NewGuid():N}",
            Name = "Elevated Platform",
            Description = "A raised maintenance walkway provides tactical high ground.",
            RequiresClimbing = true,
            ClimbDC = 10
        };
    }
}
