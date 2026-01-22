-- =====================================================
-- v0.30.3: Niflheim Enemy Definitions & Spawn System
-- =====================================================
-- Version: v0.30.3
-- Author: Rune & Rust Development Team
-- Date: 2025-11-16
-- Prerequisites: v0.30.2 (Environmental Hazards & Ambient Conditions)
-- =====================================================
-- Document ID: RR-SPEC-v0.30.3-ENEMIES
-- Parent Specification: v0.30 Niflheim Biome Implementation
-- Status: Design Complete — Ready for Implementation
-- Timeline: 10-15 hours
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- SECTION 1: TABLE DEFINITIONS (IF NOT EXISTS)
-- =====================================================

-- Table: Enemies (simplified structure, following Muspelheim pattern)
-- NOTE: Full detailed Enemies table may exist elsewhere in codebase
-- This script uses Biome_EnemySpawns with JSON for flexibility

-- Table: Biome_EnemySpawns (already created in v0.29.1)
CREATE TABLE IF NOT EXISTS Biome_EnemySpawns (
    spawn_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    enemy_name TEXT NOT NULL,
    enemy_type TEXT NOT NULL,
    min_level INTEGER DEFAULT 1,
    max_level INTEGER DEFAULT 12,
    spawn_weight INTEGER DEFAULT 100,
    spawn_rules_json TEXT,
    verticality_tier TEXT CHECK(verticality_tier IN ('Roots', 'Canopy', 'Both')),
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_biome_enemy_spawns_biome ON Biome_EnemySpawns(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_enemy_spawns_type ON Biome_EnemySpawns(enemy_type);

-- =====================================================
-- SECTION 2: ENEMY DEFINITIONS (5 Types)
-- =====================================================
-- Using Muspelheim pattern: Biome_EnemySpawns with JSON spawn_rules
-- JSON structure includes: resistances, vulnerabilities, tags, abilities, loot
-- =====================================================

-- =====================================================
-- ENEMY 1: Frost-Rimed Undying
-- =====================================================
-- Concept: Standard Undying with ice coating
-- Level: 8 (mid-tier)
-- Ice Resistance: 75%, Fire Vulnerability: 50%
-- Spawn Weight: 150 (Very Common)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    5, 'Frost-Rimed Undying', 'Undying',
    7, 9,
    150, -- Very Common (most frequent encounter)
    'Both', -- Spawns in both Roots and Canopy
    '{
        "hp": 85,
        "accuracy": 70,
        "mitigation": 25,
        "attributes": {
            "MIGHT": 14,
            "FINESSE": 8,
            "STURDINESS": 16,
            "WITS": 6,
            "WILL": 4
        },
        "resistances": {
            "Ice": 75,
            "Physical": 25,
            "Psychic": 100
        },
        "vulnerabilities": {
            "Fire": 50,
            "Energy": 25
        },
        "movement_range": 2,
        "action_points": 2,
        "abilities": [
            {
                "name": "Frost Strike",
                "ap_cost": 1,
                "damage_dice": "2d8+4",
                "damage_type": "Ice",
                "range": "Melee",
                "description": "Melee attack with ice-encrusted limbs. Lethal in [Frigid Cold] due to universal Ice vulnerability."
            },
            {
                "name": "Brittle Armor",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Ice coating provides Ice Resistance but creates Fire Vulnerability. Fire damage reduces Physical Mitigation by 10 for 2 turns."
            }
        ],
        "loot_drops": [
            {"resource": "Frozen Circuitry", "chance": 0.6},
            {"resource": "Supercooled Alloy", "chance": 0.3},
            {"resource": "Cryo-Coolant Fluid", "chance": 0.4}
        ],
        "tags": ["brittle_on_ice", "cold_immune", "machine", "ice_coated"],
        "description": "Security unit that has operated in cryogenic conditions for centuries. Ice has formed over its chassis, providing insulation against cold but creating structural weaknesses. Slow but resilient."
    }'
);

