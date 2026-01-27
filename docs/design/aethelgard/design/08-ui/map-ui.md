---
id: SPEC-UI-MAP
title: "Map UI — TUI & GUI Specification"
version: 1.0
status: draft
last-updated: 2025-12-14
related-files:
  - path: "docs/07-environment/navigation.md"
    status: Reference
  - path: "docs/07-environment/room-engine/spatial-layout.md"
    status: Reference
  - path: "docs/08-ui/tui-layout.md"
    status: Reference
---

# Map UI — TUI & GUI Specification

> *"A scavenger who doesn't know where they've been is a corpse who hasn't realized it yet."*

---

## 1. Overview

This specification defines the terminal (TUI) and graphical (GUI) interfaces for map display, including the minimap, world map, room navigation, and vertical layer visualization.

### 1.1 Identity Table

| Property | Value |
|----------|-------|
| Spec ID | `SPEC-UI-MAP` |
| Category | UI System |
| Priority | High |
| Status | Draft |

### 1.2 Map Scales

| Scale | View | Purpose |
|-------|------|---------|
| **Minimap** | 5×5 to 9×9 rooms | Immediate surroundings |
| **Zone Map** | Current dungeon/area | Full exploration tracking |
| **Realm Map** | Inter-realm navigation | Travel planning |
| **World Map** | All Nine Realms | Big-picture orientation |

### 1.3 Design Pillars

- **Fog of War** — Unexplored areas hidden
- **Discovered Persistence** — Visited rooms remain visible
- **Vertical Awareness** — Z-level indicators
- **Point of Interest Markers** — Notable locations marked
- **Fast Travel Integration** — Settlement waypoints

---

## 2. TUI Minimap (Exploration Mode)

### 2.1 Minimap Size Examples

**5×5 (Compact):**
```
╔═══════════════╗
║ ?─?─?─?─?     ║
║ │ │ │ │ │     ║
║ ?─░─░─░─?     ║
║ │ │ │ │ │     ║
║ ?─░─@─░─?     ║
╚═══════════════╝
```

**7×7 (Standard — Default):**
```
╔═══════════════════════╗
║ ? ? ? ? ? ? ?         ║
║                       ║
║ ? ░─░─░─░─░ ?         ║
║   │ │ │ │ │           ║
║ ? ░─░▣@─░─░ ?         ║
║   │ │ │ │ │           ║
║ ? ░─!♦░─◊─↑ ?         ║
║                       ║
║ ? ? ? ? ? ? ?         ║
╚═══════════════════════╝
```

**9×9 (Large):**
```
╔═══════════════════════════════╗
║ ? ? ? ? ? ? ? ? ?             ║
║                               ║
║ ? ? ░─░─░─░─░─░ ? ?           ║
║     │ │ │ │ │ │               ║
║ ? ░─░─░─░─░─░─░─░ ?           ║
║   │ │ │ │ │ │ │ │             ║
║ ? ░─░─░─░▣@─░─░─░ ?           ║
║   │ │ │ │ │ │ │ │             ║
║ ? ░─░─░─░─░─░─░─░ ?           ║
║     │ │ │ │ │ │               ║
║ ? ? ░─░─░─░─░─░ ? ?           ║
║                               ║
║ ? ? ? ? ? ? ? ? ?             ║
╚═══════════════════════════════╝
```

### 2.2 Room Status Symbols (Center of Room)

| Symbol | Meaning | Color |
|--------|---------|-------|
| `@` | Player position | White (bright) |
| `░` | Explored, clear | Gray |
| `!` | Unfinished encounter | Red |
| `◊` | Unsolved puzzle | Yellow |
| `♦` | Uncollected loot | Gold |
| `○` | NPC present | Green |
| `↑` | Stairs up | Cyan |
| `↓` | Stairs down | Cyan |
| `⌂` | Settlement/safe zone | Blue |
| `?` | Unexplored (fog of war) | Dim |

### 2.3 Composite Status Symbols

When a room has multiple statuses, combine them:

| Composite | Meaning |
|-----------|---------|
| `!♦` | Hostile + Loot |
| `!◊` | Hostile + Puzzle |
| `◊♦` | Puzzle + Loot |
| `○♦` | NPC + Loot |
| `↑!` | Stairs + Hostile |

**Priority for single-symbol display (if space limited):**
```
! (hostile) > ◊ (puzzle) > ♦ (loot) > ○ (NPC) > ↑↓ (stairs) > ░ (clear)
```

### 2.4 Wall/Edge Symbols (Between Rooms)

