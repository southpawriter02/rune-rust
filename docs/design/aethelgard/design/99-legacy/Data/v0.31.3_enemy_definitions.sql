-- =====================================================
-- v0.31.3: Alfheim Enemy Definitions & Spawn System
-- =====================================================
-- Version: v0.31.3
-- Author: Rune & Rust Development Team
-- Date: 2025-11-16
-- Prerequisites: v0.31.2 (Environmental Hazards & Ambient Conditions)
-- =====================================================
-- Document ID: RR-SPEC-v0.31.3-ENEMIES
-- Parent Specification: v0.31 Alfheim Biome Implementation
-- Status: Design Complete — Ready for Implementation
-- Timeline: 10-15 hours
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- SECTION 1: TABLE DEFINITIONS (IF NOT EXISTS)
-- =====================================================

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
-- Using Niflheim pattern: Biome_EnemySpawns with JSON spawn_rules
-- JSON structure includes: resistances, vulnerabilities, tags, abilities, loot
-- =====================================================

-- =====================================================
-- ENEMY 1: Aether-Vulture
-- =====================================================
-- Concept: Energy-adapted aerial predator
-- Level: 8-10 (mid-tier)
-- Energy Resistance: 75%, Psychic Resistance: 25%
-- Spawn Weight: 150 (Very Common)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    6, 'Aether-Vulture', 'Aerial_Predator',
    8, 10,
    150, -- Very Common (35% of spawns)
    'Canopy', -- Canopy only
    '{
        "hp": 45,
        "accuracy": 75,
        "mitigation": 12,
        "attributes": {
            "MIGHT": 12,
            "FINESSE": 16,
            "STURDINESS": 10,
            "WITS": 10,
            "WILL": 8
        },
        "resistances": {
            "Energy": 75,
            "Psychic": 25
        },
        "vulnerabilities": {},
        "movement_range": 6,
        "action_points": 2,
        "initiative_bonus": 4,
        "abilities": [
            {
                "name": "Energy Dive",
                "ap_cost": 2,
                "damage_dice": "2d8+4",
                "damage_type": "Energy",
                "range": 4,
                "cooldown": 2,
                "description": "Diving attack from flight. +50% damage vs. targets with active Mystic buffs."
            },
            {
                "name": "Aetheric Screech",
                "ap_cost": 3,
                "damage_dice": "1d6",
                "damage_type": "Energy",
                "range": "3 tile cone",
                "cooldown": 3,
                "description": "Cone attack. All targets take damage and lose 5 Aether Pool (Mystics only)."
            },
            {
                "name": "Opportunistic Strike",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Gains +2 attack vs. characters with <50% HP."
            }
        ],
        "loot_drops": [
            {"resource": "Aetheric Residue", "chance": 0.8},
            {"resource": "Energy Crystal Shard", "chance": 0.4},
            {"resource": "Vulture Talon", "chance": 0.15}
        ],
        "tags": ["aerial", "flight", "energy_adapted", "mystic_hunter"],
        "description": "Scavenger species that evolved to feed on Aetheric energy leakage. Their biology has been transformed by centuries of exposure - barely recognizable as birds anymore. They hunt Mystics preferentially, drawn to active Aether manipulation."
    }'
);

