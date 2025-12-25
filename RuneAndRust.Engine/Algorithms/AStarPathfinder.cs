using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Engine.Algorithms;

/// <summary>
/// A* pathfinding implementation using PriorityQueue for optimal performance (v0.3.18b - The Hot Path).
/// Uses Manhattan distance heuristic for consistent path calculation.
/// </summary>
public class AStarPathfinder : IPathfindingService
{
    private readonly ILogger<AStarPathfinder> _logger;

    /// <summary>
    /// Maximum number of nodes to explore before giving up.
    /// Prevents infinite loops on large or impossible searches.
    /// </summary>
    private const int MaxNodesExplored = 10000;

    /// <summary>
    /// Initializes a new instance of the <see cref="AStarPathfinder"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public AStarPathfinder(ILogger<AStarPathfinder> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public List<Coordinate>? FindPath(Coordinate start, Coordinate end, ISpatialHashGrid obstacles)
    {
        var stopwatch = Stopwatch.StartNew();

        // Same position - no movement needed
        if (start == end)
        {
            _logger.LogTrace("[Pathing] Start equals end at {Position}, returning empty path", start);
            return new List<Coordinate>();
        }

        // Check if end is blocked (but allow if it's the start position)
        if (obstacles.IsBlocked(end))
        {
            _logger.LogDebug(
                "[Pathing] Target {End} is blocked, no path possible from {Start}",
                end, start);
            return null;
        }

        var frontier = new PriorityQueue<Coordinate, int>();
        frontier.Enqueue(start, 0);

        var cameFrom = new Dictionary<Coordinate, Coordinate>();
        var costSoFar = new Dictionary<Coordinate, int>
        {
            [start] = 0
        };

        var nodesExplored = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            nodesExplored++;

            // Safety limit
            if (nodesExplored > MaxNodesExplored)
            {
                _logger.LogWarning(
                    "[Pathing] Search exceeded {MaxNodes} nodes from {Start} to {End}, aborting",
                    MaxNodesExplored, start, end);
                return null;
            }

            // Found the goal
            if (current == end)
            {
                var path = ReconstructPath(cameFrom, start, end);
                stopwatch.Stop();

                _logger.LogTrace(
                    "[Perf] Pathfinding took {Ms}ms. Nodes checked: {Nodes}. Path length: {Length}",
                    stopwatch.ElapsedMilliseconds, nodesExplored, path.Count);

                return path;
            }

            // Explore neighbors (4-directional: N, S, E, W)
            foreach (var next in GetNeighbors(current))
            {
                // Skip blocked positions (but allow the end position to be "blocked" by target)
                if (obstacles.IsBlocked(next) && next != end)
                {
                    continue;
                }

                var newCost = costSoFar[current] + 1; // Uniform cost of 1 per move

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    var priority = newCost + GetDistance(next, end);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        // No path found
        stopwatch.Stop();
        _logger.LogDebug(
            "[Pathing] No path from {Start} to {End}. Nodes checked: {Nodes}. Time: {Ms}ms",
            start, end, nodesExplored, stopwatch.ElapsedMilliseconds);

        return null;
    }

    /// <inheritdoc/>
    public int GetDistance(Coordinate a, Coordinate b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z);
    }

    /// <inheritdoc/>
    public bool HasPath(Coordinate start, Coordinate end, ISpatialHashGrid obstacles)
    {
        // For efficiency, we could implement a simpler BFS here,
        // but for now we reuse FindPath and check result
        var path = FindPath(start, end, obstacles);
        return path != null;
    }

    /// <summary>
    /// Gets the 4-directional neighbors of a coordinate (N, S, E, W).
    /// Does not include diagonal movement for simplicity.
    /// </summary>
    /// <param name="coord">The coordinate to get neighbors for.</param>
    /// <returns>An enumerable of neighboring coordinates.</returns>
    private static IEnumerable<Coordinate> GetNeighbors(Coordinate coord)
    {
        // North (Y+1)
        yield return coord.Offset(0, 1, 0);
        // South (Y-1)
        yield return coord.Offset(0, -1, 0);
        // East (X+1)
        yield return coord.Offset(1, 0, 0);
        // West (X-1)
        yield return coord.Offset(-1, 0, 0);

        // Note: Z-axis movement (Up/Down) could be added for multi-level combat
        // yield return coord.Offset(0, 0, 1);  // Up
        // yield return coord.Offset(0, 0, -1); // Down
    }

    /// <summary>
    /// Reconstructs the path from start to end using the cameFrom map.
    /// </summary>
    /// <param name="cameFrom">Map of coordinate to its predecessor.</param>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="end">The ending coordinate.</param>
    /// <returns>A list of coordinates from start to end (excluding start, including end).</returns>
    private static List<Coordinate> ReconstructPath(
        Dictionary<Coordinate, Coordinate> cameFrom,
        Coordinate start,
        Coordinate end)
    {
        var path = new List<Coordinate>();
        var current = end;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }
}
