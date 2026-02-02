// ═══════════════════════════════════════════════════════════════════════════════
// UnifiedRestHandlerTests.cs
// Unit tests for UnifiedRestHandler — the unified rest processing service
// that coordinates across all trauma economy systems.
// Version: 0.18.5c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Records;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="UnifiedRestHandler"/> service.
/// </summary>
[TestFixture]
public class UnifiedRestHandlerTests
{
    // ═══════════════════════════════════════════════════════════════
    // Test Setup
    // ═══════════════════════════════════════════════════════════════

    private Mock<IStressService> _mockStressService = null!;
    private Mock<ITraumaService> _mockTraumaService = null!;
    private Mock<IRageService> _mockRageService = null!;
    private Mock<IMomentumService> _mockMomentumService = null!;
    private Mock<ICoherenceService> _mockCoherenceService = null!;
    private Mock<ILogger<UnifiedRestHandler>> _mockLogger = null!;
    private UnifiedRestHandler _handler = null!;
    private Guid _characterId;

    [SetUp]
    public void SetUp()
    {
        _mockStressService = new Mock<IStressService>();
        _mockTraumaService = new Mock<ITraumaService>();
        _mockRageService = new Mock<IRageService>();
        _mockMomentumService = new Mock<IMomentumService>();
        _mockCoherenceService = new Mock<ICoherenceService>();
        _mockLogger = new Mock<ILogger<UnifiedRestHandler>>();

        _characterId = Guid.NewGuid();

        // Default stress state: 50 stress
        var stressState = StressState.Create(50);
        _mockStressService
            .Setup(s => s.GetStressState(It.IsAny<Guid>()))
            .Returns(stressState);

        // Default stress recovery result
        var recoveryResult = StressRecoveryResult.Create(
            previousStress: 50,
            newStress: 25,
            recoverySource: RestType.Long);
        _mockStressService
            .Setup(s => s.RecoverStress(It.IsAny<Guid>(), It.IsAny<RestType>()))
            .Returns(recoveryResult);

        // Default trauma check result (passed = no trauma acquired)
        var traumaCheckResult = TraumaCheckResult.CreatePassed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.ProlongedExposure,
            diceRolled: 3,
            successesNeeded: 2,
            successesAchieved: 3);
        _mockTraumaService
            .Setup(t => t.PerformTraumaCheckAsync(It.IsAny<Guid>(), It.IsAny<TraumaCheckTrigger>()))
            .ReturnsAsync(traumaCheckResult);

        // Default: no specialization states (null)
        _mockRageService
            .Setup(r => r.GetRageState(It.IsAny<Guid>()))
            .Returns((RageState?)null);

        _mockMomentumService
            .Setup(m => m.GetMomentumState(It.IsAny<Guid>()))
            .Returns((MomentumState?)null);

        _mockCoherenceService
            .Setup(c => c.GetCoherenceState(It.IsAny<Guid>()))
            .Returns((CoherenceState?)null);

