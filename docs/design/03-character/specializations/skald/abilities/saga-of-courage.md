---
id: ABILITY-SKALD-28002
title: "Saga of Courage"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Saga of Courage

**Type:** Active [Performance] | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | All Allies (Aura) |
| **Resource Cost** | 40 Stamina |
| **Performance** | Yes |
| **Duration** | WILL rounds |
| **Ranks** | 1 → 2 → 3 |

---

## Description

A rousing chant of a hero who stood against overwhelming odds. The structured verse creates a pocket of coherence, steadying allies' resolve against the madness of the world.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- While performing, all allies are IMMUNE to [Feared]
- All allies gain +1d10 to WILL Resolve vs Psychic Stress

**Formula:**
```
Duration = Skald.WILL (rounds)
While Performing:
    For each Ally:
        Ally.AddImmunity("Feared")
        Ally.ResolveBonus_Stress += 1d10
```

**Tooltip:** "Saga of Courage (Rank 1): WILL rounds. Allies immune to Fear, +1d10 vs Psychic Stress. Cost: 40 Stamina"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- +2d10 to Stress resistance (increased from +1d10)

**Formula:**
```
Ally.ResolveBonus_Stress += 2d10
```

**Tooltip:** "Saga of Courage (Rank 2): Allies immune to Fear, +2d10 vs Psychic Stress."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +2d10 to Stress resistance
- **NEW:** Allies also gain +1d10 to resist [Disoriented]

**Formula:**
```
Ally.ResolveBonus_Stress += 2d10
Ally.ResolveBonus_Disoriented += 1d10
```

**Tooltip:** "Saga of Courage (Rank 3): Fear immunity, +2d10 vs Stress, +1d10 vs Disoriented."

---

## Performance Mechanics

- Cannot take other Standard Actions while performing
- Duration = WILL score in rounds
- Can be interrupted by [Stunned], [Feared], or [Silenced]
- With Enduring Performance: +3-4 rounds duration

---

## Combat Log Examples

- "Saga of Courage begins! Duration: 5 rounds"
- "[Ally] is immune to Fear (Saga of Courage)"
- "[Ally] +2d10 to Resolve vs Psychic Stress"
- "Saga of Courage interrupted by [Silenced]!"
- "Saga of Courage ends (duration expired)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skald Overview](../skald-overview.md) | Parent specialization |
| [Enduring Performance](enduring-performance.md) | Duration extension |
