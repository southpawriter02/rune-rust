---
id: ABILITY-SCRAP-TINKER-26006
title: "Weapon Modification"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Weapon Modification

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action (out-of-combat) |
| **Target** | Single ally weapon |
| **Resource Cost** | 20 Scrap Materials |
| **Requirement** | Workbench, out-of-combat |
| **Duration** | Permanent |
| **Ranks** | 2 → 3 |

---

## Description

You disassemble the weapon, integrate salvaged components, reassemble. Better than factory spec.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Apply permanent modification (10 minutes at workbench):
  - **[Elemental]:** +2d6 Fire/Frost/Lightning damage
  - **[Precision]:** +2 to hit
  - **[Reinforced]:** +100% durability + 10% crit chance

**Formula:**
```
Requires: Workbench, OutOfCombat
ModOptions:
    Elemental: Damage += Roll(2d6, ChosenElement)
    Precision: HitBonus += 2
    Reinforced: Durability *= 2, CritChance += 0.10
ScrapCost = 20
```

**Tooltip:** "Weapon Modification (Rank 2): [Elemental] +2d6, [Precision] +2 hit, or [Reinforced] +100% dur +10% crit. Cost: 20 Scrap"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- **NEW:** Can apply 2 modifications to same weapon (stacking)
- Prototype quality mods: bonus doubled

**Formula:**
```
MaxModsPerWeapon = 2
If Prototype:
    ModBonus *= 2
```

**Tooltip:** "Weapon Modification (Rank 3): Can apply 2 mods. Prototype: bonuses doubled."

---

## Modification Types

| Mod | Standard Effect | Prototype Effect |
|-----|-----------------|------------------|
| **Elemental** | +2d6 elemental damage | +4d6 elemental damage |
| **Precision** | +2 to hit | +4 to hit |
| **Reinforced** | +100% durability, +10% crit | +200% durability, +20% crit |

---

## Combat Log Examples

- "Weapon Modification: Applying [Elemental - Fire] to Longsword..."
- "Modification complete! Longsword now deals +2d6 Fire damage."
- "Rank 3: Second modification slot available."
- "PROTOTYPE modification: Bonuses doubled!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Scrap-Tinker Overview](../scrap-tinker-overview.md) | Parent specialization |
| [Crafting System](../../../../04-systems/crafting/crafting-overview.md) | Crafting mechanics |
