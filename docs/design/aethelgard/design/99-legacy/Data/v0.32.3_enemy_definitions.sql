-- =====================================================
-- v0.32.3: Jötunheim Enemy Definitions & Spawn System
-- =====================================================
-- Version: v0.32.3
-- Author: Rune & Rust Development Team
-- Date: 2025-11-16
-- Prerequisites: v0.32.2 (Environmental Hazards)
-- =====================================================
-- Document ID: RR-SPEC-v0.32.3-ENEMIES
-- Parent Specification: v0.32 Jötunheim Biome Implementation
-- Status: Implementation Complete
-- Timeline: 10-16 hours
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- SECTION 1: TABLE DEFINITIONS (IF NOT EXISTS)
-- =====================================================

CREATE TABLE IF NOT EXISTS Biome_EnemySpawns (
    spawn_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    enemy_name TEXT NOT NULL,
    enemy_type TEXT NOT NULL,
    min_level INTEGER DEFAULT 1,
    max_level INTEGER DEFAULT 12,
    spawn_weight INTEGER DEFAULT 100,
    spawn_rules_json TEXT,
    verticality_tier TEXT CHECK(verticality_tier IN ('Roots', 'Trunk', 'Canopy', 'Both')),
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_biome_enemy_spawns_biome ON Biome_EnemySpawns(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_enemy_spawns_type ON Biome_EnemySpawns(enemy_type);

-- =====================================================
-- SECTION 2: ENEMY DEFINITIONS (6 Types)
-- =====================================================
-- 3 Undying (60% spawn weight total)
-- 2 Humanoid (29% spawn weight)
-- 1 Beast (12% spawn weight)
-- =====================================================

-- =====================================================
-- ENEMY 1: Rusted Servitor (Undying - Very Common)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    7, 'Rusted Servitor', 'Undying_Janitorial',
    5, 7,
    200, -- Very Common (~29% of spawns)
    'Both', -- Trunk and Roots
    '{
        "hp": 35,
        "accuracy": 70,
        "mitigation": 10,
        "physical_soak": 4,
        "attributes": {
            "MIGHT": 12,
            "FINESSE": 8,
            "STURDINESS": 14,
            "WITS": 4,
            "WILL": 2
        },
        "resistances": {
            "Physical": 25,
            "Psychic": 100
        },
        "vulnerabilities": {
            "Fire": 25
        },
        "movement_range": 4,
        "action_points": 2,
        "initiative_bonus": 1,
        "abilities": [
            {
                "name": "Maintenance Protocol",
                "ap_cost": 1,
                "damage_dice": "1d8+2",
                "damage_type": "Physical",
                "range": 1,
                "cooldown": 0,
                "description": "Basic attack. Servitor attempts to disassemble targets."
            },
            {
                "name": "Sweep Mode",
                "ap_cost": 2,
                "damage_dice": "1d6",
                "damage_type": "Physical",
                "range": "melee cone (2 adjacent)",
                "cooldown": 2,
                "description": "Attacks 2 adjacent targets simultaneously."
            },
            {
                "name": "Undying Resilience",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Cannot be reduced below 1 HP by single attack. Physical Soak reduces damage before HP loss."
            }
        ],
        "loot_drops": [
            {"resource": "Rusted Scrap Metal", "chance": 0.9},
            {"resource": "Intact Servomotor", "chance": 0.3},
            {"resource": "Ball Bearings", "chance": 0.2}
        ],
        "tags": ["undying", "janitorial", "construct", "corrupted", "fire_vulnerable"],
        "description": "Corroded maintenance unit following 800-year-old cleaning protocols. Not aggressive by design - but anything living registers as debris to be removed. The automation never stopped."
    }'
);

