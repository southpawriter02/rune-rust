using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.35.3: Modifies merchant inventory based on territorial faction control
/// Each faction adds unique items to merchant stock when they control a sector
/// </summary>
public class MerchantFactionInventory
{
    private static readonly ILogger _log = Log.ForContext<MerchantFactionInventory>();

    /// <summary>
    /// Get faction-specific inventory additions
    /// Returns list of items that should be added to merchant stock
    /// </summary>
    public List<ShopItem> GetFactionInventoryAdditions(string? controllingFaction)
    {
        if (string.IsNullOrEmpty(controllingFaction))
        {
            return new List<ShopItem>();
        }

        var additions = controllingFaction switch
        {
            "IronBanes" => GetIronBanesInventory(),
            "JotunReaders" => GetJotunReadersInventory(),
            "GodSleeperCultists" => GetGodSleepersInventory(),
            "RustClans" => GetRustClansInventory(),
            "Independents" => GetIndependentsInventory(),
            _ => new List<ShopItem>()
        };

        _log.Debug("Generated {Count} faction-specific items for {Faction}",
            additions.Count, controllingFaction);

        return additions;
    }

    /// <summary>
    /// Iron-Banes: Anti-Undying gear and blessed equipment
    /// </summary>
    private List<ShopItem> GetIronBanesInventory()
    {
        return new List<ShopItem>
        {
            new ShopItem
            {
                ItemId = "Blessed Weapon Oil",
                ItemType = "Consumable",
                Quantity = 5,
                BasePrice = 50,
                IsInfiniteStock = false
            },
            new ShopItem
            {
                ItemId = "Salt Grenades",
                ItemType = "Consumable",
                Quantity = 10,
                BasePrice = 30,
                IsInfiniteStock = false
            },
            new ShopItem
            {
                ItemId = "Iron-Forged Armor",
                ItemType = "Equipment",
                Quantity = 2,
                BasePrice = 300,
                IsInfiniteStock = false,
                MinimumRarity = ComponentRarity.Uncommon
            },
            new ShopItem
            {
                ItemId = "Purity Oath Talisman",
                ItemType = "Equipment",
                Quantity = 1,
                BasePrice = 200,
                IsInfiniteStock = false
            }
        };
    }

    /// <summary>
    /// Jötun-Readers: Pre-Glitch technology and schematics
    /// </summary>
    private List<ShopItem> GetJotunReadersInventory()
    {
        return new List<ShopItem>
        {
            new ShopItem
            {
                ItemId = "Ancient Schematic",
                ItemType = "Consumable",
                Quantity = 3,
                BasePrice = 150,
                IsInfiniteStock = false,
                MinimumRarity = ComponentRarity.Rare
            },
            new ShopItem
            {
                ItemId = "Data Core Fragment",
                ItemType = "Component",
                Quantity = 5,
                BasePrice = 80,
                IsInfiniteStock = false,
                MinimumRarity = ComponentRarity.Uncommon
            },
            new ShopItem
            {
                ItemId = "Pre-Glitch Tool Kit",
                ItemType = "Consumable",
                Quantity = 4,
                BasePrice = 120,
                IsInfiniteStock = false
            },
            new ShopItem
            {
                ItemId = "Research Notes",
                ItemType = "Consumable",
                Quantity = 10,
                BasePrice = 40,
                IsInfiniteStock = true
            }
        };
    }

    /// <summary>
    /// God-Sleeper Cultists: Aetheric items and corrupted gear
    /// </summary>
    private List<ShopItem> GetGodSleepersInventory()
    {
        return new List<ShopItem>
        {
            new ShopItem
            {
                ItemId = "Aether Flask",
                ItemType = "Consumable",
                Quantity = 8,
                BasePrice = 60,
                IsInfiniteStock = false
            },
            new ShopItem
            {
                ItemId = "Corrupted Talisman",
                ItemType = "Equipment",
                Quantity = 2,
                BasePrice = 250,
                IsInfiniteStock = false,
                MinimumRarity = ComponentRarity.Rare
            },
            new ShopItem
            {
                ItemId = "Ritual Components",
                ItemType = "Consumable",
                Quantity = 12,
                BasePrice = 35,
                IsInfiniteStock = true
            },
            new ShopItem
            {
                ItemId = "Awakening Catalyst",
                ItemType = "Consumable",
                Quantity = 3,
                BasePrice = 180,
                IsInfiniteStock = false
            }
        };
    }

    /// <summary>
    /// Rust-Clans: Salvage materials and practical gear
    /// Also provides 15% discount on all items
    /// </summary>
    private List<ShopItem> GetRustClansInventory()
    {
        return new List<ShopItem>
        {
            new ShopItem
            {
                ItemId = "Scrap Bundle",
                ItemType = "Component",
                Quantity = 100,
                BasePrice = 5,
                IsInfiniteStock = true
            },
            new ShopItem
            {
                ItemId = "Repair Kit",
                ItemType = "Consumable",
                Quantity = 10,
                BasePrice = 45,
                IsInfiniteStock = false
            },
            new ShopItem
            {
                ItemId = "Salvaged Plating",
                ItemType = "Component",
                Quantity = 15,
                BasePrice = 70,
                IsInfiniteStock = false,
                MinimumRarity = ComponentRarity.Uncommon
            },
            new ShopItem
            {
                ItemId = "Trade Goods",
                ItemType = "Consumable",
                Quantity = 20,
                BasePrice = 25,
                IsInfiniteStock = true
            }
        };
    }

    /// <summary>
    /// Independents: Diverse selection, no specialization
    /// </summary>
    private List<ShopItem> GetIndependentsInventory()
    {
        return new List<ShopItem>
        {
            new ShopItem
            {
                ItemId = "Mystery Box",
                ItemType = "Consumable",
                Quantity = 5,
                BasePrice = 100,
                IsInfiniteStock = false
            },
            new ShopItem
            {
                ItemId = "Traveler's Supplies",
                ItemType = "Consumable",
                Quantity = 10,
                BasePrice = 30,
                IsInfiniteStock = true
            },
            new ShopItem
            {
                ItemId = "Survival Kit",
                ItemType = "Consumable",
                Quantity = 6,
                BasePrice = 55,
                IsInfiniteStock = false
            }
        };
    }

    /// <summary>
    /// Get price modifier for Rust-Clans controlled territory
    /// Rust-Clans provide 15% discount on all goods
    /// </summary>
    public double GetFactionPriceModifier(string? controllingFaction)
    {
        return controllingFaction switch
        {
            "RustClans" => 0.85, // 15% discount
            _ => 1.0 // No modifier
        };
    }

    /// <summary>
    /// Apply faction inventory to merchant
    /// Adds faction-specific items and applies price modifiers
    /// </summary>
    public void ApplyFactionInventory(Merchant merchant, string? controllingFaction)
    {
        if (string.IsNullOrEmpty(controllingFaction))
        {
            return;
        }

        // Add faction-specific items
        var additions = GetFactionInventoryAdditions(controllingFaction);
        foreach (var item in additions)
        {
            merchant.Inventory.AddItem(item);
        }

        _log.Information("Applied {Faction} inventory to merchant {MerchantId}: added {Count} items",
            controllingFaction, merchant.Id, additions.Count);
    }
}
