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
/// Unit tests for <see cref="EchoCallerAbilityService"/>.
/// Tests all 6 active abilities, passive utility methods, and the full guard chain pattern.
/// No Corruption mechanics (Coherent path).
/// </summary>
[TestFixture]
public class EchoCallerAbilityServiceTests
{
    // ===== Test Subclass for Deterministic Dice Rolls =====

    /// <summary>
    /// Test subclass that overrides virtual dice roll methods for deterministic results.
    /// </summary>
    private class TestEchoCallerAbilityService : EchoCallerAbilityService
    {
        /// <summary>Fixed d6 roll result for damage calculations.</summary>
        public int FixedD6 { get; set; } = 3;

        /// <summary>Fixed d8 roll result for fear bonuses.</summary>
        public int FixedD8 { get; set; } = 4;

        /// <summary>Fixed d10 roll result for capstone damage.</summary>
        public int FixedD10 { get; set; } = 5;

        public TestEchoCallerAbilityService(ILogger<EchoCallerAbilityService> logger)
            : base(logger) { }

        internal override int RollD6() => FixedD6;
        internal override int RollD8() => FixedD8;
        internal override int RollD10() => FixedD10;
    }

    private TestEchoCallerAbilityService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new TestEchoCallerAbilityService(
            Mock.Of<ILogger<EchoCallerAbilityService>>());
    }

    // ===== Helper: Create Echo-Caller Player =====

    /// <summary>
    /// Creates an Echo-Caller player with specified unlocked abilities for testing.
    /// </summary>
    /// <param name="abilities">The abilities to unlock.</param>
    /// <returns>A configured Echo-Caller player with 20 AP and 100 Aether.</returns>
    private static Player CreateEchoCaller(params EchoCallerAbilityId[] abilities)
    {
        var player = new Player("Test Echo-Caller");
        player.SetSpecialization("echo-caller");
        player.CurrentAP = 20;
        // NOTE: Aether is tracked via AetherResonance value object, not a simple int property.
        // Aether restoration amounts are returned in result records for the combat system to apply.
        foreach (var ability in abilities)
        {
            player.UnlockEchoCallerAbility(ability);
        }
        return player;
    }

    /// <summary>
    /// Creates an Echo-Caller player with ALL abilities unlocked (all tiers).
    /// </summary>
    private static Player CreateFullEchoCaller()
    {
        return CreateEchoCaller(
            EchoCallerAbilityId.EchoAttunement,
            EchoCallerAbilityId.ScreamOfSilence,
            EchoCallerAbilityId.PhantomMenace,
            EchoCallerAbilityId.EchoCascade,
            EchoCallerAbilityId.RealityFracture,
            EchoCallerAbilityId.TerrorFeedback,
            EchoCallerAbilityId.FearCascade,
            EchoCallerAbilityId.EchoDisplacement,
            EchoCallerAbilityId.SilenceMadeWeapon);
    }

    // ===== Constructor Tests =====

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new EchoCallerAbilityService(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ===== Scream of Silence Tests =====

    [Test]
    public void ExecuteScreamOfSilence_NullPlayer_ThrowsArgumentNullException()
    {
        var act = () => _service.ExecuteScreamOfSilence(null!, Guid.NewGuid(), false);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ExecuteScreamOfSilence_WrongSpecialization_ReturnsNull()
    {
        var player = new Player("Not Echo-Caller");
        player.SetSpecialization("rust-witch");
        player.CurrentAP = 10;

        var result = _service.ExecuteScreamOfSilence(player, Guid.NewGuid(), false);
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteScreamOfSilence_AbilityNotUnlocked_ReturnsNull()
    {
        var player = CreateEchoCaller(); // No abilities unlocked
        var result = _service.ExecuteScreamOfSilence(player, Guid.NewGuid(), false);
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteScreamOfSilence_InsufficientAP_ReturnsNull()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.ScreamOfSilence);
        player.CurrentAP = 1; // Needs 2 AP

        var result = _service.ExecuteScreamOfSilence(player, Guid.NewGuid(), false);
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteScreamOfSilence_TargetNotFeared_BaseDamageOnly()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.ScreamOfSilence);
        _service.FixedD6 = 3;

        var result = _service.ExecuteScreamOfSilence(player, Guid.NewGuid(), targetIsFeared: false, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.DamageDealt.Should().Be(6); // 2d6 with FixedD6=3 → 3+3=6
        result.FearBonusDamage.Should().Be(0);
        result.TargetWasFeared.Should().BeFalse();
        result.AetherSpent.Should().Be(2);
        result.AbilityRank.Should().Be(1);
    }

    [Test]
    public void ExecuteScreamOfSilence_TargetFeared_Rank1_Bonus1d8()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.ScreamOfSilence);
        _service.FixedD6 = 3;
        _service.FixedD8 = 4;

        var result = _service.ExecuteScreamOfSilence(player, Guid.NewGuid(), targetIsFeared: true, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.DamageDealt.Should().Be(6); // Base damage (2d6)
        result.FearBonusDamage.Should().Be(4); // +1d8 at R1
        result.TargetWasFeared.Should().BeTrue();
    }

    [Test]
    public void ExecuteScreamOfSilence_TargetFeared_Rank2_Bonus2d8()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.ScreamOfSilence);
        _service.FixedD6 = 3;
        _service.FixedD8 = 4;

        var result = _service.ExecuteScreamOfSilence(player, Guid.NewGuid(), targetIsFeared: true, rank: 2);

        result.Should().NotBeNull();
        result!.DamageDealt.Should().Be(6);
        result.FearBonusDamage.Should().Be(8); // +2d8 at R2+ (4+4)
    }

    [Test]
    public void ExecuteScreamOfSilence_DeductsAP()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.ScreamOfSilence);
        var apBefore = player.CurrentAP;

        _service.ExecuteScreamOfSilence(player, Guid.NewGuid(), false);

        player.CurrentAP.Should().Be(apBefore - 2);
    }

    // ===== Phantom Menace Tests =====

    [Test]
    public void ExecutePhantomMenace_WrongSpecialization_ReturnsNull()
    {
        var player = new Player("Not Echo-Caller");
        player.SetSpecialization("blot-priest");
        player.CurrentAP = 10;

        var result = _service.ExecutePhantomMenace(player, Guid.NewGuid());
        result.Should().BeNull();
    }

    [Test]
    public void ExecutePhantomMenace_AbilityNotUnlocked_ReturnsNull()
    {
        var player = CreateEchoCaller();
        var result = _service.ExecutePhantomMenace(player, Guid.NewGuid());
        result.Should().BeNull();
    }

    [Test]
    public void ExecutePhantomMenace_InsufficientAP_ReturnsNull()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.PhantomMenace);
        player.CurrentAP = 1; // Needs 2 AP

        var result = _service.ExecutePhantomMenace(player, Guid.NewGuid());
        result.Should().BeNull();
    }

    [Test]
    public void ExecutePhantomMenace_Rank1_Fear3Turns()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.PhantomMenace);

        var result = _service.ExecutePhantomMenace(player, Guid.NewGuid(), rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.FearApplied.Should().BeTrue();
        result.FearDuration.Should().Be(3); // R1 = 3 turns
        result.AetherRestored.Should().Be(0); // No TerrorFeedback yet
        result.AetherSpent.Should().Be(2);
    }

    [Test]
    public void ExecutePhantomMenace_Rank2_Fear4Turns()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.PhantomMenace);

        var result = _service.ExecutePhantomMenace(player, Guid.NewGuid(), rank: 2);

        result.Should().NotBeNull();
        result!.FearDuration.Should().Be(4); // R2+ = 4 turns
    }

    [Test]
    public void ExecutePhantomMenace_WithTerrorFeedback_RestoresAether()
    {
        var player = CreateEchoCaller(
            EchoCallerAbilityId.PhantomMenace,
            EchoCallerAbilityId.TerrorFeedback);

        var result = _service.ExecutePhantomMenace(player, Guid.NewGuid());

        result.Should().NotBeNull();
        result!.AetherRestored.Should().Be(15); // TerrorFeedback = +15
        // NOTE: Aether restoration is returned in the result for the combat system to apply.
    }

    [Test]
    public void ExecutePhantomMenace_DeductsAP()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.PhantomMenace);
        var apBefore = player.CurrentAP;

        _service.ExecutePhantomMenace(player, Guid.NewGuid());

        player.CurrentAP.Should().Be(apBefore - 2);
    }

    // ===== Reality Fracture Tests =====

    [Test]
    public void ExecuteRealityFracture_WrongSpecialization_ReturnsNull()
    {
        var player = new Player("Not Echo-Caller");
        player.SetSpecialization("seidkona");
        player.CurrentAP = 10;

        var result = _service.ExecuteRealityFracture(player, Guid.NewGuid(), false);
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteRealityFracture_InsufficientTier_ReturnsNull()
    {
        // Only Tier 1 abilities — no Tier 2 abilities unlocked
        var player = CreateEchoCaller(EchoCallerAbilityId.ScreamOfSilence);

        var result = _service.ExecuteRealityFracture(player, Guid.NewGuid(), false);
        result.Should().BeNull(); // Requires 8+ PP invested
    }

    [Test]
    public void ExecuteRealityFracture_Rank1_3d6Damage()
    {
        var player = CreateFullEchoCaller();
        _service.FixedD6 = 2;

        var result = _service.ExecuteRealityFracture(player, Guid.NewGuid(), targetIsFeared: false, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.DamageDealt.Should().Be(6); // 3d6 with FixedD6=2 → 2+2+2=6
        result.DisorientedApplied.Should().BeTrue();
        result.PushDistance.Should().Be(1);
        result.AetherSpent.Should().Be(3);
    }

    [Test]
    public void ExecuteRealityFracture_AlwaysAppliesCrowdControl()
    {
        var player = CreateFullEchoCaller();

        var result = _service.ExecuteRealityFracture(player, Guid.NewGuid(), targetIsFeared: false);

        result.Should().NotBeNull();
        result!.DisorientedApplied.Should().BeTrue();
        result.PushDistance.Should().Be(1);
    }

    [Test]
    public void ExecuteRealityFracture_DeductsAP()
    {
        var player = CreateFullEchoCaller();
        var apBefore = player.CurrentAP;

        _service.ExecuteRealityFracture(player, Guid.NewGuid(), false);

        player.CurrentAP.Should().Be(apBefore - 3);
    }

    // ===== Fear Cascade Tests =====

    [Test]
    public void ExecuteFearCascade_WrongSpecialization_ReturnsNull()
    {
        var player = new Player("Not Echo-Caller");
        player.SetSpecialization("blot-priest");
        player.CurrentAP = 10;

        var targets = new List<Guid> { Guid.NewGuid() };
        var result = _service.ExecuteFearCascade(player, targets);
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteFearCascade_InsufficientTier_ReturnsNull()
    {
        // Only Tier 1 abilities
        var player = CreateEchoCaller(EchoCallerAbilityId.ScreamOfSilence);
        var targets = new List<Guid> { Guid.NewGuid() };

        var result = _service.ExecuteFearCascade(player, targets);
        result.Should().BeNull(); // Requires 16+ PP invested
    }

    [Test]
    public void ExecuteFearCascade_MultipleTargets_ApplyFearsToAll()
    {
        var player = CreateFullEchoCaller();
        var targets = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var result = _service.ExecuteFearCascade(player, targets, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.TargetsAffected.Should().Be(3);
        result.FearsApplied.Should().Be(3);
        result.FearDuration.Should().Be(3); // R1 = 3 turns
        result.AffectedTargetNames.Should().HaveCount(3);
        result.AetherSpent.Should().Be(4);
    }

    [Test]
    public void ExecuteFearCascade_Rank2_Fear4Turns()
    {
        var player = CreateFullEchoCaller();
        var targets = new List<Guid> { Guid.NewGuid() };

        var result = _service.ExecuteFearCascade(player, targets, rank: 2);

        result.Should().NotBeNull();
        result!.FearDuration.Should().Be(4); // R2+ = 4 turns
    }

    [Test]
    public void ExecuteFearCascade_WithTerrorFeedback_RestoresAetherPerFear()
    {
        var player = CreateFullEchoCaller();
        var targets = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var result = _service.ExecuteFearCascade(player, targets);

        result.Should().NotBeNull();
        result!.FearsApplied.Should().Be(3);
        result.TotalAetherRestored.Should().Be(45); // +15 per fear × 3 = 45
        // NOTE: Aether restoration is returned in the result for the combat system to apply.
    }

    [Test]
    public void ExecuteFearCascade_NullTargets_ThrowsArgumentNullException()
    {
        var player = CreateFullEchoCaller();
        var act = () => _service.ExecuteFearCascade(player, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ===== Echo Displacement Tests =====

    [Test]
    public void ExecuteEchoDisplacement_WrongSpecialization_ReturnsNull()
    {
        var player = new Player("Not Echo-Caller");
        player.SetSpecialization("seidkona");
        player.CurrentAP = 10;

        var result = _service.ExecuteEchoDisplacement(player, Guid.NewGuid());
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteEchoDisplacement_InsufficientTier_ReturnsNull()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.ScreamOfSilence);

        var result = _service.ExecuteEchoDisplacement(player, Guid.NewGuid());
        result.Should().BeNull(); // Requires 16+ PP
    }

    [Test]
    public void ExecuteEchoDisplacement_Rank1_Displaces()
    {
        var player = CreateFullEchoCaller();

        var result = _service.ExecuteEchoDisplacement(player, Guid.NewGuid(), rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.TargetDisplaced.Should().BeTrue();
        result.DisorientedApplied.Should().BeTrue();
        result.AetherSpent.Should().Be(4);
    }

    [Test]
    public void ExecuteEchoDisplacement_DeductsAP()
    {
        var player = CreateFullEchoCaller();
        var apBefore = player.CurrentAP;

        _service.ExecuteEchoDisplacement(player, Guid.NewGuid());

        player.CurrentAP.Should().Be(apBefore - 4);
    }

    // ===== Silence Made Weapon (Capstone) Tests =====

    [Test]
    public void ExecuteSilenceMadeWeapon_WrongSpecialization_ReturnsNull()
    {
        var player = new Player("Not Echo-Caller");
        player.SetSpecialization("rust-witch");
        player.CurrentAP = 10;

        var targets = new List<Guid> { Guid.NewGuid() };
        var result = _service.ExecuteSilenceMadeWeapon(player, targets, 0);
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteSilenceMadeWeapon_InsufficientTier_ReturnsNull()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.ScreamOfSilence);
        var targets = new List<Guid> { Guid.NewGuid() };

        var result = _service.ExecuteSilenceMadeWeapon(player, targets, 0);
        result.Should().BeNull(); // Requires 24+ PP
    }

    [Test]
    public void ExecuteSilenceMadeWeapon_AlreadyUsedThisCombat_ReturnsNull()
    {
        var player = CreateFullEchoCaller();
        var targets = new List<Guid> { Guid.NewGuid() };

        // First cast succeeds
        var result1 = _service.ExecuteSilenceMadeWeapon(player, targets, 0);
        result1.Should().NotBeNull();

        // Second cast should fail (already used this combat)
        var result2 = _service.ExecuteSilenceMadeWeapon(player, targets, 0);
        result2.Should().BeNull();
    }

    [Test]
    public void ExecuteSilenceMadeWeapon_NullTargets_ThrowsArgumentNullException()
    {
        var player = CreateFullEchoCaller();
        var act = () => _service.ExecuteSilenceMadeWeapon(player, null!, 0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ExecuteSilenceMadeWeapon_BaseDamageOnly_NoFearedTargets()
    {
        var player = CreateFullEchoCaller();
        var targets = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _service.FixedD10 = 5;

        var result = _service.ExecuteSilenceMadeWeapon(player, targets, fearedTargetCount: 0, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.TotalDamage.Should().Be(20); // 4d10 with FixedD10=5 → 5+5+5+5=20
        result.TargetsHit.Should().Be(2);
        result.FearedTargetCount.Should().Be(0);
        result.FearScalingBonus.Should().Be(0);
        result.AetherSpent.Should().Be(5);
    }

    [Test]
    public void ExecuteSilenceMadeWeapon_ScalesWithFearedTargets()
    {
        var player = CreateFullEchoCaller();
        var targets = new List<Guid> { Guid.NewGuid() };
        _service.FixedD10 = 5;

        var result = _service.ExecuteSilenceMadeWeapon(player, targets, fearedTargetCount: 2, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        // Base: 4d10 = 20. Scaling: 2 × 2d10 = 20. Total = 40
        result.TotalDamage.Should().Be(40);
        result.FearedTargetCount.Should().Be(2);
        result.FearScalingBonus.Should().Be(20); // 2d10 × 2 feared = 20
    }

    [Test]
    public void ExecuteSilenceMadeWeapon_WithTerrorFeedback_RestoresAether()
    {
        var player = CreateFullEchoCaller();
        var targets = new List<Guid> { Guid.NewGuid() };

        var result = _service.ExecuteSilenceMadeWeapon(player, targets, fearedTargetCount: 1);

        result.Should().NotBeNull();
        result!.AetherRestored.Should().Be(15); // TerrorFeedback = +15
        // NOTE: Aether restoration is returned in the result for the combat system to apply.
    }

    [Test]
    public void ExecuteSilenceMadeWeapon_DeductsAP()
    {
        var player = CreateFullEchoCaller();
        var targets = new List<Guid> { Guid.NewGuid() };
        var apBefore = player.CurrentAP;

        _service.ExecuteSilenceMadeWeapon(player, targets, 0);

        player.CurrentAP.Should().Be(apBefore - 5);
    }

    [Test]
    public void ExecuteSilenceMadeWeapon_SetsOncePerCombatFlag()
    {
        var player = CreateFullEchoCaller();
        var targets = new List<Guid> { Guid.NewGuid() };

        player.HasUsedSilenceMadeWeaponThisCombat.Should().BeFalse();
        _service.ExecuteSilenceMadeWeapon(player, targets, 0);
        player.HasUsedSilenceMadeWeaponThisCombat.Should().BeTrue();
    }

    // ===== Echo Chain Processing Tests =====

    [Test]
    public void ProcessEchoChain_NoEchoCascade_Base50Percent()
    {
        var adjacentTargets = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var result = _service.ProcessEchoChain(
            baseDamage: 20,
            adjacentTargetIds: adjacentTargets,
            hasEchoCascade: false,
            echoCascadeRank: 1);

        result.ChainDamage.Should().Be(10); // 50% of 20
        result.ChainRange.Should().Be(1);
        result.ChainDamagePercent.Should().Be(50);
        result.ChainTargets.Should().Be(1); // Only 1 target despite 2 available (base echo = 1 max)
    }

    [Test]
    public void ProcessEchoChain_EchoCascadeRank2_70PercentRange2()
    {
        var adjacentTargets = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var result = _service.ProcessEchoChain(
            baseDamage: 20,
            adjacentTargetIds: adjacentTargets,
            hasEchoCascade: true,
            echoCascadeRank: 2);

        result.ChainDamage.Should().Be(14); // 70% of 20
        result.ChainRange.Should().Be(2);
        result.ChainDamagePercent.Should().Be(70);
        result.ChainTargets.Should().Be(2); // Both targets hit at Rank 2 (max 2)
    }

    [Test]
    public void ProcessEchoChain_EchoCascadeRank3_80PercentRange3()
    {
        var adjacentTargets = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var result = _service.ProcessEchoChain(
            baseDamage: 20,
            adjacentTargetIds: adjacentTargets,
            hasEchoCascade: true,
            echoCascadeRank: 3);

        result.ChainDamage.Should().Be(16); // 80% of 20
        result.ChainRange.Should().Be(3);
        result.ChainDamagePercent.Should().Be(80);
        result.ChainTargets.Should().Be(2); // Max 2 targets even with 3 available
    }

    [Test]
    public void ProcessEchoChain_NullAdjacentTargets_ThrowsArgumentNullException()
    {
        var act = () => _service.ProcessEchoChain(20, null!, false);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ProcessEchoChain_ZeroDamage_ReturnsZeroChain()
    {
        var adjacentTargets = new List<Guid> { Guid.NewGuid() };

        var result = _service.ProcessEchoChain(0, adjacentTargets, false);

        result.ChainDamage.Should().Be(0);
        result.ChainTargets.Should().Be(0);
    }

    [Test]
    public void ProcessEchoChain_EmptyAdjacentTargets_ReturnsZeroChain()
    {
        var result = _service.ProcessEchoChain(20, new List<Guid>(), false);

        result.ChainDamage.Should().Be(0);
        result.ChainTargets.Should().Be(0);
    }

    // ===== GetAbilityReadiness Tests =====

    [Test]
    public void GetAbilityReadiness_NullPlayer_ThrowsArgumentNullException()
    {
        var act = () => _service.GetAbilityReadiness(null!, EchoCallerAbilityId.ScreamOfSilence);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetAbilityReadiness_WrongSpecialization_ReturnsRequiresEchoCaller()
    {
        var player = new Player("Not Echo-Caller");
        player.SetSpecialization("seidkona");

        var readiness = _service.GetAbilityReadiness(player, EchoCallerAbilityId.ScreamOfSilence);

        readiness.Should().Contain("Echo-Caller");
    }

    [Test]
    public void GetAbilityReadiness_AbilityNotUnlocked_ReturnsNotUnlocked()
    {
        var player = CreateEchoCaller();

        var readiness = _service.GetAbilityReadiness(player, EchoCallerAbilityId.ScreamOfSilence);

        readiness.Should().Contain("not unlocked");
    }

    [Test]
    public void GetAbilityReadiness_InsufficientAP_ReturnsInsufficientAP()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.ScreamOfSilence);
        player.CurrentAP = 1;

        var readiness = _service.GetAbilityReadiness(player, EchoCallerAbilityId.ScreamOfSilence);

        readiness.Should().Contain("Insufficient AP");
    }

    [Test]
    public void GetAbilityReadiness_InsufficientPP_ReturnsInsufficientPP()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.RealityFracture); // Tier 2, needs 8 PP
        // Only has 4 PP (RealityFracture is 4 PP, but needs 8+ to use it)

        var readiness = _service.GetAbilityReadiness(player, EchoCallerAbilityId.RealityFracture);

        readiness.Should().Contain("Insufficient PP");
    }

    [Test]
    public void GetAbilityReadiness_SilenceMadeWeapon_AlreadyUsed_ReturnsUsedThisCombat()
    {
        var player = CreateFullEchoCaller();
        var targets = new List<Guid> { Guid.NewGuid() };

        // Use capstone once
        _service.ExecuteSilenceMadeWeapon(player, targets, 0);

        var readiness = _service.GetAbilityReadiness(player, EchoCallerAbilityId.SilenceMadeWeapon);

        readiness.Should().Contain("already used");
    }

    [Test]
    public void GetAbilityReadiness_AllConditionsMet_ReturnsReady()
    {
        var player = CreateEchoCaller(EchoCallerAbilityId.ScreamOfSilence);
        player.CurrentAP = 20;

        var readiness = _service.GetAbilityReadiness(player, EchoCallerAbilityId.ScreamOfSilence);

        readiness.Should().Be("Ready");
    }

    [Test]
    public void GetAbilityReadiness_Tier2Ability_WithRequiredPP_ReturnsReady()
    {
        var player = CreateFullEchoCaller(); // Has all abilities, so has 8+ PP for Tier 2

        var readiness = _service.GetAbilityReadiness(player, EchoCallerAbilityId.RealityFracture);

        readiness.Should().Be("Ready");
    }
}
