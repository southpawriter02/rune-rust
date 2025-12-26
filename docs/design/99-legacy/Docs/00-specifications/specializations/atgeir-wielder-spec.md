# Atgeir-Wielder Specialization - Complete Specification

> **Specification ID**: SPEC-SPECIALIZATION-ATGEIR-WIELDER
> **Version**: 1.0
> **Last Updated**: 2025-11-27
> **Status**: Draft - Implementation Review

---

## Document Control

### Purpose
This document provides the complete specification for the Atgeir-Wielder specialization, including:
- Design philosophy and mechanical identity
- All 9 abilities with **exact formulas per rank**
- **Rank unlock requirements** (tree-progression based, NOT PP-based)
- **GUI display specifications per rank**
- Current implementation status
- Combat system integration points

### Related Files
| Component | File Path | Status |
|-----------|-----------|--------|
| Data Seeding | `RuneAndRust.Persistence/DataSeeder.cs` (line 1515) | Implemented |
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
| **Internal Name** | AtgeirWielder |
| **Display Name** | Atgeir-Wielder |
| **Specialization ID** | 12 |
| **Archetype** | Warrior (ArchetypeID = 1) |
| **Path Type** | Coherent |
| **Mechanical Role** | Battlefield Controller / Formation Anchor |
| **Primary Attribute** | MIGHT |
| **Secondary Attribute** | WITS |
| **Resource System** | Stamina (no secondary resource) |
| **Trauma Risk** | None |
| **Icon** | :crossed_swords: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 3 | Early-game specialization |
| **Maximum Corruption** | 100 | No corruption restriction |
| **Minimum Corruption** | 0 | No minimum corruption |
| **Required Quest** | None | No quest prerequisite |

### 1.3 Design Philosophy

**Tagline**: "Tactical discipline — control the battlefield"

**Core Fantasy**: You are the disciplined hoplite, the master of formation warfare. Wielding a long polearm, you command the space around you with tactical precision. Your [Reach] allows you to strike from safety while your Push and Pull effects shatter enemy formations.

You are the immovable anchor that holds the line, the thinking warrior who controls where battles happen. You don't fight with rage—you fight with discipline, leverage, and perfect positioning.

**Mechanical Identity**:
1. **[Reach] Keyword**: Can attack front row from back row (tactical safety)
2. **Forced Movement**: Push enemies back, Pull enemies forward to disrupt formations
3. **Formation Warfare**: Auras and bonuses that benefit adjacent allies
4. **Defensive Anchor**: Stance abilities that make you immovable and punish attackers

### 1.4 The [Reach] Keyword

The Atgeir-Wielder's core mechanic is **[Reach]**, allowing attacks from the back row:

| Position | Normal Attack Range | With [Reach] |
|----------|-------------------|--------------|
| Front Row | Front Row enemies | Front + Back Row enemies |
| Back Row | Cannot melee | Front Row enemies |

### 1.5 Forced Movement Mechanics

| Effect | Description | Opposed Check |
|--------|-------------|---------------|
| **[Push]** | Move enemy from Front → Back row | MIGHT vs STURDINESS |
| **[Pull]** | Move enemy from Back → Front row | MIGHT vs STURDINESS |

### 1.6 Specialization Description (Full Text)

> You are the disciplined hoplite, the master of formation warfare. Wielding a long polearm, you command the space around you with tactical precision. Your [Reach] allows you to strike from safety while your Push and Pull effects shatter enemy formations.
>
> You are the immovable anchor that holds the line, the thinking warrior who controls where battles happen. You don't fight with rage—you fight with discipline, leverage, and perfect positioning.
>
> In a chaotic, glitching world, you impose order through superior reach, forced movement, and formation mastery. The polearm is logic made physical.

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

### 2.2 Ability Structure by Tier

