# UC-105: Apply Status Effects

**Actor:** System
**Priority:** Medium
**Version:** v0.0.6
**Status:** Implemented

## Description

System processes all active status effects on entities at the end of each turn, applying ongoing damage/healing, reducing durations, and removing expired effects.

## Trigger

- Turn end processing (UC-101) reaches status effect phase

## Preconditions

- Entity has one or more active status effects
- Status effect definitions loaded

## Basic Flow

1. System receives entity (player or monster)
2. For each active status effect:
   a. Apply per-turn effect (damage, healing, stat change)
   b. Reduce remaining duration by 1
   c. If duration reaches 0, mark for removal
3. Remove all expired effects
4. Recalculate entity stats if modifiers changed
5. Return effect processing summary

## Alternative Flows

### AF-1: Damage Over Time (DoT)

1. Effect type is damage (Poison, Burn)
2. Calculate tick damage
3. Apply damage to entity
4. Check if entity dies from DoT
5. If dead, handle death immediately

### AF-2: Healing Over Time (HoT)

1. Effect type is healing (Regeneration)
2. Calculate heal amount
3. Apply healing (capped at max health)
4. Continue to duration reduction

### AF-3: Stat Modifier

1. Effect type is stat modification (Strength buff)
2. Modifier already applied when effect started
3. No per-turn action needed
4. On removal, recalculate base stats

### AF-4: Multiple Effects

1. Entity has multiple effects
2. Process each in order (debuffs before buffs typically)
3. All effects processed in same turn
4. Combined result displayed

### AF-5: Permanent Effect

1. Effect has duration of -1 (permanent)
2. No duration reduction
3. Effect persists until explicitly removed

### AF-6: Effect Stacking

1. Same effect applied multiple times
2. Either refresh duration (replace)
3. Or stack intensity (add)
4. Behavior defined per effect type

## Exception Flows

### EF-1: Effect Definition Missing

1. Effect type not in configuration
2. Skip this effect
3. Log warning
4. Continue with other effects

### EF-2: Entity Dies from DoT

1. Damage effect kills entity
2. Process death immediately
3. Skip remaining effects for this entity
4. Continue turn processing for others

### EF-3: Invalid Effect State

1. Effect has invalid duration or values
2. Remove the effect
3. Log warning
4. Continue processing

## Postconditions

- All per-turn effects applied
- Durations decremented
- Expired effects removed
- Entity stats recalculated if needed
- Effect summary available

## Business Rules

- Effects process in defined order
- DoT damage can kill
- HoT cannot exceed max health
- Duration 0 means effect expires this turn
- Duration -1 means permanent until removed
- Stat modifiers applied additively
- Percentage modifiers apply after flat modifiers

## Common Status Effects

| Effect | Type | Per-Turn | Duration |
|--------|------|----------|----------|
| Poison | DoT | 3 damage | 3 turns |
| Burn | DoT | 5 damage | 2 turns |
| Regeneration | HoT | 5 healing | 3 turns |
| Strength | Buff | +5 Attack | 5 turns |
| Weakness | Debuff | -3 Attack | 3 turns |
| Stun | Disable | Skip turn | 1 turn |
| Slow | Debuff | -Initiative | 3 turns |

## Effect Processing Order

```
Turn End
    │
    ▼
┌─────────────────────────┐
│ Get Active Effects      │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ For Each Effect:        │
│ 1. Apply per-turn       │
│ 2. Reduce duration      │
│ 3. Check expiration     │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Remove Expired Effects  │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Recalculate Stats       │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Return Summary          │
└─────────────────────────┘
```

## UI Display

DoT damage:

```
╭─────────────────────────────────────╮
│         STATUS EFFECTS              │
├─────────────────────────────────────┤
│                                     │
│  Poison deals 3 damage!             │
│  Health: 77 → 74                    │
│  [Poison: 2 turns remaining]        │
│                                     │
╰─────────────────────────────────────╯
```

Effect expiration:

```
╭─────────────────────────────────────╮
│                                     │
│  Strength buff has expired.         │
│  Attack: 25 → 20                    │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-101: Process Turn End](UC-101-process-turn-end.md) - Calls this use case
- [UC-006: Use Ability](../player/UC-006-use-ability.md) - May apply effects
- [UC-008: Use Item](../player/UC-008-use-item.md) - May apply effects

## Implementation Notes

- Processing via `StatusEffectService.ProcessTurnEnd(entity)`
- Effect storage in `Entity.StatusEffects` list
- Per-turn logic in `StatusEffectService.ApplyTick(effect, entity)`
- Stat recalculation via `Entity.RecalculateStats()`
