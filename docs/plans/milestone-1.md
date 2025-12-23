# Phase 1: Infrastructure & Core Domain (Milestone 1)

## v0.0.1: The Foundation

Scope: Established solution structure, Dependency Injection (DI) container, and Serilog logging configuration. Implemented the "Hello World" proof of concept to verify the architecture.

Deliverables: IGameService, GameService, Test Project setup.

## v0.0.2: The Domain & Logic

Scope: Implemented core RPG logic, including the DiceService (d10 pool mechanics) and the StatCalculationService. Defined the Attribute enum (Might, Finesse, etc.) and basic Character entity.

Deliverables: IDiceService, Attribute.cs, Character.cs, Unit Tests for math logic.

## v0.0.3: The Loop (Input/Output Pipeline)

Scope: Transformed the static application into an interactive state machine. Implemented the GameLoop, CommandParser, and GamePhase states (MainMenu, Exploration, Combat).

Deliverables: IInputHandler, GameState, CommandParser.

## v0.0.4: The Persistence (Database Integration)

Scope: Integrated PostgreSQL with Entity Framework Core. Implemented the SaveManager service and SaveGame entity to serialize game state to JSONB columns.

Deliverables: RuneAndRustDbContext, GenericRepository, ISaveGameRepository.

## v0.0.5: The Spatial Core

Scope: Implemented the 3D coordinate system (Coordinate record) and the Room entity. Established the NavigationService to handle movement (North, South, Up, Down) and exit validation.

Deliverables: Room.cs, NavigationService, DungeonGenerator (Basic Test Map).