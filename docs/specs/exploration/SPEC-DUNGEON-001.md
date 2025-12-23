---
id: SPEC-DUNGEON-001
title: Dungeon Generation System
version: 2.0.0
status: Implemented
related_specs: [SPEC-ENVPOP-001, SPEC-SPAWN-001, SPEC-NAV-001, SPEC-DICE-001]
---

# SPEC-DUNGEON-001: Dungeon Generation System

**Version:** 2.0.0 (v0.4.0 Dynamic Room Engine + v0.0.5 Legacy Test Map)
**Status:** Implemented
**Last Updated:** 2025-12-22
**Owner:** Engine Team
**Category:** World Generation

---

## Overview

The **Dungeon Generator** creates interconnected room networks with bidirectional exits, spatial coordinates, and environmental content. The current implementation (v0.0.5) provides a **deterministic 5-room test map** to validate navigation and gameplay systems before implementing procedural generation algorithms (planned: Wave Function Collapse, BSP, etc.).

### Core Design Principles

1. **Deterministic Test Layout**: Fixed 5-room structure with predictable connections for QA testing
2. **Bidirectional Exit Integrity**: All connections are two-way (enforced by `LinkRooms()` method)
3. **Spatial Coordinate System**: 3D positioning using `Coordinate(X, Y, Z)` value objects
4. **Clean Slate Generation**: Clears existing rooms before creating new layout (idempotent operation)
5. **Environment Integration**: Delegates hazard/condition spawning to `EnvironmentPopulator` (v0.3.3c)

### System Boundaries

**IN SCOPE:**
- Test map generation (5 specific rooms)
- Bidirectional room linking (exit creation)
- Spatial positioning (3D coordinate assignment)
- Environment population integration
- Database persistence (room entity creation)

**OUT OF SCOPE:**
- Procedural generation algorithms (future feature)
- Room content generation (interactable objects, loot - handled by other systems)
- Enemy spawning (handled by EnvironmentPopulator)
- Hazard/condition placement (handled by EnvironmentPopulator)
- Save/load of dungeon state (handled by SaveManager)

---

## Behaviors

### Primary Behaviors

#### 1. Test Map Generation (`GenerateTestMapAsync`)

**Trigger:** New game initialization or manual dungeon regeneration

**Process:**
1. **Database Cleanup**:
   ```csharp
   await _roomRepository.ClearAllRoomsAsync();
   ```

2. **Room Creation**:
   ```csharp
   var rooms = CreateTestRooms(); // Returns Dictionary<Coordinate, Room> (5 rooms)
   ```

3. **Exit Linking**:
   ```csharp
   LinkRooms(rooms); // Populates bidirectional exits
   ```

4. **Environment Population** (v0.3.3c):
   ```csharp
   await _environmentPopulator.PopulateDungeonAsync(rooms.Values);
   ```

5. **Database Persistence**:
   ```csharp
   await _roomRepository.AddRangeAsync(rooms.Values);
   await _roomRepository.SaveChangesAsync();
   ```

6. **Starting Room ID Return**:
   ```csharp
   var startingRoom = rooms.Values.First(r => r.IsStartingRoom);
   return startingRoom.Id;
   ```

**Outcomes:**
- **Success**: 5 rooms created, linked, populated, and persisted. Returns starting room GUID.
- **Database Failure**: Exception propagates to caller (no retry logic).

**Logging:**
```csharp
_logger.LogInformation("Generating test dungeon map...");
_logger.LogDebug("Created {Count} room entities", rooms.Count); // 5
_logger.LogDebug("Linking rooms together...");
_logger.LogDebug("Room linking complete. Entry hall has {ExitCount} exits.", entry.Exits.Count); // 4
_logger.LogInformation("Generated dungeon with {Count} rooms. Starting room: {RoomName} ({RoomId})", ...);
```

---

#### 2. Room Creation (`CreateTestRooms`)

**Internal Method** - Creates the 5 hardcoded room entities

**Room Definitions:**

**1. Entry Hall (0,0,0) - Starting Room**
```csharp
new Room
{
    Name = "Entry Hall",
    Description = "A cold, metallic chamber. The air smells of ozone and ancient dust. " +
                 "Faded runes pulse weakly along the walls, their meaning lost to time. " +
                 "Passages lead in several directions.",
    Position = new Coordinate(0, 0, 0),
    IsStartingRoom = true
}
```

**2. Rusted Corridor (0,1,0) - North of Entry**
```csharp
new Room
{
    Name = "Rusted Corridor",
    Description = "Corroded pipes line the walls of this narrow passage. " +
                 "Water drips from unseen sources, leaving rust-red stains on the floor. " +
                 "The air grows colder here.",
    Position = new Coordinate(0, 1, 0)
}
```

**3. Storage Chamber (1,0,0) - East of Entry**
```csharp
new Room
{
    Name = "Storage Chamber",
    Description = "Broken crates and shattered containers litter this abandoned storeroom. " +
                 "Whatever was kept here was either looted long ago or claimed by decay. " +
                 "Dust motes drift in the pale light.",
    Position = new Coordinate(1, 0, 0)
}
```

**4. Collapsed Tunnel (-1,0,0) - West of Entry**
```csharp
new Room
{
    Name = "Collapsed Tunnel",
    Description = "Rubble partially blocks this passage. The ceiling groans ominously overhead. " +
                 "Cracks in the walls reveal glimpses of darkness beyond. " +
                 "This area seems unstable.",
    Position = new Coordinate(-1, 0, 0)
}
```

**5. The Pit (0,0,-1) - Below Entry**
```csharp
new Room
{
    Name = "The Pit",
    Description = "A deep shaft descends into absolute darkness. " +
                 "Ancient machinery clings to the walls, silent and still. " +
                 "The echoes of your footsteps seem to go on forever.",
    Position = new Coordinate(0, 0, -1)
}
```

**Return Value:**
```csharp
Dictionary<Coordinate, Room> rooms = {
    [Coordinate(0,0,0)]   = entryHall,
    [Coordinate(0,1,0)]   = corridor,
    [Coordinate(1,0,0)]   = storage,
    [Coordinate(-1,0,0)]  = collapsed,
    [Coordinate(0,0,-1)]  = pit
};
```

**Spatial Layout:**
```
        Corridor (0,1,0)
             |
             N
             |
Collapsed --- Entry --- Storage
(-1,0,0)  W   |   E    (1,0,0)
           (0,0,0)
             |
             D
             |
        The Pit (0,0,-1)
```

---

#### 3. Exit Linking (`LinkRooms`)

**Internal Method** - Establishes bidirectional connections between rooms

**Exit Configuration:**

