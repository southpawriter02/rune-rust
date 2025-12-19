using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the LineageType enum.
/// Validates the four character lineages: Human, RuneMarked, IronBlooded, VargrKin.
/// </summary>
public class LineageTypeTests
{
    [Fact]
    public void LineageType_ShouldHaveExactlyFourValues()
    {
        // Arrange
        var values = Enum.GetValues<LineageType>();

        // Assert
        values.Should().HaveCount(4, "Rune & Rust has exactly four lineages");
    }

    [Fact]
    public void LineageType_ShouldContain_Human()
    {
        // Assert
        Enum.IsDefined(typeof(LineageType), LineageType.Human).Should().BeTrue();
    }

    [Fact]
    public void LineageType_ShouldContain_RuneMarked()
    {
        // Assert
        Enum.IsDefined(typeof(LineageType), LineageType.RuneMarked).Should().BeTrue();
    }

    [Fact]
    public void LineageType_ShouldContain_IronBlooded()
    {
        // Assert
        Enum.IsDefined(typeof(LineageType), LineageType.IronBlooded).Should().BeTrue();
    }

    [Fact]
    public void LineageType_ShouldContain_VargrKin()
    {
        // Assert
        Enum.IsDefined(typeof(LineageType), LineageType.VargrKin).Should().BeTrue();
    }

    [Fact]
    public void LineageType_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)LineageType.Human).Should().Be(0);
        ((int)LineageType.RuneMarked).Should().Be(1);
        ((int)LineageType.IronBlooded).Should().Be(2);
        ((int)LineageType.VargrKin).Should().Be(3);
    }

    [Theory]
    [InlineData(LineageType.Human, "Human")]
    [InlineData(LineageType.RuneMarked, "RuneMarked")]
    [InlineData(LineageType.IronBlooded, "IronBlooded")]
    [InlineData(LineageType.VargrKin, "VargrKin")]
    public void LineageType_ToString_ReturnsExpectedName(LineageType lineage, string expectedName)
    {
        // Assert
        lineage.ToString().Should().Be(expectedName);
    }
}
