// ═══════════════════════════════════════════════════════════════════════════════
// NodePositionCalculationTests.cs
// Unit tests for node position calculation logic.
// Version: 0.13.2a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for node position calculation in <see cref="TreeLayoutRenderer"/>.
/// </summary>
[TestFixture]
public class NodePositionCalculationTests
{
    private AbilityTreeDisplayConfig _config = null!;
    private TreeLayoutRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _config = new AbilityTreeDisplayConfig
        {
            StartX = 0,
            StartY = 0,
            Padding = 2,
            NodeWidth = 11,
            ColumnSpacing = 8,
            NodeAreaStartRow = 7,
            NodeHeight = 3,
            NodeVerticalSpacing = 1
        };
        _renderer = new TreeLayoutRenderer(_config, NullLogger<TreeLayoutRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // POSITION CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Tests position calculation with various tier and node index combinations.
    /// </summary>
    /// <remarks>
    /// X = StartX + Padding + ((Tier - 1) * (NodeWidth + ColumnSpacing))
    /// Y = StartY + NodeAreaStartRow + (NodeIndex * (NodeHeight + NodeVerticalSpacing))
    /// 
    /// With config: StartX=0, StartY=0, Padding=2, NodeWidth=11, ColumnSpacing=8, NodeAreaStartRow=7, NodeHeight=3, NodeVerticalSpacing=1
    /// 
    /// Tier 1, Node 0: X = 0 + 2 + (0 * 19) = 2, Y = 0 + 7 + (0 * 4) = 7
    /// Tier 1, Node 1: X = 2, Y = 7 + (1 * 4) = 11
    /// Tier 2, Node 0: X = 0 + 2 + (1 * 19) = 21, Y = 7
    /// Tier 3, Node 0: X = 0 + 2 + (2 * 19) = 40, Y = 7
    /// </remarks>
    [Test]
    [TestCase(1, 0, 2, 7)]   // Tier 1, Node 0
    [TestCase(1, 1, 2, 11)]  // Tier 1, Node 1
    [TestCase(2, 0, 21, 7)]  // Tier 2, Node 0
    [TestCase(3, 0, 40, 7)]  // Tier 3, Node 0
    public void CalculateNodePositions_WithTierAndIndex_ReturnsCorrectPosition(
        int tierNumber,
        int nodeIndex,
        int expectedX,
        int expectedY)
    {
        // Arrange - create nodes with increasing definition Y values
        var nodes = new List<NodeLayoutDto>();
        for (var i = 0; i <= nodeIndex; i++)
        {
            nodes.Add(new NodeLayoutDto(
                NodeId: $"node-{tierNumber}-{i}",
                NodeName: $"Node {tierNumber}.{i}",
                Tier: tierNumber,
                DefinitionX: 0,
                DefinitionY: i,
                BranchId: "branch"));
        }

        var tiers = new List<TierDisplayDto>
        {
            new(tierNumber, $"TIER {tierNumber}", nodes)
        };

        var dto = new TreeDisplayDto(
            TreeId: "test",
            TreeName: "Test",
            ClassId: "test",
            Tiers: tiers,
            Branches: new List<BranchDisplayDto>(),
            MaxTier: tierNumber);

        // Act
        var positions = _renderer.CalculateNodePositions(dto);
        var nodeId = $"node-{tierNumber}-{nodeIndex}";

        // Assert
        positions.Should().ContainKey(nodeId);
        positions[nodeId].X.Should().Be(expectedX, $"X position for tier {tierNumber}, node {nodeIndex}");
        positions[nodeId].Y.Should().Be(expectedY, $"Y position for tier {tierNumber}, node {nodeIndex}");
    }

    [Test]
    public void CalculateNodePositions_WithMultipleTiers_CalculatesAllPositions()
    {
        // Arrange
        var tier1Nodes = new List<NodeLayoutDto>
        {
            new("node-1-0", "Node 1.0", 1, 0, 0, "branch"),
            new("node-1-1", "Node 1.1", 1, 0, 1, "branch")
        };

        var tier2Nodes = new List<NodeLayoutDto>
        {
            new("node-2-0", "Node 2.0", 2, 0, 0, "branch")
        };

        var tiers = new List<TierDisplayDto>
        {
            new(1, "TIER 1", tier1Nodes),
            new(2, "TIER 2", tier2Nodes)
        };

        var dto = new TreeDisplayDto(
            TreeId: "test",
            TreeName: "Test",
            ClassId: "test",
            Tiers: tiers,
            Branches: new List<BranchDisplayDto>(),
            MaxTier: 2);

        // Act
        var positions = _renderer.CalculateNodePositions(dto);

        // Assert
        positions.Should().HaveCount(3);
        positions.Should().ContainKey("node-1-0");
        positions.Should().ContainKey("node-1-1");
        positions.Should().ContainKey("node-2-0");

        // Tier 2 nodes should have higher X than tier 1
        positions["node-2-0"].X.Should().BeGreaterThan(positions["node-1-0"].X);
    }

    [Test]
    public void CalculateNodePositions_WithEmptyTree_ReturnsEmptyDictionary()
    {
        // Arrange
        var dto = new TreeDisplayDto(
            TreeId: "empty",
            TreeName: "Empty Tree",
            ClassId: "test",
            Tiers: new List<TierDisplayDto>(),
            Branches: new List<BranchDisplayDto>(),
            MaxTier: 0);

        // Act
        var positions = _renderer.CalculateNodePositions(dto);

        // Assert
        positions.Should().BeEmpty();
    }
}
