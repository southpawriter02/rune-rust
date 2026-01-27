# Optimization Strategies

Parent item: Technical Reference (Technical%20Reference%202ba55eb312da8079a291e020980301c1.md)

> Version: v0.42.4+
Last Updated: November 2024
Location: RuneAndRust.Engine/, RuneAndRust.Core/
> 

## Overview

Rune & Rust employs various optimization strategies to maintain performance across procedural generation, combat, AI, and UI systems. This document covers the caching mechanisms, lazy loading patterns, batch operations, and memory optimizations used throughout the codebase.

## Caching Strategies

### 1. Lazy Singleton Cache (BiomeElementCache)

**Purpose:** Eliminate database lookups during dungeon generation.
**Target:** Sub-500ms generation time.

**File:** `RuneAndRust.Engine/BiomeElementCache.cs`

```csharp
public class BiomeElementCache
{
    private static readonly Lazy<BiomeElementCache> _instance =
        new Lazy<BiomeElementCache>(() => new BiomeElementCache());

    private Dictionary<string, BiomeElementTable> _cache = new Dictionary<string, BiomeElementTable>();
    private bool _isInitialized = false;

    private BiomeElementCache()
    {
        // Private constructor for singleton
    }

    public static BiomeElementCache Instance => _instance.Value;

    public void Initialize()
    {
        if (_isInitialized)
        {
            _log.Warning("BiomeElementCache already initialized, skipping");
            return;
        }

        _log.Information("Initializing BiomeElementCache...");
        var startTime = DateTime.UtcNow;

        LoadTheRootsBiome();  // Load all biome elements into memory
        _isInitialized = true;

        var loadTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _log.Information("BiomeElementCache initialized: {BiomeCount} biomes loaded in {LoadTime}ms",
            _cache.Count, loadTime);
    }

    public BiomeElementTable? GetBiome(string biomeId)
    {
        if (!_isInitialized)
        {
            _log.Warning("BiomeElementCache not initialized, calling Initialize()");
            Initialize();  // Lazy initialization on first access
        }

        if (_cache.TryGetValue(biomeId, out var table))
        {
            return table;
        }

        _log.Error("Biome not found in cache: {BiomeId}", biomeId);
        return null;
    }

    public void Clear()
    {
        _cache.Clear();
        _isInitialized = false;
        _log.Information("BiomeElementCache cleared");
    }
}

```

**Benefits:**

- Thread-safe lazy initialization via `Lazy<T>`
- Single load at startup, O(1) access during generation
- 25+ biome elements cached per biome
- Can be cleared for hot-reload scenarios

---

### 2. MemoryCache with TTL (TerritoryService)

**Purpose:** Cache frequently accessed sector control data.
**TTL:** 5 minutes.

**File:** `RuneAndRust.Engine/TerritoryService.cs`

```csharp
public class TerritoryService
{
    private readonly MemoryCache _sectorControlCache;
    private const int CACHE_DURATION_SECONDS = 300; // 5 minutes

    public TerritoryService(...)
    {
        _sectorControlCache = new MemoryCache(new MemoryCacheOptions());
        _log.Debug("TerritoryService initialized with caching enabled");
    }

    // Cache invalidation on state changes
    private void InvalidateSectorCache(int sectorId)
    {
        _sectorControlCache.Remove($"control_state_{sectorId}");
    }
}

```

**Benefits:**

- Reduces database queries for control state
- Automatic expiration after 5 minutes
- Explicit invalidation on state changes

---

### 3. Static Dictionary Cache (CommandParser)

**Purpose:** O(1) command type lookup.

**File:** `RuneAndRust.Engine/CommandParser.cs`

```csharp
private static readonly Dictionary<string, CommandType> CommandAliases = new()
{
    { "look", CommandType.Look },
    { "l", CommandType.Look },
    { "examine", CommandType.Look },
    { "investigate", CommandType.Investigate },
    { "inspect", CommandType.Investigate },
    { "attack", CommandType.Attack },
    { "a", CommandType.Attack },
    { "hit", CommandType.Attack },
    { "strike", CommandType.Attack },
    // ... 100+ command aliases ...
};

```

**Benefits:**

- Single allocation at class load
- O(1) lookup for command parsing
- No runtime dictionary building

---

### 4. Static Lookup Tables (GaldrFlavorTextService)

**Purpose:** Pre-allocated magic system constants.

**File:** `RuneAndRust.Engine/GaldrFlavorTextService.cs`

```csharp
private static readonly Dictionary<string, string> _runeSymbols = new()
{
    {"Fehu", "ᚠ"},
    {"Uruz", "ᚢ"},
    {"Thurisaz", "ᚦ"},
    {"Ansuz", "ᚨ"},
    // ... Elder Futhark runes ...
};

private static readonly Dictionary<string, string> _magicColors = new()
{
    {"Fehu", "crimson"},
    {"Thurisaz", "frost-white"},
    {"Algiz", "golden"},
    // ... magic color palette ...
};

```

**Benefits:**