-- =====================================================
-- ENEMY 2: Energy Elemental
-- =====================================================
-- Concept: Aetheric manifestation from ruptured containment
-- Level: 9-11 (high-tier)
-- Energy Resistance: 90%, Physical Resistance: 50%
-- Spawn Weight: 100 (Uncommon)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    6, 'Energy Elemental', 'Aetheric_Manifestation',
    9, 11,
    100, -- Uncommon (23% of spawns)
    'Canopy',
    '{
        "hp": 60,
        "accuracy": 70,
        "mitigation": 10,
        "attributes": {
            "MIGHT": 14,
            "FINESSE": 10,
            "STURDINESS": 12,
            "WITS": 6,
            "WILL": 4
        },
        "resistances": {
            "Energy": 90,
            "Physical": 50,
            "Fire": 25,
            "Cold": 25
        },
        "vulnerabilities": {},
        "movement_range": 4,
        "action_points": 2,
        "initiative_bonus": 2,
        "abilities": [
            {
                "name": "Discharge Pulse",
                "ap_cost": 2,
                "damage_dice": "2d10",
                "damage_type": "Energy",
                "range": "2 tile AoE (self)",
                "cooldown": 1,
                "description": "AoE around self. All adjacent characters take damage. Energy Elemental takes 10 HP self-damage."
            },
            {
                "name": "Energy Beam",
                "ap_cost": 3,
                "damage_dice": "3d8",
                "damage_type": "Energy",
                "range": "6 tiles (line)",
                "cooldown": 2,
                "description": "Concentrated beam attack. Ignores cover."
            },
            {
                "name": "Unstable Core",
                "ap_cost": 0,
                "type": "Passive",
                "damage_dice": "2d12",
                "damage_type": "Energy",
                "description": "On death, explodes for 2d12 Energy damage in 2 tile radius."
            },
            {
                "name": "Aether Absorption",
                "ap_cost": 0,
                "type": "Reaction",
                "description": "When hit by Mystic ability, absorbs 50% of damage as healing instead."
            }
        ],
        "loot_drops": [
            {"resource": "Aetheric Residue", "chance": 1.0},
            {"resource": "Energy Crystal Shard", "chance": 0.6},
            {"resource": "Unstable Aether Sample", "chance": 0.3},
            {"resource": "Pure Aether Shard", "chance": 0.05}
        ],
        "tags": ["manifestation", "energy", "incorporeal", "explodes_on_death", "mystic_counter"],
        "description": "Emergent phenomenon from ruptured Aetheric containment. Not alive in any meaningful sense - more like a walking feedback loop. Extremely dangerous. Mystic attacks feed it. Explodes catastrophically when containment fails (death)."
    }'
);

-- =====================================================
-- ENEMY 3: Forlorn Echo
-- =====================================================
-- Concept: The Original Dead - psychic fragments of All-Rune researchers
-- Level: 9-12 (high-tier elite)
-- Physical Resistance: 75%, Psychic Immunity: 100%
-- Spawn Weight: 60 (Rare Elite)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    6, 'Forlorn Echo', 'Psychic_Entity_Elite',
    9, 12,
    60, -- Rare Elite (14% of spawns)
    'Canopy',
    '{
        "hp": 55,
        "accuracy": 80,
        "mitigation": 11,
        "attributes": {
            "MIGHT": 8,
            "FINESSE": 14,
            "STURDINESS": 10,
            "WITS": 16,
            "WILL": 18
        },
        "resistances": {
            "Energy": 50,
            "Physical": 75,
            "Psychic": 100,
            "Fire": 50,
            "Cold": 50
        },
        "vulnerabilities": {},
        "movement_range": 5,
        "action_points": 2,
        "initiative_bonus": 5,
        "abilities": [
            {
                "name": "Memory Fragment Strike",
                "ap_cost": 2,
                "damage_dice": "2d8",
                "damage_type": "Psychic",
                "range": 4,
                "cooldown": 1,
                "description": "Psychic attack. Target gains +8 Psychic Stress. If target reaches Stress threshold during combat, Forlorn Echo heals 15 HP."
            },
            {
                "name": "Reality Echo",
                "ap_cost": 3,
                "damage_dice": "3d6",
                "damage_type": "Psychic",
                "range": 3,
                "cooldown": 3,
                "description": "Teleports target 1d4 tiles in random direction (Reality Tear effect). Target gains +5 Corruption."
            },
            {
                "name": "Psychic Resonance Amplification",
                "ap_cost": 0,
                "type": "Passive",
                "description": "All party members gain +3 Psychic Stress per turn while Forlorn Echo is alive."
            },
            {
                "name": "Last Moments",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Cannot be reduced below 1 HP until all other enemies are defeated."
            }
        ],
        "loot_drops": [
            {"resource": "Holographic Data Fragment", "chance": 0.7},
            {"resource": "Psychic Residue", "chance": 0.4},
            {"resource": "Paradox-Touched Component", "chance": 0.2},
            {"resource": "Fragment of the All-Rune", "chance": 0.01}
        ],
        "tags": ["forlorn", "elite", "psychic", "original_dead", "incorporeal", "unkillable"],
        "description": "The Original Dead. Psychic fragments of the researchers who crashed reality. More coherent and powerful than any other Forlorn - they remember what they did. They are reliving their final moments in an endless loop. The psychic pressure from their presence is overwhelming."
    }'
);

