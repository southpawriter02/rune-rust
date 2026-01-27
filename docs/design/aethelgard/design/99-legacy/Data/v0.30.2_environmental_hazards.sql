-- =====================================================
-- v0.30.2: Niflheim Environmental Hazards & Ambient Conditions
-- =====================================================
-- Version: v0.30.2
-- Author: Rune & Rust Development Team
-- Date: 2025-11-16
-- Prerequisites: v0.30.1 (Niflheim Database Schema)
-- =====================================================
-- Document ID: RR-SPEC-v0.30.2-ENVIRONMENTAL
-- Parent Specification: v0.30 Niflheim Biome Implementation
-- Status: Design Complete — Ready for Implementation
-- Timeline: 8-12 hours
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- SECTION 1: CONDITION SYSTEM TABLES
-- =====================================================

-- Table: Conditions (if not exists)
CREATE TABLE IF NOT EXISTS Conditions (
    condition_id INTEGER PRIMARY KEY,
    condition_name TEXT NOT NULL UNIQUE,
    condition_type TEXT CHECK(condition_type IN ('Buff', 'Debuff', 'Environmental', 'Status')),
    description TEXT,
    is_ambient INTEGER DEFAULT 0,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_conditions_name ON Conditions(condition_name);
CREATE INDEX IF NOT EXISTS idx_conditions_type ON Conditions(condition_type);

-- Table: Condition_Effects (if not exists)
CREATE TABLE IF NOT EXISTS Condition_Effects (
    effect_id INTEGER PRIMARY KEY AUTOINCREMENT,
    condition_id INTEGER NOT NULL,
    effect_type TEXT NOT NULL,
    effect_value INTEGER DEFAULT 0,
    effect_description TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (condition_id) REFERENCES Conditions(condition_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_condition_effects_condition ON Condition_Effects(condition_id);
CREATE INDEX IF NOT EXISTS idx_condition_effects_type ON Condition_Effects(effect_type);

-- =====================================================
-- SECTION 2: AMBIENT CONDITION - [Frigid Cold]
-- =====================================================

-- Condition Definition
INSERT OR IGNORE INTO Conditions (condition_id, condition_name, condition_type, description, is_ambient)
VALUES (
    105,
    'Frigid Cold',
    'Environmental',
    'Biome-wide cryogenic exposure. All combatants are Vulnerable to Ice damage (+50%). Critical hits inflict [Slowed] for 2 turns. The absolute zero temperature bypasses all standard thermal protection.',
    1
);

-- Effect 1: Universal Ice Vulnerability
INSERT OR IGNORE INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    105,
    'Damage_Vulnerability',
    50,
    'Ice vulnerability: All characters take +50% damage from Ice attacks.'
);

-- Effect 2: Critical Hit Slow
INSERT OR IGNORE INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    105,
    'On_Critical_Hit',
    0,
    'When critically hit, apply [Slowed] status for 2 turns.'
);

-- Effect 3: Stress Accumulation
INSERT OR IGNORE INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    105,
    'Psychic_Stress',
    5,
    'Environmental anxiety from extreme cold: +5 Psychic Stress per encounter.'
);

-- =====================================================
-- SECTION 3: DEBUFF CONDITION - [Brittle]
-- =====================================================

-- Condition Definition
INSERT OR IGNORE INTO Conditions (condition_id, condition_name, condition_type, description, is_ambient)
VALUES (
    106,
    'Brittle',
    'Debuff',
    'Target has been supercooled by Ice damage. Physical attacks exploit structural weakness. Vulnerable to Physical damage (+50%) for 1 turn.',
    0
);

-- Effect: Physical Vulnerability
INSERT OR IGNORE INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    106,
    'Damage_Vulnerability',
    50,
    'Physical vulnerability: Take +50% Physical damage.'
);

-- =====================================================
-- SECTION 4: UPDATE BIOMES TABLE
-- =====================================================

-- Link Niflheim to [Frigid Cold] ambient condition
UPDATE Biomes
SET ambient_condition_id = 105
WHERE biome_id = 5;

-- =====================================================
-- SECTION 5: ENVIRONMENTAL HAZARDS (8 Types)
-- =====================================================

