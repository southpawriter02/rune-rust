# GUI Implementation Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-28
> **Status**: Active
> **Specification ID**: SPEC-SYSTEM-009

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2025-11-28 | AI | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [x] **Review**: Ready for stakeholder review
- [x] **Approved**: Approved for implementation
- [x] **Active**: Currently implemented and maintained
- [ ] **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders
- **Primary Owner**: UI/UX Lead
- **Design**: Interface layout, navigation patterns, visual design
- **Implementation**: RuneAndRust.DesktopUI project
- **QA/Testing**: UI responsiveness, accessibility, cross-platform compatibility

---

## Executive Summary

### Purpose Statement
The GUI Implementation System provides the complete presentation layer for Rune & Rust, enabling player interaction with all game systems through a reactive, cross-platform desktop interface built on the Avalonia framework with ReactiveUI MVVM architecture.

### Scope
**In Scope**:
- MVVM architecture with ReactiveUI pattern
- 20 ViewModels managing UI state
- 17 Views with XAML layout definitions
- 9 Controllers orchestrating game workflows
- 26 services supporting UI functionality
- Custom controls (Combat Grid, Minimap, Boss Overlay)
- Navigation system with back-stack support
- Data binding and value converters
- Dependency injection configuration
- Keyboard shortcuts and accessibility

**Out of Scope**:
- Game logic implementation → `RuneAndRust.Engine`
- Data persistence → `RuneAndRust.Persistence`
- Core data models → `RuneAndRust.Core`
- Mobile/touch interfaces → Future platform expansion
- Audio engine internals → Wrapped by AudioService

### Success Criteria
- **Player Experience**: Responsive UI (<16ms frame time), intuitive navigation, clear feedback
- **Technical**: Zero-regression design (GUI never duplicates engine logic)
- **Design**: Consistent theming via UIConstants and UIColors
- **Accessibility**: Keyboard navigation, screen reader support, colorblind modes

---

## Related Documentation

### Dependencies
**Depends On**:
- `RuneAndRust.Core`: Game data models, character/enemy structures
- `RuneAndRust.Engine`: Combat engine, AI, game logic services
- `RuneAndRust.Persistence`: Save/load, configuration storage

**Depended Upon By**:
- No other systems depend on GUI (presentation layer is terminal)

### Related Specifications
- `SPEC-SYSTEM-001`: Save/Load System (SaveLoadViewModel integration)
- `SPEC-SYSTEM-002`: Inventory & Equipment (InventoryViewModel integration)
- `SPEC-SYSTEM-005`: Enemy AI Behavior (CombatViewModel integration)
- `SPEC-COMBAT-001`: Combat Resolution (CombatController integration)

### Implementation Documentation
- `RuneAndRust.DesktopUI/IMPLEMENTATION_v0.43.4.md`: Combat Grid Control specs
- `RuneAndRust.DesktopUI/IMPLEMENTATION_v0.43.5.md`: Combat Actions & Turn Management

### Code References
- **Project Root**: `RuneAndRust.DesktopUI/`
- **Entry Point**: `Program.cs`, `App.axaml.cs`
- **ViewModels**: `ViewModels/*.cs` (20 files)
- **Views**: `Views/*.axaml` (17 files)
- **Services**: `Services/*.cs` (26 files)
- **Controllers**: `Controllers/*.cs` (9 files)
- **Tests**: `RuneAndRust.Tests/UI/*.cs`

---

## Design Philosophy

### Design Pillars

1. **Zero-Regression Architecture**
   - **Rationale**: All game logic lives in Engine; GUI only presents and collects input
   - **Examples**: CombatController calls CombatEngine methods, never calculates damage
   - **Anti-Pattern**: Never duplicate formulas, rules, or game logic in ViewModels

2. **Reactive UI Pattern**
   - **Rationale**: Observable properties eliminate manual refresh calls and state sync bugs
   - **Examples**: `WhenAnyValue()` subscriptions auto-update UI on property changes
   - **Anti-Pattern**: Never use polling or timer-based UI updates

