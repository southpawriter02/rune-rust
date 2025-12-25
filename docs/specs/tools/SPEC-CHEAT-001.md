---
id: SPEC-CHEAT-001
title: Cheat Command System
version: 1.0.0
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-DEBUG-001, SPEC-COMBAT-001, SPEC-TRAUMA-001]
---

# SPEC-CHEAT-001: Cheat Command System

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `ICheatService`, `CheatService`
> **Location:** `RuneAndRust.Engine/Services/CheatService.cs`

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Behaviors](#behaviors)
- [Restrictions](#restrictions)
- [Limitations](#limitations)
- [Use Cases](#use-cases)
- [Decision Trees](#decision-trees)
- [Cross-Links](#cross-links)
- [Related Services](#related-services)
- [Data Models](#data-models)
- [Configuration](#configuration)
- [Testing](#testing)
- [Design Rationale](#design-rationale)
- [Changelog](#changelog)

---

## Overview

The Cheat Command System (introduced in v0.3.17b "The Toolbox") provides developers with gameplay manipulation tools for testing and debugging. Accessible through the Debug Console (SPEC-DEBUG-001), the system offers five commands: God Mode toggle, instant full heal, spatial teleportation, map reveal, and item spawning (placeholder).

The system integrates deeply with combat and trauma systems to enforce God Mode invulnerability across all damage vectors: physical attacks, psychic stress, and Runic Blight corruption. All cheat operations are logged at Warning level to maintain an audit trail during testing sessions.

Key features include GUID and partial name matching for teleportation, non-persistent God Mode state (not saved to disk), and complete state restoration with the heal command.

---

## Core Concepts

### God Mode

A toggle state stored in `GameState.IsGodMode` that grants complete invulnerability when enabled:
- **Damage Negation:** Physical damage set to 0 in AttackResolutionService
- **Stress Negation:** Psychic stress returns 0 in TraumaService
- **Corruption Negation:** Runic Blight corruption returns 0 in TraumaService

God Mode is marked with `[JsonIgnore]` to prevent serialization to save files, ensuring it resets on game load.

### Cheat Command Prefix

All cheat commands use the `/` prefix to distinguish them from built-in console commands:
- `/god` - Toggle God Mode
- `/heal` - Full restoration
- `/tp <room>` - Teleport to room
- `/reveal` - Reveal all map rooms
- `/spawn` - Item spawning (placeholder)

### Dual Lookup for Teleportation

Teleportation supports two lookup methods:
1. **GUID Lookup:** Exact match on room ID (e.g., `3fa85f64-5717-4562-b3fc-2c963f66afa6`)
2. **Name Lookup:** Case-insensitive partial match (e.g., `sunken` matches "The Sunken Archive")

---

## Behaviors

### Primary Behaviors

#### 1. Toggle God Mode (`ToggleGodMode`)

```csharp
bool ToggleGodMode()
```

**Purpose:** Flips the invulnerability state for the player character.

**Logic:**
1. Log current state at Debug level
2. Flip `_state.IsGodMode` boolean
3. Log new state at Warning level
4. Return new state for console feedback

**Example:**
```csharp
var newState = cheatService.ToggleGodMode();
// newState == true if God Mode is now ON
// newState == false if God Mode is now OFF
```

**Integration Points:**
- `AttackResolutionService.ResolveMeleeAttack()` checks `IsGodMode` before applying damage
- `TraumaService.InflictStress()` checks `IsGodMode` before applying stress
- `TraumaService.AddCorruption()` checks `IsGodMode` before applying corruption

#### 2. Full Heal (`FullHeal`)

```csharp
bool FullHeal()
```

**Purpose:** Instantly restores the player character to perfect condition.

**Logic:**
1. Log invocation at Debug level
2. Validate `CurrentCharacter` is not null
3. If null, log warning and return false
4. Restore: `CurrentHP = MaxHP`
5. Restore: `CurrentStamina = MaxStamina`
6. Restore: `CurrentAp = MaxAp`
7. Set: `PsychicStress = 0`
8. Clear: `ActiveStatusEffects`
9. Log restoration at Warning level with all values
10. Return true

**Example:**
```csharp
if (cheatService.FullHeal())
{
    console.WriteLog("Character fully restored.", "Cheat");
}
else
{
    console.WriteLog("No active character.", "Error");
}
```

**Restoration Targets:**

| Property | Restored To | Purpose |
|----------|-------------|---------|
| CurrentHP | MaxHP | Full health |
| CurrentStamina | MaxStamina | Full stamina |
| CurrentAp | MaxAp | Full Aether Points |
| PsychicStress | 0 | Clear stress |
| ActiveStatusEffects | Empty | Clear debuffs |

#### 3. Teleport (`TeleportAsync`)

```csharp
Task<string?> TeleportAsync(string roomIdOrName)
```

**Purpose:** Instantly moves the player to a specified room.

**Logic:**
1. Log invocation with input at Debug level
2. Attempt GUID parse on input
3. If valid GUID:
   - Query `IRoomRepository.GetByIdAsync(roomId)`
   - If found: Update state, log Warning with "(GUID match)", return room name
4. If not GUID or GUID not found:
   - Load all rooms via `GetAllRoomsAsync()`
   - Search for case-insensitive partial name match
   - If found: Update state, log Warning with "(name match)", return room name
5. If neither found: Log warning, return null

**State Updates on Success:**
```csharp
_state.CurrentRoomId = room.Id;
_state.VisitedRoomIds.Add(room.Id);  // Reveals on map
```

**Example:**
```csharp
// By GUID
var result = await cheatService.TeleportAsync("3fa85f64-5717-4562-b3fc-2c963f66afa6");
// result == "The Sunken Archive" (if found)

// By partial name
var result = await cheatService.TeleportAsync("sunken");
// result == "The Sunken Archive" (case-insensitive partial match)
```

#### 4. Reveal Map (`RevealMapAsync`)

```csharp
Task<int> RevealMapAsync()
```

**Purpose:** Clears fog of war by adding all rooms to the visited set.

**Logic:**
1. Log invocation at Debug level
2. Load all rooms via `GetAllRoomsAsync()`
3. For each room:
   - Attempt `VisitedRoomIds.Add(room.Id)`
   - If returns true (newly added), increment count
4. Log count at Warning level
5. Return count of newly revealed rooms

**Example:**
```csharp
var count = await cheatService.RevealMapAsync();
console.WriteLog($"Revealed {count} rooms.", "Cheat");
// count == 12 if 12 rooms were newly revealed
// count == 0 if all rooms were already visited
```

#### 5. Spawn Item (`SpawnItemAsync`)

```csharp
Task<bool> SpawnItemAsync(string itemId, int quantity = 1)
```

**Purpose:** Placeholder for future item spawning functionality.

**Current Implementation:**
1. Log at Warning level with item ID and quantity
2. Return false (not yet implemented)

**Future Implementation Notes:**
- Will require `IItemRepository` integration
- Will add items directly to player inventory
- Will respect stack limits and inventory capacity

---

## Restrictions

### What This System MUST NOT Do

1. **Never persist God Mode:** The `[JsonIgnore]` attribute prevents serialization; God Mode resets on load.

2. **Never modify enemy combatants:** God Mode only protects the player; enemies take full damage.

3. **Never bypass game logic for non-player entities:** Teleportation only affects player position.

4. **Never allow remote execution:** Cheat commands are only accessible through the local debug console.

5. **Never suppress logging:** All cheat operations log at Warning level for audit trail.

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| Teleport lookup | Full table scan | `GetAllRoomsAsync()` loads entire room list for name search |
| Name matching | First match only | `FirstOrDefault()` returns first partial match; duplicates not handled |
| Item spawn | Not implemented | Placeholder awaiting ItemRepository integration |
| God Mode scope | Player only | Enemies and NPCs unaffected |
| State persistence | None | God Mode resets on game load |

---

## Use Cases

### UC-1: Enable God Mode for Combat Testing

**Actor:** Developer
**Trigger:** Type `/god` in debug console
**Preconditions:** Game running, debug console visible

```csharp
// DebugConsoleRenderer.ProcessCheatCommand
case "god":
case "godmode":
    var state = _cheats.ToggleGodMode();
    _console.WriteLog($"God Mode: {(state ? "ON" : "OFF")}", "Cheat");
    break;
```

**Postconditions:**
- God Mode enabled/disabled
- Player takes no damage/stress/corruption while enabled
- Console displays confirmation

### UC-2: Full Recovery After Testing Damage

**Actor:** Developer
**Trigger:** Type `/heal` in debug console
**Preconditions:** Character exists, may have damage/stress/debuffs

```csharp
case "heal":
    if (_cheats.FullHeal())
    {
        _console.WriteLog("Character fully restored.", "Cheat");
    }
    else
    {
        _console.WriteLog("No active character.", "Error");
    }
    break;
```

**Postconditions:**
- HP = MaxHP
- Stamina = MaxStamina
- AP = MaxAp
- Stress = 0
- All status effects cleared

### UC-3: Teleport to Specific Room by Name

**Actor:** Developer
**Trigger:** Type `/tp sunken` in debug console
**Preconditions:** Room "The Sunken Archive" exists in database

```csharp
case "tp":
case "teleport":
    if (string.IsNullOrEmpty(args))
    {
        _console.WriteLog("Usage: /tp <room-id or name>", "Error");
        break;
    }
    var roomName = _cheats.TeleportAsync(args).GetAwaiter().GetResult();
    if (roomName != null)
    {
        _console.WriteLog($"Teleported to: {roomName}", "Cheat");
    }
    else
    {
        _console.WriteLog($"Room not found: {args}", "Error");
    }
    break;
```

**Postconditions:**
- Player moved to matched room
- Room added to VisitedRoomIds (map revealed)
- Console displays room name

### UC-4: Reveal Entire Map

**Actor:** Developer
**Trigger:** Type `/reveal` in debug console
**Preconditions:** Some rooms may be unexplored

```csharp
case "reveal":
    var count = _cheats.RevealMapAsync().GetAwaiter().GetResult();
    _console.WriteLog($"Revealed {count} rooms.", "Cheat");
    break;
```

**Postconditions:**
- All rooms added to VisitedRoomIds
- Map UI shows all rooms
- Console displays count of newly revealed rooms

---

## Decision Trees

### God Mode Check Flow (Combat)

```
┌─────────────────────────────────────────────────────────────┐
│              AttackResolutionService.ResolveMeleeAttack      │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
                    ┌────────────────────┐
                    │ Calculate damage   │
                    │ (dice, soak, etc)  │
                    └────────┬───────────┘
                             │
                    ┌────────┴────────┐
                    │ Defender is     │
                    │ Player?         │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │ NO           │              │ YES
              ▼              │              ▼
    ┌─────────────────┐      │    ┌─────────────────────────┐
    │ Apply normal    │      │    │ GameState.IsGodMode?    │
    │ damage          │      │    └───────────┬─────────────┘
    └─────────────────┘      │                │
                             │    ┌───────────┼───────────┐
                             │    │ NO        │           │ YES
                             │    ▼           │           ▼
                             │ ┌──────────┐   │    ┌──────────────┐
                             │ │ Apply    │   │    │ Set damage=0 │
                             │ │ damage   │   │    │ Log bypass   │
                             │ └──────────┘   │    └──────────────┘
```

### Teleport Lookup Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    TeleportAsync(input)                      │
└────────────────────────────┬────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │ Guid.TryParse   │
                    │ (input)?        │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │ YES          │              │ NO
              ▼              │              ▼
    ┌─────────────────┐      │    ┌─────────────────────────┐
    │ GetByIdAsync    │      │    │ GetAllRoomsAsync        │
    │ (GUID)          │      │    │ + FirstOrDefault        │
    └────────┬────────┘      │    │ (partial name match)    │
             │               │    └───────────┬─────────────┘
    ┌────────┴────────┐      │                │
    │ Found?          │      │       ┌────────┴────────┐
    └────────┬────────┘      │       │ Found?          │
             │               │       └────────┬────────┘
    ┌────────┼────────┐      │       ┌────────┼────────┐
    │ YES    │   NO   │      │       │ YES    │   NO   │
    ▼        ▼        │      │       ▼        ▼
┌────────┐ (fall      │      │   ┌────────┐ ┌────────┐
│ Update │  through   │      │   │ Update │ │ Return │
│ state, │  to name   │      │   │ state, │ │ null   │
│ return │  search)   │      │   │ return │ └────────┘
│ name   │            │      │   │ name   │
└────────┘            │      │   └────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `GameState` | [SPEC-GAME-001](../core/SPEC-GAME-001.md) | IsGodMode, CurrentCharacter, CurrentRoomId |
| `IRoomRepository` | [SPEC-REPO-001](../data/SPEC-REPO-001.md) | Room lookup for teleportation |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| `DebugConsoleRenderer` | [SPEC-DEBUG-001](./SPEC-DEBUG-001.md) | Routes cheat commands |
| `AttackResolutionService` | [SPEC-ATTACK-001](../combat/SPEC-ATTACK-001.md) | God Mode damage check |
| `TraumaService` | [SPEC-TRAUMA-001](../character/SPEC-TRAUMA-001.md) | God Mode stress/corruption check |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `RuneAndRust.Core/Interfaces/ICheatService.cs` | Service contract | 1-41 |
| `RuneAndRust.Engine/Services/CheatService.cs` | Cheat execution | 1-132 |
| `RuneAndRust.Core/Models/GameState.cs` | IsGodMode property | 70-75, 91 |

### Integration Points

| File | Purpose | Key Lines |
|------|---------|-----------|
| `RuneAndRust.Engine/Services/AttackResolutionService.cs` | God Mode damage negation | 169-173 |
| `RuneAndRust.Engine/Services/TraumaService.cs` | God Mode stress negation | 40-56 |
| `RuneAndRust.Engine/Services/TraumaService.cs` | God Mode corruption negation | 337-351 |
| `RuneAndRust.Terminal/Rendering/DebugConsoleRenderer.cs` | Command routing | 154-215 |

---

## Data Models

### GameState.IsGodMode

```csharp
/// <summary>
/// Gets or sets whether God Mode is active (v0.3.17b).
/// When true, player takes no damage, stress, or corruption.
/// Not serialized to save files.
/// </summary>
[JsonIgnore]
public bool IsGodMode { get; set; } = false;
```

**Key Attributes:**
- `[JsonIgnore]` - Prevents serialization to save files
- Default: `false` - Disabled by default
- Reset in `Reset()` method - Cleared on new game

### Cheat Command Syntax

| Command | Syntax | Arguments | Example |
|---------|--------|-----------|---------|
| God Mode | `/god` or `/godmode` | None | `/god` |
| Full Heal | `/heal` | None | `/heal` |
| Teleport | `/tp <room>` | GUID or partial name | `/tp sunken` |
| Reveal | `/reveal` | None | `/reveal` |
| Spawn | `/spawn <item> [qty]` | Item ID, optional quantity | `/spawn sword_01 2` |

---

## Configuration

### Constants

No configurable constants; all behavior is hardcoded.

### DI Lifetime

| Service | Lifetime | Rationale |
|---------|----------|-----------|
| `ICheatService` | Scoped | Depends on scoped IRoomRepository |

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `RuneAndRust.Tests/Engine/CheatServiceTests.cs` | 16 | All cheat methods |

### Critical Test Scenarios

1. **ToggleGodMode_FlipsState_FalseToTrue** - Initial toggle enables
2. **ToggleGodMode_FlipsState_TrueToFalse** - Second toggle disables
3. **ToggleGodMode_ReturnsNewState** - Returns correct boolean
4. **FullHeal_RestoresHP** - HP set to MaxHP
5. **FullHeal_RestoresStamina** - Stamina set to MaxStamina
6. **FullHeal_RestoresAP** - AP set to MaxAp
7. **FullHeal_ClearsStress** - PsychicStress set to 0
8. **FullHeal_ClearsStatusEffects** - ActiveStatusEffects cleared
9. **FullHeal_ReturnsFalse_WhenNoCharacter** - Handles null character
10. **TeleportAsync_ByGuid_TeleportsToRoom** - GUID lookup works
11. **TeleportAsync_ByName_TeleportsToMatchingRoom** - Name lookup works
12. **TeleportAsync_ReturnsNull_WhenRoomNotFound** - Handles not found
13. **RevealMapAsync_AddsAllRoomsToVisited** - All rooms revealed
14. **RevealMapAsync_ReturnsCount_OfNewlyRevealed** - Correct count
15. **RevealMapAsync_ReturnsZero_WhenAllAlreadyVisited** - Handles pre-visited
16. **SpawnItemAsync_ReturnsFalse_NotYetImplemented** - Placeholder behavior

### Validation Checklist

- [x] God Mode toggles correctly
- [x] God Mode blocks damage in combat
- [x] God Mode blocks stress in trauma
- [x] God Mode blocks corruption in trauma
- [x] Full heal restores all values
- [x] Full heal clears status effects
- [x] Teleport works with GUID
- [x] Teleport works with partial name
- [x] Reveal map counts correctly
- [x] God Mode not saved to disk

---

## Design Rationale

### Why Non-Persistent God Mode?

- **Prevents Accidental Cheating:** Loading a save automatically disables cheats
- **Intentional Checkpoint:** Developer must re-enable cheats after each load
- **Clean Saves:** Production saves never contain cheat state

### Why Deep Integration vs. Wrapper Pattern?

- **Performance:** 4 lines per check; JIT-optimizable
- **Audit Trail:** Each bypass logged at Debug level
- **Minimal Coupling:** Only requires `GameState.IsGodMode` check

### Why Warning-Level Logging?

- **Visibility:** Cheats should stand out in logs during testing
- **Audit Trail:** Easy to identify when cheats were used
- **Debugging:** Helps trace unexpected behavior during development

### Why Full Table Scan for Name Lookup?

- **Simplicity:** Avoids complex query building for partial match
- **Acceptable for Dev Tool:** Not performance-critical in debug context
- **Room Count:** Typical dungeon has <100 rooms; full scan is fast

### Why Async for Database Operations?

- **Consistency:** Matches async pattern used throughout codebase
- **Future-Proofing:** Remote database scenarios may require async
- **EF Core Best Practice:** Async recommended for database access

---

## Changelog

### v1.0.0 (2025-12-25)

- Initial specification documenting v0.3.17b implementation
- Documents ICheatService, CheatService
- Documents God Mode integration in AttackResolutionService
- Documents God Mode integration in TraumaService
- Documents teleportation with GUID and name matching
- Documents map reveal and item spawn placeholder
- Documents GameState.IsGodMode with [JsonIgnore]
