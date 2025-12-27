---
id: ABILITY-VARD-WARDEN-28013
title: "Rune of Shielding"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Rune of Shielding

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Ally (30 ft range) |
| **Resource Cost** | 30-35 Aether |
| **Cooldown** | 3 turns |
| **Attribute** | WILL |
| **Tags** | [Buff], [Defensive], [Anti-Corruption] |
| **Ranks** | 2 → 3 |

---

## Description

You inscribe a protective rune directly onto an ally's form, wrapping them in stable Aether. The rune absorbs incoming damage and filters out corrupting influences—a personal ward against the wasteland's hazards.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Target ally gains +2 Soak for 3 turns
- Target gains +1d10 to resist Corruption checks
- Cost: 30 Aether

**Formula:**
```
Caster.Aether -= 30

Target.AddBuff("RuneOfShielding", {
    Soak: +2,
    CorruptionResist: "+1d10",
    Duration: 3
})

Log("Rune of Shielding: {Target} gains +2 Soak, +1d10 vs Corruption (3 turns)")
```

**Tooltip:** "Rune of Shielding (Rank 2): +2 Soak, +1d10 vs Corruption. 3 turns. Cost: 30 Aether"

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Target ally gains +3 Soak for 4 turns
- Target gains +2d10 to resist Corruption checks
- Cost: 35 Aether

**Formula:**
```
Caster.Aether -= 35

Target.AddBuff("RuneOfShielding", {
    Soak: +3,
    CorruptionResist: "+2d10",
    Duration: 4
})

Log("Rune of Shielding: {Target} gains +3 Soak, +2d10 vs Corruption (4 turns)")
```

**Tooltip:** "Rune of Shielding (Rank 3): +3 Soak, +2d10 vs Corruption. 4 turns. Cost: 35 Aether"

---

## Effect Summary

| Property | R2 | R3 |
|----------|----|----|
| Soak Bonus | +2 | +3 |
| Corruption Resist | +1d10 | +2d10 |
| Duration | 3 turns | 4 turns |
| Aether Cost | 30 | 35 |

---

## Soak Mechanics

**How Soak Works:**
- Reduces incoming damage from all sources
- Applied after damage roll, before HP loss
- Example: 10 damage - 3 Soak = 7 HP lost

---

## Corruption Resistance

**When Applied:**
- Blight exposure checks
- Corruption status effects
- Corrupted enemy special attacks
- Consuming corrupted items

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Tank protection | Buff front-line ally |
| Blight areas | Protect scout exploring corruption |
| Boss fights | Reduce sustained damage |
| Corruption hazards | Resist environmental effects |

---

## Priority Targets

**Best Recipients:**
- Berserkr (takes extra damage from rage)
- Front-line warriors (constant damage)
- Allies entering corrupted zones
- Anyone targeted by Blighted enemies

---

## Combat Log Examples

- "Rune of Shielding cast on [Ally]"
- "[Ally] gains +3 Soak (4 turns)"
- "[Ally] gains +2d10 vs Corruption (4 turns)"
- "[Ally] takes 12 damage → 9 after Soak"
- "Rune of Shielding fades from [Ally]"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Vard-Warden Overview](../vard-warden-overview.md) | Parent specialization |
| [Glyph of Sanctuary](glyph-of-sanctuary.md) | Party-wide protection |
| [Aegis of Sanctity](aegis-of-sanctity.md) | Enhanced barrier effects |
