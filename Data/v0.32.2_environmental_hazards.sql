-- =====================================================
-- v0.32.2: Jötunheim Environmental Hazards & Industrial Terrain
-- =====================================================
-- Version: v0.32.2
-- Author: Rune & Rust Development Team
-- Date: 2025-11-16
-- Prerequisites: v0.32.1 (Jötunheim Database Schema)
-- =====================================================
-- Document ID: RR-SPEC-v0.32.2-ENVIRONMENTAL
-- Parent Specification: v0.32 Jötunheim Biome Implementation
-- Status: Implementation Complete
-- Timeline: 10-14 hours
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- SECTION 1: ENVIRONMENTAL FEATURES TABLE (IF NOT EXISTS)
-- =====================================================

-- Table: Biome_EnvironmentalFeatures (should exist from previous biomes)
CREATE TABLE IF NOT EXISTS Biome_EnvironmentalFeatures (
    feature_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    feature_name TEXT NOT NULL,
    feature_type TEXT CHECK(feature_type IN (
        'Hazard', 'Terrain', 'Ambient', 'Dynamic_Hazard',
        'Interactive_Hazard', 'Cover', 'Ambient_Effect',
        'Triggered_Hazard', 'Interactive_Terrain', 'Special_Terrain', 'Elevation'
    )),
    feature_description TEXT,
    damage_per_turn INTEGER DEFAULT 0,
    damage_type TEXT,
    tile_coverage_percent REAL DEFAULT 0.0,
    is_destructible INTEGER DEFAULT 0,
    blocks_movement INTEGER DEFAULT 0,
    blocks_line_of_sight INTEGER DEFAULT 0,
    hazard_density_category TEXT CHECK(hazard_density_category IN ('None', 'Low', 'Medium', 'High', 'Extreme')),
    special_rules TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_biome_env_features_biome ON Biome_EnvironmentalFeatures(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_env_features_type ON Biome_EnvironmentalFeatures(feature_type);
CREATE INDEX IF NOT EXISTS idx_biome_env_features_density ON Biome_EnvironmentalFeatures(hazard_density_category);

-- =====================================================
-- SECTION 2: NO AMBIENT CONDITION
-- =====================================================

-- IMPORTANT: Jötunheim has NO ambient condition.
-- This is intentional and canonical from v2.0.
-- Threats are physical and technological, not metaphysical.
-- The ambient_condition_id field remains NULL for biome_id = 7.

-- Verify no ambient condition is set
UPDATE Biomes
SET ambient_condition_id = NULL
WHERE biome_id = 7;

-- =====================================================
-- SECTION 3: SIGNATURE HAZARD - [Live Power Conduit]
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    7,
    'Live Power Conduit',
    'Dynamic_Hazard',
    'Failing power grid cable arcing with electrical current. Deals 1d8 Energy damage per turn to adjacent characters. AMPLIFIED IN FLOODED TERRAIN: 2d10 damage in 2-tile radius. Destructible (15 HP). Represents 800 years of failing infrastructure.',
    0, -- Damage handled by special_rules
    'Energy',
    25.0, -- Very high coverage (signature hazard)
    1, -- Destructible
    0, -- Does not block movement
    0, -- Does not block LoS
    'High',
    '{"damage_dice": "1d8", "flooded_damage_dice": "2d10", "flooded_radius": 2, "hp": 15, "double_damage_on_forced_movement": true, "warning": "The failing power grid arcs with deadly electrical current"}'
);

-- =====================================================
-- HAZARD 2: [High-Pressure Steam Vent]
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    7,
    'High-Pressure Steam Vent',
    'Dynamic_Hazard',
    'Faulty industrial piping. Erupts every 3 turns in 3-tile cone. Deals 2d6 Fire damage and knocks targets back 1 tile. Hisses loudly 1 turn before eruption (predictable). Represents failing coolant system pressure management.',
    0,
    'Fire',
    15.0, -- Medium-high coverage
    0, -- Not destructible (structural)
    0,
    0,
    'High',
    '{"damage_dice": "2d6", "eruption_cycle": 3, "cone_length": 3, "knockback_distance": 1, "warning_turn": true, "warning_message": "The steam vent hisses - pressure is building"}'
);

-- =====================================================
-- HAZARD 3: [Unstable Ceiling/Wall]
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    7,
    'Unstable Ceiling/Wall',
    'Triggered_Hazard',
    'Structurally compromised section. Collapses when heavy impact (10+ damage) occurs nearby. Deals 3d8 Physical damage in 2x2 area. Creates [Debris Pile] terrain (difficult terrain + cover). One-time hazard. 800 years of decay finally catching up.',
    0,
    'Physical',
    13.0,
    0, -- Not destructible (already unstable)
    0,
    0,
    'Medium',
    '{"damage_dice": "3d8", "trigger_damage_threshold": 10, "area_size": "2x2", "creates_terrain": "Debris Pile", "one_time_only": true, "warning": "The ceiling groans ominously"}'
);

