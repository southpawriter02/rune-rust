---
id: SPEC-SYSTEM-DATA-CAPTURES
title: "Data Capture System"
version: 1.0
status: draft
last-updated: 2025-12-15
related-files:
  - path: "docs/08-ui/scavengers-journal-ui.md"
    status: UI Specification
  - path: "docs/01-core/resources/stress.md"
    status: Examination costs
  - path: "docs/02-entities/dialogue-system.md"
    status: Keyword integration
---

# Data Capture System

> *"The world's memory is scattered across a thousand ruins. Every data-slate is a puzzle piece. Every echo-recording is a ghost's confession. Collect them all, and perhaps you'll understand why everything fell apart."*

---

## 1. Overview

The Data Capture system is the core content delivery mechanism for the Scavenger's Journal. Rather than providing complete information instantly, knowledge is fragmented across the world and must be collected, assembled, and sometimes analyzed to reveal complete entries.

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-SYSTEM-DATA-CAPTURES` |
| Category | Knowledge System |
| Priority | High |
| Status | Draft |

### 1.2 Design Goals

| Goal | Rationale |
|------|-----------|
| **Reward Exploration** | Players who thoroughly explore find more fragments |
| **Create Narrative Tension** | Incomplete entries hint at mysteries |
| **Support Multiple Playstyles** | Combat, dialogue, and examination all yield captures |
| **Tie Into Progression** | Legend rewards, keyword unlocks, Saga Feats |
| **Maintain Diegetic Feel** | Information feels discovered, not given |
| **No Inventory Burden** | Knowledge collection never competes with gear |

### 1.3 Storage Model

> [!IMPORTANT]
> **Data Captures do not consume inventory space.** They are stored directly in the Scavenger's Journal, separate from the player's physical inventory.

| Item Type | Storage Location | Inventory Impact |
|-----------|------------------|------------------|
| Data Captures | Scavenger's Journal | **None** |
| Quest Items | Quest Journal | **None** |
| Equipment/Consumables | Inventory | Uses slots |

This design ensures:
- Players never choose between knowledge and loot
- Exploration is always rewarded without penalty
- The journal functions as a separate "knowledge inventory"
- Physical data-slates and objects are "read" and transcribed, not carried

---

## 2. Capture Types

### 2.1 Type Definitions

| Type | Icon | Physical Form | Content Style |
|------|------|---------------|---------------|
| **Text Fragment** | `📄` | Data-slates, inscriptions, journals | Direct prose excerpts |
| **Echo Recording** | `🔊` | Memory echoes, audio logs | Transcribed speech (often corrupted) |
| **Visual Record** | `📸` | Schematics, images, diagrams | Descriptive text of visual content |
| **Specimen** | `🧪` | Biological/material samples | Analytical observations |
| **Oral History** | `💬` | NPC dialogue, specialist insights | Quoted conversation |
| **Runic Trace** | `ᚱ` | Decoded runes, aether residue | Technical/mystical notation |

### 2.2 Type Distribution Guidelines

| Capture Type | Primary Sources | Typical Frequency |
|--------------|-----------------|-------------------|
| Text Fragment | Ruins, settlements, corpses | Very Common |
| Echo Recording | Jötun-Forged sites, Silver Cord infrastructure | Uncommon |
| Visual Record | Engineering bays, archives, labs | Uncommon |
| Specimen | Combat, environmental examination | Common |
| Oral History | Dialogue, completed quests | Common |
| Runic Trace | Rune examination, Seiðkona abilities | Rare |

---

## 3. Acquisition Methods

### 3.1 Discovery (Free)

Passive acquisition through exploration:

| Trigger | Example | Stress Cost |
|---------|---------|-------------|
| Enter room with readable object | Data-slate on table | 0 |
| Examine environment feature | Inscription on wall | 0 |
| Loot corpse/container | Journal in chest | 0 |
| Complete quest stage | Reward data capture | 0 |

### 3.2 Examination (Stress Cost)

Active investigation with skill checks:

| Examination Level | WITS DC | Stress Cost | Capture Quality |
|-------------------|---------|-------------|-----------------|
| Cursory | — | 0 | Basic stub only |
| Basic | 10 | 5 | Standard fragment |
| Detailed | 14 | 10 | Enhanced fragment |
| Expert | 18 | 15 | Complete fragment |

### 3.3 Specialist Analysis

Specialization abilities provide unique captures:

| Specialization | Ability | Stress Cost | Unique Content |
|----------------|---------|-------------|----------------|
| **Jötun-Reader** | Analyze Corrupted Code | 15 | Technical specifications, system data |
| **Bone-Setter** | Anatomical Analysis | 12 | Biological weaknesses, physiology |
| **Skald** | Saga Recollection | 10 | Historical connections, oral traditions |
| **Vard-Warden** | Blight Reading | 15 | Corruption mechanics, paradox data |
| **Seiðkona** | Runic Divination | 12 | Aetheric patterns, magical theory |

### 3.4 Dialogue Acquisition

NPCs can provide Oral History captures:

| Dialogue Condition | Example |
|--------------------|---------|
| Reputation threshold | Friendly disposition unlocks lore |
| Keyword used | "All-Rune" unlocks scholar dialogue |
| Quest completion | Reward dialogue reveals history |
| Gift/trade | Information exchange |

---

## 4. Fragment Properties

### 4.1 Core Properties

| Property | Type | Description |
|----------|------|-------------|
| `CaptureId` | string | Unique identifier |
| `Type` | CaptureType | Text/Echo/Visual/Specimen/Oral/Runic |
| `Title` | string | Display name |
| `Content` | string | The actual fragment text |
| `Source` | string | Where/how it was found |
| `TargetEntryId` | string? | Intended Codex/Bestiary entry |
| `MatchConfidence` | float | 0.0-1.0 auto-assignment confidence |
| `IsAssigned` | bool | Whether assigned to an entry |
| `DiscoveredAt` | DateTime | When player found it |
| `DiscoveredLocation` | string | Room/area identifier |

### 4.2 Fragment Quality

| Quality | Content Depth | Legend Bonus |
|---------|---------------|--------------|
| **Standard** | Core information | +15 |
| **Enhanced** | Additional detail | +20 |
| **Complete** | Full revelation | +25 |
| **Specialist** | Unique insight | +30 |

---

## 5. Entry Assembly

### 5.1 Assembly Rules

| Rule | Description |
|------|-------------|
| **One-to-Many** | Multiple fragments can target same entry |
| **Order Independent** | Fragments can be found in any order |
| **Progressive Reveal** | More fragments = more readable text |
| **Threshold Unlocks** | Certain % triggers keyword/reward unlocks |

### 5.2 Completion Thresholds

| Threshold | Effect |
|-----------|--------|
| 1+ fragments | Entry becomes visible (Stub status) |
| 25% | First keyword unlock (if applicable) |
| 50% | Partial status, core content readable |
| 75% | Most content readable |
| 100% | Complete status, full Legend reward |
| 100% + Specialist | Mastered status, bonus content |

### 5.3 Redaction Display

Incomplete entries show placeholder text:

| Fragments Found | Display Style |
|-----------------|---------------|
| 0% | Entry hidden or `[UNKNOWN]` |
| 1-24% | Heavy redaction `████████████` |
| 25-49% | Moderate redaction with hints |
| 50-74% | Light redaction, core readable |
| 75-99% | Minimal redaction |
| 100% | Full text |

---

## 6. Auto-Assignment

### 6.1 Matching Algorithm

Captures are matched to entries based on:

| Factor | Weight | Description |
|--------|--------|-------------|
| **Explicit Tag** | 100% | Designer-specified target |
| **Keyword Overlap** | High | Shared terms in content |
| **Category Match** | Medium | Same Codex category |
| **Location Proximity** | Low | Found near related content |

### 6.2 Assignment Confidence

| Confidence | Behavior |
|------------|----------|
| 90%+ | Auto-assign immediately |
| 70-89% | Auto-assign with notification |
| 50-69% | Suggest match, await confirmation |
| <50% | Store as unassigned |

### 6.3 Manual Override

Players can always:
- Reassign fragments to different entries
- Create new entries from unassigned fragments
- View all possible matches for any fragment

---

## 7. Rewards

### 7.1 Legend Rewards

| Discovery | Legend |
|-----------|--------|
| New fragment collected | +15 |
| Entry reaches Stub (1+) | +10 |
| Entry reaches Partial (50%) | +25 |
| Entry reaches Complete (100%) | +50 |
| Entry reaches Mastered | +100 |

### 7.2 Keyword Unlocks

Certain entries unlock dialogue keywords:

| Entry Progress | Unlock |
|----------------|--------|
| 25%+ on The Rust Lord | "Rust Lord" keyword |
| 50%+ on The All-Rune Paradox | "All-Rune" keyword |
| 100% on God-Sleeper Cult | "God-Sleeper" keyword |

### 7.3 Saga Feats

Major completion achievements:

| Achievement | Feat | Effect |
|-------------|------|--------|
| Complete all BlightOrigin entries | "Blight Scholar" | 25% Corruption resistance |
| Complete all Mechanical Bestiary | "System Administrator" | +2 vs Mechanical |
| Master all Undying entries | "Death's Chronicler" | +1 damage vs Undying |

---

## 8. Data Schema

### 8.1 DataCapture Table

```sql
CREATE TABLE DataCaptures (
    CaptureId TEXT PRIMARY KEY,
    Type TEXT NOT NULL,  -- TextFragment, EchoRecording, etc.
    Title TEXT NOT NULL,
    Content TEXT NOT NULL,
    Source TEXT,
    Quality TEXT DEFAULT 'Standard',  -- Standard, Enhanced, Complete, Specialist
    TargetEntryId TEXT,  -- Suggested entry match
    TargetEntryType TEXT,  -- Codex, Bestiary, FieldGuide
    MatchConfidence REAL DEFAULT 0.0,
    IsUnique BOOLEAN DEFAULT 0,  -- Can only be found once
    IsRepeatable BOOLEAN DEFAULT 0,  -- Can be found multiple times
    RequiresExamination TEXT,  -- Basic, Detailed, Expert, Specialist
    RequiresSpecialization TEXT,  -- jotun-reader, bone-setter, etc.
    HintText TEXT,  -- Hint shown when fragment is missing
    LocationHint TEXT  -- General area hint
);

