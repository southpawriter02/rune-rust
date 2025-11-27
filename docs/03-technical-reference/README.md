# Technical Reference

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

#### [Database Schema](./database-schema.md)
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

#### [Database Queries](./database-queries.md)
Common query patterns and examples.

**Contents:**
- CRUD operations per table
- Common joins
- Performance considerations
- Query optimization tips

---

### ⚙️ Service Architecture

#### [Service Overview](./service-architecture.md)
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

#### [Combat Service API](./combat-service-api.md)
Detailed API for combat-related services.

**Services Covered:**
- `CombatEngine` - Turn resolution, damage calculation
- `DamageService` - Damage calculation and mitigation
- `AccuracyService` - Hit/miss determination
- `StatusEffectService` - Status effect management
- `EnemyAI` - AI decision-making

---

#### [Progression Service API](./progression-service-api.md)
Detailed API for progression-related services.

**Services Covered:**
- `ProgressionService` - XP, leveling, PP
- `AttributeService` - Attribute increases
- `AbilityService` - Ability unlocks and usage
- `SpecializationService` - Specialization selection

---

#### [Procedural Generation API](./procedural-generation-api.md)
Detailed API for generation services.

**Services Covered:**
- `WaveFunctionCollapseEngine` - Room graph generation
- `SectorGenerator` - Sector creation pipeline
- `RoomPopulator` - Enemy and loot spawning
- `BiomeLibrary` - Biome definitions
- `QuestAnchorService` - Quest placement

---

#### [Persistence API](./persistence-api.md)
Detailed API for save/load services.

**Services Covered:**
- `SaveRepository` - Save/load operations
- `WorldStateRepository` - World state changes
- `StateSerializer` - Data serialization

---

### 🏗️ Code Architecture

#### [Project Structure](./project-structure.md)
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

#### [Class Hierarchies](./class-hierarchies.md)
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

#### [Design Patterns](./design-patterns.md)
Patterns used throughout the codebase.

**Patterns:**
- **Factory Pattern** - Enemy, Equipment, Ability creation
- **Service Pattern** - Business logic encapsulation
- **Repository Pattern** - Data access abstraction
- **Command Pattern** - Player action processing
- **Observer Pattern** - Event notifications
- **Strategy Pattern** - AI behaviors

---

#### [Data Flow](./data-flow.md)
How data moves through the system.

**Flow Diagrams:**
- Player Input → Command Processing → Game State Update → UI Render
- Combat Turn → Damage Calculation → State Update → Log Entry
- Room Generation → Population → State Application → Display

---

### 🔗 Integration Patterns

#### [System Integration Map](./system-integration-map.md)
How systems depend on each other.

**Integration Points:**
- Combat ↔ Progression (XP rewards)
- Combat ↔ Trauma Economy (Stress/Corruption)
- Combat ↔ Equipment (stat bonuses)
- Procedural Generation ↔ Quest System (anchor placement)
- World State ↔ Save/Load (persistence)

---

#### [Event System](./event-system.md)
Event-driven communication between systems.

**Events:**
- `OnEnemyDefeated` - Triggers XP gain, loot drops, quest progress
- `OnLevelUp` - Triggers PP gain, stat increases
- `OnBreakingPoint` - Triggers trauma application
- `OnRoomEnter` - Triggers autosave, state restoration

---

#### [Error Handling](./error-handling.md)
Exception handling patterns.

**Contents:**
- Exception types
- Error recovery strategies
- Logging patterns
- Validation approach

---

### 📊 Performance Documentation

#### [Performance Benchmarks](./performance-benchmarks.md)
Performance targets and measurements.

**Benchmarks:**
- Combat turn resolution: < 50ms
- Room generation: < 200ms
- World state application: < 30ms
- Save operation: < 100ms
- Load operation: < 200ms

---

#### [Optimization Strategies](./optimization-strategies.md)
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

**File:** `RuneAndRust.Engine/ServiceName.cs`
**Purpose:** [One-line description]

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
  - ✅ [Database Schema](./database-schema.md)
  - ✅ [Data Access Patterns](./data-access-patterns.md)
- Services: 8 / 10 documents complete
  - ✅ [CombatEngine](./services/combat-engine.md)
  - ✅ [DungeonGenerator](./services/dungeon-generator.md)
  - ✅ [CoherentGlitchRuleEngine](./services/coherent-glitch-rule-engine.md)
  - ✅ [PopulationPipeline](./services/population-pipeline.md)
  - ✅ [DiceService](./services/dice-service.md)
  - ✅ [TraumaEconomyService](./services/trauma-economy-service.md)
  - ✅ [EnemyAI](./services/enemy-ai.md)
  - ✅ [BiomeLibrary](./services/biome-library.md)
- Architecture: 4 / 4 documents complete
  - ✅ [Service Architecture](./service-architecture.md)
  - ✅ [Data Flow](./data-flow.md)
  - ✅ [System Integration Map](./system-integration-map.md)
  - ✅ [Class Hierarchies & Design Patterns](./class-hierarchies.md)
- Integration: 0 / 3 documents complete
- Performance: 0 / 2 documents complete

**Overall Progress:** 67% (14/21 documents)

---

**Last Updated:** 2025-11-27
**Documentation Version:** v0.20
