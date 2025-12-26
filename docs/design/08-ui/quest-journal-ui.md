---
id: SPEC-UI-QUEST-JOURNAL
title: "Quest Journal UI — TUI & GUI Specification"
version: 1.0
status: draft
last-updated: 2025-12-14
related-files:
  - path: "docs/08-ui/scavengers-journal-ui.md"
    status: Parent (Contracts tab)
  - path: "docs/08-ui/tui-layout.md"
    status: Reference
  - path: "docs/08-ui/map-ui.md"
    status: Reference
  - path: "docs/99-legacy/docs/specifications/QUEST_JOURNAL_SPECIFICATION.md"
    status: Legacy Reference
---

# Quest Journal UI — TUI & GUI Specification

> *"Every scar tells a story. Every contract tells who paid for it."*

---

## 1. Overview

This specification defines the terminal (TUI) and graphical (GUI) interfaces for the Quest Journal, tracking active quests, objectives, rewards, and completion history.

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-UI-QUEST-JOURNAL` |
| Category | UI System |
| Priority | High |
| Status | Draft |

### 1.2 Quest Terminology

| Term | Meaning |
|------|---------|
| **Contract** | A quest (thematic name) |
| **Objective** | Individual task within a quest |
| **Tracked** | Currently displayed on HUD |
| **Turn In** | Return to quest giver to complete |

### 1.3 Quest Types

| Type | Symbol | Color | Description |
|------|--------|-------|-------------|
| **Main** | `★` | Gold | Major story quests |
| **Side** | `○` | White | Optional side quests |
| **Dynamic** | `◇` | Cyan | Procedurally generated |
| **Repeatable** | `↻` | Green | Can be repeated |

### 1.4 Quest Categories

| Category | Icon | Description |
|----------|------|-------------|
| **Combat** | `⚔` | Defeat enemies |
| **Exploration** | `🧭` | Discover locations |
| **Retrieval** | `📦` | Collect items |
| **Delivery** | `→` | Transport items |
| **Investigation** | `?` | Examine/investigate |
| **Dialogue** | `💬` | Talk to NPCs |

---

## 2. TUI Quest Journal Layout

### 2.1 Full Journal Screen

Accessed via `journal` or `J`:

```
┌─────────────────────────────────────────────────────────────────────┐
│  QUEST JOURNAL                                            [J]ournal │
├─────────────────────────────────────────────────────────────────────┤
│  [1] Active (3)  [2] Available (2)  [3] Completed (5)  [4] Failed   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ★ THE IRON PATH [Main] ◄ TRACKED                                   │
│    "Reach the ancient forge beneath Ironhold."                      │
│    ▸ ☐ Find the descent to the Iron Crypts                          │
│    ▸ ☐ Defeat the Rust Lord (0/1)                                   │
│    ▸ ☐ Return to Kjartan                                            │
│                                                                     │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  ○ LOST TOOLS [Side]                                                │
│    "Recover scattered implements for the smith."                    │
│    ▸ ☐ Collect Dvergr Hammers (2/5)  ████████░░░░░░ 40%             │
│                                                                     │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  ◇ PATROL ROUTE [Dynamic]                                           │
│    "Clear the eastern corridor of hostiles."                        │
│    ▸ ☐ Eliminate Rust-Horrors (3/5)  ████████████░░ 60%             │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│  [T] Track  [D] Details  [A] Abandon  [↑↓] Select  [C] Close        │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Tab System

| Tab | Key | Shows |
|-----|-----|-------|
| **Active** | `1` | Accepted, in-progress quests |
| **Available** | `2` | Quests you can accept |
| **Completed** | `3` | Finished and turned-in quests |
| **Failed** | `4` | Failed or abandoned quests |

### 2.3 Quest List Entry Format

```
★ THE IRON PATH [Main] ◄ TRACKED
  "Brief description of the quest."
  ▸ ☐ Current objective (progress if applicable)
```

| Element | Description |
|---------|-------------|
| `★` | Quest type symbol |
| `[Main]` | Quest type tag |
| `◄ TRACKED` | Tracking indicator |
| `"..."` | Quest description excerpt |
| `▸ ☐` | Next incomplete objective |

---

## 3. Quest Detail View

### 3.1 Detailed Quest Panel

Accessed via `D` on selected quest:

