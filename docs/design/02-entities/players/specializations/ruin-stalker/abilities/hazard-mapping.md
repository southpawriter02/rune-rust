---
id: SPEC-ABILITY-4004
title: "Hazard Mapping"
parent: ruin-stalker
tier: 2
type: active
version: 1.0
---

# Hazard Mapping

**Ability ID:** 4004 | **Tier:** 2 | **Type:** Active | **PP Cost:** 4

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Room/Corridor |
| **Resource Cost** | 25 Stamina + 1 Mapping Component |
| **Prerequisite** | 8 PP in Ruin-Stalker tree |
| **Starting Rank** | 2 |

---

## 2. Description

> "You mark the dangers for those who follow. A chalk symbol here, a cord there—your map saves lives."

---

## 3. Mechanical Effects

### 3.1 Primary Effect

```
Survey = Current room/corridor
Create = Hazard Map (document)
Party bonus = +2d10 vs marked hazards
Duration = Current exploration session
```

---

## 4. Rank Progression

### Rank 2 (Starting Rank)

**Mechanical Effects:**
- Survey room/corridor
- Create Hazard Map
- Party: +2d10 vs marked hazards
- Cost: 25 Stamina + 1 Mapping Component

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Mechanical Effects:**
- Map persists between sessions
- Can be shared with other parties
- +3d10 bonus
- **NEW:** Reveals optimal extraction routes
- **NEW:** Reveals hidden caches

---

## 5. Hazard Map Contents

| Information | Rank 2 | Rank 3 |
|-------------|--------|--------|
| Trap locations | ✓ | ✓ |
| Anomaly positions | ✓ | ✓ |
| Danger ratings | ✓ | ✓ |
| Hidden passages | ✗ | ✓ |
| Extraction routes | ✗ | ✓ |

---

## 6. Balance Data

### 6.1 Economy
- **Stamina:** 25 Stamina is moderate.
- **Component:** 1 Mapping Component (Cost ~10-20 HS).
- **Return:** +2d10 on checks for WHOLE party. Massive efficiency for Boss rooms or complex trap gauntlets.

### 6.2 Data Value
- **Info:** Knowing extraction routes (Rank 3) changes the meta-game from "Survival" to "Optimization".

---

## 7. Phased Implementation Guide

### Phase 1: Mechanics
- [ ] **Action**: Create `CreateMap` capability.
- [ ] **Data**: Define `HazardMap` class containing list of `KnownElements`.
- [ ] **Bonus**: Hook `CheckSystem` to look for valid map in inventory/active list.

### Phase 2: Logic Integration
- [ ] **Survey**: Scan room for all Interactables/Traps -> Add to Map Knowledge.
- [ ] **Persist**: Save Map data to `SaveGame` (Rank 3).
- [ ] **Share**: Allow item transfer of Map Item to other players/NPCs.

### Phase 3: Visuals
- [ ] **UI**: Minimap overlay showing icons for hazards.
- [ ] **Item**: Physical "Scroll" item in inventory.

---

## 8. Testing Requirements

### 8.1 Unit Tests
- [ ] **Create**: Consumes Stamina/Component -> Returns Map Item.
- [ ] **Bonus**: Party Member check -> HasMap? -> Adds 2d10.
- [ ] **Content**: Map contains list of all local traps.

### 8.2 Integration Tests
- [ ] **Persistence**: Save game -> Load game -> Map still exists (Rank 3).
- [ ] **Session**: Rank 2 map expires on dungeon exit.

### 8.3 Manual QA
- [ ] **Visual**: Check minimap updates when map is used.

---

## 9. Logging Requirements

**Reference:** [logging.md](../../../../../00-project/logging.md)

### 9.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Map | Info | "Mapped {Room}. Discovered {Count} hazards." | `Room`, `Count` |
| Bonus | Debug | "Map guidance provided +2d10." | - |

---

## 10. Related Specifications
| Document | Purpose |
|----------|---------|
| [Anomaly Sense](anomaly-sense-i.md) | Detection source |
| [Room Engine](../../../../07-environment/room-engine/core.md) | Map data source |

---

## 11. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Balance, Phased Guide, Testing, Logging |
