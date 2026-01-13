using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Services;

/// <summary>
/// Tests for CombatService light penalty methods (v0.4.3a).
/// </summary>
[TestFixture]
public class CombatServiceLightTests
{
    private CombatService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new CombatService();
    }

    private Room CreateRoom(LightLevel level)
    {
        var room = new Room("Test Room", "A room", Position3D.Origin);
        room.SetBaseLightLevel(level);
        return room;
    }

    [Test]
    public void GetLightPenalty_BrightRoom_ReturnsZero()
    {
        // Arrange
        var room = CreateRoom(LightLevel.Bright);

        // Act
        var penalty = _service.GetLightPenalty(room);

        // Assert
        penalty.Should().Be(0);
    }

    [Test]
    public void GetLightPenalty_DimRoom_ReturnsNegativeOne()
    {
        // Arrange
        var room = CreateRoom(LightLevel.Dim);

        // Act
        var penalty = _service.GetLightPenalty(room);

        // Assert
        penalty.Should().Be(-1);
    }

    [Test]
    public void GetLightPenalty_DarkRoom_ReturnsNegativeThree()
    {
        // Arrange
        var room = CreateRoom(LightLevel.Dark);

        // Act
        var penalty = _service.GetLightPenalty(room);

        // Assert
        penalty.Should().Be(-3);
    }

    [Test]
    public void GetLightPenalty_MagicalDarknessRoom_ReturnsNegativeFive()
    {
        // Arrange
        var room = CreateRoom(LightLevel.MagicalDarkness);

        // Act
        var penalty = _service.GetLightPenalty(room);

        // Assert
        penalty.Should().Be(-5);
    }

    [Test]
    public void GetLightPenalty_WithPlayer_ReturnsRoomPenalty()
    {
        // Arrange
        var room = CreateRoom(LightLevel.Dark);
        var player = new Player("TestPlayer");

        // Act
        var penalty = _service.GetLightPenalty(room, player);

        // Assert (v0.4.3a ignores player vision type)
        penalty.Should().Be(-3);
    }

    [Test]
    public void GetLightPenalty_WithNullRoom_ThrowsException()
    {
        // Act
        var act = () => _service.GetLightPenalty(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