3. **MVVM Separation of Concerns**
   - **Rationale**: Testable ViewModels, designer-friendly Views, orchestrated workflows
   - **Model**: Core/Engine data structures (Character, Enemy, Item)
   - **ViewModel**: UI state, commands, presentation logic
   - **View**: XAML layout, data binding, visual hierarchy

4. **Dependency Injection Throughout**
   - **Rationale**: Loose coupling enables testing, mocking, and modular development
   - **Examples**: Services injected into Controllers/ViewModels via constructor
   - **Anti-Pattern**: Never use static service locators or singletons

---

## Technology Stack

### Framework & Libraries
| Component | Version | Purpose |
|-----------|---------|---------|
| **.NET** | 8.0 LTS | Runtime and base class libraries |
| **Avalonia** | 11.0.0 | Cross-platform desktop UI framework |
| **Avalonia.ReactiveUI** | 11.0.0 | ReactiveUI integration for Avalonia |
| **ReactiveUI** | 19.5.1 | MVVM framework with reactive extensions |
| **SkiaSharp** | 2.88.8 | 2D graphics for sprite rendering |
| **Microsoft.Extensions.DependencyInjection** | 8.0.0 | IoC container |
| **Serilog** | 4.0.0 | Structured logging |

### Target Platforms
- **Windows**: Primary development target (WinExe output)
- **macOS**: Avalonia cross-platform support (untested)
- **Linux**: Avalonia cross-platform support (untested)

---

## Architectural Overview

### Component Hierarchy

```
┌─────────────────────────────────────────────────────────────────┐
│                         App.axaml.cs                            │
│                    (DI Configuration, Startup)                  │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                        MainWindow                               │
│                    (Root Window Host)                           │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    NavigationService                            │
│              (View/ViewModel Routing, Back Stack)               │
└─────────────────────────────────────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        ▼                       ▼                       ▼
┌───────────────┐       ┌───────────────┐       ┌───────────────┐
│   Controllers │       │   ViewModels  │       │    Services   │
│ (Orchestrate  │◄─────►│  (UI State,   │◄─────►│ (UI Helpers,  │
│  Workflows)   │       │   Commands)   │       │  Integration) │
└───────────────┘       └───────────────┘       └───────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                          Views                                  │
│                (XAML Layout, Data Binding)                      │
└─────────────────────────────────────────────────────────────────┘
```

### Data Flow Pattern

```
User Input → View → Command (ViewModel) → Controller → Engine
     ↑                                                    │
     └───── Observable Property ← ViewModel ← Service ←──┘
```

1. **User Action**: Click button, press key, select item
2. **Command Execution**: ReactiveCommand invoked in ViewModel
3. **Controller Orchestration**: Complex workflow logic executed
4. **Engine Delegation**: Game logic performed by Engine services
5. **Result Propagation**: Engine returns result to Controller
6. **ViewModel Update**: Observable properties updated
7. **View Binding**: XAML bindings auto-refresh UI elements

---

## ViewModels Specification

### ViewModelBase (Foundation Class)

All ViewModels extend `ViewModelBase`, providing:

```csharp
public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }
    protected CompositeDisposable Disposables { get; }

    protected virtual void OnActivated() { }
    protected virtual void OnDeactivated() { }
}
```

**Lifecycle Management**:
- `OnActivated()`: Called when view becomes visible (initialize subscriptions)
- `OnDeactivated()`: Called when view is hidden (cleanup subscriptions)
- `Disposables`: Auto-disposed when ViewModel is deactivated

### ViewModel Registry