| Symbol | Meaning | Location |
|--------|---------|----------|
| `─` | Open passage (horizontal) | Between rooms |
| `│` | Open passage (vertical) | Between rooms |
| `▣` | Locked door | On wall between rooms |
| `═` | Solid wall | No passage possible |
| ` ` | No connection / unexplored | Between unknown rooms |

**Example with locked doors on walls:**
```
    A   B   C   D   E
  ╔═══════════════════╗
1 ║ ░─░─◊▣░─░─░       ║   ← Locked door between C1 and D1
  ║ │   │   │ │       ║
2 ║ ░───@───!♦░       ║   ← Room D2 has hostile + loot
  ║ │   │ │ │         ║
3 ║ ?───↑▣░▣?         ║   ← Two locked doors on level 3
  ╚═══════════════════╝
```

### 2.5 Minimap in Context

The minimap appears in the upper-right corner during exploration:

```
┌─────────────────────────────────────────────────────────────────────┐
│  HP: 45/60 ████████░░  Stamina: 80/100 ████████░░                   │
├─────────────────────────────────────────────────┬───────────────────┤
│                                                 │ ╔═══════════════╗ │
│  RUINED VESTIBULE                               │ ║ ?─?─?─?─?     ║ │
│  ───────────────                                │ ║   │ │ │       ║ │
│  A crumbling chamber of corroded bronze.        │ ║ ?─░─░▣◊─?     ║ │
│  Faded murals depict Aesir victories.           │ ║   │ │   │     ║ │
│  Exits: [N] Corridor [E] Sealed Door (locked)   │ ║ ?─░─@─░─?     ║ │
│                                                 │ ║   │ │ │ │     ║ │
│  You see:                                       │ ║ ?─!♦░─○─?     ║ │
│  • Broken automaton (dormant)                   │ ╚═══════════════╝ │
│  • Scrap pile                                   │   Z: 0  [M]ap     │
├─────────────────────────────────────────────────┴───────────────────┤
```

---

## 3. TUI Zone Map (Full Screen)

### 3.1 Zone Map Display

Accessed via `map` or `M` command:

```
┌─────────────────────────────────────────────────────────────────────┐
│  ZONE MAP — THE IRON CRYPTS                        Level: -1 (Z:-1) │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│        ? ? ? ? ? ? ? ? ? ? ?                                        │
│        ?┌───┬───┬───┐? ? ? ?                                        │
│        ?│ ░ │ ░ │ ◊ │? ? ? ?     LEGEND                             │
│        ?├───┼───┼───┤? ? ? ?     ─────────────────                  │
│        ?│ ░ │ @ │ ░ │? ? ? ?     @ = You are here                   │
│        ?├───┼───┼───┤? ? ? ?     ░ = Explored                       │
│        ?│ ↑ │ ░ │ ▣ │? ? ? ?     ◊ = Point of Interest              │
│        ?└───┴───┴───┘? ? ? ?     ↑ = Stairs up (to Z:0)             │
│        ? ? ? ? ? ? ? ? ? ? ?     ▣ = Locked door                    │
│                                                                     │
│  Explored: 8/24 rooms (33%)                                         │
│  Points of Interest: 1 discovered                                   │
│  Current Z-Level: -1 (Below Ground)                                 │
├─────────────────────────────────────────────────────────────────────┤
│  [←↑↓→] Navigate  [Z/X] Change level  [F] Fast travel  [C] Close    │
└─────────────────────────────────────────────────────────────────────┘
```

### 3.2 Multi-Level Display

For dungeons with multiple Z-levels:

```
╔═══════════════════════════════════════════════════════════════════╗
║  Z-LEVEL SELECTOR                                                 ║
╟───────────────────────────────────────────────────────────────────╢
║                                                                   ║
║    [Z: +1]  Upper Crypts        (3 rooms explored)                ║
║  ► [Z:  0]  Entry Level         (5 rooms explored)                ║
║    [Z: -1]  Lower Crypts        (8 rooms explored)  ← VIEWING     ║
║    [Z: -2]  Flooded Depths      (0 rooms explored)                ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
```

---

## 4. Room Description Display

### 4.1 Room Panel (TUI)

