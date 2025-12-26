---
id: ABILITY-SCRAP-TINKER-26003
title: "Salvage Expertise"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Salvage Expertise

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Your understanding of pre-Glitch engineering is encyclopedic. Your work is precise, efficient, masterful.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- +1d10 bonus to all Engineering crafting checks
- Crafted gadgets have 15% chance to be [Masterwork]

**Formula:**
```
EngineeringCheckPool += 1d10
MasterworkChance = 0.15
```

**Tooltip:** "Salvage Expertise (Rank 1): +1d10 Engineering. 15% Masterwork chance."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- +2d10 crafting bonus
- Masterwork chance: 25%
- **NEW:** Crafting time reduced by 25%

**Formula:**
```
EngineeringCheckPool += 2d10
MasterworkChance = 0.25
CraftingTimeMultiplier = 0.75
```

**Tooltip:** "Salvage Expertise (Rank 2): +2d10 Engineering. 25% Masterwork. 25% faster crafting."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +3d10 crafting bonus
- Masterwork chance: 40%
- **NEW:** Can craft [Prototype] quality (10% chance, superior to Masterwork)

**Formula:**
```
EngineeringCheckPool += 3d10
MasterworkChance = 0.40
PrototypeChance = 0.10
```

**Tooltip:** "Salvage Expertise (Rank 3): +3d10 Engineering. 40% Masterwork, 10% Prototype."

---

## Quality Tier Effects

| Quality | Visual | Bonus |
|---------|--------|-------|
| Standard | Gray border | Base effects |
| Masterwork | Silver border + ⭐ | Enhanced effects |
| Prototype | Gold border + ⭐⭐ | Effects doubled |

---

## Combat Log Examples

- "Salvage Expertise: Crafting Flash Bomb... MASTERWORK!"
- "Salvage Expertise: +2d10 to Engineering check"
- "Salvage Expertise: PROTOTYPE quality achieved! (10% roll)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Scrap-Tinker Overview](../scrap-tinker-overview.md) | Parent specialization |
| [Crafting System](../../../../04-systems/crafting/crafting-overview.md) | Crafting mechanics |
