---
id: ABILITY-STRANDHOGG-25006
title: "Vicious Flank"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Vicious Flank

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 40 Stamina + 20-25 Momentum |
| **Cooldown** | 3 turns |
| **Damage Type** | Physical |
| **Attribute** | MIGHT |
| **Tags** | [Momentum], [Execution], [Debuff Exploit] |
| **Ranks** | 2 → 3 |

---

## Description

You exploit every weakness, every distraction. Against enemies already struggling, your strikes become truly vicious—dealing catastrophic damage to those who cannot defend themselves.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Deal 4d10 + MIGHT Physical damage
- If target is debuffed, deal +50% damage
- Cost: 40 Stamina + 20 Momentum

**Formula:**
```
Caster.Stamina -= 40
Caster.Momentum -= 20

BaseDamage = Roll(4d10) + MIGHT

If Target.HasAnyDebuff():
    FinalDamage = BaseDamage * 1.5  // +50%
    Log("Vicious Flank: +50% damage vs debuffed target!")
Else:
    FinalDamage = BaseDamage

Target.TakeDamage(FinalDamage, "Physical")
```

**Tooltip:** "Vicious Flank (Rank 2): 4d10+MIGHT (+50% vs debuffed). Cost: 40 Stamina + 20 Momentum"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Deal 4d10 + MIGHT Physical damage
- If target is debuffed, deal +50% damage
- **NEW:** On kill, refund 10 Momentum
- Cost: 40 Stamina + 25 Momentum (net 15 on kill)

**Formula:**
```
Caster.Stamina -= 40
Caster.Momentum -= 25

BaseDamage = Roll(4d10) + MIGHT

If Target.HasAnyDebuff():
    FinalDamage = BaseDamage * 1.5
Else:
    FinalDamage = BaseDamage

Target.TakeDamage(FinalDamage, "Physical")

If Target.HP <= 0:
    Caster.Momentum += 10
    Log("Kill confirmed! +10 Momentum refund")
```

**Tooltip:** "Vicious Flank (Rank 3): 4d10+MIGHT (+50% vs debuffed). Kill = +10 Momentum refund."

---

## Damage Calculation

**Base Damage (4d10 + MIGHT +3):**
- Average: 22 + 3 = 25 damage

**Vs Debuffed Target (+50%):**
- Average: 25 × 1.5 = 37.5 damage

---

## Qualifying Debuffs

Any negative status effect qualifies:

| Category | Examples |
|----------|----------|
| Control | [Disoriented], [Stunned], [Rooted], [Slowed] |
| Mental | [Feared], [Charmed] |
| DoT | [Bleeding], [Burning], [Poisoned] |
| Other | [Silenced], [Corroded], [Vulnerable] |

---

## Optimal Setup

1. **Dread Charge** → [Disoriented] on target
2. **Vicious Flank** → +50% damage (debuffed)
3. **If kill:** +10 Momentum refund

**Momentum Flow:**
- Start: 50
- Dread Charge: +15 (now 65)
- Vicious Flank: -25 (now 40)
- Kill refund: +10 (now 50)
- Net: 0 Momentum cost for Vicious Flank if kill

---

## Combat Log Examples

- "Vicious Flank: 24 base damage"
- "Target is [Disoriented]: +50% damage! (36 total)"
- "Kill confirmed! +10 Momentum refund (now 50)"
- "Vicious Flank vs healthy target: 24 damage (no bonus)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Strandhogg Overview](../strandhogg-overview.md) | Parent specialization |
| [Dread Charge](dread-charge.md) | Debuff setup |
| [Tidal Rush](tidal-rush.md) | Bonus Momentum vs debuffed |
