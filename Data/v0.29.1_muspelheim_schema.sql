-- =====================================================
-- v0.29.1: Muspelheim Database Schema & Room Templates
-- =====================================================
-- Version: v0.29.1
-- Author: Rune & Rust Development Team
-- Date: 2025-11-15
-- Prerequisites: v0.10-v0.12 (Dynamic Room Engine base tables)
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- TABLE CREATION
-- =====================================================

-- Table: Biomes
-- Core biome metadata (migrating from JSON to database)
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

-- Table: Biome_RoomTemplates
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
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_biome_room_templates_biome ON Biome_RoomTemplates(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_room_templates_size ON Biome_RoomTemplates(room_size_category);

-- Table: Biome_EnvironmentalFeatures (structure only, content in v0.29.2)
CREATE TABLE IF NOT EXISTS Biome_EnvironmentalFeatures (
    feature_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    feature_name TEXT NOT NULL,
    feature_type TEXT CHECK(feature_type IN ('Hazard', 'Terrain', 'Ambient')),
    feature_description TEXT,
    damage_dice INTEGER DEFAULT 0,
    damage_type TEXT,
    status_effects_json TEXT,
    spawn_weight INTEGER DEFAULT 100,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_biome_env_features_biome ON Biome_EnvironmentalFeatures(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_env_features_type ON Biome_EnvironmentalFeatures(feature_type);

-- Table: Biome_EnemySpawns (structure only, content in v0.29.3)
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
);

CREATE INDEX IF NOT EXISTS idx_biome_enemy_spawns_biome ON Biome_EnemySpawns(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_enemy_spawns_type ON Biome_EnemySpawns(enemy_type);

-- Table: Biome_ResourceDrops
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

-- Table: Characters_BiomeStatus
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
);

CREATE INDEX IF NOT EXISTS idx_biome_status_character ON Characters_BiomeStatus(character_id);
CREATE INDEX IF NOT EXISTS idx_biome_status_biome ON Characters_BiomeStatus(biome_id);

-- =====================================================
-- DATA SEEDING: BIOMES
-- =====================================================

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
    4,
    'Muspelheim',
    'Catastrophic geothermal failure zone where containment systems have collapsed and thermal regulators have liquefied. Industrial forges and magma-tap stations vent raw heat into the ruins.',
    '[Roots]',
    1004,
    7,
    12,
    1
);

-- =====================================================
-- DATA SEEDING: ROOM TEMPLATES
-- =====================================================

-- Template 1: Geothermal Control Chamber
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Geothermal Control Chamber',
    'Octagonal command center with defunct thermal monitoring stations and shattered observation glass. Central control console radiates heat. Multiple exits lead to auxiliary systems.',
    'Large', 3, 5, 1, 0, 1, 'Medium', 120, 0.5,
    '{"allow": ["Lava Flow Corridor", "Equipment Bay", "Heat Exchanger Platform"], "forbid": ["Containment Breach Zone"]}'
);

-- Template 2: Lava Flow Corridor
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Lava Flow Corridor',
    'Narrow passage bisected by molten slag river. Catwalks provide precarious crossing points. Heat shimmer distorts vision at range.',
    'Small', 2, 2, 0, 0, 0, 'High', 80, 0.1,
    '{"allow": ["Geothermal Control Chamber", "Collapsed Forge Floor", "Emergency Coolant Junction"], "forbid": ["Containment Breach Zone", "Molten Slag Repository"]}'
);

-- Template 3: Collapsed Forge Floor
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Collapsed Forge Floor',
    'Multi-tiered industrial platform partially collapsed into molten pit below. Structurally unstable catwalks and exposed rebar create vertical combat zones.',
    'Medium', 2, 3, 0, 0, 0, 'Extreme', 150, 0.2,
    '{"allow": ["Lava Flow Corridor", "Heat Exchanger Platform"], "forbid": ["Containment Breach Zone"]}'
);

-- Template 4: Scorched Equipment Bay
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Scorched Equipment Bay',
    'Industrial storage chamber filled with heat-warped machinery and blackened supply crates. Residual thermal energy makes salvage dangerous but rewarding.',
    'Medium', 1, 3, 0, 0, 0, 'Low', 60, 0.8,
    '{"allow": ["Geothermal Control Chamber", "Emergency Coolant Junction"], "forbid": ["Molten Slag Repository", "Containment Breach Zone"]}'
);

