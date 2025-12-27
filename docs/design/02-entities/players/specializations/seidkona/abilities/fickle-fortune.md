---
id: ABILITY-SEIDKONA-27006
title: "Fickle Fortune"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Fickle Fortune

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Ranks** | 2 → 3 |

---

## Description

The spirits favor you—or perhaps they simply enjoy watching. Either way, their bargains come through more often.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- All [Spirit Bargain] trigger chances increased by +15%

**Formula:**
```
For all SpiritBargainAbilities:
    TriggerChance += 0.15
```

**Tooltip:** "Fickle Fortune (Rank 2): +15% to all Spirit Bargain chances."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +25% to all Spirit Bargain chances
- **NEW:** Once per combat, force a failed bargain to succeed

**Formula:**
```
For all SpiritBargainAbilities:
    TriggerChance += 0.25
ForceSuccessUses = 1  // Per combat
```

**Tooltip:** "Fickle Fortune (Rank 3): +25% bargain chance. Once/combat: force bargain success."

---

## Spirit Bargain Chances with Fickle Fortune

| Ability Base | Rank 2 (+15%) | Rank 3 (+25%) |
|--------------|---------------|---------------|
| 25% | 40% | 50% |
| 30% | 45% | 55% |
| 35% | 50% | 60% |

---

## Forced Success Mechanic

At Rank 3, when a Spirit Bargain fails to trigger:
- You may activate Fickle Fortune to force it to succeed
- This uses your one-per-combat charge
- Useful for critical moments

---

## Combat Log Examples

- "Fickle Fortune: Spirit Bargain chance boosted to 40%"
- "Spirit Bargain failed... FICKLE FORTUNE ACTIVATED! Bargain succeeds!"
- "Fickle Fortune (Rank 3): +25% to all bargains this combat"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Seiðkona Overview](../seidkona-overview.md) | Parent specialization |
| [Echo of Vigor](echo-of-vigor.md) | Affected ability |
| [Echo of Misfortune](echo-of-misfortune.md) | Affected ability |
