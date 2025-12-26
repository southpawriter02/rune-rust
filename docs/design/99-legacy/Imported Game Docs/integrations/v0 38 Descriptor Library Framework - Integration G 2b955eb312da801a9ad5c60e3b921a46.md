# v0.38: Descriptor Library Framework - Integration Guide

## Overview

The Descriptor Library Framework (v0.38) provides a unified, data-driven system for managing procedural content across all biomes. Instead of duplicating descriptors for each biome, the framework uses a three-tier composition model:

- **Tier 1**: Base Templates (biome-agnostic archetypes like `Pillar_Base`)
- **Tier 2**: Thematic Modifiers (biome-specific variations like `Scorched`, `Frozen`)
- **Tier 3**: Composite Descriptors (Base + Modifier = final content)

## Benefits

### Before v0.38 (Duplication)

```
Muspelheim: "Scorched Support Pillar" (defined manually)
Niflheim: "Ice-Covered Support Pillar" (defined manually)
Alfheim: "Crystalline Support Pillar" (defined manually)
The Roots: "Corroded Support Pillar" (defined manually)

```

### After v0.38 (Composition)

```
Base Template: "Pillar_Base" (defined once)
Modifiers: "Scorched", "Frozen", "Crystalline", "Rusted" (defined once each)
Composites: Pillar_Base + Scorched = "Scorched Support Pillar" (auto-generated)
            Pillar_Base + Frozen = "Ice-Covered Support Pillar" (auto-generated)
            ... etc.

```

**Result**: Define once, use everywhere. DRY principle applied to game content.

## Quick Start

### 1. Initialize the Database

Run the migration script to create the descriptor tables:

```bash
sqlite3 rune_and_rust.db < Data/v0.38.0_descriptor_framework_schema.sql

```

This creates:

- `Descriptor_Base_Templates`
- `Descriptor_Thematic_Modifiers`
- `Descriptor_Composites`
- Helper views and seed data

### 2. Initialize the Service

```csharp
using RuneAndRust.Persistence;
using RuneAndRust.Engine;

// Create repository
var repository = new DescriptorRepository(connectionString);

// Create service
var descriptorService = new DescriptorService(repository);

```

### 3. Query Descriptors

```csharp
// Get all cover features for Muspelheim
var query = new DescriptorQuery
{
    Category = "Feature",
    Archetype = "Cover",
    Biome = "Muspelheim"
};

var result = descriptorService.QueryDescriptors(query);

foreach (var descriptor in result.Descriptors)
{
    Console.WriteLine($"{descriptor.FinalName}: {descriptor.FinalDescription}");
}

// Output:
// Scorched Support Pillar: A scorched pillar that radiates intense heat...

```

## Integration with Dynamic Room Engine

### Example: Populating a Room with Features

The Dynamic Room Engine (v0.10-v0.12) can now use the descriptor system instead of hardcoded biome elements.

### Before (v0.29-v0.32): Hardcoded Biome Elements

```csharp
public class MuspelheimBiomeService
{
    private readonly MuspelheimDataRepository _dataRepository;

    public void PlaceHazardsInRoom(MuspelheimRoom room, Random random)
    {
        // Query hardcoded Muspelheim-specific environmental features
        var hazards = _dataRepository.GetEnvironmentalHazards(room.HazardDensity);

        foreach (var hazard in hazards)
        {
            // Place hazard...
        }
    }
}

```

### After (v0.38): Using Descriptor Service

```csharp
public class DynamicRoomEngine
{
    private readonly IDescriptorService _descriptorService;

    public void PopulateRoomWithFeatures(Room room, BiomeDefinition biome)
    {
        // Query descriptors dynamically based on room context
        var query = new DescriptorQuery
        {
            Category = "Feature",
            Archetype = "Cover",
            Biome = biome.BiomeId,
            RequiredTags = room.Tags  // Match room theme
        };

        var result = _descriptorService.QueryDescriptors(query);

        // Weighted selection based on spawn weights
        for (int i = 0; i < room.GetFeatureCount(); i++)
        {
            var descriptor = _descriptorService.WeightedRandomSelection(result.Descriptors);
            if (descriptor != null)
            {
                var feature = InstantiateFeature(descriptor);
                room.StaticTerrain.Add(feature);
            }
        }
    }

    private StaticTerrainFeature InstantiateFeature(DescriptorComposite descriptor)
    {
        // Parse mechanics from descriptor
        var mechanics = descriptor.GetFinalMechanics();

        return new StaticTerrainFeature
        {
            Name = descriptor.FinalName,
            Description = descriptor.FinalDescription,
            HP = GetMechanicValue<int>(mechanics, "hp"),
            Soak = GetMechanicValue<int>(mechanics, "soak"),
            CoverQuality = GetMechanicValue<string>(mechanics, "cover_quality"),
            IsDestructible = GetMechanicValue<bool>(mechanics, "destructible")
        };
    }

    private T GetMechanicValue<T>(Dictionary<string, object>? mechanics, string key)
    {
        if (mechanics == null || !mechanics.ContainsKey(key))
            return default(T)!;

        var value = mechanics[key];

        if (value is System.Text.Json.JsonElement element)
        {
            if (typeof(T) == typeof(int))
                return (T)(object)element.GetInt32();
            if (typeof(T) == typeof(bool))
                return (T)(object)element.GetBoolean();
            if (typeof(T) == typeof(string))
                return (T)(object)element.GetString()!;
        }

        return (T)value;
    }
}

```

