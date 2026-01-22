-- =====================================================
-- v0.31.1: Alfheim Database Schema & Room Templates
-- =====================================================
-- Version: v0.31.1
-- Author: Rune & Rust Development Team
-- Date: 2025-11-16
-- Prerequisites: v0.30.1 (Niflheim database structure)
-- =====================================================
-- Document ID: RR-SPEC-v0.31.1-DATABASE
-- Parent Specification: v0.31 Alfheim Biome Implementation
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

-- Alfheim: The Aetheric Spires
-- biome_id: 6
-- Verticality: Canopy only (high-altitude energy facilities)
-- Level Range: 8-12 (Late Game, endgame positioning)
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
    6,
    'Alfheim',
    'The Aetheric Spires - catastrophic failure of Pre-Glitch reality manipulation research. High-altitude energy conduits and Aetheric laboratories leak raw, uncontrolled energy. Not natural lightning, but industrial-scale physics manipulation gone catastrophically wrong. Where containment fields have collapsed and Aetheric generators cycle out of control.',
    '[Canopy]',
    NULL,
    8,
    12,
    1
);

-- =====================================================
-- DATA SEEDING: ROOM TEMPLATES
-- =====================================================
-- 8 Room Templates: All Canopy (high-altitude exclusive)
-- =====================================================

-- =====================================================
-- CANOPY TEMPLATES (High-Altitude Aetheric Facilities)
-- =====================================================

-- Template 1: Aetheric Conduit Nexus (Large, Canopy, Hub)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    6, 'Aetheric Conduit Nexus',
    'A vast chamber where energy conduits converge. The air crackles with unstable Aetheric discharge, arcing between crystalline structures in chaotic patterns. Pre-Glitch containment fields flicker erratically, creating zones of warped physics. The constant hum of overloaded systems is deafening.',
    'Large', 3, 5, 1, 0, 1, 'High', 130, 0.7,
    '{"allow": ["Reality-Warping Chamber", "Energy Regulator Station", "Crystalline Garden"], "forbid": ["All-Rune Proving Ground"]}',
    'Canopy'
);

-- Template 2: Reality-Warping Chamber (Medium, Canopy)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    6, 'Reality-Warping Chamber',
    'An experimental laboratory where Pre-Glitch scientists attempted to manipulate spacetime fabric. The equipment here creates localized physics anomalies - gravity fluctuates randomly, time seems to stutter, and solid surfaces phase between states. The research logs are corrupted beyond recovery.',
    'Medium', 2, 3, 0, 0, 0, 'Extreme', 100, 0.6,
    '{"allow": ["Aetheric Conduit Nexus", "Holographic Archive", "Paradox Containment Field"], "forbid": []}',
    'Canopy'
);

-- Template 3: All-Rune Proving Ground (Large, Canopy, BOSS ROOM)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    6, 'All-Rune Proving Ground',
    'The site where researchers attempted to compile the paradoxical All-Rune. The center of the room contains a frozen moment - a crystallized Reality Glitch that pulses with impossible colors. The walls are scored with runic inscriptions that shift and change when observed. This is ground zero of the Great Silence.',
    'Large', 1, 2, 0, 0, 0, 'Extreme', 10, 0.95,
    '{"allow": ["Energy Regulator Station"], "forbid": ["Aetheric Conduit Nexus", "Crystalline Garden"]}',
    'Canopy'
);

-- Template 4: Holographic Archive (Medium, Canopy)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    6, 'Holographic Archive',
    'Pre-Glitch data storage facility using holographic projection technology. The projectors have malfunctioned, creating ghostly, translucent images that flicker through centuries of accumulated data. Some holograms have gained a disturbing semblance of agency, replaying the moments before the crash in endless loops.',
    'Medium', 2, 4, 1, 0, 1, 'Medium', 110, 0.75,
    '{"allow": ["Reality-Warping Chamber", "Unstable Platform Array", "Crystalline Garden"], "forbid": []}',
    'Canopy'
);

