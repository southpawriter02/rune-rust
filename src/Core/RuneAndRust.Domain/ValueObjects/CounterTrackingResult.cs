using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of a counter-tracking (trail concealment) attempt.
/// </summary>
/// <remarks>
/// <para>
/// Encapsulates the complete outcome of a concealment attempt including:
/// <list type="bullet">
///   <item><description>The resulting concealment DC that trackers must beat</description></item>
///   <item><description>The technique bonus that was applied</description></item>
///   <item><description>The net successes from the skill roll</description></item>
///   <item><description>The time multiplier incurred by the techniques used</description></item>
///   <item><description>Details about the roll for logging and display</description></item>
/// </list>
/// </para>
/// <para>
/// The concealment DC is calculated as:
/// <code>
/// ConcealmentDc = Math.Clamp(NetSuccesses + TotalBonus, MinDc, MaxDc)
/// </code>
/// where MinDc = 10 and MaxDc = 30.
/// </para>
/// <para>
/// This DC becomes the contested DC that any tracker must beat when following
/// the concealed trail. It replaces the normal trail age DC in the TrackingState.
/// </para>
/// </remarks>
/// <param name="ConcealmentDc">
/// The difficulty class that trackers must beat to follow this trail.
/// Clamped between 10 (minimum) and 30 (maximum).
/// </param>
/// <param name="TotalBonus">
/// The total bonus from all techniques used (additively stacked).
/// For example, BrushTracks (+4) + Backtracking (+4) = +8.
/// </param>
/// <param name="NetSuccesses">
/// The net successes from the Wasteland Survival skill roll.
/// This is the raw roll result before technique bonuses.
/// </param>
/// <param name="TimeMultiplier">
/// The total time multiplier from all techniques used (multiplicatively compounded).
/// For example, BrushTracks (x1.5) × Backtracking (x1.25) = x1.875.
/// </param>
/// <param name="TechniquesUsed">
/// The concealment techniques that were successfully applied.
/// Only includes valid techniques (those with environmental requirements met).
/// </param>
/// <param name="RollDetails">
/// Human-readable description of the roll for logging and display.
/// Includes dice, successes, botches, and bonuses applied.
/// </param>
public readonly record struct CounterTrackingResult(
    int ConcealmentDc,
    int TotalBonus,
    int NetSuccesses,
    decimal TimeMultiplier,
    IReadOnlyList<ConcealmentTechnique> TechniquesUsed,
    string RollDetails)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Minimum concealment DC (even a poor roll provides some protection).
    /// </summary>
    public const int MinConcealmentDc = 10;

    /// <summary>
    /// Maximum concealment DC (even the best concealment has limits).
    /// </summary>
    public const int MaxConcealmentDc = 30;

    // ═══════════════════════════════════════════════════════════════════════════
    // DERIVED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Whether the concealment was effective (DC >= minimum threshold).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns true if the concealment DC meets or exceeds the minimum threshold.
    /// Since the DC is clamped to minimum 10, this will always be true for
    /// successful concealment attempts.
    /// </para>
    /// <para>
    /// Use <see cref="EffectivenessRating"/> for a more nuanced assessment.
    /// </para>
    /// </remarks>
    public bool IsEffective => ConcealmentDc >= MinConcealmentDc;

    /// <summary>
    /// A qualitative rating of the concealment effectiveness.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ratings based on concealment DC:
    /// <list type="bullet">
    ///   <item><description>10-14: "Minimal" - Basic concealment, easy to track</description></item>
    ///   <item><description>15-19: "Moderate" - Solid concealment, challenging to track</description></item>
    ///   <item><description>20-24: "Strong" - Excellent concealment, difficult to track</description></item>
    ///   <item><description>25-30: "Exceptional" - Near-perfect concealment, very hard to track</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public string EffectivenessRating => ConcealmentDc switch
    {
        >= 25 => "Exceptional",
        >= 20 => "Strong",
        >= 15 => "Moderate",
        _ => "Minimal"
    };

    /// <summary>
    /// Whether any techniques were used.
    /// </summary>
    public bool HasTechniques => TechniquesUsed.Count > 0;

    /// <summary>
    /// The number of techniques that were applied.
    /// </summary>
    public int TechniqueCount => TechniquesUsed.Count;

    /// <summary>
    /// Whether the time multiplier is significant (greater than 1.0).
    /// </summary>
    /// <remarks>
    /// True when the concealment techniques impose a time penalty.
    /// </remarks>
    public bool HasTimePenalty => TimeMultiplier > 1.0m;

    /// <summary>
    /// The percentage increase in travel time from techniques.
    /// </summary>
    /// <remarks>
    /// For example, a time multiplier of 1.5 returns 50 (50% increase).
    /// Returns 0 if no time penalty.
    /// </remarks>
    public int TimePenaltyPercent => (int)((TimeMultiplier - 1.0m) * 100);

    /// <summary>
    /// The raw concealment score before clamping.
    /// </summary>
    /// <remarks>
    /// Useful for understanding the actual roll result before the
    /// 10-30 bounds were applied.
    /// </remarks>
    public int RawConcealmentScore => NetSuccesses + TotalBonus;

    /// <summary>
    /// Whether the concealment DC was clamped at the minimum.
    /// </summary>
    /// <remarks>
    /// True if the raw score was below 10 but was raised to 10.
    /// </remarks>
    public bool WasClampedAtMinimum => RawConcealmentScore < MinConcealmentDc;

    /// <summary>
    /// Whether the concealment DC was clamped at the maximum.
    /// </summary>
    /// <remarks>
    /// True if the raw score was above 30 but was capped at 30.
    /// </remarks>
    public bool WasClampedAtMaximum => RawConcealmentScore > MaxConcealmentDc;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result for a failed concealment attempt.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the failed roll.</param>
    /// <param name="rollDetails">Description of the failed roll.</param>
    /// <returns>A CounterTrackingResult with minimum DC and no techniques.</returns>
    /// <remarks>
    /// <para>
    /// Used when the skill check fails entirely. Even a failed concealment
    /// attempt provides minimum protection (DC 10) since some unconscious
    /// trail-obscuring behavior occurs.
    /// </para>
    /// <para>
    /// No techniques are recorded and no time penalty is applied since the
    /// concealment was not successfully executed.
    /// </para>
    /// </remarks>
    public static CounterTrackingResult Failed(int netSuccesses, string rollDetails)
    {
        return new CounterTrackingResult(
            ConcealmentDc: MinConcealmentDc,
            TotalBonus: 0,
            NetSuccesses: netSuccesses,
            TimeMultiplier: 1.0m,
            TechniquesUsed: Array.Empty<ConcealmentTechnique>(),
            RollDetails: rollDetails);
    }

    /// <summary>
    /// Creates a result from roll components with automatic DC calculation.
    /// </summary>
    /// <param name="netSuccesses">Net successes from the skill roll.</param>
    /// <param name="totalBonus">Total technique bonus to apply.</param>
    /// <param name="timeMultiplier">Compounded time multiplier.</param>
    /// <param name="techniques">List of techniques used.</param>
    /// <param name="rollDetails">Description of the roll.</param>
    /// <returns>A CounterTrackingResult with clamped concealment DC.</returns>
    /// <remarks>
    /// Automatically clamps the concealment DC between 10 and 30.
    /// </remarks>
    public static CounterTrackingResult Create(
        int netSuccesses,
        int totalBonus,
        decimal timeMultiplier,
        IReadOnlyList<ConcealmentTechnique> techniques,
        string rollDetails)
    {
        var rawDc = netSuccesses + totalBonus;
        var clampedDc = Math.Clamp(rawDc, MinConcealmentDc, MaxConcealmentDc);

        return new CounterTrackingResult(
            ConcealmentDc: clampedDc,
            TotalBonus: totalBonus,
            NetSuccesses: netSuccesses,
            TimeMultiplier: timeMultiplier,
            TechniquesUsed: techniques,
            RollDetails: rollDetails);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a human-readable summary of the concealment result.
    /// </summary>
    /// <returns>A formatted string describing the concealment outcome.</returns>
    public override string ToString()
    {
        var techniqueStr = TechniquesUsed.Count > 0
            ? $"[{string.Join(", ", TechniquesUsed)}]"
            : "[No techniques]";

        var timeStr = HasTimePenalty
            ? $" (Time: +{TimePenaltyPercent}%)"
            : "";

        return $"Concealment DC {ConcealmentDc} ({EffectivenessRating}) {techniqueStr}{timeStr}";
    }

    /// <summary>
    /// Returns a detailed diagnostic string for logging.
    /// </summary>
    /// <returns>A multi-line string with complete result details.</returns>
    public string ToDetailedString()
    {
        var clampNote = WasClampedAtMinimum
            ? " (clamped from below)"
            : WasClampedAtMaximum
                ? " (capped at max)"
                : "";

        var techniqueList = TechniquesUsed.Count > 0
            ? string.Join(", ", TechniquesUsed)
            : "None";

        return $"CounterTrackingResult\n" +
               $"  Concealment DC: {ConcealmentDc}{clampNote}\n" +
               $"  Effectiveness: {EffectivenessRating}\n" +
               $"  Net Successes: {NetSuccesses}\n" +
               $"  Technique Bonus: +{TotalBonus}\n" +
               $"  Raw Score: {RawConcealmentScore}\n" +
               $"  Techniques: {techniqueList}\n" +
               $"  Time Multiplier: x{TimeMultiplier:F2}{(HasTimePenalty ? $" (+{TimePenaltyPercent}%)" : "")}\n" +
               $"  Roll: {RollDetails}";
    }

    /// <summary>
    /// Returns a narrative description for player feedback.
    /// </summary>
    /// <returns>A flavor text description of the concealment result.</returns>
    /// <remarks>
    /// Generates contextual narrative based on effectiveness rating
    /// and techniques used.
    /// </remarks>
    public string ToNarrativeString()
    {
        var baseNarrative = EffectivenessRating switch
        {
            "Exceptional" =>
                "You execute a masterful concealment, leaving virtually no trace of your passage. " +
                "Only the most skilled tracker could hope to follow this trail.",
            "Strong" =>
                "You carefully conceal your trail, employing multiple techniques to confuse pursuers. " +
                "Following this trail would be a significant challenge.",
            "Moderate" =>
                "You make a reasonable effort to hide your tracks. " +
                "While not perfect, your trail should prove difficult to follow.",
            _ =>
                "You attempt to conceal your trail, but the result is far from perfect. " +
                "An experienced tracker might still be able to follow you."
        };

        if (HasTimePenalty)
        {
            baseNarrative += $" The extra precautions cost you time (+{TimePenaltyPercent}% travel time).";
        }

        return baseNarrative;
    }
}
