# UC-007: Manage Inventory

**Actor:** Player
**Priority:** Medium
**Version:** v0.0.2
**Status:** Implemented

## Description

Player manages their inventory by picking up items, dropping items, and viewing their current possessions. Inventory management does not consume turns.

## Preconditions

- Active game session exists
- Player has an inventory (always true after character creation)

## Basic Flow

1. Player enters "inventory" or "i" command
2. System displays inventory contents:
   - Items grouped by type
   - Stack counts for stackable items
   - Equipped items marked
   - Currency totals
3. No turn consumed

## Alternative Flows

### AF-1: Pick Up Item

1. Player enters "take [item]" or "get [item]"
2. System finds item in current room
3. System adds item to player inventory
4. System removes item from room
5. System displays "You picked up [item]"
6. Turn consumed

### AF-2: Pick Up All Items

1. Player enters "take all"
2. System adds all room items to inventory
3. System removes all items from room
4. System displays list of picked up items
5. Turn consumed

### AF-3: Drop Item

1. Player enters "drop [item]"
2. System finds item in inventory
3. System removes item from inventory
4. System adds item to current room
5. System displays "You dropped [item]"
6. Turn consumed

### AF-4: View Item Details

1. Player enters "examine [item]" (UC-004 AF-1)
2. System displays item details
3. No turn consumed

### AF-5: Pick Up Currency

1. Player enters "take gold" or "take coins"
2. System adds currency to player wallet
3. System removes currency from room
4. System displays "You collected [X] gold"
5. Turn consumed

## Exception Flows

### EF-1: Item Not in Room

1. At step 2 (AF-1): Item not found in room
2. System displays "There's no [item] here"
3. No turn consumed

### EF-2: Item Not in Inventory

1. At step 2 (AF-3): Item not found in inventory
2. System displays "You don't have [item]"
3. No turn consumed

### EF-3: Inventory Full (Future)

1. At step 3 (AF-1): Inventory at capacity
2. System displays "Your inventory is full"
3. Item remains in room
4. No turn consumed

### EF-4: Cannot Drop Equipped Item (Future)

1. At step 2 (AF-3): Item is currently equipped
2. System displays "Unequip [item] first"
3. No turn consumed

## Postconditions

After Pick Up:
- Item moved from room to inventory
- Turn count incremented

After Drop:
- Item moved from inventory to room
- Turn count incremented

After View:
- No state changed

## Business Rules

- Picking up items consumes a turn
- Dropping items consumes a turn
- Viewing inventory does not consume a turn
- Stackable items (potions, etc.) show count
- Currency stored separately from items
- Equipped items cannot be dropped
- Some items may be quest-bound (cannot drop)

## Item Categories

| Category | Examples | Stackable |
|----------|----------|-----------|
| Weapons | Sword, Staff | No |
| Armor | Helmet, Chainmail | No |
| Consumables | Health Potion, Scroll | Yes |
| Currency | Gold, Silver | Yes |
| Quest Items | Key, Artifact | No |
| Misc | Gem, Trophy | Sometimes |

## UI Display

### Inventory View

```
╭─────────────────────────────────────╮
│           INVENTORY                 │
├─────────────────────────────────────┤
│                                     │
│  EQUIPPED:                          │
│    Weapon: Iron Sword               │
│    Armor: Leather Armor             │
│                                     │
│  ITEMS:                             │
│    Health Potion (x3)               │
│    Mana Potion (x2)                 │
│    Rusty Key                        │
│    Dragon Scale                     │
│                                     │
│  CURRENCY:                          │
│    Gold: 150                        │
│    Silver: 45                       │
│                                     │
╰─────────────────────────────────────╯
```

### Pick Up Item

```
╭─────────────────────────────────────╮
│                                     │
│  You pick up the Health Potion.     │
│                                     │
│  [Health Potion added to inventory] │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-004: Examine Environment](UC-004-examine-environment.md) - See items in room
- [UC-008: Use Item](UC-008-use-item.md) - Consume inventory items
- [UC-009: Equip Item](UC-009-equip-item.md) - Equip inventory items
- [UC-014: Collect Loot](UC-014-collect-loot.md) - Auto-collect after combat

## Implementation Notes

- Inventory stored in `Player.Inventory`
- Room items in `Room.Items`
- Item transfer via `Inventory.Add(item)` and `Room.RemoveItem(item)`
- Currency in `Player.Currency` dictionary
- Rendering via `IGameRenderer.RenderInventory()`
