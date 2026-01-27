# Implementation Summary: v0.34.1 - Companion System Database Schema

**Version**: v0.34.1
**Date**: 2025-11-16
**Status**: ✅ Complete
**Timeline**: Database foundation for NPC Companion System

---

## Summary

Implemented complete database architecture for the NPC Companion System, including:
- 5 new tables for companion data and progression
- 6 recruitable companion definitions with complete stat blocks
- 18 companion abilities (3 per companion)
- All seeded automatically on database initialization

---

## Changes Made

### 1. Database Schema (`RuneAndRust.Persistence/SaveRepository.cs`)

#### New Method: `CreateCompanionTables(SqliteConnection connection)` (Lines 1002-1192)

Created 5 new tables:

**Companions** - Master definition table
- 6 recruitable NPCs with stats, backgrounds, faction requirements
- Columns: companion_id, companion_name, display_name, archetype, faction_affiliation, background_summary, personality_traits, recruitment_location, required_faction, required_reputation_tier, required_reputation_value, recruitment_quest_id, base stats (MIGHT/FINESSE/STURDINESS/WITS/WILL), base_max_hp, base_defense, base_soak, resource_type, base_max_resource, combat_role, default_stance, starting_abilities, personal_quest_id, personal_quest_title

**Characters_Companions** - Per-character party membership
- Tracks which companions each player has recruited
- Columns: character_companion_id, character_id, companion_id, is_recruited, recruited_at, is_in_party, current_hp, current_resource, is_incapacitated, current_stance
- Foreign keys: character_id → saves(id), companion_id → Companions(companion_id)
- UNIQUE constraint on (character_id, companion_id)

**Companion_Progression** - Level and equipment tracking
- Companion leveling, stat overrides, equipment slots
- Columns: progression_id, character_id, companion_id, current_level, current_legend, legend_to_next_level, override stats, equipped_weapon_id, equipped_armor_id, equipped_accessory_id, unlocked_abilities
- Foreign keys: character_id → saves(id), companion_id → Companions(companion_id)

**Companion_Quests** - Personal questlines
- Tracks companion personal quest state per character
- Columns: companion_quest_id, character_id, companion_id, quest_id, is_unlocked, is_started, is_completed, loyalty_reward, timestamps
- Foreign keys: character_id → saves(id), companion_id → Companions(companion_id)

**Companion_Abilities** - Ability definitions
- Separate from player Abilities table (different schema)
- Columns: ability_id, ability_name, owner, description, resource_cost_type, resource_cost, duration_turns, range_tiles, target_type, range_type, damage_type, special_effects, conditions, ability_category

#### Indices Created (8 total)
- `idx_companions_faction` - Faction lookup
- `idx_companions_location` - Location-based queries
- `idx_char_companions_character` - Character's companions
- `idx_char_companions_party` - Active party lookup
- `idx_companion_prog_character` - Progression by character
- `idx_companion_quests_char` - Quests by character
- `idx_companion_quests_companion` - Quests by companion
- `idx_companion_abilities_owner` - Abilities by owner

#### New Method: `SeedCompanionData(SqliteConnection connection)` (Lines 1194-1470)

Seeded 6 companions:

1. **Kára Ironbreaker** (34001) - Warrior, Iron-Bane, Tank
   - Stats: MIGHT 14, FINESSE 10, STURDINESS 15, WITS 9, WILL 8
   - HP: 45, Defense: 12, Soak: 3, Stamina: 120
   - Abilities: Shield Bash, Taunt, Purification Strike

2. **Finnr the Rust-Sage** (34002) - Mystic, Jötun-Reader, Support
   - Stats: MIGHT 8, FINESSE 10, STURDINESS 9, WITS 15, WILL 14
   - HP: 28, Defense: 10, Soak: 1, Aether Pool: 150
   - Abilities: Aetheric Bolt, Data Analysis, Runic Shield

3. **Bjorn Scrap-Hand** (34003) - Adept, Rust-Clan, Utility
   - Stats: MIGHT 11, FINESSE 12, STURDINESS 12, WITS 14, WILL 9
   - HP: 35, Defense: 11, Soak: 2, Stamina: 110
   - Abilities: Improvised Repair, Scrap Grenade, Resourceful (Passive)

4. **Valdis the Forlorn-Touched** (34004) - Mystic, Independent, DPS
   - Stats: MIGHT 7, FINESSE 9, STURDINESS 7, WITS 12, WILL 16
   - HP: 24, Defense: 9, Soak: 0, Aether Pool: 180
   - Abilities: Spirit Bolt, Forlorn Whisper, Fragile Mind (Passive)

5. **Runa Shield-Sister** (34005) - Warrior, Independent, Tank
   - Stats: MIGHT 13, FINESSE 11, STURDINESS 16, WITS 10, WILL 9
   - HP: 50, Defense: 13, Soak: 4, Stamina: 130
   - Abilities: Defensive Stance, Interpose, Shield Wall

