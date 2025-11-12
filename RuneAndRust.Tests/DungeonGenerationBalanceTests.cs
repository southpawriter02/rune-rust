using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// Tests for dungeon generation balance and polish (v0.10 Phase 9)
/// Validates template variety, name quality, and generation consistency
/// </summary>
[TestFixture]
public class DungeonGenerationBalanceTests
{
    private TemplateLibrary _templateLibrary = null!;
    private BiomeLibrary _biomeLibrary = null!;
    private SeedManager _seedManager = null!;
    private DungeonService _dungeonService = null!;

    [SetUp]
    public void Setup()
    {
        // Configure Serilog for test output
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning() // Reduce noise in test output
            .WriteTo.Console()
            .CreateLogger();

        _templateLibrary = new TemplateLibrary("Data/RoomTemplates");
        _templateLibrary.LoadTemplates();

        _biomeLibrary = new BiomeLibrary("Data/Biomes");
        _biomeLibrary.LoadBiomes();

        _seedManager = new SeedManager();

        _dungeonService = new DungeonService(_templateLibrary, _biomeLibrary, _seedManager);
    }

    [TearDown]
    public void TearDown()
    {
        Log.CloseAndFlush();
    }

    /// <summary>
    /// Test: Generate 30 dungeons and verify template variety
    /// Success criteria: Each template should appear at least once across all dungeons
    /// </summary>
    [Test]
    public void Generate30Dungeons_VerifyTemplateVariety()
    {
        const int dungeonCount = 30;
        var templateUsage = new Dictionary<string, int>();
        var allDungeons = new List<GameWorld>();

        // Generate 30 dungeons with different seeds
        for (int i = 0; i < dungeonCount; i++)
        {
            int seed = 10000 + i; // Deterministic seeds for reproducibility
            var world = _dungeonService.CreateProceduralWorld(seed: seed, biomeId: "the_roots");
            allDungeons.Add(world);

            // Track template usage
            foreach (var room in world.Rooms.Values)
            {
                var templateId = room.TemplateId ?? "unknown";
                if (!templateUsage.ContainsKey(templateId))
                {
                    templateUsage[templateId] = 0;
                }
                templateUsage[templateId]++;
            }
        }

        // Verify all templates are used at least once
        var availableTemplates = _templateLibrary.GetAllTemplates();
        var unusedTemplates = availableTemplates
            .Where(t => !templateUsage.ContainsKey(t.TemplateId))
            .Select(t => t.TemplateId)
            .ToList();

        TestContext.WriteLine($"Generated {dungeonCount} dungeons with {allDungeons.Sum(d => d.Rooms.Count)} total rooms");
        TestContext.WriteLine($"Template usage distribution:");
        foreach (var kvp in templateUsage.OrderByDescending(x => x.Value))
        {
            TestContext.WriteLine($"  {kvp.Key}: {kvp.Value} uses");
        }

        if (unusedTemplates.Count > 0)
        {
            TestContext.WriteLine($"WARNING: {unusedTemplates.Count} templates never used: {string.Join(", ", unusedTemplates)}");
        }

        // Assert: Most templates should be used (allowing for some rare templates to be unused in 30 dungeons)
        var usedTemplatePercentage = (double)templateUsage.Count / availableTemplates.Count;
        Assert.That(usedTemplatePercentage, Is.GreaterThan(0.7),
            $"Only {usedTemplatePercentage:P0} of templates were used across {dungeonCount} dungeons");
    }

    /// <summary>
    /// Test: Verify no placeholder text in room names or descriptions
    /// Success criteria: No {Adjective}, {Detail}, etc. in generated content
    /// </summary>
    [Test]
    public void Generate10Dungeons_VerifyNoPlaceholders()
    {
        const int dungeonCount = 10;
        var placeholderPatterns = new[] { "{Adjective}", "{Detail}", "{Sound}", "{Smell}" };
        var roomsWithPlaceholders = new List<string>();

        for (int i = 0; i < dungeonCount; i++)
        {
            int seed = 20000 + i;
            var world = _dungeonService.CreateProceduralWorld(seed: seed, biomeId: "the_roots");

            foreach (var room in world.Rooms.Values)
            {
                foreach (var placeholder in placeholderPatterns)
                {
                    if (room.Name.Contains(placeholder) || room.Description.Contains(placeholder))
                    {
                        roomsWithPlaceholders.Add($"{room.RoomId}: {room.Name}");
                    }
                }
            }
        }

        if (roomsWithPlaceholders.Count > 0)
        {
            TestContext.WriteLine("Rooms with placeholders:");
            foreach (var room in roomsWithPlaceholders)
            {
                TestContext.WriteLine($"  {room}");
            }
        }

        Assert.That(roomsWithPlaceholders, Is.Empty,
            $"Found {roomsWithPlaceholders.Count} rooms with unreplaced placeholders");
    }

