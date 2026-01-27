---
id: ABILITY-VEIDIMADUR-24001
title: "Wilderness Acclimation I"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Wilderness Acclimation I

**Type:** Passive | **Tier:** 1 | **PP Cost:** 3

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Self |
| **Resource Cost** | None |
| **Attribute** | WITS |
| **Ranks** | 1 → 2 → 3 |

---

## Description

You've learned to read the subtle signs of the corrupted wilderness. Tracks, spoor, and the faint shimmer of Blight reveal what others cannot see.

---

## Rank Progression

### Rank 1 (Base — included with ability unlock)

**Effect:**
- +1d10 bonus to WITS-based checks for tracking, foraging, perceiving hidden/ambushing creatures
- Can identify Blighted creatures by spoor

**Formula:**
```
TrackingCheckPool = WITS + 1d10
ForagingCheckPool = WITS + 1d10
PerceptionCheckPool = WITS + 1d10 (vs hidden/ambush)
CanIdentifyBlightedBySpoor = true
```

**Tooltip:** "Wilderness Acclimation I (Rank 1): +1d10 to tracking, foraging, perceiving hidden creatures. Identify Blighted by spoor."

---

### Rank 2 (Upgrade Cost: +2 PP)

**Effect:**
- +2d10 bonus to tracking/foraging/perception checks
- Can estimate corruption level (Low/Medium/High/Extreme)

**Formula:**
```
TrackingCheckPool = WITS + 2d10
CorruptionEstimation = true  // Returns: Low, Medium, High, Extreme
```

**Tooltip:** "Wilderness Acclimation I (Rank 2): +2d10 to tracking/foraging/perception. Estimate corruption level."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- +3d10 bonus to tracking/foraging/perception checks
- **NEW:** Automatically detect [Blighted] items without touching them

**Formula:**
```
TrackingCheckPool = WITS + 3d10
AutoDetectBlightedItems = true  // No contact required
```

**Tooltip:** "Wilderness Acclimation I (Rank 3): +3d10 to tracking/foraging/perception. Auto-detect Blighted items."

---

## Exploration Examples

- "You identify tracks belonging to a Blighted creature."
- "Corruption level estimated: High (60-89)"
- "WARNING: That item radiates Blight corruption."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Veiðimaðr Overview](../veidimadur-overview.md) | Parent specialization |
