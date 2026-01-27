---
id: SPEC-LOC-001
title: Localization System
version: 1.0.0
status: Implemented
last_updated: 2025-12-24
related_specs: [SPEC-UI-001, SPEC-DESC-001]
---

# SPEC-LOC-001: Localization System

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `LocalizationService`, `PseudoLocalizer`
> **Location:** `RuneAndRust.Engine/Services/LocalizationService.cs`

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Dependencies](#dependencies)
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

The **Localization System** provides multi-language support for Rune & Rust through a type-safe, JSON-based architecture. Implemented across three phases (v0.3.15a-c), the system transforms hardcoded UI strings into a dynamic, resource-driven infrastructure that supports:

- **Type-Safe Key Constants** (`LocKeys`) - Compile-time validation prevents string typos
- **Hierarchical JSON Locale Files** - Human-readable nested structure flattened to O(1) lookup
- **Two-Tier Fallback Chain** - Primary locale → en-US fallback → key return for graceful degradation
- **Pseudo-Localization** - QA testing mode that reveals UI truncation and hardcoded strings
- **Live Locale Switching** - Change language without application restart

The system currently provides **100+ localized strings** for UI menus, options, character creation, and ASCII art, with infrastructure for future content localization.

---

## Core Concepts

### Type-Safe Key Registry

All localization keys are defined as `public const string` fields in the `LocKeys` static class. This pattern enables:
- **Compile-time validation** - Typos cause build errors, not runtime failures
- **IntelliSense support** - Autocomplete for all available keys
- **Refactoring safety** - Rename support across the codebase

**Naming Convention:**
```
Constant Name: UI_MainMenu_NewGame
Constant Value: "UI.MainMenu.NewGame"
Pattern: CATEGORY_Subcategory_Element → "Category.Subcategory.Element"
```

### Hierarchical JSON Flattening

Locale files use nested JSON for readability:
```json
{
  "UI": {
    "MainMenu": {
      "NewGame": "New Game"
    }
  }
}
```

The `LocalizationService` recursively flattens this to a `Dictionary<string, string>`:
```
"UI.MainMenu.NewGame" → "New Game"
```

This provides O(1) lookup while maintaining human-editable JSON structure.

### Two-Tier Fallback Chain

When a key is requested, the lookup order is:
1. **Primary Locale** - The user's selected language (e.g., "de-DE")
2. **Fallback Locale** - Always en-US for incomplete translations
3. **Key Return** - Returns the key itself for debugging visibility

This ensures incomplete translations gracefully degrade to English rather than showing broken UI.

### Pseudo-Localization

The special locale `qps-ploc` transforms English strings for QA testing:
- **Diacritics** - `a→á, e→é, c→ç, n→ñ` reveals character encoding issues
- **Brackets** - `[text]` identifies localized vs hardcoded strings
- **Expansion** - ~30% length increase reveals UI truncation

Example: `"New Game"` → `"[Ñéw Gámé__]"`

---

## Dependencies

### Injected Services

| Dependency | Interface | Purpose |
|------------|-----------|---------|
| Logger | `ILogger<LocalizationService>` | Structured logging for traceability |

### Constructor Signature

```csharp
public LocalizationService(ILogger<LocalizationService> logger)
```

### Consumed By

| Service | Usage |
|---------|-------|
| `MainMenuController` | Menu option text, version string, messages |
| `OptionsController` | Setting labels, rebind prompts |
| `OptionsScreenRenderer` | Headers, footers, tab names |
| `OptionsViewHelperService` | Theme names, key names, command names |
| `CreationWizard` | Step prompts, validation messages, success screen |
| `TitleScreenService` | Title logo, version display |

---

## Behaviors

### Primary Behaviors

#### 1. Load Locale (`LoadLocaleAsync`)

```csharp
public async Task<bool> LoadLocaleAsync(string locale)
```

**Purpose:** Loads a locale file and optional fallback for string lookup.

**Logic:**
1. If `locale == "qps-ploc"`:
   - Clear primary dictionary
   - Load en-US into fallback dictionary
   - Set `CurrentLocale = "qps-ploc"`
   - Return true (no file required)
2. Load primary locale from `data/locales/{locale}.json`
3. If primary fails, fall back to en-US
4. If `locale != "en-US"`, also load en-US into fallback dictionary
5. Log result and return success status

**Logging:**
```csharp
_logger.LogInformation("[Localization] Loaded locale {Locale} with {Count} strings", locale, count);
_logger.LogWarning("[Localization] Locale file not found: {Path}. Falling back to {Default}", path, "en-US");
_logger.LogInformation("[Localization] Pseudo-localization enabled");
```

#### 2. Get String (`Get`)

```csharp
public string Get(string key)
public string Get(string key, params object[] args)
```

**Purpose:** Retrieves a localized string by key with optional format arguments.

**Logic:**
1. If `CurrentLocale == "qps-ploc"`:
   - Get string from fallback dictionary
   - Transform via `PseudoLocalizer.Transform()`
   - Return transformed string
2. Try primary dictionary lookup
3. If not found, try fallback dictionary lookup (log debug)
4. If still not found, return the key itself (for visibility)
5. If args provided, apply `string.Format()` with error handling

**Example:**
```csharp
// Simple lookup
var text = _loc.Get(LocKeys.UI_MainMenu_NewGame); // "New Game"

// Format string with placeholder
var text = _loc.Get(LocKeys.UI_Options_Units_Minutes, 5); // "5 min"
```

#### 3. Get Available Locales (`GetAvailableLocales`)

```csharp
public IReadOnlyList<string> GetAvailableLocales()
```

**Purpose:** Returns all installed locales for UI selection.

**Logic:**
1. Scan `data/locales/*.json` files
2. Extract locale codes from filenames
3. Always include `qps-ploc` (no file required)
4. Sort alphabetically
5. Return list

**Example Return:** `["en-US", "qps-ploc"]`

#### 4. Pseudo-Localization Transform (`PseudoLocalizer.Transform`)

```csharp
public static string Transform(string input)
```

**Purpose:** Transforms a string for QA testing.

**Algorithm:**
1. Return unchanged if null/empty
2. Append opening bracket `[`
3. Iterate characters:
   - If `{` found, preserve format placeholder until `}`
   - Otherwise, apply diacritic mapping if available
4. Append ~30% length expansion (underscores)
5. Append closing bracket `]`

**Diacritic Mappings (14 total):**
```csharp
['a'] = 'á', ['e'] = 'é', ['i'] = 'í', ['o'] = 'ó', ['u'] = 'ú',
['A'] = 'Á', ['E'] = 'É', ['I'] = 'Í', ['O'] = 'Ó', ['U'] = 'Ú',
['c'] = 'ç', ['C'] = 'Ç', ['n'] = 'ñ', ['N'] = 'Ñ'
```

**Example:**
```csharp
PseudoLocalizer.Transform("New Game") // "[Ñéw Gámé__]"
PseudoLocalizer.Transform("{0} min")  // "[{0} míñ_]"
```

---

## Restrictions

### What This System MUST NOT Do

1. **MUST NOT throw exceptions for missing keys** - Returns the key itself for debugging visibility
2. **MUST NOT modify format placeholders in pseudo-localization** - `{0}`, `{1:N2}` must remain functional
3. **MUST NOT load locale files synchronously** - All I/O operations are async
4. **MUST NOT expose internal dictionaries** - All access through `Get()` method

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| Default locale | en-US | Hardcoded fallback for all missing strings |
| Locale file location | `data/locales/` | Relative to app base directory |
| Key format | Dot-separated | e.g., "UI.MainMenu.NewGame" |
| JSON depth | Unlimited | Recursive flattening handles any depth |
| Pseudo-locale expansion | ~30% | Simulates German/Russian translation length |

### Current Scope

The v0.3.15 implementation covers:
- Main menu (8 keys)
- Options screen (37+ keys)
- Character creation wizard (17 keys)
- Title screen ASCII art (1 key)

**Not Yet Localized:**
- Combat messages
- Item descriptions
- Bestiary entries
- In-game journal

---

## Use Cases

### UC-1: Application Startup

**Actor:** Game Engine
**Trigger:** Application launch
**Preconditions:** `data/locales/en-US.json` exists

```csharp
// In Program.cs after DI registration
var settingsService = host.Services.GetRequiredService<ISettingsService>();
await settingsService.LoadAsync(); // Sets GameSettings.Language

var locService = host.Services.GetRequiredService<ILocalizationService>();
await locService.LoadLocaleAsync(GameSettings.Language);
```

**Postconditions:** Locale loaded, all UI services can retrieve strings

### UC-2: Change Language in Options

**Actor:** Player
**Trigger:** Select new language in Options > Display > Language

```csharp
// In OptionsController.HandleInputAsync()
if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.RightArrow)
{
    var direction = key.Key == ConsoleKey.LeftArrow ? -1 : 1;
    var locales = _loc.GetAvailableLocales();
    vm.Language = _viewHelper.CycleLanguage(vm.Language, direction, locales);

    // Live reload without restart
    _loc.LoadLocaleAsync(vm.Language).GetAwaiter().GetResult();
}
```

**Postconditions:** UI immediately displays in new language

### UC-3: Incomplete Translation Fallback

**Actor:** Translator
**Trigger:** French locale with 60/100 keys translated

```csharp
// User selects French
await _loc.LoadLocaleAsync("fr-FR");

// French key exists
_loc.Get("UI.MainMenu.NewGame"); // Returns "Nouveau Jeu"

// French key missing, fallback to en-US
_loc.Get("UI.Options.Tabs.Audio"); // Returns "Audio" (from en-US)
```

**Postconditions:** Mixed language display, no broken UI

---

## Decision Trees

### String Lookup Flow

```
Get(key) INVOKED
│
├─ CurrentLocale == "qps-ploc"?
│   └─ YES → Transform(fallbackStrings[key] ?? key)
│
├─ NO → _strings.TryGetValue(key)?
│   └─ FOUND → Return value
│
├─ NOT FOUND → _fallbackStrings.TryGetValue(key)?
│   └─ FOUND → Log debug, return fallback value
│
└─ NOT FOUND → Log debug, return key (for visibility)
```

### Locale Loading Flow

```
LoadLocaleAsync(locale) INVOKED
│
├─ locale == "qps-ploc"?
│   └─ YES → Clear _strings
│            Load en-US → _fallbackStrings
│            CurrentLocale = "qps-ploc"
│            Return true
│
├─ NO → Load {locale}.json → _strings
│   └─ FILE NOT FOUND?
│       └─ YES → Fallback to en-US
│
├─ locale != "en-US"?
│   └─ YES → Load en-US → _fallbackStrings
│
└─ Return success status
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `LocKeys` | *(internal)* | Key constants for type-safe lookup |
| File System | *(framework)* | JSON file reading |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| `OptionsScreenRenderer` | [SPEC-UI-001](../ui/SPEC-UI-001.md) | Headers, footers, legends |
| `OptionsController` | [SPEC-UI-001](../ui/SPEC-UI-001.md) | Setting labels, prompts |
| `CreationWizard` | [SPEC-UI-001](../ui/SPEC-UI-001.md) | All creation wizard text |
| `MainMenuController` | [SPEC-UI-001](../ui/SPEC-UI-001.md) | Menu options, version |
| `TitleScreenService` | [SPEC-UI-001](../ui/SPEC-UI-001.md) | Title logo |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `RuneAndRust.Engine/Services/LocalizationService.cs` | Core service implementation | 1-255 |
| `RuneAndRust.Engine/Helpers/PseudoLocalizer.cs` | Pseudo-locale transformer | 1-70 |
| `RuneAndRust.Core/Constants/LocKeys.cs` | Type-safe key constants | 1-382 |
| `RuneAndRust.Core/Interfaces/ILocalizationService.cs` | Service contract | 1-64 |

### Supporting Files

| File | Purpose |
|------|---------|
| `data/locales/en-US.json` | Master English locale file |
| `RuneAndRust.Terminal/Controllers/MainMenuController.cs` | Menu localization consumer |
| `RuneAndRust.Terminal/Rendering/OptionsViewHelperService.cs` | Options UI helper |

---

## Data Models

### LocKeys Static Class

```csharp
public static class LocKeys
{
    #region UI.MainMenu (8 keys)
    public const string UI_MainMenu_NewGame = "UI.MainMenu.NewGame";
    public const string UI_MainMenu_LoadGame = "UI.MainMenu.LoadGame";
    // ... 6 more
    #endregion

    #region UI.Options (37+ keys)
    public const string UI_Options_Title = "UI.Options.Title";
    // ... settings, tabs, themes, toggles, units, commands, categories, keys
    #endregion

    #region UI.Creation (17 keys)
    public const string UI_Creation_Title = "UI.Creation.Title";
    // ... prompts, validation, success screen
    #endregion

    #region Art.TitleScreen (1 key)
    public const string Art_TitleScreen_Logo = "Art.TitleScreen.Logo";
    #endregion

    // Reflection-based enumeration for validation
    public static IReadOnlyList<string> AllKeys => _allKeys.Value;
}
```

**Total Keys:** 100+ constants across 12 regions

### Locale File Schema (en-US.json)

```json
{
  "meta": {
    "locale": "en-US",
    "version": "0.3.15c",
    "name": "English (US)"
  },
  "UI": {
    "MainMenu": {
      "NewGame": "New Game",
      "LoadGame": "Load Game"
    },
    "Options": {
      "Title": "OPTIONS",
      "Settings": { /* ... */ },
      "Tabs": { /* ... */ }
    },
    "Creation": { /* ... */ }
  },
  "Art": {
    "TitleScreen": {
      "Logo": "╔══════════════════╗\n║  RUNE  &  RUST  ║\n╚══════════════════╝"
    }
  }
}
```

---

## Configuration

### Constants

```csharp
// LocalizationService.cs
private const string DefaultLocale = "en-US";
private readonly string _localesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "locales");
```

### Settings Integration

```csharp
// GameSettings.cs (v0.3.15b)
public static string Language { get; set; } = "en-US";

