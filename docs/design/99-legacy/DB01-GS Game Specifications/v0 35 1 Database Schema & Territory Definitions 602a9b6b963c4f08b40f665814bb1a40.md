# v0.35.1: Database Schema & Territory Definitions

Type: Technical
Description: 4 new tables (Faction_Territory_Control, Faction_Wars, World_Events, Player_Territorial_Actions), seed data for initial territory states (5 factions across 10 sectors), SQL migration script. 7-10 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.33.1 (Faction Database), v0.13 (World State Persistence)
Implementation Difficulty: Medium
Balance Validated: No
Parent item: v0.35: Territory Control & Dynamic World (v0%2035%20Territory%20Control%20&%20Dynamic%20World%2029703a131b8c4c4699182fcc43d99a22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.35.1-TERRITORY-DATABASE

**Parent Specification:** [v0.35: Territory Control & Dynamic World](v0%2035%20Territory%20Control%20&%20Dynamic%20World%2029703a131b8c4c4699182fcc43d99a22.md)

**Status:** Design Complete — Ready for Implementation

**Timeline:** 7-10 hours

**Prerequisites:** v0.33.1 (Faction Database), v0.13 (World State Persistence)

---

## I. Executive Summary

v0.35.1 establishes the **database foundation for Territory Control** by creating 4 new tables:

- **Faction_Territory_Control** — Track faction influence per sector
- **Faction_Wars** — Record active and historical faction conflicts
- **World_Events** — Dynamic event catalog (incursions, raids, catastrophes)
- **Player_Territorial_Actions** — Player action history affecting influence

This specification provides the data layer for faction territorial competition, war mechanics, and dynamic world events.

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.35.1)

- Complete database schema for 4 new tables
- Indexes for query performance
- Seed data for initial territory states (5 factions across 10 sectors)
- SQL migration script (v0.35.1_territory_control.sql)
- Foreign key constraints and data integrity
- Initial influence distribution
- Deployment instructions

### ❌ Explicitly Out of Scope

- Service layer implementation (defer to v0.35.2-v0.35.4)
- War triggering logic (defer to v0.35.2)
- Event generation algorithms (defer to v0.35.3)
- Player influence calculation (defer to v0.35.4)
- UI components (separate phase)
- Testing (covered in service specs)

---

## III. Database Schema

### Table 1: Faction_Territory_Control

**Purpose:** Track faction influence values per sector.

```sql
CREATE TABLE Faction_Territory_Control (
    territory_control_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    faction_name TEXT NOT NULL,
    influence_value REAL NOT NULL DEFAULT 0.0, -- 0.0 to 100.0
    control_state TEXT NOT NULL CHECK(control_state IN ('Stable', 'Contested', 'War', 'Independent', 'Ruined')),
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
    FOREIGN KEY (faction_name) REFERENCES Factions(faction_name) ON DELETE CASCADE,
    UNIQUE(world_id, sector_id, faction_name)
);

-- Indexes for performance
CREATE INDEX idx_territory_control_sector ON Faction_Territory_Control(sector_id);
CREATE INDEX idx_territory_control_faction ON Faction_Territory_Control(faction_name);
CREATE INDEX idx_territory_control_state ON Faction_Territory_Control(control_state);
CREATE INDEX idx_territory_control_influence ON Faction_Territory_Control(influence_value DESC);
```

**Column Definitions:**

| Column | Type | Description |
| --- | --- | --- |
| territory_control_id | INTEGER | Primary key |
| world_id | INTEGER | Which world this applies to |
| sector_id | INTEGER | Which sector is being influenced |
| faction_name | TEXT | Which faction (Iron-Banes, Jötun-Readers, etc.) |
| influence_value | REAL | 0.0 to 100.0 (percentage influence) |
| control_state | TEXT | 'Stable', 'Contested', 'War', 'Independent', 'Ruined' |
| last_updated | TIMESTAMP | When influence last changed |

**Business Rules:**