| Tier | Abilities | PP Cost to Unlock | Starting Rank | Max Rank | Rank Progression |
|------|-----------|-------------------|---------------|----------|------------------|
| **Tier 1** | 3 | 3 PP each | 1 | 3 | 1→2 (2× Tier 2), 2→3 (Capstone) |
| **Tier 2** | 3 | 4 PP each | 2 | 3 | 2→3 (Capstone) |
| **Tier 3** | 2 | 5 PP each | 2 | 3 | 2→3 (Capstone) |
| **Capstone** | 1 | 6 PP | 1 | 3 | 1→2→3 (tree-based) |

### 2.3 Total PP Investment

| Milestone | PP Spent | Abilities Unlocked | Tier 1 Rank | Tier 2 Rank |
|-----------|----------|-------------------|-------------|-------------|
| Unlock Specialization | 3 PP | 0 | - | - |
| All Tier 1 | 3 + 9 = 12 PP | 3 Tier 1 | Rank 1 | - |
| 2 Tier 2 | 12 + 8 = 20 PP | 3 Tier 1 + 2 Tier 2 | **Rank 2** | Rank 2 |
| All Tier 2 | 20 + 4 = 24 PP | 3 Tier 1 + 3 Tier 2 | Rank 2 | Rank 2 |
| All Tier 3 | 24 + 10 = 34 PP | 3 Tier 1 + 3 Tier 2 + 2 Tier 3 | Rank 2 | Rank 2 |
| Capstone | 34 + 6 = 40 PP | All 9 abilities | **Rank 3** | **Rank 3** |

---

## 3. Ability Tree Overview

### 3.1 Visual Tree Structure

```
                    TIER 1: FOUNDATION (3 PP each, Ranks 1-3)
    ┌─────────────────────┼─────────────────────┐
    │                     │                     │
[Formal Training]      [Skewer]        [Disciplined Stance]
   (Passive)           (Active)             (Active)
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
[Hook and Drag]     [Line Breaker]    [Guarding Presence]
    (Active)           (Active)           (Passive)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
   [Brace for Charge]    [Unstoppable Phalanx]
       (Active)               (Active)
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
                 [Living Fortress]
                    (Passive)
```

### 3.2 Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Resource Cost | Key Effect |
|----|--------------|------|------|-------|---------------|------------|
| 1201 | Formal Training | 1 | Passive | 1→2→3 | None | +Stamina regen, resist disorientation |
| 1202 | Skewer | 1 | Active | 1→2→3 | 40 Stamina | [Reach] attack |
| 1203 | Disciplined Stance | 1 | Active | 1→2→3 | 30 Stamina | +Soak, resist Push/Pull |
| 1204 | Hook and Drag | 2 | Active | 2→3 | 45 Stamina | [Pull] enemy to front |
| 1205 | Line Breaker | 2 | Active | 2→3 | 50 Stamina | AoE [Push] enemies back |
| 1206 | Guarding Presence | 2 | Passive | 2→3 | None | Aura: +Soak for allies |
| 1207 | Brace for Charge | 3 | Active | 2→3 | 40 Stamina | Counter-attack stance |
| 1208 | Unstoppable Phalanx | 3 | Active | 2→3 | 60 Stamina | Line-piercing attack |
| 1209 | Living Fortress | 4 | Passive | 1→2→3 | None | Forced movement immunity, Zone of Control |

---

## 4. Tier 1 Abilities (Detailed Rank Specifications)

---

### 4.1 Formal Training (ID: 1201)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | None (passive) |

#### Description
Your formal training instills deep physical and mental discipline, allowing you to remain focused amid chaos.

#### Rank Details

##### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:
- +5 Stamina regeneration per turn
- +1d10 bonus dice to Resolve checks vs [Stagger]

**Formula**:
```
StaminaRegen += 5
ResolveBonus += 1d10 (vs Stagger only)
```

**GUI Display**:
- Passive icon: Disciplined stance figure
- Tooltip: "Formal Training (Rank 1): +5 Stamina regen. +1d10 vs [Stagger]"
- Color: Bronze border

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- +7 Stamina regeneration per turn
- +1d10 vs [Stagger] AND [Disoriented]

**Formula**:
```
StaminaRegen += 7
ResolveBonus += 1d10 (vs Stagger, Disoriented)
```

