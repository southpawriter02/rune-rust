# Database Schema Reference

Parent item: Technical Reference (Technical%20Reference%202ba55eb312da8079a291e020980301c1.md)

> Version: 0.41+
Last Updated: November 2024
Database: SQLite (Microsoft.Data.Sqlite 8.0.0)
Location: RuneAndRust.Persistence/
> 

## Overview

Rune & Rust uses a single SQLite database (`runeandrust.db`) for all persistent data storage. The schema has evolved through 40+ versions with forward-compatible migrations. Complex nested data is stored as JSON columns to avoid excessive normalization.

## Database Architecture

```
runeandrust.db
├── Core Tables
│   ├── saves                    # Character save data (50+ columns)
│   └── world_state_changes      # Delta-based world state
├── Specialization System (v0.19)
│   ├── Specializations          # Specialization definitions
│   ├── Abilities                # Ability definitions
│   ├── CharacterSpecializations # Unlocked specs per character
│   ├── CharacterAbilities       # Learned abilities per character
│   ├── Characters_Fury          # Berserkr resource (v0.26.1)
│   ├── Characters_SpiritBargains # Seidkona mechanics (v0.28.1)
│   └── Characters_EchoChains    # Echo-Caller mechanics (v0.28.2)
├── Battlefield Grid (v0.20)
│   ├── battlefield_grids        # Grid metadata
│   ├── battlefield_tiles        # Individual tile state
│   └── battlefield_traps        # Active traps
├── Biome System (v0.29-0.30)
│   ├── Biomes                   # Biome definitions
│   ├── Biome_RoomTemplates      # Room templates per biome
│   ├── Biome_EnvironmentalFeatures # Environmental hazards
│   ├── Biome_EnemySpawns        # Enemy spawn weights
│   ├── Biome_ResourceDrops      # Resource drop tables
│   └── Characters_BiomeStatus   # Per-character biome progress
├── Faction System (v0.33.1)
│   └── [Faction tables]
├── Companion System (v0.34.1)
│   └── [Companion tables]
├── Territory Control (v0.35.1)
│   └── [Territory tables]
└── Meta-Progression (v0.41)
    ├── Account_Progression      # Account-wide progression
    ├── Account_Unlocks          # Unlock definitions
    ├── Account_Unlock_Progress  # Per-account unlock status
    ├── Milestone_Tiers          # Milestone tier definitions
    ├── Account_Milestone_Progress # Per-account milestone status
    ├── Achievements             # Achievement definitions
    ├── Account_Achievement_Progress # Per-account achievements
    ├── Cosmetics               # Cosmetic item definitions
    ├── Account_Cosmetics       # Per-account cosmetic ownership
    └── Alternative_Starts      # Alternative start scenarios

```

## Core Tables

### saves

Primary table storing all character save data. Uses `INSERT OR REPLACE` for upsert behavior.

