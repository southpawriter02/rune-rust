# Skar-Horde Aspirant Specialization - Complete Specification

> **Specification ID**: SPEC-SPECIALIZATION-SKAR-HORDE-ASPIRANT
> **Version**: 1.0
> **Last Updated**: 2025-11-27
> **Status**: Draft - Implementation Review

---

## Document Control

### Purpose
This document provides the complete specification for the Skar-Horde Aspirant specialization, including:
- Design philosophy and mechanical identity
- All 9 abilities with **exact formulas per rank**
- **Rank unlock requirements** (tree-progression based, NOT PP-based)
- **GUI display specifications per rank**
- Current implementation status
- Combat system integration points

### Related Files
| Component | File Path | Status |
|-----------|-----------|--------|
| Data Seeding | `RuneAndRust.Persistence/DataSeeder.cs` (line 918) | Implemented |
| Specialization Enum | `RuneAndRust.Core/Specialization.cs` | Defined |
| Specialization Factory | `RuneAndRust.Engine/SpecializationFactory.cs` | Referenced |
| Tests | N/A | Not Yet Implemented |

### Change Log
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-11-27 | Initial specification from DataSeeder implementation |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
|----------|-------|
| **Internal Name** | SkarHordeAspirant |
| **Display Name** | Skar-Horde Aspirant |
| **Specialization ID** | 10 |
| **Archetype** | Warrior (ArchetypeID = 1) |
| **Path Type** | Heretical |
| **Mechanical Role** | Melee DPS / Armor-Breaker |
| **Primary Attribute** | MIGHT |
| **Secondary Attribute** | None |
| **Resource System** | Stamina + Savagery (0-100) |
| **Trauma Risk** | Extreme |
| **Icon** | :crossed_swords: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 5 | Mid-game specialization |
| **Maximum Corruption** | 100 | No corruption restriction |
| **Minimum Corruption** | 0 | No minimum corruption |
| **Required Quest** | None | No quest prerequisite |

### 1.3 Design Philosophy

**Tagline**: "The Warrior Who Bleeds for Power"

**Core Fantasy**: The Skar-Horde Aspirant embodies the heretical philosophy of achieving power through savage, willful self-mutilation. You have ritualistically replaced your hand with a modular weapon-stump augment, trading humanity for devastating combat prowess. Build Savagery by fighting in melee, then unleash armor-bypassing attacks that ignore all defenses. Every strike pushes you closer to madness, but power is worth any price. You are no longer human. You are a weapon.

**Mechanical Identity**:
1. **Augmentation System**: Replaced hand with modular weapon-stump that can be swapped for different damage types
2. **Savagery Resource**: Builds through combat, required for powerful abilities
3. **Armor Bypass**: Specializes in damage that ignores enemy defenses
4. **High-Risk Trauma**: Extreme psychic cost - the power comes at the price of sanity

### 1.4 The Savagery Resource System

**Savagery Mechanics**:
- **Range**: 0-100
- **Generation**: Gained by hitting enemies, taking damage, and causing Fear
- **Decay**: Decays slowly out of combat
- **Consumption**: Required for Tier 2+ abilities

**Savagery Generation**:
| Source | Base Savagery Gained |
|--------|----------------------|
| Savage Strike hit | 15-25 (by rank) |
| Taking damage | 10-20% of damage taken |
| Causing Fear | +5 Savagery |
| Kill | Varies by ability |

### 1.5 The Augmentation System

The Skar-Horde Aspirant uses a unique **[Augmentation] slot** instead of a standard weapon:

| Augment Type | Damage Type | Special Properties |
|--------------|-------------|-------------------|
| **Piercing Spike** | Piercing | Required for Impaling Spike |
| **Blunt Piston** | Bludgeoning | Required for Overcharged Piston Slam |
| **Slashing Blade** | Slashing | Standard damage |
| **Flame Emitter** | Fire | Bonus vs organic |

### 1.6 Specialization Description (Full Text)

> The Skar-Horde Aspirant embodies the heretical philosophy of achieving power through savage, willful self-mutilation. You have ritualistically replaced your hand with a modular weapon-stump augment, trading humanity for devastating combat prowess.
>
> Build Savagery by fighting in melee, then unleash armor-bypassing attacks that ignore all defenses. Every strike pushes you closer to madness, but power is worth any price. You are no longer human. You are a weapon.

---

## 2. Rank Progression System

