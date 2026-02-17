---
id: SPEC-DUNGEON-001
title: Dungeon Generation System
version: 0.4.1
status: Implemented
last_updated: 2025-12-24
related_specs: [SPEC-ENVPOP-001, SPEC-TEMPLATE-001, SPEC-NAV-001, SPEC-DICE-001]
---

# SPEC-DUNGEON-001: Dungeon Generation System

> **Version:** 0.4.1 (Dynamic Room Engine)
> **Status:** Implemented
> **Service:** `DungeonGenerator`
> **Location:** `RuneAndRust.Engine/Services/DungeonGenerator.cs`

---

## Overview

The **Dungeon Generator** creates interconnected room networks using a template-based generation system. Version 0.4.0 implements the **Dynamic Room Engine** which loads room templates from the database, renders names and descriptions with variable substitution, and creates linear dungeon layouts with biome-specific content.

### Core Design Principles

1. **Template-Based Room Creation**: Rooms are instantiated from `RoomTemplate` entities with variable substitution for dynamic content
2. **Biome-Driven Generation**: `BiomeDefinition` controls available templates, room count ranges, and descriptor pools
3. **Deterministic Randomization**: Uses `IDiceService` for reproducible random selection with context logging
4. **Linear Layout (Phase 1)**: North-progression chain: EntryHall → Corridors/Chambers → BossArena
5. **Environment Integration**: Delegates hazard/condition spawning to `IEnvironmentPopulator` per-room

### System Boundaries

**IN SCOPE:**
- Template-based room generation from biome definitions
- Variable substitution for room names and descriptions
- Linear room layout with bidirectional North/South connections
- BiomeType and DangerLevel assignment
- Environment population integration
- Database persistence (room entity creation)

**OUT OF SCOPE:**
- Non-linear layout algorithms (BSP, WFC - planned for Phase 2)
- Room content generation (loot, interactables - handled by other systems)
- Enemy spawning (handled by EnvironmentPopulator)
- Save/load of dungeon state (handled by SaveManager)

---

## Dependencies

### Injected Services

| Dependency | Interface | Purpose |
|------------|-----------|---------|
| Room Repository | `IRoomRepository` | Room persistence (CRUD, clear, save) |
| Environment Populator | `IEnvironmentPopulator` | Hazard/condition spawning per room |
| Room Template Repository | `IRoomTemplateRepository` | Load room templates from database |
| Biome Definition Repository | `IBiomeDefinitionRepository` | Load biome configurations |
| Template Renderer Service | `ITemplateRendererService` | Variable substitution for names/descriptions |
| Dice Service | `IDiceService` | Deterministic random number generation |
| Logger | `ILogger<DungeonGenerator>` | Structured logging and traceability |

### Constructor Signature

```csharp
public DungeonGenerator(
    IRoomRepository roomRepository,
    IEnvironmentPopulator environmentPopulator,
    IRoomTemplateRepository roomTemplateRepository,
    IBiomeDefinitionRepository biomeDefinitionRepository,
    ITemplateRendererService templateRendererService,
    IDiceService diceService,
    ILogger<DungeonGenerator> logger)
```

### Dependency Flow

```
BiomeDefinitionRepository → BiomeDefinition
                              ↓
                         AvailableTemplates
                              ↓
RoomTemplateRepository → RoomTemplate (filtered by archetype)
                              ↓
TemplateRendererService → Rendered Name/Description
                              ↓
DiceService → Random template selection, room count
                              ↓
EnvironmentPopulator → Hazards/Conditions per room
                              ↓
RoomRepository → Persistence
```

---

## Behaviors

### Primary Behaviors

#### 1. Template-Based Dungeon Generation (`GenerateDungeonAsync`)

**Signature:**
```csharp
public async Task<Guid> GenerateDungeonAsync(string biomeId)
```

**Parameters:**
- `biomeId`: The biome identifier (e.g., `"the_roots"`)

**Returns:** The starting room GUID (EntryHall)

**Process Flow:**