6. **Einar the God-Touched** (34006) - Warrior, God-Sleeper, DPS
   - Stats: MIGHT 16, FINESSE 10, STURDINESS 13, WITS 8, WILL 11
   - HP: 42, Defense: 11, Soak: 2, Stamina: 140
   - Abilities: Berserker Rage, Jötun Attunement (Passive), Reckless Strike

#### Integration Point
- Added `CreateCompanionTables(connection);` call in `InitializeDatabase()` method (Line 149)
- Tables created automatically when SaveRepository is instantiated

---

## 2. Test Suite (`RuneAndRust.Tests/CompanionDatabaseTests.cs`)

Created comprehensive test suite with 13 tests:

**Schema Tests:**
- `CompanionTables_AfterInitialization_AllTablesExist` - Verifies 5 tables created
- `Companions_Indices_AreCreated` - Verifies all indices exist

**Data Seeding Tests:**
- `Companions_AfterSeeding_HasSixCompanions` - Verifies 6 companions seeded
- `CompanionAbilities_AfterSeeding_Has18Abilities` - Verifies 18 abilities seeded
- `Companions_SeededData_ContainsKaraIronbreaker` - Validates specific companion data

**Data Integrity Tests:**
- `Companions_WithFactionRequirements_HasCorrectData` - Validates faction requirements
- `CompanionAbilities_ForKara_HasThreeAbilities` - Validates ability assignment
- `Companions_Archetypes_CorrectDistribution` - Validates 3 Warriors, 1 Adept, 2 Mystics
- `CompanionAbilities_PassiveAbilities_HaveZeroCost` - Validates passive abilities
- `Companions_StartingAbilities_JsonArrayIsValid` - Validates JSON format

**Constraint Tests:**
- `CharactersCompanions_ForeignKeyConstraints_AreEnforced` - Validates FK enforcement
- `CompanionProgression_DefaultValues_AreCorrect` - Validates default values

---

## 3. SQL Reference File (`Data/v0.34.1_companion_schema.sql`)

Created standalone SQL file for reference/manual execution:
- Complete table creation statements
- All 6 companion INSERT statements
- All 18 companion ability INSERT statements
- Matches SaveRepository.cs implementation exactly

---

## Key Design Decisions

