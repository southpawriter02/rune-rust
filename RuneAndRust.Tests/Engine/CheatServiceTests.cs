using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;
using GameState = RuneAndRust.Core.Models.GameState;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for CheatService (v0.3.17b).
/// Validates god mode, full heal, teleport, and map reveal operations.
/// </summary>
public class CheatServiceTests
{
    private readonly CheatService _sut;
    private readonly GameState _gameState;
    private readonly IRoomRepository _mockRoomRepo;
    private readonly ILogger<CheatService> _mockLogger;

    public CheatServiceTests()
    {
        _gameState = new GameState();
        _mockRoomRepo = Substitute.For<IRoomRepository>();
        _mockLogger = Substitute.For<ILogger<CheatService>>();
        _sut = new CheatService(_gameState, _mockRoomRepo, _mockLogger);
    }

    #region ToggleGodMode Tests

    [Fact]
    public void ToggleGodMode_FlipsState_FalseToTrue()
    {
        // Arrange
        _gameState.IsGodMode.Should().BeFalse("initial state should be false");

        // Act
        var result = _sut.ToggleGodMode();

        // Assert
        result.Should().BeTrue();
        _gameState.IsGodMode.Should().BeTrue();
    }

    [Fact]
    public void ToggleGodMode_FlipsState_TrueToFalse()
    {
        // Arrange
        _gameState.IsGodMode = true;

        // Act
        var result = _sut.ToggleGodMode();

        // Assert
        result.Should().BeFalse();
        _gameState.IsGodMode.Should().BeFalse();
    }

    [Fact]
    public void ToggleGodMode_ReturnsNewState()
    {
        // Act & Assert
        _sut.ToggleGodMode().Should().BeTrue("first toggle should return true");
        _sut.ToggleGodMode().Should().BeFalse("second toggle should return false");
        _sut.ToggleGodMode().Should().BeTrue("third toggle should return true");
    }

    #endregion

    #region FullHeal Tests

    [Fact]
    public void FullHeal_RestoresHP()
    {
        // Arrange
        var character = CreateDamagedCharacter();
        _gameState.CurrentCharacter = character;

        // Act
        var result = _sut.FullHeal();

        // Assert
        result.Should().BeTrue();
        character.CurrentHP.Should().Be(character.MaxHP);
    }

    [Fact]
    public void FullHeal_RestoresStamina()
    {
        // Arrange
        var character = CreateDamagedCharacter();
        _gameState.CurrentCharacter = character;

        // Act
        _sut.FullHeal();

        // Assert
        character.CurrentStamina.Should().Be(character.MaxStamina);
    }

    [Fact]
    public void FullHeal_RestoresAP()
    {
        // Arrange
        var character = CreateDamagedCharacter();
        character.MaxAp = 10;
        character.CurrentAp = 2;
        _gameState.CurrentCharacter = character;

        // Act
        _sut.FullHeal();

        // Assert
        character.CurrentAp.Should().Be(character.MaxAp);
    }

    [Fact]
    public void FullHeal_ClearsStress()
    {
        // Arrange
        var character = CreateDamagedCharacter();
        character.PsychicStress = 75;
        _gameState.CurrentCharacter = character;

        // Act
        _sut.FullHeal();

        // Assert
        character.PsychicStress.Should().Be(0);
    }

    [Fact]
    public void FullHeal_ClearsStatusEffects()
    {
        // Arrange
        var character = CreateDamagedCharacter();
        character.ActiveStatusEffects.Add(StatusEffectType.Bleeding);
        character.ActiveStatusEffects.Add(StatusEffectType.Poisoned);
        _gameState.CurrentCharacter = character;

        // Act
        _sut.FullHeal();

        // Assert
        character.ActiveStatusEffects.Should().BeEmpty();
    }

