---
id: ABILITY-GORGE-MAW-26011
title: "Stone Fist"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Stone Fist

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 30 Stamina |
| **Damage Type** | Physical |
| **Attribute** | MIGHT |
| **Tags** | [Unarmed], [Melee] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Your weighted gauntlets channel seismic force into devastating unarmed strikes. Each blow carries the weight of the earth behind it.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Deal 2d8 + MIGHT Physical damage
- Single target, melee range

**Formula:**
```
Damage = Roll(2d8) + MIGHT
Target.TakeDamage(Damage, "Physical")
```

**Tooltip:** "Stone Fist (Rank 1): 2d8+MIGHT Physical damage. Cost: 30 Stamina"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Deal 3d8 + MIGHT Physical damage
- Single target, melee range

**Formula:**
```
Damage = Roll(3d8) + MIGHT
Target.TakeDamage(Damage, "Physical")
```

**Tooltip:** "Stone Fist (Rank 2): 3d8+MIGHT Physical damage. Cost: 30 Stamina"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Deal 4d8 + MIGHT Physical damage
- Single target, melee range
- **NEW:** 10% chance to apply [Staggered] for 1 turn

**Formula:**
```
Damage = Roll(4d8) + MIGHT
Target.TakeDamage(Damage, "Physical")

If Roll(1d100) <= 10:
    Target.AddStatus("Staggered", Duration: 1)
    Log("Stone Fist staggers {Target}!")
```

**Tooltip:** "Stone Fist (Rank 3): 4d8+MIGHT Physical. 10% Stagger chance. Cost: 30 Stamina"

---

## Damage Scaling

| Rank | Dice | Average (MIGHT +3) |
|------|------|-------------------|
| 1 | 2d8 | 12 |
| 2 | 3d8 | 16.5 |
| 3 | 4d8 | 21 |

---

## [Staggered] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 1 turn |
| **Effect** | -1 Defense, no Reactions, +1 movement cost |
| **Removal** | End of turn |

---

## Combat Log Examples

- "Stone Fist: 14 Physical damage to [Enemy]"
- "Stone Fist (Rank 3): 23 Physical damage + [Staggered]!"
- "Stone Fist: 10% Stagger check... failed"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Gorge-Maw Ascetic Overview](../gorge-maw-ascetic-overview.md) | Parent specialization |
| [Concussive Pulse](concussive-pulse.md) | AoE alternative |
