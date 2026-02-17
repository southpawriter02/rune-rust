---
id: ABILITY-MYRK-GENGR-24017
title: "Ghostly Form"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Ghostly Form

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (passive + triggered) |
| **Target** | Self |
| **Trigger** | While [Hidden] / After Shadow Strike |
| **Resource Cost** | None |
| **Tags** | [Stealth], [Defense], [Persistence] |
| **Ranks** | None (full power when unlocked) |

---

## Description

Your connection to the world's static becomes so profound you are no longer just hiding—you are a part of it. Your form flickers and desynchronizes, making you difficult to hit even when detected.

---

## Mechanical Effect

**While [Hidden] - Defense Bonus:**
- Gain +3d10 Defense bonus to all incoming attacks
- Makes you extremely difficult to hit even if AoE reveals you

**After Shadow Strike - Stealth Persistence:**
- 65% chance to remain [Hidden] instead of breaking stealth
- Roll d100; if ≤ 65, maintain [Hidden] state
- Enables chain assassinations without re-entering stealth

**Formula:**
```
// Defense Bonus
While Caster.HasStatus("Hidden"):
    DefensePool += 3d10

// Persistence Check
OnShadowStrikeResolve:
    If Roll(1d100) <= 65:
        // Keep Hidden status
        Log("Ghostly Form: Stealth maintained!")
    Else:
        Caster.RemoveStatus("Hidden")
        Log("Ghostly Form: Stealth broken")
```

**Tooltip:** "Ghostly Form: +3d10 Defense while Hidden. 65% chance to stay Hidden after Shadow Strike."

---

## Effect Breakdown

| Effect | Value |
|--------|-------|
| Defense Bonus (Hidden) | +3d10 |
| Stealth Persistence | 65% |
| Persistence Trigger | After Shadow Strike |

---

## Defense Calculation

**While Hidden (FINESSE 4):**
```
Defense Pool = 4d10 (FINESSE) + 3d10 (Ghostly Form) = 7d10
Average: 38.5 vs typical attack
```

This makes the Shadow-Walker extremely hard to hit even if detected.

---

## Stealth Persistence Flow

```
After Shadow Strike:
1. Check if Ghostly Form is trained
2. Roll d100
3. If roll ≤ 65: Remain [Hidden]
   - Can attack again from stealth next turn
   - Mind of Stillness will trigger
4. If roll > 65: [Hidden] state ends
   - Must use Enter the Void to re-enter stealth
```

---

## Chain Assassination Potential

With 65% persistence:
- 2 kills without re-stealth: 65% × 65% = 42%
- 3 kills without re-stealth: 65%³ = 27%

Expected kills before stealth break: ~2.86 on average

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Multiple targets | Chain kills without re-stealth |
| AoE environments | +3d10 Defense protects even if revealed |
| Sustained combat | Mind of Stillness recovery between strikes |
| Boss fights | Multiple critical hits possible |

---

## Combat Log Examples

```
┌────────────────────────────────────────┐
│ 👻 GHOSTLY FORM - PERSISTENCE CHECK   │
│ ────────────────────────────────────── │
│ Roll: 42 vs 65%                        │
│                                        │
│ [SUCCESS - Stealth Maintained!]        │
└────────────────────────────────────────┘
```

- "Ghostly Form: +3d10 Defense (Hidden)"
- "Ghostly Form persistence check: 42 vs 65% - Success!"
- "Ghostly Form: Stealth maintained!"
- "Ghostly Form persistence check: 78 vs 65% - Failed. Stealth broken."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Myrk-Gengr Overview](../myrk-gengr-overview.md) | Parent specialization |
| [Shadow Strike](shadow-strike.md) | Persistence trigger |
| [Mind of Stillness](mind-of-stillness.md) | Recovery while hidden |
| [Enter the Void](enter-the-void.md) | Re-enter stealth |
