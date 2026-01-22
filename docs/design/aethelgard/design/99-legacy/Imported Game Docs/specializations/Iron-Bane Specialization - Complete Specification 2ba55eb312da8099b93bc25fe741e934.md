# Iron-Bane Specialization - Complete Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Specification ID: SPEC-SPECIALIZATION-IRON-BANE
Version: 1.0
Last Updated: 2025-11-27
Status: Draft - Implementation Review
> 

---

## Document Control

### Purpose

This document provides the complete specification for the Iron-Bane specialization, including:

- Design philosophy and mechanical identity
- All 9 abilities with **exact formulas per rank**
- **Rank unlock requirements** (tree-progression based, NOT PP-based)
- **GUI display specifications per rank**
- Current implementation status
- Combat system integration points

### Related Files

| Component | File Path | Status |
| --- | --- | --- |
| Data Seeding | `RuneAndRust.Persistence/DataSeeder.cs` (line 1214) | Implemented |
| Specialization Enum | `RuneAndRust.Core/Specialization.cs` | Defined |
| Specialization Factory | `RuneAndRust.Engine/SpecializationFactory.cs` | Referenced |
| Tests | N/A | Not Yet Implemented |

### Change Log

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | 2025-11-27 | Initial specification from DataSeeder implementation |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
| --- | --- |
| **Internal Name** | IronBane |
| **Display Name** | Iron-Bane |
| **Specialization ID** | 11 |
| **Archetype** | Warrior (ArchetypeID = 1) |
| **Path Type** | Coherent |
| **Mechanical Role** | Anti-Mechanical Specialist / Controller |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | MIGHT |
| **Resource System** | Stamina + Righteous Fervor (0-100) |
| **Trauma Risk** | Low |
| **Icon** | :fire: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
| --- | --- | --- |
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 3 | Early-game specialization |
| **Maximum Corruption** | 100 | No corruption restriction |
| **Minimum Corruption** | 0 | No minimum corruption |
| **Required Quest** | None | No quest prerequisite |

### 1.3 Design Philosophy

**Tagline**: "Knowledge is the Deadliest Weapon"

**Core Fantasy**: You are a warrior-scholar who has studied the Undying and their mechanical corruption. Where others see invincible foes, you see exploitable weaknesses in their code. You wield flame and faith to purge the abominations, using your knowledge of pre-Glitch technology to identify and destroy critical systems.

You are the debugger, the antivirus, the purifier who turns the Blight's own corrupted machinery against itself. Through study and conviction, you have become the deadliest weapon against the mechanical horrors that plague the world.

**Mechanical Identity**:

1. **Enemy-Type Specialist**: Massive bonuses against Mechanical and Undying enemies
2. **Knowledge-Based Combat**: Observe enemies to reveal weaknesses and unlock bonus effects
3. **Fire Damage Focus**: Primary damage type is Fire, with persistent Burning effects
4. **Righteous Fervor**: Build conviction through purging the corrupted

### 1.4 The Righteous Fervor Resource System

**Righteous Fervor Mechanics**:

- **Range**: 0-100
- **Generation**: Gained by damaging/killing Mechanical/Undying, observing enemies
- **Decay**: Decays slowly out of combat
- **Consumption**: Required for powerful anti-corruption abilities

**Fervor Generation**:

| Source | Base Fervor Gained |
| --- | --- |
| Purifying Flame vs Mechanical/Undying | 15-25 (by rank) |
| Observing enemy (Rank 2+) | 10 |
| Flame Ward retaliation (Rank 3) | 10 |
| Killing Mechanical/Undying | Varies by ability |

### 1.5 Enemy Type Targeting

The Iron-Bane specializes against two enemy categories:

| Enemy Type | Examples | Iron-Bane Bonus |
| --- | --- | --- |
| **Mechanical** | Constructs, Drones, Automatons | All ability bonuses apply |
| **Undying** | Zombies, Revenants, Corrupted Dead | All ability bonuses apply |
| **Other** | Organic, Beasts, Humanoids | Base effects only |

### 1.6 Specialization Description (Full Text)

