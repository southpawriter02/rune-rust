using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of rolling a dice pool using success-counting mechanics.
/// </summary>
/// <remarks>
/// <para>
/// Immutable value object containing all roll details including individual dice,
/// success counts, botch counts, and special outcome detection.
/// </para>
/// <para>
/// Success-Counting Mechanics:
/// <list type="bullet">
///   <item><description>Success: Die shows 8, 9, or 10 (30% probability)</description></item>
///   <item><description>Botch: Die shows 1 (10% probability)</description></item>
///   <item><description>Net Successes: Successes - Botches (minimum 0)</description></item>
///   <item><description>Fumble: 0 successes AND ≥1 botch</description></item>
///   <item><description>Critical Success: Net successes ≥ 5</description></item>
/// </list>
/// </para>
/// </remarks>
public readonly record struct DiceRollResult
{
    /// <summary>The dice pool that was rolled.</summary>
    public DicePool Pool { get; init; }

    /// <summary>Individual dice results before any modifier.</summary>
    public IReadOnlyList<int> Rolls { get; init; }

    /// <summary>Additional rolls from exploding dice.</summary>
    public IReadOnlyList<int> ExplosionRolls { get; init; }

    /// <summary>
    /// Count of dice showing success values (8, 9, or 10).
    /// </summary>
    /// <remarks>
    /// Each die has a 30% chance of being a success.
    /// Includes both base rolls and explosion rolls.
    /// </remarks>
    public int TotalSuccesses { get; init; }

    /// <summary>
    /// Count of dice showing botch value (1).
    /// </summary>
    /// <remarks>
    /// Each die has a 10% chance of being a botch.
    /// Botches reduce the net success count.
    /// Includes both base rolls and explosion rolls.
    /// </remarks>
    public int TotalBotches { get; init; }

    /// <summary>
    /// Net successes after subtracting botches (minimum 0).
    /// </summary>
    /// <remarks>
    /// Formula: max(0, TotalSuccesses - TotalBotches)
    /// This is the primary value used for skill check comparisons.
    /// </remarks>
    public int NetSuccesses { get; init; }

    /// <summary>
    /// Whether this roll is a critical success (net successes ≥ 5).
    /// </summary>
    /// <remarks>
    /// Critical successes provide exceptional outcomes in skill checks
    /// and combat, such as bonus damage or special effects.
    /// </remarks>
    public bool IsCriticalSuccess { get; init; }

    /// <summary>
    /// Whether this roll is a fumble (0 successes AND ≥1 botch).
    /// </summary>
    /// <remarks>
    /// Fumbles represent catastrophic failures that may incur
    /// additional consequences beyond simple failure.
    /// </remarks>
    public bool IsFumble { get; init; }

    /// <summary>
    /// Raw sum of all dice values (preserved for damage calculations).
    /// </summary>
    /// <remarks>
    /// Damage rolls still use sum-based mechanics, not success-counting.
    /// This property provides backward compatibility for those systems.
    /// </remarks>
    public int RawTotal { get; init; }

    /// <summary>
    /// Total including pool modifier (preserved for legacy compatibility).
    /// </summary>
    /// <remarks>
    /// Equivalent to RawTotal + Pool.Modifier.
    /// Maintained for backward compatibility with existing code.
    /// </remarks>
    public int Total { get; init; }

    /// <summary>Total of all dice (including explosions, before modifier).</summary>
    public int DiceTotal { get; init; }

    /// <summary>Advantage type used for this roll.</summary>
    public AdvantageType AdvantageType { get; init; }

    /// <summary>All roll totals made (for advantage/disadvantage comparison).</summary>
    public IReadOnlyList<int> AllRollTotals { get; init; }

    /// <summary>Index of the selected roll (0 for normal, 0 or 1 for advantage/disadvantage).</summary>
    public int SelectedRollIndex { get; init; }

    /// <summary>
    /// True if the first die rolled its maximum value.
    /// </summary>
    /// <remarks>
    /// Preserved for backward compatibility. In the new system,
    /// use <see cref="IsCriticalSuccess"/> for critical detection.
    /// </remarks>
    [Obsolete("Use IsCriticalSuccess for success-counting mechanics. IsNaturalMax preserved for damage rolls only.")]
    public bool IsNaturalMax => Rolls.Count > 0 && Rolls[0] == Pool.Faces;

    /// <summary>
    /// True if the first die rolled a 1.
    /// </summary>
    /// <remarks>
    /// Preserved for backward compatibility. In the new system,
    /// use <see cref="IsFumble"/> for fumble detection.
    /// </remarks>
    [Obsolete("Use IsFumble for success-counting mechanics. IsNaturalOne preserved for damage rolls only.")]
    public bool IsNaturalOne => Rolls.Count > 0 && Rolls[0] == 1;

    /// <summary>True if any dice exploded.</summary>
    public bool HadExplosions => ExplosionRolls.Count > 0;

    /// <summary>Number of explosions that occurred.</summary>
    public int ExplosionCount => ExplosionRolls.Count;

    /// <summary>
    /// Total number of dice evaluated (base rolls + explosions).
    /// </summary>
    public int TotalDiceEvaluated => Rolls.Count + ExplosionRolls.Count;

    /// <summary>
    /// Creates a roll result with success-counting properties calculated.
    /// </summary>
    /// <param name="pool">The dice pool that was rolled.</param>
    /// <param name="rolls">Individual die results.</param>
    /// <param name="advantageType">Advantage type used.</param>
    /// <param name="explosionRolls">Explosion roll results (optional).</param>
    /// <param name="allRollTotals">All roll totals for advantage/disadvantage.</param>
    /// <param name="selectedRollIndex">Index of selected roll.</param>
    public DiceRollResult(
        DicePool pool,
        IReadOnlyList<int> rolls,
        AdvantageType advantageType = AdvantageType.Normal,
        IReadOnlyList<int>? explosionRolls = null,
        IReadOnlyList<int>? allRollTotals = null,
        int selectedRollIndex = 0)
    {
        Pool = pool;
        Rolls = rolls;
        ExplosionRolls = explosionRolls ?? Array.Empty<int>();
        AdvantageType = advantageType;
        SelectedRollIndex = selectedRollIndex;

        // Combine base rolls and explosions for counting
        var allDice = rolls.Concat(ExplosionRolls).ToList();

        // Count successes and botches
        TotalSuccesses = allDice.Count(d => d >= DiceConstants.SuccessThreshold);
        TotalBotches = allDice.Count(d => d == DiceConstants.BotchValue);

        // Calculate net successes (minimum 0)
        NetSuccesses = Math.Max(0, TotalSuccesses - TotalBotches);

        // Detect special outcomes
        IsFumble = TotalSuccesses == 0 && TotalBotches > 0;
        IsCriticalSuccess = NetSuccesses >= DiceConstants.CriticalSuccessNet;

        // Calculate raw totals (for damage and backward compatibility)
        RawTotal = allDice.Sum();
        DiceTotal = RawTotal;
        Total = RawTotal + pool.Modifier;

        // For advantage/disadvantage, store all totals (now using NetSuccesses)
        AllRollTotals = allRollTotals ?? new[] { NetSuccesses };
    }

    /// <summary>
    /// Returns a formatted string showing the roll breakdown with success counts.
    /// </summary>
    /// <example>
    /// "5d10: [1, 4, 8, 9, 1] → 2 successes - 2 botches = 0 net"
    /// "3d10: [8, 9, 10] → 3 successes - 0 botches = 3 net [CRITICAL!]"
    /// </example>
    public override string ToString()
    {
        var rollsStr = $"[{string.Join(", ", Rolls)}]";

        if (HadExplosions)
            rollsStr += $" + explosions [{string.Join(", ", ExplosionRolls)}]";

        var result = $"{Pool}: {rollsStr} → {TotalSuccesses} successes - {TotalBotches} botches = {NetSuccesses} net";

        if (IsFumble)
            result += " [FUMBLE!]";
        else if (IsCriticalSuccess)
            result += " [CRITICAL!]";

        if (AdvantageType != AdvantageType.Normal)
        {
            var allRolls = string.Join(", ", AllRollTotals);
            var typeStr = AdvantageType == AdvantageType.Advantage ? "ADV" : "DIS";
            result += $" ({typeStr}: [{allRolls}])";
        }

        return result;
    }

    /// <summary>
    /// Returns a formatted string showing legacy sum-based breakdown.
    /// </summary>
    /// <remarks>
    /// Use for damage rolls or other sum-based mechanics.
    /// </remarks>
    /// <example>
    /// "3d6+2: [4, 2, 5] +2 = 13"
    /// </example>
    public string ToSumString()
    {
        var rollsStr = $"[{string.Join(", ", Rolls)}]";

        if (HadExplosions)
            rollsStr += $" + explosions [{string.Join(", ", ExplosionRolls)}]";

        var result = $"{Pool}: {rollsStr}";

        if (Pool.Modifier != 0)
        {
            var modSign = Pool.Modifier > 0 ? "+" : "";
            result += $" {modSign}{Pool.Modifier}";
        }

        result += $" = {Total}";

        return result;
    }
}
