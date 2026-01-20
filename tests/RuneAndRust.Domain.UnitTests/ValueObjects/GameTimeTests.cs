using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for GameTime value object.
/// </summary>
[TestFixture]
public class GameTimeTests
{
    [Test]
    public void Create_WithDefaults_StartsAtMorning()
    {
        // Act
        var time = GameTime.Create();

        // Assert
        time.Hour.Should().Be(8);
        time.Day.Should().Be(1);
        time.TimeOfDay.Should().Be(TimeOfDay.Morning);
        time.IsDaytime.Should().BeTrue();
        time.IsNighttime.Should().BeFalse();
    }

    [Test]
    public void Create_WithCustomValues_SetsCorrectly()
    {
        // Act
        var time = GameTime.Create(hour: 22, day: 5);

        // Assert
        time.Hour.Should().Be(22);
        time.Day.Should().Be(5);
        time.TimeOfDay.Should().Be(TimeOfDay.Evening);
        time.IsNighttime.Should().BeTrue();
    }

    [Test]
    public void AdvanceTurn_AfterTurnsPerHour_IncrementsHour()
    {
        // Arrange
        var time = GameTime.Create(hour: 8, turnsPerHour: 6);

        // Act - advance 6 turns
        for (int i = 0; i < 6; i++)
            time.AdvanceTurn();

        // Assert
        time.Hour.Should().Be(9);
        time.TurnCount.Should().Be(0);
    }

    [Test]
    public void AdvanceHours_CrossesMidnight_IncrementDay()
    {
        // Arrange
        var time = GameTime.Create(hour: 22);

        // Act
        time.AdvanceHours(5);

        // Assert
        time.Hour.Should().Be(3);
        time.Day.Should().Be(2);
    }

    [Test]
    public void TimeOfDay_Hour2_ReturnsNight()
    {
        // Arrange
        var time = GameTime.Create(hour: 2);

        // Assert
        time.TimeOfDay.Should().Be(TimeOfDay.Night);
    }

    [Test]
    public void TimeOfDay_Hour12_ReturnsNoon()
    {
        // Arrange
        var time = GameTime.Create(hour: 12);

        // Assert
        time.TimeOfDay.Should().Be(TimeOfDay.Noon);
    }

    [Test]
    public void GetOutdoorLightLevel_AtNight_ReturnsDark()
    {
        // Arrange
        var time = GameTime.Create(hour: 2);

        // Act
        var light = time.GetOutdoorLightLevel(WeatherType.Clear);

        // Assert
        light.Should().Be(LightLevel.Dark);
    }

    [Test]
    public void GetOutdoorLightLevel_AtNoonWithFog_ReturnsDim()
    {
        // Arrange
        var time = GameTime.Create(hour: 12);

        // Act
        var light = time.GetOutdoorLightLevel(WeatherType.Fog);

        // Assert
        light.Should().Be(LightLevel.Dim);
    }

    [Test]
    public void GetDisplayString_ReturnsFormattedString()
    {
        // Arrange
        var time = GameTime.Create(hour: 14, day: 3);

        // Act
        var display = time.GetDisplayString();

        // Assert
        display.Should().Contain("Day 3");
        display.Should().Contain("PM");
        display.Should().Contain("Afternoon");
    }
}