-- =====================================================
-- TERRAIN 1: [Flooded (Coolant)]
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    7,
    'Flooded (Coolant)',
    'Terrain',
    'Ankle-deep coolant fluid. Movement costs +1 Stamina (wading). Conductive - amplifies Live Power Conduit damage (1d8 → 2d10, radius 1 → 2). Characters ending turn in fluid take 1 Poison damage. Rainbow-sheened surface obscures submerged hazards.',
    1, -- 1 Poison per turn
    'Poison',
    18.0, -- High in Coolant Reservoir rooms
    0,
    0, -- Does not block movement (slows it)
    0,
    'Medium',
    '{"stamina_cost": 1, "amplifies_power_conduit": true, "poison_damage": 1, "obscures_vision": "submerged hazards", "flavor": "The fluid is conductive and mildly toxic"}'
);

-- =====================================================
-- TERRAIN 2: [Cover (Industrial)]
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    7,
    'Cover (Industrial)',
    'Cover',
    'Industrial debris provides excellent cover. Shipping containers, engine blocks, and structural components litter the factory floor. Provides +3 Defense. Destructible (25-40 HP depending on size). Some containers can be opened for salvage.',
    0,
    NULL,
    25.0, -- Very high (industrial debris everywhere)
    1, -- Destructible
    1, -- Blocks movement (solid objects)
    1, -- Blocks LoS
    'Low',
    '{"defense_bonus": 3, "hp_min": 25, "hp_max": 40, "can_be_salvaged": true, "types": ["Shipping Container", "Engine Block", "Structural Debris"]}'
);

-- =====================================================
-- TERRAIN 3: [Jötun Corpse Terrain]
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    7,
    'Jotun Corpse Terrain',
    'Special_Terrain',
    'Terrain formed by dormant Jötun-Forged chassis. Hull sections = platforms, limbs = bridges, interior = cave. Creates extreme verticality. Characters on corpse terrain gain +2 Psychic Stress per turn (corrupted logic core broadcast). God-Sleeper Cultists gain power here.',
    2, -- Psychic Stress, not damage
    'Psychic',
    8.0, -- Rare but iconic
    0,
    0, -- Does not block (creates traversable terrain)
    0,
    'High',
    '{"psychic_stress_per_turn": 2, "creates_verticality": true, "cultist_amplification": true, "elements": ["hull_platform", "limb_bridge", "interior_cave"], "flavor": "The low-level psychic broadcast creates constant oppressive hum"}'
);

-- =====================================================
-- HAZARD 4: [Assembly Line (Active)]
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    7,
    'Assembly Line (Active)',
    'Dynamic_Hazard',
    'Still-functional conveyor belt. Characters standing on belt are moved 2 tiles per turn in belt direction. Can push targets into hazards or off platforms. Shutting down requires Engineering check (DC 15). The automation protocols never stopped - even after 800 years.',
    0,
    NULL,
    10.0,
    0, -- Cannot be destroyed (structural)
    0, -- Does not block (moves characters instead)
    0,
    'Medium',
    '{"forced_movement_per_turn": 2, "shutdown_dc": 15, "shutdown_skill": "Engineering", "can_push_into_hazards": true, "warning": "The conveyor belt is still active"}'
);

-- =====================================================
-- TERRAIN 4: [Scrap Heap]
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    7,
    'Scrap Heap',
    'Interactive_Terrain',
    'Mountain of compacted scrap metal. Difficult terrain (+1 Stamina movement). Salvageable: 1 action to search, roll 1d20 vs. DC 12 for Tier 1-3 component. Unstable: Movement alerts enemies. Valuable components mixed with worthless debris.',
    0,
    NULL,
    16.0, -- Common
    0,
    0, -- Does not block (difficult terrain)
    0,
    'Low',
    '{"stamina_cost": 1, "salvage_action": 1, "salvage_dc": 12, "salvage_tier_min": 1, "salvage_tier_max": 3, "alerts_enemies": true, "flavor": "The scrap crunches and groans underfoot"}'
);

-- =====================================================
-- HAZARD 5: [Toxic Haze]
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    7,
    'Toxic Haze',
    'Ambient_Effect',
    'Chemical vapor from leaking coolant. Obscures vision in 4x4 zone - ranged attacks at Disadvantage. Deals 1d4 Poison damage per turn. Dissipates slowly (1 tile/turn from edges). Prolonged exposure causes respiratory damage. Found near coolant reservoirs.',
    4, -- 1d4 average Poison
    'Poison',
    7.0, -- Uncommon, coolant sector specific
    0,
    0,
    1, -- Obscures vision (partial LoS block)
    'Medium',
    '{"damage_dice": "1d4", "zone_size": "4x4", "ranged_penalty": "Disadvantage", "dissipation_rate": 1, "flavor": "The air is thick with acrid chemical fumes"}'
);

