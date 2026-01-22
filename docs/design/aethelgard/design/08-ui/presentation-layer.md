---
id: SPEC-UI-PRESENTATION-LAYER
title: "Presentation Layer — Interface Specification"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "RuneAndRust.Core/UI/IPresenter.cs"
    status: Planned
  - path: "RuneAndRust.Core/UI/IInputHandler.cs"
    status: Planned
  - path: "RuneAndRust.Core/UI/IRenderTarget.cs"
    status: Planned
---

# Presentation Layer — Interface Specification

---

## 1. Overview

Four core interfaces abstract the presentation layer from the game engine:

| Interface | Purpose | Terminal | GUI |
|-----------|---------|----------|-----|
| `IPresenter` | Messages, logs, prompts | Console output | Toast/log panels |
| `IInputHandler` | Commands, hotkeys | ReadLine parser | ReactiveCommand |
| `IRenderTarget` | Screen rendering | ASCII art | ViewModels |
| `IMapRenderer` | Map display | ASCII map | Canvas/tiles |

---

## 2. IPresenter

Handles discrete messages and prompts.

### 2.1 Interface Definition

```csharp
public interface IPresenter
{
    /// <summary>Display a message to the player</summary>
    void ShowMessage(string message, MessageType type = MessageType.Info);
    
    /// <summary>Add entry to combat log</summary>
    void ShowCombatLog(CombatLogEntry entry);
    
    /// <summary>Update a resource bar display</summary>
    void UpdateResource(ResourceType type, int current, int max);
    
    /// <summary>Display status effect application</summary>
    void ShowStatusEffect(string effectName, StatusChange change);
    
    /// <summary>Prompt player for choice from options</summary>
    Task<int> PromptChoiceAsync(string prompt, IReadOnlyList<string> options);
    
    /// <summary>Prompt player for text input</summary>
    Task<string> PromptTextAsync(string prompt);
    
    /// <summary>Show confirmation dialog</summary>
    Task<bool> ConfirmAsync(string message);
}

public enum MessageType
{
    Info,       // Normal information
    Success,    // Positive outcome
    Warning,    // Caution required
    Error,      // Something went wrong
    Combat,     // Combat-specific
    Loot,       // Item acquired
    Quest       // Quest update
}

public enum StatusChange
{
    Applied,
    Removed,
    Refreshed,
    Stacked
}
```

### 2.2 Combat Log Entry

```csharp
public record CombatLogEntry(
    string Message,
    CombatLogType Type,
    Guid? SourceId = null,
    Guid? TargetId = null,
    int? DamageAmount = null,
    bool IsCritical = false
);

public enum CombatLogType
{
    Attack,
    Damage,
    Heal,
    Miss,
    Block,
    Ability,
    StatusApplied,
    Death
}
```

---

## 3. IInputHandler

Handles player input and command routing.

### 3.1 Interface Definition

```csharp
public interface IInputHandler
{
    /// <summary>Fired when a command is received</summary>
    event Action<PlayerCommand>? OnCommandReceived;
    
    /// <summary>Start listening for input</summary>
    Task StartAsync(CancellationToken cancellationToken);
    
    /// <summary>Stop listening for input</summary>
    void Stop();
    
    /// <summary>Set current input context</summary>
    void SetContext(InputContext context);
    
    /// <summary>Register a command handler</summary>
    void RegisterCommand(string verb, Action<string[]> handler);
    
    /// <summary>Register a hotkey handler</summary>
    void RegisterHotkey(string key, Action handler);
}

public record PlayerCommand(
    string Verb,
    string[] Arguments,
    string RawInput
);

public enum InputContext
{
    MainMenu,
    Exploration,
    Combat,
    Dialogue,
    Inventory,
    CharacterSheet
}
```

### 3.2 Command Parser Specification

**Syntax**: `<verb> [target] [on <secondary_target>]`

```
attack goblin          → Verb: attack, Args: [goblin]
use skewer on orc      → Verb: use, Args: [skewer, on, orc]
go north               → Verb: go, Args: [north]
inventory              → Verb: inventory, Args: []
equip longsword        → Verb: equip, Args: [longsword]
```

### 3.3 Built-in Commands

| Verb | Aliases | Context | Action |
|------|---------|---------|--------|
| `attack` | `hit`, `a` | Combat | Attack target |
| `use` | `u` | Any | Use ability/item |
| `go` | `move`, `walk` | Exploration | Move direction |
| `look` | `examine`, `l` | Any | Inspect |
| `inventory` | `inv`, `i` | Any | Open inventory |
| `help` | `?`, `h` | Any | Show help |
| `quit` | `exit`, `q` | Any | Exit game |

---

## 4. IRenderTarget

Handles full-screen or panel rendering.

### 4.1 Interface Definition

```csharp
public interface IRenderTarget
{
    /// <summary>Clear the display</summary>
    void Clear();
    
    /// <summary>Render exploration view</summary>
    void RenderRoom(RoomDisplay room);
    
    /// <summary>Render combat view</summary>
    void RenderCombat(CombatDisplay combat);
    
    /// <summary>Render dialogue view</summary>
    void RenderDialogue(DialogueDisplay dialogue);
    
    /// <summary>Render inventory view</summary>
    void RenderInventory(InventoryDisplay inventory);
    
    /// <summary>Render character sheet</summary>
    void RenderCharacter(CharacterDisplay character);
    
    /// <summary>Render smart commands panel</summary>
    void RenderCommands(IReadOnlyList<SmartCommand> commands);
}
```

