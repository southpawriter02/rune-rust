---
id: ABILITY-SKJALDMAER-26020
title: "Shield Bash"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Shield Bash

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 40 Stamina |
| **Attribute** | MIGHT |
| **Damage Type** | Physical |
| **Status Effects** | [Staggered] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Slam shield into foe—a brutal statement of physical truth. The impact can stagger even the most stalwart opponents, and masters of this technique can send enemies reeling backward.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Effect:**
- Damage: 1d8 + MIGHT (Physical)
- Stagger Chance: 50%
- If Stagger succeeds: Target gains [Staggered] for 1 turn

**Formula:**
```
Damage = Roll(1d8) + MIGHT
StaggerCheck = Roll(1d100) <= 50
```

**Tooltip:** "Shield Bash (Rank 1): 1d8+MIGHT damage, 50% Stagger chance. Cost: 40 Stamina"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Damage: 2d8 + MIGHT (Physical)
- Stagger Chance: 65%
- If Stagger succeeds: Target gains [Staggered] for 1 turn

**Formula:**
```
Damage = Roll(2d8) + MIGHT
StaggerCheck = Roll(1d100) <= 65
```

**Tooltip:** "Shield Bash (Rank 2): 2d8+MIGHT damage, 65% Stagger chance. Cost: 40 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Damage: 3d8 + MIGHT (Physical)
- Stagger Chance: 75%
- If Stagger succeeds: Target gains [Staggered] for 1 turn
- **NEW:** If Stagger succeeds AND target is in Front Row, push target to Back Row

**Formula:**
```
Damage = Roll(3d8) + MIGHT
StaggerCheck = Roll(1d100) <= 75
If (StaggerCheck AND Target.Row == Front):
    Target.Row = Back
```

**Tooltip:** "Shield Bash (Rank 3): 3d8+MIGHT damage, 75% Stagger chance. Staggered enemies pushed to Back Row. Cost: 40 Stamina"

---

## Status Effect: [Staggered]

| Property | Value |
|----------|-------|
| **Duration** | 1 turn |
| **Icon** | Dizzy stars |
| **Effects** | -1 Defense, cannot React, movement slowed |

---

## Combat Log Examples

- "Shield Bash deals 9 damage! (1d8[5] + MIGHT[4])"
- "Shield Bash (Rank 2) deals 14 damage! Target is Staggered!"
- "Shield Bash (Rank 3) deals 22 damage! Target is Staggered and pushed to Back Row!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skjaldmær Overview](../skjaldmaer-overview.md) | Parent specialization |
