using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="SkillBonus"/> value object.
/// </summary>
/// <remarks>
/// Verifies correct behavior of factory methods, computed properties,
/// skill matching logic, and ToString formatting.
/// </remarks>
[TestFixture]
public class SkillBonusTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create with valid parameters creates a skill bonus.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesSkillBonus()
    {
        // Arrange & Act
        var bonus = SkillBonus.Create("social", 1);

        // Assert
        bonus.SkillId.Should().Be("social");
        bonus.BonusAmount.Should().Be(1);
        bonus.NormalizedSkillId.Should().Be("social");
        bonus.IsPositive.Should().BeTrue();
        bonus.IsPenalty.Should().BeFalse();
        bonus.IsNeutral.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create normalizes skill ID to lowercase.
    /// </summary>
    [Test]
    public void Create_WithMixedCaseSkillId_NormalizesToLowercase()
    {
        // Arrange & Act
        var bonus = SkillBonus.Create("SoCiAl", 1);

        // Assert
        bonus.SkillId.Should().Be("social");
        bonus.NormalizedSkillId.Should().Be("social");
    }

    /// <summary>
    /// Verifies that Create throws when skill ID is null.
    /// </summary>
    [Test]
    public void Create_WithNullSkillId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SkillBonus.Create(null!, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("skillId");
    }

    /// <summary>
    /// Verifies that Create throws when skill ID is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceSkillId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SkillBonus.Create("   ", 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("skillId");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies penalty identification for negative bonus amounts.
    /// </summary>
    [Test]
    public void IsPenalty_WithNegativeAmount_ReturnsTrue()
    {
        // Arrange & Act
        var penalty = SkillBonus.Create("stealth", -2);

        // Assert
        penalty.IsPenalty.Should().BeTrue();
        penalty.IsPositive.Should().BeFalse();
        penalty.IsNeutral.Should().BeFalse();
    }

    /// <summary>
    /// Verifies neutral identification for zero bonus amount.
    /// </summary>
    [Test]
    public void IsNeutral_WithZeroAmount_ReturnsTrue()
    {
        // Arrange & Act
        var neutral = SkillBonus.Create("athletics", 0);

        // Assert
        neutral.IsNeutral.Should().BeTrue();
        neutral.IsPositive.Should().BeFalse();
        neutral.IsPenalty.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // APPLIESTO TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that AppliesTo is case-insensitive.
    /// </summary>
    [Test]
    public void AppliesTo_WithMatchingSkillId_IsCaseInsensitive()
    {
        // Arrange
        var bonus = SkillBonus.Create("social", 1);

        // Act & Assert
        bonus.AppliesTo("social").Should().BeTrue();
        bonus.AppliesTo("SOCIAL").Should().BeTrue();
        bonus.AppliesTo("SoCiAl").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that AppliesTo returns false for non-matching skill.
    /// </summary>
    [Test]
    public void AppliesTo_WithNonMatchingSkillId_ReturnsFalse()
    {
        // Arrange
        var bonus = SkillBonus.Create("social", 1);

        // Act & Assert
        bonus.AppliesTo("lore").Should().BeFalse();
        bonus.AppliesTo("craft").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that AppliesTo returns false for null or whitespace.
    /// </summary>
    [Test]
    public void AppliesTo_WithNullOrWhitespace_ReturnsFalse()
    {
        // Arrange
        var bonus = SkillBonus.Create("social", 1);

        // Act & Assert
        bonus.AppliesTo(null).Should().BeFalse();
        bonus.AppliesTo("").Should().BeFalse();
        bonus.AppliesTo("   ").Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies ToString format for positive bonuses.
    /// </summary>
    [Test]
    public void ToString_ForPositiveBonus_ShowsPlusSign()
    {
        // Arrange
        var bonus = SkillBonus.Create("social", 1);

        // Act
        var result = bonus.ToString();

        // Assert
        result.Should().Be("social +1");
    }

    /// <summary>
    /// Verifies ToString format for penalties.
    /// </summary>
    [Test]
    public void ToString_ForPenalty_ShowsMinusSign()
    {
        // Arrange
        var penalty = SkillBonus.Create("stealth", -2);

        // Act
        var result = penalty.ToString();

        // Assert
        result.Should().Be("stealth -2");
    }
}
