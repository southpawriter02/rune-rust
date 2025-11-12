using NUnit.Framework;
using RuneAndRust.Core;

namespace RuneAndRust.Tests;

/// <summary>
/// Tests for DungeonGraph, DungeonNode, and DungeonEdge (v0.10)
/// </summary>
[TestFixture]
public class DungeonGraphTests
{
    private DungeonGraph _graph = null!;
    private RoomTemplate _testTemplate = null!;

    [SetUp]
    public void Setup()
    {
        _graph = new DungeonGraph();
        _testTemplate = CreateTestTemplate("test_template");
    }

    #region Node Management Tests

    [Test]
    public void AddNode_AddsNodeToGraph()
    {
        // Arrange
        var node = CreateTestNode(NodeType.Main);

        // Act
        _graph.AddNode(node);

        // Assert
        Assert.That(_graph.NodeCount, Is.EqualTo(1));
        Assert.That(_graph.GetNodes(), Contains.Item(node));
    }

    [Test]
    public void AddNode_AssignsIdAutomatically()
    {
        // Arrange
        var node1 = CreateTestNode(NodeType.Main);
        var node2 = CreateTestNode(NodeType.Main);

        // Act
        _graph.AddNode(node1);
        _graph.AddNode(node2);

        // Assert
        Assert.That(node1.Id, Is.EqualTo(1));
        Assert.That(node2.Id, Is.EqualTo(2));
    }

    [Test]
    public void RemoveNode_RemovesNodeAndConnectedEdges()
    {
        // Arrange
        var node1 = CreateTestNode(NodeType.Start);
        var node2 = CreateTestNode(NodeType.Main);
        var node3 = CreateTestNode(NodeType.Boss);

        _graph.AddNode(node1);
        _graph.AddNode(node2);
        _graph.AddNode(node3);
        _graph.AddEdge(node1, node2);
        _graph.AddEdge(node2, node3);

        // Act
        _graph.RemoveNode(node2);

        // Assert
        Assert.That(_graph.NodeCount, Is.EqualTo(2));
        Assert.That(_graph.EdgeCount, Is.EqualTo(0)); // Both edges removed
    }

    [Test]
    public void GetNodesByType_ReturnsFilteredNodes()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var main1 = CreateTestNode(NodeType.Main);
        var main2 = CreateTestNode(NodeType.Main);
        var boss = CreateTestNode(NodeType.Boss);

        _graph.AddNode(start);
        _graph.AddNode(main1);
        _graph.AddNode(main2);
        _graph.AddNode(boss);

        // Act
        var mainNodes = _graph.GetNodesByType(NodeType.Main);

