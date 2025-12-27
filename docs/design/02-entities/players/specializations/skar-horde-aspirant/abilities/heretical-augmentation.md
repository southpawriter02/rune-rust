---
id: ABILITY-SKAR-HORDE-29001
title: "Heretical Augmentation"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Heretical Augmentation

**Type:** Passive | **Tier:** 1 | **PP Cost:** 0 (free with specialization)

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

You have performed the ritual of replacement, carving away weakness and grafting brutal functionality. Your hand is gone. Your weapon-stump remains.

---

## Rank Progression

### Rank 1 (Starting Rank - When specialization is chosen)

**Effect:**
- Unlocks [Augmentation] equipment slot (replaces weapon slot)
- Enables crafting and installing augments at workbenches
- Augment determines damage type for Skar-Horde abilities

**Formula:**
```
EquipmentSlot.Weapon = DISABLED
EquipmentSlot.Augmentation = ENABLED
DamageType = Augment.DamageType
```

**Tooltip:** "Heretical Augmentation (Rank 1): Unlocks [Augmentation] slot. Craft and install augments at workbenches."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- All Rank 1 effects
- **NEW:** Swap augments in 1 action (normally 2 actions)

**Formula:**
```
AugmentSwapCost = 1 action  // Reduced from 2
```

**Tooltip:** "Heretical Augmentation (Rank 2): Swap augments in 1 action."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- All Rank 2 effects
- **NEW:** All augments gain +1 to all damage dice

**Formula:**
```
AugmentDamage = BaseDamage + 1 per die
// Example: 3d8 becomes 3d8+3
```

**Tooltip:** "Heretical Augmentation (Rank 3): Augments gain +1 to all damage dice."

---

## Available Augment Types

| Augment | Damage Type | Required For |
|---------|-------------|--------------|
| Piercing Spike | Piercing | Impaling Spike |
| Blunt Piston | Bludgeoning | Overcharged Piston Slam |
| Slashing Blade | Slashing | General use |
| Flame Emitter | Fire | Bonus vs organic |

---

## Combat Log Examples

- "Heretical Augmentation: [Augmentation] slot unlocked"
- "Augment equipped: Piercing Spike (Piercing damage)"
- "Heretical Augmentation (Rank 2): Swapping augment (1 action)"
- "Heretical Augmentation (Rank 3): Damage dice enhanced (+1 per die)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skar-Horde Aspirant Overview](../skar-horde-aspirant-overview.md) | Parent specialization |
