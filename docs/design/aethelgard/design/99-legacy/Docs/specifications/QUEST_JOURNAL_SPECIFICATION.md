# Quest Journal GUI Specification
## Version 1.0 - Comprehensive Quest Journal Interface Documentation

**Document Version:** 1.0
**Last Updated:** November 2024
**Target Framework:** Avalonia UI 11.x with ReactiveUI
**Architecture:** MVVM Pattern with Controllers
**Core Dependencies:** `RuneAndRust.Core.Quests`, `RuneAndRust.Engine.QuestService`

---

## Table of Contents

1. [Overview](#1-overview)
2. [Quest Journal View](#2-quest-journal-view)
3. [Quest List Panel](#3-quest-list-panel)
4. [Quest Detail Panel](#4-quest-detail-panel)
5. [Objectives Display](#5-objectives-display)
6. [Rewards Preview](#6-rewards-preview)
7. [Quest Tracking System](#7-quest-tracking-system)
8. [Map Integration](#8-map-integration)
9. [Filter & Sort Controls](#9-filter--sort-controls)
10. [Keyboard Shortcuts](#10-keyboard-shortcuts)
11. [Services & Controllers](#11-services--controllers)
12. [Implementation Roadmap](#12-implementation-roadmap)

---

## 1. Overview

### 1.1 Purpose

This specification defines the GUI implementation for the Quest Journal system in Rune & Rust. The Quest Journal provides players with a comprehensive interface to track active quests, view objectives, preview rewards, and navigate to quest-related locations.

### 1.2 Core System Summary

The underlying quest system (implemented in `RuneAndRust.Core.Quests` and `RuneAndRust.Engine.QuestService`) provides:

| Feature | Implementation |
|---------|---------------|
| Quest Types | Main, Side, Dynamic, Repeatable |
| Quest Categories | Combat, Exploration, Retrieval, Delivery, Investigation, Dialogue |
| Quest Statuses | NotStarted, Available, Active, Complete, Completed, TurnedIn, Failed, Abandoned |
| Objective Types | KillEnemy, CollectItem, TalkToNPC, ExploreRoom |
| Typed Objectives | KillObjective, CollectObjective, ExploreObjective, InteractObjective |
| Rewards | Experience/Legend, Currency, Items, Reputation, Unlocks |

### 1.3 Design Philosophy

- **Tab-Based Navigation**: Clear separation between Active, Available, Completed, and Failed quests
- **Detail-on-Select**: Selecting a quest reveals full details in a side panel
- **Progress Visibility**: Objective progress displayed with visual indicators (progress bars, checkmarks)
- **Map Integration**: Tracked quests show waypoints on the minimap
- **Accessibility**: Full keyboard navigation, screen reader support, colorblind-friendly status indicators

### 1.4 Visual Design

| Element | Specification |
|---------|--------------|
| Background | Dark panel (#1C1C1C) with lighter quest cards (#2A2A2A) |
| Quest Type Colors | Main: Gold (#FFD700), Side: White (#FFFFFF), Dynamic: Cyan (#00CED1), Repeatable: Green (#4CAF50) |
| Status Indicators | Active: Blue glow, Complete: Green checkmark, Failed: Red X, Available: Yellow exclamation |
| Quality Colors | Standard item colors (Gray → White → Blue → Purple → Gold) |

---

## 2. Quest Journal View

**ViewModel:** `QuestJournalViewModel.cs`
**Controller:** `QuestJournalController`
**View:** `QuestJournalView.axaml`

### 2.1 Layout Structure

```
┌─────────────────────────────────────────────────────────────────┐
│ QUEST JOURNAL                                    [X] Close      │
├─────────────────────────────────────────────────────────────────┤
│ [Active (3)] [Available (2)] [Completed (12)] [Failed (1)]      │
├──────────────────────────┬──────────────────────────────────────┤
│ ┌──────────────────────┐ │ QUEST DETAILS                        │
│ │ Filter: [All ▼]      │ │                                      │
│ │ Sort: [Type ▼]       │ │ [Main Quest] The Iron Path           │
│ ├──────────────────────┤ │ ─────────────────────────────────    │
│ │ ★ [Main] Iron Path   │ │ Description text here describing    │
│ │   ● Defeat the Rust  │ │ the quest narrative and context...   │
│ │     Lord (2/3)       │ │                                      │
│ │                      │ │ OBJECTIVES:                          │
│ │ □ [Side] Lost Tools  │ │ ☑ Find the ancient forge             │
│ │   ● Collect Hammers  │ │ ☐ Defeat the Rust Lord (2/3)        │
│ │     (1/5)            │ │   [████████░░░░] 66%                 │
│ │                      │ │ ☐ Return to Kjartan                  │
│ │ □ [Dynamic] Patrol   │ │                                      │
│ │   ● Clear Sector 4   │ │ REWARDS:                             │
│ │                      │ │ • +500 Legend                        │
│ │                      │ │ • +200 Dvergr Cogs                   │
│ │                      │ │ • Rune-Etched Blade                  │
│ │                      │ │ • +50 Iron-Banes Reputation          │
│ │                      │ │                                      │
│ │                      │ │ [Track Quest] [Show on Map] [Abandon]│
│ └──────────────────────┘ │                                      │
├──────────────────────────┴──────────────────────────────────────┤
│ Active: 3 | Available: 2 | Tracked: ★ The Iron Path             │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 Display Elements

| Element | Type | Binding | Description |
|---------|------|---------|-------------|
| Title | Label | - | "Quest Journal" (static) |
| Tab Bar | TabControl | `SelectedTab` | Quest status tabs |
| Quest List | ListView | `FilteredQuests` | Quests for current tab |
| Detail Panel | Panel | `SelectedQuest` | Full quest information |
| Status Bar | Panel | Various | Summary counts and tracked quest |

### 2.3 Properties

| Property | Type | Description |
|----------|------|-------------|
| `SelectedTab` | QuestTab | Currently selected tab (Active/Available/Completed/Failed) |
| `SelectedQuest` | QuestViewModel? | Currently selected quest for detail view |
| `TrackedQuest` | QuestViewModel? | Quest being tracked on map |
| `HasSelectedQuest` | bool | Whether a quest is selected |
| `HasTrackedQuest` | bool | Whether a quest is being tracked |
| `IsVisible` | bool | Journal visibility state |

### 2.4 Tab Definitions

| Tab | Enum Value | Filter Condition | Badge Count |
|-----|------------|------------------|-------------|
| Active | `QuestTab.Active` | `Status == Active` | `ActiveQuestCount` |
| Available | `QuestTab.Available` | `Status == Available` | `AvailableQuestCount` |
| Completed | `QuestTab.Completed` | `Status == TurnedIn \|\| Status == Completed` | `CompletedQuestCount` |
| Failed | `QuestTab.Failed` | `Status == Failed \|\| Status == Abandoned` | `FailedQuestCount` |

### 2.5 Commands

| Button | Command | Parameter | Behavior | Enabled Condition |
|--------|---------|-----------|----------|-------------------|
| **Close** | `CloseCommand` | - | Hide journal | Always |
| **Track** | `TrackQuestCommand` | QuestViewModel | Set as tracked quest | `CanTrackQuest` |
| **Untrack** | `UntrackQuestCommand` | - | Remove tracking | `HasTrackedQuest` |
| **Show Map** | `ShowOnMapCommand` | QuestViewModel | Center map on objective | Quest has location |
| **Abandon** | `AbandonQuestCommand` | QuestViewModel | Abandon quest (with confirm) | `CanAbandonQuest` |
| **Accept** | `AcceptQuestCommand` | QuestViewModel | Accept available quest | Tab == Available |
| **Turn In** | `TurnInQuestCommand` | QuestViewModel | Turn in complete quest | Quest status == Complete |

---

## 3. Quest List Panel

### 3.1 Quest List Item Display

**Collection:** `FilteredQuests` (ObservableCollection<QuestViewModel>)

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Quest unique identifier |
| `Title` | string | Quest display name |
| `Description` | string | Brief quest description |
| `Type` | QuestType | Main/Side/Dynamic/Repeatable |
| `TypeTag` | string | Formatted type tag (e.g., "[Main Quest]") |
| `TypeColor` | string | Type-specific color hex |
| `Category` | QuestCategory | Combat/Exploration/etc. |
| `CategoryIcon` | string | Category emoji icon |
| `Status` | QuestStatus | Current quest status |
| `StatusIcon` | string | Status indicator icon |
| `StatusColor` | string | Status-specific color |
| `GiverNpcName` | string | Quest giver display name |
| `IsTracked` | bool | Whether this is the tracked quest |
| `IsSelected` | bool | Whether this quest is selected |
| `ObjectivePreview` | string | First incomplete objective text |
| `ProgressText` | string | "2/5 objectives complete" |
| `ProgressPercent` | float | Overall completion (0.0-1.0) |
| `EstimatedDuration` | string | "~15 min" formatted |
| `AcceptedAt` | string | Formatted acceptance date |
| `CompletedAt` | string | Formatted completion date |

### 3.2 Type Tag Display

| Quest Type | Tag Text | Color | Icon |
|------------|----------|-------|------|
| Main | "[Main Quest]" | #FFD700 (Gold) | ★ |
| Side | "[Side Quest]" | #FFFFFF (White) | ○ |
| Dynamic | "[Dynamic]" | #00CED1 (Cyan) | ◇ |
| Repeatable | "[Repeatable]" | #4CAF50 (Green) | ↻ |

### 3.3 Category Icons

| Category | Icon | Description |
|----------|------|-------------|
| Combat | ⚔ | Defeat enemies |
| Exploration | 🧭 | Discover locations |
| Retrieval | 📦 | Collect items |
| Delivery | 📬 | Transport items |
| Investigation | 🔍 | Examine objects |
| Dialogue | 💬 | Talk to NPCs |

### 3.4 List Item Commands

| Command | Trigger | Behavior |
|---------|---------|----------|
| `SelectQuestCommand` | Click | Select quest, show details |
| `DoubleClickQuestCommand` | Double-click | Toggle tracking |
| `RightClickQuestCommand` | Right-click | Show context menu |

### 3.5 Context Menu Items

| Menu Item | Command | Visibility |
|-----------|---------|------------|
| Track Quest | `TrackQuestCommand` | Active quests only |
| Show on Map | `ShowOnMapCommand` | Quests with locations |
| Accept Quest | `AcceptQuestCommand` | Available quests only |
| Turn In | `TurnInQuestCommand` | Complete quests only |
| Abandon Quest | `AbandonQuestCommand` | Active/Available quests |
| View Giver Location | `ViewGiverCommand` | Always |

---

## 4. Quest Detail Panel

### 4.1 Header Section

| Element | Binding | Description |
|---------|---------|-------------|
| Type Tag | `SelectedQuest.TypeTag` | Colored type indicator |
| Title | `SelectedQuest.Title` | Quest name (large font) |
| Tracking Indicator | `SelectedQuest.IsTracked` | Star icon if tracked |
| Status Badge | `SelectedQuest.StatusBadge` | Current status display |

### 4.2 Description Section

| Element | Binding | Description |
|---------|---------|-------------|
| Description | `SelectedQuest.Description` | Full quest narrative |
| Giver Info | `SelectedQuest.GiverDisplay` | "Given by: [NPC Name]" |
| Location Hint | `SelectedQuest.LocationHint` | General area hint |
| Duration | `SelectedQuest.EstimatedDuration` | "Estimated: ~15 min" |

### 4.3 Requirements Section

**Displayed when:** `SelectedQuest.HasRequirements == true`

| Element | Binding | Description |
|---------|---------|-------------|
| Legend Requirement | `SelectedQuest.MinimumLegend` | Required Legend level |
| Prerequisite Quests | `SelectedQuest.Prerequisites` | List of required quests |
| Requirements Met | `SelectedQuest.RequirementsMet` | All requirements satisfied |

### 4.4 Timestamps Section

**Displayed when:** Quest has been accepted

| Element | Binding | Description |
|---------|---------|-------------|
| Accepted | `SelectedQuest.AcceptedAtDisplay` | "Accepted: Nov 27, 2024" |
| Completed | `SelectedQuest.CompletedAtDisplay` | "Completed: Nov 27, 2024" |
| Duration | `SelectedQuest.ElapsedTime` | "Time: 2h 15m" |

---

## 5. Objectives Display

### 5.1 Objectives List

**Collection:** `SelectedQuest.Objectives` (ObservableCollection<ObjectiveViewModel>)

| Property | Type | Description |
|----------|------|-------------|
| `ObjectiveId` | string | Unique objective ID |
| `Description` | string | Objective text |
| `Type` | ObjectiveType | KillEnemy/CollectItem/TalkToNPC/ExploreRoom |
| `TypeIcon` | string | Type-specific icon |
| `IsComplete` | bool | Objective completed |
| `IsOptional` | bool | Optional objective flag |
| `CurrentProgress` | int | Current count |
| `TargetProgress` | int | Required count |
| `ProgressPercent` | float | Completion percent (0.0-1.0) |
| `ProgressText` | string | "3/5" or "Complete" |
| `StatusIcon` | string | ☑ (complete) or ☐ (incomplete) |
| `HasProgressBar` | bool | Show progress bar (Required > 1) |
| `TargetName` | string | Friendly name for target |
| `LocationHint` | string | Where to find target |

### 5.2 Objective Type Icons

| Type | Icon | Example Text |
|------|------|--------------|
| KillEnemy | ⚔ | "Defeat 5 Rust-Horrors" |
| CollectItem | 📦 | "Collect 3 Scrap Metal" |
| TalkToNPC | 💬 | "Speak with Kjartan" |
| ExploreRoom | 🧭 | "Discover the Deep Chamber" |

### 5.3 Progress Bar Display

**Displayed when:** `Objective.HasProgressBar == true`

| Element | Style | Description |
|---------|-------|-------------|
| Background | #3A3A3A | Empty portion |
| Fill | #4CAF50 (green) | Completed portion |
| Text | Centered | "66%" or "3/5" |

### 5.4 Objective States

| State | Visual | Description |
|-------|--------|-------------|
| Incomplete | ☐ + Gray text | Not yet completed |
| In Progress | ☐ + White text + Progress bar | Has some progress |
| Complete | ☑ + Green text + Strikethrough | Fully completed |
| Optional | (optional) suffix | Can skip this objective |

---

## 6. Rewards Preview

### 6.1 Rewards Section

**Collection:** `SelectedQuest.Rewards` (ObservableCollection<RewardViewModel>)

| Property | Type | Description |
|----------|------|-------------|
| `RewardType` | RewardType | Experience/Currency/Item/Reputation/Unlock |
| `TypeIcon` | string | Type-specific icon |
| `DisplayText` | string | Formatted reward text |
| `Amount` | int | Numeric amount (if applicable) |
| `ItemQuality` | QualityTier? | Item quality tier |
| `QualityColor` | string | Quality-specific color |

### 6.2 Reward Types

| Type | Icon | Format Example |
|------|------|----------------|
| Experience | ⭐ | "+500 Legend" |
| Currency | 🪙 | "+200 Dvergr Cogs" |
| Item | 📦 | "Rune-Etched Blade" (with quality color) |
| Reputation | 🏛 | "+50 Iron-Banes Reputation" |
| Ability | ✨ | "Unlocks: Whirlwind Strike" |
| Area | 🗺 | "Unlocks: The Deep Forge" |
| Quest | 📜 | "Unlocks: The Forgemaster's Secret" |

### 6.3 Item Reward Display

| Element | Binding | Description |
|---------|---------|-------------|
| Quantity | `Reward.Quantity` | "3x" prefix if > 1 |
| Quality | `Reward.ItemQuality` | Quality tier name |
| Name | `Reward.ItemName` | Item display name |
| Border | `Reward.QualityColor` | Quality-colored border |

### 6.4 Reputation Display

| Element | Binding | Description |
|---------|---------|-------------|
| Change | `Reward.Amount` | +/- value |
| Faction | `Reward.FactionName` | Faction display name |
| Icon | `Reward.FactionIcon` | Faction emblem |
| Change Color | Green (+) / Red (-) | Positive/negative indicator |

---

## 7. Quest Tracking System

### 7.1 Tracking State

| Property | Type | Description |
|----------|------|-------------|
| `TrackedQuest` | QuestViewModel? | Currently tracked quest |
| `TrackedObjective` | ObjectiveViewModel? | Current objective to display |
| `HasTrackedQuest` | bool | Whether tracking is active |
| `TrackingDisplay` | string | HUD tracking text |

### 7.2 HUD Tracking Widget

**Location:** Top-right of exploration/combat view

| Element | Binding | Description |
|---------|---------|-------------|
| Quest Title | `TrackedQuest.Title` | Quest name (truncated) |
| Current Objective | `TrackedObjective.Description` | Active objective |
| Progress | `TrackedObjective.ProgressText` | "2/5" or similar |
| Progress Bar | `TrackedObjective.ProgressPercent` | Mini progress bar |

### 7.3 Tracking Commands

| Command | Behavior |
|---------|----------|
| `TrackQuestCommand` | Set quest as tracked, update HUD |
| `UntrackQuestCommand` | Remove tracking |
| `CycleTrackedObjectiveCommand` | Switch between objectives |
| `ClickTrackingWidgetCommand` | Open journal to tracked quest |

### 7.4 Auto-Tracking Behavior

| Event | Behavior |
|-------|----------|
| Quest Accepted | Auto-track if no quest tracked |
| Objective Complete | Auto-advance to next objective |
| Quest Complete | Show "Return to [NPC]" message |
| Tracked Quest Completed | Auto-track next active quest |

---

## 8. Map Integration

### 8.1 Quest Waypoints

| Property | Type | Description |
|----------|------|-------------|
| `QuestWaypoints` | ObservableCollection | Map markers for quest objectives |
| `TrackedWaypoint` | WaypointViewModel | Primary tracked waypoint |
| `ShowQuestWaypoints` | bool | Toggle waypoint visibility |

### 8.2 Waypoint Display

| Element | Description |
|---------|-------------|
| Icon | Quest type icon (★ for main, ○ for side) |
| Color | Quest type color |
| Pulse | Tracked quest waypoint pulses |
| Label | Objective description (optional) |
| Distance | Distance from player (optional) |

### 8.3 Waypoint Types

| Type | Icon | Description |
|------|------|-------------|
| Objective Location | 🎯 | Where to complete objective |
| Quest Giver | ❗ | NPC location (for turn-in) |
| Item Location | 📦 | Where to find items |
| Enemy Location | ⚔ | Where to find enemies |
| Exploration Target | 🧭 | Room to discover |

### 8.4 Map Commands

| Command | Behavior |
|---------|----------|
| `ShowOnMapCommand` | Open minimap, center on quest location |
| `SetWaypointCommand` | Add custom waypoint for objective |
| `ClearWaypointsCommand` | Remove quest waypoints |
| `ToggleWaypointsCommand` | Show/hide all quest waypoints |

---

## 9. Filter & Sort Controls

### 9.1 Filter Options

| Property | Type | Description |
|----------|------|-------------|
| `FilterCategory` | QuestCategory? | Filter by category |
| `FilterType` | QuestType? | Filter by type |
| `FilterGiver` | string? | Filter by quest giver |
| `SearchQuery` | string | Text search in title/description |

### 9.2 Filter Dropdown Options

**Category Filter:**

| Option | Value | Description |
|--------|-------|-------------|
| All Categories | null | Show all |
| Combat | QuestCategory.Combat | Combat quests only |
| Exploration | QuestCategory.Exploration | Exploration quests |
| Retrieval | QuestCategory.Retrieval | Collection quests |
| Delivery | QuestCategory.Delivery | Delivery quests |
| Investigation | QuestCategory.Investigation | Investigation quests |
| Dialogue | QuestCategory.Dialogue | Dialogue quests |

**Type Filter:**

| Option | Value | Description |
|--------|-------|-------------|
| All Types | null | Show all |
| Main Quest | QuestType.Main | Main quests only |
| Side Quest | QuestType.Side | Side quests only |
| Dynamic | QuestType.Dynamic | Dynamic quests |
| Repeatable | QuestType.Repeatable | Repeatable quests |

### 9.3 Sort Options

| Property | Type | Description |
|----------|------|-------------|
| `SortBy` | QuestSortOption | Current sort field |
| `SortDescending` | bool | Sort direction |

**Sort Options:**

| Option | Enum Value | Description |
|--------|------------|-------------|
| Type | `QuestSortOption.Type` | Main → Side → Dynamic → Repeatable |
| Title | `QuestSortOption.Title` | Alphabetical by title |
| Progress | `QuestSortOption.Progress` | Most complete first |
| Date Accepted | `QuestSortOption.AcceptedDate` | Most recent first |
| Category | `QuestSortOption.Category` | Group by category |
| Giver | `QuestSortOption.Giver` | Group by quest giver |

### 9.4 Filter Commands

| Command | Behavior |
|---------|----------|
| `SetCategoryFilterCommand` | Apply category filter |
| `SetTypeFilterCommand` | Apply type filter |
| `SetSortCommand` | Apply sort option |
| `ClearFiltersCommand` | Reset all filters |
| `SearchCommand` | Apply text search |

---

## 10. Keyboard Shortcuts

### 10.1 Global Shortcuts

| Key | Command | Description |
|-----|---------|-------------|
| J | `ToggleJournalCommand` | Open/close quest journal |
| Q | `ToggleJournalCommand` | Alternative journal toggle |

### 10.2 Journal Navigation

| Key | Command | Description |
|-----|---------|-------------|
| Tab | `NextTabCommand` | Cycle to next tab |
| Shift+Tab | `PreviousTabCommand` | Cycle to previous tab |
| 1-4 | `SelectTabCommand` | Jump to specific tab |
| ↑/↓ | `SelectQuestCommand` | Navigate quest list |
| Enter | `ToggleTrackingCommand` | Track/untrack selected quest |
| T | `TrackSelectedCommand` | Track selected quest |
| M | `ShowOnMapCommand` | Show selected quest on map |
| Escape | `CloseCommand` | Close journal |
| Delete | `AbandonCommand` | Abandon selected quest |

### 10.3 Filter Shortcuts

| Key | Command | Description |
|-----|---------|-------------|
| F | `FocusFilterCommand` | Focus filter dropdown |
| S | `FocusSortCommand` | Focus sort dropdown |
| / | `FocusSearchCommand` | Focus search box |
| Ctrl+R | `ClearFiltersCommand` | Reset all filters |

---

## 11. Services & Controllers

### 11.1 QuestJournalController

**Responsibilities:**

| Method | Description |
|--------|-------------|
| `OpenJournal()` | Show journal, refresh quest data |
| `CloseJournal()` | Hide journal |
| `SelectQuest(questId)` | Select quest, populate detail panel |
| `TrackQuest(questId)` | Set quest as tracked |
| `AcceptQuest(questId)` | Accept available quest |
| `AbandonQuest(questId)` | Abandon quest with confirmation |
| `TurnInQuest(questId)` | Complete quest turn-in |
| `RefreshQuests()` | Reload quest data from service |

### 11.2 Service Dependencies

| Service | Responsibility |
|---------|----------------|
| `IQuestService` | Quest data operations (accept, abandon, turn-in) |
| `INavigationService` | View navigation |
| `IDialogService` | Confirmation dialogs |
| `IMinimapService` | Map waypoint integration |
| `IPlayerService` | Player quest lists access |
| `INotificationService` | Quest update notifications |

### 11.3 Events

| Event | Trigger | Handler |
|-------|---------|---------|
| `QuestAccepted` | Quest accepted | Refresh list, auto-track |
| `QuestCompleted` | All objectives done | Show notification, update status |
| `QuestTurnedIn` | Quest turned in | Move to completed, show rewards |
| `QuestAbandoned` | Quest abandoned | Move to failed, update tracking |
| `ObjectiveProgress` | Objective updated | Refresh detail panel |

### 11.4 Notifications

| Notification | Display | Duration |
|--------------|---------|----------|
| Quest Accepted | Toast + sound | 3 seconds |
| Objective Complete | Toast + sound | 2 seconds |
| Quest Complete | Modal + fanfare | Until dismissed |
| Quest Available | Toast | 3 seconds |
| Quest Failed | Toast | 3 seconds |

---

## 12. Implementation Roadmap

### 12.1 Phase 1: Core UI (Priority: Critical)

| Task | Description | Complexity |
|------|-------------|------------|
| Create `QuestJournalViewModel` | Main ViewModel with quest collections | Medium |
| Create `QuestViewModel` | Quest item display model | Low |
| Create `ObjectiveViewModel` | Objective display model | Low |
| Create `QuestJournalView.axaml` | Main journal layout | Medium |
| Implement tab navigation | Active/Available/Completed/Failed tabs | Low |
| Implement quest list | Scrollable, selectable quest list | Medium |
| Implement detail panel | Quest information display | Medium |

### 12.2 Phase 2: Interactions (Priority: High)

| Task | Description | Complexity |
|------|-------------|------------|
| Implement quest tracking | Track/untrack functionality | Medium |
| Implement HUD widget | Mini tracking display | Medium |
| Implement quest acceptance | Accept available quests | Low |
| Implement quest turn-in | Turn in complete quests | Low |
| Implement abandon confirmation | Abandon quest with dialog | Low |
| Add keyboard shortcuts | Full keyboard navigation | Medium |

### 12.3 Phase 3: Map Integration (Priority: Medium)

| Task | Description | Complexity |
|------|-------------|------------|
| Add quest waypoints to minimap | Display objective locations | High |
| Implement "Show on Map" | Center map on quest location | Medium |
| Add waypoint indicators | Visual markers on map | Medium |
| Add distance display | Show distance to objectives | Low |

### 12.4 Phase 4: Polish (Priority: Low)

| Task | Description | Complexity |
|------|-------------|------------|
| Add filter controls | Category/type filtering | Medium |
| Add sort controls | Multiple sort options | Medium |
| Add search functionality | Text search in quests | Medium |
| Add animations | Panel transitions, progress updates | Medium |
| Add sound effects | UI feedback sounds | Low |
| Add accessibility features | Screen reader, high contrast | Medium |

---

## Appendix A: Data Flow Diagram

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│   QuestService  │────▶│ QuestController  │────▶│ QuestJournalVM  │
│                 │     │                  │     │                 │
│ - Quest Data    │     │ - Accept Quest   │     │ - Quest Lists   │
│ - Objectives    │     │ - Turn In Quest  │     │ - Selected      │
│ - Rewards       │     │ - Track Quest    │     │ - Filtered      │
│ - Status        │     │ - Abandon Quest  │     │ - Tracked       │
└─────────────────┘     └──────────────────┘     └─────────────────┘
        │                       │                       │
        │                       │                       ▼
        │                       │               ┌─────────────────┐
        │                       │               │ QuestJournalView│
        │                       │               │                 │
        │                       │               │ - Tab Control   │
        │                       │               │ - Quest List    │
        │                       │               │ - Detail Panel  │
        │                       │               │ - Commands      │
        │                       │               └─────────────────┘
        │                       │                       │
        ▼                       ▼                       ▼
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│ PlayerCharacter │     │  MinimapService  │     │   HUD Widget    │
│                 │     │                  │     │                 │
│ - ActiveQuests  │     │ - Waypoints      │     │ - Tracked Quest │
│ - CompletedQ.   │     │ - Markers        │     │ - Objective     │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

---

## Appendix B: ViewModel Property Summary

### QuestJournalViewModel

```csharp
// Collections
ObservableCollection<QuestViewModel> ActiveQuests { get; }
ObservableCollection<QuestViewModel> AvailableQuests { get; }
ObservableCollection<QuestViewModel> CompletedQuests { get; }
ObservableCollection<QuestViewModel> FailedQuests { get; }
ObservableCollection<QuestViewModel> FilteredQuests { get; }

// Selection
QuestTab SelectedTab { get; set; }
QuestViewModel? SelectedQuest { get; set; }
QuestViewModel? TrackedQuest { get; set; }

// Counts
int ActiveQuestCount { get; }
int AvailableQuestCount { get; }
int CompletedQuestCount { get; }
int FailedQuestCount { get; }

// Filters
QuestCategory? FilterCategory { get; set; }
QuestType? FilterType { get; set; }
QuestSortOption SortBy { get; set; }
string SearchQuery { get; set; }

// State
bool IsVisible { get; set; }
bool HasSelectedQuest { get; }
bool HasTrackedQuest { get; }
bool CanAcceptQuest { get; }
bool CanTurnInQuest { get; }
bool CanAbandonQuest { get; }

// Commands
ICommand CloseCommand { get; }
ICommand SelectQuestCommand { get; }
ICommand TrackQuestCommand { get; }
ICommand UntrackQuestCommand { get; }
ICommand AcceptQuestCommand { get; }
ICommand TurnInQuestCommand { get; }
ICommand AbandonQuestCommand { get; }
ICommand ShowOnMapCommand { get; }
ICommand SetFilterCommand { get; }
ICommand SetSortCommand { get; }
ICommand ClearFiltersCommand { get; }
```

---

## Appendix C: Enum Definitions

```csharp
public enum QuestTab
{
    Active,
    Available,
    Completed,
    Failed
}

public enum QuestSortOption
{
    Type,
    Title,
    Progress,
    AcceptedDate,
    Category,
    Giver
}

public enum RewardType
{
    Experience,
    Currency,
    Item,
    Reputation,
    Ability,
    Area,
    Quest
}
```

---

## Document History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Nov 2024 | Initial Quest Journal GUI specification |

---

*This document is part of the Rune & Rust technical documentation suite.*
*Related: GUI_SPECIFICATION.md (v0.45), GUI_GAP_ANALYSIS.md (v1.0)*
