---
id: SPEC-TEMPLATE-001
title: Dynamic Room Template System
version: 1.0.1
status: Implemented
priority: P1
owner: Backend Team
related_specs: [SPEC-DUNGEON-001, SPEC-ENVPOP-001, SPEC-DESC-001, SPEC-SEED-001]
created: 2025-12-22
last_updated: 2025-12-24
---

# SPEC-TEMPLATE-001: Dynamic Room Template System

## 1. Overview

### 1.1 Purpose

The **Dynamic Room Template System** provides a data-driven approach to procedural room generation in Rune & Rust. It enables the creation of varied, thematically consistent dungeon environments through JSON-defined templates with token substitution, weighted spawn rules, and biome-based organization.

### 1.2 Scope

This specification covers:

- BiomeDefinition entity and biome-level configuration
- RoomTemplate entity and template-based room generation
- BiomeElement entity and spawnable element definitions
- TemplateLoaderService for JSON loading and database persistence
- TemplateRendererService for dynamic text generation
- JSON file structure and organization
- Token substitution patterns ({Adjective}, {Detail})
- Weighted spawn rules and conditional logic

### 1.3 Design Goals

| Goal | Description |
|------|-------------|
| **Data-Driven** | Room content defined in JSON, not code |
| **Variability** | Multiple name/description variations per template |
| **Thematic Consistency** | Biome-level descriptor pools ensure cohesion |
| **Extensibility** | New templates/biomes added without code changes |
| **Domain 4 Compliance** | All template content avoids precision measurements |

### 1.4 Key Terminology

| Term | Definition |
|------|------------|
| **Biome** | A thematic dungeon region with shared descriptor pools |
| **Template** | A reusable room archetype with variable text |
| **Token** | A placeholder like `{Adjective}` replaced at runtime |
| **Element** | A spawnable entity (enemy, hazard, loot, condition) |
| **Spawn Rules** | Conditional constraints on element placement |
| **Weight** | Probability factor for weighted random selection |

---

## 2. Core Concepts

