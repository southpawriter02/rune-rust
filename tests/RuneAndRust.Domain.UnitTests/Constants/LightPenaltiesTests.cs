using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Constants;

/// <summary>
/// Tests for the LightPenalties constants class.
/// </summary>
[TestFixture]
public class LightPenaltiesTests
{
    #region Accuracy Penalty Tests

    [Test]
    public void GetAccuracyPenalty_Bright_ReturnsZero()
    {
        // Act
        var penalty = LightPenalties.GetAccuracyPenalty(LightLevel.Bright);

        // Assert
        penalty.Should().Be(0);
    }

    [Test]
    public void GetAccuracyPenalty_Dim_ReturnsNegativeOne()
    {
        // Act
        var penalty = LightPenalties.GetAccuracyPenalty(LightLevel.Dim);

        // Assert
        penalty.Should().Be(-1);
    }

    [Test]
    public void GetAccuracyPenalty_Dark_ReturnsNegativeThree()
    {
        // Act
        var penalty = LightPenalties.GetAccuracyPenalty(LightLevel.Dark);

        // Assert
        penalty.Should().Be(-3);
    }

    [Test]
    public void GetAccuracyPenalty_MagicalDarkness_ReturnsNegativeFive()
    {
        // Act
        var penalty = LightPenalties.GetAccuracyPenalty(LightLevel.MagicalDarkness);

        // Assert
        penalty.Should().Be(-5);
    }

    #endregion

    #region Perception DC Modifier Tests

    [Test]
    public void GetPerceptionDCModifier_Bright_ReturnsZero()
    {
        // Act
        var modifier = LightPenalties.GetPerceptionDCModifier(LightLevel.Bright);

        // Assert
        modifier.Should().Be(0);
    }

    [Test]
    public void GetPerceptionDCModifier_Dim_ReturnsTwo()
    {
        // Act
        var modifier = LightPenalties.GetPerceptionDCModifier(LightLevel.Dim);

        // Assert
        modifier.Should().Be(2);
    }

    [Test]
    public void GetPerceptionDCModifier_Dark_ReturnsFive()
    {
        // Act
        var modifier = LightPenalties.GetPerceptionDCModifier(LightLevel.Dark);

        // Assert
        modifier.Should().Be(5);
    }

    [Test]
    public void GetPerceptionDCModifier_MagicalDarkness_ReturnsTen()
    {
        // Act
        var modifier = LightPenalties.GetPerceptionDCModifier(LightLevel.MagicalDarkness);

        // Assert
        modifier.Should().Be(10);
    }

    #endregion

    #region Helper Method Tests

    [Test]
    public void HasPenalties_Bright_ReturnsFalse()
    {
        // Act & Assert
        LightPenalties.HasPenalties(LightLevel.Bright).Should().BeFalse();
    }

    [Test]
    public void HasPenalties_Dim_ReturnsTrue()
    {
        // Act & Assert
        LightPenalties.HasPenalties(LightLevel.Dim).Should().BeTrue();
    }

    [Test]
    public void GetDescription_Dark_ReturnsExpectedText()
    {
        // Act
        var desc = LightPenalties.GetDescription(LightLevel.Dark);

        // Assert
        desc.Should().Contain("darkness");
    }

    [Test]
    public void GetLabel_MagicalDarkness_ReturnsExpectedLabel()
    {
        // Act
        var label = LightPenalties.GetLabel(LightLevel.MagicalDarkness);

        // Assert
        label.Should().Be("Magical Darkness");
    }

    #endregion
}
