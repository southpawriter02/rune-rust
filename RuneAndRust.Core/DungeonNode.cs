namespace RuneAndRust.Core;

/// <summary>
/// Represents the type/role of a dungeon node in the generation graph (v0.10)
/// </summary>
public enum NodeType
{
    Start,      // Entry hall (exactly 1 per dungeon)
    Main,       // On critical path to boss
    Branch,     // Off main path (optional exploration)
    Secret,     // Hidden/optional (requires discovery)
    Boss        // Final room (exactly 1 per dungeon)
}

/// <summary>
/// Represents a node in the dungeon generation graph (v0.10)
/// Each node will eventually become a Room instance
/// </summary>
public class DungeonNode
{
    // Identity
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // For debugging/logging

    // Template Association
    public RoomTemplate Template { get; set; } = null!;

    // Graph Properties
    public NodeType Type { get; set; } = NodeType.Main;
    public int Depth { get; set; } = 0; // Distance from start node

    // Metadata
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new(); // Extensibility

    /// <summary>
    /// Gets whether this node is on the critical path (Start, Main, or Boss)
    /// </summary>
    public bool IsOnCriticalPath()
    {
        return Type is NodeType.Start or NodeType.Main or NodeType.Boss;
    }

    /// <summary>
    /// Gets whether this node is optional (Branch or Secret)
    /// </summary>
    public bool IsOptional()
    {
        return Type is NodeType.Branch or NodeType.Secret;
    }

    /// <summary>
    /// Gets a debug-friendly representation of this node
    /// </summary>
    public override string ToString()
    {
        return $"Node {Id}: {Template?.TemplateId ?? "No Template"} ({Type}, Depth={Depth})";
    }
}