```sql
CREATE TABLE IF NOT EXISTS saves (
    -- Identity
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_name TEXT UNIQUE NOT NULL,
    class TEXT NOT NULL,
    specialization TEXT DEFAULT 'None',    -- v0.7

    -- Progression
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
    temp_hp INTEGER DEFAULT 0,             -- v0.7
    currency INTEGER DEFAULT 0,            -- v0.9

    -- Trauma Economy (v0.5)
    psychic_stress INTEGER DEFAULT 0,
    corruption INTEGER DEFAULT 0,
    rooms_explored_since_rest INTEGER DEFAULT 0,
    traumas_json TEXT DEFAULT '[]',        -- v0.15

    -- Status Effects (v0.7 Adept)
    vulnerable_turns INTEGER DEFAULT 0,
    analyzed_turns INTEGER DEFAULT 0,
    seized_turns INTEGER DEFAULT 0,
    is_performing INTEGER DEFAULT 0,
    performing_turns INTEGER DEFAULT 0,
    current_performance TEXT,
    inspired_turns INTEGER DEFAULT 0,
    silenced_turns INTEGER DEFAULT 0,

    -- World State
    current_room_id INTEGER NOT NULL,
    current_room_string_id TEXT,           -- v0.10
    cleared_rooms_json TEXT NOT NULL,
    puzzle_solved INTEGER NOT NULL,
    boss_defeated INTEGER NOT NULL,

    -- Equipment (v0.3)
    equipped_weapon_json TEXT,
    equipped_armor_json TEXT,
    inventory_json TEXT DEFAULT '[]',
    room_items_json TEXT DEFAULT '{}',

    -- Consumables & Crafting (v0.7)
    consumables_json TEXT DEFAULT '[]',
    crafting_components_json TEXT DEFAULT '{}',

    -- Factions & Quests (v0.8)
    faction_reputations_json TEXT DEFAULT '{}',
    active_quests_json TEXT DEFAULT '[]',
    completed_quests_json TEXT DEFAULT '[]',
    npc_states_json TEXT DEFAULT '[]',

    -- Procedural Generation (v0.10)
    is_procedural_dungeon INTEGER DEFAULT 0,
    current_dungeon_seed INTEGER DEFAULT 0,
    dungeons_completed INTEGER DEFAULT 0,
    current_biome_id TEXT,

    -- Tactical Combat Grid (v0.20)
    position_zone TEXT,
    position_row TEXT,
    position_column INTEGER,
    position_elevation INTEGER DEFAULT 0,
    kinetic_energy INTEGER DEFAULT 0,
    max_kinetic_energy INTEGER DEFAULT 100,

    -- Stance System (v0.21.1)
    active_stance_type TEXT DEFAULT 'Calculated',
    stance_turns_in_current INTEGER DEFAULT 0,
    stance_shifts_remaining INTEGER DEFAULT 1,

    -- Metadata
    last_saved TEXT NOT NULL
)

```

**JSON Column Schemas:**

| Column | JSON Structure |
| --- | --- |
| `cleared_rooms_json` | `int[]` - Array of cleared room IDs |
| `equipped_weapon_json` | `Equipment` object with type, stats, bonuses |
| `equipped_armor_json` | `Equipment` object |
| `inventory_json` | `Equipment[]` - Array of equipment objects |
| `consumables_json` | `Consumable[]` - Array of consumable items |
| `crafting_components_json` | `{ComponentType: count}` - Component counts |
| `traumas_json` | `Trauma[]` - Array of permanent trauma effects |
| `faction_reputations_json` | `{FactionId: reputation}` - Faction standings |
| `active_quests_json` | `Quest[]` - Active quest data |
| `completed_quests_json` | `string[]` - Quest IDs |

### world_state_changes

Delta-based storage for world state changes. Enables efficient room state reconstruction.

```sql
CREATE TABLE IF NOT EXISTS world_state_changes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    save_id INTEGER NOT NULL,
    sector_seed TEXT NOT NULL,
    room_id TEXT NOT NULL,
    change_type TEXT NOT NULL,
    target_id TEXT NOT NULL,
    change_data TEXT NOT NULL,       -- JSON payload
    timestamp TEXT NOT NULL,
    turn_number INTEGER NOT NULL,
    schema_version INTEGER NOT NULL DEFAULT 1,

    FOREIGN KEY (save_id) REFERENCES saves(id) ON DELETE CASCADE
)

```

**Indices:**

```sql
CREATE INDEX idx_world_state_save_sector ON world_state_changes(save_id, sector_seed);
CREATE INDEX idx_world_state_save_room ON world_state_changes(save_id, room_id);
CREATE INDEX idx_world_state_timestamp ON world_state_changes(timestamp);

```

**Change Types:**

- `EnemyDefeated` - Enemy removed from room
- `ItemLooted` - Item picked up
- `ChestOpened` - Container opened
- `TrapTriggered` - Trap activated
- `NpcInteracted` - NPC state changed
- `EnvironmentModified` - Terrain/hazard changed