- Constant-time rune and color lookups
- No repeated string allocations
- Shared across all service instances

---

## Concurrent Collections

### 1. ConcurrentDictionary for Metrics (AIPerformanceMonitor)

**Purpose:** Thread-safe metrics collection.

**File:** `RuneAndRust.Engine/AI/AIPerformanceMonitor.cs`

```csharp
private readonly ConcurrentDictionary<string, PerformanceMetrics> _metrics = new();

public void RecordMetric(string operationName, long milliseconds)
{
    var metric = _metrics.GetOrAdd(operationName, _ => new PerformanceMetrics
    {
        MinMs = long.MaxValue
    });

    lock (metric)  // Per-metric locking for atomic updates
    {
        metric.TotalCalls++;
        metric.TotalMs += milliseconds;
        metric.MaxMs = Math.Max(metric.MaxMs, milliseconds);
        metric.MinMs = Math.Min(metric.MinMs, milliseconds);
        metric.AverageMs = (double)metric.TotalMs / metric.TotalCalls;
        metric.LastRecorded = DateTime.UtcNow;
    }
}

```

**Pattern:**

- `GetOrAdd` for lock-free metric registration
- Per-object locking for atomic metric updates
- No global lock contention

---

### 2. ConcurrentDictionary for Decision Logs (AIDebugService)

**Purpose:** Thread-safe AI decision logging.

**File:** `RuneAndRust.Engine/AI/AIDebugService.cs`

```csharp
private readonly ConcurrentDictionary<Guid, List<DecisionLog>> _decisionLogs = new();

```

**Benefits:**

- Concurrent AI systems can log decisions
- No lock contention between different encounters

---

## Lazy Loading Patterns

### 1. Lazy<T> Singleton

**Purpose:** Deferred instantiation until first use.

```csharp
// BiomeElementCache.cs
private static readonly Lazy<BiomeElementCache> _instance =
    new Lazy<BiomeElementCache>(() => new BiomeElementCache());

public static BiomeElementCache Instance => _instance.Value;

```

**Benefits:**

- Thread-safe without explicit locking
- No initialization cost until needed
- Single instance guaranteed

### 2. On-Demand Initialization

**Purpose:** Initialize only when accessed.

```csharp
public BiomeElementTable? GetBiome(string biomeId)
{
    if (!_isInitialized)
    {
        Initialize();  // First access triggers load
    }
    return _cache.TryGetValue(biomeId, out var table) ? table : null;
}

```

---

## Batch Operations

### 1. Bulk Room Population (PopulationPipeline)

**Purpose:** Allocate budgets to all rooms before population.

**File:** `RuneAndRust.Engine/PopulationPipeline.cs`

```csharp
public void PopulateDungeon(Dungeon dungeon, BiomeDefinition biome, Random rng, DifficultyTier difficulty)
{
    var rooms = dungeon.Rooms.Values.Where(r => !r.IsHandcrafted).ToList();

    // Step 1: Calculate global budget (one calculation)
    var globalBudget = _densityService.CalculateGlobalBudget(
        rooms.Count, difficulty, biome.BiomeId);

    // Step 2: Classify all rooms at once
    var densityMap = _classificationService.ClassifyRooms(rooms, rng);

    // Step 3: Distribute budget across all rooms
    var populationPlan = _distributionService.DistributeBudget(
        globalBudget, densityMap, rng);

    // Step 4: Apply allocations in batch
    foreach (var room in rooms)
    {
        if (populationPlan.RoomAllocations.TryGetValue(room.RoomId, out var allocation))
        {
            room.DensityClassification = allocation.Density;
            room.AllocatedEnemyBudget = allocation.AllocatedEnemies;
            room.AllocatedHazardBudget = allocation.AllocatedHazards;
            room.AllocatedLootBudget = allocation.AllocatedLoot;
        }
    }

    // Step 5: Populate all rooms
    foreach (var room in dungeon.Rooms.Values.Where(r => !r.IsHandcrafted))
    {
        PopulateRoom(room, biome, rng);
    }
}

```

**Benefits:**

- Single budget calculation instead of per-room
- Batch classification for consistent density distribution
- Predictable threat heatmap generation

### 2. Bulk List Operations (SaveRepository)

**Purpose:** Batch-load items into rooms.

**File:** `RuneAndRust.Persistence/SaveRepository.cs`

```csharp
public void RestoreRoomItems(Dictionary<string, Room> rooms, string roomItemsJson)
{
    var roomItemsDict = JsonSerializer.Deserialize<Dictionary<int, List<Equipment>>>(roomItemsJson);

    foreach (var kvp in roomItemsDict)
    {
        var room = rooms.Values.FirstOrDefault(r => r.Id == kvp.Key);
        if (room != null)
        {
            room.ItemsOnGround.Clear();
            room.ItemsOnGround.AddRange(kvp.Value);  // Bulk addition
        }
    }
}

```

**Benefits:**

- Single `AddRange` instead of multiple `Add` calls
- Reduces list resizing operations

---

## Memory Optimization

