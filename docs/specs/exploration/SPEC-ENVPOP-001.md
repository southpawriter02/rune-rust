---
id: SPEC-ENVPOP-001
title: Environment Population System
version: 2.0.1
status: Implemented
last_updated: 2025-12-24
related_specs: [SPEC-DUNGEON-001, SPEC-HAZARD-001, SPEC-COND-001, SPEC-DICE-001, SPEC-ENEMY-001]
---

# SPEC-ENVPOP-001: Environment Population System

**Version:** 2.0.1 (v0.4.0 BiomeElement System + v0.3.3c Legacy)
**Status:** Implemented
**Last Updated:** 2025-12-24
**Owner:** Engine Team
**Category:** World Generation & Procedural Content

---

## Overview

The **Environment Populator** assigns biome-appropriate hazards and ambient conditions to dungeon rooms during world generation. The system uses probability-based spawning with danger-level scaling, template-driven instantiation, and biome tag filtering to ensure thematic consistency across different dungeon environments.

### Core Design Principles

1. **Biome-Based Thematic Filtering**: Only spawn hazards/conditions compatible with room biome type
2. **Danger-Scaled Probability**: Higher danger levels increase spawn chance (Safe +10% → Lethal +70%)
3. **Template-Driven Instantiation**: Hazards created from reusable `HazardTemplate` database entities
4. **Single Condition Per Room**: Rooms can have multiple hazards but only one ambient condition
5. **Non-Destructive Population**: Preserves existing hazards/conditions (additive, not replacement)

### System Boundaries

**IN SCOPE:**
- Hazard template selection and instantiation
- Ambient condition assignment
- Biome-based content filtering
- Danger-level-based spawn probability calculation
- Dungeon-wide batch population

**OUT OF SCOPE:**
- Hazard template creation (handled by seeding/admin tools)
- Hazard trigger execution (handled by HazardService)
- Condition effect application (handled by ConditionService)
- Room biome assignment (handled by DungeonGenerator)
- Enemy spawning (separate system)
- Loot/object spawning (handled by ObjectSpawner)

---

## Behaviors

### Primary Behaviors

#### 1. Room Population (`PopulateRoomAsync`)

**Trigger:** DungeonGenerator calls after creating room entities

**Process:**
1. **Extract Room Context**:
   ```csharp
   var biome = room.BiomeType;
   var dangerMultiplier = BiomeEnvironmentMapping.GetDangerMultiplier(room.DangerLevel);
   ```

2. **Calculate Spawn Probabilities**:
   ```csharp
   var hazardChance = BaseHazardChance + dangerMultiplier;   // 0.2 + (0.1 to 0.7)
   var conditionChance = BaseConditionChance + dangerMultiplier; // 0.15 + (0.1 to 0.7)
   ```

3. **Hazard Spawn Roll**:
   ```csharp
   if (RollChance(hazardChance))
   {
       await AssignHazardAsync(room, biome);
   }
   ```

4. **Condition Spawn Roll** (only if room has no existing condition):
   ```csharp
   if (room.ConditionId == null && RollChance(conditionChance))
   {
       await AssignConditionAsync(room, biome);
   }
   ```

**Outcomes:**
- **Both Spawned**: Room receives 1 hazard + 1 condition (rare, requires passing both rolls)
- **Hazard Only**: Room receives 1 hazard, condition roll failed
- **Condition Only**: Hazard roll failed, room receives 1 condition
- **Neither**: Both rolls failed (common in Safe danger rooms)

**Probability Examples:**
- **Safe + BaseHazardChance**: 0.2 + 0.1 = 0.3 (30% hazard chance)
- **Lethal + BaseHazardChance**: 0.2 + 0.7 = 0.9 (90% hazard chance)
- **Safe + BaseConditionChance**: 0.15 + 0.1 = 0.25 (25% condition chance)
- **Lethal + BaseConditionChance**: 0.15 + 0.7 = 0.85 (85% condition chance)

---

#### 2. Hazard Template Selection (`AssignHazardAsync`)

**Purpose:** Select biome-compatible hazard template and instantiate as `DynamicHazard`

**Process:**
1. **Load All Templates**:
   ```csharp
   var templates = await _hazardTemplateRepo.GetAllAsync();
   ```

2. **Filter by Biome Tags**:
   ```csharp
   var validTemplates = templates
       .Where(t => t.BiomeTags.Count == 0 || t.BiomeTags.Contains(biome))
       .ToList();
   ```
   - `BiomeTags.Count == 0`: Universal templates (valid for all biomes)
   - `BiomeTags.Contains(biome)`: Templates tagged for this specific biome

3. **Validation**:
   ```csharp
   if (validTemplates.Count == 0)
   {
       _logger.LogWarning("[Environment] No valid hazard templates for biome {Biome}", biome);
       return; // No hazard spawned
   }
   ```

4. **Random Selection**:
   ```csharp
   var template = validTemplates[_diceService.RollSingle(validTemplates.Count, "Hazard selection") - 1];
   ```

5. **Instantiate DynamicHazard**:
   ```csharp
   var hazard = new DynamicHazard
   {
       Id = Guid.NewGuid(),
       RoomId = room.Id,
       Name = template.Name,
       Description = template.Description,
       HazardType = template.HazardType,
       Trigger = template.Trigger,
       EffectScript = template.EffectScript,
       MaxCooldown = template.MaxCooldown,
       OneTimeUse = template.OneTimeUse,
       State = HazardState.Dormant  // Always starts dormant
   };
   room.Hazards.Add(hazard);
   ```

**Logging:**
```csharp
_logger.LogInformation(
    "[Environment] Assigned hazard [{HazardName}] to room {RoomName}",
    hazard.Name, room.Name);
```

---

#### 3. Ambient Condition Selection (`AssignConditionAsync`)

**Purpose:** Assign biome-compatible ambient condition to room

