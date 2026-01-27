# Save/Load System Specification

Parent item: Specs: Systems (Specs%20Systems%202ba55eb312da80c6aa36ce6564319160.md)

> Template Version: 1.0
Last Updated: 2025-11-27
Status: Active
Specification ID: SPEC-SYSTEM-001
> 

---

## Document Control

### Version History

| Version | Date | Author | Changes | Reviewers |
| --- | --- | --- | --- | --- |
| 1.0 | 2025-11-27 | AI | Initial specification | - |

### Approval Status

- [x]  **Draft**: Initial authoring in progress
- [x]  **Review**: Ready for stakeholder review
- [x]  **Approved**: Approved for implementation
- [x]  **Active**: Currently implemented and maintained
- [ ]  **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders

- **Primary Owner**: Technical Lead
- **Design**: Data persistence strategy
- **Balance**: N/A (infrastructure system)
- **Implementation**: SaveRepository.cs, SaveData.cs
- **QA/Testing**: Save integrity verification

---

## Executive Summary

### Purpose Statement

The Save/Load System provides persistent storage for all player progress, character state, and world state data, enabling players to suspend and resume gameplay across sessions with full data integrity.

### Scope

**In Scope**:

- Character data serialization (attributes, progression, status effects)
- Equipment and inventory persistence
- World state tracking (cleared rooms, quest progress, faction standings)
- Multiple save slot management
- Database schema versioning and migrations
- Auto-save integration points

**Out of Scope**:

- Cloud save synchronization → Future enhancement
- Save file encryption → Future security enhancement
- Replay/demo recording → Separate system
- Configuration settings persistence → Settings system

### Success Criteria

- **Player Experience**: Seamless save/load with no perceived data loss
- **Technical**: Save operations complete in <500ms, load in <1000ms
- **Design**: All meaningful player choices persist across sessions
- **Balance**: No exploits via save manipulation (save scumming bounded)

---

## Related Documentation

### Dependencies

**Depends On**:

- Character System: Player character data model → `SPEC-PROGRESSION-001`
- Inventory System: Equipment and consumable data → `SPEC-SYSTEM-002`
- Quest System: Active/completed quests → `docs/01-systems/quests.md`
- Faction System: Reputation standings → `SPEC-SYSTEM-010`

**Depended Upon By**:

- All gameplay systems: Require persistent state
- Desktop UI: Save/Load screen implementation
- Auto-save triggers: Combat exit, room transitions

### Related Specifications

- `SPEC-SYSTEM-002`: Inventory & Equipment System
- `SPEC-PROGRESSION-001`: Character Progression System
- `SPEC-ECONOMY-001`: Loot & Currency System

### Implementation Documentation

- **System Docs**: `docs/01-systems/` (to be created)
- **Technical Reference**: `docs/03-technical-reference/services/` (to be created)

### Code References

- **Primary Repository**: `RuneAndRust.Persistence/SaveRepository.cs`
- **Data Model**: `RuneAndRust.Persistence/SaveData.cs`
- **UI Service**: `RuneAndRust.DesktopUI/Services/SaveGameService.cs`
- **UI ViewModel**: `RuneAndRust.DesktopUI/ViewModels/SaveLoadViewModel.cs`
- **Tests**: `RuneAndRust.Tests/SaveRepositoryTests.cs`

---

## Design Philosophy

### Design Pillars

1. **Data Integrity First**
    - **Rationale**: Lost progress destroys player trust; data corruption is unacceptable
    - **Examples**: Transactional writes, validation on load, backup before overwrite
2. **Backward Compatibility**
    - **Rationale**: Players should not lose saves when game updates
    - **Examples**: Schema migrations via ALTER TABLE, default values for new fields
3. **Minimal Performance Impact**
    - **Rationale**: Save/load should be instant from player perspective
    - **Examples**: SQLite for fast local access, lazy loading where possible

### Player Experience Goals

**Target Experience**: Invisible infrastructure - players never think about saving

**Moment-to-Moment Gameplay**:

- Auto-save occurs at natural breakpoints (room transitions, combat end)
- Manual save always available in non-combat
- Load screen shows save metadata (character, milestone, last played)

**Learning Curve**:

- **Novice** (0-2 hours): Understands "save" preserves progress
- **Intermediate** (2-10 hours): Uses manual saves strategically before risky encounters
- **Expert** (10+ hours): Manages multiple save slots for different character builds

