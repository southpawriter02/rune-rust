---
trigger: always_on
---

# Entity & Value Object Rules

## 1.1 Entity Design
- **ALWAYS** implement `IEntity` interface for domain entities
- **ALWAYS** use `Guid Id` as the primary identifier
- **ALWAYS** provide a private parameterless constructor for EF Core with null-forgiving assignments
- **ALWAYS** use factory methods (e.g., `Create()`) for complex construction
- **NEVER** expose setters publicly; use domain methods for state changes

```csharp
public class AbilityDefinition : IEntity
{
    public Guid Id { get; private set; }
    public string AbilityId { get; private set; }
    
    // Private constructor for EF Core
    private AbilityDefinition()
    {
        AbilityId = null!;
    }
    
    // Factory method for creation
    public static AbilityDefinition Create(string abilityId, string name, ...)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId);
        return new AbilityDefinition { ... };
    }
}
```

## 1.2 Value Object Design
- Use `readonly record struct` for simple value objects
- Provide `static` factory methods or properties for common instances (e.g., `AbilityCost.None`)
- Override `ToString()` for debugging clarity

## 1.3 Immutability
- Definition entities (e.g., `AbilityDefinition`, `ClassDefinition`) **MUST** be immutable after creation
- State entities (e.g., `PlayerAbility`, `Player`) can have mutable state through domain methods