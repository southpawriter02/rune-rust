# SPEC-CAPTURE-001: Data Capture Generation System

**Version:** 1.0.1
**Status:** Implemented
**Last Updated:** 2025-12-25
**Author:** The Architect
**Implementation:** `RuneAndRust.Engine/Services/DataCaptureService.cs` (418 lines)
**Tests:** `RuneAndRust.Tests/Engine/DataCaptureServiceTests.cs` (651 lines, 25 tests)
**Templates:** `RuneAndRust.Engine/Services/CaptureTemplates.cs` (static template library)

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

The **Data Capture Generation System** (`DataCaptureService`) is the probabilistic fragment generation engine for *Rune & Rust*'s discovery mechanics. It generates lore fragments (`DataCapture` entities) during gameplay actions (searching containers, examining objects) and automatically assigns them to `CodexEntry` records, progressively revealing journal content.

### Purpose

- **Probabilistic Fragment Generation**: Roll-based capture generation with WITS-based probability modifiers
- **Multi-Source Triggering**: Generate captures from searches (containers) and examinations (objects/NPCs)
- **Auto-Assignment**: Keyword matching system to link fragments to corresponding `CodexEntry` records
- **Quality Tiering**: Standard vs. Specialist quality based on tier/expertise
- **Completion Tracking**: Calculate entry completion % and unlocked threshold tags
- **Discovery Queries**: Retrieve all discovered entries for journal display

### Key Features

1. **Two Trigger Sources**:
   - **Search** (`TryGenerateFromSearchAsync`): 25% base chance + (WITS × 5%)
   - **Examination** (`TryGenerateFromExaminationAsync`): 37% (detailed tier) or 75% (expert tier) + (WITS × 3%)

2. **Template-Based Content**:
   - 5 template categories: RustedServitor, BlightedCreature, IndustrialSite, AncientRuin, GenericContainer
   - Each template includes: `CaptureType`, `FragmentContent`, `Source`, `MatchingKeywords[]`
   - All templates are **Domain 4 compliant** (AAM-VOICE Layer 2 diagnostic language)

3. **Quality System**:
   - **Standard Quality (15)**: Search, Detailed tier examination
   - **Specialist Quality (30)**: Expert tier examination (3+ net successes)

4. **Auto-Assignment Logic**:
   - Match `template.MatchingKeywords[]` against `CodexEntry.Title` (case-insensitive)
   - If match found: Set `DataCapture.CodexEntryId` → fragment contributes to completion %
   - If no match: Leave `CodexEntryId = null` → fragment remains unassigned

5. **Completion Calculation**:
   ```
   CompletionPercent = (FragmentCount / TotalFragments) × 100
   Capped at 100%
   ```

6. **Threshold Unlocking**:
   - `CodexEntry.UnlockThresholds`: `{ 25: "WEAKNESS_REVEALED", 50: "HABITAT_REVEALED", 100: "FULL_ENTRY" }`
   - Filter thresholds where `Key <= CompletionPercent`
   - Return unlocked tags for journal display

### System Context

**DataCaptureService** sits between **gameplay actions** (InteractionService, CombatService) and **journal display** (JournalService). It **generates** fragments during play and **provides** completion data for journal rendering.

**Dependencies**:
- `IDataCaptureRepository` - Persist `DataCapture` entities, query fragment counts
- `ICodexEntryRepository` - Load `CodexEntry` entities for auto-assignment and completion calculation
- `Random` - Probabilistic roll generation (seedable for testing)

**Dependents**:
- `InteractionService` - Calls `TryGenerateFromSearchAsync` on container searches
- `ExaminationService` - Calls `TryGenerateFromExaminationAsync` on object examination
- `JournalService` - Calls `GetDiscoveredEntriesAsync`, `GetCompletionPercentageAsync`, `GetUnlockedThresholdsAsync`

---

## Core Behaviors

### 1. Search-Based Capture Generation (`TryGenerateFromSearchAsync`)

**Signature**: `Task<CaptureResult> TryGenerateFromSearchAsync(Guid characterId, InteractableObject container, int witsBonus = 0)`

**Purpose**: Generate fragment when player searches a container.

**Behavior**:

```csharp
// DataCaptureService.cs:93-148
public async Task<CaptureResult> TryGenerateFromSearchAsync(
    Guid characterId,
    InteractableObject container,
    int witsBonus = 0)
{
    _logger.LogDebug("Attempting capture generation for Character {CharacterId} from search of {ContainerName}",
        characterId, container.Name);

    // 1. Calculate effective chance: base 25% + (wits bonus × 5%)
    var effectiveChance = BaseSearchCaptureChance + (witsBonus * 5);
    var roll = _random.Next(100); // 0-99

    _logger.LogDebug("Capture roll: {Roll} vs target {Target} (WITS bonus: {WitsBonus})",
        roll, effectiveChance, witsBonus);

    // 2. Check success
    if (roll >= effectiveChance)
    {
        _logger.LogDebug("Capture roll failed, no capture generated");
        return CaptureResult.NoCapture("No lore fragments discovered.");
    }

    // 3. Select a template based on the container
    var template = SelectTemplate(container);
    if (template == null)
    {
        _logger.LogDebug("No suitable capture template found for container {ContainerName}", container.Name);
        return CaptureResult.NoCapture("No lore fragments discovered.");
    }

    // 4. Create the capture
    var capture = new DataCapture
    {
        CharacterId = characterId,
        Type = template.Type,
        FragmentContent = template.FragmentContent,
        Source = $"{template.Source} ({container.Name})",
        Quality = StandardQuality, // 15
        IsAnalyzed = false
    };

    // 5. Try to auto-assign to a matching Codex entry
    var wasAutoAssigned = await TryAutoAssignAsync(capture, template.MatchingKeywords);

    // 6. Persist the capture
    await _captureRepository.AddAsync(capture);
    await _captureRepository.SaveChangesAsync();

    _logger.LogInformation("Generated {CaptureType} capture for Character {CharacterId}",
        capture.Type, characterId);

    var message = wasAutoAssigned
        ? $"You discovered a {capture.Type} fragment and added it to your Codex."
        : $"You discovered a {capture.Type} fragment. It may relate to something you haven't encountered yet.";

    return CaptureResult.Generated(message, capture, wasAutoAssigned);
}
```

**Probability Formula**:
```
EffectiveChance = BaseSearchCaptureChance + (witsBonus × 5)
               = 25 + (witsBonus × 5)

Examples:
- WITS 0: 25% chance (roll < 25 succeeds)
- WITS 4: 45% chance (25 + 20)
- WITS 10: 75% chance (25 + 50)
```

**Edge Cases**:
- **WITS bonus exceeds 100%**: No explicit cap (assumes game design prevents WITS > 15)
- **Template Selection Returns Null**: NoCapture result (no fragment generated)
- **Container Name Empty**: Template still selected by description keywords

**Logging**:
- **Debug**: Method entry with characterId and container name
- **Debug**: Roll result vs. target chance
- **Debug**: Template selection result
- **Information**: Successful capture generation with type

---

### 2. Examination-Based Capture Generation (`TryGenerateFromExaminationAsync`)

**Signature**: `Task<CaptureResult> TryGenerateFromExaminationAsync(Guid characterId, InteractableObject target, int tierRevealed, int witsBonus = 0)`

**Purpose**: Generate fragment when player examines an object/NPC.

**Behavior**:

```csharp
// DataCaptureService.cs:151-221
public async Task<CaptureResult> TryGenerateFromExaminationAsync(
    Guid characterId,
    InteractableObject target,
    int tierRevealed,
    int witsBonus = 0)
{
    _logger.LogDebug("Attempting capture generation for Character {CharacterId} from examination of {TargetName} (tier {Tier})",
        characterId, target.Name, tierRevealed);

    // 1. Base tier (0) never generates captures
    if (tierRevealed < DetailedTier) // DetailedTier = 1
    {
        _logger.LogDebug("Base tier examination, no capture chance");
        return CaptureResult.NoCapture("Basic examination reveals no hidden knowledge.");
    }

    // 2. Calculate effective chance based on tier
    // Expert tier (2+): 75% base
    // Detailed tier (1): 37% base
    var baseChance = tierRevealed >= ExpertTier ? ExpertExamCaptureChance : DetailedExamCaptureChance;
    var effectiveChance = baseChance + (witsBonus * 3); // 3% per WITS
    var roll = _random.Next(100);

    _logger.LogDebug("Capture roll: {Roll} vs target {Target} (WITS bonus: {WitsBonus})",
        roll, effectiveChance, witsBonus);

    // 3. Check success
    if (roll >= effectiveChance)
    {
        _logger.LogDebug("Capture roll failed, no capture generated");
        return CaptureResult.NoCapture("Your examination reveals no additional knowledge.");
    }

    // 4. Select a template based on the target
    var template = SelectTemplate(target);
    if (template == null)
    {
        _logger.LogDebug("No suitable capture template found for target {TargetName}", target.Name);
        return CaptureResult.NoCapture("Your examination reveals no additional knowledge.");
    }

    // 5. Quality is higher for expert tier
    var quality = tierRevealed >= ExpertTier ? SpecialistQuality : StandardQuality;
    _logger.LogTrace("Assigned quality {Quality} based on tier {Tier} (Expert threshold: {ExpertTier})",
        quality, tierRevealed, ExpertTier);

    // 6. Create the capture
    var capture = new DataCapture
    {
        CharacterId = characterId,
        Type = template.Type,
        FragmentContent = template.FragmentContent,
        Source = $"{template.Source} ({target.Name})",
        Quality = quality,
        IsAnalyzed = false
    };

    // 7. Try to auto-assign to a matching Codex entry
    var wasAutoAssigned = await TryAutoAssignAsync(capture, template.MatchingKeywords);

    // 8. Persist the capture
    await _captureRepository.AddAsync(capture);
    await _captureRepository.SaveChangesAsync();

    _logger.LogInformation("Generated {CaptureType} capture for Character {CharacterId}",
        capture.Type, characterId);

    var tierName = tierRevealed >= ExpertTier ? "expert" : "detailed";
    var message = wasAutoAssigned
        ? $"Your {tierName} examination reveals a {capture.Type} fragment, added to your Codex."
        : $"Your {tierName} examination reveals a {capture.Type} fragment.";

    return CaptureResult.Generated(message, capture, wasAutoAssigned);
}
```

**Tier System**:
```
Tier 0 (Base): No capture chance (early return)
Tier 1 (Detailed): 37% base + (WITS × 3%), Quality = 15 (Standard)
Tier 2+ (Expert): 75% base + (WITS × 3%), Quality = 30 (Specialist)
```

**Probability Formulas**:
```
Detailed Tier (1):
  EffectiveChance = 37 + (witsBonus × 3)
  Examples: WITS 0 = 37%, WITS 4 = 49%, WITS 10 = 67%

Expert Tier (2+):
  EffectiveChance = 75 + (witsBonus × 3)
  Examples: WITS 0 = 75%, WITS 4 = 87%, WITS 8 = 99%
```

**Edge Cases**:
- **tierRevealed = 0**: NoCapture result (no fragment chance)
- **tierRevealed < 0**: Treated as 0 (no capture)
- **tierRevealed > 2**: Still uses Expert tier probability (75% + WITS)