### 2.1 Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        TEMPLATE SYSTEM                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                      JSON FILES (data/)                          │   │
│  │  ┌─────────────────┐    ┌──────────────────────────────────┐    │   │
│  │  │ templates/*.json│    │ biomes/the_roots.json           │    │   │
│  │  │ (20 room files) │    │ • BiomeDefinition                │    │   │
│  │  └────────┬────────┘    │ • DescriptorCategories           │    │   │
│  │           │             │ • BiomeElements (28)              │    │   │
│  │           │             └──────────────┬───────────────────┘    │   │
│  └───────────┼────────────────────────────┼────────────────────────┘   │
│              │                            │                             │
│              ▼                            ▼                             │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                   TemplateLoaderService                          │   │
│  │  • LoadAllTemplatesAsync()                                       │   │
│  │  • LoadRoomTemplatesFromDirectoryAsync()                         │   │
│  │  • LoadBiomeDefinitionAsync()                                    │   │
│  │  • ValidateTemplateReferencesAsync()                             │   │
│  └───────────────────────────┬─────────────────────────────────────┘   │
│                              │                                          │
│                              ▼                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                     PostgreSQL Database                          │   │
│  │  ┌────────────────┐ ┌───────────────┐ ┌──────────────────┐      │   │
│  │  │ RoomTemplates  │ │BiomeDefinitions│ │ BiomeElements    │      │   │
│  │  │ (20 rows)      │ │ (1 row)        │ │ (28 rows)        │      │   │
│  │  └────────────────┘ └───────────────┘ └──────────────────┘      │   │
│  └───────────────────────────┬─────────────────────────────────────┘   │
│                              │                                          │
│                              ▼                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                  TemplateRendererService                         │   │
│  │  • RenderRoomName(template) → "The Unstable Reactor Core"       │   │
│  │  • RenderRoomDescription(template, biome) → Rendered text       │   │
│  │  • Token substitution: {Adjective}, {Detail}                     │   │
│  │  • Atmospheric details (30% chance): sounds, smells              │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Entity Relationships

```
BiomeDefinition (1)
    │
    ├── AvailableTemplates (List<string>) ──→ RoomTemplate.TemplateId
    │
    ├── DescriptorCategories (nested object)
    │   ├── Adjectives
    │   ├── Details
    │   ├── Sounds
    │   └── Smells
    │
    └── BiomeElements (1:many)
        ├── DormantProcess (enemies)
        ├── DynamicHazard (hazards)
        ├── StaticTerrain (terrain features)
        ├── LootNode (containers)
        └── AmbientCondition (conditions)
```

---

## 3. Data Models

### 3.1 BiomeDefinition Entity

**Location:** `RuneAndRust.Core/Entities/BiomeDefinition.cs`

```csharp
public class BiomeDefinition
{
    public Guid Id { get; set; }
    public string BiomeId { get; set; }           // "the_roots"
    public string Name { get; set; }               // "[The Roots]"
    public string Description { get; set; }        // Narrative description
    public List<string> AvailableTemplates { get; set; }
    public BiomeDescriptorCategories DescriptorCategories { get; set; }
    public int MinRoomCount { get; set; }          // 5
    public int MaxRoomCount { get; set; }          // 7
    public float BranchingProbability { get; set; } // 0.4
    public float SecretRoomProbability { get; set; } // 0.2
}

public class BiomeDescriptorCategories
{
    public List<string> Adjectives { get; set; }   // ["Corroded", "Decaying"]
    public List<string> Details { get; set; }      // Detail sentences
    public List<string> Sounds { get; set; }       // Atmospheric sounds
    public List<string> Smells { get; set; }       // Atmospheric smells
}
```

**JSONB Columns:**
- `AvailableTemplates` - List of template IDs
- `DescriptorCategories` - Nested descriptor pools

### 3.2 RoomTemplate Entity

**Location:** `RuneAndRust.Core/Entities/RoomTemplate.cs`

```csharp
public class RoomTemplate
{
    public Guid Id { get; set; }
    public string TemplateId { get; set; }         // "reactor_core"
    public string BiomeId { get; set; }            // "the_roots"
    public string Size { get; set; }               // "Small", "Medium", "Large"
    public string Archetype { get; set; }          // "Corridor", "Chamber", etc.
    public List<string> NameTemplates { get; set; }
    public List<string> Adjectives { get; set; }
    public List<string> DescriptionTemplates { get; set; }
    public List<string> Details { get; set; }
    public List<string> ValidConnections { get; set; }
    public List<string> Tags { get; set; }
    public int MinConnectionPoints { get; set; }
    public int MaxConnectionPoints { get; set; }
    public string Difficulty { get; set; }         // "Easy", "Medium", "Hard", "VeryHard"
}
```

**Archetype Values:**
| Archetype | Description |
|-----------|-------------|
| `EntryHall` | Starting area, always accessible |
| `Corridor` | Connecting passage between rooms |
| `Chamber` | Standard encounter room |
| `Junction` | Multi-exit hub room |
| `BossArena` | Boss encounter space |
| `SecretRoom` | Hidden optional room |

### 3.3 BiomeElement Entity

**Location:** `RuneAndRust.Core/Entities/BiomeElement.cs`

```csharp
public class BiomeElement
{
    public Guid Id { get; set; }
    public string BiomeId { get; set; }            // FK to BiomeDefinition
    public string ElementName { get; set; }        // "Rust-Horror"
    public string ElementType { get; set; }        // Category
    public float Weight { get; set; }              // 0.0-1.0
    public int SpawnCost { get; set; }             // Budget cost
    public string AssociatedDataId { get; set; }   // Reference ID
    public ElementSpawnRules SpawnRules { get; set; }
}

public class ElementSpawnRules
{
    public bool? NeverInEntryHall { get; set; }
    public bool? NeverInBossArena { get; set; }
    public bool? OnlyInLargeRooms { get; set; }
    public string? RequiredArchetype { get; set; }
    public List<string>? RequiresRoomNameContains { get; set; }
    public bool? HigherWeightInSecretRooms { get; set; }
    public float? SecretRoomWeightMultiplier { get; set; }
    public string? RequiresEnemyType { get; set; }
    public string? RequiresHazardType { get; set; }
}
```

**Element Types:**
| Type | Description | Example |
|------|-------------|---------|
| `DormantProcess` | Enemy entities | Rust-Horror, Iron-Wraith |
| `DynamicHazard` | Environmental dangers | Steam Vent, Collapsing Floor |
| `StaticTerrain` | Passive features | Rubble, Destroyed Workbench |
| `LootNode` | Container objects | Hidden Container, Salvage Pile |
| `AmbientCondition` | Area effects | Rust Spore Cloud, Steam Vents |

---

## 4. Behaviors

### 4.1 Template Loading

**TemplateLoaderService.LoadAllTemplatesAsync()**

```
┌─────────────────────────────────────────────────────────────────────┐
│                     Template Loading Flow                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  1. Construct Paths                                                  │
│     ├── templates: {BaseDirectory}/data/templates/                   │
│     └── biomes: {BaseDirectory}/data/biomes/                         │
│                                                                      │
│  2. Load Room Templates                                              │
│     ├── Scan templates/ for *.json                                   │
│     ├── For each file:                                               │
│     │   ├── Deserialize JSON → RoomTemplate                          │
│     │   ├── Validate TemplateId not empty                            │
│     │   └── Upsert to database (by TemplateId)                       │
│     └── Log loaded/updated counts                                    │
│                                                                      │
│  3. Load Biome Definition                                            │
│     ├── Read biomes/the_roots.json                                   │
│     ├── Deserialize JSON → BiomeDefinitionDto                        │
│     ├── Convert DTO → BiomeDefinition entity                         │
│     ├── Upsert to database (by BiomeId)                              │
│     └── Load BiomeElements (nested)                                  │
│                                                                      │
│  4. Validate Template References                                     │
│     ├── For each BiomeDefinition.AvailableTemplates                  │
│     │   └── Check RoomTemplate exists                                │
│     └── Log warnings for missing templates                           │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### 4.2 Token Substitution

**RenderRoomName(template)**

```
Input Template: "The {Adjective} Reactor Core"
Adjectives: ["Pulsing", "Unstable", "Critical"]

Process:
1. Select random NameTemplate → "The {Adjective} Reactor Core"
2. Select random Adjective → "Unstable"
3. Replace {Adjective} → "The Unstable Reactor Core"

Output: "The Unstable Reactor Core"
```

**RenderRoomDescription(template, biome)**

```
Input Template: "A {Adjective} reactor core dominates this space. {Detail}."
Adjectives (lowercase): ["pulsing", "unstable", "critical"]
Details: ["The reactor core pulses with blinding runic light."]

Process:
1. Select random DescriptionTemplate
2. Select random Adjective (lowercase for mid-sentence)
3. Replace {Adjective} → "A pulsing reactor core dominates this space. {Detail}."
4. Select random Detail
5. Replace {Detail} → "A pulsing reactor core dominates this space. The reactor core pulses with blinding runic light."
6. 30% chance: Append atmospheric detail from biome.DescriptorCategories

Output: "A pulsing reactor core dominates this space. The reactor core pulses with blinding runic light. You hear electrical arcing from exposed conduits."
```

### 4.3 Spawn Rule Evaluation

```csharp
bool CanSpawn(BiomeElement element, Room room)
{
    var rules = element.SpawnRules;

    if (rules.NeverInEntryHall == true && room.Archetype == "EntryHall")
        return false;

    if (rules.NeverInBossArena == true && room.Archetype == "BossArena")
        return false;

    if (rules.OnlyInLargeRooms == true && room.Size != "Large")
        return false;

    if (rules.RequiredArchetype != null && room.Archetype != rules.RequiredArchetype)
        return false;

    if (rules.RequiresRoomNameContains?.Any() == true)
    {
        if (!rules.RequiresRoomNameContains.Any(keyword =>
            room.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            return false;
    }

    return true;
}
```

---

## 5. JSON File Structure

### 5.1 Room Template JSON

**Location:** `data/templates/{template_id}.json`

```json
{
  "TemplateId": "reactor_core",
  "Biome": "the_roots",
  "Size": "Large",
  "Archetype": "BossArena",
  "NameTemplates": [
    "The {Adjective} Reactor Core",
    "The {Adjective} Power Nexus",
    "The {Adjective} Energy Heart"
  ],
  "Adjectives": [
    "Pulsing",
    "Unstable",
    "Critical",
    "Overloaded",
    "Corrupted"
  ],
  "DescriptionTemplates": [
    "A {Adjective} reactor core dominates this space. {Detail}.",
    "This chamber houses the {Adjective} remains of a massive reactor. {Detail}."
  ],
  "Details": [
    "The reactor core pulses with blinding runic light.",
    "Energy conduits snake across every surface.",
    "Warning runes flicker ominously across control panels."
  ],
  "MinConnectionPoints": 1,
  "MaxConnectionPoints": 2,
  "ValidConnections": ["Corridor", "Chamber"],
  "Difficulty": "Hard",
  "Tags": ["Boss", "Combat", "Hazard", "Power", "Climactic"]
}
```

### 5.2 Biome Definition JSON

**Location:** `data/biomes/{biome_id}.json`

```json
{
  "BiomeId": "the_roots",
  "Name": "[The Roots]",
  "Description": "The deepest level of the facility, where power conduits and maintenance tunnels interweave. Steam vents, Blight-corrupted machinery, and forgotten horrors await.",
  "AvailableTemplates": [
    "collapsed_entry_hall",
    "maintenance_access",
    "reactor_core"
  ],
  "DescriptorCategories": {
    "Adjectives": ["Corroded", "Decaying", "Twisted", "Forgotten"],
    "Details": ["Ancient pipes groan overhead.", "Rust flakes drift in the stale air."],
    "Sounds": ["hissing steam escaping from fractured pipes", "distant machinery grinding"],
    "Smells": ["ozone from arcing power conduits", "rust and decay"]
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
          "NeverInEntryHall": true,
          "NeverInBossArena": true
        }
      },
      {
        "ElementName": "Steam Vent",
        "ElementType": "DynamicHazard",
        "Weight": 0.30,
        "SpawnCost": 0,
        "AssociatedDataId": "steam_vent",
        "SpawnRules": {
          "RequiresRoomNameContains": ["Geothermal", "Pumping", "Maintenance"]
        }
      }
    ]
  }
}
```

---

## 6. Use Cases

### 6.1 Use Case: Fresh Database Initialization

**Scenario:** First application startup with empty database.

**Flow:**
1. RoomTemplateSeeder.SeedAsync() detects empty RoomTemplates table
2. Calls TemplateLoaderService.LoadAllTemplatesAsync()
3. Loads 20 room templates from `data/templates/`
4. Loads 1 biome definition from `data/biomes/`
5. Loads 28 biome elements from biome JSON
6. Validates template references
7. Database now contains full template data

### 6.2 Use Case: Adding a New Room Template

**Scenario:** Designer creates `armory.json` for a new room type.

**Steps:**
1. Create `data/templates/armory.json` with template structure
2. Add `"armory"` to biome's `AvailableTemplates` array
3. Restart application (or trigger reload)
4. TemplateLoaderService inserts new template
5. DungeonGenerator can now select armory template

### 6.3 Use Case: Rendering Room During Generation

**Scenario:** DungeonGenerator creates a new room from template.

**Flow:**
1. Generator selects `reactor_core` template based on archetype requirements
2. Calls `RenderRoomName(template)` → "The Unstable Reactor Core"
3. Calls `RenderRoomDescription(template, biome)` → Full description with details
4. Creates Room entity with rendered name and description
5. Room appears unique despite using same template

### 6.4 Use Case: Conditional Element Spawning

**Scenario:** Toxic Spore Cloud should only spawn if Rust-Horror is present.

**Configuration:**
```json
{
  "ElementName": "Toxic Spore Cloud",
  "ElementType": "DynamicHazard",
  "SpawnRules": {
    "RequiresEnemyType": "RustHorror"
  }
}
```

**Evaluation:**
1. EnvironmentPopulator processes room
2. Checks if RustHorror enemy was spawned
3. If present, Toxic Spore Cloud eligible for spawn
4. If absent, element skipped

### 6.5 Use Case: Secret Room Weight Boost

**Scenario:** Hidden Container more likely in secret rooms.

**Configuration:**
```json
{
  "ElementName": "Hidden Container",
  "ElementType": "LootNode",
  "Weight": 0.05,
  "SpawnRules": {
    "HigherWeightInSecretRooms": true,
    "SecretRoomWeightMultiplier": 4.0
  }
}
```

**Evaluation:**
- Normal room: Weight = 0.05 (5%)
- Secret room: Weight = 0.05 × 4.0 = 0.20 (20%)

### 6.6 Use Case: Template Validation Warning

**Scenario:** Biome references template that doesn't exist.

**Flow:**
1. TemplateLoaderService loads biome with `"missing_template"` in AvailableTemplates
2. ValidateTemplateReferencesAsync() checks database
3. No matching RoomTemplate found
4. Logs warning: "Template 'missing_template' referenced by biome 'the_roots' not found"
5. Biome still loads (non-fatal warning)

---

## 7. Decision Trees

### 7.1 Template Selection Flow

```
                    ┌─────────────────────────┐
                    │ DungeonGenerator needs  │
                    │ room for position       │
                    └───────────┬─────────────┘
                                │
                                ▼
                    ┌─────────────────────────┐
                    │ Query BiomeDefinition   │
                    │ for current biome       │
                    └───────────┬─────────────┘
                                │
                                ▼
                    ┌─────────────────────────┐
                    │ Get AvailableTemplates  │
                    │ list from biome         │
                    └───────────┬─────────────┘
                                │
                                ▼
                    ┌─────────────────────────┐
                    │ Required archetype?     │
                    └───────────┬─────────────┘
                                │
           ┌────────────────────┼────────────────────┐
           │ YES                │ NO                 │
           ▼                    ▼                    │
    ┌──────────────┐    ┌──────────────┐            │
    │ Filter by    │    │ Filter by    │            │
    │ Archetype    │    │ ValidConnect │            │
    │ (BossArena)  │    │ (from prev)  │            │
    └──────┬───────┘    └──────┬───────┘            │
           │                   │                     │
           └─────────┬─────────┘                     │
                     │                               │
                     ▼                               │
           ┌─────────────────────┐                  │
           │ Weighted random     │◄─────────────────┘
           │ selection from      │
           │ filtered templates  │
           └─────────┬───────────┘
                     │
                     ▼
           ┌─────────────────────┐
           │ Selected template   │
           │ (e.g., reactor_core)│
           └─────────────────────┘
```

### 7.2 Element Spawn Evaluation

```
                    ┌─────────────────────────┐
                    │ Evaluate BiomeElement   │
                    │ for room                │
                    └───────────┬─────────────┘
                                │
                                ▼
                    ┌─────────────────────────┐
                    │ NeverInEntryHall?       │
                    └───────────┬─────────────┘
                                │
           ┌────────────────────┴────────────────────┐
           │ YES && IsEntryHall                      │ NO
           ▼                                         ▼
    ┌──────────────┐                        ┌──────────────┐
    │ SKIP Element │                        │ NeverInBoss? │
    └──────────────┘                        └──────┬───────┘
                                                   │
                              ┌─────────────────────┴─────────────────────┐
                              │ YES && IsBossArena                        │ NO
                              ▼                                           ▼
                       ┌──────────────┐                          ┌──────────────┐
                       │ SKIP Element │                          │ OnlyInLarge? │
                       └──────────────┘                          └──────┬───────┘
                                                                        │
                                            ┌───────────────────────────┴───────┐
                                            │ YES && !IsLarge                   │ NO
                                            ▼                                   ▼
                                     ┌──────────────┐                  ┌──────────────┐
                                     │ SKIP Element │                  │ RequiredArch?│
                                     └──────────────┘                  └──────┬───────┘
                                                                              │
                                            ┌─────────────────────────────────┴─────────┐
                                            │ YES && !MatchesArchetype                  │ NO
                                            ▼                                           ▼
                                     ┌──────────────┐                          ┌──────────────┐
                                     │ SKIP Element │                          │ ELIGIBLE for │
                                     └──────────────┘                          │ weighted pick│
                                                                               └──────────────┘
```

### 7.3 Token Rendering Flow

```
                    ┌─────────────────────────┐
                    │ RenderRoomDescription() │
                    └───────────┬─────────────┘
                                │
                                ▼
                    ┌─────────────────────────┐
                    │ DescriptionTemplates    │
                    │ empty?                  │
                    └───────────┬─────────────┘
                                │
           ┌────────────────────┴────────────────────┐
           │ YES                                     │ NO
           ▼                                         ▼
    ┌──────────────────┐                    ┌──────────────────┐
    │ Return fallback: │                    │ Select random    │
    │ "This area is    │                    │ DescriptionTemp  │
    │ shrouded..."     │                    └────────┬─────────┘
    └──────────────────┘                             │
                                                     ▼
                                            ┌──────────────────┐
                                            │ Contains         │
                                            │ {Adjective}?     │
                                            └────────┬─────────┘
                                                     │
                               ┌─────────────────────┴─────────────────────┐
                               │ YES                                       │ NO
                               ▼                                           │
                      ┌──────────────────┐                                │
                      │ Select random    │                                │
                      │ Adjective        │                                │
                      │ (lowercase)      │                                │
                      └────────┬─────────┘                                │
                               │                                          │
                               ▼                                          │
                      ┌──────────────────┐                                │
                      │ Replace          │                                │
                      │ {Adjective}      │                                │
                      └────────┬─────────┘                                │
                               │◄─────────────────────────────────────────┘
                               ▼
                      ┌──────────────────┐
                      │ Contains         │
                      │ {Detail}?        │
                      └────────┬─────────┘
                               │
              ┌────────────────┴────────────────┐
              │ YES                             │ NO
              ▼                                 │
     ┌──────────────────┐                      │
     │ Select random    │                      │
     │ Detail           │                      │
     │ Replace {Detail} │                      │
     └────────┬─────────┘                      │
              │◄───────────────────────────────┘
              ▼
     ┌──────────────────┐
     │ Roll: Add        │
     │ atmospheric?     │
     │ (30% chance)     │
     └────────┬─────────┘
              │
     ┌────────┴────────┐
     │ YES             │ NO
     ▼                 ▼
┌──────────────┐ ┌──────────────┐
│ Append sound │ │ Return text  │
│ or smell     │ │ as-is        │
└──────┬───────┘ └──────────────┘
       │
       ▼
┌──────────────┐
│ Return final │
│ description  │
└──────────────┘
```

---

## 8. Restrictions

### 8.1 Template Content

| Rule | Rationale |
|------|-----------|
| **No precision measurements** | Domain 4 compliance |
| **No modern technical terms** | Thematic consistency |
| **No meta-references** | Immersion preservation |
| **Adjectives must be lowercase-safe** | Mid-sentence substitution |

### 8.2 System Constraints

| Constraint | Description |
|------------|-------------|
| **TemplateId must be unique** | Upsert key for database |
| **BiomeId must be unique** | Upsert key for database |
| **AvailableTemplates must exist** | Validation warning if missing |
| **Empty NameTemplates** | Returns "Unnamed Room" fallback |
| **Empty DescriptionTemplates** | Returns generic fallback text |

### 8.3 Spawn Rule Limitations

| Rule | Behavior |
|------|----------|
| **Null SpawnRules** | No restrictions, always eligible |
| **All rules must pass** | AND logic (any failure = skip) |
| **SecretRoomWeightMultiplier without flag** | Multiplier ignored |
| **RequiresEnemyType** | Checked after enemy spawning phase |

---

## 9. Testing Strategy

### 9.1 Integration Tests

**TemplateLoadingIntegrationTests.cs:**

```csharp
[Fact]
public async Task LoadAllTemplatesAsync_LoadsAllTemplates()
{
    // Arrange
    var loader = new TemplateLoaderService(_context, _logger);

    // Act
    await loader.LoadAllTemplatesAsync();

    // Assert
    var templates = await _context.RoomTemplates.ToListAsync();
    Assert.Equal(20, templates.Count);

    var biome = await _context.BiomeDefinitions.FirstOrDefaultAsync();
    Assert.NotNull(biome);
    Assert.Equal("the_roots", biome.BiomeId);
}

[Fact]
public async Task LoadAllTemplatesAsync_PreservesJsonbArrays()
{
    // Verify JSONB arrays are not corrupted
    await loader.LoadAllTemplatesAsync();

    var template = await _context.RoomTemplates
        .FirstAsync(t => t.TemplateId == "reactor_core");

    Assert.NotEmpty(template.NameTemplates);
    Assert.Contains("Pulsing", template.Adjectives);
}
```

### 9.2 Unit Tests

**TemplateRendererServiceTests.cs:**

```csharp
[Fact]
public void RenderRoomName_SubstitutesAdjective()
{
    // Arrange
    var template = new RoomTemplate
    {
        NameTemplates = new() { "The {Adjective} Core" },
        Adjectives = new() { "Pulsing" }
    };

    // Act
    var name = _renderer.RenderRoomName(template);

    // Assert
    Assert.Equal("The Pulsing Core", name);
}

[Fact]
public void RenderRoomDescription_UsesLowercaseAdjective()
{
    // Arrange
    var template = new RoomTemplate
    {
        DescriptionTemplates = new() { "A {Adjective} chamber." },
        Adjectives = new() { "Corroded" },
        Details = new()
    };

    // Act
    var description = _renderer.RenderRoomDescription(template, _biome);

    // Assert
    Assert.Contains("corroded", description);
}
```

---

## 10. Configuration

### 10.1 File Paths

| Path | Content |
|------|---------|
| `data/templates/` | Room template JSON files (20) |
| `data/biomes/` | Biome definition JSON files (1) |

### 10.2 DI Registration

```csharp
// Program.cs
services.AddScoped<ITemplateLoaderService, TemplateLoaderService>();
services.AddScoped<ITemplateRendererService, TemplateRendererService>();
services.AddScoped<IRoomTemplateRepository, RoomTemplateRepository>();
services.AddScoped<IBiomeDefinitionRepository, BiomeDefinitionRepository>();
services.AddScoped<IBiomeElementRepository, BiomeElementRepository>();
```

---

## 11. Cross-References

| Specification | Relationship |
|---------------|--------------|
| [SPEC-DUNGEON-001](../exploration/SPEC-DUNGEON-001.md) | Consumes templates for room generation |
| [SPEC-ENVPOP-001](../exploration/SPEC-ENVPOP-001.md) | Consumes BiomeElements for spawning |
| [SPEC-DESC-001](./SPEC-DESC-001.md) | Related text generation patterns |
| [SPEC-SEED-001](../data/SPEC-SEED-001.md) | Template loading during seeding |

---

## 12. Design Rationale

### 12.1 Why JSON Templates?

| Alternative | Rejected Because |
|-------------|------------------|
| Hardcoded rooms | No designer control, requires code changes |
| Database-only | Harder to version control, no diff-friendly format |
| XML | More verbose, less readable |
| YAML | Extra dependency, JSON native to .NET |

### 12.2 Why Token Substitution?

| Alternative | Rejected Because |
|-------------|------------------|
| Static descriptions | Repetitive, breaks immersion |
| Full procedural generation | Inconsistent quality, unpredictable |
| Template per variation | Exponential file count |

**Token Benefits:**
- Controlled variability from curated pools
- Designer-authored quality
- Predictable structure with dynamic content

### 12.3 Why Weighted Spawn Rules?

| Alternative | Rejected Because |
|-------------|------------------|
| Fixed spawn locations | No procedural variety |
| Pure random | Thematically inconsistent placement |
| Complex scripting | Designer unfriendly, error-prone |

**Spawn Rule Benefits:**
- Declarative, data-driven constraints
- Optional rules (null = no restriction)
- AND logic for predictable evaluation

---

## 13. AAM-VOICE Compliance

### 13.1 Domain 4 Requirements

All template content must comply with Domain 4 technology constraints:

**Forbidden:**
- ❌ Precision measurements ("45 Hz", "200 meters", "35°C")
- ❌ Modern technical terms ("API", "Debug", "Glitch")
- ❌ Exact percentages ("95% chance", "89% efficiency")

**Allowed:**
- ✅ Qualitative descriptions ("a spear's throw", "oppressively hot")
- ✅ Archaic equivalents ("Anomaly", "Phenomenon", "Corruption")
- ✅ Epistemic uncertainty ("appears to", "suggests", "reportedly")

### 13.2 Content Validation

All template text should be reviewed for:
- Observer perspective (Jötun-Reader voice)
- Atmospheric consistency
- Thematic alignment with biome
- Absence of precision measurements

---

## Appendix A: Available Templates (v0.4.0)

| # | TemplateId | Archetype | Size | Difficulty |
|---|------------|-----------|------|------------|
| 1 | collapsed_entry_hall | EntryHall | Medium | Easy |
| 2 | maintenance_access | Corridor | Small | Easy |
| 3 | loading_dock | Chamber | Large | Easy |
| 4 | rust_choked_corridor | Corridor | Small | Medium |
| 5 | pipe_gallery | Corridor | Medium | Medium |
| 6 | data_spine | Corridor | Medium | Medium |
| 7 | maintenance_tunnel | Corridor | Small | Easy |
| 8 | geothermal_passage | Corridor | Medium | Medium |
| 9 | observation_walkway | Corridor | Medium | Medium |
| 10 | salvage_bay | Chamber | Medium | Medium |
| 11 | pump_station | Chamber | Medium | Medium |
| 12 | research_lab | Chamber | Medium | Hard |
| 13 | training_hall | Chamber | Large | Medium |
| 14 | power_substation | Chamber | Medium | Hard |
| 15 | operations_nexus | Junction | Large | Medium |
| 16 | transit_hub | Junction | Large | Medium |
| 17 | vault_chamber | SecretRoom | Small | Hard |
| 18 | reactor_core | BossArena | Large | Hard |
| 19 | hidden_cache | SecretRoom | Small | VeryHard |
| 20 | maintenance_crawlspace | SecretRoom | Small | Medium |

---

## Appendix B: BiomeElement Distribution (the_roots)

| Type | Count | Examples |
|------|-------|----------|
| DormantProcess | 7 | Rust-Horror, Iron-Wraith, Servitor Overseer |
| DynamicHazard | 6 | Steam Vent, Collapsing Floor, Energy Surge |
| StaticTerrain | 5 | Rubble, Destroyed Workbench, Leaking Pipe |
| LootNode | 5 | Hidden Container, Salvage Pile, Supply Cache |
| AmbientCondition | 5 | Rust Spore Cloud, Steam Vents, Electrical Storm |

**Total: 28 BiomeElements**

---

## Changelog

### v1.0.1 (2025-12-24)
**Documentation Update:**
- Renamed `updated` to `last_updated` in frontmatter for consistency
- Added code traceability remarks to services and interfaces

### v1.0.0 (2025-12-22)
**Initial Release:**
- Dynamic Room Template System with token substitution
- BiomeDefinition, RoomTemplate, BiomeElement entities
- TemplateRendererService for name/description rendering
- TemplateLoaderService for JSON template loading
- ElementSpawnRules for conditional element spawning
