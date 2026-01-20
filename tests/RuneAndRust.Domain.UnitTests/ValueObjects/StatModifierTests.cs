using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class StatModifierTests
{
    [Test]
    public void Flat_ShouldCreateFlatModifier()
    {
        // Act
        var modifier = StatModifier.Flat("attack", 5);

        // Assert
        modifier.StatId.Should().Be("attack");
        modifier.ModifierType.Should().Be(StatModifierType.Flat);
        modifier.Value.Should().Be(5);
    }

    [Test]
    public void Percentage_ShouldCreatePercentageModifier()
    {
        // Act
        var modifier = StatModifier.Percentage("defense", 0.25f);

        // Assert
        modifier.StatId.Should().Be("defense");
        modifier.ModifierType.Should().Be(StatModifierType.Percentage);
        modifier.Value.Should().Be(0.25f);
    }

    [Test]
    public void Apply_FlatModifier_ShouldAddValue()
    {
        // Arrange
        var modifier = StatModifier.Flat("attack", 5);

        // Act
        var result = modifier.Apply(10);

        // Assert
        result.Should().Be(15);
    }

    [Test]
    public void Apply_FlatModifier_NegativeValue_ShouldSubtract()
    {
        // Arrange
        var modifier = StatModifier.Flat("attack", -3);

        // Act
        var result = modifier.Apply(10);

        // Assert
        result.Should().Be(7);
    }

    [Test]
    public void Apply_PercentageModifier_Positive_ShouldIncrease()
    {
        // Arrange
        var modifier = StatModifier.Percentage("attack", 0.5f);

        // Act
        var result = modifier.Apply(10);

        // Assert
        result.Should().Be(15); // 10 * 1.5 = 15
    }

    [Test]
    public void Apply_PercentageModifier_Negative_ShouldDecrease()
    {
        // Arrange
        var modifier = StatModifier.Percentage("attack", -0.5f);

        // Act
        var result = modifier.Apply(10);

        // Assert
        result.Should().Be(5); // 10 * 0.5 = 5
    }

    [Test]
    public void Apply_OverrideModifier_ShouldSetValue()
    {
        // Arrange
        var modifier = StatModifier.Override("attack", 20);

        // Act
        var result = modifier.Apply(10);

        // Assert
        result.Should().Be(20);
    }

    [Test]
    public void ToString_FlatPositive_ShouldFormatCorrectly()
    {
        // Arrange
        var modifier = StatModifier.Flat("attack", 5);

        // Act
        var result = modifier.ToString();

        // Assert
        result.Should().Be("attack +5");
    }

    [Test]
    public void ToString_PercentageNegative_ShouldFormatCorrectly()
    {
        // Arrange
        var modifier = StatModifier.Percentage("defense", -0.25f);

        // Act
        var result = modifier.ToString();

        // Assert
        result.Should().Be("defense -25%");
    }
}
