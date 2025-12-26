---
id: ABILITY-VEIDIMADUR-24002
title: "Aimed Shot"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Aimed Shot

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Ranged) |
| **Resource Cost** | 40 Stamina (35 at Rank 2+) |
| **Attribute** | FINESSE |
| **Damage Type** | Physical |
| **Ranks** | 1 → 2 → 3 |

---

## Description

A precise shot that rewards patience and accuracy. At higher ranks, the Hunter can find vital points that cause lasting wounds.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Effect:**
- FINESSE-based ranged attack dealing weapon damage

**Formula:**
```
AttackRoll = Roll(FINESSE dice)
Damage = WeaponDamage
StaminaCost = 40
```

**Tooltip:** "Aimed Shot (Rank 1): FINESSE ranged attack dealing weapon damage. Cost: 40 Stamina"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Cost reduced to 35 Stamina
- Damage increased by +1d6

**Formula:**
```
Damage = WeaponDamage + Roll(1d6)
StaminaCost = 35
```

**Tooltip:** "Aimed Shot (Rank 2): Weapon damage +1d6. Cost: 35 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Damage +2d6 total
- **NEW:** On critical hit, apply [Bleeding] for 2 turns (1d6/turn)

**Formula:**
```
Damage = WeaponDamage + Roll(2d6)
StaminaCost = 35
If CriticalHit:
    Target.AddStatus("Bleeding", DamagePerTurn: Roll(1d6), Duration: 2)
```

**Tooltip:** "Aimed Shot (Rank 3): Weapon damage +2d6. Critical hits apply [Bleeding] 1d6/turn for 2 turns. Cost: 35 Stamina"

---

## Combat Log Examples

- "Aimed Shot deals 12 damage!"
- "Aimed Shot (Rank 2) deals 16 damage! (+1d6)"
- "CRITICAL! Aimed Shot (Rank 3) deals 24 damage! [Bleeding] applied for 2 turns."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Veiðimaðr Overview](../veidimadur-overview.md) | Parent specialization |
| [Bleeding](../../../../04-systems/status-effects/bleeding.md) | Critical hit effect |
