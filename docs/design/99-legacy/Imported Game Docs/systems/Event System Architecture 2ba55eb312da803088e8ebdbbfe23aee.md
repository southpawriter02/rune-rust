# Event System Architecture

Parent item: Technical Reference (Technical%20Reference%202ba55eb312da8079a291e020980301c1.md)

> Version: v0.44.7+
Last Updated: November 2024
Location: RuneAndRust.Engine/, RuneAndRust.Core/, RuneAndRust.DesktopUI/
> 

## Overview

Rune & Rust implements three distinct event systems, each serving different purposes:

1. **World Events** - Persistent territory events stored in the database
2. **Environmental Events** - Combat environmental interactions tracked per-encounter
3. **Controller Events** - C# events for UI/state communication between components

---

## 1. World Event System

### Purpose

Drives dynamic world changes through territory events that affect faction influence, merchant availability, and sector conditions.

### WorldEventService

**File:** `RuneAndRust.Engine/WorldEventService.cs`

```csharp
/// <summary>
/// v0.35.3: Manages dynamic world events and their consequences
/// Generates, processes, and resolves events that affect territorial control
/// </summary>
public class WorldEventService
{
    private readonly string _connectionString;
    private readonly TerritoryControlService _territoryService;

    // Event spawn probability constants
    private const double EVENT_SPAWN_CHANCE_CONTESTED = 0.05; // 5% per day
    private const double EVENT_SPAWN_CHANCE_STABLE = 0.01;    // 1% per day
    private const double EVENT_SPAWN_CHANCE_WAR = 0.10;       // 10% per day
    private const double EVENT_SPAWN_CHANCE_INDEPENDENT = 0.02; // 2% per day
}

```

### Event Types

| Event Type | Duration | Faction | Consequence |
| --- | --- | --- | --- |
| `Awakening_Ritual` | 7 days | GodSleeperCultists | Elite enemies + 5% influence |
| `Excavation_Discovery` | 5 days | JotunReaders | Rare artifacts + 5% influence |
| `Purge_Campaign` | 10 days | IronBanes | -50% Undying spawns + 5% influence |
| `Incursion` | 3 days | Any dominant | +10% influence |
| `Supply_Raid` | 1 day | Any dominant | -30% merchant stock |
| `Catastrophe` | 2 days | None | +50% hazard density |
| `Scavenger_Caravan` | 2 days | RustClans | -15% prices + 3% influence |

### Event Lifecycle

```
ProcessDailyEventCheck(sectorId)
    │
    ├── CalculateSectorControlState(sectorId)
    │
    ├── Roll for spawn (based on control state)
    │   │
    │   └── SpawnRandomEvent(sectorId, controlState)
    │       ├── FilterFactionSpecificEvent()
    │       ├── GenerateEventTitle()
    │       ├── GenerateEventDescription()
    │       └── INSERT INTO World_Events
    │
    └── Process active events
        │
        └── foreach (event in GetActiveSectorEvents)
            │
            └── ProcessEvent(event)
                │
                ├── Check duration elapsed
                │
                └── ResolveEvent(event)
                    ├── Apply event-specific consequences
                    ├── ShiftInfluence() via TerritoryControlService
                    └── UPDATE World_Events SET is_resolved = 1

```

### WorldEvent Model

**File:** `RuneAndRust.Core/Territory/WorldEvent.cs`

```csharp
public class WorldEvent
{
    public int EventId { get; set; }
    public int WorldId { get; set; }
    public int? SectorId { get; set; }           // Null for world-wide events
    public string EventType { get; set; }         // "Incursion", "Awakening_Ritual", etc.
    public string? AffectedFaction { get; set; }
    public string EventTitle { get; set; }
    public string EventDescription { get; set; }
    public DateTime EventStartDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public int EventDurationDays { get; set; }
    public bool IsResolved { get; set; }
    public bool PlayerInfluenced { get; set; }
    public string? Outcome { get; set; }
    public double InfluenceChange { get; set; }
}

```

### Spawn Probability by Control State

```csharp
double spawnChance = controlState.State switch
{
    "War" => EVENT_SPAWN_CHANCE_WAR,           // 10%
    "Contested" => EVENT_SPAWN_CHANCE_CONTESTED, // 5%
    "Stable" => EVENT_SPAWN_CHANCE_STABLE,       // 1%
    "Independent" => EVENT_SPAWN_CHANCE_INDEPENDENT, // 2%
    _ => EVENT_SPAWN_CHANCE_STABLE
};

if (random.NextDouble() <= spawnChance)
{
    SpawnRandomEvent(sectorId, controlState);
}

```

### Event Resolution Examples

