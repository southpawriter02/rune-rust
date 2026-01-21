// ═══════════════════════════════════════════════════════════════════════════════
// GridUtilities.cs
// Shared grid coordinate utilities for TUI and GUI presentation layers.
// Version: 0.13.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Shared.Utilities;

/// <summary>
/// Provides static utility methods for grid coordinate calculations used
/// consistently across TUI and GUI presentation layers.
/// </summary>
/// <remarks>
/// <para>
/// This utility class centralizes grid math operations that were previously
/// duplicated across combat grid components. By consolidating these methods,
/// we ensure consistent coordinate handling throughout the application.
/// </para>
/// <para>Calculation categories include:</para>
/// <list type="bullet">
///   <item><description>Position conversion between linear indices and (x, y) coordinates</description></item>
///   <item><description>Position validation for bounds checking</description></item>
///   <item><description>Adjacent position enumeration for movement and targeting</description></item>
///   <item><description>Distance calculations (Manhattan and Chebyshev)</description></item>
///   <item><description>Direction determination between positions</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Convert linear index to grid position
/// var (x, y) = GridUtilities.CalculateGridPosition(12, 5); // (2, 2)
/// 
/// // Check if position is valid
/// var isValid = GridUtilities.IsValidPosition(3, 4, 8, 8); // true
/// 
/// // Get adjacent cells for movement
/// var adjacent = GridUtilities.GetAdjacentPositions(4, 4).ToList();
/// </code>
/// </example>
public static class GridUtilities
{
    // ═══════════════════════════════════════════════════════════════════════════
    // POSITION CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converts a linear index to grid coordinates.
    /// </summary>
    /// <param name="index">The linear index (0-based).</param>
    /// <param name="gridWidth">The width of the grid in cells.</param>
    /// <returns>
    /// A tuple containing the (X, Y) grid coordinates.
    /// X = index % gridWidth (column), Y = index / gridWidth (row).
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="gridWidth"/> is zero or negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // For a 5-wide grid:
    /// GridUtilities.CalculateGridPosition(0, 5);   // (0, 0)
    /// GridUtilities.CalculateGridPosition(4, 5);   // (4, 0)
    /// GridUtilities.CalculateGridPosition(5, 5);   // (0, 1)
    /// GridUtilities.CalculateGridPosition(12, 5);  // (2, 2)
    /// </code>
    /// </example>
    public static (int X, int Y) CalculateGridPosition(int index, int gridWidth)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(gridWidth, 0, nameof(gridWidth));