> You are a warrior-scholar who has studied the Undying and their mechanical corruption. Where others see invincible foes, you see exploitable weaknesses in their code. You wield flame and faith to purge the abominations, using your knowledge of pre-Glitch technology to identify and destroy critical systems.
> 
> 
> You are the debugger, the antivirus, the purifier who turns the Blight's own corrupted machinery against itself. Through study and conviction, you have become the deadliest weapon against the mechanical horrors that plague the world.
> 
> Your power comes not from brute force, but from understanding. Every Undying weakness memorized. Every mechanical schematic analyzed. Every corrupted system waiting to be shut down.
> 

---

## 2. Rank Progression System

### 2.1 CRITICAL: Rank Unlock Rules

**Ranks are unlocked through TREE PROGRESSION, not PP spending.**

| Tier | Starting Rank | Progresses To | Rank 3 Trigger |
| --- | --- | --- | --- |
| **Tier 1** | Rank 1 (when learned) | Rank 2 (when 2 Tier 2 trained) | Capstone trained |
| **Tier 2** | Rank 2 (when learned) | Rank 3 (when Capstone trained) | Capstone trained |
| **Tier 3** | Rank 2 (when learned) | Rank 3 (when Capstone trained) | Capstone trained |
| **Capstone** | Rank 1 | Rank 2→3 (tree-based) | Full tree completion |

### 2.2 Ability Structure by Tier

| Tier | Abilities | PP Cost to Unlock | Starting Rank | Max Rank | Rank Progression |
| --- | --- | --- | --- | --- | --- |
| **Tier 1** | 3 | 0 PP (free with spec) | 1 | 3 | 1→2 (2× Tier 2), 2→3 (Capstone) |
| **Tier 2** | 3 | 4 PP each | 2 | 3 | 2→3 (Capstone) |
| **Tier 3** | 2 | 5 PP each | 2 | 3 | 2→3 (Capstone) |
| **Capstone** | 1 | 6 PP | 1 | 3 | 1→2→3 (tree-based) |

### 2.3 Total PP Investment

| Milestone | PP Spent | Abilities Unlocked | Tier 1 Rank | Tier 2 Rank |
| --- | --- | --- | --- | --- |
| Unlock Specialization | 3 PP | 3 Tier 1 (free) | Rank 1 | - |
| 2 Tier 2 | 3 + 8 = 11 PP | 3 Tier 1 + 2 Tier 2 | **Rank 2** | Rank 2 |
| All Tier 2 | 11 + 4 = 15 PP | 3 Tier 1 + 3 Tier 2 | Rank 2 | Rank 2 |
| All Tier 3 | 15 + 10 = 25 PP | 3 Tier 1 + 3 Tier 2 + 2 Tier 3 | Rank 2 | Rank 2 |
| Capstone | 25 + 6 = 31 PP | All 9 abilities | **Rank 3** | **Rank 3** |

---

## 3. Ability Tree Overview

### 3.1 Visual Tree Structure

```
                    TIER 1: FOUNDATION (Free with Spec, Ranks 1-3)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Scholar of         [Purifying Flame]   [Weakness Exploiter]
 Corruption]           (Active)            (Passive)
  (Passive)
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
[System Shutdown]  [Critical Strike]    [Flame Ward]
    (Active)           (Active)           (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
   [Purging Flame]        [Righteous Conviction]
     (Active)                  (Passive)
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
                 [Divine Purge]
                    (Active)

```

### 3.2 Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Resource Cost | Key Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1101 | Scholar of Corruption | 1 | Passive | 1→2→3 | None | Reveal enemy weaknesses |
| 1102 | Purifying Flame | 1 | Active | 1→2→3 | 35 Stamina | Fire attack + bonus vs Mech/Undying |
| 1103 | Weakness Exploiter | 1 | Passive | 1→2→3 | None | +Damage vs identified enemies |
| 1104 | System Shutdown | 2 | Active | 2→3 | 40 Stamina + 30 Fervor | Stun Mech/Undying |
| 1105 | Critical Strike | 2 | Active | 2→3 | 45 Stamina + 25 Fervor | Guaranteed crit, execute |
| 1106 | Flame Ward | 2 | Passive | 2→3 | None | Fire resist + retaliation |
| 1107 | Purging Flame | 3 | Active | 2→3 | 55 Stamina + 40 Fervor | AoE Fire, devastates Mech/Undying |
| 1108 | Righteous Conviction | 3 | Passive | 2→3 | None | +Fervor gen, +WILL, accuracy |
| 1109 | Divine Purge | 4 | Active | 1→2→3 | 60 Stamina + 75 Fervor | Instant death vs Mech/Undying |

