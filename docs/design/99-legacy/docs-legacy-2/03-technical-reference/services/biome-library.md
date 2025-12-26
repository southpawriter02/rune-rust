# BiomeLibrary Service

**File Path:** `RuneAndRust.Engine/BiomeLibrary.cs`
**Version:** v0.10
**Last Updated:** 2025-11-27
**Status:** ✅ Implemented

---

## Overview

The `BiomeLibrary` manages biome definitions for procedural dungeon generation. Biomes act as "style guides" that control the aesthetic and mechanical properties of generated areas—determining room templates, enemy types, hazards, environmental descriptions, and generation parameters.

---

## Architecture

### Data-Driven Design

```
Data/Biomes/
├── the_roots.json       → BiomeDefinition
├── lower_depths.json    → BiomeDefinition
├── frozen_vault.json    → BiomeDefinition
└── ...

        ▼ LoadBiomes()

BiomeLibrary._biomes Dictionary
        │
        ├── "the_roots" → BiomeDefinition
        ├── "lower_depths" → BiomeDefinition
        └── "frozen_vault" → BiomeDefinition
```

### Class Relationships

```
BiomeLibrary (Service)
    │
    └── manages → BiomeDefinition (Model)
                      │
                      ├── AvailableTemplates: List<string>
                      ├── DescriptorCategories: Dictionary<string, List<string>>
                      ├── Generation Parameters
                      └── BiomeElementTable? (v0.11+)
                              │
                              └── List<BiomeElement>
                                      │
                                      └── SpawnRules?
```

---

## Public API

### `LoadBiomes`

Loads all biome definitions from JSON files.

```csharp
public void LoadBiomes()
```

**Behavior:**
1. Scans `Data/Biomes` directory for `.json` files
2. Deserializes each file into `BiomeDefinition`
3. Validates each biome via `IsValid()`
4. Adds valid biomes to internal dictionary

**Logging:**
- Counts loaded, invalid, and total files
- Warns on missing data path or validation failures

---

### `GetBiome`

Gets a biome by its unique ID.

```csharp
public BiomeDefinition? GetBiome(string biomeId)
```

**Parameters:**
- `biomeId` - Unique biome identifier (e.g., "the_roots")

**Returns:** `BiomeDefinition` or `null` if not found

---

### `GetDefaultBiome`

Gets the default biome ("the_roots").

```csharp
public BiomeDefinition GetDefaultBiome()
```

**Returns:**
- `the_roots` biome if loaded
- Fallback biome if not found

**Fallback Biome Properties:**
- Generic dungeon aesthetics
- MinRoomCount: 5, MaxRoomCount: 7
- BranchingProbability: 0.4, SecretRoomProbability: 0.2

---

### `GetAllBiomes`

Gets all loaded biomes.

```csharp
public List<BiomeDefinition> GetAllBiomes()
```

---

### `GetBiomeCount`

Gets the count of loaded biomes.

```csharp
public int GetBiomeCount()
```

---

### `ValidateLibrary`

Validates that the library has usable biomes.

```csharp
public bool ValidateLibrary()
```

**Returns:** `false` if no biomes loaded

---

## BiomeDefinition Model

### Core Properties

```csharp
public class BiomeDefinition
{
    // Identity
    public string BiomeId { get; set; }      // "the_roots"
    public string Name { get; set; }          // "The Roots"
    public string Description { get; set; }   // Lore description

    // Template Pool
    public List<string> AvailableTemplates { get; set; }

    // Aesthetic Properties
    public Dictionary<string, List<string>> DescriptorCategories { get; set; }

    // Generation Parameters
    public int MinRoomCount { get; set; }
    public int MaxRoomCount { get; set; }
    public float BranchingProbability { get; set; }
    public float SecretRoomProbability { get; set; }

    // v0.11+ Population
    public BiomeElementTable? Elements { get; set; }
}
```

### Descriptor Categories

Biomes define thematic text pools for procedural description generation:

| Category | Purpose | Example Values |
|----------|---------|----------------|
| `Adjectives` | Room modifiers | "Corroded", "Decaying", "Twisted" |
| `Details` | Visual details | "Runic glyphs flicker weakly" |
| `Sounds` | Ambient audio | "hissing steam", "groaning metal" |
| `Smells` | Olfactory cues | "ozone", "rust", "decay" |

### Generation Parameters

| Property | Default | Description |
|----------|---------|-------------|
| `MinRoomCount` | 5 | Minimum rooms in dungeon |
| `MaxRoomCount` | 7 | Maximum rooms in dungeon |
| `BranchingProbability` | 0.4 | Chance to add branching paths |
| `SecretRoomProbability` | 0.2 | Chance to add secret rooms |

### Validation Rules

A biome is valid if:
- `BiomeId` is not empty
- `Name` is not empty
- `AvailableTemplates` has at least one template
- `MinRoomCount` >= 3
- `MaxRoomCount` >= `MinRoomCount`

---

## BiomeElementTable (v0.11+)

The element table defines spawnable content for the population pipeline.

### Element Types

```csharp
public enum BiomeElementType
{
    // v0.10 (basic)
    RoomTemplate,
    DescriptionDetail,
    AmbientCondition,

    // v0.11 (population)
    DormantProcess,      // Enemy spawns
    DynamicHazard,       // Environmental dangers
    StaticTerrain,       // Cover, obstacles
    LootNode,            // Containers, veins

    // v0.12 (polish)
    CoherentGlitchRule   // Contextual rules
}
```

### BiomeElement Structure

```csharp
public class BiomeElement
{
    public string ElementName { get; set; }
    public BiomeElementType ElementType { get; set; }
    public float Weight { get; set; }           // Spawn probability weight
    public string? AssociatedDataId { get; set; } // Link to enemy/hazard ID
    public int SpawnCost { get; set; }          // Budget cost
    public SpawnRules? SpawnRules { get; set; } // Placement constraints
}
```

### SpawnRules

Constraints for element placement:

```csharp
public class SpawnRules
{
    // Size constraints
    public bool OnlyInLargeRooms { get; set; }
    public bool OnlyInSmallRooms { get; set; }

    // Archetype constraints
    public string? RequiredArchetype { get; set; }
    public bool NeverInEntryHall { get; set; }
    public bool NeverInBossArena { get; set; }
    public bool NeverInSecretRooms { get; set; }

    // Weight modifiers
    public bool HigherWeightInSecretRooms { get; set; }
    public float SecretRoomWeightMultiplier { get; set; }

    // Conditional spawning
    public string? RequiresEnemyType { get; set; }
    public string? RequiresHazardType { get; set; }
    public string? RequiresCondition { get; set; }
}
```

---

## Example Biome JSON

```json
{
  "BiomeId": "the_roots",
  "Name": "The Roots",
  "Description": "Ancient maintenance tunnels beneath the facility...",
  "AvailableTemplates": [
    "corridor",
    "chamber",
    "junction",
    "maintenance_bay"
  ],
  "DescriptorCategories": {
    "Adjectives": ["Corroded", "Decaying", "Twisted", "Rusted"],
    "Details": [
      "Runic glyphs flicker weakly on the walls",
      "Condensation drips from overhead pipes"
    ],
    "Sounds": ["hissing steam", "groaning metal", "distant machinery"],
    "Smells": ["ozone", "rust", "stagnant water"]
  },
  "MinRoomCount": 5,
  "MaxRoomCount": 8,
  "BranchingProbability": 0.5,
  "SecretRoomProbability": 0.3,
  "Elements": {
    "Elements": [
      {
        "ElementName": "Corrupted Servitor",
        "ElementType": "DormantProcess",
        "Weight": 1.0,
        "AssociatedDataId": "corrupted_servitor",
        "SpawnCost": 1
      },
      {
        "ElementName": "Steam Vent",
        "ElementType": "DynamicHazard",
        "Weight": 0.5,
        "AssociatedDataId": "steam_vent",
        "SpawnRules": {
          "RequiredArchetype": "Geothermal"
        }
      }
    ]
  }
}
```

---

## Integration Points

### Called By

| Caller | Context |
|--------|---------|
| `DungeonGenerator` | Gets biome for generation parameters |
| `PopulationPipeline` | Gets element tables for spawning |
| `RoomInstantiator` | Gets descriptor categories for room descriptions |

### Works With

| Service | Interaction |
|---------|-------------|
| `TemplateLibrary` | Validates templates exist |
| `PopulationPipeline` | Uses BiomeElementTable for spawning |
| `CoherentGlitchRuleEngine` | Applies biome-specific rules |

---

## Data Flow

### Biome Loading Flow

```
Application Startup
        │
        ▼
BiomeLibrary.LoadBiomes()
        │
        ▼
┌─────────────────────────┐
│ Scan Data/Biomes/*.json │
└───────────┬─────────────┘
            │
            ▼ For each file
┌─────────────────────────┐
│ Deserialize JSON        │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Validate BiomeDefinition│
│ - BiomeId not empty     │
│ - Name not empty        │
│ - Templates exist       │
│ - Room counts valid     │
└───────────┬─────────────┘
            │
    Valid   │   Invalid
    ┌───────┴───────┐
    │               │
    ▼               ▼
Add to          Log warning,
_biomes         skip
```

### Biome Usage Flow

```
DungeonGenerator.Generate(seed, biome)
        │
        ▼
┌─────────────────────────┐
│ Get room count range    │
│ from biome parameters   │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Use branching/secret    │
│ probabilities           │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Select templates from   │
│ AvailableTemplates      │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Generate descriptions   │
│ from DescriptorCategories│
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Populate via            │
│ BiomeElementTable       │
└─────────────────────────┘
```

---

## Version History

| Version | Changes |
|---------|---------|
| v0.10 | Initial implementation with JSON loading |
| v0.11 | Added BiomeElementTable for population |
| v0.12 | Added CoherentGlitchRule element type |

---

## Cross-References

### Related Documentation

- [Procedural Generation](../../PROCEDURAL_GENERATION.md) - Generation overview
- [DungeonGenerator](./dungeon-generator.md) - Uses biome definitions

### Related Services

- [DungeonGenerator](./dungeon-generator.md) - Consumes biome parameters
- [PopulationPipeline](./population-pipeline.md) - Uses element tables
- [TemplateLibrary](./template-library.md) - Room templates

---

**Documentation Status:** ✅ Complete
**Last Reviewed:** 2025-11-27
