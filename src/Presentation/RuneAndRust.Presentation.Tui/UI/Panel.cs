namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Represents a rectangular panel in the screen layout.
/// </summary>
/// <param name="Position">The logical position of this panel.</param>
/// <param name="X">The left column (0-indexed).</param>
/// <param name="Y">The top row (0-indexed).</param>
/// <param name="Width">The width in characters.</param>
/// <param name="Height">The height in rows.</param>
/// <param name="Title">Optional title to display in the border.</param>
/// <param name="HasBorder">Whether to draw a border around the panel.</param>
public readonly record struct Panel(
    PanelPosition Position,
    int X,
    int Y,
    int Width,
    int Height,
    string? Title = null,
    bool HasBorder = true)
{
    /// <summary>
    /// Gets the inner content area, accounting for borders.
    /// </summary>
    /// <returns>Tuple of (X, Y, Width, Height) for content area.</returns>
    public (int X, int Y, int Width, int Height) ContentArea =>
        HasBorder 
            ? (X + 1, Y + 1, Math.Max(0, Width - 2), Math.Max(0, Height - 2))
            : (X, Y, Width, Height);
    
    /// <summary>
    /// Gets the number of content lines that fit in this panel.
    /// </summary>
    public int ContentLines => ContentArea.Height;
    
    /// <summary>
    /// Gets the width available for content text.
    /// </summary>
    public int ContentWidth => ContentArea.Width;
    
    /// <summary>
    /// Checks if a point is within the content area.
    /// </summary>
    /// <param name="x">The x coordinate to check.</param>
    /// <param name="y">The y coordinate to check.</param>
    /// <returns><c>true</c> if point is inside content area; otherwise, <c>false</c>.</returns>
    public bool ContainsPoint(int x, int y)
    {
        var content = ContentArea;
        return x >= content.X && x < content.X + content.Width &&
               y >= content.Y && y < content.Y + content.Height;
    }
    
    /// <summary>
    /// Creates an empty panel at a given position.
    /// </summary>
    public static Panel Empty(PanelPosition position) =>
        new(position, 0, 0, 0, 0, null, false);
}
