---
id: ABILITY-IRONBANE-1106
title: "Flame Ward"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Flame Ward

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Trigger** | When hit by melee attack |
| **Ranks** | 2 → 3 |

---

## Description

You are wreathed in holy flame. The corrupted dare not touch you. Those who try burn.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- 75% Fire resistance
- Mechanical/Undying melee attackers take 1d8 Fire damage
- **NEW:** +2 Soak vs Mechanical/Undying attacks

**Formula:**
```
FireResistance = 0.75
OnMeleeHit:
    If (Attacker.Type == "Mechanical" OR Attacker.Type == "Undying"):
        Attacker.TakeDamage(Roll(1d8), "Fire")
        SoakBonus += 2
```

**Tooltip:** "Flame Ward (Rank 2): 75% Fire resist. Mech/Undying melee attackers take 1d8 Fire. +2 Soak vs Mech/Undying."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Fire immunity (100% resistance)
- Retaliation: 1d10 Fire damage
- **NEW:** Generate +10 Fervor when hit by Mechanical/Undying

**Formula:**
```
FireResistance = 1.00  // Immunity
RetaliationDamage = Roll(1d10)
OnMeleeHitByMechUndying:
    Fervor += 10
```

**Tooltip:** "Flame Ward (Rank 3): Fire IMMUNE. 1d10 retaliation. +10 Fervor when hit by Mech/Undying."

---

## Combat Log Examples

- "Flame Ward: 75% Fire resistance active."
- "[Automaton] attacks and takes 6 Fire retaliation damage!"
- "Flame Ward (Rank 3): +10 Fervor from Mechanical attacker!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Iron-Bane Overview](../iron-bane-overview.md) | Parent specialization |
