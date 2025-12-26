# Berserkr Specialization - Complete Specification

> **Specification ID**: SPEC-SPECIALIZATION-BERSERKR
> **Version**: 1.0
> **Last Updated**: 2025-11-27
> **Status**: Draft - Implementation Review

---

## Document Control

### Purpose
This document provides the complete specification for the Berserkr specialization, including:
- Design philosophy and mechanical identity
- All 9 abilities with **exact formulas per rank**
- **Rank unlock requirements** (tree-progression based, NOT PP-based)
- **GUI display specifications per rank**
- Current implementation status
- Combat system integration points

### Related Files
| Component | File Path | Status |
|-----------|-----------|--------|
| Data Seeding | `RuneAndRust.Persistence/BerserkrSeeder.cs` | Implemented |
| Specialization Factory | `RuneAndRust.Engine/SpecializationFactory.cs` | Partial |
| Tests | N/A | Not Yet Implemented |
| Specialization Tree UI | `RuneAndRust.DesktopUI/Views/SpecializationTreeView.axaml` | Generic |
| Combat UI | `RuneAndRust.DesktopUI/Views/CombatView.axaml` | No specialization integration |

### Change Log
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-11-27 | Initial specification from BerserkrSeeder |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
|----------|-------|
| **Internal Name** | Berserkr |
| **Display Name** | Berserkr |
| **Specialization ID** | 26001 |
| **Archetype** | Warrior (ArchetypeID = 1) |
| **Path Type** | Heretical |
| **Mechanical Role** | Melee Damage Dealer / Fury Fighter |
| **Primary Attribute** | MIGHT |
| **Secondary Attribute** | STURDINESS |
| **Resource System** | Stamina + Fury (0-100) |
| **Trauma Risk** | High |
| **Icon** | :fire: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 10 PP | Higher than standard 3 PP (reflects power level) |
| **Minimum Legend** | 5 | Mid-game specialization |
| **Maximum Corruption** | 100 | No corruption restriction |
| **Minimum Corruption** | 0 | No minimum corruption |
| **Required Quest** | None | No quest prerequisite |

### 1.3 Design Philosophy

**Tagline**: "The Roaring Fire"

**Core Fantasy**: The Berserkr embodies the heretical warrior who channels the world's trauma into pure, untamed physical power. They are roaring fires of destruction who open their minds to the violent psychic static of the Great Silence, transforming that chaos into battle-lust that pushes their bodies beyond normal limits.

**Mechanical Identity**:
1. **Fury Resource System**: Builds Fury (0-100) by dealing and taking damage, creating a dangerous feedback loop
2. **High-Risk, High-Reward**: More dangerous as combat intensifies, but more mentally vulnerable
3. **WILL Penalty While Holding Fury**: -2 dice to WILL Resolve Checks, making vulnerable to Fear, psychic attacks, and mental status effects
4. **AoE and Burst Damage**: Emphasis on wide-reaching destruction and devastating single-target strikes

### 1.4 The Fury Resource System

**Fury Mechanics**:
- **Range**: 0-100
- **Generation**: Gained by dealing damage and taking damage
- **Decay**: Decays slowly out of combat
- **WILL Penalty**: While holding ANY Fury, suffer -2 dice penalty to WILL Resolve Checks

**Fury Generation**:
| Source | Base Fury Gained |
|--------|------------------|
| Dealing HP damage | 1 Fury per 5 damage dealt |
| Taking HP damage | 1 Fury per 1 HP damage taken |
| Killing an enemy | +10 Fury |

### 1.5 Specialization Description (Full Text)

> The Berserkr embodies the heretical warrior who channels the world's trauma into pure, untamed physical power. They are roaring fires of destruction who open their minds to the violent psychic static of the Great Silence, transforming that chaos into battle-lust that pushes their bodies beyond normal limits.
>
> Playing a Berserkr means embracing high-risk, high-reward gameplay—becoming more dangerous as combat intensifies, but also more mentally vulnerable. Your Fury resource (0-100) is gained by dealing damage and taking damage, creating a dangerous feedback loop where pain fuels power.
>
> However, the Trauma Economy exacts its toll: while holding any Fury, you suffer -2 dice penalty to WILL Resolve Checks, making you vulnerable to Fear, psychic attacks, and mental status effects.

