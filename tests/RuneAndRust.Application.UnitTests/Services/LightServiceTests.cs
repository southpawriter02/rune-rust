using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for LightService (v0.4.3b).
/// </summary>
[TestFixture]
public class LightServiceTests
{
    private LightService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new LightService();
    }

    private Player CreateTestPlayer() => new Player("TestPlayer");

    private Room CreateTestRoom(LightLevel level)
    {
        var room = new Room("Test Room", "A room", Position3D.Origin);
        room.SetBaseLightLevel(level);
        return room;
    }

    [Test]
    public void GetLightSensitivityPenalty_SensitiveMonsterInBrightLight_ReturnsPenalty()
    {
        // Arrange
        var monster = new Monster("Goblin", "A goblin", 30, Stats.Default);
        monster.SetLightSensitivity(true, -2);
        var room = CreateTestRoom(LightLevel.Bright);

        // Act
        var penalty = _service.GetLightSensitivityPenalty(monster, room);

        // Assert
        penalty.Should().Be(-2);
    }

    [Test]
    public void GetLightSensitivityPenalty_SensitiveMonsterInDark_ReturnsZero()
    {
        // Arrange
        var monster = new Monster("Goblin", "A goblin", 30, Stats.Default);
        monster.SetLightSensitivity(true, -2);
        var room = CreateTestRoom(LightLevel.Dark);

        // Act
        var penalty = _service.GetLightSensitivityPenalty(monster, room);

        // Assert
        penalty.Should().Be(0);
    }

    [Test]
    public void GetEffectiveLightLevel_DelegatesToPlayer()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.SetVisionType(VisionType.DarkVision);
        var room = CreateTestRoom(LightLevel.Dark);

        // Act
        var level = _service.GetEffectiveLightLevel(player, room);

        // Assert
        level.Should().Be(LightLevel.Dim);
    }
}
