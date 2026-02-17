# Technical Reference

Sub-item: Statistical Registry (Statistical%20Registry%202ba55eb312da80349cecf91d352f7b1a.md), Class Hierarchies & Design Patterns (Class%20Hierarchies%20&%20Design%20Patterns%202ba55eb312da8021bc61f832b969c293.md), Controller Integration Architecture (Controller%20Integration%20Architecture%202ba55eb312da80949191eaa48748418a.md), Data Access Patterns (Data%20Access%20Patterns%202ba55eb312da80bfbd02f0db51dbc522.md), Data Flow Documentation (Data%20Flow%20Documentation%202ba55eb312da80f8aa52d16a8fe2a34a.md), Database Schema Reference (Database%20Schema%20Reference%202ba55eb312da80d0ad8afa4584b27fbc.md), Error Handling Patterns (Error%20Handling%20Patterns%202ba55eb312da80289745c4e564c207ec.md), Event System Architecture (Event%20System%20Architecture%202ba55eb312da803088e8ebdbbfe23aee.md), Optimization Strategies (Optimization%20Strategies%202ba55eb312da80b4b3c7fbd81e69a94e.md), Performance Benchmarks (Performance%20Benchmarks%202ba55eb312da80ed9acbc3f85f0c758c.md), Service Architecture Overview (Service%20Architecture%20Overview%202ba55eb312da80a18965d6f5e87a15ec.md), System Integration Map (System%20Integration%20Map%202ba55eb312da803098a3ee1872ce3926.md)

This directory contains **developer-focused technical documentation** for the Rune & Rust codebase. Use this as your primary reference for understanding architecture, APIs, and integration patterns.

---

## Purpose

The Technical Reference provides:

- **Database schema** - Complete ERD and table definitions
- **Service architecture** - All services, their APIs, and dependencies
- **Code organization** - Class hierarchies and design patterns
- **Integration patterns** - How systems communicate and interact

---

## Documentation Index

### 🗄️ Database Documentation

### [Database Schema](https://www.notion.so/database-schema.md)

Complete database structure with ERD.

**Contents:**

- Entity-Relationship Diagram (ERD)
- Table definitions with column descriptions
- Primary keys, foreign keys, constraints
- Indexes and their purposes
- SQL DDL exports

**Tables:**

- Player/Character data
- Progression (Legend, PP, Attributes)
- Equipment & Inventory
- World State Changes
- Save/Load data
- Quest & Dialogue state

---

### [Database Queries](https://www.notion.so/database-queries.md)

Common query patterns and examples.

**Contents:**

- CRUD operations per table
- Common joins
- Performance considerations
- Query optimization tips

---

### ⚙️ Service Architecture

### [Service Overview](https://www.notion.so/service-architecture.md)

High-level service layer documentation.

**Contents:**

- All services listed with purposes
- Service dependency graph
- Initialization order
- Lifetime management (singleton vs transient)

**Core Services:**

- `CombatEngine` - Combat resolution
- `ProgressionService` - XP, leveling, PP
- `EquipmentService` - Equipment management
- `TraumaService` - Stress, Corruption, Breaking Points
- `DestructionService` - World state changes
- `QuestService` - Quest generation and tracking
- `DialogueService` - Dialogue trees
- `DiceService` - Random number generation
- `SaveRepository` - Persistence

---

### [Combat Service API](https://www.notion.so/combat-service-api.md)

Detailed API for combat-related services.

**Services Covered:**

- `CombatEngine` - Turn resolution, damage calculation
- `DamageService` - Damage calculation and mitigation
- `AccuracyService` - Hit/miss determination
- `StatusEffectService` - Status effect management
- `EnemyAI` - AI decision-making

---

### [Progression Service API](https://www.notion.so/progression-service-api.md)

Detailed API for progression-related services.

**Services Covered:**

- `ProgressionService` - XP, leveling, PP
- `AttributeService` - Attribute increases
- `AbilityService` - Ability unlocks and usage
- `SpecializationService` - Specialization selection

---

### [Procedural Generation API](https://www.notion.so/procedural-generation-api.md)

