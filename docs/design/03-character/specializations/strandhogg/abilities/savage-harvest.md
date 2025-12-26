---
id: ABILITY-STRANDHOGG-25008
title: "Savage Harvest"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Savage Harvest

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 50 Stamina + 40 Momentum |
| **Cooldown** | 4 turns |
| **Damage Type** | Physical |
| **Attribute** | MIGHT |
| **Tags** | [Momentum], [Execution], [Sustain] |
| **Ranks** | None (full power when unlocked) |

---

## Description

The ultimate expression of kinetic violence. You gather all your momentum into a single, devastating strike that reaps both life and resources from your victim.

---

## Mechanical Effect

**Execution Strike:**
- Deal 10d10 + MIGHT Physical damage
- If this kills the target:
  - Refund 20 Stamina
  - Refund 20 Momentum
  - Heal for 20% of max HP

**Formula:**
```
Caster.Stamina -= 50
Caster.Momentum -= 40

Damage = Roll(10d10) + MIGHT
Target.TakeDamage(Damage, "Physical")

If Target.HP <= 0:
    Caster.Stamina += 20
    Caster.Momentum += 20
    HealAmount = Caster.MaxHP * 0.20
    Caster.HP += HealAmount
    Log("SAVAGE HARVEST! Kill confirmed!")
    Log("+20 Stamina, +20 Momentum, +{HealAmount} HP")
```

**Tooltip:** "Savage Harvest: 10d10+MIGHT. Kill: +20 Stamina, +20 Momentum, +20% HP. Cost: 50 Stamina + 40 Momentum"

---

## Damage Output

| Metric | Value |
|--------|-------|
| Base Dice | 10d10 |
| Average (MIGHT +3) | 55 + 3 = 58 |
| Minimum | 13 |
| Maximum | 103 |

---

## Kill Refund Economics

| Resource | Spent | Refunded | Net Cost |
|----------|-------|----------|----------|
| Stamina | 50 | 20 | 30 |
| Momentum | 40 | 20 | 20 |
| HP | 0 | +20% max | Gain |

**If kill:** This becomes highly efficient.

---

## Combo with No Quarter

If Savage Harvest kills:
1. Savage Harvest: +20 Stamina, +20 Momentum, +20% HP
2. No Quarter triggers: Free move, +10 Momentum, +15 TempHP

**Total on kill:**
- +20 Stamina
- +30 Momentum (20 + 10)
- +20% HP + 15 TempHP
- Free reposition

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Finishing blow | Secure kill for refunds |
| Sustain | Heal while fighting |
| Momentum recovery | Refund enables follow-up |
| Priority target | Delete dangerous enemy |

---

## Optimal Target Selection

**Best Targets:**
- Enemies below 60 HP (guaranteed kill range)
- Already debuffed enemies (Tidal Rush bonus on approach)
- Isolated targets (no retaliation from allies)

**Avoid:**
- Healthy bosses (waste if no kill)
- Targets that might survive

---

## Combat Log Examples

- "SAVAGE HARVEST! 67 massive damage!"
- "Kill confirmed on [Enemy]!"
- "+20 Stamina refund (now 45/100)"
- "+20 Momentum refund (now 50/100)"
- "+18 HP healed (20% of max)"
- "No Quarter: Free move + 10 Momentum + 15 TempHP"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Strandhogg Overview](../strandhogg-overview.md) | Parent specialization |
| [No Quarter](no-quarter.md) | Kill synergy |
| [Riptide of Carnage](riptide-of-carnage.md) | Capstone execution |
