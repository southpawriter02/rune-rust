---
id: ABILITY-IRONBANE-1107
title: "Purging Flame"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Purging Flame

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | All Enemies (Front Row) |
| **Resource Cost** | 55 Stamina + 40 Fervor |
| **Cooldown** | 5 turns |
| **Status Effects** | [Burning], [Vulnerable] |
| **Ranks** | 2 → 3 |

---

## Description

A wave of cleansing fire washes over the battlefield. Corrupted metal screams as it melts.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Damage: 5d8 Fire to all enemies in Front Row
- Mechanical/Undying: Take double damage
- Mechanical/Undying: [Burning] 2d6/turn for 4 turns (cannot be cleansed)

**Formula:**
```
For each Enemy in FrontRow:
    Damage = Roll(5d8)
    If (Enemy.Type == "Mechanical" OR Enemy.Type == "Undying"):
        Damage *= 2
        Enemy.AddStatus("Burning", DamagePerTurn: Roll(2d6), Duration: 4, CanCleanse: false)
```

**Tooltip:** "Purging Flame (Rank 2): 5d8 Fire to Front Row. Mech/Undying: 2x damage, [Burning] 2d6/turn for 4 turns (uncleansable). Cost: 55 Stamina, 40 Fervor"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Damage: 6d8 Fire
- **NEW:** Mechanical/Undying also gain [Vulnerable] (+50% damage taken) for 2 turns

**Formula:**
```
For each Enemy in FrontRow:
    Damage = Roll(6d8)
    If (Enemy.Type == "Mechanical" OR Enemy.Type == "Undying"):
        Damage *= 2
        Enemy.AddStatus("Burning", DamagePerTurn: Roll(2d6), Duration: 4, CanCleanse: false)
        Enemy.AddStatus("Vulnerable", DamageIncrease: 0.50, Duration: 2)
```

**Tooltip:** "Purging Flame (Rank 3): 6d8 Fire. Mech/Undying: 2x damage, [Burning], [Vulnerable] +50% for 2 turns."

---

## Status Effect: [Vulnerable]

| Property | Value |
|----------|-------|
| **Duration** | 2 turns |
| **Icon** | Cracked armor |
| **Effects** | +50% damage taken from all sources |

---

## Combat Log Examples

- "Purging Flame sweeps the front row! 3 enemies hit for 24-48 damage!"
- "[Automaton] takes 48 damage (doubled) and gains [Burning] (uncleansable)!"
- "Purging Flame (Rank 3): [Vulnerable] applied! +50% damage for 2 turns."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Iron-Bane Overview](../iron-bane-overview.md) | Parent specialization |
