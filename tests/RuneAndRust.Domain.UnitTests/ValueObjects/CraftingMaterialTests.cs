using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="CraftingMaterial"/> value object.
/// Tests factory method validation and property initialization.
/// </summary>
[TestFixture]
public class CraftingMaterialTests
{
    // ===== Factory Method Tests =====

    [Test]
    public void Create_WithValidParameters_SetsAllProperties()
    {
        // Act
        var material = CraftingMaterial.Create(
            CraftingMaterialType.PlantFiber, 3, 4);

        // Assert
        material.Type.Should().Be(CraftingMaterialType.PlantFiber);
        material.Quantity.Should().Be(3);
        material.Quality.Should().Be(4);
    }

    [Test]
    public void Create_MinimumValues_Succeeds()
    {
        // Act
        var material = CraftingMaterial.Create(
            CraftingMaterialType.MineralPowder, 1, 1);

        // Assert
        material.Quantity.Should().Be(1);
        material.Quality.Should().Be(1);
    }

    [Test]
    public void Create_MaximumQuality_Succeeds()
    {
        // Act
        var material = CraftingMaterial.Create(
            CraftingMaterialType.AlchemicalReagent, 5, 5);

        // Assert
        material.Quality.Should().Be(5);
    }

    // ===== Validation Tests =====

    [Test]
    public void Create_QualityBelowMinimum_ThrowsArgumentOutOfRange()
    {
        // Act
        var act = () => CraftingMaterial.Create(
            CraftingMaterialType.PlantFiber, 2, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quality");
    }

    [Test]
    public void Create_QualityAboveMaximum_ThrowsArgumentOutOfRange()
    {
        // Act
        var act = () => CraftingMaterial.Create(
            CraftingMaterialType.PlantFiber, 2, 6);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quality");
    }

    [Test]
    public void Create_QuantityBelowMinimum_ThrowsArgumentOutOfRange()
    {
        // Act
        var act = () => CraftingMaterial.Create(
            CraftingMaterialType.PlantFiber, 0, 3);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }

    // ===== All CraftingMaterialType Values =====

    [Test]
    public void Create_AllMaterialTypes_Succeed()
    {
        // Act & Assert — ensure all enum values can be used
        foreach (var type in Enum.GetValues<CraftingMaterialType>())
        {
            var material = CraftingMaterial.Create(type, 1, 3);
            material.Type.Should().Be(type);
        }
    }
}
