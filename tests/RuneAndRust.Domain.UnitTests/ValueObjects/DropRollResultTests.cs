using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="DropRollResult"/> value object.
/// </summary>
[TestFixture]
public class DropRollResultTests
{
    /// <summary>
    /// Verifies that Dropped creates a result with the correct tier.
    /// </summary>
    [Test]
    public void Dropped_CreatesDroppedResult_WithCorrectTier()
    {
        // Act
        var result = DropRollResult.Dropped(QualityTier.Optimized, 0.65m);

        // Assert
        result.DroppedTier.Should().Be(QualityTier.Optimized);
        result.IsNoDrop.Should().BeFalse();
        result.HasDrop.Should().BeTrue();
        result.RolledValue.Should().Be(0.65m);
    }

    /// <summary>
    /// Verifies that NoDrop creates a no-drop result with null tier.
    /// </summary>
    [Test]
    public void NoDrop_CreatesNoDropResult_WithNullTier()
    {
        // Act
        var result = DropRollResult.NoDrop(0.95m);

        // Assert
        result.DroppedTier.Should().BeNull();
        result.IsNoDrop.Should().BeTrue();
        result.HasDrop.Should().BeFalse();
        result.RolledValue.Should().Be(0.95m);
    }

    /// <summary>
    /// Verifies that Tier throws when accessing on no-drop result.
    /// </summary>
    [Test]
    public void Tier_WhenNoDrop_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = DropRollResult.NoDrop(0.95m);

        // Act
        var act = () => result.Tier;

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*no-drop*");
    }

    /// <summary>
    /// Verifies that HasDrop returns true when dropped.
    /// </summary>
    [Test]
    public void HasDrop_WhenDropped_ReturnsTrue()
    {
        // Arrange
        var result = DropRollResult.Dropped(QualityTier.ClanForged, 0.5m);

        // Assert
        result.HasDrop.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasDrop returns false when no drop.
    /// </summary>
    [Test]
    public void HasDrop_WhenNoDrop_ReturnsFalse()
    {
        // Arrange
        var result = DropRollResult.NoDrop(0.99m);

        // Assert
        result.HasDrop.Should().BeFalse();
    }

    /// <summary>
    /// Verifies ToString formats correctly for dropped result.
    /// </summary>
    [Test]
    public void ToString_WhenDropped_ReturnsFormattedString()
    {
        // Arrange
        var result = DropRollResult.Dropped(QualityTier.MythForged, 0.85m);

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("MythForged");
        str.Should().Contain("0.850");
    }

    /// <summary>
    /// Verifies ToString formats correctly for no-drop result.
    /// </summary>
    [Test]
    public void ToString_WhenNoDrop_ReturnsNoDropString()
    {
        // Arrange
        var result = DropRollResult.NoDrop(0.92m);

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("No Drop");
        str.Should().Contain("0.920");
    }
}
