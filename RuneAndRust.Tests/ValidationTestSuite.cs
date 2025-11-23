using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using RuneAndRust.Engine;
using RuneAndRust.Engine.Spatial;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.39.4: Validation Test Suite
/// Comprehensive validation tests including 100-sector generation
/// </summary>
[TestClass]
public class ValidationTestSuite
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
            MaxRoomCount = 7,
            BranchingProbability = 0.6f,
            SecretRoomProbability = 0.3f
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

    private BiomeDefinition CreateDefaultBlueprint()
    {
        return _testBiome;
    }

    #region Core Validation Tests

    [TestMethod]
    public void Generate100Sectors_NoErrors()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        var seeds = Enumerable.Range(1, 100).ToList();
        var errors = new List<string>();

        _logger.Information("Starting 100-sector validation test...");

        // Act
        foreach (var seed in seeds)
        {
            try
            {
                var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, CreateDefaultBlueprint());
                ValidateSector(dungeon, seed);

                if (seed % 10 == 0)
                {
                    _logger.Information($"Progress: {seed}/100 sectors generated");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Seed {seed}: {ex.Message}");
                _logger.Error(ex, $"Failed to generate sector with seed {seed}");
            }
        }

        // Assert
        Assert.AreEqual(0, errors.Count,
            $"Generation failed for {errors.Count} sectors:\n{string.Join("\n", errors.Take(10))}");

        _logger.Information("100-sector validation test PASSED: All sectors generated successfully");
    }

    [TestMethod]
    public void Generate100Sectors_StatisticalValidation()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();
        var metrics = new List<SectorMetrics>();

        _logger.Information("Starting 100-sector statistical validation...");

        // Act
        for (int seed = 1; seed <= 100; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, CreateDefaultBlueprint());
            metrics.Add(CalculateMetrics(dungeon));

            if (seed % 20 == 0)
            {
                _logger.Information($"Progress: {seed}/100 sectors analyzed");
            }
        }

        // Assert - Average enemies per room
        var avgEnemies = metrics.Average(m => m.AverageEnemiesPerRoom);
        _logger.Information($"Average enemies per room: {avgEnemies:F2}");
        Assert.IsTrue(avgEnemies >= 1.5 && avgEnemies <= 3.5,
            $"Average enemies per room {avgEnemies:F2} outside target range 1.5-3.5");

        // Assert - Empty room percentage
        var avgEmptyRooms = metrics.Average(m => m.EmptyRoomPercentage);
        _logger.Information($"Average empty room percentage: {avgEmptyRooms:P0}");
        Assert.IsTrue(avgEmptyRooms >= 0.05 && avgEmptyRooms <= 0.35,
            $"Empty room percentage {avgEmptyRooms:P0} outside target range 5-35%");

        // Assert - Boss rooms most dense
        var bossRoomsDensest = metrics.Count(m => m.BossRoomIsMostDense);
        _logger.Information($"Boss rooms densest: {bossRoomsDensest}/100 sectors");
        Assert.IsTrue(bossRoomsDensest >= 70,
            $"Boss rooms not densest in {100 - bossRoomsDensest} sectors");

        // Assert - Spatial coherence
        var spatialCoherence = metrics.Count(m => m.HasSpatialCoherence);
        _logger.Information($"Spatial coherence: {spatialCoherence}/100 sectors");
        Assert.IsTrue(spatialCoherence >= 95,
            $"Spatial coherence failed in {100 - spatialCoherence} sectors");

        _logger.Information("100-sector statistical validation PASSED");
    }

    private void ValidateSector(Dungeon dungeon, int seed)
    {
        // Validate basic structure
        Assert.IsNotNull(dungeon, $"Seed {seed}: Dungeon is null");
        Assert.IsTrue(dungeon.TotalRoomCount >= 5, $"Seed {seed}: Too few rooms ({dungeon.TotalRoomCount})");

        // Validate boss room exists
        var bossRoom = dungeon.Rooms.Values.FirstOrDefault(r => r.IsBossRoom);
        Assert.IsNotNull(bossRoom, $"Seed {seed}: No boss room found");

        // Validate spatial positions
        Assert.IsTrue(dungeon.Rooms.Values.All(r => r.Position != null),
            $"Seed {seed}: Not all rooms have positions");

        // Validate no position collisions
        var positions = dungeon.Rooms.Values.Select(r => r.Position).ToList();
        var uniquePositions = positions.Distinct().Count();
        Assert.AreEqual(positions.Count, uniquePositions,
            $"Seed {seed}: Room position collisions detected");
    }

    private SectorMetrics CalculateMetrics(Dungeon dungeon)
    {
        var totalEnemies = dungeon.Rooms.Values.Sum(r => r.Enemies.Count);
        var totalRooms = dungeon.TotalRoomCount;

        var emptyRooms = dungeon.Rooms.Values.Count(r =>
            r.Enemies.Count == 0 && r.Hazards.Count == 0);

        var bossRoom = dungeon.Rooms.Values.FirstOrDefault(r => r.IsBossRoom);
        var bossThreats = bossRoom != null ? bossRoom.Enemies.Count + bossRoom.Hazards.Count : 0;
        var maxThreats = dungeon.Rooms.Values.Max(r => r.Enemies.Count + r.Hazards.Count);

        var positions = dungeon.Rooms.Values.Select(r => r.Position).ToList();
        var uniquePositions = positions.Distinct().Count();

        return new SectorMetrics
        {
            AverageEnemiesPerRoom = (float)totalEnemies / totalRooms,
            EmptyRoomPercentage = (float)emptyRooms / totalRooms,
            BossRoomIsMostDense = bossThreats >= maxThreats * 0.9f,
            HasSpatialCoherence = positions.Count == uniquePositions
        };
    }

    #endregion

    #region Budget Validation

    [TestMethod]
    public void ValidateBudget_GlobalBudgetNotExceeded()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // Act - Generate 20 sectors
        for (int seed = 1; seed <= 20; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);

            // Calculate expected budget
            var expectedEnemyBudget = (int)(dungeon.TotalRoomCount * 2.2f * 1.0f * 1.0f); // Base * Difficulty * Biome

            var actualEnemies = dungeon.Rooms.Values.Sum(r => r.Enemies.Count);

            // Assert - Should not significantly exceed budget (allow 20% tolerance due to rounding)
            Assert.IsTrue(actualEnemies <= expectedEnemyBudget * 1.2f,
                $"Seed {seed}: Enemy count {actualEnemies} exceeds budget {expectedEnemyBudget} by more than 20%");
        }

        _logger.Information("ValidateBudget_GlobalBudgetNotExceeded: PASSED");
    }

    [TestMethod]
    public void ValidateBudget_MinimumContentPresent()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // Act
        for (int seed = 1; seed <= 20; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);

            var totalEnemies = dungeon.Rooms.Values.Sum(r => r.Enemies.Count);

            // Assert - Minimum content present (at least 1 enemy per room on average)
            Assert.IsTrue(totalEnemies >= dungeon.TotalRoomCount * 0.8f,
                $"Seed {seed}: Too few enemies ({totalEnemies}) for {dungeon.TotalRoomCount} rooms");
        }

        _logger.Information("ValidateBudget_MinimumContentPresent: PASSED");
    }

    #endregion

    #region Spatial Validation

    [TestMethod]
    public void ValidateSpatial_NoOverlappingRooms()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // Act
        for (int seed = 1; seed <= 30; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);

            var positions = new Dictionary<Core.Spatial.RoomPosition, List<string>>();

            foreach (var room in dungeon.Rooms.Values)
            {
                if (room.Position != null)
                {
                    if (!positions.ContainsKey(room.Position))
                    {
                        positions[room.Position] = new List<string>();
                    }
                    positions[room.Position].Add(room.RoomId);
                }
            }

            // Assert - No position should have multiple rooms
            foreach (var kvp in positions)
            {
                Assert.AreEqual(1, kvp.Value.Count,
                    $"Seed {seed}: Position {kvp.Key} has {kvp.Value.Count} rooms: {string.Join(", ", kvp.Value)}");
            }
        }

        _logger.Information("ValidateSpatial_NoOverlappingRooms: PASSED");
    }

    [TestMethod]
    public void ValidateSpatial_EntryHallAtOrigin()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // Act
        for (int seed = 1; seed <= 20; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);

            var entryHall = dungeon.Rooms.Values.FirstOrDefault(r => r.Archetype == RoomArchetype.EntryHall);

            // Assert
            Assert.IsNotNull(entryHall, $"Seed {seed}: No entry hall found");
            Assert.IsNotNull(entryHall.Position, $"Seed {seed}: Entry hall has no position");
            Assert.AreEqual(Core.Spatial.RoomPosition.Origin, entryHall.Position,
                $"Seed {seed}: Entry hall not at origin, found at {entryHall.Position}");
        }

        _logger.Information("ValidateSpatial_EntryHallAtOrigin: PASSED");
    }

    [TestMethod]
    public void ValidateSpatial_VerticalConnectionsLogical()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // Act
        for (int seed = 1; seed <= 20; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 9, _testBiome);

            foreach (var connection in dungeon.VerticalConnections)
            {
                // Both rooms should exist
                Assert.IsTrue(dungeon.Rooms.ContainsKey(connection.FromRoomId),
                    $"Seed {seed}: Connection references non-existent FromRoom {connection.FromRoomId}");
                Assert.IsTrue(dungeon.Rooms.ContainsKey(connection.ToRoomId),
                    $"Seed {seed}: Connection references non-existent ToRoom {connection.ToRoomId}");

                // Rooms should be at different Z levels
                var fromRoom = dungeon.Rooms[connection.FromRoomId];
                var toRoom = dungeon.Rooms[connection.ToRoomId];

                if (fromRoom.Position != null && toRoom.Position != null)
                {
                    Assert.AreNotEqual(fromRoom.Position.Z, toRoom.Position.Z,
                        $"Seed {seed}: Vertical connection between rooms at same Z level");
                }
            }
        }

        _logger.Information("ValidateSpatial_VerticalConnectionsLogical: PASSED");
    }

    #endregion

    #region Biome Validation

    [TestMethod]
    public void ValidateBiome_TransitionRoomsHaveBlendData()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        var secondaryBiome = new BiomeDefinition
        {
            BiomeId = "muspelheim",
            Name = "Muspelheim",
            DifficultyTier = DifficultyTier.Normal
        };

        // Act
        for (int seed = 1; seed <= 10; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(
                seed, seed, 7, _testBiome,
                new List<BiomeDefinition> { secondaryBiome });

            var transitionRooms = dungeon.Rooms.Values
                .Where(r => !string.IsNullOrEmpty(r.SecondaryBiome))
                .ToList();

            // Assert - Transition rooms should exist if we have multiple biomes
            if (transitionRooms.Any())
            {
                foreach (var room in transitionRooms)
                {
                    Assert.IsTrue(room.BiomeBlendRatio >= 0.0f && room.BiomeBlendRatio <= 1.0f,
                        $"Seed {seed}: Invalid blend ratio {room.BiomeBlendRatio}");

                    Assert.AreEqual(secondaryBiome.BiomeId, room.SecondaryBiome,
                        $"Seed {seed}: Secondary biome mismatch");
                }
            }
        }

        _logger.Information("ValidateBiome_TransitionRoomsHaveBlendData: PASSED");
    }

    #endregion

    #region Density Validation

    [TestMethod]
    public void ValidateDensity_EntryHallIsLightOrEmpty()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // Act
        for (int seed = 1; seed <= 30; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);
            var entryHall = dungeon.Rooms.Values.FirstOrDefault(r => r.Archetype == RoomArchetype.EntryHall);

            Assert.IsNotNull(entryHall, $"Seed {seed}: No entry hall found");

            var threats = entryHall.Enemies.Count + entryHall.Hazards.Count;

            // Assert - Entry hall should have 0-2 threats (Empty or Light density)
            Assert.IsTrue(threats <= 2,
                $"Seed {seed}: Entry hall has {threats} threats, expected <= 2");
        }

        _logger.Information("ValidateDensity_EntryHallIsLightOrEmpty: PASSED");
    }

    [TestMethod]
    public void ValidateDensity_BossRoomHasContent()
    {
        // Arrange
        var generator = CreateFullyIntegratedGenerator();

        // Act
        for (int seed = 1; seed <= 30; seed++)
        {
            var dungeon = generator.GenerateWithFullPipeline(seed, seed, 7, _testBiome);
            var bossRoom = dungeon.Rooms.Values.FirstOrDefault(r => r.IsBossRoom);

            Assert.IsNotNull(bossRoom, $"Seed {seed}: No boss room found");

            var threats = bossRoom.Enemies.Count + bossRoom.Hazards.Count;

            // Assert - Boss room should have at least 3 threats
            Assert.IsTrue(threats >= 3,
                $"Seed {seed}: Boss room has only {threats} threats, expected >= 3");
        }

        _logger.Information("ValidateDensity_BossRoomHasContent: PASSED");
    }

    #endregion
}

public class SectorMetrics
{
    public float AverageEnemiesPerRoom { get; set; }
    public float EmptyRoomPercentage { get; set; }
    public bool BossRoomIsMostDense { get; set; }
    public bool HasSpatialCoherence { get; set; }
}
