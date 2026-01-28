using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ArmorTierScaling"/> value object.
/// </summary>
[TestFixture]
public class ArmorTierScalingTests
{
    /// <summary>
    /// Verifies that Create returns a valid scaling with all properties set.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesScaling()
    {
        // Arrange
        var attrRange = AttributeBonusRange.Create(2, 4);

        // Act
        var scaling = ArmorTierScaling.Create(
            QualityTier.MythForged,
            30,
            3,
            1,
            attrRange);

        // Assert
        scaling.Tier.Should().Be(QualityTier.MythForged);
        scaling.HpBonus.Should().Be(30);
        scaling.DefenseBonus.Should().Be(3);
        scaling.PenaltyReduction.Should().Be(1);
        scaling.AttributeBonusRange.Should().Be(attrRange);
    }

    /// <summary>
    /// Verifies that Create throws for negative HP bonus.
    /// </summary>
    [Test]
    public void Create_WithNegativeHpBonus_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => ArmorTierScaling.Create(
            QualityTier.Scavenged,
            hpBonus: -5,
            defenseBonus: 1,
            penaltyReduction: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws for negative defense bonus.
    /// </summary>
    [Test]
    public void Create_WithNegativeDefenseBonus_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => ArmorTierScaling.Create(
            QualityTier.Scavenged,
            hpBonus: 10,
            defenseBonus: -1,
            penaltyReduction: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws for negative penalty reduction.
    /// </summary>
    [Test]
    public void Create_WithNegativePenaltyReduction_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => ArmorTierScaling.Create(
            QualityTier.Scavenged,
            hpBonus: 10,
            defenseBonus: 1,
            penaltyReduction: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws for penalty reduction exceeding 2.
    /// </summary>
    [Test]
    public void Create_WithPenaltyReductionExceedingMax_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => ArmorTierScaling.Create(
            QualityTier.MythForged,
            hpBonus: 30,
            defenseBonus: 3,
            penaltyReduction: 3);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that HasPenaltyReduction returns true when reduction is provided.
    /// </summary>
    [Test]
    public void HasPenaltyReduction_WhenReductionProvided_ReturnsTrue()
    {
        // Arrange
        var scaling = ArmorTierScaling.Create(
            QualityTier.MythForged,
            30, 3, 1);

        // Assert
        scaling.HasPenaltyReduction.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasPenaltyReduction returns false when reduction is zero.
    /// </summary>
    [Test]
    public void HasPenaltyReduction_WhenZero_ReturnsFalse()
    {
        // Arrange
        var scaling = ArmorTierScaling.Create(
            QualityTier.Optimized,
            20, 2, 0);

        // Assert
        scaling.HasPenaltyReduction.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that HasAttributeBonus returns true when range is provided.
    /// </summary>
    [Test]
    public void HasAttributeBonus_WhenRangeProvided_ReturnsTrue()
    {
        // Arrange
        var scaling = ArmorTierScaling.Create(
            QualityTier.ClanForged,
            15, 2, 0,
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
        var scaling = ArmorTierScaling.Create(
            QualityTier.Scavenged,
            10, 1, 0);

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
        var scaling = ArmorTierScaling.Create(
            QualityTier.MythForged,
            30, 3, 1,
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
        var scaling = ArmorTierScaling.Create(
            QualityTier.Scavenged,
            10, 1, 0);
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
        var scaling = ArmorTierScaling.Create(tier, 10, 1, 0);

        // Assert
        scaling.TierValue.Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies armor scaling values per spec.
    /// </summary>
    [Test]
    [TestCase(QualityTier.JuryRigged, 5, 1, 0)]
    [TestCase(QualityTier.Scavenged, 10, 1, 0)]
    [TestCase(QualityTier.ClanForged, 15, 2, 0)]
    [TestCase(QualityTier.Optimized, 20, 2, 0)]
    [TestCase(QualityTier.MythForged, 30, 3, 1)]
    public void Create_WithSpecValues_MatchesSpecification(
        QualityTier tier, int hp, int def, int penalty)
    {
        // Act
        var scaling = ArmorTierScaling.Create(tier, hp, def, penalty);

        // Assert
        scaling.Tier.Should().Be(tier);
        scaling.HpBonus.Should().Be(hp);
        scaling.DefenseBonus.Should().Be(def);
        scaling.PenaltyReduction.Should().Be(penalty);
    }

    /// <summary>
    /// Verifies that ToString returns useful representation.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var scaling = ArmorTierScaling.Create(
            QualityTier.MythForged,
            30, 3, 1,
            AttributeBonusRange.Tier4Default);

        // Act
        var result = scaling.ToString();

        // Assert
        result.Should().Contain("Tier 4");
        result.Should().Contain("+30 HP");
        result.Should().Contain("+3 Def");
        result.Should().Contain("-1 penalty tier");
    }
}
