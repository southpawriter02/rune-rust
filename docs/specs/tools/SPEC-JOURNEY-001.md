# SPEC-JOURNEY-001: E2E Journey Testing Framework

```yaml
id: SPEC-JOURNEY-001
title: E2E Journey Testing Framework
version: 1.0.1
status: Implemented
last_updated: 2025-12-25
related_specs:
  - SPEC-DICE-001 (deterministic seeding)
  - SPEC-SAVE-001 (persistence testing)
  - SPEC-GAME-001 (orchestration testing)
```

---

## 1. Overview

The E2E Journey Testing Framework provides infrastructure for running automated integration tests that simulate complete gameplay sequences. Using a ScriptedInputHandler for deterministic command injection and TestGameHost for isolated DI containers, tests can verify entire user journeys from character creation through combat, exploration, and persistence.

**Primary Components:**
- **ScriptedInputHandler** - Queue-based input simulation with output capture
- **TestGameHost** - Self-contained DI container with in-memory database
- **Deterministic Seeding** - Reproducible dice rolls via seeded Random

**Design Philosophy:**
The framework follows the "Survivor's Cycle" pattern: Session A creates state, saves, and terminates; Session B loads from the same database and verifies state preservation. This simulates real application restarts without requiring actual process boundaries.

---

## 2. Core Components

### 2.1 ScriptedInputHandler

A test implementation of `IInputHandler` that provides scripted commands from a queue.

**Location:** `RuneAndRust.Tests/Infrastructure/ScriptedInputHandler.cs`

```csharp
public class ScriptedInputHandler : IInputHandler
{
    private readonly Queue<string> _commandQueue;

    // Capture buffers for assertions
    public List<string> OutputBuffer { get; }
    public List<string> ErrorBuffer { get; }
    public List<string> PromptBuffer { get; }

    // Status properties
    public int RemainingCommands => _commandQueue.Count;
    public bool IsScriptExhausted => _commandQueue.Count == 0;

    // IInputHandler implementation
    public string GetInput(string prompt);      // Dequeues next command
    public void DisplayMessage(string message); // Captures to OutputBuffer
    public void DisplayError(string message);   // Captures to ErrorBuffer
}
```

**Key Behaviors:**
| Behavior | Description |
|----------|-------------|
| FIFO Command Queue | Commands execute in order of addition |
| Script Exhaustion | Returns `"quit"` when queue is empty |
| Output Capture | All display calls stored for assertion |
| Prompt Tracking | Records all prompts shown to user |

### 2.2 TestGameHost

A self-contained test host providing isolated DI container and in-memory database.

**Location:** `RuneAndRust.Tests/Infrastructure/TestGameHost.cs`

```csharp
public class TestGameHost : IDisposable
{
    // Factory method
    public static TestGameHost Create(
        int? seed,
        IEnumerable<string> script,
        string? databaseName = null);

    // Properties
    public IServiceProvider Services { get; }
    public GameState GameState { get; }
    public ScriptedInputHandler InputHandler { get; }
    public int? Seed { get; }
    public string DatabaseName { get; }

    // Setup methods
    public Task SetupExplorationAsync(string characterName = "TestRunner");
    public Task SetupCombatAsync(string characterName = "TestWarrior", int enemyHp = 15);

    // Persistence methods
    public Task<bool> SaveGameAsync(int slot);
    public Task<bool> LoadGameAsync(int slot);
}
```

**DI Registrations:**
The TestGameHost registers 40+ services mirroring production:
- All repositories (in-memory EF Core)
- All engine services (Combat, Navigation, Crafting, etc.)
- Seeded DiceService (deterministic rolls)
- ScriptedInputHandler (instead of ConsoleInputHandler)
- No TUI renderers (headless execution)

### 2.3 Deterministic Seeding

**Spec Reference:** SPEC-DICE-001

```csharp
// DiceService constructor supports optional seed
public DiceService(ILogger<DiceService> logger, int? seed = null)
{
    _random = seed.HasValue ? new Random(seed.Value) : new Random();
}

// TestGameHost passes seed during registration
services.AddSingleton<IDiceService>(sp =>
    new DiceService(sp.GetRequiredService<ILogger<DiceService>>(), seed));
```

