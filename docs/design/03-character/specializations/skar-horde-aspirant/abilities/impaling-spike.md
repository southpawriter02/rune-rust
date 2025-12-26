---
id: ABILITY-SKAR-HORDE-29005
title: "Impaling Spike"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Impaling Spike

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 40 Stamina + 25 Savagery |
| **Cooldown** | 3 turns |
| **Required Augment** | Piercing-type |
| **Status Effect** | [Rooted] |
| **Ranks** | 2 → 3 |

---

## Description

You slam your spike through foot, pinning them to the broken earth. They are not going anywhere.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Damage: 2d10 + MIGHT Piercing
- 90% chance to apply [Rooted] for 3 turns
- **Requires Piercing-type augment**

**Formula:**
```
Requires: Augment.Type == "Piercing"
Damage = Roll(2d10) + MIGHT
If Random() < 0.90:
    Target.AddStatus("Rooted", Duration: 3)
```

**Tooltip:** "Impaling Spike (Rank 2): 2d10+MIGHT Piercing. 90% [Rooted] 3 turns. Requires Piercing augment. Cost: 40 Stamina, 25 Savagery"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Root chance: 100% (guaranteed)
- **NEW:** +2 to hit against [Rooted] targets

**Formula:**
```
RootChance = 1.00  // Guaranteed
If Target.HasStatus("Rooted"):
    AttackBonus += 2
```

**Tooltip:** "Impaling Spike (Rank 3): 100% [Rooted]. +2 to hit Rooted targets. Cost: 40 Stamina, 25 Savagery"

---

## [Rooted] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 3 turns |
| **Effects** | Cannot move |
| | Cannot use movement abilities |
| | -2 to Dodge/Defense rolls |

---

## Augment Requirement

This ability is **disabled** if a Piercing-type augment is not equipped:
- Piercing Spike: ✓ Works
- Blunt Piston: ✗ Cannot use
- Slashing Blade: ✗ Cannot use
- Flame Emitter: ✗ Cannot use

---

## Combat Log Examples

- "Impaling Spike: 16 Piercing damage!"
- "[Enemy] is Rooted for 3 turns! (90% triggered)"
- "Impaling Spike (Rank 3): Guaranteed root!"
- "+2 to hit vs Rooted target"
- "Cannot use Impaling Spike: Wrong augment type equipped"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skar-Horde Aspirant Overview](../skar-horde-aspirant-overview.md) | Parent specialization |
| [Rooted Status](../../../../04-systems/status-effects/rooted.md) | Status effect details |
