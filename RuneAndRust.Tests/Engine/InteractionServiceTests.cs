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
/// Tests for the InteractionService.
/// Validates WITS-based examination, container operations, and search mechanics.
/// </summary>
public class InteractionServiceTests
{
    private readonly Mock<ILogger<InteractionService>> _mockLogger;
    private readonly Mock<IInteractableObjectRepository> _mockRepository;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly Mock<ILootService> _mockLootService;
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly GameState _gameState;
    private readonly InteractionService _sut;

    private static readonly Guid TestRoomId = Guid.NewGuid();
    private static readonly Guid TestCharacterId = Guid.NewGuid();

    public InteractionServiceTests()
    {
        _mockLogger = new Mock<ILogger<InteractionService>>();
        _mockRepository = new Mock<IInteractableObjectRepository>();
        _mockRoomRepository = new Mock<IRoomRepository>();
        _mockLootService = new Mock<ILootService>();
        _mockDiceService = new Mock<IDiceService>();
        _gameState = new GameState
        {
            CurrentRoomId = TestRoomId,
            CurrentCharacter = CreateTestCharacter(wits: 5)
        };
        _sut = new InteractionService(
            _mockLogger.Object,
            _mockRepository.Object,
            _mockRoomRepository.Object,
            _mockLootService.Object,
            _mockDiceService.Object,
            _gameState);
    }

    #region ExamineAsync Tests

