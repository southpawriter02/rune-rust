using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for combat lifecycle management.
/// </summary>
public interface ICombatService
{
    /// <summary>
    /// Starts a new combat encounter with the specified enemies.
    /// Rolls initiative for all combatants and sets the game phase to Combat.
    /// </summary>
    /// <param name="enemies">The list of enemies to fight.</param>
    void StartCombat(List<Enemy> enemies);

    /// <summary>
    /// Advances to the next combatant's turn.
    /// Increments the round number when all combatants have acted.
    /// </summary>
    void NextTurn();

    /// <summary>
    /// Ends the current combat encounter.
    /// Clears the combat state and returns to Exploration phase.
    /// </summary>
    void EndCombat();
}
