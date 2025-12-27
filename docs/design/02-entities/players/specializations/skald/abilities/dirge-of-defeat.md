---
id: ABILITY-SKALD-28003
title: "Dirge of Defeat"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Dirge of Defeat

**Type:** Active [Performance] | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | All Intelligent Enemies (Aura) |
| **Resource Cost** | 40 Stamina |
| **Performance** | Yes |
| **Duration** | WILL rounds |
| **Ranks** | 1 → 2 → 3 |

---

## Description

A sorrowful dirge recounting the doom of a great army. The narrative weight unnerves intelligent foes, filling them with the certainty of their own defeat.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- All intelligent enemies suffer -1d10 penalty to Accuracy
- All intelligent enemies suffer -1d10 penalty to damage
- Does NOT affect mindless or Undying enemies

**Formula:**
```
Duration = Skald.WILL (rounds)
While Performing:
    For each IntelligentEnemy:
        Enemy.AccuracyPenalty += 1d10
        Enemy.DamagePenalty += 1d10
```

**Tooltip:** "Dirge of Defeat (Rank 1): Intelligent enemies -1d10 accuracy and damage. WILL rounds. Cost: 40 Stamina"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Penalty increased to -2d10

**Formula:**
```
Enemy.AccuracyPenalty += 2d10
Enemy.DamagePenalty += 2d10
```

**Tooltip:** "Dirge of Defeat (Rank 2): Intelligent enemies -2d10 accuracy and damage."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- -2d10 accuracy and damage penalty
- **NEW:** Intelligent enemies take 1d4 Psychic damage per turn from narrative weight

**Formula:**
```
Enemy.AccuracyPenalty += 2d10
Enemy.DamagePenalty += 2d10
OnEnemyTurnStart:
    If Enemy.IsIntelligent:
        Enemy.TakeDamage(Roll(1d4), "Psychic")
```

**Tooltip:** "Dirge of Defeat (Rank 3): -2d10 penalties + 1d4 Psychic damage/turn to intelligent enemies."

---

## Target Restrictions

| Enemy Type | Affected? |
|------------|-----------|
| Intelligent (humans, etc.) | Yes |
| Beasts | No |
| Mindless constructs | No |
| Undying | No |
| Corrupted intelligent | Yes |

---

## Combat Log Examples

- "Dirge of Defeat begins! Duration: 5 rounds"
- "[Intelligent Enemy] suffers -2d10 accuracy and damage"
- "[Mindless Construct] is unaffected (not intelligent)"
- "Dirge of Defeat (Rank 3): [Enemy] takes 3 Psychic damage (narrative weight)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skald Overview](../skald-overview.md) | Parent specialization |
| [Enduring Performance](enduring-performance.md) | Duration extension |
