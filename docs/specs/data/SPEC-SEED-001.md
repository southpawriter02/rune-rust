---
id: SPEC-SEED-001
title: Database Seeding System
version: 1.0.1
status: Implemented
related_specs: [SPEC-REPO-001, SPEC-MIGRATE-001]
last_updated: 2025-12-24
---

# SPEC-SEED-001: Database Seeding System

> **Version:** 1.0.1
> **Status:** Implemented
> **Services:** AbilitySeeder, ConditionSeeder, HazardTemplateSeeder, CodexSeeder, RoomTemplateSeeder, TemplateLoaderService
> **Location:** `RuneAndRust.Persistence/Data/`, `RuneAndRust.Engine/Services/`

---

## Overview

The **Database Seeding System** initializes the Rune & Rust database with foundational game data using a **hybrid architecture**: C# seeder classes for static data and JSON files for configurable templates. The system ensures **idempotent seeding** (safe to run multiple times) and enforces **Domain 4 compliance** for all narrative content.

### Key Responsibilities

1. **Ability Seeding**: Initialize player and enemy combat abilities
2. **Condition Seeding**: Load ambient environmental conditions
3. **Hazard Seeding**: Populate hazard template prototypes
4. **Codex Seeding**: Create journal entries with unlock fragments
5. **Template Loading**: Parse JSON files for room templates and biome definitions
6. **Idempotent Execution**: Skip seeding if data already exists

### Architecture Pattern

```
Application Startup (Program.cs)
        ↓
    DI Scope Created
        ↓
┌───────────────────────────────────────────────────────────────┐
│                    Static C# Seeders                          │
├───────────────────────────────────────────────────────────────┤
│ AbilitySeeder.SeedAsync()     → ActiveAbilities (16 records)  │
│ ConditionSeeder.SeedAsync()   → AmbientConditions (8 records) │
│ HazardTemplateSeeder.SeedAsync() → HazardTemplates (12 records)│
└───────────────────────────────────────────────────────────────┘
        ↓
┌───────────────────────────────────────────────────────────────┐
│                    Optional Seeders (Not Auto-Called)         │
├───────────────────────────────────────────────────────────────┤
│ CodexSeeder.SeedAsync()       → CodexEntries (4 records)      │
│ (Available but not called in Program.cs initialization)       │
└───────────────────────────────────────────────────────────────┘
        ↓
┌───────────────────────────────────────────────────────────────┐
│                    JSON Template Loader                       │
├───────────────────────────────────────────────────────────────┤
│ RoomTemplateSeeder.SeedAsync()                                │
│   → ITemplateLoaderService.LoadAllTemplatesAsync()            │
│     → /data/templates/*.json (20 room templates)              │
│     → /data/biomes/the_roots.json (1 biome + 28 elements)     │
└───────────────────────────────────────────────────────────────┘
        ↓
    DbContext.SaveChangesAsync()
```

**Key Design Decision**: The system uses a **hybrid approach** combining:
- **C# Seeder Classes**: For static game data (abilities, conditions, hazards) that requires Domain 4 compliance validation during development
- **JSON Files**: For configurable templates (rooms, biomes) that may be modified by game designers without recompilation

**Technology Stack**:
- **C# Static Classes**: Seeder methods with `async Task SeedAsync(DbContext)`
- **System.Text.Json**: JSON deserialization for template files
- **Entity Framework Core**: Database persistence via DbContext
- **PostgreSQL JSONB**: Complex nested data stored as JSON columns

---

## Core Concepts

### 1. Idempotent Seeding Pattern

**Definition**: Seeders check for existing data before inserting, ensuring safe re-execution.

**Pattern**:
```csharp
public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
{
    // Guard clause: Skip if data exists
    if (await context.ActiveAbilities.AnyAsync())
    {
        logger?.LogDebug("Active abilities already exist, skipping seed");
        return;
    }

    // Seed data
    var abilities = CreateAbilities();
    await context.ActiveAbilities.AddRangeAsync(abilities);
    await context.SaveChangesAsync();

    logger?.LogInformation("Seeded {Count} active abilities", abilities.Count);
}
```

**Benefits**:
- **Safe Re-runs**: Application can restart without duplicate data
- **Development Friendly**: Can drop/recreate database during development
- **Incremental Updates**: Can add new seed data without affecting existing

**Limitation**: Does not handle partial seeding failures (all-or-nothing per seeder).

---

### 2. Static Seeder Class Pattern

**Definition**: C# static classes with `SeedAsync` methods for domain data.

**Structure**:
```csharp
public static class AbilitySeeder
{
    public static async Task SeedAsync(
        RuneAndRustDbContext context,
        ILogger? logger = null)
    {
        // 1. Idempotency check
        if (await context.ActiveAbilities.AnyAsync())
            return;

        // 2. Create seed data
        var abilities = new List<ActiveAbility>
        {
            new() { Id = Guid.NewGuid(), Name = "...", ... },
            // ... more entities
        };

        // 3. Persist
        await context.ActiveAbilities.AddRangeAsync(abilities);
        await context.SaveChangesAsync();
    }
}
```

**Location**: `/Users/ryan/Documents/GitHub/rune-rust/RuneAndRust.Persistence/Data/`

**Characteristics**:
- **Static Class/Method**: No instantiation, called directly
- **DbContext Dependency**: Passed as parameter (not injected)
- **Optional Logger**: For debug/info output during seeding
- **Async/Await**: Non-blocking database operations

**Seeder Classes**:
| Class | Target Table | Records | Purpose |
|-------|--------------|---------|---------|
| `AbilitySeeder` | ActiveAbilities | 16 | Player and enemy combat abilities |
| `ConditionSeeder` | AmbientConditions | 8 | Environmental room effects |
| `HazardTemplateSeeder` | HazardTemplates | 13 | Prototype hazard definitions |
| `CodexSeeder` | CodexEntries | 4 | Journal entries with fragments |
| `RoomTemplateSeeder` | RoomTemplates, BiomeDefinitions, BiomeElements | 48+ | Dynamic room generation data |

---

### 3. JSON Template Loading

**Definition**: External JSON files parsed and persisted via `ITemplateLoaderService`.

**File Structure**:
```
data/
├── templates/                    # Room template JSON files (20)
│   ├── operations_nexus.json
│   ├── collapsed_entry_hall.json
│   ├── maintenance_access.json
│   └── ... (17 more)
├── biomes/                       # Biome definition JSON files
│   └── the_roots.json            # Contains biome + 27 spawn elements
└── dialogues/                    # NPC dialogue JSON files (8)
    ├── astrid_dialogues.json
    └── ... (7 more)
```

