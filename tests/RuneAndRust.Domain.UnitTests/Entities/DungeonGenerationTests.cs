using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class DungeonGenerationTests
{
    [Test]
    public void CanGenerateAt_EmptyPosition_ReturnsTrue()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var position = new Position3D(0, 0, 0);

        // Act & Assert
        dungeon.CanGenerateAt(position).Should().BeTrue();
    }

    [Test]
    public void CanGenerateAt_OccupiedPosition_ReturnsFalse()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var position = new Position3D(0, 0, 0);
        var room = new Room("Test Room", "Description", position);
        dungeon.AddRoom(room);

        // Act & Assert
        dungeon.CanGenerateAt(position).Should().BeFalse();
    }

    [Test]
    public void CanGenerateAt_AtRoomLimit_ReturnsFalse()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        dungeon.MaxRoomsPerLevel = 3;

        // Add 3 rooms at level 0
        for (var i = 0; i < 3; i++)
        {
            var room = new Room($"Room {i}", "Description", new Position3D(i, 0, 0));
            dungeon.AddRoom(room);
        }

        // Act - Try to generate at a new position on level 0
        var newPosition = new Position3D(10, 10, 0);

        // Assert
        dungeon.CanGenerateAt(newPosition).Should().BeFalse();
    }

    [Test]
    public void GetOrAddRoom_ExistingRoom_ReturnsExistingRoom()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var position = new Position3D(0, 0, 0);
        var existingRoom = new Room("Existing Room", "Description", position);
        dungeon.AddRoom(existingRoom);

        var factoryCalled = false;

        // Act
        var result = dungeon.GetOrAddRoom(position, () =>
        {
            factoryCalled = true;
            return new Room("New Room", "Description", position);
        });

        // Assert
        result.Should().Be(existingRoom);
        factoryCalled.Should().BeFalse();
    }

    [Test]
    public void GetOrAddRoom_NewPosition_CreatesAndAddsRoom()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var position = new Position3D(1, 1, 0);
        var newRoom = new Room("New Room", "Description", position);

        // Act
        var result = dungeon.GetOrAddRoom(position, () => newRoom);

        // Assert
        result.Should().Be(newRoom);
        dungeon.HasRoomAt(position).Should().BeTrue();
        dungeon.RoomCount.Should().Be(1);
    }

    [Test]
    public void GetOrAddRoom_AtRoomLimit_ReturnsNull()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        dungeon.MaxRoomsPerLevel = 2;

        // Fill level 0
        for (var i = 0; i < 2; i++)
        {
            var room = new Room($"Room {i}", "Description", new Position3D(i, 0, 0));
            dungeon.AddRoom(room);
        }

        // Act
        var result = dungeon.GetOrAddRoom(
            new Position3D(10, 10, 0),
            () => new Room("New", "Desc", new Position3D(10, 10, 0)));

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetRoomCountAtLevel_ReturnsCorrectCount()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        dungeon.AddRoom(new Room("L0 Room 1", "Desc", new Position3D(0, 0, 0)));
        dungeon.AddRoom(new Room("L0 Room 2", "Desc", new Position3D(1, 0, 0)));
        dungeon.AddRoom(new Room("L1 Room 1", "Desc", new Position3D(0, 0, 1)));

        // Act & Assert
        dungeon.GetRoomCountAtLevel(0).Should().Be(2);
        dungeon.GetRoomCountAtLevel(1).Should().Be(1);
        dungeon.GetRoomCountAtLevel(2).Should().Be(0);
    }

    [Test]
    public void MaxRoomsPerLevel_DefaultValue_IsFifty()
    {
        // Arrange & Act
        var dungeon = new Dungeon("Test Dungeon");

        // Assert
        dungeon.MaxRoomsPerLevel.Should().Be(50);
    }
}
