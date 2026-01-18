using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing environmental combat mechanics including push, knockback, and hazard interactions.
/// </summary>
/// <remarks>
/// <para>IEnvironmentalCombatService coordinates all push/knockback operations and hazard damage
/// during tactical combat on the grid.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
///   <item><description>Handle push operations with opposed STR checks</description></item>
///   <item><description>Handle knockback operations (forced movement without opposition)</description></item>
///   <item><description>Detect and process environmental hazards</description></item>
///   <item><description>Apply hazard damage and status effects</description></item>
///   <item><description>Process per-turn hazard tick damage</description></item>
///   <item><description>Handle critical hit knockback triggers</description></item>
/// </list>
/// <para>Push Mechanics:</para>
/// <list type="bullet">
///   <item><description>Opposed STR check: 1d20 + pusher STR mod vs 1d20 + target STR mod</description></item>
///   <item><description>Target wins ties (resists the push)</description></item>
///   <item><description>Movement stops at walls, other entities, or hazards</description></item>
/// </list>
/// <para>Knockback Mechanics:</para>
/// <list type="bullet">
///   <item><description>Forced movement with no opposed check</description></item>
///   <item><description>Direction is away from the source position</description></item>
///   <item><description>Critical hits trigger 1-cell knockback</description></item>
/// </list>
/// </remarks>
/// <seealso cref="PushResult"/>
/// <seealso cref="HazardDamageResult"/>
/// <seealso cref="IEnvironmentalHazardProvider"/>
public interface IEnvironmentalCombatService
{
    /// <summary>
    /// Attempts to push a target combatant using an opposed STR check.
    /// </summary>
    /// <param name="pusher">The combatant performing the push.</param>
    /// <param name="target">The combatant being pushed.</param>
    /// <param name="grid">The combat grid containing both combatants.</param>
    /// <param name="direction">The direction to push the target.</param>
    /// <param name="distance">The maximum number of cells to push (default 1).</param>
    /// <returns>
    /// A <see cref="PushResult"/> describing the outcome of the push attempt,
    /// including whether it was resisted, blocked, or successful.
    /// </returns>
    /// <remarks>
    /// <para>Push uses an opposed STR check where:</para>
    /// <list type="bullet">
    ///   <item><description>Pusher rolls: 1d20 + STR modifier</description></item>
    ///   <item><description>Target rolls: 1d20 + STR modifier</description></item>
    ///   <item><description>Target wins ties (push is resisted)</description></item>
    /// </list>
    /// <para>If the target is pushed into a hazard, hazard damage is applied automatically.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when pusher, target, or grid is null.</exception>
    PushResult Push(Combatant pusher, Combatant target, CombatGrid grid, MovementDirection direction, int distance = 1);

    /// <summary>
    /// Applies forced knockback movement to a target without an opposed check.
    /// </summary>
    /// <param name="target">The combatant being knocked back.</param>
    /// <param name="grid">The combat grid containing the target.</param>
    /// <param name="sourcePosition">The position the knockback originates from (target moves away).</param>
    /// <param name="distance">The number of cells to knock back (default 1).</param>
    /// <returns>
    /// A <see cref="PushResult"/> describing the outcome, including the final position
    /// and any hazard damage if the target was knocked into a hazard.
    /// </returns>
    /// <remarks>
    /// <para>Knockback is forced movement that does not allow an opposed STR check.</para>
    /// <para>The direction is calculated automatically away from the source position.</para>
    /// <para>If the target is knocked into a hazard, hazard damage is applied automatically.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when target or grid is null.</exception>
    PushResult Knockback(Combatant target, CombatGrid grid, GridPosition sourcePosition, int distance = 1);

    /// <summary>
    /// Checks whether a grid position contains an environmental hazard.
    /// </summary>
    /// <param name="grid">The combat grid to check.</param>
    /// <param name="position">The position to query.</param>
    /// <returns>True if the position contains a hazard; otherwise, false.</returns>
    /// <remarks>
    /// A cell is considered hazardous if its TerrainType is <see cref="TerrainType.Hazardous"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when grid is null.</exception>
    bool IsHazard(CombatGrid grid, GridPosition position);

    /// <summary>
    /// Gets the type of environmental hazard at a position.
    /// </summary>
    /// <param name="grid">The combat grid to check.</param>
    /// <param name="position">The position to query.</param>
    /// <returns>
    /// The <see cref="HazardType"/> at the position, or null if no hazard is present.
    /// </returns>
    /// <remarks>
    /// The hazard type is determined from the cell's TerrainDefinitionId
    /// (e.g., "hazard:lava" maps to HazardType.Lava).
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when grid is null.</exception>
    HazardType? GetHazardType(CombatGrid grid, GridPosition position);