-- Template 5: Molten Slag Repository
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Molten Slag Repository',
    'Waste containment chamber overflowing with liquefied industrial byproducts. Islands of stable ground surrounded by glowing slag. Legendary materials solidify in cooler pockets.',
    'Large', 1, 2, 0, 0, 0, 'Extreme', 40, 0.9,
    '{"allow": ["Heat Exchanger Platform"], "forbid": ["Lava Flow Corridor", "Scorched Equipment Bay", "Geothermal Control Chamber"]}'
);

-- Template 6: Heat Exchanger Platform
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Heat Exchanger Platform',
    'Massive vertical chamber with colossal heat exchange pipes venting superheated steam. Multi-level catwalks provide tactical positioning. Pressure release valves create dynamic hazards.',
    'XLarge', 2, 4, 0, 0, 0, 'High', 110, 0.4,
    '{"allow": ["Geothermal Control Chamber", "Collapsed Forge Floor", "Molten Slag Repository"], "forbid": ["Containment Breach Zone"]}'
);

-- Template 7: Containment Breach Zone
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Containment Breach Zone',
    'Catastrophic failure site where primary containment vessel has ruptured. Radiation of extreme heat, molten metal geysers, and structural instability. The heart of the meltdown.',
    'XLarge', 1, 1, 0, 0, 0, 'Extreme', 10, 0.0,
    '{"allow": [], "forbid": ["Geothermal Control Chamber", "Lava Flow Corridor", "Scorched Equipment Bay", "Molten Slag Repository", "Emergency Coolant Junction"]}'
);

-- Template 8: Emergency Coolant Junction
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
) VALUES (
    4, 'Emergency Coolant Junction',
    'Crossroads of defunct coolant pipelines. Residual coolant vapor provides brief respite from heat. Chokepoint for tactical defense or ambush.',
    'Small', 2, 4, 0, 1, 0, 'Low', 90, 0.3,
    '{"allow": ["Lava Flow Corridor", "Scorched Equipment Bay"], "forbid": ["Containment Breach Zone", "Molten Slag Repository"]}'
);

-- =====================================================
-- DATA SEEDING: RESOURCE DROPS
-- =====================================================

-- Tier 3 Resources
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Star-Metal Ore', 'Heat-forged metallic ore with unusual crystalline structure. Used in high-temperature weapon and armor crafting.', 3, 'Uncommon', 0.25, 1, 3, 0, 120);

INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Obsidian Shards', 'Volcanic glass fragments formed from rapidly cooled slag. Razor-sharp edges ideal for cutting tools and projectiles.', 3, 'Common', 0.35, 2, 5, 0, 150);

INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Hardened Servomotors', 'Heat-resistant mechanical components from defunct industrial systems. Useful for equipment maintenance and advanced crafting.', 3, 'Uncommon', 0.20, 1, 2, 1, 80);

-- Tier 4 Resources
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Heart of the Inferno', 'Runic catalyst supercharged by extreme heat exposure. Glows with inner fire. Highly sought for aetheric weaving.', 4, 'Rare', 0.08, 1, 1, 0, 40);

INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Molten Core Fragment', 'Superheated core sample from failed containment system. Radiates constant thermal energy. Handle with ablative gloves.', 4, 'Rare', 0.12, 1, 1, 1, 50);

INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Thermal Regulator Component', 'Intact component from pre-Glitch thermal management system. Rare find. Used in advanced environmental protection gear.', 4, 'Rare', 0.10, 1, 1, 1, 35);

-- Tier 5 Resources
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Surtur Engine Core', 'Legendary power core from Jotun-Forged warmachine. Pulsates with residual energy. Centerpiece for masterwork crafting.', 5, 'Legendary', 0.05, 1, 1, 0, 5);

INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Eternal Ember', 'Self-sustaining thermal anomaly contained in crystalline matrix. Never cools. Source unknown. Priceless to artificers.', 5, 'Legendary', 0.02, 1, 1, 1, 3);

INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (4, 'Ablative Plating Schematic', 'Intact technical document detailing pre-Glitch heat shielding technology. Enables crafting of superior Fire Resistance armor.', 5, 'Epic', 0.03, 1, 1, 1, 8);

COMMIT;

-- =====================================================
-- END v0.29.1 SEEDING SCRIPT
-- =====================================================
