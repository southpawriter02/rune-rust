---
id: SPEC-OPTIONS-001
title: Options Menu System
version: 1.0.0
status: implemented
last_updated: 2025-12-25
related_specs: [SPEC-SETTINGS-001, SPEC-THEME-001, SPEC-INPUT-001, SPEC-LOC-001]
---

# SPEC-OPTIONS-001: Options Menu System

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `OptionsController`, `OptionsScreenRenderer`, `OptionsViewHelperService`
> **Location:** `RuneAndRust.Terminal/Services/OptionsController.cs`

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

The Options Menu System provides a full-screen terminal UI for managing user preferences in Rune & Rust. Introduced in **v0.3.10b "The Control Panel"** with key rebinding added in **v0.3.10c "The Keymaster"**, this system implements a modal input loop with tabbed navigation, ASCII-rendered controls, and live preview of setting changes.

The architecture follows a Model-View-Controller pattern:
- **Model:** `OptionsViewModel` (mutable state container)
- **View:** `OptionsScreenRenderer` (Spectre.Console layout rendering)
- **Controller:** `OptionsController` (input handling, navigation, persistence)

All user-facing strings are localized via the Localization System (SPEC-LOC-001), enabling full internationalization support. Settings are persisted via the Settings Persistence System (SPEC-SETTINGS-001).

---

## Core Concepts

### OptionsTab Enum

Organizes settings into logical groups for easier navigation:

```csharp
public enum OptionsTab
{
    General,    // Autosave interval, reset to defaults
    Display,    // Theme, language, reduce motion, text speed
    Audio,      // Master volume
    Controls    // Key rebinding (v0.3.10c)
}
```

### SettingType Enum

Defines how settings are rendered and modified:

```csharp
public enum SettingType
{
    Toggle,   // Boolean on/off (Enter/Space)
    Slider,   // Numeric range (Left/Right arrows)
    Enum,     // Cycle through options (Left/Right arrows)
    Action    // Button (Enter/Space triggers)
}
```

### Modal Input Loop

The Options screen runs as a **blocking modal loop** that captures all keyboard input until the user exits with Escape/Q. This prevents game input from interfering with menu navigation.

### Live Preview

Setting changes are applied immediately to `GameSettings` for live preview (e.g., theme changes apply instantly). Persistence to disk occurs only on menu exit.

---

## Behaviors

### Primary Behaviors

#### 1. Run Options Loop (`RunAsync`)

```csharp
Task RunAsync()
```

**Purpose:** Main entry point that runs the modal options loop until user exits.

**Logic:**
1. Create `OptionsViewModel` from current `GameSettings`
2. Refresh visible items based on active tab
3. Enter infinite loop:
   - Render current state via `IOptionsScreenRenderer`
   - Read key input (blocking, intercepted)
   - Process input: Tab, arrows, Enter, Escape
   - Refresh items if values changed
4. On exit (Esc/Q): Save settings and bindings, return

**Example:**
```csharp
// Open options from main menu
await optionsController.RunAsync();
// Execution resumes here after user exits options
```

#### 2. Tab Navigation (`CycleTab`)

```csharp
private void CycleTab(OptionsViewModel vm, bool reverse)
```

**Purpose:** Cycles between tabs with Tab/Shift+Tab keys.

**Logic:**
1. Get all `OptionsTab` enum values as array
2. Find index of current active tab
3. Calculate next index: `(current + direction + count) % count`
4. Set new active tab, reset selected index to 0
5. Refresh items for new tab

#### 3. Setting Modification (`ModifySetting`)

```csharp
private void ModifySetting(OptionsViewModel vm, int direction)
```

**Purpose:** Modifies slider/enum values with Left/Right arrows.

**Logic:**
1. Get currently selected `SettingItemView`
2. Based on `SettingType`:
   - **Slider:** Add `step * direction`, clamp to min/max
   - **Enum:** Cycle theme or language in direction
   - **Toggle:** No effect (use Enter/Space instead)
3. Apply changes to ViewModel
4. Log the modification

#### 4. Key Rebinding (`HandleRebind`)

```csharp
private void HandleRebind(OptionsViewModel vm)
```

**Purpose:** Enters listening mode to capture a new key binding.

**Logic:**
1. Display "Press key for [Action]..." prompt
2. Render temporary listening view
3. Capture next key press (blocking)
4. If Escape, cancel and return
5. Check for conflict (key already bound to different command)
6. Remove old binding for this command (if any)
7. Set new binding (auto-overwrites existing for this key)
8. Log the rebind action

---

## Restrictions

### What This System MUST NOT Do

1. **No Game Logic:** Options menu only manages preferences. No combat, inventory, or gameplay logic.

