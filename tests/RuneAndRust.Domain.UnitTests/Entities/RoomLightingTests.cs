using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Room light level functionality (v0.4.3a).
/// </summary>
[TestFixture]
public class RoomLightingTests
{
    private Room CreateTestRoom() =>
        new Room("Test Room", "A room", Position3D.Origin);

    [Test]
    public void Room_DefaultBaseLightLevel_IsDim()
    {
        // Arrange & Act
        var room = CreateTestRoom();

        // Assert
        room.BaseLightLevel.Should().Be(LightLevel.Dim);
    }

    [Test]
    public void Room_DefaultIsOutdoor_IsFalse()
    {
        // Arrange & Act
        var room = CreateTestRoom();

        // Assert
        room.IsOutdoor.Should().BeFalse();
    }

    [Test]
    public void Room_SetBaseLightLevel_UpdatesProperty()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        room.SetBaseLightLevel(LightLevel.Dark);

        // Assert
        room.BaseLightLevel.Should().Be(LightLevel.Dark);
    }

    [Test]
    public void Room_SetIsOutdoor_UpdatesProperty()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        room.SetIsOutdoor(true);

        // Assert
        room.IsOutdoor.Should().BeTrue();
    }

    [Test]
    public void Room_CurrentLightLevel_ReturnsBaseLightLevel()
    {
        // Arrange
        var room = CreateTestRoom();
        room.SetBaseLightLevel(LightLevel.Dark);

        // Act & Assert
        room.CurrentLightLevel.Should().Be(LightLevel.Dark);
    }

    [Test]
    public void Room_HasActiveLightSources_ReturnsFalse()
    {
        // Arrange & Act
        var room = CreateTestRoom();

        // Assert (placeholder for v0.4.3b)
        room.HasActiveLightSources.Should().BeFalse();
    }

    [Test]
    public void Room_CalculateCurrentLightLevel_ReturnsBase()
    {
        // Arrange
        var room = CreateTestRoom();
        room.SetBaseLightLevel(LightLevel.MagicalDarkness);

        // Act
        var level = room.CalculateCurrentLightLevel();

        // Assert
        level.Should().Be(LightLevel.MagicalDarkness);
    }

    [Test]
    public void Room_AllLightLevelsStorable()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act & Assert - each level can be stored
        foreach (var level in Enum.GetValues<LightLevel>())
        {
            room.SetBaseLightLevel(level);
            room.BaseLightLevel.Should().Be(level);
        }
    }
}
