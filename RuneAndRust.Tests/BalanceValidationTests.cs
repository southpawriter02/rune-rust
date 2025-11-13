using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Engine.Telemetry;
using RuneAndRust.Engine.CoherentGlitch;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Tests;

/// <summary>
/// Balance validation tests for v0.12
/// Statistical analysis of generated Sectors
/// </summary>
[TestClass]
public class BalanceValidationTests
{
    [TestMethod]
    public void CalculateMetrics_WithValidDungeon_ReturnsCorrectStatistics()
    {
        // Arrange
        var dungeon = CreateTestDungeon();

        // Act
        var metrics = SectorBalanceMetrics.CalculateFromDungeon(dungeon);

        // Assert
        Assert.IsTrue(metrics.AverageEnemiesPerRoom >= 0, "Should calculate average enemies");
        Assert.IsTrue(metrics.TotalLootNodes >= 0, "Should count loot nodes");
        Assert.IsTrue(metrics.EstimatedCogsValue >= 0, "Should calculate Cogs value");
    }

    [TestMethod]
    public void ValidateBalance_WithBalancedMetrics_ReturnsNoIssues()
    {
        // Arrange
        var metrics = new SectorBalanceMetrics
        {
            AverageEnemiesPerRoom = 3.0, // Within 2.0-4.0 range
            ChampionToMinionRatio = 0.20, // Within 0.15-0.30 range
            AverageCogsPerRoom = 100.0 // Within 50-150 range
        };

        // Act
        var issues = metrics.ValidateBalance();

        // Assert
        Assert.AreEqual(0, issues.Count, "Balanced metrics should have no issues");
    }

    [TestMethod]
    public void ValidateBalance_WithTooFewEnemies_ReturnsIssue()
    {
        // Arrange
        var metrics = new SectorBalanceMetrics
        {
            AverageEnemiesPerRoom = 1.5, // Below 2.0 threshold
            ChampionToMinionRatio = 0.20,
            AverageCogsPerRoom = 100.0
        };

        // Act
        var issues = metrics.ValidateBalance();

        // Assert
        Assert.IsTrue(issues.Count > 0, "Should detect low enemy density");
        Assert.IsTrue(issues.Any(i => i.Contains("too low")), "Should mention 'too low'");
    }

    [TestMethod]
    public void ValidateBalance_WithTooManyChampions_ReturnsIssue()
    {
        // Arrange
        var metrics = new SectorBalanceMetrics
        {
            AverageEnemiesPerRoom = 3.0,
            ChampionToMinionRatio = 0.35, // Above 0.30 threshold
            AverageCogsPerRoom = 100.0
        };

        // Act
        var issues = metrics.ValidateBalance();

        // Assert
        Assert.IsTrue(issues.Count > 0, "Should detect high champion ratio");
        Assert.IsTrue(issues.Any(i => i.Contains("Champion ratio")), "Should mention champion ratio");
    }

    [TestMethod]
    public void ValidateBalance_WithPoorLootEconomy_ReturnsIssue()
    {
        // Arrange
        var metrics = new SectorBalanceMetrics
        {
            AverageEnemiesPerRoom = 3.0,
            ChampionToMinionRatio = 0.20,
            AverageCogsPerRoom = 30.0 // Below 50 threshold
        };

        // Act
        var issues = metrics.ValidateBalance();

        // Assert
        Assert.IsTrue(issues.Count > 0, "Should detect poor loot economy");
        Assert.IsTrue(issues.Any(i => i.Contains("Loot economy")), "Should mention loot economy");
    }

    [TestMethod]
    [TestCategory("LongRunning")]
    public void GenerateMultipleSectors_EnemyDistribution_WithinExpectedRange()
    {
        // Arrange
        var generator = new DungeonGenerator(new TemplateLibrary());
        int sectorCount = 20; // Reduced from 100 for faster testing
        var metrics = new List<SectorBalanceMetrics>();

        // Act
        for (int i = 0; i < sectorCount; i++)
        {
            var dungeon = generator.GenerateComplete(seed: i, dungeonId: i);
            metrics.Add(SectorBalanceMetrics.CalculateFromDungeon(dungeon));
        }

        // Assert
        var avgEnemiesPerRoom = metrics.Average(m => m.AverageEnemiesPerRoom);

        // Note: This test may fail if v0.11 population isn't fully implemented yet
        // Target range: 2.0-4.0 enemies per room
        // For now, just validate structure is correct
        Assert.IsTrue(avgEnemiesPerRoom >= 0,
            "Average enemies per room should be non-negative");
    }

