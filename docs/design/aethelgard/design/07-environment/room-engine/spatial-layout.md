---
id: SPEC-ROOMENGINE-SPATIAL
title: "Spatial Layout — 3D Coordinate System"
version: 1.0
status: draft
last-updated: 2025-12-07
related-files:
  - path: "data/schemas/v0.39.1_spatial_layout_schema.sql"
    status: Active
  - path: "RuneAndRust.Engine/SpatialLayoutService.cs"
    status: Planned
---

# Spatial Layout — 3D Coordinate System

---

## 1. Overview

The spatial layout system transforms abstract graph nodes into **3D coordinate space**, enabling:
- Vertical exploration (stairs, shafts, elevators)
- Spatial queries ("what's above this room?")
- Multi-level dungeon structures
- Tactical positioning in combat

**Key Components:**
- 3D coordinate system (X, Y, Z)
- 7 vertical layers (-3 to +3)
- 5 vertical connection types
- Traversal mechanics with skill checks

---

## 2. Coordinate System

### 2.1 Axis Definitions

```csharp
public struct RoomPosition
{
    public int X { get; set; }  // East (+) / West (-)
    public int Y { get; set; }  // North (+) / South (-)
    public int Z { get; set; }  // Up (+) / Down (-)
    
    public static readonly RoomPosition Origin = new(0, 0, 0);
}
```

| Axis | Positive | Negative | Notes |
|------|----------|----------|-------|
| **X** | East | West | Horizontal |
| **Y** | North | South | Horizontal |
| **Z** | Up (+3) | Down (-3) | Vertical |

**Origin:** Entry Hall always at `(0, 0, 0)`

### 2.2 Room Footprints

| Room Size | Units | Typical Dimensions |
|-----------|-------|-------------------|
| Small | 1×1×1 | 10m × 10m × 3m |
| Medium | 2×2×1 | 20m × 20m × 3m |
| Large | 3×3×1 | 30m × 30m × 5m |
| Vertical Shaft | 1×1×N | Spans multiple Z |

---

## 3. Vertical Layers

### 3.1 Layer Definitions

```csharp
public enum VerticalLayer
{
    DeepRoots   = -3,  // -300m: Ancient deep infrastructure
    LowerRoots  = -2,  // -200m: Maintenance tunnels
    UpperRoots  = -1,  // -100m: Lower facility levels
    GroundLevel =  0,  // Origin: Entry level
    LowerTrunk  = +1,  // +100m: Mid-facility
    UpperTrunk  = +2,  // +200m: Upper facility
    Canopy      = +3   // +300m: Surface exposure
}
```

### 3.2 Layer Characteristics

| Layer | Z | Depth | Characteristics |
|-------|---|-------|-----------------|
| DeepRoots | -3 | -300m | Ancient, heavily decayed, rare |
| LowerRoots | -2 | -200m | Geothermal, steam vents, hot |
| UpperRoots | -1 | -100m | Cooling systems, frozen zones |
| GroundLevel | 0 | 0m | Entry point, most common |
| LowerTrunk | +1 | +100m | Industrial, Aetheric resonance |
| UpperTrunk | +2 | +200m | High Aetheric energy |
| Canopy | +3 | +300m | Exposed to ash-filled sky |

> [!IMPORTANT]
> **Layers ≠ Biomes.** Biomes are assigned separately and can span 2-4 Z-levels within a sector. 
> A sector might have The Roots (Z=0 to Z=-1) transitioning to Muspelheim (Z=-2 to Z=-3).
> See [Biome System](../biomes/biomes-overview.md) for adjacency rules and transitions.

---

## 4. Vertical Connection Types

### 4.1 Connection Definitions

```csharp
public enum VerticalConnectionType
{
    Stairs,     // Free traversal, 1-2 levels
    Shaft,      // Climbing required, 2-4 levels
    Elevator,   // Mechanical, any distance
    Ladder,     // Climbing route, 1-3 levels
    Collapsed   // Blocked, requires clearing
}
```

### 4.2 Traversal Mechanics

| Type | DC | Levels | On Failure | Notes |
|------|-----|--------|------------|-------|
| **Stairs** | — | 1-2 | — | Free traversal |
| **Shaft** | MIGHT 12 | 2-4 | 1d6-2d6 fall damage | Climbing required |
| **Elevator** | — or WITS 15 | Any | Must climb shaft | May need repair |
| **Ladder** | MIGHT 10 | 1-3 | 1d4 damage | Safer than shaft |
| **Collapsed** | MIGHT 15 | — | Blocked | 10 min to clear |

### 4.3 Failure Consequences

**Shaft Failure:**
- Margin 1-5: 1d6 Physical damage (slip)
- Margin 6+: 2d6 Physical damage + fall to bottom

