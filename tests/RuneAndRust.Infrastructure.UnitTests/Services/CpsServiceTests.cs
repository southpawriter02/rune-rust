namespace RuneAndRust.Infrastructure.UnitTests.Services;

using FluentAssertions;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="CpsService"/> implementation.
/// Verifies state queries, stage transitions, panic table operations,
/// and recovery protocol retrieval.
/// </summary>
[TestFixture]
public class CpsServiceTests
{
    // -------------------------------------------------------------------------
    // Test Setup
    // -------------------------------------------------------------------------

    private Mock<IStressService> _mockStressService = null!;
    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<IStatusEffectService> _mockStatusEffectService = null!;
    private Mock<ILogger<CpsService>> _mockLogger = null!;
    private CpsService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockStressService = new Mock<IStressService>();
        _mockDiceService = new Mock<IDiceService>();
        _mockStatusEffectService = new Mock<IStatusEffectService>();
        _mockLogger = new Mock<ILogger<CpsService>>();

        _sut = new CpsService(
            _mockStressService.Object,
            _mockDiceService.Object,
            _mockStatusEffectService.Object,
            _mockLogger.Object);
    }

    // -------------------------------------------------------------------------
    // GetCpsState Tests
    // -------------------------------------------------------------------------

    [Test]
    public void GetCpsState_ReturnsCorrectState_ForStress65()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var stressState = StressState.Create(65);
        _mockStressService
            .Setup(s => s.GetStressState(characterId))
            .Returns(stressState);

        // Act
        var result = _sut.GetCpsState(characterId);

        // Assert
        result.Stage.Should().Be(CpsStage.RuinMadness);
        result.CurrentStress.Should().Be(65);
        result.RequiresPanicCheck.Should().BeTrue();
        result.IsTerminal.Should().BeFalse();
    }

    [Test]
    [TestCase(0, CpsStage.None)]
    [TestCase(19, CpsStage.None)]
    [TestCase(20, CpsStage.WeightOfKnowing)]
    [TestCase(39, CpsStage.WeightOfKnowing)]
    [TestCase(40, CpsStage.GlimmerMadness)]
    [TestCase(59, CpsStage.GlimmerMadness)]
    [TestCase(60, CpsStage.RuinMadness)]
    [TestCase(79, CpsStage.RuinMadness)]
    [TestCase(80, CpsStage.HollowShell)]
    [TestCase(100, CpsStage.HollowShell)]
    public void GetCpsState_ReturnsCorrectStage_AtBoundaryValues(
        int stress,
        CpsStage expectedStage)
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var stressState = StressState.Create(stress);
        _mockStressService
            .Setup(s => s.GetStressState(characterId))
            .Returns(stressState);

        // Act
        var result = _sut.GetCpsState(characterId);

        // Assert
        result.Stage.Should().Be(expectedStage);
    }

    // -------------------------------------------------------------------------
    // CheckStageChange Tests
    // -------------------------------------------------------------------------

    [Test]
    public void CheckStageChange_DetectsRuinMadnessEntry()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var previousStress = 55; // GlimmerMadness
        var newStress = 65;      // RuinMadness

        // Act
        var result = _sut.CheckStageChange(characterId, previousStress, newStress);

        // Assert
        result.StageChanged.Should().BeTrue();
        result.PreviousStage.Should().Be(CpsStage.GlimmerMadness);
        result.NewStage.Should().Be(CpsStage.RuinMadness);
        result.EnteredRuinMadness.Should().BeTrue();
        result.IsCriticalTransition.Should().BeTrue();
    }

    [Test]
    public void CheckStageChange_DetectsHollowShellEntry()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var previousStress = 75; // RuinMadness
        var newStress = 85;      // HollowShell

        // Act
        var result = _sut.CheckStageChange(characterId, previousStress, newStress);

        // Assert
        result.StageChanged.Should().BeTrue();
        result.PreviousStage.Should().Be(CpsStage.RuinMadness);
        result.NewStage.Should().Be(CpsStage.HollowShell);
        result.EnteredHollowShell.Should().BeTrue();
        result.IsCriticalTransition.Should().BeTrue();
    }

    [Test]
    public void CheckStageChange_ReturnsNoChange_WhenWithinSameStage()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var previousStress = 45; // GlimmerMadness
        var newStress = 50;      // Still GlimmerMadness

        // Act
        var result = _sut.CheckStageChange(characterId, previousStress, newStress);

        // Assert
        result.StageChanged.Should().BeFalse();
        result.PreviousStage.Should().Be(CpsStage.GlimmerMadness);
        result.NewStage.Should().Be(CpsStage.GlimmerMadness);
    }

    // -------------------------------------------------------------------------
    // RollPanicTable Tests
    // -------------------------------------------------------------------------

    [Test]
    public void RollPanicTable_ThrowsException_WhenNotInRuinMadness()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var stressState = StressState.Create(45); // GlimmerMadness
        _mockStressService
            .Setup(s => s.GetStressState(characterId))
            .Returns(stressState);

        // Act
        var act = () => _sut.RollPanicTable(characterId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*requires RuinMadness*");
    }

    [Test]
    public void RollPanicTable_ReturnsViolence_ForRoll7()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var stressState = StressState.Create(70); // RuinMadness
        _mockStressService
            .Setup(s => s.GetStressState(characterId))
            .Returns(stressState);

        var diceResult = new DiceRollResult(DicePool.D10(), new[] { 7 });
        _mockDiceService
            .Setup(d => d.Roll(DiceType.D10, 1, 0))
            .Returns(diceResult);

        // Act
        var result = _sut.RollPanicTable(characterId);

        // Assert
        result.Effect.Should().Be(PanicEffect.Violence);
        result.EffectName.Should().Be("Paradox Fury");
        result.ForcesAction.Should().BeTrue();
        result.ForcedActionType.Should().Be("AttackNearest");
    }

    [Test]
    public void RollPanicTable_ReturnsLuckyBreak_ForRoll10()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var stressState = StressState.Create(65); // RuinMadness
        _mockStressService
            .Setup(s => s.GetStressState(characterId))
            .Returns(stressState);

        var diceResult = new DiceRollResult(DicePool.D10(), new[] { 10 });
        _mockDiceService
            .Setup(d => d.Roll(DiceType.D10, 1, 0))
            .Returns(diceResult);

        // Act
        var result = _sut.RollPanicTable(characterId);

        // Assert
        result.Effect.Should().Be(PanicEffect.None);
        result.IsLuckyBreak.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // ApplyPanicEffect Tests
    // -------------------------------------------------------------------------

    [Test]
    public void ApplyPanicEffect_AppliesStunnedEffect_ForFrozenPanic()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var panicResult = new PanicResult(
            DieRoll: 1,
            Effect: PanicEffect.Frozen,
            EffectName: "Logic Lock",
            Description: "Your mind freezes.",
            DurationTurns: 1,
            SelfDamage: null,
            StatusEffects: new[] { "Stunned" },
            ForcesAction: false,
            ForcedActionType: null);

        // Act
        _sut.ApplyPanicEffect(characterId, panicResult);

        // Assert
        _mockStatusEffectService.Verify(
            s => s.ApplyEffect(characterId, "Stunned", 1),
            Times.Once);
    }

    [Test]
    public void ApplyPanicEffect_DoesNotApplyEffects_ForLuckyBreak()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var panicResult = new PanicResult(
            DieRoll: 10,
            Effect: PanicEffect.None,
            EffectName: "Lucky Break",
            Description: "Your mind holds together.",
            DurationTurns: null,
            SelfDamage: null,
            StatusEffects: Array.Empty<string>(),
            ForcesAction: false,
            ForcedActionType: null);

        // Act
        _sut.ApplyPanicEffect(characterId, panicResult);

        // Assert
        _mockStatusEffectService.Verify(
            s => s.ApplyEffect(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
    }

    // -------------------------------------------------------------------------
    // IsRecoverable Tests
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, true)]    // None
    [TestCase(25, true)]   // WeightOfKnowing
    [TestCase(45, true)]   // GlimmerMadness
    [TestCase(65, false)]  // RuinMadness
    [TestCase(85, false)]  // HollowShell
    public void IsRecoverable_ReturnsExpectedResult_ForStressLevel(
        int stress,
        bool expectedRecoverable)
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var stressState = StressState.Create(stress);
        _mockStressService
            .Setup(s => s.GetStressState(characterId))
            .Returns(stressState);

        // Act
        var result = _sut.IsRecoverable(characterId);

        // Assert
        result.Should().Be(expectedRecoverable);
    }

    // -------------------------------------------------------------------------
    // GetRecoveryProtocol Tests
    // -------------------------------------------------------------------------

    [Test]
    public void GetRecoveryProtocol_ReturnsCorrectProtocol_ForGlimmerMadness()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var stressState = StressState.Create(50); // GlimmerMadness
        _mockStressService
            .Setup(s => s.GetStressState(characterId))
            .Returns(stressState);

        // Act
        var result = _sut.GetRecoveryProtocol(characterId);

        // Assert
        result.ProtocolName.Should().Be("Silent Room Protocol");
        result.Urgency.Should().Be("Critical");
    }
}