```
GenerateDungeonAsync(biomeId) INVOKED
│
├─ STEP 1: Load Biome Definition
│  └─ _biomeDefinitionRepository.GetByBiomeIdAsync(biomeId)
│     └─ If null → throw InvalidOperationException
│
├─ STEP 2: Determine Room Count
│  └─ _diceService.RollSingle(MaxRoomCount - MinRoomCount + 1) + MinRoomCount - 1
│
├─ STEP 3: Clear Existing Rooms
│  └─ _roomRepository.ClearAllRoomsAsync()
│
├─ STEP 4: Generate Layout
│  └─ GenerateLinearLayoutAsync(roomCount, biome)
│     └─ Returns List<RoomLayout> with templates and positions
│
├─ STEP 5: Instantiate Rooms
│  └─ For each layout:
│     └─ InstantiateRoomFromTemplateAsync(template, position, isStartingRoom, biomeId)
│
├─ STEP 6: Link Rooms
│  └─ LinkRoomsInSequence(rooms)
│     └─ Bidirectional North/South connections
│
├─ STEP 7: Populate Environment
│  └─ For each room:
│     └─ _environmentPopulator.PopulateRoomAsync(room)
│
├─ STEP 8: Persist to Database
│  ├─ _roomRepository.AddRangeAsync(rooms)
│  └─ _roomRepository.SaveChangesAsync()
│
└─ STEP 9: Return Starting Room ID
   └─ rooms.First(r => r.IsStartingRoom).Id
```

**Logging:**
```csharp
_logger.LogInformation("[DungeonGenerator] Generating dungeon for biome: {BiomeId}", biomeId);
_logger.LogDebug("[DungeonGenerator] Loaded biome: {Name} with {TemplateCount} available templates", ...);
_logger.LogDebug("[DungeonGenerator] Generating {RoomCount} rooms (range: {Min}-{Max})", ...);
_logger.LogInformation("[DungeonGenerator] Generated dungeon with {Count} rooms. Starting room: {RoomName} ({RoomId})", ...);
```

**Outcomes:**
- **Success**: Variable room count created, linked, populated, and persisted. Returns starting room GUID.
- **Biome Not Found**: `InvalidOperationException` with message to run seeder.
- **Template Not Found**: `InvalidOperationException` if no templates match archetype.
- **Database Failure**: Exception propagates to caller.

---

#### 2. Linear Layout Generation (`GenerateLinearLayoutAsync`)

**Signature:**
```csharp
private async Task<List<RoomLayout>> GenerateLinearLayoutAsync(int roomCount, BiomeDefinition biome)
```

**Layout Algorithm:**

```
Position (0,0,0) → EntryHall (IsStartingRoom = true)
Position (0,1,0) → Corridor or Chamber (33% / 67%)
Position (0,2,0) → Corridor or Chamber
...
Position (0,N,0) → BossArena (final room)
```

**Archetype Selection:**
- **First Room**: Always `"EntryHall"`
- **Middle Rooms**: Random `"Corridor"` (33%) or `"Chamber"` (67%) via `_diceService.RollSingle(3)`
- **Last Room**: Always `"BossArena"`

**Process:**
```csharp
// 1. Place EntryHall at origin
layouts.Add(new RoomLayout { Template = entryHallTemplate, Position = (0,0,0), IsStartingRoom = true });

// 2. Generate main path northward (alternating Corridor and Chamber)
for (int i = 1; i < roomCount - 1; i++)
{
    var archetype = _diceService.RollSingle(3) == 1 ? "Corridor" : "Chamber";
    var template = await SelectTemplateByArchetypeAsync(archetype, biome);
    layouts.Add(new RoomLayout { Template = template, Position = (0, i, 0), IsStartingRoom = false });
}

// 3. Place BossArena at the end
layouts.Add(new RoomLayout { Template = bossArenaTemplate, Position = (0, roomCount-1, 0), IsStartingRoom = false });
```

**Spatial Layout (5 rooms):**
```
    BossArena (0,4,0)
         |
    Chamber (0,3,0)
         |
    Corridor (0,2,0)
         |
    Chamber (0,1,0)
         |
    EntryHall (0,0,0) ← Starting Room
```

