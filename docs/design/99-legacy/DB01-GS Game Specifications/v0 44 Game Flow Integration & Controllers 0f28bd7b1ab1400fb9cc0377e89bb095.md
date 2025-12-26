# v0.44: Game Flow Integration & Controllers

Type: Feature
Description: Implements game flow integration layer creating controllers and workflows that transform v0.43 UI components into a fully playable game. Delivers main menu functionality, complete Survivor initialization workflow, game state initialization with Saga System integration, game loop controllers, death/victory handling, and session management.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43 (Avalonia UI), v0.1-v0.42 (All core systems)
Implementation Difficulty: Very Complex
Balance Validated: No
Proof-of-Concept Flag: No
Sub-item: v0.44.1: Main Menu & New Game Initialization (v0%2044%201%20Main%20Menu%20&%20New%20Game%20Initialization%20dde468e6283343978af4cb1242f8ce4e.md), v0.44.3: Exploration Loop Controller (v0%2044%203%20Exploration%20Loop%20Controller%20083bff50c7ca4bf3a65628f0e16cd65e.md), v0.44.6: Death & Game Over Handling (v0%2044%206%20Death%20&%20Game%20Over%20Handling%20b1202ec0a39d44528f0e8537184a65b7.md), v0.44.5: Progression & Loot Controllers (v0%2044%205%20Progression%20&%20Loot%20Controllers%20bc02ab207b2d43fd86c82ff442dac44b.md), v0.44.4: Combat Loop Controller (v0%2044%204%20Combat%20Loop%20Controller%20c207defae46742d09634de1bf4fd1e7d.md), v0.44.2: Character Creation Workflow (v0%2044%202%20Character%20Creation%20Workflow%20b334f2c82f934a6db87225144ab76726.md), v0.44.7: Victory & Endgame Transition (v0%2044%207%20Victory%20&%20Endgame%20Transition%20031650b85aea4494bf34669b7d9eb029.md)
Template Validated: No
Voice Validated: No

**Status:** Design Phase

**Prerequisites:** v0.43 (Avalonia UI), v0.1-v0.42 (All core systems)

**Timeline:** 40-55 hours (5-7 weeks part-time)

**Goal:** Wire GUI into playable game with complete flow from main menu to victory

**Philosophy:** Bridge presentation to gameplay through robust controllers

---

## I. Executive Summary

v0.44 implements the **game flow integration layer**, creating the controllers and workflows that transform v0.43's beautiful UI components into a fully playable game. This specification focuses on the critical gap: **nothing happens when you click New Game**.

**What v0.44 Delivers:**

- Main menu with functional New Game button
- Complete **Survivor initialization** workflow (Lineage → Background → Attributes → Archetype → Specialization)
- Game state initialization with **Saga System** integration (Legend, PP, Milestones)
- Game loop controllers (Sector navigation → combat with Undying → progression)
- Death and victory handling with trauma economy integration
- Session management and state persistence
- Integration between all UI views (v0.43) and game engine (v0.1-v0.42)
- **Aethelgard voice consistency** across all controller implementations

**The Problem v0.44 Solves:**

v0.43 built comprehensive UI components that can *display* combat, Survivor stats, Sectors, and progression. But clicking "New Game" does nothing because there's no specification for:

- Creating a new **Survivor** (PlayerCharacter with full Saga System initialization)
- Initializing Saga progression (Legend=0, PP=0, Milestone=0)
- Initializing a **Sector** (DungeonState with rooms and encounters)
- Starting the Sector navigation loop
- Managing state transitions between views (MainMenu → CharacterCreation → Exploration → Combat → Death/Victory)
- Handling trauma accumulation (Psychic Stress, Runic Blight Corruption)
- Handling victory/defeat conditions with meta-progression updates

v0.44 is the **missing integration layer** between UI (v0.43) and game systems (v0.1-v0.42).

**Success Metric:**

