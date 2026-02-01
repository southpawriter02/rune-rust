namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Records;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TraumaService"/>.
/// </summary>
[TestFixture]
public class TraumaServiceTests
{
    private Mock<ITraumaCheckService> _checkServiceMock = null!;
    private Mock<ITraumaRepository> _traumaRepositoryMock = null!;
    private Mock<IPlayerRepository> _playerRepositoryMock = null!;
    private Mock<IGameConfigurationProvider> _configProviderMock = null!;
    private Mock<ILogger<TraumaService>> _loggerMock = null!;
    private TraumaService _service = null!;

    private static readonly Guid TestCharacterId = Guid.NewGuid();
    private static readonly Player TestPlayer = new("TestPlayer");

    [SetUp]
    public void SetUp()
    {
        _checkServiceMock = new Mock<ITraumaCheckService>();
        _traumaRepositoryMock = new Mock<ITraumaRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        _configProviderMock = new Mock<IGameConfigurationProvider>();
        _loggerMock = new Mock<ILogger<TraumaService>>();

        SetupDefaultConfiguration();

        _service = new TraumaService(
            _checkServiceMock.Object,
            _traumaRepositoryMock.Object,
            _playerRepositoryMock.Object,
            _configProviderMock.Object,
            _loggerMock.Object);
    }

    private void SetupDefaultConfiguration()
    {
        var config = new TraumaConfiguration
        {
            Version = "1.0",
            Traumas =
            [
                new TraumaConfigEntry
                {
                    Id = "survivors-guilt",
                    Name = "Survivor's Guilt",
                    Type = "Emotional",
                    Description = "You carry the weight of those who didn't make it.",
                    FlavorText = "Why them? Why not me?",
                    IsRetirementTrauma = true,
                    RetirementCondition = "On acquisition",
                    IsStackable = false,
                    AcquisitionSources = ["AllyDeath"],
                    Triggers = [],
                    Effects = []
                },
                new TraumaConfigEntry
                {
                    Id = "combat-flashbacks",
                    Name = "Combat Flashbacks",
                    Type = "Cognitive",
                    Description = "Intrusive memories of past combat situations.",
                    FlavorText = "The sounds, the sights...",
                    IsRetirementTrauma = false,
                    RetirementCondition = null,
                    IsStackable = true,
                    AcquisitionSources = ["CombatEvents"],
                    Triggers = [],
                    Effects =
                    [
                        new TraumaConfigEffect
                        {
                            EffectType = "Penalty",
                            Target = "initiative",
                            Value = -1,
                            Description = "-1 to initiative"
                        }
                    ]
                },
                new TraumaConfigEntry
                {
                    Id = "reality-doubt",
                    Name = "Reality Doubt",
                    Type = "Existential",
                    Description = "Questioning whether anything is real.",
                    FlavorText = "Is this a dream?",
                    IsRetirementTrauma = true,
                    RetirementCondition = "5+",
                    IsStackable = true,
                    AcquisitionSources = ["ParadoxExposure"],
                    Triggers = [],
                    Effects = []
                }
            ]
        };

        _configProviderMock.Setup(c => c.GetTraumaConfiguration()).Returns(config);
    }

    #region GetTraumasAsync Tests

    [Test]
    public async Task GetTraumasAsync_ReturnsOrderedTraumas()
    {
        // Arrange
        var older = CharacterTrauma.Create(
            TestCharacterId,
            "combat-flashbacks",
            "CombatEvents",
            DateTime.UtcNow.AddDays(-5));

        var newer = CharacterTrauma.Create(
            TestCharacterId,
            "survivors-guilt",
            "AllyDeath",
            DateTime.UtcNow);

        _traumaRepositoryMock
            .Setup(r => r.GetByCharacterIdAsync(TestCharacterId))
            .ReturnsAsync(new List<CharacterTrauma> { newer, older }.AsReadOnly());

        // Act
        var result = await _service.GetTraumasAsync(TestCharacterId);

        // Assert
        result.Should().HaveCount(2);
        result[0].TraumaDefinitionId.Should().Be("combat-flashbacks");
        result[1].TraumaDefinitionId.Should().Be("survivors-guilt");
    }

