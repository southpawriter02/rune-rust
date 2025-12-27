---
id: ABILITY-GORGE-MAW-26014
title: "Shattering Wave"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Shattering Wave

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy (Any Range) |
| **Resource Cost** | 40 Stamina |
| **Cooldown** | 2 turns |
| **Status Effect** | [Stunned], [Staggered] |
| **Tags** | [Control], [Ranged] |
| **Ranks** | 2 → 3 |

---

## Description

You send a targeted tremor through the earth directly to your enemy, disrupting their balance and potentially stunning them outright. The ground itself becomes a conduit for your will.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Single target at any range, auto-hit (no attack roll)
- 60% chance to apply [Stunned] for 1 turn
- If Stun fails: Guaranteed [Staggered] for 1 turn instead

**Formula:**
```
// Auto-hit, no attack roll required
If Roll(1d100) <= 60:
    Target.AddStatus("Stunned", Duration: 1)
    Log("Shattering Wave stuns {Target}!")
Else:
    Target.AddStatus("Staggered", Duration: 1)
    Log("Shattering Wave staggers {Target}")
```

**Tooltip:** "Shattering Wave (Rank 2): 60% Stun (1 turn) or guaranteed Stagger. Any range, auto-hit. Cost: 40 Stamina"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Single target at any range, auto-hit
- 85% chance to apply [Stunned] for 2 turns
- If Stun fails: Guaranteed [Staggered] + 3d6 Physical damage

**Formula:**
```
If Roll(1d100) <= 85:
    Target.AddStatus("Stunned", Duration: 2)
    Log("Shattering Wave stuns {Target} for 2 turns!")
Else:
    Target.AddStatus("Staggered", Duration: 1)
    Damage = Roll(3d6)
    Target.TakeDamage(Damage, "Physical")
    Log("Shattering Wave: Stagger + {Damage} damage")
```

**Tooltip:** "Shattering Wave (Rank 3): 85% Stun (2 turns) or Stagger + 3d6 damage. Cost: 40 Stamina"

---

## Effect Comparison

| Rank | Stun Chance | Stun Duration | Fallback |
|------|-------------|---------------|----------|
| 2 | 60% | 1 turn | Staggered |
| 3 | 85% | 2 turns | Staggered + 3d6 |

---

## [Stunned] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 1-2 turns |
| **Effect** | Cannot take any actions |
| **Defense** | Automatically fails Defense rolls |
| **Removal** | Optional: Removed on taking damage |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Priority target | Neutralize healers/casters |
| Ranged enemies | Lock down back-line threats |
| Boss fights | Reliable control even if Stun fails |
| Setup | Immobilize for Earthshaker follow-up |

---

## Combat Log Examples

- "Shattering Wave: 60% Stun check... 42! [Enemy] is Stunned!"
- "Shattering Wave (Rank 3): 85% Stun check... 91. Stagger + 14 damage"
- "[Stunned Enemy] cannot act this turn"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Gorge-Maw Ascetic Overview](../gorge-maw-ascetic-overview.md) | Parent specialization |
| [Stunned Status](../../../../04-systems/status-effects/stunned.md) | Status effect details |
| [Earthen Grasp](earthen-grasp.md) | AoE control alternative |
