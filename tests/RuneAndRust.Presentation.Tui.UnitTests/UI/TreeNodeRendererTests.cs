// ═══════════════════════════════════════════════════════════════════════════════
// TreeNodeRendererTests.cs
// Unit tests for TreeNodeRenderer.
// Version: 0.13.2b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for <see cref="TreeNodeRenderer"/>.
/// </summary>
[TestFixture]
public class TreeNodeRendererTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private Mock<IPrerequisiteValidator> _mockPrereqValidator = null!;
    private NodeStateRenderer _stateRenderer = null!;
    private TreeNodeRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsColor).Returns(true);
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);

        _mockPrereqValidator = new Mock<IPrerequisiteValidator>();

        _stateRenderer = new NodeStateRenderer(
            null,
            NullLogger<NodeStateRenderer>.Instance);

        _renderer = new TreeNodeRenderer(
            _stateRenderer,
            _mockTerminal.Object,
            null,
            _mockPrereqValidator.Object,
            null,
            NullLogger<TreeNodeRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // STATE DETERMINATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void DetermineNodeState_WhenUnlocked_ReturnsUnlocked()
    {
        // Arrange
        var node = CreateTestNode("power-strike");
        var player = CreateTestPlayer();
        var unlockedNodes = new HashSet<string> { "power-strike" };
        var nodeRanks = new Dictionary<string, int> { ["power-strike"] = 1 };

        // Act
        var result = _renderer.DetermineNodeState(node, player, unlockedNodes, nodeRanks);

        // Assert
        result.Should().Be(NodeState.Unlocked);
    }

    [Test]
    public void DetermineNodeState_WhenPrereqMet_ReturnsAvailable()
    {
        // Arrange
        var node = CreateTestNode("power-strike", hasPrerequisites: false);
        var player = CreateTestPlayer();
        var unlockedNodes = new HashSet<string>();
        var nodeRanks = new Dictionary<string, int>();

        _mockPrereqValidator
            .Setup(v => v.ValidatePrerequisites(player, node))
            .Returns(PrerequisiteResult.Valid());

        // Act
        var result = _renderer.DetermineNodeState(node, player, unlockedNodes, nodeRanks);

        // Assert
        result.Should().Be(NodeState.Available);
    }

    [Test]
    public void DetermineNodeState_WhenPrereqNotMet_ReturnsLocked()
    {
        // Arrange
        var node = CreateTestNode("rage", hasPrerequisites: true);
        var player = CreateTestPlayer();
        var unlockedNodes = new HashSet<string>();
        var nodeRanks = new Dictionary<string, int>();

        _mockPrereqValidator
            .Setup(v => v.ValidatePrerequisites(player, node))
            .Returns(PrerequisiteResult.Invalid("Requires Frenzy"));

        // Act
        var result = _renderer.DetermineNodeState(node, player, unlockedNodes, nodeRanks);

        // Assert
        result.Should().Be(NodeState.Locked);
    }

    [Test]
    public void DetermineNodeState_WithMultipleRanks_ReturnsUnlockedIfAnyRank()
    {
        // Arrange
        var node = CreateTestNode("frenzy", maxRank: 3);
        var player = CreateTestPlayer();
        var unlockedNodes = new HashSet<string> { "frenzy" };
        var nodeRanks = new Dictionary<string, int> { ["frenzy"] = 2 }; // 2 of 3 ranks

        // Act
        var result = _renderer.DetermineNodeState(node, player, unlockedNodes, nodeRanks);

        // Assert
        result.Should().Be(NodeState.Unlocked);
    }

    // ═══════════════════════════════════════════════════════════════
    // HIGHLIGHT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void SetHighlight_WithNodeId_TogglesHighlight()
    {
        // Arrange
        var nodeId = "power-strike";

        // Act & Assert - Turn on
        _renderer.SetHighlight(nodeId, true);
        _renderer.IsHighlighted(nodeId).Should().BeTrue();

        // Act & Assert - Turn off
        _renderer.SetHighlight(nodeId, false);
        _renderer.IsHighlighted(nodeId).Should().BeFalse();
    }

    [Test]
    public void ClearAllHighlights_RemovesAllHighlights()
    {
        // Arrange
        _renderer.SetHighlight("node1", true);
        _renderer.SetHighlight("node2", true);
        _renderer.SetHighlight("node3", true);

        // Act
        _renderer.ClearAllHighlights();

        // Assert
        _renderer.IsHighlighted("node1").Should().BeFalse();
        _renderer.IsHighlighted("node2").Should().BeFalse();
        _renderer.IsHighlighted("node3").Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // POSITION TRACKING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderNode_TracksPosition()
    {
        // Arrange
        var dto = new NodeStateDisplayDto(
            NodeId: "power-strike",
            NodeName: "Power Strike",
            State: NodeState.Available,
            CurrentRank: 0,
            MaxRank: 1,
            PointCost: 1,
            Tier: 1,
            BranchId: "berserker");

        // Act
        _renderer.RenderNode(dto, 10, 5);

        // Assert
        var position = _renderer.GetNodeScreenPosition("power-strike");
        position.Should().NotBeNull();
        position!.X.Should().Be(10);
        position.Y.Should().Be(5);
    }

    [Test]
    public void GetNodeScreenPosition_WhenNotRendered_ReturnsNull()
    {
        // Arrange & Act
        var result = _renderer.GetNodeScreenPosition("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetNodeScreenBounds_ReturnsCorrectBounds()
    {
        // Arrange
        var dto = new NodeStateDisplayDto(
            NodeId: "test-node",
            NodeName: "Test",
            State: NodeState.Unlocked,
            CurrentRank: 1,
            MaxRank: 1,
            PointCost: 1,
            Tier: 1,
            BranchId: "test");

        _renderer.RenderNode(dto, 10, 5);

        // Act
        var bounds = _renderer.GetNodeScreenBounds("test-node");

        // Assert
        bounds.Should().NotBeNull();
        bounds!.Value.TopLeft.X.Should().Be(10);
        bounds.Value.TopLeft.Y.Should().Be(5);
        bounds.Value.BottomRight.X.Should().Be(21); // 10 + 11 (node width)
        bounds.Value.BottomRight.Y.Should().Be(8);  // 5 + 3 (node height)
    }

    [Test]
    public void ClearNodePositions_RemovesAllPositions()
    {
        // Arrange
        var dto = new NodeStateDisplayDto(
            NodeId: "node1",
            NodeName: "Node 1",
            State: NodeState.Available,
            CurrentRank: 0,
            MaxRank: 1,
            PointCost: 1,
            Tier: 1,
            BranchId: "test");

        _renderer.RenderNode(dto, 0, 0);

        // Act
        _renderer.ClearNodePositions();

        // Assert
        _renderer.GetNodeScreenPosition("node1").Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // DTO CREATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CreateNodeDisplayDto_CreatesCorrectDto()
    {
        // Arrange
        var node = CreateTestNode("power-strike", maxRank: 3);
        var player = CreateTestPlayer();
        var unlockedNodes = new HashSet<string> { "power-strike" };
        var nodeRanks = new Dictionary<string, int> { ["power-strike"] = 2 };

        // Act
        var result = _renderer.CreateNodeDisplayDto(
            node, player, unlockedNodes, nodeRanks, "berserker");

        // Assert
        result.NodeId.Should().Be("power-strike");
        result.NodeName.Should().Be("Power Strike");
        result.State.Should().Be(NodeState.Unlocked);
        result.CurrentRank.Should().Be(2);
        result.MaxRank.Should().Be(3);
        result.BranchId.Should().Be("berserker");
    }

    [Test]
    public void CreateNodeDisplayDto_WithZeroRanks_ReturnsZeroCurrentRank()
    {
        // Arrange
        var node = CreateTestNode("new-node");
        var player = CreateTestPlayer();
        var unlockedNodes = new HashSet<string>();
        var nodeRanks = new Dictionary<string, int>();

        _mockPrereqValidator
            .Setup(v => v.ValidatePrerequisites(player, node))
            .Returns(PrerequisiteResult.Valid());

        // Act
        var result = _renderer.CreateNodeDisplayDto(
            node, player, unlockedNodes, nodeRanks, "test");

        // Assert
        result.CurrentRank.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR ALL TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ClearAll_ClearsPositionsAndHighlights()
    {
        // Arrange
        var dto = new NodeStateDisplayDto(
            NodeId: "node1",
            NodeName: "Node 1",
            State: NodeState.Available,
            CurrentRank: 0,
            MaxRank: 1,
            PointCost: 1,
            Tier: 1,
            BranchId: "test");

        _renderer.RenderNode(dto, 0, 0);
        _renderer.SetHighlight("node1", true);

        // Act
        _renderer.ClearAll();

        // Assert
        _renderer.GetNodeScreenPosition("node1").Should().BeNull();
        _renderer.IsHighlighted("node1").Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetConfig_ReturnsConfiguration()
    {
        // Arrange & Act
        var result = _renderer.GetConfig();

        // Assert
        result.Should().NotBeNull();
        result.NodeWidth.Should().Be(11);
        result.NodeHeight.Should().Be(3);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static AbilityTreeNode CreateTestNode(
        string nodeId,
        int maxRank = 1,
        bool hasPrerequisites = false)
    {
        return new AbilityTreeNode
        {
            NodeId = nodeId,
            Name = nodeId.Replace("-", " ").Split(' ')
                .Select(w => char.ToUpperInvariant(w[0]) + w[1..])
                .Aggregate((a, b) => $"{a} {b}"),
            AbilityId = nodeId,
            Description = $"Test node {nodeId}",
            Tier = hasPrerequisites ? 2 : 1,
            PointCost = 1,
            MaxRank = maxRank,
            PrerequisiteNodeIds = hasPrerequisites ? ["frenzy"] : []
        };
    }

    private static Player CreateTestPlayer()
    {
        return new Player("TestPlayer");
    }
}
