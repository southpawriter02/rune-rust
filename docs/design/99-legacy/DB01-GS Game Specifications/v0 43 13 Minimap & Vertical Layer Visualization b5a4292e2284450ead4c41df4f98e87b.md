# v0.43.13: Minimap & Vertical Layer Visualization

Type: UI
Description: Minimap system: minimap control showing room graph, fog-of-war for unexplored rooms, current room indicator, vertical layer display (depth/height), zoom/pan controls. 5-7 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.12, v0.4-v0.5 (Dungeon Systems)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.12, v0.4-v0.5 (Dungeon Systems)

**Estimated Time:** 5-7 hours

**Group:** Exploration

**Deliverable:** Minimap with fog-of-war and vertical layer display

---

## Executive Summary

v0.43.13 implements the minimap system, showing explored rooms, connections, fog-of-war for unexplored areas, and vertical layer indicators for dungeon depth/height.

**What This Delivers:**

- Minimap control showing room graph
- Fog-of-war for unexplored rooms
- Current room indicator
- Vertical layer display (depth/height)
- Vertical connections visualization (stairs, shafts)
- Zoom controls
- Pan functionality

**Success Metric:** Minimap accurately reflects explored dungeon with clear navigation aid.

---

## Service Implementation

### MinimapViewModel

```csharp
using ReactiveUI;
using RuneAndRust.Core.Dungeon;
using System.Collections.ObjectModel;

namespace RuneAndRust.DesktopUI.ViewModels;

public class MinimapViewModel : ViewModelBase
{
    private DungeonState? _dungeon;
    private Room? _currentRoom;
    private float _zoomLevel = 1.0f;
    private Point _panOffset = new Point(0, 0);
    
    public ObservableCollection<RoomNodeViewModel> ExploredRooms { get; } = new();
    public ObservableCollection<ConnectionViewModel> Connections { get; } = new();
    
    public float ZoomLevel
    {
        get => _zoomLevel;
        set => this.RaiseAndSetIfChanged(ref _zoomLevel, Math.Clamp(value, 0.5f, 3.0f));
    }
    
    public Point PanOffset
    {
        get => _panOffset;
        set => this.RaiseAndSetIfChanged(ref _panOffset, value);
    }
    
    public int CurrentLayer => _currentRoom?.Layer ?? 0;
    
    public void LoadDungeon(DungeonState dungeon, Room currentRoom)
    {
        _dungeon = dungeon;
        _currentRoom = currentRoom;
        UpdateMinimap();
    }
    
    public void UpdateCurrentRoom(Room room)
    {
        _currentRoom = room;
        UpdateMinimap();
        this.RaisePropertyChanged(nameof(CurrentLayer));
    }
    
    private void UpdateMinimap()
    {
        ExploredRooms.Clear();
        Connections.Clear();
        
        if (_dungeon == null) return;
        
        // Only show rooms on current layer
        var currentLayerRooms = _dungeon.Rooms
            .Where(r => r.IsExplored && r.Layer == CurrentLayer)
            .ToList();
        
        foreach (var room in currentLayerRooms)
        {
            ExploredRooms.Add(new RoomNodeViewModel
            {
                Room = room,
                Position = new Point(room.GridX * 60, room.GridY * 60),
                IsCurrentRoom = [room.Id](http://room.Id) == _currentRoom?.Id,
                HasUpConnection = room.HasExit(Direction.Up),
                HasDownConnection = room.HasExit(Direction.Down)
            });
        }
        
        // Draw connections between rooms
        foreach (var room in currentLayerRooms)
        {
            foreach (var exit in room.Exits)
            {
                if (exit.Direction == Direction.Up || exit.Direction == Direction.Down)
                    continue; // Vertical connections shown separately
                
                var connectedRoom = _dungeon.GetAdjacentRoom([room.Id](http://room.Id), exit.Direction);
                if (connectedRoom != null && connectedRoom.IsExplored)
                {
                    Connections.Add(new ConnectionViewModel
                    {
                        StartPos = new Point(room.GridX * 60 + 20, room.GridY * 60 + 20),
                        EndPos = new Point(connectedRoom.GridX * 60 + 20, connectedRoom.GridY * 60 + 20)
                    });
                }
            }
        }
    }
    
    public void ZoomIn() => ZoomLevel += 0.2f;
    public void ZoomOut() => ZoomLevel -= 0.2f;
    public void ResetZoom() => ZoomLevel = 1.0f;
}

public class RoomNodeViewModel
{
    public Room Room { get; set; } = null!;
    public Point Position { get; set; }
    public bool IsCurrentRoom { get; set; }
    public bool HasUpConnection { get; set; }
    public bool HasDownConnection { get; set; }
}

public class ConnectionViewModel
{
    public Point StartPos { get; set; }
    public Point EndPos { get; set; }
}
```

