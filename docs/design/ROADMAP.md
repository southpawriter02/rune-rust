# Rune & Rust - Implementation Version Roadmap

---

## Architecture Principles: Data-Driven & Customizable Design

All game systems should be implemented with **high customizability** in mind. Rather than hardcoding game mechanics, terminology, or values, the architecture should support easy modification and extension through configuration and data definitions.

### Core Principles

1. **Data-Driven Configuration**
   - Game constants, formulas, and terminology should be defined in configuration files (JSON/YAML) or database tables, not hardcoded in source
   - Examples: attribute names, progression curves, combat formulas, UI labels

2. **Extensible Type Systems**
   - Use definition-based systems rather than enums where customization is expected
   - New types (races, classes, damage types, resources) should be addable without code changes
   - Example: Instead of `enum DamageType { Fire, Ice, Poison }`, use a `DamageTypeDefinition` entity loaded from data

3. **Terminology Abstraction**
   - All player-facing terminology should be configurable
   - Examples:
     - "Level" could be renamed to "Saga", "Rank", or "Tier"
     - "XP" could be "Legend", "Glory", or "Essence"
     - "Health" could be "Vitality", "Life Force", or "HP"
     - "Mana" could be "Arcane Power", "Spirit", or "Focus"

4. **Pluggable Resource Systems**
   - Support multiple resource types beyond Health (Mana, Rage, Energy, Focus, etc.)
   - Resources should be class-specific or universal based on configuration
   - Resource regeneration, caps, and costs defined per-resource-type

5. **Flexible Class/Ability Architecture**
   - Classes defined as data with stat modifiers, available abilities, and resource pools
   - Ability trees/progression paths loaded from configuration
   - Support for multi-classing or hybrid builds through data definitions

6. **Attribute Flexibility**
   - Core attributes (Strength, Dexterity, etc.) defined in data, not code
   - Attribute effects on derived stats (damage, defense, etc.) defined by formulas in configuration
   - Support for custom attributes per game variant

### Implementation Guidelines

When implementing features, prefer:

```
✅ DO: Load from configuration
- AttributeDefinitions loaded from JSON defining name, abbreviation, description, derived effects
- ClassDefinitions specifying base stats, growth rates, abilities, resource types
- ProgressionTable defining level thresholds, rewards, terminology

❌ DON'T: Hardcode values
- Avoid: enum PlayerClass { Warrior, Mage, Rogue, Cleric }
- Avoid: const int MAX_LEVEL = 20;
- Avoid: string levelUpMessage = "You gained a level!";
```

### Example: Customizable Progression System

Instead of hardcoding "XP" and "Level":

```csharp
// Configuration (progression.json)
{
  "progressionTerminology": {
    "experiencePointsName": "Legend",
    "experiencePointsAbbreviation": "LGD",
    "levelName": "Saga",
    "levelUpMessage": "Your legend grows! You have reached Saga {0}!"
  },
  "levelThresholds": [0, 100, 300, 600, 1000, ...],
  "levelRewards": [
    { "level": 1, "statPoints": 0, "abilitySlots": 1 },
    { "level": 2, "statPoints": 2, "abilitySlots": 1 },
    ...
  ]
}
```

### Example: Customizable Resource Types

Support for class-specific resources:

```csharp
// Configuration (resources.json)
{
  "resourceTypes": [
    {
      "id": "health",
      "displayName": "Vitality",
      "color": "#FF0000",
      "regenPerTurn": 0,
      "isUniversal": true
    },
    {
      "id": "mana",
      "displayName": "Arcane Power",
      "color": "#0066FF",
      "regenPerTurn": 5,
      "classRestriction": ["Mage", "Cleric"]
    },
    {
      "id": "rage",
      "displayName": "Fury",
      "color": "#FF6600",
      "regenPerTurn": -10,
      "buildsOnHit": 15,
      "classRestriction": ["Warrior"]
    }
  ]
}
```

This approach ensures the game engine is a **flexible framework** that can power many different game experiences through data changes alone.

---

## Current State: v0.0.1 (Walking Skeleton) - COMPLETE

The walking skeleton is complete with:
- Clean Architecture foundation (Domain, Application, Infrastructure, Presentation layers)
- 5-room starter dungeon with interconnected rooms
- Basic movement (N/S/E/W)
- Simple turn-based combat (single monster)
- Inventory system (20-item capacity)
- Item pickup
- TUI with Spectre.Console
- GUI placeholder shell with AvaloniaUI
- In-memory repository (PostgreSQL infrastructure ready)
- 33 passing tests

---

## v0.0.2 - Core Game Loop Polish
[v0.0.2 Design Specification](v0.0.x/v0.0.2-design-specification.md)

**Focus:** Stabilize the foundation and add essential missing commands

### Features
- [ ] **Load Game Command** - Implement actual load game functionality from saved sessions
- [ ] **Drop Command** - Allow players to drop items from inventory (`drop <item>`)
- [ ] **Use Command** - Consume items like health potions (`use <item>`)
- [ ] **Item Effects System** - Health potions restore HP, scrolls provide buffs
- [ ] **Examine Command** - Get detailed information about items or room features (`examine <target>`)
- [ ] **Status Command** - Display detailed player stats (`status` or `stats`)
- [ ] **Room Re-entry Messages** - Different text when revisiting a room vs first visit

### Technical Debt
- [x] Add logging throughout game flow with Serilog
- [ ] Implement PostgreSQL repository (currently using in-memory)
- [ ] Add input validation and error handling improvements

### Developer Tools
- [ ] **Cheat Code System** - Debug commands for development and testing:
  - `godmode` / `god` - Toggle invincibility
  - `heal [amount]` - Restore health (full if no amount specified)
  - `kill [target]` - Instantly defeat a monster (or all if no target)
  - `teleport <room>` / `tp <room>` - Move to any room by name or ID
  - `setstat <stat> <value>` - Modify player stats (attack, defense, health, etc.)
  - `spawn <item>` - Generate any item into inventory
  - `loot` - Generate random loot drop
  - `gold <amount>` - Add currency
  - `levelup [levels]` - Gain levels instantly
  - `reveal` - Show all rooms on map
  - `noclip` - Ignore movement restrictions
- [ ] **Cheat Code Configuration** - Enable/disable cheats, restrict to debug builds
- [ ] **Cheat Code Logging** - Track when cheats are used (for debugging)

---

## v0.0.3 - Player Creation & Identity
[v0.0.3 Design Specification](v0.0.x/v0.0.3-design-specification.md)

**Focus:** Rich player creation experience with data-driven customization foundation

### Features
- [ ] **Player Name Validation** - Enforce name rules (length, allowed characters)
- [ ] **Player Race Selection** - Races loaded from configuration (default: Human, Elf, Dwarf, Halfling)
- [ ] **Player Background/Lineage** - Customized stat bonuses and starter abilities based on chosen past profession or geographic upbringing
- [ ] **Starting Stats Configuration** - Point-buy for stats (Might, Fortitude, Will, Wits, and Finesse)
- [ ] **Player Description** - Optional backstory/description field
- [ ] **Character Summary Screen** - Display full character sheet
- [ ] **Multiple Save Slots** - Support multiple characters/save games
- [ ] **Delete Save Command** - Remove saved games

### Customization Foundation
- [ ] **Game Terminology Configuration** - JSON/YAML file for all player-facing text
- [ ] **Race Definitions** - Data-driven race definitions with stat modifiers, descriptions
- [ ] **Attribute Definitions** - Configurable attribute names (Strength, Dexterity, etc.)