**Room Template Structure** (`operations_nexus.json`):
```json
{
  "TemplateId": "operations_nexus",
  "Biome": "the_roots",
  "Size": "Large",
  "Archetype": "Junction",
  "NameTemplates": [
    "The {Adjective} Operations Nexus",
    "A {Adjective} Command Center",
    "The {Adjective} Control Hub"
  ],
  "Adjectives": ["Vast", "Silent", "Dormant", "Humming", "Echoing"],
  "DescriptionTemplates": [
    "A {Adjective} operations nexus opens before you. {Detail}",
    "You enter a {Adjective} command center. {Detail}"
  ],
  "Details": [
    "Dormant terminals line the walls, their screens dark.",
    "The hum of distant machinery echoes through the space."
  ],
  "MinConnectionPoints": 3,
  "MaxConnectionPoints": 5,
  "ValidConnections": ["Corridor", "Chamber", "Junction"],
  "Difficulty": "Easy",
  "Tags": ["Branching", "Choice", "Hub", "Safe"]
}
```

**Biome Definition Structure** (`the_roots.json`):
```json
{
  "BiomeId": "the_roots",
  "Name": "[The Roots]",
  "Description": "The industrial underbelly of a forgotten age...",
  "AvailableTemplates": ["operations_nexus", "collapsed_entry_hall", ...],
  "DescriptorCategories": {
    "Adjectives": ["Rusted", "Silent", "Dormant", ...],
    "Details": ["Pipes drip with condensation.", ...],
    "Sounds": ["The groan of settling metal.", ...],
    "Smells": ["Stale oil and decay.", ...]
  },
  "MinRoomCount": 5,
  "MaxRoomCount": 7,
  "BranchingProbability": 0.4,
  "SecretRoomProbability": 0.2,
  "Elements": {
    "Elements": [
      {
        "ElementName": "Rust-Horror",
        "ElementType": "DormantProcess",
        "Weight": 0.20,
        "SpawnCost": 1,
        "AssociatedDataId": "rust_horror",
        "SpawnRules": {
          "MinDifficulty": "Easy",
          "MaxPerBiome": 3,
          "RequiredTags": [],
          "ForbiddenTags": ["Safe"]
        }
      }
      // ... 26 more elements (enemies, hazards, terrain, loot, conditions)
    ]
  }
}
```

**Benefits**:
- **Designer-Editable**: JSON files can be modified without recompilation
- **Version Control**: JSON changes tracked in git
- **Validation**: Deserialization fails fast on malformed JSON

---

### 4. TemplateLoaderService

**Definition**: Service that reads, parses, and persists JSON template files.

**Interface**:
```csharp
public interface ITemplateLoaderService
{
    Task LoadAllTemplatesAsync();
    Task LoadRoomTemplatesFromDirectoryAsync(string directoryPath);
    Task LoadBiomeDefinitionAsync(string filePath);
}
```

**Implementation** (`TemplateLoaderService`):
```csharp
public class TemplateLoaderService : ITemplateLoaderService
{
    private readonly RuneAndRustDbContext _context;
    private readonly ILogger<TemplateLoaderService> _logger;

    public async Task LoadAllTemplatesAsync()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var templatesPath = Path.Combine(baseDir, "data", "templates");
        var biomesPath = Path.Combine(baseDir, "data", "biomes");

        await LoadRoomTemplatesFromDirectoryAsync(templatesPath);

        var biomeFile = Path.Combine(biomesPath, "the_roots.json");
        if (File.Exists(biomeFile))
            await LoadBiomeDefinitionAsync(biomeFile);
    }

    public async Task LoadRoomTemplatesFromDirectoryAsync(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            _logger.LogWarning("Templates directory not found: {Path}", directoryPath);
            return;
        }

        var jsonFiles = Directory.GetFiles(directoryPath, "*.json");

        foreach (var file in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var template = JsonSerializer.Deserialize<RoomTemplate>(json, _jsonOptions);

                if (template?.TemplateId == null)
                {
                    _logger.LogWarning("Invalid template (no TemplateId): {File}", file);
                    continue;
                }

                // Upsert pattern: update if exists, insert if new
                var existing = await _context.RoomTemplates
                    .FirstOrDefaultAsync(t => t.TemplateId == template.TemplateId);

                if (existing != null)
                {
                    // Update existing
                    existing.Biome = template.Biome;
                    existing.NameTemplates = template.NameTemplates;
                    // ... update other properties
                    _context.RoomTemplates.Update(existing);
                }
                else
                {
                    // Insert new
                    await _context.RoomTemplates.AddAsync(template);
                }

                _logger.LogDebug("Loaded template: {TemplateId}", template.TemplateId);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse template: {File}", file);
                // Continue with next file (don't fail entire seed)
            }
        }

        await _context.SaveChangesAsync();
    }
}
```

**Location**: `/Users/ryan/Documents/GitHub/rune-rust/RuneAndRust.Engine/Services/TemplateLoaderService.cs`

**Key Features**:
- **Directory Scanning**: Loads all `*.json` files in templates directory
- **Upsert Pattern**: Updates existing templates, inserts new ones
- **Error Resilience**: Per-file exception handling prevents cascading failures
- **Logging**: Debug messages for successful loads, warnings/errors for failures

---

### 5. Seeding Initialization Sequence

**Definition**: Order of seeder execution during application startup.

**Location**: `/Users/ryan/Documents/GitHub/rune-rust/RuneAndRust.Terminal/Program.cs` (lines 190-203)

**Sequence**:
```csharp
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RuneAndRustDbContext>();
    var templateLoader = scope.ServiceProvider.GetRequiredService<ITemplateLoaderService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // 1. Seed abilities (player + enemy)
    await AbilitySeeder.SeedAsync(context);

    // 2. Seed ambient conditions
    await ConditionSeeder.SeedAsync(context);

    // 3. Seed hazard templates
    await HazardTemplateSeeder.SeedAsync(context);

    // 4. Seed room templates and biome definitions (v0.4.0)
    await RoomTemplateSeeder.SeedAsync(context, templateLoader, logger);

    // NOTE: CodexSeeder.SeedAsync() exists but is NOT called in current initialization
    // It can be invoked manually when codex/journal features are implemented
}
```

**Order Rationale**:
1. **Abilities first**: Required for character creation and combat
2. **Conditions next**: Referenced by room generation
3. **Hazards next**: Referenced by room generation
4. **Templates last**: Depends on other data being present; largest dataset

