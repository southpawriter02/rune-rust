---
id: ABILITY-SKAR-HORDE-29009
title: "Monstrous Apotheosis"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Monstrous Apotheosis

**Type:** Active (Capstone) | **Tier:** 4 | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Bonus Action |
| **Target** | Self |
| **Resource Cost** | 20 Stamina + 75 Savagery |
| **Cooldown** | Once per combat |
| **Status Effect** | [Apotheosis] |
| **Special** | Training upgrades all Tier 1, 2, & 3 abilities to Rank 3 |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You give in completely. The whispers become a roar. Your augment screams with power. You are no longer human. You are a weapon. You are inevitable.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- Enter [Apotheosis] state for 3 turns:
  - Savage Strike costs 0 Stamina
  - Grievous Wound automatically applies [Bleeding]
  - Immune to [Feared] and [Stunned]
- **AFTERMATH:** +30 Psychic Stress when [Apotheosis] ends

**Formula:**
```
Self.AddStatus("Apotheosis", Duration: 3)
While Apotheosis:
    SavageStrike.StaminaCost = 0
    GrievousWound.AppliesBleeding = true
    Immunity: Feared, Stunned

OnApotheosisEnd:
    PsychicStress += 30
```

**Tooltip:** "Monstrous Apotheosis (Rank 1): 3 turns: Free Savage Strikes, Grievous Wound bleeds, Fear/Stun immunity. AFTERMATH: +30 Stress. Cost: 20 Stamina, 75 Savagery"

---

### Rank 2 (Unlocked: Based on tree progression)

**Effect:**
- Duration increased to 4 turns
- Aftermath Stress reduced to +25

**Formula:**
```
ApotheosisDuration = 4
PostApotheosisStress = 25
```

**Tooltip:** "Monstrous Apotheosis (Rank 2): 4 turns. +25 Stress after."

---

### Rank 3 (Unlocked: Full tree completion)

**Effect:**
- +25% damage to all attacks during [Apotheosis]
- Aftermath Stress reduced to +20
- **NEW:** Can end [Apotheosis] early (as Bonus Action) to avoid Stress penalty entirely

**Formula:**
```
While Apotheosis:
    AllDamage *= 1.25
PostApotheosisStress = 20
CanEndEarly = true  // Avoids Stress if ended voluntarily
```

**Tooltip:** "Monstrous Apotheosis (Rank 3): +25% damage. Can end early to avoid +20 Stress."

---

## [Apotheosis] Status Effect

| Property | Rank 1 | Rank 2 | Rank 3 |
|----------|--------|--------|--------|
| Duration | 3 turns | 4 turns | 4 turns |
| Free Savage Strikes | ✓ | ✓ | ✓ |
| Grievous bleeds | ✓ | ✓ | ✓ |
| Fear/Stun immunity | ✓ | ✓ | ✓ |
| Damage bonus | — | — | +25% |
| Aftermath Stress | +30 | +25 | +20 (avoidable) |

---

## Tactical Considerations

**Activation Timing:**
- Requires 75 Savagery (build up first)
- Best used for burst damage windows
- Plan for Stress aftermath

**Early Termination (Rank 3):**
- End voluntarily to avoid Stress
- Useful if fight ends early
- Requires Bonus Action

---

## Combat Log Examples

- "MONSTROUS APOTHEOSIS! You are no longer human. You are a WEAPON!"
- "[Apotheosis] active: Savage Strike costs 0 Stamina!"
- "Grievous Wound: +[Bleeding] (Apotheosis bonus)"
- "[Enemy] attempts to Fear you... IMMUNE (Apotheosis)"
- "[Apotheosis] ending..."
- "Apotheosis aftermath: +30 Psychic Stress"
- "(Rank 3) Ending Apotheosis early - no Stress penalty"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skar-Horde Aspirant Overview](../skar-horde-aspirant-overview.md) | Parent specialization |
| [Stress System](../../../../01-core/resources/stress.md) | Psychic Stress mechanics |
| [Savage Strike](savage-strike.md) | Enhanced ability |
| [Grievous Wound](grievous-wound.md) | Enhanced ability |
