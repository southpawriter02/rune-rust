# SPEC-NAV-001: Navigation System

**Version:** 1.0.1
**Status:** Active (v0.3.3a - v0.3.3b)
**Last Updated:** 2025-12-25
**Owner:** Engine Team
**Category:** Navigation & Exploration
**Related Specs:** SPEC-HAZARD-001, SPEC-COND-001, SPEC-INTERACT-001, SPEC-DICE-001

---

## Overview

The **Navigation System** provides room-to-room movement mechanics for the player, managing directional travel through dungeon spaces with exit validation, state tracking, and automatic hazard/condition triggers. The system enforces spatial integrity through the Exit Dictionary architecture while maintaining a persistent visited-room registry for fog-of-war mechanics.

### Core Design Principles

1. **Atomic State Updates**: Room transitions complete fully or fail entirely—no partial state corruption
2. **Exit Validation Chain**: Multi-stage validation (current room exists → exit exists → destination exists) prevents invalid states
3. **Automatic Trigger Resolution**: Movement triggers hazards and ambient conditions on room entry (v0.3.3a/b)
4. **Turn Tracking**: Every successful movement increments global turn count for resource decay/event systems
5. **Bidirectional Exit Integrity**: Exits are unidirectional by default; bidirectionality enforced at dungeon generation time

### System Boundaries

**IN SCOPE:**
- Directional movement between rooms (6 cardinal directions including vertical)
- Exit validation and error handling
- Room description formatting with exit display
- Visited room tracking for fog-of-war systems
- Movement hazard triggering (pressure plates, tripwires, etc.)
- Ambient condition display on room entry

**OUT OF SCOPE:**
- Exit unlocking/locking mechanics (handled by InteractionService)
- Room generation (handled by DungeonGenerator)
- Combat encounter triggering (handled by CombatService)
- Fog-of-war visualization (handled by UI layer)

---

## Behaviors

### Primary Behaviors

#### 1. Directional Movement (`MoveAsync`)

**Trigger:** Player issues movement command (e.g., "go north", "move down")

**Process:**
1. **Current Room Validation**:
   ```csharp
   if (_gameState.CurrentRoomId == null)
       return "Error: You are lost in the void.";

   var currentRoom = await _roomRepository.GetByIdAsync(_gameState.CurrentRoomId.Value);
   if (currentRoom == null)
       return "Error: Your current location no longer exists.";
   ```

2. **Exit Validation**:
   ```csharp
   if (!currentRoom.Exits.TryGetValue(direction, out var nextRoomId))
       return $"You cannot go {direction.ToString().ToLower()} from here.";
   ```

3. **Destination Validation**:
   ```csharp
   var nextRoom = await _roomRepository.GetByIdAsync(nextRoomId);
   if (nextRoom == null)
       return "Error: The path leads nowhere. The dungeon seems corrupted.";
   ```

4. **State Update** (Atomic):
   ```csharp
   _gameState.CurrentRoomId = nextRoomId;
   _gameState.TurnCount++;
   _gameState.VisitedRoomIds.Add(nextRoomId);
   ```

5. **Hazard Triggering** (v0.3.3a):
   ```csharp
   var hazardResults = await _hazardService.TriggerOnRoomEnterAsync(nextRoom);
   foreach (var result in hazardResults.Where(r => r.WasTriggered))
   {
       _inputHandler.DisplayMessage($"[HAZARD] {result.Message}");
   }
   ```

6. **Room Description** (v0.3.3b):
   ```csharp
   return await FormatRoomDescriptionAsync(nextRoom, $"You move {direction}...\n\n");
   ```

**Outcomes:**
- **Success**: Player moved to new room, hazards triggered, description displayed
- **Invalid Exit**: "You cannot go [direction] from here."
- **Broken Exit**: "Error: The path leads nowhere."
- **Null State**: "Error: You are lost in the void."

**Logging:**
```csharp
_logger.LogInformation("Player moved {Direction} to '{RoomName}' ({RoomId})",
    direction, nextRoom.Name, nextRoom.Id);
```

---

#### 2. Room Description (`LookAsync`)

**Trigger:** Player issues "look" command

**Process:**
1. Retrieve current room via `GetCurrentRoomAsync()`
2. Format description via `FormatRoomDescriptionAsync(room)`
3. Return formatted string (no state changes)

**Format Structure:**
```
[Room Name]
Room description text here.
[AMBIENT] [Color]Condition Name[/]: Condition description (if present)

Exits: north, east, down
```

**Special Cases:**
- **No Current Room**: "You are surrounded by an impenetrable void."
- **No Exits**: "There are no obvious exits."

---

#### 3. Room Description Formatting (`FormatRoomDescriptionAsync`)

**Internal Method** - Shared formatting logic for `MoveAsync` and `LookAsync`

**Structure:**
```csharp
private async Task<string> FormatRoomDescriptionAsync(Room room, string prefix = "")
{
    // 1. Collect exits
    var exits = room.Exits.Keys.Select(d => d.ToString().ToLower());
    var exitText = exits.Any()
        ? $"Exits: {string.Join(", ", exits)}"
        : "There are no obvious exits.";

    // 2. Check for ambient condition (v0.3.3b)
    var conditionText = string.Empty;
    var condition = await _conditionService.GetRoomConditionAsync(room.Id);
    if (condition != null)
    {
        conditionText = $"\n[AMBIENT] [{condition.Color}]{condition.Name}[/]: {condition.Description}";
    }

    // 3. Assemble final string
    return $"{prefix}[{room.Name}]\n{room.Description}{conditionText}\n\n{exitText}";
}
```

**Prefix Usage:**
- Movement: `"You move north...\n\n"` (confirmation text)
- Look command: `""` (no prefix)

---

#### 4. Current Room Retrieval (`GetCurrentRoomAsync`)

**Purpose:** Safe retrieval of current room with null handling

**Implementation:**
```csharp
public async Task<Room?> GetCurrentRoomAsync()
{
    if (_gameState.CurrentRoomId == null)
        return null;

    return await _roomRepository.GetByIdAsync(_gameState.CurrentRoomId.Value);
}
```

**Used By:**
- `LookAsync()` for room description
- `GetAvailableExitsAsync()` for exit enumeration
- External systems (CombatService, HazardService, etc.)

---

#### 5. Exit Enumeration (`GetAvailableExitsAsync`)

**Purpose:** Retrieve available directions from current room for UI display / validation

**Implementation:**
```csharp
public async Task<IEnumerable<Direction>> GetAvailableExitsAsync()
{
    var room = await GetCurrentRoomAsync();
    if (room == null)
        return Enumerable.Empty<Direction>();

    return room.Exits.Keys;
}
```

**Returns:**
- Collection of `Direction` enum values
- Empty collection if no current room

---

### Secondary Behaviors

#### 1. Visited Room Tracking

**Purpose:** Maintain persistent log of all visited rooms for fog-of-war systems

**Implementation:**
```csharp
_gameState.VisitedRoomIds.Add(nextRoomId);
```

**Persistence:**
- Stored in `GameState.VisitedRoomIds` (HashSet<Guid>)
- Persisted to save file via SaveManager
- Used by UI layer to determine room visibility on map

**Deduplication:**
- HashSet prevents duplicate entries automatically
- Re-visiting a room does not increment visited count

---

#### 2. Turn Count Increment

**Purpose:** Track global turn counter for resource decay, event triggers, time-based systems

**Implementation:**
```csharp
_gameState.TurnCount++;
```

**Triggered By:**
- Every successful `MoveAsync()` call
- NOT triggered by `LookAsync()` (free action)

**Used By:**
- RestService (rest duration calculations)
- HazardService (timed hazard triggers)
- StatusEffectService (duration decrements)
- ResourceService (light decay, hunger, etc.)

---

### Edge Cases

#### 1. Null GameState.CurrentRoomId

**Scenario:** Player state is corrupted or uninitialized

**Handling:**
```csharp
if (_gameState.CurrentRoomId == null)
{
    _logger.LogWarning("Move attempted with no current room set");
    return "Error: You are lost in the void. No current location.";
}
```

**Recovery:** Requires game restart or manual state correction (admin command)

---

#### 2. Current Room Not Found in Database

**Scenario:** Room was deleted after being set as current location

**Handling:**
```csharp
if (currentRoom == null)
{
    _logger.LogError("Current room ID {RoomId} not found in database", _gameState.CurrentRoomId);
    return "Error: Your current location no longer exists.";
}
```

**Recovery:** Requires manual state correction (set valid CurrentRoomId)

---

#### 3. Exit Leads to Non-Existent Room

**Scenario:** Broken exit reference (destination room deleted but exit remains)

**Handling:**
```csharp
if (nextRoom == null)
{
    _logger.LogError("Exit leads to non-existent room {RoomId}", nextRoomId);
    return "Error: The path leads nowhere. The dungeon seems corrupted.";
}
```

**State Impact:** Player remains in current room (no state changes)

**Recovery:** Database repair required (remove broken exit or recreate room)

---

#### 4. Multiple Hazards Trigger Simultaneously

