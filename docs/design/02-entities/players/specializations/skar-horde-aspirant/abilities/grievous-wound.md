---
id: ABILITY-SKAR-HORDE-29004
title: "Grievous Wound"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Grievous Wound

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 45 Stamina + 30 Savagery |
| **Cooldown** | 2 turns |
| **Status Effect** | [Grievous Wound] |
| **Ranks** | 2 → 3 |

---

## Description

You carve a wound that armor cannot protect against. A wound that does not close. A wound that reminds them what mortality means.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Damage: 3d8 + MIGHT Physical
- Applies [Grievous Wound]: 1d10 damage/turn for 3 turns
- **[Grievous Wound] bypasses ALL Soak**

**Formula:**
```
Damage = Roll(3d8) + MIGHT
Target.AddStatus("GrievousWound",
    DamagePerTurn: Roll(1d10),
    Duration: 3,
    BypassesSoak: true)
```

**Tooltip:** "Grievous Wound (Rank 2): 3d8+MIGHT + [Grievous Wound] (1d10/turn, ignores Soak, 3 turns). Cost: 45 Stamina, 30 Savagery"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Damage: 4d8 + MIGHT Physical
- [Grievous Wound] duration: 4 turns
- DoT damage: 1d12/turn
- **NEW:** If target dies from [Grievous Wound], refund 20 Savagery

**Formula:**
```
Damage = Roll(4d8) + MIGHT
Target.AddStatus("GrievousWound",
    DamagePerTurn: Roll(1d12),
    Duration: 4,
    BypassesSoak: true)

OnTargetDeath (from GrievousWound):
    Savagery += 20
```

**Tooltip:** "Grievous Wound (Rank 3): 4d8+MIGHT + [Grievous Wound] (1d12/turn, 4 turns). +20 Savagery if kill. Cost: 45 Stamina, 30 Savagery"

---

## [Grievous Wound] Status Effect

| Property | Rank 2 | Rank 3 |
|----------|--------|--------|
| Duration | 3 turns | 4 turns |
| Damage/turn | 1d10 | 1d12 |
| Soak Bypass | Yes | Yes |
| Kill Refund | — | +20 Savagery |

---

## Combat Log Examples

- "Grievous Wound: 18 damage! [Grievous Wound] applied!"
- "[Grievous Wound] tick: [Enemy] takes 7 damage (bypasses Soak)"
- "[Enemy] dies from Grievous Wound! +20 Savagery refunded"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skar-Horde Aspirant Overview](../skar-horde-aspirant-overview.md) | Parent specialization |