### Design Constraints

- **Technical**: SQLite database for cross-platform compatibility
- **Gameplay**: Single save slot per character name (no duplicate names)
- **Narrative**: Save state must capture all story-relevant flags
- **Scope**: Local saves only (no cloud sync in initial release)

---

## Functional Requirements

> Completeness Checklist:
> 
> - [x]  All requirements have unique IDs (FR-[NUMBER])
> - [x]  All requirements have priority assigned
> - [x]  All requirements have acceptance criteria
> - [x]  All requirements have at least one example scenario
> - [x]  All requirements trace to design goals
> - [x]  All requirements are testable

### FR-001: Create New Save

**Priority**: Critical
**Status**: Implemented

**Description**:
System must create a new save entry in the database with all current character and world state when player initiates save with a new character name.

**Rationale**:
New saves establish the persistence foundation. Character name uniqueness prevents confusion.

**Acceptance Criteria**:

- [x]  New row inserted into `saves` table
- [x]  Character name uniqueness enforced (unique constraint)
- [x]  All SaveData properties serialized correctly
- [x]  LastSaved timestamp set to current UTC time
- [x]  JSON fields validated before insertion

**Example Scenarios**:

1. **Scenario**: Player saves new character "Bjorn"
    - **Input**: SaveData with CharacterName="Bjorn", all properties populated
    - **Expected Output**: New row in saves table, success returned
    - **Success Condition**: Row exists with correct character_name
2. **Edge Case**: Duplicate character name
    - **Input**: SaveData with CharacterName="Bjorn" when "Bjorn" already exists
    - **Expected Behavior**: Upsert behavior - existing save overwritten

**Dependencies**:

- Requires: Database connection
- Blocks: FR-002 (cannot overwrite without existing save)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Persistence/SaveRepository.cs:Save()`
- **Data Requirements**: Complete SaveData object
- **Performance Considerations**: Single INSERT operation, <100ms

---

### FR-002: Update Existing Save

**Priority**: Critical
**Status**: Implemented

**Description**:
System must update an existing save entry when player saves a character with an existing name, preserving all current state.

**Rationale**:
Overwrites enable progress updates without save slot proliferation.

**Acceptance Criteria**:

- [x]  Existing row updated via character_name match
- [x]  All columns updated atomically
- [x]  LastSaved timestamp refreshed
- [x]  No data loss during concurrent access

**Example Scenarios**:

1. **Scenario**: Player saves after defeating boss
    - **Input**: SaveData with BossDefeated=true, same CharacterName
    - **Expected Output**: Existing row updated, BossDefeated column now true
    - **Success Condition**: Only one row exists for character, boss_defeated=1

**Dependencies**:

- Requires: FR-001 (save must exist to update)

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Persistence/SaveRepository.cs:Save()` (upsert pattern)
- **Data Requirements**: Complete SaveData object with existing character name
- **Performance Considerations**: Single UPDATE operation, <100ms

---

### FR-003: Load Save by Character Name

**Priority**: Critical
**Status**: Implemented

**Description**:
System must retrieve complete save state from database given character name and reconstruct SaveData object.

**Rationale**:
Loading is the primary entry point for continuing play. Fast, complete restoration is critical.

**Acceptance Criteria**:

- [x]  SaveData object fully populated from database row
- [x]  JSON fields deserialized to proper objects
- [x]  Missing columns (from older saves) use default values
- [x]  Non-existent character returns null (not error)

**Example Scenarios**:

1. **Scenario**: Player selects "Bjorn" from save list
    - **Input**: CharacterName="Bjorn"
    - **Expected Output**: Complete SaveData with all Bjorn's progress
    - **Success Condition**: All properties match last saved values
2. **Edge Case**: Load character from pre-v0.7 save (missing specialization)
    - **Input**: Old save without specialization column
    - **Expected Behavior**: Specialization defaults to "None"

**Dependencies**:

- Requires: Database with saves table

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Persistence/SaveRepository.cs:Load()`
- **Data Requirements**: Valid character_name
- **Performance Considerations**: Single SELECT, <200ms

---

### FR-004: List All Saves (Save Info Summary)

**Priority**: High
**Status**: Implemented

**Description**:
System must provide list of all saves with metadata (name, class, specialization, milestone, last played) for save selection UI.

**Rationale**:
Players need to see save options before choosing which to load.

**Acceptance Criteria**:

- [x]  Returns List<SaveInfo> with all save entries
- [x]  SaveInfo contains: CharacterName, Class, Specialization, CurrentMilestone, LastPlayed, BossDefeated
- [x]  Sorted by LastPlayed descending (most recent first)
- [x]  Empty list returned if no saves exist (not null)

**Example Scenarios**:

1. **Scenario**: Player opens load screen
    - **Input**: GetAllSaveInfo() called
    - **Expected Output**: List with 3 saves: [{Bjorn, Warrior, Berserkr, M3}, {Sigrid, Adept, Skald, M1}, {Erik, Rogue, None, M0}]
    - **Success Condition**: All saves listed with correct metadata

**Dependencies**:

- Requires: Database with saves table

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Persistence/SaveRepository.cs:GetAllSaveInfo()`
- **Data Requirements**: None (reads all rows)
- **Performance Considerations**: Full table scan acceptable for <100 saves

---

### FR-005: Delete Save

**Priority**: Medium
**Status**: Implemented

**Description**:
System must permanently remove save entry when player requests deletion, with confirmation.

**Rationale**:
Players need ability to clean up unwanted saves. Deletion is irreversible.

**Acceptance Criteria**:

- [x]  Row deleted from saves table by character_name
- [x]  Returns success boolean
- [x]  Deleting non-existent save returns false (not error)
- [x]  UI requires confirmation before calling delete

**Example Scenarios**:

1. **Scenario**: Player deletes "Erik" save
    - **Input**: DeleteSave("Erik")
    - **Expected Output**: true, row removed
    - **Success Condition**: GetAllSaveInfo() no longer includes Erik

**Dependencies**:

- Requires: Database with saves table

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Persistence/SaveRepository.cs:Delete()`
- **Data Requirements**: Character name to delete
- **Performance Considerations**: Single DELETE, <50ms

---

### FR-006: Database Schema Migration

**Priority**: Critical
**Status**: Implemented

**Description**:
System must automatically add new columns to existing saves table when schema evolves, preserving existing data.

**Rationale**:
Game updates should not break existing saves. New features must be additive.

**Acceptance Criteria**:

- [x]  ALTER TABLE commands execute on database open
- [x]  New columns have sensible defaults
- [x]  Existing row data unchanged by migration
- [x]  Migration errors logged but don't crash application
- [x]  Duplicate ALTER attempts silently ignored

**Example Scenarios**:

1. **Scenario**: Game updates from v0.20 to v0.21 (adds stance system)
    - **Input**: Existing save without stance columns
    - **Expected Output**: New columns added: active_stance_type, stance_turns_in_current, stance_shifts_remaining
    - **Success Condition**: Load succeeds, stance defaults to "Calculated"

**Dependencies**:

- Requires: InitializeDatabase() called on repository construction

**Implementation Notes**:

- **Code Location**: `RuneAndRust.Persistence/SaveRepository.cs:InitializeDatabase()`
- **Data Requirements**: ALTER TABLE commands for each new column
- **Performance Considerations**: Migrations run once per session, <1s total

---

### FR-007: Serialize Complex Objects to JSON

**Priority**: High
**Status**: Implemented

**Description**:
System must serialize complex game objects (equipment, quests, traumas) to JSON strings for storage and deserialize on load.

**Rationale**:
SQLite has limited type support. JSON enables structured data in TEXT columns.

**Acceptance Criteria**:

- [x]  Equipment objects serialize to valid JSON
- [x]  Quest objects serialize to valid JSON
- [x]  Trauma objects serialize to valid JSON
- [x]  Faction reputation dictionaries serialize correctly
- [x]  Deserialization restores full object graphs
- [x]  Empty collections serialize to "[]" or "{}"

**Example Scenarios**:

1. **Scenario**: Save player with equipped Iron Sword
    - **Input**: Equipment{Name="Iron Sword", Type=Weapon, Might=2, Rarity=Common}
    - **Expected Output**: equipped_weapon_json = `{"Name":"Iron Sword","Type":"Weapon","Might":2,"Rarity":"Common"}`
    - **Success Condition**: JSON parses back to equivalent Equipment object
2. **Edge Case**: Null equipment
    - **Input**: player.EquippedWeapon = null
    - **Expected Behavior**: equipped_weapon_json = null (not "null" string)

**Dependencies**:

- Requires: System.Text.Json
- Requires: Serializable model classes

**Implementation Notes**:

- **Code Location**: SaveRepository save/load methods with JsonSerializer
- **Data Requirements**: All complex types must be JSON-serializable
- **Performance Considerations**: Serialization adds ~10ms per complex object

---

## System Mechanics

### Mechanic 1: Save Data Mapping

**Overview**:
The SaveData class acts as a Data Transfer Object (DTO) between the game's runtime models and the database schema.

**How It Works**:

1. Game state collected from PlayerCharacter, World, and other sources
2. State mapped to SaveData properties
3. Complex objects serialized to JSON strings
4. SaveData passed to repository for persistence
5. On load, process reverses

**Data Flow**:

```
Save Flow:
  PlayerCharacter →
  EquippedWeapon → JsonSerializer → SaveData.EquippedWeaponJson
  Inventory[] → JsonSerializer → SaveData.InventoryJson
  Attributes → SaveData.Might/Finesse/Wits/Will/Sturdiness
  → SaveRepository.Save() → SQLite saves table