---

#### 3. Template Selection (`SelectTemplateByArchetypeAsync`)

**Signature:**
```csharp
private async Task<RoomTemplate> SelectTemplateByArchetypeAsync(string archetype, BiomeDefinition biome)
```

**Process:**
1. Load all templates from `_roomTemplateRepository.GetAllAsync()`
2. Filter by:
   - `t.Archetype.Equals(archetype, StringComparison.OrdinalIgnoreCase)`
   - `biome.AvailableTemplates.Contains(t.TemplateId)`
3. If no candidates → throw `InvalidOperationException`
4. Select random template via `_diceService.RollSingle(candidates.Count) - 1`

**Example:**
```csharp
// Biome "the_roots" with AvailableTemplates: ["roots_entry", "roots_corridor_1", "roots_chamber_1", "roots_boss"]
// Searching for "Chamber" archetype
// Candidates: templates where Archetype == "Chamber" AND TemplateId in AvailableTemplates
```

---

#### 4. Room Instantiation (`InstantiateRoomFromTemplateAsync`)

**Signature:**
```csharp
private async Task<Room> InstantiateRoomFromTemplateAsync(
    RoomTemplate template,
    Coordinate position,
    bool isStartingRoom,
    string biomeId)
```

**Process:**
1. Load biome definition for description rendering
2. Render name: `_templateRendererService.RenderRoomName(template)`
3. Render description: `_templateRendererService.RenderRoomDescription(template, biome)`
4. Map `biomeId` → `BiomeType` enum
5. Map `template.Difficulty` → `DangerLevel` enum
6. Create `Room` entity with all properties

**Field Mapping:**

| Room Field | Source |
|------------|--------|
| `Name` | `TemplateRendererService.RenderRoomName()` |
| `Description` | `TemplateRendererService.RenderRoomDescription()` |
| `Position` | Layout coordinate |
| `IsStartingRoom` | Layout flag |
| `BiomeType` | `MapBiomeIdToBiomeType(biomeId)` |
| `DangerLevel` | `MapDifficultyToDangerLevel(template.Difficulty)` |

---

#### 5. Room Linking (`LinkRoomsInSequence`)

**Signature:**
```csharp
private void LinkRoomsInSequence(List<Room> rooms)
```

**Process:**
```csharp
for (int i = 0; i < rooms.Count - 1; i++)
{
    var currentRoom = rooms[i];
    var nextRoom = rooms[i + 1];

    currentRoom.Exits[Direction.North] = nextRoom.Id;
    nextRoom.Exits[Direction.South] = currentRoom.Id;
}
```

**Exit Configuration (5 rooms):**
- EntryHall: 1 exit (North)
- Chamber 1: 2 exits (North, South)
- Corridor: 2 exits (North, South)
- Chamber 2: 2 exits (North, South)
- BossArena: 1 exit (South)
- **Total: 8 exit references (4 bidirectional pairs)**

**Invariant:** For every `roomA.Exits[Direction.North] = roomB.Id`, there exists `roomB.Exits[Direction.South] = roomA.Id`.

---

#### 6. BiomeId to BiomeType Mapping (`MapBiomeIdToBiomeType`)

**Signature:**
```csharp
private BiomeType MapBiomeIdToBiomeType(string biomeId)
```

**Mapping:**
```csharp
return biomeId.ToLowerInvariant() switch
{
    "the_roots" => BiomeType.Industrial,
    _ => BiomeType.Industrial // Default fallback
};
```

---

#### 7. Difficulty to DangerLevel Mapping (`MapDifficultyToDangerLevel`)

**Signature:**
```csharp
private DangerLevel MapDifficultyToDangerLevel(string difficulty)
```

**Mapping:**
```csharp
return difficulty.ToLowerInvariant() switch
{
    "easy" => DangerLevel.Safe,
    "medium" => DangerLevel.Unstable,
    "hard" => DangerLevel.Hostile,
    "veryhard" => DangerLevel.Lethal,
    _ => DangerLevel.Unstable // Default fallback
};
```

