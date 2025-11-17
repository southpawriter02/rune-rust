using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Service for generating and managing loot drops
/// </summary>
public class LootService
{
    private static readonly ILogger _log = Log.ForContext<LootService>();
    private readonly Random _random;

    public LootService()
    {
        _random = new Random();
    }

    /// <summary>
    /// Generate loot from a defeated enemy
    /// </summary>
    public Equipment? GenerateLoot(Enemy enemy, PlayerCharacter? player = null)
    {
        var loot = enemy.Type switch
        {
            EnemyType.CorruptedServitor => GenerateServitorLoot(player),
            EnemyType.BlightDrone => GenerateDroneLoot(player),
            EnemyType.RuinWarden => GenerateBossLoot(player),
            _ => null
        };

        if (loot != null)
        {
            _log.Information("Loot generated: Enemy={EnemyType}, Item={ItemName}, Quality={Quality}, Type={ItemType}",
                enemy.Type, loot.Name, loot.Quality, loot.Type);
        }
        else
        {
            _log.Debug("No loot dropped: Enemy={EnemyType}", enemy.Type);
        }

        return loot;
    }

    /// <summary>
    /// Generate currency drop from a defeated enemy (v0.9)
    /// </summary>
    public int GenerateCurrencyDrop(Enemy enemy)
    {
        int amount = enemy.Type switch
        {
            EnemyType.CorruptedServitor => _random.Next(10, 31), // 10-30 Cogs (trash mob)
            EnemyType.BlightDrone => _random.Next(40, 81),       // 40-80 Cogs (standard enemy)
            EnemyType.RuinWarden => _random.Next(300, 801),      // 300-800 Cogs (boss)
            _ => _random.Next(5, 21)                             // 5-20 Cogs (default)
        };

        _log.Information("Currency dropped: Enemy={EnemyType}, Amount={Amount}", enemy.Type, amount);

        return amount;
    }

    /// <summary>
    /// Generate crafting material drops from a defeated enemy (v0.9)
    /// Returns a dictionary of ComponentType -> quantity
    /// </summary>
    public Dictionary<ComponentType, int> GenerateMaterialDrops(Enemy enemy)
    {
        var drops = new Dictionary<ComponentType, int>();

        // 40% chance for Common material (1-3 units)
        if (_random.Next(100) < 40)
        {
            var commonMaterials = new[]
            {
                ComponentType.ScrapMetal,
                ComponentType.RustedComponents,
                ComponentType.ClothScraps,
                ComponentType.BoneShards
            };
            var material = commonMaterials[_random.Next(commonMaterials.Length)];
            var quantity = _random.Next(1, 4); // 1-3 units
            drops[material] = quantity;
            _log.Information("Material dropped: Enemy={EnemyType}, Material={Material}, Quantity={Quantity}, Rarity=Common",
                enemy.Type, material, quantity);
        }

        // 15% chance for Uncommon material (1-2 units)
        if (_random.Next(100) < 15)
        {
            var uncommonMaterials = new[]
            {
                ComponentType.StructuralScrap,
                ComponentType.AethericDust,
                ComponentType.TemperedSprings,
                ComponentType.MedicinalHerbs
            };
            var material = uncommonMaterials[_random.Next(uncommonMaterials.Length)];
            var quantity = _random.Next(1, 3); // 1-2 units
            drops[material] = quantity;
            _log.Information("Material dropped: Enemy={EnemyType}, Material={Material}, Quantity={Quantity}, Rarity=Uncommon",
                enemy.Type, material, quantity);
        }

        // 4% chance for Rare material (1 unit)
        if (_random.Next(100) < 4)
        {
            var rareMaterials = new[]
            {
                ComponentType.DvergrAlloyIngot,
                ComponentType.CorruptedCrystal,
                ComponentType.AncientCircuitBoard
            };
            var material = rareMaterials[_random.Next(rareMaterials.Length)];
            drops[material] = 1;
            _log.Information("Material dropped: Enemy={EnemyType}, Material={Material}, Quantity=1, Rarity=Rare",
                enemy.Type, material);
        }

        // 0.5% chance for Epic material (1 unit) - only from bosses
        if (enemy.Type == EnemyType.RuinWarden && _random.Next(1000) < 5)
        {
            var epicMaterials = new[]
            {
                ComponentType.JotunCoreFragment,
                ComponentType.RunicEtchingTemplate
            };
            var material = epicMaterials[_random.Next(epicMaterials.Length)];
            drops[material] = 1;
            _log.Information("Material dropped: Enemy={EnemyType}, Material={Material}, Quantity=1, Rarity=Epic",
                enemy.Type, material);
        }

        if (drops.Count == 0)
        {
            _log.Debug("No materials dropped: Enemy={EnemyType}", enemy.Type);
        }

        return drops;
    }

