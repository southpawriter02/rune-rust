using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;

using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for rest and camp command integration with the CommandParser.
/// These tests validate the full flow with mocked services (v0.3.2c).
/// </summary>
public class CommandParserRestTests
{
    private readonly Mock<ILogger<CommandParser>> _mockLogger;
    private readonly Mock<IInputHandler> _mockInputHandler;
    private readonly Mock<IRestService> _mockRestService;
    private readonly Mock<IRestScreenRenderer> _mockRestRenderer;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly GameState _state;
    private readonly CommandParser _sut;

    public CommandParserRestTests()
    {
        _mockLogger = new Mock<ILogger<CommandParser>>();
        _mockInputHandler = new Mock<IInputHandler>();
        _mockRestService = new Mock<IRestService>();
        _mockRestRenderer = new Mock<IRestScreenRenderer>();
        _mockRoomRepository = new Mock<IRoomRepository>();
        _state = new GameState();

        _sut = new CommandParser(
            _mockLogger.Object,
            _mockInputHandler.Object,
            _state,
            journalService: null,
            combatService: null,
            victoryRenderer: null,
            restService: _mockRestService.Object,
            restRenderer: _mockRestRenderer.Object,
            roomRepository: _mockRoomRepository.Object);
    }

    #region Rest Command - Sanctuary Mode

    [Fact]
    public async Task Rest_AtRunicAnchor_UsesSanctuaryType()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room
        {
            Id = roomId,
            Name = "Runic Anchor Chamber",
            Features = new List<RoomFeature> { RoomFeature.RunicAnchor }
        };
        var character = new CharacterEntity { Name = "Test Hero" };
        var restResult = new RestResult(10, 20, 5, false, false);

        _state.Phase = GamePhase.Exploration;
        _state.CurrentRoomId = roomId;
        _state.CurrentCharacter = character;

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRestService.Setup(r => r.PerformRestAsync(character, RestType.Sanctuary, room))
            .ReturnsAsync(restResult);

        // Act
        await _sut.ParseAndExecuteAsync("rest", _state);

