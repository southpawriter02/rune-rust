// ==============================================================================
// v0.38.8: Status Effects & Condition Descriptors
// StatusEffectSourceModifier.cs
// ==============================================================================
// Purpose: Source-specific variations (beast bite vs. toxic haze)
// Usage: Additional flavor based on how the status effect was applied
// Integration: Layered with base descriptors for richer narratives
// ==============================================================================

namespace RuneAndRust.Core.StatusEffectFlavor;

/// <summary>
/// Provides source-specific variations for status effect descriptors.
/// Allows different flavor text based on whether a poison came from a beast bite,
/// toxic haze, or weapon coating.
/// </summary>
public class StatusEffectSourceModifier
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this modifier.
    /// </summary>
    public int ModifierId { get; set; }

    // ==================== SOURCE CLASSIFICATION ====================

    /// <summary>
    /// Which status effect this modifies.
    /// Values: Burning, Bleeding, Poisoned, etc.
    /// </summary>
    public string EffectType { get; set; } = string.Empty;

    /// <summary>
    /// Specific source type.
    /// Values: BeastBite, ToxicHaze, WeaponCoated, etc.
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    // ==================== SOURCE DETAILS ====================

    /// <summary>
    /// Enemy archetype that applied the effect.
    /// Values: Servitor, Forlorn, Corrupted_Dvergr, Blight_Touched_Beast,
    ///         Aether_Wraith, NULL
    /// </summary>
    public string? EnemyArchetype { get; set; }

    /// <summary>
    /// Weapon type that applied the effect.
    /// Values: TwoHanded, OneHanded, Bow, Crossbow, NULL
    /// </summary>
    public string? WeaponType { get; set; }

    /// <summary>
    /// Environmental hazard that applied the effect.
    /// Values: Lava, BrokenPipe, CollapsingCeiling, JaggedMetal, BrokenGlass, NULL
    /// </summary>
    public string? EnvironmentalHazard { get; set; }

    // ==================== BIOME CONTEXT ====================

    /// <summary>
    /// Biome-specific source modifier.
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim, NULL (any)
    /// </summary>
    public string? BiomeName { get; set; }

    // ==================== MODIFIER TEXT ====================

    /// <summary>
    /// Text to prepend to the base descriptor.
    /// Example: "The corrupted venom " (prepended to "...burns through your veins")
    /// </summary>
    public string? ModifierPrefix { get; set; }

    /// <summary>
    /// Text to append to the base descriptor.
    /// Example: "...you're burning! The Muspelheim flames are relentless!"
    /// </summary>
    public string? ModifierSuffix { get; set; }

    /// <summary>
    /// Complete replacement descriptor (optional).
    /// If provided, replaces the base descriptor entirely.
    /// </summary>
    public string? ReplacementText { get; set; }

    // ==================== METADATA ====================

    /// <summary>
    /// Probability weight for random selection.
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this modifier is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization.
    /// </summary>
    public string? Tags { get; set; }
}
