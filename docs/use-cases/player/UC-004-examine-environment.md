# UC-004: Examine Environment

**Actor:** Player
**Priority:** Medium
**Version:** v0.0.2
**Status:** Implemented

## Description

Player examines the current room, specific items, or monsters to get detailed information. This does not consume a turn.

## Preconditions

- Active game session exists
- Player is in a room

## Basic Flow

1. Player enters "look" command
2. System displays current room information:
   - Room name and description
   - Available exits
   - Items in room
   - Monsters in room (with health if in combat)
3. No turn is consumed

## Alternative Flows

### AF-1: Examine Specific Item

1. Player enters "look at [item]" or "examine [item]"
2. System finds item in room or inventory
3. System displays item details:
   - Name and description
   - Item type
   - Effects (if consumable)
   - Stats (if equipment)
4. No turn consumed

### AF-2: Examine Monster

1. Player enters "look at [monster]" or "examine [monster]"
2. System finds monster in room
3. System displays monster details:
   - Name and description
   - Current/Max health
   - Tier and traits (if visible)
   - Damage resistances (if known)
4. No turn consumed

### AF-3: Examine Self

1. Player enters "look at self" or "examine me"
2. System displays player status:
   - Name, class, level
   - Health and resource
   - Stats (attack, defense)
   - Active status effects
4. No turn consumed

### AF-4: Examine Exits

1. Player enters "exits" or "look exits"
2. System lists available directions
3. May include brief description of each direction
4. No turn consumed

## Exception Flows

### EF-1: Item Not Found

1. At step 2 (AF-1): Item not in room or inventory
2. System displays "You don't see that here"
3. No turn consumed

### EF-2: Monster Not Found

1. At step 2 (AF-2): Monster not in room
2. System displays "There's no [monster] here"
3. No turn consumed

### EF-3: Ambiguous Target

1. At step 2: Multiple items/monsters match query
2. System displays "Which one? There are multiple [name]s here"
3. Player must specify more precisely

## Postconditions

- Information displayed to player
- No game state changed
- No turn consumed

## Business Rules

- Examining does not consume turns
- Examining is always available (even in combat)
- Room description always includes exits
- Monster health shown as current/max in combat
- Item stats shown for equipment items
- Hidden information may require higher perception (future)

## UI Display

### Room Look

```
╭─────────────────────────────────────╮
│         TREASURE CHAMBER            │
├─────────────────────────────────────┤
│                                     │
│  A glittering chamber filled with   │
│  ancient treasures. Gold coins      │
│  carpet the floor.                  │
│                                     │
│  Exits: [South] [Down]              │
│                                     │
│  Monsters:                          │
│    - Dragon Hatchling (45/50 HP)    │
│                                     │
│  Items:                             │
│    - Gold Coins (x100)              │
│    - Ruby Amulet                    │
│                                     │
╰─────────────────────────────────────╯
```

### Monster Examine

```
╭─────────────────────────────────────╮
│         DRAGON HATCHLING            │
├─────────────────────────────────────┤
│                                     │
│  A young dragon, scales gleaming    │
│  with an inner fire. Its eyes       │
│  watch you with predatory intent.   │
│                                     │
│  Health: 45/50                      │
│  Tier: Elite                        │
│  Traits: Fire Breath, Armored       │
│                                     │
│  Resistances:                       │
│    Fire: Immune                     │
│    Physical: 25%                    │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-003: Navigate Dungeon](UC-003-navigate-dungeon.md) - See room when moving
- [UC-005: Engage in Combat](UC-005-engage-in-combat.md) - Examine during combat
- [UC-007: Manage Inventory](UC-007-manage-inventory.md) - Examine items

## Implementation Notes

- Room rendering via `IGameRenderer.RenderRoom()`
- Item details via `IGameRenderer.RenderItemDetails()`
- Monster details via `IGameRenderer.RenderMonsterDetails()`
- No calls to game state modification services
