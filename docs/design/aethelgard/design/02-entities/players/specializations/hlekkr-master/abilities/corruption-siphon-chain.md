---
id: ABILITY-HLEKKR-25017
title: "Corruption Siphon Chain"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Corruption Siphon Chain

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 30 Stamina + 5 Psychic Stress (3 at Rank 3) |
| **Cooldown** | 3 turns |
| **Status Effect** | [Stunned] |
| **Special** | Corruption purge |
| **Tags** | [Control], [Corruption] |
| **Ranks** | None (full power when unlocked) |

---

## Description

Your chain becomes a conduit for corruption itself. By channeling your will through the links, you can disrupt the glitching energy within corrupted enemies—stunning them and potentially purging some of their corruption entirely.

---

## Mechanical Effect

**Corruption-Scaled Stun:**
- No damage dealt
- [Stunned] chance based on target corruption:
  - Low (1-29): 20% chance
  - Medium (30-59): 40% chance
  - High (60-89): 70% chance
  - Extreme (90+): 90% chance
- [Stunned] duration: 2 turns
- **Extreme Corruption Bonus:** If successful vs 90+ corruption, purge 10 Corruption from target
- Psychic Stress cost reduced to 3 (from 5)

**Formula:**
```
Caster.Stamina -= 30
Caster.PsychicStress += 3  // Reduced from 5

StunChance = GetStunChance(Target.Corruption)

If Roll(1d100) <= StunChance:
    Target.AddStatus("Stunned", Duration: 2)
    Log("Corruption Siphon Chain: {Target} STUNNED!")

    If Target.Corruption >= 90:
        Target.Corruption -= 10
        Log("Corruption purged! {Target} loses 10 Corruption.")
Else:
    Log("Corruption Siphon Chain: {Target} resists stun")

GetStunChance(corruption):
    If corruption >= 90: return 90
    If corruption >= 60: return 70
    If corruption >= 30: return 40
    If corruption >= 1: return 20
    return 0
```

**Tooltip:** "Corruption Siphon Chain: [Stunned] 2 turns (chance scales with corruption). Extreme: +corruption purge. Cost: 30 Stamina + 3 Stress"

---

## Stun Chance by Corruption

| Corruption Level | Range | Stun Chance | Special |
|------------------|-------|-------------|---------|
| None | 0 | 0% | Cannot target |
| Low | 1-29 | 20% | - |
| Medium | 30-59 | 40% | - |
| High | 60-89 | 70% | - |
| Extreme | 90+ | 90% | -10 Corruption on success |

---

## [Stunned] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 2 turns (+2 with PP Rank 3 = 4) |
| **Effect** | Cannot take any actions |
| **Defense** | Automatically fails Defense rolls |

---

## Corruption Purge

**Effect:** Remove 10 Corruption from target
**Trigger:** Successful stun vs 90+ Corruption enemy
**Strategic Value:**
- Can reduce Extreme to High corruption
- Weakens enemy corruption-based abilities
- Slight thematic "redemption" element

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| High corruption target | Near-guaranteed stun |
| Boss control | Extended window with PP bonus |
| Corruption reduction | Weaken corruption abilities |
| Setup | 4-turn stun for massive damage |

---

## Combat Log Examples

- "Corruption Siphon Chain: 70% stun chance vs High Corruption"
- "[Corrupted Boss] is STUNNED for 2 turns!"
- "Extreme Corruption: 10 Corruption purged from [Target]!"
- "Pragmatic Preparation: [Stunned] extended to 4 turns"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Hlekkr-Master Overview](../hlekkr-master-overview.md) | Parent specialization |
| [Snag the Glitch](snag-the-glitch.md) | Corruption scaling |
| [Stunned Status](../../../../04-systems/status-effects/stunned.md) | Status effect |
| [Coherence Resource](../../../../01-core/resources/coherence.md) | Corruption mechanics |
