---
id: ABILITY-STRANDHOGG-25003
title: "Dread Charge"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Dread Charge

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 40 Stamina |
| **Cooldown** | 3 turns |
| **Damage Type** | Physical |
| **Attribute** | MIGHT |
| **Status Effect** | [Disoriented] |
| **Tags** | [Momentum], [Charge], [Control] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You launch yourself across the battlefield in a blur of violence, crashing into your target with disorienting force. The psychological impact leaves them reeling, unable to process your impossible speed.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Move from Back Row to Front Row, then attack
- Deal 2d10 + MIGHT Physical damage
- Apply [Disoriented] for 1 turn
- Inflict 10 Psychic Stress on target
- Generate 10 Momentum

**Formula:**
```
Caster.Position = FrontRow
Damage = Roll(2d10) + MIGHT
Target.TakeDamage(Damage, "Physical")
Target.AddStatus("Disoriented", Duration: 1)
Target.PsychicStress += 10
Caster.Momentum += 10
```

**Tooltip:** "Dread Charge (Rank 1): Move to Front, 2d10+MIGHT, [Disoriented] 1 turn, +10 Stress, +10 Momentum. Cost: 40 Stamina"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Move from Back Row to Front Row, then attack
- Deal 3d10 + MIGHT Physical damage
- Apply [Disoriented] for 2 turns
- Inflict 10 Psychic Stress
- Generate 15 Momentum

**Formula:**
```
Caster.Position = FrontRow
Damage = Roll(3d10) + MIGHT
Target.TakeDamage(Damage, "Physical")
Target.AddStatus("Disoriented", Duration: 2)
Target.PsychicStress += 10
Caster.Momentum += 15
```

**Tooltip:** "Dread Charge (Rank 2): 3d10+MIGHT, [Disoriented] 2 turns, +15 Momentum. Cost: 40 Stamina"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- **NEW:** Can charge from Front Row into enemy Back Row
- Deal 3d10 + MIGHT Physical damage
- Apply [Disoriented] for 2 turns
- Inflict 10 Psychic Stress
- Generate 15 Momentum

**Formula:**
```
// Can now charge in either direction
If Caster.Position == BackRow:
    Caster.Position = FrontRow
Else:  // From Front Row
    Caster.Position = EnemyBackRow  // Penetrating charge

Damage = Roll(3d10) + MIGHT
Target.TakeDamage(Damage, "Physical")
Target.AddStatus("Disoriented", Duration: 2)
Target.PsychicStress += 10
Caster.Momentum += 15
```

**Tooltip:** "Dread Charge (Rank 3): Charge in any direction! 3d10+MIGHT, [Disoriented] 2 turns. Cost: 40 Stamina"

---

## Rank Comparison

| Rank | Damage | [Disoriented] | Momentum | Special |
|------|--------|---------------|----------|---------|
| 1 | 2d10 + MIGHT | 1 turn | +10 | Back → Front only |
| 2 | 3d10 + MIGHT | 2 turns | +15 | Back → Front only |
| 3 | 3d10 + MIGHT | 2 turns | +15 | Any direction |

---

## [Disoriented] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 1-2 turns |
| **Effect** | -2 dice to Accuracy |
| **Complex Abilities** | Cannot use |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Opener | First turn to apply debuff |
| Back-line dive | Rank 3: Reach healers/casters |
| Debuff setup | Enables Tidal Rush + Vicious Flank |
| Momentum boost | Quick +15 generation |

---

## Tidal Rush Combo

1. Dread Charge → [Disoriented] + 15 Momentum
2. Target now debuffed
3. Reaver's Strike → +25 Momentum (Tidal Rush bonus)
4. Vicious Flank → +50% damage vs debuffed

---

## Combat Log Examples

- "DREAD CHARGE! [Character] blurs across the battlefield!"
- "[Enemy] takes 22 damage and is [Disoriented] for 2 turns!"
- "[Enemy] suffers +10 Psychic Stress from the sudden assault"
- "+15 Momentum generated (now 45/100)"
- "Rank 3: Charging into enemy Back Row!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Strandhogg Overview](../strandhogg-overview.md) | Parent specialization |
| [Tidal Rush](tidal-rush.md) | Debuff synergy |
| [Vicious Flank](vicious-flank.md) | Debuff damage bonus |
| [Disoriented Status](../../../../04-systems/status-effects/disoriented.md) | Status effect |
