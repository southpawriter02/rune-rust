namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of dangerous areas that can be encountered on a navigation fumble.
/// </summary>
/// <remarks>
/// <para>
/// When a navigation check results in a fumble (0 successes + ≥1 botch), the navigator
/// stumbles into a dangerous area. The type of area is determined by rolling a d6:
/// <list type="bullet">
///   <item><description>Roll 1-2: <see cref="HazardZone"/> - Environmental hazard</description></item>
///   <item><description>Roll 3-4: <see cref="HostileTerritory"/> - Enemy-controlled area</description></item>
///   <item><description>Roll 5-6: <see cref="GlitchPocket"/> - Reality-warped zone</description></item>
/// </list>
/// </para>
/// <para>
/// Each dangerous area type presents different challenges and potential encounters.
/// The Disoriented fumble consequence is applied regardless of area type.
/// </para>
/// </remarks>
public enum DangerousAreaType
{
    /// <summary>
    /// Environmental hazard such as a toxic zone or unstable structure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Triggered on d6 roll of 1-2.
    /// Examples: Radiation zones, toxic gas vents, collapsing buildings, acid pools.
    /// </para>
    /// <para>
    /// Typically requires immediate hazard avoidance or environmental protection.
    /// May cause ongoing damage or status effects if not addressed quickly.
    /// </para>
    /// </remarks>
    HazardZone = 0,

    /// <summary>
    /// Area controlled by hostile factions or creatures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Triggered on d6 roll of 3-4.
    /// Examples: Raider camps, mutant lairs, faction territory, predator hunting grounds.
    /// </para>
    /// <para>
    /// May trigger immediate combat or require stealth to escape.
    /// The navigator is at disadvantage as they stumbled in unexpectedly.
    /// </para>
    /// </remarks>
    HostileTerritory = 1,

    /// <summary>
    /// Reality-warped zone with unpredictable Glitch effects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Triggered on d6 roll of 5-6.
    /// Examples: Temporal loops, spatial distortions, memory echoes, unstable reality.
    /// </para>
    /// <para>
    /// May impose additional Glitch-related penalties or trigger random effects.
    /// Navigation out of these areas may require additional skill checks.
    /// </para>
    /// </remarks>
    GlitchPocket = 2
}

/// <summary>
/// Extension methods for <see cref="DangerousAreaType"/>.
/// </summary>
public static class DangerousAreaTypeExtensions
{
    /// <summary>
    /// Gets the human-readable display name for this dangerous area type.
    /// </summary>
    /// <param name="areaType">The dangerous area type.</param>
    /// <returns>A display name suitable for UI presentation.</returns>
    public static string GetDisplayName(this DangerousAreaType areaType)
    {
        return areaType switch
        {
            DangerousAreaType.HazardZone => "Hazard Zone",
            DangerousAreaType.HostileTerritory => "Hostile Territory",
            DangerousAreaType.GlitchPocket => "Glitch Pocket",
            _ => "Unknown Dangerous Area"
        };
    }

    /// <summary>
    /// Gets a description of the dangerous area type.
    /// </summary>
    /// <param name="areaType">The dangerous area type.</param>
    /// <returns>A descriptive string explaining the danger.</returns>
    public static string GetDescription(this DangerousAreaType areaType)
    {
        return areaType switch
        {
            DangerousAreaType.HazardZone =>
                "Environmental hazard such as toxic area or unstable structure. Immediate danger to health.",
            DangerousAreaType.HostileTerritory =>
                "Area controlled by hostile factions or creatures. Combat or stealth required to escape.",
            DangerousAreaType.GlitchPocket =>
                "Reality-warped zone with unpredictable effects. Navigation may be impaired.",
            _ => "Unknown dangerous area with unpredictable threats."
        };
    }

    /// <summary>
    /// Gets the minimum d6 roll that results in this area type.
    /// </summary>
    /// <param name="areaType">The dangerous area type.</param>
    /// <returns>The minimum roll value (1-6).</returns>
    public static int GetMinRoll(this DangerousAreaType areaType)
    {
        return areaType switch
        {
            DangerousAreaType.HazardZone => 1,
            DangerousAreaType.HostileTerritory => 3,
            DangerousAreaType.GlitchPocket => 5,
            _ => 1
        };
    }

    /// <summary>
    /// Gets the maximum d6 roll that results in this area type.
    /// </summary>
    /// <param name="areaType">The dangerous area type.</param>
    /// <returns>The maximum roll value (1-6).</returns>
    public static int GetMaxRoll(this DangerousAreaType areaType)
    {
        return areaType switch
        {
            DangerousAreaType.HazardZone => 2,
            DangerousAreaType.HostileTerritory => 4,
            DangerousAreaType.GlitchPocket => 6,
            _ => 2
        };
    }

    /// <summary>
    /// Determines the dangerous area type from a d6 roll.
    /// </summary>
    /// <param name="roll">The d6 roll result (1-6).</param>
    /// <returns>The corresponding dangerous area type.</returns>
    public static DangerousAreaType FromRoll(int roll)
    {
        return roll switch
        {
            1 or 2 => DangerousAreaType.HazardZone,
            3 or 4 => DangerousAreaType.HostileTerritory,
            5 or 6 => DangerousAreaType.GlitchPocket,
            _ => DangerousAreaType.HazardZone // Default for out-of-range
        };
    }

    /// <summary>
    /// Determines whether this area type involves environmental hazards.
    /// </summary>
    /// <param name="areaType">The dangerous area type.</param>
    /// <returns>True if the area has environmental hazards.</returns>
    public static bool HasEnvironmentalHazards(this DangerousAreaType areaType)
    {
        return areaType is DangerousAreaType.HazardZone or DangerousAreaType.GlitchPocket;
    }

    /// <summary>
    /// Determines whether this area type may trigger combat.
    /// </summary>
    /// <param name="areaType">The dangerous area type.</param>
    /// <returns>True if the area may contain hostile entities.</returns>
    public static bool MayTriggerCombat(this DangerousAreaType areaType)
    {
        return areaType == DangerousAreaType.HostileTerritory;
    }

    /// <summary>
    /// Determines whether this area type has Glitch effects.
    /// </summary>
    /// <param name="areaType">The dangerous area type.</param>
    /// <returns>True if the area has reality distortions.</returns>
    public static bool HasGlitchEffects(this DangerousAreaType areaType)
    {
        return areaType == DangerousAreaType.GlitchPocket;
    }
}
