using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for creature trait generation and application.
/// </summary>
/// <remarks>See: SPEC-TRAIT-001 for Creature Trait System design.</remarks>
public interface ICreatureTraitService
{
    /// <summary>
    /// Enhances an enemy with traits based on their threat tier.
    /// Applies stat modifiers and adds traits to ActiveTraits list.
    /// </summary>
    /// <param name="enemy">The enemy to enhance.</param>
    void EnhanceEnemy(Enemy enemy);

    /// <summary>
    /// Processes trait effects at the start of a combatant's turn.
    /// </summary>
    /// <param name="combatant">The combatant whose turn is starting.</param>
    /// <returns>HP restored from regeneration (0 if none).</returns>
    int ProcessTraitTurnStart(Combatant combatant);

    /// <summary>
    /// Processes trait effects when a combatant dies.
    /// </summary>
    /// <param name="victim">The combatant that died.</param>
    /// <param name="allCombatants">All combatants in combat for AoE effects.</param>
    /// <returns>List of (Combatant, Damage) tuples for AoE damage.</returns>
    List<(Combatant Target, int Damage)> ProcessTraitOnDeath(Combatant victim, IEnumerable<Combatant> allCombatants);

    /// <summary>
    /// Processes trait effects when damage is dealt.
    /// </summary>
    /// <param name="attacker">The attacking combatant.</param>
    /// <param name="damageDealt">The amount of damage dealt.</param>
    /// <returns>HP healed from vampiric effect (0 if none).</returns>
    int ProcessTraitOnDamageDealt(Combatant attacker, int damageDealt);

    /// <summary>
    /// Processes trait effects when damage is received.
    /// </summary>
    /// <param name="defender">The defending combatant.</param>
    /// <param name="attacker">The attacking combatant.</param>
    /// <param name="damageReceived">The amount of damage received.</param>
    /// <returns>Thorns damage to reflect to attacker (0 if none).</returns>
    int ProcessTraitOnDamageReceived(Combatant defender, Combatant attacker, int damageReceived);

    /// <summary>
    /// Checks if a combatant is immune to a status effect due to traits.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <param name="effectType">The status effect type.</param>
    /// <returns>True if immune, false otherwise.</returns>
    bool IsImmuneToEffect(Combatant combatant, StatusEffectType effectType);
}
