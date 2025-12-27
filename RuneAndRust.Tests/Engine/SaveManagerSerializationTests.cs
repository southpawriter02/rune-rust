using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for SaveManager serialization with source-generated context (v0.3.18c).
/// Verifies that SaveManager uses the optimized serialization path.
/// </summary>
public class SaveManagerSerializationTests
{
    private readonly Mock<ISaveGameRepository> _mockRepo;
    private readonly Mock<IRoomRepository> _mockRoomRepo;
    private readonly Mock<ILogger<SaveManager>> _mockLogger;
    private readonly SaveManager _sut;

    public SaveManagerSerializationTests()
    {
        _mockRepo = new Mock<ISaveGameRepository>();
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockLogger = new Mock<ILogger<SaveManager>>();
        _sut = new SaveManager(_mockRepo.Object, _mockRoomRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SaveGameAsync_SerializesWithSourceGenContext_SucceedsWithValidState()
    {
        // Arrange
        var gameState = new GameState
        {
            Phase = GamePhase.Exploration,
            TurnCount = 15,
            IsSessionActive = true,
            CurrentRoomId = Guid.NewGuid(),
            CurrentCharacter = new CharacterEntity
            {
                Id = Guid.NewGuid(),
                Name = "SerializationTestHero",
                Lineage = LineageType.Human,
                Archetype = ArchetypeType.Warrior,
                CurrentHP = 80,
                MaxHP = 100
            }
        };

        _mockRepo.Setup(r => r.GetBySlotAsync(1)).ReturnsAsync((SaveGame?)null);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<SaveGame>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SaveGameAsync(1, gameState);

        // Assert
        result.Should().BeTrue();
        _mockRepo.Verify(r => r.AddAsync(It.Is<SaveGame>(s =>
            s.SlotNumber == 1 &&
            s.CharacterName == "SerializationTestHero" &&
            s.SerializedState.Contains("\"phase\":") &&
            s.SerializedState.Contains("\"turnCount\":15") &&
            s.SerializedState.Contains("\"currentCharacter\":"))),
            Times.Once);
    }

    [Fact]
    public async Task LoadGameAsync_DeserializesWithSourceGenContext_ReturnsValidState()
    {
        // Arrange
        var savedJson = @"{
            ""phase"":1,
            ""currentCharacter"":{
                ""id"":""11111111-1111-1111-1111-111111111111"",
                ""name"":""LoadedHero"",
                ""lineage"":0,
                ""archetype"":0,
                ""background"":0,
                ""sturdiness"":5,
                ""might"":5,
                ""wits"":5,
                ""will"":5,
                ""finesse"":5,
                ""maxHP"":100,
                ""currentHP"":75,
                ""maxStamina"":60,
                ""currentStamina"":45,
                ""actionPoints"":3,
                ""maxAp"":0,
                ""currentAp"":0,
                ""psychicStress"":10,
                ""corruption"":5,
                ""experiencePoints"":250,
                ""level"":1
            },
            ""turnCount"":25,
            ""isSessionActive"":true,
            ""currentRoomId"":""22222222-2222-2222-2222-222222222222"",
            ""visitedRoomIds"":[""22222222-2222-2222-2222-222222222222""]
        }";

        var saveGame = new SaveGame
        {
            Id = Guid.NewGuid(),
            SlotNumber = 2,
            CharacterName = "LoadedHero",
            SerializedState = savedJson.Replace("\n", "").Replace(" ", ""),
            LastPlayed = DateTime.UtcNow
        };

        _mockRepo.Setup(r => r.GetBySlotAsync(2)).ReturnsAsync(saveGame);

        // Act
        var result = await _sut.LoadGameAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.Phase.Should().Be(GamePhase.Exploration);
        result.TurnCount.Should().Be(25);
        result.IsSessionActive.Should().BeTrue();
        result.CurrentCharacter.Should().NotBeNull();
        result.CurrentCharacter!.Name.Should().Be("LoadedHero");
        result.CurrentCharacter.CurrentHP.Should().Be(75);
        result.CurrentCharacter.PsychicStress.Should().Be(10);
        result.CurrentCharacter.Corruption.Should().Be(5);
    }

    [Fact]
    public async Task GetSaveSlotSummariesAsync_UsesProjection_CallsGetSummariesAsync()
    {
        // Arrange
        var summaries = new List<SaveGameSummary>
        {
            new SaveGameSummary
            {
                SlotNumber = 1,
                CharacterName = "Hero1",
                LastPlayed = DateTime.UtcNow.AddHours(-1),
                IsEmpty = false
            },
            new SaveGameSummary
            {
                SlotNumber = 2,
                CharacterName = "Hero2",
                LastPlayed = DateTime.UtcNow.AddDays(-1),
                IsEmpty = false
            }
        };

        _mockRepo.Setup(r => r.GetSummariesAsync()).ReturnsAsync(summaries);

        // Act
        var result = await _sut.GetSaveSlotSummariesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().CharacterName.Should().Be("Hero1");
        result.Last().CharacterName.Should().Be("Hero2");

        // Verify projection method was called (not GetAllOrderedByLastPlayedAsync)
        _mockRepo.Verify(r => r.GetSummariesAsync(), Times.Once);
        _mockRepo.Verify(r => r.GetAllOrderedByLastPlayedAsync(), Times.Never,
            "Should use GetSummariesAsync projection, not GetAllOrderedByLastPlayedAsync");
    }
}