-- =====================================================
-- ENEMY 2: Cryo-Drone
-- =====================================================
-- Concept: Malfunctioning cryogenic maintenance drone
-- Level: 7 (mid-tier)
-- Ice Resistance: 100% (Immune), Fire Vulnerability: 75%
-- Spawn Weight: 120 (Common)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    5, 'Cryo-Drone', 'Undying',
    6, 8,
    120, -- Common
    'Both',
    '{
        "hp": 60,
        "accuracy": 75,
        "mitigation": 15,
        "attributes": {
            "MIGHT": 10,
            "FINESSE": 14,
            "STURDINESS": 10,
            "WITS": 8,
            "WILL": 4
        },
        "resistances": {
            "Ice": 100,
            "Psychic": 100
        },
        "vulnerabilities": {
            "Fire": 75,
            "Physical": 25,
            "Energy": 50
        },
        "movement_range": 3,
        "action_points": 2,
        "abilities": [
            {
                "name": "Nitrogen Spray",
                "ap_cost": 2,
                "damage_dice": "3d6",
                "damage_type": "Ice",
                "range": "Ranged",
                "aoe": "Cone_3",
                "accuracy_modifier": 10,
                "special": "STURDINESS DC 12 or [Slowed] for 1 turn",
                "description": "Sprays liquid nitrogen in 3-tile cone. Devastating in [Frigid Cold]."
            },
            {
                "name": "Evasive Flight",
                "ap_cost": 1,
                "type": "Mobility",
                "description": "Flies to any tile within 3 spaces. Ignores [Slippery Terrain]. No opportunity attacks."
            }
        ],
        "loot_drops": [
            {"resource": "Cryo-Coolant Fluid", "chance": 0.8},
            {"resource": "Frozen Circuitry", "chance": 0.4},
            {"resource": "Pristine Ice Core", "chance": 0.15}
        ],
        "tags": ["brittle_on_ice", "cold_immune", "flying", "machine", "drone"],
        "description": "Corrupted cryogenic maintenance drone. Malfunctioning liquid nitrogen dispensers spray freezing jets. Mobile and dangerous in close quarters."
    }'
);

-- =====================================================
-- ENEMY 3: Ice-Adapted Beast
-- =====================================================
-- Concept: Feral predator adapted to extreme cold
-- Level: 8 (mid-tier)
-- Ice Resistance: 50%, Fire Vulnerability: 50%
-- Spawn Weight: 80 (Roots), 40 (Canopy)
-- =====================================================

-- Spawn entry 1: Ice-Adapted Beast (Roots - more common)
INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    5, 'Ice-Adapted Beast', 'Beast',
    7, 9,
    80, -- Medium-High (more common in Roots)
    'Roots',
    '{
        "hp": 95,
        "accuracy": 75,
        "mitigation": 20,
        "attributes": {
            "MIGHT": 16,
            "FINESSE": 12,
            "STURDINESS": 14,
            "WITS": 10,
            "WILL": 8
        },
        "resistances": {
            "Ice": 50,
            "Physical": 15
        },
        "vulnerabilities": {
            "Fire": 50,
            "Psychic": 25
        },
        "movement_range": 3,
        "action_points": 2,
        "abilities": [
            {
                "name": "Savage Bite",
                "ap_cost": 1,
                "damage_dice": "2d10+6",
                "damage_type": "Physical",
                "range": "Melee",
                "special": "Critical hit applies [Bleeding] (1d6 per turn for 3 turns)",
                "description": "Powerful bite attack. High Physical damage."
            },
            {
                "name": "Pounce",
                "ap_cost": 2,
                "damage_dice": "2d8+4",
                "damage_type": "Physical",
                "range": "4_tiles_leap",
                "special": "+50% damage if target is knocked down",
                "description": "Leaps up to 4 tiles and attacks. Ignores [Slippery Terrain]. Exploits vulnerability."
            },
            {
                "name": "Pack Hunter",
                "ap_cost": 0,
                "type": "Passive",
                "description": "+10 Accuracy for each allied Ice-Adapted Beast within 3 tiles. Pack tactics."
            },
            {
                "name": "Ice-Walker",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Immune to [Slippery Terrain]. No FINESSE checks for movement on ice."
            }
        ],
        "loot_drops": [
            {"resource": "Ice-Bear Pelt", "chance": 0.7},
            {"resource": "Frost-Lichen", "chance": 0.4},
            {"resource": "Pristine Ice Core", "chance": 0.1}
        ],
        "tags": ["brittle_on_ice", "cold_adapted", "pack_hunter", "ice_walker", "beast"],
        "description": "Feral predator adapted to extreme cold. Thick fur provides insulation and camouflage. Hunts in packs. Dangerous and aggressive."
    }'
);

