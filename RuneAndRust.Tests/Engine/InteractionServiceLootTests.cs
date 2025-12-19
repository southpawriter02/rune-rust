using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;
using EntityCharacter = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the InteractionService loot-related functionality.
/// Validates container searching, item taking, and loot integration.
/// </summary>
public class InteractionServiceLootTests
{
    private readonly Mock<ILogger<InteractionService>> _mockLogger;
    private readonly Mock<IInteractableObjectRepository> _mockObjectRepository;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly Mock<ILootService> _mockLootService;
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly Mock<IInventoryService> _mockInventoryService;
    private readonly GameState _gameState;
    private readonly InteractionService _sut;

    private static readonly Guid TestRoomId = Guid.NewGuid();
    private static readonly Guid TestContainerId = Guid.NewGuid();

    public InteractionServiceLootTests()
    {
        _mockLogger = new Mock<ILogger<InteractionService>>();
        _mockObjectRepository = new Mock<IInteractableObjectRepository>();
        _mockRoomRepository = new Mock<IRoomRepository>();
        _mockLootService = new Mock<ILootService>();
        _mockDiceService = new Mock<IDiceService>();
        _mockInventoryService = new Mock<IInventoryService>();

        _gameState = new GameState
        {
            CurrentRoomId = TestRoomId,
            CurrentCharacter = CreateTestCharacter(wits: 5)
        };

        _sut = new InteractionService(
            _mockLogger.Object,
            _mockObjectRepository.Object,
            _mockRoomRepository.Object,
            _mockLootService.Object,
            _mockDiceService.Object,
            _gameState,
            _mockInventoryService.Object);
    }

    #region SearchContainerAsync Tests

    [Fact]
    public async Task SearchContainerAsync_NoCurrentRoom_ReturnsFailure()
    {
        // Arrange
        _gameState.CurrentRoomId = null;

        // Act
        var result = await _sut.SearchContainerAsync("chest");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("must be in a room");
    }