```
╔═══════════════════════════════════════════════════════════════════╗
║  ★ THE IRON PATH                                     [Main Quest] ║
╟───────────────────────────────────────────────────────────────────╢
║                                                                   ║
║  "The rumours speak of an ancient Dvergr forge, hidden beneath   ║
║  Ironhold. Kjartan believes it may hold the key to breaking the  ║
║  Rust Curse that plagues the eastern valleys."                   ║
║                                                                   ║
║  Given By: Kjartan the Smith (Ironhold)                          ║
║  Status: Active                                                   ║
║  Accepted: 2 days ago                                            ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  OBJECTIVES                                                       ║
║  ───────────                                                      ║
║  ☑ Speak with Kjartan about the forge legends                    ║
║  ☐ Find the descent to the Iron Crypts                           ║
║  ☐ Defeat the Rust Lord                                          ║
║  ☐ Return to Kjartan with proof                                  ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  REWARDS                                                          ║
║  ───────                                                          ║
║  • +500 Legend                                                    ║
║  • +200 Dvergr Cogs                                               ║
║  • Rune-Etched Blade [Rare]                                       ║
║  • +50 Iron-Banes Reputation                                      ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  [T] Track  [M] Show on Map  [A] Abandon  [C] Close               ║
╚═══════════════════════════════════════════════════════════════════╝
```

### 3.2 Objective Display

| Symbol | Meaning |
|--------|---------|
| `☑` | Completed objective |
| `☐` | Incomplete objective |
| `(optional)` | Optional objective |
| `(3/5)` | Progress counter |
| `████░░` | Progress bar |

### 3.3 Reward Display

| Type | Format | Example |
|------|--------|---------|
| **Legend** | `+X Legend` | `+500 Legend` |
| **Currency** | `+X [Currency]` | `+200 Dvergr Cogs` |
| **Item** | `[Name] [Rarity]` | `Rune-Etched Blade [Rare]` |
| **Reputation** | `+X [Faction]` | `+50 Iron-Banes Reputation` |
| **Unlock** | `Unlocks: [Thing]` | `Unlocks: The Deep Forge` |

---

## 4. Quest Tracking (HUD)

### 4.1 Tracked Quest Widget

Appears in exploration mode:

```
┌─────────────────────────────────────────────────────────────────────┐
│  HP: 45/60 ████████░░  Stamina: 80/100 ████████░░                   │
├───────────────────────────────────────────────┬─────────────────────┤
│                                               │ ┌─────────────────┐ │
│  [Room Description]                           │ │ ★ THE IRON PATH │ │
│                                               │ │ ─────────────── │ │
│                                               │ │ Find the        │ │
│                                               │ │ descent to the  │ │
│                                               │ │ Iron Crypts     │ │
│                                               │ └─────────────────┘ │
│                                               │ ╔═══════════════╗   │
│                                               │ ║   [Minimap]   ║   │
│                                               │ ╚═══════════════╝   │
├───────────────────────────────────────────────┴─────────────────────┤
```

### 4.2 Tracking Widget Content

| Element | Description |
|---------|-------------|
| Quest Type + Name | `★ THE IRON PATH` |
| Current Objective | First incomplete objective |
| Progress (if applicable) | `(3/5)` or progress bar |

### 4.3 Tracking Commands

| Command | Description |
|---------|-------------|
| `track [quest]` | Set as tracked quest |
| `untrack` | Remove tracking |
| `next objective` | Cycle to next objective |

---

## 5. Available Quest Display

### 5.1 Quest Giver Interaction

When speaking to an NPC with available quests:

```
┌─────────────────────────────────────────────────────────────────────┐
│  KJARTAN THE SMITH                                   [Friendly]     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  "Ah, another scavenger. Perhaps you can help me with something."   │
│                                                                     │
│  AVAILABLE CONTRACTS:                                               │
│  ─────────────────────                                              │
│  [1] ★ The Iron Path [Main]                                         │
│      "Legends speak of an ancient forge..."                         │
│      Rewards: +500 Legend, +200 Cogs, Equipment                     │
│                                                                     │
│  [2] ○ Lost Tools [Side]                                            │
│      "My implements have been scattered..."                         │
│      Rewards: +100 Legend, +50 Cogs                                 │
│                                                                     │
│  ─────────────────────────────────────────────────────────────────  │
│  [1-2] Accept  [D] Details  [0] Leave                               │
├─────────────────────────────────────────────────────────────────────┤
│  > _                                                                │
└─────────────────────────────────────────────────────────────────────┘
```

### 5.2 Quest Acceptance

```
> 1

  You accept the contract: THE IRON PATH
  
  ★ Quest Added: The Iron Path
  
  Objective: Speak with Kjartan about the forge legends
  
  [Quest auto-tracked]
```

---

## 6. Quest Completion Flow

### 6.1 Objective Completion

When an objective is completed:

```
  ☑ OBJECTIVE COMPLETE: Collect Dvergr Hammers (5/5)
  
  Quest Progress: Lost Tools — 1/2 objectives complete
```

### 6.2 Quest Ready for Turn-In

When all objectives complete:

