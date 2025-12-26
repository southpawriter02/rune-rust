# v0.29.3: Enemy Definitions & Spawn System

Type: Technical
Description: Defines 5 Muspelheim enemy types with Forge-Hardened/Brittleness mechanics, Biome_EnemySpawns table, spawn weight system, and Surtur's Herald boss framework.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.29.1 (Database Schema), v0.29.2 (Environmental Hazards)
Implementation Difficulty: Hard
Balance Validated: No
Parent item: v0.29: Muspelheim Biome Implementation (v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Document ID:** RR-SPEC-v0.29.3-ENEMIES

**Status:** Design Complete — Ready for Phase 2 (Database Implementation)

**Timeline:** 10-15 hours

**Prerequisites:** v0.29.1 (Database Schema), v0.29.2 (Environmental Hazards)

**Parent Spec:** v0.29: Muspelheim Biome Implementation[[1]](v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)

---

## I. Executive Summary

This specification defines **enemy types and spawn systems** for the Muspelheim biome, including 5 enemy types with Fire Resistance mechanics, the Brittleness system (Fire Resistance + Ice Vulnerability → [Brittle] debuff), and weighted spawn tables integrated with room templates.

### Scope

**5 Enemy Types:**

1. **Forge-Hardened Undying** - Fire Resistant (75%), Ice Vulnerable (50%), standard melee
2. **Magma Elemental** - Fire Immune (100%), leaves burning trail, ranged fire attacks
3. **Rival Berserker** - Fury-driven, Fire adapted (50% resist), aggressive melee
4. **Surtur's Herald** - Boss framework (multi-phase encounter in v0.35)
5. **Iron-Bane Crusader** - Potential ally, zealous purifier, Fire Resistant (60%)

**Brittleness Mechanic:**

- **[Brittle] Debuff** - Ice damage → Fire Resistant enemies → Physical Vulnerability (+50% Physical damage taken)
- Tactical combo: Ice Mystic + Physical Warrior synergy

**Technical Deliverables:**

- Biome_EnemySpawns table seeding (spawn weights, level ranges)
- Enemy stat blocks (HP, damage, resistances)
- BrittlenessService implementation
- SpawnService integration with room templates
- Unit test suite (~85% coverage)

---

## II. The Rule: What's In vs. Out

### ✅ In Scope (v0.29.3)

**Enemy Definitions:**

- Complete stat blocks (HP, damage, armor, resistances)
- Fire Resistance percentages (50%-100%)
- Ice Vulnerability percentages (30%-50%)
- Level ranges (7-12) matching biome tier
- Special abilities (burning trail, Fury generation)

**Brittleness Mechanic:**

- [Brittle] debuff definition in database
- BrittlenessService.TryApplyBrittle()
- Physical Vulnerability application (+50% damage)
- Duration and stacking rules

**Spawn System:**

- Biome_EnemySpawns table with weighted spawns
- Room template integration (enemy_spawn_weight)
- Level-appropriate encounters
- Boss spawn conditions (Containment Breach Zone)

**Quality:**

- v5.0 setting compliance (corrupted constructs, not demons)
- ASCII-only entity names
- Serilog structured logging
- 80%+ unit test coverage

### ❌ Explicitly Out of Scope

- Surtur's Herald **full multi-phase encounter** (v0.35)
- Advanced AI behaviors (v0.36)
- Loot drop tables (handled by v0.29.1 resources)
- Environmental Combat specific interactions (v0.29.4)
- Procedural generation algorithms (v0.29.4)
- Encounter balancing tuning (post-v0.29 playtesting)

---

## III. Enemy Type Definitions

### Enemy 1: Forge-Hardened Undying

**Concept:** Standard melee Undying that has adapted to extreme heat through exposure. Fire Resistant but Ice Vulnerable.

**v5.0 Explanation:** Undying whose flesh has been seared and hardened by prolonged exposure to geothermal heat. Their charred tissue is resistant to fire but brittle when frozen.

**Stat Block:**

```yaml
Name: Forge-Hardened Undying
Type: Undying (Melee)
Level Range: 7-9
HP: 65-85
Armor: 3
Speed: 25 ft

Attributes:
  MIGHT: 3
  FINESSE: 1
  WITS: 1
  WILL: 1
  STURDINESS: 4

Resistances:
  Fire: 75%
  Ice: -50% (Vulnerable)

Attacks:
  - Seared Fist (Melee)
    Damage: 2d6+3 Physical
    Range: Melee
    To-Hit: +5
  
  - Ember Grasp (Special, Recharge 5-6)
    Damage: 1d8 Physical + 1d6 Fire
    Range: Melee
    To-Hit: +5
    Effect: Target must succeed STURDINESS DC 13 or catch fire (1d4 Fire/turn for 3 turns)

Special:
  - Forge-Hardened: Fire Resistance 75%
  - Brittle When Frozen: Ice damage applies [Brittle] debuff
  - Undying Resilience: Cannot be frightened or charmed
  - Heat Adapted: Immune to [Intense Heat] ambient condition
```

**Tactical Role:**

- **Frontline Tank:** High HP and armor
- **Fire Resistant Wall:** Absorbs Fire damage meant for squishier allies
- **Ice Weakness:** Vulnerable to Ice Mystic + Physical Warrior combo
- **Blocking:** Occupies chokepoints, forces positioning challenges

**Brittleness Interaction:**

- Ice attack (e.g., Frost Bolt) → [Brittle] applied for 2 turns
- Physical attacks deal +50% damage while [Brittle] active
- Example: 2d6+3 (avg 10) becomes 15 damage against [Brittle] target

**Database Definition:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id,
    enemy_name,
    enemy_type,
    min_level,
    max_level,
    base_hp,
    armor,
    move_speed,
    might, finesse, wits, will, sturdiness,
    fire_resistance,
    ice_resistance,
    spawn_weight,
    special_tags
) VALUES (
    4,
    'Forge-Hardened Undying',
    'Undying',
    7, 9,
    75, 3, 25,
    3, 1, 1, 1, 4,
    75, -50,
    150, -- Most common enemy
    'brittle_on_ice,heat_immune'
);
```

---

### Enemy 2: Magma Elemental

**Concept:** Corrupted geological monitoring construct that fell into magma. Fire Immune, leaves burning trail.

**v5.0 Explanation:** NOT a supernatural fire being. This is a **corrupted Pre-Glitch monitoring drone** that toppled into molten slag during the containment failure. Its chassis superheated and fused with volcanic material, creating a lumbering, semi-molten automaton that radiates extreme heat.

**Stat Block:**

```yaml
Name: Magma Elemental
Type: Corrupted Construct (Ranged)
Level Range: 8-11
HP: 50-70
Armor: 2
Speed: 20 ft (slow)