-- =====================================================
-- TERRAIN 5: [Gantry Platform]
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    7,
    'Gantry Platform',
    'Elevation',
    'Elevated maintenance platform. Accessible via ladder (costs 2 Stamina to climb). Provides height advantage: +1 to ranged attacks from platform. Gantry-Runner specialization climbs for free. Creates tactical verticality in factory floor combat.',
    0,
    NULL,
    14.0,
    0,
    0, -- Does not block (elevated)
    0,
    'Low',
    '{"climb_stamina_cost": 2, "ranged_attack_bonus": 1, "gantry_runner_free_climb": true, "creates_verticality": true, "flavor": "Rusted catwalks overlook the factory floor"}'
);

-- =====================================================
-- TERRAIN 6: [Debris Pile] (Created by Ceiling Collapse)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    7,
    'Debris Pile',
    'Terrain',
    'Rubble from structural collapse. Difficult terrain (+1 Stamina). Provides half cover (+2 Defense). Created dynamically by [Unstable Ceiling/Wall] collapse. Represents the final failure of 800-year-old construction.',
    0,
    NULL,
    0.0, -- Dynamically created (not spawned)
    0,
    0,
    0, -- Partial LoS block (half cover)
    'None',
    '{"stamina_cost": 1, "defense_bonus": 2, "created_by": "Unstable Ceiling/Wall", "dynamic_terrain": true}'
);

COMMIT;

-- =====================================================
-- END v0.32.2 SEEDING SCRIPT
-- =====================================================

-- =====================================================
-- VERIFICATION QUERIES (Run these to verify seeding)
-- =====================================================

-- Test 1: Environmental Hazard Count
-- SELECT COUNT(*) FROM Biome_EnvironmentalFeatures WHERE biome_id = 7;
-- Expected: 11 (10 main + 1 debris pile)

-- Test 2: Verify Live Power Conduit (signature hazard)
-- SELECT * FROM Biome_EnvironmentalFeatures WHERE biome_id = 7 AND feature_name = 'Live Power Conduit';
-- Expected: 1 row with feature_type = 'Dynamic_Hazard'

-- Test 3: Verify No Ambient Condition
-- SELECT ambient_condition_id FROM Biomes WHERE biome_id = 7;
-- Expected: NULL

-- Test 4: Hazard Type Distribution
-- SELECT feature_type, COUNT(*) FROM Biome_EnvironmentalFeatures WHERE biome_id = 7 GROUP BY feature_type;
-- Expected: Mix of Dynamic_Hazard, Terrain, Special_Terrain, Cover, etc.

-- Test 5: Verify Coverage Percentages
-- SELECT feature_name, tile_coverage_percent FROM Biome_EnvironmentalFeatures WHERE biome_id = 7 ORDER BY tile_coverage_percent DESC;
-- Expected: Cover (Industrial) and Live Power Conduit highest coverage

-- Test 6: Verify Special Rules JSON
-- SELECT feature_name, special_rules FROM Biome_EnvironmentalFeatures WHERE biome_id = 7 AND feature_name = 'Live Power Conduit';
-- Expected: Valid JSON with flooded_damage_dice, flooded_radius, hp

-- Test 7: Verify Destructible Hazards
-- SELECT COUNT(*) FROM Biome_EnvironmentalFeatures WHERE biome_id = 7 AND is_destructible = 1;
-- Expected: 2 (Live Power Conduit, Cover)

-- Test 8: Verify Jötun Corpse Terrain (signature terrain)
-- SELECT * FROM Biome_EnvironmentalFeatures WHERE biome_id = 7 AND feature_name = 'Jotun Corpse Terrain';
-- Expected: 1 row with feature_type = 'Special_Terrain', damage_type = 'Psychic'

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] 11 environmental features inserted
-- [ ] [Live Power Conduit] signature hazard created
-- [ ] [High-Pressure Steam Vent] created
-- [ ] [Unstable Ceiling/Wall] created
-- [ ] [Flooded (Coolant)] terrain created
-- [ ] [Jötun Corpse Terrain] special terrain created
-- [ ] No ambient condition (NULL verified)
-- [ ] All special_rules contain valid JSON
-- [ ] Coverage percentages sum appropriately
-- [ ] All foreign key constraints satisfied
-- [ ] v5.0 voice compliance (industrial, not supernatural)
-- =====================================================
