---
id: ABILITY-JOTUNREADER-207
title: "Calculated Triage"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Calculated Triage

**Type:** Passive | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Adjacent Allies |
| **Resource Cost** | None |
| **Ranks** | None (full power when unlocked) |

---

## Description

"Apply pressure to brachial artery first. Follow wound track with the applicator." Your clinical guidance optimizes treatment. You don't heal—you make healers more effective.

---

## Mechanical Effect

**Passive Component (Always Active):**
- Healing consumables used on adjacent allies heal +50% more
- Range: 2 squares from Jötun-Reader
- Also removes one minor debuff ([Bleeding], [Poisoned], [Disoriented])

**Active Component (Once per combat):**
- Activate "Field Hospital" zone (3x3 area, 3 rounds)
- +75% healing in zone
- +2 Resolve for characters in zone
- Healing actions cost half as much time

**Formula:**
```
If Ally.IsAdjacent(JotunReader, Range: 2):
    HealingReceived *= 1.50  // +50%
    RemoveDebuff([Bleeding, Poisoned, Disoriented], Count: 1)

OncePerCombat FieldHospital:
    Create Zone(3x3, Duration: 3 rounds)
    Zone.HealingBonus = 0.75  // +75%
    Zone.ResolveBonus = +2
    Zone.HealingActionCost *= 0.5
```

**Tooltip:** "Calculated Triage: Adjacent allies receive +50% healing, cleanse 1 minor debuff. 1/combat: Field Hospital zone (+75% healing, +2 Resolve)."

---

## Combat Log Examples

- "Calculated Triage: [Ally]'s healing increased by 50% (healed 45 → 68)"
- "[Bleeding] cleansed by Calculated Triage"
- "FIELD HOSPITAL activated! 3x3 zone provides +75% healing, +2 Resolve for 3 rounds"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Jötun-Reader Overview](../jotun-reader-overview.md) | Parent specialization |
| [Bone-Setter Overview](../../bone-setter/bone-setter-overview.md) | Synergy specialization |
| [Bleeding](../../../../04-systems/status-effects/bleeding.md) | Cleansable debuff |