    /// <summary>
    /// Generate loot from Corrupted Servitor (Tier 0 - Trash Mob)
    /// </summary>
    private Equipment? GenerateServitorLoot(PlayerCharacter? player)
    {
        // 60% chance: Jury-Rigged
        // 30% chance: Scavenged
        // 10% chance: Nothing
        int roll = _random.Next(100);

        QualityTier quality;
        if (roll < 10) return null; // No loot
        else if (roll < 70) quality = QualityTier.JuryRigged;
        else quality = QualityTier.Scavenged;

        return GenerateRandomItem(quality, player);
    }

    /// <summary>
    /// Generate loot from Blight-Drone (Tier 1 - Standard Enemy)
    /// </summary>
    private Equipment? GenerateDroneLoot(PlayerCharacter? player)
    {
        // 40% chance: Scavenged
        // 40% chance: Clan-Forged
        // 20% chance: Optimized
        int roll = _random.Next(100);

        QualityTier quality;
        if (roll < 40) quality = QualityTier.Scavenged;
        else if (roll < 80) quality = QualityTier.ClanForged;
        else quality = QualityTier.Optimized;

        return GenerateRandomItem(quality, player);
    }

    /// <summary>
    /// Generate loot from Ruin-Warden (Boss - Tier 2)
    /// </summary>
    private Equipment? GenerateBossLoot(PlayerCharacter? player)
    {
        // 30% chance: Optimized
        // 70% chance: Myth-Forged
        int roll = _random.Next(100);

        QualityTier quality;
        if (roll < 30) quality = QualityTier.Optimized;
        else quality = QualityTier.MythForged;

        // Boss always drops appropriate for player class
        return GenerateClassAppropriateItem(quality, player);
    }

    /// <summary>
    /// Generate a random item of a specific quality
    /// 50% chance weapon (appropriate for player class if possible)
    /// 50% chance armor
    /// </summary>
    private Equipment? GenerateRandomItem(QualityTier quality, PlayerCharacter? player)
    {
        // 50/50 weapon or armor
        bool isWeapon = _random.Next(2) == 0;

        if (isWeapon && player != null)
        {
            // 60% chance to drop weapon appropriate for player's class
            if (_random.Next(100) < 60)
            {
                var weapon = EquipmentDatabase.GetRandomWeaponForClass(player.Class, quality);
                if (weapon != null) return weapon;
            }
        }

        // Fallback: random item of this quality
        var allItems = EquipmentDatabase.GetAllEquipment()
            .Where(e => e.Quality == quality)
            .ToList();

        if (allItems.Count == 0) return null;

        // Filter to weapon or armor based on roll
        var filtered = allItems.Where(e =>
            isWeapon ? e.Type == EquipmentType.Weapon : e.Type == EquipmentType.Armor
        ).ToList();

        if (filtered.Count == 0) return allItems[_random.Next(allItems.Count)];

        return filtered[_random.Next(filtered.Count)];
    }

    /// <summary>
    /// Generate an item appropriate for the player's class
    /// Boss loot - always class-appropriate
    /// </summary>
    private Equipment? GenerateClassAppropriateItem(QualityTier quality, PlayerCharacter? player)
    {
        if (player == null)
        {
            return GenerateRandomItem(quality, null);
        }

        // 50/50 weapon or armor
        bool isWeapon = _random.Next(2) == 0;

        if (isWeapon)
        {
            var weapon = EquipmentDatabase.GetRandomWeaponForClass(player.Class, quality);
            if (weapon != null) return weapon;
        }
        else
        {
            var armor = EquipmentDatabase.GetRandomArmor(quality);
            if (armor != null) return armor;
        }

        // Fallback
        return GenerateRandomItem(quality, player);
    }

    /// <summary>
    /// Drop loot to room ground
    /// </summary>
    public void DropLootToRoom(Equipment? loot, Room room, List<string> combatLog)
    {
        if (loot == null) return;

        room.ItemsOnGround.Add(loot);
        combatLog.Add($"[yellow]💎 {loot.GetDisplayName()} dropped![/]");
    }

    /// <summary>
    /// Place starting loot in a room
    /// Used for initial room setup
    /// </summary>
    public void PlaceStartingLoot(Room room, Equipment item)
    {
        room.ItemsOnGround.Add(item);
    }

    /// <summary>
    /// Create a specific weapon for starting rooms
    /// </summary>
    public Equipment? CreateStartingWeapon(CharacterClass characterClass)
    {
        // Give player a Scavenged-tier weapon appropriate for their class
        return characterClass switch
        {
            CharacterClass.Warrior => EquipmentDatabase.GetByName("Scavenged Axe"),
            CharacterClass.Scavenger => EquipmentDatabase.GetByName("Scavenged Spear"),
            CharacterClass.Mystic => EquipmentDatabase.GetByName("Scavenged Staff"),
            _ => null
        };
    }

