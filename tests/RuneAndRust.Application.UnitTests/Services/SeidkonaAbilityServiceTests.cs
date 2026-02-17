using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="SeidkonaAbilityService"/>.
/// Covers Tier 1 abilities (Seiðr Bolt, Wyrd Sight, Aether Attunement),
/// Tier 2 abilities (Fate's Thread, Weave Disruption, Resonance Cascade),
/// including probability-based Corruption checks, Resonance building,
/// Cascade AP cost reduction, and guard-clause validation.
/// </summary>
[TestFixture]
public class SeidkonaAbilityServiceTests
{
    private Mock<ISeidkonaResonanceService> _mockResonanceService = null!;
    private Mock<ISeidkonaCorruptionService> _mockCorruptionService = null!;
    private TestSeidkonaAbilityService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockResonanceService = new Mock<ISeidkonaResonanceService>();
        _mockCorruptionService = new Mock<ISeidkonaCorruptionService>();
        _service = new TestSeidkonaAbilityService(
            _mockResonanceService.Object,
            _mockCorruptionService.Object,
            Mock.Of<ILogger<SeidkonaAbilityService>>());
    }

    /// <summary>
    /// Test subclass that overrides dice methods for deterministic testing.
    /// </summary>
    private class TestSeidkonaAbilityService : SeidkonaAbilityService
    {
        /// <summary>Fixed 2d6 roll result for Seiðr Bolt damage.</summary>
        public int Fixed2D6 { get; set; } = 7;

        /// <summary>Fixed d20 roll result for Weave Disruption dispel checks.</summary>
        public int FixedD20 { get; set; } = 14;

        public TestSeidkonaAbilityService(
            ISeidkonaResonanceService resonanceService,
            ISeidkonaCorruptionService corruptionService,
            ILogger<SeidkonaAbilityService> logger)
            : base(resonanceService, corruptionService, logger) { }

        internal override int Roll2D6() => Fixed2D6;
        internal override int RollD20() => FixedD20;
    }

    /// <summary>
    /// Test subclass for the Corruption service that provides deterministic d100 rolls.
    /// </summary>
    private class TestSeidkonaCorruptionService : SeidkonaCorruptionService
    {
        /// <summary>Fixed d100 roll result for Corruption checks.</summary>
        public int FixedD100 { get; set; } = 50;

        public TestSeidkonaCorruptionService(ILogger<SeidkonaCorruptionService> logger)
            : base(logger) { }

        internal override int RollD100() => FixedD100;
    }

    /// <summary>
    /// Creates a Seiðkona player with specified unlocked abilities for testing.
    /// </summary>
    /// <param name="abilities">The abilities to unlock.</param>
    /// <returns>A configured Seiðkona player with 10 AP and initialized resources.</returns>
    private static Player CreateSeidkona(params SeidkonaAbilityId[] abilities)
    {
        var player = new Player("Test Seidkona");
        player.SetSpecialization("seidkona");
        player.InitializeAetherResonance();
        player.CurrentAP = 10;
        foreach (var ability in abilities)
        {
            player.UnlockSeidkonaAbility(ability);
        }
        return player;
    }

    // ===== Seiðr Bolt Tests =====

    [Test]
    public void ExecuteSeidrBolt_WithValidPrereqs_ReturnsResult()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.SeidrBolt);
        var resonance = AetherResonanceResource.CreateAt(3, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.SeidrBolt, 3))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Below risk threshold"));

        // Act
        var result = _service.ExecuteSeidrBolt(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.DamageRoll.Should().Be(7); // Fixed2D6
        result.TotalDamage.Should().Be(7);
        result.ResonanceBefore.Should().Be(3);
        result.CorruptionTriggered.Should().BeFalse();
        player.CurrentAP.Should().Be(9); // 10 - 1
    }

    [Test]
    public void ExecuteSeidrBolt_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.SeidrBolt);
        player.CurrentAP = 0;

        // Act
        var result = _service.ExecuteSeidrBolt(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(0); // Unchanged
    }

    [Test]
    public void ExecuteSeidrBolt_WrongSpecialization_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Warrior");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteSeidrBolt(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteSeidrBolt_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(); // No abilities unlocked

        // Act
        var result = _service.ExecuteSeidrBolt(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteSeidrBolt_BuildsResonance()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.SeidrBolt);
        var resonance = AetherResonanceResource.CreateAt(2, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.SeidrBolt, 2))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        _service.ExecuteSeidrBolt(player, Guid.NewGuid());

        // Assert — BuildResonance should be called with +1
        _mockResonanceService.Verify(
            s => s.BuildResonance(player, 1, "Seiðr Bolt cast"),
            Times.Once);
    }

    [Test]
    public void ExecuteSeidrBolt_AccumulatesDamage()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.SeidrBolt);
        var resonance = AetherResonanceResource.CreateAt(0, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.SeidrBolt, 0))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        _service.ExecuteSeidrBolt(player, Guid.NewGuid());

        // Assert — AddAccumulatedDamage should be called with the damage roll
        _mockResonanceService.Verify(
            s => s.AddAccumulatedDamage(player, 7), // Fixed2D6
            Times.Once);
    }

    [Test]
    public void ExecuteSeidrBolt_LowResonance_NoCorruptionCheck()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.SeidrBolt);
        var resonance = AetherResonanceResource.CreateAt(3, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.SeidrBolt, 3))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Below threshold"));

        // Act
        var result = _service.ExecuteSeidrBolt(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeFalse();
        result.CorruptionCheckPerformed.Should().BeFalse();

        // ApplyCorruption should NOT be called
        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(It.IsAny<Guid>(), It.IsAny<SeidkonaCorruptionRiskResult>()),
            Times.Never);
    }

    [Test]
    public void ExecuteSeidrBolt_MidResonance_CorruptionTriggered()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.SeidrBolt);
        var resonance = AetherResonanceResource.CreateAt(6, 10);
        player.SetAetherResonance(resonance);

        var triggeredResult = SeidkonaCorruptionRiskResult.CreateTriggered(
            1,
            SeidkonaCorruptionTrigger.SeidrBoltLowResonance,
            "Corruption triggered at Resonance 6",
            3, 5);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.SeidrBolt, 6))
            .Returns(triggeredResult);

        // Act
        var result = _service.ExecuteSeidrBolt(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeTrue();
        result.CorruptionRoll.Should().Be(3);
        result.CorruptionRiskPercent.Should().Be(5);

        // ApplyCorruption SHOULD be called
        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(player.Id, triggeredResult),
            Times.Once);
    }

    [Test]
    public void ExecuteSeidrBolt_MidResonance_CorruptionSafeWithRoll()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.SeidrBolt);
        var resonance = AetherResonanceResource.CreateAt(7, 10);
        player.SetAetherResonance(resonance);

        var safeWithRoll = SeidkonaCorruptionRiskResult.CreateSafeWithRoll(
            "Safe — will holds", 78, 5);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.SeidrBolt, 7))
            .Returns(safeWithRoll);

        // Act
        var result = _service.ExecuteSeidrBolt(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeFalse();
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionRoll.Should().Be(78);
        result.CorruptionRiskPercent.Should().Be(5);
    }

    // ===== Wyrd Sight Tests =====

    [Test]
    public void ExecuteWyrdSight_WithValidPrereqs_ReturnsResult()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.WyrdSight);

        // Act
        var result = _service.ExecuteWyrdSight(player);

        // Assert
        result.Should().NotBeNull();
        result!.TurnsRemaining.Should().Be(3);
        result.DetectionRadius.Should().Be(10);
        result.DetectsInvisible.Should().BeTrue();
        result.DetectsMagic.Should().BeTrue();
        result.DetectsCorruption.Should().BeTrue();
        result.CasterId.Should().Be(player.Id);
        player.CurrentAP.Should().Be(8); // 10 - 2
        player.WyrdSight.Should().NotBeNull();
    }

    [Test]
    public void ExecuteWyrdSight_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.WyrdSight);
        player.CurrentAP = 1; // Need 2

        // Act
        var result = _service.ExecuteWyrdSight(player);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(1); // Unchanged
    }

    [Test]
    public void ExecuteWyrdSight_WrongSpecialization_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Warrior");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteWyrdSight(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteWyrdSight_DoesNotBuildResonance()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.WyrdSight);

        // Act
        _service.ExecuteWyrdSight(player);

        // Assert — BuildResonance should NOT be called
        _mockResonanceService.Verify(
            s => s.BuildResonance(It.IsAny<Player>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public void ExecuteWyrdSight_DoesNotTriggerCorruptionCheck()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.WyrdSight);

        // Act
        _service.ExecuteWyrdSight(player);

        // Assert — EvaluateRisk should NOT be called
        _mockCorruptionService.Verify(
            s => s.EvaluateRisk(It.IsAny<SeidkonaAbilityId>(), It.IsAny<int>()),
            Times.Never);
    }

    // ===== Aether Attunement Tests =====

    [Test]
    public void GetAetherAttunementBonus_Unlocked_Returns10()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.AetherAttunement);

        // Act
        var bonus = _service.GetAetherAttunementBonus(player);

        // Assert
        bonus.Should().Be(10);
    }

    [Test]
    public void GetAetherAttunementBonus_NotUnlocked_ReturnsZero()
    {
        // Arrange
        var player = CreateSeidkona(); // No abilities unlocked

        // Act
        var bonus = _service.GetAetherAttunementBonus(player);

        // Assert
        bonus.Should().Be(0);
    }

    [Test]
    public void GetAetherAttunementBonus_WrongSpecialization_ReturnsZero()
    {
        // Arrange
        var player = new Player("Test Warrior");
        player.SetSpecialization("berserkr");

        // Act
        var bonus = _service.GetAetherAttunementBonus(player);

        // Assert
        bonus.Should().Be(0);
    }

    // ===== Fate's Thread Tests (v0.20.8b) =====

    [Test]
    public void ExecuteFatesThread_WithValidPrereqs_ReturnsResult()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.FatesThread);
        var resonance = AetherResonanceResource.CreateAt(3, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Fate's Thread cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.FatesThread, 3))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Below risk threshold"));

        // Act
        var result = _service.ExecuteFatesThread(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.ResonanceBefore.Should().Be(3);
        result.ResonanceGained.Should().Be(2);
        result.CorruptionTriggered.Should().BeFalse();
        result.ApCostPaid.Should().Be(2); // Base cost, no Cascade
        result.CascadeApplied.Should().BeFalse();
        player.CurrentAP.Should().Be(8); // 10 - 2
    }

    [Test]
    public void ExecuteFatesThread_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.FatesThread);
        player.CurrentAP = 1; // Need 2

        // Act
        var result = _service.ExecuteFatesThread(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(1); // Unchanged
    }

    [Test]
    public void ExecuteFatesThread_WrongSpecialization_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Warrior");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteFatesThread(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteFatesThread_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(); // No abilities unlocked

        // Act
        var result = _service.ExecuteFatesThread(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteFatesThread_BuildsResonancePlusTwo()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.FatesThread);
        var resonance = AetherResonanceResource.CreateAt(2, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Fate's Thread cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.FatesThread, 2))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        _service.ExecuteFatesThread(player, Guid.NewGuid());

        // Assert — BuildResonance should be called with +2 (higher than Tier 1's +1)
        _mockResonanceService.Verify(
            s => s.BuildResonance(player, 2, "Fate's Thread cast"),
            Times.Once);
    }

    [Test]
    public void ExecuteFatesThread_DoesNotAccumulateDamage()
    {
        // Arrange — Fate's Thread is divination, NOT damage
        var player = CreateSeidkona(SeidkonaAbilityId.FatesThread);
        var resonance = AetherResonanceResource.CreateAt(0, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Fate's Thread cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.FatesThread, 0))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        _service.ExecuteFatesThread(player, Guid.NewGuid());

        // Assert — AddAccumulatedDamage should NOT be called (no damage dealt)
        _mockResonanceService.Verify(
            s => s.AddAccumulatedDamage(It.IsAny<Player>(), It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public void ExecuteFatesThread_LowResonance_NoCorruptionCheck()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.FatesThread);
        var resonance = AetherResonanceResource.CreateAt(3, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Fate's Thread cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.FatesThread, 3))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Below threshold"));

        // Act
        var result = _service.ExecuteFatesThread(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeFalse();
        result.CorruptionCheckPerformed.Should().BeFalse();

        // ApplyCorruption should NOT be called
        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(It.IsAny<Guid>(), It.IsAny<SeidkonaCorruptionRiskResult>()),
            Times.Never);
    }

    [Test]
    public void ExecuteFatesThread_MidResonance_CorruptionTriggered()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.FatesThread);
        var resonance = AetherResonanceResource.CreateAt(6, 10);
        player.SetAetherResonance(resonance);

        var triggeredResult = SeidkonaCorruptionRiskResult.CreateTriggered(
            1,
            SeidkonaCorruptionTrigger.CastingAtHighResonance,
            "FatesThread at Resonance 6 — Corruption triggered",
            3, 5);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Fate's Thread cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.FatesThread, 6))
            .Returns(triggeredResult);

        // Act
        var result = _service.ExecuteFatesThread(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeTrue();
        result.CorruptionRoll.Should().Be(3);
        result.CorruptionRiskPercent.Should().Be(5);

        // ApplyCorruption SHOULD be called
        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(player.Id, triggeredResult),
            Times.Once);
    }

    [Test]
    public void ExecuteFatesThread_WithCascadeActive_ReducesApCost()
    {
        // Arrange — Cascade requires ResonanceCascade unlocked AND Resonance ≥ 5
        var player = CreateSeidkona(
            SeidkonaAbilityId.FatesThread,
            SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(6, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Fate's Thread cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.FatesThread, 6))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        var result = _service.ExecuteFatesThread(player, Guid.NewGuid());

        // Assert — Cascade reduces 2 AP → 1 AP
        result.Should().NotBeNull();
        result!.ApCostPaid.Should().Be(1); // Reduced from base 2
        result.CascadeApplied.Should().BeTrue();
        player.CurrentAP.Should().Be(9); // 10 - 1 (Cascade reduced)
    }

    // ===== Weave Disruption Tests (v0.20.8b) =====

    [Test]
    public void ExecuteWeaveDisruption_WithValidPrereqs_ReturnsResult()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.WeaveDisruption);
        var resonance = AetherResonanceResource.CreateAt(3, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 1, "Weave Disruption cast"))
            .Returns(1);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.WeaveDisruption, 3))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Below risk threshold"));

        // Act
        var result = _service.ExecuteWeaveDisruption(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.DispelRoll.Should().Be(14); // FixedD20
        result.ResonanceBonus.Should().Be(3); // Current Resonance added to roll
        result.TotalRoll.Should().Be(17); // 14 + 3
        result.ResonanceBefore.Should().Be(3);
        result.ResonanceGained.Should().Be(1);
        result.CorruptionTriggered.Should().BeFalse();
        result.ApCostPaid.Should().Be(3); // Base cost, no Cascade
        result.CascadeApplied.Should().BeFalse();
        player.CurrentAP.Should().Be(7); // 10 - 3
    }

    [Test]
    public void ExecuteWeaveDisruption_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.WeaveDisruption);
        player.CurrentAP = 2; // Need 3

        // Act
        var result = _service.ExecuteWeaveDisruption(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(2); // Unchanged
    }

    [Test]
    public void ExecuteWeaveDisruption_WrongSpecialization_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Warrior");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteWeaveDisruption(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteWeaveDisruption_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(); // No abilities unlocked

        // Act
        var result = _service.ExecuteWeaveDisruption(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteWeaveDisruption_BuildsResonancePlusOne()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.WeaveDisruption);
        var resonance = AetherResonanceResource.CreateAt(2, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 1, "Weave Disruption cast"))
            .Returns(1);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.WeaveDisruption, 2))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        _service.ExecuteWeaveDisruption(player, Guid.NewGuid());

        // Assert — BuildResonance should be called with +1
        _mockResonanceService.Verify(
            s => s.BuildResonance(player, 1, "Weave Disruption cast"),
            Times.Once);
    }

    [Test]
    public void ExecuteWeaveDisruption_DoesNotAccumulateDamage()
    {
        // Arrange — Weave Disruption is dispel, NOT damage
        var player = CreateSeidkona(SeidkonaAbilityId.WeaveDisruption);
        var resonance = AetherResonanceResource.CreateAt(0, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 1, "Weave Disruption cast"))
            .Returns(1);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.WeaveDisruption, 0))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        _service.ExecuteWeaveDisruption(player, Guid.NewGuid());

        // Assert — AddAccumulatedDamage should NOT be called (no damage dealt)
        _mockResonanceService.Verify(
            s => s.AddAccumulatedDamage(It.IsAny<Player>(), It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public void ExecuteWeaveDisruption_LowResonance_NoCorruptionCheck()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.WeaveDisruption);
        var resonance = AetherResonanceResource.CreateAt(3, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 1, "Weave Disruption cast"))
            .Returns(1);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.WeaveDisruption, 3))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Below threshold"));

        // Act
        var result = _service.ExecuteWeaveDisruption(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeFalse();
        result.CorruptionCheckPerformed.Should().BeFalse();

        // ApplyCorruption should NOT be called
        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(It.IsAny<Guid>(), It.IsAny<SeidkonaCorruptionRiskResult>()),
            Times.Never);
    }

    [Test]
    public void ExecuteWeaveDisruption_MidResonance_CorruptionSafeWithRoll()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.WeaveDisruption);
        var resonance = AetherResonanceResource.CreateAt(7, 10);
        player.SetAetherResonance(resonance);

        var safeWithRoll = SeidkonaCorruptionRiskResult.CreateSafeWithRoll(
            "Safe — will holds", 78, 5);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 1, "Weave Disruption cast"))
            .Returns(1);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.WeaveDisruption, 7))
            .Returns(safeWithRoll);

        // Act
        var result = _service.ExecuteWeaveDisruption(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeFalse();
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionRoll.Should().Be(78);
        result.CorruptionRiskPercent.Should().Be(5);
    }

    [Test]
    public void ExecuteWeaveDisruption_WithCascadeActive_ReducesApCost()
    {
        // Arrange — Cascade requires ResonanceCascade unlocked AND Resonance ≥ 5
        var player = CreateSeidkona(
            SeidkonaAbilityId.WeaveDisruption,
            SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(6, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 1, "Weave Disruption cast"))
            .Returns(1);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.WeaveDisruption, 6))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        var result = _service.ExecuteWeaveDisruption(player, Guid.NewGuid());

        // Assert — Cascade reduces 3 AP → 2 AP
        result.Should().NotBeNull();
        result!.ApCostPaid.Should().Be(2); // Reduced from base 3
        result.CascadeApplied.Should().BeTrue();
        player.CurrentAP.Should().Be(8); // 10 - 2 (Cascade reduced)
    }

    // ===== Resonance Cascade State Tests (v0.20.8b) =====

    [Test]
    public void GetResonanceCascadeState_NotUnlocked_ReturnsInactive()
    {
        // Arrange — player has no Cascade unlocked
        var player = CreateSeidkona(SeidkonaAbilityId.SeidrBolt);
        var resonance = AetherResonanceResource.CreateAt(7, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);

        // Act
        var state = _service.GetResonanceCascadeState(player);

        // Assert
        state.IsActive.Should().BeFalse();
        state.IsUnlocked.Should().BeFalse();
        state.CostReduction.Should().Be(0);
    }

    [Test]
    public void GetResonanceCascadeState_UnlockedAtResonance4_ReturnsInactive()
    {
        // Arrange — Cascade unlocked, but Resonance below threshold (5)
        var player = CreateSeidkona(SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(4, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);

        // Act
        var state = _service.GetResonanceCascadeState(player);

        // Assert
        state.IsActive.Should().BeFalse();
        state.IsUnlocked.Should().BeTrue();
        state.CostReduction.Should().Be(0);
        state.CurrentResonance.Should().Be(4);
    }

    [Test]
    public void GetResonanceCascadeState_UnlockedAtResonance5_ReturnsActive()
    {
        // Arrange — Cascade unlocked, Resonance at threshold (5)
        var player = CreateSeidkona(SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(5, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);

        // Act
        var state = _service.GetResonanceCascadeState(player);

        // Assert
        state.IsActive.Should().BeTrue();
        state.IsUnlocked.Should().BeTrue();
        state.CostReduction.Should().Be(1);
        state.CurrentResonance.Should().Be(5);
    }

    [Test]
    public void GetResonanceCascadeState_WrongSpecialization_ReturnsInactive()
    {
        // Arrange
        var player = new Player("Test Warrior");
        player.SetSpecialization("berserkr");

        // Act
        var state = _service.GetResonanceCascadeState(player);

        // Assert
        state.IsActive.Should().BeFalse();
        state.IsUnlocked.Should().BeFalse();
    }

    // ===== GetEffectiveApCost Tests (v0.20.8b) =====

    [Test]
    public void GetEffectiveApCost_WithoutCascade_ReturnsBaseCost()
    {
        // Arrange — no Cascade unlocked
        var player = CreateSeidkona(SeidkonaAbilityId.FatesThread);

        // Act
        var cost = _service.GetEffectiveApCost(player, SeidkonaAbilityId.FatesThread);

        // Assert
        cost.Should().Be(2); // Base cost for Fate's Thread
    }

    [Test]
    public void GetEffectiveApCost_WithCascadeActive_ReturnsReducedCost()
    {
        // Arrange — Cascade unlocked AND Resonance ≥ 5
        var player = CreateSeidkona(
            SeidkonaAbilityId.FatesThread,
            SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(6, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);

        // Act
        var cost = _service.GetEffectiveApCost(player, SeidkonaAbilityId.FatesThread);

        // Assert — Cascade reduces 2 → 1
        cost.Should().Be(1);
    }

    [Test]
    public void GetEffectiveApCost_PassiveAbility_ReturnsZero()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.AetherAttunement);

        // Act
        var cost = _service.GetEffectiveApCost(player, SeidkonaAbilityId.AetherAttunement);

        // Assert
        cost.Should().Be(0); // Passive abilities have no AP cost
    }

    [Test]
    public void GetEffectiveApCost_SeidrBoltWithCascade_StaysAtMinimum()
    {
        // Arrange — Seiðr Bolt costs 1 AP, Cascade can't reduce below 1
        var player = CreateSeidkona(
            SeidkonaAbilityId.SeidrBolt,
            SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(6, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);

        // Act
        var cost = _service.GetEffectiveApCost(player, SeidkonaAbilityId.SeidrBolt);

        // Assert — 1 AP already at minimum, Cascade doesn't reduce further
        cost.Should().Be(1);
    }

    [Test]
    public void GetEffectiveApCost_WeaveDisruptionWithCascade_ReducesBy1()
    {
        // Arrange
        var player = CreateSeidkona(
            SeidkonaAbilityId.WeaveDisruption,
            SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(7, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);

        // Act
        var cost = _service.GetEffectiveApCost(player, SeidkonaAbilityId.WeaveDisruption);

        // Assert — 3 AP → 2 AP
        cost.Should().Be(2);
    }

    // ===== Utility Tests =====

    [Test]
    public void GetAbilityReadiness_WithUnlockedAbilities_ReturnsCorrectStatus()
    {
        // Arrange
        var player = CreateSeidkona(
            SeidkonaAbilityId.SeidrBolt,
            SeidkonaAbilityId.WyrdSight,
            SeidkonaAbilityId.AetherAttunement);
        player.CurrentAP = 1; // Enough for SeidrBolt (1), NOT for WyrdSight (2)

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().ContainKey(SeidkonaAbilityId.SeidrBolt);
        readiness[SeidkonaAbilityId.SeidrBolt].Should().BeTrue(); // 1 AP available, 1 needed
        readiness.Should().ContainKey(SeidkonaAbilityId.WyrdSight);
        readiness[SeidkonaAbilityId.WyrdSight].Should().BeFalse(); // 1 AP available, 2 needed
        readiness.Should().ContainKey(SeidkonaAbilityId.AetherAttunement);
        readiness[SeidkonaAbilityId.AetherAttunement].Should().BeTrue(); // Passive, always ready
    }

    [Test]
    public void GetAbilityReadiness_WithTier2Abilities_IncludesThem()
    {
        // Arrange
        var player = CreateSeidkona(
            SeidkonaAbilityId.FatesThread,
            SeidkonaAbilityId.WeaveDisruption,
            SeidkonaAbilityId.ResonanceCascade);
        player.CurrentAP = 2; // Enough for FatesThread (2), NOT for WeaveDisruption (3)

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().ContainKey(SeidkonaAbilityId.FatesThread);
        readiness[SeidkonaAbilityId.FatesThread].Should().BeTrue(); // 2 AP available, 2 needed
        readiness.Should().ContainKey(SeidkonaAbilityId.WeaveDisruption);
        readiness[SeidkonaAbilityId.WeaveDisruption].Should().BeFalse(); // 2 AP available, 3 needed
        readiness.Should().ContainKey(SeidkonaAbilityId.ResonanceCascade);
        readiness[SeidkonaAbilityId.ResonanceCascade].Should().BeTrue(); // Passive, always ready
    }

    [Test]
    public void GetAbilityReadiness_WithCascadeActive_UsesReducedCosts()
    {
        // Arrange — with Cascade active, WeaveDisruption needs 2 AP instead of 3
        var player = CreateSeidkona(
            SeidkonaAbilityId.WeaveDisruption,
            SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(6, 10);
        player.SetAetherResonance(resonance);
        player.CurrentAP = 2; // Enough for reduced cost (2), NOT for base cost (3)

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert — Cascade reduces 3→2, so WeaveDisruption should be ready at 2 AP
        readiness.Should().ContainKey(SeidkonaAbilityId.WeaveDisruption);
        readiness[SeidkonaAbilityId.WeaveDisruption].Should().BeTrue();
    }

    [Test]
    public void GetPPInvested_Tier1Only_ReturnsZero()
    {
        // Arrange
        var player = CreateSeidkona(
            SeidkonaAbilityId.SeidrBolt,
            SeidkonaAbilityId.WyrdSight,
            SeidkonaAbilityId.AetherAttunement);

        // Act
        var pp = _service.GetPPInvested(player);

        // Assert
        pp.Should().Be(0); // All T1 abilities are free
    }

    [Test]
    public void CanUnlockTier2_InsufficientPP_ReturnsFalse()
    {
        // Arrange
        var player = CreateSeidkona(
            SeidkonaAbilityId.SeidrBolt,
            SeidkonaAbilityId.WyrdSight,
            SeidkonaAbilityId.AetherAttunement);

        // Act
        var canUnlock = _service.CanUnlockTier2(player);

        // Assert
        canUnlock.Should().BeFalse(); // 0 PP invested, need 8
    }

    // ===== Corruption Service Direct Tests =====

    [Test]
    public void CorruptionService_SeidrBolt_BelowThreshold_ReturnsSafe()
    {
        // Arrange
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>());

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.SeidrBolt, 3);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.RollResult.Should().Be(0); // No roll performed
        result.RiskPercent.Should().Be(0);
    }

    [Test]
    public void CorruptionService_SeidrBolt_RiskyRange_RollUnder_Triggers()
    {
        // Arrange
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>())
        { FixedD100 = 3 }; // 3 ≤ 5% threshold → triggered

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.SeidrBolt, 6);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
        result.Trigger.Should().Be(SeidkonaCorruptionTrigger.SeidrBoltLowResonance);
        result.RollResult.Should().Be(3);
        result.RiskPercent.Should().Be(5);
    }

    [Test]
    public void CorruptionService_SeidrBolt_RiskyRange_RollOver_Safe()
    {
        // Arrange
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>())
        { FixedD100 = 50 }; // 50 > 5% threshold → safe

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.SeidrBolt, 5);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.RollResult.Should().Be(50);
        result.RiskPercent.Should().Be(5);
    }

    [Test]
    public void CorruptionService_SeidrBolt_DangerousRange_CorrectThreshold()
    {
        // Arrange
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>())
        { FixedD100 = 10 }; // 10 ≤ 15% threshold → triggered

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.SeidrBolt, 9);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.Trigger.Should().Be(SeidkonaCorruptionTrigger.SeidrBoltHighResonance);
        result.RiskPercent.Should().Be(15);
    }

    [Test]
    public void CorruptionService_SeidrBolt_CriticalRange_CorrectThreshold()
    {
        // Arrange
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>())
        { FixedD100 = 20 }; // 20 ≤ 25% threshold → triggered

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.SeidrBolt, 10);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.Trigger.Should().Be(SeidkonaCorruptionTrigger.SeidrBoltMaxResonance);
        result.RiskPercent.Should().Be(25);
    }

    [Test]
    public void CorruptionService_WyrdSight_AlwaysSafe()
    {
        // Arrange
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>());

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.WyrdSight, 10);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.RollResult.Should().Be(0);
    }

    [Test]
    public void CorruptionService_AetherAttunement_AlwaysSafe()
    {
        // Arrange
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>());

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.AetherAttunement, 10);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.RollResult.Should().Be(0);
    }

    // ===== Corruption Service — Tier 2 Direct Tests (v0.20.8b) =====

    [Test]
    public void CorruptionService_ResonanceCascade_AlwaysSafe()
    {
        // Arrange — Resonance Cascade is passive, never triggers Corruption
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>());

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.ResonanceCascade, 10);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.RollResult.Should().Be(0); // No roll performed
        result.Reason.Should().Contain("passive");
    }

    [Test]
    public void CorruptionService_FatesThread_AtHighResonance_TriggersCheck()
    {
        // Arrange — Fate's Thread at Resonance 6 should use CastingAtHighResonance trigger
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>())
        { FixedD100 = 3 }; // 3 ≤ 5% → triggered

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.FatesThread, 6);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.Trigger.Should().Be(SeidkonaCorruptionTrigger.CastingAtHighResonance);
        result.CorruptionAmount.Should().Be(1);
        result.RiskPercent.Should().Be(5);
    }

    [Test]
    public void CorruptionService_WeaveDisruption_AtHighResonance_TriggersCheck()
    {
        // Arrange — Weave Disruption at Resonance 9 should use Dangerous tier (15%)
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>())
        { FixedD100 = 10 }; // 10 ≤ 15% → triggered

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.WeaveDisruption, 9);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.Trigger.Should().Be(SeidkonaCorruptionTrigger.CastingAtHighResonance);
        result.RiskPercent.Should().Be(15);
    }

    [Test]
    public void CorruptionService_FatesThread_BelowThreshold_ReturnsSafe()
    {
        // Arrange — Fate's Thread at low Resonance should be safe (no check)
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>());

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.FatesThread, 3);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.RollResult.Should().Be(0); // No roll performed
    }
}
