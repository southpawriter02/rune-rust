---
id: ABILITY-MYRK-GENGR-24010
title: "One with the Static I"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# One with the Static I

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Attribute** | FINESSE |
| **Tags** | [Stealth], [Resonance] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You find comfort in the world's background noise. The hum of the Blight is not a threat—it is camouflage. You have learned to synchronize with the psychic static, becoming harder to perceive.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- +1d10 to all FINESSE-based Stealth checks
- When in [Psychic Resonance] zones, gain additional +2d10 (total +3d10)

**Formula:**
```
OnStealthCheck:
    BonusDice += 1  // d10s

If InPsychicResonanceZone:
    BonusDice += 2  // Additional +2d10
```

**Tooltip:** "One with the Static I (Rank 1): +1d10 Stealth. +2d10 additional in Resonance zones."

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- +2d10 to Stealth checks
- Partial immunity to [Psychic Resonance] penalties (ignore -1d10 to other checks)
- In Resonance zones: +4d10 total (+2d10 base + 2d10 zone)

**Formula:**
```
OnStealthCheck:
    BonusDice += 2

If InPsychicResonanceZone:
    BonusDice += 2  // Additional
    IgnoreResonancePenalty = 1d10  // Partial immunity
```

**Tooltip:** "One with the Static I (Rank 2): +2d10 Stealth. +4d10 in zones. Ignore minor Resonance penalties."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- +3d10 to Stealth checks
- Full immunity to [Psychic Resonance] penalties
- In Resonance zones: enemies suffer -2d10 to detect you
- Total in Resonance zone: +5d10 Stealth

**Formula:**
```
OnStealthCheck:
    BonusDice += 3

If InPsychicResonanceZone:
    BonusDice += 2  // Additional
    IgnoreAllResonancePenalties = true
    EnemyPerceptionPenalty = -2d10
```

**Tooltip:** "One with the Static I (Rank 3): +5d10 Stealth in zones. Enemies -2d10 to detect you."

---

## Stealth Bonus Summary

| Location | Rank 1 | Rank 2 | Rank 3 |
|----------|--------|--------|--------|
| Normal | +1d10 | +2d10 | +3d10 |
| Resonance Zone | +3d10 | +4d10 | +5d10 |
| Enemy Detection | Normal | Normal | -2d10 |

---

## Synergy with Enter the Void

| Rank | Enter the Void DC | Your Dice Pool (FINESSE 4) |
|------|-------------------|---------------------------|
| 1 | 16 | 5d10 (normal) / 7d10 (zone) |
| 2 | 14 | 6d10 (normal) / 8d10 (zone) |
| 3 | 12 | 7d10 (normal) / 9d10 (zone) |

At Rank 3 in a Resonance zone, stealth is nearly guaranteed.

---

## Combat Log Examples

- "One with the Static: +2d10 to Stealth check"
- "Psychic Resonance detected: +2d10 additional Stealth"
- "One with the Static (Rank 3): Enemies -2d10 to detect you in zone"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Myrk-Gengr Overview](../myrk-gengr-overview.md) | Parent specialization |
| [Enter the Void](enter-the-void.md) | Stealth entry ability |
| [Sensory Scramble](sensory-scramble.md) | Zone creation |
