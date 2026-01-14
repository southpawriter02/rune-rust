using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a 2D grid for tactical combat positioning.
/// </summary>
/// <remarks>
/// <para>
/// The CombatGrid manages a rectangular grid of cells where combatants
/// (player and monsters) can be positioned during tactical combat.
/// </para>
/// <para>
/// Grid dimensions are constrained between 3x3 and 20x20 cells.
/// The default grid is 8x8, suitable for most combat encounters.
/// </para>
/// <para>
/// Entity tracking is bidirectional:
/// <list type="bullet">
/// <item><description>Cells track which entity occupies them</description></item>
/// <item><description>EntityPositions provides O(1) lookup of entity positions</description></item>
/// </list>
/// </para>
/// </remarks>
public class CombatGrid : IEntity
{
    /// <summary>
    /// Minimum allowed grid dimension.
    /// </summary>
    public const int MinDimension = 3;

    /// <summary>
    /// Maximum allowed grid dimension.
    /// </summary>
    public const int MaxDimension = 20;

    /// <summary>
    /// Gets the unique identifier for this grid.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the width of the grid (number of columns).
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the height of the grid (number of rows).
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the associated room ID, if any.
    /// </summary>
    public Guid? RoomId { get; private set; }

    private readonly Dictionary<GridPosition, GridCell> _cells = new();
    private readonly Dictionary<Guid, GridPosition> _entityPositions = new();

    /// <summary>
    /// Gets all cells in the grid, keyed by position.
    /// </summary>
    public IReadOnlyDictionary<GridPosition, GridCell> Cells => _cells;

    /// <summary>
    /// Gets all entity positions, keyed by entity ID.
    /// </summary>
    public IReadOnlyDictionary<Guid, GridPosition> EntityPositions => _entityPositions;

    /// <summary>
    /// Private constructor for factory methods.
    /// </summary>
    private CombatGrid() { }