**Entry Hall → Connected to All 4 Rooms:**
```csharp
entry.Exits[Direction.North] = corridor.Id;
entry.Exits[Direction.East]  = storage.Id;
entry.Exits[Direction.West]  = collapsed.Id;
entry.Exits[Direction.Down]  = pit.Id;
```

**Corridor → Connected to Entry Only:**
```csharp
corridor.Exits[Direction.South] = entry.Id;
```

**Storage → Connected to Entry Only:**
```csharp
storage.Exits[Direction.West] = entry.Id;
```

**Collapsed → Connected to Entry Only:**
```csharp
collapsed.Exits[Direction.East] = entry.Id;
```

**Pit → Connected to Entry Only:**
```csharp
pit.Exits[Direction.Up] = entry.Id;
```

**Bidirectionality Enforcement:**
- Every `entry.Exits[Direction.X] = targetRoom.Id` has corresponding `targetRoom.Exits[OppositeDirection] = entry.Id`
- Example: Entry → North → Corridor AND Corridor → South → Entry

**Exit Count Summary:**
- Entry Hall: 4 exits (North, East, West, Down)
- Corridor: 1 exit (South)
- Storage: 1 exit (West)
- Collapsed: 1 exit (East)
- Pit: 1 exit (Up)
- **Total: 8 exit references (4 bidirectional connections)**

---

#### 4. Opposite Direction Calculation (`GetOppositeDirection`)

**Purpose:** Static utility method for bidirectional exit creation

**Implementation:**
```csharp
public static Direction GetOppositeDirection(Direction direction)
{
    return direction switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };
}
```

**Properties:**
- **Symmetric**: `GetOppositeDirection(GetOppositeDirection(x)) == x`
- **Exhaustive**: Covers all 6 `Direction` enum values
- **Pure Function**: No side effects, deterministic

**Usage:**
```csharp
// Manual bidirectional linking
room1.Exits[direction] = room2.Id;
room2.Exits[GetOppositeDirection(direction)] = room1.Id;
```

---

### Secondary Behaviors

#### 1. Environment Population Integration (v0.3.3c)

**Purpose:** Delegate hazard/condition spawning to specialized service

**Integration Point:**
```csharp
await _environmentPopulator.PopulateDungeonAsync(rooms.Values);
```

**EnvironmentPopulator Responsibilities:**
- Spawn `DynamicHazard` entities in rooms based on biome/danger level
- Apply `AmbientCondition` effects to rooms
- Add `InteractableObject` entities (containers, terminals, etc.)

**Timing:**
- Population occurs AFTER room creation and linking
- Population occurs BEFORE database persistence
- Allows populator to modify room entities before save

**See Also:** SPEC-ENVPOP-001 for environment population details

---

#### 2. Database Cleanup (Idempotent Generation)

**Purpose:** Ensure fresh dungeon state on each generation

**Implementation:**
```csharp
await _roomRepository.ClearAllRoomsAsync();
```

**Behavior:**
- Deletes ALL rooms from database (cascade deletes hazards, objects, etc.)
- Resets auto-increment counters (if applicable)
- Allows repeated calls to `GenerateTestMapAsync()` without duplication

**Safety:**
- **NO CONFIRMATION**: Destructive operation with no undo
- **NO VALIDATION**: Does not check if player is in dungeon
- **ADMIN OPERATION**: Should only be called during new game initialization

**Risk:**
- Calling during active gameplay will delete current dungeon and corrupt player state
- Future: Add safety check (`if (GameState.IsInGame) throw new InvalidOperationException()`)

---

### Edge Cases

#### 1. Repeated Generation (Multiple Calls)

**Scenario:** `GenerateTestMapAsync()` called multiple times

**Handling:**
```csharp
// First call
var startingRoomId1 = await _generator.GenerateTestMapAsync();

// Second call
var startingRoomId2 = await _generator.GenerateTestMapAsync();

// Result: startingRoomId1 != startingRoomId2 (new GUIDs generated)
```

**State Impact:**
- First dungeon completely deleted
- Second dungeon created with new GUIDs
- Player state must be re-initialized with new starting room ID

**Safety:** Calling during active game corrupts player location (CurrentRoomId points to deleted room)

---

#### 2. Database Failure During Persistence

**Scenario:** `AddRangeAsync()` or `SaveChangesAsync()` throws exception

**Handling:**
- Exception propagates to caller (no try/catch)
- Database transaction may be rolled back (depends on repository implementation)
- Rooms not persisted (dungeon incomplete)

**Recovery:**
- Caller must handle exception
- Retry `GenerateTestMapAsync()` (idempotent due to `ClearAllRoomsAsync()`)

---

#### 3. EnvironmentPopulator Failure (v0.3.3c)

**Scenario:** `PopulateDungeonAsync()` throws exception

**Handling:**
- Exception propagates before room persistence
- Rooms NOT saved to database (all-or-nothing)
- Dungeon generation aborted

**Recovery:**
- Fix populator issue
- Retry generation

**Logging:**
- No specific DungeonGenerator error logging (exception bubbles up)

---

#### 4. Concurrent Generation Calls

**Scenario:** Two threads call `GenerateTestMapAsync()` simultaneously

**Handling:**
- **NOT THREAD-SAFE**: Race condition on `ClearAllRoomsAsync()`
- Possible outcomes:
  - Duplicate room creation (if GUIDs collide - extremely unlikely)
  - Partial dungeon state (one thread's rooms, other thread's links)
- **UNDEFINED BEHAVIOR**: No synchronization primitives

**Recommendation:**
- Ensure single-threaded access
- Future: Add `lock` or `SemaphoreSlim` around generation

---

## Restrictions

### MUST Requirements

1. **MUST clear existing rooms before generation**
   - **Reason:** Prevents duplicate rooms and stale data
   - **Implementation:** DungeonGenerator.cs:47

2. **MUST create exactly 5 rooms** (test map specification)
   - **Reason:** Fixed test layout for QA validation
   - **Implementation:** DungeonGenerator.cs:50, rooms dictionary size

3. **MUST create bidirectional exits for all connections**
   - **Reason:** Navigation expects two-way traversal
   - **Implementation:** DungeonGenerator.cs:152-166

4. **MUST designate exactly 1 starting room** (`IsStartingRoom = true`)
   - **Reason:** Player spawn point must be unambiguous
   - **Implementation:** DungeonGenerator.cs:84

5. **MUST assign unique spatial coordinates** to each room
   - **Reason:** Coordinate-based dictionary lookup requires uniqueness
   - **Implementation:** DungeonGenerator.cs:74-134

6. **MUST persist rooms to database** before returning
   - **Reason:** NavigationService requires rooms in database
   - **Implementation:** DungeonGenerator.cs:59-60

7. **MUST return starting room ID** on success
   - **Reason:** GameState initialization requires CurrentRoomId
   - **Implementation:** DungeonGenerator.cs:62-66