    [Fact]
    public async Task ExamineAsync_ObjectNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "chest"))
            .ReturnsAsync((InteractableObject?)null);

        // Act
        var result = await _sut.ExamineAsync("chest");

        // Assert
        result.Success.Should().BeFalse();
        result.Description.Should().Contain("chest");
    }

    [Fact]
    public async Task ExamineAsync_NoCurrentRoom_ReturnsNotFoundResult()
    {
        // Arrange
        _gameState.CurrentRoomId = null;

        // Act
        var result = await _sut.ExamineAsync("chest");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ExamineAsync_NoCurrentCharacter_ReturnsNotFoundResult()
    {
        // Arrange
        _gameState.CurrentCharacter = null;

        // Act
        var result = await _sut.ExamineAsync("chest");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ExamineAsync_ZeroNetSuccesses_ReturnsTierZero()
    {
        // Arrange
        var testObject = CreateTestObject("Rusted Chest");
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(testObject);
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(1, 1, new[] { 8, 1, 5, 3, 2 })); // 1 success, 1 botch = 0 net

        // Act
        var result = await _sut.ExamineAsync("Rusted Chest");

        // Assert
        result.TierRevealed.Should().Be(0);
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ExamineAsync_OneNetSuccess_ReturnsTierOne()
    {
        // Arrange
        var testObject = CreateTestObject("Rusted Chest");
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(testObject);
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(2, 1, new[] { 8, 9, 1, 3, 2 })); // 2 successes, 1 botch = 1 net

        // Act
        var result = await _sut.ExamineAsync("Rusted Chest");

        // Assert
        result.TierRevealed.Should().Be(1);
        result.Success.Should().BeTrue();
        result.RevealedDetailed.Should().BeTrue();
    }

    [Fact]
    public async Task ExamineAsync_ThreeOrMoreNetSuccesses_ReturnsTierTwo()
    {
        // Arrange
        var testObject = CreateTestObject("Rusted Chest");
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(testObject);
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(4, 0, new[] { 8, 9, 10, 8, 5 })); // 4 successes, 0 botches = 4 net

        // Act
        var result = await _sut.ExamineAsync("Rusted Chest");

        // Assert
        result.TierRevealed.Should().Be(2);
        result.Success.Should().BeTrue();
        result.RevealedExpert.Should().BeTrue();
    }

    [Fact]
    public async Task ExamineAsync_UsesCharacterWits_ForDicePool()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter(wits: 7);
        var testObject = CreateTestObject("Rusted Chest");
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(testObject);
        _mockDiceService
            .Setup(d => d.Roll(7, It.IsAny<string>()))
            .Returns(new DiceResult(2, 0, new int[7]));

        // Act
        await _sut.ExamineAsync("Rusted Chest");

        // Assert
        _mockDiceService.Verify(d => d.Roll(7, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ExamineAsync_UpdatesObjectState_WhenNewTierRevealed()
    {
        // Arrange
        var testObject = CreateTestObject("Rusted Chest");
        testObject.HighestExaminationTier = 0;
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(testObject);
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(2, 0, new[] { 8, 9, 5, 3, 2 })); // 2 net = tier 1

        // Act
        var result = await _sut.ExamineAsync("Rusted Chest");

        // Assert
        result.NewInfoRevealed.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<InteractableObject>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExamineAsync_IncludesRolls_InResult()
    {
        // Arrange
        var expectedRolls = new[] { 8, 3, 10, 1, 5 };
        var testObject = CreateTestObject("Rusted Chest");
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(testObject);
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(2, 1, expectedRolls));

        // Act
        var result = await _sut.ExamineAsync("Rusted Chest");

        // Assert
        result.Rolls.Should().BeEquivalentTo(expectedRolls);
    }

    #endregion

    #region OpenAsync Tests

    [Fact]
    public async Task OpenAsync_ObjectNotFound_ReturnsNotFoundMessage()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "chest"))
            .ReturnsAsync((InteractableObject?)null);

        // Act
        var result = await _sut.OpenAsync("chest");

        // Assert
        result.Should().Contain("chest");
        result.Should().Contain("don't see");
    }

    [Fact]
    public async Task OpenAsync_NotAContainer_ReturnsCannotOpenMessage()
    {
        // Arrange
        var testObject = CreateTestObject("Ancient Bones");
        testObject.IsContainer = false;
        testObject.ObjectType = ObjectType.Corpse;
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Ancient Bones"))
            .ReturnsAsync(testObject);

        // Act
        var result = await _sut.OpenAsync("Ancient Bones");

        // Assert
        result.Should().Contain("cannot be opened");
    }

    [Fact]
    public async Task OpenAsync_AlreadyOpen_ReturnsAlreadyOpenMessage()
    {
        // Arrange
        var testObject = CreateTestObject("Rusted Chest");
        testObject.IsContainer = true;
        testObject.IsOpen = true;
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(testObject);

        // Act
        var result = await _sut.OpenAsync("Rusted Chest");

        // Assert
        result.Should().Contain("already open");
    }

    [Fact]
    public async Task OpenAsync_Locked_ReturnsLockedMessage()
    {
        // Arrange
        var testObject = CreateTestObject("Rusted Chest");
        testObject.IsContainer = true;
        testObject.IsOpen = false;
        testObject.IsLocked = true;
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(testObject);

        // Act
        var result = await _sut.OpenAsync("Rusted Chest");

        // Assert
        result.Should().Contain("locked");
    }

    [Fact]
    public async Task OpenAsync_Success_OpensContainer()
    {
        // Arrange
        var testObject = CreateTestObject("Rusted Chest");
        testObject.IsContainer = true;
        testObject.IsOpen = false;
        testObject.IsLocked = false;
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(testObject);

        // Act
        var result = await _sut.OpenAsync("Rusted Chest");

        // Assert
        result.Should().Contain("open");
        testObject.IsOpen.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(testObject), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region CloseAsync Tests

    [Fact]
    public async Task CloseAsync_ObjectNotFound_ReturnsNotFoundMessage()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "chest"))
            .ReturnsAsync((InteractableObject?)null);

        // Act
        var result = await _sut.CloseAsync("chest");

        // Assert
        result.Should().Contain("chest");
        result.Should().Contain("don't see");
    }

    [Fact]
    public async Task CloseAsync_NotAContainer_ReturnsCannotCloseMessage()
    {
        // Arrange
        var testObject = CreateTestObject("Wall Inscription");
        testObject.IsContainer = false;
        testObject.ObjectType = ObjectType.Inscription;
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Wall Inscription"))
            .ReturnsAsync(testObject);

        // Act
        var result = await _sut.CloseAsync("Wall Inscription");

        // Assert
        result.Should().Contain("cannot be closed");
    }

    [Fact]
    public async Task CloseAsync_AlreadyClosed_ReturnsAlreadyClosedMessage()
    {
        // Arrange
        var testObject = CreateTestObject("Rusted Chest");
        testObject.IsContainer = true;
        testObject.IsOpen = false;
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(testObject);

        // Act
        var result = await _sut.CloseAsync("Rusted Chest");

        // Assert
        result.Should().Contain("already closed");
    }

    [Fact]
    public async Task CloseAsync_Success_ClosesContainer()
    {
        // Arrange
        var testObject = CreateTestObject("Rusted Chest");
        testObject.IsContainer = true;
        testObject.IsOpen = true;
        _mockRepository
            .Setup(r => r.GetByNameInRoomAsync(TestRoomId, "Rusted Chest"))
            .ReturnsAsync(testObject);

        // Act
        var result = await _sut.CloseAsync("Rusted Chest");

        // Assert
        result.Should().Contain("close");
        testObject.IsOpen.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(testObject), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_NoCurrentRoom_ReturnsErrorMessage()
    {
        // Arrange
        _gameState.CurrentRoomId = null;

        // Act
        var result = await _sut.SearchAsync();

        // Assert
        result.Should().Contain("must be in a room");
    }

    [Fact]
    public async Task SearchAsync_EmptyRoom_ReturnsNothingFoundMessage()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByRoomIdAsync(TestRoomId))
            .ReturnsAsync(Enumerable.Empty<InteractableObject>());
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(2, 0, new[] { 8, 9 }));

        // Act
        var result = await _sut.SearchAsync();

        // Assert
        result.Should().Contain("nothing of interest");
    }

    [Fact]
    public async Task SearchAsync_WithObjects_ListsObjectNames()
    {
        // Arrange
        var objects = new List<InteractableObject>
        {
            CreateTestObject("Rusted Chest"),
            CreateTestObject("Ancient Bones")
        };
        _mockRepository
            .Setup(r => r.GetByRoomIdAsync(TestRoomId))
            .ReturnsAsync(objects);
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(1, 0, new[] { 8 }));

        // Act
        var result = await _sut.SearchAsync();

        // Assert
        result.Should().Contain("Rusted Chest");
        result.Should().Contain("Ancient Bones");
    }

    [Fact]
    public async Task SearchAsync_UsesCharacterWits()
    {
        // Arrange
        _gameState.CurrentCharacter = CreateTestCharacter(wits: 8);
        _mockRepository
            .Setup(r => r.GetByRoomIdAsync(TestRoomId))
            .ReturnsAsync(new List<InteractableObject>());
        _mockDiceService
            .Setup(d => d.Roll(8, It.IsAny<string>()))
            .Returns(new DiceResult(2, 0, new int[8]));

        // Act
        await _sut.SearchAsync();

        // Assert
        _mockDiceService.Verify(d => d.Roll(8, It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region GetVisibleObjectsAsync Tests

    [Fact]
    public async Task GetVisibleObjectsAsync_NoCurrentRoom_ReturnsEmpty()
    {
        // Arrange
        _gameState.CurrentRoomId = null;

        // Act
        var result = await _sut.GetVisibleObjectsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVisibleObjectsAsync_WithObjects_ReturnsAllObjects()
    {
        // Arrange
        var objects = new List<InteractableObject>
        {
            CreateTestObject("Chest"),
            CreateTestObject("Table"),
            CreateTestObject("Bones")
        };
        _mockRepository
            .Setup(r => r.GetByRoomIdAsync(TestRoomId))
            .ReturnsAsync(objects);

        // Act
        var result = await _sut.GetVisibleObjectsAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region ListObjectsAsync Tests

    [Fact]
    public async Task ListObjectsAsync_EmptyRoom_ReturnsNothingMessage()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByRoomIdAsync(TestRoomId))
            .ReturnsAsync(Enumerable.Empty<InteractableObject>());

        // Act
        var result = await _sut.ListObjectsAsync();

        // Assert
        result.Should().Contain("nothing of particular interest");
    }

    [Fact]
    public async Task ListObjectsAsync_SingleObject_ListsIt()
    {
        // Arrange
        var objects = new List<InteractableObject> { CreateTestObject("Rusted Chest") };
        _mockRepository
            .Setup(r => r.GetByRoomIdAsync(TestRoomId))
            .ReturnsAsync(objects);

        // Act
        var result = await _sut.ListObjectsAsync();

        // Assert
        result.Should().Contain("Rusted Chest");
    }

    [Fact]
    public async Task ListObjectsAsync_MultipleObjects_ListsAllWithCommas()
    {
        // Arrange
        var objects = new List<InteractableObject>
        {
            CreateTestObject("Rusted Chest"),
            CreateTestObject("Ancient Bones"),
            CreateTestObject("Wall Inscription")
        };
        _mockRepository
            .Setup(r => r.GetByRoomIdAsync(TestRoomId))
            .ReturnsAsync(objects);

        // Act
        var result = await _sut.ListObjectsAsync();

        // Assert
        result.Should().Contain("Rusted Chest");
        result.Should().Contain("Ancient Bones");
        result.Should().Contain("Wall Inscription");
        result.Should().Contain("and");
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

    private static InteractableObject CreateTestObject(string name)
    {
        return new InteractableObject
        {
            RoomId = TestRoomId,
            Name = name,
            ObjectType = ObjectType.Container,
            Description = $"A {name.ToLower()}.",
            DetailedDescription = $"Upon closer inspection, the {name.ToLower()} reveals more.",
            ExpertDescription = $"Expert knowledge reveals secrets about the {name.ToLower()}.",
            IsContainer = true,
            IsOpen = false,
            IsLocked = false
        };
    }

    #endregion
}