### Terminology & Lexicon System
- [ ] **Lexicon Configuration File** - Central JSON/YAML defining all game terminology:
  - Core stats terminology (HP → "Vitality", MP → "Arcane Power", etc.)
  - Action verbs (attack → "strike", "slash", "swing"; move → "walk", "stride", "venture")
  - Quantity descriptors ("a few", "several", "many", "horde")
  - Condition descriptors ("wounded", "bloodied", "near death")
  - Success/failure language ("hits", "connects", "lands" vs "misses", "whiffs", "goes wide")
- [ ] **Lexicon Service** - Runtime service for retrieving terminology with context
- [ ] **Synonym Pools** - Multiple words for the same concept to add variety
- [ ] **Context-Aware Selection** - Choose appropriate term based on situation (formal vs casual, combat vs exploration)

### Descriptor System (Text Variety Engine)
- [ ] **Setting Aesthetic/Theme** - Determine the style and type of descriptors to use (horror, high fantasy, dark fantasy, etc.)
- [ ] **Environmental Descriptors** - Atmospheric text pools for room descriptions:
  - Lighting ("dimly lit", "bathed in torchlight", "shrouded in darkness", "pale light filters through")
  - Sounds ("dripping water echoes", "distant rumbling", "eerie silence", "scratching in the walls")
  - Smells ("musty air", "damp stone", "rotting vegetation", "sulfurous fumes")
  - Temperature ("bone-chilling cold", "oppressive heat", "damp chill", "stale warmth")
  - Textures ("rough-hewn stone", "slick moss", "crumbling mortar", "ancient brickwork")
- [ ] **Interactive Object Descriptors** - Rich descriptions for examinable items:
  - Containers ("weathered chest", "ornate coffer", "rusted lockbox", "ancient urn")
  - Furniture ("rickety table", "collapsed throne", "rotting bookshelf", "overturned cart")
  - Architectural ("crumbling pillar", "iron-banded door", "spiral staircase", "collapsed archway")
- [ ] **Puzzle Descriptors** - Clue and mechanism descriptions:
  - Mechanisms ("ancient gears grind", "runes flicker", "pressure plate clicks", "crystal resonates")
  - Clues ("faded inscription reads", "symbols arranged in pattern", "dust disturbed recently")
  - States ("partially solved", "reset to original position", "mechanism jammed")
- [ ] **Examine Descriptors** - Detailed inspection text:
  - Item conditions ("pristine", "battle-worn", "corroded", "masterfully crafted")
  - Hidden details ("upon closer inspection", "barely visible etching", "concealed compartment")
  - Lore hints ("bears the mark of", "style suggests origin from", "craftsmanship indicates")
- [ ] **Combat Descriptors** - Attack and damage variety:
  - Hit descriptions ("strikes true", "slashes deeply", "pierces armor", "connects solidly")
  - Miss descriptions ("swings wide", "deflected away", "narrowly dodged", "parried expertly")
  - Damage severity ("grazes", "wounds", "grievously injures", "devastates")
  - Death descriptions ("collapses", "crumbles to dust", "lets out a final cry", "falls silent")
- [ ] **Skill Check Descriptors** - Dice roll outcome flavor:
  - Critical success ("spectacular success", "masterful execution", "beyond expectations")
  - Success ("accomplished", "succeeded", "managed to", "pulled off")
  - Failure ("fell short", "couldn't quite", "fumbled", "failed to")
  - Critical failure ("catastrophic failure", "disastrous attempt", "spectacular misfortune")
- [ ] **Ability Descriptors** - Spell and skill effects:
  - Casting ("channels energy", "incants ancient words", "focuses power", "draws upon")
  - Effects ("flames erupt", "frost spreads", "shadows coalesce", "light blazes")
  - Duration ("fades gradually", "dissipates", "lingers", "intensifies")
- [ ] **Dice Result Descriptors** - Roll narration:
  - Natural 1 ("the dice betray you", "fortune turns against", "cruel twist of fate")
  - Low rolls ("luck is not on your side", "barely missing the mark")
  - High rolls ("fortune favors you", "the odds align", "skillful execution")
  - Natural max ("destiny itself intervenes", "legendary success", "the stars align")
- [ ] **NPC Appearance Descriptors** - Character description pools:
  - Physical features ("weathered face", "keen eyes", "scarred hands", "imposing stature")
  - Clothing ("tattered robes", "gleaming armor", "humble garb", "ornate vestments")
  - Demeanor ("nervous glance", "confident stance", "weary expression", "eager smile")
  - Voice ("gravelly tone", "melodic speech", "hesitant whisper", "booming voice")
- [ ] **Descriptor Weighting** - Frequency and context rules for selection
- [ ] **Descriptor Combinations** - Rules for combining multiple descriptors coherently

### Data Model Updates
- [ ] Create `RaceDefinition` entity loaded from configuration (not enum)
- [ ] Create `AttributeDefinition` entity for customizable stats
- [ ] Create `GameTerminology` service for loading display text
- [ ] Create `LexiconService` for terminology and synonym management
- [ ] Create `DescriptorService` for context-aware text generation
- [ ] Create `DescriptorPool` entity for grouped descriptor sets
- [ ] Add `Description` field to Player
- [ ] Update DTOs and ViewModels

---

## v0.0.4 - Player Classes & Abilities
[v0.0.4 Design Specification](v0.0.x/v0.0.4-design-specification.md)

**Focus:** Data-driven class system with configurable abilities and resources

### Features
- [ ] **Player Archetypes** - Base/core class types from which players can then specialize into more specific classes (Warrior -> Shieldmaiden; Skirmisher -> Shadow-Walker; Adept -> Scrap-Tinker; Mystic -> Galdr-Caster/Blood-Priest)
- [ ] **Player Classes** - Classes loaded from configuration (default: Shieldmaiden, Galdr-Caster, Shadow-Walker, Blood-Cleric, Scrap-Tinker)
- [ ] **Class-Specific Stats** - Base stats, growth rates defined per class in data
- [ ] **Class-Specific Resources** - Each class can have unique resource pools:
  - Warrior: Rage (builds on hit, decays over time)
  - Mage: Mana (regenerates per turn)
  - Rogue: Energy (fast regeneration)
  - Cleric: Faith (builds from healing/support)
- [ ] **Special Abilities** - Abilities defined in configuration with:
  - Resource costs (which resource, how much)
  - Cooldowns (turns between uses)
  - Effects (damage, healing, buffs, etc.)
- [ ] **Ability Command** - Use special ability (`ability <name>` or `skill <name>`)
- [ ] **Class Selection UI** - TUI selection during character creation

### Customization Features
- [ ] **Class Definitions File** - JSON defining all classes with stats, resources, abilities
- [ ] **Resource Type Definitions** - Configurable resource types (Health, Mana, Rage, Energy, Focus, etc.)
- [ ] **Ability Definitions** - Data-driven abilities with formula-based effects
- [ ] **Class Requirements** - Optional race/stat requirements for classes

### Data Model Updates
- [ ] Create `ClassDefinition` entity loaded from configuration (not enum)
- [ ] Create `ResourceTypeDefinition` for custom resource pools
- [ ] Create `ResourcePool` value object for tracking current/max values
- [ ] Create `AbilityDefinition` entity with effects, costs, cooldowns
- [ ] Add `Resources` dictionary to Player (keyed by resource type ID)
- [ ] Add `Abilities` collection to Player with cooldown tracking

---

## v0.0.5 - Dice Pool System

**Focus:** Core randomization engine for all game mechanics

### Features
- [ ] **Dice Pool Engine** - Configurable dice pools (e.g., 3d6, 2d10+5)
- [ ] **Dice Types** - d4, d6, d8, d10
- [ ] **Roll Modifiers** - Bonuses/penalties from stats, equipment, abilities
- [ ] **Advantage/Disadvantage** - Roll multiple dice, take best/worst
- [ ] **Exploding Dice** - Max roll triggers additional die
- [ ] **Roll Display** - Show individual dice results to player
- [ ] **Skill Checks** - Dice-based success/failure for actions
- [ ] **Configurable Difficulty** - Target numbers for checks

