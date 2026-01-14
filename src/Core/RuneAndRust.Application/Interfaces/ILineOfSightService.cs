using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for line of sight calculations.
/// </summary>
public interface ILineOfSightService
{
    /// <summary>
    /// Checks if there is line of sight between two entities.
    /// </summary>
    /// <param name="fromEntityId">The source entity ID.</param>
    /// <param name="toEntityId">The target entity ID.</param>
    /// <returns>Result indicating LOS status.</returns>
    LineOfSightResult HasLineOfSight(Guid fromEntityId, Guid toEntityId);

    /// <summary>
    /// Checks if there is line of sight between two positions.
    /// </summary>
    /// <param name="from">The source position.</param>
    /// <param name="to">The target position.</param>
    /// <returns>Result indicating LOS status.</returns>
    LineOfSightResult HasLineOfSight(GridPosition from, GridPosition to);

    /// <summary>
    /// Gets all cells along the line between two positions using Bresenham's algorithm.
    /// </summary>
    /// <param name="from">The source position.</param>
    /// <param name="to">The target position.</param>
    /// <returns>All positions on the line.</returns>
    IEnumerable<GridPosition> GetLineCells(GridPosition from, GridPosition to);

    /// <summary>
    /// Gets the first blocking cell along the line between positions.
    /// </summary>
    /// <param name="from">The source position.</param>
    /// <param name="to">The target position.</param>
    /// <returns>The first blocking cell, or null if path is clear.</returns>
    GridPosition? GetFirstBlockingCell(GridPosition from, GridPosition to);

    /// <summary>
    /// Gets all positions visible from a given position within range.
    /// </summary>
    /// <param name="from">The source position.</param>
    /// <param name="maxRange">Maximum range to check.</param>
    /// <returns>All visible positions.</returns>
    IEnumerable<GridPosition> GetVisiblePositions(GridPosition from, int maxRange);
}

/// <summary>
/// Result of a line of sight check.
/// </summary>
/// <param name="HasLOS">True if line of sight is clear.</param>
/// <param name="FromPosition">The source position.</param>
/// <param name="ToPosition">The target position.</param>
/// <param name="BlockedBy">The position blocking LOS, if any.</param>
/// <param name="Message">Human-readable message.</param>
public readonly record struct LineOfSightResult(
    bool HasLOS,
    GridPosition FromPosition,
    GridPosition ToPosition,
    GridPosition? BlockedBy,
    string Message);