    [Fact]
    public async Task SearchContainerAsync_ContainerNotFound_ReturnsFailure()
    {
        // Arrange
        _mockObjectRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "chest"))
            .ReturnsAsync((InteractableObject?)null);

        // Act
        var result = await _sut.SearchContainerAsync("chest");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("don't see");
    }

    [Fact]
    public async Task SearchContainerAsync_NotContainer_ReturnsFailure()
    {
        // Arrange
        var notContainer = CreateTestContainer("Stone Pillar");
        notContainer.IsContainer = false;
        _mockObjectRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Stone Pillar"))
            .ReturnsAsync(notContainer);

        // Act
        var result = await _sut.SearchContainerAsync("Stone Pillar");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("cannot be searched");
    }

    [Fact]
    public async Task SearchContainerAsync_ClosedContainer_ReturnsFailure()
    {
        // Arrange
        var container = CreateTestContainer("Rusted Chest");
        container.IsOpen = false;
        _mockObjectRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(container);

        // Act
        var result = await _sut.SearchContainerAsync("Rusted Chest");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("closed");
    }

    [Fact]
    public async Task SearchContainerAsync_AlreadySearched_ReturnsEmpty()
    {
        // Arrange
        var container = CreateTestContainer("Empty Crate");
        container.IsOpen = true;
        container.HasBeenSearched = true;
        _mockObjectRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Empty Crate"))
            .ReturnsAsync(container);

        // Act
        var result = await _sut.SearchContainerAsync("Empty Crate");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already searched");
    }

    [Fact]
    public async Task SearchContainerAsync_OpenContainer_GeneratesLoot()
    {
        // Arrange
        var container = CreateTestContainer("Supply Crate");
        container.IsOpen = true;
        container.HasBeenSearched = false;

        var room = new Room
        {
            Id = TestRoomId,
            BiomeType = BiomeType.Industrial,
            DangerLevel = DangerLevel.Unstable
        };

        var expectedItems = new List<Item>
        {
            new Item { Name = "Iron Ingot", Value = 20, Weight = 800 }
        }.AsReadOnly();

        _mockObjectRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Supply Crate"))
            .ReturnsAsync(container);
        _mockRoomRepository
            .Setup(r => r.GetByIdAsync(TestRoomId))
            .ReturnsAsync(room);
        _mockLootService
            .Setup(l => l.SearchContainerAsync(container, It.IsAny<LootGenerationContext>()))
            .Callback<InteractableObject, LootGenerationContext>((c, _) => c.HasBeenSearched = true)
            .ReturnsAsync(LootResult.Found("You find: Iron Ingot.", expectedItems));

        // Act
        var result = await _sut.SearchContainerAsync("Supply Crate");

        // Assert
        result.Success.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        // Note: HasBeenSearched is set by the LootService, which we mock with a callback
        container.HasBeenSearched.Should().BeTrue();
    }

    [Fact]
    public async Task SearchContainerAsync_UsesRoomBiomeAndDanger()
    {
        // Arrange
        var container = CreateTestContainer("Void Cache");
        container.IsOpen = true;

        var room = new Room
        {
            Id = TestRoomId,
            BiomeType = BiomeType.Void,
            DangerLevel = DangerLevel.Lethal
        };

        _mockObjectRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Void Cache"))
            .ReturnsAsync(container);
        _mockRoomRepository
            .Setup(r => r.GetByIdAsync(TestRoomId))
            .ReturnsAsync(room);
        _mockLootService
            .Setup(l => l.SearchContainerAsync(
                container,
                It.Is<LootGenerationContext>(c =>
                    c.BiomeType == BiomeType.Void &&
                    c.DangerLevel == DangerLevel.Lethal)))
            .ReturnsAsync(LootResult.Empty("Nothing found."));

        // Act
        await _sut.SearchContainerAsync("Void Cache");

        // Assert
        _mockLootService.Verify(l => l.SearchContainerAsync(
            container,
            It.Is<LootGenerationContext>(c =>
                c.BiomeType == BiomeType.Void &&
                c.DangerLevel == DangerLevel.Lethal)),
            Times.Once);
    }

    [Fact]
    public async Task SearchContainerAsync_UsesContainerLootTier()
    {
        // Arrange
        var container = CreateTestContainer("Ornate Chest");
        container.IsOpen = true;
        container.LootTier = QualityTier.Optimized;

        var room = new Room { Id = TestRoomId };

        _mockObjectRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Ornate Chest"))
            .ReturnsAsync(container);
        _mockRoomRepository
            .Setup(r => r.GetByIdAsync(TestRoomId))
            .ReturnsAsync(room);
        _mockLootService
            .Setup(l => l.SearchContainerAsync(
                container,
                It.Is<LootGenerationContext>(c => c.LootTier == QualityTier.Optimized)))
            .ReturnsAsync(LootResult.Empty("Nothing found."));

        // Act
        await _sut.SearchContainerAsync("Ornate Chest");

        // Assert
        _mockLootService.Verify(l => l.SearchContainerAsync(
            container,
            It.Is<LootGenerationContext>(c => c.LootTier == QualityTier.Optimized)),
            Times.Once);
    }

    #endregion

    #region TakeItemAsync Tests

    [Fact]
    public async Task TakeItemAsync_NoCurrentRoom_ReturnsError()
    {
        // Arrange
        _gameState.CurrentRoomId = null;

        // Act
        var result = await _sut.TakeItemAsync("sword");

        // Assert
        result.Should().Contain("must be in a room");
    }

    [Fact]
    public async Task TakeItemAsync_ItemNotFound_ReturnsError()
    {
        // Arrange
        _mockObjectRepository
            .Setup(r => r.GetByRoomIdAsync(TestRoomId))
            .ReturnsAsync(new List<InteractableObject>());

        // Act
        var result = await _sut.TakeItemAsync("sword");

        // Assert
        result.Should().Contain("don't see");
        result.Should().Contain("sword");
    }

    [Fact]
    public async Task TakeItemAsync_NoOpenContainers_ReturnsError()
    {
        // Arrange
        var closedContainer = CreateTestContainer("Locked Chest");
        closedContainer.IsOpen = false;

        _mockObjectRepository
            .Setup(r => r.GetByRoomIdAsync(TestRoomId))
            .ReturnsAsync(new List<InteractableObject> { closedContainer });

        // Act
        var result = await _sut.TakeItemAsync("sword");

        // Assert
        result.Should().Contain("don't see");
    }

    #endregion

    #region GetAvailableItemsAsync Tests

    [Fact]
    public async Task GetAvailableItemsAsync_NoCurrentRoom_ReturnsEmpty()
    {
        // Arrange
        _gameState.CurrentRoomId = null;

        // Act
        var result = await _sut.GetAvailableItemsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailableItemsAsync_NoOpenContainers_ReturnsEmpty()
    {
        // Arrange
        var closedContainer = CreateTestContainer("Closed Chest");
        closedContainer.IsOpen = false;

        _mockObjectRepository
            .Setup(r => r.GetByRoomIdAsync(TestRoomId))
            .ReturnsAsync(new List<InteractableObject> { closedContainer });

        // Act
        var result = await _sut.GetAvailableItemsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private static EntityCharacter CreateTestCharacter(int wits = 5)
    {
        var character = new EntityCharacter
        {
            Name = "Test Hero",
            Wits = wits
        };
        return character;
    }

    private static InteractableObject CreateTestContainer(string name)
    {
        return new InteractableObject
        {
            Id = TestContainerId,
            RoomId = TestRoomId,
            Name = name,
            ObjectType = ObjectType.Container,
            Description = $"A {name.ToLower()}.",
            IsContainer = true,
            IsOpen = false,
            IsLocked = false,
            HasBeenSearched = false
        };
    }

    #endregion
}
