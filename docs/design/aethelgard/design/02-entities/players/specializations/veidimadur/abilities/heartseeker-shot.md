---
id: ABILITY-VEIDIMADUR-24008
title: "Heartseeker Shot"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Heartseeker Shot

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action (requires charge) |
| **Target** | Single Enemy (Ranged) |
| **Resource Cost** | 60 Stamina + 30 Focus |
| **Cooldown** | 4 turns |
| **Ranks** | None (full power when unlocked) |

---

## Description

A devastating shot that requires perfect concentration. Against marked, corrupted targets, you can purge the Blight from their system—violently.

---

## Mechanical Effect

**Charge Requirement:**
- Requires full turn to charge (cannot move or use other abilities)
- Next turn: Release shot

**Damage:**
- Base: 10d10 Physical damage

**Corruption Purge (vs [Marked] targets):**
- Purge 20 Corruption from target
- Deal +2 bonus damage per Corruption purged (max +40)

**Kill Refund:**
- If kills [Marked] target: Refund 30 Stamina and 15 Focus

**Formula:**
```
// Turn 1: Charge
Hunter.IsCharging = true
Hunter.CannotMove = true
Hunter.CannotUseOtherAbilities = true

// Turn 2: Release
Damage = Roll(10d10)

If Target.HasStatus("Marked"):
    CorruptionPurged = Min(Target.Corruption, 20)
    Target.Corruption -= CorruptionPurged
    Damage += CorruptionPurged * 2  // Max +40

    If Target.HP <= 0:  // Kill
        Hunter.Stamina += 30
        Hunter.Focus += 15
```

**Tooltip:** "Heartseeker Shot: Charge 1 turn, then 10d10 Physical. Vs [Marked]: Purge 20 Corruption, +2 damage per purged. Kill refunds 30 Stamina, 15 Focus. Cost: 60 Stamina, 30 Focus"

---

## Combat Log Examples

- "Heartseeker Shot charging... (cannot move or act)"
- "Heartseeker Shot released! 58 Physical damage!"
- "Heartseeker Shot vs [Marked]: Purged 20 Corruption! +40 bonus damage (total: 98)"
- "[Marked] target killed! Refunded 30 Stamina, 15 Focus."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Veiðimaðr Overview](../veidimadur-overview.md) | Parent specialization |
| [Mark for Death](mark-for-death.md) | Synergy ability |
