---
id: ABILITY-MYRK-GENGR-24016
title: "Terror from the Void"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Terror from the Void

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (triggered) |
| **Target** | First Shadow Strike target per combat |
| **Trigger** | First Shadow Strike each combat |
| **Resource Cost** | None |
| **Tags** | [Terror], [Psychological] |
| **Ranks** | None (full power when unlocked) |

---

## Description

You have mastered the art of the psychological alpha strike. The sheer shock and terror of your initial assault shatters minds. Targets struck from nowhere suffer not just physical wounds, but existential dread.

---

## Mechanical Effect

**First Strike Terror:**
- Triggers automatically on your first Shadow Strike each combat
- Target suffers 15 Psychic Stress (in addition to Shadow Strike damage)
- 85% chance to apply [Feared] for 3 turns
- Combat flag tracks "first strike used" to prevent multiple triggers

**Formula:**
```
OnShadowStrike:
    If NOT Combat.TerrorFromVoidUsed:
        Target.PsychicStress += 15

        If Roll(1d100) <= 85:
            Target.AddStatus("Feared", Duration: 3)
            Log("Terror from the Void: {Target} is [Feared]!")

        Combat.TerrorFromVoidUsed = true
        Log("Terror from the Void: +15 Psychic Stress!")
```

**Tooltip:** "Terror from the Void: First Shadow Strike per combat: +15 Psychic Stress to target, 85% [Feared] for 3 turns."

---

## Effect Breakdown

| Effect | Value |
|--------|-------|
| Psychic Stress Inflicted | 15 |
| [Feared] Chance | 85% |
| [Feared] Duration | 3 turns |
| Uses per Combat | 1 |

---

## [Feared] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 3 turns |
| **Effect** | -2d10 to all checks |
| **Movement** | Cannot willingly approach fear source |
| **Combat Log** | "Fled in terror" animations |

---

## First Strike Damage Stack

**Complete First Strike Damage (Rank 3 Shadow Strike):**
1. Shadow Strike: ~52 damage (doubled crit)
2. Terror from the Void: +15 Psychic Stress
3. [Feared] 3 turns (85% chance)
4. [Bleeding] 2d6/turn for 2 turns

**Total First Strike Impact:**
- ~52 Physical damage
- +15 Psychic Stress
- -2d10 to all checks for 3 turns
- ~14 DoT damage over 2 turns

---

## Combo with Throat-Cutter

If the target survives and is [Feared]:
1. Next turn: Throat-Cutter on [Feared] target
2. Applies [Silenced] + [Bleeding] (Rank 3)
3. Target now: [Feared] + [Silenced] + [Bleeding]
4. Completely neutralized

---

## Combat Log Examples

```
[TERROR FROM THE VOID]
{Target} is struck by existential dread!
Psychic Stress: +15
[Feared] applied for 3 turns!
```

- "Terror from the Void: First strike bonus activated!"
- "{Target} recoils in existential horror!"
- "85% Fear check... SUCCESS! [Feared] for 3 turns!"

---

## GUI Display

**First Strike Indicator:**
- "READY" status when Terror from the Void hasn't triggered yet
- Changes to "EXPENDED" after first Shadow Strike
- Visual glow on Shadow Strike button indicating bonus damage

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Myrk-Gengr Overview](../myrk-gengr-overview.md) | Parent specialization |
| [Shadow Strike](shadow-strike.md) | Trigger ability |
| [Throat-Cutter](throat-cutter.md) | Follow-up combo |
| [Feared Status](../../../../04-systems/status-effects/feared.md) | Status effect |