**Scenario:** Room entry triggers multiple hazards (pressure plate + toxic gas + tripwire)

**Handling:**
```csharp
var hazardResults = await _hazardService.TriggerOnRoomEnterAsync(nextRoom);
foreach (var result in hazardResults.Where(r => r.WasTriggered))
{
    _inputHandler.DisplayMessage($"[HAZARD] {result.Message}");
    _logger.LogWarning("[Hazard] Movement trigger: {Hazard} in {Room}. Damage: {Damage}",
        result.HazardName, nextRoom.Name, result.TotalDamage);
}
```

**Order:** Hazards processed in the order returned by HazardService (creation order)

**Damage Stacking:** All damage applied cumulatively (no caps)

---

#### 5. Room with No Exits (Dead End)

**Scenario:** Room.Exits dictionary is empty

**Handling:**
- `LookAsync()`: Returns "There are no obvious exits."
- `MoveAsync(any direction)`: Returns "You cannot go [direction] from here."

**Purpose:** Intentional design for trap rooms, puzzle endpoints, boss arenas

---

#### 6. Vertical Movement (Up/Down)

**Scenario:** Player moves vertically between dungeon levels

**Handling:**
- Identical to horizontal movement
- No special physics or restrictions
- Direction enum includes `Direction.Up` and `Direction.Down`

**Example:**
```csharp
await _navigationService.MoveAsync(Direction.Down);
// "You move down..."
// "[The Pit]"
// "A yawning chasm stretches below."
// "Exits: up"
```

---

## Restrictions

### MUST Requirements

1. **MUST validate CurrentRoomId before any movement operation**
   - **Reason:** Prevents null reference exceptions and corrupted state
   - **Implementation:** NavigationService.cs:55-59

2. **MUST validate exit existence before attempting room transition**
   - **Reason:** Prevents invalid room state from broken references
   - **Implementation:** NavigationService.cs:69-73

3. **MUST validate destination room existence before state update**
   - **Reason:** Prevents player being placed in void (null room)
   - **Implementation:** NavigationService.cs:76-81

4. **MUST update all three state properties atomically** (`CurrentRoomId`, `TurnCount`, `VisitedRoomIds`)
   - **Reason:** Prevents partial state corruption on failure
   - **Implementation:** NavigationService.cs:84-86

5. **MUST trigger movement hazards on room entry** (v0.3.3a)
   - **Reason:** Core gameplay mechanic for pressure plates, tripwires, etc.
   - **Implementation:** NavigationService.cs:94-101

6. **MUST display ambient conditions on room entry** (v0.3.3b)
   - **Reason:** Player awareness of environmental effects
   - **Implementation:** NavigationService.cs:160-168

7. **MUST increment turn count on successful movement only**
   - **Reason:** Failed movement should not consume time
   - **Implementation:** NavigationService.cs:85

8. **MUST log all movement attempts** (success and failure)
   - **Reason:** Debugging, analytics, replay systems
   - **Implementation:** NavigationService.cs:52, 88-91, 98-100

---

### MUST NOT Requirements

1. **MUST NOT allow movement without CurrentRoomId set**
   - **Violation Impact:** Null reference exception, application crash
   - **Enforcement:** NavigationService.cs:55-59

2. **MUST NOT update state on failed movement**
   - **Violation Impact:** Player location desync, turn count inflation
   - **Enforcement:** Early return on validation failures (lines 58, 72, 80)

3. **MUST NOT allow movement to non-existent rooms**
   - **Violation Impact:** Player stuck in void, game unplayable
   - **Enforcement:** NavigationService.cs:76-81

4. **MUST NOT modify room data during navigation**
   - **Violation Impact:** Concurrent modification issues, data corruption
   - **Enforcement:** All operations are read-only except GameState updates

5. **MUST NOT suppress hazard triggering**
   - **Violation Impact:** Bypasses core gameplay mechanics
   - **Enforcement:** No conditional skipping of hazard service calls

6. **MUST NOT increment turn count on `LookAsync()`**
   - **Violation Impact:** Looking becomes costly, discourages exploration
   - **Enforcement:** LookAsync() has no state mutation code

---

## Limitations

### Numerical Limits

- **Supported Directions:** 6 (North, South, East, West, Up, Down)
- **Maximum Exits Per Room:** Dictionary size limit (~2 billion, effectively unlimited)
- **Turn Count Range:** `int.MaxValue` (2,147,483,647 turns)
- **Visited Room Set Size:** HashSet capacity (~billions of GUIDs)

### Functional Limitations

1. **Unidirectional Exits by Default**
   - Exits do NOT automatically create reverse paths
   - Example: Room A has exit North → Room B, but Room B does NOT automatically have exit South → Room A
   - Bidirectionality must be explicitly configured at dungeon generation time

2. **No Exit Locking/Unlocking**
   - NavigationService does not handle locked doors
   - Locked exit mechanics handled by InteractionService
   - Validation chain assumes all exits in Exits dictionary are traversable

3. **No Movement Cost Variation**
   - All movement consumes exactly 1 turn
   - No concept of "difficult terrain" or speed modifiers
   - Future extension: ConditionService could add movement penalties

4. **No Diagonal Movement**
   - Only cardinal directions supported
   - No "northeast" or "southwest" movement
   - 3D dungeons use Up/Down for vertical traversal

5. **No Movement History Tracking**
   - System tracks visited rooms (set) but not path taken (sequence)
   - Future extension: PathTracker service for backtracking

6. **No Room Discovery Radius**
   - Only current room is "seen" on movement
   - Adjacent rooms not revealed until entered
   - Fog-of-war is strict (no lookahead)

---

### System-Specific Limitations

1. **Hazard Triggering is Synchronous**
   - All hazards must complete before room description displays
   - Slow hazard logic blocks movement completion
   - Future: Async hazard processing with progress indicators

2. **Room Description is Static**
   - Descriptions do not change based on previous visits
   - No "you return to..." contextual text
   - Future: DynamicDescriptionService for contextual variations

3. **No Exit Prioritization**
   - Exit display order is dictionary order (non-deterministic)
   - Cannot specify "primary" vs "hidden" exits
   - UI layer must sort for consistent display

---

## Use Cases

### USE CASE 1: Standard Room Movement (Success Path)

**Setup:**
```csharp
// Game state initialized
_gameState.CurrentRoomId = entryHallId;
_gameState.TurnCount = 0;

// Room structure
entryHall.Exits = { { Direction.North, corridorId } };
```

**Execution:**
```csharp
var result = await _navigationService.MoveAsync(Direction.North);
```

**Internal Flow:**

1. **Validation Chain**:
   ```csharp
   // Line 55: Check CurrentRoomId not null ✓
   // Line 61: Load current room from database ✓
   // Line 69: Check exit exists in direction ✓
   // Line 76: Load destination room ✓
   ```

2. **State Update**:
   ```csharp
   _gameState.CurrentRoomId = corridorId;      // Line 84
   _gameState.TurnCount++;                     // Line 85 (0 → 1)
   _gameState.VisitedRoomIds.Add(corridorId);  // Line 86
   ```

3. **Hazard Check**:
   ```csharp
   var hazardResults = await _hazardService.TriggerOnRoomEnterAsync(corridor);
   // Returns empty list (no hazards in corridor)
   ```

4. **Condition Check**:
   ```csharp
   var condition = await _conditionService.GetRoomConditionAsync(corridorId);
   // Returns null (no ambient condition)
   ```

5. **Description Assembly**:
   ```csharp
   return "You move north...\n\n[Rusted Corridor]\nCorroded pipes line the walls.\n\nExits: north, south";
   ```

**Assertions:**
- `_gameState.CurrentRoomId == corridorId`
- `_gameState.TurnCount == 1`
- `_gameState.VisitedRoomIds.Contains(corridorId) == true`
- Output contains "[Rusted Corridor]"

**Test Reference:** NavigationServiceTests.cs:108-144

---

### USE CASE 2: Invalid Direction (No Exit Exists)

**Setup:**
```csharp
_gameState.CurrentRoomId = entryHallId;
entryHall.Exits = { { Direction.North, corridorId } }; // Only north exit
```

**Execution:**
```csharp
var result = await _navigationService.MoveAsync(Direction.East);
```

**Internal Flow:**

1. **Validation Chain**:
   ```csharp
   // Line 55: Check CurrentRoomId not null ✓
   // Line 61: Load current room ✓
   // Line 69: Check exit exists → FAIL
   if (!currentRoom.Exits.TryGetValue(Direction.East, out var nextRoomId))
       return "You cannot go east from here.";
   ```

2. **State Changes**: NONE (early return prevents state mutation)

**Assertions:**
- `_gameState.CurrentRoomId == entryHallId` (unchanged)
- `_gameState.TurnCount == 0` (unchanged)
- Output: "You cannot go east from here."

**Test Reference:** NavigationServiceTests.cs:85-105

---

### USE CASE 3: Broken Exit (Destination Room Missing)

**Setup:**
```csharp
_gameState.CurrentRoomId = entryHallId;
var brokenExitId = Guid.NewGuid(); // ID that doesn't exist in database
entryHall.Exits = { { Direction.North, brokenExitId } };
```

