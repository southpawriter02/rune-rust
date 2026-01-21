// ═══════════════════════════════════════════════════════════════════════════════
// NodeTooltipTests.cs
// Unit tests for the NodeTooltip UI component.
// Version: 0.13.2d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for the NodeTooltip class.
/// </summary>
[TestFixture]
public class NodeTooltipTests
{
    private NodeTooltip _sut = null!;
    private Mock<ITerminalService> _terminalServiceMock = null!;
    private TooltipRenderer _tooltipRenderer = null!;

    // ═══════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════

    [SetUp]
    public void SetUp()
    {
        _terminalServiceMock = new Mock<ITerminalService>();
        var config = NodeTooltipConfig.CreateDefault();
        var configOptions = Options.Create(config);
        _tooltipRenderer = new TooltipRenderer(configOptions);
        _sut = new NodeTooltip(_tooltipRenderer, _terminalServiceMock.Object, configOptions);
    }

    // ═══════════════════════════════════════════════════════════════
    // VISIBILITY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsVisible_InitialState_ReturnsFalse()
    {
        // Assert
        _sut.IsVisible.Should().BeFalse();
    }

    [Test]
    public void ShowTooltip_WithValidNode_SetsIsVisibleTrue()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike");
        var bounds = new NodeBounds(10, 5, 20, 3);

        // Act
        _sut.ShowTooltip(node, NodeState.Available, bounds);

        // Assert
        _sut.IsVisible.Should().BeTrue();
    }

    [Test]
    public void HideTooltip_WhenVisible_SetsIsVisibleFalse()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike");
        var bounds = new NodeBounds(10, 5, 20, 3);
        _sut.ShowTooltip(node, NodeState.Available, bounds);

        // Act
        _sut.HideTooltip();

        // Assert
        _sut.IsVisible.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // CURRENT NODE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowTooltip_WithNode_SetsCurrentNodeId()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike");
        var bounds = new NodeBounds(10, 5, 20, 3);

        // Act
        _sut.ShowTooltip(node, NodeState.Available, bounds);

        // Assert
        _sut.CurrentNodeId.Should().Be("power-strike");
    }

    [Test]
    public void HideTooltip_ClearsCurrentNodeId()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike");
        var bounds = new NodeBounds(10, 5, 20, 3);
        _sut.ShowTooltip(node, NodeState.Available, bounds);

        // Act
        _sut.HideTooltip();

        // Assert
        _sut.CurrentNodeId.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // UNLOCK PROMPT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowUnlockPrompt_WhenTooltipVisible_SetsAwaitingConfirmationTrue()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike");
        var bounds = new NodeBounds(10, 5, 20, 3);
        _sut.ShowTooltip(node, NodeState.Available, bounds);

        // Act
        _sut.ShowUnlockPrompt(node);

        // Assert
        _sut.AwaitingConfirmation.Should().BeTrue();
    }

    [Test]
    public void ShowUnlockPrompt_WhenNotVisible_DoesNotSetAwaitingConfirmation()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike");

        // Act
        _sut.ShowUnlockPrompt(node);

        // Assert
        _sut.AwaitingConfirmation.Should().BeFalse();
    }

    [Test]
    public void HideUnlockPrompt_WhenAwaiting_SetsAwaitingConfirmationFalse()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike");
        var bounds = new NodeBounds(10, 5, 20, 3);
        _sut.ShowTooltip(node, NodeState.Available, bounds);
        _sut.ShowUnlockPrompt(node);

        // Act
        _sut.HideUnlockPrompt();

        // Assert
        _sut.AwaitingConfirmation.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // TERMINAL SERVICE INTERACTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShowTooltip_CallsTerminalService()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike");
        var bounds = new NodeBounds(10, 5, 20, 3);

        // Act
        _sut.ShowTooltip(node, NodeState.Available, bounds);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteColoredAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ConsoleColor>()),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static AbilityTreeNode CreateTestNode(string nodeId, string name)
    {
        return new AbilityTreeNode
        {
            NodeId = nodeId,
            AbilityId = nodeId,
            Name = name,
            Description = $"Test node {nodeId}",
            Tier = 1,
            PointCost = 1,
            MaxRank = 1,
            PrerequisiteNodeIds = []
        };
    }
}
