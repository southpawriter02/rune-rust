---
id: ABILITY-SEIDKONA-27008
title: "Ride the Echoes"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Ride the Echoes

**Type:** Active | **Tier:** 3 | **PP Cost:** 5

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Self |
| **Resource Cost** | Aether |
| **Corruption Cost** | +1-2 |
| **Ranks** | None (full power when unlocked) |

---

## Description

You slip between the cracks in reality, riding the psychic echoes to emerge elsewhere. The journey leaves its mark.

---

## Mechanical Effect

**Base Effect:**
- Instantly teleport to any unoccupied tile
- No line-of-sight required
- **COST: +2 Runic Blight Corruption**

**Formula:**
```
Caster.Position = TargetTile  // No LOS required
Caster.Corruption += 2
```

**Tooltip:** "Ride the Echoes: Teleport anywhere (no LOS). COST: +2 Corruption"

---

## Rank-Like Progression (Tree-Based)

While this ability has no explicit ranks, upgrades occur through tree progression:

| Condition | Effect |
|-----------|--------|
| Base | +2 Corruption cost |
| With Capstone | +1 Corruption (reduced) |
| Capstone + investment | Can bring one adjacent ally (+1 Corruption for them) |

**With Full Tree:**
```
Caster.Position = TargetTile
Caster.Corruption += 1  // Reduced

If BringingAlly:
    Ally.Position = AdjacentToTargetTile
    Ally.Corruption += 1
```

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Escape | Teleport away from danger |
| Positioning | Reach optimal back row |
| Rescue | Extract ally from danger (with upgrade) |
| Ambush | Appear behind enemy lines |
| Puzzle solving | Bypass physical barriers |

---

## Combat Log Examples

- "Ride the Echoes: Teleporting to [location]..."
- "+2 Corruption (Ride the Echoes cost)"
- "Ride the Echoes (upgraded): Bringing [Ally] along. +1 Corruption each."

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Seiðkona Overview](../seidkona-overview.md) | Parent specialization |
| [Corruption System](../../../../01-core/resources/coherence.md) | Corruption mechanics |