| ViewModel | Size | Purpose | Key Commands |
|-----------|------|---------|--------------|
| **MainWindowViewModel** | 11KB | Root navigation container | NavigateTo, GoBack |
| **MenuViewModel** | 9KB | Main menu navigation | NewGame, LoadGame, Settings, Quit |
| **CharacterCreationViewModel** | 35KB | Character creation workflow | SelectClass, AllocatePoints, Confirm |
| **CharacterSheetViewModel** | 18KB | Character details display | EquipItem, UnequipItem, ViewStats |
| **DungeonExplorationViewModel** | 57KB | Dungeon traversal | Move, Interact, OpenInventory, UseAbility |
| **CombatViewModel** | 39KB | Turn-based combat | Attack, Defend, UseAbility, UseItem, Flee |
| **BossCombatViewModel** | 14KB | Boss-specific combat | (inherits CombatViewModel) |
| **InventoryViewModel** | 27KB | Item management | EquipItem, UseItem, DropItem, SortInventory |
| **SpecializationTreeViewModel** | 32KB | Skill tree allocation | UnlockAbility, SelectSpecialization |
| **SaveLoadViewModel** | 18KB | Save game management | Save, Load, Delete, CreateNew |
| **SettingsViewModel** | 27KB | Game settings | ApplySettings, ResetDefaults |
| **MetaProgressionViewModel** | 14KB | Achievements & stats | ViewAchievement, ClaimReward |
| **EndgameModeViewModel** | 21KB | Endgame mode selection | SelectMode, ApplyModifiers |
| **MinimapViewModel** | 17KB | Map visualization | ToggleZoom, CenterOnPlayer |
| **VictoryScreenViewModel** | 9KB | Victory display | CollectLoot, Continue |
| **DeathScreenViewModel** | 7KB | Death handling | Respawn, LoadSave, ReturnToMenu |
| **HelpViewModel** | 9KB | Help/tutorial content | SearchHelp, OpenSection |
| **SearchResultViewModel** | 10KB | Search result display | SelectResult, FilterResults |
| **SpriteDemoViewModel** | 4KB | Sprite testing | LoadSprite, ChangeScale |

### ViewModel Communication Patterns

**Pattern 1: Direct Property Observation**
```csharp
// In ViewModel
this.WhenAnyValue(x => x.SelectedItem)
    .Where(item => item != null)
    .Subscribe(item => LoadItemDetails(item));
```

**Pattern 2: Command with CanExecute**
```csharp
var canAttack = this.WhenAnyValue(x => x.IsPlayerTurn, x => x.HasValidTarget,
    (turn, target) => turn && target);
AttackCommand = ReactiveCommand.Create(ExecuteAttack, canAttack);
```

**Pattern 3: Service Callback**
```csharp
_combatService.OnCombatEnded
    .Subscribe(result => HandleCombatEnd(result))
    .DisposeWith(Disposables);
```

---

## Views Specification

### View Registry

| View | Size | ViewModel | Key Components |
|------|------|-----------|----------------|
| **MainWindow.axaml** | 1KB | MainWindowViewModel | ContentPresenter, navigation host |
| **MenuView.axaml** | 3KB | MenuViewModel | Button grid, version display |
| **CharacterCreationView.axaml** | 29KB | CharacterCreationViewModel | Class selector, stat allocator, preview |
| **CharacterSheetView.axaml** | 27KB | CharacterSheetViewModel | Stat display, equipment slots, ability list |
| **DungeonExplorationView.axaml** | 45KB | DungeonExplorationViewModel | Room display, action buttons, minimap |
| **CombatView.axaml** | 9KB | CombatViewModel | Combat grid, action menu, turn indicator |
| **InventoryView.axaml** | 22KB | InventoryViewModel | Item grid, equipment panel, item details |
| **SpecializationTreeView.axaml** | 31KB | SpecializationTreeViewModel | Skill tree visualization, unlock panel |
| **SaveLoadView.axaml** | 18KB | SaveLoadViewModel | Save slot list, character preview |
| **SettingsView.axaml** | 24KB | SettingsViewModel | Settings categories, sliders, toggles |
| **MetaProgressionView.axaml** | 24KB | MetaProgressionViewModel | Achievement grid, stats display |
| **EndgameModeView.axaml** | 34KB | EndgameModeViewModel | Mode selector, modifier toggles |
| **MinimapView.axaml** | 12KB | MinimapViewModel | Map canvas, legend |
| **VictoryScreenView.axaml** | 11KB | VictoryScreenViewModel | Loot display, continue button |
| **DeathScreenView.axaml** | 9KB | DeathScreenViewModel | Death message, respawn options |
| **HelpView.axaml** | 15KB | HelpViewModel | Help index, content panel |
| **SpriteDemoView.axaml** | 5KB | SpriteDemoViewModel | Sprite display, controls |

