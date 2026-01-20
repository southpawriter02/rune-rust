namespace RuneAndRust.Presentation.Gui.ViewModels;

using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the dungeon map view panel.
/// </summary>
public partial class MapViewPanelViewModel : ViewModelBase
{
    /// <summary>Room nodes on the map.</summary>
    [ObservableProperty] private ObservableCollection<RoomNodeViewModel> _rooms = [];

    /// <summary>Connections between rooms.</summary>
    [ObservableProperty] private ObservableCollection<ConnectionViewModel> _connections = [];

    /// <summary>Current room ID.</summary>
    [ObservableProperty] private Guid _currentRoomId;

    /// <summary>Zoom level (0.5 to 2.0).</summary>
    [ObservableProperty] private double _zoomLevel = 1.0;

    /// <summary>Pan offset for scrolling.</summary>
    [ObservableProperty] private Point _panOffset = new(0, 0);

    /// <summary>Whether the map is visible.</summary>
    [ObservableProperty] private bool _isVisible;

    private const double MinZoom = 0.5;
    private const double MaxZoom = 2.0;
    private const double ZoomStep = 0.25;

    /// <summary>Creates the map view panel ViewModel.</summary>
    public MapViewPanelViewModel()
    {
        LoadSampleData();
    }

    /// <summary>Zooms the map in.</summary>
    [RelayCommand]
    private void ZoomIn()
    {
        ZoomLevel = Math.Min(MaxZoom, ZoomLevel + ZoomStep);
        Log.Debug("Map zoomed in to {ZoomLevel}", ZoomLevel);
    }

    /// <summary>Zooms the map out.</summary>
    [RelayCommand]
    private void ZoomOut()
    {
        ZoomLevel = Math.Max(MinZoom, ZoomLevel - ZoomStep);
        Log.Debug("Map zoomed out to {ZoomLevel}", ZoomLevel);
    }

    /// <summary>Centers the map on the player's current room.</summary>
    [RelayCommand]
    private void CenterOnPlayer()
    {
        var currentRoom = Rooms.FirstOrDefault(r => r.RoomId == CurrentRoomId);
        if (currentRoom is not null)
        {
            PanOffset = new Point(-currentRoom.X + 200, -currentRoom.Y + 150);
            Log.Debug("Map centered on player at room {RoomId}", CurrentRoomId);
        }
    }

    /// <summary>Resets zoom to default.</summary>
    [RelayCommand]
    private void ResetZoom()
    {
        ZoomLevel = 1.0;
        PanOffset = new Point(0, 0);
    }

    /// <summary>Toggles map visibility.</summary>
    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;
        Log.Debug("Map visibility: {IsVisible}", IsVisible);
    }

    private void LoadSampleData()
    {
        // Sample dungeon layout for demonstration
        var entrance = new RoomNodeViewModel(Guid.NewGuid(), "Entrance Hall", "Standard", 100, 50, true, false);
        var hallway = new RoomNodeViewModel(Guid.NewGuid(), "Dark Hallway", "Standard", 100, 150, true, true);
        var armory = new RoomNodeViewModel(Guid.NewGuid(), "Old Armory", "Armory", 20, 150, true, false);
        var shop = new RoomNodeViewModel(Guid.NewGuid(), "Merchant's Corner", "Shop", 180, 150, true, false);
        var treasure = new RoomNodeViewModel(Guid.NewGuid(), "Treasure Vault", "Treasure", 100, 250, true, false);
        var bossRoom = new RoomNodeViewModel(Guid.NewGuid(), "???", "Boss", 100, 350, false, false);

        Rooms.Add(entrance);
        Rooms.Add(hallway);
        Rooms.Add(armory);
        Rooms.Add(shop);
        Rooms.Add(treasure);
        Rooms.Add(bossRoom);

        CurrentRoomId = hallway.RoomId;

        // Connections (center of rooms: +40 X, +25 Y)
        Connections.Add(new ConnectionViewModel(new Point(140, 75), new Point(140, 150)));
        Connections.Add(new ConnectionViewModel(new Point(100, 175), new Point(60, 175)));
        Connections.Add(new ConnectionViewModel(new Point(180, 175), new Point(220, 175)));
        Connections.Add(new ConnectionViewModel(new Point(140, 200), new Point(140, 250)));
        Connections.Add(new ConnectionViewModel(new Point(140, 300), new Point(140, 350), isLocked: true));

        Log.Information("Map loaded with {RoomCount} rooms", Rooms.Count);
    }
}
