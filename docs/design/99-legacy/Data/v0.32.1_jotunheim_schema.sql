-- =====================================================
-- v0.32.1: Jötunheim Database Schema & Room Templates
-- =====================================================
-- Version: v0.32.1
-- Author: Rune & Rust Development Team
-- Date: 2025-11-16
-- Prerequisites: v0.31.1 (Alfheim database structure)
-- =====================================================
-- Document ID: RR-SPEC-v0.32.1-DATABASE
-- Parent Specification: v0.32 Jötunheim Biome Implementation
-- Status: Implementation Complete
-- Timeline: 10-14 hours
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

-- Jötunheim: The Assembly Yards
-- biome_id: 7
-- Verticality: Multiple (Trunk 70%, Roots 30%)
-- Level Range: 5-9 (Mid-Game positioning)
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
    7,
    'Jotunheim',
    'The Assembly Yards - colossal industrial manufacturing sector where Jötun-Forged terraforming units were built. Vast factory floors, silent assembly lines, and the titanic corpses of dead metal giants. 800 years of rust and decay. No ambient condition - threats are physical and technological.',
    '[Trunk/Roots]',
    NULL,
    5,
    9,
    1
);

-- =====================================================
-- DATA SEEDING: ROOM TEMPLATES
-- =====================================================
-- 10 Room Templates: 7 Trunk (70% weight), 3 Roots (30% weight)
-- =====================================================

-- =====================================================
-- TRUNK TEMPLATES (Factory Floor Level - 70% spawn weight)
-- =====================================================

-- Template 1: Primary Assembly Line (Large, Trunk, Hub)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    7, 'Primary Assembly Line',
    'A colossal, silent conveyor belt stretches into the darkness. Robotic arms hang motionless above the line, frozen mid-assembly. The floor is littered with half-finished components and 800 years of accumulated rust. The air smells of oil and ozone. This is where giants were born.',
    'Large', 3, 5, 1, 0, 1, 'High', 150, 0.6,
    '{"allow": ["Jotun Umbilical Gantry", "Scrap Compactor Sector", "Shipping Container Maze"], "forbid": ["Fallen Einherjar Torso-Cave"]}',
    'Trunk'
);

-- Template 2: Jotun Umbilical Gantry (Medium, Trunk)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    7, 'Jotun Umbilical Gantry',
    'The massive power and data cables that once fed a Jötun-Forged during construction hang like dead vines. The gantry structure creates extreme verticality - you can see dozens of meters up and down through the lattice of rusted steel. Some cables still hum with residual power.',
    'Medium', 2, 4, 0, 0, 1, 'High', 120, 0.5,
    '{"allow": ["Primary Assembly Line", "Coolant Reservoir", "Command Deck Wreckage"], "forbid": []}',
    'Trunk'
);

-- Template 3: Scrap Compactor Sector (Large, Trunk)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    7, 'Scrap Compactor Sector',
    'Mountains of crushed and compacted metal dominate this sector. The compactor itself - a hydraulic press the size of a building - stands dormant. The ground is treacherous, shifting scrap that crunches and groans underfoot. Valuable components are mixed with worthless debris.',
    'Large', 2, 4, 0, 0, 0, 'Medium', 100, 0.75,
    '{"allow": ["Primary Assembly Line", "Shipping Container Maze"], "forbid": ["Fallen Einherjar Torso-Cave"]}',
    'Trunk'
);

-- Template 4: Coolant Reservoir (Medium, Trunk)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    7, 'Coolant Reservoir',
    'A vast pool of stagnant, oily coolant fluid. The surface is covered with a rainbow-sheened film. Rusted catwalks provide precarious passage above the reservoir. The fluid is conductive - any electrical discharge here would be catastrophic. The smell is acrid and chemical.',
    'Medium', 2, 3, 0, 0, 0, 'Extreme', 110, 0.55,
    '{"allow": ["Jotun Umbilical Gantry", "Shipping Container Maze"], "forbid": ["Power Distribution Core"]}',
    'Trunk'
);

-- Template 5: Fallen Einherjar Torso-Cave (Large, Trunk, SPECIAL)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    7, 'Fallen Einherjar Torso-Cave',
    'You stand inside the hollow chest cavity of a dormant Jötun-Forged. Its outer hull forms the ceiling and walls of this chamber. Massive cables and servo-mechanisms create a maze of obstacles. The low-level psychic broadcast from its corrupted logic core creates a constant, oppressive hum.',
    'Large', 1, 2, 0, 0, 0, 'Extreme', 80, 0.85,
    '{"allow": ["Shipping Container Maze", "Command Deck Wreckage"], "forbid": ["Primary Assembly Line", "Scrap Compactor Sector"]}',
    'Trunk'
);

