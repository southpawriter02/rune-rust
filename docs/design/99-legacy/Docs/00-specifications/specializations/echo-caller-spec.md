# Echo-Caller Specialization - Complete Specification

> **Specification ID**: SPEC-SPECIALIZATION-ECHO-CALLER
> **Version**: 1.0
> **Last Updated**: 2025-11-27
> **Status**: Draft - Implementation Review

---

## Document Control

### Purpose
This document provides the complete specification for the Echo-Caller specialization.

### Related Files
| Component | File Path | Status |
|-----------|-----------|--------|
| Data Seeding | `RuneAndRust.Persistence/EchoCallerSeeder.cs` | Implemented |

### Change Log
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-11-27 | Initial specification from EchoCallerSeeder |

---

## 1. Specialization Overview

### 1.1 Identity

| Property | Value |
|----------|-------|
| **Internal Name** | EchoCaller |
| **Display Name** | Echo-Caller |
| **Specialization ID** | 28002 |
| **Archetype** | Mystic (ArchetypeID = 5) |
| **Path Type** | Coherent |
| **Mechanical Role** | Psychic Artillery / Crowd Control |
| **Primary Attribute** | WILL |
| **Secondary Attribute** | WITS |
| **Resource System** | Aether Pool |
| **Trauma Risk** | Medium |
| **Icon** | :eye: |

### 1.2 Unlock Requirements

| Requirement | Value | Notes |
|-------------|-------|-------|
| **PP Cost to Unlock** | 10 PP | Higher than standard 3 PP |
| **Minimum Legend** | 5 | Mid-game specialization |

### 1.3 Design Philosophy

**Tagline**: "Psychic Artillery"

**Core Fantasy**: You are the reality manipulator who weaponizes the Great Silence's eternal psychic scream. Where others commune with echoes, you command them. You are psychic artillery—projecting traumatic memories as weapons, creating cascading fear, and implanting phantom sensations in enemy minds.

**Mechanical Identity**:
1. **[Echo] Tag System**: Most abilities have [Echo] tag enabling chain effects
2. **Fear Manipulation**: Specializes in applying and exploiting [Feared] status
3. **Echo Chain**: Abilities spread to adjacent enemies for reduced effect
4. **Terror Feedback**: Gains resources from applying Fear

### 1.4 The Echo System

**Echo Mechanics**:
- Abilities tagged [Echo] can trigger **Echo Chain**
- **Echo Chain**: 50-80% damage/effect spreads to 1-2 adjacent enemies
- Echo Cascade passive enhances chain range and damage
- Some abilities gain bonus effects vs [Feared] targets

---

## 2. Ability Summary Table

| ID | Ability Name | Tier | Type | Ranks | Key Effect |
|----|--------------|------|------|-------|------------|
| 28010 | Echo Attunement | 1 | Passive | 1→2→3 | -Aether cost, +psychic resist |
| 28011 | Scream of Silence | 1 | Active | 1→2→3 | [Echo] Psychic damage, bonus vs Feared |
| 28012 | Phantom Menace | 1 | Active | 1→2→3 | [Echo] Apply [Feared] |
| 28013 | Echo Cascade | 2 | Passive | 2→3 | Echo Chain +range +damage |
| 28014 | Reality Fracture | 2 | Active | 2→3 | [Echo] Damage + [Disoriented] + Push |
| 28015 | Terror Feedback | 2 | Passive | 2→3 | Restore Aether when applying Fear |
| 28016 | Fear Cascade | 3 | Active | — | [Echo] AoE Fear spread |
| 28017 | Echo Displacement | 3 | Active | — | [Echo] Forced teleportation |
| 28018 | Silence Made Weapon | 4 | Active | — | Ultimate AoE scaling with Fear |

---

## 3. Tier 1 Abilities

### 3.1 Echo Attunement (ID: 28010)

**Type**: Passive | **Target**: Self

#### Rank Details

**Rank 1**: All Echo-tagged abilities cost -5 Aether. +1 die to WILL checks to resist psychic attacks.

**Rank 2**: -10 Aether cost, +2 dice resist.

**Rank 3**: -15 Aether, +2 dice, Echo Chain range increased by 1 tile.

---

### 3.2 Scream of Silence (ID: 28011)

**Type**: Active [Echo] | **Cost**: 35 Aether | **Target**: Single Enemy

#### Rank Details

**Rank 1**: [Echo] 3d8 Psychic damage. If target already [Feared], deal +1d8 damage.

**Rank 2**: 4d8 damage, +2d8 if Feared.

**Rank 3**: 5d8 damage, +2d8 if Feared, [Echo Chain] spreads 50% damage to adjacent enemy.

---

### 3.3 Phantom Menace (ID: 28012)

**Type**: Active [Echo] | **Cost**: 30 Aether | **Target**: Single Enemy

#### Rank Details

**Rank 1**: [Echo] Apply [Feared] to single target for 2 turns. [Feared]: Cannot attack, -2 dice to all checks, flee if possible.

**Rank 2**: [Feared] 3 turns.

**Rank 3**: [Feared] 3 turns, [Echo Chain] 50% chance spreads to adjacent enemy for 2 turns.

---

## 4. Tier 2 Abilities

### 4.1 Echo Cascade (ID: 28013)

**Type**: Passive | **Target**: Self