**Process:**
1. **Retrieve Valid Condition Types for Biome**:
   ```csharp
   var validTypes = BiomeEnvironmentMapping.GetConditionTypes(biome);
   // Example for BiomeType.Industrial: [ToxicAtmosphere, StaticField, ScorchingHeat]
   ```

2. **Load All Conditions from Database**:
   ```csharp
   var conditions = await _conditionRepo.GetAllAsync();
   ```

3. **Filter by Biome Tags AND Condition Type**:
   ```csharp
   var validConditions = conditions
       .Where(c => c.BiomeTags.Count == 0 || c.BiomeTags.Contains(biome))
       .Where(c => validTypes.Contains(c.Type))
       .ToList();
   ```

4. **Validation**:
   ```csharp
   if (validConditions.Count == 0)
   {
       _logger.LogDebug("[Environment] No valid conditions for biome {Biome}", biome);
       return; // No condition assigned
   }
   ```

5. **Random Selection & Assignment**:
   ```csharp
   var condition = validConditions[_diceService.RollSingle(validConditions.Count, "Condition selection") - 1];
   room.ConditionId = condition.Id; // FK reference only
   ```

**Logging:**
```csharp
_logger.LogInformation(
    "[Environment] Assigned condition [{ConditionName}] to room {RoomName}",
    condition.Name, room.Name);
```

---

#### 4. Dungeon-Wide Population (`PopulateDungeonAsync`)

**Purpose:** Batch process all rooms in dungeon with logging summary

**Process:**
1. **Initialization**:
   ```csharp
   var roomList = rooms.ToList();
   var hazardCount = 0;
   var conditionCount = 0;
   ```

2. **Iterate and Track**:
   ```csharp
   foreach (var room in roomList)
   {
       var hadHazard = room.Hazards.Count;
       var hadCondition = room.ConditionId != null;

       await PopulateRoomAsync(room);

       if (room.Hazards.Count > hadHazard) hazardCount++;
       if (room.ConditionId != null && !hadCondition) conditionCount++;
   }
   ```

3. **Summary Logging**:
   ```csharp
   _logger.LogInformation(
       "[Environment] Dungeon population complete. Assigned {HazardCount} hazards and {ConditionCount} conditions to {RoomCount} rooms",
       hazardCount, conditionCount, roomList.Count);
   ```

**Outcome:**
- All rooms populated with biome-appropriate content
- Summary statistics logged (e.g., "Assigned 7 hazards and 4 conditions to 10 rooms")

---

#### 5. Probability Roll (`RollChance`)

**Internal Method** - Percentile check for spawn rolls

**Implementation:**
```csharp
private bool RollChance(float chance)
{
    var roll = _diceService.RollSingle(100, "Environment population") / 100.0f;
    return roll <= chance;
}
```

**Mechanics:**
- Rolls 1d100 (1-100 range)
- Divides by 100 to get 0.01-1.00 range
- Compares to `chance` parameter (0.0-1.0 range)
- Returns `true` if `roll <= chance`

**Examples:**
- `chance = 0.30` (30%): Roll 1-30 → pass, 31-100 → fail
- `chance = 0.90` (90%): Roll 1-90 → pass, 91-100 → fail
- `chance = 0.05` (5%): Roll 1-5 → pass, 6-100 → fail

---

### Secondary Behaviors

#### 1. Biome-Condition Type Mapping (`BiomeEnvironmentMapping.GetConditionTypes`)

**Purpose:** Define thematic condition types per biome

**Mapping Table:**

| Biome Type | Valid Condition Types |
|------------|----------------------|
| **Ruin** | LowVisibility, DreadPresence |
| **Industrial** | ToxicAtmosphere, StaticField, ScorchingHeat |
| **Organic** | BlightedGround, PsychicResonance, ToxicAtmosphere |
| **Void** | PsychicResonance, DreadPresence, DeepCold |
| **Default** | LowVisibility (fallback) |

**Implementation:**
```csharp
public static List<ConditionType> GetConditionTypes(BiomeType biome) => biome switch
{
    BiomeType.Ruin => new List<ConditionType>
    {
        ConditionType.LowVisibility,
        ConditionType.DreadPresence
    },
    BiomeType.Industrial => new List<ConditionType>
    {
        ConditionType.ToxicAtmosphere,
        ConditionType.StaticField,
        ConditionType.ScorchingHeat
    },
    // ... remaining cases
    _ => new List<ConditionType> { ConditionType.LowVisibility }
};
```

**Design Rationale:**
- **Ruin**: Darkness, ancient dread (archaeological horror)
- **Industrial**: Toxic fumes, electrical hazards, heat from machinery
- **Organic**: Corruption, psychic energy, biological toxins
- **Void**: Cosmic horror, existential dread, unnatural cold

---

#### 2. Danger Level Multipliers (`BiomeEnvironmentMapping.GetDangerMultiplier`)

**Purpose:** Scale spawn probability by room danger level

**Multiplier Table:**

| Danger Level | Multiplier | Example Hazard Chance (Base 0.2) |
|--------------|------------|----------------------------------|
| **Safe** | +0.1 (10%) | 0.2 + 0.1 = 0.3 (30%) |
| **Unstable** | +0.3 (30%) | 0.2 + 0.3 = 0.5 (50%) |
| **Hostile** | +0.5 (50%) | 0.2 + 0.5 = 0.7 (70%) |
| **Lethal** | +0.7 (70%) | 0.2 + 0.7 = 0.9 (90%) |
| **Default** | +0.2 (20%) | 0.2 + 0.2 = 0.4 (40%) |

**Implementation:**
```csharp
public static float GetDangerMultiplier(DangerLevel level) => level switch
{
    DangerLevel.Safe => 0.1f,
    DangerLevel.Unstable => 0.3f,
    DangerLevel.Hostile => 0.5f,
    DangerLevel.Lethal => 0.7f,
    _ => 0.2f
};
```

**Design Rationale:**
- Safe areas: Minimal environmental threats (player learning zone)
- Lethal areas: Almost guaranteed hazards/conditions (high-risk zones)
- Exponential scaling encourages risk/reward gameplay

---

