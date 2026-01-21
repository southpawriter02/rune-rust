// ═══════════════════════════════════════════════════════════════════════════════
// ResourceInventoryPanelTests.cs
// Unit tests for ResourceInventoryPanel.
// Version: 0.13.3a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for <see cref="ResourceInventoryPanel"/>.
/// </summary>
[TestFixture]
public class ResourceInventoryPanelTests
{
    private ResourceStackRenderer _renderer = null!;
    private Mock<ITerminalService> _mockTerminal = null!;
    private ResourcePanelConfig _config = null!;
    private ResourceInventoryPanel _panel = null!;

    [SetUp]
    public void SetUp()
    {
        _config = new ResourcePanelConfig();
        _renderer = new ResourceStackRenderer(_config);
        _mockTerminal = new Mock<ITerminalService>();
        
        _panel = new ResourceInventoryPanel(
            _renderer,
            _mockTerminal.Object,
            _config,
            NullLogger<ResourceInventoryPanel>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // SET FILTER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void SetFilter_WithCategory_UpdatesActiveFilter()
    {
        // Arrange & Act
        _panel.SetFilter(ResourceCategory.Ore);

        // Assert
        _panel.ActiveFilter.Should().Be(ResourceCategory.Ore);
    }

    [Test]
    public void SetFilter_WithNull_ClearsFilter()
    {
        // Arrange - Set a filter first
        _panel.SetFilter(ResourceCategory.Ore);

        // Act
        _panel.SetFilter(null);

        // Assert
        _panel.ActiveFilter.Should().BeNull();
    }

    [Test]
    [TestCase(ResourceCategory.Ore)]
    [TestCase(ResourceCategory.Herb)]
    [TestCase(ResourceCategory.Leather)]
    [TestCase(ResourceCategory.Gem)]
    [TestCase(ResourceCategory.Misc)]
    public void SetFilter_WithEachCategory_SetsCorrectFilter(ResourceCategory category)
    {
        // Arrange & Act
        _panel.SetFilter(category);

        // Assert
        _panel.ActiveFilter.Should().Be(category);
    }

    // ═══════════════════════════════════════════════════════════════
    // HANDLE FILTER INPUT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ConsoleKey.D1, null)]
    [TestCase(ConsoleKey.D2, ResourceCategory.Ore)]
    [TestCase(ConsoleKey.D3, ResourceCategory.Herb)]
    [TestCase(ConsoleKey.D4, ResourceCategory.Leather)]
    [TestCase(ConsoleKey.D5, ResourceCategory.Gem)]
    [TestCase(ConsoleKey.D6, ResourceCategory.Misc)]
    public void HandleFilterInput_WithKey_SetsCorrectFilter(
        ConsoleKey key, ResourceCategory? expectedFilter)
    {
        // Arrange
        _panel.SetPosition(0, 0);

        // Act
        var handled = _panel.HandleFilterInput(key);

        // Assert
        handled.Should().BeTrue();
        _panel.ActiveFilter.Should().Be(expectedFilter);
    }

    [Test]
    [TestCase(ConsoleKey.NumPad1, null)]
    [TestCase(ConsoleKey.NumPad2, ResourceCategory.Ore)]
    [TestCase(ConsoleKey.NumPad3, ResourceCategory.Herb)]
    [TestCase(ConsoleKey.NumPad4, ResourceCategory.Leather)]
    [TestCase(ConsoleKey.NumPad5, ResourceCategory.Gem)]
    [TestCase(ConsoleKey.NumPad6, ResourceCategory.Misc)]
    public void HandleFilterInput_WithNumPadKey_SetsCorrectFilter(
        ConsoleKey key, ResourceCategory? expectedFilter)
    {
        // Arrange
        _panel.SetPosition(0, 0);

        // Act
        var handled = _panel.HandleFilterInput(key);

        // Assert
        handled.Should().BeTrue();
        _panel.ActiveFilter.Should().Be(expectedFilter);
    }

    [Test]
    public void HandleFilterInput_WithUnrecognizedKey_ReturnsFalse()
    {
        // Arrange & Act
        var handled = _panel.HandleFilterInput(ConsoleKey.A);

        // Assert
        handled.Should().BeFalse();
    }

    [Test]
    public void HandleFilterInput_WithEscapeKey_ReturnsFalse()
    {
        // Arrange & Act
        var handled = _panel.HandleFilterInput(ConsoleKey.Escape);

        // Assert
        handled.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER RESOURCES TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderResources_WithResources_CallsTerminalService()
    {
        // Arrange
        var resources = new List<ResourceStackDisplayDto>
        {
            CreateResourceDto("iron-ore", "Iron Ore", ResourceCategory.Ore, 24),
            CreateResourceDto("healing-herb", "Healing Herb", ResourceCategory.Herb, 15)
        };

        _panel.SetPosition(0, 0);

        // Act
        _panel.RenderResources(resources);

        // Assert
        _mockTerminal.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Test]
    public void RenderResources_WithEmptyList_RendersEmptyState()
    {
        // Arrange
        var resources = new List<ResourceStackDisplayDto>();
        _panel.SetPosition(0, 0);

        // Act
        _panel.RenderResources(resources);

        // Assert - Should still call WriteAt for empty state message
        _mockTerminal.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Test]
    public void RenderResources_WithFilter_OnlyRendersMatchingCategory()
    {
        // Arrange
        var resources = new List<ResourceStackDisplayDto>
        {
            CreateResourceDto("iron-ore", "Iron Ore", ResourceCategory.Ore, 24),
            CreateResourceDto("healing-herb", "Healing Herb", ResourceCategory.Herb, 15),
            CreateResourceDto("ruby", "Ruby", ResourceCategory.Gem, 5)
        };

        _panel.SetPosition(0, 0);
        _panel.SetFilter(ResourceCategory.Ore);

        // Act
        _panel.RenderResources(resources);

        // Assert - ActiveFilter should still be Ore after render
        _panel.ActiveFilter.Should().Be(ResourceCategory.Ore);
    }

    // ═══════════════════════════════════════════════════════════════
    // SET POSITION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void SetPosition_SetsPositionForRendering()
    {
        // Arrange & Act
        _panel.SetPosition(10, 5);
        
        var resources = new List<ResourceStackDisplayDto>
        {
            CreateResourceDto("iron-ore", "Iron Ore", ResourceCategory.Ore, 24)
        };

        _panel.RenderResources(resources);

        // Assert - Verify WriteAt was called with the correct starting position
        _mockTerminal.Verify(
            t => t.WriteAt(10, It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRenderer_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new ResourceInventoryPanel(
            null!,
            _mockTerminal.Object,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("stackRenderer");
    }

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new ResourceInventoryPanel(
            _renderer,
            null!,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    [Test]
    public void Constructor_WithNullConfig_UsesDefaults()
    {
        // Arrange & Act
        var panel = new ResourceInventoryPanel(
            _renderer,
            _mockTerminal.Object,
            null);

        // Assert - Should not throw, uses defaults
        panel.ActiveFilter.Should().BeNull();
    }

    #region Test Helpers

    /// <summary>
    /// Creates a test resource DTO.
    /// </summary>
    private static ResourceStackDisplayDto CreateResourceDto(
        string id,
        string name,
        ResourceCategory category,
        int quantity)
    {
        return new ResourceStackDisplayDto(
            ResourceId: id,
            DisplayName: name,
            Description: "Test resource",
            Category: category,
            Quantity: quantity);
    }

    #endregion
}