-- =====================================================
-- ENEMY 2: Rusted Warden (Undying - Common)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    7, 'Rusted Warden', 'Undying_Security',
    6, 8,
    150, -- Common (~22% of spawns)
    'Both',
    '{
        "hp": 50,
        "accuracy": 75,
        "mitigation": 12,
        "physical_soak": 6,
        "attributes": {
            "MIGHT": 14,
            "FINESSE": 10,
            "STURDINESS": 16,
            "WITS": 6,
            "WILL": 4
        },
        "resistances": {
            "Physical": 35,
            "Psychic": 100
        },
        "vulnerabilities": {
            "Fire": 20
        },
        "movement_range": 4,
        "action_points": 2,
        "initiative_bonus": 2,
        "abilities": [
            {
                "name": "Security Strike",
                "ap_cost": 2,
                "damage_dice": "2d6+3",
                "damage_type": "Physical",
                "range": 1,
                "cooldown": 1,
                "description": "Heavy melee attack. If target <50% HP, applies [Stunned] for 1 turn."
            },
            {
                "name": "Defensive Stance",
                "ap_cost": 2,
                "type": "Buff",
                "effect": "+4 Defense for 2 turns",
                "cooldown": 3,
                "description": "Gain +4 Defense for 2 turns. Cannot move while in stance."
            },
            {
                "name": "Threat Detection",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Cannot be surprised. Always acts first when combat begins."
            }
        ],
        "loot_drops": [
            {"resource": "Rusted Scrap Metal", "chance": 0.7},
            {"resource": "Intact Servomotor", "chance": 0.5},
            {"resource": "Hydraulic Cylinder", "chance": 0.25},
            {"resource": "Power Relay Circuit", "chance": 0.15}
        ],
        "tags": ["undying", "security", "construct", "corrupted", "fire_vulnerable", "heavy_armor"],
        "description": "Security unit with heavier armor plating. Still follows perimeter defense protocols - hostile to all unregistered personnel. Physical Soak 6 makes it resistant to weak attacks. Requires focused fire or armor-shredding."
    }'
);

-- =====================================================
-- ENEMY 3: Draugr Juggernaut (Undying - Rare Elite)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    7, 'Draugr Juggernaut', 'Undying_Heavy_Assault',
    7, 9,
    60, -- Rare Elite (~9% of spawns)
    'Trunk', -- Factory floor only (too massive for Roots)
    '{
        "hp": 90,
        "accuracy": 80,
        "mitigation": 14,
        "physical_soak": 10,
        "attributes": {
            "MIGHT": 18,
            "FINESSE": 6,
            "STURDINESS": 20,
            "WITS": 4,
            "WILL": 2
        },
        "resistances": {
            "Physical": 50,
            "Psychic": 100
        },
        "vulnerabilities": {
            "Fire": 15
        },
        "movement_range": 3,
        "action_points": 2,
        "initiative_bonus": 0,
        "abilities": [
            {
                "name": "Juggernaut Slam",
                "ap_cost": 3,
                "damage_dice": "3d8+6",
                "damage_type": "Physical",
                "range": 1,
                "cooldown": 2,
                "description": "Devastating melee attack. Target knocked back 2 tiles. If collision, +1d8 damage."
            },
            {
                "name": "Armored Advance",
                "ap_cost": 2,
                "type": "Movement",
                "effect": "Move 3 tiles, gain +4 Defense for 1 turn",
                "cooldown": 2,
                "description": "Can move through difficult terrain."
            },
            {
                "name": "Fortress Protocol",
                "ap_cost": 0,
                "type": "Passive",
                "description": "TEACHING MECHANIC: Takes HALF damage from attacks dealing <15 damage. Physical Soak 10 reduces damage significantly. Requires armor-shredding or high-damage attacks."
            },
            {
                "name": "Undying Endurance",
                "ap_cost": 0,
                "type": "Passive",
                "description": "When reduced to 0 HP, revives at 20 HP once per combat (unless killed by Fire damage)."
            }
        ],
        "loot_drops": [
            {"resource": "Intact Servomotor", "chance": 0.8},
            {"resource": "Hydraulic Cylinder", "chance": 0.6},
            {"resource": "Unblemished Jotun Plating", "chance": 0.15},
            {"resource": "Industrial Servo Actuator", "chance": 0.1}
        ],
        "tags": ["undying", "elite", "heavy_assault", "construct", "corrupted", "fire_vulnerable", "massive_armor", "teaching_enemy"],
        "description": "ELITE: Heavy assault construct with extreme armor plating. TEACHING ENEMY: Requires armor-shredding tactics (Iron-Bane, Rust-Witch) or high-damage focus fire. Physical Soak 10, 50% Physical Resistance. Takes HALF damage from attacks <15 damage. Slow but devastating."
    }'
);

