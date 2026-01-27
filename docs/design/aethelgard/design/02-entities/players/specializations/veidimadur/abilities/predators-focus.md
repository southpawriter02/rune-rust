---
id: ABILITY-VEIDIMADUR-24006
title: "Predator's Focus"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Predator's Focus

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self (while in back row) |
| **Resource Cost** | None |
| **Condition** | Must be in back row, not adjacent to enemies |
| **Ranks** | 2 → 3 |

---

## Description

When you maintain distance from your prey, your mind enters a state of perfect clarity. The chaos of battle fades, leaving only you and your target.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- While in back row and not adjacent to enemies:
  - +2d10 to Resolve vs Psychic Stress
  - +1d10 to Perception

**Formula:**
```
If Hunter.Position.Row == Back AND NOT Hunter.AdjacentToEnemy:
    ResolveCheckPool += 2d10 (vs Psychic Stress)
    PerceptionCheckPool += 1d10
```

**Tooltip:** "Predator's Focus (Rank 2): While in back row, not adjacent to enemies: +2d10 Resolve vs Stress, +1d10 Perception."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +3d10 to Resolve vs Psychic Stress
- +3d10 to Perception
- **NEW:** Regenerate 5 Stamina per turn out of combat

**Formula:**
```
If Hunter.Position.Row == Back AND NOT Hunter.AdjacentToEnemy:
    ResolveCheckPool += 3d10 (vs Psychic Stress)
    PerceptionCheckPool += 3d10
If OutOfCombat:
    Hunter.Stamina += 5 per turn
```

**Tooltip:** "Predator's Focus (Rank 3): While in back row: +3d10 Resolve, +3d10 Perception. Regenerate 5 Stamina/turn out of combat."

---

## Combat Log Examples

- "Predator's Focus: +2d10 to Resolve vs Psychic Stress (back row bonus)"
- "Predator's Focus (Rank 3): +3d10 to all perception checks"
- "Predator's Focus: Regenerated 5 Stamina (out of combat)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Veiðimaðr Overview](../veidimadur-overview.md) | Parent specialization |
| [Stress](../../../../01-core/resources/stress.md) | Resolve system |
