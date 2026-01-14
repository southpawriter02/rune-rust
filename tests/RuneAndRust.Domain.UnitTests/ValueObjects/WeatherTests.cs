using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for Weather value object.
/// </summary>
[TestFixture]
public class WeatherTests
{
    [Test]
    public void Create_WithType_SetsPropertiesCorrectly()
    {
        // Act
        var weather = Weather.Create(WeatherType.LightRain, duration: 3);

        // Assert
        weather.Type.Should().Be(WeatherType.LightRain);
        weather.Duration.Should().Be(3);
        weather.HasDuration.Should().BeTrue();
        weather.IsPermanent.Should().BeFalse();
    }

    [Test]
    public void Clear_ReturnsIndefiniteClearWeather()
    {
        // Act
        var weather = Weather.Clear();

        // Assert
        weather.Type.Should().Be(WeatherType.Clear);
        weather.IsPermanent.Should().BeTrue();
    }

    [Test]
    public void VisibilityModifier_Fog_ReturnsNegativeFour()
    {
        // Arrange
        var weather = Weather.Fog();

        // Assert
        weather.VisibilityModifier.Should().Be(-4);
    }

    [Test]
    public void VisibilityModifier_Storm_ReturnsNegativeFive()
    {
        // Arrange
        var weather = Weather.Storm();

        // Assert
        weather.VisibilityModifier.Should().Be(-5);
    }

    [Test]
    public void AffectsLight_ForHeavyRain_ReturnsTrue()
    {
        // Arrange
        var weather = Weather.HeavyRain();

        // Assert
        weather.AffectsLight.Should().BeTrue();
    }

    [Test]
    public void AffectsLight_ForCloudy_ReturnsFalse()
    {
        // Arrange
        var weather = Weather.Cloudy();

        // Assert
        weather.AffectsLight.Should().BeFalse();
    }

    [Test]
    public void AdvanceHour_DecrementsDuration()
    {
        // Arrange
        var weather = Weather.Create(WeatherType.LightRain, duration: 3);

        // Act
        weather.AdvanceHour();

        // Assert
        weather.Duration.Should().Be(2);
    }

    [Test]
    public void AdvanceHour_AtZeroDuration_ReturnsTrue()
    {
        // Arrange
        var weather = Weather.Create(WeatherType.LightRain, duration: 1);

        // Act
        var ended = weather.AdvanceHour();

        // Assert
        ended.Should().BeTrue();
        weather.Duration.Should().Be(0);
    }

    [Test]
    public void GetDescription_ReturnsAppropriateText()
    {
        // Assert
        Weather.Clear().GetDescription().Should().Contain("Clear");
        Weather.Fog().GetDescription().Should().Contain("fog");
        Weather.Storm().GetDescription().Should().Contain("storm");
    }
}
