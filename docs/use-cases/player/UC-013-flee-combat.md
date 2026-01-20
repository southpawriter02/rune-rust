# UC-013: Flee Combat

**Actor:** Player
**Priority:** Medium
**Version:** v0.0.6
**Status:** Implemented

## Description

Player attempts to escape from combat, requiring a successful skill check. Fleeing allows retreat but may have consequences.

## Preconditions

- Active game session exists
- Player is currently in combat
- At least one exit from current room exists

## Basic Flow

1. Player enters "flee" or "run" command
2. System validates player is in combat
3. System determines flee difficulty based on:
   - Number of monsters
   - Monster tier
   - Player encumbrance (future)
4. System performs skill check (Dexterity vs. DC)
5. On success:
   - Combat ends
   - Player moves to previous room (or random exit)
   - Monsters remain in original room
   - Turn consumed
6. System displays flee result

## Alternative Flows

### AF-1: Flee to Specific Exit

1. Player enters "flee north" or "run east"
2. System validates exit exists
3. Flee check performed
4. On success, player moves to specified direction
5. Flow continues at step 5

### AF-2: Flee Failure

1. At step 5: Skill check fails
2. Player remains in combat
3. Player loses turn (cannot act this round)
4. All monsters get attacks of opportunity
5. Combat continues

### AF-3: Multiple Attempts

1. Player can attempt flee multiple turns
2. Each attempt follows same process
3. Difficulty may increase after failures (optional)

### AF-4: Guaranteed Escape (Future)

1. Special ability or item guarantees escape
2. No skill check required
3. Flee automatically succeeds

## Exception Flows

### EF-1: Not in Combat

1. At step 2: Player is not in combat
2. System displays "You're not in combat"
3. No action taken

### EF-2: No Exits Available

1. At step 2: Room has no exits (trapped)
2. System displays "There's nowhere to run!"
3. No flee attempt made

### EF-3: Fleeing Disabled

1. Some encounters prevent fleeing
2. System displays "You cannot escape this fight!"
3. Player must defeat enemies or die

## Postconditions

On Success:
- Combat state ended
- Player position changed
- Turn count incremented
- Monsters remain in original room

On Failure:
- Player remains in combat
- Turn consumed
- Monsters may have attacked

## Business Rules

- Flee consumes a turn
- Failed flee allows monsters opportunity attack
- Base DC for flee: 10
- DC increases by 2 per additional monster
- Elite/Boss monsters add +3 to DC
- Successful flee moves to last entered room
- Cannot flee on first turn of combat (optional rule)
- Some monsters have "Can't Escape" trait

## Flee Difficulty Calculation

| Factor | DC Modifier |
|--------|-------------|
| Base | 10 |
| Per additional monster | +2 |
| Elite monster present | +3 |
| Boss monster present | +5 |
| Player wounded (<25% HP) | +2 |
| Player encumbered (future) | +2 |

## UI Display

### Flee Success

```
╭─────────────────────────────────────╮
│          🏃 FLEEING! 🏃             │
├─────────────────────────────────────┤
│                                     │
│  You attempt to flee!               │
│                                     │
│  Skill Check: Dexterity             │
│  Roll: 2d6 → [5, 4] = 9             │
│  Modifier: +3                       │
│  Total: 12 vs DC 10                 │
│                                     │
│  ✓ SUCCESS!                         │
│                                     │
│  You escape to the north!           │
│  The monsters do not pursue.        │
│                                     │
╰─────────────────────────────────────╯
```

### Flee Failure

```
╭─────────────────────────────────────╮
│        ❌ ESCAPE FAILED ❌          │
├─────────────────────────────────────┤
│                                     │
│  You attempt to flee!               │
│                                     │
│  Skill Check: Dexterity             │
│  Roll: 2d6 → [1, 3] = 4             │
│  Modifier: +3                       │
│  Total: 7 vs DC 12                  │
│                                     │
│  ✗ FAILURE!                         │
│                                     │
│  The monsters block your escape!    │
│                                     │
│  Goblin Warrior attacks you!        │
│  You take 6 damage.                 │
│  Goblin Archer attacks you!         │
│  You take 4 damage.                 │
│                                     │
╰─────────────────────────────────────╯
```

### Cannot Flee

```
╭─────────────────────────────────────╮
│                                     │
│  The Dragon blocks all exits!       │
│  There is no escape from this       │
│  battle!                            │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-005: Engage in Combat](UC-005-engage-in-combat.md) - Combat context
- [UC-012: Skill Check](UC-012-skill-check.md) - Flee uses skill check
- [UC-003: Navigate Dungeon](UC-003-navigate-dungeon.md) - Movement after flee
- [UC-104: Monster Turn](../system/UC-104-monster-turn.md) - Attacks on flee failure

## Implementation Notes

- Flee attempt via `CombatService.AttemptFlee(player, direction?)`
- DC calculation in `CombatService.CalculateFleeDC(combat)`
- Skill check via `SkillCheckService.PerformCheck(player, "dexterity", dc)`
- Room transition via `GameSessionService.MovePlayer(direction)`
- Combat state via `GameSession.IsInCombat`