---

## 2. Rank Progression System

### 2.1 CRITICAL: Rank Unlock Rules

**Ranks are unlocked through TREE PROGRESSION, not PP spending.**

| Tier | Starting Rank | Progresses To | Rank 3 Trigger |
|------|---------------|---------------|----------------|
| **Tier 1** | Rank 1 (when learned) | Rank 2 (when 2 Tier 2 trained) | Capstone trained |
| **Tier 2** | Rank 2 (when learned) | Rank 3 (when Capstone trained) | Capstone trained |
| **Tier 3** | No ranks | N/A | N/A |
| **Capstone** | No ranks | N/A | N/A |

**Important Notes**:
- **Tier 1 abilities** start at Rank 1, progress to Rank 2 when 2 Tier 2 abilities are trained, and reach Rank 3 when Capstone is trained
- **Tier 2 abilities** start at Rank 2 (reflecting their higher tier), and progress to Rank 3 when Capstone is trained
- **Tier 3 and Capstone abilities** do NOT have ranks - they are powerful single-effect abilities
- Rank progression is **automatic** when requirements are met (no additional PP cost)

### 2.2 Ability Structure by Tier

| Tier | Abilities | PP Cost to Unlock | Starting Rank | Max Rank | Rank Progression |
|------|-----------|-------------------|---------------|----------|------------------|
| **Tier 1** | 3 | 3 PP each | 1 | 3 | 1→2 (2× Tier 2), 2→3 (Capstone) |
| **Tier 2** | 3 | 4 PP each | 2 | 3 | 2→3 (Capstone) |
| **Tier 3** | 2 | 5 PP each | N/A | N/A | No ranks |
| **Capstone** | 1 | 6 PP | N/A | N/A | No ranks |

### 2.3 Total PP Investment

| Milestone | PP Spent | Abilities Unlocked | Tier 1 Rank | Tier 2 Rank |
|-----------|----------|-------------------|-------------|-------------|
| Unlock Specialization | 10 PP | 0 | - | - |
| All Tier 1 | 10 + 9 = 19 PP | 3 Tier 1 | Rank 1 | - |
| 2 Tier 2 | 19 + 8 = 27 PP | 3 Tier 1 + 2 Tier 2 | **Rank 2** | Rank 2 |
| All Tier 2 | 27 + 4 = 31 PP | 3 Tier 1 + 3 Tier 2 | Rank 2 | Rank 2 |
| All Tier 3 | 31 + 10 = 41 PP | 3 Tier 1 + 3 Tier 2 + 2 Tier 3 | Rank 2 | Rank 2 |
| Capstone | 41 + 6 = 47 PP | All 9 abilities | **Rank 3** | **Rank 3** |

---

## 3. Ability Tree Overview

### 3.1 Visual Tree Structure

```
                    TIER 1: FOUNDATION (3 PP each, Ranks 1-3)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Primal Vigor]      [Wild Swing]        [Reckless Assault]
 (Passive)            (Active)             (Active)
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
[Unleashed Roar]   [Whirlwind of      [Blood-Fueled]
    (Active)        Destruction]         (Passive)
                      (Active)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
   [Hemorrhaging           [Death or Glory]
     Strike]                  (Passive)
     (Active)
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
              [Unstoppable Fury]
                 (Passive)
```

