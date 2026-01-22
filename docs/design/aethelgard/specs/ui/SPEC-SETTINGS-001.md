---
id: SPEC-SETTINGS-001
title: Settings Persistence System
version: 1.0.0
status: implemented
last_updated: 2025-12-25
related_specs: [SPEC-OPTIONS-001, SPEC-THEME-001, SPEC-LOC-001]
---

# SPEC-SETTINGS-001: Settings Persistence System

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `SettingsService`
> **Location:** `RuneAndRust.Engine/Services/SettingsService.cs`

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

The Settings Persistence System provides a JSON-based mechanism for saving and loading user preferences in Rune & Rust. Introduced in **v0.3.10a "The Preferences"**, this system bridges the gap between the static `GameSettings` class (runtime state) and the persistent `options.json` file on disk.

The architecture uses an immutable `SettingsDto` record for serialization, ensuring clean separation between the runtime state and the persistence format. All numeric settings undergo validation and clamping to prevent invalid values from corrupting the game state.

This system serves as the foundation for the Options Menu (SPEC-OPTIONS-001) and integrates with the Localization System (SPEC-LOC-001) for language persistence.

---

## Core Concepts

### SettingsDto (Data Transfer Object)

An immutable C# record used for JSON serialization/deserialization. The DTO mirrors the `GameSettings` static properties but uses primitive types suitable for JSON storage:

```csharp
public record SettingsDto
{
    public bool ReduceMotion { get; init; } = false;
    public int Theme { get; init; } = 0;           // ThemeType enum as int
    public int TextSpeed { get; init; } = 100;
    public int MasterVolume { get; init; } = 100;
    public int AutosaveIntervalMinutes { get; init; } = 5;
    public string Language { get; init; } = "en-US";
}
```

### GameSettings (Runtime State)

A static class providing global access to user preferences. The `SettingsService` populates these values at startup and persists them on save:

```csharp
public static class GameSettings
{
    public static bool ReduceMotion { get; set; }
    public static ThemeType Theme { get; set; }
    public static int TextSpeed { get; set; }
    public static int MasterVolume { get; set; }
    public static int AutosaveIntervalMinutes { get; set; }
    public static string Language { get; set; }
}
```

### Validation and Clamping

All numeric settings have defined valid ranges. Values outside these ranges are clamped to the nearest boundary with a warning log entry:

| Property | Min | Max | Default |
|----------|-----|-----|---------|
| TextSpeed | 10 | 200 | 100 |
| MasterVolume | 0 | 100 | 100 |
| AutosaveIntervalMinutes | 1 | 60 | 5 |

---

## Behaviors

### Primary Behaviors

#### 1. Load Settings (`LoadAsync`)

```csharp
Task LoadAsync()
```

**Purpose:** Loads user preferences from `data/options.json` and applies them to `GameSettings`.

**Logic:**
1. Check if `options.json` exists
2. If missing, call `ResetToDefaultsAsync()` to create default file
3. Read and deserialize JSON to `SettingsDto`
4. If deserialization fails (null result), reset to defaults
5. Apply DTO values to `GameSettings` via `ApplyDtoToSettings()`
6. Log the loaded theme and reduce motion state

**Example:**
```csharp
// At application startup
await settingsService.LoadAsync();
// GameSettings.Theme now reflects persisted value
```

#### 2. Save Settings (`SaveAsync`)

```csharp
Task SaveAsync()
```

**Purpose:** Persists current `GameSettings` values to `data/options.json`.

**Logic:**
1. Create `SettingsDto` from current `GameSettings` values
2. Ensure `data/` directory exists (create if missing)
3. Serialize DTO to indented JSON
4. Write to `options.json`
5. Log success or error

**Example:**
```csharp
// After user changes a setting in Options menu
GameSettings.Theme = ThemeType.HighContrast;
await settingsService.SaveAsync();
```

#### 3. Reset to Defaults (`ResetToDefaultsAsync`)

```csharp
Task ResetToDefaultsAsync()
```

**Purpose:** Restores all settings to their default values and persists to disk.

**Logic:**
1. Set `GameSettings.ReduceMotion = false`
2. Set `GameSettings.Theme = ThemeType.Standard`
3. Set `GameSettings.TextSpeed = 100`
4. Set `GameSettings.MasterVolume = 100`
5. Set `GameSettings.AutosaveIntervalMinutes = 5`
6. Set `GameSettings.Language = "en-US"`
7. Call `SaveAsync()` to persist defaults
8. Log the reset action

**Example:**
```csharp
// User clicks "Reset to Defaults" in Options menu
await settingsService.ResetToDefaultsAsync();
```

### Internal Behaviors

#### 4. Apply DTO to Settings (`ApplyDtoToSettings`)

```csharp
private void ApplyDtoToSettings(SettingsDto dto)
```