Attributes:
  MIGHT: 2
  FINESSE: 1
  WITS: 2
  WILL: 0
  STURDINESS: 5

Resistances:
  Fire: 100% (Immune)
  Ice: -30% (Vulnerable)
  Physical: 25% (Hardened chassis)

Attacks:
  - Molten Sling (Ranged)
    Damage: 2d8 Fire
    Range: 60 ft
    To-Hit: +4
    Effect: Leaves [Burning Ground] on impact tile
  
  - Slag Wave (AoE, Recharge 5-6)
    Damage: 3d6 Fire (3-tile cone)
    Range: 15 ft cone
    To-Hit: STURDINESS save DC 14
    Effect: Creates [Burning Ground] in cone area

Special:
  - Burning Trail: Leaves [Burning Ground] on tiles it moves through
  - Fire Immunity: 100% Fire Resistance, heals from Fire damage
  - Molten Core: Explodes on death (2d6 Fire, 2-tile radius)
  - Heat Immune: Immune to [Intense Heat] ambient condition
  - Construct: Immune to Psychic damage, poison, disease
```

**Tactical Role:**

- **Area Denial:** Creates [Burning Ground] hazards
- **Ranged Damage:** Forces party to spread out
- **Fire Immune:** Cannot be damaged by Fire (heals instead)
- **Ice Priority Target:** Vulnerable to Ice damage
- **Death Explosion:** Dangerous to melee attackers

**Brittleness Interaction:**

- Fire Immunity 100% → [Brittle] DOES apply from Ice damage
- However, Physical Vulnerability less impactful due to 25% Physical Resistance
- Net effect: +37.5% Physical damage vs. normal (combines resistances)

**Database Definition:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id,
    enemy_name,
    enemy_type,
    min_level,
    max_level,
    base_hp,
    armor,
    move_speed,
    might, finesse, wits, will, sturdiness,
    fire_resistance,
    ice_resistance,
    physical_resistance,
    spawn_weight,
    special_tags
) VALUES (
    4,
    'Magma Elemental',
    'Construct',
    8, 11,
    60, 2, 20,
    2, 1, 2, 0, 5,
    100, -30, 25,
    80, -- Common
    'burning_trail,death_explosion,brittle_on_ice,heat_immune,construct'
);
```

---

### Enemy 3: Rival Berserker

**Concept:** Human/Undying warrior from rival faction, adapted to heat, Fury-driven aggression.

