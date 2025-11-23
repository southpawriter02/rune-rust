namespace RuneAndRust.Core;

/// <summary>
/// Interface for dice rolling functionality (v0.39.1)
/// Used by combat and traversal systems
/// </summary>
public interface IDiceRoller
{
    /// <summary>
    /// Rolls dice and returns the total
    /// </summary>
    /// <param name="count">Number of dice to roll</param>
    /// <param name="sides">Number of sides on each die</param>
    /// <returns>Total of all dice rolled</returns>
    int Roll(int count, int sides);
}
