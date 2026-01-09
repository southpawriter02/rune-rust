# UC-010: Level Up

**Actor:** Player
**Priority:** High
**Version:** v0.0.8
**Status:** Implemented

## Description

Player gains a level when accumulating enough experience points, receiving stat increases and potentially unlocking new abilities.

## Preconditions

- Active game session exists
- Player has accumulated XP >= threshold for next level
- Player is not at maximum level

## Basic Flow

1. System detects XP threshold reached (after UC-109)
2. System increments player level
3. System applies stat gains based on class growth rates:
   - Health increased (and current health restored)
   - Attack increased
   - Defense increased
4. System checks for newly unlocked abilities
5. If new abilities available:
   - System adds abilities to player
   - System displays "New ability unlocked: [ability]"
6. System increases resource maximum (if applicable)
7. System displays level up summary
8. If XP still exceeds next threshold, repeat

## Alternative Flows

### AF-1: Multiple Level Ups

1. At step 8: XP exceeds multiple level thresholds
2. System processes each level up sequentially
3. All stat gains and unlocks accumulated
4. Single comprehensive summary displayed

### AF-2: Max Level Reached

1. At step 2: Player reaches maximum level
2. System sets level to max
3. Excess XP is retained (or capped)
4. System displays "Maximum level reached!"

### AF-3: Choose Stat Improvement (Future)

1. After step 3: System offers stat choice
2. Player selects one stat to receive bonus points
3. Chosen stat receives additional increase
4. Flow continues at step 4

### AF-4: Ability Choice (Future)

1. At step 5: Multiple abilities become available
2. Player chooses which ability to learn
3. Chosen ability added to player
4. Other abilities remain available for future levels

## Exception Flows

### EF-1: Configuration Missing

1. At step 3: Progression configuration not found
2. System uses default growth rates
3. Level up proceeds normally
4. Warning logged

### EF-2: Ability Definition Missing

1. At step 5: Unlocked ability ID not in configuration
2. Ability is skipped
3. Warning logged
4. Level up continues

## Postconditions

- Player level increased by one (or more)
- Player stats increased based on class growth
- Current health restored to new maximum
- New abilities unlocked (if any)
- Resource maximum increased
- Level up message displayed

## Business Rules

- XP thresholds follow progression table
- Growth rates defined per class
- Health fully restored on level up
- Abilities unlock at specific levels
- Level cap exists (typically 20)
- XP continues accumulating at max level
- Multiple levels gained at once are processed

## Level Progression Example

| Level | XP Required | Total XP | Health Gain | Attack Gain | Defense Gain |
|-------|-------------|----------|-------------|-------------|--------------|
| 1 | 0 | 0 | - | - | - |
| 2 | 100 | 100 | +6 | +2 | +1 |
| 3 | 150 | 250 | +6 | +2 | +1 |
| 4 | 225 | 475 | +6 | +2 | +1 |
| 5 | 340 | 815 | +8 | +3 | +2 |

## UI Display

### Level Up

```
╭─────────────────────────────────────╮
│        ⭐ LEVEL UP! ⭐              │
├─────────────────────────────────────┤
│                                     │
│  Congratulations!                   │
│  You reached Level 5!               │
│                                     │
│  STAT INCREASES:                    │
│    Health:  115 → 123 (+8)          │
│    Attack:   18 → 21  (+3)          │
│    Defense:  12 → 14  (+2)          │
│                                     │
│  NEW ABILITY UNLOCKED:              │
│    🔥 Flame Strike                  │
│    A powerful fire attack           │
│                                     │
│  Health fully restored!             │
│                                     │
╰─────────────────────────────────────╯
```

### Multiple Levels

```
╭─────────────────────────────────────╮
│      ⭐⭐ DOUBLE LEVEL UP! ⭐⭐      │
├─────────────────────────────────────┤
│                                     │
│  You gained 2 levels!               │
│  Level 3 → Level 5                  │
│                                     │
│  TOTAL STAT INCREASES:              │
│    Health:  106 → 123 (+17)         │
│    Attack:   14 → 21  (+7)          │
│    Defense:   9 → 14  (+5)          │
│                                     │
│  ABILITIES UNLOCKED:                │
│    🔥 Fire Shield (Level 4)         │
│    🔥 Flame Strike (Level 5)        │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-005: Engage in Combat](UC-005-engage-in-combat.md) - Source of XP
- [UC-006: Use Ability](UC-006-use-ability.md) - Uses unlocked abilities
- [UC-109: Award Experience](../system/UC-109-award-experience.md) - Triggers level check

## Implementation Notes

- XP threshold lookup via `ProgressionService.GetXpForLevel(level)`
- Level up processing via `ProgressionService.ProcessLevelUp(player)`
- Growth rates from `ClassDefinition.GrowthRates`
- Ability unlock check via `AbilityService.GetNewlyUnlockedAbilities(player)`
- Health restoration via `Player.RestoreFullHealth()`