### Example: Room Description Generation

```csharp
public class RoomDescriptionGenerator
{
    private readonly IDescriptorService _descriptorService;

    public string GenerateRoomDescription(Room room, BiomeDefinition biome)
    {
        // Get room descriptor
        var query = new DescriptorQuery
        {
            Category = "Room",
            Archetype = room.ArchetypeType,  // "Corridor", "Chamber", etc.
            Biome = biome.BiomeId
        };

        var result = _descriptorService.QueryDescriptors(query);
        var roomDescriptor = _descriptorService.WeightedRandomSelection(result.Descriptors);

        if (roomDescriptor == null)
            return "A nondescript room.";

        // Get atmospheric descriptors
        var atmosphericQuery = new DescriptorQuery
        {
            Category = "Atmospheric",
            Biome = biome.BiomeId,
            Limit = 3
        };

        var atmosphericResult = _descriptorService.QueryDescriptors(atmosphericQuery);

        var description = new System.Text.StringBuilder();
        description.AppendLine(roomDescriptor.FinalDescription);
        description.AppendLine();

        foreach (var atmospheric in atmosphericResult.Descriptors.Take(2))
        {
            description.AppendLine(atmospheric.FinalDescription);
        }

        return description.ToString();
    }
}

// Output example:
// A scorched corridor that radiates oppressive heat. The narrow confines
// create a sense of scorched claustrophobia.
//
// The air shimmers with heat distortion, making distant objects waver.
// A persistent hissing sound echoes from ruptured steam pipes.

```

## Creating New Descriptors

### 1. Add a Base Template

```sql
INSERT INTO Descriptor_Base_Templates (
    template_name, category, archetype,
    base_mechanics, name_template, description_template, tags
) VALUES (
    'Chasm_Base', 'Feature', 'Obstacle',
    '{"traversal_difficulty": 3, "fall_damage": "3d10", "bridgeable": true}',
    '{Modifier} Chasm',
    'A {Modifier_Adj} chasm that {Modifier_Detail}. Crossing requires careful navigation.',
    '["Obstacle", "Dangerous", "Traversable"]'
);

```

### 2. Create Composites

```sql
-- Automatically generate composites for all modifiers
INSERT INTO Descriptor_Composites (
    base_template_id, modifier_id,
    final_name, final_description, final_mechanics,
    biome_restrictions, spawn_weight
)
SELECT
    bt.template_id,
    tm.modifier_id,
    REPLACE(bt.name_template, '{Modifier}', tm.modifier_name),
    REPLACE(REPLACE(bt.description_template,
        '{Modifier_Adj}', tm.adjective),
        '{Modifier_Detail}', tm.detail_fragment),
    -- Merge mechanics (simplified here, use service for full merge logic)
    bt.base_mechanics,
    '["' || tm.primary_biome || '"]',
    1.0
FROM Descriptor_Base_Templates bt
CROSS JOIN Descriptor_Thematic_Modifiers tm
WHERE bt.template_name = 'Chasm_Base';

```

Result:

- `Scorched Chasm` (Muspelheim)
- `Frozen Chasm` (Niflheim)
- `Corroded Chasm` (The Roots)
- `Crystalline Chasm` (Alfheim)

### 3. Use in Code

```csharp
// Query chasms for Niflheim
var query = new DescriptorQuery
{
    Category = "Feature",
    Archetype = "Obstacle",
    Biome = "Niflheim"
};

var chasms = descriptorService.QueryDescriptors(query);

// Output:
// Frozen Chasm: An ice-covered chasm that drips with meltwater.
// Crossing requires careful navigation.

```

## Advanced Usage

### Custom Composition

Generate descriptors on-the-fly without database entries:

```csharp
var baseTemplate = descriptorService.GetBaseTemplate("Pillar_Base");
var modifier = descriptorService.GetModifier("Scorched");

// Generate composite
var composite = descriptorService.GenerateComposite(baseTemplate, modifier);

Console.WriteLine(composite.FinalName);  // "Scorched Support Pillar"
Console.WriteLine(composite.FinalDescription);

```

### Weighted Selection for Variety

```csharp
var query = new DescriptorQuery
{
    Category = "Feature",
    Archetype = "Cover",
    Biome = "Muspelheim"
};

var descriptors = descriptorService.QueryDescriptors(query).Descriptors;

// Select 5 features with varied weights
for (int i = 0; i < 5; i++)
{
    var selected = descriptorService.WeightedRandomSelection(descriptors);
    Console.WriteLine($"Selected: {selected?.FinalName}");
}

// Output (random, but weighted):
// Selected: Scorched Support Pillar
// Selected: Scorched Support Pillar  (higher weight = more likely)
// Selected: Scorched Machinery Wreckage
// Selected: Scorched Support Pillar
// Selected: Scorched Rubble Pile

```

