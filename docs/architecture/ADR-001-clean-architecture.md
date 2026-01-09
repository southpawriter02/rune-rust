# ADR-001: Clean Architecture

**Status:** Accepted
**Date:** 2026-01-06
**Deciders:** Development Team

## Context

Rune and Rust is a dungeon crawler game with complex game mechanics including combat, abilities, equipment, and progression systems. The application needs to:

- Support multiple presentation layers (TUI and GUI)
- Be easily testable with high unit test coverage
- Allow game data to be configured without code changes
- Support future expansion with new features and systems
- Maintain clean separation between game logic and infrastructure concerns

Traditional layered architectures often lead to tight coupling between business logic and data access, making testing difficult and reducing flexibility.

## Decision

We will use Clean Architecture (also known as Onion Architecture or Hexagonal Architecture) with four distinct layers:

### Layer Structure

1. **Domain Layer** (`RuneAndRust.Domain`)
   - Contains core game entities, value objects, and domain logic
   - No dependencies on other layers or external libraries
   - Defines the fundamental game concepts: Player, Monster, Room, Abilities, etc.

2. **Application Layer** (`RuneAndRust.Application`)
   - Contains application services that orchestrate domain operations
   - Defines interfaces for infrastructure concerns (repositories, configuration)
   - Depends only on the Domain layer
   - Contains DTOs for data transfer between layers

3. **Infrastructure Layer** (`RuneAndRust.Infrastructure`)
   - Implements interfaces defined in Application layer
   - Contains database access (Entity Framework Core)
   - Contains configuration loading (JSON files)
   - Depends on Domain and Application layers

4. **Presentation Layer** (`RuneAndRust.Presentation.*`)
   - Contains UI implementations (TUI, GUI)
   - Depends on Application layer (via interfaces)
   - Contains views, input handlers, renderers

### Dependency Rule

Dependencies must point inward:
- Presentation → Application → Domain
- Infrastructure → Application → Domain
- Domain has no outward dependencies

## Consequences

### Positive

- **Testability**: Domain and Application layers can be tested in complete isolation without mocking infrastructure
- **Flexibility**: Infrastructure components (database, file system) can be swapped without affecting business logic
- **Maintainability**: Clear boundaries make it easy to understand where code belongs
- **Multiple UIs**: Same application layer serves both TUI and GUI presentations
- **Parallel Development**: Teams can work on different layers simultaneously

### Negative

- **Initial Complexity**: More projects and abstractions than a simple layered architecture
- **Mapping Overhead**: DTOs must be mapped between layers
- **Learning Curve**: Developers must understand the dependency rules

### Neutral

- Test projects mirror the main project structure (Domain.UnitTests, Application.UnitTests)
- Configuration loading abstracted behind `IConfigurationProvider` interface

## Alternatives Considered

### Alternative 1: Traditional N-Tier Architecture

Layers: Presentation → Business Logic → Data Access

**Rejected because:**
- Business logic tends to become coupled to data access layer
- Difficult to test business logic without database
- Changing data access technology requires business logic changes

### Alternative 2: Microservices Architecture

Separate services for combat, inventory, progression, etc.

**Rejected because:**
- Overkill for a single-player game application
- Adds deployment and operational complexity
- Inter-service communication overhead for a local game

### Alternative 3: CQRS (Command Query Responsibility Segregation)

Separate read and write models.

**Rejected because:**
- Game state updates are synchronous and immediate
- Read and write patterns are similar for game operations
- Added complexity not justified by requirements

## Related

- [ADR-002](ADR-002-json-configuration.md): JSON Configuration System
- [ADR-003](ADR-003-entity-framework.md): Entity Framework Core Integration
- [ADR-004](ADR-004-presentation-layers.md): Multiple Presentation Layers