**Purpose:** Validates and applies DTO values to `GameSettings` with range clamping.

**Validation Rules:**
- **ReduceMotion:** Boolean, no validation needed
- **Theme:** Validated via `Enum.IsDefined()`, defaults to `Standard` if invalid
- **TextSpeed:** Clamped to 10-200 range
- **MasterVolume:** Clamped to 0-100 range
- **AutosaveIntervalMinutes:** Clamped to 1-60 range
- **Language:** Validated non-empty, defaults to "en-US" if invalid

---

## Restrictions

### What This System MUST NOT Do

1. **No Direct File Access from Other Services:** Only `SettingsService` should read/write `options.json`. Other services access settings via `GameSettings` static class.

2. **No Validation Bypass:** All loaded values must pass through `ApplyDtoToSettings()` validation. Never assign raw DTO values directly to `GameSettings`.

3. **No Silent Failures:** All file I/O errors must be logged and handled gracefully (reset to defaults).

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| File Path | `data/options.json` | Hardcoded for simplicity; no cross-platform path resolution |
| Static State | Single instance | `GameSettings` is static, no per-profile settings support |
| No Encryption | Plain JSON | User preferences are non-sensitive data |
| Sync Only | No async file watch | Settings changes require explicit save; no hot-reload |

---

## Use Cases

### UC-1: First Launch Experience

**Actor:** New Player
**Trigger:** Game starts for the first time
**Preconditions:** No `data/options.json` exists

```csharp
// SettingsService.LoadAsync() detects missing file
if (!File.Exists(_optionsPath))
{
    await ResetToDefaultsAsync(); // Creates file with defaults
    return;
}
```

**Postconditions:** `options.json` created with default values, `GameSettings` populated

### UC-2: Corrupted Settings Recovery

**Actor:** System
**Trigger:** `LoadAsync()` encounters malformed JSON
**Preconditions:** `options.json` contains invalid JSON

```csharp
catch (JsonException ex)
{
    _logger.LogError(ex, "[Settings] Failed to parse...");
    await ResetToDefaultsAsync(); // Graceful recovery
}
```

**Postconditions:** `GameSettings` reset to defaults, corrupted file overwritten

### UC-3: Settings Round-Trip Persistence

**Actor:** Player
**Trigger:** Player modifies settings and restarts game
**Preconditions:** Player changes Theme to HighContrast, saves, exits

```csharp
// Session 1: Modify and save
GameSettings.Theme = ThemeType.HighContrast;
await settingsService.SaveAsync();

// Session 2: Load on restart
await settingsService.LoadAsync();
// GameSettings.Theme == ThemeType.HighContrast
```

**Postconditions:** Modified settings persist across sessions

---

## Decision Trees

### Load Settings Flow

```
┌─────────────────────────────┐
│  LoadAsync() called         │
└────────────┬────────────────┘
             │
    ┌────────┴────────┐
    │ File exists?    │
    └────────┬────────┘
             │
    ┌────────┼────────┐
    │ NO     │        │ YES
    ▼        │        ▼
ResetTo      │   Read JSON
Defaults     │        │
             │   ┌────┴────┐
             │   │ Valid?  │
             │   └────┬────┘
             │        │
             │   ┌────┼────┐
             │   │ NO │    │ YES
             │   ▼    │    ▼
             │ Reset  │  Apply with
             │ Defaults   Validation
             │        │    │
             └────────┴────┘
```

### Validation Flow

```
┌─────────────────────────────┐
│  ApplyDtoToSettings(dto)    │
└────────────┬────────────────┘
             │
    ┌────────┴────────┐
    │ For each property│
    └────────┬────────┘
             │
    ┌────────┼────────────┐
    │        │            │
    ▼        ▼            ▼
Boolean   Enum        Numeric
 (copy)  (validate)   (clamp)
    │        │            │
    │   ┌────┴────┐  ┌────┴────┐
    │   │ Valid?  │  │ In range?│
    │   └────┬────┘  └────┬────┘
    │        │            │
    │   Yes/No       Yes/No
    │   Use/Default  Use/Clamp
    │        │            │
    └────────┴────────────┘
             │
    ┌────────┴────────┐
    │ Apply to        │
    │ GameSettings    │
    └─────────────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `ThemeType` | [SPEC-THEME-001](./SPEC-THEME-001.md) | Theme enum for color scheme validation |
| `ILogger` | (Framework) | Structured logging for all operations |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| `OptionsController` | [SPEC-OPTIONS-001](./SPEC-OPTIONS-001.md) | Saves settings on menu exit |
| `LocalizationService` | [SPEC-LOC-001](../content/SPEC-LOC-001.md) | Reads `GameSettings.Language` for locale |
| `ScreenTransitionService` | [SPEC-TRANSITION-001](./SPEC-TRANSITION-001.md) | Checks `GameSettings.ReduceMotion` |
| `GameController` | [SPEC-GAME-001](../core/SPEC-GAME-001.md) | Loads settings at startup |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `SettingsService.cs` | Main service implementation | 1-190 |
| `SettingsDto.cs` | Serialization record | 1-45 |
| `GameSettings.cs` | Runtime static state | 1-48 |
| `ISettingsService.cs` | Interface contract | 1-26 |

### Supporting Types

| File | Purpose | Key Lines |
|------|---------|-----------|
| `ThemeType.cs` | Theme enum definition | (in SPEC-THEME-001) |

---

## Data Models

### SettingsDto

```csharp
public record SettingsDto
{
    // Accessibility
    public bool ReduceMotion { get; init; } = false;

