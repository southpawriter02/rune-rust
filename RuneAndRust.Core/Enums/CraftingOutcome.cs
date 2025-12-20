namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the possible outcomes of a crafting attempt.
/// Outcome is determined by comparing net successes (successes - botches) to the recipe's DC.
/// </summary>
public enum CraftingOutcome
{
    /// <summary>
    /// Net successes below DC but zero or positive.
    /// Ingredients are consumed but no output is produced.
    /// </summary>
    Failure = 0,

    /// <summary>
    /// Net successes meet or exceed DC.
    /// Produces output at Good quality tier.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Net successes exceed DC by 5 or more.
    /// Produces output at Masterwork quality tier.
    /// </summary>
    Masterwork = 2,

    /// <summary>
    /// Net successes are negative (more botches than successes).
    /// Ingredients are consumed, no output, possible negative effects.
    /// </summary>
    Catastrophe = 3
}
