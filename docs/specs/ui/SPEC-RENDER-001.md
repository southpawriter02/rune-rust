---
id: SPEC-RENDER-001
title: Rendering Pipeline System
version: 1.0.1
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-UI-001, SPEC-THEME-001, SPEC-COMBAT-001]
---

# SPEC-RENDER-001: Rendering Pipeline System

> **Version:** 1.0.1
> **Status:** Implemented
> **Services:** ICombatScreenRenderer, IExplorationScreenRenderer, IInventoryScreenRenderer, IVictoryScreenRenderer, IVisualEffectService
> **Location:** `RuneAndRust.Terminal/Services/`, `RuneAndRust.Terminal/Rendering/`

---

## Overview

The **Rendering Pipeline System** manages all visual output for Rune & Rust using **Spectre.Console 0.54.0**, a terminal rendering library for .NET. The architecture employs **immediate mode rendering** where each user action triggers a complete screen redraw, ensuring deterministic visual state without retained frame buffers.

### Key Responsibilities

1. **Screen Rendering**: Transform immutable ViewModels into Spectre.Console layouts (8 screen types)
2. **Sub-Renderer Composition**: Delegate complex UI sections to specialized sub-renderers (Combat grid, Timeline, Minimap)
3. **Layout Management**: Construct hierarchical panel/table structures using Spectre.Console Layout API
4. **Visual Effects**: Coordinate transient UI enhancements (border flashes, duration-based emphasis)
5. **Theme Integration**: Apply semantic color roles via IThemeService for accessibility compliance

### Architecture Pattern

```
ViewModel (Immutable) → IScreenRenderer → Spectre.Console Layout → Terminal Output
                             ↓
                    Sub-Renderers (Grid, Timeline, Minimap, etc.)
                             ↓
                        IThemeService (Color Lookup)
```

**Rendering Frequency**: Once per player action (not frame-based). The game loop (GameService) triggers rendering only when GameState changes, achieving efficient terminal updates without continuous refresh cycles.

**Technology Stack**:
- **Spectre.Console 0.54.0**: Layout, Panel, Table, Rule, Grid, BarChart components
- **.NET 9.0**: Async rendering, nullable reference types, record ViewModels
- **Dependency Injection**: Singleton lifetime for all renderers (stateless services)

---

## Core Concepts

### 1. Immediate Mode Rendering

**Definition**: The terminal is fully cleared and redrawn on every render call. No state is retained between frames.

**Implications**:
- **Deterministic Output**: Each render produces identical output for identical ViewModel input
- **Simplified Debugging**: No "dirty rectangle" tracking or partial updates
- **Performance Trade-Off**: Higher rendering overhead (acceptable for turn-based gameplay where rendering is infrequent)

**Example (Combat Screen)**:
```csharp
public void Render(CombatViewModel viewModel)
{
    AnsiConsole.Clear(); // Full screen clear

    var layout = new Layout("Root")
        .SplitRows(
            new Layout("Header").Size(3),
            new Layout("Main").Ratio(1),
            new Layout("Footer").Size(1)
        );

    layout["Header"].Update(RenderHeader(viewModel));
    layout["Main"].Update(RenderCombatGrid(viewModel));
    layout["Footer"].Update(RenderFooter(viewModel));

    AnsiConsole.Write(layout);
}
```

### 2. Screen Renderer Interfaces

**8 Dedicated Renderers**:
| Interface | Implementation | ViewModel | Purpose |
|-----------|---------------|-----------|---------|
| `ICombatScreenRenderer` | `CombatScreenRenderer` | `CombatViewModel` | 4-row tactical grid + timeline |
| `IExplorationScreenRenderer` | `ExplorationScreenRenderer` | `ExplorationViewModel` | 3-pane layout (room + minimap + status) |
| `IInventoryScreenRenderer` | `InventoryScreenRenderer` | `InventoryViewModel` | Item list + equipped gear display |
| `IJournalScreenRenderer` | `JournalScreenRenderer` | `JournalViewModel` | Quest log + timeline entries |
| `ICraftingScreenRenderer` | `CraftingScreenRenderer` | `CraftingViewModel` | Recipe browser + material inventory |
| `IOptionsScreenRenderer` | `OptionsScreenRenderer` | `OptionsViewModel` | Settings menu (theme, controls, accessibility) |
| `IRestScreenRenderer` | `RestScreenRenderer` | `RestViewModel` | Camp menu (heal, craft, manage inventory) |
| `IVictoryScreenRenderer` | `VictoryScreenRenderer` | `CombatResult` | XP gain summary + loot display |

**Contract**: Each renderer implements `void Render(TViewModel viewModel)` where TViewModel is the corresponding immutable record from `RuneAndRust.Core/ViewModels/` (exception: `IVictoryScreenRenderer` uses `CombatResult` from `RuneAndRust.Core/Models/Combat/`).

### 3. Sub-Renderer Pattern

**Purpose**: Delegate complex UI sections to specialized components for code organization and reusability.

**Key Sub-Renderers** (all static classes):
- **CombatGridRenderer**: Renders 4-row tactical grid with player/enemy positions
- **TimelineRenderer**: Renders initiative timeline with 8-slot projection and turn order
- **MinimapRenderer**: Renders 3×3 grid of adjacent rooms with direction indicators
- **RoomRenderer**: Renders current room description with exits and objects
- **StatusWidget**: Static helper for HP/Stamina/Stress bars with color-coded thresholds

**Example (Sub-Renderer Composition with Static Methods)**:
```csharp
public class CombatScreenRenderer : ICombatScreenRenderer
{
    private readonly IThemeService _themeService;
    private readonly IVisualEffectService _visualEffectService;

    public void Render(CombatViewModel vm)
    {
        AnsiConsole.Clear();

        // Query active border override from visual effect service
        var borderOverride = _visualEffectService.GetBorderOverride();

        var mainLayout = new Layout()
            .SplitColumns(
                new Layout("Grid").Ratio(3),
                new Layout("Timeline").Ratio(1)
            );

        // Static method calls with theme service injection
        mainLayout["Grid"].Update(CombatGridRenderer.Render(vm, _themeService, borderOverride));
        mainLayout["Timeline"].Update(TimelineRenderer.Render(
            vm.TimelineProjection, vm.RoundNumber, _themeService));

        AnsiConsole.Write(mainLayout);
    }
}
```

### 4. Spectre.Console Layout System

**Hierarchical Composition**: Layouts are split into rows/columns recursively to create complex screen structures.

**Core Components**:
- **Layout**: Container for hierarchical splits (`.SplitRows()`, `.SplitColumns()`)
- **Panel**: Bordered box with title and content
- **Table**: Grid with headers and rows (used for item lists, ability tables)
- **Rule**: Horizontal separator with optional title
- **Grid**: Flexible column/row layout (used for combat tactical grid)
- **BarChart**: Horizontal bar visualization (used for HP/Stamina)

**Example (3-Pane Exploration Layout)**:
```csharp
var root = new Layout("Root")
    .SplitRows(
        new Layout("Header").Size(5),      // Room name + description
        new Layout("Main").Ratio(1),       // Main content area
        new Layout("Footer").Size(3)       // Status bars
    );

root["Main"].SplitColumns(
    new Layout("Content").Ratio(3),        // Room details + exits
    new Layout("Sidebar").Ratio(1)         // Minimap + quick stats
);
```

### 5. Visual Effect System

**IVisualEffectService**: Coordinates transient UI enhancements that don't persist between renders.

**Core Methods** (v0.3.9a):
```csharp
Task TriggerEffectAsync(VisualEffectType effectType, int intensity = 1);
void SetBorderOverride(string? colorOverride);
string? GetBorderOverride();
void ClearBorderOverride();
```

**VisualEffectType Enum**:
- `DamageTaken` - Red flash when player takes damage
- `CriticalHit` - Yellow/gold flash for critical strikes
- `Healing` - Green flash for HP restoration

**Behavior**:
- **Intensity Mapping**: `1 = 50ms`, `2 = 100ms`, `3 = 150ms`, etc.
- **Async Pattern**: Effect runs asynchronously, respects `GameSettings.ReduceMotion`
- **Border Override**: `SetBorderOverride()` sets color, renderer queries via `GetBorderOverride()`
- **Use Cases**: Damage taken (red flash), healing (green flash), critical hit (gold flash)

**Example (Combat Damage Flash)**:
```csharp
// In CombatService after damage resolution:
await _visualEffectService.TriggerEffectAsync(VisualEffectType.DamageTaken, intensity: 3);

// In CombatScreenRenderer, query active override:
var borderOverride = _visualEffectService.GetBorderOverride();
var grid = CombatGridRenderer.Render(vm, _themeService, borderOverride);
```

### 6. Theme-Driven Color Application

**Integration with IThemeService** (see SPEC-THEME-001):
- All color references use semantic roles (`PlayerColor`, `HealthCritical`, `QualityLegendary`)
- Renderers never hardcode color values
- Markup syntax: `[{themeService.GetColor("PlayerColor")}]Player Name[/]`

**Example (HP Bar with Theme Colors)**:
```csharp
var hpColor = hpPercent > 0.5 ? _theme.GetColor("HealthFull") :
              hpPercent > 0.25 ? _theme.GetColor("HealthLow") :
                                 _theme.GetColor("HealthCritical");

var hpBar = new BarChart()
    .Width(20)
    .AddItem("HP", currentHp, Color.Parse(hpColor));
```

---

## Behaviors

### B-1: Render(ViewModel) - Core Rendering Method

**Signature**: `void Render(TViewModel viewModel)`

**Purpose**: Transform immutable ViewModel into terminal output using Spectre.Console.

