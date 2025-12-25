using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for dynamic hazard processing (v0.3.3a).
/// Handles hazard triggers, effect execution, and lifecycle state management.
/// </summary>
/// <remarks>See: SPEC-HAZARD-001 for Dynamic Hazard System design.</remarks>
public interface IHazardService
{
    /// <summary>
    /// Process hazards when a combatant enters a room (movement trigger).
    /// Called by NavigationService on room change.
    /// </summary>
    /// <param name="room">The room being entered.</param>
    /// <param name="entrant">Optional combatant entering the room. Null for exploration phase.</param>
    /// <returns>List of hazard results for all triggered hazards.</returns>
    Task<List<HazardResult>> TriggerOnRoomEnterAsync(Room room, Combatant? entrant = null);

    /// <summary>
    /// Process hazards when damage is dealt in the room.
    /// Called by CombatService after damage is applied.
    /// </summary>
    /// <param name="room">The room where damage occurred.</param>
    /// <param name="damageType">The type of damage dealt.</param>
    /// <param name="amount">The amount of damage dealt.</param>
    /// <param name="target">Optional combatant to receive hazard effects.</param>
    /// <returns>List of hazard results for all triggered hazards.</returns>
    Task<List<HazardResult>> TriggerOnDamageAsync(
        Room room,
        DamageType damageType,
        int amount,
        Combatant? target = null);

    /// <summary>
    /// Process turn-start hazards (periodic environmental effects).
    /// Called by CombatService at the start of each combat round.
    /// </summary>
    /// <param name="room">The room where combat is occurring.</param>
    /// <param name="combatants">All combatants in the room to potentially affect.</param>
    /// <returns>List of hazard results for all triggered hazards.</returns>
    Task<List<HazardResult>> ProcessTurnStartHazardsAsync(Room room, List<Combatant> combatants);

    /// <summary>
    /// Tick cooldowns at end of combat round.
    /// Hazards in Cooldown state decrement; at 0 they return to Dormant.
    /// </summary>
    /// <param name="room">The room to process cooldowns for.</param>
    Task TickCooldownsAsync(Room room);

    /// <summary>
    /// Get all active (non-Destroyed) hazards in a room.
    /// </summary>
    /// <param name="roomId">The room ID to query.</param>
    /// <returns>List of hazards that can still trigger.</returns>
    Task<List<DynamicHazard>> GetActiveHazardsAsync(Guid roomId);

    /// <summary>
    /// Manually activate a hazard by player interaction.
    /// Used for levers, switches, and interactive mechanisms.
    /// </summary>
    /// <param name="hazard">The hazard to activate.</param>
    /// <param name="activator">The combatant activating the hazard.</param>
    /// <returns>The result of the activation.</returns>
    Task<HazardResult> ManualActivateAsync(DynamicHazard hazard, Combatant activator);
}
