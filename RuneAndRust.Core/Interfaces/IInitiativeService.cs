using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for initiative calculation and turn order sorting.
/// </summary>
public interface IInitiativeService
{
    /// <summary>
    /// Rolls initiative for a combatant using the formula: d10 + FINESSE + WITS.
    /// Sets the <see cref="Combatant.Initiative"/> property.
    /// </summary>
    /// <param name="combatant">The combatant to roll initiative for.</param>
    void RollInitiative(Combatant combatant);

    /// <summary>
    /// Sorts combatants by initiative in descending order.
    /// Ties are broken by Finesse, then randomly.
    /// </summary>
    /// <param name="combatants">The combatants to sort.</param>
    /// <returns>A new list sorted by initiative (highest first).</returns>
    List<Combatant> SortTurnOrder(IEnumerable<Combatant> combatants);
}