8. **MUST populate environment content** (v0.3.3c)
   - **Reason:** Rooms require hazards/conditions for gameplay
   - **Implementation:** DungeonGenerator.cs:56

---

### MUST NOT Requirements

1. **MUST NOT create rooms with duplicate coordinates**
   - **Violation Impact:** Dictionary key collision, room overwrite
   - **Enforcement:** Dictionary key uniqueness (automatic)

2. **MUST NOT create rooms with duplicate IDs**
   - **Violation Impact:** Database primary key violation
   - **Enforcement:** Guid.NewGuid() generates unique IDs (statistically guaranteed)

3. **MUST NOT create unidirectional exits**
   - **Violation Impact:** Player can enter room but cannot return (softlock)
   - **Enforcement:** Manual `LinkRooms()` implementation ensures bidirectionality

4. **MUST NOT create exits pointing to non-existent rooms**
   - **Violation Impact:** NavigationService error ("path leads nowhere")
   - **Enforcement:** All exit target IDs reference rooms in the same generation batch

5. **MUST NOT skip environment population** (v0.3.3c)
   - **Violation Impact:** Rooms have no hazards/conditions (incomplete gameplay)
   - **Enforcement:** Required method call in `GenerateTestMapAsync()`

6. **MUST NOT persist rooms before environment population**
   - **Violation Impact:** Hazards/conditions not persisted (database incomplete)
   - **Enforcement:** Population call before `AddRangeAsync()` (line 56 → line 59)

---

## Limitations

### Numerical Limits

- **Room Count:** Fixed at 5 (hardcoded test map)
- **Starting Room Count:** Exactly 1
- **Exit Count Per Room:** 1-4 (Entry Hall has 4, all others have 1)
- **Spatial Dimensions:** 3D coordinates (X, Y, Z) using `int` range
- **Coordinate Range:** `int.MinValue` to `int.MaxValue` (not practically limited)

### Functional Limitations

1. **No Procedural Generation**
   - Current implementation is purely deterministic
   - Future: Wave Function Collapse, BSP tree partitioning, cellular automata
   - Placeholder design allows algorithm swapping without API changes

2. **No Room Customization**
   - Room names, descriptions, and positions are hardcoded
   - No configuration parameters (e.g., room count, biome type)
   - Future: Accept `DungeonConfig` parameter for procedural settings

3. **No Branching/Loops**
   - Current layout is star topology (Entry Hall connected to 4 leaf rooms)
   - No cycles (cannot traverse A → B → C → A)
   - Future: Graph algorithms for complex topologies

4. **No Room Archetypes**
   - All rooms have `IsStartingRoom` flag, but no other categorization
   - No boss rooms, treasure rooms, trap rooms, etc. (classification system)
   - Future: `RoomArchetype` enum (EntryHall, Corridor, Chamber, BossArena, etc.)

5. **No Biome Support**
   - Rooms do not have `BiomeType` assigned in generation
   - EnvironmentPopulator may infer biome from other properties
   - Future: Assign `BiomeType` during room creation for themed dungeons

6. **No Difficulty Scaling**
   - Rooms do not have `DangerLevel` assigned
   - No progression from safe → dangerous areas
   - Future: Distance-based difficulty scaling (further from start = harder)

7. **No Save/Load**
   - Generated dungeon is persisted to database but not versioned
   - No dungeon state snapshots
   - Future: Dungeon serialization for procedural seed replay

---

### System-Specific Limitations

1. **Not Thread-Safe**
   - Concurrent calls to `GenerateTestMapAsync()` cause race conditions
   - No synchronization primitives
   - Future: Add mutex or ensure single-threaded access

2. **No Validation**
   - Does not validate room data (e.g., empty names, null descriptions)
   - Assumes hardcoded data is valid
   - Future: Add validation pass before persistence

3. **No Rollback on Failure**
   - If `EnvironmentPopulator` fails, rooms are not persisted (good)
   - If database save fails, no cleanup occurs (potentially bad)
   - Future: Explicit transaction management

---

## Use Cases

### USE CASE 1: New Game Initialization

**Setup:**
```csharp
// New game started, no existing dungeon
_database.Rooms.Count == 0;
```

**Execution:**
```csharp
var startingRoomId = await _dungeonGenerator.GenerateTestMapAsync();
_gameState.CurrentRoomId = startingRoomId;
```

**Internal Flow:**

1. **Cleanup**: `ClearAllRoomsAsync()` → No-op (already empty)
2. **Room Creation**: `CreateTestRooms()` → 5 rooms with unique GUIDs
3. **Exit Linking**: `LinkRooms()` → 8 exit references (4 bidirectional)
4. **Environment Population**: `PopulateDungeonAsync()` → Hazards/conditions added
5. **Persistence**: `AddRangeAsync()` + `SaveChangesAsync()` → Database write
6. **Return**: Starting room GUID (Entry Hall ID)

**Database State After:**
```
Rooms Table:
- Entry Hall (IsStartingRoom=true, Exits=4)
- Rusted Corridor (Exits=1)
- Storage Chamber (Exits=1)
- Collapsed Tunnel (Exits=1)
- The Pit (Exits=1)
```

**Assertions:**
- `_database.Rooms.Count == 5`
- `_database.Rooms.Single(r => r.IsStartingRoom).Id == startingRoomId`
- All exits are bidirectional (verified by test)

**Test Reference:** DungeonGeneratorTests.cs:50-66

---

### USE CASE 2: Dungeon Regeneration (Admin Command)

**Setup:**
```csharp
// Existing dungeon present
_database.Rooms.Count == 5;
var oldStartingRoomId = _database.Rooms.Single(r => r.IsStartingRoom).Id;
```

**Execution:**
```csharp
var newStartingRoomId = await _dungeonGenerator.GenerateTestMapAsync();
```

**Internal Flow:**

1. **Cleanup**: `ClearAllRoomsAsync()` → **Deletes all 5 existing rooms**
2. **Room Creation**: `CreateTestRooms()` → 5 NEW rooms with NEW GUIDs
3. **Linking, Population, Persistence**: Standard
4. **Return**: NEW starting room GUID

**Database State After:**
```
Rooms Table:
- Entry Hall (NEW GUID, IsStartingRoom=true)
- Rusted Corridor (NEW GUID)
- Storage Chamber (NEW GUID)
- Collapsed Tunnel (NEW GUID)
- The Pit (NEW GUID)
```

**Assertions:**
- `_database.Rooms.Count == 5` (same count, different instances)
- `newStartingRoomId != oldStartingRoomId` (GUIDs regenerated)
- **CRITICAL**: Player's `CurrentRoomId` is now INVALID (points to deleted room)

