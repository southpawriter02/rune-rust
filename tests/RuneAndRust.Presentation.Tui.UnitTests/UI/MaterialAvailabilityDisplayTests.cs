// ═══════════════════════════════════════════════════════════════════════════════
// MaterialAvailabilityDisplayTests.cs
// Unit tests for the MaterialAvailabilityDisplay class.
// Version: 0.13.3b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for MaterialAvailabilityDisplay.
/// </summary>
[TestFixture]
public class MaterialAvailabilityDisplayTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private ResourceStackRenderer _renderer = null!;
    private CraftingStationConfig _config = null!;
    private MaterialAvailabilityDisplay _display = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _renderer = new ResourceStackRenderer();
        _config = new CraftingStationConfig();
        _display = new MaterialAvailabilityDisplay(
            _renderer,
            _mockTerminal.Object,
            _config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetAvailabilityIndicator Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(10, 5, "[x]", Description = "Owned > Required")]
    [TestCase(5, 5, "[x]", Description = "Owned = Required")]
    [TestCase(4, 5, "[ ]", Description = "Owned < Required")]
    [TestCase(0, 5, "[ ]", Description = "Owned is 0")]
    public void GetAvailabilityIndicator_WithQuantities_ReturnsCorrectIndicator(
        int owned, int required, string expected)
    {
        // Arrange
        var material = CreateMaterial("test", required, optional: false);

        // Act
        var result = _display.GetAvailabilityIndicator(material, owned);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IsCraftable Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void IsCraftable_WithSufficientMaterials_ReturnsTrue()
    {
        // Arrange
        var materials = new[]
        {
            CreateMaterial("iron-ore", required: 2, optional: false),
            CreateMaterial("coal", required: 1, optional: false)
        };

        var resources = new[]
        {
            CreateResource("iron-ore", quantity: 10),
            CreateResource("coal", quantity: 5)
        };

        // Act
        var result = _display.IsCraftable(materials, resources);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsCraftable_WithMissingMaterials_ReturnsFalse()
    {
        // Arrange
        var materials = new[]
        {
            CreateMaterial("iron-ore", required: 2, optional: false),
            CreateMaterial("mithril-ore", required: 5, optional: false)
        };

        var resources = new[]
        {
            CreateResource("iron-ore", quantity: 10),
            CreateResource("mithril-ore", quantity: 2) // Short by 3
        };

        // Act
        var result = _display.IsCraftable(materials, resources);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsCraftable_WithMissingOptionalMaterials_ReturnsTrue()
    {
        // Arrange
        var materials = new[]
        {
            CreateMaterial("iron-ore", required: 2, optional: false),
            CreateMaterial("magic-dust", required: 1, optional: true) // Optional!
        };

        var resources = new[]
        {
            CreateResource("iron-ore", quantity: 10)
            // Magic dust missing, but it's optional
        };

        // Act
        var result = _display.IsCraftable(materials, resources);

        // Assert
        result.Should().BeTrue("optional materials should not affect craftability");
    }

    [Test]
    public void IsCraftable_WithResourceNotInList_ReturnsFalse()
    {
        // Arrange
        var materials = new[]
        {
            CreateMaterial("dragon-scale", required: 1, optional: false)
        };

        var resources = Array.Empty<ResourceStackDisplayDto>();

        // Act
        var result = _display.IsCraftable(materials, resources);

        // Assert
        result.Should().BeFalse("missing required material means not craftable");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new MaterialAvailabilityDisplay(
            _renderer,
            terminalService: null!,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    [Test]
    public void Constructor_WithNullResourceRenderer_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new MaterialAvailabilityDisplay(
            resourceRenderer: null!,
            _mockTerminal.Object,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("resourceRenderer");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Test Helpers
    // ═══════════════════════════════════════════════════════════════════════════

    private static MaterialRequirementDto CreateMaterial(string id, int required, bool optional)
    {
        return new MaterialRequirementDto(
            MaterialId: id,
            MaterialName: id.Replace("-", " "),
            RequiredQuantity: required,
            OwnedQuantity: 0,
            IsOptional: optional);
    }

    private static ResourceStackDisplayDto CreateResource(string id, int quantity)
    {
        return new ResourceStackDisplayDto(
            ResourceId: id,
            DisplayName: id.Replace("-", " "),
            Description: string.Empty,
            Category: ResourceCategory.Ore,
            Quantity: quantity);
    }
}
