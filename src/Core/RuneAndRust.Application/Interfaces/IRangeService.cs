using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

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

    // ===== Extended Range Methods (v0.5.1b) =====

    /// <summary>
    /// Checks full range including min range and calculates penalty.
    /// </summary>
    /// <param name="attackerId">The attacking entity's ID.</param>
    /// <param name="targetId">The target entity's ID.</param>
    /// <param name="weapon">The weapon being used.</param>
    /// <returns>Full range check result with penalty.</returns>
    RangeCheckResult CheckFullRange(Guid attackerId, Guid targetId, Item weapon);

    /// <summary>
    /// Gets the range penalty for a given distance and weapon.
    /// </summary>
    /// <param name="distance">The distance to target.</param>
    /// <param name="weapon">The weapon being used.</param>
    /// <returns>Accuracy penalty (0 if in optimal range).</returns>
    int GetRangePenalty(int distance, Item weapon);

    /// <summary>
    /// Gets the range penalty for an ability at a given distance.
    /// </summary>
    /// <param name="distance">The distance to target.</param>
    /// <param name="ability">The ability being used.</param>
    /// <returns>Accuracy penalty (0 if no penalty configured).</returns>
    int GetAbilityRangePenalty(int distance, AbilityDefinition ability);

    /// <summary>
    /// Checks if target is too close (within min range).
    /// </summary>
    /// <param name="attackerId">The attacking entity's ID.</param>
    /// <param name="targetId">The target entity's ID.</param>
    /// <param name="weapon">The weapon being used.</param>
    /// <returns>True if target is within minimum range.</returns>
    bool IsTooClose(Guid attackerId, Guid targetId, Item weapon);
}

/// <summary>
/// Result of a range check operation.
/// </summary>
public readonly record struct RangeCheckResult
{
    /// <summary>Whether the target is in range.</summary>
    public bool InRange { get; init; }

    /// <summary>The distance to the target.</summary>
    public int Distance { get; init; }

    /// <summary>The weapon's configured range.</summary>
    public int WeaponRange { get; init; }

    /// <summary>The range type used for validation.</summary>
    public RangeType RangeType { get; init; }

    /// <summary>Human-readable result message.</summary>
    public string Message { get; init; }

    /// <summary>The reason for failure, if any.</summary>
    public RangeFailureReason? FailureReason { get; init; }

    // ===== Extended Properties (v0.5.1b) =====

    /// <summary>Accuracy penalty for long-range shots.</summary>
    public int Penalty { get; init; }

    /// <summary>True if within optimal range (no penalty).</summary>
    public bool IsOptimal { get; init; }

    /// <summary>True if target is within minimum range.</summary>
    public bool TooClose { get; init; }

    // ===== Line of Sight Properties (v0.5.1c) =====

    /// <summary>Position that blocks line of sight, if any.</summary>
    public GridPosition? BlockedBy { get; init; }

    /// <summary>Creates a success result.</summary>
    public static RangeCheckResult Success(int distance, int weaponRange, RangeType rangeType) =>
        new() { InRange = true, Distance = distance, WeaponRange = weaponRange, RangeType = rangeType, 
                Message = "Target is in range.", IsOptimal = true };

    /// <summary>Creates a failure result.</summary>
    public static RangeCheckResult Fail(RangeFailureReason reason, string message) =>
        new() { InRange = false, Message = message, FailureReason = reason };

    /// <summary>Creates an out-of-range failure result.</summary>
    public static RangeCheckResult OutOfRange(int distance, int weaponRange, RangeType rangeType, string message) =>
        new() { InRange = false, Distance = distance, WeaponRange = weaponRange, RangeType = rangeType, 
                Message = message, FailureReason = rangeType == RangeType.Melee 
                    ? RangeFailureReason.NotAdjacent : RangeFailureReason.OutOfRange };
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
