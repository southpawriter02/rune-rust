namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Threat level classifications for enemies detected during scouting operations.
/// </summary>
/// <remarks>
/// <para>
/// ThreatLevel provides a quick assessment of how dangerous a detected enemy is,
/// allowing players to make informed tactical decisions before entering a room.
/// The assessment is based on the enemy's challenge rating.
/// </para>
/// <para>
/// Threat level thresholds by challenge rating:
/// <list type="bullet">
///   <item><description>Low (CR 0-2): Minor enemies, easily handled by prepared characters</description></item>
///   <item><description>Moderate (CR 3-5): Requires attention, may cause damage</description></item>
///   <item><description>High (CR 6-8): Dangerous encounters, preparation recommended</description></item>
///   <item><description>Extreme (CR 9+): Potentially lethal, approach with extreme caution</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ThreatLevel
{
    /// <summary>
    /// Low threat enemies that are easily handled.
    /// </summary>
    /// <remarks>
    /// Challenge rating 0-2. Minor threats such as scavengers, feral animals,
    /// or damaged automatons. Can typically be dispatched without significant risk.
    /// </remarks>
    Low = 0,

    /// <summary>
    /// Moderate threat enemies that require attention.
    /// </summary>
    /// <remarks>
    /// Challenge rating 3-5. Competent opponents such as raiders, mutant beasts,
    /// or functional security drones. May cause injury if underestimated.
    /// </remarks>
    Moderate = 1,

    /// <summary>
    /// High threat enemies representing dangerous encounters.
    /// </summary>
    /// <remarks>
    /// Challenge rating 6-8. Serious threats such as elite raiders, apex predators,
    /// or corrupted war machines. Preparation and tactics strongly recommended.
    /// </remarks>
    High = 2,

    /// <summary>
    /// Extreme threat enemies that are potentially lethal.
    /// </summary>
    /// <remarks>
    /// Challenge rating 9+. Apex threats such as Glitch-corrupted horrors,
    /// Jotun remnants, or legendary creatures. Retreat may be the wisest option.
    /// </remarks>
    Extreme = 3
}

/// <summary>
/// Extension methods for <see cref="ThreatLevel"/>.
/// </summary>
public static class ThreatLevelExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the human-readable display name for this threat level.
    /// </summary>
    /// <param name="threatLevel">The threat level.</param>
    /// <returns>A display name suitable for UI presentation.</returns>
    public static string GetDisplayName(this ThreatLevel threatLevel)
    {
        return threatLevel switch
        {
            ThreatLevel.Low => "Low Threat",
            ThreatLevel.Moderate => "Moderate Threat",
            ThreatLevel.High => "High Threat",
            ThreatLevel.Extreme => "Extreme Threat",
            _ => "Unknown Threat"
        };
    }

    /// <summary>
    /// Gets a short descriptor for this threat level suitable for inline display.
    /// </summary>
    /// <param name="threatLevel">The threat level.</param>
    /// <returns>A short descriptor (e.g., "minor", "dangerous").</returns>
    public static string GetShortDescriptor(this ThreatLevel threatLevel)
    {
        return threatLevel switch
        {
            ThreatLevel.Low => "minor",
            ThreatLevel.Moderate => "moderate",
            ThreatLevel.High => "dangerous",
            ThreatLevel.Extreme => "deadly",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Gets a detailed description of the threat level.
    /// </summary>
    /// <param name="threatLevel">The threat level.</param>
    /// <returns>A descriptive string explaining the threat assessment.</returns>
    public static string GetDescription(this ThreatLevel threatLevel)
    {
        return threatLevel switch
        {
            ThreatLevel.Low =>
                "Minor threat that can be handled without significant risk. " +
                "Suitable for direct engagement.",
            ThreatLevel.Moderate =>
                "Competent opponents that require attention. " +
                "May cause injury if underestimated.",
            ThreatLevel.High =>
                "Dangerous enemies that pose serious risk. " +
                "Preparation and tactical approach recommended.",
            ThreatLevel.Extreme =>
                "Potentially lethal encounter. " +
                "Consider retreat or exceptional preparation before engaging.",
            _ => "Unknown threat level."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // THRESHOLD METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the minimum challenge rating for this threat level.
    /// </summary>
    /// <param name="threatLevel">The threat level.</param>
    /// <returns>The minimum CR for this threat category.</returns>
    public static int GetMinimumChallengeRating(this ThreatLevel threatLevel)
    {
        return threatLevel switch
        {
            ThreatLevel.Low => 0,
            ThreatLevel.Moderate => 3,
            ThreatLevel.High => 6,
            ThreatLevel.Extreme => 9,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the maximum challenge rating for this threat level.
    /// </summary>
    /// <param name="threatLevel">The threat level.</param>
    /// <returns>The maximum CR for this threat category, or int.MaxValue for Extreme.</returns>
    public static int GetMaximumChallengeRating(this ThreatLevel threatLevel)
    {
        return threatLevel switch
        {
            ThreatLevel.Low => 2,
            ThreatLevel.Moderate => 5,
            ThreatLevel.High => 8,
            ThreatLevel.Extreme => int.MaxValue,
            _ => int.MaxValue
        };
    }

    /// <summary>
    /// Determines the threat level from a challenge rating.
    /// </summary>
    /// <param name="challengeRating">The enemy's challenge rating.</param>
    /// <returns>The corresponding threat level.</returns>
    public static ThreatLevel FromChallengeRating(int challengeRating)
    {
        return challengeRating switch
        {
            <= 2 => ThreatLevel.Low,
            <= 5 => ThreatLevel.Moderate,
            <= 8 => ThreatLevel.High,
            _ => ThreatLevel.Extreme
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TACTICAL ASSESSMENT METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines whether this threat level suggests caution.
    /// </summary>
    /// <param name="threatLevel">The threat level.</param>
    /// <returns>True if preparation is recommended before engaging.</returns>
    public static bool SuggestsCaution(this ThreatLevel threatLevel)
    {
        return threatLevel >= ThreatLevel.High;
    }

    /// <summary>
    /// Determines whether this threat level suggests possible retreat.
    /// </summary>
    /// <param name="threatLevel">The threat level.</param>
    /// <returns>True if retreat should be considered.</returns>
    public static bool SuggestsRetreat(this ThreatLevel threatLevel)
    {
        return threatLevel == ThreatLevel.Extreme;
    }

    /// <summary>
    /// Gets a tactical recommendation based on the threat level.
    /// </summary>
    /// <param name="threatLevel">The threat level.</param>
    /// <returns>A string with tactical advice.</returns>
    public static string GetTacticalRecommendation(this ThreatLevel threatLevel)
    {
        return threatLevel switch
        {
            ThreatLevel.Low => "Engage at will.",
            ThreatLevel.Moderate => "Proceed with normal caution.",
            ThreatLevel.High => "Prepare before engaging. Consider tactical approach.",
            ThreatLevel.Extreme => "Extreme caution advised. Consider retreat or exceptional preparation.",
            _ => "Assess situation before proceeding."
        };
    }
}
