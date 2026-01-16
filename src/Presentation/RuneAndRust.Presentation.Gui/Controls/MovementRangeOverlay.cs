namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Displays movement range with gradient opacity (brighter near player, dimmer at edges).
/// </summary>
/// <remarks>
/// <para>
/// The overlay renders reachable positions with opacity based on distance from origin:
/// <list type="bullet">
///   <item><description>Adjacent cells (1 distance): 0.7 opacity</description></item>
///   <item><description>Mid-range cells: Interpolated opacity</description></item>
///   <item><description>Max distance cells: 0.2 opacity</description></item>
/// </list>
/// </para>
/// <para>
/// This visual gradient helps players understand movement costs at a glance.
/// </para>
/// </remarks>
public class MovementRangeOverlay : Control
{
    /// <summary>
    /// Default cell size in pixels for rendering.
    /// </summary>
    public const double DefaultCellSize = 48;

    /// <summary>
    /// Defines the <see cref="ReachablePositions"/> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<GridPosition>?> ReachablePositionsProperty =
        AvaloniaProperty.Register<MovementRangeOverlay, IReadOnlyList<GridPosition>?>(nameof(ReachablePositions));

    /// <summary>
    /// Defines the <see cref="Origin"/> property.
    /// </summary>
    public static readonly StyledProperty<GridPosition> OriginProperty =
        AvaloniaProperty.Register<MovementRangeOverlay, GridPosition>(nameof(Origin));

    /// <summary>
    /// Defines the <see cref="MaxDistance"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxDistanceProperty =
        AvaloniaProperty.Register<MovementRangeOverlay, int>(nameof(MaxDistance), 1);

    /// <summary>
    /// Defines the <see cref="CellSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CellSizeProperty =
        AvaloniaProperty.Register<MovementRangeOverlay, double>(nameof(CellSize), DefaultCellSize);

    /// <summary>
    /// Defines the <see cref="OverlayColor"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> OverlayColorProperty =
        AvaloniaProperty.Register<MovementRangeOverlay, Color>(nameof(OverlayColor), Colors.DeepSkyBlue);

    /// <summary>
    /// Gets or sets the list of reachable positions to highlight.
    /// </summary>
    public IReadOnlyList<GridPosition>? ReachablePositions
    {
        get => GetValue(ReachablePositionsProperty);
        set => SetValue(ReachablePositionsProperty, value);
    }

    /// <summary>
    /// Gets or sets the origin position (player's current location).
    /// </summary>
    public GridPosition Origin
    {
        get => GetValue(OriginProperty);
        set => SetValue(OriginProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum movement distance.
    /// </summary>
    public int MaxDistance
    {
        get => GetValue(MaxDistanceProperty);
        set => SetValue(MaxDistanceProperty, value);
    }

    /// <summary>
    /// Gets or sets the cell size in pixels.
    /// </summary>
    public double CellSize
    {
        get => GetValue(CellSizeProperty);
        set => SetValue(CellSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the overlay highlight color.
    /// </summary>
    public Color OverlayColor
    {
        get => GetValue(OverlayColorProperty);
        set => SetValue(OverlayColorProperty, value);
    }

    static MovementRangeOverlay()
    {
        AffectsRender<MovementRangeOverlay>(
            ReachablePositionsProperty,
            OriginProperty,
            MaxDistanceProperty,
            CellSizeProperty,
            OverlayColorProperty);
    }

    /// <summary>
    /// Renders the movement range overlay with gradient opacity.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (ReachablePositions is null || ReachablePositions.Count == 0)
            return;

        foreach (var pos in ReachablePositions)
        {
            var distance = CalculateDistance(Origin, pos);
            var opacity = CalculateOpacity(distance, MaxDistance);
            var brush = new SolidColorBrush(OverlayColor, opacity);
            var rect = GetCellRect(pos);
            context.FillRectangle(brush, rect);
        }
    }

    /// <summary>
    /// Calculates the opacity for a cell based on distance from origin.
    /// </summary>
    /// <param name="distance">Distance from origin in cells.</param>
    /// <param name="maxDistance">Maximum movement distance.</param>
    /// <returns>Opacity value between 0.2 (far) and 0.7 (near).</returns>
    /// <remarks>
    /// Formula: 0.7 - (0.5 * distance / maxDistance)
    /// This produces:
    /// - Distance 0: 0.7 (origin)
    /// - Distance max: 0.2 (edges)
    /// </remarks>
    public static double CalculateOpacity(int distance, int maxDistance)
    {
        if (maxDistance <= 0) return 0.5;
        var ratio = (double)distance / maxDistance;
        return Math.Max(0.2, 0.7 - (0.5 * ratio));
    }

    /// <summary>
    /// Calculates the Manhattan distance between two positions.
    /// </summary>
    /// <param name="a">First position.</param>
    /// <param name="b">Second position.</param>
    /// <returns>Manhattan distance (|dx| + |dy|).</returns>
    public static int CalculateDistance(GridPosition a, GridPosition b) =>
        Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    /// <summary>
    /// Gets the rectangle bounds for a cell at the given position.
    /// </summary>
    /// <param name="pos">Grid position.</param>
    /// <returns>Rectangle in screen coordinates.</returns>
    private Rect GetCellRect(GridPosition pos) =>
        new(pos.X * CellSize, pos.Y * CellSize, CellSize, CellSize);
}
