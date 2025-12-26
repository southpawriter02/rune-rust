# Earthshaker

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-EARTHSHAKER-v5.0
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
| **Tier** | 2 (Advanced) |
| **PP Cost** | 4 / 4 / 4 |
| **Resource Cost** | 50 Stamina + 30 Resonance |
| **Cooldown** | None |

---

## Thematic Description

> *"Strike the fault line; let the earth speak your fury in a language of broken stone."*
> 

Earthshaker is the Gorge-Maw's **signature terrain-shaping attack**. You slam the ground with devastating force, sending shockwaves through the earth that damage enemies, knock them prone, and create rubble that reshapes the battlefield itself.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 50 Stamina + 30 Resonance
- **Target**: 3×3 tile area within 4 tiles
- **Attack**: STURDINESS vs area Defense
- **Damage**: 3d8 Physical to all enemies in area
- **Terrain**: Creates [Rubble Pile] in **center tile**
- **Save Effect**: All enemies must make **FINESSE check DC 13** or be [Knocked Down]

### Rank 2

- **Damage**: 4d8 Physical (up from 3d8)
- **Area**: 4×4 tiles (up from 3×3)
- **Terrain**: Creates **2 [Rubble Piles]**
- **PP Cost**: 4

### Rank 3

- **Damage**: 5d8 Physical
- **Save DC**: 15 (up from 13)
- **God-Sleeper Synergy**: [Rubble Piles] count as **[Scrap Piles]** for God-Sleeper abilities
- **PP Cost**: 4

---

## Terrain Created: [Rubble Pile]

**Properties**:

- **Difficult Terrain**: Costs 2 movement to enter
- **Half Cover**: +2 to Defense while behind
- **Resonance Generation**: +5 Resonance/turn while standing on
- **God-Sleeper Synergy (R3)**: Counts as [Scrap Pile] for Animate Scrap, Scrap Shield

**Duration**: Persists until cleared or end of combat

---

## Synergies

### Internal (Gorge-Maw Tree)

- **Rooted Stance**: Position in rubble for cover + Resonance
- **Fissure**: Rubble + Fissure creates impassable zones
- **Avalanche**: Rubble slows enemies fleeing cone

### External (Party Composition)

- **God-Sleeper Cultist**: R3 creates [Scrap Piles] for Animate Scrap
- **Ranged specialists**: Rubble provides cover for backline
- **Mobile strikers**: Enemies slowed, strikers can kite

---

## Tactical Applications

### Battlefield Reshaping

1. Identify chokepoint or enemy cluster
2. Earthshaker creates rubble + knockdown
3. Enemies prone in difficult terrain
4. Follow with Fissure to trap
5. Party exploits immobilized enemies

### God-Sleeper Combo (Rank 3)

- Earthshaker creates [Scrap Piles]
- God-Sleeper uses Animate Scrap on rubble
- Animated constructs emerge from your destruction
- **Thematic**: The broken earth rises to fight

---

## v5.0 Compliance Notes

✅ **Terrain Control**: Core controller identity

✅ **Resonance Spend**: Uses unique resource

✅ **Cross-Specialization Synergy**: God-Sleeper interaction

✅ **AoE Scaling**: Clear area progression