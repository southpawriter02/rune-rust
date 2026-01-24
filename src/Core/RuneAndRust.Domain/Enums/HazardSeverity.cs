namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Severity classifications for hazards detected during scouting operations.
/// </summary>
/// <remarks>
/// <para>
/// HazardSeverity indicates how dangerous a hazard is, helping players assess
/// whether to proceed, find alternate routes, or prepare countermeasures.
/// </para>
/// <para>
/// Severity levels and their implications:
/// <list type="bullet">
///   <item><description>Minor: Causes inconvenience or light damage (1d6 or less)</description></item>
///   <item><description>Moderate: Causes notable damage, should be avoided (2d6 damage)</description></item>
///   <item><description>Severe: Causes serious damage, dangerous to traverse (3d6+ damage)</description></item>
///   <item><description>Lethal: Potentially fatal, may cause instant death or extreme damage</description></item>
/// </list>
/// </para>
/// </remarks>
public enum HazardSeverity
{
    /// <summary>
    /// Minor hazard causing light damage or inconvenience.
    /// </summary>
    /// <remarks>
    /// Typically deals 1d6 or less damage. Examples include minor debris,
    /// weak electrical fields, or shallow toxic puddles. Can often be
    /// traversed with minimal precautions.
    /// </remarks>
    Minor = 0,

    /// <summary>
    /// Moderate hazard causing notable damage.
    /// </summary>
    /// <remarks>
    /// Typically deals around 2d6 damage. Examples include active fire zones,
    /// moderate radiation, or concealed pits. Should be avoided or
    /// approached with proper equipment.
    /// </remarks>
    Moderate = 1,

    /// <summary>
    /// Severe hazard causing serious damage.
    /// </summary>
    /// <remarks>
    /// Typically deals 3d6+ damage. Examples include intense heat zones,
    /// heavy chemical contamination, or deep chasms. Dangerous to enter
    /// without specific countermeasures.
    /// </remarks>
    Severe = 2,

    /// <summary>
    /// Lethal hazard that may cause death.
    /// </summary>
    /// <remarks>
    /// May cause instant death or extreme damage (4d6+ or special effects).
    /// Examples include Glitch pockets, lava flows, or structural collapse zones.
    /// Should be avoided entirely unless absolutely necessary.
    /// </remarks>
    Lethal = 3
}

