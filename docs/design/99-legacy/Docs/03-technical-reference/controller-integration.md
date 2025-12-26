# Controller Integration Architecture

> **Version:** v0.44.7+
> **Last Updated:** November 2024
> **Location:** `RuneAndRust.DesktopUI/Controllers/`

## Overview

Rune & Rust uses a controller-based architecture to orchestrate game flow, manage state transitions, and coordinate between the UI layer (ViewModels) and the Engine layer (Services). This document covers the controller integration patterns, game phase management, and inter-controller communication.

---

## Controller Architecture

### Core Controllers

| Controller | Responsibility | Dependencies |
|------------|----------------|--------------|
| `GameStateController` | Master state management, phase transitions | SaveGameService |
| `ExplorationController` | Room navigation, encounter initiation | GameStateController, EncounterService |
| `CombatController` | Combat orchestration, victory/defeat handling | GameStateController, CombatEngine, EnemyAI |
| `LootController` | Post-combat loot collection | GameStateController, LootService |
| `ProgressionController` | Level-up, ability advancement | GameStateController, SagaService |
| `DeathController` | Player death handling, statistics | GameStateController, MetaProgressionService |
| `VictoryController` | Dungeon completion, endgame | GameStateController, EndgameService |
| `CharacterCreationController` | New character workflow | GameStateController |
| `MainMenuController` | Game start/load/settings | SaveGameService, GameStateController |

---

## GameStateController (Master Controller)

### Purpose

The `GameStateController` maintains the authoritative game state and coordinates phase transitions across all other controllers.

**File:** `RuneAndRust.DesktopUI/Controllers/GameStateController.cs`

```csharp
public class GameStateController : IDisposable
{
    private readonly ILogger _logger;
    private readonly ISaveGameService _saveGameService;
    private GameState? _currentGameState;
    private System.Timers.Timer? _autoSaveTimer;

    /// <summary>
    /// Gets the current game state. Throws if no active game.
    /// </summary>
    public GameState CurrentGameState
    {
        get => _currentGameState ?? throw new InvalidOperationException("No active game");
    }

    /// <summary>
    /// Gets whether there is an active game session.
    /// </summary>
    public bool HasActiveGame => _currentGameState != null;

    /// <summary>
    /// Raised when the game state changes.
    /// </summary>
    public event EventHandler<GameState>? GameStateChanged;

    /// <summary>
    /// Raised when the game phase changes.
    /// </summary>
    public event EventHandler<GamePhase>? PhaseChanged;
}
```

### Phase Transition Validation

```csharp
public async Task UpdatePhaseAsync(GamePhase newPhase, string reason = "")
{
    if (_currentGameState == null)
        throw new InvalidOperationException("Cannot update phase without active game");

    var oldPhase = _currentGameState.CurrentPhase;

    // Validate transition
    if (!IsValidTransition(oldPhase, newPhase))
    {
        _logger.Error("Invalid phase transition: {OldPhase} → {NewPhase}", oldPhase, newPhase);
        throw new InvalidOperationException($"Invalid phase transition from {oldPhase} to {newPhase}");
    }

    _currentGameState.PreviousPhase = oldPhase;
    _currentGameState.CurrentPhase = newPhase;

    _logger.Information("Phase transition: {OldPhase} → {NewPhase}. Reason: {Reason}",
        oldPhase, newPhase, string.IsNullOrEmpty(reason) ? "(none)" : reason);

    // Validate new state
    try
    {
        _currentGameState.Validate();
    }
    catch (InvalidOperationException ex)
    {
        _logger.Error(ex, "State validation failed after phase transition");
        // Rollback
        _currentGameState.CurrentPhase = oldPhase;
        _currentGameState.PreviousPhase = null;
        throw;
    }

    PhaseChanged?.Invoke(this, newPhase);
    GameStateChanged?.Invoke(this, _currentGameState);

    // Auto-save on significant transitions
    if (ShouldAutoSaveOnTransition(newPhase))
    {
        await AutoSaveAsync();
    }
}
```

### Game Phase Flow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         GAME PHASE STATE MACHINE                        │
└─────────────────────────────────────────────────────────────────────────┘

    ┌─────────────────┐
    │   MainMenu      │◄───────────────────────────────────────────┐
    └────────┬────────┘                                            │
             │                                                      │
             ▼                                                      │
    ┌─────────────────┐                                            │
    │CharacterCreation│                                            │
    └────────┬────────┘                                            │
             │                                                      │
             ▼                                                      │
    ┌─────────────────┐         ┌─────────────────┐               │
    │  Exploration    │────────►│     Combat      │               │
    │                 │◄────────│                 │               │
    └────────┬────────┘         └────────┬────────┘               │
             │                           │                         │
             │                           ├─────────┐               │
             │                           │         │               │
             │                           ▼         ▼               │
             │                  ┌────────────┐ ┌────────────┐      │
             │                  │   Loot     │ │   Death    │──────┤
             │                  └─────┬──────┘ └────────────┘      │
             │                        │                            │
             │                        ▼                            │
             │                  ┌────────────┐                     │
             │                  │Progression │                     │
             │                  └─────┬──────┘                     │
             │                        │                            │
             │◄───────────────────────┘                            │
             │                                                     │
             ▼                                                     │
    ┌─────────────────┐                                           │
    │    Victory      │────────────────────────────────────────────┘
    │   (Boss Room)   │
    └─────────────────┘