Can click New Game, create a Survivor, navigate Sectors, engage Undying in combat, earn Legend and spend PP at Milestones, accumulate trauma (Psychic Stress/Corruption), and reach death or victory - a complete playthrough from the GUI using proper Aethelgard terminology throughout.

---

## II. Design Philosophy

### A. Controllers as State Orchestrators

**Principle:** Controllers manage state transitions, not business logic.

**Design Rationale:**

Game logic lives in Engine services (v0.1-v0.42). UI components live in Views/ViewModels (v0.43). Controllers are the **thin orchestration layer** that:

- Responds to user actions from UI
- Calls appropriate Engine services
- Updates ViewModels with results
- Manages state transitions between game phases

**Example: Attack Action Flow**

```jsx
User clicks Attack button (v0.43.5: CombatViewModel)
  ↓
CombatController.ProcessPlayerAttackAsync()
  ↓
Calls CombatEngine.ProcessPlayerAction() (v0.1)
  ↓
Receives AttackResult
  ↓
Updates CombatViewModel state
  ↓
Triggers combat animation (v0.43.8)
  ↓
Checks for combat end condition
  ↓
If combat over: Transition to LootController
```

**Why This Matters:**

- UI remains thin presentation layer
- Engine remains pure game logic
- Controllers handle only state orchestration
- Easy to test each layer independently
- Terminal UI can remain functional (different controllers, same Engine)

### B. Complete Game Loop from Start to Finish

**Principle:** Every player action has a defined flow through the system.

**Game Loop States:**

```jsx
MAIN_MENU
  ↓ (New Game)
CHARACTER_CREATION
  ↓ (Confirm character)
DUNGEON_EXPLORATION
  ↓ (Move/Search/Rest)
DUNGEON_EXPLORATION (or COMBAT_ENCOUNTER)
  ↓ (Enemy encounter)
COMBAT
  ↓ (Victory)
LOOT_COLLECTION
  ↓ (Collect loot)
CHARACTER_PROGRESSION (if level up)
  ↓ (Allocate points)
DUNGEON_EXPLORATION
  ↓ (Reach boss/complete)
VICTORY_SCREEN
  ↓ (Continue to endgame)
ENDGAME_MENU
```

**State Transition Rules:**

- Every state has defined entry/exit conditions
- Invalid transitions blocked at controller level
- State history maintained for back navigation
- Save game captures current state + history

**Example: Cannot Enter Combat Without Dungeon**

```csharp
public async Task StartCombatAsync(CombatEncounter encounter)
{
    if (_gameState.CurrentPhase != GamePhase.DungeonExploration)
    {
        _logger.Error("Cannot start combat from phase {Phase}", _gameState.CurrentPhase);
        throw new InvalidOperationException("Combat can only start during exploration");
    }
    
    _gameState.PreviousPhase = _gameState.CurrentPhase;
    _gameState.CurrentPhase = GamePhase.Combat;
    _gameState.CurrentCombat = encounter;
    
    await _navigationService.NavigateTo<CombatViewModel>();
}
```

### C. Fail-Safe State Management

**Principle:** Game state never becomes corrupted or unrecoverable.

**State Validation:**

- Every state transition validates preconditions
- Invalid states logged as errors
- Automatic fallback to safe state (main menu)
- State snapshots before risky operations

**Example: Death Recovery**

```csharp
public async Task HandlePlayerDeathAsync()
{
    _logger.Information("Player death detected, HP: {HP}", _player.CurrentHP);
    
    // Snapshot state before cleanup
    var deathSnapshot = _gameState.CreateSnapshot();
    
    try
    {
        // Show death screen
        await ShowDeathScreenAsync();
        
        // Record statistics
        await _metaProgressionService.RecordRunEndAsync(deathSnapshot, wasVictory: false);
        
        // Cleanup
        _gameState.Reset();
        
        // Return to menu
        await _navigationService.NavigateTo<MainMenuViewModel>();
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Error handling player death, returning to main menu");
        _gameState.Reset();
        await _navigationService.NavigateTo<MainMenuViewModel>();
    }
}
```

