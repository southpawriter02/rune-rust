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
/// Tests for the DungeonGenerator service.
/// Validates test map generation and room connectivity.
/// </summary>
public class DungeonGeneratorTests
{
    private readonly Mock<IRoomRepository> _mockRoomRepo;
    private readonly Mock<IEnvironmentPopulator> _mockEnvironmentPopulator;
    private readonly Mock<IRoomTemplateRepository> _mockRoomTemplateRepo;
    private readonly Mock<IBiomeDefinitionRepository> _mockBiomeDefinitionRepo;
    private readonly Mock<ITemplateRendererService> _mockTemplateRendererService;
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly Mock<ILogger<DungeonGenerator>> _mockLogger;
    private readonly DungeonGenerator _generator;
    private List<Room> _addedRooms = new();

    public DungeonGeneratorTests()
    {
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockEnvironmentPopulator = new Mock<IEnvironmentPopulator>();
        _mockRoomTemplateRepo = new Mock<IRoomTemplateRepository>();
        _mockBiomeDefinitionRepo = new Mock<IBiomeDefinitionRepository>();
        _mockTemplateRendererService = new Mock<ITemplateRendererService>();
        _mockDiceService = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<DungeonGenerator>>();
        _generator = new DungeonGenerator(
            _mockRoomRepo.Object,
            _mockEnvironmentPopulator.Object,
            _mockRoomTemplateRepo.Object,
            _mockBiomeDefinitionRepo.Object,
            _mockTemplateRendererService.Object,
            _mockDiceService.Object,
            _mockLogger.Object);

        // Capture rooms when AddRangeAsync is called
        _mockRoomRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Room>>()))
            .Callback<IEnumerable<Room>>(rooms => _addedRooms = rooms.ToList())
            .Returns(Task.CompletedTask);

        _mockRoomRepo.Setup(r => r.ClearAllRoomsAsync())
            .Returns(Task.CompletedTask);

        _mockRoomRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
    }

    #region GenerateTestMapAsync Tests

    [Fact]
    public async Task GenerateTestMapAsync_ClearsExistingRooms()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        _mockRoomRepo.Verify(r => r.ClearAllRoomsAsync(), Times.Once);
    }

    [Fact]
    public async Task GenerateTestMapAsync_CreatesAtLeastTwoRooms()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        _addedRooms.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GenerateTestMapAsync_CreatesFiveRooms()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        _addedRooms.Should().HaveCount(5);
    }

    [Fact]
    public async Task GenerateTestMapAsync_HasExactlyOneStartingRoom()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        _addedRooms.Count(r => r.IsStartingRoom).Should().Be(1);
    }

    [Fact]
    public async Task GenerateTestMapAsync_StartingRoomIsAtOrigin()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        var startingRoom = _addedRooms.Single(r => r.IsStartingRoom);
        startingRoom.Position.Should().Be(Coordinate.Origin);
    }

    [Fact]
    public async Task GenerateTestMapAsync_ReturnsStartingRoomId()
    {
        // Act
        var result = await _generator.GenerateTestMapAsync();

        // Assert
        var startingRoom = _addedRooms.Single(r => r.IsStartingRoom);
        result.Should().Be(startingRoom.Id);
    }

    [Fact]
    public async Task GenerateTestMapAsync_AllRoomsHaveUniqueIds()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        var ids = _addedRooms.Select(r => r.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GenerateTestMapAsync_AllRoomsHaveUniquePositions()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        var positions = _addedRooms.Select(r => r.Position).ToList();
        positions.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GenerateTestMapAsync_AllRoomsHaveNonEmptyNames()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        foreach (var room in _addedRooms)
        {
            room.Name.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GenerateTestMapAsync_AllRoomsHaveNonEmptyDescriptions()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        foreach (var room in _addedRooms)
        {
            room.Description.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GenerateTestMapAsync_StartingRoomHasMultipleExits()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        var startingRoom = _addedRooms.Single(r => r.IsStartingRoom);
        startingRoom.Exits.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GenerateTestMapAsync_ExitsAreBidirectional()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert - verify that for each exit, the target room has a return exit
        foreach (var room in _addedRooms)
        {
            foreach (var (direction, targetId) in room.Exits)
            {
                var targetRoom = _addedRooms.Single(r => r.Id == targetId);
                var oppositeDirection = DungeonGenerator.GetOppositeDirection(direction);
                targetRoom.Exits.Should().ContainKey(oppositeDirection);
                targetRoom.Exits[oppositeDirection].Should().Be(room.Id);
            }
        }
    }

    [Fact]
    public async Task GenerateTestMapAsync_ExitsPointToValidRooms()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        var allIds = _addedRooms.Select(r => r.Id).ToHashSet();

        foreach (var room in _addedRooms)
        {
            foreach (var targetId in room.Exits.Values)
            {
                allIds.Should().Contain(targetId);
            }
        }
    }

    [Fact]
    public async Task GenerateTestMapAsync_SavesChangesToDatabase()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        _mockRoomRepo.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Room>>()), Times.Once);
        _mockRoomRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GenerateTestMapAsync_IncludesVerticalRoom()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert - should have a room below origin (The Pit)
        var pitRoom = _addedRooms.FirstOrDefault(r => r.Position.Z < 0);
        pitRoom.Should().NotBeNull();
    }

    #endregion

    #region GetOppositeDirection Tests

    [Theory]
    [InlineData(Direction.North, Direction.South)]
    [InlineData(Direction.South, Direction.North)]
    [InlineData(Direction.East, Direction.West)]
    [InlineData(Direction.West, Direction.East)]
    [InlineData(Direction.Up, Direction.Down)]
    [InlineData(Direction.Down, Direction.Up)]
    public void GetOppositeDirection_ReturnsCorrectOpposite(Direction input, Direction expected)
    {
        // Act
        var result = DungeonGenerator.GetOppositeDirection(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetOppositeDirection_IsSymmetric()
    {
        // Arrange
        var allDirections = Enum.GetValues<Direction>();

        // Assert
        foreach (var direction in allDirections)
        {
            var opposite = DungeonGenerator.GetOppositeDirection(direction);
            var backToOriginal = DungeonGenerator.GetOppositeDirection(opposite);
            backToOriginal.Should().Be(direction);
        }
    }

    #endregion

    #region Expected Room Layout Tests

    [Fact]
    public async Task GenerateTestMapAsync_EntryHallIsNamed()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        _addedRooms.Should().Contain(r => r.Name == "Entry Hall");
    }

    [Fact]
    public async Task GenerateTestMapAsync_RustedCorridorIsNorthOfEntry()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        var corridor = _addedRooms.FirstOrDefault(r => r.Name == "Rusted Corridor");
        corridor.Should().NotBeNull();
        corridor!.Position.Should().Be(new Coordinate(0, 1, 0));
    }

    [Fact]
    public async Task GenerateTestMapAsync_ThePitIsBelowEntry()
    {
        // Act
        await _generator.GenerateTestMapAsync();

        // Assert
        var pit = _addedRooms.FirstOrDefault(r => r.Name == "The Pit");
        pit.Should().NotBeNull();
        pit!.Position.Should().Be(new Coordinate(0, 0, -1));
    }

    #endregion
}