**Execution:**
```csharp
var result = await _navigationService.MoveAsync(Direction.North);
```

**Internal Flow:**

1. **Validation Chain**:
   ```csharp
   // Line 55: Check CurrentRoomId not null ✓
   // Line 61: Load current room ✓
   // Line 69: Check exit exists ✓
   // Line 76: Load destination room → RETURNS NULL
   if (nextRoom == null)
   {
       _logger.LogError("Exit leads to non-existent room {RoomId}", nextRoomId);
       return "Error: The path leads nowhere. The dungeon seems corrupted.";
   }
   ```

2. **State Changes**: NONE (early return before state update block)

**Assertions:**
- `_gameState.CurrentRoomId == entryHallId` (unchanged)
- `_gameState.TurnCount == 0` (unchanged)
- Output contains "nowhere"
- Error log entry created

**Test Reference:** NavigationServiceTests.cs:213-235

---

### USE CASE 4: Vertical Movement (Down into Pit)

**Setup:**
```csharp
_gameState.CurrentRoomId = chamberRoomId;
chamber.Exits = { { Direction.Down, pitRoomId } };
pit.Position = new Coordinate(0, 0, -1); // Below chamber
```

**Execution:**
```csharp
var result = await _navigationService.MoveAsync(Direction.Down);
```

**Internal Flow:**

1. **Validation Chain**: Standard (all pass)

2. **State Update**:
   ```csharp
   _gameState.CurrentRoomId = pitRoomId;
   _gameState.TurnCount++;
   _gameState.VisitedRoomIds.Add(pitRoomId);
   ```

3. **Description**:
   ```
   You move down...

   [The Pit]
   A yawning chasm stretches below.

   Exits: up
   ```

**Assertions:**
- `_gameState.CurrentRoomId == pitRoomId`
- Output contains "move down"
- Direction formatting is lowercase ("down", not "Down")

**Test Reference:** NavigationServiceTests.cs:237-268 (Theory test for all directions)

---

### USE CASE 5: Movement with Hazard Triggering (v0.3.3a)

**Setup:**
```csharp
_gameState.CurrentRoomId = corridorId;
corridor.Exits = { { Direction.North, trapRoomId } };

// Trap room has pressure plate hazard
var pressurePlate = new DynamicHazard
{
    Name = "Rusty Pressure Plate",
    TriggerType = HazardTriggerType.OnRoomEnter,
    // ... hazard configuration
};
```

**Execution:**
```csharp
var result = await _navigationService.MoveAsync(Direction.North);
```

**Internal Flow:**

1. **Validation & State Update**: Standard (all pass)

2. **Hazard Triggering**:
   ```csharp
   var hazardResults = await _hazardService.TriggerOnRoomEnterAsync(trapRoom);
   // Returns: HazardResult {
   //     WasTriggered = true,
   //     HazardName = "Rusty Pressure Plate",
   //     Message = "You step on a rusty pressure plate! SNAP!",
   //     TotalDamage = 5
   // }
   ```

3. **Hazard Display**:
   ```csharp
   foreach (var result in hazardResults.Where(r => r.WasTriggered))
   {
       _inputHandler.DisplayMessage($"[HAZARD] {result.Message}");
       // Outputs: "[HAZARD] You step on a rusty pressure plate! SNAP!"
   }
   ```

4. **Room Description**: Standard assembly

**Output:**
```
[HAZARD] You step on a rusty pressure plate! SNAP!

You move north...

[Trapped Chamber]
The floor is littered with rusted mechanisms.

Exits: south
```

**Assertions:**
- Hazard message displayed BEFORE room description
- Player HP reduced by hazard damage
- Log entry created: `[Hazard] Movement trigger: Rusty Pressure Plate in Trapped Chamber. Damage: 5`

**Test Reference:** HazardServiceTests.cs (integration with NavigationService)

---

### USE CASE 6: Movement with Ambient Condition (v0.3.3b)

**Setup:**
```csharp
_gameState.CurrentRoomId = corridorId;
corridor.Exits = { { Direction.East, toxicRoomId } };

// Toxic room has ambient condition
var toxicHaze = new AmbientCondition
{
    RoomId = toxicRoomId,
    Name = "Corrosive Haze",
    Description = "Acrid fumes burn your lungs.",
    Color = "yellow",
    // ... condition effects
};
```

**Execution:**
```csharp
var result = await _navigationService.MoveAsync(Direction.East);
```

**Internal Flow:**

1. **Validation, State Update, Hazard Check**: Standard

2. **Condition Retrieval**:
   ```csharp
   var condition = await _conditionService.GetRoomConditionAsync(toxicRoomId);
   // Returns: toxicHaze
   ```

3. **Description Assembly**:
   ```csharp
   conditionText = "\n[AMBIENT] [yellow]Corrosive Haze[/]: Acrid fumes burn your lungs.";

   return $"You move east...\n\n[Toxic Storage]\nBarrels leak noxious fluids.{conditionText}\n\nExits: west";
   ```

**Output:**
```
You move east...

[Toxic Storage]
Barrels leak noxious fluids.
[AMBIENT] [yellow]Corrosive Haze[/]: Acrid fumes burn your lungs.

Exits: west
```

**Assertions:**
- Condition displayed AFTER room description, BEFORE exit list
- Markup colors preserved (`[yellow]...[/]`)
- Condition effects applied by ConditionService on subsequent turns

**Test Reference:** ConditionServiceTests.cs (integration with NavigationService)

---

### USE CASE 7: Look Command (No State Change)

**Setup:**
```csharp
_gameState.CurrentRoomId = entryHallId;
_gameState.TurnCount = 5;
entryHall.Exits = { { Direction.North, corridorId }, { Direction.East, chamberRoomId } };
```

**Execution:**
```csharp
var result = await _navigationService.LookAsync();
```

**Internal Flow:**

1. **Room Retrieval**:
   ```csharp
   var room = await GetCurrentRoomAsync();
   // Returns: entryHall
   ```

2. **Description Formatting**:
   ```csharp
   return await FormatRoomDescriptionAsync(room);
   // No prefix (empty string)
   ```

**Output:**
```
[Entry Hall]
A cold, metallic chamber hums with distant machinery.

Exits: north, east
```

**Assertions:**
- `_gameState.TurnCount == 5` (UNCHANGED - look is free action)
- `_gameState.CurrentRoomId == entryHallId` (unchanged)
- No "[HAZARD]" messages (hazards only trigger on movement)
- No movement confirmation text ("You move...")

**Test Reference:** NavigationServiceTests.cs:274-314

---

### USE CASE 8: Get Available Exits (UI Integration)

**Setup:**
```csharp
_gameState.CurrentRoomId = chamberRoomId;
chamber.Exits = {
    { Direction.North, Guid.NewGuid() },
    { Direction.Down, Guid.NewGuid() }
};
```

**Execution:**
```csharp
var exits = await _navigationService.GetAvailableExitsAsync();
```

**Internal Flow:**

1. **Room Retrieval**:
   ```csharp
   var room = await GetCurrentRoomAsync();
   ```

2. **Exit Enumeration**:
   ```csharp
   return room.Exits.Keys;
   // Returns: IEnumerable<Direction> { Direction.North, Direction.Down }
   ```

**Assertions:**
- `exits.Count() == 2`
- `exits.Contains(Direction.North) == true`
- `exits.Contains(Direction.Down) == true`

**Usage:** UI layer uses this to dynamically enable/disable direction buttons

**Test Reference:** NavigationServiceTests.cs:391-416

---

### USE CASE 9: Dead End Room (No Exits)

**Setup:**
```csharp
_gameState.CurrentRoomId = deadEndRoomId;
deadEnd.Exits = new Dictionary<Direction, Guid>(); // Empty
```

**Execution:**
```csharp
var description = await _navigationService.LookAsync();
```

**Internal Flow:**

1. **Room Retrieval**: Standard

2. **Exit Formatting**:
   ```csharp
   var exits = room.Exits.Keys.Select(d => d.ToString().ToLower());
   // exits.Any() == false
   var exitText = "There are no obvious exits.";
   ```

**Output:**
```
[Dead End]
A wall of solid rock blocks your path.

There are no obvious exits.
```

**Assertions:**
- Output contains "no obvious exits"
- Attempting movement in any direction returns "You cannot go [direction] from here."

**Test Reference:** NavigationServiceTests.cs:317-337

---

### USE CASE 10: Turn Count Tracking Across Multiple Movements

**Setup:**
```csharp
_gameState.TurnCount = 0;
_gameState.CurrentRoomId = roomAId;

// Linear path: A → B → C
roomA.Exits = { { Direction.North, roomBId } };
roomB.Exits = { { Direction.North, roomCId }, { Direction.South, roomAId } };
roomC.Exits = { { Direction.South, roomBId } };
```

**Execution:**
```csharp
await _navigationService.MoveAsync(Direction.North); // A → B
await _navigationService.MoveAsync(Direction.North); // B → C
await _navigationService.LookAsync();                // Look (free action)
await _navigationService.MoveAsync(Direction.South); // C → B
```

