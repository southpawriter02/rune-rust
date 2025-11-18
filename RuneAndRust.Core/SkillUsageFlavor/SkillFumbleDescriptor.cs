// ==============================================================================
// v0.38.10: Skill Usage Flavor Text
// SkillFumbleDescriptor.cs
// ==============================================================================
// Purpose: Fumble and catastrophic failure descriptors for skill checks
// Pattern: Follows StatusEffectDescriptor structure
// Integration: Used by SkillUsageFlavorTextService for fumble consequences
// ==============================================================================

namespace RuneAndRust.Core.SkillUsageFlavor;

/// <summary>
/// Represents a fumble consequence descriptor for skill checks.
/// Describes catastrophic failures and their narrative/mechanical consequences.
/// v0.38.10: Skill Usage Flavor Text
/// </summary>
public class SkillFumbleDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int FumbleId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Skill type that fumbled.
    /// Values: SystemBypass, Acrobatics, WastelandSurvival, Rhetoric
    /// </summary>
    public string SkillType { get; set; } = string.Empty;

    /// <summary>
    /// Specific action that fumbled.
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Type of fumble consequence.
    /// Values: ToolBreakage, AlarmTriggered, TrapActivated, InjuryTaken,
    ///         ItemLost, DetectedByEnemy, ResourceWasted, StructuralCollapse,
    ///         SocialConsequence, TimeWasted, Poisoned
    /// </summary>
    public string ConsequenceType { get; set; } = string.Empty;

    // ==================== SEVERITY ====================

    /// <summary>
    /// Severity of the fumble consequence.
    /// Values: Minor, Moderate, Severe, Catastrophic
    /// </summary>
    public string Severity { get; set; } = "Moderate";

    // ==================== MECHANICAL EFFECTS ====================

    /// <summary>
    /// Damage dealt by fumble (if any).
    /// Examples: "1d6", "2d6", "3d10", NULL
    /// </summary>
    public string? DamageFormula { get; set; }

    /// <summary>
    /// Status effect applied by fumble (if any).
    /// Examples: "Poisoned", "Stunned", "Bleeding", "Burning", NULL
    /// </summary>
    public string? StatusEffectApplied { get; set; }

    /// <summary>
    /// DC modifier for next attempt (if any).
    /// Positive values increase difficulty.
    /// Example: +2 (lock jammed), +5 (mechanism damaged)
    /// </summary>
    public int? NextAttemptDCModifier { get; set; }

    /// <summary>
    /// Time penalty in minutes (if any).
    /// Example: 10 (lost 10 minutes), 60 (lost 1 hour)
    /// </summary>
    public int? TimePenaltyMinutes { get; set; }

    /// <summary>
    /// Whether this fumble prevents further attempts.
    /// Example: true (lock destroyed), false (can retry)
    /// </summary>
    public bool PreventsRetry { get; set; } = false;

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The flavor text describing the fumble consequence.
    ///
    /// Available variables:
    /// - CHARACTER: {Player}, {Character}
    /// - CONSEQUENCE: {Damage}, {StatusEffect}, {DCModifier}, {TimeLost}
    /// - TOOL: {Tool}, {ToolType}
    /// - ENVIRONMENT: {Alarm}, {Trap}, {Structure}
    ///
    /// Example: "Your pick snaps off inside the lock! The broken piece jams the mechanism."
    /// Example: "The entire section of wall you're climbing collapses! [5d6 damage, buried in debris]"
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
    /// Example: ["Dangerous", "Comedic", "Tragic", "Equipment_Loss"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid consequence types.
    /// </summary>
    public static class ConsequenceTypes
    {
        // Equipment/Tool Consequences
        public const string ToolBreakage = "ToolBreakage";
        public const string ItemLost = "ItemLost";
        public const string ResourceWasted = "ResourceWasted";

        // Detection/Alarm Consequences
        public const string AlarmTriggered = "AlarmTriggered";
        public const string DetectedByEnemy = "DetectedByEnemy";

        // Trap/Hazard Consequences
        public const string TrapActivated = "TrapActivated";
        public const string StructuralCollapse = "StructuralCollapse";
        public const string InjuryTaken = "InjuryTaken";
        public const string Poisoned = "Poisoned";

        // Social/Narrative Consequences
        public const string SocialConsequence = "SocialConsequence";
        public const string ReputationLoss = "ReputationLoss";

        // Time/Progress Consequences
        public const string TimeWasted = "TimeWasted";
        public const string ProgressLost = "ProgressLost";

        // Environmental Consequences
        public const string EnvironmentalHazard = "EnvironmentalHazard";
        public const string BlightCorruption = "BlightCorruption";
    }

    /// <summary>
    /// Valid severity levels.
    /// </summary>
    public static class SeverityLevels
    {
        public const string Minor = "Minor";           // Annoying but recoverable
        public const string Moderate = "Moderate";     // Setback with real consequences
        public const string Severe = "Severe";         // Major problem
        public const string Catastrophic = "Catastrophic"; // Disaster
    }
}
