using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Interface for dice rolling services.
/// </summary>
/// <remarks>
/// Abstraction allows for mock implementations in testing and
/// alternative implementations (e.g., seeded random, fixed rolls).
/// </remarks>
public interface IDiceService
{
    /// <summary>
    /// Rolls a dice pool and returns the result.
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <param name="advantageType">Whether to roll with advantage or disadvantage.</param>
    /// <param name="context">Roll context for logging (default: General). See <see cref="RollContexts"/>.</param>
    /// <param name="actorId">Optional actor ID for roll attribution.</param>
    /// <param name="targetId">Optional target ID for roll attribution.</param>
    /// <returns>The complete roll result with breakdown.</returns>
    /// <remarks>
    /// <b>v0.15.0e:</b> Added context and actor/target parameters for structured roll logging.
    /// </remarks>
    DiceRollResult Roll(
        DicePool pool,
        AdvantageType advantageType = AdvantageType.Normal,
        string context = RollContexts.Default,
        Guid? actorId = null,
        Guid? targetId = null);


    /// <summary>
    /// Parses dice notation and rolls.
    /// </summary>
    /// <param name="notation">Dice notation (e.g., "3d6+5").</param>
    /// <param name="advantageType">Advantage/disadvantage.</param>
    /// <returns>Roll result.</returns>
    DiceRollResult Roll(string notation, AdvantageType advantageType = AdvantageType.Normal);

    /// <summary>
    /// Convenience method for rolling a single die type.
    /// </summary>
    DiceRollResult Roll(DiceType diceType, int count = 1, int modifier = 0);

    /// <summary>
    /// Quick roll returning just the total.
    /// </summary>
    int RollTotal(DicePool pool);

    /// <summary>
    /// Quick roll returning just the total from notation.
    /// </summary>
    int RollTotal(string notation);
}
