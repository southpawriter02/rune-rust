---
id: SPEC-SPAWN-001
title: Object Spawning System
version: 1.0.1
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-DUNGEON-001, SPEC-INTERACT-001, SPEC-DICE-001]
---

# SPEC-SPAWN-001: Object Spawning System

**Version:** 1.0.1
**Status:** Implemented
**Last Updated:** 2025-12-25
**Owner:** Engine Team
**Category:** World Generation & Procedural Content

---

## Overview

The **Object Spawner** populates dungeon rooms with interactable objects during world generation, using predefined AAM-VOICE-compliant templates to create furniture, containers, devices, inscriptions, and corpses. The system ensures variety through randomized selection while maintaining thematic consistency via biome-based type preferences.

### Core Design Principles

1. **Template-Based Generation**: All objects created from hardcoded templates (not procedural text)
2. **Three-Tier Information Reveal**: Base → Detailed → Expert descriptions for progressive discovery
3. **Biome-Aware Type Selection**: 50% bias toward biome-appropriate object types
4. **Guaranteed Variety**: 2-3 objects per room, excluding hazards (handled separately)
5. **AAM-VOICE Compliance**: All templates pre-validated for Domain 4 and narrative voice

### System Boundaries

**IN SCOPE:**
- Interactable object creation from templates
- Biome-biased type selection
- Lock difficulty assignment for containers
- Database persistence via repository pattern
- Batch room population

**OUT OF SCOPE:**
- Hazard spawning (handled by EnvironmentPopulator/HazardService)
- Loot generation (handled by LootService)
- Object examination mechanics (handled by InteractionService)
- Container searching (handled by InteractionService)
- Template content creation (hardcoded in service)

---

## Behaviors

### Primary Behaviors

#### 1. Room Object Spawning (`SpawnObjectsInRoomAsync`)

**Trigger:** Dungeon generation calls after creating room entities

**Process:**
1. **Clear Existing Objects**:
   ```csharp
   await _objectRepository.ClearRoomObjectsAsync(roomId);
   ```

2. **Determine Object Count**:
   ```csharp
   var objectCount = Random.Shared.Next(MinObjectsPerRoom, MaxObjectsPerRoom + 1); // 2-3
   ```

3. **Select Object Types** (biome-biased):
   ```csharp
   var objectTypes = SelectObjectTypes(objectCount, biome);
   ```

4. **Create Objects from Templates**:
   ```csharp
   foreach (var objectType in objectTypes)
   {
       var obj = CreateObject(roomId, objectType, biome);
       spawnedObjects.Add(obj);
   }
   ```

5. **Persist to Database**:
   ```csharp
   await _objectRepository.AddRangeAsync(spawnedObjects);
   await _objectRepository.SaveChangesAsync();
   ```

**Outcomes:**
- 2-3 `InteractableObject` entities created and persisted
- Object types biased toward biome-appropriate themes (50% probability)
- Containers have 30% chance of being locked

---

#### 2. Object Type Selection (`SelectObjectTypes`)

**Purpose:** Choose which object types to spawn with biome-aware variety

**Process:**
1. **Exclude Hazard Type**:
   ```csharp
   var availableTypes = Enum.GetValues<ObjectType>()
       .Where(t => t != ObjectType.Hazard)
       .ToList();
   // Result: [Furniture, Container, Device, Inscription, Corpse]
   ```

2. **Get Biome-Biased Types**:
   ```csharp
   var biasedTypes = GetBiomeBias(biome);
   // Example for Industrial: [Device, Container]
   ```

3. **Select Types with 50% Bias Chance**:
   ```csharp
   for (int i = 0; i < count; i++)
   {
       if (biasedTypes.Any() && Random.Shared.NextDouble() < 0.5)
       {
           selected = biasedTypes[Random.Shared.Next(biasedTypes.Count)]; // Biome-specific
       }
       else
       {
           selected = availableTypes[Random.Shared.Next(availableTypes.Count)]; // Any type
       }
       types.Add(selected);
   }
   ```

**Biome Bias Mapping:**