---

#### 8. Opposite Direction Utility (`GetOppositeDirection`)

**Signature:**
```csharp
public static Direction GetOppositeDirection(Direction direction)
```

**Implementation:**
```csharp
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
```

**Properties:**
- **Symmetric**: `GetOppositeDirection(GetOppositeDirection(x)) == x`
- **Exhaustive**: Covers all 6 `Direction` enum values
- **Pure Function**: No side effects, deterministic

---

### Secondary Behaviors

#### 1. Legacy Test Map Wrapper (`GenerateTestMapAsync`)

**Status:** `[Obsolete]` - Use `GenerateDungeonAsync(biomeId)` instead.

**Signature:**
```csharp
[Obsolete("Use GenerateDungeonAsync(biomeId) instead. This method is deprecated in v0.4.0.")]
public async Task<Guid> GenerateTestMapAsync()
```

**Behavior:** Forwards to `GenerateDungeonAsync("the_roots")` for backwards compatibility.

---

## Restrictions

### MUST Requirements

1. **MUST load biome from database** before generating rooms
   - **Reason:** Template selection depends on `BiomeDefinition.AvailableTemplates`
   - **Enforcement:** Exception thrown if biome not found

2. **MUST use DiceService for all randomization**
   - **Reason:** Deterministic generation with context logging for debugging
   - **Enforcement:** No `Random` class usage in generator

3. **MUST render names/descriptions via TemplateRendererService**
   - **Reason:** Variable substitution requires biome descriptor pools
   - **Enforcement:** Direct template string usage prohibited

4. **MUST clear existing rooms before generation**
   - **Reason:** Prevents duplicate rooms and stale data
   - **Enforcement:** `ClearAllRoomsAsync()` call in generation flow

5. **MUST create bidirectional exits for all connections**
   - **Reason:** Navigation expects two-way traversal
   - **Enforcement:** `LinkRoomsInSequence()` creates paired exits

6. **MUST designate exactly 1 starting room** (`IsStartingRoom = true`)
   - **Reason:** Player spawn point must be unambiguous
   - **Enforcement:** Only EntryHall gets `IsStartingRoom = true`

7. **MUST assign BiomeType and DangerLevel** to all rooms
   - **Reason:** Environment population requires these values
   - **Enforcement:** Mapping functions in instantiation flow

8. **MUST persist rooms to database** before returning
   - **Reason:** NavigationService requires rooms in database
   - **Enforcement:** `AddRangeAsync()` + `SaveChangesAsync()` in generation flow

---

### MUST NOT Requirements

1. **MUST NOT use hardcoded room definitions** in production code
   - **Violation Impact:** Bypasses template system, no biome theming
   - **Enforcement:** Legacy methods marked `[Obsolete]`

2. **MUST NOT create rooms with duplicate coordinates**
   - **Violation Impact:** Layout overlap, navigation confusion
   - **Enforcement:** Linear layout generates unique Y coordinates

3. **MUST NOT create unidirectional exits**
   - **Violation Impact:** Player can enter room but cannot return (softlock)
   - **Enforcement:** `LinkRoomsInSequence()` creates bidirectional pairs

4. **MUST NOT skip environment population**
   - **Violation Impact:** Rooms have no hazards/conditions (incomplete gameplay)
   - **Enforcement:** Required loop in generation flow

5. **MUST NOT persist rooms before environment population**
   - **Violation Impact:** Hazards/conditions not on Room entities
   - **Enforcement:** Population occurs before `AddRangeAsync()`

---

## Limitations

### Current Limitations (Phase 1)

1. **Linear Layout Only**
   - Only North/South connections (no branching, no loops)
   - Future: BSP, WFC, cellular automata for complex topologies

2. **Single Biome Per Dungeon**
   - All rooms share same `BiomeType`
   - Future: Multi-biome dungeons with transition zones

3. **Fixed Archetype Sequence**
   - EntryHall → Corridors/Chambers → BossArena
   - Future: Configurable archetype pools and placement rules

