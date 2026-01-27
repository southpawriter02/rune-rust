# Rune & Rust - Codebase Catalog

**Version:** v0.17 Documentation
**Last Updated:** 2025-11-12
**Purpose:** Complete inventory of all source files for documentation reference

---

## Project Overview

```
RuneAndRust.sln
├── RuneAndRust.Core/        # 57 files - Data models (POCOs)
├── RuneAndRust.Engine/      # 75 files - Game logic & services
├── RuneAndRust.Persistence/ # ? files - Database & save/load
├── RuneAndRust.ConsoleApp/  # ? files - UI & main loop
└── RuneAndRust.Tests/       # ? files - Unit tests

```

**Total C# Files:** 132+ across Core and Engine

---

## RuneAndRust.Core/ (57 files)

**Core Purpose:** Data models and entities (Plain Old CLR Objects - no logic)

### Character & Combat (7 files)

- `PlayerCharacter.cs` - Player character model
- `Enemy.cs` - Enemy entity model
- `NPC.cs` - Non-player character model
- `Merchant.cs` - Merchant NPC model
- `CombatState.cs` - Combat session state
- `Ability.cs` - Ability definition model
- `Stance.cs` - Combat stance model

### Attributes & Progression (2 files)

- `Attributes.cs` - 5 core attributes (MIGHT, FINESSE, WITS, WILL, STURDINESS)
- `Specialization.cs` - Specialization model
- `CharacterClass.cs` - Character class definitions
- `Archetype.cs` - Archetype base class
    - `Archetypes/WarriorArchetype.cs` - Warrior archetype

### Equipment & Items (5 files)

- `Equipment.cs` - Equipment item model
- `Consumable.cs` - Consumable item model
- `CraftingComponent.cs` - Crafting material model
- `CraftingRecipe.cs` - Crafting recipe model
- `LootNode.cs` - Loot spawn point model

### World & Rooms (7 files)

- `Room.cs` - Room entity
- `RoomTemplate.cs` - Room template definition
- `HandcraftedRoom.cs` - Pre-designed room data
- `QuestAnchor.cs` - Quest objective placement point
- `Dungeon.cs` - Dungeon/Sector entity
- `Direction.cs` - Cardinal directions enum

### Dungeon Graph (4 files)

- `DungeonGraph.cs` - Graph structure for sector
- `DungeonNode.cs` - Node in dungeon graph
- `DungeonEdge.cs` - Edge between nodes
- `WorldState.cs` - Overall world state

### Environmental Elements (5 files)

- `DynamicHazard.cs` - Active hazard model
- `HazardType.cs` - Hazard type enum
- `DestructibleElement.cs` - Destructible terrain model
- `StaticTerrain.cs` - Static terrain model
- `AmbientCondition.cs` - Room ambient conditions

### Population System (5 files in Population/)

- `Population/RoomArchetype.cs` - Room type definitions
- `Population/LootNode.cs` - Loot placement
- `Population/DormantProcess.cs` - Background processes
- `Population/DynamicHazard.cs` - Hazard definitions
- `Population/StaticTerrain.cs` - Terrain definitions
- `Population/AmbientCondition.cs` - Ambient effects

### Quest System (5 files in Quests/)

- `Quests/Quest.cs` - Quest entity
- `Quests/QuestObjective.cs` - Objective model
- `Quests/QuestReward.cs` - Reward model
- `Quests/QuestGenerationRequirements.cs` - Quest gen constraints
- `Quests/ObjectiveTypes.cs` - Objective type enum

### Dialogue System (4 files in Dialogue/)

- `Dialogue/DialogueNode.cs` - Dialogue tree node
- `Dialogue/DialogueOption.cs` - Player choice option
- `Dialogue/DialogueOutcome.cs` - Outcome of choice
- `Dialogue/SkillCheckRequirement.cs` - Skill check definition

### Trauma Economy (7 files)

- `Trauma.cs` - Trauma entity model
- `TraumaCategory.cs` - Trauma category enum
- `TraumaEffect.cs` - Trauma effect model
- `TraumaSeverity.cs` - Severity enum
- `TraumaThresholds.cs` - Breaking point thresholds
- `PsychicResonanceLevel.cs` - Environmental psychic stress

### World State & Changes (2 files)

- `WorldState.cs` - Current world state
- `WorldStateChange.cs` - Delta record for changes

### Faction System (2 files)

- `FactionReputationSystem.cs` - Faction reputation tracking
- `FactionType.cs` - Faction enum

### Biomes (1 file)

- `BiomeDefinition.cs` - Biome configuration

