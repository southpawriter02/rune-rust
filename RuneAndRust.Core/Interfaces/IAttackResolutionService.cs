using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for attack resolution services.
/// Handles the mechanics of determining hit/miss, damage calculation, and attack outcomes.
/// </summary>
/// <remarks>See: SPEC-ATTACK-001 for Attack Resolution System design.</remarks>
public interface IAttackResolutionService
{
    /// <summary>
    /// Resolves a melee attack between an attacker and defender.
    /// </summary>
    /// <param name="attacker">The attacking combatant.</param>
    /// <param name="defender">The defending combatant.</param>
    /// <param name="attackType">The type of attack (Light, Standard, Heavy).</param>
    /// <returns>The result of the attack including outcome, damage, and hit status.</returns>
    AttackResult ResolveMeleeAttack(Combatant attacker, Combatant defender, AttackType attackType);

    /// <summary>
    /// Gets the stamina cost for a given attack type.
    /// </summary>
    /// <param name="attackType">The type of attack.</param>
    /// <returns>The stamina cost in points.</returns>
    int GetStaminaCost(AttackType attackType);

    /// <summary>
    /// Checks if a combatant has enough stamina to perform an attack.
    /// </summary>
    /// <param name="combatant">The combatant attempting the attack.</param>
    /// <param name="attackType">The type of attack.</param>
    /// <returns>True if the combatant can afford the attack, false otherwise.</returns>
    bool CanAffordAttack(Combatant combatant, AttackType attackType);

    /// <summary>
    /// Calculates a combatant's Defense Score.
    /// Formula: 10 + FINESSE - Stress (Stress defaults to 0 until implemented).
    /// </summary>
    /// <param name="defender">The defending combatant.</param>
    /// <returns>The defender's Defense Score.</returns>
    int CalculateDefenseScore(Combatant defender);

    /// <summary>
    /// Converts a Defense Score to a Success Threshold.
    /// Formula: Defense Score / 5 (rounded down).
    /// </summary>
    /// <param name="defenseScore">The defender's Defense Score.</param>
    /// <returns>The number of successes required to hit.</returns>
    int GetSuccessThreshold(int defenseScore);
}
