// ==============================================================================
// v0.38.8: Status Effects & Condition Descriptors
// StatusEffectDescriptor.cs
// ==============================================================================
// Purpose: Core status effect flavor text for application, ticks, and expiry
// Pattern: Follows CombatActionDescriptor and GaldrActionDescriptor structure
// Integration: Used by StatusEffectFlavorTextService to generate affliction narratives
// ==============================================================================

namespace RuneAndRust.Core.StatusEffectFlavor;

/// <summary>
/// Represents a status effect descriptor for generating flavor text.
/// Describes how status effects are applied, how they feel during ticks, and how they expire.
/// </summary>
public class StatusEffectDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Status effect type.
    /// Values: Burning, Bleeding, Poisoned, Stunned, Slowed, Weakened, Blinded,
    ///         Confused, Corroding, Freezing, Haste, Strengthened, Protected,
    ///         Regenerating, BlightCorruption, Cursed
    /// </summary>
    public string EffectType { get; set; } = string.Empty;

    /// <summary>
    /// Application context for when this descriptor is used.
    /// Values: OnApply (initial application), OnTick (each turn),
    ///         OnExpire (natural expiration), OnRemove (active removal/cleanse)
    /// </summary>
    public string ApplicationContext { get; set; } = string.Empty;

    // ==================== SEVERITY ====================

    /// <summary>
    /// Severity level of the status effect.
    /// Values: Minor (1-2 damage/turn or weak effects),
    ///         Moderate (3-5 damage/turn or medium effects),
    ///         Severe (6+ damage/turn or strong effects),
    ///         Catastrophic (extreme effects),
    ///         NULL (for non-severity-based effects)
    /// </summary>
    public string? Severity { get; set; }

    // ==================== SOURCE CONTEXT ====================

    /// <summary>
    /// Type of source that applied the status effect.
    /// Values: EnemyAttack, Environmental, GaldrBackfire, WeaponCoated, BeastBite,
    ///         ToxicHaze, Lightning, ConcussiveForce, AlfheimTear, ForlornTouch,
    ///         Natural, Antidote, Bandaged, NULL (generic)
    /// </summary>
    public string? SourceType { get; set; }

    /// <summary>
    /// Specific detail about the source (e.g., "Fire", "Slashing", "Piercing", "Acid", "Venom").
    /// </summary>
    public string? SourceDetail { get; set; }

    // ==================== TARGET INFORMATION ====================

    /// <summary>
    /// Target type for this descriptor.
    /// Values: Player, Enemy, Ally, NULL (all)
    /// </summary>
    public string? TargetType { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The flavor text template with {Variable} placeholders.
    ///
    /// Available variables:
    /// - CHARACTER: {Target}, {Enemy}, {Player}
    /// - EFFECT: {EffectType}, {Damage}, {Duration}, {StackCount}, {Severity}
    /// - SOURCE: {Source}, {SourceDetail}, {Weapon}
    /// - LOCATION: {Location}, {Vital_Location}, {Biome}, {Environment_Feature}
    /// - MANIFESTATION: {BlightEffect}, {ParadoxLevel}, {CorruptionStacks}
    ///
    /// Example: "The {Enemy}'s flames catch on your clothing—you're burning!"
    /// Example: "You're hemorrhaging—the world swims as blood loss takes its toll!"
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
    /// Example: ["Verbose", "Concise", "Dramatic"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid status effect types.
    /// </summary>
    public static class EffectTypes
    {
        // Damage Over Time
        public const string Burning = "Burning";
        public const string Bleeding = "Bleeding";
        public const string Poisoned = "Poisoned";
        public const string Corroding = "Corroding";
        public const string Freezing = "Freezing";

        // Control Debuffs
        public const string Stunned = "Stunned";
        public const string Slowed = "Slowed";
        public const string Blinded = "Blinded";
        public const string Confused = "Confused";

        // Stat Debuffs
        public const string Weakened = "Weakened";

        // Buffs
        public const string Haste = "Haste";
        public const string Strengthened = "Strengthened";
        public const string Protected = "Protected";
        public const string Regenerating = "Regenerating";

        // Special
        public const string BlightCorruption = "BlightCorruption";
        public const string Cursed = "Cursed";
    }

    /// <summary>
    /// Valid application contexts.
    /// </summary>
    public static class ApplicationContexts
    {
        public const string OnApply = "OnApply";       // Initial application
        public const string OnTick = "OnTick";         // Each turn
        public const string OnExpire = "OnExpire";     // Natural expiration
        public const string OnRemove = "OnRemove";     // Active removal/cleanse
    }

    /// <summary>
    /// Valid severity levels.
    /// </summary>
    public static class SeverityLevels
    {
        public const string Minor = "Minor";           // 1-2 damage/turn or weak effects
        public const string Moderate = "Moderate";     // 3-5 damage/turn or medium effects
        public const string Severe = "Severe";         // 6+ damage/turn or strong effects
        public const string Catastrophic = "Catastrophic"; // Extreme effects
    }

    /// <summary>
    /// Valid source types.
    /// </summary>
    public static class SourceTypes
    {
        // Attack Sources
        public const string EnemyAttack = "EnemyAttack";
        public const string Environmental = "Environmental";
        public const string GaldrBackfire = "GaldrBackfire";
        public const string WeaponCoated = "WeaponCoated";

        // Specific Sources
        public const string BeastBite = "BeastBite";
        public const string ToxicHaze = "ToxicHaze";
        public const string Lightning = "Lightning";
        public const string ConcussiveForce = "ConcussiveForce";
        public const string AlfheimTear = "AlfheimTear";
        public const string ForlornTouch = "ForlornTouch";

        // Removal Sources
        public const string Natural = "Natural";
        public const string Antidote = "Antidote";
        public const string Bandaged = "Bandaged";
    }

    /// <summary>
    /// Valid target types.
    /// </summary>
    public static class TargetTypes
    {
        public const string Player = "Player";
        public const string Enemy = "Enemy";
        public const string Ally = "Ally";
    }
}
