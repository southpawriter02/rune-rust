using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing combat grid operations.
/// </summary>
/// <remarks>
/// <para>
/// The combat grid service handles grid creation, entity placement,
/// and provides query methods for distances and adjacency.
/// </para>
/// <para>
/// Only one grid can be active at a time. Call <see cref="SetActiveGrid"/>
/// when combat starts and <see cref="ClearGrid"/> when combat ends.
/// </para>
/// </remarks>
public interface ICombatGridService
{
    /// <summary>
    /// Creates a new combat grid for a room.
    /// </summary>
    /// <param name="room">The room where combat is taking place.</param>
    /// <param name="width">Optional custom width (uses default if not specified).</param>
    /// <param name="height">Optional custom height (uses default if not specified).</param>
    /// <returns>A new CombatGrid instance.</returns>
    CombatGrid CreateGrid(Room room, int? width = null, int? height = null);

    /// <summary>
    /// Initializes entity positions on a grid.
    /// </summary>
    /// <param name="grid">The grid to initialize.</param>
    /// <param name="player">The player to place.</param>
    /// <param name="monsters">The monsters to place.</param>
    /// <returns>Result containing positions and success status.</returns>
    GridInitializationResult InitializePositions(
        CombatGrid grid,
        Player player,
        IEnumerable<Monster> monsters);

    /// <summary>
    /// Gets the currently active combat grid.
    /// </summary>
    /// <returns>The active grid, or null if no combat is in progress.</returns>
    CombatGrid? GetActiveGrid();

    /// <summary>
    /// Sets the active combat grid.
    /// </summary>
    /// <param name="grid">The grid to set as active, or null to clear.</param>
    void SetActiveGrid(CombatGrid? grid);

    /// <summary>
    /// Clears the active grid (typically when combat ends).
    /// </summary>
    void ClearGrid();

    /// <summary>
    /// Gets an entity's position on the active grid.
    /// </summary>
    /// <param name="entityId">The entity's unique identifier.</param>
    /// <returns>The entity's position, or null if not found.</returns>
    GridPosition? GetEntityPosition(Guid entityId);

    /// <summary>
    /// Gets the distance between two entities on the active grid.
    /// </summary>
    /// <param name="entityId1">First entity's ID.</param>
    /// <param name="entityId2">Second entity's ID.</param>
    /// <returns>The Chebyshev distance, or null if either entity is not found.</returns>
    int? GetDistance(Guid entityId1, Guid entityId2);

    /// <summary>
    /// Checks if two entities are in adjacent cells.
    /// </summary>
    /// <param name="entityId1">First entity's ID.</param>
    /// <param name="entityId2">Second entity's ID.</param>
    /// <returns><c>true</c> if adjacent; otherwise, <c>false</c>.</returns>
    bool AreAdjacent(Guid entityId1, Guid entityId2);
}

/// <summary>
/// Result of grid initialization with entity placements.
/// </summary>
/// <param name="Success">Whether initialization succeeded.</param>
/// <param name="PlayerPosition">The player's assigned position.</param>
/// <param name="MonsterPositions">Mapping of monster IDs to their positions.</param>
/// <param name="Message">Descriptive message about the result.</param>
public readonly record struct GridInitializationResult(
    bool Success,
    GridPosition PlayerPosition,
    IReadOnlyDictionary<Guid, GridPosition> MonsterPositions,
    string Message);
