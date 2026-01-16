namespace RuneAndRust.Presentation.Gui.ViewModels;

using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// ViewModel for a connection between two rooms on the map.
/// </summary>
public partial class ConnectionViewModel : ViewModelBase
{
    /// <summary>Gets the starting point of the connection line.</summary>
    [ObservableProperty] private Point _startPoint;

    /// <summary>Gets the ending point of the connection line.</summary>
    [ObservableProperty] private Point _endPoint;

    /// <summary>Gets whether the connection is locked.</summary>
    [ObservableProperty] private bool _isLocked;

    /// <summary>Creates a connection ViewModel.</summary>
    public ConnectionViewModel(Point start, Point end, bool isLocked = false)
    {
        _startPoint = start;
        _endPoint = end;
        _isLocked = isLocked;
    }
}
