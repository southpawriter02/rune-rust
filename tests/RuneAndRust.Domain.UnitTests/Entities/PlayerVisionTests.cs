using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Player vision properties (v0.4.3b).
/// </summary>
[TestFixture]
public class PlayerVisionTests
{
    private Player CreateTestPlayer() => new Player("TestPlayer");

    private Room CreateTestRoom(LightLevel level)
    {
        var room = new Room("Test Room", "A room", Position3D.Origin);
        room.SetBaseLightLevel(level);
        return room;
    }

    [Test]
    public void Player_DefaultVisionType_IsNormal()
    {
        // Arrange & Act
        var player = CreateTestPlayer();

        // Assert
        player.VisionType.Should().Be(VisionType.Normal);
    }

    [Test]
    public void Player_SetVisionType_UpdatesProperty()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        player.SetVisionType(VisionType.DarkVision);

        // Assert
        player.VisionType.Should().Be(VisionType.DarkVision);
    }

    [Test]
    public void GetEffectiveLightLevel_DarkVision_NegatesDark()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.SetVisionType(VisionType.DarkVision);
        var room = CreateTestRoom(LightLevel.Dark);

        // Act
        var effective = player.GetEffectiveLightLevel(room);

        // Assert
        effective.Should().Be(LightLevel.Dim);
    }

    [Test]
    public void GetEffectiveLightLevel_TrueSight_NegatesAll()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.SetVisionType(VisionType.TrueSight);
        var room = CreateTestRoom(LightLevel.MagicalDarkness);

        // Act
        var effective = player.GetEffectiveLightLevel(room);

        // Assert
        effective.Should().Be(LightLevel.Bright);
    }
}