### 3.2 Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Resource Cost | Key Effect |
|----|--------------|------|------|-------|---------------|------------|
| 26001 | Primal Vigor | 1 | Passive | 1→2→3 | None | +Stamina regen per 25 Fury |
| 26002 | Wild Swing | 1 | Active | 1→2→3 | 40 Stamina | AoE Front Row damage + Fury |
| 26003 | Reckless Assault | 1 | Active | 1→2→3 | 35 Stamina | High damage + Fury + [Vulnerable] self |
| 26004 | Unleashed Roar | 2 | Active | 2→3 | 30 Stamina + 20 Fury | Taunt + Fury on being attacked |
| 26005 | Whirlwind of Destruction | 2 | Active | 2→3 | 50 Stamina + 30 Fury | AoE ALL enemies damage |
| 26006 | Blood-Fueled | 2 | Passive | 2→3 | None | Doubles Fury from damage taken |
| 26007 | Hemorrhaging Strike | 3 | Active | — | 45 Stamina + 40 Fury | Massive damage + [Bleeding] |
| 26008 | Death or Glory | 3 | Passive | — | None | +50% Fury gen while [Bloodied] |
| 26009 | Unstoppable Fury | 4 | Passive | — | None | Fear/Stun immunity + death prevention |

---

## 4. Tier 1 Abilities (Detailed Rank Specifications)

These abilities have 3 ranks. Rank progression is automatic based on tree investment.

---

### 4.1 Primal Vigor (ID: 26001)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | None (passive) |
| **Attribute Used** | None |

#### Description
The Berserkr's physiology is tied to their rage. As fury builds, their body surges with adrenaline, accelerating stamina recovery.

#### Rank Details

##### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:
- For every 25 Fury, gain +2 Stamina regeneration per turn
- Breakpoints: 25 Fury = +2, 50 Fury = +4, 75 Fury = +6, 100 Fury = +8

**Formula**:
```
StaminaRegen = Floor(Fury / 25) * 2
```

**GUI Display**:
- Passive icon: Flame with stamina symbol
- Tooltip: "Primal Vigor (Rank 1): +2 Stamina regen per 25 Fury (max +8 at 100 Fury)"
- Color: Bronze border

**Combat Log Examples**:
- "Primal Vigor grants +4 Stamina regen (50 Fury)"
- "Stamina regenerates 6 (base 2 + Primal Vigor 4)"

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- Bonus increases to +3 Stamina regen per 25 Fury
- Breakpoints: 25 Fury = +3, 50 Fury = +6, 75 Fury = +9, 100 Fury = +12

**Formula**:
```
StaminaRegen = Floor(Fury / 25) * 3
```

**GUI Display**:
- Passive icon: Brighter flame with stamina symbol
- Tooltip: "Primal Vigor (Rank 2): +3 Stamina regen per 25 Fury (max +12 at 100 Fury)"
- Color: Silver border
- **Rank-up notification**: "Primal Vigor has reached Rank 2!"

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Bonus increases to +4 Stamina regen per 25 Fury
- Breakpoints: 25 Fury = +4, 50 Fury = +8, 75 Fury = +12, 100 Fury = +16

**Formula**:
```
StaminaRegen = Floor(Fury / 25) * 4
```

**GUI Display**:
- Passive icon: Blazing flame with stamina symbol
- Tooltip: "Primal Vigor (Rank 3): +4 Stamina regen per 25 Fury (max +16 at 100 Fury)"
- Color: Gold border
- **Rank-up notification**: "Primal Vigor has reached Rank 3!"

#### Implementation Status
- [x] Data seeded in `BerserkrSeeder.SeedBerserkrTier1()`
- [ ] **FIX NEEDED**: Seeder has `CostToRank2 = 20` - should be removed/ignored
- [ ] GUI: Passive indicator with rank-specific icon
- [ ] GUI: Rank border color (Bronze/Silver/Gold)
- [ ] Combat: Stamina regen integration with Fury system

---

### 4.2 Wild Swing (ID: 26002)

**Type**: Active | **Action**: Standard Action | **Target**: AoE Front Row

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 40 Stamina |
| **Attribute Used** | MIGHT |
| **Damage Type** | Physical |

#### Description
The Berserkr unleashes a wide, reckless swing, caring little for precision and focusing only on widespread destruction.

#### Rank Details

##### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:
- Damage: 2d8 + MIGHT Physical damage to all enemies in Front Row
- Generates +5 Fury per enemy hit

**Formulas**:
```
Damage = Roll(2d8) + MIGHT (per target)
FuryGained = 5 * EnemiesHit
```

**GUI Display**:
- Ability button: Sweeping weapon arc
- Tooltip: "Wild Swing (Rank 1): 2d8+MIGHT damage to all Front Row. +5 Fury per hit. Cost: 40 Stamina"
- Color: Bronze border

**Combat Log Examples**:
- "Wild Swing hits 3 enemies! 11 damage (2d8[4,3] + MIGHT[4])"
- "Wild Swing generates +15 Fury (5 × 3 enemies)"

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- Damage: 2d10 + MIGHT Physical damage
- Generates +7 Fury per enemy hit

**Formulas**:
```
Damage = Roll(2d10) + MIGHT (per target)
FuryGained = 7 * EnemiesHit
```

**GUI Display**:
- Tooltip: "Wild Swing (Rank 2): 2d10+MIGHT damage to all Front Row. +7 Fury per hit. Cost: 40 Stamina"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Damage: 3d8 + MIGHT Physical damage
- Generates +10 Fury per enemy hit
- Resource cost reduced to 35 Stamina
- **NEW**: Can hit Back Row if Front Row is clear

**Formulas**:
```
Damage = Roll(3d8) + MIGHT (per target)
FuryGained = 10 * EnemiesHit
If (FrontRow.Empty):
    Target = BackRow
```

**GUI Display**:
- Tooltip: "Wild Swing (Rank 3): 3d8+MIGHT damage to Front Row (or Back if empty). +10 Fury per hit. Cost: 35 Stamina"
- Color: Gold border
- **Rank-up notification**: "Wild Swing has reached Rank 3! Now hits Back Row when Front is empty!"

#### Implementation Status
- [x] Data seeded in `BerserkrSeeder.SeedBerserkrTier1()`
- [ ] **FIX NEEDED**: Seeder has `CostToRank2 = 20` - should be removed/ignored
- [ ] GUI: Ability button with rank-specific icon
- [ ] Combat: AoE targeting system
- [ ] Combat: Fury generation integration

---

### 4.3 Reckless Assault (ID: 26003)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Melee)

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 35 Stamina |
| **Attribute Used** | MIGHT |
| **Damage Type** | Physical |
| **Self-Debuff** | [Vulnerable] |

#### Description
Lowering their guard completely, the Berserkr lunges forward to deliver a powerful single-target attack.

#### Rank Details

##### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:
- Damage: 3d10 + MIGHT Physical damage
- Generates +15 Fury
- Applies [Vulnerable] to self for 1 round (+25% damage taken)

**Formulas**:
```
Damage = Roll(3d10) + MIGHT
FuryGained = 15
Self.AddStatus("Vulnerable", DamageBonus: 1.25, Duration: 1)
```

**GUI Display**:
- Ability button: Lunging attack with exposed stance
- Tooltip: "Reckless Assault (Rank 1): 3d10+MIGHT damage. +15 Fury. You become Vulnerable (+25% damage taken) for 1 round. Cost: 35 Stamina"
- Warning indicator: Shows self-debuff
- Color: Bronze border

**Combat Log Examples**:
- "Reckless Assault deals 24 damage! (3d10[8,6,6] + MIGHT[4]) +15 Fury"
- "You are now Vulnerable (+25% damage taken)"

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- Damage: 4d10 + MIGHT Physical damage
- Generates +18 Fury
- [Vulnerable] reduced to +20% damage taken

**Formulas**:
```
Damage = Roll(4d10) + MIGHT
FuryGained = 18
Self.AddStatus("Vulnerable", DamageBonus: 1.20, Duration: 1)
```

