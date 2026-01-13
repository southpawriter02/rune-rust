using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Monster vision and light sensitivity (v0.4.3b).
/// </summary>
[TestFixture]
public class MonsterVisionTests
{
    private Monster CreateTestMonster() =>
        new Monster("Test Monster", "A monster", 50, Stats.Default);

    private Room CreateTestRoom(LightLevel level)
    {
        var room = new Room("Test Room", "A room", Position3D.Origin);
        room.SetBaseLightLevel(level);
        return room;
    }

    [Test]
    public void Monster_DefaultVisionType_IsNormal()
    {
        // Arrange & Act
        var monster = CreateTestMonster();

        // Assert
        monster.VisionType.Should().Be(VisionType.Normal);
    }

    [Test]
    public void Monster_DefaultLightSensitivity_IsFalse()
    {
        // Arrange & Act
        var monster = CreateTestMonster();

        // Assert
        monster.LightSensitivity.Should().BeFalse();
    }

    [Test]
    public void SetLightSensitivity_ConfiguresMonster()
    {
        // Arrange
        var monster = CreateTestMonster();

        // Act
        monster.SetLightSensitivity(true, -3);

        // Assert
        monster.LightSensitivity.Should().BeTrue();
        monster.LightSensitivityPenalty.Should().Be(-3);
    }

    [Test]
    public void GetEffectiveLightLevel_DarkVisionMonster_NegatesDark()
    {
        // Arrange
        var monster = CreateTestMonster();
        monster.SetVisionType(VisionType.DarkVision);
        var room = CreateTestRoom(LightLevel.Dark);

        // Act
        var effective = monster.GetEffectiveLightLevel(room);

        // Assert
        effective.Should().Be(LightLevel.Dim);
    }
}
