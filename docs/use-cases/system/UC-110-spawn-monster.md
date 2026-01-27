# UC-110: Spawn Monster

**Actor:** System
**Priority:** Medium
**Version:** v0.0.9
**Status:** Implemented

## Description

System creates a monster instance from a monster definition, applying tier modifiers, traits, and placing it in a room.

## Trigger

- Room generation/population
- Player enters room with monster spawns
- Scripted encounter trigger
- Random encounter roll

## Preconditions

- Monster definition exists in configuration
- Room exists to spawn monster into
- Tier definition loaded (if tier specified)

## Basic Flow

1. System receives spawn parameters:
   - Monster definition ID
   - Tier (or default)
   - Room to spawn in
2. System retrieves monster definition
3. System creates monster instance:
   - Copy base stats from definition
   - Generate unique ID
   - Set initial health to max
4. System applies tier modifiers:
   - Scale health, attack, defense
   - Adjust XP reward
   - Modify loot quality
5. System applies traits (if any):
   - Add trait effects
   - Modify AI behavior
6. System places monster in room
7. System returns spawned monster

## Alternative Flows

### AF-1: Random Tier Selection

1. No specific tier requested
2. Roll for tier based on dungeon level
3. Higher dungeon levels = higher tier chances
4. Apply selected tier

### AF-2: Elite Spawn

1. Tier is Elite or higher
2. Apply significant stat multipliers
3. Add guaranteed trait
4. Enhance loot table

### AF-3: Boss Spawn

1. Tier is Boss
2. Major stat multipliers
3. Multiple traits
4. Unique abilities
5. Special loot table

### AF-4: Group Spawn

1. Spawn multiple monsters at once
2. Each monster created independently
3. Group may have mixed types
4. All added to same room

### AF-5: Named Monster

1. Monster is unique/named
2. Use specific name instead of generic
3. May have predetermined stats
4. Special dialogue or behavior

### AF-6: Trait Assignment

1. Monster definition has trait pool
2. Roll for each trait chance
3. Add successful traits to instance
4. Traits modify stats and behavior

## Exception Flows

### EF-1: Definition Not Found

1. Monster ID not in configuration
2. Return error or spawn default monster
3. Log warning

### EF-2: Tier Not Found

1. Requested tier not configured
2. Use default tier (Common)
3. Log warning
4. Continue spawn

### EF-3: Room Full (Future)

1. Room has maximum monsters
2. Spawn fails
3. Return null or error
4. Log info

## Postconditions

- Monster instance created
- Stats calculated with modifiers
- Traits assigned
- Monster placed in room
- Monster ready for combat

## Business Rules

- Each spawn creates unique instance
- Base stats come from definition
- Tier modifies all stats multiplicatively
- Traits add additional effects
- Monster starts at full health
- AI behavior from definition
- Loot table inherited from definition (tier-modified)

## Monster Tiers

| Tier | Health | Attack | Defense | XP |
|------|--------|--------|---------|-----|
| Minion | 0.5x | 0.5x | 0.5x | 0.5x |
| Common | 1.0x | 1.0x | 1.0x | 1.0x |
| Uncommon | 1.25x | 1.15x | 1.15x | 1.5x |
| Elite | 1.75x | 1.4x | 1.3x | 2.0x |
| Boss | 3.0x | 2.0x | 1.75x | 5.0x |
| Legendary | 5.0x | 3.0x | 2.5x | 10.0x |

## Monster Traits

| Trait | Effect |
|-------|--------|
| Armored | +25% Defense |
| Aggressive | +15% Attack, -10% Defense |
| Cowardly | Flees at 30% HP |
| Regenerating | Heals 5 HP/turn |
| Fire Breath | Has fire attack ability |
| Venomous | Attacks apply poison |
| Pack Hunter | +10% per ally |
| Ambusher | +3 Initiative |

## Spawn Flow

```
Spawn Request
      │
      ▼
┌─────────────────────┐
│ Get Definition      │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Create Instance     │
│ - Generate ID       │
│ - Copy base stats   │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Apply Tier          │
│ - Multiply stats    │
│ - Adjust rewards    │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Roll Traits         │
│ - Check trait pool  │
│ - Apply effects     │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Place in Room       │
└──────────┬──────────┘
           │
           ▼
      Monster Ready
```

## Monster Definition Example

```json
{
  "id": "goblin-warrior",
  "name": "Goblin Warrior",
  "description": "A small but fierce goblin fighter.",
  "baseHealth": 30,
  "baseAttack": 8,
  "baseDefense": 3,
  "baseXP": 25,
  "aiBehavior": "Aggressive",
  "resistances": {
    "fire": 0,
    "ice": 0,
    "physical": 0
  },
  "traitPool": [
    { "traitId": "armored", "chance": 0.1 },
    { "traitId": "aggressive", "chance": 0.2 }
  ],
  "lootTableId": "goblin-common"
}
```

## UI Display

Monster appears:

```
╭─────────────────────────────────────╮
│                                     │
│  A Goblin Warrior appears!          │
│                                     │
│  Health: 30                         │
│  Tier: Common                       │
│                                     │
╰─────────────────────────────────────╯
```

Elite monster:

```
╭─────────────────────────────────────╮
│                                     │
│  ⚠️ An ELITE Goblin Warchief        │
│     appears! ⚠️                     │
│                                     │
│  Health: 52                         │
│  Tier: Elite                        │
│  Traits: Armored, Battle Cry        │
│                                     │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-005: Engage in Combat](../player/UC-005-engage-in-combat.md) - Combat with spawned monster
- [UC-106: Generate Loot](UC-106-generate-loot.md) - Loot from defeated monster
- [UC-109: Award Experience](UC-109-award-experience.md) - XP from monster

## Implementation Notes

- Spawn via `MonsterService.SpawnMonster(definitionId, tier, room)`
- Definition from `IConfigurationProvider.GetMonsterDefinition(id)`
- Instance creation: `Monster.CreateFromDefinition(definition, tier)`
- Tier modifiers from `TierDefinition.Modifiers`
- Trait rolling via random chance checks
