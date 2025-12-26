---
id: ABILITY-GORGE-MAW-26015
title: "Resonant Tremor"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Resonant Tremor

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Target Area |
| **Resource Cost** | 35 Stamina |
| **Cooldown** | 3 turns |
| **Damage Type** | Physical (DoT) |
| **Tags** | [Zone], [Terrain], [Control] |
| **Ranks** | 2 → 3 |

---

## Description

You set the earth into continuous vibration, creating a zone of treacherous, cracked terrain. Enemies within struggle to maintain their footing as the ground shifts beneath them.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Create [Difficult Terrain] zone in 4×4 tile area
- Zone lasts 3 turns
- Enemies in zone take 1d6 Physical damage per turn

**Formula:**
```
CreateZone(
    Type: "DifficultTerrain",
    Size: 4x4,
    Duration: 3,
    CenterPoint: TargetTile
)

OnEnemyTurnInZone:
    Damage = Roll(1d6)
    Enemy.TakeDamage(Damage, "Physical")
```

**Tooltip:** "Resonant Tremor (Rank 2): Create 4×4 Difficult Terrain (3 turns). 1d6 damage/turn. Cost: 35 Stamina"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Create [Difficult Terrain] zone in 5×5 tile area
- Zone lasts 4 turns
- Enemies in zone take 2d6 Physical damage per turn
- **NEW:** Enemies in zone suffer -1 Accuracy

**Formula:**
```
CreateZone(
    Type: "DifficultTerrain",
    Size: 5x5,
    Duration: 4,
    CenterPoint: TargetTile,
    AccuracyPenalty: -1
)

OnEnemyTurnInZone:
    Damage = Roll(2d6)
    Enemy.TakeDamage(Damage, "Physical")
    Enemy.Accuracy -= 1
```

**Tooltip:** "Resonant Tremor (Rank 3): 5×5 Difficult Terrain (4 turns). 2d6/turn, -1 Accuracy. Cost: 35 Stamina"

---

## Zone Comparison

| Property | Rank 2 | Rank 3 |
|----------|--------|--------|
| Size | 4×4 (16 tiles) | 5×5 (25 tiles) |
| Duration | 3 turns | 4 turns |
| Damage/turn | 1d6 (avg 3.5) | 2d6 (avg 7) |
| Accuracy Penalty | None | -1 |

---

## [Difficult Terrain] Effects

| Effect | Description |
|--------|-------------|
| Movement Cost | Double movement cost to traverse |
| Charge Prevention | Cannot charge through |
| DoT | Damage each turn while in zone |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Chokepoint | Block enemy advance |
| Melee enemies | Force them to take damage approaching |
| Ranged setup | Create safe zone for allies |
| Zone stacking | Combine with Earthen Grasp |

---

## Combat Log Examples

- "Resonant Tremor: 4×4 Difficult Terrain created for 3 turns"
- "[Enemy] takes 4 damage (Resonant Tremor DoT)"
- "[Enemy] suffers -1 Accuracy while in Resonant Tremor zone"
- "Resonant Tremor zone expires"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Gorge-Maw Ascetic Overview](../gorge-maw-ascetic-overview.md) | Parent specialization |
| [Earthshaker](earthshaker.md) | Permanent terrain creation |
| [Room Engine Core](../../../../07-environment/room-engine/core.md) | Zone mechanics |
