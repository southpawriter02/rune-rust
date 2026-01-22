---
id: ABILITY-VARD-WARDEN-28010
title: "Sanctified Resolve I"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Sanctified Resolve I

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Tags** | [Defensive], [Anti-Control] |
| **Ranks** | None (NO RANKS - full power when unlocked) |

---

## Description

Your connection to stable Aether anchors you against forces that would move you. Where others are thrown about by reality-warping effects, you stand unmoved—a fixed point in a chaotic world.

---

## Mechanical Effect

**Displacement Resistance:**
- Add +1d10 to all WILL checks against Push or Pull effects
- Applies to both magical and physical displacement
- Stacks with other resistance bonuses

**Formula:**
```
OnPushPullCheck:
    If Check.Type == "WILL" AND Check.Against IN ["Push", "Pull"]:
        BonusDice = 1d10
        Check.AddBonus(BonusDice, "Sanctified Resolve")
        Log("Sanctified Resolve: +1d10 vs displacement")
```

**Tooltip:** "Sanctified Resolve I: +1d10 WILL vs Push/Pull effects. Passive."

---

## Effect Summary

| Benefit | Value |
|---------|-------|
| Bonus Dice | +1d10 |
| Check Type | WILL |
| Against | Push and Pull effects |
| Duration | Permanent (passive) |

---

## Why No Ranks?

This ability has **NO RANKS** because:
1. It provides simple, consistent defensive utility
2. The +1d10 bonus is significant but not scaling
3. Other abilities in this tree scale instead

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Enemy knockback | Resist being pushed from position |
| Gravity effects | Maintain ground against pulls |
| Protect barriers | Stay near your constructs |
| Zone control | Remain in consecrated ground |

---

## Synergy with Other Abilities

| Ability | Synergy |
|---------|---------|
| Runic Barrier | Stay near your walls |
| Consecrate Ground | Remain in healing zones |
| Warden's Vigil | Maintain row position for allies |

---

## Combat Log Examples

- "Sanctified Resolve: +1d10 vs Push (rolling 7)"
- "[Character] resists knockback! (Sanctified Resolve)"
- "Push effect fails! Anchored by Sanctified Resolve"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Vard-Warden Overview](../vard-warden-overview.md) | Parent specialization |
| [Runic Barrier](runic-barrier.md) | Positional control |
| [Consecrate Ground](consecrate-ground.md) | Zone benefit |
