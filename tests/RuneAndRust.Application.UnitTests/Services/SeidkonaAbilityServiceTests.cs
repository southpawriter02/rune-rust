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
/// Tier 3 abilities (Völva's Vision, Aether Storm),
/// Capstone ability (The Unraveling),
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

        /// <summary>Fixed 4d6 roll result for Aether Storm damage.</summary>
        public int Fixed4D6 { get; set; } = 16;

        public TestSeidkonaAbilityService(
            ISeidkonaResonanceService resonanceService,
            ISeidkonaCorruptionService corruptionService,
            ILogger<SeidkonaAbilityService> logger)
            : base(resonanceService, corruptionService, logger) { }

        internal override int Roll2D6() => Fixed2D6;
        internal override int RollD20() => FixedD20;
        internal override int Roll4D6() => Fixed4D6;
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

    // ===== Völva's Vision Tests (v0.20.8c) =====

    [Test]
    public void ExecuteVolvasVision_WithValidPrereqs_ReturnsResult()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.VolvasVision);
        var resonance = AetherResonanceResource.CreateAt(3, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Völva's Vision cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.VolvasVision, 3))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Below risk threshold"));

        // Act
        var result = _service.ExecuteVolvasVision(player);

        // Assert
        result.Should().NotBeNull();
        result!.RevealRadius.Should().Be(15);
        result.ResonanceBefore.Should().Be(3);
        result.ResonanceGained.Should().Be(2);
        result.CorruptionTriggered.Should().BeFalse();
        result.ApCostPaid.Should().Be(3); // Base cost, no Cascade
        result.CascadeApplied.Should().BeFalse();
        player.CurrentAP.Should().Be(7); // 10 - 3
    }

    [Test]
    public void ExecuteVolvasVision_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.VolvasVision);
        player.CurrentAP = 2; // Need 3

        // Act
        var result = _service.ExecuteVolvasVision(player);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(2); // Unchanged
    }

    [Test]
    public void ExecuteVolvasVision_WrongSpecialization_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Warrior");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteVolvasVision(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteVolvasVision_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(); // No abilities unlocked

        // Act
        var result = _service.ExecuteVolvasVision(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteVolvasVision_BuildsResonancePlusTwo()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.VolvasVision);
        var resonance = AetherResonanceResource.CreateAt(2, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Völva's Vision cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.VolvasVision, 2))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        _service.ExecuteVolvasVision(player);

        // Assert — BuildResonance should be called with +2
        _mockResonanceService.Verify(
            s => s.BuildResonance(player, 2, "Völva's Vision cast"),
            Times.Once);
    }

    [Test]
    public void ExecuteVolvasVision_DoesNotAccumulateDamage()
    {
        // Arrange — Völva's Vision is detection, NOT damage
        var player = CreateSeidkona(SeidkonaAbilityId.VolvasVision);
        var resonance = AetherResonanceResource.CreateAt(0, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Völva's Vision cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.VolvasVision, 0))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        _service.ExecuteVolvasVision(player);

        // Assert — AddAccumulatedDamage should NOT be called (no damage dealt)
        _mockResonanceService.Verify(
            s => s.AddAccumulatedDamage(It.IsAny<Player>(), It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public void ExecuteVolvasVision_LowResonance_NoCorruptionCheck()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.VolvasVision);
        var resonance = AetherResonanceResource.CreateAt(3, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Völva's Vision cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.VolvasVision, 3))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Below threshold"));

        // Act
        var result = _service.ExecuteVolvasVision(player);

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeFalse();
        result.CorruptionCheckPerformed.Should().BeFalse();

        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(It.IsAny<Guid>(), It.IsAny<SeidkonaCorruptionRiskResult>()),
            Times.Never);
    }

    [Test]
    public void ExecuteVolvasVision_MidResonance_CorruptionTriggered()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.VolvasVision);
        var resonance = AetherResonanceResource.CreateAt(6, 10);
        player.SetAetherResonance(resonance);

        var triggeredResult = SeidkonaCorruptionRiskResult.CreateTriggered(
            1,
            SeidkonaCorruptionTrigger.CastingAtHighResonance,
            "VolvasVision at Resonance 6 — Corruption triggered",
            3, 5);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Völva's Vision cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.VolvasVision, 6))
            .Returns(triggeredResult);

        // Act
        var result = _service.ExecuteVolvasVision(player);

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeTrue();

        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(player.Id, triggeredResult),
            Times.Once);
    }

    [Test]
    public void ExecuteVolvasVision_WithCascadeActive_ReducesApCost()
    {
        // Arrange — Cascade requires ResonanceCascade unlocked AND Resonance ≥ 5
        var player = CreateSeidkona(
            SeidkonaAbilityId.VolvasVision,
            SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(6, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Völva's Vision cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.VolvasVision, 6))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        var result = _service.ExecuteVolvasVision(player);

        // Assert — Cascade reduces 3 AP → 2 AP
        result.Should().NotBeNull();
        result!.ApCostPaid.Should().Be(2); // Reduced from base 3
        result.CascadeApplied.Should().BeTrue();
        player.CurrentAP.Should().Be(8); // 10 - 2 (Cascade reduced)
    }

    // ===== Aether Storm Tests (v0.20.8c) =====

    [Test]
    public void ExecuteAetherStorm_WithValidPrereqs_ReturnsResult()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.AetherStorm);
        var resonance = AetherResonanceResource.CreateAt(3, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Aether Storm cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.AetherStorm, 3))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Below risk threshold"));

        // Act
        var result = _service.ExecuteAetherStorm(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.DamageRoll.Should().Be(16); // Fixed4D6
        result.TotalDamage.Should().Be(16);
        result.ResonanceBefore.Should().Be(3);
        result.ResonanceGained.Should().Be(2);
        result.CorruptionTriggered.Should().BeFalse();
        result.ApCostPaid.Should().Be(5); // Base cost, no Cascade
        result.CascadeApplied.Should().BeFalse();
        player.CurrentAP.Should().Be(5); // 10 - 5
    }

    [Test]
    public void ExecuteAetherStorm_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.AetherStorm);
        player.CurrentAP = 4; // Need 5

        // Act
        var result = _service.ExecuteAetherStorm(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(4); // Unchanged
    }

    [Test]
    public void ExecuteAetherStorm_WrongSpecialization_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Warrior");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteAetherStorm(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteAetherStorm_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(); // No abilities unlocked

        // Act
        var result = _service.ExecuteAetherStorm(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteAetherStorm_BuildsResonancePlusTwo()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.AetherStorm);
        var resonance = AetherResonanceResource.CreateAt(2, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Aether Storm cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.AetherStorm, 2))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        _service.ExecuteAetherStorm(player, Guid.NewGuid());

        // Assert
        _mockResonanceService.Verify(
            s => s.BuildResonance(player, 2, "Aether Storm cast"),
            Times.Once);
    }

    [Test]
    public void ExecuteAetherStorm_AccumulatesDamage()
    {
        // Arrange — Aether Storm DOES accumulate damage
        var player = CreateSeidkona(SeidkonaAbilityId.AetherStorm);
        var resonance = AetherResonanceResource.CreateAt(0, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Aether Storm cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.AetherStorm, 0))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        _service.ExecuteAetherStorm(player, Guid.NewGuid());

        // Assert — AddAccumulatedDamage SHOULD be called with Fixed4D6 (16)
        _mockResonanceService.Verify(
            s => s.AddAccumulatedDamage(player, 16),
            Times.Once);
    }

    [Test]
    public void ExecuteAetherStorm_LowResonance_NoCorruptionCheck()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.AetherStorm);
        var resonance = AetherResonanceResource.CreateAt(3, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Aether Storm cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.AetherStorm, 3))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Below threshold"));

        // Act
        var result = _service.ExecuteAetherStorm(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeFalse();
        result.CorruptionCheckPerformed.Should().BeFalse();

        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(It.IsAny<Guid>(), It.IsAny<SeidkonaCorruptionRiskResult>()),
            Times.Never);
    }

    [Test]
    public void ExecuteAetherStorm_HighResonance_CorruptionTriggered()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.AetherStorm);
        var resonance = AetherResonanceResource.CreateAt(8, 10);
        player.SetAetherResonance(resonance);

        var triggeredResult = SeidkonaCorruptionRiskResult.CreateTriggered(
            1,
            SeidkonaCorruptionTrigger.CastingAtHighResonance,
            "AetherStorm at Resonance 8 — Corruption triggered",
            10, 15);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Aether Storm cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.AetherStorm, 8))
            .Returns(triggeredResult);

        // Act
        var result = _service.ExecuteAetherStorm(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeTrue();

        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(player.Id, triggeredResult),
            Times.Once);
    }

    [Test]
    public void ExecuteAetherStorm_HighResonance_CorruptionSafeWithRoll()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.AetherStorm);
        var resonance = AetherResonanceResource.CreateAt(8, 10);
        player.SetAetherResonance(resonance);

        var safeWithRoll = SeidkonaCorruptionRiskResult.CreateSafeWithRoll(
            "Safe — will holds", 85, 15);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Aether Storm cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.AetherStorm, 8))
            .Returns(safeWithRoll);

        // Act
        var result = _service.ExecuteAetherStorm(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeFalse();
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionRoll.Should().Be(85);
        result.CorruptionRiskPercent.Should().Be(15);
    }

    [Test]
    public void ExecuteAetherStorm_WithCascadeActive_ReducesApCost()
    {
        // Arrange — Cascade requires ResonanceCascade unlocked AND Resonance ≥ 5
        var player = CreateSeidkona(
            SeidkonaAbilityId.AetherStorm,
            SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(6, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService
            .Setup(s => s.BuildResonance(player, 2, "Aether Storm cast"))
            .Returns(2);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.AetherStorm, 6))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        var result = _service.ExecuteAetherStorm(player, Guid.NewGuid());

        // Assert — Cascade reduces 5 AP → 4 AP
        result.Should().NotBeNull();
        result!.ApCostPaid.Should().Be(4); // Reduced from base 5
        result.CascadeApplied.Should().BeTrue();
        player.CurrentAP.Should().Be(6); // 10 - 4 (Cascade reduced)
    }

    // ===== The Unraveling Tests (v0.20.8c Capstone) =====

    [Test]
    public void ExecuteUnraveling_WithValidPrereqs_ReturnsResult()
    {
        // Arrange — Resonance = 10, Accumulated Damage > 0, not used this combat
        var player = CreateSeidkona(SeidkonaAbilityId.Unraveling);
        var resonance = AetherResonanceResource.CreateAt(10, 10);
        player.SetAetherResonance(resonance);
        var accumulated = AccumulatedAethericDamage.Create().AddDamage(42);
        player.SetAccumulatedAethericDamage(accumulated);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService.Setup(s => s.GetAccumulatedDamage(player)).Returns(accumulated);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.Unraveling, 10))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafeWithRoll(
                "Unraveling safe", 55, 20));

        // Act
        var result = _service.ExecuteUnraveling(player);

        // Assert
        result.Should().NotBeNull();
        result!.AccumulatedDamageConsumed.Should().Be(42);
        result.TotalDamage.Should().Be(42);
        result.ResonanceBefore.Should().Be(10);
        result.ResonanceAfter.Should().Be(0);
        result.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionTriggered.Should().BeFalse();
        result.CorruptionRoll.Should().Be(55);
        result.CorruptionRiskPercent.Should().Be(20);
        result.ApCostPaid.Should().Be(5);
        result.CooldownActivated.Should().BeTrue();
        player.CurrentAP.Should().Be(5); // 10 - 5
        player.HasUsedUnravelingThisCombat.Should().BeTrue();
    }

    [Test]
    public void ExecuteUnraveling_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.Unraveling);
        player.CurrentAP = 4; // Need 5

        // Act
        var result = _service.ExecuteUnraveling(player);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(4); // Unchanged
    }

    [Test]
    public void ExecuteUnraveling_WrongSpecialization_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Warrior");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteUnraveling(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteUnraveling_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange
        var player = CreateSeidkona(); // No abilities unlocked

        // Act
        var result = _service.ExecuteUnraveling(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteUnraveling_ResonanceNotTen_ReturnsNull()
    {
        // Arrange — Resonance is 8, not 10
        var player = CreateSeidkona(SeidkonaAbilityId.Unraveling);
        var resonance = AetherResonanceResource.CreateAt(8, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);

        // Act
        var result = _service.ExecuteUnraveling(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteUnraveling_NoAccumulatedDamage_ReturnsNull()
    {
        // Arrange — Resonance 10 but no accumulated damage
        var player = CreateSeidkona(SeidkonaAbilityId.Unraveling);
        var resonance = AetherResonanceResource.CreateAt(10, 10);
        player.SetAetherResonance(resonance);
        var accumulated = AccumulatedAethericDamage.Create(); // TotalDamage = 0

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService.Setup(s => s.GetAccumulatedDamage(player)).Returns(accumulated);

        // Act
        var result = _service.ExecuteUnraveling(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteUnraveling_AlreadyUsedThisCombat_ReturnsNull()
    {
        // Arrange — Already used this combat
        var player = CreateSeidkona(SeidkonaAbilityId.Unraveling);
        player.HasUsedUnravelingThisCombat = true;

        // Act
        var result = _service.ExecuteUnraveling(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteUnraveling_ResetsResonanceToZero()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.Unraveling);
        var resonance = AetherResonanceResource.CreateAt(10, 10);
        player.SetAetherResonance(resonance);
        var accumulated = AccumulatedAethericDamage.Create().AddDamage(30);
        player.SetAccumulatedAethericDamage(accumulated);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService.Setup(s => s.GetAccumulatedDamage(player)).Returns(accumulated);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.Unraveling, 10))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafeWithRoll("Safe", 55, 20));

        // Act
        _service.ExecuteUnraveling(player);

        // Assert — ResetResonance should be called
        _mockResonanceService.Verify(
            s => s.ResetResonance(player, "Unraveling capstone"),
            Times.Once);
    }

    [Test]
    public void ExecuteUnraveling_ResetsAccumulatedDamage()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.Unraveling);
        var resonance = AetherResonanceResource.CreateAt(10, 10);
        player.SetAetherResonance(resonance);
        var accumulated = AccumulatedAethericDamage.Create().AddDamage(30);
        player.SetAccumulatedAethericDamage(accumulated);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService.Setup(s => s.GetAccumulatedDamage(player)).Returns(accumulated);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.Unraveling, 10))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafeWithRoll("Safe", 55, 20));

        // Act
        _service.ExecuteUnraveling(player);

        // Assert — ResetAccumulatedDamage should be called
        _mockResonanceService.Verify(
            s => s.ResetAccumulatedDamage(player),
            Times.Once);
    }

    [Test]
    public void ExecuteUnraveling_SetsCooldownFlag()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.Unraveling);
        var resonance = AetherResonanceResource.CreateAt(10, 10);
        player.SetAetherResonance(resonance);
        var accumulated = AccumulatedAethericDamage.Create().AddDamage(30);
        player.SetAccumulatedAethericDamage(accumulated);

        player.HasUsedUnravelingThisCombat.Should().BeFalse(); // Before

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService.Setup(s => s.GetAccumulatedDamage(player)).Returns(accumulated);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.Unraveling, 10))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafeWithRoll("Safe", 55, 20));

        // Act
        _service.ExecuteUnraveling(player);

        // Assert
        player.HasUsedUnravelingThisCombat.Should().BeTrue();
    }

    [Test]
    public void ExecuteUnraveling_GuaranteedCorruptionCheck()
    {
        // Arrange
        var player = CreateSeidkona(SeidkonaAbilityId.Unraveling);
        var resonance = AetherResonanceResource.CreateAt(10, 10);
        player.SetAetherResonance(resonance);
        var accumulated = AccumulatedAethericDamage.Create().AddDamage(30);
        player.SetAccumulatedAethericDamage(accumulated);

        var triggeredResult = SeidkonaCorruptionRiskResult.CreateTriggered(
            2,
            SeidkonaCorruptionTrigger.CapstoneActivation,
            "The Unraveling tears at the fabric of reality",
            15, 20);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService.Setup(s => s.GetAccumulatedDamage(player)).Returns(accumulated);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.Unraveling, 10))
            .Returns(triggeredResult);

        // Act
        var result = _service.ExecuteUnraveling(player);

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionCheckPerformed.Should().BeTrue();
        result.CorruptionTriggered.Should().BeTrue();

        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(player.Id, triggeredResult),
            Times.Once);
    }

    [Test]
    public void ExecuteUnraveling_NoCascadeReduction_AlwaysFiveAP()
    {
        // Arrange — Even with Cascade unlocked and Resonance at 10, Unraveling costs 5 AP
        var player = CreateSeidkona(
            SeidkonaAbilityId.Unraveling,
            SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(10, 10);
        player.SetAetherResonance(resonance);
        var accumulated = AccumulatedAethericDamage.Create().AddDamage(30);
        player.SetAccumulatedAethericDamage(accumulated);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService.Setup(s => s.GetAccumulatedDamage(player)).Returns(accumulated);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(SeidkonaAbilityId.Unraveling, 10))
            .Returns(SeidkonaCorruptionRiskResult.CreateSafeWithRoll("Safe", 55, 20));

        // Act
        var result = _service.ExecuteUnraveling(player);

        // Assert — Always 5 AP, Cascade does NOT reduce
        result.Should().NotBeNull();
        result!.ApCostPaid.Should().Be(5);
        player.CurrentAP.Should().Be(5); // 10 - 5 (not 10 - 4)
    }

    // ===== Corruption Service — T3/Capstone Direct Tests (v0.20.8c) =====

    [Test]
    public void CorruptionService_Unraveling_RollUnderTwenty_Triggers()
    {
        // Arrange — d100 = 15, threshold 20% → triggered
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>())
        { FixedD100 = 15 };

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.Unraveling, 10);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(2); // Capstone = +2 Corruption
        result.Trigger.Should().Be(SeidkonaCorruptionTrigger.CapstoneActivation);
        result.RollResult.Should().Be(15);
        result.RiskPercent.Should().Be(20);
    }

    [Test]
    public void CorruptionService_Unraveling_RollOverTwenty_Safe()
    {
        // Arrange — d100 = 55, threshold 20% → safe
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>())
        { FixedD100 = 55 };

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.Unraveling, 10);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.RollResult.Should().Be(55);
        result.RiskPercent.Should().Be(20);
    }

    [Test]
    public void CorruptionService_VolvasVision_AtHighResonance_TriggersCheck()
    {
        // Arrange — Völva's Vision at Resonance 6 uses CastingAtHighResonance trigger
        var corruptionService = new TestSeidkonaCorruptionService(
            Mock.Of<ILogger<SeidkonaCorruptionService>>())
        { FixedD100 = 3 }; // 3 ≤ 5% → triggered

        // Act
        var result = corruptionService.EvaluateRisk(SeidkonaAbilityId.VolvasVision, 6);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.Trigger.Should().Be(SeidkonaCorruptionTrigger.CastingAtHighResonance);
        result.CorruptionAmount.Should().Be(1);
        result.RiskPercent.Should().Be(5);
    }

    // ===== GetEffectiveApCost — T3/Capstone Tests (v0.20.8c) =====

    [Test]
    public void GetEffectiveApCost_Unraveling_IgnoresCascade()
    {
        // Arrange — Cascade is active (unlocked + Resonance 10), but Unraveling is immune
        var player = CreateSeidkona(
            SeidkonaAbilityId.Unraveling,
            SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(10, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);

        // Act
        var cost = _service.GetEffectiveApCost(player, SeidkonaAbilityId.Unraveling);

        // Assert — Always 5, never reduced by Cascade
        cost.Should().Be(5);
    }

    [Test]
    public void GetEffectiveApCost_AetherStormWithCascade_ReducesBy1()
    {
        // Arrange
        var player = CreateSeidkona(
            SeidkonaAbilityId.AetherStorm,
            SeidkonaAbilityId.ResonanceCascade);
        var resonance = AetherResonanceResource.CreateAt(7, 10);
        player.SetAetherResonance(resonance);

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);

        // Act
        var cost = _service.GetEffectiveApCost(player, SeidkonaAbilityId.AetherStorm);

        // Assert — 5 AP → 4 AP
        cost.Should().Be(4);
    }

    // ===== GetAbilityReadiness — T3/Capstone Tests (v0.20.8c) =====

    [Test]
    public void GetAbilityReadiness_WithTier3Abilities_IncludesThem()
    {
        // Arrange
        var player = CreateSeidkona(
            SeidkonaAbilityId.VolvasVision,
            SeidkonaAbilityId.AetherStorm);
        player.CurrentAP = 4; // Enough for VolvasVision (3), NOT for AetherStorm (5)

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().ContainKey(SeidkonaAbilityId.VolvasVision);
        readiness[SeidkonaAbilityId.VolvasVision].Should().BeTrue(); // 4 AP ≥ 3 needed
        readiness.Should().ContainKey(SeidkonaAbilityId.AetherStorm);
        readiness[SeidkonaAbilityId.AetherStorm].Should().BeFalse(); // 4 AP < 5 needed
    }

    [Test]
    public void GetAbilityReadiness_Unraveling_ChecksAllPreconditions()
    {
        // Arrange — Resonance 10, Accumulated > 0, not used, enough AP
        var player = CreateSeidkona(SeidkonaAbilityId.Unraveling);
        var resonance = AetherResonanceResource.CreateAt(10, 10);
        player.SetAetherResonance(resonance);
        var accumulated = AccumulatedAethericDamage.Create().AddDamage(30);
        player.SetAccumulatedAethericDamage(accumulated);
        player.CurrentAP = 5;

        _mockResonanceService.Setup(s => s.GetResonance(player)).Returns(resonance);
        _mockResonanceService.Setup(s => s.GetAccumulatedDamage(player)).Returns(accumulated);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert — All preconditions met
        readiness.Should().ContainKey(SeidkonaAbilityId.Unraveling);
        readiness[SeidkonaAbilityId.Unraveling].Should().BeTrue();
    }
}
