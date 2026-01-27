# SPEC-LIBRARY-001: Library Service (Dynamic Knowledge Engine)

**Version:** 1.0.0 (v0.3.11a)
**Status:** Implemented
**Last Updated:** 2025-12-25
**Author:** The Architect
**Implementation:** `RuneAndRust.Engine/Services/LibraryService.cs`
**Tests:** `RuneAndRust.Tests/Engine/LibraryServiceTests.cs` (19 tests)

---

## Table of Contents

1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [Behaviors](#behaviors)
4. [Restrictions](#restrictions)
5. [Limitations](#limitations)
6. [Use Cases](#use-cases)
7. [Decision Trees](#decision-trees)
8. [Cross-System Integration](#cross-system-integration)
9. [Data Models](#data-models)
10. [Configuration](#configuration)
11. [Testing](#testing)
12. [Design Rationale](#design-rationale)
13. [Changelog](#changelog)

---

## Overview

The **Library Service** (`LibraryService`) is a reflection-based documentation engine that scans assembly metadata to generate dynamic Field Guide entries for *Rune & Rust*. It transforms code-level `[GameDocument]` attribute annotations into runtime-accessible `CodexEntry` objects, enabling automatic synchronization between game code and player-facing documentation.

### Purpose

- **Reflection-Based Documentation**: Scan assembly types for `[GameDocument]` attributes at runtime
- **Deterministic ID Generation**: Create stable, reproducible GUIDs from type/member names using MD5 hashing
- **Transient Entry Pattern**: Generate entries on-demand without database persistence
- **Category-Based Filtering**: Provide entries grouped by `EntryCategory` for Field Guide tabs
- **JournalService Integration**: Supply system entries for merge with database-persisted entries

### Key Features

1. **Assembly Scanning**:
   - Scans `RuneAndRust.Core` assembly via `typeof(EntryCategory).Assembly`
   - Discovers public types (classes, enums) decorated with `[GameDocument]`
   - Discovers public fields (enum values) decorated with `[GameDocument]`

2. **Attribute-Driven Documentation**:
   - `[GameDocument("Title", "Description", Category)]` defines entry metadata
   - Optional `IsSecret` property hides entries from Field Guide
   - Supports `AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Field`

3. **Deterministic ID Generation**:
   - ID = MD5 hash of `"TypeName:MemberName"` string
   - Same code produces identical IDs across sessions
   - Enables cross-referencing between JournalService and LibraryService

4. **Thread-Safe Caching**:
   - Lazy initialization on first access
   - `lock(_cacheLock)` prevents race conditions
   - Single scan per application lifetime

5. **JournalService Integration**:
   - `JournalService.BuildViewModelAsync` merges system entries with database entries
   - `BuildEntryDetailsAsync` falls back to LibraryService for system entries
   - Enables unified Field Guide display from dual sources

### System Context

**LibraryService** is a **read-only singleton** that provides game documentation at runtime. It does **not** persist data to the database; entries are regenerated on each application start from source code annotations.

**Dependencies**:
- `System.Reflection` - Assembly and attribute scanning
- `System.Security.Cryptography.MD5` - Deterministic ID generation
- `ILogger<LibraryService>` - Structured logging

**Dependents**:
- `JournalService` - Merges system entries with database entries for Field Guide
- `DocGenService` - Exports entries to Markdown files for external documentation
- UI Layer - Displays Field Guide entries via JournalService

---

## Core Concepts

### GameDocumentAttribute

**Purpose**: Marks code elements for inclusion in the dynamic Field Guide.

**Definition**:
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Field)]
public class GameDocumentAttribute : System.Attribute
{
    public string Title { get; }
    public string Description { get; }
    public EntryCategory Category { get; }
    public bool IsSecret { get; set; } = false;
}
```

**Properties**:
| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `Title` | `string` | Yes | - | Display name in Field Guide |
| `Description` | `string` | Yes | - | Full text content for entry |
| `Category` | `EntryCategory` | Yes | - | Categorization (FieldGuide, Bestiary, etc.) |
| `IsSecret` | `bool` | No | `false` | If `true`, excluded from Field Guide display |

**Usage Examples**:
```csharp
// Type-level (enum)
[GameDocument("Attack Types", "The various attack styles available in combat.", EntryCategory.FieldGuide)]
public enum AttackType { Standard, Light, Heavy }

// Field-level (enum value)
public enum DamageType
{
    [GameDocument("Slashing Damage", "Cuts and slices from bladed weapons.", EntryCategory.FieldGuide)]
    Slashing,

    [GameDocument("Bludgeoning Damage", "Impact force from blunt weapons.", EntryCategory.FieldGuide)]
    Bludgeoning
}

// Type-level (class) - Hidden
[GameDocument("Debug Commands", "Internal testing commands.", EntryCategory.FieldGuide, IsSecret = true)]
public class DebugCommands { }
```

### Deterministic ID Generation

**Purpose**: Create stable GUIDs that remain identical across application restarts.

**Algorithm**:
```csharp
private static Guid GenerateDeterministicId(string typeName, string memberName)
{
    // 1. Construct unique identifier string
    var identifier = $"{typeName}:{memberName}";

    // 2. Compute MD5 hash (16 bytes = GUID size)
    using var md5 = System.Security.Cryptography.MD5.Create();
    var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(identifier));

    // 3. Convert to GUID
    return new Guid(hashBytes);
}
```

**Examples**:
| Input | Identifier String | Generated GUID |
|-------|-------------------|----------------|
| `typeof(AttackType)` | `"AttackType:"` | `a1b2c3d4-...` |
| `DamageType.Slashing` | `"DamageType:Slashing"` | `e5f6a7b8-...` |

**Collision Avoidance**: The `TypeName:MemberName` format ensures uniqueness across types. Two fields with the same name in different enums produce different IDs.

### System Entries vs. Database Entries

| Aspect | System Entries (LibraryService) | Database Entries (CodexEntryRepository) |
|--------|--------------------------------|----------------------------------------|
| **Source** | Code attributes | Database rows |
| **Persistence** | Transient (regenerated on start) | Persistent (saved/loaded) |
| **ID Stability** | Deterministic (MD5 hash) | UUID (randomly generated) |
| **Content** | Static (from code) | Dynamic (player progress affects visibility) |
| **Use Case** | Field Guide mechanics | Bestiary, Lore discoveries |

---

## Behaviors

### 1. Get All System Entries (`GetSystemEntries`)

**Signature**: `IEnumerable<CodexEntry> GetSystemEntries()`

**Purpose**: Retrieve all `[GameDocument]`-annotated entries from the assembly.

**Behavior**:
```csharp
public IEnumerable<CodexEntry> GetSystemEntries()
{
    _logger.LogTrace("[Library] GetSystemEntries called");

    EnsureCachePopulated();

    _logger.LogDebug("[Library] Returning {Count} system entries", _cachedEntries.Count);
    return _cachedEntries.Values;
}

private void EnsureCachePopulated()
{
    if (_cachePopulated) return;

    lock (_cacheLock)
    {
        if (_cachePopulated) return;

        ScanAssemblyForEntries();
        _cachePopulated = true;
    }
}
```

**Return Value**: `IEnumerable<CodexEntry>` containing all non-secret entries.

**Logging**:
- **Trace**: Method entry
- **Debug**: Entry count returned

**Thread Safety**: Uses `lock(_cacheLock)` to ensure single scan.

---

### 2. Get Entries by Category (`GetEntriesByCategory`)

**Signature**: `IEnumerable<CodexEntry> GetEntriesByCategory(EntryCategory category)`

**Purpose**: Filter system entries by a specific category.

**Behavior**:
```csharp
public IEnumerable<CodexEntry> GetEntriesByCategory(EntryCategory category)
{
    _logger.LogTrace("[Library] GetEntriesByCategory called for {Category}", category);

    EnsureCachePopulated();

    var filtered = _cachedEntries.Values
        .Where(e => e.Category == category)
        .ToList();

    _logger.LogDebug("[Library] Returning {Count} entries for category {Category}",
        filtered.Count, category);

    return filtered;
}
```

**Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| `category` | `EntryCategory` | Filter criteria (FieldGuide, Bestiary, etc.) |

**Return Value**: `IEnumerable<CodexEntry>` filtered by category.

**Logging**:
- **Trace**: Method entry with category
- **Debug**: Filtered count and category

---

### 3. Get Entry by ID (`GetEntryById`)

**Signature**: `CodexEntry? GetEntryById(Guid id)`

**Purpose**: Retrieve a specific entry by its deterministic ID.

**Behavior**:
```csharp
public CodexEntry? GetEntryById(Guid id)
{
    _logger.LogTrace("[Library] GetEntryById called for {Id}", id);

    EnsureCachePopulated();

    if (_cachedEntries.TryGetValue(id, out var entry))
    {
        _logger.LogDebug("[Library] Found entry {Title} for ID {Id}", entry.Title, id);
        return entry;
    }

    _logger.LogDebug("[Library] No entry found for ID {Id}", id);
    return null;
}
```

**Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | `Guid` | Deterministic ID generated from type/member name |

**Return Value**: `CodexEntry?` (null if not found).

**Logging**:
- **Trace**: Method entry with ID
- **Debug**: Found entry title or "not found"

---

### 4. Assembly Scanning (`ScanAssemblyForEntries`)

**Purpose**: Scan assembly for `[GameDocument]` attributes and populate cache.

**Behavior**:
```csharp
private void ScanAssemblyForEntries()
{
    _logger.LogInformation("[Library] Scanning assembly for GameDocument attributes");

    var assembly = typeof(EntryCategory).Assembly; // RuneAndRust.Core
    var types = assembly.GetTypes();

    foreach (var type in types)
    {
        // 1. Check type-level attributes
        var typeAttr = type.GetCustomAttribute<GameDocumentAttribute>();
        if (typeAttr != null && !typeAttr.IsSecret)
        {
            var entry = CreateEntryFromAttribute(type.Name, "", typeAttr);
            AddEntryToCache(entry);
        }

        // 2. Check field-level attributes (enum values)
        if (type.IsEnum)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var fieldAttr = field.GetCustomAttribute<GameDocumentAttribute>();
                if (fieldAttr != null && !fieldAttr.IsSecret)
                {
                    var entry = CreateEntryFromAttribute(type.Name, field.Name, fieldAttr);
                    AddEntryToCache(entry);
                }
            }
        }
    }

    _logger.LogInformation("[Library] Scan complete. Found {Count} entries", _cachedEntries.Count);
}
```

**Scanning Scope**:
- **Assembly**: `RuneAndRust.Core` (via `typeof(EntryCategory).Assembly`)
- **Types**: All public types (classes, enums)
- **Fields**: Public static fields (enum values) using `BindingFlags.Public | BindingFlags.Static`

**Filtering**:
- Excludes entries where `IsSecret = true`
- Skips types/fields without `[GameDocument]` attribute

**Logging**:
- **Information**: Scan start and completion with entry count

---

### 5. Entry Creation (`CreateEntryFromAttribute`)

**Purpose**: Transform attribute data into `CodexEntry` object.

**Behavior**:
```csharp
private CodexEntry CreateEntryFromAttribute(string typeName, string memberName, GameDocumentAttribute attr)
{
    var id = GenerateDeterministicId(typeName, memberName);

    return new CodexEntry
    {
        Id = id,
        Title = attr.Title,
        FullText = attr.Description,
        Category = attr.Category,
        TotalFragments = 0, // System entries don't use fragments
        UnlockThresholds = new Dictionary<int, string>() // No progressive unlock
    };
}
```

**Field Mapping**:
| CodexEntry Property | Source |
|--------------------|--------|
| `Id` | MD5 hash of `"TypeName:MemberName"` |
| `Title` | `GameDocumentAttribute.Title` |
| `FullText` | `GameDocumentAttribute.Description` |
| `Category` | `GameDocumentAttribute.Category` |
| `TotalFragments` | `0` (system entries don't use fragments) |
| `UnlockThresholds` | Empty dictionary (no progressive unlock) |

---

## Restrictions

### Hard Constraints (MUST NOT Violate)

1. **MUST NOT modify assembly at runtime**:
   - LibraryService is read-only (reflection scanning only)
   - No dynamic code generation or attribute modification

2. **MUST NOT persist entries to database**:
   - System entries are transient (regenerated on each application start)
   - Database storage handled by `CodexEntryRepository` for player-discovered entries

3. **MUST exclude IsSecret entries from public methods**:
   - Entries marked `IsSecret = true` are filtered during scanning
   - No secret entries returned by `GetSystemEntries`, `GetEntriesByCategory`, `GetEntryById`

4. **MUST use deterministic ID generation**:
   - Same `TypeName:MemberName` always produces identical GUID
   - Enables stable cross-referencing with JournalService

5. **MUST scan RuneAndRust.Core assembly only**:
   - Assembly determined by `typeof(EntryCategory).Assembly`
   - Other assemblies (Engine, Terminal) not scanned for game documentation

### Soft Constraints (SHOULD Follow)

1. **SHOULD log scan results at Information level**:
   - Scan start and completion with entry count
   - Helps diagnose missing entries

2. **SHOULD handle duplicate IDs gracefully**:
   - Log warning if `TypeName:MemberName` collision occurs
   - First entry wins (subsequent duplicates skipped)

3. **SHOULD cache entries for entire application lifetime**:
   - Single scan on first access
   - No cache invalidation (code doesn't change at runtime)

---

## Limitations

### Design Limitations

1. **No Runtime Content Updates**:
   - Entries are generated from code at compile time
   - Cannot add/modify entries without recompilation

2. **No Rich Text Formatting**:
   - Descriptions are plain text only
   - No markdown, HTML, or Spectre.Console markup support

3. **No Image/Media Support**:
   - System entries contain text only
   - No attachment paths or media references

4. **No Localization**:
   - Titles and descriptions are hardcoded in source language
   - Would require attribute localization infrastructure

5. **Single Assembly Scope**:
   - Only scans `RuneAndRust.Core`
   - Cannot document types in Engine or Terminal assemblies

### Performance Limitations

1. **Synchronous Scanning**:
   - Assembly reflection is CPU-bound (not async)
   - Scan occurs on first access (may delay initial request)

2. **Full Type Enumeration**:
   - Scans all types in assembly (not filtered by namespace)
   - Large assemblies increase scan time

---

## Use Cases

### UC-LIBRARY-01: Generate Field Guide on Startup

**Scenario**: Application starts and JournalService needs Field Guide entries.

**Pre-Conditions**:
- `RuneAndRust.Core` assembly contains 13 annotated enums with 75+ entries
- LibraryService cache is empty

**Execution**:
```csharp
var libraryService = serviceProvider.GetRequiredService<ILibraryService>();
var systemEntries = libraryService.GetSystemEntries();

// Result: 75+ CodexEntry objects from annotated enums
```

**Post-Conditions**:
- Cache populated with all system entries
- Subsequent calls return cached entries (no re-scan)

---

### UC-LIBRARY-02: Filter Entries for FieldGuide Tab

**Scenario**: UI requests only FieldGuide entries for tab display.

**Pre-Conditions**:
- Cache contains entries across multiple categories

**Execution**:
```csharp
var fieldGuideEntries = libraryService.GetEntriesByCategory(EntryCategory.FieldGuide);

// Result: ~50 entries from FieldGuide category
```

**Post-Conditions**:
- Only FieldGuide entries returned
- Other categories (Bestiary, Lore) excluded

---

### UC-LIBRARY-03: Lookup Entry by Deterministic ID

**Scenario**: JournalService needs details for a specific system entry.

**Pre-Conditions**:
- Entry exists with ID generated from `"AttackType:"` (type-level annotation)

**Execution**:
```csharp
var id = GenerateDeterministicId("AttackType", ""); // Same algorithm
var entry = libraryService.GetEntryById(id);

// Result: CodexEntry with Title="Attack Types"
```

**Post-Conditions**:
- Entry returned with full details
- `null` returned if ID not found

---

### UC-LIBRARY-04: JournalService Merge Pattern

**Scenario**: JournalService builds Field Guide view merging system and database entries.

**Pre-Conditions**:
- LibraryService has 75 system entries
- Database has 0 player-discovered entries for FieldGuide category

**Execution**:
```csharp
// In JournalService.BuildViewModelAsync
var systemEntries = _libraryService.GetEntriesByCategory(EntryCategory.FieldGuide);
var dbEntries = await _codexRepository.GetByCategoryAsync(EntryCategory.FieldGuide);

var merged = systemEntries
    .Select(e => (Entry: e, CompletionPercent: 100)) // System entries always "complete"
    .Concat(dbEntries.Select(e => (Entry: e, CompletionPercent: /* from DataCaptureService */)))
    .ToList();
```

**Post-Conditions**:
- Unified list contains both system and database entries
- System entries show 100% completion (no fragments required)

---

## Decision Trees

### Decision Tree 1: Cache Access

```
Input: Any public method call
│
├─ Is cache populated (_cachePopulated)?
│  │
│  ├─ YES → Return cached data
│  │
│  └─ NO → Acquire lock
│     │
│     ├─ Re-check: Is cache populated?
│     │  │
│     │  ├─ YES → Release lock, return cached data
│     │  │
│     │  └─ NO → ScanAssemblyForEntries()
│     │     └─ Set _cachePopulated = true
│     │     └─ Release lock
│     │     └─ Return cached data
```

**Double-Check Pattern**: Prevents race condition where multiple threads attempt to scan simultaneously.

---

### Decision Tree 2: Attribute Processing

```
Input: Type from assembly
│
├─ Has [GameDocument] attribute (type-level)?
│  │
│  ├─ YES → Is IsSecret = true?
│  │  │
│  │  ├─ YES → Skip (do not add to cache)
│  │  │
│  │  └─ NO → CreateEntryFromAttribute(typeName, "", attr)
│  │     └─ AddEntryToCache(entry)
│  │
│  └─ NO → Continue to field scanning
│
├─ Is type an enum?
│  │
│  ├─ NO → Skip field scanning
│  │
│  └─ YES → For each public static field:
│     │
│     ├─ Has [GameDocument] attribute (field-level)?
│     │  │
│     │  ├─ YES → Is IsSecret = true?
│     │  │  │
│     │  │  ├─ YES → Skip
│     │  │  │
│     │  │  └─ NO → CreateEntryFromAttribute(typeName, fieldName, attr)
│     │  │     └─ AddEntryToCache(entry)
│     │  │
│     │  └─ NO → Skip field
```

---

## Cross-System Integration

### Integration 1: JournalService (Entry Consumer)

**Relationship**: `JournalService` → `ILibraryService`

**Integration Points**:

1. **FieldGuide Tab Population**:
   ```csharp
   // JournalService.cs
   var systemEntries = _libraryService.GetEntriesByCategory(EntryCategory.FieldGuide);
   ```

2. **Entry Detail Fallback**:
   ```csharp
   // JournalService.BuildEntryDetailsAsync
   var entry = await _codexRepository.GetByIdAsync(entryId);
   if (entry == null)
   {
       // Fallback to system entry
       entry = _libraryService.GetEntryById(entryId);
   }
   ```

**Data Flow**:
- LibraryService provides system entries (FieldGuide mechanics)
- JournalService merges with database entries (player-discovered lore)
- UI displays unified view

---

### Integration 2: DocGenService (Documentation Export)

**Relationship**: `DocGenService` → `ILibraryService`

**Integration Points**:

1. **Markdown Generation**:
   ```csharp
   // DocGenService.GenerateDocsAsync
   var entries = _libraryService.GetSystemEntries();
   var grouped = entries.GroupBy(e => e.Category);

   foreach (var group in grouped)
   {
       var markdown = GenerateCategoryMarkdown(group);
       await File.WriteAllTextAsync($"docs/generated/{group.Key}.md", markdown);
   }
   ```

**Data Flow**:
- LibraryService provides all system entries
- DocGenService groups by category and exports to Markdown
- External wikis consume generated documentation

---

## Data Models

### Entity: CodexEntry (Output)

**Purpose**: Standard entry format returned by LibraryService.

**Properties** (relevant subset):
```csharp
public class CodexEntry
{
    public Guid Id { get; set; }              // Deterministic MD5 hash
    public string Title { get; set; }          // From attribute
    public string FullText { get; set; }       // Description from attribute
    public EntryCategory Category { get; set; } // From attribute
    public int TotalFragments { get; set; }    // Always 0 for system entries
    public Dictionary<int, string> UnlockThresholds { get; set; } // Always empty
}
```

**System Entry Characteristics**:
- `Id`: Deterministic (same code = same ID)
- `TotalFragments`: 0 (no fragments for system entries)
- `UnlockThresholds`: Empty (no progressive unlock)

---

### Attribute: GameDocumentAttribute (Input)

**See [Core Concepts: GameDocumentAttribute](#gamedocumentattribute)** for full definition.

---

### Enum: EntryCategory

**Purpose**: Categorization for CodexEntry entities.

**Values** (FieldGuide-relevant subset):
```csharp
public enum EntryCategory
{
    FieldGuide,     // Game mechanics, tutorials
    Bestiary,       // Enemy entries
    BlightOrigin,   // Lore: Blight backstory
    Factions,       // Lore: Faction descriptions
    Technical,      // Lore: Aesir technology
    Geography       // Lore: World locations
}
```

---

## Configuration

### Annotated Enums (v0.3.11a)

The following 13 enums are annotated with `[GameDocument]` attributes:

| Enum | Entry Count | Category | Description |
|------|-------------|----------|-------------|
| `AttackType` | 3 | FieldGuide | Standard, Light, Heavy attacks |
| `DamageType` | 6 | FieldGuide | Physical, Elemental damage types |
| `StatusEffectType` | 12 | FieldGuide | Buffs, debuffs, conditions |
| `Attribute` | 5 | FieldGuide | Core character attributes |
| `DerivedStat` | 8 | FieldGuide | HP, Stamina, Defense, etc. |
| `ItemRarity` | 5 | FieldGuide | Common through Legendary |
| `ItemType` | 10 | FieldGuide | Weapons, Armor, Consumables |
| `WeaponCategory` | 6 | FieldGuide | Sword, Axe, Bow, etc. |
| `ArmorSlot` | 4 | FieldGuide | Head, Chest, Hands, Feet |
| `EquipmentSlot` | 8 | FieldGuide | All equipment positions |
| `TraumaType` | 5 | FieldGuide | Psychological trauma types |
| `CorruptionTier` | 4 | FieldGuide | Corruption progression |
| `BiomeType` | 6 | FieldGuide | Dungeon environment types |

**Total**: ~75+ annotated entries across 13 enums.

---

## Testing

### Test File: LibraryServiceTests.cs

**Location**: `RuneAndRust.Tests/Engine/LibraryServiceTests.cs`
**Test Count**: 19 tests

### Test Categories

#### Category 1: Assembly Scanning (5 tests)

| Test | Description |
|------|-------------|
| `GetSystemEntries_ReturnsNonEmptyCollection` | Verifies entries are found |
| `GetSystemEntries_ContainsExpectedEntry` | Specific entry title check |
| `GetSystemEntries_ExcludesSecretEntries` | IsSecret filtering |
| `GetSystemEntries_IncludesEnumValues` | Field-level attributes work |
| `GetSystemEntries_IncludesTypeLevel` | Type-level attributes work |

#### Category 2: Caching (4 tests)

| Test | Description |
|------|-------------|
| `GetSystemEntries_CachesResults` | Second call returns same objects |
| `GetSystemEntries_ThreadSafe` | Concurrent access doesn't crash |
| `GetSystemEntries_SingleScan` | Scan count verification |
| `GetEntriesByCategory_SharesCache` | All methods share same cache |

#### Category 3: ID Generation (5 tests)

| Test | Description |
|------|-------------|
| `GenerateDeterministicId_SameInput_SameOutput` | Consistency |
| `GenerateDeterministicId_DifferentInput_DifferentOutput` | Uniqueness |
| `GenerateDeterministicId_TypeLevel_Unique` | Type-level IDs unique |
| `GenerateDeterministicId_FieldLevel_Unique` | Field-level IDs unique |
| `GetEntryById_FindsCorrectEntry` | Lookup works |

#### Category 4: Filtering (5 tests)

| Test | Description |
|------|-------------|
| `GetEntriesByCategory_ReturnsOnlyMatchingCategory` | Filter accuracy |
| `GetEntriesByCategory_EmptyForUnusedCategory` | Empty result handling |
| `GetEntryById_ReturnsNullForUnknownId` | Not found handling |
| `GetEntriesByCategory_FieldGuide_HasExpectedCount` | Count verification |
| `GetSystemEntries_AllEntriesHaveRequiredFields` | Data integrity |

---

## Design Rationale

### Why Reflection-Based Documentation?

**Problem**: Game documentation (Field Guide) must stay synchronized with code.

**Solution**: `[GameDocument]` attributes live in source code next to the types they document. Changes to enums automatically update Field Guide entries on next application start.

**Alternative Considered**: Database-stored documentation would require manual synchronization and could drift from code reality.

### Why MD5 for ID Generation?

**Problem**: IDs must be stable across application restarts for cross-referencing.

**Solution**: MD5 produces a 128-bit hash (exactly GUID size) from `"TypeName:MemberName"`. Same input always produces same output.

**Alternative Considered**: GUID.NewGuid() would produce random IDs, breaking cross-session references.

### Why Transient Entries (No Database)?

**Problem**: System entries should reflect current code, not stale database state.

**Solution**: Regenerate entries on each application start from source code annotations. No migration required when code changes.

**Alternative Considered**: Database persistence would require migration scripts for each enum change.

### Why Lazy Initialization with Lock?

**Problem**: First access may occur from any thread; concurrent scans would waste resources.

**Solution**: Double-check locking pattern ensures single scan, even with concurrent initial access.

```csharp
if (_cachePopulated) return; // Fast path
lock (_cacheLock)
{
    if (_cachePopulated) return; // Re-check inside lock
    ScanAssemblyForEntries();
    _cachePopulated = true;
}
```

### Why Exclude Secret Entries?

**Problem**: Some documentation (debug commands, internal mechanics) shouldn't appear in player Field Guide.

**Solution**: `IsSecret = true` property allows developers to document code without exposing to players.

---

## Changelog

### v1.0.0 (v0.3.11a) - 2025-12-23

**Implemented Features**:
- `ILibraryService` interface with 3 methods
- `LibraryService` implementation with assembly scanning
- `GameDocumentAttribute` for code-level documentation
- MD5 deterministic ID generation
- Thread-safe lazy caching
- 13 annotated enums with 75+ entries
- JournalService integration (merge pattern)

**Test Coverage**:
- 19 tests across 4 categories
- 100% method coverage

**Related Releases**:
- v0.3.11a "The Living Guide" - Initial implementation

---

**End of SPEC-LIBRARY-001**
