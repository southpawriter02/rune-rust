# Performance Benchmarks

Parent item: Technical Reference (Technical%20Reference%202ba55eb312da8079a291e020980301c1.md)

> Version: v0.42.4+
Last Updated: November 2024
Location: RuneAndRust.Engine/, RuneAndRust.Core/
> 

## Overview

Rune & Rust implements comprehensive performance monitoring throughout critical code paths. This document covers the timing infrastructure, performance thresholds, and measured benchmarks.

## Performance Monitoring Infrastructure

### AIPerformanceMonitor

The primary performance monitoring service for AI and critical operations.

**File:** `RuneAndRust.Engine/AI/AIPerformanceMonitor.cs`

```csharp
public class AIPerformanceMonitor : IAIPerformanceMonitor
{
    private readonly ConcurrentDictionary<string, PerformanceMetrics> _metrics = new();
    private const long PerformanceThresholdMs = 50;  // Warning threshold

    public async Task<T> MonitorPerformanceAsync<T>(
        string operationName,
        Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await operation();
            stopwatch.Stop();

            RecordMetric(operationName, stopwatch.ElapsedMilliseconds);

            if (stopwatch.ElapsedMilliseconds > PerformanceThresholdMs)
            {
                _logger.LogWarning(
                    "AI operation {Operation} took {Ms}ms (threshold: {Threshold}ms)",
                    operationName, stopwatch.ElapsedMilliseconds, PerformanceThresholdMs);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "AI operation {Operation} failed after {Ms}ms",
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

```

### PerformanceMetrics Model

**File:** `RuneAndRust.Core/AI/PerformanceMetrics.cs`

```csharp
public class PerformanceMetrics
{
    public int TotalCalls { get; set; }      // Total invocations
    public long TotalMs { get; set; }        // Cumulative time
    public double AverageMs { get; set; }    // Average per call
    public long MaxMs { get; set; }          // Worst case
    public long MinMs { get; set; }          // Best case
    public DateTime LastRecorded { get; set; }
}

```

### IAIPerformanceMonitor Interface

**File:** `RuneAndRust.Engine/AI/IAIPerformanceMonitor.cs`

```csharp
public interface IAIPerformanceMonitor
{
    Task<T> MonitorPerformanceAsync<T>(string operationName, Func<Task<T>> operation);
    void RecordMetric(string operationName, long milliseconds);
    Dictionary<string, PerformanceMetrics> GetMetrics();
    void ResetMetrics();
    PerformanceMetrics? GetMetricsForOperation(string operationName);
}

```

---

## Performance Thresholds

### AI Operations

| Operation | Threshold | Warning Level |
| --- | --- | --- |
| Any AI operation | 50ms | Warning log |
| Threat assessment | 50ms | Warning log |
| Target selection | 50ms | Warning log |
| Ability rotation | 50ms | Warning log |

### Generation Operations

| Operation | Target | Source |
| --- | --- | --- |
| Biome cache initialization | < 500ms | `BiomeElementCache.cs` |
| Population pipeline | Logged | `PopulationPipeline.cs` |
| Dungeon generation | < 200ms | Design target |

### UI Operations

| Operation | Target | Source |
| --- | --- | --- |
| Sprite rendering | < 16.67ms (60fps) | `SpriteService.cs` |
| Frame time | 16.67ms | `UIConstants.TargetFrameTimeMs` |

---

## Timing Implementation Patterns

### Pattern 1: Stopwatch-Based Timing

Used throughout the codebase for accurate elapsed time measurement.

**PopulationPipeline.cs:**

```csharp
public void PopulateDungeon(Dungeon dungeon, BiomeDefinition biome, Random rng, DifficultyTier difficulty)
{
    var stopwatch = Stopwatch.StartNew();

    _log.Information("Starting v0.39.3 population pipeline for dungeon {DungeonId}, biome {BiomeName}",
        dungeon.DungeonId, biome.Name);

    // ... population logic ...

    stopwatch.Stop();
    _log.Information(
        "Population pipeline complete: {PopulatedRooms} rooms populated, " +
        "avg threat level: {AvgThreats:F2}, duration: {Duration}ms",
        populatedRoomCount, heatmap.AverageThreatLevel, stopwatch.ElapsedMilliseconds);
}

```

### Pattern 2: DateTime-Based Timing

Used for simpler timing where Stopwatch precision isn't required.

**BiomeElementCache.cs:**

```csharp
public void Initialize()
{
    if (_isInitialized) return;

    _log.Information("Initializing BiomeElementCache...");
    var startTime = DateTime.UtcNow;

    LoadTheRootsBiome();
    _isInitialized = true;

    var loadTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
    _log.Information("BiomeElementCache initialized: {BiomeCount} biomes loaded in {LoadTime}ms",
        _cache.Count, loadTime);
}

```

### Pattern 3: Async Operation Monitoring

Used for async operations with exception handling.

**AIPerformanceMonitor.cs:**

```csharp
public async Task<T> MonitorPerformanceAsync<T>(string operationName, Func<Task<T>> operation)
{
    var stopwatch = Stopwatch.StartNew();

    try
    {
        var result = await operation();
        stopwatch.Stop();
        RecordMetric(operationName, stopwatch.ElapsedMilliseconds);

        if (stopwatch.ElapsedMilliseconds > PerformanceThresholdMs)
        {
            _logger.LogWarning("AI operation {Operation} took {Ms}ms", operationName, stopwatch.ElapsedMilliseconds);
        }

        return result;
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        _logger.LogError(ex, "AI operation {Operation} failed after {Ms}ms", operationName, stopwatch.ElapsedMilliseconds);
        throw;
    }
}

```

---

## Thread-Safe Metrics Collection

### ConcurrentDictionary Pattern