```csharp
// Awakening Ritual - God-Sleepers awaken dormant constructs
private void ResolveAwakeningRitual(WorldEvent evt)
{
    if (evt.SectorId.HasValue && evt.AffectedFaction != null)
    {
        _territoryService.ShiftInfluence(
            evt.SectorId.Value,
            evt.AffectedFaction,
            5.0,  // +5% faction influence
            $"Event: {evt.EventType} completed");
    }
    // Elite Jötun-Forged enemies become available in sector
}

// Incursion - Faction territorial expansion
private void ResolveIncursion(WorldEvent evt)
{
    if (evt.SectorId.HasValue && evt.AffectedFaction != null)
    {
        _territoryService.ShiftInfluence(
            evt.SectorId.Value,
            evt.AffectedFaction,
            10.0,  // +10% faction influence (larger impact)
            $"Event: {evt.EventType} successful");
    }
}

```

---

## 2. Environmental Event System

### Purpose

Tracks combat environmental interactions for analytics, replay, statistics, and narrative generation.

### EnvironmentalEvent Class

**File:** `RuneAndRust.Core/EnvironmentalEvent.cs`

```csharp
public class EnvironmentalEvent
{
    // Identity
    public int EventId { get; set; }
    public int CombatInstanceId { get; set; }
    public int TurnNumber { get; set; }

    // Event details
    public EnvironmentalEventType EventType { get; set; }
    public int? ObjectId { get; set; }        // Environmental object involved
    public int? ActorId { get; set; }         // Character who triggered (null for auto)
    public List<int> Targets { get; set; }    // Affected character IDs

    // Effects
    public int DamageDealt { get; set; }
    public int Kills { get; set; }
    public string? StatusEffectApplied { get; set; }  // "[Burning]", "[Poisoned]"

    // Context
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
}

```

### EnvironmentalEventType Enum

```csharp
public enum EnvironmentalEventType
{
    ObjectDestroyed,      // Environmental object destroyed
    HazardTriggered,      // Static/dynamic hazard dealt damage
    EnvironmentalKill,    // Character killed by environmental damage
    CoverPlaced,          // Cover deployed/established
    CoverDestroyed,       // Cover destroyed (subset of ObjectDestroyed)
    CeilingCollapse,      // Controlled collapse triggered
    PushIntoHazard,       // Character pushed/pulled into hazard
    InteractionTriggered, // Interactive object activated
    AmbientDamage,        // Ambient condition dealt damage
    WeatherEffectApplied, // Weather effect modified combat
    ChainReaction         // Destruction caused cascade effect
}

```

### Log Entry Generation

```csharp
public string GenerateLogEntry()
{
    return EventType switch
    {
        EnvironmentalEventType.ObjectDestroyed =>
            $"[STRUCTURE DESTROYED] {Description ?? "Environmental object destroyed"}",
        EnvironmentalEventType.HazardTriggered =>
            $"💀 {Description ?? "Environmental hazard activated"} → {DamageDealt} damage",
        EnvironmentalEventType.EnvironmentalKill =>
            $"☠️ ENVIRONMENTAL KILL! {Description ?? "Enemy killed by environment"}",
        EnvironmentalEventType.CoverPlaced =>
            $"🛡️ Cover established: {Description}",
        EnvironmentalEventType.CeilingCollapse =>
            $"⚠️ STRUCTURAL FAILURE! {Description ?? "Ceiling collapsed"} → {DamageDealt} damage",
        EnvironmentalEventType.PushIntoHazard =>
            $"⚡ {Description ?? "Enemy pushed into hazard"} → {DamageDealt} damage",
        _ => Description ?? "Environmental event occurred"
    };
}

```

### Result Types

Environmental operations return typed result objects for graceful handling:

### DestructionResult

```csharp
public class DestructionResult
{
    public bool Success { get; set; } = true;
    public bool ObjectDestroyed { get; set; }
    public int ObjectId { get; set; }
    public string ObjectName { get; set; }
    public string? DestructionMethod { get; set; }
    public int DamageDealt { get; set; }
    public int RemainingDurability { get; set; }
    public List<int> SecondaryTargets { get; set; }     // Character IDs hit
    public string? TerrainCreated { get; set; }
    public List<DestructionResult> ChainReactions { get; set; }  // Cascade effects
    public string Message { get; set; }
    public bool SpawnRubble { get; set; }
    public bool CausedSecondaryEffect { get; set; }
}

```

### HazardResult

```csharp
public class HazardResult
{
    public bool WasTriggered { get; set; }
    public int TotalDamage { get; set; }
    public List<int> AffectedCharacters { get; set; }
    public string? StatusEffectApplied { get; set; }
    public string Description { get; set; }
    public string HazardName { get; set; }
}

```

### PushResult

