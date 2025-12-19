using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the DangerLevel enum.
/// Validates the four threat levels: Safe, Unstable, Hostile, Lethal.
/// </summary>
public class DangerLevelTests
{
    [Fact]
    public void DangerLevel_ShouldHaveExactlyFourValues()
    {
        // Arrange
        var values = Enum.GetValues<DangerLevel>();

        // Assert
        values.Should().HaveCount(4, "Rune & Rust has exactly four danger levels");
    }

    [Fact]
    public void DangerLevel_ShouldContain_Safe()
    {
        // Assert
        Enum.IsDefined(typeof(DangerLevel), DangerLevel.Safe).Should().BeTrue();
    }

    [Fact]
    public void DangerLevel_ShouldContain_Unstable()
    {
        // Assert
        Enum.IsDefined(typeof(DangerLevel), DangerLevel.Unstable).Should().BeTrue();
    }

    [Fact]
    public void DangerLevel_ShouldContain_Hostile()
    {
        // Assert
        Enum.IsDefined(typeof(DangerLevel), DangerLevel.Hostile).Should().BeTrue();
    }

    [Fact]
    public void DangerLevel_ShouldContain_Lethal()
    {
        // Assert
        Enum.IsDefined(typeof(DangerLevel), DangerLevel.Lethal).Should().BeTrue();
    }

    [Fact]
    public void DangerLevel_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)DangerLevel.Safe).Should().Be(0);
        ((int)DangerLevel.Unstable).Should().Be(1);
        ((int)DangerLevel.Hostile).Should().Be(2);
        ((int)DangerLevel.Lethal).Should().Be(3);
    }

    [Theory]
    [InlineData(DangerLevel.Safe, "Safe")]
    [InlineData(DangerLevel.Unstable, "Unstable")]
    [InlineData(DangerLevel.Hostile, "Hostile")]
    [InlineData(DangerLevel.Lethal, "Lethal")]
    public void DangerLevel_ToString_ReturnsExpectedName(DangerLevel dangerLevel, string expectedName)
    {
        // Assert
        dangerLevel.ToString().Should().Be(expectedName);
    }

    [Fact]
    public void DangerLevel_Order_ShouldRepresentIncreasingThreat()
    {
        // Arrange & Act
        var dangerLevels = new[] { DangerLevel.Safe, DangerLevel.Unstable, DangerLevel.Hostile, DangerLevel.Lethal };

        // Assert - each subsequent value should be higher than the previous
        for (int i = 1; i < dangerLevels.Length; i++)
        {
            ((int)dangerLevels[i]).Should().BeGreaterThan((int)dangerLevels[i - 1]);
        }
    }
}
