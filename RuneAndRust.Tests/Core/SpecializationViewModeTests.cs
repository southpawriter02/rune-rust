using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the SpecializationViewMode enum.
/// Validates the view mode values and structure.
/// </summary>
/// <remarks>See: v0.4.1c (The Tree of Runes) for implementation.</remarks>
public class SpecializationViewModeTests
{
    [Fact]
    public void SpecializationViewMode_ShouldHaveExactlyTwoValues()
    {
        // Arrange
        var values = Enum.GetValues<SpecializationViewMode>();

        // Assert
        values.Should().HaveCount(2, "SpecializationViewMode should have exactly 2 values: SpecList, TreeDetail");
    }

    [Fact]
    public void SpecializationViewMode_ShouldContain_SpecList()
    {
        // Assert
        Enum.IsDefined(typeof(SpecializationViewMode), SpecializationViewMode.SpecList).Should().BeTrue();
    }

    [Fact]
    public void SpecializationViewMode_ShouldContain_TreeDetail()
    {
        // Assert
        Enum.IsDefined(typeof(SpecializationViewMode), SpecializationViewMode.TreeDetail).Should().BeTrue();
    }

    [Fact]
    public void SpecializationViewMode_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)SpecializationViewMode.SpecList).Should().Be(0);
        ((int)SpecializationViewMode.TreeDetail).Should().Be(1);
    }

    [Theory]
    [InlineData(SpecializationViewMode.SpecList, "SpecList")]
    [InlineData(SpecializationViewMode.TreeDetail, "TreeDetail")]
    public void SpecializationViewMode_ToString_ReturnsExpectedName(SpecializationViewMode mode, string expectedName)
    {
        // Assert
        mode.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(0, SpecializationViewMode.SpecList)]
    [InlineData(1, SpecializationViewMode.TreeDetail)]
    public void SpecializationViewMode_FromInt_ReturnsCorrectMode(int value, SpecializationViewMode expected)
    {
        // Act
        var mode = (SpecializationViewMode)value;

        // Assert
        mode.Should().Be(expected);
    }

    [Fact]
    public void SpecializationViewMode_DefaultValue_ShouldBeSpecList()
    {
        // Arrange & Act
        var defaultMode = default(SpecializationViewMode);

        // Assert
        defaultMode.Should().Be(SpecializationViewMode.SpecList);
    }
}
