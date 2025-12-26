---
id: SPEC-ABILITY-1507
title: "Runelore Mastery"
parent: runasmidr
tier: 3
type: passive
version: 1.0
---

# Runelore Mastery

**Ability ID:** 1507 | **Tier:** 3 | **Type:** Passive | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Prerequisite** | 16 PP in Rúnasmiðr tree |
| **Starting Rank** | 2 |

---

## Description

> The patterns are second nature now. Your hand moves before your mind thinks.

---

## Rank Progression

### Rank 2 (Starting Rank)

**Mechanical Effects:**
- All runeink costs reduced 40%
- Runeforging time reduced 50%
- **NEW:** Can craft runeink without alchemy check

**Formula:**
```
RuneinkCostMultiplier = 0.60
CraftingTimeMultiplier = 0.50
AutoCraftRuneink = true
```

**GUI Display:**
- Passive icon: Stylized rune with speed lines
- Tooltip: "Runelore Mastery (Rank 2): -40% runeink cost. -50% time. Auto-craft ink."
- Border: Silver

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- All runeink costs reduced 50%
- Runeforging time reduced 75%
- **NEW:** Corruption from runic failures reduced to 1/4

**Formula:**
```
RuneinkCostMultiplier = 0.50
CraftingTimeMultiplier = 0.25
CorruptionMultiplier = 0.25
```

**GUI Display:**
- Tooltip: "Runelore Mastery (Rank 3): -50% ink. -75% time. 1/4 corruption."
- Border: Gold

---

## Cost Reduction Examples

| Action | Base Cost | Rank 2 Cost | Rank 3 Cost |
|--------|-----------|-------------|-------------|
| Simple rune | 10 ink | 6 ink | 5 ink |
| Standard rune | 20 ink | 12 ink | 10 ink |
| Complex rune | 35 ink | 21 ink | 17 ink |
| Elder rune | 50 ink | 30 ink | 25 ink |

---

## Time Reduction Examples

| Action | Base Time | Rank 2 Time | Rank 3 Time |
|--------|-----------|-------------|-------------|
| Simple rune | 1 hour | 30 min | 15 min |
| Standard rune | 2 hours | 1 hour | 30 min |
| Complex rune | 4 hours | 2 hours | 1 hour |
| Elder rune | 8 hours | 4 hours | 2 hours |

---

## Synergy Table

| Combination | Effect |
|-------------|--------|
| + Inscription Expertise | Time reductions stack |
| + Elder Patterns | Combinations faster too |
| + All-Rune Glimpse | Corruption reduction crucial |

---

## Implementation Status

### Balance Data

#### Efficiency
- **Cost:** -50% runeink saves ~25 Hacksilver per inscription.
- **Time:** -75% time means 4x more inscriptions per day.
- **Corruption:** 1/4 makes high-DC attempts much safer.

---

### Phased Implementation Guide

#### Phase 1: Mechanics
- [ ] **Cost**: Hook `GetRuneinkCost()` -> Apply multiplier.
- [ ] **Time**: Hook `GetCraftingTime()` -> Apply multiplier.

#### Phase 2: Logic Integration
- [ ] **Auto-Craft**: Skip alchemy check for runeink if ability present.
- [ ] **Corruption**: Hook `OnCorruption` -> Apply multiplier.

#### Phase 3: Visuals
- [ ] **UI**: Display effective cost/time in crafting UI.

---

### Testing Requirements

#### Unit Tests
- [ ] **Cost**: 20 base ink, Rank 2 = 12 ink.
- [ ] **Time**: 2 hour base, Rank 3 = 30 min.

#### Integration Tests
- [ ] **Stacking**: Expertise time + Mastery time = Combined reduction.

#### Manual QA
- [ ] **Tooltip**: Shows "-50% ink, -75% time" at Rank 3.

---

### Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

#### Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Cost | Debug | "Runelore Mastery reduced cost to {Amount}." | `Amount` |
| Time | Debug | "Crafting time reduced to {Duration}." | `Duration` |

---

### Related Specifications
| Document | Purpose |
|----------|---------|
| [Runeforging](../../../../04-systems/crafting/runeforging.md) | Rune system |

---

### Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
