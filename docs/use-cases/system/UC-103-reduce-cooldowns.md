# UC-103: Reduce Cooldowns

**Actor:** System
**Priority:** Medium
**Version:** v0.0.4
**Status:** Implemented

## Description

System reduces all ability cooldowns by 1 at the end of each turn, making abilities available again after their cooldown period.

## Trigger

- Turn end processing (UC-101) reaches cooldown phase

## Preconditions

- Player has abilities with cooldowns
- At least one ability has cooldown > 0

## Basic Flow

1. System receives player
2. For each ability the player has:
   a. Check current cooldown
   b. If cooldown > 0, decrement by 1
   c. If cooldown reaches 0, mark as ready
3. System records which abilities became ready
4. System returns cooldown changes

## Alternative Flows

### AF-1: Ability Already Ready

1. Ability has cooldown = 0
2. No change needed
3. Skip to next ability

### AF-2: Ability Becomes Ready

1. Ability cooldown was 1
2. Decrement to 0
3. Ability now usable
4. May trigger notification

### AF-3: Long Cooldown

1. Ability cooldown > 1
2. Decrement by 1
3. Ability still on cooldown
4. Continue to next ability

### AF-4: No Abilities on Cooldown

1. All abilities have cooldown = 0
2. Nothing to process
3. Return empty change summary

### AF-5: Cooldown Reduction Buff (Future)

1. Player has effect that reduces cooldowns faster
2. Decrement by more than 1
3. Cannot go below 0

## Exception Flows

### EF-1: Invalid Ability State

1. Ability has negative cooldown
2. Set cooldown to 0
3. Log warning
4. Continue processing

## Postconditions

- All ability cooldowns decremented (if > 0)
- Abilities reaching 0 marked as ready
- Cooldown change summary available

## Business Rules

- Standard reduction is 1 per turn
- Cooldowns cannot go below 0
- Ability with 0 cooldown can be used immediately
- Cooldown reduction happens once per turn
- Reduction applies in both combat and non-combat

## Cooldown Flow Example

```
Turn 1: Use Fireball (Cooldown: 3)
        Fireball: Ready → CD 3

Turn 2: Turn End
        Fireball: CD 3 → CD 2

Turn 3: Turn End
        Fireball: CD 2 → CD 1

Turn 4: Turn End
        Fireball: CD 1 → Ready!

Turn 5: Can use Fireball again
```

## UI Display

Cooldown reduction in abilities list:

```
╭─────────────────────────────────────╮
│           YOUR ABILITIES            │
├─────────────────────────────────────┤
│                                     │
│  [1] Fireball      30 MP  Ready     │
│  [2] Frost Nova    40 MP  2 turns   │
│  [3] Meteor        60 MP  1 turn    │←─ About to be ready
│                                     │
╰─────────────────────────────────────╯
```

Notification when ability becomes ready:

```
╭─────────────────────────────────────╮
│                                     │
│  ✓ Meteor is now ready!             │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-101: Process Turn End](UC-101-process-turn-end.md) - Calls this use case
- [UC-006: Use Ability](../player/UC-006-use-ability.md) - Sets cooldowns when used

## Implementation Notes

- Processing via `AbilityService.ProcessTurnEnd(player)`
- Cooldown tracked in `PlayerAbility.CurrentCooldown`
- Decrement: `ability.CurrentCooldown = Math.Max(0, ability.CurrentCooldown - 1)`
- Returns list of abilities that became ready
