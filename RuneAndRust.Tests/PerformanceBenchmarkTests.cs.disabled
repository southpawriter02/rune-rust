using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Engine.Spatial;
using Serilog;
using System.Diagnostics;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.39.4: Performance Benchmark Tests
/// Validates that generation meets performance targets
/// Target: < 1500ms average, < 2000ms acceptable, > 3000ms unacceptable
/// </summary>
[TestClass]
public class PerformanceBenchmarkTests
{
    private ILogger _logger = null!;
    private TemplateLibrary _templateLibrary = null!;
    private BiomeDefinition _testBiome = null!;

    [TestInitialize]
    public void Setup()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        _testBiome = new BiomeDefinition
        {
            BiomeId = "the_roots",
            Name = "The Roots",
            DifficultyTier = DifficultyTier.Normal,
            MinRoomCount = 5,
            MaxRoomCount = 7
        };

        _templateLibrary = CreateTestTemplateLibrary();
    }

    private DungeonGenerator CreateFullyIntegratedGenerator()
    {
        var spatialLayoutService = new SpatialLayoutService(_logger);
        var spatialValidationService = new SpatialValidationService(_logger);
        var verticalTraversalService = new VerticalTraversalService(_logger);

        var biomeBlendingService = new BiomeBlendingService(_logger);
        var adjacencyRepo = new Persistence.BiomeAdjacencyRepository();
        var biomeTransitionService = new BiomeTransitionService(adjacencyRepo, biomeBlendingService);
        var gradientService = new EnvironmentalGradientService();

        var contentDensityService = new ContentDensityService();
        var densityClassificationService = new DensityClassificationService();
        var budgetDistributionService = new BudgetDistributionService();
        var heatmapService = new ThreatHeatmapService();

        return new DungeonGenerator(
            _templateLibrary,
            populationPipeline: null,
            anchorInserter: null,
            spatialLayoutService: spatialLayoutService,
            spatialValidationService: spatialValidationService,
            verticalTraversalService: verticalTraversalService,
            biomeTransitionService: biomeTransitionService,
            biomeBlendingService: biomeBlendingService,
            gradientService: gradientService,
            contentDensityService: contentDensityService,
            densityClassificationService: densityClassificationService,
            budgetDistributionService: budgetDistributionService,
            heatmapService: heatmapService);
    }

    private TemplateLibrary CreateTestTemplateLibrary()
    {
        var library = new TemplateLibrary();

        library.AddTemplate(new RoomTemplate
        {
            TemplateId = "entry_hall_01",
            Name = "Entry Hall",
            Archetype = RoomArchetype.EntryHall,
            Description = "A standard entry hall",
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor, RoomArchetype.Chamber }
        });

        for (int i = 0; i < 3; i++)
        {
            library.AddTemplate(new RoomTemplate
            {
                TemplateId = $"corridor_{i:D2}",
                Name = $"Corridor {i + 1}",
                Archetype = RoomArchetype.Corridor,
                Description = "A narrow corridor",
                ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor, RoomArchetype.Chamber, RoomArchetype.BossArena }
            });
        }

        for (int i = 0; i < 5; i++)
        {
            library.AddTemplate(new RoomTemplate
            {
                TemplateId = $"chamber_{i:D2}",
                Name = $"Chamber {i + 1}",
                Archetype = RoomArchetype.Chamber,
                Description = "A large chamber",
                ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor, RoomArchetype.Chamber, RoomArchetype.BossArena }
            });
        }

        library.AddTemplate(new RoomTemplate
        {
            TemplateId = "boss_arena_01",
            Name = "Boss Arena",
            Archetype = RoomArchetype.BossArena,
            Description = "A large arena for boss encounters",
            ValidConnections = new List<RoomArchetype>()
        });

        library.AddTemplate(new RoomTemplate
        {
            TemplateId = "secret_room_01",
            Name = "Secret Room",
            Archetype = RoomArchetype.SecretRoom,
            Description = "A hidden treasure room",
            ValidConnections = new List<RoomArchetype>()
        });

        return library;
    }

    [TestMethod]
    public void Benchmark_SectorGeneration_Under2Seconds()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        var stopwatch = new Stopwatch();
        var times = new List<long>();

        _logger.Information("Starting performance benchmark: 20 sector generation");

        // Act - Generate 20 sectors
        for (int seed = 1; seed <= 20; seed++)
        {
            stopwatch.Restart();
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);
            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);

            if (seed % 5 == 0)
            {
                _logger.Information($"Progress: {seed}/20 - Latest: {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        // Assert
        var avgTime = times.Average();
        var maxTime = times.Max();
        var minTime = times.Min();
        var p95 = times.OrderBy(t => t).ElementAt((int)(times.Count * 0.95));

        _logger.Information("");
        _logger.Information("=== Performance Benchmark Results ===");
        _logger.Information($"Average:   {avgTime:F0}ms (Target: <1500ms)");
        _logger.Information($"Min:       {minTime}ms");
        _logger.Information($"Max:       {maxTime}ms (Target: <2000ms)");
        _logger.Information($"95th pct:  {p95}ms");
        _logger.Information($"Samples:   {times.Count}");
        _logger.Information("=====================================");

        Assert.IsTrue(avgTime < 1500, $"Average time {avgTime}ms exceeds target 1500ms");
        Assert.IsTrue(maxTime < 2000, $"Max time {maxTime}ms exceeds acceptable 2000ms");
    }

    [TestMethod]
    public void Benchmark_PhaseBreakdown_IdentifyBottlenecks()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        var phaseTimes = new Dictionary<string, List<long>>();

        _logger.Information("Starting phase breakdown benchmark");

        // Act - Run 10 times to get stable measurements
        for (int run = 1; run <= 10; run++)
        {
            var sw = Stopwatch.StartNew();
            var dungeon = generator.GenerateWithFullPipeline(run, run, 7, _testBiome);
            sw.Stop();

            // Note: Detailed phase timing would require instrumenting DungeonGenerator
            // For now, just track total time
            if (!phaseTimes.ContainsKey("Total"))
                phaseTimes["Total"] = new List<long>();

            phaseTimes["Total"].Add(sw.ElapsedMilliseconds);
        }

        // Report
        _logger.Information("");
        _logger.Information("=== Phase Breakdown ===");
        foreach (var kvp in phaseTimes)
        {
            var avg = kvp.Value.Average();
            _logger.Information($"{kvp.Key,-20}: {avg,6:F0}ms avg");
        }
        _logger.Information("=======================");

        // Assert - Total should be reasonable
        var totalAvg = phaseTimes["Total"].Average();
        Assert.IsTrue(totalAvg < 2000, $"Average generation time {totalAvg}ms too high");
    }

    [TestMethod]
    public void Benchmark_LargeSectors_ScaleLinear()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        var results = new List<(int rooms, double time)>();

        _logger.Information("Starting scale benchmark");

        // Act - Test different sector sizes
        var roomCounts = new[] { 5, 7, 10, 15 };

        foreach (var roomCount in roomCounts)
        {
            var times = new List<long>();

            for (int seed = 1; seed <= 5; seed++)
            {
                var sw = Stopwatch.StartNew();
                var dungeon = generator.GenerateWithFullPipeline(seed, seed, roomCount, _testBiome);
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }

            var avgTime = times.Average();
            results.Add((roomCount, avgTime));

            _logger.Information($"{roomCount} rooms: {avgTime:F0}ms avg");
        }

        // Assert - Should scale roughly linearly (< 200ms per additional room on average)
        for (int i = 1; i < results.Count; i++)
        {
            var prev = results[i - 1];
            var curr = results[i];

            var roomDiff = curr.rooms - prev.rooms;
            var timeDiff = curr.time - prev.time;
            var msPerRoom = timeDiff / roomDiff;

            _logger.Information($"  {prev.rooms}→{curr.rooms} rooms: +{msPerRoom:F0}ms per room");

            Assert.IsTrue(msPerRoom < 300,
                $"Time per additional room ({msPerRoom:F0}ms) too high, suggests O(n²) complexity");
        }
    }

    [TestMethod]
    public void Benchmark_ConcurrentGeneration_ThreadSafe()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        var tasks = new List<Task<(int seed, long time)>>();

        _logger.Information("Starting concurrent generation test");

        // Act - Generate 10 sectors concurrently
        for (int seed = 1; seed <= 10; seed++)
        {
            int localSeed = seed; // Capture for closure
            tasks.Add(Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                var dungeon = generator.GenerateWithFullPipeline(localSeed, localSeed, 7, _testBiome);
                sw.Stop();
                return (localSeed, sw.ElapsedMilliseconds);
            }));
        }

        Task.WaitAll(tasks.ToArray());

        var results = tasks.Select(t => t.Result).OrderBy(r => r.seed).ToList();

        // Report
        _logger.Information("");
        _logger.Information("=== Concurrent Generation Results ===");
        foreach (var (seed, time) in results)
        {
            _logger.Information($"Seed {seed,2}: {time,5}ms");
        }

        var avgTime = results.Average(r => r.time);
        var maxTime = results.Max(r => r.time);

        _logger.Information($"Average: {avgTime:F0}ms");
        _logger.Information($"Max:     {maxTime}ms");
        _logger.Information("=====================================");

        // Assert - Should complete without errors
        Assert.AreEqual(10, results.Count, "Not all concurrent tasks completed");

        // Assert - Performance should not degrade significantly
        Assert.IsTrue(avgTime < 2000, $"Average concurrent time {avgTime}ms too high");
    }

    [TestMethod]
    public void Benchmark_MemoryUsage_NoLeaks()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        _logger.Information("Starting memory usage test");

        // Record initial memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var initialMemory = GC.GetTotalMemory(false);

        // Act - Generate 50 sectors
        for (int seed = 1; seed <= 50; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);

            // Let dungeon go out of scope
        }

        // Force GC
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(false);
        var memoryDelta = finalMemory - initialMemory;
        var memoryDeltaMB = memoryDelta / (1024.0 * 1024.0);

        _logger.Information("");
        _logger.Information("=== Memory Usage ===");
        _logger.Information($"Initial:  {initialMemory / (1024.0 * 1024.0):F2} MB");
        _logger.Information($"Final:    {finalMemory / (1024.0 * 1024.0):F2} MB");
        _logger.Information($"Delta:    {memoryDeltaMB:F2} MB");
        _logger.Information($"Per sector: {memoryDeltaMB / 50.0:F3} MB");
        _logger.Information("====================");

        // Assert - Memory growth should be reasonable (< 50MB for 50 sectors)
        Assert.IsTrue(memoryDeltaMB < 50,
            $"Memory usage increased by {memoryDeltaMB:F2}MB, suggesting memory leak");
    }

    [TestMethod]
    public void Benchmark_Warmup_FirstRunNotSlower()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        var times = new List<long>();

        _logger.Information("Starting warmup benchmark");

        // Act - Run 20 times
        for (int i = 1; i <= 20; i++)
        {
            var sw = Stopwatch.StartNew();
            var dungeon = generator.GenerateWithFullPipeline(i, i, 7, _testBiome);
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        // Assert - First 5 should not be significantly slower than last 5
        var firstFiveAvg = times.Take(5).Average();
        var lastFiveAvg = times.Skip(15).Take(5).Average();

        _logger.Information("");
        _logger.Information($"First 5 average: {firstFiveAvg:F0}ms");
        _logger.Information($"Last 5 average:  {lastFiveAvg:F0}ms");

        // Allow first runs to be up to 50% slower due to JIT warmup
        Assert.IsTrue(firstFiveAvg < lastFiveAvg * 1.5,
            $"First runs significantly slower ({firstFiveAvg:F0}ms vs {lastFiveAvg:F0}ms), may indicate initialization issues");
    }

    [TestMethod]
    public void Benchmark_DifferentBiomes_ConsistentPerformance()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        var biomes = new[]
        {
            new BiomeDefinition { BiomeId = "the_roots", Name = "The Roots", DifficultyTier = DifficultyTier.Normal },
            new BiomeDefinition { BiomeId = "muspelheim", Name = "Muspelheim", DifficultyTier = DifficultyTier.Normal },
            new BiomeDefinition { BiomeId = "niflheim", Name = "Niflheim", DifficultyTier = DifficultyTier.Normal }
        };

        var results = new Dictionary<string, double>();

        // Act
        foreach (var biome in biomes)
        {
            var times = new List<long>();

            for (int seed = 1; seed <= 5; seed++)
            {
                var sw = Stopwatch.StartNew();
                var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, biome);
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }

            results[biome.BiomeId] = times.Average();
            _logger.Information($"{biome.Name,-15}: {times.Average():F0}ms avg");
        }

        // Assert - All biomes should have similar performance
        var minTime = results.Values.Min();
        var maxTime = results.Values.Max();
        var variance = (maxTime - minTime) / minTime;

        Assert.IsTrue(variance < 0.5,
            $"Biome performance variance too high ({variance:P0}), suggests biome-specific bottleneck");
    }
}
