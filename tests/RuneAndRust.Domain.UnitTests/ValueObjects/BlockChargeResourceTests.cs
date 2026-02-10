using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="BlockChargeResource"/> value object.
/// </summary>
[TestFixture]
public class BlockChargeResourceTests
{
    [Test]
    public void CreateFull_InitializesAtMaxCharges()
    {
        // Act
        var resource = BlockChargeResource.CreateFull();

        // Assert
        resource.CurrentCharges.Should().Be(BlockChargeResource.DefaultMaxCharges);
        resource.MaxCharges.Should().Be(BlockChargeResource.DefaultMaxCharges);
        resource.LastRestoredAt.Should().NotBeNull();
    }

    [Test]
    public void CreateFull_WithCustomMax_InitializesCorrectly()
    {
        // Act
        var resource = BlockChargeResource.CreateFull(5);

        // Assert
        resource.CurrentCharges.Should().Be(5);
        resource.MaxCharges.Should().Be(5);
    }

    [Test]
    public void Spend_WithSufficientCharges_ReturnsTrue()
    {
        // Arrange
        var resource = BlockChargeResource.CreateFull();

        // Act
        var result = resource.Spend(2);

        // Assert
        result.Should().BeTrue();
        resource.CurrentCharges.Should().Be(1); // 3 - 2
    }

    [Test]
    public void Spend_WithInsufficientCharges_ReturnsFalse()
    {
        // Arrange
        var resource = BlockChargeResource.CreateFull();
        resource.Spend(2); // Now at 1

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
        var resource = BlockChargeResource.CreateFull();

        // Act & Assert
        resource.Spend(0).Should().BeFalse();
        resource.Spend(-1).Should().BeFalse();
        resource.CurrentCharges.Should().Be(3); // Unchanged
    }

    [Test]
    public void Restore_AddsChargesUpToMax()
    {
        // Arrange
        var resource = BlockChargeResource.CreateFull();
        resource.Spend(2); // Now at 1

        // Act
        resource.Restore(1);

        // Assert
        resource.CurrentCharges.Should().Be(2);
    }

    [Test]
    public void Restore_DoesNotExceedMax()
    {
        // Arrange
        var resource = BlockChargeResource.CreateFull();
        resource.Spend(1); // Now at 2

        // Act
        resource.Restore(5); // Would exceed max

        // Assert
        resource.CurrentCharges.Should().Be(3); // Capped at max
    }

    [Test]
    public void RestoreAll_ResetsToMax()
    {
        // Arrange
        var resource = BlockChargeResource.CreateFull();
        resource.Spend(3); // Now at 0

        // Act
        resource.RestoreAll();

        // Assert
        resource.CurrentCharges.Should().Be(3);
    }

    [Test]
    public void GetBulwarkHpBonus_ReturnsCorrectBonus()
    {
        // Arrange
        var resource = BlockChargeResource.CreateFull(); // 3 charges

        // Assert
        resource.GetBulwarkHpBonus().Should().Be(15); // 3 * 5

        // Act — spend 1
        resource.Spend(1);

        // Assert
        resource.GetBulwarkHpBonus().Should().Be(10); // 2 * 5
    }

    [Test]
    public void IsModified_WhenAtMax_ReturnsFalse()
    {
        // Arrange
        var resource = BlockChargeResource.CreateFull();

        // Assert
        resource.IsModified().Should().BeFalse();
    }

    [Test]
    public void IsModified_WhenCharged_ReturnsTrue()
    {
        // Arrange
        var resource = BlockChargeResource.CreateFull();
        resource.Spend(1);

        // Assert
        resource.IsModified().Should().BeTrue();
    }
}
