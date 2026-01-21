// ═══════════════════════════════════════════════════════════════════════════════
// TreeLayoutRendererTests.cs
// Unit tests for TreeLayoutRenderer.
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
/// Unit tests for <see cref="TreeLayoutRenderer"/>.
/// </summary>
[TestFixture]
public class TreeLayoutRendererTests
{
    private TreeLayoutRenderer _renderer = null!;
    private AbilityTreeDisplayConfig _config = null!;

    [SetUp]
    public void SetUp()
    {
        _config = AbilityTreeDisplayConfig.CreateDefault();
        _renderer = new TreeLayoutRenderer(_config, NullLogger<TreeLayoutRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET TIER LABEL TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetTierLabel_WithTierNumber_ReturnsFormattedLabel()
    {
        // Arrange
        var tierNumber = 2;

        // Act
        var result = _renderer.GetTierLabel(tierNumber);

        // Assert
        result.Should().Be("TIER 2");
    }

    [Test]
    [TestCase(1, "TIER 1")]
    [TestCase(3, "TIER 3")]
    [TestCase(5, "TIER 5")]
    public void GetTierLabel_WithVariousTiers_ReturnsCorrectLabel(int tier, string expected)
    {
        // Arrange & Act
        var result = _renderer.GetTierLabel(tier);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT TREE HEADER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatTreeHeader_WithTreeName_ReturnsCenteredHeader()
    {
        // Arrange
        var treeName = "Warrior Ability Tree";
        var totalWidth = 60;

        // Act
        var result = _renderer.FormatTreeHeader(treeName, totalWidth);

        // Assert
        result.Should().Contain("WARRIOR ABILITY TREE");
        result.Length.Should().Be(totalWidth - 4); // Account for borders
    }

    [Test]
    public void FormatTreeHeader_WithLongName_TruncatesToFit()
    {
        // Arrange
        var treeName = "This Is An Extremely Long Ability Tree Name That Should Be Truncated";
        var totalWidth = 40;
        var expectedMaxLength = totalWidth - 4;

        // Act
        var result = _renderer.FormatTreeHeader(treeName, totalWidth);

        // Assert
        result.Length.Should().Be(expectedMaxLength);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT NODE BOX TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatNodeBox_WithNodeName_ReturnsValidBoxStructure()
    {
        // Arrange
        var node = new NodeLayoutDto(
            NodeId: "power-strike",
            NodeName: "Power Strike",
            Tier: 1,
            DefinitionX: 0,
            DefinitionY: 0,
            BranchId: "berserker");

        // Act - width 11 means inner width 9, and "Power Strike" (12 chars) splits into 2 lines
        var result = _renderer.FormatNodeBox(node, width: 11, height: 4);

        // Assert
        var lines = result.Split('\n');
        lines.Should().HaveCount(4); // top border + 2 content lines + bottom border
        lines[0].Should().StartWith("┌").And.EndWith("┐");
        lines[1].Should().StartWith("│").And.EndWith("│");
        lines[2].Should().StartWith("│").And.EndWith("│");
        lines[3].Should().StartWith("└").And.EndWith("┘");
    }

    [Test]
    public void FormatNodeBox_WithShortName_CentersContent()
    {
        // Arrange
        var node = new NodeLayoutDto(
            NodeId: "rage",
            NodeName: "Rage",
            Tier: 1,
            DefinitionX: 0,
            DefinitionY: 0,
            BranchId: "berserker");

        // Act
        var result = _renderer.FormatNodeBox(node, width: 11, height: 3);

        // Assert
        var lines = result.Split('\n');
        lines[1].Should().Contain("Rage");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT BRANCH HEADER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatBranchHeader_WithBranchName_ReturnsBracketedUppercase()
    {
        // Arrange
        var branchName = "Berserker";

        // Act
        var result = _renderer.FormatBranchHeader(branchName);

        // Assert
        result.Should().Be("[ BERSERKER ]");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET TREE WIDTH TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetTreeWidth_WithMaxTier_CalculatesCorrectWidth()
    {
        // Arrange
        var maxTier = 3;
        // Expected: (2 * 2) + (3 * 11) + ((3 - 1) * 8) = 4 + 33 + 16 = 53

        // Act
        var result = _renderer.GetTreeWidth(maxTier);

        // Assert
        result.Should().Be(53);
    }
}
