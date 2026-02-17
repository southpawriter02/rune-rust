# PopulationPipeline Service

Parent item: Service Architecture Overview (Service%20Architecture%20Overview%202ba55eb312da80a18965d6f5e87a15ec.md)

**File Path:** `RuneAndRust.Engine/PopulationPipeline.cs`**Version:** v0.39.3
**Last Updated:** 2025-11-27
**Status:** ✅ Implemented

---

## Overview

The `PopulationPipeline` coordinates all spawners to fill procedurally generated rooms with gameplay content: enemies, hazards, terrain, loot, and ambient conditions. It implements a data-driven population system with global budget management to prevent over-saturation.

**Pipeline Order:**

1. Conditions (first, to affect spawn weights via Coherent Glitch rules)
2. Hazards
3. Terrain
4. Enemies
5. Loot

---

## Architecture

### Component Spawners

```
┌──────────────────────────────────────────────────────────┐
│                   PopulationPipeline                     │
└───────────────────────────┬──────────────────────────────┘
                            │
    ┌───────────────────────┼───────────────────────┐
    │           │           │           │           │
    ▼           ▼           ▼           ▼           ▼
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│Condition│ │ Hazard │ │Terrain │ │ Enemy  │ │  Loot  │
│Applier │ │Spawner │ │Spawner │ │Spawner │ │Spawner │
└────────┘ └────────┘ └────────┘ └────────┘ └────────┘
    │           │           │           │           │
    ▼           ▼           ▼           ▼           ▼
┌──────────────────────────────────────────────────────────┐
│                    Room Population                       │
│  • AmbientConditions  • DynamicHazards  • StaticTerrain │
│  • Enemies            • LootNodes                        │
└──────────────────────────────────────────────────────────┘

```

### Dependencies

| Service | Version | Purpose |
| --- | --- | --- |
| `DormantProcessSpawner` | v0.11 | Enemy spawning |
| `HazardSpawner` | v0.11 | Dynamic hazard spawning |
| `TerrainSpawner` | v0.11 | Static terrain placement |
| `LootSpawner` | v0.11 | Loot node placement |
| `ConditionApplier` | v0.11 | Ambient condition application |
| `ContentDensityService` | v0.39.3 | Global budget calculation |
| `DensityClassificationService` | v0.39.3 | Room density classification |
| `BudgetDistributionService` | v0.39.3 | Budget allocation per room |
| `ThreatHeatmapService` | v0.39.3 | Threat distribution analysis |

---

## Public API

### `PopulateDungeon`

Populates all rooms in a dungeon with content.

```csharp
public void PopulateDungeon(
    Dungeon dungeon,
    BiomeDefinition biome,
    Random rng,
    DifficultyTier difficulty = DifficultyTier.Normal)

```

**Parameters:**

- `dungeon` - The dungeon to populate
- `biome` - Biome definition containing spawn tables
- `rng` - Seeded random number generator
- `difficulty` - Difficulty tier affecting budgets

**Pipeline Steps (v0.39.3):**

1. **Calculate Global Budget**
    - Uses `ContentDensityService` to determine total content budget
    - Based on room count, difficulty, and biome
2. **Classify Room Densities**
    - Uses `DensityClassificationService` to assign density tiers
    - Classifications: `Sparse`, `Light`, `Normal`, `Dense`, `Heavy`
3. **Distribute Budget**
    - Uses `BudgetDistributionService` to allocate budgets per room
    - Sets `room.AllocatedEnemyBudget`, `AllocatedHazardBudget`, `AllocatedLootBudget`
4. **Populate Each Room**
    - Skips handcrafted rooms (Quest Anchors)
    - Calls `PopulateRoom()` for each non-handcrafted room
5. **Generate Threat Heatmap**
    - Uses `ThreatHeatmapService` to visualize threat distribution
    - Logs statistics for balance analysis
6. **Validate Population**
    - Checks for empty rooms, overpopulation, Coherent Glitch violations

---

### `PopulateRoom`

Populates a single room with content.

