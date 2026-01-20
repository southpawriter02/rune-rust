using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for ResourceDefinition (v0.11.0a).
/// </summary>
[TestFixture]
public class ResourceDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidParameters_ReturnsDefinition()
    {
        // Arrange
        var resourceId = "iron-ore";
        var name = "Iron Ore";
        var description = "Raw iron ore";
        var category = ResourceCategory.Ore;
        var quality = ResourceQuality.Common;
        var baseValue = 5;
        var stackSize = 20;

        // Act
        var definition = ResourceDefinition.Create(
            resourceId, name, description, category, quality, baseValue, stackSize);

        // Assert
        definition.Should().NotBeNull();
        definition.ResourceId.Should().Be("iron-ore");
        definition.Name.Should().Be("Iron Ore");
        definition.Description.Should().Be("Raw iron ore");
        definition.Category.Should().Be(ResourceCategory.Ore);
        definition.Quality.Should().Be(ResourceQuality.Common);
        definition.BaseValue.Should().Be(5);
        definition.StackSize.Should().Be(20);
        definition.Id.Should().NotBeEmpty();
    }

    [Test]
    public void Create_WithMixedCaseResourceId_LowercasesId()
    {
        // Act
        var definition = ResourceDefinition.Create(
            "IRON-ORE",
            "Iron Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);

        // Assert
        definition.ResourceId.Should().Be("iron-ore");
    }

    [Test]
    public void Create_WithNullResourceId_ThrowsArgumentException()
    {
        // Act
        var act = () => ResourceDefinition.Create(
            null!,
            "Name",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("resourceId");
    }

    [Test]
    public void Create_WithEmptyResourceId_ThrowsArgumentException()
    {
        // Act
        var act = () => ResourceDefinition.Create(
            "",
            "Name",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("resourceId");
    }

    [Test]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        // Act
        var act = () => ResourceDefinition.Create(
            "test-resource",
            null!,
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void Create_WithZeroBaseValue_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => ResourceDefinition.Create(
            "test",
            "Test",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("baseValue");
    }

    [Test]
    public void Create_WithNegativeBaseValue_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => ResourceDefinition.Create(
            "test",
            "Test",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            -5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("baseValue");
    }

    [Test]
    public void Create_WithZeroStackSize_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => ResourceDefinition.Create(
            "test",
            "Test",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5,
            0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("stackSize");
    }

    [Test]
    public void Create_WithDefaultStackSize_UsesDefaultOf20()
    {
        // Act
        var definition = ResourceDefinition.Create(
            "test",
            "Test",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);

        // Assert
        definition.StackSize.Should().Be(20);
    }

    // ═══════════════════════════════════════════════════════════════
    // VALUE CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetActualValue_WithCommonQuality_ReturnsBaseValue()
    {
        // Arrange
        var definition = ResourceDefinition.Create(
            "iron-ore",
            "Iron Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);

        // Act
        var actualValue = definition.GetActualValue();

        // Assert: 5 * 1.0 = 5
        actualValue.Should().Be(5);
    }

    [Test]
    public void GetActualValue_WithFineQuality_ReturnsMultipliedValue()
    {
        // Arrange
        var definition = ResourceDefinition.Create(
            "gold-ore",
            "Gold Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Fine,
            25);

        // Act
        var actualValue = definition.GetActualValue();

        // Assert: 25 * 1.5 = 37.5 -> 37 (truncated to int)
        actualValue.Should().Be(37);
    }

    [Test]
    public void GetActualValue_WithRareQuality_ReturnsTripleValue()
    {
        // Arrange
        var definition = ResourceDefinition.Create(
            "mithril-ore",
            "Mithril Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Rare,
            100);

        // Act
        var actualValue = definition.GetActualValue();

        // Assert: 100 * 3.0 = 300
        actualValue.Should().Be(300);
    }

    [Test]
    public void GetActualValue_WithLegendaryQuality_ReturnsTenTimesValue()
    {
        // Arrange
        var definition = ResourceDefinition.Create(
            "legendary-gem",
            "Legendary Gem",
            "Description",
            ResourceCategory.Gem,
            ResourceQuality.Legendary,
            100);

        // Act
        var actualValue = definition.GetActualValue();

        // Assert: 100 * 10.0 = 1000
        actualValue.Should().Be(1000);
    }

    [Test]
    public void GetCraftingBonus_ReturnsQualityCraftingBonus()
    {
        // Arrange
        var common = ResourceDefinition.Create("c", "C", "D", ResourceCategory.Ore, ResourceQuality.Common, 1);
        var fine = ResourceDefinition.Create("f", "F", "D", ResourceCategory.Ore, ResourceQuality.Fine, 1);
        var rare = ResourceDefinition.Create("r", "R", "D", ResourceCategory.Ore, ResourceQuality.Rare, 1);
        var legendary = ResourceDefinition.Create("l", "L", "D", ResourceCategory.Ore, ResourceQuality.Legendary, 1);

        // Act & Assert
        common.GetCraftingBonus().Should().Be(0);
        fine.GetCraftingBonus().Should().Be(1);
        rare.GetCraftingBonus().Should().Be(2);
        legendary.GetCraftingBonus().Should().Be(3);
    }

    // ═══════════════════════════════════════════════════════════════
    // STACKING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CanStackWith_SameResourceId_ReturnsTrue()
    {
        // Arrange
        var resource1 = ResourceDefinition.Create(
            "iron-ore",
            "Iron Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);
        var resource2 = ResourceDefinition.Create(
            "iron-ore",
            "Iron Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);

        // Act & Assert
        resource1.CanStackWith(resource2).Should().BeTrue();
    }

    [Test]
    public void CanStackWith_DifferentResourceId_ReturnsFalse()
    {
        // Arrange
        var resource1 = ResourceDefinition.Create(
            "iron-ore",
            "Iron Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);
        var resource2 = ResourceDefinition.Create(
            "copper-ore",
            "Copper Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            3);

        // Act & Assert
        resource1.CanStackWith(resource2).Should().BeFalse();
    }

    [Test]
    public void CanStackWith_NullResource_ThrowsArgumentNullException()
    {
        // Arrange
        var resource = ResourceDefinition.Create(
            "iron-ore",
            "Iron Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);

        // Act
        var act = () => resource.CanStackWith(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("other");
    }

    // ═══════════════════════════════════════════════════════════════
    // CATEGORY AND QUALITY QUERY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsCategory_WithMatchingCategory_ReturnsTrue()
    {
        // Arrange
        var definition = ResourceDefinition.Create(
            "iron-ore",
            "Iron Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);

        // Act & Assert
        definition.IsCategory(ResourceCategory.Ore).Should().BeTrue();
        definition.IsCategory(ResourceCategory.Herb).Should().BeFalse();
    }

    [Test]
    public void MeetsQualityRequirement_WithHigherQuality_ReturnsTrue()
    {
        // Arrange
        var rare = ResourceDefinition.Create(
            "ruby",
            "Ruby",
            "Description",
            ResourceCategory.Gem,
            ResourceQuality.Rare,
            100);

        // Act & Assert
        rare.MeetsQualityRequirement(ResourceQuality.Common).Should().BeTrue();
        rare.MeetsQualityRequirement(ResourceQuality.Fine).Should().BeTrue();
        rare.MeetsQualityRequirement(ResourceQuality.Rare).Should().BeTrue();
        rare.MeetsQualityRequirement(ResourceQuality.Legendary).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ToString_WithCommonQuality_ReturnsJustName()
    {
        // Arrange
        var definition = ResourceDefinition.Create(
            "iron-ore",
            "Iron Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Be("Iron Ore");
    }

    [Test]
    public void ToString_WithHigherQuality_ReturnsNameWithQuality()
    {
        // Arrange
        var definition = ResourceDefinition.Create(
            "gold-ore",
            "Gold Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Fine,
            25);

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Be("Gold Ore (Fine)");
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT BUILDER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void WithIcon_SetsIconPath()
    {
        // Arrange
        var definition = ResourceDefinition.Create(
            "iron-ore",
            "Iron Ore",
            "Description",
            ResourceCategory.Ore,
            ResourceQuality.Common,
            5);

        // Act
        var result = definition.WithIcon("icons/resources/iron_ore.png");

        // Assert
        result.IconPath.Should().Be("icons/resources/iron_ore.png");
        result.Should().BeSameAs(definition); // Fluent pattern returns same instance
    }
}
