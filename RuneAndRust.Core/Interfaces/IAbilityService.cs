using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for ability execution and cooldown management.
/// Parses EffectScript strings and applies their effects to combatants.
/// </summary>
/// <remarks>See: SPEC-ABILITY-001 for Ability System design.</remarks>
public interface IAbilityService
{
    /// <summary>
    /// Checks if a combatant can use the specified ability.
    /// Validates cooldown status and resource availability.
    /// </summary>
    /// <param name="user">The combatant attempting to use the ability.</param>
    /// <param name="ability">The ability to check.</param>
    /// <returns>True if the ability can be used; otherwise, false.</returns>
    bool CanUse(Combatant user, ActiveAbility ability);

    /// <summary>
    /// Executes an ability, applying its effects to the target.
    /// Deducts resources, parses the EffectScript, and sets cooldown.
    /// </summary>
    /// <param name="user">The combatant using the ability.</param>
    /// <param name="target">The target of the ability.</param>
    /// <param name="ability">The ability to execute.</param>
    /// <returns>An AbilityResult containing success status and effect details.</returns>
    AbilityResult Execute(Combatant user, Combatant target, ActiveAbility ability);

    /// <summary>
    /// Decrements all ability cooldowns for a combatant by 1.
    /// Removes cooldowns that reach 0. Called at the start of each turn.
    /// </summary>
    /// <param name="combatant">The combatant whose cooldowns to process.</param>
    void ProcessCooldowns(Combatant combatant);

    /// <summary>
    /// Gets the remaining cooldown turns for a specific ability.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <param name="abilityId">The ability ID to check.</param>
    /// <returns>Remaining turns until ready, or 0 if the ability is ready.</returns>
    int GetCooldownRemaining(Combatant combatant, Guid abilityId);
}
