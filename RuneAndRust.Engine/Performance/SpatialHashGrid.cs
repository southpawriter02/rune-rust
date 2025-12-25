using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Engine.Performance;

/// <summary>
/// Thread-safe spatial hash grid for O(1) entity position lookups (v0.3.18b - The Hot Path).
/// Uses Dictionary&lt;Coordinate, Guid&gt; for efficient collision detection and pathfinding obstacle queries.
/// </summary>
public class SpatialHashGrid : ISpatialHashGrid
{
    private readonly Dictionary<Coordinate, Guid> _grid = new();
    private readonly Dictionary<Guid, Coordinate> _entityPositions = new();
    private readonly object _lock = new();
    private readonly ILogger<SpatialHashGrid> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpatialHashGrid"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public SpatialHashGrid(ILogger<SpatialHashGrid> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _grid.Count;
            }
        }
    }

    /// <inheritdoc/>
    public void Register(Guid entityId, Coordinate position)
    {
        lock (_lock)
        {
            if (_grid.TryGetValue(position, out var existingEntity))
            {
                _logger.LogWarning(
                    "[Spatial] Position {Position} already occupied by {ExistingEntity}, cannot register {NewEntity}",
                    position, existingEntity, entityId);
                throw new InvalidOperationException(
                    $"Position {position} is already occupied by entity {existingEntity}.");
            }

            _grid[position] = entityId;
            _entityPositions[entityId] = position;

            _logger.LogTrace(
                "[Spatial] Registered entity {EntityId} at position {Position}. Grid count: {Count}",
                entityId, position, _grid.Count);
        }
    }

    /// <inheritdoc/>
    public void Move(Guid entityId, Coordinate oldPosition, Coordinate newPosition)
    {
        lock (_lock)
        {
            // Validate old position
            if (!_grid.TryGetValue(oldPosition, out var occupant) || occupant != entityId)
            {
                _logger.LogWarning(
                    "[Spatial] Cannot move {EntityId}: not found at {OldPosition}",
                    entityId, oldPosition);
                return;
            }

            // Check new position is free (or same as old)
            if (oldPosition != newPosition && _grid.ContainsKey(newPosition))
            {
                _logger.LogWarning(
                    "[Spatial] Cannot move {EntityId} to {NewPosition}: position occupied",
                    entityId, newPosition);
                throw new InvalidOperationException(
                    $"Cannot move entity {entityId} to {newPosition}: position is already occupied.");
            }

            // Perform the move
            _grid.Remove(oldPosition);
            _grid[newPosition] = entityId;
            _entityPositions[entityId] = newPosition;

            _logger.LogTrace(
                "[Spatial] Moved entity {EntityId} from {OldPosition} to {NewPosition}",
                entityId, oldPosition, newPosition);
        }
    }

    /// <inheritdoc/>
    public void Remove(Guid entityId, Coordinate position)
    {
        lock (_lock)
        {
            if (_grid.TryGetValue(position, out var occupant) && occupant == entityId)
            {
                _grid.Remove(position);
                _entityPositions.Remove(entityId);

                _logger.LogTrace(
                    "[Spatial] Removed entity {EntityId} from position {Position}. Grid count: {Count}",
                    entityId, position, _grid.Count);
            }
            else
            {
                _logger.LogDebug(
                    "[Spatial] Cannot remove {EntityId} from {Position}: entity not found at position",
                    entityId, position);
            }
        }
    }

    /// <inheritdoc/>
    public bool IsBlocked(Coordinate position)
    {
        lock (_lock)
        {
            return _grid.ContainsKey(position);
        }
    }

    /// <inheritdoc/>
    public Guid? GetEntityAt(Coordinate position)
    {
        lock (_lock)
        {
            return _grid.TryGetValue(position, out var entityId) ? entityId : null;
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        lock (_lock)
        {
            var previousCount = _grid.Count;
            _grid.Clear();
            _entityPositions.Clear();

            _logger.LogDebug(
                "[Spatial] Cleared grid. Removed {Count} entities",
                previousCount);
        }
    }

    /// <summary>
    /// Gets the current position of an entity, if registered.
    /// </summary>
    /// <param name="entityId">The entity ID to look up.</param>
    /// <returns>The entity's position, or null if not registered.</returns>
    public Coordinate? GetEntityPosition(Guid entityId)
    {
        lock (_lock)
        {
            return _entityPositions.TryGetValue(entityId, out var position) ? position : null;
        }
    }
}
