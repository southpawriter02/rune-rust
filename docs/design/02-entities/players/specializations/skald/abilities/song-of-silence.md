---
id: ABILITY-SKALD-28005
title: "Song of Silence"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Song of Silence

**Type:** Active (NOT Performance) | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Intelligent Enemy |
| **Resource Cost** | 45 Stamina |
| **Opposed Check** | WILL + Rhetoric vs enemy WILL |
| **Status Effect** | [Silenced] |
| **Performance** | **No** (instant effect) |
| **Ranks** | 2 → 3 |

---

## Description

A counter-resonant chant designed to disrupt hostile vocalizations, choking the words in your enemy's throat.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Opposed check: WILL + Rhetoric vs enemy WILL
- On success: Apply [Silenced] for 3 rounds

**Formula:**
```
SkaldRoll = Roll(WILL + Rhetoric)
EnemyRoll = Roll(Enemy.WILL)

If SkaldRoll > EnemyRoll:
    Enemy.AddStatus("Silenced", Duration: 3)
```

**Tooltip:** "Song of Silence (Rank 2): WILL + Rhetoric vs WILL. [Silenced] 3 rounds. Cost: 45 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- [Silenced] for 3 rounds
- **NEW:** Target takes 2d6 Psychic damage from vocal disruption

**Formula:**
```
If SkaldRoll > EnemyRoll:
    Enemy.AddStatus("Silenced", Duration: 3)
    Enemy.TakeDamage(Roll(2d6), "Psychic")
```

**Tooltip:** "Song of Silence (Rank 3): [Silenced] 3 rounds + 2d6 Psychic damage."

---

## [Silenced] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 3 rounds |
| **Effects** | Cannot cast spells |
| | Cannot use speech-based abilities |
| | Cannot use [Performance] abilities |

---

## Tactical Use

Song of Silence is excellent against:
- Enemy casters
- Enemy Skalds
- Commanders giving orders
- Any speech-based ability users

---

## Combat Log Examples

- "Song of Silence: Opposed check vs [Enemy Caster]..."
- "Song of Silence SUCCESS! [Enemy] is Silenced for 3 rounds!"
- "[Enemy] cannot cast spells (Silenced)"
- "Song of Silence (Rank 3): 8 Psychic damage + Silenced!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skald Overview](../skald-overview.md) | Parent specialization |
| [Silenced Status](../../../../04-systems/status-effects/silenced.md) | Status effect details |
