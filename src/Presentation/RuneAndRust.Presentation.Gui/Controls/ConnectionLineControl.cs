namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

/// <summary>
/// Renders a connection line between two room nodes on the map.
/// </summary>
public class ConnectionLineControl : Control
{
    public static readonly StyledProperty<Point> StartPointProperty =
        AvaloniaProperty.Register<ConnectionLineControl, Point>(nameof(StartPoint));

    public static readonly StyledProperty<Point> EndPointProperty =
        AvaloniaProperty.Register<ConnectionLineControl, Point>(nameof(EndPoint));

    public static readonly StyledProperty<bool> IsLockedProperty =
        AvaloniaProperty.Register<ConnectionLineControl, bool>(nameof(IsLocked), false);

    private static readonly IPen StandardPen = new Pen(Brushes.DimGray, 2);
    private static readonly IPen LockedPen = new Pen(Brushes.OrangeRed, 2,
        lineCap: PenLineCap.Round,
        dashStyle: new DashStyle(new[] { 4.0, 2.0 }, 0));

    /// <summary>Gets or sets the start point.</summary>
    public Point StartPoint
    {
        get => GetValue(StartPointProperty);
        set => SetValue(StartPointProperty, value);
    }

    /// <summary>Gets or sets the end point.</summary>
    public Point EndPoint
    {
        get => GetValue(EndPointProperty);
        set => SetValue(EndPointProperty, value);
    }

    /// <summary>Gets or sets whether the connection is locked.</summary>
    public bool IsLocked
    {
        get => GetValue(IsLockedProperty);
        set => SetValue(IsLockedProperty, value);
    }

    static ConnectionLineControl()
    {
        AffectsRender<ConnectionLineControl>(StartPointProperty, EndPointProperty, IsLockedProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var pen = IsLocked ? LockedPen : StandardPen;
        context.DrawLine(pen, StartPoint, EndPoint);
    }
}
