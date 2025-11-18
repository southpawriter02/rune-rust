// ==============================================================================
// v0.38.9: Perception & Examination Descriptors
// PerceptionCheckDescriptor.cs
// ==============================================================================
// Purpose: Hidden element detection (traps, secrets, caches)
// Usage: Triggered on successful Perception/WITS checks
// ==============================================================================

namespace RuneAndRust.Core.ExaminationFlavor;

/// <summary>
/// Represents a perception check result descriptor.
/// Describes successful detection of hidden elements (traps, doors, caches).
/// </summary>
public class PerceptionCheckDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Type of hidden element detected.
    /// Values: HiddenTrap, SecretDoor, HiddenCache, AmbushPoint,
    ///         WeakStructure, RunicInscription, RecentActivity
    /// </summary>
    public string DetectionType { get; set; } = string.Empty;

    /// <summary>
    /// Success level of the check.
    /// Values: Success (basic DC), ExpertSuccess (high DC)
    /// </summary>
    public string SuccessLevel { get; set; } = string.Empty;

    /// <summary>
    /// Difficulty class for this detection.
    /// Common values: 12, 15, 18, 20, 22
    /// </summary>
    public int? DifficultyClass { get; set; }

    // ==================== CONTEXT ====================

    /// <summary>
    /// Biome-specific detection cues.
    /// NULL for any biome.
    /// </summary>
    public string? BiomeName { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// Detection success narrative.
    ///
    /// Available variables:
    /// - {HiddenElement}, {DetectionCue}
    /// - {Player}, {WITS}
    /// - {Biome}, {Location}
    ///
    /// Example: "Your trained eye catches a discrepancy—a pressure plate!"
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    /// <summary>
    /// Additional lore/context for expert-level success.
    /// NULL for basic success.
    /// </summary>
    public string? ExpertInsight { get; set; }

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
    /// Valid detection types.
    /// </summary>
    public static class DetectionTypes
    {
        public const string HiddenTrap = "HiddenTrap";
        public const string SecretDoor = "SecretDoor";
        public const string HiddenCache = "HiddenCache";
        public const string AmbushPoint = "AmbushPoint";
        public const string WeakStructure = "WeakStructure";
        public const string RunicInscription = "RunicInscription";
        public const string RecentActivity = "RecentActivity";
    }

    /// <summary>
    /// Valid success levels.
    /// </summary>
    public static class SuccessLevels
    {
        public const string Success = "Success";               // Basic DC
        public const string ExpertSuccess = "ExpertSuccess";   // High DC
    }
}