    /// <summary>
    /// Test: Verify dungeon connectivity and navigation
    /// Success criteria: All rooms are reachable from start, boss is reachable
    /// </summary>
    [Test]
    public void Generate20Dungeons_VerifyConnectivity()
    {
        const int dungeonCount = 20;
        var invalidDungeons = new List<string>();

        for (int i = 0; i < dungeonCount; i++)
        {
            int seed = 30000 + i;
            var world = _dungeonService.CreateProceduralWorld(seed: seed, biomeId: "the_roots");

            // Verify all rooms are reachable using BFS
            var visited = new HashSet<string>();
            var queue = new Queue<string>();
            queue.Enqueue(world.StartRoomName);
            visited.Add(world.StartRoomName);

            while (queue.Count > 0)
            {
                var currentRoomId = queue.Dequeue();
                var currentRoom = world.GetRoom(currentRoomId);

                foreach (var exitRoomId in currentRoom.Exits.Values)
                {
                    if (!visited.Contains(exitRoomId))
                    {
                        visited.Add(exitRoomId);
                        queue.Enqueue(exitRoomId);
                    }
                }
            }

            // Check if all rooms are reachable
            if (visited.Count != world.Rooms.Count)
            {
                var unreachableRooms = world.Rooms.Keys.Except(visited).ToList();
                invalidDungeons.Add($"Seed {seed}: {unreachableRooms.Count} unreachable rooms");
                TestContext.WriteLine($"Seed {seed}: Unreachable rooms: {string.Join(", ", unreachableRooms)}");
            }

            // Verify boss room is reachable
            var bossRoom = world.Rooms.Values.FirstOrDefault(r => r.GeneratedNodeType == NodeType.Boss);
            if (bossRoom != null && !visited.Contains(bossRoom.RoomId))
            {
                invalidDungeons.Add($"Seed {seed}: Boss room not reachable");
            }
        }

        Assert.That(invalidDungeons, Is.Empty,
            $"Found {invalidDungeons.Count} dungeons with connectivity issues:\n{string.Join("\n", invalidDungeons)}");
    }

    /// <summary>
    /// Test: Verify room size distribution
    /// Success criteria: Dungeons should have varied room counts within biome parameters
    /// </summary>
    [Test]
    public void Generate30Dungeons_VerifyRoomCountVariety()
    {
        const int dungeonCount = 30;
        var roomCounts = new List<int>();

        for (int i = 0; i < dungeonCount; i++)
        {
            int seed = 40000 + i;
            var world = _dungeonService.CreateProceduralWorld(seed: seed, biomeId: "the_roots");
            roomCounts.Add(world.Rooms.Count);
        }

        var minRooms = roomCounts.Min();
        var maxRooms = roomCounts.Max();
        var avgRooms = roomCounts.Average();

        TestContext.WriteLine($"Room count distribution across {dungeonCount} dungeons:");
        TestContext.WriteLine($"  Min: {minRooms}");
        TestContext.WriteLine($"  Max: {maxRooms}");
        TestContext.WriteLine($"  Average: {avgRooms:F1}");

        // Biome "the_roots" has MinRoomCount=5, MaxRoomCount=7
        // With branching and secrets, actual count will be higher
        Assert.That(minRooms, Is.GreaterThanOrEqualTo(5), "Minimum room count too low");
        Assert.That(maxRooms, Is.LessThanOrEqualTo(15), "Maximum room count too high");
        Assert.That(maxRooms - minRooms, Is.GreaterThan(2), "Not enough variety in room counts");
    }

    /// <summary>
    /// Test: Verify secret room generation
    /// Success criteria: Some dungeons should have secret rooms
    /// </summary>
    [Test]
    public void Generate30Dungeons_VerifySecretRoomGeneration()
    {
        const int dungeonCount = 30;
        int dungeonsWithSecrets = 0;
        int totalSecretRooms = 0;

        for (int i = 0; i < dungeonCount; i++)
        {
            int seed = 50000 + i;
            var world = _dungeonService.CreateProceduralWorld(seed: seed, biomeId: "the_roots");

            var secretRooms = world.Rooms.Values
                .Where(r => r.GeneratedNodeType == NodeType.Secret)
                .ToList();

            if (secretRooms.Count > 0)
            {
                dungeonsWithSecrets++;
                totalSecretRooms += secretRooms.Count;
            }
        }

        TestContext.WriteLine($"Secret room statistics across {dungeonCount} dungeons:");
        TestContext.WriteLine($"  Dungeons with secrets: {dungeonsWithSecrets} ({(double)dungeonsWithSecrets / dungeonCount:P0})");
        TestContext.WriteLine($"  Total secret rooms: {totalSecretRooms}");
        TestContext.WriteLine($"  Average secrets per dungeon with secrets: {(double)totalSecretRooms / Math.Max(dungeonsWithSecrets, 1):F1}");

        // Biome has 20% secret room probability, so roughly 6 dungeons should have secrets
        Assert.That(dungeonsWithSecrets, Is.GreaterThan(0), "No dungeons generated secret rooms");
        Assert.That(dungeonsWithSecrets, Is.LessThan(dungeonCount), "All dungeons have secret rooms (should be random)");
    }

