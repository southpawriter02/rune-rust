using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Room vertical exits functionality.
/// </summary>
[TestFixture]
public class RoomExitsTests
{
    [Test]
    public void GetExitsDescription_WithHorizontalExits_ReturnsFormattedString()
    {
        // Arrange
        var room = new Room("Test Room", "A test room.", new Position3D(0, 0, 0));
        room.AddExit(Direction.North, Guid.NewGuid());
        room.AddExit(Direction.East, Guid.NewGuid());

        // Act
        var description = room.GetExitsDescription();

        // Assert
        description.Should().Be("Exits: north, east");
    }

    [Test]
    public void GetExitsDescription_WithVerticalExits_IncludesUpAndDown()
    {
        // Arrange
        var room = new Room("Test Room", "A test room.", new Position3D(0, 0, 1));
        room.AddExit(Direction.Up, Guid.NewGuid());
        room.AddExit(Direction.Down, Guid.NewGuid());
        room.AddExit(Direction.North, Guid.NewGuid());

        // Act
        var description = room.GetExitsDescription();

        // Assert
        description.Should().Contain("north");
        description.Should().Contain("up");
        description.Should().Contain("down");
    }

    [Test]
    public void GetExitsDescription_WithNoExits_ReturnsNoExitsMessage()
    {
        // Arrange
        var room = new Room("Test Room", "A test room.", new Position3D(0, 0, 0));

        // Act
        var description = room.GetExitsDescription();

        // Assert
        description.Should().Be("There are no visible exits.");
    }

    [Test]
    public void HasExit_WithVerticalDirection_Works()
    {
        // Arrange
        var room = new Room("Test Room", "A test room.", new Position3D(0, 0, 0));
        var targetId = Guid.NewGuid();
        room.AddExit(Direction.Down, targetId);

        // Act & Assert
        room.HasExit(Direction.Down).Should().BeTrue();
        room.HasExit(Direction.Up).Should().BeFalse();
        room.GetExit(Direction.Down).Should().Be(targetId);
    }

    [Test]
    public void Position3D_Constructor_SetsCorrectPosition()
    {
        // Arrange & Act
        var room = new Room("Test Room", "A test room.", new Position3D(1, 2, 3));

        // Assert
        room.Position.X.Should().Be(1);
        room.Position.Y.Should().Be(2);
        room.Position.Z.Should().Be(3);
    }

    [Test]
    [Obsolete("Testing obsolete constructor for backwards compatibility")]
    public void Position2D_Constructor_SetsZToZero()
    {
        // Arrange & Act
        var room = new Room("Test Room", "A test room.", new Position(1, 2));

        // Assert
        room.Position.X.Should().Be(1);
        room.Position.Y.Should().Be(2);
        room.Position.Z.Should().Be(0);
    }
}
