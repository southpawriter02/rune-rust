# UI Specifications — Walkthrough

> **Status:** All specifications complete (code implementation pending)  
> **Date:** 2025-12-14

---

## Summary

The `docs/08-ui/` folder contains **15 comprehensive specifications** covering all UI systems needed for TUI and GUI implementation.

---

## Specification Inventory

### Core Architecture (4 specs)

| Spec | Purpose | Lines |
|------|---------|-------|
| [overview.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/ui-overview.md) | UI architecture, file structure | 156 |
| [presentation-layer.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/presentation-layer.md) | IPresenter, IInputHandler, IRenderTarget interfaces | 372 |
| [display-models.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/display-models.md) | Shared display record definitions | 16k bytes |
| [tui-layout.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/tui-layout.md) | Screen composition, activity log | 398 |

---

### Adapters (2 specs)

| Spec | Purpose | Lines |
|------|---------|-------|
| [terminal-adapter.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/terminal-adapter.md) | Full terminal implementation spec | 683 |
| [gui-adapter.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/gui-adapter.md) | Avalonia GUI implementation | 16k bytes |

---

### Feature UIs (3 specs)

| Spec | Purpose | Lines |
|------|---------|-------|
| [dialogue-ui.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/dialogue-ui.md) | Dialogue TUI/GUI screens | 570+ |
| [crafting-ui.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/crafting-ui.md) | Crafting interface | 49k bytes |
| [smart-commands.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/smart-commands.md) | Context-aware command suggestions | 9k bytes |

---

### Command Specs (6 specs)

| Spec | Purpose |
|------|---------|
| [commands/parser.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/commands/parser.md) | Grammar, target resolution, aliases |
| [commands/navigation.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/commands/navigation.md) | go, look, search commands |
| [commands/combat.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/commands/combat.md) | attack, defend, flee commands |
| [commands/interaction.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/commands/interaction.md) | pull, push, turn, press |
| [commands/inventory.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/commands/inventory.md) | equip, take, drop commands |
| [commands/system.md](file:///Volumes/GitHub/github/southpawriter02/r-r-251212/docs/08-ui/commands/system.md) | save, load, help, quit |

---

## Key Design Decisions (Dialogue UI)

Finalized during this session:

| Decision | Choice |
|----------|--------|
| Disposition | Tier only (`[Neutral]`) |
| Outcome tags | Revealed after selection |
| Leave button | Always visible |
| Conversation log | Optional, collapsed |

---

## Implementation Status

All specs are **documentation complete** but **code pending**:

| Layer | Status |
|-------|--------|
| `IPresenter` interface | ❌ Planned |
| `IInputHandler` interface | ❌ Planned |
| `IRenderTarget` interface | ❌ Planned |
| `TerminalPresenter` | ❌ Planned |
| `TerminalMapRenderer` | ❌ Planned |
| `CommandParser` | ❌ Planned |
| `DialogueViewModel` | ❌ Planned |
| GUI (Avalonia) | ❌ Planned |

---

## No Gaps Identified

All major UI systems have specifications. The following are documented:

- ✅ Core interfaces (IPresenter, IInputHandler, IRenderTarget, IMapRenderer)
- ✅ Terminal adapter (full implementation spec)
- ✅ GUI adapter (Avalonia ViewModels)
- ✅ Screen layouts (Exploration, Combat, Dialogue)
- ✅ Activity log system
- ✅ Command parser and dispatcher
- ✅ All command categories
- ✅ Dialogue UI with skill checks
- ✅ Crafting UI
- ✅ Smart commands