```
  ★ QUEST COMPLETE: The Iron Path
  
  Return to Kjartan to claim your reward.
```

### 6.3 Turn-In Dialogue

```
┌─────────────────────────────────────────────────────────────────────┐
│  KJARTAN THE SMITH                                   [Grateful]     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  "You've done it! The Rust Lord is slain, and the forge can be     │
│  reclaimed. You have my thanks, scavenger."                        │
│                                                                     │
│  ═══════════════════════════════════════════════════════════════   │
│  REWARDS RECEIVED                                                   │
│  ─────────────────                                                  │
│  • +500 Legend                                                      │
│  • +200 Dvergr Cogs                                                 │
│  • Rune-Etched Blade [Rare]                                         │
│  • +50 Iron-Banes Reputation                                        │
│  ═══════════════════════════════════════════════════════════════   │
│                                                                     │
│  [Continue]                                                         │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 7. GUI Quest Journal Panel

### 7.1 Layout

```
┌───────────────────────────────────────────────────────────────────────┐
│  QUEST JOURNAL                                                [X]    │
├───────────────────────────────────────────────────────────────────────┤
│  [Active (3)] [Available (2)] [Completed (5)] [Failed (1)]           │
├─────────────────────────────┬─────────────────────────────────────────┤
│  QUEST LIST                 │  QUEST DETAILS                         │
│  ─────────────────────────  │  ───────────────────────────────────── │
│  Filter: [All Types ▼]      │                                        │
│  Sort:   [Type ▼]           │  ★ THE IRON PATH                       │
│  ─────────────────────────  │  [Main Quest]                          │
│                             │                                        │
│  ★ The Iron Path ◄          │  Description...                        │
│    ☐ Find the descent...    │                                        │
│                             │  OBJECTIVES:                           │
│  ○ Lost Tools               │  ☑ Speak with Kjartan                  │
│    ☐ Collect Hammers (2/5)  │  ☐ Find the descent                    │
│                             │  ☐ Defeat the Rust Lord                │
│  ◇ Patrol Route             │  ☐ Return to Kjartan                   │
│    ☐ Eliminate (3/5)        │                                        │
│                             │  REWARDS:                              │
│                             │  • +500 Legend                         │
│                             │  • +200 Dvergr Cogs                    │
│                             │  • Rune-Etched Blade [Rare]            │
│                             │                                        │
│                             │  [Track] [Show on Map] [Abandon]       │
├─────────────────────────────┴─────────────────────────────────────────┤
│  Tracked: ★ The Iron Path — Find the descent to the Iron Crypts      │
└───────────────────────────────────────────────────────────────────────┘
```

### 7.2 GUI Components

| Component | Description |
|-----------|-------------|
| **Tab Bar** | Active/Available/Completed/Failed |
| **Quest List** | Scrollable, filterable |
| **Filter/Sort** | Dropdowns for filtering |
| **Detail Panel** | Full quest information |
| **Action Buttons** | Track, Map, Abandon |
| **Status Bar** | Currently tracked quest |

### 7.3 Interactive Elements

| Element | Click | Hover | Right-Click |
|---------|-------|-------|-------------|
| Quest in list | Select → show details | Preview | Context menu |
| Objective | — | Show location hint | — |
| Reward item | Show item details | Tooltip | — |
| Track button | Set as tracked | — | — |

---

## 8. QuestJournalViewModel

### 8.1 Interface

```csharp
public interface IQuestJournalViewModel
{
    // Tabs
    QuestTab SelectedTab { get; set; }
    
    // Collections
    IReadOnlyList<QuestViewModel> ActiveQuests { get; }
    IReadOnlyList<QuestViewModel> AvailableQuests { get; }
    IReadOnlyList<QuestViewModel> CompletedQuests { get; }
    IReadOnlyList<QuestViewModel> FailedQuests { get; }
    IReadOnlyList<QuestViewModel> FilteredQuests { get; }
    
    // Selection
    QuestViewModel? SelectedQuest { get; set; }
    QuestViewModel? TrackedQuest { get; set; }
    
    // Filters
    QuestType? FilterType { get; set; }
    QuestCategory? FilterCategory { get; set; }
    QuestSortOption SortBy { get; set; }
    
    // Commands
    ICommand TrackCommand { get; }
    ICommand UntrackCommand { get; }
    ICommand AbandonCommand { get; }
    ICommand ShowOnMapCommand { get; }
    ICommand AcceptCommand { get; }
    ICommand TurnInCommand { get; }
    ICommand CloseCommand { get; }
}

public record QuestViewModel(
    Guid Id,
    string Title,
    string Description,
    QuestType Type,
    QuestCategory Category,
    QuestStatus Status,
    string GiverName,
    IReadOnlyList<ObjectiveViewModel> Objectives,
    IReadOnlyList<RewardViewModel> Rewards,
    bool IsTracked,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? CompletedAt
);

