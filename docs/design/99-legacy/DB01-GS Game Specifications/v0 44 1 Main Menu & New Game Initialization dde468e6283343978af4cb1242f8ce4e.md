# v0.44.1: Main Menu & New Game Initialization

Type: Technical
Description: MainMenuController implementation with New Game, Continue, Load Game functionality. Game state initialization and session tracking.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.3, v0.3 (Persistence)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.44: Game Flow Integration & Controllers (v0%2044%20Game%20Flow%20Integration%20&%20Controllers%200f28bd7b1ab1400fb9cc0377e89bb095.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.3, v0.3 (Persistence)

**Estimated Time:** 7-9 hours

**Phase:** Initialization

**Deliverable:** Functional main menu with New Game, Continue, and Load Game

---

## Executive Summary

v0.44.1 implements the MainMenuController that makes the main menu actually functional. This is the entry point to the game, handling New Game initialization, Continue Game loading, and the transition to **Survivor initialization** (character creation).

**Aethelgard Voice Integration:**

- Uses "Survivor" terminology in logs and comments
- References "Saga" for progression system
- Uses "Sector" for dungeon/exploration areas
- Maintains Layer 2 (diagnostic/clinical) voice in controller logging

**What This Delivers:**

- MainMenuController implementation
- New Game button functionality
- Continue Game loading most recent save
- Load Game browser integration
- Game state initialization
- Session tracking setup
- Integration with v0.43.3 navigation

**The Problem:**

v0.43.3 created a MainMenuViewModel with commands, but those commands don't do anything. Clicking New Game should start character creation, but there's no controller to handle it.

**Success Metric:** Clicking New Game navigates to Survivor initialization (character creation) with initialized game state, ready to create a Survivor with Saga System progression fields.

---

## Database Schema Changes

No new tables (uses GameSessions from v0.44 parent spec).

---

## Service Implementation

### MainMenuController

```csharp
using Microsoft.Extensions.DependencyInjection;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.ViewModels;
using [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

public class MainMenuController
{
    private readonly ILogger _logger;
    private readonly INavigationService _navigationService;
    private readonly ISaveGameService _saveGameService;
    private readonly IServiceProvider _serviceProvider;
    private GameStateController _gameStateController;
    
    public MainMenuController(
        ILogger logger,
        INavigationService navigationService,
        ISaveGameService saveGameService,
        IServiceProvider serviceProvider,
        GameStateController gameStateController)
    {
        _logger = logger;
        _navigationService = navigationService;
        _saveGameService = saveGameService;
        _serviceProvider = serviceProvider;
        _gameStateController = gameStateController;
    }
    
    public async Task OnNewGameAsync()
    {
        _logger.Information("New Game started");
        
        try
        {
            // Initialize new game state
            var gameState = new GameState
            {
                SessionId = Guid.NewGuid(),
                SessionStarted = DateTime.UtcNow,
                CurrentPhase = GamePhase.CharacterCreation,
                PlayTime = [TimeSpan.Zero](http://TimeSpan.Zero),
                RunNumber = await GetNextRunNumberAsync()
            };
            
            // Register with GameStateController
            _gameStateController.InitializeNewGame(gameState);
            
            // Navigate to character creation
            var characterCreationController = _serviceProvider.GetRequiredService<CharacterCreationController>();
            await _navigationService.NavigateTo<CharacterCreationViewModel>();
            
            _logger.Information("Navigated to character creation, SessionId: {SessionId}", gameState.SessionId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error starting new game");
            throw;
        }
    }
    
    public async Task OnContinueGameAsync()
    {
        _logger.Information("Continue Game selected");
        
        try
        {
            // Load most recent save
            var mostRecentSave = await _saveGameService.GetMostRecentSaveAsync();
            
            if (mostRecentSave == null)
            {
                _logger.Warning("No save games found, starting new game instead");
                await OnNewGameAsync();
                return;
            }
            
            // Load game state
            var gameState = await _saveGameService.LoadGameAsync(mostRecentSave.SaveId);
            
            // Register with GameStateController
            _gameStateController.LoadGame(gameState);
            
            // Navigate to appropriate view based on phase
            await NavigateToCurrentPhaseAsync(gameState);
            
            _logger.Information("Loaded save {SaveId}, Phase: {Phase}", mostRecentSave.SaveId, gameState.CurrentPhase);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading most recent save");
            throw;
        }
    }
    
    public async Task OnLoadGameAsync(int saveId)
    {
        _logger.Information("Loading save {SaveId}", saveId);
        
        try
        {
            // Load specified save
            var gameState = await _saveGameService.LoadGameAsync(saveId);
            
            // Register with GameStateController
            _gameStateController.LoadGame(gameState);
            
            // Navigate to appropriate view
            await NavigateToCurrentPhaseAsync(gameState);
            
            _logger.Information("Loaded save {SaveId} successfully", saveId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading save {SaveId}", saveId);
            throw;
        }
    }
    
    public async Task OnSettingsAsync()
    {
        _logger.Information("Opening settings");
        await _navigationService.NavigateTo<SettingsViewModel>();
    }
    
    public async Task OnAchievementsAsync()
    {
        _logger.Information("Opening achievements");
        await _navigationService.NavigateTo<MetaProgressionViewModel>();
    }
    
    public async Task OnQuitAsync()
    {
        _logger.Information("Quit selected");
        
        // Ensure any active game is saved
        if (_gameStateController.HasActiveGame)
        {
            await _gameStateController.AutoSaveAsync();
        }
        
        // Close application (Avalonia-specific)
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
    
    private async Task<int> GetNextRunNumberAsync()
    {
        // Query database for highest run number
        var stats = await _saveGameService.GetPlayerStatisticsAsync();
        return stats.TotalRuns + 1;
    }
    
    private async Task NavigateToCurrentPhaseAsync(GameState gameState)
    {
        switch (gameState.CurrentPhase)
        {
            case GamePhase.CharacterCreation:
                await _navigationService.NavigateTo<CharacterCreationViewModel>();
                break;
            
            case GamePhase.DungeonExploration:
                await _navigationService.NavigateTo<DungeonExplorationViewModel>();
                break;
            
            case GamePhase.Combat:
                await _navigationService.NavigateTo<CombatViewModel>();
                break;
            
            case GamePhase.CharacterProgression:
                await _navigationService.NavigateTo<CharacterSheetViewModel>();
                break;
            
            case GamePhase.EndgameMenu:
                await _navigationService.NavigateTo<EndgameModeViewModel>();
                break;
            
            default:
                _logger.Warning("Unknown phase {Phase}, returning to main menu", gameState.CurrentPhase);
                await _navigationService.NavigateTo<MainMenuViewModel>();
                break;
        }
    }
}
```

### GameStateController (Core State Manager)

```csharp
using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

public class GameStateController
{
    private readonly ILogger _logger;
    private readonly ISaveGameService _saveGameService;
    private GameState? _currentGameState;
    private System.Timers.Timer? _autoSaveTimer;
    
    public GameState CurrentGameState
    {
        get => _currentGameState ?? throw new InvalidOperationException("No active game");
    }
    
    public bool HasActiveGame => _currentGameState != null;
    
    public event EventHandler<GameState>? GameStateChanged;
    public event EventHandler<GamePhase>? PhaseChanged;
    
    public GameStateController(ILogger logger, ISaveGameService saveGameService)
    {
        _logger = logger;
        _saveGameService = saveGameService;
    }
    
    public void InitializeNewGame(GameState gameState)
    {
        _logger.Information("Initializing new game, SessionId: {SessionId}", gameState.SessionId);
        
        gameState.Validate();
        _currentGameState = gameState;
        
        StartAutoSaveTimer();
        
        GameStateChanged?.Invoke(this, gameState);
        PhaseChanged?.Invoke(this, gameState.CurrentPhase);
    }
    
    public void LoadGame(GameState gameState)
    {
        _logger.Information("Loading game, SessionId: {SessionId}", gameState.SessionId);
        
        gameState.Validate();
        _currentGameState = gameState;
        
        StartAutoSaveTimer();
        
        GameStateChanged?.Invoke(this, gameState);
        PhaseChanged?.Invoke(this, gameState.CurrentPhase);
    }
    
    public async Task UpdatePhaseAsync(GamePhase newPhase, string reason = "")
    {
        if (_currentGameState == null)
            throw new InvalidOperationException("Cannot update phase without active game");
        
        var oldPhase = _currentGameState.CurrentPhase;
        _currentGameState.PreviousPhase = oldPhase;
        _currentGameState.CurrentPhase = newPhase;
        
        _logger.Information("Phase transition: {OldPhase} → {NewPhase}. Reason: {Reason}",
            oldPhase, newPhase, reason);
        
        // Log to database for debugging
        await LogPhaseTransitionAsync(oldPhase, newPhase, reason);
        
        PhaseChanged?.Invoke(this, newPhase);
        GameStateChanged?.Invoke(this, _currentGameState);
    }
    
    public async Task AutoSaveAsync()
    {
        if (_currentGameState == null) return;
        
        try
        {
            await _saveGameService.SaveGameAsync(_currentGameState, isAutoSave: true);
            _logger.Debug("Auto-save completed");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Auto-save failed");
        }
    }
    
    public void Reset()
    {
        _logger.Information("Resetting game state");
        
        StopAutoSaveTimer();
        _currentGameState?.Reset();
        _currentGameState = null;
    }
    
    private void StartAutoSaveTimer()
    {
        StopAutoSaveTimer();
        
        _autoSaveTimer = new System.Timers.Timer(60000); // 60 seconds
        _autoSaveTimer.Elapsed += async (s, e) => await AutoSaveAsync();
        _autoSaveTimer.Start();
        
        _logger.Debug("Auto-save timer started");
    }
    
    private void StopAutoSaveTimer()
    {
        if (_autoSaveTimer != null)
        {
            _autoSaveTimer.Stop();
            _autoSaveTimer.Dispose();
            _autoSaveTimer = null;
            _logger.Debug("Auto-save timer stopped");
        }
    }
    
    private async Task LogPhaseTransitionAsync(GamePhase from, GamePhase to, string reason)
    {
        // Write to GameStateTransitions table
        // Implementation depends on repository pattern
    }
}
```

---

## Integration Points

**With v0.43.3 (Navigation):**

- Uses NavigationService to switch views
- MainMenuViewModel commands wire to controller methods

**With v0.3 (Persistence):**

- Loads saves via SaveGameService
- Initializes GameState from save data

**With v0.44.2 (Character Creation):**

- Transitions to character creation on New Game
- Passes initialized GameState

---

## Functional Requirements

### FR1: New Game Initialization

**Requirement:** New Game button creates empty game state and navigates to character creation.

**Test:**

```csharp
[Fact]
public async Task NewGame_InitializesGameStateAndNavigates()
{
    var controller = CreateMainMenuController();
    
    await controller.OnNewGameAsync();
    
    Assert.True(_gameStateController.HasActiveGame);
    Assert.Equal(GamePhase.CharacterCreation, _gameStateController.CurrentGameState.CurrentPhase);
    Assert.Equal(1, _gameStateController.CurrentGameState.RunNumber);
}
```

### FR2: Continue Game Loading

**Requirement:** Continue Game loads most recent save.

**Test:**

```csharp
[Fact]
public async Task ContinueGame_LoadsMostRecentSave()
{
    // Setup: Create a save
    var save = await CreateTestSaveAsync();
    
    var controller = CreateMainMenuController();
    await controller.OnContinueGameAsync();
    
    Assert.True(_gameStateController.HasActiveGame);
    Assert.Equal(save.SessionId, _gameStateController.CurrentGameState.SessionId);
}

[Fact]
public async Task ContinueGame_WithNoSaves_StartsNewGame()
{
    var controller = CreateMainMenuController();
    await controller.OnContinueGameAsync();
    
    // Should create new game instead
    Assert.True(_gameStateController.HasActiveGame);
    Assert.Equal(GamePhase.CharacterCreation, _gameStateController.CurrentGameState.CurrentPhase);
}
```

### FR3: Load Specific Save

**Requirement:** Can load any save from the list.

**Test:**

```csharp
[Fact]
public async Task LoadGame_LoadsSpecifiedSave()
{
    var save1 = await CreateTestSaveAsync();
    var save2 = await CreateTestSaveAsync();
    
    var controller = CreateMainMenuController();
    await controller.OnLoadGameAsync(save1.SaveId);
    
    Assert.Equal(save1.SessionId, _gameStateController.CurrentGameState.SessionId);
}
```

### FR4: Phase-Based Navigation

**Requirement:** Loading a save navigates to correct view based on phase.

**Test:**

```csharp
[Fact]
public async Task LoadGame_InCombat_NavigatesToCombatView()
{
    var save = await CreateTestSaveAsync(phase: GamePhase.Combat);
    
    var controller = CreateMainMenuController();
    await controller.OnLoadGameAsync(save.SaveId);
    
    // Verify navigation to CombatViewModel
    Assert.IsType<CombatViewModel>(_navigationService.CurrentView);
}
```

---

## Testing Requirements

### Unit Tests (Target: 80%+ coverage)

**MainMenuController Tests:**

- New game initialization
- Continue game loading
- Load specific save
- Phase-based navigation
- Settings/achievements navigation
- Quit with auto-save

**GameStateController Tests:**

- Game state initialization
- Phase transitions
- State validation
- Auto-save functionality
- State reset

### Integration Tests

**End-to-End:**

```csharp
[Fact]
public async Task FullFlow_NewGameToCharacterCreation()
{
    // Click New Game
    await _mainMenuController.OnNewGameAsync();
    
    // Verify at character creation
    Assert.IsType<CharacterCreationViewModel>(_navigationService.CurrentView);
    Assert.Equal(GamePhase.CharacterCreation, _gameStateController.CurrentGameState.CurrentPhase);
}
```

---

## Success Criteria

**v0.44.1 is DONE when:**

### ✅ Main Menu Functions

- [ ]  New Game button starts character creation
- [ ]  Continue Game loads most recent save
- [ ]  Load Game opens save browser
- [ ]  Settings opens settings menu
- [ ]  Achievements opens meta-progression
- [ ]  Quit saves and exits cleanly

### ✅ Game State Management

- [ ]  GameStateController tracks current game
- [ ]  Phase transitions logged
- [ ]  Auto-save runs every 60 seconds
- [ ]  State validation prevents corruption

### ✅ Save/Load

- [ ]  Can load saves
- [ ]  Navigates to correct view for phase
- [ ]  No save games handled gracefully

### ✅ Testing

- [ ]  Unit tests written (80%+ coverage)
- [ ]  All tests pass
- [ ]  Manual testing complete

---

**Main menu functional. Ready for character creation in v0.44.2.**