```
┌─────────────────────────────────────────────────────────────────────┐
│  RUINED VESTIBULE                                     [Z: 0]        │
│  ═══════════════════════════════════════════════════════════════    │
│                                                                     │
│  A crumbling chamber of corroded bronze and cracked stone.          │
│  Faded murals depict Aesir victories, now defaced by age and        │
│  vandalism. The air smells of rust and old decay.                   │
│                                                                     │
│  ─────────────────────────────────────────────────────────────────  │
│  EXITS                                                              │
│  • [N] Narrow Corridor — leads deeper into darkness                 │
│  • [E] Sealed Door — runic lock, unfamiliar pattern (locked)        │
│  • [S] Entry Hall — where you came from                             │
│  ─────────────────────────────────────────────────────────────────  │
│  YOU SEE                                                            │
│  • [1] Broken automaton — dormant, limbs scattered (examinable)     │
│  • [2] Scrap pile — twisted metal and wire (lootable)               │
│  • [3] Wall inscription — barely legible text (readable)            │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│  [1-3] Interact  [N/E/S] Move  [I] Inventory  [M] Map               │
└─────────────────────────────────────────────────────────────────────┘
```

### 4.2 Exit Display Format

| Exit Type | Display | Color |
|-----------|---------|-------|
| Open passage | `[N] Corridor` | White |
| Locked door | `[E] Sealed Door (locked)` | Magenta |
| Barred | `[W] Collapsed Tunnel (blocked)` | Red |
| Hidden (discovered) | `[SECRET] Concealed Panel` | Yellow |
| Stairs up | `[↑] Stairs Up (to Z:+1)` | Cyan |
| Stairs down | `[↓] Stairs Down (to Z:-1)` | Cyan |

### 4.3 Object Interaction Tags

| Tag | Meaning |
|-----|---------|
| `(examinable)` | Can be examined for details |
| `(lootable)` | Contains items |
| `(readable)` | Has text content |
| `(interactable)` | Can be used/activated |
| `(hostile)` | Enemy present |
| `(friendly)` | NPC available for dialogue |

---

## 5. Realm Map (Region View)

### 5.1 Realm Map Display

For inter-realm navigation (accessed via `world` command):

```
┌─────────────────────────────────────────────────────────────────────┐
│  REALM MAP — MIDGARD                                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│                        ▲ NIFLHEIM                                   │
│                        │ (7-12 days)                                │
│                        │                                            │
│      ┌─────────────────┼─────────────────┐                          │
│      │                 │                 │                          │
│      │   THE MIRE    IRONHOLD   THE SCAR │                          │
│      │       ○          ⌂          ◊     │                          │
│      │                  │                │                          │
│      │      ASHENDALE ──┼── KNOTWOOD     │                          │
│      │         ⌂        │       ○        │                          │
│      │                  │                │                          │
│      │     RIVERCROSS ──┴── DEEPWELL     │                          │
│      │         @            ⌂           ◀──── JÖTUNHEIM (5-8 days)  │
│      │                                   │                          │
│      └─────────────────┬─────────────────┘                          │
│                        │                                            │
│                        ▼ VANAHEIM                                   │
│                        (6-9 days)                                   │
│                                                                     │
│  @ = Current Location (Rivercross)                                  │
│  ⌂ = Settlement (fast travel available)                             │
│  ○ = Point of Interest                                              │
│  ◊ = Dungeon Entrance                                               │
├─────────────────────────────────────────────────────────────────────┤
│  [F] Fast travel  [Enter] Select location  [C] Close                │
└─────────────────────────────────────────────────────────────────────┘
```

### 5.2 Route Classification Display

When selecting a route:

```
╔═══════════════════════════════════════════════════════════════════╗
║  TRAVEL ROUTE: RIVERCROSS → IRONHOLD                              ║
╟───────────────────────────────────────────────────────────────────╢
║                                                                   ║
║  Route Class: B (Secondary Route)                                 ║
║  Distance: ~45 km                                                 ║
║  Travel Time: 2-3 days                                            ║
║                                                                   ║
║  Route Conditions:                                                ║
║  • Weather: Clear                                                 ║
║  • Hazard Level: Low                                              ║
║  • Patrol Status: Rangers active                                  ║
║                                                                   ║
║  Requirements:                                                    ║
║  • Supplies: 3 days rations                                       ║
║  • No faction restrictions                                        ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  [T] Begin Travel  [C] Cancel                                     ║
╚═══════════════════════════════════════════════════════════════════╝
```

---

## 6. Long-Distance Travel System

> [!NOTE]
> **Design Decision:** Fast travel (instant teleportation) may break immersion. This section offers alternatives that maintain the journey experience while reducing tedium.

### 6.1 Travel Options

| Option | Description | Immersion | Speed |
|--------|-------------|-----------|-------|
| **Manual Travel** | Walk room-by-room | Maximum | Slowest |
| **Auto-Pathfinding** | Select destination, character walks automatically | High | Medium |
| **Journey Mode** | Time-skip with random encounter checks | Medium | Fast |
| **Fast Travel** | Instant arrival (if enabled) | Low | Instant |

### 6.2 Auto-Pathfinding

