using FluentAssertions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ValueObjects;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the Room entity.
/// Validates room properties, exits, and default values.
/// </summary>
public class RoomTests
{
    #region Default Value Tests

    [Fact]
    public void Room_DefaultId_ShouldBeNewGuid()
    {
        // Arrange & Act
        var room = new Room();

        // Assert
        room.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Room_DefaultName_ShouldBeEmptyString()
    {
        // Arrange & Act
        var room = new Room();

        // Assert
        room.Name.Should().BeEmpty();
    }

    [Fact]
    public void Room_DefaultDescription_ShouldBeEmptyString()
    {
        // Arrange & Act
        var room = new Room();

        // Assert
        room.Description.Should().BeEmpty();
    }

    [Fact]
    public void Room_DefaultPosition_ShouldBeOrigin()
    {
        // Arrange & Act
        var room = new Room();

        // Assert
        room.Position.Should().Be(Coordinate.Origin);
    }

    [Fact]
    public void Room_DefaultExits_ShouldBeEmptyDictionary()
    {
        // Arrange & Act
        var room = new Room();

        // Assert
        room.Exits.Should().NotBeNull();
        room.Exits.Should().BeEmpty();
    }

    [Fact]
    public void Room_DefaultIsStartingRoom_ShouldBeFalse()
    {
        // Arrange & Act
        var room = new Room();

        // Assert
        room.IsStartingRoom.Should().BeFalse();
    }

    #endregion

    #region Property Setter Tests

    [Fact]
    public void Room_Id_CanBeSet()
    {
        // Arrange
        var room = new Room();
        var newId = Guid.NewGuid();

        // Act
        room.Id = newId;

        // Assert
        room.Id.Should().Be(newId);
    }

    [Fact]
    public void Room_Name_CanBeSet()
    {
        // Arrange
        var room = new Room();

        // Act
        room.Name = "Entry Hall";

        // Assert
        room.Name.Should().Be("Entry Hall");
    }

    [Fact]
    public void Room_Description_CanBeSet()
    {
        // Arrange
        var room = new Room();

        // Act
        room.Description = "A cold, metallic chamber.";

        // Assert
        room.Description.Should().Be("A cold, metallic chamber.");
    }

    [Fact]
    public void Room_Position_CanBeSet()
    {
        // Arrange
        var room = new Room();
        var position = new Coordinate(5, 10, -3);

        // Act
        room.Position = position;

        // Assert
        room.Position.Should().Be(position);
    }

    [Fact]
    public void Room_IsStartingRoom_CanBeSet()
    {
        // Arrange
        var room = new Room();

        // Act
        room.IsStartingRoom = true;

        // Assert
        room.IsStartingRoom.Should().BeTrue();
    }

    #endregion

    #region Exits Tests

    [Fact]
    public void Room_Exits_CanAddSingleExit()
    {
        // Arrange
        var room = new Room();
        var targetRoomId = Guid.NewGuid();

        // Act
        room.Exits[Direction.North] = targetRoomId;

        // Assert
        room.Exits.Should().HaveCount(1);
        room.Exits[Direction.North].Should().Be(targetRoomId);
    }

    [Fact]
    public void Room_Exits_CanAddMultipleExits()
    {
        // Arrange
        var room = new Room();
        var northRoom = Guid.NewGuid();
        var southRoom = Guid.NewGuid();
        var upRoom = Guid.NewGuid();

        // Act
        room.Exits[Direction.North] = northRoom;
        room.Exits[Direction.South] = southRoom;
        room.Exits[Direction.Up] = upRoom;

        // Assert
        room.Exits.Should().HaveCount(3);
        room.Exits[Direction.North].Should().Be(northRoom);
        room.Exits[Direction.South].Should().Be(southRoom);
        room.Exits[Direction.Up].Should().Be(upRoom);
    }

    [Fact]
    public void Room_Exits_CanBeReplaced()
    {
        // Arrange
        var room = new Room();
        var originalTarget = Guid.NewGuid();
        var newTarget = Guid.NewGuid();
        room.Exits[Direction.North] = originalTarget;

        // Act
        room.Exits[Direction.North] = newTarget;

        // Assert
        room.Exits[Direction.North].Should().Be(newTarget);
    }

    [Fact]
    public void Room_Exits_CanRemoveExit()
    {
        // Arrange
        var room = new Room();
        room.Exits[Direction.North] = Guid.NewGuid();
        room.Exits[Direction.South] = Guid.NewGuid();

        // Act
        room.Exits.Remove(Direction.North);

        // Assert
        room.Exits.Should().HaveCount(1);
        room.Exits.ContainsKey(Direction.North).Should().BeFalse();
        room.Exits.ContainsKey(Direction.South).Should().BeTrue();
    }

    [Fact]
    public void Room_Exits_ContainsKey_ReturnsTrueForExistingExit()
    {
        // Arrange
        var room = new Room();
        room.Exits[Direction.East] = Guid.NewGuid();

        // Act & Assert
        room.Exits.ContainsKey(Direction.East).Should().BeTrue();
    }

    [Fact]
    public void Room_Exits_ContainsKey_ReturnsFalseForMissingExit()
    {
        // Arrange
        var room = new Room();

        // Act & Assert
        room.Exits.ContainsKey(Direction.West).Should().BeFalse();
    }

    [Fact]
    public void Room_Exits_TryGetValue_ReturnsValueForExistingExit()
    {
        // Arrange
        var room = new Room();
        var targetId = Guid.NewGuid();
        room.Exits[Direction.Down] = targetId;

        // Act
        var found = room.Exits.TryGetValue(Direction.Down, out var resultId);

        // Assert
        found.Should().BeTrue();
        resultId.Should().Be(targetId);
    }

    [Fact]
    public void Room_Exits_TryGetValue_ReturnsFalseForMissingExit()
    {
        // Arrange
        var room = new Room();

        // Act
        var found = room.Exits.TryGetValue(Direction.Up, out _);

        // Assert
        found.Should().BeFalse();
    }

    [Fact]
    public void Room_Exits_CanSupportAllSixDirections()
    {
        // Arrange
        var room = new Room();
        var directions = Enum.GetValues<Direction>();

        // Act
        foreach (var dir in directions)
        {
            room.Exits[dir] = Guid.NewGuid();
        }

        // Assert
        room.Exits.Should().HaveCount(6);
        foreach (var dir in directions)
        {
            room.Exits.ContainsKey(dir).Should().BeTrue();
        }
    }

    #endregion

    #region Object Initializer Tests

    [Fact]
    public void Room_ObjectInitializer_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var position = new Coordinate(1, 2, 3);
        var exits = new Dictionary<Direction, Guid>
        {
            { Direction.North, Guid.NewGuid() }
        };

        // Act
        var room = new Room
        {
            Id = id,
            Name = "Test Room",
            Description = "A test room description.",
            Position = position,
            Exits = exits,
            IsStartingRoom = true
        };

        // Assert
        room.Id.Should().Be(id);
        room.Name.Should().Be("Test Room");
        room.Description.Should().Be("A test room description.");
        room.Position.Should().Be(position);
        room.Exits.Should().BeSameAs(exits);
        room.IsStartingRoom.Should().BeTrue();
    }

    #endregion

    #region Unique ID Tests

    [Fact]
    public void Room_MultipleInstances_HaveUniqueIds()
    {
        // Arrange & Act
        var room1 = new Room();
        var room2 = new Room();
        var room3 = new Room();

        // Assert
        room1.Id.Should().NotBe(room2.Id);
        room2.Id.Should().NotBe(room3.Id);
        room1.Id.Should().NotBe(room3.Id);
    }

    #endregion
}
