using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for WeatherType enum.
/// </summary>
[TestFixture]
public class WeatherTypeTests
{
    [Test]
    public void WeatherType_HasSixValues()
    {
        // Assert
        var values = Enum.GetValues<WeatherType>();
        values.Should().HaveCount(6);
    }

    [Test]
    public void WeatherType_ContainsExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(WeatherType), WeatherType.Clear).Should().BeTrue();
        Enum.IsDefined(typeof(WeatherType), WeatherType.Cloudy).Should().BeTrue();
        Enum.IsDefined(typeof(WeatherType), WeatherType.LightRain).Should().BeTrue();
        Enum.IsDefined(typeof(WeatherType), WeatherType.HeavyRain).Should().BeTrue();
        Enum.IsDefined(typeof(WeatherType), WeatherType.Fog).Should().BeTrue();
        Enum.IsDefined(typeof(WeatherType), WeatherType.Storm).Should().BeTrue();
    }
}