**Ladder Failure:**
- Any failure: 1d4 Physical damage

---

## 5. Data Schema

### 5.1 Room Coordinates

```sql
ALTER TABLE Rooms ADD COLUMN coord_x INTEGER DEFAULT 0;
ALTER TABLE Rooms ADD COLUMN coord_y INTEGER DEFAULT 0;
ALTER TABLE Rooms ADD COLUMN coord_z INTEGER DEFAULT 0;
ALTER TABLE Rooms ADD COLUMN vertical_layer TEXT DEFAULT 'GroundLevel';

CREATE INDEX idx_rooms_position ON Rooms(coord_x, coord_y, coord_z);
CREATE INDEX idx_rooms_layer ON Rooms(vertical_layer);
```

### 5.2 Vertical Connections

```sql
CREATE TABLE Vertical_Connections (
    connection_id INTEGER PRIMARY KEY,
    from_room_id TEXT NOT NULL,
    to_room_id TEXT NOT NULL,
    connection_type TEXT NOT NULL,    -- Stairs/Shaft/Elevator/Ladder/Collapsed
    traversal_dc INTEGER DEFAULT 0,
    is_blocked BOOLEAN DEFAULT 0,
    blockage_description TEXT,
    levels_spanned INTEGER DEFAULT 1,
    description TEXT,
    hazards TEXT,                     -- JSON array
    is_bidirectional BOOLEAN DEFAULT 1,
    
    CHECK (connection_type IN ('Stairs','Shaft','Elevator','Ladder','Collapsed')),
    CHECK (traversal_dc >= 0 AND traversal_dc <= 25),
    CHECK (levels_spanned >= 1 AND levels_spanned <= 6)
);
```

**Full schema:** [v0.39.1_spatial_layout_schema.sql](../../../data/schemas/v0.39.1_spatial_layout_schema.sql)

---

## 6. Layout Algorithm

### 6.1 Graph to 3D Conversion

```
┌─────────────────────────────────────────────────────────────┐
│ INPUT: DungeonGraph + Seed                                  │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 1: Place Entry Hall at Origin (0, 0, 0)               │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 2: BFS Traversal                                       │
│   • For each edge, calculate new position from direction    │
│   • 30% chance to change Z level                           │
│   • Validate no overlaps                                    │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 3: Add Vertical Connections                            │
│   • Find rooms with same X,Y but different Z               │
│   • Generate connection type based on Z difference          │
│   • 10% chance of blocked passage                          │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 4: Validate Spatial Coherence                          │
│   • No overlapping positions                                │
│   • All rooms reachable from origin                         │
│   • Valid layer bounds (-3 to +3)                          │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ OUTPUT: Sector with positioned rooms + vertical connections │
└─────────────────────────────────────────────────────────────┘
```

### 6.2 Direction to Offset Mapping

```csharp
var newPosition = direction switch
{
    Direction.North => new RoomPosition(current.X, current.Y + 1, current.Z),
    Direction.South => new RoomPosition(current.X, current.Y - 1, current.Z),
    Direction.East  => new RoomPosition(current.X + 1, current.Y, current.Z),
    Direction.West  => new RoomPosition(current.X - 1, current.Y, current.Z),
    Direction.Up    => new RoomPosition(current.X, current.Y, current.Z + 1),
    Direction.Down  => new RoomPosition(current.X, current.Y, current.Z - 1),
    _ => current
};
```

### 6.3 Vertical Bias Rules

| Node Type | Z Tendency | Rationale |
|-----------|------------|-----------|
| Main Path | Descend (-1, 0) | Deeper = more danger |
| Boss | Deeper (-2 to 0) | Final challenge at depth |
| Secret | Any (-2 to +1) | Hidden anywhere |
| Branch | Same or ±1 | Side exploration |

---

## 7. Service Interfaces

### 7.1 SpatialLayoutService

```csharp
public interface ISpatialLayoutService
{
    Sector ConvertGraphTo3DLayout(DungeonGraph graph, int seed);
    bool ValidateNoOverlaps(Sector sector);
    RoomPosition GetRoomPosition(string roomId);
    IReadOnlyList<Room> GetRoomsAtLayer(VerticalLayer layer);
    IReadOnlyList<Room> GetRoomsInRange(RoomPosition center, int radius);
}
```

### 7.2 VerticalTraversalService

```csharp
public interface IVerticalTraversalService
{
    VerticalConnection? GetConnectionBetween(string fromRoomId, string toRoomId);
    bool CanTraverse(Character character, VerticalConnection connection);
    TraversalResult AttemptTraversal(Character character, VerticalConnection connection);
    IReadOnlyList<VerticalLayer> GetReachableLayers(string startRoomId);
}

public record TraversalResult(
    bool Success,
    string Message,
    int Damage = 0
);
```