Player selects destination on map; character automatically navigates:

```
> travel to Ironhold

  [Auto-pathfinding to Ironhold...]
  Route: 47 rooms via Long Road (Class B)
  Estimated time: 2 days
  
  [Press SPACE to pause, ESC to cancel]
  
  Traveling... ████████░░░░░░░░ (35%)
  
  [!] ENCOUNTER — Bandits block your path!
  [Auto-travel paused. Entering combat...]
```

**Features:**
- Can be interrupted by encounters
- Player watches movement on minimap
- Consumes supplies in real-time
- Full control retained

### 6.3 Journey Mode (Time-Skip)

For known safe routes between settlements:

```
╔═══════════════════════════════════════════════════════════════════╗
║  JOURNEY: RIVERCROSS → IRONHOLD                                   ║
╟───────────────────────────────────────────────────────────────────╢
║                                                                   ║
║  Route: Long Road (Class B)                                       ║
║  Distance: ~45 km                                                 ║
║  Travel Time: 2-3 days                                            ║
║                                                                   ║
║  JOURNEY CHECKS:                                                  ║
║  • Weather Roll: Potential delay (20% chance)                     ║
║  • Encounter Roll: Random encounter (15% chance/day)              ║
║  • Supply Cost: 3 rations consumed                                ║
║                                                                   ║
║  Requirements Met:                                                ║
║  ✓ Route previously traveled                                      ║
║  ✓ No hostile faction control                                     ║
║  ✓ Sufficient supplies (3/10 rations)                             ║
║                                                                   ║
╟───────────────────────────────────────────────────────────────────╢
║  [J] Begin Journey  [A] Auto-pathfind instead  [C] Cancel         ║
╚═══════════════════════════════════════════════════════════════════╝
```

**Journey Outcome:**
```
  [Journey to Ironhold...]
  
  Day 1: Uneventful travel. (-1 ration)
  Day 2: Rain delays progress by 4 hours. (-1 ration)
  Day 3: Arrived at Ironhold gates. (-1 ration)
  
  Total: 3 days, 3 rations consumed, no encounters.
  
  Welcome to Ironhold.
```

### 6.4 Configuration

| Setting | Default | Options |
|---------|---------|---------|
| `AllowFastTravel` | false | true/false |
| `JourneyEncounterChance` | 15% | 0-50% |
| `AutoTravelSpeed` | 1x | 1x/2x/4x |
| `ShowTravelAnimation` | true | true/false |

---

## 7. GUI Map Panel

### 7.1 Minimap (HUD Overlay)

```
┌─────────────────────────────────────────────────────────────────────┐
│                                                    ┌─────────────┐  │
│                                                    │ ░ ░ ◊ ░ ?   │  │
│  [Room Description Area]                           │ ░ ░ ░ ░ ?   │  │
│                                                    │ ░ @ ░ ↑ ?   │  │
│                                                    │ ░ ░ ░ ░ ?   │  │
│                                                    │ ? ? ? ? ?   │  │
│                                                    ├─────────────┤  │
│                                                    │ Z: 0  Iron  │  │
│                                                    │    Crypts   │  │
│                                                    └─────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### 7.2 Full Map View (GUI)

```
┌───────────────────────────────────────────────────────────────────────┐
│  ZONE MAP — THE IRON CRYPTS                              [Z-Level: 0] │
├───────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────────┐   │
│ │                                                                 │   │
│ │              ┌───────────────────────────────────┐              │   │
│ │              │                                   │              │   │
│ │              │   Interactive Grid Map            │              │   │
│ │              │   - Click rooms to select         │              │   │
│ │              │   - Hover for room info           │              │   │
│ │              │   - Drag to pan                   │              │   │
│ │              │   - Scroll to zoom                │              │   │
│ │              │                                   │              │   │
│ │              └───────────────────────────────────┘              │   │
│ │                                                                 │   │
│ └─────────────────────────────────────────────────────────────────┘   │
├───────────────────────────────────────────────────────────────────────┤
│ SELECTED ROOM: Ruined Vestibule                                       │
│ Explored: Yes  |  Hostiles: None  |  Loot: Available                  │
├───────────────────────────────────────────────────────────────────────┤
│ [Z-1] [Z 0] [Z+1]  |  [Legend]  |  [Fast Travel]  |  [Close]          │
└───────────────────────────────────────────────────────────────────────┘
```

### 7.3 GUI Interactive Elements

| Element | Click | Hover | Right-Click |
|---------|-------|-------|-------------|
| Room tile | Select/Navigate | Show room name | Context menu |
| Exit arrow | Navigate that direction | Show destination | — |
| POI marker | Show details | Tooltip | Set waypoint |
| Z-level button | Switch level | — | — |

---

## 8. MapViewModel

### 8.1 Interface

```csharp
public interface IMapViewModel
{
    // Current State
    RoomViewModel CurrentRoom { get; }
    ZoneViewModel CurrentZone { get; }
    int CurrentZLevel { get; }
    
