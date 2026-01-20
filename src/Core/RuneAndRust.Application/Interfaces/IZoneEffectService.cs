using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing persistent zone effects during combat.
/// </summary>
/// <remarks>
/// <para>IZoneEffectService is responsible for the complete lifecycle of zone effects:</para>
/// <list type="bullet">
///   <item><description>Creating zones from definitions with calculated affected cells</description></item>
///   <item><description>Tracking active zones during combat</description></item>
///   <item><description>Ticking zones each round to apply effects and manage duration</description></item>
///   <item><description>Querying zones by position, caster, or other criteria</description></item>
///   <item><description>Handling entity movement into/out of zones</description></item>
/// </list>
/// <para>Zone effects are processed during the round tick phase, after initiative but before
/// individual turns are processed.</para>
/// </remarks>
public interface IZoneEffectService
{
    // ═══════════════════════════════════════════════════════════════
    // ZONE CREATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new zone effect at the specified position.
    /// </summary>
    /// <param name="zoneId">The zone definition ID.</param>
    /// <param name="center">The center position of the zone.</param>
    /// <param name="caster">The combatant creating the zone.</param>
    /// <returns>The created zone effect.</returns>
    /// <exception cref="ArgumentException">Thrown when zone definition is not found.</exception>
    /// <remarks>
    /// <para>For zones that require a direction (Line, Cone shapes), this method uses
    /// a default direction of North. Use the overload with direction for explicit control.</para>
    /// <para>Affected cells are calculated based on the zone's shape and radius,
    /// filtered to valid grid positions.</para>
    /// </remarks>
    ZoneEffect CreateZone(string zoneId, GridPosition center, Combatant caster);

    /// <summary>
    /// Creates a zone with a specific direction (for Line and Cone shapes).
    /// </summary>
    /// <param name="zoneId">The zone definition ID.</param>
    /// <param name="center">The center position of the zone.</param>
    /// <param name="direction">The direction the zone extends in.</param>
    /// <param name="caster">The combatant creating the zone.</param>
    /// <returns>The created zone effect.</returns>
    /// <exception cref="ArgumentException">Thrown when zone definition is not found.</exception>
    /// <remarks>
    /// <para>Direction is used for Line and Cone shapes to determine which cells are affected.</para>
    /// <para>For Circle, Square, and Ring shapes, direction is ignored.</para>
    /// </remarks>
    ZoneEffect CreateZone(string zoneId, GridPosition center, Direction direction, Combatant caster);

    // ═══════════════════════════════════════════════════════════════
    // ZONE REMOVAL
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Removes a zone by its effect ID.
    /// </summary>
    /// <param name="zoneEffectId">The unique ID of the zone effect to remove.</param>
    /// <returns>True if the zone was removed; false if not found.</returns>
    bool RemoveZone(Guid zoneEffectId);

    /// <summary>
    /// Removes all zones created by a specific caster.
    /// </summary>
    /// <param name="casterId">The caster's entity ID.</param>
    /// <returns>The number of zones removed.</returns>
    int RemoveZonesByCaster(Guid casterId);

    /// <summary>
    /// Clears all active zones (typically called when combat ends).
    /// </summary>
    void ClearAllZones();

    // ═══════════════════════════════════════════════════════════════
    // ZONE QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all zones affecting a specific position.
    /// </summary>
    /// <param name="position">The grid position to check.</param>
    /// <returns>List of zones that include this position in their affected area.</returns>
    IReadOnlyList<ZoneEffect> GetZonesAt(GridPosition position);

    /// <summary>
    /// Gets all currently active zones.
    /// </summary>
    /// <returns>List of all active zone effects.</returns>
    IReadOnlyList<ZoneEffect> GetAllActiveZones();

    /// <summary>
    /// Gets all zones created by a specific caster.
    /// </summary>
    /// <param name="casterId">The caster's entity ID.</param>
    /// <returns>List of zones created by the caster.</returns>
    IReadOnlyList<ZoneEffect> GetZonesByCaster(Guid casterId);

    /// <summary>
    /// Gets zones filtered by effect type.
    /// </summary>
    /// <param name="effectType">The effect type to filter by.</param>
    /// <returns>List of zones with the specified effect type.</returns>
    IReadOnlyList<ZoneEffect> GetZonesByType(ZoneEffectType effectType);

    /// <summary>
    /// Gets the total count of active zones.
    /// </summary>
    /// <returns>The number of active zones.</returns>
    int GetActiveZoneCount();

    /// <summary>
    /// Gets the number of zones created by a specific caster.
    /// </summary>
    /// <param name="casterId">The caster's entity ID.</param>
    /// <returns>The count of zones by this caster.</returns>
    int GetZoneCountByCaster(Guid casterId);

    // ═══════════════════════════════════════════════════════════════
    // ZONE PROCESSING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Ticks all zones and applies their effects to combatants within their areas.
    /// </summary>
    /// <param name="combatants">All combatants currently in combat.</param>
    /// <returns>Result containing damage dealt, healing done, and expired zones.</returns>
    /// <remarks>
    /// <para>This method should be called once per round during combat processing.</para>
    /// <para>Processing order:</para>
    /// <list type="number">
    ///   <item><description>For each zone, identify combatants within affected cells</description></item>
    ///   <item><description>Apply zone effects based on friendly/enemy targeting rules</description></item>
    ///   <item><description>Decrement zone duration</description></item>
    ///   <item><description>Remove expired zones and fire expiration events</description></item>
    /// </list>
    /// </remarks>
    ZoneTickResult TickZones(IEnumerable<Combatant> combatants);

    // ═══════════════════════════════════════════════════════════════
    // ENTITY MOVEMENT TRACKING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Called when an entity enters a position that may be within a zone.
    /// </summary>
    /// <param name="combatant">The combatant that moved.</param>
    /// <param name="position">The new position.</param>
    /// <remarks>
    /// <para>Used to track zone entry for buff/debuff application and logging.</para>
    /// </remarks>
    void OnEntityEntered(Combatant combatant, GridPosition position);

    /// <summary>
    /// Called when an entity exits a position that may be within a zone.
    /// </summary>
    /// <param name="combatant">The combatant that moved.</param>
    /// <param name="position">The previous position.</param>
    /// <remarks>
    /// <para>Used to track zone exit for buff/debuff removal and logging.</para>
    /// </remarks>
    void OnEntityExited(Combatant combatant, GridPosition position);

    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a position is within any active zone.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>True if the position is within at least one zone.</returns>
    bool IsPositionInZone(GridPosition position);

    /// <summary>
    /// Checks if a position is within a damaging zone.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>True if the position is within a damage zone.</returns>
    bool IsPositionInDamageZone(GridPosition position);

    /// <summary>
    /// Checks if a position is within a healing zone.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>True if the position is within a healing zone.</returns>
    bool IsPositionInHealingZone(GridPosition position);
}