| Biome Type | Preferred Object Types |
|------------|----------------------|
| **Ruin** | Furniture, Inscription |
| **Industrial** | Device, Container |
| **Organic** | Corpse, Furniture |
| **Void** | Inscription, Corpse |
| **Default** | No bias (equal probability) |

**Example Output:**
- Input: 3 objects, Biome.Industrial
- Possible: [Device, Container, Furniture] (first two biased, third random)

---

#### 3. Object Creation from Template (`CreateObject`)

**Purpose:** Instantiate `InteractableObject` from hardcoded template

**Process:**
1. **Select Random Template**:
   ```csharp
   var templates = ObjectTemplates[objectType]; // 3 templates per type
   var template = templates[Random.Shared.Next(templates.Length)];
   ```

2. **Instantiate Object**:
   ```csharp
   var obj = new InteractableObject
   {
       RoomId = roomId,
       Name = template.Name,
       ObjectType = objectType,
       Description = template.BaseDescription,        // Tier 0
       DetailedDescription = template.DetailedDescription,  // Tier 1
       ExpertDescription = template.ExpertDescription,      // Tier 2
       IsContainer = objectType == ObjectType.Container,
       IsOpen = false,
       IsLocked = objectType == ObjectType.Container && Random.Shared.NextDouble() < 0.3,
       LockDifficulty = 0
   };
   ```

3. **Assign Lock Difficulty** (if locked):
   ```csharp
   if (obj.IsLocked)
   {
       obj.LockDifficulty = Random.Shared.Next(1, 4); // 1-3 net successes required
   }
   ```

**Three-Tier Description Structure:**
- **Base**: Initial observation (always visible)
- **Detailed**: Revealed on 1+ net successes during examination
- **Expert**: Revealed on 3+ net successes during examination

---

#### 4. Batch Room Population (`SpawnObjectsInRoomsAsync`)

**Purpose:** Populate multiple rooms in sequence

**Process:**
```csharp
foreach (var roomId in roomIds)
{
    var biome = (BiomeType)Random.Shared.Next(0, 4); // Random biome per room
    totalSpawned += await SpawnObjectsInRoomAsync(roomId, biome);
}
```

**Outcome:**
- All rooms populated with 2-3 objects each
- Total object count returned for logging
- Each room gets random biome assignment (if not specified)

---

### Secondary Behaviors

#### 1. Template Library (Hardcoded)

**Structure:** 5 object types × 3 templates each = 15 total templates

**Example Templates:**

**Furniture:**
```csharp
new ObjectTemplate(
    "Collapsed Table",
    "A metal table lies overturned, its surface scarred by ancient violence.",
    "Scratch marks suggest something was dragged across it long ago.",
    "The markings form a crude map, perhaps scratched by a desperate survivor.")
```

**Container:**
```csharp
new ObjectTemplate(
    "Rusted Chest",
    "A corroded metal chest sits against the wall, its lock mechanism visible.",
    "Despite the rust, the hinges still seem functional.",
    "Faint inscriptions on the lock suggest it requires a specific key.")
```

**Device:**
```csharp
new ObjectTemplate(
    "Silent Terminal",
    "A data terminal sits dormant, its screen dark and lifeless.",
    "Pressing the activation panel produces no response, but warmth radiates from within.",
    "Faded text on the casing reads: 'J.T.N. COMMAND INTERFACE - RESTRICTED'")
```

**Inscription:**
```csharp
new ObjectTemplate(
    "Faded Runes",
    "Strange symbols are carved into the wall, glowing faintly with residual energy.",
    "The characters resemble old Dvergr script, though corrupted by time.",
    "Translation reveals a warning: 'The Sleeper stirs in depths below.'")
```

**Corpse:**
```csharp
new ObjectTemplate(
    "Fallen Ranger",
    "The remains of a ranger lie slumped against the wall, equipment scattered nearby.",
    "The uniform suggests this was once a patrol member. Personal effects remain.",
    "A datapad clutched in skeletal hands contains encrypted coordinates.")
```

