using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;

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

    /// <summary>
    /// Executes a player attack against a named target.
    /// Validates turn order, stamina availability, resolves the attack, and applies damage.
    /// </summary>
    /// <param name="targetName">The name (or partial name) of the target to attack.</param>
    /// <param name="attackType">The type of attack to perform.</param>
    /// <returns>A narrative message describing the attack result.</returns>
    string ExecutePlayerAttack(string targetName, AttackType attackType);

    /// <summary>
    /// Removes a defeated combatant from the turn order.
    /// Adjusts the turn index if necessary to maintain correct turn flow.
    /// </summary>
    /// <param name="combatant">The defeated combatant to remove.</param>
    void RemoveDefeatedCombatant(Combatant combatant);

    /// <summary>
    /// Checks if the player has won the combat encounter.
    /// Victory occurs when no enemies remain in the turn order.
    /// </summary>
    /// <returns>True if all enemies are defeated, false otherwise.</returns>
    bool CheckVictoryCondition();

    /// <summary>
    /// Gets the current combat status including HP and stamina for all combatants.
    /// </summary>
    /// <returns>A formatted string showing the status of all combatants.</returns>
    string GetCombatStatus();
}