**Determinism Guarantee:** Same seed produces identical dice sequences across test runs.

---

## 3. Testing Patterns

### 3.1 Basic Script Pattern

```csharp
[Fact]
public async Task Journey_BasicNavigation()
{
    // Arrange: Create host with command script
    using var host = TestGameHost.Create(
        seed: 42,
        new[] { "north", "look", "south", "quit" }
    );
    await host.SetupExplorationAsync("TestRunner");

    // Act: Run until script exhausted
    await host.RunAsync();

    // Assert: Check output buffer
    host.InputHandler.OutputContains("Northern Corridor").Should().BeTrue();
}
```

### 3.2 Survivor's Cycle Pattern (Persistence)

```csharp
[Fact]
public async Task Journey_SurvivorsCycle()
{
    var sharedDb = $"TestDb_{Guid.NewGuid()}";
    Guid expectedRoomId;

    // --- SESSION A: The Life ---
    using (var hostA = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDb))
    {
        await hostA.SetupExplorationAsync("Survivor");
        expectedRoomId = hostA.GameState.CurrentRoomId!.Value;
        await hostA.SaveGameAsync(slot: 1);
    }
    // hostA disposed = simulated app shutdown

    // --- SESSION B: The Afterlife ---
    using (var hostB = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDb))
    {
        await hostB.LoadGameAsync(slot: 1);

        // Assert: State survived the "reboot"
        hostB.GameState.CurrentRoomId.Should().Be(expectedRoomId);
    }
}
```

### 3.3 Combat Journey Pattern

```csharp
[Fact]
public async Task Journey_CombatVictory()
{
    using var host = TestGameHost.Create(
        seed: 42,
        new[] { "attack", "attack", "attack", "quit" }
    );
    await host.SetupCombatAsync("Warrior", enemyHp: 15);

    await host.RunAsync();

    // Assert: Combat ended in victory
    host.GameState.Phase.Should().Be(GamePhase.Exploration);
    host.InputHandler.OutputContains("defeated").Should().BeTrue();
}
```

### 3.4 Deterministic Replay Pattern

```csharp
[Fact]
public async Task Journey_DeterministicReplay()
{
    var script = new[] { "attack", "attack", "quit" };

    // Run with same seed twice
    string output1, output2;

    using (var host1 = TestGameHost.Create(seed: 12345, script))
    {
        await host1.SetupCombatAsync();
        await host1.RunAsync();
        output1 = host1.InputHandler.GetFullOutput();
    }

    using (var host2 = TestGameHost.Create(seed: 12345, script))
    {
        await host2.SetupCombatAsync();
        await host2.RunAsync();
        output2 = host2.InputHandler.GetFullOutput();
    }

    // Assert: Identical outputs
    output1.Should().Be(output2);
}
```

---

## 4. Query Methods

### 4.1 Output Assertions

| Method | Description |
|--------|-------------|
| `OutputContains(text)` | Case-insensitive search in OutputBuffer |
| `ErrorContains(text)` | Case-insensitive search in ErrorBuffer |
| `GetFullOutput()` | Joins OutputBuffer with newlines |
| `ClearBuffers()` | Clears all capture buffers |

### 4.2 State Queries

| Property | Type | Description |
|----------|------|-------------|
| `RemainingCommands` | int | Commands left in queue |
| `IsScriptExhausted` | bool | True when queue is empty |
| `PromptBuffer` | List<string> | All prompts shown |

---

## 5. Setup Methods

### 5.1 SetupExplorationAsync

Creates a minimal exploration environment:

1. Creates character via `CharacterFactory.CreateSimple()`
2. Sets `GamePhase.Exploration`
3. Creates two test rooms (start + north)
4. Links rooms bidirectionally
5. Sets `CurrentRoomId` to start room

**Bypasses:** DungeonGenerator, BiomeSeeder (no database dependencies)

### 5.2 SetupCombatAsync

Extends exploration setup with combat:

1. Calls `SetupExplorationAsync()`
2. Creates "Training Dummy" enemy (configurable HP)
3. Calls `CombatService.StartCombat()`

**Enemy Attributes:**
- Sturdiness: 3, Might: 3, Wits: 1, Will: 1, Finesse: 2
- Weapon: d4 damage, +0 accuracy, 0 soak

---

## 6. Constraints & Limitations

### 6.1 No TUI Renderers

TestGameHost **does not register** terminal renderers:
- `CombatRenderer`
- `RestScreenRenderer`
- `ExplorationRenderer`

**Implication:** GameService detects null renderers and uses text-only mode.

### 6.2 In-Memory Database Only

Uses `UseInMemoryDatabase()` for isolation:
- No file I/O during tests
- Each test gets unique database (GUID-based name)
- Shared database requires explicit `databaseName` parameter

### 6.3 No Async Console I/O

ScriptedInputHandler is synchronous:
- `GetInput()` returns immediately from queue
- No support for `ReadKeyAsync()` or similar

### 6.4 Script Exhaustion Safety

When commands run out:
- Logs warning with last prompt
- Returns `"quit"` to end game loop gracefully
- Prevents infinite loops in tests

---

## 7. Test Categories

### 7.1 Exploration Journey Tests

**Location:** `RuneAndRust.Tests/Integration/ExplorationJourneyTests.cs`
**Count:** 9 tests

| Test | Description |
|------|-------------|
| `Journey_NewGame_To_FirstRoom_DisplaysWelcome` | Initial game start shows welcome |
| `Journey_Look_Command_DisplaysRoomInfo` | Look command output |
| `Journey_Navigation_MovesCharacter` | Basic north/south movement |
| `Journey_InvalidCommand_ShowsError` | Error handling for invalid commands |
| `Journey_Help_Command_DisplaysOptions` | Help command lists options |
| `Journey_ScriptExhaustion_ReturnsQuit` | Queue empty returns "quit" |
| `Journey_DeterministicSeed_ProducesSameResults` | Seed reproducibility |
| `Journey_DifferentSeeds_ProduceDifferentRoomIds` | Different seeds = different results |
| `Journey_ExtendedExploration_CompletesSuccessfully` | Multi-command exploration sequence |

### 7.2 Combat Journey Tests

**Location:** `RuneAndRust.Tests/Integration/CombatJourneyTests.cs`
**Count:** 12 tests

| Test | Description |
|------|-------------|
| `Combat_Victory_DefeatsEnemy_ReturnsToExploration` | Basic attack loop to victory |
| `Combat_Flee_EscapesCombat_ReturnsToExploration` | Flee action exits combat |
| `Combat_AttackCommand_DamagesEnemy` | Attack deals damage |
| `Combat_DeterministicSeed_ProducesSameOutcome` | Seed reproducibility |
| `Combat_EnemyTurn_ExecutesAfterPlayer` | Turn order verification |
| `Combat_Status_DisplaysCombatantInfo` | Status command shows info |
| `Combat_LightAttack_ExecutesWithReducedStamina` | Light attack stamina cost |
| `Combat_HeavyAttack_ExecutesWithIncreasedDamage` | Heavy attack damage bonus |
| `Combat_DifferentSeeds_ProduceDifferentOutcomes` | Different seeds = different results |
| `Combat_FullEncounter_CompletesSuccessfully` | Complete combat sequence |
| `Combat_Setup_SetsCorrectPhase` | Phase verification |
| `Combat_GetCombatService_ReturnsValidService` | DI resolution check |

### 7.3 Persistence Journey Tests

**Location:** `RuneAndRust.Tests/Integration/PersistenceJourneyTests.cs`
**Count:** 12 tests