### 1. StringBuilder for Complex Strings

**Purpose:** Efficient string concatenation.

**File:** `RuneAndRust.Engine/Commands/InventoryCommand.cs`

```csharp
var output = new StringBuilder();

output.AppendLine("╔════════════════════════════════════════╗");
output.AppendLine("║ Your Inventory                         ║");
output.AppendLine($"║ Capacity: {currentSize}/{maxSize} ({capacityStatus})               ║");
output.AppendLine("╠════════════════════════════════════════╣");

foreach (var weapon in weapons)
{
    string weaponInfo = FormatWeaponInfo(weapon);
    output.AppendLine($"║  - {weaponInfo.PadRight(36)} ║");
}

return output.ToString();

```

**Also used in:**

- `AIPerformanceMonitor.GeneratePerformanceSummary()`
- Various UI rendering methods

**Benefits:**

- Avoids O(n²) string concatenation
- Single final allocation with `ToString()`

### 2. Struct Value Types (RoomPosition)

**Purpose:** Stack allocation for spatial coordinates.

**File:** `RuneAndRust.Core/Spatial/RoomPosition.cs`

```csharp
public struct RoomPosition : IEquatable<RoomPosition>
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    public static readonly RoomPosition Origin = new RoomPosition(0, 0, 0);

    public RoomPosition(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);  // Efficient combined hash
    }

    public bool Equals(RoomPosition other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }
}

```

**Benefits:**

- Stack allocation, no GC pressure
- Efficient copying (value semantics)
- Implements `IEquatable<T>` for fast equality checks

### 3. Static Readonly Collections

**Purpose:** Shared immutable data across instances.

**Pattern used throughout:**

```csharp
// Logger instances (shared per type)
private static readonly ILogger _log = Log.ForContext<ServiceName>();

// Lookup tables
private static readonly Dictionary<string, CommandType> CommandAliases = new() { ... };

// Default values
public static readonly RoomPosition Origin = new RoomPosition(0, 0, 0);

```

**Benefits:**

- Single allocation per application lifetime
- No per-instance memory overhead
- Thread-safe for read operations

---

## Database Query Optimization

### 1. Parameterized Queries

**Purpose:** Prevent SQL injection, enable query plan caching.

```csharp
command.CommandText = "SELECT * FROM saves WHERE character_name = $name";
command.Parameters.AddWithValue("$name", characterName);

```

### 2. INSERT OR REPLACE

**Purpose:** Efficient upsert without separate SELECT.

```csharp
command.CommandText = @"
    INSERT OR REPLACE INTO saves (
        character_name, class, current_hp, max_hp, last_saved
    ) VALUES (
        $name, $class, $hp, $maxhp, $saved
    )
";

```

### 3. Index Usage

All frequently-queried columns are indexed:

```sql
CREATE INDEX idx_world_state_save_sector ON world_state_changes(save_id, sector_seed);
CREATE INDEX idx_world_state_save_room ON world_state_changes(save_id, room_id);
CREATE INDEX idx_abilities_specialization ON Abilities(SpecializationID);

```

---

## Optimization Summary

| Strategy | Location | Benefit |
| --- | --- | --- |
| **Lazy<T> Singleton** | BiomeElementCache | Deferred init, thread-safe |
| **MemoryCache + TTL** | TerritoryService | Reduced DB queries |
| **Static Dictionary** | CommandParser | O(1) command lookup |
| **ConcurrentDictionary** | AIPerformanceMonitor | Thread-safe metrics |
| **Batch Population** | PopulationPipeline | Single budget calculation |
| **AddRange** | SaveRepository | Bulk list operations |
| **StringBuilder** | UI Commands | Efficient string building |
| **Struct Types** | RoomPosition | Stack allocation |
| **Static readonly** | All services | Shared immutable data |
| **Parameterized SQL** | Repositories | Query plan caching |

---

## When to Apply Each Strategy

### Use Lazy<T> Singleton When:

- Expensive one-time initialization
- Thread-safe singleton required
- May not be needed in all code paths

### Use MemoryCache When:

- Data changes occasionally
- TTL-based invalidation is acceptable
- External data source (DB, API)

### Use Static Dictionary When:

- Data is constant at compile time
- Frequent lookups required
- No runtime modification needed

### Use ConcurrentDictionary When:

- Multiple threads read/write
- Per-key operations (not global transactions)
- Lock-free read performance critical

### Use Batch Operations When:

- Processing collections of items
- Shared calculations apply to multiple items
- Reducing per-item overhead matters

### Use StringBuilder When:

- Building strings in loops
- Complex formatted output
- String length > 10 concatenations

### Use Struct When:

- Small data (< 16 bytes ideal)
- Value semantics desired
- High allocation frequency

---

## Related Documentation

- [Performance Benchmarks](https://www.notion.so/performance-benchmarks.md) - Timing thresholds and metrics
- [Database Schema](https://www.notion.so/database-schema.md) - Index definitions
- [Data Access Patterns](https://www.notion.so/data-access-patterns.md) - Query optimization