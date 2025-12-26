-- ═══════════════════════════════════════════════════════════════════
-- v0.35.1: Territory Control & Dynamic World - Database Schema
-- ═══════════════════════════════════════════════════════════════════
-- Document ID: RR-SPEC-v0.35.1-TERRITORY-DATABASE
-- Parent Specification: v0.35: Territory Control & Dynamic World
-- Status: Implementation Ready
-- Timeline: 7-10 hours
-- Dependencies: v0.33.1 (Faction Database), v0.13 (World State Persistence)
-- ═══════════════════════════════════════════════════════════════════

BEGIN TRANSACTION;

-- ═══════════════════════════════════════════════════════════════════
-- Table 0: Worlds (New - Foundation for territory system)
-- ═══════════════════════════════════════════════════════════════════
-- Represents game worlds. Currently single world (Aethelgard) but
-- structured for future multi-world support.

CREATE TABLE IF NOT EXISTS Worlds (
    world_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_name TEXT NOT NULL UNIQUE,
    world_description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_worlds_name ON Worlds(world_name);

-- Seed initial world: Aethelgard
INSERT OR IGNORE INTO Worlds (world_id, world_name, world_description)
VALUES (
    1,
    'Aethelgard',
    'The World-Tree, an ancient structure containing the remnants of pre-Glitch civilization. A failing technological megastructure where reality coherence decreases with depth.'
);

-- ═══════════════════════════════════════════════════════════════════
-- Table 1: Sectors (New - Territorial divisions)
-- ═══════════════════════════════════════════════════════════════════
-- Represents major territorial sectors within Aethelgard.
-- Links to Biomes where applicable, but sectors are political/territorial
-- divisions while biomes are environmental/generation zones.

CREATE TABLE IF NOT EXISTS Sectors (
    sector_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id INTEGER NOT NULL,
    sector_name TEXT NOT NULL,
    sector_description TEXT,
    biome_id INTEGER, -- Optional link to Biomes table for environmental data
    z_level TEXT, -- 'Trunk', 'Roots', 'Canopy', etc.
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE SET NULL,
    UNIQUE(world_id, sector_name)
);

CREATE INDEX IF NOT EXISTS idx_sectors_world ON Sectors(world_id);
CREATE INDEX IF NOT EXISTS idx_sectors_name ON Sectors(sector_name);
CREATE INDEX IF NOT EXISTS idx_sectors_biome ON Sectors(biome_id);

-- Seed initial 10 sectors for Aethelgard
INSERT OR IGNORE INTO Sectors (sector_id, world_id, sector_name, sector_description, biome_id, z_level)
VALUES
(1, 1, 'Midgard', 'The Trunk level - main operational zones and habitable sectors. Neutral ground where Rust-Clans establish their trade networks.', NULL, 'Trunk'),
(2, 1, 'Muspelheim', 'Volcanic geothermal sectors with extreme temperatures. Iron-Banes patrol heavily to contain Runic Blight spreading from corrupted forges.', 3, 'Roots'),
(3, 1, 'Niflheim', 'Cryo-facilities frozen in catastrophic failure. Data repositories make this contested ground between Jötun-Readers and Rust-Clans.', 5, 'Roots'),
(4, 1, 'Alfheim', 'Ancient archives and data preservation zones. Jötun-Readers maintain primary research facilities here.', NULL, 'Canopy'),
(5, 1, 'Jotunheim', 'Assembly yards containing dormant Jötun-Forged constructs. Sacred ground for God-Sleeper cultists.', NULL, 'Trunk'),
(6, 1, 'Svartalfheim', 'Deep salvage zones rich in scrap and components. Rust-Clan stronghold with established trade routes.', NULL, 'Roots'),
(7, 1, 'Vanaheim', 'Transition zones between Trunk and Canopy. Contested between Independents and Iron-Banes.', NULL, 'Trunk'),
(8, 1, 'Helheim', 'Corrupted sectors with high Undying presence. Dangerous independent territory.', NULL, 'Roots'),
(9, 1, 'Asgard', 'Upper command structures near Canopy. Contested between God-Sleepers and Jötun-Readers.', NULL, 'Canopy'),
(10, 1, 'Valhalla', 'Historical defense installations. Iron-Bane operational headquarters.', NULL, 'Trunk');

-- ═══════════════════════════════════════════════════════════════════
-- Table 2: Faction_Territory_Control
-- ═══════════════════════════════════════════════════════════════════
-- Tracks faction influence values per sector. Each faction has an
-- influence percentage (0-100) in each sector. Control state is
-- calculated from influence distribution.

CREATE TABLE IF NOT EXISTS Faction_Territory_Control (
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

CREATE INDEX IF NOT EXISTS idx_territory_control_sector ON Faction_Territory_Control(sector_id);
CREATE INDEX IF NOT EXISTS idx_territory_control_faction ON Faction_Territory_Control(faction_name);
CREATE INDEX IF NOT EXISTS idx_territory_control_state ON Faction_Territory_Control(control_state);
CREATE INDEX IF NOT EXISTS idx_territory_control_influence ON Faction_Territory_Control(influence_value DESC);

-- ═══════════════════════════════════════════════════════════════════
-- Table 3: Faction_Wars
-- ═══════════════════════════════════════════════════════════════════
-- Tracks active and historical faction wars over territory.
-- Wars occur when two factions reach high influence in same sector.

CREATE TABLE IF NOT EXISTS Faction_Wars (
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

CREATE INDEX IF NOT EXISTS idx_wars_active ON Faction_Wars(is_active) WHERE is_active = 1;
CREATE INDEX IF NOT EXISTS idx_wars_sector ON Faction_Wars(sector_id);
CREATE INDEX IF NOT EXISTS idx_wars_date ON Faction_Wars(war_start_date DESC);

-- ═══════════════════════════════════════════════════════════════════
-- Table 4: World_Events
-- ═══════════════════════════════════════════════════════════════════
-- Catalog dynamic world events affecting territories.
-- Events drive narrative and create quest opportunities.

CREATE TABLE IF NOT EXISTS World_Events (
    event_id INTEGER PRIMARY KEY AUTOINCREMENT,
    world_id INTEGER NOT NULL,
    sector_id INTEGER, -- NULL for world-wide events
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

CREATE INDEX IF NOT EXISTS idx_events_active ON World_Events(is_resolved) WHERE is_resolved = 0;
CREATE INDEX IF NOT EXISTS idx_events_sector ON World_Events(sector_id);
CREATE INDEX IF NOT EXISTS idx_events_faction ON World_Events(affected_faction);
CREATE INDEX IF NOT EXISTS idx_events_type ON World_Events(event_type);

-- ═══════════════════════════════════════════════════════════════════
-- Table 5: Player_Territorial_Actions
-- ═══════════════════════════════════════════════════════════════════
-- Log player actions that affect faction influence.
-- Used to calculate player impact on territorial control.

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
    influence_delta REAL NOT NULL, -- -10.0 to +10.0
    action_timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    notes TEXT, -- Optional context

    FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
    FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
    FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
    FOREIGN KEY (affected_faction) REFERENCES Factions(faction_name) ON DELETE CASCADE,

    CHECK(influence_delta BETWEEN -10.0 AND 10.0)
);

CREATE INDEX IF NOT EXISTS idx_player_actions_character ON Player_Territorial_Actions(character_id);
CREATE INDEX IF NOT EXISTS idx_player_actions_sector ON Player_Territorial_Actions(sector_id);
CREATE INDEX IF NOT EXISTS idx_player_actions_faction ON Player_Territorial_Actions(affected_faction);
CREATE INDEX IF NOT EXISTS idx_player_actions_date ON Player_Territorial_Actions(action_timestamp DESC);

-- ═══════════════════════════════════════════════════════════════════
-- SEED DATA: Initial Territory Distribution
-- ═══════════════════════════════════════════════════════════════════

-- Sector 1: Midgard (Trunk) - Independent
INSERT OR IGNORE INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 1, 'RustClans', 35.0, 'Independent'),
(1, 1, 'IronBanes', 30.0, 'Independent'),
(1, 1, 'JotunReaders', 20.0, 'Independent'),
(1, 1, 'GodSleeperCultists', 10.0, 'Independent'),
(1, 1, 'Independents', 5.0, 'Independent');

-- Sector 2: Muspelheim - Iron-Bane Stable Control
INSERT OR IGNORE INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 2, 'IronBanes', 65.0, 'Stable'),
(1, 2, 'RustClans', 20.0, 'Stable'),
(1, 2, 'JotunReaders', 10.0, 'Stable'),
(1, 2, 'GodSleeperCultists', 3.0, 'Stable'),
(1, 2, 'Independents', 2.0, 'Stable');

-- Sector 3: Niflheim - Contested (Jötun-Readers vs Rust-Clans)
INSERT OR IGNORE INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 3, 'JotunReaders', 48.0, 'Contested'),
(1, 3, 'RustClans', 45.0, 'Contested'),
(1, 3, 'IronBanes', 5.0, 'Contested'),
(1, 3, 'GodSleeperCultists', 2.0, 'Contested'),
(1, 3, 'Independents', 0.0, 'Contested');