> **Note**: CodexSeeder exists but is not called during application startup.
> It will be integrated when the journal/codex feature is fully implemented.

**Timing**: Runs after `MigrateAsync()` but before game loop starts.

---

### 6. Domain 4 Compliance

**Definition**: All seeded narrative content must adhere to Domain 4 (Technology Constraints).

**Rule**: POST-Glitch content cannot contain precision measurements.

**Forbidden**:
- ❌ "95% chance"
- ❌ "4.2 meters"
- ❌ "35°C"
- ❌ "18 seconds"

**Allowed**:
- ✅ "Almost certain"
- ✅ "A spear's throw"
- ✅ "Oppressively hot"
- ✅ "Several moments"

**Example (AbilitySeeder)**:
```csharp
// ❌ Forbidden (precision)
new ActiveAbility
{
    Name = "Cleaving Strike",
    Description = "Deal 45 damage in a 3-meter cone."
}

// ✅ Compliant (qualitative)
new ActiveAbility
{
    Name = "Cleaving Strike",
    Description = "A brutal swing that tears through nearby foes."
}
```

**Enforcement**: Manual review during development. All seeded descriptions are written in Domain 4 compliant voice.

---

## Behaviors

### B-1: AbilitySeeder.SeedAsync - Combat Abilities

**Signature**: `Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)`

**Purpose**: Initialize player archetype abilities and enemy-specific abilities.

**Seeded Data**:

**Player Abilities (8)** - 2 per archetype at Tier 1:
| Archetype | Ability 1 | Ability 2 |
|-----------|-----------|-----------|
| Warrior | Crushing Blow | Iron Stance |
| Skirmisher | Crippling Strike | Shadow Step |
| Mystic | Psychic Lash | Mind Ward |
| Adept | Galdr: Flame Tongue | Galdr: Frost Bind |

**Enemy Abilities (8)** - Archetype = null:
| Ability | Used By | Effect |
|---------|---------|--------|
| Rusty Cleave | Rusted Servitor | Wide sweep attack |
| Grave Chill | Blight-Touched | Cold aura damage |
| Servo Slam | Iron-Husk | Heavy impact |
| Savage Lunge | Rust-Horror | Gap-closing strike |
| Corrosive Spit | Spore-Beast | Acid projectile |
| Overload Surge | Malfunctioning Drones | Electric burst |
| Rending Claws | Feral Scavengers | Bleed attack |
| Psychic Shriek | Mind-Broken | AoE psychic damage |

**Code**:
```csharp
public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
{
    if (await context.ActiveAbilities.AnyAsync())
    {
        logger?.LogDebug("Active abilities already exist, skipping seed");
        return;
    }

    var abilities = new List<ActiveAbility>
    {
        // Warrior Tier 1
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Crushing Blow",
            Description = "A devastating overhead strike that staggers the target.",
            StaminaCost = 15,
            AetherCost = 0,
            CooldownTurns = 2,
            Range = 1,
            EffectScript = "damage:physical:high;debuff:stagger:1",
            Archetype = Archetype.Warrior,
            Tier = 1
        },
        // ... 15 more abilities
    };

    await context.ActiveAbilities.AddRangeAsync(abilities);
    await context.SaveChangesAsync();

    logger?.LogInformation("Seeded {Count} active abilities", abilities.Count);
}
```

**Domain 4 Compliance**: All ability descriptions use qualitative language ("devastating," "staggers") not precision values.

---

### B-2: ConditionSeeder.SeedAsync - Ambient Conditions

**Signature**: `Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)`

**Purpose**: Initialize room-wide environmental effects.

**Seeded Data (8 conditions)**:
| Condition | Biome Tags | Effect |
|-----------|------------|--------|
| Psychic Resonance | Void, Ruin | Mental strain each turn |
| Toxic Atmosphere | Industrial, Organic | Poison damage over time |
| Deep Cold | Void | Slow stamina regeneration |
| Scorching Heat | Industrial | Faster stamina drain |
| Low Visibility | All | Reduced perception |
| Blighted Ground | Organic | Corruption damage on movement |
| Static Field | Industrial | Electric damage chance |
| Dread Presence | Void, Ruin | Fear debuff risk |

**Code**:
```csharp
public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
{
    if (await context.AmbientConditions.AnyAsync())
    {
        logger?.LogDebug("Ambient conditions already exist, skipping seed");
        return;
    }

    var conditions = new List<AmbientCondition>
    {
        new()
        {
            Id = Guid.NewGuid(),
            Type = ConditionType.Environmental,
            Name = "Psychic Resonance",
            Description = "The air thrums with latent psychic energy, pressing against your thoughts.",
            Color = "purple",
            TickScript = "damage:psychic:minor;chance:0.3",
            TickChance = 0.3,
            BiomeTags = new List<string> { "Void", "Ruin" }
        },
        // ... 7 more conditions
    };

    await context.AmbientConditions.AddRangeAsync(conditions);
    await context.SaveChangesAsync();

    logger?.LogInformation("Seeded {Count} ambient conditions", conditions.Count);
}
```

**JSONB Storage**: `BiomeTags` stored as PostgreSQL JSONB array.

---

### B-3: HazardTemplateSeeder.SeedAsync - Hazard Prototypes

**Signature**: `Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)`

**Purpose**: Initialize hazard prototypes grouped by biome.

**Seeded Data (12 templates)**:

**Ruin Hazards (3)**:
- Pressure Plate - Triggers trap when stepped on
- Collapsing Floor - Falls through on second step
- Dart Trap - Ranged projectile trap

**Industrial Hazards (3)**:
- Steam Vent - Periodic burst damage
- Electrified Floor - Constant electric damage zone
- Unstable Machinery - Explosion on interaction

**Organic Hazards (3)**:
- Spore Pod - Releases poison cloud when disturbed
- Corruption Pool - Acid damage zone
- Grasping Tendrils - Immobilizes on contact

**Void Hazards (3)**:
- Reality Fissure - Teleports player randomly
- Entropy Field - Drains stamina over time
- Echoing Whispers - Psychic damage and confusion

**Code**:
```csharp
public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
{
    if (await context.HazardTemplates.AnyAsync())
    {
        logger?.LogDebug("Hazard templates already exist, skipping seed");
        return;
    }

    var templates = new List<HazardTemplate>
    {
        // Ruin Hazards
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Pressure Plate",
            Description = "A section of floor that sinks slightly underfoot. Something clicks.",
            HazardType = HazardType.Trap,
            Trigger = TriggerType.OnStep,
            EffectScript = "trap:trigger:dart_volley",
            MaxCooldown = 3,
            OneTimeUse = false,
            BiomeTags = new List<string> { "Ruin" }
        },
        // ... 12 more templates
    };

    await context.HazardTemplates.AddRangeAsync(templates);
    await context.SaveChangesAsync();

    logger?.LogInformation("Seeded {Count} hazard templates", templates.Count);
}
```

