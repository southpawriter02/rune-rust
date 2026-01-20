using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class SkillCheckResultTests
{
    [Test]
    public void Constructor_WhenTotalMeetsDC_ReturnsSuccess()
    {
        // Arrange
        var dicePool = new DicePool(1, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 7 }, 7);

        // Act
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 3,
            otherBonus: 2,
            difficultyClass: 12,
            difficultyName: "Moderate");

        // Assert
        result.TotalResult.Should().Be(12); // 7 + 3 + 2
        result.SuccessLevel.Should().Be(SuccessLevel.Success);
        result.IsSuccess.Should().BeTrue();
        result.Margin.Should().Be(0);
    }

    [Test]
    public void Constructor_WhenNaturalMax_ReturnsCriticalSuccess()
    {
        // Arrange
        var dicePool = new DicePool(1, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 10 }, 10);

        // Act
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 20,
            difficultyName: "Hard");

        // Assert
        result.SuccessLevel.Should().Be(SuccessLevel.CriticalSuccess);
        result.IsCriticalSuccess.Should().BeTrue();
    }

    [Test]
    public void Constructor_WhenNaturalOne_ReturnsCriticalFailure()
    {
        // Arrange
        var dicePool = new DicePool(1, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 1 }, 1);

        // Act
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 10,
            otherBonus: 5,
            difficultyClass: 5,
            difficultyName: "Trivial");

        // Assert
        result.TotalResult.Should().Be(16);
        result.SuccessLevel.Should().Be(SuccessLevel.CriticalFailure);
        result.IsCriticalFailure.Should().BeTrue();
    }

    [Test]
    public void Margin_CalculatesCorrectly()
    {
        // Arrange
        var dicePool = new DicePool(1, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 5 }, 5);

        // Act
        var result = new SkillCheckResult(
            "stealth", "Stealth",
            diceResult,
            attributeBonus: 3,
            otherBonus: 0,
            difficultyClass: 12,
            difficultyName: "Moderate");

        // Assert
        result.TotalResult.Should().Be(8);
        result.Margin.Should().Be(-4);
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var dicePool = new DicePool(1, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 8 }, 8);
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 3,
            otherBonus: 0,
            difficultyClass: 12,
            difficultyName: "Moderate");

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("Perception");
        str.Should().Contain("DC 12");
        str.Should().Contain("Moderate");
    }
}