See: [display-models.md](display-models.md) for display record definitions.

---

## 5. IMapRenderer

Handles map-specific rendering (separate from main screen rendering for complexity).

### 5.1 Interface Definition

```csharp
public interface IMapRenderer
{
    /// <summary>Render minimap in exploration</summary>
    void RenderMinimap(MinimapDisplay minimap);
    
    /// <summary>Render full world/dungeon map</summary>
    void RenderWorldMap(WorldMapDisplay worldMap);
    
    /// <summary>Render room layout (furniture, objects)</summary>
    void RenderRoomLayout(RoomLayoutDisplay layout);
    
    /// <summary>Render combat grid</summary>
    void RenderCombatGrid(CombatGridDisplay grid);
    
    /// <summary>Update fog of war</summary>
    void UpdateFogOfWar(FogOfWarUpdate update);
    
    /// <summary>Highlight path</summary>
    void HighlightPath(IReadOnlyList<Position> path);
    
    /// <summary>Show territory control overlay</summary>
    void RenderTerritoryOverlay(TerritoryDisplay territory);
}
```

### 5.2 Map Display Models

```csharp
public record MinimapDisplay(
    IReadOnlyList<RoomNode> Rooms,
    Guid CurrentRoomId,
    IReadOnlyList<Connection> Connections
);

public record RoomNode(
    Guid RoomId,
    int X,
    int Y,
    RoomType Type,
    bool IsVisited,
    bool HasEnemies
);

public record CombatGridDisplay(
    int Width,
    int Height,
    IReadOnlyList<CellDisplay> Cells,
    IReadOnlyList<CombatantPosition> Combatants
);

public record CellDisplay(
    int X,
    int Y,
    TerrainType Terrain,
    bool IsOccupied,
    bool InMovementRange,
    bool InAbilityRange
);
```

---

## 6. Registration Pattern

### 6.1 Dependency Injection

```csharp
// Terminal setup
services.AddSingleton<IPresenter, TerminalPresenter>();
services.AddSingleton<IInputHandler, TerminalInputHandler>();
services.AddSingleton<IRenderTarget, TerminalRenderTarget>();
services.AddSingleton<IMapRenderer, TerminalMapRenderer>();

// GUI setup
services.AddSingleton<IPresenter, GuiPresenter>();
services.AddSingleton<IInputHandler, GuiInputHandler>();
services.AddSingleton<IRenderTarget, GuiRenderTarget>();
services.AddSingleton<IMapRenderer, GuiMapRenderer>();
```

### 6.2 Factory Pattern (Runtime Selection)

```csharp
public class PresentationFactory
{
    public static IPresentationLayer Create(PresentationMode mode)
    {
        return mode switch
        {
            PresentationMode.Terminal => new TerminalPresentationLayer(),
            PresentationMode.Gui => new GuiPresentationLayer(),
            _ => throw new ArgumentException("Invalid mode")
        };
    }
}
```

---

## 7. Event Subscriptions

UI adapters subscribe to game events for reactive updates:

```csharp
public class TerminalPresenter : IPresenter
{
    private readonly IEventBus _eventBus;
    
    public TerminalPresenter(IEventBus eventBus)
    {
        _eventBus = eventBus;
        
        // Subscribe to events
        _eventBus.Subscribe<DamageDealtEvent>(OnDamage, priority: 150);
        _eventBus.Subscribe<StatusEffectAppliedEvent>(OnStatus, priority: 150);
    }
    
    private void OnDamage(DamageDealtEvent evt)
    {
        var crit = evt.WasCritical ? " CRITICAL!" : "";
        ShowCombatLog(new CombatLogEntry(
            $"→ {evt.FinalDamage} {evt.DamageType} damage{crit}",
            CombatLogType.Damage,
            evt.AttackerId,
            evt.TargetId,
            evt.FinalDamage,
            evt.WasCritical
        ));
    }
}
```

---

## 8. Implementation Status

| Component | File Path | Status |
|-----------|-----------|--------|
| IPresenter | `RuneAndRust.Core/UI/IPresenter.cs` | ❌ Planned |
| IInputHandler | `RuneAndRust.Core/UI/IInputHandler.cs` | ❌ Planned |
| IRenderTarget | `RuneAndRust.Core/UI/IRenderTarget.cs` | ❌ Planned |
| IMapRenderer | `RuneAndRust.Core/UI/IMapRenderer.cs` | ❌ Planned |
| Display Models | `RuneAndRust.Core/UI/DisplayModels.cs` | ❌ Planned |
| TerminalPresenter | `RuneAndRust.Terminal/TerminalPresenter.cs` | ❌ Planned |
| TerminalMapRenderer | `RuneAndRust.Terminal/TerminalMapRenderer.cs` | ❌ Planned |
| GuiPresenter | `RuneAndRust.DesktopUI/GuiPresenter.cs` | ❌ Planned |
| GuiMapRenderer | `RuneAndRust.DesktopUI/GuiMapRenderer.cs` | ❌ Planned |
