// ═══════════════════════════════════════════════════════════════════════════════
// TooltipRendererTests.cs
// Unit tests for the TooltipRenderer.
// Version: 0.13.2d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for the TooltipRenderer class.
/// </summary>
[TestFixture]
public class TooltipRendererTests
{
    private TooltipRenderer _sut = null!;
    private IOptions<NodeTooltipConfig> _configOptions = null!;

    // ═══════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════

    [SetUp]
    public void SetUp()
    {
        var config = NodeTooltipConfig.CreateDefault();
        _configOptions = Options.Create(config);
        _sut = new TooltipRenderer(_configOptions);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT ABILITY DETAILS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatAbilityDetails_WithValidNode_ReturnsExpectedTitle()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike", tier: 1);

        // Act
        var result = _sut.FormatAbilityDetails(node, NodeState.Available);

        // Assert
        result.Title.Should().Be("POWER STRIKE");
    }

    [Test]
    public void FormatAbilityDetails_WithTierOne_ReturnsExpectedSubtitle()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike", tier: 1, pointCost: 1);

        // Act
        var result = _sut.FormatAbilityDetails(node, NodeState.Available);

        // Assert
        result.Subtitle.Should().Be("Tier 1 | Cost: 1 point");
    }

    [Test]
    public void FormatAbilityDetails_WithMultiRankNode_IncludesRankSection()
    {
        // Arrange
        var node = CreateTestNode("frenzy", "Frenzy", maxRank: 3);

        // Act
        var result = _sut.FormatAbilityDetails(node, NodeState.Available);

        // Assert
        result.Sections.Should().HaveCountGreaterThanOrEqualTo(1);
        var rankSection = result.Sections.FirstOrDefault(s => s.Lines.Any(l => l.Label == "Max Rank"));
        rankSection.Should().NotBeNull();
    }

    [Test]
    public void FormatAbilityDetails_WhenUnlocked_SetsStatusToUnlocked()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike");

        // Act
        var result = _sut.FormatAbilityDetails(node, NodeState.Unlocked);

        // Assert
        result.Footer.Should().Contain("UNLOCKED");
    }

    [Test]
    public void FormatAbilityDetails_WhenAvailable_IncludesUnlockPrompt()
    {
        // Arrange
        var node = CreateTestNode("power-strike", "Power Strike");

        // Act
        var result = _sut.FormatAbilityDetails(node, NodeState.Available);

        // Assert
        result.Footer.Should().Contain("AVAILABLE");
        result.Footer.Should().Contain("[U]");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT REQUIREMENTS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatRequirements_WithSatisfiedPrereqs_MarksSatisfied()
    {
        // Arrange
        var prereqs = new List<string> { "power-strike", "cleave" };
        var unlocked = new HashSet<string> { "power-strike" };

        // Act
        var result = _sut.FormatRequirements(prereqs, unlocked);

        // Assert
        result.Header.Should().Be("Prerequisites:");
        result.Lines.Should().HaveCount(2);
        result.Lines[0].IsSatisfied.Should().BeTrue();
        result.Lines[1].IsSatisfied.Should().BeFalse();
    }

    [Test]
    public void FormatRequirements_WithNoUnlocked_NoneAreSatisfied()
    {
        // Arrange
        var prereqs = new List<string> { "power-strike", "cleave" };
        var unlocked = new HashSet<string>();

        // Act
        var result = _sut.FormatRequirements(prereqs, unlocked);

        // Assert
        result.Lines.Should().AllSatisfy(line => line.IsSatisfied.Should().BeFalse());
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT COST TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatCost_WithOnePoint_ReturnsSingular()
    {
        // Act
        var result = _sut.FormatCost(1);

        // Assert
        result.Should().Be("1 point");
    }

    [Test]
    public void FormatCost_WithMultiplePoints_ReturnsPlural()
    {
        // Act
        var result = _sut.FormatCost(3);

        // Assert
        result.Should().Be("3 points");
    }

    // ═══════════════════════════════════════════════════════════════
    // BUILD TOOLTIP LINES TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void BuildTooltipLines_WithValidTooltip_ReturnsFormattedLines()
    {
        // Arrange
        var tooltip = new TooltipDisplayDto(
            Title: "POWER STRIKE",
            Subtitle: "Tier 1 | Cost: 1 point",
            Description: "A powerful attack",
            Sections: [],
            Footer: "Status: AVAILABLE");

        // Act
        var result = _sut.BuildTooltipLines(tooltip, 40);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Should().Contain("POWER STRIKE");
    }

    // ═══════════════════════════════════════════════════════════════
    // TOOLTIP POSITION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetTooltipPosition_WithNodeBounds_ReturnsPositionToRight()
    {
        // Arrange
        var bounds = new NodeBounds(10, 5, 20, 3);

        // Act
        var result = _sut.GetTooltipPosition(bounds, 40, 10);

        // Assert
        result.X.Should().BeGreaterThan(bounds.X);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static AbilityTreeNode CreateTestNode(
        string nodeId,
        string name,
        int tier = 1,
        int pointCost = 1,
        int maxRank = 1)
    {
        return new AbilityTreeNode
        {
            NodeId = nodeId,
            AbilityId = nodeId,
            Name = name,
            Description = $"Test node {nodeId}",
            Tier = tier,
            PointCost = pointCost,
            MaxRank = maxRank,
            PrerequisiteNodeIds = []
        };
    }
}
