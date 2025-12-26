# Gorge-Maw Ascetic Specialization - Complete Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Specification ID: SPEC-SPECIALIZATION-GORGE-MAW-ASCETIC
Version: 1.0
Last Updated: 2025-11-27
Status: Draft - Implementation Review
> 

---

## Document Control

### Purpose

This document provides the complete specification for the Gorge-Maw Ascetic specialization, including:

- Design philosophy and mechanical identity
- All 9 abilities with **exact formulas per rank**
- **Rank unlock requirements** (tree-progression based, NOT PP-based)
- **GUI display specifications per rank**
- Current implementation status
- Combat system integration points

### Related Files

| Component | File Path | Status |
| --- | --- | --- |
| Service Implementation | `RuneAndRust.Engine/GorgeMawAsceticService.cs` | Implemented |
| Data Seeding | `RuneAndRust.Persistence/GorgeMawAsceticSeeder.cs` | Implemented |
| Tests | N/A | Not Yet Implemented |
| Specialization Tree UI | `RuneAndRust.DesktopUI/Views/SpecializationTreeView.axaml` | Generic |
| Combat UI | `RuneAndRust.DesktopUI/Views/CombatView.axaml` | No specialization integration |

### Change Log

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | 2025-11-27 | Initial specification from GorgeMawAsceticSeeder |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
| --- | --- |
| **Internal Name** | GorgeMawAscetic |
| **Display Name** | Gorge-Maw Ascetic |
| **Specialization ID** | 26002 |
| **Archetype** | Warrior (ArchetypeID = 1) |
| **Path Type** | Coherent |
| **Mechanical Role** | Control Fighter / Seismic Monk |
| **Primary Attribute** | MIGHT |
| **Secondary Attribute** | WILL |
| **Resource System** | Stamina |
| **Trauma Risk** | None (Coherent) |
| **Icon** | :mountain: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
| --- | --- | --- |
| **PP Cost to Unlock** | 10 PP | Higher than standard 3 PP (reflects power level) |
| **Minimum Legend** | 5 | Mid-game specialization |
| **Maximum Corruption** | 100 | No corruption restriction |
| **Minimum Corruption** | 0 | No minimum corruption |
| **Required Quest** | None | No quest prerequisite |

### 1.3 Design Philosophy

**Tagline**: "The Seismic Monk"

**Core Fantasy**: The Gorge-Maw Ascetic embodies the warrior-philosopher who perceives the world through vibrations in the earth rather than sight. Through disciplined meditation near colossal Gorge-Maws, they have mastered Tremorsense—a seismic perception that makes them immune to darkness and blindness but completely vulnerable to flying enemies.

**Mechanical Identity**:

1. **Tremorsense**: Immune to visual impairment, auto-detect ground-based enemies, but blind to flying enemies
2. **Unarmed Combat**: Specialized in earth-channeling strikes and shockwaves
3. **Battlefield Control**: Emphasis on Push, Stun, Root, and Difficult Terrain effects
4. **Mental Fortress**: Exceptional resistance to Fear and mental effects, with aura protection for allies

### 1.4 Unique Mechanic: Tremorsense

**Tremorsense Effects**:

- **IMMUNE TO**: [Blinded], [Thick Fog], [Absolute Darkness]
- **AUTO-DETECT**: All ground-based enemies, including Hidden/Stealth targets
- **VULNERABILITY**: 50% miss chance vs flying enemies, 0 Defense vs flying attacks, flying enemies invisible on minimap

### 1.5 Specialization Description (Full Text)

