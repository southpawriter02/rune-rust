// ==============================================================================
// v0.38.7: Ability & Galdr Flavor Text
// GaldrManifestationDescriptor.cs
// ==============================================================================
// Purpose: Visual/sensory descriptions of magical effects
// Usage: What the magic looks like, sounds like, feels like
// Integration: Layered with action descriptors for full narrative
// ==============================================================================

namespace RuneAndRust.Core.GaldrFlavor;

/// <summary>
/// Represents the visual, auditory, and sensory manifestation of Galdr magic.
/// Describes how magic appears, sounds, and feels in the world.
/// </summary>
public class GaldrManifestationDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this manifestation descriptor.
    /// </summary>
    public int ManifestationId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Rune school this manifestation belongs to.
    /// Values: Fehu, Thurisaz, Ansuz, Raido, Hagalaz, Naudiz, Isa, Jera,
    ///         Tiwaz, Berkanan, Mannaz, Laguz
    /// </summary>
    public string RuneSchool { get; set; } = string.Empty;

    /// <summary>
    /// Type of sensory manifestation.
    /// Values: Visual, Auditory, Tactile, Supernatural, RunicGlyph
    /// </summary>
    public string ManifestationType { get; set; } = string.Empty;

    // ==================== MAGIC TYPE ====================

    /// <summary>
    /// Element associated with the manifestation.
    /// Values: Fire, Ice, Lightning, Wind, Earth, Water,
    ///         Healing, Shadow, Aether, NULL
    /// </summary>
    public string? Element { get; set; }

    /// <summary>
    /// Power level of the manifestation.
    /// Values: Weak, Moderate, Strong, Devastating, Catastrophic
    /// </summary>
    public string? PowerLevel { get; set; }

    // ==================== CONTEXT ====================

    /// <summary>
    /// Biome-specific visual modifiers.
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim, NULL (any)
    /// </summary>
    public string? BiomeName { get; set; }

    /// <summary>
    /// How the magic interacts with the environment.
    /// Example: "Steam erupts from rusty pipes", "Frost spreads across metal"
    /// </summary>
    public string? EnvironmentalInteraction { get; set; }

    // ==================== DESCRIPTION ====================

    /// <summary>
    /// The manifestation flavor text.
    ///
    /// Examples:
    /// - Visual: "The rune blazes white-hot, casting harsh shadows across the room."
    /// - Auditory: "A thunderous crack splits the air, echoing off metal walls."
    /// - Tactile: "Heat washes over you in waves, singeing exposed skin."
    /// - Supernatural: "Reality warps around the spell, geometry folding impossibly."
    /// - RunicGlyph: "The Fehu rune materializes in burning crimson lines."
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    // ==================== METADATA ====================

    /// <summary>
    /// Probability weight for random selection (1.0 = default).
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this descriptor is active and can be selected.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization.
    /// Example: ["Subtle", "Dramatic", "Horrifying", "Beautiful"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid manifestation types.
    /// </summary>
    public static class ManifestationTypes
    {
        public const string Visual = "Visual";
        public const string Auditory = "Auditory";
        public const string Tactile = "Tactile";
        public const string Supernatural = "Supernatural";
        public const string RunicGlyph = "RunicGlyph";
    }

    /// <summary>
    /// Valid elements.
    /// </summary>
    public static class Elements
    {
        public const string Fire = "Fire";
        public const string Ice = "Ice";
        public const string Lightning = "Lightning";
        public const string Wind = "Wind";
        public const string Earth = "Earth";
        public const string Water = "Water";
        public const string Healing = "Healing";
        public const string Shadow = "Shadow";
        public const string Aether = "Aether";
    }

    /// <summary>
    /// Valid power levels.
    /// </summary>
    public static class PowerLevels
    {
        public const string Weak = "Weak";
        public const string Moderate = "Moderate";
        public const string Strong = "Strong";
        public const string Devastating = "Devastating";
        public const string Catastrophic = "Catastrophic";
    }
}
