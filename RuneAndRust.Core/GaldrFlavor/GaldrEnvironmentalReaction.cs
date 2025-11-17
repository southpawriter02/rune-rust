// ==============================================================================
// v0.38.7: Ability & Galdr Flavor Text
// GaldrEnvironmentalReaction.cs
// ==============================================================================
// Purpose: How biomes react to Galdr casting
// Pattern: Similar to EnvironmentalCombatModifier from v0.38.6
// Usage: Additional flavor for casting in specific locations
// ==============================================================================

namespace RuneAndRust.Core.GaldrFlavor;

/// <summary>
/// Represents how the environment reacts to Galdr casting.
/// Provides biome-specific flavor for magical manifestations.
/// </summary>
public class GaldrEnvironmentalReaction
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this environmental reaction.
    /// </summary>
    public int ReactionId { get; set; }

    // ==================== LOCATION ====================

    /// <summary>
    /// Biome where this reaction occurs.
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim
    /// </summary>
    public string BiomeName { get; set; } = string.Empty;

    // ==================== REACTION TYPE ====================

    /// <summary>
    /// Type of environmental reaction to magic.
    /// Values: Resonance, Interference, Amplification, Distortion, Harmony, Rejection
    /// </summary>
    public string ReactionType { get; set; } = string.Empty;

    // ==================== MAGIC INTERACTION ====================

    /// <summary>
    /// Rune school that triggers this reaction (NULL = all schools).
    /// Values: Fehu, Thurisaz, Ansuz, Raido, Hagalaz, Naudiz, Isa, Jera,
    ///         Tiwaz, Berkanan, Mannaz, Laguz, NULL
    /// </summary>
    public string? RuneSchool { get; set; }

    /// <summary>
    /// Element that triggers this reaction (NULL = all elements).
    /// Values: Fire, Ice, Lightning, Wind, Earth, Water, Healing, Shadow, Aether, NULL
    /// </summary>
    public string? Element { get; set; }

    // ==================== TRIGGER CONDITIONS ====================

    /// <summary>
    /// Probability this reaction triggers (0.0 - 1.0).
    /// Default: 0.30 (30% chance)
    /// </summary>
    public float TriggerChance { get; set; } = 0.30f;

    /// <summary>
    /// Minimum power level required to trigger this reaction.
    /// Values: NULL (any), Weak, Moderate, Strong, Devastating
    /// </summary>
    public string? PowerLevelMin { get; set; }

    // ==================== REACTION DESCRIPTION ====================

    /// <summary>
    /// Environmental flavor text describing the reaction.
    ///
    /// Examples:
    /// - The_Roots + Fire: "Steam erupts from rusty pipes as your Galdr ignites the humid air."
    /// - Muspelheim + Ice: "The Inferno fights your ice! Steam explodes as cold meets heat!"
    /// - Alfheim + Any: "The Cursed Choir harmonizes with your Galdr—or does it corrupt it?"
    /// - Niflheim + Lightning: "Your lightning arcs between ice formations, amplified by
    ///                         crystalline structures!"
    /// - Jotunheim + Earth: "Stone resonates with your chant, ancient runes glowing faintly
    ///                      in response."
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    // ==================== METADATA ====================

    /// <summary>
    /// Probability weight for random selection (1.0 = default).
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this reaction is active and can be triggered.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization.
    /// Example: ["Dramatic", "Subtle", "Dangerous", "Beautiful"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid reaction types.
    /// </summary>
    public static class ReactionTypes
    {
        public const string Resonance = "Resonance";       // Environment enhances magic
        public const string Interference = "Interference";  // Environment hinders magic
        public const string Amplification = "Amplification"; // Environment strengthens magic
        public const string Distortion = "Distortion";     // Environment warps magic
        public const string Harmony = "Harmony";           // Environment cooperates with magic
        public const string Rejection = "Rejection";       // Environment opposes magic
    }

    /// <summary>
    /// Valid biome names.
    /// </summary>
    public static class BiomeNames
    {
        public const string TheRoots = "The_Roots";
        public const string Muspelheim = "Muspelheim";
        public const string Niflheim = "Niflheim";
        public const string Alfheim = "Alfheim";
        public const string Jotunheim = "Jotunheim";
    }
}
