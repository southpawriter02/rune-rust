using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Result of a push or knockback operation in environmental combat.
/// </summary>
/// <remarks>
/// <para>PushResult encapsulates all information about push/knockback operations,
/// including whether the target was moved, blocked, resisted, or pushed into a hazard.</para>
/// <para>Push mechanics use an opposed STR check where the target wins ties.
/// Knockback (forced movement) does not use an opposed check.</para>
/// <para>Possible outcomes:</para>
/// <list type="bullet">
///   <item><description>Success: Target was pushed the full distance</description></item>
///   <item><description>PartialSuccess: Target was pushed but stopped early (wall/entity)</description></item>
///   <item><description>Resisted: Target won the opposed STR check</description></item>
///   <item><description>Blocked: Target hit a wall or entity immediately</description></item>
///   <item><description>PushedIntoHazard: Target was pushed into an environmental hazard</description></item>
/// </list>
/// </remarks>
/// <seealso cref="HazardDamageResult"/>
/// <seealso cref="RuneAndRust.Application.Interfaces.IEnvironmentalCombatService"/>
public record PushResult
{
    /// <summary>
    /// Gets whether the target was pushed at all.
    /// </summary>
    /// <remarks>
    /// True if the target moved at least one cell. False if resisted or blocked immediately.
    /// </remarks>
    public bool WasPushed { get; init; }

    /// <summary>
    /// Gets whether the target successfully resisted the push.
    /// </summary>
    /// <remarks>
    /// True when the target won the opposed STR check (target wins ties).
    /// Only applicable to push operations, not knockback.
    /// </remarks>
    public bool WasResisted { get; init; }

    /// <summary>
    /// Gets whether the push was blocked by a wall or entity.
    /// </summary>
    /// <remarks>
    /// True when the target could not be pushed because the destination was blocked.
    /// The target remains at their current position when blocked.
    /// </remarks>
    public bool WasBlocked { get; init; }

    /// <summary>
    /// Gets whether the target was pushed into a hazardous cell.
    /// </summary>
    /// <remarks>
    /// When true, check <see cref="HazardDamage"/> for the damage result.
    /// </remarks>
    public bool HitHazard { get; init; }

    /// <summary>
    /// Gets the target's starting position before the push.
    /// </summary>
    public GridPosition StartPosition { get; init; }

    /// <summary>
    /// Gets the target's ending position after the push.
    /// </summary>
    /// <remarks>
    /// Equal to StartPosition if the push was resisted or blocked.
    /// </remarks>
    public GridPosition EndPosition { get; init; }

    /// <summary>
    /// Gets the number of cells the target was moved.
    /// </summary>
    /// <remarks>
    /// Zero if resisted or blocked. May be less than requested distance if blocked mid-push.
    /// </remarks>
    public int CellsMoved { get; init; }

    /// <summary>
    /// Gets the intended push distance (number of cells).
    /// </summary>
    /// <remarks>
    /// The maximum distance the target could have been pushed.
    /// </remarks>
    public int IntendedDistance { get; init; }

    /// <summary>
    /// Gets the direction the target was pushed.
    /// </summary>
    public MovementDirection Direction { get; init; }

    /// <summary>
    /// Gets the pusher's STR check roll (for opposed check pushes).
    /// </summary>
    /// <remarks>
    /// Null for knockback operations which don't use an opposed check.
    /// Format: 1d20 + STR modifier.
    /// </remarks>
    public int? PusherRoll { get; init; }

    /// <summary>
    /// Gets the target's STR check roll (for opposed check pushes).
    /// </summary>
    /// <remarks>
    /// Null for knockback operations which don't use an opposed check.
    /// Format: 1d20 + STR modifier.
    /// </remarks>
    public int? TargetRoll { get; init; }