### XAML Binding Patterns

**Standard Property Binding**:
```xml
<TextBlock Text="{Binding CharacterName}" />
```

**Two-Way Binding with Validation**:
```xml
<TextBox Text="{Binding PlayerName, Mode=TwoWay}" />
```

**Command Binding**:
```xml
<Button Command="{Binding AttackCommand}" Content="Attack" />
```

**Collection Binding with Template**:
```xml
<ItemsControl ItemsSource="{Binding Enemies}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}" />
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

**Visibility Binding with Converter**:
```xml
<Panel IsVisible="{Binding HasItems, Converter={StaticResource BoolToVisible}}" />
```

---

## Controllers Specification

### Controller Registry

| Controller | Size | Purpose | Workflow Scope |
|------------|------|---------|----------------|
| **GameStateController** | 11KB | High-level game state | Menu → Creation → Exploration → Combat |
| **MainMenuController** | 8KB | Main menu actions | New game, load, quit |
| **CharacterCreationController** | 34KB | Character setup | Class → Stats → Specialization → Confirm |
| **ExplorationController** | 16KB | Dungeon navigation | Room transitions, hazards, loot, encounters |
| **CombatController** | 23KB | Combat management | Initiative → Turns → Actions → Resolution |
| **LootController** | 9KB | Loot handling | Drop generation, inventory updates |
| **ProgressionController** | 12KB | Character advancement | XP gain, level up, ability unlock |
| **VictoryController** | 16KB | Victory handling | Rewards, next dungeon, meta-progression |
| **DeathController** | 12KB | Death handling | Loot loss, respawn, save load |

### Controller Responsibilities

Controllers bridge ViewModels and Engine:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    ViewModel    │────►│   Controller    │────►│     Engine      │
│  (UI Commands)  │     │  (Orchestrate)  │     │  (Game Logic)   │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

**Example: Combat Attack Flow**
1. `CombatViewModel.AttackCommand` executed
2. `CombatController.ExecuteAttack(target)` called
3. `CombatEngine.ResolveAttack(attacker, target)` invoked
4. Engine returns `AttackResult`
5. Controller updates combat state
6. ViewModel properties updated (HP, status effects)
7. View auto-refreshes via bindings

---

## Services Specification

### Service Categories

#### Navigation & UI Management
| Service | Interface | Purpose |
|---------|-----------|---------|
| **NavigationService** | INavigationService | View/ViewModel routing with back stack |
| **DialogService** | IDialogService | Modal dialogs (confirm, input, message) |
| **TooltipService** | ITooltipService | Rich tooltips with formatting |
| **KeyboardShortcutService** | IKeyboardShortcutService | Keyboard binding management |

#### Configuration & Settings
| Service | Interface | Purpose |
|---------|-----------|---------|
| **ConfigurationService** | IConfigurationService | Game settings persistence |
| **AudioService** | IAudioService | Audio playback, volume control |

#### Game Content & Progression
| Service | Interface | Purpose |
|---------|-----------|---------|
| **MetaProgressionService** | IMetaProgressionService | Achievements, unlocks, statistics |
| **EndgameService** | IEndgameService | Endgame mode configuration |
| **SaveGameService** | ISaveGameService | Save/load game state |

#### Visual & Animation
| Service | Interface | Purpose |
|---------|-----------|---------|
| **SpriteService** | ISpriteService | Sprite loading and caching |
| **AnimationService** | N/A | Sprite animation management |
| **StatusEffectIconService** | IStatusEffectIconService | Status effect icon display |
| **HazardVisualizationService** | N/A | Environmental hazard rendering |
| **BossDisplayService** | IBossDisplayService | Boss health bars, phase display |

### Service Injection Pattern

Services are registered in `App.axaml.cs`:

```csharp
services.AddSingleton<INavigationService, NavigationService>();
services.AddSingleton<IDialogService, DialogService>();
services.AddSingleton<ISaveGameService, SaveGameService>();
// ... additional registrations
```

Injected via constructor:

```csharp
public class CombatViewModel : ViewModelBase
{
    private readonly ICombatEngine _combatEngine;
    private readonly IDialogService _dialogService;

