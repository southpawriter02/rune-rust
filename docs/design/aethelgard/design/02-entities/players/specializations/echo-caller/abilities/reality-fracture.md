---
id: ABILITY-ECHO-CALLER-28014
title: "Reality Fracture"
version: 1.0
status: approved
last-updated: 2025-12-14
---

# Reality Fracture

**Type:** Active [Echo] | **Tier:** 2 | **PP Cost:** 4

---

## Overview

| Property | Value |
|----------|-------|
| **Action** | Standard Action |
| **Target** | Single Enemy |
| **Resource Cost** | 40 Aether |
| **Damage Type** | Psychic |
| **Status Effects** | [Disoriented], Push |
| **Tags** | [Echo] |
| **Ranks** | 2 → 3 |

---

## Description

You fracture the target's perception of reality, shattering their sense of space and direction. They stagger back, uncertain of what is real.

---

## Rank Progression

### Rank 2 (Starting Rank - When ability is learned)

**Effect:**
- Deal 3d8 Psychic damage
- Apply [Disoriented] for 2 turns
- Push target 3 tiles in chosen direction

**Formula:**
```
Damage = Roll(3d8)
Target.AddStatus("Disoriented", Duration: 2)
Target.Push(Direction, Distance: 3)
```

**Tooltip:** "Reality Fracture (Rank 2): 3d8 Psychic + [Disoriented] 2 turns + Push 3 tiles. Cost: 40 Aether"

---

### Rank 3 (Upgrade Cost: +3 PP, requires Rank 2)

**Effect:**
- Deal 4d8 Psychic damage
- Apply [Disoriented] for 2 turns
- Push target 3 tiles
- **[Echo Chain]:** Adjacent enemy also Pushed 2 tiles

**Formula:**
```
Damage = Roll(4d8)
Target.AddStatus("Disoriented", Duration: 2)
Target.Push(Direction, Distance: 3)

// Echo Chain
If AdjacentEnemy exists:
    AdjacentEnemy.Push(Direction, Distance: 2)
    Log("Echo Chain: Adjacent enemy pushed!")
```

**Tooltip:** "Reality Fracture (Rank 3): 4d8 Psychic + [Disoriented] + Push 3. Echo Chain: Push adjacent 2 tiles."

---

## [Disoriented] Status Effect

| Property | Value |
|----------|-------|
| **Duration** | 2 turns |
| **Effects** | -2 dice to Accuracy |
| | Cannot use complex abilities |

---

## Tactical Applications

| Scenario | Use Case |
|----------|----------|
| Enemy at choke point | Push into hazard |
| Back row threat | Push to front row |
| Multiple targets | Chain push disrupts formation |

---

## Combat Log Examples

- "Reality Fracture: 14 Psychic damage to [Enemy]!"
- "[Enemy] is [Disoriented] for 2 turns"
- "[Enemy] pushed 3 tiles back!"
- "Echo Chain: [Adjacent Enemy] pushed 2 tiles!"

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [Echo-Caller Overview](../echo-caller-overview.md) | Parent specialization |
| [Disoriented Status](../../../../04-systems/status-effects/disoriented.md) | Status effect details |