```csharp
public class PushResult
{
    public bool Success { get; set; }
    public string? NewPosition { get; set; }
    public List<int> HazardsEncountered { get; set; }  // Hazard ObjectIds
    public int TotalDamage { get; set; }
    public string LogMessage { get; set; }
}

```

### CollapseResult

```csharp
public class CollapseResult
{
    public bool Success { get; set; }
    public List<int> AffectedCharacters { get; set; }
    public int DamageDealt { get; set; }
    public string? TerrainCreated { get; set; }  // "Rubble", "Difficult Terrain"
    public string LogMessage { get; set; }
}

```

### ComboResult

```csharp
public class ComboResult
{
    public bool ComboDetected { get; set; }
    public string? ComboName { get; set; }
    public int BonusDamage { get; set; }
    public string LogMessage { get; set; }
}

```

### InteractionResult

```csharp
public class InteractionResult
{
    public bool Success { get; set; }
    public int StaminaCost { get; set; }
    public string? EffectDescription { get; set; }
    public int DamageDealt { get; set; }
    public List<int> AffectedCharacters { get; set; }
    public string LogMessage { get; set; }
}

```

---

## 3. Controller Event System

### Purpose

Provides C# event-based communication between UI controllers and ViewModels for state transitions, notifications, and workflow coordination.

### CombatController Events

**File:** `RuneAndRust.DesktopUI/Controllers/CombatController.cs`

```csharp
public class CombatController
{
    /// <summary>
    /// Event raised when combat ends (victory or defeat).
    /// </summary>
    public event EventHandler<CombatEndedEventArgs>? CombatEnded;

    /// <summary>
    /// Event raised when the player flees combat.
    /// </summary>
    public event EventHandler? PlayerFled;

    /// <summary>
    /// Event raised when loot is ready to be collected.
    /// </summary>
    public event EventHandler<LootCollectionEventArgs>? LootReady;
}

```

### ExplorationController Events

**File:** `RuneAndRust.DesktopUI/Controllers/ExplorationController.cs`

```csharp
public class ExplorationController
{
    /// <summary>
    /// Event raised when combat is initiated.
    /// </summary>
    public event EventHandler<CombatInitiationEventArgs>? CombatInitiated;

    /// <summary>
    /// Event raised when a new room is entered.
    /// </summary>
    public event EventHandler<Room>? RoomEntered;

    /// <summary>
    /// Event raised when a message should be displayed.
    /// </summary>
    public event EventHandler<string>? MessageRaised;
}

```

### ProgressionController Events

**File:** `RuneAndRust.DesktopUI/Controllers/ProgressionController.cs`

```csharp
public class ProgressionController
{
    /// <summary>
    /// Event raised when progression workflow completes.
    /// </summary>
    public event EventHandler? ProgressionComplete;

    /// <summary>
    /// Event raised when an attribute is increased.
    /// </summary>
    public event EventHandler<string>? AttributeIncreased;

    /// <summary>
    /// Event raised when an ability is advanced.
    /// </summary>
    public event EventHandler<string>? AbilityAdvanced;
}

```

### DeathController Events

**File:** `RuneAndRust.DesktopUI/Controllers/DeathController.cs`

```csharp
public class DeathController
{
    /// <summary>
    /// Event raised when death handling workflow completes.
    /// </summary>
    public event EventHandler? DeathHandlingComplete;

    /// <summary>
    /// Event raised when run statistics are calculated.
    /// </summary>
    public event EventHandler<RunStatistics>? StatisticsCalculated;
}

```

### VictoryController Events

**File:** `RuneAndRust.DesktopUI/Controllers/VictoryController.cs`

```csharp
public class VictoryController
{
    /// <summary>
    /// Event raised when victory handling workflow completes.
    /// </summary>
    public event EventHandler? VictoryHandlingComplete;

    /// <summary>
    /// Event raised when victory statistics are calculated.
    /// </summary>
    public event EventHandler<VictoryStatistics>? StatisticsCalculated;

    /// <summary>
    /// Event raised when endgame menu is requested.
    /// </summary>
    public event EventHandler? EndgameMenuRequested;
}

```

### LootController Events

**File:** `RuneAndRust.DesktopUI/Controllers/LootController.cs`

```csharp
public class LootController
{
    /// <summary>
    /// Event raised when loot collection workflow completes.
    /// </summary>
    public event EventHandler? LootCollectionComplete;
}

```

### SaveGameService Events

**File:** `RuneAndRust.DesktopUI/Services/SaveGameService.cs`

```csharp
public class SaveGameService
{
    /// <summary>
    /// Event raised when auto-save starts.
    /// </summary>
    public event EventHandler? AutoSaveStarted;

    /// <summary>
    /// Event raised when auto-save completes.
    /// </summary>
    public event EventHandler? AutoSaveCompleted;

    /// <summary>
    /// Event raised when a manual save completes.
    /// </summary>
    public event EventHandler<SaveFileMetadata>? SaveCompleted;
}

```