**Recovery Required:**
```csharp
_gameState.CurrentRoomId = newStartingRoomId;
_gameState.VisitedRoomIds.Clear();
_gameState.TurnCount = 0;
```

**Test Reference:** DungeonGeneratorTests.cs:50-56 (cleanup verification)

---

### USE CASE 3: Bidirectional Exit Verification

**Setup:**
```csharp
var startingRoomId = await _dungeonGenerator.GenerateTestMapAsync();
```

**Execution:**
```csharp
// Load Entry Hall
var entry = await _roomRepository.GetByIdAsync(startingRoomId);

// Load Corridor via North exit
var corridorId = entry.Exits[Direction.North];
var corridor = await _roomRepository.GetByIdAsync(corridorId);

// Verify reverse exit
var entryIdFromCorridor = corridor.Exits[Direction.South];
```

**Assertions:**
- `entry.Exits[Direction.North] == corridor.Id` ✓
- `corridor.Exits[Direction.South] == entry.Id` ✓
- Bidirectional integrity verified

**Test Reference:** DungeonGeneratorTests.cs:171-187

---

### USE CASE 4: Spatial Coordinate Layout Validation

**Setup:**
```csharp
await _dungeonGenerator.GenerateTestMapAsync();
var rooms = await _roomRepository.GetAllAsync();
```

**Execution:**
```csharp
var entry = rooms.Single(r => r.Name == "Entry Hall");
var corridor = rooms.Single(r => r.Name == "Rusted Corridor");
var pit = rooms.Single(r => r.Name == "The Pit");
```

**Coordinate Assertions:**
- `entry.Position == Coordinate.Origin` (0,0,0) ✓
- `corridor.Position == new Coordinate(0,1,0)` (North of entry) ✓
- `pit.Position == new Coordinate(0,0,-1)` (Below entry, Z=-1) ✓

**Manhattan Distance:**
```csharp
entry.Position.ManhattanDistance(corridor.Position) == 1; // Adjacent
entry.Position.ManhattanDistance(pit.Position) == 1;      // Vertically adjacent
```

**Test Reference:** DungeonGeneratorTests.cs:279-300

---

### USE CASE 5: Environment Population Integration (v0.3.3c)

**Setup:**
```csharp
// Mock environment populator
_mockEnvironmentPopulator
    .Setup(e => e.PopulateDungeonAsync(It.IsAny<IEnumerable<Room>>()))
    .Callback<IEnumerable<Room>>(rooms =>
    {
        // Add hazard to Collapsed Tunnel
        var collapsed = rooms.Single(r => r.Name == "Collapsed Tunnel");
        collapsed.Hazards.Add(new DynamicHazard { Name = "Falling Rubble" });
    });
```

**Execution:**
```csharp
await _dungeonGenerator.GenerateTestMapAsync();
var collapsed = await _roomRepository.GetByNameAsync("Collapsed Tunnel");
```

**Assertions:**
- `collapsed.Hazards.Count > 0` ✓
- `collapsed.Hazards.Any(h => h.Name == "Falling Rubble")` ✓
- Environment populator was invoked BEFORE persistence

**Test Reference:** Integration test in EnvironmentPopulatorTests.cs

---

### USE CASE 6: Starting Room ID Return

**Execution:**
```csharp
var returnedId = await _dungeonGenerator.GenerateTestMapAsync();
var rooms = await _roomRepository.GetAllAsync();
var startingRoom = rooms.Single(r => r.IsStartingRoom);
```

**Assertions:**
- `returnedId == startingRoom.Id` ✓
- `startingRoom.Name == "Entry Hall"` ✓
- `startingRoom.Position == Coordinate.Origin` ✓

**Usage:**
```csharp
// Initialize game state with returned ID
_gameState.CurrentRoomId = returnedId;
_gameState.VisitedRoomIds.Add(returnedId);
```

**Test Reference:** DungeonGeneratorTests.cs:101-109

---

### USE CASE 7: Opposite Direction Utility

**Execution:**
```csharp
var opposite = DungeonGenerator.GetOppositeDirection(Direction.North);
```

**Assertions:**
- `opposite == Direction.South` ✓

**Symmetry Test:**
```csharp
foreach (var direction in Enum.GetValues<Direction>())
{
    var opp = DungeonGenerator.GetOppositeDirection(direction);
    var backToOriginal = DungeonGenerator.GetOppositeDirection(opp);
    Assert.Equal(direction, backToOriginal);
}
```

**Test Reference:** DungeonGeneratorTests.cs:232-262

---

## Decision Trees

### Decision Tree 1: Test Map Generation Flow

```
GenerateTestMapAsync() INVOKED
│
├─ STEP 1: Database Cleanup
│  └─ ClearAllRoomsAsync() → DELETE all rooms
│
├─ STEP 2: Room Creation
│  └─ CreateTestRooms()
│     ├─ Create Entry Hall (0,0,0) → IsStartingRoom=true
│     ├─ Create Rusted Corridor (0,1,0)
│     ├─ Create Storage Chamber (1,0,0)
│     ├─ Create Collapsed Tunnel (-1,0,0)
│     └─ Create The Pit (0,0,-1)
│
├─ STEP 3: Exit Linking
│  └─ LinkRooms(rooms)
│     ├─ Entry ↔ Corridor (North/South)
│     ├─ Entry ↔ Storage (East/West)
│     ├─ Entry ↔ Collapsed (West/East)
│     └─ Entry ↔ Pit (Down/Up)
│
├─ STEP 4: Environment Population (v0.3.3c)
│  └─ PopulateDungeonAsync(rooms)
│     ├─ Add DynamicHazards
│     ├─ Add AmbientConditions
│     └─ Add InteractableObjects
│
├─ STEP 5: Database Persistence
│  ├─ AddRangeAsync(rooms) → INSERT 5 rooms
│  └─ SaveChangesAsync() → COMMIT transaction
│
└─ STEP 6: Return Starting Room ID
   └─ RETURN Entry Hall GUID
```

**Error Paths:**
- **Database Failure**: Exception at Step 5 → No persistence, generation aborted
- **Population Failure**: Exception at Step 4 → No persistence, generation aborted

---

### Decision Tree 2: Bidirectional Exit Linking

```
LinkRooms(rooms) INVOKED
│
├─ LINK: Entry Hall ↔ Rusted Corridor
│  ├─ entry.Exits[Direction.North] = corridor.Id
│  └─ corridor.Exits[Direction.South] = entry.Id
│
├─ LINK: Entry Hall ↔ Storage Chamber
│  ├─ entry.Exits[Direction.East] = storage.Id
│  └─ storage.Exits[Direction.West] = entry.Id
│
├─ LINK: Entry Hall ↔ Collapsed Tunnel
│  ├─ entry.Exits[Direction.West] = collapsed.Id
│  └─ collapsed.Exits[Direction.East] = entry.Id
│
└─ LINK: Entry Hall ↔ The Pit
   ├─ entry.Exits[Direction.Down] = pit.Id
   └─ pit.Exits[Direction.Up] = entry.Id

RESULT:
- Entry Hall: 4 exits
- Other Rooms: 1 exit each
- Total: 8 exit references (4 bidirectional pairs)
```