### Edge Cases

#### 1. No Matching Hazard Templates

**Scenario:** Biome filter returns empty list (no templates tagged for biome)

**Handling:**
```csharp
if (validTemplates.Count == 0)
{
    _logger.LogWarning("[Environment] No valid hazard templates for biome {Biome}", biome);
    return; // Graceful degradation - no hazard spawned
}
```

**Impact:**
- Room population continues (condition may still spawn)
- Warning logged for content team (indicates missing templates)

---

#### 2. No Matching Ambient Conditions

**Scenario:** Biome/type filter returns empty list

**Handling:**
```csharp
if (validConditions.Count == 0)
{
    _logger.LogDebug("[Environment] No valid conditions for biome {Biome}", biome);
    return; // Debug log only (less critical than hazards)
}
```

**Impact:**
- Room receives no condition
- Debug log (not warning) since conditions are optional

---

#### 3. Room Already Has Condition

**Scenario:** `room.ConditionId` is not null (pre-existing condition)

**Handling:**
```csharp
if (room.ConditionId == null && RollChance(conditionChance))
{
    await AssignConditionAsync(room, biome);
}
```

**Impact:**
- Condition spawn skipped entirely (preserves existing)
- No overwrite, no duplicate conditions

---

#### 4. Room Already Has Hazards

**Scenario:** `room.Hazards.Count > 0` before population

**Handling:**
```csharp
room.Hazards.Add(hazard); // Additive, not replacement
```

**Impact:**
- New hazard added to list (rooms can have multiple hazards)
- Pre-existing hazards preserved

---

#### 5. Empty Dungeon (Zero Rooms)

**Scenario:** `PopulateDungeonAsync(new List<Room>())`

**Handling:**
```csharp
var roomList = rooms.ToList(); // Empty list
foreach (var room in roomList) // Loop never executes
{
    // ...
}
```

**Impact:**
- No errors thrown
- Summary log: "Assigned 0 hazards and 0 conditions to 0 rooms"

---

#### 6. Chance Roll Edge Cases

**Scenario:** Chance values at boundaries (0.0, 1.0)

**Handling:**
- `chance = 0.0`: Roll 1-100 → all > 0.0 → always fails
- `chance = 1.0`: Roll 1-100 → all ≤ 1.0 → always passes (100/100 = 1.0)
- `chance > 1.0`: Not validated (undefined behavior - could always pass)

**Future:** Add validation `chance = Math.Clamp(chance, 0f, 1f);`

---

## Restrictions

### MUST Requirements

1. **MUST filter hazard templates by biome tags**
   - **Reason:** Prevent thematic violations (ice hazards in lava biome)
   - **Implementation:** EnvironmentPopulator.cs:111-113

2. **MUST filter conditions by biome tags AND condition types**
   - **Reason:** Double validation for thematic consistency
   - **Implementation:** EnvironmentPopulator.cs:152-155

3. **MUST NOT overwrite existing conditions**
   - **Reason:** Preserve manually assigned or special conditions
   - **Implementation:** EnvironmentPopulator.cs:72 (null check)

4. **MUST use danger multiplier in spawn chance calculations**
   - **Reason:** Scale difficulty with room danger level
   - **Implementation:** EnvironmentPopulator.cs:56-59

5. **MUST set hazard state to Dormant on creation**
   - **Reason:** Hazards activate on trigger, not immediately
   - **Implementation:** EnvironmentPopulator.cs:135

6. **MUST assign RoomId foreign key to hazards**
   - **Reason:** Database relationship integrity
   - **Implementation:** EnvironmentPopulator.cs:127

7. **MUST generate unique GUID for each hazard instance**
   - **Reason:** Multiple rooms can use same template (distinct entities)
   - **Implementation:** EnvironmentPopulator.cs:126

8. **MUST log hazard/condition assignments**
   - **Reason:** Debugging, content validation, analytics
   - **Implementation:** EnvironmentPopulator.cs:140-142, 166-168

---

### MUST NOT Requirements

1. **MUST NOT spawn hazards without passing probability roll**
   - **Violation Impact:** Deterministic spawning (bypasses danger scaling)
   - **Enforcement:** EnvironmentPopulator.cs:66 (chance guard)

2. **MUST NOT assign conditions without passing probability roll**
   - **Violation Impact:** Every room gets condition (overwhelming player)
   - **Enforcement:** EnvironmentPopulator.cs:72 (chance guard)

3. **MUST NOT assign multiple conditions per room**
   - **Violation Impact:** Database schema violation (Room.ConditionId is single FK)
   - **Enforcement:** EnvironmentPopulator.cs:72 (null check + single assignment)

4. **MUST NOT spawn hazards incompatible with biome**
   - **Violation Impact:** Thematic inconsistency (steam vents in frozen wasteland)
   - **Enforcement:** Biome tag filtering (lines 111-113)

5. **MUST NOT modify templates during spawning**
   - **Violation Impact:** All rooms using template get modified (shared state)
   - **Enforcement:** Copy all properties from template to new instance

6. **MUST NOT skip population on missing templates**
   - **Violation Impact:** Silent failures (rooms always empty)
   - **Enforcement:** Warning log + graceful degradation (lines 117-119)

---

## Limitations

### Numerical Limits

- **Hazards Per Room:** Unlimited (List<DynamicHazard>)
- **Conditions Per Room:** Maximum 1 (single FK constraint)
- **Hazard Templates in Database:** Unlimited (depends on storage)
- **Condition Templates in Database:** Unlimited
- **Biome Tags Per Template:** Unlimited (List<BiomeType>)

### Functional Limitations

1. **No Weighted Template Selection**
   - All valid templates have equal spawn probability
   - Future: Add `Weight` property to HazardTemplate

2. **No Hazard Quantity Control**
   - Always spawns 0 or 1 hazard per population call
   - Future: Allow `HazardCount` parameter (spawn 2-3 hazards in Lethal rooms)

3. **No Spatial Awareness**
   - Does not consider room size, exits, or adjacent rooms
   - Future: Avoid spawning movement hazards in tiny rooms

