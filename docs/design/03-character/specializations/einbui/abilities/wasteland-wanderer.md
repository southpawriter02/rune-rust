---
id: ABILITY-EINBUI-27015
title: "Wasteland Wanderer"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Wasteland Wanderer

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Tags** | [Survival], [Resistance] |
| **Ranks** | 2 → 3 |

---

## Description

The wasteland has tried to kill you a thousand times. You've learned to endure toxic air, poisoned water, and environments that would fell an unprepared traveler.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Half damage from environmental hazards (radiation, toxic air, extreme temperatures)
- +1 die to resist [Poisoned] status effect
- +1 die to resist [Disease] status effect

**Formula:**
```
OnEnvironmentalDamage:
    DamageTaken = BaseDamage / 2

OnStatusResist("Poisoned"):
    BonusDice += 1

OnStatusResist("Disease"):
    BonusDice += 1
```

**Tooltip:** "Wasteland Wanderer (Rank 2): Half environmental damage. +1 die vs Poison/Disease."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Half damage from environmental hazards
- +2 dice to resist [Poisoned] status effect
- +2 dice to resist [Disease] status effect
- **NEW:** Can rest safely in mildly hazardous environments

**Formula:**
```
OnEnvironmentalDamage:
    DamageTaken = BaseDamage / 2

OnStatusResist("Poisoned"):
    BonusDice += 2

OnStatusResist("Disease"):
    BonusDice += 2

OnRestAttempt:
    If Environment.HazardLevel == "Mild":
        AllowSafeRest = true
```

**Tooltip:** "Wasteland Wanderer (Rank 3): Half environmental damage. +2 dice vs Poison/Disease. Rest safely in mild hazards."

---

## Environmental Hazard Types

| Hazard | Effect | Wanderer Reduction |
|--------|--------|-------------------|
| Radiation | 2d6/turn | 1d6/turn |
| Toxic Air | 1d8/turn | 1d4/turn |
| Extreme Cold | 1d6/turn | 1d3/turn |
| Extreme Heat | 1d6/turn | 1d3/turn |

---

## Environment Hazard Levels

| Level | Examples | Rest Allowed (Rank 3) |
|-------|----------|----------------------|
| None | Normal rooms | Yes |
| Mild | Light radiation, thin air | Yes (Rank 3 only) |
| Moderate | Toxic spores, intense cold | No |
| Severe | Active contamination | No |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Hazardous exploration | Scout contaminated areas |
| Poison encounters | Resist enemy toxins |
| Extended travel | Rest in suboptimal locations |
| Resource conservation | Less healing needed |

---

## Combat Log Examples

- "Wasteland Wanderer: Environmental damage reduced from 8 to 4"
- "Resist [Poisoned]: +2 bonus dice (Wasteland Wanderer)"
- "Resting in mild hazard zone (Wasteland Wanderer Rank 3)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Einbui Overview](../einbui-overview.md) | Parent specialization |
| [Poisoned Status](../../../../04-systems/status-effects/poisoned.md) | Status effect details |
| [Room Engine Core](../../../../07-environment/room-engine/core.md) | Environmental hazards |
