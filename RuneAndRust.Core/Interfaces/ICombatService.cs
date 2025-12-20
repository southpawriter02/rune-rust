using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Core.ViewModels;

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
    /// Clears the combat state, generates loot on victory, and returns to Exploration phase.
    /// </summary>
    /// <returns>A CombatResult containing victory state, XP earned, and loot found, or null if no combat was active.</returns>
    CombatResult? EndCombat();

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

    /// <summary>
    /// Gets an immutable view model snapshot of the current combat state for UI rendering.
    /// Transforms raw combat data into display-ready format with narrative health for enemies.
    /// </summary>
    /// <returns>A CombatViewModel containing all display data, or null if not in combat.</returns>
    CombatViewModel? GetViewModel();

    /// <summary>
    /// Adds a message to the player-visible combat log.
    /// The log maintains a rolling buffer of the last 10 events.
    /// </summary>
    /// <param name="message">The formatted message to add (may include Spectre markup).</param>
    void LogCombatEvent(string message);

    /// <summary>
    /// Executes an enemy's attack against a target.
    /// Called by the combat loop after AI determines action.
    /// </summary>
    /// <param name="attacker">The attacking enemy combatant.</param>
    /// <param name="target">The target combatant (player).</param>
    /// <param name="attackType">The type of attack to perform.</param>
    /// <returns>A narrative message describing the attack result.</returns>
    string ExecuteEnemyAttack(Combatant attacker, Combatant target, AttackType attackType);

    /// <summary>
    /// Processes a defend action, granting temporary soak bonus.
    /// </summary>
    /// <param name="combatant">The defending combatant.</param>
    void ProcessDefend(Combatant combatant);

    /// <summary>
    /// Processes a flee action, removing the combatant from combat.
    /// </summary>
    /// <param name="combatant">The fleeing combatant.</param>
    void ProcessFlee(Combatant combatant);

    /// <summary>
    /// Processes an enemy's turn using AI to determine and execute their action.
    /// Called automatically when it becomes an enemy combatant's turn.
    /// </summary>
    /// <param name="enemy">The enemy combatant whose turn it is.</param>
    /// <returns>A task that completes when the enemy turn is processed.</returns>
    Task ProcessEnemyTurnAsync(Combatant enemy);

    /// <summary>
    /// Gets the list of abilities available to the player during combat.
    /// </summary>
    /// <returns>The list of abilities for the current player combatant.</returns>
    List<ActiveAbility> GetPlayerAbilities();

    /// <summary>
    /// Executes a player ability by hotkey index (1-based).
    /// Auto-targets if only one valid target exists.
    /// </summary>
    /// <param name="hotkey">The 1-based hotkey index of the ability.</param>
    /// <param name="targetName">Optional target name for multi-enemy scenarios.</param>
    /// <returns>A narrative message describing the ability result.</returns>
    string ExecutePlayerAbility(int hotkey, string? targetName = null);

    /// <summary>
    /// Executes a player ability by name (partial match supported).
    /// Auto-targets if only one valid target exists.
    /// </summary>
    /// <param name="abilityName">The name (or partial name) of the ability to use.</param>
    /// <param name="targetName">Optional target name for multi-enemy scenarios.</param>
    /// <returns>A narrative message describing the ability result.</returns>
    string ExecutePlayerAbility(string abilityName, string? targetName = null);
}
