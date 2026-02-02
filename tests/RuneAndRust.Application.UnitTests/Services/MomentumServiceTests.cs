// ═══════════════════════════════════════════════════════════════════════════════
// MomentumServiceTests.cs
// Unit tests for MomentumService — the core implementation of IMomentumService
// managing momentum gain/decay, chain bonuses, and threshold-based effects.
// Version: 0.18.4e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Exceptions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class MomentumServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST SETUP
    // ═══════════════════════════════════════════════════════════════

    private Mock<IPlayerRepository> _mockRepository = null!;
    private Mock<ILogger<MomentumService>> _mockLogger = null!;
    private MomentumService _service = null!;
    private Player _testPlayer = null!;
    private Guid _characterId;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IPlayerRepository>();
        _mockLogger = new Mock<ILogger<MomentumService>>();

        _characterId = Guid.NewGuid();
        _testPlayer = CreateTestPlayer(_characterId, initialMomentum: 0, consecutiveHits: 0);

        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => id == _characterId ? _testPlayer : null);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SaveResult.Succeeded(Guid.NewGuid()));

        _service = new MomentumService(
            _mockRepository.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_NullPlayerRepository_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MomentumService(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("playerRepository");
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MomentumService(_mockRepository.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET MOMENTUM STATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetMomentumState_ReturnsCurrentState()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 50, consecutiveHits: 3);
        SetupPlayer(player);

        // Act
        var state = _service.GetMomentumState(_characterId);

        // Assert
        state.Should().NotBeNull();
        state!.CurrentMomentum.Should().Be(50);
        state.ConsecutiveHits.Should().Be(3);
        state.Threshold.Should().Be(MomentumThreshold.Flowing);
    }

    [Test]
    public void GetMomentumState_PlayerNotFound_ThrowsCharacterNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();

        // Act
        var act = () => _service.GetMomentumState(missingId);

        // Assert
        act.Should().Throw<CharacterNotFoundException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // GAIN MOMENTUM TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GainMomentum_ReturnsValidResult_WhenSuccessful()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 20, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var result = _service.GainMomentum(_characterId, 10, MomentumSource.SuccessfulAttack);

        // Assert
        result.Should().NotBeNull();
        result.AmountGained.Should().BeGreaterThan(0);
    }

    [Test]
    public void GainMomentum_IncreasesMomentumValue()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 20, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var result = _service.GainMomentum(_characterId, 10, MomentumSource.SuccessfulAttack);

        // Assert
        result.NewMomentum.Should().BeGreaterThan(20);
    }

    [Test]
    public void GainMomentum_CapsAt100()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 95, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var result = _service.GainMomentum(_characterId, 20, MomentumSource.SuccessfulAttack);

        // Assert
        result.NewMomentum.Should().Be(100);
    }

    [Test]
    public void GainMomentum_CrossingThreshold_SetsThresholdChangedTrue()
    {
        // Arrange — start at Stationary (0-20), gain enough to cross to Moving (21-40)
        var player = CreateTestPlayer(_characterId, initialMomentum: 18, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var result = _service.GainMomentum(_characterId, 10, MomentumSource.SuccessfulAttack);

        // Assert
        result.ThresholdChanged.Should().BeTrue();
        result.NewThreshold.Should().Be(MomentumThreshold.Moving);
    }

    [Test]
    public void GainMomentum_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 0, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var act = () => _service.GainMomentum(_characterId, -5, MomentumSource.SuccessfulAttack);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // DECAY MOMENTUM TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ApplyDecay_ReturnsValidResult()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 50, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var result = _service.ApplyDecay(_characterId, "No Action");

        // Assert
        result.Should().NotBeNull();
        result.AmountDecayed.Should().BeGreaterThan(0);
    }

    [Test]
    public void ApplyDecay_ReducesMomentum()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 50, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var result = _service.ApplyDecay(_characterId, "No Action");

        // Assert
        result.NewMomentum.Should().BeLessThan(50);
    }

    [Test]
    public void ApplyDecay_ClampsToZero()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 5, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var result = _service.ApplyDecay(_characterId, "No Action");

        // Assert
        result.NewMomentum.Should().BeGreaterThanOrEqualTo(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // RESET MOMENTUM TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ResetMomentum_ReturnsValidResult()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 75, consecutiveHits: 5);
        SetupPlayer(player);

        // Act
        var result = _service.ResetMomentum(_characterId, "Stunned");

        // Assert
        result.Should().NotBeNull();
        result.ChainBroken.Should().BeTrue();
    }

    [Test]
    public void ResetMomentum_SetsMomentumToZero()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 75, consecutiveHits: 5);
        SetupPlayer(player);

        // Act
        var result = _service.ResetMomentum(_characterId, "Stunned");

        // Assert
        result.NewMomentum.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // RECORD HIT/MISS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RecordHit_IncrementsConsecutiveHits()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 50, consecutiveHits: 2);
        SetupPlayer(player);

        // Act
        _service.RecordHit(_characterId);

        // Assert
        var state = _service.GetMomentumState(_characterId);
        state!.ConsecutiveHits.Should().Be(3);
    }

    [Test]
    public void RecordMiss_ResetsConsecutiveHits()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: 50, consecutiveHits: 5);
        SetupPlayer(player);

        // Act
        _service.RecordMiss(_characterId);

        // Assert
        var state = _service.GetMomentumState(_characterId);
        state!.ConsecutiveHits.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // BONUS CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 0)]
    [TestCase(25, 1)]
    [TestCase(50, 2)]
    [TestCase(75, 3)]
    [TestCase(100, 4)]
    public void GetAttackBonus_ReturnsBonusByMomentum(int momentum, int expectedBonus)
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: momentum, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var bonus = _service.GetAttackBonus(_characterId);

        // Assert
        bonus.Should().Be(expectedBonus);
    }

    [Test]
    [TestCase(0, 0)]
    [TestCase(20, 1)]
    [TestCase(40, 2)]
    [TestCase(60, 3)]
    [TestCase(80, 4)]
    [TestCase(100, 5)]
    public void GetMovementBonus_ReturnsBonusByMomentum(int momentum, int expectedBonus)
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: momentum, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var bonus = _service.GetMovementBonus(_characterId);

        // Assert
        bonus.Should().Be(expectedBonus);
    }

    [Test]
    [TestCase(0, 0)]     // Stationary
    [TestCase(25, 1)]    // Moving
    [TestCase(45, 2)]    // Flowing
    [TestCase(65, 3)]    // Surging
    [TestCase(85, 4)]    // Unstoppable
    public void GetDefenseBonus_ReturnsBonusByThreshold(int momentum, int expectedBonus)
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: momentum, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var bonus = _service.GetDefenseBonus(_characterId);

        // Assert
        bonus.Should().Be(expectedBonus);
    }

    [Test]
    [TestCase(0, 0)]     // Stationary
    [TestCase(25, 0)]    // Moving
    [TestCase(45, 1)]    // Flowing (1 bonus attack)
    [TestCase(65, 1)]    // Surging (1 bonus attack)
    [TestCase(85, 2)]    // Unstoppable (2 bonus attacks)
    public void GetBonusAttacks_ReturnsBonusByThreshold(int momentum, int expectedBonusAttacks)
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialMomentum: momentum, consecutiveHits: 0);
        SetupPlayer(player);

        // Act
        var bonusAttacks = _service.GetBonusAttacks(_characterId);

        // Assert
        bonusAttacks.Should().Be(expectedBonusAttacks);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private static Player CreateTestPlayer(Guid characterId, int? initialMomentum, int consecutiveHits)
    {
        var player = new Player("TestStormBlade");
        // Set the ID via reflection for testing purposes
        typeof(Player).GetProperty("Id")!.SetValue(player, characterId);
        if (initialMomentum.HasValue)
        {
            player.SetMomentumState(MomentumState.Create(
                characterId,
                initialMomentum.Value,
                consecutiveHits));
        }
        return player;
    }

    private void SetupPlayer(Player player)
    {
        _testPlayer = player;
        _mockRepository
            .Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
    }
}
