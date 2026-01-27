# UC-005: Engage in Combat

**Actor:** Player
**Priority:** High
**Version:** v0.0.2 (Enhanced v0.0.6)
**Status:** Implemented

## Description

Player attacks monsters in the current room. Combat continues until all monsters are defeated, player dies, or player flees.

## Preconditions

- Active game session exists
- One or more monsters in current room
- Player is alive

## Basic Flow

1. Combat initiates (entering room with monsters or first attack)
2. System rolls initiative for all combatants (UC-108)
3. System determines turn order
4. On player's turn:
   - System displays combat options (attack, ability, item, flee)
   - Player chooses "attack" command
5. System calculates damage (UC-107):
   - Base: Player attack value
   - Apply damage type (if weapon equipped)
   - Apply monster resistances
6. System applies damage to monster
7. System displays attack result
8. If monster survives, monster takes turn (UC-104)
9. Repeat steps 4-8 until combat ends
10. When monster defeated:
    - System awards XP (UC-109)
    - System generates loot (UC-106)

## Alternative Flows

### AF-1: Multiple Monsters

1. At step 4: Multiple monsters in room
2. Player enters "attack [monster name]"
3. System attacks specified monster
4. Other monsters act on their turns
5. Combat continues with all monsters

### AF-2: Use Ability Instead

1. At step 4: Player chooses ability (UC-006)
2. Ability effects applied instead of basic attack
3. Flow continues at step 8

### AF-3: Use Item Instead

1. At step 4: Player uses item (UC-008)
2. Item effects applied
3. Flow continues at step 8

### AF-4: Flee Combat

1. At step 4: Player attempts to flee (UC-013)
2. If successful, combat ends
3. Player moves to previous room

### AF-5: Monster Dies Mid-Combat

1. At step 6: Monster health reaches 0
2. System removes monster from combat
3. If other monsters remain, continue combat
4. If last monster, end combat (step 10)

## Exception Flows

### EF-1: No Monsters to Attack

1. At step 4: No monsters in room
2. System displays "There's nothing to attack"
3. No turn consumed

### EF-2: Invalid Target

1. At step 4: Specified monster not in room
2. System displays "That monster isn't here"
3. Player can choose different target

### EF-3: Player Dies

1. During combat: Player health reaches 0
2. System displays death message
3. Game over screen shown
4. Option to load save or start new game

## Postconditions

After Victory:
- All monsters removed from room
- Player awarded XP
- Loot dropped in room
- Turn count incremented

After Death:
- Game session ended
- Game over state

After Flee:
- Player in previous room
- Monsters remain in original room
- Combat ended

## Business Rules

- Basic attack uses player's attack stat
- Damage = Attack - Monster Defense (minimum 1)
- Critical hits deal 150% damage (if implemented)
- Turn order determined by initiative rolls
- Each attack consumes one turn
- XP awarded per monster based on tier and level
- Loot generated from monster's loot table

## Combat Flow Diagram

```
┌─────────────────┐
│ Combat Start    │
│ (Enter room or  │
│  attack command)│
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Roll Initiative │
│ (UC-108)        │
└────────┬────────┘
         │
         ▼
┌─────────────────┐◄──────────────────┐
│ Next Turn       │                   │
│ (by initiative) │                   │
└────────┬────────┘                   │
         │                            │
    ┌────┴────┐                       │
    │ Player? │                       │
    └────┬────┘                       │
    Yes  │  No                        │
    ┌────┴────┐                       │
    ▼         ▼                       │
┌───────┐  ┌───────────┐              │
│ Player│  │ Monster   │              │
│ Action│  │ Turn(104) │              │
└───┬───┘  └─────┬─────┘              │
    │            │                    │
    └─────┬──────┘                    │
          │                           │
          ▼                           │
    ┌───────────┐    Yes              │
    │ All Dead? ├────────────┐        │
    └─────┬─────┘            │        │
          │ No               ▼        │
          │         ┌────────────────┐│
          │         │ Award XP/Loot  ││
          │         │ End Combat     ││
          │         └────────────────┘│
          └───────────────────────────┘
```

## UI Display

```
╭─────────────────────────────────────╮
│            ⚔️ COMBAT ⚔️              │
├─────────────────────────────────────┤
│                                     │
│  Goblin Warrior         [████░] 25  │
│  Goblin Archer          [██░░░] 12  │
│                                     │
├─────────────────────────────────────┤
│                                     │
│  You attack Goblin Warrior!         │
│  You deal 8 damage.                 │
│                                     │
│  Goblin Warrior attacks you!        │
│  You take 5 damage.                 │
│                                     │
├─────────────────────────────────────┤
│  [A]ttack  [U]se Ability  [I]tem    │
│  [F]lee    [L]ook                   │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-006: Use Ability](UC-006-use-ability.md) - Alternative to attack
- [UC-008: Use Item](UC-008-use-item.md) - Healing during combat
- [UC-013: Flee Combat](UC-013-flee-combat.md) - Escape option
- [UC-104: Monster Turn](../system/UC-104-monster-turn.md) - Enemy actions
- [UC-107: Calculate Damage](../system/UC-107-calculate-damage.md) - Damage formula
- [UC-108: Roll Initiative](../system/UC-108-roll-initiative.md) - Turn order
- [UC-109: Award Experience](../system/UC-109-award-experience.md) - XP on victory

## Implementation Notes

- Combat state tracked in `GameSession.IsInCombat`
- Attack via `CombatService.Attack(attacker, target)`
- Damage calculation in `DamageCalculationService`
- Turn order managed by initiative system
