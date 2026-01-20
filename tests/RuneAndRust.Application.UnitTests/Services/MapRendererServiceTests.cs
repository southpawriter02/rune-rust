using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for MapRendererService.
/// </summary>
[TestFixture]
public class MapRendererServiceTests
{
    private MapRendererService CreateService()
    {
        var logger = Mock.Of<ILogger<MapRendererService>>();
        return new MapRendererService(logger);
    }

    private Dungeon CreateDungeonWithSingleRoom()
    {
        var dungeon = new Dungeon("Test Dungeon");
        var room = new Room("Start", "Starting room", Position3D.Origin);
        room.MarkVisited();
        dungeon.AddRoom(room, isStartingRoom: true);
        return dungeon;
    }

    private Dungeon CreateDungeonWithConnectedRooms()
    {
        var dungeon = new Dungeon("Test Dungeon");

        var room1 = new Room("Room 1", "First room", new Position3D(0, 0, 0));
        var room2 = new Room("Room 2", "Second room", new Position3D(1, 0, 0));
        var room3 = new Room("Room 3", "Third room", new Position3D(0, 1, 0));

        dungeon.AddRoom(room1, isStartingRoom: true);
        dungeon.AddRoom(room2);
        dungeon.AddRoom(room3);

        dungeon.ConnectRooms(room1.Id, Direction.East, room2.Id);
        dungeon.ConnectRooms(room1.Id, Direction.North, room3.Id);

        room1.MarkVisited();
        room2.MarkVisited();
        room3.MarkVisited();

        return dungeon;
    }

    [Test]
    public void GetRoomSymbol_PlayerPosition_ReturnsAtSign()
    {
        // Arrange
        var service = CreateService();
        var room = new Room("Test", "Test room", Position3D.Origin);

        // Act
        var symbol = service.GetRoomSymbol(room, isPlayerHere: true);

        // Assert
        symbol.Should().Be('@');
    }

    [Test]
    public void GetRoomSymbol_UnvisitedRoom_ReturnsQuestionMark()
    {
        // Arrange
        var service = CreateService();
        var room = new Room("Test", "Test room", Position3D.Origin);
        // Room is unexplored by default

        // Act
        var symbol = service.GetRoomSymbol(room, isPlayerHere: false);

        // Assert
        symbol.Should().Be('?');
    }

    [Test]
    public void GetRoomSymbol_VisitedStandardRoom_ReturnsHash()
    {
        // Arrange
        var service = CreateService();
        var room = new Room("Test", "Test room", Position3D.Origin);
        room.MarkVisited();

        // Act
        var symbol = service.GetRoomSymbol(room, isPlayerHere: false);

        // Assert
        symbol.Should().Be('#');
    }

    [Test]
    public void GetRoomSymbol_VisitedTreasureRoom_ReturnsDollarSign()
    {
        // Arrange
        var service = CreateService();
        var room = new Room("Treasure", "Treasure room", Position3D.Origin);
        room.SetRoomType(RoomType.Treasure);
        room.MarkVisited();

        // Act
        var symbol = service.GetRoomSymbol(room, isPlayerHere: false);

        // Assert
        symbol.Should().Be('$');
    }

    [Test]
    public void GetRoomSymbol_AllRoomTypes_ReturnCorrectSymbols()
    {
        // Arrange
        var service = CreateService();

        var testCases = new (RoomType Type, char Expected)[]
        {
            (RoomType.Standard, '#'),
            (RoomType.Treasure, '$'),
            (RoomType.Trap, '!'),
            (RoomType.Boss, 'B'),
            (RoomType.Safe, '+'),
            (RoomType.Shrine, '*')
        };

        foreach (var (type, expected) in testCases)
        {
            var room = new Room("Test", "Test", Position3D.Origin);
            room.SetRoomType(type);
            room.MarkVisited();

            // Act
            var symbol = service.GetRoomSymbol(room, isPlayerHere: false);

            // Assert
            symbol.Should().Be(expected, $"because {type} should be '{expected}'");
        }
    }

