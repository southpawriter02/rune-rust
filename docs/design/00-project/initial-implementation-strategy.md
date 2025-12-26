---
id: SPEC-PROJECT-INITIAL-STRATEGY
title: "Initial Implementation Strategy"
version: 1.0
status: draft
last-updated: 2025-12-18
---

# Initial Implementation Strategy

> *"First the bones, then the flesh, then the armor."* — Dvergr Proverb

## 1. Objective
To establish a robust, scalable C# solution structure for *Rune & Rust* that strictly enforces the separation between Game Logic (Engine) and Presentation (UI). This ensures we can support both the immediate Terminal UI (TUI) and the future Graphical UI (GUI) without code duplication.

## 2. Architectural Blueprint

We will implement a **Clean Architecture** solution with four primary projects. This structure ensures that the core game rules are isolated from external concerns like databases or UI frameworks.

### 2.1 Solution Structure

```text
RuneAndRust.sln
├── src/
│   ├── RuneAndRust.Core/           # The "Inner Circle"
│   │   ├── Entities/               # Rich domain models (Character, Item, Room)
│   │   ├── Interfaces/             # Service contracts (IGameEngine, IRepository)
│   │   ├── ValueObjects/           # Immutable primitives (DiceRoll, Position)
│   │   └── Enums/                  # Game constants (Attribute, DamageType)
│   │
│   ├── RuneAndRust.Engine/         # The "Application Layer"
│   │   ├── Systems/                # Discrete logic units (CombatSystem, MovementSystem)
│   │   ├── GameLoop/               # State machine and timing logic
│   │   └── Services/               # Orchestration of systems
│   │
│   ├── RuneAndRust.Data/           # The "Infrastructure Layer"
│   │   ├── Contexts/               # EF Core DbContext
│   │   └── Repositories/           # Implementation of Core interfaces
│   │
│   └── RuneAndRust.UI.Terminal/    # The "Presentation Layer" (TUI)
│       ├── Views/                  # Spectre.Console renderers
│       └── Program.cs              # DI Composition Root & Entry Point
```

### 2.2 Dependency Flow
*   `UI.Terminal` depends on `Engine`, `Core`, and `Data`.
*   `Data` depends on `Core` (and implements its interfaces).
*   `Engine` depends on `Core`.
*   `Core` depends on **nothing**.

## 3. Implementation Phasing

We will adhere to a "Walking Skeleton" approach: building a tiny slice of end-to-end functionality before expanding the breadth.

### Phase 1: The Foundation (Current Goal)
**Focus**: Project scaffolding and Dependency Injection.
1.  Create the Solution (`.sln`) and Project (`.csproj`) files.
2.  Establish the Project Dependencies defining the architecture.
3.  Implement a minimal Dependency Injection (DI) container in the Terminal app.
4.  **Goal**: Run the application and see "Rune & Rust Core initialized" in the terminal.

### Phase 2: The Loop
**Focus**: The State Machine described in `SPEC-CORE-GAMELOOP`.
1.  Implement the `GameState` class in `Core`.
2.  Implement the `GameEngine` class in `Engine`.
3.  Create the `IInputHandler` interface to abstract TUI input vs GUI input.
4.  **Goal**: A running loop where the user can type "start" and "quit", creating Game State transitions.

### Phase 3: The Persistence
**Focus**: EF Core integration.
1.  Setup `RuneAndRust.Data` with Entity Framework Core.
2.  Configure PostgreSQL (or SQLite for rapid prototyping if preferred).
3.  Implement `GenericRepository`.
4.  **Goal**: Start the game, make a change, close it, reload, and see the change persisted.

### Phase 4: Exploration (The First Gameplay)
**Focus**: Movement and Room description.
1.  Define `Room` and `Exit` entities.
2.  Implement `MovementSystem` in Engine.
3.  Render Room descriptions in TUI.
4.  **Goal**: Walk between two defined rooms using "North" and "South".

## 4. Key Technical Decisions

*   **TUI Library**: `Spectre.Console` will be used for the Terminal UI. It provides rich text, tables, and prompts while remaining a console application.
*   **Result Pattern**: We will use a `Result<T>` pattern for service returns to handle success/failure explicitly without exceptions for logic flow.
*   **Mediator Pattern (Optional)**: We may consider `MediatR` for decoupling internal Engine commands if complexity grows, but simple Service interfaces will suffice for Phase 1.

## 5. Immediate Action Plan

1.  Execute `dotnet` commands to initialize the solution.
2.  Create the detailed `task.md` for Phase 1 implementation.
3.  Begin coding `RuneAndRust.Core`.