    /// <summary>
    /// Gets the hazard damage result if the target was pushed into a hazard.
    /// </summary>
    /// <remarks>
    /// Null if no hazard was hit. Contains damage details when <see cref="HitHazard"/> is true.
    /// </remarks>
    public HazardDamageResult? HazardDamage { get; init; }

    /// <summary>
    /// Gets a description of why the push failed or was modified.
    /// </summary>
    /// <remarks>
    /// Examples: "Resisted the push", "Blocked by wall", "Pushed into lava".
    /// </remarks>
    public string? ResultDescription { get; init; }

    /// <summary>
    /// Gets whether this was a knockback (forced movement) rather than a push (opposed check).
    /// </summary>
    public bool WasKnockback { get; init; }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a successful push result where the target was moved the full distance.
    /// </summary>
    /// <param name="startPosition">The starting position.</param>
    /// <param name="endPosition">The ending position.</param>
    /// <param name="direction">The push direction.</param>
    /// <param name="distance">The number of cells moved.</param>
    /// <param name="pusherRoll">The pusher's STR roll.</param>
    /// <param name="targetRoll">The target's STR roll.</param>
    /// <returns>A successful PushResult.</returns>
    public static PushResult Success(
        GridPosition startPosition,
        GridPosition endPosition,
        MovementDirection direction,
        int distance,
        int? pusherRoll = null,
        int? targetRoll = null)
    {
        return new PushResult
        {
            WasPushed = true,
            WasResisted = false,
            WasBlocked = false,
            HitHazard = false,
            StartPosition = startPosition,
            EndPosition = endPosition,
            Direction = direction,
            CellsMoved = distance,
            IntendedDistance = distance,
            PusherRoll = pusherRoll,
            TargetRoll = targetRoll,
            ResultDescription = $"Pushed {distance} cell(s) {direction}"
        };
    }

    /// <summary>
    /// Creates a push result where the target resisted via opposed STR check.
    /// </summary>
    /// <param name="position">The target's current position (unchanged).</param>
    /// <param name="direction">The attempted push direction.</param>
    /// <param name="intendedDistance">The intended push distance.</param>
    /// <param name="pusherRoll">The pusher's STR roll.</param>
    /// <param name="targetRoll">The target's STR roll.</param>
    /// <returns>A PushResult indicating resistance.</returns>
    public static PushResult Resisted(
        GridPosition position,
        MovementDirection direction,
        int intendedDistance,
        int pusherRoll,
        int targetRoll)
    {
        return new PushResult
        {
            WasPushed = false,
            WasResisted = true,
            WasBlocked = false,
            HitHazard = false,
            StartPosition = position,
            EndPosition = position,
            Direction = direction,
            CellsMoved = 0,
            IntendedDistance = intendedDistance,
            PusherRoll = pusherRoll,
            TargetRoll = targetRoll,
            ResultDescription = $"Resisted the push (Target: {targetRoll} vs Pusher: {pusherRoll})"
        };
    }

    /// <summary>
    /// Creates a push result where the push was blocked by a wall or entity.
    /// </summary>
    /// <param name="startPosition">The starting position.</param>
    /// <param name="endPosition">The position where movement stopped.</param>
    /// <param name="direction">The push direction.</param>
    /// <param name="cellsMoved">The number of cells moved before blocking.</param>
    /// <param name="intendedDistance">The intended push distance.</param>
    /// <param name="blockedBy">Description of what blocked the push.</param>
    /// <param name="pusherRoll">The pusher's STR roll (if applicable).</param>
    /// <param name="targetRoll">The target's STR roll (if applicable).</param>
    /// <returns>A PushResult indicating blocking.</returns>
    public static PushResult Blocked(
        GridPosition startPosition,
        GridPosition endPosition,
        MovementDirection direction,
        int cellsMoved,
        int intendedDistance,
        string blockedBy,
        int? pusherRoll = null,
        int? targetRoll = null)
    {
        return new PushResult
        {
            WasPushed = cellsMoved > 0,
            WasResisted = false,
            WasBlocked = true,
            HitHazard = false,
            StartPosition = startPosition,
            EndPosition = endPosition,
            Direction = direction,
            CellsMoved = cellsMoved,
            IntendedDistance = intendedDistance,
            PusherRoll = pusherRoll,
            TargetRoll = targetRoll,
            ResultDescription = $"Blocked by {blockedBy} after {cellsMoved} cell(s)"
        };
    }