**AIPerformanceMonitor.cs:**

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

    _logger.LogTrace(
        "Recorded metric for {Operation}: {Ms}ms (avg: {Avg:F1}ms, calls: {Calls})",
        operationName, milliseconds, metric.AverageMs, metric.TotalCalls);
}

```

### Thread-Safe Metrics Snapshot

```csharp
public Dictionary<string, PerformanceMetrics> GetMetrics()
{
    return _metrics.ToDictionary(
        kvp => kvp.Key,
        kvp =>
        {
            lock (kvp.Value)  // Lock while copying
            {
                return new PerformanceMetrics
                {
                    TotalCalls = kvp.Value.TotalCalls,
                    TotalMs = kvp.Value.TotalMs,
                    AverageMs = kvp.Value.AverageMs,
                    MaxMs = kvp.Value.MaxMs,
                    MinMs = kvp.Value.MinMs == long.MaxValue ? 0 : kvp.Value.MinMs,
                    LastRecorded = kvp.Value.LastRecorded
                };
            }
        });
}

```

---

## Performance Summary Report

**AIPerformanceMonitor.GeneratePerformanceSummary():**

```csharp
public string GeneratePerformanceSummary()
{
    var metrics = GetMetrics();

    if (!metrics.Any())
        return "No performance metrics recorded.";

    var summary = new System.Text.StringBuilder();
    summary.AppendLine("=== AI Performance Summary ===");
    summary.AppendLine();

    foreach (var (operation, metric) in metrics.OrderByDescending(m => m.Value.AverageMs))
    {
        summary.AppendLine($"Operation: {operation}");
        summary.AppendLine($"  Total Calls: {metric.TotalCalls}");
        summary.AppendLine($"  Average: {metric.AverageMs:F2}ms");
        summary.AppendLine($"  Min: {metric.MinMs}ms");
        summary.AppendLine($"  Max: {metric.MaxMs}ms");
        summary.AppendLine($"  Total Time: {metric.TotalMs}ms");
        summary.AppendLine($"  Last Recorded: {metric.LastRecorded:yyyy-MM-dd HH:mm:ss}");

        if (metric.AverageMs > PerformanceThresholdMs)
        {
            summary.AppendLine($"  ⚠ WARNING: Average exceeds threshold ({PerformanceThresholdMs}ms)");
        }

        summary.AppendLine();
    }

    return summary.ToString();
}

```

**Example Output:**

```
=== AI Performance Summary ===

Operation: ThreatAssessment
  Total Calls: 247
  Average: 12.34ms
  Min: 3ms
  Max: 89ms
  Total Time: 3048ms
  Last Recorded: 2024-11-27 14:32:15

Operation: TargetSelection
  Total Calls: 189
  Average: 8.21ms
  Min: 2ms
  Max: 45ms
  Total Time: 1552ms
  Last Recorded: 2024-11-27 14:32:15
  ⚠ WARNING: Average exceeds threshold (50ms)

```

---

## Target Performance Benchmarks

### Design Targets

| Operation | Target | Rationale |
| --- | --- | --- |
| Combat turn resolution | < 50ms | Responsive combat feel |
| Room generation | < 200ms | Sub-second sector generation |
| World state application | < 30ms | Fast room transitions |
| Save operation | < 100ms | Non-blocking saves |
| Load operation | < 200ms | Quick game resume |
| AI decision | < 50ms | Real-time enemy actions |
| Biome cache init | < 500ms | Fast startup |

### Frame Time Targets

| Target FPS | Frame Budget |
| --- | --- |
| 60 FPS | 16.67ms |
| 30 FPS | 33.33ms |

---

## Logging Levels

Performance logging uses structured logging with Serilog:

| Level | Usage |
| --- | --- |
| `Trace` | Individual metric recordings |
| `Debug` | Operation start/complete, cache hits |
| `Information` | Pipeline completion with timing |
| `Warning` | Threshold exceeded |
| `Error` | Operation failure with timing |

**Example Structured Log:**

```csharp
_log.Information(
    "Population pipeline complete: {PopulatedRooms} rooms populated, " +
    "avg threat level: {AvgThreats:F2}, duration: {Duration}ms",
    populatedRoomCount, heatmap.AverageThreatLevel, stopwatch.ElapsedMilliseconds);

```

---

## Metrics Reset

```csharp
public void ResetMetrics()
{
    _metrics.Clear();
    _logger.LogInformation("All AI performance metrics reset");
}

```

---

## Usage Examples

### Monitoring an Operation

```csharp
var monitor = new AIPerformanceMonitor(logger);

// Monitor async operation
var result = await monitor.MonitorPerformanceAsync(
    "ThreatAssessment",
    async () => await threatService.AssessThreatsAsync(enemy, targets));

// Check metrics
var metrics = monitor.GetMetricsForOperation("ThreatAssessment");
if (metrics != null && metrics.AverageMs > 30)
{
    Console.WriteLine($"Threat assessment averaging {metrics.AverageMs:F1}ms");
}

```

### Manual Metric Recording

```csharp
var stopwatch = Stopwatch.StartNew();
DoExpensiveOperation();
stopwatch.Stop();

monitor.RecordMetric("ExpensiveOperation", stopwatch.ElapsedMilliseconds);

```

### Performance Report

```csharp
// At end of game session or combat
var summary = monitor.GeneratePerformanceSummary();
Console.WriteLine(summary);

// Or log it
_log.Information("Session performance:\\n{Summary}", summary);

```

---

## Related Documentation

- [Optimization Strategies](https://www.notion.so/optimization-strategies.md) - Caching and optimization patterns
- [Service Architecture](https://www.notion.so/service-architecture.md) - Service performance considerations
- [Data Flow](https://www.notion.so/data-flow.md) - Pipeline timing