---

## 4. Tier 1 Abilities (Detailed Rank Specifications)

These abilities are **free** when the specialization is unlocked and have 3 ranks.

---

### 4.1 Scholar of Corruption (ID: 1101)

**Type**: Passive/Active | **Action**: Free Action | **Target**: Single Enemy

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 0 PP (free with specialization) |
| **Ranks** | 3 |
| **Resource Cost** | None |
| **Attribute Used** | None |

### Description

You have studied the schematics of the Undying, memorized their corrupted code. Where others see invincible foes, you see exploitable bugs.

### Rank Details

### Rank 1 (Unlocked: When specialization is chosen)

**Mechanical Effect**:

- **Observe** (Free Action): Target enemy reveals type, resistances, and vulnerabilities
- Mechanical/Undying targets reveal 1 additional weakness

**Formula**:

```
Observe(Target):
    Reveal: EnemyType, Resistances, Vulnerabilities
    If (Target.Type == "Mechanical" OR Target.Type == "Undying"):
        Reveal: AdditionalWeakness
    Target.IsIdentified = true

```

**GUI Display**:

- Passive icon: Eye with mechanical overlay
- Tooltip: "Scholar of Corruption (Rank 1): Observe (Free Action) reveals enemy type, resistances, vulnerabilities. Mech/Undying reveal 1 extra weakness."
- Color: Bronze border

**Combat Log Examples**:

- "Scholar of Corruption: [Automaton] analyzed. Type: Mechanical. Weak to Fire, Electricity."

---

### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:

- All Rank 1 effects
- **NEW**: Observing an enemy grants +10 Righteous Fervor
- **NEW**: Auto-observe all enemies at combat start

**Formula**:

```
OnCombatStart:
    For each Enemy in AllEnemies:
        Observe(Enemy)
OnObserve:
    Fervor += 10

```

**GUI Display**:

- Tooltip: "Scholar of Corruption (Rank 2): +10 Fervor on Observe. Auto-observe all enemies at combat start."
- Color: Silver border
- **Rank-up notification**: "Scholar of Corruption has reached Rank 2! Auto-analysis enabled!"

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- All Rank 2 effects
- **NEW**: See exact enemy HP values
- **NEW**: Mechanical/Undying below 30% HP marked [Critical Failure] (guaranteed crit)

**Formula**:

```
Target.ShowHP = true
If (Target.Type == "Mechanical" OR Target.Type == "Undying"):
    If (Target.HP < Target.MaxHP * 0.30):
        Target.AddStatus("CriticalFailure")
        // All attacks against target auto-crit

```

**GUI Display**:

- Tooltip: "Scholar of Corruption (Rank 3): See enemy HP. Mech/Undying below 30% HP: guaranteed crits."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedIronBaneTier1()`
- [ ]  Combat: Observe action
- [ ]  Combat: Enemy identification system
- [ ]  GUI: Enemy info panel with revealed data

---

### 4.2 Purifying Flame (ID: 1102)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 0 PP (free with specialization) |
| **Ranks** | 3 |
| **Resource Cost** | 35 Stamina |
| **Attribute Used** | WILL |
| **Damage Type** | Fire |

### Description

Holy fire cleanses corrupted iron. Your flame burns hotter against the abominations.

### Rank Details

### Rank 1 (Unlocked: When specialization is chosen)

**Mechanical Effect**:

- Roll: WILL + 2 bonus dice vs Target Defense
- Damage: 2d8 Fire damage
- Vs Mechanical/Undying: +2d6 bonus Fire damage, +15 Fervor

**Formulas**:

```
AttackRoll = Roll(WILL + 2) >= 2 successes
Damage = Roll(2d8)
If (Target.Type == "Mechanical" OR Target.Type == "Undying"):
    Damage += Roll(2d6)
    Fervor += 15

