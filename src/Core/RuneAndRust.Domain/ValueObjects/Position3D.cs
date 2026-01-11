using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a 3D position in the dungeon grid.
/// </summary>
/// <remarks>
/// Position3D is an immutable value object used to track locations across
/// multiple dungeon levels. The Z coordinate represents depth, where 0 is
/// the surface/entrance level and positive values indicate deeper underground.
/// </remarks>
/// <param name="X">The horizontal coordinate (positive = east, negative = west).</param>
/// <param name="Y">The vertical coordinate on the map (positive = north, negative = south).</param>
/// <param name="Z">The depth coordinate (0 = surface, positive = deeper underground).</param>
public readonly record struct Position3D(int X, int Y, int Z)
{
    /// <summary>
    /// Gets the origin position (0, 0, 0), typically the starting room location.
    /// </summary>
    public static Position3D Origin => new(0, 0, 0);

    /// <summary>
    /// Creates a new position by moving from the current position.
    /// </summary>
    /// <param name="deltaX">The horizontal movement (positive = east, negative = west).</param>
    /// <param name="deltaY">The map vertical movement (positive = north, negative = south).</param>
    /// <param name="deltaZ">The depth movement (positive = deeper, negative = shallower).</param>
    /// <returns>A new Position3D at the resulting coordinates.</returns>
    public Position3D Move(int deltaX, int deltaY, int deltaZ) =>
        new(X + deltaX, Y + deltaY, Z + deltaZ);

    /// <summary>
    /// Creates a new position one level up (toward the surface).
    /// </summary>
    /// <returns>A new Position3D at Z - 1.</returns>
    public Position3D MoveUp() => new(X, Y, Z - 1);

    /// <summary>
    /// Creates a new position one level down (deeper underground).
    /// </summary>
    /// <returns>A new Position3D at Z + 1.</returns>
    public Position3D MoveDown() => new(X, Y, Z + 1);

    /// <summary>
    /// Creates a new position by moving in a cardinal direction.
    /// </summary>
    /// <param name="direction">The direction to move.</param>
    /// <returns>A new Position3D at the resulting coordinates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when direction is invalid.</exception>
    public Position3D Move(Direction direction) => direction switch
    {
        Direction.North => new(X, Y + 1, Z),
        Direction.South => new(X, Y - 1, Z),
        Direction.East => new(X + 1, Y, Z),
        Direction.West => new(X - 1, Y, Z),
        Direction.Up => MoveUp(),
        Direction.Down => MoveDown(),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid direction")
    };

    /// <summary>
    /// Converts this 3D position to a 2D position (discarding Z).
    /// </summary>
    /// <returns>A Position with the same X and Y coordinates.</returns>
    public Position ToPosition2D() => new(X, Y);

    /// <summary>
    /// Creates a Position3D from a 2D position with the specified Z level.
    /// </summary>
    /// <param name="position">The 2D position.</param>
    /// <param name="z">The Z level (default 0 = surface).</param>
    /// <returns>A new Position3D with the specified coordinates.</returns>
    public static Position3D FromPosition2D(Position position, int z = 0) =>
        new(position.X, position.Y, z);

    /// <summary>
    /// Gets the Manhattan distance to another position.
    /// </summary>
    /// <param name="other">The other position.</param>
    /// <returns>The sum of absolute differences in X, Y, and Z.</returns>
    public int ManhattanDistanceTo(Position3D other) =>
        Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z);

    /// <summary>
    /// Returns a string representation of this position.
    /// </summary>
    /// <returns>A formatted string showing the X, Y, and Z coordinates.</returns>
    public override string ToString() => $"({X}, {Y}, Z={Z})";
}
