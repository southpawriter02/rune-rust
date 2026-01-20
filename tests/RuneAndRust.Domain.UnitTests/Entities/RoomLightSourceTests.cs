using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Room light source management (v0.4.3b).
/// </summary>
[TestFixture]
public class RoomLightSourceTests
{
    private Room CreateTestRoom() =>
        new Room("Test Room", "A room", Position3D.Origin);

    [Test]
    public void AddLightSource_AddsToCollection()
    {
        // Arrange
        var room = CreateTestRoom();
        room.SetBaseLightLevel(LightLevel.Dark);
        var light = LightSource.Create("torch", "Torch", LightLevel.Bright);

        // Act
        room.AddLightSource(light);

        // Assert
        room.LightSources.Should().Contain(light);
    }

    [Test]
    public void HasActiveLightSources_WhenActive_ReturnsTrue()
    {
        // Arrange
        var room = CreateTestRoom();
        var light = LightSource.Create("torch", "Torch", LightLevel.Bright);
        light.Activate();
        room.AddLightSource(light);

        // Assert
        room.HasActiveLightSources.Should().BeTrue();
    }

    [Test]
    public void CalculateCurrentLightLevel_WithActiveSource_ReturnsBrightest()
    {
        // Arrange
        var room = CreateTestRoom();
        room.SetBaseLightLevel(LightLevel.Dark);
        var light = LightSource.Create("torch", "Torch", LightLevel.Bright);
        light.Activate();
        room.AddLightSource(light);

        // Act
        var level = room.CalculateCurrentLightLevel();

        // Assert
        level.Should().Be(LightLevel.Bright);
    }
}