// SettingsDto.cs
public string Language { get; init; } = "en-US";
```

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `LocKeysTests.cs` | 7 | Key format, uniqueness, pattern validation |
| `LocalizationServiceTests.cs` | 20 | Loading, lookup, fallback, pseudo-locale |
| `PseudoLocalizerTests.cs` | 16 | Transform, diacritics, placeholders, expansion |

**Total: 43 tests (100% passing)**

### Critical Test Scenarios

1. **AllKeys_ShouldHaveValidFormat** - Validates dot-notation pattern
2. **AllKeys_ShouldBeUnique** - Ensures no duplicate key values
3. **LoadLocaleAsync_LoadsFallbackForNonDefaultLocale** - Verifies two-tier chain
4. **Get_ReturnsKey_WhenKeyMissing** - Confirms fallback behavior
5. **GetAvailableLocales_AlwaysIncludesPseudoLocale** - Pseudo-locale always available
6. **Transform_PreservesFormatPlaceholders** - `{0}` remains functional
7. **Transform_AppliesDiacritics** - All 14 mappings verified
8. **Transform_AddsExpansion** - ~30% length increase confirmed

### Validation Checklist

- [x] All 100+ keys have valid dot-notation format
- [x] No duplicate key values exist
- [x] en-US.json contains all keys from LocKeys.AllKeys
- [x] Format placeholders work with string.Format()
- [x] Pseudo-locale transforms correctly without breaking placeholders
- [x] Missing keys return key itself (not null/exception)
- [x] Fallback chain loads both dictionaries

---

## Design Rationale

### Why Type-Safe Key Constants?

- **Compile-time errors** - Catch typos during build, not at runtime
- **Refactoring support** - IDE rename works across entire codebase
- **IntelliSense** - Discoverability of available keys
- **Centralized documentation** - XML comments on each constant

### Why JSON over RESX?

- **Human-editable** - Translators can work with familiar format
- **Hierarchical** - Logical grouping visible in file structure
- **Cross-platform** - No Windows-specific tooling required
- **Flexible** - Can add metadata section, comments via `$schema`

### Why Return Key on Missing?

- **Debugging visibility** - Immediately see which strings are missing
- **Graceful degradation** - UI never crashes from missing translation
- **Development workflow** - Can test with incomplete locale files

### Why Dual-Dictionary Fallback?

- **Incremental translation** - Ship with partial translations
- **Community contributions** - Translators can start without completing all strings
- **Runtime efficiency** - O(1) lookup without cascading file reads

---

## Changelog

### v1.0.0 (2025-12-24)

**Initial Specification (Post-Implementation)**
- Documented v0.3.15a-c Localization System implementation
- Defined ILocalizationService interface (6 methods)
- Documented LocKeys registry (100+ keys across 12 regions)
- Documented LocalizationService with two-tier fallback
- Documented PseudoLocalizer for QA testing
- Documented integration with Options, Creation, Menu systems
- Referenced 43 passing tests across 3 test files

---

**END OF SPECIFICATION**
