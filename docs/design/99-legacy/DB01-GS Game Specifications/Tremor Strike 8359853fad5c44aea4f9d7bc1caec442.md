# Tremor Strike

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-TREMORSTRIKE-v5.0
Parent item: Gorge-Maw Ascetic (Seismic Monk) — Specialization Specification v5.0 (Gorge-Maw%20Ascetic%20(Seismic%20Monk)%20%E2%80%94%20Specialization%20%200746d736f9a04324976a9f605c4ac276.md)
Proof-of-Concept Flag: No
Resource System: Stamina
Sub-Type: Active
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Trauma Economy Risk: None
Voice Validated: No

## Overview

| Attribute | Value |
| --- | --- |
| **Ability Type** | Active — Standard Action |
| **Tier** | 1 (Foundational) |
| **PP Cost** | 3 / 3 / 3 |
| **Resource Cost** | 35 Stamina |
| **Cooldown** | None |

---

## Thematic Description

> *"Channel the earth's restless energy through your weapon; the ground shudders in sympathy."*
> 

Tremor Strike is the Gorge-Maw's **core melee attack**. Your weapon channels seismic energy, transferring the earth's restless power through the impact. The ground itself shudders, staggering your target and building your connection to the deep places.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 35 Stamina
- **Target**: Single enemy (melee range)
- **Attack**: STURDINESS vs target's Defense
- **Damage**: 2d10 Physical
- **Save Effect**: Target must make **STURDINESS check DC 12** or become [Staggered] for 1 round
- **Resonance**: Generates **15 Resonance**

### Rank 2

- **Damage**: 3d10 Physical (up from 2d10)
- **Save DC**: 14 (up from 12)
- **AoE**: Affects all enemies **adjacent to target** as well
- **PP Cost**: 3

### Rank 3

- **Damage**: 4d10 Physical
- **Save DC**: 16
- **Duration**: [Staggered] lasts **2 rounds** (up from 1)
- **PP Cost**: 3

---

## Status Effect: [Staggered]

**Duration**: 1/2 rounds (by rank)

**Effects**:

- -1d10 penalty to attack rolls
- Cannot take Reactions
- Movement speed halved

---

## Resonance Generation

Tremor Strike is your primary Resonance builder:

| Action | Resonance Generated |
| --- | --- |
| Tremor Strike (hit) | +15 |
| + Rooted Stance (R3) | +10/turn |
| + Taking damage (grounded) | +10 |
| = Potential per round | 25-35 |

---

## Synergies

### Internal (Gorge-Maw Tree)

- **Rooted Stance**: Stay stationary for Soak while using Tremor Strike
- **Earthshaker**: Build Resonance for AoE terrain control
- **Fissure**: Stagger into fissure for combo control

### External (Party Composition)

- **Follow-up strikers**: [Staggered] targets easier to hit
- **Caster disruptors**: Stagger breaks concentration
- **Controllers**: Stack debuffs on staggered enemies

---

## Tactical Applications

### Resonance Building Loop

1. Tremor Strike (+15 Resonance)
2. Take hits while grounded (+10 per hit)
3. Rooted Stance R3 (+10/turn)
4. At 30 Resonance: Earthshaker available
5. At 80 Resonance: Tectonic Fury available

### Rank 2: Cleave Effect

Additional targets adjacent to primary:

- Effective against clustered enemies
- Can stagger multiple threats simultaneously
- Synergizes with terrain control (group in fissure zones)

---

## v5.0 Compliance Notes

✅ **STURDINESS Attack**: Uses primary attribute

✅ **Resonance Integration**: Builds unique resource

✅ **Control Element**: [Staggered] provides battlefield control

✅ **Scalable**: Clear rank progression