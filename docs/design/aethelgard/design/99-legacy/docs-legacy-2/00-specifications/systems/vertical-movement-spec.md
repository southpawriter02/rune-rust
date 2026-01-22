# Vertical Movement & Z-Axis System Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-27
> **Status**: Active
> **Specification ID**: SPEC-SYSTEM-003

---

## Document Control

### Version History
| Version | Date | Author | Changes | Reviewers |
|---------|------|--------|---------|-----------|
| 1.0 | 2025-11-27 | AI | Initial specification | - |

### Approval Status
- [x] **Draft**: Initial authoring in progress
- [x] **Review**: Ready for stakeholder review
- [x] **Approved**: Approved for implementation
- [x] **Active**: Currently implemented and maintained
- [ ] **Deprecated**: Superseded by [SPEC-ID]

### Stakeholders
- **Primary Owner**: Level Designer
- **Design**: Multi-layer dungeon structure, traversal mechanics
- **Balance**: Skill check DCs, fall damage scaling
- **Implementation**: VerticalTraversalService.cs, SpatialLayoutService.cs
- **QA/Testing**: Traversal edge cases, layer connectivity

---

## Executive Summary

### Purpose Statement
The Vertical Movement System enables 3D dungeon exploration through seven vertical layers (-300m to +300m), providing skill-based traversal mechanics, fall damage, and layer-specific environmental characteristics.

### Scope
**In Scope**:
- Seven vertical layers from DeepRoots (-3) to Canopy (+3)
- Vertical connection types (Stairs, Shaft, Elevator, Ladder, Collapsed)
- Traversal skill checks (MIGHT-based Athletics)
- Fall damage calculation for failed traversals
- Blocked passage clearing mechanics
- Layer-specific biome associations

**Out of Scope**:
- In-combat elevation changes → `SPEC-COMBAT-001` (Combat Grid)
- Room generation within layers → `SPEC-SYSTEM-008` (Procedural Room Generation)
- Biome-specific hazards → Individual biome specifications
- Flight/levitation mechanics → Future enhancement

### Success Criteria
- **Player Experience**: Exploration feels three-dimensional with meaningful vertical choices
- **Technical**: Traversal checks resolve in <100ms, pathfinding respects Z-axis
- **Design**: Each layer has distinct character; vertical progress feels earned
- **Balance**: Skill checks create tension without frustrating progress

---

## Related Documentation

### Dependencies
**Depends On**:
- Character System: MIGHT attribute for Athletics checks → `SPEC-PROGRESSION-001`
- Dice System: Skill check resolution → `docs/01-systems/dice-pool.md`
- Room System: Room positions in 3D space → `SPEC-SYSTEM-008`

**Depended Upon By**:
- Dungeon Generator: Creates vertical connections → `docs/03-technical-reference/services/`
- Navigation System: Pathfinding across layers → Future
- Quest System: Layer-specific objectives → `docs/01-systems/quests.md`

### Related Specifications
- `SPEC-COMBAT-001`: Combat Resolution (in-combat elevation)
- `SPEC-SYSTEM-008`: Procedural Room Generation
- `SPEC-WORLD-001`: Biome System

### Code References
- **Primary Service**: `RuneAndRust.Engine/Spatial/VerticalTraversalService.cs`
- **Core Models**: `RuneAndRust.Core/Spatial/VerticalLayer.cs`
- **Connection Model**: `RuneAndRust.Core/Spatial/VerticalConnection.cs`
- **Connection Types**: `RuneAndRust.Core/Spatial/VerticalConnectionType.cs`
- **Layout Service**: `RuneAndRust.Engine/Spatial/SpatialLayoutService.cs`
- **Tests**: `RuneAndRust.Tests/Spatial/VerticalConnectionTests.cs`

---

## Design Philosophy

### Design Pillars

1. **Vertical Exploration Rewards Preparation**
   - **Rationale**: Climbing into dangerous depths should require character investment
   - **Examples**: High MIGHT enables safer traversal; fall damage creates meaningful risk

