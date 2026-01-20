# ADR-004: Multiple Presentation Layers

**Status:** Accepted
**Date:** 2026-01-06
**Deciders:** Development Team

## Context

Rune and Rust targets two presentation modes:

1. **Terminal User Interface (TUI)**: A text-based console interface using Spectre.Console for rich terminal rendering
2. **Graphical User Interface (GUI)**: A cross-platform desktop application using Avalonia

Both interfaces need to:
- Display the same game information (player stats, room descriptions, combat)
- Accept the same player commands (move, attack, use ability)
- Share game logic without duplication
- Be independently testable

## Decision

We will implement a shared presentation architecture with abstraction layers:

### Project Structure

```
src/Presentation/
├── RuneAndRust.Presentation.Shared/    # Shared code
│   ├── Adapters/                       # Interface implementations
│   ├── ViewModels/                     # Shared view models
│   └── Extensions/                     # Utility extensions
├── RuneAndRust.Presentation.Tui/       # TUI implementation
│   ├── Views/                          # Console views
│   ├── Adapters/                       # Spectre.Console adapters
│   └── Program.cs                      # TUI entry point
└── RuneAndRust.Presentation.Gui/       # GUI implementation
    ├── Views/                          # Avalonia views
    ├── Adapters/                       # Avalonia adapters
    └── Program.cs                      # GUI entry point
```

### Core Abstractions

Two primary interfaces enable presentation independence:

1. **IGameRenderer**: Displays game state to the user
   - `RenderRoom()`: Display current room
   - `RenderCombat()`: Display combat state
   - `RenderPlayerStatus()`: Display player stats
   - `RenderMessage()`: Display game messages

2. **IInputHandler**: Captures player input
   - `GetCommand()`: Get next player command
   - `GetConfirmation()`: Get yes/no response
   - `GetSelection()`: Get choice from list

### Implementation Pattern

Each presentation project implements these interfaces:

**TUI** (`RuneAndRust.Presentation.Tui`):
- `SpectreGameRenderer`: Uses Spectre.Console for rich console output
- `ConsoleInputHandler`: Reads from console with command parsing

**GUI** (`RuneAndRust.Presentation.Gui`):
- `AvaloniaGameRenderer`: Updates Avalonia views with game state
- `AvaloniaInputHandler`: Captures input from GUI controls

### Shared ViewModels

ViewModels in `Presentation.Shared` contain presentation logic shared between TUI and GUI:

```csharp
public class CombatViewModel
{
    public PlayerDto Player { get; set; }
    public IReadOnlyList<MonsterDto> Monsters { get; set; }
    public string CurrentMessage { get; set; }
    public IReadOnlyList<AbilityDto> AvailableAbilities { get; set; }
}
```

## Consequences

### Positive

- **Code Reuse**: ViewModels, adapters, and utilities shared between TUI and GUI
- **Consistent Behavior**: Same game logic produces same results regardless of UI
- **Independent Development**: TUI and GUI can be developed and tested separately
- **Easy Testing**: Mock renderers and input handlers for integration tests
- **Extensibility**: New presentation layers (web, mobile) follow same pattern

### Negative

- **Abstraction Overhead**: Interfaces add indirection
- **Lowest Common Denominator**: Some UI capabilities can't be fully utilized
- **Adapter Complexity**: Each presentation needs adapter implementations
- **Synchronization**: Keeping TUI and GUI feature-parity requires discipline

### Neutral

- TUI is primary development target (faster iteration)
- GUI development can lag behind TUI
- Both share same DTOs from Application layer

## Implementation Details

### IGameRenderer Interface

```csharp
public interface IGameRenderer
{
    void RenderWelcome();
    void RenderRoom(RoomDto room, PlayerDto player);
    void RenderCombatStart(IReadOnlyList<MonsterDto> monsters);
    void RenderCombatRound(CombatStateDto combat);
    void RenderPlayerStatus(PlayerDto player);
    void RenderInventory(InventoryDto inventory);
    void RenderAbilities(IReadOnlyList<AbilityDto> abilities);
    void RenderMessage(string message, MessageType type);
    void RenderGameOver(GameOverDto gameOver);
    void Clear();
}
```

### IInputHandler Interface

```csharp
public interface IInputHandler
{
    string GetCommand(string prompt);
    bool GetConfirmation(string prompt);
    T GetSelection<T>(string prompt, IReadOnlyList<T> options, Func<T, string> display);
    string GetTextInput(string prompt);
}
```

### Spectre.Console Renderer Example

```csharp
public class SpectreGameRenderer : IGameRenderer
{
    public void RenderRoom(RoomDto room, PlayerDto player)
    {
        var panel = new Panel(new Markup(room.Description))
            .Header($"[bold]{room.Name}[/]")
            .Border(BoxBorder.Rounded);

        AnsiConsole.Write(panel);

        if (room.Monsters.Any())
        {
            AnsiConsole.MarkupLine("[red]Monsters:[/]");
            foreach (var monster in room.Monsters)
            {
                AnsiConsole.MarkupLine($"  - {monster.Name} ({monster.CurrentHealth}/{monster.MaxHealth} HP)");
            }
        }
    }
}
```

## Alternatives Considered

### Alternative 1: Single Presentation Project

One project with conditional compilation for TUI vs GUI.

**Rejected because:**
- Build complexity with conditionals
- Harder to maintain separate codebases
- All dependencies included regardless of target

### Alternative 2: No Abstraction

Direct calls to Spectre.Console and Avalonia from services.

**Rejected because:**
- Tightly couples application to presentation
- Cannot test without real UI
- Duplicated logic between TUI and GUI

### Alternative 3: MVVM with Data Binding

Full MVVM pattern with observable properties.

**Rejected because:**
- Overkill for TUI which has no data binding
- Adds complexity without proportional benefit
- Current approach is simpler and sufficient

### Alternative 4: Web-First with Terminal Emulation

Build web UI first, emulate terminal for TUI.

**Rejected because:**
- Terminal experience would be compromised
- Additional dependency on web stack
- Primary target is native terminal

## Related

- [ADR-001](ADR-001-clean-architecture.md): Clean Architecture (presentation as outer layer)
- [Use Cases](../use-cases/README.md): Define presentation behaviors
