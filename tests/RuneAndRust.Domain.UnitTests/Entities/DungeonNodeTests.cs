using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class DungeonNodeTests
{
    [Test]
    public void Constructor_WithValidParameters_CreatesNode()
    {
        // Arrange & Act
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);

        // Assert
        node.Id.Should().NotBeEmpty();
        node.SectorId.Should().Be("Sec1_Rm01");
        node.Coordinate.Should().Be(Coordinate3D.Origin);
        node.Archetype.Should().Be(RoomArchetype.Chamber);
        node.IsStartNode.Should().BeFalse();
        node.IsBossArena.Should().BeFalse();
        node.GetConnectionCount().Should().Be(0);
    }

    [Test]
    public void Constructor_WithEmptySectorId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new DungeonNode("", Coordinate3D.Origin);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("sectorId");
    }

    [Test]
    public void AddConnection_AddsValidConnection()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);
        var targetId = Guid.NewGuid();

        // Act
        node.AddConnection(Direction.North, targetId);

        // Assert
        node.HasConnection(Direction.North).Should().BeTrue();
        node.GetConnection(Direction.North).Should().Be(targetId);
        node.GetConnectionCount().Should().Be(1);
    }

    [Test]
    public void AddConnection_WithEmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);

        // Act
        var act = () => node.AddConnection(Direction.North, Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("targetNodeId");
    }

    [Test]
    public void GetConnection_WhenNotConnected_ReturnsNull()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);

        // Act
        var result = node.GetConnection(Direction.South);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void SetAsStartNode_SetsIsStartNode()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);

        // Act
        node.SetAsStartNode();

        // Assert
        node.IsStartNode.Should().BeTrue();
        node.IsBossArena.Should().BeFalse();
    }

    [Test]
    public void SetAsBossArena_SetsIsBossArenaAndArchetype()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);

        // Act
        node.SetAsBossArena();

        // Assert
        node.IsBossArena.Should().BeTrue();
        node.IsStartNode.Should().BeFalse();
        node.Archetype.Should().Be(RoomArchetype.BossArena);
    }

    [Test]
    public void UpdateArchetypeFromConnections_OneConnection_SetsDeadEnd()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);
        node.AddConnection(Direction.North, Guid.NewGuid());

        // Act
        node.UpdateArchetypeFromConnections();

        // Assert
        node.Archetype.Should().Be(RoomArchetype.DeadEnd);
    }

    [Test]
    public void UpdateArchetypeFromConnections_TwoConnections_SetsCorridor()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);
        node.AddConnection(Direction.North, Guid.NewGuid());
        node.AddConnection(Direction.South, Guid.NewGuid());

        // Act
        node.UpdateArchetypeFromConnections();

        // Assert
        node.Archetype.Should().Be(RoomArchetype.Corridor);
    }

    [Test]
    public void UpdateArchetypeFromConnections_ThreeOrMoreConnections_SetsJunction()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);
        node.AddConnection(Direction.North, Guid.NewGuid());
        node.AddConnection(Direction.South, Guid.NewGuid());
        node.AddConnection(Direction.East, Guid.NewGuid());

        // Act
        node.UpdateArchetypeFromConnections();

        // Assert
        node.Archetype.Should().Be(RoomArchetype.Junction);
    }

    [Test]
    public void UpdateArchetypeFromConnections_BossArena_DoesNotChange()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);
        node.SetAsBossArena();
        node.AddConnection(Direction.North, Guid.NewGuid());

        // Act
        node.UpdateArchetypeFromConnections();

        // Assert
        node.Archetype.Should().Be(RoomArchetype.BossArena);
    }

    [Test]
    public void AddTag_AddsValidTag()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);

        // Act
        node.AddTag("Wet");

        // Assert
        node.HasTag("Wet").Should().BeTrue();
        node.Tags.Should().Contain("Wet");
    }

    [Test]
    public void AddTag_WithEmptyString_DoesNotAdd()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);

        // Act
        node.AddTag("");
        node.AddTag("  ");

        // Assert
        node.Tags.Should().BeEmpty();
    }

    [Test]
    public void AddTags_AddsMultipleTags()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);

        // Act
        node.AddTags(["Wet", "Cold", "Dark"]);

        // Assert
        node.Tags.Should().HaveCount(3);
        node.HasTag("Wet").Should().BeTrue();
        node.HasTag("Cold").Should().BeTrue();
        node.HasTag("Dark").Should().BeTrue();
    }

    [Test]
    public void RemoveTag_RemovesExistingTag()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);
        node.AddTag("Wet");

        // Act
        var result = node.RemoveTag("Wet");

        // Assert
        result.Should().BeTrue();
        node.HasTag("Wet").Should().BeFalse();
    }

    [Test]
    public void ClearTags_RemovesAllTags()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", Coordinate3D.Origin);
        node.AddTags(["Wet", "Cold", "Dark"]);

        // Act
        node.ClearTags();

        // Assert
        node.Tags.Should().BeEmpty();
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var node = new DungeonNode("Sec1_Rm01", new Coordinate3D(1, 2, 0));

        // Act
        var result = node.ToString();

        // Assert
        result.Should().Be("Sec1_Rm01 @ (1, 2, 0) [Chamber]");
    }
}