    /// <summary>
    /// Creates a push result where the target was pushed into a hazard.
    /// </summary>
    /// <param name="startPosition">The starting position.</param>
    /// <param name="hazardPosition">The hazard position (end position).</param>
    /// <param name="direction">The push direction.</param>
    /// <param name="cellsMoved">The number of cells moved into the hazard.</param>
    /// <param name="intendedDistance">The intended push distance.</param>
    /// <param name="hazardDamage">The hazard damage result.</param>
    /// <param name="pusherRoll">The pusher's STR roll (if applicable).</param>
    /// <param name="targetRoll">The target's STR roll (if applicable).</param>
    /// <returns>A PushResult indicating hazard impact.</returns>
    public static PushResult PushedIntoHazard(
        GridPosition startPosition,
        GridPosition hazardPosition,
        MovementDirection direction,
        int cellsMoved,
        int intendedDistance,
        HazardDamageResult hazardDamage,
        int? pusherRoll = null,
        int? targetRoll = null)
    {
        return new PushResult
        {
            WasPushed = true,
            WasResisted = false,
            WasBlocked = false,
            HitHazard = true,
            StartPosition = startPosition,
            EndPosition = hazardPosition,
            Direction = direction,
            CellsMoved = cellsMoved,
            IntendedDistance = intendedDistance,
            PusherRoll = pusherRoll,
            TargetRoll = targetRoll,
            HazardDamage = hazardDamage,
            ResultDescription = $"Pushed into {hazardDamage.HazardName} ({hazardDamage.DamageDealt} {hazardDamage.DamageType} damage)"
        };
    }

    /// <summary>
    /// Creates a knockback result (forced movement with no opposed check).
    /// </summary>
    /// <param name="startPosition">The starting position.</param>
    /// <param name="endPosition">The ending position.</param>
    /// <param name="direction">The knockback direction.</param>
    /// <param name="cellsMoved">The number of cells moved.</param>
    /// <param name="hazardDamage">Optional hazard damage if knocked into hazard.</param>
    /// <returns>A PushResult for knockback.</returns>
    public static PushResult Knockback(
        GridPosition startPosition,
        GridPosition endPosition,
        MovementDirection direction,
        int cellsMoved,
        HazardDamageResult? hazardDamage = null)
    {
        return new PushResult
        {
            WasPushed = cellsMoved > 0,
            WasResisted = false,
            WasBlocked = cellsMoved == 0,
            HitHazard = hazardDamage != null,
            StartPosition = startPosition,
            EndPosition = endPosition,
            Direction = direction,
            CellsMoved = cellsMoved,
            IntendedDistance = cellsMoved,
            HazardDamage = hazardDamage,
            WasKnockback = true,
            ResultDescription = hazardDamage != null
                ? $"Knocked back into {hazardDamage.HazardName}"
                : $"Knocked back {cellsMoved} cell(s) {direction}"
        };
    }

    /// <summary>
    /// Creates a result indicating no push occurred (invalid operation).
    /// </summary>
    /// <param name="position">The target's current position.</param>
    /// <param name="reason">The reason the push could not occur.</param>
    /// <returns>A PushResult indicating failure.</returns>
    public static PushResult NoPush(GridPosition position, string reason)
    {
        return new PushResult
        {
            WasPushed = false,
            WasResisted = false,
            WasBlocked = false,
            HitHazard = false,
            StartPosition = position,
            EndPosition = position,
            CellsMoved = 0,
            IntendedDistance = 0,
            ResultDescription = reason
        };
    }
}