**GUI Display**:
- Tooltip: "Formal Training (Rank 2): +7 Stamina regen. +1d10 vs [Stagger] and [Disoriented]"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- +10 Stamina regeneration per turn
- +2d10 vs [Stagger] and [Disoriented]
- **NEW**: +1 WITS (permanent attribute bonus)

**Formula**:
```
StaminaRegen += 10
ResolveBonus += 2d10 (vs Stagger, Disoriented)
WITS += 1
```

**GUI Display**:
- Tooltip: "Formal Training (Rank 3): +10 Stamina regen. +2d10 vs disruption. +1 WITS."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedAtgeirWielderTier1()`
- [ ] Combat: Stamina regeneration bonus
- [ ] Combat: Resolve check bonuses

---

### 4.2 Skewer (ID: 1202)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Front Row)

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 40 Stamina |
| **Attribute Used** | MIGHT |
| **Damage Type** | Physical |
| **Special** | [Reach] |

#### Description
A precise, powerful thrust designed to exploit your weapon's length. Strike from tactical safety.

#### Rank Details

##### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:
- Damage: 2d8 Physical damage
- **[Reach]**: Can attack front row from back row

**Formulas**:
```
AttackRoll = Roll(MIGHT + 2) >= 2 successes
Damage = Roll(2d8)
Range = REACH (Back Row → Front Row valid)
```

**GUI Display**:
- Ability button: Thrusting polearm
- Tooltip: "Skewer (Rank 1): 2d8 Physical. [Reach] - Attack front row from back row. Cost: 40 Stamina"
- Color: Bronze border

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- Resource cost reduced to 35 Stamina
- Damage: 2d8 + 1d6 Physical

**Formula**:
```
StaminaCost = 35
Damage = Roll(2d8) + Roll(1d6)
```

**GUI Display**:
- Tooltip: "Skewer (Rank 2): 2d8+1d6 Physical. [Reach]. Cost: 35 Stamina"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Damage: 2d8 + 2d6 Physical
- **NEW**: On critical hit, apply [Bleeding] for 2 turns

**Formula**:
```
Damage = Roll(2d8) + Roll(2d6)
If (Critical):
    Target.AddStatus("Bleeding", Duration: 2)
```

**GUI Display**:
- Tooltip: "Skewer (Rank 3): 2d8+2d6 Physical. [Reach]. Crit: [Bleeding] 2 turns. Cost: 35 Stamina"
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedAtgeirWielderTier1()`
- [ ] Combat: [Reach] targeting system
- [ ] Combat: Back-to-front row attack validation

---

### 4.3 Disciplined Stance (ID: 1203)

**Type**: Active | **Action**: Bonus Action | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 30 Stamina |
| **Cooldown** | 3 turns |

#### Description
You plant your feet, becoming an anchor of stability. This line will not be broken.

#### Rank Details

##### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:
- Enter stance for 2 turns
- +4 Soak
- +3 bonus dice to resist [Push]/[Pull]
- Cannot move while active

**Formula**:
```
Self.AddStatus("DisciplinedStance", Duration: 2)
While (DisciplinedStance):
    Soak += 4
    ResistForcedMovement += 3 dice
    CanMove = false
```

**GUI Display**:
- Ability button: Planted stance figure
- Tooltip: "Disciplined Stance (Rank 1): 2 turns: +4 Soak, +3 dice vs Push/Pull. Cannot move. Cost: 30 Stamina"
- Color: Bronze border

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- Duration: 3 turns
- +6 Soak
- +4 bonus dice vs [Push]/[Pull]

**Formula**:
```
Duration = 3
Soak += 6
ResistForcedMovement += 4 dice
```

**GUI Display**:
- Tooltip: "Disciplined Stance (Rank 2): 3 turns: +6 Soak, +4 dice vs Push/Pull. Cannot move."
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- +8 Soak
- **IMMUNE** to [Push] and [Pull]
- **NEW**: +5 Stamina regen while in stance