-- Spawn entry 2: Ice-Adapted Beast (Canopy - less common)
INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    5, 'Ice-Adapted Beast', 'Beast',
    7, 9,
    40, -- Medium (less common in Canopy)
    'Canopy',
    '{
        "hp": 95,
        "accuracy": 75,
        "mitigation": 20,
        "attributes": {
            "MIGHT": 16,
            "FINESSE": 12,
            "STURDINESS": 14,
            "WITS": 10,
            "WILL": 8
        },
        "resistances": {
            "Ice": 50,
            "Physical": 15
        },
        "vulnerabilities": {
            "Fire": 50,
            "Psychic": 25
        },
        "movement_range": 3,
        "action_points": 2,
        "abilities": [
            {
                "name": "Savage Bite",
                "ap_cost": 1,
                "damage_dice": "2d10+6",
                "damage_type": "Physical",
                "range": "Melee",
                "special": "Critical hit applies [Bleeding] (1d6 per turn for 3 turns)",
                "description": "Powerful bite attack. High Physical damage."
            },
            {
                "name": "Pounce",
                "ap_cost": 2,
                "damage_dice": "2d8+4",
                "damage_type": "Physical",
                "range": "4_tiles_leap",
                "special": "+50% damage if target is knocked down",
                "description": "Leaps up to 4 tiles and attacks. Ignores [Slippery Terrain]."
            },
            {
                "name": "Pack Hunter",
                "ap_cost": 0,
                "type": "Passive",
                "description": "+10 Accuracy for each allied Ice-Adapted Beast within 3 tiles."
            },
            {
                "name": "Ice-Walker",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Immune to [Slippery Terrain]."
            }
        ],
        "loot_drops": [
            {"resource": "Ice-Bear Pelt", "chance": 0.7},
            {"resource": "Frost-Lichen", "chance": 0.4},
            {"resource": "Pristine Ice Core", "chance": 0.1}
        ],
        "tags": ["brittle_on_ice", "cold_adapted", "pack_hunter", "ice_walker", "beast"],
        "description": "Feral predator adapted to extreme cold. Thick fur provides insulation and camouflage. Hunts in packs."
    }'
);

-- =====================================================
-- ENEMY 4: Frost-Giant (MINI-BOSS)
-- =====================================================
-- Concept: Dormant Jötun-Forged warmachine, flash-frozen
-- Level: 12 (Boss tier)
-- Ice Resistance: 90%, Fire Vulnerability: 75%
-- Spawn Weight: 5 (Very Rare, boss encounters only)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    5, 'Frost-Giant', 'Boss',
    12, 12,
    5, -- Very Rare (boss encounter)
    'Roots', -- Only spawns in Roots (The Frost-Giant Tomb)
    '{
        "hp": 250,
        "accuracy": 65,
        "mitigation": 40,
        "attributes": {
            "MIGHT": 22,
            "FINESSE": 6,
            "STURDINESS": 20,
            "WITS": 8,
            "WILL": 6
        },
        "resistances": {
            "Ice": 90,
            "Physical": 50,
            "Psychic": 100
        },
        "vulnerabilities": {
            "Fire": 75,
            "Energy": 25
        },
        "movement_range": 1,
        "action_points": 3,
        "abilities": [
            {
                "name": "Shattering Slam",
                "ap_cost": 2,
                "damage_dice": "3d12+8",
                "damage_type": "Physical",
                "range": "Melee",
                "accuracy_modifier": -10,
                "special": "STURDINESS DC 15 or [Knocked Down] + pushed 2 tiles. Destroys cover.",
                "description": "Devastating melee attack. Massive Physical damage."
            },
            {
                "name": "Frost Wave",
                "ap_cost": 3,
                "damage_dice": "2d10+6",
                "damage_type": "Ice",
                "aoe": "Cone_4",
                "special": "Creates [Slippery Terrain] in affected area",
                "description": "Slams ground, releases ice shards in 4-tile cone. Devastating in [Frigid Cold]."
            },
            {
                "name": "Frozen Colossus",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Immune to [Knocked Down], [Slowed], forced movement. Fire damage melts ice coating (-10 Mitigation per Fire attack, stacks)."
            },
            {
                "name": "Shatter Defense",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Physical attacks reduce target Mitigation by 5 for 2 turns (stacks up to 3 times)."
            }
        ],
        "loot_drops": [
            {"resource": "Heart of the Frost-Giant", "chance": 1.0},
            {"resource": "Supercooled Alloy", "chance": 0.8},
            {"resource": "Pristine Ice Core", "chance": 0.6},
            {"resource": "Cryogenic Data-Slate", "chance": 0.5}
        ],
        "tags": ["boss", "legendary_resistances", "brittle_on_ice", "cold_immune", "jotun_forged", "multi_phase"],
        "description": "Dormant Jötun-Forged flash-frozen during atmospheric failure. Ice has locked its joints, making it slow but incredibly resilient. Boss-tier threat requiring coordinated tactics."
    }'
);

