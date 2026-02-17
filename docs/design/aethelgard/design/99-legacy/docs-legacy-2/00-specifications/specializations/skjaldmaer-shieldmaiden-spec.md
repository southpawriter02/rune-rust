# Skjaldmaer (Shieldmaiden) Specialization - Complete Specification

> **Specification ID**: SPEC-SPECIALIZATION-SKJALDMAER
> **Version**: 2.0
> **Last Updated**: 2025-11-27
> **Status**: Draft - Implementation Review

---

## Document Control

### Purpose
This document provides the complete specification for the Skjaldmaer (Shieldmaiden) specialization, including:
- Design philosophy and mechanical identity
- All 9 abilities with **exact formulas per rank**
- **Rank unlock requirements** (tree-progression based, NOT PP-based)
- **GUI display specifications per rank**
- Current implementation status
- Combat system integration points

This document serves as a template for documenting other specializations.

### Related Files
| Component | File Path | Status |
|-----------|-----------|--------|
| Service Implementation | `RuneAndRust.Engine/SkjaldmaerService.cs` | Implemented |
| Data Seeding | `RuneAndRust.Persistence/SkjaldmaerSeeder.cs` | **NEEDS UPDATE** (rank costs incorrect) |
| Tests | `RuneAndRust.Tests/SkjaldmaerSpecializationTests.cs` | Implemented |
| Specialization Tree UI | `RuneAndRust.DesktopUI/Views/SpecializationTreeView.axaml` | Generic (not Skjaldmaer-specific) |
| Combat UI | `RuneAndRust.DesktopUI/Views/CombatView.axaml` | No specialization integration |

### Change Log
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-11-27 | Initial specification |
| 2.0 | 2025-11-27 | Corrected rank unlock system (tree-based, not PP-based), added detailed per-rank specifications |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
|----------|-------|
| **Internal Name** | Skjaldmaer |
| **Display Name** | Shieldmaiden |
| **Specialization ID** | 26003 |
| **Archetype** | Warrior (ArchetypeID = 1) |
| **Path Type** | Coherent |
| **Mechanical Role** | Tank / Psychic Stress Mitigation |
| **Primary Attribute** | STURDINESS |
| **Secondary Attribute** | WILL |
| **Resource System** | Stamina |
| **Trauma Risk** | Low |
| **Icon** | :shield: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 10 PP | Higher than standard 3 PP (reflects power level) |
| **Minimum Legend** | 5 | Mid-game specialization |
| **Maximum Corruption** | 100 | Coherent path - no corruption restriction |
| **Minimum Corruption** | 0 | No minimum corruption |
| **Required Quest** | None | No quest prerequisite |

### 1.3 Design Philosophy

**Tagline**: "The Bastion of Coherence"

**Core Fantasy**: The Skjaldmaer is a living firewall against both physical trauma and mental breakdown. In a world where reality glitches, she shields not just bodies but sanity itself. Her shield is a grounding rod against the psychic scream of the Great Silence.

**Mechanical Identity**:
1. **Dual Protection**: Shields both HP and Psychic Stress simultaneously
2. **Trauma Economy Anchor**: Actively mitigates party Psychic Stress through abilities and auras
3. **WILL-Based Tanking**: Taunt system draws aggro with WILL-based projection of coherence
4. **Ultimate Sacrifice**: Capstone ability allows absorbing permanent Trauma to save allies

### 1.4 Specialization Description (Full Text)

> The bastion of coherence—a living firewall against both physical trauma and mental breakdown. In a world where reality glitches, the Skjaldmaer shields not just bodies but sanity itself. Her shield is a grounding rod against the psychic scream of the Great Silence. Her power comes from indomitable WILL channeled into protection, transforming the tank role from 'meat shield' to 'reality anchor.'
>
> This specialization provides dual protection: shields both HP and Psychic Stress simultaneously. As the Trauma Economy anchor, she actively mitigates party Psychic Stress through abilities and auras. Her taunt system draws aggro with WILL-based projection of coherence. Unparalleled damage reduction and HP pool make her the ultimate soak master.
>
> The ultimate expression is Bastion of Sanity—absorb Trauma to save an ally from permanent mental scarring.

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
- This creates a natural power curve: Capstone training is a major power spike for ALL trained abilities

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
[Sanctified      [Shield Bash]        [Oath of the
 Resolve]                              Protector]
 (Passive)         (Active)             (Active)
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
[Guardian's        [Shield Wall]      [Interposing
 Taunt]                                 Shield]
 (Active)           (Active)           (Reaction)
    │                     │                     │
    └─────────────────────┴─────────────────────┘
                          │
                          ▼
                TIER 3: MASTERY (5 PP each)
          ┌───────────────┴───────────────┐
          │                               │
    [Implacable              [Aegis of the
     Defense]                    Clan]
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
                [Bastion of Sanity]
                (Passive + Reaction)