**Logging**:
- **Debug**: Method entry with characterId, target name, and tier
- **Debug**: Base tier early return
- **Trace**: Quality assignment rationale
- **Information**: Successful capture generation

---

### 3. Template Selection (`SelectTemplate`)

**Signature**: `CaptureTemplate? SelectTemplate(InteractableObject obj)`

**Purpose**: Choose appropriate capture template based on object keywords.

**Behavior**:

```csharp
// DataCaptureService.cs:314-357
private CaptureTemplate? SelectTemplate(InteractableObject obj)
{
    _logger.LogTrace("Selecting capture template for object {ObjectName}", obj.Name);

    // 1. Combine name and description for keyword matching
    var objectName = obj.Name.ToLowerInvariant();
    var objectDesc = obj.Description.ToLowerInvariant();
    var combined = $"{objectName} {objectDesc}";

    // 2. Check for specific object types (ordered by priority)
    if (ContainsAny(combined, "servitor", "automaton", "machine", "mechanical"))
    {
        _logger.LogTrace("Selected RustedServitor template category for {ObjectName}", obj.Name);
        return SelectRandomTemplate(CaptureTemplates.RustedServitor);
    }

    if (ContainsAny(combined, "blight", "corrupted", "infected", "mutation"))
    {
        _logger.LogTrace("Selected BlightedCreature template category for {ObjectName}", obj.Name);
        return SelectRandomTemplate(CaptureTemplates.BlightedCreature);
    }

    if (ContainsAny(combined, "industrial", "forge", "foundry", "factory", "mechanism"))
    {
        _logger.LogTrace("Selected IndustrialSite template category for {ObjectName}", obj.Name);
        return SelectRandomTemplate(CaptureTemplates.IndustrialSite);
    }

    if (ContainsAny(combined, "ruin", "ancient", "inscription", "tomb", "temple"))
    {
        _logger.LogTrace("Selected AncientRuin template category for {ObjectName}", obj.Name);
        return SelectRandomTemplate(CaptureTemplates.AncientRuin);
    }

    // 3. For containers, use generic templates
    if (obj.IsContainer)
    {
        _logger.LogTrace("Selected GenericContainer template category for {ObjectName}", obj.Name);
        return SelectRandomTemplate(CaptureTemplates.GenericContainer);
    }

    // 4. No matching template
    _logger.LogTrace("No template category matched for {ObjectName}", obj.Name);
    return null;
}
```

**Keyword Matching** (`ContainsAny`):
```csharp
// DataCaptureService.cs:362-365
private static bool ContainsAny(string text, params string[] keywords)
{
    return keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
}
```

**Template Categories (Priority Order)**:
1. **RustedServitor**: Keywords: "servitor", "automaton", "machine", "mechanical"
2. **BlightedCreature**: Keywords: "blight", "corrupted", "infected", "mutation"
3. **IndustrialSite**: Keywords: "industrial", "forge", "foundry", "factory", "mechanism"
4. **AncientRuin**: Keywords: "ruin", "ancient", "inscription", "tomb", "temple"
5. **GenericContainer**: Fallback for `obj.IsContainer == true`

**Random Template Selection** (`SelectRandomTemplate`):
```csharp
// DataCaptureService.cs:370-384
private CaptureTemplate SelectRandomTemplate(CaptureTemplate[] templates)
{
    _logger.LogTrace("Selecting random template from {TemplateCount} templates", templates.Length);

    if (templates.Length == 0)
    {
        _logger.LogWarning("Template array is empty, returning null");
        return null!;
    }

    var index = _random.Next(templates.Length);
    var selected = templates[index];
    _logger.LogTrace("Selected template at index {Index}: Type {TemplateType}", index, selected.Type);
    return selected;
}
```

**Edge Cases**:
- **Multiple Keyword Matches**: First matching category selected (priority order matters)
- **No Keyword Matches + Not Container**: Returns `null` (no fragment generated)
- **Empty Template Array**: Returns `null` (with warning log)

---

### 4. Auto-Assignment Logic (`TryAutoAssignAsync`)

**Signature**: `Task<bool> TryAutoAssignAsync(DataCapture capture, string[] keywords)`

**Purpose**: Link fragment to matching `CodexEntry` via keyword matching.

**Behavior**:

```csharp
// DataCaptureService.cs:389-415
private async Task<bool> TryAutoAssignAsync(DataCapture capture, string[] keywords)
{
    _logger.LogDebug("Attempting auto-assignment for capture using keywords: {Keywords}",
        string.Join(", ", keywords));

    // 1. Get all codex entries (future: optimize with indexed query)
    var entries = await _codexRepository.GetAllAsync();

    // 2. Try to match keywords against entry titles
    foreach (var entry in entries)
    {
        _logger.LogTrace("Checking CodexEntry {EntryId} ({EntryTitle}) for keyword match",
            entry.Id, entry.Title);

        var titleLower = entry.Title.ToLowerInvariant();

        // 3. Check if any keyword matches the entry title
        if (keywords.Any(k => titleLower.Contains(k.ToLowerInvariant())))
        {
            capture.CodexEntryId = entry.Id;
            _logger.LogDebug("Auto-assigned capture to CodexEntry {EntryTitle}", entry.Title);
            return true;
        }
    }

    _logger.LogDebug("No matching CodexEntry found, capture remains unassigned");
    return false;
}
```

**Matching Logic**:
```
For each CodexEntry:
  entry.Title.ToLowerInvariant().Contains(keyword.ToLowerInvariant())

Examples:
- Keyword: "servitor", Entry Title: "Rusted Servitor" → MATCH
- Keyword: "blight", Entry Title: "Blighted Wolf" → MATCH
- Keyword: "machine", Entry Title: "Ancient Mechanisms" → MATCH
```

**Edge Cases**:
- **Multiple Matching Entries**: First match wins (loop breaks on first success)
- **Partial Keyword Matches**: Allowed (e.g., "ruin" matches "Ancient Ruin")
- **No Matching Entries**: `CodexEntryId` remains `null` (unassigned fragment)

**Performance Consideration**:
- Queries **all** `CodexEntry` records on every auto-assign
- Future optimization: Add keyword index to `CodexEntry` table

---

### 5. Completion Percentage Calculation (`GetCompletionPercentageAsync`)

**Signature**: `Task<int> GetCompletionPercentageAsync(Guid entryId, Guid characterId)`

**Purpose**: Calculate discovery completion % for a `CodexEntry`.

**Behavior**:

```csharp
// DataCaptureService.cs:224-248
public async Task<int> GetCompletionPercentageAsync(Guid entryId, Guid characterId)
{
    _logger.LogDebug("Calculating completion percentage for Entry {EntryId} and Character {CharacterId}",
        entryId, characterId);

    // 1. Load CodexEntry
    var entry = await _codexRepository.GetByIdAsync(entryId);
    if (entry == null)
    {
        _logger.LogWarning("CodexEntry {EntryId} not found", entryId);
        return 0;
    }

    // 2. Get fragment count for this character
    var fragmentCount = await _captureRepository.GetFragmentCountAsync(entryId, characterId);

    // 3. Calculate percentage
    var percentage = entry.TotalFragments > 0
        ? (fragmentCount * 100) / entry.TotalFragments
        : 0;

    // 4. Cap at 100%
    percentage = Math.Min(percentage, 100);

    _logger.LogDebug("Character {CharacterId} has {Percentage}% completion for Entry {EntryId} ({FragmentCount}/{TotalFragments})",
        characterId, percentage, entryId, fragmentCount, entry.TotalFragments);

    return percentage;
}
```

**Formula**:
```
Percentage = (FragmentCount / TotalFragments) × 100
Capped at: Min(Percentage, 100)

Examples:
- 0/4 fragments: 0%
- 1/4 fragments: 25%
- 2/4 fragments: 50%
- 4/4 fragments: 100%
- 5/4 fragments: 125% → capped to 100%
```

**Edge Cases**:
- **Entry Not Found**: Returns `0%` (with warning log)
- **TotalFragments = 0**: Returns `0%` (prevents division by zero)
- **Fragment Count Exceeds Total**: Capped at `100%`

---

### 6. Threshold Unlocking (`GetUnlockedThresholdsAsync`)

**Signature**: `Task<IEnumerable<string>> GetUnlockedThresholdsAsync(Guid entryId, Guid characterId)`

**Purpose**: Get unlocked threshold tags for an entry based on completion %.

**Behavior**:

```csharp
// DataCaptureService.cs:251-278
public async Task<IEnumerable<string>> GetUnlockedThresholdsAsync(Guid entryId, Guid characterId)
{
    _logger.LogDebug("Getting unlocked thresholds for Entry {EntryId} and Character {CharacterId}",
        entryId, characterId);

    // 1. Load CodexEntry
    var entry = await _codexRepository.GetByIdAsync(entryId);
    if (entry == null)
    {
        _logger.LogWarning("CodexEntry {EntryId} not found", entryId);
        return Enumerable.Empty<string>();
    }

    // 2. Get completion percentage
    var percentage = await GetCompletionPercentageAsync(entryId, characterId);

    // 3. Filter thresholds where Key <= percentage
    var unlockedThresholds = entry.UnlockThresholds
        .Where(kv => kv.Key <= percentage)
        .OrderBy(kv => kv.Key)
        .Select(kv => kv.Value)
        .ToList();

    _logger.LogTrace("Filtered thresholds at {Percentage}%: {Thresholds}",
        percentage, string.Join(", ", unlockedThresholds));

    _logger.LogDebug("Character {CharacterId} has {Count} unlocked thresholds for Entry {EntryTitle}",
        characterId, unlockedThresholds.Count, entry.Title);

    return unlockedThresholds;
}
```

**Unlocking Logic**:
```
For each threshold in entry.UnlockThresholds:
  if threshold.Key <= completionPercentage:
    Add threshold.Value to unlocked list

Example UnlockThresholds:
{
  25: "WEAKNESS_REVEALED",
  50: "HABITAT_REVEALED",
  75: "BEHAVIOR_REVEALED",
  100: "FULL_ENTRY"
}

At 50% completion:
  Unlocked: ["WEAKNESS_REVEALED", "HABITAT_REVEALED"]
  Locked: ["BEHAVIOR_REVEALED", "FULL_ENTRY"]
```

**Edge Cases**:
- **Entry Not Found**: Returns empty list (with warning log)
- **No Thresholds Defined**: Returns empty list (no crash)
- **0% Completion**: Returns empty list (no thresholds unlocked)

---

### 7. Discovered Entries Query (`GetDiscoveredEntriesAsync`)

**Signature**: `Task<IEnumerable<(CodexEntry Entry, int CompletionPercent)>> GetDiscoveredEntriesAsync(Guid characterId)`

**Purpose**: Get all entries the character has discovered (at least 1 fragment).

**Behavior**:

```csharp
// DataCaptureService.cs:281-307
public async Task<IEnumerable<(CodexEntry Entry, int CompletionPercent)>> GetDiscoveredEntriesAsync(Guid characterId)
{
    _logger.LogDebug("Fetching discovered entries for Character {CharacterId}", characterId);

    // 1. Get all unique entry IDs with at least 1 fragment
    var entryIds = await _captureRepository.GetDiscoveredEntryIdsAsync(characterId);
    var results = new List<(CodexEntry, int)>();

    // 2. Load each entry and calculate completion %
    foreach (var entryId in entryIds)
    {
        _logger.LogTrace("Processing discovered entry {EntryId}", entryId);

        var entry = await _codexRepository.GetByIdAsync(entryId);
        if (entry != null)
        {
            var pct = await GetCompletionPercentageAsync(entryId, characterId);
            results.Add((entry, pct));
            _logger.LogTrace("Added entry {EntryTitle} with {Pct}% completion", entry.Title, pct);
        }
        else
        {
            _logger.LogWarning("Discovered entry {EntryId} not found in CodexRepository", entryId);
        }
    }

    _logger.LogDebug("Retrieved {Count} discovered entries for Character {CharacterId}", results.Count, characterId);
    return results;
}
```

**Discovery Definition**:
- Entry is "discovered" if character has **at least 1 fragment** assigned to it
- `DataCaptureRepository.GetDiscoveredEntryIdsAsync()` returns distinct `CodexEntryId` values where `CharacterId = characterId AND CodexEntryId IS NOT NULL`