```csharp
public void PopulateRoom(Room room, BiomeDefinition biome, Random rng)

```

**Parameters:**

- `room` - The room to populate
- `biome` - Biome definition with spawn tables
- `rng` - Random number generator

**Spawn Order:**

1. `ConditionApplier.ApplyConditions()` - Ambient conditions (darkness, flooded, etc.)
2. `HazardSpawner.PopulateRoom()` - Dynamic hazards (traps, environmental dangers)
3. `TerrainSpawner.PopulateRoom()` - Static terrain (cover, obstacles)
4. `DormantProcessSpawner.PopulateRoom()` - Enemies
5. `LootSpawner.PopulateRoom()` - Loot nodes

**Why This Order:**

- Conditions must apply first so Coherent Glitch rules can modify spawn weights
- Hazards before terrain allows terrain to react (rubble under unstable ceilings)
- Enemies after terrain for tactical positioning
- Loot last as it doesn't affect other spawns

---

### `GetStatistics`

Returns population statistics for a dungeon.

```csharp
public Dictionary<string, int> GetStatistics(Dungeon dungeon)

```

**Returns:**

| Key | Description |
| --- | --- |
| `TotalEnemies` | Sum of all enemies across rooms |
| `TotalHazards` | Sum of all dynamic hazards |
| `TotalTerrain` | Sum of all static terrain |
| `TotalLoot` | Sum of all loot nodes |
| `TotalConditions` | Sum of all ambient conditions |
| `RoomsWithEnemies` | Count of rooms containing enemies |
| `RoomsWithHazards` | Count of rooms with hazards |
| `RoomsWithLoot` | Count of rooms with loot |
| `EmptyRooms` | Count of completely empty rooms |

---

## Budget System (v0.39.3)

### Global Budget Calculation

```csharp
GlobalBudget = ContentDensityService.CalculateGlobalBudget(
    roomCount,
    difficulty,
    biomeId)

```

The global budget determines total content across the entire dungeon based on:

- Number of rooms
- Difficulty tier multiplier
- Biome-specific modifiers

### Density Classifications

Rooms are classified into density tiers:

| Classification | Enemy Budget | Hazard Budget | Loot Budget |
| --- | --- | --- | --- |
| `Sparse` | 0-1 | 0-1 | 0-1 |
| `Light` | 1-2 | 1-2 | 1-2 |
| `Normal` | 2-4 | 1-3 | 1-3 |
| `Dense` | 4-6 | 2-4 | 2-4 |
| `Heavy` | 6-8 | 3-5 | 3-5 |

### Budget Distribution

```csharp
// Applied to each room
room.DensityClassification = allocation.Density;
room.AllocatedEnemyBudget = allocation.AllocatedEnemies;
room.AllocatedHazardBudget = allocation.AllocatedHazards;
room.AllocatedLootBudget = allocation.AllocatedLoot;

```

Individual spawners respect these budgets when populating rooms.

---

## Validation

The pipeline validates population after completion:

### Checks Performed

| Check | Warning Condition |
| --- | --- |
| Empty rooms | Non-entry, non-secret rooms with no content |
| Boss room | Boss room has no enemies |
| Overpopulation | Rooms with >8 enemies |
| Coherent Glitch | Unstable ceiling without rubble pile |

### Example Validation Log

```
Population validation found 2 issues:
- Boss room has no enemies
- Room R_05 has Unstable Ceiling but no Rubble Pile (Coherent Glitch violation)

```

---

## Integration Points

### Called By

| Caller | Context |
| --- | --- |
| `DungeonGenerator.GenerateComplete()` | During dungeon generation |
| `DungeonGenerator.GenerateWithFullPipeline()` | Phase 6 of full pipeline |
| `DungeonGenerator.GenerateFromBlueprint()` | After anchor insertion |

### Calls Into