### 2.1 CRITICAL: Rank Unlock Rules

**Ranks are unlocked through TREE PROGRESSION, not PP spending.**

| Tier | Starting Rank | Progresses To | Rank 3 Trigger |
|------|---------------|---------------|----------------|
| **Tier 1** | Rank 1 (when learned) | Rank 2 (when 2 Tier 2 trained) | Capstone trained |
| **Tier 2** | Rank 2 (when learned) | Rank 3 (when Capstone trained) | Capstone trained |
| **Tier 3** | Rank 2 (when learned) | Rank 3 (when Capstone trained) | Capstone trained |
| **Capstone** | Rank 1 | Rank 2→3 (tree-based) | Full tree completion |

**Important Notes**:
- **Tier 1 abilities** start at Rank 1, progress to Rank 2 when 2 Tier 2 abilities are trained, and reach Rank 3 when Capstone is trained
- **Tier 2 abilities** start at Rank 2 when trained, progress to Rank 3 when Capstone is trained
- **Tier 3 abilities** start at Rank 2 when trained, progress to Rank 3 when Capstone is trained
- Rank progression is **automatic** when requirements are met (no additional PP cost)

### 2.2 Ability Structure by Tier

| Tier | Abilities | PP Cost to Unlock | Starting Rank | Max Rank | Rank Progression |
|------|-----------|-------------------|---------------|----------|------------------|
| **Tier 1** | 3 | 0 PP (free with spec) | 1 | 3 | 1→2 (2× Tier 2), 2→3 (Capstone) |
| **Tier 2** | 3 | 4 PP each | 2 | 3 | 2→3 (Capstone) |
| **Tier 3** | 2 | 5 PP each | 2 | 3 | 2→3 (Capstone) |
| **Capstone** | 1 | 6 PP | 1 | 3 | 1→2→3 (tree-based) |

### 2.3 Total PP Investment

