# Avalanche

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-AVALANCHE-v5.0
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
| **Tier** | 3 (Mastery) |
| **PP Cost** | 5 / 5 / 5 |
| **Resource Cost** | 55 Stamina + 40 Resonance |
| **Cooldown** | None |

---

## Thematic Description

> *"When the mountain moves, nothing stands before it."*
> 

Avalanche is the Gorge-Maw's **offensive masterwork**. You channel the fury of a collapsing mountainside, blasting enemies away in a devastating cone of seismic force while leaving the ground beneath them treacherous and unstable.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 55 Stamina + 40 Resonance
- **Target**: Cone (5 tiles long, 3 tiles wide at end)
- **Attack**: STURDINESS vs area Defense
- **Damage**: 5d10 Physical to all enemies in cone
- **Displacement**: All enemies [Pushed] **2 tiles** away from you
- **Terrain**: Creates [Unstable Ground] throughout affected area

### Rank 2

- **Damage**: 6d10 Physical (up from 5d10)
- **Displacement**: [Push] **3 tiles** (up from 2)
- **Collision Damage**: Enemies who collide with obstacles take **+2d6 damage**
- **PP Cost**: 5

### Rank 3

- **Damage**: 7d10 Physical
- **Status Effect**: All enemies are also [Staggered] for **2 rounds**
- **PP Cost**: 5

---

## Terrain Created: [Unstable Ground]

**Properties**:

- **Movement Trigger**: Any movement triggers FINESSE check (DC 12) or fall [Prone]
- **Passive Damage**: Enemies starting turn here take 1d4 Physical damage
- **Resonance**: +5 Resonance/turn while standing on

**Duration**: Persists until end of combat or cleared

---

## Synergies

### Internal (Gorge-Maw Tree)

- **Fissure**: Push enemies into fissure line
- **Earthshaker**: Unstable Ground + Rubble = nightmare terrain
- **Tectonic Fury**: Primes battlefield for capstone

### External (Party Composition)

- **Ranged specialists**: Pushed enemies at range
- **Mobile strikers**: Chase pushed enemies
- **Controllers**: Prone enemies easy to chain-CC

---

## Tactical Applications

### Battlefield Reset

1. Enemies close on your position
2. Avalanche pushes all back
3. Unstable Ground slows pursuit
4. Buy time for party repositioning
5. Repeat as needed

### Collision Combo (Rank 2)

- Push enemies toward walls, pillars, Rubble Piles
- Each collision adds +2d6 damage
- Position near environmental obstacles
- Chain multiple collisions if geometry permits

### Rank 3: Stagger Wave

- 7d10 damage + Push + [Staggered]
- Enemies moved, slowed, debuffed simultaneously
- Follow-up actions against staggered targets have advantage

---

## v5.0 Compliance Notes

✅ **High Resonance Cost**: 40 Resonance requires buildup

✅ **Offensive Controller**: Damage + displacement + terrain

✅ **Cone AoE**: Clear directional targeting

✅ **Environmental Interaction**: Collision damage rewards positioning