---

## RuneAndRust.Engine/ (75 files)

**Engine Purpose:** Game logic, services, algorithms

### Core Combat (2 files)

- `CombatEngine.cs` - Combat orchestration, turn resolution
- `EnemyAI.cs` - Enemy AI decision-making

### Character Management (2 files)

- `CharacterFactory.cs` - Character creation
- `SpecializationFactory.cs` - Specialization instantiation

### Equipment & Loot (4 files)

- `EquipmentDatabase.cs` - Equipment definitions (60+ items)
- `EquipmentService.cs` - Equip/unequip logic
- `LootService.cs` - Loot generation
- `LootSpawner.cs` - Loot placement in rooms

### Consumables & Crafting (3 files)

- `ConsumableDatabase.cs` - Consumable definitions (10+ items)
- `CraftingService.cs` - Crafting system logic

### Abilities (1 file)

- `AbilityDatabase.cs` - Ability definitions (45+ abilities)

### Enemies (1 file)

- `EnemyFactory.cs` - Enemy creation (20+ enemy types)

### Hazards (3 files)

- `HazardDatabase.cs` - Hazard definitions (8+ types)
- `HazardService.cs` - Hazard logic
- `HazardSpawner.cs` - Hazard placement

### Procedural Generation (3 files)

- `DungeonGenerator.cs` - Wave Function Collapse + graph generation
- `DungeonService.cs` - Dungeon management
- `SeedManager.cs` - Seed management for generation

### Room Population (8 files)

- `PopulationPipeline.cs` - Main population orchestrator
- `BiomeLibrary.cs` - Biome definitions and weights
- `BiomeElementCache.cs` - Cached biome elements
- `ConditionApplier.cs` - Apply ambient conditions
- `TerrainSpawner.cs` - Terrain placement
- `DormantProcessSpawner.cs` - Background process spawning
- `DirectionAssigner.cs` - Assign room directions

### Coherent Glitch System (18 files)

**Location:** `CoherentGlitch/`

**Core Engine:**

- `CoherentGlitchRule.cs` - Base class for rules
- `CoherentGlitchRuleEngine.cs` - Rule evaluation engine
- `PopulationContext.cs` - Context for rule evaluation

**17 Contextual Rules:** (in `CoherentGlitch/Rules/`)

- `BossArenaAmplifierRule.cs` - Boss room difficulty
- `BrokenMaintenanceCycleRule.cs` - Maintenance narrative
- `ChasmInfrastructureRule.cs` - Chasm hazards
- `DarknessStressAmplifierRule.cs` - Dark room stress
- `EntryHallSafetyRule.cs` - Entry room safety
- `FailedEvacuationNarrativeRule.cs` - Evacuation story
- `FloodedElectricalDangerRule.cs` - Water + electricity
- `GeothermalSteamRule.cs` - Geothermal hazards
- `HiddenContainerDiscoveryRule.cs` - Secret loot
- `LongCorridorAmbushRule.cs` - Corridor encounters
- `MaintenanceHubOrganizationRule.cs` - Hub organization
- `NoSteamInFloodedRule.cs` - Prevent steam in flooded
- `PowerStationElectricalRule.cs` - Power station hazards
- `ResourceVeinClusterRule.cs` - Resource clustering
- `SecretRoomRewardRule.cs` - Secret room rewards
- `TacticalCoverPlacementRule.cs` - Combat cover
- `UnstableCeilingRubbleRule.cs` - Ceiling hazards

### Room Templates (3 files)

- `TemplateLibrary.cs` - Template management
- `HandcraftedRoomLibrary.cs` - Handcrafted room storage (30+ rooms)
- `RoomInstantiator.cs` - Room instantiation from templates
- `AnchorInserter.cs` - Quest anchor placement

### Quest System (2 files)

- `QuestService.cs` - Quest management
- `DynamicQuestGenerator.cs` - Procedural quest generation

### Dialogue System (1 file)

- `DialogueService.cs` - Dialogue tree evaluation

### Trauma Economy (5 files)

- `TraumaEconomyService.cs` - Stress/Corruption tracking
- `TraumaManagementService.cs` - Trauma application
- `TraumaProgressionService.cs` - Trauma scaling
- `TraumaLibrary.cs` - Trauma definitions
- `EnvironmentalStressService.cs` - Environmental Stress

### World State & Destruction (1 file)

- `DestructionService.cs` - Destructible environment logic

### Merchants & Economy (5 files)

- `MerchantService.cs` - Merchant logic
- `PricingService.cs` - Dynamic pricing
- `TransactionService.cs` - Buy/sell transactions
- `CurrencyService.cs` - Currency management