2. **No Direct GameSettings Writes on Every Change:** Changes accumulate in `OptionsViewModel` with live preview. Disk persistence happens only on exit.

3. **No Reserved Key Bindings:** System keys (Escape, Tab) cannot be rebound to game actions in the Controls tab.

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| Max tabs | 4 | Fixed enum (General, Display, Audio, Controls) |
| Slider step | 5-10 | Coarse granularity for keyboard-only control |
| Bindings shown | 21 | Fixed list of rebindable commands |
| Language support | 2 | en-US and qps-ploc (pseudo-localization for testing) |

---

## Use Cases

### UC-1: Change Color Theme

**Actor:** Player
**Trigger:** Player opens Options and navigates to Display tab
**Preconditions:** Options menu is open

```csharp
// Navigate to Display tab, Theme setting
// Press Right arrow to cycle themes
vm.Theme = _viewHelper.CycleTheme(vm.Theme, +1);
// Live preview: screen redraws with new theme
ApplyToGameSettings(vm);
```

**Postconditions:** Theme visually changes immediately; saved on exit

### UC-2: Rebind Movement Key

**Actor:** Player
**Trigger:** Player presses Enter on "Move North" in Controls tab
**Preconditions:** Controls tab active, binding selected

```csharp
// Show listening prompt
var tempVm = new OptionsViewModel { ... }; // "Press key for Move North..."
_renderer.Render(tempVm);

// Capture W key
var keyInfo = Console.ReadKey(intercept: true);
// keyInfo.Key == ConsoleKey.W

// Check conflict: W was bound to nothing
// Set binding: W → "north"
_inputConfigService.SetBinding(ConsoleKey.W, "north");
```

**Postconditions:** Pressing W now moves player north; saved on exit

### UC-3: Reset to Defaults

**Actor:** Player
**Trigger:** Player activates "Reset to Defaults" action in General tab
**Preconditions:** General tab active, Reset action selected

```csharp
await _settingsService.ResetToDefaultsAsync();
// Refresh ViewModel from GameSettings
vm.ReduceMotion = GameSettings.ReduceMotion;
vm.Theme = (int)GameSettings.Theme;
// ... etc
```

**Postconditions:** All settings restored to defaults, UI updated

---

## Decision Trees

### Input Handling Flow

```
┌─────────────────────────────┐
│  Console.ReadKey()          │
└────────────┬────────────────┘
             │
    ┌────────┴────────┐
    │ Which key?      │
    └────────┬────────┘
             │
    ┌───┬───┬───┬───┬───┬────┐
    │   │   │   │   │   │    │
    ▼   ▼   ▼   ▼   ▼   ▼    ▼
  Tab  ↑/K ↓/J ←/H →/L Enter Esc
   │    │   │   │   │    │    │
   ▼    ▼   ▼   ▼   ▼    ▼    ▼
Cycle  Nav Nav Mod Mod Toggle Exit
 Tab   Up  Down -1  +1  /Act  Save
```

### Tab-Specific Content

```
┌─────────────────────────────┐
│  Which Tab Active?          │
└────────────┬────────────────┘
             │
    ┌────────┼────────┬────────┐
    │        │        │        │
    ▼        ▼        ▼        ▼
General   Display   Audio   Controls
    │        │        │        │
    ▼        ▼        ▼        ▼
Autosave  Theme    Volume   Bindings
 Reset   Language            list
        ReduceMot
        TextSpeed
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `ISettingsService` | [SPEC-SETTINGS-001](./SPEC-SETTINGS-001.md) | Loads, saves, resets preferences |
| `IThemeService` | [SPEC-THEME-001](./SPEC-THEME-001.md) | Color resolution for rendering |
| `IInputConfigurationService` | [SPEC-INPUT-001](./SPEC-INPUT-001.md) | Key binding management |
| `ILocalizationService` | [SPEC-LOC-001](../content/SPEC-LOC-001.md) | All UI strings |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| `TitleScreenService` | (not specified) | Opens options from main menu |
| `GameController` | [SPEC-GAME-001](../core/SPEC-GAME-001.md) | Opens options from pause menu |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `OptionsController.cs` | Modal input loop, navigation, persistence | 1-538 |
| `OptionsScreenRenderer.cs` | Full-screen Layout rendering | 1-253 |
| `OptionsViewHelperService.cs` | Formatting, localization helpers | 1-222 |

### Supporting Types

| File | Purpose | Key Lines |
|------|---------|-----------|
| `OptionsViewModel.cs` | Mutable state container | 1-107 |
| `OptionsTab.cs` | Tab enum definition | 1-28 |
| `SettingType.cs` | Setting control type enum | 1-28 |
| `IOptionsScreenRenderer.cs` | Rendering interface | 1-17 |

---

## Data Models

### OptionsViewModel

```csharp
public class OptionsViewModel
{
    // Navigation state
    public OptionsTab ActiveTab { get; set; } = OptionsTab.General;
    public int SelectedIndex { get; set; } = 0;