-- Hazard 1: Slippery Terrain (DOMINANT FLOOR TYPE)
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    5, 'Slippery Terrain',
    'Terrain',
    'Ice sheet covering the floor. Moving requires FINESSE check (DC 12) or risk [Knocked Down] with 1d4 Physical damage. Forced movement effects gain +1 tile distance. Characters immune to [Knocked Down] ignore this terrain.',
    4, -- 1d4 average fall damage
    'Physical',
    70, -- DOMINANT coverage (60-80% of floors)
    0, -- Not destructible (terrain)
    0, -- Does not block movement (just risky)
    0, -- Does not block LoS
    'High',
    'finesse_dc_12,knockdown_on_fail,forced_movement_amplify,immunity_bypass'
);

-- Hazard 2: Unstable Ceiling (Icicles)
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    5, 'Unstable Ceiling (Icicles)',
    'Dynamic',
    'Massive icicles hanging from ceiling. Can be shattered by attacks or explosions. Deals 2d8 Ice damage to all in 2-tile radius. Tactical opportunity for area damage.',
    16, -- 2d8 average damage
    'Ice',
    12, -- Moderate coverage
    1, -- Destructible
    0, -- Does not block movement (ceiling-mounted)
    0, -- Does not block LoS
    'Medium',
    'aoe_radius_2,shatter_trigger,tactical_destruction'
);

-- Hazard 3: Frozen Machinery (Cover)
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    5, 'Frozen Machinery',
    'Cover',
    'Pre-Glitch equipment encased in ice. Provides excellent cover (+4 Defense). Blocks line of sight. Destructible (50 HP).',
    0, -- No damage (provides protection)
    NULL,
    18, -- Common coverage
    1, -- Destructible (50 HP)
    0, -- Does not block movement (can be used as cover)
    1, -- Blocks line of sight
    'Low',
    'cover_bonus_4,hp_50,blocks_los'
);

-- Hazard 4: Ice Boulders (Destructible Obstacles)
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    5, 'Ice Boulders',
    'Obstacle',
    'Large blocks of solid ice. Blocks movement and line of sight. Destructible (30 HP). When destroyed, creates [Slippery Terrain] in adjacent tiles.',
    0, -- No damage (obstacle)
    NULL,
    8, -- Minor coverage
    1, -- Destructible (30 HP)
    1, -- Blocks movement
    1, -- Blocks line of sight
    'Medium',
    'hp_30,creates_slippery_on_destroy'
);

-- Hazard 5: Cryo-Vent (Liquid Nitrogen Jets)
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    5, 'Cryo-Vent',
    'Dynamic',
    'Ruptured coolant line sprays liquid nitrogen jets. Active 1 turn, inactive 2 turns. Deals 3d6 Ice damage to anyone entering or starting turn in spray area. Timing pattern is predictable.',
    18, -- 3d6 average damage
    'Ice',
    6, -- Minor coverage
    0, -- Not destructible (continuous leak)
    0, -- Does not block movement (just dangerous)
    0, -- Does not block LoS
    'High',
    'periodic_activation,1_on_2_off,predictable_pattern'
);

-- Hazard 6: Brittle Ice Bridge (Conditional Chasm)
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    5, 'Brittle Ice Bridge',
    'Conditional_Obstacle',
    'Thin ice spanning a chasm. Crossing requires STURDINESS check (DC 10). Weight limit: 2 combatants. Failure = fall into chasm (instant death or 10d10 damage if bottom visible). Success = bridge holds.',
    999, -- Instant death or massive damage on fall
    'Physical',
    4, -- Rare coverage
    0, -- Not destructible (would cause fall)
    0, -- Does not block movement (conditional)
    0, -- Does not block LoS
    'Extreme',
    'sturdiness_dc_10,weight_limit_2,fall_on_fail'
);

-- Hazard 7: Frozen Corpse (Storytelling Feature)
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    5, 'Frozen Corpse',
    'Interactive_Object',
    'Pre-Glitch victim perfectly preserved in ice. No mechanical threat. Can be searched (WITS DC 15) for resources. Environmental storytelling: frozen at moment of death, expressions of panic preserved.',
    0, -- No damage (storytelling)
    NULL,
    10, -- Moderate coverage (common storytelling element)
    0, -- Not destructible
    0, -- Does not block movement
    0, -- Does not block LoS
    'None',
    'wits_dc_15,resource_search,storytelling'
);

