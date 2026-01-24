using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a hazard detected during scouting operations.
/// </summary>
/// <remarks>
/// <para>
/// DetectedHazard provides reconnaissance information about environmental dangers
/// in adjacent rooms. This allows players to prepare countermeasures, find alternate
/// routes, or proceed with appropriate caution.
/// </para>
/// <para>
/// Information revealed through scouting:
/// <list type="bullet">
///   <item><description>HazardType: The category of environmental hazard</description></item>
///   <item><description>Severity: How dangerous the hazard is (Minor to Lethal)</description></item>
///   <item><description>Description: Brief description of what was observed</description></item>
///   <item><description>AvoidanceHint: Tactical advice for handling the hazard</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="HazardType">The type of environmental hazard.</param>
/// <param name="Severity">How dangerous the hazard is.</param>
/// <param name="Description">Brief description of the hazard.</param>
/// <param name="AvoidanceHint">Tactical advice for avoiding or handling the hazard.</param>
public readonly record struct DetectedHazard(
    DetectableHazardType HazardType,
    HazardSeverity Severity,
    string Description,
    string AvoidanceHint)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this hazard should be avoided if possible.
    /// </summary>
    public bool ShouldAvoid => Severity.SuggestsAvoidance();

    /// <summary>
    /// Gets whether this hazard requires special equipment to traverse safely.
    /// </summary>
    public bool RequiresEquipment => Severity.RequiresSpecialEquipment();

    /// <summary>
    /// Gets whether this hazard is potentially lethal.
    /// </summary>
    public bool IsLethal => Severity.IsPotentiallyLethal();

    /// <summary>
    /// Gets the typical damage dice for this hazard.
    /// </summary>
    public string ExpectedDamage => Severity.GetTypicalDamageDice();

    /// <summary>
    /// Gets a tactical recommendation for handling this hazard.
    /// </summary>
    public string TacticalRecommendation => Severity.GetTacticalRecommendation();

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a DetectedHazard with default values inferred from type.
    /// </summary>
    /// <param name="hazardType">The type of hazard.</param>
    /// <param name="severity">The severity level.</param>
    /// <param name="customDescription">Optional custom description.</param>
    /// <returns>A new DetectedHazard instance.</returns>
    public static DetectedHazard Create(
        DetectableHazardType hazardType,
        HazardSeverity severity,
        string? customDescription = null)
    {
        return new DetectedHazard(
            HazardType: hazardType,
            Severity: severity,
            Description: customDescription ?? hazardType.GetDescription(),
            AvoidanceHint: GetDefaultAvoidanceHint(hazardType));
    }

    /// <summary>
    /// Creates a DetectedHazard with full customization.
    /// </summary>
    /// <param name="hazardType">The type of hazard.</param>
    /// <param name="severity">The severity level.</param>
    /// <param name="description">Description of the hazard.</param>
    /// <param name="avoidanceHint">Advice for handling the hazard.</param>
    /// <returns>A new DetectedHazard instance.</returns>
    public static DetectedHazard CreateCustom(
        DetectableHazardType hazardType,
        HazardSeverity severity,
        string description,
        string avoidanceHint)
    {
        return new DetectedHazard(
            HazardType: hazardType,
            Severity: severity,
            Description: description,
            AvoidanceHint: avoidanceHint);
    }

    /// <summary>
    /// Gets the default avoidance hint for a hazard type.
    /// </summary>
    /// <param name="hazardType">The type of hazard.</param>
    /// <returns>Default tactical advice for avoiding the hazard.</returns>
    private static string GetDefaultAvoidanceHint(DetectableHazardType hazardType)
    {
        return hazardType switch
        {
            DetectableHazardType.ObviousDanger => "Step carefully around the obvious hazard.",
            DetectableHazardType.HiddenPit => "Test the ground before stepping.",
            DetectableHazardType.ToxicZone => "Hold your breath or find another route.",
            DetectableHazardType.GlitchPocket => "Do not enter—reality is unstable here.",
            DetectableHazardType.AmbushSite => "Approach with weapons ready.",
            _ => "Proceed with caution."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for this detected hazard.
    /// </summary>
    /// <returns>A formatted string suitable for player display.</returns>
    /// <example>
    /// "Hidden Pit (moderate)" or "Glitch Pocket (lethal)"
    /// </example>
    public string ToDisplayString()
    {
        return $"{HazardType.GetDisplayName()} ({Severity.GetShortDescriptor()})";
    }

    /// <summary>
    /// Creates a detailed display string including avoidance advice.
    /// </summary>
    /// <returns>A formatted string with full details.</returns>
    public string ToDetailedString()
    {
        return $"{HazardType.GetDisplayName()} [{Severity.GetDisplayName()}]\n" +
               $"  {Description}\n" +
               $"  Advice: {AvoidanceHint}";
    }

    /// <summary>
    /// Creates a warning message for this hazard detection.
    /// </summary>
    /// <returns>A warning message appropriate for the severity level.</returns>
    public string ToWarningString()
    {
        return Severity.GetWarningMessage() + $" {HazardType.GetDisplayName()} detected.";
    }

    /// <summary>
    /// Returns a human-readable summary of the detected hazard.
    /// </summary>
    /// <returns>A formatted string describing the detection.</returns>
    public override string ToString() => ToDisplayString();
}