| Test | Description |
|------|-------------|
| `Journey_SaveLoad_PreservesLocation` | CurrentRoomId persistence |
| `Journey_SaveLoad_PreservesCharacter` | Character data persistence |
| `Journey_SaveLoad_PreservesVisitedRooms` | Fog of war persistence |
| `Journey_SaveLoad_PreservesPhase` | GamePhase persistence |
| `Journey_SaveLoad_PreservesTurnCount` | Turn counter persistence |
| `Journey_SaveLoad_DifferentSlotsAreIsolated` | Slot isolation verification |
| `Journey_SaveLoad_OverwritesExistingSlot` | Slot overwrite behavior |
| `Journey_Load_NonExistentSlot_ReturnsFalse` | Missing slot handling |
| `Journey_DatabaseName_IsExposedForReuse` | DB name accessibility |
| `Journey_SharedDatabase_PersistsAcrossHosts` | Cross-host persistence |
| `Journey_SaveLoad_PreservesInventoryCount` | Inventory item count persistence |
| `Journey_SurvivorsCycle_CompleteStatePreservation` | Full state round-trip |

**Total E2E Tests:** 33

---

## 8. Integration Points

### 8.1 DiceService (SPEC-DICE-001)

- Seeded constructor enables deterministic testing
- Tests can replay exact dice sequences

### 8.2 SaveManager (SPEC-SAVE-001)

- `SaveGameAsync(slot)` and `LoadGameAsync(slot)` wrappers
- Enables Survivor's Cycle pattern

### 8.3 GameService (SPEC-GAME-001)

- `RunAsync()` invokes `GameService.StartAsync()`
- Detects null renderers for headless mode

### 8.4 CommandParser

- Processes scripted commands as if typed by user
- Phase-specific command routing preserved

---

## 9. Decision Trees

### 9.1 Test Host Creation

```
Create TestGameHost
  ├─ seed provided?
  │   ├─ Yes: Register DiceService(seed)
  │   └─ No: Register DiceService() (random)
  │
  ├─ databaseName provided?
  │   ├─ Yes: Use for persistence tests
  │   └─ No: Generate unique GUID name
  │
  └─ script provided
      └─ Create ScriptedInputHandler with commands
```

### 9.2 Script Execution

```
GetInput(prompt) called
  ├─ Commands remaining?
  │   ├─ Yes: Dequeue and return command
  │   └─ No: Log warning, return "quit"
  │
  └─ Prompt recorded to PromptBuffer
```

### 9.3 Survivor's Cycle

```
Session A                    Session B
    │                            │
    ├─ Create(dbName)            │
    ├─ Setup + Play              │
    ├─ SaveGameAsync()           │
    ├─ Dispose()                 │
    │                            ├─ Create(same dbName)
    │                            ├─ LoadGameAsync()
    │                            ├─ Assert state matches
    │                            └─ Dispose()
```

---

## 10. Use Cases

### 10.1 Regression Testing

**Scenario:** Verify combat doesn't break after refactoring AttackResolutionService.

```csharp
[Fact]
public async Task Regression_CombatStillWorks()
{
    using var host = TestGameHost.Create(seed: 1, new[] { "attack", "quit" });
    await host.SetupCombatAsync();
    await host.RunAsync();

    host.InputHandler.OutputContains("damage").Should().BeTrue();
    host.InputHandler.ErrorBuffer.Should().BeEmpty();
}
```

### 10.2 Bug Reproduction

**Scenario:** User reports crash when saving during combat.

```csharp
[Fact]
public async Task BugRepro_SaveDuringCombat_DoesNotCrash()
{
    using var host = TestGameHost.Create(seed: 1, new[] { "attack", "save", "quit" });
    await host.SetupCombatAsync();

    // Act: Should not throw
    await host.RunAsync();

    // Assert: Save was rejected gracefully
    host.InputHandler.OutputContains("Cannot save during combat").Should().BeTrue();
}
```

### 10.3 Feature Verification

**Scenario:** New /heal cheat command implemented.

```csharp
[Fact]
public async Task Feature_HealCheat_RestoresResources()
{
    using var host = TestGameHost.Create(seed: 1, new[] { "~", "/heal", "~", "quit" });
    await host.SetupExplorationAsync();
    host.GameState.CurrentCharacter!.CurrentHP = 1; // Damage character

    await host.RunAsync();

    host.GameState.CurrentCharacter!.CurrentHP.Should().Be(host.GameState.CurrentCharacter.MaxHP);
}
```

