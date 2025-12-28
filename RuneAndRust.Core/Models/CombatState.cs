using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Represents the state of an active combat encounter.
/// Tracks turn order, round number, and the currently active combatant.
/// </summary>
public class CombatState
{
    /// <summary>
    /// Unique identifier for this combat encounter.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The ordered list of combatants, sorted by initiative.
    /// </summary>
    public List<Combatant> TurnOrder { get; set; } = new();

    /// <summary>
    /// The current round number. Starts at 1.
    /// </summary>
    public int RoundNumber { get; set; } = 1;

    /// <summary>
    /// Accumulated XP pool from defeated enemies during this combat.
    /// Awarded to the player character on victory via SagaService.
    /// </summary>
    /// <remarks>See: v0.4.0d (The Reward) for XP Integration.</remarks>
    public int XpPool { get; set; } = 0;

    /// <summary>
    /// The index of the currently active combatant in the TurnOrder list.
    /// </summary>
    public int TurnIndex { get; set; } = 0;

    /// <summary>
    /// Gets the currently active combatant, or null if TurnOrder is empty.
    /// </summary>
    public Combatant? ActiveCombatant =>
        TurnOrder.Count > TurnIndex ? TurnOrder[TurnIndex] : null;

    /// <summary>
    /// Gets whether it is currently the player's turn.
    /// </summary>
    public bool IsPlayerTurn => ActiveCombatant?.IsPlayer ?? false;
}
