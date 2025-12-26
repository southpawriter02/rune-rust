---
id: ABILITY-EINBUI-27012
title: "Basic Concoction"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Basic Concoction

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Self (creates item) |
| **Resource Cost** | 10 Stamina + 1 [Common Herb] + 1 [Clean Cloth] |
| **Tags** | [Crafting], [Healing] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

With basic materials and field knowledge, you create healing poultices or energizing stimulants. Crude but effective—the difference between survival and death in the wasteland.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Create ONE of:
  - [Crude Poultice]: Heal 2d6 HP when used
  - [Weak Stimulant]: Restore 15 Stamina when used
- Maximum 3 field-crafted items in inventory

**Formula:**
```
Caster.Stamina -= 10
Caster.Inventory.Remove("Common Herb", 1)
Caster.Inventory.Remove("Clean Cloth", 1)

If Choice == "Poultice":
    Caster.Inventory.Add("Crude Poultice")
    // On Use: Target.HP += Roll(2d6)
Else:
    Caster.Inventory.Add("Weak Stimulant")
    // On Use: Target.Stamina += 15
```

**Tooltip:** "Basic Concoction (Rank 1): Create Crude Poultice (2d6 HP) or Weak Stimulant (15 Stamina). Max 3."

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Create ONE of:
  - [Refined Poultice]: Heal 4d6 HP when used
  - [Potent Stimulant]: Restore 30 Stamina when used
- Maximum 3 field-crafted items in inventory

**Formula:**
```
Caster.Stamina -= 10
Caster.Inventory.Remove("Common Herb", 1)
Caster.Inventory.Remove("Clean Cloth", 1)

If Choice == "Poultice":
    Caster.Inventory.Add("Refined Poultice")
    // On Use: Target.HP += Roll(4d6)
Else:
    Caster.Inventory.Add("Potent Stimulant")
    // On Use: Target.Stamina += 30
```

**Tooltip:** "Basic Concoction (Rank 2): Create Refined Poultice (4d6 HP) or Potent Stimulant (30 Stamina). Max 3."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Create ONE of:
  - [Superior Poultice]: Heal 6d6 HP + remove [Bleeding] when used
  - [Exceptional Stimulant]: Restore 45 Stamina + remove [Exhausted] when used
- Maximum 3 field-crafted items in inventory

**Formula:**
```
Caster.Stamina -= 10
Caster.Inventory.Remove("Common Herb", 1)
Caster.Inventory.Remove("Clean Cloth", 1)

If Choice == "Poultice":
    Caster.Inventory.Add("Superior Poultice")
    // On Use: Target.HP += Roll(6d6); Target.RemoveStatus("Bleeding")
Else:
    Caster.Inventory.Add("Exceptional Stimulant")
    // On Use: Target.Stamina += 45; Target.RemoveStatus("Exhausted")
```

**Tooltip:** "Basic Concoction (Rank 3): Superior Poultice (6d6 HP + cure Bleeding) or Exceptional Stimulant (45 Stamina + cure Exhausted)."

---

## Crafted Item Summary

| Rank | Poultice | Stimulant |
|------|----------|-----------|
| 1 | 2d6 HP (avg 7) | 15 Stamina |
| 2 | 4d6 HP (avg 14) | 30 Stamina |
| 3 | 6d6 HP (avg 21) + cure Bleeding | 45 Stamina + cure Exhausted |

---

## Material Requirements

| Material | Quantity | Source |
|----------|----------|--------|
| [Common Herb] | 1 | Foraging |
| [Clean Cloth] | 1 | Salvage, loot |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Pre-combat | Stock up on healing |
| Mid-expedition | Restore party resources |
| Emergency | Quick healing without healer |

---

## Combat Log Examples

- "[Einbui] crafts Refined Poultice using Common Herb + Clean Cloth"
- "[Character] uses Superior Poultice: +18 HP, [Bleeding] removed!"
- "Field-crafted item limit reached (3/3)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Einbui Overview](../einbui-overview.md) | Parent specialization |
| [Field Medicine](../../../../04-systems/crafting/field-medicine.md) | Crafting system |
| [Master Improviser](master-improviser.md) | Upgrades this ability |
