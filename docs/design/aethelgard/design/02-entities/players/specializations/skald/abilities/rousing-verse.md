---
id: ABILITY-SKALD-28004
title: "Rousing Verse"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Rousing Verse

**Type:** Active (NOT Performance) | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Ally |
| **Resource Cost** | 35 Stamina |
| **Performance** | **No** (instant effect) |
| **Ranks** | 2 → 3 |

---

## Description

A quick verse from a saga about a tireless warrior, banishing fatigue through structured recollection of legendary endurance.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Restore 15 + (WILL × 2) Stamina to single ally

**Formula:**
```
StaminaRestored = 15 + (Skald.WILL * 2)
Target.Stamina += StaminaRestored
```

**Tooltip:** "Rousing Verse (Rank 2): Restore 15 + (WILL × 2) Stamina to ally. Cost: 35 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Restore 25 + (WILL × 3) Stamina
- **NEW:** Also remove [Exhausted] status

**Formula:**
```
StaminaRestored = 25 + (Skald.WILL * 3)
Target.Stamina += StaminaRestored
Target.RemoveStatus("Exhausted")
```

**Tooltip:** "Rousing Verse (Rank 3): Restore 25 + (WILL × 3) Stamina. Removes [Exhausted]."

---

## Stamina Restoration Examples

| WILL Score | Rank 2 | Rank 3 |
|------------|--------|--------|
| 3 | 21 | 34 |
| 4 | 23 | 37 |
| 5 | 25 | 40 |
| 6 | 27 | 43 |

---

## Not a Performance

Unlike most Skald abilities, Rousing Verse is an instant effect:
- Does not require concentration
- Can be used while maintaining another Performance
- Does not prevent other actions

---

## Combat Log Examples

- "Rousing Verse: [Ally] restored 25 Stamina"
- "Rousing Verse (Rank 3): [Ally] restored 40 Stamina, [Exhausted] removed!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skald Overview](../skald-overview.md) | Parent specialization |
