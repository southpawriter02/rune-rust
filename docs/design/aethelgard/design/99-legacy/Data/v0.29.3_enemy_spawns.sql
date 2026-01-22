-- =====================================================
-- v0.29.3: Enemy Definitions & Spawn System
-- =====================================================
-- Version: v0.29.3
-- Author: Rune & Rust Development Team
-- Date: 2025-11-15
-- Prerequisites: v0.29.1 (Database Schema), v0.29.2 (Environmental Hazards)
-- =====================================================

-- NOTE: This script is for reference only.
-- The actual migration will be integrated into SaveRepository.cs

BEGIN TRANSACTION;

-- =====================================================
-- ENEMY SPAWNS SEEDING
-- =====================================================

-- Enemy 1: Forge-Hardened Undying
-- Standard melee with Fire Resistance 75%, Ice Vulnerable -50%
INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    spawn_rules_json
) VALUES (
    4, 'Forge-Hardened Undying', 'Undying',
    7, 9,
    150, -- Most common
    '{"fire_resistance": 75, "ice_resistance": -50, "tags": ["brittle_on_ice", "heat_immune"]}'
);

-- Enemy 2: Magma Elemental
-- Fire Immune 100%, Ice Vulnerable -30%, leaves burning trail
INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    spawn_rules_json
) VALUES (
    4, 'Magma Elemental', 'Construct',
    8, 11,
    80, -- Common
    '{"fire_resistance": 100, "ice_resistance": -30, "physical_resistance": 25, "tags": ["burning_trail", "death_explosion", "brittle_on_ice", "heat_immune", "construct"]}'
);

-- Enemy 3: Rival Berserker
-- Fire Resistant 50%, Ice Vulnerable -25%, Fury mechanic
INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    spawn_rules_json
) VALUES (
    4, 'Rival Berserker', 'Humanoid',
    9, 12,
    60, -- Uncommon (elite)
    '{"fire_resistance": 50, "ice_resistance": -25, "tags": ["fury_resource", "brittle_on_ice", "heat_adapted"]}'
);

-- Enemy 4: Surtur's Herald (Boss Framework)
-- Fire Resistant 90%, Ice Vulnerable -40%, Multi-phase boss
INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    spawn_rules_json
) VALUES (
    4, 'Surtur''s Herald', 'Boss',
    12, 12,
    1, -- Ultra-rare (boss only)
    '{"fire_resistance": 90, "ice_resistance": -40, "physical_resistance": 50, "lightning_resistance": 50, "tags": ["boss", "legendary_resistances", "brittle_on_ice", "heat_immune", "multi_phase"]}'
);

-- Enemy 5: Iron-Bane Crusader
-- Fire Resistant 60%, Potential ally, Construct hunter
INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    spawn_rules_json
) VALUES (
    4, 'Iron-Bane Crusader', 'Humanoid',
    10, 12,
    20, -- Rare (special encounter)
    '{"fire_resistance": 60, "ice_resistance": 0, "corruption_resistance": 75, "tags": ["construct_hunter", "brittle_on_ice", "heat_adapted", "potential_ally"]}'
);

COMMIT;

-- =====================================================
-- VALIDATION QUERIES
-- =====================================================

-- Verify all 5 enemies inserted
SELECT COUNT(*) as enemy_count
FROM Biome_EnemySpawns
WHERE biome_id = 4;
-- Expected: 5

-- View all enemies with spawn weights
SELECT
    enemy_name,
    enemy_type,
    min_level,
    max_level,
    spawn_weight,
    spawn_rules_json
FROM Biome_EnemySpawns
WHERE biome_id = 4
ORDER BY spawn_weight DESC;

-- Check enemy type distribution
SELECT
    enemy_type,
    COUNT(*) as count,
    SUM(spawn_weight) as total_weight
FROM Biome_EnemySpawns
WHERE biome_id = 4
GROUP BY enemy_type;

-- Verify boss spawns
SELECT enemy_name, spawn_weight
FROM Biome_EnemySpawns
WHERE biome_id = 4
  AND enemy_type = 'Boss';
-- Expected: Surtur's Herald with weight 1

-- Check level ranges
SELECT
    enemy_name,
    min_level,
    max_level,
    (max_level - min_level + 1) as level_span
FROM Biome_EnemySpawns
WHERE biome_id = 4
ORDER BY min_level;

-- =====================================================
-- BRITTLENESS MECHANIC NOTES
-- =====================================================

-- [Brittle] Debuff Application Rules:
-- 1. Enemy has Fire Resistance > 0% (check spawn_rules_json.fire_resistance)
-- 2. Enemy takes Ice damage
-- 3. Apply [Brittle] status effect (duration: 2 turns)
-- 4. While [Brittle]: Physical damage +50%
-- 5. [Brittle] can be refreshed (resets duration)

-- Eligible Enemies (Fire Resistance > 0%):
-- - Forge-Hardened Undying (75%)
-- - Magma Elemental (100%)
-- - Rival Berserker (50%)
-- - Surtur's Herald (90%)
-- - Iron-Bane Crusader (60%)

-- Tactical Combo Example:
-- Ice Mystic casts Frost Bolt (Ice damage) → [Brittle] applied
-- Physical Warrior attacks → Base 20 damage becomes 30 damage (+50%)

-- =====================================================
-- END v0.29.3 REFERENCE SCRIPT
-- =====================================================
