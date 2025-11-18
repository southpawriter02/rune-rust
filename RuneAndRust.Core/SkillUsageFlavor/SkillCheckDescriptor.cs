// ==============================================================================
// v0.38.10: Skill Usage Flavor Text
// SkillCheckDescriptor.cs
// ==============================================================================
// Purpose: Core skill check flavor text for attempt, success, and failure descriptors
// Pattern: Follows StatusEffectDescriptor and ExaminationDescriptor structure
// Integration: Used by SkillUsageFlavorTextService to generate skill check narratives
// ==============================================================================

namespace RuneAndRust.Core.SkillUsageFlavor;

/// <summary>
/// Represents a skill check descriptor for generating flavor text.
/// Describes skill usage attempts, successes by degree, and failures.
/// v0.38.10: Skill Usage Flavor Text
/// </summary>
public class SkillCheckDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Skill type being used.
    /// Values: SystemBypass, Acrobatics, WastelandSurvival, Rhetoric
    /// </summary>
    public string SkillType { get; set; } = string.Empty;

    /// <summary>
    /// Specific action within the skill.
    /// SystemBypass: Lockpicking, TerminalHacking, TrapDisarm
    /// Acrobatics: Climbing, Leaping, Stealth
    /// WastelandSurvival: Tracking, Foraging, Navigation
    /// Rhetoric: Persuasion, Deception, Intimidation
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Check phase for when this descriptor is used.
    /// Values: Attempt (setup/context), Success, Failure, CriticalSuccess
    /// </summary>
    public string CheckPhase { get; set; } = string.Empty;

    // ==================== SUCCESS/FAILURE DEGREE ====================

    /// <summary>
    /// Degree of success or failure.
    /// Values:
    /// - Minimal (1-2 over DC / 1-2 under DC)
    /// - Solid (3-5 over DC / 3-5 under DC)
    /// - Critical (6+ over DC)
    /// - NULL (for Attempt phase or non-graded results)
    /// </summary>
    public string? ResultDegree { get; set; }

    // ==================== ENVIRONMENTAL CONTEXT ====================

    /// <summary>
    /// Environmental difficulty modifier.
    /// Values: Normal, Damaged, Complex, Dangerous, Glitched, NULL
    /// Lockpicking: Normal, Corroded, Complex (Jötun), Damaged
    /// Climbing: Normal, Corroded, Dangerous (height), Glitched (Blight)
    /// Tracking: Normal, Old_Tracks, Fresh_Tracks, Unusual_Tracks
    /// </summary>
    public string? EnvironmentalContext { get; set; }

    /// <summary>
    /// Biome-specific context (optional).
    /// Values: Muspelheim, Niflheim, Alfheim, The_Roots, NULL
    /// </summary>
    public string? BiomeContext { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The flavor text template with {Variable} placeholders.
    ///
    /// Available variables:
    /// - CHARACTER: {Player}, {Character}
    /// - SKILL: {SkillType}, {ActionType}, {DC}, {Roll}, {Margin}
    /// - TARGET: {LockType}, {Terminal}, {Gap}, {Height}, {Target}
    /// - ENVIRONMENT: {Biome}, {Condition}, {Terrain}
    /// - TIME: {Duration}, {TimeOfDay}
    /// - TOOL: {Tool}, {ToolQuality}
    ///
    /// Example: "You kneel before the lock, examining the mechanism with your picks."
    /// Example: "After several tense minutes, the lock finally yields. Your hands are cramping."
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
    /// Example: ["Verbose", "Concise", "Dramatic", "Technical"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid skill types.
    /// </summary>
    public static class SkillTypes
    {
        public const string SystemBypass = "SystemBypass";
        public const string Acrobatics = "Acrobatics";
        public const string WastelandSurvival = "WastelandSurvival";
        public const string Rhetoric = "Rhetoric";
    }

    /// <summary>
    /// Valid action types per skill.
    /// </summary>
    public static class ActionTypes
    {
        // System Bypass
        public const string Lockpicking = "Lockpicking";
        public const string TerminalHacking = "TerminalHacking";
        public const string TrapDisarm = "TrapDisarm";

        // Acrobatics
        public const string Climbing = "Climbing";
        public const string Leaping = "Leaping";
        public const string Stealth = "Stealth";

        // Wasteland Survival
        public const string Tracking = "Tracking";
        public const string Foraging = "Foraging";
        public const string Navigation = "Navigation";

        // Rhetoric
        public const string Persuasion = "Persuasion";
        public const string Deception = "Deception";
        public const string Intimidation = "Intimidation";
    }

    /// <summary>
    /// Valid check phases.
    /// </summary>
    public static class CheckPhases
    {
        public const string Attempt = "Attempt";           // Setup/context
        public const string Success = "Success";           // General success
        public const string Failure = "Failure";           // General failure
        public const string CriticalSuccess = "CriticalSuccess"; // Exceptional success
    }

    /// <summary>
    /// Valid result degrees.
    /// </summary>
    public static class ResultDegrees
    {
        public const string Minimal = "Minimal";           // 1-2 margin
        public const string Solid = "Solid";               // 3-5 margin
        public const string Critical = "Critical";         // 6+ margin
    }

    /// <summary>
    /// Valid environmental contexts.
    /// </summary>
    public static class EnvironmentalContexts
    {
        // Lockpicking
        public const string SimpleLock = "SimpleLock";
        public const string ComplexLock = "ComplexLock";
        public const string CorrodedLock = "CorrodedLock";
        public const string DamagedLock = "DamagedLock";

        // Climbing
        public const string CorrodedStructure = "CorrodedStructure";
        public const string DangerousHeight = "DangerousHeight";
        public const string GlitchedTerrain = "GlitchedTerrain";

        // Tracking
        public const string FreshTracks = "FreshTracks";
        public const string OldTracks = "OldTracks";
        public const string UnusualTracks = "UnusualTracks";

        // Foraging
        public const string RichArea = "RichArea";
        public const string DangerousArea = "DangerousArea";
        public const string ContaminatedArea = "ContaminatedArea";

        // Navigation
        public const string NormalTravel = "NormalTravel";
        public const string StormHazard = "StormHazard";
        public const string GlitchedSpace = "GlitchedSpace";

        // Stealth
        public const string ShadowyCover = "ShadowyCover";
        public const string NoisyEnvironment = "NoisyEnvironment";
        public const string OpenGround = "OpenGround";

        // Rhetoric
        public const string ReasonableRequest = "ReasonableRequest";
        public const string DifficultRequest = "DifficultRequest";
        public const string SimpleDeception = "SimpleDeception";
        public const string ComplexDeception = "ComplexDeception";
    }
}