-- Player's discovered captures
CREATE TABLE PlayerCaptures (
    PlayerId INTEGER NOT NULL,
    CaptureId TEXT NOT NULL,
    DiscoveredAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    DiscoveredLocation TEXT,
    AssignedEntryId TEXT,
    IsNew BOOLEAN DEFAULT 1,
    PRIMARY KEY (PlayerId, CaptureId),
    FOREIGN KEY (CaptureId) REFERENCES DataCaptures(CaptureId)
);

-- Capture placement in world
CREATE TABLE CaptureLocations (
    LocationId TEXT PRIMARY KEY,
    CaptureId TEXT NOT NULL,
    RoomId TEXT,  -- Specific room placement
    ContainerId TEXT,  -- Container/corpse placement
    InteractableId TEXT,  -- Readable object placement
    RequiresTrigger TEXT,  -- Event/quest trigger
    SpawnChance REAL DEFAULT 1.0,  -- Probability (for procedural)
    FOREIGN KEY (CaptureId) REFERENCES DataCaptures(CaptureId)
);
```

### 8.2 Entry Fragment Requirements

```sql
-- Defines which captures belong to which entries
CREATE TABLE EntryFragments (
    EntryId TEXT NOT NULL,
    EntryType TEXT NOT NULL,  -- Codex, Bestiary
    CaptureId TEXT NOT NULL,
    FragmentOrder INTEGER DEFAULT 0,  -- Display order when assembled
    IsRequired BOOLEAN DEFAULT 1,  -- Required for completion
    IsBonus BOOLEAN DEFAULT 0,  -- Extra content beyond 100%
    PRIMARY KEY (EntryId, CaptureId),
    FOREIGN KEY (CaptureId) REFERENCES DataCaptures(CaptureId)
);
```

---

## 9. Implementation Notes

### 9.1 Events

| Event | Trigger | Payload |
|-------|---------|---------|
| `CaptureDiscovered` | Player finds capture | CaptureId, Source, Quality |
| `CaptureAssigned` | Capture linked to entry | CaptureId, EntryId |
| `EntryProgressUpdated` | Entry completion changes | EntryId, NewProgress |
| `EntryCompleted` | Entry reaches 100% | EntryId, LegendAwarded |
| `KeywordUnlocked` | Threshold reached | Keyword, EntryId |

### 9.2 Service Interface

```csharp
public interface IDataCaptureService
{
    // Discovery
    Task<DataCapture> DiscoverCaptureAsync(string captureId, string location);
    Task<IEnumerable<DataCapture>> GetUnassignedCapturesAsync();

