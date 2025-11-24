using Avalonia;
using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// Represents a room node on the minimap.
/// </summary>
public class RoomNodeViewModel : ViewModelBase
{
    /// <summary>
    /// The underlying room data.
    /// </summary>
    public Room Room { get; set; } = null!;

    /// <summary>
    /// Position on the minimap canvas.
    /// </summary>
    public Point Position { get; set; }

    /// <summary>
    /// Whether this is the current room the player is in.
    /// </summary>
    public bool IsCurrentRoom { get; set; }

    /// <summary>
    /// Whether this room has been explored.
    /// </summary>
    public bool IsExplored { get; set; } = true;

    /// <summary>
    /// Whether this room has an upward vertical connection.
    /// </summary>
    public bool HasUpConnection { get; set; }

    /// <summary>
    /// Whether this room has a downward vertical connection.
    /// </summary>
    public bool HasDownConnection { get; set; }

    /// <summary>
    /// Room type for styling (Start, Boss, Normal, etc.).
    /// </summary>
    public string RoomType { get; set; } = "Normal";

    /// <summary>
    /// Biome color for the room.
    /// </summary>
    public string BiomeColor { get; set; } = "#666666";

    /// <summary>
    /// Room name for tooltip.
    /// </summary>
    public string RoomName => Room?.Name ?? "Unknown";

    /// <summary>
    /// Whether the room is a sanctuary.
    /// </summary>
    public bool IsSanctuary => Room?.IsSanctuary ?? false;

    /// <summary>
    /// Whether the room has enemies.
    /// </summary>
    public bool HasEnemies => Room?.Enemies?.Count > 0 && !(Room?.HasBeenCleared ?? true);
}

/// <summary>
/// Represents a connection line between rooms on the minimap.
/// </summary>
public class ConnectionViewModel : ViewModelBase
{
    /// <summary>
    /// Start position of the connection line.
    /// </summary>
    public Point StartPos { get; set; }

    /// <summary>
    /// End position of the connection line.
    /// </summary>
    public Point EndPos { get; set; }

    /// <summary>
    /// Whether this is a vertical connection.
    /// </summary>
    public bool IsVertical { get; set; }

    /// <summary>
    /// Connection type for styling.
    /// </summary>
    public string ConnectionType { get; set; } = "Normal";
}

/// <summary>
/// View model for the minimap control.
/// Displays explored rooms, connections, and vertical layer information.
/// </summary>
public class MinimapViewModel : ViewModelBase
{
    private Dungeon? _dungeon;
    private Room? _currentRoom;
    private float _zoomLevel = 1.0f;
    private Point _panOffset = new Point(0, 0);
    private VerticalLayer _displayedLayer = VerticalLayer.GroundLevel;
    private HashSet<string> _exploredRoomIds = new();

    // Constants for minimap layout
    private const float RoomSize = 40f;
    private const float RoomSpacing = 60f;
    private const float CenterOffset = 150f; // Center of minimap canvas

    #region Properties

    /// <summary>
    /// Collection of room nodes to display on the minimap.
    /// </summary>
    public ObservableCollection<RoomNodeViewModel> RoomNodes { get; } = new();

    /// <summary>
    /// Collection of connection lines between rooms.
    /// </summary>
    public ObservableCollection<ConnectionViewModel> Connections { get; } = new();