    [Fact]
    public void FullHeal_ReturnsFalse_WhenNoCharacter()
    {
        // Arrange
        _gameState.CurrentCharacter = null;

        // Act
        var result = _sut.FullHeal();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region TeleportAsync Tests

    [Fact]
    public async Task TeleportAsync_ByGuid_TeleportsToRoom()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room { Id = roomId, Name = "Test Chamber" };
        _mockRoomRepo.GetByIdAsync(roomId).Returns(room);

        // Act
        var result = await _sut.TeleportAsync(roomId.ToString());

        // Assert
        result.Should().Be("Test Chamber");
        _gameState.CurrentRoomId.Should().Be(roomId);
        _gameState.VisitedRoomIds.Should().Contain(roomId);
    }

    [Fact]
    public async Task TeleportAsync_ByName_TeleportsToMatchingRoom()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room { Id = roomId, Name = "Ancient Hall" };
        _mockRoomRepo.GetAllRoomsAsync().Returns(new List<Room> { room });

        // Act
        var result = await _sut.TeleportAsync("ancient");

        // Assert
        result.Should().Be("Ancient Hall");
        _gameState.CurrentRoomId.Should().Be(roomId);
    }

    [Fact]
    public async Task TeleportAsync_ReturnsNull_WhenRoomNotFound()
    {
        // Arrange
        _mockRoomRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((Room?)null);
        _mockRoomRepo.GetAllRoomsAsync().Returns(new List<Room>());

        // Act
        var result = await _sut.TeleportAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region RevealMapAsync Tests

    [Fact]
    public async Task RevealMapAsync_AddsAllRoomsToVisited()
    {
        // Arrange
        var room1 = new Room { Id = Guid.NewGuid(), Name = "Room 1" };
        var room2 = new Room { Id = Guid.NewGuid(), Name = "Room 2" };
        var room3 = new Room { Id = Guid.NewGuid(), Name = "Room 3" };
        _mockRoomRepo.GetAllRoomsAsync().Returns(new List<Room> { room1, room2, room3 });

        // Act
        await _sut.RevealMapAsync();

        // Assert
        _gameState.VisitedRoomIds.Should().Contain(room1.Id);
        _gameState.VisitedRoomIds.Should().Contain(room2.Id);
        _gameState.VisitedRoomIds.Should().Contain(room3.Id);
    }

    [Fact]
    public async Task RevealMapAsync_ReturnsCount_OfNewlyRevealed()
    {
        // Arrange
        var room1 = new Room { Id = Guid.NewGuid(), Name = "Room 1" };
        var room2 = new Room { Id = Guid.NewGuid(), Name = "Room 2" };
        var room3 = new Room { Id = Guid.NewGuid(), Name = "Room 3" };
        _mockRoomRepo.GetAllRoomsAsync().Returns(new List<Room> { room1, room2, room3 });

        // Pre-visit one room
        _gameState.VisitedRoomIds.Add(room1.Id);

        // Act
        var count = await _sut.RevealMapAsync();

        // Assert
        count.Should().Be(2, "only 2 rooms were newly revealed");
    }

    [Fact]
    public async Task RevealMapAsync_ReturnsZero_WhenAllAlreadyVisited()
    {
        // Arrange
        var room1 = new Room { Id = Guid.NewGuid(), Name = "Room 1" };
        _mockRoomRepo.GetAllRoomsAsync().Returns(new List<Room> { room1 });
        _gameState.VisitedRoomIds.Add(room1.Id);

        // Act
        var count = await _sut.RevealMapAsync();

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region SpawnItemAsync Tests

    [Fact]
    public async Task SpawnItemAsync_ReturnsFalse_NotYetImplemented()
    {
        // Act
        var result = await _sut.SpawnItemAsync("test_item", 5);

        // Assert
        result.Should().BeFalse("spawn is not yet implemented");
    }

    #endregion

    #region Helper Methods

    private static Character CreateDamagedCharacter()
    {
        return new Character
        {
            Name = "Test Character",
            MaxHP = 100,
            CurrentHP = 25,
            MaxStamina = 60,
            CurrentStamina = 10,
            MaxAp = 0,
            CurrentAp = 0,
            PsychicStress = 0,
            ActiveStatusEffects = new List<StatusEffectType>()
        };
    }

    #endregion
}
