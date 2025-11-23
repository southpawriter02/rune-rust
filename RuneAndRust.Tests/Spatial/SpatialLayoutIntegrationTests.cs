using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using RuneAndRust.Engine;
using RuneAndRust.Engine.Spatial;
using Serilog;

namespace RuneAndRust.Tests.Spatial;

/// <summary>
/// Integration tests for the full 3D spatial pipeline (v0.39.1)
/// Tests the complete flow: Graph → 3D Layout → Validation → Room Instantiation
/// </summary>
[TestClass]
public class SpatialLayoutIntegrationTests
{
    private ILogger _logger = null!;
    private TemplateLibrary _templateLibrary = null!;
    private DungeonGenerator _generator = null!;
    private SpatialLayoutService _layoutService = null!;
    private SpatialValidationService _validationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        _templateLibrary = CreateTestTemplateLibrary();
        _layoutService = new SpatialLayoutService(_logger);
        _validationService = new SpatialValidationService(_logger);
        _generator = new DungeonGenerator(
            _templateLibrary,
            spatialLayoutService: _layoutService,
            spatialValidationService: _validationService);
    }

    #region Helper Methods

    private TemplateLibrary CreateTestTemplateLibrary()
    {
        var library = new TemplateLibrary();

        // Create test templates for different node types
        var entryTemplate = new RoomTemplate
        {
            TemplateId = "entry_hall",
            Name = "Entry Hall",
            MaxConnectionPoints = 4,
            NameTemplates = new List<string> { "Entry Hall" },
            DescriptionTemplates = new List<string> { "A starting chamber" },
            Adjectives = new List<string> { "Ancient" },
            Details = new List<string> { "Dust covers the floor" }
        };

        var corridorTemplate = new RoomTemplate
        {
            TemplateId = "corridor",
            Name = "Corridor",
            MaxConnectionPoints = 4,
            NameTemplates = new List<string> { "Corridor" },
            DescriptionTemplates = new List<string> { "A passageway" },
            Adjectives = new List<string> { "Dark" },
            Details = new List<string> { "The walls are corroded" }
        };

        var bossTemplate = new RoomTemplate
        {
            TemplateId = "boss_arena",
            Name = "Boss Arena",
            MaxConnectionPoints = 2,
            NameTemplates = new List<string> { "Boss Arena" },
            DescriptionTemplates = new List<string> { "A final chamber" },
            Adjectives = new List<string> { "Massive" },
            Details = new List<string> { "Ancient machinery hums" }
        };

        // Use reflection to add templates to private dictionary
        var templatesField = typeof(TemplateLibrary).GetField("_templates",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var templatesDict = (Dictionary<string, RoomTemplate>)templatesField!.GetValue(library)!;

        templatesDict["entry_hall"] = entryTemplate;
        templatesDict["corridor"] = corridorTemplate;
        templatesDict["boss_arena"] = bossTemplate;

        return library;
    }

    #endregion

    #region Full Pipeline Tests

    [TestMethod]
    public void FullPipeline_SmallDungeon_GeneratesSuccessfully()
    {
        // Arrange
        int seed = 12345;
        int targetRoomCount = 5;
        string biome = "the_roots";

        // Act
        var dungeon = _generator.GenerateComplete(
            dungeonId: 1,
            seed: seed,
            targetRoomCount: targetRoomCount,
            biome: biome);

        // Assert
        Assert.IsNotNull(dungeon);
        Assert.IsTrue(dungeon.TotalRoomCount >= 3, "Should have at least start, main, boss");
        Assert.IsNotNull(dungeon.StartRoomId);
        Assert.IsNotNull(dungeon.BossRoomId);

        // Verify spatial properties are set
        Assert.IsTrue(dungeon.RoomPositions.Count > 0, "Should have room positions");
        var startRoom = dungeon.GetStartRoom();
        Assert.IsNotNull(startRoom);
        Assert.AreEqual(RoomPosition.Origin, startRoom.Position);
        Assert.AreEqual(VerticalLayer.GroundLevel, startRoom.Layer);
    }

    [TestMethod]
    public void FullPipeline_MediumDungeon_ValidatesWithoutCriticalIssues()
    {
        // Arrange
        int seed = 54321;
        int targetRoomCount = 10;
        string biome = "the_roots";

        // Act
        var dungeon = _generator.GenerateComplete(
            dungeonId: 2,
            seed: seed,
            targetRoomCount: targetRoomCount,
            biome: biome);

        // Assert
        Assert.IsNotNull(dungeon);

        // Validate spatial coherence
        var issues = _validationService.ValidateSector(
            dungeon.RoomPositions,
            dungeon.VerticalConnections,
            CreateGraphFromDungeon(dungeon));

        var criticalIssues = issues.Where(i => i.Severity == "Critical").ToList();
        Assert.AreEqual(0, criticalIssues.Count,
            $"Should have no critical issues. Found: {string.Join(", ", criticalIssues.Select(i => i.Description))}");
    }

    [TestMethod]
    public void FullPipeline_EntryHallAlwaysAtOrigin()
    {
        // Arrange - test multiple seeds
        var seeds = new[] { 111, 222, 333, 444, 555 };

        foreach (var seed in seeds)
        {
            // Act
            var dungeon = _generator.GenerateComplete(
                dungeonId: seed,
                seed: seed,
                targetRoomCount: 5,
                biome: "the_roots");

            // Assert
            var startRoom = dungeon.GetStartRoom();
            Assert.IsNotNull(startRoom, $"Seed {seed}: No start room found");
            Assert.AreEqual(RoomPosition.Origin, startRoom.Position,
                $"Seed {seed}: Entry hall not at origin, found at {startRoom.Position}");
        }
    }

    [TestMethod]
    public void FullPipeline_AllRoomsReachable()
    {
        // Arrange
        int seed = 99999;
        int targetRoomCount = 15;

        // Act
        var dungeon = _generator.GenerateComplete(
            dungeonId: 3,
            seed: seed,
            targetRoomCount: targetRoomCount,
            biome: "the_roots");

        var graph = CreateGraphFromDungeon(dungeon);
        var startRoomId = dungeon.StartRoomId;

        // Assert - check every room is reachable
        foreach (var roomId in dungeon.Rooms.Keys)
        {
            var isReachable = _validationService.IsReachableFromOrigin(
                roomId,
                dungeon.RoomPositions,
                dungeon.VerticalConnections,
                graph);

            Assert.IsTrue(isReachable,
                $"Room {roomId} at position {dungeon.RoomPositions.GetValueOrDefault(roomId)} is not reachable from entry hall");
        }
    }

    [TestMethod]
    public void FullPipeline_NoRoomOverlaps()
    {
        // Arrange
        int seed = 77777;
        int targetRoomCount = 20;

        // Act
        var dungeon = _generator.GenerateComplete(
            dungeonId: 4,
            seed: seed,
            targetRoomCount: targetRoomCount,
            biome: "the_roots");

        // Assert - check for overlaps
        var occupiedPositions = new HashSet<RoomPosition>();
        foreach (var position in dungeon.RoomPositions.Values)
        {
            Assert.IsFalse(occupiedPositions.Contains(position),
                $"Position {position} is occupied by multiple rooms");
            occupiedPositions.Add(position);
        }
    }

    [TestMethod]
    public void FullPipeline_AllRoomsWithinLayerBounds()
    {
        // Arrange
        int seed = 11111;
        int targetRoomCount = 10;

        // Act
        var dungeon = _generator.GenerateComplete(
            dungeonId: 5,
            seed: seed,
            targetRoomCount: targetRoomCount,
            biome: "the_roots");

        // Assert - check all rooms within -3 to +3
        foreach (var kvp in dungeon.RoomPositions)
        {
            var roomId = kvp.Key;
            var position = kvp.Value;

            Assert.IsTrue(position.Z >= -3,
                $"Room {roomId} below minimum layer: Z={position.Z}");
            Assert.IsTrue(position.Z <= 3,
                $"Room {roomId} above maximum layer: Z={position.Z}");
        }
    }

    [TestMethod]
    public void FullPipeline_VerticalConnectionsValid()
    {
        // Arrange
        int seed = 22222;
        int targetRoomCount = 12;

        // Act
        var dungeon = _generator.GenerateComplete(
            dungeonId: 6,
            seed: seed,
            targetRoomCount: targetRoomCount,
            biome: "the_roots");

        // Assert - validate all vertical connections
        foreach (var connection in dungeon.VerticalConnections)
        {
            // Both rooms should exist
            Assert.IsTrue(dungeon.Rooms.ContainsKey(connection.FromRoomId),
                $"Vertical connection references non-existent FROM room: {connection.FromRoomId}");
            Assert.IsTrue(dungeon.Rooms.ContainsKey(connection.ToRoomId),
                $"Vertical connection references non-existent TO room: {connection.ToRoomId}");

            // Should connect different Z levels
            var fromPos = dungeon.RoomPositions[connection.FromRoomId];
            var toPos = dungeon.RoomPositions[connection.ToRoomId];

            Assert.AreNotEqual(fromPos.Z, toPos.Z,
                $"Vertical connection {connection.ConnectionId} connects rooms at same Z level: {fromPos.Z}");

            // Should be directly above/below
            Assert.IsTrue(fromPos.IsDirectlyAboveOrBelow(toPos),
                $"Vertical connection {connection.ConnectionId} connects non-aligned rooms: {fromPos} → {toPos}");
        }
    }

    [TestMethod]
    public void FullPipeline_PerformanceBenchmark_CompletesQuickly()
    {
        // Arrange
        int seed = 33333;
        int targetRoomCount = 10;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var dungeon = _generator.GenerateComplete(
            dungeonId: 7,
            seed: seed,
            targetRoomCount: targetRoomCount,
            biome: "the_roots");

        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500,
            $"Generation took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms for 10-room dungeon");

        _logger.Information(
            "Performance benchmark: Generated {RoomCount} rooms in {ElapsedMs}ms",
            dungeon.TotalRoomCount,
            stopwatch.ElapsedMilliseconds);
    }

    [TestMethod]
    public void FullPipeline_BossRoomTendsToBeDeeper()
    {
        // Arrange - test multiple dungeons
        var bossDepthCounts = new Dictionary<string, int>
        {
            ["negative"] = 0, // Below ground
            ["zero"] = 0,     // Ground level
            ["positive"] = 0  // Above ground
        };

        for (int i = 0; i < 10; i++)
        {
            // Act
            var dungeon = _generator.GenerateComplete(
                dungeonId: 100 + i,
                seed: 1000 + i,
                targetRoomCount: 8,
                biome: "the_roots");

            var bossRoom = dungeon.GetBossRoom();
            if (bossRoom != null)
            {
                var bossZ = bossRoom.Position.Z;
                if (bossZ < 0)
                    bossDepthCounts["negative"]++;
                else if (bossZ == 0)
                    bossDepthCounts["zero"]++;
                else
                    bossDepthCounts["positive"]++;
            }
        }

        // Assert - boss rooms should tend to be at negative Z (below ground)
        _logger.Information(
            "Boss room depth distribution: Negative={Negative}, Zero={Zero}, Positive={Positive}",
            bossDepthCounts["negative"],
            bossDepthCounts["zero"],
            bossDepthCounts["positive"]);

        // At least 30% should be below ground (due to 60% vertical movement chance for boss rooms)
        Assert.IsTrue(bossDepthCounts["negative"] >= 3,
            $"Expected at least 30% of boss rooms below ground, found {bossDepthCounts["negative"]}/10");
    }

    [TestMethod]
    public void FullPipeline_VerticalVariety_MultipleLayersUsed()
    {
        // Arrange
        int seed = 44444;
        int targetRoomCount = 20; // Larger dungeon for more variety

        // Act
        var dungeon = _generator.GenerateComplete(
            dungeonId: 8,
            seed: seed,
            targetRoomCount: targetRoomCount,
            biome: "the_roots");

        // Assert - count unique Z levels used
        var uniqueLayers = dungeon.RoomPositions.Values
            .Select(p => p.Z)
            .Distinct()
            .Count();

        Assert.IsTrue(uniqueLayers >= 2,
            $"Expected at least 2 vertical layers in 20-room dungeon, found {uniqueLayers}");

        _logger.Information(
            "Vertical variety: {Layers} different layers used in {RoomCount} rooms",
            uniqueLayers,
            dungeon.TotalRoomCount);
    }

    [TestMethod]
    public void FullPipeline_SameSeedProducesSameLayout()
    {
        // Arrange
        int seed = 55555;

        // Act
        var dungeon1 = _generator.GenerateComplete(
            dungeonId: 9,
            seed: seed,
            targetRoomCount: 8,
            biome: "the_roots");

        var dungeon2 = _generator.GenerateComplete(
            dungeonId: 10,
            seed: seed,
            targetRoomCount: 8,
            biome: "the_roots");

        // Assert - room counts should match
        Assert.AreEqual(dungeon1.TotalRoomCount, dungeon2.TotalRoomCount);

        // Positions for corresponding node IDs should match
        // (Note: Room IDs will differ due to different dungeonIds, but node ordering should match)
        var positions1 = dungeon1.RoomPositions.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
        var positions2 = dungeon2.RoomPositions.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();

        CollectionAssert.AreEqual(positions1, positions2,
            "Same seed should produce same spatial layout");
    }

    #endregion

    #region Helper Methods for Graph Reconstruction

    /// <summary>
    /// Creates a DungeonGraph from a generated Dungeon for validation purposes
    /// This is a workaround since the graph isn't stored in the dungeon
    /// </summary>
    private DungeonGraph CreateGraphFromDungeon(Dungeon dungeon)
    {
        var graph = new DungeonGraph();
        var template = _templateLibrary.GetTemplateById("corridor") ?? CreateFallbackTemplate();

        // Create nodes for each room
        var nodeMap = new Dictionary<string, DungeonNode>();

        foreach (var room in dungeon.Rooms.Values)
        {
            var node = new DungeonNode
            {
                Id = int.Parse(room.RoomId.Split('_').Last().TrimStart('n')),
                Template = template,
                Type = room.GeneratedNodeType,
                Depth = room.Position.ManhattanDistanceHorizontal(RoomPosition.Origin)
            };

            graph.AddNode(node);
            nodeMap[room.RoomId] = node;
        }

        // Create edges from room exits
        var edgesField = typeof(DungeonGraph).GetField("_edges",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var edgesList = (List<DungeonEdge>)edgesField!.GetValue(graph)!;

        foreach (var room in dungeon.Rooms.Values)
        {
            if (!nodeMap.TryGetValue(room.RoomId, out var fromNode))
                continue;

            foreach (var exit in room.Exits)
            {
                var targetRoomId = exit.Value;
                if (nodeMap.TryGetValue(targetRoomId, out var toNode))
                {
                    edgesList.Add(new DungeonEdge
                    {
                        From = fromNode,
                        To = toNode,
                        FromDirection = ParseDirection(exit.Key),
                        IsBidirectional = true
                    });
                }
            }
        }

        return graph;
    }

    private Direction ParseDirection(string directionString)
    {
        return directionString.ToLower() switch
        {
            "north" => Direction.North,
            "south" => Direction.South,
            "east" => Direction.East,
            "west" => Direction.West,
            _ => Direction.North
        };
    }

    private RoomTemplate CreateFallbackTemplate()
    {
        return new RoomTemplate
        {
            TemplateId = "fallback",
            Name = "Fallback",
            MaxConnectionPoints = 4,
            NameTemplates = new List<string> { "Room" },
            DescriptionTemplates = new List<string> { "A room" },
            Adjectives = new List<string> { "Test" },
            Details = new List<string> { "Test detail" }
        };
    }

    #endregion
}
