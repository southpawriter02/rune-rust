namespace RuneAndRust.Core;

/// <summary>
/// Represents the type of connection between nodes (v0.10)
/// </summary>
public enum EdgeType
{
    Normal,     // Standard visible exit
    Secret,     // Hidden passage (requires discovery)
    Locked      // Requires key/puzzle solution (future use)
}

/// <summary>
/// Represents a directional connection between two dungeon nodes (v0.10)
/// </summary>
public class DungeonEdge
{
    // Connection
    public DungeonNode From { get; set; } = null!;
    public DungeonNode To { get; set; } = null!;

    // Edge Properties
    public EdgeType Type { get; set; } = EdgeType.Normal;

    // Direction Assignment (populated during direction assignment phase)
    public string? FromDirection { get; set; } = null; // e.g., "north", "east"
    public string? ToDirection { get; set; } = null;   // Opposite of FromDirection

    /// <summary>
    /// Gets whether this edge represents a bidirectional connection
    /// (Most edges are bidirectional - you can go both ways)
    /// </summary>
    public bool IsBidirectional { get; set; } = true;

    /// <summary>
    /// Gets whether this edge is part of the critical path
    /// </summary>
    public bool IsOnCriticalPath()
    {
        return From.IsOnCriticalPath() && To.IsOnCriticalPath();
    }

    /// <summary>
    /// Gets whether directions have been assigned
    /// </summary>
    public bool HasDirections()
    {
        return !string.IsNullOrEmpty(FromDirection) && !string.IsNullOrEmpty(ToDirection);
    }

    /// <summary>
    /// Gets a debug-friendly representation of this edge
    /// </summary>
    public override string ToString()
    {
        var dirInfo = HasDirections() ? $"{FromDirection} <-> {ToDirection}" : "No directions";
        return $"Edge: Node {From.Id} -> Node {To.Id} ({Type}, {dirInfo})";
    }
}