-- Sector 4: Alfheim - Jötun-Reader Stable Control
INSERT OR IGNORE INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 4, 'JotunReaders', 70.0, 'Stable'),
(1, 4, 'Independents', 15.0, 'Stable'),
(1, 4, 'GodSleeperCultists', 10.0, 'Stable'),
(1, 4, 'RustClans', 3.0, 'Stable'),
(1, 4, 'IronBanes', 2.0, 'Stable');

-- Sector 5: Jötunheim - God-Sleeper Stable Control
INSERT OR IGNORE INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 5, 'GodSleeperCultists', 60.0, 'Stable'),
(1, 5, 'Independents', 25.0, 'Stable'),
(1, 5, 'JotunReaders', 10.0, 'Stable'),
(1, 5, 'RustClans', 3.0, 'Stable'),
(1, 5, 'IronBanes', 2.0, 'Stable');

-- Sector 6: Svartalfheim - Rust-Clan Stable Control
INSERT OR IGNORE INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 6, 'RustClans', 62.0, 'Stable'),
(1, 6, 'Independents', 20.0, 'Stable'),
(1, 6, 'IronBanes', 10.0, 'Stable'),
(1, 6, 'JotunReaders', 5.0, 'Stable'),
(1, 6, 'GodSleeperCultists', 3.0, 'Stable');