**State Tracking:**
```
After Move 1: TurnCount = 1, CurrentRoomId = roomBId
After Move 2: TurnCount = 2, CurrentRoomId = roomCId
After Look:   TurnCount = 2 (unchanged)
After Move 3: TurnCount = 3, CurrentRoomId = roomBId
```

**Visited Rooms:**
```csharp
_gameState.VisitedRoomIds.Count == 3; // {roomAId, roomBId, roomCId}
// Re-visiting roomB does not add duplicate
```

**Assertions:**
- Turn count increments only on successful movement
- Look command does not consume turns
- Visited set prevents duplicate entries

**Test Reference:** NavigationServiceTests.cs:146-176 (turn count tests)

---

## Decision Trees

### Decision Tree 1: Movement Validation Chain

```
MoveAsync(direction) INVOKED
│
├─ CurrentRoomId == null?
│  ├─ YES → RETURN "Error: You are lost in the void."
│  └─ NO → CONTINUE
│
├─ Load CurrentRoom from Database
│  ├─ Room == null?
│  │  ├─ YES → LOG ERROR → RETURN "Error: Your current location no longer exists."
│  │  └─ NO → CONTINUE
│  │
│  ├─ CurrentRoom.Exits.ContainsKey(direction)?
│  │  ├─ NO → RETURN "You cannot go [direction] from here."
│  │  └─ YES → EXTRACT nextRoomId → CONTINUE
│  │
│  ├─ Load NextRoom from Database
│  │  ├─ NextRoom == null?
│  │  │  ├─ YES → LOG ERROR → RETURN "Error: The path leads nowhere."
│  │  │  └─ NO → CONTINUE TO STATE UPDATE
│  │
│  └─ STATE UPDATE BLOCK (ATOMIC)
│     ├─ Set CurrentRoomId = nextRoomId
│     ├─ Increment TurnCount
│     └─ Add nextRoomId to VisitedRoomIds
│
├─ TRIGGER HAZARDS
│  └─ For Each Triggered Hazard → Display "[HAZARD] {message}"
│
└─ FORMAT ROOM DESCRIPTION
   ├─ Check for AmbientCondition
   ├─ Assemble Prefix + Name + Description + Condition + Exits
   └─ RETURN formatted string
```

**Key Decision Points:**
1. **Null CurrentRoomId**: Application state error (requires restart)
2. **Missing Current Room**: Database integrity error (requires admin intervention)
3. **Invalid Direction**: Normal gameplay (player explored all exits)
4. **Broken Exit**: Database integrity error (requires repair)
5. **Valid Path**: Standard success case

---

### Decision Tree 2: Room Description Formatting

```
FormatRoomDescriptionAsync(room, prefix) INVOKED
│
├─ EXTRACT EXITS
│  ├─ room.Exits.Keys → SELECT direction.ToString().ToLower()
│  ├─ exits.Any()?
│  │  ├─ YES → exitText = "Exits: {joined}"
│  │  └─ NO → exitText = "There are no obvious exits."
│  │
│  └─ CONTINUE
│
├─ CHECK FOR AMBIENT CONDITION (v0.3.3b)
│  ├─ condition = await _conditionService.GetRoomConditionAsync(room.Id)
│  ├─ condition != null?
│  │  ├─ YES → conditionText = "\n[AMBIENT] [{color}]{name}[/]: {description}"
│  │  │         LOG "Room {room} has condition [{condition}]"
│  │  └─ NO → conditionText = ""
│  │
│  └─ CONTINUE
│
└─ ASSEMBLE FINAL STRING
   └─ RETURN "{prefix}[{room.Name}]\n{room.Description}{conditionText}\n\n{exitText}"
```

**Formatting Rules:**
- Prefix: Added only for movement confirmations ("You move north...\n\n")
- Room Name: Always wrapped in brackets `[Name]`
- Description: Raw text from database
- Condition: Inserted AFTER description, BEFORE exits
- Exits: Lowercase, comma-separated, or "no obvious exits" message

---

### Decision Tree 3: Exit Availability Check

```
GetAvailableExitsAsync() INVOKED
│
├─ room = await GetCurrentRoomAsync()
│  ├─ room == null?
│  │  ├─ YES → RETURN Enumerable.Empty<Direction>()
│  │  └─ NO → CONTINUE
│  │
│  └─ RETURN room.Exits.Keys
```

**Usage Patterns:**
- **UI Layer**: Enable/disable direction buttons based on available exits
- **Validation**: Pre-check before attempting movement (optional, not required)
- **Pathfinding**: Retrieve all possible paths from current node

---

## Sequence Diagrams

### Sequence Diagram 1: Successful Room Movement

```
Player         NavigationService    GameState    RoomRepository    HazardService    ConditionService    InputHandler
  |                    |                |               |                 |                  |                |
  |--- MoveAsync(North) --------------->|               |                 |                  |                |
  |                    |                |               |                 |                  |                |
  |                    |-- Get CurrentRoomId ---------->|                 |                  |                |
  |                    |<-- entryHallId ----------------|                 |                  |                |
  |                    |                |               |                 |                  |                |
  |                    |-- GetByIdAsync(entryHallId) -->|                 |                  |                |
  |                    |<-- Room[EntryHall] ------------|                 |                  |                |
  |                    |                |               |                 |                  |                |
  |                    |-- Check Exits[North] = corridorId                |                  |                |
  |                    |                |               |                 |                  |                |
  |                    |-- GetByIdAsync(corridorId) --->|                 |                  |                |
  |                    |<-- Room[Corridor] -------------|                 |                  |                |
  |                    |                |               |                 |                  |                |
  |                    |-- Set CurrentRoomId = corridorId -->            |                  |                |
  |                    |-- Increment TurnCount ---------->|               |                  |                |
  |                    |-- Add corridorId to VisitedRoomIds ->           |                  |                |
  |                    |                |               |                 |                  |                |
  |                    |-- TriggerOnRoomEnterAsync(corridor) ----------->|                  |                |
  |                    |<-- List<HazardResult> (empty) ------------------|                  |                |
  |                    |                |               |                 |                  |                |
  |                    |-- GetRoomConditionAsync(corridorId) --------------------------->   |                |
  |                    |<-- null (no condition) -------------------------------------------   |                |
  |                    |                |               |                 |                  |                |
  |                    |-- Format Description (Name + Description + Exits)                   |                |
  |                    |                |               |                 |                  |                |
  |<-- "You move north...\n\n[Rusted Corridor]\n..." --|                 |                  |                |
```

**Key Steps:**
1. Validation: CurrentRoomId → Load current room → Check exit → Load destination
2. State Update: Atomic modification of GameState (3 properties)
3. Triggers: Hazards → Conditions (sequential)
4. Description: Assembly and return

**Timing:**
- Total execution: ~50-100ms (3 DB queries + 2 service calls)
- No blocking UI operations during navigation

---

### Sequence Diagram 2: Movement with Hazard Triggering

```
Player         NavigationService    GameState    RoomRepository    HazardService    InputHandler
  |                    |                |               |                 |                |
  |--- MoveAsync(North) --------------->|               |                 |                |
  |                    |                |               |                 |                |
  |                    |-- [Standard Validation & State Update] -------> |                |
  |                    |                |               |                 |                |
  |                    |-- TriggerOnRoomEnterAsync(trapRoom) ----------->|                |
  |                    |                |               |                 |                |
  |                    |                |               |   [Process Hazards]             |
  |                    |                |               |   - Evaluate Triggers           |
  |                    |                |               |   - Roll for Activation         |
  |                    |                |               |   - Calculate Damage            |
  |                    |                |               |                 |                |
  |                    |<-- List<HazardResult> -------------------------|                |
  |                    |    [{ WasTriggered: true,                       |                |
  |                    |       HazardName: "Pressure Plate",             |                |
  |                    |       Message: "SNAP!",                         |                |
  |                    |       TotalDamage: 5 }]                         |                |
  |                    |                |               |                 |                |
  |                    |-- FOR EACH result WHERE WasTriggered ------------------------------>|
  |                    |   DisplayMessage("[HAZARD] SNAP!") -------------------------------->|
  |                    |                |               |                 |                |
  |                    |-- Log Hazard Warning ------------------------------------------->   |
  |                    |                |               |                 |                |
  |                    |-- Format Room Description ---->|                 |                |
  |                    |                |               |                 |                |
  |<-- "[HAZARD] SNAP!\n\nYou move north...\n..." -----|                 |                |
```

**Hazard Processing:**
- Hazard messages displayed BEFORE room description
- Multiple hazards processed in sequence (loop iteration)
- Damage application handled by HazardService (player HP already reduced)

---

### Sequence Diagram 3: Failed Movement (No Exit)

```
Player         NavigationService    GameState    RoomRepository
  |                    |                |               |
  |--- MoveAsync(East) --------------->|               |
  |                    |                |               |
  |                    |-- Get CurrentRoomId ---------->|
  |                    |<-- entryHallId ----------------|
  |                    |                |               |
  |                    |-- GetByIdAsync(entryHallId) -->|
  |                    |<-- Room[EntryHall] ------------|
  |                    |                |               |
  |                    |-- Check Exits[East] → NOT FOUND
  |                    |                |               |
  |                    |-- [EARLY RETURN] --------------|
  |                    |                |               |
  |<-- "You cannot go east from here." |               |
  |                    |                |               |
  [NO STATE CHANGES]   [NO TURN INCREMENT]   [NO HAZARD TRIGGERS]
```