> The Gorge-Maw Ascetic embodies the warrior-philosopher who perceives the world through vibrations in the earth rather than sight. Through disciplined meditation near colossal Gorge-Maws, they have mastered Tremorsense—a seismic perception that makes them immune to darkness and blindness but completely vulnerable to flying enemies.
> 
> 
> They weaponize the earth itself with unarmed strikes and shockwaves, creating a unique tactical dynamic of extreme situational power. Their mental discipline grants exceptional resistance to Fear and mental effects, providing aura protection to allies.
> 
> This path emphasizes control over raw damage, manipulating the battlefield through Push, Stun, Root, and Difficult Terrain effects. The ultimate expression is Earthshaker—a battlefield-wide earthquake that permanently alters terrain and knocks down all ground-based enemies.
> 

---

## 2. Rank Progression System

### 2.1 CRITICAL: Rank Unlock Rules

**Ranks are unlocked through TREE PROGRESSION, not PP spending.**

| Tier | Starting Rank | Progresses To | Rank 3 Trigger |
| --- | --- | --- | --- |
| **Tier 1** | Rank 1 (when learned) | Rank 2 (when 2 Tier 2 trained) | Capstone trained |
| **Tier 2** | Rank 2 (when learned) | Rank 3 (when Capstone trained) | Capstone trained |
| **Tier 3** | No ranks | N/A | N/A |
| **Capstone** | No ranks | N/A | N/A |

**Note**: Tremorsense (Ability 26010) has **no ranks** - it is a foundational passive that defines the specialization.

### 2.2 Ability Structure by Tier

| Tier | Abilities | PP Cost to Unlock | Starting Rank | Max Rank | Rank Progression |
| --- | --- | --- | --- | --- | --- |
| **Tier 1** | 3 | 3 PP each | 1 (except Tremorsense) | 3 | 1→2 (2× Tier 2), 2→3 (Capstone) |
| **Tier 2** | 3 | 4 PP each | 2 | 3 | 2→3 (Capstone) |
| **Tier 3** | 2 | 5 PP each | N/A | N/A | No ranks |
| **Capstone** | 1 | 6 PP | N/A | N/A | No ranks |

---

## 3. Ability Tree Overview

### 3.1 Visual Tree Structure

```
                    TIER 1: FOUNDATION (3 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Tremorsense]       [Stone Fist]       [Concussive Pulse]
 (Passive)           (Active)             (Active)
 (NO RANKS)         (Ranks 1-3)          (Ranks 1-3)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
              ════════════════════════
              RANK 2 UNLOCKS HERE
              (when 2 Tier 2 trained)
              ════════════════════════
                          │
                          ▼
                TIER 2: ADVANCED (4 PP each)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Sensory             [Shattering        [Resonant Tremor]
 Discipline]           Wave]               (Active)
 (Passive)            (Active)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
    [Earthen Grasp]          [Inner Stillness]
       (Active)                 (Passive)
          │                               │
          └───────────────┬───────────────┘
                          │
                          ▼
              ════════════════════════
              RANK 3 UNLOCKS HERE
              (when Capstone trained)
              ════════════════════════
                          │
                          ▼
              TIER 4: CAPSTONE (6 PP)
                          │
                   [Earthshaker]
                     (Active)

```

### 3.2 Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Resource Cost | Key Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 26010 | Tremorsense | 1 | Passive | — | None | Seismic perception (immune blind, vs flying penalty) |
| 26011 | Stone Fist | 1 | Active | 1→2→3 | 30 Stamina | Unarmed strike + MIGHT |
| 26012 | Concussive Pulse | 1 | Active | 1→2→3 | 35 Stamina | Push Front Row to Back + damage |
| 26013 | Sensory Discipline | 2 | Passive | 2→3 | None | +dice vs [Fear] and [Disoriented] |
| 26014 | Shattering Wave | 2 | Active | 2→3 | 40 Stamina | Single target stun at any range |
| 26015 | Resonant Tremor | 2 | Active | 2→3 | 35 Stamina | Create [Difficult Terrain] zone |
| 26016 | Earthen Grasp | 3 | Active | — | 45 Stamina | AoE [Root] + damage |
| 26017 | Inner Stillness | 3 | Passive | — | None | Mental immunity + ally aura |
| 26018 | Earthshaker | 4 | Active | — | 60 Stamina | Battlefield earthquake + terrain |

