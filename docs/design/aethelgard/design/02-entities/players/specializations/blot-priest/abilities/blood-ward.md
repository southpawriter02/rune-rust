---
id: ABILITY-BLOT-PRIEST-30013
title: "Blood Ward"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Blood Ward

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Ally (30 ft range) |
| **Resource Cost** | 10% of Caster's Max HP (cannot use AP) |
| **Cooldown** | 3 turns |
| **Attribute** | WILL |
| **Tags** | [Shield], [Sacrificial], [Defensive] |
| **Ranks** | 2 → 3 |

---

## Description

*"The Priest draws a glyph with their own blood, which solidifies into a shimmering, crimson shield."*

You convert your life force into preventative protection for allies. The shield that forms is made of crystallized Aether drawn from your own blood—and when it shatters, it strikes back at the attacker.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Cost: 10% of Max HP (cannot use AP)
- Create damage-absorbing shield on ally
- Shield HP: 2.5× HP sacrificed
- Duration: 2 turns
- **Special:** When shattered, inflicts 2 Psychic Stress on attacker

**Formula:**
```
HPCost = Floor(Caster.MaxHP × 0.10)

If Caster.HP - HPCost < 1:
    Return FAIL

Caster.HP -= HPCost

ShieldHP = Floor(HPCost × 2.5)
Target.AddShield("BloodWard", {
    HP: ShieldHP,
    Duration: 2,
    OnShatter: Function(Attacker):
        Attacker.Stress += 2
        Log("Blood Ward shattered! Attacker gains 2 Stress!")
})

Log("Blood Ward: {Target} gains {ShieldHP} HP shield (2 turns)")
```

**Tooltip:** "Blood Ward (Rank 2): Shield = 2.5× HP spent. Shatter = 2 Stress to attacker. Cost: 10% Max HP"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Cost: 10% of Max HP
- Shield HP: 3.5× HP sacrificed
- Duration: 3 turns
- Shatter: 2 Stress to attacker
- **NEW:** While shield is active, target has +2 Soak

**Formula:**
```
HPCost = Floor(Caster.MaxHP × 0.10)
Caster.HP -= HPCost

ShieldHP = Floor(HPCost × 3.5)
Target.AddShield("BloodWard", {
    HP: ShieldHP,
    Duration: 3,
    SoakBonus: 2,
    OnShatter: Function(Attacker):
        Attacker.Stress += 2
})

Target.AddBuff("BloodWardSoak", {
    Soak: +2,
    Duration: 3,
    LinkedTo: "BloodWard"
})

Log("Blood Ward: {Target} gains {ShieldHP} HP shield + 2 Soak (3 turns)")
```

**Tooltip:** "Blood Ward (Rank 3): Shield = 3.5× HP spent. +2 Soak while active. Shatter = 2 Stress."

---

## Shield Value Calculation

| Max HP | 10% Cost | R2 Shield (2.5×) | R3 Shield (3.5×) |
|--------|----------|------------------|------------------|
| 60 | 6 HP | 15 Shield | 21 Shield |
| 80 | 8 HP | 20 Shield | 28 Shield |
| 100 | 10 HP | 25 Shield | 35 Shield |
| 120 | 12 HP | 30 Shield | 42 Shield |

---

## Shield Mechanics

### Damage Absorption

```
OnDamage(Target, Amount):
    If Target.HasShield("BloodWard"):
        ShieldRemaining = Target.GetShield("BloodWard").HP

        If Amount <= ShieldRemaining:
            // Shield absorbs all
            Target.GetShield("BloodWard").HP -= Amount
            Log("Blood Ward absorbs {Amount} damage")
            Return 0
        Else:
            // Shield breaks, excess passes through
            Overflow = Amount - ShieldRemaining
            TriggerShatter(Target.GetShield("BloodWard"))
            Return Overflow
```

### Shatter Effect

When the shield is destroyed by damage (not expiration):
- Attacker gains 2 Psychic Stress
- Stress is non-resistible (self-inflicted by attacking)
- Creates deterrent against focusing shielded ally

---

## Duration vs Shatter

| Outcome | Effect |
|---------|--------|
| Shield expires (duration ends) | No shatter effect, no Stress |
| Shield destroyed by damage | Shatter triggers, 2 Stress to attacker |
| Shield refreshed before expiration | Old shield replaced, no shatter |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Protecting squishy ally | Absorb incoming damage |
| Deterrence | Discourage enemies from attacking target |
| Tank support | Additional buffer for front-liner |
| Pre-emptive protection | Cast before dangerous enemy phase |

---

## Soak Bonus (Rank 3)

While Blood Ward is active:
- Target has +2 Soak
- Applies to damage that gets through the shield
- Applies to all damage sources
- Removed when shield expires or shatters

**Example:**
```
Target has 35 HP Blood Ward + 2 Soak
Enemy attacks for 20 damage:
  → Shield absorbs 20 (15 remaining)
  → Target takes 0 HP damage

Enemy attacks for 40 damage:
  → Shield absorbs 15, shatters
  → 25 damage passes through
  → -2 Soak = 23 HP damage
  → Attacker gains 2 Stress
```

---

## Resource Economics

| Resource | Cost | Value Generated |
|----------|------|-----------------|
| 10 HP (from 100 Max) | 10% | 35 Shield (3.5×) + 2 Soak |
| AP | N/A | Cannot use AP |
| Corruption | 0 | No Corruption cost! |

**Note:** Blood Ward is one of the few Blót-Priest abilities that does NOT generate Corruption—making it relatively "safe" to use.

---

## Combat Log Examples

- "Blood Ward: [Warrior] gains 35 HP shield (3 turns)"
- "Blood Ward: +2 Soak while active"
- "Blood Ward absorbs 18 damage (17 shield remaining)"
- "Blood Ward shattered! [Enemy] gains 2 Psychic Stress!"
- "Blood Ward expires (duration ended)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Blót-Priest Overview](../blot-priest-overview.md) | Parent specialization |
| [Gift of Vitae](gift-of-vitae.md) | Direct healing alternative |
| [Martyr's Resolve](martyrs-resolve.md) | Self-defense while [Bloodied] |