Load Flow:
  SQLite saves table → SaveRepository.Load()
  → SaveData populated
  → JsonDeserialize → Equipment/Quests/Traumas
  → PlayerCharacter reconstructed

```

**Parameters**:

| Parameter | Type | Range | Default | Description | Tunable? |
| --- | --- | --- | --- | --- | --- |
| MaxInventorySize | int | 10-50 | 20 | Items in inventory JSON | Yes |
| TraumaLimit | int | 0-10 | 5 | Max traumas in array | Yes |
| JSONDepth | int | 1-5 | 3 | Nested serialization depth | No |

**Edge Cases**:

1. **Corrupted JSON**: Log error, use default value, continue load
    - **Condition**: Malformed JSON string in column
    - **Behavior**: Catch JsonException, default to empty collection
    - **Example**: inventory_json="invalid" → Inventory = []
2. **Missing required field**: Fail save operation
    - **Condition**: CharacterName is null/empty
    - **Behavior**: Throw ArgumentException
    - **Example**: SaveData{CharacterName=""} → Exception

**Related Requirements**: FR-001, FR-003, FR-007

---

### Mechanic 2: Schema Versioning

**Overview**:
Database schema evolves as features are added. Migration strategy ensures backward compatibility.

**How It Works**:

1. InitializeDatabase() creates base schema if not exists
2. ALTER TABLE commands attempt to add each new column
3. SqliteException caught and ignored if column exists
4. New saves get all columns; old saves get new columns with defaults

**Schema Evolution Example**:

```sql
-- Base schema (v0.1)
CREATE TABLE saves (
    id INTEGER PRIMARY KEY,
    character_name TEXT UNIQUE NOT NULL,
    class TEXT NOT NULL,
    ...
);

-- v0.7 migration
ALTER TABLE saves ADD COLUMN specialization TEXT DEFAULT 'None';

-- v0.21.1 migration
ALTER TABLE saves ADD COLUMN active_stance_type TEXT DEFAULT 'Calculated';
ALTER TABLE saves ADD COLUMN stance_turns_in_current INTEGER DEFAULT 0;
ALTER TABLE saves ADD COLUMN stance_shifts_remaining INTEGER DEFAULT 1;

```

**Parameters**:

| Parameter | Type | Range | Default | Description | Tunable? |
| --- | --- | --- | --- | --- | --- |
| DatabaseName | string | - | runeandrust.db | SQLite file name | No |
| MigrationOrder | string[] | - | chronological | Order of ALTER commands | No |

**Edge Cases**:

1. **Database locked**: Retry with exponential backoff
    - **Condition**: Another process has write lock
    - **Behavior**: Wait and retry up to 3 times
    - **Example**: Concurrent auto-save during manual save
2. **Disk full**: Fail gracefully with user message
    - **Condition**: SQLite write fails due to disk space
    - **Behavior**: Log error, display "Save failed - disk full"
    - **Example**: Attempting save on full drive

**Related Requirements**: FR-006

---

## State Management

### System State

**State Variables**:

| Variable | Type | Persistence | Default | Description |
| --- | --- | --- | --- | --- |
| _connectionString | string | Session | computed | SQLite connection path |
| DatabaseName | const | Permanent | runeandrust.db | Database file name |

**State Transitions**:

```
[Database Not Exist] ---InitializeDatabase()---> [Database Ready]
[Database Ready] ---Save()---> [Database Ready]
[Database Ready] ---Load()---> [Database Ready]
[Database Ready] ---Delete()---> [Database Ready]

