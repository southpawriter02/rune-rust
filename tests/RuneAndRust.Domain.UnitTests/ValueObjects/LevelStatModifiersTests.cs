using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for LevelStatModifiers value object (v0.0.8b).
/// </summary>
[TestFixture]
public class LevelStatModifiersTests
{
    [Test]
    public void Zero_HasNoModifications()
    {
        // Arrange & Act
        var modifiers = LevelStatModifiers.Zero;

        // Assert
        modifiers.MaxHealth.Should().Be(0);
        modifiers.Attack.Should().Be(0);
        modifiers.Defense.Should().Be(0);
        modifiers.HasModifications.Should().BeFalse();
    }

    [Test]
    public void DefaultLevelUp_HasCorrectValues()
    {
        // Arrange & Act
        var modifiers = LevelStatModifiers.DefaultLevelUp;

        // Assert
        modifiers.MaxHealth.Should().Be(5);
        modifiers.Attack.Should().Be(1);
        modifiers.Defense.Should().Be(1);
        modifiers.HasModifications.Should().BeTrue();
    }

    [Test]
    public void Multiply_ScalesAllValues()
    {
        // Arrange
        var modifiers = LevelStatModifiers.DefaultLevelUp;

        // Act
        var scaled = modifiers.Multiply(3);

        // Assert
        scaled.MaxHealth.Should().Be(15);
        scaled.Attack.Should().Be(3);
        scaled.Defense.Should().Be(3);
    }

    [Test]
    public void Multiply_ByZero_ReturnsZeroModifiers()
    {
        // Arrange
        var modifiers = LevelStatModifiers.DefaultLevelUp;

        // Act
        var scaled = modifiers.Multiply(0);

        // Assert
        scaled.MaxHealth.Should().Be(0);
        scaled.Attack.Should().Be(0);
        scaled.Defense.Should().Be(0);
        scaled.HasModifications.Should().BeFalse();
    }

    [Test]
    public void Add_CombinesModifiers()
    {
        // Arrange
        var mod1 = new LevelStatModifiers(5, 1, 1);
        var mod2 = new LevelStatModifiers(10, 2, 3);

        // Act
        var combined = mod1.Add(mod2);

        // Assert
        combined.MaxHealth.Should().Be(15);
        combined.Attack.Should().Be(3);
        combined.Defense.Should().Be(4);
    }

    [Test]
    public void ForLevels_ZeroLevels_ReturnsZero()
    {
        // Act
        var modifiers = LevelStatModifiers.ForLevels(0);

        // Assert
        modifiers.Should().Be(LevelStatModifiers.Zero);
    }

    [Test]
    public void ForLevels_NegativeLevels_ReturnsZero()
    {
        // Act
        var modifiers = LevelStatModifiers.ForLevels(-5);

        // Assert
        modifiers.Should().Be(LevelStatModifiers.Zero);
    }

    [Test]
    public void ForLevels_SingleLevel_ReturnsDefaultLevelUp()
    {
        // Act
        var modifiers = LevelStatModifiers.ForLevels(1);

        // Assert
        modifiers.Should().Be(LevelStatModifiers.DefaultLevelUp);
    }

    [Test]
    public void ForLevels_MultipleLevels_ReturnsScaledModifiers()
    {
        // Act
        var modifiers = LevelStatModifiers.ForLevels(4);

        // Assert
        modifiers.MaxHealth.Should().Be(20); // 5 * 4
        modifiers.Attack.Should().Be(4);     // 1 * 4
        modifiers.Defense.Should().Be(4);    // 1 * 4
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var modifiers = new LevelStatModifiers(10, 2, 3);

        // Act
        var result = modifiers.ToString();

        // Assert
        result.Should().Contain("HP: +10");
        result.Should().Contain("ATK: +2");
        result.Should().Contain("DEF: +3");
    }
}
