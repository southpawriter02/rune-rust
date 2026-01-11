using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Dungeon vertical connection functionality.
/// </summary>
[TestFixture]
public class DungeonVerticalTests
{
    // ===== ConnectRoomsVertically Tests =====

    [Test]
    public void ConnectRoomsVertically_CreatesBidirectionalConnection()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var upperRoom = new Room("Upper Room", "A room above.", new Position3D(0, 0, 0));
        var lowerRoom = new Room("Lower Room", "A room below.", new Position3D(0, 0, 1));
        dungeon.AddRoom(upperRoom);
        dungeon.AddRoom(lowerRoom);

        // Act
        dungeon.ConnectRoomsVertically(upperRoom.Id, lowerRoom.Id, StairType.Ladder);

        // Assert
        upperRoom.GetExit(Direction.Down).Should().Be(lowerRoom.Id);
        lowerRoom.GetExit(Direction.Up).Should().Be(upperRoom.Id);
    }

    [Test]
    public void ConnectRoomsVertically_WithPit_CreatesOneWayConnection()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var upperRoom = new Room("Upper Room", "A room above.", new Position3D(0, 0, 0));
        var lowerRoom = new Room("Lower Room", "A room below.", new Position3D(0, 0, 1));
        dungeon.AddRoom(upperRoom);
        dungeon.AddRoom(lowerRoom);

        // Act
        dungeon.ConnectRoomsVertically(upperRoom.Id, lowerRoom.Id, StairType.Pit);

        // Assert
        upperRoom.GetExit(Direction.Down).Should().Be(lowerRoom.Id);
        lowerRoom.GetExit(Direction.Up).Should().BeNull(); // Pit is one-way
    }

    [Test]
    public void ConnectRoomsVertically_WithInvalidZRelationship_ThrowsArgumentException()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var room1 = new Room("Room 1", "Same level.", new Position3D(0, 0, 0));
        var room2 = new Room("Room 2", "Same level.", new Position3D(1, 0, 0));
        dungeon.AddRoom(room1);
        dungeon.AddRoom(room2);

        // Act
        var act = () => dungeon.ConnectRoomsVertically(room1.Id, room2.Id, StairType.Ladder);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ConnectRoomsVertically_WithNonExistentRoom_ThrowsArgumentException()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var room = new Room("Room", "A room.", new Position3D(0, 0, 0));
        dungeon.AddRoom(room);

        // Act
        var act = () => dungeon.ConnectRoomsVertically(room.Id, Guid.NewGuid(), StairType.Ladder);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ===== GetStairType Tests =====

    [Test]
    public void GetStairType_ReturnsCorrectType()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var upperRoom = new Room("Upper Room", "A room above.", new Position3D(0, 0, 0));
        var lowerRoom = new Room("Lower Room", "A room below.", new Position3D(0, 0, 1));
        dungeon.AddRoom(upperRoom);
        dungeon.AddRoom(lowerRoom);
        dungeon.ConnectRoomsVertically(upperRoom.Id, lowerRoom.Id, StairType.SpiralStairs);

        // Act
        var stairType1 = dungeon.GetStairType(upperRoom.Id, lowerRoom.Id);
        var stairType2 = dungeon.GetStairType(lowerRoom.Id, upperRoom.Id);

        // Assert
        stairType1.Should().Be(StairType.SpiralStairs);
        stairType2.Should().Be(StairType.SpiralStairs);
    }

    [Test]
    public void GetStairType_WithNoConnection_ReturnsNull()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var room1 = new Room("Room 1", "A room.", new Position3D(0, 0, 0));
        var room2 = new Room("Room 2", "Another room.", new Position3D(0, 0, 1));
        dungeon.AddRoom(room1);
        dungeon.AddRoom(room2);

        // Act
        var stairType = dungeon.GetStairType(room1.Id, room2.Id);

        // Assert
        stairType.Should().BeNull();
    }

    // ===== GetRoomByPosition(Position3D) Tests =====

    [Test]
    public void GetRoomByPosition3D_FindsExactRoom()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var room1 = new Room("Level 0 Room", "Surface.", new Position3D(0, 0, 0));
        var room2 = new Room("Level 1 Room", "Underground.", new Position3D(0, 0, 1));
        dungeon.AddRoom(room1);
        dungeon.AddRoom(room2);

        // Act
        var foundL0 = dungeon.GetRoomByPosition(new Position3D(0, 0, 0));
        var foundL1 = dungeon.GetRoomByPosition(new Position3D(0, 0, 1));

        // Assert
        foundL0.Should().Be(room1);
        foundL1.Should().Be(room2);
    }

    [Test]
    public void HasRoomAt_ReturnsTrueForExistingRoom()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var room = new Room("Test Room", "A room.", new Position3D(1, 2, 3));
        dungeon.AddRoom(room);

        // Act & Assert
        dungeon.HasRoomAt(new Position3D(1, 2, 3)).Should().BeTrue();
        dungeon.HasRoomAt(new Position3D(0, 0, 0)).Should().BeFalse();
    }

    // ===== Starter Dungeon Tests =====

    [Test]
    public void CreateStarterDungeon_HasTwoLevels()
    {
        // Act
        var dungeon = Dungeon.CreateStarterDungeon();

        // Assert
        var level0Rooms = dungeon.Rooms.Values.Where(r => r.Position.Z == 0);
        var level1Rooms = dungeon.Rooms.Values.Where(r => r.Position.Z == 1);

        level0Rooms.Should().HaveCount(5);
        level1Rooms.Should().HaveCount(1);
    }

    [Test]
    public void CreateStarterDungeon_HasVerticalConnection()
    {
        // Act
        var dungeon = Dungeon.CreateStarterDungeon();

        // Assert
        var passage = dungeon.Rooms.Values.First(r => r.Name == "Dungeon Passage");
        var crypt = dungeon.Rooms.Values.First(r => r.Name == "Forgotten Crypt");

        passage.GetExit(Direction.Down).Should().Be(crypt.Id);
        crypt.GetExit(Direction.Up).Should().Be(passage.Id);
    }

    [Test]
    public void CreateStarterDungeon_VerticalConnectionHasStairType()
    {
        // Act
        var dungeon = Dungeon.CreateStarterDungeon();
        var passage = dungeon.Rooms.Values.First(r => r.Name == "Dungeon Passage");
        var crypt = dungeon.Rooms.Values.First(r => r.Name == "Forgotten Crypt");

        // Assert
        var stairType = dungeon.GetStairType(passage.Id, crypt.Id);
        stairType.Should().Be(StairType.SpiralStairs);
    }
}
