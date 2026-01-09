# UC-011: Roll Dice

**Actor:** Player
**Priority:** Medium
**Version:** v0.0.5
**Status:** Implemented

## Description

Player performs a direct dice roll using standard dice notation. This can be used for ad-hoc rolls, gambling, or any situation where random determination is needed.

## Preconditions

- Active game session exists
- Valid dice notation provided

## Basic Flow

1. Player enters "roll [notation]" command (e.g., "roll 2d6+3")
2. System parses dice notation:
   - Count: Number of dice
   - Type: d4, d6, d8, d10
   - Modifier: Flat bonus/penalty
   - Exploding: ! suffix for exploding dice
3. System validates notation
4. System rolls each die
5. System applies modifier to total
6. System displays result:
   - Individual die results
   - Total with modifier
7. No turn consumed

## Alternative Flows

### AF-1: Simple Roll

1. Player enters "roll d6" or "roll 1d6"
2. System rolls single die
3. Result displayed

### AF-2: Multiple Dice

1. Player enters "roll 3d8"
2. System rolls 3 eight-sided dice
3. Individual results and sum displayed

### AF-3: Roll with Modifier

1. Player enters "roll 2d6+5" or "roll 2d6-2"
2. System rolls dice
3. Modifier applied to total
4. Shows: "2d6+5: [4, 3] + 5 = 12"

### AF-4: Exploding Dice

1. Player enters "roll 1d6!"
2. System rolls die
3. If maximum value rolled, roll again and add
4. Continue until non-maximum or max explosions
5. Shows all explosions: "1d6!: [6, 6, 3] = 15"

### AF-5: Quick Roll Shortcuts

1. Player enters "d20" without "roll" prefix
2. System recognizes dice notation
3. Performs roll as normal

## Exception Flows

### EF-1: Invalid Notation

1. At step 3: Notation cannot be parsed
2. System displays "Invalid dice notation: [input]"
3. System shows examples of valid notation
4. No roll performed

### EF-2: Unsupported Dice Type

1. At step 3: Dice type not supported (e.g., d12, d20, d100)
2. System displays "Unsupported dice type. Use d4, d6, d8, or d10"
3. No roll performed

### EF-3: Zero or Negative Count

1. At step 3: Dice count < 1
2. System displays "Must roll at least 1 die"
3. No roll performed

### EF-4: Excessive Dice Count

1. At step 3: Dice count exceeds limit (e.g., >100)
2. System displays "Too many dice (max 100)"
3. No roll performed

## Postconditions

- Random result generated
- Result displayed to player
- No game state changed
- No turn consumed

## Business Rules

- Supported dice: d4, d6, d8, d10
- Minimum dice count: 1
- Maximum dice count: 100
- Exploding dice cap at 10 explosions
- Modifiers can be positive or negative
- Rolls do not consume turns
- Results are purely random (no skill influence)

## Dice Notation Reference

| Notation | Meaning |
|----------|---------|
| `d6` | Roll one d6 |
| `2d6` | Roll two d6, sum results |
| `3d8+5` | Roll 3d8, add 5 to total |
| `1d10-2` | Roll 1d10, subtract 2 |
| `1d6!` | Exploding d6 (reroll on max) |
| `4d4!+2` | 4 exploding d4 plus 2 |

## UI Display

### Simple Roll

```
╭─────────────────────────────────────╮
│           🎲 DICE ROLL 🎲           │
├─────────────────────────────────────┤
│                                     │
│  Rolling: 2d6+3                     │
│                                     │
│  Results: [4] [6]                   │
│  Sum: 10                            │
│  Modifier: +3                       │
│  ─────────────────                  │
│  Total: 13                          │
│                                     │
╰─────────────────────────────────────╯
```

### Exploding Dice

```
╭─────────────────────────────────────╮
│        🎲 EXPLODING DICE 🎲         │
├─────────────────────────────────────┤
│                                     │
│  Rolling: 1d6!                      │
│                                     │
│  Die 1: [6] EXPLODE!                │
│         [6] EXPLODE!                │
│         [4]                         │
│                                     │
│  Total: 16                          │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-012: Skill Check](UC-012-skill-check.md) - Uses dice with modifiers
- [UC-005: Engage in Combat](UC-005-engage-in-combat.md) - Combat uses dice internally
- [UC-108: Roll Initiative](../system/UC-108-roll-initiative.md) - System dice roll

## Implementation Notes

- Dice notation parsing via `DicePool.Parse(notation)`
- Roll execution via `DiceService.Roll(dicePool)`
- Random generation via seeded `Random` (for reproducibility in tests)
- Results returned as `DiceRollResult` with individual and total values