-- =====================================================
-- ENEMY 5: Forlorn Echo (Frozen Dead)
-- =====================================================
-- Concept: Psychic echo preserved by flash-freezing
-- Level: 7 (mid-tier)
-- Ice Resistance: 50%, Physical Resistance: 75%
-- Spawn Weight: 60 (Canopy), 30 (Roots)
-- =====================================================

-- Spawn entry 1: Forlorn Echo (Canopy - more common)
INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    5, 'Forlorn Echo (Frozen Dead)', 'Forlorn',
    6, 8,
    60, -- Medium (more common in Canopy)
    'Canopy',
    '{
        "hp": 50,
        "accuracy": 70,
        "mitigation": 0,
        "attributes": {
            "MIGHT": 8,
            "FINESSE": 12,
            "STURDINESS": 8,
            "WITS": 14,
            "WILL": 16
        },
        "resistances": {
            "Physical": 75,
            "Ice": 50,
            "Energy": 50
        },
        "vulnerabilities": {
            "Psychic": 50,
            "Fire": 25
        },
        "movement_range": 2,
        "action_points": 2,
        "abilities": [
            {
                "name": "Frozen Wail",
                "ap_cost": 1,
                "damage_dice": "2d6+4",
                "damage_type": "Psychic",
                "range": "Ranged",
                "special": "Target gains +3 Psychic Stress",
                "description": "Crystallized scream of terror. Psychic damage + Stress accumulation."
            },
            {
                "name": "Incorporeal",
                "ap_cost": 0,
                "type": "Passive",
                "description": "75% Physical damage resistance. Ignores [Slippery Terrain]. Moves through solid objects."
            },
            {
                "name": "Preserved Echo",
                "ap_cost": 0,
                "type": "Passive",
                "description": "When defeated, +8 Psychic Stress to all party members within 4 tiles."
            }
        ],
        "loot_drops": [
            {"resource": "Cryogenic Data-Slate", "chance": 0.4},
            {"resource": "Pristine Ice Core", "chance": 0.2},
            {"resource": "Eternal Frost Crystal", "chance": 0.05}
        ],
        "tags": ["psychic_entity", "incorporeal", "stress_dealer", "forlorn"],
        "description": "Psychic echo preserved by flash-freezing. Fainter than typical Forlorn due to cold preservation. Less aggressive but still generates Psychic Stress."
    }'
);

-- Spawn entry 2: Forlorn Echo (Roots - less common)
INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    5, 'Forlorn Echo (Frozen Dead)', 'Forlorn',
    6, 8,
    30, -- Low-Medium (less common in Roots)
    'Roots',
    '{
        "hp": 50,
        "accuracy": 70,
        "mitigation": 0,
        "attributes": {
            "MIGHT": 8,
            "FINESSE": 12,
            "STURDINESS": 8,
            "WITS": 14,
            "WILL": 16
        },
        "resistances": {
            "Physical": 75,
            "Ice": 50,
            "Energy": 50
        },
        "vulnerabilities": {
            "Psychic": 50,
            "Fire": 25
        },
        "movement_range": 2,
        "action_points": 2,
        "abilities": [
            {
                "name": "Frozen Wail",
                "ap_cost": 1,
                "damage_dice": "2d6+4",
                "damage_type": "Psychic",
                "range": "Ranged",
                "special": "Target gains +3 Psychic Stress",
                "description": "Crystallized scream of terror. Psychic damage + Stress."
            },
            {
                "name": "Incorporeal",
                "ap_cost": 0,
                "type": "Passive",
                "description": "75% Physical damage resistance. Ignores [Slippery Terrain]."
            },
            {
                "name": "Preserved Echo",
                "ap_cost": 0,
                "type": "Passive",
                "description": "When defeated, +8 Psychic Stress to all within 4 tiles."
            }
        ],
        "loot_drops": [
            {"resource": "Cryogenic Data-Slate", "chance": 0.4},
            {"resource": "Pristine Ice Core", "chance": 0.2},
            {"resource": "Eternal Frost Crystal", "chance": 0.05}
        ],
        "tags": ["psychic_entity", "incorporeal", "stress_dealer", "forlorn"],
        "description": "Psychic echo preserved by flash-freezing. Fainter than typical Forlorn. Generates Psychic Stress."
    }'
);

COMMIT;

-- =====================================================
-- VALIDATION QUERIES
-- =====================================================

-- Test 1: Verify all 7 enemy spawn entries inserted (5 unique enemies, 2 duplicates for verticality)
SELECT COUNT(*) as spawn_entry_count
FROM Biome_EnemySpawns
WHERE biome_id = 5;
-- Expected: 7