        // Assert
        Assert.That(mainNodes.Count, Is.EqualTo(2));
        Assert.That(mainNodes, Contains.Item(main1));
        Assert.That(mainNodes, Contains.Item(main2));
    }

    [Test]
    public void GetMainPathNodes_ReturnsOnlyCriticalPathNodes()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var main = CreateTestNode(NodeType.Main);
        var branch = CreateTestNode(NodeType.Branch);
        var secret = CreateTestNode(NodeType.Secret);
        var boss = CreateTestNode(NodeType.Boss);

        _graph.AddNode(start);
        _graph.AddNode(main);
        _graph.AddNode(branch);
        _graph.AddNode(secret);
        _graph.AddNode(boss);

        // Act
        var mainPath = _graph.GetMainPathNodes();

        // Assert
        Assert.That(mainPath.Count, Is.EqualTo(3));
        Assert.That(mainPath, Contains.Item(start));
        Assert.That(mainPath, Contains.Item(main));
        Assert.That(mainPath, Contains.Item(boss));
        Assert.That(mainPath, Does.Not.Contain(branch));
        Assert.That(mainPath, Does.Not.Contain(secret));
    }

    #endregion

    #region Edge Management Tests

    [Test]
    public void AddEdge_CreatesConnectionBetweenNodes()
    {
        // Arrange
        var node1 = CreateTestNode(NodeType.Start);
        var node2 = CreateTestNode(NodeType.Main);
        _graph.AddNode(node1);
        _graph.AddNode(node2);

        // Act
        _graph.AddEdge(node1, node2);

        // Assert
        Assert.That(_graph.EdgeCount, Is.EqualTo(1));
        Assert.That(_graph.HasEdge(node1, node2), Is.True);
    }

    [Test]
    public void AddEdge_WithEdgeType_SetsTypeCorrectly()
    {
        // Arrange
        var node1 = CreateTestNode(NodeType.Main);
        var node2 = CreateTestNode(NodeType.Secret);
        _graph.AddNode(node1);
        _graph.AddNode(node2);

        // Act
        _graph.AddEdge(node1, node2, EdgeType.Secret);

        // Assert
        var edge = _graph.GetEdgesFrom(node1).First();
        Assert.That(edge.Type, Is.EqualTo(EdgeType.Secret));
    }

    [Test]
    public void GetEdgesFrom_ReturnsBidirectionalEdges()
    {
        // Arrange
        var node1 = CreateTestNode(NodeType.Start);
        var node2 = CreateTestNode(NodeType.Main);
        _graph.AddNode(node1);
        _graph.AddNode(node2);
        _graph.AddEdge(node1, node2);

        // Act
        var edges = _graph.GetEdgesFrom(node2);

        // Assert (bidirectional edge should allow traversal from node2 back to node1)
        Assert.That(edges.Count, Is.EqualTo(1));
        Assert.That(edges[0].To, Is.EqualTo(node1));
    }

    [Test]
    public void GetNeighbors_ReturnsConnectedNodes()
    {
        // Arrange
        var node1 = CreateTestNode(NodeType.Start);
        var node2 = CreateTestNode(NodeType.Main);
        var node3 = CreateTestNode(NodeType.Main);
        _graph.AddNode(node1);
        _graph.AddNode(node2);
        _graph.AddNode(node3);
        _graph.AddEdge(node1, node2);
        _graph.AddEdge(node1, node3);

        // Act
        var neighbors = _graph.GetNeighbors(node1);

        // Assert
        Assert.That(neighbors.Count, Is.EqualTo(2));
        Assert.That(neighbors, Contains.Item(node2));
        Assert.That(neighbors, Contains.Item(node3));
    }

    #endregion

    #region Traversal Tests

    [Test]
    public void IsReachable_WithConnectedNodes_ReturnsTrue()
    {
        // Arrange
        var node1 = CreateTestNode(NodeType.Start);
        var node2 = CreateTestNode(NodeType.Main);
        var node3 = CreateTestNode(NodeType.Boss);
        _graph.AddNode(node1);
        _graph.AddNode(node2);
        _graph.AddNode(node3);
        _graph.AddEdge(node1, node2);
        _graph.AddEdge(node2, node3);

        // Act
        bool result = _graph.IsReachable(node1, node3);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsReachable_WithDisconnectedNodes_ReturnsFalse()
    {
        // Arrange
        var node1 = CreateTestNode(NodeType.Start);
        var node2 = CreateTestNode(NodeType.Main);
        var orphan = CreateTestNode(NodeType.Main);
        _graph.AddNode(node1);
        _graph.AddNode(node2);
        _graph.AddNode(orphan);
        _graph.AddEdge(node1, node2);

        // Act
        bool result = _graph.IsReachable(node1, orphan);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetMainPath_ReturnsPathFromStartToBoss()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var main1 = CreateTestNode(NodeType.Main);
        var main2 = CreateTestNode(NodeType.Main);
        var boss = CreateTestNode(NodeType.Boss);

        _graph.AddNode(start);
        _graph.AddNode(main1);
        _graph.AddNode(main2);
        _graph.AddNode(boss);

        _graph.AddEdge(start, main1);
        _graph.AddEdge(main1, main2);
        _graph.AddEdge(main2, boss);

        // Act
        var path = _graph.GetMainPath();

        // Assert
        Assert.That(path, Is.Not.Null);
        Assert.That(path!.Count, Is.EqualTo(4));
        Assert.That(path[0], Is.EqualTo(start));
        Assert.That(path[^1], Is.EqualTo(boss));
    }

    [Test]
    public void GetMainPath_WithNoPath_ReturnsNull()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var boss = CreateTestNode(NodeType.Boss);
        _graph.AddNode(start);
        _graph.AddNode(boss);
        // No edge between them

        // Act
        var path = _graph.GetMainPath();

        // Assert
        Assert.That(path, Is.Null);
    }

    [Test]
    public void GetReachableNodes_ReturnsAllConnectedNodes()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var main = CreateTestNode(NodeType.Main);
        var branch = CreateTestNode(NodeType.Branch);
        var orphan = CreateTestNode(NodeType.Main);

        _graph.AddNode(start);
        _graph.AddNode(main);
        _graph.AddNode(branch);
        _graph.AddNode(orphan);

        _graph.AddEdge(start, main);
        _graph.AddEdge(main, branch);

        // Act
        var reachable = _graph.GetReachableNodes(start);

        // Assert
        Assert.That(reachable.Count, Is.EqualTo(3));
        Assert.That(reachable, Contains.Item(start));
        Assert.That(reachable, Contains.Item(main));
        Assert.That(reachable, Contains.Item(branch));
        Assert.That(reachable, Does.Not.Contain(orphan));
    }

    #endregion

    #region Validation Tests

    [Test]
    public void Validate_WithValidGraph_ReturnsTrue()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var main = CreateTestNode(NodeType.Main);
        var boss = CreateTestNode(NodeType.Boss);

        _graph.AddNode(start);
        _graph.AddNode(main);
        _graph.AddNode(boss);

        _graph.AddEdge(start, main);
        _graph.AddEdge(main, boss);

        // Act
        var (isValid, errors) = _graph.Validate();

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_WithNoStartNode_ReturnsError()
    {
        // Arrange
        var main = CreateTestNode(NodeType.Main);
        var boss = CreateTestNode(NodeType.Boss);
        _graph.AddNode(main);
        _graph.AddNode(boss);
        _graph.AddEdge(main, boss);

        // Act
        var (isValid, errors) = _graph.Validate();

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Has.Some.Contains("no start node"));
    }

    [Test]
    public void Validate_WithNoBossNode_ReturnsError()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var main = CreateTestNode(NodeType.Main);
        _graph.AddNode(start);
        _graph.AddNode(main);
        _graph.AddEdge(start, main);

        // Act
        var (isValid, errors) = _graph.Validate();

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Has.Some.Contains("no boss node"));
    }

    [Test]
    public void Validate_WithNoPathToBoss_ReturnsError()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var boss = CreateTestNode(NodeType.Boss);
        _graph.AddNode(start);
        _graph.AddNode(boss);
        // No edge connecting them

        // Act
        var (isValid, errors) = _graph.Validate();

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Has.Some.Contains("No path exists from start to boss"));
    }

    [Test]
    public void Validate_WithOrphanedNodes_ReturnsError()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var main = CreateTestNode(NodeType.Main);
        var boss = CreateTestNode(NodeType.Boss);
        var orphan = CreateTestNode(NodeType.Main);

        _graph.AddNode(start);
        _graph.AddNode(main);
        _graph.AddNode(boss);
        _graph.AddNode(orphan);

        _graph.AddEdge(start, main);
        _graph.AddEdge(main, boss);

        // Act
        var (isValid, errors) = _graph.Validate();

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Has.Some.Contains("orphaned nodes"));
    }

    [Test]
    public void Validate_WithMultipleStartNodes_ReturnsError()
    {
        // Arrange
        var start1 = CreateTestNode(NodeType.Start);
        var start2 = CreateTestNode(NodeType.Start);
        var boss = CreateTestNode(NodeType.Boss);

        _graph.AddNode(start1);
        _graph.AddNode(start2);
        _graph.AddNode(boss);

        _graph.AddEdge(start1, boss);
        _graph.AddEdge(start2, boss);

        // Act
        var (isValid, errors) = _graph.Validate();

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Has.Some.Contains("start nodes (should have exactly 1)"));
    }

    #endregion

    #region Statistics Tests

    [Test]
    public void GetStatistics_ReturnsCorrectCounts()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var main1 = CreateTestNode(NodeType.Main);
        var main2 = CreateTestNode(NodeType.Main);
        var branch = CreateTestNode(NodeType.Branch);
        var boss = CreateTestNode(NodeType.Boss);

        _graph.AddNode(start);
        _graph.AddNode(main1);
        _graph.AddNode(main2);
        _graph.AddNode(branch);
        _graph.AddNode(boss);

        _graph.AddEdge(start, main1);
        _graph.AddEdge(main1, main2);
        _graph.AddEdge(main2, boss);
        _graph.AddEdge(main1, branch, EdgeType.Secret);

        // Act
        var stats = _graph.GetStatistics();

        // Assert
        Assert.That(stats["TotalNodes"], Is.EqualTo(5));
        Assert.That(stats["TotalEdges"], Is.EqualTo(4));
        Assert.That(stats["StartNodes"], Is.EqualTo(1));
        Assert.That(stats["MainNodes"], Is.EqualTo(2));
        Assert.That(stats["BranchNodes"], Is.EqualTo(1));
        Assert.That(stats["BossNodes"], Is.EqualTo(1));
        Assert.That(stats["NormalEdges"], Is.EqualTo(3));
        Assert.That(stats["SecretEdges"], Is.EqualTo(1));
    }

    #endregion

    #region Node Tests

    [Test]
    public void DungeonNode_IsOnCriticalPath_ReturnsCorrectly()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var main = CreateTestNode(NodeType.Main);
        var branch = CreateTestNode(NodeType.Branch);
        var secret = CreateTestNode(NodeType.Secret);
        var boss = CreateTestNode(NodeType.Boss);

        // Assert
        Assert.That(start.IsOnCriticalPath(), Is.True);
        Assert.That(main.IsOnCriticalPath(), Is.True);
        Assert.That(boss.IsOnCriticalPath(), Is.True);
        Assert.That(branch.IsOnCriticalPath(), Is.False);
        Assert.That(secret.IsOnCriticalPath(), Is.False);
    }

    [Test]
    public void DungeonNode_IsOptional_ReturnsCorrectly()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var branch = CreateTestNode(NodeType.Branch);
        var secret = CreateTestNode(NodeType.Secret);

        // Assert
        Assert.That(start.IsOptional(), Is.False);
        Assert.That(branch.IsOptional(), Is.True);
        Assert.That(secret.IsOptional(), Is.True);
    }

    #endregion

    #region Edge Tests

    [Test]
    public void DungeonEdge_HasDirections_WorksCorrectly()
    {
        // Arrange
        var node1 = CreateTestNode(NodeType.Start);
        var node2 = CreateTestNode(NodeType.Main);
        var edge = new DungeonEdge
        {
            From = node1,
            To = node2
        };

        // Assert - initially no directions
        Assert.That(edge.HasDirections(), Is.False);

        // Act - assign directions
        edge.FromDirection = "north";
        edge.ToDirection = "south";

        // Assert
        Assert.That(edge.HasDirections(), Is.True);
    }

    [Test]
    public void DungeonEdge_IsOnCriticalPath_WorksCorrectly()
    {
        // Arrange
        var start = CreateTestNode(NodeType.Start);
        var main = CreateTestNode(NodeType.Main);
        var branch = CreateTestNode(NodeType.Branch);

        var criticalEdge = new DungeonEdge { From = start, To = main };
        var branchEdge = new DungeonEdge { From = main, To = branch };

        // Assert
        Assert.That(criticalEdge.IsOnCriticalPath(), Is.True);
        Assert.That(branchEdge.IsOnCriticalPath(), Is.False);
    }

    #endregion

    #region Helper Methods

    private DungeonNode CreateTestNode(NodeType type)
    {
        return new DungeonNode
        {
            Template = _testTemplate,
            Type = type,
            Name = $"Test {type} Node"
        };
    }

    private RoomTemplate CreateTestTemplate(string id)
    {
        return new RoomTemplate
        {
            TemplateId = id,
            Archetype = RoomArchetype.Chamber,
            NameTemplates = new List<string> { "Test Room" },
            Adjectives = new List<string> { "Test" },
            DescriptionTemplates = new List<string> { "Test description" },
            Details = new List<string> { "Test detail" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 4
        };
    }

    #endregion
}
