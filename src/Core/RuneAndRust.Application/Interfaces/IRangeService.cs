using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for range calculations and validation in tactical combat.
/// </summary>
public interface IRangeService
{
    /// <summary>
    /// Checks if an attack is within valid range.
    /// </summary>
    /// <param name="attackerId">The attacking entity's ID.</param>
    /// <param name="targetId">The target entity's ID.</param>
    /// <param name="weaponRange">The weapon's configured range.</param>
    /// <param name="rangeType">The range type for validation.</param>
    /// <returns>Result indicating if target is in range.</returns>
    RangeCheckResult CheckRange(Guid attackerId, Guid targetId, int weaponRange, RangeType rangeType);

    /// <summary>
    /// Checks if an ability target is within valid range.
    /// </summary>
    /// <param name="casterId">The casting entity's ID.</param>
    /// <param name="targetId">The target entity's ID.</param>
    /// <param name="ability">The ability being used.</param>
    /// <returns>Result indicating if target is in range.</returns>
    RangeCheckResult CheckAbilityRange(Guid casterId, Guid targetId, AbilityDefinition ability);

    /// <summary>
    /// Gets all valid targets within weapon range.
    /// </summary>
    /// <param name="attackerId">The attacking entity's ID.</param>
    /// <param name="range">The maximum range.</param>
    /// <param name="rangeType">The range type for filtering.</param>
    /// <returns>Entity IDs of valid targets.</returns>
    IEnumerable<Guid> GetValidTargets(Guid attackerId, int range, RangeType rangeType);

    /// <summary>
    /// Gets the distance between two entities.
    /// </summary>
    /// <param name="entityId1">First entity ID.</param>
    /// <param name="entityId2">Second entity ID.</param>
    /// <returns>The Chebyshev distance, or null if positions unavailable.</returns>
    int? GetDistance(Guid entityId1, Guid entityId2);

    /// <summary>
    /// Checks if two entities are adjacent (distance 1).
    /// </summary>
    /// <param name="entityId1">First entity ID.</param>
    /// <param name="entityId2">Second entity ID.</param>
    /// <returns>True if adjacent.</returns>
    bool AreAdjacent(Guid entityId1, Guid entityId2);

    /// <summary>
    /// Gets the effective range for a weapon.
    /// </summary>
    /// <param name="weapon">The weapon item.</param>
    /// <returns>The effective attack range.</returns>
    int GetEffectiveRange(Item weapon);

    /// <summary>
    /// Gets the effective range for an ability.
    /// </summary>
    /// <param name="ability">The ability definition.</param>
    /// <returns>The effective ability range.</returns>
    int GetEffectiveRange(AbilityDefinition ability);
}

/// <summary>
/// Result of a range check operation.
/// </summary>
/// <param name="InRange">Whether the target is in range.</param>
/// <param name="Distance">The distance to the target.</param>
/// <param name="WeaponRange">The weapon's effective range.</param>
/// <param name="RangeType">The range type used for validation.</param>
/// <param name="Message">Human-readable result message.</param>
/// <param name="FailureReason">The reason for failure, if any.</param>
public readonly record struct RangeCheckResult(
    bool InRange,
    int Distance,
    int WeaponRange,
    RangeType RangeType,
    string Message,
    RangeFailureReason? FailureReason)
{
    /// <summary>Creates a success result.</summary>
    public static RangeCheckResult Success(int distance, int weaponRange, RangeType rangeType) =>
        new(true, distance, weaponRange, rangeType, "Target is in range.", null);

    /// <summary>Creates a failure result.</summary>
    public static RangeCheckResult Fail(RangeFailureReason reason, string message) =>
        new(false, 0, 0, RangeType.Melee, message, reason);

    /// <summary>Creates an out-of-range failure result.</summary>
    public static RangeCheckResult OutOfRange(int distance, int weaponRange, RangeType rangeType, string message) =>
        new(false, distance, weaponRange, rangeType, message, 
            rangeType == RangeType.Melee ? RangeFailureReason.NotAdjacent : RangeFailureReason.OutOfRange);
}

/// <summary>
/// Reasons why a range check failed.
/// </summary>
public enum RangeFailureReason
{
    /// <summary>No active combat grid.</summary>
    NoActiveGrid,

    /// <summary>Attacker is not positioned on the grid.</summary>
    AttackerNotOnGrid,

    /// <summary>Target is not positioned on the grid.</summary>
    TargetNotOnGrid,

    /// <summary>Target is beyond maximum range.</summary>
    OutOfRange,

    /// <summary>Target is too close (for minimum range, v0.5.1b).</summary>
    TooClose,

    /// <summary>Target is not adjacent (for melee attacks).</summary>
    NotAdjacent,

    /// <summary>No line of sight to target (v0.5.1c).</summary>
    NoLineOfSight
}
