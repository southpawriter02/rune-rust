using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for flanking calculations in tactical combat.
/// </summary>
/// <remarks>
/// <para>
/// Provides flanking mechanics for melee combat, including:
/// </para>
/// <list type="bullet">
///   <item><description>Detection of flanking bonuses when allies attack from opposite sides</description></item>
///   <item><description>Attack and damage bonuses for attacking from behind</description></item>
///   <item><description>Side attack detection (informational only)</description></item>
///   <item><description>Configuration-driven bonus values</description></item>
/// </list>
/// </remarks>
public class FlankingService : IFlankingService
{
    private readonly ICombatGridService _gridService;
    private readonly IGameConfigurationProvider _config;
    private readonly ILogger<FlankingService> _logger;

    // Flanking configuration (loaded from combat.json)
    private bool _flankingEnabled = true;
    private int _flankingAttackBonus = 2;
    private int _flankingDamageBonus = 0;
    private int _behindAttackBonus = 2;
    private int _behindDamageBonus = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlankingService"/> class.
    /// </summary>
    /// <param name="gridService">The combat grid service.</param>
    /// <param name="config">The configuration provider.</param>
    /// <param name="logger">The logger.</param>
    public FlankingService(
        ICombatGridService gridService,
        IGameConfigurationProvider config,
        ILogger<FlankingService> logger)
    {
        _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        LoadConfiguration();
    }

    /// <summary>
    /// Loads flanking configuration from combat.json.
    /// </summary>
    private void LoadConfiguration()
    {
        var flankingConfig = _config.GetFlankingConfiguration();
        if (flankingConfig != null)
        {
            _flankingEnabled = flankingConfig.Enabled;
            _flankingAttackBonus = flankingConfig.AttackBonus;
            _flankingDamageBonus = flankingConfig.DamageBonus;
            _behindAttackBonus = flankingConfig.BehindAttackBonus;
            _behindDamageBonus = flankingConfig.BehindDamageBonus;

            _logger.LogInformation(
                "Flanking config loaded: Enabled={Enabled}, AttackBonus={AttackBonus}, " +
                "DamageBonus={DamageBonus}, BehindAttack={BehindAttack}, BehindDamage={BehindDamage}",
                _flankingEnabled, _flankingAttackBonus, _flankingDamageBonus,
                _behindAttackBonus, _behindDamageBonus);
        }
        else
        {
            _logger.LogDebug("Using default flanking configuration");
        }
    }

    /// <inheritdoc/>
    public FlankingResult CheckFlanking(Guid attackerId, Guid targetId)
    {
        if (!_flankingEnabled)
            return FlankingResult.None;

        var grid = _gridService.GetActiveGrid();
        if (grid == null)
            return FlankingResult.NoGrid;

        var attackerPos = grid.GetEntityPosition(attackerId);
        var targetPos = grid.GetEntityPosition(targetId);

        if (!attackerPos.HasValue || !targetPos.HasValue)
            return FlankingResult.NotOnGrid;

        // Get target facing
        var targetFacing = grid.GetEntityFacing(targetId) ?? FacingDirection.North;

        // Get ally positions (other entities on the grid that are not the attacker or target)
        var allyPositions = grid.EntityPositions
            .Where(kvp => kvp.Key != attackerId && kvp.Key != targetId)
            .Select(kvp => kvp.Value)
            .ToList();

        return CheckFlanking(attackerPos.Value, targetPos.Value, targetFacing, allyPositions);
    }

    /// <inheritdoc/>
    public FlankingResult CheckFlanking(
        GridPosition attackerPosition,
        GridPosition targetPosition,
        FacingDirection targetFacing,
        IEnumerable<GridPosition> allyPositions)
    {
        if (!_flankingEnabled)
            return FlankingResult.None;

        // Must be adjacent to flank
        if (attackerPosition.DistanceTo(targetPosition) > 1)
        {
            _logger.LogDebug(
                "No flanking: attacker at {Attacker} not adjacent to target at {Target}",
                attackerPosition, targetPosition);
            return FlankingResult.NotAdjacent;
        }

        // Check if attacking from behind
        var attackingFromBehind = FacingDirectionExtensions.IsBehind(
            attackerPosition, targetPosition, targetFacing);

        if (attackingFromBehind)
        {
            _logger.LogDebug(
                "Backstab detected: attacker at {Attacker} is behind target at {Target} (facing {Facing})",
                attackerPosition, targetPosition, targetFacing);

            return new FlankingResult(
                true, FlankingType.Behind,
                _behindAttackBonus, _behindDamageBonus, null,
                $"Backstab! (+{_behindAttackBonus} attack, +{_behindDamageBonus} damage)");
        }

        // Check if attacking from side
        var attackingFromSide = FacingDirectionExtensions.IsSide(
            attackerPosition, targetPosition, targetFacing);

        // Check for flanking by ally
        var flankingAllyPos = FindFlankingAlly(attackerPosition, targetPosition, allyPositions);

        if (flankingAllyPos.HasValue)
        {
            _logger.LogDebug(
                "Flanking detected: attacker at {Attacker}, ally at {Ally}, target at {Target}",
                attackerPosition, flankingAllyPos.Value, targetPosition);

            return new FlankingResult(
                true, FlankingType.Flanked,
                _flankingAttackBonus, _flankingDamageBonus, null,
                $"Flanking! (+{_flankingAttackBonus} attack)");
        }

        // Side attack (no bonus, but noted)
        if (attackingFromSide)
        {
            _logger.LogDebug(
                "Side attack: attacker at {Attacker} is at side of target at {Target}",
                attackerPosition, targetPosition);

            return new FlankingResult(
                false, FlankingType.Side,
                0, 0, null, "Attacking from the side.");
        }

        // Front attack
        _logger.LogDebug(
            "Frontal attack: attacker at {Attacker}, target at {Target} facing {Facing}",
            attackerPosition, targetPosition, targetFacing);

        return FlankingResult.None;
    }