-- Template 5: Paradox Containment Field (Small, Canopy)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    6, 'Paradox Containment Field',
    'A sealed chamber designed to contain the aftermath of failed Aetheric experiments. The containment has partially breached, leaking causality violations into the surrounding space. Objects exist in multiple states simultaneously, and the distinction between past and present events is blurred.',
    'Small', 2, 2, 0, 1, 0, 'High', 80, 0.4,
    '{"allow": ["Reality-Warping Chamber", "Energy Regulator Station"], "forbid": ["All-Rune Proving Ground"]}',
    'Canopy'
);

-- Template 6: Unstable Platform Array (Medium, Canopy)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    6, 'Unstable Platform Array',
    'A series of floating platforms held aloft by malfunctioning anti-gravity generators. The platforms flicker between solid and incorporeal states at random intervals. Below is an endless drop - the original floor collapsed eight hundred years ago. Navigation requires careful timing and nerve.',
    'Medium', 1, 3, 0, 0, 0, 'High', 90, 0.5,
    '{"allow": ["Holographic Archive", "Crystalline Garden"], "forbid": ["All-Rune Proving Ground"]}',
    'Canopy'
);

-- Template 7: Crystalline Garden (Large, Canopy)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    6, 'Crystalline Garden',
    'An observation dome where Aetheric energy has crystallized into bizarre, tree-like formations. These crystal structures resonate with each other, creating haunting harmonics. The growth patterns suggest something between geology and biology - reality itself attempting to heal by growing around the wound.',
    'Large', 2, 4, 0, 0, 1, 'Medium', 100, 0.8,
    '{"allow": ["Aetheric Conduit Nexus", "Holographic Archive", "Unstable Platform Array"], "forbid": ["All-Rune Proving Ground"]}',
    'Canopy'
);