**v5.0 Explanation:** Skar-Horde or independent survivor who has staked claim to Muspelheim's resources. Wears heat-resistant gear and fights with reckless abandon. Fire adapted but not immune.

**Stat Block:**

```yaml
Name: Rival Berserker
Type: Humanoid (Melee, Elite)
Level Range: 9-12
HP: 90-120
Armor: 4
Speed: 30 ft

Attributes:
  MIGHT: 4
  FINESSE: 2
  WITS: 2
  WILL: 2
  STURDINESS: 3

Resistances:
  Fire: 50%
  Ice: -25% (Vulnerable)

Attacks:
  - Greataxe Swing (Melee)
    Damage: 2d10+4 Physical
    Range: Melee
    To-Hit: +7
  
  - Reckless Strike (Special)
    Damage: 3d10+6 Physical
    Range: Melee
    To-Hit: +7 (with disadvantage on self-defense)
    Effect: Berserker takes +50% damage until next turn
  
  - Fury Shout (AoE, 1/encounter)
    Range: 4-tile radius
    Effect: All allies gain +2 to-hit and +10 ft movement for 3 turns

Fury Resource:
  - Starts with 0 Fury, max 100
  - Gains 15 Fury on hit, 25 Fury on crit
  - Gains 10 Fury when damaged
  - Spends 50 Fury for Reckless Strike

Special:
  - Fury-Driven: Gains bonuses as Fury increases
  - Heat Resistant: 50% Fire Resistance from gear
  - Aggressive: Prioritizes high-threat targets
  - Heat Adapted: Passes [Intense Heat] checks with advantage
```

**Tactical Role:**

- **Elite Melee Threat:** High damage, high HP
- **Fury Mechanic:** Gets stronger as fight progresses
- **Reckless Risk/Reward:** Can be exploited by timing attacks
- **Party Coordination:** Requires focus fire or crowd control
- **Ice Weakness:** Vulnerable to Ice + [Brittle] combo

**Brittleness Interaction:**

- Fire Resistance 50% → [Brittle] applies from Ice damage
- Physical Vulnerability +50% makes Reckless Strike devastating to them
- Counter-play: Freeze them when they use Reckless Strike (vulnerable to both)

**Database Definition:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id,
    enemy_name,
    enemy_type,
    min_level,
    max_level,
    base_hp,
    armor,
    move_speed,
    might, finesse, wits, will, sturdiness,
    fire_resistance,
    ice_resistance,
    spawn_weight,
    special_tags
) VALUES (
    4,
    'Rival Berserker',
    'Humanoid',
    9, 12,
    105, 4, 30,
    4, 2, 2, 2, 3,
    50, -25,
    60, -- Uncommon (elite)
    'fury_resource,brittle_on_ice,heat_adapted'
);
```

---

### Enemy 4: Surtur's Herald (Boss Framework)

**Concept:** Jötun-Forged warmachine boss. Framework only (full multi-phase encounter in v0.35).

**v5.0 Explanation:** Massive Pre-Glitch military automaton named after Norse mythology. "Surtur" was likely the designation of a whole model line. This Herald-class unit was stationed at the geothermal plant as a guard and has remained active for 800 years, now corrupted and territorial.

**Stat Block (Framework):**

```yaml
Name: Surtur's Herald
Type: Jotun-Forged Warmachine (Boss)
Level: 12 (Boss)
HP: 500
Armor: 8
Speed: 35 ft

Attributes:
  MIGHT: 6
  FINESSE: 3
  WITS: 4
  WILL: 5
  STURDINESS: 6

Resistances:
  Fire: 90%
  Ice: -40% (Vulnerable)
  Physical: 50% (Heavy armor plating)
  Lightning: 50% (Conductive chassis)

Attacks:
  - (Framework only; full encounter in v0.35)
  - Thermal Lance (Melee)
  - Molten Barrage (Ranged AoE)
  - Heat Surge (Environmental hazard activation)

Phases:
  - Phase 1: 100%-66% HP (standard combat)
  - Phase 2: 66%-33% HP (activates all steam vents, summons adds)
  - Phase 3: 33%-0% HP (enrage, meltdown countdown)

Special:
  - Boss: Immune to crowd control
  - Legendary Resistances: 3/encounter
  - Heat Mastery: Controls [Burning Ground] placement
  - Brittle Vulnerable: [Brittle] applies from Ice damage
  - Heat Immune: Immune to [Intense Heat] ambient condition