2. **Layers Tell Environmental Stories**
   - **Rationale**: Each layer should feel distinct, reinforcing world depth
   - **Examples**: DeepRoots = ancient/decayed, Canopy = ash-filled surface exposure

3. **Multiple Traversal Options**
   - **Rationale**: Players should have choices in how they navigate vertically
   - **Examples**: Stairs (safe, slow), Shafts (fast, risky), Elevators (variable)

### Player Experience Goals
**Target Experience**: Sense of adventure when descending into depths; relief when finding safe passage

**Moment-to-Moment Gameplay**:
- Player discovers vertical connection in room
- Evaluates risk (DC, potential damage) vs reward (destination layer)
- Makes informed decision to attempt or find alternative route
- Skill check creates tension; success/failure has meaningful consequences

**Learning Curve**:
- **Novice** (0-2 hours): Understands stairs = safe, other types = risky
- **Intermediate** (2-10 hours): Invests in MIGHT for safer climbing; plans routes
- **Expert** (10+ hours): Exploits shortcuts via risky traversals; clears blockages strategically

### Design Constraints
- **Technical**: Z-coordinate stored as integer (-3 to +3)
- **Gameplay**: Maximum 4 layers per single traversal (elevators excluded)
- **Narrative**: Vertical structure supports Yggdrasil metaphor
- **Scope**: No jumping/flying; all traversal via defined connections

---

## Functional Requirements

### FR-001: Define Vertical Layers
**Priority**: Critical
**Status**: Implemented

**Description**:
System must support seven vertical layers with distinct characteristics, each representing ~100m of vertical distance.

**Rationale**:
Multi-layer structure creates sense of depth and progression through the dungeon.

**Acceptance Criteria**:
- [x] Seven layers defined: DeepRoots (-3) through Canopy (+3)
- [x] Each layer has approximate depth/height value
- [x] Each layer has descriptive text and characteristics
- [x] Each layer has associated typical biomes

**Layer Definitions**:
| Layer | Z | Depth | Description |
|-------|---|-------|-------------|
| DeepRoots | -3 | -300m | Ancient infrastructure, extreme decay |
| LowerRoots | -2 | -200m | Lower maintenance, geothermal systems |
| UpperRoots | -1 | -100m | Upper maintenance, cooling systems |
| GroundLevel | 0 | 0m | Primary access, main corridors |
| LowerTrunk | +1 | +100m | Mid-facility, administrative |
| UpperTrunk | +2 | +200m | Upper facility, observation |
| Canopy | +3 | +300m | Surface exposure, environmental interface |

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Core/Spatial/VerticalLayer.cs`

---

### FR-002: Define Connection Types
**Priority**: Critical
**Status**: Implemented

**Description**:
System must support multiple vertical connection types with different traversal mechanics.

**Rationale**:
Variety of connection types creates tactical decisions and exploration variety.

**Acceptance Criteria**:
- [x] Stairs: Free traversal, 1-2 levels max
- [x] Shaft: MIGHT DC 12, 2-4 levels, fall damage on failure
- [x] Elevator: Auto if powered, WITS DC 15 to repair if not
- [x] Ladder: MIGHT DC 10, 1-3 levels, minor fall damage
- [x] Collapsed: Impassable until cleared, MIGHT DC 15

**Connection Type Summary**:
| Type | DC | Attribute | Max Levels | Fall Damage |
|------|----|-----------|-----------:|-------------|
| Stairs | 0 | - | 2 | None |
| Shaft | 12 | MIGHT | 4 | 2d6-6d6 |
| Elevator | 0/15 | -/WITS | 6 | None |
| Ladder | 10 | MIGHT | 3 | 1d4-2d4 |
| Collapsed | 15 | MIGHT | 0 | None (blocks) |

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Core/Spatial/VerticalConnectionType.cs`

---

