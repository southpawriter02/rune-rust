# Dungeon Generation Balance Tests (v0.10 Phase 9)

## Test Suite Overview

The `DungeonGenerationBalanceTests.cs` test suite validates generation quality, variety, and performance across multiple dungeons.

## Test Coverage

### 1. Template Variety Test

**Purpose:** Verify that all templates are used across multiple generations
**Method:** Generate 30 dungeons and track template usage
**Success Criteria:** >70% of templates used at least once
**What it validates:**

- Templates are not over-represented (avoiding repetition)
- Rare templates still appear occasionally
- Template selection algorithm is working correctly

### 2. Placeholder Validation Test

**Purpose:** Ensure no unreplaced placeholders in generated content
**Method:** Generate 10 dungeons and scan for `{Adjective}`, `{Detail}`, etc.
**Success Criteria:** Zero placeholders in room names or descriptions
**What it validates:**

- RoomInstantiator correctly replaces all placeholders
- Templates have sufficient descriptor lists
- Name/description generation is functioning

### 3. Connectivity Test

**Purpose:** Verify all rooms are reachable from start
**Method:** Generate 20 dungeons and BFS from start room
**Success Criteria:** 100% of rooms reachable in each dungeon
**What it validates:**

- Graph generation creates fully connected dungeons
- No orphaned rooms or disconnected subgraphs
- Boss room is always reachable

### 4. Room Count Variety Test

**Purpose:** Ensure dungeons have varied sizes
**Method:** Generate 30 dungeons and track room counts
**Success Criteria:** Range of 5-15 rooms with >2 room variance
**What it validates:**

- Biome parameters are respected (5-7 base rooms)
- Branching and secret rooms add appropriate variety
- No excessive dungeon sizes

### 5. Secret Room Generation Test

**Purpose:** Verify secret rooms appear probabilistically
**Method:** Generate 30 dungeons and count secret rooms
**Success Criteria:** Some dungeons have secrets, not all
**What it validates:**

- 20% secret room probability is working
- Secret rooms are correctly marked with NodeType.Secret
- Secret room templates are being used

### 6. Branching Path Test

**Purpose:** Verify branching paths appear probabilistically
**Method:** Generate 30 dungeons and count branch rooms
**Success Criteria:** ~40% of dungeons have branches
**What it validates:**

- 40% branching probability is working
- Branch rooms are correctly marked with NodeType.Branch
- Branching algorithm creates valid alternate paths

### 7. Performance Benchmark

**Purpose:** Ensure generation is fast
**Method:** Generate 100 dungeons and measure time
**Success Criteria:** <200ms average, <500ms max
**What it validates:**

- Generation algorithm is efficient
- No performance regressions
- System can generate dungeons on-demand during gameplay

### 8. Seed Reproducibility Test

**Purpose:** Verify same seed produces identical dungeons
**Method:** Generate same seed 10 times, compare output
**Success Criteria:** Identical room count, names, and structure
**What it validates:**

- Random number generation is deterministic
- Save/load will work correctly
- Dungeon regeneration is reliable

## Running the Tests

```bash
dotnet test --filter "FullyQualifiedName~DungeonGenerationBalanceTests" --logger "console;verbosity=detailed"

```

## Expected Results

All tests should pass. If failures occur:

- **Template variety failure:** Add more variety to template selection or increase template pool
- **Placeholder failure:** Check template descriptor lists are populated
- **Connectivity failure:** Bug in graph generation - investigate DungeonGenerator
- **Room count failure:** Adjust biome parameters or generation algorithm
- **Secret/branch failure:** Check probability calculations
- **Performance failure:** Optimize template selection or graph algorithms
- **Reproducibility failure:** Check Random seeding in all generation components

## Balance Adjustments

Based on test results, adjust:

1. **Biome parameters** (`Data/Biomes/the_roots.json`):
    - MinRoomCount / MaxRoomCount
    - BranchingProbability
    - SecretRoomProbability
2. **Template selection** (`DungeonGenerator.cs`):
    - Window size for recent template tracking
    - Template weighting algorithm
3. **Graph generation** (`DungeonGenerator.cs`):
    - Branch length (currently 1-2 rooms)
    - Secret room count (currently 0-1)
    - Main path length

## Performance Targets

- **Generation time:** <100ms for 5-7 room dungeon (v0.10 target)
- **Memory:** ~1KB per room
- **Throughput:** >10 dungeons/second

## Quality Metrics

- **Template diversity:** >70% templates used in 30 dungeons
- **Connectivity:** 100% rooms reachable
- **Variety:** 3+ room count variance across dungeons
- **Features:** 20% secret rooms, 40% branching paths

---

**Status:** Tests written, ready for execution when dotnet is available.