    // Cached display items
    public List<SettingItemView> CurrentItems { get; set; } = new();
    public List<BindingItemView> Bindings { get; set; } = new();  // Controls tab

    // Setting values (mutable for live preview)
    public bool ReduceMotion { get; set; }
    public int Theme { get; set; }
    public int TextSpeed { get; set; }
    public int MasterVolume { get; set; }
    public int AutosaveIntervalMinutes { get; set; }
    public string Language { get; set; } = "en-US";
}
```

### SettingItemView

```csharp
public record SettingItemView(
    string Name,           // Localized display name
    string ValueDisplay,   // Pre-formatted value string
    SettingType Type,      // Toggle, Slider, Enum, Action
    bool IsSelected,       // Highlight state
    int? MinValue = null,  // For Slider
    int? MaxValue = null,
    int? Step = null,
    string? PropertyName = null  // For value mapping
);
```

### BindingItemView

```csharp
public record BindingItemView(
    string ActionName,   // Localized command name
    string KeyDisplay,   // Formatted key with markup
    string Command,      // Internal command string
    string Category,     // Localized category for grouping
    bool IsSelected,
    bool IsUnbound       // True if no key assigned
);
```

---

## Configuration

### Tab Contents

| Tab | Settings | Types |
|-----|----------|-------|
| General | Autosave Interval, Reset to Defaults | Slider, Action |
| Display | Theme, Language, Reduce Motion, Text Speed | Enum, Enum, Toggle, Slider |
| Audio | Master Volume | Slider |
| Controls | 21 rebindable commands | Key bindings |

### Keyboard Controls

| Key | Action |
|-----|--------|
| Tab / Shift+Tab | Cycle tabs forward/backward |
| ↑ / K | Navigate up in list |
| ↓ / J | Navigate down in list |
| ← / H | Decrease slider / previous enum |
| → / L | Increase slider / next enum |
| Enter / Space | Toggle / activate action / rebind key |
| Escape / Q | Exit and save |

### Command Categories (Controls Tab)

| Category | Commands |
|----------|----------|
| Movement | north, south, east, west, up, down |
| Core | confirm, cancel, menu, help |
| Screens | inventory, character, journal, bench |
| Gameplay | interact, look, search, wait |
| Combat | attack, light, heavy |

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `OptionsViewHelperTests.cs` | 23 | Rendering, formatting, localization |

### Critical Test Scenarios

1. **Slider rendering** - Verifies ASCII bar accurately reflects value/range
2. **Toggle formatting** - Verifies color-coded ON/OFF display
3. **Theme cycling** - Verifies wrap-around at enum boundaries
4. **Language cycling** - Verifies available locales rotate correctly
5. **Key name formatting** - Verifies special keys (Space, arrows) display properly
6. **Category grouping** - Verifies commands grouped by correct localized categories

### Validation Checklist

- [x] Tab cycling wraps around at boundaries
- [x] Navigation respects list bounds
- [x] Slider values clamped to min/max
- [x] Theme changes apply immediately (live preview)
- [x] Language changes reload locale immediately
- [x] Key rebinding detects and resolves conflicts
- [x] Settings saved on exit (not on each change)
- [x] Bindings saved on exit via InputConfigurationService

---

## Design Rationale

### Why Modal Loop?

- **Input isolation:** Game input won't interfere with menu navigation
- **Clear exit condition:** Escape always exits, regardless of nested state
- **Render simplicity:** Single render call per loop iteration

### Why Mutable ViewModel?

- **Live preview:** Settings changes need immediate UI feedback
- **State accumulation:** Multiple changes before save/exit
- **Simplifies rendering:** Renderer reads current state without transforms

### Why Separate ViewHelper Service?

- **Testability:** Formatting logic can be unit tested without rendering
- **Localization:** Service can inject `ILocalizationService` for translations
- **Reusability:** Helper methods useful across multiple renderers

### Why Fixed Command List for Rebinding?

- **Scoped scope:** Not all commands should be rebindable (e.g., Escape)
- **Predictable UI:** Fixed list ensures consistent Controls tab layout
- **Easier validation:** Known commands simplify conflict detection

---

## Changelog

### v1.0.0 (2025-12-25)

- Initial specification documenting v0.3.10b/c implementation
- Documents OptionsController modal loop pattern
- Documents OptionsScreenRenderer Spectre.Console layout
- Documents OptionsViewHelperService formatting and localization
- Includes v0.3.15c language selection and live locale switching