- Sum of all influence_values per sector should trend toward 100.0 (but can temporarily exceed)
- control_state calculated from influence distribution
- Stable: One faction has 60%+ influence
- Contested: Two or more factions have 40-60% influence
- War: Active war in Faction_Wars table for this sector
- Independent: No faction exceeds 40% influence
- Ruined: Special event flag (sector temporarily uninhabitable)

---

### Table 2: Faction_Wars

**Purpose:** Track active and historical faction wars over territory.

```sql
CREATE TABLE Faction_Wars (
    war_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    faction_a TEXT NOT NULL,
    faction_b TEXT NOT NULL,
    war_start_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    war_end_date TIMESTAMP,
    war_balance REAL DEFAULT 0.0, -- -100 to +100 (+ favors faction_a)
    is_active BOOLEAN NOT NULL DEFAULT 1,
    victor TEXT, -- NULL if ongoing, faction_name if resolved
    collateral_damage INTEGER DEFAULT 0, -- Hazard density increase %
    
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
    FOREIGN KEY (faction_a) REFERENCES Factions(faction_name) ON DELETE CASCADE,
    FOREIGN KEY (faction_b) REFERENCES Factions(faction_name) ON DELETE CASCADE,
    
    CHECK(faction_a != faction_b),
    CHECK(war_balance BETWEEN -100 AND 100)
);

-- Indexes
CREATE INDEX idx_wars_active ON Faction_Wars(is_active) WHERE is_active = 1;
CREATE INDEX idx_wars_sector ON Faction_Wars(sector_id);
CREATE INDEX idx_wars_date ON Faction_Wars(war_start_date DESC);
```

**Column Definitions:**

| Column | Type | Description |
| --- | --- | --- |
| war_id | INTEGER | Primary key |
| world_id | INTEGER | Which world |
| sector_id | INTEGER | Contested sector |
| faction_a | TEXT | First combatant faction |
| faction_b | TEXT | Second combatant faction |
| war_start_date | TIMESTAMP | When war began |
| war_end_date | TIMESTAMP | When war ended (NULL if ongoing) |
| war_balance | REAL | -100 to +100 (positive favors faction_a) |
| is_active | BOOLEAN | 1 if war ongoing, 0 if resolved |
| victor | TEXT | Winning faction name (NULL if ongoing) |
| collateral_damage | INTEGER | Percentage increase in hazard density |

**Business Rules:**

- Only 1 active war per sector at a time
- war_balance starts at 0.0
- Player actions shift war_balance (quest completion, enemy kills)
- War resolves when war_balance reaches ±50 or time limit (15 days)
- Victor gets +20% influence, loser gets -20% influence
- collateral_damage increases sector hazard density

---

### Table 3: World_Events

**Purpose:** Catalog dynamic world events affecting territories.

```sql
CREATE TABLE World_Events (
    event_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id INTEGER NOT NULL,
    sector_id INTEGER,
    event_type TEXT NOT NULL CHECK(event_type IN (
        'Faction_War', 'Incursion', 'Supply_Raid', 'Diplomatic_Shift', 
        'Catastrophe', 'Awakening_Ritual', 'Excavation_Discovery', 
        'Purge_Campaign', 'Scavenger_Caravan'
    )),
    affected_faction TEXT,
    event_title TEXT NOT NULL,
    event_description TEXT NOT NULL,
    event_start_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    event_end_date TIMESTAMP,
    event_duration_days INTEGER DEFAULT 7, -- Expected duration
    is_resolved BOOLEAN NOT NULL DEFAULT 0,
    player_influenced BOOLEAN NOT NULL DEFAULT 0,
    outcome TEXT, -- 'Success', 'Failure', 'Partial', 'Player_Intervention'
    influence_change REAL DEFAULT 0.0, -- ±influence from event resolution
    
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
    FOREIGN KEY (affected_faction) REFERENCES Factions(faction_name) ON DELETE CASCADE
);

-- Indexes
CREATE INDEX idx_events_active ON World_Events(is_resolved) WHERE is_resolved = 0;
CREATE INDEX idx_events_sector ON World_Events(sector_id);
CREATE INDEX idx_events_faction ON World_Events(affected_faction);
CREATE INDEX idx_events_type ON World_Events(event_type);
```

