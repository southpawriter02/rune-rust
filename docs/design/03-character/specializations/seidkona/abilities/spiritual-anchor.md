---
id: ABILITY-SEIDKONA-27005
title: "Spiritual Anchor"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Spiritual Anchor

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Self |
| **Resource Cost** | Aether |
| **Effect** | Remove Psychic Stress |
| **Ranks** | 2 → 3 |

---

## Description

You anchor your consciousness to the present, pulling yourself back from the edge of madness through meditation and will.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Enter meditative state (cannot take other actions this turn)
- Remove 20 Psychic Stress from self

**Formula:**
```
Caster.CannotActThisTurn = true
Caster.PsychicStress -= 20
```

**Tooltip:** "Spiritual Anchor (Rank 2): Meditate. Remove 20 Psychic Stress. (No other actions this turn)"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Remove 30 Psychic Stress
- **NEW:** Cleanse one mental debuff ([Fear], [Disoriented], [Charmed])

**Formula:**
```
Caster.CannotActThisTurn = true
Caster.PsychicStress -= 30
Caster.RemoveStatus(OneOf: ["Feared", "Disoriented", "Charmed"])
```

**Tooltip:** "Spiritual Anchor (Rank 3): Remove 30 Psychic Stress + cleanse mental debuff."

---

## Stress Recovery Comparison

| Source | Stress Removed |
|--------|----------------|
| Natural recovery | ~5/rest |
| Skald support | Variable |
| Spiritual Anchor (Rank 2) | 20 |
| Spiritual Anchor (Rank 3) | 30 |

---

## Moment of Clarity Enhancement

During Moment of Clarity:
- Can target an ally instead of self
- Stress removal still applies

---

## Combat Log Examples

- "Spiritual Anchor: Entering meditative state..."
- "Spiritual Anchor: -20 Psychic Stress (50 → 30)"
- "Spiritual Anchor (Rank 3): -30 Stress, [Feared] cleansed"
- "Moment of Clarity: Spiritual Anchor cast on [Ally]"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Seiðkona Overview](../seidkona-overview.md) | Parent specialization |
| [Stress System](../../../../01-core/resources/stress.md) | Psychic Stress mechanics |
