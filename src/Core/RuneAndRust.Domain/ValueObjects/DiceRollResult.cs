using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of rolling a dice pool.
/// </summary>
/// <remarks>
/// Immutable value object containing all roll details including
/// individual dice, explosions, advantage/disadvantage selections, and criticals.
/// </remarks>
public readonly record struct DiceRollResult
{
    /// <summary>The dice pool that was rolled.</summary>
    public DicePool Pool { get; init; }

    /// <summary>Individual dice results before modifier.</summary>
    public IReadOnlyList<int> Rolls { get; init; }

    /// <summary>Additional rolls from exploding dice.</summary>
    public IReadOnlyList<int> ExplosionRolls { get; init; }

    /// <summary>Total of all dice (including explosions, before modifier).</summary>
    public int DiceTotal { get; init; }

    /// <summary>Final total including modifier.</summary>
    public int Total { get; init; }

    /// <summary>Advantage type used for this roll.</summary>
    public AdvantageType AdvantageType { get; init; }

    /// <summary>All rolls made (for advantage/disadvantage comparison).</summary>
    public IReadOnlyList<int> AllRollTotals { get; init; }

    /// <summary>Index of the selected roll (0 for normal, 0 or 1 for advantage/disadvantage).</summary>
    public int SelectedRollIndex { get; init; }

    /// <summary>True if the first die rolled its maximum value.</summary>
    public bool IsNaturalMax => Rolls.Count > 0 && Rolls[0] == Pool.Faces;

    /// <summary>True if the first die rolled a 1.</summary>
    public bool IsNaturalOne => Rolls.Count > 0 && Rolls[0] == 1;

    /// <summary>True if any dice exploded.</summary>
    public bool HadExplosions => ExplosionRolls.Count > 0;

    /// <summary>Number of explosions that occurred.</summary>
    public int ExplosionCount => ExplosionRolls.Count;

    /// <summary>
    /// Creates a roll result with required fields.
    /// </summary>
    /// <param name="pool">The dice pool that was rolled.</param>
    /// <param name="rolls">Individual die results.</param>
    /// <param name="total">Final total including modifier.</param>
    /// <param name="advantageType">Advantage type used.</param>
    /// <param name="explosionRolls">Explosion roll results.</param>
    /// <param name="allRollTotals">All roll totals for advantage/disadvantage.</param>
    /// <param name="selectedRollIndex">Index of selected roll.</param>
    public DiceRollResult(
        DicePool pool,
        IReadOnlyList<int> rolls,
        int total,
        AdvantageType advantageType = AdvantageType.Normal,
        IReadOnlyList<int>? explosionRolls = null,
        IReadOnlyList<int>? allRollTotals = null,
        int selectedRollIndex = 0)
    {
        Pool = pool;
        Rolls = rolls;
        ExplosionRolls = explosionRolls ?? Array.Empty<int>();
        DiceTotal = rolls.Sum() + (explosionRolls?.Sum() ?? 0);
        Total = total;
        AdvantageType = advantageType;
        AllRollTotals = allRollTotals ?? new[] { total };
        SelectedRollIndex = selectedRollIndex;
    }

    /// <summary>
    /// Returns a formatted string showing the roll breakdown.
    /// </summary>
    /// <example>"3d6+2: [4, 2, 5] +2 = 13" or "1d10: [10] = 10 (ADV: [10, 7])"</example>
    public override string ToString()
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

        if (AdvantageType != AdvantageType.Normal)
        {
            var allRolls = string.Join(", ", AllRollTotals);
            var typeStr = AdvantageType == AdvantageType.Advantage ? "ADV" : "DIS";
            result += $" ({typeStr}: [{allRolls}])";
        }

        return result;
    }
}
