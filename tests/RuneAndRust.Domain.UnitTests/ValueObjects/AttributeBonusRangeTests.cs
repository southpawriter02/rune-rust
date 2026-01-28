using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="AttributeBonusRange"/> value object.
/// </summary>
[TestFixture]
public class AttributeBonusRangeTests
{
    /// <summary>
    /// Verifies that Create returns a valid range with correct properties.
    /// </summary>
    [Test]
    public void Create_WithValidRange_CreatesRange()
    {
        // Act
        var range = AttributeBonusRange.Create(1, 3);

        // Assert
        range.MinBonus.Should().Be(1);
        range.MaxBonus.Should().Be(3);
        range.IsFixed.Should().BeFalse();
        range.RangeSpan.Should().Be(3);
    }

    /// <summary>
    /// Verifies that Create throws when min is greater than max.
    /// </summary>
    [Test]
    public void Create_WithMinGreaterThanMax_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => AttributeBonusRange.Create(5, 2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage("*MinBonus (5) cannot be greater than MaxBonus (2)*");
    }

    /// <summary>
    /// Verifies that Create throws for negative min value.
    /// </summary>
    [Test]
    public void Create_WithNegativeMin_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => AttributeBonusRange.Create(-1, 2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Fixed creates a range with equal min and max.
    /// </summary>
    [Test]
    public void Fixed_CreatesRangeWithEqualMinMax()
    {
        // Act
        var range = AttributeBonusRange.Fixed(2);

        // Assert
        range.MinBonus.Should().Be(2);
        range.MaxBonus.Should().Be(2);
        range.IsFixed.Should().BeTrue();
        range.RangeSpan.Should().Be(1);
    }

    /// <summary>
    /// Verifies that Fixed throws for negative value.
    /// </summary>
    [Test]
    public void Fixed_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => AttributeBonusRange.Fixed(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that RollBonus with a fixed range returns the same value.
    /// </summary>
    [Test]
    public void RollBonus_WithFixedRange_ReturnsSameValue()
    {
        // Arrange
        var range = AttributeBonusRange.Fixed(3);
        var random = new Random(42);

        // Act & Assert - Roll multiple times
        for (var i = 0; i < 10; i++)
        {
            range.RollBonus(random).Should().Be(3);
        }
    }

    /// <summary>
    /// Verifies that RollBonus returns values within the defined bounds.
    /// </summary>
    [Test]
    public void RollBonus_WithRange_ReturnsValueWithinBounds()
    {
        // Arrange
        var range = AttributeBonusRange.Create(2, 4);
        var random = new Random(42);

        // Act & Assert - Roll multiple times and verify bounds
        for (var i = 0; i < 100; i++)
        {
            var result = range.RollBonus(random);
            result.Should().BeGreaterThanOrEqualTo(2);
            result.Should().BeLessThanOrEqualTo(4);
        }
    }

    /// <summary>
    /// Verifies that RollBonus throws for null random.
    /// </summary>
    [Test]
    public void RollBonus_WithNullRandom_ThrowsArgumentNullException()
    {
        // Arrange
        var range = AttributeBonusRange.Create(1, 3);

        // Act
        var act = () => range.RollBonus(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that IsFixed returns true when min equals max.
    /// </summary>
    [Test]
    public void IsFixed_WhenMinEqualsMax_ReturnsTrue()
    {
        // Arrange
        var range = AttributeBonusRange.Create(2, 2);

        // Assert
        range.IsFixed.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that default tier ranges match specification.
    /// </summary>
    [Test]
    public void DefaultRanges_MatchSpecification()
    {
        // Tier 2: Fixed +1
        AttributeBonusRange.Tier2Default.MinBonus.Should().Be(1);
        AttributeBonusRange.Tier2Default.MaxBonus.Should().Be(1);
        AttributeBonusRange.Tier2Default.IsFixed.Should().BeTrue();

        // Tier 3: +1 to +2
        AttributeBonusRange.Tier3Default.MinBonus.Should().Be(1);
        AttributeBonusRange.Tier3Default.MaxBonus.Should().Be(2);
        AttributeBonusRange.Tier3Default.IsFixed.Should().BeFalse();

        // Tier 4: +2 to +4
        AttributeBonusRange.Tier4Default.MinBonus.Should().Be(2);
        AttributeBonusRange.Tier4Default.MaxBonus.Should().Be(4);
        AttributeBonusRange.Tier4Default.IsFixed.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ToString returns correct format for fixed range.
    /// </summary>
    [Test]
    public void ToString_WithFixedRange_ReturnsCorrectFormat()
    {
        // Arrange
        var range = AttributeBonusRange.Fixed(2);

        // Act
        var result = range.ToString();

        // Assert
        result.Should().Be("+2");
    }

    /// <summary>
    /// Verifies that ToString returns correct format for variable range.
    /// </summary>
    [Test]
    public void ToString_WithVariableRange_ReturnsCorrectFormat()
    {
        // Arrange
        var range = AttributeBonusRange.Create(1, 3);

        // Act
        var result = range.ToString();

        // Assert
        result.Should().Be("+1 to +3");
    }
}
