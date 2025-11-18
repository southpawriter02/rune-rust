// ==============================================================================
// v0.38.8: Status Effects & Condition Descriptors
// StatusEffectSeverityProfile.cs
// ==============================================================================
// Purpose: Define severity thresholds and characteristics per effect type
// Usage: Maps damage ranges to severity levels (e.g., 1-2 = Minor, 3-5 = Moderate)
// ==============================================================================

namespace RuneAndRust.Core.StatusEffectFlavor;

/// <summary>
/// Defines severity thresholds and characteristics for a specific status effect.
/// Used to select appropriate descriptors based on effect strength.
/// </summary>
public class StatusEffectSeverityProfile
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this profile.
    /// </summary>
    public int ProfileId { get; set; }

    // ==================== EFFECT CLASSIFICATION ====================

    /// <summary>
    /// Status effect type this profile applies to.
    /// Values: Burning, Bleeding, Poisoned, etc.
    /// </summary>
    public string EffectType { get; set; } = string.Empty;

    /// <summary>
    /// Severity level for this profile.
    /// Values: Minor, Moderate, Severe, Catastrophic
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    // ==================== DAMAGE THRESHOLDS (for DoT effects) ====================

    /// <summary>
    /// Minimum damage per turn for this severity level.
    /// NULL for non-DoT effects.
    /// </summary>
    public int? DamagePerTurnMin { get; set; }

    /// <summary>
    /// Maximum damage per turn for this severity level.
    /// NULL for non-DoT effects.
    /// </summary>
    public int? DamagePerTurnMax { get; set; }

    // ==================== STACK THRESHOLDS (for stackable effects) ====================

    /// <summary>
    /// Minimum stack count for this severity level.
    /// Used for effects like Bleeding, BlightCorruption.
    /// NULL for non-stackable effects.
    /// </summary>
    public int? StackCountMin { get; set; }

    /// <summary>
    /// Maximum stack count for this severity level.
    /// NULL for non-stackable effects.
    /// </summary>
    public int? StackCountMax { get; set; }

    // ==================== DURATION THRESHOLDS (for timed effects) ====================

    /// <summary>
    /// Minimum duration (turns) for this severity level.
    /// NULL if duration doesn't affect severity.
    /// </summary>
    public int? DurationMin { get; set; }

    /// <summary>
    /// Maximum duration (turns) for this severity level.
    /// NULL if duration doesn't affect severity.
    /// </summary>
    public int? DurationMax { get; set; }

    // ==================== NARRATIVE CUES ====================

    /// <summary>
    /// Narrative guidance for this severity level.
    /// Example: "Manageable pain" vs "Excruciating agony"
    /// </summary>
    public string? IntensityDescription { get; set; }

    /// <summary>
    /// Urgency level for this severity.
    /// Values: Low, Medium, High, Critical
    /// </summary>
    public string? UrgencyLevel { get; set; }

    // ==================== METADATA ====================

    /// <summary>
    /// Whether this profile is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Urgency levels for severity profiles.
    /// </summary>
    public static class UrgencyLevels
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string Critical = "Critical";
    }
}
