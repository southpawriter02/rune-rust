namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the cardinal directions for movement within the dungeon.
/// </summary>
/// <remarks>
/// Directions are used for navigating between rooms and establishing exits.
/// The dungeon uses a 2D grid system where North increases Y, South decreases Y,
/// East increases X, and West decreases X.
/// </remarks>
public enum Direction
{
    /// <summary>
    /// Movement toward positive Y coordinates (up on the map).
    /// </summary>
    North,

    /// <summary>
    /// Movement toward negative Y coordinates (down on the map).
    /// </summary>
    South,

    /// <summary>
    /// Movement toward positive X coordinates (right on the map).
    /// </summary>
    East,

    /// <summary>
    /// Movement toward negative X coordinates (left on the map).
    /// </summary>
    West
}
