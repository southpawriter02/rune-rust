namespace RuneAndRust.Core;

/// <summary>
/// Represents a graph structure for dungeon generation (v0.10)
/// Nodes are rooms, edges are connections between rooms
/// </summary>
public class DungeonGraph
{
    private readonly List<DungeonNode> _nodes = new();
    private readonly List<DungeonEdge> _edges = new();
    private int _nextNodeId = 0;

    /// <summary>
    /// Gets the start node (entry point of dungeon)
    /// </summary>
    public DungeonNode? StartNode => _nodes.FirstOrDefault(n => n.Type == NodeType.Start);

    /// <summary>
    /// Gets the boss node (final room of dungeon)
    /// </summary>
    public DungeonNode? BossNode => _nodes.FirstOrDefault(n => n.Type == NodeType.Boss);

    /// <summary>
    /// Gets the total number of nodes in the graph
    /// </summary>
    public int NodeCount => _nodes.Count;

    /// <summary>
    /// Gets the total number of edges in the graph
    /// </summary>
    public int EdgeCount => _edges.Count;

    #region Node Management

    /// <summary>
    /// Adds a node to the graph
    /// </summary>
    public void AddNode(DungeonNode node)
    {
        if (node.Id == 0)
        {
            node.Id = ++_nextNodeId;
        }

        _nodes.Add(node);
    }

    /// <summary>
    /// Removes a node from the graph (and all connected edges)
    /// </summary>
    public void RemoveNode(DungeonNode node)
    {
        // Remove all edges connected to this node
        _edges.RemoveAll(e => e.From == node || e.To == node);

        // Remove the node itself
        _nodes.Remove(node);
    }

    /// <summary>
    /// Gets all nodes in the graph
    /// </summary>
    public List<DungeonNode> GetNodes()
    {
        return new List<DungeonNode>(_nodes);
    }

    /// <summary>
    /// Gets nodes matching a specific type
    /// </summary>
    public List<DungeonNode> GetNodesByType(NodeType type)
    {
        return _nodes.Where(n => n.Type == type).ToList();
    }

    /// <summary>
    /// Gets nodes on the main path (Start, Main, Boss)
    /// </summary>
    public List<DungeonNode> GetMainPathNodes()
    {
        return _nodes.Where(n => n.IsOnCriticalPath()).ToList();
    }

    #endregion

    #region Edge Management

    /// <summary>
    /// Adds an edge between two nodes
    /// </summary>
    public void AddEdge(DungeonNode from, DungeonNode to, EdgeType type = EdgeType.Normal)
    {
        var edge = new DungeonEdge
        {
            From = from,
            To = to,
            Type = type,
            IsBidirectional = true // Default to bidirectional
        };

        _edges.Add(edge);
    }

    /// <summary>
    /// Removes an edge from the graph
    /// </summary>
    public void RemoveEdge(DungeonEdge edge)
    {
        _edges.Remove(edge);
    }

    /// <summary>
    /// Gets all edges in the graph
    /// </summary>
    public List<DungeonEdge> GetEdges()
    {
        return new List<DungeonEdge>(_edges);
    }

    /// <summary>
    /// Gets edges originating from a specific node
    /// </summary>
    public List<DungeonEdge> GetEdgesFrom(DungeonNode node)
    {
        var edges = _edges.Where(e => e.From == node).ToList();

        // For bidirectional edges, also include edges where this node is the destination
        var reverseEdges = _edges
            .Where(e => e.To == node && e.IsBidirectional)
            .Select(e => new DungeonEdge
            {
                From = node,
                To = e.From,
                Type = e.Type,
                IsBidirectional = true,
                FromDirection = e.ToDirection,
                ToDirection = e.FromDirection
            });

        edges.AddRange(reverseEdges);
        return edges;
    }

    /// <summary>
    /// Gets edges pointing to a specific node
    /// </summary>
    public List<DungeonEdge> GetEdgesTo(DungeonNode node)
    {
        return _edges.Where(e => e.To == node).ToList();
    }

    /// <summary>
    /// Checks if an edge exists between two nodes
    /// </summary>
    public bool HasEdge(DungeonNode from, DungeonNode to)
    {
        return _edges.Any(e => e.From == from && e.To == to);
    }

    #endregion

    #region Graph Traversal

    /// <summary>
    /// Gets the depth (distance from start) of a node
    /// </summary>
    public int GetDepth(DungeonNode node)
    {
        return node.Depth;
    }

