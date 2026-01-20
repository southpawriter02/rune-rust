using FluentAssertions;
using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for HarvestableFeatureDefinition (v0.11.0b).
/// </summary>
[TestFixture]
public class HarvestableFeatureDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidParameters_ReturnsDefinition()
    {
        // Arrange
        var featureId = "iron-ore-vein";
        var name = "Iron Ore Vein";
        var description = "A vein of iron ore embedded in the rock wall";
        var resourceId = "iron-ore";
        var minQuantity = 1;
        var maxQuantity = 5;
        var difficultyClass = 12;

        // Act
        var definition = HarvestableFeatureDefinition.Create(
            featureId, name, description, resourceId,
            minQuantity, maxQuantity, difficultyClass);

        // Assert
        definition.Should().NotBeNull();
        definition.FeatureId.Should().Be("iron-ore-vein");
        definition.Name.Should().Be("Iron Ore Vein");
        definition.Description.Should().Be("A vein of iron ore embedded in the rock wall");
        definition.ResourceId.Should().Be("iron-ore");
        definition.MinQuantity.Should().Be(1);
        definition.MaxQuantity.Should().Be(5);
        definition.DifficultyClass.Should().Be(12);
        definition.RequiredToolId.Should().BeNull();
        definition.ReplenishTurns.Should().Be(0);
        definition.Id.Should().NotBeEmpty();
    }

    [Test]
    public void Create_WithMixedCaseFeatureId_LowercasesId()
    {
        // Act
        var definition = HarvestableFeatureDefinition.Create(
            "IRON-ORE-VEIN",
            "Iron Ore Vein",
            "Description",
            "iron-ore",
            1, 5, 12);

        // Assert
        definition.FeatureId.Should().Be("iron-ore-vein");
    }

    [Test]
    public void Create_WithMixedCaseResourceId_LowercasesId()
    {
        // Act
        var definition = HarvestableFeatureDefinition.Create(
            "iron-ore-vein",
            "Iron Ore Vein",
            "Description",
            "IRON-ORE",
            1, 5, 12);

        // Assert
        definition.ResourceId.Should().Be("iron-ore");
    }

    [Test]
    public void Create_WithNullFeatureId_ThrowsArgumentException()
    {
        // Act
        var act = () => HarvestableFeatureDefinition.Create(
            null!,
            "Name",
            "Description",
            "resource-id",
            1, 5, 12);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("featureId");
    }

    [Test]
    public void Create_WithEmptyFeatureId_ThrowsArgumentException()
    {
        // Act
        var act = () => HarvestableFeatureDefinition.Create(
            "",
            "Name",
            "Description",
            "resource-id",
            1, 5, 12);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("featureId");
    }

    [Test]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        // Act
        var act = () => HarvestableFeatureDefinition.Create(
            "test-feature",
            null!,
            "Description",
            "resource-id",
            1, 5, 12);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void Create_WithNullResourceId_ThrowsArgumentException()
    {
        // Act
        var act = () => HarvestableFeatureDefinition.Create(
            "test-feature",
            "Test Feature",
            "Description",
            null!,
            1, 5, 12);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("resourceId");
    }

    [Test]
    public void Create_WithNegativeMinQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => HarvestableFeatureDefinition.Create(
            "test-feature",
            "Test Feature",
            "Description",
            "resource-id",
            -1, 5, 12);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("minQuantity");
    }

    [Test]
    public void Create_WithMaxQuantityLessThanMinQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => HarvestableFeatureDefinition.Create(
            "test-feature",
            "Test Feature",
            "Description",
            "resource-id",
            5, 3, 12); // max (3) < min (5)

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxQuantity");
    }

    [Test]
    public void Create_WithZeroDifficultyClass_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => HarvestableFeatureDefinition.Create(
            "test-feature",
            "Test Feature",
            "Description",
            "resource-id",
            1, 5, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("difficultyClass");
    }

    [Test]
    public void Create_WithNegativeDifficultyClass_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => HarvestableFeatureDefinition.Create(
            "test-feature",
            "Test Feature",
            "Description",
            "resource-id",
            1, 5, -5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("difficultyClass");
    }

    [Test]
    public void Create_WithNegativeReplenishTurns_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => HarvestableFeatureDefinition.Create(
            "test-feature",
            "Test Feature",
            "Description",
            "resource-id",
            1, 5, 12,
            replenishTurns: -10);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("replenishTurns");
    }

    // ═══════════════════════════════════════════════════════════════
    // OPTIONAL PARAMETER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithRequiredTool_SetsRequiredToolId()
    {
        // Act
        var definition = HarvestableFeatureDefinition.Create(
            "gem-cluster",
            "Gem Cluster",
            "Description",
            "ruby",
            1, 2, 16,
            requiredToolId: "pickaxe");

        // Assert
        definition.RequiredToolId.Should().Be("pickaxe");
    }

    [Test]
    public void Create_WithMixedCaseToolId_LowercasesToolId()
    {
        // Act
        var definition = HarvestableFeatureDefinition.Create(
            "gem-cluster",
            "Gem Cluster",
            "Description",
            "ruby",
            1, 2, 16,
            requiredToolId: "PICKAXE");

        // Assert
        definition.RequiredToolId.Should().Be("pickaxe");
    }

    [Test]
    public void Create_WithReplenishTurns_SetsReplenishTurns()
    {
        // Act
        var definition = HarvestableFeatureDefinition.Create(
            "herb-patch",
            "Herb Patch",
            "Description",
            "healing-herb",
            2, 6, 10,
            replenishTurns: 100);

        // Assert
        definition.ReplenishTurns.Should().Be(100);
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RequiresTool_WithNoToolSet_ReturnsFalse()
    {
        // Arrange
        var definition = HarvestableFeatureDefinition.Create(
            "iron-ore-vein",
            "Iron Ore Vein",
            "Description",
            "iron-ore",
            1, 5, 12);

        // Assert
        definition.RequiresTool.Should().BeFalse();
    }

    [Test]
    public void RequiresTool_WithToolSet_ReturnsTrue()
    {
        // Arrange
        var definition = HarvestableFeatureDefinition.Create(
            "gem-cluster",
            "Gem Cluster",
            "Description",
            "ruby",
            1, 2, 16,
            requiredToolId: "pickaxe");

        // Assert
        definition.RequiresTool.Should().BeTrue();
    }

    [Test]
    public void Replenishes_WithZeroReplenishTurns_ReturnsFalse()
    {
        // Arrange
        var definition = HarvestableFeatureDefinition.Create(
            "iron-ore-vein",
            "Iron Ore Vein",
            "Description",
            "iron-ore",
            1, 5, 12,
            replenishTurns: 0);

        // Assert
        definition.Replenishes.Should().BeFalse();
    }

    [Test]
    public void Replenishes_WithPositiveReplenishTurns_ReturnsTrue()
    {
        // Arrange
        var definition = HarvestableFeatureDefinition.Create(
            "herb-patch",
            "Herb Patch",
            "Description",
            "healing-herb",
            2, 6, 10,
            replenishTurns: 100);

        // Assert
        definition.Replenishes.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetYieldRangeDisplay_WithDifferentMinMax_ReturnsRange()
    {
        // Arrange
        var definition = HarvestableFeatureDefinition.Create(
            "iron-ore-vein",
            "Iron Ore Vein",
            "Description",
            "iron-ore",
            1, 5, 12);

        // Act
        var result = definition.GetYieldRangeDisplay();

        // Assert
        result.Should().Be("1-5");
    }

    [Test]
    public void GetYieldRangeDisplay_WithSameMinMax_ReturnsSingleValue()
    {
        // Arrange
        var definition = HarvestableFeatureDefinition.Create(
            "leather-source",
            "Leather Source",
            "Description",
            "leather",
            3, 3, 11);

        // Act
        var result = definition.GetYieldRangeDisplay();

        // Assert
        result.Should().Be("3");
    }

    [Test]
    public void GetRequirementsSummary_WithBasicFeature_ReturnsDCAndYield()
    {
        // Arrange
        var definition = HarvestableFeatureDefinition.Create(
            "iron-ore-vein",
            "Iron Ore Vein",
            "Description",
            "iron-ore",
            1, 5, 12);

        // Act
        var result = definition.GetRequirementsSummary();

        // Assert
        result.Should().Contain("DC: 12");
        result.Should().Contain("Yield: 1-5");
    }

    [Test]
    public void GetRequirementsSummary_WithToolRequired_IncludesTool()
    {
        // Arrange
        var definition = HarvestableFeatureDefinition.Create(
            "gem-cluster",
            "Gem Cluster",
            "Description",
            "ruby",
            1, 2, 16,
            requiredToolId: "pickaxe");

        // Act
        var result = definition.GetRequirementsSummary();

        // Assert
        result.Should().Contain("Requires: pickaxe");
    }

    [Test]
    public void GetRequirementsSummary_WithReplenishment_IncludesReplenishTurns()
    {
        // Arrange
        var definition = HarvestableFeatureDefinition.Create(
            "herb-patch",
            "Herb Patch",
            "Description",
            "healing-herb",
            2, 6, 10,
            replenishTurns: 100);

        // Act
        var result = definition.GetRequirementsSummary();

        // Assert
        result.Should().Contain("Replenishes: 100 turns");
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var definition = HarvestableFeatureDefinition.Create(
            "iron-ore-vein",
            "Iron Ore Vein",
            "Description",
            "iron-ore",
            1, 5, 12);

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Be("Iron Ore Vein (iron-ore-vein) - iron-ore");
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT BUILDER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void WithIcon_SetsIconPath()
    {
        // Arrange
        var definition = HarvestableFeatureDefinition.Create(
            "iron-ore-vein",
            "Iron Ore Vein",
            "Description",
            "iron-ore",
            1, 5, 12);

        // Act
        var result = definition.WithIcon("icons/features/ore_vein.png");

        // Assert
        result.IconPath.Should().Be("icons/features/ore_vein.png");
        result.Should().BeSameAs(definition); // Fluent pattern returns same instance
    }
}