### FR-003: Attempt Traversal
**Priority**: Critical
**Status**: Implemented

**Description**:
System must resolve traversal attempts through skill checks, applying damage on failures and moving character on success.

**Rationale**:
Traversal attempts are the core gameplay mechanic for vertical exploration.

**Acceptance Criteria**:
- [x] Blocked connections return failure with message
- [x] Stairs return automatic success
- [x] Powered elevators return automatic success
- [x] Shaft/Ladder require Athletics check (d20 + MIGHT vs DC)
- [x] Failed check calculates and applies fall damage
- [x] Success returns flavor text and levels traversed

**Example Scenarios**:
1. **Scenario**: Player attempts shaft climb
   - **Input**: Character MIGHT 5, Shaft DC 12
   - **Roll**: d20=10, Total=15
   - **Expected Output**: Success, move to destination, flavor text
   - **Success Condition**: Character at new Z level

2. **Edge Case**: Catastrophic failure (margin -5 or worse)
   - **Input**: Character MIGHT 3, Shaft DC 12, Roll 2
   - **Expected Behavior**: Extra damage die added
   - **Example**: 3d6 instead of 2d6 damage

**Dependencies**:
- Requires: FR-001 (layers defined)
- Requires: FR-002 (connection types defined)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/Spatial/VerticalTraversalService.cs:AttemptTraversal()`

---

### FR-004: Calculate Fall Damage
**Priority**: High
**Status**: Implemented

**Description**:
System must calculate fall damage based on connection type, levels spanned, and check failure margin.

**Rationale**:
Fall damage creates meaningful risk for risky traversals.

**Formula/Logic**:
```
Shaft Fall Damage:
  BaseDice = 2 + LevelsSpanned (max 6)
  DieSize = d6
  If FailureMargin <= -5: BaseDice += 1

  Example: Fall from 2-level shaft
    BaseDice = 2 + 2 = 4d6
    Roll: 4d6 = [3, 5, 2, 6] = 16 damage

Ladder Fall Damage:
  BaseDice = 1 + (LevelsSpanned / 2)
  DieSize = d4

  Example: Fall from 2-level ladder
    BaseDice = 1 + 1 = 2d4
    Roll: 2d4 = [2, 3] = 5 damage
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| ShaftBaseDice | int | 2-6 | 2 | Base d6s for shaft fall | No |
| ShaftDieSize | int | 6 | 6 | Die size for shaft damage | No |
| LadderBaseDice | int | 1-2 | 1 | Base d4s for ladder fall | No |
| LadderDieSize | int | 4 | 4 | Die size for ladder damage | No |
| CatastrophicThreshold | int | -5 | -5 | Margin for extra damage | Yes |

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/Spatial/VerticalTraversalService.cs:CalculateFallDamage()`

---

### FR-005: Clear Blocked Passages
**Priority**: Medium
**Status**: Implemented

**Description**:
System must allow characters to clear blocked/collapsed passages via skill check.

**Rationale**:
Blocked passages create exploration puzzles; clearing them rewards persistence.

**Acceptance Criteria**:
- [x] Only applies to blocked or collapsed connections
- [x] Requires MIGHT Athletics check vs ClearanceDC
- [x] Success unblocks passage (sets IsBlocked = false)
- [x] Failure deals minor exertion damage (margin as damage)
- [x] Takes time (ClearanceTimeMinutes)

**Example Scenarios**:
1. **Scenario**: Clear collapsed stairwell
   - **Input**: Collapsed connection, ClearanceDC 15, Character MIGHT 6
   - **Roll**: d20=12, Total=18 vs DC 15
   - **Expected Output**: Success, passage cleared, 10 minutes elapsed
   - **Success Condition**: Connection.IsBlocked = false

2. **Edge Case**: Failed clearance attempt
   - **Input**: ClearanceDC 15, Roll total 10
   - **Expected Behavior**: Failure, margin (5) dealt as Physical damage

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/Spatial/VerticalTraversalService.cs:AttemptClearBlockage()`