-- Template 6: Command Deck Wreckage (Medium, Trunk)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    7, 'Command Deck Wreckage',
    'The elevated control station where Pre-Glitch engineers oversaw the assembly process. Most of the floor has collapsed, leaving only narrow catwalks and precarious platforms. Holographic displays flicker with corrupted data. The view from here shows the staggering scale of the facility.',
    'Medium', 2, 3, 1, 0, 0, 'High', 90, 0.65,
    '{"allow": ["Jotun Umbilical Gantry", "Fallen Einherjar Torso-Cave"], "forbid": []}',
    'Trunk'
);

-- Template 7: Shipping Container Maze (Medium, Trunk)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    7, 'Shipping Container Maze',
    'Thousands of standardized cargo containers are stacked in chaotic piles. The original organization has long since collapsed into a labyrinth of rusted metal corridors. Some containers have burst open, spilling their contents. Others are sealed tight - their contents a mystery.',
    'Medium', 2, 4, 0, 1, 0, 'Medium', 130, 0.70,
    '{"allow": ["Primary Assembly Line", "Scrap Compactor Sector", "Coolant Reservoir", "Fallen Einherjar Torso-Cave"], "forbid": []}',
    'Trunk'
);

-- =====================================================
-- ROOTS TEMPLATES (Maintenance/Utility Level - 30% spawn weight)
-- =====================================================

-- Template 8: Maintenance Tunnel Network (Small, Roots)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    7, 'Maintenance Tunnel Network',
    'Cramped, claustrophobic passages beneath the factory floor. These tunnels once allowed maintenance crews to access the facility infrastructure. Now they are flooded with ankle-deep coolant and echoing with the sounds of failing machinery. Rusted Servitors still patrol here.',
    'Small', 2, 3, 0, 0, 0, 'High', 60, 0.45,
    '{"allow": ["Power Distribution Core", "Waste Reclamation Chamber"], "forbid": []}',
    'Roots'
);

-- Template 9: Power Distribution Core (Medium, Roots, HIGH DANGER)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    7, 'Power Distribution Core',
    'Deep below the factory, the primary power grid still functions. Massive transformers and conduits hum with dangerous energy. The walls are lined with warning signs in Pre-Glitch script. This is the source of the facility live power conduits - and the most dangerous sector.',
    'Medium', 1, 2, 0, 0, 0, 'Extreme', 50, 0.90,
    '{"allow": ["Maintenance Tunnel Network"], "forbid": ["Coolant Reservoir"]}',
    'Roots'
);

-- Template 10: Waste Reclamation Chamber (Medium, Roots)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    7, 'Waste Reclamation Chamber',
    'The lowest level, where industrial waste was processed and recycled. The chamber is filled with toxic sludge and piles of corroded metal. The air is thick with chemical fumes. Few come here willingly, but the waste contains rare components that were deemed too damaged to use.',
    'Medium', 1, 2, 0, 1, 0, 'High', 40, 0.80,
    '{"allow": ["Maintenance Tunnel Network"], "forbid": []}',
    'Roots'
);

-- =====================================================
-- DATA SEEDING: RESOURCE DROPS
-- =====================================================
-- 10 Resource Types across Tiers 1-4
-- =====================================================

-- =====================================================
-- TIER 1 (Common - Basic Components)
-- =====================================================

-- Resource 1: Rusted Scrap Metal (Tier 1, Very Common)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (7, 'Rusted Scrap Metal', 'Heavily corroded metal fragments from the assembly yard. Low quality but abundant. Essential for basic repairs and Tier 1 crafting. The rust can be sanded off with effort. Salvaged from shipping containers, structural debris, and fallen machinery.', 1, 'Common', 0.40, 2, 5, 0, 200);

-- Resource 2: Ball Bearings (Tier 1, Common)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (7, 'Ball Bearings', 'Precision-manufactured ball bearings from assembly line equipment. Surprisingly well-preserved despite the rust. Used in mechanical crafting and as a common reagent for movement-based equipment. Can also be scattered as improvised caltrops.', 1, 'Common', 0.35, 10, 30, 0, 140);

-- Resource 3: Coolant Fluid (Tier 1, Common - Specialized)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (7, 'Coolant Fluid', 'Industrial coolant extracted from reservoir tanks. Conductive and mildly toxic. Used in alchemical brewing and as a reagent for electrical resistance treatments. Handle with care - prolonged exposure causes chemical burns.', 1, 'Common', 0.32, 1, 3, 0, 130);

-- =====================================================
-- TIER 2 (Uncommon - Functional Components)
-- =====================================================

-- Resource 4: Intact Servomotor (Tier 2, Uncommon)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (7, 'Intact Servomotor', 'Functional motor assembly salvaged from dormant machinery. Still operates despite 800 years of disuse - testament to Pre-Glitch engineering. Used in Tier 2 mechanical crafting and equipment repairs. Tinkers prize these components.', 2, 'Uncommon', 0.28, 1, 2, 0, 150);

-- Resource 5: Hydraulic Cylinder (Tier 2, Uncommon)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (7, 'Hydraulic Cylinder', 'Heavy-duty hydraulic component from industrial machinery. Contains pressurized fluid - exercise caution when disassembling. Used in crafting high-force mechanical equipment and siege weapons. The pressure system is remarkably intact.', 2, 'Uncommon', 0.24, 1, 2, 0, 90);

