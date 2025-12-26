# Procedural Dungeon Generation

Type: Mechanic
Description: Algorithm for generating varied and interesting dungeon layouts with different themes (crypt, mine, ancient ruins, etc.). Should include placement of enemies, treasures, traps, and secrets based on difficulty level and player progress.
Priority: Must-Have
Status: Review
Target Version: Alpha
Dependencies: Asset library for dungeon components, Enemy spawning system
Implementation Difficulty: Hard
Balance Validated: No
Document ID: AAM-SPEC-MECH-PROCEDURALGEN-v5.0
Proof-of-Concept Flag: No
Template Compliance: v5.0 Three-Tier Template
Template Validated: No
Voice Validated: No

## I. Core Philosophy

The Procedural Room Generation System creates varied dungeon layouts using template-based room generation, biome-specific features, hazard placement, and puzzle integration. **Controlled randomness** ensures layouts feel hand-crafted while remaining replayable.

> Consolidated from SPEC-SYSTEM-008 (Imported Game Docs / codebase reflection).
> 

---

## II. Room Template System

### Room Types & Distribution

| Type | Description | Frequency |
| --- | --- | --- |
| **Combat** | Standard enemy encounter | 40% |
| **Puzzle** | Environmental puzzle | 15% |
| **Rest** | Safe zone, healing | 10% |
| **Loot** | Treasure cache | 15% |
| **Boss** | Boss encounter | 1 per dungeon |
| **Entrance** | Starting room | 1 per dungeon |
| **Secret** | Hidden bonus room | 0-3 per dungeon |

### Dungeon Size by Milestone

| Milestone | Main Path | Side Rooms | Secret Rooms | Total |
| --- | --- | --- | --- | --- |
| 0 | 4 | 2 | 0-1 | 6-7 |
| 1 | 5 | 3 | 0-2 | 8-10 |
| 2 | 6 | 4 | 1-2 | 11-12 |
| 3+ | 7 | 5 | 1-3 | 13-15 |

---

## III. Generation Algorithm

### Layout Generation Pipeline

```
1. Set random seed: Hash(PlayerID, DungeonsCompleted, Biome)
2. Determine dungeon size (Milestone-based)
3. Place entrance room
4. Generate main path to boss
5. Branch side paths (exploration)
6. Place secret rooms
7. Apply biome theming
8. Validate connectivity
```

### Room Connectivity Rules

- Every room must be reachable from entrance
- Main path = shortest route entrance → boss
- Max 4 exits per room
- No dead ends on main path
- Secret rooms: exactly 1 hidden entrance

### Validation

```
After generation:
  1. BFS from entrance
  2. If any room unreachable: regenerate
  3. If boss unreachable: regenerate
  4. If path length < minimum: add rooms
```

---

## IV. Biome Theming

### Hazard Types by Biome

| Biome | Atmosphere | Primary Hazard | Secondary Hazard |
| --- | --- | --- | --- |
| **Muspelheim** | Smoky, orange glow | Fire vents | Lava pools |
| **Niflheim** | Freezing, blue mist | Ice patches | Cold aura |
| **Jotunheim** | Industrial, dim | Crushing terrain | Falling debris |
| **Alfheim** | Ethereal, shifting | Aetheric storms | Energy fields |
| **The Roots** | Decayed, dark | Decay pools | Structural collapse |

---

## V. Secret Room System

### Discovery Methods

- High WITS check (DC 15) when entering adjacent room
- Specific item/ability reveals secrets
- Environmental clue (crack in wall, draft)

### Secret Room Contents

- Guaranteed quality loot (Clan-Forged+)
- Unique crafting components
- Lore fragments
- Shortcut to later areas

---

## VI. Room Population Pipeline

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

## VII. Database Schema

```sql
CREATE TABLE DungeonState (
    dungeon_id INTEGER PRIMARY KEY,
    seed INTEGER NOT NULL,
    biome TEXT NOT NULL,
    milestone INTEGER NOT NULL,
    rooms_json TEXT NOT NULL,
    cleared_rooms_json TEXT DEFAULT '[]',
    discovered_secrets_json TEXT DEFAULT '[]'
);

CREATE TABLE RoomTemplates (
    template_id TEXT PRIMARY KEY,
    type TEXT NOT NULL,
    size TEXT NOT NULL,
    exits_json TEXT NOT NULL,
    features_json TEXT NOT NULL,
    hazard_slots_json TEXT NOT NULL,
    encounter_budget TEXT
);
```

---

## VIII. Service Architecture

### DungeonGeneratorService

```csharp
public interface IDungeonGeneratorService
{
    Dungeon GenerateDungeon(int seed, string biome, int milestone);
    Room GetRoom(string roomId);
    List<string> GetAdjacentRooms(string roomId);
    bool TryDiscoverSecret(string roomId, int witsCheck);
    void MarkRoomCleared(string roomId);
}
```

---

## IX. Balance Parameters

| Parameter | Current Value | Impact |
| --- | --- | --- |
| RoomsPerMilestone | 5 | Dungeon size scaling |
| SecretRoomChance | 0.2 (20%) | Exploration reward frequency |
| HazardDensity | 0.3 (30%) | Environmental danger |
| PuzzleFrequency | 0.15 (15%) | Non-combat content rate |
| SecretDiscoveryDC | 15 | WITS investment reward |

### Balance Targets

- **Exploration Time:** 20-40 minutes per dungeon run
- **Secret Discovery:** 30-50% on first run, 80%+ with WITS investment

---

## X. Integration Points

**Dependencies:**

- BiomeLibrary → biome theming data
- EncounterService → combat room population
- LootService → treasure placement
- PuzzleService → puzzle generation

**Referenced By:**

- Navigation commands → room traversal
- Save/Load System → dungeon state persistence
- Quest System → dungeon-specific objectives

---

*Consolidated from SPEC-SYSTEM-008 (Procedural Room Generation System Specification) per Source Authority guidelines.*