    /// <summary>
    /// Finds an ally that provides flanking (on opposite side of target).
    /// </summary>
    private GridPosition? FindFlankingAlly(
        GridPosition attacker, GridPosition target, IEnumerable<GridPosition> allyPositions)
    {
        var attackerDirection = FacingDirectionExtensions.GetDirectionTo(target, attacker);
        var oppositeDirection = attackerDirection.GetOpposite();

        foreach (var allyPos in allyPositions)
        {
            // Ally must be adjacent to target
            if (allyPos.DistanceTo(target) > 1)
                continue;

            // Ally must be on opposite side
            var allyDirection = FacingDirectionExtensions.GetDirectionTo(target, allyPos);
            if (allyDirection == oppositeDirection || allyDirection.IsAdjacent(oppositeDirection))
            {
                return allyPos;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public int GetFlankingAttackBonus(Guid attackerId, Guid targetId)
    {
        var result = CheckFlanking(attackerId, targetId);
        return result.AttackBonus;
    }

    /// <inheritdoc/>
    public int GetFlankingDamageBonus(Guid attackerId, Guid targetId)
    {
        var result = CheckFlanking(attackerId, targetId);
        return result.DamageBonus;
    }

    /// <inheritdoc/>
    public IEnumerable<GridPosition> GetFlankingPositions(Guid targetId)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
            yield break;

        var targetPos = grid.GetEntityPosition(targetId);
        if (!targetPos.HasValue)
            yield break;

        var targetFacing = grid.GetEntityFacing(targetId) ?? FacingDirection.North;
        var behindDirection = targetFacing.GetOpposite();

        // Return adjacent cells that are behind or on flanking sides
        foreach (var cell in grid.GetAdjacentCells(targetPos.Value))
        {
            var cellDirection = FacingDirectionExtensions.GetDirectionTo(targetPos.Value, cell.Position);
            if (cellDirection == behindDirection || cellDirection.IsAdjacent(behindDirection))
            {
                yield return cell.Position;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<GridPosition> GetAllyFlankingPositions(Guid attackerId, Guid targetId)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
            yield break;

        var attackerPos = grid.GetEntityPosition(attackerId);
        var targetPos = grid.GetEntityPosition(targetId);

        if (!attackerPos.HasValue || !targetPos.HasValue)
            yield break;

        var attackerDirection = FacingDirectionExtensions.GetDirectionTo(targetPos.Value, attackerPos.Value);
        var oppositeDirection = attackerDirection.GetOpposite();

        // Find allies that are on the opposite side
        foreach (var (entityId, entityPos) in grid.EntityPositions)
        {
            if (entityId == attackerId || entityId == targetId)
                continue;

            if (entityPos.DistanceTo(targetPos.Value) > 1)
                continue;

            var entityDirection = FacingDirectionExtensions.GetDirectionTo(targetPos.Value, entityPos);
            if (entityDirection == oppositeDirection || entityDirection.IsAdjacent(oppositeDirection))
            {
                yield return entityPos;
            }
        }
    }

    /// <inheritdoc/>
    public bool IsAttackingFromBehind(Guid attackerId, Guid targetId)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
            return false;

        var attackerPos = grid.GetEntityPosition(attackerId);
        var targetPos = grid.GetEntityPosition(targetId);

        if (!attackerPos.HasValue || !targetPos.HasValue)
            return false;

        var targetFacing = grid.GetEntityFacing(targetId) ?? FacingDirection.North;

        return FacingDirectionExtensions.IsBehind(attackerPos.Value, targetPos.Value, targetFacing);
    }

    /// <inheritdoc/>
    public bool IsAttackingFromSide(Guid attackerId, Guid targetId)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
            return false;

        var attackerPos = grid.GetEntityPosition(attackerId);
        var targetPos = grid.GetEntityPosition(targetId);

        if (!attackerPos.HasValue || !targetPos.HasValue)
            return false;

        var targetFacing = grid.GetEntityFacing(targetId) ?? FacingDirection.North;

        return FacingDirectionExtensions.IsSide(attackerPos.Value, targetPos.Value, targetFacing);
    }
}
