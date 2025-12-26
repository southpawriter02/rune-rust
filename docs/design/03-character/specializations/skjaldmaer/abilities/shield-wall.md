---
id: ABILITY-SKJALDMAER-26023
title: "Shield Wall"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Shield Wall

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Self + Adjacent Front Row Allies |
| **Resource Cost** | 45 Stamina |
| **Status Effects** | [Fortified] |
| **Ranks** | 2 → 3 |

---

## Description

Plant feet creating bastion of physical and metaphysical stability. The Skjaldmær becomes an immovable anchor, extending this stability to nearby allies.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Self and all adjacent Front Row allies gain:
  - +3 Soak for 2 turns
  - Immunity to Push/Pull/Knockback effects for 2 turns
  - +1 bonus die to Psychic Stress resistance for 2 turns
- Affected characters gain [Fortified] status

**Formula:**
```
AffectedTargets = [Self] + GetAdjacentFrontRowAllies(Self)
For each Target in AffectedTargets:
    Target.Soak += 3 (for 2 turns)
    Target.AddStatusEffect("Fortified", Duration: 2)
    Target.StressResistanceDice += 1 (for 2 turns)
```

**Tooltip:** "Shield Wall (Rank 2): Self + adjacent allies gain +3 Soak, immunity to forced movement, +1 Stress resistance die for 2 turns. Cost: 45 Stamina"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Self and all adjacent Front Row allies gain:
  - +4 Soak for 3 turns (increased)
  - Immunity to Push/Pull/Knockback effects for 3 turns
  - +2 bonus dice to Psychic Stress resistance for 3 turns (increased)
- Affected characters gain [Fortified] status
- **NEW:** Also grants immunity to [Stun] for duration

**Formula:**
```
AffectedTargets = [Self] + GetAdjacentFrontRowAllies(Self)
For each Target in AffectedTargets:
    Target.Soak += 4 (for 3 turns)
    Target.AddStatusEffect("Fortified", Duration: 3)
    Target.StressResistanceDice += 2 (for 3 turns)
    Target.AddImmunity("Stun", Duration: 3)
```

**Tooltip:** "Shield Wall (Rank 3): Self + adjacent allies gain +4 Soak, immunity to forced movement AND Stun, +2 Stress resistance dice for 3 turns. Cost: 45 Stamina"

---

## Status Effect: [Fortified]

| Property | Value |
|----------|-------|
| **Duration** | 2-3 turns |
| **Icon** | Planted feet |
| **Effects** | Immune to Push/Pull/Knockback/Knockdown |

---

## Combat Log Examples

- "Shield Wall protects [Skjaldmær], [Ally1], [Ally2] (+3 Soak, Fortified for 2 turns)"
- "[Enemy] attempts to push [Ally1] - blocked by Fortified!"
- "Shield Wall (Rank 3): +4 Soak, Fortified, Stun Immune for 3 turns"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skjaldmær Overview](../skjaldmaer-overview.md) | Parent specialization |
| [Stunned](../../../../04-systems/status-effects/stunned.md) | Immunity at Rank 3 |