```

**GUI Display**:

- Ability button: Holy flame eruption
- Tooltip: "Purifying Flame (Rank 1): 2d8 Fire. Vs Mech/Undying: +2d6, +15 Fervor. Cost: 35 Stamina"
- Color: Bronze border

---

### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:

- Bonus vs Mech/Undying: +3d6 damage
- Fervor generation: +20
- Resource cost reduced to 30 Stamina

**Formulas**:

```
BonusDamage = Roll(3d6) (vs Mech/Undying)
FervorGained = 20 (vs Mech/Undying)
StaminaCost = 30

```

**GUI Display**:

- Tooltip: "Purifying Flame (Rank 2): Vs Mech/Undying: +3d6, +20 Fervor. Cost: 30 Stamina"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Bonus vs Mech/Undying: +4d6 damage
- Fervor generation: +25
- **NEW**: Apply [Burning] (1d6 Fire/turn for 3 turns)

**Formulas**:

```
BonusDamage = Roll(4d6) (vs Mech/Undying)
FervorGained = 25 (vs Mech/Undying)
Target.AddStatus("Burning", DamagePerTurn: Roll(1d6), Duration: 3)

```

**GUI Display**:

- Tooltip: "Purifying Flame (Rank 3): Vs Mech/Undying: +4d6, +25 Fervor. Applies [Burning] 1d6/turn for 3 turns."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedIronBaneTier1()`
- [ ]  Combat: Enemy type detection
- [ ]  Status Effect: [Burning] implementation

---

### 4.3 Weakness Exploiter (ID: 1103)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 0 PP (free with specialization) |
| **Ranks** | 3 |
| **Resource Cost** | None (passive) |
| **Synergy** | Scholar of Corruption |

### Description

Every system has a flaw. Every machine has a breaking point. You know exactly where to strike.

### Rank Details

### Rank 1 (Unlocked: When specialization is chosen)

**Mechanical Effect**:

- +25% damage against enemies identified by Scholar of Corruption

**Formula**:

```
If (Target.IsIdentified):
    Damage *= 1.25

```

**GUI Display**:

- Passive icon: Target reticle with damage symbol
- Tooltip: "Weakness Exploiter (Rank 1): +25% damage vs identified enemies"
- Color: Bronze border

---

### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:

- Damage bonus: +35%
- **NEW**: +1 to hit vs identified enemies

**Formulas**:

```
If (Target.IsIdentified):
    Damage *= 1.35
    AttackBonus += 1

```

**GUI Display**:

- Tooltip: "Weakness Exploiter (Rank 2): +35% damage, +1 to hit vs identified enemies"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Damage bonus: +50%
- **NEW**: Critical hits deal triple damage instead of double

**Formulas**:

```
If (Target.IsIdentified):
    Damage *= 1.50
    CriticalMultiplier = 3.0 (instead of 2.0)

```

**GUI Display**:

- Tooltip: "Weakness Exploiter (Rank 3): +50% damage. Crits deal triple damage vs identified enemies."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedIronBaneTier1()`
- [ ]  Combat: Identified enemy tracking
- [ ]  Combat: Critical multiplier modification

---

## 5. Tier 2 Abilities (Rank 2→3 Progression)

---

### 5.1 System Shutdown (ID: 1104)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Mechanical/Undying)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Iron-Bane tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 40 Stamina + 30 Fervor |
| **Cooldown** | 3 turns |
| **Target Restriction** | Mechanical or Undying only |
| **Status Effect** | [Stunned] |

### Description

You strike at the central processor, the corrupted core. Their systems crash. They stand frozen, helpless.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Damage: 4d10 Fire damage
- Target makes WILL save DC 17
- Failed save: [Stunned] for 2 turns
- **NEW**: Failed save also applies -3 to all actions for rest of combat
- Only affects Mechanical/Undying enemies

**Formulas**:

```
Requires: Target.Type == "Mechanical" OR Target.Type == "Undying"
Damage = Roll(4d10)
If (Target.WILLSave < 17):
    Target.AddStatus("Stunned", Duration: 2)
    Target.AddStatus("SystemDamage", ActionPenalty: -3, Duration: COMBAT)