## Specialization System Tables (v0.19)

### Specializations

Master table of all character specializations (talent trees).

```sql
CREATE TABLE IF NOT EXISTS Specializations (
    SpecializationID INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    ArchetypeID INTEGER NOT NULL,
    PathType TEXT NOT NULL,
    MechanicalRole TEXT NOT NULL,
    PrimaryAttribute TEXT NOT NULL,
    SecondaryAttribute TEXT,
    Description TEXT,
    Tagline TEXT,
    UnlockRequirementsJson TEXT NOT NULL,
    ResourceSystem TEXT NOT NULL,
    TraumaRisk TEXT NOT NULL,
    IconEmoji TEXT,
    PPCostToUnlock INTEGER DEFAULT 3,
    IsActive INTEGER DEFAULT 1
)

```

**Indices:**

```sql
CREATE INDEX idx_specializations_archetype ON Specializations(ArchetypeID);

```

### Abilities

All ability definitions linked to specializations.

```sql
CREATE TABLE IF NOT EXISTS Abilities (
    AbilityID INTEGER PRIMARY KEY,
    SpecializationID INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT NOT NULL,
    TierLevel INTEGER NOT NULL,
    PPCost INTEGER NOT NULL,
    PrerequisitesJson TEXT NOT NULL,
    AbilityType TEXT NOT NULL,
    ActionType TEXT NOT NULL,
    TargetType TEXT NOT NULL,
    ResourceCostJson TEXT NOT NULL,
    AttributeUsed TEXT,
    BonusDice INTEGER DEFAULT 0,
    SuccessThreshold INTEGER DEFAULT 2,
    MechanicalSummary TEXT,
    DamageDice INTEGER DEFAULT 0,
    IgnoresArmor INTEGER DEFAULT 0,
    HealingDice INTEGER DEFAULT 0,
    StatusEffectsAppliedJson TEXT,
    StatusEffectsRemovedJson TEXT,
    MaxRank INTEGER DEFAULT 3,
    CostToRank2 INTEGER DEFAULT 5,
    CostToRank3 INTEGER DEFAULT 0,
    CooldownTurns INTEGER DEFAULT 0,
    CooldownType TEXT DEFAULT 'None',
    IsActive INTEGER DEFAULT 1,
    Notes TEXT,

    FOREIGN KEY (SpecializationID) REFERENCES Specializations(SpecializationID)
)

```

**Indices:**

```sql
CREATE INDEX idx_abilities_specialization ON Abilities(SpecializationID);
CREATE INDEX idx_abilities_tier ON Abilities(TierLevel);

```

### CharacterSpecializations

Tracks which specializations each character has unlocked.

```sql
CREATE TABLE IF NOT EXISTS CharacterSpecializations (
    CharacterID INTEGER NOT NULL,
    SpecializationID INTEGER NOT NULL,
    UnlockedAt TEXT NOT NULL,
    IsActive INTEGER DEFAULT 1,
    PPSpentInTree INTEGER DEFAULT 0,

    PRIMARY KEY (CharacterID, SpecializationID),
    FOREIGN KEY (SpecializationID) REFERENCES Specializations(SpecializationID)
)

```

### CharacterAbilities

Tracks which abilities each character has learned and their ranks.

```sql
CREATE TABLE IF NOT EXISTS CharacterAbilities (
    CharacterID INTEGER NOT NULL,
    AbilityID INTEGER NOT NULL,
    UnlockedAt TEXT NOT NULL,
    CurrentRank INTEGER DEFAULT 1,
    TimesUsed INTEGER DEFAULT 0,

    PRIMARY KEY (CharacterID, AbilityID),
    FOREIGN KEY (AbilityID) REFERENCES Abilities(AbilityID)
)

```

### Characters_Fury (v0.26.1)

Berserkr specialization resource tracking.

