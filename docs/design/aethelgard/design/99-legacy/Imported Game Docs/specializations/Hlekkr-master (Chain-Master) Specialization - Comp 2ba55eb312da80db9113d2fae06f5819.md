# Hlekkr-master (Chain-Master) Specialization - Complete Specification

Parent item: Specs: Specializations (Specs%20Specializations%202ba55eb312da8022a82bc3d0883e1d26.md)

> Specification ID: SPEC-SPECIALIZATION-HLEKKR-MASTER
Version: 1.0
Last Updated: 2025-11-27
Status: Draft - Implementation Review
> 

---

## Document Control

### Purpose

This document provides the complete specification for the Hlekkr-master (Chain-Master) specialization.

### Related Files

| Component | File Path | Status |
| --- | --- | --- |
| Service Implementation | `RuneAndRust.Engine/HlekkmasterService.cs` | Implemented |
| Data Seeding | `RuneAndRust.Persistence/HlekkmasterSeeder.cs` | Implemented |

### Change Log

| Version | Date | Changes |
| --- | --- | --- |
| 1.0 | 2025-11-27 | Initial specification from HlekkmasterSeeder |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
| --- | --- |
| **Internal Name** | Hlekkr-master |
| **Display Name** | Chain-Master |
| **Specialization ID** | 25002 |
| **Archetype** | Skirmisher (ArchetypeID = 4) |
| **Path Type** | Coherent |
| **Mechanical Role** | Battlefield Controller / Formation Breaker |
| **Primary Attribute** | FINESSE |
| **Secondary Attribute** | MIGHT |
| **Resource System** | Stamina |
| **Trauma Risk** | Low |
| **Icon** | :chains: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
| --- | --- | --- |
| **PP Cost to Unlock** | 3 PP | Standard cost |
| **Minimum Legend** | 5 | Mid-game specialization |

### 1.3 Design Philosophy

**Tagline**: "The Chain-Master"

**Core Fantasy**: The Hlekkr-master is the battlefield puppeteer who exploits glitching physics to drag enemies into kill zones and lock them down. You use chains, hooks, and nets to control positioning and punish helplessness. Your chains don't just lock down—they make enemies die faster.

**Mechanical Identity**:

1. **Battlefield Control**: Specializes in Pull, Push, Root, and Slow effects
2. **Corruption Exploitation**: Control effects more effective vs corrupted enemies
3. **Formation Breaking**: Drags enemies out of position
4. **Punish the Helpless**: Massive bonus damage vs controlled enemies

---

## 2. Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Key Effect |
| --- | --- | --- | --- | --- | --- |
| 25010 | Pragmatic Preparation I | 1 | Passive | 1→2→3 | +dice to traps, control duration +1 |
| 25011 | Netting Shot | 1 | Active | 1→2→3 | Low damage + [Rooted] |
| 25012 | Grappling Hook Toss | 1 | Active | 1→2→3 | Pull from Back to Front + [Disoriented] |
| 25013 | Snag the Glitch | 2 | Passive | 2→3 | Control effects scale with corruption |
| 25014 | Unyielding Grip | 2 | Active | 2→3 | [Seized] vs machines |
| 25015 | Punish the Helpless | 2 | Passive | 2→3 | +100% damage vs controlled enemies |
| 25016 | Chain Scythe | 3 | Active | — | AoE row + [Slowed] + [Knocked Down] |
| 25017 | Corruption Siphon Chain | 3 | Active | — | [Stunned] scales with corruption |
| 25018 | Master of Puppets | 4 | Hybrid | — | Pulled = [Vulnerable] + corruption bomb |

---

## 3. Tier 1 Abilities

### 3.1 Pragmatic Preparation I (ID: 25010)

**Type**: Passive | **Target**: Self

### Rank Details

**Rank 1**: +1d10 to FINESSE checks for setting/disarming traps. All [Rooted] and [Slowed] effects last +1 turn longer.

**Rank 2**: +2d10 bonus. Control duration +1 turn.

**Rank 3**: +3d10 bonus. Control effects last +2 turns total.

---

### 3.2 Netting Shot (ID: 25011)

**Type**: Active | **Cost**: 20 Stamina | **Cooldown**: 2 turns

### Rank Details

**Rank 1**: 1d6 Physical damage. Apply [Rooted] for 2 turns (3 with Pragmatic Preparation).

**Rank 2**: [Rooted] 3 turns (4 with PP). Can target 2 enemies (split net).

**Rank 3**: Cost 15 Stamina. Against 60+ Corruption enemies, also apply [Slowed] 2 turns.

---

### 3.3 Grappling Hook Toss (ID: 25012)

**Type**: Active | **Cost**: 30 Stamina | **Cooldown**: 3 turns

### Rank Details

**Rank 1**: 2d8 Physical damage. Pull target from Back Row to Front Row. Apply [Disoriented] (1 turn).

**Rank 2**: 3d8 damage. Pull from further positions.

