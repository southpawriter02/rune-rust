-- =====================================================
-- v0.30.1: Niflheim Database Schema & Room Templates
-- =====================================================
-- Version: v0.30.1
-- Author: Rune & Rust Development Team
-- Date: 2025-11-16
-- Prerequisites: v0.29.1 (Muspelheim database structure)
-- =====================================================
-- Document ID: RR-SPEC-v0.30.1-DATABASE
-- Parent Specification: v0.30 Niflheim Biome Implementation
-- Status: Design Complete — Ready for Implementation
-- Timeline: 8-12 hours
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- TABLE CREATION (IF NOT EXISTS from v0.29.1)
-- =====================================================

-- Tables should already exist from v0.29.1, but we include
-- CREATE TABLE IF NOT EXISTS for safety.

-- Table: Biomes (already created in v0.29.1)
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
);

CREATE INDEX IF NOT EXISTS idx_biomes_name ON Biomes(biome_name);
CREATE INDEX IF NOT EXISTS idx_biomes_active ON Biomes(is_active);

-- Table: Biome_RoomTemplates (already created in v0.29.1)
CREATE TABLE IF NOT EXISTS Biome_RoomTemplates (
    template_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    template_name TEXT NOT NULL,
    template_description TEXT,
    room_size_category TEXT CHECK(room_size_category IN ('Small', 'Medium', 'Large', 'XLarge')),
    min_connections INTEGER DEFAULT 1,
    max_connections INTEGER DEFAULT 4,
    can_be_entrance INTEGER DEFAULT 0,
    can_be_exit INTEGER DEFAULT 0,
    can_be_hub INTEGER DEFAULT 0,
    hazard_density TEXT CHECK(hazard_density IN ('None', 'Low', 'Medium', 'High', 'Extreme')),
    enemy_spawn_weight INTEGER DEFAULT 100,
    resource_spawn_chance REAL DEFAULT 0.3,
    wfc_adjacency_rules TEXT,
    z_level TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_biome_room_templates_biome ON Biome_RoomTemplates(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_room_templates_size ON Biome_RoomTemplates(room_size_category);

-- Table: Biome_ResourceDrops (already created in v0.29.1)
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
);

CREATE INDEX IF NOT EXISTS idx_biome_resources_biome ON Biome_ResourceDrops(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_resources_tier ON Biome_ResourceDrops(resource_tier);
CREATE INDEX IF NOT EXISTS idx_biome_resources_rarity ON Biome_ResourceDrops(rarity);

-- Table: Characters_BiomeStatus (already created in v0.29.1)
CREATE TABLE IF NOT EXISTS Characters_BiomeStatus (
    status_id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    biome_id INTEGER NOT NULL,
    first_entry_timestamp TEXT,
    total_time_seconds INTEGER DEFAULT 0,
    rooms_explored INTEGER DEFAULT 0,
    enemies_defeated INTEGER DEFAULT 0,
    cold_damage_taken INTEGER DEFAULT 0,
    times_died_to_cold INTEGER DEFAULT 0,
    resources_collected INTEGER DEFAULT 0,
    has_reached_frost_giant INTEGER DEFAULT 0,
    last_updated TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE,
    UNIQUE(character_id, biome_id)
);

CREATE INDEX IF NOT EXISTS idx_biome_status_character ON Characters_BiomeStatus(character_id);
CREATE INDEX IF NOT EXISTS idx_biome_status_biome ON Characters_BiomeStatus(biome_id);

-- =====================================================
-- DATA SEEDING: BIOMES
-- =====================================================

-- Niflheim: The Cryo-Facilities
-- biome_id: 5
-- Verticality: Multiple (can appear in both Roots and Canopy)
-- Level Range: 7-12 (Mid-to-Late Game, parallels Muspelheim)
INSERT OR IGNORE INTO Biomes (
    biome_id,
    biome_name,
    biome_description,
    z_level_restriction,
    ambient_condition_id,
    min_character_level,
    max_character_level,
    is_active
) VALUES (
    5,
    'Niflheim',
    'The Cryo-Facilities - catastrophic coolant system failure has created permanent flash-frozen sectors. Deep cryogenic laboratories and high-altitude research outposts locked in absolute zero. Not natural winter, but industrial refrigeration run amok.',
    '[Roots,Canopy]',
    NULL,
    7,
    12,
    1
);

-- =====================================================
-- DATA SEEDING: ROOM TEMPLATES
-- =====================================================
-- 8 Room Templates: 4 for [Roots], 4 for [Canopy]
-- =====================================================

-- =====================================================
-- ROOTS TEMPLATES (Deep Cryogenic Facilities)
-- =====================================================

-- Template 1: Cryo-Storage Bay (Large, Roots, Hub)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    5, 'Cryo-Storage Bay',
    'Vast chambers lined with row upon row of cryogenic suspension pods, their contents frozen for eight centuries. Frost coats every surface in crystalline sheets. The air is so cold it feels like breathing shards of glass. Many pods have shattered, their occupants preserved in blocks of ice.',
    'Large', 3, 5, 1, 0, 1, 'Medium', 120, 0.7,
    '{"allow": ["Coolant Pumping Station", "Ice-Choked Maintenance Tunnel"], "forbid": ["The Frost-Giant Tomb"]}',
    'Roots'
);

-- Template 2: Coolant Pumping Station (Medium, Roots)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    5, 'Coolant Pumping Station',
    'Industrial-scale refrigeration equipment dominates the space. Massive pumps stand silent, their coolant lines ruptured and leaking liquid nitrogen in slow, frozen cascades. The floor is a treacherous sheet of ice. Pressure gauges are frozen at critical readings.',
    'Medium', 2, 3, 0, 0, 0, 'High', 100, 0.5,
    '{"allow": ["Cryo-Storage Bay", "Ice-Choked Maintenance Tunnel", "The Frost-Giant Tomb"], "forbid": []}',
    'Roots'
);

