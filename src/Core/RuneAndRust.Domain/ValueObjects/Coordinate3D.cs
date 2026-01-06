using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a 3D coordinate in the dungeon grid.
/// Used by the topology generator for node placement.
/// </summary>
public readonly record struct Coordinate3D(int X, int Y, int Z)
{
    public static Coordinate3D Origin => new(0, 0, 0);

    /// <summary>
    /// Returns a new coordinate moved in the specified direction.
    /// Only cardinal directions and Up/Down are supported for dungeon generation.
    /// </summary>
    public Coordinate3D Move(Direction direction) => direction switch
    {
        Direction.North => this with { Y = Y + 1 },
        Direction.South => this with { Y = Y - 1 },
        Direction.East => this with { X = X + 1 },
        Direction.West => this with { X = X - 1 },
        Direction.Up => this with { Z = Z + 1 },
        Direction.Down => this with { Z = Z - 1 },
        _ => this // Diagonal directions not supported in 3D grid
    };

    /// <summary>
    /// Calculates Manhattan distance to another coordinate.
    /// Used for pathfinding and distance calculations.
    /// </summary>
    public int ManhattanDistanceTo(Coordinate3D other)
    {
        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z);
    }

    /// <summary>
    /// Gets all adjacent coordinates (6 cardinal + vertical neighbors).
    /// </summary>
    public IEnumerable<Coordinate3D> GetAdjacentCoordinates()
    {
        yield return this with { X = X + 1 };
        yield return this with { X = X - 1 };
        yield return this with { Y = Y + 1 };
        yield return this with { Y = Y - 1 };
        yield return this with { Z = Z + 1 };
        yield return this with { Z = Z - 1 };
    }

    public override string ToString() => $"({X}, {Y}, {Z})";
}