    public CombatViewModel(ICombatEngine combatEngine, IDialogService dialogService)
    {
        _combatEngine = combatEngine;
        _dialogService = dialogService;
    }
}
```

---

## Custom Controls Specification

### CombatGridControl (36KB)

**Purpose**: Tactical grid rendering for turn-based combat

**Features**:
- Zone/Row/Column grid architecture
- Unit sprite positioning and scaling
- Cell selection and highlighting
- Mouse interaction (click to select, hover to highlight)
- Real-time position updates during combat

**Grid Layout**:
```
┌─────────────────────────────────────┐
│          Zone: Backline             │
├─────────────────────────────────────┤
│          Zone: Midline              │
├─────────────────────────────────────┤
│          Zone: Frontline            │
├─────────────────────────────────────┤
│          Zone: Enemy Frontline      │
├─────────────────────────────────────┤
│          Zone: Enemy Midline        │
├─────────────────────────────────────┤
│          Zone: Enemy Backline       │
└─────────────────────────────────────┘
```

**Key Properties**:
- `Units`: Observable collection of combat participants
- `SelectedUnit`: Currently selected unit
- `HighlightedCells`: Cells marked for targeting
- `GridScale`: Zoom level (1x, 2x, 3x)

### MinimapControl (16KB)

**Purpose**: Dungeon map visualization

**Features**:
- Room layout rendering
- Current position indicator
- Visited/unvisited room markers
- Connection lines between rooms
- Zoom and pan support

**Key Properties**:
- `Rooms`: Observable collection of room data
- `CurrentRoom`: Player's current location
- `VisitedRooms`: Set of explored rooms
- `ZoomLevel`: Map scale factor

### BossOverlayControl (17KB)

**Purpose**: Boss encounter UI overlay

**Features**:
- Boss health bar with phase markers
- Phase transition indicators
- Special attack telegraph display
- Enrage timer visualization

**Key Properties**:
- `BossHealth`: Current/max HP values
- `CurrentPhase`: Active boss phase (1-4)
- `TelegraphedAttack`: Pending boss ability
- `EnrageTimer`: Turns until enrage

---

## Navigation System

### Navigation Flow

```
MainMenu
    ├── NewGame → CharacterCreation → Exploration
    ├── LoadGame → SaveLoad → Exploration
    ├── Settings → Settings
    ├── Help → Help
    └── Quit

Exploration
    ├── Combat (encounter) → Combat → Exploration (victory)
    │                              └── Death (defeat)
    ├── Inventory → Inventory → Exploration
    ├── Character → CharacterSheet → Exploration
    ├── Specialization → SpecializationTree → Exploration
    └── Pause → Menu

Combat
    ├── Victory → VictoryScreen → Exploration
    └── Defeat → DeathScreen → MainMenu / LoadGame

EndGame
    └── EndgameMode → Exploration (with modifiers)
```

### NavigationService API

```csharp
public interface INavigationService
{
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase;
    void GoBack();
    bool CanGoBack { get; }
    event EventHandler<ViewModelBase> CurrentViewChanged;
}
```

### View-ViewModel Registration

ViewModels are registered with factory methods:

```csharp
_navigationService.RegisterViewModelFactory<CombatViewModel>(
    () => _serviceProvider.GetRequiredService<CombatViewModel>()
);
```

---

## Data Binding & Converters

### Converter Registry

| Converter | Purpose | Input → Output |
|-----------|---------|----------------|
| **NotNullToBoolConverter** | Null checks | object → bool |
| **IntGreaterThanConverter** | Integer comparison | int → bool |
| **BoolToUpgradeConverter** | Upgrade availability | bool → brush/style |
| **BossUIConverters** | Boss health formatting | float → string/color |
| **EndgameModeConverters** | Mode display | enum → string/icon |
| **HelpConverters** | Help content formatting | HelpItem → formatted text |
| **AchievementConverters** | Achievement display | Achievement → progress bar |
| **SaveLoadConverters** | Save metadata | SaveInfo → display string |

### Binding Best Practices

1. **Use Compiled Bindings**: Project setting `AvaloniaUseCompiledBindingsByDefault=true`
2. **Validate Binding Paths**: Compile-time errors for invalid property names
3. **Prefer OneWay for Display**: Reduce binding overhead
4. **Use Converters for Formatting**: Keep ViewModels clean of display logic

---

## Theming & Style System

### UIConstants.cs

```csharp
public static class UIConstants
{
    // Font Sizes
    public static readonly double FontSizeH1 = 28;
    public static readonly double FontSizeH2 = 22;
    public static readonly double FontSizeH3 = 18;
    public static readonly double FontSizeBody = 14;
    public static readonly double FontSizeSmall = 12;
    public static readonly double FontSizeTiny = 10;
    public static readonly double FontSizeDisplay = 36;

