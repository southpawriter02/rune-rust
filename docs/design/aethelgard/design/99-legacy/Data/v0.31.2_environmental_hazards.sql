-- =====================================================
-- v0.31.2: Alfheim Environmental Hazards & Ambient Conditions
-- =====================================================
-- Version: v0.31.2
-- Author: Rune & Rust Development Team
-- Date: 2025-11-16
-- Prerequisites: v0.31.1 (Alfheim Database Schema)
-- =====================================================
-- Document ID: RR-SPEC-v0.31.2-ENVIRONMENTAL
-- Parent Specification: v0.31 Alfheim Biome Implementation
-- Status: Design Complete — Ready for Implementation
-- Timeline: 8-12 hours
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- SECTION 1: CONDITION SYSTEM TABLES (IF NOT EXISTS)
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
-- SECTION 2: AMBIENT CONDITION - [Runic Instability]
-- =====================================================

-- Condition Definition
INSERT OR IGNORE INTO Conditions (condition_id, condition_name, condition_type, description, is_ambient)
VALUES (
    107,
    'Runic Instability',
    'Environmental',
    'Biome-wide Aetheric feedback loop. Mystic abilities have 25% chance to trigger Wild Magic Surge (random modification). Mystics gain +10% Aether Pool capacity but each surge generates +5 Psychic Stress. The ambient energy amplifies power but makes control dangerous.',
    1
);

-- Effect 1: Wild Magic Surge Chance
INSERT OR IGNORE INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    107,
    'Wild_Magic_Surge_Chance',
    25,
    'Wild Magic Surge: 25% chance per Mystic ability to trigger random modification (damage, range, targets, or duration ±50%).'
);

-- Effect 2: Aether Pool Amplification
INSERT OR IGNORE INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    107,
    'Aether_Pool_Modifier',
    10,
    'Amplified Aether: Mystics gain +10% base Aether Pool capacity in this biome.'
);

-- Effect 3: Psychic Feedback
INSERT OR IGNORE INTO Condition_Effects (condition_id, effect_type, effect_value, effect_description)
VALUES (
    107,
    'Psychic_Stress_Per_Surge',
    5,
    'Psychic Feedback: Each Wild Magic Surge generates +5 Psychic Stress.'
);

-- Update Biomes table to link Runic Instability
UPDATE Biomes
SET ambient_condition_id = 107
WHERE biome_id = 6;

-- =====================================================
-- SECTION 3: ENVIRONMENTAL FEATURES
-- =====================================================

-- Table: Biome_EnvironmentalFeatures (if not exists)
CREATE TABLE IF NOT EXISTS Biome_EnvironmentalFeatures (
    feature_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    feature_name TEXT NOT NULL,
    feature_type TEXT CHECK(feature_type IN ('Hazard', 'Terrain', 'Ambient', 'Dynamic_Hazard', 'Interactive_Hazard', 'Cover', 'Ambient_Effect')),
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

-- =====================================================
-- HAZARD 1: Reality Tear
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    6,
    'Reality Tear',
    'Dynamic_Hazard',
    'Localized physics failure. Entering Reality Tear warps character to random tile 3-5 spaces away, deals 2d8 Energy damage, applies +5 Corruption, and inflicts [Dazed] for 1 turn. Represents catastrophic spacetime fabric rupture.',
    0, -- Damage is handled by special_rules
    'Energy',
    8.0, -- 8% tile coverage
    0, -- Not destructible
    0, -- Does not block movement (warps instead)
    0, -- Does not block LoS
    'High',
    '{"damage_dice": "2d8", "warp_distance_min": 3, "warp_distance_max": 5, "corruption": 5, "status_effect": "Dazed", "status_duration": 1}'
);

-- =====================================================
-- HAZARD 2: Low Gravity Pocket
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    6,
    'Low Gravity Pocket',
    'Ambient_Effect',
    'Physics disruption zone (4x4 tile area). Movement costs half Stamina (minimum 1), Leap distance doubled, forced movement amplified +1 tile. Represents localized gravity field collapse from failed anti-grav systems.',
    0,
    NULL,
    12.0, -- 12% coverage (larger zones)
    0,
    0,
    0,
    'Medium',
    '{"zone_size": "4x4", "movement_cost_modifier": 0.5, "leap_distance_modifier": 2.0, "forced_movement_bonus": 1}'
);

-- =====================================================
-- HAZARD 3: Unstable Platform
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    6,
    'Unstable Platform',
    'Dynamic_Hazard',
    'Platform flickers between solid and incorporeal states every 2 turns. When incorporeal: impassable, projectiles pass through. Pattern is predictable - requires timing. Malfunctioning phase-shift technology from Pre-Glitch architecture.',
    0,
    NULL,
    10.0,
    0,
    1, -- Blocks movement when incorporeal
    0,
    'Medium',
    '{"phase_cycle_turns": 2, "initial_state": "solid", "projectiles_pass_through_when_incorporeal": true}'
);

-- =====================================================
-- HAZARD 4: Energy Conduit
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    6,
    'Energy Conduit',
    'Interactive_Hazard',
    'Active Aetheric conduit. Deals 1d10 Energy damage per turn to adjacent characters. Characters with 50%+ Energy Resistance can channel it for +5 Aether Pool. Destructible (30 HP). Ruptured energy distribution systems.',
    10, -- Average damage (1d10)
    'Energy',
    15.0, -- Common hazard
    1, -- Destructible
    0,
    0,
    'High',
    '{"damage_dice": "1d10", "damage_range": "adjacent", "hp": 30, "channel_requirement": "Energy_Resistance_50", "channel_bonus": 5}'
);

