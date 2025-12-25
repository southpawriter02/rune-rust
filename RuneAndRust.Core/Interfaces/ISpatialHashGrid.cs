using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Interface for a spatial hash grid that provides O(1) entity position lookups (v0.3.18b).
/// Used by combat systems for efficient collision detection and pathfinding obstacle queries.
/// </summary>
public interface ISpatialHashGrid
{
    /// <summary>
    /// Registers an entity at the specified position.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="position">The position to register the entity at.</param>
    /// <exception cref="InvalidOperationException">Thrown if the position is already occupied.</exception>
    void Register(Guid entityId, Coordinate position);

    /// <summary>
    /// Moves an entity from one position to another.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="oldPosition">The entity's current position.</param>
    /// <param name="newPosition">The position to move the entity to.</param>
    /// <exception cref="InvalidOperationException">Thrown if the new position is already occupied.</exception>
    void Move(Guid entityId, Coordinate oldPosition, Coordinate newPosition);

    /// <summary>
    /// Removes an entity from the specified position.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="position">The position to remove the entity from.</param>
    void Remove(Guid entityId, Coordinate position);

    /// <summary>
    /// Checks if a position is blocked by any entity.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>True if the position is occupied; otherwise, false.</returns>
    bool IsBlocked(Coordinate position);

    /// <summary>
    /// Gets the entity ID at the specified position, if any.
    /// </summary>
    /// <param name="position">The position to query.</param>
    /// <returns>The entity ID at the position, or null if empty.</returns>
    Guid? GetEntityAt(Coordinate position);

    /// <summary>
    /// Clears all entities from the grid.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the number of entities currently registered in the grid.
    /// </summary>
    int Count { get; }
}