### Event Subscription Pattern

```csharp
// CombatController.cs - Subscription in Initialize
public void Initialize(CombatViewModel viewModel)
{
    _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    _logger.Debug("CombatController initialized with ViewModel");
}

// Typical event raising pattern
protected virtual void OnCombatEnded(CombatEndedEventArgs args)
{
    CombatEnded?.Invoke(this, args);
}

// Subscriber example
combatController.CombatEnded += (sender, args) =>
{
    if (args.PlayerVictorious)
    {
        _lootController.ShowLootScreen(args.DefeatedEnemies);
    }
    else
    {
        _deathController.HandlePlayerDeath(args.KillingBlow);
    }
};

```

---

## Event System Comparison

| Aspect | World Events | Environmental Events | Controller Events |
| --- | --- | --- | --- |
| **Scope** | Territory/World | Combat encounter | UI/Application |
| **Persistence** | Database (SQLite) | In-memory per combat | Transient |
| **Duration** | Days (1-10) | Combat turn | Instant |
| **Resolution** | Async (time-based) | Immediate | Synchronous |
| **Purpose** | World dynamics | Combat analytics | State transitions |
| **Model** | `WorldEvent` class | `EnvironmentalEvent` class | EventArgs classes |

---

## Data Flow Diagrams

### World Event Flow

```
[Daily Tick] → WorldEventService.ProcessDailyEventCheck()
                    │
                    ├── [Spawn Check] → SpawnRandomEvent()
                    │                       │
                    │                       └── INSERT → World_Events table
                    │
                    └── [Active Events] → ProcessEvent()
                                             │
                                             └── ResolveEvent()
                                                    │
                                                    ├── Apply consequences
                                                    ├── ShiftInfluence()
                                                    └── UPDATE World_Events

```

### Environmental Event Flow

```
[Combat Action] → EnvironmentalObjectService
                        │
                        ├── TriggerHazard() → HazardResult
                        ├── DestroyObject() → DestructionResult
                        ├── PushCharacter() → PushResult
                        └── CollapseStructure() → CollapseResult
                                │
                                └── Create EnvironmentalEvent
                                        │
                                        └── Log to combat instance

```

### Controller Event Flow

```
[Combat Victory] → CombatController
                        │
                        └── CombatEnded?.Invoke()
                                │
                                ├── [Victory] → LootController
                                │                   │
                                │                   └── LootCollectionComplete?.Invoke()
                                │                           │
                                │                           └── ProgressionController
                                │
                                └── [Defeat] → DeathController
                                                   │
                                                   └── DeathHandlingComplete?.Invoke()

```

---

## Usage Examples

### Subscribing to World Events

```csharp
// Check for active events in a sector
var events = _worldEventService.GetActiveSectorEvents(sectorId);
foreach (var evt in events)
{
    Console.WriteLine($"Active: {evt.EventTitle} ({evt.EventType})");
    Console.WriteLine($"  Duration: {evt.EventDurationDays} days");
    Console.WriteLine($"  Affected: {evt.AffectedFaction}");
}

// Process daily event check
_worldEventService.ProcessDailyEventCheck(sectorId);

```

### Tracking Environmental Events

```csharp
// Create environmental event for tracking
var environmentalEvent = new EnvironmentalEvent
{
    CombatInstanceId = combatId,
    TurnNumber = currentTurn,
    EventType = EnvironmentalEventType.HazardTriggered,
    ObjectId = hazardId,
    ActorId = null,  // Automatic trigger
    Targets = affectedCharacterIds,
    DamageDealt = totalDamage,
    StatusEffectApplied = "[Burning]",
    Description = "Fire pit erupted"
};

// Generate log entry
string logEntry = environmentalEvent.GenerateLogEntry();
// "💀 Fire pit erupted → 15 damage"

```

### Subscribing to Controller Events

```csharp
// Wire up combat flow
_combatController.CombatEnded += HandleCombatEnded;
_combatController.LootReady += HandleLootReady;
_lootController.LootCollectionComplete += HandleLootComplete;

private void HandleCombatEnded(object? sender, CombatEndedEventArgs e)
{
    if (e.PlayerVictorious)
    {
        _logger.Information("Combat victory - preparing loot");
    }
}

```

---

## Related Documentation

- [Service Architecture](https://www.notion.so/service-architecture.md) - Overall service design
- [Data Flow](https://www.notion.so/data-flow.md) - System data flow patterns
- [Combat Engine](https://www.notion.so/combat-engine.md) - Combat system details
- [Database Schema](https://www.notion.so/database-schema.md) - World_Events table structure