4. **No Narrative Context**
   - Does not adjust spawning based on story progression
   - Future: Allow "boss arena" flag to override probabilities

5. **No Exclusion Rules**
   - Cannot specify "never spawn X and Y together"
   - Future: Add `ConflictsWith` property to templates

6. **No Conditional Spawning**
   - Cannot spawn "bonus hazard if condition present"
   - Future: Add `RequiresCondition` property

7. **No Room Archetype Filtering**
   - Does not adjust spawning for EntryHall vs BossArena
   - Future: Add archetype-based spawn rules

---

### System-Specific Limitations

1. **Single-Pass Population**
   - Rooms are only populated once during dungeon generation
   - No dynamic spawning during gameplay
   - Future: Allow "repopulate" on dungeon reset

2. **Synchronous Database Queries**
   - `GetAllAsync()` loads ALL templates/conditions for each room
   - Inefficient for large template libraries
   - Future: Cache templates in memory

3. **No Template Versioning**
   - Templates can be modified, affecting existing dungeons
   - No snapshot of template state at spawn time
   - Future: Store TemplateVersion with hazard instance

---

## Use Cases

### USE CASE 1: Standard Room Population (Successful Spawns)

**Setup:**
```csharp
var room = new Room
{
    Id = Guid.NewGuid(),
    Name = "Toxic Storage",
    BiomeType = BiomeType.Industrial,
    DangerLevel = DangerLevel.Hostile
};

// Database contains:
// - HazardTemplate "Steam Vent" (BiomeTags: [Industrial])
// - AmbientCondition "Toxic Atmosphere" (Type: ToxicAtmosphere, BiomeTags: [Industrial])
```

**Execution:**
```csharp
await _environmentPopulator.PopulateRoomAsync(room);
```

**Internal Flow:**

1. **Calculate Spawn Chances**:
   ```csharp
   var dangerMultiplier = BiomeEnvironmentMapping.GetDangerMultiplier(DangerLevel.Hostile); // 0.5
   var hazardChance = 0.2 + 0.5 = 0.7; // 70%
   var conditionChance = 0.15 + 0.5 = 0.65; // 65%
   ```

2. **Hazard Roll**:
   ```csharp
   RollChance(0.7); // Dice rolls 45 → 0.45 <= 0.7 → PASS
   await AssignHazardAsync(room, BiomeType.Industrial);
   // Filters templates → finds "Steam Vent" → creates DynamicHazard → adds to room.Hazards
   ```

3. **Condition Roll**:
   ```csharp
   RollChance(0.65); // Dice rolls 32 → 0.32 <= 0.65 → PASS
   await AssignConditionAsync(room, BiomeType.Industrial);
   // Filters conditions → finds "Toxic Atmosphere" → sets room.ConditionId
   ```

**Assertions:**
- `room.Hazards.Count == 1`
- `room.Hazards[0].Name == "Steam Vent"`
- `room.ConditionId == toxicAtmosphereId`
- Log: "[Environment] Assigned hazard [Steam Vent] to room Toxic Storage"
- Log: "[Environment] Assigned condition [Toxic Atmosphere] to room Toxic Storage"

**Test Reference:** EnvironmentPopulatorTests.cs:122-146

---

### USE CASE 2: Failed Probability Rolls (Empty Room)

**Setup:**
```csharp
var room = new Room
{
    BiomeType = BiomeType.Ruin,
    DangerLevel = DangerLevel.Safe // Low spawn chances
};
// Safe danger: hazardChance = 0.2 + 0.1 = 0.3 (30%)
```

**Execution:**
```csharp
await _environmentPopulator.PopulateRoomAsync(room);
```

**Internal Flow:**

1. **Hazard Roll**:
   ```csharp
   RollChance(0.3); // Dice rolls 85 → 0.85 > 0.3 → FAIL
   // AssignHazardAsync() never called
   ```

2. **Condition Roll**:
   ```csharp
   RollChance(0.25); // Dice rolls 78 → 0.78 > 0.25 → FAIL
   // AssignConditionAsync() never called
   ```

**Assertions:**
- `room.Hazards.Count == 0`
- `room.ConditionId == null`
- No hazard/condition assignment logs

**Test Reference:** EnvironmentPopulatorTests.cs:148-170

---

### USE CASE 3: Biome Filter Excludes Templates

**Setup:**
```csharp
var room = new Room
{
    BiomeType = BiomeType.Void, // Void biome
    DangerLevel = DangerLevel.Lethal
};

// Database contains:
// - HazardTemplate "Pressure Plate" (BiomeTags: [Ruin])
// - HazardTemplate "Steam Vent" (BiomeTags: [Industrial])
// - NO templates with BiomeTags containing Void
```

**Execution:**
```csharp
await _environmentPopulator.PopulateRoomAsync(room);
```

**Internal Flow:**

1. **Hazard Roll**: PASSES (90% chance at Lethal danger)

2. **Template Filtering**:
   ```csharp
   var validTemplates = templates
       .Where(t => t.BiomeTags.Count == 0 || t.BiomeTags.Contains(BiomeType.Void))
       .ToList();
   // Result: Empty list (no matches)
   ```

3. **Validation Check**:
   ```csharp
   if (validTemplates.Count == 0)
   {
       _logger.LogWarning("[Environment] No valid hazard templates for biome Void");
       return; // Early exit
   }
   ```

**Assertions:**
- `room.Hazards.Count == 0` (no hazard assigned)
- Warning log: "No valid hazard templates for biome Void"

**Test Reference:** EnvironmentPopulatorTests.cs:200-220

---

### USE CASE 4: Existing Condition Preserved

**Setup:**
```csharp
var existingConditionId = Guid.NewGuid();
var room = new Room
{
    BiomeType = BiomeType.Industrial,
    DangerLevel = DangerLevel.Lethal,
    ConditionId = existingConditionId // Pre-existing condition
};
```

