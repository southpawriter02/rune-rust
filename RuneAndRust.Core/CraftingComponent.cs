namespace RuneAndRust.Core;

/// <summary>
/// Rarity tier for crafting components (v0.9)
/// </summary>
public enum ComponentRarity
{
    Common,
    Uncommon,
    Rare,
    Epic
}

/// <summary>
/// Types of crafting components for Field Medicine and Economy (v0.9)
/// </summary>
public enum ComponentType
{
    // Field Medicine Components (Bone-Setter specialty)
    CommonHerb,      // Basic medicinal plants
    CleanCloth,      // Sterile bandages, cloth strips
    Suture,          // Thread, wire for stitching
    Antiseptic,      // Alcohol, cleaning agents
    Splint,          // Wood, metal rods for setting bones
    Stimulant,       // Caffeine, adrenaline compounds (rare)

    // v0.9: Economy Materials - Common (5-20 Cogs)
    ScrapMetal,      // Used for weapon/armor repair
    RustedComponents, // Low-grade mechanical parts
    ClothScraps,     // Armor padding
    BoneShards,      // Crafting material

    // v0.9: Economy Materials - Uncommon (25-75 Cogs)
    StructuralScrap,  // High-grade building material (Sigrun quest item)
    AethericDust,     // Magical crafting component
    TemperedSprings,  // Quality mechanical parts
    MedicinalHerbs,   // Potent healing ingredient

    // v0.9: Economy Materials - Rare (100-300 Cogs)
    DvergrAlloyIngot, // Pre-Glitch metal
    CorruptedCrystal, // Unstable Aetheric source
    AncientCircuitBoard, // Pre-Glitch electronics

    // v0.9: Economy Materials - Epic (500-1000 Cogs)
    JotunCoreFragment, // Power source
    RunicEtchingTemplate, // Enchanting material

    // v0.19.10: Runeforging Components (Rúnasmiðr specialization)
    AetherDust,      // Magical fuel for Runeforging (common)
    UruzStone,       // Catalyst for Bull's Strength imbuement (uncommon)
    AlgizTablet,     // Catalyst for Warding Rune imbuement (uncommon)
    HagalazCrystal   // Catalyst for ice trap runes (rare)
}

/// <summary>
/// Represents a crafting component used in recipes (v0.7, v0.9)
/// </summary>
public class CraftingComponent
{
    public ComponentType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ComponentRarity Rarity { get; set; } = ComponentRarity.Common; // v0.9
    public int SellValue { get; set; } = 0; // v0.9 - Dvergr Cogs value
    public bool IsTradeable { get; set; } = false; // v0.9 - Can be sold to merchants