    // Spacing
    public static readonly Thickness MarginSmall = new(4);
    public static readonly Thickness MarginMedium = new(8);
    public static readonly Thickness MarginLarge = new(16);
    public static readonly Thickness PaddingSmall = new(4);
    public static readonly Thickness PaddingMedium = new(8);
    public static readonly Thickness PaddingLarge = new(16);

    // Corner Radius
    public static readonly CornerRadius RadiusSmall = new(4);
    public static readonly CornerRadius RadiusMedium = new(8);
    public static readonly CornerRadius RadiusLarge = new(12);
}
```

### UIColors.cs

```csharp
public static class UIColors
{
    // Primary Palette
    public static readonly Color Primary = Color.Parse("#3B82F6");
    public static readonly Color Secondary = Color.Parse("#6366F1");
    public static readonly Color Accent = Color.Parse("#F59E0B");

    // Status Colors
    public static readonly Color Health = Color.Parse("#22C55E");
    public static readonly Color HealthLow = Color.Parse("#EF4444");
    public static readonly Color Mana = Color.Parse("#3B82F6");
    public static readonly Color Stamina = Color.Parse("#F59E0B");

    // UI States
    public static readonly Color Disabled = Color.Parse("#6B7280");
    public static readonly Color Hover = Color.Parse("#4B5563");
    public static readonly Color Selected = Color.Parse("#2563EB");
}
```

---

## Accessibility Features

### Implemented Accessibility Support

1. **Keyboard Navigation**
   - Tab/Shift-Tab for focus traversal
   - Arrow keys for grid/list navigation
   - Enter/Space for activation
   - Escape for cancel/back

2. **Configurable UI Scale**
   - Settings option: 100%, 125%, 150%, 175%, 200%
   - Applied to all text and UI elements

3. **Colorblind Modes**
   - Deuteranopia (red-green)
   - Protanopia (red-green)
   - Tritanopia (blue-yellow)
   - Color-coded elements use patterns + color

4. **Screen Reader Support**
   - Automation properties on interactive elements
   - Descriptive labels for buttons and controls
   - Status announcements for combat events

### Accessibility Configuration

```csharp
public class AccessibilitySettings
{
    public float UIScale { get; set; } = 1.0f;
    public ColorblindMode ColorblindMode { get; set; } = ColorblindMode.None;
    public bool ScreenReaderEnabled { get; set; } = false;
    public bool ReducedMotion { get; set; } = false;
    public bool HighContrastMode { get; set; } = false;
}
```

---

## Performance Considerations

### UI Threading Model

- **UI Thread**: All property updates must occur on UI thread
- **Background Work**: Long operations use `Task.Run()` or async patterns
- **Dispatcher**: `Dispatcher.UIThread.Post()` for cross-thread UI updates

### Rendering Optimization

1. **Virtualization**: ItemsControl with virtualization for large lists
2. **Deferred Loading**: Lazy-load view content until visible
3. **Sprite Caching**: Pre-rendered sprites cached in SpriteService
4. **Binding Throttling**: Use `.Throttle()` on rapidly-changing observables

### Memory Management

1. **Subscription Disposal**: All subscriptions added to `Disposables`
2. **Image Disposal**: Sprite bitmaps disposed when no longer needed
3. **View Recycling**: Views recycled in virtualized lists

### Performance Targets

| Metric | Target | Notes |
|--------|--------|-------|
| Frame Time | <16ms | 60 FPS minimum |
| Navigation | <100ms | View transition time |
| Combat Grid Update | <50ms | After each action |
| Save/Load UI | <200ms | List population |

---

## Testing Strategy

### Unit Testing

- **ViewModels**: Test commands, property updates, validation
- **Controllers**: Test workflow logic with mocked services
- **Services**: Test service behavior with mocked dependencies
- **Converters**: Test value conversion edge cases

### Integration Testing

- **Navigation Flow**: Verify screen transitions work correctly
- **Data Binding**: Ensure bindings don't break on data changes
- **Save/Load Integration**: Test UI with real persistence layer

### Manual Testing Checklist

- [ ] All buttons responsive and correctly enabled/disabled
- [ ] Keyboard shortcuts work as documented
- [ ] Tab order is logical
- [ ] No visual glitches on window resize
- [ ] Text readable at all UI scale settings
- [ ] Colorblind modes differentiate elements

---

## Known Limitations & Future Work

### Current Limitations

1. **Single Window**: No multi-window support
2. **Fixed Aspect Ratio**: Optimized for 16:9, may have layout issues at extreme ratios
3. **No Touch Support**: Mouse/keyboard only
4. **Windows Primary**: macOS/Linux untested

### Planned Enhancements

1. **Tooltip Improvements**: Rich tooltips with item comparisons
2. **Animation System**: Combat action animations
3. **Sound Integration**: UI feedback sounds
4. **Localization**: Multi-language support infrastructure
5. **Gamepad Support**: Controller input mapping

---

## Appendix A: DI Container Registration

Complete service registration in `App.axaml.cs`:

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Core Services
    services.AddSingleton<INavigationService, NavigationService>();
    services.AddSingleton<IDialogService, DialogService>();
    services.AddSingleton<ITooltipService, TooltipService>();
    services.AddSingleton<IKeyboardShortcutService, KeyboardShortcutService>();

    // Configuration
    services.AddSingleton<IConfigurationService, ConfigurationService>();
    services.AddSingleton<IAudioService, AudioService>();

    // Game Services
    services.AddSingleton<ISaveGameService, SaveGameService>();
    services.AddSingleton<IMetaProgressionService, MetaProgressionService>();
    services.AddSingleton<IEndgameService, EndgameService>();

    // Visual Services
    services.AddSingleton<ISpriteService, SpriteService>();
    services.AddSingleton<IStatusEffectIconService, StatusEffectIconService>();
    services.AddSingleton<IBossDisplayService, BossDisplayService>();
    services.AddSingleton<HazardVisualizationService>();
    services.AddSingleton<AnimationService>();

    // Controllers
    services.AddTransient<GameStateController>();
    services.AddTransient<MainMenuController>();
    services.AddTransient<CharacterCreationController>();
    services.AddTransient<ExplorationController>();
    services.AddTransient<CombatController>();
    services.AddTransient<LootController>();
    services.AddTransient<ProgressionController>();
    services.AddTransient<VictoryController>();
    services.AddTransient<DeathController>();

    // ViewModels
    services.AddTransient<MainWindowViewModel>();
    services.AddTransient<MenuViewModel>();
    services.AddTransient<CharacterCreationViewModel>();
    services.AddTransient<CharacterSheetViewModel>();
    services.AddTransient<DungeonExplorationViewModel>();
    services.AddTransient<CombatViewModel>();
    services.AddTransient<BossCombatViewModel>();
    services.AddTransient<InventoryViewModel>();
    services.AddTransient<SpecializationTreeViewModel>();
    services.AddTransient<SaveLoadViewModel>();
    services.AddTransient<SettingsViewModel>();
    services.AddTransient<MetaProgressionViewModel>();
    services.AddTransient<EndgameModeViewModel>();
    services.AddTransient<MinimapViewModel>();
    services.AddTransient<VictoryScreenViewModel>();
    services.AddTransient<DeathScreenViewModel>();
    services.AddTransient<HelpViewModel>();

    // Engine Services (from RuneAndRust.Engine)
    services.AddSingleton<ICombatEngine, CombatEngine>();
    services.AddSingleton<IDiceService, DiceService>();
    services.AddSingleton<IEnemyAI, EnemyAI>();
    // ... additional engine services
}
```