**Invariant:** For every `roomA.Exits[dir] = roomB.Id`, there exists `roomB.Exits[opposite(dir)] = roomA.Id`

---

## Sequence Diagrams

### Sequence Diagram 1: Complete Test Map Generation

```
Client        DungeonGenerator   RoomRepository   EnvironmentPopulator   Database
  |                  |                  |                    |                |
  |-- GenerateTestMapAsync() ---------->|                    |                |
  |                  |                  |                    |                |
  |                  |-- ClearAllRoomsAsync() -------------->|                |
  |                  |                  |                    |                |
  |                  |                  |-- DELETE FROM Rooms --------------->|
  |                  |                  |<-- Success -----------------------  |
  |                  |<-- Done ---------|                    |                |
  |                  |                  |                    |                |
  |                  |-- CreateTestRooms() (internal)                         |
  |                  |   [Generate 5 Room objects with GUIDs]                 |
  |                  |                  |                    |                |
  |                  |-- LinkRooms() (internal)                               |
  |                  |   [Populate Exits dictionaries]                        |
  |                  |                  |                    |                |
  |                  |-- PopulateDungeonAsync(rooms) -------->|                |
  |                  |                  |                    |                |
  |                  |                  |          [Add hazards/conditions]   |
  |                  |                  |          [Modify room entities]     |
  |                  |                  |                    |                |
  |                  |<-- Done (rooms modified) -------------|                |
  |                  |                  |                    |                |
  |                  |-- AddRangeAsync(rooms) -------------->|                |
  |                  |                  |                    |                |
  |                  |                  |-- INSERT INTO Rooms (5 rows) ------>|
  |                  |                  |<-- Success -----------------------  |
  |                  |<-- Done ---------|                    |                |
  |                  |                  |                    |                |
  |                  |-- SaveChangesAsync() ---------------->|                |
  |                  |                  |-- COMMIT -------------------------->|
  |                  |                  |<-- Success -----------------------  |
  |                  |<-- Done ---------|                    |                |
  |                  |                  |                    |                |
  |                  |-- Find Starting Room (Entry Hall) --                   |
  |                  |                  |                    |                |
  |<-- startingRoomId (GUID) -----------|                    |                |
```

**Timing:**
- Total execution: ~200-500ms (database cleanup + 5 inserts + environment population)
- Largest time sink: Environment population (procedural spawning)

---

### Sequence Diagram 2: Failed Generation (Environment Populator Exception)

```
Client        DungeonGenerator   RoomRepository   EnvironmentPopulator
  |                  |                  |                    |
  |-- GenerateTestMapAsync() ---------->|                    |
  |                  |                  |                    |
  |                  |-- ClearAllRoomsAsync() -------------->|
  |                  |<-- Done ---------|                    |
  |                  |                  |                    |
  |                  |-- CreateTestRooms() + LinkRooms()     |
  |                  |                  |                    |
  |                  |-- PopulateDungeonAsync(rooms) -------->|
  |                  |                  |                    |
  |                  |                  |          [EXCEPTION THROWN]
  |                  |                  |                    |
  |                  |<-- EXCEPTION --------------------------
  |                  |                  |                    |
  |<-- EXCEPTION -----|                    [No persistence occurs]
  |                  |                  |                    |
  [Caller handles exception]   [Rooms NOT in database]
```

**Result:**
- Database remains empty (cleanup occurred, but persistence never reached)
- Idempotent: Retry will work (cleanup will no-op on empty database)

---

## Workflows

### Workflow 1: New Game Dungeon Setup

**Purpose:** Initialize game world with test dungeon

**Steps:**

1. ☐ **Call DungeonGenerator**
   ```csharp
   var startingRoomId = await _dungeonGenerator.GenerateTestMapAsync();
   ```

2. ☐ **Verify 5 Rooms Created**
   ```csharp
   var rooms = await _roomRepository.GetAllAsync();
   Debug.Assert(rooms.Count == 5);
   ```

3. ☐ **Initialize GameState**
   ```csharp
   _gameState.CurrentRoomId = startingRoomId;
   _gameState.VisitedRoomIds.Add(startingRoomId);
   _gameState.TurnCount = 0;
   ```

4. ☐ **Load Starting Room Description**
   ```csharp
   var description = await _navigationService.LookAsync();
   _inputHandler.DisplayMessage(description);
   ```

5. ☐ **Save Initial Game State**
   ```csharp
   await _saveManager.SaveGameAsync();
   ```

**Validation Points:**
- After step 1: Verify `startingRoomId != Guid.Empty`
- After step 2: Verify starting room has `IsStartingRoom == true`
- After step 3: Verify `CurrentRoomId` is valid
- After step 4: Verify description contains "Entry Hall"

---

### Workflow 2: Dungeon Regeneration (Admin)

**Purpose:** Clear and regenerate dungeon (DESTRUCTIVE)

**Steps:**

1. ☐ **Confirm Regeneration** (User Prompt)
   - Display: "This will DELETE the current dungeon. Continue? (Y/N)"
   - If NO: Abort

2. ☐ **Backup Current Save** (Optional)
   ```csharp
   await _saveManager.CreateBackupAsync();
   ```

3. ☐ **Generate New Dungeon**
   ```csharp
   var newStartingRoomId = await _dungeonGenerator.GenerateTestMapAsync();
   ```

4. ☐ **Reset Player State**
   ```csharp
   _gameState.CurrentRoomId = newStartingRoomId;
   _gameState.VisitedRoomIds.Clear();
   _gameState.VisitedRoomIds.Add(newStartingRoomId);
   _gameState.TurnCount = 0;
   ```

5. ☐ **Clear Temporary State**
   ```csharp
   _combatService.ClearActiveCombat();
   _hazardService.ClearActiveHazards();
   ```

6. ☐ **Display Confirmation**
   ```csharp
   _inputHandler.DisplayMessage("Dungeon regenerated. You awaken in a new configuration...");
   ```

**Validation Points:**
- After step 3: Verify `newStartingRoomId` is different from previous ID
- After step 4: Verify `CurrentRoomId` is valid
- After step 5: Verify no orphaned combat/hazard state

---

## Cross-System Integration

### Integration Matrix

