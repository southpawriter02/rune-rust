# Use Case Documentation

This directory contains use case specifications for all player interactions and system behaviors in Rune and Rust.

## Overview

Use cases define the interactions between actors (players or system) and the game. Each use case documents:

- **Preconditions**: What must be true before the use case starts
- **Basic Flow**: The standard path through the use case
- **Alternative Flows**: Valid variations of the basic flow
- **Exception Flows**: Error handling and edge cases
- **Postconditions**: What is true after the use case completes
- **Business Rules**: Game rules that govern the use case

## Actors

### Player

The human user interacting with the game through commands (TUI) or controls (GUI).

### System

Automated game processes that execute without direct player input.

## Player Use Cases

Interactions initiated by the player.

| ID | Name | Version | Description |
|----|------|---------|-------------|
| [UC-001](player/UC-001-create-character.md) | Create Character | v0.0.3 | Create a new player character |
| [UC-002](player/UC-002-select-class.md) | Select Class | v0.0.4 | Choose archetype and class |
| [UC-003](player/UC-003-navigate-dungeon.md) | Navigate Dungeon | v0.0.2 | Move between rooms |
| [UC-004](player/UC-004-examine-environment.md) | Examine Environment | v0.0.2 | Look at room, items, monsters |
| [UC-005](player/UC-005-engage-in-combat.md) | Engage in Combat | v0.0.2 | Attack monsters |
| [UC-006](player/UC-006-use-ability.md) | Use Ability | v0.0.4 | Activate class ability |
| [UC-007](player/UC-007-manage-inventory.md) | Manage Inventory | v0.0.2 | Pick up, drop, view items |
| [UC-008](player/UC-008-use-item.md) | Use Item | v0.0.2 | Consume or activate item |
| [UC-009](player/UC-009-equip-item.md) | Equip Item | v0.0.7 | Equip weapon or armor |
| [UC-010](player/UC-010-level-up.md) | Level Up | v0.0.8 | Gain level and improvements |
| [UC-011](player/UC-011-roll-dice.md) | Roll Dice | v0.0.5 | Perform direct dice roll |
| [UC-012](player/UC-012-skill-check.md) | Skill Check | v0.0.5 | Attempt skill-based action |
| [UC-013](player/UC-013-flee-combat.md) | Flee Combat | v0.0.6 | Escape from combat |
| [UC-014](player/UC-014-collect-loot.md) | Collect Loot | v0.0.9 | Gather dropped items/currency |

## System Use Cases

Automated behaviors triggered by game events.

| ID | Name | Version | Description |
|----|------|---------|-------------|
| [UC-101](system/UC-101-process-turn-end.md) | Process Turn End | v0.0.4 | Execute all turn-end effects |
| [UC-102](system/UC-102-regenerate-resources.md) | Regenerate Resources | v0.0.4 | Apply resource regen/decay |
| [UC-103](system/UC-103-reduce-cooldowns.md) | Reduce Cooldowns | v0.0.4 | Tick down ability cooldowns |
| [UC-104](system/UC-104-monster-turn.md) | Monster Turn | v0.0.6 | Execute monster AI decision |
| [UC-105](system/UC-105-apply-status-effects.md) | Apply Status Effects | v0.0.6 | Process active status effects |
| [UC-106](system/UC-106-generate-loot.md) | Generate Loot | v0.0.9 | Create loot from defeated monster |
| [UC-107](system/UC-107-calculate-damage.md) | Calculate Damage | v0.0.9 | Apply damage types/resistances |
| [UC-108](system/UC-108-roll-initiative.md) | Roll Initiative | v0.0.6 | Determine combat turn order |
| [UC-109](system/UC-109-award-experience.md) | Award Experience | v0.0.8 | Grant XP for defeating monsters |
| [UC-110](system/UC-110-spawn-monster.md) | Spawn Monster | v0.0.9 | Create monster from definition |

## Use Case Template

Each use case follows this structure:

```markdown
# UC-XXX: [Title]

**Actor:** Player | System
**Priority:** High | Medium | Low
**Version:** v0.0.X
**Status:** Implemented

## Description
[Brief description]

## Preconditions
- [Condition 1]
- [Condition 2]

## Basic Flow
1. [Step 1]
2. [Step 2]

## Alternative Flows
### AF-1: [Name]
1. [Step]

## Exception Flows
### EF-1: [Name]
1. [Trigger]
2. [Handling]

## Postconditions
- [Result 1]

## Business Rules
- [Rule 1]

## Related Use Cases
- UC-XXX: [Related]
```

## Related Documentation

- [Architecture Decision Records](../architecture/README.md)
- [Implementation Specifications](../v0.0.x/implementation-specifications/)
