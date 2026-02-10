// ═══════════════════════════════════════════════════════════════════════════════
// BlockChargeResourceTests.cs
// Unit tests for the BlockChargeResource value object.
// Version: 0.20.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class BlockChargeResourceTests
{
    [Test]
    public void CreateDefault_InitializesWithMaxCharges()
    {
        // Arrange & Act
        var resource = BlockChargeResource.CreateDefault();

        // Assert
        resource.CurrentCharges.Should().Be(3);
        resource.MaxCharges.Should().Be(3);
        resource.LastRestoredAt.Should().BeNull();
        resource.HasCharges.Should().BeTrue();
        resource.IsFullyCharged.Should().BeTrue();
    }

    [Test]
    public void TrySpend_WithSufficientCharges_ReturnsSuccessAndReducedResource()
    {
        // Arrange
        var resource = BlockChargeResource.CreateDefault();

        // Act
        var (success, updated) = resource.TrySpend(1);

        // Assert
        success.Should().BeTrue();
        updated.CurrentCharges.Should().Be(2);
        resource.CurrentCharges.Should().Be(3, "original should be unchanged");
    }

    [Test]
    public void TrySpend_WithInsufficientCharges_ReturnsFalseAndUnchanged()
    {
        // Arrange
        var resource = BlockChargeResource.CreateDefault() with { CurrentCharges = 1 };

        // Act
        var (success, updated) = resource.TrySpend(2);

        // Assert
        success.Should().BeFalse();
        updated.CurrentCharges.Should().Be(1, "should return original unchanged");
    }

    [Test]
    public void RestoreAll_SetsToMaxAndUpdatesTimestamp()
    {
        // Arrange
        var resource = BlockChargeResource.CreateDefault() with { CurrentCharges = 0 };

        // Act
        var restored = resource.RestoreAll();

        // Assert
        restored.CurrentCharges.Should().Be(3);
        restored.LastRestoredAt.Should().NotBeNull();
        restored.IsFullyCharged.Should().BeTrue();
    }

    [Test]
    public void GetBulwarkHpBonus_ReturnsCorrectMultiple()
    {
        // Arrange & Act & Assert
        BlockChargeResource.CreateDefault().GetBulwarkHpBonus().Should().Be(15);

        (BlockChargeResource.CreateDefault() with { CurrentCharges = 2 })
            .GetBulwarkHpBonus().Should().Be(10);

        (BlockChargeResource.CreateDefault() with { CurrentCharges = 0 })
            .GetBulwarkHpBonus().Should().Be(0);
    }
}