| System | Dependency Type | Integration Points | Data Flow |
|--------|----------------|-------------------|-----------|
| **RoomRepository** | Required | `ClearAllRoomsAsync()`, `AddRangeAsync()`, `SaveChangesAsync()` | DungeonGenerator → RoomRepository (room persistence) |
| **EnvironmentPopulator** | Required | `PopulateDungeonAsync()` | DungeonGenerator → EnvironmentPopulator (hazard/condition spawning) |
| **NavigationService** | Consumer | Room data retrieval | DungeonGenerator → RoomRepository → NavigationService |
| **GameService** | Consumer | `GenerateTestMapAsync()` on new game | GameService → DungeonGenerator |
| **SaveManager** | Observer | Dungeon state persistence | DungeonGenerator → RoomRepository → SaveManager |

---

### Integration Details

#### 1. RoomRepository Integration

**Interface:** `IRoomRepository`

**Methods Used:**
- `ClearAllRoomsAsync()` - Delete all existing rooms
- `AddRangeAsync(IEnumerable<Room> rooms)` - Bulk insert 5 rooms
- `SaveChangesAsync()` - Commit transaction

**Data Flow:**
```
DungeonGenerator.GenerateTestMapAsync()
  └→ RoomRepository.ClearAllRoomsAsync()
      └→ Database: DELETE FROM Rooms
  └→ RoomRepository.AddRangeAsync(5 rooms)
      └→ Database: INSERT INTO Rooms (5 rows)
  └→ RoomRepository.SaveChangesAsync()
      └→ Database: COMMIT
```

**Room Entity Fields Populated:**
- `Id` (Guid) - Auto-generated
- `Name` (string) - Hardcoded room names
- `Description` (string) - Hardcoded descriptions
- `Position` (Coordinate) - Hardcoded spatial positions
- `Exits` (Dictionary<Direction, Guid>) - Populated by `LinkRooms()`
- `IsStartingRoom` (bool) - True for Entry Hall only
- `Hazards` (List<DynamicHazard>) - Populated by EnvironmentPopulator
- `Objects` (List<InteractableObject>) - Populated by EnvironmentPopulator

---

#### 2. EnvironmentPopulator Integration (v0.3.3c)

**Interface:** `IEnvironmentPopulator`

**Method Used:**
- `PopulateDungeonAsync(IEnumerable<Room> rooms)`

**Data Flow:**
```
DungeonGenerator.GenerateTestMapAsync()
  └→ CreateTestRooms() + LinkRooms() [Room entities in memory]
  └→ EnvironmentPopulator.PopulateDungeonAsync(rooms)
      └→ For each room:
          ├→ Add DynamicHazards (pressure plates, falling rubble, etc.)
          ├→ Add AmbientConditions (toxic gas, extreme cold, etc.)
          └→ Add InteractableObjects (containers, terminals, etc.)
  └→ RoomRepository.AddRangeAsync(rooms) [Now includes hazards/objects]
```

**Timing:**
- Population occurs AFTER room creation and linking
- Population occurs BEFORE database persistence
- Allows populator to modify room entities before save

**See Also:** SPEC-ENVPOP-001 for environment population details

---

#### 3. GameService Integration

**Usage:** GameService calls DungeonGenerator on new game initialization

**Pattern:**
```csharp
// GameService.StartNewGameAsync()
var startingRoomId = await _dungeonGenerator.GenerateTestMapAsync();
_gameState.CurrentRoomId = startingRoomId;
_gameState.VisitedRoomIds.Add(startingRoomId);
_gameState.TurnCount = 0;
```

**Responsibility:**
- GameService owns game state initialization
- DungeonGenerator owns room creation and linking

---

## Data Models

### Room Entity

**Source:** `RuneAndRust.Core.Entities.Room`

```csharp
public class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Coordinate Position { get; set; } = Coordinate.Origin;
    public Dictionary<Direction, Guid> Exits { get; set; } = new();

    public bool IsStartingRoom { get; set; }
    public bool IsBossRoom { get; set; }

    public BiomeType BiomeType { get; set; } // Not currently set by DungeonGenerator
    public DangerLevel DangerLevel { get; set; } // Not currently set by DungeonGenerator

    // Populated by EnvironmentPopulator
    public List<DynamicHazard> Hazards { get; set; } = new();
    public List<InteractableObject> Objects { get; set; } = new();
}
```

**Fields Set by DungeonGenerator:**
- `Id` - Auto-generated GUID
- `Name` - Hardcoded room name
- `Description` - Hardcoded description
- `Position` - Hardcoded coordinate
- `Exits` - Populated by `LinkRooms()`
- `IsStartingRoom` - True for Entry Hall

**Fields NOT Set by DungeonGenerator:**
- `BiomeType` - Defaults to 0 (future feature)
- `DangerLevel` - Defaults to 0 (future feature)
- `IsBossRoom` - Defaults to false
- `Hazards` - Populated by EnvironmentPopulator
- `Objects` - Populated by EnvironmentPopulator

---

### Coordinate Value Object

**Source:** `RuneAndRust.Core.ValueObjects.Coordinate`

```csharp
public record Coordinate(int X, int Y, int Z)
{
    public static readonly Coordinate Origin = new(0, 0, 0);

    public int ManhattanDistance(Coordinate other)
    {
        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z);
    }
}
```

**Usage in DungeonGenerator:**
- Dictionary key for room lookup during creation
- Spatial positioning for future pathfinding/distance calculations

**Coordinate Assignments:**
- Entry Hall: `(0, 0, 0)` - Origin
- Rusted Corridor: `(0, 1, 0)` - North (+Y)
- Storage Chamber: `(1, 0, 0)` - East (+X)
- Collapsed Tunnel: `(-1, 0, 0)` - West (-X)
- The Pit: `(0, 0, -1)` - Down (-Z)

---

### Direction Enum

**Source:** `RuneAndRust.Core.Enums.Direction`

```csharp
public enum Direction
{
    North = 0,
    South = 1,
    East = 2,
    West = 3,
    Up = 4,
    Down = 5
}
```

**Usage in DungeonGenerator:**
- Dictionary keys for `Room.Exits`
- `GetOppositeDirection()` utility method

**Opposite Pairs:**
- North ↔ South
- East ↔ West
- Up ↔ Down

---

## Configuration

### Constants (Hardcoded in Implementation)

**Room Count:**
```csharp
private const int TestMapRoomCount = 5;
```

**Starting Room Position:**
```csharp
private static readonly Coordinate StartingRoomPosition = Coordinate.Origin; // (0,0,0)
```

**Room Names (Hardcoded):**
```csharp
"Entry Hall"
"Rusted Corridor"
"Storage Chamber"
"Collapsed Tunnel"
"The Pit"
```

---

### Future Configuration (Procedural Generation)