**GUI Display**:
- Tooltip: "Reckless Assault (Rank 2): 4d10+MIGHT damage. +18 Fury. Vulnerable (+20% damage) for 1 round. Cost: 35 Stamina"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Damage: 5d10 + MIGHT Physical damage
- Generates +20 Fury
- [Vulnerable] reduced to +15% damage taken
- Resource cost reduced to 30 Stamina
- **NEW**: If this attack kills the target, [Vulnerable] is NOT applied

**Formulas**:
```
Damage = Roll(5d10) + MIGHT
FuryGained = 20
If (Target.HP <= 0):
    // No Vulnerable applied
Else:
    Self.AddStatus("Vulnerable", DamageBonus: 1.15, Duration: 1)
```

**GUI Display**:
- Tooltip: "Reckless Assault (Rank 3): 5d10+MIGHT damage. +20 Fury. Vulnerable (+15%) unless target dies. Cost: 30 Stamina"
- Color: Gold border
- **Rank-up notification**: "Reckless Assault has reached Rank 3! No Vulnerable on kills!"

#### Implementation Status
- [x] Data seeded in `BerserkrSeeder.SeedBerserkrTier1()`
- [ ] **FIX NEEDED**: Seeder has `CostToRank2 = 20` - should be removed/ignored
- [ ] Status Effect: [Vulnerable] definition
- [ ] Combat: Kill detection for Rank 3 bonus

---

## 5. Tier 2 Abilities (Rank 2→3 Progression)

These abilities start at **Rank 2** when trained and progress to **Rank 3** when the Capstone is trained.

---

### 5.1 Unleashed Roar (ID: 26004)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Berserkr tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 30 Stamina + 20 Fury |
| **Status Effects** | [Taunted] |

#### Description
The Berserkr lets out a terrifying, guttural war cry, challenging a single foe to face their wrath.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Taunts single enemy for 2 rounds
- Gain +10 Fury each time the taunted enemy attacks you

**Formulas**:
```
Target.AddStatus("Taunted", Duration: 2, TauntSource: Berserkr)
OnTauntedAttack: Berserkr.Fury += 10
```

**GUI Display**:
- Ability button: Roaring figure with sound waves
- Tooltip: "Unleashed Roar (Rank 2): Taunt enemy for 2 rounds. +10 Fury when they attack you. Cost: 30 Stamina, 20 Fury"
- Color: Silver border

**Combat Log Examples**:
- "Unleashed Roar! [Enemy] is Taunted for 2 rounds"
- "[Enemy] attacks you! +10 Fury (Unleashed Roar)"

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Cost reduced to 25 Stamina + 15 Fury
- Taunt duration increased to 3 rounds
- Fury refund increased to +15 per attack
- **NEW**: First attack from taunted enemy deals -20% damage
- **NEW**: If taunted enemy dies, gain [Empowered] for 1 round

**Formulas**:
```
Target.AddStatus("Taunted", Duration: 3, TauntSource: Berserkr, FirstAttackPenalty: 0.80)
OnTauntedAttack: Berserkr.Fury += 15
OnTauntedDeath: Berserkr.AddStatus("Empowered", Duration: 1)
```

**GUI Display**:
- Tooltip: "Unleashed Roar (Rank 3): Taunt for 3 rounds. +15 Fury when attacked. First attack -20% damage. [Empowered] on kill. Cost: 25 Stamina, 15 Fury"
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `BerserkrSeeder.SeedBerserkrTier2()`
- [ ] Status Effect: [Taunted] with damage penalty
- [ ] Status Effect: [Empowered] definition
- [ ] Combat: Taunt attack tracking

---

### 5.2 Whirlwind of Destruction (ID: 26005)

**Type**: Active | **Action**: Standard Action | **Target**: AoE All Enemies

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Berserkr tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 50 Stamina + 30 Fury |
| **Damage Type** | Physical |

#### Description
A spinning vortex of pure destruction that reaches across the entire battlefield.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Damage: 3d8 + MIGHT Physical damage to ALL enemies (Front + Back rows)

**Formula**:
```
For each Enemy in AllEnemies:
    Damage = Roll(3d8) + MIGHT
```

