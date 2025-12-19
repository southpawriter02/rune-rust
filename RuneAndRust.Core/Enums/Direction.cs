namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the six cardinal directions for room navigation.
/// Supports full 3D movement including vertical traversal.
/// </summary>
public enum Direction
{
    /// <summary>Movement toward positive Y-axis.</summary>
    North = 0,

    /// <summary>Movement toward negative Y-axis.</summary>
    South = 1,

    /// <summary>Movement toward positive X-axis.</summary>
    East = 2,

    /// <summary>Movement toward negative X-axis.</summary>
    West = 3,

    /// <summary>Movement toward positive Z-axis (ascend).</summary>
    Up = 4,

    /// <summary>Movement toward negative Z-axis (descend).</summary>
    Down = 5
}
