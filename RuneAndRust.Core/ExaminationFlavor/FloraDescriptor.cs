// ==============================================================================
// v0.38.9: Perception & Examination Descriptors
// FloraDescriptor.cs
// ==============================================================================
// Purpose: Plants, fungi, mosses, crystalline growths
// Usage: Adds living world feeling, biome atmosphere, alchemy components
// ==============================================================================

namespace RuneAndRust.Core.ExaminationFlavor;

/// <summary>
/// Represents a flora descriptor (plants, fungi, mosses, growths).
/// Adds biome atmosphere and alchemy integration.
/// </summary>
public class FloraDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Display name of the flora.
    /// Example: "Luminous Shelf Fungus", "Ember Moss", "Frost Lichen"
    /// </summary>
    public string FloraName { get; set; } = string.Empty;

    /// <summary>
    /// Type of flora.
    /// Values: Fungus, Moss, Lichen, Plant, CrystallineGrowth, Vine,
    ///         Flower, Algae, Spore
    /// </summary>
    public string FloraType { get; set; } = string.Empty;

    /// <summary>
    /// Detail level for this description.
    /// Values: Cursory, Detailed, Expert
    /// </summary>
    public string DetailLevel { get; set; } = string.Empty;

    // ==================== BIOME ====================

    /// <summary>
    /// Biome where this flora appears.
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim
    /// </summary>
    public string BiomeName { get; set; } = string.Empty;

    // ==================== PROPERTIES ====================

    /// <summary>
    /// Can be harvested for alchemy.
    /// </summary>
    public bool IsHarvestable { get; set; } = false;

    /// <summary>
    /// Harmful to touch or proximity.
    /// </summary>
    public bool IsDangerous { get; set; } = false;

    /// <summary>
    /// Alchemical applications.
    /// NULL if not harvestable.
    /// Example: "light-source potions, vision enhancement, consciousness-altering"
    /// </summary>
    public string? AlchemyUse { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// Flora description with {Variable} placeholders.
    ///
    /// Available variables:
    /// - {Species}, {Biome}, {AlchemyUse}
    /// - {Behavior}, {Habitat}
    ///
    /// Example: "Luminous Shelf Fungus, glowing faintly with bioluminescence..."
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    // ==================== METADATA ====================

    /// <summary>
    /// Probability weight for random selection.
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this descriptor is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization.
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid flora types.
    /// </summary>
    public static class FloraTypes
    {
        public const string Fungus = "Fungus";
        public const string Moss = "Moss";
        public const string Lichen = "Lichen";
        public const string Plant = "Plant";
        public const string CrystallineGrowth = "CrystallineGrowth";
        public const string Vine = "Vine";
        public const string Flower = "Flower";
        public const string Algae = "Algae";
        public const string Spore = "Spore";
    }

    /// <summary>
    /// Common biomes.
    /// </summary>
    public static class Biomes
    {
        public const string TheRoots = "The_Roots";
        public const string Muspelheim = "Muspelheim";
        public const string Niflheim = "Niflheim";
        public const string Alfheim = "Alfheim";
        public const string Jotunheim = "Jotunheim";
    }
}
