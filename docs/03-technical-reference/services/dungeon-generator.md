# DungeonGenerator Service

**File Path:** `RuneAndRust.Engine/DungeonGenerator.cs`
**Version:** v0.39.4
**Last Updated:** 2025-11-27
**Status:** ✅ Implemented

---

## Overview

The `DungeonGenerator` is responsible for procedural dungeon generation using a graph-based algorithm. It creates interconnected room layouts with main paths, branching corridors, and secret areas. The service has evolved through multiple versions, adding support for 3D spatial layouts, biome transitions, and content density balancing.

---

## Architecture

### Generation Pipeline Evolution

```
v0.10: Graph-only generation
v0.11: + PopulationPipeline integration (enemies, hazards, loot)
v0.39.1: + 3D Spatial Layout (vertical connections, elevation)
v0.39.2: + Biome Transitions (multi-biome sectors)
v0.39.3: + Content Density (balanced population budgets)
v0.39.4: Full integrated 6-phase pipeline
```

### Dependencies

The DungeonGenerator integrates with up to 13 services (many optional):

| Service | Version | Purpose | Required |
|---------|---------|---------|----------|
| `TemplateLibrary` | v0.10 | Room template definitions | ✅ Yes |
| `PopulationPipeline` | v0.11 | Enemy/hazard/loot spawning | Optional |
| `AnchorInserter` | v0.11 | Quest anchor placement | Optional |
| `ISpatialLayoutService` | v0.39.1 | 3D coordinate assignment | Optional |
| `ISpatialValidationService` | v0.39.1 | Spatial layout validation | Optional |
| `IVerticalTraversalService` | v0.39.1 | Stairs/ladder connections | Optional |
| `BiomeTransitionService` | v0.39.2 | Multi-biome sector support | Optional |
| `BiomeBlendingService` | v0.39.2 | Biome boundary blending | Optional |
| `EnvironmentalGradientService` | v0.39.2 | Environmental effect gradients | Optional |
| `ContentDensityService` | v0.39.3 | Population budget calculation | Optional |
| `DensityClassificationService` | v0.39.3 | Room density classification | Optional |
| `BudgetDistributionService` | v0.39.3 | Content budget allocation | Optional |
| `ThreatHeatmapService` | v0.39.3 | Threat distribution visualization | Optional |

### Service Tiers

```
┌─────────────────────────────────────────────────────────────┐
│                    DungeonGenerator                         │
├─────────────────────────────────────────────────────────────┤
│  REQUIRED (v0.10)                                           │
│  └─ TemplateLibrary                                         │
├─────────────────────────────────────────────────────────────┤
│  POPULATION (v0.11)                                         │
│  ├─ PopulationPipeline                                      │
│  └─ AnchorInserter                                          │
├─────────────────────────────────────────────────────────────┤
│  SPATIAL (v0.39.1)                                          │
│  ├─ ISpatialLayoutService                                   │
│  ├─ ISpatialValidationService                               │
│  └─ IVerticalTraversalService                               │
├─────────────────────────────────────────────────────────────┤
│  BIOME (v0.39.2)                                            │
│  ├─ BiomeTransitionService                                  │
│  ├─ BiomeBlendingService                                    │
│  └─ EnvironmentalGradientService                            │
├─────────────────────────────────────────────────────────────┤
│  DENSITY (v0.39.3)                                          │
│  ├─ ContentDensityService                                   │
│  ├─ DensityClassificationService                            │
│  ├─ BudgetDistributionService                               │
│  └─ ThreatHeatmapService                                    │
└─────────────────────────────────────────────────────────────┘
```

---

## Public API

### Core Generation Methods

#### `Generate`

Generates a dungeon graph structure (nodes and edges) without instantiating rooms.

```csharp
public DungeonGraph Generate(
    int seed,
    int targetRoomCount = 7,
    BiomeDefinition? biome = null)
```

**Parameters:**
- `seed` - Random seed for deterministic generation
- `targetRoomCount` - Target number of rooms (overridden by biome if provided)
- `biome` - Optional biome definition with room count range and probabilities

**Returns:** `DungeonGraph` containing nodes and edges

**Generation Steps:**
1. Initialize RNG with seed
2. Generate main path (linear room sequence)
3. Add branching paths (probability from biome, default 60%)
4. Add secret rooms (probability from biome, default 30%)
5. Calculate node depths (distance from entrance)
6. Assign cardinal directions to edges via `DirectionAssigner`
7. Validate connectivity

**Biome Parameters Used:**
- `MinRoomCount`, `MaxRoomCount` - Room count range
- `BranchingProbability` - Chance to add branching paths (default 0.6)
- `SecretRoomProbability` - Chance to add secret rooms (default 0.3)

