# Integrated Developer Tools & God Mode Specification

## 1. Overview

This document outlines the design for an **Integrated Developer Toolset** embedded directly within the *Rune & Rust* game client. Unlike the standalone *Forge Editor* (focused on static content creation), this toolset focuses on **real-time state manipulation**, **gameplay testing**, and **live debugging**.

It serves as a "Game Master" interface, allowing developers to inspect the running game loop, modify memory states on the fly, and persist tweaks to the database if desired.

### Goals
1.  **Immediacy**: Modify the world without restarting the client.
2.  **Omnipotence**: Bypass gameplay restrictions (God Mode, No-Clip, Instant Kill).
3.  **Inspection**: View invisible state (AI decision logs, RNG seeds, hidden rolls).
4.  **Safety**: Strictly segregated from Player builds to prevent cheating/spoilers.

---

## 2. Architecture & Security

To ensure players never access these tools, we will employ a dual-layer security strategy.

### 2.1 Compilation Guards
The entire DevTools assembly and its UI views will be wrapped in strict preprocessor directives.

```csharp
#if DEBUG || DEV_BUILD
    // DevTools initialization and UI registration
    services.AddSingleton<IDevToolsService, DevToolsService>();
#endif
```

*   **Release Builds**: The code is physically stripped from the executable.
*   **Dev Builds**: The tools are compiled in but hidden by default.

### 2.2 Activation
In Dev builds, the overlay is toggled via a specific key chord (e.g., `Ctrl + ~` or `F12`) or a startup argument `--enable-dev-tools`.

### 2.3 Integration Point
The DevTools will exist as an **Overlay Layer** in the Avalonia visual tree, sitting above the `GameView`.

```xml
<!-- MainWindow.axaml -->
<Panel>
    <views:GameView />

    <!-- Only present in Debug builds -->
    <devtools:DebugOverlayView IsVisible="{Binding IsDevToolsOpen}" />
</Panel>
```

It communicates with the core engine via a privileged interface `IDevToolsService` that exposes `GameEngine`, `GameState`, and `UnitOfWork`.

---

## 3. UI/UX Design

The interface mimics a "Head-Up Display" (HUD) or a docking IDE layout.

### 3.1 The "God Bar" (Toolbar)
A thin strip at the top of the screen for common actions:
*   **Play/Pause**: Freeze the `GameLoop` (stop time/ticks).
*   **Step**: Advance 1 turn or 1 second.
*   **Speed**: 1x, 2x, 10x game speed.
*   **Save/Load**: Instant state snapshotting (memory-only) for quick retry loops.
*   **No-Clip**: Toggle wall collision.
*   **God Mode**: Toggle invulnerability.

### 3.2 The "Inspector" (Side Panel)
Context-aware CRUD panel that changes based on selection.
*   **Selection Mode**: Click any object in the game view (enemy, chest, tile) to inspect it.
*   **Visual Debugging**: Draws bounding boxes, aggro radii, and pathfinding nodes overlaying the game world.

### 3.3 The "Console" (Bottom Panel)
A command-line interface for text-based manipulation and logging.
*   **Log Streams**: Filterable logs for `Combat`, `AI`, `Input`, `System`.
*   **Command Line**: Auto-completing command entry (e.g., `spawn item_sword_01`).

---

## 4. Functional Specifications

### 4.1 Player Manipulation (The "Avatar" Tab)
Allows modifying the local player state.

*   **Vitals**: Sliders/Inputs for HP, Stamina, Mana. "Fill" button.
*   **Progression**:
    *   `Set Level [N]`
    *   `Grant XP [Amount]`
    *   `Grant Legend [Amount]`
    *   `Grant PP [Amount]`
*   **Inventory**:
    *   **Search & Add**: Filterable list of all `ItemDefinitions`. Click to add to inventory.
    *   **Clear All**: Empty inventory.
    *   **Repair All**: Reset durability.
*   **Status Effects**:
    *   List active buffs/debuffs with duration.
    *   "Add Effect" dropdown (e.g., Poison, Haste).
    *   "Clear All" button.

### 4.2 Room Manipulation (The "World" Tab)
Real-time CRUD for the `RoomRuntimeState`.

*   **Entity List**: Tree view of all objects in the current room.
    *   `Enemies`
    *   `Interactables` (Doors, Chests, Levers)
    *   `Triggers` (Traps, Zones)