**Formula**:
```
Soak += 8
ImmuneToForcedMovement = true
StaminaRegen += 5
```

**GUI Display**:
- Tooltip: "Disciplined Stance (Rank 3): +8 Soak. IMMUNE to Push/Pull. +5 Stamina regen while in stance."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedAtgeirWielderTier1()`
- [ ] Combat: Stance system
- [ ] Combat: Forced movement immunity

---

## 5. Tier 2 Abilities (Rank 2→3 Progression)

---

### 5.1 Hook and Drag (ID: 1204)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Back Row)

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Atgeir-Wielder tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 45 Stamina |
| **Cooldown** | 4 turns |
| **Status Effect** | [Pull], [Slowed], [Stunned] |

#### Description
Using your weapon's hooked blade, you violently yank a priority target out of position.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Damage: 3d8 Physical
- MIGHT vs STURDINESS opposed check to [Pull] target from back → front row
- +2 bonus to Pull check
- **NEW**: Pulled target is [Slowed] for 1 turn

**Formulas**:
```
Damage = Roll(3d8)
PullCheck = Roll(MIGHT + 2) vs Target.Roll(STURDINESS)
If (PullCheck succeeds):
    Target.MoveToFrontRow()
    Target.AddStatus("Slowed", Duration: 1)
```

**GUI Display**:
- Ability button: Hook catching enemy
- Tooltip: "Hook and Drag (Rank 2): 3d8 Physical. [Pull] back→front (+2 check). Pulled target [Slowed] 1 turn. Cost: 45 Stamina"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Damage: 4d8 Physical
- +3 bonus to Pull check
- **NEW**: If Pull succeeds, target is also [Stunned] for 1 turn

**Formulas**:
```
Damage = Roll(4d8)
PullBonus = 3
If (PullCheck succeeds):
    Target.AddStatus("Stunned", Duration: 1)
```

**GUI Display**:
- Tooltip: "Hook and Drag (Rank 3): 4d8 Physical. [Pull] (+3 check). [Slowed] + [Stunned] on Pull."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedAtgeirWielderTier2()`
- [ ] Combat: Opposed check system
- [ ] Combat: Row movement (Pull)

---

### 5.2 Line Breaker (ID: 1205)

**Type**: Active | **Action**: Standard Action | **Target**: All Enemies (Front Row)

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Atgeir-Wielder tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 50 Stamina |
| **Cooldown** | 5 turns |
| **Status Effect** | [Push], [Off-Balance] |

#### Description
A wide, sweeping strike that shatters enemy formations and drives them backward.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Damage: 4d6 Physical to all enemies in Front Row
- MIGHT vs STURDINESS to [Push] all targets to Back Row
- +1 bonus to Push check
- Successfully pushed enemies take +1d6 bonus damage

**Formulas**:
```
For each Enemy in FrontRow:
    Damage = Roll(4d6)
    PushCheck = Roll(MIGHT + 1) vs Target.Roll(STURDINESS)
    If (PushCheck succeeds):
        Target.MoveToBackRow()
        BonusDamage = Roll(1d6)
```

**GUI Display**:
- Ability button: Sweeping polearm strike
- Tooltip: "Line Breaker (Rank 2): 4d6 Physical to Front Row. [Push] to Back (+1 check). +1d6 on Push. Cost: 50 Stamina"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Damage: 5d6 Physical
- +2 bonus to Push check
- **NEW**: Pushed enemies are [Off-Balance] (-2 to hit) for 1 turn

**Formulas**:
```
Damage = Roll(5d6)
PushBonus = 2
If (PushCheck succeeds):
    Target.AddStatus("Off-Balance", HitPenalty: -2, Duration: 1)
```

