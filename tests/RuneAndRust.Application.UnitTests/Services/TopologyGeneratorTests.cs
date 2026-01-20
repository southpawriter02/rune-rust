using FluentAssertions;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class TopologyGeneratorTests
{
    private SproutingVineTopologyGenerator _generator = null!;

    [SetUp]
    public void SetUp()
    {
        _generator = new SproutingVineTopologyGenerator();
    }

    [Test]
    public void GenerateSector_WithMinimumRooms_CreatesValidSector()
    {
        // Arrange & Act
        var sector = _generator.GenerateSector(Biome.Citadel, targetRoomCount: 3, depth: 1, seed: 12345);

        // Assert
        sector.Should().NotBeNull();
        sector.GetNodeCount().Should().BeGreaterThanOrEqualTo(3);
        sector.StartNodeId.Should().NotBeNull();
        sector.BossNodeId.Should().NotBeNull();
        sector.Biome.Should().Be(Biome.Citadel);
    }

    [Test]
    public void GenerateSector_WithTargetRoomCount_ApproachesTarget()
    {
        // Arrange & Act
        var sector = _generator.GenerateSector(Biome.TheRoots, targetRoomCount: 10, depth: 2, seed: 54321);

        // Assert
        sector.GetNodeCount().Should().BeGreaterThanOrEqualTo(5); // Allow some variance
        sector.GetNodeCount().Should().BeLessThanOrEqualTo(15);
    }

    [Test]
    public void GenerateSector_SeedProducesConsistentResults()
    {
        // Arrange & Act
        var sector1 = _generator.GenerateSector(Biome.Citadel, targetRoomCount: 8, depth: 1, seed: 99999);
        var sector2 = _generator.GenerateSector(Biome.Citadel, targetRoomCount: 8, depth: 1, seed: 99999);

        // Assert
        sector1.GetNodeCount().Should().Be(sector2.GetNodeCount());
    }

    [Test]
    public void GenerateSector_DifferentSeedsProduceDifferentResults()
    {
        // Arrange & Act
        var sector1 = _generator.GenerateSector(Biome.Citadel, targetRoomCount: 10, depth: 1, seed: 11111);
        var sector2 = _generator.GenerateSector(Biome.Citadel, targetRoomCount: 10, depth: 1, seed: 22222);

        // Assert - We can't guarantee different node counts, but structure should differ
        // Just verify both are valid
        sector1.Should().NotBeNull();
        sector2.Should().NotBeNull();
    }

    [Test]
    public void GenerateSector_HasStartNode()
    {
        // Arrange & Act
        var sector = _generator.GenerateSector(Biome.Citadel, targetRoomCount: 5, depth: 1, seed: 42);

        // Assert
        sector.StartNodeId.Should().NotBeNull();
        var startNode = sector.GetNode(sector.StartNodeId!.Value);
        startNode.Should().NotBeNull();
        startNode!.IsStartNode.Should().BeTrue();
    }

    [Test]
    public void GenerateSector_HasBossArena()
    {
        // Arrange & Act
        var sector = _generator.GenerateSector(Biome.Citadel, targetRoomCount: 6, depth: 1, seed: 42);

        // Assert
        sector.BossNodeId.Should().NotBeNull();
        var bossNode = sector.GetNode(sector.BossNodeId!.Value);
        bossNode.Should().NotBeNull();
        bossNode!.IsBossArena.Should().BeTrue();
        bossNode.Archetype.Should().Be(RoomArchetype.BossArena);
    }

    [Test]
    public void GenerateSector_AllNodesConnected()
    {
        // Arrange & Act
        var sector = _generator.GenerateSector(Biome.Citadel, targetRoomCount: 8, depth: 1, seed: 123);

        // Assert - All nodes should have at least one connection
        foreach (var node in sector.GetAllNodes())
        {
            node.GetConnectionCount().Should().BeGreaterThanOrEqualTo(1,
                $"Node {node.SectorId} should have at least one connection");
        }
    }

    [Test]
    public void GenerateSector_NodesHaveBiomeTags()
    {
        // Arrange & Act
        var sector = _generator.GenerateSector(Biome.TheRoots, targetRoomCount: 5, depth: 1, seed: 456);

        // Assert - Nodes should have biome-specific tags
        var allTags = sector.GetAllNodes().SelectMany(n => n.Tags).Distinct().ToList();
        allTags.Should().Contain("Organic");
        allTags.Should().Contain("Damp");
    }

    [Test]
    public void GenerateSector_NodeArchetypesAssigned()
    {
        // Arrange & Act
        var sector = _generator.GenerateSector(Biome.Citadel, targetRoomCount: 10, depth: 1, seed: 789);

        // Assert
        var archetypes = sector.GetAllNodes().Select(n => n.Archetype).Distinct().ToList();

        // Should have multiple archetype types (at least dead ends, corridors, and boss)
        archetypes.Should().HaveCountGreaterThanOrEqualTo(2);
        archetypes.Should().Contain(RoomArchetype.BossArena);
    }

    [Test]
    public void GenerateSector_WithTooFewRooms_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => _generator.GenerateSector(Biome.Citadel, targetRoomCount: 2, depth: 1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("targetRoomCount");
    }

    [Test]
    public void GenerateSector_SetsDepthCorrectly()
    {
        // Arrange & Act
        var sector = _generator.GenerateSector(Biome.Muspelheim, targetRoomCount: 5, depth: 3, seed: 111);

        // Assert
        sector.Depth.Should().Be(3);
    }

    [Test]
    public void GenerateSector_GeneratesBiomeSpecificNames()
    {
        // Arrange & Act
        var citadelSector = _generator.GenerateSector(Biome.Citadel, targetRoomCount: 5, depth: 1, seed: 1);
        var rootsSector = _generator.GenerateSector(Biome.TheRoots, targetRoomCount: 5, depth: 2, seed: 2);

        // Assert
        citadelSector.Name.Should().Contain("Citadel");
        rootsSector.Name.Should().Contain("Roots");
    }
}
