// ═══════════════════════════════════════════════════════════════════════════════
// CoherenceServiceTests.cs
// Unit tests for CoherenceService — the core implementation of ICoherenceService
// managing coherence gain/loss, cascade checks, apotheosis, and meditation.
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
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class CoherenceServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST SETUP
    // ═══════════════════════════════════════════════════════════════

    private Mock<IPlayerRepository> _mockRepository = null!;
    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<IStressService> _mockStressService = null!;
    private Mock<ILogger<CoherenceService>> _mockLogger = null!;
    private CoherenceService _service = null!;
    private Player _testPlayer = null!;
    private Guid _characterId;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IPlayerRepository>();
        _mockDiceService = new Mock<IDiceService>();
        _mockStressService = new Mock<IStressService>();
        _mockLogger = new Mock<ILogger<CoherenceService>>();

        _characterId = Guid.NewGuid();
        _testPlayer = CreateTestPlayer(_characterId, initialCoherence: 50, isCombat: false);

        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => id == _characterId ? _testPlayer : null);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SaveResult.Succeeded(Guid.NewGuid()));

        // Setup default dice service behavior - returns 50 on 1d100
        _mockDiceService
            .Setup(d => d.RollTotal(It.IsAny<string>()))
            .Returns(50);

        _service = new CoherenceService(
            _mockRepository.Object,
            _mockDiceService.Object,
            _mockStressService.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_NullPlayerRepository_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CoherenceService(
            null!,
            _mockDiceService.Object,
            _mockStressService.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("playerRepository");
    }

    [Test]
    public void Constructor_NullDiceService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CoherenceService(
            _mockRepository.Object,
            null!,
            _mockStressService.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("diceService");
    }

    [Test]
    public void Constructor_NullStressService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CoherenceService(
            _mockRepository.Object,
            _mockDiceService.Object,
            null!,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("stressService");
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CoherenceService(
            _mockRepository.Object,
            _mockDiceService.Object,
            _mockStressService.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET COHERENCE STATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetCoherenceState_ReturnsCurrentState()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 55, isCombat: false);
        SetupPlayer(player);

        // Act
        var state = _service.GetCoherenceState(_characterId);

        // Assert
        state.Should().NotBeNull();
        state!.CurrentCoherence.Should().Be(55);
        state.Threshold.Should().Be(CoherenceThreshold.Balanced);
    }

    [Test]
    public void GetCoherenceState_PlayerNotFound_ThrowsCharacterNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();

        // Act
        var act = () => _service.GetCoherenceState(missingId);

        // Assert
        act.Should().Throw<CharacterNotFoundException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // GAIN COHERENCE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GainCoherence_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 40, isCombat: false);
        SetupPlayer(player);

        // Act
        var result = _service.GainCoherence(_characterId, 15, CoherenceSource.SuccessfulCast);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void GainCoherence_ReturnsFalse_WhenNoCoherenceState()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: null, isCombat: false);
        SetupPlayer(player);

        // Act
        var result = _service.GainCoherence(_characterId, 15, CoherenceSource.SuccessfulCast);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GainCoherence_IncreasesCoherenceValue()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 40, isCombat: false);
        SetupPlayer(player);

        // Act
        _service.GainCoherence(_characterId, 15, CoherenceSource.SuccessfulCast);

        // Assert
        var state = _service.GetCoherenceState(_characterId);
        state!.CurrentCoherence.Should().Be(55);
    }

    [Test]
    public void GainCoherence_CapsAt100()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 90, isCombat: false);
        SetupPlayer(player);

        // Act
        _service.GainCoherence(_characterId, 20, CoherenceSource.SuccessfulCast);

        // Assert
        var state = _service.GetCoherenceState(_characterId);
        state!.CurrentCoherence.Should().Be(100);
    }

    [Test]
    public void GainCoherence_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 50, isCombat: false);
        SetupPlayer(player);

        // Act
        var act = () => _service.GainCoherence(_characterId, -5, CoherenceSource.SuccessfulCast);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // LOSE COHERENCE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void LoseCoherence_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 60, isCombat: false);
        SetupPlayer(player);

        // Act
        var result = _service.LoseCoherence(_characterId, 15, "Failed spell");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void LoseCoherence_ReturnsFalse_WhenNoCoherenceState()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: null, isCombat: false);
        SetupPlayer(player);

        // Act
        var result = _service.LoseCoherence(_characterId, 15, "Failed spell");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void LoseCoherence_DecreasesCoherenceValue()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 60, isCombat: false);
        SetupPlayer(player);

        // Act
        _service.LoseCoherence(_characterId, 15, "Failed spell");

        // Assert
        var state = _service.GetCoherenceState(_characterId);
        state!.CurrentCoherence.Should().Be(45);
    }

    [Test]
    public void LoseCoherence_ClampsToZero()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 10, isCombat: false);
        SetupPlayer(player);

        // Act
        _service.LoseCoherence(_characterId, 30, "Cascade effect");

        // Assert
        var state = _service.GetCoherenceState(_characterId);
        state!.CurrentCoherence.Should().Be(0);
    }

    [Test]
    public void LoseCoherence_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 50, isCombat: false);
        SetupPlayer(player);

        // Act
        var act = () => _service.LoseCoherence(_characterId, -5, "Invalid");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void LoseCoherence_NullReason_ThrowsArgumentException()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 50, isCombat: false);
        SetupPlayer(player);

        // Act
        var act = () => _service.LoseCoherence(_characterId, 5, null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // CHECK CASCADE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CheckCascade_AtBalancedThreshold_ReturnsNoCascade()
    {
        // Arrange — Balanced has 0% cascade risk
        var player = CreateTestPlayer(_characterId, initialCoherence: 50, isCombat: false);
        SetupPlayer(player);

        // Act
        var result = _service.CheckCascade(_characterId);

        // Assert
        result.CascadeTriggered.Should().BeFalse();
    }

    [Test]
    public void CheckCascade_AtUnstable_RollAboveRisk_NoCascade()
    {
        // Arrange — Unstable has 10% cascade risk
        var player = CreateTestPlayer(_characterId, initialCoherence: 30, isCombat: false);
        SetupPlayer(player);
        _mockDiceService
            .Setup(d => d.RollTotal(It.IsAny<string>()))
            .Returns(50); // Above 10%

        // Act
        var result = _service.CheckCascade(_characterId);

        // Assert
        result.CascadeTriggered.Should().BeFalse();
    }

    [Test]
    public void CheckCascade_AtUnstable_RollBelowRisk_TriggersCascade()
    {
        // Arrange — Unstable has 10% cascade risk
        var player = CreateTestPlayer(_characterId, initialCoherence: 30, isCombat: false);
        SetupPlayer(player);
        _mockDiceService
            .Setup(d => d.RollTotal(It.IsAny<string>()))
            .Returns(5); // Below 10%

        // Act
        var result = _service.CheckCascade(_characterId);

        // Assert
        result.CascadeTriggered.Should().BeTrue();
        result.CoherenceLost.Should().BeGreaterThan(0);
        result.SpellDisrupted.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE APOTHEOSIS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void UpdateApotheosis_NotInApotheosis_ReturnsNoChange()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 70, isCombat: false);
        SetupPlayer(player);

        // Act
        var result = _service.UpdateApotheosis(_characterId);

        // Assert
        result.EnteredApotheosis.Should().BeFalse();
        result.StressCostPerTurn.Should().Be(0);
    }

    [Test]
    public void UpdateApotheosis_InApotheosis_AppliesStressCost()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 90, isCombat: false);
        SetupPlayer(player);

        // Act
        var result = _service.UpdateApotheosis(_characterId);

        // Assert
        result.StressCostPerTurn.Should().BeGreaterThan(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // MEDITATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Meditate_OutOfCombat_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 40, isCombat: false);
        SetupPlayer(player);

        // Act
        var result = _service.Meditate(_characterId);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Meditate_InCombat_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 40, isCombat: true);
        SetupPlayer(player);

        // Act
        var result = _service.Meditate(_characterId);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void Meditate_IncreasesCoherence()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 40, isCombat: false);
        SetupPlayer(player);

        // Act
        _service.Meditate(_characterId);

        // Assert
        var state = _service.GetCoherenceState(_characterId);
        state!.CurrentCoherence.Should().BeGreaterThan(40);
    }

    // ═══════════════════════════════════════════════════════════════
    // CAN MEDITATE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CanMeditate_OutOfCombat_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 50, isCombat: false);
        SetupPlayer(player);

        // Act
        var canMeditate = _service.CanMeditate(_characterId);

        // Assert
        canMeditate.Should().BeTrue();
    }

    [Test]
    public void CanMeditate_InCombat_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: 50, isCombat: true);
        SetupPlayer(player);

        // Act
        var canMeditate = _service.CanMeditate(_characterId);

        // Assert
        canMeditate.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // BONUS CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(10, -2)]   // Destabilized
    [TestCase(30, -1)]   // Unstable
    [TestCase(50, 0)]    // Balanced
    [TestCase(70, 2)]    // Focused
    [TestCase(90, 5)]    // Apotheosis
    public void GetSpellPowerBonus_ReturnsBonusByThreshold(int coherence, int expectedBonus)
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: coherence, isCombat: false);
        SetupPlayer(player);

        // Act
        var bonus = _service.GetSpellPowerBonus(_characterId);

        // Assert
        bonus.Should().Be(expectedBonus);
    }

    [Test]
    [TestCase(10, 0)]    // Destabilized
    [TestCase(30, 0)]    // Unstable
    [TestCase(50, 5)]    // Balanced
    [TestCase(70, 10)]   // Focused
    [TestCase(90, 20)]   // Apotheosis
    public void GetCriticalCastChance_ReturnsBonusByThreshold(int coherence, int expectedBonus)
    {
        // Arrange
        var player = CreateTestPlayer(_characterId, initialCoherence: coherence, isCombat: false);
        SetupPlayer(player);

        // Act
        var bonus = _service.GetCriticalCastChance(_characterId);

        // Assert
        bonus.Should().Be(expectedBonus);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private static Player CreateTestPlayer(Guid characterId, int? initialCoherence, bool isCombat)
    {
        var player = new Player("TestArcanist");
        // Set the ID via reflection for testing purposes
        typeof(Player).GetProperty("Id")!.SetValue(player, characterId);
        if (initialCoherence.HasValue)
        {
            player.SetCoherenceState(CoherenceState.Create(
                characterId,
                initialCoherence.Value,
                isCombat));
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
