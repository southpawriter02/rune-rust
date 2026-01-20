# UC-012: Skill Check

**Actor:** Player
**Priority:** Medium
**Version:** v0.0.5
**Status:** Implemented

## Description

Player attempts a skill-based action that requires a dice roll against a difficulty threshold. Success or failure determines the outcome.

## Preconditions

- Active game session exists
- Skill check triggered by game situation or player action
- Skill defined in configuration

## Basic Flow

1. System initiates skill check (or player attempts skilled action)
2. System determines:
   - Skill type (Strength, Dexterity, Perception, etc.)
   - Difficulty Class (DC)
   - Player's skill modifier
3. System rolls dice pool for the skill
4. System adds player's modifier to roll
5. System compares total to DC
6. System determines success/failure
7. System applies outcome:
   - Success: Intended action succeeds
   - Failure: Action fails, possible consequences
8. System displays result

## Alternative Flows

### AF-1: Critical Success

1. At step 4: Natural maximum on all dice
2. System marks as critical success
3. Enhanced outcome applied
4. Special message displayed

### AF-2: Critical Failure

1. At step 4: Natural minimum on all dice
2. System marks as critical failure
3. Negative consequences may apply
4. Special message displayed

### AF-3: Passive Check

1. Skill check without player action
2. System uses player's passive score (10 + modifier)
3. No dice rolled
4. Outcome determined silently

### AF-4: Contested Check

1. Two entities make opposing skill checks
2. Both roll dice + modifier
3. Higher total wins
4. Ties favor defender (or determined by rules)

### AF-5: Advantage/Disadvantage (Future)

1. Situation grants advantage or disadvantage
2. Roll twice
3. Advantage: Take higher result
4. Disadvantage: Take lower result

## Exception Flows

### EF-1: Skill Not Found

1. At step 2: Skill type not in configuration
2. System uses default dice pool
3. Warning logged
4. Check proceeds

### EF-2: Invalid Difficulty

1. At step 2: DC not specified or invalid
2. System uses default DC (10)
3. Warning logged
4. Check proceeds

## Postconditions

- Skill check result recorded
- Action success or failure determined
- Game state updated based on outcome
- Turn may or may not be consumed (context dependent)

## Business Rules

- DC ranges: Easy (5), Medium (10), Hard (15), Very Hard (20)
- Modifiers come from attributes, class, equipment
- Critical success: All dice show maximum
- Critical failure: All dice show minimum
- Some checks are automatic failures below a threshold
- Some checks cannot critically fail

## Difficulty Classes

| Difficulty | DC | Example |
|------------|-----|---------|
| Trivial | 5 | Climbing a ladder |
| Easy | 8 | Forcing a stuck door |
| Medium | 10 | Picking a basic lock |
| Hard | 15 | Disarming a trap |
| Very Hard | 18 | Deciphering ancient text |
| Extreme | 20+ | Impossible feats |

## Skill Types

| Skill | Attribute | Common Uses |
|-------|-----------|-------------|
| Athletics | Strength | Climbing, jumping, swimming |
| Stealth | Dexterity | Sneaking, hiding |
| Perception | Wisdom | Spotting, searching |
| Arcana | Intelligence | Magic knowledge |
| Persuasion | Charisma | Convincing NPCs |
| Survival | Wisdom | Tracking, foraging |

## UI Display

### Skill Check Success

```
╭─────────────────────────────────────╮
│        📊 SKILL CHECK 📊            │
├─────────────────────────────────────┤
│                                     │
│  Attempting: Pick Lock              │
│  Skill: Dexterity                   │
│  Difficulty: 12 (Medium)            │
│                                     │
│  Roll: 2d6 → [5, 4] = 9             │
│  Modifier: +4                       │
│  ─────────────────────              │
│  Total: 13 vs DC 12                 │
│                                     │
│  ✓ SUCCESS!                         │
│  The lock clicks open.              │
│                                     │
╰─────────────────────────────────────╯
```

### Skill Check Failure

```
╭─────────────────────────────────────╮
│        📊 SKILL CHECK 📊            │
├─────────────────────────────────────┤
│                                     │
│  Attempting: Disarm Trap            │
│  Skill: Dexterity                   │
│  Difficulty: 15 (Hard)              │
│                                     │
│  Roll: 2d6 → [2, 3] = 5             │
│  Modifier: +4                       │
│  ─────────────────────              │
│  Total: 9 vs DC 15                  │
│                                     │
│  ✗ FAILURE!                         │
│  The trap triggers!                 │
│  You take 8 damage.                 │
│                                     │
╰─────────────────────────────────────╯
```

### Critical Success

```
╭─────────────────────────────────────╮
│     ⭐ CRITICAL SUCCESS! ⭐         │
├─────────────────────────────────────┤
│                                     │
│  Roll: 2d6 → [6, 6] = 12            │
│  Modifier: +3                       │
│  Total: 15 vs DC 10                 │
│                                     │
│  Exceptional success!               │
│  You find a hidden compartment      │
│  containing extra treasure!         │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-011: Roll Dice](UC-011-roll-dice.md) - Underlying dice mechanics
- [UC-004: Examine Environment](UC-004-examine-environment.md) - Perception checks
- [UC-003: Navigate Dungeon](UC-003-navigate-dungeon.md) - May trigger checks
- [UC-013: Flee Combat](UC-013-flee-combat.md) - Flee is a skill check

## Implementation Notes

- Skill check via `SkillCheckService.PerformCheck(player, skill, dc)`
- Dice roll via `DiceService.Roll(skillDicePool)`
- Modifier calculation from player stats and equipment
- Result returned as `SkillCheckResult` with success, roll, total
