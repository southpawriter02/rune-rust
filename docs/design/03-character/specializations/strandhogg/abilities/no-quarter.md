---
id: ABILITY-STRANDHOGG-25007
title: "No Quarter"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# No Quarter

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (triggered) |
| **Target** | Self |
| **Trigger** | When you reduce an enemy to 0 HP |
| **Resource Cost** | None |
| **Tags** | [Momentum], [Kill Reward], [Mobility] |
| **Ranks** | None (full power when unlocked) |

---

## Description

No mercy. No hesitation. When an enemy falls, you're already moving to the next target. Each kill feeds your momentum and your very life force—death begets death.

---

## Mechanical Effect

**On Kill Trigger:**
- Immediately move to any valid position (costs 0 Stamina)
- Generate 10 Momentum (doubled from standard kill bonus)
- Gain +15 temporary HP

**Formula:**
```
OnEnemyKill:
    // Free reposition
    Caster.Position = ChosenValidPosition
    Log("No Quarter: Free movement!")

    // Momentum bonus
    Caster.Momentum += 10
    Log("No Quarter: +10 Momentum")

    // Temporary HP
    Caster.TempHP += 15
    Log("No Quarter: +15 Temporary HP")
```

**Tooltip:** "No Quarter: On kill, free move + 10 Momentum + 15 Temporary HP."

---

## Effect Summary

| Benefit | Value |
|---------|-------|
| Free Movement | Any valid position |
| Momentum | +10 |
| Temporary HP | +15 |
| Trigger | Any kill by any ability |

---

## Kill Chain Potential

**Multi-Kill Scenario:**
1. Kill Enemy A → Free move + 10 Momentum + 15 TempHP
2. Attack Enemy B, kill → Free move + 10 Momentum + 15 TempHP
3. Total: 2 free moves, +20 Momentum, +30 TempHP

**With Riptide of Carnage (4 attacks):**
- Potential: 4 kills = 40 Momentum refund + 60 TempHP

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Mopping up | Chain through weak enemies |
| Survivability | TempHP keeps you alive |
| Repositioning | Free moves enable hit-and-run |
| Momentum sustain | Kills fuel next executions |

---

## Synergy with Other Abilities

| Ability | Synergy |
|---------|---------|
| Savage Harvest | Double kill rewards |
| Riptide of Carnage | 4 potential No Quarter triggers |
| Vicious Flank | Kill refund stacks |

---

## Combat Log Examples

- "Kill confirmed on [Enemy]!"
- "No Quarter: Free movement available"
- "No Quarter: +10 Momentum (now 75/100)"
- "No Quarter: +15 Temporary HP"
- "[Character] moves to flanking position (No Quarter)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Strandhogg Overview](../strandhogg-overview.md) | Parent specialization |
| [Savage Harvest](savage-harvest.md) | Execution ability |
| [Riptide of Carnage](riptide-of-carnage.md) | Multi-kill potential |
