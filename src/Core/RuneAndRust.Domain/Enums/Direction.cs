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
    North,
    South,
    East,
    West,
    Up,
    Down,
    Northeast,
    Northwest,
    Southeast,
    Southwest
}