**Total Template Count:**
- Furniture: 3 (Collapsed Table, Shattered Cabinet, Debris Pile)
- Container: 3 (Rusted Chest, Corroded Locker, Supply Crate)
- Device: 3 (Silent Terminal, Dormant Console, Corroded Switch)
- Inscription: 3 (Faded Runes, Etched Symbols, Warning Sign)
- Corpse: 3 (Fallen Ranger, Ancient Bones, Creature Remains)

---

### Edge Cases

#### 1. Empty Room List (Zero Rooms)

**Handling:**
```csharp
foreach (var roomId in roomIds) // Empty list → loop never executes
{
    // ...
}
return 0; // Total spawned = 0
```

**Impact:** No errors, graceful no-op

---

#### 2. Container Lock Generation

**Scenario:** Container type selected

**Lock Probability:**
```csharp
IsLocked = Random.Shared.NextDouble() < 0.3; // 30% chance
```

**Lock Difficulty:**
```csharp
if (IsLocked)
{
    LockDifficulty = Random.Shared.Next(1, 4); // 1, 2, or 3 net successes
}
```

**Impact:** 30% of containers require lockpicking (WITS-based check in InteractionService)

---

#### 3. Non-Container Objects

**Scenario:** Furniture, Device, Inscription, Corpse types

**Properties:**
- `IsContainer = false`
- `IsLocked = false`
- `LockDifficulty = 0`

**Impact:** Cannot be opened/searched (examination only)

---

#### 4. Biome Parameter Not Specified

**Scenario:** Caller omits biome parameter

**Handling:**
```csharp
public async Task<int> SpawnObjectsInRoomAsync(Guid roomId, BiomeType biome = BiomeType.Ruin)
```

**Default:** `BiomeType.Ruin` used

---

#### 5. Same Template Selected Twice

**Scenario:** Room with 2-3 objects may select same template randomly

**Handling:** ALLOWED (no duplicate prevention)

**Example:** Room could have "Collapsed Table" and "Collapsed Table" (identical objects)

**Future:** Add deduplication logic to ensure unique templates per room

---

## Restrictions

### MUST Requirements

1. **MUST spawn 2-3 objects per room**
   - **Reason:** Guaranteed content for player exploration
   - **Implementation:** ObjectSpawner.cs:151

2. **MUST clear existing objects before spawning**
   - **Reason:** Prevent duplicate objects on regeneration
   - **Implementation:** ObjectSpawner.cs:148

3. **MUST assign RoomId foreign key**
   - **Reason:** Database relationship integrity
   - **Implementation:** ObjectSpawner.cs:258

4. **MUST exclude Hazard type from random selection**
   - **Reason:** Hazards placed intentionally, not randomly spawned
   - **Implementation:** ObjectSpawner.cs:206-207

5. **MUST provide three-tier descriptions**
   - **Reason:** Progressive information reveal system requirement
   - **Implementation:** All templates have Base/Detailed/Expert descriptions

6. **MUST mark containers with IsContainer flag**
   - **Reason:** InteractionService uses this for search/open validation
   - **Implementation:** ObjectSpawner.cs:264

7. **MUST set initial object state** (`IsOpen = false`)
   - **Reason:** All objects start closed/inactive
   - **Implementation:** ObjectSpawner.cs:265

8. **MUST persist objects to database**
   - **Reason:** Objects must survive room transitions
   - **Implementation:** ObjectSpawner.cs:169-170

---

### MUST NOT Requirements

1. **MUST NOT spawn hazards**
   - **Violation Impact:** Bypasses HazardService trigger logic
   - **Enforcement:** Hazard type excluded from selection (line 207)

2. **MUST NOT spawn 0 or 1 objects**
   - **Violation Impact:** Sparse rooms reduce exploration value
   - **Enforcement:** Min = 2, Max = 3 (line 151)

3. **MUST NOT spawn >3 objects**
   - **Violation Impact:** Room clutter, UI overflow
   - **Enforcement:** Random.Next(2, 4) yields 2 or 3 only

4. **MUST NOT generate procedural text**
   - **Violation Impact:** Domain 4 violations, voice inconsistency
   - **Enforcement:** All text hardcoded in templates

5. **MUST NOT skip database persistence**
   - **Violation Impact:** Objects lost on room revisit
   - **Enforcement:** SaveChangesAsync() always called (line 170)

