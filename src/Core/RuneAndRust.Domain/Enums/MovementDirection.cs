namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Directions for grid movement in tactical combat.
/// </summary>
/// <remarks>
/// <para>
/// The grid uses a coordinate system where:
/// - X increases to the East (right)
/// - Y increases to the South (down)
/// - Origin (0,0) is at the top-left corner
/// </para>
/// <para>
/// Cardinal directions (North, South, East, West) move one cell in a single axis.
/// Diagonal directions move one cell in both axes simultaneously.
/// </para>
/// </remarks>
public enum MovementDirection
{
    /// <summary>
    /// Up on the grid (Y decreases by 1).
    /// </summary>
    North,

    /// <summary>
    /// Down on the grid (Y increases by 1).
    /// </summary>
    South,

    /// <summary>
    /// Right on the grid (X increases by 1).
    /// </summary>
    East,

    /// <summary>
    /// Left on the grid (X decreases by 1).
    /// </summary>
    West,

    /// <summary>
    /// Up-right diagonal (X increases, Y decreases).
    /// </summary>
    NorthEast,

    /// <summary>
    /// Up-left diagonal (X decreases, Y decreases).
    /// </summary>
    NorthWest,

    /// <summary>
    /// Down-right diagonal (X increases, Y increases).
    /// </summary>
    SouthEast,

    /// <summary>
    /// Down-left diagonal (X decreases, Y increases).
    /// </summary>
    SouthWest
}
