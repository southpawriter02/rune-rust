using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for range calculations and validation in tactical combat.
/// </summary>
/// <remarks>
/// Uses the combat grid (v0.5.0) for position-based range calculations.
/// Range types:
/// - Melee: Must be adjacent (distance 1)
/// - Reach: Distance 1-2
/// - Ranged: Configurable max distance
/// </remarks>
public class RangeService : IRangeService
{
    private readonly ICombatGridService _gridService;
    private readonly ILogger<RangeService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeService"/> class.
    /// </summary>
    /// <param name="gridService">The combat grid service.</param>
    /// <param name="logger">The logger.</param>
    public RangeService(ICombatGridService gridService, ILogger<RangeService> logger)
    {
        _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public RangeCheckResult CheckRange(
        Guid attackerId, Guid targetId, int weaponRange, RangeType rangeType)
    {
        _logger.LogDebug("Checking range: attacker {Attacker}, target {Target}, range {Range}, type {Type}",
            attackerId, targetId, weaponRange, rangeType);

        // Validate grid exists
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
        {
            _logger.LogWarning("Range check failed: no active grid");
            return RangeCheckResult.Fail(RangeFailureReason.NoActiveGrid, "No active combat grid.");
        }

        // Get attacker position
        var attackerPos = grid.GetEntityPosition(attackerId);
        if (!attackerPos.HasValue)
        {
            _logger.LogWarning("Range check failed: attacker {Id} not on grid", attackerId);
            return RangeCheckResult.Fail(RangeFailureReason.AttackerNotOnGrid, "Attacker is not on the grid.");
        }

        // Get target position
        var targetPos = grid.GetEntityPosition(targetId);
        if (!targetPos.HasValue)
        {
            _logger.LogWarning("Range check failed: target {Id} not on grid", targetId);
            return RangeCheckResult.Fail(RangeFailureReason.TargetNotOnGrid, "Target is not on the grid.");
        }

        // Calculate distance (Chebyshev)
        var distance = attackerPos.Value.DistanceTo(targetPos.Value);

        // Check range by type
        var inRange = rangeType switch
        {
            RangeType.Melee => distance == 1,
            RangeType.Reach => distance >= 1 && distance <= 2,
            RangeType.Ranged => distance >= 1 && distance <= weaponRange,
            _ => distance <= weaponRange
        };

        if (!inRange)
        {
            var message = rangeType == RangeType.Melee
                ? $"Target is not adjacent (distance: {distance})."
                : $"Target is out of range (distance: {distance}, range: {weaponRange}).";

            _logger.LogDebug("Range check failed: {Message}", message);
            return RangeCheckResult.OutOfRange(distance, weaponRange, rangeType, message);
        }

        _logger.LogDebug("Range check passed: distance {Dist}, range {Range}, type {Type}",
            distance, weaponRange, rangeType);

        return RangeCheckResult.Success(distance, weaponRange, rangeType);
    }

    /// <inheritdoc/>
    public RangeCheckResult CheckAbilityRange(Guid casterId, Guid targetId, AbilityDefinition ability)
    {
        ArgumentNullException.ThrowIfNull(ability);
        return CheckRange(casterId, targetId, ability.Range, ability.RangeType);
    }

    /// <inheritdoc/>
    public IEnumerable<Guid> GetValidTargets(Guid attackerId, int range, RangeType rangeType)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
        {
            _logger.LogDebug("GetValidTargets: no active grid");
            yield break;
        }

        var attackerPos = grid.GetEntityPosition(attackerId);
        if (!attackerPos.HasValue)
        {
            _logger.LogDebug("GetValidTargets: attacker {Id} not on grid", attackerId);
            yield break;
        }

        // Determine effective range
        var effectiveRange = rangeType switch
        {
            RangeType.Melee => 1,
            RangeType.Reach => 2,
            RangeType.Ranged => range,
            _ => range
        };

        _logger.LogDebug("GetValidTargets: checking range {Range} from {Pos}", effectiveRange, attackerPos.Value);

        // Get all entities in range
        foreach (var entityId in grid.GetEntitiesInRange(attackerPos.Value, effectiveRange))
        {
            // Skip self
            if (entityId == attackerId)
                continue;

            // For melee, ensure adjacency
            if (rangeType == RangeType.Melee)
            {
                if (grid.AreAdjacent(attackerId, entityId))
                    yield return entityId;
            }
            else
            {
                yield return entityId;
            }
        }
    }

