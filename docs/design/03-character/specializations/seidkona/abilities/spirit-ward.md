---
id: ABILITY-SEIDKONA-27007
title: "Spirit Ward"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Spirit Ward

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Row (All Allies) |
| **Resource Cost** | Aether |
| **Status Effect** | [Spirit Ward] |
| **Spirit Bargain** | Extended duration |
| **Ranks** | None (full power when unlocked) |

---

## Description

You weave a protective ward from friendly spirits, creating a barrier against psychic assault.

---

## Mechanical Effect

**Base Effect:**
- Place [Spirit Ward] on your row for 3 turns
- Allies in ward: Negate environmental Psychic Stress
- Allies gain +2d10 to Resolve vs psychic attacks
- **[Spirit Bargain] 25%:** Ward lasts 4 turns instead of 3

**Formula:**
```
For each Ally in Caster.Row:
    Ally.AddStatus("SpiritWard", Duration: 3)
    Ally.EnvironmentalStressImmunity = true
    Ally.ResolveBonus_Psychic += 2d10

If Random() < 0.25:  // Spirit Bargain
    Ward.Duration = 4
    Log("Spirit Bargain: Ward duration extended!")
```

**Tooltip:** "Spirit Ward: Row gains psychic protection for 3 turns. +2d10 Resolve vs psychic. 25% chance: 4 turns."

---

## [Spirit Ward] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 3-4 turns |
| **Effects** | Negate environmental Psychic Stress |
| **Bonus** | +2d10 Resolve vs psychic attacks |
| **Visual** | Shimmering protective aura |

---

## With Fickle Fortune

| Base Chance | With Rank 2 | With Rank 3 |
|-------------|-------------|-------------|
| 25% | 40% | 50% |

---

## Moment of Clarity Enhancement

During Moment of Clarity:
- Duration always 4 turns (guaranteed bargain)
- Can place ward on BOTH rows (costs double Aether)

---

## Combat Log Examples

- "Spirit Ward placed on front row. 3 turn duration."
- "[Ally] protected from environmental Psychic Stress (Spirit Ward)"
- "[Ally] +2d10 to Resolve vs [Psychic Attack]"
- "Spirit Bargain: Ward duration extended to 4 turns!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Seiðkona Overview](../seidkona-overview.md) | Parent specialization |
| [Stress System](../../../../01-core/resources/stress.md) | Psychic Stress mechanics |
