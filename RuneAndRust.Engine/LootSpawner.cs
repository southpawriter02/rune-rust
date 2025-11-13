using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.11 Loot Node Spawner
/// Places resource veins, containers, and salvageable wreckage
/// v5.0 compliant: All loot is salvaged/found, not manufactured
/// </summary>
public class LootSpawner
{
    private static readonly ILogger _log = Log.ForContext<LootSpawner>();

    /// <summary>
    /// Populates a room with loot nodes based on biome elements
    /// </summary>
    public void PopulateRoom(Room room, BiomeDefinition biome, Random rng)
    {
        // Skip if handcrafted
        if (room.IsHandcrafted)
        {
            _log.Debug("Skipping loot population for handcrafted room {RoomId}", room.RoomId);
            return;
        }

        // Determine loot node count (0-2 per room)
        int lootCount = DetermineLootCount(room, rng);
        if (lootCount == 0)
        {
            _log.Debug("Room {RoomId}: No loot nodes", room.RoomId);
            return;
        }

        _log.Debug("Room {RoomId}: Spawning {Count} loot nodes", room.RoomId, lootCount);

        // Get eligible loot nodes
        if (biome.Elements == null)
        {
            _log.Warning("Biome {BiomeName} has no Elements table", biome.Name);
            return;
        }

        var availableLoot = biome.Elements.GetEligibleElements(
            BiomeElementType.LootNode, room, rng);

        if (availableLoot.Count == 0)
        {
            _log.Debug("No eligible loot for room {RoomId}", room.RoomId);
            return;
        }

        // Apply weight modifiers for secret rooms
        if (room.GeneratedNodeType == NodeType.Secret)
        {
            foreach (var loot in availableLoot.Where(l => l.SpawnRules?.HigherWeightInSecretRooms == true))
            {
                loot.Weight *= loot.SpawnRules.SecretRoomWeightMultiplier;
                _log.Debug("Increased weight for {LootName} in secret room", loot.ElementName);
            }
        }

        // Spawn loot nodes
        for (int i = 0; i < lootCount; i++)
        {
            var selected = biome.Elements.WeightedRandomSelection(availableLoot, rng);
            if (selected == null) break;

            var lootNode = CreateLootNodeFromElement(selected, room, rng);
            if (lootNode != null)
            {
                room.LootNodes.Add(lootNode);
                _log.Debug("Spawned loot node {LootName} in room {RoomId}", lootNode.Name, room.RoomId);
            }

            // Remove from pool to avoid duplicates
            availableLoot = availableLoot.Where(l => l.ElementName != selected.ElementName).ToList();
        }

        _log.Information("Room {RoomId}: Spawned {Count} loot nodes", room.RoomId, room.LootNodes.Count);
    }

    /// <summary>
    /// Determines how many loot nodes to spawn
    /// </summary>
    private int DetermineLootCount(Room room, Random rng)
    {
        // Entry halls: 0-1 (minimal loot)
        if (room.IsStartRoom || room.GeneratedNodeType == NodeType.Start)
        {
            return rng.NextDouble() < 0.3 ? 1 : 0;
        }

        // Secret rooms: 1-2 (ALWAYS has loot, reward for exploration)
        if (room.GeneratedNodeType == NodeType.Secret)
        {
            return rng.NextDouble() < 0.6 ? 2 : 1;
        }

        // Boss arenas: 0 (boss drops loot directly)
        if (room.IsBossRoom)
        {
            return 0;
        }

        // Normal rooms: 0-2
        double roll = rng.NextDouble();
        if (roll < 0.5) return 0;
        if (roll < 0.85) return 1;
        return 2;
    }

    /// <summary>
    /// Creates a LootNode from a BiomeElement
    /// </summary>
    private Population.LootNode? CreateLootNodeFromElement(BiomeElement element, Room room, Random rng)
    {
        var lootType = MapElementToLootType(element.AssociatedDataId);
        if (lootType == null)
        {
            _log.Warning("Could not map element {ElementName} to loot type", element.ElementName);
            return null;
        }

        return lootType.Value switch
        {
            LootNodeType.OreVein => CreateOreVein(rng),
            LootNodeType.SalvageableWreckage => CreateSalvageableWreckage(rng),
            LootNodeType.HiddenContainer => CreateHiddenContainer(rng),
            LootNodeType.CorruptedDataSlate => CreateCorruptedDataSlate(rng),
            LootNodeType.ResourceCache => CreateResourceCache(rng),
            _ => null
        };
    }

    private LootNodeType? MapElementToLootType(string? dataId)
    {
        return dataId switch
        {
            "ore_vein" => LootNodeType.OreVein,
            "salvageable_wreckage" => LootNodeType.SalvageableWreckage,
            "hidden_container" => LootNodeType.HiddenContainer,
            "corrupted_data_slate" => LootNodeType.CorruptedDataSlate,
            "resource_cache" => LootNodeType.ResourceCache,
            _ => null
        };
    }

    // Loot node creation methods
    private Population.LootNode CreateOreVein(Random rng)
    {
        return new ResourceVein
        {
            Id = $"ore_vein_{Guid.NewGuid():N}",
            NodeId = $"ore_vein_{Guid.NewGuid():N}",
            Name = "[Ore Vein]",
            Description = "Mineral deposits glint in the wall - iron, copper, and traces of rare Dvergr alloys.",
            ResourceType = "Ore Deposits",
            EstimatedCogsValue = 30,
            Quality = LootQuality.Common
        };
    }

    private Population.LootNode CreateSalvageableWreckage(Random rng)
    {
        return new SalvageableWreckage
        {
            Id = $"salvageable_wreckage_{Guid.NewGuid():N}",
            NodeId = $"salvageable_wreckage_{Guid.NewGuid():N}",
            Name = "[Salvageable Wreckage]",
            Description = "The remains of a destroyed automaton lie scattered. Components might be salvageable.",
            Type = WreckageType.Machinery,
            EstimatedCogsValue = 40,
            Quality = LootQuality.Common
        };
    }

    private Population.LootNode CreateHiddenContainer(Random rng)
    {
        return new HiddenContainer
        {
            Id = $"hidden_container_{Guid.NewGuid():N}",
            NodeId = $"hidden_container_{Guid.NewGuid():N}",
            Name = "[Hidden Container]",
            Description = "A concealed storage locker. Its location suggests someone valued secrecy.",
            IsHidden = true,
            DiscoveryDC = 15,
            IsLocked = true,
            LockDC = 10,
            EstimatedCogsValue = 120,
            Quality = LootQuality.Rare
        };
    }

    private Population.LootNode CreateCorruptedDataSlate(Random rng)
    {
        return new CorruptedDataSlate
        {
            Id = $"corrupted_data_slate_{Guid.NewGuid():N}",
            NodeId = $"corrupted_data_slate_{Guid.NewGuid():N}",
            Name = "[Corrupted Data-Slate]",
            Description = "A Pre-Glitch data storage device. The screen flickers with fragmented text.",
            LoreFragmentId = null,
            EstimatedCogsValue = 20,
            Quality = LootQuality.Uncommon
        };
    }

    private Population.LootNode CreateResourceCache(Random rng)
    {
        return new ResourceCache
        {
            Id = $"resource_cache_{Guid.NewGuid():N}",
            NodeId = $"resource_cache_{Guid.NewGuid():N}",
            Name = "[Resource Cache]",
            Description = "An emergency supply stash. Surprisingly well-preserved.",
            CacheType = "Emergency Supplies",
            EstimatedCogsValue = 35,
            Quality = LootQuality.Common
        };
    }
}
