namespace RuneAndRust.Core.Population;

/// <summary>
/// Loot spawn points in procedurally generated Sectors (v0.11)
/// </summary>
public abstract class LootNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string NodeId { get; set; } = string.Empty; // Alias for Core compatibility
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Loot Properties
    public int EstimatedCogsValue { get; set; } = 0;
    public LootQuality Quality { get; set; } = LootQuality.Common;

    // Discovery
    public bool IsHidden { get; set; } = false;
    public int DiscoveryDC { get; set; } = 0; // WITS check DC
    public bool HasBeenDiscovered { get; set; } = false;
    public bool HasBeenLooted { get; set; } = false;

    // Positional Data
    public Vector2 Position { get; set; } = new Vector2(0, 0);

    // v0.37.1: Investigation and Search properties
    public string NodeType => Name; // Alias for command system
    public string FlavorText => Description; // Alias for command system
    public int Tier { get; set; } = 0; // Loot tier (0-3)
    public bool RequiresInvestigation { get; set; } = false;
    public int InvestigationDC { get; set; } = 2;
    public bool HiddenContentRevealed { get; set; } = false;
}

/// <summary>
/// Loot quality tiers (v0.11)
/// </summary>
public enum LootQuality
{
    Vendor = 0,     // 5-10 Cogs worth
    Common = 1,     // 20-40 Cogs worth
    Uncommon = 2,   // 50-80 Cogs worth
    Rare = 3,       // 100-150 Cogs worth
    Legendary = 4   // 200+ Cogs worth
}

/// <summary>
/// [Resource Vein] - Salvageable materials (v0.11)
/// </summary>
public class ResourceVein : LootNode
{
    public ResourceVein()
    {
        Name = "Resource Vein";
        Description = "A deposit of salvageable materials embedded in the walls.";
        Quality = LootQuality.Common;
        EstimatedCogsValue = 30;
    }

    public string ResourceType { get; set; } = "Scrap Metal"; // "Scrap Metal", "Power Cells", etc.
}

/// <summary>
/// [Salvageable Wreckage] - Equipment loot (v0.11)
/// </summary>
public class SalvageableWreckage : LootNode
{
    public SalvageableWreckage()
    {
        Name = "Salvageable Wreckage";
        Description = "Broken equipment that might yield useful components.";
        Quality = LootQuality.Common;
        EstimatedCogsValue = 40;
    }

    public WreckageType Type { get; set; } = WreckageType.Machinery;
}

public enum WreckageType
{
    Machinery,
    Weapons,
    Equipment,
    PersonalEffects
}

/// <summary>
/// [Hidden Container] - Rare high-value loot (v0.11)
/// </summary>
public class HiddenContainer : LootNode
{
    public HiddenContainer()
    {
        Name = "Hidden Container";
        Description = "A concealed storage locker, its contents unknown.";
        Quality = LootQuality.Rare;
        EstimatedCogsValue = 120;
        IsHidden = true;
        DiscoveryDC = 12; // WITS check DC 12 (reduced from 15 in v0.12 balance tuning)
    }

    public bool IsLocked { get; set; } = true;
    public int LockDC { get; set; } = 10; // FINESSE check to pick
}

/// <summary>
/// [Corrupted Data-Slate] - Lore and quest hooks (v0.11)
/// </summary>
public class CorruptedDataSlate : LootNode
{
    public CorruptedDataSlate()
    {
        Name = "Corrupted Data-Slate";
        Description = "A Pre-Glitch data storage device. The screen flickers with fragmented text.";
        Quality = LootQuality.Uncommon;
        EstimatedCogsValue = 20;
    }

    public string? LoreFragmentId { get; set; } = null;
}

/// <summary>
/// [Resource Cache] - Consumables and supplies (v0.11)
/// </summary>
public class ResourceCache : LootNode
{
    public ResourceCache()
    {
        Name = "Resource Cache";
        Description = "An emergency supply stash. Surprisingly well-preserved.";
        Quality = LootQuality.Common;
        EstimatedCogsValue = 35;
    }

    public string CacheType { get; set; } = "Emergency Supplies";
}