```

**Tactical Role:**

- **Boss Encounter:** Requires full party coordination
- **Multi-Phase:** Escalates in difficulty
- **Environmental Control:** Activates hazards dynamically
- **Ice Weakness:** Primary strategy is Ice + Physical combo
- **Legendary Loot:** Drops Surtur Engine Core (Tier 5)

**Database Definition:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id,
    enemy_name,
    enemy_type,
    min_level,
    max_level,
    base_hp,
    armor,
    move_speed,
    might, finesse, wits, will, sturdiness,
    fire_resistance,
    ice_resistance,
    physical_resistance,
    lightning_resistance,
    spawn_weight,
    special_tags
) VALUES (
    4,
    'Surtur''s Herald',
    'Boss',
    12, 12,
    500, 8, 35,
    6, 3, 4, 5, 6,
    90, -40, 50, 50,
    1, -- Ultra-rare (boss)
    'boss,legendary_resistances,brittle_on_ice,heat_immune,multi_phase'
);
```

**Note:** Full encounter implementation deferred to v0.35. This entry provides spawn framework only.

---

### Enemy 5: Iron-Bane Crusader

**Concept:** Potential ally, zealous purifier from Iron-Bane specialization, hunting corrupted constructs.

**v5.0 Explanation:** Independent Iron-Bane Warrior (Zealous Purifier archetype) on a mission to destroy Magma Elementals and recover Pre-Glitch technology. May be hostile, neutral, or allied depending on party composition and dialogue choices.

**Stat Block:**

```yaml
Name: Iron-Bane Crusader
Type: Humanoid (Melee, Elite)
Level Range: 10-12
HP: 100-130
Armor: 6
Speed: 30 ft

Attributes:
  MIGHT: 4
  FINESSE: 2
  WITS: 3
  WILL: 4
  STURDINESS: 4

Resistances:
  Fire: 60% (Heavy armor + training)
  Ice: 0%
  Corruption: 75% (Iron-Bane specialty)

Attacks:
  - Maul Strike (Melee)
    Damage: 2d8+4 Physical
    Range: Melee
    To-Hit: +7
    Bonus: +2d6 vs. Constructs
  
  - Purging Strike (Special, Recharge 5-6)
    Damage: 3d8+6 Physical
    Range: Melee
    To-Hit: +7
    Effect: Removes 1 Corruption stack from self or ally
  
  - Rallying Cry (Support, 1/encounter)
    Range: 6-tile radius
    Effect: All allies gain +2 AC and 2d10 temp HP for 3 turns

Special:
  - Iron-Bane Training: +2d6 damage vs. Constructs and Corrupted enemies
  - Fire Resistant: 60% Fire Resistance from heavy armor
  - Purification Focus: Can cleanse Corruption from allies
  - Heat Adapted: Passes [Intense Heat] checks with advantage
  - Potential Ally: May join party if convinced (dialogue check)
```

**Tactical Role:**

- **Potential Ally:** Social encounter option
- **Construct Hunter:** Specialized against Magma Elementals
- **Support Tank:** Provides buffs and tanking
- **Fire Resistant:** Survives Muspelheim well
- **Neutral/Hostile Default:** Must be convinced to ally

**Brittleness Interaction:**

- Fire Resistance 60% → [Brittle] applies from Ice damage
- However, Iron-Bane likely has Ice resistance gear (not shown in base stats)
- If hostile: Standard [Brittle] vulnerability
- If allied: Can coordinate Ice Mystic + Physical Warrior combos

**Database Definition:**

```sql
INSERT INTO Biome_EnemySpawns (
    biome_id,
    enemy_name,
    enemy_type,
    min_level,
    max_level,
    base_hp,
    armor,
    move_speed,
    might, finesse, wits, will, sturdiness,
    fire_resistance,
    ice_resistance,
    corruption_resistance,
    spawn_weight,
    special_tags
) VALUES (
    4,
    'Iron-Bane Crusader',
    'Humanoid',
    10, 12,
    115, 6, 30,
    4, 2, 3, 4, 4,
    60, 0, 75,
    20, -- Rare (special encounter)
    'construct_hunter,brittle_on_ice,heat_adapted,potential_ally'
);
```

---

## IV. Brittleness Mechanic

### [Brittle] Debuff Definition

**Concept:** Fire Resistant enemies become vulnerable to Physical damage after taking Ice damage.

**Mechanical Implementation:**

```
1. Enemy has Fire Resistance > 0%
2. Enemy takes Ice damage
3. Apply [Brittle] debuff for 2 turns
4. While [Brittle]: Physical damage +50% (applied after armor)
5. [Brittle] can be refreshed (reset duration to 2 turns)
6. [Brittle] stacks with other vulnerabilities (additive)
```

**Database Definition:**

