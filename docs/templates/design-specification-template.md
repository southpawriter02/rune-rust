# Design Specification Template

Use this template for creating design specification files (e.g., `v0.0.Xa-design-specification.md`) for sub-phases of a version.

---

# v0.0.Xa Design Specification: [Phase Name]

**Version:** 0.0.Xa
**Parent:** v0.0.X ([Version Theme])
**Prerequisites:** [e.g., "v0.0.5 Complete (Dice Pool System)"]
**Status:** [Design In Progress / Design Complete / Implementation In Progress / Complete]

---

## Table of Contents

1. [Overview](#overview)
2. [Scope](#scope)
3. [Data Model](#data-model)
4. [Services](#services)
5. [Command Changes](#command-changes)
6. [Rendering Changes](#rendering-changes)
7. [Configuration](#configuration)
8. [Acceptance Criteria](#acceptance-criteria)
9. [Test Specifications](#test-specifications)
10. [Dependencies](#dependencies)

---

## Overview

### Purpose

[2-3 sentences describing what this phase accomplishes and why it matters.]

### Key Changes

| Area | Current State (v0.0.Y) | Target State (v0.0.Xa) |
|------|------------------------|------------------------|
| [Area] | [Current behavior] | [Target behavior] |

### Design Principles

1. **[Principle Name]**: [Description]
2. **[Principle Name]**: [Description]
3. **[Principle Name]**: [Description]

---

## Scope

### In Scope

- [Specific feature or component]
- [Specific entity or service]
- [Specific command or interaction]

### Out of Scope (Future Phases)

- [Feature] (v0.0.Xb)
- [Feature] (v0.0.Xc)
- [Feature] (v0.0.Y)

---

## Data Model

### New: [EntityName] (Aggregate Root/Entity/Value Object)

```csharp
/// <summary>
/// [Description of the entity/value object.]
/// </summary>
/// <remarks>
/// [Additional context about usage, relationships, or design decisions.]
/// </remarks>
public class [EntityName]
{
    /// <summary>
    /// [Property description.]
    /// </summary>
    public [Type] [PropertyName] { get; private set; }

    /// <summary>
    /// [Property description.]
    /// </summary>
    public [Type] [PropertyName] { get; private set; }

    // Private constructor for EF Core
    private [EntityName]() { }

    /// <summary>
    /// Creates a new [EntityName].
    /// </summary>
    /// <param name="paramName">[Parameter description.]</param>
    /// <returns>A new [EntityName] instance.</returns>
    public static [EntityName] Create([params])
    {
        return new [EntityName]
        {
            // Initialize properties
        };
    }

    /// <summary>
    /// [Method description.]
    /// </summary>
    /// <param name="paramName">[Parameter description.]</param>
    /// <exception cref="InvalidOperationException">[When exception is thrown.]</exception>
    public void [MethodName]([params])
    {
        // Method implementation outline
    }
}
```

### New: [EnumName] (Enum)

```csharp
/// <summary>
/// [Description of the enum.]
/// </summary>
public enum [EnumName]
{
    /// <summary>[Value description.]</summary>
    [Value1],

    /// <summary>[Value description.]</summary>
    [Value2],

    /// <summary>[Value description.]</summary>
    [Value3]
}
```

### New: [ValueObjectName] (Value Object)

```csharp
/// <summary>
/// [Description of the value object.]
/// </summary>
/// <param name="Property1">[Description.]</param>
/// <param name="Property2">[Description.]</param>
public readonly record struct [ValueObjectName](
    [Type] Property1,
    [Type] Property2)
{
    /// <summary>
    /// [Computed property description.]
    /// </summary>
    public [Type] ComputedProperty => // calculation;

    /// <summary>
    /// Creates a display string for [context].
    /// </summary>
    public string ToDisplayString()
    {
        // Return formatted string
    }
}
```

### Modified: [ExistingEntityName] (Entity)

```csharp
// Add to existing [ExistingEntityName] entity:

/// <summary>
/// [Property description.]
/// </summary>
/// <remarks>
/// [Additional context.]
/// </remarks>
public [Type] [NewPropertyName] { get; private set; }

// Update existing method:
public [ReturnType] [ExistingMethodName]([params])
{
    // Updated implementation
}
```

---

## Services

### [ServiceName] Service

```csharp
/// <summary>
/// [Description of the service.]
/// </summary>
public class [ServiceName]
{
    private readonly [IDependency] _dependency;
    private readonly ILogger<[ServiceName]> _logger;

    public [ServiceName]([IDependency] dependency, ILogger<[ServiceName]> logger)
    {
        _dependency = dependency;
        _logger = logger;
    }

    /// <summary>
    /// [Method description.]
    /// </summary>
    /// <param name="paramName">[Parameter description.]</param>
    /// <returns>[Return description.]</returns>
    public [ReturnType] [MethodName]([params])
    {
        // Implementation outline:
        // 1. Step one
        // 2. Step two
        // 3. Return result
    }
}
```

### Service Flow Diagram *(optional)*

```
┌─────────────────────────────────────────────────────────────────┐
│                        [FLOW TITLE]                              │
└─────────────────────────────────────────────────────────────────┘

    [Entry point]
           │
           ▼
┌─────────────────────────────────────────────────────────────────┐
│                         STEP 1                                   │
├─────────────────────────────────────────────────────────────────┤
│  [Description of what happens]                                  │
└─────────────────────────────────────────────────────────────────┘
           │
           ▼
    [Continue flow...]
```

---

## Command Changes

### Updated [CommandName]

```csharp
/// <summary>
/// [Command description.]
/// </summary>
/// <param name="Parameter">[Parameter description.]</param>
public record [CommandName]([Type]? Parameter = null) : GameCommand;
```

### Command Parsing Updates

```csharp
// In ConsoleInputHandler.ParseCommand():

"[command]" or "[alias]" => Parse[CommandName](argument),

private GameCommand Parse[CommandName](string? argument)
{
    // Parsing logic
    return new [CommandName](parsedValue);
}
```

### User Commands

| Command | Description | Example |
|---------|-------------|---------|
| `[command]` | [What it does] | `[command]` |
| `[command] <arg>` | [What it does with arg] | `[command] goblin` |

---

## Rendering Changes

### IGameRenderer Additions

```csharp
/// <summary>
/// [Method description.]
/// </summary>
/// <param name="data">[Parameter description.]</param>
/// <param name="ct">Cancellation token.</param>
Task Render[FeatureName]Async([DtoType] data, CancellationToken ct = default);
```

### DTOs

```csharp
/// <summary>
/// [DTO description.]
/// </summary>
/// <param name="Property1">[Description.]</param>
/// <param name="Property2">[Description.]</param>
public record [DtoName](
    [Type] Property1,
    [Type] Property2);
```

### Display Example

```
[Example of what the user sees in the terminal/UI]

[ASCII art or formatted output example]
```

---

## Configuration

### [config-name].json

```json
{
  "[section]": {
    "[property]": "[value]",
    "[nestedSection]": {
      "[property]": "[value]"
    }
  }
}
```

### Configuration Schema

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `[property]` | [type] | [default] | [Description] |

---

## Acceptance Criteria

### [Feature Category]
- [ ] [Specific, testable criterion]
- [ ] [Specific, testable criterion]
- [ ] [Specific, testable criterion]

### [Feature Category]
- [ ] [Specific, testable criterion]
- [ ] [Specific, testable criterion]

### General
- [ ] ~[#] unit tests pass
- [ ] Build completes with 0 errors
- [ ] Build completes with 0 warnings

---

## Test Specifications

### Unit Tests (~[#] tests)

#### [TestClassName].cs
```csharp
[TestFixture]
public class [TestClassName]
{
    [Test]
    public void [MethodName]_[Scenario]_[ExpectedBehavior]();

    [Test]
    public void [MethodName]_[Scenario]_[ExpectedBehavior]();

    // Additional test signatures...
}
```

#### [TestClassName].cs
```csharp
[TestFixture]
public class [TestClassName]
{
    [Test]
    public void [MethodName]_[Scenario]_[ExpectedBehavior]();
}
```

---

## Dependencies

### Required from v0.0.Y

| Type | Location | Usage in this phase |
|------|----------|---------------------|
| `[TypeName]` | `[Path/File.cs]` | [How it's used] |

### Provides to v0.0.Xb

| Type | Usage |
|------|-------|
| `[TypeName]` | [How next phase uses it] |

---

*This design specification provides the detailed blueprint for implementing v0.0.Xa. See [v0.0.Xa Implementation Plan](v0.0.Xa-implementation-plan.md) for step-by-step implementation guidance.*

---

## Usage Notes

1. **When to create a design specification:**
   - For every sub-phase (v0.0.Xa, v0.0.Xb, etc.)
   - When implementing a complex feature requiring detailed planning
   - When multiple developers need to understand the design

2. **Level of detail:**
   - Include enough code to show interfaces and key method signatures
   - Don't include full implementations (save for implementation plan)
   - Focus on "what" and "why", not "how"

3. **Keep updated:**
   - Update status as work progresses
   - Note any design changes discovered during implementation
   - Link to implementation plan when created