### D. Separation of Game State and UI State

**Principle:** Game state is authoritative, UI state is derived.

**Two Types of State:**

**Game State (Authoritative):**

- PlayerCharacter data
- DungeonState
- CombatState
- Inventory
- Progression
- Lives in Engine/Core

**UI State (Derived):**

- Selected cell on grid
- Highlighted cells
- Open dialogs
- Animation states
- Scroll positions
- Lives in ViewModels

**Synchronization Pattern:**

```csharp
public class GameStateController
{
    // Authoritative game state
    private GameState _gameState;
    
    // UI representation
    private GameStateViewModel _viewModel;
    
    public async Task UpdateGameStateAsync(Action<GameState> updateAction)
    {
        // Update authoritative state
        updateAction(_gameState);
        
        // Validate
        _gameState.Validate();
        
        // Sync to ViewModel
        _viewModel.CurrentHP = _gameState.Player.CurrentHP;
        _viewModel.CurrentStamina = _gameState.Player.CurrentStamina;
        _viewModel.Legend = _gameState.Player.Legend;
        // ... etc
        
        // Auto-save if enabled
        if (_configService.AutoSaveEnabled)
        {
            await _saveService.QuickSaveAsync(_gameState);
        }
    }
}
```

**Why This Matters:**

- Game state always saveable/loadable
- UI can be rebuilt from game state
- No risk of UI bugs corrupting game data
- Terminal and GUI can share same game state

### E. Progressive Disclosure of Complexity

**Principle:** Show simple flows first, reveal complexity as needed.

**Character Creation Example:**

**Simple Path (New Players):**

1. Choose archetype (Warrior/Adept/Mystic/Ranger)
2. Choose specialization (one of 3 per archetype)
3. Auto-allocate starting attributes (recommended build)
4. Start game

**Advanced Path (Experienced Players):**

1. Choose archetype
2. Choose specialization
3. Toggle "Custom Build"
4. Manually allocate 15 attribute points
5. Preview starting abilities
6. Choose alternative start scenario (if unlocked)
7. Start game

**Implementation:**

```csharp
public class CharacterCreationController
{
    public bool UseAdvancedMode { get; set; } = false;
    
    public async Task<PlayerCharacter> CreateCharacterAsync()
    {
        if (UseAdvancedMode)
            return await CreateCharacterAdvancedAsync();
        else
            return await CreateCharacterSimpleAsync();
    }
    
    private async Task<PlayerCharacter> CreateCharacterSimpleAsync()
    {
        // Just archetype + specialization
        var character = new PlayerCharacter();
        character.SetArchetype(_selectedArchetype);
        character.SetSpecialization(_selectedSpecialization);
        
        // Use recommended build
        _characterCreationService.ApplyRecommendedBuild(character);
        
        return character;
    }
}
```

---

## III. System Overview

### Current State Analysis (Pre-v0.44)

**What Works:**

- v0.43 UI components render correctly
- v0.1-v0.42 game systems function correctly
- Can manually instantiate game state in code
- Terminal UI has complete flow (reference implementation)

**What Doesn't Work:**

- Clicking "New Game" button does nothing
- No character creation workflow
- No way to start dungeon exploration from GUI
- Combat can only be triggered manually in code
- No death/victory handling in GUI
- No state persistence between sessions

**The Missing Piece:**

v0.43 created ViewModels that **display** data:

```csharp
public class CombatViewModel : ViewModelBase
{
    public ObservableCollection<CombatantViewModel> PlayerTeam { get; }
    public ObservableCollection<CombatantViewModel> EnemyTeam { get; }
    
    // But no logic to START combat, only display it
}
```

v0.44 creates Controllers that **orchestrate** workflows:

```csharp
public class CombatController
{
    public async Task StartCombatAsync(CombatEncounter encounter)
    {
        // Initialize combat state
        var combatState = await _combatEngine.InitializeCombatAsync(encounter);
        
        // Populate ViewModel
        _combatViewModel.LoadCombatState(combatState);
        
        // Navigate to combat view
        await _navigationService.NavigateTo<CombatViewModel>();
        
        // Start combat loop
        await RunCombatLoopAsync();
    }
}
```

### Scope Definition

**✅ In Scope (v0.44):**

**Game Initialization:**

- Main menu functionality
- New Game workflow
- Character creation controllers
- Game state initialization
- Dungeon generation trigger

**Game Loop Controllers:**

- Exploration loop controller
- Combat encounter controller
- Loot collection controller
- Character progression controller
- Rest/camp controller

**State Management:**

- GameStateController
- Session management
- State validation
- State transitions
- Auto-save integration

**End Conditions:**

- Player death handling
- Victory condition detection
- Game over screen
- Victory screen
- Transition to endgame modes

**Persistence Integration:**

- New game initialization
- Continue game loading
- Quick save/load wiring
- State recovery on crash

**❌ Out of Scope:**

**New Game Features:**

- Any new combat mechanics (v0.1-v0.23)
- New enemies or content (separate specs)
- UI component design (already done in v0.43)
- Balance changes (v0.45-v0.46)

**Performance Optimization:**

- Already handled in v0.43.21
- Engine optimization separate

**Advanced Features:**

- Multiplayer (v2.0+)
- Cloud saves (v2.0+)
- Mod support (v2.0+)

**Why These Limits:**

v0.44 is **integration only**. All game systems exist (v0.1-v0.42), all UI exists (v0.43). This specification wires them together into a playable game loop.

---

## IV. Child Specifications Overview

### Phase 1: Initialization (2 specs, 13-18 hours)

**v0.44.1: Main Menu & New Game Initialization (7-9 hours)**

- MainMenuController implementation
- New Game button wiring
- Continue Game loading
- Game state initialization
- Integration with save system
- **Deliverable:** Clicking New Game starts character creation

**v0.44.2: Character Creation Workflow (6-9 hours)**

- CharacterCreationController
- Archetype selection flow
- Specialization selection
- Attribute allocation (simple & advanced)
- Starting abilities assignment
- Alternative start scenarios (if v0.41 unlocks exist)
- Character validation
- **Deliverable:** Complete character creation wizard that produces valid PlayerCharacter

---

### Phase 2: Core Game Loop (3 specs, 16-22 hours)

**v0.44.3: Exploration Loop Controller (6-8 hours)**

- ExplorationController implementation
- Room navigation flow
- Search action wiring
- Rest action handling
- Random encounter triggers
- Transition to combat controller
- Minimap updates
- **Deliverable:** Can navigate dungeon, search rooms, trigger encounters

**v0.44.4: Combat Loop Controller (6-8 hours)**

- CombatController implementation
- Combat initialization from encounters
- Turn management (player → enemy cycles)
- Action execution coordination
- Combat end detection (victory/defeat/flee)
- Transition to loot or death
- Integration with v0.43.4-v0.43.8 combat UI
- **Deliverable:** Complete combat loop from encounter to loot

**v0.44.5: Progression & Loot Controllers (4-6 hours)**

- LootController for post-combat collection
- ProgressionController for level-up flow
- Attribute point allocation
- Ability unlock/rank-up
- Equipment upgrade decisions
- Integration with v0.43.9-v0.43.11
- **Deliverable:** Post-combat loot and progression workflows

---

### Phase 3: End Conditions (2 specs, 11-15 hours)

**v0.44.6: Death & Game Over Handling (5-7 hours)**

- DeathController implementation
- Death detection across all game states
- Death screen UI
- Run statistics display
- Meta-progression updates on death
- Return to main menu
- Optional: Permadeath vs continue options
- **Deliverable:** Clean death handling with statistics

**v0.44.7: Victory & Endgame Transition (6-8 hours)**