Detailed API for generation services.

**Services Covered:**

- `WaveFunctionCollapseEngine` - Room graph generation
- `SectorGenerator` - Sector creation pipeline
- `RoomPopulator` - Enemy and loot spawning
- `BiomeLibrary` - Biome definitions
- `QuestAnchorService` - Quest placement

---

### [Persistence API](https://www.notion.so/persistence-api.md)

Detailed API for save/load services.

**Services Covered:**

- `SaveRepository` - Save/load operations
- `WorldStateRepository` - World state changes
- `StateSerializer` - Data serialization

---

### 🏗️ Code Architecture

### [Project Structure](https://www.notion.so/project-structure.md)

Physical organization of the codebase.

**Contents:**

- Solution structure
- Project dependencies
- Folder organization per project
- Naming conventions

**Projects:**

```
RuneAndRust.sln
├── RuneAndRust.Core/        # POCOs, data models
├── RuneAndRust.Engine/      # Game logic, services
├── RuneAndRust.Persistence/ # Database, save/load
├── RuneAndRust.ConsoleApp/  # UI, main loop
└── RuneAndRust.Tests/       # Unit tests

```

---

### [Class Hierarchies](https://www.notion.so/class-hierarchies.md)

Object-oriented design documentation.

**Contents:**

- Inheritance hierarchies
- Interface definitions
- Abstract base classes
- Design patterns used

**Key Hierarchies:**

- `Character` → `PlayerCharacter`, `Enemy`
- `Ability` → `ActiveAbility`, `PassiveAbility`
- `Equipment` → `Weapon`, `Armor`, `Accessory`
- `Quest` → `KillQuest`, `ExploreQuest`, `InteractQuest`

---

### [Design Patterns](https://www.notion.so/design-patterns.md)

Patterns used throughout the codebase.

**Patterns:**

- **Factory Pattern** - Enemy, Equipment, Ability creation
- **Service Pattern** - Business logic encapsulation
- **Repository Pattern** - Data access abstraction
- **Command Pattern** - Player action processing
- **Observer Pattern** - Event notifications
- **Strategy Pattern** - AI behaviors

---

### [Data Flow](https://www.notion.so/data-flow.md)

How data moves through the system.

**Flow Diagrams:**

- Player Input → Command Processing → Game State Update → UI Render
- Combat Turn → Damage Calculation → State Update → Log Entry
- Room Generation → Population → State Application → Display

---

### 🔗 Integration Patterns

### [Event System](https://www.notion.so/event-system.md)

Comprehensive event system documentation.

**Contents:**

- World Events - Territory events stored in database (WorldEventService)
- Environmental Events - Combat environmental interactions (EnvironmentalEvent)
- Controller Events - C# events for UI/state communication
- Event types, lifecycle, and resolution patterns
- Result types (DestructionResult, HazardResult, etc.)

---

### [Error Handling](https://www.notion.so/error-handling.md)

Exception handling patterns.

**Contents:**

- Constructor validation (ArgumentNullException guards)
- Service-level exception handling with structured logging
- Validation exceptions (InvalidOperationException, ArgumentException)
- Null-safe patterns and early returns
- Result types for expected failures
- File I/O error handling
- Logging level guidelines

---

### [Controller Integration](https://www.notion.so/controller-integration.md)

Controller-layer integration architecture.

**Contents:**

- GameStateController (master state management)
- Phase transition validation and state machine
- Inter-controller event communication
- Controller wiring and initialization patterns
- State validation and auto-save integration
- Combat/Loot/Progression/Death/Victory workflows

---

### 📊 Performance Documentation

### [Performance Benchmarks](https://www.notion.so/performance-benchmarks.md)

Performance targets and measurements.

**Benchmarks:**

- Combat turn resolution: < 50ms
- Room generation: < 200ms
- World state application: < 30ms
- Save operation: < 100ms
- Load operation: < 200ms

---

### [Optimization Strategies](https://www.notion.so/optimization-strategies.md)

Performance optimization approaches.

**Strategies:**