---

#### `GenerateComplete`

Generates a complete playable dungeon with instantiated rooms and optional population.

```csharp
public Dungeon GenerateComplete(
    int seed,
    int dungeonId = 1,
    int targetRoomCount = 7,
    BiomeDefinition? biome = null)
```

**Parameters:**
- `seed` - Random seed
- `dungeonId` - Unique dungeon identifier
- `targetRoomCount` - Target room count
- `biome` - Optional biome definition

**Returns:** Complete `Dungeon` with instantiated `Room` objects

**Pipeline Steps:**
1. Generate graph via `Generate()`
2. (v0.39.1) Convert to 3D spatial layout if `ISpatialLayoutService` available
3. (v0.39.1) Generate vertical connections (stairs, ladders)
4. (v0.39.1) Validate spatial layout if `ISpatialValidationService` available
5. Instantiate rooms via `RoomInstantiator`
6. (v0.11) Populate rooms via `PopulationPipeline` if available

---

#### `GenerateFromBlueprint`

Generates a dungeon from a predefined blueprint with quest anchor support.

```csharp
public Dungeon GenerateFromBlueprint(
    DungeonBlueprint blueprint,
    int dungeonId,
    BiomeDefinition biome)
```

**Parameters:**
- `blueprint` - Blueprint specifying seed, room count, and required quest anchors
- `dungeonId` - Unique dungeon identifier
- `biome` - Biome definition (required)

**Returns:** `Dungeon` with quest anchors placed

**Pipeline Steps:**
1. Validate blueprint
2. Generate base graph
3. Insert quest anchors via `AnchorInserter` (if available)
4. Instantiate rooms
5. Populate rooms (skipping handcrafted quest anchor rooms)

---

#### `GenerateWithFullPipeline`

Full v0.39.4 integrated generation using all 6 phases.

```csharp
public Dungeon GenerateWithFullPipeline(
    int seed,
    int dungeonId,
    int targetRoomCount = 7,
    BiomeDefinition? biome = null,
    List<BiomeDefinition>? additionalBiomes = null)
```

**Parameters:**
- `seed` - Random seed
- `dungeonId` - Unique dungeon identifier
- `targetRoomCount` - Target room count
- `biome` - Primary biome definition
- `additionalBiomes` - Additional biomes for multi-biome sectors

**Returns:** Fully populated `Dungeon` with all v0.39 features

**6-Phase Pipeline:**

| Phase | Name | Service(s) | Output |
|-------|------|------------|--------|
| 1 | Layout Generation | `Generate()` | `DungeonGraph` |
| 2 | 3D Spatial Layout | `ISpatialLayoutService` | 3D positions, vertical connections |
| 3 | Room Instantiation | `RoomInstantiator` | `Dungeon` with rooms |
| 4 | Biome Transitions | `BiomeTransitionService` | Biome assignments per room |
| 5 | Content Density | `ContentDensityService` | Population budgets |
| 6 | Population | `PopulationPipeline` | Enemies, hazards, loot |

---

## Key Models

### DungeonGraph

Represents the abstract structure of a dungeon.

```csharp
public class DungeonGraph
{
    // Nodes represent room locations
    public List<DungeonNode> Nodes { get; }

    // Edges represent connections between rooms
    public List<DungeonEdge> Edges { get; }

    // Statistics and validation
    public Dictionary<string, int> GetStatistics();
    public (bool IsValid, List<string> Errors) Validate();
}
```

### DungeonNode

Represents a room location in the graph.

```csharp
public class DungeonNode
{
    public int Id { get; set; }
    public RoomTemplate Template { get; set; }      // Room type/template
    public int Depth { get; set; }                  // Distance from entrance
    public bool IsMainPath { get; set; }            // On critical path?
    public bool IsBranch { get; set; }              // Side branch?
    public bool IsSecret { get; set; }              // Hidden room?
    public string? QuestAnchorType { get; set; }    // Quest anchor, if any
}
```

### Dungeon

Complete dungeon with instantiated rooms.

```csharp
public class Dungeon
{
    public int Id { get; set; }
    public int Seed { get; set; }
    public string Biome { get; set; }
    public List<Room> Rooms { get; }
    public Room Entrance { get; }                   // Starting room
    public Room? Exit { get; }                      // Boss/exit room
    public int TotalRoomCount { get; }
}
```

### DungeonBlueprint

Blueprint for controlled dungeon generation with quest anchors.