**GUI Display**:
- Ability button: Spinning warrior with weapon trails
- Tooltip: "Whirlwind of Destruction (Rank 2): 3d8+MIGHT to ALL enemies. Cost: 50 Stamina, 30 Fury"
- Color: Silver border

**Combat Log Examples**:
- "Whirlwind of Destruction! 5 enemies take damage!"
- "[Enemy1] takes 18 damage, [Enemy2] takes 15 damage..."

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Damage: 5d8 + MIGHT Physical damage
- Cost reduced to 45 Stamina + 25 Fury
- **NEW**: Each enemy killed refunds +8 Fury
- **NEW**: If kills 3+ enemies, gain [Fortified] for 2 rounds

**Formulas**:
```
For each Enemy in AllEnemies:
    Damage = Roll(5d8) + MIGHT
KillCount = Count enemies killed
Berserkr.Fury += KillCount * 8
If (KillCount >= 3):
    Berserkr.AddStatus("Fortified", Duration: 2)
```

**GUI Display**:
- Tooltip: "Whirlwind of Destruction (Rank 3): 5d8+MIGHT to ALL enemies. +8 Fury per kill. [Fortified] if 3+ kills. Cost: 45 Stamina, 25 Fury"
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `BerserkrSeeder.SeedBerserkrTier2()`
- [ ] Combat: All-enemy targeting
- [ ] Combat: Kill count tracking
- [ ] Status Effect: [Fortified] definition

---

### 5.3 Blood-Fueled (ID: 26006)

**Type**: Passive | **Action**: Free Action | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Berserkr tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | None (passive) |

#### Description
Pain is a catalyst. Every wound is an invitation to greater violence. The Berserkr has learned to transform suffering into power.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Doubles Fury gained from taking damage (1 HP damage = 2 Fury)

**Formula**:
```
FuryFromDamage = DamageTaken * 2
```

**GUI Display**:
- Passive icon: Blood drop with flame
- Tooltip: "Blood-Fueled (Rank 2): Double Fury from damage taken (10 HP = 20 Fury)"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Fury from damage increased to 3x (10 HP = 30 Fury)
- **NEW**: Also gain +1 Stamina per 5 damage taken

**Formulas**:
```
FuryFromDamage = DamageTaken * 3
StaminaGained = Floor(DamageTaken / 5)
```

**GUI Display**:
- Tooltip: "Blood-Fueled (Rank 3): Triple Fury from damage (10 HP = 30 Fury). +1 Stamina per 5 damage."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `BerserkrSeeder.SeedBerserkrTier2()`
- [ ] Combat: Damage-to-Fury conversion modifier
- [ ] Combat: Damage-to-Stamina conversion (Rank 3)

---

## 6. Tier 3 Abilities (No Ranks)

Tier 3 abilities are powerful effects that do **not** have rank progression.

---

### 6.1 Hemorrhaging Strike (ID: 26007)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Melee)

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 20 PP invested in Berserkr tree |
| **Ranks** | None (full power when unlocked) |
| **Resource Cost** | 45 Stamina + 40 Fury |
| **Status Effects** | [Bleeding] |

#### Description
Focusing their rage into a single savage blow, the Berserkr opens a grievous injury that will bleed the enemy dry.

#### Mechanical Effect
- Damage: 4d10 + MIGHT Physical damage
- Applies [Bleeding] (3d6 damage per turn for 3 rounds)

**Formulas**:
```
Damage = Roll(4d10) + MIGHT
Target.AddStatus("Bleeding", DamagePerTurn: Roll(3d6), Duration: 3)
```

#### GUI Display
- Ability button: Savage strike with blood spray
- Tooltip: "Hemorrhaging Strike: 4d10+MIGHT damage + [Bleeding] (3d6/turn, 3 rounds). Cost: 45 Stamina, 40 Fury"
- Self-buff icon: Blood droplets

#### Combat Log Examples
- "Hemorrhaging Strike deals 32 damage! Target is Bleeding!"
- "[Enemy] bleeds for 12 damage (Hemorrhaging Strike)"

