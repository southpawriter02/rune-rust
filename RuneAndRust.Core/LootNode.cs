namespace RuneAndRust.Core;

/// <summary>
/// v0.11 Loot Node System
/// Resource veins, salvageable wreckage, and containers
/// v5.0 compliant: All loot is salvaged/found, not manufactured
/// </summary>
public class LootNode
{
    // Identity
    public string NodeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LootNodeType Type { get; set; }

    // Discovery
    public bool RequiresDiscovery { get; set; } = false; // Hidden containers
    public int DiscoveryDC { get; set; } = 15; // WITS check DC
    public bool HasBeenDiscovered { get; set; } = false;

    // Interaction
    public bool RequiresInteraction { get; set; } = true; // Must be manually looted
    public int InteractionTurns { get; set; } = 1; // Turns required (e.g., 2 for mining)
    public string InteractionDescription { get; set; } = "Search"; // "Mine", "Salvage", "Open"

    // Loot Contents
    public LootTable LootTable { get; set; } = new();
    public bool HasBeenLooted { get; set; } = false;

    // Visual/Thematic
    public string FlavorText { get; set; } = string.Empty;
}

/// <summary>
/// Types of loot nodes for v0.11
/// </summary>
public enum LootNodeType
{
    OreVein,             // Iron, Copper, Dvergr Alloy (requires mining)
    SalvageableWreckage, // Scrap Metal, Springs, Circuit Boards
    HiddenContainer,     // Currency, equipment (requires discovery)
    CorruptedDataSlate,  // Quest hooks, lore (read-only per v5.0)
    ResourceCache,       // Consumables (healing, buffs)
    AncientEquipment,    // Pre-Glitch gear in stasis
    ChemicalBarrel,      // Crafting materials (acids, coolants)
    SupplyCrate          // Mixed consumables and materials
}

/// <summary>
/// Loot table defining what can be found in a loot node
/// </summary>
public class LootTable
{
    // Currency
    public bool DropsCurrency { get; set; } = false;
    public int MinCurrency { get; set; } = 0;
    public int MaxCurrency { get; set; } = 0;

    // Crafting Materials
    public List<LootDrop> CraftingMaterials { get; set; } = new();

    // Equipment
    public List<LootDrop> Equipment { get; set; } = new();

    // Consumables
    public List<LootDrop> Consumables { get; set; } = new();

    // Quest Items
    public List<LootDrop> QuestItems { get; set; } = new();

    /// <summary>
    /// Generates loot from this table
    /// </summary>
    public List<string> GenerateLoot(Random rng)
    {
        var loot = new List<string>();

        // Currency
        if (DropsCurrency)
        {
            int amount = rng.Next(MinCurrency, MaxCurrency + 1);
            loot.Add($"{amount} Dvergr Cogs");
        }

        // Roll for each drop category
        loot.AddRange(RollDrops(CraftingMaterials, rng));
        loot.AddRange(RollDrops(Equipment, rng));
        loot.AddRange(RollDrops(Consumables, rng));
        loot.AddRange(RollDrops(QuestItems, rng));

        return loot;
    }

    private List<string> RollDrops(List<LootDrop> drops, Random rng)
    {
        var results = new List<string>();
        foreach (var drop in drops)
        {
            if (rng.NextDouble() < drop.DropChance)
            {
                int quantity = rng.Next(drop.MinQuantity, drop.MaxQuantity + 1);
                results.Add($"{drop.ItemId} x{quantity}");
            }
        }
        return results;
    }
}

/// <summary>
/// Individual loot drop with chance and quantity
/// </summary>
public class LootDrop
{
    public string ItemId { get; set; } = string.Empty;
    public float DropChance { get; set; } = 1.0f; // 0.0 to 1.0
    public int MinQuantity { get; set; } = 1;
    public int MaxQuantity { get; set; } = 1;
    public string Rarity { get; set; } = "Common"; // Common, Uncommon, Rare
}