/// <summary>
/// Extension methods for <see cref="HazardSeverity"/>.
/// </summary>
public static class HazardSeverityExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the human-readable display name for this severity level.
    /// </summary>
    /// <param name="severity">The hazard severity.</param>
    /// <returns>A display name suitable for UI presentation.</returns>
    public static string GetDisplayName(this HazardSeverity severity)
    {
        return severity switch
        {
            HazardSeverity.Minor => "Minor",
            HazardSeverity.Moderate => "Moderate",
            HazardSeverity.Severe => "Severe",
            HazardSeverity.Lethal => "Lethal",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets a short descriptor for this severity level suitable for inline display.
    /// </summary>
    /// <param name="severity">The hazard severity.</param>
    /// <returns>A short descriptor (e.g., "minor", "deadly").</returns>
    public static string GetShortDescriptor(this HazardSeverity severity)
    {
        return severity switch
        {
            HazardSeverity.Minor => "minor",
            HazardSeverity.Moderate => "moderate",
            HazardSeverity.Severe => "severe",
            HazardSeverity.Lethal => "lethal",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Gets a detailed description of the severity level.
    /// </summary>
    /// <param name="severity">The hazard severity.</param>
    /// <returns>A descriptive string explaining the danger level.</returns>
    public static string GetDescription(this HazardSeverity severity)
    {
        return severity switch
        {
            HazardSeverity.Minor =>
                "Light damage or inconvenience. Can often be traversed with minimal precautions.",
            HazardSeverity.Moderate =>
                "Notable damage that should be avoided. Approach with proper equipment.",
            HazardSeverity.Severe =>
                "Serious damage that is dangerous to traverse. Requires specific countermeasures.",
            HazardSeverity.Lethal =>
                "Potentially fatal. Should be avoided entirely unless absolutely necessary.",
            _ => "Unknown severity level."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DAMAGE ESTIMATION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the typical damage dice expression for this severity level.
    /// </summary>
    /// <param name="severity">The hazard severity.</param>
    /// <returns>A dice expression representing typical damage.</returns>
    public static string GetTypicalDamageDice(this HazardSeverity severity)
    {
        return severity switch
        {
            HazardSeverity.Minor => "1d6",
            HazardSeverity.Moderate => "2d6",
            HazardSeverity.Severe => "3d6",
            HazardSeverity.Lethal => "4d6+",
            _ => "1d6"
        };
    }

    /// <summary>
    /// Gets the estimated average damage for this severity level.
    /// </summary>
    /// <param name="severity">The hazard severity.</param>
    /// <returns>The expected average damage value.</returns>
    public static int GetAverageDamage(this HazardSeverity severity)
    {
        return severity switch
        {
            HazardSeverity.Minor => 3,      // 1d6 avg = 3.5
            HazardSeverity.Moderate => 7,   // 2d6 avg = 7
            HazardSeverity.Severe => 10,    // 3d6 avg = 10.5
            HazardSeverity.Lethal => 14,    // 4d6 avg = 14
            _ => 3
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TACTICAL ASSESSMENT METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines whether this severity level suggests avoiding the hazard.
    /// </summary>
    /// <param name="severity">The hazard severity.</param>
    /// <returns>True if the hazard should ideally be avoided.</returns>
    public static bool SuggestsAvoidance(this HazardSeverity severity)
    {
        return severity >= HazardSeverity.Moderate;
    }

    /// <summary>
    /// Determines whether this severity level requires special equipment.
    /// </summary>
    /// <param name="severity">The hazard severity.</param>
    /// <returns>True if special equipment is recommended for traversal.</returns>
    public static bool RequiresSpecialEquipment(this HazardSeverity severity)
    {
        return severity >= HazardSeverity.Severe;
    }

    /// <summary>
    /// Determines whether this severity level is potentially lethal.
    /// </summary>
    /// <param name="severity">The hazard severity.</param>
    /// <returns>True if the hazard may cause death.</returns>
    public static bool IsPotentiallyLethal(this HazardSeverity severity)
    {
        return severity == HazardSeverity.Lethal;
    }

    /// <summary>
    /// Gets a tactical recommendation based on the severity level.
    /// </summary>
    /// <param name="severity">The hazard severity.</param>
    /// <returns>A string with tactical advice.</returns>
    public static string GetTacticalRecommendation(this HazardSeverity severity)
    {
        return severity switch
        {
            HazardSeverity.Minor => "Can be traversed with minimal precautions.",
            HazardSeverity.Moderate => "Avoid if possible, or use protective equipment.",
            HazardSeverity.Severe => "Find alternate route or use specific countermeasures.",
            HazardSeverity.Lethal => "Do not enter. Seek alternate path.",
            _ => "Assess hazard before proceeding."
        };
    }

    /// <summary>
    /// Gets a warning message appropriate for the severity level.
    /// </summary>
    /// <param name="severity">The hazard severity.</param>
    /// <returns>A warning message for display to the player.</returns>
    public static string GetWarningMessage(this HazardSeverity severity)
    {
        return severity switch
        {
            HazardSeverity.Minor => "Minor hazard ahead.",
            HazardSeverity.Moderate => "[CAUTION] Moderate hazard detected.",
            HazardSeverity.Severe => "[WARNING] Severe hazard! Approach with extreme care.",
            HazardSeverity.Lethal => "[DANGER] Lethal hazard! Entry not recommended.",
            _ => "Hazard detected."
        };
    }
}