---

### FR-006: Get Reachable Layers
**Priority**: Medium
**Status**: Implemented

**Description**:
System must determine which vertical layers are reachable from a given room via BFS across non-blocked connections.

**Rationale**:
Pathfinding and UI need to show accessible areas; quest system needs to verify completion possibility.

**Acceptance Criteria**:
- [x] BFS traversal from start room
- [x] Blocked connections excluded from search
- [x] Returns set of reachable VerticalLayer values
- [x] Sorted by layer order (ascending)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/Spatial/VerticalTraversalService.cs:GetReachableLayers()`

---

### FR-007: Connection Validation
**Priority**: High
**Status**: Implemented

**Description**:
System must validate whether a connection can be traversed before allowing attempt.

**Rationale**:
Pre-validation provides clear feedback about why traversal is blocked.

**Acceptance Criteria**:
- [x] Blocked connections return false
- [x] Collapsed connections return false
- [x] Stairs always return true
- [x] Unpowered elevators return true (can attempt shaft climb)
- [x] Other types return true (check required but traversable)

**Implementation Notes**:
- **Code Location**: `RuneAndRust.Engine/Spatial/VerticalTraversalService.cs:CanTraverse()`

---

## System Mechanics

### Mechanic 1: Skill Check Resolution

**Overview**:
Vertical traversal uses Athletics skill checks resolved via d20 + MIGHT vs DC.

**How It Works**:
1. Get connection's TraversalDC
2. Roll d20, add character's MIGHT attribute
3. Compare total to DC
4. Success = traverse, Failure = fall damage

**Formula**:
```
TotalRoll = d20 + MIGHT
Success = TotalRoll >= DC
Margin = TotalRoll - DC

Example:
  Character MIGHT = 5
  Shaft DC = 12
  d20 Roll = 8
  TotalRoll = 8 + 5 = 13
  Margin = 13 - 12 = +1
  Result: SUCCESS (Margin >= 0)
```

**Parameters**:
| Parameter | Type | Range | Default | Description | Tunable? |
|-----------|------|-------|---------|-------------|----------|
| StairsDC | int | 0 | 0 | Never fails | No |
| ShaftDC | int | 10-15 | 12 | Athletics DC for shafts | Yes |
| LadderDC | int | 8-12 | 10 | Athletics DC for ladders | Yes |
| ElevatorRepairDC | int | 12-18 | 15 | WITS DC to repair | Yes |
| ClearanceDC | int | 12-18 | 15 | MIGHT DC to clear blockage | Yes |

**Edge Cases**:
1. **Natural 20**: Auto-success (future enhancement - not currently implemented)
   - **Condition**: d20 = 20
   - **Behavior**: Currently normal success; could add bonus

2. **Extremely low MIGHT**: Still possible to succeed
   - **Condition**: MIGHT = 1, DC = 12
   - **Behavior**: Need d20 >= 11 (45% success)
   - **Example**: Character with 1 MIGHT rolls 15 → Total 16 → Success

**Related Requirements**: FR-003, FR-004, FR-005

---

### Mechanic 2: Layer Characteristics

**Overview**:
Each vertical layer has distinct environmental characteristics affecting gameplay and narrative.

**Layer Details**:

| Layer | Characteristics | Typical Biomes |
|-------|-----------------|----------------|
| DeepRoots (-3) | Ancient, extreme decay, structural instability | The_Roots, Jotunheim |
| LowerRoots (-2) | Geothermal activity, steam vents, high heat | The_Roots, Muspelheim |
| UpperRoots (-1) | Cryogenic systems, frozen zones, ice | The_Roots, Niflheim |
| GroundLevel (0) | Most common, varied, primary pathways | All biomes |
| LowerTrunk (+1) | Industrial, Aetheric resonance, mechanical | Jotunheim, Alfheim |
| UpperTrunk (+2) | High Aether, reality distortions | Alfheim |
| Canopy (+3) | Surface exposure, ash-filled sky, hazards | Alfheim |

**Data Flow**:
```
Input Sources:
  → Room.Position.Z (Z coordinate)
  → VerticalLayerExtensions.FromZCoordinate()

