using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;
using EntityCharacter = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the SaveManager service.
/// </summary>
public class SaveManagerTests
{
    private readonly Mock<ISaveGameRepository> _mockRepo;
    private readonly Mock<ILogger<SaveManager>> _mockLogger;
    private readonly SaveManager _saveManager;

    public SaveManagerTests()
    {
        _mockRepo = new Mock<ISaveGameRepository>();
        _mockLogger = new Mock<ILogger<SaveManager>>();
        _saveManager = new SaveManager(_mockRepo.Object, _mockLogger.Object);
    }

    #region SaveGameAsync Tests

    [Fact]
    public async Task SaveGameAsync_NewSlot_CreatesSaveGame()
    {
        // Arrange
        var gameState = new GameState
        {
            Phase = GamePhase.Exploration,
            TurnCount = 10,
            IsSessionActive = true,
            CurrentCharacter = new EntityCharacter { Name = "TestHero" }
        };

        _mockRepo.Setup(r => r.GetBySlotAsync(1)).ReturnsAsync((SaveGame?)null);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<SaveGame>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _saveManager.SaveGameAsync(1, gameState);

        // Assert
        result.Should().BeTrue();
        _mockRepo.Verify(r => r.AddAsync(It.Is<SaveGame>(s =>
            s.SlotNumber == 1 &&
            s.CharacterName == "TestHero")), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SaveGameAsync_ExistingSlot_UpdatesSaveGame()
    {
        // Arrange
        var existingSave = new SaveGame
        {
            Id = Guid.NewGuid(),
            SlotNumber = 1,
            CharacterName = "OldChar"
        };
        var gameState = new GameState
        {
            CurrentCharacter = new EntityCharacter { Name = "NewChar" }
        };

        _mockRepo.Setup(r => r.GetBySlotAsync(1)).ReturnsAsync(existingSave);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<SaveGame>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _saveManager.SaveGameAsync(1, gameState);

        // Assert
        result.Should().BeTrue();
        _mockRepo.Verify(r => r.UpdateAsync(It.Is<SaveGame>(s =>
            s.CharacterName == "NewChar")), Times.Once);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<SaveGame>()), Times.Never);
    }

    [Fact]
    public async Task SaveGameAsync_NoCharacter_UsesUnknownName()
    {
        // Arrange
        var gameState = new GameState { CurrentCharacter = null };

        _mockRepo.Setup(r => r.GetBySlotAsync(1)).ReturnsAsync((SaveGame?)null);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<SaveGame>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _saveManager.SaveGameAsync(1, gameState);

        // Assert
        result.Should().BeTrue();
        _mockRepo.Verify(r => r.AddAsync(It.Is<SaveGame>(s =>
            s.CharacterName == "Unknown")), Times.Once);
    }

    [Fact]
    public async Task SaveGameAsync_RepoThrowsException_ReturnsFalse()
    {
        // Arrange
        var gameState = new GameState();
        _mockRepo.Setup(r => r.GetBySlotAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _saveManager.SaveGameAsync(1, gameState);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveGameAsync_SerializesGameState()
    {
        // Arrange
        var gameState = new GameState
        {
            Phase = GamePhase.Exploration,
            TurnCount = 5,
            IsSessionActive = true
        };
        SaveGame? capturedSave = null;

        _mockRepo.Setup(r => r.GetBySlotAsync(1)).ReturnsAsync((SaveGame?)null);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<SaveGame>()))
            .Callback<SaveGame>(s => capturedSave = s)
            .Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _saveManager.SaveGameAsync(1, gameState);

        // Assert
        capturedSave.Should().NotBeNull();
        capturedSave!.SerializedState.Should().Contain("\"phase\"");
        capturedSave.SerializedState.Should().Contain("\"turnCount\"");
    }

    #endregion

    #region LoadGameAsync Tests

