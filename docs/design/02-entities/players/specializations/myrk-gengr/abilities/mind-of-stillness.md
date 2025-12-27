---
id: ABILITY-MYRK-GENGR-24015
title: "Mind of Stillness"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Mind of Stillness

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (triggered) |
| **Target** | Self |
| **Trigger** | While in [Hidden] state, at start of turn |
| **Resource Cost** | None |
| **Tags** | [Recovery], [Mental] |
| **Ranks** | 2 → 3 |

---

## Description

To manipulate the static, your mind must be a fortress of perfect calm. You meditate within chaos, becoming the quiet center of the psychic storm. While hidden, your body and mind recover.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- While in [Hidden] state, at start of your turn:
  - Remove 3 Psychic Stress
  - Regenerate 5 Stamina
- Does nothing if not Hidden

**Formula:**
```
OnTurnStart:
    If Caster.HasStatus("Hidden"):
        Caster.PsychicStress = Max(0, Caster.PsychicStress - 3)
        Caster.Stamina = Min(Caster.MaxStamina, Caster.Stamina + 5)
        Log("Mind of Stillness: -3 Stress, +5 Stamina")
```

**Tooltip:** "Mind of Stillness (Rank 2): While Hidden, -3 Stress and +5 Stamina per turn."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Stress removal increases to 7 per turn
- Stamina regeneration increases to 10 per turn
- **NEW:** Gain +1d10 to all Resolve checks while Hidden
- Encourages tactical hiding to recover between strikes

**Formula:**
```
OnTurnStart:
    If Caster.HasStatus("Hidden"):
        Caster.PsychicStress = Max(0, Caster.PsychicStress - 7)
        Caster.Stamina = Min(Caster.MaxStamina, Caster.Stamina + 10)
        Log("Mind of Stillness: -7 Stress, +10 Stamina")

While Hidden:
    ResolveBonus = +1d10
```

**Tooltip:** "Mind of Stillness (Rank 3): While Hidden, -7 Stress, +10 Stamina, +1d10 Resolve."

---

## Recovery Comparison

| Resource | Rank 2 | Rank 3 |
|----------|--------|--------|
| Stress Removed | 3/turn | 7/turn |
| Stamina Regen | 5/turn | 10/turn |
| Resolve Bonus | None | +1d10 |

---

## Tactical Implications

**Sustained Operations:**
- Enter stealth before combat to pre-regenerate
- After Shadow Strike, re-enter stealth to recover
- Each turn hidden = significant resource recovery

**3-Turn Hidden Recovery (Rank 3):**
- Stress: -21 total
- Stamina: +30 total
- Result: Fully refreshed for next assassination

---

## Combat Log Examples

- "Mind of Stillness: -3 Stress, +5 Stamina (Hidden)"
- "Mind of Stillness (Rank 3): -7 Stress, +10 Stamina"
- "+1d10 to Resolve while Hidden (Mind of Stillness)"

---

## Synergy with Ghostly Form

If Ghostly Form allows you to stay hidden after Shadow Strike:
- Next turn: Mind of Stillness triggers
- Recover resources while planning next strike
- Sustainable assassination gameplay loop

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Myrk-Gengr Overview](../myrk-gengr-overview.md) | Parent specialization |
| [Enter the Void](enter-the-void.md) | Enter [Hidden] state |
| [Ghostly Form](ghostly-form.md) | Stealth persistence |
| [Stress System](../../../../01-core/resources/stress.md) | Psychic Stress mechanics |
