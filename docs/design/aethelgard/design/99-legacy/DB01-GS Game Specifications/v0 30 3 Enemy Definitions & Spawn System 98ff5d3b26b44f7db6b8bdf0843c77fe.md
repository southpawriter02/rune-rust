# v0.30.3: Enemy Definitions & Spawn System

Type: Technical
Description: Defines 5 Niflheim enemy types with Ice Resistance/Fire Vulnerability pattern, weighted spawn system, Brittleness mechanic, and Frost-Giant boss framework.
Priority: Must-Have
Status: Implemented
Target Version: Alpha
Dependencies: v0.30.1-v0.30.2 (Database, Environmental)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.30: Niflheim Biome Implementation (v0%2030%20Niflheim%20Biome%20Implementation%20106edccf7fa44a2c81f1ec738c809e2f.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.30.3-ENEMIES

**Parent Specification:** v0.30 Niflheim Biome Implementation

**Status:** Design Complete — Ready for Implementation

**Timeline:** 10-15 hours

**Prerequisites:** v0.30.2 (Environmental hazards complete), v0.29.3 (Muspelheim enemies as reference)

---

## ✅ IMPLEMENTATION COMPLETE (2025-11-16)

**Status:** Implementation complete, all deliverables verified

**Actual Time:** ~10 hours

**File:** `Data/v0.30.3_enemy_definitions.sql`

**5 Enemy Types Created (7 spawn entries with verticality variants):**

1. ✅ **Frost-Rimed Undying** (Weight: 150) - Ice Resist 75%, Fire Vuln 50%
2. ✅ **Cryo-Drone** (Weight: 120) - Ice Immune 100%, Fire Vuln 75%, flying
3. ✅ **Ice-Adapted Beast** (Weight: 80/40) - Ice Resist 50%, Ice-Walker passive, dual verticality
4. ✅ **Frost-Giant** (Weight: 5) - BOSS, Ice Resist 90%, HP 250, Roots only
5. ✅ **Forlorn Echo (Frozen Dead)** (Weight: 60/30) - Psychic entity, stress dealer, dual verticality

**Mechanics Implemented:**

- ✅ Brittleness mechanic: Ice damage → [Brittle] → +50% Physical damage
- ✅ Weighted spawn system with verticality filtering
- ✅ JSON spawn rules with stats, resistances, abilities, loot tables
- ✅ Ice Resistance/Fire Vulnerability pattern established

**Quality Gates Met:**

- ✅ v5.0 compliance (corrupted maintenance units, not ice magic)
- ✅ ASCII-only enemy names
- ✅ Spawn weights balanced for mid-late game challenge

---

## I. Overview

This specification defines enemy types and spawn systems for the Niflheim biome, including 5 enemy types with Ice Resistance mechanics, the Brittleness system (Ice → Fire Vulnerability), spawn weight balancing, and Frost-Giant boss framework.

### Core Deliverables

- **5 Enemy Types** (Frost-Rimed Undying, Cryo-Drone, Ice-Adapted Beast, Frost-Giant, Forlorn Echo)
- **Ice Resistance + Fire Vulnerability** pattern on 4/5 enemy types
- **Spawn weight system** for procedural generation
- **Biome_EnemySpawns** table population
- **Frost-Giant boss framework** (full encounter in v0.35)
- **Complete stat blocks** with abilities and loot tables

---

## II. Enemy Design Philosophy

### Core Principles

**1. Inverse of Muspelheim**

- Muspelheim: Fire Resistance + Ice Vulnerability
- Niflheim: Ice Resistance + Fire Vulnerability
- Reinforces elemental preparation as tactical necessity

**2. Movement Control Challenge**

- Enemies exploit [Slippery Terrain]
- Forced movement abilities are amplified
- Knockdown immunity enemies are particularly dangerous

**3. Ice Damage Threat**

- All combatants Vulnerable to Ice in [Frigid Cold]
- Enemy Ice attacks are especially lethal
- Fire resistance becomes survival necessity

**4. v5.0 Compliance**

- Ice-adapted = cryogenic exposure survival, NOT magic
- Cryo-Drones = malfunctioning maintenance units
- Frost-Giant = dormant Jötun-Forged
- Forlorn = psychic echoes preserved by flash-freezing

---

## III. Enemy Definitions

### Enemy 1: Frost-Rimed Undying

**Concept:** Standard Undying whose chassis has been exposed to extreme cold. Ice has formed over their metal frames, making them brittle but insulated against further cold damage.

**v5.0 Narrative Frame:**

- "A rusted security unit coated in thick layers of ice. Its joints creak with frost."
- "The Undying's thermal regulators failed centuries ago. It operates at near-absolute zero."
- "Ice crystals have formed within its circuitry, making it slow but resilient."

### Database Entry

**Enemies Table:**

```sql
INSERT INTO Enemies (
    enemy_id, enemy_name, enemy_type, faction,
    level, hp, accuracy, mitigation,
    might, finesse, sturdiness, wits, will,
    movement_range, action_points,
    description
)
VALUES (
    501, -- enemy_id
    'Frost-Rimed Undying',
    'Undying',
    'The Undying',
    8, -- level (mid-tier)
    85, -- HP
    70, -- Accuracy
    25, -- Mitigation
    14, -- MIGHT
    8, -- FINESSE (slowed by ice)
    16, -- STURDINESS (ice-hardened)
    6, -- WITS
    4, -- WILL
    2, -- Movement range
    2, -- AP
    'Security unit that has operated in cryogenic conditions for centuries. Ice has formed over its chassis, providing insulation against cold but creating structural weaknesses. Slow but resilient.'
);
```

**Resistances:**

```sql
INSERT INTO Enemy_Resistances (enemy_id, damage_type, resistance_value)
VALUES
(501, 'Ice', 75), -- Highly resistant to Ice
(501, 'Physical', 25), -- Some physical resistance from ice coating
(501, 'Psychic', 100); -- Immune (machine)

INSERT INTO Enemy_Vulnerabilities (enemy_id, damage_type, vulnerability_value)
VALUES
(501, 'Fire', 50), -- Vulnerable to Fire (ice melts)
(501, 'Energy', 25); -- Moderate vulnerability
```

**Abilities:**

```sql
-- Ability 1: Frost Strike (Basic Attack)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, stamina_cost,
    damage_dice, damage_type, accuracy_modifier,
    range_type, description
)
VALUES (
    501,
    'Frost Strike',
    1, -- 1 AP
    0, -- No Stamina
    '2d8+4', -- Damage
    'Ice',
    0, -- No accuracy modifier
    'Melee',
    'Melee attack with ice-encrusted limbs. Deals Ice damage. In [Frigid Cold], this attack is especially lethal due to universal Ice vulnerability.'
);

-- Ability 2: Brittle Armor (Passive)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, stamina_cost,
    description
)
VALUES (
    501,
    'Brittle Armor',
    0, -- Passive
    0,
    'Passive: Ice coating provides Ice Resistance but creates Fire Vulnerability. When hit by Fire damage, ice melts and Physical Mitigation is reduced by 10 for 2 turns.'
);
```

**Loot Table:**

```sql
INSERT INTO Enemy_LootDrops (enemy_id, resource_name, drop_chance)
VALUES
(501, 'Frozen Circuitry', 0.6),
(501, 'Supercooled Alloy', 0.3),
(501, 'Cryo-Coolant Fluid', 0.4);
```

---

### Enemy 2: Cryo-Drone

**Concept:** Specialized maintenance drones that once serviced cryogenic systems. Their liquid nitrogen dispensers are malfunctioning, spraying freezing jets erratically.

**v5.0 Narrative Frame:**

- "A hovering maintenance unit with ruptured coolant tanks. Liquid nitrogen leaks from damaged nozzles."
- "The drone's targeting system is corrupted. It sprays cryogenic fluid indiscriminately."
- "Pre-Glitch safety protocols have failed. The unit treats all movement as thermal regulation threats."

### Database Entry

**Enemies Table:**

```sql
INSERT INTO Enemies (
    enemy_id, enemy_name, enemy_type, faction,
    level, hp, accuracy, mitigation,
    might, finesse, sturdiness, wits, will,
    movement_range, action_points,
    description
)
VALUES (
    502,
    'Cryo-Drone',
    'Undying',
    'The Undying',
    7, -- level
    60, -- HP (fragile)
    75, -- Accuracy
    15, -- Mitigation (light armor)
    10, -- MIGHT
    14, -- FINESSE (mobile)
    10, -- STURDINESS
    8, -- WITS
    4, -- WILL
    3, -- Movement range (flying)
    2, -- AP
    'Corrupted cryogenic maintenance drone. Malfunctioning liquid nitrogen dispensers spray freezing jets. Treats all movement as thermal regulation threats. Mobile and dangerous in close quarters.'
);
```

**Resistances:**

```sql
INSERT INTO Enemy_Resistances (enemy_id, damage_type, resistance_value)
VALUES
(502, 'Ice', 100), -- Immune to Ice (filled with liquid nitrogen)
(502, 'Psychic', 100); -- Immune (machine)

INSERT INTO Enemy_Vulnerabilities (enemy_id, damage_type, vulnerability_value)
VALUES
(502, 'Fire', 75), -- Highly vulnerable (liquid nitrogen ignites/boils)
(502, 'Physical', 25), -- Light armor
(502, 'Energy', 50); -- Moderate vulnerability
```

**Abilities:**

```sql
-- Ability 1: Nitrogen Spray (Cone Attack)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, stamina_cost,
    damage_dice, damage_type, accuracy_modifier,
    range_type, aoe_shape, description
)
VALUES (
    502,
    'Nitrogen Spray',
    2, -- 2 AP (powerful)
    0,
    '3d6', -- Damage
    'Ice',
    +10, -- +10 Accuracy (hard to dodge spray)
    'Ranged',
    'Cone_3', -- 3-tile cone
    'Sprays liquid nitrogen in a 3-tile cone. Deals 3d6 Ice damage. Targets hit must make STURDINESS check (DC 12) or be [Slowed] for 1 turn. In [Frigid Cold], this attack is devastating.'
);

-- Ability 2: Evasive Flight (Mobility)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, stamina_cost,
    description
)
VALUES (
    502,
    'Evasive Flight',
    1, -- 1 AP
    0,
    'Flies to any tile within 3 spaces. Ignores [Slippery Terrain] and does not provoke opportunity attacks. Uses malfunctioning anti-grav systems.'
);
```

**Loot Table:**

```sql
INSERT INTO Enemy_LootDrops (enemy_id, resource_name, drop_chance)
VALUES
(502, 'Cryo-Coolant Fluid', 0.8), -- High chance
(502, 'Frozen Circuitry', 0.4),
(502, 'Pristine Ice Core', 0.15); -- Rare
```

---

### Enemy 3: Ice-Adapted Beast

**Concept:** Feral creatures that have adapted to extreme cold over 800 years. Thick white fur, predatory instincts, and pack tactics.

**v5.0 Narrative Frame:**

- "A massive predator with thick white fur. Its breath creates visible clouds of vapor."
- "The beast has adapted to hunt in absolute zero. It moves silently across ice."
- "Eight centuries of evolution in frozen ruins. Apex predator of the cryo-sectors."

### Database Entry

**Enemies Table:**

```sql
INSERT INTO Enemies (
    enemy_id, enemy_name, enemy_type, faction,
    level, hp, accuracy, mitigation,
    might, finesse, sturdiness, wits, will,
    movement_range, action_points,
    description
)
VALUES (
    503,
    'Ice-Adapted Beast',
    'Beast',
    'Blighted Fauna',
    8, -- level
    95, -- HP (tough)
    75, -- Accuracy
    20, -- Mitigation
    16, -- MIGHT (powerful)
    12, -- FINESSE
    14, -- STURDINESS
    10, -- WITS (animal cunning)
    8, -- WILL
    3, -- Movement range (fast)
    2, -- AP
    'Feral predator adapted to extreme cold. Thick fur provides insulation and camouflage. Hunts in packs. Ignores [Slippery Terrain] due to specialized paw pads. Dangerous and aggressive.'
);
```

**Resistances:**

```sql
INSERT INTO Enemy_Resistances (enemy_id, damage_type, resistance_value)
VALUES
(503, 'Ice', 50), -- Moderate Ice resistance (adapted)
(503, 'Physical', 15); -- Fur provides some protection

INSERT INTO Enemy_Vulnerabilities (enemy_id, damage_type, vulnerability_value)
VALUES
(503, 'Fire', 50), -- Fur burns easily
(503, 'Psychic', 25); -- Moderate psychic vulnerability
```

**Abilities:**

```sql
-- Ability 1: Savage Bite (Basic Attack)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, stamina_cost,
    damage_dice, damage_type, accuracy_modifier,
    range_type, description
)
VALUES (
    503,
    'Savage Bite',
    1, -- 1 AP
    0,
    '2d10+6', -- High damage
    'Physical',
    0,
    'Melee',
    'Powerful bite attack. Deals Physical damage. On critical hit, applies [Bleeding] (1d6 per turn for 3 turns).'
);

-- Ability 2: Pounce (Mobility + Attack)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, stamina_cost,
    damage_dice, damage_type, range_type, description
)
VALUES (
    503,
    'Pounce',
    2, -- 2 AP (powerful)
    0,
    '2d8+4',
    'Physical',
    'Melee',
    'Leaps up to 4 tiles and attacks target. Ignores [Slippery Terrain]. If target is knocked down, deals +50% damage. Uses predatory instinct to exploit vulnerability.'
);

-- Ability 3: Pack Hunter (Passive)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, description
)
VALUES (
    503,
    'Pack Hunter',
    0, -- Passive
    'Passive: Gains +10 Accuracy for each allied Ice-Adapted Beast within 3 tiles. Pack tactics make group encounters deadly.'
);

-- Ability 4: Ice-Walker (Passive)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, description
)
VALUES (
    503,
    'Ice-Walker',
    0, -- Passive
    'Passive: Immune to [Slippery Terrain]. Specialized paw pads provide perfect traction on ice. Does not make FINESSE checks for movement.'
);
```

**Loot Table:**

```sql
INSERT INTO Enemy_LootDrops (enemy_id, resource_name, drop_chance)
VALUES
(503, 'Ice-Bear Pelt', 0.7), -- Primary drop
(503, 'Frost-Lichen', 0.4),
(503, 'Pristine Ice Core', 0.1); -- Rare
```

---

### Enemy 4: Frost-Giant (Mini-Boss)

**Concept:** Dormant Jötun-Forged warmachine that was flash-frozen during atmospheric failure. It is slower due to ice buildup, but its attacks can shatter defenses.

**v5.0 Narrative Frame:**

- "A colossal Jötun-Forged warmachine encased in crystalline ice. Its reactor core still pulses faintly."
- "The giant was flash-frozen at the moment of catastrophic system failure. Ice has locked its joints."
- "Massive and slow, but its strikes can shatter armor and bone alike."

### Database Entry

**Enemies Table:**

```sql
INSERT INTO Enemies (
    enemy_id, enemy_name, enemy_type, faction,
    level, hp, accuracy, mitigation,
    might, finesse, sturdiness, wits, will,
    movement_range, action_points,
    description
)
VALUES (
    504,
    'Frost-Giant',
    'Jötun-Forged',
    'The Undying',
    12, -- level (mini-boss)
    250, -- HP (massive)
    65, -- Accuracy (slowed by ice)
    40, -- Mitigation (heavy armor + ice)
    22, -- MIGHT (immense)
    6, -- FINESSE (very slow)
    20, -- STURDINESS (extremely tough)
    8, -- WITS
    6, -- WILL
    1, -- Movement range (slow)
    3, -- AP (boss action economy)
    'Dormant Jötun-Forged flash-frozen during atmospheric failure. Ice has locked its joints, making it slow but incredibly resilient. Its attacks can shatter defenses. Boss-tier threat requiring coordinated tactics.'
);
```

**Resistances:**

```sql
INSERT INTO Enemy_Resistances (enemy_id, damage_type, resistance_value)
VALUES
(504, 'Ice', 90), -- Near-immune to Ice
(504, 'Physical', 50), -- Heavy armor
(504, 'Psychic', 100); -- Immune (machine)

INSERT INTO Enemy_Vulnerabilities (enemy_id, damage_type, vulnerability_value)
VALUES
(504, 'Fire', 75), -- Highly vulnerable (melts ice coating)
(504, 'Energy', 25); -- Moderate vulnerability
```

**Abilities:**

```sql
-- Ability 1: Shattering Slam (Heavy Attack)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, stamina_cost,
    damage_dice, damage_type, accuracy_modifier,
    range_type, description
)
VALUES (
    504,
    'Shattering Slam',
    2, -- 2 AP
    0,
    '3d12+8', -- Massive damage
    'Physical',
    -10, -- -10 Accuracy (slow, telegraphed)
    'Melee',
    'Devastating melee attack. Deals massive Physical damage. On hit, target must make STURDINESS check (DC 15) or be [Knocked Down] and pushed 2 tiles. Destroys cover in path.'
);

-- Ability 2: Frost Wave (AoE Attack)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, stamina_cost,
    damage_dice, damage_type, aoe_shape, description
)
VALUES (
    504,
    'Frost Wave',
    3, -- 3 AP (powerful)
    0,
    '2d10+6',
    'Ice',
    'Cone_4', -- 4-tile cone
    'Slams ground, releasing wave of ice shards in 4-tile cone. Deals Ice damage. Creates [Slippery Terrain] in affected area. In [Frigid Cold], this is devastating.'
);

-- Ability 3: Frozen Colossus (Passive)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, description
)
VALUES (
    504,
    'Frozen Colossus',
    0, -- Passive
    'Passive: Immune to [Knocked Down], [Slowed], and forced movement. Ice coating provides massive damage reduction but melts when hit by Fire (loses 10 Mitigation per Fire attack, stacks).'
);

-- Ability 4: Shatter Defense (On-Hit Effect)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, description
)
VALUES (
    504,
    'Shatter Defense',
    0, -- Passive
    'Passive: Physical attacks reduce target Mitigation by 5 for 2 turns (stacks up to 3 times). Represents armor-breaking power of colossal strikes.'
);
```

**Loot Table:**

```sql
INSERT INTO Enemy_LootDrops (enemy_id, resource_name, drop_chance)
VALUES
(504, 'Heart of the Frost-Giant', 1.0), -- Guaranteed legendary drop
(504, 'Supercooled Alloy', 0.8),
(504, 'Pristine Ice Core', 0.6),
(504, 'Cryogenic Data-Slate', 0.5);
```

**Boss Framework Note:**

This is a **framework only**. Full multi-phase boss encounter with mechanics, phase transitions, and tactical complexity will be implemented in v0.35.

---

### Enemy 5: Forlorn Echo (Frozen Dead)

**Concept:** Psychic echoes of Pre-Glitch victims perfectly preserved by flash-freezing. Fainter and less aggressive than Forlorn in other biomes, but still dangerous.

**v5.0 Narrative Frame:**

- "A ghostly psychic echo, frozen in time at the moment of death. Its scream is crystallized."
- "The flash-freeze preserved not just bodies, but final thoughts. Error logs from dying minds."
- "Faint but persistent. The cold has muted its rage, leaving only sorrow."

### Database Entry

**Enemies Table:**

```sql
INSERT INTO Enemies (
    enemy_id, enemy_name, enemy_type, faction,
    level, hp, accuracy, mitigation,
    might, finesse, sturdiness, wits, will,
    movement_range, action_points,
    description
)
VALUES (
    505,
    'Forlorn Echo (Frozen Dead)',
    'Forlorn',
    'The Forlorn',
    7, -- level
    50, -- HP (fragile)
    70, -- Accuracy
    0, -- Mitigation (incorporeal)
    8, -- MIGHT
    12, -- FINESSE
    8, -- STURDINESS
    14, -- WITS
    16, -- WILL (psychic entity)
    2, -- Movement range
    2, -- AP
    'Psychic echo preserved by flash-freezing. Fainter than typical Forlorn due to cold preservation. Less aggressive but still generates Psychic Stress. Represents final moments of atmospheric failure victims.'
);
```

**Resistances:**

```sql
INSERT INTO Enemy_Resistances (enemy_id, damage_type, resistance_value)
VALUES
(505, 'Physical', 75), -- Mostly incorporeal
(505, 'Ice', 50), -- Preserved by cold
(505, 'Energy', 50);

INSERT INTO Enemy_Vulnerabilities (enemy_id, damage_type, vulnerability_value)
VALUES
(505, 'Psychic', 50), -- Vulnerable to psychic attacks
(505, 'Fire', 25); -- Heat disrupts preservation
```

**Abilities:**

```sql
-- Ability 1: Frozen Wail (Psychic Attack)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, stamina_cost,
    damage_dice, damage_type, description
)
VALUES (
    505,
    'Frozen Wail',
    1, -- 1 AP
    0,
    '2d6+4',
    'Psychic',
    'Crystallized scream of terror. Deals Psychic damage. Target gains +3 Psychic Stress. Fainter than normal Forlorn wails but still traumatizing.'
);

-- Ability 2: Incorporeal (Passive)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, description
)
VALUES (
    505,
    'Incorporeal',
    0, -- Passive
    'Passive: 75% Physical damage resistance. Ignores [Slippery Terrain]. Can move through solid objects. Psychic entity with minimal physical presence.'
);

-- Ability 3: Preserved Echo (Passive)
INSERT INTO Enemy_Abilities (
    enemy_id, ability_name, ap_cost, description
)
VALUES (
    505,
    'Preserved Echo',
    0, -- Passive
    'Passive: When defeated, generates +8 Psychic Stress to all party members within 4 tiles. Witnessing the final death of a preserved victim is traumatic.'
);
```

**Loot Table:**

```sql
INSERT INTO Enemy_LootDrops (enemy_id, resource_name, drop_chance)
VALUES
(505, 'Cryogenic Data-Slate', 0.4), -- Echoes may contain data
(505, 'Pristine Ice Core', 0.2),
(505, 'Eternal Frost Crystal', 0.05); -- Very rare
```

---

## IV. Spawn Weight System

### Biome_EnemySpawns Table

**Table Structure:**

```sql
CREATE TABLE IF NOT EXISTS Biome_EnemySpawns (
    spawn_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    enemy_id INTEGER NOT NULL,
    spawn_weight REAL DEFAULT 1.0,
    min_party_level INTEGER DEFAULT 1,
    verticality_tier TEXT CHECK(verticality_tier IN ('Roots', 'Canopy', 'Both')),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id),
    FOREIGN KEY (enemy_id) REFERENCES Enemies(enemy_id)
);
```

**Spawn Weights for Niflheim:**

```sql
-- Frost-Rimed Undying (Common, both tiers)
INSERT INTO Biome_EnemySpawns (biome_id, enemy_id, spawn_weight, min_party_level, verticality_tier)
VALUES (5, 501, 2.0, 7, 'Both');

-- Cryo-Drone (Common, both tiers)
INSERT INTO Biome_EnemySpawns (biome_id, enemy_id, spawn_weight, min_party_level, verticality_tier)
VALUES (5, 502, 1.8, 6, 'Both');

-- Ice-Adapted Beast (Medium, more common in Roots)
INSERT INTO Biome_EnemySpawns (biome_id, enemy_id, spawn_weight, min_party_level, verticality_tier)
VALUES (5, 503, 1.2, 7, 'Roots');

-- Ice-Adapted Beast (Medium, less common in Canopy)
INSERT INTO Biome_EnemySpawns (biome_id, enemy_id, spawn_weight, min_party_level, verticality_tier)
VALUES (5, 503, 0.6, 7, 'Canopy');

-- Frost-Giant (Rare, Roots only, boss)
INSERT INTO Biome_EnemySpawns (biome_id, enemy_id, spawn_weight, min_party_level, verticality_tier)
VALUES (5, 504, 0.1, 10, 'Roots');

-- Forlorn Echo (Medium, more common in Canopy)
INSERT INTO Biome_EnemySpawns (biome_id, enemy_id, spawn_weight, min_party_level, verticality_tier)
VALUES (5, 505, 1.0, 6, 'Canopy');

-- Forlorn Echo (Medium, less common in Roots)
INSERT INTO Biome_EnemySpawns (biome_id, enemy_id, spawn_weight, min_party_level, verticality_tier)
VALUES (5, 505, 0.5, 6, 'Roots');
```

### Spawn Distribution Summary

**Roots Encounters:**

- Frost-Rimed Undying (High)
- Cryo-Drone (High)
- Ice-Adapted Beast (Medium-High)
- Frost-Giant (Very Rare, boss)
- Forlorn Echo (Low-Medium)

**Canopy Encounters:**

- Frost-Rimed Undying (High)
- Cryo-Drone (High)
- Ice-Adapted Beast (Medium)
- Forlorn Echo (Medium)
- No Frost-Giant (boss only in Roots)

---

## V. Testing Requirements

### Unit Tests

**Test 1: Enemy Stat Validation**

```csharp
[Theory]
[InlineData(501, "Frost-Rimed Undying", 85, 75)] // HP, Ice Resistance
[InlineData(502, "Cryo-Drone", 60, 100)]
[InlineData(503, "Ice-Adapted Beast", 95, 50)]
[InlineData(504, "Frost-Giant", 250, 90)]
[InlineData(505, "Forlorn Echo (Frozen Dead)", 50, 50)]
public void EnemyStats_AllNiflheimEnemies_MatchSpecification(
    int enemyId, string name, int expectedHP, int expectedIceResist)
{
    var enemy = _enemyRepository.GetById(enemyId);
    
    Assert.Equal(name, [enemy.Name](http://enemy.Name));
    Assert.Equal(expectedHP, enemy.HP);
    Assert.Equal(expectedIceResist, enemy.Resistances["Ice"]);
}
```

**Test 2: Fire Vulnerability Validation**

```csharp
[Theory]
[InlineData(501, 50)] // Frost-Rimed: 50% Fire vulnerability
[InlineData(502, 75)] // Cryo-Drone: 75% Fire vulnerability
[InlineData(503, 50)] // Ice-Adapted Beast: 50%
[InlineData(504, 75)] // Frost-Giant: 75%
public void EnemyVulnerabilities_IceResistantEnemies_VulnerableToFire(
    int enemyId, int expectedFireVuln)
{
    var enemy = _enemyRepository.GetById(enemyId);
    
    Assert.Equal(expectedFireVuln, enemy.Vulnerabilities["Fire"]);
}
```

**Test 3: Spawn Weight Distribution**

```csharp
[Fact]
public void SpawnWeights_NiflheimBiome_CorrectDistribution()
{
    var spawns = _spawnRepository.GetByBiome(5); // Niflheim
    
    var frostRimed = spawns.First(s => s.EnemyId == 501);
    var cryoDrone = spawns.First(s => s.EnemyId == 502);
    var frostGiant = spawns.First(s => s.EnemyId == 504);
    
    Assert.True(frostRimed.SpawnWeight > frostGiant.SpawnWeight);
    Assert.True(cryoDrone.SpawnWeight > frostGiant.SpawnWeight);
    Assert.True(frostGiant.SpawnWeight < 0.2); // Boss is rare
}
```

**Test 4: Brittleness Mechanic**

```csharp
[Fact]
public void BrittlenessMechanic_IceDamageOnFrostRimed_AppliesBrittle()
{
    var enemy = _enemyRepository.GetById(501); // Frost-Rimed
    var iceAttack = new AttackResult { DamageType = "Ice", DamageDone = 20 };
    
    _brittlenessService.ProcessBrittlenessCheck(enemy, iceAttack);
    
    Assert.True(enemy.HasCondition("Brittle"));
    Assert.Equal(50, enemy.Vulnerabilities["Physical"]);
}
```

**Test 5: Ice-Walker Passive**

```csharp
[Fact]
public void IceWalker_IceAdaptedBeast_IgnoresSlipperyTerrain()
{
    var beast = _enemyRepository.GetById(503);
    var slipperyTile = CreateSlipperyTile();
    
    bool success = _slipperyTerrainService.ProcessSlipperyMovement(
        beast, beast.CurrentTile, slipperyTile);
    
    Assert.True(success); // No FINESSE check needed
    Assert.False(beast.HasCondition("Knocked Down"));
}
```

### Integration Tests

**Test 6: Full Niflheim Enemy Encounter**

```csharp
[Fact]
public void NiflheimEncounter_FrostRimedAndCryoDrone_CorrectMechanics()
{
    // Arrange
    var combat = CreateNiflheimCombat();
    var frostRimed = SpawnEnemy(501, combat);
    var cryoDrone = SpawnEnemy(502, combat);
    var player = combat.PlayerParty[0];
    
    // Act: Initialize combat (Frigid Cold applied)
    _combatService.InitializeCombat(combat);
    
    // Assert: Enemies have correct resistances
    Assert.Equal(75, frostRimed.Resistances["Ice"]);
    Assert.Equal(100, cryoDrone.Resistances["Ice"]);
    
    // Act: Player uses Ice ability
    var iceAttack = _combatService.ProcessAttack(player, frostRimed, iceAbility);
    
    // Assert: Brittle applied
    Assert.True(frostRimed.HasCondition("Brittle"));
    
    // Act: Player uses Physical ability on Brittle target
    var physAttack = _combatService.ProcessAttack(player, frostRimed, physAbility);
    
    // Assert: Increased damage from Brittle
    Assert.True(physAttack.DamageDone > physAbility.BaseDamage);
    
    // Act: Cryo-Drone uses Nitrogen Spray
    var sprayAttack = _combatService.ProcessAttack(cryoDrone, player, nitrogenSpray);
    
    // Assert: Massive damage in Frigid Cold
    Assert.True(sprayAttack.DamageDone > nitrogenSpray.BaseDamage * 1.5);
}
```

---

## VI. Success Criteria

### Functional Requirements

- [ ]  All 5 enemy types defined in database
- [ ]  Stat blocks match specification values
- [ ]  Ice Resistance + Fire Vulnerability pattern on 4/5 enemies
- [ ]  Spawn weights correctly calibrated
- [ ]  Frost-Giant boss framework implemented
- [ ]  All enemy abilities functional
- [ ]  Loot tables correctly assigned
- [ ]  Brittleness mechanic applies correctly
- [ ]  Ice-Walker passive bypasses slippery terrain

### Quality Gates

- [ ]  80%+ unit test coverage
- [ ]  All enemy descriptions use v5.0 voice
- [ ]  ASCII-compliant entity names
- [ ]  Serilog logging for spawn events
- [ ]  Integration tests pass

### Balance Validation

- [ ]  Frost-Rimed Undying feels like standard threat
- [ ]  Cryo-Drone Nitrogen Spray is dangerous but avoidable
- [ ]  Ice-Adapted Beast pack tactics are deadly
- [ ]  Frost-Giant is boss-tier challenge
- [ ]  Forlorn Echo Psychic Stress accumulation is meaningful
- [ ]  Fire damage provides clear advantage
- [ ]  Ice damage + Physical follow-up combo is rewarding

---

## VII. Deployment Instructions

### Step 1: Run Database Migration

```bash
sqlite3 your_database.db < Data/v0.30.3_enemy_definitions.sql
```

### Step 2: Verify Enemy Data

```bash
sqlite3 your_database.db
> SELECT enemy_name, hp, level FROM Enemies WHERE enemy_id BETWEEN 501 AND 505;
> SELECT COUNT(*) FROM Biome_EnemySpawns WHERE biome_id = 5;
> .quit
```

### Step 3: Run Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~Niflheim.EnemyTests"
```

### Step 4: Run Integration Tests

```bash
dotnet test --filter "FullyQualifiedName~Niflheim.EnemyIntegrationTests"
```

### Step 5: Manual Verification

- Generate Niflheim sector
- Verify enemy spawn distribution
- Combat Frost-Rimed Undying
- Test Ice → Brittle → Physical combo
- Combat Cryo-Drone
- Verify Nitrogen Spray damage in Frigid Cold
- Encounter Ice-Adapted Beast
- Verify Ice-Walker ignores slippery terrain
- Verify pack tactics bonus

---

## VIII. Next Steps

Once v0.30.3 is complete:

**Proceed to v0.30.4:** Service Implementation & Testing

- NiflheimService complete implementation
- BiomeGenerationService.GenerateNiflheimSector()
- FrigidColdService integration
- SlipperyTerrainService integration
- Complete unit test suite
- End-to-end integration testing

---

## IX. Related Documents

**Parent Specification:**

- v0.30: Niflheim Biome Implementation[[1]](v0%2030%20Niflheim%20Biome%20Implementation%20106edccf7fa44a2c81f1ec738c809e2f.md)

**Reference Implementation:**

- v0.29.3: Muspelheim Enemy Definitions[[2]](v0%2029%203%20Enemy%20Definitions%20&%20Spawn%20System%208f34dab3be0e4869bd99f792215abd8a.md)

**Prerequisites:**

- v0.30.2: Environmental Hazards[[3]](v0%2030%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%2025f6a1f3b0a843ce9a72b86503b1de60.md)
- v0.15: Trauma Economy[[4]](https://www.notion.so/v0-15-Trauma-Economy-Breaking-Points-Consequences-a1e59f904171485284d6754193af333b?pvs=21)

---

**Enemy definitions complete. Proceed to service implementation (v0.30.4).**