- Caching frequently-accessed data
- Lazy loading of room templates
- Batch database operations
- Object pooling for combat entities

---

## API Documentation Format

Each service API follows this format:

### Service Name

**File:** `RuneAndRust.Engine/ServiceName.cs`**Purpose:** [One-line description]

**Dependencies:**

- Service A: [Why needed]
- Service B: [Why needed]

**Public Methods:**

```csharp
public ReturnType MethodName(ParameterType param)
{
    // Purpose: [What this does]
    // Parameters:
    //   param: [Parameter description]
    // Returns: [Return value description]
    // Throws: [Exceptions thrown]
}

```

**Usage Example:**

```csharp
var service = new ServiceName(dependencies);
var result = service.MethodName(value);
// result is [description]

```

---

## Code Examples

All API documentation includes:

- **Minimal example** - Simplest possible usage
- **Real-world example** - From actual game scenarios
- **Edge case handling** - Error handling, validation

---

## Diagram Standards

### UML Diagrams

- Use PlantUML or Mermaid syntax
- Include class relationships
- Show key methods and properties

### Sequence Diagrams

- Show method call order
- Include actors (Player, System, Database)
- Annotate with timing if relevant

### Data Flow Diagrams

- Use boxes for processes
- Use arrows for data movement
- Annotate with data types

---

## Cross-References

Each technical doc links to:

- **System Documentation** - Higher-level explanation
- **Statistical Registry** - Data values used
- **Testing Documentation** - Test coverage
- **Source Code** - File paths and line numbers

---

## Maintenance

### When to Update

- **After refactoring** - Update architecture docs
- **After adding new service** - Document API
- **After schema changes** - Update database docs
- **After dependency changes** - Update integration map

### Update Process

1. Make code change
2. Update relevant technical doc
3. Update diagrams if structure changed
4. Verify cross-references still valid
5. Run documentation build (if automated)

---

## Progress Tracking

**Documentation Status:**

- Database: 2 / 2 documents complete
    - ✅ [Database Schema](https://www.notion.so/database-schema.md)
    - ✅ [Data Access Patterns](https://www.notion.so/data-access-patterns.md)
- Services: 10 / 10 documents complete
    - ✅ [CombatEngine](https://www.notion.so/services/combat-engine.md)
    - ✅ [DungeonGenerator](https://www.notion.so/services/dungeon-generator.md)
    - ✅ [CoherentGlitchRuleEngine](https://www.notion.so/services/coherent-glitch-rule-engine.md)
    - ✅ [PopulationPipeline](https://www.notion.so/services/population-pipeline.md)
    - ✅ [DiceService](https://www.notion.so/services/dice-service.md)
    - ✅ [TraumaEconomyService](https://www.notion.so/services/trauma-economy-service.md)
    - ✅ [EnemyAI](https://www.notion.so/services/enemy-ai.md)
    - ✅ [BiomeLibrary](https://www.notion.so/services/biome-library.md)
    - ✅ [QuestService](https://www.notion.so/services/quest-service.md)
    - ✅ [EquipmentService](https://www.notion.so/services/equipment-service.md)
- Architecture: 4 / 4 documents complete
    - ✅ [Service Architecture](https://www.notion.so/service-architecture.md)
    - ✅ [Data Flow](https://www.notion.so/data-flow.md)
    - ✅ [System Integration Map](https://www.notion.so/system-integration-map.md)
    - ✅ [Class Hierarchies & Design Patterns](https://www.notion.so/class-hierarchies.md)
- Integration: 3 / 3 documents complete
    - ✅ [Event System](https://www.notion.so/event-system.md)
    - ✅ [Error Handling](https://www.notion.so/error-handling.md)
    - ✅ [Controller Integration](https://www.notion.so/controller-integration.md)
- Performance: 2 / 2 documents complete
    - ✅ [Performance Benchmarks](https://www.notion.so/performance-benchmarks.md)
    - ✅ [Optimization Strategies](https://www.notion.so/optimization-strategies.md)

**Overall Progress:** 100% (21/21 documents)

---

**Last Updated:** 2025-11-27
**Documentation Version:** v0.20