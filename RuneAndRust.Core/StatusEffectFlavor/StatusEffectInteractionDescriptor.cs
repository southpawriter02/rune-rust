// ==============================================================================
// v0.38.8: Status Effects & Condition Descriptors
// StatusEffectInteractionDescriptor.cs
// ==============================================================================
// Purpose: Flavor text for status effect interactions
// Usage: When multiple status effects interact, combine, or cancel each other
// Integration: Triggered by AdvancedStatusEffectService interaction system
// ==============================================================================

namespace RuneAndRust.Core.StatusEffectFlavor;

/// <summary>
/// Describes the interaction between two status effects (e.g., Burning + Freezing).
/// Used when effects combine, cancel, or transform into new effects.
/// </summary>
public class StatusEffectInteractionDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this interaction descriptor.
    /// </summary>
    public int InteractionId { get; set; }

    // ==================== INTERACTION CLASSIFICATION ====================

    /// <summary>
    /// First status effect in the interaction.
    /// </summary>
    public string EffectType1 { get; set; } = string.Empty;

    /// <summary>
    /// Second status effect in the interaction.
    /// </summary>
    public string EffectType2 { get; set; } = string.Empty;

    /// <summary>
    /// Type of interaction between the two effects.
    /// Values: Suppress (cancels out), Amplify (enhances effect),
    ///         Transform (creates new effect), Synergy (bonus effect),
    ///         Neutralize (both removed)
    /// </summary>
    public string InteractionType { get; set; } = string.Empty;

    // ==================== RESULT ====================

    /// <summary>
    /// New effect created if transformation occurs.
    /// NULL if suppression/neutralization.
    /// </summary>
    public string? ResultEffect { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The flavor text template describing the interaction.
    ///
    /// Available variables:
    /// - {Target}, {Effect1}, {Effect2}, {ResultEffect}
    /// - {Damage}, {Duration}, {StackCount}
    ///
    /// Example: "The {Effect1} and {Effect2} cancel each other out!"
    /// Example: "The flames and ice clash—steam hisses as both effects neutralize!"
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
    /// Valid interaction types.
    /// </summary>
    public static class InteractionTypes
    {
        public const string Suppress = "Suppress";           // One effect cancels another
        public const string Amplify = "Amplify";             // One effect enhances another
        public const string Transform = "Transform";         // Creates new effect
        public const string Synergy = "Synergy";             // Bonus effect
        public const string Neutralize = "Neutralize";       // Both removed
    }
}