**Execution:**
```csharp
await _environmentPopulator.PopulateRoomAsync(room);
```

**Internal Flow:**

1. **Hazard Roll**: PASSES → hazard assigned

2. **Condition Roll Check**:
   ```csharp
   if (room.ConditionId == null && RollChance(conditionChance))
   // room.ConditionId != null → condition is FALSE → skip entire block
   ```

**Assertions:**
- `room.ConditionId == existingConditionId` (unchanged)
- No condition assignment log
- Room may still receive hazard

**Test Reference:** EnvironmentPopulatorTests.cs:248-272

---

### USE CASE 5: Dungeon-Wide Batch Population

**Setup:**
```csharp
var rooms = new List<Room>
{
    new Room { Name = "Entry Hall", BiomeType = BiomeType.Ruin, DangerLevel = DangerLevel.Safe },
    new Room { Name = "Forge", BiomeType = BiomeType.Industrial, DangerLevel = DangerLevel.Hostile },
    new Room { Name = "Organic Lab", BiomeType = BiomeType.Organic, DangerLevel = DangerLevel.Lethal }
};
```

**Execution:**
```csharp
await _environmentPopulator.PopulateDungeonAsync(rooms);
```

**Internal Flow:**

1. **Initialize Counters**:
   ```csharp
   var hazardCount = 0;
   var conditionCount = 0;
   ```

2. **Iterate Rooms**:
   ```csharp
   foreach (var room in rooms)
   {
       var hadHazard = room.Hazards.Count; // 0
       var hadCondition = room.ConditionId != null; // false

       await PopulateRoomAsync(room); // Standard population

       if (room.Hazards.Count > hadHazard) hazardCount++; // Increment if hazard added
       if (room.ConditionId != null && !hadCondition) conditionCount++; // Increment if condition added
   }
   ```

3. **Summary Log**:
   ```csharp
   _logger.LogInformation(
       "[Environment] Dungeon population complete. Assigned 2 hazards and 2 conditions to 3 rooms");
   ```

**Assertions:**
- Entry Hall: Likely 0 hazards/conditions (Safe danger, low probability)
- Forge: Likely 1 hazard (Industrial biome, Hostile danger)
- Organic Lab: Likely 1 hazard + 1 condition (Lethal danger, 90%+ chances)
- Summary log provides statistical overview

**Test Reference:** EnvironmentPopulatorTests.cs:323-362

---

### USE CASE 6: Hazard Instantiation from Template

**Setup:**
```csharp
var room = new Room { BiomeType = BiomeType.Industrial, DangerLevel = DangerLevel.Lethal };

var template = new HazardTemplate
{
    Id = Guid.NewGuid(),
    Name = "Steam Vent",
    Description = "Hot steam bursts forth",
    HazardType = HazardType.Environmental,
    Trigger = TriggerType.TurnStart,
    EffectScript = "DAMAGE:Fire:1d4",
    MaxCooldown = 2,
    OneTimeUse = false,
    BiomeTags = new List<BiomeType> { BiomeType.Industrial }
};
```

**Execution:**
```csharp
await _environmentPopulator.PopulateRoomAsync(room);
```

**Hazard Creation**:
```csharp
var hazard = new DynamicHazard
{
    Id = Guid.NewGuid(),              // NEW GUID (not template.Id)
    RoomId = room.Id,                 // FK to room
    Name = "Steam Vent",              // Copied from template
    Description = "Hot steam bursts forth", // Copied
    HazardType = HazardType.Environmental,  // Copied
    Trigger = TriggerType.TurnStart,        // Copied
    EffectScript = "DAMAGE:Fire:1d4",       // Copied
    MaxCooldown = 2,                         // Copied
    OneTimeUse = false,                      // Copied
    State = HazardState.Dormant              // ALWAYS Dormant on creation
};
room.Hazards.Add(hazard);
```

**Assertions:**
- `hazard.Id != template.Id` (unique instance)
- `hazard.RoomId == room.Id` (FK relationship)
- `hazard.State == HazardState.Dormant` (not Active)
- All template properties copied to instance

**Test Reference:** EnvironmentPopulatorTests.cs:275-317

---

## Decision Trees

### Decision Tree 1: Room Population Flow

```
PopulateRoomAsync(room) INVOKED
│
├─ EXTRACT CONTEXT
│  ├─ biome = room.BiomeType
│  ├─ dangerMultiplier = BiomeEnvironmentMapping.GetDangerMultiplier(room.DangerLevel)
│  ├─ hazardChance = BaseHazardChance + dangerMultiplier (0.2 + 0.1-0.7)
│  └─ conditionChance = BaseConditionChance + dangerMultiplier (0.15 + 0.1-0.7)
│
├─ HAZARD SPAWN ROLL
│  ├─ RollChance(hazardChance)?
│  │  ├─ YES → AssignHazardAsync(room, biome)
│  │  │         ├─ Filter templates by biome
│  │  │         ├─ Random selection
│  │  │         ├─ Instantiate DynamicHazard
│  │  │         └─ Add to room.Hazards
│  │  └─ NO → Skip hazard assignment
│  │
│  └─ CONTINUE
│
├─ CONDITION SPAWN ROLL
│  ├─ room.ConditionId == null?
│  │  ├─ YES → RollChance(conditionChance)?
│  │  │        ├─ YES → AssignConditionAsync(room, biome)
│  │  │        │        ├─ Get valid condition types for biome
│  │  │        │        ├─ Filter conditions by biome + type
│  │  │        │        ├─ Random selection
│  │  │        │        └─ Set room.ConditionId
│  │  │        └─ NO → Skip condition assignment
│  │  └─ NO → Skip condition roll entirely (preserve existing)
│  │
│  └─ CONTINUE
│
└─ RETURN room (modified in place)
```

**Outcomes:**
- Both spawned (rare): Hazard + Condition
- Hazard only: Hazard, no condition
- Condition only: No hazard, condition
- Neither: No hazard, no condition
- Existing condition preserved: Hazard may spawn, condition unchanged

---

