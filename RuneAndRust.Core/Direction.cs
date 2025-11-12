namespace RuneAndRust.Core;

/// <summary>
/// Compass directions for room navigation (v0.10)
/// </summary>
public enum Direction
{
    North,
    South,
    East,
    West
}

/// <summary>
/// Helper methods for Direction enum (v0.10)
/// </summary>
public static class DirectionExtensions
{
    /// <summary>
    /// Gets the opposite direction
    /// </summary>
    public static Direction GetOpposite(this Direction direction)
    {
        return direction switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => throw new ArgumentException($"Invalid direction: {direction}")
        };
    }

    /// <summary>
    /// Converts direction to lowercase string for room navigation
    /// </summary>
    public static string ToNavigationString(this Direction direction)
    {
        return direction.ToString().ToLower();
    }

    /// <summary>
    /// Parses a navigation string to Direction enum
    /// </summary>
    public static Direction? FromString(string directionString)
    {
        return directionString.ToLower() switch
        {
            "north" or "n" => Direction.North,
            "south" or "s" => Direction.South,
            "east" or "e" => Direction.East,
            "west" or "w" => Direction.West,
            _ => null
        };
    }

    /// <summary>
    /// Gets all cardinal directions
    /// </summary>
    public static Direction[] GetAllDirections()
    {
        return new[] { Direction.North, Direction.South, Direction.East, Direction.West };
    }
}
