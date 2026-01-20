using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for the LightLevel enum.
/// </summary>
[TestFixture]
public class LightLevelTests
{
    [Test]
    public void LightLevel_HasFourValues()
    {
        // Assert
        var values = Enum.GetValues<LightLevel>();
        values.Should().HaveCount(4);
    }

    [Test]
    public void LightLevel_BrightIsZero()
    {
        // Assert
        ((int)LightLevel.Bright).Should().Be(0);
    }

    [Test]
    public void LightLevel_MagicalDarknessIsThree()
    {
        // Assert
        ((int)LightLevel.MagicalDarkness).Should().Be(3);
    }

    [Test]
    public void LightLevel_ValuesAreOrdered()
    {
        // Assert
        ((int)LightLevel.Bright).Should().BeLessThan((int)LightLevel.Dim);
        ((int)LightLevel.Dim).Should().BeLessThan((int)LightLevel.Dark);
        ((int)LightLevel.Dark).Should().BeLessThan((int)LightLevel.MagicalDarkness);
    }
}