**Key Characteristic:**
- Early return prevents ALL downstream operations
- GameState remains completely unchanged
- No database writes occur
- No service calls beyond room repository

---

## Workflows

### Workflow 1: Room Movement Execution Checklist

**Purpose:** Comprehensive checklist for implementing room movement logic

**Steps:**

1. ☐ **Validate GameState.CurrentRoomId**
   - Check for null
   - Return error message if null: "Error: You are lost in the void."

2. ☐ **Load Current Room from Database**
   - Call `_roomRepository.GetByIdAsync(CurrentRoomId)`
   - Check for null (room deleted)
   - Return error message if null: "Error: Your current location no longer exists."

3. ☐ **Validate Exit Exists**
   - Check `currentRoom.Exits.TryGetValue(direction, out var nextRoomId)`
   - Return failure message if false: "You cannot go [direction] from here."

4. ☐ **Load Destination Room from Database**
   - Call `_roomRepository.GetByIdAsync(nextRoomId)`
   - Check for null (broken exit)
   - Return error message if null: "Error: The path leads nowhere."

5. ☐ **Update GameState Atomically**
   - Set `_gameState.CurrentRoomId = nextRoomId`
   - Increment `_gameState.TurnCount++`
   - Add `_gameState.VisitedRoomIds.Add(nextRoomId)`

6. ☐ **Log Movement**
   - Information level: Player moved {Direction} to '{RoomName}' ({RoomId})
   - Debug level: Room marked as visited, total visited count

7. ☐ **Trigger Movement Hazards** (v0.3.3a)
   - Call `_hazardService.TriggerOnRoomEnterAsync(nextRoom)`
   - Iterate triggered hazards
   - Display "[HAZARD] {message}" via InputHandler
   - Log hazard warnings

8. ☐ **Format Room Description**
   - Call `FormatRoomDescriptionAsync(nextRoom, "You move [direction]...\n\n")`
   - Include ambient condition if present (v0.3.3b)
   - Include exit list

9. ☐ **Return Formatted Description**

**Validation Points:**
- After step 5: Verify all 3 state properties updated
- After step 7: Ensure hazards displayed BEFORE room description
- After step 8: Verify description includes movement confirmation prefix

---

### Workflow 2: Room Description Formatting Workflow

**Purpose:** Consistent room description assembly for movement and look commands

**Steps:**

1. ☐ **Extract Exits from Room.Exits Dictionary**
   - Convert Direction enum values to lowercase strings
   - Store in `exits` collection

2. ☐ **Format Exit Text**
   - If exits.Any() == true:
     - Join with ", " → "Exits: north, east, down"
   - If exits.Any() == false:
     - Use "There are no obvious exits."

3. ☐ **Check for Ambient Condition** (v0.3.3b)
   - Call `_conditionService.GetRoomConditionAsync(room.Id)`
   - If condition != null:
     - Format: "\n[AMBIENT] [{condition.Color}]{condition.Name}[/]: {condition.Description}"
     - Log: "[Condition] Room {RoomName} has condition [{ConditionName}]"
   - If condition == null:
     - conditionText = ""

4. ☐ **Assemble Final String**
   - Structure: `{prefix}[{room.Name}]\n{room.Description}{conditionText}\n\n{exitText}`
   - Prefix: Empty for look command, "You move [direction]...\n\n" for movement

5. ☐ **Return Formatted String**

**Formatting Rules:**
- Room names ALWAYS bracketed: `[Name]`
- Exit directions ALWAYS lowercase: "north" not "North"
- Condition appears AFTER description, BEFORE exits
- Blank line separator between description and exits

---

### Workflow 3: Navigation System Initialization

**Purpose:** Setup checklist for integrating NavigationService into game

**Steps:**

1. ☐ **Register Service in DI Container** (App.axaml.cs)
   ```csharp
   services.AddSingleton<INavigationService, NavigationService>();
   ```

2. ☐ **Verify Dependencies Registered**
   - ☐ GameState (singleton)
   - ☐ IRoomRepository (scoped or singleton)
   - ☐ IHazardService (scoped or singleton)
   - ☐ IConditionService (scoped or singleton)
   - ☐ IInputHandler (scoped or singleton)
   - ☐ ILogger<NavigationService> (automatic)

3. ☐ **Initialize GameState.CurrentRoomId**
   - Set to starting room ID (usually entry hall)
   - Ensure room exists in database

4. ☐ **Initialize GameState.TurnCount**
   - Set to 0 for new game
   - Load from save file for continued game

5. ☐ **Initialize GameState.VisitedRoomIds**
   - Create new HashSet<Guid>() for new game
   - Add starting room ID
   - Load from save file for continued game

6. ☐ **Wire Movement Commands to Service**
   - Map "go north" → `MoveAsync(Direction.North)`
   - Map "look" → `LookAsync()`
   - Map "exits" → `GetAvailableExitsAsync()`

7. ☐ **Configure Input Handler for Hazard Messages**
   - Ensure InputHandler can display formatted text
   - Support markup tags: `[HAZARD]`, `[color]...[/]`

8. ☐ **Test Initial Navigation**
   - Load starting room
   - Verify `LookAsync()` returns formatted description
   - Attempt movement to valid/invalid exits

---

## Cross-System Integration

### Integration Matrix

| System | Dependency Type | Integration Points | Data Flow |
|--------|----------------|-------------------|-----------|
| **RoomRepository** | Required | `GetByIdAsync()` | NavigationService → RoomRepository (room data retrieval) |
| **GameState** | Required | `CurrentRoomId`, `TurnCount`, `VisitedRoomIds` | NavigationService ↔ GameState (bidirectional state management) |
| **HazardService** | Required | `TriggerOnRoomEnterAsync()` | NavigationService → HazardService (hazard activation) |
| **ConditionService** | Required | `GetRoomConditionAsync()` | NavigationService → ConditionService (ambient condition retrieval) |
| **InputHandler** | Required | `DisplayMessage()` | NavigationService → InputHandler (hazard message output) |
| **DungeonGenerator** | Producer | Room creation with Exits dictionary | DungeonGenerator → RoomRepository → NavigationService (dungeon structure) |
| **SaveManager** | Consumer | GameState persistence | NavigationService → GameState → SaveManager (save file) |
| **CombatService** | Consumer | Room position for encounter logic | CombatService → NavigationService.GetCurrentRoomAsync() |
| **InteractionService** | Consumer | Current room for object interaction | InteractionService → NavigationService.GetCurrentRoomAsync() |
| **ResourceService** | Observer | Turn count for resource decay | NavigationService (TurnCount increment) → ResourceService (observer pattern) |

---

### Integration Details

#### 1. RoomRepository Integration

**Interface:** `IRoomRepository`

**Methods Used:**
- `GetByIdAsync(Guid roomId) → Task<Room?>`

**Data Flow:**
```
NavigationService.MoveAsync()
  └→ RoomRepository.GetByIdAsync(CurrentRoomId)
      └→ Load Room entity from database
          └→ Return Room with Exits dictionary populated
```

**Error Handling:**
- Null return from `GetByIdAsync()` triggers error messages
- No retry logic (assumes database integrity)

**Performance:**
- 2 database queries per successful movement (current room + destination room)
- 1 database query per `LookAsync()` call

---

#### 2. GameState Integration

**Properties Modified:**
- `CurrentRoomId` (Guid?) - Current player location
- `TurnCount` (int) - Global turn counter
- `VisitedRoomIds` (HashSet<Guid>) - Room discovery tracking

**State Update Pattern:**
```csharp
// ATOMIC: All three properties updated together
_gameState.CurrentRoomId = nextRoomId;      // Position
_gameState.TurnCount++;                     // Time
_gameState.VisitedRoomIds.Add(nextRoomId);  // Discovery
```

**Thread Safety:**
- GameState is not thread-safe by default
- NavigationService assumes single-threaded access
- Concurrent movement not supported

**Persistence:**
- GameState serialized by SaveManager
- VisitedRoomIds saved as List<Guid> in save file
- CurrentRoomId and TurnCount saved as primitives

---

#### 3. HazardService Integration

**Interface:** `IHazardService`

**Method Used:**
- `TriggerOnRoomEnterAsync(Room room, Combatant? combatant = null) → Task<List<HazardResult>>`

**Trigger Flow:**
```
NavigationService.MoveAsync()
  └→ After state update
      └→ HazardService.TriggerOnRoomEnterAsync(nextRoom)
          └→ Returns List<HazardResult>
              └→ For each WasTriggered == true:
                  └→ InputHandler.DisplayMessage($"[HAZARD] {result.Message}")
```

**HazardResult Structure:**
```csharp
public record HazardResult
{
    public bool WasTriggered { get; init; }
    public string HazardName { get; init; }
    public string Message { get; init; }
    public int TotalDamage { get; init; }
}
```

