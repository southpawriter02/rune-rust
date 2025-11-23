using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using RuneAndRust.Engine.Spatial;
using Serilog;

namespace RuneAndRust.Tests.Spatial;

/// <summary>
/// Unit tests for SpatialValidationService (v0.39.1)
/// Tests spatial coherence validation, overlap detection, and reachability checks
/// </summary>
[TestClass]
public class SpatialValidationServiceTests
{
    private SpatialValidationService _service = null!;
    private ILogger _logger = null!;

    [TestInitialize]
    public void Setup()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        _service = new SpatialValidationService(_logger);
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

    private DungeonGraph CreateSimpleGraph()
    {
        var graph = new DungeonGraph();
        var template = CreateTestTemplate();

        var start = new DungeonNode { Template = template, Type = NodeType.Start, Depth = 0 };
        var main1 = new DungeonNode { Template = template, Type = NodeType.Main, Depth = 1 };
        var boss = new DungeonNode { Template = template, Type = NodeType.Boss, Depth = 2 };

        graph.AddNode(start);
        graph.AddNode(main1);
        graph.AddNode(boss);

        // Use reflection to add edges
        var edgesField = typeof(DungeonGraph).GetField("_edges",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var edgesList = (List<DungeonEdge>)edgesField!.GetValue(graph)!;

        edgesList.Add(new DungeonEdge
        {
            From = start,
            To = main1,
            FromDirection = Direction.North,
            IsBidirectional = true
        });

        edgesList.Add(new DungeonEdge
        {
            From = main1,
            To = boss,
            FromDirection = Direction.North,
            IsBidirectional = true
        });

        return graph;
    }

    #endregion

    #region ValidateSector Tests

    [TestMethod]
    public void ValidateSector_ValidLayout_NoIssues()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),  // Start node (ID=1)
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(0, 2, 0)
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);

        // Assert
        Assert.AreEqual(0, issues.Count);
    }

    [TestMethod]
    public void ValidateSector_MultipleIssues_ReturnsAll()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(1, 1, 0),  // Not at origin - Error
            ["2"] = new RoomPosition(0, 0, 0),
            ["3"] = new RoomPosition(0, 0, 0)   // Overlap with room2 - Critical
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);

        // Assert
        Assert.IsTrue(issues.Count >= 2); // At least overlap + origin placement
        Assert.IsTrue(issues.Any(i => i.Type == "Overlap"));
        Assert.IsTrue(issues.Any(i => i.Type == "InvalidFootprint"));
    }

    #endregion

    #region Overlap Detection Tests

    [TestMethod]
    public void ValidateSector_NoOverlaps_PassesCheck()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(1, 0, 0),
            ["3"] = new RoomPosition(0, 1, 0)
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var overlapIssues = issues.Where(i => i.Type == "Overlap").ToList();

        // Assert
        Assert.AreEqual(0, overlapIssues.Count);
    }

    [TestMethod]
    public void ValidateSector_TwoRoomsOverlap_CriticalIssue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(5, 5, 1),
            ["3"] = new RoomPosition(5, 5, 1)  // Overlaps with room2
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var overlapIssues = issues.Where(i => i.Type == "Overlap").ToList();

        // Assert
        Assert.AreEqual(1, overlapIssues.Count);
        Assert.AreEqual("Critical", overlapIssues[0].Severity);
        Assert.AreEqual(2, overlapIssues[0].AffectedRoomIds.Count);
    }

    [TestMethod]
    public void ValidateSector_SameHorizontalDifferentVertical_NoOverlap()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(5, 5, 0),
            ["3"] = new RoomPosition(5, 5, 1)  // Same X,Y but different Z
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var overlapIssues = issues.Where(i => i.Type == "Overlap").ToList();

        // Assert
        Assert.AreEqual(0, overlapIssues.Count);
    }

    #endregion

    #region Reachability Tests

    [TestMethod]
    public void ValidateSector_AllRoomsReachable_NoIssues()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),  // Start
            ["2"] = new RoomPosition(0, 1, 0),  // Connected via graph
            ["3"] = new RoomPosition(0, 2, 0)   // Connected via graph
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var reachabilityIssues = issues.Where(i => i.Type == "Unreachable").ToList();

        // Assert
        Assert.AreEqual(0, reachabilityIssues.Count);
    }

    [TestMethod]
    public void ValidateSector_IsolatedRoom_ErrorIssue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(0, 2, 0),
            ["4"] = new RoomPosition(10, 10, 0)  // Not in graph, isolated
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var reachabilityIssues = issues.Where(i => i.Type == "Unreachable").ToList();

        // Assert
        Assert.AreEqual(1, reachabilityIssues.Count);
        Assert.AreEqual("Error", reachabilityIssues[0].Severity);
        Assert.IsTrue(reachabilityIssues[0].AffectedRoomIds.Contains("4"));
    }

    [TestMethod]
    public void ValidateSector_ReachableViaVerticalConnection_NoIssue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(0, 2, 1)  // Above room2
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("2", "3")
        };

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var reachabilityIssues = issues.Where(i => i.Type == "Unreachable").ToList();

        // Assert
        Assert.AreEqual(0, reachabilityIssues.Count);
    }

    [TestMethod]
    public void ValidateSector_BlockedVerticalConnection_Unreachable()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(0, 1, 1)
        };
        var connection = VerticalConnection.CreateStairs("2", "3");
        connection.IsBlocked = true;
        var connections = new List<VerticalConnection> { connection };

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var reachabilityIssues = issues.Where(i => i.Type == "Unreachable").ToList();

        // Assert - Room 3 should be unreachable because connection is blocked
        Assert.AreEqual(1, reachabilityIssues.Count);
        Assert.IsTrue(reachabilityIssues[0].AffectedRoomIds.Contains("3"));
    }

    #endregion

    #region Vertical Connection Validation Tests

    [TestMethod]
    public void ValidateSector_ValidVerticalConnections_NoIssues()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(0, 1, 1)  // Directly above room2
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("2", "3")
        };

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var connectionIssues = issues.Where(i => i.Type == "MissingConnection" || i.Type == "InvalidFootprint").ToList();

        // Assert
        Assert.AreEqual(0, connectionIssues.Count);
    }

    [TestMethod]
    public void ValidateSector_ConnectionToNonExistentRoom_CriticalIssue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 0)
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("2", "999")  // Room 999 doesn't exist
        };

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var connectionIssues = issues.Where(i => i.Type == "MissingConnection").ToList();

        // Assert
        Assert.IsTrue(connectionIssues.Count > 0);
        Assert.IsTrue(connectionIssues.Any(i => i.Severity == "Critical"));
        Assert.IsTrue(connectionIssues.Any(i => i.Description.Contains("999")));
    }

    [TestMethod]
    public void ValidateSector_ConnectionOnSameZLevel_WarningIssue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(0, 2, 0)  // Same Z as room2
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("2", "3")  // Should be horizontal, not vertical
        };

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var connectionIssues = issues.Where(i => i.Type == "InvalidFootprint" && i.Description.Contains("same Z level")).ToList();

        // Assert
        Assert.AreEqual(1, connectionIssues.Count);
        Assert.AreEqual("Warning", connectionIssues[0].Severity);
    }

    [TestMethod]
    public void ValidateSector_ConnectionNotDirectlyAboveBelow_WarningIssue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(5, 5, 1)  // Different X,Y, not directly above/below
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("2", "3")
        };

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var connectionIssues = issues.Where(i => i.Type == "InvalidFootprint" && i.Description.Contains("not directly above/below")).ToList();

        // Assert
        Assert.AreEqual(1, connectionIssues.Count);
        Assert.AreEqual("Warning", connectionIssues[0].Severity);
    }

    #endregion

    #region Layer Bounds Tests

    [TestMethod]
    public void ValidateSector_AllRoomsWithinBounds_NoIssues()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, -3),  // Min bound
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(0, 2, 3)   // Max bound
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var boundsIssues = issues.Where(i => i.Type == "LayerBounds").ToList();

        // Assert
        Assert.AreEqual(0, boundsIssues.Count);
    }

    [TestMethod]
    public void ValidateSector_RoomBelowMinimum_CriticalIssue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, -4),  // Below minimum -3
            ["3"] = new RoomPosition(0, 2, 0)
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var boundsIssues = issues.Where(i => i.Type == "LayerBounds").ToList();

        // Assert
        Assert.AreEqual(1, boundsIssues.Count);
        Assert.AreEqual("Critical", boundsIssues[0].Severity);
        Assert.IsTrue(boundsIssues[0].Description.Contains("Z=-4"));
    }

    [TestMethod]
    public void ValidateSector_RoomAboveMaximum_CriticalIssue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 4),  // Above maximum +3
            ["3"] = new RoomPosition(0, 2, 0)
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var boundsIssues = issues.Where(i => i.Type == "LayerBounds").ToList();

        // Assert
        Assert.AreEqual(1, boundsIssues.Count);
        Assert.AreEqual("Critical", boundsIssues[0].Severity);
        Assert.IsTrue(boundsIssues[0].Description.Contains("Z=4"));
    }

    #endregion

    #region Origin Placement Tests

    [TestMethod]
    public void ValidateSector_EntryHallAtOrigin_NoIssue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),  // Entry hall at origin
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(0, 2, 0)
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var originIssues = issues.Where(i => i.Type == "InvalidFootprint" && i.Description.Contains("origin")).ToList();

        // Assert
        Assert.AreEqual(0, originIssues.Count);
    }

    [TestMethod]
    public void ValidateSector_EntryHallNotAtOrigin_ErrorIssue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(5, 5, 1),  // Entry hall NOT at origin
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(0, 2, 0)
        };
        var connections = new List<VerticalConnection>();

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);
        var originIssues = issues.Where(i => i.Type == "InvalidFootprint" && i.Description.Contains("origin")).ToList();

        // Assert
        Assert.AreEqual(1, originIssues.Count);
        Assert.AreEqual("Error", originIssues[0].Severity);
        Assert.IsTrue(originIssues[0].Description.Contains("(5, 5, 1)"));
    }

    #endregion

    #region IsReachableFromOrigin Tests

    [TestMethod]
    public void IsReachableFromOrigin_DirectlyConnected_ReturnsTrue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 0)
        };
        var connections = new List<VerticalConnection>();

        // Act
        var reachable = _service.IsReachableFromOrigin("2", positions, connections, graph);

        // Assert
        Assert.IsTrue(reachable);
    }

    [TestMethod]
    public void IsReachableFromOrigin_EntryHallItself_ReturnsTrue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0)
        };
        var connections = new List<VerticalConnection>();

        // Act
        var reachable = _service.IsReachableFromOrigin("1", positions, connections, graph);

        // Assert
        Assert.IsTrue(reachable);
    }

    [TestMethod]
    public void IsReachableFromOrigin_IsolatedRoom_ReturnsFalse()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 0),
            ["999"] = new RoomPosition(10, 10, 0)  // Isolated
        };
        var connections = new List<VerticalConnection>();

        // Act
        var reachable = _service.IsReachableFromOrigin("999", positions, connections, graph);

        // Assert
        Assert.IsFalse(reachable);
    }

    [TestMethod]
    public void IsReachableFromOrigin_ViaVerticalConnection_ReturnsTrue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(0, 1, 1)  // Above room2
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("2", "3")
        };

        // Act
        var reachable = _service.IsReachableFromOrigin("3", positions, connections, graph);

        // Assert
        Assert.IsTrue(reachable);
    }

    [TestMethod]
    public void IsReachableFromOrigin_BlockedConnection_ReturnsFalse()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),
            ["2"] = new RoomPosition(0, 1, 0),
            ["3"] = new RoomPosition(0, 1, 1)
        };
        var connection = VerticalConnection.CreateStairs("2", "3");
        connection.IsBlocked = true;
        var connections = new List<VerticalConnection> { connection };

        // Act
        var reachable = _service.IsReachableFromOrigin("3", positions, connections, graph);

        // Assert
        Assert.IsFalse(reachable);
    }

    #endregion

    #region LogValidationIssues Tests

    [TestMethod]
    public void LogValidationIssues_EmptyList_CompletesSuccessfully()
    {
        // Arrange
        var issues = new List<ValidationIssue>();

        // Act & Assert (should not throw)
        _service.LogValidationIssues(sectorId: 1, issues);
    }

    [TestMethod]
    public void LogValidationIssues_MultipleIssues_LogsAll()
    {
        // Arrange
        var issues = new List<ValidationIssue>
        {
            new ValidationIssue
            {
                Type = "Overlap",
                Severity = "Critical",
                Description = "Test overlap"
            },
            new ValidationIssue
            {
                Type = "Unreachable",
                Severity = "Error",
                Description = "Test unreachable"
            }
        };

        // Act & Assert (should not throw)
        _service.LogValidationIssues(sectorId: 1, issues);
    }

    #endregion

    #region Integration-Style Tests

    [TestMethod]
    public void ValidateSector_ComplexValidLayout_NoIssues()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(0, 0, 0),   // Origin
            ["2"] = new RoomPosition(0, 1, 0),   // North
            ["3"] = new RoomPosition(0, 1, 1)    // Above room2
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("2", "3")
        };

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);

        // Assert
        Assert.AreEqual(0, issues.Count);
    }

    [TestMethod]
    public void ValidateSector_AllValidationTypes_DetectsCorrectly()
    {
        // Arrange - create a layout with multiple types of issues
        var graph = CreateSimpleGraph();
        var positions = new Dictionary<string, RoomPosition>
        {
            ["1"] = new RoomPosition(1, 0, 0),   // Not at origin (Error)
            ["2"] = new RoomPosition(0, 0, 5),   // Out of bounds (Critical)
            ["3"] = new RoomPosition(0, 0, 5),   // Overlap with room2 (Critical)
            ["4"] = new RoomPosition(10, 10, 0)  // Unreachable (Error)
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("1", "999")  // Invalid target (Critical)
        };

        // Act
        var issues = _service.ValidateSector(positions, connections, graph);

        // Assert
        Assert.IsTrue(issues.Count >= 4); // At least 4 different types of issues
        Assert.IsTrue(issues.Any(i => i.Type == "InvalidFootprint" && i.Description.Contains("origin")));
        Assert.IsTrue(issues.Any(i => i.Type == "LayerBounds"));
        Assert.IsTrue(issues.Any(i => i.Type == "Overlap"));
        Assert.IsTrue(issues.Any(i => i.Type == "MissingConnection"));
    }

    #endregion
}
