-- =====================================================
-- v0.29.2: Environmental Hazards & Ambient Conditions
-- =====================================================
-- Version: v0.29.2
-- Author: Rune & Rust Development Team
-- Date: 2025-11-15
-- Prerequisites: v0.29.1 (Database Schema & Room Templates)
-- =====================================================

-- NOTE: This script is for reference only.
-- The actual migration is integrated into SaveRepository.cs
-- See: SeedMuspelheimBiomeData() method

BEGIN TRANSACTION;

-- =====================================================
-- ENVIRONMENTAL HAZARDS SEEDING
-- =====================================================

-- Hazard 1: Burning Ground
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Burning Ground]', 'Terrain',
    'Flames or superheated metal. Deals Fire damage each turn to those standing on it.',
    8, 'Fire', 15, 0, 0, 0, 'Medium', 'persistent_fire'
);

-- Hazard 2: Chasm/Lava River
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Chasm/Lava River]', 'Obstacle',
    'Impassable molten slag river. Instant death if pushed/moved into. Controllers dream of this.',
    999, 'Fire', 10, 0, 1, 0, 'High', 'instant_death_on_enter'
);

-- Hazard 3: High-Pressure Steam Vent
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[High-Pressure Steam Vent]', 'Dynamic',
    'Pressure valve venting superheated steam. Deals burst damage + Disoriented. Destructible via Environmental Combat.',
    16, 'Fire', 5, 1, 0, 1, 'High', 'applies_disoriented,destructible'
);

-- Hazard 4: Volatile Gas Pocket
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Volatile Gas Pocket]', 'Explosive',
    'Combustible gas pocket. Explodes when Fire damage dealt nearby (3-tile radius), causing 4d6 AoE Fire damage.',
    24, 'Fire', 3, 1, 0, 0, 'Extreme', 'chain_reaction,aoe_radius_3'
);

-- Hazard 5: Scorched Metal Plating
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Scorched Metal Plating]', 'Terrain',
    'Heat-warped structural plating. Movement cost doubled (10 ft becomes 5 ft effective). No damage.',
    0, NULL, 20, 0, 0, 0, 'Low', 'movement_cost_doubled'
);

-- Hazard 6: Molten Slag Pool
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Molten Slag Pool]', 'Terrain',
    'Shallow pool of cooling slag. Deals Fire damage and applies [Slowed] (-50% movement speed).',
    4, 'Fire', 8, 0, 0, 0, 'Medium', 'applies_slowed'
);

-- Hazard 7: Collapsing Catwalk
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Collapsing Catwalk]', 'Dynamic',
    'Unstable walkway. 20% chance per turn of collapse if occupied. Combatant falls to [Chasm] below.',
    999, 'Physical', 5, 0, 0, 0, 'Extreme', 'collapse_chance_20,fall_to_chasm'
);

-- Hazard 8: Thermal Mirage
INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    4, '[Thermal Mirage]', 'Vision',
    'Heat shimmer distorts vision. Ranged attacks through affected tiles suffer -2 penalty.',
    0, NULL, 25, 0, 0, 0, 'Low', 'ranged_attack_penalty_2'
);

COMMIT;

-- =====================================================
-- VALIDATION QUERIES
-- =====================================================

-- Verify all 8 hazards inserted
SELECT COUNT(*) as hazard_count
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 4;
-- Expected: 8

-- View all hazards with details
SELECT
    feature_name,
    feature_type,
    hazard_density_category,
    damage_per_turn,
    damage_type,
    tile_coverage_percent
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 4
ORDER BY hazard_density_category, feature_name;

-- Check hazard type distribution
SELECT
    feature_type,
    COUNT(*) as count
FROM Biome_EnvironmentalFeatures
WHERE biome_id = 4
GROUP BY feature_type;

-- =====================================================
-- END v0.29.2 REFERENCE SCRIPT
-- =====================================================
