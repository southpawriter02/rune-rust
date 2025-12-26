---
id: ABILITY-SKALD-28006
title: "Enduring Performance"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Enduring Performance

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

Honed vocal endurance allows you to maintain powerful performances far longer than others, and eventually, to weave multiple sagas simultaneously.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- All Performance durations increased by +3 rounds

**Formula:**
```
For all PerformanceAbilities:
    Duration += 3
```

**Tooltip:** "Enduring Performance (Rank 2): +3 rounds to all Performance durations."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +4 rounds to Performance durations
- **NEW:** Can maintain 2 Performances simultaneously

**Formula:**
```
For all PerformanceAbilities:
    Duration += 4
MaxSimultaneousPerformances = 2
```

**Tooltip:** "Enduring Performance (Rank 3): +4 rounds duration. Can maintain 2 Performances at once."

---

## Dual Performance Rules (Rank 3)

When maintaining two performances:
- Must pay both Stamina costs
- Must spend 2 Standard Actions to initiate both (one per turn)
- Both can be interrupted independently
- Both use the same WILL-based duration (+ Enduring bonus)

**Example:**
1. Turn 1: Begin Saga of Courage (40 Stamina)
2. Turn 2: Begin Dirge of Defeat (40 Stamina)
3. Turn 3+: Both performances active, cannot take other Standard Actions

---

## Duration Examples

| WILL | Base | +Rank 2 | +Rank 3 |
|------|------|---------|---------|
| 3 | 3 rounds | 6 rounds | 7 rounds |
| 4 | 4 rounds | 7 rounds | 8 rounds |
| 5 | 5 rounds | 8 rounds | 9 rounds |
| 6 | 6 rounds | 9 rounds | 10 rounds |

---

## Combat Log Examples

- "Enduring Performance: Saga of Courage duration extended to 8 rounds"
- "Enduring Performance (Rank 3): Beginning second performance..."
- "Dual Performance active: Saga of Courage + Dirge of Defeat"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skald Overview](../skald-overview.md) | Parent specialization |
| [Saga of Courage](saga-of-courage.md) | Affected performance |
| [Dirge of Defeat](dirge-of-defeat.md) | Affected performance |
