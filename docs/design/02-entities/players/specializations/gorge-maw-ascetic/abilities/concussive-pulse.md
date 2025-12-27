---
id: ABILITY-GORGE-MAW-26012
title: "Concussive Pulse"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Concussive Pulse

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | All Front Row enemies |
| **Resource Cost** | 35 Stamina |
| **Damage Type** | Physical |
| **Attribute** | MIGHT |
| **Control Effect** | Push to Back Row |
| **Tags** | [AoE], [Control], [Push] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You strike the ground with tremendous force, creating a shockwave that pushes all enemies in the front row backward. The earth itself becomes your weapon.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Push all Front Row enemies to Back Row
- Deal 1d6 + MIGHT Physical damage to each

**Formula:**
```
For each Enemy in FrontRow:
    Enemy.Position = BackRow
    Damage = Roll(1d6) + MIGHT
    Enemy.TakeDamage(Damage, "Physical")
```

**Tooltip:** "Concussive Pulse (Rank 1): Push Front Row to Back. 1d6+MIGHT damage. Cost: 35 Stamina"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Push all Front Row enemies to Back Row
- Deal 2d6 + MIGHT Physical damage to each

**Formula:**
```
For each Enemy in FrontRow:
    Enemy.Position = BackRow
    Damage = Roll(2d6) + MIGHT
    Enemy.TakeDamage(Damage, "Physical")
```

**Tooltip:** "Concussive Pulse (Rank 2): Push Front Row to Back. 2d6+MIGHT damage. Cost: 35 Stamina"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Push all Front Row enemies to Back Row
- Deal 2d8 + MIGHT Physical damage to each
- **NEW:** If enemy collides with occupied Back Row, apply [Staggered] for 1 turn

**Formula:**
```
For each Enemy in FrontRow:
    If BackRow.HasEnemies:
        Enemy.AddStatus("Staggered", Duration: 1)
        Log("Collision! {Enemy} is Staggered!")
    Enemy.Position = BackRow
    Damage = Roll(2d8) + MIGHT
    Enemy.TakeDamage(Damage, "Physical")
```

**Tooltip:** "Concussive Pulse (Rank 3): Push to Back. 2d8+MIGHT. Staggers on collision. Cost: 35 Stamina"

---

## Damage Scaling

| Rank | Dice | Average (MIGHT +3) |
|------|------|-------------------|
| 1 | 1d6 | 6.5 |
| 2 | 2d6 | 10 |
| 3 | 2d8 | 12 |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Melee pressure | Push enemies away from allies |
| Back row setup | Position for other abilities |
| Collision damage | Stack enemies for bonus effects |
| Zone control | Clear front line for positioning |

---

## Combat Log Examples

- "Concussive Pulse: 3 enemies pushed to Back Row!"
- "[Enemy A] takes 8 damage and is pushed back"
- "Collision! [Enemy B] is Staggered (Back Row occupied)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Gorge-Maw Ascetic Overview](../gorge-maw-ascetic-overview.md) | Parent specialization |
| [Earthshaker](earthshaker.md) | Capstone control ability |
