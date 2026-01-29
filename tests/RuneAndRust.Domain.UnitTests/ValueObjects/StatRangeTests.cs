using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="StatRange"/> value object.
/// </summary>
/// <remarks>
/// Tests cover dice expression parsing, range validation, and factory methods.
/// </remarks>
[TestFixture]
public class StatRangeTests
{
    #region CreateFromDice Tests

    /// <summary>
    /// Tests that CreateFromDice correctly calculates min/max for dice expression.
    /// </summary>
    [Test]
    public void CreateFromDice_WithDiceExpression_CalculatesCorrectRange()
    {
        // Arrange & Act - 2d6+4 = min 6, max 16
        var range = StatRange.CreateFromDice(diceCount: 2, diceSides: 6, bonus: 4);

        // Assert
        range.MinValue.Should().Be(6, "minimum should be diceCount + bonus = 2 + 4");
        range.MaxValue.Should().Be(16, "maximum should be diceCount * diceSides + bonus = 12 + 4");
        range.DiceExpression.Should().Be("2d6+4");
        range.GetAverageValue().Should().Be(11.0, "average should be (min + max) / 2");
        range.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CreateFromDice handles no bonus correctly.
    /// </summary>
    [Test]
    public void CreateFromDice_WithNoBonus_OmitsBonusInExpression()
    {
        // Arrange & Act - 1d6 = min 1, max 6
        var range = StatRange.CreateFromDice(diceCount: 1, diceSides: 6, bonus: 0);

        // Assert
        range.MinValue.Should().Be(1);
        range.MaxValue.Should().Be(6);
        range.DiceExpression.Should().Be("1d6", "expression should not include +0");
    }

    #endregion

    #region IsInRange Tests

    /// <summary>
    /// Tests that IsInRange returns true for values within range boundaries.
    /// </summary>
    [Test]
    public void IsInRange_WithValueInRange_ReturnsTrue()
    {
        // Arrange
        var range = StatRange.Create(1, 6, "1d6");

        // Assert - test boundaries and middle
        range.IsInRange(1).Should().BeTrue("minimum value should be in range");
        range.IsInRange(3).Should().BeTrue("middle value should be in range");
        range.IsInRange(6).Should().BeTrue("maximum value should be in range");
    }

    /// <summary>
    /// Tests that IsInRange returns false for values outside range.
    /// </summary>
    [Test]
    public void IsInRange_WithValueOutOfRange_ReturnsFalse()
    {
        // Arrange
        var range = StatRange.Create(1, 6, "1d6");

        // Assert
        range.IsInRange(0).Should().BeFalse("value below minimum should be out of range");
        range.IsInRange(7).Should().BeFalse("value above maximum should be out of range");
    }

    #endregion

    #region Factory Method Tests

    /// <summary>
    /// Tests that CreateFlat creates correct min-max expression.
    /// </summary>
    [Test]
    public void CreateFlat_WithMinMax_CreatesCorrectRange()
    {
        // Arrange & Act
        var range = StatRange.CreateFlat(7, 10);

        // Assert
        range.MinValue.Should().Be(7);
        range.MaxValue.Should().Be(10);
        range.DiceExpression.Should().Be("7-10");
        range.Span.Should().Be(3);
    }

    /// <summary>
    /// Tests that CreateFixed creates single-value range.
    /// </summary>
    [Test]
    public void CreateFixed_WithValue_CreatesSingleValueRange()
    {
        // Arrange & Act
        var range = StatRange.CreateFixed(4);

        // Assert
        range.MinValue.Should().Be(4);
        range.MaxValue.Should().Be(4);
        range.DiceExpression.Should().Be("+4");
        range.Span.Should().Be(0, "fixed value should have span of 0");
    }

    #endregion

    #region FormatRange Tests

    /// <summary>
    /// Tests that FormatRange produces correct display string.
    /// </summary>
    [Test]
    public void FormatRange_WithDiceRange_ReturnsFormattedString()
    {
        // Arrange
        var range = StatRange.CreateFromDice(2, 6, 4);

        // Act
        var formatted = range.FormatRange();

        // Assert
        formatted.Should().Be("6-16 (2d6+4)");
    }

    #endregion
}

/// <summary>
/// Unit tests for <see cref="StatViolation"/> value object.
/// </summary>
[TestFixture]
public class StatViolationTests
{
    /// <summary>
    /// Tests that Damage factory creates correct violation.
    /// </summary>
    [Test]
    public void Damage_WithExpectedAndActual_CreatesCorrectViolation()
    {
        // Arrange
        var expected = StatRange.Create(1, 6, "1d6");

        // Act
        var violation = StatViolation.Damage(expected, actual: 15);

        // Assert
        violation.StatType.Should().Be(StatViolationType.Damage);
        violation.StatName.Should().Be("Damage");
        violation.ExpectedRange.Should().Be(expected);
        violation.ActualValue.Should().Be(15);
        violation.Message.Should().Be("Damage value 15 outside expected range 1-6 (1d6)");
        violation.Deviation.Should().Be(9, "15 - 6 = 9 above max");
    }

    /// <summary>
    /// Tests that Attribute factory creates correct violation with attribute name.
    /// </summary>
    [Test]
    public void Attribute_WithAttributeName_CreatesCorrectViolation()
    {
        // Arrange
        var expected = StatRange.CreateFixed(4);

        // Act
        var violation = StatViolation.Attribute("Might", expected, actual: 7);

        // Assert
        violation.StatType.Should().Be(StatViolationType.Attribute);
        violation.StatName.Should().Be("Might");
        violation.Message.Should().Contain("Might value 7");
    }
}

/// <summary>
/// Unit tests for <see cref="StatVerificationResult"/> value object.
/// </summary>
[TestFixture]
public class StatVerificationResultTests
{
    /// <summary>
    /// Tests that Valid factory creates correct result.
    /// </summary>
    [Test]
    public void Valid_WithItemIdAndTier_CreatesValidResult()
    {
        // Arrange & Act
        var result = StatVerificationResult.Valid("sword-001", QualityTier.MythForged);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Violations.Should().BeEmpty();
        result.ItemId.Should().Be("sword-001");
        result.QualityTier.Should().Be(QualityTier.MythForged);
        result.ViolationCount.Should().Be(0);
        result.GetViolationSummary().Should().Be("All stats valid");
    }

    /// <summary>
    /// Tests that Invalid factory creates correct result with violations.
    /// </summary>
    [Test]
    public void Invalid_WithViolations_CreatesInvalidResult()
    {
        // Arrange
        var violations = new[]
        {
            StatViolation.Damage(StatRange.Create(1, 6, "1d6"), 15),
            StatViolation.Attribute("Might", StatRange.CreateFixed(4), 7)
        };

        // Act
        var result = StatVerificationResult.Invalid("sword-002", QualityTier.Scavenged, violations);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Violations.Should().HaveCount(2);
        result.ViolationCount.Should().Be(2);
        result.HasDamageViolation.Should().BeTrue();
        result.HasAttributeViolation.Should().BeTrue();
        result.HasDefenseViolation.Should().BeFalse();
        result.GetViolationSummary().Should().Contain(";", "multiple violations should be semicolon-separated");
    }
}