### Data Model Updates
- [ ] Create `DicePool` value object
- [ ] Create `DiceRoll` result object with breakdown
- [ ] Add `DiceService` for all randomization
- [ ] Add skill check thresholds to actions

---

## v0.0.6 - Enhanced Combat System

**Focus:** Deeper combat mechanics using dice pools

### Features
- [ ] **Multi-Monster Rooms** - Multiple enemies in a single room
- [ ] **Target Selection** - Choose which monster to attack (`attack <target>`)
- [ ] **Combat Initiative** - Dice roll + modifier for turn order
- [ ] **Attack Rolls** - Dice pool vs defense to determine hits
- [ ] **Damage Rolls** - Dice-based damage calculation
- [ ] **Critical Hits** - Natural max roll for bonus damage
- [ ] **Miss/Dodge** - Failed attack rolls miss entirely
- [ ] **Monster AI Behaviors** - Aggressive, defensive, fleeing
- [ ] **Flee Command** - Dice check to escape combat (`flee` or `run`)
- [ ] **Combat Log** - Detailed scrolling combat history with dice results

### Status Effects System
- [ ] **Status Effect Framework** - Data-driven status effect definitions:
  - Duration (turns, permanent until cured, or triggered removal)
  - Stacking rules (refreshes duration, stacks intensity, or blocked)
  - Application conditions (on hit, on damage, on ability use)
  - Removal conditions (rest, item use, time, specific action)
- [ ] **Negative Status Effects (Debuffs)**:
  - **Bleeding** - Damage over time, worsens with movement
  - **Poisoned** - Damage over time, may spread to lower stats
  - **Burning** - Fire damage over time, can spread to environment
  - **Frozen** - Reduced movement/actions, vulnerability to shatter
  - **Stunned** - Skip next action, reduced defenses
  - **Knocked Down** - Must spend action to stand, vulnerability to attacks
  - **Weakened** - Reduced attack/damage output
  - **Blinded** - Reduced accuracy, cannot target distant enemies
  - **Slowed** - Reduced movement and initiative
  - **Feared** - Must flee, cannot attack source of fear
  - **Silenced** - Cannot use abilities requiring incantation
  - **Cursed** - Various negative effects, harder to heal
  - **Exhausted** - Reduced stamina/energy regeneration
  - **Disarmed** - Cannot use equipped weapon
- [ ] **Positive Status Effects (Buffs)**:
  - **Fortified** - Increased defense/damage resistance
  - **Hasted** - Extra actions or movement
  - **Regenerating** - Health restored each turn
  - **Strengthened** - Increased attack/damage
  - **Shielded** - Absorbs incoming damage
  - **Invisible** - Cannot be targeted, bonus to stealth
  - **Blessed** - Bonus to all rolls, resistance to curses
  - **Inspired** - Bonus to skill checks and critical chance
  - **Protected** - Reduced damage from specific damage type
  - **Enraged** - Increased damage, reduced defense
- [ ] **Environmental Status Effects**:
  - **Wet** - Vulnerability to lightning/ice, resistance to fire
  - **On Fire** - Spreading fire damage (environmental)
  - **Chilled** - Slowed, vulnerability to freeze
  - **Electrified** - Chance to stun on contact
- [ ] **Status Effect Display** - Show active effects on player/monster status
- [ ] **Status Effect Interactions** - Combinations create new effects (wet + lightning = bonus damage)
- [ ] **Cleanse/Cure Mechanics** - Items and abilities to remove negative effects

### Data Model Updates
- [ ] Add `Initiative` stat or calculation
- [ ] Add attack/damage dice pools to weapons
- [ ] Add `AIBehavior` enum to Monster
- [ ] Create `StatusEffectDefinition` entity with all effect properties
- [ ] Create `ActiveStatusEffect` value object for applied effects
- [ ] Add `ActiveEffects` collection to Player and Monster
- [ ] Create `StatusEffectService` for applying/removing/ticking effects

---

## v0.0.7 - Equipment System

**Focus:** Wearable and wieldable items

### Features
- [ ] **Equipment Slots** - Weapon, Armor, Shield, Helmet, Boots, Ring, Amulet
- [ ] **Equip Command** - Put on equipment (`equip <item>`)
- [ ] **Unequip Command** - Remove equipment (`unequip <slot>`)
- [ ] **Equipment Stats** - Items modify player stats when equipped
- [ ] **Equipment Display** - Show equipped items in status/inventory
- [ ] **Weapon Types** - Swords, axes, daggers, staffs (different damage dice)
- [ ] **Armor Types** - Light, medium, heavy (defense vs speed tradeoffs)
- [ ] **Equipment Requirements** - Class or stat requirements for items

### Data Model Updates
- [ ] Create `EquipmentSlot` enum
- [ ] Add `Equipment` dictionary to Player
- [ ] Add equipment-related properties to Item (slot, stat modifiers, requirements)

---

## v0.0.8 - Experience & Leveling

**Focus:** Fully customizable progression system with configurable terminology

### Features
- [ ] **Experience Points** - Gain XP from defeating monsters (terminology configurable)
- [ ] **Level System** - Level up at configurable thresholds
- [ ] **Stat Increases** - Gain stats on level up (amounts defined in progression config)
- [ ] **Level-Up Notification** - Configurable messages and effects
- [ ] **Ability Unlocks** - New abilities at certain levels (defined per class)
- [ ] **Monster XP Values** - Different XP for different monsters
- [ ] **XP Display** - Show current XP and progress to next level

### Customization Features
- [ ] **Progression Terminology** - Configurable names for XP ("Legend", "Glory", "Essence") and Level ("Saga", "Rank", "Tier")
- [ ] **Progression Curve** - JSON-defined level thresholds (linear, exponential, custom)
- [ ] **Level Rewards Configuration** - Per-level rewards (stat points, ability slots, unlocks)
- [ ] **Class-Specific Progression** - Different progression rates/rewards per class
- [ ] **Prestige/Rebirth System** - Optional reset mechanics for post-cap progression

### Data Model Updates
- [ ] Create `ProgressionDefinition` loaded from configuration
- [ ] Create `LevelReward` entity for per-level bonuses
- [ ] Add `Experience`, `Level` properties to Player (using configured terminology in display)
- [ ] Add `ExperienceValue` to Monster
- [ ] Create `ProgressionService` for level-up logic with configurable formulas

---

## v0.0.9 - Monster Variety & Loot

**Focus:** Data-driven monster/mob system with configurable types, abilities, and loot

### Features
- [ ] **Monster Types** - Monsters loaded from definitions (default: Goblins, Skeletons, Orcs, etc.)
- [ ] **Monster Tiers** - Configurable tier system (default: Common, Named, Elite, Boss)
- [ ] **Loot Tables** - Data-driven loot with weighted drops
- [ ] **Gold/Currency** - Configurable currency system (name, icon, drop rates)
- [ ] **Loot Command** - Pick up all loot after combat
- [ ] **Monster Descriptions** - Examine monsters for info, uncover unique details from examination checks
- [ ] **Monster Special Attacks** - Monsters can have traits from a predefined/shared pool of behaviors (run away, flying, swimming, heal self), damage resistances/vulnerabilities,abilities, etc.
- [ ] **Damage Types** - Configurable damage types with resistances/vulnerabilities

### Customization Features
- [ ] **Monster Definitions** - JSON files defining monster stats, abilities, loot, behavior
- [ ] **Damage Type Definitions** - Configurable damage types (Fire, Ice, Poison, Holy, etc.)
- [ ] **Resistance System** - Percentage-based resistances/vulnerabilities per damage type
- [ ] **Tier Definitions** - Configurable tier names, stat multipliers, loot bonuses
- [ ] **Currency Configuration** - Custom currency names ("Gold", "Coins", "Runes", "Souls")

