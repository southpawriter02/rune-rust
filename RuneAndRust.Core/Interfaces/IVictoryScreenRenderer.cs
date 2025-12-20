using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for rendering the post-combat victory screen.
/// Implementations display loot drops, XP rewards, and combat summary.
/// </summary>
public interface IVictoryScreenRenderer
{
    /// <summary>
    /// Renders the victory screen with loot and XP rewards.
    /// Blocks until the player acknowledges the screen.
    /// </summary>
    /// <param name="result">The combat result containing victory state, rewards, and summary.</param>
    void Render(CombatResult result);
}
