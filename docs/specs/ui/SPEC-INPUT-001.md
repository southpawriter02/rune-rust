---
id: SPEC-INPUT-001
title: Input Handling System
version: 1.0.0
status: Implemented
related_specs: [SPEC-UI-001, SPEC-COMBAT-001]
---

# SPEC-INPUT-001: Input Handling System

> **Version:** 1.0.0
> **Status:** Implemented
> **Services:** CommandParser, TerminalInputHandler, InputConfigurationService
> **Location:** `RuneAndRust.Engine/Services/`, `RuneAndRust.Terminal/Services/`

---

## Overview

The **Input Handling System** manages all player input for Rune & Rust, transforming raw terminal commands into structured game actions. The system employs a **command parsing architecture** where user input is parsed into `ParseResult` objects representing player intentions, which are then executed by the appropriate service layer.

### Key Responsibilities

1. **Command Parsing**: Transform raw text input into structured `ParseResult` objects (CommandParser)
2. **Input Abstraction**: Provide platform-agnostic input interface (IInputHandler → TerminalInputHandler)
3. **Keybinding Configuration**: Load and manage customizable hotkey mappings (InputConfigurationService)
4. **Phase-Based Routing**: Route commands to appropriate handlers based on GamePhase (MainMenu, Exploration, Combat)
5. **Natural Language Support**: Recognize command aliases and variants ("take"/"get"/"grab", "equip"/"bind")
6. **Error Handling**: Provide helpful feedback for unknown commands and missing parameters

### Architecture Pattern

```
User Input (Terminal) → IInputHandler → CommandParser → ParseResult → Service Execution
                                              ↓
                                    InputConfigurationService (Keybindings)
                                              ↓
                                    Phase-Based Command Routing
```

**Key Design Decision**: The system uses a **ParseResult pattern** where commands are parsed to **intention objects** (not executed during parsing). This separates parsing logic from game logic, enabling better testing and clearer command validation.

**Technology Stack**:
- **Spectre.Console 0.54.0**: Terminal input via `AnsiConsole.Ask<string>()`
- **.NET 9.0**: Pattern matching for command routing, async command execution
- **JSON Configuration**: Keybindings stored in `data/input_bindings.json`

---

## Core Concepts

### 1. ParseResult Pattern

**Definition**: Command parsing produces `ParseResult` objects that represent player **intentions**, not executed actions.

**Structure**:
```csharp
public record ParseResult(
    CommandType Type,
    Dictionary<string, string> Parameters,
    bool IsValid,
    string? ErrorMessage = null
);

public enum CommandType
{
    // Navigation
    MoveNorth, MoveSouth, MoveEast, MoveWest, MoveUp, MoveDown,

    // Observation
    Look, Examine, Search,

    // Inventory
    TakeItem, DropItem, EquipItem, UnequipItem, UseItem,

    // Combat
    Attack, Defend, UseAbility, Flee,

    // Crafting
    Craft, OpenCrafting, SelectTrade,

    // Screens
    OpenInventory, OpenJournal, OpenOptions,

    // System
    Help, Quit, Unknown
}
```

**Example**:
```csharp
// Input: "attack orc with fireball"
var result = new ParseResult(
    Type: CommandType.Attack,
    Parameters: new Dictionary<string, string>
    {
        ["target"] = "orc",
        ["ability"] = "fireball"
    },
    IsValid: true
);
```

**Execution Flow**:
```
1. CommandParser.ParseAndExecuteAsync(input, gameState)
2. Parse input → ParseResult
3. Validate parameters (e.g., target exists?)
4. If valid → Route to service (e.g., _combatService.Attack(target, ability))
5. If invalid → Set ErrorMessage, return to player
```

**Benefits**:
- **Testability**: Parse logic can be tested independently of game logic
- **Validation**: Parameter validation occurs before service method calls
- **Error Reporting**: Clear separation between syntax errors (parsing) and semantic errors (execution)

---

### 2. Input Abstraction (IInputHandler)

**Purpose**: Decouple game logic from terminal-specific input mechanisms.

**Interface**:
```csharp
public interface IInputHandler
{
    string GetInput(string prompt);
    Task<string> GetInputAsync(string prompt, CancellationToken ct);
}
```

**Implementation (TerminalInputHandler)**:
```csharp
public class TerminalInputHandler : IInputHandler
{
    public string GetInput(string prompt)
    {
        return AnsiConsole.Ask<string>(prompt);
    }

    public async Task<string> GetInputAsync(string prompt, CancellationToken ct)
    {
        return await Task.Run(() => AnsiConsole.Ask<string>(prompt), ct);
    }
}
```

**Benefits**:
- **Platform Independence**: Game logic only depends on IInputHandler interface
- **Testability**: Mock input handler for automated tests
- **Future GUI Support**: Implement `GUIInputHandler` without changing game logic

**Current Limitation**: Synchronous blocking input (no async readline). Player must wait for prompt before next action.

---

### 3. Phase-Based Command Routing

**Purpose**: Different game phases (MainMenu, Exploration, Combat) accept different command sets.

**Routing Logic**:
```csharp
public async Task ParseAndExecuteAsync(string input, GameState state)
{
    var trimmed = input.Trim().ToLowerInvariant();

    // Phase-specific routing
    var result = state.Phase switch
    {
        GamePhase.MainMenu => ParseMainMenuCommand(trimmed),
        GamePhase.Exploration => ParseExplorationCommand(trimmed, state),
        GamePhase.Combat => ParseCombatCommand(trimmed, state),
        GamePhase.Inventory => ParseInventoryCommand(trimmed, state),
        GamePhase.Crafting => ParseCraftingCommand(trimmed, state),
        _ => new ParseResult(CommandType.Unknown, IsValid: false, ErrorMessage: "Unknown game phase")
    };

    if (result.IsValid)
        await ExecuteCommand(result, state);
    else
        DisplayError(result.ErrorMessage);
}
```

**Phase-Specific Command Sets**:

| Phase | Allowed Commands | Examples |
|-------|------------------|----------|
| MainMenu | New Game, Continue, Options, Quit | `new`, `continue`, `options` |
| Exploration | Navigation, Observation, Inventory, Crafting, Screens | `north`, `look`, `take sword`, `open inventory` |
| Combat | Attack, Defend, UseAbility, Flee, Inventory (limited) | `attack orc`, `defend`, `use fireball`, `flee` |
| Inventory | Equip, Unequip, Drop, Use, Back | `equip sword`, `drop potion`, `back` |
| Crafting | Craft, SelectTrade, Back | `craft longsword`, `trade blacksmithing`, `back` |

**Error Handling**:
```csharp
// Example: Trying to craft during combat
if (state.Phase == GamePhase.Combat && result.Type == CommandType.OpenCrafting)
{
    return new ParseResult(
        Type: CommandType.Unknown,
        IsValid: false,
        ErrorMessage: "Cannot craft during combat. Defeat enemies first."
    );
}
```

---

### 4. Command Alias System

**Purpose**: Support natural language variations for commands ("take"/"get"/"grab").

**Implementation**:
```csharp
private static readonly Dictionary<string, CommandType> CommandAliases = new()
{
    // Navigation
    ["n"] = CommandType.MoveNorth,
    ["north"] = CommandType.MoveNorth,
    ["go north"] = CommandType.MoveNorth,

    ["s"] = CommandType.MoveSouth,
    ["south"] = CommandType.MoveSouth,
    ["go south"] = CommandType.MoveSouth,

    // Inventory
    ["take"] = CommandType.TakeItem,
    ["get"] = CommandType.TakeItem,
    ["grab"] = CommandType.TakeItem,
    ["pick up"] = CommandType.TakeItem,

    ["drop"] = CommandType.DropItem,
    ["discard"] = CommandType.DropItem,

    ["equip"] = CommandType.EquipItem,
    ["bind"] = CommandType.EquipItem,
    ["wear"] = CommandType.EquipItem,

    // Observation
    ["look"] = CommandType.Look,
    ["l"] = CommandType.Look,

    ["examine"] = CommandType.Examine,
    ["x"] = CommandType.Examine,
    ["inspect"] = CommandType.Examine,

    // Combat
    ["attack"] = CommandType.Attack,
    ["a"] = CommandType.Attack,
    ["hit"] = CommandType.Attack,

    ["flee"] = CommandType.Flee,
    ["run"] = CommandType.Flee,
    ["escape"] = CommandType.Flee,

    // Screens
    ["inventory"] = CommandType.OpenInventory,
    ["i"] = CommandType.OpenInventory,

    ["journal"] = CommandType.OpenJournal,
    ["j"] = CommandType.OpenJournal,
    ["log"] = CommandType.OpenJournal,

    ["crafting"] = CommandType.OpenCrafting,
    ["c"] = CommandType.OpenCrafting,
    ["craft"] = CommandType.OpenCrafting,

    // System
    ["help"] = CommandType.Help,
    ["h"] = CommandType.Help,
    ["?"] = CommandType.Help,

    ["quit"] = CommandType.Quit,
    ["q"] = CommandType.Quit,
    ["exit"] = CommandType.Quit
};
```

