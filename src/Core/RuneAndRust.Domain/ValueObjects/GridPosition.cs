using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a position on the combat grid using X,Y coordinates.
/// </summary>
/// <remarks>
/// <para>
/// GridPosition uses a 0-indexed coordinate system where:
/// - X represents the column (0 = leftmost, increases rightward)
/// - Y represents the row (0 = topmost, increases downward)
/// </para>
/// <para>
/// For display purposes, positions use chess-like notation (A1, B3, H8) where:
/// - Column letters start at 'A' for X=0
/// - Row numbers start at 1 for Y=0
/// </para>
/// <para>
/// Distance calculations support both Chebyshev distance (allowing diagonal movement
/// to count as 1) and Manhattan distance (only cardinal movement).
/// </para>
/// </remarks>
public readonly record struct GridPosition
{
    /// <summary>
    /// Gets the X coordinate (column index, 0-based).
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the Y coordinate (row index, 0-based).
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Creates a new grid position at the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate (column).</param>
    /// <param name="y">The Y coordinate (row).</param>
    public GridPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Returns a new position one cell away in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to move.</param>
    /// <returns>A new GridPosition offset by one cell in the given direction.</returns>
    /// <remarks>
    /// This method does not validate whether the resulting position is within
    /// grid bounds. The caller is responsible for boundary checking.
    /// </remarks>
    public GridPosition Move(MovementDirection direction) => direction switch
    {
        MovementDirection.North => new GridPosition(X, Y - 1),
        MovementDirection.South => new GridPosition(X, Y + 1),
        MovementDirection.East => new GridPosition(X + 1, Y),
        MovementDirection.West => new GridPosition(X - 1, Y),
        MovementDirection.NorthEast => new GridPosition(X + 1, Y - 1),
        MovementDirection.NorthWest => new GridPosition(X - 1, Y - 1),
        MovementDirection.SouthEast => new GridPosition(X + 1, Y + 1),
        MovementDirection.SouthWest => new GridPosition(X - 1, Y + 1),
        _ => this
    };

    /// <summary>
    /// Calculates the Chebyshev distance to another position.
    /// </summary>
    /// <param name="other">The target position.</param>
    /// <returns>
    /// The minimum number of moves required to reach the target,
    /// allowing diagonal movement.
    /// </returns>
    /// <remarks>
    /// Chebyshev distance treats diagonal moves as equivalent to cardinal moves,
    /// making it ideal for grid-based games with 8-directional movement.
    /// </remarks>
    public int DistanceTo(GridPosition other) =>
        Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

    /// <summary>
    /// Calculates the Manhattan distance to another position.
    /// </summary>
    /// <param name="other">The target position.</param>
    /// <returns>
    /// The sum of horizontal and vertical distances,
    /// as if only cardinal movement were allowed.
    /// </returns>
    public int ManhattanDistanceTo(GridPosition other) =>
        Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

    /// <summary>
    /// Determines whether this position is adjacent to another position.
    /// </summary>
    /// <param name="other">The position to check.</param>
    /// <returns>
    /// <c>true</c> if the positions are exactly one cell apart
    /// (including diagonals); otherwise, <c>false</c>.
    /// </returns>
    public bool IsAdjacentTo(GridPosition other) => DistanceTo(other) == 1;

    /// <summary>
    /// Returns the display notation for this position (e.g., "A1", "H8").
    /// </summary>
    /// <returns>A string in column-letter/row-number format.</returns>
    public override string ToString()
    {
        var column = (char)('A' + X);
        var row = Y + 1;
        return $"{column}{row}";
    }

    /// <summary>
    /// Attempts to parse a position from chess-like notation.
    /// </summary>
    /// <param name="input">The input string (e.g., "A1", "H8").</param>
    /// <param name="position">When successful, contains the parsed position.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Valid notation consists of a column letter (A-Z) followed by a row number (1+).
    /// Parsing is case-insensitive for the column letter.
    /// </remarks>
    public static bool TryParse(string input, out GridPosition position)
    {
        position = default;

        if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            return false;

        var column = char.ToUpperInvariant(input[0]);
        if (column < 'A' || column > 'Z')
            return false;

        if (!int.TryParse(input[1..], out var row) || row < 1)
            return false;

        position = new GridPosition(column - 'A', row - 1);
        return true;
    }

    /// <summary>
    /// Creates a position from chess-like notation.
    /// </summary>
    /// <param name="notation">The notation string (e.g., "A1", "H8").</param>
    /// <returns>The parsed GridPosition.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the notation is invalid.
    /// </exception>
    public static GridPosition FromNotation(string notation)
    {
        if (!TryParse(notation, out var position))
            throw new ArgumentException($"Invalid grid notation: {notation}", nameof(notation));
        return position;
    }

    /// <summary>
    /// Gets the origin position (0, 0).
    /// </summary>
    public static GridPosition Origin => new(0, 0);
}