### Data Model Updates
- [ ] Create `MonsterDefinition` loaded from configuration (not enum)
- [ ] Create `DamageTypeDefinition` entity
- [ ] Create `TierDefinition` for monster difficulty scaling
- [ ] Create `LootTable` with weighted item/currency drops
- [ ] Create `CurrencyDefinition` for customizable money systems
- [ ] Add `Resistances` dictionary to creatures
- [ ] Add `Currency` dictionary to Player (supports multiple currency types)

---

## v0.1.0 - Expanded Dungeon & Z-Axis

**Focus:** Vertical exploration and larger dungeons

### Features
- [ ] **Z-Axis Movement** - Up/down commands for stairs/ladders
- [ ] **Multiple Dungeon Levels** - Deeper = harder enemies, better loot
- [ ] **Stairs Up/Down** - Room connections between Z-levels
- [ ] **Procedural Room Generation** - Basic random dungeon generation
- [ ] **Room Types** - Treasure rooms, trap rooms, boss rooms, safe rooms
- [ ] **Hidden Passages** - Discoverable secret exits (dice check to find)
- [ ] **Search Command** - Look for hidden items/passages (`search`)
- [ ] **Map Command** - ASCII map of explored rooms (`map`)

### Data Model Updates
- [ ] Extend `Position` to include Z coordinate
- [ ] Add `RoomType` enum
- [ ] Add `IsDiscovered`, `IsHidden` properties to exits
- [ ] Add visited/explored tracking to rooms

---

## v0.1.1 - Dynamic Room Generation

**Focus:** Infinite playability through randomization

### Features
- [ ] **Room Templates** - Reusable room patterns with variable content
- [ ] **Dungeon Seeds** - Reproducible random dungeons from seed values
- [ ] **Difficulty Scaling** - Rooms harder based on depth and distance from start
- [ ] **Procedural Descriptions** - Randomized room flavor text
- [ ] **Dynamic Monster Placement** - Random enemies based on room type/difficulty
- [ ] **Dynamic Item Placement** - Random loot based on room type/difficulty
- [ ] **Branch Generation** - Random side paths and dead ends
- [ ] **Special Room Probability** - Configurable chance for treasure/boss rooms
- [ ] **Infinite Dungeon Mode** - Endless procedural generation option

### Data Model Updates
- [ ] Create `RoomTemplate` entity
- [ ] Create `DungeonGenerator` service
- [ ] Add `Seed` property to Dungeon
- [ ] Add `DifficultyRating` to Room

---

## v0.1.2 - Dungeon Theming & Biomes
[v0.1.2 Index](v0.1.x/v0.1.2-index.md)

**Focus:** Environmental variety through themed dungeon zones

### Features
- [ ] **Biome System** - Configurable dungeon zone types (Catacombs, Sewers, Mines, Ruins, Frozen, Volcanic)
- [ ] **Biome Definitions** - Data-driven `biomes.json` with descriptors, spawn pools, modifiers
- [ ] **Biome-Specific Monsters** - Themed monster spawn pools per zone
- [ ] **Biome-Specific Hazards** - Environmental dangers per zone (poison gas, lava, ice)
- [ ] **Biome-Specific Loot** - Themed crafting materials and equipment
- [ ] **Transitional Zones** - Blended areas between biomes with mixed content
- [ ] **Biome Discovery** - Codex unlocks and exploration tracking

### Data Model Updates
- [ ] Create `BiomeDefinition` entity loaded from configuration
- [ ] Create `BiomeSpawnTable` for monster/item/hazard pools
- [ ] Add `BiomeId` property to Room
- [ ] Create `BiomeService` for biome-aware generation

---

## v0.1.3 - Advanced Procedural Architecture
[v0.1.3 Index](v0.1.x/v0.1.3-index.md)

**Focus:** Coherent, varied dungeon layouts with architectural intelligence

### Features
- [ ] **Architectural Styles** - Natural Caves, Carved Halls, Ruined Structures, Ancient Temples
- [ ] **Room Shape Variety** - Circular, L-shaped, irregular caverns (not just rectangles)
- [ ] **Landmark Rooms** - Hand-crafted centerpieces in procedural layouts
- [ ] **Structural Coherence** - Logical architecture (corridors, pillars, consistent materials)
- [ ] **Dungeon Age & Decay** - Fresh, Ancient, Collapsed, Reclaimed states

### Data Model Updates
- [ ] Create `ArchitecturalStyle` entity with rules/descriptors
- [ ] Create `RoomShape` enum (Rectangular, Circular, LShaped, Irregular)
- [ ] Create `LandmarkRoom` entity for special rooms
- [ ] Add `Shape`, `Age` to Room

---

## v0.1.4 - World Persistence & Evolution
[v0.1.4 Index](v0.1.x/v0.1.4-index.md)

**Focus:** Living dungeons that remember and change

### Features
- [ ] **Persistent World State** - Cleared rooms, opened chests, solved puzzles persist
- [ ] **Monster Repopulation** - Configurable respawn timers and spawner logic
- [ ] **Faction Territory Control** - Monster groups claim/contest dungeon zones
- [ ] **Environmental Storytelling** - Procedural evidence (battle sites, abandoned camps)
- [ ] **Dungeon Evolution Events** - Cave-ins, migrations, seasonal effects

### Data Model Updates
- [ ] Create `RoomState` entity tracking persistent changes
- [ ] Create `FactionDefinition` entity for monster groups
- [ ] Create `TerritoryControl` for zone ownership
- [ ] Add `LastCleared`, `RespawnTimer`, `ControllingFaction` to Room
- [ ] Create `WorldStateService` for persistence logic

---

## v0.1.5 - Procedural Puzzles & Secrets
[v0.1.5 Index](v0.1.x/v0.1.5-index.md)

**Focus:** Dynamic puzzle and secret generation

### Features
- [ ] **Procedural Puzzles** - Generated brain teasers with templates (sequence, combination, riddle)
- [ ] **Secret Room Generation** - Hidden passages with skill check discovery
- [ ] **Dynamic Trap Placement** - Trap types and density by biome/architecture
- [ ] **Treasure Room Variety** - Templates with guardians and puzzle-locked vaults

### Data Model Updates
- [ ] Create `PuzzleTemplate` entity for procedural puzzles
- [ ] Create `SecretRoomTemplate` for hidden areas
- [ ] Create `TrapPlacementRules` for dynamic traps
- [ ] Update `DungeonGenerator` with puzzle/secret integration

---

## v0.2.0 - NPCs & Dialogue

**Focus:** Non-combat interactions and merchant systems

### Features
- [ ] **NPC Entity** - Friendly characters in the dungeon
- [ ] **Talk Command** - Initiate dialogue (`talk` or `talk <npc>`)
- [ ] **Dialogue Trees** - Branching conversations with choices
- [ ] **Dialogue Outcomes** - Choices affect relationships, unlock options
- [ ] **NPC Types** - Merchants, quest givers, information sources, companions
- [ ] **NPC Attitudes** - Friendly, neutral, hostile (can change based on actions)
- [ ] **Persuasion Checks** - Dice rolls to influence NPC responses
- [ ] **NPC Memory** - NPCs remember previous interactions
- [ ] **Faction/Reputation** - NPCs react differently to a player's standing with their community (talk with hostility, attack automatically, act friendly and give unique conversation options)
- [ ] **NPC Companion** - Recruit NPCs to help fight in combat that you can control, equip, and train

### Data Model Updates
- [ ] Create `NPC` entity with attitude tracking
- [ ] Create `Dialogue` and `DialogueOption` entities
- [ ] Create `DialogueCondition` for branching logic
- [ ] Add `NPCs` collection to Room

---

## v0.2.1 - Codex & Lore Discovery

