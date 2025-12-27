---
id: ABILITY-STRANDHOGG-25004
title: "Tidal Rush"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Tidal Rush

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (triggered) |
| **Target** | Self |
| **Trigger** | On hitting debuffed enemy |
| **Resource Cost** | None |
| **Tags** | [Momentum], [Synergy] |
| **Ranks** | 2 → 3 |

---

## Description

The tide of violence builds faster when your enemies are already weakened. Every strike against a struggling foe feeds your momentum exponentially—they falter, and you accelerate.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- When hitting enemy with mental/control debuffs, generate +10 bonus Momentum
- Qualifying debuffs: [Disoriented], [Stunned], [Feared], [Slowed], [Rooted]

**Formula:**
```
OnHitEnemy:
    If Target.HasAnyStatus("Disoriented", "Stunned", "Feared", "Slowed", "Rooted"):
        Caster.Momentum += 10
        Log("Tidal Rush: +10 bonus Momentum vs debuffed enemy!")
```

**Tooltip:** "Tidal Rush (Rank 2): +10 bonus Momentum when hitting enemies with control debuffs."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Bonus Momentum increased to +15 per hit
- **NEW:** Also triggers on enemies with DoT effects
- Qualifying DoTs: [Bleeding], [Burning], [Poisoned]

**Formula:**
```
OnHitEnemy:
    ControlDebuffs = ["Disoriented", "Stunned", "Feared", "Slowed", "Rooted"]
    DoTEffects = ["Bleeding", "Burning", "Poisoned"]

    If Target.HasAnyStatus(ControlDebuffs) OR Target.HasAnyStatus(DoTEffects):
        Caster.Momentum += 15
        Log("Tidal Rush: +15 bonus Momentum!")
```

**Tooltip:** "Tidal Rush (Rank 3): +15 Momentum vs enemies with control debuffs OR DoTs."

---

## Qualifying Status Effects

### Control Debuffs (Both Ranks)

| Status | Triggers? |
|--------|-----------|
| [Disoriented] | Yes |
| [Stunned] | Yes |
| [Feared] | Yes |
| [Slowed] | Yes |
| [Rooted] | Yes |

### DoT Effects (Rank 3 Only)

| Status | Triggers? |
|--------|-----------|
| [Bleeding] | Yes (Rank 3) |
| [Burning] | Yes (Rank 3) |
| [Poisoned] | Yes (Rank 3) |

---

## Momentum Generation Stacking

Tidal Rush bonus **stacks** with ability base generation:

| Ability | Base | + Tidal Rush (R2) | + Tidal Rush (R3) |
|---------|------|-------------------|-------------------|
| Reaver's Strike | +15 | +25 | +30 |
| Dread Charge | +15 | +25 | +30 |
| Harrier's Whirlwind | +5-10 | +15-20 | +20-25 |

---

## Party Synergy

**Best Partners for Tidal Rush:**

| Partner | Debuff Provided |
|---------|-----------------|
| Hlekkr-Master | [Rooted], [Slowed], [Stunned] |
| Echo-Caller | [Feared], [Disoriented] |
| Rust-Witch | [Corroded] (DoT-like) |
| Myrk-Gengr | [Feared], [Silenced] |

---

## Combat Log Examples

- "Tidal Rush: +10 bonus Momentum vs [Disoriented] enemy!"
- "Tidal Rush (Rank 3): +15 Momentum vs [Bleeding] target!"
- "Total Momentum generated: 30 (15 base + 15 Tidal Rush)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Strandhogg Overview](../strandhogg-overview.md) | Parent specialization |
| [Dread Charge](dread-charge.md) | [Disoriented] source |
| [Vicious Flank](vicious-flank.md) | Debuff damage bonus |
