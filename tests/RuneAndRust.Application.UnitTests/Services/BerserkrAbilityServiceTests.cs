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
/// Unit tests for <see cref="BerserkrAbilityService"/>.
/// </summary>
[TestFixture]
public class BerserkrAbilityServiceTests
{
    private Mock<IBerserkrRageService> _mockRageService = null!;
    private Mock<IBerserkrCorruptionService> _mockCorruptionService = null!;
    private TestBerserkrAbilityService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockRageService = new Mock<IBerserkrRageService>();
        _mockCorruptionService = new Mock<IBerserkrCorruptionService>();
        _service = new TestBerserkrAbilityService(
            _mockRageService.Object,
            _mockCorruptionService.Object,
            Mock.Of<ILogger<BerserkrAbilityService>>());
    }

    /// <summary>
    /// Test subclass that overrides dice methods for deterministic testing.
    /// </summary>
    private class TestBerserkrAbilityService : BerserkrAbilityService
    {
        public int FixedD20 { get; set; } = 15;
        public int FixedD6 { get; set; } = 4;
        public int Fixed3D6 { get; set; } = 12;
        public int FixedWeaponDamage { get; set; } = 6;

        public TestBerserkrAbilityService(
            IBerserkrRageService rageService,
            IBerserkrCorruptionService corruptionService,
            ILogger<BerserkrAbilityService> logger)
            : base(rageService, corruptionService, logger) { }

        internal override int RollD20() => FixedD20;
        internal override int RollD6() => FixedD6;
        internal override int Roll3D6() => Fixed3D6;
        internal override int RollWeaponDamage() => FixedWeaponDamage;
    }

    private static Player CreateBerserkr(params BerserkrAbilityId[] abilities)
    {
        var player = new Player("Test Berserkr");
        player.SetSpecialization("berserkr");
        player.InitializeRageResource();
        player.CurrentAP = 10;
        foreach (var ability in abilities)
        {
            player.UnlockBerserkrAbility(ability);
        }
        return player;
    }

    // ===== Fury Strike Tests =====

    [Test]
    public void UseFuryStrike_WithSufficientResources_ReturnsResult()
    {
        // Arrange
        var player = CreateBerserkr(BerserkrAbilityId.FuryStrike);
        var rage = RageResource.CreateAt(50, 100);

        _mockRageService.Setup(s => s.GetRage(player)).Returns(rage);
        _mockRageService.Setup(s => s.SpendRage(player, 20)).Returns(true);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.FuryStrike, 50, false))
            .Returns(BerserkrCorruptionRiskResult.CreateSafe("Below threshold"));

        // Act
        var result = _service.UseFuryStrike(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.BaseDamage.Should().Be(6); // FixedWeaponDamage
        result.FuryDamage.Should().Be(12); // Fixed3D6
        result.AttackRoll.Should().Be(15); // FixedD20
        result.WasCritical.Should().BeFalse();
        result.RageSpent.Should().Be(20);
        result.CorruptionTriggered.Should().BeFalse();
        player.CurrentAP.Should().Be(8); // 10 - 2
    }

    [Test]
    public void UseFuryStrike_InsufficientRage_ReturnsNull()
    {
        // Arrange
        var player = CreateBerserkr(BerserkrAbilityId.FuryStrike);
        var rage = RageResource.CreateAt(10, 100); // Need 20

        _mockRageService.Setup(s => s.GetRage(player)).Returns(rage);

        // Act
        var result = _service.UseFuryStrike(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged
    }

    [Test]
    public void UseFuryStrike_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateBerserkr(BerserkrAbilityId.FuryStrike);
        player.CurrentAP = 1; // Need 2
        var rage = RageResource.CreateAt(50, 100);

        _mockRageService.Setup(s => s.GetRage(player)).Returns(rage);

        // Act
        var result = _service.UseFuryStrike(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void UseFuryStrike_CriticalHit_AddsBonusDamage()
    {
        // Arrange
        var player = CreateBerserkr(BerserkrAbilityId.FuryStrike);
        _service.FixedD20 = 20; // Natural 20!
        var rage = RageResource.CreateAt(50, 100);

        _mockRageService.Setup(s => s.GetRage(player)).Returns(rage);
        _mockRageService.Setup(s => s.SpendRage(player, 20)).Returns(true);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.FuryStrike, 50, false))
            .Returns(BerserkrCorruptionRiskResult.CreateSafe("Below threshold"));

        // Act
        var result = _service.UseFuryStrike(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.WasCritical.Should().BeTrue();
        result.CriticalBonusDamage.Should().Be(4); // FixedD6
        result.TotalDamage.Should().Be(6 + 12 + 4); // weapon + fury + crit = 22
    }

    [Test]
    public void UseFuryStrike_WithCorruptionTriggered_SetsCorruptionFlag()
    {
        // Arrange
        var player = CreateBerserkr(BerserkrAbilityId.FuryStrike);
        var rage = RageResource.CreateAt(85, 100); // Enraged

        _mockRageService.Setup(s => s.GetRage(player)).Returns(rage);
        _mockRageService.Setup(s => s.SpendRage(player, 20)).Returns(true);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.FuryStrike, 85, false))
            .Returns(BerserkrCorruptionRiskResult.CreateTriggered(
                1, BerserkrCorruptionTrigger.FuryStrikeWhileEnraged, "Enraged"));

        // Act
        var result = _service.UseFuryStrike(player, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeTrue();
        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(player.Id, It.Is<BerserkrCorruptionRiskResult>(r => r.IsTriggered)),
            Times.Once);
    }

    [Test]
    public void UseFuryStrike_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange
        var player = CreateBerserkr(); // No abilities

        // Act
        var result = _service.UseFuryStrike(player, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    // ===== Blood Scent Tests =====

    [Test]
    public void CheckBloodScent_JustBloodied_ReturnsStateAndAddsRage()
    {
        // Arrange
        var player = CreateBerserkr(BerserkrAbilityId.BloodScent);
        var targetId = Guid.NewGuid();

        _mockRageService
            .Setup(s => s.AddRage(player, RageResource.BloodScentGain, "Blood Scent"))
            .Returns(10);

        // Act — target was at 60/100, now at 45/100 (just became bloodied)
        var result = _service.CheckBloodScent(player, targetId, "Draugr", 60, 45, 100);

        // Assert
        result.Should().NotBeNull();
        result!.IsBloodied.Should().BeTrue();
        _mockRageService.Verify(s => s.AddRage(player, 10, "Blood Scent"), Times.Once);
    }

    [Test]
    public void CheckBloodScent_AlreadyBloodied_ReturnsNull()
    {
        // Arrange
        var player = CreateBerserkr(BerserkrAbilityId.BloodScent);

        // Act — target was already at 40/100 (already bloodied), now at 30/100
        var result = _service.CheckBloodScent(player, Guid.NewGuid(), "Draugr", 40, 30, 100);

        // Assert
        result.Should().BeNull(); // No duplicate trigger
    }

    [Test]
    public void CheckBloodScent_NotBloodied_ReturnsNull()
    {
        // Arrange
        var player = CreateBerserkr(BerserkrAbilityId.BloodScent);

        // Act — target at 80/100 (not bloodied)
        var result = _service.CheckBloodScent(player, Guid.NewGuid(), "Draugr", 90, 80, 100);

        // Assert
        result.Should().BeNull();
    }

    // ===== Pain is Fuel Tests =====

    [Test]
    public void CheckPainIsFuel_WithDamage_AddsRage()
    {
        // Arrange
        var player = CreateBerserkr(BerserkrAbilityId.PainIsFuel);

        _mockRageService
            .Setup(s => s.AddRage(player, RageResource.PainIsFuelGain, "Pain is Fuel"))
            .Returns(5);

        // Act
        var gained = _service.CheckPainIsFuel(player, 15);

        // Assert
        gained.Should().Be(5);
        _mockRageService.Verify(s => s.AddRage(player, 5, "Pain is Fuel"), Times.Once);
    }

    [Test]
    public void CheckPainIsFuel_ZeroDamage_ReturnsZero()
    {
        // Arrange
        var player = CreateBerserkr(BerserkrAbilityId.PainIsFuel);

        // Act
        var gained = _service.CheckPainIsFuel(player, 0);

        // Assert
        gained.Should().Be(0);
    }

    // ===== Ability Readiness Tests =====

    [Test]
    public void GetAbilityReadiness_ReturnsCorrectStatus()
    {
        // Arrange
        var player = CreateBerserkr(
            BerserkrAbilityId.FuryStrike,
            BerserkrAbilityId.BloodScent,
            BerserkrAbilityId.PainIsFuel);
        var rage = RageResource.CreateAt(25, 100);
        player.CurrentAP = 3;

        _mockRageService.Setup(s => s.GetRage(player)).Returns(rage);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness[BerserkrAbilityId.FuryStrike].Should().BeTrue(); // 3 AP >= 2, 25 Rage >= 20
        readiness[BerserkrAbilityId.BloodScent].Should().BeTrue(); // Passive always ready
        readiness[BerserkrAbilityId.PainIsFuel].Should().BeTrue(); // Passive always ready
    }

    // ===== PP Validation Tests =====

    [Test]
    public void GetPPInvested_CalculatesCorrectly()
    {
        // Arrange
        var player = CreateBerserkr(
            BerserkrAbilityId.FuryStrike,          // T1: 0 PP
            BerserkrAbilityId.BloodScent,           // T1: 0 PP
            BerserkrAbilityId.RecklessAssault,      // T2: 4 PP
            BerserkrAbilityId.Unstoppable);         // T2: 4 PP
        // Total: 0 + 0 + 4 + 4 = 8 PP

        // Act
        var pp = _service.GetPPInvested(player);

        // Assert
        pp.Should().Be(8);
    }

    [Test]
    public void CanUnlockTier2_With8PPInvested_ReturnsTrue()
    {
        // Arrange — T1 abilities are free, need Tier 2 to reach 8 PP
        var player = CreateBerserkr(
            BerserkrAbilityId.FuryStrike,          // T1: 0 PP
            BerserkrAbilityId.RecklessAssault,     // T2: 4 PP
            BerserkrAbilityId.Unstoppable);        // T2: 4 PP = 8 PP

        // Act
        var result = _service.CanUnlockTier2(player);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void CanUnlockTier2_NonBerserkr_ReturnsFalse()
    {
        // Arrange
        var player = new Player("Warrior");
        player.SetSpecialization("skjaldmaer");

        // Act
        var result = _service.CanUnlockTier2(player);

        // Assert
        result.Should().BeFalse();
    }

    // ===== Reckless Assault Tests (v0.20.5b) =====

    private Player CreateTier2Berserkr(params BerserkrAbilityId[] extraAbilities)
    {
        // Create a Berserkr with all Tier 1 + specified Tier 2 abilities
        var player = CreateBerserkr(
            BerserkrAbilityId.FuryStrike,
            BerserkrAbilityId.BloodScent,
            BerserkrAbilityId.PainIsFuel);

        // Unlock Tier 2 abilities (each costs 4 PP; need 3 to reach 12 PP >= 8 PP threshold)
        player.UnlockBerserkrAbility(BerserkrAbilityId.RecklessAssault);
        player.UnlockBerserkrAbility(BerserkrAbilityId.Unstoppable);
        player.UnlockBerserkrAbility(BerserkrAbilityId.IntimidatingPresence);

        foreach (var ability in extraAbilities)
        {
            player.UnlockBerserkrAbility(ability);
        }

        return player;
    }

    [Test]
    public void ExecuteRecklessAssault_EntersStance_GrantsAttackBonus()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.Rage!.Gain(40, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.RecklessAssault, 40, false))
            .Returns(BerserkrCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        var result = _service.ExecuteRecklessAssault(player);

        // Assert
        result.Should().NotBeNull();
        result!.GetCurrentAttackBonus(40).Should().Be(6); // 4 + 40/20 = 6
        result.DefensePenalty.Should().Be(-2);
        player.IsInRecklessAssault.Should().BeTrue();
        player.CurrentAP.Should().Be(9); // 10 - 1 AP
    }

    [Test]
    public void ExecuteRecklessAssault_ExitsStance_WhenAlreadyActive()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.Rage!.Gain(40, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.RecklessAssault, It.IsAny<int>(), false))
            .Returns(BerserkrCorruptionRiskResult.CreateSafe("Safe"));

        // Enter stance first
        _service.ExecuteRecklessAssault(player);
        player.IsInRecklessAssault.Should().BeTrue();

        // Act — toggle off
        var result = _service.ExecuteRecklessAssault(player);

        // Assert
        result.Should().BeNull(); // null = exited
        player.IsInRecklessAssault.Should().BeFalse();
    }

    [Test]
    public void ExecuteRecklessAssault_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.CurrentAP = 0;

        // Act
        var result = _service.ExecuteRecklessAssault(player);

        // Assert
        result.Should().BeNull();
        player.IsInRecklessAssault.Should().BeFalse();
    }

    [Test]
    public void ExecuteRecklessAssault_AtHighRage_TriggersCorruption()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.Rage!.Gain(85, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        var corruptionResult = BerserkrCorruptionRiskResult.CreateTriggered(
            1, BerserkrCorruptionTrigger.RecklessAssaultEnraged,
            "Maintaining Reckless Assault while Enraged");
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.RecklessAssault, 85, false))
            .Returns(corruptionResult);

        // Act
        var result = _service.ExecuteRecklessAssault(player);

        // Assert
        result.Should().NotBeNull();
        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(player.Id, corruptionResult), Times.Once);
    }

    // ===== Unstoppable Tests (v0.20.5b) =====

    [Test]
    public void ExecuteUnstoppable_GrantsMovementImmunity()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.Rage!.Gain(50, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 15)).Returns(true);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.Unstoppable, 50, false))
            .Returns(BerserkrCorruptionRiskResult.CreateSafe("Safe"));

        // Act
        var result = _service.ExecuteUnstoppable(player);

        // Assert
        result.Should().NotBeNull();
        result!.TurnsRemaining.Should().Be(2);
        result.IsActive().Should().BeTrue();
        result.MovementPenaltiesIgnored.Should().HaveCount(6);
        player.HasUnstoppableActive.Should().BeTrue();
        player.CurrentAP.Should().Be(9); // 10 - 1 AP
    }

    [Test]
    public void ExecuteUnstoppable_InsufficientRage_ReturnsNull()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.Rage!.Gain(10, "Test"); // Need 15
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        // Act
        var result = _service.ExecuteUnstoppable(player);

        // Assert
        result.Should().BeNull();
        player.HasUnstoppableActive.Should().BeFalse();
    }

    [Test]
    public void ExecuteUnstoppable_AlreadyActive_ReturnsNull()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.Rage!.Gain(50, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 15)).Returns(true);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.Unstoppable, It.IsAny<int>(), false))
            .Returns(BerserkrCorruptionRiskResult.CreateSafe("Safe"));

        // Activate first time
        _service.ExecuteUnstoppable(player);
        player.HasUnstoppableActive.Should().BeTrue();

        // Act — try to use again while active
        var result = _service.ExecuteUnstoppable(player);

        // Assert
        result.Should().BeNull();
    }

    // ===== Intimidating Presence Tests (v0.20.5b) =====

    [Test]
    public void ExecuteIntimidatingPresence_AffectsEnemies()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.Rage!.Gain(50, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 10)).Returns(true);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.IntimidatingPresence, It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(BerserkrCorruptionRiskResult.CreateSafe("Safe"));

        var targets = new List<(Guid targetId, string targetName, int willSaveRoll, bool isCoherent, bool isMindless, bool hasFearImmunity)>
        {
            (Guid.NewGuid(), "Draugr Warrior 1", 10, false, false, false), // Fail (DC 14)
            (Guid.NewGuid(), "Draugr Warrior 2", 18, false, false, false), // Save
        };

        // Act
        var results = _service.ExecuteIntimidatingPresence(player, targets);

        // Assert
        results.Should().HaveCount(2);
        results[0].DidSave.Should().BeFalse();
        results[0].TurnsRemaining.Should().Be(3);
        results[1].DidSave.Should().BeTrue();
        results[1].ImmuneUntil.Should().NotBeNull();
        player.CurrentAP.Should().Be(8); // 10 - 2 AP
    }

    [Test]
    public void ExecuteIntimidatingPresence_SkipsMindlessTargets()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.Rage!.Gain(50, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 10)).Returns(true);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(It.IsAny<BerserkrAbilityId>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(BerserkrCorruptionRiskResult.CreateSafe("Safe"));

        var targets = new List<(Guid targetId, string targetName, int willSaveRoll, bool isCoherent, bool isMindless, bool hasFearImmunity)>
        {
            (Guid.NewGuid(), "Skeleton Archer", 5, false, true, false),   // Mindless — immune
            (Guid.NewGuid(), "Draugr Warrior", 10, false, false, false),  // Normal — affected
        };

        // Act
        var results = _service.ExecuteIntimidatingPresence(player, targets);

        // Assert
        results.Should().HaveCount(1); // Only Draugr processed
        results[0].TargetName.Should().Be("Draugr Warrior");
    }

    [Test]
    public void ExecuteIntimidatingPresence_InsufficientRage_ReturnsEmpty()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.Rage!.Gain(5, "Test"); // Need 10
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        var targets = new List<(Guid targetId, string targetName, int willSaveRoll, bool isCoherent, bool isMindless, bool hasFearImmunity)>
        {
            (Guid.NewGuid(), "Draugr", 10, false, false, false),
        };

        // Act
        var results = _service.ExecuteIntimidatingPresence(player, targets);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void ExecuteIntimidatingPresence_CoherentTarget_TriggersCorruption()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.Rage!.Gain(50, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 10)).Returns(true);

        var corruptionResult = BerserkrCorruptionRiskResult.CreateTriggered(
            1, BerserkrCorruptionTrigger.IntimidatingCoherentTarget,
            "Coherent target");
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.IntimidatingPresence, 50, true))
            .Returns(corruptionResult);

        var targets = new List<(Guid targetId, string targetName, int willSaveRoll, bool isCoherent, bool isMindless, bool hasFearImmunity)>
        {
            (Guid.NewGuid(), "Lawkeeper", 10, true, false, false), // Coherent target
        };

        // Act
        var results = _service.ExecuteIntimidatingPresence(player, targets);

        // Assert
        results.Should().HaveCount(1);
        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(player.Id, corruptionResult), Times.Once);
    }

    // ===== Tier 2 Readiness Tests =====

    [Test]
    public void GetAbilityReadiness_IncludesTier2Abilities()
    {
        // Arrange
        var player = CreateTier2Berserkr();
        player.Rage!.Gain(50, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert — should include all 6 abilities
        readiness.Should().ContainKey(BerserkrAbilityId.RecklessAssault);
        readiness.Should().ContainKey(BerserkrAbilityId.Unstoppable);
        readiness.Should().ContainKey(BerserkrAbilityId.IntimidatingPresence);

        readiness[BerserkrAbilityId.RecklessAssault].Should().BeTrue(); // 1 AP, no rage cost
        readiness[BerserkrAbilityId.Unstoppable].Should().BeTrue();     // 1 AP, 15 rage (have 50)
        readiness[BerserkrAbilityId.IntimidatingPresence].Should().BeTrue(); // 2 AP, 10 rage (have 50)
    }

    [Test]
    public void ExecuteRecklessAssault_Tier2Locked_ReturnsNull()
    {
        // Arrange — only Tier 1 abilities unlocked (0 PP = not enough for Tier 2)
        var player = CreateBerserkr(BerserkrAbilityId.FuryStrike);
        // Force unlock the ability without proper PP to test the PP check
        player.UnlockBerserkrAbility(BerserkrAbilityId.RecklessAssault);
        // PP invested = 0 (Fury Strike) + 4 (Reckless Assault) = 4, need 8

        // Act
        var result = _service.ExecuteRecklessAssault(player);

        // Assert
        result.Should().BeNull();
    }

    // ===== Tier 3 & Capstone Tests (v0.20.5c) =====

    /// <summary>
    /// Creates a Berserkr with all Tier 1, Tier 2, and specified Tier 3 abilities unlocked.
    /// Unlocks FuryOfTheForlorn + DeathDefiance by default (22 PP total >= 16 PP for Tier 3).
    /// </summary>
    private Player CreateTier3Berserkr(params BerserkrAbilityId[] extraAbilities)
    {
        var player = CreateTier2Berserkr(
            BerserkrAbilityId.FuryOfTheForlorn,
            BerserkrAbilityId.DeathDefiance);

        // Initialize Death Defiance state
        player.DeathDefianceState = DeathDefianceState.Create(player.Id);

        foreach (var ability in extraAbilities)
        {
            player.UnlockBerserkrAbility(ability);
        }

        return player;
    }

    // ===== Fury of the Forlorn Tests =====

    [Test]
    public void ExecuteFuryOfTheForlorn_HitsAllTargets_ReturnsResult()
    {
        // Arrange
        var player = CreateTier3Berserkr();
        player.Rage!.Gain(85, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 30)).Returns(true);

        var corruptionResult = BerserkrCorruptionRiskResult.CreateTriggered(
            1, BerserkrCorruptionTrigger.FuryOfTheForlornUsage,
            "Fury of the Forlorn always corrupts");
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.FuryOfTheForlorn, 85, false))
            .Returns(corruptionResult);

        // FixedD20 = 15, FixedWeaponDamage = 6, Fixed3D6 = 12 → damage = 18
        var targets = new List<(Guid targetId, string targetName, int defense)>
        {
            (Guid.NewGuid(), "Draugr Warrior 1", 10),
            (Guid.NewGuid(), "Draugr Warrior 2", 12),
            (Guid.NewGuid(), "Draugr Warrior 3", 14),
        };

        // Act
        var result = _service.ExecuteFuryOfTheForlorn(player, targets);

        // Assert
        result.Should().NotBeNull();
        result!.AttackRoll.Should().Be(15);
        result.DamageDealt.Should().Be(18); // 6 weapon + 12 fury
        result.GetHitCount().Should().Be(3); // 15 >= 10, 12, 14
        result.GetMissCount().Should().Be(0);
        result.TotalDamageAcrossTargets.Should().Be(54); // 18 × 3
        result.CorruptionTriggered.Should().BeTrue();
        result.RageSpent.Should().Be(30);
        player.CurrentAP.Should().Be(7); // 10 - 3
    }

    [Test]
    public void ExecuteFuryOfTheForlorn_PartialHitsMisses_SplitsCorrectly()
    {
        // Arrange
        var player = CreateTier3Berserkr();
        player.Rage!.Gain(90, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 30)).Returns(true);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.FuryOfTheForlorn, 90, false))
            .Returns(BerserkrCorruptionRiskResult.CreateTriggered(
                1, BerserkrCorruptionTrigger.FuryOfTheForlornUsage, "Always"));

        // Override attack roll to 12 for this test
        _service.FixedD20 = 12;
        var targets = new List<(Guid targetId, string targetName, int defense)>
        {
            (Guid.NewGuid(), "Draugr 1", 10),  // hit (12 >= 10)
            (Guid.NewGuid(), "Draugr 2", 12),  // hit (12 >= 12)
            (Guid.NewGuid(), "Draugr 3", 15),  // miss (12 < 15)
        };

        // Act
        var result = _service.ExecuteFuryOfTheForlorn(player, targets);

        // Assert
        result.Should().NotBeNull();
        result!.GetHitCount().Should().Be(2);
        result.GetMissCount().Should().Be(1);
        result.WasEffective().Should().BeTrue();
    }

    [Test]
    public void ExecuteFuryOfTheForlorn_InsufficientRage_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3Berserkr();
        player.Rage!.Gain(20, "Test"); // Only 20 Rage, need 30
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        var targets = new List<(Guid targetId, string targetName, int defense)>
        {
            (Guid.NewGuid(), "Enemy", 10),
        };

        // Act
        var result = _service.ExecuteFuryOfTheForlorn(player, targets);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteFuryOfTheForlorn_RageBelow80_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3Berserkr();
        player.Rage!.Gain(50, "Test"); // 50 Rage (above 30 cost but below 80 requirement)
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        var targets = new List<(Guid targetId, string targetName, int defense)>
        {
            (Guid.NewGuid(), "Enemy", 10),
        };

        // Act
        var result = _service.ExecuteFuryOfTheForlorn(player, targets);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteFuryOfTheForlorn_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3Berserkr();
        player.Rage!.Gain(85, "Test");
        player.CurrentAP = 2; // Need 3 AP
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        var targets = new List<(Guid targetId, string targetName, int defense)>
        {
            (Guid.NewGuid(), "Enemy", 10),
        };

        // Act
        var result = _service.ExecuteFuryOfTheForlorn(player, targets);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteFuryOfTheForlorn_NoTargets_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3Berserkr();
        player.Rage!.Gain(85, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        var targets = new List<(Guid targetId, string targetName, int defense)>();

        // Act
        var result = _service.ExecuteFuryOfTheForlorn(player, targets);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteFuryOfTheForlorn_AlwaysTriggersCorruption()
    {
        // Arrange
        var player = CreateTier3Berserkr();
        player.Rage!.Gain(85, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 30)).Returns(true);

        var corruptionResult = BerserkrCorruptionRiskResult.CreateTriggered(
            1, BerserkrCorruptionTrigger.FuryOfTheForlornUsage,
            "Always triggers");
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.FuryOfTheForlorn, 85, false))
            .Returns(corruptionResult);

        var targets = new List<(Guid targetId, string targetName, int defense)>
        {
            (Guid.NewGuid(), "Enemy", 10),
        };

        // Act
        var result = _service.ExecuteFuryOfTheForlorn(player, targets);

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(player.Id, corruptionResult), Times.Once);
    }

    [Test]
    public void ExecuteFuryOfTheForlorn_Tier3Locked_ReturnsNull()
    {
        // Arrange — player with FuryOfTheForlorn unlocked but insufficient PP for Tier 3
        var player = CreateBerserkr(BerserkrAbilityId.FuryStrike);
        player.UnlockBerserkrAbility(BerserkrAbilityId.FuryOfTheForlorn);
        // PP: 0 (FuryStrike) + 5 (FuryOfTheForlorn) = 5 PP, need 16

        var targets = new List<(Guid targetId, string targetName, int defense)>
        {
            (Guid.NewGuid(), "Enemy", 10),
        };

        // Act
        var result = _service.ExecuteFuryOfTheForlorn(player, targets);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteFuryOfTheForlorn_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange — Tier 3 PP met but ability not unlocked
        var player = CreateTier2Berserkr();
        // PP: 12, but FuryOfTheForlorn not unlocked

        player.Rage!.Gain(85, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        var targets = new List<(Guid targetId, string targetName, int defense)>
        {
            (Guid.NewGuid(), "Enemy", 10),
        };

        // Act
        var result = _service.ExecuteFuryOfTheForlorn(player, targets);

        // Assert
        result.Should().BeNull();
    }

    // ===== Death Defiance Tests =====

    [Test]
    public void CheckDeathDefiance_PreventsLethalDamage()
    {
        // Arrange
        var player = CreateTier3Berserkr();
        player.Rage!.Gain(40, "Test");
        _mockRageService.Setup(s => s.AddRage(player, 20, "Death Defiance")).Returns(20);

        // Simulate lethal damage: player starts at 100 HP (MaxHealth), Defense = 5
        // TakeDamage(105) → actualDamage = 105 - 5 = 100 → Health = 0
        player.TakeDamage(105);
        player.Health.Should().Be(0);

        // Act
        var result = _service.CheckDeathDefiance(player, 105);

        // Assert
        result.Should().BeTrue();
        player.Health.Should().Be(1); // Healed to 1 HP
        player.DeathDefianceState!.HasTriggeredThisCombat.Should().BeTrue();
        _mockRageService.Verify(s => s.AddRage(player, 20, "Death Defiance"), Times.Once);
    }

    [Test]
    public void CheckDeathDefiance_NonLethalDamage_ReturnsFalse()
    {
        // Arrange
        var player = CreateTier3Berserkr();

        // Take some damage but survive
        player.TakeDamage(10); // 10 - 5 defense = 5 actual → HP = 95
        player.Health.Should().BeGreaterThan(0);

        // Act
        var result = _service.CheckDeathDefiance(player, 10);

        // Assert
        result.Should().BeFalse();
        player.DeathDefianceState!.HasTriggeredThisCombat.Should().BeFalse();
    }

    [Test]
    public void CheckDeathDefiance_GrantsTwentyRage()
    {
        // Arrange
        var player = CreateTier3Berserkr();
        _mockRageService.Setup(s => s.AddRage(player, 20, "Death Defiance")).Returns(20);

        player.TakeDamage(105); // HP → 0
        player.Health.Should().Be(0);

        // Act
        _service.CheckDeathDefiance(player, 105);

        // Assert
        _mockRageService.Verify(
            s => s.AddRage(player, 20, "Death Defiance"), Times.Once);
    }

    [Test]
    public void CheckDeathDefiance_CannotTriggerTwice()
    {
        // Arrange
        var player = CreateTier3Berserkr();
        _mockRageService.Setup(s => s.AddRage(player, 20, "Death Defiance")).Returns(20);

        // First lethal hit — triggers Death Defiance
        player.TakeDamage(105);
        _service.CheckDeathDefiance(player, 105);
        player.Health.Should().Be(1);

        // Second lethal hit — should NOT trigger
        player.TakeDamage(6); // 6 - 5 defense = 1 actual → HP = 0
        player.Health.Should().Be(0);

        // Act
        var result = _service.CheckDeathDefiance(player, 6);

        // Assert
        result.Should().BeFalse(); // Already used this combat
    }

    [Test]
    public void CheckDeathDefiance_AbilityNotUnlocked_ReturnsFalse()
    {
        // Arrange — create player without Death Defiance
        var player = CreateTier2Berserkr();
        player.TakeDamage(105);
        player.Health.Should().Be(0);

        // Act
        var result = _service.CheckDeathDefiance(player, 105);

        // Assert
        result.Should().BeFalse();
    }

    // ===== Avatar of Destruction Tests =====

    [Test]
    public void ExecuteAvatarOfDestruction_At100Rage_ReturnsAvatarState()
    {
        // Arrange
        var player = CreateTier3Berserkr(BerserkrAbilityId.AvatarOfDestruction);
        player.Rage!.Gain(100, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 100)).Returns(true);

        var corruptionResult = BerserkrCorruptionRiskResult.CreateTriggered(
            2, BerserkrCorruptionTrigger.CapstoneActivation,
            "The price of ultimate power");
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.AvatarOfDestruction, 100, false))
            .Returns(corruptionResult);

        // Act
        var result = _service.ExecuteAvatarOfDestruction(player);

        // Assert
        result.Should().NotBeNull();
        result!.TurnsRemaining.Should().Be(2);
        result.GetDamageMultiplier().Should().Be(2.0f);
        result.IsActive().Should().BeTrue();
        result.ImmuneToCrowdControl.Should().BeTrue();
        result.MovementBonus.Should().Be(2);
        result.CorruptionGenerated.Should().Be(2);
        result.WillCauseExhaustion.Should().BeTrue();
        player.CurrentAP.Should().Be(7); // 10 - 3
        player.HasUsedCapstoneThisCombat.Should().BeTrue();
        player.IsInAvatarState.Should().BeTrue();
    }

    [Test]
    public void ExecuteAvatarOfDestruction_RageNot100_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3Berserkr(BerserkrAbilityId.AvatarOfDestruction);
        player.Rage!.Gain(99, "Test"); // Not exactly 100
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        // Act
        var result = _service.ExecuteAvatarOfDestruction(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteAvatarOfDestruction_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3Berserkr(BerserkrAbilityId.AvatarOfDestruction);
        player.Rage!.Gain(100, "Test");
        player.CurrentAP = 2; // Need 3
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        // Act
        var result = _service.ExecuteAvatarOfDestruction(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteAvatarOfDestruction_AlreadyUsedThisCombat_ReturnsNull()
    {
        // Arrange
        var player = CreateTier3Berserkr(BerserkrAbilityId.AvatarOfDestruction);
        player.Rage!.Gain(100, "Test");
        player.HasUsedCapstoneThisCombat = true; // Already used
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        // Act
        var result = _service.ExecuteAvatarOfDestruction(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteAvatarOfDestruction_DoublesDamageMultiplier()
    {
        // Arrange
        var player = CreateTier3Berserkr(BerserkrAbilityId.AvatarOfDestruction);
        player.Rage!.Gain(100, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 100)).Returns(true);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.AvatarOfDestruction, 100, false))
            .Returns(BerserkrCorruptionRiskResult.CreateTriggered(
                2, BerserkrCorruptionTrigger.CapstoneActivation, "Always"));

        // Act
        var result = _service.ExecuteAvatarOfDestruction(player);

        // Assert
        result.Should().NotBeNull();
        result!.GetDamageMultiplier().Should().Be(2.0f);
    }

    [TestCase(CrowdControlType.Stun)]
    [TestCase(CrowdControlType.Root)]
    [TestCase(CrowdControlType.Fear)]
    [TestCase(CrowdControlType.Charm)]
    [TestCase(CrowdControlType.Paralyze)]
    [TestCase(CrowdControlType.Slow)]
    [TestCase(CrowdControlType.Prone)]
    [TestCase(CrowdControlType.ForcedMovement)]
    [TestCase(CrowdControlType.Sleep)]
    [TestCase(CrowdControlType.Confusion)]
    public void ExecuteAvatarOfDestruction_CCImmunity_AllTenTypes(CrowdControlType ccType)
    {
        // Arrange
        var player = CreateTier3Berserkr(BerserkrAbilityId.AvatarOfDestruction);
        player.Rage!.Gain(100, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 100)).Returns(true);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.AvatarOfDestruction, 100, false))
            .Returns(BerserkrCorruptionRiskResult.CreateTriggered(
                2, BerserkrCorruptionTrigger.CapstoneActivation, "Always"));

        // Act
        var result = _service.ExecuteAvatarOfDestruction(player);

        // Assert
        result.Should().NotBeNull();
        result!.IsImmuneToCC(ccType).Should().BeTrue();
    }

    [Test]
    public void ExecuteAvatarOfDestruction_AlwaysTriggersTwoCorruption()
    {
        // Arrange
        var player = CreateTier3Berserkr(BerserkrAbilityId.AvatarOfDestruction);
        player.Rage!.Gain(100, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 100)).Returns(true);

        var corruptionResult = BerserkrCorruptionRiskResult.CreateTriggered(
            2, BerserkrCorruptionTrigger.CapstoneActivation,
            "The price of ultimate power");
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.AvatarOfDestruction, 100, false))
            .Returns(corruptionResult);

        // Act
        var result = _service.ExecuteAvatarOfDestruction(player);

        // Assert
        result.Should().NotBeNull();
        result!.CorruptionGenerated.Should().Be(2);
        _mockCorruptionService.Verify(
            s => s.ApplyCorruption(player.Id, corruptionResult), Times.Once);
    }

    [Test]
    public void ExecuteAvatarOfDestruction_CapstoneLocked_ReturnsNull()
    {
        // Arrange — player with AvatarOfDestruction unlocked but insufficient PP for Capstone
        var player = CreateBerserkr(
            BerserkrAbilityId.FuryStrike,
            BerserkrAbilityId.BloodScent,
            BerserkrAbilityId.PainIsFuel);
        player.UnlockBerserkrAbility(BerserkrAbilityId.RecklessAssault); // +4 = 4 PP
        player.UnlockBerserkrAbility(BerserkrAbilityId.AvatarOfDestruction); // +6 = 10 PP < 24

        player.Rage!.Gain(100, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        // Act
        var result = _service.ExecuteAvatarOfDestruction(player);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteAvatarOfDestruction_SpendsAll100Rage()
    {
        // Arrange
        var player = CreateTier3Berserkr(BerserkrAbilityId.AvatarOfDestruction);
        player.Rage!.Gain(100, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);
        _mockRageService.Setup(s => s.SpendRage(player, 100)).Returns(true);
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(BerserkrAbilityId.AvatarOfDestruction, 100, false))
            .Returns(BerserkrCorruptionRiskResult.CreateTriggered(
                2, BerserkrCorruptionTrigger.CapstoneActivation, "Always"));

        // Act
        _service.ExecuteAvatarOfDestruction(player);

        // Assert
        _mockRageService.Verify(s => s.SpendRage(player, 100), Times.Once);
    }

    // ===== Tier 3 & Capstone Readiness Tests =====

    [Test]
    public void GetAbilityReadiness_IncludesTier3Abilities()
    {
        // Arrange
        var player = CreateTier3Berserkr();
        player.Rage!.Gain(85, "Test");
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().ContainKey(BerserkrAbilityId.FuryOfTheForlorn);
        readiness.Should().ContainKey(BerserkrAbilityId.DeathDefiance);

        readiness[BerserkrAbilityId.FuryOfTheForlorn].Should().BeTrue(); // 3 AP, 80+ Rage
        readiness[BerserkrAbilityId.DeathDefiance].Should().BeTrue(); // Passive, not triggered
    }

    [Test]
    public void GetAbilityReadiness_AvatarRequiresExactly100Rage()
    {
        // Arrange
        var player = CreateTier3Berserkr(BerserkrAbilityId.AvatarOfDestruction);
        player.Rage!.Gain(99, "Test"); // 99, not 100
        _mockRageService.Setup(s => s.GetRage(player)).Returns(player.Rage);

        // Act
        var readiness = _service.GetAbilityReadiness(player);

        // Assert
        readiness.Should().ContainKey(BerserkrAbilityId.AvatarOfDestruction);
        readiness[BerserkrAbilityId.AvatarOfDestruction].Should().BeFalse(); // Need exactly 100
    }
}