4. **No Vertical Rooms**
   - All rooms at Z=0 (no Up/Down exits)
   - Future: Multi-level dungeons with stairs/elevators

5. **Not Thread-Safe**
   - Concurrent calls cause race conditions on `ClearAllRoomsAsync()`
   - Future: Add mutex or ensure single-threaded access

### Numerical Limits

| Constraint | Value | Source |
|------------|-------|--------|
| Min rooms | `BiomeDefinition.MinRoomCount` | Database |
| Max rooms | `BiomeDefinition.MaxRoomCount` | Database |
| Starting rooms | Exactly 1 | Hardcoded |
| Exit count per room | 1-2 (linear chain) | Layout algorithm |

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

    public BiomeType BiomeType { get; set; }
    public DangerLevel DangerLevel { get; set; }

    public List<RoomFeature> Features { get; set; } = new();
    public List<DynamicHazard> Hazards { get; set; } = new();
    public Guid? ConditionId { get; set; }
}
```

**Fields Set by DungeonGenerator:**
- `Id` - Auto-generated GUID
- `Name` - Rendered from template
- `Description` - Rendered from template with biome descriptors
- `Position` - Layout coordinate
- `Exits` - Populated by `LinkRoomsInSequence()`
- `IsStartingRoom` - True for EntryHall
- `BiomeType` - Mapped from biomeId
- `DangerLevel` - Mapped from template difficulty

**Fields Set by EnvironmentPopulator:**
- `Hazards` - Dynamic hazards spawned per room
- `ConditionId` - Ambient condition reference

---

### RoomLayout (Internal)

**Source:** `DungeonGenerator.RoomLayout` (private nested class)

```csharp
private class RoomLayout
{
    public required RoomTemplate Template { get; init; }
    public required Coordinate Position { get; init; }
    public required bool IsStartingRoom { get; init; }
}
```

---

### Coordinate Value Object

**Source:** `RuneAndRust.Core.ValueObjects.Coordinate`

```csharp
public record Coordinate(int X, int Y, int Z)
{
    public static readonly Coordinate Origin = new(0, 0, 0);

    public override string ToString() => $"({X}, {Y}, {Z})";