*   **Object Inspector**:
    *   **Properties**: Editable fields (Name, State, Position).
        *   *Example*: Select a `Door`. Change State from `Locked` -> `Open`.
        *   *Example*: Select a `Goblin`. Change HP `10/10` -> `1/10`.
    *   **Actions**: Context buttons.
        *   `Kill` / `Resurrect`
        *   `Delete` (Remove from instance)
        *   `Duplicate` (Clone instance)
*   **Puzzle Solver**:
    *   "Force Success": specific to `Puzzle` objects. Simulates correct input sequence.

### 4.3 Persistence & "Edit Mode"
This is the bridge between *Testing* and *Creating*.

1.  **Session Edit (Default)**: Changes affect `RoomRuntimeState` only. If you leave the room and come back (or reload), changes revert (unless the engine persists runtime state).
2.  **Definition Commit**:
    *   When an object is modified, a "Save to DB" button becomes active.
    *   **Action**: Updates the `world.room_object_states` (for this save) OR `world.room_object_definitions` (global game data).
    *   *Warning Prompt*: "You are modifying base game data. This will affect all new games. Confirm?"

### 4.4 Global Functions (The "System" Tab)
*   **Teleport**: Dropdown of `Dungeon` -> `Floor` -> `Room`. "Go" button.
*   **Time Warp**: "Advance Time" (1 Hour, 6 Hours, 1 Day). useful for testing respawns or lighting.
*   **Kill All Hostiles**: Nukes all enemies in the current room.
*   **Reveal Map**: Sets `IsMapped = true` for all rooms on current floor.

---

## 5. Technical Implementation (The Weeds)

### 5.1 IDevToolsService

```csharp
public interface IDevToolsService
{
    // Access to core internals
    GameEngine Engine { get; }

    // Command Execution
    void ExecuteCommand(string commandString);

    // Selection
    void SelectObject(Guid objectInstanceId);
    object? GetSelectedObject();

    // Manipulation
    void SetObjectProperty(Guid objectId, string propertyName, object value);
    void SpawnEntity(Guid definitionId, Vector2 position);

    // Overlays
    void EnableOverlay(DebugOverlayType type); // Grid, Pathing, Hitboxes
}
```

### 5.2 The "Visual Selector"
To implement "Click to Edit" in Avalonia:
1.  The Game View renders the scene.
2.  In `DebugMode`, the renderer draws invisible "Click Targets" (rectangles) over entities.
3.  `PointerPressed` events on the GameView are intercepted.
4.  Raycast/Hit-test against the `RoomRuntimeState` objects.
5.  If hit, populate the `InspectorViewModel`.

### 5.3 Scripting & Macros
To support repetitive testing (e.g., "Test Boss Phase 2"), we allow **Macros**.
*   **Format**: JSON or simple text list of console commands.
*   **Storage**: `data/macros/*.macro`.
*   **Example `boss_test.macro`**:
    ```text
    god_mode 1
    teleport dungeon_deep_keep floor_03 room_boss
    kill_all
    spawn_enemy boss_necromancer
    set_hp target 50%
    add_item potion_health_super 10
    ```

### 5.4 Database Write-Back
To support the "CRUD" aspect:
1.  **Read**: `Inspector` binds to `ObjectRuntimeState`.
2.  **Modify**: UI updates properties on `ObjectRuntimeState`.
3.  **Commit**:
    ```csharp
    public async Task CommitChangesToDefinitionAsync(ObjectRuntimeState runtimeState) {
        var def = await _repo.GetDefinitionAsync(runtimeState.DefinitionId);
        // Map runtime changes to definition
        def.BaseStats = runtimeState.Stats;
        def.LootTableId = runtimeState.LootTableId;
        await _unitOfWork.SaveChangesAsync();
    }
    ```

---

## 6. Development Phases

### Phase 1: The Eye (ReadOnly)
*   Implement `IDevToolsService` stub.
*   Create Overlay UI shell.
*   Implement `Inspector` showing read-only values of Player and Current Room.
*   Console log viewer.

### Phase 2: The Hand (State Manipulation)
*   Implement "God Bar" (Time, Pause, Speed).
*   Implement Player Stat editing.
*   Implement "Teleport" and "Give Item".

### Phase 3: The World Builder (CRUD)
*   Implement Object Selection (Clicking in view).
*   Implement Room Entity list.
*   Implement Property Editors (Reflection-based or typed).
*   Implement "Spawn" functionality.

### Phase 4: Persistence
*   Implement "Commit to DB" logic.
*   Macro system.
