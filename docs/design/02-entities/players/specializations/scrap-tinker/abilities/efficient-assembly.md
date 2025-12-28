---
id: ABILITY-SCRAP-TINKER-26008
title: "Efficient Assembly"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Efficient Assembly

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Ranks** | 2 → 3 |

---

## Description

Muscle memory. Optimized workflows. You assemble gadgets faster than most people load a gun.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- All gadget crafting costs 40% less Scrap Materials
- Crafting time reduced by 75%
- **NEW:** Can craft 2 gadgets simultaneously

**Formula:**
```
ScrapCostMultiplier = 0.60
CraftingTimeMultiplier = 0.25
SimultaneousCrafts = 2
```

**Tooltip:** "Efficient Assembly (Rank 2): -40% Scrap cost. -75% time. Craft 2 at once."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Costs 50% less Scrap
- Some gadgets craftable instantly (Flash Bombs, Repair Kits)
- **NEW:** Can craft 3 gadgets simultaneously

**Formula:**
```
ScrapCostMultiplier = 0.50
InstantCraft = ["FlashBomb", "RepairKit"]
SimultaneousCrafts = 3
```

**Tooltip:** "Efficient Assembly (Rank 3): -50% Scrap cost. Flash Bombs instant. Craft 3 at once."

---

## Cost Reduction Examples

| Item | Base Cost | Rank 2 (-40%) | Rank 3 (-50%) |
|------|-----------|---------------|---------------|
| Flash Bomb | 10 Scrap | 6 Scrap | 5 Scrap |
| Shock Mine | 15 Scrap | 9 Scrap | 8 Scrap |
| Scout Drone | 15 Scrap | 9 Scrap | 8 Scrap |
| Weapon Mod | 20 Scrap | 12 Scrap | 10 Scrap |
| Scrap Golem | 50 Scrap | 30 Scrap | 25 Scrap |

---

## Combat Log Examples

- "Efficient Assembly: Flash Bomb cost reduced to 6 Scrap"
- "Efficient Assembly: Crafting 2 items simultaneously..."
- "Efficient Assembly (Rank 3): Flash Bomb crafted instantly!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Scrap-Tinker Overview](../scrap-tinker-overview.md) | Parent specialization |
| [Crafting System](../../../../04-systems/crafting/crafting-overview.md) | Crafting mechanics |
