---
id: ABILITY-GORGE-MAW-26016
title: "Earthen Grasp"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Earthen Grasp

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | All enemies (Front + Back Row) |
| **Resource Cost** | 45 Stamina |
| **Cooldown** | 3 turns |
| **Damage Type** | Physical |
| **Status Effect** | [Rooted], [Vulnerable] |
| **Tags** | [AoE], [Control] |
| **Ranks** | None (full power when unlocked) |

---

## Description

The earth itself erupts in grasping hands of stone and soil, seizing all enemies and holding them fast. Those caught in Earthen Grasp find themselves vulnerable to follow-up attacks.

---

## Mechanical Effect

**AoE Root:**
- Targets ALL enemies in both Front Row and Back Row
- Apply [Rooted] for 3 turns
- Deal 4d6 Physical damage
- Apply [Vulnerable] while [Rooted] (linked duration)

**Formula:**
```
For each Enemy in (FrontRow + BackRow):
    Damage = Roll(4d6)
    Enemy.TakeDamage(Damage, "Physical")
    Enemy.AddStatus("Rooted", Duration: 3)
    Enemy.AddStatus("Vulnerable", Duration: 3, LinkedTo: "Rooted")

    Log("Earthen Grasp: {Enemy} takes {Damage} and is Rooted!")
```

**Tooltip:** "Earthen Grasp: Both rows, [Rooted] 3 turns, 4d6 damage, [Vulnerable] while Rooted. Cost: 45 Stamina"

---

## [Rooted] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 3 turns |
| **Effect** | Cannot move or use movement abilities |
| **Actions** | Can still attack and use non-movement abilities |
| **Removal** | Ally Standard Action, or duration expires |

---

## [Vulnerable] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | While [Rooted] |
| **Effect** | +2 dice to attacks against this target |
| **Removal** | When [Rooted] is removed |

---

## Damage Output

| Metric | Value |
|--------|-------|
| Base Damage | 4d6 |
| Average | 14 |
| Minimum | 4 |
| Maximum | 24 |
| Per Enemy | Same damage to each |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Mass lockdown | Freeze all enemies for 3 turns |
| Damage setup | Vulnerable enables big follow-ups |
| Escape prevention | Stop fleeing enemies |
| Earthshaker combo | Root + Earthshaker devastation |

---

## Optimal Combo

1. **Turn 1:** Earthen Grasp (Root all enemies, apply Vulnerable)
2. **Turn 2:** Party focuses fire on Vulnerable targets
3. **Turn 3:** Earthshaker while still Rooted = guaranteed Knocked Down

---

## Combat Log Examples

- "Earthen Grasp! 4 enemies Rooted and Vulnerable for 3 turns!"
- "[Enemy A] takes 18 damage and is [Rooted]"
- "[Enemy A] is [Vulnerable] while Rooted"
- "[Enemy B] cannot move (Rooted)"
- "Party gains +2 dice vs [Rooted] targets (Vulnerable)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Gorge-Maw Ascetic Overview](../gorge-maw-ascetic-overview.md) | Parent specialization |
| [Rooted Status](../../../../04-systems/status-effects/rooted.md) | Status effect details |
| [Earthshaker](earthshaker.md) | Capstone follow-up |