public record ObjectiveViewModel(
    string Description,
    ObjectiveType Type,
    bool IsComplete,
    bool IsOptional,
    int CurrentProgress,
    int RequiredProgress
);

public enum QuestTab { Active, Available, Completed, Failed }
public enum QuestType { Main, Side, Dynamic, Repeatable }
public enum QuestCategory { Combat, Exploration, Retrieval, Delivery, Investigation, Dialogue }
public enum QuestStatus { NotStarted, Available, Active, Complete, TurnedIn, Failed, Abandoned }
```

---

## 9. Configuration

| Setting | Default | Options |
|---------|---------|---------|
| `AutoTrackNewQuests` | true | true/false |
| `ShowTrackedOnHUD` | true | true/false |
| `ShowObjectiveProgress` | true | true/false |
| `NotifyOnObjectiveComplete` | true | true/false |
| `DefaultSortOrder` | Type | Type/Title/Progress/AcceptedDate |

---

## 10. Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `J` | Open/close journal |
| `1-4` | Switch tabs |
| `↑↓` | Navigate quest list |
| `Enter` | View quest details |
| `T` | Track selected quest |
| `M` | Show on map |
| `A` | Abandon quest |
| `Esc` | Close journal/detail view |

---

## 11. Implementation Status

| Component | TUI Status | GUI Status |
|-----------|------------|------------|
| Journal screen layout | ❌ Planned | ❌ Planned |
| Tab navigation | ❌ Planned | ❌ Planned |
| Quest list display | ❌ Planned | ❌ Planned |
| Quest detail view | ❌ Planned | ❌ Planned |
| Objective tracking | ❌ Planned | ❌ Planned |
| HUD tracking widget | ❌ Planned | ❌ Planned |
| Quest acceptance | ❌ Planned | ❌ Planned |
| Quest turn-in | ❌ Planned | ❌ Planned |
| Map integration | ❌ Planned | ❌ Planned |
| Filter/sort | ❌ Planned | ❌ Planned |
| QuestJournalViewModel | ❌ Planned | ❌ Planned |

---

## 12. Phased Implementation Guide

### Phase 1: Core Journal
- [ ] Journal screen layout
- [ ] Tab navigation (Active/Available/Completed/Failed)
- [ ] Quest list display
- [ ] Quest selection

### Phase 2: Quest Details
- [ ] Detail panel
- [ ] Objective list
- [ ] Reward display
- [ ] Accept/Abandon commands

### Phase 3: Tracking
- [ ] Track/untrack commands
- [ ] HUD tracking widget
- [ ] Objective completion notifications

### Phase 4: Integration
- [ ] Quest giver dialogue integration
- [ ] Turn-in flow
- [ ] Map waypoint integration

### Phase 5: GUI
- [ ] QuestJournalViewModel
- [ ] Filter/sort controls
- [ ] Interactive elements

---

## 13. Testing Requirements

### 13.1 TUI Tests
- [ ] Tab switching works correctly
- [ ] Quest list updates on quest changes
- [ ] Tracking persists between sessions
- [ ] Objective progress displays correctly

### 13.2 GUI Tests
- [ ] Filter/sort functions correctly
- [ ] Detail panel updates on selection
- [ ] Map integration shows waypoints

### 13.3 Integration Tests
- [ ] Quest acceptance updates journal
- [ ] Objective completion triggers notification
- [ ] Quest turn-in grants rewards

---

## 14. Relationship to Scavenger's Journal

> [!NOTE]
> The Quest Journal is the **Contracts** tab within the larger [Scavenger's Journal](scavengers-journal-ui.md) system.

### 14.1 Integration

This specification defines the **Contracts** section of the Scavenger's Journal. See [scavengers-journal-ui.md](scavengers-journal-ui.md) for:

| Section | Description |
|---------|-------------|
| **Codex** | Lore entries assembled from Data Captures |
| **Bestiary** | Creature knowledge and combat data |
| **Field Guide** | In-world mechanics glossary |
| **Contracts** | Quest tracking (this spec) |
| **Contacts** | NPCs met, faction relationships |
| **Data Captures** | Fragment inventory and assignment |

### 14.2 Navigation

From the main Scavenger's Journal, pressing `4` or selecting the **Contracts** tab displays the Quest Journal interface defined in this specification.

---

## 15. Related Specifications

| Spec | Relationship |
|------|--------------|
| [map-ui.md](map-ui.md) | Quest waypoints on map |
| [dialogue-ui.md](dialogue-ui.md) | Quest giver dialogue |
| [tui-layout.md](tui-layout.md) | Screen composition |

---

## 16. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification |
