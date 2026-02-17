---
id: ABILITY-STRANDHOGG-25002
title: "Reaver's Strike"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Reaver's Strike

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 35 Stamina (30 at Rank 2+) |
| **Damage Type** | Physical |
| **Attribute** | FINESSE + MIGHT |
| **Tags** | [Momentum], [Melee] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

A vicious strike that builds your momentum for deadlier attacks. Each hit feeds the tide of violence, preparing you for the killing blow.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- FINESSE-based melee attack dealing Weapon Damage + MIGHT
- Generate 15 Momentum on hit

**Formula:**
```
Caster.Stamina -= 35
Damage = WeaponDamage + MIGHT
Target.TakeDamage(Damage, "Physical")
Caster.Momentum += 15
```

**Tooltip:** "Reaver's Strike (Rank 1): Weapon + MIGHT damage. +15 Momentum. Cost: 35 Stamina"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Cost reduced to 30 Stamina
- Damage: Weapon + MIGHT + 1d6
- Generate 15 Momentum on hit

**Formula:**
```
Caster.Stamina -= 30
Damage = WeaponDamage + MIGHT + Roll(1d6)
Target.TakeDamage(Damage, "Physical")
Caster.Momentum += 15
```

**Tooltip:** "Reaver's Strike (Rank 2): Weapon + MIGHT + 1d6. +15 Momentum. Cost: 30 Stamina"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Cost: 30 Stamina
- Damage: Weapon + MIGHT + 2d6
- Generate 15 Momentum on hit
- **NEW:** When hitting debuffed enemy, generate +10 bonus Momentum (25 total)

**Formula:**
```
Caster.Stamina -= 30
Damage = WeaponDamage + MIGHT + Roll(2d6)
Target.TakeDamage(Damage, "Physical")

BaseMomentum = 15
If Target.HasAnyDebuff():
    BonusMomentum = 10
    Log("Debuffed target: +10 bonus Momentum!")
Else:
    BonusMomentum = 0

Caster.Momentum += BaseMomentum + BonusMomentum
```

**Tooltip:** "Reaver's Strike (Rank 3): Weapon + MIGHT + 2d6. +15 Momentum (+25 vs debuffed). Cost: 30 Stamina"

---

## Rank Comparison

| Rank | Cost | Damage | Momentum |
|------|------|--------|----------|
| 1 | 35 | Weapon + MIGHT | +15 |
| 2 | 30 | Weapon + MIGHT + 1d6 | +15 |
| 3 | 30 | Weapon + MIGHT + 2d6 | +15 (+25 vs debuffed) |

---

## Momentum Building Strategy

**Basic Loop:**
1. Reaver's Strike → +15 Momentum
2. Reaver's Strike → +15 Momentum
3. Now at 50+ Momentum → Execute with Savage Harvest

**With Tidal Rush (vs debuffed):**
1. Dread Charge → Apply [Disoriented], +10 Momentum
2. Reaver's Strike → +25 Momentum (debuffed target)
3. Now at 55+ Momentum → Ready for big execute

---

## Combat Log Examples

- "Reaver's Strike: 18 damage to [Enemy]. +15 Momentum (now 45)"
- "Reaver's Strike (Rank 3): 24 damage. Debuffed target: +25 Momentum!"
- "Building momentum... (now 65/100)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Strandhogg Overview](../strandhogg-overview.md) | Parent specialization |
| [Tidal Rush](tidal-rush.md) | Bonus Momentum passive |
| [Savage Harvest](savage-harvest.md) | Momentum spender |
