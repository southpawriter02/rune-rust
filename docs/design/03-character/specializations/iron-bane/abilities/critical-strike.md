---
id: ABILITY-IRONBANE-1105
title: "Critical Strike"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Critical Strike

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Mechanical/Undying only) |
| **Resource Cost** | 45 Stamina + 25 Fervor |
| **Cooldown** | 4 turns |
| **Special** | Guaranteed Critical Hit |
| **Ranks** | 2 → 3 |

---

## Description

You've identified the critical failure point. One precise strike and their entire system collapses.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Damage: 5d8 Fire damage
- Guaranteed critical hit vs Mechanical/Undying (applies Weakness Exploiter bonuses)
- **NEW:** Target loses 1 action next turn

**Formula:**
```
Requires: Target.Type == "Mechanical" OR Target.Type == "Undying"
Damage = Roll(5d8) * CriticalMultiplier  // 2x or 3x with Weakness Exploiter R3
Target.ActionsNextTurn -= 1
```

**Tooltip:** "Critical Strike (Rank 2): 5d8 Fire. Guaranteed crit vs Mech/Undying. Target loses 1 action. Cost: 45 Stamina, 25 Fervor"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Damage: 6d8 Fire damage
- **NEW:** If target is below 40% HP: instant death (execute)

**Formula:**
```
Damage = Roll(6d8) * CriticalMultiplier
If (Target.HP < Target.MaxHP * 0.40):
    Target.HP = 0  // Execute
```

**Tooltip:** "Critical Strike (Rank 3): 6d8 Fire. Guaranteed crit. EXECUTE below 40% HP."

---

## Combat Log Examples

- "Critical Strike: Guaranteed crit! 42 Fire damage (5d8 x2)!"
- "Critical Strike (Rank 3): EXECUTE! [Automaton at 35% HP] destroyed!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Iron-Bane Overview](../iron-bane-overview.md) | Parent specialization |
| [Weakness Exploiter](weakness-exploiter.md) | Critical multiplier synergy |
