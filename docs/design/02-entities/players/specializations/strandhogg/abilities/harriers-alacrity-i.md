---
id: ABILITY-STRANDHOGG-25001
title: "Harrier's Alacrity I"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Harrier's Alacrity I

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Tags** | [Momentum], [Initiative] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You are always in motion, always ready. When combat begins, you're already building speed while others are still drawing weapons. Your natural agility ensures you act before slower foes.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Start every combat with 20 Momentum
- Gain +2 bonus to Vigilance (turn order)

**Formula:**
```
OnCombatStart:
    Caster.Momentum = 20
    Log("Harrier's Alacrity: Starting with 20 Momentum")

OnVigilanceRoll:
    VigilanceBonus += 2
```

**Tooltip:** "Harrier's Alacrity I (Rank 1): Start combat with 20 Momentum. +2 Vigilance."

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Start every combat with 20 Momentum
- Vigilance bonus increased to +3

**Formula:**
```
OnCombatStart:
    Caster.Momentum = 20

OnVigilanceRoll:
    VigilanceBonus += 3
```

**Tooltip:** "Harrier's Alacrity I (Rank 2): Start with 20 Momentum. +3 Vigilance."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Start every combat with 30 Momentum
- Vigilance bonus +3

**Formula:**
```
OnCombatStart:
    Caster.Momentum = 30
    Log("Harrier's Alacrity: Starting with 30 Momentum!")

OnVigilanceRoll:
    VigilanceBonus += 3
```

**Tooltip:** "Harrier's Alacrity I (Rank 3): Start with 30 Momentum. +3 Vigilance."

---

## Rank Comparison

| Rank | Starting Momentum | Vigilance Bonus |
|------|-------------------|-----------------|
| 1 | 20 | +2 |
| 2 | 20 | +3 |
| 3 | 30 | +3 |

---

## Strategic Impact

**Starting Momentum Value:**
- 20 Momentum: Enables Vicious Flank immediately (25 cost)
- 30 Momentum: Enables Harrier's Whirlwind immediately (30 cost)

**Vigilance Bonus:**
- Going first means applying [Disoriented] before enemies act
- Sets up Tidal Rush bonuses for team

---

## Combat Log Examples

- "Harrier's Alacrity: Combat begins with 20 Momentum!"
- "Vigilance roll: +3 bonus (Harrier's Alacrity)"
- "Harrier's Alacrity (Rank 3): Starting with 30 Momentum!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Strandhogg Overview](../strandhogg-overview.md) | Parent specialization |
| [Momentum Resource](../../../../01-core/resources/momentum.md) | Resource system |
| [Dread Charge](dread-charge.md) | First-turn combo |
