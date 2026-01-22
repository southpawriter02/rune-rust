---
id: ABILITY-EINBUI-27013
title: "Resourceful Eye"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Resourceful Eye

**Type:** Active | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action (out of combat) |
| **Target** | Current Room |
| **Resource Cost** | 20 Stamina |
| **Check** | WITS + Wasteland Survival |
| **Tags** | [Exploration], [Detection] |
| **Ranks** | 2 → 3 |

---

## Description

Your trained eye spots what others miss—hidden caches, resource nodes, and the subtle signs of concealed passages. Nothing stays hidden from the Einbui for long.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Make WITS + Wasteland Survival check (DC 12)
- Success: Reveal all hidden Resource Nodes in current room
- Failure: No resources found (can retry after rest)

**Formula:**
```
Caster.Stamina -= 20
CheckResult = Roll(WITS + WastelandSurvival)

If CheckResult >= 12:
    For each HiddenNode in CurrentRoom:
        HiddenNode.Reveal()
    Log("Resourceful Eye reveals {Count} hidden resource nodes!")
Else:
    Log("Resourceful Eye: No hidden resources detected.")
```

**Tooltip:** "Resourceful Eye (Rank 2): WITS + Survival DC 12 to reveal hidden Resource Nodes in room. Cost: 20 Stamina."

---

### Rank 3 (Unlocked: Train Capstone)

**Effect:**
- Make WITS + Wasteland Survival check (DC 10)
- Success: Reveal all hidden Resource Nodes, hidden passages, AND traps
- Failure: No resources found (can retry after rest)

**Formula:**
```
Caster.Stamina -= 20
CheckResult = Roll(WITS + WastelandSurvival)

If CheckResult >= 10:
    For each HiddenNode in CurrentRoom:
        HiddenNode.Reveal()
    For each HiddenPassage in CurrentRoom:
        HiddenPassage.Reveal()
    For each HiddenTrap in CurrentRoom:
        HiddenTrap.Reveal()
    Log("Resourceful Eye reveals nodes, passages, and traps!")
Else:
    Log("Resourceful Eye: Nothing detected.")
```

**Tooltip:** "Resourceful Eye (Rank 3): WITS + Survival DC 10 to reveal Resources, Passages, and Traps."

---

## Rank Comparison

| Rank | DC | Reveals |
|------|-----|---------|
| 2 | 12 | Resource Nodes only |
| 3 | 10 | Resource Nodes + Passages + Traps |

---

## Resource Node Types

| Node Type | Contents |
|-----------|----------|
| Herb Cache | 1-3 [Common Herb] |
| Salvage Pile | [Scrap Metal], [Clean Cloth] |
| Water Source | Refill waterskins |
| Hidden Stash | Random supplies |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| New room entry | Always scan before proceeding |
| Resource shortage | Find materials for crafting |
| Dungeon exploration | Detect traps before triggering |
| Shortcut finding | Reveal hidden passages |

---

## Combat Log Examples

- "Resourceful Eye: WITS + Survival check... 14 vs DC 12. Success!"
- "Resourceful Eye reveals 2 hidden resource nodes in Ruined Chapel"
- "Resourceful Eye (Rank 3) reveals: 1 Resource Node, 1 Hidden Passage, 2 Traps"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Einbui Overview](../einbui-overview.md) | Parent specialization |
| [Room Engine Core](../../../../07-environment/room-engine/core.md) | Hidden object system |
| [Wasteland Survival](../../../../01-core/skills/wasteland-survival.md) | Associated skill |