```sql
CREATE TABLE IF NOT EXISTS Characters_Fury (
    character_id INTEGER PRIMARY KEY,
    current_fury INTEGER NOT NULL DEFAULT 0
        CHECK(current_fury >= 0 AND current_fury <= 100),
    max_fury INTEGER NOT NULL DEFAULT 100,
    in_combat INTEGER NOT NULL DEFAULT 0,
    last_fury_gain_timestamp TEXT,
    total_fury_generated INTEGER NOT NULL DEFAULT 0,
    total_fury_spent INTEGER NOT NULL DEFAULT 0,
    unstoppable_fury_triggered INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
)

```

### Characters_SpiritBargains (v0.28.1)

Seidkona specialization mechanics tracking.

```sql
CREATE TABLE IF NOT EXISTS Characters_SpiritBargains (
    character_id INTEGER PRIMARY KEY,
    total_bargains_triggered INTEGER NOT NULL DEFAULT 0,
    total_bargains_attempted INTEGER NOT NULL DEFAULT 0,
    bargain_success_rate REAL NOT NULL DEFAULT 0.0,
    fickle_fortune_rank INTEGER NOT NULL DEFAULT 0,
    in_moment_of_clarity INTEGER NOT NULL DEFAULT 0,
    clarity_turns_remaining INTEGER NOT NULL DEFAULT 0,
    clarity_uses_this_combat INTEGER NOT NULL DEFAULT 0,
    forced_bargain_used_this_combat INTEGER NOT NULL DEFAULT 0,
    psychic_resonance_bonus_active INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
)

```

### Characters_EchoChains (v0.28.2)

Echo-Caller specialization mechanics tracking.

```sql
CREATE TABLE IF NOT EXISTS Characters_EchoChains (
    character_id INTEGER PRIMARY KEY,
    total_echoes_triggered INTEGER NOT NULL DEFAULT 0,
    total_echo_chains_triggered INTEGER NOT NULL DEFAULT 0,
    echo_cascade_rank INTEGER NOT NULL DEFAULT 0,
    echo_chain_range INTEGER NOT NULL DEFAULT 1,
    echo_chain_damage_multiplier REAL NOT NULL DEFAULT 0.5,
    echo_chain_max_targets INTEGER NOT NULL DEFAULT 1,
    silence_weapon_uses_this_combat INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE
)

```

## Battlefield Grid Tables (v0.20)

### battlefield_grids

Grid metadata for tactical combat.

```sql
CREATE TABLE IF NOT EXISTS battlefield_grids (
    grid_id TEXT PRIMARY KEY,
    combat_id TEXT,
    columns INTEGER NOT NULL,
    created_at TEXT NOT NULL
)

```

### battlefield_tiles

Individual tile state within grids.

```sql
CREATE TABLE IF NOT EXISTS battlefield_tiles (
    tile_id TEXT PRIMARY KEY,
    grid_id TEXT NOT NULL,
    zone TEXT NOT NULL CHECK(zone IN ('Player', 'Enemy')),
    row TEXT NOT NULL CHECK(row IN ('Front', 'Back')),
    column_index INTEGER NOT NULL,
    elevation INTEGER DEFAULT 0,
    tile_type TEXT NOT NULL
        CHECK(tile_type IN ('Normal', 'HighGround', 'Glitched')),
    cover_type TEXT NOT NULL
        CHECK(cover_type IN ('None', 'Physical', 'Metaphysical', 'Both')),
    glitch_type TEXT
        CHECK(glitch_type IN ('Flickering', 'InvertedGravity', 'Looping')),
    glitch_severity INTEGER CHECK(glitch_severity BETWEEN 1 AND 3),
    is_occupied INTEGER DEFAULT 0,
    occupant_id TEXT,

    FOREIGN KEY (grid_id) REFERENCES battlefield_grids(grid_id)
)

```

### battlefield_traps

Active traps on battlefield tiles.

