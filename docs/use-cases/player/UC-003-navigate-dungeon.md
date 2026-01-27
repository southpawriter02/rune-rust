# UC-003: Navigate Dungeon

**Actor:** Player
**Priority:** High
**Version:** v0.0.2
**Status:** Implemented

## Description

Player moves between rooms in the dungeon using directional commands. Navigation is the primary way to explore the dungeon and encounter monsters.

## Preconditions

- Active game session exists
- Player is in a room
- Player is not in combat (or has fled/won combat)

## Basic Flow

1. Player enters directional command (north, south, east, west, or n, s, e, w)
2. System checks if exit exists in that direction from current room
3. System moves player to connected room
4. System increments turn count
5. System triggers turn-end processing (UC-101)
6. System displays new room (description, exits, monsters, items)
7. If room has monsters, combat begins

## Alternative Flows

### AF-1: Use Short Commands

1. At step 1: Player enters single-letter command (n, s, e, w)
2. System maps to full direction
3. Flow continues at step 2

### AF-2: Use Go Command

1. At step 1: Player enters "go north" or "go n"
2. System parses direction from command
3. Flow continues at step 2

### AF-3: Room Has Items

1. At step 6: Room contains items
2. System lists items in room
3. Player can pick up items (UC-007) before next move

### AF-4: Room Has Monster

1. At step 6: Room contains one or more monsters
2. System initiates combat automatically
3. Flow continues with combat (UC-005)

## Exception Flows

### EF-1: No Exit in Direction

1. At step 2: No exit exists in requested direction
2. System displays "You can't go that way"
3. No turn consumed
4. Player remains in current room

### EF-2: Blocked Exit (Future)

1. At step 2: Exit exists but is locked/blocked
2. System displays "The way is blocked"
3. Player may need key or action to proceed

### EF-3: Player in Combat

1. At step 1: Player attempts to move while in combat
2. System displays "You cannot flee that easily!"
3. Player must defeat monsters or use flee action (UC-013)

## Postconditions

- Player position updated to new room
- Turn count incremented
- Turn-end effects processed
- New room displayed to player
- Combat initiated if monsters present

## Business Rules

- Movement consumes one turn
- Valid directions are: north, south, east, west (or n, s, e, w)
- Player cannot move through walls (non-existent exits)
- Entering a room with monsters triggers combat
- Room descriptions shown on each entry
- Previously visited rooms still show full description

## UI Display

```
╭─────────────────────────────────────╮
│           DARK CORRIDOR             │
├─────────────────────────────────────┤
│                                     │
│  A narrow stone corridor stretches  │
│  before you. Torches flicker on     │
│  the walls, casting dancing shadows.│
│                                     │
│  Exits: [North] [East]              │
│                                     │
│  You see:                           │
│    - Health Potion                  │
│                                     │
╰─────────────────────────────────────╯
> _
```

## Related Use Cases

- [UC-004: Examine Environment](UC-004-examine-environment.md) - Look at current room
- [UC-005: Engage in Combat](UC-005-engage-in-combat.md) - Triggered by monsters
- [UC-007: Manage Inventory](UC-007-manage-inventory.md) - Pick up items
- [UC-101: Process Turn End](../system/UC-101-process-turn-end.md) - After movement

## Implementation Notes

- Room connections stored in `Room.Exits` dictionary (Direction → Room)
- Player position updated via `Player.MoveTo(room)` or position change
- Room rendering via `IGameRenderer.RenderRoom(roomDto, playerDto)`
- Combat check in `GameSessionService` after room change