```

### 3.2 Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Resource Cost | Key Effect |
|----|--------------|------|------|-------|---------------|------------|
| 26019 | Sanctified Resolve | 1 | Passive | 1→2→3 | None | +dice vs Fear/Stress |
| 26020 | Shield Bash | 1 | Active | 1→2→3 | 40 Stamina | Damage + Stagger |
| 26021 | Oath of the Protector | 1 | Active | 1→2→3 | 35 Stamina | +Soak, +Stress resist |
| 26022 | Guardian's Taunt | 2 | Active | 2→3 | 30 Stamina + Stress | Taunt enemies |
| 26023 | Shield Wall | 2 | Active | 2→3 | 45 Stamina | AoE defensive buff |
| 26024 | Interposing Shield | 2 | Reaction | 2→3 | 25 Stamina | Redirect critical hit |
| 26025 | Implacable Defense | 3 | Active | — | 40 Stamina | Debuff immunity |
| 26026 | Aegis of the Clan | 3 | Passive | — | None | Auto-protect stressed allies |
| 26027 | Bastion of Sanity | 4 | Passive+Reaction | — | 40 Stress + 1 Corruption | Absorb ally Trauma |

---

## 4. Tier 1 Abilities (Detailed Rank Specifications)

These abilities have 3 ranks. Rank progression is automatic based on tree investment.

---

### 4.1 Sanctified Resolve (ID: 26019)

**Type**: Passive | **Action**: Free Action (always active) | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | None (passive) |
| **Attribute Used** | WILL (for Resolve Checks) |

#### Description
Mental fortitude training grants resistance to Fear and Psychic Stress. The Skjaldmaer's mind is a fortress, unyielding against the horrors that shatter lesser warriors.

#### Rank Details

##### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:
- +1 bonus die to all WILL Resolve Checks vs [Fear] effects
- +1 bonus die to all WILL Resolve Checks vs Psychic Stress

**Formula**:
```
ResolveCheckDicePool = WILL + 1
```

**GUI Display**:
- Passive icon: Single shield with mind symbol
- Tooltip: "Sanctified Resolve (Rank 1): +1 die vs Fear and Psychic Stress checks"
- Color: Bronze border

**Combat Log Examples**:
- "Sanctified Resolve grants +1 die to Fear resistance"
- "Rolling WILL (3) + Sanctified Resolve (1) = 4 dice vs [Fear]"

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- +2 bonus dice to all WILL Resolve Checks vs [Fear] effects
- +2 bonus dice to all WILL Resolve Checks vs Psychic Stress

**Formula**:
```
ResolveCheckDicePool = WILL + 2
```

**GUI Display**:
- Passive icon: Double shield with mind symbol
- Tooltip: "Sanctified Resolve (Rank 2): +2 dice vs Fear and Psychic Stress checks"
- Color: Silver border
- **Rank-up notification**: "Sanctified Resolve has reached Rank 2!"

**Combat Log Examples**:
- "Sanctified Resolve (Rank 2) grants +2 dice to Fear resistance"
- "Rolling WILL (3) + Sanctified Resolve (2) = 5 dice vs [Fear]"

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- +3 bonus dice to all WILL Resolve Checks vs [Fear] effects
- +3 bonus dice to all WILL Resolve Checks vs Psychic Stress
- **NEW**: Reduce all ambient Psychic Stress gain by 10%

**Formulas**:
```
ResolveCheckDicePool = WILL + 3

AmbientStressGain = BaseAmbientStress * 0.90
```

**GUI Display**:
- Passive icon: Triple shield with glowing mind symbol
- Tooltip: "Sanctified Resolve (Rank 3): +3 dice vs Fear and Psychic Stress checks. Ambient Stress reduced by 10%."
- Color: Gold border
- Additional indicator: Small "-10% Stress" badge
- **Rank-up notification**: "Sanctified Resolve has reached Rank 3! You now resist ambient Psychic Stress."

**Combat Log Examples**:
- "Sanctified Resolve (Rank 3) grants +3 dice to Fear resistance"
- "Ambient Stress reduced by 10% (Sanctified Resolve)"

#### Implementation Status
- [x] Service method: `SkjaldmaerService.GetSanctifiedResolveBonus(rank)` - Returns 1/2/3
- [x] Service method: `SkjaldmaerService.GetSanctifiedResolveStressReduction(rank)` - Returns 0/0/0.10
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier1()`
- [ ] **FIX NEEDED**: Seeder has `CostToRank2 = 20` - should be removed/ignored
- [ ] GUI: Passive indicator with rank-specific icon
- [ ] GUI: Rank border color (Bronze/Silver/Gold)
- [ ] Combat: Integration with WILL Resolve Check system
- [ ] Combat: Ambient Stress reduction modifier (Rank 3)

---

### 4.2 Shield Bash (ID: 26020)

**Type**: Active | **Action**: Standard Action | **Target**: Single Enemy (Melee)

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 40 Stamina |
| **Attribute Used** | MIGHT (damage bonus) |
| **Damage Type** | Physical |
| **Status Effects** | [Staggered] |

#### Description
Slam shield into foe—a brutal statement of physical truth. The impact can stagger even the most stalwart opponents, and masters of this technique can send enemies reeling backward.

#### Rank Details

##### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:
- Damage: 1d8 + MIGHT (Physical)
- Stagger Chance: 50%
- If Stagger succeeds: Target gains [Staggered] for 1 turn

**Formulas**:
```
Damage = Roll(1d8) + MIGHT
StaggerCheck = Roll(1d100) <= 50
```

**GUI Display**:
- Ability button: Shield icon with impact mark
- Tooltip: "Shield Bash (Rank 1): 1d8+MIGHT damage, 50% Stagger chance. Cost: 40 Stamina"
- Damage preview: "Est. Damage: 5-12" (assuming MIGHT 4)
- Color: Bronze border

**Combat Log Examples**:
- "Shield Bash deals 9 damage! (1d8[5] + MIGHT[4])"
- "Shield Bash deals 7 damage! Target is Staggered!"
- "Shield Bash deals 6 damage. Stagger failed (rolled 67, needed ≤50)"

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- Damage: 2d8 + MIGHT (Physical)
- Stagger Chance: 65%
- If Stagger succeeds: Target gains [Staggered] for 1 turn

**Formulas**:
```
Damage = Roll(2d8) + MIGHT
StaggerCheck = Roll(1d100) <= 65
```

**GUI Display**:
- Ability button: Shield icon with double impact marks
- Tooltip: "Shield Bash (Rank 2): 2d8+MIGHT damage, 65% Stagger chance. Cost: 40 Stamina"
- Damage preview: "Est. Damage: 6-20" (assuming MIGHT 4)
- Color: Silver border
- **Rank-up notification**: "Shield Bash has reached Rank 2! Damage and Stagger chance increased."

**Combat Log Examples**:
- "Shield Bash (Rank 2) deals 14 damage! (2d8[6,4] + MIGHT[4])"
- "Shield Bash (Rank 2) deals 11 damage! Target is Staggered!"

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Damage: 3d8 + MIGHT (Physical)
- Stagger Chance: 75%
- If Stagger succeeds: Target gains [Staggered] for 1 turn
- **NEW**: If Stagger succeeds AND target is in Front Row, push target to Back Row

**Formulas**:
```
Damage = Roll(3d8) + MIGHT
StaggerCheck = Roll(1d100) <= 75
If (StaggerCheck AND Target.Row == Front):
    Target.Row = Back
```

**GUI Display**:
- Ability button: Shield icon with triple impact marks and arrow
- Tooltip: "Shield Bash (Rank 3): 3d8+MIGHT damage, 75% Stagger chance. Staggered enemies pushed to Back Row. Cost: 40 Stamina"
- Damage preview: "Est. Damage: 7-28" (assuming MIGHT 4)
- Color: Gold border
- **Rank-up notification**: "Shield Bash has reached Rank 3! Now pushes Staggered enemies to Back Row!"