**Sequence**:
```
1. Clear terminal (AnsiConsole.Clear())
2. Create root Layout with hierarchical splits
3. Populate layout sections with panels/tables/grids
4. Apply theme colors via IThemeService.GetColor()
5. Invoke sub-renderers for complex sections
6. Write final layout to terminal (AnsiConsole.Write())
```

**Example (Inventory Screen)**:
```csharp
public void Render(InventoryViewModel viewModel)
{
    AnsiConsole.Clear();

    var layout = new Layout("Root")
        .SplitRows(
            new Layout("Header").Size(3),
            new Layout("Main").Ratio(1),
            new Layout("Footer").Size(2)
        );

    // Header: Character name + encumbrance
    var header = new Panel($"[{_theme.GetColor("PlayerColor")}]{viewModel.CharacterName}[/] - Inventory")
        .BorderColor(Color.Parse(_theme.GetColor("PanelBorder")));

    // Main: Item table
    var itemTable = new Table()
        .AddColumn("Item")
        .AddColumn("Slot")
        .AddColumn("Quality");

    foreach (var item in viewModel.Items)
    {
        var qualityColor = _theme.GetColor($"Quality{item.Quality}");
        itemTable.AddRow(
            $"[{qualityColor}]{item.Name}[/]",
            item.EquippedSlot ?? "-",
            item.Quality.ToString()
        );
    }

    layout["Header"].Update(header);
    layout["Main"].Update(itemTable);
    layout["Footer"].Update(RenderStatusBars(viewModel.HP, viewModel.Stamina));

    AnsiConsole.Write(layout);
}
```

**Performance**: Typical render time < 50ms for complex screens (Combat, Inventory). Acceptable for turn-based gameplay where renders occur once per player action.

---

### B-2: Sub-Renderer Delegation

**Purpose**: Isolate complex UI sections for testability and reusability.

**Pattern**:
```csharp
public class CombatScreenRenderer : ICombatScreenRenderer
{
    private readonly CombatGridRenderer _gridRenderer;      // 4-row tactical grid
    private readonly TimelineRenderer _timelineRenderer;    // Initiative timeline
    private readonly IThemeService _theme;

    public CombatScreenRenderer(
        CombatGridRenderer gridRenderer,
        TimelineRenderer timelineRenderer,
        IThemeService theme)
    {
        _gridRenderer = gridRenderer;
        _timelineRenderer = timelineRenderer;
        _theme = theme;
    }

    public void Render(CombatViewModel vm)
    {
        AnsiConsole.Clear();

        var layout = new Layout("Root")
            .SplitRows(
                new Layout("Header").Size(3),
                new Layout("Main").Ratio(1),
                new Layout("Footer").Size(2)
            );

        // Delegate to sub-renderers
        layout["Header"].Update(RenderHeader(vm));
        layout["Main"].Update(_gridRenderer.Render(vm));  // Sub-renderer 1
        layout["Footer"].Update(_timelineRenderer.Render(vm)); // Sub-renderer 2

        AnsiConsole.Write(layout);
    }
}
```

**Key Sub-Renderers**:

#### CombatGridRenderer
**Input**: `CombatViewModel` (contains `List<CombatantView>`, `Grid<GridCell>`)
**Output**: Spectre.Console `Grid` with 4 rows × 8 columns

**Logic**:
```csharp
public Grid Render(CombatViewModel vm)
{
    var grid = new Grid()
        .AddColumn(new GridColumn().Width(10)) // Repeat for 8 columns
        .Border(GridBorder.Rounded);

    // 4 rows: Back, Mid-Back, Mid-Front, Front
    for (int row = 0; row < 4; row++)
    {
        var cells = new List<string>();
        for (int col = 0; col < 8; col++)
        {
            var cellData = vm.Grid[row, col];
            var cellMarkup = FormatCell(cellData); // Apply theme colors
            cells.Add(cellMarkup);
        }
        grid.AddRow(cells.ToArray());
    }

    return grid;
}
```

#### TimelineRenderer
**Input**: `CombatViewModel.TurnOrder` (List of combatants sorted by initiative)
**Output**: Spectre.Console `Panel` with 8-slot turn projection

**Logic**:
```csharp
public Panel Render(CombatViewModel vm)
{
    var timeline = new StringBuilder();

    for (int i = 0; i < 8 && i < vm.TurnOrder.Count; i++)
    {
        var combatant = vm.TurnOrder[i];
        var color = combatant.IsPlayer ? _theme.GetColor("PlayerColor") :
                                        _theme.GetColor("EnemyColor");
        var indicator = (i == 0) ? "▶" : " ";

        timeline.AppendLine($"{indicator} [{color}]{combatant.Name}[/] (Init: {combatant.Initiative})");
    }

    return new Panel(timeline.ToString())
        .Header("[yellow]Initiative Timeline[/]")
        .BorderColor(Color.Yellow);
}
```

#### MinimapRenderer
**Input**: `ExplorationViewModel.Minimap` (3×3 grid of room summaries)
**Output**: Spectre.Console `Grid` with direction indicators

**Logic**:
```csharp
public Grid Render(ExplorationViewModel vm)
{
    var grid = new Grid()
        .AddColumn(new GridColumn().Width(15)) // 3 columns
        .AddColumn(new GridColumn().Width(15))
        .AddColumn(new GridColumn().Width(15));

    // Direction labels: NW, N, NE, W, [You], E, SW, S, SE
    var directions = new[] { "NW", "N", "NE", "W", "[You]", "E", "SW", "S", "SE" };

    for (int row = 0; row < 3; row++)
    {
        var cells = new List<string>();
        for (int col = 0; col < 3; col++)
        {
            int index = row * 3 + col;
            var roomData = vm.Minimap[index];
            var label = (row == 1 && col == 1) ? "[green][You][/]" : directions[index];
            var cellMarkup = roomData?.HasBeenVisited == true ?
                             $"[dim]{label}[/]" :
                             $"[grey]{label}[/]";
            cells.Add(cellMarkup);
        }
        grid.AddRow(cells.ToArray());
    }

    return grid;
}
```

#### StatusWidget
**Input**: HP, Stamina, MaxHP, MaxStamina
**Output**: Spectre.Console `Panel` with color-coded bars

**Logic**:
```csharp
public Panel Render(int hp, int maxHp, int stamina, int maxStamina)
{
    var hpPercent = (double)hp / maxHp;
    var staminaPercent = (double)stamina / maxStamina;

    var hpColor = hpPercent > 0.5 ? _theme.GetColor("HealthFull") :
                  hpPercent > 0.25 ? _theme.GetColor("HealthLow") :
                                     _theme.GetColor("HealthCritical");

    var staminaColor = _theme.GetColor("StaminaColor");

    var hpBar = new BarChart()
        .Width(30)
        .AddItem("HP", hp, Color.Parse(hpColor));

    var staminaBar = new BarChart()
        .Width(30)
        .AddItem("Stamina", stamina, Color.Parse(staminaColor));

    var content = new Rows(hpBar, staminaBar);

    return new Panel(content)
        .Header("[white]Status[/]")
        .BorderColor(Color.White);
}
```

---

### B-3: Visual Effect Application

**Signature**: `void ApplyBorderFlash(Panel panel, int intensity)`

**Purpose**: Provide visual feedback for game events (damage, healing, critical hits).

**Implementation**:
```csharp
public class VisualEffectService : IVisualEffectService
{
    public void ApplyBorderFlash(Panel panel, int intensity)
    {
        var originalColor = panel.Border.Color;
        var durationMs = intensity * 50; // 1 = 50ms, 2 = 100ms, etc.

        // Flash effect (in actual implementation, would use async delay)
        panel.BorderColor(Color.Red);
        Thread.Sleep(durationMs);
        panel.BorderColor(originalColor);
    }
}
```

**Use Cases**:
- **Damage Taken**: Red flash, intensity = damage magnitude (1-5)
- **Healing**: Green flash, intensity = heal magnitude (1-3)
- **Critical Hit**: Yellow flash, intensity = 5
- **Ability Activation**: Blue flash, intensity = 2

**Limitations**: Current implementation uses blocking `Thread.Sleep`. Future enhancement should use async delays with cancellation tokens.

---

## Restrictions

### R-1: Render Frequency
- **Rule**: Renderers are called ONLY when ViewModel changes (once per player action).
- **Rationale**: Turn-based gameplay does not require continuous frame updates.
- **Enforcement**: GameService.RunAsync() only triggers rendering after CommandParser.ParseAndExecuteAsync() modifies GameState.

### R-2: No State Retention
- **Rule**: Renderers must be stateless. All rendering state must come from ViewModel.
- **Rationale**: Immediate mode rendering requires deterministic output from input.
- **Enforcement**: Renderers are registered as Singletons in DI container. No private fields storing rendering state.

### R-3: Theme Color Mandates
- **Rule**: No hardcoded color values. All colors MUST be retrieved via `IThemeService.GetColor(string role)`.
- **Rationale**: Accessibility compliance (5 theme support: Standard, HighContrast, Protanopia, Deuteranopia, Tritanopia).
- **Enforcement**: Code review, ThemeService tests validate all semantic roles exist.

### R-4: Sub-Renderer Isolation
- **Rule**: Sub-renderers MUST NOT call `AnsiConsole.Clear()` or `AnsiConsole.Write()`.
- **Rationale**: Only top-level screen renderers control terminal lifecycle.
- **Enforcement**: Sub-renderers return Spectre.Console objects (Panel, Grid, Table), not void.

---

## Limitations

### L-1: Terminal Size Constraints
- **Issue**: Layouts are fixed-width and may break on small terminals (< 120 columns).
- **Workaround**: Game displays warning on startup if terminal width < 120 or height < 30.
- **Future Enhancement**: Responsive layouts with dynamic column counts based on terminal size.

