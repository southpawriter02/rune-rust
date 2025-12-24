namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Represents the result of a dice pool roll.
/// </summary>
/// <param name="Successes">Number of dice that rolled 8 or higher (successes).</param>
/// <param name="Botches">Number of dice that rolled 1 (botches).</param>
/// <param name="Rolls">The individual roll values for each die in the pool.</param>
public record DiceResult(int Successes, int Botches, IReadOnlyList<int> Rolls);

/// <summary>
/// Defines the contract for dice rolling services.
/// Implements a d10 dice pool system where 8+ is a success and 1 is a botch.
/// </summary>
/// <remarks>See: SPEC-DICE-001 for Dice Pool System design.</remarks>
public interface IDiceService
{
    /// <summary>
    /// Rolls a pool of d10 dice and calculates successes and botches.
    /// </summary>
    /// <param name="poolSize">The number of dice to roll. Must be at least 1.</param>
    /// <param name="context">Optional context describing what the roll is for (used in logging).</param>
    /// <returns>A <see cref="DiceResult"/> containing the successes, botches, and individual rolls.</returns>
    DiceResult Roll(int poolSize, string context = "Unspecified");

    /// <summary>
    /// Rolls a single die and returns the raw value.
    /// </summary>
    /// <param name="sides">The number of sides on the die (e.g., 10 for d10).</param>
    /// <param name="context">Optional context describing what the roll is for (used in logging).</param>
    /// <returns>The raw die value (1 to sides, inclusive).</returns>
    int RollSingle(int sides, string context = "Unspecified");
}