---

## 11. Test Scenarios

### 11.1 Scenario: Multi-Room Exploration

```
GIVEN: TestGameHost with exploration setup
  AND: Script ["north", "look", "south", "quit"]
WHEN: RunAsync() completes
THEN: OutputBuffer contains "Northern Corridor"
  AND: Final room is start room
  AND: VisitedRoomIds contains both rooms
```

### 11.2 Scenario: Combat to Victory

```
GIVEN: TestGameHost with combat setup (enemy HP = 15)
  AND: Script ["attack", "attack", "attack", "quit"]
  AND: Seed produces sufficient hits
WHEN: RunAsync() completes
THEN: GamePhase == Exploration (combat ended)
  AND: OutputBuffer contains "defeated" or "victory"
```

### 11.3 Scenario: Persistence Integrity

```
GIVEN: Two TestGameHost instances sharing database name
WHEN: Host A saves game with character at room R
  AND: Host A is disposed
  AND: Host B loads from same slot
THEN: Host B's CurrentRoomId == R
  AND: Host B's CurrentCharacter matches Host A's
```

---

## 12. Error Handling

### 12.1 Script Exhaustion

```csharp
if (_commandQueue.Count == 0)
{
    _logger.LogWarning(
        "Script exhausted at prompt: {Prompt}. Returning 'quit' to end test.",
        prompt);
    return "quit";
}
```

**Behavior:** Graceful termination, not a test failure.

### 12.2 Service Resolution Failure

```csharp
var combatService = _serviceProvider.GetRequiredService<ICombatService>();
```

**Behavior:** Throws `InvalidOperationException` if service not registered.

### 12.3 Null Renderer Handling

GameService detects null renderers and operates in text-only mode:
- Combat output goes to `IInputHandler.DisplayMessage()`
- No terminal escape sequences emitted

---

## 13. Performance Considerations

### 13.1 In-Memory Database Speed

EF Core in-memory provider is significantly faster than SQLite:
- No file I/O
- No transaction logging
- Suitable for high-volume test suites

### 13.2 Test Isolation

Each test creates a unique database:
```csharp
var dbName = databaseName ?? $"TestDb_{Guid.NewGuid()}";
```

**Trade-off:** Slight memory overhead, but guaranteed isolation.

### 13.3 Parallel Test Execution

Tests are designed for parallel execution:
- No shared static state
- Unique database per test
- No file system dependencies

---

## 14. Future Considerations

### 14.1 Async Input Support

Current limitation: Synchronous `GetInput()` only.

**Potential Enhancement:**
```csharp
public async Task<string> GetInputAsync(string prompt, CancellationToken ct);
```

### 14.2 Timing Assertions

Add support for verifying operation duration:
```csharp
host.AssertCompletedWithin(TimeSpan.FromSeconds(5));
```

### 14.3 Visual Regression Testing

Capture renderer output for snapshot comparison:
```csharp
host.CaptureFrame(); // Stores terminal buffer state
host.VerifyAgainstBaseline("expected_combat_screen.txt");
```

### 14.4 Fuzzing Support

Random command generation for chaos testing:
```csharp
var fuzzScript = FuzzGenerator.CreateRandomCommands(seed: 42, count: 100);
```

---

## 15. Related Specifications

| Spec | Relationship |
|------|--------------|
| [SPEC-DICE-001](../core/SPEC-DICE-001.md) | Deterministic seeding for reproducible tests |
| [SPEC-SAVE-001](../data/SPEC-SAVE-001.md) | SaveManager integration for persistence tests |
| [SPEC-GAME-001](../core/SPEC-GAME-001.md) | GameService orchestration under test |
| [SPEC-COMBAT-001](../combat/SPEC-COMBAT-001.md) | Combat scenarios verified by journey tests |
| [SPEC-NAV-001](../exploration/SPEC-NAV-001.md) | Navigation verified by exploration tests |

---

*Generated by The Architect | SPEC-JOURNEY-001 v1.0.0*
