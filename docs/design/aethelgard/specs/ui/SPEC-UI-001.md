---
id: SPEC-UI-001
title: UI Framework System
version: 1.1.0
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-RENDER-001, SPEC-INPUT-001, SPEC-COMBAT-001, SPEC-CHAR-001]
---

# SPEC-UI-001: UI Framework System

> **Version:** 1.1.0
> **Status:** Implemented
> **Services:** Multiple (ViewModels, GameService, DI Container)
> **Location:** `RuneAndRust.Core/ViewModels/`, `RuneAndRust.Engine/Services/GameService.cs`

---

## Overview

The UI Framework System provides the architectural foundation for Rune & Rust's terminal-based user interface using **Spectre.Console** for rendering. It implements a **Model-View-Service pattern** where immutable C# record ViewModels serve as data transfer objects between game logic and rendering layers, eschewing traditional MVVM frameworks like ReactiveUI in favor of simplicity and testability.

This system manages screen lifecycle, phase-based rendering, and dependency injection for 50+ services across 8 primary game screens.

---

## Core Concepts

### ViewModel Pattern (Immutable Records)

**Definition:** ViewModels are **immutable C# records** (not ReactiveUI `ReactiveObject`), built by service layer methods and consumed by renderers.

**Characteristics:**
- **Immutability:** Records cannot be mutated after creation (thread-safe snapshots)
- **Builder Pattern:** Services expose `BuildViewModel()` methods
- **No Binding:** No data binding or observable properties (immediate mode rendering)
- **DTO Role:** Pure data transfer objects with no logic

**Example:**
```csharp
public record CombatViewModel(
    string CharacterName,
    int CurrentHp,
    int MaxHp,
    List<CombatantView> TurnOrder,
    List<string> CombatLog,
    List<AbilityView> Abilities
);
```

---

### Screen Lifecycle

**Pattern:** Service → ViewModel → Renderer → Spectre.Console

**Flow:**
```
1. GameService game loop reads GameState.Phase
2. Service builds ViewModel from current game state
3. Renderer consumes ViewModel
4. Spectre.Console writes to terminal
5. InputHandler blocks for user input
6. CommandParser processes input
7. Service updates GameState
8. Loop restarts (rebuild ViewModel)
```

---

### Phase Management

**GamePhase Enum** (from GameState):
```csharp
public enum GamePhase
{
    MainMenu = 0,       // Main menu phase
    Exploration = 1,    // World navigation phase
    Combat = 2,         // Turn-based combat phase
    Quit = 3            // Application exit signal
}
```

**Phase-Based Rendering:**
- Each phase maps to specific ViewModel/Renderer pair
- GameService.RunAsync() contains phase dispatch logic
- Renderers are nullable (enables headless testing)

---

## Behaviors

### Primary Behaviors

#### 1. Build ViewModel (`BuildViewModel`) - SERVICE LAYER PATTERN

**Pattern across services:**
```csharp
// CombatService - builds from internal CombatState
CombatViewModel GetViewModel()

// GameService - private method builds exploration view
Task<ExplorationViewModel> BuildExplorationViewModelAsync()

// CraftingService - builds from character and trade parameters
CraftingViewModel BuildViewModel(Character crafter, CraftingTrade trade, int selectedIndex = 0)

// JournalService - builds from character context
JournalViewModel BuildViewModelAsync(Guid characterId, string characterName, JournalTab tab, int selectedIndex = 0, int stressLevel = 0)
```

**Sequence (Example: CraftingViewModel):**
1. Load character from repository
2. Load recipes filtered by trade
3. Map ingredients to availability status
4. Build selected recipe details (if index valid)
5. Return immutable record

**Example:**
```csharp
// CraftingService.cs
public CraftingViewModel BuildViewModel(Character crafter, CraftingTrade trade, int selectedIndex)
{
    var recipes = _recipes
        .Where(r => r.Trade == trade)
        .Select(r => new RecipeView(
            r.Name,
            r.Difficulty,
            r.Ingredients.Select(ing => new IngredientView(
                ing.Name,
                ing.Quantity,
                _inventoryService.HasItem(crafter, ing.ItemId, ing.Quantity)
            )).ToList()
        ))
        .ToList();

    RecipeDetailsView? details = null;
    if (selectedIndex >= 0 && selectedIndex < recipes.Count)
    {
        details = BuildRecipeDetails(recipes[selectedIndex]);
    }

    return new CraftingViewModel(
        crafter.Name,
        crafter.GetEffectiveAttribute(Attribute.Wits),
        trade,
        recipes,
        selectedIndex,
        details
    );
}
```