**Domain 4 Compliance**: Descriptions like "sinks slightly underfoot" avoid precision measurements.

---

### B-4: CodexSeeder.SeedAsync - Journal Entries

**Signature**: `Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)`

**Purpose**: Initialize Scavenger's Journal codex entries with progressive unlock fragments.

**Seeded Data (4 entries)**:

**Field Guide Entries (3)** - Tutorial information:
| Entry | Fragments | Topic |
|-------|-----------|-------|
| Psychic Stress | 1 | Mental health mechanics |
| Combat Basics | 1 | Combat system tutorial |
| Burden and Carrying | 1 | Encumbrance system |

**Bestiary Entries (1)** - Enemy lore with progressive unlock:
| Entry | Fragments | Unlock Thresholds |
|-------|-----------|-------------------|
| Rusted Servitor | 4 | 1: Basic, 2: Behavior, 3: Weaknesses, 4: Origin |

**Code**:
```csharp
public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
{
    if (await context.CodexEntries.AnyAsync())
    {
        logger?.LogDebug("Codex entries already exist, skipping seed");
        return;
    }

    var entries = new List<CodexEntry>
    {
        // Field Guide: Psychic Stress
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Psychic Stress",
            Category = CodexCategory.FieldGuide,
            FullText = "The Blight does not merely corrupt the body...",
            TotalFragments = 1,
            UnlockThresholds = new Dictionary<int, string>
            {
                { 1, "Full entry unlocked" }
            }
        },

        // Bestiary: Rusted Servitor (4 fragments)
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Rusted Servitor",
            Category = CodexCategory.Bestiary,
            FullText = "[Full lore text with all 4 sections...]",
            TotalFragments = 4,
            UnlockThresholds = new Dictionary<int, string>
            {
                { 1, "Basic identification unlocked" },
                { 2, "Behavioral patterns revealed" },
                { 3, "Combat weaknesses discovered" },
                { 4, "Origin and purpose understood" }
            }
        }
    };

    await context.CodexEntries.AddRangeAsync(entries);
    await context.SaveChangesAsync();

    logger?.LogInformation("Seeded {Count} codex entries", entries.Count);
}
```

**JSONB Storage**: `UnlockThresholds` stored as PostgreSQL JSONB dictionary.

---

### B-5: RoomTemplateSeeder.SeedAsync - JSON Template Loading

**Signature**: `Task SeedAsync(RuneAndRustDbContext context, ITemplateLoaderService loader, ILogger? logger)`

**Purpose**: Orchestrate loading of room templates and biome definitions from JSON files.

**Code**:
```csharp
public static async Task SeedAsync(
    RuneAndRustDbContext context,
    ITemplateLoaderService templateLoader,
    ILogger? logger = null)
{
    // Check if templates already exist
    if (await context.RoomTemplates.AnyAsync())
    {
        logger?.LogDebug("Room templates already exist, checking for updates");
        // Continue with upsert logic (update existing, add new)
    }

    // Delegate to template loader service
    await templateLoader.LoadAllTemplatesAsync();

    logger?.LogInformation("Room template seeding complete");
}
```

**Loaded Data**:
- **Room Templates**: 20 JSON files from `/data/templates/`
- **Biome Definition**: 1 JSON file from `/data/biomes/the_roots.json`
- **Biome Elements**: 27 spawn configurations embedded in biome definition

---

## Restrictions

### R-1: Idempotent Execution
- **Rule**: All seeders MUST check for existing data before inserting.
- **Rationale**: Prevents duplicate data on application restart.
- **Enforcement**: Guard clause with `AnyAsync()` at start of each seeder.

### R-2: Domain 4 Compliance
- **Rule**: All narrative content (descriptions, names, dialogue) MUST avoid precision measurements.
- **Rationale**: POST-Glitch world lacks accurate measurement technology.
- **Enforcement**: Manual review during development; AAM-VOICE validation.

### R-3: Async Operations
- **Rule**: All seeding operations MUST be async.
- **Rationale**: Non-blocking I/O for database operations.
- **Enforcement**: All seeder methods return `Task`.

### R-4: Error Isolation
- **Rule**: JSON template loading MUST NOT fail entire seed on single file error.
- **Rationale**: One malformed template shouldn't prevent all seeding.
- **Enforcement**: Per-file try/catch in `TemplateLoaderService`.

---

## Limitations

### L-1: No Incremental Updates for Static Seeders
- **Issue**: Static seeders (Ability, Condition, Hazard, Codex) are all-or-nothing.
- **Impact**: Cannot add single new ability without clearing table.
- **Workaround**: Add new records manually or clear table and re-seed.
- **Future Enhancement**: Per-entity upsert logic in static seeders.

### L-2: No Foreign Key Validation
- **Issue**: Seeders don't validate FK references (e.g., BiomeElement → RoomTemplate).
- **Impact**: Invalid references cause runtime errors, not seed-time errors.
- **Workaround**: Careful ordering of seed execution.
- **Future Enhancement**: Add FK validation step before SaveChanges.

### L-3: Hardcoded File Paths
- **Issue**: JSON paths use `AppDomain.CurrentDomain.BaseDirectory` + relative paths.
- **Impact**: Files must be copied to output directory during build.
- **Workaround**: `.csproj` includes `<CopyToOutputDirectory>` for data files.

### L-4: No Versioning
- **Issue**: No mechanism to track which seed version has been applied.
- **Impact**: Cannot detect if new seed data should be applied to existing database.
- **Future Enhancement**: SeedVersion table tracking applied seeds.

---

## Use Cases

### UC-1: Fresh Database Initialization

**Scenario**: First application run after database creation.

**Actors**: Program.cs, All Seeders, DbContext

**Sequence**:
```
1. Application starts
2. DbContext.MigrateAsync() creates schema (no data yet)
3. DI scope created
4. AbilitySeeder.SeedAsync() - AnyAsync() returns false, seeds 16 abilities
5. ConditionSeeder.SeedAsync() - AnyAsync() returns false, seeds 8 conditions
6. HazardTemplateSeeder.SeedAsync() - AnyAsync() returns false, seeds 12 hazards
7. RoomTemplateSeeder.SeedAsync() - loads 20 templates + 1 biome + 28 elements
8. All SaveChangesAsync() calls succeed
9. Game loop starts with fully seeded database
(Note: CodexSeeder is not called during initialization)
```