### L-2: No Animation Support
- **Issue**: Immediate mode rendering with full clears makes smooth animations impractical.
- **Impact**: Visual effects are limited to color changes and delays (no sliding, fading, etc.).
- **Future Enhancement**: Retained mode sub-regions for animation (e.g., combat damage numbers).

### L-3: Blocking Visual Effects
- **Issue**: `VisualEffectService.ApplyBorderFlash()` uses blocking delays, freezing input during flash.
- **Impact**: 50-250ms input lag during damage events.
- **Future Enhancement**: Async/await pattern with non-blocking delays.

### L-4: No Cross-Renderer Communication
- **Issue**: Renderers cannot directly influence each other (e.g., Combat screen cannot trigger minimap update).
- **Workaround**: All cross-screen state changes go through GameState → ViewModel rebuild → Re-render.
- **Rationale**: Maintains immutability guarantees and unidirectional data flow.

---

## Use Cases

### UC-1: Combat Screen - Tactical Grid Rendering

**Scenario**: Player enters combat. System renders 4-row tactical grid showing player/enemy positions.

**Actors**: GameService, CombatScreenRenderer, CombatGridRenderer, IThemeService

**Sequence**:
```
1. GameService detects GameState.Phase == GamePhase.Combat
2. GameService calls _combatService.GetViewModel() → CombatViewModel
3. GameService calls _combatRenderer.Render(viewModel)
4. CombatScreenRenderer:
   a. Calls AnsiConsole.Clear()
   b. Creates 3-section layout (Header, Main, Footer)
   c. Delegates grid rendering to CombatGridRenderer
   d. CombatGridRenderer builds 4×8 Grid with combatant positions
   e. Applies theme colors (PlayerColor, EnemyColor, HealthCritical, etc.)
   f. Returns Grid to CombatScreenRenderer
5. CombatScreenRenderer writes final layout to terminal
```

**Code**:
```csharp
// GameService.RunAsync() (lines 60-99)
if (_state.Phase == GamePhase.Combat && _combatRenderer != null)
{
    var viewModel = _combatService.GetViewModel();
    if (viewModel != null)
        _combatRenderer.Render(viewModel);
}

// CombatScreenRenderer.Render()
public void Render(CombatViewModel vm)
{
    AnsiConsole.Clear();

    var layout = new Layout("Root")
        .SplitRows(
            new Layout("Header").Size(3),
            new Layout("Main").Ratio(1),
            new Layout("Footer").Size(2)
        );

    // Header: Combat round counter
    var header = new Panel($"[yellow]Combat - Round {vm.RoundNumber}[/]")
        .BorderColor(Color.Yellow);

    // Main: 4-row tactical grid (delegated to sub-renderer)
    var grid = _gridRenderer.Render(vm);

    // Footer: Combat log (last 3 entries)
    var log = new Panel(string.Join("\n", vm.CombatLog.TakeLast(3)))
        .Header("[white]Combat Log[/]");

    layout["Header"].Update(header);
    layout["Main"].Update(grid);
    layout["Footer"].Update(log);

    AnsiConsole.Write(layout);
}

// CombatGridRenderer.Render()
public Grid Render(CombatViewModel vm)
{
    var grid = new Grid();
    for (int i = 0; i < 8; i++)
        grid.AddColumn(new GridColumn().Width(12));

    // 4 rows: Back, Mid-Back, Mid-Front, Front
    var rowLabels = new[] { "Back", "Mid-Back", "Mid-Front", "Front" };

    for (int row = 0; row < 4; row++)
    {
        var cells = new List<string> { $"[dim]{rowLabels[row]}[/]" };

        for (int col = 0; col < 8; col++)
        {
            var cellData = vm.Grid.GetCellAt(row, col);

            if (cellData?.Combatant != null)
            {
                var color = cellData.Combatant.IsPlayer ?
                            _theme.GetColor("PlayerColor") :
                            _theme.GetColor("EnemyColor");

                var hpColor = GetHpColor(cellData.Combatant.HpPercent);

                cells.Add($"[{color}]{cellData.Combatant.Name}[/]\n[{hpColor}]HP: {cellData.Combatant.CurrentHp}/{cellData.Combatant.MaxHp}[/]");
            }
            else
            {
                cells.Add("[grey]-[/]"); // Empty cell
            }
        }

        grid.AddRow(cells.ToArray());
    }

    return grid;
}

private string GetHpColor(double hpPercent)
{
    return hpPercent > 0.5 ? _theme.GetColor("HealthFull") :
           hpPercent > 0.25 ? _theme.GetColor("HealthLow") :
                              _theme.GetColor("HealthCritical");
}
```

**Output** (ASCII representation):
```
┌─────────────────────────────────────────────────────────────┐
│               Combat - Round 3                             │
└─────────────────────────────────────────────────────────────┘
┌────────┬────────┬────────┬────────┬────────┬────────┬────────┬────────┐
│ Back   │ -      │ -      │ Ranger │ -      │ -      │ -      │ -      │
│        │        │        │ HP: 45/60                │        │        │
├────────┼────────┼────────┼────────┼────────┼────────┼────────┼────────┤
│Mid-Back│ -      │ Goblin │ -      │ Goblin │ -      │ -      │ -      │
│        │        │ HP: 18/30       │ HP: 22/30       │        │        │
├────────┼────────┼────────┼────────┼────────┼────────┼────────┼────────┤
│Mid-Frnt│ Warrior│ -      │ -      │ -      │ -      │ Orc    │ -      │
│        │ HP: 80/80       │        │        │        │ HP: 55/70       │
├────────┼────────┼────────┼────────┼────────┼────────┼────────┼────────┤
│ Front  │ -      │ -      │ -      │ -      │ -      │ -      │ -      │
└────────┴────────┴────────┴────────┴────────┴────────┴────────┴────────┘
┌─────────────────────────────────────────────────────────────┐
│ Combat Log                                                  │
├─────────────────────────────────────────────────────────────┤
│ Warrior attacks Orc for 15 damage!                         │
│ Goblin attacks Ranger for 8 damage!                        │
│ Ranger uses Aimed Shot on Goblin for 12 damage!            │
└─────────────────────────────────────────────────────────────┘
```

**Validation**: Combat grid correctly displays all combatants in their tactical positions with color-coded HP values.

---

### UC-2: Initiative Timeline - 8-Slot Turn Projection

**Scenario**: During combat, display next 8 turns in initiative order with current turn highlighted.

**Actors**: CombatScreenRenderer, TimelineRenderer, IThemeService

**Sequence**:
```
1. CombatViewModel contains TurnOrder list (sorted by initiative descending)
2. TimelineRenderer.Render() receives CombatViewModel
3. Iterates first 8 entries in TurnOrder
4. Highlights current turn (index 0) with "▶" indicator
5. Applies PlayerColor/EnemyColor theme roles
6. Returns Panel with timeline text
```

**Code**:
```csharp
public Panel Render(CombatViewModel vm)
{
    var timeline = new StringBuilder();
    timeline.AppendLine("[yellow]Next 8 Turns:[/]\n");

    for (int i = 0; i < 8 && i < vm.TurnOrder.Count; i++)
    {
        var combatant = vm.TurnOrder[i];
        var color = combatant.IsPlayer ? _theme.GetColor("PlayerColor") :
                                        _theme.GetColor("EnemyColor");

        var indicator = (i == 0) ? "▶ " : "  "; // Highlight current turn
        var status = combatant.IsStunned ? " [red](STUNNED)[/]" : "";

        timeline.AppendLine($"{indicator}[{color}]{combatant.Name}[/] (Init: {combatant.Initiative}){status}");
    }

    return new Panel(timeline.ToString())
        .Header("[yellow]Initiative Timeline[/]")
        .BorderColor(Color.Yellow)
        .Expand();
}
```

**Output** (ASCII representation):
```
┌─────────────────────────────┐
│ Initiative Timeline         │
├─────────────────────────────┤
│ Next 8 Turns:               │
│                             │
│ ▶ Ranger (Init: 18)         │
│   Goblin (Init: 15)         │
│   Warrior (Init: 14)        │
│   Orc (Init: 12)            │
│   Goblin (Init: 10)         │
│   Ranger (Init: 8)          │
│   Warrior (Init: 6)         │
│   Orc (Init: 4)             │
└─────────────────────────────┘
```

**Validation**: Current turn (Ranger, Init 18) is highlighted with ▶ indicator. All combatants displayed in descending initiative order.

---

### UC-3: Exploration Screen - 3-Pane Layout with Minimap

**Scenario**: Player explores dungeon. System renders current room description, 3×3 minimap, and status bars.

**Actors**: ExplorationScreenRenderer, MinimapRenderer, RoomRenderer, StatusWidget, IThemeService

**Sequence**:
```
1. GameService calls _explorationRenderer.Render(explorationViewModel)
2. ExplorationScreenRenderer creates 3-row layout (Header, Main, Footer)
3. Main splits into Content (3/4 width) and Sidebar (1/4 width)
4. Content: RoomRenderer displays room name, description, exits, objects
5. Sidebar: MinimapRenderer displays 3×3 grid of adjacent rooms
6. Footer: StatusWidget displays HP/Stamina bars
7. All components apply theme colors (RoomNameColor, ExitColor, MinimapColor, etc.)
```

