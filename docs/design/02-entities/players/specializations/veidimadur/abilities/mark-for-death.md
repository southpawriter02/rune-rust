---
id: ABILITY-VEIDIMADUR-24004
title: "Mark for Death"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Mark for Death

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 30 Stamina + Variable Psychic Stress |
| **Cooldown** | 3 turns |
| **Status Effects** | [Marked] |
| **Ranks** | 2 → 3 |

---

## Description

You focus your intent on a single target, observing the subtle tells of its Blighted nature. The mark calls your attacks with deadly precision.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Apply [Marked] for 3 turns
- Your attacks vs [Marked] deal +8 bonus damage
- **Psychic Stress Cost:** 5 Stress

**Formula:**
```
Target.AddStatus("Marked", Duration: 3, BonusDamage: 8, Source: Hunter)
Hunter.PsychicStress += 5
```

**Tooltip:** "Mark for Death (Rank 2): [Marked] for 3 turns. Your attacks deal +8 damage vs marked. Cost: 30 Stamina, 5 Stress"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- [Marked] duration: 4 turns
- Your attacks deal +15 bonus damage
- **NEW:** Allies gain +5 damage vs [Marked]
- **Reduced Stress Cost:** 2

**Formula:**
```
Target.AddStatus("Marked", Duration: 4, BonusDamage: 15, AllyBonus: 5, Source: Hunter)
Hunter.PsychicStress += 2
```

**Tooltip:** "Mark for Death (Rank 3): [Marked] for 4 turns. You deal +15 damage, allies deal +5. Cost: 30 Stamina, 2 Stress"

---

## Status Effect: [Marked]

| Property | Value |
|----------|-------|
| **Duration** | 3-4 turns |
| **Icon** | Target reticle |
| **Color** | Red |
| **Effects** | Hunter deals +8/+15 damage, Allies deal +5 (Rank 3) |

---

## Combat Log Examples

- "Mark for Death: [Enemy] is marked! +8 damage for 3 turns."
- "Mark for Death (Rank 3): [Enemy] is marked! +15 damage (you), +5 damage (allies) for 4 turns."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Veiðimaðr Overview](../veidimadur-overview.md) | Parent specialization |
| [Heartseeker Shot](heartseeker-shot.md) | Synergy with mark |
