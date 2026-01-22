# Procedural Room Generation System Specification

> **Template Version**: 1.0
> **Last Updated**: 2025-11-27
> **Status**: Active
> **Specification ID**: SPEC-SYSTEM-008

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
- **Design**: Room templates, hazard placement, puzzle generation
- **Balance**: Difficulty distribution, secret rooms
- **Implementation**: DungeonGenerator.cs, RoomPopulationService.cs
- **QA/Testing**: Generation validity, accessibility

---

## Executive Summary

### Purpose Statement
The Procedural Room Generation System creates varied dungeon layouts using template-based room generation, biome-specific features, hazard placement, and puzzle integration.

### Scope
**In Scope**:
- Room template system
- Procedural layout generation
- Hazard placement algorithms
- Secret room discovery
- Biome-specific variations
- Room connectivity rules
- Environmental object placement

**Out of Scope**:
- Vertical traversal mechanics → `SPEC-SYSTEM-003`
- Encounter generation → `SPEC-SYSTEM-007`
- Loot spawning → `SPEC-ECONOMY-001`
- Combat grid layout → `SPEC-COMBAT-001`

---

## Design Philosophy

### Design Pillars

1. **Exploration Rewards**
   - **Rationale**: Players should be rewarded for thorough exploration
   - **Examples**: Secret rooms with bonus loot

2. **Biome Identity**
   - **Rationale**: Each biome should feel distinct
   - **Examples**: Muspelheim = fire hazards, Niflheim = ice/cold

3. **Controlled Randomness**
   - **Rationale**: Random but fair layouts
   - **Examples**: Seeded generation for reproducibility

---

## Functional Requirements

### FR-001: Generate Dungeon Layout
**Priority**: Critical
**Status**: Implemented

**Generation Algorithm**:
```
1. Set random seed (from DungeonSeed or player ID)
2. Determine dungeon size (Milestone-based)
3. Place entrance room
4. Generate main path to boss
5. Branch side paths (exploration)
6. Place secret rooms
7. Apply biome theming
8. Validate connectivity
```

### FR-002: Select Room Templates
**Priority**: High
**Status**: Implemented

**Room Template Types**:
| Type | Description | Frequency |
|------|-------------|-----------|
| Combat | Standard enemy encounter | 40% |
| Puzzle | Environmental puzzle | 15% |
| Rest | Safe zone, healing | 10% |
| Loot | Treasure cache | 15% |
| Boss | Boss encounter | 1 per dungeon |
| Entrance | Starting room | 1 per dungeon |
| Secret | Hidden bonus room | 0-3 per dungeon |

### FR-003: Place Environmental Hazards
**Priority**: High
**Status**: Implemented

**Hazard Types by Biome**:
| Biome | Primary Hazard | Secondary Hazard |
|-------|----------------|------------------|
| Muspelheim | Fire vents | Lava pools |
| Niflheim | Ice patches | Cold aura |
| Jotunheim | Crushing terrain | Falling debris |
| Alfheim | Aetheric storms | Energy fields |
| The Roots | Decay pools | Structural collapse |

### FR-004: Generate Secret Rooms
**Priority**: Medium
**Status**: Implemented

**Secret Room Discovery**:
```
Discovery Methods:
- High WITS check (DC 15) when entering adjacent room
- Specific item/ability reveals secrets
- Environmental clue (crack in wall, draft)

Secret Room Contents:
- Guaranteed quality loot (Clan-Forged+)
- Unique crafting components
- Lore fragments
- Shortcut to later areas
```

### FR-005: Apply Biome Theming
**Priority**: High
**Status**: Implemented

**Biome Features**:

| Biome | Atmosphere | Floor Type | Common Objects |
|-------|------------|------------|----------------|
| Muspelheim | Smoky, orange glow | Scorched metal | Forges, vents |
| Niflheim | Freezing, blue mist | Frost-covered | Ice crystals, frost |
| Jotunheim | Industrial, dim | Metal grating | Machinery, corpses |
| Alfheim | Ethereal, shifting | Energy grid | Terminals, obelisks |
| The Roots | Decayed, dark | Rusted pipes | Fungi, debris |