### Decision Tree 2: Hazard Template Filtering

```
AssignHazardAsync(room, biome) INVOKED
│
├─ LOAD ALL TEMPLATES
│  └─ templates = await _hazardTemplateRepo.GetAllAsync()
│
├─ FILTER BY BIOME TAGS
│  ├─ FOR EACH template:
│  │  ├─ template.BiomeTags.Count == 0?
│  │  │  ├─ YES → INCLUDE (universal template)
│  │  │  └─ NO → template.BiomeTags.Contains(biome)?
│  │  │           ├─ YES → INCLUDE (biome match)
│  │  │           └─ NO → EXCLUDE
│  │  │
│  │  └─ RESULT: validTemplates list
│  │
│  └─ CONTINUE
│
├─ VALIDATION
│  ├─ validTemplates.Count == 0?
│  │  ├─ YES → LOG WARNING → RETURN (no hazard)
│  │  └─ NO → CONTINUE
│  │
│  └─ RANDOM SELECTION
│     ├─ Roll 1d(validTemplates.Count)
│     └─ template = validTemplates[roll - 1]
│
├─ INSTANTIATE HAZARD
│  ├─ Create new DynamicHazard (copy all properties from template)
│  ├─ Assign unique GUID
│  ├─ Set RoomId = room.Id
│  ├─ Set State = HazardState.Dormant
│  └─ Add to room.Hazards
│
└─ LOG INFO "[Environment] Assigned hazard [{HazardName}] to room {RoomName}"
```

**Key Filters:**
- Empty BiomeTags → Universal (valid for all biomes)
- BiomeTags contains current biome → Biome-specific match
- No matches → Warning log + graceful degradation

---

### Decision Tree 3: Condition Assignment Logic

```
AssignConditionAsync(room, biome) INVOKED
│
├─ GET VALID CONDITION TYPES
│  └─ validTypes = BiomeEnvironmentMapping.GetConditionTypes(biome)
│     Example: BiomeType.Industrial → [ToxicAtmosphere, StaticField, ScorchingHeat]
│
├─ LOAD ALL CONDITIONS
│  └─ conditions = await _conditionRepo.GetAllAsync()
│
├─ DOUBLE FILTER (Biome Tags AND Condition Type)
│  ├─ FOR EACH condition:
│  │  ├─ condition.BiomeTags.Count == 0 OR condition.BiomeTags.Contains(biome)?
│  │  │  ├─ YES → CONTINUE TO TYPE CHECK
│  │  │  └─ NO → EXCLUDE
│  │  │
│  │  ├─ validTypes.Contains(condition.Type)?
│  │  │  ├─ YES → INCLUDE
│  │  │  └─ NO → EXCLUDE
│  │  │
│  │  └─ RESULT: validConditions list
│  │
│  └─ CONTINUE
│
├─ VALIDATION
│  ├─ validConditions.Count == 0?
│  │  ├─ YES → LOG DEBUG → RETURN (no condition)
│  │  └─ NO → CONTINUE
│  │
│  └─ RANDOM SELECTION
│     ├─ Roll 1d(validConditions.Count)
│     └─ condition = validConditions[roll - 1]
│
├─ ASSIGN CONDITION
│  └─ room.ConditionId = condition.Id (FK reference only)
│
└─ LOG INFO "[Environment] Assigned condition [{ConditionName}] to room {RoomName}"
```

**Validation Layers:**
1. Biome tag filtering (same as hazards)
2. Condition type filtering (biome-specific valid types)
3. Empty result → Debug log (less critical than hazards)

---

## Sequence Diagrams

### Sequence Diagram 1: Successful Room Population

```
DungeonGenerator  EnvironmentPopulator  BiomeEnvMapping  HazardTemplateRepo  ConditionRepo  DiceService
      |                    |                   |                  |                 |              |
      |-- PopulateRoomAsync(room) ----------->|                   |                 |              |
      |                    |                   |                   |                 |              |
      |                    |-- GetDangerMultiplier(room.DangerLevel) -->            |              |
      |                    |<-- 0.5 (Hostile) ----------------------               |              |
      |                    |                   |                   |                 |              |
      |                    |-- Calculate hazardChance (0.2 + 0.5 = 0.7)            |              |
      |                    |-- Calculate conditionChance (0.15 + 0.5 = 0.65)       |              |
      |                    |                   |                   |                 |              |
      |                    |-- RollChance(0.7) -------------------------------------------------->|
      |                    |                   |                   |                 |              |
      |                    |                   |       [Roll 1d100 → 45]           |              |
      |                    |<-- true (0.45 <= 0.7) -------------------------------------------- |
      |                    |                   |                   |                 |              |
      |                    |-- AssignHazardAsync(room, biome) ---------------------------->      |
      |                    |                   |                   |                 |              |
      |                    |                   |-- GetAllAsync() -->|                 |              |
      |                    |                   |<-- List<HazardTemplate> (2 templates) |          |
      |                    |                   |                   |                 |              |
      |                    |       [Filter by biome → 1 valid template]           |              |
      |                    |                   |                   |                 |              |
      |                    |-- RollSingle(1, "Hazard selection") ---------------------------->    |
      |                    |<-- 1 ------------------------------------------------------------ |
      |                    |                   |                   |                 |              |
      |                    |       [Create DynamicHazard from template]           |              |
      |                    |       [Add to room.Hazards]                          |              |
      |                    |                   |                   |                 |              |
      |                    |-- RollChance(0.65) --------------------------------------------->   |
      |                    |<-- true (0.32 <= 0.65) ----------------------------------------- |
      |                    |                   |                   |                 |              |
      |                    |-- AssignConditionAsync(room, biome) -------------------------------->|
      |                    |                   |                   |                 |              |
      |                    |-- GetConditionTypes(biome) ------->  |                 |              |
      |                    |<-- [ToxicAtmosphere, StaticField, ...] |              |              |
      |                    |                   |                   |                 |              |
      |                    |                   |-- GetAllAsync() --------------------------->     |
      |                    |                   |<-- List<AmbientCondition> (3 conditions) --|     |
      |                    |                   |                   |                 |              |
      |                    |       [Filter by biome + type → 1 valid condition]   |              |
      |                    |                   |                   |                 |              |
      |                    |-- RollSingle(1, "Condition selection") ------------------------->   |
      |                    |<-- 1 ------------------------------------------------------------ |
      |                    |                   |                   |                 |              |
      |                    |       [Set room.ConditionId]                         |              |
      |                    |                   |                   |                 |              |
      |<-- room (modified) -------------------|                   |                 |              |
```

