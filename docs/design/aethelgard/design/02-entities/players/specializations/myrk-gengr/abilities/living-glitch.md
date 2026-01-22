---
id: ABILITY-MYRK-GENGR-24018
title: "Living Glitch"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Living Glitch

**Type:** Active (Capstone) | **Tier:** 4 | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 60 Stamina + 75 Focus |
| **Requirement** | Must be in [Hidden] state |
| **Self-Corruption** | +15 |
| **Cooldown** | Once Per Combat |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |
| **Tags** | [Capstone], [Assassination], [Guaranteed Hit] |
| **Ranks** | None (full power when unlocked) |

---

## Description

For a single, horrifying moment, you do not hide in a glitch—you become one. You de-compile your own physical presence, stepping outside the world's logical grid to deliver a blow that is a fundamental violation of causality.

---

## Mechanical Effect

**Requirements:**
- Must be in [Hidden] state
- Must have 60+ Stamina
- Must have 75+ Focus
- Once per combat limitation

**Attack:**
- GUARANTEED HIT—cannot be parried, dodged, or blocked
- Ignores all defensive abilities and reactions
- Deals 10d10 + Weapon Damage + (FINESSE modifier × 2)

**Psychic Effect:**
- Target suffers 30 Psychic Stress
- Separate from Terror from the Void (stacks if both apply)

**Self-Corruption:**
- You gain 15 Corruption
- The cost of violating causality

**Stealth Resolution:**
- Normally breaks [Hidden] state
- If attack kills target, maintain [Hidden] state

**Formula:**
```
If NOT Caster.HasStatus("Hidden"):
    Fail("Living Glitch requires [Hidden]")
    return

Caster.Stamina -= 60
Caster.Focus -= 75

// Guaranteed hit - no defense possible
Damage = Roll(10d10) + WeaponDamage + (FINESSE * 2)
Target.TakeDamage(Damage, "Physical", IgnoreDefense: true)

Target.PsychicStress += 30

Caster.Corruption += 15

If Target.HP <= 0:
    // Keep Hidden
    Log("Target eliminated—Stealth maintained!")
Else:
    Caster.RemoveStatus("Hidden")
```

**Tooltip:** "Living Glitch: 10d10 + Weapon + FINESSE×2. GUARANTEED HIT. +30 Stress to target. +15 Self-Corruption. Once/combat."

---

## Damage Calculation

**Dagger (2d6+2), FINESSE +4:**
```
Damage = 10d10 + (2d6+2) + (4 × 2)
       = 10d10 + 2d6 + 10
Average = 55 + 7 + 10 = 72 damage

This is GUARANTEED - no miss chance, no defense
```

---

## Psychic Stress Impact

| Effect | Stress |
|--------|--------|
| Terror from the Void | +15 |
| Living Glitch | +30 |
| **Combined First Strike** | **+45** |

+45 Psychic Stress can trigger Breaking Points in most enemies.

---

## Self-Corruption Cost

| Corruption | Effect |
|------------|--------|
| +15 per use | Pushes toward Heretical threshold |
| Cumulative | Multiple uses over campaign stack |
| Trade-off | Raw power vs. corruption risk |

---

## Maximum First Strike Combo

If Living Glitch is your first attack from stealth:
1. Living Glitch: 72 avg damage (guaranteed)
2. Terror from the Void: +15 Psychic Stress
3. Living Glitch: +30 Psychic Stress
4. [Feared] 3 turns (85% chance)

**Total:**
- 72 Physical damage (unavoidable)
- 45 Psychic Stress
- [Feared] 3 turns
- +15 Self-Corruption

---

## GUI Display - Capstone Notification

```
┌────────────────────────────────────────────┐
│ ⚡ LIVING GLITCH                 [CAPSTONE] │
│ ────────────────────────────────────────── │
│ Cost: 60 Stamina + 75 Focus                │
│ Self-Corruption: +15                       │
│ ────────────────────────────────────────── │
│ GUARANTEED HIT (Unblockable)               │
│ Damage: 10d10 + Weapon + FINESSE × 2       │
│ Target Stress: +30                         │
│ ────────────────────────────────────────── │
│ ⚠️ Requires [Hidden]                       │
│ ⚠️ Once per combat                         │
│ ⚠️ +15 Corruption to self                  │
└────────────────────────────────────────────┘
```

---

## Combat Log Example

```
═══════════════════════════════════════════
        ⚡ LIVING GLITCH ⚡
═══════════════════════════════════════════
{Character} becomes a violation of causality!
GUARANTEED HIT - UNBLOCKABLE
Damage: 78 (10d10=56, Weapon=14, FINESSE×2=8)
{Target} suffers 30 Psychic Stress!

⚠️ {Character} gains 15 Corruption
[Target Eliminated - Stealth Maintained]
═══════════════════════════════════════════
```

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Myrk-Gengr Overview](../myrk-gengr-overview.md) | Parent specialization |
| [Terror from the Void](terror-from-the-void.md) | First strike synergy |
| [Coherence Resource](../../../../01-core/resources/coherence.md) | Corruption mechanics |
| [Stress System](../../../../01-core/resources/stress.md) | Psychic Stress |