**Message Display:**
- Hazards displayed immediately after state update
- Displayed BEFORE room description
- Multiple hazards displayed sequentially

---

#### 4. ConditionService Integration

**Interface:** `IConditionService`

**Method Used:**
- `GetRoomConditionAsync(Guid roomId) → Task<AmbientCondition?>`

**Trigger Flow:**
```
NavigationService.FormatRoomDescriptionAsync()
  └→ ConditionService.GetRoomConditionAsync(room.Id)
      └→ Returns AmbientCondition or null
          └→ If not null:
              └→ Append "\n[AMBIENT] [{color}]{name}[/]: {description}" to description
```

**AmbientCondition Structure:**
```csharp
public class AmbientCondition
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Color { get; set; } // Markup color (e.g., "yellow", "red")
    // ... passive effects
}
```

**Display Logic:**
- Condition text inserted AFTER room description
- Condition text inserted BEFORE exit list
- Markup tags preserved for UI rendering

---

#### 5. InputHandler Integration

**Interface:** `IInputHandler`

**Method Used:**
- `DisplayMessage(string message)`

**Usage:**
```csharp
foreach (var result in hazardResults.Where(r => r.WasTriggered))
{
    _inputHandler.DisplayMessage($"[HAZARD] {result.Message}");
}
```

**Message Format:**
- Prefix: `[HAZARD]` for visual distinction
- Content: Hazard-specific message (e.g., "You step on a pressure plate! SNAP!")

**UI Responsibility:**
- InputHandler routes message to appropriate UI component
- Text formatting (color, bold, etc.) handled by UI layer

---

#### 6. DungeonGenerator Integration

**Relationship:** DungeonGenerator produces room structures that NavigationService consumes

**Exit Creation Pattern:**
```csharp
// DungeonGenerator creates bidirectional exits
room1.Exits[Direction.North] = room2.Id;
room2.Exits[Direction.South] = room1.Id;

await _roomRepository.AddAsync(room1);
await _roomRepository.AddAsync(room2);
```

**NavigationService Expectations:**
- Exits dictionary is populated and valid
- Exit target IDs reference existing rooms
- No validation of exit integrity (assumes DungeonGenerator correctness)

---

#### 7. SaveManager Integration

**Relationship:** SaveManager persists GameState modified by NavigationService

**Serialization:**
```json
{
  "currentRoomId": "guid-value",
  "turnCount": 42,
  "visitedRoomIds": ["guid1", "guid2", "guid3"]
}
```

**Load Logic:**
```csharp
var saveData = await _saveManager.LoadAsync();
_gameState.CurrentRoomId = saveData.CurrentRoomId;
_gameState.TurnCount = saveData.TurnCount;
_gameState.VisitedRoomIds = new HashSet<Guid>(saveData.VisitedRoomIds);
```

---

#### 8. CombatService Integration

**Usage:** CombatService retrieves current room for encounter spawn logic

**Pattern:**
```csharp
var currentRoom = await _navigationService.GetCurrentRoomAsync();
if (currentRoom != null)
{
    var enemies = await _combatService.SpawnEncounter(currentRoom.DangerLevel);
}
```

**Current Room Properties Used:**
- `DangerLevel` - Scales enemy difficulty
- `BiomeType` - Determines enemy types
- `Name` - Display in combat header

---

#### 9. InteractionService Integration

**Usage:** InteractionService uses current room to scope object searches

**Pattern:**
```csharp
var currentRoom = await _navigationService.GetCurrentRoomAsync();
var interactableObjects = await _objectRepository.GetByRoomIdAsync(currentRoom.Id);
```

**Scoping:**
- Objects only interactable in current room
- Room transitions clear interaction state (e.g., open containers)

---

#### 10. ResourceService Integration

**Usage:** ResourceService observes TurnCount for resource decay (light, hunger, etc.)

**Pattern:**
```csharp
// NavigationService increments TurnCount
_gameState.TurnCount++;

// ResourceService monitors TurnCount
public async Task ProcessTurnAsync()
{
    if (_gameState.TurnCount % 10 == 0) // Every 10 turns
    {
        await DecayLightAsync();
    }
}
```

**Turn-Based Triggers:**
- Light decay: Every 10 turns
- Hunger increase: Every 20 turns
- Rest recovery: Based on turn duration

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

    /// <summary>
    /// Dictionary mapping directions to destination room IDs.
    /// Key: Direction enum value
    /// Value: Destination Room.Id
    /// </summary>
    public Dictionary<Direction, Guid> Exits { get; set; } = new();

    public Coordinate Position { get; set; } = new(0, 0, 0);
    public BiomeType BiomeType { get; set; }
    public DangerLevel DangerLevel { get; set; }

    // Navigation metadata
    public bool IsStartingRoom { get; set; }
    public bool IsBossRoom { get; set; }

    // Relationships
    public List<DynamicHazard> Hazards { get; set; } = new();
    public List<InteractableObject> Objects { get; set; } = new();
}
```

**Key Fields for Navigation:**
- `Id` - Unique identifier for room references
- `Name` - Displayed in brackets `[Name]`
- `Description` - Displayed below name
- `Exits` - Dictionary of valid movement directions
- `Position` - 3D coordinate (not used by NavigationService, but stored for mapping)

---

### Direction Enum

**Source:** `RuneAndRust.Core.Enums.Direction`

```csharp
public enum Direction
{
    /// <summary>Movement toward positive Y-axis.</summary>
    North = 0,

    /// <summary>Movement toward negative Y-axis.</summary>
    South = 1,

    /// <summary>Movement toward positive X-axis.</summary>
    East = 2,

    /// <summary>Movement toward negative X-axis.</summary>
    West = 3,

    /// <summary>Movement toward positive Z-axis (ascend).</summary>
    Up = 4,

    /// <summary>Movement toward negative Z-axis (descend).</summary>
    Down = 5
}
```

**Coordinate Mapping:**
- North = +Y
- South = -Y
- East = +X
- West = -X
- Up = +Z
- Down = -Z

**String Formatting:**
- Enum value `Direction.North` formatted as lowercase "north" in messages
- Conversion: `direction.ToString().ToLower()`

---

### GameState (Relevant Properties)

**Source:** `RuneAndRust.Core.Models.GameState`

```csharp
public class GameState
{
    /// <summary>
    /// The ID of the room the player is currently in.
    /// Null indicates uninitialized or invalid state.
    /// </summary>
    public Guid? CurrentRoomId { get; set; }

    /// <summary>
    /// Global turn counter. Incremented on successful movement.
    /// Used for resource decay, event triggers, time-based effects.
    /// </summary>
    public int TurnCount { get; set; }

    /// <summary>
    /// Set of all room IDs the player has visited.
    /// Used for fog-of-war mechanics and discovery tracking.
    /// </summary>
    public HashSet<Guid> VisitedRoomIds { get; set; } = new();

    // Other properties (character, inventory, etc.)
    public Character? CurrentCharacter { get; set; }
    // ...
}
```

**NavigationService Modifications:**
- Writes: `CurrentRoomId`, `TurnCount`, `VisitedRoomIds`
- Reads: `CurrentRoomId` (for validation)

---

### Coordinate Value Object

**Source:** `RuneAndRust.Core.ValueObjects.Coordinate`

```csharp
public record Coordinate(int X, int Y, int Z)
{
    /// <summary>Coordinate origin (0,0,0).</summary>
    public static readonly Coordinate Origin = new(0, 0, 0);

