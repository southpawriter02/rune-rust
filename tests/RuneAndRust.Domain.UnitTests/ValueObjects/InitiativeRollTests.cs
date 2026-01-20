using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for InitiativeRoll value object.
/// </summary>
[TestFixture]
public class InitiativeRollTests
{
    [Test]
    public void Total_ShouldSumDiceResultAndModifier()
    {
        // Arrange
        var pool = DicePool.D10();
        var diceResult = new DiceRollResult(pool, [7], 7);

        // Act
        var initiative = new InitiativeRoll(diceResult, 3);

        // Assert
        initiative.Total.Should().Be(10); // 7 + 3
        initiative.RollValue.Should().Be(7);
        initiative.Modifier.Should().Be(3);
    }

    [Test]
    public void Total_WithNegativeModifier_ShouldCalculateCorrectly()
    {
        // Arrange
        var pool = DicePool.D10();
        var diceResult = new DiceRollResult(pool, [5], 5);

        // Act
        var initiative = new InitiativeRoll(diceResult, -2);

        // Assert
        initiative.Total.Should().Be(3); // 5 - 2
    }

    [Test]
    public void ToDisplayString_WithPositiveModifier_ShouldFormatCorrectly()
    {
        // Arrange
        var pool = DicePool.D10();
        var diceResult = new DiceRollResult(pool, [8], 8);
        var initiative = new InitiativeRoll(diceResult, 4);

        // Act
        var display = initiative.ToDisplayString();

        // Assert
        display.Should().Be("[8] +4 = 12");
    }

    [Test]
    public void ToDisplayString_WithNegativeModifier_ShouldFormatCorrectly()
    {
        // Arrange
        var pool = DicePool.D10();
        var diceResult = new DiceRollResult(pool, [6], 6);
        var initiative = new InitiativeRoll(diceResult, -1);

        // Act
        var display = initiative.ToDisplayString();

        // Assert
        display.Should().Be("[6] -1 = 5");
    }

    [Test]
    public void IsNaturalMax_WhenDiceResultIsMax_ShouldReturnTrue()
    {
        // Arrange
        var pool = DicePool.D10();
        var diceResult = new DiceRollResult(pool, [10], 10); // 10 is max on d10

        // Act
        var initiative = new InitiativeRoll(diceResult, 0);

        // Assert
        initiative.IsNaturalMax.Should().BeTrue();
        initiative.IsNaturalOne.Should().BeFalse();
    }

    [Test]
    public void IsNaturalOne_WhenDiceResultIsOne_ShouldReturnTrue()
    {
        // Arrange
        var pool = DicePool.D10();
        var diceResult = new DiceRollResult(pool, [1], 1);

        // Act
        var initiative = new InitiativeRoll(diceResult, 0);

        // Assert
        initiative.IsNaturalOne.Should().BeTrue();
        initiative.IsNaturalMax.Should().BeFalse();
    }
}