Processing:
  → Map Z to VerticalLayer enum
  → Retrieve characteristics via extension methods

Output Destinations:
  → Room descriptions
  → Biome selection
  → Environmental hazards
```

**Related Requirements**: FR-001

---

### Mechanic 3: Connection Bidirectionality

**Overview**:
Most vertical connections are bidirectional, allowing traversal in both directions.

**How It Works**:
1. Connections store FromRoomId and ToRoomId
2. IsBidirectional flag (default true) enables reverse traversal
3. GetConnectionBetween() checks both directions if bidirectional
4. Some connections may be one-way (e.g., drops without climbing)

**Edge Cases**:
1. **One-way drop**: Bidirectional = false
   - **Condition**: Shaft with no handholds
   - **Behavior**: Can descend but not ascend
   - **Example**: Emergency escape chute

2. **Powered vs unpowered elevator direction**:
   - **Condition**: Elevator without power
   - **Behavior**: Can climb shaft in both directions (if strength allows)

**Related Requirements**: FR-003, FR-007

---

## State Management

### System State

**State Variables**:
| Variable | Type | Persistence | Default | Description |
|----------|------|-------------|---------|-------------|
| connection.IsBlocked | bool | Permanent | false | Whether passage is blocked |
| connection.IsPowered | bool? | Permanent | null | Elevator power state |
| room.Position.Z | int | Permanent | 0 | Room's vertical coordinate |

**State Transitions**:
```
[Blocked] ---ClearBlockage(success)---> [Unblocked]
[Unpowered] ---RepairElevator---> [Powered]
[At Layer A] ---Traverse(success)---> [At Layer B]
```

### Persistence Requirements

**Must Persist**:
- Connection.IsBlocked: Player may have cleared passage
- Connection.IsPowered: Player may have repaired elevator
- Character.Position.Z: Current layer

**Save Format**:
- Room positions stored with dungeon seed
- Connection states stored per-dungeon
- Character Z persisted via room ID

---

## Integration Points

### Systems This System Consumes

#### Integration with Dice Service
**What We Use**: d20 and dX rolling for checks and damage
**How We Use It**: ResolveAthleticsCheck() calls dice service
**Dependency Type**: Soft (fallback random available)
**Failure Handling**: Use built-in random if dice service unavailable

#### Integration with Character System
**What We Use**: Character.Attributes["MIGHT"] for checks
**How We Use It**: Add to d20 roll for total
**Dependency Type**: Hard
**Failure Handling**: Missing attribute defaults to 0

### Systems That Consume This System

#### Consumed By Dungeon Generator
**What They Use**: VerticalConnection creation methods
**How They Use It**: CreateStairs(), CreateShaft(), CreateElevator()
**Stability Contract**: Factory methods always produce valid connections

#### Consumed By Navigation System
**What They Use**: GetReachableLayers(), CanTraverse()
**How They Use It**: Pathfinding across layers
**Stability Contract**: Reachability always reflects current blockage state

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Min | Max | Impact | Change Frequency |
|-----------|----------|---------------|-----|-----|--------|------------------|
| ShaftDC | VerticalConnectionType | 12 | 10 | 15 | Climbing difficulty | Medium |
| LadderDC | VerticalConnectionType | 10 | 8 | 12 | Ladder safety | Medium |
| ClearanceDC | VerticalConnection | 15 | 12 | 18 | Blockage challenge | Medium |
| ClearanceTime | VerticalConnection | 10 min | 5 | 30 | Time cost | Low |

### Balance Targets

**Target 1: Traversal Success Rate**
- **Metric**: Success rate for average character (MIGHT 5) on shaft
- **Current**: ~65% (d20+5 >= 12)
- **Target**: 60-70% for meaningful risk
- **Levers**: DC adjustment

**Target 2: Fall Damage Severity**
- **Metric**: Average damage as % of starting HP
- **Current**: 2d6 (~7 dmg) vs 50 HP = 14%
- **Target**: 10-20% for single layer fall
- **Levers**: Dice count, die size

---

## Implementation Guidance

### Implementation Status

**Current State**: Complete

**Completed**:
- [x] VerticalLayer enum with 7 layers
- [x] VerticalConnectionType enum with 5 types
- [x] VerticalConnection model with all properties
- [x] VerticalTraversalService with all operations
- [x] Fall damage calculation
- [x] Blockage clearing
- [x] Reachable layer BFS
- [x] Unit tests

### Code Architecture

**Recommended Structure**:
```
RuneAndRust.Core/Spatial/
  ├─ VerticalLayer.cs           // Layer enum with extensions
  ├─ VerticalConnectionType.cs  // Connection type enum
  ├─ VerticalConnection.cs      // Connection model
  └─ RoomPosition.cs            // Room 3D coordinates

