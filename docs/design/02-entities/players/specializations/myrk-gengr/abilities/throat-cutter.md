---
id: ABILITY-MYRK-GENGR-24013
title: "Throat-Cutter"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Throat-Cutter

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 45 Stamina |
| **Cooldown** | 3 turns |
| **Damage Type** | Physical |
| **Status Effect** | [Silenced], [Bleeding] |
| **Attribute** | FINESSE |
| **Tags** | [Melee], [Control] |
| **Ranks** | 2 → 3 |

---

## Description

You strike from behind with lethal precision, severing vocal cords. Their screams are silenced before they can escape. Casters and commanders fear this strike above all others.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Melee attack dealing Weapon Damage + 2d8
- If attacking from flanking position OR [Hidden] state: apply [Silenced] for 1 turn
- [Silenced]: Cannot use vocal abilities, spells, or call for reinforcements

**Formula:**
```
Damage = WeaponDamage + Roll(2d8) + FINESSE
Target.TakeDamage(Damage, "Physical")

If Caster.HasStatus("Hidden") OR Caster.IsFlanking(Target):
    Target.AddStatus("Silenced", Duration: 1)
    Log("{Target} is [Silenced]!")
```

**Tooltip:** "Throat-Cutter (Rank 2): Weapon + 2d8. [Silenced] 1 turn from flank/Hidden. Cost: 45 Stamina"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Bonus damage increases to +4d8
- [Silenced] duration increases to 2 turns
- **NEW:** If target had [Feared] when hit, also apply [Bleeding] (2d6/turn, 3 turns)

**Formula:**
```
Damage = WeaponDamage + Roll(4d8) + FINESSE
Target.TakeDamage(Damage, "Physical")

If Caster.HasStatus("Hidden") OR Caster.IsFlanking(Target):
    Target.AddStatus("Silenced", Duration: 2)

    If Target.HasStatus("Feared"):
        Target.AddStatus("Bleeding", Duration: 3, DamagePerTurn: "2d6")
        Log("{Target} bleeds from the Feared strike!")
```

**Tooltip:** "Throat-Cutter (Rank 3): Weapon + 4d8. [Silenced] 2 turns. Feared targets also [Bleed]."

---

## Damage Calculation Example

**Rank 3, Dagger (2d6+2), FINESSE +3:**
```
Damage = (2d6+2) + 4d8 + 3
Average = 9 + 18 + 3 = 30 damage
```

---

## [Silenced] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 1-2 turns |
| **Effect** | Cannot use vocal abilities |
| **Blocks** | Spells requiring incantation |
| **Blocks** | Calling for reinforcements |
| **Blocks** | Command abilities |

---

## Terror from the Void Combo

1. Enter the Void → [Hidden]
2. Shadow Strike → Guaranteed crit + Terror from the Void ([Feared])
3. Next turn: Throat-Cutter on [Feared] target → [Silenced] + [Bleeding]

This combo inflicts:
- Critical damage
- +15 Psychic Stress
- [Feared] 3 turns
- [Silenced] 2 turns
- [Bleeding] 3 turns

---

## Combat Log Examples

- "[THROAT-CUTTER]"
- "{Character} silences {Target}!"
- "Damage: 28"
- "[Silenced] applied for 2 turns"
- "[Bleeding] applied—target was Feared"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Myrk-Gengr Overview](../myrk-gengr-overview.md) | Parent specialization |
| [Terror from the Void](terror-from-the-void.md) | [Feared] source |
| [Silenced Status](../../../../04-systems/status-effects/silenced.md) | Status effect |
