---
id: ABILITY-HLEKKR-25010
title: "Pragmatic Preparation I"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Pragmatic Preparation I

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Passive (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Attribute** | FINESSE |
| **Tags** | [Trap], [Control] |
| **Ranks** | 1 → 2 → 3 |

---

## Description

A true chain-master prepares the battlefield before combat begins. Your expertise with traps and bindings extends the duration of all control effects you apply.

---

## Rank Progression

### Rank 1 (Starting Rank)

**Effect:**
- +1d10 to FINESSE checks for setting/disarming traps
- All [Rooted] and [Slowed] effects you apply last +1 turn longer

**Formula:**
```
OnTrapCheck(FINESSE):
    BonusDice += 1  // d10s

OnApplyStatus("Rooted", "Slowed"):
    Duration += 1
```

**Tooltip:** "Pragmatic Preparation I (Rank 1): +1d10 to trap checks. Control effects +1 turn duration."

---

### Rank 2 (Unlocked: Train any Tier 2 ability)

**Effect:**
- +2d10 to FINESSE checks for setting/disarming traps
- All [Rooted] and [Slowed] effects last +1 turn longer

**Formula:**
```
OnTrapCheck(FINESSE):
    BonusDice += 2

OnApplyStatus("Rooted", "Slowed"):
    Duration += 1
```

**Tooltip:** "Pragmatic Preparation I (Rank 2): +2d10 to trap checks. Control +1 turn."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- +3d10 to FINESSE checks for setting/disarming traps
- All control effects ([Rooted], [Slowed], [Stunned], [Seized]) last +2 turns longer

**Formula:**
```
OnTrapCheck(FINESSE):
    BonusDice += 3

OnApplyStatus("Rooted", "Slowed", "Stunned", "Seized"):
    Duration += 2
```

**Tooltip:** "Pragmatic Preparation I (Rank 3): +3d10 trap checks. ALL control effects +2 turns."

---

## Duration Scaling

| Status Effect | Base | Rank 1/2 | Rank 3 |
|---------------|------|----------|--------|
| Netting Shot [Rooted] | 2 turns | 3 turns | 4 turns |
| Chain Scythe [Slowed] | 3 turns | 4 turns | 5 turns |
| Shattering Wave [Stunned] | 1 turn | 1 turn | 3 turns |
| Unyielding Grip [Seized] | 2 turns | 2 turns | 4 turns |

---

## Combat Log Examples

- "Pragmatic Preparation: +2d10 to trap disarm check"
- "[Rooted] duration extended: 2 → 3 turns (Pragmatic Preparation)"
- "Rank 3: [Stunned] extended +2 turns (total: 3)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Hlekkr-Master Overview](../hlekkr-master-overview.md) | Parent specialization |
| [Netting Shot](netting-shot.md) | Synergizes with duration |
| [Chain Scythe](chain-scythe.md) | Synergizes with duration |
