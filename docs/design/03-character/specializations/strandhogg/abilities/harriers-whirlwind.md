---
id: ABILITY-STRANDHOGG-25005
title: "Harrier's Whirlwind"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Harrier's Whirlwind

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 40-45 Stamina + 30 Momentum |
| **Cooldown** | 2 turns |
| **Damage Type** | Physical |
| **Attribute** | MIGHT |
| **Special** | Free reposition after attack |
| **Tags** | [Momentum], [Mobility] |
| **Ranks** | 2 → 3 |

---

## Description

You become a whirlwind of violence—strike and vanish before they can retaliate. The perfect expression of hit-and-run warfare, leaving enemies swinging at empty air.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Deal 4d10 + MIGHT Physical damage
- After attacking, immediately move to any valid position
- Free movement costs 0 Stamina
- Free movement generates 5 Momentum
- Total cost: 40 Stamina + 30 Momentum

**Formula:**
```
Caster.Stamina -= 40
Caster.Momentum -= 30

Damage = Roll(4d10) + MIGHT
Target.TakeDamage(Damage, "Physical")

// Free reposition
Caster.Position = ChosenValidPosition
Caster.Momentum += 5
Log("Harrier's Whirlwind: Free reposition! +5 Momentum")
```

**Tooltip:** "Harrier's Whirlwind (Rank 2): 4d10+MIGHT, free move after (+5 Momentum). Cost: 40 Stamina + 30 Momentum"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Deal 4d10 + MIGHT Physical damage
- Free reposition after attack
- **UPGRADE:** Free move generates 10 Momentum (doubled)
- Total cost: 40 Stamina + 30 Momentum
- Net Momentum cost: 20 (30 spent, 10 regained)

**Formula:**
```
Caster.Stamina -= 40
Caster.Momentum -= 30

Damage = Roll(4d10) + MIGHT
Target.TakeDamage(Damage, "Physical")

Caster.Position = ChosenValidPosition
Caster.Momentum += 10
Log("Harrier's Whirlwind: Free reposition! +10 Momentum")
```

**Tooltip:** "Harrier's Whirlwind (Rank 3): 4d10+MIGHT, free move after (+10 Momentum). Net cost: 20 Momentum"

---

## Net Momentum Cost

| Rank | Spent | Regained | Net Cost |
|------|-------|----------|----------|
| 2 | 30 | 5 | 25 |
| 3 | 30 | 10 | 20 |

---

## Free Reposition Rules

| Property | Value |
|----------|-------|
| Movement Cost | 0 Stamina |
| Valid Positions | Any unoccupied tile |
| Timing | Immediately after damage |
| Enemy Reaction | None (free action) |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Hit-and-run | Strike then retreat to safety |
| Flanking | Move to flanking position after hit |
| Target switching | Reposition to reach another enemy |
| Escape | Attack then move out of melee |

---

## Example Combat Flow

**Turn 1:**
1. Start in Back Row
2. Dread Charge → Move to Front, attack, +15 Momentum
3. End in Front Row (dangerous)

**Turn 2:**
1. Harrier's Whirlwind (spend 30 Momentum)
2. Attack from Front Row
3. Free move back to Back Row (+10 Momentum)
4. Safe from melee retaliation

---

## Combat Log Examples

- "Harrier's Whirlwind: 28 damage to [Enemy]!"
- "Free reposition: Moving to Back Row"
- "+10 Momentum from movement (Rank 3)"
- "Net Momentum: -20 (30 spent, 10 regained)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Strandhogg Overview](../strandhogg-overview.md) | Parent specialization |
| [No Quarter](no-quarter.md) | Free move on kill |
| [Tidal Rush](tidal-rush.md) | Bonus Momentum |