**Code**:
```csharp
public void Render(ExplorationViewModel vm)
{
    AnsiConsole.Clear();

    var layout = new Layout("Root")
        .SplitRows(
            new Layout("Header").Size(5),
            new Layout("Main").Ratio(1),
            new Layout("Footer").Size(3)
        );

    // Main: Content (room) + Sidebar (minimap)
    layout["Main"].SplitColumns(
        new Layout("Content").Ratio(3),
        new Layout("Sidebar").Ratio(1)
    );

    // Header: Room name
    var header = new Panel($"[{_theme.GetColor("RoomNameColor")}]{vm.RoomName}[/]")
        .BorderColor(Color.Parse(_theme.GetColor("PanelBorder")));

    // Content: Room details (delegated to RoomRenderer)
    var roomPanel = _roomRenderer.Render(vm);

    // Sidebar: Minimap (delegated to MinimapRenderer)
    var minimapPanel = _minimapRenderer.Render(vm);

    // Footer: Status bars (delegated to StatusWidget)
    var statusPanel = _statusWidget.Render(vm.CurrentHp, vm.MaxHp, vm.CurrentStamina, vm.MaxStamina);

    layout["Header"].Update(header);
    layout["Content"].Update(roomPanel);
    layout["Sidebar"].Update(minimapPanel);
    layout["Footer"].Update(statusPanel);

    AnsiConsole.Write(layout);
}
```

**RoomRenderer Implementation**:
```csharp
public Panel Render(ExplorationViewModel vm)
{
    var content = new StringBuilder();

    // Description
    content.AppendLine(vm.RoomDescription);
    content.AppendLine();

    // Exits
    if (vm.Exits.Any())
    {
        content.AppendLine($"[{_theme.GetColor("ExitColor")}]Exits:[/]");
        foreach (var exit in vm.Exits)
            content.AppendLine($"  - {exit.Direction}: {exit.Destination}");
        content.AppendLine();
    }

    // Objects
    if (vm.VisibleObjects.Any())
    {
        content.AppendLine($"[{_theme.GetColor("ObjectColor")}]Objects:[/]");
        foreach (var obj in vm.VisibleObjects)
            content.AppendLine($"  - {obj.Name}");
    }

    return new Panel(content.ToString())
        .Header("[white]Room Details[/]")
        .BorderColor(Color.White);
}
```

**MinimapRenderer Implementation**:
```csharp
public Panel Render(ExplorationViewModel vm)
{
    var grid = new Grid();
    for (int i = 0; i < 3; i++)
        grid.AddColumn(new GridColumn().Width(8));

    // 3×3 grid: NW, N, NE, W, [You], E, SW, S, SE
    var directions = new[] { "NW", "N", "NE", "W", "[You]", "E", "SW", "S", "SE" };

    for (int row = 0; row < 3; row++)
    {
        var cells = new List<string>();
        for (int col = 0; col < 3; col++)
        {
            int index = row * 3 + col;
            var room = vm.Minimap[index];

            if (row == 1 && col == 1)
            {
                cells.Add($"[{_theme.GetColor("PlayerColor")}]@[/]"); // Current room
            }
            else if (room?.HasBeenVisited == true)
            {
                cells.Add($"[{_theme.GetColor("VisitedRoomColor")}]{directions[index]}[/]");
            }
            else if (room != null)
            {
                cells.Add($"[{_theme.GetColor("UnvisitedRoomColor")}]?[/]");
            }
            else
            {
                cells.Add("[grey]·[/]"); // No room
            }
        }
        grid.AddRow(cells.ToArray());
    }

    return new Panel(grid)
        .Header("[cyan]Minimap[/]")
        .BorderColor(Color.Cyan);
}
```

**Output** (ASCII representation):
```
┌──────────────────────────────────────────────────────────────────────┐
│                         Crumbling Shrine                            │
└──────────────────────────────────────────────────────────────────────┘
┌───────────────────────────────────────┬──────────────────────────────┐
│ Room Details                          │ Minimap                      │
├───────────────────────────────────────┼──────────────────────────────┤
│ Ancient pillars rise from a dusty     │ ┌────┬────┬────┐            │
│ floor, their surfaces etched with     │ │ ·  │ N  │ ·  │            │
│ forgotten runes. A faint glow pulses  │ ├────┼────┼────┤            │
│ from the altar at the room's center.  │ │ W  │ @  │ E  │            │
│                                        │ ├────┼────┼────┤            │
│ Exits:                                 │ │ ·  │ S  │ ·  │            │
│   - North: Collapsed Hallway           │ └────┴────┴────┘            │
│   - East: Dusty Antechamber            │                              │
│   - South: Winding Passage             │                              │
│                                        │                              │
│ Objects:                               │                              │
│   - Ancient Altar                      │                              │
│   - Broken Vase                        │                              │
└───────────────────────────────────────┴──────────────────────────────┘
┌──────────────────────────────────────────────────────────────────────┐
│ Status                                                               │
├──────────────────────────────────────────────────────────────────────┤
│ HP:      ████████████████████████████  80/80                        │
│ Stamina: ████████████████              45/60                        │
└──────────────────────────────────────────────────────────────────────┘
```

**Validation**:
- Room description formatted with proper line breaks
- Exits displayed with color coding
- Minimap shows current position (@) with adjacent rooms
- Status bars color-coded (green HP, blue Stamina)

---

### UC-4: Victory Screen - XP Gain Summary with Loot Table

**Scenario**: Combat ends. System displays XP earned, loot dropped, and legend progress.

**Actors**: VictoryScreenRenderer, IThemeService

**Sequence**:
```
1. GameService transitions to GamePhase.Victory
2. GameService calls _victoryRenderer.Render(victoryViewModel)
3. VictoryScreenRenderer creates layout with:
   a. Header: "VICTORY!" banner
   b. XP Section: Base XP + bonuses + total
   c. Loot Section: Table of dropped items with quality color coding
   d. Legend Section: Progress toward next legend milestone
4. All sections apply theme colors (XpColor, QualityLegendary, LegendColor, etc.)
```

**Code**:
```csharp
public void Render(VictoryViewModel vm)
{
    AnsiConsole.Clear();

    var layout = new Layout("Root")
        .SplitRows(
            new Layout("Banner").Size(5),
            new Layout("XP").Size(8),
            new Layout("Loot").Ratio(1),
            new Layout("Legend").Size(4)
        );

    // Banner
    var banner = new FigletText("VICTORY!")
        .Color(Color.Parse(_theme.GetColor("VictoryColor")));

    // XP Section
    var xpPanel = new Panel(BuildXpSummary(vm))
        .Header($"[{_theme.GetColor("XpColor")}]Experience Gained[/]")
        .BorderColor(Color.Parse(_theme.GetColor("XpColor")));

    // Loot Section
    var lootTable = BuildLootTable(vm.LootDrops);

    // Legend Section
    var legendPanel = new Panel($"Legend Progress: {vm.LegendProgress}% to [{_theme.GetColor("LegendColor")}]Next Milestone[/]")
        .BorderColor(Color.Parse(_theme.GetColor("LegendColor")));

    layout["Banner"].Update(banner);
    layout["XP"].Update(xpPanel);
    layout["Loot"].Update(lootTable);
    layout["Legend"].Update(legendPanel);

    AnsiConsole.Write(layout);
}

private string BuildXpSummary(VictoryViewModel vm)
{
    var summary = new StringBuilder();
    summary.AppendLine($"Base XP:        +{vm.BaseXp}");

    if (vm.BonusXp > 0)
        summary.AppendLine($"Bonus XP:       +{vm.BonusXp} (flawless victory)");

    summary.AppendLine($"─────────────────────");
    summary.AppendLine($"Total XP:       +{vm.TotalXp}");
    summary.AppendLine();
    summary.AppendLine($"Current Level:  {vm.CurrentLevel}");
    summary.AppendLine($"Next Level:     {vm.XpToNextLevel} XP remaining");

    return summary.ToString();
}

private Table BuildLootTable(List<LootItemView> lootDrops)
{
    var table = new Table()
        .Title($"[{_theme.GetColor("LootColor")}]Loot Acquired[/]")
        .BorderColor(Color.Parse(_theme.GetColor("LootColor")))
        .AddColumn("Item")
        .AddColumn("Quality")
        .AddColumn("Type");

    foreach (var item in lootDrops)
    {
        var qualityColor = _theme.GetColor($"Quality{item.Quality}");
        table.AddRow(
            $"[{qualityColor}]{item.Name}[/]",
            $"[{qualityColor}]{item.Quality}[/]",
            item.ItemType
        );
    }

    return table;
}
```

**Output** (ASCII representation):
```
┌──────────────────────────────────────────────────────────────────────┐
│                                                                      │
│  ██╗   ██╗██╗ ██████╗████████╗ ██████╗ ██████╗ ██╗   ██╗           │
│  ██║   ██║██║██╔════╝╚══██╔══╝██╔═══██╗██╔══██╗╚██╗ ██╔╝           │
│  ██║   ██║██║██║        ██║   ██║   ██║██████╔╝ ╚████╔╝            │
│  ╚██╗ ██╔╝██║██║        ██║   ██║   ██║██╔══██╗  ╚██╔╝             │
│   ╚████╔╝ ██║╚██████╗   ██║   ╚██████╔╝██║  ██║   ██║              │
│    ╚═══╝  ╚═╝ ╚═════╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝   ╚═╝              │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
┌──────────────────────────────────────────────────────────────────────┐
│ Experience Gained                                                    │
├──────────────────────────────────────────────────────────────────────┤
│ Base XP:        +350                                                 │
│ Bonus XP:       +50 (flawless victory)                               │
│ ─────────────────────                                                │
│ Total XP:       +400                                                 │
│                                                                      │
│ Current Level:  7                                                    │
│ Next Level:     200 XP remaining                                     │
└──────────────────────────────────────────────────────────────────────┘
┌──────────────────────────────────────────────────────────────────────┐
│ Loot Acquired                                                        │
├──────────────────────────────────┬──────────────┬────────────────────┤
│ Item                             │ Quality      │ Type               │
├──────────────────────────────────┼──────────────┼────────────────────┤
│ Runic Longsword                  │ Legendary    │ Weapon             │
│ Orc Hide Armor                   │ Common       │ Armor              │
│ Health Potion (x3)               │ Common       │ Consumable         │
│ Enchanted Ring of Warding        │ Rare         │ Accessory          │
└──────────────────────────────────┴──────────────┴────────────────────┘
┌──────────────────────────────────────────────────────────────────────┐
│ Legend Progress: 68% to Next Milestone                               │
└──────────────────────────────────────────────────────────────────────┘
```

