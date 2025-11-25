using RuneAndRust.Core;
using Serilog;
using Direction = RuneAndRust.Core.Direction;

namespace RuneAndRust.Engine;

/// <summary>
/// Assigns compass directions to dungeon graph edges (v0.10)
/// </summary>
public class DirectionAssigner
{
    private static readonly ILogger _log = Log.ForContext<DirectionAssigner>();
    private Random _rng = null!;

    /// <summary>
    /// Assigns directions to all edges in the graph
    /// </summary>
    public void AssignDirections(DungeonGraph graph, Random rng)
    {
        _rng = rng;
        _log.Debug("Starting direction assignment for {NodeCount} nodes, {EdgeCount} edges",
            graph.NodeCount, graph.EdgeCount);

        // Track used directions for each node
        var usedDirections = new Dictionary<DungeonNode, HashSet<Direction>>();
        foreach (var node in graph.GetNodes())
        {
            usedDirections[node] = new HashSet<Direction>();
        }

        // Process edges in BFS order from start node for more logical layouts
        var processedEdges = new HashSet<DungeonEdge>();
        var startNode = graph.StartNode;

        if (startNode == null)
        {
            _log.Warning("No start node found, cannot assign directions");
            return;
        }

        // BFS traversal for direction assignment
        var visited = new HashSet<DungeonNode>();
        var queue = new Queue<DungeonNode>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            var edges = graph.GetEdgesFrom(currentNode);

            foreach (var edge in edges)
            {
                // Skip if already processed (handles bidirectional edges)
                if (processedEdges.Contains(edge))
                    continue;

                // Assign direction to this edge
                AssignDirectionToEdge(edge, usedDirections);
                processedEdges.Add(edge);

                // Add neighbor to queue
                if (!visited.Contains(edge.To))
                {
                    visited.Add(edge.To);
                    queue.Enqueue(edge.To);
                }
            }
        }

        // Log statistics
        var assignedCount = graph.GetEdges().Count(e => e.HasDirections());
        _log.Information("Direction assignment complete: {Assigned}/{Total} edges assigned",
            assignedCount, graph.EdgeCount);

        if (assignedCount < graph.EdgeCount)
        {
            _log.Warning("Some edges were not assigned directions");
        }
    }

    /// <summary>
    /// Assigns a direction to a single edge
    /// </summary>
    private void AssignDirectionToEdge(DungeonEdge edge, Dictionary<DungeonNode, HashSet<Direction>> usedDirections)
    {
        // Get available directions from the From node
        var availableDirections = DirectionExtensions.GetAllDirections()
            .Where(d => !usedDirections[edge.From].Contains(d))
            .ToList();

        if (availableDirections.Count == 0)
        {
            _log.Warning("No available directions for edge {From} -> {To}",
                edge.From.Id, edge.To.Id);
            return;
        }

        // Pick a random available direction
        var direction = availableDirections[_rng.Next(availableDirections.Count)];
        var oppositeDirection = direction.GetOpposite();

        // Check if opposite direction is available at To node
        if (usedDirections[edge.To].Contains(oppositeDirection))
        {
            // Try to find a different direction where both sides are available
            var validPair = availableDirections.FirstOrDefault(d =>
                !usedDirections[edge.To].Contains(d.GetOpposite()));

            if (validPair != default(Direction))
            {
                direction = validPair;
                oppositeDirection = direction.GetOpposite();
            }
            else
            {
                // No perfect pair available, just use what we have
                _log.Debug("Direction conflict at edge {From} -> {To}, using {Direction} anyway",
                    edge.From.Id, edge.To.Id, direction);
            }
        }

        // Assign directions
        edge.FromDirection = direction;
        edge.ToDirection = oppositeDirection;

        // Mark as used
        usedDirections[edge.From].Add(direction);
        usedDirections[edge.To].Add(oppositeDirection);

        _log.Debug("Assigned direction: Node {From} --{Direction}--> Node {To} (back: {OppositeDirection})",
            edge.From.Id, direction, edge.To.Id, oppositeDirection);
    }

    /// <summary>
    /// Validates that all edges have valid, consistent directions
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateDirections(DungeonGraph graph)
    {
        var errors = new List<string>();

        foreach (var edge in graph.GetEdges())
        {
            // Check if directions are assigned
            if (!edge.HasDirections())
            {
                errors.Add($"Edge {edge.From.Id} -> {edge.To.Id} has no directions assigned");
                continue;
            }

            // Check if directions are opposites (for bidirectional edges)
            if (edge.IsBidirectional)
            {
                var expectedOpposite = edge.FromDirection!.Value.GetOpposite();
                if (edge.ToDirection != expectedOpposite)
                {
                    errors.Add($"Edge {edge.From.Id} -> {edge.To.Id} has inconsistent directions: " +
                              $"{edge.FromDirection} / {edge.ToDirection} (expected {expectedOpposite})");
                }
            }
        }

        // Check for duplicate directions at each node
        foreach (var node in graph.GetNodes())
        {
            var edgesFromNode = graph.GetEdgesFrom(node);
            var directions = edgesFromNode
                .Where(e => e.FromDirection.HasValue)
                .Select(e => e.FromDirection!.Value)
                .ToList();

            var duplicates = directions.GroupBy(d => d)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                errors.Add($"Node {node.Id} has duplicate directions: {string.Join(", ", duplicates)}");
            }
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Gets statistics about direction usage
    /// </summary>
    public Dictionary<string, int> GetDirectionStatistics(DungeonGraph graph)
    {
        var stats = new Dictionary<string, int>
        {
            ["TotalEdges"] = graph.EdgeCount,
            ["AssignedEdges"] = graph.GetEdges().Count(e => e.HasDirections()),
            ["UnassignedEdges"] = graph.GetEdges().Count(e => !e.HasDirections()),
            ["NorthEdges"] = graph.GetEdges().Count(e => e.FromDirection == Direction.North),
            ["SouthEdges"] = graph.GetEdges().Count(e => e.FromDirection == Direction.South),
            ["EastEdges"] = graph.GetEdges().Count(e => e.FromDirection == Direction.East),
            ["WestEdges"] = graph.GetEdges().Count(e => e.FromDirection == Direction.West)
        };

        return stats;
    }
}
