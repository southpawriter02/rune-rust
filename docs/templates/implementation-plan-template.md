# Implementation Plan Template

Use this template for creating implementation plan files (e.g., `v0.0.Xa-implementation-plan.md`) when detailed step-by-step implementation guidance is needed beyond the design specification.

---

# v0.0.Xa Implementation Plan: [Phase Name]

**Version:** 0.0.Xa
**Parent:** v0.0.X ([Version Theme])
**Prerequisites:** [e.g., "v0.0.5a (Core Dice Engine) Complete, v0.0.5b (Skill Check System) Complete"]
**Status:** Ready for Implementation
**Target Tests:** [Current] -> [Target] (+[number] tests)

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Dependencies from Previous Phases](#dependencies-from-previous-phases)
3. [Current System Analysis](#current-system-analysis)
4. [Detailed Implementation](#detailed-implementation)
   - [Domain Layer](#domain-layer)
   - [Application Layer](#application-layer)
   - [Presentation Layer](#presentation-layer) *(if applicable)*
5. [Flow Diagrams](#flow-diagrams)
6. [Testing Strategy](#testing-strategy)
7. [Logging Strategy](#logging-strategy)
8. [Implementation Checklist](#implementation-checklist)
9. [Acceptance Criteria](#acceptance-criteria)
10. [Risk Assessment](#risk-assessment)
11. [File Summary](#file-summary)

---

## Executive Summary

### Purpose
[2-3 sentences describing what this implementation accomplishes.]

### Scope
- **In Scope:**
  - [Bullet points of what IS included]

- **Out of Scope:**
  - [Bullet points of what is NOT included, with references]

### Key Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| [Decision area] | [Choice made] | [Why this choice] |

---

## Dependencies from Previous Phases

### Dependencies from v0.0.Ya ([Phase Name])

| Type | Location | Usage in this phase |
|------|----------|---------------------|
| `[TypeName]` | `[Path/File.cs]` | [Specific usage] |

### Dependencies from v0.0.Yb ([Phase Name])

| Type | Location | Usage in this phase |
|------|----------|---------------------|
| `[TypeName]` | `[Path/File.cs]` | [Specific usage] |

---

## Current System Analysis

### Existing [SystemName]

**Location:** `src/Core/RuneAndRust.[Layer]/[Path]/[File].cs`

**Current Flow:**
```
[MethodName]([params])
    ├── Step 1
    │   └── [What happens]
    └── Step 2
        └── [What happens]
```

**Current Types:**
```csharp
// Current implementation
public record [CurrentType](
    [Property],
    [Property]);
```

### New [SystemName] Flow

```
[MethodName]([params])
    ├── Step 1 (NEW)
    │   ├── [New behavior]
    │   └── [New behavior]
    └── Step 2 (MODIFIED)
        └── [Changed behavior]
```

---

## Detailed Implementation

### Domain Layer

#### 1. [FileName].cs (NEW)

**File:** `src/Core/RuneAndRust.Domain/[Folder]/[FileName].cs`

```csharp
using [Namespace];

namespace RuneAndRust.Domain.[Folder];

/// <summary>
/// [Type description.]
/// </summary>
/// <remarks>
/// <para>[Additional context about design decisions.]</para>
/// <para>[Usage examples or important notes.]</para>
/// </remarks>
/// <example>
/// <code>
/// var example = [TypeName].Create(param);
/// Console.WriteLine(example.Property);
/// </code>
/// </example>
public [class/record/struct] [TypeName]
{
    // ===== Properties =====

    /// <summary>
    /// Gets [property description].
    /// </summary>
    public [Type] [PropertyName] { get; init; }

    // ===== Computed Properties =====

    /// <summary>
    /// Gets [computed property description].
    /// </summary>
    public [Type] [ComputedPropertyName] => /* calculation */;

    // ===== Constructors =====

    /// <summary>
    /// Creates a new [TypeName].
    /// </summary>
    public [TypeName]([params])
    {
        // Initialize properties
    }

    // ===== Methods =====

    /// <summary>
    /// [Method description.]
    /// </summary>
    /// <param name="paramName">[Parameter description.]</param>
    /// <returns>[Return description.]</returns>
    public [ReturnType] [MethodName]([params])
    {
        // Full implementation
    }

    /// <summary>
    /// Returns a formatted string [context].
    /// </summary>
    public override string ToString()
    {
        // Implementation
    }
}
```

#### 2. Update [ExistingFile].cs (MODIFY)

**File:** `src/Core/RuneAndRust.Domain/[Folder]/[ExistingFile].cs`

```csharp
// Add to existing [ClassName]:

/// <summary>
/// [New property/method description.]
/// </summary>
public [Type] [NewMemberName] { get; private set; }

// Update existing method:
public [ReturnType] [ExistingMethodName]([newParams])
{
    // Full updated implementation
}
```

### Application Layer

#### 3. [InterfaceName].cs (NEW)

**File:** `src/Core/RuneAndRust.Application/Interfaces/[InterfaceName].cs`

```csharp
using [Namespace];

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Interface for [purpose].
/// </summary>
/// <remarks>
/// [Additional context, design rationale.]
/// </remarks>
public interface [InterfaceName]
{
    /// <summary>
    /// [Method description.]
    /// </summary>
    /// <param name="paramName">[Description.]</param>
    /// <returns>[Return description.]</returns>
    [ReturnType] [MethodName]([params]);
}
```

#### 4. [ServiceName].cs (NEW/MODIFY)

**File:** `src/Core/RuneAndRust.Application/Services/[ServiceName].cs`

```csharp
using Microsoft.Extensions.Logging;
using [Namespace];

namespace RuneAndRust.Application.Services;

/// <summary>
/// [Service description.]
/// </summary>
public class [ServiceName] : [InterfaceName]
{
    private readonly [IDependency] _dependency;
    private readonly ILogger<[ServiceName]> _logger;

    /// <summary>
    /// Creates a new [ServiceName] instance.
    /// </summary>
    public [ServiceName]([IDependency] dependency, ILogger<[ServiceName]> logger)
    {
        _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
        _logger = logger;
    }

    /// <inheritdoc />
    public [ReturnType] [MethodName]([params])
    {
        // Full implementation with logging
        _logger.LogDebug("[Action]: {Param}", param);

        // Step 1
        // Step 2
        // Return result
    }
}
```

#### 5. [DtoName]Dtos.cs (NEW)

**File:** `src/Core/RuneAndRust.Application/DTOs/[DtoName]Dtos.cs`

```csharp
using RuneAndRust.Domain.[Folder];

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for [purpose].
/// </summary>
/// <param name="Property1">[Description.]</param>
/// <param name="Property2">[Description.]</param>
public record [DtoName](
    [Type] Property1,
    [Type] Property2)
{
    /// <summary>
    /// Creates a DTO from a domain [Type].
    /// </summary>
    public static [DtoName] FromDomain([DomainType] domain)
    {
        return new [DtoName](
            domain.Property1,
            domain.Property2);
    }
}
```

### Presentation Layer *(if applicable)*

#### 6. Update [RendererName].cs (MODIFY)

**File:** `src/Presentation/RuneAndRust.Presentation.Tui/Adapters/[RendererName].cs`

```csharp
/// <inheritdoc />
public Task Render[FeatureName]Async([DtoType] data, CancellationToken ct = default)
{
    // Full rendering implementation
    var panel = new Panel(BuildContent(data))
        .Header("[color]Header[/]")
        .Border(BoxBorder.Rounded);

    AnsiConsole.Write(panel);
    return Task.CompletedTask;
}

private string BuildContent([DtoType] data)
{
    // Build formatted content
}
```

---

## Flow Diagrams

### [Process Name] Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        [FLOW TITLE]                                          │
└─────────────────────────────────────────────────────────────────────────────┘

    [Entry Point] called
           │
           ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         PHASE 1: [PHASE NAME]                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌────────────────────┐                                                     │
│  │ 1. [STEP NAME]     │                                                     │
│  │    [Description]   │                                                     │
│  └──────────┬─────────┘                                                     │
│             │                                                               │
│             ▼                                                               │
│  ┌────────────────────────────────────────────────────────────────────┐    │
│  │ 2. [STEP NAME]                                                      │    │
│  │    [Description with decision points]                              │    │
│  └────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
              │
              ▼
     [Continue to next phase or end]
```

### Decision Tree *(if applicable)*

```
                         ┌─────────────────┐
                         │   [Decision]    │
                         └────────┬────────┘
                                  │
                    ┌─────────────┼─────────────┐
                    │             │             │
                    ▼             ▼             ▼
             ┌───────────┐ ┌───────────┐ ┌───────────┐
             │ Option A  │ │ Option B  │ │ Option C  │
             └─────┬─────┘ └─────┬─────┘ └─────┬─────┘
                   │             │             │
                   ▼             ▼             ▼
            [Outcome A]   [Outcome B]   [Outcome C]
```

---

## Testing Strategy

### Test Organization

```
tests/
├── RuneAndRust.Domain.UnitTests/
│   ├── [Folder]/
│   │   └── [TestFileName]Tests.cs           ([#] tests)
│   └── Services/
│       └── [ServiceName]Tests.cs            ([#] tests)
├── RuneAndRust.Application.UnitTests/
│   └── Services/
│       └── [ServiceName]Tests.cs            ([#] tests)
```

### Test Files

#### 1. [TestFileName]Tests.cs

**File:** `tests/RuneAndRust.Domain.UnitTests/[Folder]/[TestFileName]Tests.cs`

```csharp
using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.[Folder];

namespace RuneAndRust.Domain.UnitTests.[Folder];

[TestFixture]
public class [TestFileName]Tests
{
    [Test]
    public void [MethodUnderTest]_[Scenario]_[ExpectedBehavior]()
    {
        // Arrange
        var input = /* setup */;

        // Act
        var result = /* action */;

        // Assert
        result.Should().Be(/* expected */);
    }

    [Test]
    public void [MethodUnderTest]_[Scenario]_[ExpectedBehavior]()
    {
        // Arrange
        // Act
        // Assert
    }

    // Additional tests...
}
```

#### 2. [ServiceName]Tests.cs

**File:** `tests/RuneAndRust.Domain.UnitTests/Services/[ServiceName]Tests.cs`

```csharp
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Domain.Services;

namespace RuneAndRust.Domain.UnitTests.Services;

[TestFixture]
public class [ServiceName]Tests
{
    private [ServiceName] _service = null!;
    private Mock<ILogger<[ServiceName]>> _mockLogger = null!;
    private Mock<[IDependency]> _mockDependency = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<[ServiceName]>>();
        _mockDependency = new Mock<[IDependency]>();
        _service = new [ServiceName](_mockDependency.Object, _mockLogger.Object);
    }

    // Helper methods
    private static [Type] Create[TestHelper]([params])
    {
        return /* helper implementation */;
    }

    [Test]
    public void [MethodUnderTest]_[Scenario]_[ExpectedBehavior]()
    {
        // Arrange
        var input = Create[TestHelper](/* params */);

        // Act
        var result = _service.[MethodUnderTest](input);

        // Assert
        result.Should().NotBeNull();
        result.Property.Should().Be(/* expected */);
    }
}
```

---

## Logging Strategy

### Log Levels by Operation

| Operation | Level | Example |
|-----------|-------|---------|
| [Operation start] | Information | "[Action] initiated: {Params}" |
| [Internal step] | Debug | "[Step]: {Detail}" |
| [Important event] | Information | "[Event]: {Result}" |
| [Warning condition] | Warning | "[Condition]: {Context}" |
| [Error state] | Error | "[Error]: {Exception}" |

### Structured Logging Format

```csharp
_logger.LogDebug(
    "[Action] [step]: [{Value}] + {Modifier} = {Total}",
    value,
    modifier,
    total);

_logger.LogInformation(
    "[Action] complete - {Actor}: {Result}, {Metric1}: {Value1}, {Metric2}: {Value2}",
    actor,
    result,
    value1,
    value2);
```

---

## Implementation Checklist

### Domain Layer
- [ ] Create `src/Core/RuneAndRust.Domain/[Folder]/[File].cs`
  - [ ] [Type/Class name]
  - [ ] XML documentation
  - [ ] All properties
  - [ ] All methods
- [ ] Update `src/Core/RuneAndRust.Domain/[Folder]/[ExistingFile].cs`
  - [ ] New property/method
  - [ ] Updated logic

### Application Layer
- [ ] Create `src/Core/RuneAndRust.Application/Interfaces/[Interface].cs`
- [ ] Create `src/Core/RuneAndRust.Application/Services/[Service].cs`
- [ ] Create `src/Core/RuneAndRust.Application/DTOs/[Dtos].cs`
- [ ] Update DI registration in `Program.cs` or startup

### Presentation Layer *(if applicable)*
- [ ] Update `src/Presentation/RuneAndRust.Presentation.Tui/[File].cs`

### Tests
- [ ] Create `tests/RuneAndRust.Domain.UnitTests/[Folder]/[Tests].cs` ([#] tests)
- [ ] Create `tests/RuneAndRust.Application.UnitTests/[Folder]/[Tests].cs` ([#] tests)

### Validation
- [ ] All [#] new tests pass
- [ ] All existing tests pass
- [ ] Build completes with 0 errors
- [ ] Build completes with 0 warnings

---

## Acceptance Criteria

### Functional Criteria

| ID | Criterion | Verification |
|----|-----------|--------------|
| AC-1 | [Requirement] | Unit test |
| AC-2 | [Requirement] | Unit test |
| AC-3 | [Requirement] | Manual verification |

### Non-Functional Criteria

| ID | Criterion | Verification |
|----|-----------|--------------|
| NF-1 | [Requirement] | Code review |
| NF-2 | [Requirement] | Code review |
| NF-3 | [Requirement] | Test suite |

---

## Risk Assessment

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| [Risk] | High/Medium/Low | High/Medium/Low | [Strategy] |

---

## File Summary

### Files to Create (New)

| File | Purpose | Est. Lines |
|------|---------|------------|
| `[Path/File].cs` | [Purpose] | ~[#] |

### Files to Modify

| File | Changes |
|------|---------|
| `[Path/File].cs` | [Description of changes] (~[#] lines) |

### Final Metrics

| Metric | Before | After |
|--------|--------|-------|
| [Type] Count | [#] | [#] |
| Unit Tests | ~[#] | ~[#] |

---

## Next Steps

After completing this phase:

1. **v0.0.Xb ([Phase Name])** - [Brief description]
2. **v0.0.Xc ([Phase Name])** - [Brief description]

---

## Usage Notes

1. **When to create an implementation plan:**
   - Complex implementations requiring step-by-step guidance
   - When full code implementations are needed before coding begins
   - When multiple developers will work on the implementation
   - When the design specification alone isn't detailed enough

2. **Difference from design specification:**
   - Design spec: "What" and "Why" - interfaces, signatures, structure
   - Implementation plan: "How" - full code, step-by-step, testing strategy

3. **Level of detail:**
   - Include complete, copy-paste-ready code
   - Include full test implementations
   - Include all logging statements
   - Include helper methods and utilities