**Focus:** Discovery-based world-building and collectible knowledge

### Features
- [ ] **Codex System** - In-game encyclopedia unlocked through discovery:
  - `codex` command - Open the codex interface
  - `codex <topic>` - View specific entry
  - `codex search <term>` - Search codex entries
- [ ] **Codex Categories**:
  - **Bestiary** - Monster entries with stats, weaknesses, lore (unlocked by defeating/examining)
  - **Locations** - Region and dungeon lore (unlocked by exploration)
  - **Items & Artifacts** - Detailed item lore (unlocked by acquiring/examining)
  - **Characters** - NPC backgrounds and histories (unlocked through dialogue)
  - **Factions** - Groups, guilds, and organizations (unlocked by encounters)
  - **History** - World events and timeline (unlocked by finding documents/talking to NPCs)
  - **Legends** - Myths, prophecies, and tales (pieced together from fragments)
  - **Crafting Knowledge** - Material properties and recipe hints
- [ ] **Discovery Mechanics**:
  - Partial entries - Initial discovery reveals basic info, deeper investigation reveals more
  - Examination reveals lore - Using `examine` on items/monsters adds codex entries
  - Combat reveals weaknesses - Fighting monsters reveals their vulnerabilities
  - Dialogue unlocks entries - NPCs share knowledge that populates codex
  - Found documents - Books, scrolls, inscriptions add entries
  - Cross-references - Codex entries link to related topics
- [ ] **Codex Progress Tracking**:
  - Completion percentage per category
  - Total discovery progress
  - Achievement unlocks for codex milestones
- [ ] **Lore Fragments**:
  - Scattered pieces of larger lore entries
  - Collecting all fragments completes the entry
  - Fragments found in different locations/sources
- [ ] **Codex UI**:
  - TUI: Navigable text-based interface with categories and search
  - GUI: Visual codex with images, formatted text, and navigation
- [ ] **Codex Hints** - Incomplete entries suggest where to find more information

### Data Model Updates
- [ ] Create `CodexEntry` entity with categories, content, unlock conditions
- [ ] Create `CodexFragment` for partial lore pieces
- [ ] Create `CodexProgress` tracking per-player discoveries
- [ ] Create `CodexCategory` enum/definition for organization
- [ ] Add `UnlocksCodexEntry` property to monsters, items, NPCs, rooms
- [ ] Create `CodexService` for managing discovery and retrieval

---

## v0.2.2 - Merchant System

**Focus:** Economy and trading

### Features
- [ ] **Shop System** - Buy/sell items with gold
- [ ] **Shop UI** - TUI interface for shopping with item comparison
- [ ] **Dynamic Pricing** - Prices vary by merchant, charisma, reputation
- [ ] **Haggle Command** - Dice check to negotiate better prices
- [ ] **Merchant Inventory** - Limited stock that refreshes
- [ ] **Specialty Merchants** - Weaponsmith, armorer, alchemist, general store
- [ ] **Sell Command** - Sell items from inventory (`sell <item>`)
- [ ] **Buy Command** - Purchase items from merchant (`buy <item>`)
- [ ] **Item Appraisal** - See item value before selling

### Data Model Updates
- [ ] Create `Merchant` entity extending NPC
- [ ] Create `ShopInventory` with refresh logic
- [ ] Add `BasePrice`, `SellPrice` to Item
- [ ] Add `Charisma` stat to Player

---

## v0.3.0 - Quest Foundation

**Focus:** Core quest entity and basic objective tracking

### Features
- [ ] **Quest Entity** - Trackable objectives with status
- [ ] **QuestObjective Entity** - Individual goals within a quest
- [ ] **Quest Status** - Tracking: Available, Active, Completed, Failed
- [ ] **Basic Quest Types** - Kill quests, fetch quests, explore quests
- [ ] **Quest Journal** - View active and completed quests (`quests` or `journal`)
- [ ] **Quest Progress Tracking** - "Kill 5 goblins (3/5)"
- [ ] **Quest Acceptance** - Accept quests via dialogue (`accept quest`)
- [ ] **Quest Completion** - Turn in completed quests for rewards

### Data Model Updates
- [ ] Create `Quest` entity with objectives and status
- [ ] Create `QuestDefinition` for quest templates
- [ ] Create `QuestObjective` entity with progress tracking
- [ ] Create `ObjectiveType` enum (Kill, Collect, Explore, Talk)
- [ ] Create `QuestStatus` enum (Available, Active, Completed, Failed)
- [ ] Add `ActiveQuests`, `CompletedQuests` to Player
- [ ] Create `QuestService` for quest management

---

## v0.3.1 - Quest Rewards & NPCs

**Focus:** Quest rewards system and NPC quest giver integration

### Features
- [ ] **Quest Rewards** - XP, gold, items, reputation for completion
- [ ] **QuestReward Entity** - Configurable reward bundles
- [ ] **Quest NPCs** - NPCs marked as quest givers
- [ ] **Quest Giver Indicators** - Visual markers for NPCs with quests
- [ ] **Quest Dialogue Integration** - Accept/decline through dialogue
- [ ] **Reward Distribution** - Automatic reward on turn-in
- [ ] **Reputation Rewards** - Quest completion affects faction standing
- [ ] **Quest Item Rewards** - Specific items granted on completion
- [ ] **Quest Prerequisites** - Level, reputation, or prior quest requirements

### Data Model Updates
- [ ] Create `QuestReward` value object for reward bundles
- [ ] Create `RewardType` enum (XP, Gold, Item, Reputation)
- [ ] Create `QuestPrerequisite` value object
- [ ] Add `HasAvailableQuests` to NPC entity
- [ ] Add `OfferedQuests` collection to NPC
- [ ] Update DialogueOutcome with `StartQuest` type
- [ ] Create `QuestRewardService` for reward distribution

---

## v0.3.2 - Quest Chains & Advanced Features

**Focus:** Quest chains, failure conditions, and narrative structure

### Features
- [ ] **Quest Chains** - Completing one quest unlocks the next
- [ ] **Main Quest Line** - Story-driven sequence of quests
- [ ] **Side Quests** - Optional quests for extra rewards
- [ ] **Failed Quests** - Time limits or impossible conditions
- [ ] **Quest Categories** - Main, Side, Daily, Repeatable
- [ ] **Escort Quests** - Protect an NPC to a destination
- [ ] **Timed Quests** - Must complete within time limit
- [ ] **Quest Abandonment** - Option to abandon active quests
- [ ] **Quest Log Filters** - Filter by category, status, or region

### Data Model Updates
- [ ] Create `QuestChain` entity linking related quests
- [ ] Create `QuestCategory` enum (Main, Side, Daily, Repeatable)
- [ ] Add `TimeLimit` to Quest entity
- [ ] Add `FailureConditions` to Quest entity
- [ ] Add `ChainId`, `ChainOrder` to Quest entity
- [ ] Add `FailedQuests` collection to Player
- [ ] Create `EscortObjective` extending QuestObjective
- [ ] Create `QuestChainService` for chain management
- [ ] Update QuestService with failure detection

---

## v0.4.0 - Interactive Objects Foundation
[v0.4.0 Index](v0.4.x/v0.4.0-index.md)

**Focus:** Core interactive object system and basic environment interaction

### Features
- [ ] **InteractiveObject Entity** - Base class for all interactable objects
- [ ] **Interact Command** - Primary interaction (`interact <object>` or `use <object>`)
- [ ] **Object States** - Open/closed, locked/unlocked, active/inactive, broken
- [ ] **Containers** - Chests, crates, barrels with inventory
- [ ] **Open/Close Commands** - Toggle container states (`open <object>`, `close <object>`)
- [ ] **Doors** - Passage blockers requiring interaction to pass
- [ ] **Keys & Locks** - Key items unlock matching locked objects
- [ ] **Lockpicking** - Dice check to bypass locks without keys
- [ ] **Basic Levers** - Single-state toggles that affect other objects
- [ ] **Buttons** - Momentary triggers for immediate effects
- [ ] **Destructible Objects** - Break crates, webs, barriers with attacks

