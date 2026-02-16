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
/// Tests Tier 2 abilities: Emergency Surgery (high-impact heal), Antidote Craft
/// (crafting), and Triage (passive healing bonus).
/// Tests Tier 3 abilities: Resuscitate (revival), Preventive Care (passive aura).
/// Tests Capstone ability: Miracle Worker (full heal + condition clear).
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
    /// Provides <see cref="Fixed2D6"/> for Field Dressing and <see cref="Fixed4D6"/>
    /// for Emergency Surgery.
    /// </summary>
    private class TestBoneSetterAbilityService : BoneSetterAbilityService
    {
        public int Fixed2D6 { get; set; } = 7;
        public int Fixed4D6 { get; set; } = 14;

        public TestBoneSetterAbilityService(
            IBoneSetterMedicalSuppliesService suppliesService,
            ILogger<BoneSetterAbilityService> logger)
            : base(suppliesService, logger) { }

        internal override int Roll2D6() => Fixed2D6;
        internal override int Roll4D6() => Fixed4D6;
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

    // ===== Tier 2 Test Helpers =====

    /// <summary>
    /// Creates a Tier 2 Bone-Setter player with all Tier 1 and Tier 2 abilities unlocked.
    /// Unlocks: 3 T1 (0 PP each) + 3 T2 (4 PP each = 12 PP) → 12 PP total, meeting
    /// the 8 PP Tier 2 requirement.
    /// </summary>
    private static Player CreateTier2BoneSetter(params BoneSetterAbilityId[] extraAbilities)
    {
        var player = CreateBoneSetter(
            BoneSetterAbilityId.FieldDressing,
            BoneSetterAbilityId.Diagnose,
            BoneSetterAbilityId.SteadyHands,
            BoneSetterAbilityId.EmergencySurgery,
            BoneSetterAbilityId.AntidoteCraft,
            BoneSetterAbilityId.Triage);

        foreach (var ability in extraAbilities)
            player.UnlockBoneSetterAbility(ability);

        return player;
    }

    /// <summary>
    /// Sets up the mock supplies service for Emergency Surgery scenarios.
    /// Emergency Surgery uses the highest-quality supply available.
    /// </summary>
    /// <param name="player">The player to configure mocks for.</param>
    /// <param name="supplyType">Supply type to return from GetHighestQualitySupply.</param>
    /// <param name="quality">Quality rating of the supply.</param>
    /// <param name="qualityBonus">Expected quality bonus (quality - 1).</param>
    private void SetupEmergencySurgeryMocks(
        Player player,
        MedicalSupplyType supplyType,
        int quality,
        int qualityBonus)
    {
        var supply = MedicalSupplyItem.Create(
            supplyType, $"Test {supplyType}", $"Quality {quality} {supplyType}", quality, "salvage");

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.GetHighestQualitySupply(player))
            .Returns(supply);
        _mockSuppliesService
            .Setup(s => s.SpendSupply(player, supplyType))
            .Returns(supply);
        _mockSuppliesService
            .Setup(s => s.CalculateQualityBonus(supply))
            .Returns(qualityBonus);
    }

    // ===== Emergency Surgery Tests (v0.20.6b) =====

    [Test]
    public void ExecuteEmergencySurgery_WithValidPrereqs_ReturnsHealingResult()
    {
        // Arrange
        var player = CreateTier2BoneSetter();
        var targetId = Guid.NewGuid();
        SetupEmergencySurgeryMocks(player, MedicalSupplyType.Suture, 3, 2);
        _service.Fixed4D6 = 14;

        // Act
        var result = _service.ExecuteEmergencySurgery(
            player, targetId, "Fighter", 20, 80, RecoveryCondition.Active);

        // Assert
        result.Should().NotBeNull();
        result!.HealingRoll.Should().Be(14);           // Fixed4D6
        result.QualityBonus.Should().Be(2);             // Quality 3 - 1
        result.SteadyHandsBonus.Should().Be(2);         // Steady Hands unlocked (Tier 2 includes all T1)
        result.RecoveryBonus.Should().Be(0);             // Active condition = no bonus
        result.TotalHealing.Should().Be(18);             // 14 + 2 + 2 + 0
        result.HpBefore.Should().Be(20);
        result.HpAfter.Should().Be(38);                  // 20 + 18 = 38, capped at 80
        result.TargetName.Should().Be("Fighter");
        result.SupplyTypeUsed.Should().Be("Suture");
        result.SuppliesUsed.Should().Be(1);
        result.BonusTriggered.Should().BeFalse();
        result.TargetCondition.Should().BeNull();
        player.CurrentAP.Should().Be(7);                 // 10 - 3
    }

    [Test]
    public void ExecuteEmergencySurgery_WithSteadyHands_AddsBonus()
    {
        // Arrange — Tier 2 player inherently has Steady Hands unlocked
        var player = CreateTier2BoneSetter();
        var targetId = Guid.NewGuid();
        SetupEmergencySurgeryMocks(player, MedicalSupplyType.Bandage, 2, 1);
        _service.Fixed4D6 = 10;

        // Act
        var result = _service.ExecuteEmergencySurgery(
            player, targetId, "Ranger", 30, 60, RecoveryCondition.Active);

        // Assert
        result.Should().NotBeNull();
        result!.SteadyHandsBonus.Should().Be(2);        // +2 from Steady Hands
        result.TotalHealing.Should().Be(13);              // 10 + 1 + 2 + 0
    }

    [Test]
    public void ExecuteEmergencySurgery_RecoveringTarget_AppliesRecoveryBonus()
    {
        // Arrange
        var player = CreateTier2BoneSetter();
        var targetId = Guid.NewGuid();
        SetupEmergencySurgeryMocks(player, MedicalSupplyType.Suture, 4, 3);
        _service.Fixed4D6 = 16;

        // Act
        var result = _service.ExecuteEmergencySurgery(
            player, targetId, "Warrior", 15, 100, RecoveryCondition.Recovering);

        // Assert
        result.Should().NotBeNull();
        result!.RecoveryBonus.Should().Be(3);            // Recovering = +3
        result.BonusTriggered.Should().BeTrue();
        result.TargetCondition.Should().Be(RecoveryCondition.Recovering);
        result.TotalHealing.Should().Be(24);              // 16 + 3 + 2 + 3
    }

    [Test]
    public void ExecuteEmergencySurgery_DyingTarget_AppliesMaxRecoveryBonus()
    {
        // Arrange
        var player = CreateTier2BoneSetter();
        var targetId = Guid.NewGuid();
        SetupEmergencySurgeryMocks(player, MedicalSupplyType.Suture, 5, 4);
        _service.Fixed4D6 = 20;

        // Act
        var result = _service.ExecuteEmergencySurgery(
            player, targetId, "Cleric", 2, 80, RecoveryCondition.Dying);

        // Assert
        result.Should().NotBeNull();
        result!.RecoveryBonus.Should().Be(4);            // Dying = +4 (maximum)
        result.BonusTriggered.Should().BeTrue();
        result.TargetCondition.Should().Be(RecoveryCondition.Dying);
        result.TotalHealing.Should().Be(30);              // 20 + 4 + 2 + 4
    }

    [Test]
    public void ExecuteEmergencySurgery_IncapacitatedTarget_AppliesRecoveryBonus()
    {
        // Arrange
        var player = CreateTier2BoneSetter();
        var targetId = Guid.NewGuid();
        SetupEmergencySurgeryMocks(player, MedicalSupplyType.Bandage, 2, 1);
        _service.Fixed4D6 = 12;

        // Act
        var result = _service.ExecuteEmergencySurgery(
            player, targetId, "Scout", 5, 60, RecoveryCondition.Incapacitated);

        // Assert
        result.Should().NotBeNull();
        result!.RecoveryBonus.Should().Be(1);            // Incapacitated = +1
        result.BonusTriggered.Should().BeTrue();
        result.TargetCondition.Should().Be(RecoveryCondition.Incapacitated);
        result.TotalHealing.Should().Be(16);              // 12 + 1 + 2 + 1
    }

    [Test]
    public void ExecuteEmergencySurgery_HealingCappedAtMaxHp()
    {
        // Arrange — target nearly full HP, high healing roll
        var player = CreateTier2BoneSetter();
        var targetId = Guid.NewGuid();
        SetupEmergencySurgeryMocks(player, MedicalSupplyType.Suture, 5, 4);
        _service.Fixed4D6 = 24; // Max 4d6 roll

        // Act — target at 75/80 HP, healing 24 + 4 + 2 + 4 = 34 would overshoot
        var result = _service.ExecuteEmergencySurgery(
            player, targetId, "Tank", 75, 80, RecoveryCondition.Dying);

        // Assert
        result.Should().NotBeNull();
        result!.TotalHealing.Should().Be(34);             // 24 + 4 + 2 + 4
        result.HpAfter.Should().Be(80);                   // Capped at max HP (not 75 + 34 = 109)
    }

    [Test]
    public void ExecuteEmergencySurgery_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateTier2BoneSetter();
        player.CurrentAP = 2; // Need 3

        // Act
        var result = _service.ExecuteEmergencySurgery(
            player, Guid.NewGuid(), "Target", 30, 50, RecoveryCondition.Active);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(2); // Unchanged
    }

    [Test]
    public void ExecuteEmergencySurgery_NoSupplies_ReturnsNull()
    {
        // Arrange
        var player = CreateTier2BoneSetter();

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(false);

        // Act
        var result = _service.ExecuteEmergencySurgery(
            player, Guid.NewGuid(), "Target", 30, 50, RecoveryCondition.Active);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged — AP not deducted
    }

    [Test]
    public void ExecuteEmergencySurgery_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange — Bone-Setter with only Tier 1 abilities (no Emergency Surgery)
        var player = CreateBoneSetter(
            BoneSetterAbilityId.FieldDressing,
            BoneSetterAbilityId.Diagnose,
            BoneSetterAbilityId.SteadyHands);

        // Act
        var result = _service.ExecuteEmergencySurgery(
            player, Guid.NewGuid(), "Target", 30, 50, RecoveryCondition.Active);

        // Assert
        result.Should().BeNull();
    }

    // ===== Antidote Craft Tests (v0.20.6b) =====

    [Test]
    public void ExecuteAntidoteCraft_WithAllMaterials_CreatesAntidote()
    {
        // Arrange
        var player = CreateTier2BoneSetter();
        var herbsSupply = MedicalSupplyItem.Create(
            MedicalSupplyType.Herbs, "Herbs", "Fresh herbs", 3, "salvage");

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.SpendSupply(player, MedicalSupplyType.Herbs))
            .Returns(herbsSupply);
        _mockSuppliesService
            .Setup(s => s.AddSupply(player, It.IsAny<MedicalSupplyItem>()))
            .Returns(true);

        var materials = new[]
        {
            CraftingMaterial.Create(CraftingMaterialType.PlantFiber, 3, 2),
            CraftingMaterial.Create(CraftingMaterialType.MineralPowder, 2, 2)
        };

        // Act
        var result = _service.ExecuteAntidoteCraft(player, materials);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.RecipeUsed.Should().Be("Basic Antidote");
        result.CraftedQuality.Should().Be(3);            // Herbs Q3 + 0 bonus (materials not all Q3+)
        result.CreatedAntidote.Should().NotBeNull();
        result.CreatedAntidote!.SupplyType.Should().Be(MedicalSupplyType.Antidote);
        result.CreatedAntidote.Quality.Should().Be(3);
        result.MaterialsConsumed.Should().ContainKey("Herbs");
        result.MaterialsConsumed.Should().ContainKey("Plant Fiber");
        result.MaterialsConsumed.Should().ContainKey("Mineral Powder");
        player.CurrentAP.Should().Be(8);                  // 10 - 2
    }

    [Test]
    public void ExecuteAntidoteCraft_HighQualityMaterials_IncrementsQuality()
    {
        // Arrange — all materials Q3+ triggers +1 bonus
        var player = CreateTier2BoneSetter();
        var herbsSupply = MedicalSupplyItem.Create(
            MedicalSupplyType.Herbs, "Herbs", "Premium herbs", 3, "craft");

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.SpendSupply(player, MedicalSupplyType.Herbs))
            .Returns(herbsSupply);
        _mockSuppliesService
            .Setup(s => s.AddSupply(player, It.IsAny<MedicalSupplyItem>()))
            .Returns(true);

        var materials = new[]
        {
            CraftingMaterial.Create(CraftingMaterialType.PlantFiber, 3, 3), // Q3 ≥ threshold
            CraftingMaterial.Create(CraftingMaterialType.MineralPowder, 2, 4) // Q4 ≥ threshold
        };

        // Act
        var result = _service.ExecuteAntidoteCraft(player, materials);

        // Assert
        result.Should().NotBeNull();
        result!.CraftedQuality.Should().Be(4);            // Herbs Q3 + 1 bonus = 4
    }

    [Test]
    public void ExecuteAntidoteCraft_QualityCappedAt5()
    {
        // Arrange — Herbs Q5 + high quality bonus would be 6, capped at 5
        var player = CreateTier2BoneSetter();
        var herbsSupply = MedicalSupplyItem.Create(
            MedicalSupplyType.Herbs, "Herbs", "Superior herbs", 5, "quest_reward");

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.SpendSupply(player, MedicalSupplyType.Herbs))
            .Returns(herbsSupply);
        _mockSuppliesService
            .Setup(s => s.AddSupply(player, It.IsAny<MedicalSupplyItem>()))
            .Returns(true);

        var materials = new[]
        {
            CraftingMaterial.Create(CraftingMaterialType.PlantFiber, 3, 5), // Q5
            CraftingMaterial.Create(CraftingMaterialType.MineralPowder, 2, 5) // Q5
        };

        // Act
        var result = _service.ExecuteAntidoteCraft(player, materials);

        // Assert
        result.Should().NotBeNull();
        result!.CraftedQuality.Should().Be(5);            // Min(5 + 1, 5) = 5 (capped)
    }

    [Test]
    public void ExecuteAntidoteCraft_MissingHerbs_ReturnsNull()
    {
        // Arrange — no Herbs supply available
        var player = CreateTier2BoneSetter();

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
            .Returns(false);

        var materials = new[]
        {
            CraftingMaterial.Create(CraftingMaterialType.PlantFiber, 3, 2),
            CraftingMaterial.Create(CraftingMaterialType.MineralPowder, 2, 2)
        };

        // Act
        var result = _service.ExecuteAntidoteCraft(player, materials);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged
    }

    [Test]
    public void ExecuteAntidoteCraft_InsufficientPlantFiber_ReturnsFailure()
    {
        // Arrange — only 1 Plant Fiber, need 2
        var player = CreateTier2BoneSetter();

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
            .Returns(true);

        var materials = new[]
        {
            CraftingMaterial.Create(CraftingMaterialType.PlantFiber, 1, 2), // Only 1, need 2
            CraftingMaterial.Create(CraftingMaterialType.MineralPowder, 2, 2)
        };

        // Act
        var result = _service.ExecuteAntidoteCraft(player, materials);

        // Assert — returns failure result (not null) per AntidoteCraftResult design
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Plant Fiber");
        player.CurrentAP.Should().Be(10); // Unchanged — AP not deducted on material failure
    }

    [Test]
    public void ExecuteAntidoteCraft_InsufficientMineralPowder_ReturnsFailure()
    {
        // Arrange — no Mineral Powder
        var player = CreateTier2BoneSetter();

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
            .Returns(true);

        var materials = new[]
        {
            CraftingMaterial.Create(CraftingMaterialType.PlantFiber, 3, 2)
            // No MineralPowder provided
        };

        // Act
        var result = _service.ExecuteAntidoteCraft(player, materials);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Mineral Powder");
    }

    [Test]
    public void ExecuteAntidoteCraft_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateTier2BoneSetter();
        player.CurrentAP = 1; // Need 2

        // Act
        var result = _service.ExecuteAntidoteCraft(player, []);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(1); // Unchanged
    }

    // ===== Triage Tests (v0.20.6b) =====

    [Test]
    public void EvaluateTriage_IdentifiesMostWounded_ReturnsBonus()
    {
        // Arrange
        var player = CreateTier2BoneSetter();
        var allies = new[]
        {
            TriageTarget.Create(Guid.NewGuid(), "Ranger", 40, 60),    // 66.7%
            TriageTarget.Create(Guid.NewGuid(), "Fighter", 10, 80),   // 12.5% — most wounded
            TriageTarget.Create(Guid.NewGuid(), "Cleric", 50, 50)     // 100%
        };

        // Act
        var result = _service.EvaluateTriage(player, allies, 14);

        // Assert
        result.Should().NotBeNull();
        result!.MostWoundedTargetName.Should().Be("Fighter");
        result.MostWoundedHpPercentage.Should().BeApproximately(0.125f, 0.001f);
        result.TargetsInRadius.Should().Be(3);
        result.BaseHealing.Should().Be(14);
        result.BonusHealing.Should().Be(7);              // (int)(14 * 0.5) = 7
        result.TotalHealing.Should().Be(21);              // 14 + 7
    }

    [Test]
    public void EvaluateTriage_SingleAlly_ReturnsBonus()
    {
        // Arrange
        var player = CreateTier2BoneSetter();
        var allies = new[]
        {
            TriageTarget.Create(Guid.NewGuid(), "Scout", 20, 40) // 50%
        };

        // Act
        var result = _service.EvaluateTriage(player, allies, 10);

        // Assert
        result.Should().NotBeNull();
        result!.MostWoundedTargetName.Should().Be("Scout");
        result.BonusHealing.Should().Be(5);              // (int)(10 * 0.5) = 5
        result.TotalHealing.Should().Be(15);
        result.TargetsInRadius.Should().Be(1);
    }

    [Test]
    public void EvaluateTriage_NoAlliesInRadius_ReturnsNull()
    {
        // Arrange
        var player = CreateTier2BoneSetter();

        // Act
        var result = _service.EvaluateTriage(player, [], 14);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void EvaluateTriage_NullAlliesArray_ReturnsNull()
    {
        // Arrange
        var player = CreateTier2BoneSetter();

        // Act
        var result = _service.EvaluateTriage(player, null!, 14);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void EvaluateTriage_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange — Bone-Setter without Triage unlocked
        var player = CreateBoneSetter(
            BoneSetterAbilityId.FieldDressing,
            BoneSetterAbilityId.Diagnose,
            BoneSetterAbilityId.SteadyHands);

        var allies = new[]
        {
            TriageTarget.Create(Guid.NewGuid(), "Fighter", 10, 80)
        };

        // Act
        var result = _service.EvaluateTriage(player, allies, 14);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void EvaluateTriage_OddBaseHealing_RoundsDown()
    {
        // Arrange — bonus for odd number should round down via integer cast
        var player = CreateTier2BoneSetter();
        var allies = new[]
        {
            TriageTarget.Create(Guid.NewGuid(), "Mage", 5, 50)
        };

        // Act — base healing 9: (int)(9 * 0.5) = (int)(4.5) = 4
        var result = _service.EvaluateTriage(player, allies, 9);

        // Assert
        result.Should().NotBeNull();
        result!.BonusHealing.Should().Be(4);              // Rounded down
        result.TotalHealing.Should().Be(13);               // 9 + 4
    }

    // ===== Tier 2 Readiness & PP Tests (v0.20.6b) =====

    [Test]
    public void GetAbilityReadiness_IncludesTier2Abilities()
    {
        // Arrange
        var player = CreateTier2BoneSetter();

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
            .Returns(true);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().ContainKey(BoneSetterAbilityId.EmergencySurgery);
        readiness[BoneSetterAbilityId.EmergencySurgery].Should().BeTrue();

        readiness.Should().ContainKey(BoneSetterAbilityId.AntidoteCraft);
        readiness[BoneSetterAbilityId.AntidoteCraft].Should().BeTrue();

        readiness.Should().ContainKey(BoneSetterAbilityId.Triage);
        readiness[BoneSetterAbilityId.Triage].Should().BeTrue(); // Passive always ready
    }

    [Test]
    public void GetAbilityReadiness_EmergencySurgeryInsufficientAP_ReturnsFalse()
    {
        // Arrange
        var player = CreateTier2BoneSetter();
        player.CurrentAP = 2; // Need 3 for Emergency Surgery

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
            .Returns(true);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness[BoneSetterAbilityId.EmergencySurgery].Should().BeFalse();
        readiness[BoneSetterAbilityId.AntidoteCraft].Should().BeTrue();  // Only needs 2 AP
        readiness[BoneSetterAbilityId.Triage].Should().BeTrue();         // Passive
    }

    [Test]
    public void CanUnlockTier2_WithTier2Unlocked_ReturnsTrue()
    {
        // Arrange — 3 T2 abilities × 4 PP each = 12 PP invested
        var player = CreateTier2BoneSetter();

        // Act
        var canUnlock = _service.CanUnlockTier2(player);

        // Assert
        canUnlock.Should().BeTrue(); // 12 PP ≥ 8 PP requirement
    }

    [Test]
    public void GetPPInvested_WithTier2Abilities_ReturnsCorrectTotal()
    {
        // Arrange — 3 T1 (0 PP) + 3 T2 (4 PP each) = 12 PP
        var player = CreateTier2BoneSetter();

        // Act
        var pp = _service.GetPPInvested(player);

        // Assert
        pp.Should().Be(12); // 3 × 4 PP (Tier 2 abilities)
    }

    // ===== Tier 3 Test Helpers (v0.20.6c) =====

    /// <summary>
    /// Creates a Tier 3 Bone-Setter player with all Tier 1, Tier 2, and Tier 3 abilities unlocked.
    /// Unlocks: 3 T1 (0 PP each) + 3 T2 (4 PP each = 12 PP) + 2 T3 (5 PP each = 10 PP) → 22 PP total.
    /// Also initializes with 3 supplies (2 Bandages + 1 Suture) to support Resuscitate's 2-supply cost.
    /// </summary>
    private static Player CreateTier3BoneSetter(params BoneSetterAbilityId[] extraAbilities)
    {
        var player = new Player("Test Bone-Setter");
        player.SetSpecialization("bone-setter");

        // Initialize with multiple supplies for Resuscitate testing (needs 2)
        var supplies = new[]
        {
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "Bandage", "Basic bandage", 2, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Bandage, "Bandage", "Basic bandage", 1, "salvage"),
            MedicalSupplyItem.Create(MedicalSupplyType.Suture, "Suture", "Quality suture", 3, "craft")
        };
        player.InitializeMedicalSupplies(supplies);

        player.CurrentAP = 10;

        // Unlock all T1 + T2 + T3 abilities
        player.UnlockBoneSetterAbility(BoneSetterAbilityId.FieldDressing);
        player.UnlockBoneSetterAbility(BoneSetterAbilityId.Diagnose);
        player.UnlockBoneSetterAbility(BoneSetterAbilityId.SteadyHands);
        player.UnlockBoneSetterAbility(BoneSetterAbilityId.EmergencySurgery);
        player.UnlockBoneSetterAbility(BoneSetterAbilityId.AntidoteCraft);
        player.UnlockBoneSetterAbility(BoneSetterAbilityId.Triage);
        player.UnlockBoneSetterAbility(BoneSetterAbilityId.Resuscitate);
        player.UnlockBoneSetterAbility(BoneSetterAbilityId.PreventiveCare);

        foreach (var ability in extraAbilities)
            player.UnlockBoneSetterAbility(ability);

        return player;
    }

    /// <summary>
    /// Sets up the mock supplies service for Resuscitate scenarios.
    /// Resuscitate consumes 2 supplies sequentially (lowest quality first).
    /// Uses <see cref="MockSequence"/> to ensure correct ordering.
    /// </summary>
    /// <param name="player">The player to configure mocks for.</param>
    private void SetupResuscitateMocks(Player player)
    {
        var firstSupply = MedicalSupplyItem.Create(
            MedicalSupplyType.Bandage, "Bandage", "Basic bandage", 1, "salvage");
        var secondSupply = MedicalSupplyItem.Create(
            MedicalSupplyType.Bandage, "Bandage", "Basic bandage", 2, "salvage");

        _mockSuppliesService
            .SetupSequence(s => s.SpendSupply(player))
            .Returns(firstSupply)
            .Returns(secondSupply);
    }

    // ===== Resuscitate Tests (v0.20.6c) =====

    [Test]
    public void ExecuteResuscitate_WithValidPrereqs_ReturnsRevivalResult()
    {
        // Arrange
        var player = CreateTier3BoneSetter();
        var targetId = Guid.NewGuid();
        SetupResuscitateMocks(player);

        // Act
        var result = _service.ExecuteResuscitate(player, targetId, "Fallen Warrior", 0);

        // Assert
        result.Should().NotBeNull();
        result!.TargetName.Should().Be("Fallen Warrior");
        result.TargetId.Should().Be(targetId);
        result.HpBefore.Should().Be(0);
        result.HpAfter.Should().Be(1);                       // Always 1 HP
        result.SuppliesUsed.Should().Be(2);                   // Always 2 supplies
        result.Method.Should().Be(ResurrectionMethod.SkillBasedResuscitation);
        result.ResurrectionMessage.Should().NotBeEmpty();
        player.CurrentAP.Should().Be(6);                      // 10 - 4
    }

    [Test]
    public void ExecuteResuscitate_TargetNotAtZeroHp_ReturnsNull()
    {
        // Arrange — target at 5 HP (not unconscious)
        var player = CreateTier3BoneSetter();

        // Act
        var result = _service.ExecuteResuscitate(player, Guid.NewGuid(), "Fighter", 5);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged
    }

    [Test]
    public void ExecuteResuscitate_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3BoneSetter();
        player.CurrentAP = 3; // Need 4

        // Act
        var result = _service.ExecuteResuscitate(player, Guid.NewGuid(), "Target", 0);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(3); // Unchanged
    }

    [Test]
    public void ExecuteResuscitate_InsufficientSupplies_ReturnsNull()
    {
        // Arrange — only 1 supply available, need 2
        var player = CreateBoneSetter(
            BoneSetterAbilityId.FieldDressing,
            BoneSetterAbilityId.Diagnose,
            BoneSetterAbilityId.SteadyHands,
            BoneSetterAbilityId.EmergencySurgery,
            BoneSetterAbilityId.AntidoteCraft,
            BoneSetterAbilityId.Triage,
            BoneSetterAbilityId.Resuscitate,
            BoneSetterAbilityId.PreventiveCare);

        // CreateBoneSetter only provides 1 supply — insufficient for Resuscitate

        // Act
        var result = _service.ExecuteResuscitate(player, Guid.NewGuid(), "Target", 0);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged
    }

    [Test]
    public void ExecuteResuscitate_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange — Bone-Setter without Resuscitate
        var player = CreateTier2BoneSetter();

        // Act
        var result = _service.ExecuteResuscitate(player, Guid.NewGuid(), "Target", 0);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteResuscitate_DeductsCorrectAP()
    {
        // Arrange
        var player = CreateTier3BoneSetter();
        SetupResuscitateMocks(player);
        player.CurrentAP = 6;

        // Act
        var result = _service.ExecuteResuscitate(player, Guid.NewGuid(), "Warrior", 0);

        // Assert
        result.Should().NotBeNull();
        player.CurrentAP.Should().Be(2); // 6 - 4
    }

    [Test]
    public void ExecuteResuscitate_SpendsTwoSupplies()
    {
        // Arrange
        var player = CreateTier3BoneSetter();
        SetupResuscitateMocks(player);

        // Act
        _service.ExecuteResuscitate(player, Guid.NewGuid(), "Warrior", 0);

        // Assert — SpendSupply called exactly twice
        _mockSuppliesService.Verify(
            s => s.SpendSupply(player),
            Times.Exactly(2));
    }

    [Test]
    public void ExecuteResuscitate_WrongSpecialization_ReturnsNull()
    {
        // Arrange
        var player = new Player("Test Berserkr");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;

        // Act
        var result = _service.ExecuteResuscitate(player, Guid.NewGuid(), "Target", 0);

        // Assert
        result.Should().BeNull();
    }

    // ===== Preventive Care Tests (v0.20.6c) =====

    [Test]
    public void EvaluatePreventiveCare_WithAllies_ReturnsAura()
    {
        // Arrange
        var player = CreateTier3BoneSetter();
        var allyIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = _service.EvaluatePreventiveCare(player, allyIds);

        // Assert
        result.Should().NotBeNull();
        result!.BoneSetterId.Should().Be(player.Id);
        result.AuraRadius.Should().Be(5);
        result.PoisonSaveBonus.Should().Be(1);
        result.DiseaseSaveBonus.Should().Be(1);
        result.IsActive.Should().BeTrue();
        result.AffectedAllyIds.Should().HaveCount(3);
        result.AffectedAllyIds.Should().BeEquivalentTo(allyIds);
    }

    [Test]
    public void EvaluatePreventiveCare_NoAllies_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3BoneSetter();

        // Act
        var result = _service.EvaluatePreventiveCare(player, []);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void EvaluatePreventiveCare_NullAllies_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3BoneSetter();

        // Act
        var result = _service.EvaluatePreventiveCare(player, null!);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void EvaluatePreventiveCare_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange — Bone-Setter without Preventive Care
        var player = CreateTier2BoneSetter();
        var allyIds = new[] { Guid.NewGuid() };

        // Act
        var result = _service.EvaluatePreventiveCare(player, allyIds);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void EvaluatePreventiveCare_AuraContainsCorrectAllyIds()
    {
        // Arrange
        var player = CreateTier3BoneSetter();
        var ally1 = Guid.NewGuid();
        var ally2 = Guid.NewGuid();
        var allyIds = new[] { ally1, ally2 };

        // Act
        var result = _service.EvaluatePreventiveCare(player, allyIds);

        // Assert
        result.Should().NotBeNull();
        result!.AffectedAllyIds.Should().Contain(ally1);
        result.AffectedAllyIds.Should().Contain(ally2);
        result.AffectedAllyIds.Should().HaveCount(2);
    }

    // ===== Miracle Worker Tests (v0.20.6c) =====

    [Test]
    public void ExecuteMiracleWorker_WithValidPrereqs_FullHeals()
    {
        // Arrange
        var player = CreateTier3BoneSetter(BoneSetterAbilityId.MiracleWorker);
        var targetId = Guid.NewGuid();
        var conditions = new[] { "Poisoned", "Blinded" };

        // Act
        var result = _service.ExecuteMiracleWorker(
            player, targetId, "Dying Cleric", 15, 100, conditions);

        // Assert
        result.Should().NotBeNull();
        result!.TargetName.Should().Be("Dying Cleric");
        result.TargetId.Should().Be(targetId);
        result.HpBefore.Should().Be(15);
        result.HpAfter.Should().Be(100);                     // Full HP restore
        result.MaxHp.Should().Be(100);
        result.TotalHealing.Should().Be(85);                  // 100 - 15
        result.MiracleMessage.Should().NotBeEmpty();
        player.CurrentAP.Should().Be(5);                      // 10 - 5
    }

    [Test]
    public void ExecuteMiracleWorker_ClearsConditions()
    {
        // Arrange
        var player = CreateTier3BoneSetter(BoneSetterAbilityId.MiracleWorker);
        var conditions = new[] { "Poisoned", "Blinded", "Stunned" };

        // Act
        var result = _service.ExecuteMiracleWorker(
            player, Guid.NewGuid(), "Fighter", 20, 80, conditions);

        // Assert
        result.Should().NotBeNull();
        result!.ConditionsCleared.Should().Be(3);
        result.ClearedConditions.Should().Contain("Poisoned");
        result.ClearedConditions.Should().Contain("Blinded");
        result.ClearedConditions.Should().Contain("Stunned");
    }

    [Test]
    public void ExecuteMiracleWorker_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3BoneSetter(BoneSetterAbilityId.MiracleWorker);
        player.CurrentAP = 4; // Need 5

        // Act
        var result = _service.ExecuteMiracleWorker(
            player, Guid.NewGuid(), "Target", 30, 100, []);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(4); // Unchanged
    }

    [Test]
    public void ExecuteMiracleWorker_AlreadyUsedThisRest_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3BoneSetter(BoneSetterAbilityId.MiracleWorker);
        player.HasUsedMiracleWorkerThisRestCycle = true; // Already used

        // Act
        var result = _service.ExecuteMiracleWorker(
            player, Guid.NewGuid(), "Target", 30, 100, []);

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged
    }

    [Test]
    public void ExecuteMiracleWorker_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange — Tier 3 player without capstone
        var player = CreateTier3BoneSetter();

        // Act
        var result = _service.ExecuteMiracleWorker(
            player, Guid.NewGuid(), "Target", 30, 100, []);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteMiracleWorker_SetsRestCycleCooldown()
    {
        // Arrange
        var player = CreateTier3BoneSetter(BoneSetterAbilityId.MiracleWorker);
        player.HasUsedMiracleWorkerThisRestCycle.Should().BeFalse(); // Precondition

        // Act
        var result = _service.ExecuteMiracleWorker(
            player, Guid.NewGuid(), "Target", 30, 100, []);

        // Assert
        result.Should().NotBeNull();
        player.HasUsedMiracleWorkerThisRestCycle.Should().BeTrue(); // Cooldown set
    }

    [Test]
    public void ExecuteMiracleWorker_NoSupplyCost()
    {
        // Arrange
        var player = CreateTier3BoneSetter(BoneSetterAbilityId.MiracleWorker);

        // Act
        _service.ExecuteMiracleWorker(
            player, Guid.NewGuid(), "Target", 30, 100, []);

        // Assert — SpendSupply should never be called
        _mockSuppliesService.Verify(
            s => s.SpendSupply(It.IsAny<Player>()),
            Times.Never);
        _mockSuppliesService.Verify(
            s => s.SpendSupply(It.IsAny<Player>(), It.IsAny<MedicalSupplyType>()),
            Times.Never);
    }

    [Test]
    public void ExecuteMiracleWorker_EmptyConditions_ReturnsResult()
    {
        // Arrange
        var player = CreateTier3BoneSetter(BoneSetterAbilityId.MiracleWorker);

        // Act
        var result = _service.ExecuteMiracleWorker(
            player, Guid.NewGuid(), "Healthy Ranger", 50, 80, []);

        // Assert
        result.Should().NotBeNull();
        result!.ConditionsCleared.Should().Be(0);
        result.ClearedConditions.Should().BeEmpty();
        result.HpAfter.Should().Be(80);
        result.TotalHealing.Should().Be(30);
    }

    // ===== Tier 3 Readiness & PP Tests (v0.20.6c) =====

    [Test]
    public void GetAbilityReadiness_IncludesTier3Abilities()
    {
        // Arrange
        var player = CreateTier3BoneSetter();

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
            .Returns(true);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert — Tier 3 abilities
        readiness.Should().ContainKey(BoneSetterAbilityId.Resuscitate);
        readiness[BoneSetterAbilityId.Resuscitate].Should().BeTrue();

        readiness.Should().ContainKey(BoneSetterAbilityId.PreventiveCare);
        readiness[BoneSetterAbilityId.PreventiveCare].Should().BeTrue(); // Passive always ready
    }

    [Test]
    public void GetAbilityReadiness_ResuscitateNeedsTwoSupplies()
    {
        // Arrange — player with only 1 supply (CreateBoneSetter default)
        var player = CreateBoneSetter(
            BoneSetterAbilityId.FieldDressing,
            BoneSetterAbilityId.Diagnose,
            BoneSetterAbilityId.SteadyHands,
            BoneSetterAbilityId.EmergencySurgery,
            BoneSetterAbilityId.AntidoteCraft,
            BoneSetterAbilityId.Triage,
            BoneSetterAbilityId.Resuscitate,
            BoneSetterAbilityId.PreventiveCare);

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
            .Returns(false);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert — only 1 supply, Resuscitate needs 2
        readiness.Should().ContainKey(BoneSetterAbilityId.Resuscitate);
        readiness[BoneSetterAbilityId.Resuscitate].Should().BeFalse();
    }

    [Test]
    public void GetAbilityReadiness_MiracleWorkerOnCooldown_ReturnsFalse()
    {
        // Arrange
        var player = CreateTier3BoneSetter(BoneSetterAbilityId.MiracleWorker);
        player.HasUsedMiracleWorkerThisRestCycle = true; // Cooldown active

        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player))
            .Returns(true);
        _mockSuppliesService
            .Setup(s => s.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
            .Returns(true);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().ContainKey(BoneSetterAbilityId.MiracleWorker);
        readiness[BoneSetterAbilityId.MiracleWorker].Should().BeFalse();
    }

    [Test]
    public void CanUnlockTier3_WithTier3Unlocked_ReturnsTrue()
    {
        // Arrange — T3 player has 22 PP invested (3×0 + 3×4 + 2×5 = 22)
        var player = CreateTier3BoneSetter();

        // Act
        var canUnlock = _service.CanUnlockTier3(player);

        // Assert
        canUnlock.Should().BeTrue(); // 22 PP ≥ 16 PP requirement
    }

    [Test]
    public void GetPPInvested_WithAllAbilities_ReturnsCorrectTotal()
    {
        // Arrange — All 9 abilities: 3 T1 (0) + 3 T2 (12) + 2 T3 (10) + 1 Capstone (6) = 28
        var player = CreateTier3BoneSetter(BoneSetterAbilityId.MiracleWorker);

        // Act
        var pp = _service.GetPPInvested(player);

        // Assert
        pp.Should().Be(28); // 0 + 12 + 10 + 6
    }
}
