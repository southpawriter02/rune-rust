using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the BiomeType enum.
/// Validates the four environmental atmospheres: Ruin, Industrial, Organic, Void.
/// </summary>
public class BiomeTypeTests
{
    [Fact]
    public void BiomeType_ShouldHaveExactlyFourValues()
    {
        // Arrange
        var values = Enum.GetValues<BiomeType>();

        // Assert
        values.Should().HaveCount(4, "Rune & Rust has exactly four biome types");
    }

    [Fact]
    public void BiomeType_ShouldContain_Ruin()
    {
        // Assert
        Enum.IsDefined(typeof(BiomeType), BiomeType.Ruin).Should().BeTrue();
    }

    [Fact]
    public void BiomeType_ShouldContain_Industrial()
    {
        // Assert
        Enum.IsDefined(typeof(BiomeType), BiomeType.Industrial).Should().BeTrue();
    }

    [Fact]
    public void BiomeType_ShouldContain_Organic()
    {
        // Assert
        Enum.IsDefined(typeof(BiomeType), BiomeType.Organic).Should().BeTrue();
    }

    [Fact]
    public void BiomeType_ShouldContain_Void()
    {
        // Assert
        Enum.IsDefined(typeof(BiomeType), BiomeType.Void).Should().BeTrue();
    }

    [Fact]
    public void BiomeType_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)BiomeType.Ruin).Should().Be(0);
        ((int)BiomeType.Industrial).Should().Be(1);
        ((int)BiomeType.Organic).Should().Be(2);
        ((int)BiomeType.Void).Should().Be(3);
    }

    [Theory]
    [InlineData(BiomeType.Ruin, "Ruin")]
    [InlineData(BiomeType.Industrial, "Industrial")]
    [InlineData(BiomeType.Organic, "Organic")]
    [InlineData(BiomeType.Void, "Void")]
    public void BiomeType_ToString_ReturnsExpectedName(BiomeType biomeType, string expectedName)
    {
        // Assert
        biomeType.ToString().Should().Be(expectedName);
    }
}
