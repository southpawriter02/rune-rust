---
id: SPEC-CODEX-001
title: Scavenger's Journal (Codex) System
version: 1.0.1
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-INTERACT-001, SPEC-CAPTURE-001]
---

# SPEC-CODEX-001: Scavenger's Journal (Codex) System

> **Version:** 1.0.1
> **Status:** Implemented (v0.1.3a)
> **Services:** `ICodexEntryRepository`, `IDataCaptureRepository`, `IDataCaptureService`
> **Location:** `RuneAndRust.Core/Entities/`

---

## Overview

The Scavenger's Journal (Codex) System is a core progression mechanic that allows players to discover, compile, and unlock lore about the world. Unlike traditional RPG journals that are purely informational, this system treats information as a collectable resource ("Data Captures") that must be pieced together to reveal the full picture ("Codex Entries").

This system incentivizes exploration and incentivizes finding lore fragments in the game world.

---

## Core Concepts

### Data Captures (Fragments)
- **Definition:** Individual pieces of information discovered in the world.
- **Form:** Text snippets, recordings, images, biological samples.
- **Mechanics:** No weight, auto-collected, stored in `DataCaptures` table.
- **Assignment:** Linked to a specific Codex Entry (or unassigned/mystery).

### Codex Entries (Topics)
- **Definition:** The complete subject matter (e.g., "Rusted Servitor", "The Glitch").
- **Mechanics:** Static entities with unlock thresholds.
- **Progression:** Unlock % = (Fragments Found / Total Fragments) * 100.
- **Rewards:** Unlocking entries provides specific gameplay benefits (e.g., damage bonuses vs monsters) via Legend points (future).

### Unlock Thresholds
- **Concept:** Information is revealed progressively.
- **Examples:**
  - 25% Completion: Reveal creature name/image.
  - 50% Completion: Reveal behavior/habitat.
  - 75% Completion: Reveal combat weakness.
  - 100% Completion: Full lore entry unlocked.

---

## Behaviors

### Primary Behaviors

#### 1. Discovery (`AddCapture`)
When a player interacts with a lore object (corpse, terminal, slate):
1. Create a `DataCapture` entity.
2. Link it to the player (`CharacterId`).
3. Link it to a `CodexEntry` (if known).
4. Persist to database.

#### 2. Entry Lookup (`GetCodexEntry`)
Retrieves the Codex Entry details, including calculating the completion percentage based on the number of linked `DataCapture` entities found by the character.

#### 3. Progression Tracking (`GetFragmentCount`)
Counts how many fragments the player has found for a specific entry.
```csharp
completionPercent = (foundFragments / totalFragments) * 100;
```

#### 4. Unassigned Fragments (`GetUnassigned`)
Allows players to find fragments that don't immediately reveal their topic. These "mystery fragments" can be auto-assigned later when the player finds a "key" fragment or gains enough knowledge (future mechanic).

---

## Data Models

### CodexEntry Entity
```csharp
public class CodexEntry
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public EntryCategory Category { get; set; }
    public string FullText { get; set; }
    public int TotalFragments { get; set; } = 1;

    // JSONB storage for progressive reveals
    // Key: % Threshold (25, 50, 100)
    // Value: Tag/Keyword ("WEAKNESS_REVEALED")
    public Dictionary<int, string> UnlockThresholds { get; set; }
}
```

### DataCapture Entity
```csharp
public class DataCapture
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Guid? CodexEntryId { get; set; } // Nullable for mystery fragments
    public CaptureType Type { get; set; } // Text, Audio, Visual, Specimen
    public string FragmentContent { get; set; }
    public string Source { get; set; } // "Found on Servitor corpse"
    public int Quality { get; set; } // 15 (Standard), 30 (Specialist)
    public DateTime DiscoveredAt { get; set; }
}
```

---

## Enums

### EntryCategory
| Category | Description |
|----------|-------------|
| **FieldGuide** | Game mechanics, tutorial info |
| **BlightOrigin** | Lore about the Glitch and corruption |
| **Bestiary** | Creature data, stats, weaknesses |
| **Factions** | Politics, groups, social structures |
| **Technical** | Pre-Glitch tech, artifacts |
| **Geography** | Locations, maps, history |

### CaptureType
| Type | Description |
|------|-------------|
| **TextFragment** | Notes, slates, pages |
| **EchoRecording** | Audio logs |
| **VisualRecord** | Images, schematics |
| **Specimen** | Biological samples |
| **OralHistory** | Dialogue, rumors |
| **RunicTrace** | Magical signatures |

---

## Persistence

### Repositories
- `ICodexEntryRepository`: Handles static entry data.
  - `GetByCategoryAsync`: Fetch entries for UI tabs.
  - `GetByTitleAsync`: Search.
- `IDataCaptureRepository`: Handles player progress.
  - `GetByCharacterIdAsync`: All discoveries for a save.
  - `GetFragmentCountAsync`: Calculate completion.
  - `GetUnassignedAsync`: Mystery fragment management.

### Database Schema
- **CodexEntries**: Static lookup table. `UnlockThresholds` stored as JSONB.
- **DataCaptures**: Transactional table. Linked to `Character` and `CodexEntry`.
- **Relationships**: One-to-Many (`CodexEntry` -> `DataCapture`).
- **Delete Behavior**: `SetNull` (Deleting an entry doesn't delete the player's found fragments, they just become unassigned).

---

## Domain 4 Compliance (Technology Validation)

**All Codex content MUST adhere to Domain 4 restrictions:**
- **No Precision:** "Roughly 20 meters", not "19.5m".
- **Archaic Voice:** "The machine spirit sleeps," not "System in standby".
- **No Scientific Terms:** Avoid "DNA", "Cellular", "Voltage". Use "Blood-Script", "Flesh-Weave", "Lightning".

---

## Future Roadmap
- **v0.1.3b:** Service layer implementation (`CodexService`).
- **v0.1.3c:** World triggers (containers/corpses).
- **v0.1.3d:** UI implementation (Journal screen).
- **v0.1.3e:** Text redaction logic based on % completion.
- **v0.1.4:** Legend Reward system integration.

---

## Changelog

### v1.0.1 (2025-12-25)
**Documentation Updates:**
- Added `last_updated` field to YAML frontmatter
- Added `SPEC-CAPTURE-001` to related specs (companion spec for data capture mechanics)
- Added `IDataCaptureService` to Services list (implements capture generation)
- Added code traceability remarks to entity files:
  - `CodexEntry.cs` - entity spec reference
  - `DataCapture.cs` - entity spec reference

### v1.0.0 (Initial)
- Initial specification documenting Scavenger's Journal (Codex) System
- Defined CodexEntry and DataCapture entities
- Documented EntryCategory and CaptureType enums
- Specified unlock threshold mechanics
- Outlined repository interfaces
