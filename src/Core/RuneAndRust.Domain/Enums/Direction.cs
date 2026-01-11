namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the directions for movement within the dungeon.
/// </summary>
/// <remarks>
/// Directions are used for navigating between rooms and establishing exits.
/// The dungeon uses a 3D grid system where North increases Y, South decreases Y,
/// East increases X, West decreases X, Up decreases Z (toward surface), and
/// Down increases Z (deeper underground).
/// </remarks>
public enum Direction
{
    /// <summary>
    /// Movement toward positive Y coordinates (up on the map).
    /// </summary>
    North = 0,

    /// <summary>
    /// Movement toward negative Y coordinates (down on the map).
    /// </summary>
    South = 1,

    /// <summary>
    /// Movement toward positive X coordinates (right on the map).
    /// </summary>
    East = 2,

    /// <summary>
    /// Movement toward negative X coordinates (left on the map).
    /// </summary>
    West = 3,

    /// <summary>
    /// Movement toward the surface (Z - 1). Used for stairs, ladders going up.
    /// </summary>
    Up = 4,

    /// <summary>
    /// Movement deeper underground (Z + 1). Used for stairs, ladders going down.
    /// </summary>
    Down = 5
}