**Combat Log Examples**:
- "Shield Bash (Rank 3) deals 19 damage! (3d8[5,6,4] + MIGHT[4])"
- "Shield Bash (Rank 3) deals 22 damage! Target is Staggered and pushed to Back Row!"

#### Implementation Status
- [x] Service method: `SkjaldmaerService.ExecuteShieldBash(caster, target, rank)`
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier1()`
- [ ] **FIX NEEDED**: Seeder has `CostToRank2 = 20` - should be removed/ignored
- [ ] GUI: Ability button with rank-specific icon
- [ ] GUI: Rank border color (Bronze/Silver/Gold)
- [ ] GUI: Target selection for single enemy
- [ ] Combat: Integration with CombatEngine.PlayerUseAbility()
- [ ] Status Effect: [Staggered] definition and icon

---

### 4.3 Oath of the Protector (ID: 26021)

**Type**: Active | **Action**: Standard Action | **Target**: Single Ally

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 1 (Foundation) |
| **PP Cost to Unlock** | 3 PP |
| **Ranks** | 3 |
| **Resource Cost** | 35 Stamina |
| **Buff Applied** | [Oath of the Protector] |

#### Description
Extend protective aura to single ally, shielding flesh and mind. The Skjaldmaer speaks words of ancient power, binding her fate to her ward's survival.

#### Rank Details

##### Rank 1 (Unlocked: When ability is learned)

**Mechanical Effect**:
- Target ally gains +2 Soak for 2 turns
- Target ally gains +1 bonus die to Psychic Stress resistance for 2 turns

**Formulas**:
```
Target.Soak += 2 (for 2 turns)
Target.StressResistanceDice += 1 (for 2 turns)
```

**GUI Display**:
- Ability button: Shield with protective glow
- Tooltip: "Oath of the Protector (Rank 1): Grant ally +2 Soak, +1 Stress resistance die for 2 turns. Cost: 35 Stamina"
- Target must be ally (highlight allies on activation)
- Color: Bronze border
- Buff icon on target: Small shield (bronze)
- Buff tooltip: "Oath of the Protector: +2 Soak, +1 Stress die (2 turns remaining)"

**Combat Log Examples**:
- "Oath of the Protector shields [Ally Name] (+2 Soak, +1 Stress resistance for 2 turns)"
- "[Ally Name]'s Oath of the Protector fades (0 turns remaining)"

---

##### Rank 2 (Unlocked: When 2 Tier 2 abilities are trained)

**Mechanical Effect**:
- Target ally gains +3 Soak for 2 turns
- Target ally gains +2 bonus dice to Psychic Stress resistance for 2 turns

**Formulas**:
```
Target.Soak += 3 (for 2 turns)
Target.StressResistanceDice += 2 (for 2 turns)
```

**GUI Display**:
- Ability button: Shield with stronger protective glow
- Tooltip: "Oath of the Protector (Rank 2): Grant ally +3 Soak, +2 Stress resistance dice for 2 turns. Cost: 35 Stamina"
- Color: Silver border
- Buff icon on target: Shield (silver)
- **Rank-up notification**: "Oath of the Protector has reached Rank 2! Protection increased."

**Combat Log Examples**:
- "Oath of the Protector (Rank 2) shields [Ally Name] (+3 Soak, +2 Stress resistance for 2 turns)"

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Target ally gains +4 Soak for 3 turns
- Target ally gains +2 bonus dice to Psychic Stress resistance for 3 turns
- **NEW**: Immediately cleanse 1 mental debuff from target (priority: [Fear] > [Disoriented] > [Charmed])

**Formulas**:
```
Target.Soak += 4 (for 3 turns)
Target.StressResistanceDice += 2 (for 3 turns)
Target.RemoveFirstMentalDebuff() // [Fear], [Disoriented], or [Charmed]
```

**GUI Display**:
- Ability button: Shield with radiant protective glow and cleanse sparkle
- Tooltip: "Oath of the Protector (Rank 3): Grant ally +4 Soak, +2 Stress resistance dice for 3 turns. Cleanses 1 mental debuff. Cost: 35 Stamina"
- Color: Gold border
- Buff icon on target: Radiant shield (gold)
- **Rank-up notification**: "Oath of the Protector has reached Rank 3! Now cleanses mental debuffs!"

**Combat Log Examples**:
- "Oath of the Protector (Rank 3) shields [Ally Name] (+4 Soak, +2 Stress resistance for 3 turns)"
- "Oath of the Protector (Rank 3) cleanses [Fear] from [Ally Name]!"

#### Implementation Status
- [x] Service method: `SkjaldmaerService.ExecuteOathOfProtector(caster, target, rank)`
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier1()`
- [ ] **FIX NEEDED**: Seeder has `CostToRank2 = 20` - should be removed/ignored
- [ ] GUI: Ability button with rank-specific icon
- [ ] GUI: Ally targeting mode
- [ ] GUI: Buff icon on target with duration
- [ ] Combat: Apply buff with duration tracking
- [ ] Combat: Mental debuff cleanse logic (Rank 3)

---

## 5. Tier 2 Abilities (Rank 2→3 Progression)

These abilities start at **Rank 2** when trained (reflecting their higher tier) and progress to **Rank 3** when the Capstone is trained.

---

### 5.1 Guardian's Taunt (ID: 26022)

**Type**: Active | **Action**: Standard Action | **Target**: AoE (Front Row or All Enemies)

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Skjaldmaer tree |
| **Ranks** | 2→3 (starts at Rank 2, Rank 3 with Capstone) |
| **Resource Cost** | 30 Stamina + Variable Psychic Stress |
| **Status Effects** | [Taunted] |

#### Description
Projection of coherent will draws even maddened creatures to attack. The Skjaldmaer's presence becomes a beacon of stability that enemies cannot ignore—but maintaining this projection takes a toll on her own psyche.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- All enemies in Front Row gain [Taunted] for 2 rounds
- [Taunted] enemies must target the Skjaldmaer with their attacks if able
- **Psychic Stress Cost**: Skjaldmaer gains 5 Psychic Stress

**Formulas**:
```
For each Enemy in FrontRow:
    Enemy.AddStatusEffect("Taunted", Duration: 2, TauntSource: Skjaldmaer)
Skjaldmaer.PsychicStress += 5
```

