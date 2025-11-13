using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Service for managing merchant inventories, restocking, and shop operations (v0.9)
/// </summary>
public class MerchantService
{
    private static readonly ILogger _log = Log.ForContext<MerchantService>();
    private readonly Random _random = new();

    /// <summary>
    /// Check if merchant needs restock and perform if necessary
    /// </summary>
    public void CheckAndRestock(Merchant merchant, DateTime currentGameTime)
    {
        if (merchant.NeedsRestock(currentGameTime))
        {
            RestockInventory(merchant, currentGameTime);
        }
    }

    /// <summary>
    /// Restock merchant's inventory with core and rotating stock
    /// </summary>
    public void RestockInventory(Merchant merchant, DateTime currentGameTime)
    {
        _log.Information("Restocking merchant: MerchantId={MerchantId}, Name={Name}, Type={Type}",
            merchant.Id, merchant.Name, merchant.Type);

        // Clear rotating stock (keep infinite stock items)
        merchant.Inventory.Items = merchant.Inventory.Items
            .Where(i => i.IsInfiniteStock)
            .ToList();

        // Generate new rotating stock based on merchant type
        var rotatingStock = GenerateRotatingStock(merchant);
        foreach (var item in rotatingStock)
        {
            merchant.Inventory.AddItem(item);
        }

        merchant.LastRestockTime = currentGameTime;
        merchant.Inventory.LastRestock = currentGameTime;

        _log.Information("Merchant restocked: MerchantId={MerchantId}, TotalItems={ItemCount}, RestockTime={Time}",
            merchant.Id, merchant.Inventory.Items.Count, currentGameTime);
    }

    /// <summary>
    /// Initialize merchant with core stock (items that always available)
    /// </summary>
    public void InitializeCoreStock(Merchant merchant)
    {
        _log.Debug("Initializing core stock for merchant: {MerchantId}, Type={Type}",
            merchant.Id, merchant.Type);

        merchant.Inventory.Items.Clear();

        switch (merchant.Type)
        {
            case MerchantType.General: // Kjartan
                InitializeKjartanCoreStock(merchant);
                break;

            case MerchantType.Apothecary: // Ragnhild
                InitializeRagnhildCoreStock(merchant);
                break;

            case MerchantType.ScrapTrader: // Ulf
                InitializeUlfCoreStock(merchant);
                break;
        }

        _log.Information("Core stock initialized: MerchantId={MerchantId}, CoreItems={Count}",
            merchant.Id, merchant.Inventory.Items.Count);
    }

    /// <summary>
    /// Initialize Kjartan's core stock (General Merchant)
    /// </summary>
    private void InitializeKjartanCoreStock(Merchant merchant)
    {
        // Infinite stock basic items
        merchant.Inventory.AddItem(new ShopItem
        {
            ItemId = "Healing Poultice",
            ItemType = "Consumable",
            Quantity = 999,
            BasePrice = 20,
            IsInfiniteStock = true
        });

        merchant.Inventory.AddItem(new ShopItem
        {
            ItemId = "Repair Kit",
            ItemType = "Component",
            Quantity = 999,
            BasePrice = 30,
            IsInfiniteStock = true
        });
    }

    /// <summary>
    /// Initialize Ragnhild's core stock (Apothecary)
    /// </summary>
    private void InitializeRagnhildCoreStock(Merchant merchant)
    {
        // Infinite stock consumables
        merchant.Inventory.AddItem(new ShopItem
        {
            ItemId = "Healing Poultice",
            ItemType = "Consumable",
            Quantity = 999,
            BasePrice = 25,
            IsInfiniteStock = true
        });

        merchant.Inventory.AddItem(new ShopItem
        {
            ItemId = "Antidote",
            ItemType = "Consumable",
            Quantity = 999,
            BasePrice = 40,
            IsInfiniteStock = true
        });

        merchant.Inventory.AddItem(new ShopItem
        {
            ItemId = "Stamina Draught",
            ItemType = "Consumable",
            Quantity = 5,
            BasePrice = 60,
            IsInfiniteStock = false
        });
    }

    /// <summary>
    /// Initialize Ulf's core stock (Scrap Trader)
    /// </summary>
    private void InitializeUlfCoreStock(Merchant merchant)
    {
        // Infinite stock materials
        merchant.Inventory.AddItem(new ShopItem
        {
            ItemId = ComponentType.ScrapMetal.ToString(),
            ItemType = "Component",
            Quantity = 999,
            BasePrice = 10,
            IsInfiniteStock = true
        });

        merchant.Inventory.AddItem(new ShopItem
        {
            ItemId = ComponentType.RustedComponents.ToString(),
            ItemType = "Component",
            Quantity = 999,
            BasePrice = 15,
            IsInfiniteStock = true
        });

        merchant.Inventory.AddItem(new ShopItem
        {
            ItemId = ComponentType.CommonHerb.ToString(),
            ItemType = "Component",
            Quantity = 999,
            BasePrice = 5,
            IsInfiniteStock = true
        });
    }

