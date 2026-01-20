# ADR-002: JSON Configuration System

**Status:** Accepted
**Date:** 2026-01-06
**Deciders:** Development Team

## Context

Rune and Rust has extensive game data that defines the game experience:
- Class definitions (9 classes across 3 archetypes)
- Ability definitions (15+ abilities with effects and costs)
- Monster definitions (multiple types with tiers and traits)
- Equipment definitions (weapons, armor, accessories)
- Resource types (mana, rage, stamina, etc.)
- Progression tables (XP thresholds, stat gains)

This data needs to be:
- Easily modifiable without recompiling the application
- Human-readable for game designers and modders
- Validated at load time to prevent runtime errors
- Type-safe when used in code

## Decision

We will use JSON files for game configuration data with a three-stage loading pipeline:

### Configuration Pipeline

```
JSON File → DTO → Definition Entity
  (file)    (deserialization)   (validated, immutable)
```

### Components

1. **JSON Files** (`config/` directory)
   - Human-readable configuration files
   - One file per configuration type (classes.json, abilities.json, etc.)
   - Schema follows DTO structure

2. **Configuration DTOs** (`Application/Configuration/`)
   - Data transfer objects for JSON deserialization
   - Use `System.Text.Json` for deserialization
   - Nullable properties for optional fields

3. **Definition Entities** (`Domain/Definitions/`)
   - Immutable domain objects created from DTOs
   - `init` properties for JSON deserialization
   - `Create()` factory methods for programmatic creation with validation
   - IDs normalized to lowercase for consistent lookup

4. **Configuration Provider** (`IConfigurationProvider`)
   - Interface in Application layer
   - Implementation in Infrastructure layer (`JsonConfigurationProvider`)
   - Caches loaded configurations
   - Validates all definitions at startup

### File Structure

```
config/
├── archetypes.json      # 3 archetypes
├── classes.json         # 9 classes
├── resource-types.json  # 5 resource types
├── abilities.json       # 15+ abilities
├── monsters.json        # Monster definitions with tiers
├── equipment.json       # Weapons, armor, accessories
├── progression.json     # Level thresholds and stat gains
├── currency.json        # Currency definitions
└── loot-tables.json     # Loot drop configurations
```

## Consequences

### Positive

- **Data-Driven Design**: Game balance can be tuned without code changes
- **Modding Support**: Players can create custom content by editing JSON
- **Separation of Concerns**: Game data separate from game logic
- **Validation**: All data validated at load time with clear error messages
- **Type Safety**: Definition entities provide compile-time type checking
- **Testability**: Tests can use custom configuration without files

### Negative

- **Validation Code**: Each definition needs validation logic in factory methods
- **Mapping Overhead**: DTOs must be converted to definition entities
- **Startup Cost**: All configuration loaded and validated at startup
- **Schema Evolution**: Changes to JSON structure require migration

### Neutral

- Configuration changes require application restart
- Invalid configuration prevents application from starting
- Definition entities are immutable after creation

## Implementation Details

### Example: ClassDefinition

```csharp
// Configuration DTO (Application layer)
public record ClassConfigurationDto
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string ArchetypeId { get; init; } = "";
    public StatModifiersDto? StatModifiers { get; init; }
}

// Definition Entity (Domain layer)
public class ClassDefinition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string ArchetypeId { get; init; } = "";
    public StatModifiers StatModifiers { get; init; }

    public static ClassDefinition Create(
        string id,
        string name,
        string archetypeId,
        StatModifiers statModifiers)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID required", nameof(id));
        // Additional validation...

        return new ClassDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            ArchetypeId = archetypeId.ToLowerInvariant(),
            StatModifiers = statModifiers
        };
    }
}
```

### Example: classes.json

```json
{
  "classes": [
    {
      "id": "berserker",
      "name": "Berserker",
      "archetypeId": "warrior",
      "statModifiers": { "health": 15, "attack": 5, "defense": 0 },
      "primaryResourceId": "rage",
      "startingAbilityIds": ["rage-strike", "battle-cry"]
    }
  ]
}
```

## Alternatives Considered

### Alternative 1: Database Configuration

Store configuration in SQLite database.

**Rejected because:**
- Less human-readable for designers
- Requires database tooling for edits
- Overkill for read-only configuration data

### Alternative 2: XML Configuration

Use XML files instead of JSON.

**Rejected because:**
- More verbose than JSON
- JSON has better tooling support in .NET
- JSON is more familiar to web developers

### Alternative 3: YAML Configuration

Use YAML files for better readability.

**Rejected because:**
- No built-in .NET support (requires third-party library)
- JSON is sufficient for our data structures
- Consistent with ASP.NET Core conventions

### Alternative 4: Compiled Resources

Embed configuration as compiled resources.

**Rejected because:**
- Cannot be modified without recompilation
- No modding support
- Harder to version control separately

## Related

- [ADR-006](ADR-006-definition-pattern.md): Definition Entity Pattern
- [v0.0.4 Implementation Specification](../v0.0.x/implementation-specifications/v0.0.4-implementation-specification.md): Classes & Abilities implementation
