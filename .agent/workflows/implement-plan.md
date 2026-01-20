---
description: How to implement an implementation plan following the design specification
---

# Implement an Implementation Plan

## Usage

```
Implement [version] following the implementation plan, design specification, and scope breakdown.
Example: Implement v0.0.11a following the implementation plan, design specification, and scope breakdown.
```

## Instructions

When asked to implement a versioned implementation plan (e.g., `v0.0.Xa`), follow these steps:

### 1. Load Required Context

Read the following files in order:
1. `docs/v0.0.x/[version]-scope-breakdown.md` - Overall scope and context
2. `docs/v0.0.x/[version]-design-specification.md` - Detailed design requirements
3. `docs/v0.0.x/[version]-implementation-plan.md` - Step-by-step implementation checklist

### 2. Implementation Approach

- **Follow the implementation checklist** in the implementation plan document sequentially
- **Do not add functionality** beyond what is specified in the design specification
- **Mark checklist items** as `[/]` (in progress) or `[x]` (complete) as you work
- **Follow existing patterns** established in the codebase for similar components
- **Use Clean Architecture** principles (Domain → Application → Infrastructure → Presentation)

### 3. Implementation Order

For each phase in the implementation plan:

1. **Domain Layer First** - Enums, value objects, entities
2. **Configuration Classes** - Application layer configuration models
3. **Configuration Files** - JSON files in `config/` directory
4. **Application Services** - Core business logic services
5. **Infrastructure Integration** - Providers, DI registration
6. **Presentation Integration** - Commands, views, renderers
7. **Unit Tests** - Tests for all new components

### 4. Code Standards

- **XML Documentation** - All public members with `<summary>`, `<param>`, `<returns>`
- **Logging** - Integrate `Microsoft.Extensions.Logging` for all services
- **Null Safety** - Use nullable reference types and guards
- **Testing** - NUnit with FluentAssertions and Moq
- **Naming** - Follow existing naming conventions in the codebase

### 5. Verification

After implementation:
1. Run `dotnet build` to verify no compilation errors
2. Run `dotnet test` to verify all tests pass
3. Verify acceptance criteria from the design specification

### 6. Key Principles

- **No New Functionality** - Only implement what is specified
- **Data-Driven** - Configuration loaded from JSON files
- **Immutability** - Use `readonly record struct` for value objects
- **Dependency Injection** - Register all services in `DependencyInjection.cs`
- **Fallback Patterns** - Use graceful fallbacks for missing configuration

## Example Invocation

```
Implement v0.0.11a following the implementation plan, design specification, and scope breakdown.
```

This will:
1. Read the scope breakdown, design spec, and implementation plan for v0.0.11a
2. Implement each checklist item in order
3. Create all specified files (enums, value objects, services, tests)
4. Register services in dependency injection
5. Verify the build passes