**Result:**
- Room receives 1 hazard (filtered by biome)
- Room receives 1 condition (filtered by biome + type)
- Both assignments logged

---

### Sequence Diagram 2: Failed Rolls (Empty Room)

```
EnvironmentPopulator  DiceService
         |                  |
         |-- RollChance(0.3) --------->|
         |                  |          |
         |         [Roll 1d100 → 85]   |
         |<-- false (0.85 > 0.3) ------|
         |                  |
         [Skip AssignHazardAsync()]
         |                  |
         |-- RollChance(0.25) -------->|
         |                  |          |
         |         [Roll 1d100 → 78]   |
         |<-- false (0.78 > 0.25) -----|
         |                  |
         [Skip AssignConditionAsync()]
         |
         [Return room (unmodified)]
```

**Result:**
- No hazard assigned
- No condition assigned
- Room remains empty

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `IRepository<HazardTemplate>` | Infrastructure | Hazard template entity loading |
| `IRepository<AmbientCondition>` | [SPEC-COND-001](SPEC-COND-001.md) | Condition entity loading |
| `IRepository<BiomeElement>` | Infrastructure | v0.4.0 BiomeElement definitions |
| `IElementSpawnEvaluator` | Infrastructure | v0.4.0 Spawn rule evaluation |
| `IDiceService` | [SPEC-DICE-001](SPEC-DICE-001.md) | Probability rolls, weighted random selection |
| `ILogger` | Infrastructure | Population event tracing |

### Dependents (Provides To)

| Service | Specification | Usage |
|---------|---------------|-------|
| `DungeonGenerator` | [SPEC-DUNGEON-001](SPEC-DUNGEON-001.md) | Batch room population via PopulateDungeonAsync() |
| `HazardService` | [SPEC-HAZARD-001](SPEC-HAZARD-001.md) | Consumes spawned DynamicHazard entities |
| `ConditionService` | [SPEC-COND-001](SPEC-COND-001.md) | Consumes room condition assignments |

### Cross-System Integration

### Integration Matrix

| System | Dependency Type | Integration Points | Data Flow |
|--------|----------------|-------------------|-----------|
| **HazardTemplate Repository** | Required | `GetAllAsync()` | EnvironmentPopulator → Repository (template retrieval) |
| **AmbientCondition Repository** | Required | `GetAllAsync()` | EnvironmentPopulator → Repository (condition retrieval) |
| **BiomeElement Repository** | Required (v0.4.0) | `GetByBiomeIdAsync()` | EnvironmentPopulator → Repository (element definitions) |
| **DiceService** | Required | `RollSingle()` | EnvironmentPopulator → DiceService (random selection, probability rolls) |
| **BiomeEnvironmentMapping** | Required | `GetConditionTypes()`, `GetDangerMultiplier()` | EnvironmentPopulator → Static mapping (thematic rules) |
| **DungeonGenerator** | Consumer | `PopulateDungeonAsync()` | DungeonGenerator → EnvironmentPopulator (batch population) |
| **HazardService** | Consumer | Hazard trigger execution | EnvironmentPopulator (creates hazards) → HazardService (activates them) |
| **ConditionService** | Consumer | Condition effect application | EnvironmentPopulator (assigns conditions) → ConditionService (applies effects) |

---

## Data Models

### HazardTemplate Entity

**Source:** `RuneAndRust.Core.Entities.HazardTemplate`

```csharp
public class HazardTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public HazardType HazardType { get; set; }
    public TriggerType Trigger { get; set; }
    public string EffectScript { get; set; } = string.Empty;

    public int MaxCooldown { get; set; }
    public bool OneTimeUse { get; set; }

    // Biome filtering
    public List<BiomeType> BiomeTags { get; set; } = new();
}
```

**Key Fields:**
- `BiomeTags`: Empty list = universal, populated = biome-specific

---

### AmbientCondition Entity

**Source:** `RuneAndRust.Core.Entities.AmbientCondition`

```csharp
public class AmbientCondition
{
    public Guid Id { get; set; }
    public Guid? RoomId { get; set; } // FK (optional for templates)

    public ConditionType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "grey";

    public string TickScript { get; set; } = string.Empty;
    public float TickChance { get; set; } = 1.0f;

    // Biome filtering
    public List<BiomeType> BiomeTags { get; set; } = new();
}
```

**Key Fields:**
- `Type`: ConditionType enum (ToxicAtmosphere, LowVisibility, etc.)
- `BiomeTags`: Same filtering logic as hazards

---

### DynamicHazard Entity

**Source:** `RuneAndRust.Core.Entities.DynamicHazard`

```csharp
public class DynamicHazard
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; } // FK to Room

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public HazardType HazardType { get; set; }
    public TriggerType Trigger { get; set; }
    public string EffectScript { get; set; } = string.Empty;

    public int MaxCooldown { get; set; }
    public bool OneTimeUse { get; set; }

    public HazardState State { get; set; } // Dormant, Active, Triggered, Disabled
    public int CooldownRemaining { get; set; }
}
```

**Instantiation from Template:**
- All properties copied from `HazardTemplate`
- `Id` = new GUID (unique instance)
- `RoomId` = room.Id (FK)
- `State` = HazardState.Dormant (always)

---

## Configuration

### Constants

**Base Spawn Probabilities:**
```csharp
private const float BaseHazardChance = 0.2f;      // 20%
private const float BaseConditionChance = 0.15f;  // 15%
```