```

**GUI Display**:

- Ability button: Circuit breaker with sparks
- Tooltip: "System Shutdown (Rank 2): 4d10 Fire. WILL DC 17 or Stunned 2 turns + -3 actions (permanent). Mech/Undying only. Cost: 40 Stamina, 30 Fervor"
- Color: Silver border
- Disabled if target is not Mechanical/Undying

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Damage: 5d10 Fire damage
- Stun duration: 3 turns
- **NEW**: Failed save adds [System Malfunction] (30% chance to skip turn each round)

**Formulas**:

```
Damage = Roll(5d10)
StunDuration = 3
If (SaveFailed):
    Target.AddStatus("SystemMalfunction", SkipChance: 0.30, Duration: COMBAT)

```

**GUI Display**:

- Tooltip: "System Shutdown (Rank 3): 5d10 Fire. 3 turn Stun. [System Malfunction]: 30% skip turn."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedIronBaneTier2()`
- [ ]  Combat: Target type restriction
- [ ]  Status Effect: [System Malfunction] implementation

---

### 5.2 Critical Strike (ID: 1105)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Mechanical/Undying)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Iron-Bane tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 45 Stamina + 25 Fervor |
| **Cooldown** | 4 turns |
| **Target Restriction** | Mechanical or Undying only |
| **Special** | Guaranteed Critical Hit |

### Description

You've identified the critical failure point. One precise strike and their entire system collapses.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Damage: 5d8 Fire damage
- Guaranteed critical hit vs Mechanical/Undying (applies Weakness Exploiter bonuses)
- **NEW**: Target loses 1 action next turn

**Formulas**:

```
Requires: Target.Type == "Mechanical" OR Target.Type == "Undying"
Damage = Roll(5d8) * CriticalMultiplier  // 2x or 3x with Weakness Exploiter R3
Target.ActionsNextTurn -= 1

```

**GUI Display**:

- Ability button: Precise strike at core
- Tooltip: "Critical Strike (Rank 2): 5d8 Fire. Guaranteed crit vs Mech/Undying. Target loses 1 action. Cost: 45 Stamina, 25 Fervor"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Damage: 6d8 Fire damage
- **NEW**: If target is below 40% HP: instant death (execute)

**Formulas**:

```
Damage = Roll(6d8) * CriticalMultiplier
If (Target.HP < Target.MaxHP * 0.40):
    Target.HP = 0  // Execute

```

**GUI Display**:

- Tooltip: "Critical Strike (Rank 3): 6d8 Fire. Guaranteed crit. EXECUTE below 40% HP."
- Color: Gold border
- Execute indicator when target HP < 40%

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedIronBaneTier2()`
- [ ]  Combat: Guaranteed crit implementation
- [ ]  Combat: Execute threshold check

---

### 5.3 Flame Ward (ID: 1106)

**Type**: Passive | **Action**: Free Action | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Iron-Bane tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | None (passive) |
| **Trigger** | When hit by melee attack |

### Description

You are wreathed in holy flame. The corrupted dare not touch you. Those who try burn.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- 75% Fire resistance
- Mechanical/Undying melee attackers take 1d8 Fire damage
- **NEW**: +2 Soak vs Mechanical/Undying attacks

**Formulas**:

```
FireResistance = 0.75
OnMeleeHit:
    If (Attacker.Type == "Mechanical" OR Attacker.Type == "Undying"):
        Attacker.TakeDamage(Roll(1d8), "Fire")
        SoakBonus += 2

```

**GUI Display**:

- Passive icon: Flame aura shield
- Tooltip: "Flame Ward (Rank 2): 75% Fire resist. Mech/Undying melee attackers take 1d8 Fire. +2 Soak vs Mech/Undying."
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Fire immunity (100% resistance)
- Retaliation: 1d10 Fire damage
- **NEW**: Generate +10 Fervor when hit by Mechanical/Undying

**Formulas**:

```
FireResistance = 1.00  // Immunity
RetaliationDamage = Roll(1d10)
OnMeleeHitByMechUndying:
    Fervor += 10

