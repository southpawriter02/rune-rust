# Veiðimaðr (Hunter) Specialization - Complete Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Specification ID: SPEC-SPECIALIZATION-VEIDIMADUR
Version: 1.0
Last Updated: 2025-11-27
Status: Draft - Implementation Review
> 

---

## Document Control

### Purpose

This document provides the complete specification for the Veiðimaðr (Hunter) specialization, including:

- Design philosophy and mechanical identity
- All 9 abilities with **exact formulas per rank**
- **Rank unlock requirements** (tree-progression based, NOT PP-based)
- Current implementation status

### Related Files

| Component | File Path | Status |
| --- | --- | --- |
| Service Implementation | `RuneAndRust.Engine/VeidimadurService.cs` | Implemented |
| Data Seeding | `RuneAndRust.Persistence/VeidimadurSeeder.cs` | Implemented |

### Change Log

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | 2025-11-27 | Initial specification from VeidimadurSeeder |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
| --- | --- |
| **Internal Name** | Veiðimaðr |
| **Display Name** | Hunter |
| **Specialization ID** | 24001 |
| **Archetype** | Skirmisher (ArchetypeID = 4) |
| **Path Type** | Coherent |
| **Mechanical Role** | Ranged DPS / Corruption Tracker |
| **Primary Attribute** | FINESSE |
| **Secondary Attribute** | WITS |
| **Resource System** | Stamina + Focus |
| **Trauma Risk** | Medium |
| **Icon** | :bow_and_arrow: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
| --- | --- | --- |
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 5 | Mid-game specialization |

### 1.3 Design Philosophy

**Tagline**: "The Hunter Who Tracks the Blight"

**Core Fantasy**: You are the patient predator of a corrupted world. You've learned to read the invisible signs of the Runic Blight—the subtle tells that reveal a creature's corruption level. You mark high-priority targets, exploit their weaknesses, and deliver devastating shots from the safety of the back row.

**Mechanical Identity**:

1. **Corruption Tracking**: Specializes in detecting and exploiting enemy corruption levels
2. **Mark System**: Mark for Death provides significant damage bonuses
3. **Back Row Safety**: Gains defensive bonuses when positioned in back row
4. **Corruption Purging**: Can purge corruption from heavily Blighted foes for bonus damage

---

## 2. Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Key Effect |
| --- | --- | --- | --- | --- | --- |
| 24001 | Wilderness Acclimation I | 1 | Passive | 1→2→3 | +dice to tracking/perception, identify Blighted |
| 24002 | Aimed Shot | 1 | Active | 1→2→3 | FINESSE ranged attack |
| 24003 | Set Snare | 1 | Active | 1→2→3 | Place [Root] trap |
| 24004 | Mark for Death | 2 | Active | 2→3 | Apply [Marked] for bonus damage |
| 24005 | Blight-Tipped Arrow | 2 | Active | 2→3 | DoT + [Glitch] vs corrupted |
| 24006 | Predator's Focus | 2 | Passive | 2→3 | +Resolve while in back row |
| 24007 | Exploit Corruption | 3 | Passive | — | Increased crit vs corrupted |
| 24008 | Heartseeker Shot | 3 | Active | — | Charged shot + corruption purge |
| 24009 | Stalker of the Unseen | 4 | Hybrid | — | Auto-learn vulnerabilities + stance |

---

## 3. Tier 1 Abilities

### 3.1 Wilderness Acclimation I (ID: 24001)

**Type**: Passive | **Target**: Self

### Rank Details

**Rank 1**: +1d10 bonus to WITS-based checks for tracking, foraging, perceiving hidden/ambushing creatures. Can identify Blighted creatures by spoor.

**Rank 2**: +2d10 bonus. Can estimate corruption level (Low/Medium/High/Extreme).

**Rank 3**: +3d10 bonus. Automatically detect [Blighted] items without touching them.

---

### 3.2 Aimed Shot (ID: 24002)

**Type**: Active | **Cost**: 40 Stamina | **Target**: Single Enemy (Ranged)

### Rank Details

**Rank 1**: FINESSE-based ranged attack dealing weapon damage.

**Rank 2**: Cost reduced to 35 Stamina. Damage increased by +1d6.

**Rank 3**: Damage +2d6 total. On critical hit, apply [Bleeding] for 2 turns (1d6/turn).

---

