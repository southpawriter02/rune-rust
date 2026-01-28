// ═══════════════════════════════════════════════════════════════════════════════
// ArmorPenaltiesTests.cs
// Unit tests for the ArmorPenalties value object.
// Version: 0.16.2b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="ArmorPenalties"/>.
/// </summary>
[TestFixture]
public class ArmorPenaltiesTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Static Instances Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ArmorPenalties.None returns zero penalties.
    /// </summary>
    [Test]
    public void None_ReturnsZeroPenalties()
    {
        // Arrange & Act
        var penalties = ArmorPenalties.None;

        // Assert
        penalties.AgilityDicePenalty.Should().Be(0);
        penalties.StaminaCostModifier.Should().Be(0);
        penalties.MovementPenalty.Should().Be(0);
        penalties.HasStealthDisadvantage.Should().BeFalse();
        penalties.HasAnyPenalty.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create with valid parameters constructs correctly.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_ConstructsCorrectly()
    {
        // Arrange & Act
        var penalties = ArmorPenalties.Create(-2, 5, -10, true);

        // Assert
        penalties.AgilityDicePenalty.Should().Be(-2);
        penalties.StaminaCostModifier.Should().Be(5);
        penalties.MovementPenalty.Should().Be(-10);
        penalties.HasStealthDisadvantage.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Create throws when agility penalty is positive.
    /// </summary>
    [Test]
    public void Create_WithPositiveAgilityPenalty_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArmorPenalties.Create(1, 0, 0, false);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws when agility penalty is too low.
    /// </summary>
    [Test]
    public void Create_WithAgilityPenaltyBelowMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArmorPenalties.Create(-11, 0, 0, false);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws when stamina cost is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeStaminaCost_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArmorPenalties.Create(0, -1, 0, false);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws when stamina cost exceeds maximum.
    /// </summary>
    [Test]
    public void Create_WithStaminaCostAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArmorPenalties.Create(0, 21, 0, false);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws when movement penalty is positive.
    /// </summary>
    [Test]
    public void Create_WithPositiveMovementPenalty_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArmorPenalties.Create(0, 0, 1, false);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws when movement penalty is too low.
    /// </summary>
    [Test]
    public void Create_WithMovementPenaltyBelowMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArmorPenalties.Create(0, 0, -31, false);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasAgilityPenalty returns true for negative values.
    /// </summary>
    [Test]
    public void HasAgilityPenalty_WhenNegative_ReturnsTrue()
    {
        // Arrange
        var penalties = ArmorPenalties.Create(-1, 0, 0, false);

        // Act & Assert
        penalties.HasAgilityPenalty.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasAgilityPenalty returns false for zero.
    /// </summary>
    [Test]
    public void HasAgilityPenalty_WhenZero_ReturnsFalse()
    {
        // Arrange
        var penalties = ArmorPenalties.None;

        // Act & Assert
        penalties.HasAgilityPenalty.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that HasStaminaCost returns true for positive values.
    /// </summary>
    [Test]
    public void HasStaminaCost_WhenPositive_ReturnsTrue()
    {
        // Arrange
        var penalties = ArmorPenalties.Create(0, 1, 0, false);

        // Act & Assert
        penalties.HasStaminaCost.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasMovementPenalty returns true for negative values.
    /// </summary>
    [Test]
    public void HasMovementPenalty_WhenNegative_ReturnsTrue()
    {
        // Arrange
        var penalties = ArmorPenalties.Create(0, 0, -5, false);

        // Act & Assert
        penalties.HasMovementPenalty.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasAnyPenalty returns true when any penalty exists.
    /// </summary>
    [Test]
    public void HasAnyPenalty_WhenOnlyStealthDisadvantage_ReturnsTrue()
    {
        // Arrange - only stealth disadvantage, no numerical penalties
        var penalties = ArmorPenalties.Create(0, 0, 0, true);

        // Act & Assert
        penalties.HasAnyPenalty.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Utility Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Multiply doubles numerical penalties but not stealth disadvantage.
    /// </summary>
    [Test]
    public void Multiply_DoublesNumericalPenalties_NotStealthDisadvantage()
    {
        // Arrange
        var basePenalties = ArmorPenalties.Create(-2, 5, -10, true);

        // Act
        var doubled = basePenalties.Multiply(2.0m);

        // Assert
        doubled.AgilityDicePenalty.Should().Be(-4);
        doubled.StaminaCostModifier.Should().Be(10);
        doubled.MovementPenalty.Should().Be(-20);
        doubled.HasStealthDisadvantage.Should().BeTrue(); // Unchanged
    }

    /// <summary>
    /// Verifies that Multiply with 1.0 returns equivalent penalties.
    /// </summary>
    [Test]
    public void Multiply_WithOne_ReturnsSamePenalties()
    {
        // Arrange
        var penalties = ArmorPenalties.Create(-2, 5, -10, false);

        // Act
        var result = penalties.Multiply(1.0m);

        // Assert
        result.AgilityDicePenalty.Should().Be(-2);
        result.StaminaCostModifier.Should().Be(5);
        result.MovementPenalty.Should().Be(-10);
    }

    /// <summary>
    /// Verifies that Multiply with 0.5 halves numerical penalties.
    /// </summary>
    [Test]
    public void Multiply_WithHalf_HalvesPenalties()
    {
        // Arrange
        var penalties = ArmorPenalties.Create(-2, 4, -10, true);

        // Act
        var result = penalties.Multiply(0.5m);

        // Assert
        result.AgilityDicePenalty.Should().Be(-1);
        result.StaminaCostModifier.Should().Be(2);
        result.MovementPenalty.Should().Be(-5);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies FormatAgilityPenalty returns "None" for zero.
    /// </summary>
    [Test]
    public void FormatAgilityPenalty_WhenZero_ReturnsNone()
    {
        // Arrange
        var penalties = ArmorPenalties.None;

        // Act & Assert
        penalties.FormatAgilityPenalty().Should().Be("None");
    }

    /// <summary>
    /// Verifies FormatAgilityPenalty returns dice notation for non-zero.
    /// </summary>
    [Test]
    public void FormatAgilityPenalty_WhenNegative_ReturnsDiceNotation()
    {
        // Arrange
        var penalties = ArmorPenalties.Create(-2, 0, 0, false);

        // Act & Assert
        penalties.FormatAgilityPenalty().Should().Be("-2d10");
    }

    /// <summary>
    /// Verifies FormatStaminaCost returns "None" for zero.
    /// </summary>
    [Test]
    public void FormatStaminaCost_WhenZero_ReturnsNone()
    {
        // Arrange
        var penalties = ArmorPenalties.None;

        // Act & Assert
        penalties.FormatStaminaCost().Should().Be("None");
    }

    /// <summary>
    /// Verifies FormatStaminaCost returns plus notation for positive.
    /// </summary>
    [Test]
    public void FormatStaminaCost_WhenPositive_ReturnsPlusNotation()
    {
        // Arrange
        var penalties = ArmorPenalties.Create(0, 5, 0, false);

        // Act & Assert
        penalties.FormatStaminaCost().Should().Be("+5");
    }

    /// <summary>
    /// Verifies FormatMovementPenalty returns "None" for zero.
    /// </summary>
    [Test]
    public void FormatMovementPenalty_WhenZero_ReturnsNone()
    {
        // Arrange
        var penalties = ArmorPenalties.None;

        // Act & Assert
        penalties.FormatMovementPenalty().Should().Be("None");
    }

    /// <summary>
    /// Verifies FormatMovementPenalty returns feet notation for non-zero.
    /// </summary>
    [Test]
    public void FormatMovementPenalty_WhenNegative_ReturnsFeetNotation()
    {
        // Arrange
        var penalties = ArmorPenalties.Create(0, 0, -10, false);

        // Act & Assert
        penalties.FormatMovementPenalty().Should().Be("-10 ft");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies ToString includes all penalty information.
    /// </summary>
    [Test]
    public void ToString_IncludesAllPenalties()
    {
        // Arrange
        var penalties = ArmorPenalties.Create(-2, 5, -10, true);

        // Act
        var result = penalties.ToString();

        // Assert
        result.Should().Contain("-2d10");
        result.Should().Contain("+5");
        result.Should().Contain("-10 ft");
        result.Should().Contain("Disadvantage");
    }

    /// <summary>
    /// Verifies ToDebugString includes raw values.
    /// </summary>
    [Test]
    public void ToDebugString_IncludesRawValues()
    {
        // Arrange
        var penalties = ArmorPenalties.Create(-2, 5, -10, true);

        // Act
        var result = penalties.ToDebugString();

        // Assert
        result.Should().Contain("ArmorPenalties");
        result.Should().Contain("-2");
        result.Should().Contain("5");
        result.Should().Contain("-10");
        result.Should().Contain("True");
    }
}
