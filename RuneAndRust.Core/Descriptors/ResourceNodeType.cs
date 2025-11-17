namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.5: Resource node type classification
/// Four core types of resource nodes
/// </summary>
public enum ResourceNodeType
{
    MineralVein,        // Ore deposits (metal, crystal)
    SalvageWreckage,    // Mechanical salvage
    OrganicHarvest,     // Biological/chemical resources
    AethericAnomaly     // Runic/magical resources
}

/// <summary>
/// v0.38.5: Extraction method required for resource node
/// Determines time cost, tool requirements, and skill checks
/// </summary>
public enum ExtractionType
{
    Mining,         // 2 turns, MIGHT check or Mining Tool
    Salvaging,      // 3 turns, WITS check or Salvage Kit
    Harvesting,     // 1 turn, no tools required
    Siphoning,      // 2 turns, requires Aether Siphon or Galdr-caster
    Search          // 2 turns, WITS check (for caches)
}

/// <summary>
/// v0.38.5: Resource rarity tier
/// Affects spawn probability and value
/// </summary>
public enum RarityTier
{
    Common,         // 70% spawn probability
    Uncommon,       // 25% spawn probability
    Rare,           // 5% spawn probability
    Legendary       // <1% spawn probability (special conditions)
}
