using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the NavigationService.
/// Validates movement logic, room descriptions, and exit handling.
/// </summary>
public class NavigationServiceTests
{
    private readonly Mock<IRoomRepository> _mockRoomRepo;
    private readonly Mock<IHazardService> _mockHazardService;
    private readonly Mock<IInputHandler> _mockInputHandler;
    private readonly Mock<ILogger<NavigationService>> _mockLogger;
    private readonly GameState _gameState;
    private readonly NavigationService _navigationService;

    public NavigationServiceTests()
    {
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockHazardService = new Mock<IHazardService>();
        _mockInputHandler = new Mock<IInputHandler>();
        _mockLogger = new Mock<ILogger<NavigationService>>();
        _gameState = new GameState();
        _navigationService = new NavigationService(
            _gameState,
            _mockRoomRepo.Object,
            _mockHazardService.Object,
            _mockInputHandler.Object,
            _mockLogger.Object);

        // Default setup: no hazards trigger
        _mockHazardService.Setup(h => h.TriggerOnRoomEnterAsync(It.IsAny<Room>(), It.IsAny<Combatant?>()))
            .ReturnsAsync(new List<HazardResult>());
    }

    #region MoveAsync Tests

    [Fact]
    public async Task MoveAsync_NoCurrentRoom_ReturnsErrorMessage()
    {
        // Arrange
        _gameState.CurrentRoomId = null;

        // Act
        var result = await _navigationService.MoveAsync(Direction.North);

        // Assert
        result.Should().Contain("void");
    }