-- =====================================================
-- ENEMY 4: God-Sleeper Cultist (Humanoid - Medium)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    7, 'God-Sleeper Cultist', 'Humanoid_Fanatic',
    6, 8,
    130, -- Medium (~19% of spawns)
    'Both',
    '{
        "hp": 40,
        "accuracy": 70,
        "mitigation": 11,
        "physical_soak": 2,
        "attributes": {
            "MIGHT": 11,
            "FINESSE": 13,
            "STURDINESS": 10,
            "WITS": 10,
            "WILL": 16
        },
        "resistances": {
            "Psychic": 50,
            "Energy": 15
        },
        "vulnerabilities": {},
        "movement_range": 5,
        "action_points": 2,
        "initiative_bonus": 3,
        "abilities": [
            {
                "name": "Salvaged Weapon Strike",
                "ap_cost": 2,
                "damage_dice": "2d6+2",
                "damage_type": "Physical",
                "range": 1,
                "cooldown": 0,
                "description": "Attack with improvised industrial weapon."
            },
            {
                "name": "Devotional Chant",
                "ap_cost": 3,
                "type": "Buff",
                "range": 4,
                "effect": "All allies within range gain +2 Defense and +3 HP for 2 turns",
                "cooldown": 4,
                "description": "Support ability empowering nearby allies."
            },
            {
                "name": "Jotun Attunement",
                "ap_cost": 0,
                "type": "Passive",
                "description": "When in [Jotun Corpse Terrain] or within 3 tiles of dormant Jötun-Forged, gain +4 to all rolls and +10 HP. Cargo cult power boost from psychic broadcast."
            },
            {
                "name": "Fanatical Zeal",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Immune to [Fear] effects. When ally dies, gain +2 to next attack roll."
            }
        ],
        "loot_drops": [
            {"resource": "Rusted Scrap Metal", "chance": 0.6},
            {"resource": "Salvaged Weapon Parts", "chance": 0.4},
            {"resource": "Cultist Robe", "chance": 0.2}
        ],
        "tags": ["humanoid", "fanatic", "cultist", "jotun_attuned", "fearless"],
        "description": "Cargo cultists worshipping Jötun-Forged as sleeping gods. Not delusional - the psychic broadcast from corrupted logic cores DOES grant them power. Dangerous near their temples (Jötun corpses)."
    }'
);

-- =====================================================
-- ENEMY 5: Scrap-Tinker (Humanoid - Uncommon)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    7, 'Scrap-Tinker', 'Humanoid_Scavenger',
    6, 8,
    70, -- Uncommon (~10% of spawns)
    'Both',
    '{
        "hp": 35,
        "accuracy": 75,
        "mitigation": 10,
        "physical_soak": 1,
        "attributes": {
            "MIGHT": 10,
            "FINESSE": 16,
            "STURDINESS": 9,
            "WITS": 14,
            "WILL": 10
        },
        "resistances": {
            "Energy": 25
        },
        "vulnerabilities": {
            "Fire": 10
        },
        "movement_range": 5,
        "action_points": 2,
        "initiative_bonus": 4,
        "abilities": [
            {
                "name": "Improvised Crossbow",
                "ap_cost": 2,
                "damage_dice": "2d6",
                "damage_type": "Physical",
                "range": 6,
                "cooldown": 1,
                "description": "Ranged attack with makeshift crossbow."
            },
            {
                "name": "Deploy Trap",
                "ap_cost": 3,
                "type": "Trap",
                "range": "Adjacent tile",
                "effect": "First character entering takes 2d8 damage and is [Snared] for 1 turn",
                "cooldown": 4,
                "description": "Places [Scrap Trap] on adjacent tile."
            },
            {
                "name": "Smoke Bomb",
                "ap_cost": 2,
                "type": "Utility",
                "range": 3,
                "effect": "Creates [Smoke] in 2x2 area for 2 turns",
                "cooldown": 5,
                "description": "Obscures vision. Tinker can use to escape."
            },
            {
                "name": "Salvage Expertise",
                "ap_cost": 0,
                "type": "Passive",
                "description": "When killed near [Scrap Heap] or industrial debris, guaranteed Tier 2-3 component drop."
            }
        ],
        "loot_drops": [
            {"resource": "Intact Servomotor", "chance": 0.7},
            {"resource": "Power Relay Circuit", "chance": 0.5},
            {"resource": "Ball Bearings", "chance": 0.4},
            {"resource": "Industrial Servo Actuator", "chance": 0.08}
        ],
        "tags": ["humanoid", "scavenger", "ranged", "trapper", "fire_vulnerable", "high_initiative"],
        "description": "Rival scavenger with improvised tech. Not evil - just competition for resources. Uses traps and ranged attacks. Prefers to avoid direct combat. Valuable loot if defeated."
    }'
);