-- Template 3: Ice-Choked Maintenance Tunnel (Small, Roots)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    5, 'Ice-Choked Maintenance Tunnel',
    'A narrow service corridor where coolant breach turned moisture into ice stalagmites and stalactites. The passage is treacherous, forcing careful navigation between jagged formations. Frozen condensation makes every surface slick. Emergency lighting flickers through layers of frost.',
    'Small', 2, 2, 0, 1, 0, 'Medium', 80, 0.3,
    '{"allow": ["Cryo-Storage Bay", "Coolant Pumping Station"], "forbid": ["The Frost-Giant Tomb"]}',
    'Roots'
);

-- Template 4: The Frost-Giant Tomb (Large, Roots, BOSS ROOM)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    5, 'The Frost-Giant Tomb',
    'A colossal chamber where a dormant Jotun-Forged warmachine stands frozen in place, its systems locked in cryogenic stasis. Ice has formed around its massive frame like a crystal sarcophagus. The silence here is absolute, broken only by the occasional crack of shifting ice.',
    'Large', 1, 2, 0, 0, 0, 'Extreme', 10, 0.95,
    '{"allow": ["Coolant Pumping Station"], "forbid": ["Cryo-Storage Bay", "Ice-Choked Maintenance Tunnel"]}',
    'Roots'
);

-- =====================================================
-- CANOPY TEMPLATES (High-Altitude Flash-Frozen Facilities)
-- =====================================================

-- Template 5: Flash-Frozen Skywalk (Small, Canopy)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    5, 'Flash-Frozen Skywalk',
    'A transparent bridge connecting high-altitude research platforms. When atmospheric shields failed, everything froze instantly. Pre-Glitch scientists are preserved mid-stride, their final moments captured in ice. The view beyond shows a world locked in winter. The bridge itself is dangerously slippery.',
    'Small', 2, 2, 0, 1, 0, 'High', 90, 0.4,
    '{"allow": ["Rimed Laboratory", "High-Altitude Observatory"], "forbid": ["Atmospheric Control Chamber"]}',
    'Canopy'
);

