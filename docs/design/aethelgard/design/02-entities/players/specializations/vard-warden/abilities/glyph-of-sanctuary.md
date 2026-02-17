---
id: ABILITY-VARD-WARDEN-28016
title: "Glyph of Sanctuary"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Glyph of Sanctuary

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | All allies (party-wide) |
| **Resource Cost** | 50 Aether |
| **Cooldown** | Once per combat |
| **Attribute** | WILL |
| **Tags** | [Party], [Defensive], [Anti-Stress], [Temp HP] |
| **Ranks** | None (full power when unlocked) |

---

## Description

You inscribe a master glyph of absolute sanctuary—a rune so powerful it wraps your entire party in protective Aether. For a brief moment, your allies exist in a pocket of perfect stability, immune to the psychological horrors of the wasteland.

---

## Mechanical Effect

**Party Protection:**
- All allies gain 15 Temporary HP
- All allies become immune to Stress for 2 turns
- Cost: 50 Aether
- Once per combat limitation

**Formula:**
```
Caster.Aether -= 50

For Each Ally in Party:
    Ally.TempHP += 15
    Ally.AddBuff("Sanctuary", {
        StressImmunity: True,
        Duration: 2
    })
    Log("Glyph of Sanctuary: {Ally} gains 15 TempHP + Stress Immunity")

Combat.SetCooldown("GlyphOfSanctuary", "OncePerCombat")
Log("GLYPH OF SANCTUARY! Party shielded!")
```

**Tooltip:** "Glyph of Sanctuary: All allies gain 15 TempHP + Stress Immunity (2 turns). Once per combat. Cost: 50 Aether"

---

## Effect Summary

| Benefit | Value |
|---------|-------|
| Temporary HP | +15 (each ally) |
| Stress Immunity | 2 turns |
| Target | Entire party |
| Limitation | Once per combat |

---

## Temporary HP Mechanics

**How Temp HP Works:**
- Absorbs damage before real HP
- Does not stack (highest value applies)
- Fades at end of combat

---

## Stress Immunity

**What It Blocks:**
- Horror checks
- Fear effects
- Sanity damage
- Stress-inducing attacks
- Environmental Stress triggers

**What It Doesn't Block:**
- Physical damage
- Status effects (non-Stress)
- Corruption (separate system)

---

## Optimal Timing

**Best Moments:**
- Before a boss uses terror ability
- When party Stress is climbing dangerously
- Entering a horrifying room/encounter
- When ally is near Stress threshold

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Terror boss | Pre-emptive Stress immunity |
| Horror waves | Block sustained psychological damage |
| Berserkr protection | Prevent rage-induced Stress spiral |
| Critical moment | Absorb damage spike |

---

## Resource Economics

| Stat | Value |
|------|-------|
| Aether Cost | 50 (significant investment) |
| Total TempHP | 60 (4-person party) |
| Stress Prevented | All (for 2 turns) |

**Comparison:** This is a high-cost, high-impact ability. Save for critical moments rather than routine use.

---

## Combat Log Examples

- "GLYPH OF SANCTUARY! Party shielded!"
- "[Character] gains 15 Temporary HP (Glyph of Sanctuary)"
- "[Character] is immune to Stress (2 turns)"
- "Horror attack blocked! [Character] is immune (Sanctuary)"
- "Glyph of Sanctuary fades after 2 turns"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Vard-Warden Overview](../vard-warden-overview.md) | Parent specialization |
| [Warden's Vigil](wardens-vigil.md) | Passive Stress resistance |
| [Indomitable Bastion](indomitable-bastion.md) | Ultimate protection |