---

#### 2. Game Loop (`RunAsync`) - GAMESERVICE ORCHESTRATION

**Location:** `GameService.cs` (lines 60-99)

**Sequence:**
```csharp
public async Task RunAsync()
{
    while (_state.Phase != GamePhase.Quit)
    {
        // 1. Render phase-specific UI
        if (_state.Phase == GamePhase.Combat && _combatRenderer != null)
        {
            var viewModel = _combatService.GetViewModel();
            if (viewModel != null) _combatRenderer.Render(viewModel);
        }
        else if (_state.Phase == GamePhase.Exploration && _explorationRenderer != null)
        {
            var viewModel = await BuildExplorationViewModelAsync();
            if (viewModel != null) _explorationRenderer.Render(viewModel);
        }

        // 2. Get input (blocking)
        string input = _inputHandler.GetInput(prompt);

        // 3. Process command
        await _parser.ParseAndExecuteAsync(input, _state);
    }
}
```

**Key Characteristics:**
- **Blocking Input:** `GetInput()` pauses game loop until user action
- **Nullable Renderers:** Enables headless mode for testing
- **Async Support:** Journal and rest commands use async/await
- **Phase Dispatch:** Different render paths per GamePhase

---

#### 3. DI Registration (`ConfigureServices`) - DEPENDENCY INJECTION

**Location:** `Program.cs` (lines 97-159)

**Sequence:**
1. Register Singletons (GameState, InputHandler, Theme, Renderers)
2. Register Scoped services (Repositories, most game services)
3. Register Transient services (none currently)
4. Build ServiceProvider
5. Resolve and start GameService

**Example Registrations:**
```csharp
// Core State
services.AddSingleton<GameState>();

// Input/Output
services.AddSingleton<IInputHandler, TerminalInputHandler>();
services.AddSingleton<IThemeService, ThemeService>();

// Screen Renderers (all Singleton)
services.AddSingleton<ICombatScreenRenderer, CombatScreenRenderer>();
services.AddSingleton<IExplorationScreenRenderer, ExplorationScreenRenderer>();
services.AddSingleton<IInventoryScreenRenderer, InventoryScreenRenderer>();
services.AddSingleton<ICraftingScreenRenderer, CraftingScreenRenderer>();
services.AddSingleton<IJournalScreenRenderer, JournalScreenRenderer>();
services.AddSingleton<IOptionsScreenRenderer, OptionsScreenRenderer>();
services.AddSingleton<IRestScreenRenderer, RestScreenRenderer>();
services.AddSingleton<IVictoryScreenRenderer, VictoryScreenRenderer>();

// Game Services (Scoped for DB context)
services.AddScoped<ICraftingService, CraftingService>();
services.AddScoped<IJournalService, JournalService>();
services.AddScoped<IInventoryService, InventoryService>();
services.AddSingleton<ICombatService, CombatService>();
```

**Scoping Strategy:**
- **Singleton:** Stateless renderers, theme service, input handler, game state
- **Scoped:** Services that access database (created per operation)
- **Transient:** Not currently used

---

## Restrictions

### Immutability Constraints

