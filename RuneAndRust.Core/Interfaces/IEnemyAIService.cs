using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for enemy AI decision-making during combat.
/// </summary>
/// <remarks>See: SPEC-AI-001 for Enemy AI & Behavior System design.</remarks>
public interface IEnemyAIService
{
    /// <summary>
    /// Determines the action an enemy should take based on archetype and combat state.
    /// </summary>
    /// <param name="enemy">The enemy combatant making the decision.</param>
    /// <param name="state">The current combat state with all combatants.</param>
    /// <returns>A CombatAction describing what the enemy will do.</returns>
    CombatAction DetermineAction(Combatant enemy, CombatState state);
}
