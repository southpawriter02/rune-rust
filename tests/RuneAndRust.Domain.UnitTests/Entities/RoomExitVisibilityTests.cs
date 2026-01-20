using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Room exit visibility methods.
/// </summary>
[TestFixture]
public class RoomExitVisibilityTests
{
    [Test]
    public void GetVisibleExits_ReturnsOnlyVisibleExits()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        var visibleTarget = Guid.NewGuid();
        var hiddenTarget = Guid.NewGuid();

        room.AddExit(Direction.North, visibleTarget);
        room.AddHiddenExit(Direction.East, hiddenTarget, 15);

        // Act
        var visibleExits = room.GetVisibleExits();

        // Assert
        visibleExits.Should().HaveCount(1);
        visibleExits.Should().ContainKey(Direction.North);
        visibleExits.Should().NotContainKey(Direction.East);
    }

    [Test]
    public void GetHiddenExits_ReturnsOnlyUndiscoveredHiddenExits()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        room.AddExit(Direction.North, Guid.NewGuid());
        room.AddHiddenExit(Direction.East, Guid.NewGuid(), 12);
        room.AddHiddenExit(Direction.West, Guid.NewGuid(), 15);

        // Act
        var hiddenExits = room.GetHiddenExits();

        // Assert
        hiddenExits.Should().HaveCount(2);
        hiddenExits.Should().ContainKey(Direction.East);
        hiddenExits.Should().ContainKey(Direction.West);
    }

    [Test]
    public void RevealExit_MarksHiddenExitAsDiscovered()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        var targetId = Guid.NewGuid();
        room.AddHiddenExit(Direction.South, targetId, 12);

        // Act
        var revealed = room.RevealExit(Direction.South);

        // Assert
        revealed.Should().BeTrue();
        room.GetVisibleExits().Should().ContainKey(Direction.South);
        room.GetHiddenExits().Should().NotContainKey(Direction.South);
    }

    [Test]
    public void RevealExit_ReturnsFalseForNonHiddenExit()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        room.AddExit(Direction.North, Guid.NewGuid());

        // Act
        var revealed = room.RevealExit(Direction.North);

        // Assert
        revealed.Should().BeFalse();
    }

    [Test]
    public void RevealExit_ReturnsFalseForNonExistentExit()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);

        // Act
        var revealed = room.RevealExit(Direction.West);

        // Assert
        revealed.Should().BeFalse();
    }

    [Test]
    public void HasVisibleExit_ReturnsTrueForVisibleExit()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        room.AddExit(Direction.North, Guid.NewGuid());

        // Act & Assert
        room.HasVisibleExit(Direction.North).Should().BeTrue();
    }

    [Test]
    public void HasVisibleExit_ReturnsFalseForHiddenUndiscoveredExit()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        room.AddHiddenExit(Direction.East, Guid.NewGuid(), 15);

        // Act & Assert
        room.HasVisibleExit(Direction.East).Should().BeFalse();
    }

    [Test]
    public void GetExitsDescription_OnlyShowsVisibleExits()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        room.AddExit(Direction.North, Guid.NewGuid());
        room.AddHiddenExit(Direction.East, Guid.NewGuid(), 15);

        // Act
        var description = room.GetExitsDescription();

        // Assert
        description.Should().Contain("north");
        description.Should().NotContain("east");
    }

    [Test]
    public void GetExit_ReturnsIdForVisibleExit()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        var targetId = Guid.NewGuid();
        room.AddExit(Direction.North, targetId);

        // Act
        var result = room.GetExit(Direction.North);

        // Assert
        result.Should().Be(targetId);
    }

    [Test]
    public void GetExit_ReturnsNullForHiddenUndiscoveredExit()
    {
        // Arrange
        var room = new Room("Test", "Test room", Position3D.Origin);
        room.AddHiddenExit(Direction.East, Guid.NewGuid(), 15);

        // Act
        var result = room.GetExit(Direction.East);

        // Assert
        result.Should().BeNull();
    }
}
