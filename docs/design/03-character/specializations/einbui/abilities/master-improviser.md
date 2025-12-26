---
id: ABILITY-EINBUI-27016
title: "Master Improviser"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Master Improviser

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Tags** | [Crafting], [Upgrade] |
| **Ranks** | None (full power when unlocked) |

---

## Description

Your field crafting has reached a level of mastery that others can only dream of. Every trap is perfectly constructed, every concoction optimally brewed—and you do it faster and cheaper than anyone.

---

## Mechanical Effect

**Automatic Rank 3 Crafting:**
- Improvised Trap always uses Rank 3 effects ([Rooted] 2 turns + [Bleeding] 1d4 for 3 turns)
- Basic Concoction always uses Rank 3 effects (Superior Poultice/Exceptional Stimulant)
- Field crafting costs -5 Stamina (minimum 5)
- Can craft 2 items per Standard Action

**Formula:**
```
// Override trap/concoction rank
ImprovisedTrap.EffectiveRank = 3
BasicConcoction.EffectiveRank = 3

// Reduce stamina costs
OnFieldCraft:
    StaminaCost = Max(BaseCost - 5, 5)

// Allow double crafting
OnCraftAction:
    If CraftingType == "Field":
        MaxItemsPerAction = 2
```

**Tooltip:** "Master Improviser: Traps and Concoctions always use Rank 3 effects. -5 Stamina cost. Craft 2 items per action."

---

## Effect Breakdown

### Improvised Trap (Always Rank 3)

| Property | Base | With Master Improviser |
|----------|------|------------------------|
| Stamina Cost | 15 | 10 |
| [Rooted] Duration | 1-2 turns | 2 turns |
| [Bleeding] | None (Rank 1-2) | 1d4 for 3 turns |

### Basic Concoction (Always Rank 3)

| Property | Base | With Master Improviser |
|----------|------|------------------------|
| Stamina Cost | 10 | 5 |
| Poultice Healing | 2d6-4d6 | 6d6 + cure Bleeding |
| Stimulant Restore | 15-30 | 45 + cure Exhausted |

---

## Double Crafting

With one Standard Action, you can:
- Craft 2 Superior Poultices
- Craft 2 Exceptional Stimulants
- Craft 1 Poultice + 1 Stimulant
- Place 2 Improvised Traps

**Material Requirement:** Still need materials for each item crafted

---

## Economy Impact

| Metric | Without | With Master Improviser |
|--------|---------|------------------------|
| Stamina per Trap | 15 | 10 |
| Stamina per Concoction | 10 | 5 |
| Traps per action | 1 | 2 |
| Items per action | 1 | 2 |
| Effective crafting speed | 100% | 200% |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Pre-combat | Rapidly deploy trap fields |
| Low resources | Efficient stamina usage |
| Party support | Mass produce healing items |
| Exploration prep | Stock up quickly during rest |

---

## Combat Log Examples

- "Master Improviser: Improvised Trap uses Rank 3 effects"
- "Basic Concoction Stamina cost reduced: 10 → 5"
- "[Einbui] crafts 2 Superior Poultices (Master Improviser)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Einbui Overview](../einbui-overview.md) | Parent specialization |
| [Improvised Trap](improvised-trap.md) | Upgraded ability |
| [Basic Concoction](basic-concoction.md) | Upgraded ability |
| [Crafting Overview](../../../../04-systems/crafting/crafting-overview.md) | Crafting system |
