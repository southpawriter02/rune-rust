# Avalonia UI Implementation Specification
# Rune & Rust - Desktop GUI Combat System

**Version:** 1.0.0
**Date:** 2025-11-23
**Status:** Planning Phase
**Target Framework:** .NET 8.0
**UI Framework:** Avalonia UI 11.x

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Goals](#project-goals)
3. [Technology Stack](#technology-stack)
4. [Architecture Overview](#architecture-overview)
5. [Implementation Phases](#implementation-phases)
6. [Detailed Component Specifications](#detailed-component-specifications)
7. [File Structure](#file-structure)
8. [Dependencies](#dependencies)
9. [Migration Strategy](#migration-strategy)
10. [Testing Strategy](#testing-strategy)
11. [Timeline and Milestones](#timeline-and-milestones)
12. [Risk Assessment](#risk-assessment)

---

## Executive Summary

This specification outlines the implementation of a desktop GUI for Rune & Rust using Avalonia UI. The current terminal-based interface (Spectre.Console) will be supplemented with a rich graphical interface featuring a tactical combat grid with 16x16 pixel art sprites, real-time combat visualization, and enhanced player interaction.

**Key Objectives:**
- Maintain existing combat engine without breaking changes
- Create cross-platform desktop GUI (Windows, macOS, Linux)
- Implement tactical grid with pixel art rendering
- Provide superior UX compared to terminal interface
- Enable future expansion (animations, effects, sound)

**Approach:**
- New `RuneAndRust.DesktopUI` project alongside existing `ConsoleApp`
- MVVM architecture for clean separation of concerns
- Incremental development with working deliverables at each phase
- Preserve backward compatibility with existing save system

---

## Project Goals

### Immediate Goals (Phase 1-2, Weeks 1-4)

1. **Project Foundation**
   - Set up Avalonia UI project structure
   - Configure MVVM framework (ReactiveUI)
   - Establish basic window and navigation
   - Implement sprite rendering system

2. **Combat Grid MVP**
   - Render 6x4 tactical battlefield grid
   - Display combatant sprites at grid positions
   - Show HP bars and basic unit information
   - Handle cell selection and highlighting

3. **Core Combat Integration**
   - Connect to existing `CombatEngine`
   - Process player actions (attack, defend, move)
   - Display combat log messages
   - Handle turn-based flow

### Medium-Term Goals (Phase 3, Weeks 5-8)

4. **Enhanced Combat UI**
   - Detailed unit information panels
   - Action menu with ability selection
   - Status effect indicators
   - Cover and environmental hazard visualization

5. **Character Management**
   - Character sheet display
   - Inventory management UI
   - Equipment visualization
   - Progression tracking (XP, levels, specializations)

6. **Dungeon Navigation**
   - Room display with 3D vertical layer visualization
   - Movement between rooms
   - Map overlay showing explored areas
   - Vertical connection indicators (stairs, shafts)

### Future Goals (Phase 4+, Weeks 9+)

7. **Visual Polish**
   - Combat animations (attacks, abilities, damage)
   - Particle effects for magic and environmental hazards
   - Smooth transitions and UI animations
   - Biome-specific backgrounds and themes

8. **Advanced Features**
   - Save/load UI integration
   - Settings and configuration panels
   - Achievement tracking display
   - Companion management interface

9. **Accessibility & UX**
   - Keyboard navigation throughout
   - Tooltips and contextual help
   - Colorblind-friendly palette options
   - Scalable UI for different resolutions

---

## Technology Stack

### Core Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 | Runtime framework |
| Avalonia UI | 11.2+ | Cross-platform UI framework |
| ReactiveUI | 19.5+ | MVVM framework with reactive extensions |
| C# | 12.0 | Programming language |

### Supporting Libraries

| Library | Purpose |
|---------|---------|
| Avalonia.ReactiveUI | MVVM integration for Avalonia |
| Avalonia.Desktop | Desktop platform support |
| Avalonia.Diagnostics | Development debugging tools |
| SkiaSharp | 2D graphics rendering (included with Avalonia) |
| System.Reactive | Reactive programming support |

### Why Avalonia?

**Advantages:**
- ✅ Cross-platform (Windows, macOS, Linux)
- ✅ Modern XAML-based UI like WPF
- ✅ Active development and community
- ✅ Supports .NET 8.0 natively
- ✅ Excellent performance for 2D graphics
- ✅ Built-in styling and theming system
- ✅ No platform-specific dependencies

**Alternatives Considered:**
- **WPF:** Windows-only, ruled out for cross-platform requirement
- **MAUI:** Too mobile-focused, less mature for desktop
- **WinForms:** Legacy, limited styling capabilities

---

## Architecture Overview

### Architectural Principles

1. **Separation of Concerns**
   - UI layer has NO game logic
   - Engine layer has NO UI dependencies
   - ViewModels mediate between UI and Engine

2. **Unidirectional Data Flow**
   ```
   User Input → ViewModel Command → Engine Service → CombatState Update → ViewModel Property Change → UI Update
   ```

3. **Reactive Updates**
   - CombatState changes trigger UI updates automatically
   - No manual refresh calls needed
   - Observable collections for dynamic lists

4. **Dependency Injection**
   - Services registered in DI container
   - ViewModels receive dependencies via constructor injection
   - Testable and maintainable

### Layer Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    RuneAndRust.DesktopUI                     │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐ │
│  │  Views (XAML)  │  │  ViewModels    │  │  Services      │ │
│  │  - MainWindow  │  │  - CombatVM    │  │  - SpriteServ. │ │
│  │  - CombatView  │  │  - CharacterVM │  │  - AudioServ.  │ │
│  │  - CharSheet   │  │  - DungeonVM   │  │  - ConfigServ. │ │
│  └────────────────┘  └────────────────┘  └────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            ↓ References
┌─────────────────────────────────────────────────────────────┐
│                    RuneAndRust.Engine                        │
│  - CombatEngine, EnemyAIService, DungeonGenerator, etc.     │
│  - NO UI DEPENDENCIES                                        │
└─────────────────────────────────────────────────────────────┘
                            ↓ References
┌─────────────────────────────────────────────────────────────┐
│                     RuneAndRust.Core                         │
│  - CombatState, PlayerCharacter, Enemy, BattlefieldGrid     │
│  - Pure data models                                          │
└─────────────────────────────────────────────────────────────┘
```

### MVVM Pattern Implementation

**View (XAML)**
- Pure UI markup
- Data binding to ViewModel properties
- Event bindings to ViewModel commands
- No code-behind logic (minimal exceptions)

**ViewModel**
- Observable properties (INotifyPropertyChanged)
- Commands (ICommand implementations)
- Calls Engine services
- Transforms data for display

**Model**
- Existing `Core` entities (PlayerCharacter, Enemy, etc.)
- CombatState as source of truth
- No changes needed to existing models

---

## Implementation Phases

### Phase 1: Foundation (Week 1, ~16 hours)

**Deliverables:**
- ✅ Avalonia project created and configured
- ✅ Basic window with title and layout
- ✅ DI container setup with services
- ✅ Navigation system between views
- ✅ Sprite data structures and loader

**Tasks:**
1. Create `RuneAndRust.DesktopUI` project
2. Install Avalonia packages
3. Set up App.axaml and MainWindow.axaml
4. Configure DI container (Microsoft.Extensions.DependencyInjection)
5. Create base ViewModel class with INotifyPropertyChanged
6. Implement sprite data models and parser
7. Create basic "Hello World" window with navigation

**Success Criteria:**
- Application launches without errors
- Window displays with proper title and icon
- Can navigate between placeholder views
- Sprites can be loaded from embedded resources

---

### Phase 2: Combat Grid MVP (Weeks 2-3, ~30 hours)

**Deliverables:**
- ✅ Functional tactical combat grid display
- ✅ Combatant sprites rendered at positions
- ✅ Cell selection and highlighting
- ✅ HP bars and basic stats
- ✅ Combat log display
- ✅ Turn indicator

**Tasks:**

**Week 2: Grid Rendering**
1. Create `CombatGridControl` custom control
2. Implement grid cell rendering (6 columns × 4 rows)
3. Add checker pattern for cells
4. Implement player/enemy zone visual distinction
5. Create sprite rendering with SkiaSharp
6. Add cell hover and selection states
7. Implement coordinate display

**Week 3: Combat Integration**
8. Create `CombatViewModel` with reactive properties
9. Bind grid to `BattlefieldGrid` from `CombatState`
10. Display combatant sprites at `GridPosition`
11. Implement HP bar overlay rendering
12. Create combat log panel with scrolling
13. Add turn order indicator
14. Wire up cell click to selection

**Success Criteria:**
- Grid displays correctly with 24 cells (6×4)
- Player units visible in player zone (rows 0-1)
- Enemy units visible in enemy zone (rows 2-3)
- Clicking a cell shows unit details
- Combat log updates with messages
- Turn indicator shows current participant

---

### Phase 3: Combat Actions & UI Panels (Weeks 4-5, ~28 hours)

**Deliverables:**
- ✅ Action menu with combat commands
- ✅ Unit details panel
- ✅ Status effect indicators
- ✅ Ability selection interface
- ✅ Movement targeting system

**Tasks:**

**Week 4: Action System**
1. Create action button panel (Attack, Defend, Ability, Item, Move)
2. Implement command bindings for each action
3. Create ability picker dialog for multi-ability characters
4. Add target selection mode for attacks
5. Implement movement range highlighting
6. Create confirmation dialogs for critical actions
7. Add keyboard shortcuts (1-5 for actions)

**Week 5: Information Panels**
8. Design unit details panel layout
9. Implement large sprite preview (5x scale)
10. Add stat displays (HP, ATK, DEF, SPD) with progress bars
11. Create status effect icon renderer
12. Implement tooltip system for status effects
13. Add equipment display in unit panel
14. Create turn order sidebar

**Success Criteria:**
- Player can execute attack action by clicking Attack → Target
- Defend action increases defense for one turn
- Ability selection shows available abilities
- Movement highlights valid target cells
- Unit details panel shows all stats correctly
- Status effects display with icons and tooltips

---

### Phase 4: Character & Inventory Management (Weeks 6-7, ~24 hours)

**Deliverables:**
- ✅ Character sheet screen
- ✅ Inventory grid display
- ✅ Equipment slots visualization
- ✅ Item tooltips and details
- ✅ Drag-and-drop equipment

**Tasks:**

**Week 6: Character Sheet**
1. Create `CharacterSheetView` and `CharacterSheetViewModel`
2. Design layout for stats, specialization, trauma
3. Implement attribute display (Might, Finesse, etc.)
4. Add specialization tree visualization
5. Create trauma/stress/corruption meters
6. Implement XP bar and level progression display
7. Add skill list with descriptions

**Week 7: Inventory System**
8. Design inventory grid layout
9. Create item icon rendering system
10. Implement equipment slot rendering (weapon, armor, etc.)
11. Add drag-and-drop for equipping items
12. Create item tooltip with full stats
13. Implement item comparison preview
14. Add inventory sorting and filtering

**Success Criteria:**
- Character sheet displays all attributes correctly
- Specialization tree shows unlocked/available nodes
- Inventory shows all items with icons
- Can equip/unequip items via drag-and-drop
- Item tooltips show complete information
- Equipment changes reflect in character stats

---

### Phase 5: Dungeon Navigation & Map (Week 8, ~20 hours)

**Deliverables:**
- ✅ Room display with description
- ✅ Exit/connection visualization
- ✅ Minimap showing explored areas
- ✅ Vertical layer indicator
- ✅ Room transition animations

**Tasks:**

1. Create `DungeonExplorationView` and ViewModel
2. Implement room description panel
3. Add exit button for each cardinal direction
4. Create vertical connection indicators (stairs, shafts)
5. Design minimap control showing room graph
6. Implement fog-of-war for unexplored rooms
7. Add vertical layer visualization (depth indicator)
8. Create room transition fade effect
9. Implement search/rest/camp actions in room
10. Add environmental hazard indicators

**Success Criteria:**
- Current room displays with full description
- Available exits highlighted and clickable
- Minimap shows explored rooms in correct positions
- Vertical connections (stairs) clearly indicated
- Moving between rooms updates display smoothly
- Player can search rooms and find loot

---

### Phase 6: Polish & Animation (Weeks 9-10, ~30 hours)

**Deliverables:**
- ✅ Combat attack animations
- ✅ Damage number popups
- ✅ Ability effect visuals
- ✅ UI transitions and polish
- ✅ Biome-specific theming

**Tasks:**

**Week 9: Combat Animations**
1. Implement sprite flash on hit
2. Create damage number popup animation
3. Add attack arc/projectile animation
4. Implement healing effect visualization
5. Create status effect application animations
6. Add death/defeat animation for enemies
7. Implement victory/defeat screen transitions

**Week 10: Visual Polish**
8. Design biome-specific backgrounds (Muspelheim fire, etc.)
9. Create particle system for environmental effects
10. Add UI sound effects (clicks, confirmations)
11. Implement smooth HP bar transitions
12. Create ability charge-up visual effects
13. Add screen shake for heavy attacks
14. Polish button hover/press states

**Success Criteria:**
- Attacks show visual feedback (flash, damage numbers)
- Abilities have distinct visual effects
- Biome backgrounds change based on current dungeon
- UI feels responsive and polished
- Sound effects enhance user experience

---

### Phase 7: Accessibility & Settings (Week 11, ~16 hours)

**Deliverables:**
- ✅ Settings panel
- ✅ Keybind configuration
- ✅ Colorblind modes
- ✅ Volume controls
- ✅ Resolution/window options

**Tasks:**

1. Create settings screen layout
2. Implement volume sliders (master, SFX, music)
3. Add keybind remapping interface
4. Create colorblind palette options
5. Implement window mode options (fullscreen, windowed, borderless)
6. Add UI scale slider
7. Create accessibility options (high contrast, reduced motion)
8. Implement settings persistence (JSON config file)
9. Add "Reset to Defaults" functionality
10. Create keybind conflict detection

**Success Criteria:**
- Settings save and load correctly
- Keybinds can be remapped without conflicts
- Colorblind modes change palette appropriately
- Volume controls affect audio playback
- Window mode changes apply immediately

---

### Phase 8: Integration & Testing (Week 12+, Ongoing)

**Deliverables:**
- ✅ Full save/load integration
- ✅ Bug fixes and stability improvements
- ✅ Performance optimization
- ✅ Unit tests for ViewModels
- ✅ User testing feedback implementation

**Tasks:**

1. Connect save/load system to UI
2. Implement save game browser UI
3. Add autosave indicator
4. Create comprehensive unit tests for ViewModels
5. Performance profiling and optimization
6. Memory leak detection and fixes
7. User testing sessions
8. Bug triage and fixing
9. Documentation updates
10. Prepare for release

**Success Criteria:**
- Can save and load games from UI
- No crashes or critical bugs
- Smooth performance (60 FPS) on target hardware
- All ViewModel logic covered by unit tests
- User feedback incorporated

---

## Detailed Component Specifications

### 1. Sprite System

**Purpose:** Render 16x16 pixel art sprites at various scales

**Data Structure:**
```csharp
public class PixelSprite
{
    public string Name { get; set; }
    public string[] PixelData { get; set; }  // 16 rows of 16 characters
    public Dictionary<char, SKColor> Palette { get; set; }

    public SKBitmap ToBitmap(int scale = 3)
    {
        var bitmap = new SKBitmap(16 * scale, 16 * scale);
        using var canvas = new SKCanvas(bitmap);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                char pixel = PixelData[y][x];
                if (Palette.TryGetValue(pixel, out SKColor color) && color != SKColor.Empty)
                {
                    using var paint = new SKPaint { Color = color };
                    canvas.DrawRect(x * scale, y * scale, scale, scale, paint);
                }
            }
        }

        return bitmap;
    }
}
```

**Sprite Definitions:**
- Shieldmaiden, Berserker, Runecaster, Greatsword (player sprites)
- Draugr, Goblin Rider, Jötun-Forged, Necromancer (enemy sprites)
- Additional sprites as needed for new enemy types

**Loading Strategy:**
- Embed sprite data as JSON resource files
- Load on application startup
- Cache bitmaps at common scales (3x, 5x)
- Support runtime sprite registration for mods

**File Location:**
```
/Resources/Sprites/
  - player_sprites.json
  - enemy_sprites.json
  - item_sprites.json
  - terrain_sprites.json
```

---

### 2. Combat Grid Control

**Purpose:** Custom Avalonia control for rendering tactical battlefield

**Control Class:**
```csharp
public class CombatGridControl : Control
{
    // Dependency Properties
    public static readonly StyledProperty<BattlefieldGrid?> GridProperty = ...
    public static readonly StyledProperty<GridPosition?> SelectedCellProperty = ...
    public static readonly StyledProperty<GridPosition?> HoveredCellProperty = ...

    // Properties
    public BattlefieldGrid? Grid { get; set; }
    public GridPosition? SelectedCell { get; set; }
    public GridPosition? HoveredCell { get; set; }

    // Rendering
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Grid == null) return;

        RenderGridCells(context);
        RenderOccupants(context);
        RenderOverlays(context);
    }

    // Interaction
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var position = e.GetPosition(this);
        var cell = GetCellAtPosition(position);

        if (cell.HasValue)
        {
            SelectedCell = cell.Value;
            RaiseEvent(new CellSelectedEventArgs(cell.Value));
        }
    }
}
```

**Visual Specifications:**
- Cell size: 100×100 logical pixels
- Grid dimensions: 6 columns × 4 rows
- Total size: 600×400 pixels (plus borders/padding)
- Player zone (rows 0-1): Blue gradient background
- Enemy zone (rows 2-3): Red gradient background
- Checkerboard pattern with alternating shades
- 3px border for selected cell (gold)
- 2px border for hovered cell (light blue)

**Cell States:**
- Normal: Base background color
- Hovered: Lighter background + border
- Selected: Gold border + glow effect
- Occupied: Shows sprite + HP bar
- Valid Move Target: Green highlight
- Valid Attack Target: Red highlight
- Cover Present: Overlay icon (▓ for physical, ▒ for metaphysical)
- Hazard Present: Warning icon and tint

---

### 3. Combat ViewModel

**Purpose:** Mediate between CombatEngine and UI, provide reactive data

**Class Structure:**
```csharp
public class CombatViewModel : ViewModelBase
{
    private readonly CombatEngine _combatEngine;
    private readonly SpriteService _spriteService;
    private CombatState _combatState;

    // Observable Properties
    private ObservableCollection<GridCellViewModel> _gridCells;
    public ObservableCollection<GridCellViewModel> GridCells
    {
        get => _gridCells;
        set => this.RaiseAndSetIfChanged(ref _gridCells, value);
    }

    private ObservableCollection<string> _combatLog;
    public ObservableCollection<string> CombatLog
    {
        get => _combatLog;
        set => this.RaiseAndSetIfChanged(ref _combatLog, value);
    }

    private CombatantViewModel? _selectedUnit;
    public CombatantViewModel? SelectedUnit
    {
        get => _selectedUnit;
        set => this.RaiseAndSetIfChanged(ref _selectedUnit, value);
    }

    private string _currentPhase;
    public string CurrentPhase
    {
        get => _currentPhase;
        set => this.RaiseAndSetIfChanged(ref _currentPhase, value);
    }

    // Commands
    public ReactiveCommand<Unit, Unit> AttackCommand { get; }
    public ReactiveCommand<Unit, Unit> DefendCommand { get; }
    public ReactiveCommand<string, Unit> UseAbilityCommand { get; }
    public ReactiveCommand<GridPosition, Unit> MoveCommand { get; }
    public ReactiveCommand<Unit, Unit> FleeCommand { get; }

    // Constructor
    public CombatViewModel(CombatEngine combatEngine, SpriteService spriteService)
    {
        _combatEngine = combatEngine;
        _spriteService = spriteService;

        // Initialize commands
        AttackCommand = ReactiveCommand.CreateFromTask(ExecuteAttackAsync);
        DefendCommand = ReactiveCommand.CreateFromTask(ExecuteDefendAsync);
        UseAbilityCommand = ReactiveCommand.CreateFromTask<string>(ExecuteAbilityAsync);
        MoveCommand = ReactiveCommand.CreateFromTask<GridPosition>(ExecuteMoveAsync);

        // Set up reactive subscriptions
        this.WhenAnyValue(x => x.SelectedUnit)
            .Subscribe(UpdateAvailableActions);
    }

    // Methods
    public async Task InitializeCombatAsync(CombatState state)
    {
        _combatState = state;
        RefreshGrid();
        UpdateCombatLog();
        CurrentPhase = "Player Turn";
    }

    private async Task ExecuteAttackAsync()
    {
        if (SelectedUnit?.IsEnemy ?? true) return;

        // Trigger target selection mode
        await ShowTargetSelectionAsync(TargetType.Enemy);
    }

    private void RefreshGrid()
    {
        var cells = new List<GridCellViewModel>();

        foreach (var tile in _combatState.Grid.Tiles.Values)
        {
            var cellVM = new GridCellViewModel
            {
                Position = tile.Position,
                HasCover = tile.CoverType != CoverType.None,
                HasTrap = tile.Traps.Any(),
                Occupant = GetOccupantViewModel(tile)
            };

            cells.Add(cellVM);
        }

        GridCells = new ObservableCollection<GridCellViewModel>(cells);
    }
}
```

**Reactive Behaviors:**
- When `CombatState.CombatLog` updates → Update UI `CombatLog`
- When turn advances → Update `CurrentPhase` and trigger animations
- When unit HP changes → Animate HP bar transition
- When status effects change → Update status icons
- When grid changes (movement) → Re-render affected cells

---

### 4. Action Menu Panel

**Purpose:** Display available combat actions with visual feedback

**XAML Layout:**
```xml
<StackPanel Classes="action-menu">
    <TextBlock Text="═══ ACTIONS ═══" Classes="panel-title"/>

    <Button Classes="action-button attack"
            Command="{Binding AttackCommand}"
            IsEnabled="{Binding CanAttack}">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="[1] ATTACK"/>
            <TextBlock Text="⚔" Classes="action-icon"/>
        </StackPanel>
    </Button>

    <Button Classes="action-button defend"
            Command="{Binding DefendCommand}"
            IsEnabled="{Binding CanDefend}">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="[2] DEFEND"/>
            <TextBlock Text="🛡" Classes="action-icon"/>
        </StackPanel>
    </Button>

    <Button Classes="action-button ability"
            Command="{Binding ShowAbilityMenuCommand}"
            IsEnabled="{Binding CanUseAbility}">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="[3] ABILITY"/>
            <TextBlock Text="✦" Classes="action-icon"/>
        </StackPanel>
    </Button>

    <Button Classes="action-button item"
            Command="{Binding ShowItemMenuCommand}"
            IsEnabled="{Binding CanUseItem}">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="[4] ITEM"/>
            <TextBlock Text="◈" Classes="action-icon"/>
        </StackPanel>
    </Button>

    <Button Classes="action-button move"
            Command="{Binding ShowMoveMenuCommand}"
            IsEnabled="{Binding CanMove}">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="[5] MOVE"/>
            <TextBlock Text="▶" Classes="action-icon"/>
        </StackPanel>
    </Button>
</StackPanel>
```

**Styling (CSS-like syntax for Avalonia):**
```xml
<Style Selector="Button.action-button">
    <Setter Property="Padding" Value="12,8"/>
    <Setter Property="Margin" Value="0,0,0,6"/>
    <Setter Property="Background" Value="#3a0000"/>
    <Setter Property="BorderBrush" Value="#5a0000"/>
    <Setter Property="BorderThickness" Value="2"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="FontWeight" Value="Bold"/>
</Style>

<Style Selector="Button.action-button:pointerover">
    <Setter Property="Background" Value="#5a0000"/>
    <Setter Property="BorderBrush" Value="#FF4500"/>
    <Setter Property="RenderTransform" Value="translate(6px, 0)"/>
</Style>

<Style Selector="Button.action-button:disabled">
    <Setter Property="Opacity" Value="0.4"/>
    <Setter Property="Cursor" Value="NotAllowed"/>
</Style>
```

**Keyboard Shortcuts:**
- Number keys 1-5 trigger respective actions
- Implement via KeyBindings in View
- Visual feedback when hotkey pressed

---

### 5. Unit Details Panel

**Purpose:** Show detailed information about selected unit

**Layout Structure:**
```
╔═══ UNIT DETAILS ═══╗
┌───────────────────┐
│   Large Sprite    │  ← 5x scale (80×80px)
│     Preview       │
└───────────────────┘

Position: (X, Y)
Name: [Unit Name]
Type: PLAYER / ENEMY

HP: [███████░░░] 75/100

┌─ STATS ─────────┐
│ ATK: [███░░] 8  │
│ DEF: [█████] 9  │
│ SPD: [████░] 6  │
└─────────────────┘

┌─ STATUS EFFECTS ─┐
│ 🔥 Burning (2)   │
│ 🛡 Shield (50)   │
└──────────────────┘

┌─ EQUIPMENT ──────┐
│ ⚔ Iron Sword    │
│ 🛡 Steel Shield  │
└──────────────────┘
```

**Data Bindings:**
```xml
<StackPanel Classes="unit-details-panel">
    <!-- Sprite Preview -->
    <Border Classes="sprite-preview-container">
        <Image Source="{Binding SelectedUnit.LargeSprite}"
               Width="80" Height="80"
               RenderOptions.BitmapInterpolationMode="None"/>
    </Border>

    <!-- Position -->
    <TextBlock>
        <Run Text="Position: "/>
        <Run Text="{Binding SelectedUnit.Position}" FontWeight="Bold"/>
    </TextBlock>

    <!-- Name -->
    <TextBlock Text="{Binding SelectedUnit.Name}"
               Classes="unit-name"/>

    <!-- Type -->
    <TextBlock>
        <Run Text="Type: "/>
        <Run Text="{Binding SelectedUnit.Type}"
             Foreground="{Binding SelectedUnit.TypeColor}"/>
    </TextBlock>

    <!-- HP Bar -->
    <ProgressBar Value="{Binding SelectedUnit.HP}"
                 Maximum="{Binding SelectedUnit.MaxHP}"
                 Classes="hp-bar"/>

    <TextBlock>
        <Run Text="HP: "/>
        <Run Text="{Binding SelectedUnit.HP}"/>
        <Run Text="/"/>
        <Run Text="{Binding SelectedUnit.MaxHP}"/>
    </TextBlock>

    <!-- Stats -->
    <ItemsControl Items="{Binding SelectedUnit.Stats}">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="{Binding Name}" Width="50"/>
                    <ProgressBar Value="{Binding Value}" Maximum="10" Width="100"/>
                    <TextBlock Text="{Binding Value}" Margin="8,0,0,0"/>
                </StackPanel>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>

    <!-- Status Effects -->
    <ItemsControl Items="{Binding SelectedUnit.StatusEffects}">
        <!-- Display status icons with tooltips -->
    </ItemsControl>
</StackPanel>
```

---

## File Structure

Proposed directory structure for `RuneAndRust.DesktopUI` project:

```
RuneAndRust.DesktopUI/
├── App.axaml                          # Application definition
├── App.axaml.cs                       # Application code-behind
├── Program.cs                         # Entry point
├── ViewLocator.cs                     # ViewModel → View mapping
│
├── Views/                             # XAML views
│   ├── MainWindow.axaml
│   ├── Combat/
│   │   ├── CombatView.axaml           # Main combat screen
│   │   ├── CombatGridControl.axaml    # Custom grid control
│   │   ├── UnitDetailsPanel.axaml     # Unit info panel
│   │   ├── ActionMenuPanel.axaml      # Action buttons
│   │   └── CombatLogPanel.axaml       # Combat log display
│   ├── Character/
│   │   ├── CharacterSheetView.axaml   # Full character sheet
│   │   ├── InventoryView.axaml        # Inventory grid
│   │   └── EquipmentView.axaml        # Equipment slots
│   ├── Dungeon/
│   │   ├── DungeonExplorationView.axaml  # Room navigation
│   │   ├── MinimapControl.axaml          # Minimap display
│   │   └── RoomDetailsPanel.axaml        # Room description
│   ├── Menus/
│   │   ├── MainMenuView.axaml         # Title screen
│   │   ├── SaveLoadView.axaml         # Save/load browser
│   │   └── SettingsView.axaml         # Settings panel
│   └── Dialogs/
│       ├── AbilitySelectionDialog.axaml
│       ├── ItemSelectionDialog.axaml
│       ├── ConfirmationDialog.axaml
│       └── MessageDialog.axaml
│
├── ViewModels/                        # ViewModels (MVVM)
│   ├── ViewModelBase.cs               # Base ViewModel class
│   ├── MainWindowViewModel.cs
│   ├── Combat/
│   │   ├── CombatViewModel.cs         # Main combat logic
│   │   ├── GridCellViewModel.cs       # Individual cell state
│   │   ├── CombatantViewModel.cs      # Unit wrapper
│   │   └── AbilityViewModel.cs        # Ability display data
│   ├── Character/
│   │   ├── CharacterSheetViewModel.cs
│   │   ├── InventoryViewModel.cs
│   │   └── ItemViewModel.cs
│   ├── Dungeon/
│   │   ├── DungeonViewModel.cs
│   │   └── RoomViewModel.cs
│   └── Menus/
│       ├── MainMenuViewModel.cs
│       ├── SaveLoadViewModel.cs
│       └── SettingsViewModel.cs
│
├── Services/                          # UI-specific services
│   ├── SpriteService.cs               # Sprite loading and caching
│   ├── AudioService.cs                # Sound effects and music
│   ├── ConfigurationService.cs        # User settings persistence
│   ├── NavigationService.cs           # View navigation
│   └── DialogService.cs               # Dialog management
│
├── Models/                            # UI-specific models
│   ├── PixelSprite.cs                 # Sprite data structure
│   ├── UIConfiguration.cs             # Settings model
│   └── NavigationContext.cs           # Navigation state
│
├── Controls/                          # Custom controls
│   ├── CombatGridControl.cs           # Main grid control
│   ├── PixelArtImage.cs               # Pixel-perfect image rendering
│   ├── AnimatedSprite.cs              # Sprite with animation
│   └── ProgressBarWithGradient.cs     # Styled progress bar
│
├── Converters/                        # Value converters for binding
│   ├── BoolToVisibilityConverter.cs
│   ├── ColorToBrushConverter.cs
│   ├── HPToColorConverter.cs          # HP % to color (green/yellow/red)
│   └── StatusEffectToIconConverter.cs
│
├── Resources/                         # Embedded resources
│   ├── Sprites/
│   │   ├── player_sprites.json        # Player sprite definitions
│   │   ├── enemy_sprites.json         # Enemy sprite definitions
│   │   ├── item_icons.json            # Item icons
│   │   └── ui_icons.json              # UI element icons
│   ├── Fonts/
│   │   └── CourierNew.ttf             # Monospace font
│   ├── Audio/
│   │   ├── SFX/                       # Sound effects
│   │   └── Music/                     # Background music
│   └── Styles/
│       ├── DefaultTheme.axaml         # Default color scheme
│       ├── MuspelheimTheme.axaml      # Fire biome theme
│       └── NiflheimTheme.axaml        # Ice biome theme
│
├── Animations/                        # Animation helpers
│   ├── SpriteAnimator.cs              # Sprite animation controller
│   ├── DamageNumberAnimation.cs       # Floating damage numbers
│   └── TransitionAnimations.cs        # Screen transitions
│
└── Helpers/                           # Utility classes
    ├── KeyboardShortcutHandler.cs     # Global hotkey management
    ├── ColorPalettes.cs               # Predefined color schemes
    └── LayoutHelpers.cs               # Responsive layout utilities
```

---

## Dependencies

### Required NuGet Packages

Add to `RuneAndRust.DesktopUI.csproj`:

```xml
<ItemGroup>
  <!-- Avalonia Core -->
  <PackageReference Include="Avalonia" Version="11.2.0" />
  <PackageReference Include="Avalonia.Desktop" Version="11.2.0" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.2.0" />

  <!-- MVVM Framework -->
  <PackageReference Include="Avalonia.ReactiveUI" Version="11.2.0" />

  <!-- Development Tools -->
  <PackageReference Include="Avalonia.Diagnostics" Version="11.2.0" Condition="'$(Configuration)' == 'Debug'" />

  <!-- Dependency Injection -->
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />

  <!-- Logging -->
  <PackageReference Include="Serilog" Version="4.0.0" />
  <PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
  <PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />

  <!-- JSON Serialization -->
  <PackageReference Include="System.Text.Json" Version="8.0.0" />
</ItemGroup>

<ItemGroup>
  <!-- Project References -->
  <ProjectReference Include="..\RuneAndRust.Engine\RuneAndRust.Engine.csproj" />
  <ProjectReference Include="..\RuneAndRust.Core\RuneAndRust.Core.csproj" />
  <ProjectReference Include="..\RuneAndRust.Persistence\RuneAndRust.Persistence.csproj" />
</ItemGroup>
```

### Optional Packages (Future Phases)

```xml
<!-- Audio Support -->
<PackageReference Include="NAudio" Version="2.2.1" />

<!-- Advanced Graphics -->
<PackageReference Include="SkiaSharp.Views.Avalonia" Version="2.88.8" />

<!-- Animation Library -->
<PackageReference Include="Avalonia.Animation" Version="11.2.0" />
```

---

## Migration Strategy

### Coexistence Approach

The Desktop UI and Console App will **coexist** during development:

1. **Phase 1-3:** Console app remains primary, desktop app in development
2. **Phase 4-6:** Desktop app becomes feature-complete, console still available
3. **Phase 7+:** Desktop app recommended, console deprecated (but still functional)

### Shared Code Strategy

**Engine Layer (No Changes Needed):**
- `CombatEngine`, `DungeonGenerator`, all services remain unchanged
- Both UIs call the same methods
- No UI-specific code in Engine

**State Management:**
- `CombatState` is the source of truth for both UIs
- Console app and Desktop UI both read from same state
- No duplicate game logic

**Save System:**
- Both UIs use same `SaveRepository`
- Save files are compatible between Console and Desktop
- No migration needed for existing saves

### Testing Strategy

**Dual Testing:**
- Run same combat scenarios in both UIs
- Verify state consistency
- Ensure no divergence in behavior

**Compatibility Checks:**
- Save game created in Console → Load in Desktop ✅
- Save game created in Desktop → Load in Console ✅
- Both should produce identical combat outcomes

---

## Testing Strategy

### Unit Testing

**ViewModel Tests:**
```csharp
[Fact]
public async Task AttackCommand_WithValidTarget_ExecutesAttack()
{
    // Arrange
    var mockEngine = new Mock<ICombatEngine>();
    var viewModel = new CombatViewModel(mockEngine.Object, ...);

    // Act
    await viewModel.AttackCommand.Execute();

    // Assert
    mockEngine.Verify(e => e.ProcessPlayerAction(
        It.IsAny<AttackAction>()), Times.Once);
}
```

**Service Tests:**
```csharp
[Fact]
public void SpriteService_LoadSprite_ReturnsCachedBitmap()
{
    // Arrange
    var service = new SpriteService();

    // Act
    var sprite1 = service.GetSprite("shieldmaiden", scale: 3);
    var sprite2 = service.GetSprite("shieldmaiden", scale: 3);

    // Assert
    Assert.Same(sprite1, sprite2);  // Same instance (cached)
}
```

### Integration Testing

**Full Combat Flow:**
1. Initialize combat with test state
2. Execute player attack
3. Verify grid updates
4. Verify combat log updated
5. Verify enemy turn triggered
6. Verify victory condition detected

**Navigation Flow:**
1. Start at main menu
2. Load save game
3. Navigate to dungeon
4. Enter combat
5. Win combat
6. Return to exploration
7. Save game

### Manual Testing Checklist

**Combat Grid:**
- [ ] All 24 cells render correctly
- [ ] Sprites display at correct positions
- [ ] HP bars update smoothly
- [ ] Cell selection highlights properly
- [ ] Player/enemy zones visually distinct

**Combat Actions:**
- [ ] Attack targets correct enemy
- [ ] Defend increases defense stat
- [ ] Abilities execute with correct effects
- [ ] Items consumed from inventory
- [ ] Movement updates grid position

**User Interface:**
- [ ] All panels resize correctly
- [ ] Text is readable at all scales
- [ ] Tooltips appear on hover
- [ ] Keyboard shortcuts work
- [ ] Buttons respond to clicks

**Performance:**
- [ ] Smooth 60 FPS during combat
- [ ] No lag during animations
- [ ] Fast load times (<2 seconds)
- [ ] Memory usage stable (no leaks)

---

## Timeline and Milestones

### Overview

| Phase | Duration | Completion Date | Deliverables |
|-------|----------|-----------------|--------------|
| **Phase 1** | 1 week | Week 1 | Foundation & Sprites |
| **Phase 2** | 2 weeks | Week 3 | Combat Grid MVP |
| **Phase 3** | 2 weeks | Week 5 | Combat Actions |
| **Phase 4** | 2 weeks | Week 7 | Character/Inventory |
| **Phase 5** | 1 week | Week 8 | Dungeon Navigation |
| **Phase 6** | 2 weeks | Week 10 | Polish & Animation |
| **Phase 7** | 1 week | Week 11 | Settings & Accessibility |
| **Phase 8** | 1+ week | Week 12+ | Integration & Testing |
| **Total** | **12+ weeks** | - | **Full Desktop UI** |

### Detailed Milestones

**Milestone 1: "Hello Aethelgard" (End of Week 1)**
- ✅ Application launches successfully
- ✅ Main window displays with title
- ✅ Basic navigation between placeholder views
- ✅ Sprites can be rendered
- **Demo:** Show window with rendered sprite examples

**Milestone 2: "Grid Vision" (End of Week 3)**
- ✅ Combat grid displays with 6×4 layout
- ✅ Units render at positions from CombatState
- ✅ Cell selection functional
- ✅ Combat log shows messages
- **Demo:** Load test combat, see units on grid, click cells

**Milestone 3: "Interactive Combat" (End of Week 5)**
- ✅ Player can execute attacks via UI
- ✅ Abilities can be selected and used
- ✅ Movement system functional
- ✅ Turn order visible
- **Demo:** Play through a full combat encounter

**Milestone 4: "Character Central" (End of Week 7)**
- ✅ Character sheet displays all stats
- ✅ Inventory grid functional
- ✅ Equipment can be changed
- **Demo:** View character, manage inventory, equip items

**Milestone 5: "Dungeon Delve" (End of Week 8)**
- ✅ Navigate between rooms
- ✅ Minimap shows explored areas
- ✅ Vertical layers visualized
- **Demo:** Explore procedurally generated dungeon

**Milestone 6: "Eye Candy" (End of Week 10)**
- ✅ Attacks show animations
- ✅ Damage numbers pop up
- ✅ Biome backgrounds implemented
- **Demo:** Show polished combat with effects

**Milestone 7: "Player Preference" (End of Week 11)**
- ✅ Settings panel functional
- ✅ Keybinds can be remapped
- ✅ Colorblind mode available
- **Demo:** Configure settings, test accessibility features

**Milestone 8: "Ready to Ship" (End of Week 12+)**
- ✅ Save/load works from UI
- ✅ No critical bugs
- ✅ Performance acceptable
- ✅ Documentation complete
- **Demo:** Full gameplay session from start to finish

---

## Risk Assessment

### Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **Avalonia learning curve** | Medium | Medium | Allocate extra time for Phase 1, leverage docs/community |
| **Performance issues with grid rendering** | Low | High | Use hardware acceleration, optimize render loop, profile early |
| **Breaking changes in Engine** | Low | High | Maintain strict API contracts, comprehensive testing |
| **Complex animations cause lag** | Medium | Medium | Make animations optional, use simple tweens first |
| **Save file compatibility** | Low | High | Thorough testing, version save format |

### Project Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **Scope creep** | High | Medium | Stick to phase deliverables, defer "nice-to-haves" |
| **Underestimated effort** | Medium | High | Build buffer time into schedule, prioritize ruthlessly |
| **Lack of UI/UX expertise** | Low | Medium | Get user feedback early, iterate on design |
| **Platform-specific bugs** | Medium | Low | Test on all target platforms regularly |

### Timeline Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **12-week timeline too aggressive** | Medium | Medium | Each phase delivers working software, can extend if needed |
| **Blocked by dependencies** | Low | Medium | Identify blockers early, have parallel work streams |
| **Feature interactions complex** | Medium | Medium | Incremental integration, test each phase thoroughly |

---

## Success Criteria

The Avalonia UI implementation will be considered successful when:

### Functional Criteria
- ✅ Player can complete a full dungeon run using only the desktop UI
- ✅ All combat actions available in Console app are available in Desktop UI
- ✅ Character progression (leveling, specialization, equipment) fully functional
- ✅ Save/load system works seamlessly
- ✅ No loss of functionality compared to Console app

### Quality Criteria
- ✅ Consistent 60 FPS during combat on target hardware (mid-range PC, 2020+)
- ✅ Application launches in <3 seconds
- ✅ No crashes during typical 1-hour play session
- ✅ Memory usage stable (<500 MB for typical session)
- ✅ All critical paths covered by automated tests

### User Experience Criteria
- ✅ New players can learn combat system within 5 minutes
- ✅ Experienced players prefer Desktop UI over Console (via survey)
- ✅ All interactive elements have clear visual feedback
- ✅ Keyboard shortcuts available for all common actions
- ✅ UI is readable and usable at default window size (1280×720 minimum)

### Cross-Platform Criteria
- ✅ Works on Windows 10/11
- ✅ Works on macOS 12+ (Monterey and newer)
- ✅ Works on Ubuntu 22.04 LTS (or recent Linux distro)
- ✅ Identical functionality across all platforms
- ✅ No platform-specific critical bugs

---

## Next Steps

### Immediate Actions (This Week)

1. **Review and Approve Specification**
   - Stakeholder review
   - Gather feedback
   - Finalize scope for Phase 1

2. **Set Up Development Environment**
   - Install Avalonia templates
   - Configure IDE (Rider, VS, or VS Code with extensions)
   - Set up version control branch strategy

3. **Create Initial Project**
   - Run `dotnet new avalonia.mvvm -o RuneAndRust.DesktopUI`
   - Add to solution
   - Configure project references
   - Verify build succeeds

4. **Establish Development Workflow**
   - Create GitHub issues for Phase 1 tasks
   - Set up project board (Kanban)
   - Define definition of "done" for tasks
   - Schedule daily/weekly check-ins

### Questions to Resolve

Before starting implementation:

1. **Platform Priority:** Which platform should be primary development target? (Recommend Windows for initial development, then test on macOS/Linux)

2. **Theme Preference:** Should we use Avalonia Fluent theme, or create custom theme matching prototype? (Recommend custom for Norse aesthetic)

3. **Audio Priority:** Should audio be included in early phases, or deferred to Phase 6+? (Recommend defer to Phase 6)

4. **Console Deprecation:** When should Console app be officially deprecated? (Recommend after Phase 8, keep available for headless/CI scenarios)

5. **Sprite Format:** Should sprites be JSON embedded resources, or PNG files? (Recommend JSON for flexibility, generate PNGs as optimization later)

---

## Appendix

### A. Glossary

- **Avalonia:** Cross-platform .NET UI framework
- **MVVM:** Model-View-ViewModel architectural pattern
- **ReactiveUI:** Reactive programming framework for MVVM
- **ViewModel:** Intermediary between View (UI) and Model (data)
- **Dependency Injection:** Design pattern for managing object dependencies
- **Observable Property:** Property that notifies subscribers when changed
- **Command:** Bindable action triggered by UI interaction
- **XAML:** Extensible Application Markup Language for UI definition
- **SKBitmap:** SkiaSharp bitmap class for image rendering

### B. Reference Links

- **Avalonia Documentation:** https://docs.avaloniaui.net/
- **ReactiveUI Documentation:** https://www.reactiveui.net/docs/
- **SkiaSharp Documentation:** https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/
- **MVVM Pattern Guide:** https://docs.microsoft.com/en-us/dotnet/architecture/maui/mvvm

### C. Contact & Resources

- **Project Repository:** [GitHub URL]
- **Issue Tracker:** [GitHub Issues URL]
- **Design Assets:** [Figma/Asset Repository URL]
- **Team Communication:** [Discord/Slack URL]

---

**Document Revision History:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-11-23 | Claude | Initial specification created |

---

**End of Specification**

This document should be treated as a living document and updated as:
- Requirements change
- New technical constraints discovered
- Feedback from implementation reveals better approaches
- Milestones are completed and new phases begin

Regular review (bi-weekly recommended) ensures specification stays aligned with reality of development.