#### Implementation Status
- [x] Data seeded in `BerserkrSeeder.SeedBerserkrTier3()`
- [ ] Status Effect: [Bleeding] with DoT
- [ ] Combat: DoT tick integration

---

### 6.2 Death or Glory (ID: 26008)

**Type**: Passive | **Action**: Free Action | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 20 PP invested in Berserkr tree |
| **Ranks** | None (full power when unlocked) |
| **Resource Cost** | None (passive) |
| **Trigger** | Below 50% HP ([Bloodied]) |

#### Description
The Berserkr fights with the greatest ferocity when on the brink of death. Desperation fuels transcendent rage.

#### Mechanical Effect
- While [Bloodied] (below 50% HP), all Fury generation increased by +50%
- Example: Reckless Assault generates 15 Fury normally, 22-23 Fury while Bloodied

**Formulas**:
```
If (Berserkr.HP < Berserkr.MaxHP * 0.5):
    FuryGeneration *= 1.5
```

#### GUI Display
- Passive icon: Skull with flame
- Tooltip: "Death or Glory: While below 50% HP, +50% Fury generation"
- Active indicator when Bloodied: Pulsing red glow

#### Combat Log Examples
- "Death or Glory: You are Bloodied! Fury generation +50%!"
- "Reckless Assault generates 22 Fury (+7 from Death or Glory)"

#### Implementation Status
- [x] Data seeded in `BerserkrSeeder.SeedBerserkrTier3()`
- [ ] Combat: Bloodied threshold detection
- [ ] Combat: Fury generation modifier

---

## 7. Capstone Ability (No Ranks)

The Capstone is a unique, powerful ability that does **not** have rank progression. When trained, it also upgrades all Tier 1 and Tier 2 abilities to Rank 3.

---

### 7.1 Unstoppable Fury (ID: 26009)

**Type**: Passive | **Action**: Free Action | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 4 (Capstone) |
| **PP Cost to Unlock** | 6 PP |
| **Prerequisite** | 40 PP invested in tree + both Tier 3 abilities |
| **Ranks** | None (full power when unlocked) |
| **Resource Cost** | None (passive) |
| **Trigger** | Reduced to 0 HP (once per combat) |
| **Special** | Training this ability upgrades all Tier 1 & Tier 2 abilities to Rank 3 |

#### Description
The Berserkr's rage transcends mere emotion and becomes a force of nature, allowing them to defy death itself through sheer will and fury.

#### Component 1: Permanent Immunities

**Mechanical Effect**:
- Immune to [Feared]
- Immune to [Stunned]

**GUI Display**:
- Passive icon: Roaring warrior with immunity aura
- Tooltip: "Unstoppable Fury: Immune to Fear and Stun"

#### Component 2: Death Prevention (Once Per Combat)

**Trigger**: When reduced to 0 HP

**Mechanical Effect**:
- HP is set to 1 instead of 0
- Immediately gain 100 Fury
- Can only trigger once per combat

**Formulas**:
```
Trigger: Berserkr.HP <= 0 AND NOT DeathPreventionUsed
If Triggered:
    Berserkr.HP = 1
    Berserkr.Fury = 100
    DeathPreventionUsed = true
```

#### GUI Display - DEATH PREVENTION

When triggered:

```
┌─────────────────────────────────────────────┐
│         UNSTOPPABLE FURY TRIGGERED!         │
├─────────────────────────────────────────────┤
│                                             │
│  [Berserkr] refuses to fall!                │
│                                             │
│  • HP set to 1                              │
│  • Fury set to 100                          │
│                                             │
│  "Pain is fuel. Death is optional."         │
│                                             │
└─────────────────────────────────────────────┘
```

- Flash notification with screen effect
- Fury bar fills to maximum
- Once-per-combat indicator appears

