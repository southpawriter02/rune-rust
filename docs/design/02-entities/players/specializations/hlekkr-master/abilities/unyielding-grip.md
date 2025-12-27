---
id: ABILITY-HLEKKR-25014
title: "Unyielding Grip"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Unyielding Grip

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 25 Stamina |
| **Cooldown** | 4 turns |
| **Damage Type** | Physical |
| **Status Effect** | [Seized] |
| **Tags** | [Control], [Anti-Machine] |
| **Ranks** | 2 → 3 |

---

## Description

Your chain wraps around servos, joints, and malfunctioning limbs, locking them completely in place. Mechanical and Undying creatures find their mechanisms seized, unable to take any action.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Deal 2d8 Physical damage
- 80% chance to apply [Seized] for 2 turns vs Undying/Mechanical enemies
- [Seized] prevents ALL actions

**Formula:**
```
Damage = Roll(2d8)
Target.TakeDamage(Damage, "Physical")

If Target.Type == "Undying" OR Target.Type == "Mechanical":
    If Roll(1d100) <= 80:
        Target.AddStatus("Seized", Duration: 2)
        Log("{Target} is completely Seized!")
    Else:
        Log("Unyielding Grip: {Target} resists seizure")
```

**Tooltip:** "Unyielding Grip (Rank 2): 2d8 Physical. 80% [Seized] 2 turns vs Undying/Mechanical. Cost: 25 Stamina"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Deal 2d8 Physical damage
- 80% chance [Seized] 3 turns vs Undying/Mechanical
- **NEW:** 40% chance to apply [Seized] vs NON-mechanical enemies
- **NEW:** Seized enemies take 1d6 crushing damage per turn

**Formula:**
```
Damage = Roll(2d8)
Target.TakeDamage(Damage, "Physical")

If Target.Type == "Undying" OR Target.Type == "Mechanical":
    SeizeChance = 80
Else:
    SeizeChance = 40

If Roll(1d100) <= SeizeChance:
    Duration = (Target.Type == "Undying" OR Target.Type == "Mechanical") ? 3 : 2
    Target.AddStatus("Seized", Duration: Duration, DoT: "1d6")
    Log("{Target} Seized! Taking 1d6 crushing damage per turn.")
```

**Tooltip:** "Unyielding Grip (Rank 3): 80% [Seized] 3 turns vs machines, 40% vs others. Seized: 1d6/turn."

---

## [Seized] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 2-3 turns |
| **Effect** | Cannot take ANY actions |
| **Movement** | Completely immobile |
| **DoT (Rank 3)** | 1d6 crushing damage per turn |
| **Special** | More effective vs Undying/Mechanical |

---

## Target Type Effectiveness

| Target Type | Rank 2 | Rank 3 |
|-------------|--------|--------|
| Undying | 80% / 2 turns | 80% / 3 turns + DoT |
| Mechanical | 80% / 2 turns | 80% / 3 turns + DoT |
| Organic | 0% | 40% / 2 turns + DoT |
| Other | 0% | 40% / 2 turns + DoT |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Undying enemies | High reliability lockdown |
| Mechanical foes | Jam servos and gears |
| Boss adds | Remove threats completely |
| Setup | 3-turn window for allies |

---

## Combat Log Examples

- "Unyielding Grip: [Mechanical Enemy] SEIZED for 3 turns!"
- "[Seized Enemy] cannot act (Unyielding Grip)"
- "[Seized Enemy] takes 4 crushing damage"
- "Unyielding Grip: [Organic Enemy] resists seizure (40% failed)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Hlekkr-Master Overview](../hlekkr-master-overview.md) | Parent specialization |
| [Punish the Helpless](punish-the-helpless.md) | Damage vs [Seized] |
| [Creature Traits](../../../../03-combat/creature-traits.md) | Enemy types |