**Log Output**:
```
[INFO] Seeded 16 active abilities
[INFO] Seeded 8 ambient conditions
[INFO] Seeded 12 hazard templates
[INFO] Room template seeding complete
```

---

### UC-2: Application Restart (Data Exists)

**Scenario**: Application restarts with previously seeded database.

**Actors**: Program.cs, All Seeders, DbContext

**Sequence**:
```
1. Application starts
2. DbContext.MigrateAsync() - no new migrations, schema unchanged
3. DI scope created
4. AbilitySeeder.SeedAsync() - AnyAsync() returns true, skips
5. ConditionSeeder.SeedAsync() - AnyAsync() returns true, skips
6. HazardTemplateSeeder.SeedAsync() - AnyAsync() returns true, skips
7. RoomTemplateSeeder.SeedAsync() - checks for updates, upserts if needed
8. Game loop starts immediately (minimal seeding time)
```

**Log Output**:
```
[DEBUG] Active abilities already exist, skipping seed
[DEBUG] Ambient conditions already exist, skipping seed
[DEBUG] Hazard templates already exist, skipping seed
[DEBUG] Codex entries already exist, skipping seed
[DEBUG] Room templates already exist, checking for updates
```

---

### UC-3: Adding New Room Template

**Scenario**: Game designer adds new room template JSON file.

**Actors**: Designer, TemplateLoaderService, DbContext

**Sequence**:
```
1. Designer creates new file: /data/templates/storage_vault.json
2. Application restarts
3. RoomTemplateSeeder.SeedAsync() called
4. TemplateLoaderService.LoadRoomTemplatesFromDirectoryAsync()
5. Enumerates all *.json files including new storage_vault.json
6. Deserializes storage_vault.json to RoomTemplate
7. Checks: existing template with TemplateId="storage_vault"? NO
8. Inserts new RoomTemplate entity
9. SaveChangesAsync() persists new template
10. New room type available for generation
```

**JSON File** (`storage_vault.json`):
```json
{
  "TemplateId": "storage_vault",
  "Biome": "the_roots",
  "Size": "Medium",
  "Archetype": "Chamber",
  "NameTemplates": ["The {Adjective} Storage Vault"],
  "Adjectives": ["Forgotten", "Sealed", "Breached"],
  "DescriptionTemplates": ["A {Adjective} storage vault. {Detail}"],
  "Details": ["Crates and containers line the walls."],
  "MinConnectionPoints": 1,
  "MaxConnectionPoints": 2,
  "ValidConnections": ["Corridor"],
  "Difficulty": "Medium",
  "Tags": ["Loot", "Dead-End"]
}
```

---

### UC-4: Malformed JSON Handling

**Scenario**: JSON template file has syntax error.

**Actors**: TemplateLoaderService, Logger

**Sequence**:
```
1. Application starts seeding
2. TemplateLoaderService.LoadRoomTemplatesFromDirectoryAsync()
3. Enumerates *.json files
4. Attempts to parse malformed_template.json
5. JsonSerializer.Deserialize throws JsonException
6. Exception caught in per-file try/catch
7. Logger.LogError with file path and exception
8. Continues to next file (seeding not aborted)
9. Other templates loaded successfully
10. SaveChangesAsync() persists valid templates only
```

**Log Output**:
```
[ERROR] Failed to parse template: /data/templates/malformed_template.json
        JsonException: Unexpected character at line 15, position 3
[DEBUG] Loaded template: operations_nexus
[DEBUG] Loaded template: collapsed_entry_hall
... (continues with valid templates)
```

---

### UC-5: Biome Element Spawn Configuration

**Scenario**: Room generator needs enemy spawn weights for "the_roots" biome.

**Actors**: RoomGeneratorService, BiomeElement entities (seeded)

**Sequence**:
```
1. RoomGeneratorService generating room in "the_roots"
2. Queries BiomeElements for "the_roots" biome
3. Returns 27 seeded elements:
   - 5 DormantProcess (enemies): Rust-Horror, Servitor, etc.
   - 4 Hazard (traps): Steam Vent, Pressure Plate, etc.
   - 6 Terrain (environmental): Rubble, Debris, etc.
   - 8 Loot (containers): Crate, Locker, etc.
   - 4 Condition (effects): Toxic Atmosphere, etc.
4. Applies spawn weights and rules from seeded data
5. Generates room with appropriate encounters
```

**Seeded Element Example**:
```json
{
  "ElementName": "Rust-Horror",
  "ElementType": "DormantProcess",
  "Weight": 0.20,
  "SpawnCost": 1,
  "AssociatedDataId": "rust_horror",
  "SpawnRules": {
    "MinDifficulty": "Easy",
    "MaxPerBiome": 3,
    "RequiredTags": [],
    "ForbiddenTags": ["Safe"]
  }
}
```

---

### UC-6: Codex Fragment Discovery

**Scenario**: Player discovers fragment of Rusted Servitor bestiary entry.

**Actors**: JournalService, DataCaptureRepository, CodexEntry (seeded)

**Sequence**:
```
1. Player defeats Rusted Servitor enemy
2. JournalService.DiscoverFragmentAsync("Rusted Servitor", 1)
3. Looks up CodexEntry by title
4. Seeded entry has TotalFragments = 4, UnlockThresholds defined
5. Creates DataCapture record (fragment 1 discovered)
6. Returns unlock message: "Basic identification unlocked"
7. Player journal shows partial entry
8. Future discoveries unlock fragments 2, 3, 4
```

**Seeded CodexEntry**:
```csharp
new CodexEntry
{
    Title = "Rusted Servitor",
    Category = CodexCategory.Bestiary,
    TotalFragments = 4,
    UnlockThresholds = new Dictionary<int, string>
    {
        { 1, "Basic identification unlocked" },
        { 2, "Behavioral patterns revealed" },
        { 3, "Combat weaknesses discovered" },
        { 4, "Origin and purpose understood" }
    }
}
```

---

## Decision Trees

### DT-1: Seed Execution Flow

**Trigger**: Application startup after MigrateAsync()