    /// <summary>
    /// Applies hazard damage to a combatant at a hazardous position.
    /// </summary>
    /// <param name="target">The combatant taking hazard damage.</param>
    /// <param name="grid">The combat grid containing the hazard.</param>
    /// <param name="position">The hazard position.</param>
    /// <param name="isEntryDamage">Whether this is entry damage (true) or tick damage (false).</param>
    /// <returns>
    /// A <see cref="HazardDamageResult"/> containing damage dealt, status effects applied,
    /// and the combatant's remaining health.
    /// </returns>
    /// <remarks>
    /// <para>Hazard damage is determined by the hazard definition's damage dice.</para>
    /// <para>Status effects (burning, bleeding, prone) are applied if configured.</para>
    /// <para>Entry damage occurs when first entering a hazard cell.</para>
    /// <para>Tick damage occurs at the start of each turn while in the hazard.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when target or grid is null.</exception>
    HazardDamageResult ApplyHazardDamage(Combatant target, CombatGrid grid, GridPosition position, bool isEntryDamage = true);

    /// <summary>
    /// Processes per-turn hazard tick damage for all combatants in hazardous cells.
    /// </summary>
    /// <param name="grid">The combat grid to process.</param>
    /// <param name="combatants">The collection of combatants to check for hazard positions.</param>
    /// <returns>
    /// A list of <see cref="HazardDamageResult"/> for each combatant that took tick damage.
    /// </returns>
    /// <remarks>
    /// <para>Should be called at the start of each combat round.</para>
    /// <para>Only processes hazards with <see cref="EnvironmentalHazardDefinition.DamagePerTurn"/> = true.</para>
    /// <para>Publishes <see cref="Events.HazardTickProcessedEvent"/> after processing.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when grid or combatants is null.</exception>
    IReadOnlyList<HazardDamageResult> TickHazards(CombatGrid grid, IEnumerable<Combatant> combatants);

    /// <summary>
    /// Processes knockback triggered by a critical hit.
    /// </summary>
    /// <param name="attacker">The combatant who scored the critical hit.</param>
    /// <param name="target">The combatant who was critically hit.</param>
    /// <param name="grid">The combat grid containing both combatants.</param>
    /// <param name="isCritical">Whether the attack was a critical hit.</param>
    /// <returns>
    /// A <see cref="PushResult"/> if knockback occurred, or null if not a critical hit
    /// or knockback was not possible.
    /// </returns>
    /// <remarks>
    /// <para>Critical hits trigger a 1-cell knockback away from the attacker.</para>
    /// <para>Knockback is forced movement (no opposed check).</para>
    /// <para>Returns null if isCritical is false or the target cannot be moved.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when attacker, target, or grid is null.</exception>
    PushResult? ProcessCriticalKnockback(Combatant attacker, Combatant target, CombatGrid grid, bool isCritical);

    /// <summary>
    /// Calculates the movement direction from one position toward another.
    /// </summary>
    /// <param name="from">The starting position.</param>
    /// <param name="to">The target position.</param>
    /// <returns>
    /// The <see cref="MovementDirection"/> that moves from the start toward the target.
    /// </returns>
    /// <remarks>
    /// <para>Uses the relative position to determine the primary direction.</para>
    /// <para>Diagonal directions are supported (NorthEast, SouthWest, etc.).</para>
    /// <para>Returns South as default if positions are equal.</para>
    /// </remarks>
    MovementDirection GetDirectionToward(GridPosition from, GridPosition to);

    /// <summary>
    /// Calculates the movement direction away from a source position.
    /// </summary>
    /// <param name="current">The current position.</param>
    /// <param name="source">The source position to move away from.</param>
    /// <returns>
    /// The <see cref="MovementDirection"/> that moves the current position away from the source.
    /// </returns>
    /// <remarks>
    /// <para>This is the opposite of <see cref="GetDirectionToward"/>.</para>
    /// <para>Used for knockback calculations where the target moves away from the damage source.</para>
    /// <para>Returns North as default if positions are equal.</para>
    /// </remarks>
    MovementDirection GetDirectionAway(GridPosition current, GridPosition source);

    /// <summary>
    /// Gets the STR modifier for a combatant used in opposed push checks.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>The STR modifier value for opposed checks.</returns>
    /// <remarks>
    /// <para>For players, this is typically derived from their Strength attribute.</para>
    /// <para>For monsters, this may be a fixed value or derived from their stats.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when combatant is null.</exception>
    int GetStrengthModifier(Combatant combatant);
}
