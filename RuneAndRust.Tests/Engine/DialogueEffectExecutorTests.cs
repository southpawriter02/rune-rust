using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Effects;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the DialogueEffectExecutor (v0.4.2c - The Voice).
/// Tests all 3 effect types: ModifyReputation, GiveItem, SetFlag.
/// </summary>
public class DialogueEffectExecutorTests
{
    private readonly Mock<IFactionService> _mockFactionService;
    private readonly Mock<IInventoryService> _mockInventoryService;
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<ILogger<DialogueEffectExecutor>> _mockLogger;
    private readonly GameState _gameState;
    private readonly DialogueEffectExecutor _sut;

    public DialogueEffectExecutorTests()
    {
        _mockFactionService = new Mock<IFactionService>();
        _mockInventoryService = new Mock<IInventoryService>();
        _mockItemRepository = new Mock<IItemRepository>();
        _mockLogger = new Mock<ILogger<DialogueEffectExecutor>>();
        _gameState = new GameState();

        _sut = new DialogueEffectExecutor(
            _mockFactionService.Object,
            _mockInventoryService.Object,
            _mockItemRepository.Object,
            _mockLogger.Object);
    }

    private Character CreateTestCharacter(string name = "TestChar")
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = name,
            Level = 1
        };
    }

    private Item CreateTestItem(string name = "Test Item")
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "A test item"
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ModifyReputationEffect Tests (5 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExecuteEffectAsync_ModifyReputation_Positive_ReturnsSuccess()
    {
        // Arrange
        var character = CreateTestCharacter();
        var effect = new ModifyReputationEffect
        {
            Faction = FactionType.IronBanes,
            Amount = 10
        };

        _mockFactionService
            .Setup(s => s.ModifyReputationAsync(character, FactionType.IronBanes, 10, "dialogue effect"))
            .ReturnsAsync(ReputationChangeResult.Ok(FactionType.IronBanes, 0, 10, Disposition.Neutral, Disposition.Neutral, "dialogue effect"));

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.EffectType.Should().Be(DialogueEffectType.ModifyReputation);
        result.Description.Should().Contain("IronBanes");
        result.Description.Should().Contain("10");
    }

    [Fact]
    public async Task ExecuteEffectAsync_ModifyReputation_Negative_ReturnsSuccess()
    {
        // Arrange
        var character = CreateTestCharacter();
        var effect = new ModifyReputationEffect
        {
            Faction = FactionType.TheBound,
            Amount = -15
        };

        _mockFactionService
            .Setup(s => s.ModifyReputationAsync(character, FactionType.TheBound, -15, "dialogue effect"))
            .ReturnsAsync(ReputationChangeResult.Ok(FactionType.TheBound, 0, -15, Disposition.Neutral, Disposition.Hostile, "dialogue effect"));

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.EffectType.Should().Be(DialogueEffectType.ModifyReputation);
    }

    [Fact]
    public async Task ExecuteEffectAsync_ModifyReputation_ServiceFails_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();
        var effect = new ModifyReputationEffect
        {
            Faction = FactionType.IronBanes,
            Amount = 10
        };

        _mockFactionService
            .Setup(s => s.ModifyReputationAsync(character, FactionType.IronBanes, 10, "dialogue effect"))
            .ReturnsAsync(ReputationChangeResult.Failure("Faction not found", FactionType.IronBanes));

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.EffectType.Should().Be(DialogueEffectType.ModifyReputation);
        result.ErrorMessage.Should().Contain("Faction not found");
    }

    [Fact]
    public async Task ExecuteEffectAsync_ModifyReputation_IncludesBeforeAfterInDescription()
    {
        // Arrange
        var character = CreateTestCharacter();
        var effect = new ModifyReputationEffect
        {
            Faction = FactionType.IronBanes,
            Amount = 25
        };

        _mockFactionService
            .Setup(s => s.ModifyReputationAsync(character, FactionType.IronBanes, 25, "dialogue effect"))
            .ReturnsAsync(ReputationChangeResult.Ok(FactionType.IronBanes, 10, 35, Disposition.Neutral, Disposition.Friendly, "dialogue effect"));

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.Description.Should().Contain("10");
        result.Description.Should().Contain("35");
    }

    [Fact]
    public async Task ExecuteEffectAsync_ModifyReputation_ServiceThrows_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();
        var effect = new ModifyReputationEffect
        {
            Faction = FactionType.IronBanes,
            Amount = 10
        };

        _mockFactionService
            .Setup(s => s.ModifyReputationAsync(character, FactionType.IronBanes, 10, "dialogue effect"))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.EffectType.Should().Be(DialogueEffectType.ModifyReputation);
        result.ErrorMessage.Should().Contain("Database error");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GiveItemEffect Tests (5 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExecuteEffectAsync_GiveItem_SingleItem_ReturnsSuccess()
    {
        // Arrange
        var character = CreateTestCharacter();
        var item = CreateTestItem("Iron Key");
        var effect = new GiveItemEffect
        {
            ItemId = item.Id,
            ItemName = "Iron Key",
            Quantity = 1
        };

        _mockItemRepository
            .Setup(r => r.GetByIdAsync(item.Id))
            .ReturnsAsync(item);

        _mockInventoryService
            .Setup(s => s.AddItemAsync(character, item, 1))
            .ReturnsAsync(new InventoryResult(true, "Added Iron Key"));

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.EffectType.Should().Be(DialogueEffectType.GiveItem);
        result.Description.Should().Contain("Iron Key");
        result.Description.Should().NotContain("x"); // Single item should not show quantity
    }

    [Fact]
    public async Task ExecuteEffectAsync_GiveItem_MultipleItems_ShowsQuantity()
    {
        // Arrange
        var character = CreateTestCharacter();
        var item = CreateTestItem("Gold Coin");
        var effect = new GiveItemEffect
        {
            ItemId = item.Id,
            ItemName = "Gold Coin",
            Quantity = 50
        };

        _mockItemRepository
            .Setup(r => r.GetByIdAsync(item.Id))
            .ReturnsAsync(item);

        _mockInventoryService
            .Setup(s => s.AddItemAsync(character, item, 50))
            .ReturnsAsync(new InventoryResult(true, "Added Gold Coin x50"));

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.Description.Should().Contain("x50");
    }

    [Fact]
    public async Task ExecuteEffectAsync_GiveItem_ItemNotFound_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();
        var effect = new GiveItemEffect
        {
            ItemId = Guid.NewGuid(),
            ItemName = "Nonexistent Item",
            Quantity = 1
        };

        _mockItemRepository
            .Setup(r => r.GetByIdAsync(effect.ItemId))
            .ReturnsAsync((Item?)null);

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.EffectType.Should().Be(DialogueEffectType.GiveItem);
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteEffectAsync_GiveItem_InventoryFull_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();
        var item = CreateTestItem("Heavy Boulder");
        var effect = new GiveItemEffect
        {
            ItemId = item.Id,
            ItemName = "Heavy Boulder",
            Quantity = 1
        };

        _mockItemRepository
            .Setup(r => r.GetByIdAsync(item.Id))
            .ReturnsAsync(item);

        _mockInventoryService
            .Setup(s => s.AddItemAsync(character, item, 1))
            .ReturnsAsync(new InventoryResult(false, "Inventory is full"));

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("full");
    }

    [Fact]
    public async Task ExecuteEffectAsync_GiveItem_ServiceThrows_ReturnsFailed()
    {
        // Arrange
        var character = CreateTestCharacter();
        var item = CreateTestItem("Test Item");
        var effect = new GiveItemEffect
        {
            ItemId = item.Id,
            ItemName = "Test Item",
            Quantity = 1
        };

        _mockItemRepository
            .Setup(r => r.GetByIdAsync(item.Id))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Database error");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SetFlagEffect Tests (3 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExecuteEffectAsync_SetFlag_True_SetsFlag()
    {
        // Arrange
        var character = CreateTestCharacter();
        var effect = new SetFlagEffect
        {
            FlagKey = "quest_completed",
            Value = true
        };

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.EffectType.Should().Be(DialogueEffectType.SetFlag);
        result.Description.Should().Contain("quest_completed");
        result.Description.Should().Contain("set");
        _gameState.GetFlag("quest_completed").Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteEffectAsync_SetFlag_False_ClearsFlag()
    {
        // Arrange
        var character = CreateTestCharacter();
        _gameState.SetFlag("temporary_buff", true);

        var effect = new SetFlagEffect
        {
            FlagKey = "temporary_buff",
            Value = false
        };

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeTrue();
        result.Description.Should().Contain("cleared");
        _gameState.GetFlag("temporary_buff").Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteEffectAsync_SetFlag_OverwritesExisting()
    {
        // Arrange
        var character = CreateTestCharacter();
        _gameState.SetFlag("npc_met", false);

        var effect = new SetFlagEffect
        {
            FlagKey = "npc_met",
            Value = true
        };

        // Act
        var result = await _sut.ExecuteEffectAsync(character, effect, _gameState);

        // Assert
        result.Success.Should().BeTrue();
        _gameState.GetFlag("npc_met").Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ExecuteEffectsAsync Tests (2 tests)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExecuteEffectsAsync_MultipleEffects_ExecutesAll()
    {
        // Arrange
        var character = CreateTestCharacter();
        var item = CreateTestItem("Reward");

        var effects = new List<DialogueEffect>
        {
            new SetFlagEffect { FlagKey = "talked_to_npc", Value = true },
            new ModifyReputationEffect { Faction = FactionType.IronBanes, Amount = 5 },
            new GiveItemEffect { ItemId = item.Id, ItemName = "Reward", Quantity = 1 }
        };

        _mockFactionService
            .Setup(s => s.ModifyReputationAsync(character, FactionType.IronBanes, 5, "dialogue effect"))
            .ReturnsAsync(ReputationChangeResult.Ok(FactionType.IronBanes, 0, 5, Disposition.Neutral, Disposition.Neutral, "dialogue effect"));

        _mockItemRepository
            .Setup(r => r.GetByIdAsync(item.Id))
            .ReturnsAsync(item);

        _mockInventoryService
            .Setup(s => s.AddItemAsync(character, item, 1))
            .ReturnsAsync(new InventoryResult(true, "Added"));

        // Act
        var results = await _sut.ExecuteEffectsAsync(character, effects, _gameState);

        // Assert
        results.Should().HaveCount(3);
        results.All(r => r.Success).Should().BeTrue();
        _gameState.GetFlag("talked_to_npc").Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteEffectsAsync_PartialFailure_ContinuesExecution()
    {
        // Arrange
        var character = CreateTestCharacter();

        var effects = new List<DialogueEffect>
        {
            new SetFlagEffect { FlagKey = "flag1", Value = true },
            new GiveItemEffect { ItemId = Guid.NewGuid(), ItemName = "Missing", Quantity = 1 }, // Will fail
            new SetFlagEffect { FlagKey = "flag2", Value = true }
        };

        _mockItemRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Item?)null);

        // Act
        var results = await _sut.ExecuteEffectsAsync(character, effects, _gameState);

        // Assert
        results.Should().HaveCount(3);
        results[0].Success.Should().BeTrue(); // First flag set
        results[1].Success.Should().BeFalse(); // Item not found
        results[2].Success.Should().BeTrue(); // Second flag still set

        _gameState.GetFlag("flag1").Should().BeTrue();
        _gameState.GetFlag("flag2").Should().BeTrue();
    }
}