    /// <summary>
    /// Generate rotating stock based on merchant type
    /// </summary>
    private List<ShopItem> GenerateRotatingStock(Merchant merchant)
    {
        var items = new List<ShopItem>();

        switch (merchant.Type)
        {
            case MerchantType.General:
                items.AddRange(GenerateKjartanRotatingStock());
                break;

            case MerchantType.Apothecary:
                items.AddRange(GenerateRagnhildRotatingStock());
                break;

            case MerchantType.ScrapTrader:
                items.AddRange(GenerateUlfRotatingStock());
                break;
        }

        _log.Debug("Generated rotating stock: MerchantType={Type}, ItemCount={Count}",
            merchant.Type, items.Count);

        return items;
    }

    /// <summary>
    /// Generate Kjartan's rotating stock (weapons, armor)
    /// </summary>
    private List<ShopItem> GenerateKjartanRotatingStock()
    {
        var items = new List<ShopItem>();

        // 2-3 Uncommon weapons
        var uncommonWeapons = new[] { "Iron Sword", "Battle Axe", "War Spear" };
        var weaponCount = _random.Next(2, 4);
        for (int i = 0; i < weaponCount; i++)
        {
            var weapon = uncommonWeapons[_random.Next(uncommonWeapons.Length)];
            items.Add(new ShopItem
            {
                ItemId = weapon,
                ItemType = "Equipment",
                Quantity = 1,
                BasePrice = _random.Next(300, 601), // 300-600 Cogs
                IsInfiniteStock = false
            });
        }

        // 1-2 Uncommon armor pieces
        var uncommonArmor = new[] { "Chain Mail", "Reinforced Leathers", "Scale Armor" };
        var armorCount = _random.Next(1, 3);
        for (int i = 0; i < armorCount; i++)
        {
            var armor = uncommonArmor[_random.Next(uncommonArmor.Length)];
            items.Add(new ShopItem
            {
                ItemId = armor,
                ItemType = "Equipment",
                Quantity = 1,
                BasePrice = _random.Next(400, 801), // 400-800 Cogs
                IsInfiniteStock = false
            });
        }

        // 10% chance for Rare weapon
        if (_random.Next(100) < 10)
        {
            items.Add(new ShopItem
            {
                ItemId = "Masterwork Blade",
                ItemType = "Equipment",
                Quantity = 1,
                BasePrice = _random.Next(1500, 3001), // 1500-3000 Cogs
                IsInfiniteStock = false,
                MinimumRarity = ComponentRarity.Rare
            });
        }

        return items;
    }

    /// <summary>
    /// Generate Ragnhild's rotating stock (consumables, herbs)
    /// </summary>
    private List<ShopItem> GenerateRagnhildRotatingStock()
    {
        var items = new List<ShopItem>();

        // 2-3 Uncommon consumables
        var uncommonConsumables = new[] { "Greater Healing Potion", "Resist Poison", "Fortify Stamina" };
        var consumableCount = _random.Next(2, 4);
        for (int i = 0; i < consumableCount; i++)
        {
            var consumable = uncommonConsumables[_random.Next(uncommonConsumables.Length)];
            items.Add(new ShopItem
            {
                ItemId = consumable,
                ItemType = "Consumable",
                Quantity = _random.Next(2, 5),
                BasePrice = _random.Next(50, 101), // 50-100 Cogs
                IsInfiniteStock = false
            });
        }

        // 20% chance for Rare consumable
        if (_random.Next(100) < 20)
        {
            items.Add(new ShopItem
            {
                ItemId = "Elixir of Life",
                ItemType = "Consumable",
                Quantity = 1,
                BasePrice = _random.Next(150, 251), // 150-250 Cogs
                IsInfiniteStock = false,
                MinimumRarity = ComponentRarity.Rare
            });
        }

        // Herbal materials
        items.Add(new ShopItem
        {
            ItemId = ComponentType.MedicinalHerbs.ToString(),
            ItemType = "Component",
            Quantity = _random.Next(3, 7),
            BasePrice = 55,
            IsInfiniteStock = false
        });

        return items;
    }