```

### Persistence Requirements

**Must Persist**:

- Character attributes and progression: Core player investment
- Equipment and inventory: Progression rewards
- Quest state: Narrative progress
- Faction reputations: World relationship state
- Trauma/stress/corruption: Economy state

**Can Be Transient**:

- Combat state: Recreated on combat start
- UI state: Managed by viewmodels
- Cached calculations: Recomputed on load

**Save Format**:

- **Database**: SQLite `runeandrust.db`
- **Table**: `saves` with ~50 columns
- **JSON Columns**: Equipment, inventory, quests, traumas, faction rep

---

## Integration Points

### Systems This System Consumes

### Integration with Character System

**What We Use**: PlayerCharacter model for state extraction
**How We Use It**: Read all properties during save, write all during load
**Dependency Type**: Hard
**Failure Handling**: Save fails if character is null

### Integration with Quest System

**What We Use**: Quest objects for serialization
**How We Use It**: Serialize active/completed quests to JSON
**Dependency Type**: Soft
**Failure Handling**: Empty quest lists if serialization fails

### Systems That Consume This System

### Consumed By Desktop UI

**What They Use**: Save/Load/List operations
**How They Use It**: SaveLoadViewModel calls repository methods
**Stability Contract**: SaveInfo structure stable for UI binding

### Consumed By Auto-Save System

**What They Use**: Save() method
**How They Use It**: Triggered on room transition, combat end
**Stability Contract**: Save() is synchronous, returns before caller continues

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
| --- | --- | --- | --- | --- | --- | --- |
| Auto-save frequency | GameLoop | Every room | Never | Every action | Data safety vs performance | Medium |
| Max save slots | SaveRepository | Unlimited | 1 | 100 | Player flexibility | Low |
| Save timeout | SaveRepository | 5000ms | 1000 | 30000 | Reliability vs responsiveness | Low |

### Balance Targets

**Target 1: Save Performance**

- **Metric**: Time to complete save operation
- **Current**: ~200ms
- **Target**: <500ms
- **Levers**: JSON serialization, write batching

**Target 2: Load Performance**

- **Metric**: Time to complete load operation
- **Current**: ~400ms
- **Target**: <1000ms
- **Levers**: Query optimization, lazy loading

---

## Implementation Guidance

### Implementation Status

**Current State**: Complete

**Completed**:

- [x]  Core models created (SaveData, SaveInfo)
- [x]  Repository implemented (SaveRepository.cs)
- [x]  Schema migrations functional
- [x]  JSON serialization for complex objects
- [x]  UI integration complete
- [x]  Basic testing complete

### Code Architecture

**Recommended Structure**:

```
RuneAndRust.Persistence/
  ├─ SaveData.cs          // DTO for save state
  ├─ SaveInfo.cs          // Summary for UI listing
  └─ SaveRepository.cs    // Database operations

RuneAndRust.DesktopUI/
  ├─ Services/
  │   ├─ SaveGameService.cs     // Business logic layer
  │   └─ ISaveGameService.cs    // Interface
  └─ ViewModels/
      └─ SaveLoadViewModel.cs   // UI binding

