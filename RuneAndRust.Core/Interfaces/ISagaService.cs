using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for managing character progression (Legend, Levels, Milestones).
/// The Saga system tracks experience accumulation and level advancement.
/// </summary>
/// <remarks>See: v0.4.0a (The Legend) for implementation details.</remarks>
public interface ISagaService
{
    /// <summary>
    /// Adds Legend (XP) to the character and checks for level-up.
    /// If the character's Legend crosses a milestone threshold, they will level up
    /// and receive Progression Points. Multiple level-ups can occur in a single call.
    /// </summary>
    /// <param name="character">The character to award Legend to.</param>
    /// <param name="amount">The amount of Legend to add. Must be positive.</param>
    /// <param name="reason">A log-friendly description of the source (e.g., "Defeated Draugr").</param>
    void AddLegend(Character character, int amount, string reason);

    /// <summary>
    /// Gets the Legend required to reach the next level.
    /// </summary>
    /// <param name="currentLevel">The character's current level.</param>
    /// <returns>The total cumulative Legend needed to reach the next level, or -1 if at max level.</returns>
    int GetLegendForNextLevel(int currentLevel);

    /// <summary>
    /// Gets the Progression Points awarded for reaching a specific level.
    /// </summary>
    /// <param name="level">The level reached.</param>
    /// <returns>The PP awarded for reaching that level, or 0 if invalid.</returns>
    int GetPpAward(int level);
}