    /// <summary>
    /// Calculates Manhattan distance to another coordinate.
    /// </summary>
    public int ManhattanDistance(Coordinate other)
    {
        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z);
    }
}
```

**NavigationService Usage:**
- NOT directly used by NavigationService
- Stored in `Room.Position` for mapping/visualization
- Future extension: Pathfinding, distance calculations

---

### HazardResult Record

**Source:** `RuneAndRust.Core.Models.HazardResult`

```csharp
public record HazardResult
{
    public bool WasTriggered { get; init; }
    public string HazardName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public int TotalDamage { get; init; }
    public List<StatusEffect> AppliedEffects { get; init; } = new();
}
```

**NavigationService Usage:**
- Received from `HazardService.TriggerOnRoomEnterAsync()`
- Filtered by `WasTriggered == true`
- `Message` displayed via InputHandler
- `TotalDamage` and `AppliedEffects` handled by HazardService (player already modified)

---

### AmbientCondition Entity

**Source:** `RuneAndRust.Core.Entities.AmbientCondition`

```csharp
public class AmbientCondition
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "white"; // Markup color

    public ConditionIntensity Intensity { get; set; }
    public ConditionType Type { get; set; }

    // Passive effects (applied per turn while in room)
    public int DamagePerTurn { get; set; }
    public int StaminaDrainPerTurn { get; set; }
    // ...
}
```

**NavigationService Usage:**
- Retrieved via `ConditionService.GetRoomConditionAsync(room.Id)`
- Only `Name`, `Description`, `Color` used for display
- Passive effects processed by ConditionService on subsequent turns

---

## Configuration

### Constants (Implicit)

**Direction Count:**
```csharp
private const int SupportedDirections = 6; // North, South, East, West, Up, Down
```

**Error Messages:**
```csharp
private const string ErrorVoid = "Error: You are lost in the void. No current location.";
private const string ErrorRoomMissing = "Error: Your current location no longer exists.";
private const string ErrorBrokenExit = "Error: The path leads nowhere. The dungeon seems corrupted.";
private const string ErrorInvalidDirection = "You cannot go {0} from here.";
```

**Description Formatting:**
```csharp
private const string DescriptionFormat = "{0}[{1}]\n{2}{3}\n\n{4}";
// {0} = prefix, {1} = room name, {2} = description, {3} = condition, {4} = exits
```

---

### Logging Configuration

**Log Levels:**
- **Debug**: Movement attempts, exit validation, room retrieval
- **Information**: Successful movement, room transitions
- **Warning**: Hazard triggers
- **Error**: Missing rooms, broken exits, state corruption

**Sample Log Messages:**
```
[Debug] Attempting to move North
[Debug] Invalid move: No exit North from Entry Hall
[Information] Player moved North to 'Rusted Corridor' (guid-123)
[Debug] [Navigation] Room guid-123 marked as visited. Total visited: 5
[Warning] [Hazard] Movement trigger: Pressure Plate in Trapped Chamber. Damage: 5
[Error] Current room ID guid-456 not found in database
[Error] Exit leads to non-existent room guid-789
```

---

### Feature Flags

**v0.3.3a - Movement Hazard Triggering:**
```csharp
// Enabled by default in NavigationService
var hazardResults = await _hazardService.TriggerOnRoomEnterAsync(nextRoom);
```

**v0.3.3b - Ambient Condition Display:**
```csharp
// Enabled by default in FormatRoomDescriptionAsync
var condition = await _conditionService.GetRoomConditionAsync(room.Id);
```

**Future Configuration:**
- `ENABLE_MOVEMENT_HAZARDS` (toggle hazard triggering)
- `ENABLE_AMBIENT_CONDITIONS` (toggle condition display)
- `FOG_OF_WAR_ENABLED` (toggle visited room tracking)

---

## Testing

### Test Summary

**Source:** `RuneAndRust.Tests/Engine/NavigationServiceTests.cs` (420 lines)

**Test Count:** 18 tests

**Coverage Breakdown:**
- `MoveAsync()`: 9 tests
- `LookAsync()`: 3 tests
- `GetCurrentRoomAsync()`: 2 tests
- `GetAvailableExitsAsync()`: 2 tests
- Direction formatting: 1 parameterized test (6 test cases)

**Coverage Percentage:** ~95% (all public methods, all edge cases)

---

### Test Categories

#### 1. MoveAsync() Tests (9 tests)

**NavigationServiceTests.cs:57-67** - `MoveAsync_NoCurrentRoom_ReturnsErrorMessage`
- Validates null CurrentRoomId handling
- Asserts error message contains "void"

**NavigationServiceTests.cs:70-82** - `MoveAsync_CurrentRoomNotInDatabase_ReturnsErrorMessage`
- Validates missing current room handling
- Asserts error message contains "no longer exists"

**NavigationServiceTests.cs:85-105** - `MoveAsync_NoExitInDirection_ReturnsCannotGoMessage`
- Validates invalid direction handling
- Asserts error message contains "cannot go [direction]"

**NavigationServiceTests.cs:108-144** - `MoveAsync_ValidExit_UpdatesCurrentRoomId`
- Validates successful movement
- Asserts `CurrentRoomId` updated to destination room

**NavigationServiceTests.cs:146-176** - `MoveAsync_ValidExit_IncrementsTurnCount`
- Validates turn count increment
- Asserts `TurnCount` increments from 5 → 6

**NavigationServiceTests.cs:179-210** - `MoveAsync_ValidExit_ReturnsRoomDescription`
- Validates description formatting
- Asserts output contains "move north", room name, room description

**NavigationServiceTests.cs:213-235** - `MoveAsync_ExitLeadsToNonexistentRoom_ReturnsErrorMessage`
- Validates broken exit handling
- Asserts error message contains "nowhere"

**NavigationServiceTests.cs:237-268** - `MoveAsync_AllDirections_FormatsDirectionCorrectly`
- Parameterized test for all 6 directions
- Asserts lowercase formatting ("north" not "North")

---

#### 2. LookAsync() Tests (3 tests)

**NavigationServiceTests.cs:275-285** - `LookAsync_NoCurrentRoom_ReturnsVoidMessage`
- Validates null CurrentRoomId handling
- Asserts error message contains "void"

**NavigationServiceTests.cs:288-314** - `LookAsync_ValidRoom_ReturnsFormattedDescription`
- Validates description formatting
- Asserts output contains room name, description, exit list

**NavigationServiceTests.cs:317-337** - `LookAsync_RoomWithNoExits_ShowsNoObviousExits`
- Validates dead end handling
- Asserts output contains "no obvious exits"

---

#### 3. GetCurrentRoomAsync() Tests (2 tests)

**NavigationServiceTests.cs:344-354** - `GetCurrentRoomAsync_NoCurrentRoom_ReturnsNull`
- Validates null CurrentRoomId handling
- Asserts null return value

**NavigationServiceTests.cs:357-372** - `GetCurrentRoomAsync_ValidRoom_ReturnsRoom`
- Validates successful room retrieval
- Asserts room object returned with correct name

---

#### 4. GetAvailableExitsAsync() Tests (2 tests)

**NavigationServiceTests.cs:379-389** - `GetAvailableExitsAsync_NoCurrentRoom_ReturnsEmpty`
- Validates null CurrentRoomId handling
- Asserts empty collection returned

**NavigationServiceTests.cs:391-416** - `GetAvailableExitsAsync_RoomWithExits_ReturnsDirections`
- Validates exit enumeration
- Asserts correct direction count and values

---

### Mock Setup Patterns

**Default Mock Configuration:**
```csharp
public NavigationServiceTests()
{
    // ...setup mocks...

    // Default: No hazards trigger
    _mockHazardService.Setup(h => h.TriggerOnRoomEnterAsync(It.IsAny<Room>(), It.IsAny<Combatant?>()))
        .ReturnsAsync(new List<HazardResult>());

    // Default: No ambient conditions
    _mockConditionService.Setup(c => c.GetRoomConditionAsync(It.IsAny<Guid>()))
        .ReturnsAsync((AmbientCondition?)null);
}
```

**Room Repository Mock:**
```csharp
_mockRoomRepo.Setup(r => r.GetByIdAsync(currentRoomId))
    .ReturnsAsync(currentRoom);
_mockRoomRepo.Setup(r => r.GetByIdAsync(nextRoomId))
    .ReturnsAsync(nextRoom);
```

---

### Edge Case Test Coverage

**Covered Edge Cases:**
- ✅ Null CurrentRoomId (application state error)
- ✅ Missing current room from database (integrity error)
- ✅ Invalid exit direction (normal gameplay)
- ✅ Broken exit reference (integrity error)
- ✅ Vertical movement (Up/Down directions)
- ✅ Dead end room (no exits)
- ✅ All 6 cardinal directions (parameterized test)

**Not Covered (Future Tests):**
- ⏸️ Multiple hazard triggering (integration test needed)
- ⏸️ Ambient condition display (integration test needed)
- ⏸️ Concurrent movement attempts (thread safety)
- ⏸️ Turn count overflow (int.MaxValue boundary)

---

## Domain 4 Compliance

### Validation Status

**NavigationService.cs:** ✅ **COMPLIANT**

All flavor text, error messages, and room descriptions pass Domain 4 validation:

- ✅ Error messages use qualitative language ("void", "corrupted")
- ✅ No precision measurements in movement descriptions
- ✅ Room descriptions loaded from database (pre-validated)
- ✅ No hardcoded numeric values in messages

---

### Forbidden Patterns (NONE PRESENT)

**Precision Measurements:**
- ❌ "You move 4.2 meters north" → ✅ "You move north..."
- ❌ "Turn count: 42 turns remaining" → ✅ Turn count is internal state (not displayed)

**Technical Terms:**
- ❌ "ERROR: Null pointer exception" → ✅ "Error: You are lost in the void."
- ❌ "Database query failed" → ✅ "The path leads nowhere. The dungeon seems corrupted."

**Modern Units:**
- ❌ "5 seconds pass" → ✅ Turn count is abstract time unit (not displayed)

---

### Compliant Examples

**Movement Confirmation:**
```
You move north...

[Rusted Corridor]
Corroded pipes line the walls, dripping with decades of neglect.

Exits: north, south
```

**Error Messages:**
```
"You are lost in the void." (abstract, not "ERROR: CurrentRoomId == null")
"Your current location no longer exists." (narrative, not "Database integrity error")
"The path leads nowhere. The dungeon seems corrupted." (evocative, not "Broken FK reference")
```

**Hazard Display:**
```
[HAZARD] You step on a rusty pressure plate! SNAP!
```
(Action-focused, no "5 damage" display in NavigationService—damage application is internal)

---

## Future Extensions

### Planned Enhancements

#### 1. Movement Animation Support

**Requirement:** UI layer needs transition animations between rooms

**Extension Point:**
```csharp
public interface INavigationService
{
    // New method
    Task<NavigationResult> MoveWithAnimationAsync(Direction direction);
}

