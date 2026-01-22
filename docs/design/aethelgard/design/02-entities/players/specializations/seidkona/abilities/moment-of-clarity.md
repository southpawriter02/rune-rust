---
id: ABILITY-SEIDKONA-27009
title: "Moment of Clarity"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Moment of Clarity

**Type:** Active (Capstone) | **Tier:** 4 | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Self |
| **Resource Cost** | Aether |
| **Cooldown** | Once per combat |
| **Duration** | 2-3 turns |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |
| **Ranks** | None (full power when unlocked) |

---

## Description

For a brief, shining moment, the psychic static clears. You see the threads of fate laid bare. All bargains succeed. All spirits answer.

---

## Mechanical Effect

**[Clarity] State (2 turns):**
- ALL Spirit Bargains are **100% guaranteed**
- Echo of Vigor always cleanses debuffs
- Echo of Misfortune always spreads to adjacent enemies
- Spirit Ward always lasts 4 turns
- Forlorn Communion: 0 Aether cost, +7 Stress, auto-success
- Spiritual Anchor can target an ally

**AFTERMATH:** When [Clarity] ends, gain +20 Psychic Stress

**Formula:**
```
Caster.AddStatus("Clarity", Duration: 2)

While Clarity:
    AllSpiritBargains.TriggerChance = 1.00
    ForlornCommunion.AetherCost = 0
    ForlornCommunion.StressCost = 7
    ForlornCommunion.AutoSuccess = true
    SpiritualAnchor.CanTargetAlly = true

OnClarityEnd:
    Caster.PsychicStress += 20
```

**Tooltip:** "Moment of Clarity: 2 turns of guaranteed Spirit Bargains. Enhanced abilities. AFTERMATH: +20 Psychic Stress."

---

## Enhanced Ability Effects During Clarity

| Ability | Normal | During Clarity |
|---------|--------|----------------|
| Echo of Vigor | 25-35% cleanse | 100% cleanse |
| Echo of Misfortune | Spread on crit | Always spread |
| Spirit Ward | 25% for 4 turns | Always 4 turns |
| Forlorn Communion | DC check, full cost | Auto-success, reduced cost |
| Spiritual Anchor | Self only | Can target ally |

---

## Timing Considerations

**Optimal Use:**
- Before burst healing phase (guaranteed cleanses)
- When multiple enemies are adjacent (guaranteed [Cursed] spread)
- When knowledge is critical (Forlorn Communion enhanced)
- When ally needs emergency Stress relief

**Aftermath Management:**
- +20 Stress is significant
- Plan to use Spiritual Anchor afterward
- Skald support helps mitigate

---

## Combat Log Examples

- "MOMENT OF CLARITY! All Spirit Bargains guaranteed for 2 turns!"
- "Echo of Vigor: CLARITY - Guaranteed cleanse!"
- "Echo of Misfortune: CLARITY - [Cursed] spreads automatically!"
- "Moment of Clarity fading..."
- "Clarity ended. +20 Psychic Stress (aftermath)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Seiðkona Overview](../seidkona-overview.md) | Parent specialization |
| [Echo of Vigor](echo-of-vigor.md) | Enhanced ability |
| [Echo of Misfortune](echo-of-misfortune.md) | Enhanced ability |
| [Spirit Ward](spirit-ward.md) | Enhanced ability |
| [Forlorn Communion](forlorn-communion.md) | Enhanced ability |
| [Spiritual Anchor](spiritual-anchor.md) | Enhanced ability |
