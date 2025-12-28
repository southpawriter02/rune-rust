using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for MapExportService (v0.3.20b).
/// Validates ASCII map generation, symbol resolution, and content formatting.
/// </summary>
public class MapExportServiceTests
{
    private readonly Mock<ILogger<MapExportService>> _mockLogger;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly MapExportService _sut;

    public MapExportServiceTests()
    {
        _mockLogger = new Mock<ILogger<MapExportService>>();
        _mockRoomRepository = new Mock<IRoomRepository>();

        _sut = new MapExportService(
            _mockLogger.Object,
            _mockRoomRepository.Object);
    }

    #region ResolveSymbol Tests

    [Fact]
    public void ResolveSymbol_NullRoom_ReturnsDot()
    {
        // Act
        var symbol = _sut.ResolveSymbol(null, false, false);

        // Assert
        symbol.Should().Be('.');
    }

    [Fact]
    public void ResolveSymbol_PlayerPosition_ReturnsAtSign()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: true, hasNote: false);

        // Assert
        symbol.Should().Be('@');
    }

    [Fact]
    public void ResolveSymbol_PlayerPositionWithNote_ReturnsAtSign()
    {
        // Arrange - Player position should override note symbol
        var room = CreateRoom(0, 0, 0);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: true, hasNote: true);

        // Assert
        symbol.Should().Be('@');
    }

    [Fact]
    public void ResolveSymbol_RoomWithNote_ReturnsExclamation()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: false, hasNote: true);

        // Assert
        symbol.Should().Be('!');
    }

    [Fact]
    public void ResolveSymbol_BossLair_ReturnsX()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0, RoomFeature.BossLair);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: false, hasNote: false);

        // Assert
        symbol.Should().Be('X');
    }

    [Fact]
    public void ResolveSymbol_Settlement_ReturnsDollar()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0, RoomFeature.Settlement);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: false, hasNote: false);

        // Assert
        symbol.Should().Be('$');
    }

    [Fact]
    public void ResolveSymbol_RunicAnchor_ReturnsAsterisk()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0, RoomFeature.RunicAnchor);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: false, hasNote: false);

        // Assert
        symbol.Should().Be('*');
    }

    [Fact]
    public void ResolveSymbol_StairsUp_ReturnsCaret()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0, RoomFeature.StairsUp);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: false, hasNote: false);

        // Assert
        symbol.Should().Be('^');
    }

    [Fact]
    public void ResolveSymbol_StairsDown_ReturnsLowercaseV()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0, RoomFeature.StairsDown);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: false, hasNote: false);

        // Assert
        symbol.Should().Be('v');
    }

    [Fact]
    public void ResolveSymbol_Workbench_ReturnsPlus()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0, RoomFeature.Workbench);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: false, hasNote: false);

        // Assert
        symbol.Should().Be('+');
    }

    [Fact]
    public void ResolveSymbol_AlchemyTable_ReturnsPlus()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0, RoomFeature.AlchemyTable);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: false, hasNote: false);

        // Assert
        symbol.Should().Be('+');
    }

    [Fact]
    public void ResolveSymbol_StandardRoom_ReturnsHash()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: false, hasNote: false);

        // Assert
        symbol.Should().Be('#');
    }

    #endregion

    #region GenerateMapContent Tests

    [Fact]
    public void GenerateMapContent_EmptyRoomList_ReturnsHeaderWithNoRoomsMessage()
    {
        // Arrange
        var rooms = new List<Room>();
        var playerPos = new Coordinate(0, 0, 0);
        var userNotes = new Dictionary<Guid, string>();

        // Act
        var content = _sut.GenerateMapContent("TestCharacter", playerPos, rooms, userNotes);

        // Assert
        content.Should().Contain("ATLAS EXPORT - TestCharacter");
        content.Should().Contain("Rooms Explored: 0");
        content.Should().Contain("No rooms have been explored yet.");
    }

    [Fact]
    public void GenerateMapContent_SingleRoom_RendersCorrectly()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0);
        var rooms = new List<Room> { room };
        var playerPos = new Coordinate(0, 0, 0);
        var userNotes = new Dictionary<Guid, string>();

        // Act
        var content = _sut.GenerateMapContent("TestCharacter", playerPos, rooms, userNotes);

        // Assert
        content.Should().Contain("[DEPTH Z: 0]");
        content.Should().Contain("@"); // Player position
        content.Should().Contain("LEGEND:");
    }

    [Fact]
    public void GenerateMapContent_MultipleDepthLevels_GroupsCorrectly()
    {
        // Arrange
        var room0 = CreateRoom(0, 0, 0);
        var room1 = CreateRoom(0, 0, -1);
        var rooms = new List<Room> { room0, room1 };
        var playerPos = new Coordinate(0, 0, 0);
        var userNotes = new Dictionary<Guid, string>();

        // Act
        var content = _sut.GenerateMapContent("TestCharacter", playerPos, rooms, userNotes);

        // Assert
        content.Should().Contain("[DEPTH Z: -1]");
        content.Should().Contain("[DEPTH Z: 0]");
    }

    [Fact]
    public void GenerateMapContent_WithUserNotes_IncludesNotesSection()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0);
        var rooms = new List<Room> { room };
        var playerPos = new Coordinate(1, 1, 0); // Not at room position
        var userNotes = new Dictionary<Guid, string>
        {
            { room.Id, "Test note content" }
        };

        // Act
        var content = _sut.GenerateMapContent("TestCharacter", playerPos, rooms, userNotes);

        // Assert
        content.Should().Contain("NOTES:");
        content.Should().Contain("Test note content");
        content.Should().Contain("!"); // Note symbol in map
    }

    [Fact]
    public void GenerateMapContent_3x3Grid_RendersAllRooms()
    {
        // Arrange
        var rooms = new List<Room>();
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                rooms.Add(CreateRoom(x, y, 0));
            }
        }

        var playerPos = new Coordinate(1, 1, 0);
        var userNotes = new Dictionary<Guid, string>();

        // Act
        var content = _sut.GenerateMapContent("TestCharacter", playerPos, rooms, userNotes);

        // Assert
        content.Should().Contain("Rooms Explored: 9");
        // Verify player symbol is present
        content.Should().Contain("@");
        // Count # symbols: 8 in map grid + 1 in legend = 9 total
        var hashCount = content.Count(c => c == '#');
        hashCount.Should().Be(9);
    }

    [Fact]
    public void GenerateMapContent_IncludesLegend()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0);
        var rooms = new List<Room> { room };
        var playerPos = new Coordinate(0, 0, 0);
        var userNotes = new Dictionary<Guid, string>();

        // Act
        var content = _sut.GenerateMapContent("TestCharacter", playerPos, rooms, userNotes);

        // Assert
        content.Should().Contain("LEGEND:");
        content.Should().Contain("@  : You are here");
        content.Should().Contain("!  : Note attached");
        content.Should().Contain("X  : Boss Lair");
        content.Should().Contain("$  : Settlement");
        content.Should().Contain("*  : Runic Anchor");
        content.Should().Contain("#  : Explored Room");
    }

    [Fact]
    public void GenerateMapContent_IncludesVersionFooter()
    {
        // Arrange
        var room = CreateRoom(0, 0, 0);
        var rooms = new List<Room> { room };
        var playerPos = new Coordinate(0, 0, 0);
        var userNotes = new Dictionary<Guid, string>();

        // Act
        var content = _sut.GenerateMapContent("TestCharacter", playerPos, rooms, userNotes);

        // Assert
        content.Should().Contain("Generated by Rune & Rust - The Atlas (v0.3.20b)");
    }

    #endregion

    #region Symbol Priority Tests

    [Fact]
    public void ResolveSymbol_NoteOverridesBossLair()
    {
        // Arrange - Note should take priority over boss lair
        var room = CreateRoom(0, 0, 0, RoomFeature.BossLair);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: false, hasNote: true);

        // Assert
        symbol.Should().Be('!');
    }

    [Fact]
    public void ResolveSymbol_BossLairOverridesSettlement()
    {
        // Arrange - If a room somehow has both, boss lair wins
        var room = CreateRoom(0, 0, 0, RoomFeature.BossLair);
        room.Features.Add(RoomFeature.Settlement);

        // Act
        var symbol = _sut.ResolveSymbol(room, isPlayerPosition: false, hasNote: false);

        // Assert
        symbol.Should().Be('X');
    }

    #endregion

    #region Helper Methods

    private static Room CreateRoom(int x, int y, int z, RoomFeature? feature = null)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = $"Room {x},{y},{z}",
            Description = "A test room.",
            Position = new Coordinate(x, y, z),
            BiomeType = BiomeType.Ruin,
            DangerLevel = DangerLevel.Safe
        };

        if (feature.HasValue)
        {
            room.Features.Add(feature.Value);
        }

        return room;
    }

    #endregion
}