-- Sector 7: Vanaheim - Contested (Iron-Banes vs Independents)
INSERT OR IGNORE INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 7, 'Independents', 50.0, 'Contested'),
(1, 7, 'IronBanes', 42.0, 'Contested'),
(1, 7, 'RustClans', 5.0, 'Contested'),
(1, 7, 'JotunReaders', 2.0, 'Contested'),
(1, 7, 'GodSleeperCultists', 1.0, 'Contested');

-- Sector 8: Helheim - Independent
INSERT OR IGNORE INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 8, 'Independents', 38.0, 'Independent'),
(1, 8, 'IronBanes', 25.0, 'Independent'),
(1, 8, 'GodSleeperCultists', 20.0, 'Independent'),
(1, 8, 'RustClans', 12.0, 'Independent'),
(1, 8, 'JotunReaders', 5.0, 'Independent');

-- Sector 9: Asgard - Contested (God-Sleepers vs Jötun-Readers)
INSERT OR IGNORE INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 9, 'GodSleeperCultists', 46.0, 'Contested'),
(1, 9, 'JotunReaders', 44.0, 'Contested'),
(1, 9, 'Independents', 8.0, 'Contested'),
(1, 9, 'RustClans', 2.0, 'Contested'),
(1, 9, 'IronBanes', 0.0, 'Contested');

-- Sector 10: Valhalla - Iron-Bane Stable Control
INSERT OR IGNORE INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
VALUES
(1, 10, 'IronBanes', 68.0, 'Stable'),
(1, 10, 'Independents', 18.0, 'Stable'),
(1, 10, 'RustClans', 10.0, 'Stable'),
(1, 10, 'JotunReaders', 3.0, 'Stable'),
(1, 10, 'GodSleeperCultists', 1.0, 'Stable');

-- ═══════════════════════════════════════════════════════════════════
-- SEED DATA: Initial Wars
-- ═══════════════════════════════════════════════════════════════════

-- Event 1: Ongoing war in Niflheim (Sector 3)
INSERT OR IGNORE INTO Faction_Wars (war_id, world_id, sector_id, faction_a, faction_b, war_balance, is_active)
VALUES (1, 1, 3, 'JotunReaders', 'RustClans', 3.0, 1);

-- ═══════════════════════════════════════════════════════════════════
-- SEED DATA: Initial World Events
-- ═══════════════════════════════════════════════════════════════════

-- Event 1: Data Wars in Niflheim
INSERT OR IGNORE INTO World_Events (
    event_id, world_id, sector_id, event_type, affected_faction,
    event_title, event_description, event_duration_days
) VALUES (
    1, 1, 3, 'Faction_War', 'JotunReaders',
    'The Data Wars',
    'Jötun-Readers and Rust-Clans clash over control of ancient data repositories in Niflheim. The scavengers want the salvage, the archaeologists want the knowledge. Player actions could tip the balance.',
    12
);

-- Event 2: God-Sleeper incursion in Asgard (Sector 9)
INSERT OR IGNORE INTO World_Events (
    event_id, world_id, sector_id, event_type, affected_faction,
    event_title, event_description, event_duration_days
) VALUES (
    2, 1, 9, 'Awakening_Ritual', 'GodSleeperCultists',
    'The Awakening',
    'God-Sleeper cultists attempt to reactivate dormant Jötun-Forged constructs in Asgard. If successful, they will gain significant territorial control. If disrupted, their influence will collapse.',
    7
);

-- Event 3: Iron-Bane purge campaign in Helheim (Sector 8)
INSERT OR IGNORE INTO World_Events (
    event_id, world_id, sector_id, event_type, affected_faction,
    event_title, event_description, event_duration_days
) VALUES (
    3, 1, 8, 'Purge_Campaign', 'IronBanes',
    'The Last Protocol',
    'Iron-Banes launch systematic purge of Undying in Helheim. Their protocols demand complete eradication of corrupted processes. Success could establish Iron-Bane presence in this independent sector.',
    10
);