-- =====================================================
-- ENEMY 4: Crystalline Construct
-- =====================================================
-- Concept: Pre-Glitch architecture animated by Aetheric Blight
-- Level: 8-10 (mid-tier tank)
-- Physical Resistance: 25%, Fire Vulnerability: 25%
-- Spawn Weight: 120 (Common)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    6, 'Crystalline Construct', 'Animated_Structure',
    8, 10,
    120, -- Common (28% of spawns)
    'Canopy',
    '{
        "hp": 80,
        "accuracy": 65,
        "mitigation": 15,
        "attributes": {
            "MIGHT": 18,
            "FINESSE": 6,
            "STURDINESS": 20,
            "WITS": 4,
            "WILL": 2
        },
        "resistances": {
            "Energy": 50,
            "Physical": 25,
            "Psychic": 100
        },
        "vulnerabilities": {
            "Fire": 25
        },
        "movement_range": 3,
        "action_points": 2,
        "initiative_bonus": 0,
        "abilities": [
            {
                "name": "Crystal Slam",
                "ap_cost": 2,
                "damage_dice": "3d8+6",
                "damage_type": "Physical",
                "range": "Melee",
                "cooldown": 1,
                "description": "Melee attack. Knockback 2 tiles. If target collides with wall/obstacle, +1d8 damage."
            },
            {
                "name": "Shard Spray",
                "ap_cost": 3,
                "damage_dice": "2d6",
                "damage_type": "Physical",
                "range": "4 tile cone",
                "cooldown": 3,
                "description": "Cone attack. Applies Bleed (1d4 damage per turn for 3 turns)."
            },
            {
                "name": "Reflective Surface",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Reflects 25% of Energy damage back at attacker."
            },
            {
                "name": "Structural Integrity",
                "ap_cost": 0,
                "type": "Passive",
                "description": "Takes half damage from attacks that deal <10 damage (armor too thick)."
            }
        ],
        "loot_drops": [
            {"resource": "Energy Crystal Shard", "chance": 0.9},
            {"resource": "Crystallized Aether", "chance": 0.15},
            {"resource": "Structural Component", "chance": 0.5},
            {"resource": "Pure Aether Shard", "chance": 0.08}
        ],
        "tags": ["construct", "animated", "blight_infected", "tank", "slow"],
        "description": "Pre-Glitch architecture animated by Aetheric Blight. These are not creatures - they are support columns and structural elements that have gained hostile agency. Slow but incredibly durable. The Blight has fused with the crystal lattice structure."
    }'
);

-- =====================================================
-- ENEMY 5: All-Rune''s Echo (BOSS)
-- =====================================================
-- Concept: Sentient Reality Glitch at heart of Alfheim
-- Level: 12 (Boss)
-- High resistances across the board
-- Spawn Weight: 0 (Scripted encounter only)
-- =====================================================