```sql
INSERT INTO StatusEffects (
    effect_id,
    effect_name,
    effect_type,
    effect_description,
    duration_turns,
    is_debuff,
    stacks,
    vulnerability_type,
    vulnerability_percent
) VALUES (
    2004,
    '[Brittle]',
    'Vulnerability',
    'Flash-frozen tissue becomes vulnerable to Physical trauma. Fire-resistant enemies take +50% Physical damage.',
    2,
    1,
    0, -- Does not stack (refreshes instead)
    'Physical',
    50
);
```

### BrittlenessService Implementation

```csharp
using Serilog;
using RuneAndRust.Core.Models;
using [RuneAndRust.Services](http://RuneAndRust.Services).Combat;

namespace [RuneAndRust.Services](http://RuneAndRust.Services).Biomes
{
    public class BrittlenessService
    {
        private readonly ILogger _log;
        private readonly StatusEffectService _statusEffectService;

        public BrittlenessService(
            ILogger log,
            StatusEffectService statusEffectService)
        {
            _log = log;
            _statusEffectService = statusEffectService;
        }

        /// <summary>
        /// Check if Ice damage should apply [Brittle] debuff to target.
        /// Applies if target has Fire Resistance > 0%.
        /// </summary>
        public void TryApplyBrittle(Combatant target, int iceDamageDealt)
        {
            if (iceDamageDealt <= 0)
            {
                return;
            }

            int fireResistance = target.GetResistance([DamageType.Fire](http://DamageType.Fire));

            if (fireResistance <= 0)
            {
                _log.Information(
                    "{Target} has no Fire Resistance, [Brittle] does not apply",
                    [target.Name](http://target.Name)
                );
                return;
            }

            _log.Information(
                "{Target} has {FireRes}% Fire Resistance, applying [Brittle]",
                [target.Name](http://target.Name),
                fireResistance
            );

            // Apply [Brittle] debuff (2 turns, +50% Physical damage)
            _statusEffectService.ApplyStatusEffect(
                target,
                "[Brittle]",
                duration: 2
            );

            _log.Information(
                "{Target} is now [Brittle] - Physical damage +50% for 2 turns",
                [target.Name](http://target.Name)
            );
        }

        /// <summary>
        /// Calculate Physical damage bonus against [Brittle] target.
        /// Called after armor reduction.
        /// </summary>
        public int ApplyBrittleBonus(Combatant target, int basePhysicalDamage)
        {
            if (!target.HasStatusEffect("[Brittle]"))
            {
                return basePhysicalDamage;
            }

            int bonusDamage = basePhysicalDamage / 2; // +50%
            int totalDamage = basePhysicalDamage + bonusDamage;

            _log.Information(
                "[Brittle] bonus: {Base} Physical damage → {Total} (+{Bonus})",
                basePhysicalDamage,
                totalDamage,
                bonusDamage
            );

            return totalDamage;
        }

        /// <summary>
        /// Check if enemy qualifies for [Brittle] mechanic.
        /// </summary>
        public bool IsBrittleEligible(Combatant combatant)
        {
            int fireResistance = combatant.GetResistance([DamageType.Fire](http://DamageType.Fire));
            return fireResistance > 0;
        }
    }
}
```

### Integration with DamageService

```csharp
// In DamageService.ApplyDamage()

public void ApplyDamage(
    Combatant target,
    int rawDamage,
    DamageType damageType,
    string source)
{
    // ... existing damage calculation (armor, resistance) ...

    // Apply [Brittle] bonus if Physical damage
    if (damageType == DamageType.Physical)
    {
        finalDamage = _brittlenessService.ApplyBrittleBonus(target, finalDamage);
    }

    // Apply final damage
    target.CurrentHP -= finalDamage;

    _log.Information(
        "{Target} takes {Damage} {Type} damage from {Source} (HP: {CurrentHP}/{MaxHP})",
        [target.Name](http://target.Name),
        finalDamage,
        damageType,
        source,
        target.CurrentHP,
        target.MaxHP
    );

    // If Ice damage was dealt, check for [Brittle]
    if (damageType == [DamageType.Ice](http://DamageType.Ice) && finalDamage > 0)
    {
        _brittlenessService.TryApplyBrittle(target, finalDamage);
    }
}
```

---

## V. Spawn System

### Biome_EnemySpawns Table