**Validation**:
- XP summary calculates base + bonus correctly
- Loot table uses quality color coding (Legendary = gold, Rare = purple, Common = white)
- Legend progress displays percentage correctly

---

### UC-5: Inventory Screen - Item Table with Equipped Indicators

**Scenario**: Player opens inventory. System displays all items with equipped status, quality, and slot assignments.

**Actors**: InventoryScreenRenderer, IThemeService

**Sequence**:
```
1. GameService transitions to GamePhase.Inventory
2. GameService calls _inventoryRenderer.Render(inventoryViewModel)
3. InventoryScreenRenderer creates table with columns: Item, Quality, Equipped Slot, Weight
4. Iterates vm.Items and applies quality color coding
5. Displays encumbrance warning if over capacity
```

**Code**:
```csharp
public void Render(InventoryViewModel vm)
{
    AnsiConsole.Clear();

    var layout = new Layout("Root")
        .SplitRows(
            new Layout("Header").Size(3),
            new Layout("Table").Ratio(1),
            new Layout("Footer").Size(2)
        );

    // Header: Character name + encumbrance
    var encumbranceColor = vm.CurrentWeight > vm.MaxWeight ? "red" : "white";
    var header = new Panel($"[{_theme.GetColor("PlayerColor")}]{vm.CharacterName}[/] - Inventory\n" +
                           $"Encumbrance: [{encumbranceColor}]{vm.CurrentWeight}/{vm.MaxWeight}[/]")
        .BorderColor(Color.White);

    // Table: Items
    var table = new Table()
        .AddColumn("Item")
        .AddColumn("Quality")
        .AddColumn("Equipped Slot")
        .AddColumn("Weight");

    foreach (var item in vm.Items.OrderByDescending(i => i.Quality))
    {
        var qualityColor = _theme.GetColor($"Quality{item.Quality}");
        var equippedIndicator = item.IsEquipped ? $"[green]{item.EquippedSlot}[/]" : "-";

        table.AddRow(
            $"[{qualityColor}]{item.Name}[/]",
            $"[{qualityColor}]{item.Quality}[/]",
            equippedIndicator,
            item.Weight.ToString()
        );
    }

    // Footer: Commands
    var footer = new Panel("[cyan]E[/]quip | [cyan]D[/]rop | [cyan]U[/]se | [cyan]Esc[/] Back")
        .BorderColor(Color.Cyan);

    layout["Header"].Update(header);
    layout["Table"].Update(table);
    layout["Footer"].Update(footer);

    AnsiConsole.Write(layout);
}
```

**Output** (ASCII representation):
```
┌──────────────────────────────────────────────────────────────────────┐
│ Ranger Athel - Inventory                                            │
│ Encumbrance: 45/60                                                   │
└──────────────────────────────────────────────────────────────────────┘
┌────────────────────────────┬──────────┬───────────────┬──────────────┐
│ Item                       │ Quality  │ Equipped Slot │ Weight       │
├────────────────────────────┼──────────┼───────────────┼──────────────┤
│ Runic Longsword            │ Legendary│ Main Hand     │ 5            │
│ Enchanted Ring of Warding  │ Rare     │ Ring 1        │ 0.1          │
│ Orc Hide Armor             │ Common   │ Body          │ 12           │
│ Health Potion (x3)         │ Common   │ -             │ 1.5          │
│ Rusty Dagger               │ Poor     │ -             │ 2            │
└────────────────────────────┴──────────┴───────────────┴──────────────┘
┌──────────────────────────────────────────────────────────────────────┐
│ Equip | Drop | Use | Esc Back                                        │
└──────────────────────────────────────────────────────────────────────┘
```

**Validation**:
- Items sorted by quality (Legendary → Poor)
- Equipped items show green slot indicators
- Encumbrance displays current/max weight ratio

---

### UC-6: Crafting Modal - Recipe Browser with Material Requirements

**Scenario**: Player opens crafting menu. System displays available recipes with material requirements and craftability status.

**Actors**: CraftingScreenRenderer, IThemeService

**Sequence**:
```
1. Player triggers crafting command
2. GameService calls _craftingRenderer.Render(craftingViewModel)
3. CraftingScreenRenderer creates table with columns: Recipe, Materials, Craftable
4. Iterates vm.Recipes and checks material availability
5. Highlights craftable recipes in green, non-craftable in red
```

**Code**:
```csharp
public void Render(CraftingViewModel vm)
{
    AnsiConsole.Clear();

    var layout = new Layout("Root")
        .SplitRows(
            new Layout("Header").Size(3),
            new Layout("Recipes").Ratio(1),
            new Layout("Footer").Size(2)
        );

    // Header: Crafter name + trade
    var header = new Panel($"[{_theme.GetColor("PlayerColor")}]{vm.CharacterName}[/] - Crafting ({vm.SelectedTrade})\n" +
                           $"Crafting Skill: {vm.CrafterWits}")
        .BorderColor(Color.Parse(_theme.GetColor("CraftingColor")));

    // Recipes Table
    var table = new Table()
        .AddColumn("Recipe")
        .AddColumn("Materials Required")
        .AddColumn("Craftable");

    foreach (var recipe in vm.Recipes)
    {
        var materialsText = string.Join(", ", recipe.Materials.Select(m =>
            $"{m.Name} x{m.Quantity} ({(m.HasEnough ? "✓" : "✗")})"));

        var craftableColor = recipe.CanCraft ? "green" : "red";
        var craftableText = recipe.CanCraft ? "YES" : "NO";

        table.AddRow(
            $"[{_theme.GetColor("RecipeColor")}]{recipe.Name}[/]",
            materialsText,
            $"[{craftableColor}]{craftableText}[/]"
        );
    }

    // Footer: Commands
    var footer = new Panel("[cyan]C[/]raft | [cyan]T[/]rade | [cyan]Esc[/] Back")
        .BorderColor(Color.Cyan);

    layout["Header"].Update(header);
    layout["Recipes"].Update(table);
    layout["Footer"].Update(footer);

    AnsiConsole.Write(layout);
}
```

**Output** (ASCII representation):
```
┌──────────────────────────────────────────────────────────────────────┐
│ Ranger Athel - Crafting (Blacksmithing)                             │
│ Crafting Skill: 12                                                   │
└──────────────────────────────────────────────────────────────────────┘
┌────────────────────┬──────────────────────────────────┬──────────────┐
│ Recipe             │ Materials Required               │ Craftable    │
├────────────────────┼──────────────────────────────────┼──────────────┤
│ Iron Longsword     │ Iron Ingot x3 (✓), Leather x1 (✓)│ YES          │
│ Steel Dagger       │ Steel Ingot x2 (✗), Wood x1 (✓)  │ NO           │
│ Runic Blade        │ Mithril x1 (✗), Rune Shard x1 (✗)│ NO           │
└────────────────────┴──────────────────────────────────┴──────────────┘
┌──────────────────────────────────────────────────────────────────────┐
│ Craft | Trade | Esc Back                                             │
└──────────────────────────────────────────────────────────────────────┘
```

**Validation**:
- Craftable recipes (Iron Longsword) show green "YES"
- Non-craftable recipes (Steel Dagger, Runic Blade) show red "NO"
- Material availability indicated with ✓/✗ symbols

---

## Decision Trees

### DT-1: Screen Rendering Dispatch

**Trigger**: GameService.RunAsync() detects GameState.Phase change

```
GameState.Phase?
│
├─ MainMenu
│  └─ Render main menu (options: New Game, Continue, Options, Quit)
│
├─ Exploration
│  ├─ Has _explorationRenderer?
│  │  ├─ YES → Build ExplorationViewModel → Render(vm)
│  │  └─ NO  → Log error, skip rendering
│  └─ Return to input loop
│
├─ Combat
│  ├─ Has _combatRenderer?
│  │  ├─ YES → Build CombatViewModel → Render(vm)
│  │  └─ NO  → Log error, skip rendering
│  └─ Return to input loop
│
├─ Inventory
│  ├─ Has _inventoryRenderer?
│  │  ├─ YES → Build InventoryViewModel → Render(vm)
│  │  └─ NO  → Log error, skip rendering
│  └─ Return to input loop
│
├─ Journal
│  ├─ Has _journalRenderer?
│  │  ├─ YES → Build JournalViewModel → Render(vm)
│  │  └─ NO  → Log error, skip rendering
│  └─ Return to input loop
│
├─ Crafting
│  ├─ Has _craftingRenderer?
│  │  ├─ YES → Build CraftingViewModel → Render(vm)
│  │  └─ NO  → Log error, skip rendering
│  └─ Return to input loop
│
├─ Options
│  ├─ Has _optionsRenderer?
│  │  ├─ YES → Build OptionsViewModel → Render(vm)
│  │  └─ NO  → Log error, skip rendering
│  └─ Return to input loop
│
├─ Rest
│  ├─ Has _restRenderer?
│  │  ├─ YES → Build RestViewModel → Render(vm)
│  │  └─ NO  → Log error, skip rendering
│  └─ Return to input loop
│
├─ Victory
│  ├─ Has _victoryRenderer?
│  │  ├─ YES → Build VictoryViewModel → Render(vm)
│  │  └─ NO  → Log error, skip rendering
│  └─ Return to input loop
│
└─ Quit
   └─ Exit game loop
```