        return (index % gridWidth, index / gridWidth);
    }

    /// <summary>
    /// Converts grid coordinates to a linear index.
    /// </summary>
    /// <param name="x">The X coordinate (column, 0-based).</param>
    /// <param name="y">The Y coordinate (row, 0-based).</param>
    /// <param name="gridWidth">The width of the grid in cells.</param>
    /// <returns>
    /// The linear index calculated as (y * gridWidth + x).
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="gridWidth"/> is zero or negative.
    /// </exception>
    /// <example>
    /// <code>
    /// GridUtilities.CalculateLinearIndex(0, 0, 5);   // 0
    /// GridUtilities.CalculateLinearIndex(4, 0, 5);   // 4
    /// GridUtilities.CalculateLinearIndex(0, 1, 5);   // 5
    /// GridUtilities.CalculateLinearIndex(2, 2, 5);   // 12
    /// </code>
    /// </example>
    public static int CalculateLinearIndex(int x, int y, int gridWidth)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(gridWidth, 0, nameof(gridWidth));

        return y * gridWidth + x;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // POSITION VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if coordinates are within grid bounds.
    /// </summary>
    /// <param name="x">The X coordinate (column) to validate.</param>
    /// <param name="y">The Y coordinate (row) to validate.</param>
    /// <param name="width">The grid width (number of columns).</param>
    /// <param name="height">The grid height (number of rows).</param>
    /// <returns>
    /// <c>true</c> if 0 ≤ x &lt; width AND 0 ≤ y &lt; height; otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// GridUtilities.IsValidPosition(0, 0, 8, 8);    // true (top-left corner)
    /// GridUtilities.IsValidPosition(7, 7, 8, 8);    // true (bottom-right corner)
    /// GridUtilities.IsValidPosition(8, 0, 8, 8);    // false (x out of bounds)
    /// GridUtilities.IsValidPosition(-1, 0, 8, 8);   // false (negative x)
    /// </code>
    /// </example>
    public static bool IsValidPosition(int x, int y, int width, int height)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    /// <summary>
    /// Checks if a position tuple is within grid bounds.
    /// </summary>
    /// <param name="position">The (X, Y) position tuple to validate.</param>
    /// <param name="width">The grid width (number of columns).</param>
    /// <param name="height">The grid height (number of rows).</param>
    /// <returns>
    /// <c>true</c> if the position is within bounds; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidPosition((int X, int Y) position, int width, int height)
    {
        return IsValidPosition(position.X, position.Y, width, height);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ADJACENT POSITIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the four cardinal adjacent positions (no diagonals).
    /// </summary>
    /// <param name="x">The X coordinate of the center cell.</param>
    /// <param name="y">The Y coordinate of the center cell.</param>
    /// <returns>
    /// An enumerable of the four adjacent positions: left, right, up, down.
    /// Does NOT perform bounds checking - use <see cref="GetValidAdjacentPositions"/>
    /// for bounded results.
    /// </returns>
    /// <example>
    /// <code>
    /// var adjacent = GridUtilities.GetAdjacentPositions(4, 4).ToList();
    /// // Returns: (3, 4), (5, 4), (4, 3), (4, 5)
    /// </code>
    /// </example>
    public static IEnumerable<(int X, int Y)> GetAdjacentPositions(int x, int y)
    {
        yield return (x - 1, y);  // Left
        yield return (x + 1, y);  // Right
        yield return (x, y - 1);  // Up
        yield return (x, y + 1);  // Down
    }

    /// <summary>
    /// Gets all eight adjacent positions (including diagonals).
    /// </summary>
    /// <param name="x">The X coordinate of the center cell.</param>
    /// <param name="y">The Y coordinate of the center cell.</param>
    /// <returns>
    /// An enumerable of all eight adjacent positions.
    /// Does NOT perform bounds checking.
    /// </returns>
    /// <example>
    /// <code>
    /// var all = GridUtilities.GetAllAdjacentPositions(4, 4).ToList();
    /// // Returns 8 positions including diagonals
    /// </code>
    /// </example>
    public static IEnumerable<(int X, int Y)> GetAllAdjacentPositions(int x, int y)
    {
        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip center
                yield return (x + dx, y + dy);
            }
        }
    }

    /// <summary>
    /// Gets adjacent positions that are within grid bounds.
    /// </summary>
    /// <param name="x">The X coordinate of the center cell.</param>
    /// <param name="y">The Y coordinate of the center cell.</param>
    /// <param name="width">The grid width for bounds checking.</param>
    /// <param name="height">The grid height for bounds checking.</param>
    /// <param name="includeDiagonals">
    /// If <c>true</c>, includes diagonal positions; if <c>false</c> (default),
    /// returns only cardinal directions.
    /// </param>
    /// <returns>
    /// An enumerable of adjacent positions that pass bounds validation.
    /// </returns>
    /// <example>
    /// <code>
    /// // Corner case: only 2 valid adjacent cells
    /// var corner = GridUtilities.GetValidAdjacentPositions(0, 0, 8, 8).ToList();
    /// // Returns: (1, 0), (0, 1)
    /// </code>
    /// </example>
    public static IEnumerable<(int X, int Y)> GetValidAdjacentPositions(
        int x, int y, int width, int height, bool includeDiagonals = false)
    {
        var positions = includeDiagonals
            ? GetAllAdjacentPositions(x, y)
            : GetAdjacentPositions(x, y);

        return positions.Where(p => IsValidPosition(p, width, height));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISTANCE CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the Manhattan distance (taxicab distance) between two positions.
    /// </summary>
    /// <param name="x1">X coordinate of the first position.</param>
    /// <param name="y1">Y coordinate of the first position.</param>
    /// <param name="x2">X coordinate of the second position.</param>
    /// <param name="y2">Y coordinate of the second position.</param>
    /// <returns>
    /// The Manhattan distance: |x2 - x1| + |y2 - y1|.
    /// This represents the number of orthogonal moves required.
    /// </returns>
    /// <remarks>
    /// Manhattan distance is used when diagonal movement is not allowed.
    /// </remarks>
    /// <example>
    /// <code>
    /// GridUtilities.CalculateManhattanDistance(0, 0, 3, 4);  // 7
    /// GridUtilities.CalculateManhattanDistance(5, 5, 5, 5);  // 0
    /// </code>
    /// </example>
    public static int CalculateManhattanDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
    }

    /// <summary>
    /// Calculates the Chebyshev distance (chessboard distance) between two positions.
    /// </summary>
    /// <param name="x1">X coordinate of the first position.</param>
    /// <param name="y1">Y coordinate of the first position.</param>
    /// <param name="x2">X coordinate of the second position.</param>
    /// <param name="y2">Y coordinate of the second position.</param>
    /// <returns>
    /// The Chebyshev distance: max(|x2 - x1|, |y2 - y1|).
    /// This represents the number of king moves on a chessboard.
    /// </returns>
    /// <remarks>
    /// Chebyshev distance is used when diagonal movement costs the same as orthogonal.
    /// </remarks>
    /// <example>
    /// <code>
    /// GridUtilities.CalculateChebyshevDistance(0, 0, 3, 4);  // 4
    /// GridUtilities.CalculateChebyshevDistance(5, 5, 5, 5);  // 0
    /// </code>
    /// </example>
    public static int CalculateChebyshevDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DIRECTION CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines the direction from one position to another.
    /// </summary>
    /// <param name="fromX">X coordinate of the origin.</param>
    /// <param name="fromY">Y coordinate of the origin.</param>
    /// <param name="toX">X coordinate of the destination.</param>
    /// <param name="toY">Y coordinate of the destination.</param>
    /// <returns>
    /// A <see cref="Direction"/> value indicating the direction.
    /// Returns <see cref="Direction.None"/> if the positions are identical.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The direction is determined by the sign of the delta values:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>(dx, dy)</term>
    ///     <description>Direction</description>
    ///   </listheader>
    ///   <item><term>(0, -1)</term><description>North</description></item>
    ///   <item><term>(1, 0)</term><description>East</description></item>
    ///   <item><term>(0, 1)</term><description>South</description></item>
    ///   <item><term>(-1, 0)</term><description>West</description></item>
    ///   <item><term>(1, -1)</term><description>NorthEast</description></item>
    ///   <item><term>(1, 1)</term><description>SouthEast</description></item>
    ///   <item><term>(-1, 1)</term><description>SouthWest</description></item>
    ///   <item><term>(-1, -1)</term><description>NorthWest</description></item>
    /// </list>
    /// </remarks>
    public static Direction GetDirection(int fromX, int fromY, int toX, int toY)
    {
        var dx = Math.Sign(toX - fromX);
        var dy = Math.Sign(toY - fromY);

        return (dx, dy) switch
        {
            (0, -1) => Direction.North,
            (1, -1) => Direction.NorthEast,
            (1, 0) => Direction.East,
            (1, 1) => Direction.SouthEast,
            (0, 1) => Direction.South,
            (-1, 1) => Direction.SouthWest,
            (-1, 0) => Direction.West,
            (-1, -1) => Direction.NorthWest,
            _ => Direction.None
        };
    }
}
