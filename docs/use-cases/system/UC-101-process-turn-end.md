# UC-101: Process Turn End

**Actor:** System
**Priority:** High
**Version:** v0.0.4
**Status:** Implemented

## Description

System executes all end-of-turn effects after a player or monster action. This orchestrates resource regeneration, cooldown reduction, status effect processing, and other turn-based mechanics.

## Trigger

- Player completes an action that consumes a turn
- Monster completes its turn
- Any game event that advances the turn counter

## Preconditions

- Active game session exists
- Turn-consuming action completed

## Basic Flow

1. System increments turn counter
2. System processes resource regeneration (UC-102):
   - Determine if player is in combat
   - Apply appropriate regeneration/decay rates
3. System reduces ability cooldowns (UC-103):
   - Decrement all cooldowns by 1
   - Mark abilities with 0 cooldown as ready
4. System processes status effects (UC-105):
   - Apply ongoing damage/healing
   - Reduce durations
   - Remove expired effects
5. System checks for triggered events:
   - Level up (if XP threshold reached)
   - Death conditions
   - Quest progress
6. System updates game state
7. System notifies presentation layer of changes

## Alternative Flows

### AF-1: Combat Turn End

1. Turn ends during combat
2. Combat-specific regeneration rates applied
3. Monster turns processed (UC-104)
4. Initiative order maintained
5. Combat continues or ends based on state

### AF-2: Exploration Turn End

1. Turn ends outside combat
2. Non-combat regeneration rates applied
3. No monster turns
4. Possible random encounter check (future)

### AF-3: No Resource Changes

1. Player has no resources (rare)
2. Skip resource regeneration
3. Other processing continues normally

## Exception Flows

### EF-1: Player Death

1. At step 4: Status effect or damage kills player
2. Game state set to GameOver
3. Death message displayed
4. No further turn processing

### EF-2: Configuration Error

1. Resource or effect configuration missing
2. Skip that processing step
3. Log warning
4. Continue with other steps

## Postconditions

- Turn counter incremented
- Resources regenerated/decayed
- Cooldowns reduced
- Status effects processed
- Game state updated
- Presentation notified

## Business Rules

- Turn processing is atomic (all or nothing)
- Order of operations is fixed:
  1. Turn increment
  2. Resource changes
  3. Cooldown reduction
  4. Status effects
  5. Event triggers
- Player death halts further processing
- Multiple effects can trigger in same turn
- Effects that kill monsters award XP immediately

## Turn Processing Order

```
Turn Action Completed
         │
         ▼
┌─────────────────────┐
│ Increment Turn      │
└─────────┬───────────┘
         │
         ▼
┌─────────────────────┐
│ Resource Regen      │
│ (UC-102)            │
└─────────┬───────────┘
         │
         ▼
┌─────────────────────┐
│ Reduce Cooldowns    │
│ (UC-103)            │
└─────────┬───────────┘
         │
         ▼
┌─────────────────────┐
│ Status Effects      │
│ (UC-105)            │
└─────────┬───────────┘
         │
         ▼
┌─────────────────────┐
│ Event Triggers      │
│ (Level up, etc.)    │
└─────────┬───────────┘
         │
         ▼
┌─────────────────────┐
│ Update UI           │
└─────────────────────┘
```

## Related Use Cases

- [UC-102: Regenerate Resources](UC-102-regenerate-resources.md) - Resource processing
- [UC-103: Reduce Cooldowns](UC-103-reduce-cooldowns.md) - Cooldown processing
- [UC-105: Apply Status Effects](UC-105-apply-status-effects.md) - Effect processing
- [UC-010: Level Up](../player/UC-010-level-up.md) - Triggered by XP threshold
- [UC-104: Monster Turn](UC-104-monster-turn.md) - Combat turn processing

## Implementation Notes

- Orchestrated by `GameSessionService.ProcessTurnEnd()`
- Calls `ResourceService.ProcessTurnEnd(player, isInCombat)`
- Calls `AbilityService.ProcessTurnEnd(player)`
- Calls `StatusEffectService.ProcessTurnEnd(player)`
- Turn counter in `GameSession.TurnCount`
