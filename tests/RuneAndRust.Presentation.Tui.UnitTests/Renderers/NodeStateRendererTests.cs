// ═══════════════════════════════════════════════════════════════════════════════
// NodeStateRendererTests.cs
// Unit tests for NodeStateRenderer.
// Version: 0.13.2b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="NodeStateRenderer"/>.
/// </summary>
[TestFixture]
public class NodeStateRendererTests
{
    private NodeStateRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _renderer = new NodeStateRenderer(
            null,
            NullLogger<NodeStateRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET STATE INDICATOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(NodeState.Unlocked, "[x]")]
    [TestCase(NodeState.Available, "( )")]
    [TestCase(NodeState.Locked, "[L]")]
    public void GetStateIndicator_ForEachState_ReturnsCorrectIndicator(
        NodeState state, string expectedIndicator)
    {
        // Arrange & Act
        var result = _renderer.GetStateIndicator(state);

        // Assert
        result.Should().Be(expectedIndicator);
    }

    [Test]
    public void GetStateIndicator_UnknownState_ReturnsLockedIndicator()
    {
        // Arrange - Cast an invalid value
        var unknownState = (NodeState)99;

        // Act
        var result = _renderer.GetStateIndicator(unknownState);

        // Assert - Should default to locked
        result.Should().Be("[L]");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET STATE COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(NodeState.Unlocked, ConsoleColor.Green)]
    [TestCase(NodeState.Available, ConsoleColor.Yellow)]
    [TestCase(NodeState.Locked, ConsoleColor.DarkGray)]
    public void GetStateColor_ForEachState_ReturnsCorrectColor(
        NodeState state, ConsoleColor expectedColor)
    {
        // Arrange & Act
        var result = _renderer.GetStateColor(state);

        // Assert
        result.Should().Be(expectedColor);
    }

    [Test]
    public void GetStateColor_UnknownState_ReturnsLockedColor()
    {
        // Arrange
        var unknownState = (NodeState)99;

        // Act
        var result = _renderer.GetStateColor(unknownState);

        // Assert
        result.Should().Be(ConsoleColor.DarkGray);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET HIGHLIGHT COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetHighlightColor_ReturnsConfiguredColor()
    {
        // Arrange & Act
        var result = _renderer.GetHighlightColor();

        // Assert
        result.Should().Be(ConsoleColor.White);
    }

    [Test]
    public void GetRankProgressColor_ReturnsConfiguredColor()
    {
        // Arrange & Act
        var result = _renderer.GetRankProgressColor();

        // Assert
        result.Should().Be(ConsoleColor.Cyan);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT NODE CONTENT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatNodeContent_WithValidDto_ReturnsFormattedContent()
    {
        // Arrange
        var dto = new NodeStateDisplayDto(
            NodeId: "power-strike",
            NodeName: "Power Strike",
            State: NodeState.Available,
            CurrentRank: 0,
            MaxRank: 3,
            PointCost: 1,
            Tier: 1,
            BranchId: "berserker");

        // Act
        var result = _renderer.FormatNodeContent(dto);

        // Assert
        result.Should().Be("( ) Power Strike");
    }

    [Test]
    public void FormatNodeContent_WithUnlockedState_ShowsUnlockedIndicator()
    {
        // Arrange
        var dto = new NodeStateDisplayDto(
            NodeId: "frenzy",
            NodeName: "Frenzy",
            State: NodeState.Unlocked,
            CurrentRank: 2,
            MaxRank: 3,
            PointCost: 1,
            Tier: 1,
            BranchId: "berserker");

        // Act
        var result = _renderer.FormatNodeContent(dto);

        // Assert
        result.Should().Contain("[x]");
        result.Should().Contain("Frenzy");
    }

    [Test]
    public void FormatNodeContent_WithLockedState_ShowsLockedIndicator()
    {
        // Arrange
        var dto = new NodeStateDisplayDto(
            NodeId: "rage",
            NodeName: "Rage",
            State: NodeState.Locked,
            CurrentRank: 0,
            MaxRank: 1,
            PointCost: 2,
            Tier: 2,
            BranchId: "berserker");

        // Act
        var result = _renderer.FormatNodeContent(dto);

        // Assert
        result.Should().Contain("[L]");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT RANK PROGRESS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 3, "0/3")]
    [TestCase(1, 3, "1/3")]
    [TestCase(2, 3, "2/3")]
    [TestCase(3, 3, "3/3")]
    public void FormatRankProgress_WithMultiRank_ReturnsProgress(
        int currentRank, int maxRank, string expectedProgress)
    {
        // Arrange & Act
        var result = _renderer.FormatRankProgress(currentRank, maxRank);

        // Assert
        result.Should().Be(expectedProgress);
    }

    [Test]
    public void FormatRankProgress_WithSingleRank_ReturnsEmpty()
    {
        // Arrange
        var currentRank = 1;
        var maxRank = 1;

        // Act
        var result = _renderer.FormatRankProgress(currentRank, maxRank);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void FormatRankProgress_WithZeroRanks_ReturnsEmpty()
    {
        // Arrange
        var currentRank = 0;
        var maxRank = 1;

        // Act
        var result = _renderer.FormatRankProgress(currentRank, maxRank);

        // Assert
        result.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE RANK DISPLAY DTO TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CreateRankDisplayDto_CreatesCorrectDto()
    {
        // Arrange
        var currentRank = 2;
        var maxRank = 3;

        // Act
        var result = _renderer.CreateRankDisplayDto(currentRank, maxRank);

        // Assert
        result.CurrentRank.Should().Be(2);
        result.MaxRank.Should().Be(3);
        result.IsMaxRank.Should().BeFalse();
        result.ProgressString.Should().Be("2/3");
    }

    [Test]
    public void CreateRankDisplayDto_AtMaxRank_ReportsMaxRank()
    {
        // Arrange
        var currentRank = 3;
        var maxRank = 3;

        // Act
        var result = _renderer.CreateRankDisplayDto(currentRank, maxRank);

        // Assert
        result.IsMaxRank.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // TRUNCATE NAME TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void TruncateName_ShortName_ReturnsUnchanged()
    {
        // Arrange
        var name = "Strike";
        var maxWidth = 10;

        // Act
        var result = _renderer.TruncateName(name, maxWidth);

        // Assert
        result.Should().Be("Strike");
    }

    [Test]
    public void TruncateName_LongName_TruncatesWithEllipsis()
    {
        // Arrange
        var name = "Power Strike";
        var maxWidth = 8;

        // Act
        var result = _renderer.TruncateName(name, maxWidth);

        // Assert
        result.Should().Be("Power...");
        result.Length.Should().Be(8);
    }

    [Test]
    public void TruncateName_ExactLength_ReturnsUnchanged()
    {
        // Arrange
        var name = "Strike";
        var maxWidth = 6;

        // Act
        var result = _renderer.TruncateName(name, maxWidth);

        // Assert
        result.Should().Be("Strike");
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
}
