using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// A sector represents a contiguous area of connected dungeon nodes.
/// This is the container for the topology graph before room instantiation.
/// </summary>
public class Sector : IEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Biome Biome { get; private set; }
    public int Depth { get; private set; }
    public int ThreatBudget { get; private set; }
    public Guid? StartNodeId { get; private set; }
    public Guid? BossNodeId { get; private set; }

    private readonly Dictionary<Guid, DungeonNode> _nodes = [];
    private readonly Dictionary<Coordinate3D, Guid> _coordinateIndex = [];

    public IReadOnlyDictionary<Guid, DungeonNode> Nodes => _nodes.AsReadOnly();

    private Sector()
    {
        Name = null!;
    } // For EF Core

    public Sector(string name, Biome biome, int depth = 1)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Sector name cannot be empty", nameof(name));
        if (depth < 1)
            throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be at least 1");

        Id = Guid.NewGuid();
        Name = name;
        Biome = biome;
        Depth = depth;
        ThreatBudget = 0;
    }

    /// <summary>
    /// Adds a node to the sector.
    /// </summary>
    public void AddNode(DungeonNode node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        if (_coordinateIndex.ContainsKey(node.Coordinate))
            throw new InvalidOperationException($"A node already exists at coordinate {node.Coordinate}");

        _nodes[node.Id] = node;
        _coordinateIndex[node.Coordinate] = node.Id;
    }

    /// <summary>
    /// Gets a node by its ID.
    /// </summary>
    public DungeonNode? GetNode(Guid nodeId) =>
        _nodes.TryGetValue(nodeId, out var node) ? node : null;

    /// <summary>
    /// Gets a node by its coordinate.
    /// </summary>
    public DungeonNode? GetNodeByCoordinate(Coordinate3D coordinate) =>
        _coordinateIndex.TryGetValue(coordinate, out var nodeId) ? _nodes[nodeId] : null;

    /// <summary>
    /// Checks if a coordinate is occupied by a node.
    /// </summary>
    public bool IsCoordinateOccupied(Coordinate3D coordinate) =>
        _coordinateIndex.ContainsKey(coordinate);

    /// <summary>
    /// Sets the starting node of the sector.
    /// </summary>
    public void SetStartNode(Guid nodeId)
    {
        if (!_nodes.ContainsKey(nodeId))
            throw new ArgumentException($"Node {nodeId} does not exist in this sector", nameof(nodeId));

        StartNodeId = nodeId;
        _nodes[nodeId].SetAsStartNode();
    }

    /// <summary>
    /// Sets the boss arena node of the sector.
    /// </summary>
    public void SetBossNode(Guid nodeId)
    {
        if (!_nodes.ContainsKey(nodeId))
            throw new ArgumentException($"Node {nodeId} does not exist in this sector", nameof(nodeId));

        BossNodeId = nodeId;
        _nodes[nodeId].SetAsBossArena();
    }

    /// <summary>
    /// Sets the threat budget for this sector.
    /// </summary>
    public void SetThreatBudget(int budget)
    {
        if (budget < 0)
            throw new ArgumentOutOfRangeException(nameof(budget), "Threat budget cannot be negative");

        ThreatBudget = budget;
    }

    /// <summary>
    /// Calculates threat budget from difficulty tier and depth.
    /// Formula: BaseDifficulty + (Depth × 10)
    /// </summary>
    public void CalculateThreatBudget(DifficultyTier tier)
    {
        ThreatBudget = (int)tier + (Depth * 10);
    }

    /// <summary>
    /// Gets all nodes in the sector.
    /// </summary>
    public IEnumerable<DungeonNode> GetAllNodes() => _nodes.Values;

    /// <summary>
    /// Gets the total number of nodes.
    /// </summary>
    public int GetNodeCount() => _nodes.Count;

    /// <summary>
    /// Gets nodes that have fewer than the maximum connections (4 for most, 6 for stairwells).
    /// Used by the topology generator to find growth points.
    /// </summary>
    public IEnumerable<DungeonNode> GetActiveNodes(int maxConnections = 4) =>
        _nodes.Values.Where(n => n.GetConnectionCount() < maxConnections);

    /// <summary>
    /// Gets nodes with exactly one connection (potential boss arena candidates).
    /// </summary>
    public IEnumerable<DungeonNode> GetLeafNodes() =>
        _nodes.Values.Where(n => n.GetConnectionCount() == 1);

    /// <summary>
    /// Updates all node archetypes based on their connection counts.
    /// Called after topology generation is complete.
    /// </summary>
    public void FinalizeNodeArchetypes()
    {
        foreach (var node in _nodes.Values)
        {
            node.UpdateArchetypeFromConnections();
        }
    }

    /// <summary>
    /// Creates bidirectional connections between two nodes.
    /// </summary>
    public void ConnectNodes(Guid node1Id, Direction direction, Guid node2Id)
    {
        var node1 = GetNode(node1Id) ?? throw new ArgumentException($"Node {node1Id} not found");
        var node2 = GetNode(node2Id) ?? throw new ArgumentException($"Node {node2Id} not found");

        var oppositeDirection = GetOppositeDirection(direction);
        node1.AddConnection(direction, node2Id);
        node2.AddConnection(oppositeDirection, node1Id);
    }

    private static Direction GetOppositeDirection(Direction direction) => direction switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.Northeast => Direction.Southwest,
        Direction.Northwest => Direction.Southeast,
        Direction.Southeast => Direction.Northwest,
        Direction.Southwest => Direction.Northeast,
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };

    public override string ToString() => $"{Name} ({Biome}, Depth {Depth}, {GetNodeCount()} nodes)";
}