**Column Definitions:**

| Column | Type | Description |
| --- | --- | --- |
| event_id | INTEGER | Primary key |
| world_id | INTEGER | Which world |
| sector_id | INTEGER | Affected sector (NULL for world-wide events) |
| event_type | TEXT | Enum: 'Faction_War', 'Incursion', etc. |
| affected_faction | TEXT | Primary faction involved |
| event_title | TEXT | Display name ("Awakening Ritual", "Supply Raid") |
| event_description | TEXT | Player-facing description |
| event_start_date | TIMESTAMP | When event began |
| event_end_date | TIMESTAMP | When event concluded |
| event_duration_days | INTEGER | Expected duration (7-15 days typical) |
| is_resolved | BOOLEAN | 0 if ongoing, 1 if complete |
| player_influenced | BOOLEAN | Did player intervene? |
| outcome | TEXT | 'Success', 'Failure', 'Partial', 'Player_Intervention' |
| influence_change | REAL | ±influence applied to affected_faction |

**Event Types:**

- **Faction_War:** Links to Faction_Wars table
- **Incursion:** Faction attempts territorial expansion
- **Supply_Raid:** Enemies raid merchant supplies
- **Diplomatic_Shift:** Reputation thresholds cause influence change
- **Catastrophe:** Environmental disaster (lava eruption, reality tear)
- **Awakening_Ritual:** God-Sleeper cultists activate Jötun-Forged
- **Excavation_Discovery:** Jötun-Readers find major artifact
- **Purge_Campaign:** Iron-Banes launch Undying purge
- **Scavenger_Caravan:** Rust-Clans establish trade route

---

### Table 4: Player_Territorial_Actions

**Purpose:** Log player actions that affect faction influence.

```sql
CREATE TABLE Player_Territorial_Actions (
    action_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    world_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    action_type TEXT NOT NULL CHECK(action_type IN (
        'Kill_Enemy', 'Complete_Quest', 'Defend_Territory', 'Sabotage', 
        'Escort_Caravan', 'Destroy_Hazard', 'Activate_Artifact'
    )),
    affected_faction TEXT NOT NULL,
    influence_delta REAL NOT NULL, -- -10.0 to +10.0
    action_timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    notes TEXT, -- Optional context
    
    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE,
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
    FOREIGN KEY (affected_faction) REFERENCES Factions(faction_name) ON DELETE CASCADE,
    
    CHECK(influence_delta BETWEEN -10.0 AND 10.0)
);

-- Indexes
CREATE INDEX idx_player_actions_character ON Player_Territorial_Actions(character_id);
CREATE INDEX idx_player_actions_sector ON Player_Territorial_Actions(sector_id);
CREATE INDEX idx_player_actions_faction ON Player_Territorial_Actions(affected_faction);
CREATE INDEX idx_player_actions_date ON Player_Territorial_Actions(action_timestamp DESC);
```

**Column Definitions:**

| Column | Type | Description |
| --- | --- | --- |
| action_id | INTEGER | Primary key |
| character_id | INTEGER | Player character |
| world_id | INTEGER | Which world |
| sector_id | INTEGER | Where action occurred |
| action_type | TEXT | Enum: 'Kill_Enemy', 'Complete_Quest', etc. |
| affected_faction | TEXT | Faction gaining/losing influence |
| influence_delta | REAL | -10.0 to +10.0 (can be negative for sabotage) |
| action_timestamp | TIMESTAMP | When action occurred |
| notes | TEXT | Optional context for debugging |

**Influence Delta Guidelines:**

- **Kill_Enemy:** +0.5 to +2.0 (based on enemy difficulty)
- **Complete_Quest:** +3.0 to +10.0 (based on quest tier)
- **Defend_Territory:** +5.0 (successful defense event)
- **Sabotage:** -5.0 (hostile action against faction)
- **Escort_Caravan:** +4.0 (Rust-Clan specific)
- **Destroy_Hazard:** +2.0 (environmental cleanup)
- **Activate_Artifact:** +6.0 (God-Sleeper/Jötun-Reader specific)