**GUI Display**:
- Ability button: Taunting pose with psychic waves
- Tooltip: "Guardian's Taunt (Rank 2): Taunt all Front Row enemies for 2 rounds. Cost: 30 Stamina, 5 Psychic Stress"
- Color: Silver border
- Shows number of enemies affected: "Will taunt 3 enemies"

**Combat Log Examples**:
- "Guardian's Taunt draws 3 enemies! (Skjaldmaer gains 5 Psychic Stress)"
- "Guardian's Taunt: [Enemy1], [Enemy2], [Enemy3] are now Taunted for 2 rounds"

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- **ALL enemies** (Front AND Back Row) gain [Taunted] for 2 rounds
- [Taunted] enemies must target the Skjaldmaer with their attacks if able
- **Reduced Psychic Stress Cost**: Skjaldmaer gains only 3 Psychic Stress

**Formulas**:
```
For each Enemy in AllEnemies:  // Now includes Back Row!
    Enemy.AddStatusEffect("Taunted", Duration: 2, TauntSource: Skjaldmaer)
Skjaldmaer.PsychicStress += 3  // Reduced from 5
```

**GUI Display**:
- Ability button: Taunting pose with expanded psychic waves
- Tooltip: "Guardian's Taunt (Rank 3): Taunt ALL enemies for 2 rounds. Cost: 30 Stamina, 3 Psychic Stress"
- Color: Gold border
- Shows: "Will taunt ALL 5 enemies"
- **Rank-up notification**: "Guardian's Taunt has reached Rank 3! Now taunts ALL enemies with reduced Stress cost!"

**Combat Log Examples**:
- "Guardian's Taunt (Rank 3) draws ALL 5 enemies! (Skjaldmaer gains 3 Psychic Stress)"
- "Guardian's Taunt: [Enemy1], [Enemy2], [Enemy3], [Enemy4], [Enemy5] are now Taunted for 2 rounds"

#### Implementation Status
- [x] Service method: `SkjaldmaerService.ExecuteGuardiansTaunt(caster, frontRow, backRow, rank)`
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier2()`
- [ ] GUI: Ability button showing dual costs
- [ ] GUI: Rank-specific display (Silver→Gold)
- [ ] GUI: Stress threshold warning
- [ ] Combat: Apply [Taunted] status
- [ ] Status Effect: [Taunted] definition - forces targeting

---

### 5.2 Shield Wall (ID: 26023)

**Type**: Active | **Action**: Standard Action | **Target**: Self + Adjacent Front Row Allies

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Skjaldmaer tree |
| **Ranks** | 2→3 (starts at Rank 2, Rank 3 with Capstone) |
| **Resource Cost** | 45 Stamina |
| **Status Effects** | [Fortified] |

#### Description
Plant feet creating bastion of physical and metaphysical stability. The Skjaldmaer becomes an immovable anchor, extending this stability to nearby allies.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- Self and all adjacent Front Row allies gain:
  - +3 Soak for 2 turns
  - Immunity to Push/Pull/Knockback effects for 2 turns
  - +1 bonus die to Psychic Stress resistance for 2 turns
- Affected characters gain [Fortified] status

**Formulas**:
```
AffectedTargets = [Self] + GetAdjacentFrontRowAllies(Self)
For each Target in AffectedTargets:
    Target.Soak += 3 (for 2 turns)
    Target.AddStatusEffect("Fortified", Duration: 2)
    Target.StressResistanceDice += 1 (for 2 turns)
```

**GUI Display**:
- Ability button: Interlocked shields
- Tooltip: "Shield Wall (Rank 2): Self + adjacent allies gain +3 Soak, immunity to forced movement, +1 Stress resistance die for 2 turns. Cost: 45 Stamina"
- Color: Silver border
- Preview: Highlight affected allies before confirmation

**Combat Log Examples**:
- "Shield Wall protects [Skjaldmaer], [Ally1], [Ally2] (+3 Soak, Fortified for 2 turns)"
- "[Enemy] attempts to push [Ally1] - blocked by Fortified!"

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- Self and all adjacent Front Row allies gain:
  - +4 Soak for 3 turns (increased)
  - Immunity to Push/Pull/Knockback effects for 3 turns
  - +2 bonus dice to Psychic Stress resistance for 3 turns (increased)
- Affected characters gain [Fortified] status
- **NEW**: Also grants immunity to [Stun] for duration

**Formulas**:
```
AffectedTargets = [Self] + GetAdjacentFrontRowAllies(Self)
For each Target in AffectedTargets:
    Target.Soak += 4 (for 3 turns)  // Increased from +3
    Target.AddStatusEffect("Fortified", Duration: 3)  // Increased from 2
    Target.StressResistanceDice += 2 (for 3 turns)  // Increased from +1
    Target.AddImmunity("Stun", Duration: 3)  // NEW
```

**GUI Display**:
- Ability button: Interlocked shields with radiant glow
- Tooltip: "Shield Wall (Rank 3): Self + adjacent allies gain +4 Soak, immunity to forced movement AND Stun, +2 Stress resistance dice for 3 turns. Cost: 45 Stamina"
- Color: Gold border
- **Rank-up notification**: "Shield Wall has reached Rank 3! Increased protection, longer duration, and Stun immunity!"

**Combat Log Examples**:
- "Shield Wall (Rank 3) protects [Skjaldmaer], [Ally1], [Ally2] (+4 Soak, Fortified, Stun Immune for 3 turns)"
- "[Enemy] attempts to Stun [Ally1] - blocked by Shield Wall!"

#### Implementation Status
- [ ] Service method: NOT YET IMPLEMENTED (needs ExecuteShieldWall)
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier2()`
- [ ] GUI: Ability button with AoE preview
- [ ] GUI: Rank-specific display (Silver→Gold)
- [ ] Combat: Apply buff to multiple targets
- [ ] Status Effect: [Fortified] definition

---

### 5.3 Interposing Shield (ID: 26024)

**Type**: Reaction | **Action**: Reaction | **Target**: Adjacent Ally

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 2 (Advanced) |
| **PP Cost to Unlock** | 4 PP |
| **Prerequisite** | 8 PP invested in Skjaldmaer tree |
| **Ranks** | 2→3 (starts at Rank 2, Rank 3 with Capstone) |
| **Resource Cost** | 25 Stamina |
| **Trigger** | Adjacent ally is hit by a Critical Hit |
| **Limit** | Once per round |

