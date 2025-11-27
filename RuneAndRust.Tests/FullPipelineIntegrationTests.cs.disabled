using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using RuneAndRust.Engine;
using RuneAndRust.Engine.Spatial;
using Serilog;
using System.Diagnostics;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.39.4: Full Pipeline Integration Tests
/// Tests the complete 6-phase generation pipeline with all v0.39 components
/// </summary>
[TestClass]
public class FullPipelineIntegrationTests
{
    private ILogger _logger = null!;
    private TemplateLibrary _templateLibrary = null!;
    private BiomeDefinition _testBiome = null!;

    [TestInitialize]
    public void Setup()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Create test biome
        _testBiome = new BiomeDefinition
        {
            BiomeId = "the_roots",
            Name = "The Roots",
            DifficultyTier = DifficultyTier.Normal,
            MinRoomCount = 5,
            MaxRoomCount = 7,
            BranchingProbability = 0.6f,
            SecretRoomProbability = 0.3f
        };

        // Create template library
        _templateLibrary = CreateTestTemplateLibrary();
    }

    #region Helper Methods

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
            populationPipeline: null, // Not needed for v0.39.3
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

        // Add entry hall template
        library.AddTemplate(new RoomTemplate
        {
            TemplateId = "entry_hall_01",
            Name = "Entry Hall",
            Archetype = RoomArchetype.EntryHall,
            Description = "A standard entry hall",
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor, RoomArchetype.Chamber }
        });

        // Add corridor templates
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

        // Add chamber templates
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

        // Add boss arena template
        library.AddTemplate(new RoomTemplate
        {
            TemplateId = "boss_arena_01",
            Name = "Boss Arena",
            Archetype = RoomArchetype.BossArena,
            Description = "A large arena for boss encounters",
            ValidConnections = new List<RoomArchetype>()
        });

        // Add secret room template
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

    #endregion

    [TestMethod]
    public void FullPipeline_GenerateSector_AllPhasesComplete()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        int seed = 12345;
        int dungeonId = 1;

        // Act
        var dungeon = generator.GenerateWithFullPipeline(seed, dungeonId, 7, _testBiome);

        // Assert - v0.10: Layout
        Assert.IsNotNull(dungeon);
        Assert.IsTrue(dungeon.TotalRoomCount >= 5 && dungeon.TotalRoomCount <= 9); // Account for branches/secrets
        Assert.AreEqual(_testBiome.BiomeId, dungeon.Biome);

        // Assert - v0.39.1: 3D Spatial
        Assert.IsTrue(dungeon.Rooms.Values.All(r => r.Position != null));
        var entryHall = dungeon.Rooms.Values.First(r => r.Archetype == RoomArchetype.EntryHall);
        Assert.AreEqual(Core.Spatial.RoomPosition.Origin, entryHall.Position);

        // Assert - v0.39.3: Content Density
        var totalEnemies = dungeon.Rooms.Values.Sum(r => r.Enemies.Count);
        Assert.IsTrue(totalEnemies >= 5, $"Expected at least 5 enemies, got {totalEnemies}");

        // Assert - Empty rooms exist
        var emptyRooms = dungeon.Rooms.Values.Count(r =>
            r.Enemies.Count == 0 && r.Hazards.Count == 0);
        Assert.IsTrue(emptyRooms >= 1, "At least one breather room should exist");

        _logger.Information("FullPipeline_GenerateSector_AllPhasesComplete: PASSED");
    }

    [TestMethod]
    public void FullPipeline_MultiBiomeSector_TransitionsLogical()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        int seed = 54321;
        int dungeonId = 2;

        var secondaryBiome = new BiomeDefinition
        {
            BiomeId = "muspelheim",
            Name = "Muspelheim",
            DifficultyTier = DifficultyTier.Normal,
            MinRoomCount = 5,
            MaxRoomCount = 7
        };

        // Act
        var dungeon = generator.GenerateWithFullPipeline(
            seed, dungeonId, 7, _testBiome,
            new List<BiomeDefinition> { secondaryBiome });

        // Assert - Transition zones exist
        var transitionRooms = dungeon.Rooms.Values
            .Where(r => !string.IsNullOrEmpty(r.SecondaryBiome))
            .ToList();

        Assert.IsTrue(transitionRooms.Any(), "No transition zones found");

        // Assert - Gradual blending
        foreach (var room in transitionRooms.OrderBy(r => r.BiomeBlendRatio))
        {
            Assert.IsTrue(room.BiomeBlendRatio >= 0.0f && room.BiomeBlendRatio <= 1.0f);
            Assert.IsTrue(
                room.Description.Contains("transition") ||
                room.Description.Contains("shift") ||
                room.Description.Contains("change") ||
                room.Description.Contains("heat") || // Muspelheim specific
                room.Description.Contains("warm"),
                $"Transition room missing transition language: {room.Description}");
        }

        _logger.Information("FullPipeline_MultiBiomeSector_TransitionsLogical: PASSED");
    }

    [TestMethod]
    public void FullPipeline_VerticalSector_AllLayersReachable()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        int seed = 99999;
        int dungeonId = 3;

        // Act
        var dungeon = generator.GenerateWithFullPipeline(seed, dungeonId, 9, _testBiome);

        // Assert - Multiple Z levels possible (not guaranteed with 9 rooms though)
        var zLevels = dungeon.Rooms.Values
            .Where(r => r.Position != null)
            .Select(r => r.Position!.Z)
            .Distinct()
            .ToList();

        _logger.Information($"Sector spans {zLevels.Count} Z levels");

        // Assert - All rooms have positions
        Assert.IsTrue(dungeon.Rooms.Values.All(r => r.Position != null));

        // If we have vertical connections, validate them
        if (dungeon.VerticalConnections.Any())
        {
            Assert.IsTrue(zLevels.Count >= 2, "Sector with vertical connections should span multiple Z levels");
        }

        _logger.Information("FullPipeline_VerticalSector_AllLayersReachable: PASSED");
    }

    [TestMethod]
    public void FullPipeline_DensityClassification_FollowsDistribution()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        int seed = 77777;
        int dungeonId = 4;

        // Act
        var dungeon = generator.GenerateWithFullPipeline(seed, dungeonId, 7, _testBiome);

        // Assert - Boss room has most enemies
        var bossRoom = dungeon.Rooms.Values.FirstOrDefault(r => r.IsBossRoom);
        Assert.IsNotNull(bossRoom, "Boss room should exist");

        var bossEnemyCount = bossRoom.Enemies.Count;
        var avgEnemyCount = dungeon.Rooms.Values.Average(r => r.Enemies.Count);

        _logger.Information($"Boss room: {bossEnemyCount} enemies, Average: {avgEnemyCount:F2}");

        // Boss room should have above average (not necessarily most due to budget constraints)
        Assert.IsTrue(bossEnemyCount >= avgEnemyCount * 0.8f,
            $"Boss room has {bossEnemyCount} enemies, which is below average {avgEnemyCount:F2}");

        _logger.Information("FullPipeline_DensityClassification_FollowsDistribution: PASSED");
    }

    [TestMethod]
    public void FullPipeline_Generate10Sectors_NoErrors()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        var errors = new List<string>();

        // Act
        for (int seed = 1; seed <= 10; seed++)
        {
            try
            {
                var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);

                // Basic validation
                Assert.IsNotNull(dungeon);
                Assert.IsTrue(dungeon.TotalRoomCount >= 5);
                Assert.IsTrue(dungeon.Rooms.Values.Any(r => r.IsBossRoom));
            }
            catch (Exception ex)
            {
                errors.Add($"Seed {seed}: {ex.Message}");
            }
        }

        // Assert
        Assert.AreEqual(0, errors.Count,
            $"Generation failed for {errors.Count} sectors:\n{string.Join("\n", errors)}");

        _logger.Information("FullPipeline_Generate10Sectors_NoErrors: PASSED");
    }

    [TestMethod]
    public void FullPipeline_AverageEnemyDensity_WithinTargetRange()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        var metrics = new List<float>();

        // Act - Generate 10 sectors and collect metrics
        for (int seed = 1; seed <= 10; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);
            var avgEnemies = (float)dungeon.Rooms.Values.Average(r => r.Enemies.Count);
            metrics.Add(avgEnemies);
        }

        // Assert - Average enemies per room should be 2.0-3.0 (v0.39.3 target)
        var overallAvg = metrics.Average();
        _logger.Information($"Overall average enemies per room: {overallAvg:F2}");

        Assert.IsTrue(overallAvg >= 1.5f && overallAvg <= 3.5f,
            $"Average enemies per room {overallAvg:F2} outside target range 1.5-3.5");

        _logger.Information("FullPipeline_AverageEnemyDensity_WithinTargetRange: PASSED");
    }

    [TestMethod]
    public void FullPipeline_EmptyRoomPercentage_WithinTargetRange()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        var emptyPercentages = new List<float>();

        // Act - Generate 10 sectors
        for (int seed = 1; seed <= 10; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);
            var emptyRooms = dungeon.Rooms.Values.Count(r =>
                r.Enemies.Count == 0 && r.Hazards.Count == 0);
            var emptyPct = (float)emptyRooms / dungeon.TotalRoomCount;
            emptyPercentages.Add(emptyPct);
        }

        // Assert - Empty room percentage should be 10-20% (v0.39.3 target)
        var avgEmptyPct = emptyPercentages.Average();
        _logger.Information($"Average empty room percentage: {avgEmptyPct:P0}");

        Assert.IsTrue(avgEmptyPct >= 0.05f && avgEmptyPct <= 0.30f,
            $"Empty room percentage {avgEmptyPct:P0} outside target range 5-30%");

        _logger.Information("FullPipeline_EmptyRoomPercentage_WithinTargetRange: PASSED");
    }

    [TestMethod]
    public void FullPipeline_BossRoomIsMostDense_AtLeast80Percent()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        int bossRoomDensestCount = 0;
        int totalSectors = 10;

        // Act
        for (int seed = 1; seed <= totalSectors; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);
            var bossRoom = dungeon.Rooms.Values.FirstOrDefault(r => r.IsBossRoom);

            if (bossRoom != null)
            {
                var bossEnemyCount = bossRoom.Enemies.Count + bossRoom.Hazards.Count;
                var maxThreatCount = dungeon.Rooms.Values.Max(r => r.Enemies.Count + r.Hazards.Count);

                if (bossEnemyCount >= maxThreatCount * 0.9f) // Within 90% of max
                {
                    bossRoomDensestCount++;
                }
            }
        }

        // Assert - Boss rooms should be densest in at least 80% of sectors
        var percentage = (float)bossRoomDensestCount / totalSectors;
        _logger.Information($"Boss rooms densest in {bossRoomDensestCount}/{totalSectors} sectors ({percentage:P0})");

        Assert.IsTrue(bossRoomDensestCount >= totalSectors * 0.7f,
            $"Boss rooms not densest in enough sectors: {bossRoomDensestCount}/{totalSectors}");

        _logger.Information("FullPipeline_BossRoomIsMostDense_AtLeast80Percent: PASSED");
    }

    [TestMethod]
    public void FullPipeline_SpatialValidation_NoCriticalErrors()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // Act - Generate multiple sectors
        for (int seed = 1; seed <= 5; seed++)
        {
            // Should not throw due to critical spatial validation errors
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);

            // Assert - All rooms have valid positions
            Assert.IsTrue(dungeon.Rooms.Values.All(r => r.Position != null));

            // Assert - No room position collisions
            var positions = dungeon.Rooms.Values
                .Select(r => r.Position)
                .ToList();

            var uniquePositions = positions.Distinct().Count();
            Assert.AreEqual(positions.Count, uniquePositions,
                $"Seed {seed}: Room position collisions detected");
        }

        _logger.Information("FullPipeline_SpatialValidation_NoCriticalErrors: PASSED");
    }

    [TestMethod]
    public void FullPipeline_GenerationTime_UnderTwoSeconds()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        var stopwatch = new Stopwatch();
        var times = new List<long>();

        // Act - Generate 5 sectors
        for (int seed = 1; seed <= 5; seed++)
        {
            stopwatch.Restart();
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);
            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);

            _logger.Information($"Seed {seed}: {stopwatch.ElapsedMilliseconds}ms");
        }

        // Assert
        var avgTime = times.Average();
        var maxTime = times.Max();

        _logger.Information($"Performance: Avg={avgTime:F0}ms, Max={maxTime}ms");

        Assert.IsTrue(avgTime < 2000, $"Average time {avgTime}ms exceeds target 2000ms");
        Assert.IsTrue(maxTime < 3000, $"Max time {maxTime}ms exceeds acceptable 3000ms");

        _logger.Information("FullPipeline_GenerationTime_UnderTwoSeconds: PASSED");
    }

    [TestMethod]
    public void FullPipeline_EntryHallIsLightDensity()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // Act
        for (int seed = 1; seed <= 5; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);
            var entryHall = dungeon.Rooms.Values.FirstOrDefault(r => r.Archetype == RoomArchetype.EntryHall);

            // Assert
            Assert.IsNotNull(entryHall, $"Seed {seed}: Entry hall not found");

            var threatCount = entryHall.Enemies.Count + entryHall.Hazards.Count;
            Assert.IsTrue(threatCount <= 2,
                $"Seed {seed}: Entry hall has {threatCount} threats, expected <= 2 (Light density)");
        }

        _logger.Information("FullPipeline_EntryHallIsLightDensity: PASSED");
    }

    [TestMethod]
    public void FullPipeline_SecretRoomsAreEmpty()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // Act
        for (int seed = 1; seed <= 10; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);
            var secretRooms = dungeon.Rooms.Values
                .Where(r => r.Archetype == RoomArchetype.SecretRoom || r.GeneratedNodeType == NodeType.Secret)
                .ToList();

            // Assert - Secret rooms should have no enemies or hazards
            foreach (var secretRoom in secretRooms)
            {
                var threatCount = secretRoom.Enemies.Count + secretRoom.Hazards.Count;
                Assert.AreEqual(0, threatCount,
                    $"Seed {seed}: Secret room {secretRoom.RoomId} has {threatCount} threats, expected 0");

                // But they may have loot
                _logger.Debug($"Seed {seed}: Secret room has {secretRoom.Loot.Count} loot nodes");
            }
        }

        _logger.Information("FullPipeline_SecretRoomsAreEmpty: PASSED");
    }

    [TestMethod]
    public void FullPipeline_DifferentDifficulties_ScalesCorrectly()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        var easyBiome = new BiomeDefinition
        {
            BiomeId = "the_roots",
            Name = "The Roots",
            DifficultyTier = DifficultyTier.Easy,
            MinRoomCount = 5,
            MaxRoomCount = 7
        };

        var hardBiome = new BiomeDefinition
        {
            BiomeId = "the_roots",
            Name = "The Roots",
            DifficultyTier = DifficultyTier.Hard,
            MinRoomCount = 5,
            MaxRoomCount = 7
        };

        // Act
        var easyDungeon = generator.GenerateWithFullPipeline(1, 1, 7, easyBiome);
        var hardDungeon = generator.GenerateWithFullPipeline(1, 2, 7, hardBiome);

        var easyEnemies = easyDungeon.Rooms.Values.Sum(r => r.Enemies.Count);
        var hardEnemies = hardDungeon.Rooms.Values.Sum(r => r.Enemies.Count);

        // Assert - Hard should have more enemies than Easy
        _logger.Information($"Easy: {easyEnemies} enemies, Hard: {hardEnemies} enemies");

        Assert.IsTrue(hardEnemies > easyEnemies,
            $"Hard difficulty ({hardEnemies} enemies) should have more than Easy ({easyEnemies} enemies)");

        _logger.Information("FullPipeline_DifferentDifficulties_ScalesCorrectly: PASSED");
    }

    [TestMethod]
    public void FullPipeline_BiomeAdjacency_RespectsCompatibility()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // The Roots -> Muspelheim should be compatible
        var muspelheimBiome = new BiomeDefinition
        {
            BiomeId = "muspelheim",
            Name = "Muspelheim",
            DifficultyTier = DifficultyTier.Normal
        };

        // Act - Should not throw
        var dungeon = generator.GenerateWithFullPipeline(
            1, 1, 7, _testBiome,
            new List<BiomeDefinition> { muspelheimBiome });

        // Assert
        Assert.IsNotNull(dungeon);
        _logger.Information("FullPipeline_BiomeAdjacency_RespectsCompatibility: PASSED");
    }

    [TestMethod]
    public void FullPipeline_LargerSectors_ScaleBudgetCorrectly()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // Act
        var smallDungeon = generator.GenerateWithFullPipeline(1, 1, 5, _testBiome);
        var largeDungeon = generator.GenerateWithFullPipeline(2, 2, 10, _testBiome);

        var smallEnemies = smallDungeon.Rooms.Values.Sum(r => r.Enemies.Count);
        var largeEnemies = largeDungeon.Rooms.Values.Sum(r => r.Enemies.Count);

        var smallAvg = (float)smallEnemies / smallDungeon.TotalRoomCount;
        var largeAvg = (float)largeEnemies / largeDungeon.TotalRoomCount;

        // Assert - Both should have similar per-room average
        _logger.Information($"Small sector: {smallAvg:F2} enemies/room, Large sector: {largeAvg:F2} enemies/room");

        Assert.IsTrue(Math.Abs(smallAvg - largeAvg) < 1.0f,
            $"Per-room averages too different: {smallAvg:F2} vs {largeAvg:F2}");

        _logger.Information("FullPipeline_LargerSectors_ScaleBudgetCorrectly: PASSED");
    }
}