    public Coordinate Offset(int deltaX, int deltaY, int deltaZ) =>
        new(X + deltaX, Y + deltaY, Z + deltaZ);
}
```

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

---

### BiomeType Enum

**Source:** `RuneAndRust.Core.Enums.BiomeType`

```csharp
public enum BiomeType
{
    Ruin = 0,
    Industrial = 1,
    Organic = 2,
    Void = 3
}
```

---

### DangerLevel Enum

**Source:** `RuneAndRust.Core.Enums.DangerLevel`

```csharp
public enum DangerLevel
{
    Safe = 0,      // No active threats
    Unstable = 1,  // Environmental hazards
    Hostile = 2,   // Active enemies nearby
    Lethal = 3     // Immediate danger
}
```

---

## Use Cases

### UC-1: Template-Based Dungeon Generation

**Setup:**
```csharp
// BiomeDefinition "the_roots" seeded in database:
// - MinRoomCount: 5
// - MaxRoomCount: 8
// - AvailableTemplates: ["roots_entry", "roots_corridor_1", "roots_chamber_1", "roots_boss"]
```

**Execution:**
```csharp
var startingRoomId = await _dungeonGenerator.GenerateDungeonAsync("the_roots");
_gameState.CurrentRoomId = startingRoomId;
```

**Result:**
- 5-8 rooms generated based on dice roll
- Linear north-south layout
- Room names/descriptions rendered from templates
- All rooms have `BiomeType.Industrial` and appropriate `DangerLevel`
- Environment populated with hazards/conditions

---

### UC-2: New Game Initialization

**Setup:**
```csharp
// Starting a new game
await _dungeonGenerator.GenerateDungeonAsync("the_roots");
```

**Post-Conditions:**
- Database contains 5-8 room entities
- Starting room at `(0,0,0)` with `IsStartingRoom = true`
- All rooms linked bidirectionally
- Hazards/conditions spawned per room
- Player can navigate North from EntryHall

---

### UC-3: Dungeon Regeneration

**Warning:** This is a destructive operation.

**Execution:**
```csharp
// Clear old dungeon and generate new
var newStartingRoomId = await _dungeonGenerator.GenerateDungeonAsync("the_roots");
_gameState.CurrentRoomId = newStartingRoomId; // Required: old room IDs are invalid
```

**Post-Conditions:**
- All previous rooms deleted
- New rooms with new GUIDs
- Player state must be re-initialized

---

## Cross-System Integration

### Integration Matrix

| System | Dependency Type | Integration Points |
|--------|----------------|-------------------|
| **RoomRepository** | Required | `ClearAllRoomsAsync()`, `AddRangeAsync()`, `SaveChangesAsync()` |
| **EnvironmentPopulator** | Required | `PopulateRoomAsync()` per room |
| **RoomTemplateRepository** | Required | `GetAllAsync()` for template loading |
| **BiomeDefinitionRepository** | Required | `GetByBiomeIdAsync()` for biome config |
| **TemplateRendererService** | Required | `RenderRoomName()`, `RenderRoomDescription()` |
| **DiceService** | Required | `RollSingle()` for random selections |
| **NavigationService** | Consumer | Room data retrieval |
| **GameService** | Consumer | `GenerateDungeonAsync()` on new game |

---

## Testing

### Test Summary

**Source:** `RuneAndRust.Tests/Engine/DungeonGeneratorTests.cs`

**Test Categories:**
1. Room count and generation
2. Starting room designation
3. Bidirectional exit verification
4. Template integration
5. Biome configuration
6. `GetOppositeDirection()` utility

---

## Domain 4 Compliance

### Validation Status

**DungeonGenerator.cs:** Compliant (no direct content generation)

All room content is generated by `TemplateRendererService` using biome descriptor pools. Compliance is enforced at the template/descriptor level, not the generator level.

**Template Responsibility:**
- Room names rendered from `RoomTemplate.NameTemplate`
- Room descriptions rendered from `RoomTemplate.DescriptionTemplate`
- Variable substitution uses `BiomeDefinition.DescriptorCategories`

**See Also:** SPEC-TEMPLATE-001 for template/descriptor Domain 4 compliance.

---

## Related Specifications

| Spec | Relationship |
|------|-------------|
| [SPEC-TEMPLATE-001](../content/SPEC-TEMPLATE-001.md) | Template rendering system |
| [SPEC-ENVPOP-001](../environment/SPEC-ENVPOP-001.md) | Environment population |
| [SPEC-NAV-001](SPEC-NAV-001.md) | Navigation consumes room data |
| [SPEC-DICE-001](../combat/SPEC-DICE-001.md) | Dice service for randomization |

---

## Appendix A: Legacy Test Map (Deprecated)

> **Status:** `[Obsolete]` as of v0.4.0
> **Use Instead:** `GenerateDungeonAsync(biomeId)`

### Legacy Methods

The following methods are preserved for backwards compatibility but should not be used in new code:

#### GenerateTestMapAsync()

```csharp
[Obsolete("Use GenerateDungeonAsync(biomeId) instead. This method is deprecated in v0.4.0.")]
public async Task<Guid> GenerateTestMapAsync()
{
    return await GenerateDungeonAsync("the_roots");
}
```

#### CreateTestRooms() (Private, Obsolete)

Created 5 hardcoded rooms with fixed names/descriptions:
- Entry Hall (0,0,0) - Starting Room
- Rusted Corridor (0,1,0)
- Storage Chamber (1,0,0)
- Collapsed Tunnel (-1,0,0)
- The Pit (0,0,-1)

#### LinkRooms() (Private, Obsolete)

Created star topology connections:
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

### Legacy Room Definitions (Reference)

**Entry Hall:**
```
A cold, metallic chamber. The air smells of ozone and ancient dust.
Faded runes pulse weakly along the walls, their meaning lost to time.
Passages lead in several directions.
```

**Rusted Corridor:**
```
Corroded pipes line the walls of this narrow passage.
Water drips from unseen sources, leaving rust-red stains on the floor.
The air grows colder here.
```

**Storage Chamber:**
```
Broken crates and shattered containers litter this abandoned storeroom.
Whatever was kept here was either looted long ago or claimed by decay.
Dust motes drift in the pale light.
```

**Collapsed Tunnel:**
```
Rubble partially blocks this passage. The ceiling groans ominously overhead.
Cracks in the walls reveal glimpses of darkness beyond.
This area seems unstable.
```

**The Pit:**
```
A deep shaft descends into absolute darkness.
Ancient machinery clings to the walls, silent and still.
The echoes of your footsteps seem to go on forever.
```

---

## Changelog

### v0.4.1 (2025-12-24)
**Documentation Corrections (Deep Dive)**
- **REMOVED**: `IsBossRoom` property from Room entity documentation (not implemented in code)
- **REMOVED**: `ManhattanDistance()` method from Coordinate documentation (not implemented in code)
- **UPDATED**: Coordinate definition to match actual implementation (`ToString()`, `Offset()`)
- **VERIFIED**: All other spec content matches DungeonGenerator.cs implementation exactly

### v0.4.0 (2025-12-24)
**Major Rewrite: Template-Based Generation**
- **BREAKING**: Replaced hardcoded test map with template-based dynamic generation
- **ADDED**: `GenerateDungeonAsync(biomeId)` as primary generation method
- **ADDED**: BiomeDefinition + RoomTemplate integration
- **ADDED**: IRoomTemplateRepository and IBiomeDefinitionRepository dependencies
- **ADDED**: ITemplateRendererService for name/description variable substitution
- **ADDED**: IDiceService for deterministic randomization
- **ADDED**: Room count variability based on biome min/max configuration
- **ADDED**: BiomeType and DangerLevel assignment during generation
- **DEPRECATED**: `GenerateTestMapAsync()`, `CreateTestRooms()`, `LinkRooms()` marked `[Obsolete]`
- **CHANGED**: Layout from star topology (4 directions) to linear north chain
- **CHANGED**: Environment population from batch to per-room via `PopulateRoomAsync()`
- **UPDATED**: Spec to document v0.4.0 Dynamic Room Engine
- **MOVED**: Legacy v0.0.5 documentation to Appendix A

### v0.3.3c (2025-12-16)
- **ADDED**: `IEnvironmentPopulator` dependency injection
- **ADDED**: `PopulateDungeonAsync()` call before room persistence

### v0.0.5 (2025-11-22)
- **ADDED**: Initial `GenerateTestMapAsync()` for deterministic 5-room layout
- **ADDED**: `CreateTestRooms()` with hardcoded room definitions
- **ADDED**: `LinkRooms()` for bidirectional exit creation
- **ADDED**: `GetOppositeDirection()` static utility method

---

## Code References

**Primary Implementation:**
- `RuneAndRust.Engine/Services/DungeonGenerator.cs`

**Dependencies:**
- `RuneAndRust.Core/Interfaces/IRoomRepository.cs`
- `RuneAndRust.Core/Interfaces/IEnvironmentPopulator.cs`
- `RuneAndRust.Core/Interfaces/IRoomTemplateRepository.cs`
- `RuneAndRust.Core/Interfaces/IBiomeDefinitionRepository.cs`
- `RuneAndRust.Core/Interfaces/ITemplateRendererService.cs`
- `RuneAndRust.Core/Interfaces/IDiceService.cs`

**Data Models:**
- `RuneAndRust.Core/Entities/Room.cs`
- `RuneAndRust.Core/Entities/RoomTemplate.cs`
- `RuneAndRust.Core/Entities/BiomeDefinition.cs`
- `RuneAndRust.Core/ValueObjects/Coordinate.cs`
- `RuneAndRust.Core/Enums/Direction.cs`
- `RuneAndRust.Core/Enums/BiomeType.cs`
- `RuneAndRust.Core/Enums/DangerLevel.cs`

**Tests:**
- `RuneAndRust.Tests/Engine/DungeonGeneratorTests.cs`

---

**END OF SPECIFICATION**