| Service | Methods Used |
| --- | --- |
| `ConditionApplier` | `ApplyConditions()` |
| `HazardSpawner` | `PopulateRoom()` |
| `TerrainSpawner` | `PopulateRoom()` |
| `DormantProcessSpawner` | `PopulateRoom()` |
| `LootSpawner` | `PopulateRoom()` |
| `ContentDensityService` | `CalculateGlobalBudget()` |
| `DensityClassificationService` | `ClassifyRooms()` |
| `BudgetDistributionService` | `DistributeBudget()` |
| `ThreatHeatmapService` | `GenerateHeatmap()`, `LogHeatmapStatistics()` |

---

## Data Flow

### Full Population Flow

```
DungeonGenerator.GenerateComplete()
              │
              ▼
┌─────────────────────────────────┐
│    PopulateDungeon(dungeon)     │
└───────────────┬─────────────────┘
                │
    ┌───────────┴───────────┐
    │                       │
    ▼                       ▼
┌───────────────┐   ┌───────────────┐
│ Calculate     │   │ Classify      │
│ Global Budget │   │ Room Density  │
└───────┬───────┘   └───────┬───────┘
        │                   │
        └─────────┬─────────┘
                  │
                  ▼
        ┌─────────────────┐
        │ Distribute      │
        │ Budget to Rooms │
        └────────┬────────┘
                 │
                 ▼
        ┌─────────────────┐
        │ For Each Room   │◄──────────┐
        └────────┬────────┘           │
                 │                    │
                 ▼                    │
        ┌─────────────────┐           │
        │ PopulateRoom()  │           │
        │                 │           │
        │ 1. Conditions   │           │
        │ 2. Hazards      │           │
        │ 3. Terrain      │           │
        │ 4. Enemies      │           │
        │ 5. Loot         │           │
        └────────┬────────┘           │
                 │                    │
                 └────────────────────┘
                          │
                          ▼
        ┌─────────────────────────────┐
        │ Generate Threat Heatmap     │
        └─────────────────────────────┘
                          │
                          ▼
        ┌─────────────────────────────┐
        │ Validate Population         │
        └─────────────────────────────┘

```

---

## Room Content Types

### Populated by Pipeline

| Content Type | Spawner | Storage Property |
| --- | --- | --- |
| Enemies | `DormantProcessSpawner` | `room.Enemies` |
| Dynamic Hazards | `HazardSpawner` | `room.DynamicHazards` |
| Static Terrain | `TerrainSpawner` | `room.StaticTerrain` |
| Loot Nodes | `LootSpawner` | `room.LootNodes` |
| Ambient Conditions | `ConditionApplier` | `room.AmbientConditions` |

### Example Populated Room

```
Room: R_05 "Collapsed Maintenance Bay"
├── Conditions: [Darkness, Flooded]
├── Hazards: [Live Power Conduit (enhanced), Steam Vent]
├── Terrain: [Rubble Pile, Broken Console]
├── Enemies: [2x Corrupted Drone, 1x Maintenance Bot]
└── Loot: [Supply Crate, Hidden Container]

```

---

## Handcrafted Room Handling

Handcrafted rooms (Quest Anchors) are skipped by the pipeline:

```csharp
if (room.IsHandcrafted)
{
    _log.Debug("Skipping handcrafted room {RoomId} (Quest Anchor)", room.RoomId);
    skippedHandcraftedCount++;
    continue;
}

```

Quest Anchors are pre-designed rooms placed by the `AnchorInserter` during dungeon generation. They contain hand-placed content that should not be overwritten.

---

## Version History

| Version | Changes |
| --- | --- |
| v0.11 | Initial implementation with 5 spawners |
| v0.39.3 | Added global budget system (ContentDensityService, etc.) |

---

## Cross-References

### Related Documentation

- [DungeonGenerator](https://www.notion.so/dungeon-generator.md) - Calls PopulationPipeline
- [CoherentGlitchRuleEngine](https://www.notion.so/coherent-glitch-rule-engine.md) - Modifies spawn weights

### Related Services

- [HazardSpawner](https://www.notion.so/hazard-spawner.md) - Hazard placement logic
- [LootSpawner](https://www.notion.so/loot-spawner.md) - Loot placement logic
- [BiomeLibrary](https://www.notion.so/biome-library.md) - Biome spawn tables

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-27