**Matching Logic**:
```csharp
private CommandType ResolveAlias(string input)
{
    // Exact match
    if (CommandAliases.TryGetValue(input, out var type))
        return type;

    // Partial match (e.g., "tak" → "take")
    var partial = CommandAliases.Keys
        .FirstOrDefault(k => k.StartsWith(input, StringComparison.OrdinalIgnoreCase));

    return partial != null ? CommandAliases[partial] : CommandType.Unknown;
}
```

**Benefits**:
- **User-Friendly**: Players can use natural language ("grab sword" vs "take sword")
- **Accessibility**: Shorter aliases ("n" vs "north") reduce typing burden
- **Discoverability**: Partial matching helps players discover commands

---

### 5. Keybinding Configuration

**Purpose**: Allow players to customize hotkeys for common actions.

**Configuration File** (`data/input_bindings.json`):
```json
{
  "navigation": {
    "north": ["n", "north", "8"],
    "south": ["s", "south", "2"],
    "east": ["e", "east", "6"],
    "west": ["w", "west", "4"],
    "up": ["u", "up", "9"],
    "down": ["d", "down", "3"]
  },
  "screens": {
    "inventory": ["i", "inventory"],
    "journal": ["j", "journal"],
    "crafting": ["c", "crafting"],
    "options": ["o", "options"]
  },
  "combat": {
    "attack": ["a", "attack", "enter"],
    "defend": ["d", "defend", "space"],
    "flee": ["f", "flee", "escape"]
  },
  "system": {
    "help": ["h", "help", "?"],
    "quit": ["q", "quit", "esc"]
  }
}
```

**InputConfigurationService**:
```csharp
public class InputConfigurationService
{
    private readonly Dictionary<string, List<string>> _bindings;

    public InputConfigurationService()
    {
        _bindings = LoadBindingsFromJson("data/input_bindings.json");
    }

    public List<string> GetBindings(string action)
    {
        return _bindings.TryGetValue(action, out var bindings) ? bindings : new List<string>();
    }

    public bool IsActionBound(string input, string action)
    {
        var bindings = GetBindings(action);
        return bindings.Any(b => b.Equals(input, StringComparison.OrdinalIgnoreCase));
    }

    private Dictionary<string, List<string>> LoadBindingsFromJson(string path)
    {
        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(json);

        // Flatten nested structure: navigation.north → ["n", "north", "8"]
        var flat = new Dictionary<string, List<string>>();
        foreach (var category in config.Values)
        {
            foreach (var (action, keys) in category)
                flat[action] = keys;
        }

        return flat;
    }
}
```

**Integration with CommandParser**:
```csharp
public async Task ParseAndExecuteAsync(string input, GameState state)
{
    var trimmed = input.Trim().ToLowerInvariant();

    // Check keybindings first
    if (_inputConfig.IsActionBound(trimmed, "north"))
        return await ExecuteMoveCommand(Direction.North, state);

    if (_inputConfig.IsActionBound(trimmed, "attack"))
        return await ParseCombatCommand(trimmed, state);

    // Fallback to alias resolution
    var type = ResolveAlias(trimmed);
    // ... rest of parsing logic
}
```

**Benefits**:
- **Customizability**: Players can rebind keys to personal preference
- **Accessibility**: Support for numpad navigation (8/2/4/6 for cardinal directions)
- **Future Enhancement**: In-game keybinding editor in Options screen

---

### 6. Parameter Extraction

**Purpose**: Parse multi-word commands with targets, abilities, quantities.

**Examples**:
| Input | CommandType | Extracted Parameters |
|-------|-------------|---------------------|
| `attack orc` | Attack | `{"target": "orc"}` |
| `attack orc with fireball` | UseAbility | `{"target": "orc", "ability": "fireball"}` |
| `take sword` | TakeItem | `{"item": "sword"}` |
| `take 3 potions` | TakeItem | `{"item": "potions", "quantity": "3"}` |
| `equip sword in main hand` | EquipItem | `{"item": "sword", "slot": "main hand"}` |
| `craft longsword` | Craft | `{"recipe": "longsword"}` |

**Implementation**:
```csharp
private ParseResult ParseCombatCommand(string input, GameState state)
{
    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    // "attack orc"
    if (parts[0] == "attack" || parts[0] == "a")
    {
        if (parts.Length < 2)
        {
            return new ParseResult(
                Type: CommandType.Attack,
                Parameters: new Dictionary<string, string>(),
                IsValid: false,
                ErrorMessage: "Attack what? (Usage: attack <target>)"
            );
        }

        var target = parts[1];

        // "attack orc with fireball"
        if (parts.Length >= 4 && parts[2] == "with")
        {
            var ability = string.Join(" ", parts.Skip(3));
            return new ParseResult(
                Type: CommandType.UseAbility,
                Parameters: new Dictionary<string, string>
                {
                    ["target"] = target,
                    ["ability"] = ability
                },
                IsValid: true
            );
        }

        // Basic attack
        return new ParseResult(
            Type: CommandType.Attack,
            Parameters: new Dictionary<string, string> { ["target"] = target },
            IsValid: true
        );
    }

    // ... other combat commands
}
```

**Validation**:
```csharp
private async Task ExecuteCommand(ParseResult result, GameState state)
{
    switch (result.Type)
    {
        case CommandType.Attack:
            if (!result.Parameters.TryGetValue("target", out var target))
            {
                DisplayError("No target specified.");
                return;
            }

            var enemy = state.CurrentCombat?.Enemies.FirstOrDefault(e =>
                e.Name.Contains(target, StringComparison.OrdinalIgnoreCase));

            if (enemy == null)
            {
                DisplayError($"No enemy named '{target}' found.");
                return;
            }

            await _combatService.PlayerAttackAsync(enemy.Id);
            break;

        // ... other commands
    }
}
```

**Benefits**:
- **Natural Language**: Commands feel conversational ("attack orc with fireball")
- **Validation**: Missing parameters caught before service layer
- **Helpful Errors**: Usage hints guide players ("Usage: attack <target>")

---

## Behaviors

### B-1: ParseAndExecuteAsync - Main Entry Point

**Signature**: `Task ParseAndExecuteAsync(string input, GameState state)`

**Purpose**: Parse player input into `ParseResult`, validate, and execute corresponding game action.

**Sequence**:
```
1. Normalize input (trim, lowercase)
2. Check for special commands (help, quit)
3. Route to phase-specific parser (MainMenu, Exploration, Combat, etc.)
4. Parse input → ParseResult
5. Validate parameters (target exists? Ability known? Item in inventory?)
6. If valid → Execute via service layer
7. If invalid → Display error message with usage hint
```

**Code**:
```csharp
public async Task ParseAndExecuteAsync(string input, GameState state)
{
    var trimmed = input.Trim();
    if (string.IsNullOrWhiteSpace(trimmed))
        return;

    var lower = trimmed.ToLowerInvariant();

    // Special commands (always available)
    if (lower == "help" || lower == "h" || lower == "?")
    {
        DisplayHelp(state.Phase);
        return;
    }

    if (lower == "quit" || lower == "q" || lower == "exit")
    {
        state.Phase = GamePhase.Quit;
        return;
    }

    // Phase-based routing
    var result = state.Phase switch
    {
        GamePhase.MainMenu => ParseMainMenuCommand(lower),
        GamePhase.Exploration => ParseExplorationCommand(lower, state),
        GamePhase.Combat => ParseCombatCommand(lower, state),
        GamePhase.Inventory => ParseInventoryCommand(lower, state),
        GamePhase.Journal => ParseJournalCommand(lower, state),
        GamePhase.Crafting => ParseCraftingCommand(lower, state),
        GamePhase.Options => ParseOptionsCommand(lower, state),
        GamePhase.Rest => ParseRestCommand(lower, state),
        _ => new ParseResult(CommandType.Unknown, IsValid: false, ErrorMessage: "Unknown game phase")
    };

    // Execute or display error
    if (result.IsValid)
    {
        await ExecuteCommand(result, state);
    }
    else
    {
        AnsiConsole.MarkupLine($"[red]{result.ErrorMessage}[/]");
    }
}
```

**Performance**: Parsing + execution typically < 10ms for simple commands (navigation, observation). Complex commands (crafting with material checks) can take 50-100ms.

---

### B-2: Phase-Specific Parsers

**Purpose**: Each game phase has dedicated parsing logic for phase-appropriate commands.

#### ParseExplorationCommand