---

## IV. Seed Data

### Initial Territory Distribution

**Sector Influence Seed (10 sectors, 5 factions):**

```sql
-- Sector 1: Midgard (Trunk) - Independent
INSERT INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 1, 'Rust-Clans', 35.0, 'Independent'),
(1, 1, 'Iron-Banes', 30.0, 'Independent'),
(1, 1, 'Jötun-Readers', 20.0, 'Independent'),
(1, 1, 'God-Sleeper Cultists', 10.0, 'Independent'),
(1, 1, 'Independents', 5.0, 'Independent');

-- Sector 2: Muspelheim - Iron-Bane Stable Control
INSERT INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 2, 'Iron-Banes', 65.0, 'Stable'),
(1, 2, 'Rust-Clans', 20.0, 'Stable'),
(1, 2, 'Jötun-Readers', 10.0, 'Stable'),
(1, 2, 'God-Sleeper Cultists', 3.0, 'Stable'),
(1, 2, 'Independents', 2.0, 'Stable');

-- Sector 3: Niflheim - Contested (Jötun-Readers vs Rust-Clans)
INSERT INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 3, 'Jötun-Readers', 48.0, 'Contested'),
(1, 3, 'Rust-Clans', 45.0, 'Contested'),
(1, 3, 'Iron-Banes', 5.0, 'Contested'),
(1, 3, 'God-Sleeper Cultists', 2.0, 'Contested'),
(1, 3, 'Independents', 0.0, 'Contested');

-- Sector 4: Alfheim - Jötun-Reader Stable Control
INSERT INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 4, 'Jötun-Readers', 70.0, 'Stable'),
(1, 4, 'Independents', 15.0, 'Stable'),
(1, 4, 'God-Sleeper Cultists', 10.0, 'Stable'),
(1, 4, 'Rust-Clans', 3.0, 'Stable'),
(1, 4, 'Iron-Banes', 2.0, 'Stable');

-- Sector 5: Jötunheim - God-Sleeper Stable Control
INSERT INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 5, 'God-Sleeper Cultists', 60.0, 'Stable'),
(1, 5, 'Independents', 25.0, 'Stable'),
(1, 5, 'Jötun-Readers', 10.0, 'Stable'),
(1, 5, 'Rust-Clans', 3.0, 'Stable'),
(1, 5, 'Iron-Banes', 2.0, 'Stable');

-- Sector 6: Svartalfheim - Rust-Clan Stable Control
INSERT INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 6, 'Rust-Clans', 62.0, 'Stable'),
(1, 6, 'Independents', 20.0, 'Stable'),
(1, 6, 'Iron-Banes', 10.0, 'Stable'),
(1, 6, 'Jötun-Readers', 5.0, 'Stable'),
(1, 6, 'God-Sleeper Cultists', 3.0, 'Stable');

-- Sector 7: Vanaheim - Contested (Iron-Banes vs Independents)
INSERT INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 7, 'Independents', 50.0, 'Contested'),
(1, 7, 'Iron-Banes', 42.0, 'Contested'),
(1, 7, 'Rust-Clans', 5.0, 'Contested'),
(1, 7, 'Jötun-Readers', 2.0, 'Contested'),
(1, 7, 'God-Sleeper Cultists', 1.0, 'Contested');

-- Sector 8: Helheim - Independent
INSERT INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 8, 'Independents', 38.0, 'Independent'),
(1, 8, 'Iron-Banes', 25.0, 'Independent'),
(1, 8, 'God-Sleeper Cultists', 20.0, 'Independent'),
(1, 8, 'Rust-Clans', 12.0, 'Independent'),
(1, 8, 'Jötun-Readers', 5.0, 'Independent');

-- Sector 9: Asgard - Contested (God-Sleepers vs Jötun-Readers)
INSERT INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 9, 'God-Sleeper Cultists', 46.0, 'Contested'),
(1, 9, 'Jötun-Readers', 44.0, 'Contested'),
(1, 9, 'Independents', 8.0, 'Contested'),
(1, 9, 'Rust-Clans', 2.0, 'Contested'),
(1, 9, 'Iron-Banes', 0.0, 'Contested');

-- Sector 10: Valhalla - Iron-Bane Stable Control
INSERT INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 10, 'Iron-Banes', 68.0, 'Stable'),
(1, 10, 'Independents', 18.0, 'Stable'),
(1, 10, 'Rust-Clans', 10.0, 'Stable'),
(1, 10, 'Jötun-Readers', 3.0, 'Stable'),
(1, 10, 'God-Sleeper Cultists', 1.0, 'Stable');
```

