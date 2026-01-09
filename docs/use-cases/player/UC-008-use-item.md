# UC-008: Use Item

**Actor:** Player
**Priority:** Medium
**Version:** v0.0.2
**Status:** Implemented

## Description

Player uses a consumable item from their inventory to gain its effects, such as healing or temporary buffs.

## Preconditions

- Active game session exists
- Player has the item in inventory
- Item is usable (consumable type)

## Basic Flow

1. Player enters "use [item]" command
2. System finds item in player inventory
3. System validates item is usable
4. System applies item effects:
   - Healing: Restore health points
   - Buff: Apply temporary stat increase
   - Restore: Refill resource (mana, etc.)
5. System removes item from inventory (or decrements stack)
6. System displays effect result
7. Turn consumed

## Alternative Flows

### AF-1: Use Healing Potion

1. At step 4: Item type is healing
2. System restores health up to maximum
3. System displays "Restored [X] health"
4. Flow continues at step 5

### AF-2: Use Resource Potion

1. At step 4: Item type is resource restoration
2. System restores resource (mana/rage/etc.)
3. System displays "Restored [X] [resource]"
4. Flow continues at step 5

### AF-3: Use Scroll

1. At step 4: Item is a spell scroll
2. System casts the scroll's spell effect
3. System displays spell result
4. Flow continues at step 5

### AF-4: Use During Combat

1. Using item during combat
2. Item effect applied
3. Monster takes its turn after
4. Turn processing continues

### AF-5: Stacked Item

1. At step 5: Item has stack count > 1
2. System decrements stack count by 1
3. Item remains in inventory with reduced count

## Exception Flows

### EF-1: Item Not Found

1. At step 2: Item not in inventory
2. System displays "You don't have [item]"
3. No turn consumed

### EF-2: Item Not Usable

1. At step 3: Item type is not consumable
2. System displays "[item] cannot be used"
3. No turn consumed
4. May suggest "equip" instead for equipment

### EF-3: Already at Maximum

1. At step 4: Healing when at full health
2. System displays "You're already at full health"
3. Item NOT consumed
4. No turn consumed

### EF-4: Invalid Target

1. At step 4: Item requires target but none available
2. System displays "No valid target for [item]"
3. No turn consumed

## Postconditions

- Item effect applied to player (or target)
- Item removed from inventory (or stack reduced)
- Turn count incremented (if used successfully)
- If in combat, monster turn follows

## Business Rules

- Using items consumes a turn
- Items at full effectiveness (no partial use)
- Healing cannot exceed maximum health
- Buffs stack with existing effects (or refresh duration)
- Some items require specific conditions (combat only, etc.)
- Failed use (already at max) doesn't consume item

## Item Types and Effects

| Type | Example | Effect |
|------|---------|--------|
| Healing | Health Potion | Restore HP |
| Resource | Mana Potion | Restore mana |
| Buff | Strength Elixir | +5 Attack for 5 turns |
| Scroll | Fireball Scroll | Cast Fireball spell |
| Antidote | Antidote | Cure poison status |
| Food | Bread | Small heal over time |

## UI Display

### Use Healing Potion

```
╭─────────────────────────────────────╮
│       ❤️ HEALTH POTION ❤️           │
├─────────────────────────────────────┤
│                                     │
│  You drink the Health Potion.       │
│                                     │
│  Restored 30 health!                │
│  Health: 65 → 95                    │
│                                     │
│  [Health Potion x3 → x2]            │
│                                     │
╰─────────────────────────────────────╯
```

### Use Buff Potion

```
╭─────────────────────────────────────╮
│     💪 STRENGTH ELIXIR 💪          │
├─────────────────────────────────────┤
│                                     │
│  You drink the Strength Elixir.     │
│                                     │
│  Attack increased by 5 for 5 turns! │
│  Attack: 15 → 20                    │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-005: Engage in Combat](UC-005-engage-in-combat.md) - Use items during combat
- [UC-007: Manage Inventory](UC-007-manage-inventory.md) - View items
- [UC-009: Equip Item](UC-009-equip-item.md) - Alternative for equipment
- [UC-101: Process Turn End](../system/UC-101-process-turn-end.md) - Buff duration tick

## Implementation Notes

- Item lookup via `Inventory.FindItem(name)`
- Usage via `ItemService.UseItem(player, item)`
- Effect application via `ItemEffectHandler.Apply(effect, player)`
- Stack management in `Inventory.Remove(item, count)`
- Buff tracking via status effect system