### Data Model Updates
- [ ] Create `InteractiveObject` entity with state tracking
- [ ] Create `ObjectState` enum (Open, Closed, Locked, Active, Inactive, Broken)
- [ ] Create `InteractionType` enum (Open, Close, Unlock, Activate, Break, Search)
- [ ] Create `InteractiveObjectDefinition` for configuration
- [ ] Add `Interactables` collection to Room
- [ ] Create `LockDefinition` with key requirements and lockpick difficulty
- [ ] Create `ContainerInventory` for object-held items
- [ ] Create `InteractionService` for handling interactions

---

## v0.4.1 - Traps & Environmental Hazards
[v0.4.1 Index](v0.4.x/v0.4.1-index.md)

**Focus:** Dangerous environmental elements and trap mechanics

### Features
- [ ] **Trap Entity** - Hidden dangers with trigger conditions
- [ ] **Trap Detection** - Perception/search dice check to spot traps
- [ ] **Trap Disarm** - Dice check to safely disable traps
- [ ] **Trap Trigger** - Automatic trigger when conditions met
- [ ] **Trap Effects** - Damage, status effects, alerts, environmental changes
- [ ] **Trapped Chests** - Containers with attached traps
- [ ] **Pressure Plates** - Floor triggers activated by stepping
- [ ] **Tripwires** - Line triggers activated by crossing
- [ ] **Dart Traps** - Ranged damage when triggered
- [ ] **Pit Traps** - Fall damage and potential separation
- [ ] **Environmental Hazards** - Poison gas, fire, ice, spikes
- [ ] **Hazard Zones** - Areas with ongoing damage/effects
- [ ] **Dice Saves** - Saving throws to reduce/avoid hazard damage

### Data Model Updates
- [ ] Create `Trap` entity with trigger and effect definitions
- [ ] Create `TrapDefinition` for configuration
- [ ] Create `TrapState` enum (Hidden, Detected, Triggered, Disarmed, Broken)
- [ ] Create `TriggerType` enum (Step, Touch, Open, Proximity, Tripwire)
- [ ] Create `HazardZone` entity for area effects
- [ ] Create `HazardDefinition` with damage type, save DC, effects
- [ ] Add `Traps` collection to Room
- [ ] Add `HazardZones` collection to Room
- [ ] Create `TrapService` for detection, disarm, and trigger logic

---

## v0.4.2 - Puzzles & Riddles
[v0.4.2 Index](v0.4.x/v0.4.2-index.md)

**Focus:** Brain teasers and logic challenges

### Features
- [ ] **Puzzle Entity** - Logic challenges with solution tracking
- [ ] **Puzzle States** - Unsolved, Partially Solved, Solved, Failed
- [ ] **Sequence Puzzles** - Activate objects in correct order
- [ ] **Combination Locks** - Number/symbol combinations to unlock
- [ ] **Pattern Puzzles** - Match or reproduce visual/audio patterns
- [ ] **Riddle NPCs** - Answer riddles to progress or receive rewards
- [ ] **Riddle Command** - Respond to riddles (`answer <response>`)
- [ ] **Puzzle Rooms** - Dedicated rooms with puzzle-gated rewards
- [ ] **Puzzle Hints** - Optional hints via examine or dice checks
- [ ] **Puzzle Reset** - Failed puzzles can reset after time/action
- [ ] **Puzzle Rewards** - Items, access, or information on solve
- [ ] **Multi-Part Puzzles** - Puzzles spanning multiple rooms/objects

### Data Model Updates
- [ ] Create `Puzzle` entity with solution and state tracking
- [ ] Create `PuzzleDefinition` for configuration
- [ ] Create `PuzzleState` enum (Unsolved, InProgress, Solved, Failed, Locked)
- [ ] Create `PuzzleType` enum (Sequence, Combination, Pattern, Riddle, Logic)
- [ ] Create `PuzzleAttempt` for tracking player progress
- [ ] Create `RiddleDefinition` with question, answers, hints
- [ ] Create `SequencePuzzle` with ordered step requirements
- [ ] Create `CombinationPuzzle` with code and input tracking
- [ ] Create `PuzzleService` for puzzle logic and validation
- [ ] Add `Puzzles` collection to Room

---

## v0.4.3 - Light & Environment Systems
[v0.4.3 Index](v0.4.x/v0.4.3-index.md)

**Focus:** Lighting mechanics and advanced environmental features

### Features
- [ ] **Light Level System** - Room illumination affecting gameplay
- [ ] **Light Sources** - Torches, lanterns, magical light, windows
- [ ] **Darkness Penalties** - Reduced accuracy, perception in dark areas
- [ ] **Light/Dark Creatures** - Monsters with light sensitivity or dark vision
- [ ] **Torch Item** - Consumable light source with duration
- [ ] **Lantern Item** - Reusable light with fuel consumption
- [ ] **Light Command** - Manage light sources (`light torch`, `extinguish`)
- [ ] **Dark Vision** - Racial/ability trait to see in darkness
- [ ] **Light-Based Puzzles** - Reflect light, illuminate symbols
- [ ] **Day/Night Cycle** - Optional time-based light changes (surface areas)
- [ ] **Weather Effects** - Rain, fog affecting visibility (outdoor areas)

### Player Skills Foundation
- [ ] **Skill Entity** - Trainable player abilities
- [ ] **Acrobatics** - Jump, climb, balance, tumble
- [ ] **Stealth** - Move silently, hide, ambush
- [ ] **Perception** - Spot traps, secrets, hidden objects
- [ ] **Lockpicking** - Pick locks without keys
- [ ] **Field Medicine** - Heal self/others, cure status effects
- [ ] **Wilderness Survival** - Forage, track, navigate
- [ ] **Rhetoric** - Persuasion, intimidation, deception

### Data Model Updates
- [ ] Create `LightLevel` enum (Bright, Dim, Dark, MagicalDarkness)
- [ ] Create `LightSource` entity with radius and duration
- [ ] Create `LightSourceDefinition` for configuration
- [ ] Add `LightLevel` property to Room
- [ ] Add `LightSources` collection to Room
- [ ] Create `VisionType` enum (Normal, DarkVision, TrueSight)
- [ ] Add `VisionType` to Player and Monster
- [ ] Create `SkillDefinition` for configurable skills
- [ ] Create `PlayerSkill` entity with proficiency tracking
- [ ] Add `Skills` collection to Player
- [ ] Create `SkillService` for skill checks and progression

---

## v0.5.0 - Combat Grid Foundation
[v0.5.0 Index](v0.5.x/v0.5.0-index.md)

**Focus:** Core grid system and basic positioning mechanics

### Features
- [ ] **Combat Grid** - 2D grid for tactical positioning
- [ ] **GridPosition Value Object** - X,Y coordinates on combat grid
- [ ] **Grid Initialization** - Create grid when combat begins
- [ ] **Entity Placement** - Position player and monsters on grid
- [ ] **Grid Display** - ASCII grid showing positions
- [ ] **Basic Grid Movement** - Move during combat (`move <direction>`)
- [ ] **Movement Points** - Limited movement per turn based on stats
- [ ] **MovementSpeed Stat** - Creature stat determining movement points
- [ ] **Turn Movement Tracking** - Track movement used per turn

### Data Model Updates
- [ ] Create `CombatGrid` entity with dimensions and cell tracking
- [ ] Create `GridPosition` value object (X, Y coordinates)
- [ ] Create `GridCell` value object for cell contents
- [ ] Add `MovementSpeed` stat to Player and Monster
- [ ] Add `GridPosition` to Player and Monster (combat only)
- [ ] Add `MovementPointsRemaining` to combat state
- [ ] Create `CombatGridService` for grid management
- [ ] Update `CombatService` with grid integration

