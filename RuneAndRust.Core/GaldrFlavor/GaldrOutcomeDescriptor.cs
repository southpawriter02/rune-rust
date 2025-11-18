// ==============================================================================
// v0.38.7: Ability & Galdr Flavor Text
// GaldrOutcomeDescriptor.cs
// ==============================================================================
// Purpose: Success/failure narratives for ability resolution
// Usage: Describes the result of an ability activation
// Integration: Works with combat outcomes and status effects
// ==============================================================================

namespace RuneAndRust.Core.GaldrFlavor;

/// <summary>
/// Represents the outcome narrative of a Galdr casting or ability activation.
/// Describes what happens when the spell hits, misses, or has special effects.
/// </summary>
public class GaldrOutcomeDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this outcome descriptor.
    /// </summary>
    public int OutcomeId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Specific ability this outcome applies to.
    /// Values: FlameBolt, FrostLance, LightningBolt, HealingChant, etc.
    /// </summary>
    public string AbilityName { get; set; } = string.Empty;

    /// <summary>
    /// Type of outcome.
    /// Values: Hit, Miss, CriticalHit, PartialEffect, FullEffect,
    ///         Resisted, Immune, Amplified
    /// </summary>
    public string OutcomeType { get; set; } = string.Empty;

    /// <summary>
    /// Number of successes rolled (for graduated outcomes).
    /// Values: 1-2 (Minor), 3-4 (Solid), 5+ (Exceptional), NULL (any)
    /// </summary>
    public int? SuccessCount { get; set; }

    // ==================== TARGET INFORMATION ====================

    /// <summary>
    /// Type of target affected.
    /// Values: Enemy, Self, Ally, Area, Environment
    /// </summary>
    public string? TargetType { get; set; }

    /// <summary>
    /// Enemy archetype (if target is an enemy).
    /// Values: Servitor, Forlorn, Corrupted_Dvergr, Blight_Touched_Beast,
    ///         Aether_Wraith, NULL
    /// </summary>
    public string? EnemyArchetype { get; set; }

    // ==================== EFFECT CATEGORY ====================

    /// <summary>
    /// Category of effect produced.
    /// Values: Damage, Healing, Buff, Debuff, Control, Utility, Summon
    /// </summary>
    public string? EffectCategory { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// Outcome narrative with {Variable} placeholders.
    ///
    /// Examples:
    /// - Hit: "Fire engulfs the {Target}, scorching {Armor_Location}! [Damage: {Damage}]"
    /// - Miss: "Your flames dissipate before reaching the {Target}!"
    /// - CriticalHit: "The {Rune} rune erupts! {Target} is consumed by {Element}! [{Damage} damage]"
    /// - Healing: "Berkanan's warmth flows through you, mending wounds. [+{Healing} HP]"
    /// - Resisted: "The {Target} shrugs off your {Ability}, barely phased!"
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
    /// Example: ["Dramatic", "Subtle", "Horrifying", "Triumphant"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid outcome types.
    /// </summary>
    public static class OutcomeTypes
    {
        public const string Hit = "Hit";
        public const string Miss = "Miss";
        public const string CriticalHit = "CriticalHit";
        public const string PartialEffect = "PartialEffect";
        public const string FullEffect = "FullEffect";
        public const string Resisted = "Resisted";
        public const string Immune = "Immune";
        public const string Amplified = "Amplified";
    }

    /// <summary>
    /// Valid target types.
    /// </summary>
    public static class TargetTypes
    {
        public const string Enemy = "Enemy";
        public const string Self = "Self";
        public const string Ally = "Ally";
        public const string Area = "Area";
        public const string Environment = "Environment";
    }

    /// <summary>
    /// Valid effect categories.
    /// </summary>
    public static class EffectCategories
    {
        public const string Damage = "Damage";
        public const string Healing = "Healing";
        public const string Buff = "Buff";
        public const string Debuff = "Debuff";
        public const string Control = "Control";
        public const string Utility = "Utility";
        public const string Summon = "Summon";
    }
}