        _handler = new UnifiedRestHandler(
            _mockStressService.Object,
            _mockTraumaService.Object,
            _mockRageService.Object,
            _mockMomentumService.Object,
            _mockCoherenceService.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_NullStressService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new UnifiedRestHandler(
            null!,
            _mockTraumaService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("stressService");
    }

    [Test]
    public void Constructor_NullTraumaService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new UnifiedRestHandler(
            _mockStressService.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("traumaService");
    }

    [Test]
    public void Constructor_WithOptionalDependenciesNull_CreatesInstance()
    {
        // Act
        var handler = new UnifiedRestHandler(
            _mockStressService.Object,
            _mockTraumaService.Object,
            rageService: null,
            momentumService: null,
            coherenceService: null,
            logger: null);

        // Assert
        handler.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessRest Tests — Basic Flow
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessRest_BasicFlow_ReturnsValidResult()
    {
        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Long);

        // Assert
        result.Should().NotBeNull();
        result.RestType.Should().Be(RestType.Long);
        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void ProcessRest_ShortRest_CallsStressService()
    {
        // Act
        _handler.ProcessRest(_characterId, RestType.Short);

        // Assert
        _mockStressService.Verify(
            s => s.RecoverStress(_characterId, RestType.Short),
            Times.Once);
    }

    [Test]
    public void ProcessRest_LongRest_CallsStressServiceAndTraumaService()
    {
        // Act
        _handler.ProcessRest(_characterId, RestType.Long);

        // Assert
        _mockStressService.Verify(
            s => s.RecoverStress(_characterId, RestType.Long),
            Times.Once);
        _mockTraumaService.Verify(
            t => t.PerformTraumaCheckAsync(_characterId, TraumaCheckTrigger.ProlongedExposure),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessRest Tests — Stress Recovery
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessRest_LongRest_RecordsCorrectStressRecovery()
    {
        // Arrange — recovery of 25 stress
        var recoveryResult = StressRecoveryResult.Create(
            previousStress: 50,
            newStress: 25,
            recoverySource: RestType.Long);
        _mockStressService
            .Setup(s => s.RecoverStress(_characterId, RestType.Long))
            .Returns(recoveryResult);

        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Long);

        // Assert
        result.StressRecovered.Should().Be(25);
        result.StressRecoveryFormula.Should().Contain("Long");
    }

    [Test]
    public void ProcessRest_Sanctuary_RecordsFullReset()
    {
        // Arrange — full reset from 75 to 0
        var recoveryResult = StressRecoveryResult.Create(
            previousStress: 75,
            newStress: 0,
            recoverySource: RestType.Sanctuary);
        _mockStressService
            .Setup(s => s.RecoverStress(_characterId, RestType.Sanctuary))
            .Returns(recoveryResult);

        // Updated stress state after recovery
        var newStressState = StressState.Create(0);
        _mockStressService
            .SetupSequence(s => s.GetStressState(_characterId))
            .Returns(StressState.Create(75))  // Before
            .Returns(newStressState);         // After

        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Sanctuary);

        // Assert
        result.StressRecovered.Should().Be(75);
        result.IsFullRecovery.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessRest Tests — Trauma Checks
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessRest_ShortRest_NoTraumaCheck()
    {
        // Act
        _handler.ProcessRest(_characterId, RestType.Short);

        // Assert
        _mockTraumaService.Verify(
            t => t.PerformTraumaCheckAsync(It.IsAny<Guid>(), It.IsAny<TraumaCheckTrigger>()),
            Times.Never);
    }

    [Test]
    public void ProcessRest_LongRest_PerformsTraumaCheck()
    {
        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Long);

        // Assert
        result.TraumaChecksPerformed.Should().Be(1);
        _mockTraumaService.Verify(
            t => t.PerformTraumaCheckAsync(_characterId, TraumaCheckTrigger.ProlongedExposure),
            Times.Once);
    }

    [Test]
    public void ProcessRest_TraumaAcquired_RecordsInResult()
    {
        // Arrange — trauma acquired during rest (failed check)
        var traumaResult = TraumaCheckResult.CreateFailed(
            characterId: _characterId,
            trigger: TraumaCheckTrigger.ProlongedExposure,
            diceRolled: 2,
            successesNeeded: 2,
            successesAchieved: 1,
            traumaAcquired: "paranoia");
        _mockTraumaService
            .Setup(t => t.PerformTraumaCheckAsync(_characterId, TraumaCheckTrigger.ProlongedExposure))
            .ReturnsAsync(traumaResult);

        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Long);

        // Assert
        result.AcquiredNewTraumas.Should().BeTrue();
        result.TraumasAcquired.Should().Contain("paranoia");
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessRest Tests — Specialization Resource Resets
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessRest_StormBladeWithNoMomentum_DoesNotResetMomentum()
    {
        // Arrange — Storm Blade with 0 momentum
        var momentumState = MomentumState.Create(_characterId);
        _mockMomentumService
            .Setup(m => m.GetMomentumState(_characterId))
            .Returns(momentumState);

        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Short);

        // Assert — momentum reset not triggered because CurrentMomentum is 0
        result.MomentumReset.Should().BeFalse();
    }

    [Test]
    public void ProcessRest_ArcanistLongRest_RestoresCoherence()
    {
        // Arrange — Arcanist with coherence at 30
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 30);
        _mockCoherenceService
            .Setup(c => c.GetCoherenceState(_characterId))
            .Returns(coherenceState);
        _mockCoherenceService
            .Setup(c => c.GainCoherence(_characterId, It.IsAny<int>(), CoherenceSource.MeditationAction))
            .Returns(true);

        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Long);