**Edge Cases**:
- **Entry Deleted After Fragment Creation**: Logs warning, skips entry (doesn't crash)
- **No Discovered Entries**: Returns empty list
- **Duplicate Entry IDs**: Repository should return distinct IDs (no duplicates)

**Performance Consideration**:
- Makes **N+1 queries** (1 for entry IDs, then 1 per entry for completion %)
- Future optimization: Aggregate query with completion calculation

---

## Restrictions

### Hard Constraints (MUST NOT Violate)

1. **MUST use probabilistic generation**:
   - No guaranteed fragment generation (always roll-based)
   - Exception: Testing with seeded `Random` for deterministic tests

2. **MUST persist all fragments to database**:
   - Every generated `DataCapture` must call `AddAsync()` + `SaveChangesAsync()`
   - No in-memory-only fragments

3. **MUST apply Domain 4 constraints to template content**:
   - All `CaptureTemplate.FragmentContent` must be AAM-VOICE Layer 2 compliant
   - No precision measurements, modern terminology, or code-like language

4. **MUST enforce tier-based quality assignment**:
   - Search: Always `StandardQuality` (15)
   - Detailed tier examination: `StandardQuality` (15)
   - Expert tier examination: `SpecialistQuality` (30)

5. **MUST cap completion percentage at 100%**:
   - Even if fragment count exceeds `TotalFragments`
   - Formula: `Math.Min(percentage, 100)`

6. **MUST handle missing `CodexEntry` gracefully**:
   - `GetCompletionPercentageAsync`: Return `0%` (not exception)
   - `GetUnlockedThresholdsAsync`: Return empty list (not exception)

### Soft Constraints (SHOULD Follow)

1. **SHOULD use keyword matching for auto-assignment**:
   - Current implementation: Case-insensitive `Contains()` matching
   - Future: Could add fuzzy matching or weighted scoring

2. **SHOULD log all capture generation attempts**:
   - Debug-level for rolls and template selection
   - Information-level for successful generation

3. **SHOULD provide user-friendly messages**:
   - NoCapture: "No lore fragments discovered."
   - Generated (auto-assigned): "You discovered a {type} fragment and added it to your Codex."
   - Generated (unassigned): "You discovered a {type} fragment. It may relate to something you haven't encountered yet."

---

## Limitations

### Design Limitations

1. **Fixed Template Categories**:
   - Only 5 template categories (RustedServitor, BlightedCreature, IndustrialSite, AncientRuin, GenericContainer)
   - Cannot add new categories without code changes

2. **Simple Keyword Matching**:
   - Uses `string.Contains()` (not fuzzy matching or NLP)
   - Keywords hardcoded in `CaptureTemplate` arrays
   - No weighted keyword scoring

3. **No Duplicate Prevention**:
   - Same fragment can be generated multiple times
   - No tracking of "already generated" fragments per character

4. **Single-Pass Auto-Assignment**:
   - Auto-assignment happens immediately upon generation
   - Cannot retroactively assign unassigned fragments (future enhancement)

5. **No Fragment Rarity System**:
   - All templates within a category have equal probability
   - No "rare" vs. "common" fragments

6. **Completion Capped at 100%**:
   - Excess fragments do not provide additional benefit
   - No "over-completion" rewards

### Performance Limitations

1. **GetAllAsync() in Auto-Assignment**:
   - Loads **all** `CodexEntry` records on every auto-assign
   - Performance degrades with large entry databases (100+ entries)

2. **N+1 Query Pattern in GetDiscoveredEntriesAsync**:
   - One query per discovered entry for completion %
   - Could be optimized with single aggregate query

3. **No Caching**:
   - Completion percentages recalculated on every request
   - No in-memory cache for frequently accessed entries

---

## Use Cases

### UC-CAPTURE-01: Search Container with Low WITS (25% Chance)

**Scenario**: Player with WITS 0 searches a generic crate.

**Pre-Conditions**:
- Character: `{ Id = Guid, WITS = 0 }`
- Container: `{ Name = "Rusted Crate", IsContainer = true }`

**Execution**:
```csharp
var result = await _dataCapture.TryGenerateFromSearchAsync(characterId, container, witsBonus: 0);

// Roll calculation:
// EffectiveChance = 25 + (0 × 5) = 25%
// Roll = 0-99 (random)
// Success if roll < 25
```

**Possible Outcomes**:

**Outcome A: Roll 18 (Success)**:
```csharp
result.Success = true
result.Capture = {
  Type = CaptureType.TextFragment,
  FragmentContent = "A crumpled note with faded writing...",
  Source = "Found in container (Rusted Crate)",
  Quality = 15,
  CodexEntryId = null // No matching entry
}
result.WasAutoAssigned = false
result.Message = "You discovered a TextFragment fragment. It may relate to something you haven't encountered yet."
```

**Outcome B: Roll 68 (Failure)**:
```csharp
result.Success = false
result.Capture = null
result.WasAutoAssigned = false
result.Message = "No lore fragments discovered."
```

**Code References**:
- `TryGenerateFromSearchAsync`: [DataCaptureService.cs:93-148](DataCaptureService.cs:93-148)
- Probability calculation: [DataCaptureService.cs:102](DataCaptureService.cs:102)

---

### UC-CAPTURE-02: Search Servitor Container with High WITS (75% Chance)

**Scenario**: Player with WITS 10 searches a servitor corpse.

**Pre-Conditions**:
- Character: `{ Id = Guid, WITS = 10 }`
- Container: `{ Name = "Rusted Servitor Remains", Description = "A corroded automaton, partially disassembled." }`
- CodexEntry exists: `{ Title = "Rusted Servitor", TotalFragments = 4 }`

**Execution**:
```csharp
var result = await _dataCapture.TryGenerateFromSearchAsync(characterId, container, witsBonus: 10);

// Roll calculation:
// EffectiveChance = 25 + (10 × 5) = 75%
// Roll = 0-99 (random)
// Success if roll < 75

// Template selection:
// combined = "rusted servitor remains a corroded automaton, partially disassembled."
// Matches "servitor" keyword → RustedServitor template category
// Random selection from 4 RustedServitor templates
```

**Expected Outcome (Roll 42, Success)**:
```csharp
result.Success = true
result.Capture = {
  Type = CaptureType.Specimen,
  FragmentContent = "The servo-motor shows signs of organic fungal infiltration...",
  Source = "Servitor examination (Rusted Servitor Remains)",
  Quality = 15,
  CodexEntryId = <Guid for "Rusted Servitor" entry>
}
result.WasAutoAssigned = true
result.Message = "You discovered a Specimen fragment and added it to your Codex."
```

**Post-Conditions**:
- `DataCapture` persisted to database
- "Rusted Servitor" entry completion: 1/4 fragments = 25%
- If first fragment: Entry appears in journal list

**Code References**:
- Template selection: [DataCaptureService.cs:314-357](DataCaptureService.cs:314-357)
- Auto-assignment: [DataCaptureService.cs:389-415](DataCaptureService.cs:389-415)

---

### UC-CAPTURE-03: Detailed Tier Examination (37% Base Chance)

**Scenario**: Player with WITS 4 examines an ancient device (detailed tier = 1 net success).

**Pre-Conditions**:
- Character: `{ Id = Guid, WITS = 4 }`
- Target: `{ Name = "Ancient Device", Description = "A complex mechanism of unknown origin." }`
- Tier revealed: `1` (Detailed)

**Execution**:
```csharp
var result = await _dataCapture.TryGenerateFromExaminationAsync(characterId, target, tierRevealed: 1, witsBonus: 4);

// Roll calculation:
// tierRevealed (1) < ExpertTier (2) → Detailed tier
// EffectiveChance = 37 + (4 × 3) = 49%
// Quality = StandardQuality (15)

// Template selection:
// combined = "ancient device a complex mechanism of unknown origin."
// Matches "ancient" keyword → AncientRuin template category
```

**Expected Outcome (Roll 28, Success)**:
```csharp
result.Success = true
result.Capture = {
  Type = CaptureType.RunicTrace,
  FragmentContent = "Faint runic inscriptions circle the device's core...",
  Source = "Runic analysis (Ancient Device)",
  Quality = 15 // StandardQuality (Detailed tier)
}
result.WasAutoAssigned = false
result.Message = "Your detailed examination reveals a RunicTrace fragment."
```

**Code References**:
- Tier-based probability: [DataCaptureService.cs:168](DataCaptureService.cs:168)
- Quality assignment: [DataCaptureService.cs:190](DataCaptureService.cs:190)

---

### UC-CAPTURE-04: Expert Tier Examination (75% Base Chance, Specialist Quality)

**Scenario**: Player with WITS 0 examines a blighted corpse (expert tier = 3 net successes).

**Pre-Conditions**:
- Character: `{ Id = Guid, WITS = 0 }`
- Target: `{ Name = "Blighted Corpse", Description = "A corrupted body showing signs of mutation." }`
- Tier revealed: `2` (Expert)
- CodexEntry exists: `{ Title = "Blighted Wolf", TotalFragments = 3 }`

**Execution**:
```csharp
var result = await _dataCapture.TryGenerateFromExaminationAsync(characterId, target, tierRevealed: 2, witsBonus: 0);

// Roll calculation:
// tierRevealed (2) >= ExpertTier (2) → Expert tier
// EffectiveChance = 75 + (0 × 3) = 75%
// Quality = SpecialistQuality (30)

// Template selection:
// combined = "blighted corpse a corrupted body showing signs of mutation."
// Matches "blight" keyword → BlightedCreature template category
```

**Expected Outcome (Roll 52, Success)**:
```csharp
result.Success = true
result.Capture = {
  Type = CaptureType.Specimen,
  FragmentContent = "Tissue samples reveal extensive cellular mutation...",
  Source = "Biological analysis (Blighted Corpse)",
  Quality = 30 // SpecialistQuality (Expert tier)
}
result.WasAutoAssigned = true // "blight" matches "Blighted Wolf"
result.Message = "Your expert examination reveals a Specimen fragment, added to your Codex."
```

**Post-Conditions**:
- Fragment worth **30 quality** (double standard value)
- "Blighted Wolf" entry: +1 fragment → completion increases
- JournalService will display partial redaction based on new completion %

**Code References**:
- Expert tier detection: [DataCaptureService.cs:168](DataCaptureService.cs:168)
- SpecialistQuality assignment: [DataCaptureService.cs:190](DataCaptureService.cs:190)

---

### UC-CAPTURE-05: Base Tier Examination (No Capture Chance)

**Scenario**: Player examines a stone wall (base tier = 0 net successes).

**Pre-Conditions**:
- Character: `{ Id = Guid, WITS = 10 }`
- Target: `{ Name = "Stone Wall", Description = "A rough stone wall." }`
- Tier revealed: `0` (Base)

**Execution**:
```csharp
var result = await _dataCapture.TryGenerateFromExaminationAsync(characterId, target, tierRevealed: 0, witsBonus: 10);

// Early return:
// tierRevealed (0) < DetailedTier (1) → NoCapture
```

**Expected Outcome**:
```csharp
result.Success = false
result.Capture = null
result.WasAutoAssigned = false
result.Message = "Basic examination reveals no hidden knowledge."
```

**Post-Conditions**:
- No fragment generated (regardless of WITS bonus)
- Player must achieve detailed tier or higher to have capture chance

**Code References**:
- Base tier check: [DataCaptureService.cs:161-165](DataCaptureService.cs:161-165)

---

### UC-CAPTURE-06: Auto-Assignment to Matching Entry

**Scenario**: Fragment with "industrial" keyword matches "Industrial Foundry" entry.

**Pre-Conditions**:
- Fragment generated with template: `{ MatchingKeywords = ["industrial", "forge", "foundry"] }`
- CodexEntry exists: `{ Id = Guid, Title = "Industrial Foundry", TotalFragments = 5 }`

**Execution**:
```csharp
var wasAutoAssigned = await _dataCapture.TryAutoAssignAsync(capture, new[] { "industrial", "forge", "foundry" });

// Matching logic:
// entry.Title.ToLowerInvariant() = "industrial foundry"
// keyword "industrial".ToLowerInvariant() = "industrial"
// "industrial foundry".Contains("industrial") → MATCH
```

**Expected Outcome**:
```csharp
wasAutoAssigned = true
capture.CodexEntryId = <Guid for "Industrial Foundry">
```

**Post-Conditions**:
- Fragment contributes to "Industrial Foundry" completion %
- Entry appears in journal if first fragment
- Completion calculation: `(fragmentCount / 5) × 100`

**Code References**:
- Auto-assignment logic: [DataCaptureService.cs:389-415](DataCaptureService.cs:389-415)
- Keyword matching: [DataCaptureService.cs:405](DataCaptureService.cs:405)

---

### UC-CAPTURE-07: Unassigned Fragment (No Matching Entry)

**Scenario**: Fragment generated but no matching `CodexEntry` exists yet.

**Pre-Conditions**:
- Fragment generated with template: `{ MatchingKeywords = ["mysterious", "unknown", "artifact"] }`
- No CodexEntry with title containing these keywords

**Execution**:
```csharp
var wasAutoAssigned = await _dataCapture.TryAutoAssignAsync(capture, new[] { "mysterious", "unknown", "artifact" });

// No CodexEntry.Title contains "mysterious", "unknown", or "artifact"
```

**Expected Outcome**:
```csharp
wasAutoAssigned = false
capture.CodexEntryId = null
```

**Post-Conditions**:
- Fragment stored in database with `CodexEntryId = null`
- Appears in JournalService "Unassigned Fragments" list
- Future: When matching entry added to database, could be retroactively assigned

**Code References**:
- Unassigned path: [DataCaptureService.cs:413-414](DataCaptureService.cs:413-414)

---

### UC-CAPTURE-08: Completion Percentage Calculation

**Scenario**: Calculate completion % for entry with 2/4 fragments.

**Pre-Conditions**:
- CodexEntry: `{ Id = Guid, Title = "Iron-Husk", TotalFragments = 4 }`
- Character has 2 fragments assigned to this entry

**Execution**:
```csharp
var pct = await _dataCapture.GetCompletionPercentageAsync(entryId, characterId);

// Calculation:
// fragmentCount = 2 (from repository)
// percentage = (2 / 4) × 100 = 50
// capped = Min(50, 100) = 50
```

**Expected Outcome**:
```csharp
pct = 50
```

**Post-Conditions**:
- JournalService displays entry as 50% complete
- TextRedactor shows half of `FullText` visible, half redacted
- Threshold check: Unlocks tags with key ≤ 50

**Code References**:
- Completion calculation: [DataCaptureService.cs:237-239](DataCaptureService.cs:237-239)
- Cap at 100%: [DataCaptureService.cs:242](DataCaptureService.cs:242)

---

### UC-CAPTURE-09: Threshold Unlocking

**Scenario**: Character reaches 50% completion, unlocking 2 thresholds.

**Pre-Conditions**:
- CodexEntry: `{ UnlockThresholds = { 25: "WEAKNESS_REVEALED", 50: "HABITAT_REVEALED", 75: "BEHAVIOR_REVEALED", 100: "FULL_ENTRY" } }`
- Character completion: 50%

**Execution**:
```csharp
var unlocked = await _dataCapture.GetUnlockedThresholdsAsync(entryId, characterId);

// Filtering:
// 25 <= 50? Yes → Include "WEAKNESS_REVEALED"
// 50 <= 50? Yes → Include "HABITAT_REVEALED"
// 75 <= 50? No → Exclude "BEHAVIOR_REVEALED"
// 100 <= 50? No → Exclude "FULL_ENTRY"
```

**Expected Outcome**:
```csharp
unlocked = ["WEAKNESS_REVEALED", "HABITAT_REVEALED"]
```

**Post-Conditions**:
- JournalService displays:
  - ✓ Weakness Revealed
  - ✓ Habitat Revealed
- Next threshold at 75% (requires 1 more fragment)

**Code References**:
- Threshold filtering: [DataCaptureService.cs:265-269](DataCaptureService.cs:265-269)

---

### UC-CAPTURE-10: Discovered Entries Query

**Scenario**: Character has fragments for 3 different entries.

**Pre-Conditions**:
- Character has fragments assigned to:
  - "Iron-Husk" (2/4 = 50%)
  - "Rusted Servitor" (1/4 = 25%)
  - "Industrial Foundry" (5/5 = 100%)

**Execution**:
```csharp
var discovered = await _dataCapture.GetDiscoveredEntriesAsync(characterId);

// Process:
// 1. GetDiscoveredEntryIdsAsync → [IronHuskId, RustedServitorId, IndustrialFoundryId]
// 2. For each ID:
//    - Load CodexEntry
//    - Calculate completion %
//    - Add to results
```

**Expected Outcome**:
```csharp
discovered = [
  (CodexEntry { Title = "Iron-Husk", ... }, 50),
  (CodexEntry { Title = "Rusted Servitor", ... }, 25),
  (CodexEntry { Title = "Industrial Foundry", ... }, 100)
]
```

**Post-Conditions**:
- JournalService uses this data to build entry list
- Entries sorted by completion % for display

**Code References**:
- Discovered entries query: [DataCaptureService.cs:281-307](DataCaptureService.cs:281-307)

---

## Decision Trees

### Decision Tree 1: Search Capture Generation

```
Input: TryGenerateFromSearchAsync(characterId, container, witsBonus)
│
├─ Calculate effective chance
│  └─ effectiveChance = BaseSearchCaptureChance (25) + (witsBonus × 5)
│
├─ Roll random number (0-99)
│  └─ roll = Random.Next(100)
│
├─ Check success
│  │
│  ├─ roll >= effectiveChance?
│  │  └─ YES → Return NoCapture("No lore fragments discovered.")
│  │
│  └─ NO → Proceed to template selection
│     │
│     ├─ SelectTemplate(container)
│     │  │
│     │  ├─ Template found?
│     │  │  └─ NO → Return NoCapture
│     │  │
│     │  └─ YES → Create DataCapture
│     │     │
│     │     ├─ Set Type, FragmentContent, Source from template
│     │     ├─ Set Quality = StandardQuality (15)
│     │     ├─ Set IsAnalyzed = false
│     │     │
│     │     └─ TryAutoAssignAsync(capture, template.MatchingKeywords)
│     │        │
│     │        ├─ Match found?
│     │        │  ├─ YES → Set CodexEntryId, wasAutoAssigned = true
│     │        │  └─ NO → Leave CodexEntryId = null, wasAutoAssigned = false
│     │        │
│     │        ├─ Persist capture to repository
│     │        │  ├─ AddAsync(capture)
│     │        │  └─ SaveChangesAsync()
│     │        │
│     │        └─ Return CaptureResult.Generated(message, capture, wasAutoAssigned)
```

**Key Nodes**:
- **Roll Success Check**: `roll < effectiveChance` (not `<=`)
- **Template Selection**: Returns `null` if no keywords match
- **Auto-Assignment**: Optional (fragment stored even if unassigned)

---

### Decision Tree 2: Examination Capture Generation

```
Input: TryGenerateFromExaminationAsync(characterId, target, tierRevealed, witsBonus)
│
├─ Check tier
│  │
│  ├─ tierRevealed < DetailedTier (1)?
│  │  └─ YES → Return NoCapture("Basic examination reveals no hidden knowledge.")
│  │
│  └─ NO → Determine base chance
│     │
│     ├─ tierRevealed >= ExpertTier (2)?
│     │  ├─ YES → baseChance = ExpertExamCaptureChance (75)
│     │  │        quality = SpecialistQuality (30)
│     │  └─ NO → baseChance = DetailedExamCaptureChance (37)
│     │           quality = StandardQuality (15)
│     │
│     ├─ Calculate effective chance
│     │  └─ effectiveChance = baseChance + (witsBonus × 3)
│     │
│     ├─ Roll random number (0-99)
│     │  └─ roll = Random.Next(100)
│     │
│     ├─ Check success
│     │  │
│     │  ├─ roll >= effectiveChance?
│     │  │  └─ YES → Return NoCapture("Your examination reveals no additional knowledge.")
│     │  │
│     │  └─ NO → Proceed to template selection
│     │     │
│     │     ├─ SelectTemplate(target)
│     │     │  │
│     │     │  ├─ Template found?
│     │     │  │  └─ NO → Return NoCapture
│     │     │  │
│     │     │  └─ YES → Create DataCapture
│     │     │     │
│     │     │     ├─ Set Type, FragmentContent, Source from template
│     │     │     ├─ Set Quality = quality (from tier determination)
│     │     │     ├─ Set IsAnalyzed = false
│     │     │     │
│     │     │     └─ TryAutoAssignAsync(capture, template.MatchingKeywords)
│     │     │        │
│     │     │        ├─ Persist capture to repository
│     │     │        └─ Return CaptureResult.Generated(message, capture, wasAutoAssigned)
```

**Key Nodes**:
- **Tier Gate**: Base tier (0) never proceeds past first check
- **Quality Determination**: Happens **before** roll (determines reward value upfront)
- **WITS Scaling**: 3% per WITS (lower than search's 5%)

---

### Decision Tree 3: Template Selection

```
Input: SelectTemplate(InteractableObject obj)
│
├─ Combine name and description
│  └─ combined = obj.Name.ToLowerInvariant() + " " + obj.Description.ToLowerInvariant()
│
├─ Check template categories (priority order)
│  │
│  ├─ ContainsAny(combined, "servitor", "automaton", "machine", "mechanical")?
│  │  └─ YES → SelectRandomTemplate(CaptureTemplates.RustedServitor)
│  │
│  ├─ ContainsAny(combined, "blight", "corrupted", "infected", "mutation")?
│  │  └─ YES → SelectRandomTemplate(CaptureTemplates.BlightedCreature)
│  │
│  ├─ ContainsAny(combined, "industrial", "forge", "foundry", "factory", "mechanism")?
│  │  └─ YES → SelectRandomTemplate(CaptureTemplates.IndustrialSite)
│  │
│  ├─ ContainsAny(combined, "ruin", "ancient", "inscription", "tomb", "temple")?
│  │  └─ YES → SelectRandomTemplate(CaptureTemplates.AncientRuin)
│  │
│  ├─ obj.IsContainer == true?
│  │  └─ YES → SelectRandomTemplate(CaptureTemplates.GenericContainer)
│  │
│  └─ NO MATCH → Return null
│
└─ SelectRandomTemplate(templates[])
   │
   ├─ templates.Length == 0?
   │  └─ YES → Return null (with warning log)
   │
   └─ NO → Random.Next(templates.Length) → Return templates[index]
```

**Key Nodes**:
- **Priority Order**: First matching category selected (no fallthrough)
- **Generic Container Fallback**: Only if `IsContainer == true` and no specific keyword match
- **Random Selection**: Uniform distribution within category

---

### Decision Tree 4: Auto-Assignment

```
Input: TryAutoAssignAsync(DataCapture capture, string[] keywords)
│
├─ Load all CodexEntry records
│  └─ entries = CodexRepository.GetAllAsync()
│
├─ Loop through entries
│  │
│  └─ For each entry:
│     │
│     ├─ titleLower = entry.Title.ToLowerInvariant()
│     │
│     ├─ Check keyword match
│     │  └─ keywords.Any(k => titleLower.Contains(k.ToLowerInvariant()))?
│     │     │
│     │     ├─ YES → Auto-assign
│     │     │  ├─ capture.CodexEntryId = entry.Id
│     │     │  └─ Return true (break loop)
│     │     │
│     │     └─ NO → Continue to next entry
│     │
│     └─ Loop complete without match
│        └─ Return false (capture.CodexEntryId remains null)
```

**Key Nodes**:
- **First Match Wins**: Loop breaks on first successful match
- **Case-Insensitive**: Both title and keywords converted to lowercase
- **Partial Matching**: Uses `Contains()` (not exact match)

---

## Sequence Diagrams

### Sequence Diagram 1: Search Capture Generation with Auto-Assignment

```
Actor: InteractionService
Service: DataCaptureService
Service: Random
Repository: CodexEntryRepository
Repository: DataCaptureRepository

InteractionService -> DataCaptureService: TryGenerateFromSearchAsync(characterId, container, witsBonus=4)
DataCaptureService -> DataCaptureService: Calculate effectiveChance = 25 + (4 × 5) = 45%
DataCaptureService -> Random: Next(100)
Random -> DataCaptureService: Return 28
DataCaptureService -> DataCaptureService: 28 < 45? YES (success)
DataCaptureService -> DataCaptureService: SelectTemplate(container)
DataCaptureService -> DataCaptureService: Check keywords: "servitor" in "Rusted Servitor Remains" → MATCH
DataCaptureService -> Random: Next(4) // RustedServitor has 4 templates
Random -> DataCaptureService: Return 1
DataCaptureService -> DataCaptureService: Select template[1]: TextFragment template
DataCaptureService -> DataCaptureService: Create DataCapture { Type=TextFragment, Quality=15, ... }
DataCaptureService -> DataCaptureService: TryAutoAssignAsync(capture, ["servitor", "automaton", "aesir"])
DataCaptureService -> CodexEntryRepository: GetAllAsync()
CodexEntryRepository -> DataCaptureService: Return [entry1 {"Rusted Servitor"}, entry2 {"Iron-Husk"}, ...]
DataCaptureService -> DataCaptureService: "rusted servitor".Contains("servitor")? YES
DataCaptureService -> DataCaptureService: capture.CodexEntryId = entry1.Id
DataCaptureService -> DataCaptureRepository: AddAsync(capture)
DataCaptureRepository -> DataCaptureService: OK
DataCaptureService -> DataCaptureRepository: SaveChangesAsync()
DataCaptureRepository -> DataCaptureService: OK
DataCaptureService -> InteractionService: Return CaptureResult.Generated(wasAutoAssigned=true)
```

**Key Interactions**:
1. Probability calculated before roll
2. Template selection uses two random selections (category → specific template)
3. Auto-assignment queries all entries (optimization opportunity)
4. Persistence happens after auto-assignment

---

### Sequence Diagram 2: Expert Tier Examination with Specialist Quality

```
Actor: ExaminationService
Service: DataCaptureService
Service: Random
Repository: CodexEntryRepository
Repository: DataCaptureRepository

ExaminationService -> DataCaptureService: TryGenerateFromExaminationAsync(characterId, target, tierRevealed=2, witsBonus=0)
DataCaptureService -> DataCaptureService: tierRevealed (2) >= ExpertTier (2)? YES
DataCaptureService -> DataCaptureService: baseChance = ExpertExamCaptureChance (75)
DataCaptureService -> DataCaptureService: quality = SpecialistQuality (30)
DataCaptureService -> DataCaptureService: effectiveChance = 75 + (0 × 3) = 75%
DataCaptureService -> Random: Next(100)
Random -> DataCaptureService: Return 52
DataCaptureService -> DataCaptureService: 52 < 75? YES (success)
DataCaptureService -> DataCaptureService: SelectTemplate(target)
DataCaptureService -> DataCaptureService: Check keywords: "blight" in "Blighted Corpse..." → MATCH
DataCaptureService -> Random: Next(3) // BlightedCreature has 3 templates
Random -> DataCaptureService: Return 0
DataCaptureService -> DataCaptureService: Select template[0]: Specimen template
DataCaptureService -> DataCaptureService: Create DataCapture { Type=Specimen, Quality=30, ... }
DataCaptureService -> DataCaptureService: TryAutoAssignAsync(capture, ["blight", "corrupted", "infected", "creature"])
DataCaptureService -> CodexEntryRepository: GetAllAsync()
CodexEntryRepository -> DataCaptureService: Return [entry1 {"Blighted Wolf"}, ...]
DataCaptureService -> DataCaptureService: "blighted wolf".Contains("blight")? YES
DataCaptureService -> DataCaptureService: capture.CodexEntryId = entry1.Id
DataCaptureService -> DataCaptureRepository: AddAsync(capture)
DataCaptureRepository -> DataCaptureService: OK
DataCaptureService -> DataCaptureRepository: SaveChangesAsync()
DataCaptureRepository -> DataCaptureService: OK
DataCaptureService -> ExaminationService: Return CaptureResult.Generated(wasAutoAssigned=true)
```

**Key Interactions**:
1. Tier check happens first (gate for base tier)
2. Quality determined **before** roll (Expert = 30, Detailed = 15)
3. Specialist quality doubles fragment value (30 vs. 15)

---

### Sequence Diagram 3: Completion Percentage Calculation and Threshold Unlocking

```
Actor: JournalService
Service: DataCaptureService
Repository: CodexEntryRepository
Repository: DataCaptureRepository

JournalService -> DataCaptureService: GetCompletionPercentageAsync(entryId, characterId)
DataCaptureService -> CodexEntryRepository: GetByIdAsync(entryId)
CodexEntryRepository -> DataCaptureService: Return CodexEntry { TotalFragments=4, ... }
DataCaptureService -> DataCaptureRepository: GetFragmentCountAsync(entryId, characterId)
DataCaptureRepository -> DataCaptureService: Return 2
DataCaptureService -> DataCaptureService: percentage = (2 / 4) × 100 = 50
DataCaptureService -> DataCaptureService: Math.Min(50, 100) = 50
DataCaptureService -> JournalService: Return 50

JournalService -> DataCaptureService: GetUnlockedThresholdsAsync(entryId, characterId)
DataCaptureService -> CodexEntryRepository: GetByIdAsync(entryId)
CodexEntryRepository -> DataCaptureService: Return CodexEntry { UnlockThresholds={25:"WEAKNESS", 50:"HABITAT", 75:"BEHAVIOR", 100:"FULL"} }
DataCaptureService -> DataCaptureService: GetCompletionPercentageAsync(entryId, characterId)
DataCaptureService -> DataCaptureService: Return 50 (from previous call)
DataCaptureService -> DataCaptureService: Filter thresholds: 25<=50, 50<=50, 75<=50?, 100<=50?
DataCaptureService -> DataCaptureService: Unlocked = ["WEAKNESS", "HABITAT"]
DataCaptureService -> JournalService: Return ["WEAKNESS_REVEALED", "HABITAT_REVEALED"]
```

**Key Interactions**:
1. Completion % calculated first (used by threshold filtering)
2. Threshold filtering uses `Where(kv => kv.Key <= percentage)`
3. Results ordered by threshold key (ascending)

---

## Workflows

### Workflow 1: Fragment Generation and Assignment (End-to-End)

**Purpose**: Complete flow from gameplay action to journal update.

**Steps**:
1. ✅ **Player Action**: Search container or examine object
2. ✅ **Trigger Call**: `InteractionService` calls `TryGenerateFromSearchAsync` or `TryGenerateFromExaminationAsync`
3. ✅ **Probability Roll**: Calculate effective chance + roll Random.Next(100)
4. ✅ **Success Check**: If roll < effectiveChance, proceed; else return NoCapture
5. ✅ **Template Selection**: Match object keywords to template category → select random template
6. ✅ **Fragment Creation**: Instantiate `DataCapture` with template content, quality based on tier
7. ✅ **Auto-Assignment**: Match template keywords against `CodexEntry.Title` → set `CodexEntryId` if match
8. ✅ **Persistence**: `AddAsync(capture)` + `SaveChangesAsync()`
9. ✅ **Result Return**: `CaptureResult` with success status, capture entity, auto-assignment flag
10. ✅ **Journal Update**: If auto-assigned, completion % increases → JournalService reflects changes

**Example Timeline**:
- **T0**: Player searches "Rusted Crate" (WITS 4)
- **T1**: DataCaptureService rolls 28 vs. 45% chance → Success
- **T2**: Template selected: GenericContainer[1] (TextFragment)
- **T3**: Auto-assignment: No match (remains unassigned)
- **T4**: Fragment persisted to database
- **T5**: Player opens journal → sees unassigned fragment in "Unassigned Fragments" list

---

### Workflow 2: Completion % and Threshold Unlocking

**Purpose**: Calculate entry completion and determine unlocked thresholds.

**Steps**:
1. ✅ **Journal Query**: `JournalService.BuildViewModelAsync()` calls `DataCaptureService.GetDiscoveredEntriesAsync(characterId)`
2. ✅ **Entry IDs Retrieval**: `DataCaptureRepository.GetDiscoveredEntryIdsAsync()` → distinct `CodexEntryId` values
3. ✅ **Per-Entry Processing**:
   - Load `CodexEntry` entity
   - Call `GetCompletionPercentageAsync(entryId, characterId)`
     - Query fragment count: `DataCaptureRepository.GetFragmentCountAsync(entryId, characterId)`
     - Calculate: `(fragmentCount / TotalFragments) × 100`
     - Cap at 100%
   - Add `(CodexEntry, completionPercent)` to results list
4. ✅ **Threshold Unlocking** (per entry):
   - Call `GetUnlockedThresholdsAsync(entryId, characterId)`
   - Filter `entry.UnlockThresholds.Where(kv => kv.Key <= completionPercent)`
   - Return unlocked tag values (e.g., ["WEAKNESS_REVEALED", "HABITAT_REVEALED"])
5. ✅ **ViewModel Building**:
   - `JournalService` constructs `JournalViewModel` with entries and thresholds
   - `TextRedactor` applies word-level redaction based on completion %
6. ✅ **UI Rendering**: Avalonia binds `JournalViewModel` to journal view

---

### Workflow 3: Seeded Random for Deterministic Testing

**Purpose**: Enable repeatable unit tests with known random outcomes.

**Steps**:
1. ✅ **Test Setup**: Create `DataCaptureService` with seeded constructor:
   ```csharp
   var sut = new DataCaptureService(_mockLogger.Object, _mockCaptureRepository.Object, _mockCodexRepository.Object, seed: 42);
   ```
2. ✅ **Known Random Sequence**: Seed 42 produces known sequence (e.g., first roll = 37)
3. ✅ **Test Execution**:
   - Call `TryGenerateFromSearchAsync(characterId, container, witsBonus: 0)`
   - With seed 42 + WITS 0 (25% chance):
     - Roll 37 ≥ 25 → Failure (NoCapture)
   - With seed 42 + WITS 10 (75% chance):
     - Roll 37 < 75 → Success (CaptureResult.Generated)
4. ✅ **Assertion**: Verify expected outcome based on known seed behavior
5. ✅ **Production Use**: Production code uses parameterless constructor → unseeded `Random` for true randomness

**Code References**:
- Seeded constructor: [DataCaptureService.cs:80-90](DataCaptureService.cs:80-90)
- Unseeded constructor: [DataCaptureService.cs:61-70](DataCaptureService.cs:61-70)

---

## Cross-System Integration

### Integration 1: InteractionService (Search Trigger)

**Relationship**: `InteractionService` → `DataCaptureService`

**Integration Points**:
```csharp
// InteractionService.cs (hypothetical)
public async Task<InteractionResult> SearchContainerAsync(Guid characterId, InteractableObject container)
{
    // ... search logic (loot generation, etc.)

    // Trigger fragment generation
    var witsBonus = _statCalculation.GetAttribute(character, Attribute.Wits);
    var captureResult = await _dataCaptureService.TryGenerateFromSearchAsync(characterId, container, witsBonus);

    if (captureResult.Success)
    {
        _inputHandler.DisplayMessage(captureResult.Message);
    }

    return InteractionResult.Success();
}
```

**Data Flow**:
- `InteractionService` provides `InteractableObject` and WITS bonus
- `DataCaptureService` returns `CaptureResult`
- `InteractionService` displays result message to player

---

### Integration 2: ExaminationService (Examination Trigger)

**Relationship**: `ExaminationService` → `DataCaptureService`

**Integration Points**:
```csharp
// ExaminationService.cs (hypothetical)
public async Task<ExaminationResult> ExamineObjectAsync(Guid characterId, InteractableObject target, int netSuccesses)
{
    // Determine tier based on net successes
    var tierRevealed = netSuccesses >= 3 ? 2 : netSuccesses >= 1 ? 1 : 0;

    // Trigger fragment generation
    var witsBonus = _statCalculation.GetAttribute(character, Attribute.Wits);
    var captureResult = await _dataCaptureService.TryGenerateFromExaminationAsync(characterId, target, tierRevealed, witsBonus);

    if (captureResult.Success)
    {
        _inputHandler.DisplayMessage(captureResult.Message);
    }

    return ExaminationResult.Success();
}
```

**Data Flow**:
- `ExaminationService` calculates tier from dice roll net successes
- `DataCaptureService` determines probability and quality based on tier
- Quality affects fragment value (Standard 15 vs. Specialist 30)

---

### Integration 3: JournalService (Completion Data Consumer)

**Relationship**: `JournalService` → `DataCaptureService`

**Integration Points**:
1. **Discovered Entries Query**:
   ```csharp
   var entries = await _dataCaptureService.GetDiscoveredEntriesAsync(characterId);
   ```

2. **Completion Percentage Query**:
   ```csharp
   var pct = await _dataCaptureService.GetCompletionPercentageAsync(entryId, characterId);
   ```

3. **Unlocked Thresholds Query**:
   ```csharp
   var thresholds = await _dataCaptureService.GetUnlockedThresholdsAsync(entryId, characterId);
   ```

**Data Flow**:
- `DataCaptureService` provides completion data
- `JournalService` formats data for UI display
- No circular dependency (one-way data flow)

---

### Integration 4: CodexEntryRepository (Entry Data Provider)

**Relationship**: `DataCaptureService` → `ICodexEntryRepository`

**Integration Points**:
1. **Entry Lookup by ID**:
   ```csharp
   var entry = await _codexRepository.GetByIdAsync(entryId);
   ```

2. **All Entries Query** (for auto-assignment):
   ```csharp
   var entries = await _codexRepository.GetAllAsync();
   ```

**Data Flow**:
- `CodexEntryRepository` stores canonical entry data (`Title`, `TotalFragments`, `UnlockThresholds`)
- `DataCaptureService` reads entry data for completion calculation and auto-assignment
- No writes to `CodexEntry` from `DataCaptureService`

---

### Integration 5: DataCaptureRepository (Fragment Storage)

**Relationship**: `DataCaptureService` → `IDataCaptureRepository`

**Integration Points**:
1. **Fragment Persistence**:
   ```csharp
   await _captureRepository.AddAsync(capture);
   await _captureRepository.SaveChangesAsync();
   ```

2. **Fragment Count Query**:
   ```csharp
   var count = await _captureRepository.GetFragmentCountAsync(entryId, characterId);
   ```

3. **Discovered Entry IDs Query**:
   ```csharp
   var entryIds = await _captureRepository.GetDiscoveredEntryIdsAsync(characterId);
   ```

**Data Flow**:
- `DataCaptureService` creates `DataCapture` entities
- `DataCaptureRepository` persists and queries fragments
- Fragment count drives completion % calculation

---

## Data Models

### Core Entity: DataCapture

**Purpose**: Individual fragment discovered by player.

**Definition**:
```csharp
public class DataCapture
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Guid? CodexEntryId { get; set; } // NULL = unassigned
    public CaptureType Type { get; set; }
    public string FragmentContent { get; set; } // AAM-VOICE compliant text
    public string Source { get; set; } // e.g., "Servitor examination (Rusted Servitor Remains)"
    public int Quality { get; set; } // 15 (Standard) or 30 (Specialist)
    public bool IsAnalyzed { get; set; } // Future feature flag
    public DateTime DiscoveredAt { get; set; }
}
```

**Usage Context**:
- Created by `TryGenerateFromSearchAsync` and `TryGenerateFromExaminationAsync`
- Persisted to database via `DataCaptureRepository`
- Queried for completion calculation and unassigned fragment listing

---

### DTO: CaptureTemplate

**Purpose**: Template for fragment content generation.

**Definition**:
```csharp
public record CaptureTemplate(
    CaptureType Type,
    string FragmentContent,
    string Source,
    string[] MatchingKeywords
);
```

**Example**:
```csharp
new CaptureTemplate(
    CaptureType.Specimen,
    "The servo-motor shows signs of organic fungal infiltration. " +
    "Mycelial threads have woven through the mechanical joints, " +
    "creating an unsettling fusion of rust and growth.",
    "Servitor examination",
    new[] { "servitor", "automaton", "machine", "mechanical" }
)
```

**Usage Context**:
- Stored in `CaptureTemplates` static class (5 template categories)
- Selected by `SelectTemplate()` based on object keywords
- Provides content for `DataCapture` entity creation

---

### Enum: CaptureType

**Purpose**: Categorize fragment types.

**Definition**:
```csharp
public enum CaptureType
{
    TextFragment,
    Specimen,
    VisualRecord,
    RunicTrace,
    OralHistory,
    EchoRecording
}
```

**Usage Context**:
- Stored in `DataCapture.Type`
- Displayed in journal UI (e.g., "You discovered a Specimen fragment")
- Future: Could affect fragment quality/rarity

---

### DTO: CaptureResult

**Purpose**: Result wrapper for capture generation.

**Definition**:
```csharp
public record CaptureResult(
    bool Success,
    string Message,
    DataCapture? Capture,
    bool WasAutoAssigned
)
{
    public static CaptureResult NoCapture(string message) =>
        new CaptureResult(false, message, null, false);

    public static CaptureResult Generated(string message, DataCapture capture, bool wasAutoAssigned) =>
        new CaptureResult(true, message, capture, wasAutoAssigned);
}
```

**Usage Context**:
- Returned by `TryGenerateFromSearchAsync` and `TryGenerateFromExaminationAsync`
- Consumed by `InteractionService` and `ExaminationService`
- Provides user-friendly message for display

---

### Entity: CodexEntry

**Purpose**: Canonical journal entry data.

**Properties** (relevant subset):
```csharp
public class CodexEntry
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public EntryCategory Category { get; set; }
    public string FullText { get; set; }
    public int TotalFragments { get; set; }
    public Dictionary<int, string> UnlockThresholds { get; set; }
}
```

**Example**:
```csharp
new CodexEntry
{
    Title = "Rusted Servitor",
    TotalFragments = 4,
    UnlockThresholds = new Dictionary<int, string>
    {
        { 25, "WEAKNESS_REVEALED" },
        { 50, "HABITAT_REVEALED" },
        { 75, "BEHAVIOR_REVEALED" },
        { 100, "FULL_ENTRY" }
    }
}
```

**Usage Context**:
- Queried for auto-assignment keyword matching
- Used for completion % calculation (`fragmentCount / TotalFragments`)
- Threshold dictionary used by `GetUnlockedThresholdsAsync`

---

## Configuration

### Constants (DataCaptureService.cs:22-53)

**Search Probability**:
```csharp
private const int BaseSearchCaptureChance = 25; // 25% base
```

**Examination Probabilities**:
```csharp
private const int ExpertExamCaptureChance = 75;   // 75% for expert tier (2+)
private const int DetailedExamCaptureChance = 37; // 37% for detailed tier (1)
```

**Quality Values**:
```csharp
private const int StandardQuality = 15;    // Search, Detailed tier
private const int SpecialistQuality = 30;  // Expert tier (double value)
```

**Tier Thresholds**:
```csharp
private const int ExpertTier = 2;   // 3+ net successes
private const int DetailedTier = 1; // 1+ net successes
```

**WITS Scaling** (implicit in code):
```csharp
// Search: WITS × 5%
var effectiveChance = BaseSearchCaptureChance + (witsBonus * 5);

// Examination: WITS × 3%
var effectiveChance = baseChance + (witsBonus * 3);
```

---

## Testing

### Test File: DataCaptureServiceTests.cs

**Location**: `/Users/ryan/Documents/GitHub/rune-rust/RuneAndRust.Tests/Engine/DataCaptureServiceTests.cs`
**Lines**: 651 lines
**Test Count**: 25 tests

---

### Test Category 1: TryGenerateFromSearchAsync Tests (6 tests, lines 44-164)

**Purpose**: Validate search-based capture generation with probability scaling.

**Tests**:

1. **TryGenerateFromSearchAsync_RollSucceeds_ReturnsCapture** (lines 46-64)
   - **Scenario**: Seeded service with high WITS bonus (20) → 125% chance → guaranteed success
   - **Expected**: `Success = true`, `Quality = 15` (StandardQuality)

2. **TryGenerateFromSearchAsync_RollFails_ReturnsNoCapture** (lines 66-87)
   - **Scenario**: Seed 100 produces roll 85 vs. 25% chance → failure
   - **Expected**: `Success = false`, message contains "No lore fragments"

3. **TryGenerateFromSearchAsync_WithHighWits_IncreasesChance** (lines 89-126)
   - **Scenario**: Run 100 iterations with WITS 10 (75%) vs. WITS 0 (25%)
   - **Expected**: High WITS produces significantly more successes

4. **TryGenerateFromSearchAsync_ServitorContainer_ReturnsServitorCapture** (lines 128-145)
   - **Scenario**: Container name/description contains "servitor" keywords
   - **Expected**: Template category = RustedServitor

5. **TryGenerateFromSearchAsync_PersistsCaptureToRepository** (lines 147-163)
   - **Scenario**: Successful capture generation
   - **Expected**: `AddAsync()` and `SaveChangesAsync()` called once each

**Code References**:
- `TryGenerateFromSearchAsync`: [DataCaptureService.cs:93-148](DataCaptureService.cs:93-148)

---

### Test Category 2: TryGenerateFromExaminationAsync Tests (5 tests, lines 167-248)

**Purpose**: Validate examination-based capture generation with tier-based quality.

**Tests**:

1. **TryGenerateFromExaminationAsync_ExpertTier_HighChance** (lines 169-186)
   - **Scenario**: Tier 2 (Expert) with WITS 0 → 75% base chance
   - **Expected**: `Quality = 30` (SpecialistQuality)

2. **TryGenerateFromExaminationAsync_DetailedTier_MediumChance** (lines 188-213)
   - **Scenario**: Tier 1 (Detailed) with WITS 0 → 37% base chance
   - **Expected**: `Quality = 15` (StandardQuality)

3. **TryGenerateFromExaminationAsync_BaseTier_NoCapture** (lines 215-229)
   - **Scenario**: Tier 0 (Base) with WITS 10 → no capture chance
   - **Expected**: `Success = false`, message contains "Basic examination"

4. **TryGenerateFromExaminationAsync_ExpertTier_SetsSpecialistQuality** (lines 231-247)
   - **Scenario**: Tier 2 with WITS 5 → 75% + 15% = 90% chance
   - **Expected**: `Quality = 30`

**Code References**:
- `TryGenerateFromExaminationAsync`: [DataCaptureService.cs:151-221](DataCaptureService.cs:151-221)
- Tier-based quality: [DataCaptureService.cs:190](DataCaptureService.cs:190)

---

### Test Category 3: Auto-Assignment Tests (2 tests, lines 251-302)

**Purpose**: Validate keyword matching for auto-assignment.

**Tests**:

1. **TryGenerateCapture_MatchingEntry_AutoAssigns** (lines 253-278)
   - **Scenario**: Fragment with "servitor" keyword, entry title = "Rusted Servitor"
   - **Expected**: `WasAutoAssigned = true`, `CodexEntryId` set

2. **TryGenerateCapture_NoMatchingEntry_RemainsUnassigned** (lines 280-301)
   - **Scenario**: Fragment keywords don't match any entry titles
   - **Expected**: `WasAutoAssigned = false`, `CodexEntryId = null`

**Code References**:
- `TryAutoAssignAsync`: [DataCaptureService.cs:389-415](DataCaptureService.cs:389-415)

---

### Test Category 4: GetCompletionPercentageAsync Tests (6 tests, lines 305-427)

**Purpose**: Validate completion % calculation and edge cases.

**Tests**:

1. **GetCompletionPercentageAsync_NoFragments_ReturnsZero** (lines 307-331)
   - **Scenario**: 0/4 fragments
   - **Expected**: `0%`

2. **GetCompletionPercentageAsync_AllFragments_ReturnsHundred** (lines 333-357)
   - **Scenario**: 4/4 fragments
   - **Expected**: `100%`

3. **GetCompletionPercentageAsync_HalfFragments_ReturnsFifty** (lines 359-383)
   - **Scenario**: 2/4 fragments
   - **Expected**: `50%`

4. **GetCompletionPercentageAsync_EntryNotFound_ReturnsZero** (lines 385-400)
   - **Scenario**: Entry ID doesn't exist
   - **Expected**: `0%` (graceful degradation)

5. **GetCompletionPercentageAsync_MoreFragmentsThanRequired_CapsAtHundred** (lines 402-426)
   - **Scenario**: 5/2 fragments (overflow)
   - **Expected**: `100%` (capped)

**Code References**:
- `GetCompletionPercentageAsync`: [DataCaptureService.cs:224-248](DataCaptureService.cs:224-248)
- Cap at 100%: [DataCaptureService.cs:242](DataCaptureService.cs:242)

---

### Test Category 5: GetUnlockedThresholdsAsync Tests (6 tests, lines 430-585)

**Purpose**: Validate threshold unlocking based on completion %.

**Tests**:

1. **GetUnlockedThresholdsAsync_AtZeroPercent_ReturnsEmpty** (lines 432-462)
   - **Scenario**: 0% completion
   - **Expected**: No thresholds unlocked

2. **GetUnlockedThresholdsAsync_AtTwentyFivePercent_ReturnsFirstThreshold** (lines 464-495)
   - **Scenario**: 25% completion (1/4 fragments)
   - **Expected**: `["WEAKNESS_REVEALED"]`

3. **GetUnlockedThresholdsAsync_AtFiftyPercent_ReturnsTwoThresholds** (lines 497-529)
   - **Scenario**: 50% completion (2/4 fragments)
   - **Expected**: `["WEAKNESS_REVEALED", "HABITAT_REVEALED"]`

4. **GetUnlockedThresholdsAsync_AtHundredPercent_ReturnsAllThresholds** (lines 531-567)
   - **Scenario**: 100% completion (4/4 fragments)
   - **Expected**: `["WEAKNESS_REVEALED", "HABITAT_REVEALED", "BEHAVIOR_REVEALED", "FULL_ENTRY"]` (ordered)

5. **GetUnlockedThresholdsAsync_EntryNotFound_ReturnsEmpty** (lines 569-584)
   - **Scenario**: Entry ID doesn't exist
   - **Expected**: Empty list (graceful degradation)

**Code References**:
- `GetUnlockedThresholdsAsync`: [DataCaptureService.cs:251-278](DataCaptureService.cs:251-278)
- Threshold filtering: [DataCaptureService.cs:265-269](DataCaptureService.cs:265-269)

---

### Test Helpers

**CreateContainer** (lines 627-637):
```csharp
private static InteractableObject CreateContainer(string name, string description = "A container")
{
    return new InteractableObject
    {
        Id = Guid.NewGuid(),
        Name = name,
        Description = description,
        IsContainer = true,
        IsOpen = true
    };
}
```

**CreateInteractableObject** (lines 639-648):
```csharp
private static InteractableObject CreateInteractableObject(string name, string description = "An object")
{
    return new InteractableObject
    {
        Id = Guid.NewGuid(),
        Name = name,
        Description = description,
        IsContainer = false
    };
}
```

---

## Domain 4 Compliance

### Assessment: FULLY COMPLIANT (Template Content)

**DataCaptureService Code**: ✅ Compliant
- Service code performs probabilistic generation and data management
- No narrative content generated at runtime
- All content sourced from pre-validated `CaptureTemplates`

**CaptureTemplates Content**: ✅ VALIDATED (AAM-VOICE Layer 2)

**Template Compliance Review** (Sample):

✅ **RustedServitor Template 1**:
```
"The servo-motor shows signs of organic fungal infiltration.
Mycelial threads have woven through the mechanical joints,
creating an unsettling fusion of rust and growth."
```
- **Compliant**: Qualitative description, no precision measurements
- **AAM-VOICE**: Diagnostic observation tone ("shows signs of", "unsettling fusion")

✅ **BlightedCreature Template 1**:
```
"Tissue samples reveal extensive cellular mutation.
The corruption spreads in fractal patterns, each branch
terminating in crystalline nodules that pulse with faint light."
```
- **Compliant**: "Extensive" (not "43% increase"), "faint light" (not "0.3 lumens")
- **AAM-VOICE**: Clinical diagnostic language ("samples reveal", "patterns")

✅ **GenericContainer Template 1**:
```
"A crumpled note with faded writing. Most is illegible, but
a fragment remains: '...the eastern tunnels are blocked.
Whatever happened there, the Clans have sealed it permanently.'"
```
- **Compliant**: "Most is illegible" (not "67% unreadable"), "eastern tunnels" (directional, not "2.3 km east")
- **AAM-VOICE**: Found document style, epistemic uncertainty

### Validation Process

**Who Validates**: Template content manually reviewed during creation
**When**: Before adding templates to `CaptureTemplates.cs`

**Validation Checklist**:
- ❌ No metric measurements (meters, kilometers, degrees, percentages)
- ❌ No precise time measurements (seconds, minutes, exact dates)
- ❌ No modern/technical jargon (API, database, software)
- ✅ Qualitative language ("extensive", "faint", "considerable")
- ✅ AAM-VOICE diagnostic tone ("appears to", "suggests", "reveals")
- ✅ Pre-Glitch terminology (Aesir, servitor, Blight, Aetheric)

### Forbidden vs. Compliant Examples

❌ **Forbidden Precision**:
- "The servitor's reactor core operates at 1,200°C with 95% efficiency."
- "Measurements indicate the Blight spreads at 4.3 meters per day."
- "DNA analysis shows 87% genetic corruption in cellular structure."

✅ **Compliant Qualitative**:
- "The servitor's reactor core glows with searing heat, intense enough to warp nearby metal."
- "The Blight spreads swiftly, consuming organic matter in its path."
- "Cellular analysis reveals extensive genetic corruption, far beyond natural mutation."

---

## Future Extensions

### Enhancement 1: Retroactive Auto-Assignment

**Current Limitation**: Fragments assigned only at generation time (no retroactive assignment).

**Proposed Enhancement**:
- Add `ReassignUnassignedFragmentsAsync(Guid characterId)` method
- Run when new `CodexEntry` added to database
- Match unassigned fragments (`CodexEntryId = null`) against new entry

**Implementation**:
```csharp
public async Task<int> ReassignUnassignedFragmentsAsync(Guid characterId)
{
    var unassigned = await _captureRepository.GetUnassignedAsync(characterId);
    var entries = await _codexRepository.GetAllAsync();
    var reassignCount = 0;

    foreach (var capture in unassigned)
    {
        // Extract keywords from FragmentContent (simple approach: significant nouns)
        var keywords = ExtractKeywords(capture.FragmentContent);

        foreach (var entry in entries)
        {
            if (keywords.Any(k => entry.Title.ToLowerInvariant().Contains(k.ToLowerInvariant())))
            {
                capture.CodexEntryId = entry.Id;
                await _captureRepository.UpdateAsync(capture);
                reassignCount++;
                break;
            }
        }
    }

    await _captureRepository.SaveChangesAsync();
    return reassignCount;
}
```

**Use Case**: Player collects "mysterious artifact" fragments early game. Later, "Aesir Artifact" entry added to codex → retroactive assignment links fragments.

---

### Enhancement 2: Fragment Rarity System

**Current Limitation**: All templates within a category have equal probability.

**Proposed Enhancement**:
- Add `Rarity` property to `CaptureTemplate` (Common, Uncommon, Rare, Epic)
- Weighted random selection based on rarity
- Rare fragments worth more quality points

**Implementation**:
```csharp
public record CaptureTemplate(
    CaptureType Type,
    string FragmentContent,
    string Source,
    string[] MatchingKeywords,
    FragmentRarity Rarity // NEW
);

private CaptureTemplate SelectRandomTemplate(CaptureTemplate[] templates)
{
    var weights = templates.Select(t => GetRarityWeight(t.Rarity)).ToArray();
    var totalWeight = weights.Sum();
    var roll = _random.NextDouble() * totalWeight;

    var cumulative = 0.0;
    for (int i = 0; i < templates.Length; i++)
    {
        cumulative += weights[i];
        if (roll <= cumulative)
        {
            return templates[i];
        }
    }

    return templates[^1]; // Fallback to last template
}

private double GetRarityWeight(FragmentRarity rarity) => rarity switch
{
    FragmentRarity.Common => 60.0,
    FragmentRarity.Uncommon => 25.0,
    FragmentRarity.Rare => 12.0,
    FragmentRarity.Epic => 3.0,
    _ => 1.0
};
```

---

### Enhancement 3: Duplicate Fragment Prevention

**Current Limitation**: Same fragment can be generated multiple times.

**Proposed Enhancement**:
- Track generated fragment template IDs per character
- Exclude already-generated templates from selection pool

**Implementation**:
```csharp
private CaptureTemplate? SelectTemplate(InteractableObject obj, Guid characterId)
{
    var category = DetermineCategory(obj); // RustedServitor, etc.
    if (category == null) return null;

    // Get already-generated template IDs for this character
    var generatedIds = await _captureRepository.GetGeneratedTemplateIdsAsync(characterId, category);

    // Filter out already-generated templates
    var availableTemplates = category
        .Where(t => !generatedIds.Contains(t.TemplateId))
        .ToArray();

    if (availableTemplates.Length == 0)
    {
        // All templates exhausted, allow repeats or return null
        return null;
    }

    return SelectRandomTemplate(availableTemplates);
}
```

**Use Case**: Prevent players from seeing "The servo-motor shows fungal infiltration..." repeatedly. Each fragment unique until all templates exhausted.

---

### Enhancement 4: Fuzzy Keyword Matching

**Current Limitation**: Auto-assignment uses simple `Contains()` matching.

**Proposed Enhancement**:
- Add fuzzy matching algorithm (Levenshtein distance)
- Score matches by relevance
- Suggest manual assignment if confidence low

**Implementation**:
```csharp
private async Task<bool> TryAutoAssignAsync(DataCapture capture, string[] keywords)
{
    var entries = await _codexRepository.GetAllAsync();
    var matches = new List<(CodexEntry Entry, double Score)>();

    foreach (var entry in entries)
    {
        var score = CalculateMatchScore(entry.Title, keywords);
        if (score > 0.5) // Confidence threshold
        {
            matches.Add((entry, score));
        }
    }

    if (!matches.Any())
    {
        return false;
    }

    // Auto-assign if high confidence (>0.8)
    var bestMatch = matches.OrderByDescending(m => m.Score).First();
    if (bestMatch.Score > 0.8)
    {
        capture.CodexEntryId = bestMatch.Entry.Id;
        return true;
    }

    // Suggest manual assignment if moderate confidence
    capture.SuggestedEntryId = bestMatch.Entry.Id;
    capture.MatchConfidence = bestMatch.Score;
    return false;
}
```

---

### Enhancement 5: Quality-Based Completion Weighting

**Current Limitation**: All fragments count equally toward completion (1/4 = 25%).

**Proposed Enhancement**:
- Weight fragments by quality (Standard = 15 points, Specialist = 30 points)
- Entry completion based on quality points, not fragment count

**Implementation**:
```csharp
public async Task<int> GetCompletionPercentageAsync(Guid entryId, Guid characterId)
{
    var entry = await _codexRepository.GetByIdAsync(entryId);
    if (entry == null) return 0;

    // NEW: Sum quality points instead of counting fragments
    var qualityPoints = await _captureRepository.GetQualityPointSumAsync(entryId, characterId);

    // entry.TotalQualityPoints = total points needed (e.g., 100 points)
    var percentage = entry.TotalQualityPoints > 0
        ? (qualityPoints * 100) / entry.TotalQualityPoints
        : 0;

    return Math.Min(percentage, 100);
}
```

**Use Case**:
- Standard fragment (15 points) = 15% completion
- Specialist fragment (30 points) = 30% completion
- Rewards expert-tier examinations with faster progression

---

### Enhancement 6: Combat-Based Capture Generation

**Proposed Feature**: Generate fragments from combat victories.

**Implementation**:
```csharp
public async Task<CaptureResult> TryGenerateFromCombatAsync(
    Guid characterId,
    Enemy defeatedEnemy,
    bool wasHardFight)
{
    // Base 30% chance + 20% if hard fight
    var baseChance = wasHardFight ? 50 : 30;
    var roll = _random.Next(100);

    if (roll >= baseChance)
    {
        return CaptureResult.NoCapture("No notable insights gained from combat.");
    }

    // Select template based on enemy type
    var template = SelectTemplateForEnemy(defeatedEnemy);
    if (template == null) return CaptureResult.NoCapture("...");

    var capture = new DataCapture
    {
        CharacterId = characterId,
        Type = CaptureType.Specimen, // Combat always produces Specimens
        FragmentContent = template.FragmentContent,
        Source = $"Combat analysis ({defeatedEnemy.Name})",
        Quality = wasHardFight ? SpecialistQuality : StandardQuality
    };

    var wasAutoAssigned = await TryAutoAssignAsync(capture, template.MatchingKeywords);
    await _captureRepository.AddAsync(capture);
    await _captureRepository.SaveChangesAsync();

    return CaptureResult.Generated("Combat insights recorded.", capture, wasAutoAssigned);
}
```

---

## Error Handling

### Error Pattern 1: Template Selection Returns Null

**Scenario**: No template category matches object keywords.

**Handling**:
```csharp
// DataCaptureService.cs:115-120
var template = SelectTemplate(container);
if (template == null)
{
    _logger.LogDebug("No suitable capture template found for container {ContainerName}", container.Name);
    return CaptureResult.NoCapture("No lore fragments discovered.");
}
```

**Recovery**: Returns NoCapture result (graceful degradation). Player sees "No lore fragments discovered." message.

---

### Error Pattern 2: Entry Not Found in Completion Calculation

**Scenario**: `CodexEntry` deleted after fragments created.

**Handling**:
```csharp
// DataCaptureService.cs:229-234
var entry = await _codexRepository.GetByIdAsync(entryId);
if (entry == null)
{
    _logger.LogWarning("CodexEntry {EntryId} not found", entryId);
    return 0;
}
```

**Recovery**: Returns `0%` completion (not exception). JournalService handles gracefully.

---

### Error Pattern 3: Division by Zero in Completion Calculation

**Scenario**: `CodexEntry.TotalFragments = 0` (data integrity issue).

**Handling**:
```csharp
// DataCaptureService.cs:237-239
var percentage = entry.TotalFragments > 0
    ? (fragmentCount * 100) / entry.TotalFragments
    : 0;
```

**Recovery**: Returns `0%` (failsafe for malformed data).

---

### Error Pattern 4: Empty Template Array in SelectRandomTemplate

**Scenario**: `CaptureTemplates.RustedServitor` array is empty (code error).

**Handling**:
```csharp
// DataCaptureService.cs:374-378
if (templates.Length == 0)
{
    _logger.LogWarning("Template array is empty, returning null");
    return null!;
}
```

**Recovery**: Returns `null`, triggers NoCapture path. Logs warning for developer attention.

---

## Changelog

### v1.0.1 (2025-12-25)
**Documentation Updates:**
- Updated `Last Updated` to 2025-12-25
- Added code traceability remarks to implementation files:
  - `IDataCaptureService.cs` - interface spec reference
  - `DataCaptureService.cs` - service spec reference

### Version 1.0.0 - 2025-12-22

**Implemented Features**:
- ✅ `TryGenerateFromSearchAsync` with WITS-based probability scaling (25% + WITS × 5%)
- ✅ `TryGenerateFromExaminationAsync` with tier-based probability and quality
- ✅ Template-based content generation (5 categories, 15+ templates)
- ✅ Keyword-based auto-assignment to `CodexEntry` records
- ✅ `GetCompletionPercentageAsync` with 100% cap
- ✅ `GetUnlockedThresholdsAsync` with threshold filtering
- ✅ `GetDiscoveredEntriesAsync` for journal display
- ✅ Seeded `Random` constructor for deterministic testing
- ✅ Domain 4 compliant template content (AAM-VOICE Layer 2)

**Template Categories**:
- ✅ RustedServitor (4 templates)
- ✅ BlightedCreature (3 templates)
- ✅ IndustrialSite (3 templates)
- ✅ AncientRuin (3 templates)
- ✅ GenericContainer (3 templates)

**Test Coverage**:
- ✅ 25 tests across 5 categories
- ✅ 651 lines of test code
- ✅ 100% method coverage

**Known Limitations**:
- No retroactive auto-assignment for unassigned fragments
- All templates within category have equal probability (no rarity)
- Simple `Contains()` keyword matching (no fuzzy matching)
- `GetAllAsync()` in auto-assignment (performance concern with 100+ entries)

**Future Work**:
- Retroactive auto-assignment when new entries added
- Fragment rarity system with weighted selection
- Duplicate fragment prevention
- Fuzzy keyword matching with confidence scoring
- Quality-based completion weighting
- Combat-based capture generation

---

**End of SPEC-CAPTURE-001**
