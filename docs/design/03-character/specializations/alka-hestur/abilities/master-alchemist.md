---
id: ABILITY-ALKA-HESTUR-29018
title: "Master Alchemist"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Master Alchemist

**Type:** Mixed (Passive + Active) | **Tier:** 4 (Capstone) | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Components** | Universal Reagent (Passive) + Perfect Solution (Active) |
| **PP Cost** | 6 |
| **Requirement** | 24 PP in specialization + any Tier 3 ability |
| **Tags** | [Capstone], [Mastery], [Execution] |
| **Ranks** | None (full power when unlocked) |

---

## Description

*"You have transcended the distinction between alchemist and weapon. Your touch delivers ruin calibrated to molecular precision. Every enemy has a solution—you carry all of them."*

The Master Alchemist capstone represents the pinnacle of combat alchemy. Your understanding of chemical reactions has become instinctive, your payloads more potent, and your ability to craft the perfect counter to any enemy—absolute.

---

## Component A: Universal Reagent (Passive)

### Effect
- All payloads deal +2d8 bonus damage of their element
- All payload status effects last +2 turns baseline
- Identify ALL creature vulnerabilities automatically (no check required)

### Formula
```
// Passive enhancement to all payload abilities
OnPayloadDamage:
    BonusDamage = Roll(2d8)
    TotalDamage = BaseDamage + BonusDamage
    Log("+{BonusDamage} bonus damage (Universal Reagent)")

OnPayloadStatusApply:
    ExtendedDuration = BaseDuration + 2
    Target.ApplyStatus(Status, ExtendedDuration)
    Log("Status duration: {ExtendedDuration} turns (+2 Universal Reagent)")

OnEnemyVisible:
    // Auto-analysis, no check required
    Display(Enemy.Vulnerabilities)
    Display(Enemy.Resistances)
    Log("Universal Reagent: {Enemy} fully analyzed (automatic)")
```

---

## Component B: Perfect Solution (Active)

### Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Melee) |
| **Resource Cost** | 50 Stamina + 1 Payload Charge |
| **Cooldown** | Once per combat |
| **Tags** | [Execution], [Auto-Hit], [Debuff] |

### Effect
- Analyze target and create custom payload specifically designed for their vulnerabilities
- This attack **automatically hits** (no roll required)
- Deal 8d10 damage of target's weakest resistance type
- Apply **[Perfect Exploitation]**: Target takes +100% damage from all sources for 2 turns

### Formula
```
Caster.Stamina -= 50
Caster.PayloadCharges -= 1

// Determine weakest resistance
WeakestElement = Target.GetLowestResistance()

// Auto-hit, custom payload
Damage = Roll(8d10)
Target.TakeDamage(Damage, WeakestElement)

// Apply exploitation debuff
Target.ApplyStatus("[Perfect Exploitation]", {
    Effect: "+100% damage from all sources",
    Duration: 2
})

Log("PERFECT SOLUTION! Custom payload deals {Damage} {WeakestElement} damage!")
Log("[Perfect Exploitation] applied! +100% damage for 2 turns!")
```

**Tooltip:** "Perfect Solution: Auto-hit. 8d10 damage (weakest resistance). +100% damage for 2 turns. Once/combat."

---

## Rank Progression (via continued investment)

### Rank 2 Enhancement

**Universal Reagent:**
- Bonus damage: +3d8 (up from +2d8)
- Enemies struck have -2 to saves vs subsequent payload effects

**Perfect Solution:**
- Damage: 10d10 (up from 8d10)
- [Perfect Exploitation] lasts 3 turns (up from 2)

### Rank 3 Enhancement

**Universal Reagent:**
- Bonus damage: +4d8
- Once per combat, any payload can apply its effect TWICE

**Perfect Solution:**
- Damage: 12d10
- If target is killed, recover ALL expended payload charges

---

## [Perfect Exploitation] Status Effect

| Property | Value |
|----------|-------|
| Duration | 2-3 turns |
| Effect | +100% damage from all sources |
| Type | Debuff |
| Removal | Cannot be cleansed |

**Party Coordination:**
When Perfect Exploitation is active:
- All damage dealt to target is DOUBLED
- Applies to all party members' attacks
- Applies to DoT effects already on target
- Creates massive burst window

---

## Perfect Solution Damage

| Rank | Dice | Average | With Exploitation Follow-up |
|------|------|---------|---------------------------|
| Base | 8d10 | 44 | 88 effective |
| R2 | 10d10 | 55 | 110 effective |
| R3 | 12d10 | 66 | 132 effective |

---

## Kill Recovery (Rank 3)

If Perfect Solution kills the target:
- Recover ALL payload charges expended during combat
- Essentially resets your resource economy
- Enables continued pressure in long fights

---

## Capstone Upgrade Effect

When Master Alchemist is trained:
- All Tier 1 abilities upgrade to Rank 3
- All Tier 2 abilities upgrade to Rank 3

**Affected Abilities:**
- Alchemical Analysis I → Rank 3 (all info + party damage bonus)
- Payload Strike → Rank 3 (4d8, critical doubles)
- Field Preparation → Rank 3 (8 payloads, short rests, bonus chance)
- Rack Expansion → Rank 3 (10 capacity, quick-swap)
- Targeted Injection → Rank 3 (5d8, triple on vulnerability)
- Cocktail Mixing → Rank 3 (triple cocktails, synergies)

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Boss fight opener | Perfect Solution → Party burst |
| Priority target | Delete dangerous enemy |
| Damage check | Ensure kill for charge recovery (R3) |
| Unknown enemy | Auto-analysis reveals all weaknesses |

---

## Party Coordination Strategy

**Optimal Turn Sequence:**
```
Turn 1: Master Alchemist uses Perfect Solution
        → Target takes 8d10 damage
        → [Perfect Exploitation] applied

Turn 2: All party members attack exploited target
        → Berserkr: 40 damage → 80 damage
        → Rúnasmiðr: 30 damage → 60 damage
        → Skjaldmær: 25 damage → 50 damage
        → Total: 190 damage in one turn
```

---

## Combat Log Examples

- "PERFECT SOLUTION! Custom payload synthesized!"
- "Targeting [Boss]'s lowest resistance: Fire"
- "Auto-hit! 52 Fire damage!"
- "[Perfect Exploitation] applied! +100% damage for 3 turns!"
- "[Boss] killed! ALL payload charges recovered!"
- "Universal Reagent: +11 bonus damage (3d8)"
- "Universal Reagent: Auto-analysis complete—[Enemy] vulnerable to Ice, Energy"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Alka-hestur Overview](../alka-hestur-overview.md) | Parent specialization |
| [Alchemical Analysis I](alchemical-analysis-i.md) | Superseded by auto-analysis |
| [Targeted Injection](targeted-injection.md) | Alternative precision strike |
| [Area Saturation](area-saturation.md) | AoE alternative |
