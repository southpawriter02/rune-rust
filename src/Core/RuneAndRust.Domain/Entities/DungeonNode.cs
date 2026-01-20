using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a node in the dungeon topology graph.
/// This is the abstract skeleton before room templates are applied.
/// </summary>
public class DungeonNode : IEntity
{
    public Guid Id { get; private set; }
    public string SectorId { get; private set; }
    public Coordinate3D Coordinate { get; private set; }
    public RoomArchetype Archetype { get; private set; }
    public bool IsStartNode { get; private set; }
    public bool IsBossArena { get; private set; }

    private readonly Dictionary<Direction, Guid> _connections = [];
    private readonly HashSet<string> _tags = [];

    public IReadOnlyDictionary<Direction, Guid> Connections => _connections.AsReadOnly();
    public IReadOnlySet<string> Tags => _tags;

    private DungeonNode()
    {
        SectorId = null!;
    } // For EF Core

    public DungeonNode(string sectorId, Coordinate3D coordinate, RoomArchetype archetype = RoomArchetype.Chamber)
    {
        if (string.IsNullOrWhiteSpace(sectorId))
            throw new ArgumentException("Sector ID cannot be empty", nameof(sectorId));

        Id = Guid.NewGuid();
        SectorId = sectorId;
        Coordinate = coordinate;
        Archetype = archetype;
        IsStartNode = false;
        IsBossArena = false;
    }

    /// <summary>
    /// Adds a bidirectional connection to another node.
    /// </summary>
    public void AddConnection(Direction direction, Guid targetNodeId)
    {
        if (targetNodeId == Guid.Empty)
            throw new ArgumentException("Target node ID cannot be empty", nameof(targetNodeId));

        _connections[direction] = targetNodeId;
    }

    /// <summary>
    /// Checks if there's a connection in the specified direction.
    /// </summary>
    public bool HasConnection(Direction direction) => _connections.ContainsKey(direction);

    /// <summary>
    /// Gets the connected node ID in the specified direction.
    /// </summary>
    public Guid? GetConnection(Direction direction) =>
        _connections.TryGetValue(direction, out var nodeId) ? nodeId : null;

    /// <summary>
    /// Returns the number of connections this node has.
    /// </summary>
    public int GetConnectionCount() => _connections.Count;

    /// <summary>
    /// Marks this node as the starting point of the sector.
    /// </summary>
    public void SetAsStartNode()
    {
        IsStartNode = true;
        IsBossArena = false;
    }

    /// <summary>
    /// Marks this node as the boss arena (final room).
    /// </summary>
    public void SetAsBossArena()
    {
        IsBossArena = true;
        IsStartNode = false;
        Archetype = RoomArchetype.BossArena;
    }

    /// <summary>
    /// Updates the archetype based on connection count.
    /// Called after topology generation is complete.
    /// </summary>
    public void UpdateArchetypeFromConnections()
    {
        if (IsBossArena) return; // Boss arena archetype is fixed

        Archetype = GetConnectionCount() switch
        {
            1 => RoomArchetype.DeadEnd,
            2 => RoomArchetype.Corridor,
            _ => RoomArchetype.Junction
        };
    }

    /// <summary>
    /// Sets the archetype explicitly (used for stairwells).
    /// </summary>
    public void SetArchetype(RoomArchetype archetype)
    {
        if (!IsBossArena) // Cannot change boss arena
            Archetype = archetype;
    }

    /// <summary>
    /// Adds a tag to this node for descriptor matching.
    /// </summary>
    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
            _tags.Add(tag);
    }

    /// <summary>
    /// Adds multiple tags to this node.
    /// </summary>
    public void AddTags(IEnumerable<string> tags)
    {
        foreach (var tag in tags)
            AddTag(tag);
    }

    /// <summary>
    /// Checks if this node has a specific tag.
    /// </summary>
    public bool HasTag(string tag) => _tags.Contains(tag);

    /// <summary>
    /// Removes a tag from this node.
    /// </summary>
    public bool RemoveTag(string tag) => _tags.Remove(tag);

    /// <summary>
    /// Clears all tags from this node.
    /// </summary>
    public void ClearTags() => _tags.Clear();

    public override string ToString() => $"{SectorId} @ {Coordinate} [{Archetype}]";
}
