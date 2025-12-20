using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for resource management during combat.
/// Handles spending, regeneration, and validation of resources (Stamina, Aether).
/// </summary>
public interface IResourceService
{
    /// <summary>
    /// Checks if a combatant can afford a resource cost.
    /// For Aether, Mystics can always "afford" via Overcast if they have sufficient HP.
    /// </summary>
    /// <param name="combatant">The combatant attempting to spend.</param>
    /// <param name="type">The type of resource to check.</param>
    /// <param name="cost">The amount required.</param>
    /// <returns>True if the cost can be paid, false otherwise.</returns>
    bool CanAfford(Combatant combatant, ResourceType type, int cost);

    /// <summary>
    /// Deducts a resource cost from a combatant.
    /// For Aether with insufficient AP, Mystics will Overcast (spend HP at 2:1 ratio).
    /// </summary>
    /// <param name="combatant">The combatant spending the resource.</param>
    /// <param name="type">The type of resource to spend.</param>
    /// <param name="cost">The amount to deduct.</param>
    /// <returns>True if deduction succeeded, false if insufficient resources.</returns>
    bool Deduct(Combatant combatant, ResourceType type, int cost);

    /// <summary>
    /// Regenerates stamina at the start of a combatant's turn.
    /// Formula: Base 5 + (Finesse / 2), clamped to MaxStamina.
    /// No regeneration if Stunned or Exhausted.
    /// </summary>
    /// <param name="combatant">The combatant to regenerate.</param>
    /// <returns>The amount of stamina regenerated (0 if none).</returns>
    int RegenerateStamina(Combatant combatant);

    /// <summary>
    /// Gets the current amount of a resource for a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <param name="type">The type of resource.</param>
    /// <returns>The current resource value.</returns>
    int GetCurrent(Combatant combatant, ResourceType type);

    /// <summary>
    /// Gets the maximum amount of a resource for a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <param name="type">The type of resource.</param>
    /// <returns>The maximum resource value.</returns>
    int GetMax(Combatant combatant, ResourceType type);

    /// <summary>
    /// Checks if a combatant is a Mystic archetype (can Overcast).
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>True if the combatant is a Mystic, false otherwise.</returns>
    bool IsMystic(Combatant combatant);
}