| Milestone | PP Spent | Abilities Unlocked | Tier 1 Rank | Tier 2 Rank |
|-----------|----------|-------------------|-------------|-------------|
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
[Heretical          [Savage Strike]      [Horrific Form]
 Augmentation]         (Active)            (Passive)
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
[Grievous Wound]   [Impaling Spike]   [Pain Fuels Savagery]
    (Active)           (Active)           (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
   [Overcharged           [The Price of Power]
    Piston Slam]              (Passive)
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
              [Monstrous Apotheosis]
                   (Active)
```

### 3.2 Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Resource Cost | Key Effect |
|----|--------------|------|------|-------|---------------|------------|
| 1001 | Heretical Augmentation | 1 | Passive | 1→2→3 | None | Unlocks [Augmentation] slot |
| 1002 | Savage Strike | 1 | Active | 1→2→3 | 40 Stamina | Basic attack + Savagery gen |
| 1003 | Horrific Form | 1 | Passive | 1→2→3 | None | Fear melee attackers |
| 1004 | Grievous Wound | 2 | Active | 2→3 | 45 Stamina + 30 Savagery | DoT that bypasses Soak |
| 1005 | Impaling Spike | 2 | Active | 2→3 | 40 Stamina + 25 Savagery | Root enemy (Piercing) |
| 1006 | Pain Fuels Savagery | 2 | Passive | 2→3 | None | Damage → Savagery conversion |
| 1007 | Overcharged Piston Slam | 3 | Active | 2→3 | 55 Stamina + 40 Savagery | Massive damage + Stun (Blunt) |
| 1008 | The Price of Power | 3 | Passive | 2→3 | None | +Savagery gen, +Stress |
| 1009 | Monstrous Apotheosis | 4 | Active | 1→2→3 | 20 Stamina + 75 Savagery | Transform: free attacks, immunities |

---

## 4. Tier 1 Abilities (Detailed Rank Specifications)

These abilities are **free** when the specialization is unlocked and have 3 ranks.

---

### 4.1 Heretical Augmentation (ID: 1001)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 0 PP (free with specialization) |
| **Ranks** | 3 |
| **Resource Cost** | None (passive) |
| **Attribute Used** | None |

#### Description
You have performed the ritual of replacement, carving away weakness and grafting brutal functionality. Your hand is gone. Your weapon-stump remains.

#### Rank Details

##### Rank 1 (Unlocked: When specialization is chosen)

**Mechanical Effect**:
- Unlocks [Augmentation] equipment slot (replaces weapon slot)
- Enables crafting and installing augments at workbenches
- Augment determines damage type for Skar-Horde abilities

**Formula**:
```
EquipmentSlot.Weapon = DISABLED
EquipmentSlot.Augmentation = ENABLED
DamageType = Augment.DamageType
```

**GUI Display**:
- Passive icon: Mechanical arm with weapon attachment
- Tooltip: "Heretical Augmentation (Rank 1): Unlocks [Augmentation] slot. Craft and install augments at workbenches."
- Color: Bronze border

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- All Rank 1 effects
- **NEW**: Swap augments in 1 action (normally 2 actions)

**Formula**:
```
AugmentSwapCost = 1 action (reduced from 2)
```

**GUI Display**:
- Tooltip: "Heretical Augmentation (Rank 2): Swap augments in 1 action."
- Color: Silver border
- **Rank-up notification**: "Heretical Augmentation has reached Rank 2!"

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- All Rank 2 effects
- **NEW**: All augments gain +1 to all damage dice

**Formula**:
```
AugmentDamage = BaseDamage + 1 per die
Example: 3d8 becomes 3d8+3
```

**GUI Display**:
- Tooltip: "Heretical Augmentation (Rank 3): Augments gain +1 to all damage dice."
- Color: Gold border
- **Rank-up notification**: "Heretical Augmentation has reached Rank 3! Augments enhanced!"

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedSkarHordeTier1()`
- [ ] Equipment: [Augmentation] slot implementation
- [ ] Crafting: Augment crafting recipes
- [ ] GUI: Augment swap action button

---

### 4.2 Savage Strike (ID: 1002)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Melee)

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 0 PP (free with specialization) |
| **Ranks** | 3 |
| **Resource Cost** | 40 Stamina |
| **Attribute Used** | MIGHT |
| **Damage Type** | Varies (based on augment) |

#### Description
A brutal, straightforward blow with your augmented stump. Savage. Effective. Yours.

#### Rank Details

##### Rank 1 (Unlocked: When specialization is chosen)

**Mechanical Effect**:
- Roll: MIGHT + 2 bonus dice vs Target Defense
- Damage: 2d[Augment] + MIGHT
- Generates +15 Savagery on hit
- Success Threshold: 2

**Formulas**:
```
AttackRoll = Roll(MIGHT + 2) >= 2 successes
Damage = Roll(2 * AugmentDie) + MIGHT
SavageryGained = 15 (on hit)
```

**GUI Display**:
- Ability button: Brutal striking arm
- Tooltip: "Savage Strike (Rank 1): 2d[Augment]+MIGHT damage. +15 Savagery. Cost: 40 Stamina"
- Color: Bronze border

**Combat Log Examples**:
- "Savage Strike hits! 14 damage (2d8[6,4] + MIGHT[4]) +15 Savagery"

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- Generates +20 Savagery on hit
- Resource cost reduced to 35 Stamina

**Formulas**:
```
SavageryGained = 20 (on hit)
StaminaCost = 35
```

**GUI Display**:
- Tooltip: "Savage Strike (Rank 2): +20 Savagery. Cost: 35 Stamina"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Generates +25 Savagery on hit
- **NEW**: Apply [Bleeding] on critical hit (3+ extra successes)

**Formulas**:
```
SavageryGained = 25 (on hit)
If (Successes >= 5):  // 3+ over threshold
    Target.AddStatus("Bleeding", Duration: 2)
```

**GUI Display**:
- Tooltip: "Savage Strike (Rank 3): +25 Savagery. [Bleeding] on crit. Cost: 35 Stamina"
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedSkarHordeTier1()`
- [ ] Combat: Savagery generation
- [ ] Combat: Critical hit bleeding

---

### 4.3 Horrific Form (ID: 1003)

**Type**: Passive | **Action**: Free Action (triggered) | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 0 PP (free with specialization) |
| **Ranks** | 3 |
| **Resource Cost** | None (passive) |
| **Trigger** | When hit by melee attack |
| **Status Effect** | [Feared] |

#### Description
Your self-mutilation is deeply unsettling. Good. Let them see what you have become.

#### Rank Details

##### Rank 1 (Unlocked: When specialization is chosen)

**Mechanical Effect**:
- When hit by melee attack: 25% chance to apply [Feared] to attacker for 1 turn

**Formula**:
```
OnMeleeHit:
    If (Random() < 0.25):
        Attacker.AddStatus("Feared", Duration: 1)
```

**GUI Display**:
- Passive icon: Horrifying mechanical form
- Tooltip: "Horrific Form (Rank 1): 25% chance to Fear melee attackers for 1 turn"
- Color: Bronze border

**Combat Log Examples**:
- "[Enemy] recoils in horror! [Feared] for 1 turn (Horrific Form)"

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- Fear chance increased to 35%
- **NEW**: Feared enemies deal -2 damage to you

**Formulas**:
```
FearChance = 0.35
If (Attacker.HasStatus("Feared")):
    Attacker.DamageToYou -= 2
```

**GUI Display**:
- Tooltip: "Horrific Form (Rank 2): 35% Fear chance. Feared enemies deal -2 damage to you."
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Fear chance increased to 50%
- **NEW**: Gain +5 Savagery when enemy becomes Feared

**Formulas**:
```
FearChance = 0.50
OnEnemyFeared:
    Savagery += 5
```

**GUI Display**:
- Tooltip: "Horrific Form (Rank 3): 50% Fear chance. +5 Savagery when enemy Feared."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedSkarHordeTier1()`
- [ ] Combat: On-hit trigger system
- [ ] Status Effect: [Feared] implementation

---

## 5. Tier 2 Abilities (Rank 2→3 Progression)

These abilities start at **Rank 2** when trained and progress to **Rank 3** when the Capstone is trained.

---

### 5.1 Grievous Wound (ID: 1004)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Melee)

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Skar-Horde tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 45 Stamina + 30 Savagery |
| **Cooldown** | 2 turns |
| **Status Effect** | [Grievous Wound] |

#### Description
You carve a wound that armor cannot protect against. A wound that does not close. A wound that reminds them what mortality means.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Damage: 3d8 + MIGHT Physical damage
- Applies [Grievous Wound]: 1d10 damage per turn for 3 turns
- **[Grievous Wound] bypasses ALL Soak**

**Formulas**:
```
Damage = Roll(3d8) + MIGHT
Target.AddStatus("GrievousWound", DamagePerTurn: Roll(1d10), Duration: 3, BypassesSoak: true)
```

**GUI Display**:
- Ability button: Deep gashing wound
- Tooltip: "Grievous Wound (Rank 2): 3d8+MIGHT damage + [Grievous Wound] (1d10/turn, ignores Soak, 3 turns). Cost: 45 Stamina, 30 Savagery"
- Color: Silver border

**Combat Log Examples**:
- "Grievous Wound deals 18 damage! Target suffers [Grievous Wound]!"
- "[Enemy] bleeds for 7 damage (Grievous Wound, bypasses Soak)"

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Damage: 4d8 + MIGHT Physical damage
- [Grievous Wound] duration increased to 4 turns
- DoT damage increased to 1d12 per turn
- **NEW**: If target dies from [Grievous Wound], refund 20 Savagery

**Formulas**:
```
Damage = Roll(4d8) + MIGHT
Target.AddStatus("GrievousWound", DamagePerTurn: Roll(1d12), Duration: 4, BypassesSoak: true)
OnTargetDeath (from GrievousWound):
    Savagery += 20
```

**GUI Display**:
- Tooltip: "Grievous Wound (Rank 3): 4d8+MIGHT + [Grievous Wound] (1d12/turn, 4 turns). +20 Savagery if target dies. Cost: 45 Stamina, 30 Savagery"
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedSkarHordeTier2()`
- [ ] Status Effect: [Grievous Wound] with Soak bypass
- [ ] Combat: Death tracking for Savagery refund

---

### 5.2 Impaling Spike (ID: 1005)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Melee)

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Skar-Horde tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 40 Stamina + 25 Savagery |
| **Cooldown** | 3 turns |
| **Required Augment** | Piercing-type |
| **Status Effect** | [Rooted] |

#### Description
You slam your spike through foot, pinning them to the broken earth. They are not going anywhere.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Damage: 2d10 + MIGHT Piercing damage
- 90% chance to apply [Rooted] for 3 turns
- Requires Piercing-type augment equipped

**Formulas**:
```
Requires: Augment.Type == "Piercing"
Damage = Roll(2d10) + MIGHT
If (Random() < 0.90):
    Target.AddStatus("Rooted", Duration: 3)
```

**GUI Display**:
- Ability button: Spike through foot
- Tooltip: "Impaling Spike (Rank 2): 2d10+MIGHT Piercing. 90% [Rooted] 3 turns. Requires Piercing augment. Cost: 40 Stamina, 25 Savagery"
- Color: Silver border
- Disabled state if wrong augment equipped

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Root chance increased to 100%
- **NEW**: +2 to hit against [Rooted] targets

**Formulas**:
```
RootChance = 1.00 (guaranteed)
If (Target.HasStatus("Rooted")):
    AttackBonus += 2
```

**GUI Display**:
- Tooltip: "Impaling Spike (Rank 3): 100% [Rooted]. +2 to hit Rooted targets. Cost: 40 Stamina, 25 Savagery"
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedSkarHordeTier2()`
- [ ] Combat: Augment type requirement check
- [ ] Status Effect: [Rooted] implementation

---

### 5.3 Pain Fuels Savagery (ID: 1006)

**Type**: Passive | **Action**: Free Action | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Skar-Horde tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | None (passive) |
| **Trigger** | When taking damage |

#### Description
Every wound is fuel. Every blow against you is a gift. Pain is just another resource.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Generate Savagery equal to 15% of damage taken
- Maximum 25 Savagery per hit

**Formula**:
```
OnDamageTaken:
    SavageryGained = Min(DamageTaken * 0.15, 25)
```

**GUI Display**:
- Passive icon: Blood droplets forming energy
- Tooltip: "Pain Fuels Savagery (Rank 2): Generate 15% of damage taken as Savagery (max 25/hit)"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Conversion rate increased to 20%
- Maximum increased to 30 Savagery per hit
- **NEW**: Gain +1 Soak per 25 Savagery held

**Formulas**:
```
OnDamageTaken:
    SavageryGained = Min(DamageTaken * 0.20, 30)
BonusSoak = Floor(Savagery / 25)
```

**GUI Display**:
- Tooltip: "Pain Fuels Savagery (Rank 3): 20% conversion (max 30/hit). +1 Soak per 25 Savagery."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedSkarHordeTier2()`
- [ ] Combat: Damage-to-Savagery conversion
- [ ] Combat: Soak bonus calculation

---

## 6. Tier 3 Abilities (Rank 2→3 Progression)

---

### 6.1 Overcharged Piston Slam (ID: 1007)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Melee)

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Skar-Horde tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 55 Stamina + 40 Savagery |
| **Cooldown** | 4 turns |
| **Required Augment** | Blunt-type |
| **Status Effect** | [Stunned] |

#### Description
Superheated steam vents. Pistons compress. And then—impact. A concussive blast that reduces bone to powder.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Damage: 7d10 + MIGHT Bludgeoning damage
- 75% chance to apply [Stunned] for 1 turn
- Roll: MIGHT + 3 bonus dice
- Requires Blunt-type augment equipped

**Formulas**:
```
Requires: Augment.Type == "Blunt"
AttackRoll = Roll(MIGHT + 3) >= 2 successes
Damage = Roll(7d10) + MIGHT
If (Random() < 0.75):
    Target.AddStatus("Stunned", Duration: 1)
```

**GUI Display**:
- Ability button: Piston impact with shockwave
- Tooltip: "Overcharged Piston Slam (Rank 2): 7d10+MIGHT Bludgeoning. 75% [Stunned] 1 turn. Requires Blunt augment. Cost: 55 Stamina, 40 Savagery"
- Color: Silver border

**Combat Log Examples**:
- "OVERCHARGED PISTON SLAM! 48 damage! [Enemy] is STUNNED!"

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Stun chance increased to 100%
- **NEW**: Your next attack against the stunned target deals double damage

**Formulas**:
```
StunChance = 1.00 (guaranteed)
OnStunApplied:
    NextAttackBonus = "DoubleDamage"
```

**GUI Display**:
- Tooltip: "Overcharged Piston Slam (Rank 3): 100% [Stunned]. Next attack deals double damage. Cost: 55 Stamina, 40 Savagery"
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedSkarHordeTier3()`
- [ ] Combat: Augment type requirement
- [ ] Combat: Double damage buff tracking

---

### 6.2 The Price of Power (ID: 1008)

**Type**: Passive | **Action**: Free Action | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Skar-Horde tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | None (passive) |
| **Risk** | Psychic Stress generation |

#### Description
The rush of transhuman power is intoxicating. The whispers in your mind grow louder. You do not care. Power is worth any price.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- +75% Savagery generation from all sources
- Gain 1 Psychic Stress per 10 Savagery generated

**Formula**:
```
SavageryGeneration *= 1.75
OnSavageryGenerated:
    PsychicStress += Floor(SavageryGenerated / 10)
```

**GUI Display**:
- Passive icon: Power surge with madness spiral
- Tooltip: "The Price of Power (Rank 2): +75% Savagery generation. Gain 1 Stress per 10 Savagery generated."
- Warning indicator: Trauma risk
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- +100% Savagery generation (double)
- Stress generation reduced: 1 Stress per 15 Savagery

**Formulas**:
```
SavageryGeneration *= 2.0
OnSavageryGenerated:
    PsychicStress += Floor(SavageryGenerated / 15)
```

**GUI Display**:
- Tooltip: "The Price of Power (Rank 3): +100% Savagery (double). 1 Stress per 15 Savagery."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedSkarHordeTier3()`
- [ ] Combat: Savagery generation modifier
- [ ] Trauma: Psychic Stress integration

---

## 7. Capstone Ability

---

### 7.1 Monstrous Apotheosis (ID: 1009)

**Type**: Active | **Action**: Bonus Action | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 4 (Capstone) |
| **PP Cost to Unlock** | 6 PP |
| **Prerequisite** | 24 PP invested + both Tier 3 abilities |
| **Ranks** | 1→2→3 |
| **Resource Cost** | 20 Stamina + 75 Savagery |
| **Cooldown** | Once per combat |
| **Status Effect** | [Apotheosis] |
| **Special** | Training this ability upgrades all Tier 1, 2, & 3 abilities to Rank 3 |

#### Description
You give in completely. The whispers become a roar. Your augment screams with power. You are no longer human. You are a weapon. You are inevitable.

#### Rank Details

##### Rank 1 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Enter [Apotheosis] state for 3 turns:
  - Savage Strike costs 0 Stamina
  - Grievous Wound automatically applies [Bleeding]
  - Immune to [Feared] and [Stunned]
- After [Apotheosis] ends: Gain 30 Psychic Stress

**Formulas**:
```
Self.AddStatus("Apotheosis", Duration: 3)
While (Apotheosis):
    SavageStrike.StaminaCost = 0
    GrievousWound.AppliesBleeding = true
    Immunity: Feared, Stunned
OnApotheosisEnd:
    PsychicStress += 30
```

**GUI Display**:
- Ability button: Monstrous transformation
- Tooltip: "Monstrous Apotheosis (Rank 1): 3 turns: Free Savage Strikes, Grievous Wound bleeds, Fear/Stun immunity. 30 Stress after. Cost: 20 Stamina, 75 Savagery"
- Warning: "30 Psychic Stress after effect ends"
- Color: Bronze border

---

##### Rank 2 (Unlocked: Based on tree progression)

**Mechanical Effect**:
- Duration increased to 4 turns
- Stress penalty reduced to 25

**Formulas**:
```
ApotheosissDuration = 4
PostApotheosisStress = 25
```

**GUI Display**:
- Tooltip: "Monstrous Apotheosis (Rank 2): 4 turns. 25 Stress after."
- Color: Silver border

---

##### Rank 3 (Unlocked: Full tree completion)

**Mechanical Effect**:
- +25% damage to all attacks during [Apotheosis]
- Stress penalty reduced to 20
- **NEW**: Can end [Apotheosis] early (as Bonus Action) to avoid Stress penalty entirely

**Formulas**:
```
While (Apotheosis):
    AllDamage *= 1.25
PostApotheosisStress = 20
CanEndEarly = true (avoids Stress)
```

**GUI Display**:
- Tooltip: "Monstrous Apotheosis (Rank 3): +25% damage. Can end early to avoid 20 Stress."
- Color: Gold border

#### GUI Display - APOTHEOSIS ACTIVATION

When triggered:

```
┌─────────────────────────────────────────────┐
│         MONSTROUS APOTHEOSIS!               │
├─────────────────────────────────────────────┤
│                                             │
│  You are no longer human.                   │
│  You are a WEAPON.                          │
│                                             │
│  • Free Savage Strikes                      │
│  • Grievous Wound applies Bleeding          │
│  • Immune to Fear and Stun                  │
│                                             │
│  Duration: 3 turns                          │
│  WARNING: 30 Stress when effect ends        │
│                                             │
└─────────────────────────────────────────────┘
```

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedSkarHordeCapstone()`
- [ ] Combat: [Apotheosis] state implementation
- [ ] Combat: Cost modification during state
- [ ] Trauma: Post-effect Stress application
- [ ] GUI: Transformation notification

---

## 8. Status Effect Definitions

### 8.1 [Grievous Wound]

| Property | Value |
|----------|-------|
| **Applied By** | Grievous Wound |
| **Duration** | 3-4 turns |
| **Icon** | Deep gash with blood |
| **Color** | Dark crimson |

**Effects**:
- Takes 1d10-1d12 Physical damage at start of turn
- **Bypasses ALL Soak/Armor**
- Cannot be healed until effect ends

---

### 8.2 [Rooted]

| Property | Value |
|----------|-------|
| **Applied By** | Impaling Spike |
| **Duration** | 2-3 turns |
| **Icon** | Spike through foot |
| **Color** | Gray |

**Effects**:
- Cannot move
- Cannot use movement abilities
- -2 to Dodge/Defense rolls

---

### 8.3 [Feared]

| Property | Value |
|----------|-------|
| **Applied By** | Horrific Form |
| **Duration** | 1 turn |
| **Icon** | Terror face |
| **Color** | Purple |

**Effects**:
- Must move away from fear source
- -2 to all attack rolls
- At Rank 2+: -2 damage to Skar-Horde Aspirant

---

### 8.4 [Stunned]

| Property | Value |
|----------|-------|
| **Applied By** | Overcharged Piston Slam |
| **Duration** | 1 turn |
| **Icon** | Stars/daze |
| **Color** | Yellow |

**Effects**:
- Cannot take actions
- -4 to Defense
- At Rank 3: Next attack against target deals double damage

---

### 8.5 [Apotheosis]

| Property | Value |
|----------|-------|
| **Applied By** | Monstrous Apotheosis |
| **Duration** | 3-4 turns |
| **Icon** | Monstrous transformation |
| **Color** | Blood red with dark aura |

**Effects**:
- Savage Strike costs 0 Stamina
- Grievous Wound applies [Bleeding]
- Immune to [Feared] and [Stunned]
- At Rank 3: +25% all damage

---

## 9. GUI Requirements

### 9.1 Savagery Bar

The Skar-Horde Aspirant requires a **Savagery resource bar** displayed in combat:

```
┌─────────────────────────────────────────┐
│  SAVAGERY [████████████░░░░░░░░] 60/100 │
│  ⚠️ Extreme Trauma Risk                  │
└─────────────────────────────────────────┘
```

- Color: Blood red gradient that pulses with intensity
- Warning indicator showing trauma risk level
- Breakpoints at 25/50/75 for ability thresholds

### 9.2 Augmentation Display

```
┌─────────────────────────────────────────┐
│  AUGMENTATION: [Blunt Piston]           │
│  Damage Type: Bludgeoning               │
│  [SWAP] (1 Action)                      │
└─────────────────────────────────────────┘
```

### 9.3 Ability Card Rank Indicators

| Rank | Border Color | Badge |
|------|--------------|-------|
| 1 | Bronze (#CD7F32) | "I" |
| 2 | Silver (#C0C0C0) | "II" |
| 3 | Gold (#FFD700) | "III" |

---

## 10. Implementation Priority

### Phase 1: Critical (Foundation)
1. **Implement Savagery resource system** - 0-100 scale with generation/decay
2. **Implement [Augmentation] equipment slot** - Replace weapon slot
3. **Create augment item types** - Piercing, Blunt, Slashing, Fire
4. **Implement rank calculation logic** based on tree progression

### Phase 2: Combat Integration
5. **Route Skar-Horde abilities** through CombatEngine
6. **Implement Savagery generation** from hits/damage taken
7. **Add Savagery bar** to CombatView
8. **Implement augment type requirements** for abilities

### Phase 3: Status Effects
9. **Define status effects** ([Grievous Wound], [Rooted], [Apotheosis])
10. **Implement Soak bypass** for Grievous Wound
11. **Implement immunity system** (Fear, Stun during Apotheosis)

### Phase 4: Trauma Integration
12. **Implement Psychic Stress generation** from The Price of Power
13. **Implement post-Apotheosis Stress** penalty
14. **Add trauma warning indicators** to GUI

### Phase 5: Polish
15. **Add rank-specific icons** (Bronze/Silver/Gold)
16. **Implement rank-up notifications**
17. **Add Apotheosis transformation notification**

---

**End of Specification**
