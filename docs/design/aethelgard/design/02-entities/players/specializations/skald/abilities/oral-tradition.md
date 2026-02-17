---
id: ABILITY-SKALD-28001
title: "Oral Tradition"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Oral Tradition

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Performance** | No |
| **Ranks** | 1 → 2 → 3 |

---

## Description

The great sagas are part of the Skald's very being, carried in verse and cadence. You remember what others have forgotten.

---

## Rank Progression

### Rank 1 (Starting Rank - When ability is learned)

**Effect:**
- +1d10 to Rhetoric checks
- +1d10 to investigate checks for historical lore

**Formula:**
```
RhetoricCheckPool += 1d10
HistoricalLoreCheckPool += 1d10
```

**Tooltip:** "Oral Tradition (Rank 1): +1d10 to Rhetoric and historical lore checks."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- +2d10 to Rhetoric and lore checks

**Formula:**
```
RhetoricCheckPool += 2d10
HistoricalLoreCheckPool += 2d10
```

**Tooltip:** "Oral Tradition (Rank 2): +2d10 to Rhetoric and historical lore checks."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +2d10 to Rhetoric and lore checks
- **NEW:** Can recall any historical fact with DC 15 WITS check

**Formula:**
```
RhetoricCheckPool += 2d10
HistoricalLoreCheckPool += 2d10
PerfectRecall = true  // DC 15 WITS for any historical fact
```

**Tooltip:** "Oral Tradition (Rank 3): +2d10 checks. DC 15 WITS to recall any historical fact."

---

## Combat Log Examples

- "Oral Tradition: +1d10 to Rhetoric check"
- "Oral Tradition: +2d10 to investigate historical records"
- "Oral Tradition (Rank 3): Recalling ancient battle tactics... DC 15 WITS check"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Skald Overview](../skald-overview.md) | Parent specialization |