-- Test 2: Verify unique enemy count
SELECT COUNT(DISTINCT enemy_name) as unique_enemy_count
FROM Biome_EnemySpawns
WHERE biome_id = 5;
-- Expected: 5 (Frost-Rimed, Cryo-Drone, Ice-Adapted Beast, Frost-Giant, Forlorn Echo)

-- Test 3: View all enemies with spawn weights
SELECT
    enemy_name,
    enemy_type,
    min_level,
    max_level,
    spawn_weight,
    verticality_tier
FROM Biome_EnemySpawns
WHERE biome_id = 5
ORDER BY spawn_weight DESC;

-- Test 4: Check enemy type distribution
SELECT
    enemy_type,
    COUNT(*) as count,
    SUM(spawn_weight) as total_weight
FROM Biome_EnemySpawns
WHERE biome_id = 5
GROUP BY enemy_type;

-- Test 5: Verify boss spawns
SELECT enemy_name, spawn_weight, verticality_tier
FROM Biome_EnemySpawns
WHERE biome_id = 5
  AND enemy_type = 'Boss';
-- Expected: Frost-Giant with weight 5, Roots only

-- Test 6: Check verticality distribution
SELECT
    verticality_tier,
    COUNT(*) as spawn_entry_count,
    AVG(spawn_weight) as avg_weight
FROM Biome_EnemySpawns
WHERE biome_id = 5
GROUP BY verticality_tier;

-- Test 7: Verify level ranges
SELECT
    enemy_name,
    min_level,
    max_level,
    (max_level - min_level + 1) as level_span
FROM Biome_EnemySpawns
WHERE biome_id = 5
ORDER BY min_level;

-- Test 8: Verify Ice Resistance pattern (check JSON)
SELECT
    enemy_name,
    json_extract(spawn_rules_json, '$.resistances.Ice') as ice_resistance,
    json_extract(spawn_rules_json, '$.vulnerabilities.Fire') as fire_vulnerability
FROM Biome_EnemySpawns
WHERE biome_id = 5
ORDER BY enemy_name;
-- Expected: All have Ice Resistance, most have Fire Vulnerability

-- Test 9: Verify loot drops exist
SELECT
    enemy_name,
    json_extract(spawn_rules_json, '$.loot_drops') as loot_table
FROM Biome_EnemySpawns
WHERE biome_id = 5
LIMIT 3;

-- Test 10: Verify tags include brittle_on_ice
SELECT
    enemy_name,
    json_extract(spawn_rules_json, '$.tags') as tags
FROM Biome_EnemySpawns
WHERE biome_id = 5
  AND json_extract(spawn_rules_json, '$.tags') LIKE '%brittle_on_ice%';
-- Expected: Most enemies (4/5, excluding Forlorn Echo)

-- =====================================================
-- BRITTLENESS MECHANIC NOTES
-- =====================================================

-- [Brittle] Debuff Application Rules (Niflheim):
-- 1. Enemy has Ice Resistance > 0% (check spawn_rules_json.resistances.Ice)
-- 2. Enemy takes Ice damage
-- 3. Apply [Brittle] status effect (duration: 1 turn)
-- 4. While [Brittle]: Physical damage +50%
-- 5. [Brittle] can be refreshed (resets duration)

-- Eligible Enemies (Ice Resistance > 0%):
-- - Frost-Rimed Undying (75%)
-- - Cryo-Drone (100%)
-- - Ice-Adapted Beast (50%)
-- - Frost-Giant (90%)
-- - Forlorn Echo (50%)

-- Tactical Combo Example (Inverse of Muspelheim):
-- Frost Mystic casts Ice Bolt (Ice damage) → [Brittle] applied to Ice-resistant enemy
-- Physical Warrior attacks → Base 20 damage becomes 30 damage (+50%)
-- Fire Mage attacks → Bypasses Ice Resistance, exploits Fire Vulnerability

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] All 5 enemy types defined
-- [ ] 7 spawn entries created (verticality variants)
-- [ ] Ice Resistance + Fire Vulnerability pattern on 4/5 enemies
-- [ ] Spawn weights correctly calibrated
-- [ ] Frost-Giant boss framework implemented
-- [ ] All enemy stats in JSON format
-- [ ] Loot tables correctly assigned
-- [ ] Tags include brittle_on_ice where appropriate
-- [ ] Validation queries pass
-- [ ] v5.0 voice compliance (technology, not magic)
-- =====================================================

-- =====================================================
-- END v0.30.3 SEEDING SCRIPT
-- =====================================================
