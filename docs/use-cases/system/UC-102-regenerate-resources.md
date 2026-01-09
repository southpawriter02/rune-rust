# UC-102: Regenerate Resources

**Actor:** System
**Priority:** High
**Version:** v0.0.4
**Status:** Implemented

## Description

System applies resource regeneration or decay to player's resource pools at the end of each turn. Rates differ based on combat state and resource type.

## Trigger

- Turn end processing (UC-101) reaches resource phase

## Preconditions

- Player has at least one resource pool initialized
- Resource type definitions loaded

## Basic Flow

1. System receives player and combat state
2. For each resource pool the player has:
   a. Get resource type definition
   b. Determine rate (combat vs. non-combat)
   c. Apply rate to current value
   d. Clamp result between 0 and maximum
3. System records resource changes
4. System returns change summary

## Alternative Flows

### AF-1: Combat Regeneration

1. Player is in combat
2. Use `RegenPerTurnCombat` rate
3. Rate may be positive (mana) or negative (focus decay)

### AF-2: Non-Combat Regeneration

1. Player is not in combat
2. Use `RegenPerTurnNonCombat` rate
3. Typically higher regeneration rates

### AF-3: Rage Decay

1. Rage resource outside combat
2. Negative regeneration rate applied
3. Rage decreases toward 0
4. Cannot go below 0

### AF-4: At Maximum/Minimum

1. Resource already at max (for regen) or 0 (for decay)
2. No change applied
3. Resource stays at boundary

### AF-5: Multiple Resources

1. Player has multiple resource types (rare)
2. Each processed independently
3. All changes reported

## Exception Flows

### EF-1: Resource Type Not Found

1. Resource type ID not in configuration
2. Skip this resource
3. Log warning
4. Continue with other resources

### EF-2: Invalid Pool State

1. Resource pool has invalid values (negative, etc.)
2. Clamp to valid range
3. Log warning
4. Continue processing

## Postconditions

- All player resource pools updated
- Changes clamped within valid ranges
- Resource change summary available for display

## Business Rules

- Regeneration is additive (current + rate)
- Result clamped: 0 <= current <= maximum
- Combat state determined at moment of processing
- Each resource type processes independently
- Zero rate means no change
- Negative rate means decay/reduction

## Resource Behavior Examples

### Mana (Mystic)

| State | Rate | Example |
|-------|------|---------|
| Combat | +5/turn | 50 → 55 |
| Non-Combat | +10/turn | 50 → 60 |
| At Max | +5/turn | 100 → 100 |

### Rage (Berserker)

| State | Rate | Example |
|-------|------|---------|
| Combat | +15/turn | 30 → 45 |
| Non-Combat | -10/turn | 30 → 20 |
| At Zero | -10/turn | 0 → 0 |

### Stamina (Rogue)

| State | Rate | Example |
|-------|------|---------|
| Combat | +8/turn | 60 → 68 |
| Non-Combat | +15/turn | 60 → 75 |

## UI Display

Resource changes shown in turn summary:

```
╭─────────────────────────────────────╮
│         TURN END SUMMARY            │
├─────────────────────────────────────┤
│                                     │
│  Resources:                         │
│    Mana: 45 → 55 (+10)              │
│                                     │
╰─────────────────────────────────────╯
```

Rage decay:

```
│  Resources:                         │
│    Rage: 30 → 20 (-10)              │
│    [Rage decays outside combat]     │
```

## Related Use Cases

- [UC-101: Process Turn End](UC-101-process-turn-end.md) - Calls this use case
- [UC-006: Use Ability](../player/UC-006-use-ability.md) - Spends resources
- [UC-002: Select Class](../player/UC-002-select-class.md) - Initializes resources

## Implementation Notes

- Processing via `ResourceService.ProcessTurnEnd(player, isInCombat)`
- Resource definitions from `IConfigurationProvider.GetResourceType(id)`
- Rate selection: `isInCombat ? def.RegenPerTurnCombat : def.RegenPerTurnNonCombat`
- Clamping via `Math.Clamp(current + rate, 0, max)`
- Returns `ResourceChangeSummary` with before/after values
