using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Extensions;

/// <summary>
/// Extension methods for <see cref="FacingDirection"/>.
/// </summary>
/// <remarks>
/// Provides utility methods for direction calculations including:
/// <list type="bullet">
/// <item><description>Opposite direction lookup</description></item>
/// <item><description>Direction calculation between positions</description></item>
/// <item><description>Positional relationship checks (behind, side)</description></item>
/// <item><description>Unicode arrow character mapping</description></item>
/// </list>
/// </remarks>
public static class FacingDirectionExtensions
{
    /// <summary>
    /// Gets the opposite direction (180 degrees).
    /// </summary>
    /// <param name="direction">The direction to get the opposite of.</param>
    /// <returns>The direction 180 degrees from the input.</returns>
    /// <example>
    /// <code>
    /// FacingDirection.North.GetOpposite() // Returns South
    /// FacingDirection.NorthEast.GetOpposite() // Returns SouthWest
    /// </code>
    /// </example>
    public static FacingDirection GetOpposite(this FacingDirection direction) =>
        (FacingDirection)(((int)direction + 4) % 8);

    /// <summary>
    /// Gets the direction from one position to another.
    /// </summary>
    /// <param name="from">The origin position.</param>
    /// <param name="to">The target position.</param>
    /// <returns>The facing direction from origin to target.</returns>
    /// <remarks>
    /// Normalizes the delta to -1, 0, or 1 for each axis, then maps
    /// to the corresponding direction. Returns North if positions are identical.
    /// </remarks>
    public static FacingDirection GetDirectionTo(GridPosition from, GridPosition to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        // Normalize to -1, 0, 1
        var nx = dx == 0 ? 0 : dx / Math.Abs(dx);
        var ny = dy == 0 ? 0 : dy / Math.Abs(dy);

        return (nx, ny) switch
        {
            (0, -1) => FacingDirection.North,
            (1, -1) => FacingDirection.NorthEast,
            (1, 0) => FacingDirection.East,
            (1, 1) => FacingDirection.SouthEast,
            (0, 1) => FacingDirection.South,
            (-1, 1) => FacingDirection.SouthWest,
            (-1, 0) => FacingDirection.West,
            (-1, -1) => FacingDirection.NorthWest,
            _ => FacingDirection.North // Same position defaults to North
        };
    }

    /// <summary>
    /// Checks if two directions are opposite (180 degrees apart).
    /// </summary>
    /// <param name="direction">The first direction.</param>
    /// <param name="other">The second direction to compare.</param>
    /// <returns><c>true</c> if the directions are opposite; otherwise, <c>false</c>.</returns>
    public static bool IsOpposite(this FacingDirection direction, FacingDirection other) =>
        direction.GetOpposite() == other;

    /// <summary>
    /// Checks if a direction is adjacent (within 45 degrees).
    /// </summary>
    /// <param name="direction">The first direction.</param>
    /// <param name="other">The second direction to compare.</param>
    /// <returns><c>true</c> if the directions are within 45 degrees; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Two directions are adjacent if they differ by exactly 1 step
    /// in the 8-direction compass, accounting for wrap-around (N/NW).
    /// </remarks>
    public static bool IsAdjacent(this FacingDirection direction, FacingDirection other)
    {
        var diff = Math.Abs((int)direction - (int)other);
        return diff == 1 || diff == 7; // Wrap-around case (N <-> NW)
    }

    /// <summary>
    /// Checks if an attacker position is behind a target relative to target's facing.
    /// </summary>
    /// <param name="attackerPosition">The attacker's position.</param>
    /// <param name="targetPosition">The target's position.</param>
    /// <param name="targetFacing">The direction the target is facing.</param>
    /// <returns><c>true</c> if the attacker is behind the target; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// "Behind" means the attacker is in the direction opposite to where the target is facing.
    /// For example, if target faces North, an attacker to the South is behind.
    /// </remarks>
    public static bool IsBehind(GridPosition attackerPosition, GridPosition targetPosition, FacingDirection targetFacing)
    {
        var attackDirection = GetDirectionTo(targetPosition, attackerPosition);
        return attackDirection.IsOpposite(targetFacing);
    }

    /// <summary>
    /// Checks if an attacker position is to the side of a target relative to target's facing.
    /// </summary>
    /// <param name="attackerPosition">The attacker's position.</param>
    /// <param name="targetPosition">The target's position.</param>
    /// <param name="targetFacing">The direction the target is facing.</param>
    /// <returns><c>true</c> if the attacker is on the side (90 degrees); otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// "Side" means the attacker is perpendicular to the target's facing (90 degrees left or right).
    /// </remarks>
    public static bool IsSide(GridPosition attackerPosition, GridPosition targetPosition, FacingDirection targetFacing)
    {
        var attackDirection = GetDirectionTo(targetPosition, attackerPosition);
        var facing = (int)targetFacing;
        var attack = (int)attackDirection;
        var diff = Math.Abs(facing - attack);
        return diff == 2 || diff == 6; // 90 degrees (2 steps) on either side
    }

    /// <summary>
    /// Gets the Unicode arrow character for a facing direction.
    /// </summary>
    /// <param name="direction">The facing direction.</param>
    /// <returns>A Unicode arrow character representing the direction.</returns>
    /// <example>
    /// <code>
    /// FacingDirection.North.GetArrow() // Returns '↑'
    /// FacingDirection.SouthEast.GetArrow() // Returns '↘'
    /// </code>
    /// </example>
    public static char GetArrow(this FacingDirection direction) =>
        direction switch
        {
            FacingDirection.North => '↑',
            FacingDirection.NorthEast => '↗',
            FacingDirection.East => '→',
            FacingDirection.SouthEast => '↘',
            FacingDirection.South => '↓',
            FacingDirection.SouthWest => '↙',
            FacingDirection.West => '←',
            FacingDirection.NorthWest => '↖',
            _ => '·'
        };
}
