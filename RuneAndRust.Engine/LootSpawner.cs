using RuneAndRust.Core;
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
    private LootNode? CreateLootNodeFromElement(BiomeElement element, Room room, Random rng)
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
    private LootNode CreateOreVein(Random rng)
    {
        return new LootNode
        {
            NodeId = $"ore_vein_{Guid.NewGuid():N}",
            Name = "[Ore Vein]",
            Description = "Mineral deposits glint in the wall - iron, copper, and traces of rare Dvergr alloys.",
            Type = LootNodeType.OreVein,
            RequiresInteraction = true,
            InteractionTurns = 2,
            InteractionDescription = "Mine",
            LootTable = new LootTable
            {
                CraftingMaterials = new List<LootDrop>
                {
                    new() { ItemId = "Iron Ore", DropChance = 0.8f, MinQuantity = 2, MaxQuantity = 4, Rarity = "Common" },
                    new() { ItemId = "Copper Ore", DropChance = 0.5f, MinQuantity = 1, MaxQuantity = 3, Rarity = "Uncommon" },
                    new() { ItemId = "Dvergr Alloy Fragment", DropChance = 0.1f, MinQuantity = 1, MaxQuantity = 1, Rarity = "Rare" }
                }
            },
            FlavorText = "The Pre-Glitch extraction equipment is long dead, but the ore remains."
        };
    }

    private LootNode CreateSalvageableWreckage(Random rng)
    {
        return new LootNode
        {
            NodeId = $"salvageable_wreckage_{Guid.NewGuid():N}",
            Name = "[Salvageable Wreckage]",
            Description = "The remains of a destroyed automaton lie scattered. Components might be salvageable.",
            Type = LootNodeType.SalvageableWreckage,
            RequiresInteraction = true,
            InteractionTurns = 1,
            InteractionDescription = "Salvage",
            LootTable = new LootTable
            {
                CraftingMaterials = new List<LootDrop>
                {
                    new() { ItemId = "Scrap Metal", DropChance = 1.0f, MinQuantity = 1, MaxQuantity = 4, Rarity = "Common" },
                    new() { ItemId = "Tempered Springs", DropChance = 0.3f, MinQuantity = 1, MaxQuantity = 2, Rarity = "Uncommon" },
                    new() { ItemId = "Ancient Circuit Board", DropChance = 0.1f, MinQuantity = 1, MaxQuantity = 1, Rarity = "Rare" }
                }
            },
            FlavorText = "Rust-fused mechanisms still hold value for those who know what to look for."
        };
    }

    private LootNode CreateHiddenContainer(Random rng)
    {
        return new LootNode
        {
            NodeId = $"hidden_container_{Guid.NewGuid():N}",
            Name = "[Hidden Container]",
            Description = "A concealed storage locker. Its location suggests someone valued secrecy.",
            Type = LootNodeType.HiddenContainer,
            RequiresDiscovery = true,
            DiscoveryDC = 15,
            RequiresInteraction = true,
            InteractionTurns = 1,
            InteractionDescription = "Open",
            LootTable = new LootTable
            {
                DropsCurrency = true,
                MinCurrency = 30,
                MaxCurrency = 100,
                Equipment = new List<LootDrop>
                {
                    new() { ItemId = "Random Uncommon Equipment", DropChance = 0.6f, MinQuantity = 1, MaxQuantity = 1, Rarity = "Uncommon" }
                },
                Consumables = new List<LootDrop>
                {
                    new() { ItemId = "Repair Kit", DropChance = 0.4f, MinQuantity = 1, MaxQuantity = 2, Rarity = "Common" }
                }
            },
            FlavorText = "The lock is centuries old but still functional."
        };
    }

    private LootNode CreateCorruptedDataSlate(Random rng)
    {
        return new LootNode
        {
            NodeId = $"corrupted_data_slate_{Guid.NewGuid():N}",
            Name = "[Corrupted Data-Slate]",
            Description = "A Pre-Glitch data storage device. The screen flickers with fragmented text.",
            Type = LootNodeType.CorruptedDataSlate,
            RequiresInteraction = true,
            InteractionTurns = 1,
            InteractionDescription = "Read",
            LootTable = new LootTable
            {
                QuestItems = new List<LootDrop>
                {
                    new() { ItemId = "Lore Fragment", DropChance = 1.0f, MinQuantity = 1, MaxQuantity = 1, Rarity = "Uncommon" }
                }
            },
            FlavorText = "v5.0 compliance: Read-only. Contents cannot be modified."
        };
    }

    private LootNode CreateResourceCache(Random rng)
    {
        return new LootNode
        {
            NodeId = $"resource_cache_{Guid.NewGuid():N}",
            Name = "[Resource Cache]",
            Description = "An emergency supply stash. Surprisingly well-preserved.",
            Type = LootNodeType.ResourceCache,
            RequiresInteraction = true,
            InteractionTurns = 1,
            InteractionDescription = "Search",
            LootTable = new LootTable
            {
                Consumables = new List<LootDrop>
                {
                    new() { ItemId = "Medical Supplies", DropChance = 0.7f, MinQuantity = 1, MaxQuantity = 2, Rarity = "Common" },
                    new() { ItemId = "Combat Stim", DropChance = 0.3f, MinQuantity = 1, MaxQuantity = 1, Rarity = "Uncommon" }
                }
            },
            FlavorText = "Pre-Glitch emergency protocols mandated such caches throughout Aethelgard."
        };
    }
}