### NPCs (1 file)

- `NPCService.cs` - NPC interaction

### Sagas & Progression (1 file)

- `SagaService.cs` - Long-term progression tracking

### Utility Services (7 files)

- `DiceService.cs` - Random number generation (d6 dice pools)
- `DiceResult.cs` - Dice roll result model
- `CommandParser.cs` - Parse player commands
- `ResolveCheckService.cs` - Attribute checks (WITS, etc.)
- `PerformanceService.cs` - Performance tracking
- `MigrationService.cs` - Data migration
- `RealityGlitchService.cs` - Reality distortion effects

### Game State (2 files)

- `GameState.cs` - Current game state
- `GameWorld.cs` - World management

### Telemetry (2 files in Telemetry/)

- `Telemetry/GenerationTelemetry.cs` - Generation metrics
- `Telemetry/SectorBalanceMetrics.cs` - Balance metrics

---

## System Integration Map

### Combat Dependencies

```
CombatEngine
├── DiceService (random)
├── SagaService (XP, progression)
├── LootService (drops)
├── EquipmentService (stat bonuses)
├── HazardService (environmental damage)
├── CurrencyService (currency drops)
└── EnemyAI (enemy decisions)

```

### Procedural Generation Pipeline

```
DungeonGenerator (WFC + Graph)
└── PopulationPipeline
    ├── BiomeLibrary (biome weights)
    ├── TerrainSpawner (terrain placement)
    ├── HazardSpawner (hazard placement)
    ├── LootSpawner (loot placement)
    ├── DormantProcessSpawner (background processes)
    ├── ConditionApplier (ambient effects)
    ├── CoherentGlitchRuleEngine (contextual rules)
    │   └── 17 Rules (contextual population)
    └── AnchorInserter (quest anchors)

```

### Quest System Dependencies

```
QuestService
├── DynamicQuestGenerator (procedural quests)
├── QuestAnchors (in rooms)
└── DialogueService (quest dialogue)

```

### Trauma Economy Dependencies

```
TraumaEconomyService (Stress/Corruption)
├── TraumaManagementService (apply traumas)
├── TraumaProgressionService (scaling)
├── TraumaLibrary (trauma definitions)
└── EnvironmentalStressService (ambient stress)

```

---

## Key Algorithms & Patterns

### Wave Function Collapse

**File:** `DungeonGenerator.cs`**Purpose:** Generate dungeon graph with valid adjacencies

### Dice Pool System

**File:** `DiceService.cs`**Mechanic:** Roll Xd6, count 5-6 as successes

### Initiative System

**File:** `CombatEngine.cs`**Mechanic:** Roll FINESSE attribute, sort by successes

### Coherent Glitch Rules

**Files:** `CoherentGlitch/` (18 files)
**Purpose:** Contextual room population based on room type/archetype

### Delta-Based World State

**Files:** `WorldStateChange.cs`, `DestructionService.cs`**Purpose:** Record player modifications, apply on load

---

## File Naming Conventions

### Services

- `[System]Service.cs` - Business logic
- `[System]Factory.cs` - Object creation
- `[System]Database.cs` - Data definitions

### Models (Core)

- `[Entity].cs` - Single entity
- `[Entity]Type.cs` - Enum of types

### Systems

- `[System]Engine.cs` - Complex orchestrator
- `[System]Generator.cs` - Procedural generation
- `[System]Spawner.cs` - Placement logic

---

## Next Steps for Documentation

### High Priority (Phase 2-3)

1. Document CombatEngine.cs (combat resolution)
2. Document DiceService.cs (dice mechanics)
3. Document DungeonGenerator.cs (WFC algorithm)
4. Document TraumaEconomyService.cs (Stress/Corruption)

### Medium Priority (Phase 4-5)

1. Document AbilityDatabase.cs (abilities registry)
2. Document EquipmentDatabase.cs (equipment registry)
3. Document EnemyFactory.cs (enemy registry)
4. Document BiomeLibrary.cs (biome definitions)

### Lower Priority (Phase 6-9)

1. Document all CoherentGlitch rules
2. Document QuestService.cs + DynamicQuestGenerator.cs
3. Document DialogueService.cs
4. Document MerchantService.cs + economy services

---

**Documentation Progress:** 0% of files documented
**Estimated Total Documentation:** 50+ system docs, 100+ registry entries
**Target Completion:** 35-50 hours

---

**Last Updated:** 2025-11-12
**Status:** Catalog Complete, Ready for Documentation