- VictoryController implementation
- Victory condition detection (boss defeated, dungeon completed)
- Victory screen with run summary
- Rewards calculation
- Achievement unlocks
- Transition to endgame mode selection (NG+, Challenge Sectors, etc.)
- Integration with v0.40, v0.41
- **Deliverable:** Victory flow leading to endgame content

---

## V. Technical Architecture

### Controller Layer Diagram

```jsx
┌─────────────────────────────────────────────────────────────┐
│                    UI Layer (v0.43)                          │
│  Views + ViewModels (Display only, no game logic)            │
│  - MainMenuViewModel                                         │
│  - CombatViewModel                                           │
│  - DungeonExplorationViewModel                               │
└─────────────────────────────────────────────────────────────┘
                            ↓ Events (button clicks, etc.)
┌─────────────────────────────────────────────────────────────┐
│                 CONTROLLER LAYER (v0.44) ← NEW!              │
│  Orchestrates state transitions and game flow                │
│  - GameStateController (master state)                        │
│  - MainMenuController                                        │
│  - CharacterCreationController                               │
│  - ExplorationController                                     │
│  - CombatController                                          │
│  - ProgressionController                                     │
│  - DeathController                                           │
│  - VictoryController                                         │
└─────────────────────────────────────────────────────────────┘
                            ↓ Service calls
┌─────────────────────────────────────────────────────────────┐
│                   Engine Layer (v0.1-v0.42)                  │
│  Game logic services (Pure business logic)                   │
│  - CombatEngine                                              │
│  - DungeonGenerator                                          │
│  - CharacterProgressionService                               │
│  - EnemyAIService                                            │
└─────────────────────────────────────────────────────────────┘
                            ↓ Reads/writes
┌─────────────────────────────────────────────────────────────┐
│                  Core Data Layer (v0.1-v0.42)                │
│  - GameState                                                 │
│  - PlayerCharacter                                           │
│  - DungeonState                                              │
│  - CombatState                                               │
└─────────────────────────────────────────────────────────────┘
```

### GameState (Master State Container)

```csharp
using RuneAndRust.Core.Characters;
using RuneAndRust.Core.Dungeon;
using RuneAndRust.Core.Combat;

namespace RuneAndRust.Core;

public class GameState
{
    public Guid SessionId { get; set; }
    public DateTime SessionStarted { get; set; }
    
    // Current game phase
    public GamePhase CurrentPhase { get; set; }
    public GamePhase? PreviousPhase { get; set; }
    
    // Player
    public PlayerCharacter Player { get; set; } = null!;
    
    // Dungeon
    public DungeonState? CurrentDungeon { get; set; }
    public Room? CurrentRoom { get; set; }
    
    // Combat
    public CombatState? CurrentCombat { get; set; }
    
    // Endgame
    public int? CurrentNGPlusTier { get; set; }
    public ChallengeSector? CurrentChallengeSector { get; set; }
    
    // Meta
    public TimeSpan PlayTime { get; set; }
    public int RunNumber { get; set; }
    
    public void Validate()
    {
        if (Player == null)
            throw new InvalidOperationException("GameState must have a Player");
        
        if (CurrentPhase == GamePhase.DungeonExploration && CurrentDungeon == null)
            throw new InvalidOperationException("Cannot be in exploration without dungeon");
        
        if (CurrentPhase == GamePhase.Combat && CurrentCombat == null)
            throw new InvalidOperationException("Cannot be in combat without combat state");
    }
    
    public GameStateSnapshot CreateSnapshot()
    {
        return new GameStateSnapshot
        {
            SessionId = SessionId,
            Timestamp = DateTime.UtcNow,
            Phase = CurrentPhase,
            PlayerSnapshot = Player.Clone(),
            DungeonSnapshot = CurrentDungeon?.Clone(),
            CombatSnapshot = CurrentCombat?.Clone()
        };
    }
    
    public void Reset()
    {
        CurrentPhase = GamePhase.MainMenu;
        PreviousPhase = null;
        Player = null!;
        CurrentDungeon = null;
        CurrentRoom = null;
        CurrentCombat = null;
        CurrentNGPlusTier = null;
        CurrentChallengeSector = null;
    }
}

public enum GamePhase
{
    MainMenu,
    CharacterCreation,
    DungeonExploration,
    Combat,
    LootCollection,
    CharacterProgression,
    Death,
    Victory,
    EndgameMenu
}
```

