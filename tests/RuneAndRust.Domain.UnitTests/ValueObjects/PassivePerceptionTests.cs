using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="PassivePerception"/> value object.
/// </summary>
/// <remarks>
/// Tests verify the WITS ÷ 2 (round up) formula and modifier application.
/// </remarks>
[TestFixture]
public class PassivePerceptionTests
{
    #region Calculate Formula Tests

    [Test]
    public void Calculate_WitsValueSix_ReturnsPassiveThree()
    {
        // Arrange & Act
        // WITS 6 ÷ 2 = 3.0 → 3
        var result = PassivePerception.Calculate("char-1", 6);

        // Assert
        result.PassiveValue.Should().Be(3);
        result.WitsAttribute.Should().Be(6);
        result.CharacterId.Should().Be("char-1");
    }

    [Test]
    public void Calculate_WitsValueSeven_ReturnsPassiveFour()
    {
        // Arrange & Act
        // WITS 7 ÷ 2 = 3.5 → round up → 4
        var result = PassivePerception.Calculate("char-1", 7);

        // Assert
        result.PassiveValue.Should().Be(4);
    }

    [Test]
    public void Calculate_WitsValueTen_ReturnsPassiveFive()
    {
        // Arrange & Act
        // WITS 10 ÷ 2 = 5.0 → 5
        var result = PassivePerception.Calculate("char-1", 10);

        // Assert
        result.PassiveValue.Should().Be(5);
    }

    [Test]
    public void EffectiveValue_WithModifiers_AppliesCorrectly()
    {
        // Arrange
        var modifiers = new List<PerceptionModifier>
        {
            PerceptionModifier.Alert(),      // +2
            PerceptionModifier.DimLighting() // -1
        };

        // Act
        var result = PassivePerception.Calculate("char-1", 8, modifiers);

        // Assert
        // Base: 8 ÷ 2 = 4, Modifiers: +2 + -1 = +1, Effective: 4 + 1 = 5
        result.PassiveValue.Should().Be(4);
        result.EffectiveValue.Should().Be(5);
        result.TotalModifier.Should().Be(1);
        result.HasModifiers.Should().BeTrue();
        result.IsModified.Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Test]
    public void Calculate_WitsValueOne_ReturnsPassiveOne()
    {
        // Arrange & Act
        // WITS 1 ÷ 2 = 0.5 → round up → 1
        var result = PassivePerception.Calculate("char-1", 1);

        // Assert
        result.PassiveValue.Should().Be(1);
    }

    [Test]
    public void Calculate_WitsValueTwo_ReturnsPassiveOne()
    {
        // Arrange & Act
        // WITS 2 ÷ 2 = 1.0 → 1
        var result = PassivePerception.Calculate("char-1", 2);

        // Assert
        result.PassiveValue.Should().Be(1);
    }

    [Test]
    public void EffectiveValue_WithLargeNegativeModifiers_ClampsToZero()
    {
        // Arrange
        var modifiers = new List<PerceptionModifier>
        {
            PerceptionModifier.TotalDarkness(), // -3
            PerceptionModifier.Exhausted()      // -2
        };

        // Act
        var result = PassivePerception.Calculate("char-1", 4, modifiers);

        // Assert
        // Base: 4 ÷ 2 = 2, Modifiers: -3 + -2 = -5, Effective: max(0, 2 - 5) = 0
        result.PassiveValue.Should().Be(2);
        result.EffectiveValue.Should().Be(0);
    }

    [Test]
    public void Calculate_NoModifiers_EffectiveEqualsPassive()
    {
        // Arrange & Act
        var result = PassivePerception.Calculate("char-1", 12);

        // Assert
        result.PassiveValue.Should().Be(6);
        result.EffectiveValue.Should().Be(6);
        result.HasModifiers.Should().BeFalse();
        result.IsModified.Should().BeFalse();
    }

    #endregion

    #region Validation Tests

    [Test]
    public void Calculate_NullCharacterId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PassivePerception.Calculate(null!, 10);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Calculate_EmptyCharacterId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PassivePerception.Calculate("", 10);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Calculate_WitsValueZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => PassivePerception.Calculate("char-1", 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Calculate_WitsValueOver30_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => PassivePerception.Calculate("char-1", 31);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Display String Tests

    [Test]
    public void ToDisplayString_NoModifiers_ShowsBasicFormat()
    {
        // Arrange
        var result = PassivePerception.Calculate("char-1", 8);

        // Act
        var display = result.ToDisplayString();

        // Assert
        display.Should().Contain("Passive Perception: 4");
        display.Should().Contain("WITS 8");
    }

    [Test]
    public void ToDisplayString_WithModifiers_ShowsBreakdown()
    {
        // Arrange
        var modifiers = new List<PerceptionModifier>
        {
            PerceptionModifier.Alert()
        };
        var result = PassivePerception.Calculate("char-1", 8, modifiers);

        // Act
        var display = result.ToDisplayString();

        // Assert
        display.Should().Contain("Passive Perception: 6");
        display.Should().Contain("+2");
    }

    #endregion
}