---

## 4. Tier 1 Abilities (Detailed Rank Specifications)

---

### 4.1 Tremorsense (ID: 26010)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | None (foundational ability) |
| **Resource Cost** | None (passive) |

### Description

Perceive the world through earth vibrations. Immune to blindness and darkness, auto-detect all ground-based enemies, but completely blind to flying enemies.

### Mechanical Effect

**Immunities**:

- IMMUNE to [Blinded]
- IMMUNE to [Thick Fog]
- IMMUNE to [Absolute Darkness]

**Auto-Detection**:

- Automatically detect all ground-based enemies
- Detects Hidden and Stealth enemies touching the ground
- Cannot be surprised by ground-based ambushes

**Flying Vulnerability**:

- 50% miss chance vs flying enemies
- 0 Defense vs flying enemy attacks
- Flying enemies are invisible on minimap

**Formulas**:

```
If (Target.IsFlying):
    AttackRoll = Roll() * 0.5  // 50% miss chance
    Defense = 0
Else:
    AutoDetect(Target)
    IgnoreVisualImpairment = true

```

### GUI Display

- Passive icon: Eye with seismic waves
- Tooltip: "Tremorsense: Immune to visual impairment. Auto-detect ground enemies. 50% miss vs flying, 0 Defense vs flying."
- Warning indicator when flying enemies present: "FLYING ENEMIES DETECTED - Tremorsense ineffective!"

### Combat Log Examples

- "Tremorsense detects [Hidden Enemy]!"
- "Tremorsense: Ignoring [Thick Fog]"
- "Attack vs [Flying Enemy] misses (Tremorsense blind to flying)"
- "WARNING: [Flying Enemy] attacks with advantage (Tremorsense vulnerability)"

### Implementation Status

- [x]  Data seeded in `GorgeMawAsceticSeeder.SeedGorgeMawAsceticTier1()`
- [ ]  Combat: Visual impairment immunity
- [ ]  Combat: Flying enemy detection and penalties
- [ ]  GUI: Flying enemy warning indicator

---

### 4.2 Stone Fist (ID: 26011)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Melee)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 30 Stamina |
| **Attribute Used** | MIGHT |
| **Damage Type** | Physical |

### Description

Unarmed strike using weighted gauntlets, channeling seismic force into the blow.

### Rank Details

### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:

- Damage: 2d8 + MIGHT Physical damage, single target, melee

**Formula**:

```
Damage = Roll(2d8) + MIGHT

```

**GUI Display**:

- Ability button: Fist with stone texture
- Tooltip: "Stone Fist (Rank 1): 2d8+MIGHT Physical damage. Cost: 30 Stamina"
- Color: Bronze border

---

### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:

- Damage: 3d8 + MIGHT Physical damage

**Formula**:

```
Damage = Roll(3d8) + MIGHT

```

**GUI Display**:

- Tooltip: "Stone Fist (Rank 2): 3d8+MIGHT Physical damage. Cost: 30 Stamina"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Damage: 4d8 + MIGHT Physical damage
- **NEW**: 10% chance to apply [Staggered] for 1 turn

**Formula**:

```
Damage = Roll(4d8) + MIGHT
If (Roll(1d100) <= 10):
    Target.AddStatus("Staggered", Duration: 1)

```

**GUI Display**:

- Tooltip: "Stone Fist (Rank 3): 4d8+MIGHT Physical damage. 10% Stagger chance. Cost: 30 Stamina"
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `GorgeMawAsceticSeeder.SeedGorgeMawAsceticTier1()`
- [ ]  Combat: Basic melee attack implementation
- [ ]  Combat: Stagger chance (Rank 3)

---

### 4.3 Concussive Pulse (ID: 26012)

**Type**: Active | **Action**: Standard Action | **Target**: AoE Front Row

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 35 Stamina |
| **Attribute Used** | MIGHT |
| **Damage Type** | Physical |
| **Control Effect** | Push to Back Row |

