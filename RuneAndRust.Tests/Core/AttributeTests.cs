using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;
using Attribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the Attribute enum.
/// Validates the five core attributes: Sturdiness, Might, Wits, Will, Finesse.
/// </summary>
public class AttributeTests
{
    [Fact]
    public void Attribute_ShouldHaveExactlyFiveValues()
    {
        // Arrange
        var values = Enum.GetValues<Attribute>();

        // Assert
        values.Should().HaveCount(5, "Rune & Rust has exactly five core attributes");
    }

    [Fact]
    public void Attribute_ShouldContain_Sturdiness()
    {
        // Assert
        Enum.IsDefined(typeof(Attribute), Attribute.Sturdiness).Should().BeTrue();
    }

    [Fact]
    public void Attribute_ShouldContain_Might()
    {
        // Assert
        Enum.IsDefined(typeof(Attribute), Attribute.Might).Should().BeTrue();
    }

    [Fact]
    public void Attribute_ShouldContain_Wits()
    {
        // Assert
        Enum.IsDefined(typeof(Attribute), Attribute.Wits).Should().BeTrue();
    }

    [Fact]
    public void Attribute_ShouldContain_Will()
    {
        // Assert
        Enum.IsDefined(typeof(Attribute), Attribute.Will).Should().BeTrue();
    }

    [Fact]
    public void Attribute_ShouldContain_Finesse()
    {
        // Assert
        Enum.IsDefined(typeof(Attribute), Attribute.Finesse).Should().BeTrue();
    }

    [Fact]
    public void Attribute_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)Attribute.Sturdiness).Should().Be(0);
        ((int)Attribute.Might).Should().Be(1);
        ((int)Attribute.Wits).Should().Be(2);
        ((int)Attribute.Will).Should().Be(3);
        ((int)Attribute.Finesse).Should().Be(4);
    }

    [Theory]
    [InlineData(Attribute.Sturdiness, "Sturdiness")]
    [InlineData(Attribute.Might, "Might")]
    [InlineData(Attribute.Wits, "Wits")]
    [InlineData(Attribute.Will, "Will")]
    [InlineData(Attribute.Finesse, "Finesse")]
    public void Attribute_ToString_ReturnsExpectedName(Attribute attribute, string expectedName)
    {
        // Assert
        attribute.ToString().Should().Be(expectedName);
    }
}
