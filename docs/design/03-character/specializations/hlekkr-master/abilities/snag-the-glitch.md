---
id: ABILITY-HLEKKR-25013
title: "Snag the Glitch"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Snag the Glitch

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Tags** | [Corruption], [Control] |
| **Ranks** | 2 → 3 |

---

## Description

Corrupted flesh and malfunctioning joints make for easy targets. Your chains exploit the glitching physics of corruption, making control effects far more reliable against the tainted.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Control effects have increased success chance vs corrupted enemies:
  - Low (1-29): +20% success
  - Medium (30-59): +40% success
  - High (60-89): +80% success
  - Extreme (90+): +100% success
- +1d10 damage vs corrupted enemies

**Formula:**
```
OnControlAttempt vs Target:
    If Target.Corruption >= 1:
        SuccessBonus = GetCorruptionBonus(Target.Corruption)
        ControlChance += SuccessBonus

    DamageBonus = 1d10 vs Corrupted

GetCorruptionBonus(corruption):
    If corruption >= 90: return 100%
    If corruption >= 60: return 80%
    If corruption >= 30: return 40%
    If corruption >= 1: return 20%
    return 0%
```

**Tooltip:** "Snag the Glitch (Rank 2): Control effects scale with target corruption. +1d10 damage vs corrupted."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Same corruption scaling for control success
- **UPGRADE:** +3d10 damage vs corrupted enemies
- **NEW:** Against Extreme Corruption (90+), control effects cannot miss

**Formula:**
```
OnControlAttempt vs Target:
    If Target.Corruption >= 90:
        AutoSuccess = true  // Cannot miss
        Log("Extreme Corruption: Control auto-succeeds!")
    Else:
        SuccessBonus = GetCorruptionBonus(Target.Corruption)
        ControlChance += SuccessBonus

DamageBonus = 3d10 vs Corrupted
```

**Tooltip:** "Snag the Glitch (Rank 3): +3d10 vs corrupted. Extreme Corruption: control cannot miss."

---

## Corruption Scaling Table

| Corruption Level | Range | Success Bonus | Rank 3 Special |
|------------------|-------|---------------|----------------|
| None | 0 | +0% | - |
| Low | 1-29 | +20% | - |
| Medium | 30-59 | +40% | - |
| High | 60-89 | +80% | - |
| Extreme | 90+ | +100% | Auto-success |

---

## Damage Bonus Scaling

| Rank | Bonus Dice | Average Bonus |
|------|------------|---------------|
| 2 | +1d10 | +5.5 |
| 3 | +3d10 | +16.5 |

---

## Tactical Implications

**Priority Targets:**
- Extremely corrupted enemies become trivial to control
- Focus chains on high-corruption targets for guaranteed lockdown
- Lower-corruption enemies still receive significant bonuses

**Build Synergy:**
- Combines with Rust-Witch corruption spreading
- Synergizes with Punish the Helpless for massive damage

---

## Combat Log Examples

- "Snag the Glitch: +40% control success vs Medium Corruption"
- "Extreme Corruption: Netting Shot auto-succeeds!"
- "Snag the Glitch: +16 bonus damage vs corrupted target"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Hlekkr-Master Overview](../hlekkr-master-overview.md) | Parent specialization |
| [Corruption Siphon Chain](corruption-siphon-chain.md) | Corruption-based stun |
| [Coherence Resource](../../../../01-core/resources/coherence.md) | Corruption mechanics |