### Description

Strike the ground creating shockwave that pushes enemies back.

### Rank Details

### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:

- Push all Front Row enemies to Back Row
- Damage: 1d6 + MIGHT Physical damage

**Formula**:

```
For each Enemy in FrontRow:
    Enemy.Position = BackRow
    Damage = Roll(1d6) + MIGHT

```

**GUI Display**:

- Ability button: Ground strike with shockwave
- Tooltip: "Concussive Pulse (Rank 1): Push Front Row to Back. 1d6+MIGHT damage. Cost: 35 Stamina"
- Color: Bronze border

---

### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:

- Push all Front Row enemies to Back Row
- Damage: 2d6 + MIGHT Physical damage

**Formula**:

```
For each Enemy in FrontRow:
    Enemy.Position = BackRow
    Damage = Roll(2d6) + MIGHT

```

**GUI Display**:

- Tooltip: "Concussive Pulse (Rank 2): Push Front Row to Back. 2d6+MIGHT damage. Cost: 35 Stamina"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Push all Front Row enemies to Back Row
- Damage: 2d8 + MIGHT Physical damage
- **NEW**: If enemy collides with Back Row (already occupied), apply [Staggered] for 1 round

**Formula**:

```
For each Enemy in FrontRow:
    If (BackRow.HasEnemies):
        Enemy.AddStatus("Staggered", Duration: 1)
    Enemy.Position = BackRow
    Damage = Roll(2d8) + MIGHT

```

**GUI Display**:

- Tooltip: "Concussive Pulse (Rank 3): Push Front Row to Back. 2d8+MIGHT damage. Staggers if collision. Cost: 35 Stamina"
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `GorgeMawAsceticSeeder.SeedGorgeMawAsceticTier1()`
- [ ]  Combat: Push mechanic implementation
- [ ]  Combat: Collision detection for Stagger

---

## 5. Tier 2 Abilities (Rank 2→3 Progression)

---

### 5.1 Sensory Discipline (ID: 26013)

**Type**: Passive | **Action**: Free Action | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in tree |
| **Ranks** | 2→3 |
| **Resource Cost** | None (passive) |
| **Attribute Used** | WILL |

### Description

Profound mental stillness grants resistance to mental effects.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- +2 dice vs [Fear] effects
- +2 dice vs [Disoriented] effects

**Formula**:

```
ResolveCheck_Fear = WILL + 2
ResolveCheck_Disoriented = WILL + 2

```

**GUI Display**:

- Passive icon: Calm mind symbol
- Tooltip: "Sensory Discipline (Rank 2): +2 dice vs Fear and Disoriented"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- +4 dice vs [Fear] effects
- +4 dice vs [Disoriented] effects
- **NEW**: Immune to [Fear] from ambient sources (non-enemy Fear)

**Formula**:

```
ResolveCheck_Fear = WILL + 4
ResolveCheck_Disoriented = WILL + 4
If (FearSource.IsAmbient):
    Immune = true

```

**GUI Display**:

- Tooltip: "Sensory Discipline (Rank 3): +4 dice vs Fear/Disoriented. Immune to ambient Fear."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `GorgeMawAsceticSeeder.SeedGorgeMawAsceticTier2()`
- [ ]  Combat: Resolve check modifiers
- [ ]  Combat: Ambient Fear immunity

---

### 5.2 Shattering Wave (ID: 26014)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Any Range)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in tree |
| **Ranks** | 2→3 |
| **Resource Cost** | 40 Stamina |
| **Cooldown** | 2 turns |
| **Status Effects** | [Stunned], [Staggered] |

### Description

Targeted tremor that stuns priority target at range.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Single target at any range, auto-hit
- 60% chance to apply [Stunned] for 1 round
- If Stun fails: Guaranteed [Staggered] instead

**Formula**:

```
If (Roll(1d100) <= 60):
    Target.AddStatus("Stunned", Duration: 1)
Else:
    Target.AddStatus("Staggered", Duration: 1)

```

**GUI Display**:

- Ability button: Focused tremor waves
- Tooltip: "Shattering Wave (Rank 2): 60% Stun (1 round) or guaranteed Stagger. Any range, auto-hit. Cost: 40 Stamina"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- 85% chance to apply [Stunned] for 2 rounds
- If Stun fails: Guaranteed [Staggered] + 3d6 damage

**Formula**:

```
If (Roll(1d100) <= 85):
    Target.AddStatus("Stunned", Duration: 2)
Else:
    Target.AddStatus("Staggered", Duration: 1)
    Damage = Roll(3d6)

```

**GUI Display**:

- Tooltip: "Shattering Wave (Rank 3): 85% Stun (2 rounds) or Stagger + 3d6 damage. Cost: 40 Stamina"
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `GorgeMawAsceticSeeder.SeedGorgeMawAsceticTier2()`
- [ ]  Combat: Stun chance calculation
- [ ]  Combat: Fallback to Stagger

---

### 5.3 Resonant Tremor (ID: 26015)

**Type**: Active | **Action**: Standard Action | **Target**: Target Area

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in tree |
| **Ranks** | 2→3 |
| **Resource Cost** | 35 Stamina |
| **Cooldown** | 3 turns |

### Description

Create zone of difficult terrain under enemy formation.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Create [Difficult Terrain] in 4x4 tile area for 3 rounds
- Enemies in zone take 1d6 damage per turn

**Formula**:

```
CreateZone("DifficultTerrain", Size: 4x4, Duration: 3)
For each Enemy in Zone (per turn):
    Damage = Roll(1d6)

```

**GUI Display**:

- Ability button: Cracked earth zone
- Tooltip: "Resonant Tremor (Rank 2): Create 4x4 Difficult Terrain (3 rounds). 1d6 damage/turn to enemies in zone. Cost: 35 Stamina"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Create [Difficult Terrain] in 5x5 tile area for 4 rounds
- Enemies in zone take 2d6 damage per turn
- **NEW**: Enemies in zone suffer -1 Accuracy

**Formula**:

```
CreateZone("DifficultTerrain", Size: 5x5, Duration: 4, AccuracyPenalty: -1)
For each Enemy in Zone (per turn):
    Damage = Roll(2d6)
    Enemy.Accuracy -= 1

```

**GUI Display**:

- Tooltip: "Resonant Tremor (Rank 3): 5x5 Difficult Terrain (4 rounds). 2d6/turn, -1 Accuracy. Cost: 35 Stamina"
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `GorgeMawAsceticSeeder.SeedGorgeMawAsceticTier2()`
- [ ]  Combat: Zone creation system
- [ ]  Combat: DoT in zone
- [ ]  Combat: Accuracy debuff in zone

---

## 6. Tier 3 Abilities (No Ranks)

---

### 6.1 Earthen Grasp (ID: 26016)

**Type**: Active | **Action**: Standard Action | **Target**: AoE Front Row (or Both Rows)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in tree |
| **Ranks** | None |
| **Resource Cost** | 45 Stamina |
| **Cooldown** | 3 turns |
| **Status Effects** | [Rooted], [Vulnerable] |

### Description

Earth erupts to root all enemies in area.

### Mechanical Effect

- AoE targets both rows
- Apply [Rooted] for 3 rounds
- Deal 4d6 Physical damage
- **Bonus**: [Vulnerable] while Rooted

**Formulas**:

```
For each Enemy in FrontRow + BackRow:
    Damage = Roll(4d6)
    Enemy.AddStatus("Rooted", Duration: 3)
    Enemy.AddStatus("Vulnerable", Duration: 3)  // While Rooted

```

### GUI Display