**ViewModels Cannot:**
- Be mutated after creation (C# record init-only properties)
- Contain logic beyond computed properties
- Hold references to mutable state
- Subscribe to events or observables

**Impact:** Ensures thread safety and predictable rendering.

---

### Phase Transition Rules

**Valid Transitions:**
```
MainMenu → Exploration
Exploration ↔ Combat
Any → Quit
```

**Note:** CharacterCreation and Rest are handled as modal overlays within Exploration phase, not separate GamePhase values.

---

## Limitations

### Numerical Bounds

| Constraint | Value | Notes |
|------------|-------|-------|
| Max registered services | Unbounded | Currently 50+ |
| ViewModel rebuild frequency | Once per user action | Not continuous |
| Renderer null-checks | Required | Enables headless testing |

---

### System Gaps

- **No Avalonia/ReactiveUI** - Terminal-only, no GUI framework
- **No Data Binding** - Immediate mode rendering, no observables
- **No XAML** - No declarative UI markup
- **No Navigation Service** - Phase transitions handled by GameService directly
- **No ViewModel Base Class** - Each ViewModel is independent record
- **No Command Pattern** - Actions processed via CommandParser, not ICommand

---

## Use Cases

### UC-1: Combat Screen Rendering Flow

```csharp
// GameService.RunAsync() - Combat phase
if (_state.Phase == GamePhase.Combat && _combatRenderer != null)
{
    // 1. Service builds ViewModel from current combat state
    var viewModel = _combatService.GetViewModel();

    // ViewModel contains:
    // - TurnOrder (list of combatants with HP, status effects)
    // - CombatLog (last 10 combat events)
    // - PlayerAbilities (hotkeyed actions with cooldowns)
    // - ActiveCombatant (current turn indicator)

    // 2. Renderer consumes immutable snapshot
    _combatRenderer.Render(viewModel);

    // 3. Terminal displays:
    // ┌─ Combat Grid ──────────────┐
    // │ ENEMY FRONT: >Rust-Husk !!! │
    // │ FRONT LINE: >Thane *       │
    // └────────────────────────────┘
    // >

    // 4. InputHandler blocks for user input
    string input = _inputHandler.GetInput(">");

    // 5. CommandParser executes action
    await _parser.ParseAndExecuteAsync(input, _state);

    // 6. CombatService updates state (HP changes, turn advances)
    // 7. Loop restarts, NEW ViewModel built with updated state
}
```

**Narrative Impact:** Each combat action triggers full screen refresh with updated HP, statuses, and turn order.

---

### UC-2: Exploration Screen with Modal Overlay

```csharp
// GameService.RunAsync() - Exploration phase
if (_state.Phase == GamePhase.Exploration)
{
    var viewModel = await BuildExplorationViewModelAsync();
    _explorationRenderer.Render(viewModel);

    string input = _inputHandler.GetInput(">");

    // User presses 'I' for inventory
    if (input.ToLowerInvariant() == "i")
    {
        // Modal overlay - blocks main loop
        await ShowInventoryModalAsync();
    }
}

private async Task ShowInventoryModalAsync()
{
    bool exitRequested = false;

    while (!exitRequested)
    {
        // Build ViewModel for current selection
        var vm = _inventoryService.BuildViewModel(
            _state.CurrentCharacter,
            selectedIndex
        );

        // Render inventory screen (full-screen modal)
        _inventoryRenderer.Render(vm);

        // Get key press (arrow keys, enter, escape)
        var key = Console.ReadKey(intercept: true);

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                selectedIndex = Math.Max(0, selectedIndex - 1);
                break;
            case ConsoleKey.DownArrow:
                selectedIndex = Math.Min(vm.Items.Count - 1, selectedIndex + 1);
                break;
            case ConsoleKey.Escape:
                exitRequested = true;
                break;
        }

        // Rebuild ViewModel with new selection for next frame
    }

    // Return to exploration screen (rebuilds exploration ViewModel)
}
```

**Narrative Impact:** Modal screens (Inventory, Journal, Crafting) pause exploration loop, exit restores exploration state.

---

### UC-3: ViewModel Immutability Guarantees Thread Safety

```csharp
// CombatService builds ViewModel
var combatVm = new CombatViewModel(
    CharacterName: "Thane",
    CurrentHp: 45,
    MaxHp: 100,
    TurnOrder: turnOrderList,
    CombatLog: logList,
    Abilities: abilitiesList
);

// Renderer consumes ViewModel on main thread
_combatRenderer.Render(combatVm);

// Meanwhile, async background task updates loot tables
await Task.Run(() => _lootService.RegenerateTablesAsync());

// CombatViewModel remains unchanged (immutable)
// No race conditions, no defensive copying needed
// Next loop iteration builds NEW ViewModel with updated state
```

**Narrative Impact:** Immutability eliminates entire class of concurrency bugs without locks.

---

### UC-4: Headless Testing (Nullable Renderers)

```csharp
// In unit tests, renderers are null
var gameState = new GameState { Phase = GamePhase.Combat };
var combatService = new CombatService(...);
var gameService = new GameService(
    gameState,
    combatService,
    combatRenderer: null,  // No renderer registered
    inputHandler: mockInputHandler
);

// GameService game loop handles null renderers gracefully
if (_state.Phase == GamePhase.Combat && _combatRenderer != null)
{
    // This block is skipped in tests
    _combatRenderer.Render(viewModel);
}

// Test can verify GameState changes without UI rendering
await gameService.ProcessCombatTurnAsync("attack rust-husk");
gameState.TurnCount.Should().Be(2);  // Turn advanced
```

**Narrative Impact:** Separation of ViewModel building from rendering enables fast unit testing.

---

### UC-5: Async ViewModel Building (Journal)

```csharp
// GameService.BuildExplorationViewModelAsync()
private async Task<ExplorationViewModel> BuildExplorationViewModelAsync()
{
    var character = _state.CurrentCharacter;
    var room = await _roomRepo.GetByIdAsync(_state.CurrentRoomId);

    // Async database query for journal entries
    var journalVm = await _journalService.BuildViewModelAsync(
        character.Id,
        character.Name,
        JournalTab.Codex,
        selectedIndex: 0,
        stressLevel: character.StressLevel
    );

    return new ExplorationViewModel(
        CharacterName: character.Name,
        RoomName: room.Name,
        RoomDescription: room.Description,
        Exits: room.Connections.Select(c => c.Direction).ToList(),
        VisibleObjects: await LoadVisibleObjectsAsync(room.Id),
        JournalEntriesCount: journalVm.Entries.Count
    );
}
```

**Narrative Impact:** Database-heavy ViewModels use async/await without blocking main thread.

---

### UC-6: DI Scope Management for Database Access

```csharp
// Program.cs - Services are Scoped for DB context lifecycle
services.AddScoped<IRepository<Character>, CharacterRepository>();
services.AddScoped<ICraftingService, CraftingService>();

// GameService.cs - Create scope per operation
using (var scope = _serviceProvider.CreateScope())
{
    var craftingService = scope.ServiceProvider.GetRequiredService<ICraftingService>();

    // CraftingService has scoped EF Core DbContext
    var viewModel = craftingService.BuildViewModel(character, trade, index);

    // Scope disposes here, DbContext closes
}

// Next operation creates NEW scope with fresh DbContext
// Prevents "DbContext already disposed" errors
```

**Narrative Impact:** Scoped services prevent database connection leaks and context disposal errors.

---

## Decision Trees

### Screen Rendering Dispatch

```
┌─────────────────────────────────┐
│  GameService.RunAsync()         │
│  while (Phase != Quit)          │
└────────────┬────────────────────┘
             │
    ┌────────┴────────┐
    │ Check Phase     │
    └────────┬────────┘
         ┌───┴────────────────┐
         │                    │
    MainMenu            Exploration
         │                    │
         ▼                    ▼
    ┌─────────┐         ┌──────────┐
    │ Show    │         │ Build    │
    │ Title   │         │ Explor   │
    │ Screen  │         │ VM       │
    └─────────┘         └────┬─────┘
                             │
                        ┌────┴────┐
                        │ Render  │
                        └────┬────┘
                             │
                        ┌────┴────┐
                        │ Get     │
                        │ Input   │
                        └────┬────┘
                             │
                        ┌────┴────┐
                        │ Parse   │
                        │ Command │
                        └────┬────┘
                             │
                    ┌────────┴────────┐
                    │                 │
               Combat              Navigation
                    │                 │
              ┌─────┴─────┐          ▼
              │ Phase =   │     Update Room
              │ Combat    │     (stay Exploration)
              └───────────┘

         Combat                Rest
             │                  │
             ▼                  ▼
        ┌─────────┐        ┌─────────┐
        │ Build   │        │ Build   │
        │ Combat  │        │ Rest    │
        │ VM      │        │ VM      │
        └────┬────┘        └────┬────┘
             │                  │
        ┌────┴────┐        ┌────┴────┐
        │ Render  │        │ Render  │
        │ Combat  │        │ Rest    │
        │ Screen  │        │ Screen  │
        └─────────┘        └─────────┘
```

---

### ViewModel Lifecycle

```
┌─────────────────────────────────┐
│  User Action (Enter, Click)     │
└────────────┬────────────────────┘
             │
    ┌────────┴────────┐
    │ GameState       │
    │ Updated         │
    └────────┬────────┘
             │
    ┌────────┴────────────┐
    │ Service.Build       │
    │ ViewModel()         │
    └────────┬────────────┘
             │
    ┌────────┴──────────┐
    │ Query Repositories│
    └────────┬──────────┘
             │
    ┌────────┴──────────┐
    │ Map Domain Models │
    │ to View Models    │
    └────────┬──────────┘
             │
    ┌────────┴──────────┐
    │ Create Immutable  │
    │ Record            │
    └────────┬──────────┘
             │
    ┌────────┴──────────┐
    │ Return to         │
    │ GameService       │
    └────────┬──────────┘
             │
    ┌────────┴──────────┐
    │ Renderer.Render   │
    │ (viewModel)       │
    └────────┬──────────┘
             │
    ┌────────┴──────────┐
    │ Spectre.Console   │
    │ Output            │
    └────────┬──────────┘
             │
    ┌────────┴──────────┐
    │ ViewModel GC'd    │
    │ (out of scope)    │
    └───────────────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IRepository<Character>` | Infrastructure | Load character for ViewModel building |
| `IRepository<Room>` | Infrastructure | Load room data for ExplorationViewModel |
| `IThemeService` | [SPEC-THEME-001](SPEC-THEME-001.md) | Color palette for UI rendering |
| `IInputHandler` | [SPEC-INPUT-001](SPEC-INPUT-001.md) | User input abstraction |
| `ILogger` | Infrastructure | Game loop event tracing |

### Dependents (Provides To)

| Service | Specification | Usage |
|---------|---------------|-------|
| Screen Renderers | [SPEC-RENDER-001](SPEC-RENDER-001.md) | Consume ViewModels for rendering |
| `CommandParser` | [SPEC-INPUT-001](SPEC-INPUT-001.md) | Processes input after ViewModel rendering |
| `CombatService` | [SPEC-COMBAT-001](SPEC-COMBAT-001.md) | Builds CombatViewModel from combat state |

### Related Systems

- [SPEC-RENDER-001](SPEC-RENDER-001.md) - **Rendering Pipeline**: Consumes ViewModels to render Spectre.Console output
- [SPEC-INPUT-001](SPEC-INPUT-001.md) - **Input Handling**: Provides user input to game loop
- [SPEC-COMBAT-001](SPEC-COMBAT-001.md) - **Combat System**: Primary ViewModel builder for Combat phase

---

## Related Services

### Primary Implementation

| File | Purpose |
|------|----------|
| `GameService.cs` | Game loop orchestration, phase dispatch |
| `Program.cs` | DI container configuration |

### Supporting Types

| File | Purpose |
|------|----------|
| `CombatViewModel.cs` | Combat screen data snapshot |
| `ExplorationViewModel.cs` | Exploration screen data snapshot |
| `InventoryViewModel.cs` | Inventory modal data snapshot |
| `JournalViewModel.cs` | Journal modal data snapshot |
| `CraftingViewModel.cs` | Crafting modal data snapshot |
| `OptionsViewModel.cs` | Options modal data (mutable class, not record) |

---

## Data Models

### CombatViewModel (Immutable Record)

```csharp
public record CombatViewModel(
    int RoundNumber,
    string ActiveCombatantName,
    List<CombatantView> TurnOrder,
    List<string> CombatLog,
    PlayerStatsView PlayerStats,
    List<AbilityView>? PlayerAbilities = null,
    // Row-grouped combatants for grid display (v0.3.6a)
    List<CombatantView>? PlayerFrontRow = null,
    List<CombatantView>? PlayerBackRow = null,
    List<CombatantView>? EnemyFrontRow = null,
    List<CombatantView>? EnemyBackRow = null,
    // Timeline projection (v0.3.6b)
    List<TimelineEntryView>? TimelineProjection = null,
    // Context help tips (v0.3.9c)
    List<HelpTip>? ContextTips = null
);

// Nested record for player statistics
public record PlayerStatsView(
    int CurrentHp, int MaxHp,
    int CurrentStamina, int MaxStamina,
    int CurrentStress, int MaxStress,
    int CurrentCorruption, int MaxCorruption
);
```

---

### ExplorationViewModel (Immutable Record)

```csharp
public record ExplorationViewModel(
    string CharacterName,
    int CurrentHp,
    int MaxHp,
    int CurrentStamina,
    int MaxStamina,
    int CurrentStress,
    int MaxStress,
    int CurrentCorruption,
    int MaxCorruption,
    string RoomName,
    string RoomDescription,
    int TurnCount,
    Coordinate PlayerPosition,
    List<Room> LocalMapRooms,
    HashSet<Guid> VisitedRoomIds,
    List<string> VisibleObjects,
    List<string> VisibleEnemies,
    string Exits,                    // Comma-separated lowercase directions
    string BiomeColor,               // Spectre.Console color name
    List<HelpTip>? ContextTips = null
);
```

---

### InventoryViewModel (Immutable Record)

```csharp
public record InventoryViewModel(
    string CharacterName,
    List<EquipmentSlotView> EquippedItems,
    List<InventoryItemView> BackpackItems,
    int CurrentWeight,
    int MaxWeight,
    BurdenState BurdenState,
    int SelectedIndex
);
```

---

### JournalViewModel (Immutable Record)

```csharp
public record JournalViewModel(
    string CharacterName,
    JournalTab ActiveTab,
    List<JournalEntryView> Entries,
    int SelectedIndex,
    JournalEntryDetailView? SelectedEntryDetails
);
```

---

### CraftingViewModel (Immutable Record)

```csharp
public record CraftingViewModel(
    string CharacterName,
    int CrafterWits,
    CraftingTrade SelectedTrade,
    List<RecipeView> Recipes,
    int SelectedRecipeIndex,
    RecipeDetailsView? SelectedRecipeDetails
);
```

---

### OptionsViewModel (Mutable Class - Exception)

```csharp
public class OptionsViewModel
{
    public OptionsTab ActiveTab { get; set; } = OptionsTab.General;
    public int SelectedIndex { get; set; } = 0;
    public List<SettingItemView> CurrentItems { get; set; } = new();

    // Mutable because settings are modified during modal loop
}
```

**Rationale:** Settings UI requires in-place modification before commit, breaking immutability pattern.

---

## Configuration

### DI Service Lifetimes

**Singleton (50+ services):**
- GameState
- All Screen Renderers
- IInputHandler, IThemeService
- Stateless helpers (DiceService, StatCalculationService)

**Scoped (30+ services):**
- All Repositories (EF Core DbContext)
- Services that access database (CraftingService, JournalService)
- Command execution services

**Transient:**
- Not currently used

---

### GameState (Singleton)

```csharp
public class GameState
{
    public GamePhase Phase { get; set; } = GamePhase.MainMenu;
    public Character? CurrentCharacter { get; set; }
    public Guid? CurrentRoomId { get; set; }
    public int TurnCount { get; set; }
    public HashSet<Guid> VisitedRoomIds { get; set; } = new();

    // Combat-specific state
    public CombatState? ActiveCombat { get; set; }
}
```

---

## Testing

### Test Coverage

**ViewModel Tests:**
- `CraftingViewModelTests.cs` (288 lines)
- `JournalViewModelTests.cs` (430 lines)
- `InventoryServiceViewModelTests.cs` (inferred)

**Test Patterns:**
```csharp
public class CraftingViewModelTests
{
    [Fact]
    public void BuildViewModel_FiltersRecipesByTrade()
    {
        // Arrange
        var service = new CraftingService(...);

        // Act
        var vm = service.BuildViewModel(_testCharacter, CraftingTrade.Bodging, 0);

        // Assert
        vm.Recipes.Should().AllSatisfy(r => r.Trade.Should().Be(CraftingTrade.Bodging));
    }
}
```

---

## Design Rationale

### Why Immutable Records Over ReactiveUI?

**Decision:** Use C# 9+ records instead of ReactiveUI's `ReactiveObject`.

**Rationale:**
- **Simplicity:** No MVVM framework learning curve
- **Performance:** No observable overhead for terminal rendering
- **Testability:** Pure data structures, no mocking observables
- **Thread Safety:** Immutability eliminates race conditions
- **Terminal Fit:** Immediate mode rendering doesn't need reactivity

**Trade-off:** No automatic UI updates (must rebuild entire ViewModel per action).

---

### Why Service-Based Builders?

**Decision:** Services expose `BuildViewModel()` methods instead of ViewModels self-populating.

**Rationale:**
- **Separation of Concerns:** ViewModels are dumb DTOs, services contain logic
- **Dependency Injection:** Services can inject repositories, ViewModels cannot
- **Testing:** Mock services, not ViewModels
- **Reusability:** Same ViewModel can be built by multiple services

**Alternative Considered:** ViewModels with constructor DI. Rejected because ViewModels should be data-only.

---

### Why No Navigation Service?

**Decision:** GameService.RunAsync() directly manages phase transitions instead of dedicated NavigationService.

**Rationale:**
- **Single Responsibility:** Only one place changes GameState.Phase
- **Simplicity:** No navigation stack, no history tracking
- **Linear Flow:** Most transitions are command-driven (attack → Combat, quit → Quit)
- **Terminal Constraints:** No back button, no arbitrary navigation

**Future Enhancement:** Could extract navigation logic if GUI version adds multi-window support.

---

## Changelog

### v1.1.0 (2025-12-25) - Documentation Accuracy

**Fixed:**
- Corrected GamePhase enum: 6 values → 4 values (removed CharacterCreation, Rest which don't exist)
- Updated CombatViewModel structure: Added RoundNumber, ActiveCombatantName, PlayerStatsView, row system
- Updated ExplorationViewModel: 13 properties → 19 properties (added stress/corruption pairs, TurnCount, minimap data, BiomeColor)
- Corrected BuildViewModel signatures: GetViewModel() (no GameState param), async patterns
- Updated phase transition rules to reflect actual implementation

**Added:**
- Code traceability remarks to 6 implementation files
- PlayerStatsView nested record documentation
- Coordinate, Room, and minimap-related property documentation

### v1.0.0 (Initial - Implemented)

**Implemented:**
- 7 ViewModel records (Combat, Exploration, Inventory, Journal, Crafting, Options, TimelineEntry)
- GameService game loop with phase dispatch
- DI container with 79 service registrations
- Service-based BuildViewModel pattern
- Nullable renderer support (headless testing)
- Async ViewModel building (Journal, Exploration)
- Scoped service management for database access

**Design Decisions:**
- Immutable C# records over ReactiveUI
- Service builders over self-populating ViewModels
- Phase enum dispatch over NavigationService
- Singleton renderers (stateless)
- Scoped services for database access

---

## Future Enhancements

### Avalonia GUI Migration

**Concept:** Add Avalonia 11.0 with ReactiveUI ViewModels alongside terminal UI.

**Implementation:**
```csharp
// Convert CombatViewModel record to ReactiveObject class
public class CombatViewModel : ViewModelBase
{
    private int _currentHp;
    public int CurrentHp
    {
        get => _currentHp;
        set => this.RaiseAndSetIfChanged(ref _currentHp, value);
    }

    // CombatViewModel.xaml data binds to properties
}
```

**Benefit:** GUI version with reactive bindings, terminal version remains simple.

---

### ViewModel Caching

**Concept:** Cache ViewModels when GameState unchanged (e.g., repeated "look" commands).

**Implementation:**
```csharp
private ExplorationViewModel? _cachedExplorationVm;
private int _lastRoomHash;

public ExplorationViewModel BuildExplorationViewModel()
{
    var currentHash = _gameState.CurrentRoomId.GetHashCode();
    if (_cachedExplorationVm != null && currentHash == _lastRoomHash)
    {
        return _cachedExplorationVm;  // Reuse cached snapshot
    }

    _cachedExplorationVm = new ExplorationViewModel(...);
    _lastRoomHash = currentHash;
    return _cachedExplorationVm;
}
```

**Benefit:** Avoid database queries when state unchanged.

---

### Navigation Service Extraction

**Concept:** Extract phase transition logic from GameService into NavigationService.

**Implementation:**
```csharp
public interface INavigationService
{
    void NavigateToExploration();
    void NavigateToCombat(List<Enemy> enemies);
    void NavigateToRest(bool sanctuary);
    void ReturnFromModal();
}

// GameService.RunAsync() calls NavigationService instead of setting Phase directly
```

**Benefit:** Cleaner separation, enables navigation history/breadcrumbs.

---

## AAM-VOICE Compliance

This specification describes mechanical systems and is exempt from Domain 4 constraints. In-game UI text rendered in ViewModels must follow AAM-VOICE guidelines:

**Compliant Example:**
```csharp
// RoomDescription in ExplorationViewModel
"The walls writhe with corrupted runes. Data streams bleed into the air like dying stars."
```

**Non-Compliant Example:**
```csharp
// UI Debug Text
"ViewModel rebuilt: CombatViewModel@0x4A3F2. Render latency: 12ms."  // Layer 4 technical bleed
```