-- =====================================================
-- HAZARD 5: Aetheric Vortex
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    6,
    'Aetheric Vortex',
    'Dynamic_Hazard',
    'Swirling Aetheric energy vortex. Pulls all characters within 3 tiles closer by 1 tile per turn. Center deals 2d6 Energy damage. Mystics pulled in gain +10 Aether Pool but +5 Psychic Stress. Represents feedback loop in energy systems.',
    13, -- Average damage (2d6)
    'Energy',
    5.0, -- Rare
    0,
    0,
    0,
    'Extreme',
    '{"damage_dice": "2d6", "pull_range": 3, "pull_distance": 1, "mystic_aether_bonus": 10, "mystic_stress_penalty": 5}'
);

-- =====================================================
-- HAZARD 6: Crystalline Spire (Cover)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    6,
    'Crystalline Spire',
    'Cover',
    'Solidified Aetheric energy crystal formation. Provides excellent cover (+4 Defense). Reflects 25% of Energy damage back at attacker. Destructible (50 HP). When destroyed, creates 3x3 difficult terrain from crystal shards.',
    0,
    NULL,
    18.0, -- Common cover
    1, -- Destructible
    1, -- Blocks movement
    1, -- Blocks LoS
    'Medium',
    '{"defense_bonus": 4, "hp": 50, "energy_damage_reflection": 0.25, "destroyed_creates_terrain": "difficult_3x3"}'
);

-- =====================================================
-- HAZARD 7: Holographic Interference
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    6,
    'Holographic Interference',
    'Ambient_Effect',
    'Malfunctioning holographic projectors create flickering interference (3x3 zone). Obscures line of sight - ranged attacks at Disadvantage. Can be dispelled with 10+ Energy damage or 5 Aether spent by Mystic (clears for 2 turns).',
    0,
    NULL,
    9.0,
    0,
    0,
    1, -- Blocks LoS (obscures)
    'Low',
    '{"zone_size": "3x3", "ranged_attack_penalty": "Disadvantage", "dispel_damage": 10, "dispel_aether_cost": 5, "dispel_duration": 2}'
);

-- =====================================================
-- HAZARD 8: Paradox Containment Breach
-- =====================================================

INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
    biome_id, feature_name, feature_type, feature_description,
    damage_per_turn, damage_type, tile_coverage_percent,
    is_destructible, blocks_movement, blocks_line_of_sight,
    hazard_density_category, special_rules
) VALUES (
    6,
    'Paradox Containment Breach',
    'Ambient_Effect',
    'Zone where causality has broken down (2x2 tiles). Effects occur before their causes. Characters in zone have 50% chance each turn to act first in initiative OR last (random). Creates tactical unpredictability. Reality manipulation failure.',
    0,
    NULL,
    4.0, -- Rare
    0,
    0,
    0,
    'Extreme',
    '{"zone_size": "2x2", "initiative_randomization": 0.5, "effect": "act_first_or_last_random"}'
);

COMMIT;

-- =====================================================
-- END v0.31.2 SEEDING SCRIPT
-- =====================================================

-- =====================================================
-- VERIFICATION QUERIES (Run these to verify seeding)
-- =====================================================

-- Test 1: Runic Instability Condition Exists
-- SELECT COUNT(*) FROM Conditions WHERE condition_id = 107;
-- Expected: 1

-- Test 2: Runic Instability Effects Count
-- SELECT COUNT(*) FROM Condition_Effects WHERE condition_id = 107;
-- Expected: 3 (Wild Magic Surge, Aether Pool, Psychic Stress)

-- Test 3: Biome Ambient Condition Linked
-- SELECT ambient_condition_id FROM Biomes WHERE biome_id = 6;
-- Expected: 107

-- Test 4: Environmental Features Count
-- SELECT COUNT(*) FROM Biome_EnvironmentalFeatures WHERE biome_id = 6;
-- Expected: 8

-- Test 5: Hazard Type Distribution
-- SELECT feature_type, COUNT(*) FROM Biome_EnvironmentalFeatures WHERE biome_id = 6 GROUP BY feature_type;
-- Expected: Dynamic_Hazard: 3, Ambient_Effect: 3, Interactive_Hazard: 1, Cover: 1

-- Test 6: High-Intensity Hazards
-- SELECT feature_name FROM Biome_EnvironmentalFeatures WHERE biome_id = 6 AND hazard_density_category = 'Extreme';
-- Expected: Aetheric Vortex, Paradox Containment Breach

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] [Runic Instability] condition created (condition_id: 107)
-- [ ] 3 condition effects seeded
-- [ ] Biomes table updated with ambient_condition_id
-- [ ] 8 environmental features seeded
-- [ ] Reality Tear configured with warp mechanics
-- [ ] Energy Conduit configured with channeling rules
-- [ ] Crystalline Spire configured with cover and reflection
-- [ ] All special_rules JSON properly formatted
-- [ ] v5.0 voice compliance (technology, not magic)
-- [ ] Database integrity tests pass
-- =====================================================
