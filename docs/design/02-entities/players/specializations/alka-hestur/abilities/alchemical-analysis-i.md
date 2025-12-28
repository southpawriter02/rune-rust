---
id: ABILITY-ALKA-HESTUR-29010
title: "Alchemical Analysis I"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Alchemical Analysis I

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (part of observation) |
| **Target** | Single Enemy (visible) |
| **Resource Cost** | None |
| **Attribute** | WITS |
| **Tags** | [Analysis], [Intelligence], [Team Support] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

*"You read the weakness before you load the answer."*

Your alchemical training has honed your eye for structural vulnerabilities. With a glance, you identify the molecular fault lines in any creature—the chemical reactions that will unmake them most efficiently.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- +1d10 bonus to WITS checks for identifying creature vulnerabilities/resistances
- On successful analysis, learn ONE vulnerability OR ONE resistance
- No action cost—analysis is part of observation

**Formula:**
```
OnAnalysisCheck(Target):
    Check = Roll(WITS) + 1d10
    If Check >= Target.AnalysisDC:
        RevealedInfo = Choose("vulnerability", "resistance")
        If RevealedInfo == "vulnerability":
            Display(Target.Vulnerabilities[0])
        Else:
            Display(Target.Resistances[0])
        Log("Alchemical Analysis: {Target} - {RevealedInfo} identified")
```

**Tooltip:** "Alchemical Analysis I (Rank 1): +1d10 WITS to identify weaknesses. Learn 1 vulnerability OR resistance."

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- +2d10 bonus to WITS checks for analysis
- On successful analysis, learn ALL vulnerabilities OR ALL resistances

**Formula:**
```
OnAnalysisCheck(Target):
    Check = Roll(WITS) + Roll(2d10)
    If Check >= Target.AnalysisDC:
        RevealedInfo = Choose("vulnerabilities", "resistances")
        If RevealedInfo == "vulnerabilities":
            Display(Target.Vulnerabilities)  // All of them
        Else:
            Display(Target.Resistances)  // All of them
        Log("Alchemical Analysis: {Target} - all {RevealedInfo} identified")
```

**Tooltip:** "Alchemical Analysis I (Rank 2): +2d10 WITS. Learn ALL vulnerabilities OR resistances."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- +3d10 bonus to WITS checks for analysis
- On successful analysis, learn ALL vulnerabilities AND ALL resistances
- **NEW:** Party gains +1d10 damage vs analyzed target's weaknesses for 3 turns

**Formula:**
```
OnAnalysisCheck(Target):
    Check = Roll(WITS) + Roll(3d10)
    If Check >= Target.AnalysisDC:
        Display(Target.Vulnerabilities)  // All
        Display(Target.Resistances)  // All

        // Party bonus
        Target.AddDebuff("Analyzed", {
            Effect: "Party +1d10 damage vs weaknesses",
            Duration: 3
        })
        Log("Alchemical Analysis: {Target} fully analyzed! Party gains +1d10 vs weaknesses")
```

**Tooltip:** "Alchemical Analysis I (Rank 3): +3d10 WITS. Learn ALL weaknesses AND resistances. Party gains +1d10 damage vs weaknesses."

---

## Analysis DC Reference

| Enemy Tier | Analysis DC |
|------------|-------------|
| Minion | 8 |
| Standard | 12 |
| Elite | 16 |
| Boss | 20 |
| Legendary | 24 |

---

## Information Revealed

### Vulnerabilities
- Elemental weaknesses (Fire, Ice, Energy, etc.)
- Damage type weaknesses (Physical, Arcane, etc.)
- Status effect susceptibilities

### Resistances
- Elemental immunities/resistances
- Damage type reductions
- Status effect immunities

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Combat start | Analyze before loading payload |
| Unknown enemy | Determine optimal approach |
| Boss fights | Full analysis for party planning |
| Payload selection | Match weakness to cartridge |

---

## Party Benefit (Rank 3)

When you successfully analyze a target:
- All party members gain +1d10 damage when attacking the target's vulnerabilities
- Lasts 3 turns
- Encourages party coordination around your analysis

---

## Combat Log Examples

- "Alchemical Analysis: [Frost Hulk] vulnerable to Fire!"
- "Alchemical Analysis: [Armored Sentinel] resistant to Physical, vulnerable to Energy"
- "Full analysis complete! Party gains +1d10 damage vs [Blighted Horror]'s weaknesses"
- "Analysis failed—[Unknown Entity] resists identification"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Alka-hestur Overview](../alka-hestur-overview.md) | Parent specialization |
| [Payload Strike](payload-strike.md) | Deliver matched payload |
| [Perfect Solution](master-alchemist.md) | Auto-analysis at capstone |
