namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Cardinal and ordinal directions for creature facing.
/// </summary>
/// <remarks>
/// <para>
/// Directions are ordered clockwise starting from North (0).
/// This ordering enables efficient opposite direction calculation
/// via modular arithmetic: opposite = (direction + 4) % 8.
/// </para>
/// <para>
/// Grid coordinate mapping:
/// <list type="bullet">
/// <item><description>North: decreasing Y (up on grid)</description></item>
/// <item><description>East: increasing X (right on grid)</description></item>
/// <item><description>South: increasing Y (down on grid)</description></item>
/// <item><description>West: decreasing X (left on grid)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum FacingDirection
{
    /// <summary>North (up on grid, decreasing Y).</summary>
    North = 0,

    /// <summary>Northeast (up-right on grid).</summary>
    NorthEast = 1,

    /// <summary>East (right on grid, increasing X).</summary>
    East = 2,

    /// <summary>Southeast (down-right on grid).</summary>
    SouthEast = 3,

    /// <summary>South (down on grid, increasing Y).</summary>
    South = 4,

    /// <summary>Southwest (down-left on grid).</summary>
    SouthWest = 5,

    /// <summary>West (left on grid, decreasing X).</summary>
    West = 6,

    /// <summary>Northwest (up-left on grid).</summary>
    NorthWest = 7
}
