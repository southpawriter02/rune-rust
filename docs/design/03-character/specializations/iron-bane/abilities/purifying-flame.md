---
id: ABILITY-IRONBANE-1102
title: "Purifying Flame"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Purifying Flame

**Type:** Active | **Tier:** 1 | **PP Cost:** 0 (free with specialization)

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 35 Stamina |
| **Attribute** | WILL |
| **Damage Type** | Fire |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Holy fire cleanses corrupted iron. Your flame burns hotter against the abominations.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Effect:**
- Roll: WILL + 2 bonus dice vs Target Defense
- Damage: 2d8 Fire damage
- Vs Mechanical/Undying: +2d6 bonus Fire damage, +15 Fervor

**Formula:**
```
AttackRoll = Roll(WILL + 2) >= 2 successes
Damage = Roll(2d8)
If (Target.Type == "Mechanical" OR Target.Type == "Undying"):
    Damage += Roll(2d6)
    Fervor += 15
```

**Tooltip:** "Purifying Flame (Rank 1): 2d8 Fire. Vs Mech/Undying: +2d6, +15 Fervor. Cost: 35 Stamina"

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- Bonus vs Mech/Undying: +3d6 damage
- Fervor generation: +20
- Resource cost reduced to 30 Stamina

**Formula:**
```
StaminaCost = 30
Damage = Roll(2d8)
If (Target.Type == "Mechanical" OR Target.Type == "Undying"):
    Damage += Roll(3d6)  // Increased from 2d6
    Fervor += 20  // Increased from 15
```

**Tooltip:** "Purifying Flame (Rank 2): Vs Mech/Undying: +3d6, +20 Fervor. Cost: 30 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Bonus vs Mech/Undying: +4d6 damage
- Fervor generation: +25
- **NEW:** Apply [Burning] (1d6 Fire/turn for 3 turns)

**Formula:**
```
StaminaCost = 30
Damage = Roll(2d8)
If (Target.Type == "Mechanical" OR Target.Type == "Undying"):
    Damage += Roll(4d6)  // Increased from 3d6
    Fervor += 25  // Increased from 20
Target.AddStatus("Burning", DamagePerTurn: Roll(1d6), Duration: 3)
```

**Tooltip:** "Purifying Flame (Rank 3): Vs Mech/Undying: +4d6, +25 Fervor. Applies [Burning] 1d6/turn for 3 turns."

---

## Combat Log Examples

- "Purifying Flame deals 14 Fire damage! +15 Fervor (vs Mechanical)"
- "Purifying Flame (Rank 3) deals 22 Fire damage! [Burning] applied for 3 turns."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Iron-Bane Overview](../iron-bane-overview.md) | Parent specialization |
| [Burning](../../../../04-systems/status-effects/bleeding.md) | Applied DoT effect |
