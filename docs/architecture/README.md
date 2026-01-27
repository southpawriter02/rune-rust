# Architecture Documentation

This directory contains Architecture Decision Records (ADRs) documenting key technical decisions made during the development of Rune and Rust.

## Overview

Rune and Rust follows Clean Architecture principles with a domain-centric design. The architecture prioritizes:

- **Testability**: Domain and application logic can be tested in isolation
- **Flexibility**: Infrastructure and presentation layers are easily swappable
- **Maintainability**: Clear separation of concerns and dependency rules

## Layer Structure

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  RuneAndRust.Presentation.Tui (Spectre.Console)             │
│  RuneAndRust.Presentation.Gui (Avalonia)                    │
│  RuneAndRust.Presentation.Shared (ViewModels, Adapters)     │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  RuneAndRust.Application                                     │
│  Services, Interfaces, DTOs, Logging                        │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│  RuneAndRust.Domain                                          │
│  Entities, Value Objects, Definitions, Enums                │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                       │
│  RuneAndRust.Infrastructure                                  │
│  JsonConfigurationProvider, GameDbContext, Repositories     │
└─────────────────────────────────────────────────────────────┘
```

## Architecture Decision Records

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [ADR-001](ADR-001-clean-architecture.md) | Clean Architecture | Accepted | 2026-01-06 |
| [ADR-002](ADR-002-json-configuration.md) | JSON Configuration System | Accepted | 2026-01-06 |
| [ADR-003](ADR-003-entity-framework.md) | Entity Framework Core Integration | Accepted | 2026-01-06 |
| [ADR-004](ADR-004-presentation-layers.md) | Multiple Presentation Layers | Accepted | 2026-01-06 |
| [ADR-005](ADR-005-test-organization.md) | NUnit Test Organization | Accepted | 2026-01-06 |
| [ADR-006](ADR-006-definition-pattern.md) | Definition Entity Pattern | Accepted | 2026-01-06 |
| [ADR-007](ADR-007-resource-system.md) | Resource System Design | Accepted | 2026-01-06 |

## ADR Format

Each ADR follows this structure:

1. **Status**: Proposed, Accepted, Deprecated, or Superseded
2. **Context**: The situation that motivated the decision
3. **Decision**: What was decided
4. **Consequences**: Positive, negative, and neutral outcomes
5. **Alternatives Considered**: Other options that were evaluated

## Related Documentation

- [Use Cases](../use-cases/README.md) - Player and system interaction specifications
- [Implementation Specifications](../v0.0.x/implementation-specifications/) - Version-specific implementation details
