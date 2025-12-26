---
id: ABILITY-VEIDIMADUR-24003
title: "Set Snare"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Set Snare

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Ground Location |
| **Resource Cost** | 35 Stamina |
| **Cooldown** | 2 turns |
| **Component Required** | 1 Trap Component |
| **Status Effects** | [Rooted] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Place a concealed trap that ensnares the first enemy to step on it. Masters of this technique can place multiple traps and add damaging mechanisms.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Effect:**
- Place concealed trap
- First enemy to step on it becomes [Rooted] for 1 turn
- Requires 1 Trap Component

**Formula:**
```
PlaceTrap(Location)
OnTrigger:
    Target.AddStatus("Rooted", Duration: 1)
MaxActiveTraps = 1
```

**Tooltip:** "Set Snare (Rank 1): Place trap. First enemy is [Rooted] for 1 turn. Requires Trap Component. Cost: 35 Stamina"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- [Rooted] duration: 2 turns
- Can place up to 2 active traps

**Formula:**
```
RootDuration = 2
MaxActiveTraps = 2
```

**Tooltip:** "Set Snare (Rank 2): [Rooted] for 2 turns. Can have 2 active traps. Cost: 35 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- [Rooted] duration: 3 turns
- **NEW:** Trapped enemy also takes 2d6 Physical damage

**Formula:**
```
RootDuration = 3
OnTrigger:
    Target.AddStatus("Rooted", Duration: 3)
    Target.TakeDamage(Roll(2d6), "Physical")
MaxActiveTraps = 2
```

**Tooltip:** "Set Snare (Rank 3): [Rooted] for 3 turns + 2d6 Physical damage. Can have 2 active traps. Cost: 35 Stamina"

---

## Status Effect: [Rooted]

| Property | Value |
|----------|-------|
| **Duration** | 1-3 turns |
| **Icon** | Vines/chains |
| **Effects** | Cannot move, -2 Defense |

---

## Combat Log Examples

- "[Enemy] triggered snare! [Rooted] for 1 turn!"
- "Set Snare (Rank 2): 2 traps now active."
- "[Enemy] triggered snare! 8 Physical damage + [Rooted] for 3 turns!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Veiðimaðr Overview](../veidimadur-overview.md) | Parent specialization |
| [Rooted](../../../../04-systems/status-effects/rooted.md) | Applied status effect |
