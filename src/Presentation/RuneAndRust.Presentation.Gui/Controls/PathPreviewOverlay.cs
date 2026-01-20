namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Displays a path preview showing the movement path from origin to target on hover.
/// </summary>
/// <remarks>
/// <para>
/// The overlay renders:
/// <list type="bullet">
///   <item><description>Dotted line connecting path nodes</description></item>
///   <item><description>Movement cost display</description></item>
///   <item><description>Blocked path indicator (red) if path is invalid</description></item>
/// </list>
/// </para>
/// </remarks>
public class PathPreviewOverlay : Control
{
    /// <summary>
    /// Default cell size in pixels for rendering.
    /// </summary>
    public const double DefaultCellSize = 48;

    /// <summary>
    /// Defines the <see cref="PathNodes"/> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<GridPosition>?> PathNodesProperty =
        AvaloniaProperty.Register<PathPreviewOverlay, IReadOnlyList<GridPosition>?>(nameof(PathNodes));

    /// <summary>
    /// Defines the <see cref="TotalCost"/> property.
    /// </summary>
    public static readonly StyledProperty<int> TotalCostProperty =
        AvaloniaProperty.Register<PathPreviewOverlay, int>(nameof(TotalCost));

    /// <summary>
    /// Defines the <see cref="IsBlocked"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBlockedProperty =
        AvaloniaProperty.Register<PathPreviewOverlay, bool>(nameof(IsBlocked));

    /// <summary>
    /// Defines the <see cref="CellSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CellSizeProperty =
        AvaloniaProperty.Register<PathPreviewOverlay, double>(nameof(CellSize), DefaultCellSize);

    /// <summary>
    /// Gets or sets the ordered list of path positions.
    /// </summary>
    public IReadOnlyList<GridPosition>? PathNodes
    {
        get => GetValue(PathNodesProperty);
        set => SetValue(PathNodesProperty, value);
    }

    /// <summary>
    /// Gets or sets the total movement cost.
    /// </summary>
    public int TotalCost
    {
        get => GetValue(TotalCostProperty);
        set => SetValue(TotalCostProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the path is blocked.
    /// </summary>
    public bool IsBlocked
    {
        get => GetValue(IsBlockedProperty);
        set => SetValue(IsBlockedProperty, value);
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
    /// Gets the line color based on blocked state.
    /// </summary>
    public Color PathColor => IsBlocked ? Colors.OrangeRed : Colors.LimeGreen;

    static PathPreviewOverlay()
    {
        AffectsRender<PathPreviewOverlay>(
            PathNodesProperty,
            TotalCostProperty,
            IsBlockedProperty,
            CellSizeProperty);
    }

    /// <summary>
    /// Renders the path preview with dotted line and cost display.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (PathNodes is null || PathNodes.Count < 2)
            return;

        var pen = new Pen(new SolidColorBrush(PathColor), 2, lineCap: PenLineCap.Round)
        {
            DashStyle = new DashStyle(new double[] { 4, 4 }, 0)
        };

        // Draw path lines
        for (var i = 0; i < PathNodes.Count - 1; i++)
        {
            var from = GetCellCenter(PathNodes[i]);
            var to = GetCellCenter(PathNodes[i + 1]);
            context.DrawLine(pen, from, to);
        }

        // Draw path nodes
        var nodeBrush = new SolidColorBrush(PathColor);
        foreach (var node in PathNodes)
        {
            var center = GetCellCenter(node);
            context.DrawEllipse(nodeBrush, null, center, 4, 4);
        }

        // Draw cost label at destination
        if (PathNodes.Count > 0)
        {
            var lastNode = PathNodes[^1];
            var costText = new FormattedText(
                $"Cost: {TotalCost}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI", FontStyle.Normal, FontWeight.SemiBold),
                12,
                new SolidColorBrush(Colors.White));

            var costPos = GetCellCenter(lastNode);
            context.DrawText(costText, new Point(costPos.X - costText.Width / 2, costPos.Y + CellSize / 4));
        }
    }

    /// <summary>
    /// Gets the center point of a cell at the given position.
    /// </summary>
    /// <param name="pos">Grid position.</param>
    /// <returns>Center point in screen coordinates.</returns>
    private Point GetCellCenter(GridPosition pos) =>
        new(pos.X * CellSize + CellSize / 2, pos.Y * CellSize + CellSize / 2);
}
