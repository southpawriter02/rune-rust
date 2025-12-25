---
id: SPEC-INPUT-001
title: Input Handling System
version: 2.0.0
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-COMBAT-001, SPEC-NAV-001, SPEC-INTERACT-001]
---

# SPEC-INPUT-001: Input Handling System

> **Version:** 2.0.0
> **Status:** Implemented (v0.3.10c)
> **Services:** `CommandParser`, `TerminalInputHandler`, `InputConfigurationService`
> **Location:** `RuneAndRust.Engine/Services/`, `RuneAndRust.Terminal/Services/`

---

## Table of Contents

1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [ParseResult Class](#parseresult-class)
4. [CommandParser API](#commandparser-api)
5. [Phase Handlers](#phase-handlers)
6. [IInputHandler Interface](#iinputhandler-interface)
7. [InputConfigurationService](#inputconfigurationservice)
8. [Integration Points](#integration-points)
9. [Testing](#testing)
10. [Changelog](#changelog)

---

## Overview

The **Input Handling System** manages all player input for *Rune & Rust*, transforming raw terminal commands into structured game actions. The system employs a **deferred execution pattern** where user input is parsed into `ParseResult` objects that signal what actions the calling code must perform.

### Key Responsibilities

1. **Command Parsing**: Transform raw text input into `ParseResult` flag objects (CommandParser)
2. **Input Abstraction**: Provide platform-agnostic input interface (IInputHandler → TerminalInputHandler)
3. **Keybinding Configuration**: Load and manage customizable hotkey mappings (InputConfigurationService)
4. **Phase-Based Routing**: Route commands to appropriate handlers based on GamePhase
5. **Natural Language Support**: Recognize command aliases and variants ("take"/"get"/"grab")
6. **Error Handling**: Provide helpful feedback for unknown commands and missing parameters

### Architecture Pattern

```
User Input (Terminal) → IInputHandler.GetInput() → CommandParser.ParseAndExecuteAsync()
                                                          ↓
                                                   Phase-Based Handler
                                                   (MainMenu/Exploration/Combat)
                                                          ↓
                                                   ParseResult (flags)
                                                          ↓
                                                   Caller Executes Actions
```

**Key Design Decision**: The system uses a **deferred execution pattern** where commands are parsed into **ParseResult flag objects**, not executed during parsing. This separates parsing logic from game logic, enabling better testing and clearer responsibilities.

### Technology Stack

- **Spectre.Console 0.54.0**: Terminal input via `AnsiConsole.Prompt<string>()`
- **.NET 9.0**: Pattern matching for command routing, async command execution
- **JSON Configuration**: Keybindings stored in `data/input_bindings.json`

---

## Core Concepts

### 1. ParseResult Pattern

**Definition**: Command parsing produces `ParseResult` objects containing boolean flags and target strings that indicate what the caller must do.

**Design Pattern**: Flat boolean flag + target string pairs for each command type.

```csharp
// Example: "take rusty sword" produces
ParseResult {
    RequiresTake = true,
    TakeTarget = "rusty sword"
}
```

**Why This Pattern?**
- Easy to extend without modifying enums
- Clear separation of "what was parsed" vs "what to execute"
- Multiple flags can be set (though typically only one per command)
- Nullable targets indicate optional parameters

### 2. Phase-Based Routing

**Definition**: Commands are interpreted differently based on the current GamePhase.

| Phase | Handler Method | Available Commands |
|-------|----------------|-------------------|
| `MainMenu` | `HandleMainMenu()` | new, start, load, quit, help |
| `Exploration` | `HandleExplorationAsync()` | Movement, interaction, inventory, crafting, rest |
| `Combat` | `HandleCombat()` | Attack, abilities, flee, turn control |

**State Machine**:
```
MainMenu ──"start"──> Exploration ──"menu"──> MainMenu
    │                      │
    │                      │"ambush"
    │                      v
    │                   Combat ──"flee"──> Exploration
    │                      │
    └──────"quit"──────────┴──────────> Quit
```

### 3. Deferred Execution

**Definition**: CommandParser does NOT execute actions. It signals intent via ParseResult flags.

**Responsibilities:**
- **CommandParser DOES**: Parse input, validate syntax, set flags, mutate GameState phase/turn
- **CommandParser DOES NOT**: Call navigation, execute combat, modify inventory, persist data

**Example Flow:**
```csharp
// CommandParser sets flag
result.RequiresNavigation = true;
result.NavigationDirection = Direction.North;

// GameService (caller) executes
if (result.RequiresNavigation)
    await _navigationService.MoveAsync(result.NavigationDirection.Value);
```

---

## ParseResult Class

**Location**: Defined inline in `CommandParser.cs` (lines 12-227)

### Property Inventory

#### Navigation & Movement
| Property | Type | Description |
|----------|------|-------------|
| `RequiresNavigation` | `bool` | Movement command was parsed |
| `NavigationDirection` | `Direction?` | Target direction (N/S/E/W/U/D) |

#### Observation & Examination
| Property | Type | Description |
|----------|------|-------------|
| `RequiresLook` | `bool` | "look", "l", "exits", or post-action refresh |
| `RequiresExamine` | `bool` | Examine command parsed |
| `ExamineTarget` | `string?` | Object name to examine |

#### Container Interaction
| Property | Type | Description |
|----------|------|-------------|
| `RequiresOpen` | `bool` | Open command parsed |
| `OpenTarget` | `string?` | Container to open |
| `RequiresClose` | `bool` | Close command parsed |
| `CloseTarget` | `string?` | Container to close |
| `RequiresSearch` | `bool` | Search room command |

#### Inventory Management
| Property | Type | Description |
|----------|------|-------------|
| `RequiresInventory` | `bool` | Text inventory display |
| `RequiresInventoryScreen` | `bool` | Full UI inventory (v0.3.7a) |
| `RequiresEquipment` | `bool` | Equipment display |
| `RequiresEquip` | `bool` | Equip command |
| `EquipTarget` | `string?` | Item to equip |
| `RequiresUnequip` | `bool` | Unequip command |
| `UnequipTarget` | `string?` | Item/slot to unequip |
| `RequiresDrop` | `bool` | Drop command |
| `DropTarget` | `string?` | Item to drop |

#### Loot & Item Pickup
| Property | Type | Description |
|----------|------|-------------|
| `RequiresTake` | `bool` | Take/get/grab command |
| `TakeTarget` | `string?` | Item to take |
| `RequiresLoot` | `bool` | Loot container |
| `LootTarget` | `string?` | Container to loot |
| `RequiresUse` | `bool` | Use/consume/apply command |
| `UseTarget` | `string?` | Item to use |

#### Crafting & Bodging (v0.3.7b)
| Property | Type | Description |
|----------|------|-------------|
| `RequiresCraft` | `bool` | Craft/make/create command |
| `CraftTarget` | `string?` | Recipe name or ID |
| `RequiresCraftingScreen` | `bool` | Full crafting UI |
| `RequiresRepair` | `bool` | Repair/fix/mend command |
| `RepairTarget` | `string?` | Item to repair |
| `RequiresSalvage` | `bool` | Salvage/scrap/dismantle |
| `SalvageTarget` | `string?` | Item to salvage |

#### Alchemy & Runeforging (v0.3.1c)
| Property | Type | Description |
|----------|------|-------------|
| `RequiresBrew` | `bool` | Brew/mix/concoct command |
| `BrewTarget` | `string?` | Recipe name |
| `RequiresForge` | `bool` | Forge/inscribe/etch command |
| `ForgeRune` | `string?` | Rune name (e.g., "Rune of Fire") |
| `ForgeTarget` | `string?` | Target item (null = standalone) |

#### UI Navigation (v0.3.7a-c, v0.3.10b)
| Property | Type | Description |
|----------|------|-------------|
| `RequiresJournalScreen` | `bool` | Full journal UI |
| `RequiresListObjects` | `bool` | List room objects |
| `RequiresCharacterCreation` | `bool` | Character creation |
| `RequiresOptionsScreen` | `bool` | Options/settings UI |

### Static Factory
```csharp
public static ParseResult None => new();  // All flags false, all targets null
```

---

## CommandParser API

**Location**: `RuneAndRust.Engine/Services/CommandParser.cs`

### Constructor
```csharp
public CommandParser(
    ILogger<CommandParser> logger,
    IInputHandler inputHandler,
    GameState gameState,
    IJournalService? journalService = null,
    ICombatService? combatService = null,
    IVictoryScreenRenderer? victoryRenderer = null,
    IRestService? restService = null,
    IRestScreenRenderer? restRenderer = null,
    IRoomRepository? roomRepository = null)
```

**Dependencies:**
- **Required**: `ILogger`, `IInputHandler`, `GameState`
- **Optional**: All combat, journal, rest, and render services (graceful degradation)

### Main Method
```csharp
public async Task<ParseResult> ParseAndExecuteAsync(string input, GameState state)
```

**Parameters:**
- `input`: Raw user input (case-insensitive, whitespace-trimmed)
- `state`: GameState to mutate based on command

**Returns**: `Task<ParseResult>` with flags indicating required actions

**Behavior:**
1. Returns `ParseResult.None` if input is null/empty/whitespace
2. Normalizes input: `input.Trim().ToLowerInvariant()`
3. Routes to phase-specific handler based on `state.Phase`
4. Mutates `state` for phase transitions, turn counts, pending actions
5. Displays errors/help via `_inputHandler`

---

## Phase Handlers

### MainMenu Phase

**Handler**: `HandleMainMenu(string command, GameState state)`

| Input | Action | ParseResult |
|-------|--------|-------------|
| `new`, `create` | — | `RequiresCharacterCreation = true` |
| `start`, `play` | Set Phase=Exploration, IsSessionActive=true, TurnCount=0 | `RequiresLook = true` |
| `load` | Set PendingAction=Load | `ParseResult.None` |
| `quit`, `exit`, `q` | Set Phase=Quit | `ParseResult.None` |
| `help`, `?` | Display main menu help | `ParseResult.None` |
| *(unknown)* | Display error | `ParseResult.None` |

### Exploration Phase

**Handler**: `HandleExplorationAsync(string command, GameState state)`

#### Movement Commands
| Input | Direction | Notes |
|-------|-----------|-------|
| `n`, `north` | North | Single letter or full name |
| `s`, `south` | South | |
| `e`, `east` | East | |
| `w`, `west` | West | |
| `u`, `up` | Up | |
| `d`, `down` | Down | |
| `go <direction>` | *(parsed)* | Prefixed form |

**Returns**: `RequiresNavigation = true`, `NavigationDirection = <dir>`

#### Interaction Commands
| Input Pattern | Returns | Target Property |
|---------------|---------|-----------------|
| `examine <obj>`, `x <obj>`, `look <obj>` | `RequiresExamine` | `ExamineTarget` |
| `open <container>` | `RequiresOpen` | `OpenTarget` |
| `close <container>` | `RequiresClose` | `CloseTarget` |
| `search` | `RequiresSearch` | — |

#### Inventory Commands
| Input | Returns | Notes |
|-------|---------|-------|
| `inventory`, `i` | `RequiresInventory` | Text display |
| `pack`, `p` | `RequiresInventoryScreen` | Full UI (v0.3.7a) |
| `equipment`, `gear`, `equipped` | `RequiresEquipment` | Equipment display |
| `equip <item>`, `bind <item>` | `RequiresEquip`, `EquipTarget` | |
| `unequip <slot>`, `unbind`, `remove` | `RequiresUnequip`, `UnequipTarget` | |
| `drop <item>`, `discard` | `RequiresDrop`, `DropTarget` | |

#### Loot Commands
| Input Pattern | Returns | Target Property |
|---------------|---------|-----------------|
| `take <item>`, `get`, `grab` | `RequiresTake` | `TakeTarget` |
| `loot <container>` | `RequiresLoot` | `LootTarget` |
| `use <item>`, `consume`, `apply` | `RequiresUse` | `UseTarget` |

#### Crafting Commands
| Input Pattern | Returns | Target Property | Notes |
|---------------|---------|-----------------|-------|
| `craft <recipe>`, `make`, `create` | `RequiresCraft` | `CraftTarget` | Increments TurnCount |
| `repair <item>`, `fix`, `mend` | `RequiresRepair` | `RepairTarget` | Increments TurnCount |
| `salvage <item>`, `scrap`, `dismantle` | `RequiresSalvage` | `SalvageTarget` | Increments TurnCount |
| `brew <recipe>`, `mix`, `concoct` | `RequiresBrew` | `BrewTarget` | v0.3.1c |

#### Runeforging (v0.3.1c)
| Input Pattern | Returns | Properties |
|---------------|---------|------------|
| `forge <rune>` | `RequiresForge` | `ForgeRune` |
| `forge <rune> on <item>` | `RequiresForge` | `ForgeRune`, `ForgeTarget` |
| `inscribe <rune> on <item>` | `RequiresForge` | `ForgeRune`, `ForgeTarget` |
| `etch <rune> on <item>` | `RequiresForge` | `ForgeRune`, `ForgeTarget` |

#### Journal Commands
| Input | Action | Returns |
|-------|--------|---------|
| `journal`, `j` | Call `FormatJournalListAsync()` | Display only |
| `fragments` | Call `FormatUnassignedCapturesAsync()` | Display only |
| `codex <name>` | Call `FormatEntryDetailAsync()` | Display only |
| `codex` (no arg) | Display usage message | Display only |

#### Screen Navigation
| Input | Returns |
|-------|---------|
| `bench`, `workbench`, `craft-menu` | `RequiresCraftingScreen` |
| `archive` | `RequiresJournalScreen` |
| `options`, `settings`, `config`, `o` | `RequiresOptionsScreen` |
| `objects`, `items` | `RequiresListObjects` |

#### Meta Commands
| Input | Action | Returns |
|-------|--------|---------|
| `look`, `l`, `exits` | Increment TurnCount | `RequiresLook` |
| `status`, `stats` | Display inline | `ParseResult.None` |
| `save` | Set PendingAction=Save | `ParseResult.None` |
| `load` | Set PendingAction=Load | `ParseResult.None` |
| `menu`, `mainmenu` | Set Phase=MainMenu, clear state | `ParseResult.None` |
| `help`, `?` | Display exploration help | `ParseResult.None` |
| `quit`, `exit`, `q` | Set Phase=Quit | `ParseResult.None` |

#### Rest Commands (v0.3.2c)
| Input | RestType Preference |
|-------|---------------------|
| `rest` | Sanctuary (if at Runic Anchor) |
| `camp`, `sleep` | Wilderness (always) |

**Behavior**: Calls `IRestService.PerformRestAsync()`, handles ambush by setting Phase=Combat.

### Combat Phase

**Handler**: `HandleCombat(string command, GameState state)`

#### Attack Commands
| Input Pattern | AttackType |
|---------------|------------|
| `attack <target>`, `hit`, `strike` | Standard |
| `light <target>`, `quick`, `fast` | Light |
| `heavy <target>`, `power`, `strong` | Heavy |

#### Ability Commands
| Input Pattern | Behavior |
|---------------|----------|
| `use <ability>` | Execute by name |
| `use <ability> on <target>` | Execute with target |
| `use <ability> at <target>` | Execute with target |
| `1`-`9` (single digit) | Execute by hotkey |

#### Turn Control
| Input | Action |
|-------|--------|
| `end`, `pass`, `wait` | Call `ICombatService.NextTurn()` |
| `status`, `stats` | Display combat status |
| `flee`, `run` | End combat, return to Exploration |
| `help`, `?` | Display combat help |
| `quit`, `exit`, `q` | Set Phase=Quit |

---

## IInputHandler Interface

**Location**: `RuneAndRust.Core/Interfaces/IInputHandler.cs`

```csharp
public interface IInputHandler
{
    /// <summary>
    /// Prompts the user and waits for string input.
    /// </summary>
    string GetInput(string prompt);

    /// <summary>
    /// Displays a message to the user without expecting input.
    /// </summary>
    void DisplayMessage(string message);

    /// <summary>
    /// Displays an error message to the user.
    /// </summary>
    void DisplayError(string message);
}
```

### TerminalInputHandler Implementation

**Location**: `RuneAndRust.Terminal/Services/TerminalInputHandler.cs`

Uses **Spectre.Console** for rich terminal output:

```csharp
public string GetInput(string prompt)
{
    return AnsiConsole.Prompt(
        new TextPrompt<string>($"[green]{EscapeMarkup(prompt)}[/] [grey]>[/]")
            .AllowEmpty());
}

public void DisplayMessage(string message)
{
    AnsiConsole.MarkupLine($"[white]{EscapeMarkup(message)}[/]");
}

public void DisplayError(string message)
{
    AnsiConsole.MarkupLine($"[red]{EscapeMarkup(message)}[/]");
}
```

---

## InputConfigurationService

**Location**: `RuneAndRust.Engine/Services/InputConfigurationService.cs`
**Version**: v0.3.9c (initial), v0.3.10c (reverse lookup)

### Interface
```csharp
public interface IInputConfigurationService
{
    void LoadBindings();
    void SaveBindings();
    string? GetCommandForKey(ConsoleKey key);
    ConsoleKey? GetKeyForCommand(string command);  // v0.3.10c
    void SetBinding(ConsoleKey key, string command);
    bool RemoveBinding(ConsoleKey key);
    IReadOnlyDictionary<ConsoleKey, string> GetAllBindings();
    void ResetToDefaults();
}
```

### Configuration File
**Location**: `data/input_bindings.json`

```json
{
  "bindings": {
    "N": "north",
    "S": "south",
    "I": "inventory",
    "A": "attack"
  }
}
```

### Default Key Bindings

| Category | Key | Command |
|----------|-----|---------|
| Movement | N, S, E, W, U, D | north, south, east, west, up, down |
| Core | Enter, Escape, M, H | confirm, cancel, menu, help |
| Screens | I, C, J, B | inventory, character, journal, bench |
| Gameplay | F, L, X, Space | interact, look, search, wait |
| Combat | A, Q, R | attack, light, heavy |

**Note**: InputConfigurationService is implemented but **not yet integrated** into CommandParser. Keybindings are defined but command parsing currently uses hardcoded string matching.

---

## Integration Points

### Services Called by CommandParser

| Service | Methods Called | Trigger |
|---------|----------------|---------|
| `IJournalService` | `FormatEntryDetailAsync()`, `FormatJournalListAsync()`, `FormatUnassignedCapturesAsync()` | `codex`, `journal`, `fragments` commands |
| `ICombatService` | `StartCombat()`, `ExecutePlayerAttack()`, `ExecutePlayerAbility()`, `NextTurn()`, `CheckVictoryCondition()`, `EndCombat()` | Combat phase commands |
| `IVictoryScreenRenderer` | `Render(CombatResult)` | Victory condition met |
| `IRestService` | `PerformRestAsync()` | `rest`, `camp`, `sleep` commands |
| `IRestScreenRenderer` | `Render()`, `RenderAmbushWarning()` | Rest result display |
| `IRoomRepository` | `GetByIdAsync()` | Rest location validation |

### GameState Mutations

| Property | Mutated By | Effect |
|----------|------------|--------|
| `Phase` | start, quit, menu, ambush | State machine transitions |
| `IsSessionActive` | start, menu | Tracks active gameplay |
| `TurnCount` | Action-consuming commands | In-game time tracking |
| `CurrentRoomId` | menu | Cleared on menu transition |
| `PendingAction` | save, load | Signals persistence operations |
| `PendingEncounter` | Ambush during rest | Stores encounter for combat init |

---

## Testing

**Location**: `RuneAndRust.Tests/Engine/`

### Test Files

| File | Test Count | Coverage |
|------|------------|----------|
| `CommandParserTests.cs` | 65 tests | Main parsing logic |
| `CommandParserRestTests.cs` | 11 tests | Rest/camp/ambush handling |
| **Total** | **76 tests** | |

### Test Categories

1. **Constructor Tests** - Valid dependency injection
2. **Empty Input Tests** - Null, empty string, whitespace
3. **MainMenu Phase Tests** - Start, new, quit, help, load
4. **Exploration Phase Tests** - Movement, inventory, interaction, meta commands
5. **Combat Phase Tests** - Quit, flee, help, unknown
6. **Logging Tests** - Debug/Info level verification
7. **Case Insensitivity Tests** - Input normalization
8. **Rest/Camp Tests** - Sanctuary vs wilderness, ambush handling

### Coverage Gaps

Commands with limited or no test coverage:
- Forge command parsing (`ParseForgeCommand()`)
- Ability execution (`ExecuteAbilityCommand()`)
- Combat attack execution (`ExecuteCombatAttack()`)
- Victory condition handling
- Crafting screen commands

---

## Changelog

### Version 2.0.0 (2025-12-25) - Complete Rewrite

**Breaking Change**: This version documents the **actual implementation**, replacing the theoretical design from v1.0.x that was never built.

**Documented Architecture:**
- ParseResult class with 25+ boolean flag properties
- Phase-based routing (MainMenu, Exploration, Combat)
- Deferred execution pattern (parser signals, caller executes)
- `HandleExplorationAsync()`, `HandleCombat()`, `HandleMainMenu()` handlers

**New Sections:**
- Complete ParseResult property inventory
- All commands by phase (60+ exploration, 10+ combat)
- GameState mutation documentation
- Integration points with other services
- Actual test coverage (76 tests)

**Code Traceability:**
- Added `/// <remarks>See: SPEC-INPUT-001...</remarks>` to implementation files

---

### Version 1.0.1 (2025-12-25) - Deprecated

**Status Change:**
- Changed status from "Implemented" to "Deprecated"
- Added deprecation notice documenting architectural divergence
- Superseded by v2.0.0 rewrite

---

### Version 1.0.0 (2025-01-XX) - Initial (Historical)

**Note**: This version documented a theoretical design that was not implemented.

- Described ParseResult as record with `CommandType` enum and `Dictionary<string, string>`
- Proposed phase-specific parsers: `ParseExplorationCommand()`, `ParseCombatCommand()`
- Defined 25+ CommandType enum values (not used in implementation)
- Documented 8 game phases (implementation has 3)

---

**Specification Status**: Complete and verified against CommandParser v0.3.10c implementation.
