# v0.43.4: Combat Grid Control & Positioning

Type: UI
Description: Core combat grid visualization: CombatGridControl custom control, 6×4 grid rendering, unit sprite display at positions, cell selection/highlighting, and BattlefieldGrid integration from v0.20. 6-8 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.3, v0.20-v0.20.5 (Tactical Grid)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.3, v0.20-v0.20.5 (Tactical Grid)

**Estimated Time:** 6-8 hours

**Group:** Combat UI

**Deliverable:** Visual combat grid with unit positioning

---

## Executive Summary

v0.43.4 implements the core combat grid visualization, rendering the 6×4 battlefield from v0.20 with unit sprites, cell highlighting, and selection functionality. This is the foundation for all combat UI interactions.

**What This Delivers:**

- `CombatGridControl` custom Avalonia control
- 6×4 grid rendering with cells
- Unit sprite display at grid positions
- Cell selection and highlighting
- Mouse hover effects
- Integration with `BattlefieldGrid` from v0.20

**Success Metric:** Combat grid displays current battle state accurately with all units visible and interactive.

---

## Database Schema Changes

No database changes in this specification.

---

## Service Implementation

### CombatGridControl

```csharp
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using [Avalonia.Media](http://Avalonia.Media);
using Avalonia.Skia;
using RuneAndRust.Core.Combat;
using [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);
using SkiaSharp;

namespace RuneAndRust.DesktopUI.Controls;

public class CombatGridControl : Control
{
    private readonly ISpriteService _spriteService;
    private const int CellSize = 100;
    private const int SpriteSize = 48;
    private GridPosition? _selectedCell;
    private GridPosition? _hoveredCell;
    
    public static readonly StyledProperty<BattlefieldGrid?> GridProperty =
        AvaloniaProperty.Register<CombatGridControl, BattlefieldGrid?>(nameof(Grid));
    
    public static readonly StyledProperty<IEnumerable<GridPosition>?> HighlightedCellsProperty =
        AvaloniaProperty.Register<CombatGridControl, IEnumerable<GridPosition>?>(nameof(HighlightedCells));
    
    public static readonly StyledProperty<Color> HighlightColorProperty =
        AvaloniaProperty.Register<CombatGridControl, Color>(nameof(HighlightColor), [Colors.Blue](http://Colors.Blue));
    
    public BattlefieldGrid? Grid
    {
        get => GetValue(GridProperty);
        set => SetValue(GridProperty, value);
    }
    
    public IEnumerable<GridPosition>? HighlightedCells
    {
        get => GetValue(HighlightedCellsProperty);
        set => SetValue(HighlightedCellsProperty, value);
    }
    
    public Color HighlightColor
    {
        get => GetValue(HighlightColorProperty);
        set => SetValue(HighlightColorProperty, value);
    }
    
    public event EventHandler<GridPosition>? CellClicked;
    public event EventHandler<GridPosition>? CellHovered;
    
    public CombatGridControl(ISpriteService spriteService)
    {
        _spriteService = spriteService;
        Width = 600;  // 6 columns * 100
        Height = 400; // 4 rows * 100
        
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
    }
    
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        if (Grid == null) return;
        
        using var skContext = context as ISkiaDrawingContextImpl;
        if (skContext == null) return;
        
        var canvas = skContext.SkCanvas;
        
        // Draw grid background
        DrawGridBackground(canvas);
        
        // Draw highlighted cells
        DrawHighlightedCells(canvas);
        
        // Draw hovered cell
        if (_hoveredCell != null)
            DrawCellHighlight(canvas, _hoveredCell.Value, SKColors.Yellow.WithAlpha(50));
        
        // Draw selected cell
        if (_selectedCell != null)
            DrawCellHighlight(canvas, _selectedCell.Value, SKColors.Cyan.WithAlpha(100));
        
        // Draw grid lines
        DrawGridLines(canvas);
        
        // Draw units
        foreach (var tile in Grid.Tiles.Values)
        {
            if (tile.Occupant != null)
            {
                DrawUnit(canvas, tile.Position, tile.Occupant);
            }
        }
    }
    
    private void DrawGridBackground(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.DarkSlateGray,
            IsAntialias = true
        };
        
        canvas.DrawRect(0, 0, 600, 400, paint);
    }
    
    private void DrawGridLines(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Gray,
            StrokeWidth = 2,
            IsStroke = true
        };
        
        // Vertical lines
        for (int x = 0; x <= 6; x++)
        {
            canvas.DrawLine(x * CellSize, 0, x * CellSize, 400, paint);
        }
        
        // Horizontal lines
        for (int y = 0; y <= 4; y++)
        {
            canvas.DrawLine(0, y * CellSize, 600, y * CellSize, paint);
        }
    }
    
    private void DrawHighlightedCells(SKCanvas canvas)
    {
        if (HighlightedCells == null) return;
        
        var skColor = new SKColor(
            HighlightColor.R,
            HighlightColor.G,
            HighlightColor.B,
            (byte)(HighlightColor.A * 0.3));
        
        foreach (var pos in HighlightedCells)
        {
            DrawCellHighlight(canvas, pos, skColor);
        }
    }
    
    private void DrawCellHighlight(SKCanvas canvas, GridPosition pos, SKColor color)
    {
        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true
        };
        
        canvas.DrawRect(pos.X * CellSize, pos.Y * CellSize, CellSize, CellSize, paint);
    }
    
    private void DrawUnit(SKCanvas canvas, GridPosition pos, Combatant unit)
    {
        var spriteName = GetSpriteNameForUnit(unit);
        var sprite = _spriteService.GetSpriteBitmap(spriteName, scale: 3);
        
        if (sprite == null) return;
        
        var x = pos.X * CellSize + (CellSize - SpriteSize) / 2;
        var y = pos.Y * CellSize + (CellSize - SpriteSize) / 2;
        
        canvas.DrawBitmap(sprite, new SKPoint(x, y));
    }
    
    private string GetSpriteNameForUnit(Combatant unit)
    {
        if (unit is PlayerCharacter player)
        {
            return $"player_{player.SpecializationName.ToLower()}";
        }
        else if (unit is Enemy enemy)
        {
            return $"enemy_{enemy.EnemyType.ToLower()}";
        }
        
        return "unknown";
    }
    
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetPosition(this);
        var gridPos = ScreenToGridPosition(point);
        
        if (gridPos != null)
        {
            _selectedCell = gridPos;
            CellClicked?.Invoke(this, gridPos.Value);
            InvalidateVisual();
        }
    }
    
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetPosition(this);
        var gridPos = ScreenToGridPosition(point);
        
        if (gridPos != _hoveredCell)
        {
            _hoveredCell = gridPos;
            if (gridPos != null)
                CellHovered?.Invoke(this, gridPos.Value);
            InvalidateVisual();
        }
    }
    
    private GridPosition? ScreenToGridPosition(Point point)
    {
        var x = (int)(point.X / CellSize);
        var y = (int)(point.Y / CellSize);
        
        if (x >= 0 && x < 6 && y >= 0 && y < 4)
            return new GridPosition(x, y);
        
        return null;
    }
}
```

