---
id: ABILITY-SKALD-28009
title: "Saga of the Einherjar"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Saga of the Einherjar

**Type:** Active [Performance] (Capstone) | **Tier:** 4 | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | All Allies |
| **Resource Cost** | 75 Stamina |
| **Performance** | Yes |
| **Duration** | WILL rounds |
| **Cooldown** | Once per combat |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |
| **Ranks** | None (full power when unlocked) |

---

## Description

The masterpiece saga of the greatest heroes who ever lived. Your allies believe themselves elevated to legendary status—and for a moment, they are.

---

## Mechanical Effect

**While Performing:**
- All allies gain [Inspired] (+5d10 to damage)
- All allies gain 40 temporary HP
- All allies immune to [Feared] and [Stunned]

**AFTERMATH:** When performance ends, all affected allies take **6 Psychic Stress**

**Formula:**
```
Duration = Skald.WILL (rounds)
While Performing:
    For each Ally:
        Ally.AddStatus("Inspired", BonusDice: 5)
        Ally.TempHP += 40
        Ally.AddImmunity("Feared")
        Ally.AddImmunity("Stunned")

OnPerformanceEnd:
    For each AffectedAlly:
        Ally.PsychicStress += 6
```

**Tooltip:** "Saga of the Einherjar: All allies: [Inspired] (+5d10 damage), 40 temp HP, Fear/Stun immune. AFTERMATH: 6 Psychic Stress each. WILL rounds. Cost: 75 Stamina. Once per combat."

---

## [Inspired] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | Performance duration |
| **Effect** | +5d10 to all damage rolls |
| **Visual** | Golden warrior glow |

---

## Aftermath Consideration

The +6 Psychic Stress to all allies is significant:
- Plan for stress recovery afterward
- Seiðkona's Spiritual Anchor helps
- Don't use if party is near Stress threshold
- Worth it for decisive combat moments

---

## Timing Considerations

**Optimal Use:**
- Boss fights requiring burst damage
- Desperate situations needing immunity
- When party can afford Stress cost
- Combined with other damage buffs

**Avoid Using When:**
- Party already high on Stress
- Short fights (wasted duration)
- No immediate threat (save for later)

---

## Combat Log Examples

- "SAGA OF THE EINHERJAR! 'Rise, warriors! Be as the Einherjar!'"
- "All allies gain [Inspired] (+5d10 damage) and 40 Temp HP!"
- "All allies immune to Fear and Stun!"
- "[Ally] deals 58 damage! (+5d10 from Inspired)"
- "Saga of the Einherjar fades..."
- "Aftermath: [Ally A] +6 Psychic Stress, [Ally B] +6 Psychic Stress..."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skald Overview](../skald-overview.md) | Parent specialization |
| [Enduring Performance](enduring-performance.md) | Duration extension |
| [Stress System](../../../../01-core/resources/stress.md) | Aftermath mechanics |