---

## VI. Database Schema

### Game Session Tracking

```sql
-- Track gameplay sessions
CREATE TABLE GameSessions (
    SessionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,  -- For multi-user support
    CharacterName NVARCHAR(100) NOT NULL,
    Archetype VARCHAR(50) NOT NULL,
    Specialization VARCHAR(100) NOT NULL,
    CurrentPhase VARCHAR(50) NOT NULL,
    CurrentLegend INT NOT NULL,
    CurrentHP INT NOT NULL,
    MaxHP INT NOT NULL,
    CurrentDepth INT NOT NULL,  -- Dungeon depth
    SessionStarted DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastPlayed DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TotalPlayTime INT NOT NULL DEFAULT 0,  -- Seconds
    IsActive BIT NOT NULL DEFAULT 1,
    IsCompleted BIT NOT NULL DEFAULT 0,
    WasVictory BIT,
    DeathCause VARCHAR(200),
    RunNumber INT NOT NULL,
    NGPlusTier INT,
    INDEX IX_Session_User_Active (UserId, IsActive),
    INDEX IX_Session_LastPlayed (LastPlayed DESC)
);

-- Track state transitions for debugging
CREATE TABLE GameStateTransitions (
    TransitionId INT PRIMARY KEY IDENTITY,
    SessionId UNIQUEIDENTIFIER NOT NULL,
    FromPhase VARCHAR(50) NOT NULL,
    ToPhase VARCHAR(50) NOT NULL,
    TransitionReason VARCHAR(200),
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (SessionId) REFERENCES GameSessions(SessionId) ON DELETE CASCADE,
    INDEX IX_Transition_Session (SessionId, Timestamp)
);

-- Track run endings for statistics
CREATE TABLE RunEndings (
    RunId INT PRIMARY KEY IDENTITY,
    SessionId UNIQUEIDENTIFIER NOT NULL,
    CharacterName NVARCHAR(100) NOT NULL,
    Archetype VARCHAR(50) NOT NULL,
    FinalLegend INT NOT NULL,
    MaxDepthReached INT NOT NULL,
    TotalPlayTime INT NOT NULL,  -- Seconds
    WasVictory BIT NOT NULL,
    DeathCause VARCHAR(200),
    EnemiesDefeated INT NOT NULL DEFAULT 0,
    DamageDealt BIGINT NOT NULL DEFAULT 0,
    DamageTaken BIGINT NOT NULL DEFAULT 0,
    ItemsCollected INT NOT NULL DEFAULT 0,
    RoomsExplored INT NOT NULL DEFAULT 0,
    EndedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (SessionId) REFERENCES GameSessions(SessionId),
    INDEX IX_RunEnding_Victory (WasVictory, FinalLegend DESC),
    INDEX IX_RunEnding_Date (EndedAt DESC)
);
```

---

## VII. Integration Points

### With v0.43 (UI Layer)

**MainMenuViewModel Integration:**

```csharp
// v0.43 ViewModel exposes commands
public class MainMenuViewModel : ViewModelBase
{
    public ICommand NewGameCommand { get; }
    public ICommand ContinueGameCommand { get; }
    public ICommand LoadGameCommand { get; }
}

// v0.44 Controller implements command handlers
public class MainMenuController
{
    public async Task OnNewGameAsync()
    {
        // v0.44 implementation
        await _navigationService.NavigateTo<CharacterCreationViewModel>();
    }
}
```

**CombatViewModel Integration:**