```sql
CREATE TABLE IF NOT EXISTS Biome_EnemySpawns (
    spawn_id INTEGER PRIMARY KEY AUTOINCREMENT,
    biome_id INTEGER NOT NULL,
    enemy_name TEXT NOT NULL,
    enemy_type TEXT CHECK(enemy_type IN ('Undying', 'Construct', 'Humanoid', 'Boss', 'Beast')),
    min_level INTEGER NOT NULL,
    max_level INTEGER NOT NULL,
    base_hp INTEGER NOT NULL,
    armor INTEGER DEFAULT 0,
    move_speed INTEGER DEFAULT 30,
    
    -- Attributes
    might INTEGER DEFAULT 2,
    finesse INTEGER DEFAULT 2,
    wits INTEGER DEFAULT 2,
    will INTEGER DEFAULT 2,
    sturdiness INTEGER DEFAULT 2,
    
    -- Resistances
    fire_resistance INTEGER DEFAULT 0,
    ice_resistance INTEGER DEFAULT 0,
    lightning_resistance INTEGER DEFAULT 0,
    poison_resistance INTEGER DEFAULT 0,
    physical_resistance INTEGER DEFAULT 0,
    psychic_resistance INTEGER DEFAULT 0,
    corruption_resistance INTEGER DEFAULT 0,
    
    -- Spawn rules
    spawn_weight INTEGER DEFAULT 100,
    special_tags TEXT, -- Comma-separated
    
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_biome_enemy_spawns_biome ON Biome_EnemySpawns(biome_id);
CREATE INDEX IF NOT EXISTS idx_biome_enemy_spawns_level ON Biome_EnemySpawns(min_level, max_level);
CREATE INDEX IF NOT EXISTS idx_biome_enemy_spawns_weight ON Biome_EnemySpawns(spawn_weight);
```

### Complete Spawn Seeding Script

```sql
-- =====================================================
-- v0.29.3: Enemy Definitions & Spawn System
-- =====================================================

BEGIN TRANSACTION;

-- Enemy 1: Forge-Hardened Undying
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level, base_hp, armor, move_speed,
    might, finesse, wits, will, sturdiness,
    fire_resistance, ice_resistance,
    spawn_weight, special_tags
) VALUES (
    4, 'Forge-Hardened Undying', 'Undying',
    7, 9, 75, 3, 25,
    3, 1, 1, 1, 4,
    75, -50,
    150, 'brittle_on_ice,heat_immune'
);

-- Enemy 2: Magma Elemental
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level, base_hp, armor, move_speed,
    might, finesse, wits, will, sturdiness,
    fire_resistance, ice_resistance, physical_resistance,
    spawn_weight, special_tags
) VALUES (
    4, 'Magma Elemental', 'Construct',
    8, 11, 60, 2, 20,
    2, 1, 2, 0, 5,
    100, -30, 25,
    80, 'burning_trail,death_explosion,brittle_on_ice,heat_immune,construct'
);

-- Enemy 3: Rival Berserker
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level, base_hp, armor, move_speed,
    might, finesse, wits, will, sturdiness,
    fire_resistance, ice_resistance,
    spawn_weight, special_tags
) VALUES (
    4, 'Rival Berserker', 'Humanoid',
    9, 12, 105, 4, 30,
    4, 2, 2, 2, 3,
    50, -25,
    60, 'fury_resource,brittle_on_ice,heat_adapted'
);

-- Enemy 4: Surtur's Herald (Boss)
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level, base_hp, armor, move_speed,
    might, finesse, wits, will, sturdiness,
    fire_resistance, ice_resistance, physical_resistance, lightning_resistance,
    spawn_weight, special_tags
) VALUES (
    4, 'Surtur''s Herald', 'Boss',
    12, 12, 500, 8, 35,
    6, 3, 4, 5, 6,
    90, -40, 50, 50,
    1, 'boss,legendary_resistances,brittle_on_ice,heat_immune,multi_phase'
);

-- Enemy 5: Iron-Bane Crusader
INSERT INTO Biome_EnemySpawns (
    biome_id, enemy_name, enemy_type,
    min_level, max_level, base_hp, armor, move_speed,
    might, finesse, wits, will, sturdiness,
    fire_resistance, ice_resistance, corruption_resistance,
    spawn_weight, special_tags
) VALUES (
    4, 'Iron-Bane Crusader', 'Humanoid',
    10, 12, 115, 6, 30,
    4, 2, 3, 4, 4,
    60, 0, 75,
    20, 'construct_hunter,brittle_on_ice,heat_adapted,potential_ally'
);

-- Status Effect: [Brittle]
INSERT INTO StatusEffects (
    effect_id,
    effect_name,
    effect_type,
    effect_description,
    duration_turns,
    is_debuff,
    stacks,
    vulnerability_type,
    vulnerability_percent
) VALUES (
    2004,
    '[Brittle]',
    'Vulnerability',
    'Flash-frozen tissue becomes vulnerable to Physical trauma. Fire-resistant enemies take +50% Physical damage.',
    2,
    1,
    0,
    'Physical',
    50
);

COMMIT;

-- =====================================================
-- END v0.29.3 SEEDING SCRIPT
-- =====================================================
```