#### Description
React to incoming Critical Hit on adjacent ally, redirecting to self. The Skjaldmaer interposes her shield at the last moment, taking the brunt of the blow.

#### Rank Details

##### Rank 2 (Starting Rank - When ability is learned)

**Mechanical Effect**:
- **Trigger**: When an adjacent ally would take damage from a Critical Hit
- **Effect**: Redirect the attack to the Skjaldmaer
- **Damage Reduction**: Skjaldmaer takes 50% of the original damage
- **Original Target**: Takes 0 damage

**Formulas**:
```
Trigger: Ally.IsAdjacent(Skjaldmaer) AND IncomingAttack.IsCritical
If Triggered:
    OriginalDamage = IncomingAttack.Damage
    Skjaldmaer.HP -= Floor(OriginalDamage * 0.50)
    Ally.HP -= 0
    Skjaldmaer.Stamina -= 25
```

**GUI Display**:
- Reaction prompt with Silver border
- Tooltip: "Interposing Shield (Rank 2): Intercept critical hit, take 50% damage. Cost: 25 Stamina"

**Reaction Prompt**:
```
┌─────────────────────────────────────────────┐
│     INTERPOSING SHIELD (Rank 2)             │
├─────────────────────────────────────────────┤
│ [Ally Name] is about to take a CRITICAL HIT │
│ for [X] damage!                             │
│                                             │
│ Intercept?                                  │
│ • You will take: [X × 50%] = [Y] damage     │
│ • Cost: 25 Stamina                          │
│                                             │
│ [Current Stamina: Z/Max]                    │
│                                             │
│    [INTERCEPT]        [DECLINE]             │
└─────────────────────────────────────────────┘
```

**Combat Log Examples**:
- "REACTION: Interposing Shield! [Skjaldmaer] intercepts critical hit meant for [Ally]!"
- "[Skjaldmaer] takes 15 damage (50% of 30)"
- "[Ally] is protected from critical hit!"

---

##### Rank 3 (Unlocked: When Capstone is trained)

**Mechanical Effect**:
- **Trigger**: When an adjacent ally would take damage from a Critical Hit
- **Effect**: Redirect the attack to the Skjaldmaer
- **Damage Reduction**: Skjaldmaer takes only 35% of the original damage (improved)
- **Original Target**: Takes 0 damage
- **NEW**: Reflect 15% of original damage back to attacker

**Formulas**:
```
Trigger: Ally.IsAdjacent(Skjaldmaer) AND IncomingAttack.IsCritical
If Triggered:
    OriginalDamage = IncomingAttack.Damage
    Skjaldmaer.HP -= Floor(OriginalDamage * 0.35)  // Reduced from 50%
    Ally.HP -= 0
    Attacker.HP -= Floor(OriginalDamage * 0.15)  // NEW: Reflect damage
    Skjaldmaer.Stamina -= 25
```

**GUI Display**:
- Reaction prompt with Gold border
- Tooltip: "Interposing Shield (Rank 3): Intercept critical hit, take only 35% damage, reflect 15% to attacker. Cost: 25 Stamina"
- **Rank-up notification**: "Interposing Shield has reached Rank 3! Reduced damage taken and now reflects damage!"

**Reaction Prompt**:
```
┌─────────────────────────────────────────────┐
│     INTERPOSING SHIELD (Rank 3)             │
├─────────────────────────────────────────────┤
│ [Ally Name] is about to take a CRITICAL HIT │
│ for [X] damage!                             │
│                                             │
│ Intercept?                                  │
│ • You will take: [X × 35%] = [Y] damage     │
│ • Attacker takes: [X × 15%] = [Z] reflected │
│ • Cost: 25 Stamina                          │
│                                             │
│ [Current Stamina: W/Max]                    │
│                                             │
│    [INTERCEPT]        [DECLINE]             │
└─────────────────────────────────────────────┘
```

**Combat Log Examples**:
- "REACTION: Interposing Shield (Rank 3)! [Skjaldmaer] intercepts critical hit meant for [Ally]!"
- "[Skjaldmaer] takes 11 damage (35% of 30)"
- "[Enemy] takes 5 reflected damage!"
- "[Ally] is protected from critical hit!"

#### Implementation Status
- [ ] Service method: NOT YET IMPLEMENTED (needs ExecuteInterposingShield)
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier2()`
- [ ] **GUI: Reaction prompt system (CRITICAL GAP)**
- [ ] GUI: Rank-specific prompt display (Silver→Gold)
- [ ] Combat: Detect critical hit trigger
- [ ] Combat: Reaction timing framework
- [ ] Combat: Reflect damage logic (Rank 3)

---

## 6. Tier 3 Abilities (No Ranks)

Tier 3 abilities are powerful effects that do **not** have rank progression. They function at full power when unlocked.

---

### 6.1 Implacable Defense (ID: 26025)

**Type**: Active | **Action**: Standard Action | **Target**: Self

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Skjaldmaer tree |
| **Ranks** | None (full power when unlocked) |
| **Resource Cost** | 40 Stamina |

#### Description
Achieve state of perfect focus—immovable against physical and mental assault. For a brief time, the Skjaldmaer becomes an unshakeable pillar.

#### Mechanical Effect
- For 3 turns, Skjaldmaer is IMMUNE to:
  - [Stun]
  - [Staggered]
  - [Knocked Down]
  - [Fear]
  - [Disoriented]
- Additionally: +2 Soak for 3 turns
- Aura effect: Adjacent allies are immune to [Fear] for 3 turns

**Formulas**:
```
Skjaldmaer.AddImmunity(["Stun", "Staggered", "Knocked Down", "Fear", "Disoriented"], Duration: 3)
Skjaldmaer.Soak += 2 (for 3 turns)

For each AdjacentAlly:
    AdjacentAlly.AddImmunity(["Fear"], Duration: 3)
