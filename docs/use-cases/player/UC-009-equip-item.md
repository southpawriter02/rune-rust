# UC-009: Equip Item

**Actor:** Player
**Priority:** Medium
**Version:** v0.0.7
**Status:** Implemented

## Description

Player equips a weapon, armor, or accessory from their inventory to gain its stat bonuses and special effects.

## Preconditions

- Active game session exists
- Player has the equipment item in inventory
- Item is equippable (weapon, armor, or accessory)

## Basic Flow

1. Player enters "equip [item]" command
2. System finds item in player inventory
3. System validates item is equippable
4. System determines equipment slot for item
5. If slot occupied, current item is unequipped (moved to inventory)
6. System equips new item:
   - Item moved from inventory to equipment slot
   - Stat bonuses applied to player
   - Special effects activated
7. System displays equipment result
8. No turn consumed

## Alternative Flows

### AF-1: Equip Weapon

1. At step 4: Item is a weapon
2. System uses main hand slot
3. Weapon's attack bonus applied
4. Weapon's damage type set for attacks
5. Flow continues at step 7

### AF-2: Equip Armor

1. At step 4: Item is armor
2. System uses appropriate armor slot (head, chest, etc.)
3. Armor's defense bonus applied
4. Flow continues at step 7

### AF-3: Equip Accessory

1. At step 4: Item is accessory (ring, amulet)
2. System uses accessory slot
3. Accessory bonuses applied
4. Flow continues at step 7

### AF-4: Swap Equipment

1. At step 5: Slot already occupied
2. Current item moved to inventory
3. New item equipped in slot
4. Stats recalculated (remove old, add new)
5. System displays "Unequipped [old], equipped [new]"

### AF-5: Unequip Command

1. Player enters "unequip [slot]" or "unequip [item]"
2. System removes item from equipment slot
3. Item added to inventory
4. Stat bonuses removed
5. System displays "Unequipped [item]"

### AF-6: View Equipment

1. Player enters "equipment" or "eq"
2. System displays all equipment slots:
   - Slot name
   - Equipped item (or empty)
   - Bonuses provided
3. No turn consumed

## Exception Flows

### EF-1: Item Not Found

1. At step 2: Item not in inventory
2. System displays "You don't have [item]"
3. No action taken

### EF-2: Item Not Equippable

1. At step 3: Item cannot be equipped (consumable, etc.)
2. System displays "[item] cannot be equipped"
3. May suggest "use" instead

### EF-3: Class/Level Restriction

1. At step 3: Item has class or level requirement not met
2. System displays "Requires [class] or level [X]"
3. No action taken

### EF-4: Inventory Full on Swap

1. At step 5: Inventory full, cannot hold unequipped item
2. System displays "Inventory full, cannot swap"
3. No action taken

## Postconditions

- Item moved from inventory to equipment slot
- Player stats updated with equipment bonuses
- Previous item (if any) moved to inventory
- Equipment effects active

## Business Rules

- Equipping does not consume a turn
- Each slot holds exactly one item
- Equipment bonuses are additive
- Equipment can have level requirements
- Equipment can have class restrictions
- Unequipping always succeeds if inventory has space
- Equipped items cannot be dropped without unequipping

## Equipment Slots

| Slot | Item Types | Primary Stat |
|------|------------|--------------|
| Main Hand | Weapons | Attack |
| Off Hand | Shield, Dagger | Defense/Attack |
| Head | Helmet, Hood | Defense |
| Chest | Armor, Robe | Defense |
| Hands | Gloves, Gauntlets | Attack/Defense |
| Feet | Boots, Shoes | Defense |
| Ring | Rings | Varies |
| Amulet | Amulets, Necklaces | Varies |

## UI Display

### Equip Item

```
╭─────────────────────────────────────╮
│      ⚔️ EQUIPMENT CHANGE ⚔️         │
├─────────────────────────────────────┤
│                                     │
│  Equipped: Steel Longsword          │
│  Slot: Main Hand                    │
│                                     │
│  Attack: 15 → 22 (+7)               │
│                                     │
│  Unequipped: Iron Sword             │
│  [Iron Sword added to inventory]    │
│                                     │
╰─────────────────────────────────────╯
```

### View Equipment

```
╭─────────────────────────────────────╮
│           EQUIPMENT                 │
├─────────────────────────────────────┤
│                                     │
│  Main Hand: Steel Longsword (+7 Atk)│
│  Off Hand:  (empty)                 │
│  Head:      Iron Helm (+2 Def)      │
│  Chest:     Chainmail (+5 Def)      │
│  Hands:     (empty)                 │
│  Feet:      Leather Boots (+1 Def)  │
│  Ring:      Ring of Strength (+2 Atk│
│  Amulet:    (empty)                 │
│                                     │
│  Total Bonuses:                     │
│    Attack: +9  Defense: +8          │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-007: Manage Inventory](UC-007-manage-inventory.md) - Store unequipped items
- [UC-005: Engage in Combat](UC-005-engage-in-combat.md) - Equipment affects combat
- [UC-107: Calculate Damage](../system/UC-107-calculate-damage.md) - Weapon damage type

## Implementation Notes

- Equipment slots in `Player.Equipment` dictionary
- Slot lookup via `EquipmentService.GetSlotForItem(item)`
- Equip via `EquipmentService.Equip(player, item)`
- Stat recalculation via `Player.RecalculateStats()`
- Equipment bonuses in `EquipmentDefinition.Bonuses`