#### Rank Details

**Rank 2**: All [Echo Chain] effects have range 2 tiles. Chain damage increased from 50% to 70%.

**Rank 3**: Range 3 tiles, 80% damage, chains can hit 2 targets instead of 1.

---

### 4.2 Reality Fracture (ID: 28014)

**Type**: Active [Echo] | **Cost**: 40 Aether | **Target**: Single Enemy

#### Rank Details

**Rank 2**: [Echo] 3d8 Psychic damage + [Disoriented] 2 turns + Push 3 tiles in chosen direction.

**Rank 3**: 4d8 damage, Push 3 tiles, [Echo Chain] adjacent enemy also Pushed 2 tiles.

---

### 4.3 Terror Feedback (ID: 28015)

**Type**: Passive | **Target**: Self

#### Rank Details

**Rank 2**: Whenever you apply [Feared], restore 15 Aether.

**Rank 3**: Restore 20 Aether + gain [Empowered] for 1 turn (+2 dice to damage).

---

## 5. Tier 3 Abilities (No Ranks)

### 5.1 Fear Cascade (ID: 28016)

**Type**: Active [Echo] | **Cost**: 45 Aether | **Target**: All Enemies in Radius

#### Mechanical Effect
- [Echo] All enemies within 3 tiles of target: Make WILL Resolve check (DC 16) or become [Feared] for 3 turns
- Already-Feared enemies automatically fail and take 4d6 Psychic damage
- [Echo Chain] auto-spreads to one enemy outside initial radius

---

### 5.2 Echo Displacement (ID: 28017)

**Type**: Active [Echo] | **Cost**: 50 Aether + 5 Psychic Stress | **Target**: Single Enemy

#### Mechanical Effect
- [Echo] Teleport enemy to any unoccupied tile within 10 tiles
- Target takes 4d8 Psychic damage and is [Disoriented] for 1 turn
- Cost: +3 Psychic Stress (forceful reality manipulation)
- [Echo Chain]: Adjacent enemy also teleported to random tile within 3 tiles

---

## 6. Capstone Ability

### 6.1 Silence Made Weapon (ID: 28018)

**Type**: Active [Echo] | **Cost**: 60 Aether + 15 Psychic Stress | **Once Per Combat**

#### Mechanical Effect
- [Echo] ALL enemies on battlefield: 6d10 Psychic damage
- **Scaling**: Damage increases by +2d10 for each [Feared] or [Disoriented] enemy (max +12d10)
- All enemies make WILL Resolve (DC 18) or become [Feared] for 2 turns
- Can be used twice per combat at full power

**Maximum Potential**: 6d10 + 12d10 = **18d10 Psychic damage** if 6+ enemies are debuffed

#### GUI Display - CAPSTONE

```
┌─────────────────────────────────────────────┐
│        SILENCE MADE WEAPON!                 │
├─────────────────────────────────────────────┤
│                                             │
│  The Great Silence screams through you!     │
│                                             │
│  Base: 6d10 Psychic to ALL enemies          │
│  Bonus: +2d10 per Feared/Disoriented enemy  │
│  ALL must resist or become [Feared]         │
│                                             │
│  Cost: 60 Aether + 10 Psychic Stress        │
│                                             │
│  "The silence has found its voice."         │
│                                             │
└─────────────────────────────────────────────┘
```

---

## 7. Status Effect Definitions

### 7.1 [Feared]

| Property | Value |
|----------|-------|
| **Applied By** | Phantom Menace, Fear Cascade, Silence Made Weapon |
| **Duration** | 2-3 turns |
| **Icon** | Terror face |
| **Color** | Purple |

**Effects**:
- Cannot attack
- -2 dice to all checks
- Must flee if possible (move away from fear source)
- Echo-Caller abilities deal bonus damage vs Feared

### 7.2 [Disoriented]

| Property | Value |
|----------|-------|
| **Applied By** | Reality Fracture, Echo Displacement |
| **Duration** | 1-2 turns |
| **Effect** | -2 dice to Accuracy, cannot use complex abilities |

---

## 8. GUI Requirements

### 8.1 Aether Pool Display

```
┌─────────────────────────────────────────┐
│  AETHER [████████████░░░░░░] 75/120     │
│  🔮 Echo abilities cost reduced         │
└─────────────────────────────────────────┘
```

- Color: Ethereal purple
- Shows Aether cost reduction from Echo Attunement

### 8.2 Echo Chain Indicator

When Echo Chain triggers:
```
ECHO CHAIN! → [Adjacent Enemy] takes 80% effect
```

- Visual: Rippling wave effect between targets
- Shows percentage of effect transfer

---

## 9. Implementation Priority

### Phase 1: Echo System
1. **Implement [Echo] tag** - identifies chainable abilities
2. **Implement Echo Chain** - spread to adjacent enemies
3. **Implement Echo Cascade** - range/damage enhancement

### Phase 2: Fear Mechanics
4. **Implement [Feared] status** - flee behavior
5. **Implement bonus damage** vs Feared targets
6. **Implement Terror Feedback** - resource restoration

### Phase 3: Capstone
7. **Implement Silence Made Weapon** - scaling damage
8. **Implement forced teleportation** (Echo Displacement)
9. **Add Psychic Stress costs**

---

**End of Specification**
