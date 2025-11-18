// ==============================================================================
// v0.38.8: Status Effects & Condition Descriptors
// StatusEffectEnvironmentalContext.cs
// ==============================================================================
// Purpose: Biome-specific status effect variations
// Usage: How status effects manifest differently in each realm
// Example: Burning in Muspelheim vs. Burning in Niflheim
// ==============================================================================

namespace RuneAndRust.Core.StatusEffectFlavor;

/// <summary>
/// Defines how status effects manifest differently based on biome context.
/// Example: Burning in Muspelheim's volcanic heat vs. Niflheim's freezing cold.
/// </summary>
public class StatusEffectEnvironmentalContext
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this environmental context.
    /// </summary>
    public int ContextId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Status effect type.
    /// Values: Burning, Bleeding, Poisoned, etc.
    /// </summary>
    public string EffectType { get; set; } = string.Empty;

    /// <summary>
    /// Biome where this context applies.
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim
    /// </summary>
    public string BiomeName { get; set; } = string.Empty;

    /// <summary>
    /// Application context (optional filter).
    /// Values: OnApply, OnTick, OnExpire, NULL (all contexts)
    /// </summary>
    public string? ApplicationContext { get; set; }

    // ==================== ENVIRONMENTAL MODIFIER ====================

    /// <summary>
    /// How the biome affects the status effect.
    ///
    /// Available variables:
    /// - {Effect}, {Biome}, {Target}
    /// - {Damage}, {Duration}, {StackCount}
    ///
    /// Example: "The volcanic heat intensifies the flames!"
    /// Example: "The eternal ice slows the bleeding to a trickle..."
    /// </summary>
    public string EnvironmentalDescriptor { get; set; } = string.Empty;

    // ==================== MECHANICAL MODIFIERS (optional) ====================

    /// <summary>
    /// Multiplier for effect duration in this biome.
    /// Example: 1.5 = 50% longer, 0.75 = 25% shorter
    /// NULL = no modifier
    /// </summary>
    public float? DurationModifier { get; set; }

    /// <summary>
    /// Multiplier for effect damage in this biome.
    /// Example: 1.25 = 25% more damage, 0.5 = 50% less damage
    /// NULL = no modifier
    /// </summary>
    public float? DamageModifier { get; set; }

    // ==================== METADATA ====================

    /// <summary>
    /// Chance for this environmental flavor to appear (0.0-1.0).
    /// Default: 0.30 (30% chance)
    /// </summary>
    public float TriggerChance { get; set; } = 0.30f;

    /// <summary>
    /// Probability weight for random selection.
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this context is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization.
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid biome names.
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