    [Test]
    public void GetConnectionSymbol_HorizontalDirections_ReturnsHorizontalChar()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        service.GetConnectionSymbol(Direction.East, false).Should().Be('─');
        service.GetConnectionSymbol(Direction.West, false).Should().Be('─');
    }

    [Test]
    public void GetConnectionSymbol_VerticalDirections_ReturnsVerticalChar()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        service.GetConnectionSymbol(Direction.North, false).Should().Be('│');
        service.GetConnectionSymbol(Direction.South, false).Should().Be('│');
    }

    [Test]
    public void GetConnectionSymbol_UpDown_ReturnsStairSymbols()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        service.GetConnectionSymbol(Direction.Up, false).Should().Be('↑');
        service.GetConnectionSymbol(Direction.Down, false).Should().Be('↓');
    }

    [Test]
    public void GetConnectionSymbol_Hidden_ReturnsSpace()
    {
        // Arrange
        var service = CreateService();

        // Act
        var symbol = service.GetConnectionSymbol(Direction.North, isHidden: true);

        // Assert
        symbol.Should().Be(' ');
    }

    [Test]
    public void RenderLevel_SingleRoom_ContainsPlayerSymbol()
    {
        // Arrange
        var service = CreateService();
        var dungeon = CreateDungeonWithSingleRoom();
        var playerPos = Position3D.Origin;

        // Act
        var result = service.RenderLevel(dungeon, 0, playerPos);

        // Assert
        result.Should().Contain("@"); // Player symbol
        result.Should().Contain("Level 1"); // Header
    }

    [Test]
    public void RenderLevel_MultipleRooms_ShowsConnections()
    {
        // Arrange
        var service = CreateService();
        var dungeon = CreateDungeonWithConnectedRooms();
        var playerPos = Position3D.Origin;

        // Act
        var result = service.RenderLevel(dungeon, 0, playerPos);

        // Assert
        result.Should().Contain("─"); // Horizontal connection
        result.Should().Contain("│"); // Vertical connection
    }

    [Test]
    public void RenderLevel_IncludesLegend()
    {
        // Arrange
        var service = CreateService();
        var dungeon = CreateDungeonWithSingleRoom();
        var playerPos = Position3D.Origin;

        // Act
        var result = service.RenderLevel(dungeon, 0, playerPos);

        // Assert
        result.Should().Contain("Legend:");
        result.Should().Contain("@ You");
    }

    [Test]
    public void RenderLevel_IncludesStatistics()
    {
        // Arrange
        var service = CreateService();
        var dungeon = CreateDungeonWithSingleRoom();
        var playerPos = Position3D.Origin;

        // Act
        var result = service.RenderLevel(dungeon, 0, playerPos);

        // Assert
        result.Should().Contain("Rooms explored:");
    }

    [Test]
    public void RenderLevel_EmptyLevel_ReturnsMessage()
    {
        // Arrange
        var service = CreateService();
        var dungeon = CreateDungeonWithSingleRoom();
        var playerPos = Position3D.Origin;

        // Act
        var result = service.RenderLevel(dungeon, 99, playerPos); // Non-existent level

        // Assert
        result.Should().Contain("No rooms found on level 99");
    }

    [Test]
    public void RenderAllLevels_NoExploredLevels_ReturnsMessage()
    {
        // Arrange
        var service = CreateService();
        var dungeon = new Dungeon("Empty Dungeon");
        var room = new Room("Test", "Test", Position3D.Origin);
        dungeon.AddRoom(room); // Not visited
        var playerPos = Position3D.Origin;

        // Act
        var result = service.RenderAllLevels(dungeon, playerPos);

        // Assert
        result.Should().Contain("No explored levels to display");
    }

    [Test]
    public void RenderAllLevels_MultipleLevels_RendersAll()
    {
        // Arrange
        var service = CreateService();
        var dungeon = new Dungeon("Multi-Level Dungeon");

        var room1 = new Room("Room 1", "Level 0", new Position3D(0, 0, 0));
        var room2 = new Room("Room 2", "Level 1", new Position3D(0, 0, 1));

        dungeon.AddRoom(room1, isStartingRoom: true);
        dungeon.AddRoom(room2);

        room1.MarkVisited();
        room2.MarkVisited();

        var playerPos = Position3D.Origin;

        // Act
        var result = service.RenderAllLevels(dungeon, playerPos);

        // Assert
        result.Should().Contain("Level 1 (Z=0)");
        result.Should().Contain("Level 2 (Z=1)");
        result.Should().Contain("Current position:");
    }

    [Test]
    public void GetLegend_ContainsAllSymbols()
    {
        // Arrange
        var service = CreateService();

        // Act
        var legend = service.GetLegend();

        // Assert
        legend.Should().Contain("@");
        legend.Should().Contain("#");
        legend.Should().Contain("$");
        legend.Should().Contain("!");
        legend.Should().Contain("B");
        legend.Should().Contain("+");
        legend.Should().Contain("*");
        legend.Should().Contain("?");
        legend.Should().Contain("↑");
        legend.Should().Contain("↓");
    }
}