- Ability button: Earth hands grasping
- Tooltip: "Earthen Grasp: Both rows, [Rooted] 3 rounds, 4d6 damage, [Vulnerable] while Rooted. Cost: 45 Stamina"

### Combat Log Examples

- "Earthen Grasp roots 4 enemies! 18 damage each, Rooted and Vulnerable for 3 rounds"
- "[Enemy] cannot move (Rooted)"

### Implementation Status

- [x]  Data seeded in `GorgeMawAsceticSeeder.SeedGorgeMawAsceticTier3()`
- [ ]  Status Effect: [Rooted] definition
- [ ]  Combat: Apply Vulnerable conditionally while Rooted

---

### 6.2 Inner Stillness (ID: 26017)

**Type**: Passive | **Action**: Free Action | **Target**: Self + Adjacent Allies

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in tree |
| **Ranks** | None |
| **Resource Cost** | None (passive) |

### Description

Complete mental immunity and aura protection for adjacent allies.

### Mechanical Effect

- **Self**: IMMUNE to [Fear], [Disoriented], [Charmed]
- **Aura**: Adjacent allies gain +2 dice vs [Fear], [Disoriented], [Charmed]

**Formulas**:

```
Self.AddImmunity(["Fear", "Disoriented", "Charmed"])
For each AdjacentAlly:
    AdjacentAlly.ResolveBonus_Fear += 2
    AdjacentAlly.ResolveBonus_Disoriented += 2
    AdjacentAlly.ResolveBonus_Charmed += 2

```

### GUI Display

- Passive icon: Tranquil mind with protective aura
- Tooltip: "Inner Stillness: Immune to Fear/Disoriented/Charmed. Adjacent allies +2 dice vs all three."
- Aura indicator on adjacent allies

### Combat Log Examples

- "Inner Stillness: [Ascetic] is IMMUNE to Fear!"
- "[Adjacent Ally] gains +2 dice vs Fear (Inner Stillness aura)"

### Implementation Status

- [x]  Data seeded in `GorgeMawAsceticSeeder.SeedGorgeMawAsceticTier3()`
- [ ]  Combat: Mental immunity system
- [ ]  Combat: Aura range detection
- [ ]  GUI: Aura indicator

---

## 7. Capstone Ability (No Ranks)

---

### 7.1 Earthshaker (ID: 26018)

**Type**: Active | **Action**: Standard Action | **Target**: All Ground Enemies

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 4 (Capstone) |
| **PP Cost to Unlock** | 6 PP |
| **Prerequisite** | 24 PP invested in tree + both Tier 3 abilities |
| **Ranks** | None |
| **Resource Cost** | 60 Stamina |
| **Cooldown** | Once Per Combat |
| **Status Effects** | [Knocked Down], [Vulnerable] |
| **Special** | Training this ability upgrades all Tier 1 & Tier 2 abilities to Rank 3 |

### Description

Massive earthquake knocking down all ground enemies and permanently altering terrain.

### Mechanical Effect

- Damage: 6d10 + MIGHT Physical damage to ALL ground enemies
- Guaranteed [Knocked Down] for 2 rounds
- Apply [Vulnerable] for 1 round
- Create permanent Difficult Terrain (5x5) with Cover

**Formulas**:

```
For each Enemy in AllGroundEnemies:
    Damage = Roll(6d10) + MIGHT
    Enemy.AddStatus("KnockedDown", Duration: 2)
    Enemy.AddStatus("Vulnerable", Duration: 1)
CreatePermanentTerrain("DifficultTerrain", Size: 5x5, GrantsCover: true)

```

**Note**: Flying enemies are completely unaffected (Tremorsense limitation)

### GUI Display - CAPSTONE NOTIFICATION

When used:

```
┌─────────────────────────────────────────────┐
│            EARTHSHAKER ACTIVATED!           │
├─────────────────────────────────────────────┤
│                                             │
│  The earth itself obeys your command!       │
│                                             │
│  • ALL ground enemies take massive damage   │
│  • ALL ground enemies Knocked Down          │
│  • Permanent Difficult Terrain created      │
│                                             │
│  "The world trembles at your feet."         │
│                                             │
└─────────────────────────────────────────────┘

```