**GUI Display**:
- Tooltip: "Line Breaker (Rank 3): 5d6 Physical. [Push] (+2 check). Pushed: [Off-Balance] -2 hit, 1 turn."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedAtgeirWielderTier2()`
- [ ] Combat: AoE Front Row targeting
- [ ] Combat: Row movement (Push)

---

### 5.3 Guarding Presence (ID: 1206)

**Type**: Passive | **Action**: Free Action | **Target**: Adjacent Front-Row Allies

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Atgeir-Wielder tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | None (passive aura) |
| **Requirement** | Must be in Front Row |

#### Description
Your disciplined presence inspires fortitude in those around you. The formation holds.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- While in front row: You and adjacent front-row allies gain +2 Soak
- **NEW**: Aura also grants +1 bonus die vs [Fear]

**Formula**:
```
Requires: Self.Position == FrontRow
Aura(AdjacentFrontRowAllies):
    Soak += 2
    ResolveBonusVsFear += 1 die
```

**GUI Display**:
- Passive icon: Shield aura
- Tooltip: "Guarding Presence (Rank 2): Front row aura: +2 Soak, +1 die vs Fear"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- +3 Soak aura
- **NEW**: Allies in aura regenerate +3 Stamina per turn

**Formulas**:
```
Aura(AdjacentFrontRowAllies):
    Soak += 3
    StaminaRegen += 3
```

**GUI Display**:
- Tooltip: "Guarding Presence (Rank 3): Front row aura: +3 Soak, +1 die vs Fear, +3 Stamina regen."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedAtgeirWielderTier2()`
- [ ] Combat: Aura system
- [ ] Combat: Adjacent ally detection

---

## 6. Tier 3 Abilities (Rank 2→3 Progression)

---

### 6.1 Brace for Charge (ID: 1207)

**Type**: Active | **Action**: Standard Action | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Atgeir-Wielder tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 40 Stamina |
| **Cooldown** | Once per combat |
| **Trigger** | When hit by melee attack |

#### Description
You set your weapon with expert precision. They will run onto your spear and break.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Enter defensive stance for 1 turn
- When hit by melee attack:
  - +10 Soak
  - Immune to [Knocked Down]
  - Attacker takes 5d8 Physical damage
  - **NEW**: Attacker WILL save DC 15 or [Stunned] 1 turn

**Formulas**:
```
Self.AddStatus("Braced", Duration: 1)
OnMeleeHit:
    Soak += 10
    ImmuneToKnockdown = true
    Attacker.TakeDamage(Roll(5d8), "Physical")
    If (Attacker.WILLSave < 15):
        Attacker.AddStatus("Stunned", Duration: 1)
```

**GUI Display**:
- Ability button: Braced spear
- Tooltip: "Brace for Charge (Rank 2): 1 turn: +10 Soak, immune Knockdown. Counter: 5d8, DC 15 or Stunned. Cost: 40 Stamina"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Counter damage: 6d8 Physical
- Stun save DC: 18
- **NEW**: Mechanical/Undying attackers are automatically Stunned (no save)

**Formulas**:
```
CounterDamage = Roll(6d8)
StunDC = 18
If (Attacker.Type == "Mechanical" OR Attacker.Type == "Undying"):
    Attacker.AddStatus("Stunned", Duration: 1)  // No save
```

**GUI Display**:
- Tooltip: "Brace for Charge (Rank 3): Counter: 6d8, DC 18 or Stunned. Mech/Undying auto-Stunned."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedAtgeirWielderTier3()`
- [ ] Combat: Defensive stance with trigger
- [ ] Combat: Counter-attack system

---

### 6.2 Unstoppable Phalanx (ID: 1208)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy + Enemy Behind

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Atgeir-Wielder tree |
| **Ranks** | 2→3 (starts at Rank 2) |
| **Resource Cost** | 60 Stamina |
| **Cooldown** | 4 turns |
| **Special** | Line-piercing attack |

#### Description
Your polearm punches through armor and flesh, impaling one target and striking another.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Primary target: 7d10 Physical damage
- If primary hit: Secondary target (directly behind) takes 5d10 Physical
- **NEW**: Both targets [Off-Balance] for 1 turn

**Formulas**:
```
AttackRoll = Roll(MIGHT + 3) >= 3 successes
PrimaryDamage = Roll(7d10)
If (AttackHits):
    SecondaryTarget = GetEnemyBehind(PrimaryTarget)
    SecondaryDamage = Roll(5d10)
    PrimaryTarget.AddStatus("Off-Balance", Duration: 1)
    SecondaryTarget.AddStatus("Off-Balance", Duration: 1)
