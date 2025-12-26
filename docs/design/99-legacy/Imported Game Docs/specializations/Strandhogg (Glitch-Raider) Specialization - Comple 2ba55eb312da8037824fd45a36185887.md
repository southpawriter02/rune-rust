# Strandhogg (Glitch-Raider) Specialization - Complete Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Specification ID: SPEC-SPECIALIZATION-STRANDHOGG
Version: 1.0
Last Updated: 2025-11-27
Status: Draft - Implementation Review
> 

---

## Document Control

### Purpose

This document provides the complete specification for the Strandhogg (Glitch-Raider) specialization.

### Related Files

| Component | File Path | Status |
| --- | --- | --- |
| Service Implementation | `RuneAndRust.Engine/StrandhoggService.cs` | Implemented |
| Data Seeding | `RuneAndRust.Persistence/StrandhoggSeeder.cs` | Implemented |

### Change Log

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | 2025-11-27 | Initial specification from StrandhoggSeeder |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
| --- | --- |
| **Internal Name** | Strandhogg |
| **Display Name** | Glitch-Raider |
| **Specialization ID** | 25001 |
| **Archetype** | Skirmisher (ArchetypeID = 4) |
| **Path Type** | Coherent |
| **Mechanical Role** | Mobile Burst DPS / Momentum Fighter |
| **Primary Attribute** | FINESSE |
| **Secondary Attribute** | MIGHT |
| **Resource System** | Stamina + Momentum |
| **Trauma Risk** | Low |
| **Icon** | :crossed_swords: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
| --- | --- | --- |
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 5 | Mid-game specialization |

### 1.3 Design Philosophy

**Tagline**: "The Momentum Striker"

**Core Fantasy**: The Strandhogg is the kinetic blur, the glitch-raider who exploits unstable physics to move impossibly fast. You build Momentum through movement and strikes, then spend it on devastating executions. You are the whirlwind that strikes from unexpected angles and vanishes before retaliation.

**Mechanical Identity**:

1. **Momentum Resource System**: Builds Momentum (0-100) through movement and strikes
2. **Hit-and-Run**: Attack and reposition in the same turn
3. **Debuff Exploitation**: Deals bonus damage and generates extra Momentum vs debuffed enemies
4. **Kinetic Violence**: Executes devastating finishers by spending accumulated Momentum

### 1.4 The Momentum Resource System

**Momentum Mechanics**:

- **Range**: 0-100
- **Starting Combat**: Begins with 20-30 Momentum (depending on Harrier's Alacrity rank)
- **Generation**: Movement, attacks, and kills generate Momentum
- **Spending**: Powerful abilities consume Momentum
- **Decay**: Decays slowly out of combat

---

## 2. Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Key Effect |
| --- | --- | --- | --- | --- | --- |
| 25001 | Harrier's Alacrity I | 1 | Passive | 1→2→3 | Start with Momentum + Vigilance bonus |
| 25002 | Reaver's Strike | 1 | Active | 1→2→3 | Basic attack + 15 Momentum |
| 25003 | Dread Charge | 1 | Active | 1→2→3 | Move + attack + [Disoriented] |
| 25004 | Tidal Rush | 2 | Passive | 2→3 | Bonus Momentum vs debuffed enemies |
| 25005 | Harrier's Whirlwind | 2 | Active | 2→3 | Attack + free reposition |
| 25006 | Vicious Flank | 2 | Active | 2→3 | +50% damage vs debuffed |
| 25007 | No Quarter | 3 | Passive | — | Free move + Momentum on kill |
| 25008 | Savage Harvest | 3 | Active | — | Massive execution + refund on kill |
| 25009 | Riptide of Carnage | 4 | Active | — | 4 attacks in one turn |

---

## 3. Tier 1 Abilities

### 3.1 Harrier's Alacrity I (ID: 25001)

**Type**: Passive | **Target**: Self

### Rank Details

**Rank 1**: Start every combat with 20 Momentum. Gain +2 bonus to Vigilance (turn order).

**Rank 2**: Start with 20 Momentum. Vigilance bonus +3.

**Rank 3**: Start with 30 Momentum. Vigilance bonus +3.

---

### 3.2 Reaver's Strike (ID: 25002)

**Type**: Active | **Cost**: 35 Stamina | **Target**: Single Enemy (Melee)

### Rank Details

**Rank 1**: FINESSE-based melee attack dealing weapon damage + MIGHT. Generate 15 Momentum on hit.

**Rank 2**: Cost 30 Stamina. Damage +1d6. Generate 15 Momentum.

**Rank 3**: Damage +2d6 total. When hitting debuffed enemy, generate +10 bonus Momentum (25 total).

---

### 3.3 Dread Charge (ID: 25003)

**Type**: Active | **Cost**: 40 Stamina | **Cooldown**: 3 turns

### Rank Details

**Rank 1**: Move from Back Row to Front Row, then attack for 2d10 + MIGHT. Apply [Disoriented] (1 turn) and 10 Psychic Stress to target. Generate 10 Momentum.

**Rank 2**: 3d10 + MIGHT. [Disoriented] 2 turns. Generate 15 Momentum.

**Rank 3**: Can charge from Front Row into enemy Back Row. Damage 3d10 + MIGHT.

---

## 4. Tier 2 Abilities

### 4.1 Tidal Rush (ID: 25004)

**Type**: Passive | **Target**: Self

### Rank Details

**Rank 2**: When hitting enemy with mental/control debuffs ([Disoriented], [Stunned], [Feared], [Slowed], [Rooted]), generate +10 bonus Momentum.

**Rank 3**: +15 bonus Momentum. Also works vs enemies with DoTs ([Bleeding], [Burning], [Poisoned]).

---

### 4.2 Harrier's Whirlwind (ID: 25005)

**Type**: Active | **Cost**: 45 Stamina + 30 Momentum | **Cooldown**: 2 turns

### Rank Details

**Rank 2**: 4d10 + MIGHT Physical damage. After attacking, immediately move to any valid position (costs 0 Stamina, generates 5 Momentum). Cost 40 Stamina.

**Rank 3**: Free move generates 10 Momentum (doubled).

---

### 4.3 Vicious Flank (ID: 25006)

**Type**: Active | **Cost**: 40 Stamina + 25 Momentum | **Cooldown**: 3 turns

### Rank Details

**Rank 2**: 4d10 + MIGHT Physical damage. If target is debuffed, deal +50% damage. Cost 20 Momentum.

**Rank 3**: On kill, refund 10 Momentum.

---

## 5. Tier 3 Abilities (No Ranks)

### 5.1 No Quarter (ID: 25007)

**Type**: Passive | **Target**: Self

### Mechanical Effect

When you reduce an enemy to 0 HP:

- Immediately move to any valid position (costs 0 Stamina)
- Generate 10 Momentum (doubled from base)
- Gain +15 temporary HP

---

### 5.2 Savage Harvest (ID: 25008)

**Type**: Active | **Cost**: 50 Stamina + 40 Momentum | **Cooldown**: 4 turns

### Mechanical Effect

- Deal 10d10 + MIGHT Physical damage
- If this kills the target:
    - Refund 20 Stamina and 20 Momentum
    - Heal for 20% of max HP

---

## 6. Capstone Ability

### 6.1 Riptide of Carnage (ID: 25009)

**Type**: Active | **Cost**: 60 Stamina + 75 Momentum | **Once Per Combat**

### Mechanical Effect

- Make 4 attacks against different enemies in a single turn
- Each attack deals 4d10 + MIGHT Physical damage
- After all attacks, gain 10 Psychic Stress (causality violation)
- Each kill refunds 10 Momentum

**Note**: Requires 4 different valid targets. If fewer targets available, makes fewer attacks.

### GUI Display - CAPSTONE

```
┌─────────────────────────────────────────────┐
│         RIPTIDE OF CARNAGE!                 │
├─────────────────────────────────────────────┤
│                                             │
│  You become a blur of violence!             │
│                                             │
│  • 4 attacks against different enemies      │
│  • 4d10+MIGHT each                          │
│  • +10 Psychic Stress (reality violation)   │
│                                             │
│  "Time stutters. Bodies fall."              │
│                                             │
└─────────────────────────────────────────────┘

```

---

## 7. Status Effect Definitions

### 7.1 [Disoriented]

| Property | Value |
| --- | --- |
| **Applied By** | Dread Charge |
| **Duration** | 1-2 turns |
| **Effect** | -2 dice to Accuracy, cannot use complex abilities |

---

## 8. GUI Requirements

### 8.1 Momentum Bar

```
┌─────────────────────────────────────────┐
│  MOMENTUM [████████████░░░░░░] 65/100   │
│  ⚡ Movement and strikes build power    │
└─────────────────────────────────────────┘

```

- Color: Electric blue gradient
- Pulses when near threshold for abilities
- Shows starting value at combat begin

---

## 9. Implementation Priority

### Phase 1: Momentum System

1. **Implement Momentum resource** - 0-100 scale
2. **Implement starting Momentum** from Harrier's Alacrity
3. **Implement Momentum generation** from attacks

### Phase 2: Combat Integration

1. **Implement move + attack** combinations
2. **Implement debuff detection** for bonus damage
3. **Implement kill tracking** for refunds

### Phase 3: Capstone

1. **Implement multi-attack** (Riptide of Carnage)
2. **Add Psychic Stress costs**
3. **Add Momentum bar UI**

---

**End of Specification**