```csharp
private ParseResult ParseExplorationCommand(string input, GameState state)
{
    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var verb = parts[0];

    // Navigation (most common, optimize first)
    if (_inputConfig.IsActionBound(verb, "north"))
        return new ParseResult(CommandType.MoveNorth, IsValid: true);
    if (_inputConfig.IsActionBound(verb, "south"))
        return new ParseResult(CommandType.MoveSouth, IsValid: true);
    if (_inputConfig.IsActionBound(verb, "east"))
        return new ParseResult(CommandType.MoveEast, IsValid: true);
    if (_inputConfig.IsActionBound(verb, "west"))
        return new ParseResult(CommandType.MoveWest, IsValid: true);
    if (_inputConfig.IsActionBound(verb, "up"))
        return new ParseResult(CommandType.MoveUp, IsValid: true);
    if (_inputConfig.IsActionBound(verb, "down"))
        return new ParseResult(CommandType.MoveDown, IsValid: true);

    // Observation
    if (verb == "look" || verb == "l")
    {
        // "look" (re-display current room)
        if (parts.Length == 1)
            return new ParseResult(CommandType.Look, IsValid: true);

        // "look at altar"
        var target = string.Join(" ", parts.Skip(1)).Replace("at ", "");
        return new ParseResult(
            Type: CommandType.Examine,
            Parameters: new Dictionary<string, string> { ["target"] = target },
            IsValid: true
        );
    }

    if (verb == "examine" || verb == "x" || verb == "inspect")
    {
        if (parts.Length < 2)
        {
            return new ParseResult(
                Type: CommandType.Examine,
                IsValid: false,
                ErrorMessage: "Examine what? (Usage: examine <object>)"
            );
        }

        var target = string.Join(" ", parts.Skip(1));
        return new ParseResult(
            Type: CommandType.Examine,
            Parameters: new Dictionary<string, string> { ["target"] = target },
            IsValid: true
        );
    }

    if (verb == "search")
    {
        return new ParseResult(CommandType.Search, IsValid: true);
    }

    // Inventory Management
    if (verb == "take" || verb == "get" || verb == "grab" || verb == "pick")
    {
        if (parts.Length < 2)
        {
            return new ParseResult(
                Type: CommandType.TakeItem,
                IsValid: false,
                ErrorMessage: "Take what? (Usage: take <item>)"
            );
        }

        // "take 3 potions"
        if (int.TryParse(parts[1], out var quantity))
        {
            var item = string.Join(" ", parts.Skip(2));
            return new ParseResult(
                Type: CommandType.TakeItem,
                Parameters: new Dictionary<string, string>
                {
                    ["item"] = item,
                    ["quantity"] = quantity.ToString()
                },
                IsValid: true
            );
        }

        // "take sword"
        var itemName = string.Join(" ", parts.Skip(1));
        return new ParseResult(
            Type: CommandType.TakeItem,
            Parameters: new Dictionary<string, string> { ["item"] = itemName },
            IsValid: true
        );
    }

    // Screen Commands
    if (_inputConfig.IsActionBound(verb, "inventory"))
    {
        state.Phase = GamePhase.Inventory;
        return new ParseResult(CommandType.OpenInventory, IsValid: true);
    }

    if (_inputConfig.IsActionBound(verb, "journal"))
    {
        state.Phase = GamePhase.Journal;
        return new ParseResult(CommandType.OpenJournal, IsValid: true);
    }

    if (_inputConfig.IsActionBound(verb, "crafting"))
    {
        state.Phase = GamePhase.Crafting;
        return new ParseResult(CommandType.OpenCrafting, IsValid: true);
    }

    // Unknown command
    return new ParseResult(
        Type: CommandType.Unknown,
        IsValid: false,
        ErrorMessage: $"Unknown command: '{input}'. Type 'help' for commands."
    );
}
```

#### ParseCombatCommand

```csharp
private ParseResult ParseCombatCommand(string input, GameState state)
{
    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var verb = parts[0];

    // Attack
    if (verb == "attack" || verb == "a" || verb == "hit")
    {
        if (parts.Length < 2)
        {
            return new ParseResult(
                Type: CommandType.Attack,
                IsValid: false,
                ErrorMessage: "Attack what? (Usage: attack <target> [with <ability>])"
            );
        }

        var target = parts[1];

        // "attack orc with fireball"
        if (parts.Length >= 4 && parts[2] == "with")
        {
            var ability = string.Join(" ", parts.Skip(3));
            return new ParseResult(
                Type: CommandType.UseAbility,
                Parameters: new Dictionary<string, string>
                {
                    ["target"] = target,
                    ["ability"] = ability
                },
                IsValid: true
            );
        }

        // Basic attack
        return new ParseResult(
            Type: CommandType.Attack,
            Parameters: new Dictionary<string, string> { ["target"] = target },
            IsValid: true
        );
    }

    // Defend
    if (verb == "defend" || verb == "d" || verb == "block")
    {
        return new ParseResult(CommandType.Defend, IsValid: true);
    }

    // Flee
    if (verb == "flee" || verb == "run" || verb == "escape")
    {
        return new ParseResult(CommandType.Flee, IsValid: true);
    }

    // Use Ability (standalone)
    if (verb == "use" || verb == "cast")
    {
        if (parts.Length < 2)
        {
            return new ParseResult(
                Type: CommandType.UseAbility,
                IsValid: false,
                ErrorMessage: "Use what ability? (Usage: use <ability> [on <target>])"
            );
        }

        var ability = parts[1];
        string? target = null;

        // "use fireball on orc"
        if (parts.Length >= 4 && parts[2] == "on")
        {
            target = string.Join(" ", parts.Skip(3));
        }

        var parameters = new Dictionary<string, string> { ["ability"] = ability };
        if (target != null)
            parameters["target"] = target;

        return new ParseResult(
            Type: CommandType.UseAbility,
            Parameters: parameters,
            IsValid: true
        );
    }

    // Limited inventory access during combat
    if (verb == "inventory" || verb == "i")
    {
        state.Phase = GamePhase.Inventory;
        return new ParseResult(CommandType.OpenInventory, IsValid: true);
    }

    return new ParseResult(
        Type: CommandType.Unknown,
        IsValid: false,
        ErrorMessage: $"Unknown combat command: '{input}'. Type 'help' for commands."
    );
}
```

---

### B-3: Command Execution

**Purpose**: Route validated `ParseResult` to appropriate service layer methods.

```csharp
private async Task ExecuteCommand(ParseResult result, GameState state)
{
    switch (result.Type)
    {
        case CommandType.MoveNorth:
            await _explorationService.MoveAsync(Direction.North);
            break;

        case CommandType.MoveEast:
            await _explorationService.MoveAsync(Direction.East);
            break;

        // ... other navigation commands

        case CommandType.Look:
            _explorationService.LookAtCurrentRoom();
            break;

        case CommandType.Examine:
            var examineTarget = result.Parameters["target"];
            _explorationService.ExamineObject(examineTarget);
            break;

        case CommandType.Search:
            _explorationService.SearchRoom();
            break;

        case CommandType.TakeItem:
            var itemName = result.Parameters["item"];
            var quantity = result.Parameters.TryGetValue("quantity", out var qtyStr) ? int.Parse(qtyStr) : 1;
            await _inventoryService.TakeItemAsync(itemName, quantity);
            break;

        case CommandType.Attack:
            var attackTarget = result.Parameters["target"];
            await _combatService.PlayerAttackAsync(attackTarget);
            break;

        case CommandType.UseAbility:
            var abilityName = result.Parameters["ability"];
            var abilityTarget = result.Parameters.GetValueOrDefault("target");
            await _combatService.UseAbilityAsync(abilityName, abilityTarget);
            break;

        case CommandType.Defend:
            await _combatService.PlayerDefendAsync();
            break;

        case CommandType.Flee:
            await _combatService.AttemptFleeAsync();
            break;

        case CommandType.Craft:
            var recipeName = result.Parameters["recipe"];
            await _craftingService.CraftItemAsync(recipeName);
            break;

        case CommandType.OpenInventory:
            // Phase already set by parser
            break;

        case CommandType.OpenJournal:
            // Phase already set by parser
            break;

        case CommandType.OpenCrafting:
            // Phase already set by parser
            break;

        default:
            AnsiConsole.MarkupLine($"[red]Unhandled command type: {result.Type}[/]");
            break;
    }
}
```

**Error Handling** (Service Layer):
```csharp
// Example: _combatService.PlayerAttackAsync(target)
public async Task PlayerAttackAsync(string targetName)
{
    var enemy = _currentCombat.Enemies.FirstOrDefault(e =>
        e.Name.Contains(targetName, StringComparison.OrdinalIgnoreCase));

    if (enemy == null)
    {
        AnsiConsole.MarkupLine($"[red]No enemy named '{targetName}' found. Available targets:[/]");
        foreach (var e in _currentCombat.Enemies.Where(x => x.IsAlive))
            AnsiConsole.MarkupLine($"  - {e.Name}");
        return;
    }

    if (!enemy.IsAlive)
    {
        AnsiConsole.MarkupLine($"[red]{enemy.Name} is already defeated.[/]");
        return;
    }

    // Execute attack logic
    var damage = CalculateDamage(_player, enemy);
    enemy.CurrentHp -= damage;
    LogCombatAction($"{_player.Name} attacks {enemy.Name} for {damage} damage!");
}
```

---

## Restrictions

