# UC-108: Roll Initiative

**Actor:** System
**Priority:** Medium
**Version:** v0.0.6
**Status:** Implemented

## Description

System determines combat turn order by rolling initiative for all combatants at the start of an encounter.

## Trigger

- Combat begins (player enters room with monsters, or attacks)
- Surprise round ends

## Preconditions

- Combat encounter initiated
- At least one player and one monster

## Basic Flow

1. System gathers all combatants:
   - Player
   - All monsters in encounter
2. For each combatant:
   a. Roll initiative dice (typically 1d10)
   b. Add initiative modifier (Dexterity, Speed, etc.)
   c. Record total initiative value
3. Sort combatants by initiative (highest first)
4. Resolve ties (player wins ties, or re-roll)
5. Establish turn order
6. First combatant takes their turn

## Alternative Flows

### AF-1: Surprise Round

1. One side has surprise (ambush)
2. Surprised side doesn't roll initiative for first round
3. Ambushers act first
4. Normal initiative from round 2

### AF-2: Tie Breaking - Player Advantage

1. Player and monster roll same initiative
2. Player wins tie automatically
3. Order established

### AF-3: Tie Breaking - Re-Roll

1. Two monsters roll same initiative
2. Re-roll for tied combatants only
3. Continue until resolved

### AF-4: Speed-Based Initiative

1. No dice roll
2. Initiative = Speed stat directly
3. Faster characters always go first
4. Ties broken by other stat

### AF-5: Round-Based Re-Roll (Future)

1. Initiative re-rolled each round
2. Turn order can change
3. More dynamic but slower

### AF-6: Delayed Action (Future)

1. Combatant chooses to delay turn
2. Can act later in round
3. New position in initiative

## Exception Flows

### EF-1: Single Combatant

1. Only one side has combatants
2. No initiative needed
3. That side acts unopposed

### EF-2: Invalid Modifier

1. Combatant has no initiative modifier defined
2. Use default (0)
3. Log warning
4. Continue rolling

## Postconditions

- Initiative values assigned to all combatants
- Turn order established
- Combat ready to proceed
- Current turn pointer set to first combatant

## Business Rules

- Initiative rolled once per encounter
- Higher initiative acts first
- Standard die: 1d10 + modifier
- Player wins ties vs. monsters
- Initiative determines only turn order, not # of actions
- Newly joined combatants roll and insert into order

## Initiative Modifiers

| Source | Modifier |
|--------|----------|
| Dexterity | +1 to +5 |
| Class bonus (Rogue) | +2 |
| Equipment (Light armor) | +1 |
| Status (Slowed) | -3 |
| Monster tier (Elite) | +2 |
| Monster trait (Ambusher) | +3 |

## Initiative Order Example

```
Combat Start!
Rolling initiative...

Combatant       Roll  Mod  Total
─────────────────────────────────
Player          8     +3   11    ← First
Goblin Archer   7     +2   9
Goblin Warrior  6     +0   6
Goblin Shaman   5     +1   6     ← Tied

Resolving tie: Goblin Warrior vs Shaman
Goblin Shaman wins re-roll

Final Order:
1. Player
2. Goblin Archer
3. Goblin Shaman
4. Goblin Warrior
```

## UI Display

Initiative roll:

```
╭─────────────────────────────────────╮
│        ⚔️ COMBAT BEGINS! ⚔️         │
├─────────────────────────────────────┤
│                                     │
│  Rolling initiative...              │
│                                     │
│  You rolled: 8 + 3 = 11             │
│  Goblin Archer: 7 + 2 = 9           │
│  Goblin Warrior: 6 + 0 = 6          │
│                                     │
│  Turn Order:                        │
│  1. [YOU]                           │
│  2. Goblin Archer                   │
│  3. Goblin Warrior                  │
│                                     │
│  Your turn!                         │
│                                     │
╰─────────────────────────────────────╯
```

Surprise round:

```
╭─────────────────────────────────────╮
│        ⚡ SURPRISE ATTACK! ⚡        │
├─────────────────────────────────────┤
│                                     │
│  You caught the enemies off guard!  │
│                                     │
│  SURPRISE ROUND - You act first!    │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-005: Engage in Combat](../player/UC-005-engage-in-combat.md) - Combat context
- [UC-104: Monster Turn](UC-104-monster-turn.md) - Monster's turn in order
- [UC-011: Roll Dice](../player/UC-011-roll-dice.md) - Dice mechanics

## Implementation Notes

- Initiative roll via `InitiativeService.RollInitiative(combatants)`
- Returns `InitiativeOrder` with sorted combatant list
- Roll via `DiceService.Roll(DicePool.D10())`
- Modifier from `Entity.InitiativeModifier` or calculated from stats
- Order stored in `CombatEncounter.InitiativeOrder`
