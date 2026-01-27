# Service Architecture Overview

**Last Updated:** 2025-11-27
**Documentation Version:** v0.39

---

## Executive Summary

Rune & Rust uses a **manual service composition** architecture with 163+ services organized into 12 feature domains. Services are explicitly wired through constructor injection without a DI container, enabling clear dependency tracking and initialization ordering.

**Key Characteristics:**
- No dependency injection container (manual instantiation)
- Explicit constructor dependencies
- Optional nullable dependencies for newer features
- Repository pattern for data access
- Partial classes for cross-cutting concerns

---

## Service Layer Organization

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              PRESENTATION                                   │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ Program.cs (ConsoleApp) │ Controllers (DesktopUI)                   │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└────────────────────────────────────┬────────────────────────────────────────┘
                                     │
┌────────────────────────────────────▼────────────────────────────────────────┐
│                           BUSINESS LOGIC (Engine)                           │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                        Service Categories                           │    │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐   │    │
│  │  │ Combat   │ │Generation│ │Progression│ │ Economy │ │ Territory│   │    │
│  │  │ Services │ │ Services │ │ Services │ │ Services│ │ Services │   │    │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘   │    │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐   │    │
│  │  │  Biome   │ │Companion │ │  Quest   │ │ Crafting │ │ Endgame  │   │    │
│  │  │ Services │ │ Services │ │ Services │ │ Services │ │ Services │   │    │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘   │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└────────────────────────────────────┬────────────────────────────────────────┘
                                     │
┌────────────────────────────────────▼────────────────────────────────────────┐
│                          DATA ACCESS (Persistence)                          │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ Repositories: Save │ Ability │ StatusEffect │ Descriptor │ World   │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└────────────────────────────────────┬────────────────────────────────────────┘
                                     │
┌────────────────────────────────────▼────────────────────────────────────────┐
│                               DATABASE                                      │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                        runeandrust.db (SQLite)                      │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Service Categories

### 1. Foundation Services (No Dependencies)

Core utilities that other services depend on:

| Service | Purpose | Consumers |
|---------|---------|-----------|
| `DiceService` | Random number generation, dice rolling | Almost all services |
| `GameState` | Game phase tracking, current room | Program.cs |
| `CommandParser` | Player input parsing | CommandDispatcher |
| `SagaService` | Character narrative/legend tracking | CombatEngine |

### 2. Combat Services

```
┌─────────────────────────────────────────────────────────────────────┐
│                         COMBAT SERVICES                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│                      ┌─────────────────┐                            │
│                      │  CombatEngine   │ (Orchestrator)             │
│                      └────────┬────────┘                            │
│           ┌──────────────────┬┴┬──────────────────┐                 │
│           │                  │ │                  │                 │
│           ▼                  ▼ ▼                  ▼                 │
│  ┌─────────────────┐ ┌─────────────┐ ┌─────────────────────┐        │
│  │    EnemyAI      │ │GridInit     │ │AdvancedStatusEffect │        │
│  │                 │ │Service      │ │Service              │        │
│  └────────┬────────┘ └─────────────┘ └─────────────────────┘        │
│           │                                                         │
│           ▼                                                         │
│  ┌─────────────────────────────────────────────────────────┐        │
│  │              Tactical Services                          │        │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐    │        │
│  │  │Flanking  │ │ Cover    │ │ Stance   │ │Counter   │    │        │
│  │  │Service   │ │ Service  │ │ Service  │ │Attack    │    │        │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘    │        │
│  └─────────────────────────────────────────────────────────┘        │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────┐        │
│  │              Flavor Text Services                       │        │
│  │  ┌───────────────┐ ┌───────────────┐                    │        │
│  │  │CombatFlavor   │ │GaldrFlavor    │                    │        │
│  │  │TextService    │ │TextService    │                    │        │
│  │  └───────────────┘ └───────────────┘                    │        │
│  └─────────────────────────────────────────────────────────┘        │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

**Key Services:**
- `CombatEngine` - Turn-based combat orchestration
- `EnemyAI` - Enemy decision-making
- `AdvancedStatusEffectService` - Status effect management
- `FlankingService`, `CoverService`, `StanceService` - Tactical mechanics

### 3. Generation Services

```
┌─────────────────────────────────────────────────────────────────────┐
│                       GENERATION SERVICES                           │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│                    ┌───────────────────┐                            │
│                    │ DungeonGenerator  │ (Orchestrator)             │
│                    └─────────┬─────────┘                            │
│         ┌────────────────────┼────────────────────┐                 │
│         │                    │                    │                 │
│         ▼                    ▼                    ▼                 │
│  ┌─────────────┐    ┌───────────────┐    ┌───────────────┐          │
│  │ Template    │    │ Population    │    │ Spatial       │          │
│  │ Library     │    │ Pipeline      │    │ Services      │          │
│  └─────────────┘    └───────┬───────┘    └───────────────┘          │
│                             │                                       │
│         ┌───────────────────┼───────────────────┐                   │
│         │         │         │         │         │                   │
│         ▼         ▼         ▼         ▼         ▼                   │
│  ┌──────────┐┌──────────┐┌──────────┐┌──────────┐┌──────────┐       │
│  │Condition ││ Hazard   ││ Terrain  ││ Enemy    ││  Loot    │       │
│  │Applier   ││ Spawner  ││ Spawner  ││ Spawner  ││ Spawner  │       │
│  └──────────┘└──────────┘└──────────┘└──────────┘└──────────┘       │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────┐        │
│  │           Coherent Glitch Rule Engine                   │        │
│  │  ┌─────────────────────────────────────────────────┐    │        │
│  │  │ 17 Contextual Rules for Environmental Coherence │    │        │
│  │  └─────────────────────────────────────────────────┘    │        │
│  └─────────────────────────────────────────────────────────┘        │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 4. Progression Services