        // Assert
        _mockRestService.Verify(
            r => r.PerformRestAsync(character, RestType.Sanctuary, room),
            Times.Once);
        _mockRestRenderer.Verify(r => r.Render(restResult), Times.Once);
    }

    [Fact]
    public async Task Rest_WithoutAnchor_UsesWildernessType()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room
        {
            Id = roomId,
            Name = "Empty Corridor",
            Features = new List<RoomFeature>() // No RunicAnchor
        };
        var character = new CharacterEntity { Name = "Test Hero" };
        var restResult = new RestResult(5, 10, 2, true, false);

        _state.Phase = GamePhase.Exploration;
        _state.CurrentRoomId = roomId;
        _state.CurrentCharacter = character;

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRestService.Setup(r => r.PerformRestAsync(character, RestType.Wilderness, room))
            .ReturnsAsync(restResult);

        // Act
        await _sut.ParseAndExecuteAsync("rest", _state);

        // Assert
        _mockRestService.Verify(
            r => r.PerformRestAsync(character, RestType.Wilderness, room),
            Times.Once);
    }

    #endregion

    #region Camp Command - Always Wilderness

    [Fact]
    public async Task Camp_AlwaysUsesWildernessType()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room
        {
            Id = roomId,
            Name = "Runic Anchor Chamber",
            Features = new List<RoomFeature> { RoomFeature.RunicAnchor } // Has anchor but camp ignores it
        };
        var character = new CharacterEntity { Name = "Test Hero" };
        var restResult = new RestResult(5, 10, 2, true, false);

        _state.Phase = GamePhase.Exploration;
        _state.CurrentRoomId = roomId;
        _state.CurrentCharacter = character;

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRestService.Setup(r => r.PerformRestAsync(character, RestType.Wilderness, room))
            .ReturnsAsync(restResult);

        // Act
        await _sut.ParseAndExecuteAsync("camp", _state);

        // Assert - Should use Wilderness even though anchor is present
        _mockRestService.Verify(
            r => r.PerformRestAsync(character, RestType.Wilderness, room),
            Times.Once);
    }

    #endregion

    #region Ambush Handling

    [Fact]
    public async Task Camp_AmbushTriggered_SetsGamePhaseCombat()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room { Id = roomId, Name = "Dangerous Area" };
        var character = new CharacterEntity { Name = "Test Hero" };
        var encounter = new EncounterDefinition(new[] { "wolf" }, 1.0f, true);
        var ambushResult = new AmbushResult(true, "Wolves attack!", 30, 10, 20, 15, 1, encounter);
        var restResult = new RestResult(0, 0, 0, false, false, 480, true, ambushResult);

        _state.Phase = GamePhase.Exploration;
        _state.CurrentRoomId = roomId;
        _state.CurrentCharacter = character;

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRestService.Setup(r => r.PerformRestAsync(character, RestType.Wilderness, room))
            .ReturnsAsync(restResult);

        // Act
        await _sut.ParseAndExecuteAsync("camp", _state);

        // Assert
        _state.Phase.Should().Be(GamePhase.Combat);
    }

    [Fact]
    public async Task Camp_AmbushTriggered_StoresPendingEncounter()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room { Id = roomId, Name = "Dangerous Area" };
        var character = new CharacterEntity { Name = "Test Hero" };
        var encounter = new EncounterDefinition(new[] { "wolf", "wolf" }, 2.0f, true);
        var ambushResult = new AmbushResult(true, "Wolves attack!", 30, 10, 20, 15, 1, encounter);
        var restResult = new RestResult(0, 0, 0, false, false, 480, true, ambushResult);

        _state.Phase = GamePhase.Exploration;
        _state.CurrentRoomId = roomId;
        _state.CurrentCharacter = character;

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRestService.Setup(r => r.PerformRestAsync(character, RestType.Wilderness, room))
            .ReturnsAsync(restResult);

        // Act
        await _sut.ParseAndExecuteAsync("camp", _state);

        // Assert
        _state.PendingEncounter.Should().NotBeNull();
        _state.PendingEncounter.Should().Be(encounter);
    }

    [Fact]
    public async Task Camp_AmbushTriggered_RendersAmbushWarning()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room { Id = roomId, Name = "Dangerous Area" };
        var character = new CharacterEntity { Name = "Test Hero" };
        var encounter = new EncounterDefinition(new[] { "wolf" }, 1.0f, true);
        var ambushResult = new AmbushResult(true, "Wolves attack!", 30, 10, 20, 15, 1, encounter);
        var restResult = new RestResult(0, 0, 0, false, false, 480, true, ambushResult);

        _state.Phase = GamePhase.Exploration;
        _state.CurrentRoomId = roomId;
        _state.CurrentCharacter = character;

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRestService.Setup(r => r.PerformRestAsync(character, RestType.Wilderness, room))
            .ReturnsAsync(restResult);

        // Act
        await _sut.ParseAndExecuteAsync("camp", _state);

        // Assert
        _mockRestRenderer.Verify(r => r.RenderAmbushWarning(ambushResult), Times.Once);
        _mockRestRenderer.Verify(r => r.Render(It.IsAny<RestResult>()), Times.Never);
    }

    #endregion

    #region Successful Rest

    [Fact]
    public async Task Camp_NoAmbush_RendersCalled()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room { Id = roomId, Name = "Safe Area" };
        var character = new CharacterEntity { Name = "Test Hero" };
        var restResult = new RestResult(10, 20, 5, true, false);

        _state.Phase = GamePhase.Exploration;
        _state.CurrentRoomId = roomId;
        _state.CurrentCharacter = character;

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRestService.Setup(r => r.PerformRestAsync(character, RestType.Wilderness, room))
            .ReturnsAsync(restResult);

        // Act
        await _sut.ParseAndExecuteAsync("camp", _state);

        // Assert
        _mockRestRenderer.Verify(r => r.Render(restResult), Times.Once);
        _mockRestRenderer.Verify(r => r.RenderAmbushWarning(It.IsAny<AmbushResult>()), Times.Never);
    }

    [Fact]
    public async Task Rest_IncrementsTurnCount()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room { Id = roomId, Name = "Safe Area" };
        var character = new CharacterEntity { Name = "Test Hero" };
        var restResult = new RestResult(10, 20, 5, true, false);

        _state.Phase = GamePhase.Exploration;
        _state.CurrentRoomId = roomId;
        _state.CurrentCharacter = character;
        _state.TurnCount = 5;

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRestService.Setup(r => r.PerformRestAsync(character, RestType.Wilderness, room))
            .ReturnsAsync(restResult);

        // Act
        await _sut.ParseAndExecuteAsync("rest", _state);

        // Assert
        _state.TurnCount.Should().Be(6);
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task Rest_WithoutCharacter_DisplaysError()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        _state.Phase = GamePhase.Exploration;
        _state.CurrentRoomId = roomId;
        _state.CurrentCharacter = null;

        // Act
        await _sut.ParseAndExecuteAsync("rest", _state);

        // Assert
        _mockInputHandler.Verify(h => h.DisplayError(It.Is<string>(s => s.Contains("character"))), Times.Once);
    }

    [Fact]
    public async Task Rest_WithoutRoom_DisplaysError()
    {
        // Arrange
        var character = new CharacterEntity { Name = "Test Hero" };
        _state.Phase = GamePhase.Exploration;
        _state.CurrentRoomId = null;
        _state.CurrentCharacter = character;

        // Act
        await _sut.ParseAndExecuteAsync("rest", _state);

        // Assert
        _mockInputHandler.Verify(h => h.DisplayError(It.Is<string>(s => s.Contains("nowhere"))), Times.Once);
    }

    #endregion
}