### CombatViewModel (Initial)

```csharp
using ReactiveUI;
using RuneAndRust.Core.Combat;
using [RuneAndRust.Engine.Services](http://RuneAndRust.Engine.Services);
using System.Collections.ObjectModel;

namespace RuneAndRust.DesktopUI.ViewModels;

public class CombatViewModel : ViewModelBase
{
    private readonly ICombatEngine _combatEngine;
    private BattlefieldGrid? _grid;
    private ObservableCollection<GridPosition> _highlightedCells = new();
    
    public BattlefieldGrid? Grid
    {
        get => _grid;
        private set => this.RaiseAndSetIfChanged(ref _grid, value);
    }
    
    public ObservableCollection<GridPosition> HighlightedCells
    {
        get => _highlightedCells;
        set => this.RaiseAndSetIfChanged(ref _highlightedCells, value);
    }
    
    public CombatViewModel(ICombatEngine combatEngine)
    {
        _combatEngine = combatEngine;
    }
    
    public void LoadCombatState(CombatState state)
    {
        Grid = state.BattlefieldGrid;
    }
    
    public void HighlightMovementRange(Combatant unit)
    {
        HighlightedCells.Clear();
        var validMoves = _combatEngine.GetValidMovementPositions(unit);
        foreach (var pos in validMoves)
        {
            HighlightedCells.Add(pos);
        }
    }
    
    public void ClearHighlights()
    {
        HighlightedCells.Clear();
    }
}
```

---

## Integration Points

**With v0.20 (Tactical Grid):**

- Uses `BattlefieldGrid` as data source
- Displays all `GridTile` states
- Respects 6×4 grid dimensions

**With v0.43.2 (Sprite System):**

- Fetches unit sprites from `SpriteService`
- Renders sprites at appropriate scale (3x = 48×48px)

**With v0.43.5+ (Combat Actions):**

- Provides cell selection events
- Supports highlighting for targeting
- Foundation for action execution

