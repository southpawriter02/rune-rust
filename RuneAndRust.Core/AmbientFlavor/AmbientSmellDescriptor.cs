// ==============================================================================
// v0.38.13: Ambient Environmental Descriptors
// AmbientSmellDescriptor.cs
// ==============================================================================
// Purpose: Environmental smell descriptors that enhance immersion
// Pattern: Follows CombatManeuverDescriptor structure from v0.38.12
// Integration: Used by AmbientEnvironmentService to generate smell descriptions
// ==============================================================================

namespace RuneAndRust.Core.AmbientFlavor;

/// <summary>
/// Represents an ambient smell descriptor for environmental olfactory events.
/// Describes smells that fire periodically or when entering new areas.
/// v0.38.13: Ambient Environmental Descriptors
/// </summary>
public class AmbientSmellDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Biome where this smell occurs.
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic
    /// </summary>
    public string Biome { get; set; } = string.Empty;

    /// <summary>
    /// Category of smell.
    /// Values: Decay, Mechanical, Organic, Fire, Cold, Chemical, Paradoxical, Industrial, Psychic
    /// </summary>
    public string SmellCategory { get; set; } = string.Empty;

    /// <summary>
    /// Subcategory of smell for more specific classification.
    /// Values: Rust, Mildew, MachineOil, FungalGrowth, DeadFlesh, Sulfur, Brimstone,
    ///         Ash, FrozenOzone, Sterile, Impossible, MetallicPsychic, Dust, Abandonment
    /// </summary>
    public string SmellSubcategory { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// Intensity of the smell (optional).
    /// Values: Subtle, Moderate, Overwhelming, NULL (any intensity)
    /// </summary>
    public string? Intensity { get; set; }

    /// <summary>
    /// Proximity of the smell source (optional).
    /// Values: Immediate, Nearby, Distant, Pervasive, NULL
    /// </summary>
    public string? Proximity { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The ambient smell description text with {Variable} placeholders.
    ///
    /// Available variables:
    /// - LOCATION: {Biome}, {LocationName}
    /// - DESCRIPTION: {IntensityModifier}, {SourceDescription}
    ///
    /// Example: "The smell of rust and corrosion, metallic and sharp."
    /// Example: "Sulfur. The smell burns your nose and throat."
    /// Example: "The smell is wrong. It's simultaneously floral and putrid."
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    // ==================== METADATA ====================

    /// <summary>
    /// Probability weight for random selection (1.0 = default).
    /// Higher values increase selection chance.
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this descriptor is active and can be selected.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization and filtering.
    /// Example: ["Unpleasant", "Industrial", "Organic"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid biome values.
    /// </summary>
    public static class Biomes
    {
        public const string The_Roots = "The_Roots";
        public const string Muspelheim = "Muspelheim";
        public const string Niflheim = "Niflheim";
        public const string Alfheim = "Alfheim";
        public const string Jötunheim = "Jötunheim";
        public const string Generic = "Generic";
    }

    /// <summary>
    /// Valid smell category values.
    /// </summary>
    public static class SmellCategories
    {
        public const string Decay = "Decay";
        public const string Mechanical = "Mechanical";
        public const string Organic = "Organic";
        public const string Fire = "Fire";
        public const string Cold = "Cold";
        public const string Chemical = "Chemical";
        public const string Paradoxical = "Paradoxical";
        public const string Industrial = "Industrial";
        public const string Psychic = "Psychic";
    }

    /// <summary>
    /// Valid intensity values.
    /// </summary>
    public static class Intensities
    {
        public const string Subtle = "Subtle";
        public const string Moderate = "Moderate";
        public const string Overwhelming = "Overwhelming";
    }

    /// <summary>
    /// Valid proximity values.
    /// </summary>
    public static class Proximities
    {
        public const string Immediate = "Immediate";
        public const string Nearby = "Nearby";
        public const string Distant = "Distant";
        public const string Pervasive = "Pervasive";
    }
}
