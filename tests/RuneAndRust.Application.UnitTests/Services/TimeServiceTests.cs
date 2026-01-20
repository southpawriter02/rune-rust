using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for TimeService.
/// </summary>
[TestFixture]
public class TimeServiceTests
{
    private TimeService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new TimeService(startingHour: 8, startingDay: 1);
    }

    [Test]
    public void Constructor_InitializesTimeAndWeather()
    {
        // Assert
        _service.CurrentTime.Hour.Should().Be(8);
        _service.CurrentTime.Day.Should().Be(1);
        _service.CurrentWeather.Type.Should().Be(WeatherType.Clear);
    }

    [Test]
    public void AdvanceTurn_AdvancesTimeCorrectly()
    {
        // Act - advance 6 turns (default turnsPerHour)
        for (int i = 0; i < 6; i++)
            _service.AdvanceTurn();

        // Assert
        _service.CurrentTime.Hour.Should().Be(9);
    }

    [Test]
    public void AdvanceHours_AdvancesMultipleHours()
    {
        // Act
        _service.AdvanceHours(5);

        // Assert
        _service.CurrentTime.Hour.Should().Be(13);
    }

    [Test]
    public void SetWeather_ChangesCurrentWeather()
    {
        // Arrange
        var newWeather = Weather.Fog(duration: 3);

        // Act
        _service.SetWeather(newWeather);

        // Assert
        _service.CurrentWeather.Type.Should().Be(WeatherType.Fog);
        _service.CurrentWeather.Duration.Should().Be(3);
    }

    [Test]
    public void GetTimeDisplay_ReturnsFormattedString()
    {
        // Act
        var display = _service.GetTimeDisplay();

        // Assert
        display.Should().Contain("Day 1");
        display.Should().Contain("8");
    }

    [Test]
    public void GetTimeAndWeatherDisplay_IncludesWeather()
    {
        // Act
        var display = _service.GetTimeAndWeatherDisplay();

        // Assert
        display.Should().Contain("Weather:");
        display.Should().Contain("Clear");
    }
}