-- Template 6: Rimed Laboratory (Medium, Canopy, Hub)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    5, 'Rimed Laboratory',
    'A high-tech research facility where atmospheric failure flash-froze everything. Holographic displays are frozen mid-projection. Equipment is coated in thick layers of rime. Data terminals are locked solid. The researchers bodies are preserved at their workstations, a moment of panic captured forever.',
    'Medium', 2, 4, 1, 0, 1, 'Medium', 110, 0.75,
    '{"allow": ["Flash-Frozen Skywalk", "Atmospheric Control Chamber", "High-Altitude Observatory"], "forbid": []}',
    'Canopy'
);

-- Template 7: Atmospheric Control Chamber (Large, Canopy)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    5, 'Atmospheric Control Chamber',
    'The heart of the environmental failure. Massive climate regulators are locked in catastrophic failure states. Warning klaxons are frozen mid-blare. The breach point where external cold flooded in is clearly visible, a jagged tear in the walls ringed with ice formations.',
    'Large', 2, 3, 0, 0, 0, 'Extreme', 60, 0.6,
    '{"allow": ["Rimed Laboratory"], "forbid": ["Flash-Frozen Skywalk", "High-Altitude Observatory"]}',
    'Canopy'
);

-- Template 8: High-Altitude Observatory (Medium, Canopy)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    5, 'High-Altitude Observatory',
    'A research outpost with panoramic views of the frozen world. Observation equipment is coated in frost. The exposed location means extreme cold and wind chill. Ice crystals have formed intricate patterns across every surface, beautiful but deadly.',
    'Medium', 1, 3, 0, 0, 0, 'Medium', 80, 0.5,
    '{"allow": ["Flash-Frozen Skywalk", "Rimed Laboratory"], "forbid": ["Atmospheric Control Chamber"]}',
    'Canopy'
);

-- =====================================================
-- DATA SEEDING: RESOURCE DROPS
-- =====================================================
-- 9 Resource Types across Tiers 2-5
-- =====================================================

-- =====================================================
-- TIER 2 (Common)
-- =====================================================

-- Resource 1: Cryo-Coolant Fluid (Tier 2, Very Common)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (5, 'Cryo-Coolant Fluid', 'Supercooled liquid nitrogen from ruptured coolant lines. Essential for thermal regulation equipment and cryogenic component crafting. Still viable after 800 years of system failure.', 2, 'Common', 0.35, 1, 3, 0, 150);

-- Resource 2: Frost-Lichen (Tier 2, Common)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (5, 'Frost-Lichen', 'Hardy organism that thrives in extreme cold. Glows with faint bioluminescence. Used in cold resistance potions and medicinal compounds. Grows on frozen machinery and ice formations.', 2, 'Common', 0.30, 2, 5, 0, 120);

-- Resource 3: Frozen Circuitry (Tier 2, Common)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (5, 'Frozen Circuitry', 'Electronic components preserved by extreme cold. Supercooled circuits operate with reduced resistance. Used in precision equipment and advanced sensors. Surprisingly functional despite 800 years of freezing.', 2, 'Common', 0.25, 1, 2, 0, 100);

-- =====================================================
-- TIER 3 (Uncommon)
-- =====================================================

-- Resource 4: Ice-Bear Pelt (Tier 3, Uncommon)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (5, 'Ice-Bear Pelt', 'Thick white fur from ice-adapted predators. Provides exceptional thermal insulation. Used in cold resistance armor and thermal suits. The fur has remarkable heat retention properties.', 3, 'Uncommon', 0.20, 1, 1, 0, 80);