    /// <summary>
    /// Creates a component by type
    /// </summary>
    public static CraftingComponent Create(ComponentType type)
    {
        return type switch
        {
            // Field Medicine Components (not tradeable - used for crafting only)
            ComponentType.CommonHerb => new CraftingComponent
            {
                Type = ComponentType.CommonHerb,
                Name = "Common Herb",
                Description = "Medicinal plants salvaged from ruins",
                Rarity = ComponentRarity.Common,
                SellValue = 5,
                IsTradeable = true
            },
            ComponentType.CleanCloth => new CraftingComponent
            {
                Type = ComponentType.CleanCloth,
                Name = "Clean Cloth",
                Description = "Sterilized bandages and cloth strips",
                Rarity = ComponentRarity.Common,
                SellValue = 0,
                IsTradeable = false
            },
            ComponentType.Suture => new CraftingComponent
            {
                Type = ComponentType.Suture,
                Name = "Suture",
                Description = "Thread and wire for stitching wounds",
                Rarity = ComponentRarity.Common,
                SellValue = 0,
                IsTradeable = false
            },
            ComponentType.Antiseptic => new CraftingComponent
            {
                Type = ComponentType.Antiseptic,
                Name = "Antiseptic",
                Description = "Alcohol and cleaning agents",
                Rarity = ComponentRarity.Uncommon,
                SellValue = 0,
                IsTradeable = false
            },
            ComponentType.Splint => new CraftingComponent
            {
                Type = ComponentType.Splint,
                Name = "Splint Material",
                Description = "Wood or metal rods for setting bones",
                Rarity = ComponentRarity.Common,
                SellValue = 0,
                IsTradeable = false
            },
            ComponentType.Stimulant => new CraftingComponent
            {
                Type = ComponentType.Stimulant,
                Name = "Stimulant",
                Description = "Rare adrenaline compounds",
                Rarity = ComponentRarity.Rare,
                SellValue = 0,
                IsTradeable = false
            },

            // v0.9: Economy Materials - Common (5-20 Cogs)
            ComponentType.ScrapMetal => new CraftingComponent
            {
                Type = ComponentType.ScrapMetal,
                Name = "Scrap Metal",
                Description = "Salvaged metal fragments from Jötun-Forged ruins",
                Rarity = ComponentRarity.Common,
                SellValue = 10,
                IsTradeable = true
            },
            ComponentType.RustedComponents => new CraftingComponent
            {
                Type = ComponentType.RustedComponents,
                Name = "Rusted Components",
                Description = "Low-grade mechanical parts, corroded but functional",
                Rarity = ComponentRarity.Common,
                SellValue = 15,
                IsTradeable = true
            },
            ComponentType.ClothScraps => new CraftingComponent
            {
                Type = ComponentType.ClothScraps,
                Name = "Cloth Scraps",
                Description = "Tattered fabric suitable for armor padding",
                Rarity = ComponentRarity.Common,
                SellValue = 8,
                IsTradeable = true
            },
            ComponentType.BoneShards => new CraftingComponent
            {
                Type = ComponentType.BoneShards,
                Name = "Bone Shards",
                Description = "Fragments of pre-Glitch biological remains",
                Rarity = ComponentRarity.Common,
                SellValue = 12,
                IsTradeable = true
            },

            // v0.9: Economy Materials - Uncommon (25-75 Cogs)
            ComponentType.StructuralScrap => new CraftingComponent
            {
                Type = ComponentType.StructuralScrap,
                Name = "Structural Scrap",
                Description = "High-grade building material from Jötun frames",
                Rarity = ComponentRarity.Uncommon,
                SellValue = 50,
                IsTradeable = true
            },
            ComponentType.AethericDust => new CraftingComponent
            {
                Type = ComponentType.AethericDust,
                Name = "Aetheric Dust",
                Description = "Crystallized Aetheric energy, unstable but valuable",
                Rarity = ComponentRarity.Uncommon,
                SellValue = 60,
                IsTradeable = true
            },
            ComponentType.TemperedSprings => new CraftingComponent
            {
                Type = ComponentType.TemperedSprings,
                Name = "Tempered Springs",
                Description = "Quality mechanical parts still holding tension",
                Rarity = ComponentRarity.Uncommon,
                SellValue = 45,
                IsTradeable = true
            },
            ComponentType.MedicinalHerbs => new CraftingComponent
            {
                Type = ComponentType.MedicinalHerbs,
                Name = "Medicinal Herbs",
                Description = "Potent healing plants, rare in the wasteland",
                Rarity = ComponentRarity.Uncommon,
                SellValue = 55,
                IsTradeable = true
            },

            // v0.9: Economy Materials - Rare (100-300 Cogs)
            ComponentType.DvergrAlloyIngot => new CraftingComponent
            {
                Type = ComponentType.DvergrAlloyIngot,
                Name = "Dvergr Alloy Ingot",
                Description = "Pre-Glitch metal alloy, nearly indestructible",
                Rarity = ComponentRarity.Rare,
                SellValue = 200,
                IsTradeable = true
            },
            ComponentType.CorruptedCrystal => new CraftingComponent
            {
                Type = ComponentType.CorruptedCrystal,
                Name = "Corrupted Crystal",
                Description = "Unstable Aetheric source pulsing with dark energy",
                Rarity = ComponentRarity.Rare,
                SellValue = 180,
                IsTradeable = true
            },
            ComponentType.AncientCircuitBoard => new CraftingComponent
            {
                Type = ComponentType.AncientCircuitBoard,
                Name = "Ancient Circuit Board",
                Description = "Pre-Glitch electronics, miraculously intact",
                Rarity = ComponentRarity.Rare,
                SellValue = 250,
                IsTradeable = true
            },

            // v0.9: Economy Materials - Epic (500-1000 Cogs)
            ComponentType.JotunCoreFragment => new CraftingComponent
            {
                Type = ComponentType.JotunCoreFragment,
                Name = "Jötun Core Fragment",
                Description = "Power source from a Jötun construct, still radiating energy",
                Rarity = ComponentRarity.Epic,
                SellValue = 750,
                IsTradeable = true
            },
            ComponentType.RunicEtchingTemplate => new CraftingComponent
            {
                Type = ComponentType.RunicEtchingTemplate,
                Name = "Runic Etching Template",
                Description = "Pre-Glitch enchanting template with Dvergr runes",
                Rarity = ComponentRarity.Epic,
                SellValue = 900,
                IsTradeable = true
            },

            // v0.19.10: Runeforging Components
            ComponentType.AetherDust => new CraftingComponent
            {
                Type = ComponentType.AetherDust,
                Name = "Aether Dust",
                Description = "Powdered Aetheric energy, fuel for runic inscription",
                Rarity = ComponentRarity.Common,
                SellValue = 12,
                IsTradeable = true
            },
            ComponentType.UruzStone => new CraftingComponent
            {
                Type = ComponentType.UruzStone,
                Name = "Uruz Stone",
                Description = "Rune-carved stone resonating with primal strength",
                Rarity = ComponentRarity.Uncommon,
                SellValue = 45,
                IsTradeable = true
            },
            ComponentType.AlgizTablet => new CraftingComponent
            {
                Type = ComponentType.AlgizTablet,
                Name = "Algiz Tablet",
                Description = "Protective ward inscribed on ancient metal",
                Rarity = ComponentRarity.Uncommon,
                SellValue = 50,
                IsTradeable = true
            },
            ComponentType.HagalazCrystal => new CraftingComponent
            {
                Type = ComponentType.HagalazCrystal,
                Name = "Hagalaz Crystal",
                Description = "Ice-aspected crystal crackling with frozen energy",
                Rarity = ComponentRarity.Rare,
                SellValue = 120,
                IsTradeable = true
            },

            _ => throw new ArgumentException($"Unknown component type: {type}")
        };
    }
}
