using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using RuneAndRust.Engine.Spatial;
using Serilog;

namespace RuneAndRust.Tests.Spatial;

/// <summary>
/// Unit tests for SpatialLayoutService (v0.39.1)
/// Tests graph-to-3D conversion, overlap detection, and vertical connection generation
/// </summary>
[TestClass]
public class SpatialLayoutServiceTests
{
    private SpatialLayoutService _service = null!;
    private ILogger _logger = null!;

    [TestInitialize]
    public void Setup()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        _service = new SpatialLayoutService(_logger);
    }

    #region Helper Methods

    private RoomTemplate CreateTestTemplate(string id = "test_template")
    {
        return new RoomTemplate
        {
            TemplateId = id,
            Name = "Test Template",
            MaxConnectionPoints = 4,
            NameTemplates = new List<string> { "Test Room" },
            DescriptionTemplates = new List<string> { "A test room" },
            Adjectives = new List<string> { "Ancient" },
            Details = new List<string> { "Details here" }
        };
    }

    private DungeonGraph CreateSimpleLinearGraph(int roomCount = 3)
    {
        var graph = new DungeonGraph();
        var template = CreateTestTemplate();

        var nodes = new List<DungeonNode>();

        for (int i = 0; i < roomCount; i++)
        {
            var nodeType = i == 0 ? NodeType.Start :
                          i == roomCount - 1 ? NodeType.Boss :
                          NodeType.Main;

            var node = new DungeonNode
            {
                Template = template,
                Type = nodeType,
                Depth = i,
                Name = $"Node_{i}"
            };

            graph.AddNode(node);
            nodes.Add(node);
        }

        // Connect nodes linearly
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            var edge = new DungeonEdge
            {
                From = nodes[i],
                To = nodes[i + 1],
                FromDirection = Direction.North,
                ToDirection = Direction.South,
                Type = EdgeType.Normal,
                IsBidirectional = true
            };

            // Use reflection to add edge directly to avoid public AddEdge issues
            var edgesField = typeof(DungeonGraph).GetField("_edges",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var edgesList = (List<DungeonEdge>)edgesField!.GetValue(graph)!;
            edgesList.Add(edge);
        }

        return graph;
    }

    private DungeonGraph CreateBranchingGraph()
    {
        var graph = new DungeonGraph();
        var template = CreateTestTemplate();

        // Create nodes
        var start = new DungeonNode { Template = template, Type = NodeType.Start, Depth = 0, Name = "Start" };
        var main1 = new DungeonNode { Template = template, Type = NodeType.Main, Depth = 1, Name = "Main1" };
        var branch1 = new DungeonNode { Template = template, Type = NodeType.Branch, Depth = 2, Name = "Branch1" };
        var secret1 = new DungeonNode { Template = template, Type = NodeType.Secret, Depth = 2, Name = "Secret1" };
        var boss = new DungeonNode { Template = template, Type = NodeType.Boss, Depth = 3, Name = "Boss" };

        graph.AddNode(start);
        graph.AddNode(main1);
        graph.AddNode(branch1);
        graph.AddNode(secret1);
        graph.AddNode(boss);

        // Connect nodes with edges
        var edgesField = typeof(DungeonGraph).GetField("_edges",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var edgesList = (List<DungeonEdge>)edgesField!.GetValue(graph)!;

        // Start -> Main1
        edgesList.Add(new DungeonEdge
        {
            From = start,
            To = main1,
            FromDirection = Direction.North,
            Type = EdgeType.Normal,
            IsBidirectional = true
        });

        // Main1 -> Branch1
        edgesList.Add(new DungeonEdge
        {
            From = main1,
            To = branch1,
            FromDirection = Direction.East,
            Type = EdgeType.Normal,
            IsBidirectional = true
        });

        // Main1 -> Secret1
        edgesList.Add(new DungeonEdge
        {
            From = main1,
            To = secret1,
            FromDirection = Direction.West,
            Type = EdgeType.Secret,
            IsBidirectional = true
        });

        // Main1 -> Boss
        edgesList.Add(new DungeonEdge
        {
            From = main1,
            To = boss,
            FromDirection = Direction.North,
            Type = EdgeType.Normal,
            IsBidirectional = true
        });

        return graph;
    }

    #endregion

    #region ConvertGraphTo3DLayout Tests

    [TestMethod]
    public void ConvertGraphTo3DLayout_SimpleLinearGraph_PlacesAllRooms()
    {
        // Arrange
        var graph = CreateSimpleLinearGraph(5);

        // Act
        var positions = _service.ConvertGraphTo3DLayout(graph, seed: 42);

        // Assert
        Assert.AreEqual(5, positions.Count);
    }

    [TestMethod]
    public void ConvertGraphTo3DLayout_AnyGraph_EntryHallAtOrigin()
    {
        // Arrange
        var graph = CreateSimpleLinearGraph(3);

        // Act
        var positions = _service.ConvertGraphTo3DLayout(graph, seed: 42);

        // Assert
        var startNodeId = graph.StartNode!.Id.ToString();
        Assert.IsTrue(positions.ContainsKey(startNodeId));
        Assert.AreEqual(RoomPosition.Origin, positions[startNodeId]);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ConvertGraphTo3DLayout_GraphWithoutStartNode_ThrowsException()
    {
        // Arrange
        var graph = new DungeonGraph();
        var template = CreateTestTemplate();
        graph.AddNode(new DungeonNode { Template = template, Type = NodeType.Main });

        // Act & Assert
        _service.ConvertGraphTo3DLayout(graph, seed: 42);
    }

    [TestMethod]
    public void ConvertGraphTo3DLayout_BranchingGraph_PlacesAllRooms()
    {
        // Arrange
        var graph = CreateBranchingGraph();

        // Act
        var positions = _service.ConvertGraphTo3DLayout(graph, seed: 42);

        // Assert
        Assert.AreEqual(5, positions.Count);
    }

    [TestMethod]
    public void ConvertGraphTo3DLayout_SameSeed_ProducesSameLayout()
    {
        // Arrange
        var graph = CreateSimpleLinearGraph(5);
        var seed = 12345;

        // Act
        var positions1 = _service.ConvertGraphTo3DLayout(graph, seed);

        // Recreate graph with same structure
        var graph2 = CreateSimpleLinearGraph(5);
        var positions2 = _service.ConvertGraphTo3DLayout(graph2, seed);

        // Assert
        Assert.AreEqual(positions1.Count, positions2.Count);

        // Compare positions (need to match by node index since node IDs may differ)
        var pos1List = positions1.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
        var pos2List = positions2.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();

        CollectionAssert.AreEqual(pos1List, pos2List);
    }

    [TestMethod]
    public void ConvertGraphTo3DLayout_DifferentSeeds_ProducesDifferentLayouts()
    {
        // Arrange
        var graph1 = CreateSimpleLinearGraph(5);
        var graph2 = CreateSimpleLinearGraph(5);

        // Act
        var positions1 = _service.ConvertGraphTo3DLayout(graph1, seed: 111);
        var positions2 = _service.ConvertGraphTo3DLayout(graph2, seed: 999);

        // Assert
        var pos1List = positions1.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
        var pos2List = positions2.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();

        // At least one position should differ (given randomness in vertical placement)
        CollectionAssert.AreNotEqual(pos1List, pos2List);
    }

    [TestMethod]
    public void ConvertGraphTo3DLayout_LargeGraph_CompletesInReasonableTime()
    {
        // Arrange
        var graph = CreateSimpleLinearGraph(20);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var positions = _service.ConvertGraphTo3DLayout(graph, seed: 42);
        stopwatch.Stop();

        // Assert
        Assert.AreEqual(20, positions.Count);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000,
            $"Conversion took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    [TestMethod]
    public void ConvertGraphTo3DLayout_RespectsBounds_AllWithinValidRange()
    {
        // Arrange
        var graph = CreateSimpleLinearGraph(10);

        // Act
        var positions = _service.ConvertGraphTo3DLayout(graph, seed: 42);

        // Assert
        foreach (var kvp in positions)
        {
            Assert.IsTrue(kvp.Value.Z >= -3, $"Room {kvp.Key} Z={kvp.Value.Z} below minimum -3");
            Assert.IsTrue(kvp.Value.Z <= 3, $"Room {kvp.Key} Z={kvp.Value.Z} above maximum +3");
        }
    }

    #endregion

    #region ValidateNoOverlaps Tests

    [TestMethod]
    public void ValidateNoOverlaps_UniquePositions_ReturnsTrue()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(1, 0, 0),
            ["room3"] = new RoomPosition(0, 1, 0),
            ["room4"] = new RoomPosition(0, 0, 1)
        };
        var templates = new Dictionary<string, RoomTemplate>();

        // Act
        var result = _service.ValidateNoOverlaps(positions, templates);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ValidateNoOverlaps_DuplicatePosition_ReturnsFalse()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(1, 0, 0),
            ["room3"] = new RoomPosition(0, 0, 0) // Duplicate!
        };
        var templates = new Dictionary<string, RoomTemplate>();

        // Act
        var result = _service.ValidateNoOverlaps(positions, templates);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateNoOverlaps_EmptyDictionary_ReturnsTrue()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>();
        var templates = new Dictionary<string, RoomTemplate>();

        // Act
        var result = _service.ValidateNoOverlaps(positions, templates);

        // Assert
        Assert.IsTrue(result);
    }

    #endregion

    #region GenerateVerticalConnections Tests

    [TestMethod]
    public void GenerateVerticalConnections_RoomsDirectlyAbove_CreatesConnection()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(0, 0, 1) // Directly above
        };
        var rng = new Random(42);

        // Act
        var connections = _service.GenerateVerticalConnections(positions, rng);

        // Assert
        Assert.AreEqual(1, connections.Count);
        Assert.IsTrue(
            (connections[0].FromRoomId == "room1" && connections[0].ToRoomId == "room2") ||
            (connections[0].FromRoomId == "room2" && connections[0].ToRoomId == "room1"));
    }

    [TestMethod]
    public void GenerateVerticalConnections_RoomsNotAligned_NoConnection()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(1, 0, 1) // Different X, not directly above
        };
        var rng = new Random(42);

        // Act
        var connections = _service.GenerateVerticalConnections(positions, rng);

        // Assert
        Assert.AreEqual(0, connections.Count);
    }

    [TestMethod]
    public void GenerateVerticalConnections_RoomsTooFarApart_NoConnection()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(0, 0, 4) // 4 levels apart, exceeds max of 3
        };
        var rng = new Random(42);

        // Act
        var connections = _service.GenerateVerticalConnections(positions, rng);

        // Assert
        Assert.AreEqual(0, connections.Count);
    }

    [TestMethod]
    public void GenerateVerticalConnections_MultipleStackedRooms_CreatesMultipleConnections()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(0, 0, 1),
            ["room3"] = new RoomPosition(0, 0, 2)
        };
        var rng = new Random(42);

        // Act
        var connections = _service.GenerateVerticalConnections(positions, rng);

        // Assert
        Assert.AreEqual(2, connections.Count); // room1-room2 and room2-room3
    }

    [TestMethod]
    public void GenerateVerticalConnections_AllHorizontal_NoConnections()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(1, 0, 0),
            ["room3"] = new RoomPosition(0, 1, 0)
        };
        var rng = new Random(42);

        // Act
        var connections = _service.GenerateVerticalConnections(positions, rng);

        // Assert
        Assert.AreEqual(0, connections.Count);
    }

    [TestMethod]
    public void GenerateVerticalConnections_AllConnectionsAreBidirectional()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(0, 0, 1),
            ["room3"] = new RoomPosition(1, 1, 0),
            ["room4"] = new RoomPosition(1, 1, 1)
        };
        var rng = new Random(42);

        // Act
        var connections = _service.GenerateVerticalConnections(positions, rng);

        // Assert
        Assert.IsTrue(connections.All(c => c.IsBidirectional),
            "All vertical connections should be bidirectional");
    }

    [TestMethod]
    public void GenerateVerticalConnections_SetsLevelsSpannedCorrectly()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(0, 0, 2) // 2 levels apart
        };
        var rng = new Random(42);

        // Act
        var connections = _service.GenerateVerticalConnections(positions, rng);

        // Assert
        Assert.AreEqual(1, connections.Count);
        Assert.AreEqual(2, connections[0].LevelsSpanned);
    }

    #endregion

    #region Query Method Tests

    [TestMethod]
    public void GetRoomPosition_ExistingRoom_ReturnsPosition()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(5, 10, 2)
        };

        // Act
        var result = _service.GetRoomPosition("room1", positions);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(new RoomPosition(5, 10, 2), result.Value);
    }

    [TestMethod]
    public void GetRoomPosition_NonExistingRoom_ReturnsNull()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(5, 10, 2)
        };

        // Act
        var result = _service.GetRoomPosition("room999", positions);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetRoomsAtLayer_GroundLevel_ReturnsOnlyGroundRooms()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(1, 0, 0),
            ["room3"] = new RoomPosition(0, 1, 1), // Different layer
            ["room4"] = new RoomPosition(2, 0, 0)
        };

        // Act
        var rooms = _service.GetRoomsAtLayer(VerticalLayer.GroundLevel, positions);

        // Assert
        Assert.AreEqual(3, rooms.Count);
        CollectionAssert.Contains(rooms, "room1");
        CollectionAssert.Contains(rooms, "room2");
        CollectionAssert.Contains(rooms, "room4");
    }

    [TestMethod]
    public void GetRoomsAtLayer_EmptyLayer_ReturnsEmptyList()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0)
        };

        // Act
        var rooms = _service.GetRoomsAtLayer(VerticalLayer.Canopy, positions);

        // Assert
        Assert.AreEqual(0, rooms.Count);
    }

    [TestMethod]
    public void GetRoomsInRange_Radius1_ReturnsNearbyRooms()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(1, 0, 0), // Distance 1
            ["room3"] = new RoomPosition(0, 1, 0), // Distance 1
            ["room4"] = new RoomPosition(2, 0, 0), // Distance 2
            ["room5"] = new RoomPosition(0, 0, 1)  // Distance 1 (vertical)
        };
        var center = new RoomPosition(0, 0, 0);

        // Act
        var rooms = _service.GetRoomsInRange(center, radius: 1, positions);

        // Assert
        Assert.AreEqual(4, rooms.Count); // room1, room2, room3, room5
        CollectionAssert.Contains(rooms, "room1");
        CollectionAssert.Contains(rooms, "room2");
        CollectionAssert.Contains(rooms, "room3");
        CollectionAssert.Contains(rooms, "room5");
    }

    [TestMethod]
    public void GetRoomsInRange_Radius0_ReturnsOnlyCenter()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(1, 0, 0)
        };
        var center = new RoomPosition(0, 0, 0);

        // Act
        var rooms = _service.GetRoomsInRange(center, radius: 0, positions);

        // Assert
        Assert.AreEqual(1, rooms.Count);
        CollectionAssert.Contains(rooms, "room1");
    }

    [TestMethod]
    public void GetRoomsInRange_NoRoomsInRange_ReturnsEmpty()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(10, 10, 0),
            ["room2"] = new RoomPosition(11, 10, 0)
        };
        var center = new RoomPosition(0, 0, 0);

        // Act
        var rooms = _service.GetRoomsInRange(center, radius: 5, positions);

        // Assert
        Assert.AreEqual(0, rooms.Count);
    }

    #endregion

    #region Integration-Style Tests

    [TestMethod]
    public void FullPipeline_ConvertAndValidate_NoOverlaps()
    {
        // Arrange
        var graph = CreateSimpleLinearGraph(10);
        var templates = new Dictionary<string, RoomTemplate>();

        // Act
        var positions = _service.ConvertGraphTo3DLayout(graph, seed: 42);
        var isValid = _service.ValidateNoOverlaps(positions, templates);

        // Assert
        Assert.IsTrue(isValid, "Generated layout should have no overlaps");
    }

    [TestMethod]
    public void FullPipeline_ConvertAndGenerateConnections_CreatesValidConnections()
    {
        // Arrange
        var graph = CreateBranchingGraph();
        var rng = new Random(42);

        // Act
        var positions = _service.ConvertGraphTo3DLayout(graph, seed: 42);
        var connections = _service.GenerateVerticalConnections(positions, rng);

        // Assert - Should have connections if there are vertically aligned rooms
        Assert.IsNotNull(connections);

        // All connections should reference valid rooms
        foreach (var connection in connections)
        {
            Assert.IsTrue(positions.ContainsKey(connection.FromRoomId),
                $"Connection references invalid FromRoomId: {connection.FromRoomId}");
            Assert.IsTrue(positions.ContainsKey(connection.ToRoomId),
                $"Connection references invalid ToRoomId: {connection.ToRoomId}");
        }
    }

    [TestMethod]
    public void FullPipeline_LargeGraph_CompletesSuccessfully()
    {
        // Arrange
        var graph = CreateSimpleLinearGraph(50);
        var templates = new Dictionary<string, RoomTemplate>();
        var rng = new Random(42);

        // Act
        var positions = _service.ConvertGraphTo3DLayout(graph, seed: 42);
        var isValid = _service.ValidateNoOverlaps(positions, templates);
        var connections = _service.GenerateVerticalConnections(positions, rng);

        // Assert
        Assert.AreEqual(50, positions.Count);
        Assert.IsTrue(isValid);
        Assert.IsNotNull(connections);
    }

    #endregion
}