    // Map Data
    IReadOnlyList<RoomViewModel> ExploredRooms { get; }
    IReadOnlyList<PointOfInterestViewModel> PointsOfInterest { get; }
    IReadOnlyList<SettlementViewModel> Waypoints { get; }
    
    // Minimap
    RoomViewModel[,] MinimapGrid { get; }
    int MinimapSize { get; }
    
    // Navigation
    IReadOnlyList<ExitViewModel> AvailableExits { get; }
    
    // Fast Travel
    IReadOnlyList<WaypointViewModel> FastTravelDestinations { get; }
    bool CanFastTravel { get; }
    
    // Commands
    ICommand NavigateCommand { get; }
    ICommand OpenMapCommand { get; }
    ICommand FastTravelCommand { get; }
    ICommand ChangeZLevelCommand { get; }
}

public record RoomViewModel(
    Guid Id,
    string Name,
    string? Description,
    int X, int Y, int Z,
    bool IsExplored,
    bool HasHostiles,
    bool HasLoot,
    bool HasNpc,
    bool IsCurrentRoom
);

public record ExitViewModel(
    Direction Direction,
    string DestinationName,
    ExitType Type,
    bool IsLocked,
    bool IsBlocked
);

public enum Direction { North, South, East, West, Up, Down }
public enum ExitType { Open, Door, LockedDoor, Stairs, Ladder, Hidden }
```

---

## 9. Configuration

| Setting | Default | Options |
|---------|---------|---------|
| `MinimapSize` | 7 | 5/7/9 |
| `ShowMinimapInCombat` | false | true/false |
| `AutoRevealAdjacent` | true | true/false |
| `ShowRoomNames` | true | true/false |
| `FastTravelConfirm` | true | true/false |

---

## 10. Implementation Status

| Component | TUI Status | GUI Status |
|-----------|------------|------------|
| Minimap display | ❌ Planned | ❌ Planned |
| Zone map (full screen) | ❌ Planned | ❌ Planned |
| Room description panel | ❌ Planned | ❌ Planned |
| Z-level navigation | ❌ Planned | ❌ Planned |
| Realm map | ❌ Planned | ❌ Planned |
| Fast travel menu | ❌ Planned | ❌ Planned |
| Fog of war | ❌ Planned | ❌ Planned |
| MapViewModel | ❌ Planned | ❌ Planned |

---

## 11. Phased Implementation Guide

### Phase 1: Core Minimap
- [ ] Minimap display in exploration mode
- [ ] Player position marker
- [ ] Fog of war (unexplored rooms)
- [ ] Basic symbols (walls, rooms, exits)

### Phase 2: Room Display
- [ ] Room description panel
- [ ] Exit list with types
- [ ] Object list with interactions
- [ ] Z-level indicator

### Phase 3: Zone Map
- [ ] Full-screen zone map
- [ ] Multi-level navigation (Z/X keys)
- [ ] Exploration percentage
- [ ] Points of interest markers

### Phase 4: Fast Travel
- [ ] Waypoint discovery
- [ ] Fast travel menu
- [ ] Travel time/supply calculation
- [ ] Random encounter chance

### Phase 5: GUI
- [ ] MapViewModel
- [ ] Interactive grid map
- [ ] Zoom/pan controls
- [ ] Realm map with route selection

---

## 12. Testing Requirements

### 12.1 TUI Tests
- [ ] Minimap updates when moving
- [ ] Fog of war hides unexplored rooms
- [ ] Z-level changes correctly
- [ ] Exits display proper types

### 12.2 GUI Tests
- [ ] Click navigation works
- [ ] Hover tooltips appear
- [ ] Zoom/pan smooth

### 12.3 Integration Tests
- [ ] Fast travel consumes supplies
- [ ] Waypoints persist between sessions
- [ ] Room discovery saves to database

---

## 13. Related Specifications

| Spec | Relationship |
|------|--------------|
| [navigation.md](../07-environment/navigation.md) | Route classification, travel mechanics |
| [spatial-layout.md](../07-environment/room-engine/spatial-layout.md) | Room/zone structure |
| [tui-layout.md](tui-layout.md) | Screen composition |
| [commands/navigation.md](commands/navigation.md) | Navigation commands |

---

## 14. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-14 | Initial specification |