**Initial Events:**

```sql
-- Event 1: Ongoing war in Niflheim (Sector 3)
INSERT INTO Faction_Wars (world_id, sector_id, faction_a, faction_b, war_balance, is_active)
VALUES (1, 3, 'Jötun-Readers', 'Rust-Clans', 3.0, 1);

INSERT INTO World_Events (world_id, sector_id, event_type, affected_faction, event_title, event_description, event_duration_days)
VALUES (
    1, 3, 'Faction_War', 'Jötun-Readers',
    'The Data Wars',
    'Jötun-Readers and Rust-Clans clash over control of ancient data repositories in Niflheim. The scavengers want the salvage, the archaeologists want the knowledge.',
    12
);

-- Event 2: God-Sleeper incursion in Asgard (Sector 9)
INSERT INTO World_Events (world_id, sector_id, event_type, affected_faction, event_title, event_description, event_duration_days)
VALUES (
    1, 9, 'Awakening_Ritual', 'God-Sleeper Cultists',
    'The Awakening',
    'God-Sleeper cultists attempt to reactivate dormant Jötun-Forged constructs. If successful, they will gain significant territorial control.',
    7
);

-- Event 3: Iron-Bane purge campaign in Helheim (Sector 8)
INSERT INTO World_Events (world_id, sector_id, event_type, affected_faction, event_title, event_description, event_duration_days)
VALUES (
    1, 8, 'Purge_Campaign', 'Iron-Banes',
    'The Last Protocol',
    'Iron-Banes launch systematic purge of Undying in Helheim. Their protocols demand complete eradication.',
    10
);
```

---

## V. Migration Script

**File:** `Data/v0.35.1_territory_control.sql`