### 1. Companion_Abilities as Separate Table
- **Decision**: Created Companion_Abilities table instead of reusing player Abilities table
- **Rationale**:
  - Player Abilities table has FOREIGN KEY constraint to Specializations (NOT NULL)
  - Different schema requirements (companions don't have specialization IDs)
  - Cleaner separation of concerns
  - Follows pattern of Boss_Abilities being separate

### 2. ASCII companion_name vs. Unicode display_name
- **Decision**: Store ASCII names in `companion_name` for code use, Unicode in `display_name` for UI
- **Example**: `companion_name = "Kara_Ironbreaker"`, `display_name = "Kára Ironbreaker"`
- **Rationale**: v5.0 compliance - ASCII in data layer, diacritics in display layer

### 3. JSON for starting_abilities and unlocked_abilities
- **Decision**: Store ability ID arrays as JSON strings
- **Format**: `'[34101, 34102, 34103]'`
- **Rationale**: Flexible, no need for junction table, easy to parse in C#

### 4. Faction Requirements
- **Decision**: Some companions require faction reputation, others don't (NULL values)
- **Examples**:
  - Kára: Iron-Bane, Friendly (+25)
  - Finnr: Jötun-Reader, Friendly (+25)
  - Bjorn: Rust-Clan, Neutral (0)
  - Valdis/Runa: No requirement (NULL)
  - Einar: God-Sleeper, Friendly (+25)

### 5. No Permadeath
- **Decision**: `is_incapacitated` flag for System Crash, no deletion
- **Rationale**: v0.34 simplification - all companions recoverable

---

## Database ERD

```
Companions (master definition)
    ↓ (1:N)
Characters_Companions (recruitment state)
    ↓ (1:1)
Companion_Progression (leveling/equipment)
    ↓ (1:N)
Companion_Quests (personal quests)

Companion_Abilities (separate ability definitions)
    ↑
    Referenced by Companions.starting_abilities (JSON array)
```

---

## Verification Steps

### Manual Database Inspection

```bash
# Run application to create database
dotnet run --project RuneAndRust.ConsoleApp

# Inspect with sqlite3 (if available)
sqlite3 runeandrust.db

# Check tables
.tables
# Should show: Companions, Characters_Companions, Companion_Progression, Companion_Quests, Companion_Abilities

# Check companion count
SELECT COUNT(*) FROM Companions;
-- Expected: 6

# Check ability count
SELECT COUNT(*) FROM Companion_Abilities;
-- Expected: 18

# List all companions
SELECT companion_id, display_name, archetype, combat_role FROM Companions;

# Verify faction requirements
SELECT display_name, required_faction, required_reputation_value
FROM Companions
WHERE required_faction IS NOT NULL;
```

### Run Test Suite

```bash
# Run all companion database tests
dotnet test --filter "FullyQualifiedName~CompanionDatabaseTests"

# Expected: All 13 tests pass
```

---

## v0.34 Roadmap Integration

This completes **v0.34.1** (Database Schema & Companion Definitions).

**Next Steps:**
- **v0.34.2**: Companion AI & Tactical Behavior (8-12 hours)
  - CompanionAIService implementation
  - Stance-based behavior (Aggressive/Defensive/Passive)
  - Tactical grid integration

- **v0.34.3**: Recruitment & Progression Systems (7-10 hours)
  - RecruitmentService
  - Leveling formulas
  - Personal quest triggers

- **v0.34.4**: Service Implementation & Testing (7-11 hours)
  - CompanionService orchestration
  - Command verb integration
  - Integration testing

---

## v5.0 Compliance Checklist

✅ ASCII companion_name in database
✅ Unicode display_name for UI layer
✅ v2.0 canonical Stance System preserved (aggressive/defensive/passive)
✅ No "Echo" language (companions are NPCs, not cached data)
✅ Layer 2 voice ("incapacitated" not "system crashed" in display)
✅ Jötun-Reader (not Jotun-Reader) in display_name
✅ Structured logging comments in code
✅ Faction integration (reputation requirements)
✅ No permadeath (is_incapacitated flag)

---

## Testing Summary

**Created:**
- 13 comprehensive database tests
- Validates schema, seeding, constraints, defaults
- Validates faction requirements and archetype distribution

**Coverage:**
- Table existence ✅
- Companion count (6) ✅
- Ability count (18) ✅
- Faction requirements ✅
- Archetype distribution (3 Warriors, 1 Adept, 2 Mystics) ✅
- Foreign key constraints ✅
- Default values ✅
- JSON validation ✅

---

## Files Changed

1. **RuneAndRust.Persistence/SaveRepository.cs**
   - Added `CreateCompanionTables()` method (lines 1002-1192)
   - Added `SeedCompanionData()` method (lines 1194-1470)
   - Added method call in `InitializeDatabase()` (line 149)

2. **RuneAndRust.Tests/CompanionDatabaseTests.cs** (NEW)
   - 13 comprehensive tests for companion database schema

3. **Data/v0.34.1_companion_schema.sql** (NEW)
   - Standalone SQL reference file

4. **IMPLEMENTATION_SUMMARY_V0.34.1.md** (NEW)
   - This document

---

## Success Criteria

✅ 5 companion tables created
✅ 8 indices created for performance
✅ 6 companions seeded with complete stat blocks
✅ 18 companion abilities seeded (3 per companion)
✅ Faction requirements properly configured
✅ ASCII/Unicode name separation implemented
✅ Foreign key constraints enforced
✅ Default values set correctly
✅ Comprehensive test suite created
✅ v5.0 setting compliance verified

---

## Database Schema Quick Reference

### Companions Table
- **Primary Key**: companion_id (INTEGER)
- **Unique**: companion_name
- **Check Constraints**: archetype IN ('Warrior', 'Adept', 'Mystic'), resource_type IN ('Stamina', 'Aether Pool'), default_stance IN ('aggressive', 'defensive', 'passive')
- **JSON Fields**: starting_abilities (array of ability IDs)

### Characters_Companions Table
- **Primary Key**: character_companion_id (AUTOINCREMENT)
- **Foreign Keys**: character_id → saves(id), companion_id → Companions(companion_id)
- **Unique**: (character_id, companion_id)
- **Check Constraint**: current_stance IN ('aggressive', 'defensive', 'passive')

### Companion_Progression Table
- **Primary Key**: progression_id (AUTOINCREMENT)
- **Foreign Keys**: character_id → saves(id), companion_id → Companions(companion_id)
- **Unique**: (character_id, companion_id)
- **JSON Fields**: unlocked_abilities (array of ability IDs)
- **Nullable**: All override_* columns (use base stats from Companions if NULL)

### Companion_Quests Table
- **Primary Key**: companion_quest_id (AUTOINCREMENT)
- **Foreign Keys**: character_id → saves(id), companion_id → Companions(companion_id)
- **Unique**: (character_id, companion_id, quest_id)

### Companion_Abilities Table
- **Primary Key**: ability_id (INTEGER)
- **No Foreign Keys**: Standalone table
- **Default**: ability_category = 'companion_ability'

---

## Known Limitations & Future Work

**Current Limitations:**
- Personal quest_id references not yet populated (quests not created yet)
- recruitment_quest_id not yet populated (recruitment quests not created yet)
- No Equipment foreign key validation (Equipment table may not have companion-compatible items yet)

**Future Enhancements (v0.34.2+):**
- CompanionService to manage recruitment/dismissal
- CompanionAIService for tactical behavior
- Leveling formulas in CompanionProgressionService
- Personal quest content creation
- Recruitment quest content creation

---

**Implementation Time**: ~1 hour
**Database Foundation**: ✅ Complete
**Ready for**: v0.34.2 (Companion AI & Tactical Behavior)
