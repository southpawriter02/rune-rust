---
id: ABILITY-SKJALDMAER-26022
title: "Guardian's Taunt"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Guardian's Taunt

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | AoE (Front Row or All Enemies) |
| **Resource Cost** | 30 Stamina + Variable Psychic Stress |
| **Status Effects** | [Taunted] |
| **Ranks** | 2 → 3 |

---

## Description

Projection of coherent will draws even maddened creatures to attack. The Skjaldmær's presence becomes a beacon of stability that enemies cannot ignore—but maintaining this projection takes a toll on her own psyche.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- All enemies in Front Row gain [Taunted] for 2 rounds
- [Taunted] enemies must target the Skjaldmær with their attacks if able
- **Psychic Stress Cost:** Skjaldmær gains 5 Psychic Stress

**Formula:**
```
For each Enemy in FrontRow:
    Enemy.AddStatusEffect("Taunted", Duration: 2, TauntSource: Skjaldmaer)
Skjaldmaer.PsychicStress += 5
```

**Tooltip:** "Guardian's Taunt (Rank 2): Taunt all Front Row enemies for 2 rounds. Cost: 30 Stamina, 5 Psychic Stress"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- **ALL enemies** (Front AND Back Row) gain [Taunted] for 2 rounds
- [Taunted] enemies must target the Skjaldmær with their attacks if able
- **Reduced Psychic Stress Cost:** Skjaldmær gains only 3 Psychic Stress

**Formula:**
```
For each Enemy in AllEnemies:  // Now includes Back Row!
    Enemy.AddStatusEffect("Taunted", Duration: 2, TauntSource: Skjaldmaer)
Skjaldmaer.PsychicStress += 3  // Reduced from 5
```

**Tooltip:** "Guardian's Taunt (Rank 3): Taunt ALL enemies for 2 rounds. Cost: 30 Stamina, 3 Psychic Stress"

---

## Status Effect: [Taunted]

| Property | Value |
|----------|-------|
| **Duration** | 2 rounds |
| **Icon** | Angry red arrow |
| **Effects** | Must target taunt source, cannot move away |

---

## Combat Log Examples

- "Guardian's Taunt draws 3 enemies! (Skjaldmær gains 5 Psychic Stress)"
- "Guardian's Taunt (Rank 3) draws ALL 5 enemies! (Skjaldmær gains 3 Psychic Stress)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skjaldmær Overview](../skjaldmaer-overview.md) | Parent specialization |