```

---

## Controller Communication Patterns

### 1. Event-Based Communication

Controllers communicate through C# events for loose coupling.

**CombatController → LootController/DeathController**

```csharp
// CombatController.cs
public class CombatController
{
    public event EventHandler<CombatEndedEventArgs>? CombatEnded;
    public event EventHandler? PlayerFled;
    public event EventHandler<LootCollectionEventArgs>? LootReady;

    private void HandleCombatVictory(List<Enemy> defeatedEnemies)
    {
        _logger.Information("Combat victory: {Count} enemies defeated", defeatedEnemies.Count);

        // Raise event for loot handling
        LootReady?.Invoke(this, new LootCollectionEventArgs
        {
            DefeatedEnemies = defeatedEnemies,
            Room = _gameStateController.CurrentGameState.CurrentRoom
        });

        // Raise general combat ended event
        CombatEnded?.Invoke(this, new CombatEndedEventArgs
        {
            PlayerVictorious = true,
            DefeatedEnemies = defeatedEnemies
        });
    }

    private void HandlePlayerDeath(string killingBlow)
    {
        CombatEnded?.Invoke(this, new CombatEndedEventArgs
        {
            PlayerVictorious = false,
            KillingBlow = killingBlow
        });
    }
}
```

**ExplorationController → CombatController**

```csharp
// ExplorationController.cs
public class ExplorationController
{
    public event EventHandler<CombatInitiationEventArgs>? CombatInitiated;
    public event EventHandler<Room>? RoomEntered;
    public event EventHandler<string>? MessageRaised;

    private async Task ExecuteMoveAsync(Room targetRoom)
    {
        // Update game state
        _gameStateController.CurrentGameState.CurrentRoom = targetRoom;

        // Raise room entered event
        RoomEntered?.Invoke(this, targetRoom);

        // Check for encounter
        if (targetRoom.Enemies.Any())
        {
            CombatInitiated?.Invoke(this, new CombatInitiationEventArgs
            {
                Room = targetRoom,
                Enemies = targetRoom.Enemies.ToList()
            });
        }
    }
}
```

### 2. Controller Injection Pattern

Controllers that depend on each other are wired together at startup.

```csharp
// CombatController.cs
public class CombatController
{
    private LootController? _lootController;
    private ProgressionController? _progressionController;
    private DeathController? _deathController;
    private VictoryController? _victoryController;

    /// <summary>
    /// Sets the reward controllers for post-combat workflows.
    /// Should be called during application startup.
    /// </summary>
    public void SetRewardControllers(
        LootController lootController,
        ProgressionController progressionController,
        DeathController? deathController = null,
        VictoryController? victoryController = null)
    {
        _lootController = lootController;
        _progressionController = progressionController;
        _deathController = deathController;
        _victoryController = victoryController;

        _logger.Debug("CombatController reward controllers configured");
    }
}
```

### 3. Shared State via GameStateController

All controllers access shared state through the `GameStateController`.

```csharp
// Pattern used in all controllers
public class ExplorationController
{
    private readonly GameStateController _gameStateController;

    public async Task OnMoveAsync(string direction)
    {
        if (!ValidateExplorationState()) return;

        var gameState = _gameStateController.CurrentGameState;
        var currentRoom = gameState.CurrentRoom!;

        // Use shared state for operations
        _logger.Debug("Survivor moving {Direction} from room {RoomId}",
            direction, currentRoom.RoomId);
    }

