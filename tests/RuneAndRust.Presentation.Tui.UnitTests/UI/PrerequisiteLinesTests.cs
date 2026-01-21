// ═══════════════════════════════════════════════════════════════════════════════
// PrerequisiteLinesTests.cs
// Unit tests for the PrerequisiteLines UI component.
// Version: 0.13.2c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for <see cref="PrerequisiteLines"/>.
/// </summary>
[TestFixture]
public class PrerequisiteLinesTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private LineRenderer _lineRenderer = null!;
    private PrerequisiteLinesConfig _config = null!;
    private PrerequisiteLines _prerequisiteLines = null!;

    // ═══════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════

    [SetUp]
    public void Setup()
    {
        _config = PrerequisiteLinesConfig.CreateDefault();
        var options = Options.Create(_config);
        _lineRenderer = new LineRenderer(options);
        _mockTerminal = new Mock<ITerminalService>();

        _prerequisiteLines = new PrerequisiteLines(
            _lineRenderer,
            _mockTerminal.Object,
            options);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetLineState TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetLineState_WhenPrerequisiteUnlocked_ReturnsSatisfied()
    {
        // Arrange
        var unlockedNodeIds = new HashSet<string> { "power-strike", "cleave" };

        // Act
        var result = _prerequisiteLines.GetLineState("power-strike", unlockedNodeIds);

        // Assert
        result.Should().Be(LineState.Satisfied);
    }

    [Test]
    public void GetLineState_WhenPrerequisiteNotUnlocked_ReturnsUnsatisfied()
    {
        // Arrange
        var unlockedNodeIds = new HashSet<string> { "cleave" };

        // Act
        var result = _prerequisiteLines.GetLineState("power-strike", unlockedNodeIds);

        // Assert
        result.Should().Be(LineState.Unsatisfied);
    }

    [Test]
    public void GetLineState_WithEmptyUnlockedSet_ReturnsUnsatisfied()
    {
        // Arrange
        var unlockedNodeIds = new HashSet<string>();

        // Act
        var result = _prerequisiteLines.GetLineState("any-node", unlockedNodeIds);

        // Assert
        result.Should().Be(LineState.Unsatisfied);
    }

    // ═══════════════════════════════════════════════════════════════
    // SetNodePositions TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void SetNodePositions_StoresPositions()
    {
        // Arrange
        var positions = new Dictionary<string, NodeScreenPosition>
        {
            ["node-1"] = new NodeScreenPosition(10, 5),
            ["node-2"] = new NodeScreenPosition(30, 5)
        };
        var bounds = new Dictionary<string, NodeBounds>
        {
            ["node-1"] = new NodeBounds(10, 5, 11, 3),
            ["node-2"] = new NodeBounds(30, 5, 11, 3)
        };

        // Act
        _prerequisiteLines.SetNodePositions(positions, bounds);

        // Assert - No exception thrown, positions are stored
        _prerequisiteLines.GetRenderedLineCount().Should().Be(0);
    }

    [Test]
    public void SetNodePositions_WithNullPositions_ThrowsArgumentNullException()
    {
        // Arrange
        var bounds = new Dictionary<string, NodeBounds>();

        // Act & Assert
        var act = () => _prerequisiteLines.SetNodePositions(null!, bounds);
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // DrawLine TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void DrawLine_WithValidPositions_DrawsLineToTerminal()
    {
        // Arrange
        var positions = new Dictionary<string, NodeScreenPosition>
        {
            ["source-node"] = new NodeScreenPosition(10, 5),
            ["target-node"] = new NodeScreenPosition(30, 5)
        };
        var bounds = new Dictionary<string, NodeBounds>
        {
            ["source-node"] = new NodeBounds(10, 5, 11, 3),
            ["target-node"] = new NodeBounds(30, 5, 11, 3)
        };
        _prerequisiteLines.SetNodePositions(positions, bounds);

        // Act
        _prerequisiteLines.DrawLine("source-node", "target-node", LineState.Satisfied);

        // Assert - Terminal should have been called
        _mockTerminal.Verify(
            t => t.WriteColoredAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), ConsoleColor.Green),
            Times.AtLeastOnce);
    }

    [Test]
    public void DrawLine_WithMissingSourcePosition_DoesNotThrow()
    {
        // Arrange
        var positions = new Dictionary<string, NodeScreenPosition>
        {
            ["target-node"] = new NodeScreenPosition(30, 5)
        };
        var bounds = new Dictionary<string, NodeBounds>();
        _prerequisiteLines.SetNodePositions(positions, bounds);

        // Act & Assert - Should not throw
        var act = () => _prerequisiteLines.DrawLine("missing-source", "target-node", LineState.Satisfied);
        act.Should().NotThrow();
    }

    [Test]
    public void DrawLine_TracksRenderedLine()
    {
        // Arrange
        var positions = new Dictionary<string, NodeScreenPosition>
        {
            ["source-node"] = new NodeScreenPosition(10, 5),
            ["target-node"] = new NodeScreenPosition(30, 5)
        };
        var bounds = new Dictionary<string, NodeBounds>();
        _prerequisiteLines.SetNodePositions(positions, bounds);

        // Act
        _prerequisiteLines.DrawLine("source-node", "target-node", LineState.Satisfied);

        // Assert
        _prerequisiteLines.GetRenderedLineCount().Should().Be(1);
        var lines = _prerequisiteLines.GetRenderedLines();
        lines.Should().ContainSingle();
        lines[0].SourceNodeId.Should().Be("source-node");
        lines[0].TargetNodeId.Should().Be("target-node");
        lines[0].State.Should().Be(LineState.Satisfied);
    }

    // ═══════════════════════════════════════════════════════════════
    // ClearLines TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ClearLines_RemovesAllTrackedLines()
    {
        // Arrange
        var positions = new Dictionary<string, NodeScreenPosition>
        {
            ["source-node"] = new NodeScreenPosition(10, 5),
            ["target-node"] = new NodeScreenPosition(30, 5)
        };
        var bounds = new Dictionary<string, NodeBounds>();
        _prerequisiteLines.SetNodePositions(positions, bounds);
        _prerequisiteLines.DrawLine("source-node", "target-node", LineState.Satisfied);

        // Act
        _prerequisiteLines.ClearLines();

        // Assert
        _prerequisiteLines.GetRenderedLineCount().Should().Be(0);
    }

    [Test]
    public void ClearLines_WritesSpacesToTerminal()
    {
        // Arrange
        var positions = new Dictionary<string, NodeScreenPosition>
        {
            ["source-node"] = new NodeScreenPosition(10, 5),
            ["target-node"] = new NodeScreenPosition(30, 5)
        };
        var bounds = new Dictionary<string, NodeBounds>();
        _prerequisiteLines.SetNodePositions(positions, bounds);
        _prerequisiteLines.DrawLine("source-node", "target-node", LineState.Satisfied);

        // Act
        _prerequisiteLines.ClearLines();

        // Assert - Should write spaces to clear the line
        _mockTerminal.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), " "),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════
    // RenderConnections TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderConnections_WithNodesHavingPrerequisites_RendersLines()
    {
        // Arrange
        var positions = new Dictionary<string, NodeScreenPosition>
        {
            ["power-strike"] = new NodeScreenPosition(10, 5),
            ["cleave"] = new NodeScreenPosition(30, 5)
        };
        var bounds = new Dictionary<string, NodeBounds>();
        _prerequisiteLines.SetNodePositions(positions, bounds);

        var cleaveNode = new AbilityTreeNode
        {
            NodeId = "cleave",
            Name = "Cleave",
            AbilityId = "cleave",
            Description = "Test node",
            Tier = 2,
            PrerequisiteNodeIds = ["power-strike"]
        };

        var nodes = new List<AbilityTreeNode> { cleaveNode };
        var unlockedNodeIds = new HashSet<string> { "power-strike" };

        // Act
        _prerequisiteLines.RenderConnections(nodes, unlockedNodeIds);

        // Assert
        _prerequisiteLines.GetRenderedLineCount().Should().Be(1);
    }

    [Test]
    public void RenderConnections_WithNoPrerequisites_RendersNoLines()
    {
        // Arrange
        var positions = new Dictionary<string, NodeScreenPosition>();
        var bounds = new Dictionary<string, NodeBounds>();
        _prerequisiteLines.SetNodePositions(positions, bounds);

        var node = new AbilityTreeNode
        {
            NodeId = "basic-node",
            Name = "Basic Node",
            AbilityId = "basic-node",
            Description = "Test node",
            Tier = 1
        };

        var nodes = new List<AbilityTreeNode> { node };
        var unlockedNodeIds = new HashSet<string>();

        // Act
        _prerequisiteLines.RenderConnections(nodes, unlockedNodeIds);

        // Assert
        _prerequisiteLines.GetRenderedLineCount().Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetConfig_ReturnsCurrentConfiguration()
    {
        // Arrange & Act
        var config = _prerequisiteLines.GetConfig();

        // Assert
        config.Should().NotBeNull();
        config.SatisfiedLineColor.Should().Be(ConsoleColor.Green);
    }

    // ═══════════════════════════════════════════════════════════════
    // ARGUMENT VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullLineRenderer_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new PrerequisiteLines(null!, _mockTerminal.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lineRenderer");
    }

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new PrerequisiteLines(_lineRenderer, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    [Test]
    public void GetLineState_WithNullNodeId_ThrowsArgumentException()
    {
        // Arrange
        var unlockedNodeIds = new HashSet<string>();

        // Act & Assert
        var act = () => _prerequisiteLines.GetLineState(null!, unlockedNodeIds);
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetLineState_WithNullUnlockedSet_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => _prerequisiteLines.GetLineState("node-id", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