        // Assert
        result.CoherenceRestored.Should().Be(20); // 50 - 30 = 20
        _mockCoherenceService.Verify(
            c => c.GainCoherence(_characterId, 20, CoherenceSource.MeditationAction),
            Times.Once);
    }

    [Test]
    public void ProcessRest_ArcanistShortRest_NoCoherenceChange()
    {
        // Arrange — Arcanist with coherence at 30
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 30);
        _mockCoherenceService
            .Setup(c => c.GetCoherenceState(_characterId))
            .Returns(coherenceState);

        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Short);

        // Assert
        result.CoherenceRestored.Should().BeNull();
        _mockCoherenceService.Verify(
            c => c.GainCoherence(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CoherenceSource>()),
            Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessRest Tests — Party Effects
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessRest_BerserkerInFrenzy_AppliesPartyBonus()
    {
        // Arrange
        var berserkerId = Guid.NewGuid();
        var partyContext = new PartyContext(
            PartyMemberIds: new[] { _characterId, berserkerId },
            BerserkerId: berserkerId);

        _mockRageService
            .Setup(r => r.GetPartyStressReduction(berserkerId))
            .Returns(10);

        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Long, partyContext);

        // Assert
        result.RagePartyBonus.Should().Be(10);
        result.HasPartyBonus.Should().BeTrue();
    }

    [Test]
    public void ProcessRest_NoBerserkerInFrenzy_NoPartyBonus()
    {
        // Arrange — party without a Berserker in frenzy
        var partyContext = PartyContext.WithMembers(_characterId);

        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Long, partyContext);

        // Assert
        result.RagePartyBonus.Should().BeNull();
        result.HasPartyBonus.Should().BeFalse();
    }

    [Test]
    public void ProcessRest_ShortRest_NoPartyBonus()
    {
        // Arrange — party with Berserker, but Short Rest doesn't grant bonus
        var berserkerId = Guid.NewGuid();
        var partyContext = new PartyContext(
            PartyMemberIds: new[] { _characterId, berserkerId },
            BerserkerId: berserkerId);

        _mockRageService
            .Setup(r => r.GetPartyStressReduction(berserkerId))
            .Returns(10);

        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Short, partyContext);

        // Assert
        result.RagePartyBonus.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessRest Tests — CPS Stage Changes
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessRest_CpsStageImproves_RecordsChange()
    {
        // Arrange — stress drops from 50 (GlimmerMadness) to 15 (None)
        var beforeState = StressState.Create(50);
        var afterState = StressState.Create(15);

        _mockStressService
            .SetupSequence(s => s.GetStressState(_characterId))
            .Returns(beforeState)
            .Returns(afterState);

        var recoveryResult = StressRecoveryResult.Create(
            previousStress: 50,
            newStress: 15,
            recoverySource: RestType.Sanctuary);
        _mockStressService
            .Setup(s => s.RecoverStress(_characterId, RestType.Sanctuary))
            .Returns(recoveryResult);

        // Act
        var result = _handler.ProcessRest(_characterId, RestType.Sanctuary);

        // Assert
        result.CpsStageChanged.Should().BeTrue();
        result.NewCpsStage.Should().Be(CpsStage.None);
    }

    // ═══════════════════════════════════════════════════════════════
    // RestIntegrationResult Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RestIntegrationResult_Create_SetsDefaults()
    {
        // Act
        var result = RestIntegrationResult.Create(
            restType: RestType.Long,
            stressRecovered: 25,
            stressRecoveryFormula: "WILL × 5");

        // Assert
        result.CorruptionRecovered.Should().Be(0);
        result.CpsStageChanged.Should().BeFalse();
        result.TraumaChecksPerformed.Should().Be(0);
        result.TraumasAcquired.Should().BeEmpty();
        result.MomentumReset.Should().BeTrue();
    }

    [Test]
    public void RestIntegrationResult_IsValid_ReturnsTrueForValidResult()
    {
        // Arrange
        var result = RestIntegrationResult.Create(
            restType: RestType.Long,
            stressRecovered: 25,
            stressRecoveryFormula: "WILL × 5");

        // Assert
        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void RestIntegrationResult_IsValid_ReturnsFalseForNegativeStress()
    {
        // Arrange — manually construct with negative value
        var result = new RestIntegrationResult(
            RestType: RestType.Long,
            StressRecovered: -10,
            StressRecoveryFormula: "WILL × 5",
            CorruptionRecovered: 0,
            CpsStageChanged: false,
            NewCpsStage: null,
            TraumaChecksPerformed: 0,
            TraumasAcquired: null,
            RagePartyBonus: null,
            CoherenceRestored: null,
            MomentumReset: false,
            RecoveryMessages: new List<string>().AsReadOnly());

        // Assert
        result.IsValid().Should().BeFalse();
    }

    [Test]
    public void RestIntegrationResult_Empty_ReturnsMinimalResult()
    {
        // Act
        var result = RestIntegrationResult.Empty(RestType.Short);

        // Assert
        result.RestType.Should().Be(RestType.Short);
        result.StressRecovered.Should().Be(0);
        result.StressRecoveryFormula.Should().Be("Interrupted");
        result.MomentumReset.Should().BeFalse();
    }

    [Test]
    public void RestIntegrationResult_IsSignificantRecovery_TrueForHighStress()
    {
        // Arrange
        var result = RestIntegrationResult.Create(
            restType: RestType.Sanctuary,
            stressRecovered: 50,
            stressRecoveryFormula: "Full Reset");

        // Assert
        result.IsSignificantRecovery.Should().BeTrue();
    }

    [Test]
    public void RestIntegrationResult_AcquiredNewTraumas_DetectsTraumaList()
    {
        // Arrange
        var result = RestIntegrationResult.Create(
            restType: RestType.Long,
            stressRecovered: 25,
            stressRecoveryFormula: "WILL × 5",
            traumasAcquired: new List<string> { "paranoia" }.AsReadOnly());

        // Assert
        result.AcquiredNewTraumas.Should().BeTrue();
        result.TraumasAcquired.Should().Contain("paranoia");
    }
}