```

### Implementation Checklist

**Completed**:

- [x]  SaveData DTO with all game state properties
- [x]  SaveRepository with CRUD operations
- [x]  Schema migration system
- [x]  JSON serialization for Equipment, Quests, Traumas
- [x]  SaveGameService for UI integration
- [x]  SaveLoadViewModel for MVVM binding

**Future Work**:

- [ ]  Save file backup before overwrite
- [ ]  Corruption detection and recovery
- [ ]  Save compression for large inventories
- [ ]  Cloud sync integration

---

## Testing & Verification

### Test Scenarios

### Test Case 1: Save and Load Roundtrip

**Type**: Integration

**Objective**: Verify complete save/load preserves all data

**Preconditions**:

- Empty database
- Valid SaveData with all fields populated

**Test Steps**:

1. Create SaveData with known values
2. Call Save()
3. Call Load() with same character name
4. Compare all properties

**Expected Results**:

- All primitive properties match
- All JSON-deserialized objects match
- LastSaved within 1 second of save time

**Status**: Implemented

### Test Case 2: Migration Compatibility

**Type**: Integration

**Objective**: Verify old saves load correctly after migration

**Preconditions**:

- Save from older version (missing new columns)

**Test Steps**:

1. Load save from v0.20 (pre-stance system)
2. Verify stance defaults applied
3. Save and reload
4. Verify new columns persisted

**Expected Results**:

- ActiveStanceType = "Calculated"
- StanceTurnsInCurrent = 0
- StanceShiftsRemaining = 1

**Status**: Implemented

---

## Open Questions & Future Work

### Future Enhancements

**Enhancement 1: Cloud Save Sync**

- **Rationale**: Enable cross-device play
- **Complexity**: High (requires backend infrastructure)
- **Priority**: Medium
- **Dependencies**: Account system, conflict resolution strategy

**Enhancement 2: Save Compression**

- **Rationale**: Reduce disk usage for large inventories
- **Complexity**: Low
- **Priority**: Low
- **Dependencies**: None

**Enhancement 3: Save Integrity Verification**

- **Rationale**: Detect and recover from corruption
- **Complexity**: Medium
- **Priority**: Medium
- **Dependencies**: Checksum implementation, backup strategy

### Known Limitations

**Limitation 1: Single Save Per Character Name**

- **Impact**: Players cannot have multiple saves for same character
- **Workaround**: Use different character names
- **Planned Resolution**: Add save slot numbering

---

## Appendix

### Appendix A: Terminology

| Term | Definition |
| --- | --- |
| **SaveData** | Data Transfer Object containing all persistable game state |
| **SaveInfo** | Lightweight summary of save for UI display |
| **Upsert** | Insert or update operation (character name determines which) |
| **Migration** | Schema evolution via ALTER TABLE commands |
| **DTO** | Data Transfer Object - plain data container with no behavior |

### Appendix B: Database Schema

**saves Table (v0.21.1)**:

```sql
CREATE TABLE saves (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_name TEXT UNIQUE NOT NULL,
    class TEXT NOT NULL,
    specialization TEXT DEFAULT 'None',
    current_milestone INTEGER NOT NULL,
    current_legend INTEGER NOT NULL,
    progression_points INTEGER NOT NULL,
    might INTEGER NOT NULL,
    finesse INTEGER NOT NULL,
    wits INTEGER NOT NULL,
    will INTEGER NOT NULL,
    sturdiness INTEGER NOT NULL,
    current_hp INTEGER NOT NULL,
    max_hp INTEGER NOT NULL,
    current_stamina INTEGER NOT NULL,
    max_stamina INTEGER NOT NULL,
    current_room_id INTEGER NOT NULL,
    cleared_rooms_json TEXT NOT NULL,
    puzzle_solved INTEGER NOT NULL,
    boss_defeated INTEGER NOT NULL,
    equipped_weapon_json TEXT,
    equipped_armor_json TEXT,
    inventory_json TEXT DEFAULT '[]',
    room_items_json TEXT DEFAULT '{}',
    psychic_stress INTEGER DEFAULT 0,
    corruption INTEGER DEFAULT 0,
    rooms_explored_since_rest INTEGER DEFAULT 0,
    traumas_json TEXT DEFAULT '[]',
    currency INTEGER DEFAULT 0,
    consumables_json TEXT DEFAULT '[]',
    crafting_components_json TEXT DEFAULT '{}',
    faction_reputations_json TEXT DEFAULT '{}',
    active_quests_json TEXT DEFAULT '[]',
    completed_quests_json TEXT DEFAULT '[]',
    npc_states_json TEXT DEFAULT '[]',
    current_dungeon_seed INTEGER DEFAULT 0,
    dungeons_completed INTEGER DEFAULT 0,
    current_room_string_id TEXT,
    is_procedural_dungeon INTEGER DEFAULT 0,
    active_stance_type TEXT DEFAULT 'Calculated',
    stance_turns_in_current INTEGER DEFAULT 0,
    stance_shifts_remaining INTEGER DEFAULT 1,
    last_saved TEXT NOT NULL
);

```

---

**End of Specification**