**Planned Configuration Model:**
```csharp
public class DungeonConfig
{
    public int RoomCount { get; set; } = 10;
    public BiomeType PrimaryBiome { get; set; } = BiomeType.Industrial;
    public DangerLevel AverageDifficulty { get; set; } = DangerLevel.Moderate;
    public bool IncludeBossRoom { get; set; } = true;
    public int BranchingFactor { get; set; } = 2; // Paths from each junction
    public GenerationAlgorithm Algorithm { get; set; } = GenerationAlgorithm.WaveFunctionCollapse;
}
```

**Usage:**
```csharp
var startingRoomId = await _dungeonGenerator.GenerateProceduralDungeonAsync(config);
```

---

## Testing

### Test Summary

**Source:** `RuneAndRust.Tests/Engine/DungeonGeneratorTests.cs` (304 lines)

**Test Count:** 20 tests

**Coverage Breakdown:**
- `GenerateTestMapAsync()`: 15 tests
- `GetOppositeDirection()`: 2 tests
- Expected Layout Validation: 3 tests

**Coverage Percentage:** ~100% (all public methods, all invariants)

---

### Test Categories

#### 1. GenerateTestMapAsync() Tests (15 tests)

**DungeonGeneratorTests.cs:50-56** - `GenerateTestMapAsync_ClearsExistingRooms`
- Validates database cleanup
- Asserts `ClearAllRoomsAsync()` called exactly once

**DungeonGeneratorTests.cs:70-77** - `GenerateTestMapAsync_CreatesFiveRooms`
- Validates fixed room count
- Asserts `_addedRooms.Count == 5`

**DungeonGeneratorTests.cs:80-87** - `GenerateTestMapAsync_HasExactlyOneStartingRoom`
- Validates single starting room
- Asserts `_addedRooms.Count(r => r.IsStartingRoom) == 1`

**DungeonGeneratorTests.cs:90-98** - `GenerateTestMapAsync_StartingRoomIsAtOrigin`
- Validates starting room position
- Asserts `startingRoom.Position == Coordinate.Origin`

**DungeonGeneratorTests.cs:101-109** - `GenerateTestMapAsync_ReturnsStartingRoomId`
- Validates return value
- Asserts `result == startingRoom.Id`

**DungeonGeneratorTests.cs:112-120** - `GenerateTestMapAsync_AllRoomsHaveUniqueIds`
- Validates GUID uniqueness
- Asserts `ids.Should().OnlyHaveUniqueItems()`

**DungeonGeneratorTests.cs:123-131** - `GenerateTestMapAsync_AllRoomsHaveUniquePositions`
- Validates coordinate uniqueness
- Asserts `positions.Should().OnlyHaveUniqueItems()`

**DungeonGeneratorTests.cs:134-144** - `GenerateTestMapAsync_AllRoomsHaveNonEmptyNames`
- Validates room names
- Asserts all names are non-empty strings

**DungeonGeneratorTests.cs:147-157** - `GenerateTestMapAsync_AllRoomsHaveNonEmptyDescriptions`
- Validates room descriptions
- Asserts all descriptions are non-empty strings

**DungeonGeneratorTests.cs:160-168** - `GenerateTestMapAsync_StartingRoomHasMultipleExits`
- Validates starting room connectivity
- Asserts `startingRoom.Exits.Count >= 2` (actually 4)

**DungeonGeneratorTests.cs:171-187** - `GenerateTestMapAsync_ExitsAreBidirectional`
- **CRITICAL TEST**: Validates bidirectionality invariant
- For each room's exit, asserts target room has reverse exit

**DungeonGeneratorTests.cs:190-205** - `GenerateTestMapAsync_ExitsPointToValidRooms`
- Validates exit integrity
- Asserts all exit target IDs reference rooms in the batch

**DungeonGeneratorTests.cs:208-216** - `GenerateTestMapAsync_SavesChangesToDatabase`
- Validates persistence
- Asserts `AddRangeAsync()` and `SaveChangesAsync()` called once each

**DungeonGeneratorTests.cs:219-227** - `GenerateTestMapAsync_IncludesVerticalRoom`
- Validates 3D layout
- Asserts at least one room has `Z < 0` (The Pit)

---

#### 2. GetOppositeDirection() Tests (2 tests)

**DungeonGeneratorTests.cs:233-247** - `GetOppositeDirection_ReturnsCorrectOpposite`
- Parameterized test for all 6 directions
- Asserts correct opposite direction returned

**DungeonGeneratorTests.cs:250-262** - `GetOppositeDirection_IsSymmetric`
- Validates symmetry property
- Asserts `opposite(opposite(x)) == x` for all directions

---

#### 3. Expected Layout Tests (3 tests)

**DungeonGeneratorTests.cs:269-276** - `GenerateTestMapAsync_EntryHallIsNamed`
- Validates specific room name
- Asserts "Entry Hall" room exists

**DungeonGeneratorTests.cs:279-288** - `GenerateTestMapAsync_RustedCorridorIsNorthOfEntry`
- Validates specific room position
- Asserts Rusted Corridor at `(0,1,0)`

**DungeonGeneratorTests.cs:291-300** - `GenerateTestMapAsync_ThePitIsBelowEntry`
- Validates vertical positioning
- Asserts The Pit at `(0,0,-1)`

---

### Mock Setup Patterns

**Room Repository Mock:**
```csharp
_mockRoomRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Room>>()))
    .Callback<IEnumerable<Room>>(rooms => _addedRooms = rooms.ToList())
    .Returns(Task.CompletedTask);

_mockRoomRepo.Setup(r => r.ClearAllRoomsAsync())
    .Returns(Task.CompletedTask);

_mockRoomRepo.Setup(r => r.SaveChangesAsync())
    .Returns(Task.CompletedTask);
```

**Callback Pattern:**
- Captures rooms passed to `AddRangeAsync()` for assertions
- Allows test validation without actual database

---

## Domain 4 Compliance

### Validation Status

**DungeonGenerator.cs:** ✅ **COMPLIANT**

All room names and descriptions comply with Domain 4 constraints:

**Room Descriptions Analysis:**

1. **Entry Hall:**
   - ✅ "cold, metallic chamber" (qualitative)
   - ✅ "smells of ozone and ancient dust" (sensory, not measured)
   - ✅ "Faded runes pulse weakly" (qualitative intensity)

2. **Rusted Corridor:**
   - ✅ "Corroded pipes" (descriptive state)
   - ✅ "Water drips from unseen sources" (observational)
   - ✅ "rust-red stains" (qualitative color)

3. **Storage Chamber:**
   - ✅ "Broken crates and shattered containers" (state description)
   - ✅ "looted long ago or claimed by decay" (temporal vagueness)
   - ✅ "Dust motes drift in the pale light" (observational)