```

**GUI Display**:

- Tooltip: "Flame Ward (Rank 3): Fire IMMUNE. 1d10 retaliation. +10 Fervor when hit by Mech/Undying."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedIronBaneTier2()`
- [ ]  Combat: Resistance system
- [ ]  Combat: Retaliation damage

---

## 6. Tier 3 Abilities (Rank 2→3 Progression)

---

### 6.1 Purging Flame (ID: 1107)

**Type**: Active | **Action**: Standard Action | **Target**: All Enemies (Front Row)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Iron-Bane tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 55 Stamina + 40 Fervor |
| **Cooldown** | 5 turns |
| **Status Effect** | [Burning] |

### Description

A wave of cleansing fire washes over the battlefield. Corrupted metal screams as it melts.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Damage: 5d8 Fire to all enemies in Front Row
- Mechanical/Undying: Take double damage
- Mechanical/Undying: [Burning] 2d6/turn for 4 turns (cannot be cleansed)

**Formulas**:

```
For each Enemy in FrontRow:
    Damage = Roll(5d8)
    If (Enemy.Type == "Mechanical" OR Enemy.Type == "Undying"):
        Damage *= 2
        Enemy.AddStatus("Burning", DamagePerTurn: Roll(2d6), Duration: 4, CanCleanse: false)

```

**GUI Display**:

- Ability button: Wave of holy fire
- Tooltip: "Purging Flame (Rank 2): 5d8 Fire to Front Row. Mech/Undying: 2x damage, [Burning] 2d6/turn for 4 turns (uncleansable). Cost: 55 Stamina, 40 Fervor"
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- Damage: 6d8 Fire
- **NEW**: Mechanical/Undying also gain [Vulnerable] (+50% damage taken) for 2 turns

**Formulas**:

```
Damage = Roll(6d8)
If (Enemy.Type == "Mechanical" OR Enemy.Type == "Undying"):
    Damage *= 2
    Enemy.AddStatus("Vulnerable", DamageIncrease: 0.50, Duration: 2)

```

**GUI Display**:

- Tooltip: "Purging Flame (Rank 3): 6d8 Fire. Mech/Undying: 2x damage, [Burning], [Vulnerable] +50% for 2 turns."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedIronBaneTier3()`
- [ ]  Combat: AoE targeting
- [ ]  Status Effect: Uncleansable [Burning]

---

### 6.2 Righteous Conviction (ID: 1108)

**Type**: Passive | **Action**: Free Action | **Target**: Self

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Iron-Bane tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | None (passive) |

### Description

Your faith is unshakeable. Your purpose is clear. The corrupted will fall.

### Rank Details

### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- +75% Righteous Fervor generation from all sources
- +3 dice to WILL saves vs Psychic/Corruption effects
- **NEW**: Attacks vs Mechanical/Undying cannot miss (95% minimum hit chance)

**Formulas**:

```
FervorGeneration *= 1.75
WILLSaveBonus += 3 (vs Psychic, Corruption)
If (Target.Type == "Mechanical" OR Target.Type == "Undying"):
    MinHitChance = 0.95

```

**GUI Display**:

- Passive icon: Burning conviction aura
- Tooltip: "Righteous Conviction (Rank 2): +75% Fervor. +3 WILL vs Psychic/Corruption. 95% min hit vs Mech/Undying."
- Color: Silver border

---

### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:

- +100% Fervor generation (double)
- +5 dice to WILL saves
- **NEW**: Defeating Mechanical/Undying refunds 50% ability Stamina cost

**Formulas**:

```
FervorGeneration *= 2.0
WILLSaveBonus += 5
OnKillMechUndying:
    RefundStamina = LastAbility.StaminaCost * 0.50

```

**GUI Display**:

- Tooltip: "Righteous Conviction (Rank 3): +100% Fervor. +5 WILL. 50% Stamina refund on Mech/Undying kill."
- Color: Gold border

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedIronBaneTier3()`
- [ ]  Combat: Fervor generation modifier
- [ ]  Combat: Minimum hit chance
- [ ]  Combat: Kill refund system

---