    /// <summary>
    /// Generate Ulf's rotating stock (materials, oddities)
    /// </summary>
    private List<ShopItem> GenerateUlfRotatingStock()
    {
        var items = new List<ShopItem>();

        // 5-8 Random materials (all rarities)
        var allMaterials = new[]
        {
            ComponentType.ClothScraps,
            ComponentType.BoneShards,
            ComponentType.StructuralScrap,
            ComponentType.AethericDust,
            ComponentType.TemperedSprings
        };

        var materialCount = _random.Next(5, 9);
        for (int i = 0; i < materialCount; i++)
        {
            var material = allMaterials[_random.Next(allMaterials.Length)];
            var materialInfo = CraftingComponent.Create(material);

            items.Add(new ShopItem
            {
                ItemId = material.ToString(),
                ItemType = "Component",
                Quantity = _random.Next(1, 5),
                BasePrice = ((int)materialInfo.Quality * 10) /* SellValue calculated from Quality */,
                IsInfiniteStock = false
            });
        }

        // 2-3 Random consumables (cheap, questionable quality)
        var cheapConsumables = new[] { "Weak Healing Potion", "Dubious Elixir", "Scrap Rations" };
        var consumableCount = _random.Next(2, 4);
        for (int i = 0; i < consumableCount; i++)
        {
            var consumable = cheapConsumables[_random.Next(cheapConsumables.Length)];
            items.Add(new ShopItem
            {
                ItemId = consumable,
                ItemType = "Consumable",
                Quantity = _random.Next(2, 6),
                BasePrice = _random.Next(10, 31), // 10-30 Cogs
                IsInfiniteStock = false
            });
        }

        return items;
    }

    /// <summary>
    /// Get shop inventory as display-friendly list (basic - no pricing)
    /// </summary>
    public List<string> GetShopListing(Merchant merchant)
    {
        var listing = new List<string>();

        for (int i = 0; i < merchant.Inventory.Items.Count; i++)
        {
            var item = merchant.Inventory.Items[i];
            listing.Add($"{i + 1}. {item.GetDisplayString()}");
        }

        return listing;
    }

    /// <summary>
    /// Get shop inventory with reputation-adjusted pricing (v0.9)
    /// </summary>
    public List<string> GetShopListingWithPrices(Merchant merchant, PlayerCharacter player, PricingService pricingService)
    {
        var listing = new List<string>();

        for (int i = 0; i < merchant.Inventory.Items.Count; i++)
        {
            var item = merchant.Inventory.Items[i];
            var finalPrice = pricingService.GetFinalBuyPrice(merchant, item, player);

            var stockText = item.IsInfiniteStock ? "∞" : $"{item.Quantity}";
            var priceDisplay = pricingService.GetPriceDisplay(finalPrice, merchant, player, isBuying: true);

            listing.Add($"[cyan]{i + 1}.[/] {item.ItemId} [dim](Stock: {stockText})[/] - {priceDisplay}");
        }

        return listing;
    }

    /// <summary>
    /// Get merchant's buy-back prices for player's items (what merchant will pay)
    /// </summary>
    public List<string> GetSellListingForPlayerItems(Merchant merchant, PlayerCharacter player, PricingService pricingService)
    {
        var listing = new List<string>();
        int index = 1;

        // List equipment
        foreach (var eq in player.Inventory)
        {
            if (((int)eq.Quality * 10) /* SellValue calculated from Quality */ > 0)
            {
                var finalPrice = pricingService.GetFinalSellPrice(merchant, ((int)eq.Quality * 10) /* SellValue calculated from Quality */, "Equipment", player);
                var priceDisplay = pricingService.GetPriceDisplay(finalPrice, merchant, player, isBuying: false);
                listing.Add($"[cyan]{index}.[/] {eq.GetDisplayName()} - {priceDisplay}");
                index++;
            }
        }

        // List crafting components (if tradeable)
        foreach (var (componentType, quantity) in player.CraftingComponents)
        {
            var component = CraftingComponent.Create(componentType);
            if (component.IsTradeable && ((int)component.Quality * 10) /* SellValue calculated from Quality */ > 0)
            {
                var finalPrice = pricingService.GetFinalSellPrice(merchant, ((int)component.Quality * 10) /* SellValue calculated from Quality */, "Component", player);
                var priceDisplay = pricingService.GetPriceDisplay(finalPrice, merchant, player, isBuying: false);
                listing.Add($"[cyan]{index}.[/] {component.Name} x{quantity} - {priceDisplay} each");
                index++;
            }
        }

        return listing;
    }
}