#### Combat Log Examples
- "Unstoppable Fury: [Berserkr] is IMMUNE to Fear!"
- "UNSTOPPABLE FURY! [Berserkr] refuses to die! HP = 1, Fury = 100!"
- "Unstoppable Fury (Death Prevention) has been used this combat"

#### Implementation Status
- [x] Data seeded in `BerserkrSeeder.SeedBerserkrCapstone()`
- [ ] Combat: [Feared] immunity
- [ ] Combat: [Stunned] immunity
- [ ] Combat: Death prevention trigger
- [ ] GUI: Death prevention notification

---

## 8. Status Effect Definitions

### 8.1 [Vulnerable]

| Property | Value |
|----------|-------|
| **Applied By** | Reckless Assault |
| **Duration** | 1 turn |
| **Icon** | Cracked shield |
| **Color** | Red |

**Effects**:
- +15%/+20%/+25% damage taken (varies by rank)

**GUI Display**:
- Icon shows damage increase percentage
- Tooltip: "Vulnerable: +X% damage taken"

---

### 8.2 [Taunted]

| Property | Value |
|----------|-------|
| **Applied By** | Unleashed Roar |
| **Duration** | 2-3 turns |
| **Icon** | Angry red arrow |
| **Color** | Red |

**Effects**:
- Must target the taunt source with attacks
- Cannot move away from taunt source

---

### 8.3 [Bleeding]

| Property | Value |
|----------|-------|
| **Applied By** | Hemorrhaging Strike |
| **Duration** | 3 rounds |
| **Icon** | Blood drops |
| **Color** | Dark red |

**Effects**:
- Takes 3d6 Physical damage at start of turn
- Cannot be healed until bleeding stops

---

### 8.4 [Empowered]

| Property | Value |
|----------|-------|
| **Applied By** | Unleashed Roar (kill trigger) |
| **Duration** | 1 turn |
| **Icon** | Power surge |
| **Color** | Yellow |

**Effects**:
- +2 dice to all damage rolls

---

### 8.5 [Fortified]

| Property | Value |
|----------|-------|
| **Applied By** | Whirlwind of Destruction (3+ kills) |
| **Duration** | 2 turns |
| **Icon** | Stone barrier |
| **Color** | Brown |

**Effects**:
- Immune to Push/Pull effects
- Immune to Knockdown

---

## 9. GUI Rank Display Requirements

### 9.1 Fury Bar

The Berserkr requires a **Fury resource bar** displayed in combat:

```
┌─────────────────────────────────────────┐
│  FURY [████████████░░░░░░░░] 60/100     │
│  ⚠️ -2 WILL Resolve while holding Fury  │
└─────────────────────────────────────────┘
```

- Color: Red-orange gradient that intensifies with Fury level
- Warning indicator when Fury > 0 showing WILL penalty
- Breakpoint indicators at 25/50/75/100 for Primal Vigor

### 9.2 Ability Card Rank Indicators

| Rank | Border Color | Badge |
|------|--------------|-------|
| 1 | Bronze (#CD7F32) | "I" |
| 2 | Silver (#C0C0C0) | "II" |
| 3 | Gold (#FFD700) | "III" |

---

## 10. Implementation Priority

### Phase 1: Critical (Foundation)
1. **Implement Fury resource system** - 0-100 scale with generation/decay
2. **Implement WILL penalty** while holding Fury
3. **Fix BerserkrSeeder.cs** - Remove/correct CostToRank2 values
4. **Implement rank calculation logic** based on tree progression

### Phase 2: Combat Integration
5. **Route Berserkr abilities** through CombatEngine
6. **Implement Fury generation** from damage dealt/taken
7. **Add Fury bar** to CombatView

### Phase 3: Status Effects
8. **Define status effects** ([Vulnerable], [Bleeding], [Empowered])
9. **Implement death prevention** (Unstoppable Fury)
10. **Implement immunity system** (Fear, Stun)

### Phase 4: Polish
11. **Add rank-specific icons** (Bronze/Silver/Gold)
12. **Implement rank-up notifications**
13. **Add death prevention notification**

---

**End of Specification**
