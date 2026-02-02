// ═══════════════════════════════════════════════════════════════════════════════
// RageServiceTests.cs
// Unit tests for RageService — the core implementation of IRageService
// managing rage gain/decay, threshold-based bonuses, and special effects.
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
public class RageServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST SETUP
    // ═══════════════════════════════════════════════════════════════

    private Mock<IPlayerRepository> _mockRepository = null!;
    private Mock<ILogger<RageService>> _mockLogger = null!;
    private RageService _service = null!;
    private Player _testPlayer = null!;
    private Guid _characterId;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IPlayerRepository>();
        _mockLogger = new Mock<ILogger<RageService>>();

        _characterId = Guid.NewGuid();
        _testPlayer = CreateTestPlayer(_characterId, initialRage: 0);

        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => id == _characterId ? _testPlayer : null);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SaveResult.Succeeded(Guid.NewGuid()));

        _service = new RageService(
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
        var act = () => new RageService(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("playerRepository");
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RageService(_mockRepository.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET RAGE STATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetRageState_ReturnsCurrentState()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: 50);
        SetupPlayer(player);

        // Act
        var state = _service.GetRageState(_characterId);

        // Assert
        state.Should().NotBeNull();
        state!.CurrentRage.Should().Be(50);
        state.Threshold.Should().Be(RageThreshold.Burning);
    }

    [Test]
    public void GetRageState_CharacterNotFound_ThrowsCharacterNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();

        // Act
        var act = () => _service.GetRageState(missingId);

        // Assert
        act.Should().Throw<CharacterNotFoundException>();
    }

    [Test]
    public void GetRageState_NoRageState_ReturnsNull()
    {
        // Arrange — player with no rage state set
        var player = CreateTestPlayer(_characterId, initialRage: null);
        SetupPlayer(player);

        // Act
        var state = _service.GetRageState(_characterId);

        // Assert
        state.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GAIN RAGE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GainRage_ReturnsValidResult_WhenSuccessful()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: 20);
        SetupPlayer(player);

        // Act
        var result = _service.GainRage(_characterId, 50, RageSource.DealingDamage);

        // Assert
        result.Should().NotBeNull();
        result.AmountGained.Should().BeGreaterThan(0);
    }

    [Test]
    public void GainRage_CharacterNotFound_ThrowsCharacterNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();

        // Act
        var act = () => _service.GainRage(missingId, 50, RageSource.DealingDamage);

        // Assert
        act.Should().Throw<CharacterNotFoundException>();
    }

    [Test]
    public void GainRage_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: 0);
        SetupPlayer(player);

        // Act
        var act = () => _service.GainRage(_characterId, -5, RageSource.DealingDamage);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GainRage_IncreasesRageValue()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: 20);
        SetupPlayer(player);

        // Act
        var result = _service.GainRage(_characterId, 50, RageSource.DealingDamage);

        // Assert — result should show rage increased
        result.NewRage.Should().BeGreaterThan(20);
    }

    [Test]
    public void GainRage_CapsAt100()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: 95);
        SetupPlayer(player);

        // Act
        var result = _service.GainRage(_characterId, 100, RageSource.DealingDamage);

        // Assert
        result.NewRage.Should().Be(100);
    }

    [Test]
    public void GainRage_CrossingThreshold_SetsThresholdChangedTrue()
    {
        // Arrange — start at Calm edge (20), gain enough to cross to Simmering (21+)
        var player = CreateTestPlayer(_characterId, initialRage: 20);
        SetupPlayer(player);

        // Act — gain 5, should cross from Calm (0-20) to Simmering (21-40)
        var result = _service.GainRage(_characterId, 5, RageSource.TakingDamage);

        // Assert
        result.ThresholdChanged.Should().BeTrue();
        result.NewThreshold.Should().Be(RageThreshold.Simmering);
    }

    [Test]
    public void GainRage_PersistsChange_ToRepository()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: 0);
        SetupPlayer(player);

        // Act
        _service.GainRage(_characterId, 100, RageSource.DealingDamage);

        // Assert
        _mockRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // DECAY RAGE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ApplyDecay_ReturnsValidResult()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: 50);
        SetupPlayer(player);

        // Act
        var result = _service.ApplyDecay(_characterId);

        // Assert
        result.Should().NotBeNull();
        result.AmountDecayed.Should().BeGreaterThan(0);
    }

    [Test]
    public void ApplyDecay_ReducesRage()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: 50);
        SetupPlayer(player);

        // Act
        var result = _service.ApplyDecay(_characterId);

        // Assert
        result.NewRage.Should().BeLessThan(50);
    }

    [Test]
    public void ApplyDecay_ClampsToZero()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: 5);
        SetupPlayer(player);

        // Act
        var result = _service.ApplyDecay(_characterId);

        // Assert
        result.NewRage.Should().BeGreaterThanOrEqualTo(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // BONUS CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, 0)]
    [TestCase(10, 1)]
    [TestCase(25, 2)]
    [TestCase(50, 5)]
    [TestCase(99, 9)]
    [TestCase(100, 10)]
    public void GetDamageBonus_ReturnsFloorOfRageDividedBy10(int rage, int expectedBonus)
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: rage);
        SetupPlayer(player);

        // Act
        var bonus = _service.GetDamageBonus(_characterId);

        // Assert
        bonus.Should().Be(expectedBonus);
    }

    [Test]
    [TestCase(0, 0)]     // Calm
    [TestCase(25, 1)]    // Simmering
    [TestCase(45, 2)]    // Burning
    [TestCase(65, 3)]    // BerserkFury
    [TestCase(85, 4)]    // FrenzyBeyondReason
    public void GetSoakBonus_ReturnsBonusByThreshold(int rage, int expectedBonus)
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: rage);
        SetupPlayer(player);

        // Act
        var bonus = _service.GetSoakBonus(_characterId);

        // Assert
        bonus.Should().Be(expectedBonus);
    }

    // ═══════════════════════════════════════════════════════════════
    // SPECIAL STATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0, false)]
    [TestCase(50, false)]
    [TestCase(80, false)]
    [TestCase(81, true)]
    [TestCase(100, true)]
    public void IsFearImmune_TrueOnlyAtFrenzyBeyondReason(int rage, bool expected)
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: rage);
        SetupPlayer(player);

        // Act
        var isImmune = _service.IsFearImmune(_characterId);

        // Assert
        isImmune.Should().Be(expected);
    }

    [Test]
    [TestCase(0, false)]
    [TestCase(50, false)]
    [TestCase(60, false)]
    [TestCase(61, true)]
    [TestCase(85, true)]
    [TestCase(100, true)]
    public void MustAttackNearest_TrueAtBerserkFuryAndAbove(int rage, bool expected)
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: rage);
        SetupPlayer(player);

        // Act
        var mustAttack = _service.MustAttackNearest(_characterId);

        // Assert
        mustAttack.Should().Be(expected);
    }

    [Test]
    public void GetPartyStressReduction_Returns10AtFrenzyBeyondReason()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: 85);
        SetupPlayer(player);

        // Act
        var reduction = _service.GetPartyStressReduction(_characterId);

        // Assert
        reduction.Should().Be(10);
    }

    [Test]
    public void GetPartyStressReduction_ReturnsNullBelowFrenzyBeyondReason()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialRage: 75);
        SetupPlayer(player);

        // Act
        var reduction = _service.GetPartyStressReduction(_characterId);

        // Assert
        reduction.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private static Player CreateTestPlayer(Guid characterId, int? initialRage)
    {
        var player = new Player("TestBerserker");
        // Set the ID via reflection for testing purposes
        typeof(Player).GetProperty("Id")!.SetValue(player, characterId);
        if (initialRage.HasValue)
        {
            player.SetRageState(RageState.Create(characterId, initialRage.Value));
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