```sql
-- ═══════════════════════════════════════════════════════════════════
-- v0.35.1: Territory Control & Dynamic World - Database Schema
-- Estimated Implementation: 7-10 hours
-- Dependencies: v0.33.1 (Faction Database), v0.13 (World State)
-- ═══════════════════════════════════════════════════════════════════

BEGIN TRANSACTION;

-- ═══════════════════════════════════════════════════════════════════
-- Table 1: Faction_Territory_Control
-- ═══════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Faction_Territory_Control (
    territory_control_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    faction_name TEXT NOT NULL,
    influence_value REAL NOT NULL DEFAULT 0.0,
    control_state TEXT NOT NULL CHECK(control_state IN ('Stable', 'Contested', 'War', 'Independent', 'Ruined')),
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
    FOREIGN KEY (faction_name) REFERENCES Factions(faction_name) ON DELETE CASCADE,
    UNIQUE(world_id, sector_id, faction_name)
);

CREATE INDEX idx_territory_control_sector ON Faction_Territory_Control(sector_id);
CREATE INDEX idx_territory_control_faction ON Faction_Territory_Control(faction_name);
CREATE INDEX idx_territory_control_state ON Faction_Territory_Control(control_state);
CREATE INDEX idx_territory_control_influence ON Faction_Territory_Control(influence_value DESC);

-- ═══════════════════════════════════════════════════════════════════
-- Table 2: Faction_Wars
-- ═══════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Faction_Wars (
    war_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    faction_a TEXT NOT NULL,
    faction_b TEXT NOT NULL,
    war_start_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    war_end_date TIMESTAMP,
    war_balance REAL DEFAULT 0.0,
    is_active BOOLEAN NOT NULL DEFAULT 1,
    victor TEXT,
    collateral_damage INTEGER DEFAULT 0,
    
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
    FOREIGN KEY (faction_a) REFERENCES Factions(faction_name) ON DELETE CASCADE,
    FOREIGN KEY (faction_b) REFERENCES Factions(faction_name) ON DELETE CASCADE,
    
    CHECK(faction_a != faction_b),
    CHECK(war_balance BETWEEN -100 AND 100)
);

CREATE INDEX idx_wars_active ON Faction_Wars(is_active) WHERE is_active = 1;
CREATE INDEX idx_wars_sector ON Faction_Wars(sector_id);
CREATE INDEX idx_wars_date ON Faction_Wars(war_start_date DESC);

-- ═══════════════════════════════════════════════════════════════════
-- Table 3: World_Events
-- ═══════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS World_Events (
    event_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id INTEGER NOT NULL,
    sector_id INTEGER,
    event_type TEXT NOT NULL CHECK(event_type IN (
        'Faction_War', 'Incursion', 'Supply_Raid', 'Diplomatic_Shift',
        'Catastrophe', 'Awakening_Ritual', 'Excavation_Discovery',
        'Purge_Campaign', 'Scavenger_Caravan'
    )),
    affected_faction TEXT,
    event_title TEXT NOT NULL,
    event_description TEXT NOT NULL,
    event_start_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    event_end_date TIMESTAMP,
    event_duration_days INTEGER DEFAULT 7,
    is_resolved BOOLEAN NOT NULL DEFAULT 0,
    player_influenced BOOLEAN NOT NULL DEFAULT 0,
    outcome TEXT,
    influence_change REAL DEFAULT 0.0,
    
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
    FOREIGN KEY (affected_faction) REFERENCES Factions(faction_name) ON DELETE CASCADE
);

CREATE INDEX idx_events_active ON World_Events(is_resolved) WHERE is_resolved = 0;
CREATE INDEX idx_events_sector ON World_Events(sector_id);
CREATE INDEX idx_events_faction ON World_Events(affected_faction);
CREATE INDEX idx_events_type ON World_Events(event_type);

-- ═══════════════════════════════════════════════════════════════════
-- Table 4: Player_Territorial_Actions
-- ═══════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS Player_Territorial_Actions (
    action_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    world_id INTEGER NOT NULL,
    sector_id INTEGER NOT NULL,
    action_type TEXT NOT NULL CHECK(action_type IN (
        'Kill_Enemy', 'Complete_Quest', 'Defend_Territory', 'Sabotage',
        'Escort_Caravan', 'Destroy_Hazard', 'Activate_Artifact'
    )),
    affected_faction TEXT NOT NULL,
    influence_delta REAL NOT NULL,
    action_timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    notes TEXT,
    
    FOREIGN KEY (character_id) REFERENCES Characters(character_id) ON DELETE CASCADE,
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
    FOREIGN KEY (affected_faction) REFERENCES Factions(faction_name) ON DELETE CASCADE,
    
    CHECK(influence_delta BETWEEN -10.0 AND 10.0)
);

CREATE INDEX idx_player_actions_character ON Player_Territorial_Actions(character_id);
CREATE INDEX idx_player_actions_sector ON Player_Territorial_Actions(sector_id);
CREATE INDEX idx_player_actions_faction ON Player_Territorial_Actions(affected_faction);
CREATE INDEX idx_player_actions_date ON Player_Territorial_Actions(action_timestamp DESC);

-- ═══════════════════════════════════════════════════════════════════
-- Seed Data: Initial Territory Distribution
-- ═══════════════════════════════════════════════════════════════════

-- (Seed data SQL from Section IV goes here)

COMMIT;
```

---

## VI. Deployment Instructions

### Step 1: Database Migration

```bash
# Apply migration
sqlite3 your_database.db < Data/v0.35.1_territory_control.sql
```

### Step 2: Verification Queries