```
Start Seeding
│
├─ AbilitySeeder.SeedAsync()
│  ├─ ActiveAbilities.AnyAsync()?
│  │  ├─ YES → Skip (log debug)
│  │  └─ NO → Seed 16 abilities → SaveChangesAsync()
│
├─ ConditionSeeder.SeedAsync()
│  ├─ AmbientConditions.AnyAsync()?
│  │  ├─ YES → Skip (log debug)
│  │  └─ NO → Seed 8 conditions → SaveChangesAsync()
│
├─ HazardTemplateSeeder.SeedAsync()
│  ├─ HazardTemplates.AnyAsync()?
│  │  ├─ YES → Skip (log debug)
│  │  └─ NO → Seed 12 hazards → SaveChangesAsync()
│
│  (Note: CodexSeeder.SeedAsync() exists but is not called here)
│
└─ RoomTemplateSeeder.SeedAsync()
   └─ TemplateLoaderService.LoadAllTemplatesAsync()
      ├─ For each JSON file in /data/templates/:
      │  ├─ Parse JSON → RoomTemplate
      │  ├─ Template exists in DB?
      │  │  ├─ YES → Update existing
      │  │  └─ NO → Insert new
      │  └─ Continue to next file
      ├─ For biome definition:
      │  ├─ Parse JSON → BiomeDefinition + BiomeElements
      │  └─ Upsert all
      └─ SaveChangesAsync()
```

---

### DT-2: JSON Template Loading

**Trigger**: TemplateLoaderService.LoadRoomTemplatesFromDirectoryAsync()

```
Load Templates from Directory
│
├─ Directory exists?
│  ├─ NO → Log warning, return (no templates loaded)
│  └─ YES → Continue
│
├─ Get all *.json files
│
├─ For each JSON file:
│  │
│  ├─ Read file contents
│  │
│  ├─ Deserialize JSON
│  │  ├─ JsonException?
│  │  │  ├─ YES → Log error, continue to next file
│  │  │  └─ NO → Continue
│  │
│  ├─ Validate TemplateId
│  │  ├─ Null/Empty?
│  │  │  ├─ YES → Log warning, continue to next file
│  │  │  └─ NO → Continue
│  │
│  ├─ Check existing in database
│  │  ├─ Exists?
│  │  │  ├─ YES → Update entity properties
│  │  │  └─ NO → Add new entity
│  │
│  └─ Log debug: "Loaded template: {TemplateId}"
│
└─ SaveChangesAsync() - persist all changes
```

---

### DT-3: Domain 4 Content Validation

**Trigger**: Writing seeder content (abilities, conditions, hazards, codex)

```
Create Description Text
│
├─ Contains numeric measurement?
│  ├─ Distance (meters, feet, etc.) → ❌ Replace with qualitative
│  ├─ Temperature (°C, °F) → ❌ Replace with descriptive
│  ├─ Time (seconds, minutes) → ❌ Replace with vague
│  ├─ Percentage (%) → ❌ Replace with likelihood words
│  └─ None → ✅ Continue
│
├─ Contains forbidden technology terms?
│  ├─ "Bug", "Glitch", "API", "Debug" → ❌ Replace with in-world terms
│  └─ None → ✅ Continue
│
├─ Uses appropriate voice?
│  ├─ Omniscient narrator → ❌ Rewrite as observer
│  ├─ Technical precision → ❌ Rewrite as uncertain
│  └─ Atmospheric/qualitative → ✅ Accept
│
└─ Final content is Domain 4 compliant
```

**Transformation Examples**:
| Forbidden | Compliant |
|-----------|-----------|
| "Deals 45 damage" | "Deals devastating damage" |
| "Range: 3 meters" | "Reaches nearby targets" |
| "Lasts 10 seconds" | "Persists briefly" |
| "75% chance" | "Often succeeds" |
| "Temperature: -20°C" | "Bitterly cold" |

---

## Cross-Links

### Dependencies (Systems SPEC-SEED-001 relies on)

1. **SPEC-REPO-001 (Repository Pattern)**
   - **Relationship**: Seeders use DbContext (repository layer) for persistence
   - **Integration Point**: `context.AddRangeAsync()`, `context.SaveChangesAsync()`

2. **SPEC-MIGRATE-001 (Migration System)**
   - **Relationship**: Seeders run after migrations create schema
   - **Integration Point**: `MigrateAsync()` executes before seeding in Program.cs

---

### Dependents (Systems that rely on SPEC-SEED-001)

1. **Combat System**
   - **Relationship**: Uses seeded ActiveAbility data for player/enemy actions
   - **Integration Point**: `IActiveAbilityRepository.GetByArchetypeAsync()`

2. **Exploration System**
   - **Relationship**: Uses seeded room templates for procedural generation
   - **Integration Point**: `IRoomTemplateRepository`, `IBiomeDefinitionRepository`

3. **Journal System**
   - **Relationship**: Uses seeded CodexEntry data for discovery progression
   - **Integration Point**: `ICodexEntryRepository.GetByTitleAsync()`

4. **Environment System**
   - **Relationship**: Uses seeded conditions/hazards for room effects
   - **Integration Point**: `AmbientConditions`, `HazardTemplates` queries

---

### Related Systems

1. **Dependency Injection (Program.cs)**
   - **Relationship**: Seeders called within DI scope
   - **Integration Point**: `scope.ServiceProvider.GetRequiredService<>()`

2. **AAM-VOICE Validation**
   - **Relationship**: All seeded content must be Domain 4 compliant
   - **Integration Point**: Manual review during development

---

## Related Services

### Static Seeder Classes

1. **AbilitySeeder** (RuneAndRust.Persistence/Data/AbilitySeeder.cs)
   - Seeds 16 ActiveAbility records

2. **ConditionSeeder** (RuneAndRust.Persistence/Data/ConditionSeeder.cs)
   - Seeds 8 AmbientCondition records

3. **HazardTemplateSeeder** (RuneAndRust.Persistence/Data/HazardTemplateSeeder.cs)
   - Seeds 12 HazardTemplate records

4. **CodexSeeder** (RuneAndRust.Persistence/Data/CodexSeeder.cs)
   - Seeds 4 CodexEntry records

5. **RoomTemplateSeeder** (RuneAndRust.Persistence/Data/RoomTemplateSeeder.cs)
   - Orchestrates JSON template loading

### Template Loader Service

6. **ITemplateLoaderService / TemplateLoaderService** (RuneAndRust.Engine/Services/)
   - Loads room templates and biome definitions from JSON files

---

## Data Models

### Seeded Entities

