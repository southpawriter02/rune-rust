# Fissure

Type: Ability
Priority: Should-Have
Status: In Design
Archetype Foundation: Warrior
Balance Validated: No
Document ID: AAM-SPEC-ABIL-FISSURE-v5.0
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
| **Resource Cost** | 45 Stamina + 20 Resonance |
| **Cooldown** | None |

---

## Thematic Description

> *"Open the earth's wounds; let them swallow the unwary."*
> 

Fissure is the Gorge-Maw's **zoning tool**. You crack open the earth in a line, creating a dangerous gap that damages enemies caught in its path and blocks movement. Those who try to cross risk falling in and becoming trapped.

---

## Mechanical Implementation

### Base Effect (Rank 1)

- **Cost**: 45 Stamina + 20 Resonance
- **Target**: Line of tiles (up to **5 tiles** long, 1 tile wide)
- **Initial Damage**: 2d6 Physical to enemies in line
- **Terrain**: Creates [Fissure] along the line
- **Crossing Effect**: Enemies attempting to cross take 1d8 Physical and must make **FINESSE check DC 12** or fall in ([Restrained] for 1 round)

### Rank 2

- **Initial Damage**: 3d6 Physical (up from 2d6)
- **Line Length**: 7 tiles (up from 5)
- **Save DC**: 14 (up from 12)
- **PP Cost**: 4

### Rank 3

- **Initial Damage**: 4d6 Physical
- **Duration**: [Fissure] persists **until end of combat** (permanent)
- **Fall Damage**: 2d8 Physical (up from 1d8)
- **PP Cost**: 4

---

## Terrain Created: [Fissure]

**Properties**:

- **Blocks Movement**: Cannot walk through normally
- **Crossing Attempt**: Triggers FINESSE save or [Restrained]
- **Jumping**: Can attempt to jump (requires FINESSE check DC 14)
- **Duration**: Until cleared (R3: permanent)

**[Restrained] in Fissure**:

- Cannot move
- -2 to Defense
- Attacks against have advantage
- Climbing out requires Standard Action + STURDINESS check DC 12

---

## Synergies

### Internal (Gorge-Maw Tree)

- **Tremor Strike**: Stagger enemy toward fissure
- **Earthshaker**: Knockdown + Fissure = trapped enemies
- **Avalanche**: Push enemies into fissure line

### External (Party Composition)

- **Hólmgangr**: Fissure isolates duel targets
- **Ranged specialists**: Fissure creates safe firing lanes
- **Controllers**: Chain CC with restrained enemies

---

## Tactical Applications

### Zone Denial

1. Identify enemy approach vectors
2. Fissure across chokepoint
3. Enemies must: go around, jump (risky), or get trapped
4. Buys time for party positioning

### Isolation Combo

1. Earthshaker knockdown on group
2. Fissure between priority target and allies
3. Target isolated on your side
4. Party focuses isolated enemy

### Hólmgangr Synergy

- Fissure around duel zone
- Neither duelist can be interfered with
- Perfect isolation for Challenge of Honour

---

## v5.0 Compliance Notes

✅ **Zone Control**: Core controller identity

✅ **Movement Denial**: Strategic positioning tool

✅ **Synergy Design**: Combo potential with knockdown effects

✅ **Persistent Terrain**: Lasting battlefield impact