---

## Appendix B: Screen Flow Diagram

```
┌──────────────────────────────────────────────────────────────────────┐
│                            MAIN MENU                                 │
│  ┌─────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌─────────┐ │
│  │ New Game│  │ Continue │  │ Load Game│  │ Settings │  │  Quit   │ │
│  └────┬────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  └─────────┘ │
└───────│───────────│─────────────│─────────────│────────────────────┘
        │            │             │             │
        ▼            │             ▼             ▼
┌───────────────┐    │     ┌───────────────┐  ┌───────────────┐
│   CHARACTER   │    │     │   SAVE/LOAD   │  │   SETTINGS    │
│   CREATION    │    │     │    SCREEN     │  │    SCREEN     │
│ ┌───────────┐ │    │     │ ┌───────────┐ │  │ ┌───────────┐ │
│ │Select     │ │    │     │ │ Save Slot │ │  │ │ Graphics  │ │
│ │Class      │ │    │     │ │   List    │ │  │ │ Audio     │ │
│ │Allocate   │ │    │     │ │ Character │ │  │ │ Gameplay  │ │
│ │Stats      │ │    │     │ │  Preview  │ │  │ │ Controls  │ │
│ │Confirm    │ │    │     │ └───────────┘ │  │ └───────────┘ │
│ └───────────┘ │    │     └───────┬───────┘  └───────────────┘
└───────┬───────┘    │             │
        │            │             │
        ▼            ▼             │
┌───────────────────────────────────────────────────────────────┐
│                       EXPLORATION                              │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │  Room Display  │  Actions  │  Minimap  │  Character Info │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                │
│  Events: Move, Search, Interact, Encounter, Rest, Boss        │
└────────────────────────────────────────────────────────────────┘
        │                       │                   │
        ▼                       ▼                   ▼
┌───────────────┐     ┌──────────────┐     ┌────────────────┐
│    COMBAT     │     │  INVENTORY   │     │  CHARACTER     │
│ ┌───────────┐ │     │ ┌──────────┐ │     │    SHEET       │
│ │Combat Grid│ │     │ │Item Grid │ │     │ ┌────────────┐ │
│ │Action Menu│ │     │ │Equipment │ │     │ │ Stats      │ │
│ │Turn Order │ │     │ │Details   │ │     │ │ Equipment  │ │
│ │Combat Log │ │     │ └──────────┘ │     │ │ Abilities  │ │
│ └───────────┘ │     └──────────────┘     │ └────────────┘ │
└───────┬───────┘                          └────────────────┘
        │
   ┌────┴────┐
   ▼         ▼
┌─────────┐ ┌─────────┐
│ VICTORY │ │  DEATH  │
│ SCREEN  │ │ SCREEN  │
└─────────┘ └─────────┘
```

---

## Appendix C: Keyboard Shortcuts

| Context | Shortcut | Action |
|---------|----------|--------|
| **Global** | Escape | Back / Cancel / Pause |
| **Global** | F1 | Open Help |
| **Global** | F5 | Quick Save |
| **Global** | F9 | Quick Load |
| **Exploration** | I | Open Inventory |
| **Exploration** | C | Open Character Sheet |
| **Exploration** | M | Toggle Minimap |
| **Exploration** | Tab | Cycle Selection |
| **Combat** | 1-9 | Select Action Slot |
| **Combat** | A | Attack |
| **Combat** | D | Defend |
| **Combat** | F | Flee |
| **Combat** | Space | Confirm Selection |
| **Inventory** | E | Equip Selected |
| **Inventory** | U | Use Selected |
| **Inventory** | X | Drop Selected |

---

**End of Specification**
