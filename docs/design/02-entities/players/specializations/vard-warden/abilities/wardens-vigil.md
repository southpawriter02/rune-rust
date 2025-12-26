---
id: ABILITY-VARD-WARDEN-28015
title: "Warden's Vigil"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Warden's Vigil

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | All allies in same row |
| **Resource Cost** | None |
| **Tags** | [Aura], [Defensive], [Anti-Stress] |
| **Ranks** | None (NO RANKS - full power when unlocked) |

---

## Description

Your presence radiates calm certainty. Allies fighting beside you feel the weight of your protection—the knowledge that someone stands ready to shield them from harm. This quiet confidence bolsters their minds against the horrors of the wasteland.

---

## Mechanical Effect

**Protective Aura:**
- All allies in your row gain +1d10 to Stress resistance checks
- Effect is constant while allies remain in your row
- No action required—simply being present provides the benefit

**Formula:**
```
OnStressCheck(Ally):
    If Ally.Row == Caster.Row AND Ally != Caster:
        BonusDice = 1d10
        Check.AddBonus(BonusDice, "Warden's Vigil")
        Log("Warden's Vigil: +1d10 Stress resistance for {Ally}")
```

**Tooltip:** "Warden's Vigil: Allies in your row gain +1d10 vs Stress. Passive."

---

## Effect Summary

| Benefit | Value |
|---------|-------|
| Bonus Dice | +1d10 |
| Check Type | Stress resistance |
| Target | All allies in same row |
| Duration | Permanent (while in row) |

---

## Why No Ranks?

This ability has **NO RANKS** because:
1. It provides aura-style group utility
2. The +1d10 bonus is significant but consistent
3. Row positioning creates natural limitation
4. Other abilities in this tree scale instead

---

## Row Mechanics

**Row Positions:**
- Front Row (melee range)
- Back Row (ranged/support)

**Vigil Coverage:**
- Only affects allies in the SAME row as the Vard-Warden
- Does not affect the Vard-Warden themselves
- Allies moving between rows gain/lose the benefit immediately

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Front line | Protect melee fighters from horror |
| Back line | Shield casters from Stress |
| Pressure fights | Reduce party-wide Stress accumulation |
| Terror enemies | Counter fear-inducing attacks |

---

## Synergy with Other Abilities

| Ability | Synergy |
|---------|---------|
| Consecrate Ground | Rank 3 zone + Vigil = layered Stress defense |
| Glyph of Sanctuary | Stress immunity stacks with Vigil |
| Sanctified Resolve | Both provide resistance bonuses |

---

## Party Composition Value

**High Value Allies:**
- Berserkr (constant Stress from rage)
- Low-WILL characters (need resistance help)
- Frontline fighters (more horror exposure)

**Positioning Consideration:**
- Vard-Warden can choose row to maximize Vigil coverage
- Front row protects melee allies
- Back row protects ranged/support allies

---

## Combat Log Examples

- "Warden's Vigil: +1d10 Stress resistance for [Berserkr]"
- "[Ally] resists Horror! (Warden's Vigil bonus: 7)"
- "[Ally] moves to Front Row—gains Warden's Vigil"
- "[Ally] moves to Back Row—loses Warden's Vigil"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Vard-Warden Overview](../vard-warden-overview.md) | Parent specialization |
| [Glyph of Sanctuary](glyph-of-sanctuary.md) | Active Stress protection |
| [Stress Resource](../../../../01-core/resources/stress.md) | Stress mechanics |