-- Hazard 8: Cryogenic Fog (Visibility Reduction)
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    5, 'Cryogenic Fog',
    'Vision',
    'Dense fog from sublimating ice. Reduces visibility to 3 tiles. Ranged attacks beyond 3 tiles have Disadvantage. Melee and adjacent attacks unaffected.',
    0, -- No damage (visibility only)
    NULL,
    20, -- Moderate coverage
    0, -- Not destructible (ambient)
    0, -- Does not block movement
    0, -- Does not block LoS (reduces visibility instead)
    'Low',
    'visibility_3_tiles,ranged_disadvantage'
);

-- Hazard 9: Flash-Frozen Terminal (Interactive Object)
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    5, 'Flash-Frozen Terminal',
    'Interactive_Object',
    'Pre-Glitch data terminal frozen solid. Must be thawed with Fire damage (10+). Once thawed, WITS check (DC 14) to extract data. Success = gain Cryogenic Data-Slate resource (Tier 4).',
    0, -- No damage (interactive)
    NULL,
    5, -- Rare coverage
    0, -- Not destructible (interactive instead)
    0, -- Does not block movement
    0, -- Does not block LoS
    'None',
    'fire_thaw_10,wits_dc_14,tier_4_resource_drop'
);

COMMIT;

-- =====================================================
-- VALIDATION QUERIES
-- =====================================================

-- Test 1: Verify [Frigid Cold] condition exists
SELECT * FROM Conditions WHERE condition_id = 105;
-- Expected: 1 row, condition_name = 'Frigid Cold'

-- Test 2: Verify [Frigid Cold] effects count
SELECT COUNT(*) as effect_count
FROM Condition_Effects
WHERE condition_id = 105;
-- Expected: 3 (Ice Vulnerability, Critical Hit Slow, Stress)

-- Test 3: Verify [Brittle] condition exists
SELECT * FROM Conditions WHERE condition_id = 106;
-- Expected: 1 row, condition_name = 'Brittle'

-- Test 4: Verify Niflheim ambient condition linked
SELECT biome_name, ambient_condition_id
FROM Biomes
WHERE biome_id = 5;
-- Expected: Niflheim, 105

-- Test 5: Verify all 9 hazards inserted
SELECT COUNT(*) as hazard_count
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 5;
-- Expected: 9

-- Test 6: View all Niflheim hazards with details
SELECT
    feature_name,
    feature_type,
    hazard_density_category,
    damage_per_turn,
    damage_type,
    tile_coverage_percent
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 5
ORDER BY tile_coverage_percent DESC;

-- Test 7: Check hazard type distribution
SELECT
    feature_type,
    COUNT(*) as count
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 5
GROUP BY feature_type;

-- Test 8: Verify total tile coverage (should be < 200% for overlap tolerance)
SELECT SUM(tile_coverage_percent) as total_coverage
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 5;
-- Expected: ~153% (overlapping hazards acceptable)

-- Test 9: Verify dominant terrain (Slippery Terrain)
SELECT feature_name, tile_coverage_percent
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 5
AND feature_type = 'Terrain'
ORDER BY tile_coverage_percent DESC;
-- Expected: Slippery Terrain at 70%

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] [Frigid Cold] condition created (condition_id: 105)
-- [ ] [Frigid Cold] has 3 effects (Ice Vuln, Crit Slow, Stress)
-- [ ] [Brittle] condition created (condition_id: 106)
-- [ ] Niflheim linked to [Frigid Cold] (ambient_condition_id = 105)
-- [ ] 9 environmental hazards seeded
-- [ ] [Slippery Terrain] is dominant floor type (70% coverage)
-- [ ] Hazard types are diverse (Terrain, Dynamic, Cover, etc.)
-- [ ] All special_rules documented
-- [ ] Validation queries pass
-- [ ] v5.0 voice compliance (technology, not magic)
-- =====================================================

-- =====================================================
-- END v0.30.2 SEEDING SCRIPT
-- =====================================================
