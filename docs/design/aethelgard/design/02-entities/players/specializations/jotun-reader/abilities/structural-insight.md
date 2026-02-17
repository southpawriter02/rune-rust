---
id: ABILITY-JOTUNREADER-206
title: "Structural Insight"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Structural Insight

**Type:** Passive | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Free Action (always active) |
| **Target** | Environment |
| **Resource Cost** | None |
| **Ranks** | 2 → 3 |

---

## Description

"Support beams compromised. Eastern wall provides solid cover—load-bearing, reinforced. Center of room will collapse." You read structural integrity like others read expressions.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Auto-detect [Structurally Unstable] features in current room
- Auto-detect [Cover] quality ratings
- Auto-detect environmental hazards in current room
- Once per combat: Warning grants allies +2d10 to defensive checks vs hazard

**Formula:**
```
AutoDetect: [Structurally Unstable], [Cover], [Hazards] in CurrentRoom
DetectionRange = CurrentRoom

OncePerCombat:
    If HazardTrigger:
        Allies.DefensiveCheck.BonusDice += 2d10
```

**Tooltip:** "Structural Insight (Rank 2): Auto-detect structural features, cover, and hazards in current room. 1/combat: +2d10 vs hazard."

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Detection extends to entire dungeon floor
- Once per combat: Call controlled collapse (Standard Action + ally attack)
- **NEW:** Auto-warn of ambush (cannot be surprised)
- **NEW:** Party gains +1 Defense in all analyzed areas

**Formula:**
```
AutoDetect: [Structurally Unstable], [Cover], [Hazards] in EntireFloor
DetectionRange = DungeonFloor

OncePerCombat:
    ControlledCollapse: StandardAction + AllyCanAttack
    // Triggers environmental damage in targeted area

Party.Defense += 1 in AnalyzedAreas
AmbushImmunity = true  // Cannot be surprised
```

**Tooltip:** "Structural Insight (Rank 3): Detect hazards floor-wide. 1/combat: Trigger controlled collapse. +1 Defense in analyzed areas. Ambush immunity."

---

## Combat/Exploration Examples

- "Structural Insight detects: Unstable ceiling (center), High Cover (eastern wall), Fire Hazard (north alcove)"
- "WARNING! [Ally] gains +2d10 Defense vs collapsing pillar!"
- "Structural Insight maps entire floor: 3 unstable areas, 7 cover positions, 2 hidden hazards"
- "[Jötun-Reader] triggers controlled collapse! [Ally] follows up with attack!"
- "Ambush detected! Party cannot be surprised (Structural Insight)"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Jötun-Reader Overview](../jotun-reader-overview.md) | Parent specialization |
| [Room Engine: Spatial Layout](../../../../07-environment/room-engine/spatial-layout.md) | Environment integration |
