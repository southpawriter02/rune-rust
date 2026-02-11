// ═══════════════════════════════════════════════════════════════════════════════
// BerserkrAbilityServiceTests.cs
// Unit tests for the BerserkrAbilityService covering Fury Strike,
// Blood Scent, Pain is Fuel, and ability readiness.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class BerserkrAbilityServiceTests
{
    private Mock<IBerserkrRageService> _rageServiceMock = null!;
    private Mock<IBerserkrCorruptionService> _corruptionServiceMock = null!;
    private Mock<ILogger<BerserkrAbilityService>> _loggerMock = null!;
    private BerserkrAbilityService _service = null!;

    private static readonly Guid TestCharacterId = Guid.NewGuid();
    private static readonly Guid TestTargetId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _rageServiceMock = new Mock<IBerserkrRageService>();
        _corruptionServiceMock = new Mock<IBerserkrCorruptionService>();
        _loggerMock = new Mock<ILogger<BerserkrAbilityService>>();
        _service = new BerserkrAbilityService(
            _rageServiceMock.Object,
            _corruptionServiceMock.Object,
            _loggerMock.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Fury Strike
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void UseFuryStrike_NoRageInitialized_ReturnsEmptyResult()
    {
        // Arrange
        _rageServiceMock.Setup(r => r.GetRage(TestCharacterId)).Returns((RageResource?)null);

        // Act
        var result = _service.UseFuryStrike(TestCharacterId, TestTargetId);

        // Assert
        result.AttackRoll.Should().Be(0);
        result.FinalDamage.Should().Be(0);
    }

    [Test]
    public void UseFuryStrike_InsufficientRage_ReturnsEmptyResult()
    {
        // Arrange
        var rage = RageResource.CreateAt(10);
        _rageServiceMock.Setup(r => r.GetRage(TestCharacterId)).Returns(rage);

        // Act
        var result = _service.UseFuryStrike(TestCharacterId, TestTargetId);

        // Assert
        result.AttackRoll.Should().Be(0);
        result.FinalDamage.Should().Be(0);
    }

    [Test]
    public void UseFuryStrike_SufficientRage_SpendsRageAndReturnsResult()
    {
        // Arrange
        var rage = RageResource.CreateAt(50);
        _rageServiceMock.Setup(r => r.GetRage(TestCharacterId)).Returns(rage);
        _rageServiceMock.Setup(r => r.SpendRage(TestCharacterId, RageResource.FuryStrikeCost))
            .Returns(true);
        _corruptionServiceMock.Setup(c => c.EvaluateRisk(
                BerserkrAbilityId.FuryStrike, rage.CurrentRage, false))
            .Returns(BerserkrCorruptionRiskResult.CreateSafe());

        // Act
        var result = _service.UseFuryStrike(TestCharacterId, TestTargetId);

        // Assert
        result.RageSpent.Should().Be(RageResource.FuryStrikeCost);
        result.FuryDamage.Should().BeGreaterThan(0);
        result.BaseDamage.Should().BeGreaterThan(0);
        result.AttackRoll.Should().BeGreaterThan(0);
        result.CorruptionTriggered.Should().BeFalse();
        _rageServiceMock.Verify(r => r.SpendRage(TestCharacterId, RageResource.FuryStrikeCost), Times.Once);
    }

    [Test]
    public void UseFuryStrike_WhileEnraged_TriggersCorruption()
    {
        // Arrange
        var rage = RageResource.CreateAt(85);
        _rageServiceMock.Setup(r => r.GetRage(TestCharacterId)).Returns(rage);
        _rageServiceMock.Setup(r => r.SpendRage(TestCharacterId, RageResource.FuryStrikeCost))
            .Returns(true);

        var corruptionResult = BerserkrCorruptionRiskResult.CreateTriggered(
            1, BerserkrCorruptionTrigger.FuryStrikeWhileEnraged, "Fury Strike at 85 Rage");
        _corruptionServiceMock.Setup(c => c.EvaluateRisk(
                BerserkrAbilityId.FuryStrike, rage.CurrentRage, false))
            .Returns(corruptionResult);

        // Act
        var result = _service.UseFuryStrike(TestCharacterId, TestTargetId);

        // Assert
        result.CorruptionTriggered.Should().BeTrue();
        result.CorruptionReason.Should().Contain("85 Rage");
        _corruptionServiceMock.Verify(
            c => c.ApplyCorruption(TestCharacterId, corruptionResult), Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Blood Scent
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CheckBloodScent_TargetJustBloodied_ReturnsTrueAndAddsRage()
    {
        // Arrange — target was at 60/100 HP, now at 49/100 HP (just crossed 50%)
        // Act
        var result = _service.CheckBloodScent(
            TestCharacterId, TestTargetId,
            previousHp: 60, currentHp: 49, maxHp: 100,
            targetName: "Draugr");

        // Assert
        result.Should().BeTrue();
        _rageServiceMock.Verify(r => r.AddRage(
            TestCharacterId, RageResource.BloodScentGain, "Blood Scent"), Times.Once);
    }

    [Test]
    public void CheckBloodScent_TargetAlreadyBloodied_ReturnsFalse()
    {
        // Arrange — target was already bloodied (40/100 → 30/100)
        // Act
        var result = _service.CheckBloodScent(
            TestCharacterId, TestTargetId,
            previousHp: 40, currentHp: 30, maxHp: 100,
            targetName: "Draugr");

        // Assert
        result.Should().BeFalse();
        _rageServiceMock.Verify(r => r.AddRage(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void CheckBloodScent_TargetNotBloodied_ReturnsFalse()
    {
        // Arrange — target still above 50% (80/100 → 60/100)
        // Act
        var result = _service.CheckBloodScent(
            TestCharacterId, TestTargetId,
            previousHp: 80, currentHp: 60, maxHp: 100,
            targetName: "Draugr");

        // Assert
        result.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Pain is Fuel
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CheckPainIsFuel_WithDamage_AddsRageAndReturnsGain()
    {
        // Arrange & Act
        var rageGained = _service.CheckPainIsFuel(TestCharacterId, damageReceived: 15);

        // Assert
        rageGained.Should().Be(RageResource.PainIsFuelGain);
        _rageServiceMock.Verify(r => r.AddRage(
            TestCharacterId, RageResource.PainIsFuelGain, "Pain is Fuel"), Times.Once);
    }

    [Test]
    public void CheckPainIsFuel_ZeroDamage_ReturnsZero()
    {
        // Arrange & Act
        var rageGained = _service.CheckPainIsFuel(TestCharacterId, damageReceived: 0);

        // Assert
        rageGained.Should().Be(0);
        _rageServiceMock.Verify(r => r.AddRage(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Ability Readiness
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetAbilityReadiness_PassiveAbility_AlwaysReady()
    {
        // Arrange & Act
        var bloodScentReady = _service.GetAbilityReadiness(TestCharacterId, BerserkrAbilityId.BloodScent);
        var painIsFuelReady = _service.GetAbilityReadiness(TestCharacterId, BerserkrAbilityId.PainIsFuel);

        // Assert
        bloodScentReady.Should().BeTrue();
        painIsFuelReady.Should().BeTrue();
    }

    [Test]
    public void GetAbilityReadiness_FuryStrike_SufficientRage_Ready()
    {
        // Arrange
        var rage = RageResource.CreateAt(25);
        _rageServiceMock.Setup(r => r.GetRage(TestCharacterId)).Returns(rage);

        // Act
        var ready = _service.GetAbilityReadiness(TestCharacterId, BerserkrAbilityId.FuryStrike);

        // Assert
        ready.Should().BeTrue();
    }

    [Test]
    public void GetAbilityReadiness_FuryStrike_InsufficientRage_NotReady()
    {
        // Arrange
        var rage = RageResource.CreateAt(15);
        _rageServiceMock.Setup(r => r.GetRage(TestCharacterId)).Returns(rage);

        // Act
        var ready = _service.GetAbilityReadiness(TestCharacterId, BerserkrAbilityId.FuryStrike);

        // Assert
        ready.Should().BeFalse();
    }
}
