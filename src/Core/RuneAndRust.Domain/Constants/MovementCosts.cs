using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Constants;

/// <summary>
/// Constants and helpers for movement costs on the combat grid.
/// </summary>
/// <remarks>
/// <para>
/// Uses a point multiplier system to handle fractional costs cleanly.
/// </para>
/// <para>
/// <b>Cost System:</b>
/// <list type="bullet">
/// <item><description>Cardinal (N, S, E, W): 1 actual cost = 2 points</description></item>
/// <item><description>Diagonal (NE, NW, SE, SW): 1.5 actual cost = 3 points</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Speed Conversion:</b>
/// Speed 4 = 8 movement points per turn (4 cardinal moves or 2 diagonal + 1 cardinal)
/// </para>
/// </remarks>
public static class MovementCosts
{
    /// <summary>
    /// Base movement cost for cardinal directions (N, S, E, W).
    /// </summary>
    public const int Cardinal = 1;

    /// <summary>
    /// Base movement cost for diagonal directions.
    /// </summary>
    public const int Diagonal = 2;

    /// <summary>
    /// Movement point multiplier for clean fractional costs.
    /// </summary>
    /// <remarks>
    /// Points = Speed × Multiplier. Cardinal costs 2 pts, Diagonal costs 3 pts.
    /// </remarks>
    public const int PointMultiplier = 2;

    /// <summary>
    /// Gets the movement point cost for a direction.
    /// </summary>
    /// <param name="direction">The movement direction.</param>
    /// <returns>
    /// 2 for cardinal directions (N, S, E, W) or 3 for diagonal directions.
    /// </returns>
    public static int GetCost(MovementDirection direction) => direction switch
    {
        MovementDirection.North => Cardinal * PointMultiplier,     // 2
        MovementDirection.South => Cardinal * PointMultiplier,     // 2
        MovementDirection.East => Cardinal * PointMultiplier,      // 2
        MovementDirection.West => Cardinal * PointMultiplier,      // 2
        MovementDirection.NorthEast => Diagonal + 1,               // 3
        MovementDirection.NorthWest => Diagonal + 1,               // 3
        MovementDirection.SouthEast => Diagonal + 1,               // 3
        MovementDirection.SouthWest => Diagonal + 1,               // 3
        _ => Cardinal * PointMultiplier
    };

    /// <summary>
    /// Converts movement speed to movement points.
    /// </summary>
    /// <param name="speed">The movement speed stat.</param>
    /// <returns>Total movement points per turn.</returns>
    /// <example>
    /// Speed 4 → 8 points (4 × 2)
    /// </example>
    public static int SpeedToPoints(int speed) => speed * PointMultiplier;

    /// <summary>
    /// Converts movement points to display speed.
    /// </summary>
    /// <param name="points">Movement points.</param>
    /// <returns>Equivalent speed value.</returns>
    public static int PointsToSpeed(int points) => points / PointMultiplier;

    /// <summary>
    /// Gets human-readable display cost for a direction.
    /// </summary>
    /// <param name="direction">The movement direction.</param>
    /// <returns>"1" for cardinal, "1.5" for diagonal.</returns>
    public static string GetDisplayCost(MovementDirection direction) => direction switch
    {
        MovementDirection.NorthEast or MovementDirection.NorthWest or
        MovementDirection.SouthEast or MovementDirection.SouthWest => "1.5",
        _ => "1"
    };

    /// <summary>
    /// Checks if a direction is diagonal.
    /// </summary>
    /// <param name="direction">The direction to check.</param>
    /// <returns><c>true</c> if the direction is diagonal; otherwise, <c>false</c>.</returns>
    public static bool IsDiagonal(MovementDirection direction) => direction switch
    {
        MovementDirection.NorthEast or MovementDirection.NorthWest or
        MovementDirection.SouthEast or MovementDirection.SouthWest => true,
        _ => false
    };
}