---

## v0.5.1 - Range & Melee Combat
[v0.5.1 Index](v0.5.x/v0.5.1-index.md)

**Focus:** Range-based attack mechanics and adjacency requirements

### Features
- [ ] **Range Property** - Weapons and abilities have range values
- [ ] **Melee Range** - Must be adjacent to attack in melee (range 1)
- [ ] **Ranged Weapons** - Bows, crossbows, thrown weapons with range requirements
- [ ] **Ranged Spells** - Spells with configurable range
- [ ] **Range Checking** - Validate target is within weapon/ability range
- [ ] **Range Display** - Show valid targets based on range
- [ ] **Minimum Range** - Some ranged weapons can't attack adjacent enemies
- [ ] **Range Penalties** - Optional accuracy reduction at long range
- [ ] **Line of Sight** - Basic LOS check for ranged attacks

### Data Model Updates
- [ ] Add `Range` property to Item (weapons)
- [ ] Add `MinRange` property to Item (optional)
- [ ] Add `Range` property to AbilityDefinition
- [ ] Create `RangeType` enum (Melee, Ranged, Reach)
- [ ] Add range validation to `CombatService`
- [ ] Create `LineOfSightService` for LOS calculations
- [ ] Update attack commands with range checking

---

## v0.5.2 - Terrain & Cover
[v0.5.2 Index](v0.5.x/v0.5.2-index.md)

**Focus:** Environmental grid features affecting combat

### Features
- [ ] **Terrain Types** - Different ground types on grid cells
- [ ] **Difficult Terrain** - Costs extra movement points to traverse
- [ ] **Impassable Terrain** - Walls, pits that block movement
- [ ] **Hazardous Terrain** - Damaging terrain (fire, acid, spikes)
- [ ] **Cover System** - Objects provide defense bonuses
- [ ] **Partial Cover** - +2 defense from half cover
- [ ] **Full Cover** - Cannot be targeted from blocked angle
- [ ] **Cover Objects** - Pillars, crates, walls provide cover
- [ ] **Destructible Cover** - Cover objects can be destroyed
- [ ] **Terrain Display** - Show terrain types on ASCII grid

### Data Model Updates
- [ ] Create `TerrainType` enum (Normal, Difficult, Impassable, Hazardous)
- [ ] Create `TerrainDefinition` for configurable terrain
- [ ] Add `TerrainType` to `GridCell`
- [ ] Create `CoverType` enum (None, Partial, Full)
- [ ] Create `CoverObject` entity for cover-providing objects
- [ ] Add `Cover` collection to `CombatGrid`
- [ ] Update movement cost calculations with terrain
- [ ] Update defense calculations with cover bonuses

---

## v0.5.3 - Tactical Positioning
[v0.5.3 Index](v0.5.x/v0.5.3-index.md)

**Focus:** Advanced positioning mechanics and reactions

### Features
- [ ] **Flanking** - Bonus for attacking from behind/sides
- [ ] **Flanking Detection** - Determine if attacker has flanking advantage
- [ ] **Flanking Bonus** - Configurable attack/damage bonus when flanking
- [ ] **Opportunity Attacks** - Moving away from enemies triggers attack
- [ ] **Threatened Squares** - Track which squares are threatened by enemies
- [ ] **Disengage Action** - Move without triggering opportunity attacks
- [ ] **Area Effects** - Spells/abilities affect multiple grid cells
- [ ] **Area Shapes** - Circle, cone, line, square AoE templates
- [ ] **AoE Targeting** - Select center point for area effects
- [ ] **AoE Display** - Show affected cells when targeting AoE

### Data Model Updates
- [ ] Create `FacingDirection` enum (N, NE, E, SE, S, SW, W, NW)
- [ ] Add `Facing` to creatures on grid
- [ ] Create `FlankingService` for flanking calculations
- [ ] Create `ThreatService` for opportunity attack tracking
- [ ] Create `AreaEffectShape` enum (Circle, Cone, Line, Square)
- [ ] Create `AreaEffect` value object with shape and radius
- [ ] Add `AreaEffect` to AbilityDefinition
- [ ] Create `AreaEffectService` for AoE targeting and resolution
- [ ] Update `CombatService` with flanking and opportunity attacks

---

## v0.6.0 - TUI Enhancements

**Focus:** Rich terminal experience

### Features
- [ ] **ASCII Art Rooms** - Visual room representations
- [ ] **Colored Text Themes** - Consistent color scheme for message types
- [ ] **Health Bars** - Visual HP display for player and monsters
- [ ] **Inventory Grid** - Visual inventory layout
- [ ] **Mini-Map** - Persistent map display
- [ ] **Status Bar** - Always-visible player status
- [ ] **Combat Animations** - Text-based attack effects
- [ ] **Combat Grid View** - ASCII tactical grid during combat
- [ ] **Screen Layouts** - Organized panels for different info
- [ ] **Command History** - Arrow keys to recall previous commands
- [ ] **Tab Completion** - Auto-complete commands and item names

---

## v0.7.0 - GUI Foundation

**Focus:** Playable graphical interface

### Features
- [ ] **Main Menu Window** - New game, load, settings, quit
- [ ] **Game Window Layout** - Room view, status panel, inventory, command input
- [ ] **Room Display Panel** - Text-based room description area
- [ ] **Player Status Panel** - HP, stats, equipped items
- [ ] **Inventory Panel** - Clickable item grid
- [ ] **Command Input** - Text input for commands
- [ ] **Message Log** - Scrollable game output
- [ ] **Context Menus** - Right-click on items/monsters for actions
- [ ] **Combat Grid Panel** - Visual tactical grid for combat

---

## v0.8.0 - GUI Polish & Features

**Focus:** Full GUI feature parity with TUI

### Features
- [ ] **Character Creation Wizard** - Step-by-step character setup
- [ ] **Map View** - Visual dungeon map with explored rooms
- [ ] **Combat UI** - Visual combat interface with action buttons
- [ ] **Clickable Grid Combat** - Point-and-click tactical movement
- [ ] **Shop Interface** - Visual buy/sell windows
- [ ] **Quest Log Window** - Dedicated quest tracking
- [ ] **Dialogue Window** - Visual conversation interface
- [ ] **Puzzle UI** - Interactive puzzle solving interface
- [ ] **Settings Window** - Audio, display, gameplay options
- [ ] **Tooltips** - Hover info for items and abilities
- [ ] **Keyboard Shortcuts** - Full keyboard navigation

---

## v0.9.0 - Audio & Atmosphere

**Focus:** Immersive experience

### Features
- [ ] **Background Music** - Ambient dungeon music (GUI)
- [ ] **Sound Effects** - Combat sounds, item pickup, UI feedback (GUI)
- [ ] **TUI Bell/Sounds** - Console beep for important events
- [ ] **Audio Settings** - Volume controls, mute options
- [ ] **Music Themes** - Different tracks for areas (combat, safe, boss)
- [ ] **Puzzle Sounds** - Audio feedback for puzzle interactions

---

## v0.10.0 - Advanced Combat & Ability Trees

**Focus:** Strategic depth with ability progression and status effects

### Features
- [ ] **Buff/Debuff System** - Data-driven status effects with configurable durations, stacking
- [ ] **Area-of-Effect Abilities** - Damage zones on combat grid
- [ ] **Combo System** - Chained abilities for bonus effects (defined in ability data)
- [ ] **Monster Groups** - Coordinated enemy tactics
- [ ] **Boss Mechanics** - Special phases, unique abilities, phase transitions
- [ ] **Defense Actions** - Block, dodge, parry commands
- [ ] **Combat Stances** - Aggressive, defensive, balanced (configurable)
- [ ] **Environmental Combat** - Push enemies into hazards
- [ ] **Zone Control** - Abilities that affect grid areas over time

