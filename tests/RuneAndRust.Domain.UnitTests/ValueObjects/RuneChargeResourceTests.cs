using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="RuneChargeResource"/> value object.
/// Validates charge initialization, spending, generation from crafting,
/// restoration, and utility methods.
/// </summary>
[TestFixture]
public class RuneChargeResourceTests
{
    [Test]
    public void CreateFull_InitializesAtMaxCharges()
    {
        // Act
        var resource = RuneChargeResource.CreateFull();

        // Assert
        resource.CurrentCharges.Should().Be(RuneChargeResource.DefaultMaxCharges);
        resource.MaxCharges.Should().Be(RuneChargeResource.DefaultMaxCharges);
        resource.LastGeneratedAt.Should().NotBeNull();
    }

    [Test]
    public void CreateFull_WithCustomMax_InitializesCorrectly()
    {
        // Act
        var resource = RuneChargeResource.CreateFull(7);

        // Assert
        resource.CurrentCharges.Should().Be(7);
        resource.MaxCharges.Should().Be(7);
    }

    [Test]
    public void Spend_WithSufficientCharges_ReturnsTrue()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull(); // 5 charges

        // Act
        var result = resource.Spend(2);

        // Assert
        result.Should().BeTrue();
        resource.CurrentCharges.Should().Be(3); // 5 - 2
    }

    [Test]
    public void Spend_WithInsufficientCharges_ReturnsFalse()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull(); // 5 charges
        resource.Spend(4); // Now at 1

        // Act
        var result = resource.Spend(2); // Need 2, have 1

        // Assert
        result.Should().BeFalse();
        resource.CurrentCharges.Should().Be(1); // Unchanged
    }

    [Test]
    public void Spend_WithZeroOrNegative_ReturnsFalse()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull();

        // Act & Assert
        resource.Spend(0).Should().BeFalse();
        resource.Spend(-1).Should().BeFalse();
        resource.CurrentCharges.Should().Be(5); // Unchanged
    }

    [Test]
    public void GenerateFromCrafting_StandardCraft_AddsOneCharge()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull();
        resource.Spend(3); // Now at 2

        // Act
        var generated = resource.GenerateFromCrafting(isComplexCraft: false);

        // Assert
        generated.Should().Be(1);
        resource.CurrentCharges.Should().Be(3); // 2 + 1
    }

    [Test]
    public void GenerateFromCrafting_ComplexCraft_AddsTwoCharges()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull();
        resource.Spend(3); // Now at 2

        // Act
        var generated = resource.GenerateFromCrafting(isComplexCraft: true);

        // Assert
        generated.Should().Be(2);
        resource.CurrentCharges.Should().Be(4); // 2 + 2
    }

    [Test]
    public void GenerateFromCrafting_AtMaxCharges_GeneratesZero()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull(); // Already at 5

        // Act
        var generated = resource.GenerateFromCrafting(isComplexCraft: true);

        // Assert
        generated.Should().Be(0);
        resource.CurrentCharges.Should().Be(5); // Unchanged, capped at max
    }

    [Test]
    public void RestoreAll_ResetsToMax()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull();
        resource.Spend(5); // Now at 0

        // Act
        resource.RestoreAll();

        // Assert
        resource.CurrentCharges.Should().Be(5);
    }

    [Test]
    public void CanAfford_WithSufficientCharges_ReturnsTrue()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull(); // 5 charges

        // Assert
        resource.CanAfford(1).Should().BeTrue();
        resource.CanAfford(5).Should().BeTrue();
    }

    [Test]
    public void CanAfford_WithInsufficientCharges_ReturnsFalse()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull();
        resource.Spend(4); // Now at 1

        // Assert
        resource.CanAfford(2).Should().BeFalse();
    }

    [Test]
    public void CanAfford_WithZeroOrNegative_ReturnsFalse()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull();

        // Assert
        resource.CanAfford(0).Should().BeFalse();
        resource.CanAfford(-1).Should().BeFalse();
    }

    [Test]
    public void GetChargePercentage_ReturnsCorrectPercentage()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull(); // 5/5

        // Assert — full charges
        resource.GetChargePercentage().Should().BeApproximately(1.0, 0.001);

        // Act — spend 2
        resource.Spend(2); // 3/5

        // Assert
        resource.GetChargePercentage().Should().BeApproximately(0.6, 0.001);
    }

    [Test]
    public void IsModified_WhenAtMax_ReturnsFalse()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull();

        // Assert
        resource.IsModified().Should().BeFalse();
    }

    [Test]
    public void IsModified_WhenChargesSpent_ReturnsTrue()
    {
        // Arrange
        var resource = RuneChargeResource.CreateFull();
        resource.Spend(1);

        // Assert
        resource.IsModified().Should().BeTrue();
    }
}