```

**GUI Display**:
- Ability button: Piercing thrust through enemies
- Tooltip: "Unstoppable Phalanx (Rank 2): 7d10 to primary, 5d10 to enemy behind. Both [Off-Balance]. Cost: 60 Stamina"
- Color: Silver border

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Primary: 8d10 Physical
- Secondary: 6d10 Physical
- **NEW**: If primary dies, secondary takes doubled bonus damage

**Formulas**:
```
PrimaryDamage = Roll(8d10)
SecondaryDamage = Roll(6d10)
If (PrimaryTarget.HP <= 0):
    SecondaryDamage *= 2
```

**GUI Display**:
- Tooltip: "Unstoppable Phalanx (Rank 3): 8d10 primary, 6d10 secondary. If primary dies: 2x secondary damage."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedAtgeirWielderTier3()`
- [ ] Combat: Line-piercing targeting
- [ ] Combat: Kill-chain damage bonus

---

## 7. Capstone Ability

---

### 7.1 Living Fortress (ID: 1209)

**Type**: Passive | **Action**: Free Action | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 4 (Capstone) |
| **PP Cost to Unlock** | 6 PP |
| **Prerequisite** | 24 PP invested + both Tier 3 abilities |
| **Ranks** | 1→2→3 |
| **Resource Cost** | None (passive) |
| **Requirement** | Must be in Front Row |
| **Special** | Training this ability upgrades all Tier 1, 2, & 3 abilities to Rank 3 |

#### Description
You have become the absolute master of your domain. A living fortress around which battles are won.

#### Rank Details

##### Rank 1 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- While in front row: Immune to [Push] and [Pull]
- Brace for Charge can be used as Reaction (once per combat)

**Formula**:
```
Requires: Self.Position == FrontRow
ImmuneToForcedMovement = true
BraceForCharge.CanUseAsReaction = true
BraceForCharge.ReactiveUsesPerCombat = 1
```

**GUI Display**:
- Passive icon: Fortress silhouette
- Tooltip: "Living Fortress (Rank 1): Immune to Push/Pull. Brace as Reaction 1x/combat."
- Color: Bronze border

---

##### Rank 2 (Unlocked: Based on tree progression)

**Mechanical Effect**:
- **NEW**: Aura: Adjacent allies +3 dice to resist [Push]/[Pull]
- **NEW**: Skewer range increased by 1 row (can hit back row from front row)

**Formulas**:
```
Aura(AdjacentAllies):
    ResistForcedMovement += 3 dice
Skewer.Range += 1 row
```

**GUI Display**:
- Tooltip: "Living Fortress (Rank 2): Allies +3 dice vs Push/Pull. Skewer +1 range."
- Color: Silver border

---

##### Rank 3 (Unlocked: Full tree completion)

**Mechanical Effect**:
- **Zone of Control**: Enemies in front row opposite you:
  - -1 to hit
  - Cannot move freely (must pass check to change row)
- Brace for Charge reactive triggers **twice** per combat

**Formulas**:
```
ZoneOfControl(OpposingFrontRow):
    HitPenalty = -1
    MovementRestricted = true
BraceForCharge.ReactiveUsesPerCombat = 2
```

**GUI Display**:
- Tooltip: "Living Fortress (Rank 3): Zone of Control: Enemies -1 hit, restricted movement. Brace as Reaction 2x/combat."
- Color: Gold border

#### Implementation Status
- [x] Data seeded in `DataSeeder.SeedAtgeirWielderCapstone()`
- [ ] Combat: Zone of Control system
- [ ] Combat: Reactive ability trigger
- [ ] Combat: Movement restriction checks

---

## 8. Status Effect Definitions

### 8.1 [Slowed]