```sql
CREATE TABLE IF NOT EXISTS battlefield_traps (
    trap_id TEXT PRIMARY KEY,
    trap_name TEXT NOT NULL,
    tile_id TEXT NOT NULL,
    owner_id TEXT NOT NULL,
    turns_remaining INTEGER NOT NULL,
    is_visible INTEGER DEFAULT 0,
    effect_type TEXT NOT NULL
        CHECK(effect_type IN ('Damage', 'Status', 'Debuff', 'AreaEffect')),
    effect_data TEXT NOT NULL,
    trigger_type TEXT NOT NULL
        CHECK(trigger_type IN ('OnEnter', 'OnExit', 'Manual')),
    created_at TEXT NOT NULL,

    FOREIGN KEY (tile_id) REFERENCES battlefield_tiles(tile_id)
)

```

## Biome System Tables (v0.29-0.30)

### Biomes

Core biome metadata.

```sql
CREATE TABLE IF NOT EXISTS Biomes (
    biome_id INTEGER PRIMARY KEY,
    biome_name TEXT NOT NULL UNIQUE,
    biome_description TEXT,
    z_level_restriction TEXT,
    ambient_condition_id INTEGER,
    min_character_level INTEGER DEFAULT 1,
    max_character_level INTEGER DEFAULT 12,
    is_active INTEGER DEFAULT 1,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP
)

```

### Biome_RoomTemplates

Room templates available per biome.

```sql
CREATE TABLE IF NOT EXISTS Biome_RoomTemplates (
    template_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    template_name TEXT NOT NULL,
    template_description TEXT,
    room_size_category TEXT
        CHECK(room_size_category IN ('Small', 'Medium', 'Large', 'XLarge')),
    min_connections INTEGER DEFAULT 1,
    max_connections INTEGER DEFAULT 4,
    can_be_entrance INTEGER DEFAULT 0,
    can_be_exit INTEGER DEFAULT 0,
    can_be_hub INTEGER DEFAULT 0,
    hazard_density TEXT
        CHECK(hazard_density IN ('None', 'Low', 'Medium', 'High', 'Extreme')),
    enemy_spawn_weight INTEGER DEFAULT 100,
    resource_spawn_chance REAL DEFAULT 0.3,
    wfc_adjacency_rules TEXT,             -- JSON for Wave Function Collapse
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
)

```

### Biome_EnvironmentalFeatures

Environmental hazards and terrain features per biome.

```sql
CREATE TABLE IF NOT EXISTS Biome_EnvironmentalFeatures (
    feature_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    feature_name TEXT NOT NULL,
    feature_type TEXT
        CHECK(feature_type IN ('Hazard', 'Terrain', 'Ambient',
                               'Obstacle', 'Dynamic', 'Explosive', 'Vision')),
    feature_description TEXT,
    damage_per_turn INTEGER DEFAULT 0,
    damage_type TEXT,
    tile_coverage_percent REAL DEFAULT 0,
    is_destructible INTEGER DEFAULT 0,
    blocks_movement INTEGER DEFAULT 0,
    blocks_line_of_sight INTEGER DEFAULT 0,
    hazard_density_category TEXT,
    special_rules TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
)

```

### Biome_EnemySpawns

Enemy spawn weights per biome.

```sql
CREATE TABLE IF NOT EXISTS Biome_EnemySpawns (
    spawn_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    enemy_name TEXT NOT NULL,
    enemy_type TEXT NOT NULL,
    spawn_weight INTEGER DEFAULT 100,
    min_level INTEGER DEFAULT 1,
    max_level INTEGER DEFAULT 12,
    spawn_rules_json TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
)

```

### Biome_ResourceDrops

Resource drop tables per biome.

```sql
CREATE TABLE IF NOT EXISTS Biome_ResourceDrops (
    resource_drop_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    resource_name TEXT NOT NULL,
    resource_description TEXT,
    resource_tier INTEGER CHECK(resource_tier >= 1 AND resource_tier <= 5),
    rarity TEXT CHECK(rarity IN ('Common', 'Uncommon', 'Rare', 'Epic', 'Legendary')),
    base_drop_chance REAL DEFAULT 0.1,
    min_quantity INTEGER DEFAULT 1,
    max_quantity INTEGER DEFAULT 1,
    requires_special_node INTEGER DEFAULT 0,
    weight INTEGER DEFAULT 100,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
)

```