### 7.3 SpatialValidationService

```csharp
public interface ISpatialValidationService
{
    IReadOnlyList<ValidationIssue> ValidateSector(Sector sector);
    bool IsReachableFromOrigin(Room room, Sector sector);
}

public record ValidationIssue(
    string Type,       // Overlap, Unreachable, MissingConnection, LayerBounds
    string Severity,   // Warning, Error, Critical
    string Description,
    IReadOnlyList<string> AffectedRoomIds
);
```

---

## 8. Example: Multi-Level Sector

```
SECTOR LAYOUT (Side View - X=0 slice)

Z=+1  LowerTrunk    ┌─────────────┐
                    │ Observation │
                    │  Platform   │
                    └──────↑──────┘
                           │ Ladder (DC 10)
Z=0   GroundLevel   ┌──────┴──────┐     ┌─────────────┐
                    │  Entry Hall │ ←── │  Corridor   │
                    └──────┬──────┘     └─────────────┘
                           │ Stairs (free)
Z=-1  UpperRoots    ┌──────┴──────┐
                    │ Geothermal  │
                    │   Chamber   │
                    └──────┬──────┘
                           │ Shaft (DC 12)
Z=-2  LowerRoots    ┌──────┴──────┐
                    │ Boss Arena  │
                    └─────────────┘
```

---

## 9. Phased Implementation Guide

### Phase 1: Data Schema
- [ ] **DB**: Apply `v0.39.1_spatial_layout_schema.sql` to PostgreSQL.
- [ ] **DTOs**: Define `RoomPosition` record and `VerticalLayer` enum.
- [ ] **Entities**: Update `Room` entity to include X, Y, Z, Layer properties.

### Phase 2: Core Algorithm
- [ ] **Service**: Implement `ISpatialLayoutService`.
- [ ] **Mapping**: Implement `GraphTo3D` logic with BFS.
- [ ] **Validation**: Implement `ValidateNoOverlaps` collision detection.

### Phase 3: Verticality
- [ ] **Connections**: Implement logic to create `VerticalConnection` between rooms.
- [ ] **Traversal**: Implement `IVerticalTraversalService` (Skill checks, fall damage).
- [ ] **Query**: Implement `GetRoomsInRange` and `GetRoomsAtLayer`.

### Phase 4: Integration
- [ ] **Engine**: Wire into `DungeonGenerator.Generate()`.
- [ ] **CLI**: Add `--layout3d` flag to `Generate` command to output Z-levels.

---

## 10. Testing Requirements

### 10.1 Unit Tests
- [ ] **Coordinate**: (0,0,0) + North -> (0,1,0).
- [ ] **Overlap**: Placing room at (0,1,0) when occupied -> Throws/Returns checks.
- [ ] **Traversal**: Shaft (DC 12) roll 10 -> Fail -> Damage applied.
- [ ] **Bounds**: Assigning Z=+4 -> Clamps to +3 or throws.

### 10.2 Integration Tests
- [ ] **Generation**: Generate 100 layouts, assert 0 overlaps.
- [ ] **Connectivity**: Assert all Z-levels are reachable from Entry Hall.
- [ ] **Persistence**: Save/Load layout -> coordinates preserved in DB.

### 10.3 Manual QA
- [ ] **Visual**: Use debug renderer to view 3D structure (side profile).
- [ ] **Command**: `/traverse up` works only if connection exists.

---

## 11. Logging Requirements

**Reference:** [logging.md](../../00-project/logging.md)

### 11.1 Log Events
| Event | Level | Message Template | Properties |
|-------|-------|------------------|------------|
| Placement | Verbose | "Placed Room {Id} at ({X}, {Y}, {Z})." | `Id`, `X`, `Y`, `Z` |
| Overlap | Warn | "Overlap detected at ({X}, {Y}, {Z}). Retrying..." | `X`, `Y`, `Z` |
| Vertical | Info | "Created {Type} connection between {RoomA} and {RoomB}." | `Type`, `RoomA`, `RoomB` |
| Fall | Info | "{Character} fell down {ShaftID}. Damage: {Damage}." | `Character`, `ShaftID`, `Damage` |

---

## 12. Related Documentation
| Document | Purpose |
|----------|---------|
| [Room Engine Core](core.md) | Generation pipeline |
| [Biomes](../biomes/biomes-overview.md) | Layer biome assignment |
| [The Deep](../the-deep.md) | Depth stratification schema |
| [Inter-Realm Navigation](../navigation.md) | Route classification; continental travel |
| [Attributes](../../01-core/attributes/might.md) | Climbing checks |

---

## 13. Changelog
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-07 | Initial specification |
| 1.1 | 2025-12-14 | Standardized with Phased Guide, Testing, and Logging |