```sql
-- Verify tables created
SELECT name FROM sqlite_master WHERE type='table' 
AND name LIKE '%Territory%' OR name LIKE '%Wars%' OR name LIKE '%Events%';

-- Verify indexes
SELECT name FROM sqlite_master WHERE type='index' 
AND name LIKE '%territory%' OR name LIKE '%wars%' OR name LIKE '%events%';

-- Verify seed data
SELECT sector_id, faction_name, influence_value, control_state 
FROM Faction_Territory_Control 
ORDER BY sector_id, influence_value DESC;

-- Check initial wars
SELECT war_id, sector_id, faction_a, faction_b, war_balance, is_active 
FROM Faction_Wars WHERE is_active = 1;

-- Check initial events
SELECT event_id, event_type, event_title, is_resolved 
FROM World_Events WHERE is_resolved = 0;
```

### Step 3: Manual Validation Checklist

- ✅ All 4 tables created successfully
- ✅ All indexes present
- ✅ Foreign key constraints enforced
- ✅ Check constraints validated (control_state enum, war_balance range)
- ✅ Seed data inserted (10 sectors × 5 factions = 50 rows in Faction_Territory_Control)
- ✅ Initial wars present (1 active war in Niflheim)
- ✅ Initial events present (3 ongoing events)
- ✅ No orphaned foreign keys

---

## VII. Query Examples for Service Layer

### Get Dominant Faction for Sector

```sql
SELECT faction_name, influence_value, control_state
FROM Faction_Territory_Control
WHERE sector_id = ?
ORDER BY influence_value DESC
LIMIT 1;
```

### Get All Contested Sectors

```sql
SELECT DISTINCT sector_id
FROM Faction_Territory_Control
WHERE control_state = 'Contested';
```

### Get Active Wars

```sql
SELECT war_id, sector_id, faction_a, faction_b, war_balance
FROM Faction_Wars
WHERE is_active = 1;
```

### Get Active Events for Sector

```sql
SELECT event_id, event_type, event_title, event_description, 
       event_start_date, event_duration_days
FROM World_Events
WHERE sector_id = ? AND is_resolved = 0
ORDER BY event_start_date DESC;
```

### Get Player's Recent Territorial Actions

```sql
SELECT action_type, affected_faction, influence_delta, 
       action_timestamp, sector_id
FROM Player_Territorial_Actions
WHERE character_id = ?
ORDER BY action_timestamp DESC
LIMIT 20;
```

### Calculate Total Player Influence for Faction

```sql
SELECT affected_faction, SUM(influence_delta) as total_influence
FROM Player_Territorial_Actions
WHERE character_id = ?
GROUP BY affected_faction
ORDER BY total_influence DESC;
```

---

## VIII. v5.0 Compliance Notes

### Layer 2 Voice (Diagnostic/Clinical)

**✅ Correct Database Naming:**

- `Faction_Territory_Control` (not `Faction_Dominion`)
- `influence_value` (not `magic_power`)
- `control_state` (not `blessing_status`)
- `war_balance` (not `holy_favor`)

**✅ Technology Framing:**

- Influence represents "signal strength" not "divine right"
- Wars are "system conflicts" not "holy wars"
- Events are "operational anomalies" not "divine interventions"

### ASCII Compliance

**✅ All Column Names ASCII-only:**

- No special characters in any column names
- No diacritics (ð, þ, ö) in database identifiers

**Display Text Exception:**

- `faction_name` values store "Jötun-Readers" with ö (display text)
- This is acceptable per v5.0 standards

---

## IX. Success Criteria

**Database Schema:**

- ✅ 4 new tables created with constraints
- ✅ 13 indexes for query performance
- ✅ Foreign key relationships enforced
- ✅ Check constraints validated

**Seed Data:**

- ✅ 50 initial influence records (10 sectors × 5 factions)
- ✅ 1 initial war (Jötun-Readers vs Rust-Clans in Niflheim)
- ✅ 3 initial events (Data Wars, Awakening, Purge Campaign)

**Documentation:**

- ✅ Complete schema documentation
- ✅ Column definitions and business rules
- ✅ Query examples for service layer
- ✅ Deployment instructions
- ✅ Verification queries

---

**Status:** Implementation-ready database schema for Territory Control system complete.

**Next:** v0.35.2 (Territory Mechanics & Faction Wars)