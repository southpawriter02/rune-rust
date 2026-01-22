---
id: ABILITY-HLEKKR-25011
title: "Netting Shot"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Netting Shot

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (1-2 at Rank 2+) |
| **Resource Cost** | 20 Stamina (15 at Rank 3) |
| **Cooldown** | 2 turns |
| **Damage Type** | Physical |
| **Status Effect** | [Rooted], [Slowed] |
| **Tags** | [Control], [Ranged] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You launch a weighted net that entangles your target, rooting them in place. Against corrupted enemies, the net's chains dig deeper, also slowing their movements.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Deal 1d6 Physical damage
- Apply [Rooted] for 2 turns (+1 with Pragmatic Preparation = 3)

**Formula:**
```
Damage = Roll(1d6)
Target.TakeDamage(Damage, "Physical")
Target.AddStatus("Rooted", Duration: 2 + PragmaticBonus)
```

**Tooltip:** "Netting Shot (Rank 1): 1d6 Physical. [Rooted] 2 turns. Cost: 20 Stamina"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Deal 1d6 Physical damage to up to 2 targets (split net)
- Apply [Rooted] for 3 turns (+1 with PP = 4)

**Formula:**
```
For each Target (max 2):
    Damage = Roll(1d6)
    Target.TakeDamage(Damage, "Physical")
    Target.AddStatus("Rooted", Duration: 3 + PragmaticBonus)
```

**Tooltip:** "Netting Shot (Rank 2): 1d6 Physical to 2 targets. [Rooted] 3 turns. Cost: 20 Stamina"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Stamina cost reduced to 15
- Deal 1d6 Physical damage to up to 2 targets
- Apply [Rooted] for 3 turns (+2 with PP Rank 3 = 5)
- **NEW:** Against 60+ Corruption enemies, also apply [Slowed] for 2 turns

**Formula:**
```
Caster.Stamina -= 15  // Reduced from 20

For each Target (max 2):
    Damage = Roll(1d6)
    Target.TakeDamage(Damage, "Physical")
    Target.AddStatus("Rooted", Duration: 3 + PragmaticBonus)

    If Target.Corruption >= 60:
        Target.AddStatus("Slowed", Duration: 2)
        Log("Corruption exploitation: {Target} also Slowed!")
```

**Tooltip:** "Netting Shot (Rank 3): 1d6 to 2 targets. [Rooted] 3 turns. High corruption: +[Slowed]. Cost: 15 Stamina"

---

## Duration Summary (with Pragmatic Preparation)

| Rank | PP Rank | [Rooted] Duration |
|------|---------|-------------------|
| 1 | 1 | 3 turns |
| 2 | 2 | 4 turns |
| 3 | 3 | 5 turns |

---

## [Rooted] Status Effect

| Property | Value |
|----------|-------|
| **Effect** | Cannot move |
| **Actions** | Can still attack normally |
| **Removal** | Duration or ally assistance |

---

## Combat Log Examples

- "Netting Shot: [Enemy] takes 4 damage and is [Rooted] for 3 turns"
- "Netting Shot (Rank 2): Net splits to 2 targets!"
- "Corruption exploitation: [Corrupted Enemy] also [Slowed] for 2 turns!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Hlekkr-Master Overview](../hlekkr-master-overview.md) | Parent specialization |
| [Pragmatic Preparation I](pragmatic-preparation-i.md) | Duration bonus |
| [Rooted Status](../../../../04-systems/status-effects/rooted.md) | Status effect |
| [Punish the Helpless](punish-the-helpless.md) | Damage multiplier |