    // Assignment
    Task AssignCaptureAsync(string captureId, string entryId);
    Task<IEnumerable<CaptureMatch>> GetPossibleMatchesAsync(string captureId);

    // Entry Progress
    Task<float> GetEntryProgressAsync(string entryId);
    Task<IEnumerable<string>> GetMissingCapturesAsync(string entryId);

    // Rewards
    Task<int> CalculateLegendRewardAsync(string captureId);
    Task<IEnumerable<string>> CheckKeywordUnlocksAsync(string entryId);
}
```

---

## 10. Related Specifications

| Spec | Relationship |
|------|--------------|
| [scavengers-journal-ui.md](../08-ui/scavengers-journal-ui.md) | UI display |
| [stress.md](../01-core/resources/stress.md) | Examination costs |
| [dialogue-system.md](../02-entities/dialogue-system.md) | Keyword integration |
| [saga-system.md](../01-core/saga-system.md) | Legend rewards, Saga Feats |

---

## 11. Implementation Status

| Component | Status |
|-----------|--------|
| DataCapture schema | ❌ Planned |
| Capture discovery events | ❌ Planned |
| Auto-assignment logic | ❌ Planned |
| Entry assembly system | ❌ Planned |
| Keyword unlock triggers | ❌ Planned |
| IDataCaptureService | ❌ Planned |

---

## 12. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-15 | Initial specification |
