namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Dungeon map view panel with zoom and pan support.
/// </summary>
public partial class MapViewPanel : UserControl
{
    private bool _isPanning;
    private Point _panStart;
    private Point _panOffsetStart;

    /// <summary>Creates a new map view panel.</summary>
    public MapViewPanel()
    {
        InitializeComponent();
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _panStart = e.GetPosition(this);

            if (DataContext is MapViewPanelViewModel vm)
            {
                _panOffsetStart = vm.PanOffset;
            }

            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPanning && DataContext is MapViewPanelViewModel vm)
        {
            var currentPos = e.GetPosition(this);
            var delta = currentPos - _panStart;

            vm.PanOffset = new Point(
                _panOffsetStart.X + delta.X,
                _panOffsetStart.Y + delta.Y
            );
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPanning = false;
    }
}