| Entity | Table | Records | Source |
|--------|-------|---------|--------|
| ActiveAbility | ActiveAbilities | 16 | AbilitySeeder.cs |
| AmbientCondition | AmbientConditions | 8 | ConditionSeeder.cs |
| HazardTemplate | HazardTemplates | 13 | HazardTemplateSeeder.cs |
| CodexEntry | CodexEntries | 4 | CodexSeeder.cs |
| RoomTemplate | RoomTemplates | 20 | /data/templates/*.json |
| BiomeDefinition | BiomeDefinitions | 1 | /data/biomes/the_roots.json |
| BiomeElement | BiomeElements | 27 | Embedded in biome JSON |

### JSON File Inventory

**Room Templates** (`/data/templates/` - 20 files):
- operations_nexus.json
- collapsed_entry_hall.json
- maintenance_access.json
- loading_dock.json
- rust_choked_corridor.json
- pipe_gallery.json
- data_spine.json
- maintenance_tunnel.json
- geothermal_passage.json
- observation_walkway.json
- salvage_bay.json
- pump_station.json
- research_lab.json
- training_hall.json
- power_substation.json
- transit_hub.json
- vault_chamber.json
- reactor_core.json
- hidden_cache.json
- maintenance_crawlspace.json

**Biome Definitions** (`/data/biomes/` - 1 file):
- the_roots.json (includes 27 spawn elements)

**Dialogues** (`/data/dialogues/` - 8 files):
- astrid_dialogues.json
- bjorn_dialogues.json
- eydis_dialogues.json
- gunnar_dialogues.json
- kjartan_dialogues.json
- rolf_dialogues.json
- sigrun_dialogues.json
- thorvald_dialogues.json

---

## Configuration

### Build Configuration

**Project File** (`.csproj`):
```xml
<ItemGroup>
  <None Update="data/**/*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

**Purpose**: Copies `/data/` directory to output (`bin/`) during build.

### DI Registration

**Program.cs**:
```csharp
services.AddScoped<ITemplateLoaderService, TemplateLoaderService>();
```

### File Paths

**Base Directory Resolution**:
```csharp
var baseDir = AppDomain.CurrentDomain.BaseDirectory;
var templatesPath = Path.Combine(baseDir, "data", "templates");
var biomesPath = Path.Combine(baseDir, "data", "biomes");
```

---

## Testing

### Unit Testing Strategy

**Test Coverage Target**: 60% (seeding is largely data definition)

**Testable Components**:
1. **Idempotency Logic**: Verify seeders skip when data exists
2. **JSON Parsing**: Verify template deserialization
3. **Error Handling**: Verify malformed JSON doesn't crash seeder

### Example Test: AbilitySeeder Idempotency

**File**: RuneAndRust.Tests/Persistence/AbilitySeederTests.cs

```csharp
public class AbilitySeederTests : IDisposable
{
    private readonly RuneAndRustDbContext _context;

    public AbilitySeederTests()
    {
        var options = new DbContextOptionsBuilder<RuneAndRustDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new RuneAndRustDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_EmptyDatabase_SeedsAbilities()
    {
        // Act
        await AbilitySeeder.SeedAsync(_context);

        // Assert
        var count = await _context.ActiveAbilities.CountAsync();
        Assert.Equal(16, count);
    }

    [Fact]
    public async Task SeedAsync_DataExists_SkipsSeeding()
    {
        // Arrange - seed once
        await AbilitySeeder.SeedAsync(_context);
        var initialCount = await _context.ActiveAbilities.CountAsync();

        // Act - attempt to seed again
        await AbilitySeeder.SeedAsync(_context);

        // Assert - count unchanged
        var finalCount = await _context.ActiveAbilities.CountAsync();
        Assert.Equal(initialCount, finalCount);
    }

    [Fact]
    public async Task SeedAsync_AllAbilitiesHaveRequiredFields()
    {
        // Act
        await AbilitySeeder.SeedAsync(_context);

        // Assert
        var abilities = await _context.ActiveAbilities.ToListAsync();
        Assert.All(abilities, a =>
        {
            Assert.NotEqual(Guid.Empty, a.Id);
            Assert.NotNull(a.Name);
            Assert.NotNull(a.Description);
            Assert.True(a.StaminaCost >= 0);
        });
    }

    public void Dispose() => _context.Dispose();
}
```

### Example Test: TemplateLoaderService

```csharp
public class TemplateLoaderServiceTests
{
    [Fact]
    public async Task LoadRoomTemplatesFromDirectoryAsync_ValidJson_LoadsTemplate()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var template = new { TemplateId = "test_room", Biome = "test_biome" };
        var json = JsonSerializer.Serialize(template);
        await File.WriteAllTextAsync(Path.Combine(tempDir, "test.json"), json);

        var context = CreateInMemoryContext();
        var logger = Substitute.For<ILogger<TemplateLoaderService>>();
        var service = new TemplateLoaderService(context, logger);

        // Act
        await service.LoadRoomTemplatesFromDirectoryAsync(tempDir);

        // Assert
        var loaded = await context.RoomTemplates.FirstOrDefaultAsync();
        Assert.NotNull(loaded);
        Assert.Equal("test_room", loaded.TemplateId);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task LoadRoomTemplatesFromDirectoryAsync_InvalidJson_ContinuesWithOtherFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        // Write invalid JSON
        await File.WriteAllTextAsync(Path.Combine(tempDir, "invalid.json"), "{ invalid json }");

        // Write valid JSON
        var validTemplate = new { TemplateId = "valid_room" };
        await File.WriteAllTextAsync(
            Path.Combine(tempDir, "valid.json"),
            JsonSerializer.Serialize(validTemplate));

        var context = CreateInMemoryContext();
        var logger = Substitute.For<ILogger<TemplateLoaderService>>();
        var service = new TemplateLoaderService(context, logger);

        // Act
        await service.LoadRoomTemplatesFromDirectoryAsync(tempDir);

        // Assert - valid template was still loaded
        var loaded = await context.RoomTemplates.FirstOrDefaultAsync();
        Assert.NotNull(loaded);
        Assert.Equal("valid_room", loaded.TemplateId);

        // Cleanup
        Directory.Delete(tempDir, true);
    }
}
```

---

## Design Rationale

### DR-1: Why Hybrid Seeding (C# + JSON)?

**Decision**: Use C# static classes for gameplay data, JSON files for templates.

**Alternatives Considered**:
1. **All C#**: Embed all seed data in code
2. **All JSON**: Store all seed data in JSON files
3. **Database Seeding**: Use EF Core HasData() in OnModelCreating

**Rationale for Hybrid**:
- **C# for Gameplay Data**: Abilities, conditions, hazards require Domain 4 review; easier to validate during compilation
- **JSON for Templates**: Room templates may be edited by game designers; no recompilation needed
- **Separation of Concerns**: Static data vs. configurable content

