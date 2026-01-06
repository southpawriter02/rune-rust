using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class SectorTests
{
    [Test]
    public void Constructor_WithValidParameters_CreatesSector()
    {
        // Arrange & Act
        var sector = new Sector("Test Sector", Biome.Citadel, depth: 1);

        // Assert
        sector.Id.Should().NotBeEmpty();
        sector.Name.Should().Be("Test Sector");
        sector.Biome.Should().Be(Biome.Citadel);
        sector.Depth.Should().Be(1);
        sector.ThreatBudget.Should().Be(0);
        sector.GetNodeCount().Should().Be(0);
    }

    [Test]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new Sector("", Biome.Citadel);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void Constructor_WithZeroDepth_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => new Sector("Test", Biome.Citadel, depth: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("depth");
    }

    [Test]
    public void AddNode_AddsNodeSuccessfully()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel);
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);

        // Act
        sector.AddNode(node);

        // Assert
        sector.GetNodeCount().Should().Be(1);
        sector.GetNode(node.Id).Should().Be(node);
    }

    [Test]
    public void AddNode_DuplicateCoordinate_ThrowsInvalidOperationException()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel);
        var node1 = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);
        var node2 = new DungeonNode("Sec1_Rm02", Coordinate3D.Origin);
        sector.AddNode(node1);

        // Act
        var act = () => sector.AddNode(node2);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void GetNodeByCoordinate_ReturnsCorrectNode()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel);
        var coordinate = new Coordinate3D(1, 2, 0);
        var node = new DungeonNode("Sec1_Rm01", coordinate);
        sector.AddNode(node);

        // Act
        var result = sector.GetNodeByCoordinate(coordinate);

        // Assert
        result.Should().Be(node);
    }

    [Test]
    public void IsCoordinateOccupied_ReturnsCorrectValue()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel);
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);
        sector.AddNode(node);

        // Assert
        sector.IsCoordinateOccupied(Coordinate3D.Origin).Should().BeTrue();
        sector.IsCoordinateOccupied(new Coordinate3D(1, 0, 0)).Should().BeFalse();
    }

    [Test]
    public void SetStartNode_SetsStartNodeAndMarksNode()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel);
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);
        sector.AddNode(node);

        // Act
        sector.SetStartNode(node.Id);

        // Assert
        sector.StartNodeId.Should().Be(node.Id);
        node.IsStartNode.Should().BeTrue();
    }

    [Test]
    public void SetBossNode_SetsBossNodeAndMarksNode()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel);
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);
        sector.AddNode(node);

        // Act
        sector.SetBossNode(node.Id);

        // Assert
        sector.BossNodeId.Should().Be(node.Id);
        node.IsBossArena.Should().BeTrue();
        node.Archetype.Should().Be(RoomArchetype.BossArena);
    }

    [Test]
    public void SetThreatBudget_SetsBudget()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel);

        // Act
        sector.SetThreatBudget(150);

        // Assert
        sector.ThreatBudget.Should().Be(150);
    }

    [Test]
    public void CalculateThreatBudget_CalculatesCorrectly()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel, depth: 3);

        // Act
        sector.CalculateThreatBudget(DifficultyTier.Tier2);

        // Assert
        // Formula: BaseDifficulty (150) + Depth (3) × 10 = 180
        sector.ThreatBudget.Should().Be(180);
    }

    [Test]
    public void GetLeafNodes_ReturnsNodesWithOneConnection()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel);
        var node1 = new DungeonNode("Sec1_Rm01", new Coordinate3D(0, 0, 0));
        var node2 = new DungeonNode("Sec1_Rm02", new Coordinate3D(1, 0, 0));
        var node3 = new DungeonNode("Sec1_Rm03", new Coordinate3D(2, 0, 0));

        sector.AddNode(node1);
        sector.AddNode(node2);
        sector.AddNode(node3);

        // Connect: node1 <-> node2 <-> node3
        sector.ConnectNodes(node1.Id, Direction.East, node2.Id);
        sector.ConnectNodes(node2.Id, Direction.East, node3.Id);

        // Act
        var leafNodes = sector.GetLeafNodes().ToList();

        // Assert
        leafNodes.Should().HaveCount(2);
        leafNodes.Should().Contain(node1);
        leafNodes.Should().Contain(node3);
    }

    [Test]
    public void ConnectNodes_CreatesBidirectionalConnection()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel);
        var node1 = new DungeonNode("Sec1_Rm01", new Coordinate3D(0, 0, 0));
        var node2 = new DungeonNode("Sec1_Rm02", new Coordinate3D(0, 1, 0));

        sector.AddNode(node1);
        sector.AddNode(node2);

        // Act
        sector.ConnectNodes(node1.Id, Direction.North, node2.Id);

        // Assert
        node1.HasConnection(Direction.North).Should().BeTrue();
        node1.GetConnection(Direction.North).Should().Be(node2.Id);
        node2.HasConnection(Direction.South).Should().BeTrue();
        node2.GetConnection(Direction.South).Should().Be(node1.Id);
    }

    [Test]
    public void FinalizeNodeArchetypes_UpdatesAllNodeArchetypes()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel);
        var node1 = new DungeonNode("Sec1_Rm01", new Coordinate3D(0, 0, 0));
        var node2 = new DungeonNode("Sec1_Rm02", new Coordinate3D(1, 0, 0));
        var node3 = new DungeonNode("Sec1_Rm03", new Coordinate3D(0, 1, 0));

        sector.AddNode(node1);
        sector.AddNode(node2);
        sector.AddNode(node3);

        // node1 will have 2 connections (Corridor)
        // node2 will have 1 connection (DeadEnd)
        // node3 will have 1 connection (DeadEnd)
        sector.ConnectNodes(node1.Id, Direction.East, node2.Id);
        sector.ConnectNodes(node1.Id, Direction.North, node3.Id);

        // Act
        sector.FinalizeNodeArchetypes();

        // Assert
        node1.Archetype.Should().Be(RoomArchetype.Corridor);
        node2.Archetype.Should().Be(RoomArchetype.DeadEnd);
        node3.Archetype.Should().Be(RoomArchetype.DeadEnd);
    }

    [Test]
    public void GetAllNodes_ReturnsAllNodes()
    {
        // Arrange
        var sector = new Sector("Test", Biome.Citadel);
        var node1 = new DungeonNode("Sec1_Rm01", new Coordinate3D(0, 0, 0));
        var node2 = new DungeonNode("Sec1_Rm02", new Coordinate3D(1, 0, 0));

        sector.AddNode(node1);
        sector.AddNode(node2);

        // Act
        var allNodes = sector.GetAllNodes().ToList();

        // Assert
        allNodes.Should().HaveCount(2);
        allNodes.Should().Contain(node1);
        allNodes.Should().Contain(node2);
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var sector = new Sector("The Depths", Biome.TheRoots, depth: 2);
        sector.AddNode(new DungeonNode("Sec2_Rm01", Coordinate3D.Origin));

        // Act
        var result = sector.ToString();

        // Assert
        result.Should().Be("The Depths (TheRoots, Depth 2, 1 nodes)");
    }
}
