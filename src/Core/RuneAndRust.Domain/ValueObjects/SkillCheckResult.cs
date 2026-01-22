using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the complete result of a skill check using success-counting mechanics.
/// </summary>
/// <remarks>
/// <para>
/// Immutable value object containing all skill check details including:
/// dice roll breakdown, net successes, margin, and outcome classification.
/// </para>
/// <para>
/// <b>v0.15.0c Update:</b> Outcome Determination (success-counting):
/// <list type="bullet">
///   <item><description>CriticalFailure: Fumble (0 successes + ≥1 botch)</description></item>
///   <item><description>Failure: Margin &lt; 0</description></item>
///   <item><description>MarginalSuccess: Margin = 0</description></item>
///   <item><description>FullSuccess: Margin 1-2</description></item>
///   <item><description>ExceptionalSuccess: Margin 3-4</description></item>
///   <item><description>CriticalSuccess: Margin ≥ 5</description></item>
/// </list>
/// </para>
/// </remarks>
public readonly record struct SkillCheckResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CORE PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the ID of the skill that was checked.
    /// </summary>
    public string SkillId { get; init; }

    /// <summary>
    /// Gets the display name of the skill.
    /// </summary>
    public string SkillName { get; init; }

    /// <summary>
    /// Gets the underlying dice roll result.
    /// </summary>
    public DiceRollResult DiceResult { get; init; }

    /// <summary>
    /// Gets the bonus dice added from the player's primary attribute.
    /// </summary>
    /// <remarks>
    /// In success-counting mechanics, attribute bonuses typically add dice to the pool
    /// rather than adding to a sum. This value represents the number of bonus dice.
    /// </remarks>
    public int AttributeBonus { get; init; }

    /// <summary>
    /// Gets bonus dice from other sources (equipment, abilities, situational).
    /// </summary>
    public int OtherBonus { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // SUCCESS-COUNTING PROPERTIES (v0.15.0c)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the net successes from the dice roll.
    /// </summary>
    /// <remarks>
    /// Net successes = total successes (8+) minus total botches (1).
    /// This is the primary value used for comparison against the DC.
    /// </remarks>
    public int NetSuccesses { get; init; }

    /// <summary>
    /// Gets the difficulty class that was checked against.
    /// </summary>
    /// <remarks>
    /// In success-counting mechanics, DC represents the number of net successes
    /// required to succeed, not a sum threshold.
    /// </remarks>
    public int DifficultyClass { get; init; }

    /// <summary>
    /// Gets the name of the difficulty level.
    /// </summary>
    public string DifficultyName { get; init; }

    /// <summary>
    /// Gets the margin by which the check succeeded or failed.
    /// </summary>
    /// <remarks>
    /// Margin = NetSuccesses - DifficultyClass.
    /// Positive margin indicates success, negative indicates failure.
    /// </remarks>
    public int Margin { get; init; }

    /// <summary>
    /// Gets the 6-tier skill outcome classification.
    /// </summary>
    public SkillOutcome Outcome { get; init; }

    /// <summary>
    /// Gets whether this roll was a fumble.
    /// </summary>
    /// <remarks>
    /// A fumble occurs when there are 0 successes AND at least 1 botch.
    /// Fumbles always result in CriticalFailure regardless of DC.
    /// </remarks>
    public bool IsFumble { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // DERIVED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the check succeeded (MarginalSuccess or better).
    /// </summary>
    public bool IsSuccess => Outcome.IsSuccess();

    /// <summary>
    /// Gets whether the check was a critical result (best or worst tier).
    /// </summary>
    public bool IsCritical => Outcome.IsCritical();

    /// <summary>
    /// Gets whether this was a critical success.
    /// </summary>
    public bool IsCriticalSuccess => Outcome == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Gets whether this was a critical failure.
    /// </summary>
    public bool IsCriticalFailure => Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Gets the total bonus dice applied (attribute + other).
    /// </summary>
    public int TotalBonus => AttributeBonus + OtherBonus;

    /// <summary>
    /// Gets the raw dice total before bonuses (sum-based, for damage calculations only).
    /// </summary>
    public int RawDiceTotal => DiceResult.Total;

    // ═══════════════════════════════════════════════════════════════════════════
    // LEGACY BACKWARD COMPATIBILITY (Obsolete)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the legacy success level for backward compatibility.
    /// </summary>
    /// <remarks>
    /// Maps the 6-tier SkillOutcome to the 4-tier SuccessLevel:
    /// CriticalFailure → CriticalFailure,
    /// Failure → Failure,
    /// MarginalSuccess/FullSuccess/ExceptionalSuccess → Success,
    /// CriticalSuccess → CriticalSuccess
    /// </remarks>
    [Obsolete("Use Outcome property for 6-tier classification. SuccessLevel preserved for backward compatibility.")]
    public SuccessLevel SuccessLevel => Outcome.ToSuccessLevel();

    /// <summary>
    /// Gets the legacy total result for backward compatibility.
    /// </summary>
    /// <remarks>
    /// In success-counting mechanics, use NetSuccesses instead.
    /// This returns the raw dice sum + bonuses for legacy code.
    /// </remarks>
    [Obsolete("Use NetSuccesses for success-counting mechanics. TotalResult preserved for damage calculations.")]
    public int TotalResult => DiceResult.Total + AttributeBonus + OtherBonus;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a skill check result with automatic outcome classification.
    /// </summary>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="skillName">The skill display name.</param>
    /// <param name="diceResult">The dice roll result.</param>
    /// <param name="attributeBonus">Bonus dice from attributes.</param>
    /// <param name="otherBonus">Bonus dice from other sources.</param>
    /// <param name="difficultyClass">The difficulty class (success threshold).</param>
    /// <param name="difficultyName">The difficulty level name.</param>
    public SkillCheckResult(
        string skillId,
        string skillName,
        DiceRollResult diceResult,
        int attributeBonus,
        int otherBonus,
        int difficultyClass,
        string difficultyName)
    {
        SkillId = skillId;
        SkillName = skillName;
        DiceResult = diceResult;
        AttributeBonus = attributeBonus;
        OtherBonus = otherBonus;
        DifficultyClass = difficultyClass;
        DifficultyName = difficultyName;

        // Use net successes from dice result (v0.15.0c success-counting)
        NetSuccesses = diceResult.NetSuccesses;
        IsFumble = diceResult.IsFumble;

        // Calculate margin and classify outcome
        Margin = NetSuccesses - difficultyClass;
        Outcome = ClassifyOutcome(NetSuccesses, difficultyClass, IsFumble);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OUTCOME CLASSIFICATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Classifies the skill check outcome based on margin and fumble status.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="difficultyClass">The difficulty class.</param>
    /// <param name="isFumble">Whether the roll was a fumble.</param>
    /// <returns>The appropriate SkillOutcome tier.</returns>
    /// <remarks>
    /// <para>
    /// Classification rules:
    /// <list type="bullet">
    ///   <item><description>Fumble → CriticalFailure (regardless of DC)</description></item>
    ///   <item><description>margin &lt; 0 → Failure</description></item>
    ///   <item><description>margin = 0 → MarginalSuccess</description></item>
    ///   <item><description>margin 1-2 → FullSuccess</description></item>
    ///   <item><description>margin 3-4 → ExceptionalSuccess</description></item>
    ///   <item><description>margin ≥ 5 → CriticalSuccess</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static SkillOutcome ClassifyOutcome(int netSuccesses, int difficultyClass, bool isFumble)
    {
        // Fumble always results in critical failure
        if (isFumble)
            return SkillOutcome.CriticalFailure;

        var margin = netSuccesses - difficultyClass;

        return margin switch
        {
            < 0 => SkillOutcome.Failure,
            0 => SkillOutcome.MarginalSuccess,
            1 or 2 => SkillOutcome.FullSuccess,
            3 or 4 => SkillOutcome.ExceptionalSuccess,
            _ => SkillOutcome.CriticalSuccess  // margin >= 5
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string showing the check breakdown with success counts.
    /// </summary>
    /// <example>
    /// "Acrobatics: 3 net (5S - 2B) vs DC 2 (Moderate) → Full Success [margin: +1]"
    /// "Stealth: 0 net (0S - 1B) vs DC 1 (Easy) → Critical Failure [FUMBLE!]"
    /// </example>
    public override string ToString()
    {
        var successBreakdown = $"{DiceResult.TotalSuccesses}S - {DiceResult.TotalBotches}B";
        var marginStr = Margin >= 0 ? $"+{Margin}" : Margin.ToString();

        var result = $"{SkillName}: {NetSuccesses} net ({successBreakdown}) vs DC {DifficultyClass} ({DifficultyName}) → {Outcome.GetDisplayName()} [margin: {marginStr}]";

        if (IsFumble)
            result += " [FUMBLE!]";
        else if (IsCriticalSuccess)
            result += " [CRITICAL!]";

        return result;
    }

    /// <summary>
    /// Returns a formatted string in legacy sum-based format.
    /// </summary>
    /// <remarks>
    /// For backward compatibility with systems expecting sum-based output.
    /// </remarks>
    [Obsolete("Use ToString() for success-counting format.")]
    public string ToLegacyString()
    {
        var bonusStr = new List<string>();
        if (AttributeBonus != 0)
            bonusStr.Add($"{(AttributeBonus > 0 ? "+" : "")}{AttributeBonus} (attr)");
        if (OtherBonus != 0)
            bonusStr.Add($"{(OtherBonus > 0 ? "+" : "")}{OtherBonus} (bonus)");

        var bonusPart = bonusStr.Count > 0 ? " " + string.Join(" ", bonusStr) : "";

#pragma warning disable CS0618 // Suppress obsolete warning for legacy properties
        return $"{SkillName}: [{DiceResult.Total}]{bonusPart} = {TotalResult} vs DC {DifficultyClass} ({DifficultyName}) -> {SuccessLevel}";
#pragma warning restore CS0618
    }
}