    /// <summary>
    /// Creates a new combat grid with the specified dimensions.
    /// </summary>
    /// <param name="width">Grid width (columns). Must be between 3 and 20.</param>
    /// <param name="height">Grid height (rows). Must be between 3 and 20.</param>
    /// <param name="roomId">Optional associated room ID.</param>
    /// <returns>A new initialized CombatGrid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when width or height is outside the valid range.
    /// </exception>
    public static CombatGrid Create(int width, int height, Guid? roomId = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, MinDimension);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, MaxDimension);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, MinDimension);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(height, MaxDimension);

        var grid = new CombatGrid
        {
            Id = Guid.NewGuid(),
            Width = width,
            Height = height,
            RoomId = roomId
        };

        grid.InitializeCells();
        return grid;
    }

    /// <summary>
    /// Creates a default 8x8 combat grid.
    /// </summary>
    /// <param name="roomId">Optional associated room ID.</param>
    /// <returns>A new 8x8 CombatGrid.</returns>
    public static CombatGrid CreateDefault(Guid? roomId = null) => Create(8, 8, roomId);

    /// <summary>
    /// Initializes all cells in the grid.
    /// </summary>
    private void InitializeCells()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var position = new GridPosition(x, y);
                _cells[position] = GridCell.Create(position);
            }
        }
    }

    /// <summary>
    /// Gets the cell at the specified position.
    /// </summary>
    /// <param name="position">The position to query.</param>
    /// <returns>The GridCell at the position, or null if out of bounds.</returns>
    public GridCell? GetCell(GridPosition position) =>
        _cells.TryGetValue(position, out var cell) ? cell : null;

    /// <summary>
    /// Gets the cell at the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns>The GridCell at the coordinates, or null if out of bounds.</returns>
    public GridCell? GetCell(int x, int y) => GetCell(new GridPosition(x, y));

    /// <summary>
    /// Checks if a position is within grid bounds.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns><c>true</c> if the position is within bounds; otherwise, <c>false</c>.</returns>
    public bool IsInBounds(GridPosition position) =>
        position.X >= 0 && position.X < Width &&
        position.Y >= 0 && position.Y < Height;

    /// <summary>
    /// Checks if a position is valid for movement (in bounds, passable, unoccupied).
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns><c>true</c> if an entity can move to this position; otherwise, <c>false</c>.</returns>
    public bool IsValidPosition(GridPosition position)
    {
        if (!IsInBounds(position))
            return false;

        var cell = GetCell(position);
        return cell != null && cell.IsPassable && !cell.IsOccupied;
    }

    /// <summary>
    /// Places an entity on the grid at the specified position.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="position">The position to place the entity.</param>
    /// <param name="isPlayer">Whether the entity is the player.</param>
    /// <returns><c>true</c> if placement succeeded; otherwise, <c>false</c>.</returns>
    public bool PlaceEntity(Guid entityId, GridPosition position, bool isPlayer)
    {
        if (!IsInBounds(position))
            return false;

        var cell = GetCell(position);
        if (cell == null || !cell.PlaceEntity(entityId, isPlayer))
            return false;

        _entityPositions[entityId] = position;
        return true;
    }

    /// <summary>
    /// Removes an entity from the grid.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to remove.</param>
    /// <returns><c>true</c> if removal succeeded; otherwise, <c>false</c>.</returns>
    public bool RemoveEntity(Guid entityId)
    {
        if (!_entityPositions.TryGetValue(entityId, out var position))
            return false;

        var cell = GetCell(position);
        if (cell == null || !cell.RemoveEntity())
            return false;

        _entityPositions.Remove(entityId);
        return true;
    }

    /// <summary>
    /// Moves an entity to a new position on the grid.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to move.</param>
    /// <param name="newPosition">The target position.</param>
    /// <returns><c>true</c> if the move succeeded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// If placement at the new position fails, the entity remains at its
    /// original position (automatic rollback).
    /// </remarks>
    public bool MoveEntity(Guid entityId, GridPosition newPosition)
    {
        if (!_entityPositions.TryGetValue(entityId, out var currentPosition))
            return false;

        if (!IsValidPosition(newPosition))
            return false;

        var currentCell = GetCell(currentPosition);
        var newCell = GetCell(newPosition);

        if (currentCell == null || newCell == null)
            return false;

        var isPlayer = currentCell.IsPlayerOccupied;

        // Remove from current cell
        if (!currentCell.RemoveEntity())
            return false;

        // Attempt to place in new cell
        if (!newCell.PlaceEntity(entityId, isPlayer))
        {
            // Rollback: restore to original position
            currentCell.PlaceEntity(entityId, isPlayer);
            return false;
        }

        _entityPositions[entityId] = newPosition;
        return true;
    }

    /// <summary>
    /// Gets an entity's current position on the grid.
    /// </summary>
    /// <param name="entityId">The entity's unique identifier.</param>
    /// <returns>The entity's position, or null if not on the grid.</returns>
    public GridPosition? GetEntityPosition(Guid entityId) =>
        _entityPositions.TryGetValue(entityId, out var position) ? position : null;

    /// <summary>
    /// Gets the distance between two entities on the grid.
    /// </summary>
    /// <param name="entityId1">First entity's ID.</param>
    /// <param name="entityId2">Second entity's ID.</param>
    /// <returns>The Chebyshev distance, or null if either entity is not on the grid.</returns>
    public int? GetDistanceBetween(Guid entityId1, Guid entityId2)
    {
        var pos1 = GetEntityPosition(entityId1);
        var pos2 = GetEntityPosition(entityId2);

        if (!pos1.HasValue || !pos2.HasValue)
            return null;

        return pos1.Value.DistanceTo(pos2.Value);
    }

    /// <summary>
    /// Checks if two entities are in adjacent cells.
    /// </summary>
    /// <param name="entityId1">First entity's ID.</param>
    /// <param name="entityId2">Second entity's ID.</param>
    /// <returns><c>true</c> if the entities are adjacent; otherwise, <c>false</c>.</returns>
    public bool AreAdjacent(Guid entityId1, Guid entityId2) =>
        GetDistanceBetween(entityId1, entityId2) == 1;

    /// <summary>
    /// Gets all entity IDs within a given range of a position.
    /// </summary>
    /// <param name="center">The center position.</param>
    /// <param name="range">The maximum distance (Chebyshev).</param>
    /// <returns>An enumerable of entity IDs within range.</returns>
    public IEnumerable<Guid> GetEntitiesInRange(GridPosition center, int range)
    {
        foreach (var (entityId, position) in _entityPositions)
        {
            if (center.DistanceTo(position) <= range)
                yield return entityId;
        }
    }

    /// <summary>
    /// Gets all cells adjacent to a position.
    /// </summary>
    /// <param name="position">The center position.</param>
    /// <returns>An enumerable of adjacent GridCells within grid bounds.</returns>
    public IEnumerable<GridCell> GetAdjacentCells(GridPosition position)
    {
        var directions = Enum.GetValues<MovementDirection>();
        foreach (var direction in directions)
        {
            var adjacent = position.Move(direction);
            if (IsInBounds(adjacent))
            {
                var cell = GetCell(adjacent);
                if (cell != null)
                    yield return cell;
            }
        }
    }
}
