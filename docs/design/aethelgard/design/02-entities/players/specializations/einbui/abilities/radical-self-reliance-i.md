---
id: ABILITY-EINBUI-27010
title: "Radical Self-Reliance I"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Radical Self-Reliance I

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Tags** | [Survival] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You have learned to read the land and find sustenance where others see only ruin. Tracking prey and foraging for supplies are second nature to you.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- +1 die to Wasteland Survival (Tracking) checks
- +1 die to Wasteland Survival (Foraging) checks

**Formula:**
```
OnSkillCheck(WastelandSurvival, Tracking):
    BonusDice += 1

OnSkillCheck(WastelandSurvival, Foraging):
    BonusDice += 1
```

**Tooltip:** "Radical Self-Reliance I (Rank 1): +1 die to Tracking and Foraging checks."

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- +2 dice to Wasteland Survival (Tracking) checks
- +2 dice to Wasteland Survival (Foraging) checks

**Formula:**
```
OnSkillCheck(WastelandSurvival, Tracking):
    BonusDice += 2

OnSkillCheck(WastelandSurvival, Foraging):
    BonusDice += 2
```

**Tooltip:** "Radical Self-Reliance I (Rank 2): +2 dice to Tracking and Foraging checks."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- +2 dice to Wasteland Survival (Tracking) checks
- +2 dice to Wasteland Survival (Foraging) checks
- **NEW:** Auto-succeed on Easy (DC 10) survival checks

**Formula:**
```
OnSkillCheck(WastelandSurvival, Tracking):
    BonusDice += 2
    If DC <= 10:
        AutoSuccess = true

OnSkillCheck(WastelandSurvival, Foraging):
    BonusDice += 2
    If DC <= 10:
        AutoSuccess = true
```

**Tooltip:** "Radical Self-Reliance I (Rank 3): +2 dice to Tracking/Foraging. Auto-succeed DC 10 or less."

---

## Tactical Value

| Scenario | Benefit |
|----------|---------|
| Finding trails | Track enemies, locate passages |
| Gathering supplies | More herbs, materials found |
| Easy terrain | Auto-success saves time |

---

## Combat Log Examples

- "Radical Self-Reliance I: +1 die to Tracking check"
- "Foraging with +2 bonus dice (Radical Self-Reliance)"
- "DC 10 Survival check auto-succeeded (Rank 3)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Einbui Overview](../einbui-overview.md) | Parent specialization |
| [Wasteland Survival](../../../../01-core/skills/wasteland-survival.md) | Associated skill |
| [Radical Self-Reliance II](radical-self-reliance-ii.md) | Continuation ability |
