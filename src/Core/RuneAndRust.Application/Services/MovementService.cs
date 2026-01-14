using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for handling entity movement on the combat grid.
/// </summary>
/// <remarks>
/// Provides movement validation, point tracking, and grid position updates.
/// Uses a point multiplier system: Speed × 2 = total points per turn.
/// Cardinal moves cost 2 points, diagonal moves cost 3 points.
/// </remarks>
public class MovementService : IMovementService
{
    private readonly ICombatGridService _gridService;
    private readonly ILogger<MovementService> _logger;

    // Tracked entities (in a real implementation, this would come from a session/game state)
    private Player? _player;
    private readonly Dictionary<Guid, Monster> _monsters = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MovementService"/> class.
    /// </summary>
    /// <param name="gridService">The combat grid service.</param>
    /// <param name="logger">The logger.</param>
    public MovementService(
        ICombatGridService gridService,
        ILogger<MovementService> logger)
    {
        _gridService = gridService;
        _logger = logger;
    }

    /// <summary>
    /// Registers the player with the movement service.
    /// </summary>
    /// <param name="player">The player to track.</param>
    public void RegisterPlayer(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        _player = player;
        _logger.LogDebug("Registered player {PlayerName} for movement tracking", player.Name);
    }

    /// <summary>
    /// Registers a monster with the movement service.
    /// </summary>
    /// <param name="monster">The monster to track.</param>
    public void RegisterMonster(Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);
        _monsters[monster.Id] = monster;
        _logger.LogDebug("Registered monster {MonsterName} for movement tracking", monster.Name);
    }

    /// <summary>
    /// Clears all registered entities.
    /// </summary>
    public void ClearRegistrations()
    {
        _player = null;
        _monsters.Clear();
    }

    /// <inheritdoc/>
    public MovementResult Move(Guid entityId, MovementDirection direction)
    {
        // Validate active grid
        var grid = _gridService.GetActiveGrid();
        if (grid == null)
        {
            _logger.LogWarning("Move failed: No active combat grid");
            return MovementResult.Fail(MovementFailureReason.NoActiveGrid, "No active combat grid.");
        }

        // Get current position
        var currentPos = grid.GetEntityPosition(entityId);
        if (!currentPos.HasValue)
        {
            _logger.LogWarning("Move failed: Entity {EntityId} not on grid", entityId);
            return MovementResult.Fail(MovementFailureReason.EntityNotOnGrid, "Entity not on grid.");
        }

        // Calculate target position
        var newPos = currentPos.Value.Move(direction);

        // Validate bounds
        if (!grid.IsInBounds(newPos))
        {
            _logger.LogDebug("Move failed: Target {Position} out of bounds", newPos);
            return MovementResult.Fail(MovementFailureReason.OutOfBounds, "Cannot move outside the battlefield.");
        }

        // Validate passability
        var targetCell = grid.GetCell(newPos);
        if (targetCell == null || !targetCell.IsPassable)
        {
            _logger.LogDebug("Move failed: Target {Position} impassable", newPos);
            return MovementResult.Fail(MovementFailureReason.CellImpassable, "That path is blocked.");
        }

        // Validate occupancy
        if (targetCell.IsOccupied)
        {
            var occupantName = GetEntityName(targetCell.OccupantId!.Value);
            _logger.LogDebug("Move failed: Target {Position} occupied by {Occupant}", newPos, occupantName);
            return MovementResult.Fail(MovementFailureReason.CellOccupied, $"A {occupantName} blocks your path!");
        }

        // Validate movement points
        var cost = GetMovementCost(direction);
        var remaining = GetRemainingMovement(entityId);

        if (cost > remaining)
        {
            var displayCost = MovementCosts.GetDisplayCost(direction);
            var displayRemaining = remaining / (float)MovementCosts.PointMultiplier;
            _logger.LogDebug("Move failed: Insufficient points ({Remaining}/{Cost})", remaining, cost);
            return MovementResult.Fail(
                MovementFailureReason.InsufficientMovementPoints,
                $"Not enough movement points. Need {displayCost}, have {displayRemaining}.");
        }

        // Execute movement - deduct points
        if (!UseEntityMovementPoints(entityId, cost))
        {
            _logger.LogWarning("Move failed: Could not deduct movement points for {EntityId}", entityId);
            return MovementResult.Fail(MovementFailureReason.InsufficientMovementPoints, "Failed to use movement points.");
        }

        // Execute movement - update grid
        if (!grid.MoveEntity(entityId, newPos))
        {
            // Rollback points on failure
            RefundEntityMovementPoints(entityId, cost);
            _logger.LogWarning("Move failed: Grid rejected move for {EntityId}", entityId);
            return MovementResult.Fail(MovementFailureReason.CellOccupied, "Failed to move on grid.");
        }

        // Update entity's tracked position
        UpdateEntityGridPosition(entityId, newPos);
        var newRemaining = GetRemainingMovement(entityId);

        _logger.LogInformation(
            "Entity {EntityId} moved {Direction} from {OldPos} to {NewPos} (cost: {Cost}, remaining: {Remaining})",
            entityId, direction, currentPos.Value, newPos, cost, newRemaining);

        return MovementResult.Succeed(
            currentPos.Value,
            newPos,
            cost,
            newRemaining,
            $"Moved {direction.ToString().ToLowerInvariant()} to {newPos}.");
    }

    /// <inheritdoc/>
    public int GetRemainingMovement(Guid entityId)
    {
        if (_player?.Id == entityId)
            return _player.MovementPointsRemaining;

        if (_monsters.TryGetValue(entityId, out var monster))
            return monster.MovementPointsRemaining;

        return 0;
    }

    /// <inheritdoc/>
    public void ResetMovement(Guid entityId)
    {
        if (_player?.Id == entityId)
        {
            _player.ResetMovementPoints();
            _logger.LogDebug("Reset movement for player {PlayerName}", _player.Name);
            return;
        }

        if (_monsters.TryGetValue(entityId, out var monster))
        {
            monster.ResetMovementPoints();
            _logger.LogDebug("Reset movement for monster {MonsterName}", monster.Name);
        }
    }

    /// <inheritdoc/>
    public void ResetAllMovement()
    {
        _player?.ResetMovementPoints();

        foreach (var monster in _monsters.Values)
            monster.ResetMovementPoints();

        _logger.LogInformation("Reset movement for all entities ({MonsterCount} monsters)", _monsters.Count);
    }

    /// <inheritdoc/>
    public bool CanMove(Guid entityId, MovementDirection direction)
    {
        var grid = _gridService.GetActiveGrid();
        if (grid == null) return false;

        var currentPos = grid.GetEntityPosition(entityId);
        if (!currentPos.HasValue) return false;

        var newPos = currentPos.Value.Move(direction);
        if (!grid.IsValidPosition(newPos)) return false;

        var cost = GetMovementCost(direction);
        return GetRemainingMovement(entityId) >= cost;
    }

    /// <inheritdoc/>
    public IEnumerable<MovementDirection> GetValidDirections(Guid entityId)
    {
        foreach (var direction in Enum.GetValues<MovementDirection>())
        {
            if (CanMove(entityId, direction))
                yield return direction;
        }
    }

    /// <inheritdoc/>
    public int GetMovementCost(MovementDirection direction) =>
        MovementCosts.GetCost(direction);

    // ===== Private Helpers =====

    private bool UseEntityMovementPoints(Guid entityId, int cost)
    {
        if (_player?.Id == entityId)
            return _player.UseMovementPoints(cost);

        if (_monsters.TryGetValue(entityId, out var monster))
            return monster.UseMovementPoints(cost);

        return false;
    }

    private void RefundEntityMovementPoints(Guid entityId, int cost)
    {
        // Add negative cost to refund
        if (_player?.Id == entityId)
        {
            _player.UseMovementPoints(-cost);
            return;
        }

        if (_monsters.TryGetValue(entityId, out var monster))
            monster.UseMovementPoints(-cost);
    }

    private void UpdateEntityGridPosition(Guid entityId, GridPosition newPos)
    {
        if (_player?.Id == entityId)
        {
            _player.SetCombatGridPosition(newPos);
            return;
        }

        if (_monsters.TryGetValue(entityId, out var monster))
            monster.SetCombatGridPosition(newPos);
    }

    private string GetEntityName(Guid entityId)
    {
        if (_player?.Id == entityId)
            return _player.Name;

        if (_monsters.TryGetValue(entityId, out var monster))
            return monster.Name;

        return "something";
    }
}