### R-1: Case-Insensitive Parsing
- **Rule**: All input is converted to lowercase before parsing (`input.ToLowerInvariant()`).
- **Rationale**: User-friendly (players don't need to remember capitalization).
- **Enforcement**: CommandParser normalizes all input in `ParseAndExecuteAsync()`.

### R-2: Phase-Scoped Commands
- **Rule**: Commands are only valid in specific game phases (e.g., "attack" only in Combat phase).
- **Rationale**: Prevents nonsensical actions (crafting during combat, attacking during exploration).
- **Enforcement**: Phase-specific parsers (`ParseExplorationCommand`, `ParseCombatCommand`, etc.) only recognize phase-appropriate commands.

### R-3: Synchronous Input
- **Rule**: Input prompts block until player responds (no async/non-blocking readline).
- **Rationale**: Current terminal input library (Spectre.Console) lacks async input support.
- **Limitation**: Game cannot perform background tasks during input wait (e.g., enemy AI).
- **Future Enhancement**: Implement async input with cancellation tokens for turn timers.

### R-4: Single Command Per Input
- **Rule**: CommandParser processes one command per input line (no command chaining like "north; take sword; north").
- **Rationale**: Simplifies parsing and error reporting.
- **Future Enhancement**: Allow semicolon-separated command sequences.

---

## Limitations

### L-1: No Command History
- **Issue**: Players cannot recall previous commands (no up-arrow history).
- **Workaround**: None (terminal library limitation).
- **Future Enhancement**: Implement custom readline with history buffer.

### L-2: No Tab Completion
- **Issue**: Players cannot tab-complete item names, enemy names, abilities.
- **Impact**: Typing burden for long names ("Enchanted Ring of Warding").
- **Future Enhancement**: Implement fuzzy matching for partial names ("enc ring" → "Enchanted Ring of Warding").

### L-3: No Multi-Word Target Validation
- **Issue**: Parser accepts any multi-word target without validation during parsing.
- **Example**: `attack nonexistent enemy` parses successfully but fails during execution.
- **Rationale**: Validation requires game state access (available during execution, not parsing).
- **Impact**: Error feedback occurs after parsing, not during.

### L-4: No Contextual Help
- **Issue**: `help` command shows generic help, not context-specific commands.
- **Example**: In Combat phase, `help` should show combat commands prominently.
- **Future Enhancement**: Implement `DisplayHelp(GamePhase phase)` with phase-specific command lists.

---

## Use Cases

### UC-1: Exploration - Navigation with Natural Language

**Scenario**: Player explores dungeon using cardinal direction commands.

**Actors**: Player, CommandParser, ExplorationService, GameState

**Sequence**:
```
1. Player enters "north"
2. CommandParser.ParseAndExecuteAsync("north", gameState)
3. ParseExplorationCommand("north", gameState)
4. Check InputConfigurationService.IsActionBound("north", "north") → true
5. Return ParseResult(CommandType.MoveNorth, IsValid: true)
6. ExecuteCommand() → _explorationService.MoveAsync(Direction.North)
7. ExplorationService updates current room, triggers render
```

**Code**:
```csharp
// Input: "north"
var result = ParseExplorationCommand("north", gameState);
// result = ParseResult(CommandType.MoveNorth, IsValid: true)

await ExecuteCommand(result, gameState);
// Calls: _explorationService.MoveAsync(Direction.North)

// ExplorationService.MoveAsync()
public async Task MoveAsync(Direction direction)
{
    var currentRoom = _gameState.CurrentRoom;
    var exit = currentRoom.Exits.FirstOrDefault(e => e.Direction == direction);

    if (exit == null)
    {
        AnsiConsole.MarkupLine("[red]You cannot go that way.[/]");
        return;
    }

    var nextRoom = await _roomRepository.GetByIdAsync(exit.DestinationRoomId);
    _gameState.CurrentRoom = nextRoom;

    AnsiConsole.MarkupLine($"[cyan]You travel {direction.ToString().ToLower()}...[/]");
    LookAtCurrentRoom(); // Auto-describe new room
}
```

**Alternative Inputs** (all equivalent):
- `north`
- `n`
- `go north`
- `8` (numpad binding)

**Validation**: Direction command only succeeds if exit exists in current room.

---

### UC-2: Combat - Attack with Ability Specification

**Scenario**: Player attacks enemy using specific ability ("attack orc with fireball").

**Actors**: Player, CommandParser, CombatService, GameState

**Sequence**:
```
1. Player enters "attack orc with fireball"
2. CommandParser.ParseAndExecuteAsync("attack orc with fireball", gameState)
3. ParseCombatCommand("attack orc with fireball", gameState)
4. Split input: ["attack", "orc", "with", "fireball"]
5. Detect "with" keyword → UseAbility command (not basic attack)
6. Extract target="orc", ability="fireball"
7. Return ParseResult(CommandType.UseAbility, Parameters: {target: "orc", ability: "fireball"}, IsValid: true)
8. ExecuteCommand() → _combatService.UseAbilityAsync("fireball", "orc")
9. CombatService validates ability known, applies damage, logs action
```

**Code**:
```csharp
// Input: "attack orc with fireball"
var parts = input.Split(' '); // ["attack", "orc", "with", "fireball"]
var target = parts[1]; // "orc"
var ability = string.Join(" ", parts.Skip(3)); // "fireball"

var result = new ParseResult(
    Type: CommandType.UseAbility,
    Parameters: new Dictionary<string, string>
    {
        ["target"] = target,
        ["ability"] = ability
    },
    IsValid: true
);

await ExecuteCommand(result, gameState);
// Calls: _combatService.UseAbilityAsync("fireball", "orc")

// CombatService.UseAbilityAsync()
public async Task UseAbilityAsync(string abilityName, string? targetName)
{
    var ability = _player.Abilities.FirstOrDefault(a =>
        a.Name.Contains(abilityName, StringComparison.OrdinalIgnoreCase));

    if (ability == null)
    {
        AnsiConsole.MarkupLine($"[red]You don't know an ability named '{abilityName}'.[/]");
        return;
    }

    if (_player.CurrentStamina < ability.StaminaCost)
    {
        AnsiConsole.MarkupLine($"[red]Not enough stamina. {ability.Name} costs {ability.StaminaCost}, you have {_player.CurrentStamina}.[/]");
        return;
    }

    var target = _currentCombat.Enemies.FirstOrDefault(e =>
        e.Name.Contains(targetName, StringComparison.OrdinalIgnoreCase));

    if (target == null)
    {
        AnsiConsole.MarkupLine($"[red]No enemy named '{targetName}' found.[/]");
        return;
    }

    // Execute ability
    var damage = ability.BaseDamage + (_player.Wits * ability.WitsScaling);
    target.CurrentHp -= damage;
    _player.CurrentStamina -= ability.StaminaCost;

    LogCombatAction($"{_player.Name} uses {ability.Name} on {target.Name} for {damage} damage!");
}
```

**Validation Steps**:
1. Ability exists in player's known abilities
2. Player has sufficient stamina
3. Target exists in current combat
4. Target is alive

**Alternative Syntax**:
- `attack orc with fireball` → UseAbility
- `use fireball on orc` → UseAbility (equivalent)
- `cast fireball on orc` → UseAbility (equivalent)

---

### UC-3: Inventory - Multi-Word Item Names with Quantity

**Scenario**: Player picks up multiple items with compound names ("take 3 healing potions").

**Actors**: Player, CommandParser, InventoryService, GameState

**Sequence**:
```
1. Player enters "take 3 healing potions"
2. CommandParser.ParseAndExecuteAsync("take 3 healing potions", gameState)
3. ParseExplorationCommand("take 3 healing potions", gameState)
4. Split input: ["take", "3", "healing", "potions"]
5. Detect numeric quantity (parts[1] = "3")
6. Extract item name: "healing potions" (parts.Skip(2))
7. Return ParseResult(CommandType.TakeItem, Parameters: {item: "healing potions", quantity: "3"}, IsValid: true)
8. ExecuteCommand() → _inventoryService.TakeItemAsync("healing potions", 3)
9. InventoryService validates item exists in room, adds to player inventory
```

**Code**:
```csharp
// Input: "take 3 healing potions"
var parts = input.Split(' '); // ["take", "3", "healing", "potions"]

if (int.TryParse(parts[1], out var quantity))
{
    var itemName = string.Join(" ", parts.Skip(2)); // "healing potions"

    var result = new ParseResult(
        Type: CommandType.TakeItem,
        Parameters: new Dictionary<string, string>
        {
            ["item"] = itemName,
            ["quantity"] = quantity.ToString()
        },
        IsValid: true
    );

    await ExecuteCommand(result, gameState);
}

// InventoryService.TakeItemAsync()
public async Task TakeItemAsync(string itemName, int quantity)
{
    var roomItems = _gameState.CurrentRoom.Items;
    var item = roomItems.FirstOrDefault(i =>
        i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase));

    if (item == null)
    {
        AnsiConsole.MarkupLine($"[red]No item named '{itemName}' found here.[/]");
        return;
    }

    if (item.Quantity < quantity)
    {
        AnsiConsole.MarkupLine($"[red]Only {item.Quantity} {item.Name} available. Cannot take {quantity}.[/]");
        return;
    }

    // Check encumbrance
    var totalWeight = item.Weight * quantity;
    if (_player.CurrentWeight + totalWeight > _player.MaxWeight)
    {
        AnsiConsole.MarkupLine($"[red]Too heavy! This would exceed your carry capacity.[/]");
        return;
    }

    // Add to inventory
    item.Quantity -= quantity;
    var playerItem = _player.Inventory.FirstOrDefault(i => i.Id == item.Id);
    if (playerItem != null)
    {
        playerItem.Quantity += quantity;
    }
    else
    {
        _player.Inventory.Add(new InventoryItem(item.Id, item.Name, quantity, item.Weight));
    }

    _player.CurrentWeight += totalWeight;

    AnsiConsole.MarkupLine($"[green]You take {quantity} {item.Name}.[/]");
}
```

**Validation Steps**:
1. Item exists in current room
2. Sufficient quantity available
3. Player has carrying capacity

**Alternative Syntax**:
- `take 3 healing potions` → Quantity: 3
- `get healing potion` → Quantity: 1 (default)
- `grab 5 arrows` → Quantity: 5

---

### UC-4: Crafting - Recipe Selection with Trade Context

**Scenario**: Player attempts to craft item ("craft longsword").

**Actors**: Player, CommandParser, CraftingService, GameState

**Sequence**:
```
1. Player opens crafting screen (GamePhase.Crafting)
2. Player enters "craft longsword"
3. CommandParser.ParseAndExecuteAsync("craft longsword", gameState)
4. ParseCraftingCommand("craft longsword", gameState)
5. Extract recipe name: "longsword"
6. Return ParseResult(CommandType.Craft, Parameters: {recipe: "longsword"}, IsValid: true)
7. ExecuteCommand() → _craftingService.CraftItemAsync("longsword")
8. CraftingService validates recipe known, materials available, crafting skill sufficient
9. If valid, consume materials, create item, award XP
```

**Code**:
```csharp
// Input: "craft longsword"
var parts = input.Split(' '); // ["craft", "longsword"]
var recipeName = string.Join(" ", parts.Skip(1)); // "longsword"

var result = new ParseResult(
    Type: CommandType.Craft,
    Parameters: new Dictionary<string, string> { ["recipe"] = recipeName },
    IsValid: true
);

await ExecuteCommand(result, gameState);

// CraftingService.CraftItemAsync()
public async Task CraftItemAsync(string recipeName)
{
    var recipe = _allRecipes.FirstOrDefault(r =>
        r.Name.Contains(recipeName, StringComparison.OrdinalIgnoreCase));

    if (recipe == null)
    {
        AnsiConsole.MarkupLine($"[red]No recipe named '{recipeName}' found.[/]");
        return;
    }

    if (!_player.KnownRecipes.Contains(recipe.Id))
    {
        AnsiConsole.MarkupLine($"[red]You don't know how to craft {recipe.Name}.[/]");
        return;
    }

    // Check materials
    foreach (var material in recipe.Materials)
    {
        var playerMaterial = _player.Inventory.FirstOrDefault(i => i.Id == material.Id);
        if (playerMaterial == null || playerMaterial.Quantity < material.Quantity)
        {
            AnsiConsole.MarkupLine($"[red]Missing material: {material.Name} x{material.Quantity}[/]");
            return;
        }
    }

    // Check skill
    if (_player.CraftingSkill < recipe.RequiredSkill)
    {
        AnsiConsole.MarkupLine($"[red]Insufficient crafting skill. Requires {recipe.RequiredSkill}, you have {_player.CraftingSkill}.[/]");
        return;
    }

    // Consume materials
    foreach (var material in recipe.Materials)
    {
        var playerMaterial = _player.Inventory.First(i => i.Id == material.Id);
        playerMaterial.Quantity -= material.Quantity;
        if (playerMaterial.Quantity == 0)
            _player.Inventory.Remove(playerMaterial);
    }

    // Create item
    var craftedItem = await _itemFactory.CreateFromRecipeAsync(recipe);
    _player.Inventory.Add(craftedItem);

    // Award XP
    var xpGain = recipe.Complexity * 10;
    _player.ExperiencePoints += xpGain;

    AnsiConsole.MarkupLine($"[green]You successfully craft {craftedItem.Name}! (+{xpGain} XP)[/]");
}
```

**Validation Steps**:
1. Recipe exists in database
2. Player knows recipe (learned via quest/trainer)
3. Player has all required materials
4. Player has sufficient crafting skill

---

### UC-5: Error Handling - Missing Parameter with Usage Hint

**Scenario**: Player enters incomplete command ("attack" without target).

**Actors**: Player, CommandParser

**Sequence**:
```
1. Player enters "attack"
2. CommandParser.ParseAndExecuteAsync("attack", gameState)
3. ParseCombatCommand("attack", gameState)
4. Split input: ["attack"]
5. Detect missing target (parts.Length < 2)
6. Return ParseResult(CommandType.Attack, IsValid: false, ErrorMessage: "Attack what? (Usage: attack <target>)")
7. Display error message to player
```

**Code**:
```csharp
// Input: "attack"
var parts = input.Split(' '); // ["attack"]

if (parts.Length < 2)
{
    return new ParseResult(
        Type: CommandType.Attack,
        Parameters: new Dictionary<string, string>(),
        IsValid: false,
        ErrorMessage: "Attack what? (Usage: attack <target>)"
    );
}

// In ParseAndExecuteAsync()
if (!result.IsValid)
{
    AnsiConsole.MarkupLine($"[red]{result.ErrorMessage}[/]");
    return;
}
```

**Output**:
```
> attack
Attack what? (Usage: attack <target>)
```

**Common Error Scenarios**:
| Input | Error Message |
|-------|---------------|
| `attack` | Attack what? (Usage: attack <target>) |
| `examine` | Examine what? (Usage: examine <object>) |
| `take` | Take what? (Usage: take <item>) |
| `craft` | Craft what? (Usage: craft <recipe>) |
| `use` | Use what ability? (Usage: use <ability> [on <target>]) |

---

### UC-6: Keybinding - Custom Hotkey Recognition

**Scenario**: Player has customized keybindings (e.g., "8" for north, "2" for south).

**Actors**: Player, InputConfigurationService, CommandParser

**Sequence**:
```
1. Player presses "8"
2. CommandParser.ParseAndExecuteAsync("8", gameState)
3. InputConfigurationService.IsActionBound("8", "north") → true
4. Return ParseResult(CommandType.MoveNorth, IsValid: true)
5. ExecuteCommand() → _explorationService.MoveAsync(Direction.North)
```

**Configuration** (`data/input_bindings.json`):
```json
{
  "navigation": {
    "north": ["n", "north", "8"],
    "south": ["s", "south", "2"],
    "east": ["e", "east", "6"],
    "west": ["w", "west", "4"]
  }
}
```

**Code**:
```csharp
// InputConfigurationService.IsActionBound("8", "north")
public bool IsActionBound(string input, string action)
{
    var bindings = GetBindings(action); // ["n", "north", "8"]
    return bindings.Any(b => b.Equals(input, StringComparison.OrdinalIgnoreCase));
}

// In ParseExplorationCommand()
if (_inputConfig.IsActionBound(verb, "north"))
    return new ParseResult(CommandType.MoveNorth, IsValid: true);
```

**Benefits**:
- Players with numpads can use 8/2/4/6 for cardinal navigation
- Custom bindings support accessibility needs (e.g., one-handed play)

---

## Decision Trees

### DT-1: Command Parsing Flow

**Trigger**: Player submits input

```
Input String
│
├─ Normalize (trim, lowercase)
│
├─ Empty? → Return (ignore)
│
├─ Special Commands (always available)?
│  ├─ "help"/"h"/"?" → DisplayHelp(currentPhase)
│  ├─ "quit"/"q"/"exit" → Set GamePhase.Quit
│  └─ NO → Continue
│
├─ Route by GamePhase
│  ├─ MainMenu → ParseMainMenuCommand()
│  ├─ Exploration → ParseExplorationCommand()
│  ├─ Combat → ParseCombatCommand()
│  ├─ Inventory → ParseInventoryCommand()
│  ├─ Journal → ParseJournalCommand()
│  ├─ Crafting → ParseCraftingCommand()
│  ├─ Options → ParseOptionsCommand()
│  ├─ Rest → ParseRestCommand()
│  └─ Unknown Phase → Error
│
├─ Phase-Specific Parser Returns ParseResult
│  ├─ IsValid = true?
│  │  ├─ YES → ExecuteCommand(result, gameState)
│  │  │  ├─ Route by CommandType
│  │  │  │  ├─ MoveNorth → ExplorationService.MoveAsync(Direction.North)
│  │  │  │  ├─ Attack → CombatService.PlayerAttackAsync(target)
│  │  │  │  ├─ TakeItem → InventoryService.TakeItemAsync(item, quantity)
│  │  │  │  └─ ... (20+ command types)
│  │  │  └─ Service executes game logic
│  │  └─ NO → Display error message (result.ErrorMessage)
│  └─ Return to input loop
```

---

### DT-2: Exploration Command Parsing

**Trigger**: ParseExplorationCommand(input, gameState) called

```
Input String (lowercase)
│
├─ Check Keybindings (InputConfigurationService)
│  ├─ IsActionBound("north")? → CommandType.MoveNorth
│  ├─ IsActionBound("south")? → CommandType.MoveSouth
│  ├─ IsActionBound("east")? → CommandType.MoveEast
│  ├─ IsActionBound("west")? → CommandType.MoveWest
│  ├─ IsActionBound("up")? → CommandType.MoveUp
│  ├─ IsActionBound("down")? → CommandType.MoveDown
│  └─ NO → Continue
│
├─ Check Observation Commands
│  ├─ "look"/"l"?
│  │  ├─ 1 word ("look") → CommandType.Look (re-describe room)
│  │  └─ 2+ words ("look at altar") → CommandType.Examine, target="altar"
│  ├─ "examine"/"x"/"inspect"?
│  │  ├─ 1 word → Error: "Examine what? (Usage: examine <object>)"
│  │  └─ 2+ words → CommandType.Examine, target=words[1..]
│  └─ "search"? → CommandType.Search
│
├─ Check Inventory Commands
│  ├─ "take"/"get"/"grab"/"pick"?
│  │  ├─ 1 word → Error: "Take what? (Usage: take <item>)"
│  │  ├─ 2 words, word[1] is number?
│  │  │  ├─ YES → CommandType.TakeItem, quantity=word[1], item=word[2..]
│  │  │  └─ NO → CommandType.TakeItem, quantity=1, item=word[1..]
│  │  └─ 2+ words → CommandType.TakeItem, item=word[1..]
│  ├─ "drop"/"discard"?
│  │  ├─ 1 word → Error: "Drop what?"
│  │  └─ 2+ words → CommandType.DropItem, item=word[1..]
│  └─ "equip"/"bind"/"wear"?
│     ├─ 1 word → Error: "Equip what?"
│     └─ 2+ words → CommandType.EquipItem, item=word[1..]
│
├─ Check Screen Commands
│  ├─ IsActionBound("inventory")? → Set GamePhase.Inventory, CommandType.OpenInventory
│  ├─ IsActionBound("journal")? → Set GamePhase.Journal, CommandType.OpenJournal
│  ├─ IsActionBound("crafting")? → Set GamePhase.Crafting, CommandType.OpenCrafting
│  └─ IsActionBound("options")? → Set GamePhase.Options, CommandType.OpenOptions
│
└─ Unknown Command → Error: "Unknown command: '<input>'. Type 'help' for commands."
```

---

### DT-3: Combat Command Parsing

**Trigger**: ParseCombatCommand(input, gameState) called

```
Input String (lowercase)
│
├─ "attack"/"a"/"hit"?
│  ├─ 1 word → Error: "Attack what? (Usage: attack <target> [with <ability>])"
│  ├─ 2 words ("attack orc") → CommandType.Attack, target="orc"
│  ├─ 4+ words with "with" keyword ("attack orc with fireball")?
│  │  ├─ YES → CommandType.UseAbility, target="orc", ability="fireball"
│  │  └─ NO → CommandType.Attack, target="orc"
│
├─ "defend"/"d"/"block"?
│  └─ CommandType.Defend (no parameters)
│
├─ "flee"/"run"/"escape"?
│  └─ CommandType.Flee (no parameters)
│
├─ "use"/"cast"?
│  ├─ 1 word → Error: "Use what ability? (Usage: use <ability> [on <target>])"
│  ├─ 2 words ("use fireball") → CommandType.UseAbility, ability="fireball", target=null (auto-target)
│  ├─ 4+ words with "on" keyword ("use fireball on orc")?
│  │  ├─ YES → CommandType.UseAbility, ability="fireball", target="orc"
│  │  └─ NO → CommandType.UseAbility, ability=word[1], target=null
│
├─ "inventory"/"i"?
│  └─ Set GamePhase.Inventory, CommandType.OpenInventory (limited inventory access during combat)
│
└─ Unknown Command → Error: "Unknown combat command: '<input>'. Type 'help' for commands."
```

---

## Cross-Links

### Dependencies (Systems SPEC-INPUT-001 relies on)

1. **SPEC-UI-001 (UI Framework System)**
   - **Relationship**: CommandParser modifies GameState → Triggers ViewModel rebuild → Re-render
   - **Integration Point**: Every successful command execution changes GameState.Phase or game data
   - **Example**: `ParseAndExecuteAsync()` executes service method → Service updates GameState → GameService detects change → Renders updated screen

2. **SPEC-COMBAT-001 (Combat System)**
   - **Relationship**: Combat commands route to CombatService methods
   - **Integration Point**: ParseCombatCommand() generates ParseResult → ExecuteCommand() calls CombatService
   - **Example**: `attack orc` → `_combatService.PlayerAttackAsync("orc")`

---

### Dependents (Systems that rely on SPEC-INPUT-001)

1. **GameService (Game Loop Orchestration)**
   - **Relationship**: Game loop blocks on input, executes command, re-renders
   - **Integration Point**: `while` loop in `GameService.RunAsync()` calls `_inputHandler.GetInput()` → `_parser.ParseAndExecuteAsync()`
   - **File**: RuneAndRust.Engine/Services/GameService.cs (lines 60-99)

---

### Related Systems

1. **SPEC-RENDER-001 (Rendering Pipeline System)**
   - **Relationship**: Commands trigger state changes → Screen re-renders
   - **Integration Point**: Successful command execution → GameState modified → GameService triggers renderer
   - **Example**: `north` command → Room changes → ExplorationScreenRenderer.Render() called

2. **SPEC-THEME-001 (Theme System)**
   - **Relationship**: Input prompts use theme colors
   - **Integration Point**: `AnsiConsole.Markup()` uses theme colors for prompt text
   - **Example**: `[$"{_theme.GetColor("PromptColor")}]> [/]"`

---

## Related Services

### Core Services (from RuneAndRust.Engine/Services/)

1. **CommandParser** (RuneAndRust.Engine/Services/CommandParser.cs)
   - **Lines**: 882
   - **Key Methods**:
     - `Task ParseAndExecuteAsync(string input, GameState state)`
     - `ParseResult ParseExplorationCommand(string input, GameState state)`
     - `ParseResult ParseCombatCommand(string input, GameState state)`
     - `Task ExecuteCommand(ParseResult result, GameState state)`

2. **InputConfigurationService** (RuneAndRust.Engine/Services/InputConfigurationService.cs)
   - **Key Methods**:
     - `List<string> GetBindings(string action)`
     - `bool IsActionBound(string input, string action)`
     - `Dictionary<string, List<string>> LoadBindingsFromJson(string path)`

### Interface Implementations (from RuneAndRust.Terminal/Services/)

3. **IInputHandler** (RuneAndRust.Core/Interfaces/IInputHandler.cs)
   - **Implementation**: TerminalInputHandler (RuneAndRust.Terminal/Services/TerminalInputHandler.cs)
   - **Key Methods**:
     - `string GetInput(string prompt)`
     - `Task<string> GetInputAsync(string prompt, CancellationToken ct)`

### Supporting Services (Consumed by CommandParser)

4. **IExplorationService** (RuneAndRust.Core/Interfaces/IExplorationService.cs)
   - **Methods Called**:
     - `Task MoveAsync(Direction direction)`
     - `void LookAtCurrentRoom()`
     - `void ExamineObject(string objectName)`
     - `void SearchRoom()`

5. **ICombatService** (RuneAndRust.Core/Interfaces/ICombatService.cs)
   - **Methods Called**:
     - `Task PlayerAttackAsync(string targetName)`
     - `Task UseAbilityAsync(string abilityName, string? targetName)`
     - `Task PlayerDefendAsync()`
     - `Task AttemptFleeAsync()`

6. **IInventoryService** (RuneAndRust.Core/Interfaces/IInventoryService.cs)
   - **Methods Called**:
     - `Task TakeItemAsync(string itemName, int quantity)`
     - `Task DropItemAsync(string itemName, int quantity)`
     - `Task EquipItemAsync(string itemName, string? slot)`
     - `Task UnequipItemAsync(string slot)`
     - `Task UseItemAsync(string itemName)`

7. **ICraftingService** (RuneAndRust.Core/Interfaces/ICraftingService.cs)
   - **Methods Called**:
     - `Task CraftItemAsync(string recipeName)`
     - `void SelectTrade(CraftingTrade trade)`

---

## Data Models

### ParseResult

**Definition**: Represents parsed player intention (not executed action).

```csharp
public record ParseResult(
    CommandType Type,
    Dictionary<string, string> Parameters,
    bool IsValid,
    string? ErrorMessage = null
);
```

**Fields**:
- `Type` (CommandType): Enum representing command category (Attack, Move, TakeItem, etc.)
- `Parameters` (Dictionary<string, string>): Key-value pairs for command arguments (target, ability, item, quantity, etc.)
- `IsValid` (bool): Whether command syntax is valid (true = execute, false = display error)
- `ErrorMessage` (string?): Helpful error message with usage hint (e.g., "Attack what? (Usage: attack <target>)")

**Examples**:
```csharp
// Valid attack command
new ParseResult(
    Type: CommandType.Attack,
    Parameters: new Dictionary<string, string> { ["target"] = "orc" },
    IsValid: true
);

// Invalid (missing parameter)
new ParseResult(
    Type: CommandType.Attack,
    Parameters: new Dictionary<string, string>(),
    IsValid: false,
    ErrorMessage: "Attack what? (Usage: attack <target>)"
);
```

---

### CommandType Enum

**Definition**: Enum of all recognized command types.

```csharp
public enum CommandType
{
    // Navigation
    MoveNorth,
    MoveSouth,
    MoveEast,
    MoveWest,
    MoveUp,
    MoveDown,

    // Observation
    Look,
    Examine,
    Search,

    // Inventory
    TakeItem,
    DropItem,
    EquipItem,
    UnequipItem,
    UseItem,

    // Combat
    Attack,
    Defend,
    UseAbility,
    Flee,

    // Crafting
    Craft,
    OpenCrafting,
    SelectTrade,

    // Screens
    OpenInventory,
    OpenJournal,
    OpenOptions,
    OpenRest,

    // System
    Help,
    Quit,
    Back,
    Unknown
}
```

**Total**: 25+ command types

---

### Input Binding Configuration (JSON)

**File**: `data/input_bindings.json`

```json
{
  "navigation": {
    "north": ["n", "north", "go north", "8"],
    "south": ["s", "south", "go south", "2"],
    "east": ["e", "east", "go east", "6"],
    "west": ["w", "west", "go west", "4"],
    "up": ["u", "up", "ascend", "9"],
    "down": ["d", "down", "descend", "3"]
  },
  "screens": {
    "inventory": ["i", "inventory", "inv"],
    "journal": ["j", "journal", "log", "quest"],
    "crafting": ["c", "crafting", "craft"],
    "options": ["o", "options", "settings"]
  },
  "combat": {
    "attack": ["a", "attack", "hit", "enter"],
    "defend": ["d", "defend", "block", "space"],
    "flee": ["f", "flee", "run", "escape", "esc"]
  },
  "observation": {
    "look": ["l", "look"],
    "examine": ["x", "examine", "inspect"],
    "search": ["search", "look around"]
  },
  "system": {
    "help": ["h", "help", "?"],
    "quit": ["q", "quit", "exit"],
    "back": ["back", "return", "esc"]
  }
}
```

---

## Configuration

### DI Registration (from RuneAndRust.Terminal/Program.cs)

```csharp
// Input Services (Singleton)
services.AddSingleton<IInputHandler, TerminalInputHandler>();
services.AddSingleton<CommandParser>();
services.AddSingleton<InputConfigurationService>();
```

**Lifetime Justification**:
- **Singleton**: CommandParser has no mutable state (all state in GameState parameter)
- **Singleton**: InputConfigurationService loads keybindings once at startup
- **Singleton**: TerminalInputHandler is stateless wrapper around Spectre.Console

---

### Keybindings Configuration Path

**Default Location**: `data/input_bindings.json`

**Customization**:
- Players can manually edit JSON file
- Future enhancement: In-game keybinding editor in Options screen

---

## Testing

### Unit Testing Strategy

**Test Coverage Target**: 70% (CommandParser is critical path for all player actions)

**Testable Components**:
1. **ParseResult Generation** - Verify correct CommandType and Parameters for all input patterns
2. **Alias Resolution** - Verify all aliases map to correct CommandType
3. **Error Handling** - Verify missing parameters produce helpful error messages
4. **Phase Routing** - Verify commands only accepted in appropriate phases

**Non-Testable Components**:
- Terminal input interaction (requires manual testing)
- End-to-end command execution (requires integration tests)

### Example Test: ParseExplorationCommand

**File**: RuneAndRust.Tests/Services/CommandParserTests.cs

```csharp
public class CommandParserTests
{
    private readonly CommandParser _parser;
    private readonly GameState _testState;

    public CommandParserTests()
    {
        // Mock dependencies
        var mockInputConfig = Substitute.For<InputConfigurationService>();
        mockInputConfig.IsActionBound("north", "north").Returns(true);
        mockInputConfig.IsActionBound("n", "north").Returns(true);
        // ... configure all bindings

        _parser = new CommandParser(mockInputConfig, /* ... other services */);
        _testState = new GameState { Phase = GamePhase.Exploration };
    }

    [Theory]
    [InlineData("north", CommandType.MoveNorth)]
    [InlineData("n", CommandType.MoveNorth)]
    [InlineData("south", CommandType.MoveSouth)]
    [InlineData("s", CommandType.MoveSouth)]
    [InlineData("east", CommandType.MoveEast)]
    [InlineData("e", CommandType.MoveEast)]
    [InlineData("west", CommandType.MoveWest)]
    [InlineData("w", CommandType.MoveWest)]
    public void ParseExplorationCommand_NavigationAliases_ReturnsCorrectCommandType(string input, CommandType expected)
    {
        // Act
        var result = _parser.ParseExplorationCommand(input, _testState);

        // Assert
        Assert.Equal(expected, result.Type);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ParseExplorationCommand_Look_ReturnsLookCommand()
    {
        // Act
        var result = _parser.ParseExplorationCommand("look", _testState);

        // Assert
        Assert.Equal(CommandType.Look, result.Type);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ParseExplorationCommand_LookAtObject_ReturnsExamineCommand()
    {
        // Act
        var result = _parser.ParseExplorationCommand("look at altar", _testState);

        // Assert
        Assert.Equal(CommandType.Examine, result.Type);
        Assert.Equal("altar", result.Parameters["target"]);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("examine sword", "sword")]
    [InlineData("x broken vase", "broken vase")]
    [InlineData("inspect ancient altar", "ancient altar")]
    public void ParseExplorationCommand_Examine_ExtractsTarget(string input, string expectedTarget)
    {
        // Act
        var result = _parser.ParseExplorationCommand(input, _testState);

        // Assert
        Assert.Equal(CommandType.Examine, result.Type);
        Assert.Equal(expectedTarget, result.Parameters["target"]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ParseExplorationCommand_ExamineNoTarget_ReturnsError()
    {
        // Act
        var result = _parser.ParseExplorationCommand("examine", _testState);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Examine what?", result.ErrorMessage);
    }

    [Theory]
    [InlineData("take sword", "sword", "1")]
    [InlineData("take 3 potions", "potions", "3")]
    [InlineData("get healing potion", "healing potion", "1")]
    [InlineData("grab 5 iron ingots", "iron ingots", "5")]
    public void ParseExplorationCommand_TakeItem_ExtractsItemAndQuantity(string input, string expectedItem, string expectedQuantity)
    {
        // Act
        var result = _parser.ParseExplorationCommand(input, _testState);

        // Assert
        Assert.Equal(CommandType.TakeItem, result.Type);
        Assert.Equal(expectedItem, result.Parameters["item"]);
        Assert.Equal(expectedQuantity, result.Parameters.GetValueOrDefault("quantity", "1"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ParseExplorationCommand_TakeNoItem_ReturnsError()
    {
        // Act
        var result = _parser.ParseExplorationCommand("take", _testState);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Take what?", result.ErrorMessage);
    }

    [Fact]
    public void ParseExplorationCommand_UnknownCommand_ReturnsError()
    {
        // Act
        var result = _parser.ParseExplorationCommand("gibberish", _testState);

        // Assert
        Assert.Equal(CommandType.Unknown, result.Type);
        Assert.False(result.IsValid);
        Assert.Contains("Unknown command", result.ErrorMessage);
    }
}
```

### Example Test: ParseCombatCommand

**File**: RuneAndRust.Tests/Services/CommandParserCombatTests.cs

```csharp
public class CommandParserCombatTests
{
    private readonly CommandParser _parser;
    private readonly GameState _testState;

    public CommandParserCombatTests()
    {
        _parser = new CommandParser(/* dependencies */);
        _testState = new GameState { Phase = GamePhase.Combat };
    }

    [Theory]
    [InlineData("attack orc", "orc")]
    [InlineData("a goblin", "goblin")]
    [InlineData("hit troll", "troll")]
    public void ParseCombatCommand_BasicAttack_ExtractsTarget(string input, string expectedTarget)
    {
        // Act
        var result = _parser.ParseCombatCommand(input, _testState);

        // Assert
        Assert.Equal(CommandType.Attack, result.Type);
        Assert.Equal(expectedTarget, result.Parameters["target"]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ParseCombatCommand_AttackNoTarget_ReturnsError()
    {
        // Act
        var result = _parser.ParseCombatCommand("attack", _testState);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Attack what?", result.ErrorMessage);
    }

    [Theory]
    [InlineData("attack orc with fireball", "orc", "fireball")]
    [InlineData("attack goblin with ice shard", "goblin", "ice shard")]
    public void ParseCombatCommand_AttackWithAbility_ExtractsTargetAndAbility(string input, string expectedTarget, string expectedAbility)
    {
        // Act
        var result = _parser.ParseCombatCommand(input, _testState);

        // Assert
        Assert.Equal(CommandType.UseAbility, result.Type);
        Assert.Equal(expectedTarget, result.Parameters["target"]);
        Assert.Equal(expectedAbility, result.Parameters["ability"]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ParseCombatCommand_Defend_ReturnsDefendCommand()
    {
        // Act
        var result = _parser.ParseCombatCommand("defend", _testState);

        // Assert
        Assert.Equal(CommandType.Defend, result.Type);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ParseCombatCommand_Flee_ReturnsFleeCommand()
    {
        // Act
        var result = _parser.ParseCombatCommand("flee", _testState);

        // Assert
        Assert.Equal(CommandType.Flee, result.Type);
        Assert.True(result.IsValid);
    }
}
```

---

## Design Rationale

### DR-1: Why ParseResult Pattern (Not Direct Execution)?

**Decision**: Commands are parsed to `ParseResult` objects (intentions), not executed during parsing.

**Alternatives Considered**:
1. **Direct Execution**: Parser calls service methods directly during parsing
2. **Command Objects**: Each command is a class with `Execute()` method (Command Pattern)

**Rationale for ParseResult**:
- **Testability**: Parsing logic can be tested independently (no mocking of 50+ service methods)
- **Validation**: Parameter validation occurs before service layer (clearer error separation)
- **Flexibility**: ParseResult can be logged, queued, or replayed (future multiplayer support)
- **Simplicity**: Single data structure (ParseResult) vs. 25+ command classes

**Trade-Offs**:
- **Boilerplate**: Requires `ExecuteCommand()` switch statement (25+ cases)
- **Type Safety**: Parameters stored as `Dictionary<string, string>` (not strongly typed command objects)

**Why Not Command Pattern?**
- Overhead of 25+ command classes (Attack, Defend, TakeItem, etc.)
- Harder to serialize/log than simple ParseResult record
- More complex DI setup (each command needs service dependencies)

---

### DR-2: Why Phase-Based Command Routing?

**Decision**: Different game phases (Exploration, Combat, Crafting) have dedicated parsers.

**Alternatives Considered**:
1. **Single Parser**: One giant parser handles all commands, validates phase internally
2. **Context Object**: Pass context flags to single parser (isCombat, isCrafting, etc.)

**Rationale for Phase-Based Routing**:
- **Code Organization**: Each parser is 100-200 lines (readable), not 1000+ line monolith
- **Error Messages**: Phase-specific error messages ("Cannot craft during combat")
- **Performance**: Early rejection of invalid commands (no combat parsing during exploration)
- **Maintainability**: Adding new phase (e.g., "Trading") requires new parser, not modifying giant switch

**Trade-Offs**:
- **Duplication**: Some commands appear in multiple parsers (e.g., "inventory" in Exploration and Combat)
- **Complexity**: Requires GamePhase enum and routing switch in `ParseAndExecuteAsync()`

---

### DR-3: Why Natural Language Aliases?

**Decision**: Support multiple command variants ("take"/"get"/"grab", "equip"/"bind"/"wear").

**Alternatives Considered**:
1. **Single Canonical Syntax**: Force players to use exact commands ("take" only, not "get")
2. **Regex Matching**: Use regex patterns to match variations

**Rationale for Aliases**:
- **User-Friendly**: Players can use natural language without memorizing exact syntax
- **Accessibility**: Shorter aliases ("n" vs "north") reduce typing burden
- **Discoverability**: Players can try intuitive commands and likely succeed

**Implementation**:
- **Dictionary Lookup**: `CommandAliases` dictionary maps all variants to CommandType
- **Partial Matching**: `StartsWith()` matching supports lazy typing ("tak" → "take")

**Trade-Offs**:
- **Maintenance**: Aliases dictionary requires updates when adding commands
- **Ambiguity**: Partial matching can match wrong command ("d" could be "down" or "defend")

---

## Changelog

### Version 1.0.0 (2025-01-XX) - Initial Specification

**Added**:
- Comprehensive command parsing system documentation (CommandParser, 882 lines)
- ParseResult pattern specification (intention objects, not executed actions)
- Phase-based command routing (MainMenu, Exploration, Combat, Crafting, etc.)
- Natural language alias system (20+ command variants)
- Keybinding configuration system (JSON-based, customizable hotkeys)
- Parameter extraction for multi-word commands (targets, abilities, quantities)
- 6 detailed use cases (Navigation, Combat, Inventory, Crafting, Errors, Keybindings)
- 3 decision trees (Command parsing flow, Exploration parsing, Combat parsing)
- Testing strategy with example unit tests (CommandParserTests, 70% coverage target)

**Documented Implementation**:
- CommandParser (RuneAndRust.Engine/Services/CommandParser.cs)
- TerminalInputHandler (RuneAndRust.Terminal/Services/TerminalInputHandler.cs)
- InputConfigurationService (RuneAndRust.Engine/Services/InputConfigurationService.cs)
- 25+ CommandType enum values
- Phase-specific parsers (8 game phases)

---

## Future Enhancements

### FE-1: Command History

**Problem**: Players cannot recall previous commands (no up-arrow history).

**Proposed Solution**:
```csharp
public class CommandHistoryService
{
    private readonly List<string> _history = new();
    private int _historyIndex = 0;

    public void AddCommand(string command)
    {
        _history.Add(command);
        _historyIndex = _history.Count;
    }

    public string? GetPreviousCommand()
    {
        if (_historyIndex > 0)
        {
            _historyIndex--;
            return _history[_historyIndex];
        }
        return null;
    }

    public string? GetNextCommand()
    {
        if (_historyIndex < _history.Count - 1)
        {
            _historyIndex++;
            return _history[_historyIndex];
        }
        return null;
    }
}
```

**Benefits**: Reduces retyping for repeated commands ("north", "search", "take all")

---

### FE-2: Fuzzy Matching for Multi-Word Targets

**Problem**: Players must type exact item/enemy names ("Enchanted Ring of Warding").

**Proposed Solution**:
```csharp
public string? FuzzyMatchItemName(string input, List<Item> availableItems)
{
    // "enc ring" → "Enchanted Ring of Warding"
    return availableItems
        .FirstOrDefault(i => i.Name.Contains(input, StringComparison.OrdinalIgnoreCase))?
        .Name;
}
```

**Benefits**: Faster input, fewer typos, better user experience

---

### FE-3: Contextual Help

**Problem**: `help` command shows generic help, not phase-specific commands.

**Proposed Solution**:
```csharp
public void DisplayHelp(GamePhase phase)
{
    var helpText = phase switch
    {
        GamePhase.Combat => @"
            [yellow]Combat Commands:[/]
            - attack <target> - Basic attack
            - attack <target> with <ability> - Use ability
            - defend - Defensive stance
            - flee - Attempt to escape
            - inventory - Open inventory (limited)
        ",
        GamePhase.Exploration => @"
            [yellow]Exploration Commands:[/]
            - north/south/east/west - Move
            - look - Re-describe room
            - examine <object> - Inspect closely
            - take <item> - Pick up item
            - inventory - Open inventory
            - journal - View quests
        ",
        _ => "[yellow]Type 'help' for commands.[/]"
    };

    AnsiConsole.MarkupLine(helpText);
}
```

**Benefits**: Players discover relevant commands faster

---

### FE-4: Command Macros

**Problem**: Repetitive multi-step actions (e.g., "take all potions", "equip best gear").

**Proposed Solution**:
```json
{
  "macros": {
    "loot": ["search", "take all"],
    "heal": ["inventory", "use healing potion", "back"],
    "prep": ["equip longsword", "equip shield", "equip armor"]
  }
}
```

**Implementation**:
```csharp
if (_macroService.IsMacro(input))
{
    var commands = _macroService.GetMacroCommands(input);
    foreach (var cmd in commands)
        await ParseAndExecuteAsync(cmd, gameState);
}
```

**Benefits**: Power users can create custom shortcuts

---

### FE-5: Voice Commands (Accessibility)

**Problem**: Players with mobility impairments struggle with typing.

**Proposed Solution**:
- Integrate speech-to-text library (e.g., Microsoft Speech Platform)
- Convert voice input to text → Parse as normal
- Example: Player says "attack orc" → Recognized as "attack orc" → ParseCombatCommand()

**Benefits**: Inclusive design, broader player base

---

## AAM-VOICE Compliance

### Layer Classification: **Layer 3 (Technical Specification)**

**Rationale**: This document is a system architecture specification for developers, not in-game content. Layer 3 applies to technical documentation written POST-Glitch with modern precision language.

### Domain 4 Compliance: **NOT APPLICABLE**

**Rationale**: Domain 4 (Technology Constraints) applies to **in-game lore content** (item descriptions, bestiary entries, NPC dialogue). This specification is **out-of-game technical documentation** and may use precision measurements (e.g., "10ms parse time," "882 lines," "70% coverage").

### Voice Discipline: **Technical Authority**

**Characteristics**:
- **Precision**: Exact method signatures, line numbers, file paths
- **Definitive Statements**: "The system MUST...", "Commands are always lowercase"
- **Code Examples**: C# implementations with expected inputs/outputs
- **Quantifiable Metrics**: "70% test coverage," "25+ command types"

**Justification**: Developers require precise, unambiguous technical specifications. Epistemic uncertainty ("appears to support aliases") would introduce confusion and implementation errors.

---

**END OF SPECIFICATION**
