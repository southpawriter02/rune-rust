namespace RuneAndRust.Application.Events;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Published when a zone effect is created on the combat grid.
/// </summary>
/// <remarks>
/// <para>This event is raised when a combatant creates a new zone, such as Wall of Fire,
/// Healing Circle, or other area effects.</para>
/// <para>The event contains the zone's position, caster information, and cell count
/// for UI updates and logging.</para>
/// </remarks>
/// <param name="ZoneEffectId">The unique identifier of the created zone effect instance.</param>
/// <param name="ZoneId">The zone definition ID (e.g., "wall-of-fire").</param>
/// <param name="Name">The display name of the zone (e.g., "Wall of Fire").</param>
/// <param name="Center">The center position of the zone on the combat grid.</param>
/// <param name="CasterId">The unique identifier of the combatant who created the zone.</param>
/// <param name="CellCount">The number of cells affected by the zone.</param>
/// <param name="Duration">The initial duration of the zone in turns.</param>
public record ZoneCreatedEvent(
    Guid ZoneEffectId,
    string ZoneId,
    string Name,
    GridPosition Center,
    Guid CasterId,
    int CellCount,
    int Duration);

/// <summary>
/// Published when a zone effect expires or is removed from the combat grid.
/// </summary>
/// <remarks>
/// <para>This event is raised when a zone's duration reaches zero or when a zone
/// is explicitly removed (e.g., by an ability or when combat ends).</para>
/// </remarks>
/// <param name="ZoneEffectId">The unique identifier of the expired zone effect instance.</param>
/// <param name="ZoneId">The zone definition ID (e.g., "healing-circle").</param>
/// <param name="Name">The display name of the zone (e.g., "Healing Circle").</param>
/// <param name="Reason">The reason for expiration ("Duration", "Removed", "Combat Ended").</param>
public record ZoneExpiredEvent(
    Guid ZoneEffectId,
    string ZoneId,
    string Name,
    string Reason = "Duration");

/// <summary>
/// Published when a zone deals damage to an entity.
/// </summary>
/// <remarks>
/// <para>This event is raised during zone tick processing when a damage zone
/// affects an entity within its area.</para>
/// <para>Damage zones affect entities based on their AffectsFriendly/AffectsEnemy settings.</para>
/// </remarks>
/// <param name="ZoneEffectId">The unique identifier of the zone effect instance.</param>
/// <param name="ZoneId">The zone definition ID (e.g., "wall-of-fire").</param>
/// <param name="TargetId">The unique identifier of the entity that took damage.</param>
/// <param name="TargetName">The display name of the entity that took damage.</param>
/// <param name="Damage">The amount of damage dealt.</param>
/// <param name="DamageType">The type of damage (e.g., "fire", "acid", "cold").</param>
public record ZoneDamageEvent(
    Guid ZoneEffectId,
    string ZoneId,
    Guid TargetId,
    string TargetName,
    int Damage,
    string DamageType);

/// <summary>
/// Published when a zone heals an entity.
/// </summary>
/// <remarks>
/// <para>This event is raised during zone tick processing when a healing zone
/// affects an entity within its area.</para>
/// <para>Healing zones typically only affect friendly entities (allies of the caster).</para>
/// </remarks>
/// <param name="ZoneEffectId">The unique identifier of the zone effect instance.</param>
/// <param name="ZoneId">The zone definition ID (e.g., "healing-circle").</param>
/// <param name="TargetId">The unique identifier of the entity that was healed.</param>
/// <param name="TargetName">The display name of the entity that was healed.</param>
/// <param name="HealAmount">The amount of healing done.</param>
public record ZoneHealEvent(
    Guid ZoneEffectId,
    string ZoneId,
    Guid TargetId,
    string TargetName,
    int HealAmount);

/// <summary>
/// Published when a zone applies a status effect to an entity.
/// </summary>
/// <remarks>
/// <para>This event is raised when Buff, Debuff, or Terrain type zones apply
/// their status effects to entities within their area.</para>
/// </remarks>
/// <param name="ZoneEffectId">The unique identifier of the zone effect instance.</param>
/// <param name="ZoneId">The zone definition ID (e.g., "slow-field").</param>
/// <param name="TargetId">The unique identifier of the entity that received the status.</param>
/// <param name="TargetName">The display name of the entity that received the status.</param>
/// <param name="StatusEffectId">The status effect definition ID that was applied.</param>
public record ZoneStatusAppliedEvent(
    Guid ZoneEffectId,
    string ZoneId,
    Guid TargetId,
    string TargetName,
    string StatusEffectId);

/// <summary>
/// Published when a combatant enters a zone's affected area.
/// </summary>
/// <remarks>
/// <para>This event is raised when a combatant moves into a position that is
/// within an active zone's affected cells.</para>
/// <para>Used for logging and for triggering on-entry effects.</para>
/// </remarks>
/// <param name="ZoneEffectId">The unique identifier of the zone effect instance.</param>
/// <param name="ZoneId">The zone definition ID.</param>
/// <param name="CombatantId">The unique identifier of the combatant who entered.</param>
/// <param name="CombatantName">The display name of the combatant who entered.</param>
/// <param name="Position">The position the combatant entered at.</param>
public record ZoneEnteredEvent(
    Guid ZoneEffectId,
    string ZoneId,
    Guid CombatantId,
    string CombatantName,
    GridPosition Position);

/// <summary>
/// Published when a combatant exits a zone's affected area.
/// </summary>
/// <remarks>
/// <para>This event is raised when a combatant moves out of a position that was
/// within an active zone's affected cells.</para>
/// <para>Used for removing zone-based buffs/debuffs that should end on exit.</para>
/// </remarks>
/// <param name="ZoneEffectId">The unique identifier of the zone effect instance.</param>
/// <param name="ZoneId">The zone definition ID.</param>
/// <param name="CombatantId">The unique identifier of the combatant who exited.</param>
/// <param name="CombatantName">The display name of the combatant who exited.</param>
/// <param name="Position">The position the combatant exited from.</param>
public record ZoneExitedEvent(
    Guid ZoneEffectId,
    string ZoneId,
    Guid CombatantId,
    string CombatantName,
    GridPosition Position);