-- Template 8: Energy Regulator Station (Medium, Canopy)
INSERT OR IGNORE INTO Biome_RoomTemplates (
    biome_id, template_name, template_description, room_size_category,
    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules, z_level
) VALUES (
    6, 'Energy Regulator Station',
    'The central control room for this sector''s Aetheric power distribution. The regulators here are locked in catastrophic feedback loops, cycling between overload and collapse every few seconds. Touching any control surface risks severe electromagnetic exposure. The systems have been failing for centuries but will never fully shut down.',
    'Medium', 2, 3, 0, 0, 0, 'High', 80, 0.6,
    '{"allow": ["Aetheric Conduit Nexus", "Paradox Containment Field", "All-Rune Proving Ground"], "forbid": []}',
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

-- Resource 1: Aetheric Residue (Tier 2, Very Common)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (6, 'Aetheric Residue', 'Crystallized Aetheric energy from ruptured containment fields. Glows with unstable, multi-colored light. Essential component for Mystic ability enhancement and runic inscription. Still radiates detectable energy signatures after 800 years of discharge.', 2, 'Common', 0.35, 1, 3, 0, 150);

-- Resource 2: Energy Crystal Shard (Tier 2, Common)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (6, 'Energy Crystal Shard', 'Fragment of crystalline structures formed by solidified Aetheric energy. Naturally resonates at specific frequencies. Used in precision equipment crafting and energy shielding components. The crystal lattice structure exhibits properties between mineral and energy state.', 2, 'Common', 0.30, 1, 2, 0, 120);

-- Resource 3: Holographic Data Fragment (Tier 2, Common)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (6, 'Holographic Data Fragment', 'Partially corrupted holographic storage medium from Pre-Glitch archives. Contains fragmentary data about Aetheric research protocols. Valuable to Jotun-Readers and scholars. The projection technology is remarkably intact despite eight centuries of system failure.', 2, 'Common', 0.28, 2, 4, 0, 100);

-- =====================================================
-- TIER 3 (Uncommon)
-- =====================================================

-- Resource 4: Unstable Aether Sample (Tier 3, Uncommon)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (6, 'Unstable Aether Sample', 'Contained sample of raw Aetheric energy extracted from active conduits. Highly volatile and requires specialized containment. Used in experimental Mystic equipment and advanced runic work. The energy signature fluctuates unpredictably - handle with extreme caution.', 3, 'Uncommon', 0.22, 1, 2, 1, 80);

-- Resource 5: Paradox-Touched Component (Tier 3, Uncommon)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (6, 'Paradox-Touched Component', 'Mechanical component that has been exposed to causality violations and temporal anomalies. Exhibits unusual properties - exists in multiple quantum states simultaneously. Used in reality-manipulation equipment crafting. The physics around it feel fundamentally wrong.', 3, 'Uncommon', 0.20, 1, 1, 1, 70);

-- =====================================================
-- TIER 4 (Rare)
-- =====================================================

-- Resource 6: Pure Aether Shard (Tier 4, Rare, Runic Catalyst)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (6, 'Pure Aether Shard', 'Perfectly crystallized Aetheric energy with stable resonance patterns. Acts as runic catalyst for Energy-based effects and Mystic ability amplification. Found only in the most stable areas of Alfheim. The shard maintains perfect energy equilibrium indefinitely - a small miracle of Pre-Glitch engineering.', 4, 'Rare', 0.12, 1, 2, 1, 40);

-- Resource 7: Crystallized Aether (Tier 4, Rare)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (6, 'Crystallized Aether', 'Aetheric energy that has solidified into near-perfect crystal lattice structure. Contains immense stored energy in stable form. Essential for legendary Mystic equipment crafting. The crystallization process requires conditions that no longer exist - these are irreplaceable remnants.', 4, 'Rare', 0.10, 1, 2, 1, 30);

-- =====================================================
-- TIER 5 (Legendary)
-- =====================================================

-- Resource 8: Fragment of the All-Rune (Tier 5, Legendary, Boss Drop)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (6, 'Fragment of the All-Rune', 'A piece of the paradoxical rune that crashed reality. Contains crystallized fragments of the compiled paradox itself. Essential component for myth-forged Mystic equipment. Radiates impossible mathematics and contradictory energy signatures. Only obtainable from the All-Rune Proving Ground or All-Rune''s Echo.', 5, 'Legendary', 0.05, 1, 1, 1, 5);

-- Resource 9: Reality Anchor Core (Tier 5, Legendary)
INSERT OR IGNORE INTO Biome_ResourceDrops (biome_id, resource_name, resource_description, resource_tier, rarity, base_drop_chance, min_quantity, max_quantity, requires_special_node, weight)
VALUES (6, 'Reality Anchor Core', 'The stabilization core from a Pre-Glitch reality anchor device. One of the few remaining components that can impose coherence on chaotic Aetheric fields. Used in legendary equipment that protects against reality manipulation. The core maintains a bubble of stable physics around itself through unknown mechanisms.', 5, 'Legendary', 0.03, 1, 1, 1, 3);

COMMIT;

-- =====================================================
-- END v0.31.1 SEEDING SCRIPT
-- =====================================================

-- =====================================================
-- VERIFICATION QUERIES (Run these to verify seeding)
-- =====================================================

-- Test 1: Biome Entry Exists
-- SELECT COUNT(*) FROM Biomes WHERE biome_id = 6;
-- Expected: 1

-- Test 2: Room Template Count
-- SELECT COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 6;
-- Expected: 8 (all Canopy)

-- Test 3: Verticality Distribution
-- SELECT z_level, COUNT(*) FROM Biome_RoomTemplates WHERE biome_id = 6 GROUP BY z_level;
-- Expected: Canopy: 8

-- Test 4: Resource Drop Count
-- SELECT COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 6;
-- Expected: 9

-- Test 5: Quality Tier Distribution
-- SELECT resource_tier, COUNT(*) FROM Biome_ResourceDrops WHERE biome_id = 6 GROUP BY resource_tier;
-- Expected: Tier 2: 3, Tier 3: 2, Tier 4: 2, Tier 5: 2

-- Test 6: Drop Weight Validation
-- SELECT resource_name, weight FROM Biome_ResourceDrops WHERE biome_id = 6 ORDER BY weight DESC;
-- Expected: Aetheric Residue highest (150), legendaries lowest (5, 3)

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] Biomes table contains Alfheim entry (biome_id: 6)
-- [ ] 8 room templates seeded (all Canopy)
-- [ ] All templates have valid spawn weights
-- [ ] 9 resource types seeded across 4 tiers
-- [ ] Resource drop weights correctly calibrated
-- [ ] SQL migration script executes without errors
-- [ ] All foreign key constraints satisfied
-- [ ] All descriptions use v5.0 voice (technology, not magic)
-- [ ] Entity names are ASCII-compliant
-- [ ] Database integrity tests pass
-- =====================================================