---

## Limitations

### Numerical Limits

- **Objects Per Room:** Fixed 2-3 (not configurable)
- **Templates Per Type:** Fixed 3 (15 total templates)
- **Lock Difficulty Range:** 1-3 net successes
- **Lock Probability:** Fixed 30% for containers
- **Biome Bias Probability:** Fixed 50%

### Functional Limitations

1. **No Dynamic Template System**
   - Templates hardcoded in service code
   - Cannot add templates without recompilation
   - Future: Move to database-driven template system

2. **No Template Weighting**
   - All templates within type have equal probability
   - Cannot make rare/common variants
   - Future: Add `Weight` property to templates

3. **No Duplicate Prevention**
   - Same template can appear multiple times in one room
   - Future: Track selected templates, exclude from subsequent rolls

4. **No Spatial Awareness**
   - Does not consider room size or layout
   - Future: Adjust object count based on room size

5. **No Narrative Context**
   - Does not adjust spawning based on story progression
   - Future: Allow "boss room" flag to override types

6. **No Template Variants**
   - Each template is a single fixed description
   - No procedural variation within template
   - Future: Add {Adjective} token substitution

---

## Use Cases

### USE CASE 1: Standard Room Population

**Setup:**
```csharp
var roomId = Guid.NewGuid();
var biome = BiomeType.Industrial;
```

**Execution:**
```csharp
var count = await _objectSpawner.SpawnObjectsInRoomAsync(roomId, biome);
```

**Internal Flow:**

1. **Clear Objects**: `ClearRoomObjectsAsync(roomId)` → deletes existing
2. **Determine Count**: `Random.Next(2, 4)` → 3 objects
3. **Select Types**:
   - Roll 1: Biome bias → Device (Industrial bias)
   - Roll 2: Random → Corpse
   - Roll 3: Biome bias → Container (Industrial bias)
4. **Create Objects**:
   - Device: "Silent Terminal" template
   - Corpse: "Ancient Bones" template
   - Container: "Corroded Locker" template (30% roll → locked, difficulty 2)
5. **Persist**: 3 objects inserted

**Assertions:**
- `count == 3`
- All objects have `RoomId == roomId`
- "Corroded Locker" has `IsLocked == true`, `LockDifficulty == 2`
- All objects have 3-tier descriptions

**Test Reference:** ObjectSpawnerTests.cs:32-47

---

### USE CASE 2: Container Lock Generation

**Setup:**
```csharp
// Spawn many rooms to observe lock probability
for (int i = 0; i < 100; i++)
{
    await _objectSpawner.SpawnObjectsInRoomAsync(Guid.NewGuid());
}
```

**Expected Lock Distribution:**
- ~30% of containers locked (probability check)
- Locked containers have `LockDifficulty` of 1, 2, or 3 (uniform distribution)

**Test Reference:** ObjectSpawnerTests.cs:145-163

---

### USE CASE 3: Three-Tier Description Validation

**Setup:**
```csharp
await _objectSpawner.SpawnObjectsInRoomAsync(roomId);
var objects = await _objectRepository.GetByRoomIdAsync(roomId);
```

**Assertions:**
- All objects have `Description` (Base tier)
- All objects have `DetailedDescription` (Tier 1)
- All objects have `ExpertDescription` (Tier 2)
- No empty strings

**Test Reference:** ObjectSpawnerTests.cs:288-306

---

### USE CASE 4: Batch Room Population

**Setup:**
```csharp
var roomIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
```

**Execution:**
```csharp
var totalCount = await _objectSpawner.SpawnObjectsInRoomsAsync(roomIds);
```

**Internal Flow:**
1. Room 1: Random biome → 2 objects
2. Room 2: Random biome → 3 objects
3. Room 3: Random biome → 2 objects
4. Total: 7 objects

**Assertions:**
- `totalCount == 7`
- 3 rooms cleared
- 3 database saves

**Test Reference:** ObjectSpawnerTests.cs:187-202

---

### USE CASE 5: Biome Bias Effect