**Rank 3**: On successful pull vs corrupted enemy, generate 10 Focus.

---

## 4. Tier 2 Abilities

### 4.1 Snag the Glitch (ID: 25013)

**Type**: Passive | **Target**: Self

### Rank Details

**Rank 2**: Control effects have increased success vs corrupted enemies:

- Low (1-29): +20% success
- Medium (30-59): +40%
- High (60-89): +80%
- Extreme (90+): +100%
Also gain +1d10 damage vs corrupted.

**Rank 3**: +3d10 damage vs corrupted. Against Extreme Corruption, control effects cannot miss.

---

### 4.2 Unyielding Grip (ID: 25014)

**Type**: Active | **Cost**: 25 Stamina | **Cooldown**: 4 turns

### Description

Chain wraps around servos and malfunctioning joints, locking them in place.

### Rank Details

**Rank 2**: 2d8 Physical damage. 80% chance to apply [Seized] (2 turns) vs Undying/Mechanical. [Seized] prevents ALL actions.

**Rank 3**: [Seized] 3 turns. Also works on non-mechanical at 40% rate. Seized enemies take 1d6/turn crushing damage.

---

### 4.3 Punish the Helpless (ID: 25015)

**Type**: Passive | **Target**: Self

### Rank Details

**Rank 2**: +75% damage vs enemies with [Rooted], [Slowed], [Stunned], [Seized], or [Disoriented]. Advantage on attacks vs controlled.

**Rank 3**: +100% (double damage). Controlled enemies take 1d6 damage/turn from chains.

---

## 5. Tier 3 Abilities (No Ranks)

### 5.1 Chain Scythe (ID: 25016)

**Type**: Active | **Cost**: 35 Stamina | **Cooldown**: 4 turns

### Mechanical Effect

- Deal 3d8 Physical damage to all enemies in a row (can target Front or Back)
- Apply [Slowed] (3 turns) to all hit
- Against 60+ Corruption: 80% chance to apply [Knocked Down] instead
- Apply [Disoriented] (1 turn) to all hit

---

### 5.2 Corruption Siphon Chain (ID: 25017)

**Type**: Active | **Cost**: 30 Stamina + 5 Psychic Stress | **Cooldown**: 3 turns

### Mechanical Effect

No damage. [Stunned] chance based on target corruption:

- Low (1-29): 20% chance
- Medium (30-59): 40%
- High (60-89): 70%
- Extreme (90+): 90%

[Stunned] duration: 2 turns. If successful vs Extreme Corruption, also purge 10 Corruption from target. Stress reduced to 3.

---

## 6. Capstone Ability

### 6.1 Master of Puppets (ID: 25018)

**Type**: Hybrid | **Active Cost**: 50 Stamina | **Once Per Combat**

### Passive Component

Whenever you Pull or Push an enemy, they become [Vulnerable] (2 turns). +2d10 bonus to Pull/Push attempts.

### Active Component: Corruption Bomb

- Target single enemy with High (60+) or Extreme Corruption
- Make opposed FINESSE check
- If successful: Trigger catastrophic feedback loop
- Explosion deals 10d10 Psychic damage to all OTHER enemies

### GUI Display - CAPSTONE

```
┌─────────────────────────────────────────────┐
│          MASTER OF PUPPETS!                 │
├─────────────────────────────────────────────┤
│                                             │
│  You see only pieces to be moved at will!   │
│                                             │
│  PASSIVE: Pull/Push = [Vulnerable]          │
│                                             │
│  ACTIVE: Corruption Bomb                    │
│  • Target high-corruption enemy             │
│  • 10d10 Psychic damage to all others       │
│                                             │
│  "Dance, puppets. Dance."                   │
│                                             │
└─────────────────────────────────────────────┘

```

---

## 7. Status Effect Definitions

### 7.1 [Seized]

| Property | Value |
| --- | --- |
| **Applied By** | Unyielding Grip |
| **Duration** | 2-3 turns |
| **Effect** | Cannot take ANY actions (complete lockdown) |
| **Notes** | Special effectiveness vs Undying/Mechanical |

### 7.2 [Slowed]

| Property | Value |
| --- | --- |
| **Applied By** | Netting Shot, Chain Scythe |
| **Duration** | 2-3 turns |
| **Effect** | Movement costs doubled, -1 die to Agility checks |

---

## 8. Implementation Priority

### Phase 1: Control Effects

1. **Implement [Rooted]** - prevent movement
2. **Implement [Slowed]** - movement cost increase
3. **Implement [Seized]** - complete action lockdown

### Phase 2: Combat Integration

1. **Implement Pull/Push mechanics** - position displacement
2. **Implement corruption scaling** - success chance modifier
3. **Implement damage bonuses** vs controlled enemies

### Phase 3: Capstone

1. **Implement [Vulnerable] on displacement**
2. **Implement Corruption Bomb** - AoE Psychic burst
3. **Add chain DoT** vs controlled enemies

---

**End of Specification**