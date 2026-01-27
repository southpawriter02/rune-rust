# ADR-006: Definition Entity Pattern

**Status:** Accepted
**Date:** 2026-01-06
**Deciders:** Development Team

## Context

Rune and Rust uses JSON configuration files for game data (classes, abilities, monsters, etc.). This data needs to be:

- Loaded from JSON files at startup
- Validated before use in the game
- Immutable during gameplay (configuration shouldn't change mid-game)
- Type-safe with proper domain modeling
- Distinct from runtime entities (a MonsterDefinition vs. a Monster instance)

The challenge is supporting both JSON deserialization (which requires settable properties) and immutability (which suggests readonly properties).

## Decision

We will use a "Definition Entity" pattern with the following characteristics:

### Pattern Components

1. **Init-Only Properties**: Properties use `init` accessor for JSON deserialization
2. **Static Create() Factory**: Factory method validates and creates instances programmatically
3. **ID Normalization**: IDs converted to lowercase for consistent lookup
4. **Immutable After Creation**: No public setters, state fixed after construction

### Naming Convention

- Configuration entities end in "Definition": `ClassDefinition`, `AbilityDefinition`, `MonsterDefinition`
- Runtime entities are plain names: `Player`, `Monster`, `Room`
- Definition represents the "template", entity represents the "instance"

### Two Creation Paths

```
JSON Deserialization:           Programmatic Creation:
     JSON File                      Test/Code
        │                              │
        ▼                              ▼
  System.Text.Json              Create() Factory
        │                              │
        ▼                              │
   init setters ◄──────────────────────┘
        │
        ▼
  Definition Entity (immutable)
```

## Consequences

### Positive

- **Type Safety**: Strong typing throughout the domain
- **Validation**: Factory methods ensure valid state
- **Immutability**: Definitions cannot be accidentally modified
- **JSON Compatibility**: `init` properties support deserialization
- **Testability**: `Create()` methods allow easy test data creation
- **Clear Semantics**: "Definition" suffix distinguishes from instances

### Negative

- **Dual Path Complexity**: Two ways to create objects (JSON vs. factory)
- **Validation Duplication**: JSON path may bypass factory validation
- **Property Syntax**: `init` is less familiar than `set`

### Neutral

- Definitions are loaded once at startup
- Runtime entities reference definitions by ID
- Definitions are shared across all instances

## Implementation Details

### Definition Entity Structure

```csharp
/// <summary>
/// Defines a player class with stats, abilities, and resource type.
/// </summary>
public class ClassDefinition
{
    // Init properties for JSON deserialization
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string ArchetypeId { get; init; } = "";
    public StatModifiers StatModifiers { get; init; } = new();
    public GrowthRates GrowthRates { get; init; } = new();
    public string PrimaryResourceId { get; init; } = "";
    public IReadOnlyList<string> StartingAbilityIds { get; init; } = [];

    /// <summary>
    /// Creates a validated ClassDefinition.
    /// </summary>
    /// <exception cref="ArgumentException">When required fields are missing.</exception>
    public static ClassDefinition Create(
        string id,
        string name,
        string archetypeId,
        StatModifiers statModifiers,
        GrowthRates growthRates,
        string primaryResourceId,
        IEnumerable<string> startingAbilityIds,
        string? description = null)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID is required", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(archetypeId))
            throw new ArgumentException("ArchetypeId is required", nameof(archetypeId));

        return new ClassDefinition
        {
            Id = id.ToLowerInvariant(),  // Normalize ID
            Name = name,
            Description = description ?? "",
            ArchetypeId = archetypeId.ToLowerInvariant(),
            StatModifiers = statModifiers,
            GrowthRates = growthRates,
            PrimaryResourceId = primaryResourceId.ToLowerInvariant(),
            StartingAbilityIds = startingAbilityIds.Select(a => a.ToLowerInvariant()).ToList()
        };
    }
}
```

### Value Object for Modifiers

```csharp
/// <summary>
/// Stat modifiers applied by class selection.
/// </summary>
public readonly record struct StatModifiers(
    int Health = 0,
    int Attack = 0,
    int Defense = 0)
{
    public static StatModifiers None => new(0, 0, 0);

    public StatModifiers Add(StatModifiers other) => new(
        Health + other.Health,
        Attack + other.Attack,
        Defense + other.Defense);
}
```

### JSON Configuration Example

```json
{
  "classes": [
    {
      "id": "Berserker",
      "name": "Berserker",
      "description": "A fierce warrior who channels rage into devastating attacks.",
      "archetypeId": "Warrior",
      "statModifiers": {
        "health": 15,
        "attack": 5,
        "defense": 0
      },
      "growthRates": {
        "health": 6,
        "attack": 2,
        "defense": 1
      },
      "primaryResourceId": "Rage",
      "startingAbilityIds": ["rage-strike", "battle-cry"]
    }
  ]
}
```

### Configuration Provider Loading

```csharp
public class JsonConfigurationProvider : IConfigurationProvider
{
    private readonly Dictionary<string, ClassDefinition> _classes = new();

    public async Task LoadAsync()
    {
        var json = await File.ReadAllTextAsync("config/classes.json");
        var config = JsonSerializer.Deserialize<ClassesConfig>(json);

        foreach (var classDef in config.Classes)
        {
            // ID normalization happens via init property
            var normalizedId = classDef.Id.ToLowerInvariant();
            _classes[normalizedId] = classDef;
        }
    }

    public ClassDefinition GetClass(string id)
    {
        var normalizedId = id.ToLowerInvariant();
        return _classes.TryGetValue(normalizedId, out var def)
            ? def
            : throw new KeyNotFoundException($"Class not found: {id}");
    }
}
```

### Test Usage with Factory

```csharp
[Test]
public void Player_WithBerserkerClass_HasCorrectStatModifiers()
{
    // Arrange
    var berserker = ClassDefinition.Create(
        id: "berserker",
        name: "Berserker",
        archetypeId: "warrior",
        statModifiers: new StatModifiers(Health: 15, Attack: 5),
        growthRates: new GrowthRates(Health: 6, Attack: 2, Defense: 1),
        primaryResourceId: "rage",
        startingAbilityIds: ["rage-strike"]);

    var player = new PlayerBuilder()
        .WithClass(berserker)
        .Build();

    // Assert
    player.MaxHealth.Should().Be(100 + 15);  // Base + modifier
    player.Attack.Should().Be(10 + 5);       // Base + modifier
}
```

## Definition vs. Instance Relationship

```
ClassDefinition (template)         Player (instance)
┌─────────────────────────┐       ┌─────────────────────────┐
│ Id: "berserker"         │       │ Name: "Conan"           │
│ Name: "Berserker"       │──────▶│ ClassId: "berserker"    │
│ StatModifiers: +15 HP   │       │ MaxHealth: 115          │
│ Abilities: [rage-strike]│       │ CurrentHealth: 80       │
└─────────────────────────┘       │ Abilities: {rage-strike}│
                                  └─────────────────────────┘

MonsterDefinition (template)       Monster (instance)
┌─────────────────────────┐       ┌─────────────────────────┐
│ Id: "goblin"            │       │ Name: "Goblin Warrior"  │
│ Name: "Goblin"          │──────▶│ DefinitionId: "goblin"  │
│ BaseHealth: 30          │       │ CurrentHealth: 25       │
│ Tier: "common"          │       │ (scaled by tier)        │
└─────────────────────────┘       └─────────────────────────┘
```

## Alternatives Considered

### Alternative 1: Constructor-Only Initialization

Use constructors instead of init properties.

**Rejected because:**
- JSON deserialization requires parameterless constructor
- Large definitions have too many constructor parameters
- Less flexible for optional properties

### Alternative 2: Mutable Configuration Objects

Allow configuration to be modified after loading.

**Rejected because:**
- Risk of accidental modification during gameplay
- Harder to reason about state
- Thread safety concerns

### Alternative 3: Record Types

Use C# records for definitions.

**Partially adopted:**
- Value objects use `record struct`
- Complex definitions use class with `init` (better for inheritance, serialization)

### Alternative 4: Separate DTO and Domain Types

Full separation between JSON DTOs and domain definitions.

**Rejected because:**
- Additional mapping layer
- Duplication of properties
- Current approach is sufficient

## Related

- [ADR-002](ADR-002-json-configuration.md): JSON Configuration System
- [ADR-003](ADR-003-entity-framework.md): Entity Framework Core (persistence entities vs. definitions)
- [v0.0.4 Implementation Specification](../v0.0.x/implementation-specifications/v0.0.4-implementation-specification.md): Class definitions implementation