### Ability Tree System
- [ ] **Ability Trees** - Hierarchical ability unlocks per class
- [ ] **Talent Points** - Earned on level-up, spent to unlock abilities
- [ ] **Tree Branches** - Multiple specialization paths per class
- [ ] **Prerequisites** - Abilities can require other abilities or stats
- [ ] **Ability Upgrades** - Improve existing abilities with additional investment
- [ ] **Respec System** - Allow players to reset and reallocate points

### Customization Features
- [ ] **Status Effect Definitions** - JSON-defined effects (poison, burn, stun, haste, etc.)
- [ ] **Ability Tree Definitions** - Data-driven talent trees per class
- [ ] **Combo Definitions** - Configurable ability chains with bonus effects
- [ ] **Stance Definitions** - Customizable combat stances with stat modifiers

### Data Model Updates
- [ ] Create `StatusEffectDefinition` entity
- [ ] Create `AbilityTreeDefinition` with nodes and connections
- [ ] Create `TalentPointAllocation` for player ability investments
- [ ] Create `ComboDefinition` for ability chains
- [ ] Create `StanceDefinition` for combat stances

---

## v0.11.0 - Crafting & Resources

**Focus:** Item creation

### Features
- [ ] **Resource Items** - Ore, herbs, leather, gems
- [ ] **Crafting Recipes** - Combine resources to create items
- [ ] **Crafting Stations** - Anvil (weapons), workbench (armor), alchemy table (potions)
- [ ] **Craft Command** - Create items (`craft <recipe>`)
- [ ] **Recipe Discovery** - Find recipes in the dungeon
- [ ] **Resource Gathering** - Harvest from room features (dice check)
- [ ] **Crafting Dice Checks** - Quality varies based on roll

---

## v0.12.0 - Achievements & Statistics

**Focus:** Meta-progression and tracking

### Features
- [ ] **Achievement System** - Unlock achievements for milestones
- [ ] **Statistics Tracking** - Monsters killed, damage dealt, items found, puzzles solved, etc.
- [ ] **Leaderboards** - High scores (local)
- [ ] **Achievement Notifications** - Pop-up on unlock
- [ ] **Statistics View** - Detailed stats screen
- [ ] **Playtime Tracking** - Total time played
- [ ] **Dice Roll History** - Track lucky/unlucky streaks

---

## v1.0.0 - Release Polish

**Focus:** Complete, polished experience

### Features
- [ ] **Tutorial System** - Guided introduction for new players
- [ ] **Help System** - Context-sensitive help (`help <topic>`)
- [ ] **Balance Pass** - Tune all stats, XP, loot tables
- [ ] **Bug Fixes** - Address all known issues
- [ ] **Performance Optimization** - Ensure smooth gameplay
- [ ] **Accessibility Options** - Screen reader support, high contrast mode
- [ ] **Localization Framework** - Prepare for multi-language support
- [ ] **Full Documentation** - Complete game manual and developer docs
- [ ] **Installer/Distribution** - Packaged releases for Windows, macOS, Linux

### Quality Assurance
- [ ] Full test coverage for all game systems
- [ ] Integration tests for save/load
- [ ] Performance benchmarks
- [ ] Usability testing

---

## Future Considerations (Post v1.0.0)

### Multiplayer
- Co-op dungeon crawling
- PvP arena modes
- Shared world servers

### Full Modding Support
Building on the data-driven architecture, provide complete modding capabilities:
- **Mod Loader** - Load custom definition files from mod folders
- **Total Conversion Mods** - Replace all game content with custom data
- **Custom Classes** - Community-created classes with unique resources and abilities
- **Custom Races** - New playable races with stat modifiers and traits
- **Custom Dungeons** - User-created dungeons with custom rooms, monsters, loot
- **Custom Items** - New weapons, armor, consumables with custom effects
- **Custom Abilities** - New spells, skills, and ability trees
- **Mod Conflicts Resolution** - Handle overlapping definitions gracefully
- **Steam Workshop Integration** - Easy mod sharing and installation

### Additional Future Features
- **Mobile Ports** - iOS/Android versions
- **Procedural Storytelling** - Dynamic narrative generation using AI
- **Permadeath Mode** - Roguelike variant with run-based progression
- **Daily Challenges** - Time-limited special dungeons with leaderboards
- **Cloud Saves** - Cross-device progression
- **Seasons/Battle Pass** - Rotating content and rewards
- **Game Master Mode** - One player controls the dungeon in real-time

---

## Version Summary

| Version | Focus | Key Additions |
|---------|-------|---------------|
| v0.0.1 | Walking Skeleton | Basic game loop, 5 rooms, combat |
| v0.0.2 | Core Polish | Load game, use items, examine, **cheat codes** |
| v0.0.3 | Player Creation | Data-driven races, attributes, **lexicon/terminology system**, **descriptor text variety engine** |
| v0.0.4 | Classes | Data-driven classes, custom resources (Mana/Rage/Energy), ability definitions |
| v0.0.5 | Dice Pools | d4-d10, modifiers, skill checks |
| v0.0.6 | Combat | Multi-monster, dice-based attacks, **status effects system** |
| v0.0.7 | Equipment | 7 slots, stat modifiers |
| v0.0.8 | Leveling | Configurable XP/Level terminology, data-driven progression curves |
| v0.0.9 | Monsters | Data-driven monsters, custom damage types, configurable currency |
| v0.1.0 | Z-Axis | Vertical movement, room types |
| v0.1.1 | Dynamic Rooms | Procedural generation, infinite mode |
| v0.1.2 | **Biomes** | **Biome definitions, themed content, transitions** |
| v0.1.3 | **Architecture** | **Room shapes, styles, landmarks, coherence, decay** |
| v0.1.4 | **Persistence** | **World state, repopulation, factions, evolution** |
| v0.1.5 | **Puzzles & Secrets** | **Procedural puzzles, secrets, traps, treasures** |
| v0.2.0 | NPCs & Dialogue | Conversations, persuasion, memory |
| v0.2.1 | **Codex** | **Discovery-based lore, bestiary, cross-references** |
| v0.2.2 | Merchants | Shops, haggling, economy |
| v0.3.0 | Quest Foundation | Core quest entity, objectives, journal |
| v0.3.1 | Quest Rewards & NPCs | Rewards, quest givers, prerequisites |
| v0.3.2 | Quest Chains | Chains, failure, main/side quests |
| v0.4.0 | Interactive Objects | Containers, doors, keys, locks, levers, buttons |
| v0.4.1 | Traps & Hazards | Trap detection/disarm, pressure plates, hazard zones |
| v0.4.2 | Puzzles & Riddles | Sequence, combination, pattern puzzles, riddle NPCs |
| v0.4.3 | Light & Skills | Light levels, dark vision, player skills foundation |
| v0.5.0 | Combat Grid Foundation | Core grid, movement points, ASCII display |
| v0.5.1 | Range & Melee | Range properties, melee adjacency, line of sight |
| v0.5.2 | Terrain & Cover | Terrain types, cover system, movement costs |
| v0.5.3 | Tactical Positioning | Flanking, opportunity attacks, area effects |
| v0.6.0 | TUI Polish | ASCII art, colors, grid view |
| v0.7.0 | GUI Foundation | Basic graphical interface |
| v0.8.0 | GUI Polish | Feature parity, puzzle UI |
| v0.9.0 | Audio | Music, sound effects |
| v0.10.0 | Advanced Combat | Ability trees, talent points, combos |
| v0.11.0 | Crafting | Resources, recipes, stations |
| v0.12.0 | Achievements | Stats, leaderboards |
| v1.0.0 | Release | Polish, tutorial, distribution |
