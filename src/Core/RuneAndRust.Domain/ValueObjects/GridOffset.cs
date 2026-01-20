namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a relative position offset for spawn positioning within monster groups (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// GridOffset is used to specify relative spawn positions for group members
/// in a <see cref="Definitions.GroupMember"/>. When a monster group is spawned
/// at a center position, each member's actual position is calculated by applying
/// their preferred offset.
/// </para>
/// <para>
/// Offsets use the same coordinate system as <see cref="GridPosition"/>:
/// <list type="bullet">
///   <item><description>Positive <see cref="DeltaX"/> moves East (rightward)</description></item>
///   <item><description>Negative <see cref="DeltaX"/> moves West (leftward)</description></item>
///   <item><description>Positive <see cref="DeltaY"/> moves South (downward)</description></item>
///   <item><description>Negative <see cref="DeltaY"/> moves North (upward)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define spawn positions relative to group center
/// var leaderOffset = GridOffset.Zero; // Leader at center
/// var flankerLeft = new GridOffset(-1, 0); // One cell west
/// var flankerRight = new GridOffset(1, 0); // One cell east
/// var rearGuard = new GridOffset(0, 1); // One cell south
///
/// // Apply offset to spawn position
/// var spawnCenter = new GridPosition(5, 5);
/// var leaderPosition = leaderOffset.Apply(spawnCenter); // (5, 5)
/// var leftPosition = flankerLeft.Apply(spawnCenter); // (4, 5)
/// </code>
/// </example>
public readonly record struct GridOffset
{
    /// <summary>
    /// Gets the horizontal offset (positive = East, negative = West).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A value of 0 means no horizontal displacement from the center.
    /// A value of 1 means one cell to the right (East).
    /// A value of -1 means one cell to the left (West).
    /// </para>
    /// </remarks>
    public int DeltaX { get; }

    /// <summary>
    /// Gets the vertical offset (positive = South, negative = North).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A value of 0 means no vertical displacement from the center.
    /// A value of 1 means one cell down (South).
    /// A value of -1 means one cell up (North).
    /// </para>
    /// </remarks>
    public int DeltaY { get; }

    /// <summary>
    /// Creates a new grid offset with the specified displacements.
    /// </summary>
    /// <param name="deltaX">The horizontal offset (positive = East, negative = West).</param>
    /// <param name="deltaY">The vertical offset (positive = South, negative = North).</param>
    /// <example>
    /// <code>
    /// var offset = new GridOffset(-1, 2); // 1 cell West, 2 cells South
    /// </code>
    /// </example>
    public GridOffset(int deltaX, int deltaY)
    {
        DeltaX = deltaX;
        DeltaY = deltaY;
    }

    /// <summary>
    /// Applies this offset to a center position, returning the resulting position.
    /// </summary>
    /// <param name="center">The center position to offset from.</param>
    /// <returns>A new <see cref="GridPosition"/> with the offset applied.</returns>
    /// <remarks>
    /// <para>
    /// This method does not validate whether the resulting position is within
    /// grid bounds. The caller is responsible for boundary checking.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var center = new GridPosition(5, 5);
    /// var offset = new GridOffset(2, -1);
    /// var result = offset.Apply(center); // GridPosition(7, 4)
    /// </code>
    /// </example>
    public GridPosition Apply(GridPosition center)
    {
        return new GridPosition(center.X + DeltaX, center.Y + DeltaY);
    }

    /// <summary>
    /// Calculates the Chebyshev magnitude of this offset.
    /// </summary>
    /// <returns>The maximum of the absolute X and Y offsets.</returns>
    /// <remarks>
    /// <para>
    /// This represents the minimum number of grid moves (including diagonal)
    /// required to travel this offset distance.
    /// </para>
    /// </remarks>
    public int Magnitude => Math.Max(Math.Abs(DeltaX), Math.Abs(DeltaY));

    /// <summary>
    /// Calculates the Manhattan magnitude of this offset.
    /// </summary>
    /// <returns>The sum of the absolute X and Y offsets.</returns>
    /// <remarks>
    /// <para>
    /// This represents the number of cardinal (non-diagonal) moves
    /// required to travel this offset distance.
    /// </para>
    /// </remarks>
    public int ManhattanMagnitude => Math.Abs(DeltaX) + Math.Abs(DeltaY);

    /// <summary>
    /// Gets whether this offset represents no displacement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns true when both <see cref="DeltaX"/> and <see cref="DeltaY"/> are zero.
    /// </para>
    /// </remarks>
    public bool IsZero => DeltaX == 0 && DeltaY == 0;

    /// <summary>
    /// Returns the negation of this offset (reverses direction).
    /// </summary>
    /// <returns>A new GridOffset with negated X and Y values.</returns>
    /// <example>
    /// <code>
    /// var offset = new GridOffset(2, -1);
    /// var negated = offset.Negate(); // GridOffset(-2, 1)
    /// </code>
    /// </example>
    public GridOffset Negate() => new(-DeltaX, -DeltaY);

    /// <summary>
    /// Adds two offsets together.
    /// </summary>
    /// <param name="other">The offset to add.</param>
    /// <returns>A new GridOffset with combined displacements.</returns>
    public GridOffset Add(GridOffset other) =>
        new(DeltaX + other.DeltaX, DeltaY + other.DeltaY);

    /// <summary>
    /// Returns the display notation for this offset (e.g., "(+2, -1)").
    /// </summary>
    /// <returns>A string showing signed X and Y displacements.</returns>
    public override string ToString()
    {
        var xSign = DeltaX >= 0 ? "+" : "";
        var ySign = DeltaY >= 0 ? "+" : "";
        return $"({xSign}{DeltaX}, {ySign}{DeltaY})";
    }

    /// <summary>
    /// Gets the zero offset (no displacement).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this for group members that should spawn at the center position.
    /// </para>
    /// </remarks>
    public static GridOffset Zero => new(0, 0);

    /// <summary>
    /// Creates an offset to the North (up).
    /// </summary>
    /// <param name="cells">Number of cells to offset (default: 1).</param>
    /// <returns>A GridOffset moving northward.</returns>
    public static GridOffset North(int cells = 1) => new(0, -cells);

    /// <summary>
    /// Creates an offset to the South (down).
    /// </summary>
    /// <param name="cells">Number of cells to offset (default: 1).</param>
    /// <returns>A GridOffset moving southward.</returns>
    public static GridOffset South(int cells = 1) => new(0, cells);

    /// <summary>
    /// Creates an offset to the East (right).
    /// </summary>
    /// <param name="cells">Number of cells to offset (default: 1).</param>
    /// <returns>A GridOffset moving eastward.</returns>
    public static GridOffset East(int cells = 1) => new(cells, 0);

    /// <summary>
    /// Creates an offset to the West (left).
    /// </summary>
    /// <param name="cells">Number of cells to offset (default: 1).</param>
    /// <returns>A GridOffset moving westward.</returns>
    public static GridOffset West(int cells = 1) => new(-cells, 0);
}