**Key Points**:
- Null checks prevent crashes when renderers are unavailable (headless testing)
- Each phase builds fresh ViewModel from current GameState
- Rendering is synchronous (blocks until complete)

---

### DT-2: Sub-Renderer Composition Flow

**Trigger**: Screen renderer receives ViewModel and delegates to sub-renderers

```
CombatScreenRenderer.Render(CombatViewModel vm)
│
├─ Clear terminal (AnsiConsole.Clear())
│
├─ Create root Layout (3 rows: Header, Main, Footer)
│
├─ Header Section
│  └─ Build Panel with combat round counter
│     └─ Apply theme color (YellowColor)
│
├─ Main Section (Combat Grid)
│  ├─ Delegate to CombatGridRenderer.Render(vm)
│  │  ├─ Create 4×8 Grid
│  │  ├─ Iterate 4 rows (Back, Mid-Back, Mid-Front, Front)
│  │  │  └─ For each cell:
│  │  │     ├─ Has combatant?
│  │  │     │  ├─ YES → Format combatant (name, HP, color)
│  │  │     │  │  ├─ IsPlayer? → Apply PlayerColor
│  │  │     │  │  └─ IsEnemy? → Apply EnemyColor
│  │  │     │  └─ Get HP color (HealthFull/Low/Critical based on %)
│  │  │     └─ NO  → Display "-" (empty cell)
│  │  └─ Return Grid
│  └─ Update Main layout with Grid
│
├─ Sidebar Section (Initiative Timeline)
│  ├─ Delegate to TimelineRenderer.Render(vm)
│  │  ├─ Iterate first 8 combatants in vm.TurnOrder
│  │  │  ├─ Current turn (index 0)?
│  │  │  │  ├─ YES → Prefix with "▶ " indicator
│  │  │  │  └─ NO  → Prefix with "  " (2 spaces)
│  │  │  ├─ IsPlayer? → Apply PlayerColor
│  │  │  └─ IsEnemy? → Apply EnemyColor
│  │  │  └─ Append status effects (stunned, poisoned, etc.)
│  │  └─ Return Panel with timeline text
│  └─ Update Sidebar layout with Panel
│
├─ Footer Section (Combat Log)
│  ├─ Extract last 3 entries from vm.CombatLog
│  ├─ Build Panel with log text
│  └─ Update Footer layout with Panel
│
└─ Write final layout to terminal (AnsiConsole.Write(layout))
```

**Key Points**:
- Sub-renderers return Spectre.Console objects (Panel, Grid, Table), not void
- Sub-renderers NEVER call AnsiConsole.Clear() or AnsiConsole.Write()
- Theme color application occurs at leaf level (cell formatting)

---

### DT-3: Visual Effect Timing Flow

**Trigger**: Game event requires visual feedback (damage, healing, critical hit)

```
Combat Event (e.g., Orc takes 25 damage)
│
├─ Update GameState (Orc.CurrentHp -= 25)
│
├─ Determine visual effect intensity
│  ├─ Damage magnitude?
│  │  ├─ 1-10 damage  → intensity = 1 (50ms)
│  │  ├─ 11-20 damage → intensity = 2 (100ms)
│  │  ├─ 21-30 damage → intensity = 3 (150ms)
│  │  ├─ 31-40 damage → intensity = 4 (200ms)
│  │  └─ 41+ damage   → intensity = 5 (250ms)
│
├─ Build CombatViewModel (includes updated Orc HP)
│
├─ Render combat screen (AnsiConsole.Clear() → Layout → Write)
│
├─ Identify Orc's panel in layout
│
├─ Apply visual effect
│  ├─ IVisualEffectService.ApplyBorderFlash(orcPanel, intensity: 3)
│  │  ├─ Store original border color (White)
│  │  ├─ Change border color to Red
│  │  ├─ Thread.Sleep(intensity * 50) // 150ms
│  │  └─ Restore border color to White
│
└─ Return to input loop
```

**Limitations**:
- Blocking delay freezes input during flash
- Visual effect applied AFTER full screen render (not during)
- Future enhancement: Async delays with cancellation tokens

---

## Cross-Links

### Dependencies (Systems SPEC-RENDER-001 relies on)

1. **SPEC-UI-001 (UI Framework System)**
   - **Relationship**: Renderers consume immutable ViewModels produced by UI Framework
   - **Integration Point**: Each renderer's `Render(TViewModel)` method receives ViewModel from service builders
   - **Example**: `CombatScreenRenderer.Render(CombatViewModel vm)` where `vm = _combatService.GetViewModel()`

2. **SPEC-THEME-001 (Theme System)**
   - **Relationship**: Renderers retrieve all color values via IThemeService.GetColor()
   - **Integration Point**: Every markup string uses semantic color roles (PlayerColor, HealthCritical, etc.)
   - **Example**: `$"[{_theme.GetColor("PlayerColor")}]{player.Name}[/]"`

3. **SPEC-COMBAT-001 (Combat System)**
   - **Relationship**: Combat renderers display tactical grid, initiative timeline, combat log
   - **Integration Point**: CombatViewModel contains all combat state (combatant positions, turn order, damage events)
   - **Example**: `_gridRenderer.Render(vm)` uses `vm.Grid` (4×8 tactical grid data)

---

### Dependents (Systems that rely on SPEC-RENDER-001)

1. **GameService (Game Loop Orchestration)**
   - **Relationship**: GameService triggers rendering after each player action
   - **Integration Point**: `if (_state.Phase == GamePhase.Combat) _combatRenderer?.Render(vm);`
   - **File**: RuneAndRust.Engine/Services/GameService.cs (lines 60-99)

2. **SPEC-INPUT-001 (Input Handling System)**
   - **Relationship**: Input commands trigger state changes → ViewModel rebuild → Re-render
   - **Integration Point**: CommandParser modifies GameState → GameService detects change → Renders updated screen
   - **Example**: Player enters "attack orc" → Combat state updates → Combat screen re-renders with new HP values

---

### Related Systems

1. **SPEC-CHAR-001 (Character System)**
   - **Relationship**: Character stats displayed in status bars, inventory screens, combat panels
   - **Integration Point**: CharacterViewModel data flows into ExplorationViewModel, CombatViewModel, InventoryViewModel
   - **Example**: HP/Stamina bars rendered by StatusWidget using character stats

2. **SPEC-LOOT-001 (Loot System)**
   - **Relationship**: Loot drops displayed in Victory screen, Inventory screen
   - **Integration Point**: VictoryViewModel.LootDrops, InventoryViewModel.Items
   - **Example**: Victory screen renders loot table with quality color coding

3. **SPEC-XP-001 (Experience System)**
   - **Relationship**: XP gain displayed in Victory screen
   - **Integration Point**: VictoryViewModel.BaseXp, BonusXp, TotalXp
   - **Example**: Victory screen calculates XP breakdown and displays level progress

---

## Related Services

### Core Services (from RuneAndRust.Core/Interfaces/)

1. **ICombatScreenRenderer** → `CombatScreenRenderer` (RuneAndRust.Terminal/Services/)
2. **IExplorationScreenRenderer** → `ExplorationScreenRenderer` (RuneAndRust.Terminal/Rendering/)
3. **IInventoryScreenRenderer** → `InventoryScreenRenderer` (RuneAndRust.Terminal/Services/)
4. **IJournalScreenRenderer** → `JournalScreenRenderer` (RuneAndRust.Terminal/Services/)
5. **ICraftingScreenRenderer** → `CraftingScreenRenderer` (RuneAndRust.Terminal/Services/)
6. **IOptionsScreenRenderer** → `OptionsScreenRenderer` (RuneAndRust.Terminal/Services/)
7. **IRestScreenRenderer** → `RestScreenRenderer` (RuneAndRust.Terminal/Services/)
8. **IVictoryScreenRenderer** → `VictoryScreenRenderer` (RuneAndRust.Terminal/Services/)
9. **IVisualEffectService** → `VisualEffectService` (RuneAndRust.Terminal/Services/)
10. **IThemeService** → `ThemeService` (RuneAndRust.Terminal/Services/) - See SPEC-THEME-001

### Sub-Renderers (from RuneAndRust.Terminal/Rendering/)

1. **CombatGridRenderer** - Renders 4×8 tactical grid with combatant positions
2. **TimelineRenderer** - Renders 8-slot initiative timeline
3. **MinimapRenderer** - Renders 3×3 room grid for exploration
4. **RoomRenderer** - Renders current room description with exits/objects
5. **StatusWidget** - Renders HP/Stamina bars with color thresholds

---

## Data Models

### Screen-Specific ViewModels (from RuneAndRust.Core/ViewModels/)

All ViewModels are immutable C# records. See SPEC-UI-001 for full ViewModel specifications.

1. **CombatViewModel** - Combat screen data
   - `string CharacterName`
   - `int RoundNumber`
   - `List<CombatantView> TurnOrder`
   - `Grid<GridCell> Grid` (4×8 tactical grid)
   - `List<string> CombatLog`

2. **ExplorationViewModel** - Exploration screen data
   - `string RoomName`
   - `string RoomDescription`
   - `List<ExitView> Exits`
   - `List<ObjectView> VisibleObjects`
   - `RoomSummary[] Minimap` (9 elements: NW, N, NE, W, Center, E, SW, S, SE)
   - `int CurrentHp, MaxHp, CurrentStamina, MaxStamina`