-- =====================================================
-- ENEMY 6: Iron-Husked Boar (Beast - Low)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    7, 'Iron-Husked Boar', 'Beast_Mutated',
    5, 7,
    80, -- Low (~12% of spawns)
    'Both',
    '{
        "hp": 45,
        "accuracy": 70,
        "mitigation": 9,
        "physical_soak": 5,
        "attributes": {
            "MIGHT": 16,
            "FINESSE": 12,
            "STURDINESS": 14,
            "WITS": 4,
            "WILL": 6
        },
        "resistances": {
            "Physical": 30
        },
        "vulnerabilities": {},
        "movement_range": 6,
        "action_points": 2,
        "initiative_bonus": 2,
        "abilities": [
            {
                "name": "Gore Charge",
                "ap_cost": 2,
                "damage_dice": "2d8+4",
                "damage_type": "Physical",
                "range": "4 tiles (straight line)",
                "cooldown": 2,
                "description": "Charges in straight line. Targets in path take damage and are knocked back 1 tile."
            },
            {
                "name": "Thrashing Strike",
                "ap_cost": 1,
                "damage_dice": "1d10+2",
                "damage_type": "Physical",
                "range": 1,
                "cooldown": 0,
                "description": "Wild thrashing attack."
            },
            {
                "name": "Metal Hide",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Physical Soak 5. Scrap metal has fused with hide through Blight exposure. Reduces most attacks significantly."
            },
            {
                "name": "Feral Rage",
                "ap_cost": 0,
                "type": "Passive",
                "description": "When reduced below 50% HP, gains +2 to attack rolls and +1 movement."
            }
        ],
        "loot_drops": [
            {"resource": "Rusted Scrap Metal", "chance": 0.8},
            {"resource": "Iron-Husked Hide", "chance": 0.5},
            {"resource": "Beast Meat", "chance": 0.3}
        ],
        "tags": ["beast", "mutated", "blight_corrupted", "metal_hide", "fast", "aggressive"],
        "description": "Mutated Svin-fylking with scrap metal fused to hide through Blight exposure. Physical Soak 5 (metal hide). Not Undying - still organic. Makes dens in scrap heaps. Fast and aggressive."
    }'
);

COMMIT;

-- =====================================================
-- END v0.32.3 SEEDING SCRIPT
-- =====================================================

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Test 1: Enemy Count
-- SELECT COUNT(*) FROM Biome_EnemySpawns WHERE biome_id = 7;
-- Expected: 6

-- Test 2: Spawn Weight Distribution
-- SELECT enemy_name, spawn_weight,
--   ROUND(100.0 * spawn_weight / (SELECT SUM(spawn_weight) FROM Biome_EnemySpawns WHERE biome_id = 7), 1) as percentage
-- FROM Biome_EnemySpawns
-- WHERE biome_id = 7
-- ORDER BY spawn_weight DESC;
-- Expected: Rusted Servitor ~29%, Rusted Warden ~22%, God-Sleeper ~19%, Draugr ~9%

-- Test 3: Undying Dominance
-- SELECT
--   SUM(CASE WHEN enemy_type LIKE 'Undying%' THEN spawn_weight ELSE 0 END) * 100.0 / SUM(spawn_weight) as undying_percent
-- FROM Biome_EnemySpawns WHERE biome_id = 7;
-- Expected: ~60%

-- Test 4: Verify JSON Structure
-- SELECT enemy_name, json_extract(spawn_rules_json, '$.physical_soak') as soak
-- FROM Biome_EnemySpawns WHERE biome_id = 7
-- ORDER BY soak DESC;
-- Expected: Draugr Juggernaut (10), Rusted Warden (6), Iron-Husked Boar (5), Rusted Servitor (4)

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] 6 enemy types inserted
-- [ ] Spawn weights total 690 (200+150+60+130+70+80)
-- [ ] Undying dominance ~60% (410/690)
-- [ ] Physical Soak values range from 1-10
-- [ ] All JSON valid and includes required fields
-- [ ] Fire vulnerabilities on all Undying
-- [ ] Draugr Juggernaut has teaching mechanics
-- [ ] God-Sleeper has Jotun Attunement
-- =====================================================
