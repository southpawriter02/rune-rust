using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for LevelUpResult value object (v0.0.8b).
/// </summary>
[TestFixture]
public class LevelUpResultTests
{
    [Test]
    public void None_ReturnsResultWithNoChange()
    {
        // Act
        var result = LevelUpResult.None(5);

        // Assert
        result.OldLevel.Should().Be(5);
        result.NewLevel.Should().Be(5);
        result.LevelsGained.Should().Be(0);
        result.DidLevelUp.Should().BeFalse();
        result.IsMultiLevel.Should().BeFalse();
        result.HasNewAbilities.Should().BeFalse();
        result.StatIncreases.Should().Be(LevelStatModifiers.Zero);
    }

    [Test]
    public void LevelsGained_ReturnsCorrectDifference()
    {
        // Arrange
        var result = new LevelUpResult(3, 7, LevelStatModifiers.ForLevels(4));

        // Assert
        result.LevelsGained.Should().Be(4);
    }

    [Test]
    public void IsMultiLevel_MultipleLevels_ReturnsTrue()
    {
        // Arrange
        var result = new LevelUpResult(1, 5, LevelStatModifiers.ForLevels(4));

        // Assert
        result.IsMultiLevel.Should().BeTrue();
    }

    [Test]
    public void IsMultiLevel_SingleLevel_ReturnsFalse()
    {
        // Arrange
        var result = new LevelUpResult(1, 2, LevelStatModifiers.DefaultLevelUp);

        // Assert
        result.IsMultiLevel.Should().BeFalse();
    }

    [Test]
    public void DidLevelUp_WithLevelGain_ReturnsTrue()
    {
        // Arrange
        var result = new LevelUpResult(1, 2, LevelStatModifiers.DefaultLevelUp);

        // Assert
        result.DidLevelUp.Should().BeTrue();
    }

    [Test]
    public void DidLevelUp_NoLevelGain_ReturnsFalse()
    {
        // Arrange
        var result = LevelUpResult.None(5);

        // Assert
        result.DidLevelUp.Should().BeFalse();
    }

    [Test]
    public void HasNewAbilities_WithAbilities_ReturnsTrue()
    {
        // Arrange
        var abilities = new List<string> { "power_strike", "shield_bash" };
        var result = new LevelUpResult(1, 2, LevelStatModifiers.DefaultLevelUp, abilities);

        // Assert
        result.HasNewAbilities.Should().BeTrue();
        result.NewAbilities.Should().HaveCount(2);
        result.NewAbilities.Should().Contain("power_strike");
        result.NewAbilities.Should().Contain("shield_bash");
    }

    [Test]
    public void HasNewAbilities_WithoutAbilities_ReturnsFalse()
    {
        // Arrange
        var result = new LevelUpResult(1, 2, LevelStatModifiers.DefaultLevelUp);

        // Assert
        result.HasNewAbilities.Should().BeFalse();
        result.NewAbilities.Should().BeEmpty();
    }

    [Test]
    public void SingleLevel_CreatesCorrectResult()
    {
        // Act
        var result = LevelUpResult.SingleLevel(5);

        // Assert
        result.OldLevel.Should().Be(5);
        result.NewLevel.Should().Be(6);
        result.LevelsGained.Should().Be(1);
        result.IsMultiLevel.Should().BeFalse();
        result.StatIncreases.Should().Be(LevelStatModifiers.DefaultLevelUp);
    }

    [Test]
    public void SingleLevel_WithAbilities_CreatesCorrectResult()
    {
        // Arrange
        var abilities = new List<string> { "power_strike" };

        // Act
        var result = LevelUpResult.SingleLevel(2, abilities);

        // Assert
        result.OldLevel.Should().Be(2);
        result.NewLevel.Should().Be(3);
        result.NewAbilities.Should().Contain("power_strike");
    }

    [Test]
    public void MultiLevel_CreatesCorrectResult()
    {
        // Act
        var result = LevelUpResult.MultiLevel(1, 4);

        // Assert
        result.OldLevel.Should().Be(1);
        result.NewLevel.Should().Be(5);
        result.LevelsGained.Should().Be(4);
        result.IsMultiLevel.Should().BeTrue();
        result.StatIncreases.MaxHealth.Should().Be(20); // 5 * 4
        result.StatIncreases.Attack.Should().Be(4);     // 1 * 4
        result.StatIncreases.Defense.Should().Be(4);    // 1 * 4
    }

    [Test]
    public void ToString_LevelUp_ReturnsFormattedString()
    {
        // Arrange
        var result = new LevelUpResult(1, 3, LevelStatModifiers.ForLevels(2));

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("Level 1 -> 3");
    }

    [Test]
    public void ToString_NoChange_ReturnsNoChangeMessage()
    {
        // Arrange
        var result = LevelUpResult.None(5);

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("Level 5");
        str.Should().Contain("no change");
    }
}