3. **InventoryViewModel** - Inventory screen data
   - `string CharacterName`
   - `List<ItemView> Items`
   - `int CurrentWeight, MaxWeight`

4. **VictoryViewModel** - Victory screen data
   - `int BaseXp, BonusXp, TotalXp`
   - `int CurrentLevel, XpToNextLevel`
   - `List<LootItemView> LootDrops`
   - `int LegendProgress` (percentage to next milestone)

5. **CraftingViewModel** - Crafting modal data
   - `string CharacterName`
   - `int CrafterWits`
   - `CraftingTrade SelectedTrade`
   - `List<RecipeView> Recipes`

6. **OptionsViewModel** - Settings menu data
   - `ThemeType SelectedTheme`
   - `bool ReduceMotion`
   - `TextSpeed TextSpeed`

### Spectre.Console Component Types

1. **Layout** - Hierarchical container for row/column splits
   - `.SplitRows(Layout...)` - Vertical stacking
   - `.SplitColumns(Layout...)` - Horizontal stacking
   - `.Size(int)` - Fixed height/width
   - `.Ratio(int)` - Proportional sizing

2. **Panel** - Bordered box with optional header
   - `.Header(string)` - Title text
   - `.BorderColor(Color)` - Border color
   - `.Expand()` - Fill available space

3. **Table** - Grid with headers and rows
   - `.AddColumn(string)` - Define column
   - `.AddRow(params string[])` - Add data row
   - `.Border(TableBorder)` - Border style

4. **Grid** - Flexible column/row layout
   - `.AddColumn(GridColumn)` - Define column with width
   - `.AddRow(params string[])` - Add row of cells
   - `.Border(GridBorder)` - Border style

5. **BarChart** - Horizontal bar visualization
   - `.AddItem(string label, double value, Color)` - Add bar
   - `.Width(int)` - Chart width

6. **Rule** - Horizontal separator
   - `.Title(string)` - Centered title
   - `.Style(Style)` - Line style

---

## Configuration

### DI Registration (from RuneAndRust.Terminal/Program.cs)

```csharp
// Screen Renderers (Singleton - stateless services)
services.AddSingleton<ICombatScreenRenderer, CombatScreenRenderer>();
services.AddSingleton<IExplorationScreenRenderer, ExplorationScreenRenderer>();
services.AddSingleton<IInventoryScreenRenderer, InventoryScreenRenderer>();
services.AddSingleton<IJournalScreenRenderer, JournalScreenRenderer>();
services.AddSingleton<ICraftingScreenRenderer, CraftingScreenRenderer>();
services.AddSingleton<IOptionsScreenRenderer, OptionsScreenRenderer>();
services.AddSingleton<IRestScreenRenderer, RestScreenRenderer>();
services.AddSingleton<IVictoryScreenRenderer, VictoryScreenRenderer>();

// Sub-Renderers (Singleton)
services.AddSingleton<CombatGridRenderer>();
services.AddSingleton<TimelineRenderer>();
services.AddSingleton<MinimapRenderer>();
services.AddSingleton<RoomRenderer>();
services.AddSingleton<StatusWidget>();

// Supporting Services (Singleton)
services.AddSingleton<IThemeService, ThemeService>();
services.AddSingleton<IVisualEffectService, VisualEffectService>();
```

**Lifetime Justification**:
- **Singleton**: Renderers are stateless. All state comes from ViewModel parameters.
- **No Scoped**: Rendering does not require database access or per-request state.

---

### Spectre.Console NuGet Package

```xml
<PackageReference Include="Spectre.Console" Version="0.54.0" />
```

**Key APIs Used**:
- `AnsiConsole.Clear()` - Clear terminal screen
- `AnsiConsole.Write(IRenderable)` - Write layout/panel/table to terminal
- `Layout` - Hierarchical row/column composition
- `Panel` - Bordered container with header
- `Table` - Data grid with headers/rows
- `Grid` - Flexible cell layout
- `BarChart` - Horizontal bar visualization
- `Color.Parse(string)` - Convert hex color to Spectre.Console Color

---

## Testing

### Unit Testing Strategy

**Test Coverage Target**: 60% (lower than services due to visual nature of rendering)

**Testable Components**:
1. **Sub-Renderers** - Verify correct Panel/Grid/Table construction
2. **Color Application** - Verify theme colors applied to markup strings
3. **ViewModel Handling** - Verify correct data extraction from ViewModels

**Non-Testable Components**:
- Terminal output validation (requires manual visual inspection)
- Layout composition (Spectre.Console internal behavior)

### Example Test: CombatGridRenderer

**File**: RuneAndRust.Tests/Rendering/CombatGridRendererTests.cs

```csharp
public class CombatGridRendererTests
{
    private readonly IThemeService _mockTheme;
    private readonly CombatGridRenderer _renderer;

    public CombatGridRendererTests()
    {
        _mockTheme = Substitute.For<IThemeService>();
        _mockTheme.GetColor(Arg.Any<string>()).Returns("white"); // Default color

        _renderer = new CombatGridRenderer(_mockTheme);
    }

    [Fact]
    public void Render_EmptyGrid_ReturnsGridWith4Rows()
    {
        // Arrange
        var vm = new CombatViewModel(
            CharacterName: "Test",
            RoundNumber: 1,
            TurnOrder: new List<CombatantView>(),
            Grid: new Grid<GridCell>(4, 8), // 4 rows, 8 columns
            CombatLog: new List<string>()
        );

        // Act
        var result = _renderer.Render(vm);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Grid>(result);
        // Note: Cannot easily assert Grid internal structure (Spectre.Console limitation)
    }

    [Fact]
    public void Render_GridWithPlayer_AppliesPlayerColor()
    {
        // Arrange
        _mockTheme.GetColor("PlayerColor").Returns("green");

        var player = new CombatantView(
            Name: "Warrior",
            IsPlayer: true,
            CurrentHp: 80,
            MaxHp: 80,
            HpPercent: 1.0
        );

        var grid = new Grid<GridCell>(4, 8);
        grid.SetCellAt(0, 0, new GridCell { Combatant = player });

        var vm = new CombatViewModel(
            CharacterName: "Test",
            RoundNumber: 1,
            TurnOrder: new List<CombatantView> { player },
            Grid: grid,
            CombatLog: new List<string>()
        );

        // Act
        var result = _renderer.Render(vm);

        // Assert
        _mockTheme.Received(1).GetColor("PlayerColor");
    }

    [Theory]
    [InlineData(1.0, "HealthFull")]
    [InlineData(0.4, "HealthLow")]
    [InlineData(0.2, "HealthCritical")]
    public void Render_AppliesCorrectHpColorThresholds(double hpPercent, string expectedColorRole)
    {
        // Arrange
        var combatant = new CombatantView(
            Name: "Enemy",
            IsPlayer: false,
            CurrentHp: (int)(100 * hpPercent),
            MaxHp: 100,
            HpPercent: hpPercent
        );

        var grid = new Grid<GridCell>(4, 8);
        grid.SetCellAt(0, 0, new GridCell { Combatant = combatant });

        var vm = new CombatViewModel(
            CharacterName: "Test",
            RoundNumber: 1,
            TurnOrder: new List<CombatantView> { combatant },
            Grid: grid,
            CombatLog: new List<string>()
        );

        // Act
        var result = _renderer.Render(vm);

        // Assert
        _mockTheme.Received(1).GetColor(expectedColorRole);
    }
}
```

### Example Test: TimelineRenderer

**File**: RuneAndRust.Tests/Rendering/TimelineRendererTests.cs

```csharp
public class TimelineRendererTests
{
    private readonly IThemeService _mockTheme;
    private readonly TimelineRenderer _renderer;

    public TimelineRendererTests()
    {
        _mockTheme = Substitute.For<IThemeService>();
        _mockTheme.GetColor(Arg.Any<string>()).Returns("white");

        _renderer = new TimelineRenderer(_mockTheme);
    }

    [Fact]
    public void Render_LimitsTo8Combatants()
    {
        // Arrange
        var turnOrder = Enumerable.Range(0, 20)
            .Select(i => new CombatantView($"Combatant{i}", false, 50, 50, 1.0, i))
            .ToList();

        var vm = new CombatViewModel(
            CharacterName: "Test",
            RoundNumber: 1,
            TurnOrder: turnOrder,
            Grid: new Grid<GridCell>(4, 8),
            CombatLog: new List<string>()
        );

        // Act
        var result = _renderer.Render(vm);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Panel>(result);
        // Manual inspection: Panel should contain only 8 entries
    }

    [Fact]
    public void Render_HighlightsCurrentTurnWithIndicator()
    {
        // Arrange
        var turnOrder = new List<CombatantView>
        {
            new("Player", true, 50, 50, 1.0, 20),
            new("Enemy", false, 30, 30, 1.0, 15)
        };

        var vm = new CombatViewModel(
            CharacterName: "Test",
            RoundNumber: 1,
            TurnOrder: turnOrder,
            Grid: new Grid<GridCell>(4, 8),
            CombatLog: new List<string>()
        );

        // Act
        var result = _renderer.Render(vm);

        // Assert
        Assert.NotNull(result);
        // Manual inspection: First entry should have "▶ Player" indicator
    }
}
```

### Integration Testing

**Approach**: Manual visual inspection of terminal output during gameplay.

**Test Scenarios**:
1. Combat screen renders correctly with multiple enemies
2. Exploration screen displays minimap with correct room positions
3. Inventory screen handles 50+ items without layout breaks
4. Victory screen displays XP breakdown and loot table
5. Theme switching (Options → Change Theme) applies colors correctly
6. Visual effects (damage flash) display with correct timing