    /// <inheritdoc/>
    public int? GetDistance(Guid entityId1, Guid entityId2) =>
        _gridService.GetDistance(entityId1, entityId2);

    /// <inheritdoc/>
    public bool AreAdjacent(Guid entityId1, Guid entityId2) =>
        _gridService.AreAdjacent(entityId1, entityId2);

    /// <inheritdoc/>
    public int GetEffectiveRange(Item weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        return weapon.GetEffectiveRange();
    }

    /// <inheritdoc/>
    public int GetEffectiveRange(AbilityDefinition ability)
    {
        ArgumentNullException.ThrowIfNull(ability);
        return ability.GetEffectiveRange();
    }

    // ===== Extended Range Methods (v0.5.1b) =====

    /// <inheritdoc/>
    public RangeCheckResult CheckFullRange(Guid attackerId, Guid targetId, Item weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);

        _logger.LogDebug("Full range check: attacker {Attacker}, target {Target}, weapon {Weapon}",
            attackerId, targetId, weapon.Name);

        // Get distance
        var distance = GetDistance(attackerId, targetId);
        if (!distance.HasValue)
        {
            return RangeCheckResult.Fail(RangeFailureReason.AttackerNotOnGrid, "Entity not on grid.");
        }

        var dist = distance.Value;

        // Check minimum range (too close)
        if (weapon.HasMinRange && dist < weapon.MinRange)
        {
            _logger.LogDebug("Full range check failed: too close (dist {Dist}, minRange {Min})",
                dist, weapon.MinRange);

            return new RangeCheckResult
            {
                InRange = false,
                Distance = dist,
                WeaponRange = weapon.Range,
                RangeType = weapon.RangeType,
                FailureReason = RangeFailureReason.TooClose,
                TooClose = true,
                Message = $"Target is too close! Minimum range: {weapon.MinRange} cells."
            };
        }

        // Check maximum range
        if (dist > weapon.Range)
        {
            _logger.LogDebug("Full range check failed: out of range (dist {Dist}, range {Range})",
                dist, weapon.Range);

            return new RangeCheckResult
            {
                InRange = false,
                Distance = dist,
                WeaponRange = weapon.Range,
                RangeType = weapon.RangeType,
                FailureReason = RangeFailureReason.OutOfRange,
                Message = $"Target is out of range (distance: {dist}, range: {weapon.Range})."
            };
        }

        // Calculate penalty
        var penalty = weapon.GetPenaltyAtDistance(dist);
        var isOptimal = weapon.IsInOptimalRange(dist);

        _logger.LogDebug(
            "Full range check passed: dist {Dist}, range {Range}, minRange {Min}, optimal {Opt}, penalty {Pen}",
            dist, weapon.Range, weapon.MinRange, weapon.GetOptimalRange(), penalty);

        return new RangeCheckResult
        {
            InRange = true,
            Distance = dist,
            WeaponRange = weapon.Range,
            RangeType = weapon.RangeType,
            Penalty = penalty,
            IsOptimal = isOptimal,
            Message = isOptimal
                ? "Target is in optimal range."
                : $"Target is at long range (penalty: -{penalty})."
        };
    }

    /// <inheritdoc/>
    public int GetRangePenalty(int distance, Item weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        return weapon.GetPenaltyAtDistance(distance);
    }

    /// <inheritdoc/>
    public int GetAbilityRangePenalty(int distance, AbilityDefinition ability)
    {
        ArgumentNullException.ThrowIfNull(ability);
        return ability.GetPenaltyAtDistance(distance);
    }

    /// <inheritdoc/>
    public bool IsTooClose(Guid attackerId, Guid targetId, Item weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);

        if (!weapon.HasMinRange) return false;

        var distance = GetDistance(attackerId, targetId);
        return distance.HasValue && distance.Value < weapon.MinRange;
    }
}
