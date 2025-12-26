---
id: ABILITY-ECHO-CALLER-28018
title: "Silence Made Weapon"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Silence Made Weapon

**Type:** Active [Echo] (Capstone) | **Tier:** 4 | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | ALL Enemies |
| **Resource Cost** | 60 Aether + 15 Psychic Stress |
| **Damage Type** | Psychic |
| **Cooldown** | Twice per combat |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |
| **Tags** | [Echo] |
| **Ranks** | None (full power when unlocked) |

---

## Description

The Great Silence screams through you. For one terrible moment, you become a conduit for the world's psychic death-scream, weaponizing the eternal silence itself.

---

## Mechanical Effect

**Battlefield-Wide Assault:**
- ALL enemies on the battlefield take 6d10 Psychic damage
- **Scaling:** +2d10 damage for each [Feared] or [Disoriented] enemy (max +12d10)
- All enemies make WILL Resolve (DC 18) or become [Feared] for 2 turns
- **COST:** 60 Aether + 15 Psychic Stress
- Can be used twice per combat at full power

**Formula:**
```
BaseDamage = Roll(6d10)

// Count debuffed enemies
DebuffedCount = 0
For each Enemy:
    If Enemy.HasStatus("Feared") OR Enemy.HasStatus("Disoriented"):
        DebuffedCount += 1

BonusDice = Min(DebuffedCount * 2, 12)
TotalDamage = BaseDamage + Roll(BonusDice * d10)

For each Enemy:
    Enemy.TakeDamage(TotalDamage, "Psychic")
    If Enemy.WILLResolve < 18:
        Enemy.AddStatus("Feared", Duration: 2)

Caster.PsychicStress += 15
```

**Tooltip:** "Silence Made Weapon: 6d10 Psychic to ALL enemies. +2d10 per Feared/Disoriented (max +12d10). DC 18 or Feared. Cost: 60 Aether + 15 Stress. 2×/combat."

---

## Damage Scaling Examples

| Debuffed Enemies | Base | Bonus | Total Dice |
|------------------|------|-------|------------|
| 0 | 6d10 | 0 | 6d10 (avg 33) |
| 2 | 6d10 | 4d10 | 10d10 (avg 55) |
| 4 | 6d10 | 8d10 | 14d10 (avg 77) |
| 6+ | 6d10 | 12d10 | 18d10 (avg 99) |

**Maximum Potential:** 18d10 = ~99 average damage to every enemy

---

## Optimal Setup

1. **Turn 1-2:** Apply [Feared] and [Disoriented] to multiple enemies
2. **Turn 3:** Silence Made Weapon with maximum scaling
3. **Result:** Devastating AoE + mass Fear application

---

## Combat Log Examples

- "SILENCE MADE WEAPON! The Great Silence screams through you!"
- "3 enemies are Feared/Disoriented: +6d10 bonus damage!"
- "12d10 Psychic damage to ALL 5 enemies!"
- "[Enemy A]: 68 damage, [Feared]!"
- "[Enemy B]: 68 damage, resists Fear"
- "+15 Psychic Stress (Silence Made Weapon cost)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Echo-Caller Overview](../echo-caller-overview.md) | Parent specialization |
| [Fear Cascade](fear-cascade.md) | Setup ability |
| [Reality Fracture](reality-fracture.md) | Setup ability |