---

## Design Rationale

### DR-1: Why Immediate Mode Rendering?

**Decision**: Clear entire screen and redraw on every render (no retained mode).

**Alternatives Considered**:
1. **Retained Mode with Dirty Rectangles**: Track which UI regions changed and update only those sections
2. **Frame Buffer**: Maintain in-memory representation of screen and diff before writing

**Rationale for Immediate Mode**:
- **Simplicity**: No state tracking, no dirty rectangle management
- **Determinism**: Identical ViewModel → Identical visual output (critical for testing)
- **Turn-Based Suitability**: Rendering occurs once per player action (not 60 FPS), so full redraws are acceptable
- **Terminal Constraints**: Spectre.Console lacks partial update APIs, full redraw is required anyway

**Trade-Offs**:
- **Performance**: Higher rendering overhead (50ms average for complex screens)
- **Flicker**: Some terminal emulators may flicker during AnsiConsole.Clear() (mitigated by fast redraws)

**Future Consideration**: If migrating to GUI (Avalonia), retained mode with MVVM data binding would be appropriate.

---

### DR-2: Why Sub-Renderer Pattern?

**Decision**: Delegate complex UI sections (Combat Grid, Timeline, Minimap) to specialized sub-renderers.

**Alternatives Considered**:
1. **Monolithic Renderers**: Single Render() method with all layout logic
2. **Helper Methods**: Private methods within screen renderer class

**Rationale for Sub-Renderers**:
- **Code Organization**: Separate classes for distinct UI concerns (Combat Grid vs Timeline vs Minimap)
- **Testability**: Sub-renderers can be unit tested independently
- **Reusability**: StatusWidget used across multiple screens (Exploration, Inventory, Rest)

**Implementation Rules**:
- Sub-renderers return Spectre.Console objects (Panel, Grid, Table), not void
- Sub-renderers NEVER call AnsiConsole.Clear() or AnsiConsole.Write()
- Sub-renderers injected via constructor DI (registered as Singleton)

**Trade-Offs**:
- **Complexity**: More classes to manage (10+ renderer classes vs 8 monolithic renderers)
- **DI Overhead**: More services registered in DI container

---

### DR-3: Why Spectre.Console Over Custom Terminal Library?

**Decision**: Use Spectre.Console 0.54.0 for all terminal rendering.

**Alternatives Considered**:
1. **Raw ANSI Escape Codes**: Manual cursor positioning and color codes
2. **Custom Terminal Abstraction**: Build wrapper around System.Console with layout primitives
3. **Terminal.Gui**: Full TUI framework with widgets and mouse support

**Rationale for Spectre.Console**:
- **Simplicity**: High-level Layout/Panel/Table APIs abstract terminal complexity
- **Cross-Platform**: Works on Windows/macOS/Linux without ANSI code differences
- **Markup Syntax**: `[color]text[/]` is readable and integrates cleanly with theme system
- **Community Support**: Well-maintained NuGet package with active development

**Trade-Offs**:
- **Limited Animation**: No built-in support for smooth transitions or particle effects
- **Terminal Size Constraints**: Layouts break on small terminals (< 120 columns)
- **No Mouse Support**: Keyboard-only input (by design for accessibility)

**Why Not Terminal.Gui?**:
- Overkill for turn-based game (designed for complex business UIs)
- Adds dependency on reactive event loop (conflicts with turn-based game loop)
- Higher learning curve for team

---

## Changelog

### Version 1.0.1 (2025-12-25) - API Alignment

**Fixed**:
- Updated IVisualEffectService signature to async `TriggerEffectAsync` pattern (v0.3.9a)
- Updated IVictoryScreenRenderer to use `CombatResult` parameter instead of `VictoryViewModel`
- Corrected sub-renderer examples to show static method patterns
- Added `last_updated` to YAML frontmatter

**Added**:
- Code traceability remarks to 10+ implementation files
- Documented `SetBorderOverride`/`GetBorderOverride`/`ClearBorderOverride` methods
- Documented `VisualEffectType` enum values

### Version 1.0.0 (2025-01-XX) - Initial Specification

**Added**:
- Comprehensive rendering pipeline documentation for 8 screen types
- Sub-renderer pattern specification (CombatGridRenderer, TimelineRenderer, MinimapRenderer, RoomRenderer, StatusWidget)
- Spectre.Console integration guide (Layout, Panel, Table, Grid, BarChart)
- Visual effect system documentation (border flash with intensity-based duration)
- 6 detailed use cases (Combat grid, Timeline, Exploration, Victory, Inventory, Crafting)
- 3 decision trees (Screen dispatch, Sub-renderer composition, Visual effect timing)
- Testing strategy with example unit tests

**Documented Implementation**:
- All 8 screen renderers (Combat, Exploration, Inventory, Journal, Crafting, Options, Rest, Victory)
- Immediate mode rendering pattern (clear + redraw)
- Theme integration via IThemeService.GetColor()
- DI registration patterns (Singleton lifetime)

---

## Future Enhancements

### FE-1: Responsive Layouts

**Problem**: Current layouts are fixed-width and break on small terminals (< 120 columns).

**Proposed Solution**:
```csharp
public class ResponsiveLayoutService
{
    public Layout BuildCombatLayout(int terminalWidth)
    {
        if (terminalWidth >= 160)
            return BuildWideLayout(); // 3-column: Grid, Timeline, Log
        else if (terminalWidth >= 120)
            return BuildStandardLayout(); // 2-column: Grid, Timeline (Log in footer)
        else
            return BuildCompactLayout(); // 1-column: Grid only (Timeline collapsed)
    }
}
```

**Benefits**: Support terminals down to 80 columns, better mobile SSH support

---

### FE-2: Async Visual Effects

**Problem**: `VisualEffectService.ApplyBorderFlash()` uses blocking `Thread.Sleep`, freezing input.

**Proposed Solution**:
```csharp
public interface IVisualEffectService
{
    Task ApplyBorderFlashAsync(Panel panel, int intensity, CancellationToken ct);
}

public class VisualEffectService : IVisualEffectService
{
    public async Task ApplyBorderFlashAsync(Panel panel, int intensity, CancellationToken ct)
    {
        var originalColor = panel.Border.Color;
        var durationMs = intensity * 50;

        panel.BorderColor(Color.Red);
        await Task.Delay(durationMs, ct);
        panel.BorderColor(originalColor);
    }
}
```

**Benefits**: Non-blocking delays, cancellable effects, smoother gameplay feel

---

### FE-3: Combat Animation System

**Problem**: No support for sliding combatant positions or damage number pop-ups.

**Proposed Solution**:
```csharp
public interface ICombatAnimationService
{
    Task AnimateMovementAsync(CombatantView combatant, Position from, Position to);
    Task AnimateDamageNumberAsync(int damage, Position target);
}

// Implementation would use retained mode sub-region updates
public class CombatAnimationService : ICombatAnimationService
{
    public async Task AnimateMovementAsync(CombatantView combatant, Position from, Position to)
    {
        for (int frame = 0; frame < 10; frame++)
        {
            var interpolated = Position.Lerp(from, to, frame / 10.0);
            RenderCombatantAt(combatant, interpolated);
            await Task.Delay(50); // 20 FPS
        }
    }
}
```

**Benefits**: Improved combat feedback, more engaging visual experience

**Challenges**: Requires partial screen updates (not supported by Spectre.Console immediate mode)

---

### FE-4: Accessibility - Screen Reader Support

**Problem**: Terminal output not accessible to visually impaired players.

**Proposed Solution**:
- Add `IScreenReaderService` that generates text-only descriptions of UI state
- Provide `--screen-reader` CLI flag to enable alternate rendering mode
- Example: "Combat Round 3. Your turn. Warrior at Front Row, 80 HP. Three enemies: Goblin at Mid-Back (18 HP), Goblin at Mid-Back (22 HP), Orc at Mid-Front (55 HP)."

**Benefits**: Inclusive design, broader player base

---

### FE-5: Minimap Enhancement - Fog of War

**Problem**: Current minimap shows all 9 rooms (visited or not). No fog of war.

**Proposed Solution**:
```csharp
public Panel Render(ExplorationViewModel vm)
{
    // ...
    if (room != null && !room.HasBeenVisited)
    {
        cells.Add($"[{_theme.GetColor("FogColor")}]?[/]"); // Fog of war
    }
    else if (room != null && room.HasBeenVisited)
    {
        cells.Add($"[{_theme.GetColor("VisitedRoomColor")}]{directions[index]}[/]");
    }
    // ...
}
```

**Benefits**: More immersive exploration, rewards map discovery

---

## AAM-VOICE Compliance

### Layer Classification: **Layer 3 (Technical Specification)**

**Rationale**: This document is a system architecture specification for developers, not in-game content. Layer 3 applies to technical documentation written POST-Glitch with modern precision language.

### Domain 4 Compliance: **NOT APPLICABLE**

**Rationale**: Domain 4 (Technology Constraints) applies to **in-game lore content** (item descriptions, bestiary entries, NPC dialogue). This specification is **out-of-game technical documentation** and may use precision measurements (e.g., "50ms render time," "4×8 grid," "80% coverage").

### Voice Discipline: **Technical Authority**

**Characteristics**:
- **Precision**: Exact method signatures, line numbers, file paths
- **Definitive Statements**: "The system MUST...", "Renderers are Singleton"
- **Code Examples**: C# implementations with expected outputs
- **Quantifiable Metrics**: "60% test coverage," "50ms render time"

**Justification**: Developers require precise, unambiguous technical specifications. Epistemic uncertainty ("appears to use Singleton pattern") would introduce confusion and implementation errors.

---

**END OF SPECIFICATION**
