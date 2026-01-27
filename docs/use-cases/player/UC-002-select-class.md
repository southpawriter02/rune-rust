# UC-002: Select Class

**Actor:** Player
**Priority:** High
**Version:** v0.0.4
**Status:** Implemented

## Description

Player selects a class for their character during character creation. The class determines starting abilities, resource type, and stat modifiers.

## Preconditions

- Player has created a character (UC-001)
- Class definitions loaded from configuration
- Archetype definitions loaded from configuration

## Basic Flow

1. System displays archetype selection screen (Warrior, Mystic, Rogue)
2. Player selects an archetype
3. System displays classes available for selected archetype
4. Player selects a class from available options
5. System assigns class to player:
   - Sets player's ArchetypeId and ClassId
   - Applies class stat modifiers to player
   - Initializes primary resource pool
   - Grants starting abilities
6. System displays class assignment confirmation
7. System proceeds to game start

## Alternative Flows

### AF-1: View Class Details

1. At step 4: Player requests details on a class
2. System displays class information:
   - Name and description
   - Stat modifiers
   - Resource type
   - Starting abilities
3. Player returns to class selection
4. Flow continues at step 4

### AF-2: Change Archetype Selection

1. At step 3 or 4: Player requests to go back
2. System returns to archetype selection
3. Flow continues at step 2

## Exception Flows

### EF-1: Class Not Available

1. At step 5: Selected class ID not found in configuration
2. System displays error message
3. System returns to class selection
4. Flow continues at step 3

### EF-2: Resource Type Not Found

1. At step 5: Class's primary resource type not in configuration
2. System logs error
3. System uses default resource (Stamina)
4. Flow continues at step 6

## Postconditions

- Player has assigned ArchetypeId
- Player has assigned ClassId
- Player stats include class modifiers
- Player has initialized resource pool
- Player has starting abilities
- Player is ready to begin game

## Business Rules

- Each archetype has exactly 3 classes
- Classes provide stat modifiers (health, attack, defense)
- Each class has one primary resource type
- Resources initialize based on type (some start at 0, others at max)
- Starting abilities are immediately usable (level 1 unlocks)
- Class assignment is permanent for the game session

## Archetypes and Classes

| Archetype | Classes | Resource |
|-----------|---------|----------|
| **Warrior** | Berserker, Shieldmaiden, Weaponmaster | Rage, Stamina |
| **Mystic** | Pyromancer, Frostweaver, Stormcaller | Mana |
| **Rogue** | Shadowblade, Trickster, Ranger | Focus, Stamina |

## UI Display

```
╭─────────────────────────────────────╮
│         SELECT YOUR CLASS           │
├─────────────────────────────────────┤
│  Archetype: Warrior                 │
│                                     │
│  > [1] Berserker                    │
│        Rage-fueled damage dealer    │
│                                     │
│    [2] Shieldmaiden                 │
│        Stalwart defender            │
│                                     │
│    [3] Weaponmaster                 │
│        Versatile combatant          │
│                                     │
│  [B] Back to Archetype Selection    │
╰─────────────────────────────────────╯
```

## Related Use Cases

- [UC-001: Create Character](UC-001-create-character.md) - Precedes class selection
- [UC-006: Use Ability](UC-006-use-ability.md) - Uses abilities granted here
- [UC-102: Regenerate Resources](../system/UC-102-regenerate-resources.md) - Uses resource initialized here

## Implementation Notes

- Uses `ClassService.GetArchetypes()` and `ClassService.GetClassesForArchetype()`
- Class assigned via `ClassService.AssignClass(player, classDefinition)`
- Resource initialized via `ResourceService.InitializeResources(player, classDef)`
- Abilities granted via `AbilityService.InitializePlayerAbilities(player, classDef)`