```

#### GUI Display
- Ability button: Stalwart stance with immunity aura
- Tooltip: "Implacable Defense: Become immune to Stun, Stagger, Knockdown, Fear, Disoriented for 3 turns. +2 Soak. Adjacent allies immune to Fear. Cost: 40 Stamina"
- Self-buff icon: Glowing shield with crossed-out debuff icons
- Aura indicator on adjacent allies: Small shield icon

#### Combat Log Examples
- "Implacable Defense activated! [Skjaldmaer] is immune to control effects for 3 turns"
- "[Enemy] attempts to Stun [Skjaldmaer] - IMMUNE!"
- "[Ally] is protected from Fear by Implacable Defense aura"

#### Implementation Status
- [ ] Service method: NOT YET IMPLEMENTED
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier3()`
- [ ] GUI: Self-targeting ability button
- [ ] GUI: Immunity buff display
- [ ] GUI: Aura indicator on allies
- [ ] Combat: Immunity system integration

---

### 6.2 Aegis of the Clan (ID: 26026)

**Type**: Passive | **Action**: Free (Automatic) | **Target**: Allies in Crisis

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 3 (Mastery) |
| **PP Cost to Unlock** | 5 PP |
| **Prerequisite** | 16 PP invested in Skjaldmaer tree |
| **Ranks** | None (full power when unlocked) |
| **Resource Cost** | None (automatic) |
| **Trigger** | Ally's Psychic Stress reaches 66%+ |
| **Limit** | Once per ally per combat |

#### Description
Automatic protection triggers when ally enters mental crisis. The Skjaldmaer's bond with her clanmates runs deep—she senses their distress and responds instinctively.

#### Mechanical Effect
- **Trigger**: When any ally's Psychic Stress reaches or exceeds 66% of maximum (66/100)
- **Effect**: Automatically apply Oath of the Protector to that ally for 2 turns
- **Bonus**: Immediately reduce that ally's Psychic Stress by 10
- **Limit**: Can only trigger once per ally per combat

**Formulas**:
```
Trigger: Ally.PsychicStress >= 66 AND NOT AegisUsedOn(Ally)

If Triggered:
    ApplyOathOfProtector(Ally, Duration: 2)  // +3 Soak, +2 Stress dice
    Ally.PsychicStress -= 10
    MarkAegisUsedOn(Ally)
```

#### GUI Display
- Passive icon: Vigilant eye over shield
- When triggered: Flash notification "Aegis of the Clan protects [Ally]!"
- Buff icon appears on protected ally
- Ally's Stress bar shows -10 reduction animation

#### Combat Log Examples
- "AEGIS OF THE CLAN: [Ally] enters mental crisis (66% Stress)!"
- "Aegis of the Clan automatically shields [Ally] (+3 Soak, +2 Stress resistance for 2 turns)"
- "[Ally]'s Psychic Stress reduced by 10 (Aegis of the Clan)"

#### Implementation Status
- [ ] Service method: NOT YET IMPLEMENTED
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerTier3()`
- [ ] Combat: Stress threshold monitoring
- [ ] Combat: Automatic trigger system
- [ ] GUI: Trigger notification
- [ ] GUI: Stress reduction animation

---

## 7. Capstone Ability (No Ranks)

The Capstone is a unique, powerful ability that does **not** have rank progression. When trained, it also upgrades all Tier 1 and Tier 2 abilities to Rank 3.

---

### 7.1 Bastion of Sanity (ID: 26027)

**Type**: Passive + Reaction | **Action**: Passive / Reaction | **Target**: Aura (All Allies) / Single Ally in Crisis

#### Overview
| Property | Value |
|----------|-------|
| **Tier** | 4 (Capstone) |
| **PP Cost to Unlock** | 6 PP |
| **Prerequisite** | 24 PP invested in tree + both Tier 3 abilities |
| **Ranks** | None (full power when unlocked) |
| **Resource Cost** | Reaction: 40 Psychic Stress + 1 Corruption |
| **Trigger** | Ally would gain permanent Trauma |
| **Limit** | Reaction: Once per combat |
| **Special** | Training this ability upgrades all Tier 1 & Tier 2 abilities to Rank 3 |

#### Description
Become living Runic Anchor—a kernel of stable reality. The ultimate expression of the Skjaldmaer's protective nature: she can absorb the psychic wounds that would permanently scar her allies, taking the madness into herself.

#### Component 1: Passive Aura (Always Active While in Front Row)

**Mechanical Effect**:
- While Skjaldmaer is in Front Row position:
  - All allies in the same row gain +1 WILL
  - All allies in the same row reduce ambient Psychic Stress gain by 10%

**Formulas**:
```
If Skjaldmaer.Position.Row == Front:
    For each Ally in FrontRow:
        Ally.WILL += 1 (temporary)
        Ally.AmbientStressModifier *= 0.90
```

**GUI Display**:
- Aura indicator: Golden glow emanating from Skjaldmaer
- Buff icon on affected allies: Mind shield with +1
- Tooltip on ally buff: "Bastion Aura: +1 WILL, -10% Ambient Stress (from [Skjaldmaer])"

#### Component 2: Reaction (Once Per Combat)

**Trigger**: An ally would gain permanent Trauma from reaching Breaking Point

**Mechanical Effect**:
- Skjaldmaer absorbs the Trauma instead of the ally
- **Cost to Skjaldmaer**:
  - Gain 40 Psychic Stress
  - Gain 1 Corruption
- **Result for Ally**: Avoids gaining the Trauma entirely

**Formulas**:
```
Trigger: Ally.WouldGainTrauma(TraumaType)

If Player accepts:
    Ally.CancelTrauma()
    Skjaldmaer.PsychicStress += 40
    Skjaldmaer.Corruption += 1
    MarkBastionUsedThisCombat()
