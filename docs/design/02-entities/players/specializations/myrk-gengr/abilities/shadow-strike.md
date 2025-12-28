---
id: ABILITY-MYRK-GENGR-24012
title: "Shadow Strike"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Shadow Strike

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 35 Stamina |
| **Requirement** | Must be in [Hidden] state |
| **Damage Type** | Physical |
| **Attribute** | FINESSE |
| **Tags** | [Stealth], [Assassination], [Critical] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

A precise, brutal attack from a blind spot. The blade finds its mark before their corrupted processors can register the threat. Every Shadow Strike is a critical hit—guaranteed.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Requires [Hidden] state
- FINESSE-based melee attack
- **GUARANTEED CRITICAL HIT** (damage doubled)
- Immediately breaks stealth (unless Ghostly Form procs)

**Formula:**
```
If NOT Caster.HasStatus("Hidden"):
    Fail("Shadow Strike requires [Hidden]")
    return

BaseDamage = WeaponDamage + FINESSE
CriticalDamage = BaseDamage * 2

Target.TakeDamage(CriticalDamage, "Physical")
Caster.RemoveStatus("Hidden")  // Unless Ghostly Form
```

**Tooltip:** "Shadow Strike (Rank 1): Guaranteed critical (2× damage) from [Hidden]. Cost: 35 Stamina"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- All Rank 1 effects
- +2d6 bonus damage (before doubling)
- If attack kills target, refund 20 Stamina

**Formula:**
```
BaseDamage = WeaponDamage + FINESSE + Roll(2d6)
CriticalDamage = BaseDamage * 2

Target.TakeDamage(CriticalDamage, "Physical")

If Target.HP <= 0:
    Caster.Stamina += 20
    Log("Kill confirmed: +20 Stamina refunded!")
```

**Tooltip:** "Shadow Strike (Rank 2): 2× (Weapon + 2d6). Kill = +20 Stamina. Cost: 35 Stamina"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- All Rank 2 effects
- Bonus damage increases to +4d6 (before doubling)
- Apply [Bleeding] for 2 turns (2d6 damage/turn)

**Formula:**
```
BaseDamage = WeaponDamage + FINESSE + Roll(4d6)
CriticalDamage = BaseDamage * 2

Target.TakeDamage(CriticalDamage, "Physical")
Target.AddStatus("Bleeding", Duration: 2, DamagePerTurn: "2d6")

If Target.HP <= 0:
    Caster.Stamina += 20
```

**Tooltip:** "Shadow Strike (Rank 3): 2× (Weapon + 4d6) + [Bleeding] 2 turns. Kill = +20 Stamina."

---

## Damage Calculation Example

**Rank 3, Dagger (2d6+2), FINESSE +3:**
```
Base = (2d6+2) + 3 + 4d6 = 6d6 + 5
Average Base = 21 + 5 = 26
Critical = 26 × 2 = 52 damage average
Plus 2d6 Bleeding/turn for 2 turns = +14 total
TOTAL: ~66 damage from single strike
```

---

## [Bleeding] Status Effect (Rank 3)

| Property | Value |
|----------|-------|
| **Duration** | 2 turns |
| **Damage** | 2d6 Physical per turn |
| **Removal** | Healing, bandaging |

---

## Integration with Other Abilities

| Ability | Interaction |
|---------|-------------|
| Terror from the Void | First Shadow Strike triggers +15 Stress, [Feared] |
| Ghostly Form | 65% chance to stay [Hidden] after strike |
| Throat-Cutter | Alternative melee for [Silenced] |

---

## Combat Log Examples

- "[SHADOW STRIKE - CRITICAL HIT]"
- "{Character} strikes from the void!"
- "Damage: 52 (doubled from critical)"
- "[Bleeding] applied for 2 turns"
- "Ghostly Form: Stealth maintained! (65% success)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Myrk-Gengr Overview](../myrk-gengr-overview.md) | Parent specialization |
| [Enter the Void](enter-the-void.md) | Enter [Hidden] state |
| [Terror from the Void](terror-from-the-void.md) | First strike bonus |
| [Ghostly Form](ghostly-form.md) | Stealth persistence |