    /// <summary>
    /// Gets neighbors of a node (connected nodes)
    /// </summary>
    public List<DungeonNode> GetNeighbors(DungeonNode node)
    {
        var neighbors = new List<DungeonNode>();

        // Direct connections
        neighbors.AddRange(_edges.Where(e => e.From == node).Select(e => e.To));

        // Bidirectional reverse connections
        neighbors.AddRange(_edges.Where(e => e.To == node && e.IsBidirectional).Select(e => e.From));

        return neighbors.Distinct().ToList();
    }

    /// <summary>
    /// Checks if there's a path between two nodes using BFS
    /// </summary>
    public bool IsReachable(DungeonNode from, DungeonNode to)
    {
        if (from == to) return true;

        var visited = new HashSet<DungeonNode>();
        var queue = new Queue<DungeonNode>();

        queue.Enqueue(from);
        visited.Add(from);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor == to)
                    return true;

                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the main path from start to boss using BFS
    /// </summary>
    public List<DungeonNode>? GetMainPath()
    {
        if (StartNode == null || BossNode == null)
            return null;

        var parent = new Dictionary<DungeonNode, DungeonNode?>();
        var visited = new HashSet<DungeonNode>();
        var queue = new Queue<DungeonNode>();

        queue.Enqueue(StartNode);
        visited.Add(StartNode);
        parent[StartNode] = null;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == BossNode)
            {
                // Reconstruct path
                var path = new List<DungeonNode>();
                var node = BossNode;

                while (node != null)
                {
                    path.Add(node);
                    parent.TryGetValue(node, out node);
                }

                path.Reverse();
                return path;
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    parent[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return null; // No path found
    }

    /// <summary>
    /// Gets all reachable nodes from a starting node
    /// </summary>
    public List<DungeonNode> GetReachableNodes(DungeonNode start)
    {
        var reachable = new List<DungeonNode>();
        var visited = new HashSet<DungeonNode>();
        var queue = new Queue<DungeonNode>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            reachable.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return reachable;
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validates that the graph structure is valid for dungeon generation
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        // Check for start node
        if (StartNode == null)
        {
            errors.Add("Graph has no start node");
        }

        // Check for boss node
        if (BossNode == null)
        {
            errors.Add("Graph has no boss node");
        }

        // Check for multiple start/boss nodes
        var startCount = _nodes.Count(n => n.Type == NodeType.Start);
        if (startCount > 1)
        {
            errors.Add($"Graph has {startCount} start nodes (should have exactly 1)");
        }

        var bossCount = _nodes.Count(n => n.Type == NodeType.Boss);
        if (bossCount > 1)
        {
            errors.Add($"Graph has {bossCount} boss nodes (should have exactly 1)");
        }

        // Check connectivity: start -> boss path exists
        if (StartNode != null && BossNode != null)
        {
            if (!IsReachable(StartNode, BossNode))
            {
                errors.Add("No path exists from start to boss");
            }
        }

        // Check for orphaned nodes (unreachable from start)
        if (StartNode != null)
        {
            var reachable = GetReachableNodes(StartNode);
            var orphans = _nodes.Except(reachable).ToList();

            if (orphans.Count > 0)
            {
                errors.Add($"Graph has {orphans.Count} orphaned nodes unreachable from start");
            }
        }

        // Check that no node exceeds its template's max connections
        foreach (var node in _nodes)
        {
            var connectionCount = GetEdgesFrom(node).Count;
            if (connectionCount > node.Template.MaxConnectionPoints)
            {
                errors.Add($"Node {node.Id} has {connectionCount} connections, exceeds max of {node.Template.MaxConnectionPoints}");
            }
        }

        return (errors.Count == 0, errors);
    }

    #endregion

    #region Debug & Statistics

    /// <summary>
    /// Gets statistics about the dungeon graph
    /// </summary>
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            ["TotalNodes"] = NodeCount,
            ["TotalEdges"] = EdgeCount,
            ["StartNodes"] = GetNodesByType(NodeType.Start).Count,
            ["MainNodes"] = GetNodesByType(NodeType.Main).Count,
            ["BranchNodes"] = GetNodesByType(NodeType.Branch).Count,
            ["SecretNodes"] = GetNodesByType(NodeType.Secret).Count,
            ["BossNodes"] = GetNodesByType(NodeType.Boss).Count,
            ["NormalEdges"] = _edges.Count(e => e.Type == EdgeType.Normal),
            ["SecretEdges"] = _edges.Count(e => e.Type == EdgeType.Secret)
        };
    }

    /// <summary>
    /// Gets a debug-friendly representation of the graph
    /// </summary>
    public override string ToString()
    {
        return $"DungeonGraph: {NodeCount} nodes, {EdgeCount} edges";
    }

    #endregion
}