```

#### GUI Display - REACTION PROMPT (CRITICAL)

When ally would gain Trauma, display prompt:

```
┌─────────────────────────────────────────────────────┐
│              BASTION OF SANITY                      │
├─────────────────────────────────────────────────────┤
│ [Ally Name] is about to gain permanent Trauma:      │
│                                                     │
│   ╔═══════════════════════════════════════════╗     │
│   ║  [TRAUMA NAME]                            ║     │
│   ║  [Trauma description/effect]              ║     │
│   ╚═══════════════════════════════════════════╝     │
│                                                     │
│ Absorb this Trauma to save [Ally]?                  │
│                                                     │
│ ┌─────────────────────────────────────────────┐     │
│ │ COST TO YOU:                                │     │
│ │   • +40 Psychic Stress                      │     │
│ │   • +1 Corruption (PERMANENT)               │     │
│ │                                             │     │
│ │ Your current Stress: [X]/100                │     │
│ │ Your current Corruption: [Y]                │     │
│ └─────────────────────────────────────────────┘     │
│                                                     │
│ ⚠️ This can only be used ONCE per combat            │
│                                                     │
│    [ABSORB TRAUMA]        [LET IT PASS]             │
└─────────────────────────────────────────────────────┘
```

- Show warning if absorbing would push Skjaldmaer to Breaking Point
- Timer: 10 seconds to decide (default: Let It Pass)

#### Combat Log Examples
- "BASTION OF SANITY AURA: [Ally1], [Ally2] gain +1 WILL, -10% Stress gain"
- "[Ally] reaches Breaking Point! Would gain Trauma: [Shattered Confidence]"
- "BASTION OF SANITY TRIGGERED! [Skjaldmaer] absorbs [Ally]'s Trauma!"
- "[Skjaldmaer] gains 40 Psychic Stress and 1 Corruption"
- "[Ally] is saved from permanent Trauma!"

#### Implementation Status
- [x] Service method: `SkjaldmaerService.TriggerBastionOfSanity(skjaldmaer, ally, trauma, alreadyUsed)`
- [x] Service method: `SkjaldmaerService.GetBastionOfSanityAura()` - Returns (1, 0.10)
- [x] Data seeded in `SkjaldmaerSeeder.SeedSkjaldmaerCapstone()`
- [ ] **GUI: Reaction prompt for Trauma absorption (CRITICAL GAP)**
- [ ] GUI: Aura indicator on Skjaldmaer and affected allies
- [ ] Combat: Breaking Point detection in TraumaEconomyService
- [ ] Combat: Reaction timing framework

---

## 8. Status Effect Definitions

These status effects must be defined for Skjaldmaer abilities to function:

### 8.1 [Staggered]

| Property | Value |
|----------|-------|
| **Applied By** | Shield Bash |
| **Duration** | 1 turn |
| **Icon** | Dizzy stars over head |
| **Color** | Yellow |

**Effects**:
- -1 to Defense rolls
- Cannot take Reaction abilities
- Movement costs +1 additional movement point

**GUI Display**:
- Icon appears in target's status bar
- Tooltip: "Staggered: -1 Defense, cannot React, movement slowed"

---

### 8.2 [Taunted]

| Property | Value |
|----------|-------|
| **Applied By** | Guardian's Taunt |
| **Duration** | 2 turns |
| **Icon** | Angry red arrow pointing at taunter |
| **Color** | Red |
| **Data** | TauntSourceId (who applied the taunt) |

**Effects**:
- Must target the taunt source with attacks if able
- Cannot voluntarily move away from taunt source
- If taunt source is not valid target (dead, out of range), taunt breaks

**GUI Display**:
- Icon on taunted enemy shows arrow pointing to Skjaldmaer
- Enemy targeting UI highlights only Skjaldmaer when taunted
- Tooltip: "Taunted: Must attack [Skjaldmaer]"

---

### 8.3 [Fortified]

| Property | Value |
|----------|-------|
| **Applied By** | Shield Wall |
| **Duration** | 2 turns |
| **Icon** | Planted feet with ground crack |
| **Color** | Brown/Stone |

**Effects**:
- Immune to Push effects
- Immune to Pull effects
- Immune to Knockback effects
- Immune to Knockdown effects

**GUI Display**:
- Icon shows rooted stance
- When enemy attempts forced movement: "IMMUNE (Fortified)"
- Tooltip: "Fortified: Cannot be forcibly moved"

---

### 8.4 [Oath of the Protector]

| Property | Value |
|----------|-------|
| **Applied By** | Oath of the Protector, Aegis of the Clan |
| **Duration** | 2-3 turns (varies by source) |
| **Icon** | Shield with protective glow |
| **Color** | Blue |
| **Data** | SoakBonus, StressResistanceDice, SourceCharacter |

**Effects**:
- +2/+3/+4 Soak (varies by rank)
- +1/+2 bonus dice to Psychic Stress resistance (varies by rank)

**GUI Display**:
- Icon shows shield with number indicating Soak bonus
- Tooltip: "Oath of the Protector: +[X] Soak, +[Y] Stress dice ([Z] turns)"
- Shows source: "From: [Skjaldmaer]"

---

## 9. Data Model Corrections Required

### 9.1 SkjaldmaerSeeder.cs Updates

The current seeder has incorrect rank cost values. These need to be corrected:

**REMOVE or SET TO 0**:
```csharp
// INCORRECT - Current seeder has:
CostToRank2 = 20,  // WRONG - ranks are not purchased
CostToRank3 = 0,

// CORRECT - Should be:
CostToRank2 = 0,   // Rank 2 unlocks automatically (2 Tier 2 abilities)
CostToRank3 = 0,   // Rank 3 unlocks automatically (Capstone trained)
```

### 9.2 AbilityData.cs Updates

Add fields to support tree-based rank progression:

```csharp
public class AbilityData
{
    // Existing fields...

    // NEW: Rank unlock requirements (replaces CostToRank2/CostToRank3)
    public RankUnlockRequirements RankRequirements { get; set; } = new();
}

public class RankUnlockRequirements
{
    // Rank 1: Always available when ability is learned
    // Rank 2 requirement:
    public int Rank2RequiredTier2Abilities { get; set; } = 2;  // Default: 2 Tier 2 abilities

    // Rank 3 requirement:
    public bool Rank3RequiresCapstone { get; set; } = true;  // Default: Capstone must be trained
}
```

### 9.3 Character Ability Rank Calculation

```csharp
public int GetAbilityRank(PlayerCharacter character, AbilityData ability)
{
    var specProgress = GetSpecializationProgress(character, ability.SpecializationID);

    // Check if Capstone is trained (affects Tier 1 and Tier 2)
    bool hasCapstone = specProgress.UnlockedAbilities.Any(a => a.TierLevel == 4);

    // Count Tier 2 abilities unlocked (affects Tier 1)
    int tier2Count = specProgress.UnlockedAbilities.Count(a => a.TierLevel == 2);

    switch (ability.TierLevel)
    {
        case 1:  // Tier 1: Ranks 1→2→3
            if (hasCapstone)
                return 3;
            else if (tier2Count >= 2)
                return 2;
            else
                return 1;

        case 2:  // Tier 2: Ranks 2→3 (starts at Rank 2)
            if (hasCapstone)
                return 3;
            else
                return 2;

        case 3:  // Tier 3: No ranks
        case 4:  // Capstone: No ranks
        default:
            return 0;  // 0 indicates "no rank system"
    }
}