    private bool ValidateExplorationState()
    {
        if (!_gameStateController.HasActiveGame)
        {
            _logger.Warning("Cannot explore: no active game");
            return false;
        }

        var gameState = _gameStateController.CurrentGameState;
        if (gameState.CurrentRoom == null)
        {
            _logger.Warning("Cannot explore: no current room");
            return false;
        }

        return true;
    }
}
```

---

## Controller Flow Diagrams

### Complete Combat Flow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                       COMPLETE COMBAT FLOW                              │
└─────────────────────────────────────────────────────────────────────────┘

ExplorationController                CombatController
      │                                    │
      │ CombatInitiated event              │
      ├───────────────────────────────────►│
      │                                    │
      │                            InitializeCombat()
      │                                    │
      │                            ┌───────┴───────┐
      │                            │ Combat Loop   │
      │                            │ (Turn-based)  │
      │                            └───────┬───────┘
      │                                    │
      │                            Victory? ────────────┐
      │                                    │            │
      │                            Defeat? ─────┐      │
      │                                    │    │      │
      │                                    ▼    ▼      ▼
      │                            ┌──────┬──────┬──────┐
      │                            │ Flee │Death │ Loot │
      │                            └──┬───┴───┬──┴───┬──┘
      │                               │       │      │
      │                               │       ▼      ▼
      │                               │  DeathController
      │                               │       │    LootController
      │                               │       │      │
      │                               │       │      ▼
      │                               │       │  ProgressionController
      │                               │       │      │
      │                               ▼       │      │
      │◄──────────────────────────────────────┴──────┘
      │ (Return to exploration or game over)
```

### Progression Flow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     PROGRESSION WORKFLOW                                │
└─────────────────────────────────────────────────────────────────────────┘

LootController                    ProgressionController
     │                                     │
     │ LootCollectionComplete              │
     ├────────────────────────────────────►│
     │                                     │
     │                             CheckLevelUp()
     │                                     │
     │                             ┌───────┼───────┐
     │                             │ Level Up?     │
     │                             └───────┼───────┘
     │                                     │
     │                          Yes ───────┤────── No
     │                             │              │
     │                             ▼              │
     │                     ShowProgressionUI()   │
     │                             │              │
     │                     AttributeIncreased    │
     │                     AbilityAdvanced       │
     │                             │              │
     │                             ▼              │
     │                     ProgressionComplete   │
     │                             │              │
     │◄────────────────────────────┴──────────────┘
     │
     ▼
ExplorationController (Resume exploration)
```

### Victory Flow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      VICTORY WORKFLOW                                   │
└─────────────────────────────────────────────────────────────────────────┘

CombatController                   VictoryController
     │                                     │
     │ (Boss room victory detected)        │
     │ CombatEnded(victory, isBoss=true)   │
     ├────────────────────────────────────►│
     │                                     │
     │                             HandleVictory()
     │                                     │
     │                             CalculateStatistics()
     │                                     │
     │                             StatisticsCalculated
     │                                     │
     │                             UpdateMetaProgression()
     │                                     │
     │                             CheckEndgameContent()
     │                                     │
     │                             ┌───────┼───────┐
     │                             │ NG+ Available?│
     │                             └───────┼───────┘
     │                                     │
     │                          Yes ───────┤────── No
     │                             │              │
     │                             ▼              ▼
     │                     OfferEndgameOptions   ShowCredits
     │                             │              │
     │                     EndgameMenuRequested  │
     │                             │              │
     │◄────────────────────────────┴──────────────┘
     │
     ▼
MainMenuController (Return to menu or NG+)
```

---

## Controller Initialization

### Startup Wiring (Program.cs Pattern)

```csharp
// Typical initialization sequence
public static class GameBootstrap
{
    private static GameStateController _gameStateController;
    private static ExplorationController _explorationController;
    private static CombatController _combatController;
    private static LootController _lootController;
    private static ProgressionController _progressionController;
    private static DeathController _deathController;
    private static VictoryController _victoryController;

    public static void Initialize()
    {
        // 1. Create shared services
        var logger = Log.Logger;
        var saveService = new SaveGameService(...);
        var navigationService = new NavigationService(...);

        // 2. Create master controller
        _gameStateController = new GameStateController(logger, saveService);

        // 3. Create feature controllers
        _explorationController = new ExplorationController(
            logger, _gameStateController, encounterService, roomFeatureService, navigationService);

        _combatController = new CombatController(
            logger, _gameStateController, navigationService, combatEngine, enemyAI, sagaService, lootService);

        _lootController = new LootController(
            logger, _gameStateController, navigationService, sagaService, lootService);

        _progressionController = new ProgressionController(
            logger, _gameStateController, navigationService, sagaService);

        _deathController = new DeathController(
            logger, _gameStateController, navigationService, metaProgressionService);

        _victoryController = new VictoryController(
            logger, _gameStateController, navigationService, metaProgressionService, endgameService, sagaService);

        // 4. Wire up controller dependencies
        _combatController.SetRewardControllers(
            _lootController,
            _progressionController,
            _deathController,
            _victoryController);

        // 5. Subscribe to events
        _explorationController.CombatInitiated += OnCombatInitiated;
        _combatController.CombatEnded += OnCombatEnded;
        _combatController.LootReady += OnLootReady;
        _lootController.LootCollectionComplete += OnLootComplete;
        _deathController.DeathHandlingComplete += OnDeathComplete;
        _victoryController.VictoryHandlingComplete += OnVictoryComplete;

        logger.Information("Game controllers initialized and wired");
    }
}
```