### 3.3 Set Snare (ID: 24003)

**Type**: Active | **Cost**: 35 Stamina | **Cooldown**: 2 turns

### Rank Details

**Rank 1**: Place concealed trap. First enemy to step on it becomes [Rooted] for 1 turn. Requires 1 Trap Component.

**Rank 2**: [Rooted] 2 turns. Can place up to 2 active traps.

**Rank 3**: [Rooted] 3 turns. Trapped enemy also takes 2d6 Physical damage.

---

## 4. Tier 2 Abilities

### 4.1 Mark for Death (ID: 24004)

**Type**: Active | **Cost**: 30 Stamina + 5 Psychic Stress | **Cooldown**: 3 turns

### Description

You focus your intent on a single target, observing the subtle tells of its Blighted nature.

### Rank Details

**Rank 2**: Apply [Marked] for 3 turns. Your attacks vs [Marked] deal +8 bonus damage. Costs 5 Psychic Stress.

**Rank 3**: +15 bonus damage. Allies gain +5 damage vs [Marked]. 4 turn duration. Stress cost reduced to 2.

---

### 4.2 Blight-Tipped Arrow (ID: 24005)

**Type**: Active | **Cost**: 45 Stamina | **Cooldown**: 3 turns

### Rank Details

**Rank 2**: 4d6 Physical damage + [Blighted Toxin] (2d6/turn, 4 turns). 60% [Glitch] chance vs 30+ Corruption.

**Rank 3**: Toxin 3d6/turn. 80% [Glitch] chance. Glitch duration: 1 turn.

**[Glitch] Effect**: Target skips next action.

---

### 4.3 Predator's Focus (ID: 24006)

**Type**: Passive | **Target**: Self (while in back row)

### Rank Details

**Rank 2**: While in back row and not adjacent to enemies, +2d10 to Resolve vs Psychic Stress. +1d10 to Perception.

**Rank 3**: +3d10 to both. Regenerate 5 Stamina per turn out of combat.

---

## 5. Tier 3 Abilities (No Ranks)

### 5.1 Exploit Corruption (ID: 24007)

**Type**: Passive | **Target**: Self

### Mechanical Effect

Your attacks gain increased critical hit chance vs corrupted targets:

- Low Corruption (1-29): +10% crit
- Medium Corruption (30-59): +20% crit
- High Corruption (60-89): +30% crit
- Extreme Corruption (90+): +40% crit

Critical hits vs corrupted targets deal +50% damage. If critical kills target, refund 20 Stamina.

---

### 5.2 Heartseeker Shot (ID: 24008)

**Type**: Active | **Cost**: 60 Stamina + 30 Focus | **Cooldown**: 4 turns

### Mechanical Effect

- Requires full turn to charge (cannot move or use other abilities)
- Next turn: Release shot dealing 10d10 Physical damage
- If target is [Marked]: Purge 20 Corruption, dealing +2 bonus damage per Corruption purged (max +40)
- If kills [Marked] target: Refund 30 Stamina and 15 Focus

---

## 6. Capstone Ability

### 6.1 Stalker of the Unseen (ID: 24009)

**Type**: Hybrid (Passive + Active) | **Cost**: 20 Stamina/turn (stance)

### Passive Component

When you use Mark for Death, automatically learn target's Vulnerabilities and precise Corruption level.

### Active Component: Blight-Stalker's Stance (Toggle)

While active:

- Immune to visual impairment effects
- Aimed Shots vs High/Extreme Corruption targets have 90% chance to inflict [Staggered]
- +2d10 to attack rolls vs corrupted targets
- Pay 15 Stamina per turn to maintain
- When stance ends, gain 5 Psychic Stress

---

## 7. Implementation Priority

### Phase 1: Core Mechanics

1. **Implement Mark system** - [Marked] status with damage bonus
2. **Implement corruption tracking** - corruption level detection
3. **Implement trap placement** - Set Snare with [Root]

### Phase 2: Combat Integration

1. **Implement charged attacks** - Heartseeker Shot charge mechanic
2. **Implement [Blighted Toxin]** DoT effect
3. **Implement [Glitch]** skip action effect

### Phase 3: Capstone

1. **Implement Blight-Stalker's Stance** toggle
2. **Implement vulnerability detection**
3. **Implement corruption purge** damage bonus

---

**End of Specification**