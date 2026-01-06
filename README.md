# Rune & Rust

A text-based dungeon crawler video game built with .NET 9, featuring both TUI (Terminal User Interface) and GUI options.

## Features

- **Dual Interface**: Play in the terminal (TUI) or graphical window (GUI)
- **Dungeon Exploration**: Navigate through rooms, collect items, and battle monsters
- **Inventory System**: Pick up and manage items
- **Combat**: Engage in turn-based combat with enemies
- **Save/Load**: Persist your game progress

## Technology Stack

- **.NET 9** - Runtime platform
- **Spectre.Console** - Terminal UI rendering
- **AvaloniaUI** - Cross-platform GUI framework
- **PostgreSQL** - Database persistence (with in-memory option for development)
- **Entity Framework Core** - ORM for database access
- **Serilog** - Structured logging
- **NUnit** - Unit testing framework

## Architecture

This project follows **Clean Architecture** principles:

```
src/
├── Core/
│   ├── RuneAndRust.Domain/           # Entities, value objects, domain logic
│   └── RuneAndRust.Application/      # Use cases, interfaces, DTOs
├── Infrastructure/
│   └── RuneAndRust.Infrastructure/   # EF Core, repositories, external services
└── Presentation/
    ├── RuneAndRust.Presentation.Shared/  # Shared ViewModels
    ├── RuneAndRust.Presentation.Tui/     # Spectre.Console TUI
    └── RuneAndRust.Presentation.Gui/     # AvaloniaUI GUI
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (optional, for PostgreSQL)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/southpawriter02/rune-rust.git
cd rune-rust
```

### 2. Start PostgreSQL (optional)

If you want to use PostgreSQL for persistence:

```bash
docker-compose up -d
```

By default, the game uses in-memory storage, so this step is optional.

### 3. Build the solution

```bash
dotnet build
```

### 4. Run the TUI version

```bash
dotnet run --project src/Presentation/RuneAndRust.Presentation.Tui
```

### 5. Run the GUI version

```bash
dotnet run --project src/Presentation/RuneAndRust.Presentation.Gui
```

## Game Commands (TUI)

| Command | Description |
|---------|-------------|
| `n`, `north` | Move north |
| `s`, `south` | Move south |
| `e`, `east` | Move east |
| `w`, `west` | Move west |
| `look`, `l` | Look around the room |
| `inventory`, `i` | View your inventory |
| `take <item>` | Pick up an item |
| `attack`, `a` | Attack an enemy |
| `save` | Save your game |
| `help`, `h`, `?` | Show help |
| `quit`, `q` | Quit the game |

## Running Tests

```bash
dotnet test
```

## Project Structure

### Domain Layer
- **Entities**: `Player`, `Room`, `Item`, `Monster`, `Dungeon`, `GameSession`
- **Value Objects**: `Position`, `Stats`
- **Services**: `CombatService`

### Application Layer
- **Interfaces**: `IGameRepository`, `IGameRenderer`, `IInputHandler`
- **DTOs**: Data transfer objects for UI binding
- **Services**: `GameSessionService`

### Infrastructure Layer
- **Repositories**: `InMemoryGameRepository` (PostgreSQL repository in development)
- **Persistence**: EF Core `GameDbContext`

### Presentation Layers
- **TUI**: Full game implementation with Spectre.Console
- **GUI**: Placeholder shell (full implementation planned)

## Configuration

Configuration is in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "GameDatabase": "Host=localhost;Port=5432;Database=runeandrust;Username=postgres;Password=postgres"
  },
  "Game": {
    "UseInMemoryDatabase": true
  }
}
```

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
