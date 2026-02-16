using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="AntidoteCraftResult"/> value object.
/// Tests factory methods, material summary formatting, and success/failure states.
/// </summary>
[TestFixture]
public class AntidoteCraftResultTests
{
    // ===== CreateSuccess Tests =====

    [Test]
    public void CreateSuccess_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var antidote = MedicalSupplyItem.Create(
            MedicalSupplyType.Antidote, "Crafted Antidote", "Test antidote", 3, "craft");
        var materials = new Dictionary<string, int>
        {
            ["Herbs"] = 1,
            ["Plant Fiber"] = 2,
            ["Mineral Powder"] = 1
        };

        // Act
        var result = AntidoteCraftResult.CreateSuccess(
            "Basic Antidote", 3, materials, antidote, 5);

        // Assert
        result.Success.Should().BeTrue();
        result.RecipeUsed.Should().Be("Basic Antidote");
        result.CraftedQuality.Should().Be(3);
        result.CreatedAntidote.Should().Be(antidote);
        result.SuppliesRemaining.Should().Be(5);
        result.FailureReason.Should().BeNull();
        result.Message.Should().Contain("Successfully crafted Basic Antidote (Quality 3)");
    }

    [Test]
    public void CreateSuccess_MaterialsConsumed_IsReadOnly()
    {
        // Arrange
        var antidote = MedicalSupplyItem.Create(
            MedicalSupplyType.Antidote, "Antidote", "Test", 2, "craft");
        var materials = new Dictionary<string, int> { ["Herbs"] = 1 };

        // Act
        var result = AntidoteCraftResult.CreateSuccess("Basic Antidote", 2, materials, antidote, 4);

        // Assert
        result.MaterialsConsumed.Should().ContainKey("Herbs");
        result.MaterialsConsumed.Should().BeAssignableTo<IReadOnlyDictionary<string, int>>();
    }

    // ===== CreateFailure Tests =====

    [Test]
    public void CreateFailure_SetsAllPropertiesCorrectly()
    {
        // Act
        var result = AntidoteCraftResult.CreateFailure(
            "Basic Antidote", "Insufficient Plant Fiber", 3);

        // Assert
        result.Success.Should().BeFalse();
        result.RecipeUsed.Should().Be("Basic Antidote");
        result.CraftedQuality.Should().Be(0);
        result.CreatedAntidote.Should().BeNull();
        result.SuppliesRemaining.Should().Be(3);
        result.FailureReason.Should().Be("Insufficient Plant Fiber");
        result.MaterialsConsumed.Should().BeEmpty();
        result.Message.Should().Contain("Failed to craft Basic Antidote: Insufficient Plant Fiber");
    }

    // ===== GetMaterialSummary Tests =====

    [Test]
    public void GetMaterialSummary_WithMaterials_ReturnsFormattedString()
    {
        // Arrange
        var antidote = MedicalSupplyItem.Create(
            MedicalSupplyType.Antidote, "Antidote", "Test", 3, "craft");
        var materials = new Dictionary<string, int>
        {
            ["Herbs"] = 1,
            ["Plant Fiber"] = 2,
            ["Mineral Powder"] = 1
        };
        var result = AntidoteCraftResult.CreateSuccess("Basic Antidote", 3, materials, antidote, 5);

        // Act
        var summary = result.GetMaterialSummary();

        // Assert
        summary.Should().Contain("Herbs(1)");
        summary.Should().Contain("Plant Fiber(2)");
        summary.Should().Contain("Mineral Powder(1)");
        summary.Should().Contain(" + "); // Joined by " + "
    }

    [Test]
    public void GetMaterialSummary_NoMaterials_ReturnsNone()
    {
        // Arrange
        var result = AntidoteCraftResult.CreateFailure("Basic Antidote", "No herbs", 0);

        // Act
        var summary = result.GetMaterialSummary();

        // Assert
        summary.Should().Be("None");
    }
}