    /// <summary>
    /// Create puzzle reward (Optimized-tier weapon)
    /// </summary>
    public Equipment? CreatePuzzleReward(CharacterClass characterClass)
    {
        // Puzzle always rewards Optimized-tier class-appropriate weapon
        return EquipmentDatabase.GetRandomWeaponForClass(characterClass, QualityTier.Optimized);
    }

    /// <summary>
    /// v0.37.1: Generate loot from a LootNode (container, corpse, etc.)
    /// Returns equipment, components, and currency
    /// </summary>
    public GeneratedLoot? GenerateLootForNode(RuneAndRust.Core.Population.LootNode lootNode)
    {
        var result = new GeneratedLoot();

        // Tier-based loot generation
        var tier = lootNode.Tier;
        var quality = tier switch
        {
            0 => QualityTier.JuryRigged,
            1 => QualityTier.Scavenged,
            2 => QualityTier.ClanForged,
            3 => QualityTier.Optimized,
            _ => QualityTier.JuryRigged
        };

        // Equipment drop (40% chance for tiers 0-1, 60% for tier 2+)
        var equipmentChance = tier >= 2 ? 60 : 40;
        if (_random.Next(100) < equipmentChance)
        {
            var item = GenerateRandomItem(quality, null);
            if (item != null)
            {
                result.Equipment.Add(item);
            }
        }

        // Currency (always drops)
        result.Currency = tier switch
        {
            0 => _random.Next(5, 16),      // 5-15 Scrap
            1 => _random.Next(15, 31),     // 15-30 Scrap
            2 => _random.Next(30, 61),     // 30-60 Scrap
            3 => _random.Next(50, 101),    // 50-100 Scrap
            _ => _random.Next(5, 11)
        };

        // Crafting components (60% chance)
        if (_random.Next(100) < 60)
        {
            var componentCount = _random.Next(1, 4); // 1-3 different components
            for (int i = 0; i < componentCount; i++)
            {
                var component = GenerateRandomComponent(tier);
                if (result.Components.ContainsKey(component.Item1))
                {
                    result.Components[component.Item1] += component.Item2;
                }
                else
                {
                    result.Components[component.Item1] = component.Item2;
                }
            }
        }

        _log.Information(
            "Loot generated for node: NodeType={NodeType}, Tier={Tier}, Equipment={EquipCount}, Components={ComponentTypes}, Currency={Currency}",
            lootNode.NodeType,
            tier,
            result.Equipment.Count,
            result.Components.Count,
            result.Currency);

        return result;
    }

    /// <summary>
    /// Generate a random crafting component based on tier
    /// </summary>
    private (ComponentType, int) GenerateRandomComponent(int tier)
    {
        // Select rarity based on tier
        var rarityRoll = _random.Next(100);
        ComponentType component;
        int quantity;

        if (tier >= 3 && rarityRoll < 10) // Epic (10% for tier 3+)
        {
            var epicMaterials = new[]
            {
                ComponentType.JotunCoreFragment,
                ComponentType.RunicEtchingTemplate
            };
            component = epicMaterials[_random.Next(epicMaterials.Length)];
            quantity = 1;
        }
        else if (tier >= 2 && rarityRoll < 25) // Rare (25% for tier 2+)
        {
            var rareMaterials = new[]
            {
                ComponentType.DvergrAlloyIngot,
                ComponentType.CorruptedCrystal,
                ComponentType.AncientCircuitBoard
            };
            component = rareMaterials[_random.Next(rareMaterials.Length)];
            quantity = _random.Next(1, 3);
        }
        else if (rarityRoll < 60) // Uncommon (60%)
        {
            var uncommonMaterials = new[]
            {
                ComponentType.StructuralScrap,
                ComponentType.AethericDust,
                ComponentType.TemperedSprings,
                ComponentType.MedicinalHerbs
            };
            component = uncommonMaterials[_random.Next(uncommonMaterials.Length)];
            quantity = _random.Next(1, 4);
        }
        else // Common (40%)
        {
            var commonMaterials = new[]
            {
                ComponentType.ScrapMetal,
                ComponentType.RustedComponents,
                ComponentType.ClothScraps,
                ComponentType.BoneShards
            };
            component = commonMaterials[_random.Next(commonMaterials.Length)];
            quantity = _random.Next(2, 6);
        }

        return (component, quantity);
    }
}

/// <summary>
/// v0.37.1: Result of loot generation from containers
/// </summary>
public class GeneratedLoot
{
    public List<Equipment> Equipment { get; set; } = new();
    public Dictionary<ComponentType, int> Components { get; set; } = new();
    public int Currency { get; set; } = 0;
}
