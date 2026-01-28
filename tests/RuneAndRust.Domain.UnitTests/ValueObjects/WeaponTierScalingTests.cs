using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="WeaponTierScaling"/> value object.
/// </summary>
[TestFixture]
public class WeaponTierScalingTests
{
    /// <summary>
    /// Verifies that Create returns a valid scaling with all properties set.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesScaling()
    {
        // Arrange
        var attrRange = AttributeBonusRange.Create(1, 2);

        // Act
        var scaling = WeaponTierScaling.Create(
            QualityTier.Optimized,
            "2d6",
            1,
            attrRange);

        // Assert
        scaling.Tier.Should().Be(QualityTier.Optimized);
        scaling.DamageDice.Should().Be("2d6");
        scaling.AccuracyModifier.Should().Be(1);
        scaling.AttributeBonusRange.Should().Be(attrRange);
    }

    /// <summary>
    /// Verifies that Create throws for null damage dice.
    /// </summary>
    [Test]
    public void Create_WithNullDamageDice_ThrowsArgumentException()
    {
        // Act
        var act = () => WeaponTierScaling.Create(
            QualityTier.Scavenged,
            null!,
            0);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws for empty damage dice.
    /// </summary>
    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithEmptyDamageDice_ThrowsArgumentException(string invalidDice)
    {
        // Act
        var act = () => WeaponTierScaling.Create(
            QualityTier.Scavenged,
            invalidDice,
            0);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that HasAttributeBonus returns true when range is provided.
    /// </summary>
    [Test]
    public void HasAttributeBonus_WhenRangeProvided_ReturnsTrue()
    {
        // Arrange
        var scaling = WeaponTierScaling.Create(
            QualityTier.ClanForged,
            "1d6+3",
            0,
            AttributeBonusRange.Tier2Default);

        // Assert
        scaling.HasAttributeBonus.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasAttributeBonus returns false when no range.
    /// </summary>
    [Test]
    public void HasAttributeBonus_WhenNoRange_ReturnsFalse()
    {
        // Arrange
        var scaling = WeaponTierScaling.Create(
            QualityTier.JuryRigged,
            "1d6",
            -1);

        // Assert
        scaling.HasAttributeBonus.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that RollAttributeBonus returns value in range when range exists.
    /// </summary>
    [Test]
    public void RollAttributeBonus_WithRange_ReturnsValueInRange()
    {
        // Arrange
        var scaling = WeaponTierScaling.Create(
            QualityTier.MythForged,
            "2d6+4",
            2,
            AttributeBonusRange.Tier4Default); // +2 to +4
        var random = new Random(42);

        // Act
        var result = scaling.RollAttributeBonus(random);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().BeGreaterThanOrEqualTo(2);
        result.Value.Should().BeLessThanOrEqualTo(4);
    }

    /// <summary>
    /// Verifies that RollAttributeBonus returns null when no range.
    /// </summary>
    [Test]
    public void RollAttributeBonus_WithoutRange_ReturnsNull()
    {
        // Arrange
        var scaling = WeaponTierScaling.Create(
            QualityTier.Scavenged,
            "1d6+1",
            0);
        var random = new Random(42);

        // Act
        var result = scaling.RollAttributeBonus(random);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that TierValue returns the correct integer for each tier.
    /// </summary>
    [Test]
    [TestCase(QualityTier.JuryRigged, 0)]
    [TestCase(QualityTier.Scavenged, 1)]
    [TestCase(QualityTier.ClanForged, 2)]
    [TestCase(QualityTier.Optimized, 3)]
    [TestCase(QualityTier.MythForged, 4)]
    public void TierValue_ReturnsCorrectInteger(QualityTier tier, int expectedValue)
    {
        // Arrange
        var scaling = WeaponTierScaling.Create(tier, "1d6", 0);

        // Assert
        scaling.TierValue.Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies that HasAccuracyPenalty returns true for negative modifiers.
    /// </summary>
    [Test]
    public void HasAccuracyPenalty_WhenNegativeModifier_ReturnsTrue()
    {
        // Arrange
        var scaling = WeaponTierScaling.Create(
            QualityTier.JuryRigged,
            "1d6",
            -1);

        // Assert
        scaling.HasAccuracyPenalty.Should().BeTrue();
        scaling.HasAccuracyBonus.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that HasAccuracyBonus returns true for positive modifiers.
    /// </summary>
    [Test]
    public void HasAccuracyBonus_WhenPositiveModifier_ReturnsTrue()
    {
        // Arrange
        var scaling = WeaponTierScaling.Create(
            QualityTier.MythForged,
            "2d6+4",
            2);

        // Assert
        scaling.HasAccuracyBonus.Should().BeTrue();
        scaling.HasAccuracyPenalty.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ToString returns useful representation.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var scaling = WeaponTierScaling.Create(
            QualityTier.Optimized,
            "2d6",
            1,
            AttributeBonusRange.Tier3Default);

        // Act
        var result = scaling.ToString();

        // Assert
        result.Should().Contain("Tier 3");
        result.Should().Contain("2d6");
        result.Should().Contain("+1 acc");
    }
}
