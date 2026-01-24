using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a hazard detection attempt in the Wasteland Survival system.
/// </summary>
/// <remarks>
/// <para>
/// HazardDetectionResult captures all information about a detected (or undetected) hazard:
/// <list type="bullet">
///   <item><description>Whether detection succeeded</description></item>
///   <item><description>The type of hazard (if identified)</description></item>
///   <item><description>Options for avoiding the hazard</description></item>
///   <item><description>Consequences if the hazard is triggered</description></item>
/// </list>
/// </para>
/// <para>
/// For passive detection (hints only), HazardDetected is true but HazardType may not
/// be fully identified—only a general sense of danger is provided.
/// </para>
/// <para>
/// Detection methods:
/// <list type="bullet">
///   <item><description>Passive: WITS ÷ 2 vs DC - hint only</description></item>
///   <item><description>Active: Wasteland Survival check - full identification</description></item>
///   <item><description>Critical (net ≥ 5): Additional context revealed</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="HazardDetected">Whether the hazard was successfully detected.</param>
/// <param name="HazardType">The type of hazard, if identified.</param>
/// <param name="AvoidanceOptions">Ways to avoid or circumvent the hazard.</param>
/// <param name="ConsequenceIfMissed">What happens if the hazard is triggered.</param>
/// <param name="DetectionMethod">How the hazard was detected (passive or active).</param>
/// <param name="NetSuccesses">Number of successes on active detection (0 for passive).</param>
/// <param name="TargetDc">The detection DC for the hazard.</param>
/// <param name="RollDetails">Optional details about the dice roll for logging/display.</param>
public readonly record struct HazardDetectionResult(
    bool HazardDetected,
    DetectableHazardType? HazardType,
    IReadOnlyList<string> AvoidanceOptions,
    string ConsequenceIfMissed,
    DetectionMethod DetectionMethod,
    int NetSuccesses,
    int TargetDc = 0,
    string? RollDetails = null)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the hazard type was fully identified (not just sensed).
    /// </summary>
    /// <remarks>
    /// Passive detection provides awareness but not identification.
    /// Active detection with success provides full identification.
    /// </remarks>
    public bool FullyIdentified => HazardDetected &&
        DetectionMethod is Enums.DetectionMethod.ActiveInvestigation or Enums.DetectionMethod.AreaSweep;

    /// <summary>
    /// Gets whether this was a critical detection (additional context available).
    /// </summary>
    /// <remarks>
    /// Critical detection occurs when net successes ≥ 5 on an active investigation.
    /// Critical detection provides additional context on how to disable or exploit the hazard.
    /// </remarks>
    public bool IsCritical => NetSuccesses >= 5;

    /// <summary>
    /// Gets whether the detection was only a passive hint (not full identification).
    /// </summary>
    public bool IsHintOnly => HazardDetected && DetectionMethod == Enums.DetectionMethod.PassivePerception;

    /// <summary>
    /// Gets whether any detection occurred (either hint or full identification).
    /// </summary>
    public bool HasAwareness => HazardDetected;

    /// <summary>
    /// Gets the margin by which the detection check succeeded or failed.
    /// </summary>
    /// <remarks>
    /// Only meaningful for active detection attempts.
    /// Positive values indicate success margin (how much above DC).
    /// Negative values indicate failure margin (how much below DC).
    /// </remarks>
    public int Margin => NetSuccesses - TargetDc;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result for a passively sensed hazard (hint only).
    /// </summary>
    /// <param name="hazardType">The actual hazard type (not revealed to player).</param>
    /// <param name="targetDc">The detection DC for the hazard.</param>
    /// <returns>A detection result indicating passive awareness without identification.</returns>
    /// <remarks>
    /// Passive detection provides only a vague sense of danger.
    /// The hazard type is stored internally but not revealed to the player.
    /// </remarks>
    public static HazardDetectionResult PassiveHint(DetectableHazardType hazardType, int targetDc) =>
        new(
            HazardDetected: true,
            HazardType: null, // Type not revealed for passive
            AvoidanceOptions: new[] { "Investigate the area to learn more about the danger" },
            ConsequenceIfMissed: "Unknown danger - investigate to learn more",
            DetectionMethod: Enums.DetectionMethod.PassivePerception,
            NetSuccesses: 0,
            TargetDc: targetDc,
            RollDetails: $"Passive perception triggered for {hazardType.GetDisplayName()} (DC {targetDc})");

    /// <summary>
    /// Creates a result for an actively detected hazard.
    /// </summary>
    /// <param name="hazardType">The type of hazard detected.</param>
    /// <param name="avoidanceOptions">Options for avoiding the hazard.</param>
    /// <param name="consequence">What happens if the hazard is triggered.</param>
    /// <param name="netSuccesses">Net successes from the skill check.</param>
    /// <param name="targetDc">The detection DC for the hazard.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A detection result indicating successful identification.</returns>
    public static HazardDetectionResult ActiveSuccess(
        DetectableHazardType hazardType,
        IReadOnlyList<string> avoidanceOptions,
        string consequence,
        int netSuccesses,
        int targetDc,
        string? rollDetails = null) =>
        new(
            HazardDetected: true,
            HazardType: hazardType,
            AvoidanceOptions: avoidanceOptions,
            ConsequenceIfMissed: consequence,
            DetectionMethod: Enums.DetectionMethod.ActiveInvestigation,
            NetSuccesses: netSuccesses,
            TargetDc: targetDc,
            RollDetails: rollDetails);

    /// <summary>
    /// Creates a result for an area sweep detection.
    /// </summary>
    /// <param name="hazardType">The type of hazard detected.</param>
    /// <param name="avoidanceOptions">Options for avoiding the hazard.</param>
    /// <param name="consequence">What happens if the hazard is triggered.</param>
    /// <param name="netSuccesses">Net successes from the skill check.</param>
    /// <param name="targetDc">The detection DC for the hazard.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A detection result from an area sweep.</returns>
    public static HazardDetectionResult AreaSweepSuccess(
        DetectableHazardType hazardType,
        IReadOnlyList<string> avoidanceOptions,
        string consequence,
        int netSuccesses,
        int targetDc,
        string? rollDetails = null) =>
        new(
            HazardDetected: true,
            HazardType: hazardType,
            AvoidanceOptions: avoidanceOptions,
            ConsequenceIfMissed: consequence,
            DetectionMethod: Enums.DetectionMethod.AreaSweep,
            NetSuccesses: netSuccesses,
            TargetDc: targetDc,
            RollDetails: rollDetails);

    /// <summary>
    /// Creates a result for a failed detection (hazard not found).
    /// </summary>
    /// <param name="netSuccesses">Net successes from the failed check.</param>
    /// <param name="targetDc">The detection DC that was not met.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A detection result indicating failure to detect.</returns>
    public static HazardDetectionResult NotDetected(
        int netSuccesses = 0,
        int targetDc = 0,
        string? rollDetails = null) =>
        new(
            HazardDetected: false,
            HazardType: null,
            AvoidanceOptions: Array.Empty<string>(),
            ConsequenceIfMissed: string.Empty,
            DetectionMethod: Enums.DetectionMethod.None,
            NetSuccesses: netSuccesses,
            TargetDc: targetDc,
            RollDetails: rollDetails);

    /// <summary>
    /// Creates an empty result representing no detection attempted.
    /// </summary>
    /// <returns>An empty detection result.</returns>
    public static HazardDetectionResult Empty() =>
        new(
            HazardDetected: false,
            HazardType: null,
            AvoidanceOptions: Array.Empty<string>(),
            ConsequenceIfMissed: string.Empty,
            DetectionMethod: Enums.DetectionMethod.None,
            NetSuccesses: 0,
            TargetDc: 0,
            RollDetails: "No detection attempted");

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string describing the detection result for the player.
    /// </summary>
    /// <returns>A formatted string suitable for display to the player.</returns>
    public string ToDisplayString()
    {
        // Not detected
        if (!HazardDetected)
        {
            return "You don't detect any hazards in the immediate area.";
        }

        // Passive detection (hint only)
        if (DetectionMethod == Enums.DetectionMethod.PassivePerception)
        {
            return "Something feels wrong here. You sense danger, but can't identify its nature. " +
                   "Consider investigating further.";
        }

        // Active detection success
        var result = $"HAZARD DETECTED: {HazardType?.GetDisplayName() ?? "Unknown"}\n";
        result += $"Consequence if triggered: {ConsequenceIfMissed}\n";
        result += "Avoidance options:\n";

        foreach (var option in AvoidanceOptions)
        {
            result += $"  - {option}\n";
        }

        if (IsCritical)
        {
            result += "\n[CRITICAL] You gain additional insight into this hazard's weaknesses and how to exploit or disable it.";
        }

        return result;
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging.
    /// </summary>
    /// <returns>A multi-line string with complete result details.</returns>
    public string ToDetailedString()
    {
        var details = $"HazardDetectionResult\n" +
                      $"  Detected: {HazardDetected}\n" +
                      $"  Hazard Type: {HazardType?.GetDisplayName() ?? "Unknown/Not Identified"}\n" +
                      $"  Detection Method: {DetectionMethod.GetDisplayName()}\n" +
                      $"  Net Successes: {NetSuccesses}\n" +
                      $"  Target DC: {TargetDc}\n" +
                      $"  Margin: {Margin:+0;-0;0}\n" +
                      $"  Fully Identified: {FullyIdentified}\n" +
                      $"  Is Critical: {IsCritical}\n";

        if (HazardDetected)
        {
            details += $"  Consequence: {ConsequenceIfMissed}\n" +
                       $"  Avoidance Options: {AvoidanceOptions.Count}\n";
        }

        if (!string.IsNullOrEmpty(RollDetails))
        {
            details += $"  Roll Details: {RollDetails}\n";
        }

        return details;
    }

    /// <summary>
    /// Returns a human-readable description of the detection result.
    /// </summary>
    /// <returns>A formatted string describing the outcome.</returns>
    public override string ToString()
    {
        if (!HazardDetected)
        {
            return "No hazard detected";
        }

        if (DetectionMethod == Enums.DetectionMethod.PassivePerception)
        {
            return $"Passive awareness: Something dangerous sensed (DC {TargetDc})";
        }

        var criticalStr = IsCritical ? " [CRITICAL]" : "";
        return $"Detected: {HazardType?.GetDisplayName() ?? "Unknown"} ({NetSuccesses} vs DC {TargetDc}){criticalStr}";
    }
}