/// <summary>
/// Check if an ability uses the rank system
/// </summary>
public bool AbilityHasRanks(AbilityData ability)
{
    return ability.TierLevel == 1 || ability.TierLevel == 2;
}

/// <summary>
/// Get the starting rank for an ability when it's unlocked
/// </summary>
public int GetStartingRank(AbilityData ability)
{
    return ability.TierLevel switch
    {
        1 => 1,  // Tier 1 starts at Rank 1
        2 => 2,  // Tier 2 starts at Rank 2
        _ => 0   // Tier 3+ have no ranks
    };
}
```

---

## 10. GUI Rank Display Requirements

### 10.1 Ability Card Rank Indicators

Each ability card should show current rank with visual distinction:

| Rank | Border Color | Icon Enhancement | Badge |
|------|--------------|------------------|-------|
| 1 | Bronze (#CD7F32) | Base icon | "I" |
| 2 | Silver (#C0C0C0) | Enhanced glow | "II" |
| 3 | Gold (#FFD700) | Radiant glow + particles | "III" |

### 10.2 Rank Progression Indicators

In the Specialization Tree View, show rank unlock progress:

```
┌─────────────────────────────────────┐
│ Shield Bash                    [I]  │ ← Current rank badge
│ ─────────────────────────────────── │
│ Rank 2 unlocks: ██░░ 1/2 Tier 2     │ ← Progress to next rank
│ Rank 3 unlocks: ░░░░ Capstone       │
└─────────────────────────────────────┘
```

### 10.3 Rank-Up Notifications

When a rank unlocks (e.g., training second Tier 2 ability):

```
┌─────────────────────────────────────────┐
│ ⭐ ABILITIES RANKED UP! ⭐              │
├─────────────────────────────────────────┤
│ The following abilities reached Rank 2: │
│                                         │
│ • Sanctified Resolve                    │
│   +1 → +2 dice vs Fear/Stress           │
│                                         │
│ • Shield Bash                           │
│   1d8 → 2d8 damage, 50% → 65% Stagger   │
│                                         │
│ • Oath of the Protector                 │
│   +2 → +3 Soak, +1 → +2 Stress dice     │
│                                         │
│              [AWESOME!]                 │
└─────────────────────────────────────────┘
```

---

## 11. Implementation Priority

### Phase 1: Critical (Foundation)
1. **Fix SkjaldmaerSeeder.cs** - Remove/correct CostToRank2 values
2. **Implement rank calculation logic** based on tree progression
3. **Add Stamina/Psychic Stress display** to CombatView
4. **Define status effects** ([Staggered], [Taunted], [Fortified])

### Phase 2: Combat Integration
5. **Route Skjaldmaer abilities** through CombatEngine
6. **Implement ability execution** for all 9 abilities
7. **Create ability selection dialog** in combat

### Phase 3: Reaction System
8. **Build reaction prompt framework**
9. **Implement Interposing Shield** trigger and prompt
10. **Implement Bastion of Sanity** trigger and prompt

### Phase 4: Polish
11. **Add rank-specific icons** (Bronze/Silver/Gold)
12. **Implement rank-up notifications**
13. **Add passive ability indicators**
14. **Implement aura visual effects**

---

## 12. Testing Requirements

### 12.1 Rank Progression Tests

```csharp
// === TIER 1 RANK TESTS ===

[Test]
public void Tier1Abilities_StartAtRank1_WhenUnlocked()
{
    // When: Unlock Shield Bash (Tier 1)
    // Then: Shield Bash is Rank 1
}

[Test]
public void Tier1Abilities_ProgressToRank2_When2Tier2AbilitiesUnlocked()
{
    // Given: Shield Bash unlocked at Rank 1
    // When: Unlock Guardian's Taunt and Shield Wall (2 Tier 2 abilities)
    // Then: Shield Bash is now Rank 2
    // And: All other Tier 1 abilities are also Rank 2
}

[Test]
public void Tier1Abilities_ProgressToRank3_WhenCapstoneUnlocked()
{
    // Given: Shield Bash at Rank 2
    // When: Unlock Bastion of Sanity (Capstone)
    // Then: Shield Bash is now Rank 3
    // And: All Tier 1 abilities are Rank 3
}

// === TIER 2 RANK TESTS ===

[Test]
public void Tier2Abilities_StartAtRank2_WhenUnlocked()
{
    // When: Unlock Guardian's Taunt (Tier 2)
    // Then: Guardian's Taunt is Rank 2 (NOT Rank 1)
}

[Test]
public void Tier2Abilities_ProgressToRank3_WhenCapstoneUnlocked()
{
    // Given: Guardian's Taunt at Rank 2
    // When: Unlock Bastion of Sanity (Capstone)
    // Then: Guardian's Taunt is now Rank 3
    // And: All Tier 2 abilities are Rank 3
}

// === TIER 3 & CAPSTONE TESTS ===

[Test]
public void Tier3Abilities_HaveNoRanks()
{
    // When: Unlock Implacable Defense (Tier 3)
    // Then: GetAbilityRank returns 0 (no rank system)
    // And: AbilityHasRanks returns false
}

[Test]
public void Capstone_HasNoRanks()
{
    // When: Unlock Bastion of Sanity (Capstone)
    // Then: GetAbilityRank returns 0 (no rank system)
    // And: AbilityHasRanks returns false
}

[Test]
public void Capstone_UpgradesAllTier1AndTier2ToRank3()
{
    // Given: 3 Tier 1 abilities at Rank 2, 3 Tier 2 abilities at Rank 2
    // When: Unlock Bastion of Sanity (Capstone)
    // Then: All 6 abilities are now Rank 3
}
```

### 12.2 Ability Effect Tests Per Rank

```csharp
[Test]
public void ShieldBash_Rank1_Deals1d8PlusMightDamage()
{
    // Expected: 1d8 + 4 (MIGHT) = 5-12 damage
}

[Test]
public void ShieldBash_Rank2_Deals2d8PlusMightDamage()
{
    // Expected: 2d8 + 4 (MIGHT) = 6-20 damage
}

[Test]
public void ShieldBash_Rank3_PushesOnStagger()
{
    // When: Shield Bash at Rank 3 staggers enemy in Front Row
    // Then: Enemy is moved to Back Row
}
```

---

**End of Specification**
