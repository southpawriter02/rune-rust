# SPEC-JOURNAL-001: Journal Display System

**ID:** SPEC-JOURNAL-001
**Version:** 1.0.1 (v0.3.7c)
**Status:** Implemented
**Last Updated:** 2025-12-25
**Author:** The Architect
**Implementation:** `RuneAndRust.Engine/Services/JournalService.cs` (361 lines)
**Tests:** `RuneAndRust.Tests/Engine/JournalServiceTests.cs` (393 lines, 16 tests)

---

## Table of Contents

1. [Overview](#overview)
2. [Core Behaviors](#core-behaviors)
3. [Restrictions](#restrictions)
4. [Limitations](#limitations)
5. [Use Cases](#use-cases)
6. [Decision Trees](#decision-trees)
7. [Sequence Diagrams](#sequence-diagrams)
8. [Workflows](#workflows)
9. [Cross-System Integration](#cross-system-integration)
10. [Data Models](#data-models)
11. [Configuration](#configuration)
12. [Testing](#testing)
13. [Domain 4 Compliance](#domain-4-compliance)
14. [Future Extensions](#future-extensions)
15. [Error Handling](#error-handling)
16. [Changelog](#changelog)

---

## Overview

The **Journal Display System** (`JournalService`) is responsible for rendering the **Scavenger's Journal** UI in *Rune & Rust*. It formats discovered `CodexEntry` data into player-facing text with **progressive text redaction** based on completion percentage. The system supports both **full-screen UI ViewModels** (v0.3.7c) and **legacy string-based formatting** for terminal output.

### Purpose

- **ViewModel Construction**: Build structured `JournalViewModel` for full-screen UI rendering
- **Text Redaction**: Apply progressive reveals to `CodexEntry` content based on discovery percentage
- **Entry List Formatting**: Group and display discovered entries by category (Bestiary, FieldGuide, Codex)
- **Entry Detail Formatting**: Show redacted content with unlocked threshold tags
- **Fragment Listing**: Display unassigned fragments (fragments without matching `CodexEntry`)

### Key Features

1. **Progressive Text Redaction**:
   - **TextRedactor** class applies word-level masking based on completion percentage
   - **0%**: Fully redacted (all words masked as `[grey]████[/]`)
   - **50%**: Half of words visible, half redacted
   - **100%**: Full text revealed

2. **Dual Output Modes**:
   - **ViewModel Mode** (v0.3.7c): `BuildViewModelAsync` for Avalonia UI binding
   - **String Formatting Mode** (Legacy): `FormatJournalListAsync`, `FormatEntryDetailAsync`, `FormatUnassignedCapturesAsync` for console output

3. **Tab-Based Categorization**:
   - **Bestiary**: Enemy entries (Iron-Husk, Rusted Servitor, etc.)
   - **FieldGuide**: Game mechanics (Combat Basics, Crafting Guide, etc.)
   - **Codex**: Lore entries (BlightOrigin, Factions, Technical, Geography)

4. **Threshold Unlocking**:
   - `CodexEntry.UnlockThresholds` defines discoveries at specific completion % (e.g., 25% → "WEAKNESS_REVEALED")
   - Displayed as formatted tags: `"WEAKNESS_REVEALED"` → `"Weakness Revealed"`

5. **Completion Icons**:
   - **Completed (100%)**: `[green]★[/]` (star icon)
   - **Incomplete (<100%)**: `[grey]●[/]` (circle icon)

6. **Spectre.Console Markup**:
   - All output uses Spectre.Console markup for terminal color formatting
   - Example: `"[yellow]═══ SCAVENGER'S JOURNAL ═══[/]"`

### System Context

**JournalService** is a **presentation layer** service that formats data from `DataCaptureService` and `CodexEntryRepository` for display. It does **not** perform game logic (fragment generation, completion calculation) — only formatting.

**Dependencies**:
- `IDataCaptureService` - Provides completion %, discovered entries, unlocked thresholds
- `IDataCaptureRepository` - Queries unassigned fragments
- `ICodexEntryRepository` - Loads `CodexEntry` entities
- `TextRedactor` - Applies progressive text masking

**Dependents**:
- `CommandParser` - Routes "journal" and "codex" commands to JournalService
- UI Layer (Avalonia) - Binds `JournalViewModel` to full-screen journal view
- `GameService` - Displays journal output via `IInputHandler.DisplayMessage`

---

## Core Behaviors

### 1. ViewModel Building (`BuildViewModelAsync`)

**Signature**: `Task<JournalViewModel> BuildViewModelAsync(Guid characterId, string characterName, JournalTab tab, int selectedIndex = 0, int stressLevel = 0)`

**Purpose**: Construct a structured `JournalViewModel` for UI data binding (v0.3.7c full-screen journal).

**Behavior**:

```csharp
// JournalService.cs:44-89
public async Task<JournalViewModel> BuildViewModelAsync(
    Guid characterId,
    string characterName,
    JournalTab tab,
    int selectedIndex = 0,
    int stressLevel = 0)
{
    _logger.LogTrace("[Journal] Building ViewModel for {CharacterId}, Tab={Tab}", characterId, tab);

    // 1. Get all discovered entries from DataCaptureService
    var discovered = await _captureService.GetDiscoveredEntriesAsync(characterId);
    var discoveredList = discovered.ToList();

    // 2. Filter by tab (map JournalTab to EntryCategory)
    var filtered = discoveredList
        .Where(e => MapCategoryToTab(e.Entry.Category) == tab)
        .OrderBy(e => e.Entry.Title)
        .Select((e, i) => new JournalEntryView(
            Index: i + 1,
            EntryId: e.Entry.Id,
            Title: e.Entry.Title,
            Category: e.Entry.Category,
            CompletionPercent: e.CompletionPercent,
            IsComplete: e.CompletionPercent >= 100
        ))
        .ToList();

    _logger.LogDebug("[Journal] Found {Count} entries for {Tab}", filtered.Count, tab);

    // 3. Build details for selected entry (if valid index)
    JournalEntryDetailView? details = null;
    if (selectedIndex >= 0 && selectedIndex < filtered.Count)
    {
        var selected = filtered[selectedIndex];
        details = await BuildEntryDetailsAsync(characterId, selected.EntryId);
    }

    // 4. Return complete ViewModel
    return new JournalViewModel(
        CharacterName: characterName,
        StressLevel: stressLevel,
        ActiveTab: tab,
        Entries: filtered,
        SelectedEntryIndex: selectedIndex,
        SelectedDetail: details
    );
}
```

**Tab Mapping** (`MapCategoryToTab`):
```csharp
// JournalService.cs:96-101
private static JournalTab MapCategoryToTab(EntryCategory category) => category switch
{
    EntryCategory.Bestiary => JournalTab.Bestiary,
    EntryCategory.FieldGuide => JournalTab.FieldGuide,
    _ => JournalTab.Codex // BlightOrigin, Factions, Technical, Geography
};
```

**Edge Cases**:
- **No Discovered Entries**: Returns empty `Entries` list (UI displays "No discoveries" message)
- **Invalid selectedIndex**: `SelectedDetail` is `null` (UI displays entry list only, no detail pane)
- **selectedIndex = -1**: Valid (no selection), `SelectedDetail` is `null`

**Logging**:
- **Trace**: Method entry with characterId and tab
- **Debug**: Entry count after filtering

---

### 2. Entry Detail Building (`BuildEntryDetailsAsync`)

**Signature**: `Task<JournalEntryDetailView?> BuildEntryDetailsAsync(Guid characterId, Guid entryId)`

**Purpose**: Construct detailed view data for a specific `CodexEntry` with redacted content and threshold tags.

**Behavior**:

```csharp
// JournalService.cs:109-134
private async Task<JournalEntryDetailView?> BuildEntryDetailsAsync(Guid characterId, Guid entryId)
{
    _logger.LogTrace("[Journal] Building details for Entry {EntryId}", entryId);

    // 1. Load CodexEntry entity
    var entry = await _codexRepository.GetByIdAsync(entryId);
    if (entry == null)
    {
        _logger.LogWarning("[Journal] Entry {EntryId} not found", entryId);
        return null;
    }

    // 2. Query completion data
    var pct = await _captureService.GetCompletionPercentageAsync(entryId, characterId);
    var thresholds = await _captureService.GetUnlockedThresholdsAsync(entryId, characterId);
    var fragmentCount = await _captureRepository.GetFragmentCountAsync(entryId, characterId);

    // 3. Apply text redaction
    var redactedText = _redactor.RedactText(entry.FullText, pct);

    // 4. Return detail view
    return new JournalEntryDetailView(
        EntryId: entry.Id,
        Title: entry.Title,
        Category: entry.Category,
        CompletionPercent: pct,
        RedactedContent: redactedText,
        UnlockedThresholds: thresholds.ToList(),
        FragmentsCollected: fragmentCount,
        FragmentsRequired: entry.TotalFragments
    );
}
```

**Text Redaction** (`TextRedactor.RedactText`):
```csharp
// Internal to TextRedactor class (composition object)
public string RedactText(string fullText, int completionPercent)
{
    var words = fullText.Split(' ');
    var visibleCount = (int)(words.Length * (completionPercent / 100.0));

    for (int i = visibleCount; i < words.Length; i++)
    {
        words[i] = "[grey]████[/]"; // Redact remaining words
    }

    return string.Join(" ", words);
}
```

**Edge Cases**:
- **Entry Not Found**: Returns `null`, logs warning
- **0% Completion**: All words redacted (fully masked)
- **100% Completion**: No redaction (full text visible)
- **Empty `FullText`**: Returns empty string (no crash)

**Logging**:
- **Trace**: Method entry with entryId
- **Warning**: Entry not found

---

### 3. Journal List Formatting (`FormatJournalListAsync`)

**Signature**: `Task<string> FormatJournalListAsync(Guid characterId)`

**Purpose**: Generate formatted string output for journal entry list (legacy terminal output).

**Behavior**:

```csharp
// JournalService.cs:141-182
public async Task<string> FormatJournalListAsync(Guid characterId)
{
    _logger.LogDebug("Formatting journal list for Character {CharacterId}", characterId);

    var entries = await _captureService.GetDiscoveredEntriesAsync(characterId);
    var entryList = entries.ToList();

    // Handle empty journal
    if (!entryList.Any())
    {
        _logger.LogDebug("No discovered entries for Character {CharacterId}", characterId);
        return "[yellow]═══ SCAVENGER'S JOURNAL ═══[/]\n\n" +
               "No discoveries recorded yet.\n" +
               "Examine objects and search containers to gather knowledge.";
    }

    var sb = new StringBuilder();
    sb.AppendLine("[yellow]═══ SCAVENGER'S JOURNAL ═══[/]");
    sb.AppendLine();

    // Group by category and sort by category enum value
    var grouped = entryList
        .GroupBy(e => e.Entry.Category)
        .OrderBy(g => g.Key);

    foreach (var group in grouped)
    {
        _logger.LogTrace("Processing category {Category} with {Count} entries", group.Key, group.Count());

        sb.AppendLine($"[cyan]── {group.Key} ──[/]"); // Category header

        foreach (var (entry, pct) in group.OrderBy(e => e.Entry.Title))
        {
            // Completion icon
            var statusIcon = pct >= 100 ? "[green]★[/]" : "[grey]●[/]";

            // Completion color
            var pctColor = pct >= 100 ? "green" : pct >= 50 ? "yellow" : "grey";

            sb.AppendLine($"  {statusIcon} {entry.Title} [{pctColor}]({pct}%)[/]");
        }
        sb.AppendLine(); // Blank line between categories
    }

    sb.AppendLine("[grey]Use 'codex <name>' to view entry details.[/]");

    _logger.LogDebug("Formatted {Count} entries for journal list", entryList.Count);
    return sb.ToString();
}
```

**Completion Color Coding**:
```
100%: [green] (fully discovered)
50-99%: [yellow] (partially discovered)
0-49%: [grey] (minimal discovery)
```

**Edge Cases**:
- **No Entries**: Returns help message with instructions
- **Single Entry**: Still grouped by category (single-item group)
- **Empty Category**: Not rendered (filtered by `GroupBy`)

**Logging**:
- **Debug**: Method entry and final entry count
- **Trace**: Category processing

---

### 4. Entry Detail Formatting (`FormatEntryDetailAsync`)

**Signature**: `Task<string> FormatEntryDetailAsync(Guid characterId, string entryTitle)`

**Purpose**: Generate formatted string output for a specific entry's details (legacy terminal output).

**Behavior**:

```csharp
// JournalService.cs:185-226
public async Task<string> FormatEntryDetailAsync(Guid characterId, string entryTitle)
{
    _logger.LogDebug("Formatting entry detail for '{EntryTitle}'", entryTitle);

    // 1. Load entry by title
    var entry = await _codexRepository.GetByTitleAsync(entryTitle);
    if (entry == null)
    {
        _logger.LogDebug("Entry '{EntryTitle}' not found", entryTitle);
        return $"[red]No entry found matching '{entryTitle}'.[/]";
    }

    // 2. Query completion data
    var pct = await _captureService.GetCompletionPercentageAsync(entry.Id, characterId);
    var thresholds = await _captureService.GetUnlockedThresholdsAsync(entry.Id, characterId);
    var thresholdList = thresholds.ToList();

    _logger.LogTrace("Entry {EntryTitle} has {Pct}% completion and {ThresholdCount} unlocked thresholds",
        entry.Title, pct, thresholdList.Count);

    // 3. Build formatted output
    var sb = new StringBuilder();
    sb.AppendLine($"[yellow]═══ {entry.Title.ToUpperInvariant()} ═══[/]");
    sb.AppendLine($"[cyan]Category:[/] {entry.Category}");
    sb.AppendLine($"[cyan]Completion:[/] {pct}%");
    sb.AppendLine();

    // 4. Show redacted content
    var displayText = _redactor.RedactText(entry.FullText, pct);
    sb.AppendLine(displayText);

    // 5. Show unlocked threshold tags (if any)
    if (thresholdList.Any())
    {
        sb.AppendLine();
        sb.AppendLine("[cyan]Discoveries:[/]");
        foreach (var tag in thresholdList)
        {
            sb.AppendLine($"  [green]✓[/] {FormatTag(tag)}");
        }
    }

    _logger.LogDebug("Formatted entry detail for '{EntryTitle}' at {Pct}%", entryTitle, pct);
    return sb.ToString();
}
```

**Threshold Tag Formatting** (`FormatTag`):
```csharp
// JournalService.cs:268-279
private static string FormatTag(string tag)
{
    if (string.IsNullOrEmpty(tag))
    {
        return tag;
    }

    // Convert SCREAMING_SNAKE_CASE to Title Case
    // "WEAKNESS_REVEALED" → "Weakness Revealed"
    return string.Join(" ", tag.Split('_').Select(word =>
        word.Length > 0
            ? char.ToUpper(word[0]) + word[1..].ToLower()
            : word));
}
```

**Edge Cases**:
- **Entry Not Found**: Returns red error message with entry title
- **No Thresholds Unlocked**: "Discoveries" section omitted
- **Empty FullText**: Displays empty content area (no crash)

**Logging**:
- **Debug**: Method entry, not found, and final formatting
- **Trace**: Completion % and threshold count

---

### 5. Unassigned Fragment Listing (`FormatUnassignedCapturesAsync`)

**Signature**: `Task<string> FormatUnassignedCapturesAsync(Guid characterId)`

**Purpose**: Display fragments that have not yet been linked to a `CodexEntry`.

**Behavior**:

```csharp
// JournalService.cs:229-257
public async Task<string> FormatUnassignedCapturesAsync(Guid characterId)
{
    _logger.LogDebug("Formatting unassigned captures for Character {CharacterId}", characterId);

    var unassigned = await _captureRepository.GetUnassignedAsync(characterId);
    var captureList = unassigned.ToList();

    // Handle no unassigned fragments
    if (!captureList.Any())
    {
        _logger.LogDebug("No unassigned captures for Character {CharacterId}", characterId);
        return "[grey]No unassigned fragments.[/]";
    }

    var sb = new StringBuilder();
    sb.AppendLine("[yellow]── Unassigned Fragments ──[/]");
    sb.AppendLine("[grey]These fragments don't match any known entries yet.[/]");
    sb.AppendLine();

    // Display fragments sorted by discovery time (newest first)
    foreach (var capture in captureList.OrderByDescending(c => c.DiscoveredAt))
    {
        _logger.LogTrace("Formatting unassigned capture {CaptureId}: {CaptureType}", capture.Id, capture.Type);

        sb.AppendLine($"  [cyan]{capture.Type}[/] - {capture.Source}");
        sb.AppendLine($"    \"{Truncate(capture.FragmentContent, 60)}\"");
    }

    _logger.LogDebug("Formatted {Count} unassigned captures", captureList.Count);
    return sb.ToString();
}
```

**Text Truncation** (`Truncate`):
```csharp
// JournalService.cs:287-295
private static string Truncate(string text, int maxLength)
{
    if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
    {
        return text;
    }

    return text[..(maxLength - 3)] + "..."; // Take first (maxLength - 3) chars + ellipsis
}
```

**Edge Cases**:
- **No Unassigned Fragments**: Returns grey "No unassigned fragments" message
- **Fragment Content Exceeds 60 Chars**: Truncated with "..." ellipsis
- **Null FragmentContent**: Returns empty string (no crash due to null check)

**Logging**:
- **Debug**: Method entry, no captures found, final count
- **Trace**: Individual fragment formatting

---

## Restrictions

### Hard Constraints (MUST NOT Violate)

1. **MUST NOT modify game state**:
   - JournalService is **read-only** (presentation layer)
   - No writes to database, no completion % updates
   - All state changes handled by `DataCaptureService`

2. **MUST NOT return HTML or unsafe markup**:
   - All output uses Spectre.Console markup (safe for terminal display)
   - No raw HTML, JavaScript, or executable code in formatted strings

3. **MUST handle missing entries gracefully**:
   - `BuildEntryDetailsAsync` returns `null` for missing entries (not exception)
   - `FormatEntryDetailAsync` returns error message (not crash)

4. **MUST apply redaction to all incomplete entries**:
   - No bypassing `TextRedactor` for privileged users
   - Completion % always determines redaction level

5. **MUST use DataCaptureService for completion data**:
   - Never calculate completion % in JournalService
   - Always query `GetCompletionPercentageAsync`, `GetUnlockedThresholdsAsync`

### Soft Constraints (SHOULD Follow)

1. **SHOULD sort entries alphabetically within categories**:
   - Improves UX (predictable ordering)
   - Current implementation: `OrderBy(e => e.Entry.Title)`

2. **SHOULD truncate fragment content to 60 characters**:
   - Prevents UI overflow in terminal output
   - Current truncation: `Truncate(capture.FragmentContent, 60)`

3. **SHOULD log all entry queries**:
   - Debug-level logs for method entry/exit
   - Trace-level logs for individual entry processing

---

## Limitations

### Design Limitations

1. **No Search Functionality**:
   - Cannot filter entries by keyword
   - Must manually browse categories to find specific entry

2. **Fixed Tab Structure**:
   - Three tabs only: Bestiary, FieldGuide, Codex
   - Cannot add custom tabs at runtime

3. **Word-Level Redaction Only**:
   - `TextRedactor` masks entire words (not character-level or sentence-level)
   - Example: "The ████ ████ ancient ████" (not "The rus███ ser████ ancient tech█████")

4. **No Pagination**:
   - All discovered entries rendered in single list
   - May cause performance issues with 100+ entries

5. **Completion Color Thresholds Hardcoded**:
   - 100% = green, 50-99% = yellow, 0-49% = grey
   - Not configurable per entry or category

6. **No Image/Media Support**:
   - Journal displays text only (no bestiary illustrations)
   - Future enhancement needed for visual content

### Performance Limitations

1. **Synchronous String Building**:
   - `StringBuilder` operations are CPU-bound (not async)
   - May block UI thread for large journals (100+ entries)

2. **Multiple Database Queries Per Detail View**:
   - `BuildEntryDetailsAsync` makes 3 separate queries:
     - `GetByIdAsync` (CodexEntry)
     - `GetCompletionPercentageAsync`
     - `GetUnlockedThresholdsAsync`
     - `GetFragmentCountAsync`
   - Could be optimized with single aggregate query

---

## Use Cases

### UC-JOURNAL-01: Display Empty Journal (New Player)

**Scenario**: A new player with no discovered entries opens the journal.

**Pre-Conditions**:
- Character: `{ Id = Guid, Name = "Anya" }`
- Discovered entries: `0`

**Execution**:
```csharp
var viewModel = await _journalService.BuildViewModelAsync(
    characterId: anya.Id,
    characterName: "Anya",
    tab: JournalTab.Bestiary,
    selectedIndex: 0,
    stressLevel: 0
);

// Result:
// viewModel.Entries = [] (empty list)
// viewModel.SelectedDetail = null
```

**Post-Conditions**:
- UI displays "No discoveries recorded yet" message
- No entries visible

**Code References**:
- `BuildViewModelAsync`: [JournalService.cs:44-89](JournalService.cs:44-89)

---

### UC-JOURNAL-02: Display Journal with Mixed Completion

**Scenario**: Player has 3 discovered entries at varying completion levels.

**Pre-Conditions**:
- Discovered entries:
  - "Rusted Servitor" (Bestiary, 50% complete)
  - "Combat Basics" (FieldGuide, 100% complete)
  - "Iron-Husk" (Bestiary, 25% complete)

**Execution (String Formatting)**:
```csharp
var output = await _journalService.FormatJournalListAsync(characterId);

// Output:
// [yellow]═══ SCAVENGER'S JOURNAL ═══[/]
//
// [cyan]── Bestiary ──[/]
//   [grey]●[/] Iron-Husk [grey](25%)[/]
//   [grey]●[/] Rusted Servitor [yellow](50%)[/]
//
// [cyan]── FieldGuide ──[/]
//   [green]★[/] Combat Basics [green](100%)[/]
//
// [grey]Use 'codex <name>' to view entry details.[/]
```

**Post-Conditions**:
- 2 categories displayed (Bestiary, FieldGuide)
- Entries sorted alphabetically within categories
- "Combat Basics" shows green star (completed)
- "Iron-Husk" and "Rusted Servitor" show grey circle (incomplete)

**Code References**:
- `FormatJournalListAsync`: [JournalService.cs:141-182](JournalService.cs:141-182)
- Completion icons: [JournalService.cs:171](JournalService.cs:171)
- Completion colors: [JournalService.cs:172](JournalService.cs:172)

---

### UC-JOURNAL-03: View Entry Detail with 30% Completion

**Scenario**: Player views "Rusted Servitor" entry with 30% completion.

**Pre-Conditions**:
- Entry: `{ Title = "Rusted Servitor", FullText = "A corroded automaton. Its servos grind with each movement. Appears hostile to organic lifeforms.", CompletionPercent = 30 }`
- Unlocked thresholds: `[]` (none yet)

**Execution**:
```csharp
var output = await _journalService.FormatEntryDetailAsync(characterId, "Rusted Servitor");

// Output (redacted):
// [yellow]═══ RUSTED SERVITOR ═══[/]
// [cyan]Category:[/] Bestiary
// [cyan]Completion:[/] 30%
//
// A corroded automaton. [grey]████[/] [grey]████[/] [grey]████[/] [grey]████[/] [grey]████[/] [grey]████[/] [grey]████[/] [grey]████[/] hostile [grey]████[/] organic [grey]████[/]
```

**Calculation**:
- Total words: 13
- Visible words (30%): 13 × 0.30 = 3.9 → 3 words
- First 3 words visible: "A corroded automaton."
- Remaining 10 words redacted: `[grey]████[/]`

**Post-Conditions**:
- Entry title displayed in uppercase
- Completion % shown (30%)
- Most text redacted (only 3/13 words visible)
- No "Discoveries" section (no thresholds unlocked)

**Code References**:
- `FormatEntryDetailAsync`: [JournalService.cs:185-226](JournalService.cs:185-226)
- `TextRedactor.RedactText`: Internal to TextRedactor class

---

### UC-JOURNAL-04: View Fully Completed Entry with Thresholds

**Scenario**: Player views "Iron-Husk" entry with 100% completion and unlocked thresholds.

**Pre-Conditions**:
- Entry: `{ Title = "Iron-Husk", FullText = "The Iron-Husks are Aesir security constructs. Weakness: EMP bursts disable their targeting systems.", CompletionPercent = 100 }`
- Unlocked thresholds: `["WEAKNESS_REVEALED", "ORIGIN_DISCOVERED"]`

**Execution**:
```csharp
var output = await _journalService.FormatEntryDetailAsync(characterId, "Iron-Husk");

// Output (full text):
// [yellow]═══ IRON-HUSK ═══[/]
// [cyan]Category:[/] Bestiary
// [cyan]Completion:[/] 100%
//
// The Iron-Husks are Aesir security constructs. Weakness: EMP bursts disable their targeting systems.
//
// [cyan]Discoveries:[/]
//   [green]✓[/] Origin Discovered
//   [green]✓[/] Weakness Revealed
```

**Post-Conditions**:
- Full text visible (no redaction)
- "Discoveries" section shown with 2 formatted tags
- Tags converted from SCREAMING_SNAKE_CASE to Title Case

**Code References**:
- `FormatEntryDetailAsync`: [JournalService.cs:185-226](JournalService.cs:185-226)
- `FormatTag`: [JournalService.cs:268-279](JournalService.cs:268-279)

---

### UC-JOURNAL-05: View Entry Not Found

**Scenario**: Player attempts to view non-existent entry.

**Pre-Conditions**:
- Requested entry: `"Unknown Beast"`
- Entry does not exist in database

**Execution**:
```csharp
var output = await _journalService.FormatEntryDetailAsync(characterId, "Unknown Beast");

// Output:
// [red]No entry found matching 'Unknown Beast'.[/]
```

**Post-Conditions**:
- Error message displayed in red
- No journal data shown
- Log entry: `"Entry 'Unknown Beast' not found"` (Debug level)

**Code References**:
- `FormatEntryDetailAsync` (not found path): [JournalService.cs:189-194](JournalService.cs:189-194)

---

### UC-JOURNAL-06: Display Unassigned Fragments

**Scenario**: Player has 2 unassigned fragments from container searches.

**Pre-Conditions**:
- Unassigned fragments:
  - `{ Type = TextFragment, Source = "Rusted Crate", FragmentContent = "Warning: Aetheric resonance detected in sector 7-B. Recommend evacuation of all non-essential personnel.", DiscoveredAt = 2025-12-22T10:30:00Z }`
  - `{ Type = TextFragment, Source = "Ancient Terminal", FragmentContent = "...system diagnostic complete. All servitors operational. Awaiting further orders.", DiscoveredAt = 2025-12-22T09:15:00Z }`

**Execution**:
```csharp
var output = await _journalService.FormatUnassignedCapturesAsync(characterId);

// Output:
// [yellow]── Unassigned Fragments ──[/]
// [grey]These fragments don't match any known entries yet.[/]
//
//   [cyan]TextFragment[/] - Rusted Crate
//     "Warning: Aetheric resonance detected in sector 7-B. Re..."
//   [cyan]TextFragment[/] - Ancient Terminal
//     "...system diagnostic complete. All servitors operation..."
```

**Post-Conditions**:
- Fragments sorted by discovery time (newest first: Rusted Crate, then Ancient Terminal)
- Content truncated to 60 characters with "..." ellipsis
- Fragment type and source displayed

**Code References**:
- `FormatUnassignedCapturesAsync`: [JournalService.cs:229-257](JournalService.cs:229-257)
- `Truncate`: [JournalService.cs:287-295](JournalService.cs:287-295)

---

### UC-JOURNAL-07: Build ViewModel for Full-Screen UI (Bestiary Tab)

**Scenario**: UI layer requests ViewModel for Bestiary tab with entry selection.

**Pre-Conditions**:
- Discovered entries:
  - "Iron-Husk" (Bestiary, 75% complete)
  - "Rusted Servitor" (Bestiary, 50% complete)
  - "Combat Basics" (FieldGuide, 100% complete)
- Selected index: `0` (first entry in filtered list)

**Execution**:
```csharp
var viewModel = await _journalService.BuildViewModelAsync(
    characterId: anya.Id,
    characterName: "Anya",
    tab: JournalTab.Bestiary,
    selectedIndex: 0,
    stressLevel: 3
);

// Result:
// viewModel.CharacterName = "Anya"
// viewModel.StressLevel = 3
// viewModel.ActiveTab = JournalTab.Bestiary
// viewModel.Entries = [
//   { Index = 1, Title = "Iron-Husk", CompletionPercent = 75, IsComplete = false },
//   { Index = 2, Title = "Rusted Servitor", CompletionPercent = 50, IsComplete = false }
// ]
// viewModel.SelectedEntryIndex = 0
// viewModel.SelectedDetail = {
//   Title = "Iron-Husk",
//   CompletionPercent = 75,
//   RedactedContent = "The Iron-Husks are Aesir security constructs. ████ ████ ████...",
//   UnlockedThresholds = ["ORIGIN_DISCOVERED"],
//   FragmentsCollected = 3,
//   FragmentsRequired = 4
// }
```

**Post-Conditions**:
- ViewModel contains 2 filtered entries (Bestiary only)
- "Combat Basics" (FieldGuide) excluded from list
- Selected entry details loaded with redacted content
- UI binds to ViewModel properties for rendering

**Code References**:
- `BuildViewModelAsync`: [JournalService.cs:44-89](JournalService.cs:44-89)
- `BuildEntryDetailsAsync`: [JournalService.cs:109-134](JournalService.cs:109-134)

---

### UC-JOURNAL-08: ViewModel with Invalid Selected Index

**Scenario**: UI layer requests ViewModel with out-of-bounds selected index.

**Pre-Conditions**:
- Discovered entries: 2 Bestiary entries
- Selected index: `5` (invalid, only 0-1 valid)

**Execution**:
```csharp
var viewModel = await _journalService.BuildViewModelAsync(
    characterId: anya.Id,
    characterName: "Anya",
    tab: JournalTab.Bestiary,
    selectedIndex: 5, // Invalid
    stressLevel: 0
);

// Result:
// viewModel.Entries = [2 entries]
// viewModel.SelectedEntryIndex = 5
// viewModel.SelectedDetail = null (index out of bounds)
```

**Post-Conditions**:
- ViewModel constructed successfully (no exception)
- `SelectedDetail` is `null`
- UI displays entry list without detail pane
- UI should validate index before calling BuildViewModelAsync

**Code References**:
- `BuildViewModelAsync` (index validation): [JournalService.cs:75-79](JournalService.cs:75-79)

---

### UC-JOURNAL-09: Tag Formatting (SCREAMING_SNAKE_CASE to Title Case)

**Scenario**: Display unlocked threshold tags in readable format.

**Pre-Conditions**:
- Raw threshold tags: `["WEAKNESS_REVEALED", "HABITAT_DISCOVERED", "COMBAT_TACTICS_UNLOCKED"]`

**Execution**:
```csharp
var formatted1 = FormatTag("WEAKNESS_REVEALED");
var formatted2 = FormatTag("HABITAT_DISCOVERED");
var formatted3 = FormatTag("COMBAT_TACTICS_UNLOCKED");

// Results:
// formatted1 = "Weakness Revealed"
// formatted2 = "Habitat Discovered"
// formatted3 = "Combat Tactics Unlocked"
```

**Algorithm**:
1. Split on `_` delimiter: `["WEAKNESS", "REVEALED"]`
2. For each word:
   - Uppercase first character: `"W"`
   - Lowercase remaining characters: `"eakness"`
   - Combine: `"Weakness"`
3. Join with space: `"Weakness Revealed"`

**Post-Conditions**:
- Tags displayed in human-readable Title Case
- Underscores replaced with spaces

**Code References**:
- `FormatTag`: [JournalService.cs:268-279](JournalService.cs:268-279)

---

### UC-JOURNAL-10: Truncate Long Fragment Content

**Scenario**: Display unassigned fragment with 100-character content.

**Pre-Conditions**:
- Fragment content: `"This is a very long fragment of text that was discovered in an ancient data terminal. It contains detailed technical specifications that exceed the display limit."`
- Content length: 150 characters
- Truncate limit: 60 characters

**Execution**:
```csharp
var truncated = Truncate(fragmentContent, 60);

// Result:
// truncated = "This is a very long fragment of text that was discovered..."
```

**Calculation**:
- Take first 57 characters: `"This is a very long fragment of text that was discovered"`
- Append ellipsis: `"..."`
- Total: 60 characters

**Post-Conditions**:
- Fragment content truncated to 60 characters
- Ellipsis indicates truncation
- Full content stored in database (not lost)

**Code References**:
- `Truncate`: [JournalService.cs:287-295](JournalService.cs:287-295)

---

## Decision Trees

### Decision Tree 1: ViewModel Building

```
Input: BuildViewModelAsync(characterId, tab, selectedIndex)
│
├─ Query discovered entries
│  └─ DataCaptureService.GetDiscoveredEntriesAsync(characterId)
│
├─ Filter by tab
│  ├─ tab = Bestiary → Include EntryCategory.Bestiary
│  ├─ tab = FieldGuide → Include EntryCategory.FieldGuide
│  └─ tab = Codex → Include all other categories (BlightOrigin, Factions, etc.)
│
├─ Sort entries
│  └─ OrderBy(e => e.Entry.Title) → Alphabetical
│
├─ Map to JournalEntryView DTOs
│  └─ Select((e, i) => new JournalEntryView(...))
│
├─ Validate selectedIndex
│  │
│  ├─ selectedIndex < 0 OR selectedIndex >= filtered.Count?
│  │  └─ YES → SelectedDetail = null (no selection)
│  │
│  └─ NO → BuildEntryDetailsAsync(characterId, entryId)
│     │
│     ├─ Load CodexEntry
│     ├─ Query completion %
│     ├─ Query unlocked thresholds
│     ├─ Query fragment count
│     └─ Apply text redaction → SelectedDetail = JournalEntryDetailView
│
└─ Return JournalViewModel
```

**Key Nodes**:
- **Tab Filtering**: Maps `JournalTab` to `EntryCategory` (many-to-one for Codex)
- **Index Validation**: Out-of-bounds index results in `null` detail (graceful degradation)
- **Detail Building**: Separate async call for selected entry only (not all entries)

---

### Decision Tree 2: Text Redaction

```
Input: RedactText(fullText, completionPercent)
│
├─ Split text into words
│  └─ words = fullText.Split(' ')
│
├─ Calculate visible word count
│  └─ visibleCount = (int)(words.Length × (completionPercent / 100.0))
│
├─ Loop through words
│  │
│  ├─ For each word at index i:
│  │  │
│  │  ├─ i < visibleCount?
│  │  │  └─ YES → Keep word visible (no change)
│  │  │
│  │  └─ NO → Redact word
│  │     └─ words[i] = "[grey]████[/]"
│  │
│  └─ Return string.Join(" ", words)
│
└─ Output: Redacted text
```

**Examples**:
- **0% Completion**: visibleCount = 0 → All words redacted
- **50% Completion**: visibleCount = words.Length / 2 → Half visible, half redacted
- **100% Completion**: visibleCount = words.Length → All visible, no redaction

---

### Decision Tree 3: Completion Color Selection

```
Input: completionPercent
│
├─ completionPercent >= 100?
│  └─ YES → Color = "green", Icon = "★"
│
├─ completionPercent >= 50?
│  └─ YES → Color = "yellow", Icon = "●"
│
└─ completionPercent < 50?
   └─ YES → Color = "grey", Icon = "●"
```

**Usage**: Applied in `FormatJournalListAsync` for entry list display.

---

### Decision Tree 4: Threshold Tag Formatting

```
Input: tag (SCREAMING_SNAKE_CASE)
│
├─ tag is null or empty?
│  └─ YES → Return tag (no change)
│
└─ NO → Process tag
   │
   ├─ Split on '_' delimiter
   │  └─ words = tag.Split('_')
   │
   ├─ For each word:
   │  │
   │  ├─ word.Length > 0?
   │  │  │
   │  │  ├─ YES → Capitalize first letter + lowercase rest
   │  │  │  └─ char.ToUpper(word[0]) + word[1..].ToLower()
   │  │  │
   │  │  └─ NO → Keep word (empty string)
   │  │
   │  └─ Join with space
   │     └─ string.Join(" ", words)
   │
   └─ Return formatted tag (Title Case)
```

**Example**: `"WEAKNESS_REVEALED"` → `["WEAKNESS", "REVEALED"]` → `["Weakness", "Revealed"]` → `"Weakness Revealed"`

---

## Sequence Diagrams

### Sequence Diagram 1: Full-Screen Journal ViewModel Building

```
Actor: UI Layer (Avalonia)
Service: JournalService
Service: DataCaptureService
Repository: CodexEntryRepository
Repository: DataCaptureRepository

UI -> JournalService: BuildViewModelAsync(characterId, tab=Bestiary, selectedIndex=0)
JournalService -> DataCaptureService: GetDiscoveredEntriesAsync(characterId)
DataCaptureService -> JournalService: Return [(Entry1, 75%), (Entry2, 50%), (Entry3, 100%)]
JournalService -> JournalService: Filter by tab (Bestiary) → [Entry1, Entry2]
JournalService -> JournalService: OrderBy(Title) → [Entry1, Entry2]
JournalService -> JournalService: Map to JournalEntryView DTOs
JournalService -> JournalService: Validate selectedIndex (0 is valid)
JournalService -> JournalService: BuildEntryDetailsAsync(characterId, Entry1.Id)
JournalService -> CodexEntryRepository: GetByIdAsync(Entry1.Id)
CodexEntryRepository -> JournalService: Return Entry1 entity
JournalService -> DataCaptureService: GetCompletionPercentageAsync(Entry1.Id, characterId)
DataCaptureService -> JournalService: Return 75
JournalService -> DataCaptureService: GetUnlockedThresholdsAsync(Entry1.Id, characterId)
DataCaptureService -> JournalService: Return ["ORIGIN_DISCOVERED"]
JournalService -> DataCaptureRepository: GetFragmentCountAsync(Entry1.Id, characterId)
DataCaptureRepository -> JournalService: Return 3
JournalService -> TextRedactor: RedactText(Entry1.FullText, 75)
TextRedactor -> JournalService: Return redacted text
JournalService -> JournalService: Construct JournalEntryDetailView
JournalService -> UI: Return JournalViewModel (Entries=[2], SelectedDetail=Entry1Details)
```

**Key Interactions**:
1. Single call to `GetDiscoveredEntriesAsync` retrieves all entries
2. Filtering and sorting performed in-memory (no additional queries)
3. Detail building makes 4 separate queries (optimization opportunity)
4. `TextRedactor` applies word-level masking based on completion %

---

### Sequence Diagram 2: Legacy String Formatting (Entry Detail)

```
Actor: CommandParser
Service: JournalService
Repository: CodexEntryRepository
Service: DataCaptureService

CommandParser -> JournalService: FormatEntryDetailAsync(characterId, "Iron-Husk")
JournalService -> CodexEntryRepository: GetByTitleAsync("Iron-Husk")
CodexEntryRepository -> JournalService: Return Entry entity (or null)
JournalService -> JournalService: Entry found?
JournalService -> DataCaptureService: GetCompletionPercentageAsync(Entry.Id, characterId)
DataCaptureService -> JournalService: Return 100
JournalService -> DataCaptureService: GetUnlockedThresholdsAsync(Entry.Id, characterId)
DataCaptureService -> JournalService: Return ["WEAKNESS_REVEALED", "ORIGIN_DISCOVERED"]
JournalService -> TextRedactor: RedactText(Entry.FullText, 100)
TextRedactor -> JournalService: Return full text (no redaction)
JournalService -> JournalService: FormatTag("WEAKNESS_REVEALED") → "Weakness Revealed"
JournalService -> JournalService: FormatTag("ORIGIN_DISCOVERED") → "Origin Discovered"
JournalService -> JournalService: Build formatted string with StringBuilder
JournalService -> CommandParser: Return formatted string with Spectre.Console markup
```

**Key Interactions**:
1. Entry lookup by title (not ID) for user-friendly command parsing
2. Thresholds formatted from SCREAMING_SNAKE_CASE to Title Case
3. Full text displayed if 100% complete (no redaction)
4. StringBuilder used for efficient string concatenation

---

### Sequence Diagram 3: Empty Journal Display

```
Actor: UI Layer
Service: JournalService
Service: DataCaptureService

UI -> JournalService: BuildViewModelAsync(characterId, tab=Bestiary, selectedIndex=0)
JournalService -> DataCaptureService: GetDiscoveredEntriesAsync(characterId)
DataCaptureService -> JournalService: Return [] (empty list)
JournalService -> JournalService: Filter by tab → [] (empty)
JournalService -> JournalService: selectedIndex validation (0 >= 0, but 0 >= 0 is true, but 0 < 0 is false)
JournalService -> JournalService: SelectedDetail = null (no entries to select)
JournalService -> UI: Return JournalViewModel (Entries=[], SelectedDetail=null)
UI -> UI: Render "No discoveries recorded yet" message
```

**Key Interactions**:
1. Empty discovered entries list results in empty `Entries` array
2. No detail building performed (no entries to select)
3. UI layer handles empty state rendering

---

## Workflows

### Workflow 1: Adding New Entry to Journal (End-to-End)

**Purpose**: Complete flow from fragment discovery to journal display.

**Steps**:
1. ✅ **Fragment Generation**: `DataCaptureService.TryGenerateFromSearchAsync()` creates fragment
2. ✅ **Entry Assignment**: Fragment auto-assigned to `CodexEntry` (if match found)
3. ✅ **Completion Update**: `DataCaptureService.RecalculateCompletion()` updates completion %
4. ✅ **Threshold Unlocking**: If completion crosses threshold (e.g., 25% → 50%), unlock tag
5. ✅ **Journal Query**: Player opens journal → `JournalService.BuildViewModelAsync()`
6. ✅ **Entry Display**: Entry appears in filtered list with completion %
7. ✅ **Detail View**: Player selects entry → redacted content displayed
8. ✅ **Progressive Reveal**: As player collects more fragments, redaction decreases

**Example Timeline**:
- **0 fragments**: Entry not discovered (not in journal)
- **1 fragment (10%)**: Entry appears in list, mostly redacted
- **5 fragments (50%)**: Half of text visible, "WEAKNESS_REVEALED" tag unlocked
- **10 fragments (100%)**: Full text visible, all thresholds unlocked

---

### Workflow 2: Building ViewModel for UI Rendering

**Purpose**: Construct `JournalViewModel` for Avalonia UI data binding.

**Steps**:
1. ✅ **Query Discovered Entries**: `DataCaptureService.GetDiscoveredEntriesAsync(characterId)`
2. ✅ **Filter by Tab**: Map `JournalTab` → `EntryCategory`, filter entries
3. ✅ **Sort Alphabetically**: `OrderBy(e => e.Entry.Title)`
4. ✅ **Map to DTOs**: Convert `(CodexEntry, int)` tuples to `JournalEntryView` records
5. ✅ **Validate Selected Index**: Check if `selectedIndex` is within bounds
6. ✅ **Build Entry Details** (if valid):
   - Load `CodexEntry` entity
   - Query completion %, unlocked thresholds, fragment count
   - Apply text redaction
   - Construct `JournalEntryDetailView`
7. ✅ **Return ViewModel**: `JournalViewModel` with Entries list and SelectedDetail

**Code Path**:
```
BuildViewModelAsync
  ├─ GetDiscoveredEntriesAsync (DataCaptureService)
  ├─ MapCategoryToTab (category filtering)
  ├─ OrderBy + Select (DTO mapping)
  └─ BuildEntryDetailsAsync (if selectedIndex valid)
     ├─ GetByIdAsync (CodexEntryRepository)
     ├─ GetCompletionPercentageAsync (DataCaptureService)
     ├─ GetUnlockedThresholdsAsync (DataCaptureService)
     ├─ GetFragmentCountAsync (DataCaptureRepository)
     └─ RedactText (TextRedactor)
```

---

### Workflow 3: Formatting Entry Detail for Terminal Output

**Purpose**: Generate Spectre.Console formatted string for terminal display.

**Steps**:
1. ✅ **Lookup Entry by Title**: `CodexEntryRepository.GetByTitleAsync(entryTitle)`
2. ✅ **Handle Not Found**: Return red error message if entry doesn't exist
3. ✅ **Query Completion Data**:
   - `GetCompletionPercentageAsync`
   - `GetUnlockedThresholdsAsync`
4. ✅ **Build Header**: Title (uppercase), category, completion %
5. ✅ **Apply Redaction**: `TextRedactor.RedactText(entry.FullText, pct)`
6. ✅ **Format Thresholds** (if any):
   - Convert SCREAMING_SNAKE_CASE to Title Case
   - Prepend green checkmark: `"[green]✓[/] Weakness Revealed"`
7. ✅ **Return Formatted String**: Spectre.Console markup with headers, content, tags

**Example Output**:
```
[yellow]═══ IRON-HUSK ═══[/]
[cyan]Category:[/] Bestiary
[cyan]Completion:[/] 75%

The Iron-Husks are Aesir security constructs. [grey]████[/] [grey]████[/] [grey]████[/] disable their targeting systems.

[cyan]Discoveries:[/]
  [green]✓[/] Origin Discovered
```

---

### Workflow 4: Displaying Unassigned Fragments

**Purpose**: Show fragments that haven't been auto-assigned to entries.

**Steps**:
1. ✅ **Query Unassigned Fragments**: `DataCaptureRepository.GetUnassignedAsync(characterId)`
2. ✅ **Handle Empty List**: Return grey "No unassigned fragments" message
3. ✅ **Sort by Discovery Time**: `OrderByDescending(c => c.DiscoveredAt)` (newest first)
4. ✅ **Truncate Content**: Limit FragmentContent to 60 characters with "..."
5. ✅ **Format Fragment**:
   - Type: `[cyan]TextFragment[/]`
   - Source: `"Rusted Crate"`
   - Content: `"Warning: Aetheric resonance detected..."`
6. ✅ **Return Formatted String**: Header + fragment list

**Use Case**: Player finds cryptic data fragments that don't match known bestiary/lore entries yet. Later, when the corresponding `CodexEntry` is added to the database, fragments are auto-assigned.

---

## Cross-System Integration

### Integration 1: DataCaptureService (Completion Data Provider)

**Relationship**: `JournalService` → `DataCaptureService`

**Integration Points**:
1. **Discovered Entries Query**:
   ```csharp
   var entries = await _captureService.GetDiscoveredEntriesAsync(characterId);
   // Returns: IEnumerable<(CodexEntry Entry, int CompletionPercent)>
   ```

2. **Completion Percentage Query**:
   ```csharp
   var pct = await _captureService.GetCompletionPercentageAsync(entryId, characterId);
   // Returns: int (0-100)
   ```

3. **Unlocked Thresholds Query**:
   ```csharp
   var thresholds = await _captureService.GetUnlockedThresholdsAsync(entryId, characterId);
   // Returns: IEnumerable<string> (threshold tags)
   ```

**Data Flow**:
- `DataCaptureService` maintains completion state (fragment counts, % calculations)
- `JournalService` consumes completion state for display formatting
- No circular dependency (one-way data flow)

---

### Integration 2: CodexEntryRepository (Entry Data Provider)

**Relationship**: `JournalService` → `ICodexEntryRepository`

**Integration Points**:
1. **Entry Lookup by ID**:
   ```csharp
   var entry = await _codexRepository.GetByIdAsync(entryId);
   // Used in: BuildEntryDetailsAsync
   ```

2. **Entry Lookup by Title**:
   ```csharp
   var entry = await _codexRepository.GetByTitleAsync("Iron-Husk");
   // Used in: FormatEntryDetailAsync (user command parsing)
   ```

**Data Flow**:
- `CodexEntryRepository` stores canonical entry data (FullText, Title, Category, etc.)
- `JournalService` reads entry data for display (never modifies)

---

### Integration 3: DataCaptureRepository (Fragment Data Provider)

**Relationship**: `JournalService` → `IDataCaptureRepository`

**Integration Points**:
1. **Unassigned Fragments Query**:
   ```csharp
   var unassigned = await _captureRepository.GetUnassignedAsync(characterId);
   // Returns: IEnumerable<DataCapture> (fragments without CodexEntryId)
   ```

2. **Fragment Count Query**:
   ```csharp
   var count = await _captureRepository.GetFragmentCountAsync(entryId, characterId);
   // Returns: int (number of fragments for entry)
   ```

**Data Flow**:
- `DataCaptureRepository` stores individual `DataCapture` fragments
- `JournalService` queries fragments for display (unassigned list, fragment counts)

---

### Integration 4: CommandParser (Command Routing)

**Relationship**: `CommandParser` → `JournalService`

**Integration Points**:
1. **"journal" Command**:
   ```csharp
   // CommandParser.cs (hypothetical)
   case "journal":
       var output = await _journalService.FormatJournalListAsync(characterId);
       _inputHandler.DisplayMessage(output);
       break;
   ```

2. **"codex <name>" Command**:
   ```csharp
   case "codex":
       var entryTitle = args[0]; // e.g., "Iron-Husk"
       var output = await _journalService.FormatEntryDetailAsync(characterId, entryTitle);
       _inputHandler.DisplayMessage(output);
       break;
   ```

**Data Flow**:
- `CommandParser` parses user input and routes to JournalService
- `JournalService` returns formatted strings
- `CommandParser` displays output via `IInputHandler`

---

### Integration 5: UI Layer (Avalonia ViewModel Binding)

**Relationship**: UI ViewModels → `JournalService`

**Integration Points**:
1. **Journal View ViewModel**:
   ```csharp
   // JournalViewModel.cs (hypothetical)
   public class JournalViewModel : ViewModelBase
   {
       private readonly IJournalService _journalService;

       public async Task LoadJournalAsync(JournalTab tab)
       {
           var model = await _journalService.BuildViewModelAsync(
               characterId: CurrentCharacter.Id,
               characterName: CurrentCharacter.Name,
               tab: tab,
               selectedIndex: SelectedIndex,
               stressLevel: CurrentCharacter.StressLevel
           );

           Entries = model.Entries;
           SelectedDetail = model.SelectedDetail;
       }
   }
   ```

2. **Data Binding** (XAML):
   ```xml
   <!-- JournalView.axaml -->
   <ListBox ItemsSource="{Binding Entries}" SelectedIndex="{Binding SelectedIndex}">
       <ListBox.ItemTemplate>
           <DataTemplate>
               <StackPanel>
                   <TextBlock Text="{Binding Title}" />
                   <TextBlock Text="{Binding CompletionPercent, StringFormat='{}{0}%'}" />
               </StackPanel>
           </DataTemplate>
       </ListBox.ItemTemplate>
   </ListBox>

   <TextBlock Text="{Binding SelectedDetail.RedactedContent}" />
   ```

**Data Flow**:
- UI calls `BuildViewModelAsync` on tab change or entry selection
- `JournalViewModel` returned with Entries and SelectedDetail
- Avalonia binds properties to UI controls

---

## Data Models

### Core ViewModel: JournalViewModel

**Purpose**: Structured data for full-screen journal UI (v0.3.7c).

**Definition**:
```csharp
public record JournalViewModel(
    string CharacterName,
    int StressLevel,
    JournalTab ActiveTab,
    List<JournalEntryView> Entries,
    int SelectedEntryIndex,
    JournalEntryDetailView? SelectedDetail
);
```

**Properties**:
- **CharacterName**: Player character name for header display
- **StressLevel**: Current stress level (0-10) for UI status bar
- **ActiveTab**: Currently selected tab (Bestiary, FieldGuide, Codex)
- **Entries**: Filtered and sorted list of entries for current tab
- **SelectedEntryIndex**: Index of selected entry in Entries list (-1 = no selection)
- **SelectedDetail**: Detail view data for selected entry (`null` if no selection or invalid index)

**Usage Context**:
- Returned by `BuildViewModelAsync`
- Bound to Avalonia UI controls
- Immutable record (structural equality)

---

### DTO: JournalEntryView

**Purpose**: Summary data for journal entry list display.

**Definition**:
```csharp
public record JournalEntryView(
    int Index,
    Guid EntryId,
    string Title,
    EntryCategory Category,
    int CompletionPercent,
    bool IsComplete
);
```

**Properties**:
- **Index**: 1-based display index in filtered list
- **EntryId**: Database ID for entry (used for detail loading)
- **Title**: Entry title (e.g., "Iron-Husk")
- **Category**: Category enum (Bestiary, FieldGuide, BlightOrigin, etc.)
- **CompletionPercent**: 0-100 discovery percentage
- **IsComplete**: `true` if CompletionPercent >= 100

**Usage Context**:
- List item in `JournalViewModel.Entries`
- Displayed in left panel of journal UI

---

### DTO: JournalEntryDetailView

**Purpose**: Detailed data for selected entry display.

**Definition**:
```csharp
public record JournalEntryDetailView(
    Guid EntryId,
    string Title,
    EntryCategory Category,
    int CompletionPercent,
    string RedactedContent,
    List<string> UnlockedThresholds,
    int FragmentsCollected,
    int FragmentsRequired
);
```

**Properties**:
- **EntryId**: Database ID
- **Title**: Entry title
- **Category**: Entry category
- **CompletionPercent**: 0-100 discovery percentage
- **RedactedContent**: Text with word-level masking applied by `TextRedactor`
- **UnlockedThresholds**: List of threshold tags (e.g., ["WEAKNESS_REVEALED"])
- **FragmentsCollected**: Number of fragments player has for this entry
- **FragmentsRequired**: Total fragments needed for 100% completion

**Usage Context**:
- `JournalViewModel.SelectedDetail`
- Displayed in right panel of journal UI

---

### Enum: JournalTab

**Purpose**: Categorization tabs for journal UI.

**Definition**:
```csharp
public enum JournalTab
{
    Bestiary,
    FieldGuide,
    Codex
}
```

**Mapping to EntryCategory**:
- **Bestiary** → `EntryCategory.Bestiary` (enemies, creatures)
- **FieldGuide** → `EntryCategory.FieldGuide` (game mechanics, tutorials)
- **Codex** → All other categories (`EntryCategory.BlightOrigin`, `Factions`, `Technical`, `Geography`)

**Usage Context**:
- `BuildViewModelAsync` tab parameter
- UI tab selection control

---

### Enum: EntryCategory

**Purpose**: Granular categorization for `CodexEntry` entities.

**Definition** (subset):
```csharp
public enum EntryCategory
{
    Bestiary,       // Enemy entries
    FieldGuide,     // Mechanic entries
    BlightOrigin,   // Lore: Blight backstory
    Factions,       // Lore: Faction descriptions
    Technical,      // Lore: Aesir technology
    Geography       // Lore: World locations
}
```

**Usage Context**:
- `CodexEntry.Category` property
- Filtered by `MapCategoryToTab` for tab display

---

### Entity: CodexEntry

**Purpose**: Canonical journal entry data (database entity).

**Properties** (relevant subset):
```csharp
public class CodexEntry
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public EntryCategory Category { get; set; }
    public string FullText { get; set; }
    public int TotalFragments { get; set; }
    public Dictionary<int, string> UnlockThresholds { get; set; } // { 25: "WEAKNESS_REVEALED", 50: "HABITAT_REVEALED", 75: "ORIGIN_DISCOVERED" }
}
```

**Usage Context**:
- Queried by `CodexEntryRepository`
- `FullText` redacted by `TextRedactor`
- `UnlockThresholds` processed by `DataCaptureService`, formatted by `JournalService`

---

### Entity: DataCapture

**Purpose**: Individual fragment discovered by player (database entity).

**Properties** (relevant subset):
```csharp
public class DataCapture
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Guid? CodexEntryId { get; set; } // NULL = unassigned
    public CaptureType Type { get; set; } // TextFragment, Image, Audio
    public string Source { get; set; } // "Rusted Crate", "Ancient Terminal"
    public string FragmentContent { get; set; } // Actual fragment text
    public DateTime DiscoveredAt { get; set; }
}
```

**Usage Context**:
- Queried by `DataCaptureRepository.GetUnassignedAsync` (where `CodexEntryId` is `NULL`)
- Displayed in `FormatUnassignedCapturesAsync`

---

## Configuration

### Constants (Hardcoded)

**Truncation Limit** (Unassigned Fragments):
```csharp
// JournalService.cs:252
Truncate(capture.FragmentContent, 60)
```
- **Value**: 60 characters
- **Purpose**: Prevent fragment content overflow in terminal output
- **Future**: Could be externalized to `appsettings.json`

**Completion Color Thresholds**:
```csharp
// JournalService.cs:172
var pctColor = pct >= 100 ? "green" : pct >= 50 ? "yellow" : "grey";
```
- **Thresholds**: 100% = green, 50-99% = yellow, 0-49% = grey
- **Purpose**: Color-code completion status in journal list
- **Future**: Could be configurable per entry or category

**Completion Icons**:
```csharp
// JournalService.cs:171
var statusIcon = pct >= 100 ? "[green]★[/]" : "[grey]●[/]";
```
- **Completed**: Green star (`★`)
- **Incomplete**: Grey circle (`●`)
- **Future**: Could support custom icons per category

---

## Testing

### Test File: JournalServiceTests.cs

**Location**: `/Users/ryan/Documents/GitHub/rune-rust/RuneAndRust.Tests/Engine/JournalServiceTests.cs`
**Lines**: 385 lines
**Test Count**: 16 tests

---

### Test Category 1: FormatJournalListAsync Tests (7 tests, lines 40-163)

**Purpose**: Validate journal entry list formatting with grouping, completion icons, and colors.

**Tests**:

1. **FormatJournalListAsync_NoEntries_ReturnsEmptyMessage** (lines 42-56)
   - **Scenario**: Character with no discovered entries
   - **Expected**: "No discoveries recorded yet" message

2. **FormatJournalListAsync_WithEntries_GroupsByCategory** (lines 58-81)
   - **Scenario**: 3 entries across 2 categories (Bestiary, FieldGuide)
   - **Expected**: Category headers displayed (`"── Bestiary ──"`, `"── FieldGuide ──"`)

3. **FormatJournalListAsync_ShowsCompletionPercentage** (lines 83-101)
   - **Scenario**: Entry with 75% completion
   - **Expected**: Output contains `"(75%)"`

4. **FormatJournalListAsync_CompletedEntry_ShowsStarIcon** (lines 103-122)
   - **Scenario**: Entry with 100% completion
   - **Expected**: Green star icon `"[green]★[/]"`

5. **FormatJournalListAsync_IncompleteEntry_ShowsCircleIcon** (lines 124-142)
   - **Scenario**: Entry with 50% completion
   - **Expected**: Grey circle icon `"[grey]●[/]"`

6. **FormatJournalListAsync_ShowsUsageHint** (lines 144-162)
   - **Scenario**: Any journal with entries
   - **Expected**: Output contains `"codex <name>"` usage hint

**Code References**:
- `FormatJournalListAsync`: [JournalService.cs:141-182](JournalService.cs:141-182)

---

### Test Category 2: FormatEntryDetailAsync Tests (6 tests, lines 166-286)

**Purpose**: Validate entry detail formatting with redaction, thresholds, and error handling.

**Tests**:

1. **FormatEntryDetailAsync_EntryNotFound_ReturnsError** (lines 168-183)
   - **Scenario**: Query non-existent entry "Unknown Entry"
   - **Expected**: Red error message `"[red]No entry found matching 'Unknown Entry'.[/]"`

2. **FormatEntryDetailAsync_IncompleteEntry_RedactsText** (lines 185-208)
   - **Scenario**: Entry with 30% completion
   - **Expected**: Output contains `"[grey]"` (redaction markup), completion shows `"30%"`

3. **FormatEntryDetailAsync_CompleteEntry_ShowsFullText** (lines 210-233)
   - **Scenario**: Entry with 100% completion
   - **Expected**: Full text visible (no redaction), completion shows `"100%"`

4. **FormatEntryDetailAsync_ShowsUnlockedThresholds** (lines 235-259)
   - **Scenario**: Entry with 2 unlocked thresholds: `["WEAKNESS_REVEALED", "HABITAT_REVEALED"]`
   - **Expected**: "Discoveries:" section with formatted tags:
     - `"Weakness Revealed"`
     - `"Habitat Revealed"`
     - Green checkmarks `"[green]✓[/]"`

5. **FormatEntryDetailAsync_ShowsCategoryAndTitle** (lines 261-285)
   - **Scenario**: Entry "Iron-Husk" in Bestiary
   - **Expected**:
     - Title in uppercase: `"IRON-HUSK"`
     - Category displayed: `"Bestiary"`
     - Completion label: `"Completion:"`

**Code References**:
- `FormatEntryDetailAsync`: [JournalService.cs:185-226](JournalService.cs:185-226)
- `FormatTag`: [JournalService.cs:268-279](JournalService.cs:268-279)

---

### Test Category 3: FormatUnassignedCapturesAsync Tests (3 tests, lines 289-366)

**Purpose**: Validate unassigned fragment listing with truncation.

**Tests**:

1. **FormatUnassignedCapturesAsync_NoCaptures_ReturnsEmpty** (lines 291-304)
   - **Scenario**: Character with no unassigned fragments
   - **Expected**: `"No unassigned fragments"` message

2. **FormatUnassignedCapturesAsync_WithCaptures_ShowsDetails** (lines 306-335)
   - **Scenario**: 1 unassigned fragment from "Old Container"
   - **Expected**: Output contains:
     - Header: `"Unassigned Fragments"`
     - Type: `"TextFragment"`
     - Source: `"Old Container"`
     - Content preview: `"A mysterious inscription"`

3. **FormatUnassignedCapturesAsync_TruncatesLongContent** (lines 337-365)
   - **Scenario**: Fragment with 100-character content (exceeds 60-char limit)
   - **Expected**: Output contains `"..."` (truncation ellipsis), does NOT contain full content

**Code References**:
- `FormatUnassignedCapturesAsync`: [JournalService.cs:229-257](JournalService.cs:229-257)
- `Truncate`: [JournalService.cs:287-295](JournalService.cs:287-295)

---

### Test Helpers

**CreateCodexEntry** (lines 371-382):
```csharp
private static CodexEntry CreateCodexEntry(string title, EntryCategory category)
{
    return new CodexEntry
    {
        Id = Guid.NewGuid(),
        Title = title,
        Category = category,
        FullText = $"Full text for {title}",
        TotalFragments = 10,
        UnlockThresholds = new Dictionary<int, string>()
    };
}
```

**Purpose**: Standardized `CodexEntry` creation for test consistency.

---

## Domain 4 Compliance

### Assessment: MIXED (Service Code Compliant, Content Requires Validation)

**JournalService Code**: ✅ Compliant
- Service code performs only formatting and data transformation
- No precision measurements in logic
- No narrative content generation

**CodexEntry Content**: ⚠️ REQUIRES VALIDATION
- `CodexEntry.FullText` content is **user-generated** (stored in database)
- Content MUST be validated before database insertion
- JournalService displays content as-is (does not validate)

### Validation Responsibility

**Who Validates**: `DataCaptureService` or database seed scripts
**When**: Before inserting `CodexEntry` entities into database

**Example Validation**:
```csharp
// DataCaptureService.cs (hypothetical)
public async Task<bool> ValidateCodexEntryAsync(CodexEntry entry)
{
    // Check for forbidden precision measurements
    var forbiddenPatterns = new[]
    {
        @"\d+(\.\d+)?\s*(meters|km|miles)", // Distances
        @"\d+(\.\d+)?\s*°[CF]",             // Temperatures
        @"\d+(\.\d+)?\s*%",                 // Percentages
        @"\d+(\.\d+)?\s*(Hz|kHz|MHz)",      // Frequencies
        @"\d+(\.\d+)?\s*(seconds|minutes)"  // Precise time
    };

    foreach (var pattern in forbiddenPatterns)
    {
        if (Regex.IsMatch(entry.FullText, pattern))
        {
            _logger.LogWarning("Entry {Title} contains forbidden precision: {Pattern}", entry.Title, pattern);
            return false;
        }
    }

    return true;
}
```

### Compliant Content Examples

❌ **Forbidden** (Precision Measurements):
- "The Iron-Husk stands 2.3 meters tall and weighs 450 kg."
- "Its reactor core operates at 1,200°C with 95% efficiency."
- "Detected at a range of 500 meters using thermal imaging."

✅ **Compliant** (Qualitative Language):
- "The Iron-Husk towers over most scavengers, its rusted frame suggesting immense weight."
- "Its reactor core glows with searing heat, intense enough to warp nearby metal."
- "Rangers report detecting its presence from considerable distance through heat signatures."

### Recommendations

1. **Database Seed Scripts**: Manually review all `CodexEntry.FullText` content before insertion
2. **Content Validation Tool**: Create `Domain4Validator` service to scan text for forbidden patterns
3. **Editor Warnings**: If content editing UI is added, display real-time validation warnings
4. **Audit Logging**: Log all content insertions with validation results

---

## Future Extensions

### Enhancement 1: Search and Filter Functionality

**Current Limitation**: No keyword search or category filtering in journal UI.

**Proposed Enhancement**:
- Add `SearchQuery` parameter to `BuildViewModelAsync`
- Filter entries by title or content keywords
- Highlight matching keywords in redacted text (if visible)

**Implementation**:
```csharp
public async Task<JournalViewModel> BuildViewModelAsync(
    Guid characterId,
    string characterName,
    JournalTab tab,
    int selectedIndex = 0,
    int stressLevel = 0,
    string? searchQuery = null) // NEW
{
    var discovered = await _captureService.GetDiscoveredEntriesAsync(characterId);
    var filtered = discovered
        .Where(e => MapCategoryToTab(e.Entry.Category) == tab);

    // NEW: Apply search filter
    if (!string.IsNullOrWhiteSpace(searchQuery))
    {
        filtered = filtered.Where(e =>
            e.Entry.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
            e.Entry.FullText.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
    }

    // ... rest of method
}
```

**Use Case**: Player searches for "Aesir" → all entries mentioning Aesir technology displayed.

---

### Enhancement 2: Pagination for Large Journals

**Current Limitation**: All discovered entries rendered in single list (performance issue with 100+ entries).

**Proposed Enhancement**:
- Add `PageSize` and `PageNumber` parameters to `BuildViewModelAsync`
- Return `TotalPages` in `JournalViewModel`
- UI displays "Page 1 of 5" navigation

**Implementation**:
```csharp
public async Task<JournalViewModel> BuildViewModelAsync(
    Guid characterId,
    string characterName,
    JournalTab tab,
    int selectedIndex = 0,
    int stressLevel = 0,
    int pageNumber = 1,
    int pageSize = 20) // NEW
{
    var discovered = await _captureService.GetDiscoveredEntriesAsync(characterId);
    var filtered = discovered
        .Where(e => MapCategoryToTab(e.Entry.Category) == tab)
        .OrderBy(e => e.Entry.Title);

    var totalCount = filtered.Count();
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    var paged = filtered
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    // ... rest of method
}
```

---

### Enhancement 3: Image/Media Support in Journal

**Current Limitation**: Journal displays text only (no bestiary illustrations).

**Proposed Enhancement**:
- Add `MediaPath` property to `CodexEntry` entity
- `JournalEntryDetailView` includes `ImagePath` for UI rendering
- Display creature illustrations, diagrams, maps

**Implementation**:
```csharp
public record JournalEntryDetailView(
    Guid EntryId,
    string Title,
    EntryCategory Category,
    int CompletionPercent,
    string RedactedContent,
    List<string> UnlockedThresholds,
    int FragmentsCollected,
    int FragmentsRequired,
    string? ImagePath // NEW
);

// In BuildEntryDetailsAsync:
return new JournalEntryDetailView(
    // ... existing properties
    ImagePath: entry.MediaPath // NEW
);
```

**UI Rendering** (Avalonia):
```xml
<Image Source="{Binding SelectedDetail.ImagePath}" MaxHeight="300" />
```

---

### Enhancement 4: Configurable Completion Color Thresholds

**Current Limitation**: Hardcoded 100% = green, 50-99% = yellow, 0-49% = grey.

**Proposed Enhancement**:
- Externalize thresholds to `appsettings.json`
- Allow per-category customization (e.g., Bestiary uses different colors than FieldGuide)

**Configuration**:
```json
{
  "Journal": {
    "CompletionColors": {
      "Complete": { "Threshold": 100, "Color": "green", "Icon": "★" },
      "HighProgress": { "Threshold": 75, "Color": "yellow", "Icon": "●" },
      "MediumProgress": { "Threshold": 50, "Color": "grey", "Icon": "●" },
      "LowProgress": { "Threshold": 0, "Color": "darkgrey", "Icon": "○" }
    }
  }
}
```

---

### Enhancement 5: Character-Level vs. Sentence-Level Redaction

**Current Limitation**: Word-level redaction only (`"The ████ ████ ancient ████"`).

**Proposed Enhancement**:
- Add configurable redaction granularity:
  - **Word-level** (current): `"The ████ ████ ancient ████"`
  - **Character-level**: `"The rus███ ser████ ancient tech█████"`
  - **Sentence-level**: `"█████████████. Weakness: EMP bursts disable their targeting systems."`

**Implementation**:
```csharp
public enum RedactionGranularity
{
    Word,
    Character,
    Sentence
}

public string RedactText(string fullText, int completionPercent, RedactionGranularity granularity)
{
    return granularity switch
    {
        RedactionGranularity.Word => RedactByWord(fullText, completionPercent),
        RedactionGranularity.Character => RedactByCharacter(fullText, completionPercent),
        RedactionGranularity.Sentence => RedactBySentence(fullText, completionPercent),
        _ => fullText
    };
}
```

---

### Enhancement 6: Export Journal to External File

**Proposed Feature**: Export journal contents to PDF, Markdown, or plaintext for offline reading.

**Implementation**:
```csharp
public async Task<string> ExportJournalAsync(Guid characterId, ExportFormat format)
{
    var entries = await _captureService.GetDiscoveredEntriesAsync(characterId);

    return format switch
    {
        ExportFormat.Markdown => GenerateMarkdown(entries),
        ExportFormat.PDF => GeneratePDF(entries),
        ExportFormat.PlainText => GeneratePlainText(entries),
        _ => throw new NotSupportedException()
    };
}
```

**Use Case**: Player wants to share discovered lore with friends or print bestiary guide.

---

## Error Handling

### Error Pattern 1: Entry Not Found (FormatEntryDetailAsync)

**Scenario**: Player requests entry that doesn't exist in database.

**Handling**:
```csharp
// JournalService.cs:189-194
var entry = await _codexRepository.GetByTitleAsync(entryTitle);
if (entry == null)
{
    _logger.LogDebug("Entry '{EntryTitle}' not found", entryTitle);
    return $"[red]No entry found matching '{entryTitle}'.[/]";
}
```

**Recovery**: Returns user-friendly error message (not exception). Player can try different entry name.

---

### Error Pattern 2: Invalid Selected Index (BuildViewModelAsync)

**Scenario**: UI passes out-of-bounds selectedIndex.

**Handling**:
```csharp
// JournalService.cs:75-79
JournalEntryDetailView? details = null;
if (selectedIndex >= 0 && selectedIndex < filtered.Count)
{
    var selected = filtered[selectedIndex];
    details = await BuildEntryDetailsAsync(characterId, selected.EntryId);
}
```

**Recovery**: `SelectedDetail` is `null` (graceful degradation). UI displays entry list without detail pane.

---

### Error Pattern 3: Missing CodexEntry in BuildEntryDetailsAsync

**Scenario**: Entry ID exists in discovered list but entity deleted from database.

**Handling**:
```csharp
// JournalService.cs:113-118
var entry = await _codexRepository.GetByIdAsync(entryId);
if (entry == null)
{
    _logger.LogWarning("[Journal] Entry {EntryId} not found", entryId);
    return null;
}
```

**Recovery**: Returns `null`, logs warning. Parent method (`BuildViewModelAsync`) sets `SelectedDetail = null`.

---

### Error Pattern 4: Empty or Null FullText

**Scenario**: `CodexEntry.FullText` is empty or null.

**Handling**: `TextRedactor.RedactText` handles gracefully:
```csharp
public string RedactText(string fullText, int completionPercent)
{
    if (string.IsNullOrEmpty(fullText))
    {
        return fullText; // Return as-is (no crash)
    }

    // ... redaction logic
}
```

**Recovery**: Empty content displayed (no crash, no exception).

---

## Changelog

### Version 1.0.1 (v0.3.7c) - 2025-12-25

**Documentation Updates:**
- Added `ID` field to spec header metadata
- Updated `Last Updated` to 2025-12-25
- Corrected implementation line count (298 → 361 lines, includes v0.3.11a enhancements)
- Corrected test file line count (385 → 393 lines)
- Added code traceability remarks to implementation files:
  - `IJournalService.cs` - interface spec reference
  - `JournalService.cs` - service spec reference
  - `JournalViewModel.cs` - all records and enum spec reference

---

### Version 1.0.0 (v0.3.7c) - 2025-12-22

**Implemented Features**:
- ✅ `BuildViewModelAsync` for full-screen journal UI (v0.3.7c)
- ✅ `BuildEntryDetailsAsync` with text redaction and threshold formatting
- ✅ `FormatJournalListAsync` with category grouping and completion icons
- ✅ `FormatEntryDetailAsync` with SCREAMING_SNAKE_CASE tag formatting
- ✅ `FormatUnassignedCapturesAsync` with 60-character truncation
- ✅ `TextRedactor` class for word-level progressive redaction
- ✅ `MapCategoryToTab` for tab-based category filtering
- ✅ Completion color coding (100% = green, 50-99% = yellow, 0-49% = grey)
- ✅ Completion icons (100% = star, <100% = circle)
- ✅ Spectre.Console markup for terminal output

**Test Coverage**:
- ✅ 16 tests across 3 categories (FormatJournalList, FormatEntryDetail, FormatUnassignedCaptures)
- ✅ 385 lines of test code
- ✅ 100% method coverage

**Known Limitations**:
- No search/filter functionality
- No pagination for large journals
- Word-level redaction only (no character/sentence-level)
- No image/media support

**Future Work**:
- Search and filter by keywords
- Pagination for 100+ entries
- Configurable redaction granularity
- Image/media display in journal
- Configurable completion color thresholds
- Export journal to PDF/Markdown

---

**End of SPEC-JOURNAL-001**