- Screen shake effect
- Terrain visually changes
- Once-per-combat indicator

### Combat Log Examples

- "EARTHSHAKER! The battlefield trembles!"
- "5 ground enemies take 42 damage and are Knocked Down!"
- "Permanent Difficult Terrain created (5x5 with Cover)"
- "WARNING: [Flying Enemy] is unaffected by Earthshaker"

### Implementation Status

- [x]  Data seeded in `GorgeMawAsceticSeeder.SeedGorgeMawAsceticCapstone()`
- [ ]  Combat: All ground enemy targeting
- [ ]  Combat: Flying enemy exclusion
- [ ]  Combat: Permanent terrain creation
- [ ]  GUI: Earthquake screen effect

---

## 8. Status Effect Definitions

### 8.1 [Rooted]

| Property | Value |
| --- | --- |
| **Applied By** | Earthen Grasp |
| **Duration** | 3 rounds |
| **Icon** | Earth vines |
| **Color** | Brown |

**Effects**:

- Cannot move
- Cannot use movement abilities
- Can still attack and use abilities

---

### 8.2 [Knocked Down]

| Property | Value |
| --- | --- |
| **Applied By** | Earthshaker |
| **Duration** | 2 rounds |
| **Icon** | Prone figure |
| **Color** | Gray |

**Effects**:

- 2 dice to all attack rolls
- +2 dice to attacks against this target
- Standing up costs a Standard Action

---

### 8.3 [Staggered]

| Property | Value |
| --- | --- |
| **Applied By** | Stone Fist, Concussive Pulse, Shattering Wave |
| **Duration** | 1 round |
| **Icon** | Dizzy stars |
| **Color** | Yellow |

**Effects**:

- 1 to Defense rolls
- Cannot take Reaction abilities
- Movement costs +1 additional movement point

---

### 8.4 [Stunned]

| Property | Value |
| --- | --- |
| **Applied By** | Shattering Wave |
| **Duration** | 1-2 rounds |
| **Icon** | Lightning bolt |
| **Color** | Blue |

**Effects**:

- Cannot take any actions
- Automatically fails Defense rolls
- Status removed on damage (optional rule)

---

## 9. GUI Requirements

### 9.1 Tremorsense Indicator

```
┌─────────────────────────────────────────┐
│  TREMORSENSE ACTIVE                     │
│  ✓ Visual impairment immune             │
│  ✓ Ground enemies auto-detected         │
│  ⚠ Flying enemies: 50% miss, 0 Defense  │
└─────────────────────────────────────────┘

```

- Always visible when in combat
- Warning flashes when flying enemies present

### 9.2 Terrain Zone Display

- [Difficult Terrain] zones shown as cracked earth texture
- Zone boundaries clearly marked
- Duration indicator on zone
- Cover bonus indicator where applicable

---

## 10. Implementation Priority

### Phase 1: Critical (Foundation)

1. **Implement Tremorsense system** - Visual immunity + flying penalties
2. **Fix GorgeMawAsceticSeeder.cs** - Remove/correct CostToRank2 values
3. **Implement rank calculation logic** based on tree progression

### Phase 2: Combat Integration

1. **Implement Push mechanics** - Front to Back row movement
2. **Implement zone creation** - Difficult Terrain with effects
3. **Route abilities** through CombatEngine

### Phase 3: Status Effects

1. **Define status effects** ([Rooted], [Stunned], [Knocked Down])
2. **Implement control effect durations**
3. **Implement mental immunity** (Inner Stillness)

### Phase 4: Capstone & Polish

1. **Implement Earthshaker** - Permanent terrain creation
2. **Add flying enemy detection** and warnings
3. **Add terrain visual effects**

---

**End of Specification**