# UC-109: Award Experience

**Actor:** System
**Priority:** High
**Version:** v0.0.8
**Status:** Implemented

## Description

System awards experience points (XP) to the player when monsters are defeated, potentially triggering level ups.

## Trigger

- Monster defeated in combat
- Quest completed (future)
- Milestone reached (future)

## Preconditions

- Player exists and is alive
- XP source is valid (defeated monster, etc.)
- Player is not at maximum level (or XP still tracked)

## Basic Flow

1. System calculates XP value:
   - Base XP from monster definition
   - Apply tier modifier
   - Apply level difference modifier (optional)
2. System adds XP to player's current total
3. System checks if XP >= next level threshold
4. If threshold reached:
   - Trigger level up (UC-010)
   - Check for additional level ups
5. System returns XP award summary

## Alternative Flows

### AF-1: Multiple Monsters

1. Combat ended with multiple monster kills
2. Calculate XP for each monster
3. Sum total XP
4. Award total XP once
5. Check for level ups

### AF-2: Bonus XP

1. Special conditions apply:
   - First kill of type
   - Overkill damage
   - No damage taken
2. Add bonus percentage to base XP
3. Display bonus in summary

### AF-3: XP Penalty (Future)

1. Player significantly higher level than monster
2. Reduce XP award
3. Prevents farming low-level monsters
4. Display reduced XP

### AF-4: Multiple Level Ups

1. XP award pushes past multiple thresholds
2. Process each level up in sequence
3. Display combined level up summary
4. All stat gains applied

### AF-5: Max Level

1. Player at maximum level
2. XP may still be tracked (or capped)
3. No level up triggered
4. Display "Max level reached"

## Exception Flows

### EF-1: Invalid Monster XP

1. Monster has no XP value defined
2. Use default XP for tier
3. Log warning
4. Continue award

### EF-2: Missing Progression Config

1. Level thresholds not configured
2. Use default progression
3. Log error
4. Continue award

### EF-3: XP Overflow

1. XP would exceed integer max
2. Cap at maximum value
3. Log warning

## Postconditions

- Player XP increased
- Level up triggered if threshold reached
- XP award displayed to player
- Stats updated if leveled

## Business Rules

- Base XP defined per monster type
- Tier multipliers: Common (1x), Elite (2x), Boss (5x)
- Level difference may affect XP (optional)
- XP always awarded (minimum 1)
- Level up checks happen immediately
- Multiple level ups process sequentially

## XP Calculation

```
Base XP × Tier Multiplier × Level Modifier = Final XP
```

### Tier Multipliers

| Tier | Multiplier |
|------|------------|
| Minion | 0.5x |
| Common | 1.0x |
| Uncommon | 1.5x |
| Elite | 2.0x |
| Boss | 5.0x |
| Legendary | 10.0x |

### Level Modifier (Optional)

| Level Difference | Modifier |
|------------------|----------|
| Monster 3+ higher | 1.5x |
| Monster 1-2 higher | 1.25x |
| Same level | 1.0x |
| Monster 1-2 lower | 0.75x |
| Monster 3+ lower | 0.5x |

## XP Thresholds Example

| Level | XP Required | Total XP |
|-------|-------------|----------|
| 1 → 2 | 100 | 100 |
| 2 → 3 | 150 | 250 |
| 3 → 4 | 225 | 475 |
| 4 → 5 | 340 | 815 |
| 5 → 6 | 510 | 1325 |

## UI Display

XP Award:

```
╭─────────────────────────────────────╮
│        VICTORY!                     │
├─────────────────────────────────────┤
│                                     │
│  Goblin Warrior defeated!           │
│                                     │
│  Experience: +25 XP                 │
│  Total: 75/100 XP                   │
│                                     │
│  ████████████████░░░░ 75%          │
│                                     │
╰─────────────────────────────────────╯
```

Multiple monsters:

```
╭─────────────────────────────────────╮
│        COMBAT VICTORY!              │
├─────────────────────────────────────┤
│                                     │
│  Monsters defeated:                 │
│    Goblin Warrior      +25 XP       │
│    Goblin Archer       +20 XP       │
│    Goblin Shaman       +35 XP       │
│                        ───────      │
│  Total Experience:     +80 XP       │
│                                     │
│  XP: 175/250                        │
│  ██████████████░░░░░░ 70%          │
│                                     │
╰─────────────────────────────────────╯
```

Level up trigger:

```
╭─────────────────────────────────────╮
│                                     │
│  Experience: +50 XP                 │
│  Total: 102/100 XP                  │
│                                     │
│  ⭐ LEVEL UP! ⭐                    │
│  You are now Level 2!               │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-005: Engage in Combat](../player/UC-005-engage-in-combat.md) - Triggers XP award
- [UC-010: Level Up](../player/UC-010-level-up.md) - Triggered by XP threshold
- [UC-110: Spawn Monster](UC-110-spawn-monster.md) - Monster defines base XP

## Implementation Notes

- XP calculation via `ExperienceService.CalculateXP(monster)`
- Award via `ExperienceService.AwardXP(player, amount)`
- Level check via `ProgressionService.CheckLevelUp(player)`
- Thresholds from `ProgressionConfiguration.LevelThresholds`
- Monster XP from `MonsterDefinition.BaseXP`
