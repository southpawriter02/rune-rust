using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Interface for pathfinding services that calculate routes between coordinates (v0.3.18b).
/// Used by AI systems to navigate around obstacles during combat.
/// </summary>
public interface IPathfindingService
{
    /// <summary>
    /// Finds a path from start to end, avoiding blocked positions.
    /// </summary>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="end">The target coordinate.</param>
    /// <param name="obstacles">The spatial grid containing obstacle positions.</param>
    /// <returns>
    /// A list of coordinates representing the path from start to end (excluding start, including end),
    /// or null if no path exists.
    /// </returns>
    List<Coordinate>? FindPath(Coordinate start, Coordinate end, ISpatialHashGrid obstacles);

    /// <summary>
    /// Calculates the Manhattan distance between two coordinates.
    /// Used as the heuristic for A* pathfinding.
    /// </summary>
    /// <param name="a">The first coordinate.</param>
    /// <param name="b">The second coordinate.</param>
    /// <returns>The Manhattan distance (|x1-x2| + |y1-y2| + |z1-z2|).</returns>
    int GetDistance(Coordinate a, Coordinate b);

    /// <summary>
    /// Checks if there is any valid path between two coordinates.
    /// More efficient than FindPath when the actual path is not needed.
    /// </summary>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="end">The target coordinate.</param>
    /// <param name="obstacles">The spatial grid containing obstacle positions.</param>
    /// <returns>True if a path exists; otherwise, false.</returns>
    bool HasPath(Coordinate start, Coordinate end, ISpatialHashGrid obstacles);
}
