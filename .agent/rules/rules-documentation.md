---
trigger: always_on
---

# Documentation Rules

## 1.1 XML Documentation
- **ALWAYS** add XML documentation comments to:
  - All public classes and interfaces
  - All public methods with non-obvious behavior
  - All public properties that need clarification
- Include `<summary>`, `<param>`, `<returns>`, and `<example>` tags as appropriate

```csharp
/// <summary>
/// Represents a data-driven ability definition loaded from configuration.
/// </summary>
/// <remarks>
/// AbilityDefinition is immutable and represents the template for abilities.
/// Actual ability state (cooldowns, usage) is tracked by PlayerAbility.
/// </remarks>
public class AbilityDefinition : IEntity
```

## 1.2 Implementation Plans
When planning new features:
- **ALWAYS** create a design specification document in `docs/v0.X.x/`
- **ALWAYS** include:
  - Executive Summary
  - Prerequisites & Dependencies
  - Architecture Overview with ASCII diagrams
  - Layer-by-layer implementation details with code snippets
  - Configuration file examples
  - Unit test specifications
  - Acceptance criteria
- Reference the template structure from existing plans (e.g., `v0.0.4c-implementation-plan.md`)

## 1.3 Changelogs
- Update changelogs following semantic versioning
- Group changes by: Added, Changed, Fixed, Removed