**Setup:**
```csharp
// Spawn 100 rooms with Industrial biome
for (int i = 0; i < 100; i++)
{
    await _objectSpawner.SpawnObjectsInRoomAsync(Guid.NewGuid(), BiomeType.Industrial);
}
```

**Expected Distribution:**
- ~50% of objects are Device or Container (biome-biased types)
- ~50% of objects are random (Furniture, Inscription, Corpse)

**Probability Calculation:**
- Biome bias check: 50% probability
- If passes: Select from [Device, Container] (50% each)
- If fails: Select from all 5 types (20% each)

**Test Reference:** ObjectSpawnerTests.cs:244-261 (variety test)

---

## Decision Trees

### Decision Tree 1: Object Spawning Flow

```
SpawnObjectsInRoomAsync(roomId, biome) INVOKED
│
├─ CLEAR EXISTING OBJECTS
│  └─ ClearRoomObjectsAsync(roomId)
│
├─ DETERMINE OBJECT COUNT
│  └─ objectCount = Random.Next(2, 4) → 2 or 3
│
├─ SELECT OBJECT TYPES (for each object)
│  ├─ Get biome-biased types
│  │  └─ Biome.Industrial → [Device, Container]
│  │
│  ├─ FOR EACH object (count times):
│  │  ├─ Roll Random.NextDouble() < 0.5?
│  │  │  ├─ YES → Select from biasedTypes (if any)
│  │  │  └─ NO → Select from all types (except Hazard)
│  │  │
│  │  └─ Add type to list
│  │
│  └─ RESULT: List of ObjectTypes (length = objectCount)
│
├─ CREATE OBJECTS FROM TEMPLATES (for each type)
│  ├─ Get templates for objectType (3 templates)
│  ├─ Random select template
│  ├─ Create InteractableObject
│  │  ├─ Copy template descriptions (Base/Detailed/Expert)
│  │  ├─ Set RoomId FK
│  │  ├─ Set IsContainer (if Container type)
│  │  └─ Roll lock (30% if Container)
│  │     ├─ If locked → Random.Next(1, 4) → LockDifficulty
│  │     └─ If not locked → LockDifficulty = 0
│  │
│  └─ Add to spawnedObjects list
│
├─ PERSIST TO DATABASE
│  ├─ AddRangeAsync(spawnedObjects)
│  └─ SaveChangesAsync()
│
└─ RETURN object count
```

---

### Decision Tree 2: Object Type Selection Logic

```
SelectObjectTypes(count, biome) INVOKED
│
├─ GET AVAILABLE TYPES
│  └─ All ObjectType enum values EXCEPT Hazard
│     → [Furniture, Container, Device, Inscription, Corpse]
│
├─ GET BIOME-BIASED TYPES
│  └─ GetBiomeBias(biome)
│     ├─ Biome.Ruin → [Furniture, Inscription]
│     ├─ Biome.Industrial → [Device, Container]
│     ├─ Biome.Organic → [Corpse, Furniture]
│     ├─ Biome.Void → [Inscription, Corpse]
│     └─ Default → []
│
├─ FOR EACH object to spawn (count times):
│  ├─ Roll Random.NextDouble() < 0.5?
│  │  ├─ YES → Use Biome Bias
│  │  │  ├─ biasedTypes.Any()?
│  │  │  │  ├─ YES → Random select from biasedTypes
│  │  │  │  └─ NO → Fall through to random selection
│  │  │  │
│  │  │  └─ selected = biasedTypes[Random.Next(count)]
│  │  │
│  │  └─ NO → Random Selection
│  │     └─ selected = availableTypes[Random.Next(count)]
│  │
│  └─ Add selected to result list
│
└─ RETURN list of ObjectTypes
```

**Probabilities (Industrial Biome):**
- 50% chance: Device or Container (25% each)
- 50% chance: Any of 5 types (20% each)