**Danger Level Multipliers:**
```csharp
DangerLevel.Safe     → +0.1f (10%)
DangerLevel.Unstable → +0.3f (30%)
DangerLevel.Hostile  → +0.5f (50%)
DangerLevel.Lethal   → +0.7f (70%)
```

**Resulting Spawn Chances:**

| Danger Level | Hazard Chance | Condition Chance |
|--------------|---------------|------------------|
| **Safe** | 0.2 + 0.1 = 0.3 (30%) | 0.15 + 0.1 = 0.25 (25%) |
| **Unstable** | 0.2 + 0.3 = 0.5 (50%) | 0.15 + 0.3 = 0.45 (45%) |
| **Hostile** | 0.2 + 0.5 = 0.7 (70%) | 0.15 + 0.5 = 0.65 (65%) |
| **Lethal** | 0.2 + 0.7 = 0.9 (90%) | 0.15 + 0.7 = 0.85 (85%) |

---

## Testing

### Test Summary

**Source:** `RuneAndRust.Tests/Engine/EnvironmentPopulatorTests.cs` (427 lines)

**Test Count:** 11 tests

**Coverage Breakdown:**
- BiomeEnvironmentMapping: 6 tests
- PopulateRoomAsync(): 3 tests
- PopulateDungeonAsync(): 2 tests

**Coverage Percentage:** ~85% (main paths well covered)

---

### Test Categories

#### 1. BiomeEnvironmentMapping Tests (6 tests)

**Lines 42-54** - All biomes return non-empty condition lists
**Lines 57-65** - Ruin biome returns correct types
**Lines 68-77** - Industrial biome returns correct types
**Lines 80-89** - Organic biome returns correct types
**Lines 92-101** - Void biome returns correct types
**Lines 104-115** - Danger multipliers return correct values

---

#### 2. PopulateRoomAsync() Tests (9 tests)

**Lines 122-146** - High chance roll assigns hazard
**Lines 148-170** - Low chance roll assigns no hazard
**Lines 173-197** - Biome filter excludes incompatible templates
**Lines 200-220** - No matching templates → no hazard
**Lines 223-246** - Assigns condition when roll passes
**Lines 249-272** - Existing condition preserved (not overwritten)
**Lines 275-317** - Hazard created with all template properties

---

#### 3. PopulateDungeonAsync() Tests (2 tests)

**Lines 323-362** - Processes all rooms with biome-specific content
**Lines 365-375** - Empty room list causes no errors

---

## Domain 4 Compliance

**EnvironmentPopulator.cs:** ✅ **COMPLIANT**

No flavor text generated by this service—all descriptions come from database-stored templates which are pre-validated.

**Template Responsibility:**
- Content creators must ensure `HazardTemplate.Description` and `AmbientCondition.Description` comply with Domain 4
- EnvironmentPopulator copies descriptions verbatim (no generation)

---

## Future Extensions

### 1. Weighted Template Selection
```csharp
public class HazardTemplate
{
    public int SpawnWeight { get; set; } = 1; // Default equal probability
}

var template = WeightedRandom(validTemplates, t => t.SpawnWeight);
```

### 2. Multi-Hazard Spawning
```csharp
public async Task PopulateRoomAsync(Room room, int maxHazards = 3)
{
    for (int i = 0; i < maxHazards; i++)
    {
        if (RollChance(hazardChance / (i + 1))) // Diminishing probability
        {
            await AssignHazardAsync(room, biome);
        }
    }
}
```

### 3. Room Archetype Filtering
```csharp
public class HazardTemplate
{
    public List<RoomArchetype> ValidArchetypes { get; set; } = new();
}

var validTemplates = templates
    .Where(t => t.ValidArchetypes.Count == 0 || t.ValidArchetypes.Contains(room.Archetype))
    .ToList();
```

---

## Changelog

### v0.3.3c - Initial Environment Populator (2025-12-16)
- **ADDED**: `PopulateRoomAsync()` for biome-based hazard/condition spawning
- **ADDED**: `PopulateDungeonAsync()` for batch room processing
- **ADDED**: `BiomeEnvironmentMapping` static class for thematic rules
- **ADDED**: Danger-level-scaled spawn probabilities
- **ADDED**: Template filtering by biome tags
- **ADDED**: Hazard instantiation from HazardTemplate entities
- **ADDED**: Ambient condition assignment via FK reference

---

## Appendix

### Related Specifications

- **SPEC-DUNGEON-001**: Dungeon Generation (calls EnvironmentPopulator)
- **SPEC-HAZARD-001**: Dynamic Hazard System (consumes spawned hazards)
- **SPEC-COND-001**: Ambient Condition System (consumes spawned conditions)

---

### Code References

**Primary Implementation:**
- `RuneAndRust.Engine/Services/EnvironmentPopulator.cs` (182 lines)
- `RuneAndRust.Core/Models/BiomeEnvironmentMapping.cs` (60 lines)

**Interface:**
- `RuneAndRust.Core/Interfaces/IEnvironmentPopulator.cs` (27 lines)

**Tests:**
- `RuneAndRust.Tests/Engine/EnvironmentPopulatorTests.cs` (427 lines, 11 tests)

**Data Models:**
- `RuneAndRust.Core/Entities/HazardTemplate.cs`
- `RuneAndRust.Core/Entities/AmbientCondition.cs`
- `RuneAndRust.Core/Entities/DynamicHazard.cs`

---

## Changelog

### v2.0.1 (2025-12-24)
**Documentation Updates:**
- Added `last_updated` field to YAML frontmatter
- **FIX:** Corrected `DynamicHazard.CurrentCooldown` to `CooldownRemaining` (matches code)
- **FIX:** Updated test count from 18 to 11
- Added code traceability remarks to implementation files:
  - `IEnvironmentPopulator.cs` - interface spec reference
  - `EnvironmentPopulator.cs` - service spec reference
  - `BiomeEnvironmentMapping.cs` - mapping spec reference

---

**END OF SPECIFICATION**
