namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a 2D position in the dungeon grid.
/// </summary>
/// <remarks>
/// Position is an immutable value object used to track locations in the dungeon.
/// Each room has a position, and the player's position updates as they move between rooms.
/// </remarks>
/// <param name="X">The horizontal coordinate (positive = east, negative = west).</param>
/// <param name="Y">The vertical coordinate (positive = north, negative = south).</param>
public readonly record struct Position(int X, int Y)
{
    /// <summary>
    /// Gets the origin position (0, 0), typically the starting room location.
    /// </summary>
    public static Position Origin => new(0, 0);

    /// <summary>
    /// Creates a new position by moving from the current position.
    /// </summary>
    /// <param name="deltaX">The horizontal movement (positive = east, negative = west).</param>
    /// <param name="deltaY">The vertical movement (positive = north, negative = south).</param>
    /// <returns>A new Position at the resulting coordinates.</returns>
    public Position Move(int deltaX, int deltaY) => new(X + deltaX, Y + deltaY);

    /// <summary>
    /// Returns a string representation of this position.
    /// </summary>
    /// <returns>A formatted string showing the X and Y coordinates.</returns>
    public override string ToString() => $"({X}, {Y})";
}