### Tag-Based Filtering

```csharp
var query = new DescriptorQuery
{
    Category = "Feature",
    Biome = "The_Roots",
    RequiredTags = new List<string> { "Destructible", "Cover" },
    ExcludedTags = new List<string> { "Hazard" }
};

var safeCovers = descriptorService.QueryDescriptors(query);

// Returns only features that are:
// - In The Roots biome
// - Destructible AND provide cover
// - NOT hazards

```

## Migration from v0.29-v0.32

Use the `DescriptorMigrationTool` to analyze existing biomes:

```csharp
var migrationTool = new DescriptorMigrationTool(repository, connectionString);

// Analyze Muspelheim
var roomReport = migrationTool.AnalyzeBiomeRoomTemplates(4, "Muspelheim");
var featureReport = migrationTool.AnalyzeBiomeEnvironmentalFeatures(4, "Muspelheim");

// Generate SQL migration script
var reports = new List<BiomeMigrationReport> { roomReport, featureReport };
var migrationSQL = migrationTool.GenerateMigrationSQL(reports);

// Write to file
File.WriteAllText("migration_suggestions.sql", migrationSQL);

```

This generates:

1. Analysis of existing biome content
2. Suggestions for base templates to create
3. Suggestions for composite descriptors
4. Confidence scores (0.0-1.0) for each suggestion

## Performance Considerations

### Caching

For high-frequency queries, consider caching descriptor results:

```csharp
public class CachedDescriptorService
{
    private readonly IDescriptorService _inner;
    private readonly Dictionary<string, DescriptorQueryResult> _cache = new();

    public DescriptorQueryResult QueryDescriptors(DescriptorQuery query)
    {
        var key = SerializeQuery(query);

        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var result = _inner.QueryDescriptors(query);
        _cache[key] = result;
        return result;
    }
}

```

### Batch Loading

Load descriptors in batches during initialization:

```csharp
public class BiomeInitializer
{
    private readonly IDescriptorService _descriptorService;
    private readonly Dictionary<string, List<DescriptorComposite>> _preloadedDescriptors = new();

    public void PreloadDescriptorsForBiome(string biomeName)
    {
        // Preload all feature descriptors
        var features = _descriptorService.QueryDescriptors(new DescriptorQuery
        {
            Category = "Feature",
            Biome = biomeName
        });

        _preloadedDescriptors[$"{biomeName}_features"] = features.Descriptors;

        // Preload room descriptors
        var rooms = _descriptorService.QueryDescriptors(new DescriptorQuery
        {
            Category = "Room",
            Biome = biomeName
        });

        _preloadedDescriptors[$"{biomeName}_rooms"] = rooms.Descriptors;
    }

    public List<DescriptorComposite> GetPreloadedFeatures(string biomeName)
    {
        return _preloadedDescriptors.GetValueOrDefault($"{biomeName}_features", new());
    }
}

```

## Troubleshooting

### Issue: No descriptors returned for query

**Check:**

1. Are composites marked as `is_active = 1`?
2. Do biome restrictions match? (check `biome_restrictions` JSON)
3. Are required tags spelled correctly?

```csharp
// Debug query
var allComposites = descriptorService.QueryDescriptors(new DescriptorQuery
{
    OnlyActive = false  // Include inactive
});

Console.WriteLine($"Total composites: {allComposites.TotalCount}");

```

### Issue: Mechanics not merging correctly

**Check:**

- JSON format in `base_mechanics` and `stat_modifiers`
- Multiplier naming convention: `hp_multiplier` applies to `hp`

```csharp
// Test mechanics merge
var baseMechanics = "{\\"hp\\": 100}";
var modifiers = "{\\"hp_multiplier\\": 0.5}";

var merged = descriptorService.MergeMechanics(baseMechanics, modifiers);
Console.WriteLine(merged);  // Should contain "hp": 50

```

### Issue: Weighted selection always returns same descriptor

**Ensure:**

- Random instance is passed for deterministic testing
- Multiple descriptors have non-zero weights
- Total weight > 0

```csharp
var random = new Random();  // New instance each time
var selected = descriptorService.WeightedRandomSelection(descriptors, random);

```

## Next Steps

After implementing v0.38 parent framework:

- **v0.38.1**: Room Description Library (15+ room templates)
- **v0.38.2**: Environmental Feature Catalog (25+ features)
- **v0.38.3**: Interactive Object Repository (15+ objects)
- **v0.38.4**: Atmospheric Descriptor System (30+ atmospheric descriptors)
- **v0.38.5**: Loot & Resource Templates (20+ loot descriptors)

## References

- **Specification**: `docs/v0.38_descriptor_library_spec.md`
- **Database Schema**: `Data/v0.38.0_descriptor_framework_schema.sql`
- **Unit Tests**: `RuneAndRust.Tests/DescriptorServiceTests.cs`
- **Migration Tool**: `RuneAndRust.Engine/DescriptorMigrationTool.cs`

---

**v0.38 Parent Framework Status**: ✅ Complete
**Next**: v0.38.1 - Room Description Library