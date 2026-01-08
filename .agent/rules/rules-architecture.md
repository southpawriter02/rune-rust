---
trigger: always_on
---

# Architecture Rules

## 1.1 Clean Architecture Compliance
- **ALWAYS** respect the Clean Architecture layer boundaries:
  - **Domain Layer** → No external dependencies, contains entities, value objects, enums, and domain services
  - **Application Layer** → References Domain only, contains interfaces, DTOs, and application services
  - **Infrastructure Layer** → References Application and Domain, contains repositories, persistence, and external service implementations
  - **Presentation Layer** → References Application, contains TUI/GUI implementations and view models
- **NEVER** introduce circular dependencies between layers
- **ALWAYS** use dependency injection for cross-layer communication via interfaces defined in the Application layer

## 1.2 Project Structure
- Place new entities in `src/Core/RuneAndRust.Domain/Entities/`
- Place new value objects in `src/Core/RuneAndRust.Domain/ValueObjects/`
- Place new enums in `src/Core/RuneAndRust.Domain/Enums/`
- Place new interfaces in `src/Core/RuneAndRust.Application/Interfaces/`
- Place new DTOs in `src/Core/RuneAndRust.Application/DTOs/`
- Place new application services in `src/Core/RuneAndRust.Application/Services/`