```csharp
// v0.43 exposes action commands
public class CombatViewModel : ViewModelBase
{
    public ICommand AttackCommand { get; }
    public ICommand DefendCommand { get; }
    public ICommand EndTurnCommand { get; }
}

// v0.44 implements combat loop
public class CombatController
{
    public async Task OnAttackActionAsync()
    {
        // Coordinate between ViewModel and Engine
        var result = await _combatEngine.ProcessPlayerAction(/*...*/);
        _combatViewModel.UpdateFromResult(result);
        await CheckCombatEndConditionAsync();
    }
}
```

### With v0.1-v0.42 (Engine Layer)

**Controllers call Engine services:**

```csharp
public class ExplorationController
{
    private readonly IDungeonGenerator _dungeonGenerator;
    private readonly IEncounterService _encounterService;
    private readonly IRoomFeatureService _roomFeatureService;
    
    public async Task SearchRoomAsync()
    {
        // Call Engine service from v0.4
        var searchResult = await _roomFeatureService.SearchRoomAsync(_gameState.CurrentRoom);
        
        // Update ViewModel with result
        _explorationViewModel.ShowSearchResult(searchResult);
        
        // Check for encounter
        if (searchResult.TriggeredEncounter)
        {
            await _combatController.StartCombatAsync(searchResult.Encounter);
        }
    }
}
```

---

## VIII. Success Criteria

**v0.44 is DONE when:**

### ✅ New Game Flow

- [ ]  Click "New Game" on main menu
- [ ]  Complete character creation wizard
- [ ]  Character appears in starting room
- [ ]  Can navigate dungeon
- [ ]  No errors in logs

### ✅ Combat Flow

- [ ]  Encounter triggers combat
- [ ]  Can execute all combat actions
- [ ]  Enemy AI takes turns
- [ ]  Combat ends on victory/defeat/flee
- [ ]  Transitions to appropriate next state

### ✅ Progression Flow

- [ ]  Level up triggers progression screen
- [ ]  Can allocate attribute points
- [ ]  Can unlock abilities
- [ ]  Returns to exploration

### ✅ Death Flow

- [ ]  Player death detected
- [ ]  Death screen shows statistics
- [ ]  Meta-progression updated
- [ ]  Returns to main menu
- [ ]  Can start new game

### ✅ Victory Flow

- [ ]  Victory condition detected
- [ ]  Victory screen shows
- [ ]  Achievements unlock
- [ ]  Can access endgame modes

### ✅ State Management

- [ ]  Game state always valid
- [ ]  Can save at any point
- [ ]  Can load saved games
- [ ]  No state corruption
- [ ]  Clean error recovery

---

## IX. Timeline

**Total: 40-55 hours (5-7 weeks part-time @ 8 hours/week)**

**Phase 1: Initialization (Weeks 1-2):** v0.44.1-v0.44.2

**Phase 2: Core Game Loop (Weeks 3-5):** v0.44.3-v0.44.5

**Phase 3: End Conditions (Weeks 6-7):** v0.44.6-v0.44.7

**Milestones:**

- **Milestone 1 (Week 2):** Can create character and start game
- **Milestone 2 (Week 4):** Complete exploration → combat → loot loop
- **Milestone 3 (Week 6):** Death and victory handling complete
- **Milestone 4 (Week 7):** Full playthrough start to finish

---

## X. Risk Assessment

### Technical Risks

- **State synchronization bugs:** Mitigate with comprehensive validation
- **Deadlocks in async flow:** Careful async/await patterns
- **Memory leaks in controllers:** Proper disposal and weak references

### Project Risks

- **Scope creep into UI changes:** Strict adherence to "integration only"
- **Discovering v0.43 gaps:** May require v0.43 patches
- **Engine bugs exposed:** Fix in Engine, not controllers

### Mitigation Strategies

- Work incrementally (one child spec at a time)
- Test each controller in isolation
- Maintain terminal UI as reference
- Log all state transitions for debugging

---

**Ready to wire UI into playable game.**