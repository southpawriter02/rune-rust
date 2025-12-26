---
id: ABILITY-HLEKKR-25012
title: "Grappling Hook Toss"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Grappling Hook Toss

**Type:** Active | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Back Row) |
| **Resource Cost** | 30 Stamina |
| **Cooldown** | 3 turns |
| **Damage Type** | Physical |
| **Status Effect** | [Disoriented] |
| **Control Effect** | Pull (Back → Front) |
| **Tags** | [Control], [Pull], [Ranged] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

Your hooked chain latches onto a target in the back row and violently drags them to the front. The disorienting journey leaves them vulnerable to your allies' attacks.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- Deal 2d8 Physical damage
- Pull target from Back Row to Front Row
- Apply [Disoriented] for 1 turn

**Formula:**
```
Damage = Roll(2d8)
Target.TakeDamage(Damage, "Physical")
Target.Position = FrontRow
Target.AddStatus("Disoriented", Duration: 1)
```

**Tooltip:** "Grappling Hook Toss (Rank 1): 2d8 Physical. Pull Back→Front. [Disoriented] 1 turn. Cost: 30 Stamina"

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- Deal 3d8 Physical damage
- Pull target from any ranged position to melee
- Apply [Disoriented] for 1 turn

**Formula:**
```
Damage = Roll(3d8)
Target.TakeDamage(Damage, "Physical")
Target.Position = FrontRow  // Even from extreme range
Target.AddStatus("Disoriented", Duration: 1)
```

**Tooltip:** "Grappling Hook Toss (Rank 2): 3d8 Physical. Pull from any range. [Disoriented] 1 turn."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Deal 3d8 Physical damage
- Pull target from any position
- Apply [Disoriented] for 1 turn
- **NEW:** On successful pull vs corrupted enemy, generate 10 Focus

**Formula:**
```
Damage = Roll(3d8)
Target.TakeDamage(Damage, "Physical")
Target.Position = FrontRow
Target.AddStatus("Disoriented", Duration: 1)

If Target.Corruption > 0:
    Caster.Focus += 10
    Log("Corruption pull: +10 Focus generated!")
```

**Tooltip:** "Grappling Hook Toss (Rank 3): 3d8 Physical. Pull any range. +10 Focus vs corrupted."

---

## [Disoriented] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 1 turn |
| **Effect** | -2 dice to Accuracy |
| **Complex Abilities** | Cannot use |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Back-line threats | Pull healer/caster to front |
| Formation breaking | Disrupt enemy positioning |
| Focus generation | Target corrupted for resources |
| Setup | Pull into melee range for team |

---

## Master of Puppets Synergy

When Master of Puppets (Capstone) is trained:
- Every Pull also applies [Vulnerable] for 2 turns
- This stacks with [Disoriented] for massive damage setup

---

## Combat Log Examples

- "Grappling Hook Toss: [Enemy Healer] dragged from Back to Front!"
- "[Enemy] takes 14 damage and is [Disoriented]"
- "Corruption pull: +10 Focus generated!"
- "Master of Puppets: [Enemy] is now [Vulnerable]!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Hlekkr-Master Overview](../hlekkr-master-overview.md) | Parent specialization |
| [Master of Puppets](master-of-puppets.md) | Pull synergy |
| [Disoriented Status](../../../../04-systems/status-effects/disoriented.md) | Status effect |
| [Punish the Helpless](punish-the-helpless.md) | Damage multiplier |