```csharp
public class DungeonBlueprint
{
    public int Seed { get; set; }
    public int TargetRoomCount { get; set; }
    public List<QuestAnchor> RequiredAnchors { get; }

    public (bool IsValid, List<string> Errors) Validate();
}
```

---

## Generation Algorithm

### Main Path Generation

The main path forms the critical route from entrance to exit.

```
Entrance ──► Room 1 ──► Room 2 ──► ... ──► Boss Room
```

1. Create entrance node (depth 0)
2. For each subsequent room:
   - Select template based on biome weights
   - Create node with incremented depth
   - Connect to previous node with bidirectional edge
3. Mark final node as boss/exit room

### Branching Paths

Branching paths create exploration opportunities off the main path.

```
                    ┌── Branch Room A
                    │
Entrance ──► Room 1 ┼──► Room 2 ──► Boss
                    │
                    └── Branch Room B ──► Branch Room C
```

1. Select random main-path node (not entrance or exit)
2. Generate 1-3 branch rooms
3. Connect first branch room to selected main-path node
4. Mark branch rooms with `IsBranch = true`

### Secret Rooms

Secret rooms are hidden areas with special rewards.

```
Room 2 ═══► [SECRET ROOM]
           (hidden entrance)
```

1. Select random non-entrance node
2. Create secret room with special template
3. Connect with "hidden" edge type
4. Mark with `IsSecret = true`

---

## Integration Points

### Called By

| Caller | Context |
|--------|---------|
| `DungeonService` | High-level dungeon management |
| `Program.cs` | Console app dungeon creation |
| `SectorGenerator` | Multi-sector world generation |

### Calls Into

| Service | Methods Used |
|---------|-------------|
| `TemplateLibrary` | `GetTemplate()`, `GetWeightedTemplate()` |
| `DirectionAssigner` | `AssignDirections()` |
| `RoomInstantiator` | `Instantiate()` |
| `PopulationPipeline` | `PopulateDungeon()` |
| `AnchorInserter` | `InsertAnchors()` |
| `ISpatialLayoutService` | `ConvertGraphTo3DLayout()`, `GenerateVerticalConnections()` |
| `BiomeTransitionService` | `AssignBiomesToRooms()` |
| `ContentDensityService` | `CalculateBudgets()` |

---

## Data Flow

### Complete Generation Flow

```
                    ┌─────────────────┐
                    │      Seed       │
                    └────────┬────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │  Initialize RNG │
                    └────────┬────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
    ┌─────────┐        ┌─────────┐        ┌─────────┐
    │  Main   │        │ Branch  │        │ Secret  │
    │  Path   │        │  Paths  │        │  Rooms  │
    └────┬────┘        └────┬────┘        └────┬────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │  DungeonGraph   │
                    └────────┬────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
    ┌─────────┐        ┌─────────┐        ┌─────────┐
    │ Spatial │        │  Room   │        │  Biome  │
    │ Layout  │        │ Instant │        │ Assign  │
    └────┬────┘        └────┬────┘        └────┬────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │    Dungeon      │
                    └────────┬────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │   Population    │──► Enemies, Hazards, Loot
                    │    Pipeline     │
                    └─────────────────┘
```

---

## Deterministic Generation

The generator uses seeded random number generation for reproducibility:

```csharp
// Same seed always produces identical dungeon
var dungeon1 = generator.GenerateComplete(seed: 12345, dungeonId: 1);
var dungeon2 = generator.GenerateComplete(seed: 12345, dungeonId: 2);
// dungeon1 and dungeon2 have identical layouts (different IDs)
```

This enables:
- Save/load by storing only the seed
- Multiplayer synchronization
- Bug reproduction
- Testing consistency

---

## Version History

| Version | Changes |
|---------|---------|
| v0.10 | Initial graph-based generation |
| v0.11 | PopulationPipeline integration, Quest Anchor support |
| v0.39.1 | 3D spatial layout, vertical connections |
| v0.39.2 | Multi-biome sectors, biome transitions |
| v0.39.3 | Content density balancing, threat heatmaps |
| v0.39.4 | Full integrated 6-phase pipeline |

---

## Cross-References

### Related Documentation

- [Procedural Generation](../../PROCEDURAL_GENERATION.md) - High-level generation overview
- [PopulationPipeline](./population-pipeline.md) - Room population system

### Related Services

- [TemplateLibrary](./template-library.md) - Room template definitions
- [PopulationPipeline](./population-pipeline.md) - Enemy/hazard spawning
- [BiomeLibrary](./biome-library.md) - Biome definitions
- [SpatialLayoutService](./spatial-layout-service.md) - 3D positioning

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-27