    [Fact]
    public async Task MoveAsync_CurrentRoomNotInDatabase_ReturnsErrorMessage()
    {
        // Arrange
        _gameState.CurrentRoomId = Guid.NewGuid();
        _mockRoomRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Room?)null);

        // Act
        var result = await _navigationService.MoveAsync(Direction.North);

        // Assert
        result.Should().Contain("no longer exists");
    }

    [Fact]
    public async Task MoveAsync_NoExitInDirection_ReturnsCannotGoMessage()
    {
        // Arrange
        var currentRoomId = Guid.NewGuid();
        var currentRoom = new Room
        {
            Id = currentRoomId,
            Name = "Entry Hall",
            Exits = new Dictionary<Direction, Guid>() // No exits
        };

        _gameState.CurrentRoomId = currentRoomId;
        _mockRoomRepo.Setup(r => r.GetByIdAsync(currentRoomId))
            .ReturnsAsync(currentRoom);

        // Act
        var result = await _navigationService.MoveAsync(Direction.North);

        // Assert
        result.Should().Contain("cannot go north");
    }

    [Fact]
    public async Task MoveAsync_ValidExit_UpdatesCurrentRoomId()
    {
        // Arrange
        var currentRoomId = Guid.NewGuid();
        var nextRoomId = Guid.NewGuid();

        var currentRoom = new Room
        {
            Id = currentRoomId,
            Name = "Entry Hall",
            Exits = new Dictionary<Direction, Guid>
            {
                { Direction.North, nextRoomId }
            }
        };

        var nextRoom = new Room
        {
            Id = nextRoomId,
            Name = "Rusted Corridor",
            Description = "Pipes line the walls."
        };

        _gameState.CurrentRoomId = currentRoomId;
        _gameState.TurnCount = 5;

        _mockRoomRepo.Setup(r => r.GetByIdAsync(currentRoomId))
            .ReturnsAsync(currentRoom);
        _mockRoomRepo.Setup(r => r.GetByIdAsync(nextRoomId))
            .ReturnsAsync(nextRoom);

        // Act
        await _navigationService.MoveAsync(Direction.North);

        // Assert
        _gameState.CurrentRoomId.Should().Be(nextRoomId);
    }

    [Fact]
    public async Task MoveAsync_ValidExit_IncrementsTurnCount()
    {
        // Arrange
        var currentRoomId = Guid.NewGuid();
        var nextRoomId = Guid.NewGuid();

        var currentRoom = new Room
        {
            Id = currentRoomId,
            Exits = new Dictionary<Direction, Guid> { { Direction.North, nextRoomId } }
        };

        var nextRoom = new Room
        {
            Id = nextRoomId,
            Name = "Next Room"
        };

        _gameState.CurrentRoomId = currentRoomId;
        _gameState.TurnCount = 5;

        _mockRoomRepo.Setup(r => r.GetByIdAsync(currentRoomId)).ReturnsAsync(currentRoom);
        _mockRoomRepo.Setup(r => r.GetByIdAsync(nextRoomId)).ReturnsAsync(nextRoom);

        // Act
        await _navigationService.MoveAsync(Direction.North);

        // Assert
        _gameState.TurnCount.Should().Be(6);
    }

    [Fact]
    public async Task MoveAsync_ValidExit_ReturnsRoomDescription()
    {
        // Arrange
        var currentRoomId = Guid.NewGuid();
        var nextRoomId = Guid.NewGuid();

        var currentRoom = new Room
        {
            Id = currentRoomId,
            Exits = new Dictionary<Direction, Guid> { { Direction.North, nextRoomId } }
        };

        var nextRoom = new Room
        {
            Id = nextRoomId,
            Name = "Rusted Corridor",
            Description = "Corroded pipes line the walls."
        };

        _gameState.CurrentRoomId = currentRoomId;

        _mockRoomRepo.Setup(r => r.GetByIdAsync(currentRoomId)).ReturnsAsync(currentRoom);
        _mockRoomRepo.Setup(r => r.GetByIdAsync(nextRoomId)).ReturnsAsync(nextRoom);

        // Act
        var result = await _navigationService.MoveAsync(Direction.North);

        // Assert
        result.Should().Contain("move north");
        result.Should().Contain("[Rusted Corridor]");
        result.Should().Contain("Corroded pipes line the walls.");
    }

    [Fact]
    public async Task MoveAsync_ExitLeadsToNonexistentRoom_ReturnsErrorMessage()
    {
        // Arrange
        var currentRoomId = Guid.NewGuid();
        var brokenExitId = Guid.NewGuid();

        var currentRoom = new Room
        {
            Id = currentRoomId,
            Exits = new Dictionary<Direction, Guid> { { Direction.North, brokenExitId } }
        };

        _gameState.CurrentRoomId = currentRoomId;

        _mockRoomRepo.Setup(r => r.GetByIdAsync(currentRoomId)).ReturnsAsync(currentRoom);
        _mockRoomRepo.Setup(r => r.GetByIdAsync(brokenExitId)).ReturnsAsync((Room?)null);

        // Act
        var result = await _navigationService.MoveAsync(Direction.North);

        // Assert
        result.Should().Contain("nowhere");
    }

    [Theory]
    [InlineData(Direction.North, "north")]
    [InlineData(Direction.South, "south")]
    [InlineData(Direction.East, "east")]
    [InlineData(Direction.West, "west")]
    [InlineData(Direction.Up, "up")]
    [InlineData(Direction.Down, "down")]
    public async Task MoveAsync_AllDirections_FormatsDirectionCorrectly(Direction direction, string expectedText)
    {
        // Arrange
        var currentRoomId = Guid.NewGuid();
        var nextRoomId = Guid.NewGuid();

        var currentRoom = new Room
        {
            Id = currentRoomId,
            Exits = new Dictionary<Direction, Guid> { { direction, nextRoomId } }
        };

        var nextRoom = new Room { Id = nextRoomId, Name = "Test Room" };

        _gameState.CurrentRoomId = currentRoomId;

        _mockRoomRepo.Setup(r => r.GetByIdAsync(currentRoomId)).ReturnsAsync(currentRoom);
        _mockRoomRepo.Setup(r => r.GetByIdAsync(nextRoomId)).ReturnsAsync(nextRoom);

        // Act
        var result = await _navigationService.MoveAsync(direction);

        // Assert
        result.Should().Contain(expectedText);
    }

    #endregion

    #region LookAsync Tests

    [Fact]
    public async Task LookAsync_NoCurrentRoom_ReturnsVoidMessage()
    {
        // Arrange
        _gameState.CurrentRoomId = null;

        // Act
        var result = await _navigationService.LookAsync();

        // Assert
        result.Should().Contain("void");
    }

    [Fact]
    public async Task LookAsync_ValidRoom_ReturnsFormattedDescription()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room
        {
            Id = roomId,
            Name = "Entry Hall",
            Description = "A cold, metallic chamber.",
            Exits = new Dictionary<Direction, Guid>
            {
                { Direction.North, Guid.NewGuid() },
                { Direction.East, Guid.NewGuid() }
            }
        };

        _gameState.CurrentRoomId = roomId;
        _mockRoomRepo.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        // Act
        var result = await _navigationService.LookAsync();

        // Assert
        result.Should().Contain("[Entry Hall]");
        result.Should().Contain("A cold, metallic chamber.");
        result.Should().Contain("Exits:");
    }

    [Fact]
    public async Task LookAsync_RoomWithNoExits_ShowsNoObviousExits()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room
        {
            Id = roomId,
            Name = "Dead End",
            Description = "A wall blocks your path.",
            Exits = new Dictionary<Direction, Guid>()
        };

        _gameState.CurrentRoomId = roomId;
        _mockRoomRepo.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        // Act
        var result = await _navigationService.LookAsync();

        // Assert
        result.Should().Contain("no obvious exits");
    }

    #endregion

    #region GetCurrentRoomAsync Tests

    [Fact]
    public async Task GetCurrentRoomAsync_NoCurrentRoom_ReturnsNull()
    {
        // Arrange
        _gameState.CurrentRoomId = null;

        // Act
        var result = await _navigationService.GetCurrentRoomAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentRoomAsync_ValidRoom_ReturnsRoom()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room { Id = roomId, Name = "Test Room" };

        _gameState.CurrentRoomId = roomId;
        _mockRoomRepo.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        // Act
        var result = await _navigationService.GetCurrentRoomAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Room");
    }

    #endregion

    #region GetAvailableExitsAsync Tests

    [Fact]
    public async Task GetAvailableExitsAsync_NoCurrentRoom_ReturnsEmpty()
    {
        // Arrange
        _gameState.CurrentRoomId = null;

        // Act
        var result = await _navigationService.GetAvailableExitsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailableExitsAsync_RoomWithExits_ReturnsDirections()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room
        {
            Id = roomId,
            Exits = new Dictionary<Direction, Guid>
            {
                { Direction.North, Guid.NewGuid() },
                { Direction.Down, Guid.NewGuid() }
            }
        };

        _gameState.CurrentRoomId = roomId;
        _mockRoomRepo.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        // Act
        var result = await _navigationService.GetAvailableExitsAsync();

        // Assert
        result.Should().Contain(Direction.North);
        result.Should().Contain(Direction.Down);
        result.Should().HaveCount(2);
    }

    #endregion
}
