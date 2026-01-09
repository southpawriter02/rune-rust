# UC-001: Create Character

**Actor:** Player
**Priority:** High
**Version:** v0.0.3
**Status:** Implemented

## Description

Player creates a new character by providing a name and optionally selecting race and background. This is the first step when starting a new game.

## Preconditions

- Player has started the application
- No active game session exists (or player chose "New Game")

## Basic Flow

1. System displays character creation screen
2. System prompts for character name
3. Player enters character name
4. System validates name (non-empty, valid characters)
5. System creates new Player entity with:
   - Provided name
   - Default starting stats (Health: 100, Attack: 10, Defense: 5)
   - Empty inventory
   - Starting position at dungeon entrance
6. System creates new GameSession with the player
7. System proceeds to class selection (UC-002)

## Alternative Flows

### AF-1: Skip to Default Name

1. At step 3: Player presses Enter without entering a name
2. System assigns default name "Adventurer"
3. Flow continues at step 5

### AF-2: Character Customization (Future)

1. After step 4: System prompts for race selection
2. Player selects race from available options
3. System applies race bonuses to base stats
4. System prompts for background selection
5. Player selects background
6. System applies background bonuses
7. Flow continues at step 5

## Exception Flows

### EF-1: Invalid Character Name

1. At step 4: Name contains invalid characters or exceeds length limit
2. System displays error message
3. System prompts for name again
4. Flow returns to step 3

### EF-2: Name Too Short

1. At step 4: Name is empty or whitespace only
2. System displays "Name cannot be empty" message
3. Flow returns to step 3

## Postconditions

- New Player entity exists with provided name
- New GameSession exists containing the player
- Player has default starting stats
- Player is ready for class selection

## Business Rules

- Character names must be 1-30 characters
- Character names may contain letters, numbers, and spaces
- Character names are trimmed of leading/trailing whitespace
- Default stats are: Health 100, Attack 10, Defense 5
- Starting level is 1
- Starting XP is 0

## UI Display

```
╭─────────────────────────────────────╮
│         CHARACTER CREATION          │
├─────────────────────────────────────┤
│                                     │
│  Enter your character's name:       │
│  > _                                │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-002: Select Class](UC-002-select-class.md) - Follows character creation
- [UC-003: Navigate Dungeon](UC-003-navigate-dungeon.md) - First action after setup

## Implementation Notes

- Player entity created via `new Player(name, health, attack, defense)`
- GameSession created via `GameSession.Create(player, dungeon)`
- Validation happens in Player constructor
