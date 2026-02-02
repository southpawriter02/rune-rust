// ═══════════════════════════════════════════════════════════════════════════════
// UnifiedTurnHandlerTests.cs
// Unit tests for the UnifiedTurnHandler service.
// Tests turn start processing (resource decay, Apotheosis), turn end processing
// (panic checks, CPS effects, environmental stress, trauma triggers), and
// constructor validation.
// Version: 0.18.5d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="UnifiedTurnHandler"/>.
/// </summary>
[TestFixture]
public class UnifiedTurnHandlerTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private Mock<IStressService> _mockStressService = null!;
    private Mock<ITraumaService> _mockTraumaService = null!;
    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<ICpsService> _mockCpsService = null!;
    private Mock<ILogger<UnifiedTurnHandler>> _mockLogger = null!;
    private Mock<IRageService> _mockRageService = null!;
    private Mock<IMomentumService> _mockMomentumService = null!;
    private Mock<ICoherenceService> _mockCoherenceService = null!;

    private UnifiedTurnHandler _handler = null!;

    private static readonly Guid TestCharacterId = Guid.NewGuid();

    // ═══════════════════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════════════════

    [SetUp]
    public void Setup()
    {
        _mockStressService = new Mock<IStressService>();
        _mockTraumaService = new Mock<ITraumaService>();
        _mockDiceService = new Mock<IDiceService>();
        _mockCpsService = new Mock<ICpsService>();
        _mockLogger = new Mock<ILogger<UnifiedTurnHandler>>();
        _mockRageService = new Mock<IRageService>();
        _mockMomentumService = new Mock<IMomentumService>();
        _mockCoherenceService = new Mock<ICoherenceService>();

        // Default behaviors
        _mockCpsService
            .Setup(s => s.GetCurrentStage(It.IsAny<Guid>()))
            .Returns(CpsStage.None);

        _mockStressService
            .Setup(s => s.RequiresTraumaCheck(It.IsAny<Guid>()))
            .Returns(false);

        _handler = new UnifiedTurnHandler(
            _mockStressService.Object,
            _mockTraumaService.Object,
            _mockDiceService.Object,
            _mockCpsService.Object,
            _mockLogger.Object,
            _mockRageService.Object,
            _mockMomentumService.Object,
            _mockCoherenceService.Object);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullStressService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new UnifiedTurnHandler(
            null!,
            _mockTraumaService.Object,
            _mockDiceService.Object,
            _mockCpsService.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("stressService");
    }

    [Test]
    public void Constructor_WithNullTraumaService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new UnifiedTurnHandler(
            _mockStressService.Object,
            null!,
            _mockDiceService.Object,
            _mockCpsService.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("traumaService");
    }

    [Test]
    public void Constructor_WithNullDiceService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new UnifiedTurnHandler(
            _mockStressService.Object,
            _mockTraumaService.Object,
            null!,
            _mockCpsService.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("diceService");
    }

    [Test]
    public void Constructor_WithNullCpsService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new UnifiedTurnHandler(
            _mockStressService.Object,
            _mockTraumaService.Object,
            _mockDiceService.Object,
            null!,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cpsService");
    }

    [Test]
    public void Constructor_WithNullOptionalServices_Succeeds()
    {
        // Arrange & Act
        var handler = new UnifiedTurnHandler(
            _mockStressService.Object,
            _mockTraumaService.Object,
            _mockDiceService.Object,
            _mockCpsService.Object,
            _mockLogger.Object);

        // Assert
        handler.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TURN START — RESOURCE DECAY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ProcessTurnStart_OutOfCombat_AppliesRageDecay()
    {
        // Arrange
        var rageState = RageState.Create(TestCharacterId, initialRage: 50);
        var decayResult = new RageDecayResult(
            PreviousRage: 50,
            NewRage: 40,
            AmountDecayed: 10,
            ThresholdChanged: false,
            NewThreshold: null);

        _mockRageService
            .Setup(s => s.GetRageState(TestCharacterId))
            .Returns(rageState);
        _mockRageService
            .Setup(s => s.ApplyDecay(TestCharacterId))
            .Returns(decayResult);

        // Act
        var result = _handler.ProcessTurnStart(TestCharacterId, isInCombat: false);

        // Assert
        result.RageDecay.Should().NotBeNull();
        result.RageDecay!.Should().BeEquivalentTo(decayResult);
        _mockRageService.Verify(s => s.ApplyDecay(TestCharacterId), Times.Once);
    }

    [Test]
    public void ProcessTurnStart_OutOfCombat_AppliesMomentumDecay()
    {
        // Arrange
        var momentumState = MomentumState.Create(TestCharacterId, initialMomentum: 60);
        var decayResult = new MomentumDecayResult(
            PreviousMomentum: 60,
            NewMomentum: 45,
            AmountDecayed: 15,
            DecayReason: "No combat action",
            ChainBroken: false,
            ThresholdChanged: true,
            NewThreshold: MomentumThreshold.Flowing);

        _mockMomentumService
            .Setup(s => s.GetMomentumState(TestCharacterId))
            .Returns(momentumState);
        _mockMomentumService
            .Setup(s => s.ApplyDecay(TestCharacterId, "No combat action"))
            .Returns(decayResult);

        // Act
        var result = _handler.ProcessTurnStart(TestCharacterId, isInCombat: false);

        // Assert
        result.MomentumDecay.Should().NotBeNull();
        result.MomentumDecay!.Should().BeEquivalentTo(decayResult);
        _mockMomentumService.Verify(s => s.ApplyDecay(TestCharacterId, "No combat action"), Times.Once);
    }

    [Test]
    public void ProcessTurnStart_InCombat_SkipsResourceDecay()
    {
        // Arrange
        var rageState = RageState.Create(TestCharacterId, initialRage: 50);
        _mockRageService
            .Setup(s => s.GetRageState(TestCharacterId))
            .Returns(rageState);

        // Act
        var result = _handler.ProcessTurnStart(TestCharacterId, isInCombat: true);

        // Assert
        result.RageDecay.Should().BeNull();
        result.MomentumDecay.Should().BeNull();
        _mockRageService.Verify(s => s.ApplyDecay(It.IsAny<Guid>()), Times.Never);
        _mockMomentumService.Verify(s => s.ApplyDecay(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TURN START — APOTHEOSIS TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ProcessTurnStart_InApotheosis_AppliesStressCost()
    {
        // Arrange
        // Coherence 85 puts character in Apotheosis threshold (81+)
        var coherenceState = CoherenceState.Create(TestCharacterId, initialCoherence: 85);
        var stressResult = StressApplicationResult.Create(
            previousStress: 50,
            newStress: 60,
            source: StressSource.ApotheosisStrain);

        _mockCoherenceService
            .Setup(s => s.GetCoherenceState(TestCharacterId))
            .Returns(coherenceState);
        _mockStressService
            .Setup(s => s.ApplyStress(TestCharacterId, 10, StressSource.ApotheosisStrain, 0))
            .Returns(stressResult);

        // Act
        var result = _handler.ProcessTurnStart(TestCharacterId, isInCombat: false);

        // Assert
        result.ApotheosisActive.Should().BeTrue();
        result.ApotheosisStressCost.Should().Be(10);
        result.AutoExitedApotheosis.Should().BeFalse();
        _mockStressService.Verify(
            s => s.ApplyStress(TestCharacterId, 10, StressSource.ApotheosisStrain, 0),
            Times.Once);
    }

    [Test]
    public void ProcessTurnStart_ApotheosisAtMaxStress_AutoExits()
    {
        // Arrange
        // Coherence 85 puts character in Apotheosis threshold (81+)
        var coherenceState = CoherenceState.Create(TestCharacterId, initialCoherence: 85);
        var stressResult = StressApplicationResult.Create(
            previousStress: 90,
            newStress: 100,
            source: StressSource.ApotheosisStrain);

        _mockCoherenceService
            .Setup(s => s.GetCoherenceState(TestCharacterId))
            .Returns(coherenceState);
        _mockStressService
            .Setup(s => s.ApplyStress(TestCharacterId, 10, StressSource.ApotheosisStrain, 0))
            .Returns(stressResult);

        // Act
        var result = _handler.ProcessTurnStart(TestCharacterId, isInCombat: false);

        // Assert
        result.ApotheosisActive.Should().BeTrue();
        result.AutoExitedApotheosis.Should().BeTrue();
        result.ApotheosisExitReason.Should().Contain("100");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TURN END — PANIC CHECK TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ProcessTurnEnd_AtRuinMadness_TriggersPanicCheck()
    {
        // Arrange
        _mockCpsService
            .Setup(s => s.GetCurrentStage(TestCharacterId))
            .Returns(CpsStage.RuinMadness);

        var panicResult = PanicResult.Frozen();

        _mockCpsService
            .Setup(s => s.RollPanicTable(TestCharacterId))
            .Returns(panicResult);

        // Act
        var result = _handler.ProcessTurnEnd(TestCharacterId);

        // Assert
        result.PanicCheckPerformed.Should().BeTrue();
        result.PanicEffectApplied.Should().Be(PanicEffect.Frozen);
        _mockCpsService.Verify(s => s.RollPanicTable(TestCharacterId), Times.Once);
        _mockCpsService.Verify(s => s.ApplyPanicEffect(TestCharacterId, panicResult), Times.Once);
    }

    [Test]
    public void ProcessTurnEnd_NotInRuinMadness_SkipsPanicCheck()
    {
        // Arrange
        _mockCpsService
            .Setup(s => s.GetCurrentStage(TestCharacterId))
            .Returns(CpsStage.GlimmerMadness);

        // Act
        var result = _handler.ProcessTurnEnd(TestCharacterId);

        // Assert
        result.PanicCheckPerformed.Should().BeFalse();
        result.PanicEffectApplied.Should().BeNull();
        _mockCpsService.Verify(s => s.RollPanicTable(It.IsAny<Guid>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TURN END — ENVIRONMENTAL STRESS AND TRAUMA TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ProcessTurnEnd_WithEnvironmentalStress_AppliesStress()
    {
        // Arrange
        var stressResult = StressApplicationResult.Create(
            previousStress: 30,
            newStress: 33,
            source: StressSource.Environmental);
        _mockStressService
            .Setup(s => s.ApplyStress(TestCharacterId, 3, StressSource.Environmental, 0))
            .Returns(stressResult);

        // Act
        var result = _handler.ProcessTurnEnd(TestCharacterId, environmentalStress: 3);

        // Assert
        result.EnvironmentalStressApplied.Should().Be(3);
        _mockStressService.Verify(
            s => s.ApplyStress(TestCharacterId, 3, StressSource.Environmental, 0),
            Times.Once);
    }

    [Test]
    public void ProcessTurnEnd_AtMaxStress_TriggersTraumaCheck()
    {
        // Arrange
        _mockStressService
            .Setup(s => s.RequiresTraumaCheck(TestCharacterId))
            .Returns(true);

        // Act
        var result = _handler.ProcessTurnEnd(TestCharacterId);

        // Assert
        result.TraumaCheckTriggered.Should().BeTrue();
    }

    [Test]
    public void ProcessTurnEnd_EnvironmentalStressCappedAt5()
    {
        // Arrange
        var stressResult = StressApplicationResult.Create(
            previousStress: 30,
            newStress: 35,
            source: StressSource.Environmental);
        _mockStressService
            .Setup(s => s.ApplyStress(TestCharacterId, 5, StressSource.Environmental, 0))
            .Returns(stressResult);

        // Act
        var result = _handler.ProcessTurnEnd(TestCharacterId, environmentalStress: 10);

        // Assert
        result.EnvironmentalStressApplied.Should().Be(5);
        _mockStressService.Verify(
            s => s.ApplyStress(TestCharacterId, 5, StressSource.Environmental, 0),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RESULT OBJECT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ProcessTurnStart_ReturnsValidResult()
    {
        // Act
        var result = _handler.ProcessTurnStart(TestCharacterId, isInCombat: false);

        // Assert
        result.CharacterId.Should().Be(TestCharacterId);
        result.Phase.Should().Be(TurnPhase.Start);
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void ProcessTurnEnd_ReturnsValidResult()
    {
        // Act
        var result = _handler.ProcessTurnEnd(TestCharacterId);

        // Assert
        result.CharacterId.Should().Be(TestCharacterId);
        result.Phase.Should().Be(TurnPhase.End);
        result.IsValid.Should().BeTrue();
    }
}
