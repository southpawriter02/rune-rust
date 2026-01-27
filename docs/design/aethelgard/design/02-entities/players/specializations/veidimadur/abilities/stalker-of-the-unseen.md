---
id: ABILITY-VEIDIMADUR-24009
title: "Stalker of the Unseen"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Stalker of the Unseen

**Type:** Hybrid (Passive + Active) | **Tier:** 4 (Capstone) | **PP Cost:** 6

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive / Toggle (Stance) |
| **Target** | Self |
| **Resource Cost** | 15 Stamina/turn (stance maintenance) |
| **Ranks** | None (full power when unlocked) |
| **Special** | Training upgrades all Tier 1 & 2 abilities to Rank 3 |

---

## Description

You have become one with the hunt. You see through the Blight's deceptions, and your prey cannot escape.

---

## Passive Component (Always Active)

**Auto-Learn on Mark:**
- When you use Mark for Death, automatically learn target's Vulnerabilities and precise Corruption level

**Formula:**
```
OnMarkForDeath(Target):
    Reveal(Target.Vulnerabilities)
    Reveal(Target.ExactCorruption)  // Not just Low/Medium/High/Extreme
```

---

## Active Component: Blight-Stalker's Stance (Toggle)

**Activation:** Toggle on/off as Free Action

**While Active:**
- Immune to visual impairment effects (Blindness, Obscured, etc.)
- Aimed Shots vs High/Extreme Corruption targets have 90% chance to inflict [Staggered]
- +2d10 to attack rolls vs corrupted targets
- **Maintenance Cost:** 15 Stamina per turn
- **Deactivation Penalty:** Gain 5 Psychic Stress when stance ends

**Formula:**
```
// Stance Active
Hunter.Immunity.Add("VisualImpairment")

If Target.Corruption >= 60:  // High or Extreme
    If Roll(1d100) <= 90:
        Target.AddStatus("Staggered", Duration: 1)

AttackRollPool += 2d10 (vs corrupted)

// Per Turn
Hunter.Stamina -= 15

// On Deactivation
Hunter.PsychicStress += 5
```

**Tooltip:** "Blight-Stalker's Stance (Toggle): Immune to visual impairment. +2d10 attack vs corrupted. 90% Stagger vs High+ Corruption. Costs 15 Stamina/turn. +5 Stress when deactivated."

---

## Combat Log Examples

- "Mark for Death: Auto-learned vulnerabilities (Fire, Lightning) and Corruption (73)"
- "Blight-Stalker's Stance activated!"
- "Aimed Shot: 90% Stagger triggered! [Enemy] is Staggered."
- "Blight-Stalker's Stance: -15 Stamina (maintenance)"
- "Blight-Stalker's Stance deactivated. +5 Psychic Stress."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Veiðimaðr Overview](../veidimadur-overview.md) | Parent specialization |
| [Mark for Death](mark-for-death.md) | Triggers passive |
