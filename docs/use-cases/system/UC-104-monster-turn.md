# UC-104: Monster Turn

**Actor:** System
**Priority:** High
**Version:** v0.0.6
**Status:** Implemented

## Description

System executes a monster's turn during combat, using AI behavior to select actions like attacking, using abilities, or defensive maneuvers.

## Trigger

- Combat initiative reaches monster's turn
- After player's turn in single-monster combat

## Preconditions

- Active combat encounter
- Monster is alive (HP > 0)
- Monster's turn in initiative order

## Basic Flow

1. System identifies monster taking turn
2. System retrieves monster's AI behavior type
3. System evaluates available actions:
   - Basic attack
   - Special abilities (if any)
   - Defensive actions
4. AI selects action based on behavior and state
5. System executes selected action:
   - Calculate damage (UC-107)
   - Apply effects
6. System displays monster action result
7. System checks for player death
8. System advances to next turn in initiative

## Alternative Flows

### AF-1: Aggressive Behavior

1. AI behavior is `Aggressive`
2. Always attacks
3. Prioritizes lowest HP target (player)
4. Uses strongest available ability

### AF-2: Defensive Behavior

1. AI behavior is `Defensive`
2. Check monster health percentage
3. If HP > 50%: Attack normally
4. If HP <= 50%: Prioritize healing/defense
5. May use defensive ability or basic attack

### AF-3: Cowardly Behavior

1. AI behavior is `Cowardly`
2. Check monster health percentage
3. If HP < 30%: Attempt to flee
4. Otherwise: Attack weakest target

### AF-4: Support Behavior

1. AI behavior is `Support`
2. Check if allies need healing
3. If allies hurt: Heal/buff ally
4. If no allies or all healthy: Attack

### AF-5: Chaotic Behavior

1. AI behavior is `Chaotic`
2. Random action selection
3. May attack, heal, buff, or do nothing
4. Unpredictable but not optimal

### AF-6: Monster Uses Ability

1. Monster has special ability available
2. AI decides to use ability
3. Resource cost checked (if applicable)
4. Ability effects applied
5. Cooldown set

## Exception Flows

### EF-1: Monster Stunned/Disabled

1. Monster has status effect preventing action
2. Turn is skipped
3. Display "Monster is stunned"
4. Advance to next turn

### EF-2: No Valid Targets

1. All potential targets dead/fled
2. Combat should have ended
3. Log warning
4. End combat

### EF-3: Ability on Cooldown

1. Selected ability on cooldown
2. Fall back to basic attack
3. Continue action execution

## Postconditions

- Monster action executed
- Damage/effects applied to target
- Player state updated (HP, status)
- Initiative advances to next combatant

## Business Rules

- Each monster acts once per round
- Action determined by AI behavior + current state
- Monster abilities may have cooldowns
- Some abilities require conditions (HP threshold)
- Monster death during turn ends their action
- Stunned/disabled monsters skip their turn

## AI Behavior Decision Tree

```
Monster Turn Start
       │
       ▼
┌─────────────────┐
│ Check Disabled? │──Yes──► Skip Turn
└────────┬────────┘
         │ No
         ▼
┌─────────────────┐
│ Get AI Behavior │
└────────┬────────┘
         │
    ┌────┴────┬────────┬─────────┬──────────┐
    ▼         ▼        ▼         ▼          ▼
Aggressive Defensive Cowardly Support   Chaotic
    │         │        │         │          │
    ▼         ▼        ▼         ▼          ▼
 Attack    HP>50%?   HP<30%?  Ally hurt? Random
           │    │    │    │   │     │
          Yes   No  Yes   No Yes    No
           │    │    │    │   │     │
        Attack Heal Flee Atk Heal Attack
```

## UI Display

Monster attack:

```
╭─────────────────────────────────────╮
│          MONSTER TURN               │
├─────────────────────────────────────┤
│                                     │
│  Goblin Warrior attacks!            │
│                                     │
│  The goblin swings its rusty sword! │
│  You take 8 damage.                 │
│                                     │
│  Your Health: 85 → 77               │
│                                     │
╰─────────────────────────────────────╯
```

Monster ability:

```
╭─────────────────────────────────────╮
│                                     │
│  Dragon Hatchling uses Fire Breath! │
│                                     │
│  Flames engulf you!                 │
│  You take 15 fire damage.           │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-005: Engage in Combat](../player/UC-005-engage-in-combat.md) - Combat context
- [UC-107: Calculate Damage](UC-107-calculate-damage.md) - Damage calculation
- [UC-108: Roll Initiative](UC-108-roll-initiative.md) - Turn order
- [UC-105: Apply Status Effects](UC-105-apply-status-effects.md) - Effect checks

## Implementation Notes

- Turn execution via `MonsterAIService.ExecuteTurn(monster, combat)`
- Behavior in `MonsterDefinition.AIBehavior`
- Action selection via `MonsterAIService.SelectAction(monster, context)`
- Damage via `CombatService.MonsterAttack(monster, player)`
