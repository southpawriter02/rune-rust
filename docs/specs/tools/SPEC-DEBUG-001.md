---
id: SPEC-DEBUG-001
title: Debug Console System
version: 1.0.0
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-CHEAT-001, SPEC-GAME-001]
---

# SPEC-DEBUG-001: Debug Console System

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `IDebugConsoleService`, `DebugConsoleService`, `IDebugConsoleRenderer`, `DebugConsoleRenderer`
> **Location:** `RuneAndRust.Engine/Services/DebugConsoleService.cs`, `RuneAndRust.Terminal/Rendering/DebugConsoleRenderer.cs`

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Behaviors](#behaviors)
- [Restrictions](#restrictions)
- [Limitations](#limitations)
- [Use Cases](#use-cases)
- [Decision Trees](#decision-trees)
- [Cross-Links](#cross-links)
- [Related Services](#related-services)
- [Data Models](#data-models)
- [Configuration](#configuration)
- [Testing](#testing)
- [Design Rationale](#design-rationale)
- [Changelog](#changelog)

---

## Overview

The Debug Console System (introduced in v0.3.17a "The Console") provides developers with a Quake-style overlay console for executing debug commands, viewing system logs, and managing game state during development and testing. The console can be accessed from any game phase by pressing the tilde (`~`) key or typing `debug`.

The system consists of two main components: a state management service (`DebugConsoleService`) that handles visibility toggling, log buffer management, and command history tracking; and a rendering component (`DebugConsoleRenderer`) that implements the modal input loop and built-in command processing.

Key features include thread-safe bounded buffers, keyboard navigation through command history, phase-agnostic accessibility, and integration with the Cheat Command System (SPEC-CHEAT-001) for gameplay manipulation.

---

## Core Concepts

### Quake-Style Console

A drop-down terminal overlay that appears over the current game screen when activated. The console captures all keyboard input while visible, returning control to the game loop only when dismissed. This pattern is widely recognized from classic FPS games like Quake and Half-Life.

### Bounded Buffer Management

Both the log history and command history use fixed-size circular buffers that automatically prune the oldest entries when capacity is exceeded:
- **Log History:** Maximum 50 entries
- **Command History:** Maximum 20 entries

This prevents unbounded memory growth during long debug sessions.

### Phase-Agnostic Access

The console intercept is processed in `CommandParser.ParseAndExecuteAsync()` before phase-specific command routing, allowing developers to access debug tools from MainMenu, Exploration, Combat, or any other game phase.

### Modal Input Loop

When the console is visible, it runs a blocking loop that captures all keyboard input until the user dismisses it with `~`, `Escape`, or the `exit` command. This ensures no game input leaks through while debugging.

---

## Behaviors

### Primary Behaviors

#### 1. Toggle Visibility (`Toggle`)

```csharp
void Toggle()
```

**Purpose:** Switches console visibility between visible and hidden states.

**Logic:**
1. Flip `IsVisible` boolean property
2. Log state change at Trace level: `[DEBUG] Console visibility set to {State}`

**Example:**
```csharp
console.Toggle();  // IsVisible: false → true
console.Toggle();  // IsVisible: true → false
```

#### 2. Write Log Entry (`WriteLog`)

```csharp
void WriteLog(string message, string source = "System")
```

**Purpose:** Adds a timestamped, source-tagged entry to the log buffer.

**Logic:**
1. Format entry: `[HH:mm:ss] [Source] Message`
2. Acquire lock on internal buffer
3. Add entry to `_logHistory`
4. If count exceeds `MaxLogHistory` (50), remove oldest entry
5. Release lock

**Example:**
```csharp
console.WriteLog("Connection established", "Network");
// Log entry: "[14:32:45] [Network] Connection established"
```

#### 3. Submit Command (`SubmitCommand`)

```csharp
void SubmitCommand(string command)
```

**Purpose:** Records a user command in both log and command history.

**Logic:**
1. Validate input is not null, empty, or whitespace
2. Log at Debug level: `[DEBUG] User submitted: {Command}`
3. Call `WriteLog(command, "User")` to add to log buffer
4. Acquire lock on command history
5. Add command to `_commandHistory`
6. If count exceeds `MaxCommandHistory` (20), remove oldest entry
7. Release lock

**Example:**
```csharp
console.SubmitCommand("/god");
// Adds to command history for up-arrow navigation
// Log entry: "[14:32:50] [User] /god"
```

#### 4. Clear Log (`ClearLog`)

```csharp
void ClearLog()
```

**Purpose:** Empties the log buffer while preserving command history.

**Logic:**
1. Acquire lock on log buffer
2. Clear `_logHistory` list
3. Log at Trace level: `[DEBUG] Log buffer cleared`
4. Release lock

**Example:**
```csharp
console.ClearLog();
// Log buffer now empty; command history retained
```

#### 5. Run Modal Loop (`Run`)

```csharp
void Run()  // IDebugConsoleRenderer
```

**Purpose:** Starts the blocking modal input loop for console interaction.

**Logic:**
1. While `_console.IsVisible`:
   - Render console screen with current log and input buffer
   - Wait for keypress via `Console.ReadKey(intercept: true)`
   - Process key:
     - `~` or `Escape` → Toggle visibility (exit loop)
     - `Enter` → Submit command, clear input buffer, reset history index
     - `Backspace` → Remove last character from input buffer
     - `UpArrow` → Navigate backward through command history
     - `DownArrow` → Navigate forward through history or reset
     - Printable char → Append to input buffer
2. Return control to caller (game loop continues)

**Keyboard Mapping:**

| Key | Action |
|-----|--------|
| `~` (Oem3) | Close console |
| `Escape` | Close console |
| `Enter` | Submit command |
| `Backspace` | Delete last character |
| `UpArrow` | Previous command in history |
| `DownArrow` | Next command or clear |
| Printable | Append to input |

---

### Built-in Commands

| Command | Action | Example Output |
|---------|--------|----------------|
| `help` | Display available commands and cheat reference | Lists help, clear, exit, and cheat commands |
| `clear` | Clear log buffer | `[System] Log cleared.` |
| `exit` | Close console | `[System] Console closed.` |
| `~` | Close console (alias) | `[System] Console closed.` |

---

## Restrictions

### What This System MUST NOT Do

1. **Never process game input while visible:** All keyboard input must be captured by the modal loop to prevent accidental game actions.

2. **Never persist log or command history:** Buffers are cleared when the application exits; no disk persistence.

3. **Never expose sensitive data:** Log entries should not contain passwords, API keys, or other credentials.

4. **Never block indefinitely:** The modal loop must always be terminable via `~`, `Escape`, or `exit`.

5. **Never modify game state directly:** The console service manages visibility and buffers only; game manipulation is delegated to CheatService.

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| Max log entries | 50 | Prevents memory bloat during long sessions |
| Max command history | 20 | Sufficient for typical debugging workflow |
| Visible log lines | 15 | Fits standard terminal viewport |
| Log format | Fixed | `[HH:mm:ss] [Source] Message` - not configurable |
| Threading | Lock-based | Simple synchronization; not lock-free |

---

## Use Cases

### UC-1: Standard Console Access

**Actor:** Developer
**Trigger:** Press `~` key during any game phase
**Preconditions:** Game is running

```csharp
// CommandParser intercepts before phase routing
if (command == "~" || command == "debug")
{
    _debugConsoleRenderer.Run();  // Modal loop
    return ParseResult.None;       // Consume input
}
```

**Postconditions:**
- Console overlay appears
- Game loop paused until console dismissed
- All keyboard input captured by console

### UC-2: Execute Cheat Command

**Actor:** Developer
**Trigger:** Type `/god` and press Enter in console
**Preconditions:** Console is visible

```csharp
// ProcessCheatCommand routes to CheatService
case "god":
    var state = _cheats.ToggleGodMode();
    _console.WriteLog($"God Mode: {(state ? "ON" : "OFF")}", "Cheat");
    break;
```

**Postconditions:**
- God Mode toggled
- Feedback displayed in console log
- Console remains open for further commands

### UC-3: Navigate Command History

**Actor:** Developer
**Trigger:** Press UpArrow in console
**Preconditions:** Console visible, command history non-empty

```csharp
case ConsoleKey.UpArrow:
    if (historyIndex < history.Count)
    {
        historyIndex++;
        inputBuffer = history[^historyIndex];  // Index from end
    }
    break;
```

**Postconditions:**
- Input buffer shows previous command
- Can navigate further back with more UpArrow presses
- DownArrow reverses navigation

### UC-4: Clear Log Buffer

**Actor:** Developer
**Trigger:** Type `clear` and press Enter
**Preconditions:** Console visible, log has entries

```csharp
case "clear":
    _console.ClearLog();
    _console.WriteLog("Log cleared.", "System");
    break;
```

**Postconditions:**
- Log buffer emptied (except new "Log cleared" message)
- Command history preserved
- Console remains open

---

## Decision Trees

### Command Routing Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    User Input in Console                     │
└────────────────────────────┬────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │ Starts with "/"?│
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │ YES          │              │ NO
              ▼              │              ▼
    ┌─────────────────┐      │    ┌─────────────────────────┐
    │ ProcessCheat    │      │    │ ProcessBuiltInCommand   │
    │ (/god, /heal,   │      │    │ (help, clear, exit)     │
    │  /tp, /reveal)  │      │    └───────────┬─────────────┘
    └─────────────────┘      │                │
                             │         ┌──────┴──────┐
                             │         │ Unknown?    │
                             │         └──────┬──────┘
                             │                │
                             │    ┌───────────┼───────────┐
                             │    │ YES       │           │ NO
                             │    ▼           │           ▼
                             │ ┌──────────┐   │    ┌──────────┐
                             │ │ WriteLog │   │    │ Execute  │
                             │ │ "Unknown │   │    │ Command  │
                             │ │ command" │   │    │          │
                             │ └──────────┘   │    └──────────┘
```

### Key Input Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    Console.ReadKey()                         │
└────────────────────────────┬────────────────────────────────┘
                             │
              ┌──────────────┴──────────────┐
              │ Key Type                     │
              └──────────────┬──────────────┘
                             │
    ┌──────────┬─────────────┼─────────────┬──────────────┐
    │          │             │             │              │
    ▼          ▼             ▼             ▼              ▼
 Tilde     Escape        Enter        Up/Down       Printable
    │          │             │         Arrow              │
    │          │             │             │              │
    ▼          ▼             ▼             ▼              ▼
┌────────┐ ┌────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
│ Toggle │ │ Toggle │ │ Submit   │ │ Navigate │ │ Append   │
│ Off    │ │ Off    │ │ Command  │ │ History  │ │ to Input │
└────────┘ └────────┘ └──────────┘ └──────────┘ └──────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `ICheatService` | [SPEC-CHEAT-001](./SPEC-CHEAT-001.md) | Routes cheat commands for execution |
| `ILogger<T>` | *(external)* | Serilog structured logging |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| `CommandParser` | [SPEC-GAME-001](../core/SPEC-GAME-001.md) | Phase-agnostic console interception |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `RuneAndRust.Core/Interfaces/IDebugConsoleService.cs` | Service contract | 1-49 |
| `RuneAndRust.Core/Interfaces/IDebugConsoleRenderer.cs` | Renderer abstraction | 1-14 |
| `RuneAndRust.Engine/Services/DebugConsoleService.cs` | State & buffer management | 1-130 |
| `RuneAndRust.Terminal/Rendering/DebugConsoleRenderer.cs` | Modal UI & commands | 1-287 |
| `RuneAndRust.Engine/Services/CommandParser.cs` | Console interception | 303-316 |

### Supporting Types

| File | Purpose | Key Lines |
|------|---------|-----------|
| `RuneAndRust.Terminal/Program.cs` | DI registration | 212-217 |

---

## Data Models

### Log Entry Format

```
[HH:mm:ss] [Source] Message
```

**Examples:**
```
[14:32:45] [System] Console opened.
[14:32:50] [User] /god
[14:32:50] [Cheat] God Mode: ON
[14:32:55] [Error] Room not found: invalid_room
```

### Source Tags

| Source | Color | Usage |
|--------|-------|-------|
| `System` | Grey | Built-in commands, system messages |
| `User` | Cyan | User-entered commands |
| `Cheat` | *(default)* | Cheat command results |
| `Error` | Red | Error messages |

---

## Configuration

### Constants

```csharp
// DebugConsoleService.cs
private const int MaxLogHistory = 50;
private const int MaxCommandHistory = 20;

// DebugConsoleRenderer.cs
private const int VisibleLogLines = 15;
```

### DI Lifetimes

| Service | Lifetime | Rationale |
|---------|----------|-----------|
| `IDebugConsoleService` | Singleton | State persists across game session |
| `IDebugConsoleRenderer` | Scoped | New instance per scope (minimal state) |

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `RuneAndRust.Tests/Engine/DebugConsoleServiceTests.cs` | 12 | Core service functionality |

### Critical Test Scenarios

1. **Toggle_SwitchesVisibilityFalseToTrue** - Initial false → true
2. **Toggle_SwitchesVisibilityTrueToFalse** - Toggles back to false
3. **WriteLog_AddsEntryToHistory** - Entry appears in LogHistory
4. **WriteLog_FormatsEntryWithTimestampAndSource** - Validates format
5. **WriteLog_UsesDefaultSourceWhenNotSpecified** - Default [System]
6. **WriteLog_MaintainsBufferLimit_RemovesOldest** - 55 entries → 50
7. **SubmitCommand_AddsToCommandHistory** - Command in history
8. **SubmitCommand_WritesToLogWithUserSource** - [User] source
9. **SubmitCommand_IgnoresEmptyInput** - null/empty/whitespace
10. **CommandHistory_MaintainsLimit_RemovesOldest** - 25 → 20
11. **ClearLog_EmptiesLogBuffer** - Buffer empty after clear
12. **ClearLog_DoesNotClearCommandHistory** - History preserved

### Validation Checklist

- [x] Visibility toggles correctly
- [x] Log entries formatted with timestamp and source
- [x] Buffer limits enforced (50 logs, 20 commands)
- [x] Empty input ignored in SubmitCommand
- [x] ClearLog preserves command history
- [x] Thread-safe buffer access

---

## Design Rationale

### Why IDebugConsoleRenderer in Core Layer?

- **Prevents Circular Dependency:** Engine layer (CommandParser) needs to trigger console, but Terminal depends on Engine
- **Interface in Core:** Allows Engine to depend on abstraction without Terminal coupling
- **Implementation in Terminal:** Actual rendering uses Spectre.Console from Terminal layer

### Why Modal Loop Pattern?

- **Consistent with OptionsController:** Matches existing modal pattern in codebase
- **Input Isolation:** Prevents game input leaks during debugging
- **User Expectation:** Matches familiar console behavior from other games

### Why Thread-Safe Buffers?

- **Future-Proofing:** Prepares for Serilog sink integration that writes from background threads
- **Defensive Programming:** Prevents race conditions during concurrent access
- **Defensive Copies:** Properties return `ToList().AsReadOnly()` to prevent external mutation

### Why Fixed Log Format?

- **Simplicity:** No configuration overhead
- **Readability:** Timestamp + source provides context at a glance
- **Consistency:** All entries follow same pattern for easy parsing

---

## Changelog

### v1.0.0 (2025-12-25)

- Initial specification documenting v0.3.17a implementation
- Documents IDebugConsoleService, DebugConsoleService
- Documents IDebugConsoleRenderer, DebugConsoleRenderer
- Documents phase-agnostic interception in CommandParser
- Documents modal input loop and keyboard handling
- Documents built-in commands (help, clear, exit)
- Documents thread-safe buffer management