COMMIT;

-- ═══════════════════════════════════════════════════════════════════
-- VERIFICATION QUERIES (Run these to verify seeding)
-- ═══════════════════════════════════════════════════════════════════

-- Test 1: Table Existence
-- SELECT name FROM sqlite_master WHERE type='table'
-- AND (name LIKE '%Territory%' OR name LIKE '%Wars%' OR name LIKE '%Events%' OR name IN ('Worlds', 'Sectors'))
-- ORDER BY name;
-- Expected: 6 tables (Worlds, Sectors, Faction_Territory_Control, Faction_Wars, World_Events, Player_Territorial_Actions)

-- Test 2: Index Existence
-- SELECT name FROM sqlite_master WHERE type='index'
-- AND (name LIKE '%territory%' OR name LIKE '%wars%' OR name LIKE '%events%' OR name LIKE '%worlds%' OR name LIKE '%sectors%')
-- ORDER BY name;
-- Expected: 13+ indexes

-- Test 3: Worlds and Sectors Seeded
-- SELECT w.world_id, w.world_name, COUNT(s.sector_id) as sector_count
-- FROM Worlds w
-- LEFT JOIN Sectors s ON w.world_id = s.world_id
-- GROUP BY w.world_id;
-- Expected: 1 world (Aethelgard) with 10 sectors

-- Test 4: Territory Control Distribution
-- SELECT s.sector_name, f.faction_name, f.influence_value, f.control_state
-- FROM Faction_Territory_Control f
-- JOIN Sectors s ON f.sector_id = s.sector_id
-- ORDER BY s.sector_id, f.influence_value DESC;
-- Expected: 50 rows (10 sectors × 5 factions)

-- Test 5: Control States Summary
-- SELECT control_state, COUNT(DISTINCT sector_id) as sector_count
-- FROM Faction_Territory_Control
-- GROUP BY control_state
-- ORDER BY sector_count DESC;
-- Expected: Stable: 5, Contested: 3, Independent: 2

-- Test 6: Active Wars
-- SELECT w.war_id, s.sector_name, w.faction_a, w.faction_b, w.war_balance, w.is_active
-- FROM Faction_Wars w
-- JOIN Sectors s ON w.sector_id = s.sector_id
-- WHERE w.is_active = 1;
-- Expected: 1 active war (Niflheim: JotunReaders vs RustClans)

-- Test 7: Active Events
-- SELECT e.event_id, s.sector_name, e.event_type, e.event_title, e.is_resolved
-- FROM World_Events e
-- JOIN Sectors s ON e.sector_id = s.sector_id
-- WHERE e.is_resolved = 0
-- ORDER BY e.event_start_date;
-- Expected: 3 ongoing events (Data Wars, Awakening, Last Protocol)

-- Test 8: Foreign Key Validation
-- PRAGMA foreign_key_check;
-- Expected: No results (empty = all FKs valid)

-- Test 9: Check Constraint Validation (war_balance range)
-- INSERT INTO Faction_Wars (world_id, sector_id, faction_a, faction_b, war_balance)
-- VALUES (1, 1, 'IronBanes', 'RustClans', 150.0);
-- Expected: CHECK constraint failed (war_balance must be -100 to +100)

-- Test 10: Check Constraint Validation (influence_delta range)
-- INSERT INTO Player_Territorial_Actions (character_id, world_id, sector_id, action_type, affected_faction, influence_delta)
-- VALUES (1, 1, 1, 'Complete_Quest', 'IronBanes', 20.0);
-- Expected: CHECK constraint failed (influence_delta must be -10.0 to +10.0)

-- ═══════════════════════════════════════════════════════════════════
-- SUCCESS CRITERIA CHECKLIST
-- ═══════════════════════════════════════════════════════════════════
-- [ ] Worlds table created (1 world seeded: Aethelgard)
-- [ ] Sectors table created (10 sectors seeded)
-- [ ] Faction_Territory_Control table created (50 influence records seeded)
-- [ ] Faction_Wars table created (1 active war seeded)
-- [ ] World_Events table created (3 ongoing events seeded)
-- [ ] Player_Territorial_Actions table created (empty, ready for runtime data)
-- [ ] All 13+ indexes created for performance optimization
-- [ ] All foreign key constraints enforced
-- [ ] All check constraints validated
-- [ ] Initial territory distribution established (5 stable, 3 contested, 2 independent)
-- [ ] Migration script executes without errors
-- [ ] v5.0 compliance: Layer 2 voice (technology, not mythology)
-- [ ] ASCII-compliant column names
-- ═══════════════════════════════════════════════════════════════════