---

## Functional Requirements

### FR1: Grid Rendering

**Requirement:** Render 6×4 grid with cells and lines.

**Test:**

```csharp
[Fact]
public void CombatGridControl_RendersCorrectDimensions()
{
    var spriteService = CreateMockSpriteService();
    var control = new CombatGridControl(spriteService);
    
    Assert.Equal(600, control.Width);
    Assert.Equal(400, control.Height);
}

[Fact]
public void CombatGridControl_DisplaysGrid()
{
    var spriteService = CreateMockSpriteService();
    var control = new CombatGridControl(spriteService);
    var grid = new BattlefieldGrid(6, 4);
    
    control.Grid = grid;
    
    Assert.NotNull(control.Grid);
    Assert.Equal(6, control.Grid.Width);
    Assert.Equal(4, control.Grid.Height);
}
```

### FR2: Unit Display

**Requirement:** Show units at their grid positions with sprites.

**Test:**

```csharp
[Fact]
public void CombatGridControl_DisplaysUnitsAtPositions()
{
    var spriteService = CreateMockSpriteService();
    var control = new CombatGridControl(spriteService);
    var grid = new BattlefieldGrid(6, 4);
    
    var player = new PlayerCharacter { Name = "Warrior" };
    grid.PlaceUnit(player, new GridPosition(0, 0));
    
    control.Grid = grid;
    
    var tile = grid.GetTileAt(new GridPosition(0, 0));
    Assert.NotNull(tile.Occupant);
    Assert.Same(player, tile.Occupant);
}
```

### FR3: Cell Selection

**Requirement:** Handle mouse clicks to select cells.

**Test:**

```csharp
[Fact]
public void CombatGridControl_HandlesClickSelection()
{
    var spriteService = CreateMockSpriteService();
    var control = new CombatGridControl(spriteService);
    
    GridPosition? clickedPos = null;
    control.CellClicked += (s, pos) => clickedPos = pos;
    
    // Simulate click at (1, 1)
    var clickEvent = CreateClickEvent(150, 150);
    control.OnPointerPressed(control, clickEvent);
    
    Assert.NotNull(clickedPos);
    Assert.Equal(1, clickedPos.Value.X);
    Assert.Equal(1, clickedPos.Value.Y);
}
```

### FR4: Cell Highlighting

**Requirement:** Highlight cells for movement/attack ranges.

**Test:**

```csharp
[Fact]
public void CombatGridControl_HighlightsCells()
{
    var spriteService = CreateMockSpriteService();
    var control = new CombatGridControl(spriteService);
    
    var highlightedCells = new List<GridPosition>
    {
        new GridPosition(0, 0),
        new GridPosition(1, 0),
        new GridPosition(0, 1)
    };
    
    control.HighlightedCells = highlightedCells;
    
    Assert.Equal(3, control.HighlightedCells.Count());
}
```

---

## Testing Requirements

### Unit Tests (Target: 80%+ coverage)

**CombatGridControl Tests:**

- Grid rendering dimensions
- Unit display at positions
- Cell selection via mouse
- Cell hovering
- Highlighting multiple cells
- Screen-to-grid coordinate conversion

**CombatViewModel Tests:**

- Loading combat state
- Highlighting movement range
- Clearing highlights

### Visual Tests

**Manual Testing Checklist:**

- [ ]  Grid renders with 6 columns, 4 rows
- [ ]  Grid lines are visible
- [ ]  Units appear at correct positions
- [ ]  Clicking cell selects it (cyan highlight)
- [ ]  Hovering cell shows yellow highlight
- [ ]  Multiple cells can be highlighted (blue)
- [ ]  Sprites render at correct size

---

## Success Criteria

**v0.43.4 is DONE when:**

### ✅ Grid Display

- [ ]  CombatGridControl implemented
- [ ]  6×4 grid renders correctly
- [ ]  Grid lines visible
- [ ]  Background color appropriate

### ✅ Unit Display

- [ ]  Units display at positions
- [ ]  Sprites load from SpriteService
- [ ]  Sprites centered in cells
- [ ]  Player and enemy sprites distinguish

### ✅ Interaction

- [ ]  Mouse clicks select cells
- [ ]  Mouse hover highlights cells
- [ ]  Cell selection events fire
- [ ]  Highlighted cells render correctly

### ✅ Integration

- [ ]  Works with BattlefieldGrid from v0.20
- [ ]  CombatViewModel binds to control
- [ ]  No performance issues

---

**Combat grid foundation complete. Ready for actions in v0.43.5.**