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
/// Unit tests for <see cref="BoneSetterAbilityService"/>.
/// Tests Tier 1 abilities: Field Dressing (active heal), Diagnose (information),
/// and Steady Hands (passive bonus integration).
/// </summary>
[TestFixture]
public class BoneSetterAbilityServiceTests
{
    private Mock<IBoneSetterMedicalSuppliesService> _mockSuppliesService = null!;
    private TestBoneSetterAbilityService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockSuppliesService = new Mock<IBoneSetterMedicalSuppliesService>();
        _service = new TestBoneSetterAbilityService(
            _mockSuppliesService.Object,
            Mock.Of<ILogger<BoneSetterAbilityService>>());
    }

    /// <summary>
    /// Test subclass that overrides dice methods for deterministic testing.
    /// </summary>
    private class TestBoneSetterAbilityService : BoneSetterAbilityService
    {
        public int Fixed2D6 { get; set; } = 7;

        public TestBoneSetterAbilityService(
            IBoneSetterMedicalSuppliesService suppliesService,
            ILogger<BoneSetterAbilityService> logger)
            : base(suppliesService, logger) { }

        internal override int Roll2D6() => Fixed2D6;
    }

    /// <summary>
    /// Creates a Bone-Setter player with the specified abilities unlocked and
    /// a default Medical Supplies inventory with one Bandage (Quality 2).
    /// </summary>
    private static Player CreateBoneSetter(params BoneSetterAbilityId[] abilities)
    {
        var player = new Player("Test Bone-Setter");
        player.SetSpecialization("bone-setter");

        // Initialize with a default supply
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "Bandage", "Basic bandage", 2, "salvage")
        };
        player.InitializeMedicalSupplies(supplies);

        player.CurrentAP = 10;
        foreach (var ability in abilities)
        {
            player.UnlockBoneSetterAbility(ability);
        }
        return player;
    }

    // ===== Field Dressing Tests =====

    [Test]
    public void ExecuteFieldDressing_WithValidPrereqs_ReturnsHealingResult()
    {
        // Arrange
        var player = CreateBoneSetter(BoneSetterAbilityId.FieldDressing);
        var targetId = Guid.NewGuid();
        var spentSupply = MedicalSupplyItem.Create(
            MedicalSupplyType.Bandage, "Bandage", "Basic bandage", 2, "salvage");

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.SpendSupply(player))
            .Returns(spentSupply);
        _mockSuppliesService
            .Setup(s => s.CalculateQualityBonus(spentSupply))
            .Returns(1); // Quality 2 - 1 = 1

        _service.Fixed2D6 = 7;

        // Act
        var result = _service.ExecuteFieldDressing(player, targetId, "Fighter", 30, 50);

        // Assert
        result.Should().NotBeNull();
        result!.HealingRoll.Should().Be(7); // Fixed2D6
        result.QualityBonus.Should().Be(1); // Quality 2 - 1
        result.SteadyHandsBonus.Should().Be(0); // Steady Hands not unlocked
        result.TotalHealing.Should().Be(8); // 7 + 1 + 0
        result.HpBefore.Should().Be(30);
        result.HpAfter.Should().Be(38); // 30 + 8 = 38, capped at 50
        result.TargetName.Should().Be("Fighter");
        result.SupplyTypeUsed.Should().Be("Bandage");
        player.CurrentAP.Should().Be(8); // 10 - 2
    }

    [Test]
    public void ExecuteFieldDressing_WithSteadyHands_AddsBonus()
    {
        // Arrange
        var player = CreateBoneSetter(
            BoneSetterAbilityId.FieldDressing,
            BoneSetterAbilityId.SteadyHands);
        var targetId = Guid.NewGuid();
        var spentSupply = MedicalSupplyItem.Create(
            MedicalSupplyType.Bandage, "Bandage", "Basic bandage", 3, "craft");

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.SpendSupply(player))
            .Returns(spentSupply);
        _mockSuppliesService
            .Setup(s => s.CalculateQualityBonus(spentSupply))
            .Returns(2); // Quality 3 - 1 = 2

        _service.Fixed2D6 = 8;

        // Act
        var result = _service.ExecuteFieldDressing(player, targetId, "Ranger", 20, 60);

        // Assert
        result.Should().NotBeNull();
        result!.HealingRoll.Should().Be(8);
        result.QualityBonus.Should().Be(2);
        result.SteadyHandsBonus.Should().Be(2); // Steady Hands unlocked
        result.TotalHealing.Should().Be(12); // 8 + 2 + 2
        result.HpAfter.Should().Be(32); // 20 + 12 = 32, capped at 60
    }

    [Test]
    public void ExecuteFieldDressing_HealingCappedAtMaxHp()
    {
        // Arrange
        var player = CreateBoneSetter(BoneSetterAbilityId.FieldDressing);
        var targetId = Guid.NewGuid();
        var spentSupply = MedicalSupplyItem.Create(
            MedicalSupplyType.Bandage, "Bandage", "desc", 5, "craft");

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.SpendSupply(player))
            .Returns(spentSupply);
        _mockSuppliesService
            .Setup(s => s.CalculateQualityBonus(spentSupply))
            .Returns(4); // Quality 5 - 1 = 4

        _service.Fixed2D6 = 12; // Max roll

        // Act — target nearly full HP: 48/50
        var result = _service.ExecuteFieldDressing(player, targetId, "Warrior", 48, 50);

        // Assert
        result.Should().NotBeNull();
        result!.TotalHealing.Should().Be(16); // 12 + 4 + 0
        result.HpAfter.Should().Be(50); // Capped at max HP (not 48 + 16 = 64)
    }

    [Test]
    public void ExecuteFieldDressing_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateBoneSetter(BoneSetterAbilityId.FieldDressing);
        player.CurrentAP = 1; // Need 2

        // Act
        var result = _service.ExecuteFieldDressing(player, Guid.NewGuid(), "Target", 30, 50);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(1); // Unchanged
    }

    [Test]
    public void ExecuteFieldDressing_NoSupplies_ReturnsNull()
    {
        // Arrange
        var player = CreateBoneSetter(BoneSetterAbilityId.FieldDressing);

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(false);

        // Act
        var result = _service.ExecuteFieldDressing(player, Guid.NewGuid(), "Target", 30, 50);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged — AP not deducted
    }

    [Test]
    public void ExecuteFieldDressing_WrongSpecialization_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Berserkr");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteFieldDressing(player, Guid.NewGuid(), "Target", 30, 50);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteFieldDressing_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange — Bone-Setter with no abilities unlocked
        var player = new Player("Test Bone-Setter");
        player.SetSpecialization("bone-setter");
        player.InitializeMedicalSupplies();
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteFieldDressing(player, Guid.NewGuid(), "Target", 30, 50);

        // Assert
        result.Should().BeNull();
    }

    // ===== Diagnose Tests =====

    [Test]
    public void ExecuteDiagnose_WithValidPrereqs_ReturnsDiagnosticResult()
    {
        // Arrange
        var player = CreateBoneSetter(BoneSetterAbilityId.Diagnose);
        var targetId = Guid.NewGuid();
        var effects = new[] { "Poisoned", "Bleeding" };
        var vulns = new[] { "Fire" };
        var resists = new[] { "Cold" };

        // Act
        var result = _service.ExecuteDiagnose(
            player, targetId, "Enemy Troll", 35, 100,
            effects, vulns, resists);

        // Assert
        result.Should().NotBeNull();
        result!.TargetName.Should().Be("Enemy Troll");
        result.CurrentHp.Should().Be(35);
        result.MaxHp.Should().Be(100);
        result.WoundSeverity.Should().Be(WoundSeverity.Serious); // 35% = Serious
        result.IsBloodied.Should().BeTrue(); // 35% ≤ 50%
        result.StatusEffects.Should().HaveCount(2);
        result.Vulnerabilities.Should().Contain("Fire");
        result.Resistances.Should().Contain("Cold");
        player.CurrentAP.Should().Be(9); // 10 - 1
    }

    [Test]
    public void ExecuteDiagnose_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateBoneSetter(BoneSetterAbilityId.Diagnose);
        player.CurrentAP = 0;

        // Act
        var result = _service.ExecuteDiagnose(
            player, Guid.NewGuid(), "Target", 50, 100, [], [], []);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteDiagnose_ClassifiesWoundSeverityCorrectly()
    {
        // Arrange
        var player = CreateBoneSetter(BoneSetterAbilityId.Diagnose);

        // Act & Assert — test each severity threshold
        var minor = _service.ExecuteDiagnose(
            player, Guid.NewGuid(), "T1", 95, 100, [], [], []);
        minor!.WoundSeverity.Should().Be(WoundSeverity.Minor); // 95% ≥ 90%

        player.CurrentAP = 10; // Reset AP
        var light = _service.ExecuteDiagnose(
            player, Guid.NewGuid(), "T2", 75, 100, [], [], []);
        light!.WoundSeverity.Should().Be(WoundSeverity.Light); // 75% in [70-89%]

        player.CurrentAP = 10;
        var moderate = _service.ExecuteDiagnose(
            player, Guid.NewGuid(), "T3", 50, 100, [], [], []);
        moderate!.WoundSeverity.Should().Be(WoundSeverity.Moderate); // 50% in [40-69%]

        player.CurrentAP = 10;
        var serious = _service.ExecuteDiagnose(
            player, Guid.NewGuid(), "T4", 25, 100, [], [], []);
        serious!.WoundSeverity.Should().Be(WoundSeverity.Serious); // 25% in [15-39%]

        player.CurrentAP = 10;
        var critical = _service.ExecuteDiagnose(
            player, Guid.NewGuid(), "T5", 10, 100, [], [], []);
        critical!.WoundSeverity.Should().Be(WoundSeverity.Critical); // 10% in [1-14%]

        player.CurrentAP = 10;
        var unconscious = _service.ExecuteDiagnose(
            player, Guid.NewGuid(), "T6", 0, 100, [], [], []);
        unconscious!.WoundSeverity.Should().Be(WoundSeverity.Unconscious); // 0%
    }

    // ===== GetAbilityReadiness Tests =====

    [Test]
    public void GetAbilityReadiness_FieldDressingReady_ReturnsTrue()
    {
        // Arrange
        var player = CreateBoneSetter(BoneSetterAbilityId.FieldDressing);

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(true);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().ContainKey(BoneSetterAbilityId.FieldDressing);
        readiness[BoneSetterAbilityId.FieldDressing].Should().BeTrue();
    }

    [Test]
    public void GetAbilityReadiness_SteadyHandsPassive_AlwaysReady()
    {
        // Arrange
        var player = CreateBoneSetter(BoneSetterAbilityId.SteadyHands);

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(false); // No supplies — shouldn't matter for passive

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().ContainKey(BoneSetterAbilityId.SteadyHands);
        readiness[BoneSetterAbilityId.SteadyHands].Should().BeTrue();
    }

    [Test]
    public void GetAbilityReadiness_NonBoneSetter_ReturnsEmpty()
    {
        // Arrange
        var player = new Player("Test Berserkr");
        player.SetSpecialization("berserkr");

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().BeEmpty();
    }

    // ===== PP Investment Tests =====

    [Test]
    public void GetPPInvested_Tier1OnlyAbilities_ReturnsZero()
    {
        // Arrange
        var player = CreateBoneSetter(
            BoneSetterAbilityId.FieldDressing,
            BoneSetterAbilityId.Diagnose,
            BoneSetterAbilityId.SteadyHands);

        // Act
        var pp = _service.GetPPInvested(player);

        // Assert
        pp.Should().Be(0); // All Tier 1 = 0 PP each
    }

    [Test]
    public void CanUnlockTier2_WithInsufficientPP_ReturnsFalse()
    {
        // Arrange — only Tier 1 abilities (0 PP)
        var player = CreateBoneSetter(
            BoneSetterAbilityId.FieldDressing,
            BoneSetterAbilityId.Diagnose,
            BoneSetterAbilityId.SteadyHands);

        // Act
        var canUnlock = _service.CanUnlockTier2(player);

        // Assert
        canUnlock.Should().BeFalse(); // Need 8 PP, have 0
    }

    [Test]
    public void CanUnlockTier2_NonBoneSetter_ReturnsFalse()
    {
        // Arrange
        var player = new Player("Test Warrior");
        player.SetSpecialization("berserkr");

        // Act
        var canUnlock = _service.CanUnlockTier2(player);

        // Assert
        canUnlock.Should().BeFalse();
    }
}