### Characters_BiomeStatus

Per-character biome exploration progress.

```sql
CREATE TABLE IF NOT EXISTS Characters_BiomeStatus (
    status_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    biome_id INTEGER NOT NULL,
    first_entry_timestamp TEXT,
    total_time_seconds INTEGER DEFAULT 0,
    rooms_explored INTEGER DEFAULT 0,
    enemies_defeated INTEGER DEFAULT 0,
    heat_damage_taken INTEGER DEFAULT 0,
    times_died_to_heat INTEGER DEFAULT 0,
    resources_collected INTEGER DEFAULT 0,
    has_reached_surtur INTEGER DEFAULT 0,
    last_updated TEXT DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE,
    UNIQUE(character_id, biome_id)
)

```

## Meta-Progression Tables (v0.41)

### Account_Progression

Account-wide progression tracking.

```sql
CREATE TABLE IF NOT EXISTS Account_Progression (
    account_id INTEGER PRIMARY KEY AUTOINCREMENT,
    total_achievement_points INTEGER DEFAULT 0,
    current_milestone_tier INTEGER DEFAULT 1,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    total_characters_created INTEGER DEFAULT 0,
    total_campaigns_completed INTEGER DEFAULT 0,
    total_bosses_defeated INTEGER DEFAULT 0,
    total_achievements_unlocked INTEGER DEFAULT 0,
    highest_new_game_plus_tier INTEGER DEFAULT 0,
    highest_endless_wave INTEGER DEFAULT 0
)

```

### Account_Unlocks

Unlock definitions (what can be unlocked).

```sql
CREATE TABLE IF NOT EXISTS Account_Unlocks (
    unlock_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    unlock_type TEXT NOT NULL,
    description TEXT NOT NULL,
    requirement_description TEXT NOT NULL,
    parameters_json TEXT DEFAULT '{}'
)

```

### Account_Unlock_Progress

Per-account unlock status.

```sql
CREATE TABLE IF NOT EXISTS Account_Unlock_Progress (
    progress_id INTEGER PRIMARY KEY AUTOINCREMENT,
    account_id INTEGER NOT NULL,
    unlock_id TEXT NOT NULL,
    is_unlocked INTEGER DEFAULT 0,
    unlocked_at TEXT,

    FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id),
    FOREIGN KEY (unlock_id) REFERENCES Account_Unlocks(unlock_id),
    UNIQUE (account_id, unlock_id)
)

```

### Milestone_Tiers

Milestone tier definitions with rewards.

```sql
CREATE TABLE IF NOT EXISTS Milestone_Tiers (
    tier_number INTEGER PRIMARY KEY,
    tier_name TEXT NOT NULL,
    description TEXT NOT NULL,
    required_achievement_points INTEGER NOT NULL,
    unlock_rewards_json TEXT DEFAULT '[]',
    cosmetic_rewards_json TEXT DEFAULT '[]',
    alternative_start_unlock TEXT
)

```

## Migration Strategy

The database uses a forward-compatible migration pattern:

```csharp
// Attempt to add new columns, silently ignore if they exist
var alterCommands = new[]
{
    "ALTER TABLE saves ADD COLUMN new_column TEXT DEFAULT 'value'",
    // ... more migrations
};

foreach (var alterSql in alterCommands)
{
    try
    {
        var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = alterSql;
        alterCommand.ExecuteNonQuery();
    }
    catch (SqliteException)
    {
        // Column already exists, ignore
    }
}

```

**Migration History:**

