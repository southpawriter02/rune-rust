// ═══════════════════════════════════════════════════════════════════════════════
// ResourceStackRendererTests.cs
// Unit tests for ResourceStackRenderer.
// Version: 0.13.3a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="ResourceStackRenderer"/>.
/// </summary>
[TestFixture]
public class ResourceStackRendererTests
{
    private ResourceStackRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _renderer = new ResourceStackRenderer(
            null,
            NullLogger<ResourceStackRenderer>.Instance);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET TYPE ICON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ResourceCategory.Ore, "[O]")]
    [TestCase(ResourceCategory.Herb, "[H]")]
    [TestCase(ResourceCategory.Leather, "[L]")]
    [TestCase(ResourceCategory.Gem, "[G]")]
    [TestCase(ResourceCategory.Misc, "[M]")]
    public void GetTypeIcon_WithCategory_ReturnsCorrectIcon(
        ResourceCategory category, string expectedIcon)
    {
        // Arrange & Act
        var result = _renderer.GetTypeIcon(category);

        // Assert
        result.Should().Be(expectedIcon);
    }

    [Test]
    public void GetTypeIcon_UnknownCategory_ReturnsQuestionMark()
    {
        // Arrange - Cast an invalid value
        var unknownCategory = (ResourceCategory)99;

        // Act
        var result = _renderer.GetTypeIcon(unknownCategory);

        // Assert - Should return unknown indicator
        result.Should().Be("[?]");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET TYPE COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ResourceCategory.Ore, ConsoleColor.DarkYellow)]
    [TestCase(ResourceCategory.Herb, ConsoleColor.Green)]
    [TestCase(ResourceCategory.Leather, ConsoleColor.DarkYellow)]
    [TestCase(ResourceCategory.Gem, ConsoleColor.Cyan)]
    [TestCase(ResourceCategory.Misc, ConsoleColor.White)]
    public void GetTypeColor_WithCategory_ReturnsCorrectColor(
        ResourceCategory category, ConsoleColor expectedColor)
    {
        // Arrange & Act
        var result = _renderer.GetTypeColor(category);

        // Assert
        result.Should().Be(expectedColor);
    }

    [Test]
    public void GetTypeColor_UnknownCategory_ReturnsWhite()
    {
        // Arrange
        var unknownCategory = (ResourceCategory)99;

        // Act
        var result = _renderer.GetTypeColor(unknownCategory);

        // Assert
        result.Should().Be(ConsoleColor.White);
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT QUANTITY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(24, "x24")]
    [TestCase(156, "x156")]
    [TestCase(999, "x999")]
    [TestCase(1000, "x1k")]
    [TestCase(2500, "x2k")]
    [TestCase(10000, "x10k")]
    public void FormatQuantity_WithQuantity_ReturnsCorrectFormat(
        int quantity, string expected)
    {
        // Arrange & Act
        var result = _renderer.FormatQuantity(quantity);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void FormatQuantity_WithZero_ReturnsX0()
    {
        // Arrange & Act
        var result = _renderer.FormatQuantity(0);

        // Assert
        result.Should().Be("x0");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT STACK TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatStack_WithResource_ReturnsFormattedString()
    {
        // Arrange
        var resource = new ResourceStackDisplayDto(
            ResourceId: "iron-ore",
            DisplayName: "Iron Ore",
            Description: "A common ore",
            Category: ResourceCategory.Ore,
            Quantity: 24);

        // Act
        var result = _renderer.FormatStack(resource, 20);

        // Assert
        result.Should().Contain("Iron Ore");
        result.Should().EndWith("x24");
    }

    [Test]
    public void FormatStack_WithLongName_Truncates()
    {
        // Arrange
        var resource = new ResourceStackDisplayDto(
            ResourceId: "very-long-resource-name",
            DisplayName: "Very Long Resource Name For Testing",
            Description: "A test resource",
            Category: ResourceCategory.Ore,
            Quantity: 5);

        // Act - Use a narrow width
        var result = _renderer.FormatStack(resource, 16);

        // Assert
        result.Should().Contain("..");
        result.Length.Should().BeLessOrEqualTo(16);
    }

    [Test]
    public void FormatStack_WithLargeQuantity_UsesKNotation()
    {
        // Arrange
        var resource = new ResourceStackDisplayDto(
            ResourceId: "gold-ore",
            DisplayName: "Gold Ore",
            Description: "A precious ore",
            Category: ResourceCategory.Ore,
            Quantity: 5000);

        // Act
        var result = _renderer.FormatStack(resource, 20);

        // Assert
        result.Should().EndWith("x5k");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT STACK WITH ICON TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatStackWithIcon_WithResource_IncludesIcon()
    {
        // Arrange
        var resource = new ResourceStackDisplayDto(
            ResourceId: "healing-herb",
            DisplayName: "Healing Herb",
            Description: "A medicinal plant",
            Category: ResourceCategory.Herb,
            Quantity: 15);

        // Act
        var result = _renderer.FormatStackWithIcon(resource);

        // Assert
        result.Should().StartWith("[H]");
        result.Should().Contain("Healing Herb");
        result.Should().EndWith("x15");
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
        result.PanelWidth.Should().Be(70);
        result.ColumnCount.Should().Be(3);
    }

    [Test]
    public void Constructor_WithCustomConfig_UsesProvidedConfig()
    {
        // Arrange
        var customConfig = new ResourcePanelConfig
        {
            PanelWidth = 100,
            OreColor = ConsoleColor.Red
        };

        // Act
        var renderer = new ResourceStackRenderer(customConfig);
        var color = renderer.GetTypeColor(ResourceCategory.Ore);

        // Assert
        renderer.GetConfig().PanelWidth.Should().Be(100);
        color.Should().Be(ConsoleColor.Red);
    }
}
