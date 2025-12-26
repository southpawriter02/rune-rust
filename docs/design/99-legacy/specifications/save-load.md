# Save/Load System — Mechanic Specification v5.0

Type: Mechanic
Description: Persistent storage for player progress, character state, and world state with SQLite backend and schema migration support.
Priority: Must-Have
Status: Review
Balance Validated: No
Document ID: AAM-SPEC-MECH-SAVELOAD-v5.0
Proof-of-Concept Flag: No
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Validated: No

## I. Core Philosophy

The Save/Load System provides persistent storage for all player progress, character state, and world state data, enabling players to suspend and resume gameplay with full data integrity.

> Consolidated from SPEC-SYSTEM-001 (Imported Game Docs / codebase reflection).
> 

**Design Pillars:**

- **Data Integrity First:** Transactional writes, validation on load, backup before overwrite
- **Backward Compatibility:** Schema migrations ensure old saves work with new versions
- **Minimal Performance Impact:** <500ms save, <1000ms load

---

## II. Core Operations

### Save Data Categories

| Category | Data Elements | Serialization |
| --- | --- | --- |
| **Character** | Attributes, HP, Stamina, Specialization, Milestone | Direct columns |
| **Equipment** | Weapon, Armor, Inventory items | JSON |
| **Progression** | Legend, PP, Traumas, Corruption, Stress | Mixed |
| **World State** | Cleared rooms, Quest progress, NPC states | JSON |
| **Faction** | Reputation standings per faction | JSON dictionary |

---

## III. Schema Migration System

### Migration Strategy

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

-- v0.21.1 migration (Stance System)
ALTER TABLE saves ADD COLUMN active_stance_type TEXT DEFAULT 'Calculated';
ALTER TABLE saves ADD COLUMN stance_turns_in_current INTEGER DEFAULT 0;
ALTER TABLE saves ADD COLUMN stance_shifts_remaining INTEGER DEFAULT 1;
```

### Migration Rules

- ALTER TABLE commands execute on database open
- New columns have sensible defaults
- Existing data unchanged by migration
- Duplicate ALTER attempts silently ignored

---

## IV. JSON Serialization

### Complex Objects

- Equipment objects → valid JSON with all properties
- Quest objects → quest ID, state, objectives
- Trauma objects → trauma type, acquisition date
- Faction reputations → dictionary {faction: value}

### Edge Cases

- **Null equipment:** stored as SQL NULL (not "null" string)
- **Empty collections:** serialize to `[]` or `{}`
- **Corrupted JSON:** log error, use default, continue load

---

## V. Database Schema (v0.21.1)

```sql
CREATE TABLE saves (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_name TEXT UNIQUE NOT NULL,
    class TEXT NOT NULL,
    specialization TEXT DEFAULT 'None',
    current_milestone INTEGER NOT NULL,
    current_legend INTEGER NOT NULL,
    progression_points INTEGER NOT NULL,
    
    -- Attributes
    might INTEGER NOT NULL,
    finesse INTEGER NOT NULL,
    wits INTEGER NOT NULL,
    will INTEGER NOT NULL,
    sturdiness INTEGER NOT NULL,
    
    -- Resources
    current_hp INTEGER NOT NULL,
    max_hp INTEGER NOT NULL,
    current_stamina INTEGER NOT NULL,
    max_stamina INTEGER NOT NULL,
    psychic_stress INTEGER DEFAULT 0,
    corruption INTEGER DEFAULT 0,
    
    -- Location
    current_room_id INTEGER NOT NULL,
    current_room_string_id TEXT,
    is_procedural_dungeon INTEGER DEFAULT 0,
    current_dungeon_seed INTEGER DEFAULT 0,
    
    -- Equipment (JSON)
    equipped_weapon_json TEXT,
    equipped_armor_json TEXT,
    inventory_json TEXT DEFAULT '[]',
    consumables_json TEXT DEFAULT '[]',
    crafting_components_json TEXT DEFAULT '{}',
    
    -- Progress (JSON)
    cleared_rooms_json TEXT NOT NULL,
    traumas_json TEXT DEFAULT '[]',
    active_quests_json TEXT DEFAULT '[]',
    completed_quests_json TEXT DEFAULT '[]',
    faction_reputations_json TEXT DEFAULT '{}',
    npc_states_json TEXT DEFAULT '[]',
    
    -- Combat State
    active_stance_type TEXT DEFAULT 'Calculated',
    stance_turns_in_current INTEGER DEFAULT 0,
    stance_shifts_remaining INTEGER DEFAULT 1,
    
    -- Meta
    currency INTEGER DEFAULT 0,
    dungeons_completed INTEGER DEFAULT 0,
    rooms_explored_since_rest INTEGER DEFAULT 0,
    puzzle_solved INTEGER NOT NULL,
    boss_defeated INTEGER NOT NULL,
    last_saved TEXT NOT NULL
);
```

---

## VI. Service Architecture

### SaveRepository

```csharp
public interface ISaveRepository
{
    // CRUD Operations
    bool Save(SaveData data);
    SaveData? Load(string characterName);
    bool Delete(string characterName);
    
    // Query
    List<SaveInfo> GetAllSaveInfo();
    bool SaveExists(string characterName);
}
```

### SaveInfo (UI Summary)

```csharp
public record SaveInfo(
    string CharacterName,
    string Class,
    string Specialization,
    int CurrentMilestone,
    DateTime LastPlayed,
    bool BossDefeated
);
```

---

## VII. Performance Targets

| Operation | Current | Target | Notes |
| --- | --- | --- | --- |
| **Save** | ~200ms | <500ms | Single INSERT/UPDATE |
| **Load** | ~400ms | <1000ms | Single SELECT + deserialization |
| **List All** | ~50ms | <200ms | Full table scan (acceptable <100 saves) |
| **Delete** | ~30ms | <50ms | Single DELETE |

---

## VIII. Auto-Save Integration

### Trigger Points

- Room transition (after entering new room)
- Combat end (victory or retreat)
- Quest completion
- Manual save request

### Restrictions

- No save during active combat
- No save during dialogue
- Auto-save overwrites same character slot

---

## IX. Integration Points

**Dependencies:**

- Character System → state extraction
- Inventory System → equipment serialization
- Quest System → progress serialization
- Faction System → reputation serialization

**Referenced By:**

- Desktop UI → Save/Load screen
- Auto-Save triggers → room transition, combat end
- Game Loop → state persistence

---

*Consolidated from SPEC-SYSTEM-001 (Save/Load System Specification) per Source Authority guidelines.*