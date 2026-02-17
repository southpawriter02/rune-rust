---
id: ABILITY-EINBUI-27014
title: "Radical Self-Reliance II"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Radical Self-Reliance II

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Prerequisites** | Radical Self-Reliance I |
| **Tags** | [Survival], [Movement] |
| **Ranks** | 2 → 3 |

---

## Description

Your self-reliance extends beyond mere survival. You move through the wasteland like a ghost—unseen, unheard, scaling obstacles that stop lesser travelers.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- +1 die to Acrobatics (Stealth) checks
- +1 die to Acrobatics (Climbing) checks

**Formula:**
```
OnSkillCheck(Acrobatics, Stealth):
    BonusDice += 1

OnSkillCheck(Acrobatics, Climbing):
    BonusDice += 1
```

**Tooltip:** "Radical Self-Reliance II (Rank 2): +1 die to Stealth and Climbing checks."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- +2 dice to Acrobatics (Stealth) checks
- +2 dice to Acrobatics (Climbing) checks
- **NEW:** Climb at full movement speed (no penalty)

**Formula:**
```
OnSkillCheck(Acrobatics, Stealth):
    BonusDice += 2

OnSkillCheck(Acrobatics, Climbing):
    BonusDice += 2

OnClimbing:
    MovementPenalty = 0  // Normally 50% movement
```

**Tooltip:** "Radical Self-Reliance II (Rank 3): +2 dice to Stealth/Climbing. Climb at full speed."

---

## Combined Self-Reliance Bonuses

With both abilities at Rank 3:

| Skill Check | Total Bonus |
|-------------|-------------|
| Tracking | +2 dice, auto-succeed DC 10 |
| Foraging | +2 dice, auto-succeed DC 10 |
| Stealth | +2 dice |
| Climbing | +2 dice, full speed |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Scouting | Stealth ahead of party |
| Vertical terrain | Scale cliffs without slowing |
| Ambush setup | Position unseen |
| Escape routes | Climb to safety quickly |

---

## Combat Log Examples

- "Radical Self-Reliance II: +2 dice to Stealth check"
- "Climbing with full movement speed (Rank 3 bonus)"
- "Stealth check: 4 base + 2 bonus = 6 dice"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Einbui Overview](../einbui-overview.md) | Parent specialization |
| [Acrobatics Skill](../../../../01-core/skills/acrobatics.md) | Associated skill |
| [Radical Self-Reliance I](radical-self-reliance-i.md) | Prerequisite ability |