## 7. Capstone Ability

---

### 7.1 Divine Purge (ID: 1109)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Mechanical/Undying)

### Overview

| Property | Value |
| --- | --- |
| **Tier** | 4 (Capstone) |
| **PP Cost to Unlock** | 6 PP |
| **Prerequisite** | 24 PP invested + both Tier 3 abilities |
| **Ranks** | 1→2→3 |
| **Resource Cost** | 60 Stamina + 75 Fervor |
| **Cooldown** | Once per combat |
| **Target Restriction** | Mechanical or Undying only |
| **Special** | Training this ability upgrades all Tier 1, 2, & 3 abilities to Rank 3 |

### Description

You channel every lesson, every moment of study, into one perfect strike. This is not combat. This is deletion.

### Rank Details

### Rank 1 (Starting Rank - When ability is learned)

**Mechanical Effect**:

- Damage: 10d10 Fire damage
- Target makes WILL save DC 18
- Failed save: Instant death
- Passed save: Double damage + [Stunned] 2 turns

**Formulas**:

```
Requires: Target.Type == "Mechanical" OR Target.Type == "Undying"
Damage = Roll(10d10)
If (Target.WILLSave < 18):
    Target.HP = 0  // Instant death
Else:
    Damage *= 2
    Target.AddStatus("Stunned", Duration: 2)

```

**GUI Display**:

- Ability button: Divine flame annihilation
- Tooltip: "Divine Purge (Rank 1): 10d10 Fire. WILL DC 18: fail = death, pass = 2x damage + Stunned 2 turns. Mech/Undying only. Cost: 60 Stamina, 75 Fervor"
- Color: Bronze border

---

### Rank 2 (Unlocked: Based on tree progression)

**Mechanical Effect**:

- Damage: 12d10 Fire
- Save DC: 20
- **NEW**: Destroyed enemies explode for 6d6 Fire AoE (adjacent enemies)

**Formulas**:

```
Damage = Roll(12d10)
SaveDC = 20
OnTargetDeath:
    For each Adjacent Enemy:
        Enemy.TakeDamage(Roll(6d6), "Fire")

```

**GUI Display**:

- Tooltip: "Divine Purge (Rank 2): 12d10 Fire. DC 20. Destroyed enemies explode for 6d6 Fire AoE."
- Color: Silver border

---

### Rank 3 (Unlocked: Full tree completion)

**Mechanical Effect**:

- Damage: 15d10 Fire
- Save DC: 22
- Success still causes death (but target gets death save)
- **NEW**: Destruction causes [Feared] on all other Mechanical/Undying for 3 turns

**Formulas**:

```
Damage = Roll(15d10)
SaveDC = 22
// Even on save, target dies but gets death save
OnTargetDeath:
    For each Enemy where (Type == "Mechanical" OR Type == "Undying"):
        Enemy.AddStatus("Feared", Duration: 3)

```

**GUI Display**:

- Tooltip: "Divine Purge (Rank 3): 15d10 Fire. DC 22. Even saves require death save. All Mech/Undying [Feared] 3 turns on kill."
- Color: Gold border

### GUI Display - DIVINE PURGE ACTIVATION

When triggered:

```
┌─────────────────────────────────────────────┐
│            DIVINE PURGE                     │
├─────────────────────────────────────────────┤
│                                             │
│  "This is not combat. This is deletion."   │
│                                             │
│  Target: [Corrupted Automaton]              │
│  Damage: 15d10 Fire                         │
│  Save DC: 22 WILL                           │
│                                             │
│  Failure: INSTANT DEATH                     │
│  Success: Death Save Required               │
│                                             │
└─────────────────────────────────────────────┘

```

### Implementation Status

- [x]  Data seeded in `DataSeeder.SeedIronBaneCapstone()`
- [ ]  Combat: Instant death mechanic
- [ ]  Combat: Death save system
- [ ]  Combat: AoE explosion on kill
- [ ]  Status Effect: Mass [Feared] application

---

## 8. Status Effect Definitions

### 8.1 [Burning]

| Property | Value |
| --- | --- |
| **Applied By** | Purifying Flame, Purging Flame |
| **Duration** | 3-4 turns |
| **Icon** | Flames |
| **Color** | Orange-red |

