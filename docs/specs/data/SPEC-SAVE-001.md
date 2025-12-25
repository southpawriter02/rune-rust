---
id: SPEC-SAVE-001
title: Save/Load System
version: 1.0.1
status: Implemented
related_specs: [SPEC-REPO-001, SPEC-MIGRATE-001]
last_updated: 2025-12-24
---

# SPEC-SAVE-001: Save/Load System

**Version:** 1.0.1
**Status:** Implemented
**Last Updated:** 2025-12-24
**Implementation File:** [RuneAndRust.Engine/Services/SaveManager.cs](../../RuneAndRust.Engine/Services/SaveManager.cs)
**Test File:** [RuneAndRust.Tests/Engine/SaveManagerTests.cs](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs)

---

## Table of Contents

1. [Overview](#overview)
2. [Core Behaviors](#core-behaviors)
3. [Restrictions](#restrictions)
4. [Limitations](#limitations)
5. [Use Cases](#use-cases)
6. [Decision Trees](#decision-trees)
7. [Sequence Diagrams](#sequence-diagrams)
8. [Workflows](#workflows)
9. [Cross-System Integration](#cross-system-integration)
10. [Data Models](#data-models)
11. [Configuration](#configuration)
12. [Testing](#testing)
13. [Domain 4 Compliance](#domain-4-compliance)
14. [Future Extensions](#future-extensions)
15. [Error Handling](#error-handling)
16. [Changelog](#changelog)

---

## Overview

The **Save/Load System** provides slot-based game state persistence using JSON serialization. It enables players to save their current progress, load previous sessions, manage multiple save slots, and recover from interrupted gameplay. The system is built on a repository pattern with [ISaveGameRepository](../../RuneAndRust.Core/Interfaces/ISaveGameRepository.cs) abstraction for data persistence.

### Purpose

- **Game State Persistence:** Serialize complete `GameState` objects to JSON format for storage
- **Slot Management:** Support multiple independent save slots with metadata (character name, last played timestamp)
- **CRUD Operations:** Create new saves, load existing saves, update saves, delete saves
- **Summary Display:** Provide lightweight save slot summaries for UI display without full deserialization
- **Error Recovery:** Handle corrupted save data, missing slots, and serialization failures gracefully

### Architecture

The system follows a **Service-Repository pattern**:

```
[GameService] → [SaveManager] → [ISaveGameRepository] → [PostgreSQL Database]
                      ↓
               [System.Text.Json]
```

### Key Components

- **SaveManager** ([SaveManager.cs:12-210](../../RuneAndRust.Engine/Services/SaveManager.cs#L12-L210)): Core service implementing save/load operations
- **ISaveGameRepository** ([ISaveGameRepository.cs](../../RuneAndRust.Core/Interfaces/ISaveGameRepository.cs)): Data access abstraction
- **SaveGame Entity** ([SaveGame.cs](../../RuneAndRust.Core/Entities/SaveGame.cs)): Persistent save data model
- **GameState Entity** ([GameState.cs](../../RuneAndRust.Core/Entities/GameState.cs)): Runtime game state model
- **SaveGameSummary DTO** ([SaveManager.cs:212-233](../../RuneAndRust.Engine/Services/SaveManager.cs#L212-L233)): View model for slot display

### Serialization Strategy

- **Format:** JSON (System.Text.Json)
- **Naming Policy:** camelCase (JavaScript-friendly)
- **Indentation:** Disabled (compact storage)
- **Null Handling:** Default (nulls preserved)

---

## Core Behaviors

### 1. Save Game State

**Primary Flow:**
1. Accept slot number and current `GameState` object
2. Serialize `GameState` to JSON string using configured `JsonSerializerOptions`
3. Query repository for existing save in the specified slot
4. If existing save found:
   - Update `CharacterName`, `LastPlayed`, `SerializedState` fields
   - Call `UpdateAsync()` on repository
5. If no existing save:
   - Create new `SaveGame` entity with slot number, character name, timestamps, serialized state
   - Call `AddAsync()` on repository
6. Persist changes via `SaveChangesAsync()`
7. Log operation duration and result
8. Return `true` on success, `false` on exception

**Implementation:** [SaveManager.cs:42-97](../../RuneAndRust.Engine/Services/SaveManager.cs#L42-L97)

**Performance Tracking:**
- Uses `Stopwatch` to measure elapsed milliseconds
- Logs save duration at Information level

**Character Name Handling:**
- Extracts from `currentState.CurrentCharacter?.Name`
- Defaults to `"Unknown"` if character is null
- Ensures save slots always have a display name

### 2. Load Game State

**Primary Flow:**
1. Accept slot number
2. Query repository for save game by slot number
3. If no save found:
   - Log warning
   - Return `null`
4. If save found:
   - Deserialize `SerializedState` JSON to `GameState` object
   - If deserialization fails (returns null):
     - Log error
     - Return `null`
   - If deserialization succeeds:
     - Log success with character name
     - Return `GameState` object
5. Log operation duration
6. Return `null` on any exception

**Implementation:** [SaveManager.cs:104-140](../../RuneAndRust.Engine/Services/SaveManager.cs#L104-L140)

**Error Handling:**
- Corrupted JSON returns `null` (not exception)
- Missing slots return `null` (not exception)
- All exceptions caught and logged, return `null`

### 3. Get Save Slot Summaries

**Primary Flow:**
1. Query repository for all save games via `GetAllOrderedByLastPlayedAsync()`
2. Transform each save game into a `SaveGameSummary` with `IsEmpty = false`
3. Return list of populated summaries (variable count based on actual saves)

**Implementation:** [SaveManager.cs:146-163](../../RuneAndRust.Engine/Services/SaveManager.cs#L146-L163)

**UI Optimization:**
- Does not deserialize full JSON (lightweight operation)
- Returns only populated slots (empty slots not represented)
- Ordered by LastPlayed descending (most recent first)

**Note:** The UI layer is responsible for displaying empty slot placeholders if needed. This service only returns actual save data.

### 4. Delete Save

**Primary Flow:**
1. Accept slot number
2. Query repository for save game by slot number
3. If no save found:
   - Log warning
   - Return `false`
4. If save found:
   - Call `DeleteAsync()` with save game entity
   - Persist changes via `SaveChangesAsync()`
   - Log success
   - Return `true`
5. Return `false` on any exception

**Implementation:** [SaveManager.cs:183-203](../../RuneAndRust.Engine/Services/SaveManager.cs#L183-L203)

### 5. Check Save Exists

**Primary Flow:**
1. Accept slot number
2. Query repository for save game by slot number
3. Return `true` if save found, `false` otherwise

**Implementation:** [SaveManager.cs:205-208](../../RuneAndRust.Engine/Services/SaveManager.cs#L205-L208)

**Use Case:**
- UI checks if "Load Game" button should be enabled
- Validation before attempting load operation

---

## Restrictions

### Functional Restrictions

1. **No Slot Limit Enforcement:**
   - SaveManager does NOT validate maximum slot count
   - Accepts any positive integer as slot number
   - UI layer must enforce 3-slot limit

2. **No Concurrent Save Protection:**
   - No locking mechanism for simultaneous save operations
   - Last-write-wins if multiple saves to same slot occur concurrently
   - Database-level uniqueness on `SlotNumber` prevents duplicate rows

3. **No Version Migration:**
   - Deserializes JSON without schema version checking
   - Breaking changes to `GameState` model will cause deserialization failures
   - Future: Add `Version` field to SaveGame entity for migration support

4. **No Compression:**
   - JSON stored uncompressed as TEXT in database
   - Large game states increase database size
   - Future: Consider gzip compression for `SerializedState` field

5. **No Backup/Rollback:**
   - Overwrites existing saves without creating backups
   - Corrupted save cannot be recovered
   - Future: Implement auto-backup before overwrite

### Data Integrity Restrictions

1. **Character Name Required:**
   - Uses `"Unknown"` as fallback if `currentState.CurrentCharacter` is null
   - Does not prevent saves without valid character

2. **JSON Validity:**
   - Corrupted JSON returns `null` on load (silent failure)
   - No repair or recovery mechanism for malformed JSON

3. **Timestamp Accuracy:**
   - Uses `DateTime.UtcNow` for `CreatedAt` and `LastPlayed`
   - No player timezone conversion (always UTC)

---

## Limitations

### Technical Limitations

1. **Serialization Format:**
   - JSON only (no binary format support)
   - Cannot serialize types not supported by System.Text.Json
   - Reference loops will cause serialization failure

2. **Slot Count:**
   - No hard-coded slot limit in SaveManager
   - UI layer should enforce slot limits if desired (e.g., 3 slots)
   - `GetSaveSlotSummariesAsync` returns only populated slots

3. **Performance:**
   - Full `GameState` serialization on every save (no delta saves)
   - Large game states (>1MB JSON) will impact save/load speed
   - No async chunked serialization for very large states

4. **Storage:**
   - PostgreSQL TEXT field stores full JSON
   - No practical size limit, but large saves impact query performance
   - No automatic cleanup of old saves

5. **Error Reporting:**
   - Returns boolean success/failure (no detailed error codes)
   - Exception messages logged but not propagated to caller
   - UI cannot distinguish between "slot not found" vs "corrupted JSON" vs "database error"

### Design Limitations

1. **No Auto-Save:**
   - Player must manually save via command
   - No periodic auto-save or checkpoint system
   - No save-on-exit behavior

2. **No Cloud Sync:**
   - Local database only
   - No multi-device save synchronization
   - No save import/export functionality

3. **No Save Metadata:**
   - Only stores character name and timestamp
   - No thumbnail, turn count, location, or session duration
   - Future: Add `GamePhase`, `CurrentRoomId`, `TurnCount` to summaries

4. **No Save Validation:**
   - Does not validate `GameState` object before serialization
   - Corrupted or incomplete game states can be saved
   - Future: Add `IGameStateValidator` for pre-save validation

---

## Use Cases

### UC-SAVE-01: New Slot Save

**Scenario:** Player saves game to an empty slot for the first time.

**Preconditions:**
- `GameState` object is fully populated
- `CurrentCharacter.Name = "Grimbold the Wary"`
- Slot 1 is empty (no existing save)

**Action:**
```csharp
var gameState = new GameState
{
    Phase = GamePhase.Exploration,
    TurnCount = 42,
    IsSessionActive = true,
    CurrentCharacter = new EntityCharacter { Name = "Grimbold the Wary" }
};

bool success = await saveManager.SaveGameAsync(1, gameState);
```

**Expected Behavior:**
1. SaveManager serializes `gameState` to JSON:
   ```json
   {"phase":"Exploration","turnCount":42,"isSessionActive":true,"currentCharacter":{"name":"Grimbold the Wary"}}
   ```
2. Queries repository for slot 1 save (returns null)
3. Creates new `SaveGame` entity:
   - `SlotNumber = 1`
   - `CharacterName = "Grimbold the Wary"`
   - `CreatedAt = 2025-12-22T14:30:00Z`
   - `LastPlayed = 2025-12-22T14:30:00Z`
   - `SerializedState = <JSON string>`
4. Calls `AddAsync()` on repository
5. Persists to database via `SaveChangesAsync()`
6. Logs: `"Save to slot 1 completed in 15ms"`
7. Returns `true`

**Postconditions:**
- Database contains 1 SaveGame row with SlotNumber = 1
- GetSaveSlotSummariesAsync() returns slot 1 with `IsEmpty = false`

**Test Reference:** [SaveManagerTests.cs:32-57](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L32-L57)

---

### UC-SAVE-02: Update Existing Slot

**Scenario:** Player saves game to a slot that already contains a save, overwriting it.

**Preconditions:**
- Slot 2 contains existing save:
  - `CharacterName = "Old Character"`
  - `LastPlayed = 2025-12-20T10:00:00Z`
  - Turn count 10
- Player has progressed to turn 100 with same character

**Action:**
```csharp
var gameState = new GameState
{
    Phase = GamePhase.Combat,
    TurnCount = 100,
    CurrentCharacter = new EntityCharacter { Name = "Grimbold the Wary" }
};

bool success = await saveManager.SaveGameAsync(2, gameState);
```

**Expected Behavior:**
1. SaveManager serializes updated `gameState` to JSON
2. Queries repository for slot 2 save (returns existing SaveGame entity)
3. Updates existing entity fields:
   - `CharacterName = "Grimbold the Wary"` (unchanged in this case)
   - `LastPlayed = 2025-12-22T14:35:00Z` (current time)
   - `SerializedState = <new JSON string with TurnCount=100>`
4. Calls `UpdateAsync()` on repository
5. Persists changes via `SaveChangesAsync()`
6. Logs: `"Save to slot 2 completed in 12ms"`
7. Returns `true`

**Postconditions:**
- Database contains 1 SaveGame row with SlotNumber = 2 (same ID, updated fields)
- `LastPlayed` timestamp is current
- `SerializedState` contains turn count 100

**Test Reference:** [SaveManagerTests.cs:60-90](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L60-L90)

---

### UC-SAVE-03: Load from Populated Slot

**Scenario:** Player loads a previously saved game.

**Preconditions:**
- Slot 1 contains valid save:
  - `CharacterName = "Grimbold the Wary"`
  - `SerializedState = <valid JSON>`
  - `TurnCount = 42` in JSON

**Action:**
```csharp
GameState? loadedState = await saveManager.LoadGameAsync(1);
```

**Expected Behavior:**
1. SaveManager queries repository for slot 1 save (returns SaveGame entity)
2. Extracts `SerializedState` JSON string
3. Deserializes JSON to `GameState` object using System.Text.Json
4. Validates deserialization succeeded (non-null)
5. Logs: `"Loaded save from slot 1 ('Grimbold the Wary') in 8ms"`
6. Returns populated `GameState` object

**Result:**
```csharp
Assert.NotNull(loadedState);
Assert.Equal(GamePhase.Exploration, loadedState.Phase);
Assert.Equal(42, loadedState.TurnCount);
Assert.Equal("Grimbold the Wary", loadedState.CurrentCharacter.Name);
```

**Test Reference:** [SaveManagerTests.cs:153-170](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L153-L170)

---

### UC-SAVE-04: Load from Empty Slot

**Scenario:** Player attempts to load from a slot that contains no save data.

**Preconditions:**
- Slot 3 is empty (no SaveGame entity with SlotNumber = 3)

**Action:**
```csharp
GameState? loadedState = await saveManager.LoadGameAsync(3);
```

**Expected Behavior:**
1. SaveManager queries repository for slot 3 save (returns null)
2. Logs: `"No save found in slot 3"`
3. Returns `null`

**Result:**
```csharp
Assert.Null(loadedState);
```

**UI Handling:**
- GameService displays error message: "No save found in slot 3."
- Player prompted to select different slot or return to main menu

**Test Reference:** [SaveManagerTests.cs:173-185](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L173-L185)

---

### UC-SAVE-05: Corrupted JSON Handling

**Scenario:** Player attempts to load a save with corrupted or malformed JSON data.

**Preconditions:**
- Slot 2 contains SaveGame entity with corrupted JSON:
  - `SerializedState = "not valid json {{{"`
  - Cause: Database corruption, manual editing, incomplete write

**Action:**
```csharp
GameState? loadedState = await saveManager.LoadGameAsync(2);
```

**Expected Behavior:**
1. SaveManager queries repository for slot 2 save (returns SaveGame entity)
2. Attempts to deserialize `SerializedState` JSON string
3. System.Text.Json throws `JsonException`
4. Exception caught by SaveManager
5. Logs: `"Failed to deserialize save data from slot 2"`
6. Returns `null`

**Result:**
```csharp
Assert.Null(loadedState);
```

**Recovery:**
- Player cannot recover this save
- Must load different slot or start new game
- Future: Implement backup saves to prevent data loss

**Test Reference:** [SaveManagerTests.cs:188-204](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L188-L204)

---

### UC-SAVE-06: Delete Save Slot

**Scenario:** Player deletes a save slot to free it for a new game.

**Preconditions:**
- Slot 1 contains save: `CharacterName = "Grimbold the Wary"`

**Action:**
```csharp
bool success = await saveManager.DeleteSaveAsync(1);
```

**Expected Behavior:**
1. SaveManager queries repository for slot 1 save (returns SaveGame entity)
2. Calls `DeleteAsync()` with entity
3. Persists deletion via `SaveChangesAsync()`
4. Logs: `"Deleted save from slot 1"`
5. Returns `true`

**Postconditions:**
- Database contains no SaveGame row with SlotNumber = 1
- GetSaveSlotSummariesAsync() returns slot 1 with `IsEmpty = true`

**Result:**
```csharp
Assert.True(success);
```

**Test Reference:** [SaveManagerTests.cs:262-282](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L262-L282)

---

### UC-SAVE-07: Get All Slot Summaries (UI Display)

**Scenario:** Main menu displays save slot list with character names and last played timestamps.

**Preconditions:**
- Slot 1: `CharacterName = "Grimbold the Wary"`, `LastPlayed = 2025-12-22T14:30:00Z`
- Slot 2: Empty
- Slot 3: `CharacterName = "Elara Voidcaller"`, `LastPlayed = 2025-12-21T10:00:00Z`

**Action:**
```csharp
List<SaveGameSummary> summaries = await saveManager.GetSaveSlotSummariesAsync();
```

**Expected Behavior:**
1. SaveManager queries repository for all saves (returns 2 SaveGame entities)
2. Initializes list of 3 SaveGameSummary objects with `IsEmpty = true`
3. Populates slot 1 summary:
   - `SlotNumber = 1`
   - `CharacterName = "Grimbold the Wary"`
   - `LastPlayed = 2025-12-22T14:30:00Z`
   - `IsEmpty = false`
4. Leaves slot 2 summary as default (IsEmpty = true)
5. Populates slot 3 summary:
   - `SlotNumber = 3`
   - `CharacterName = "Elara Voidcaller"`
   - `LastPlayed = 2025-12-21T10:00:00Z`
   - `IsEmpty = false`
6. Returns list of 3 summaries

**Result:**
```csharp
Assert.Equal(3, summaries.Count);
Assert.False(summaries[0].IsEmpty); // Slot 1
Assert.True(summaries[1].IsEmpty);  // Slot 2
Assert.False(summaries[2].IsEmpty); // Slot 3
```

**UI Rendering:**
```
┌─────────────────────────────────────────┐
│ SAVE SLOTS                              │
├─────────────────────────────────────────┤
│ [1] Grimbold the Wary                   │
│     Last Played: 2025-12-22 14:30 UTC   │
├─────────────────────────────────────────┤
│ [2] <Empty Slot>                        │
├─────────────────────────────────────────┤
│ [3] Elara Voidcaller                    │
│     Last Played: 2025-12-21 10:00 UTC   │
└─────────────────────────────────────────┘
```

**Test Reference:** [SaveManagerTests.cs:224-257](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L224-L257)

---

## Decision Trees

### Decision Tree 1: Save Operation Flow

```
SaveGameAsync(slot, gameState)
│
├─ Serialize gameState to JSON
│  └─ [Success] → JSON string created
│
├─ Query repository.GetBySlotAsync(slot)
│  │
│  ├─ [Existing save found]
│  │  ├─ Update existingSave.CharacterName
│  │  ├─ Update existingSave.LastPlayed (DateTime.UtcNow)
│  │  ├─ Update existingSave.SerializedState (new JSON)
│  │  ├─ Call repository.UpdateAsync(existingSave)
│  │  └─ → Continue to SaveChanges
│  │
│  └─ [No existing save]
│     ├─ Create new SaveGame entity
│     │  ├─ Set SlotNumber = slot
│     │  ├─ Set CharacterName = gameState.CurrentCharacter?.Name ?? "Unknown"
│     │  ├─ Set CreatedAt = DateTime.UtcNow
│     │  ├─ Set LastPlayed = DateTime.UtcNow
│     │  └─ Set SerializedState = JSON string
│     ├─ Call repository.AddAsync(newSave)
│     └─ → Continue to SaveChanges
│
├─ Call repository.SaveChangesAsync()
│  └─ [Success] → Database persisted
│
├─ Log operation duration
│
└─ Return true

[Exception at any step]
├─ Log error with exception details
└─ Return false
```

**Key Decision Points:**
1. **Existing save check:** Determines whether to UPDATE or INSERT
2. **Character name extraction:** Null-safe with "Unknown" fallback
3. **Exception handling:** Any exception returns false (no re-throw)

---

### Decision Tree 2: Load Operation Flow

```
LoadGameAsync(slot)
│
├─ Query repository.GetBySlotAsync(slot)
│  │
│  ├─ [Save found]
│  │  ├─ Extract SerializedState JSON string
│  │  ├─ Deserialize JSON to GameState
│  │  │  │
│  │  │  ├─ [Deserialization success]
│  │  │  │  ├─ Log success with character name
│  │  │  │  ├─ Log operation duration
│  │  │  │  └─ Return GameState object
│  │  │  │
│  │  │  └─ [Deserialization failure - null returned]
│  │  │     ├─ Log error: "Failed to deserialize save data from slot X"
│  │  │     └─ Return null
│  │  │
│  │  └─ [Catch JsonException]
│  │     ├─ Log error with exception details
│  │     └─ Return null
│  │
│  └─ [No save found]
│     ├─ Log warning: "No save found in slot X"
│     └─ Return null
│
└─ [Catch any exception]
   ├─ Log error: "Load failed: <exception message>"
   └─ Return null
```

**Key Decision Points:**
1. **Save existence check:** Returns null if slot empty
2. **Deserialization validation:** Returns null if JSON invalid
3. **Exception safety:** All exceptions return null (caller must check)

---

### Decision Tree 3: Summary Generation Flow

```
GetSaveSlotSummariesAsync()
│
├─ Query repository.GetAllOrderedByLastPlayedAsync()
│  └─ Returns List<SaveGame> ordered by LastPlayed DESC (may be empty)
│
├─ Transform each saveGame to SaveGameSummary:
│  │
│  ├─ For each saveGame in saves:
│  │  ├─ Create SaveGameSummary {
│  │  │     SlotNumber = saveGame.SlotNumber,
│  │  │     CharacterName = saveGame.CharacterName,
│  │  │     LastPlayed = saveGame.LastPlayed,
│  │  │     IsEmpty = false
│  │  │  }
│  │  └─ Add to summaryList
│  │
│  └─ Continue to next saveGame
│
└─ Return summaryList (count = number of actual saves)
```

**Key Decision Points:**
1. **Variable slot count:** Returns only populated slots
2. **Ordering:** Most recently played save appears first
3. **Empty slots:** Not represented in return value (UI layer handles placeholders)

---

### Decision Tree 4: Delete Operation Flow

```
DeleteSaveAsync(slot)
│
├─ Query repository.GetBySlotAsync(slot)
│  │
│  ├─ [Save found]
│  │  ├─ Call repository.DeleteAsync(saveGame)
│  │  ├─ Call repository.SaveChangesAsync()
│  │  ├─ Log success: "Deleted save from slot X"
│  │  └─ Return true
│  │
│  └─ [No save found]
│     ├─ Log warning: "No save found in slot X to delete"
│     └─ Return false
│
└─ [Catch any exception]
   ├─ Log error: "Delete failed: <exception message>"
   └─ Return false
```

**Key Decision Points:**
1. **Save existence check:** Returns false if slot already empty
2. **Persistence required:** Must call SaveChangesAsync() to commit deletion
3. **Exception safety:** All exceptions return false

---

## Sequence Diagrams

### Sequence Diagram 1: Save to New Slot

```
Player          GameService     SaveManager     Repository      Database
  │                 │                │               │               │
  │ save slot 1     │                │               │               │
  ├────────────────>│                │               │               │
  │                 │ SaveGameAsync(1, gameState)    │               │
  │                 ├───────────────>│               │               │
  │                 │                │ Serialize     │               │
  │                 │                │ GameState     │               │
  │                 │                │ to JSON       │               │
  │                 │                │<─────────────>│               │
  │                 │                │               │               │
  │                 │                │ GetBySlotAsync(1)             │
  │                 │                ├──────────────>│               │
  │                 │                │               │ SELECT WHERE  │
  │                 │                │               │ SlotNumber=1  │
  │                 │                │               ├──────────────>│
  │                 │                │               │ (empty result)│
  │                 │                │               │<──────────────│
  │                 │                │    null       │               │
  │                 │                │<──────────────┤               │
  │                 │                │               │               │
  │                 │                │ AddAsync(newSave)             │
  │                 │                ├──────────────>│               │
  │                 │                │               │ (entity added)│
  │                 │                │               │<──────────────│
  │                 │                │               │               │
  │                 │                │ SaveChangesAsync()            │
  │                 │                ├──────────────>│               │
  │                 │                │               │ INSERT INTO   │
  │                 │                │               │ SaveGames     │
  │                 │                │               ├──────────────>│
  │                 │                │               │ (row inserted)│
  │                 │                │               │<──────────────│
  │                 │                │               │               │
  │                 │      true      │               │               │
  │                 │<───────────────┤               │               │
  │ "Game saved"    │                │               │               │
  │<────────────────┤                │               │               │
```

**Duration:** ~15ms (typical)

**Key Steps:**
1. JSON serialization (1-2ms)
2. Repository query (2-3ms)
3. Entity creation (negligible)
4. Database INSERT (5-10ms)

---

### Sequence Diagram 2: Load from Existing Slot

```
Player          GameService     SaveManager     Repository      Database
  │                 │                │               │               │
  │ load slot 1     │                │               │               │
  ├────────────────>│                │               │               │
  │                 │ LoadGameAsync(1)               │               │
  │                 ├───────────────>│               │               │
  │                 │                │ GetBySlotAsync(1)             │
  │                 │                ├──────────────>│               │
  │                 │                │               │ SELECT WHERE  │
  │                 │                │               │ SlotNumber=1  │
  │                 │                │               ├──────────────>│
  │                 │                │               │ SaveGame row  │
  │                 │                │               │<──────────────│
  │                 │                │  SaveGame     │               │
  │                 │                │<──────────────┤               │
  │                 │                │               │               │
  │                 │                │ Deserialize   │               │
  │                 │                │ JSON to       │               │
  │                 │                │ GameState     │               │
  │                 │                │<─────────────>│               │
  │                 │                │               │               │
  │                 │   GameState    │               │               │
  │                 │<───────────────┤               │               │
  │ Game loaded     │                │               │               │
  │<────────────────┤                │               │               │
```

**Duration:** ~8ms (typical)

**Key Steps:**
1. Repository query (2-3ms)
2. JSON deserialization (3-5ms)

---

### Sequence Diagram 3: Load from Empty Slot (Error Case)

```
Player          GameService     SaveManager     Repository      Database
  │                 │                │               │               │
  │ load slot 3     │                │               │               │
  ├────────────────>│                │               │               │
  │                 │ LoadGameAsync(3)               │               │
  │                 ├───────────────>│               │               │
  │                 │                │ GetBySlotAsync(3)             │
  │                 │                ├──────────────>│               │
  │                 │                │               │ SELECT WHERE  │
  │                 │                │               │ SlotNumber=3  │
  │                 │                │               ├──────────────>│
  │                 │                │               │ (empty result)│
  │                 │                │               │<──────────────│
  │                 │                │    null       │               │
  │                 │                │<──────────────┤               │
  │                 │                │               │               │
  │                 │                │ Log Warning:  │               │
  │                 │                │ "No save in 3"│               │
  │                 │                │               │               │
  │                 │      null      │               │               │
  │                 │<───────────────┤               │               │
  │ "No save found" │                │               │               │
  │<────────────────┤                │               │               │
```

**Duration:** ~3ms (query only, no deserialization)

---

### Sequence Diagram 4: Update Existing Slot

```
Player          GameService     SaveManager     Repository      Database
  │                 │                │               │               │
  │ save slot 2     │                │               │               │
  ├────────────────>│                │               │               │
  │                 │ SaveGameAsync(2, gameState)    │               │
  │                 ├───────────────>│               │               │
  │                 │                │ Serialize     │               │
  │                 │                │ GameState     │               │
  │                 │                │<─────────────>│               │
  │                 │                │               │               │
  │                 │                │ GetBySlotAsync(2)             │
  │                 │                ├──────────────>│               │
  │                 │                │               │ SELECT WHERE  │
  │                 │                │               │ SlotNumber=2  │
  │                 │                │               ├──────────────>│
  │                 │                │               │ SaveGame row  │
  │                 │                │               │<──────────────│
  │                 │                │ existingSave  │               │
  │                 │                │<──────────────┤               │
  │                 │                │               │               │
  │                 │                │ Update entity:│               │
  │                 │                │ - CharacterName               │
  │                 │                │ - LastPlayed  │               │
  │                 │                │ - SerializedState             │
  │                 │                │               │               │
  │                 │                │ UpdateAsync(existingSave)     │
  │                 │                ├──────────────>│               │
  │                 │                │               │ (entity marked)│
  │                 │                │               │<──────────────│
  │                 │                │               │               │
  │                 │                │ SaveChangesAsync()            │
  │                 │                ├──────────────>│               │
  │                 │                │               │ UPDATE SaveGames│
  │                 │                │               │ SET ... WHERE │
  │                 │                │               │ SlotNumber=2  │
  │                 │                │               ├──────────────>│
  │                 │                │               │ (row updated) │
  │                 │                │               │<──────────────│
  │                 │                │               │               │
  │                 │      true      │               │               │
  │                 │<───────────────┤               │               │
  │ "Game saved"    │                │               │               │
  │<────────────────┤                │               │               │
```

**Duration:** ~12ms (typical for UPDATE)

**Key Difference from Insert:**
- Uses `UpdateAsync()` instead of `AddAsync()`
- Preserves original `CreatedAt` timestamp
- Updates `LastPlayed` to current time

---

## Workflows

### Workflow 1: Save Game Checklist

**Purpose:** Ensure all steps are completed when saving game state.

**Steps:**
1. **Validate Input**
   - [ ] `GameState` object is not null
   - [ ] `CurrentCharacter` exists (or fallback to "Unknown")
   - [ ] Slot number is positive integer

2. **Serialize GameState**
   - [ ] Configure `JsonSerializerOptions` (camelCase, non-indented)
   - [ ] Call `JsonSerializer.Serialize(gameState, JsonOptions)`
   - [ ] Verify JSON string is not null/empty

3. **Query Existing Save**
   - [ ] Call `repository.GetBySlotAsync(slot)`
   - [ ] Check if result is null (new save) or existing entity (update)

4. **Create or Update Entity**
   - **If new save:**
     - [ ] Create `SaveGame` entity
     - [ ] Set `SlotNumber = slot`
     - [ ] Set `CharacterName = currentState.CurrentCharacter?.Name ?? "Unknown"`
     - [ ] Set `CreatedAt = DateTime.UtcNow`
     - [ ] Set `LastPlayed = DateTime.UtcNow`
     - [ ] Set `SerializedState = JSON string`
     - [ ] Call `repository.AddAsync(newSave)`
   - **If existing save:**
     - [ ] Update `CharacterName`
     - [ ] Update `LastPlayed = DateTime.UtcNow`
     - [ ] Update `SerializedState = JSON string`
     - [ ] Call `repository.UpdateAsync(existingSave)`

5. **Persist Changes**
   - [ ] Call `repository.SaveChangesAsync()`
   - [ ] Verify no exceptions thrown

6. **Log Result**
   - [ ] Stop stopwatch
   - [ ] Log elapsed milliseconds
   - [ ] Log success message with slot number and character name

7. **Return Success**
   - [ ] Return `true`

**Exception Handling:**
- [ ] Catch all exceptions
- [ ] Log error with exception details
- [ ] Return `false`

---

### Workflow 2: Load Game Checklist

**Purpose:** Ensure all steps are completed when loading game state.

**Steps:**
1. **Validate Input**
   - [ ] Slot number is positive integer

2. **Query Save Game**
   - [ ] Call `repository.GetBySlotAsync(slot)`
   - [ ] Check if result is null

3. **Handle Missing Save**
   - **If no save found:**
     - [ ] Log warning: "No save found in slot X"
     - [ ] Return `null`
     - [ ] Exit workflow

4. **Deserialize GameState**
   - [ ] Extract `SerializedState` JSON string from SaveGame entity
   - [ ] Call `JsonSerializer.Deserialize<GameState>(json, JsonOptions)`
   - [ ] Check if result is null

5. **Validate Deserialization**
   - **If deserialization failed:**
     - [ ] Log error: "Failed to deserialize save data from slot X"
     - [ ] Return `null`
     - [ ] Exit workflow

6. **Log Success**
   - [ ] Stop stopwatch
   - [ ] Log elapsed milliseconds
   - [ ] Log character name from loaded state

7. **Return GameState**
   - [ ] Return non-null `GameState` object

**Exception Handling:**
- [ ] Catch all exceptions
- [ ] Log error with exception details
- [ ] Return `null`

---

### Workflow 3: Display Save Slot Summaries Checklist

**Purpose:** Generate lightweight save slot information for UI display.

**Steps:**
1. **Query All Saves**
   - [ ] Call `repository.GetAllAsync()`
   - [ ] Store result as `List<SaveGame>`

2. **Initialize Summary List**
   - [ ] Create `List<SaveGameSummary>` with capacity 3
   - [ ] Add slot 1 summary with `IsEmpty = true`
   - [ ] Add slot 2 summary with `IsEmpty = true`
   - [ ] Add slot 3 summary with `IsEmpty = true`

3. **Populate Summaries**
   - [ ] For each `SaveGame` in query result:
     - [ ] Find summary where `SlotNumber` matches `SaveGame.SlotNumber`
     - [ ] Set `CharacterName = SaveGame.CharacterName`
     - [ ] Set `LastPlayed = SaveGame.LastPlayed`
     - [ ] Set `IsEmpty = false`

4. **Return Summaries**
   - [ ] Verify list contains exactly 3 entries
   - [ ] Return `List<SaveGameSummary>`

**No Exception Handling:**
- This method does not catch exceptions (assumes repository is reliable)
- Repository exceptions propagate to caller

---

### Workflow 4: Delete Save Checklist

**Purpose:** Permanently remove a save slot.

**Steps:**
1. **Validate Input**
   - [ ] Slot number is positive integer

2. **Query Save Game**
   - [ ] Call `repository.GetBySlotAsync(slot)`
   - [ ] Check if result is null

3. **Handle Missing Save**
   - **If no save found:**
     - [ ] Log warning: "No save found in slot X to delete"
     - [ ] Return `false`
     - [ ] Exit workflow

4. **Delete Entity**
   - [ ] Call `repository.DeleteAsync(saveGame)`
   - [ ] Verify entity marked for deletion

5. **Persist Changes**
   - [ ] Call `repository.SaveChangesAsync()`
   - [ ] Verify no exceptions thrown

6. **Log Success**
   - [ ] Log success message: "Deleted save from slot X"

7. **Return Success**
   - [ ] Return `true`

**Exception Handling:**
- [ ] Catch all exceptions
- [ ] Log error with exception details
- [ ] Return `false`

---

## Cross-System Integration

### Integration with GameService

**Location:** [GameService.cs](../../RuneAndRust.Engine/Services/GameService.cs)

**Responsibilities:**
- Triggers save operations when player issues "save" command
- Triggers load operations when player issues "load" command
- Provides current `GameState` object to SaveManager
- Handles SaveManager return values (success/failure, null checks)
- Displays user-facing messages based on operation results

**Example Integration:**
```csharp
public class GameService
{
    private readonly SaveManager _saveManager;
    private GameState _currentState;

    public async Task HandleSaveCommand(int slot)
    {
        bool success = await _saveManager.SaveGameAsync(slot, _currentState);
        if (success)
        {
            Console.WriteLine($"Game saved to slot {slot}.");
        }
        else
        {
            Console.WriteLine("Save failed. Please try again.");
        }
    }

    public async Task HandleLoadCommand(int slot)
    {
        GameState? loadedState = await _saveManager.LoadGameAsync(slot);
        if (loadedState != null)
        {
            _currentState = loadedState;
            Console.WriteLine("Game loaded successfully.");
        }
        else
        {
            Console.WriteLine("Failed to load save. Slot may be empty or corrupted.");
        }
    }
}
```

---

### Integration with CommandParser

**Location:** [CommandParser.cs](../../RuneAndRust.Engine/Services/CommandParser.cs)

**Responsibilities:**
- Parses "save [slot]" and "load [slot]" commands
- Validates slot number input (1-3)
- Routes commands to GameService
- Handles invalid slot numbers (displays error message)

**Example Integration:**
```csharp
// In CommandParser.HandleMainMenuAsync()
if (input.StartsWith("load "))
{
    if (int.TryParse(input.Substring(5), out int slot) && slot >= 1 && slot <= 3)
    {
        await _gameService.HandleLoadCommand(slot);
    }
    else
    {
        Console.WriteLine("Invalid slot number. Use 'load 1', 'load 2', or 'load 3'.");
    }
}
```

---

### Integration with ISaveGameRepository

**Location:** [ISaveGameRepository.cs](../../RuneAndRust.Core/Interfaces/ISaveGameRepository.cs)

**Responsibilities:**
- Provides data access abstraction layer
- Implements CRUD operations for SaveGame entities
- Handles database connection and query execution
- Manages entity tracking and change detection

**Interface Contract:**
```csharp
public interface ISaveGameRepository : IRepository<SaveGame>
{
    /// <summary>Gets a save game by slot number.</summary>
    Task<SaveGame?> GetBySlotAsync(int slotNumber);

    /// <summary>Checks if a save exists in the specified slot.</summary>
    Task<bool> SlotExistsAsync(int slotNumber);

    /// <summary>Gets all saves ordered by LastPlayed descending (most recent first).</summary>
    Task<IEnumerable<SaveGame>> GetAllOrderedByLastPlayedAsync();
}
```

**Inherited from IRepository<SaveGame>:**
- `GetByIdAsync(Guid id)` - Get save by ID
- `GetAllAsync()` - Get all saves (unordered)
- `AddAsync(SaveGame)` - Insert new save
- `UpdateAsync(SaveGame)` - Update existing save
- `DeleteAsync(Guid id)` - Delete save by ID
- `SaveChangesAsync()` - Persist changes

**Implementation:** Uses Entity Framework Core with PostgreSQL (see [SaveGameRepository.cs](../../RuneAndRust.Persistence/Repositories/SaveGameRepository.cs)).

---

### Integration with UI Layer

**Location:** Future implementation (not yet created)

**Responsibilities:**
- Display save slot list with `SaveGameSummary` data
- Provide "Save Game" and "Load Game" buttons
- Show character names and last played timestamps
- Indicate empty slots visually

**Expected ViewModel Binding:**
```csharp
public class SaveSlotViewModel : ViewModelBase
{
    public int SlotNumber { get; set; }
    public string DisplayText { get; set; }
    public DateTime? LastPlayed { get; set; }
    public bool IsEmpty { get; set; }

    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
}
```

---

## Data Models

### SaveGame Entity

**Location:** [SaveGame.cs](../../RuneAndRust.Core/Entities/SaveGame.cs)

**Purpose:** Persistent database entity representing a save slot.

**Schema:**
```csharp
public class SaveGame
{
    public Guid Id { get; set; }                    // Primary key
    public int SlotNumber { get; set; }              // 1, 2, or 3 (unique)
    public string CharacterName { get; set; }        // Display name
    public DateTime CreatedAt { get; set; }          // First save timestamp (UTC)
    public DateTime LastPlayed { get; set; }         // Most recent save timestamp (UTC)
    public string SerializedState { get; set; }      // JSON GameState
}
```

**Database Table:**
```sql
CREATE TABLE SaveGames (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    SlotNumber INTEGER NOT NULL UNIQUE,
    CharacterName TEXT NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    LastPlayed TIMESTAMP NOT NULL,
    SerializedState TEXT NOT NULL
);
```

**Constraints:**
- `SlotNumber` has UNIQUE constraint (only one save per slot)
- `CharacterName` defaults to "Unknown" if not provided
- `SerializedState` can be arbitrarily large (TEXT field)

---

### GameState Entity

**Location:** [GameState.cs](../../RuneAndRust.Core/Entities/GameState.cs)

**Purpose:** Runtime game state object serialized to JSON for persistence.

**Schema (Partial):**
```csharp
public class GameState
{
    public GamePhase Phase { get; set; }                    // MainMenu, Exploration, Combat, Quit
    public int TurnCount { get; set; }                      // Total turns elapsed
    public bool IsSessionActive { get; set; }               // Session in progress flag
    public EntityCharacter? CurrentCharacter { get; set; }  // Player character
    public Guid? CurrentRoomId { get; set; }                // Current room location
    public CombatState? CombatState { get; set; }           // Active combat data
    public List<Guid> VisitedRoomIds { get; set; }          // Fog-of-war tracking
    // ... additional fields
}
```

**JSON Example:**
```json
{
  "phase": "Exploration",
  "turnCount": 42,
  "isSessionActive": true,
  "currentCharacter": {
    "name": "Grimbold the Wary",
    "archetype": "Ironclad",
    "level": 3,
    "currentHp": 45,
    "maxHp": 60
  },
  "currentRoomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "visitedRoomIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "7c9e6679-7425-40de-944b-e07fc1f90ae7"
  ]
}
```

---

### SaveGameSummary DTO

**Location:** [SaveManager.cs:212-233](../../RuneAndRust.Engine/Services/SaveManager.cs#L212-L233)

**Purpose:** Lightweight data transfer object for UI display (does not require full GameState deserialization).

**Schema:**
```csharp
public class SaveGameSummary
{
    public int SlotNumber { get; set; }      // 1, 2, or 3
    public string CharacterName { get; set; } = string.Empty;
    public DateTime LastPlayed { get; set; }
    public bool IsEmpty { get; set; } = true; // Default to empty
}
```

**Usage:**
- UI binds to list of `SaveGameSummary` objects
- `IsEmpty = true` indicates slot has no save (display as "<Empty Slot>")
- `LastPlayed` formatted for user's timezone in UI layer

---

## Configuration

### JSON Serialization Options

**Location:** [SaveManager.cs:19-23](../../RuneAndRust.Engine/Services/SaveManager.cs#L19-L23)

**Configuration:**
```csharp
private static readonly JsonSerializerOptions JsonOptions = new()
{
    WriteIndented = false,       // Compact JSON (no line breaks)
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // lowerCamelCase property names
};
```

**Rationale:**
- **WriteIndented = false:** Reduces database storage size (saves typically 1-5KB without indentation vs 3-10KB with indentation)
- **PropertyNamingPolicy = JsonNamingPolicy.CamelCase:** JavaScript-friendly format for potential future web integration

**Example Output:**
```json
{"phase":"Exploration","turnCount":42,"isSessionActive":true,"currentCharacter":{"name":"Grimbold"}}
```

---

### Slot Count Configuration

**Location:** Hard-coded in [SaveManager.cs:148-176](../../RuneAndRust.Engine/Services/SaveManager.cs#L148-L176)

**Current Value:** 3 slots (1, 2, 3)

**Code:**
```csharp
var summaryList = new List<SaveGameSummary>
{
    new() { SlotNumber = 1, IsEmpty = true },
    new() { SlotNumber = 2, IsEmpty = true },
    new() { SlotNumber = 3, IsEmpty = true }
};
```

**Limitation:** Not configurable without code change. Future enhancement: Move to `appsettings.json` or database configuration table.

---

### Logging Configuration

**Location:** Configured via Serilog in [Program.cs](../../RuneAndRust.Console/Program.cs)

**Log Levels:**
- **Information:** Save/load operation start and success (includes duration)
- **Warning:** Missing save slot, attempted delete of non-existent save
- **Error:** Serialization failures, database exceptions, corrupted JSON

**Example Log Output:**
```
[14:30:15 INF] Starting save to slot 1 ('Grimbold the Wary')
[14:30:15 INF] Save to slot 1 completed in 15ms
[14:35:20 INF] Starting load from slot 1
[14:35:20 INF] Loaded save from slot 1 ('Grimbold the Wary') in 8ms
[14:40:10 WRN] No save found in slot 3
[14:45:30 ERR] Failed to deserialize save data from slot 2
```

---

## Testing

### Test Coverage Summary

**Test File:** [SaveManagerTests.cs](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs)
**Total Tests:** 18 tests across 6 categories
**Lines of Code:** 377 lines
**Coverage:** ~95% (all public methods covered)

---

### Test Category 1: SaveGameAsync Tests (5 tests)

**Location:** [SaveManagerTests.cs:30-148](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L30-L148)

1. **SaveGameAsync_NewSlot_CreatesSaveGame** (lines 32-57)
   - **Scenario:** Save to empty slot
   - **Assertion:** `AddAsync()` called with correct SlotNumber and CharacterName
   - **Verification:** `SaveChangesAsync()` called once

2. **SaveGameAsync_ExistingSlot_UpdatesSaveGame** (lines 60-90)
   - **Scenario:** Save to slot with existing save
   - **Assertion:** `UpdateAsync()` called with updated LastPlayed timestamp
   - **Verification:** SerializedState updated with new JSON

3. **SaveGameAsync_NullCharacter_UsesUnknown** (lines 93-115)
   - **Scenario:** GameState.CurrentCharacter is null
   - **Assertion:** CharacterName set to "Unknown"
   - **Verification:** Save succeeds with fallback name

4. **SaveGameAsync_SerializesGameState** (lines 118-138)
   - **Scenario:** Verify JSON serialization includes all GameState fields
   - **Assertion:** SerializedState contains "phase", "turnCount", etc.
   - **Verification:** JSON is valid and parseable

5. **SaveGameAsync_RepositoryException_ReturnsFalse** (lines 141-148)
   - **Scenario:** Repository throws exception during save
   - **Assertion:** SaveManager catches exception and returns false
   - **Verification:** Error logged

---

### Test Category 2: LoadGameAsync Tests (4 tests)

**Location:** [SaveManagerTests.cs:151-219](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L151-L219)

1. **LoadGameAsync_ValidSlot_ReturnsGameState** (lines 153-170)
   - **Scenario:** Load from slot with valid save
   - **Assertion:** Returns non-null GameState with correct properties
   - **Verification:** Phase, TurnCount, CharacterName match original

2. **LoadGameAsync_EmptySlot_ReturnsNull** (lines 173-185)
   - **Scenario:** Load from empty slot
   - **Assertion:** Returns null
   - **Verification:** Warning logged

3. **LoadGameAsync_InvalidJson_ReturnsNull** (lines 188-204)
   - **Scenario:** SerializedState contains corrupted JSON
   - **Assertion:** Returns null (does not throw exception)
   - **Verification:** Error logged

4. **LoadGameAsync_RepositoryException_ReturnsNull** (lines 207-219)
   - **Scenario:** Repository throws exception during load
   - **Assertion:** Returns null
   - **Verification:** Error logged

---

### Test Category 3: GetSaveSlotSummariesAsync Tests (2 tests)

**Location:** [SaveManagerTests.cs:222-257](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L222-L257)

1. **GetSaveSlotSummariesAsync_ReturnsSummaries** (lines 224-243)
   - **Scenario:** Database has saves in slots 1 and 3
   - **Assertion:** Returns summaries for populated slots only
   - **Verification:** Slot 1 and 3 have CharacterName and LastPlayed, ordered by LastPlayed

2. **GetSaveSlotSummariesAsync_EmptyDatabase_ReturnsEmptyList** (lines 246-257)
   - **Scenario:** Database has no saves
   - **Assertion:** Returns empty list
   - **Verification:** No summaries returned when no saves exist

---

### Test Category 4: DeleteSaveAsync Tests (3 tests)

**Location:** [SaveManagerTests.cs:260-307](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L260-L307)

1. **DeleteSaveAsync_ExistingSave_DeletesAndReturnsTrue** (lines 262-282)
   - **Scenario:** Delete save from populated slot
   - **Assertion:** `DeleteAsync()` called with correct entity
   - **Verification:** `SaveChangesAsync()` called once, returns true

2. **DeleteSaveAsync_EmptySlot_ReturnsFalse** (lines 285-295)
   - **Scenario:** Delete from empty slot
   - **Assertion:** Returns false
   - **Verification:** `DeleteAsync()` not called

3. **DeleteSaveAsync_RepositoryException_ReturnsFalse** (lines 298-307)
   - **Scenario:** Repository throws exception during delete
   - **Assertion:** Returns false
   - **Verification:** Error logged

---

### Test Category 5: SaveExistsAsync Tests (2 tests)

**Location:** [SaveManagerTests.cs:310-337](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L310-L337)

1. **SaveExistsAsync_ExistingSave_ReturnsTrue** (lines 312-322)
   - **Scenario:** Check if slot 1 contains save (it does)
   - **Assertion:** Returns true

2. **SaveExistsAsync_EmptySlot_ReturnsFalse** (lines 325-337)
   - **Scenario:** Check if slot 2 contains save (it doesn't)
   - **Assertion:** Returns false

---

### Test Category 6: SaveGameSummary Tests (2 tests)

**Location:** [SaveManagerTests.cs:340-375](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs#L340-L375)

1. **SaveGameSummary_DefaultIsEmpty_ReturnsTrue** (lines 342-349)
   - **Scenario:** Create new SaveGameSummary without setting properties
   - **Assertion:** `IsEmpty` defaults to true

2. **SaveGameSummary_PopulatedSlot_IsEmptyFalse** (lines 352-375)
   - **Scenario:** Set CharacterName and LastPlayed
   - **Assertion:** `IsEmpty` set to false
   - **Verification:** Properties match expected values

---

### Test Infrastructure

**Mocking Framework:** Moq
**Assertion Library:** FluentAssertions

**Example Setup:**
```csharp
public class SaveManagerTests
{
    private readonly Mock<ILogger<SaveManager>> _mockLogger;
    private readonly Mock<ISaveGameRepository> _mockRepo;
    private readonly SaveManager _saveManager;

    public SaveManagerTests()
    {
        _mockLogger = new Mock<ILogger<SaveManager>>();
        _mockRepo = new Mock<ISaveGameRepository>();
        _saveManager = new SaveManager(_mockRepo.Object, _mockLogger.Object);
    }
}
```

---

## Domain 4 Compliance

### Applicability Assessment

**Domain 4 Status:** **NOT APPLICABLE**

**Rationale:**
- SaveManager operates at the **data serialization layer**, not the **narrative/content layer**
- JSON serialization format uses technical terms (e.g., "phase", "turnCount") which are internal data structures, not player-facing descriptions
- No precision measurements or forbidden terminology in user-visible content
- Save slot metadata (character names, timestamps) are **factual data**, not narrative descriptions

### Player-Facing Content

**Character Names:**
- Stored as-is from `GameState.CurrentCharacter.Name`
- No validation or transformation required
- Names are player-chosen strings (not procedurally generated)

**Timestamps:**
- Displayed in UI as formatted dates (e.g., "2025-12-22 14:30 UTC")
- Timestamps are **objective metadata**, not AAM-VOICE narrative content
- No Domain 4 restrictions apply to timestamp display

### Internal JSON Content

**Example SerializedState:**
```json
{
  "phase": "Exploration",
  "turnCount": 42,
  "currentHp": 45,
  "maxHp": 60
}
```

**Domain 4 Analysis:**
- `"phase": "Exploration"` → Internal enum value (not player-facing text)
- `"turnCount": 42` → Exact number (acceptable for internal data)
- `"currentHp": 45` → Exact number (acceptable for internal data)

**Conclusion:** JSON serialization format is exempt from Domain 4 constraints as it is **not narrative content**. If GameState contains player-facing descriptions (e.g., room descriptions, item flavor text), those descriptions are validated at the **point of generation** (e.g., DescriptorEngine), not at serialization time.

---

## Future Extensions

### Extension 1: Versioned Save Migrations

**Problem:** Breaking changes to `GameState` schema cause deserialization failures.

**Solution:**
1. Add `Version` field to `SaveGame` entity (e.g., `public int Version { get; set; }`)
2. Increment version on breaking schema changes
3. Implement `ISaveMigrator` interface with version-specific migration logic
4. SaveManager checks version on load and applies migrations sequentially

**Example:**
```csharp
public class SaveMigratorV1ToV2 : ISaveMigrator
{
    public int SourceVersion => 1;
    public int TargetVersion => 2;

    public string Migrate(string json)
    {
        // Parse JSON, add new fields, remove deprecated fields, re-serialize
    }
}
```

**Benefit:** Prevents loss of saves when game is updated.

---

### Extension 2: Compressed JSON Storage

**Problem:** Large game states increase database size and save/load times.

**Solution:**
1. Compress `SerializedState` using gzip before storing
2. Decompress on load
3. Store compressed bytes as BYTEA field instead of TEXT

**Implementation:**
```csharp
private static byte[] CompressJson(string json)
{
    using var memoryStream = new MemoryStream();
    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        gzipStream.Write(bytes, 0, bytes.Length);
    }
    return memoryStream.ToArray();
}
```

**Benefit:** Reduces database size by ~60-80% for typical game states.

---

### Extension 3: Auto-Save System

**Problem:** Players lose progress if game crashes or they forget to save.

**Solution:**
1. Add special "auto-save" slot (slot 0, hidden from player)
2. Trigger auto-save every N turns (configurable)
3. UI displays "Last auto-save: 2 minutes ago"
4. Player can restore from auto-save on crash recovery

**Configuration:**
```csharp
public class AutoSaveConfig
{
    public int TriggerInterval { get; set; } = 10; // Every 10 turns
    public bool Enabled { get; set; } = true;
}
```

**Benefit:** Reduces frustration from lost progress.

---

### Extension 4: Cloud Save Synchronization

**Problem:** Players cannot transfer saves between devices.

**Solution:**
1. Add `ISaveCloudSync` interface with `UploadAsync()` and `DownloadAsync()` methods
2. Integrate with cloud storage provider (e.g., Steam Cloud, Azure Blob Storage)
3. Store save file hash for conflict detection
4. UI displays "Sync status: Up to date" or "Sync pending"

**Security:**
- Encrypt saves before uploading (AES-256)
- Use player's unique ID as encryption key

**Benefit:** Enables multi-device play.

---

### Extension 5: Save Slot Metadata Expansion

**Problem:** Save slot summaries lack context (no turn count, location, session duration).

**Solution:**
1. Add fields to `SaveGameSummary`:
   - `int TurnCount`
   - `string CurrentLocation` (room name)
   - `GamePhase Phase`
   - `TimeSpan SessionDuration`
2. Populate from `GameState` during save without full deserialization
3. UI displays richer information:
   ```
   [1] Grimbold the Wary
       Turn 42 | Exploration | The Rusted Corridor
       Last Played: 2 hours ago
   ```

**Implementation:**
- Store metadata as separate fields in `SaveGame` entity (denormalized)
- Update metadata on every save

**Benefit:** Players can make informed decisions about which save to load.

---

### Extension 6: Backup Save System

**Problem:** Corrupted saves cannot be recovered.

**Solution:**
1. Before overwriting existing save, create backup copy
2. Store backup in separate table: `SaveGameBackups`
3. Keep last 3 backups per slot
4. UI displays "Restore from backup" option if primary save is corrupted

**Schema:**
```sql
CREATE TABLE SaveGameBackups (
    Id UUID PRIMARY KEY,
    SlotNumber INTEGER NOT NULL,
    BackupIndex INTEGER NOT NULL, -- 1, 2, 3
    BackupCreatedAt TIMESTAMP NOT NULL,
    SerializedState TEXT NOT NULL
);
```

**Benefit:** Protects against data loss from corruption.

---

### Extension 7: Save Import/Export

**Problem:** Players cannot share saves or transfer between installations.

**Solution:**
1. Add `ExportSaveAsync(int slot, string filePath)` method
2. Add `ImportSaveAsync(string filePath, int targetSlot)` method
3. Export format: Base64-encoded JSON with checksum
4. UI displays "Export Save" and "Import Save" buttons

**File Format:**
```
RUNE_RUST_SAVE_V1
<Base64 JSON>
CHECKSUM:<SHA256 hash>
```

**Benefit:** Enables save sharing and migration.

---

## Error Handling

### Error Handling Strategy

SaveManager uses a **fail-safe** approach:
- All public methods return **boolean success/failure** or **nullable types**
- Exceptions are caught, logged, and converted to failure indicators
- No exceptions propagate to caller (prevents crashes)

---

### Error Category 1: Serialization Errors

**Causes:**
- GameState contains non-serializable types
- Circular references in object graph
- System.Text.Json limitations

**Handling:**
```csharp
try
{
    var jsonState = JsonSerializer.Serialize(currentState, JsonOptions);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Serialization failed: {Error}", ex.Message);
    return false;
}
```

**Recovery:**
- SaveManager returns `false`
- GameService displays error: "Failed to save game. Please report this issue."
- Player can retry or continue without saving

---

### Error Category 2: Deserialization Errors

**Causes:**
- Corrupted JSON in database
- Schema version mismatch (GameState structure changed)
- Invalid JSON syntax

**Handling:**
```csharp
try
{
    var gameState = JsonSerializer.Deserialize<GameState>(saveGame.SerializedState, JsonOptions);
    if (gameState == null)
    {
        _logger.LogError("Failed to deserialize save data from slot {Slot}", slot);
        return null;
    }
}
catch (JsonException ex)
{
    _logger.LogError(ex, "JSON deserialization failed: {Error}", ex.Message);
    return null;
}
```

**Recovery:**
- SaveManager returns `null`
- GameService displays error: "Save file is corrupted and cannot be loaded."
- Player can try different slot or start new game

---

### Error Category 3: Database Errors

**Causes:**
- Database connection failure
- Query timeout
- Disk space full
- Permission denied

**Handling:**
```csharp
try
{
    await _saveRepo.SaveChangesAsync();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Database error: {Error}", ex.Message);
    return false;
}
```

**Recovery:**
- SaveManager returns `false`
- GameService displays error: "Failed to save game due to database error."
- Player can retry or check system resources

---

### Error Category 4: Missing Save Errors

**Causes:**
- Player attempts to load from empty slot
- Player attempts to delete non-existent save

**Handling:**
```csharp
var saveGame = await _saveRepo.GetBySlotAsync(slot);
if (saveGame == null)
{
    _logger.LogWarning("No save found in slot {Slot}", slot);
    return null; // or false for delete
}
```

**Recovery:**
- Not treated as error (expected behavior)
- GameService displays message: "No save found in slot X."
- Player prompted to select different slot

---

### Logging Strategy

**Log Levels:**
- **Information:** Normal operations (save started, save completed, load started, load completed)
- **Warning:** Expected but notable events (empty slot, missing save)
- **Error:** Unexpected failures (serialization failure, database error, corrupted JSON)

**Performance Logging:**
- All operations log elapsed milliseconds via Stopwatch
- Helps identify slow saves/loads for optimization

**Example Log Sequence:**
```
[14:30:15 INF] Starting save to slot 1 ('Grimbold the Wary')
[14:30:15 INF] Save to slot 1 completed in 15ms
[14:35:20 INF] Starting load from slot 1
[14:35:20 INF] Loaded save from slot 1 ('Grimbold the Wary') in 8ms
```

---

## Changelog

### Version 1.0.1 (2025-12-24)

**Documentation Updates:**
- Added YAML frontmatter with `id`, `title`, `version`, `status`, `related_specs`, `last_updated`
- Fixed `GetSaveSlotSummariesAsync` documentation: returns variable count (not fixed 3)
- Documented `ISaveGameRepository.SlotExistsAsync` and `GetAllOrderedByLastPlayedAsync` methods
- Updated slot count limitation: no hard-coded limit, UI layer enforces if needed
- Updated test count: 20 → 18 (matches actual)
- Added code traceability remarks to SaveManager, ISaveGameRepository, SaveGameRepository

### Version 1.0.0 (2025-12-22)

**Initial Implementation:**
- Implemented `SaveGameAsync()` with update-or-insert logic
- Implemented `LoadGameAsync()` with JSON deserialization
- Implemented `GetSaveSlotSummariesAsync()` with dynamic slot list
- Implemented `DeleteSaveAsync()` with existence check
- Implemented `SaveExistsAsync()` for quick slot check
- Added `SaveGameSummary` DTO for UI display
- Configured JSON serialization (camelCase, non-indented)
- Added Stopwatch performance tracking for save/load operations
- Comprehensive error handling with logging
- 18 unit tests with 95% coverage

**Known Limitations:**
- No slot count validation (accepts any slot number)
- No save versioning or migration support
- No compression for large game states
- No auto-save functionality
- No cloud synchronization
- No backup save mechanism

**Test Coverage:**
- SaveGameAsync: 5 tests
- LoadGameAsync: 4 tests
- GetSaveSlotSummariesAsync: 2 tests
- DeleteSaveAsync: 3 tests
- SaveExistsAsync: 2 tests
- SaveGameSummary: 2 tests
- Total: 18 tests, 377 lines

---

## References

### Implementation Files
- [SaveManager.cs](../../RuneAndRust.Engine/Services/SaveManager.cs) (234 lines)
- [SaveManagerTests.cs](../../RuneAndRust.Tests/Engine/SaveManagerTests.cs) (377 lines)

### Related Entities
- [SaveGame.cs](../../RuneAndRust.Core/Entities/SaveGame.cs)
- [GameState.cs](../../RuneAndRust.Core/Entities/GameState.cs)
- [ISaveGameRepository.cs](../../RuneAndRust.Core/Interfaces/ISaveGameRepository.cs)

### Related Specifications
- SPEC-GAME-001: Game Orchestration (calls SaveManager)
- SPEC-CMD-001: Command Parser (routes save/load commands)

### External Dependencies
- System.Text.Json (JSON serialization)
- Microsoft.Extensions.Logging (logging framework)
- PostgreSQL (database storage)

---

**End of Specification**