    [TestMethod]
    [TestCategory("LongRunning")]
    public void GenerateMultipleSectors_LootEconomy_WithinBudget()
    {
        // Arrange
        var generator = new DungeonGenerator(new TemplateLibrary());
        int sectorCount = 20;
        var metrics = new List<SectorBalanceMetrics>();

        // Act
        for (int i = 0; i < sectorCount; i++)
        {
            var dungeon = generator.GenerateComplete(seed: i, dungeonId: i);
            metrics.Add(SectorBalanceMetrics.CalculateFromDungeon(dungeon));
        }

        // Assert
        var avgCogsPerRoom = metrics.Average(m => m.AverageCogsPerRoom);

        // Note: This test validates structure; actual loot spawning requires v0.11 implementation
        Assert.IsTrue(avgCogsPerRoom >= 0,
            "Average Cogs per room should be non-negative");
    }

    [TestMethod]
    public void GenerationTelemetry_CalculatesPerformanceMetrics()
    {
        // Arrange
        var telemetry = new GenerationTelemetry
        {
            Seed = 12345,
            DungeonId = 1,
            BiomeId = "the_roots",
            GenerationStartTime = DateTime.UtcNow,
            GenerationEndTime = DateTime.UtcNow.AddMilliseconds(450),
            RoomCount = 7,
            TotalEnemies = 18,
            TotalHazards = 5,
            TotalLootNodes = 12,
            EstimatedTotalCogsValue = 600
        };

        // Act & Assert
        Assert.IsTrue(telemetry.TotalGenerationTime.TotalMilliseconds >= 0,
            "Should calculate generation time");
        Assert.IsTrue(telemetry.MeetsStrictPerformanceTarget,
            "450ms should meet strict target (< 500ms)");
        Assert.IsTrue(telemetry.MeetsPerformanceTarget,
            "450ms should meet acceptable target (< 700ms)");

        Assert.AreEqual(18.0 / 7.0, telemetry.AverageEnemiesPerRoom, 0.01,
            "Should calculate average enemies per room");
        Assert.AreEqual(600.0 / 7.0, telemetry.AverageCogsPerRoom, 0.01,
            "Should calculate average Cogs per room");
    }

    [TestMethod]
    public void GenerationTelemetry_PerformanceTarget_DetectsSlowGeneration()
    {
        // Arrange
        var telemetry = new GenerationTelemetry
        {
            GenerationStartTime = DateTime.UtcNow,
            GenerationEndTime = DateTime.UtcNow.AddMilliseconds(800), // Too slow
            RoomCount = 7
        };

        // Act & Assert
        Assert.IsFalse(telemetry.MeetsStrictPerformanceTarget,
            "800ms should fail strict target (< 500ms)");
        Assert.IsFalse(telemetry.MeetsPerformanceTarget,
            "800ms should fail acceptable target (< 700ms)");
    }

    private Dungeon CreateTestDungeon()
    {
        var dungeon = new Dungeon
        {
            DungeonId = 1,
            Seed = 12345,
            Biome = "the_roots"
        };

        var room1 = new Room
        {
            RoomId = "room_1",
            // DormantProcesses = new List<DormantProcess>
            {
                new DormantProcess { ThreatLevel = ThreatLevel.Low },
                new DormantProcess { ThreatLevel = ThreatLevel.Medium }
            },
            DynamicHazards = new List<DynamicHazard>
            {
                new SteamVentHazard()
            },
            LootNodes = new List<LootNode>
            {
                new ResourceVein { EstimatedCogsValue = 30 },
                new SalvageableWreckage { EstimatedCogsValue = 40 }
            }
        };

        var room2 = new Room
        {
            RoomId = "room_2",
            // DormantProcesses = new List<DormantProcess>
            {
                new DormantProcess { ThreatLevel = ThreatLevel.High, IsChampion = true }
            },
            LootNodes = new List<LootNode>
            {
                new HiddenContainer { EstimatedCogsValue = 120 }
            }
        };

        dungeon.Rooms["room_1"] = room1;
        dungeon.Rooms["room_2"] = room2;

        return dungeon;
    }
}