**Effects**:

- Takes 1d6-2d6 Fire damage at start of turn
- Vs Mechanical/Undying (from Purging Flame): Cannot be cleansed

---

### 8.2 [Stunned]

| Property | Value |
| --- | --- |
| **Applied By** | System Shutdown, Divine Purge |
| **Duration** | 2-3 turns |
| **Icon** | Stars/sparks |
| **Color** | Yellow |

**Effects**:

- Cannot take actions
- 4 to Defense

---

### 8.3 [System Malfunction]

| Property | Value |
| --- | --- |
| **Applied By** | System Shutdown (Rank 3) |
| **Duration** | Rest of combat |
| **Icon** | Error symbol |
| **Color** | Red glitch |

**Effects**:

- 30% chance to skip turn each round
- Mechanical/Undying only

---

### 8.4 [Critical Failure]

| Property | Value |
| --- | --- |
| **Applied By** | Scholar of Corruption (Rank 3) |
| **Duration** | Until HP > 30% |
| **Icon** | Target with X |
| **Color** | Red |

**Effects**:

- All attacks against target automatically critical hit
- Only applies to Mechanical/Undying below 30% HP

---

### 8.5 [Vulnerable]

| Property | Value |
| --- | --- |
| **Applied By** | Purging Flame (Rank 3) |
| **Duration** | 2 turns |
| **Icon** | Cracked armor |
| **Color** | Orange |

**Effects**:

- +50% damage taken from all sources

---

## 9. GUI Requirements

### 9.1 Righteous Fervor Bar

The Iron-Bane requires a **Righteous Fervor resource bar** displayed in combat:

```
┌─────────────────────────────────────────┐
│  FERVOR [████████████░░░░░░░░] 60/100   │
│  :fire: Knowledge Burns the Corrupted            │
└─────────────────────────────────────────┘

```

- Color: Holy golden-orange gradient
- Breakpoints at 25/30/40/75 for ability thresholds

### 9.2 Enemy Analysis Display

When Scholar of Corruption reveals enemy info:

```
┌─────────────────────────────────────────┐
│  [Corrupted Automaton]                  │
│  Type: MECHANICAL                       │
│  HP: 45/120 (37%)                       │
│  ─────────────────────────              │
│  Resistances: Physical 25%              │
│  Vulnerabilities: Fire, Electricity    │
│  Weakness: Core processor exposed       │
│  ─────────────────────────              │
│  [CRITICAL FAILURE] - Guaranteed Crits  │
└─────────────────────────────────────────┘

```

### 9.3 Ability Card Rank Indicators

| Rank | Border Color | Badge |
| --- | --- | --- |
| 1 | Bronze (#CD7F32) | "I" |
| 2 | Silver (#C0C0C0) | "II" |
| 3 | Gold (#FFD700) | "III" |

---

## 10. Implementation Priority

### Phase 1: Critical (Foundation)

1. **Implement Righteous Fervor resource** - 0-100 scale with generation/decay
2. **Implement enemy type system** - Mechanical, Undying, Other
3. **Implement Observe action** - Enemy identification
4. **Implement rank calculation logic** based on tree progression

### Phase 2: Combat Integration

1. **Route Iron-Bane abilities** through CombatEngine
2. **Implement type-based damage bonuses** for Mechanical/Undying
3. **Add Fervor bar** to CombatView
4. **Implement target restrictions** for abilities

### Phase 3: Status Effects

1. **Define status effects** ([Burning], [System Malfunction], [Critical Failure])
2. **Implement uncleansable Burning** for Mechanical/Undying
3. **Implement instant death mechanics** (Divine Purge)

### Phase 4: Advanced Features

1. **Implement execute threshold** (Critical Strike Rank 3)
2. **Implement AoE explosion on kill** (Divine Purge Rank 2)
3. **Implement kill refund system** (Righteous Conviction)

### Phase 5: Polish

1. **Add rank-specific icons** (Bronze/Silver/Gold)
2. **Implement enemy analysis panel**
3. **Add Divine Purge activation notification**

---

**End of Specification**