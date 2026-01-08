using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class GameSessionServiceTests
{
    private Mock<IGameRepository> _repositoryMock = null!;
    private Mock<IGameConfigurationProvider> _mockConfig = null!;
    private Mock<ILogger<GameSessionService>> _loggerMock = null!;
    private Mock<ILogger<ItemEffectService>> _itemEffectLoggerMock = null!;
    private Mock<IDiceService> _diceServiceMock = null!;
    private ItemEffectService _itemEffectService = null!;
    private AbilityService _abilityService = null!;
    private ResourceService _resourceService = null!;
    private GameSessionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IGameRepository>();
        _mockConfig = new Mock<IGameConfigurationProvider>();
        _loggerMock = new Mock<ILogger<GameSessionService>>();
        _itemEffectLoggerMock = new Mock<ILogger<ItemEffectService>>();
        _diceServiceMock = new Mock<IDiceService>();
        _itemEffectService = new ItemEffectService(_itemEffectLoggerMock.Object);

        // Setup minimal config for AbilityService
        _mockConfig.Setup(c => c.GetAbilities()).Returns(new List<AbilityDefinition>());
        _mockConfig.Setup(c => c.GetResourceTypes()).Returns(new List<ResourceTypeDefinition>());

        var mockResourceLogger = new Mock<ILogger<ResourceService>>();
        _resourceService = new ResourceService(_mockConfig.Object, mockResourceLogger.Object);

        var mockAbilityLogger = new Mock<ILogger<AbilityService>>();
        _abilityService = new AbilityService(_mockConfig.Object, _resourceService, mockAbilityLogger.Object);

        _service = new GameSessionService(
            _repositoryMock.Object,
            _loggerMock.Object,
            _itemEffectService,
            _abilityService,
            _resourceService,
            _diceServiceMock.Object);
    }

    [Test]
    public async Task StartNewGameAsync_CreatesSessionAndSaves()
    {
        // Arrange
        var playerName = "TestPlayer";
        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _service.StartNewGameAsync(playerName);

        // Assert
        result.Should().NotBeNull();
        result.Player.Name.Should().Be(playerName);
        result.State.Should().Be(GameState.Playing);
        _service.HasActiveSession.Should().BeTrue();

        _repositoryMock.Verify(
            r => r.SaveAsync(It.Is<GameSession>(s => s.Player.Name == playerName), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task LoadGameAsync_WhenSessionExists_LoadsSession()
    {
        // Arrange
        var session = GameSession.CreateNew("ExistingPlayer");
        _repositoryMock
            .Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session.Id);

        // Act
        var result = await _service.LoadGameAsync(session.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Player.Name.Should().Be("ExistingPlayer");
        _service.HasActiveSession.Should().BeTrue();
    }

    [Test]
    public async Task LoadGameAsync_WhenSessionDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameSession?)null);

        // Act
        var result = await _service.LoadGameAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task TryMove_WhenValidDirection_ReturnsSuccess()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
        await _service.StartNewGameAsync("TestPlayer");

        // Act
        var (success, message) = _service.TryMove(Direction.North);

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("north");
    }

    [Test]
    public async Task TryMove_WhenInvalidDirection_ReturnsFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
        await _service.StartNewGameAsync("TestPlayer");

        // Act - Starting room has no south exit
        var (success, message) = _service.TryMove(Direction.South);

        // Assert
        success.Should().BeFalse();
        message.Should().Contain("cannot");
    }

    [Test]
    public void TryMove_WhenNoActiveSession_ReturnsFailure()
    {
        // Act
        var (success, message) = _service.TryMove(Direction.North);

        // Assert
        success.Should().BeFalse();
        message.Should().Contain("No active game session");
    }

    [Test]
    public async Task TryPickUpItem_WhenItemExists_ReturnsSuccess()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
        await _service.StartNewGameAsync("TestPlayer");
        _service.TryMove(Direction.North); // Move to Armory with Rusty Sword

        // Act
        var (success, message) = _service.TryPickUpItem("Rusty Sword");

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("picked up");
    }

    [Test]
    public async Task TryPickUpItem_WhenItemDoesNotExist_ReturnsFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
        await _service.StartNewGameAsync("TestPlayer");

        // Act
        var (success, message) = _service.TryPickUpItem("Nonexistent");

        // Assert
        success.Should().BeFalse();
        message.Should().Contain("no");
    }

    [Test]
    public void EndSession_ClearsActiveSession()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
        _service.StartNewGameAsync("TestPlayer").Wait();

        // Act
        _service.EndSession();

        // Assert
        _service.HasActiveSession.Should().BeFalse();
    }
}