INSERT OR IGNORE INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level,
    spawn_weight,
    verticality_tier,
    spawn_rules_json
) VALUES (
    6, 'All-Rune''s Echo', 'Reality_Glitch_Boss',
    12, 12,
    0, -- Scripted encounter only (0% random spawn)
    'Canopy',
    '{
        "hp": 200,
        "accuracy": 85,
        "mitigation": 14,
        "attributes": {
            "MIGHT": 16,
            "FINESSE": 14,
            "STURDINESS": 18,
            "WITS": 20,
            "WILL": 20
        },
        "resistances": {
            "Energy": 80,
            "Physical": 60,
            "Psychic": 75,
            "Fire": 40,
            "Cold": 40
        },
        "vulnerabilities": {},
        "movement_range": 5,
        "action_points": 3,
        "initiative_bonus": 6,
        "phases": [
            {
                "phase": 1,
                "hp_threshold": 100,
                "abilities": [
                    {
                        "name": "Paradox Weave",
                        "ap_cost": 3,
                        "damage_dice": "3d10",
                        "damage_type": "Energy",
                        "range": 5,
                        "cooldown": 2,
                        "description": "AoE attack hitting target and all adjacent. Applies [Reality Distortion] (-2 to all rolls for 2 turns)."
                    },
                    {
                        "name": "Compile Error",
                        "ap_cost": 4,
                        "damage_dice": "4d8",
                        "damage_type": "Psychic",
                        "range": "4 tile AoE",
                        "cooldown": 3,
                        "description": "All Mystics in range lose 10 Aether Pool. All non-Mystics gain +10 Stress."
                    }
                ]
            },
            {
                "phase": 2,
                "hp_threshold": 0,
                "hp_reset": 150,
                "summons": "2x Reality Fragment",
                "abilities": [
                    {
                        "name": "All-Rune Manifestation",
                        "ap_cost": 5,
                        "damage_dice": "5d10",
                        "damage_type": "Mixed (Energy + Psychic)",
                        "range": 6,
                        "cooldown": 4,
                        "description": "Ignores all resistances. Target gains +10 Corruption."
                    },
                    {
                        "name": "System Crash",
                        "ap_cost": 10,
                        "damage_dice": "6d8",
                        "damage_type": "Mixed",
                        "range": "Battlefield",
                        "cooldown": 0,
                        "trigger": "<25% HP",
                        "description": "Ultimate ability. Activates all Reality Tears simultaneously. All characters teleported randomly."
                    }
                ]
            }
        ],
        "loot_drops": [
            {"resource": "Fragment of the All-Rune", "chance": 1.0},
            {"resource": "Reality Anchor Core", "chance": 0.5},
            {"resource": "Crystallized Aether", "chance": 0.8}
        ],
        "tags": ["boss", "reality_glitch", "two_phase", "legendary"],
        "description": "BOSS: Sentient Reality Glitch. The persistent error from compiling the paradoxical All-Rune. Two-phase fight (200 HP → 150 HP). High resistances across the board. Deals mixed Energy/Psychic damage, applies Corruption, teleports party. Ultimate System Crash at <25% HP. Scripted encounter in All-Rune Proving Ground."
    }'
);

COMMIT;

-- =====================================================
-- END v0.31.3 SEEDING SCRIPT
-- =====================================================

-- =====================================================
-- VERIFICATION QUERIES (Run these to verify seeding)
-- =====================================================

-- Test 1: Enemy Spawn Count
-- SELECT COUNT(*) FROM Biome_EnemySpawns WHERE biome_id = 6;
-- Expected: 5

-- Test 2: Spawn Weight Distribution
-- SELECT enemy_name, spawn_weight FROM Biome_EnemySpawns WHERE biome_id = 6 ORDER BY spawn_weight DESC;
-- Expected: Aether-Vulture: 150, Crystalline Construct: 120, Energy Elemental: 100, Forlorn Echo: 60, All-Rune's Echo: 0

-- Test 3: Boss Spawn Weight
-- SELECT enemy_name FROM Biome_EnemySpawns WHERE biome_id = 6 AND spawn_weight = 0;
-- Expected: All-Rune's Echo

-- Test 4: Verticality Tier
-- SELECT enemy_name, verticality_tier FROM Biome_EnemySpawns WHERE biome_id = 6;
-- Expected: All 5 enemies with 'Canopy'

-- Test 5: Elite Count
-- SELECT enemy_name FROM Biome_EnemySpawns WHERE biome_id = 6 AND enemy_type LIKE '%Elite%';
-- Expected: Forlorn Echo

-- Test 6: JSON Validation (check one enemy)
-- SELECT json_valid(spawn_rules_json) FROM Biome_EnemySpawns WHERE biome_id = 6 AND enemy_name = 'Aether-Vulture';
-- Expected: 1 (valid JSON)

-- =====================================================
-- SUCCESS CRITERIA CHECKLIST
-- =====================================================
-- [ ] 5 enemy types seeded for Alfheim
-- [ ] Spawn weights correctly calibrated (total: 430)
-- [ ] All enemies are Canopy-exclusive (verticality_tier)
-- [ ] Boss has spawn_weight = 0 (scripted only)
-- [ ] Elite enemy (Forlorn Echo) has reduced spawn weight
-- [ ] All JSON structures are valid
-- [ ] Energy resistances high across the board
-- [ ] Loot tables reference Alfheim resources
-- [ ] v5.0 voice compliance in descriptions
-- [ ] ASCII-only entity names
-- =====================================================