    /// <summary>
    /// Current zoom level (0.5 to 3.0).
    /// </summary>
    public float ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            var clamped = Math.Clamp(value, 0.5f, 3.0f);
            this.RaiseAndSetIfChanged(ref _zoomLevel, clamped);
        }
    }

    /// <summary>
    /// Pan offset for the minimap view.
    /// </summary>
    public Point PanOffset
    {
        get => _panOffset;
        set => this.RaiseAndSetIfChanged(ref _panOffset, value);
    }

    /// <summary>
    /// The currently displayed vertical layer.
    /// </summary>
    public VerticalLayer DisplayedLayer
    {
        get => _displayedLayer;
        set
        {
            this.RaiseAndSetIfChanged(ref _displayedLayer, value);
            UpdateMinimap();
            this.RaisePropertyChanged(nameof(LayerDisplayName));
            this.RaisePropertyChanged(nameof(LayerDepthDisplay));
            this.RaisePropertyChanged(nameof(CanGoUp));
            this.RaisePropertyChanged(nameof(CanGoDown));
        }
    }

    /// <summary>
    /// Display name for the current layer.
    /// </summary>
    public string LayerDisplayName => DisplayedLayer switch
    {
        VerticalLayer.DeepRoots => "Deep Roots",
        VerticalLayer.LowerRoots => "Lower Roots",
        VerticalLayer.UpperRoots => "Upper Roots",
        VerticalLayer.GroundLevel => "Ground Level",
        VerticalLayer.LowerTrunk => "Lower Trunk",
        VerticalLayer.UpperTrunk => "Upper Trunk",
        VerticalLayer.Canopy => "Canopy",
        _ => "Unknown"
    };

    /// <summary>
    /// Depth display for the current layer.
    /// </summary>
    public string LayerDepthDisplay => DisplayedLayer.GetDepthNarrative();

    /// <summary>
    /// Whether we can navigate to a higher layer.
    /// </summary>
    public bool CanGoUp => (int)DisplayedLayer < 3 && HasRoomsAtLayer((VerticalLayer)((int)DisplayedLayer + 1));

    /// <summary>
    /// Whether we can navigate to a lower layer.
    /// </summary>
    public bool CanGoDown => (int)DisplayedLayer > -3 && HasRoomsAtLayer((VerticalLayer)((int)DisplayedLayer - 1));

    /// <summary>
    /// All available layers that have explored rooms.
    /// </summary>
    public ObservableCollection<VerticalLayer> AvailableLayers { get; } = new();

    /// <summary>
    /// Number of explored rooms.
    /// </summary>
    public int ExploredRoomCount => _exploredRoomIds.Count;

    /// <summary>
    /// Total rooms in dungeon.
    /// </summary>
    public int TotalRoomCount => _dungeon?.TotalRoomCount ?? 0;

    /// <summary>
    /// Exploration progress text.
    /// </summary>
    public string ExplorationProgress => $"{ExploredRoomCount}/{TotalRoomCount} rooms explored";

    #endregion

    #region Commands

    /// <summary>
    /// Command to zoom in.
    /// </summary>
    public ICommand ZoomInCommand { get; }

    /// <summary>
    /// Command to zoom out.
    /// </summary>
    public ICommand ZoomOutCommand { get; }

    /// <summary>
    /// Command to reset zoom and pan.
    /// </summary>
    public ICommand ResetViewCommand { get; }

    /// <summary>
    /// Command to go to a higher layer.
    /// </summary>
    public ICommand LayerUpCommand { get; }

    /// <summary>
    /// Command to go to a lower layer.
    /// </summary>
    public ICommand LayerDownCommand { get; }

    /// <summary>
    /// Command to center on current room.
    /// </summary>
    public ICommand CenterOnPlayerCommand { get; }

    #endregion

    public MinimapViewModel()
    {
        // Initialize commands
        ZoomInCommand = ReactiveCommand.Create(ZoomIn);
        ZoomOutCommand = ReactiveCommand.Create(ZoomOut);
        ResetViewCommand = ReactiveCommand.Create(ResetView);
        LayerUpCommand = ReactiveCommand.Create(LayerUp);
        LayerDownCommand = ReactiveCommand.Create(LayerDown);
        CenterOnPlayerCommand = ReactiveCommand.Create(CenterOnPlayer);
    }

    #region Public Methods

    /// <summary>
    /// Loads a dungeon and initializes the minimap.
    /// </summary>
    public void LoadDungeon(Dungeon dungeon, Room currentRoom)
    {
        _dungeon = dungeon;
        _currentRoom = currentRoom;
        _exploredRoomIds.Clear();

        // Mark start room as explored
        if (currentRoom != null)
        {
            _exploredRoomIds.Add(currentRoom.RoomId);
            DisplayedLayer = currentRoom.Layer;
        }

        UpdateAvailableLayers();
        UpdateMinimap();
    }

    /// <summary>
    /// Updates the current room and marks it as explored.
    /// </summary>
    public void UpdateCurrentRoom(Room room)
    {
        _currentRoom = room;

        if (room != null)
        {
            _exploredRoomIds.Add(room.RoomId);

            // Auto-switch to the room's layer if different
            if (room.Layer != DisplayedLayer)
            {
                DisplayedLayer = room.Layer;
            }
        }

        UpdateAvailableLayers();
        UpdateMinimap();
        this.RaisePropertyChanged(nameof(ExploredRoomCount));
        this.RaisePropertyChanged(nameof(ExplorationProgress));
    }

    /// <summary>
    /// Marks a room as explored without changing current room.
    /// </summary>
    public void MarkRoomExplored(string roomId)
    {
        _exploredRoomIds.Add(roomId);
        UpdateAvailableLayers();
        UpdateMinimap();
        this.RaisePropertyChanged(nameof(ExploredRoomCount));
        this.RaisePropertyChanged(nameof(ExplorationProgress));
    }

    /// <summary>
    /// Checks if a room has been explored.
    /// </summary>
    public bool IsRoomExplored(string roomId)
    {
        return _exploredRoomIds.Contains(roomId);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates the minimap display.
    /// </summary>
    private void UpdateMinimap()
    {
        RoomNodes.Clear();
        Connections.Clear();

        if (_dungeon == null) return;

        // Get explored rooms at current layer
        var layerRooms = _dungeon.Rooms.Values
            .Where(r => r.Layer == DisplayedLayer && _exploredRoomIds.Contains(r.RoomId))
            .ToList();

        // Build room nodes
        foreach (var room in layerRooms)
        {
            var position = CalculateRoomPosition(room);

            RoomNodes.Add(new RoomNodeViewModel
            {
                Room = room,
                Position = position,
                IsCurrentRoom = room.RoomId == _currentRoom?.RoomId,
                IsExplored = true,
                HasUpConnection = HasVerticalConnectionUp(room),
                HasDownConnection = HasVerticalConnectionDown(room),
                RoomType = GetRoomType(room),
                BiomeColor = GetBiomeColor(room.PrimaryBiome)
            });
        }

        // Build horizontal connections
        foreach (var room in layerRooms)
        {
            foreach (var (direction, targetRoomId) in room.Exits)
            {
                // Skip if target not explored or not on same layer
                var targetRoom = _dungeon.GetRoom(targetRoomId);
                if (targetRoom == null || !_exploredRoomIds.Contains(targetRoomId))
                    continue;
                if (targetRoom.Layer != DisplayedLayer)
                    continue;

                // Avoid duplicate connections (only draw from lower room ID)
                if (string.Compare(room.RoomId, targetRoomId, StringComparison.Ordinal) > 0)
                    continue;

                var startPos = CalculateRoomPosition(room);
                var endPos = CalculateRoomPosition(targetRoom);

                // Offset to center of room
                var centerOffset = RoomSize / 2;
                Connections.Add(new ConnectionViewModel
                {
                    StartPos = new Point(startPos.X + centerOffset, startPos.Y + centerOffset),
                    EndPos = new Point(endPos.X + centerOffset, endPos.Y + centerOffset),
                    IsVertical = false,
                    ConnectionType = "Horizontal"
                });
            }
        }

        // Add fog-of-war hints for adjacent unexplored rooms
        AddFogOfWarHints(layerRooms);
    }

    /// <summary>
    /// Calculates the position of a room on the minimap canvas.
    /// </summary>
    private Point CalculateRoomPosition(Room room)
    {
        // Use RoomPosition if available, otherwise calculate from room ID
        var pos = room.Position;

        // Convert grid coordinates to canvas coordinates
        var x = CenterOffset + (pos.X * RoomSpacing);
        var y = CenterOffset - (pos.Y * RoomSpacing); // Invert Y so north is up

        return new Point(x, y);
    }

    /// <summary>
    /// Adds fog-of-war hints for adjacent unexplored rooms.
    /// </summary>
    private void AddFogOfWarHints(List<Room> exploredRooms)
    {
        if (_dungeon == null) return;

        var hintedRooms = new HashSet<string>();

        foreach (var room in exploredRooms)
        {
            foreach (var (direction, targetRoomId) in room.Exits)
            {
                if (_exploredRoomIds.Contains(targetRoomId) || hintedRooms.Contains(targetRoomId))
                    continue;

                var targetRoom = _dungeon.GetRoom(targetRoomId);
                if (targetRoom == null || targetRoom.Layer != DisplayedLayer)
                    continue;

                hintedRooms.Add(targetRoomId);

                // Add a fog node (unexplored room hint)
                var position = CalculateRoomPosition(targetRoom);
                RoomNodes.Add(new RoomNodeViewModel
                {
                    Room = targetRoom,
                    Position = position,
                    IsCurrentRoom = false,
                    IsExplored = false,
                    HasUpConnection = false,
                    HasDownConnection = false,
                    RoomType = "Fog",
                    BiomeColor = "#333333"
                });
            }
        }
    }

    /// <summary>
    /// Checks if a room has an upward vertical connection.
    /// </summary>
    private bool HasVerticalConnectionUp(Room room)
    {
        return room.VerticalConnections.Any(vc =>
            vc.CanTraverse() &&
            ((vc.FromRoomId == room.RoomId && _dungeon?.GetRoom(vc.ToRoomId)?.Layer > room.Layer) ||
             (vc.IsBidirectional && vc.ToRoomId == room.RoomId && _dungeon?.GetRoom(vc.FromRoomId)?.Layer > room.Layer)));
    }

    /// <summary>
    /// Checks if a room has a downward vertical connection.
    /// </summary>
    private bool HasVerticalConnectionDown(Room room)
    {
        return room.VerticalConnections.Any(vc =>
            vc.CanTraverse() &&
            ((vc.FromRoomId == room.RoomId && _dungeon?.GetRoom(vc.ToRoomId)?.Layer < room.Layer) ||
             (vc.IsBidirectional && vc.ToRoomId == room.RoomId && _dungeon?.GetRoom(vc.FromRoomId)?.Layer < room.Layer)));
    }

    /// <summary>
    /// Gets the room type for styling.
    /// </summary>
    private static string GetRoomType(Room room)
    {
        if (room.IsStartRoom) return "Start";
        if (room.IsBossRoom) return "Boss";
        if (room.IsSanctuary) return "Sanctuary";
        if (room.Enemies.Count > 0 && !room.HasBeenCleared) return "Combat";
        if (room.HasPuzzle && !room.IsPuzzleSolved) return "Puzzle";
        return "Normal";
    }

    /// <summary>
    /// Gets the biome color.
    /// </summary>
    private static string GetBiomeColor(string? biome)
    {
        return biome?.ToLower().Replace("_", "") switch
        {
            "theroots" => "#8B4513",      // Brown
            "the_roots" => "#8B4513",
            "muspelheim" => "#FF4500",    // Orange-red
            "niflheim" => "#4169E1",      // Royal blue
            "jotunheim" => "#708090",     // Slate gray
            "alfheim" => "#FFD700",       // Gold
            _ => "#666666"                 // Default gray
        };
    }

    /// <summary>
    /// Checks if there are explored rooms at a given layer.
    /// </summary>
    private bool HasRoomsAtLayer(VerticalLayer layer)
    {
        if (_dungeon == null) return false;
        return _dungeon.Rooms.Values.Any(r => r.Layer == layer && _exploredRoomIds.Contains(r.RoomId));
    }

    /// <summary>
    /// Updates the list of available layers.
    /// </summary>
    private void UpdateAvailableLayers()
    {
        AvailableLayers.Clear();

        if (_dungeon == null) return;

        var layers = _dungeon.Rooms.Values
            .Where(r => _exploredRoomIds.Contains(r.RoomId))
            .Select(r => r.Layer)
            .Distinct()
            .OrderByDescending(l => (int)l)
            .ToList();

        foreach (var layer in layers)
        {
            AvailableLayers.Add(layer);
        }

        this.RaisePropertyChanged(nameof(CanGoUp));
        this.RaisePropertyChanged(nameof(CanGoDown));
    }

    #endregion

    #region Command Implementations

    private void ZoomIn()
    {
        ZoomLevel += 0.25f;
    }

    private void ZoomOut()
    {
        ZoomLevel -= 0.25f;
    }

    private void ResetView()
    {
        ZoomLevel = 1.0f;
        PanOffset = new Point(0, 0);
        CenterOnPlayer();
    }

    private void LayerUp()
    {
        if (CanGoUp)
        {
            DisplayedLayer = (VerticalLayer)((int)DisplayedLayer + 1);
        }
    }

    private void LayerDown()
    {
        if (CanGoDown)
        {
            DisplayedLayer = (VerticalLayer)((int)DisplayedLayer - 1);
        }
    }

    private void CenterOnPlayer()
    {
        if (_currentRoom == null) return;

        // Switch to player's layer
        DisplayedLayer = _currentRoom.Layer;

        // Calculate offset to center on player
        var roomPos = CalculateRoomPosition(_currentRoom);
        var centerX = CenterOffset;
        var centerY = CenterOffset;

        PanOffset = new Point(centerX - roomPos.X, centerY - roomPos.Y);
    }

    #endregion
}
