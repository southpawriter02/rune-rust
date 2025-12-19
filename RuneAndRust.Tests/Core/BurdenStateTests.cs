using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the BurdenState enum.
/// Validates the three burden states: Light, Heavy, Overburdened.
/// </summary>
public class BurdenStateTests
{
    [Fact]
    public void BurdenState_ShouldHaveExactlyThreeValues()
    {
        // Arrange
        var values = Enum.GetValues<BurdenState>();

        // Assert
        values.Should().HaveCount(3, "Rune & Rust has exactly three burden states");
    }

    [Fact]
    public void BurdenState_ShouldContain_Light()
    {
        // Assert
        Enum.IsDefined(typeof(BurdenState), BurdenState.Light).Should().BeTrue();
    }

    [Fact]
    public void BurdenState_ShouldContain_Heavy()
    {
        // Assert
        Enum.IsDefined(typeof(BurdenState), BurdenState.Heavy).Should().BeTrue();
    }

    [Fact]
    public void BurdenState_ShouldContain_Overburdened()
    {
        // Assert
        Enum.IsDefined(typeof(BurdenState), BurdenState.Overburdened).Should().BeTrue();
    }

    [Fact]
    public void BurdenState_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)BurdenState.Light).Should().Be(0);
        ((int)BurdenState.Heavy).Should().Be(1);
        ((int)BurdenState.Overburdened).Should().Be(2);
    }

    [Theory]
    [InlineData(BurdenState.Light, "Light")]
    [InlineData(BurdenState.Heavy, "Heavy")]
    [InlineData(BurdenState.Overburdened, "Overburdened")]
    public void BurdenState_ToString_ReturnsExpectedName(BurdenState burdenState, string expectedName)
    {
        // Assert
        burdenState.ToString().Should().Be(expectedName);
    }

    [Fact]
    public void BurdenState_Order_ShouldRepresentIncreasingSeverity()
    {
        // Arrange & Act
        var states = new[] { BurdenState.Light, BurdenState.Heavy, BurdenState.Overburdened };

        // Assert - each subsequent value should be higher than the previous
        for (int i = 1; i < states.Length; i++)
        {
            ((int)states[i]).Should().BeGreaterThan((int)states[i - 1]);
        }
    }
}