| Property | Value |
|----------|-------|
| **Applied By** | Hook and Drag |
| **Duration** | 1 turn |
| **Icon** | Weighted feet |
| **Color** | Blue |

**Effects**:
- Movement costs doubled
- Cannot use movement abilities

---

### 8.2 [Off-Balance]

| Property | Value |
|----------|-------|
| **Applied By** | Line Breaker, Unstoppable Phalanx |
| **Duration** | 1 turn |
| **Icon** | Stumbling figure |
| **Color** | Yellow |

**Effects**:
- -2 to all attack rolls
- -1 to Defense

---

### 8.3 [Stunned]

| Property | Value |
|----------|-------|
| **Applied By** | Hook and Drag, Brace for Charge |
| **Duration** | 1 turn |
| **Icon** | Stars |
| **Color** | Yellow |

**Effects**:
- Cannot take actions
- -4 to Defense

---

### 8.4 [Bleeding]

| Property | Value |
|----------|-------|
| **Applied By** | Skewer (Rank 3 crit) |
| **Duration** | 2 turns |
| **Icon** | Blood drops |
| **Color** | Red |

**Effects**:
- Takes 1d6 Physical damage at start of turn

---

### 8.5 [Disciplined Stance]

| Property | Value |
|----------|-------|
| **Applied By** | Disciplined Stance |
| **Duration** | 2-3 turns |
| **Icon** | Planted shield |
| **Color** | Gray |

**Effects**:
- +4/+6/+8 Soak (by rank)
- Bonus dice vs [Push]/[Pull]
- Cannot move voluntarily

---

## 9. GUI Requirements

### 9.1 Position Indicator

The Atgeir-Wielder benefits significantly from positioning. Display:

```
┌─────────────────────────────────────────┐
│  POSITION: [FRONT ROW]                  │
│  :shield: Living Fortress Active                 │
│  :crossed_swords: Zone of Control: 2 enemies affected     │
└─────────────────────────────────────────┘
```

### 9.2 [Reach] Targeting Display

When using [Reach] abilities from back row:

```
┌─────────────────────────────────────────┐
│  BACK ROW ATTACK                        │
│  [Reach] allows targeting Front Row     │
│  Available targets: [Enemy 1] [Enemy 2] │
└─────────────────────────────────────────┘
```

### 9.3 Stance Duration Indicator

```
┌─────────────────────────────────────────┐
│  [Disciplined Stance] 2 turns remaining │
│  +6 Soak | Immune Push/Pull | No Move   │
└─────────────────────────────────────────┘
```

### 9.4 Ability Card Rank Indicators

| Rank | Border Color | Badge |
|------|--------------|-------|
| 1 | Bronze (#CD7F32) | "I" |
| 2 | Silver (#C0C0C0) | "II" |
| 3 | Gold (#FFD700) | "III" |

---

## 10. Implementation Priority

### Phase 1: Critical (Foundation)
1. **Implement [Reach] keyword** - Back row → Front row targeting
2. **Implement row positioning system** - Front/Back row for players and enemies
3. **Implement forced movement** - [Push] and [Pull] mechanics
4. **Implement rank calculation logic** based on tree progression

### Phase 2: Combat Integration
5. **Implement opposed checks** - MIGHT vs STURDINESS for Push/Pull
6. **Implement stance system** - Duration-based buffs with restrictions
7. **Route Atgeir-Wielder abilities** through CombatEngine

### Phase 3: Advanced Mechanics
8. **Implement aura system** - Adjacent ally bonuses
9. **Implement Zone of Control** - Enemy debuffs in opposing row
10. **Implement reactive abilities** - Brace for Charge as Reaction

### Phase 4: Status Effects
11. **Define status effects** ([Slowed], [Off-Balance], [Disciplined Stance])
12. **Implement movement restriction** for Zone of Control
13. **Implement counter-attack triggers**

### Phase 5: Polish
14. **Add rank-specific icons** (Bronze/Silver/Gold)
15. **Implement row position indicators**
16. **Add [Reach] targeting visualization**

---

**End of Specification**