-- Resource 5: Supercooled Alloy (Tier 3, Uncommon)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (5, 'Supercooled Alloy', 'Metal alloy that has undergone phase changes at cryogenic temperatures. Exhibits unusual strength and brittleness. Used in specialized weapon and armor crafting. The material structure is unique to extreme cold exposure.', 3, 'Uncommon', 0.18, 1, 2, 1, 70);

-- =====================================================
-- TIER 4 (Rare)
-- =====================================================

-- Resource 6: Pristine Ice Core (Tier 4, Rare, Runic Catalyst)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (5, 'Pristine Ice Core', 'Perfectly formed ice crystal with unique molecular structure. Acts as runic catalyst for Ice-based effects. Found in the deepest cryogenic chambers. The crystal maintains absolute zero temperature indefinitely.', 4, 'Rare', 0.10, 1, 2, 1, 40);

-- Resource 7: Cryogenic Data-Slate (Tier 4, Rare)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (5, 'Cryogenic Data-Slate', 'Pre-Glitch research data perfectly preserved by flash-freezing. Contains atmospheric control protocols and climate regulation data. Valuable to Jotun-Readers and scholars. The information is coherent and uncorrupted.', 4, 'Rare', 0.08, 1, 1, 1, 30);

-- =====================================================
-- TIER 5 (Legendary)
-- =====================================================

-- Resource 8: Heart of the Frost-Giant (Tier 5, Legendary, Boss Drop)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (5, 'Heart of the Frost-Giant', 'The core reactor of a dormant Jotun-Forged warmachine that was flash-frozen. Contains a self-sustaining cryogenic field. Essential component for legendary crafting. Radiates absolute zero in a contained field. Only obtainable from Frost-Giant encounters.', 5, 'Legendary', 0.05, 1, 1, 1, 5);

-- Resource 9: Eternal Frost Crystal (Tier 5, Legendary)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (5, 'Eternal Frost Crystal', 'A perfect crystalline formation found only in the deepest cryo-storage chambers. Maintains absolute zero temperature through unknown physics. Used in myth-forged equipment with permanent cold effects. The crystal seems to violate thermodynamic laws.', 5, 'Legendary', 0.03, 1, 1, 1, 3);

COMMIT;

-- =====================================================
-- END v0.30.1 SEEDING SCRIPT
-- =====================================================

-- =====================================================
-- VERIFICATION QUERIES (Run these to verify seeding)
-- =====================================================

-- Test 1: Biome Entry Exists
-- SELECT COUNT(*) FROM Biomes WHERE biome_id = 5;
-- Expected: 1

-- Test 2: Room Template Count
-- SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 5;
-- Expected: 8 (4 Roots, 4 Canopy)

-- Test 3: Verticality Distribution
-- SELECT z_level, COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 5 GROUP BY z_level;
-- Expected: Roots: 4, Canopy: 4

-- Test 4: Resource Drop Count
-- SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 5;
-- Expected: 9

-- Test 5: Quality Tier Distribution
-- SELECT resource_tier, COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 5 GROUP BY resource_tier;
-- Expected: Tier 2: 3, Tier 3: 2, Tier 4: 2, Tier 5: 2

-- Test 6: Drop Weight Validation
-- SELECT resource_name, weight FROM Biome_ResourceDrops WHERE biome_id = 5 ORDER BY weight DESC;
-- Expected: Cryo-Coolant Fluid highest (150), legendaries lowest (5, 3)

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] Biomes table contains Niflheim entry (biome_id: 5)
-- [ ] 8 room templates seeded (4 Roots, 4 Canopy)
-- [ ] All templates have valid spawn weights
-- [ ] 9 resource types seeded across 4 tiers
-- [ ] Resource drop weights correctly calibrated
-- [ ] SQL migration script executes without errors
-- [ ] All foreign key constraints satisfied
-- [ ] All descriptions use v5.0 voice (technology, not magic)
-- [ ] Entity names are ASCII-compliant
-- [ ] Database integrity tests pass
-- =====================================================