---

## System Mechanics

### Mechanic 1: Seed-Based Generation

**Seeding System**:
```
DungeonSeed = Hash(PlayerID, DungeonsCompleted, Biome)

Benefits:
- Reproducible layouts for debugging
- Consistent secrets for guides/sharing
- Fair randomization

Room Generation:
  Random(seed + roomIndex) → room template
  Random(seed + roomIndex + hazardOffset) → hazard placement
```

### Mechanic 2: Room Connectivity

**Connectivity Rules**:
- Every room must be reachable from entrance
- Main path = shortest route entrance → boss
- Max 4 exits per room
- No dead ends on main path
- Secret rooms: exactly 1 hidden entrance

**Validation**:
```
After generation:
  1. BFS from entrance
  2. If any room unreachable: regenerate
  3. If boss unreachable: regenerate
  4. If path length < minimum: add rooms
```

### Mechanic 3: Room Population

**Population Pipeline**:
```
1. Select room template
2. Determine room purpose (combat, puzzle, rest)
3. Generate terrain features (cover, hazards)
4. Place environmental objects
5. If combat room: Generate encounter (→ EncounterService)
6. If loot room: Spawn loot containers (→ LootService)
7. If puzzle room: Generate puzzle (→ PuzzleService)
```

---

## State Management

### System State

**State Variables**:
| Variable | Type | Persistence | Default | Description |
|----------|------|-------------|---------|-------------|
| DungeonSeed | int | Permanent | 0 | Generation seed |
| DungeonsCompleted | int | Permanent | 0 | Seed modifier |
| ClearedRooms | List<string> | Permanent | [] | Visited rooms |
| DiscoveredSecrets | List<string> | Permanent | [] | Found secrets |

### Persistence Requirements

**Must Persist**:
- Dungeon seed (for regeneration)
- Cleared room list
- Secret room discovery state
- Current room ID

---

## Balance & Tuning

### Tunable Parameters

| Parameter | Location | Current Value | Impact |
|-----------|----------|---------------|--------|
| RoomsPerMilestone | DungeonGenerator | 5 | Dungeon size |
| SecretRoomChance | DungeonGenerator | 0.2 | Exploration reward |
| HazardDensity | RoomPopulation | 0.3 | Room danger |
| PuzzleFrequency | RoomPopulation | 0.15 | Puzzle rate |

### Balance Targets

**Target 1: Exploration Time**
- **Metric**: Rooms per dungeon exploration time
- **Target**: 20-40 minutes per dungeon run
- **Levers**: Room count, room complexity

**Target 2: Secret Discovery Rate**
- **Metric**: % of secrets found per run
- **Target**: 30-50% on first run, 80%+ with investment
- **Levers**: Discovery DC, visual clues

---

## Appendix

### Appendix A: Room Template Example

```json
{
  "templateId": "combat_standard_01",
  "type": "Combat",
  "size": "Medium",
  "exits": {
    "north": true,
    "south": true,
    "east": false,
    "west": true
  },
  "features": [
    { "type": "Cover", "position": [2, 3] },
    { "type": "Cover", "position": [4, 3] }
  ],
  "hazardSlots": [
    { "position": [3, 5], "allowedTypes": ["Fire", "Acid"] }
  ],
  "encounterBudget": "Standard"
}
```

### Appendix B: Dungeon Size by Milestone

| Milestone | Main Path | Side Rooms | Secret Rooms | Total |
|-----------|-----------|------------|--------------|-------|
| 0 | 4 | 2 | 0-1 | 6-7 |
| 1 | 5 | 3 | 0-2 | 8-10 |
| 2 | 6 | 4 | 1-2 | 11-12 |
| 3+ | 7 | 5 | 1-3 | 13-15 |

---

**End of Specification**
