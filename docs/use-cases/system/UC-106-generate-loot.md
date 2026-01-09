# UC-106: Generate Loot

**Actor:** System
**Priority:** Medium
**Version:** v0.0.9
**Status:** Implemented

## Description

System generates loot drops when a monster is defeated, using the monster's loot table to determine items and currency.

## Trigger

- Monster HP reaches 0
- Combat victory

## Preconditions

- Monster has been defeated
- Monster has a loot table defined (or uses default)

## Basic Flow

1. System detects monster death
2. System retrieves monster's loot table
3. For each loot entry in table:
   a. Roll against drop chance
   b. If successful, determine quantity
   c. Create item or currency drop
4. System creates loot container with all drops
5. System adds loot to room (or player if auto-loot)
6. System returns loot summary

## Alternative Flows

### AF-1: Currency Drop

1. Loot entry is currency type
2. Roll for base amount
3. Apply monster tier modifier
4. Create currency drop
5. Add to loot container

### AF-2: Item Drop

1. Loot entry is item type
2. Roll against drop chance
3. If successful, create item instance
4. Apply quality roll (common → legendary)
5. Add to loot container

### AF-3: No Drops

1. All loot rolls fail
2. Guaranteed minimum applies (if any)
3. At least currency usually drops
4. Empty loot if truly unlucky

### AF-4: Bonus Loot

1. Monster has bonus loot modifier
2. Additional drops calculated
3. Quality chances improved
4. Common for elite/boss monsters

### AF-5: Quest Item

1. Monster has quest-specific drop
2. Quest item always drops (if quest active)
3. Added to loot in addition to normal drops

### AF-6: Multiple Monsters

1. Combat had multiple monsters
2. Generate loot for each defeated monster
3. Combine into single loot collection
4. Display combined summary

## Exception Flows

### EF-1: Loot Table Not Found

1. Monster has no loot table defined
2. Use default loot table for tier
3. Log warning
4. Continue loot generation

### EF-2: Item Definition Missing

1. Loot entry references missing item
2. Skip this entry
3. Log warning
4. Continue with other entries

### EF-3: Invalid Quantity

1. Quantity calculation results in <= 0
2. Use minimum of 1
3. Continue processing

## Postconditions

- Loot container created with all drops
- Items and currency instantiated
- Loot placed in room (or player inventory)
- Loot summary available for display

## Business Rules

- Each loot entry has independent drop chance
- Currency always drops (configurable minimum)
- Item quality determined by separate roll
- Monster tier affects currency amounts
- Elite monsters have better loot tables
- Boss monsters have guaranteed rare drops
- Loot persists in room until collected

## Loot Generation Algorithm

```
Generate Loot
      │
      ▼
┌─────────────────────┐
│ Get Loot Table      │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ For Each Entry:     │
│ ┌─────────────────┐ │
│ │ Roll Chance     │ │
│ │ (0-100 vs %)    │ │
│ └────────┬────────┘ │
│          │Success   │
│          ▼          │
│ ┌─────────────────┐ │
│ │ Roll Quantity   │ │
│ │ (min-max range) │ │
│ └────────┬────────┘ │
│          │          │
│          ▼          │
│ ┌─────────────────┐ │
│ │ Create Drop     │ │
│ │ (item/currency) │ │
│ └─────────────────┘ │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Apply Tier Modifier │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Return Loot Drop    │
└─────────────────────┘
```

## Loot Table Example

```json
{
  "monsterId": "goblin-warrior",
  "entries": [
    {
      "type": "currency",
      "currencyId": "gold",
      "min": 5,
      "max": 15,
      "chance": 100
    },
    {
      "type": "item",
      "itemId": "rusty-dagger",
      "chance": 30
    },
    {
      "type": "item",
      "itemId": "health-potion",
      "chance": 15
    }
  ]
}
```

## Quality Tiers

| Quality | Chance | Stat Modifier |
|---------|--------|---------------|
| Common | 60% | 1.0x |
| Uncommon | 25% | 1.15x |
| Rare | 10% | 1.3x |
| Epic | 4% | 1.5x |
| Legendary | 1% | 2.0x |

## Related Use Cases

- [UC-005: Engage in Combat](../player/UC-005-engage-in-combat.md) - Triggers on victory
- [UC-014: Collect Loot](../player/UC-014-collect-loot.md) - Player collects generated loot
- [UC-110: Spawn Monster](UC-110-spawn-monster.md) - Monster has loot table

## Implementation Notes

- Generation via `LootService.GenerateLoot(monster)`
- Loot table from `MonsterDefinition.LootTableId` → `LootTable`
- Random rolls via `DiceService` or `Random`
- Item creation via `ItemFactory.Create(itemId, quality)`
- Currency via `CurrencyDrop` value object