    // Display
    public int Theme { get; init; } = 0;  // ThemeType as int

    // General
    public int TextSpeed { get; init; } = 100;
    public int MasterVolume { get; init; } = 100;
    public int AutosaveIntervalMinutes { get; init; } = 5;

    // Localization (v0.3.15b)
    public string Language { get; init; } = "en-US";
}
```

### Persisted JSON Example

```json
{
  "ReduceMotion": false,
  "Theme": 1,
  "TextSpeed": 100,
  "MasterVolume": 75,
  "AutosaveIntervalMinutes": 10,
  "Language": "en-US"
}
```

---

## Configuration

### Constants

```csharp
// File location
private readonly string _optionsPath = Path.Combine("data", "options.json");

// JSON serialization options
private static readonly JsonSerializerOptions JsonOptions = new()
{
    WriteIndented = true,
    PropertyNameCaseInsensitive = true
};
```

### Settings Ranges

| Setting | Default | Range | Purpose |
|---------|---------|-------|---------|
| ReduceMotion | `false` | Boolean | Disable visual effects for accessibility |
| Theme | `0` (Standard) | 0-4 | Color scheme (see SPEC-THEME-001) |
| TextSpeed | `100` | 10-200 | Typewriter effect speed percentage |
| MasterVolume | `100` | 0-100 | Audio volume percentage |
| AutosaveIntervalMinutes | `5` | 1-60 | Minutes between automatic saves |
| Language | `"en-US"` | String | Locale code (see SPEC-LOC-001) |

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `SettingsServiceTests.cs` | 14 | Load/Save/Reset/Validation |

### Critical Test Scenarios

1. **Default file creation** - Verifies `options.json` is created on first load
2. **Value persistence** - Round-trip save/load preserves all settings
3. **TextSpeed clamping** - Values below 10 clamped to 10, above 200 clamped to 200
4. **MasterVolume clamping** - Values below 0 clamped to 0, above 100 clamped to 100
5. **AutosaveInterval clamping** - Values outside 1-60 range clamped
6. **Invalid theme handling** - Unknown theme values default to Standard
7. **Corrupt JSON recovery** - Malformed JSON triggers reset to defaults
8. **Directory creation** - `data/` directory created if missing on save

### Validation Checklist

- [x] LoadAsync creates default file when missing
- [x] LoadAsync applies values correctly from valid JSON
- [x] LoadAsync clamps out-of-range numeric values
- [x] LoadAsync handles corrupt JSON gracefully
- [x] SaveAsync writes valid indented JSON
- [x] SaveAsync creates directory if missing
- [x] ResetToDefaultsAsync sets all default values
- [x] ResetToDefaultsAsync persists defaults to file
- [x] Round-trip persistence preserves all values

---

## Design Rationale

### Why Static GameSettings?

- **Global access:** Settings are needed across multiple services (rendering, audio, localization)
- **Simplicity:** No dependency injection needed for read-only access
- **Performance:** No lookup overhead for frequently-accessed properties like `ReduceMotion`

### Why SettingsDto Separate from GameSettings?

- **Serialization isolation:** DTO uses primitives (int for enum) while GameSettings uses typed enums
- **Forward compatibility:** DTO structure can evolve independently of runtime API
- **Validation boundary:** DTO values are untrusted input; GameSettings values are validated

### Why Clamp Instead of Reject?

- **User-friendly:** Players aren't shown error messages for slightly out-of-range values
- **Robustness:** Hand-edited JSON with typos (e.g., `TextSpeed: 250`) still works
- **Logging:** Clamped values are logged as warnings for developer awareness

### Why Reset on Corrupt JSON?

- **Recovery priority:** Getting the game running is more important than preserving corrupt data
- **User experience:** Silent recovery is preferred over crash or error dialog
- **Traceability:** Errors are logged for debugging if players report issues

---

## Changelog

### v1.0.0 (2025-12-25)

- Initial specification documenting v0.3.10a implementation
- Documents SettingsService, SettingsDto, and GameSettings
- Covers validation, clamping, and error handling behaviors
- Includes v0.3.15b Language property addition
