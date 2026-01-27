# UC-014: Collect Loot

**Actor:** Player
**Priority:** Medium
**Version:** v0.0.9
**Status:** Implemented

## Description

Player collects dropped items and currency from defeated monsters. Loot can be collected automatically after combat or manually from the room.

## Preconditions

- Active game session exists
- Loot exists in current room (from defeated monsters)
- Player is not in combat

## Basic Flow

1. System generates loot when monster dies (UC-106)
2. Loot added to room floor
3. Player enters "loot" or "take all" command
4. System collects all items and currency:
   - Items added to inventory
   - Currency added to player wallet
5. System removes loot from room
6. System displays collected items summary
7. Turn consumed (for manual collection)

## Alternative Flows

### AF-1: Auto-Loot After Combat

1. Combat ends with all monsters defeated
2. System automatically collects all loot
3. Items and currency added to player
4. Summary displayed
5. No additional turn consumed

### AF-2: Selective Looting

1. Player enters "take [item]"
2. System finds specific item in loot
3. Item added to inventory
4. Item removed from room
5. Turn consumed

### AF-3: Examine Loot First

1. Player enters "look" or "examine loot"
2. System displays loot details
3. No turn consumed
4. Player can then decide what to take

### AF-4: Currency Only

1. Player enters "take gold" or "take coins"
2. System collects only currency
3. Items remain in room
4. Turn consumed

### AF-5: Inventory Full (Future)

1. At step 4: Cannot fit all items
2. System adds what fits
3. Remaining items stay in room
4. System displays "Inventory full, some items left"

## Exception Flows

### EF-1: No Loot Available

1. At step 3: No loot in room
2. System displays "There's nothing to collect"
3. No turn consumed

### EF-2: Item Not Found

1. At step 3 (AF-2): Specified item not in room
2. System displays "There's no [item] here"
3. No turn consumed

### EF-3: Cannot Take Quest Item

1. Item is quest-bound or untakeable
2. System displays "You cannot take that"
3. Item remains

## Postconditions

- Collected items in player inventory
- Currency added to player wallet
- Loot removed from room
- Turn consumed (if manual collection)

## Business Rules

- Auto-loot enabled by default (can be toggled)
- Manual loot command always available
- Currency collected separately from items
- Loot persists until collected or room reset
- Some items may have pickup requirements
- Quest items may auto-collect

## Loot Types

| Type | Example | Destination |
|------|---------|-------------|
| Currency | Gold, Silver | Player wallet |
| Equipment | Sword, Armor | Inventory |
| Consumables | Potions, Scrolls | Inventory |
| Materials | Dragon Scale, Gem | Inventory |
| Quest Items | Key, Artifact | Inventory (special) |

## Loot Quality Indicators

| Quality | Color | Drop Rate |
|---------|-------|-----------|
| Common | White | 60% |
| Uncommon | Green | 25% |
| Rare | Blue | 10% |
| Epic | Purple | 4% |
| Legendary | Orange | 1% |

## UI Display

### Loot Collection

```
╭─────────────────────────────────────╮
│          💰 LOOT COLLECTED 💰       │
├─────────────────────────────────────┤
│                                     │
│  From: Goblin Warrior               │
│                                     │
│  ITEMS:                             │
│    [Common] Rusty Dagger            │
│    [Uncommon] Health Potion         │
│                                     │
│  CURRENCY:                          │
│    Gold: +15                        │
│    Silver: +32                      │
│                                     │
│  Total Gold: 150 → 165              │
│                                     │
╰─────────────────────────────────────╯
```

### Multiple Monster Loot

```
╭─────────────────────────────────────╮
│          💰 COMBAT LOOT 💰          │
├─────────────────────────────────────┤
│                                     │
│  Monsters Defeated: 3               │
│                                     │
│  ITEMS COLLECTED:                   │
│    Rusty Dagger (x2)                │
│    Health Potion                    │
│    [Rare] Ring of Protection        │
│                                     │
│  CURRENCY:                          │
│    Gold: +45                        │
│                                     │
│  Total Gold: 165 → 210              │
│                                     │
╰─────────────────────────────────────╯
```

### Examine Loot

```
╭─────────────────────────────────────╮
│            ROOM LOOT                │
├─────────────────────────────────────┤
│                                     │
│  Items on ground:                   │
│    - Rusty Dagger (Common)          │
│    - Health Potion (Uncommon)       │
│    - 15 Gold                        │
│                                     │
│  [L]oot All  [T]ake <item>          │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-005: Engage in Combat](UC-005-engage-in-combat.md) - Triggers loot generation
- [UC-007: Manage Inventory](UC-007-manage-inventory.md) - Store collected items
- [UC-004: Examine Environment](UC-004-examine-environment.md) - See loot in room
- [UC-106: Generate Loot](../system/UC-106-generate-loot.md) - Creates the loot

## Implementation Notes

- Loot generation via `LootService.GenerateLoot(monster)`
- Collection via `LootService.CollectLoot(player, room)`
- Currency via `Player.AddCurrency(type, amount)`
- Items via `Inventory.Add(item)`
- Auto-loot setting in `GameSettings.AutoLoot`