### MinimapControl (Custom Control)

```csharp
using Avalonia;
using Avalonia.Controls;
using [Avalonia.Media](http://Avalonia.Media);
using SkiaSharp;

namespace RuneAndRust.DesktopUI.Controls;

public class MinimapControl : Control
{
    public static readonly StyledProperty<MinimapViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<MinimapControl, MinimapViewModel?>(nameof(ViewModel));
    
    public MinimapViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
    
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        if (ViewModel == null) return;
        
        using var skContext = context as ISkiaDrawingContextImpl;
        if (skContext == null) return;
        
        var canvas = skContext.SkCanvas;
        
        // Apply zoom and pan
        [canvas.Save](http://canvas.Save)();
        canvas.Translate(ViewModel.PanOffset.X, ViewModel.PanOffset.Y);
        canvas.Scale(ViewModel.ZoomLevel, ViewModel.ZoomLevel);
        
        // Draw connections first (under rooms)
        DrawConnections(canvas);
        
        // Draw rooms
        DrawRooms(canvas);
        
        canvas.Restore();
    }
    
    private void DrawConnections(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Gray,
            StrokeWidth = 2,
            IsStroke = true
        };
        
        foreach (var connection in ViewModel!.Connections)
        {
            canvas.DrawLine(
                (float)connection.StartPos.X,
                (float)connection.StartPos.Y,
                (float)connection.EndPos.X,
                (float)connection.EndPos.Y,
                paint);
        }
    }
    
    private void DrawRooms(SKCanvas canvas)
    {
        foreach (var roomNode in ViewModel!.ExploredRooms)
        {
            var x = (float)roomNode.Position.X;
            var y = (float)roomNode.Position.Y;
            
            // Room rectangle
            var roomColor = roomNode.IsCurrentRoom ? [SKColors.Gold](http://SKColors.Gold) : SKColors.LightBlue;
            using var roomPaint = new SKPaint
            {
                Color = roomColor,
                IsAntialias = true
            };
            
            canvas.DrawRect(x, y, 40, 40, roomPaint);
            
            // Border
            using var borderPaint = new SKPaint
            {
                Color = [SKColors.Black](http://SKColors.Black),
                StrokeWidth = 2,
                IsStroke = true
            };
            canvas.DrawRect(x, y, 40, 40, borderPaint);
            
            // Vertical connection indicators
            if (roomNode.HasUpConnection)
            {
                using var upPaint = new SKPaint { Color = SKColors.Yellow };
                canvas.DrawCircle(x + 10, y + 5, 4, upPaint);
            }
            
            if (roomNode.HasDownConnection)
            {
                using var downPaint = new SKPaint { Color = SKColors.Cyan };
                canvas.DrawCircle(x + 30, y + 35, 4, downPaint);
            }
        }
    }
}
```

---

## Integration Points

**With v0.4 (Dungeon Generation):**

- Uses room graph structure
- Displays biome colors

**With v0.5 (Vertical Progression):**

- Shows layer information
- Indicates vertical connections
- Filters by current layer

**With v0.43.12 (Navigation):**

- Updates as player moves
- Highlights current room

---

## Success Criteria

**v0.43.13 is DONE when:**

### ✅ Minimap Display

- [ ]  Explored rooms visible
- [ ]  Connections drawn correctly
- [ ]  Current room highlighted
- [ ]  Fog-of-war hides unexplored

### ✅ Vertical Layers

- [ ]  Shows current layer only
- [ ]  Layer indicator visible
- [ ]  Up/down connections marked
- [ ]  Can switch between layers

### ✅ Controls

- [ ]  Zoom in/out works
- [ ]  Pan functionality
- [ ]  Reset view button
- [ ]  Responsive to updates

---

**Minimap complete. Ready for room interactions in v0.43.14.**