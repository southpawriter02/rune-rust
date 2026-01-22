---
id: SPEC-UI-OVERVIEW
title: "UI Layer — Overview & Architecture"
version: 1.0
status: draft
last-updated: 2025-12-07
---

# UI Layer — Overview & Architecture

---

## 1. Purpose

The UI layer provides **presentation-agnostic** interfaces that support both terminal (CLI) and graphical (Avalonia GUI) frontends from a single game engine.

**Design Goals:**
- Terminal is fully functional for testing and gameplay
- GUI hooks exist for parallel development
- Single source of truth for game state (engine)
- Event-driven updates to all UI adapters

---

## 2. Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                    │
├───────────────────────┬─────────────────────────────────┤
│   Terminal Adapter    │          GUI Adapter            │
│   (Console output)    │     (Avalonia ViewModels)       │
├───────────────────────┴─────────────────────────────────┤
│               Presentation Abstraction Layer             │
│      IPresenter  │  IInputHandler  │  IRenderTarget     │
├─────────────────────────────────────────────────────────┤
│                       EVENT BUS                          │
│             (GameEvent → UI subscriptions)               │
├─────────────────────────────────────────────────────────┤
│                      GAME ENGINE                         │
│      (Combat, Exploration, Dialogue, Crafting)          │
├─────────────────────────────────────────────────────────┤
│                       DATA LAYER                         │
│          (Repositories, Persistence, State)              │
└─────────────────────────────────────────────────────────┘
```

---

## 3. Core Interfaces

| Interface | Responsibility |
|-----------|---------------|
| `IPresenter` | Messages, logs, prompts |
| `IInputHandler` | Commands, hotkeys, context |
| `IRenderTarget` | Combat, dialogue, inventory |
| `IMapRenderer` | Minimap, world map, room layout |

See: [presentation-layer.md](presentation-layer.md)

---

## 4. Adapters

| Adapter | Target | Implementation |
|---------|--------|----------------|
| Terminal | Console | ANSI colors, ASCII art |
| GUI | Avalonia | ViewModels, ReactiveUI |

See: [terminal-adapter.md](terminal-adapter.md), [gui-adapter.md](gui-adapter.md)

---

## 5. Command Syntax

**Format**: Verb-first

```
> attack goblin
> use skewer on orc
> go north
> inventory
> equip longsword
```

See: [commands/parser.md](commands/parser.md), [smart-commands.md](smart-commands.md)

---

## 6. Event-Driven Updates

UI adapters subscribe to game events:

```csharp
// Engine publishes
_eventBus.Publish(new DamageDealtEvent { ... });

// Terminal subscribes
_eventBus.Subscribe<DamageDealtEvent>(evt => 
    Console.WriteLine($"You deal {evt.FinalDamage} damage!"));

// GUI subscribes
_eventBus.Subscribe<DamageDealtEvent>(evt => 
    HealthBar.Value = evt.TargetCurrentHp);
```

See: [events.md](../01-core/events.md)

---

## 7. Priority Order

| Priority | Subscriber | Timing |
|----------|------------|--------|
| 0 | Logger | First |
| 50 | Game Logic | Early |
| 100 | Audio | Middle |
| 150 | UI | Last |

---

## 8. File Structure

```
docs/08-ui/
├── overview.md               ← You are here
├── presentation-layer.md     ← Core interfaces
├── terminal-adapter.md       ← Terminal implementation
├── tui-layout.md             ← Screen composition
├── dialogue-ui.md            ← Dialogue TUI/GUI screens
├── combat-ui.md              ← Combat TUI/GUI screens
├── inventory-ui.md           ← Inventory TUI/GUI screens
├── map-ui.md                 ← Map/navigation TUI/GUI screens
├── scavengers-journal-ui.md  ← Scavenger's Journal (Codex, Bestiary, Data Captures)
├── quest-journal-ui.md       ← Quest journal (Contracts tab of Scavenger's Journal)
├── character-sheet-ui.md     ← Character sheet TUI/GUI screens
├── crafting-ui.md            ← Crafting interface
├── skills-ui.md              ← Skills interface
├── smart-commands.md         ← Context-aware commands
├── display-models.md         ← Shared data objects
├── gui-adapter.md            ← Avalonia implementation
└── commands/
    ├── parser.md             ← Grammar & disambiguation
    ├── navigation.md         ← go, look, search
    ├── combat.md             ← attack, defend, flee
    ├── interaction.md        ← pull, push, turn, press
    ├── inventory.md          ← equip, take, drop
    └── system.md             ← save, load, help, quit
```

---

## 9. Implementation Status

| Component | Status |
|-----------|--------|
| IPresenter | ❌ Planned |
| IInputHandler | ❌ Planned |
| IRenderTarget | ❌ Planned |
| Terminal Adapter | ❌ Planned |
| GUI Adapter | ◐ Partial (legacy ViewModels) |
| Smart Commands | ❌ Planned |