---

## VI. Unit Tests

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using [RuneAndRust.Services](http://RuneAndRust.Services).Biomes;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Tests.Biomes
{
    [TestClass]
    public class BrittlenessServiceTests
    {
        private Mock<ILogger> _mockLog;
        private Mock<StatusEffectService> _mockStatusEffectService;
        private BrittlenessService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockLog = new Mock<ILogger>();
            _mockStatusEffectService = new Mock<StatusEffectService>();
            _service = new BrittlenessService(_mockLog.Object, _mockStatusEffectService.Object);
        }

        [TestMethod]
        public void TryApplyBrittle_FireResistantEnemy_AppliesBrittle()
        {
            // Arrange
            var enemy = CreateEnemy("Forge-Hardened Undying", fireResistance: 75);

            // Act
            _service.TryApplyBrittle(enemy, iceDamageDealt: 10);

            // Assert
            _mockStatusEffectService.Verify(
                s => s.ApplyStatusEffect(enemy, "[Brittle]", 2),
                Times.Once
            );
        }

        [TestMethod]
        public void TryApplyBrittle_NoFireResistance_DoesNotApply()
        {
            // Arrange
            var enemy = CreateEnemy("Standard Undying", fireResistance: 0);

            // Act
            _service.TryApplyBrittle(enemy, iceDamageDealt: 10);

            // Assert
            _mockStatusEffectService.Verify(
                s => s.ApplyStatusEffect(It.IsAny<Combatant>(), It.IsAny<string>(), It.IsAny<int>()),
                Times.Never
            );
        }

        [TestMethod]
        public void TryApplyBrittle_NoDamageDealt_DoesNotApply()
        {
            // Arrange
            var enemy = CreateEnemy("Forge-Hardened Undying", fireResistance: 75);

            // Act
            _service.TryApplyBrittle(enemy, iceDamageDealt: 0);

            // Assert
            _mockStatusEffectService.Verify(
                s => s.ApplyStatusEffect(It.IsAny<Combatant>(), It.IsAny<string>(), It.IsAny<int>()),
                Times.Never
            );
        }

        [TestMethod]
        public void ApplyBrittleBonus_BrittleTarget_Adds50Percent()
        {
            // Arrange
            var enemy = CreateEnemy("Forge-Hardened Undying");
            enemy.AddStatusEffect("[Brittle]", duration: 2);

            // Act
            int finalDamage = _service.ApplyBrittleBonus(enemy, basePhysicalDamage: 10);

            // Assert
            Assert.AreEqual(15, finalDamage); // 10 + 50% = 15
        }

        [TestMethod]
        public void ApplyBrittleBonus_NotBrittle_NoBonus()
        {
            // Arrange
            var enemy = CreateEnemy("Standard Undying");

            // Act
            int finalDamage = _service.ApplyBrittleBonus(enemy, basePhysicalDamage: 10);

            // Assert
            Assert.AreEqual(10, finalDamage); // No bonus
        }

        [TestMethod]
        public void IsBrittleEligible_FireResistant_ReturnsTrue()
        {
            // Arrange
            var enemy = CreateEnemy("Forge-Hardened Undying", fireResistance: 75);

            // Act
            bool eligible = _service.IsBrittleEligible(enemy);

            // Assert
            Assert.IsTrue(eligible);
        }

        [TestMethod]
        public void IsBrittleEligible_NoFireResistance_ReturnsFalse()
        {
            // Arrange
            var enemy = CreateEnemy("Standard Undying", fireResistance: 0);

            // Act
            bool eligible = _service.IsBrittleEligible(enemy);

            // Assert
            Assert.IsFalse(eligible);
        }

        [TestMethod]
        public void ApplyBrittleBonus_LargeDamage_CalculatesCorrectly()
        {
            // Arrange
            var enemy = CreateEnemy("Rival Berserker");
            enemy.AddStatusEffect("[Brittle]", duration: 2);

            // Act
            int finalDamage = _service.ApplyBrittleBonus(enemy, basePhysicalDamage: 30);

            // Assert
            Assert.AreEqual(45, finalDamage); // 30 + 50% = 45
        }

        [TestMethod]
        public void TryApplyBrittle_MagmaElemental_FireImmune_StillApplies()
        {
            // Arrange
            var magmaElemental = CreateEnemy("Magma Elemental", fireResistance: 100);

            // Act
            _service.TryApplyBrittle(magmaElemental, iceDamageDealt: 15);

            // Assert
            _mockStatusEffectService.Verify(
                s => s.ApplyStatusEffect(magmaElemental, "[Brittle]", 2),
                Times.Once
            );
        }

        [TestMethod]
        public void Brittleness_IntegrationTest_IceThenPhysical()
        {
            // Arrange
            var enemy = CreateEnemy("Forge-Hardened Undying", fireResistance: 75);
            var mockDamageService = new Mock<DamageService>();

            // Act: Ice Mystic attacks with Ice damage
            _service.TryApplyBrittle(enemy, iceDamageDealt: 12);

            // Verify: [Brittle] applied
            _mockStatusEffectService.Verify(
                s => s.ApplyStatusEffect(enemy, "[Brittle]", 2),
                Times.Once
            );

            // Act: Physical Warrior attacks with Physical damage
            enemy.AddStatusEffect("[Brittle]", duration: 2); // Simulate status effect
            int boostedDamage = _service.ApplyBrittleBonus(enemy, basePhysicalDamage: 20);

            // Assert: Physical damage boosted +50%
            Assert.AreEqual(30, boostedDamage);
        }

        private Combatant CreateEnemy(string name, int fireResistance = 0)
        {
            var enemy = new Combatant
            {
                Name = name,
                MaxHP = 75,
                CurrentHP = 75,
                Attributes = new Attributes { STURDINESS = 3 }
            };

            enemy.SetResistance([DamageType.Fire](http://DamageType.Fire), fireResistance);
            return enemy;
        }
    }
}
```

---

## VII. v5.0 Setting Compliance

✅ **Technology, Not Magic:**

- Magma Elemental = "corrupted geological monitoring construct," NOT supernatural fire being
- Surtur's Herald = "Jötun-Forged warmachine," Norse-inspired military automaton designation
- Fire Resistance = "heat-resistant gear," "heavy armor," "hardened chassis"
- [Brittle] = "flash-frozen tissue vulnerability," not magical debuff

✅ **Layer 2 Voice:**

- "Containment failure," "monitoring construct," "corrupted chassis"
- "Heat-resistant gear," "ablative shielding," "thermal adaptation"
- Industrial/military terminology throughout

✅ **ASCII-Only Entity Names:**

- "Forge-Hardened Undying" (compliant)
- "Magma Elemental" (compliant)
- "Rival Berserker" (compliant)
- "Surtur's Herald" (compliant, not "Surtr")
- "Iron-Bane Crusader" (compliant)

✅ **v2.0 Canonical Accuracy:**

- Fire Resistance 75% for Forge-Hardened (v2.0 value)
- Ice Vulnerability → [Brittle] mechanic (v2.0 concept)
- Brittleness = Physical vulnerability (v2.0 design)
- Magma Elemental burning trail (v2.0 feature)

---

## VIII. Known Limitations

1. **No Procedural Generation:** Spawn placement algorithms in v0.29.4
2. **No Full Boss Encounter:** Surtur's Herald multi-phase fight in v0.35
3. **No AI Behaviors:** Advanced enemy AI in v0.36
4. **No Loot Tables:** Enemy-specific loot (beyond biome resources) in v0.36
5. **Simplified Resistance Calculations:** Complex stacking rules in v0.34

---

## IX. Success Criteria

- [ ]  All 5 enemy types defined in database
- [ ]  Brittleness mechanic implemented (BrittlenessService)
- [ ]  [Brittle] status effect defined
- [ ]  Spawn weight system integrated with room templates
- [ ]  Unit tests achieve 85%+ coverage
- [ ]  v5.0 setting compliance confirmed
- [ ]  ASCII-only entity names confirmed

---

## X. Related Documents

**Parent:** v0.29: Muspelheim Biome Implementation[[1]](v0%2029%20Muspelheim%20Biome%20Implementation%20d725fd4f95a041bfa3d0ee650ef68dd3.md)

**Previous:** v0.29.2: Environmental Hazards & Ambient Conditions[[2]](v0%2029%202%20Environmental%20Hazards%20&%20Ambient%20Conditions%200d398758edcb4560a4b0b4a9875c4a04.md)

**Next:** v0.29.4: Service Implementation & Testing

**Canonical:** v2.0 Muspelheim Biome[[3]](https://www.notion.so/Feature-Specification-The-Muspelheim-Biome-2a355eb312da80cdab65de771b57e414?pvs=21)

**Requirements:** MANDATORY[[4]](https://www.notion.so/MANDATORY-Specification-Requirements-Quality-Standards-a40022443c8b4abfb8cbd4882839447d?pvs=21)

---

**Phase 1 (Enemy Design): COMPLETE ✓**

**Phase 2 (Database Implementation): Ready to begin**