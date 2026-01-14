namespace RuneAndRust.Presentation.Shared.DTOs;

/// <summary>
/// Options for rendering the combat grid.
/// </summary>
public class GridRenderOptions
{
    /// <summary>
    /// Whether to use box-drawing characters for grid lines.
    /// </summary>
    public bool UseBoxDrawing { get; init; } = true;

    /// <summary>
    /// Whether to show column (A-H) and row (1-8) coordinates.
    /// </summary>
    public bool ShowCoordinates { get; init; } = true;

    /// <summary>
    /// Whether to show the legend explaining symbols.
    /// </summary>
    public bool ShowLegend { get; init; } = true;

    /// <summary>
    /// Whether to highlight cells the player can move to (future use).
    /// </summary>
    public bool HighlightValidMoves { get; init; } = false;

    /// <summary>
    /// Use compact rendering without grid lines.
    /// </summary>
    public bool Compact { get; init; } = false;

    /// <summary>
    /// Current turn number for display (optional).
    /// </summary>
    public int? TurnNumber { get; init; }

    /// <summary>
    /// Whether it's currently the player's turn.
    /// </summary>
    public bool IsPlayerTurn { get; init; }

    /// <summary>
    /// Default rendering options (full grid with box drawing).
    /// </summary>
    public static GridRenderOptions Default => new();

    /// <summary>
    /// Compact rendering options (no grid lines).
    /// </summary>
    public static GridRenderOptions CompactDefault => new() { Compact = true };
}