    [Fact]
    public async Task LoadGameAsync_ExistingSlot_ReturnsGameState()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            SlotNumber = 1,
            CharacterName = "LoadedChar",
            SerializedState = "{\"phase\":1,\"turnCount\":15,\"isSessionActive\":true}"
        };
        _mockRepo.Setup(r => r.GetBySlotAsync(1)).ReturnsAsync(saveGame);

        // Act
        var result = await _saveManager.LoadGameAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Phase.Should().Be(GamePhase.Exploration);
        result.TurnCount.Should().Be(15);
        result.IsSessionActive.Should().BeTrue();
    }

    [Fact]
    public async Task LoadGameAsync_NonExistentSlot_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetBySlotAsync(99)).ReturnsAsync((SaveGame?)null);

        // Act
        var result = await _saveManager.LoadGameAsync(99);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoadGameAsync_InvalidJson_ReturnsNull()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            SlotNumber = 1,
            SerializedState = "not valid json"
        };
        _mockRepo.Setup(r => r.GetBySlotAsync(1)).ReturnsAsync(saveGame);

        // Act
        var result = await _saveManager.LoadGameAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoadGameAsync_RepoThrowsException_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetBySlotAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _saveManager.LoadGameAsync(1);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetSaveSlotSummariesAsync Tests

    [Fact]
    public async Task GetSaveSlotSummariesAsync_ReturnsSummaries()
    {
        // Arrange - v0.3.18c: Now uses GetSummariesAsync projection
        var summaries = new List<SaveGameSummary>
        {
            new() { SlotNumber = 1, CharacterName = "Hero1", LastPlayed = DateTime.UtcNow, IsEmpty = false },
            new() { SlotNumber = 2, CharacterName = "Hero2", LastPlayed = DateTime.UtcNow.AddDays(-1), IsEmpty = false }
        };
        _mockRepo.Setup(r => r.GetSummariesAsync()).ReturnsAsync(summaries);

        // Act
        var result = (await _saveManager.GetSaveSlotSummariesAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].CharacterName.Should().Be("Hero1");
        result[0].IsEmpty.Should().BeFalse();
        result[1].CharacterName.Should().Be("Hero2");
    }

    [Fact]
    public async Task GetSaveSlotSummariesAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange - v0.3.18c: Now uses GetSummariesAsync projection
        _mockRepo.Setup(r => r.GetSummariesAsync()).ReturnsAsync(new List<SaveGameSummary>());

        // Act
        var result = await _saveManager.GetSaveSlotSummariesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region DeleteSaveAsync Tests

    [Fact]
    public async Task DeleteSaveAsync_ExistingSlot_ReturnsTrue()
    {
        // Arrange
        var saveGame = new SaveGame { Id = Guid.NewGuid(), SlotNumber = 1 };
        _mockRepo.Setup(r => r.GetBySlotAsync(1)).ReturnsAsync(saveGame);
        _mockRepo.Setup(r => r.DeleteAsync(saveGame.Id)).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _saveManager.DeleteSaveAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockRepo.Verify(r => r.DeleteAsync(saveGame.Id), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteSaveAsync_NonExistentSlot_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetBySlotAsync(99)).ReturnsAsync((SaveGame?)null);

        // Act
        var result = await _saveManager.DeleteSaveAsync(99);

        // Assert
        result.Should().BeFalse();
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSaveAsync_RepoThrowsException_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetBySlotAsync(1))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _saveManager.DeleteSaveAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SaveExistsAsync Tests

    [Fact]
    public async Task SaveExistsAsync_SlotExists_ReturnsTrue()
    {
        // Arrange
        _mockRepo.Setup(r => r.SlotExistsAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _saveManager.SaveExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SaveExistsAsync_SlotDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.SlotExistsAsync(99)).ReturnsAsync(false);

        // Act
        var result = await _saveManager.SaveExistsAsync(99);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SaveGameSummary Tests

    [Fact]
    public void SaveGameSummary_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var summary = new SaveGameSummary();

        // Assert
        summary.SlotNumber.Should().Be(0);
        summary.CharacterName.Should().BeEmpty();
        summary.LastPlayed.Should().Be(default);
        summary.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void SaveGameSummary_CanSetAllProperties()
    {
        // Arrange
        var lastPlayed = DateTime.UtcNow;
        var summary = new SaveGameSummary
        {
            SlotNumber = 1,
            CharacterName = "TestChar",
            LastPlayed = lastPlayed,
            IsEmpty = false
        };

        // Assert
        summary.SlotNumber.Should().Be(1);
        summary.CharacterName.Should().Be("TestChar");
        summary.LastPlayed.Should().Be(lastPlayed);
        summary.IsEmpty.Should().BeFalse();
    }

    #endregion
}