-- Resource 6: Power Relay Circuit (Tier 2, Uncommon - Specialized)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (7, 'Power Relay Circuit', 'Electronic circuit board from power distribution systems. Still functional - the solid-state components have survived the centuries. Essential for electrical equipment crafting and power management systems. Tinkers can repurpose these for various applications.', 2, 'Uncommon', 0.22, 1, 2, 1, 80);

-- =====================================================
-- TIER 3 (Rare - Advanced Components)
-- =====================================================

-- Resource 7: Unblemished Jotun Plating (Tier 3, Rare)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (7, 'Unblemished Jotun Plating', 'Pristine hull plating salvaged from intact sections of a dormant Jötun-Forged. Lightweight yet incredibly durable - the alloy composition is a lost art. Used in high-tier armor crafting and legendary equipment. Each piece must be carefully extracted without damaging the giant.', 3, 'Rare', 0.15, 1, 2, 1, 30);

-- Resource 8: Industrial Servo Actuator (Tier 3, Rare)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (7, 'Industrial Servo Actuator', 'High-precision actuator from assembly line robotics. Capable of micro-adjustments at massive scale. Used in advanced mechanical crafting and prosthetic equipment. The control algorithms are partially corrupted but still salvageable.', 3, 'Rare', 0.12, 1, 1, 1, 40);

-- =====================================================
-- TIER 4 (Legendary - Myth-Forged Components)
-- =====================================================

-- Resource 9: Uncorrupted Power Coil (Tier 4, Legendary)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (7, 'Uncorrupted Power Coil', 'A perfectly preserved power coil from the deepest power distribution cores. Contains a stable, high-density energy field. Essential component for myth-forged electrical weapons and legendary armor. Only found in the most dangerous sectors - the power stations that still function.', 4, 'Legendary', 0.05, 1, 1, 1, 5);

-- Resource 10: Jotun Logic Core Fragment (Tier 4, Legendary)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (7, 'Jotun Logic Core Fragment', 'A shard of a Jötun-Forged central processing unit. Contains fragments of corrupted but immensely powerful computational logic. Used in legendary equipment crafting and advanced runic inscription. The psychic broadcast from this fragment is palpable - handle with caution.', 4, 'Legendary', 0.03, 1, 1, 1, 3);

COMMIT;

-- =====================================================
-- END v0.32.1 SEEDING SCRIPT
-- =====================================================

-- =====================================================
-- VERIFICATION QUERIES (Run these to verify seeding)
-- =====================================================

-- Test 1: Biome Entry Exists
-- SELECT COUNT(*) FROM Biomes WHERE biome_id = 7;
-- Expected: 1

-- Test 2: Room Template Count
-- SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 7;
-- Expected: 10

-- Test 3: Verticality Distribution
-- SELECT z_level, COUNT(*), SUM(enemy_spawn_weight) FROM Biome_RoomTemplates WHERE biome_id = 7 GROUP BY z_level;
-- Expected: Trunk: 7 templates (~70% weight: 780), Roots: 3 templates (~30% weight: 150)

-- Test 4: Resource Drop Count
-- SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 7;
-- Expected: 10

-- Test 5: Quality Tier Distribution
-- SELECT resource_tier, COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 7 GROUP BY resource_tier;
-- Expected: Tier 1: 3, Tier 2: 3, Tier 3: 2, Tier 4: 2

-- Test 6: Drop Weight Validation
-- SELECT resource_name, weight FROM Biome_ResourceDrops WHERE biome_id = 7 ORDER BY weight DESC;
-- Expected: Rusted Scrap Metal highest (200), Jotun Logic Core Fragment lowest (3)

-- Test 7: Trunk/Roots Spawn Weight Split
-- SELECT
--   z_level,
--   SUM(enemy_spawn_weight) as total_weight,
--   ROUND(100.0 * SUM(enemy_spawn_weight) / (SELECT SUM(enemy_spawn_weight) FROM Biome_RoomTemplates WHERE biome_id = 7), 1) as percentage
-- FROM Biome_RoomTemplates
-- WHERE biome_id = 7
-- GROUP BY z_level;
-- Expected: Trunk: ~83.9% (780/930), Roots: ~16.1% (150/930)
-- NOTE: This is weighted by enemy_spawn_weight, which affects actual room frequency

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] Biomes table contains Jötunheim entry (biome_id: 7)
-- [ ] 10 room templates seeded (7 Trunk, 3 Roots)
-- [ ] All templates have valid spawn weights
-- [ ] Spawn weight distribution creates appropriate Trunk/Roots split
-- [ ] 10 resource types seeded across 4 tiers
-- [ ] Resource drop weights correctly calibrated
-- [ ] SQL migration script executes without errors
-- [ ] All foreign key constraints satisfied
-- [ ] All descriptions use v5.0 voice (industrial, not mythological)
-- [ ] Entity names are ASCII-compliant internally
-- [ ] Database integrity tests pass
-- =====================================================
