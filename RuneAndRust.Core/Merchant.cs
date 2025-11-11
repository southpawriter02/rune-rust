namespace RuneAndRust.Core;

/// <summary>
/// Type of merchant specialization (v0.9)
/// </summary>
public enum MerchantType
{
    General,        // Kjartan: weapons, armor, general goods
    Apothecary,     // Ragnhild: consumables, healing
    ScrapTrader,    // Ulf: materials, components
    Specialist      // Future: bows, heavy armor, etc.
}

/// <summary>
/// Represents a merchant NPC with shop functionality (v0.9)
/// Extends NPC with inventory, pricing, and trading capabilities
/// </summary>
public class Merchant : NPC
{
    // Merchant-specific properties
    public MerchantType Type { get; set; } = MerchantType.General;
    public ShopInventory Inventory { get; set; } = new();
    public int RestockIntervalDays { get; set; } = 3; // Days between restocks
    public DateTime LastRestockTime { get; set; } = DateTime.MinValue;

    // Pricing modifiers
    public float BaseMarkup { get; set; } = 2.0f; // Player buys at 2x, sells at 50%
    public float ReputationPriceRange { get; set; } = 0.3f; // ±30% based on reputation
    public float BarterSkillImpact { get; set; } = 0.2f; // Up to 20% discount (future)

    // Specialty modifiers - how much more/less merchant pays for specific categories
    public Dictionary<string, float> CategoryModifiers { get; set; } = new();

    /// <summary>
    /// Check if merchant needs to restock based on current game time
    /// </summary>
    public bool NeedsRestock(DateTime currentGameTime)
    {
        if (LastRestockTime == DateTime.MinValue)
        {
            return true; // First stock
        }

        var daysSinceRestock = (currentGameTime - LastRestockTime).TotalDays;
        return daysSinceRestock >= RestockIntervalDays;
    }
}

/// <summary>
/// Represents a merchant's shop inventory (v0.9)
/// </summary>
public class ShopInventory
{
    public List<ShopItem> Items { get; set; } = new();
    public int MaxInventorySize { get; set; } = 20;
    public DateTime LastRestock { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Add an item to shop inventory
    /// </summary>
    public void AddItem(ShopItem item)
    {
        // Check if item already exists in inventory
        var existing = Items.FirstOrDefault(i => i.ItemId == item.ItemId);
        if (existing != null && existing.IsInfiniteStock)
        {
            // Don't add duplicates of infinite stock items
            return;
        }

        if (Items.Count < MaxInventorySize)
        {
            Items.Add(item);
        }
    }

    /// <summary>
    /// Remove an item from shop inventory (sold out)
    /// </summary>
    public void RemoveItem(string itemId)
    {
        var item = Items.FirstOrDefault(i => i.ItemId == itemId);
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    /// <summary>
    /// Decrease item quantity (after purchase)
    /// </summary>
    public bool DecreaseQuantity(string itemId, int amount = 1)
    {
        var item = Items.FirstOrDefault(i => i.ItemId == itemId);
        if (item == null)
        {
            return false;
        }

        if (item.IsInfiniteStock)
        {
            return true; // Infinite stock never depletes
        }

        item.Quantity -= amount;
        if (item.Quantity <= 0)
        {
            Items.Remove(item);
        }

        return true;
    }
}

/// <summary>
/// Represents a single item in a merchant's shop (v0.9)
/// </summary>
public class ShopItem
{
    public string ItemId { get; set; } = string.Empty; // Equipment name, Component type, or Consumable name
    public string ItemType { get; set; } = string.Empty; // "Equipment", "Component", "Consumable"
    public int Quantity { get; set; } = 1;
    public int BasePrice { get; set; } = 0; // Price before modifiers
    public bool IsInfiniteStock { get; set; } = false; // Never runs out
    public ComponentRarity? MinimumRarity { get; set; } = null; // For filtering

    /// <summary>
    /// Get display string for shop listing
    /// </summary>
    public string GetDisplayString()
    {
        var stockText = IsInfiniteStock ? "∞" : $"{Quantity}";
        return $"{ItemId} (Stock: {stockText}) - {BasePrice} Cogs";
    }
}