public record NavigationResult
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public Room? PreviousRoom { get; init; }
    public Room? NewRoom { get; init; }
    public TimeSpan AnimationDuration { get; init; }
}
```

**Implementation:**
- Return previous and new rooms for animation
- UI interpolates player position between coordinates
- Description display delayed until animation completes

---

#### 2. Exit Unlocking/Locking

**Requirement:** Doors, gates, and puzzle locks that block exits

**Extension Point:**
```csharp
public class Exit
{
    public Guid DestinationRoomId { get; set; }
    public bool IsLocked { get; set; }
    public string? RequiredKeyItemId { get; set; }
    public string LockedMessage { get; set; } = "The path is blocked.";
}

// Update Room.Exits to use Exit objects instead of raw GUIDs
public Dictionary<Direction, Exit> Exits { get; set; }
```

**Validation Logic:**
```csharp
if (exit.IsLocked)
{
    if (exit.RequiredKeyItemId != null && !_inventoryService.HasItem(exit.RequiredKeyItemId))
    {
        return exit.LockedMessage;
    }
}
```

---

#### 3. Movement Cost Variation

**Requirement:** Difficult terrain, injuries, or conditions affect movement speed

**Extension Point:**
```csharp
public interface IMovementCostCalculator
{
    int CalculateTurnCost(Room from, Room to, Character character);
}

// Implementation
public class MovementCostCalculator : IMovementCostCalculator
{
    public int CalculateTurnCost(Room from, Room to, Character character)
    {
        int baseCost = 1;

        // Terrain modifiers
        if (to.Terrain == TerrainType.Difficult)
            baseCost += 1;

        // Status effect modifiers
        if (character.StatusEffects.Any(s => s.Type == StatusEffectType.Slowed))
            baseCost += 1;

        return baseCost;
    }
}
```

**Usage:**
```csharp
var turnCost = _movementCostCalculator.CalculateTurnCost(currentRoom, nextRoom, _gameState.CurrentCharacter);
_gameState.TurnCount += turnCost;
```

---

#### 4. Pathfinding Service

**Requirement:** "Go to [room name]" command with automatic multi-room pathfinding

**Extension Point:**
```csharp
public interface IPathfindingService
{
    Task<List<Direction>> FindPathAsync(Guid fromRoomId, Guid toRoomId);
}

// Usage
var path = await _pathfindingService.FindPathAsync(_gameState.CurrentRoomId, targetRoomId);
foreach (var direction in path)
{
    await _navigationService.MoveAsync(direction);
}
```

**Algorithm:**
- A* pathfinding using `Room.Position` coordinates
- Cost function: Manhattan distance + movement cost
- Avoid rooms with high danger levels (heuristic weight)

---

#### 5. Room Discovery Radius

**Requirement:** Reveal adjacent rooms without entering them

**Extension Point:**
```csharp
public interface INavigationService
{
    // New method
    Task<List<Room>> GetAdjacentRoomsAsync(int radius = 1);
}

// Implementation
public async Task<List<Room>> GetAdjacentRoomsAsync(int radius = 1)
{
    var currentRoom = await GetCurrentRoomAsync();
    if (currentRoom == null) return new List<Room>();

    var adjacentRoomIds = currentRoom.Exits.Values;
    var rooms = new List<Room>();

    foreach (var roomId in adjacentRoomIds)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room != null)
        {
            rooms.Add(room);
            _gameState.VisitedRoomIds.Add(roomId); // Mark as "seen" (fog-of-war)
        }
    }

    return rooms;
}
```

**UI Usage:**
- Display adjacent room names on minimap
- Show "unexplored" vs "seen but not entered" distinction

---

#### 6. Contextual Room Descriptions

**Requirement:** Room descriptions change based on previous visits or world state

**Extension Point:**
```csharp
public interface IDynamicDescriptionService
{
    Task<string> GetContextualDescriptionAsync(Room room, GameState state);
}

// Implementation
public class DynamicDescriptionService : IDynamicDescriptionService
{
    public async Task<string> GetContextualDescriptionAsync(Room room, GameState state)
    {
        var baseDescription = room.Description;

        // First visit
        if (!state.VisitedRoomIds.Contains(room.Id))
        {
            return $"You enter {room.Name} for the first time.\n{baseDescription}";
        }

        // Repeat visit
        return $"You return to {room.Name}.\n{baseDescription}";
    }
}
```

**Integration:**
```csharp
var description = await _dynamicDescriptionService.GetContextualDescriptionAsync(nextRoom, _gameState);
```

---

#### 7. Movement History Tracking

**Requirement:** Track player path for backtracking, statistics, achievements

**Extension Point:**
```csharp
public class GameState
{
    // New property
    public List<MovementRecord> MovementHistory { get; set; } = new();
}

public record MovementRecord
{
    public int TurnNumber { get; init; }
    public Guid FromRoomId { get; init; }
    public Guid ToRoomId { get; init; }
    public Direction Direction { get; init; }
    public DateTime Timestamp { get; init; }
}
```

**Recording:**
```csharp
_gameState.MovementHistory.Add(new MovementRecord
{
    TurnNumber = _gameState.TurnCount,
    FromRoomId = currentRoom.Id,
    ToRoomId = nextRoom.Id,
    Direction = direction,
    Timestamp = DateTime.UtcNow
});
```

**Use Cases:**
- Backtracking command ("retrace 5 steps")
- Achievement tracking ("Explore 100 rooms")
- Replay visualization (movement map)

---

## Changelog

### v1.0.1 (2025-12-25)
**Documentation Updates:**
- Added `Related Specs` field to frontmatter header
- Updated `Last Updated` to 2025-12-25
- Added code traceability remarks to implementation files:
  - `INavigationService.cs` - interface spec reference
  - `NavigationService.cs` - service spec reference

### v0.3.3b - Ambient Condition Display (2025-12-15)
- **ADDED**: Ambient condition display on room entry
- **ADDED**: `GetRoomConditionAsync()` integration with ConditionService
- **ADDED**: Condition text formatting in `FormatRoomDescriptionAsync()`
- **UPDATED**: Room description structure to include `[AMBIENT]` section

### v0.3.3a - Movement Hazard Triggering (2025-12-10)
- **ADDED**: `TriggerOnRoomEnterAsync()` integration with HazardService
- **ADDED**: Hazard message display via InputHandler
- **ADDED**: Hazard logging on room entry
- **UPDATED**: `MoveAsync()` to process hazards before description display

### v0.3.0 - Initial NavigationService Implementation (2025-11-20)
- **ADDED**: `MoveAsync()` for directional movement
- **ADDED**: `LookAsync()` for room descriptions
- **ADDED**: `GetCurrentRoomAsync()` for room retrieval
- **ADDED**: `GetAvailableExitsAsync()` for exit enumeration
- **ADDED**: GameState integration (CurrentRoomId, TurnCount, VisitedRoomIds)
- **ADDED**: Exit validation chain (3-stage validation)
- **ADDED**: Atomic state updates
- **ADDED**: Comprehensive error handling and logging

---

## Appendix

### Related Specifications

- **SPEC-DICE-001**: Dice Pool System (not directly used, but referenced for validation patterns)
- **SPEC-HAZARD-001**: Dynamic Hazard System (movement triggers)
- **SPEC-COND-001**: Ambient Condition System (room entry effects)
- **SPEC-INTERACT-001**: Interaction System (uses `GetCurrentRoomAsync()` for object scoping)

---

### Code References

**Primary Implementation:**
- `RuneAndRust.Engine/Services/NavigationService.cs` (173 lines)

**Interface:**
- `RuneAndRust.Core/Interfaces/INavigationService.cs` (36 lines)

**Tests:**
- `RuneAndRust.Tests/Engine/NavigationServiceTests.cs` (420 lines, 18 tests)

**Data Models:**
- `RuneAndRust.Core/Entities/Room.cs`
- `RuneAndRust.Core/Enums/Direction.cs` (27 lines)
- `RuneAndRust.Core/Models/GameState.cs`

---

### Glossary

**Exit:** A directional connection from one room to another, stored as `Direction → Guid` in Room.Exits dictionary

**Fog-of-War:** System for hiding unvisited rooms from player view, tracked via GameState.VisitedRoomIds

**Turn Count:** Global counter of player actions that consume time (movement, combat, rest), incremented on successful movement

**Atomic State Update:** Modification of multiple GameState properties together without partial commits (all-or-nothing)

**Validation Chain:** Multi-stage verification process before state updates (CurrentRoomId exists → exit exists → destination exists)

**Ambient Condition:** Persistent environmental effect in a room (e.g., toxic gas, extreme cold) with passive per-turn effects

**Movement Hazard:** Trap or environmental danger triggered on room entry (e.g., pressure plate, falling debris)

---

**END OF SPECIFICATION**
