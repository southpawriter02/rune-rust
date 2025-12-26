---
id: ABILITY-GORGE-MAW-26010
title: "Tremorsense"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Tremorsense

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Tags** | [Perception], [Foundational] |
| **Ranks** | None (foundational ability) |

---

## Description

Through disciplined meditation near colossal Gorge-Maws, you have mastered seismic perception. You perceive the world through vibrations in the earth rather than sight—a gift that makes you immune to darkness but completely blind to those who never touch the ground.

---

## Mechanical Effect

**Visual Impairment Immunity:**
- IMMUNE to [Blinded] status effect
- IMMUNE to [Thick Fog] environmental effect
- IMMUNE to [Absolute Darkness] environmental effect

**Auto-Detection:**
- Automatically detect all ground-based enemies within perception range
- Detects [Hidden] and Stealth enemies that are touching the ground
- Cannot be surprised by ground-based ambushes

**Flying Vulnerability:**
- 50% miss chance when attacking flying enemies
- 0 Defense vs attacks from flying enemies
- Flying enemies are invisible on your minimap

**Formula:**
```
// Combat checks
If Target.IsFlying:
    AttackRoll = BaseRoll * 0.5  // 50% miss chance
    DefenseVsTarget = 0
    CanSeeOnMinimap = false
Else:
    AutoDetect(Target)  // Includes Hidden/Stealth
    IgnoreVisualImpairment = true
    SurpriseImmunity = true

// Status immunity
OnStatusApply("Blinded"):
    Reject("Tremorsense immune to Blinded")

OnEnvironmentEffect("ThickFog", "AbsoluteDarkness"):
    IgnoreEffect = true
```

**Tooltip:** "Tremorsense: Immune to blindness/darkness. Auto-detect ground enemies. 50% miss vs flying, 0 Defense vs flying attacks."

---

## Tactical Implications

### Strengths

| Scenario | Advantage |
|----------|-----------|
| Darkness encounters | Full effectiveness while others struggle |
| Stealth enemies | Auto-detection bypasses concealment |
| Ambushes | Cannot be surprised from ground |
| Fog/smoke | Ignore environmental vision penalties |

### Weaknesses

| Scenario | Disadvantage |
|----------|--------------|
| Flying enemies | Massive combat penalties |
| Mixed encounters | Split effectiveness |
| Aerial bombardment | No defense capability |

---

## Flying Enemy Penalties Breakdown

| Metric | Normal | vs Flying |
|--------|--------|-----------|
| Hit Chance | 100% | 50% |
| Defense | Normal | 0 |
| Detection | Auto | None |
| Minimap | Visible | Invisible |

---

## Combat Log Examples

- "Tremorsense detects [Hidden Enemy] through earth vibrations!"
- "Tremorsense: Ignoring [Thick Fog] - full visibility"
- "Attack vs [Flying Enemy] misses (Tremorsense blind to flying)"
- "WARNING: [Flying Enemy] attacks with advantage (0 Defense)"
- "[Ambush] negated - Tremorsense detected approaching enemies"

---

## GUI Display

```
┌─────────────────────────────────────────┐
│  TREMORSENSE ACTIVE                     │
│  ✓ Visual impairment immune             │
│  ✓ Ground enemies auto-detected         │
│  ⚠ Flying enemies: 50% miss, 0 Defense  │
└─────────────────────────────────────────┘
```

- Warning flashes red when flying enemies present
- Icon: Eye with seismic wave patterns

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Gorge-Maw Ascetic Overview](../gorge-maw-ascetic-overview.md) | Parent specialization |
| [Sensory Discipline](sensory-discipline.md) | Mental resistance passive |