| Service | Purpose | Dependencies |
|---------|---------|--------------|
| `CharacterFactory` | Character creation | AccountProgressionService |
| `SpecializationFactory` | Ability specializations | AbilityRepository |
| `AbilityService` | Ability management | AbilityRepository |
| `TraumaEconomyService` | Stress/corruption | DiceService |
| `AccountProgressionService` | Meta-progression | Database |

### 5. Economy Services

```
┌─────────────────────────────────────────────────────────────────────┐
│                        ECONOMY SERVICES                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│         ┌───────────────┐                                           │
│         │CurrencyService│ (Foundation)                              │
│         └───────┬───────┘                                           │
│                 │                                                   │
│     ┌───────────┼───────────┬───────────────┐                       │
│     │           │           │               │                       │
│     ▼           ▼           ▼               ▼                       │
│  ┌──────────┐┌──────────┐┌──────────┐┌───────────────┐              │
│  │Pricing   ││Transaction││  Loot   ││   Merchant    │              │
│  │Service   ││Service   ││ Service ││   Service     │              │
│  └──────────┘└──────────┘└──────────┘└───────────────┘              │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 6. Territory & Faction Services

```
┌─────────────────────────────────────────────────────────────────────┐
│                    TERRITORY & FACTION SERVICES                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│                   ┌─────────────────┐                               │
│                   │TerritoryService │ (Orchestrator + Cache)        │
│                   └────────┬────────┘                               │
│         ┌──────────────────┼──────────────────┐                     │
│         │                  │                  │                     │
│         ▼                  ▼                  ▼                     │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐                │
│  │Territory    │   │ FactionWar  │   │ WorldEvent  │                │
│  │Control      │   │ Service     │   │ Service     │                │
│  └─────────────┘   └─────────────┘   └─────────────┘                │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────┐        │
│  │              Integration Services                       │        │
│  │  ┌───────────────────┐ ┌───────────────────────────┐    │        │
│  │  │FactionTerritory   │ │CompanionTerritory         │    │        │
│  │  │Integration        │ │Reactions                  │    │        │
│  │  └───────────────────┘ └───────────────────────────┘    │        │
│  └─────────────────────────────────────────────────────────┘        │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 7. Biome Services

Each biome has dedicated environmental services:

| Biome | Service | Key Mechanics |
|-------|---------|---------------|
| Muspelheim | `MuspelheimBiomeService` | Heat damage, brittleness |
| Niflheim | `NiflheimBiomeService` | Frozen terrain, slipping |
| Alfheim | `AlfheimBiomeService` | Reality warping, glitches |
| Jotunheim | `JotunheimBiomeService` | Corruption terrain |

**Shared Infrastructure:**
- `BiomeTransitionService` - Smooth biome transitions
- `BiomeBlendingService` - Environmental blending
- `EnvironmentalGradientService` - Gradient effects

---

## Dependency Graph

### Core Dependency Chains

```
                           ┌─────────────┐
                           │ DiceService │
                           └──────┬──────┘
                                  │
          ┌───────────────────────┼───────────────────────┐
          │                       │                       │
          ▼                       ▼                       ▼
   ┌─────────────┐        ┌─────────────┐        ┌─────────────┐
   │HazardService│        │StatusEffect │        │ TraumaEcon  │
   └──────┬──────┘        │  Service    │        │  Service    │
          │               └──────┬──────┘        └──────┬──────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                                 ▼
                        ┌─────────────────┐
                        │  CombatEngine   │
                        └────────┬────────┘
                                 │
              ┌──────────────────┼──────────────────┐
              │                  │                  │
              ▼                  ▼                  ▼
       ┌──────────┐       ┌──────────┐       ┌──────────┐
       │ EnemyAI  │       │ Companion│       │ Territory│
       │          │       │ Service  │       │ Service  │
       └──────────┘       └──────────┘       └──────────┘
```