4. **Collapsed Tunnel:**
   - ✅ "Rubble partially blocks" (qualitative obstruction)
   - ✅ "ceiling groans ominously" (sensory, threatening)
   - ✅ "This area seems unstable" (subjective assessment)

5. **The Pit:**
   - ✅ "deep shaft descends into absolute darkness" (qualitative depth)
   - ✅ "Ancient machinery clings to the walls" (observational)
   - ✅ "echoes of your footsteps seem to go on forever" (subjective experience)

**No Forbidden Patterns:**
- ❌ No precision measurements ("5 meters wide", "23°C")
- ❌ No technical jargon ("structural integrity at 47%", "oxygen levels low")
- ❌ No modern units ("15 kilograms of debris")

---

## Future Extensions

### Planned Enhancements

#### 1. Procedural Generation Algorithms

**Requirement:** Replace hardcoded test map with algorithmic generation

**Planned Algorithms:**

**Wave Function Collapse (WFC):**
```csharp
public async Task<Guid> GenerateWFCDungeonAsync(DungeonConfig config)
{
    var grid = new WFCGrid(config.Width, config.Height);
    var tiles = LoadTileSet(config.TileSetName);

    while (!grid.IsFullyCollapsed())
    {
        var cell = grid.FindLowestEntropyCell();
        grid.CollapseCell(cell, tiles);
        grid.PropagateConstraints(cell);
    }

    var rooms = ConvertGridToRooms(grid);
    await PersistRooms(rooms);
    return rooms.First(r => r.IsStartingRoom).Id;
}
```

**Binary Space Partitioning (BSP):**
```csharp
public async Task<Guid> GenerateBSPDungeonAsync(DungeonConfig config)
{
    var root = new BSPNode(new Rect(0, 0, config.Width, config.Height));
    root.Split(config.MinRoomSize, config.MaxDepth);

    var rooms = root.CreateRooms();
    var corridors = root.CreateCorridors();

    await PersistRooms(rooms.Concat(corridors));
    return rooms.First().Id;
}
```

---

#### 2. Biome-Based Generation

**Requirement:** Assign BiomeType during room creation

**Extension Point:**
```csharp
private Dictionary<Coordinate, Room> CreateProceduralRooms(DungeonConfig config)
{
    var rooms = new Dictionary<Coordinate, Room>();

    foreach (var coordinate in GenerateLayout(config))
    {
        var room = new Room
        {
            Name = GenerateRoomName(config.PrimaryBiome),
            Description = GenerateRoomDescription(config.PrimaryBiome),
            Position = coordinate,
            BiomeType = config.PrimaryBiome, // ASSIGN BIOME
            DangerLevel = CalculateDangerLevel(coordinate, config)
        };
        rooms[coordinate] = room;
    }

    return rooms;
}
```

---

#### 3. Difficulty Progression

**Requirement:** Scale DangerLevel based on distance from start

**Extension Point:**
```csharp
private DangerLevel CalculateDangerLevel(Coordinate position, DungeonConfig config)
{
    var distanceFromStart = position.ManhattanDistance(Coordinate.Origin);

    // Linear scaling: 0-2 = Safe, 3-5 = Moderate, 6-8 = Dangerous, 9+ = Lethal
    return distanceFromStart switch
    {
        <= 2 => DangerLevel.Safe,
        <= 5 => DangerLevel.Moderate,
        <= 8 => DangerLevel.Dangerous,
        _ => DangerLevel.Lethal
    };
}
```

---

#### 4. Room Archetype System

**Requirement:** Categorize rooms by function (entry, corridor, chamber, boss, treasure)

**Extension Point:**
```csharp
public enum RoomArchetype
{
    EntryHall,
    Corridor,
    Chamber,
    BossArena,
    TreasureRoom,
    TrapRoom,
    SecretRoom
}

// Assign during generation
room.Archetype = DetermineArchetype(position, config);
```

---

#### 5. Seed-Based Regeneration

**Requirement:** Allow dungeon replay via seed value

**Extension Point:**
```csharp
public async Task<Guid> GenerateSeededDungeonAsync(int seed, DungeonConfig config)
{
    var rng = new Random(seed);
    // Use rng for all randomization
    var rooms = GenerateRooms(config, rng);
    await PersistRooms(rooms);
    return rooms.First(r => r.IsStartingRoom).Id;
}
```

**Storage:**
```csharp
public class SaveGame
{
    public int DungeonSeed { get; set; }
    // ...
}
```

---

## Changelog

### v0.3.3c - Environment Population Integration (2025-12-16)
- **ADDED**: `IEnvironmentPopulator` dependency injection
- **ADDED**: `PopulateDungeonAsync()` call before room persistence
- **UPDATED**: Room entities now include hazards/conditions/objects before save

### v0.0.5 - Initial Test Map Implementation (2025-11-22)
- **ADDED**: `GenerateTestMapAsync()` for deterministic 5-room layout
- **ADDED**: `CreateTestRooms()` internal method with hardcoded room definitions
- **ADDED**: `LinkRooms()` internal method for bidirectional exit creation
- **ADDED**: `GetOppositeDirection()` static utility method
- **ADDED**: Database cleanup via `ClearAllRoomsAsync()`
- **ADDED**: Spatial positioning using `Coordinate` value objects

---

## Appendix

### Related Specifications

- **SPEC-NAV-001**: Navigation System (consumes dungeon structure)
- **SPEC-ENVPOP-001**: Environment Population (populates rooms with content)
- **SPEC-ROOMGEN-001**: Dynamic Room Engine (future procedural system)

---

### Code References

**Primary Implementation:**
- `RuneAndRust.Engine/Services/DungeonGenerator.cs` (190 lines)

**Tests:**
- `RuneAndRust.Tests/Engine/DungeonGeneratorTests.cs` (304 lines, 20 tests)

**Data Models:**
- `RuneAndRust.Core/Entities/Room.cs`
- `RuneAndRust.Core/ValueObjects/Coordinate.cs`
- `RuneAndRust.Core/Enums/Direction.cs`

---

### Glossary

**Test Map:** Deterministic 5-room dungeon layout used for QA validation before procedural generation implementation

**Bidirectional Exit:** Two-way connection between rooms (Room A → Room B AND Room B → Room A)

**Spatial Coordinate:** 3D position (X, Y, Z) assigned to each room for layout representation

**Starting Room:** Room marked with `IsStartingRoom = true` where player spawns on game start

**Opposite Direction:** Reverse direction for bidirectional linking (North ↔ South, East ↔ West, Up ↔ Down)

**Environment Population:** Process of adding hazards, conditions, and objects to rooms after creation

**Idempotent Generation:** Property that allows repeated calls to `GenerateTestMapAsync()` without side effects (cleanup ensures fresh state)

---

**END OF SPECIFICATION**
