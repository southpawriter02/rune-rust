// ═══════════════════════════════════════════════════════════════════════════════
// HarvestableIndicatorsTests.cs
// Unit tests for the HarvestableIndicators class.
// Version: 0.13.3d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for <see cref="HarvestableIndicators"/>.
/// </summary>
[TestFixture]
public class HarvestableIndicatorsTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private Mock<ILogger<HarvestableIndicators>> _mockLogger = null!;
    private ResourceStackRenderer _stackRenderer = null!;
    private GatheringDisplayConfig _config = null!;
    private HarvestableIndicators _indicators = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockLogger = new Mock<ILogger<HarvestableIndicators>>();
        _stackRenderer = new ResourceStackRenderer();
        _config = new GatheringDisplayConfig();

        _indicators = new HarvestableIndicators(
            _stackRenderer,
            _mockTerminal.Object,
            _config,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullStackRenderer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new HarvestableIndicators(
            null!,
            _mockTerminal.Object,
            _config,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("stackRenderer");
    }

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new HarvestableIndicators(
            _stackRenderer,
            null!,
            _config,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    [Test]
    public void Constructor_WithNullConfig_UsesDefault()
    {
        // Act
        var indicators = new HarvestableIndicators(
            _stackRenderer,
            _mockTerminal.Object,
            null,
            _mockLogger.Object);

        // Assert
        indicators.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RenderHarvestables Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void RenderHarvestables_WithAvailableNodes_SetsIsVisibleTrue()
    {
        // Arrange
        var nodes = CreateTestNodes(2, available: true);

        // Act
        _indicators.RenderHarvestables(nodes);

        // Assert
        _indicators.IsVisible.Should().BeTrue();
        _indicators.NodeCount.Should().Be(2);
    }

    [Test]
    public void RenderHarvestables_WithNoAvailableNodes_SetsIsVisibleFalse()
    {
        // Arrange
        var nodes = CreateTestNodes(2, available: false);

        // Act
        _indicators.RenderHarvestables(nodes);

        // Assert
        _indicators.IsVisible.Should().BeFalse();
        _indicators.NodeCount.Should().Be(0);
    }

    [Test]
    public void RenderHarvestables_WithEmptyList_SetsIsVisibleFalse()
    {
        // Arrange
        var nodes = Array.Empty<HarvestableNodeDisplayDto>();

        // Act
        _indicators.RenderHarvestables(nodes);

        // Assert
        _indicators.IsVisible.Should().BeFalse();
    }

    [Test]
    public void RenderHarvestables_FiltersUnavailableNodes()
    {
        // Arrange
        var nodes = new[]
        {
            CreateTestNode("node-1", "Herbs", true),
            CreateTestNode("node-2", "Mushrooms", false),
            CreateTestNode("node-3", "Ore", true)
        };

        // Act
        _indicators.RenderHarvestables(nodes);

        // Assert
        _indicators.NodeCount.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Selection Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetSelectedNode_AfterRender_ReturnsFirstNode()
    {
        // Arrange
        var nodes = CreateTestNodes(3, available: true);
        _indicators.RenderHarvestables(nodes);

        // Act
        var selected = _indicators.GetSelectedNode();

        // Assert
        selected.Should().NotBeNull();
        selected!.NodeId.Should().Be("node-0");
    }

    [Test]
    public void SelectNext_CyclesThroughNodes()
    {
        // Arrange
        var nodes = CreateTestNodes(3, available: true);
        _indicators.RenderHarvestables(nodes);

        // Act & Assert
        _indicators.SelectedIndex.Should().Be(0);

        _indicators.SelectNext();
        _indicators.SelectedIndex.Should().Be(1);

        _indicators.SelectNext();
        _indicators.SelectedIndex.Should().Be(2);

        _indicators.SelectNext();
        _indicators.SelectedIndex.Should().Be(0); // Wraps around
    }

    [Test]
    public void SelectPrevious_CyclesThroughNodesBackward()
    {
        // Arrange
        var nodes = CreateTestNodes(3, available: true);
        _indicators.RenderHarvestables(nodes);

        // Act & Assert
        _indicators.SelectedIndex.Should().Be(0);

        _indicators.SelectPrevious();
        _indicators.SelectedIndex.Should().Be(2); // Wraps to end

        _indicators.SelectPrevious();
        _indicators.SelectedIndex.Should().Be(1);
    }

    [Test]
    public void HighlightSelected_WithValidIndex_UpdatesSelection()
    {
        // Arrange
        var nodes = CreateTestNodes(3, available: true);
        _indicators.RenderHarvestables(nodes);

        // Act
        _indicators.HighlightSelected(2);

        // Assert
        _indicators.SelectedIndex.Should().Be(2);
    }

    [Test]
    public void HighlightSelected_WithInvalidIndex_DoesNotChange()
    {
        // Arrange
        var nodes = CreateTestNodes(3, available: true);
        _indicators.RenderHarvestables(nodes);

        // Act
        _indicators.HighlightSelected(10); // Invalid

        // Assert
        _indicators.SelectedIndex.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetNodeIcon Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ResourceCategory.Herb, "[H]")]
    [TestCase(ResourceCategory.Ore, "[O]")]
    [TestCase(ResourceCategory.Leather, "[L]")]
    [TestCase(ResourceCategory.Gem, "[G]")]
    [TestCase(ResourceCategory.Misc, "[M]")]
    public void GetNodeIcon_ReturnsCorrectIcon(ResourceCategory category, string expected)
    {
        // Act
        var icon = _indicators.GetNodeIcon(category);

        // Assert
        icon.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Hide Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Hide_WhenVisible_ClearsAndHides()
    {
        // Arrange
        var nodes = CreateTestNodes(2, available: true);
        _indicators.RenderHarvestables(nodes);

        // Act
        _indicators.Hide();

        // Assert
        _indicators.IsVisible.Should().BeFalse();
        _indicators.NodeCount.Should().Be(0);
    }

    [Test]
    public void Hide_WhenAlreadyHidden_DoesNothing()
    {
        // Act
        _indicators.Hide();

        // Assert
        _indicators.IsVisible.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════

    private static HarvestableNodeDisplayDto[] CreateTestNodes(int count, bool available)
    {
        return Enumerable.Range(0, count)
            .Select(i => CreateTestNode($"node-{i}", $"Node {i}", available))
            .ToArray();
    }

    private static HarvestableNodeDisplayDto CreateTestNode(string id, string name, bool available)
    {
        return new HarvestableNodeDisplayDto(
            NodeId: id,
            Name: name,
            ResourceTypeId: "herb",
            ResourceType: ResourceCategory.Herb,
            Quantity: 3,
            DifficultyClass: 10,
            RequiredSkill: "Herbalism",
            IsAvailable: available);
    }
}