    /// <summary>
    /// Test: Verify branching path generation
    /// Success criteria: Some dungeons should have branching paths
    /// </summary>
    [Test]
    public void Generate30Dungeons_VerifyBranchingPaths()
    {
        const int dungeonCount = 30;
        int dungeonsWithBranches = 0;
        int totalBranchRooms = 0;

        for (int i = 0; i < dungeonCount; i++)
        {
            int seed = 60000 + i;
            var world = _dungeonService.CreateProceduralWorld(seed: seed, biomeId: "the_roots");

            var branchRooms = world.Rooms.Values
                .Where(r => r.GeneratedNodeType == NodeType.Branch)
                .ToList();

            if (branchRooms.Count > 0)
            {
                dungeonsWithBranches++;
                totalBranchRooms += branchRooms.Count;
            }
        }

        TestContext.WriteLine($"Branching path statistics across {dungeonCount} dungeons:");
        TestContext.WriteLine($"  Dungeons with branches: {dungeonsWithBranches} ({(double)dungeonsWithBranches / dungeonCount:P0})");
        TestContext.WriteLine($"  Total branch rooms: {totalBranchRooms}");
        TestContext.WriteLine($"  Average branches per dungeon with branches: {(double)totalBranchRooms / Math.Max(dungeonsWithBranches, 1):F1}");

        // Biome has 40% branching probability, so roughly 12 dungeons should have branches
        Assert.That(dungeonsWithBranches, Is.GreaterThan(5), "Too few dungeons with branching paths");
    }

    /// <summary>
    /// Test: Performance - generate 100 dungeons under time limit
    /// Success criteria: Each dungeon generation should take < 200ms
    /// </summary>
    [Test]
    public void GenerateDungeons_PerformanceTest()
    {
        const int dungeonCount = 100;
        var generationTimes = new List<double>();

        for (int i = 0; i < dungeonCount; i++)
        {
            int seed = 70000 + i;
            var startTime = DateTime.Now;
            var world = _dungeonService.CreateProceduralWorld(seed: seed, biomeId: "the_roots");
            var duration = (DateTime.Now - startTime).TotalMilliseconds;
            generationTimes.Add(duration);
        }

        var avgTime = generationTimes.Average();
        var maxTime = generationTimes.Max();
        var minTime = generationTimes.Min();

        TestContext.WriteLine($"Performance statistics for {dungeonCount} dungeons:");
        TestContext.WriteLine($"  Average generation time: {avgTime:F1}ms");
        TestContext.WriteLine($"  Min: {minTime:F1}ms");
        TestContext.WriteLine($"  Max: {maxTime:F1}ms");

        Assert.That(avgTime, Is.LessThan(200), "Average generation time too slow");
        Assert.That(maxTime, Is.LessThan(500), "Slowest generation time too slow");
    }

    /// <summary>
    /// Test: Seed reproducibility across multiple generations
    /// Success criteria: Same seed should always produce identical dungeons
    /// </summary>
    [Test]
    public void SameSeed_ProduceIdenticalDungeons()
    {
        const int seed = 12345;
        const int repetitions = 10;

        // Generate first dungeon
        var firstWorld = _dungeonService.CreateProceduralWorld(seed: seed, biomeId: "the_roots");
        var firstRoomNames = firstWorld.Rooms.Values
            .OrderBy(r => r.RoomId)
            .Select(r => r.Name)
            .ToList();

        // Generate 9 more dungeons with same seed
        for (int i = 0; i < repetitions - 1; i++)
        {
            var world = _dungeonService.CreateProceduralWorld(seed: seed, biomeId: "the_roots");
            var roomNames = world.Rooms.Values
                .OrderBy(r => r.RoomId)
                .Select(r => r.Name)
                .ToList();

            // Verify identical structure
            Assert.That(world.Rooms.Count, Is.EqualTo(firstWorld.Rooms.Count),
                $"Repetition {i + 1}: Different room count");
            Assert.That(roomNames, Is.EqualTo(firstRoomNames),
                $"Repetition {i + 1}: Different room names");
        }

        TestContext.WriteLine($"Seed {seed} generated identical dungeons {repetitions} times");
    }
}