RuneAndRust.Engine/Spatial/
  ├─ VerticalTraversalService.cs  // Traversal mechanics
  ├─ SpatialLayoutService.cs      // Layout management
  └─ SpatialValidationService.cs  // Validation rules

RuneAndRust.Tests/Spatial/
  ├─ VerticalConnectionTests.cs
  └─ VerticalLayerTests.cs
```

---

## Testing & Verification

### Test Scenarios

#### Test Case 1: Successful Shaft Traversal
**Type**: Unit

**Objective**: Verify shaft traversal with passing check

**Test Steps**:
1. Create shaft connection (DC 12)
2. Create character with MIGHT 5
3. Mock dice roll to return 10
4. Call AttemptTraversal

**Expected Results**:
- Result.Success = true
- Result.Damage = 0
- Result.LevelsTraversed = connection.LevelsSpanned

#### Test Case 2: Failed Ladder with Fall Damage
**Type**: Unit

**Objective**: Verify fall damage on failed ladder climb

**Test Steps**:
1. Create ladder connection (DC 10, 2 levels)
2. Create character with MIGHT 3
3. Mock dice roll to return 5
4. Call AttemptTraversal

**Expected Results**:
- Result.Success = false
- Result.Damage > 0 (2d4 = 2-8)
- Result.LevelsTraversed = 0

---

## Appendix

### Appendix A: Terminology

| Term | Definition |
|------|------------|
| **Vertical Layer** | One of 7 Z-levels in dungeon structure |
| **Connection** | Link between rooms at different Z levels |
| **Traversal** | Act of moving through a vertical connection |
| **Athletics Check** | d20 + MIGHT vs DC |
| **Fall Damage** | HP loss from failed climbing attempt |
| **Blockage** | Impassable obstacle requiring clearance |

### Appendix B: Layer Depth Diagram

```
+300m ┌──────────────┐ Canopy (+3)
      │   Ash Sky    │
+200m ├──────────────┤ UpperTrunk (+2)
      │  Observation │
+100m ├──────────────┤ LowerTrunk (+1)
      │   Admin      │
  0m  ├══════════════┤ GroundLevel (0) ← Entry
      │   Main       │
-100m ├──────────────┤ UpperRoots (-1)
      │   Cryo       │
-200m ├──────────────┤ LowerRoots (-2)
      │  Geothermal  │
-300m └──────────────┘ DeepRoots (-3)
```

### Appendix C: Success Probability Table

**d20 + MIGHT vs DC (probability of success)**:

| MIGHT | DC 10 | DC 12 | DC 15 |
|------:|------:|------:|------:|
| 1 | 55% | 45% | 30% |
| 3 | 65% | 55% | 40% |
| 5 | 75% | 65% | 50% |
| 7 | 85% | 75% | 60% |
| 10 | 95% | 90% | 75% |

---

**End of Specification**