**Trade-Offs**:
- **Complexity**: Two seeding mechanisms to maintain
- **Consistency**: Must ensure both use idempotent patterns

---

### DR-2: Why Static Seeder Classes?

**Decision**: Use static classes with static async methods for seeders.

**Alternatives Considered**:
1. **DI Services**: Register seeders as services with constructor injection
2. **EF Core HasData()**: Use fluent API for seed data
3. **SQL Scripts**: Raw SQL INSERT statements

**Rationale for Static Classes**:
- **Simplicity**: No DI registration needed, just call method
- **Explicit Ordering**: Clear execution order in Program.cs
- **Testability**: Can instantiate DbContext in tests without full DI

**Trade-Offs**:
- **Logger Injection**: Must pass logger as optional parameter
- **DbContext Coupling**: Directly depends on concrete DbContext type

---

### DR-3: Why Idempotent Pattern?

**Decision**: Check `AnyAsync()` before seeding to prevent duplicates.

**Alternatives Considered**:
1. **Always Clear + Reseed**: Delete all, insert fresh
2. **Upsert Only**: Check each entity individually
3. **Migration-Based Seeding**: Seed in migrations (one-time)

**Rationale for Idempotent**:
- **Safe Restarts**: Application can restart without data corruption
- **Development Friendly**: Drop database, restart, data reappears
- **Production Safe**: Won't duplicate data in production environment

**Trade-Offs**:
- **No Updates**: Can't update existing seed data without clearing table
- **All-or-Nothing**: If first record exists, nothing seeded

---

## Changelog

### Version 1.0.0 (2025-12-22) - Initial Specification

**Added**:
- Comprehensive seeding system documentation
- 5 static seeder class specifications (Ability, Condition, Hazard, Codex, RoomTemplate)
- TemplateLoaderService JSON loading documentation
- Idempotent seeding pattern specification
- Domain 4 compliance requirements for seed content
- 6 detailed use cases (fresh init, restart, new template, malformed JSON, spawn config, codex discovery)
- 3 decision trees (execution flow, JSON loading, content validation)
- Testing strategy with example tests
- Design rationale (hybrid approach, static classes, idempotent pattern)

**Documented Data**:
- 16 ActiveAbility records
- 8 AmbientCondition records
- 12 HazardTemplate records
- 4 CodexEntry records
- 20 RoomTemplate JSON files
- 1 BiomeDefinition + 28 BiomeElement records

---

## Future Enhancements

### FE-1: Per-Entity Upsert for Static Seeders

**Problem**: Static seeders skip entirely if any data exists.

**Proposed Solution**:
```csharp
public static async Task SeedAsync(RuneAndRustDbContext context)
{
    var abilities = GetAbilitiesToSeed();

    foreach (var ability in abilities)
    {
        var existing = await context.ActiveAbilities
            .FirstOrDefaultAsync(a => a.Name == ability.Name);

        if (existing == null)
        {
            await context.ActiveAbilities.AddAsync(ability);
        }
        // Optionally: else update existing
    }

    await context.SaveChangesAsync();
}
```

---

### FE-2: Seed Version Tracking

**Problem**: No way to know if new seed data should be applied.

**Proposed Solution**:
```csharp
public class SeedVersion
{
    public string SeederName { get; set; }
    public int Version { get; set; }
    public DateTime AppliedAt { get; set; }
}

// In seeder:
if (await context.SeedVersions.AnyAsync(v => v.SeederName == "AbilitySeeder" && v.Version >= 2))
    return;

// Seed new data...

await context.SeedVersions.AddAsync(new SeedVersion
{
    SeederName = "AbilitySeeder",
    Version = 2,
    AppliedAt = DateTime.UtcNow
});
```

---

### FE-3: JSON Schema Validation

**Problem**: Malformed JSON detected only at deserialization time.

**Proposed Solution**:
```csharp
public async Task<bool> ValidateTemplateJsonAsync(string filePath)
{
    var schema = await LoadSchema("room_template_schema.json");
    var json = await File.ReadAllTextAsync(filePath);

    var result = schema.Validate(json);
    if (!result.IsValid)
    {
        foreach (var error in result.Errors)
            _logger.LogError("Schema validation error: {Error}", error);
        return false;
    }
    return true;
}
```

---

### FE-4: Seed Data Export/Import

**Problem**: Hard to migrate seed data between environments.

**Proposed Solution**:
```csharp
public interface ISeedExporter
{
    Task ExportToJsonAsync(string outputPath);
    Task ImportFromJsonAsync(string inputPath);
}
```

---

## AAM-VOICE Compliance

### Layer Classification: **Layer 3 (Technical Specification)**

**Rationale**: This document is a system architecture specification for developers, not in-game content. Layer 3 applies to technical documentation written POST-Glitch with modern precision language.

### Domain 4 Compliance: **PARTIALLY APPLICABLE**

**For Specification Document**: NOT APPLICABLE (technical documentation)

**For Seeded Content**: STRICTLY ENFORCED
- All ability descriptions must be qualitative
- All condition/hazard descriptions must avoid measurements
- All codex text must use atmospheric language

**Enforcement**: Manual review during development; content must pass AAM-VOICE validation before merge.

### Voice Discipline: **Technical Authority**

**For Specification**: Precise technical language (counts, file paths, method signatures)

**For Seeded Content**: Domain 4 compliant narrative voice

---

## Changelog

### v1.0.1 (2025-12-24)
**Documentation Updates:**
- Added `last_updated` field to YAML frontmatter
- Fixed HazardTemplateSeeder count: 13 → 12 (Void has 3 templates, not 4; "Shadow Tendril" does not exist)
- Fixed BiomeElements count: 27 → 28
- Updated seeding order to reflect that CodexSeeder is NOT called in Program.cs initialization
- Added note that CodexSeeder exists but is optional/manual
- Added code traceability remarks (`See: SPEC-SEED-001...`) to all 5 seeders:
  - AbilitySeeder.cs
  - ConditionSeeder.cs
  - HazardTemplateSeeder.cs
  - CodexSeeder.cs (includes note about not being called in initialization)
  - RoomTemplateSeeder.cs

### v1.0.0 (2025-12-22)
**Initial Release:**
- Database seeding system documentation
- 5 seeders: AbilitySeeder, ConditionSeeder, HazardTemplateSeeder, CodexSeeder, RoomTemplateSeeder
- JSON data file structure for templates and biomes
- Domain 4 compliance guidelines for seeded content

---

**END OF SPECIFICATION**