- v0.3: Equipment columns
- v0.5: Trauma economy columns
- v0.7: Specialization, status effects, consumables, crafting
- v0.8: Factions, quests, NPCs
- v0.9: Currency
- v0.10: Procedural generation
- v0.15: Permanent traumas
- v0.19: Specialization system tables
- v0.20: Tactical combat grid
- v0.21.1: Advanced stance system
- v0.26.1: Berserkr Fury tracking
- v0.28.1: Seidkona Spirit Bargains
- v0.28.2: Echo-Caller Echo Chains
- v0.29.1: Muspelheim biome tables
- v0.30.1: Niflheim biome tables
- v0.33.1: Faction system tables
- v0.34.1: Companion system tables
- v0.35.1: Territory control tables
- v0.41: Meta-progression tables

## Entity Relationship Diagram

```
┌─────────────────┐
│     saves       │◄───────────────────────────────────────┐
│  (character)    │                                        │
└────────┬────────┘                                        │
         │ 1                                               │
         │                                                 │
         ├──────────────────────────────┐                  │
         │                              │                  │
         ▼ n                            ▼ n                │
┌─────────────────┐              ┌─────────────────┐       │
│ world_state_    │              │ Character       │       │
│ changes         │              │ Specializations │       │
└─────────────────┘              └────────┬────────┘       │
                                          │                │
                                          ▼ n              │
                                 ┌─────────────────┐       │
                                 │ Character       │       │
                                 │ Abilities       │       │
                                 └─────────────────┘       │
                                                           │
┌─────────────────┐              ┌─────────────────┐       │
│ Specializations │◄─────────────│    Abilities    │       │
│  (definitions)  │ 1          n │  (definitions)  │       │
└─────────────────┘              └─────────────────┘       │
                                                           │
┌─────────────────┐              ┌─────────────────┐       │
│     Biomes      │◄─────────────│ Characters_     │───────┘
│  (definitions)  │ 1          n │ BiomeStatus     │
└────────┬────────┘              └─────────────────┘
         │ 1
         │
         ├─────────────┬─────────────┬─────────────┐
         ▼ n           ▼ n           ▼ n           ▼ n
┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│ RoomTemplates│  │ Environmental│  │ EnemySpawns │  │ ResourceDrops│
└─────────────┘  │ Features     │  └─────────────┘  └─────────────┘
                 └─────────────┘

┌─────────────────┐              ┌─────────────────┐
│ Account_        │◄─────────────│ Account_        │
│ Progression     │ 1          n │ Unlock_Progress │
└────────┬────────┘              └─────────────────┘
         │ 1
         │
         ▼ n
┌─────────────────┐
│ Account_        │
│ Milestone_      │
│ Progress        │
└─────────────────┘

```

## Performance Considerations

### Indices

All foreign key columns and frequently-queried columns are indexed:

```sql
-- World state queries
CREATE INDEX idx_world_state_save_sector ON world_state_changes(save_id, sector_seed);
CREATE INDEX idx_world_state_save_room ON world_state_changes(save_id, room_id);

-- Specialization lookups
CREATE INDEX idx_specializations_archetype ON Specializations(ArchetypeID);
CREATE INDEX idx_abilities_specialization ON Abilities(SpecializationID);

-- Battlefield queries
CREATE INDEX idx_tiles_position ON battlefield_tiles(grid_id, zone, row, column_index);

-- Biome queries
CREATE INDEX idx_biome_resources_rarity ON Biome_ResourceDrops(rarity);

```

### JSON Column Trade-offs

**Advantages:**

- Simpler schema for complex nested data
- Flexible structure for evolving game data
- Single row contains complete character state

**Disadvantages:**

- Cannot query inside JSON efficiently
- Larger row sizes
- Full deserialization required for partial reads

## Related Documentation

- [Data Access Patterns](https://www.notion.so/data-access-patterns.md) - Repository patterns and query examples
- [Service Architecture](https://www.notion.so/service-architecture.md) - How services use repositories
- [Save/Load System](https://www.notion.so/services/save-repository.md) - Save/load implementation details