### Generation Chain

```
┌─────────────────┐
│TemplateLibrary  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐     ┌─────────────────┐
│DungeonGenerator │────►│ SpatialLayout   │
└────────┬────────┘     │ Service         │
         │              └─────────────────┘
         ▼
┌─────────────────┐     ┌─────────────────┐
│ PopulationPipeline│──►│ BiomeTransition │
└────────┬────────┘     │ Service         │
         │              └─────────────────┘
         ▼
┌─────────────────┐
│ CoherentGlitch  │
│ RuleEngine      │
└─────────────────┘
```

---

## Instantiation Patterns

### Pattern 1: Static Manual Instantiation

Services are instantiated as static fields in `Program.cs`:

```csharp
// Phase 1: No dependencies
private static DiceService _diceService = new();
private static SagaService _sagaService = new();

// Phase 2: Single dependencies
private static HazardService _hazardService = new(_diceService, _traumaService);

// Phase 3: Multi-dependency aggregators
private static CombatEngine _combatEngine = new(
    _diceService, _sagaService, _lootService, _equipmentService,
    _hazardService, _currencyService, _statusEffectService, ...);
```

### Pattern 2: Optional Dependencies

Newer features use nullable parameters:

```csharp
public CombatEngine(
    DiceService diceService,                    // Required
    SagaService sagaService,                    // Required
    // ...
    TerritoryService? territoryService = null,  // Optional (v0.35)
    CombatFlavorTextService? flavorTextService = null) // Optional (v0.38)
```

### Pattern 3: Late Binding

Some services support post-construction setup:

```csharp
public void SetCurrencyService(CurrencyService currencyService)
{
    QuestService = new QuestService("Data/Quests", currencyService);
}
```

### Pattern 4: Lazy Initialization

Territory services are created on first use:

```csharp
private static void InitializeTerritorySystem()
{
    _territoryControlService = new TerritoryControlService(_connectionString);
    _factionWarService = new FactionWarService(_connectionString, _territoryControlService);
    // ...
}
```

### Pattern 5: Internal Composition

Services create their own sub-services:

```csharp
public CombatEngine(...)
{
    // Created internally, not injected
    _performanceService = new PerformanceService();
    _gridService = new GridInitializationService();
    _flankingService = new FlankingService();
}
```

---

## Initialization Order

### Application Startup Sequence

```
1. BOOTSTRAP (Static Initialization)
   │
   ├── Foundation Services (DiceService, GameState)
   ├── Single-Dependency Services (HazardService)
   └── Multi-Dependency Aggregators (CombatEngine)

2. MAIN() INITIALIZATION
   │
   ├── Serilog logging setup
   ├── CommandDispatcher creation
   └── Main game loop startup

3. PER-GAME INITIALIZATION (InitializeV08Systems)
   │
   ├── NPC/Dialogue/Quest database loading
   ├── NPC placement in rooms
   └── Merchant inventory initialization

4. LAZY INITIALIZATION (InitializeTerritorySystem)
   │
   ├── TerritoryControlService
   ├── FactionWarService
   ├── WorldEventService
   └── TerritoryService (orchestrator)

5. GAME LOOP
   │
   ├── GameWorld management
   ├── Room navigation
   ├── Combat triggers
   └── Quest tracking
```

---

## Service Statistics

| Category | Count | Key Services |
|----------|-------|--------------|
| Combat | 15+ | CombatEngine, EnemyAI, FlankingService |
| Generation | 10+ | DungeonGenerator, PopulationPipeline |
| Progression | 8+ | CharacterFactory, AbilityService |
| Economy | 6+ | CurrencyService, MerchantService |
| Territory | 6+ | TerritoryService, FactionWarService |
| Biomes | 8+ | Per-biome services + shared infra |
| Companions | 4 | CompanionService, CompanionAIService |
| Crafting | 3 | AdvancedCraftingService, RecipeService |
| Flavor Text | 9 | Domain-specific text services |
| **Total** | **163+** | |

---

## Cross-References

- [Data Flow Documentation](./data-flow.md) - How data moves through services
- [System Integration Map](./system-integration-map.md) - Cross-system interactions
- [Service API Documentation](./services/) - Individual service documentation

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-27