**Combined:**
- Device: 0.5 × 0.5 + 0.5 × 0.2 = 0.35 (35%)
- Container: 0.5 × 0.5 + 0.5 × 0.2 = 0.35 (35%)
- Furniture: 0.5 × 0.2 = 0.10 (10%)
- Inscription: 0.5 × 0.2 = 0.10 (10%)
- Corpse: 0.5 × 0.2 = 0.10 (10%)

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IInteractableObjectRepository` | Infrastructure | Object persistence (ClearRoomObjectsAsync, AddRangeAsync, SaveChangesAsync) |
| `Random.Shared` | .NET BCL | Randomization for object counts, type selection, template selection, lock generation |

### Dependents (Provides To)

| Service | Specification | Usage |
|---------|---------------|-------|
| `DungeonGenerator` | [SPEC-DUNGEON-001](SPEC-DUNGEON-001.md) | Object population during room generation |
| `InteractionService` | [SPEC-INTERACT-001](SPEC-INTERACT-001.md) | Examination and container interaction with spawned objects |

### Cross-System Integration

### Integration Matrix

| System | Dependency Type | Integration Points | Data Flow |
|--------|----------------|-------------------|-----------|
| **InteractableObjectRepository** | Required | `ClearRoomObjectsAsync()`, `AddRangeAsync()`, `SaveChangesAsync()` | ObjectSpawner → Repository (object persistence) |
| **DungeonGenerator** | Consumer | Calls after room creation | DungeonGenerator → ObjectSpawner (batch population) |
| **InteractionService** | Consumer | Examination, container search | ObjectSpawner (creates objects) → InteractionService (interacts with them) |

---

## Data Models

### InteractableObject Entity

**Source:** `RuneAndRust.Core.Entities.InteractableObject`

```csharp
public class InteractableObject
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; } // FK to Room

    public string Name { get; set; } = string.Empty;
    public ObjectType ObjectType { get; set; }

    // Three-tier progressive reveal
    public string Description { get; set; } = string.Empty;          // Base (0 successes)
    public string DetailedDescription { get; set; } = string.Empty;  // Tier 1 (1+ successes)
    public string ExpertDescription { get; set; } = string.Empty;    // Tier 2 (3+ successes)

    // Container properties
    public bool IsContainer { get; set; }
    public bool IsOpen { get; set; }
    public bool IsLocked { get; set; }
    public int LockDifficulty { get; set; }

    // Examination state
    public int HighestExaminationTier { get; set; }
    public bool HasBeenExamined { get; set; }
}
```

---

### ObjectType Enum

**Source:** `RuneAndRust.Core.Enums.ObjectType`

```csharp
public enum ObjectType
{
    Furniture,   // Tables, cabinets, debris
    Container,   // Chests, lockers, crates (searchable)
    Device,      // Terminals, consoles, switches
    Inscription, // Runes, symbols, signs
    Corpse,      // Remains, bones, carcasses
    Hazard       // EXCLUDED from random spawning
}
```

---

### ObjectTemplate Record

**Internal to ObjectSpawner:**

```csharp
private record ObjectTemplate(
    string Name,
    string BaseDescription,
    string DetailedDescription,
    string ExpertDescription
);
```

---

## Configuration

### Constants

```csharp
private const int MinObjectsPerRoom = 2;
private const int MaxObjectsPerRoom = 3;
```

**Lock Probability:**
```csharp
const float LockChance = 0.3f; // 30%
```

**Biome Bias Probability:**
```csharp
const float BiomeBiasChance = 0.5f; // 50%
```

**Lock Difficulty Range:**
```csharp
LockDifficulty = Random.Shared.Next(1, 4); // 1, 2, or 3
```

---

## Testing

### Test Summary

**Source:** `RuneAndRust.Tests/Engine/ObjectSpawnerTests.cs` (310 lines)

**Test Count:** 16 tests

**Coverage Breakdown:**
- SpawnObjectsInRoomAsync(): 10 tests
- SpawnObjectsInRoomsAsync(): 3 tests
- Variety Tests: 2 tests
- Description Quality: 2 tests

**Coverage Percentage:** ~85%

---

### Test Categories

#### 1. SpawnObjectsInRoomAsync() Tests (10 tests)

**Lines 32-47** - Spawns minimum 2 objects
**Lines 50-65** - Spawns maximum 3 objects (verified over 50 iterations)
**Lines 68-78** - Clears existing objects before spawning
**Lines 81-91** - Saves changes to database
**Lines 94-108** - Objects have correct RoomId FK
**Lines 111-125** - Objects have non-empty names
**Lines 128-142** - Objects have descriptions
**Lines 145-163** - Containers marked correctly
**Lines 166-180** - Accepts all biome types

#### 2. SpawnObjectsInRoomsAsync() Tests (3 tests)

**Lines 187-202** - Spawns in all rooms (batch)
**Lines 205-224** - Returns correct total count
**Lines 227-237** - Empty list returns zero

#### 3. Variety Tests (2 tests)

**Lines 244-261** - Produces varied object types (>2 types over 50 rooms)
**Lines 264-281** - Produces varied object names (>5 unique names over 50 rooms)

#### 4. Description Quality (2 tests)

**Lines 288-306** - All objects have three-tier descriptions

---

## Domain 4 Compliance

**ObjectSpawner.cs:** ✅ **COMPLIANT**

All templates pre-validated for Domain 4:

**Compliant Examples:**
- ✅ "corroded metal chest" (qualitative state)
- ✅ "faintly glowing" (qualitative intensity)
- ✅ "skeletal remains" (descriptive, not measured)
- ✅ "ancient violence" (temporal vagueness)

**No Forbidden Patterns:**
- ❌ No precision measurements ("12.5 kg chest")
- ❌ No technical jargon ("CPU at 47% capacity")
- ❌ No modern units ("5 meters tall")

---

## Future Extensions

### 1. Database-Driven Templates
```csharp
public class ObjectTemplateEntity
{
    public Guid Id { get; set; }
    public ObjectType Type { get; set; }
    public string Name { get; set; }
    public string BaseDescription { get; set; }
    public string DetailedDescription { get; set; }
    public string ExpertDescription { get; set; }
    public int Weight { get; set; } = 1;
}
```

### 2. Template Deduplication
```csharp
var usedTemplates = new HashSet<string>();
while (spawnedObjects.Count < objectCount)
{
    var template = SelectTemplate(objectType);
    if (!usedTemplates.Contains(template.Name))
    {
        usedTemplates.Add(template.Name);
        spawnedObjects.Add(CreateObject(template));
    }
}
```

### 3. Room-Size-Based Object Count
```csharp
var objectCount = room.Size switch
{
    RoomSize.Small => Random.Next(1, 3),  // 1-2 objects
    RoomSize.Medium => Random.Next(2, 4), // 2-3 objects
    RoomSize.Large => Random.Next(3, 6),  // 3-5 objects
    _ => Random.Next(2, 4)
};
```

---

## Changelog

### v1.0.1 (2025-12-25)
**Documentation Updates:**
- Updated `last_updated` to 2025-12-25
- **FIX:** Corrected test count from 17 to 16
- Added code traceability remarks to implementation:
  - `ObjectSpawner.cs` - service spec reference

### v1.0.0 - Initial ObjectSpawner Implementation (2025-11-25)
- **ADDED**: `SpawnObjectsInRoomAsync()` for single room population
- **ADDED**: `SpawnObjectsInRoomsAsync()` for batch population
- **ADDED**: 15 hardcoded templates (5 types × 3 templates)
- **ADDED**: Biome-based type selection bias (50% probability)
- **ADDED**: Container lock generation (30% probability)
- **ADDED**: Three-tier progressive description system

---

## Appendix

### Related Specifications

- **SPEC-INTERACT-001**: Interaction System (consumes spawned objects)
- **SPEC-DUNGEON-001**: Dungeon Generation (calls ObjectSpawner)
- **SPEC-ENVPOP-001**: Environment Population (hazard spawning, parallel system)

---

### Code References

**Primary Implementation:**
- `RuneAndRust.Engine/Services/ObjectSpawner.cs` (289 lines)

**Tests:**
- `RuneAndRust.Tests/Engine/ObjectSpawnerTests.cs` (310 lines, 16 tests)

**Data Models:**
- `RuneAndRust.Core/Entities/InteractableObject.cs`
- `RuneAndRust.Core/Enums/ObjectType.cs`

---

**END OF SPECIFICATION**
