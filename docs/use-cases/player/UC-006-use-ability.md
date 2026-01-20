# UC-006: Use Ability

**Actor:** Player
**Priority:** High
**Version:** v0.0.4
**Status:** Implemented

## Description

Player activates a class ability, spending resources and applying effects to self or target. Abilities provide powerful options beyond basic attacks.

## Preconditions

- Player has selected a class (UC-002)
- Player has the ability (from class or learned)
- Player is in a game session
- If targeting enemy: monster exists in room

## Basic Flow

1. Player enters `use [ability-name]` command
2. System validates ability exists and player has it
3. System checks ability is unlocked (player level >= unlock level)
4. System checks ability is not on cooldown
5. System checks player has sufficient resource
6. System deducts resource cost from player's pool
7. System sets ability cooldown
8. System applies ability effects:
   - Damage to target (if offensive)
   - Healing to self (if restorative)
   - Buffs/debuffs (if applicable)
9. System displays ability result
10. If enemy targeted and alive, monster counterattacks
11. System processes turn end (UC-101)

## Alternative Flows

### AF-1: Self-Targeting Ability

1. At step 8: Ability has `TargetType.Self`
2. No target required
3. Effects apply to player
4. No counterattack triggered

### AF-2: Multi-Target Ability (Future)

1. At step 8: Ability has `TargetType.AllEnemies`
2. Effects apply to all monsters in room
3. Damage/effects may be reduced for multi-target

### AF-3: Specify Target

1. At step 1: Player enters `use fireball goblin`
2. System targets specified monster
3. Flow continues at step 2

### AF-4: View Abilities List

1. Player enters `abilities` or `skills`
2. System displays all known abilities:
   - Name and description
   - Resource cost
   - Current cooldown
   - Locked/unlocked status
3. No turn consumed

## Exception Flows

### EF-1: Ability Not Found

1. At step 2: Ability ID not recognized
2. System displays "Unknown ability: [name]"
3. No turn consumed

### EF-2: Ability Not Known

1. At step 2: Ability exists but player doesn't have it
2. System displays "You don't know that ability"
3. No turn consumed

### EF-3: Ability Locked

1. At step 3: Player level < ability unlock level
2. System displays "Ability locked until level [X]"
3. No turn consumed

### EF-4: On Cooldown

1. At step 4: Ability cooldown > 0
2. System displays "Ability on cooldown ([X] turns remaining)"
3. No turn consumed

### EF-5: Insufficient Resource

1. At step 5: Player resource < ability cost
2. System displays "Insufficient [resource] (need [X], have [Y])"
3. No turn consumed

### EF-6: No Valid Target

1. At step 8: Offensive ability but no monster in room
2. System displays "No target available"
3. No turn consumed

## Postconditions

- Player resource reduced by ability cost
- Ability cooldown set to ability's cooldown value
- Target affected by ability effects
- Turn count incremented
- Turn-end effects processed (cooldowns reduced by 1)

## Business Rules

- Abilities with 0 cooldown can be used every turn
- Resource costs are spent before effects apply
- Damage abilities scale with player stats and ability power
- Self-targeting abilities always have a valid target
- Cooldowns reduce by 1 at turn end
- Abilities can have multiple effects (damage + status effect)

## Ability Categories

| Category | Examples | Typical Effects |
|----------|----------|-----------------|
| Damage | Fireball, Rage Strike | Deal damage to enemy |
| Healing | Heal, Regenerate | Restore player health |
| Buff | Battle Cry, Shield | Increase stats temporarily |
| Debuff | Weaken, Slow | Decrease enemy stats |
| Utility | Teleport, Invisibility | Special movement/effects |

## UI Display

### Use Ability

```
╭─────────────────────────────────────╮
│        🔥 FIREBALL 🔥               │
├─────────────────────────────────────┤
│                                     │
│  You hurl a ball of flame!          │
│                                     │
│  Goblin Warrior takes 25 fire       │
│  damage!                            │
│                                     │
│  Mana: 70/100 (-30)                 │
│  Cooldown: 2 turns                  │
│                                     │
╰─────────────────────────────────────╯
```

### Abilities List

```
╭─────────────────────────────────────╮
│           YOUR ABILITIES            │
├─────────────────────────────────────┤
│                                     │
│  [1] Fireball      30 MP  Ready     │
│      Launch a ball of flame         │
│                                     │
│  [2] Frost Nova    40 MP  2 turns   │
│      Freeze nearby enemies          │
│                                     │
│  [3] Arcane Shield 20 MP  Ready     │
│      Protect yourself               │
│                                     │
│  [🔒] Meteor (Unlocks at level 10)  │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-002: Select Class](UC-002-select-class.md) - Grants starting abilities
- [UC-005: Engage in Combat](UC-005-engage-in-combat.md) - Combat context
- [UC-101: Process Turn End](../system/UC-101-process-turn-end.md) - Cooldown reduction
- [UC-102: Regenerate Resources](../system/UC-102-regenerate-resources.md) - Resource recovery
- [UC-103: Reduce Cooldowns](../system/UC-103-reduce-cooldowns.md) - Cooldown tick

## Implementation Notes

- Ability lookup via `AbilityService.GetAbilityDefinition(id)`
- Validation via `AbilityService.CanUseAbility(player, abilityId)`
- Execution via `AbilityService.UseAbility(player, abilityId, target)`
- Resource spending via `ResourceService.Spend(player, typeId, amount)`
- Cooldown tracking in `PlayerAbility.CurrentCooldown`