---

## State Validation

### GameState Validation

```csharp
// GameState.cs
public class GameState
{
    public GamePhase CurrentPhase { get; set; }
    public GamePhase? PreviousPhase { get; set; }
    public PlayerCharacter? Player { get; set; }
    public Room? CurrentRoom { get; set; }
    public Dungeon? CurrentDungeon { get; set; }

    public void Validate()
    {
        switch (CurrentPhase)
        {
            case GamePhase.Exploration:
                if (Player == null)
                    throw new InvalidOperationException("Exploration requires Player");
                if (CurrentRoom == null)
                    throw new InvalidOperationException("Exploration requires CurrentRoom");
                if (CurrentDungeon == null)
                    throw new InvalidOperationException("Exploration requires CurrentDungeon");
                break;

            case GamePhase.Combat:
                if (Player == null)
                    throw new InvalidOperationException("Combat requires Player");
                if (CurrentRoom == null)
                    throw new InvalidOperationException("Combat requires CurrentRoom");
                break;

            case GamePhase.CharacterCreation:
                // No requirements for character creation
                break;

            case GamePhase.Progression:
                if (Player == null)
                    throw new InvalidOperationException("Progression requires Player");
                break;
        }
    }
}
```

### Pre-Operation Validation

```csharp
// Pattern used in controllers before operations
private bool ValidateExplorationState()
{
    if (!_gameStateController.HasActiveGame)
    {
        _logger.Warning("Cannot explore: no active game");
        return false;
    }

    var gameState = _gameStateController.CurrentGameState;

    if (gameState.Player == null)
    {
        _logger.Warning("Cannot explore: no player character");
        return false;
    }

    if (gameState.CurrentRoom == null)
    {
        _logger.Warning("Cannot explore: no current room");
        return false;
    }

    if (gameState.CurrentPhase != GamePhase.Exploration)
    {
        _logger.Warning("Cannot explore: not in exploration phase (current: {Phase})",
            gameState.CurrentPhase);
        return false;
    }

    return true;
}
```

---

## Auto-Save Integration

### GameStateController Auto-Save

```csharp
private void StartAutoSaveTimer()
{
    _autoSaveTimer?.Stop();
    _autoSaveTimer = new System.Timers.Timer(AUTO_SAVE_INTERVAL_MS);
    _autoSaveTimer.Elapsed += async (s, e) => await AutoSaveAsync();
    _autoSaveTimer.Start();

    _logger.Debug("Auto-save timer started: {Interval}ms interval", AUTO_SAVE_INTERVAL_MS);
}

private bool ShouldAutoSaveOnTransition(GamePhase newPhase)
{
    return newPhase switch
    {
        GamePhase.Exploration => true,  // Save when returning to exploration
        GamePhase.MainMenu => false,    // Don't auto-save at menu
        GamePhase.Combat => false,      // Don't save during combat
        _ => false
    };
}

private async Task AutoSaveAsync()
{
    if (_currentGameState == null) return;

    try
    {
        await _saveGameService.SaveAsync(_currentGameState, isAutoSave: true);
        _logger.Debug("Auto-save completed for session {SessionId}", _currentGameState.SessionId);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Auto-save failed");
        // Don't throw - auto-save failure shouldn't crash the game
    }
}
```

---

## Controller Testing Pattern

### Mock Dependencies

```csharp
// Unit test setup pattern
[TestFixture]
public class CombatControllerTests
{
    private Mock<ILogger> _loggerMock;
    private Mock<GameStateController> _gameStateControllerMock;
    private Mock<INavigationService> _navigationServiceMock;
    private CombatController _controller;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger>();
        _gameStateControllerMock = new Mock<GameStateController>(...);
        _navigationServiceMock = new Mock<INavigationService>();

        _controller = new CombatController(
            _loggerMock.Object,
            _gameStateControllerMock.Object,
            _navigationServiceMock.Object,
            new CombatEngine(...),
            new EnemyAI(...),
            new SagaService(...),
            new LootService(...));
    }

    [Test]
    public void HandleVictory_RaisesLootReadyEvent()
    {
        // Arrange
        var eventRaised = false;
        _controller.LootReady += (s, e) => eventRaised = true;

        // Act
        _controller.ProcessCombatVictory(new List<Enemy> { ... });

        // Assert
        Assert.IsTrue(eventRaised);
    }
}
```

---

## Related Documentation

- [Event System](event-system.md) - Controller event patterns
- [Error Handling](error-handling.md) - Exception patterns in controllers
- [Service Architecture](service-architecture.md) - Engine service layer
- [Data Flow](data-flow.md) - Data flow between systems
- [System Integration Map](system-integration-map.md) - Engine-level integration
