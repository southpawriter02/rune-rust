---
id: ABILITY-SKALD-28007
title: "Lay of the Iron Wall"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Lay of the Iron Wall

**Type:** Active [Performance] | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | All Front Row Allies |
| **Resource Cost** | 55 Stamina |
| **Performance** | Yes |
| **Duration** | WILL rounds |
| **Ranks** | None (full power when unlocked) |

---

## Description

The story of the unbreakable shield wall at the Battle of Black Pass. The narrative imposes structural coherence on the formation, making flesh as sturdy as iron.

---

## Mechanical Effect

**While Performing:**
- All Front Row allies gain +4 Soak (damage reduction)
- All Front Row allies gain resistance to Push/Pull effects

**Formula:**
```
Duration = Skald.WILL (rounds)
While Performing:
    For each Ally in FrontRow:
        Ally.Soak += 4
        Ally.AddResistance("Push")
        Ally.AddResistance("Pull")
```

**Tooltip:** "Lay of the Iron Wall: Front Row allies +4 Soak, resist Push/Pull. WILL rounds. Cost: 55 Stamina"

---

## Tactical Applications

| Scenario | Value |
|----------|-------|
| Holding a chokepoint | Excellent |
| Protecting back row | Strong |
| Against knockback enemies | Counters completely |
| Mobile combat | Limited (front row only) |

---

## Synergy with Other Abilities

- **Saga of the Einherjar** - Combined defensive + offensive buff
- **Heart of the Clan** - Additional Resolve protection
- **Dual Performance (Rank 3)** - Stack with Saga of Courage

---

## Combat Log Examples

- "Lay of the Iron Wall begins! Duration: 5 rounds"
- "[Front Row Ally] gains +4 Soak (Iron Wall)"
- "[Front Row Ally] resists Push effect! (Iron Wall)"
- "Lay of the Iron Wall ends"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skald Overview](../skald-overview.md) | Parent specialization |
| [Enduring Performance](enduring-performance.md) | Duration extension |