    [Test]
    public async Task GetTraumasAsync_WithEmptyGuid_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => _service.GetTraumasAsync(Guid.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    #endregion

    #region AcquireTraumaAsync Tests

    [Test]
    public async Task AcquireTraumaAsync_NewTrauma_CreatesAndPersists()
    {
        // Arrange
        _playerRepositoryMock
            .Setup(r => r.GetByIdAsync(TestCharacterId, default))
            .ReturnsAsync(TestPlayer);

        _traumaRepositoryMock
            .Setup(r => r.GetByCharacterAndTraumaIdAsync(TestCharacterId, "survivors-guilt"))
            .ReturnsAsync((CharacterTrauma?)null);

        // Act
        var result = await _service.AcquireTraumaAsync(
            TestCharacterId,
            "survivors-guilt",
            "AllyDeath");

        // Assert
        result.Success.Should().BeTrue();
        result.TraumaId.Should().Be("survivors-guilt");
        result.IsNewTrauma.Should().BeTrue();
        result.TriggersRetirementCheck.Should().BeTrue();

        _traumaRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<CharacterTrauma>()),
            Times.Once);
        _traumaRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }

    [Test]
    public async Task AcquireTraumaAsync_StackableTrauma_IncrementsStack()
    {
        // Arrange
        var existing = CharacterTrauma.Create(
            TestCharacterId,
            "combat-flashbacks",
            "CombatEvents",
            DateTime.UtcNow.AddDays(-1));

        _playerRepositoryMock
            .Setup(r => r.GetByIdAsync(TestCharacterId, default))
            .ReturnsAsync(TestPlayer);

        _traumaRepositoryMock
            .Setup(r => r.GetByCharacterAndTraumaIdAsync(TestCharacterId, "combat-flashbacks"))
            .ReturnsAsync(existing);

        // Act
        var result = await _service.AcquireTraumaAsync(
            TestCharacterId,
            "combat-flashbacks",
            "CombatEvents");

        // Assert
        result.Success.Should().BeTrue();
        result.IsNewTrauma.Should().BeFalse();
        result.NewStackCount.Should().Be(2);

        _traumaRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<CharacterTrauma>()),
            Times.Once);
    }

    [Test]
    public async Task AcquireTraumaAsync_NonStackable_ReturnsAlreadyPresent()
    {
        // Arrange
        var existing = CharacterTrauma.Create(
            TestCharacterId,
            "survivors-guilt",
            "AllyDeath",
            DateTime.UtcNow.AddDays(-1));

        _playerRepositoryMock
            .Setup(r => r.GetByIdAsync(TestCharacterId, default))
            .ReturnsAsync(TestPlayer);

        _traumaRepositoryMock
            .Setup(r => r.GetByCharacterAndTraumaIdAsync(TestCharacterId, "survivors-guilt"))
            .ReturnsAsync(existing);

        // Act
        var result = await _service.AcquireTraumaAsync(
            TestCharacterId,
            "survivors-guilt",
            "AllyDeath");

        // Assert
        result.Success.Should().BeFalse();
        result.IsNewTrauma.Should().BeFalse();

        _traumaRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<CharacterTrauma>()),
            Times.Never);
    }

    #endregion

    #region CheckRetirementConditionAsync Tests

    [Test]
    public async Task CheckRetirementConditionAsync_ImmediateTrauma_MustRetire()
    {
        // Arrange
        var trauma = CharacterTrauma.Create(
            TestCharacterId,
            "survivors-guilt",
            "AllyDeath",
            DateTime.UtcNow);

        _traumaRepositoryMock
            .Setup(r => r.GetByCharacterIdAsync(TestCharacterId))
            .ReturnsAsync(new List<CharacterTrauma> { trauma }.AsReadOnly());

        // Act
        var result = await _service.CheckRetirementConditionAsync(TestCharacterId);

        // Assert
        result.MustRetire.Should().BeTrue();
        result.TraumasCausingRetirement.Should().Contain("survivors-guilt");
    }

    [Test]
    public async Task CheckRetirementConditionAsync_NoRetirementTraumas_NoRetirement()
    {
        // Arrange
        var trauma = CharacterTrauma.Create(
            TestCharacterId,
            "combat-flashbacks",
            "CombatEvents",
            DateTime.UtcNow);

        _traumaRepositoryMock
            .Setup(r => r.GetByCharacterIdAsync(TestCharacterId))
            .ReturnsAsync(new List<CharacterTrauma> { trauma }.AsReadOnly());

        // Act
        var result = await _service.CheckRetirementConditionAsync(TestCharacterId);

        // Assert
        result.MustRetire.Should().BeFalse();
        result.CanContinueWithPermission.Should().BeFalse();
    }

    #endregion
}
