using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing combat stances and their effects on combatants.
/// </summary>
/// <remarks>
/// <para>IStanceService coordinates stance changes, applies/removes stat modifiers,
/// and enforces the once-per-round stance change limit.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
///   <item><description>Track current stance per combatant</description></item>
///   <item><description>Apply/remove stat modifiers when stances change</description></item>
///   <item><description>Enforce once-per-round change limit</description></item>
///   <item><description>Provide stance modifier values for combat calculations</description></item>
/// </list>
/// </remarks>
public interface IStanceService
{
    /// <summary>
    /// Gets the current combat stance of a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>The combatant's current <see cref="CombatStance"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    CombatStance GetCurrentStance(Combatant combatant);

    /// <summary>
    /// Gets the stance definition for a combatant's current stance.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>
    /// The <see cref="StanceDefinition"/> for the combatant's current stance,
    /// or null if the stance is not configured.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    StanceDefinition? GetStanceDefinition(Combatant combatant);

    /// <summary>
    /// Attempts to change a combatant's combat stance.
    /// </summary>
    /// <param name="combatant">The combatant changing stance.</param>
    /// <param name="stance">The new stance to adopt.</param>
    /// <returns>
    /// A <see cref="StanceChangeResult"/> indicating success or failure.
    /// </returns>
    /// <remarks>
    /// <para>This method handles:</para>
    /// <list type="bullet">
    ///   <item><description>Checking if the combatant can change stance this round</description></item>
    ///   <item><description>Removing old stance modifiers</description></item>
    ///   <item><description>Setting the new stance on the combatant</description></item>
    ///   <item><description>Applying new stance modifiers</description></item>
    ///   <item><description>Publishing the StanceChangedEvent</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    StanceChangeResult SetStance(Combatant combatant, CombatStance stance);

    /// <summary>
    /// Checks if a combatant can change their stance this round.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>True if the combatant has not changed stance this round, false otherwise.</returns>
    /// <remarks>
    /// Combatants can change stance once per round as a free action.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    bool CanChangeStance(Combatant combatant);

    /// <summary>
    /// Gets the attack bonus from the combatant's current stance.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>
    /// The attack modifier (positive or negative) from the current stance,
    /// or 0 if no stance is configured.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    int GetAttackBonus(Combatant combatant);

    /// <summary>
    /// Gets the damage bonus dice notation from the combatant's current stance.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>
    /// A dice notation string (e.g., "1d4", "-1d4") from the current stance,
    /// or null if no damage bonus applies.
    /// </returns>
    /// <remarks>
    /// The damage bonus should be rolled and added to weapon damage on hit.
    /// Negative notation (e.g., "-1d4") means roll the dice and subtract from damage.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    string? GetDamageBonus(Combatant combatant);

    /// <summary>
    /// Gets the defense bonus from the combatant's current stance.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>
    /// The defense modifier (positive or negative) from the current stance,
    /// or 0 if no stance is configured.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    int GetDefenseBonus(Combatant combatant);

    /// <summary>
    /// Gets the saving throw bonus from the combatant's current stance.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>
    /// The save modifier (positive or negative) from the current stance,
    /// or 0 if no stance is configured.
    /// </returns>
    /// <remarks>
    /// This bonus applies to all saving throws (fortitude, reflex, and will).
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    int GetSaveBonus(Combatant combatant);

    /// <summary>
    /// Resets a combatant's ability to change stance for a new round.
    /// </summary>
    /// <param name="combatant">The combatant to reset.</param>
    /// <remarks>
    /// Should be called at the start of each combat round to allow
    /// the combatant to change stance again.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    void ResetStanceChange(Combatant combatant);

    /// <summary>
    /// Resets stance change availability for all combatants at once.
    /// </summary>
    /// <remarks>
    /// Convenience method for round transitions. Clears the internal
    /// tracking of which combatants have changed stance.
    /// </remarks>
    void ResetAllStanceChanges();

    /// <summary>
    /// Gets all available combat stances.
    /// </summary>
    /// <returns>A read-only list of all configured stance definitions.</returns>
    IReadOnlyList<StanceDefinition> GetAvailableStances();

    /// <summary>
    /// Initializes a combatant with the default stance.
    /// </summary>
    /// <param name="combatant">The combatant to initialize.</param>
    /// <remarks>
    /// Sets the combatant's stance to the default (typically Balanced)
    /// without applying modifiers (default has no modifiers).
    /// Should be called when a combatant enters combat.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    void InitializeStance(Combatant combatant);
}
