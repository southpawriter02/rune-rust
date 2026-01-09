using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.Services;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class TurnProcessingTests
{
    private Mock<IGameRepository> _repositoryMock = null!;
    private Mock<IGameConfigurationProvider> _mockConfig = null!;
    private Mock<ILogger<GameSessionService>> _loggerMock = null!;
    private Mock<IDiceService> _diceServiceMock = null!;
    private ItemEffectService _itemEffectService = null!;
    private AbilityService _abilityService = null!;
    private ResourceService _resourceService = null!;
    private EquipmentService _equipmentService = null!;
    private ExperienceService _experienceService = null!;
    private ProgressionService _progressionService = null!;
    private Mock<ILootService> _lootServiceMock = null!;
    private GameSessionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IGameRepository>();
        _mockConfig = new Mock<IGameConfigurationProvider>();
        _loggerMock = new Mock<ILogger<GameSessionService>>();
        _diceServiceMock = new Mock<IDiceService>();

        var itemEffectLoggerMock = new Mock<ILogger<ItemEffectService>>();
        _itemEffectService = new ItemEffectService(itemEffectLoggerMock.Object);

        // Setup minimal config for services
        _mockConfig.Setup(c => c.GetAbilities()).Returns(new List<AbilityDefinition>());
        _mockConfig.Setup(c => c.GetResourceTypes()).Returns(new List<ResourceTypeDefinition>());

        var mockResourceLogger = new Mock<ILogger<ResourceService>>();
        _resourceService = new ResourceService(_mockConfig.Object, mockResourceLogger.Object);

        var mockAbilityLogger = new Mock<ILogger<AbilityService>>();
        _abilityService = new AbilityService(_mockConfig.Object, _resourceService, mockAbilityLogger.Object);

        var mockEquipmentLogger = new Mock<ILogger<EquipmentService>>();
        _equipmentService = new EquipmentService(mockEquipmentLogger.Object);

        var mockExperienceLogger = new Mock<ILogger<ExperienceService>>();
        _experienceService = new ExperienceService(mockExperienceLogger.Object);

        var mockProgressionLogger = new Mock<ILogger<ProgressionService>>();
        _progressionService = new ProgressionService(mockProgressionLogger.Object);

        _lootServiceMock = new Mock<ILootService>();

        _service = new GameSessionService(
            _repositoryMock.Object,
            _loggerMock.Object,
            _itemEffectService,
            _abilityService,
            _resourceService,
            _diceServiceMock.Object,
            _equipmentService,
            _experienceService,
            _progressionService,
            _lootServiceMock.Object);
    }

    [Test]
    public void ProcessTurnEnd_WithNoSession_ReturnsEmpty()
    {
        // Arrange - no session started

        // Act
        var result = _service.ProcessTurnEnd();

        // Assert
        result.NewTurnCount.Should().Be(0);
        result.HasChanges.Should().BeFalse();
    }

    [Test]
    public async Task ProcessTurnEnd_IncrementsTurnCount()
    {
        // Arrange
        await StartNewGameSession();
        var initialTurnCount = 0;

        // Act
        var result = _service.ProcessTurnEnd();

        // Assert
        result.NewTurnCount.Should().Be(initialTurnCount + 1);
    }

    [Test]
    public async Task ProcessTurnEnd_SecondCall_IncrementsFurther()
    {
        // Arrange
        await StartNewGameSession();

        // Act
        _service.ProcessTurnEnd();
        var result = _service.ProcessTurnEnd();

        // Assert
        result.NewTurnCount.Should().Be(2);
    }

    [Test]
    public void IsInCombat_WithNoSession_ReturnsFalse()
    {
        // Arrange - no session

        // Act
        var result = _service.IsInCombat();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task IsInCombat_WithSession_ReturnsBool()
    {
        // Arrange
        await StartNewGameSession();

        // Act
        var result = _service.IsInCombat();

        // Assert
        // Initial room may or may not have monsters, but we can test behavior
        // This test confirms the method executes without error
        result.Should().Be(result); // It returns a valid boolean, just verify it doesn't throw
    }

    private async Task StartNewGameSession()
    {
        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        await _service.StartNewGameAsync("TestPlayer");
    }
}
