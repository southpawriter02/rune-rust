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
/// Unit tests for <see cref="VardWardenAbilityService"/>.
/// Tests all 6 active abilities, passive utility methods, RunicBarrierTracker, and the full guard chain pattern.
/// No Corruption mechanics (Coherent path). Once-per-expedition capstone tested.
/// </summary>
[TestFixture]
public class VardWardenAbilityServiceTests
{
    // ===== Test Subclass for Deterministic Dice Rolls =====

    /// <summary>
    /// Test subclass that overrides virtual dice roll methods for deterministic results.
    /// </summary>
    private class TestVardWardenAbilityService : VardWardenAbilityService
    {
        /// <summary>Fixed d6 roll result for damage/healing calculations.</summary>
        public int FixedD6 { get; set; } = 3;

        /// <summary>Fixed d10 roll result for temp HP calculations.</summary>
        public int FixedD10 { get; set; } = 5;

        public TestVardWardenAbilityService(ILogger<VardWardenAbilityService> logger)
            : base(logger) { }

        internal override int RollD6() => FixedD6;
        internal override int RollD10() => FixedD10;
    }

    private TestVardWardenAbilityService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new TestVardWardenAbilityService(
            Mock.Of<ILogger<VardWardenAbilityService>>());
    }

    // ===== Helper: Create Varð-Warden Player =====

    /// <summary>
    /// Creates a Varð-Warden player with specified unlocked abilities for testing.
    /// </summary>
    /// <param name="abilities">The abilities to unlock.</param>
    /// <returns>A configured Varð-Warden player with 20 AP.</returns>
    private static Player CreateVardWarden(params VardWardenAbilityId[] abilities)
    {
        var player = new Player("Test Varð-Warden");
        player.SetSpecialization("vard-warden");
        player.CurrentAP = 20;
        foreach (var ability in abilities)
        {
            player.UnlockVardWardenAbility(ability);
        }
        return player;
    }

    /// <summary>
    /// Creates a Varð-Warden player with ALL abilities unlocked (all tiers).
    /// </summary>
    private static Player CreateFullVardWarden()
    {
        return CreateVardWarden(
            VardWardenAbilityId.SanctifiedResolve,
            VardWardenAbilityId.RunicBarrier,
            VardWardenAbilityId.ConsecrateGround,
            VardWardenAbilityId.RuneOfShielding,
            VardWardenAbilityId.ReinforceWard,
            VardWardenAbilityId.WardensVigil,
            VardWardenAbilityId.GlyphOfSanctuary,
            VardWardenAbilityId.AegisOfSanctity,
            VardWardenAbilityId.IndomitableBastion);
    }

    // ===== Constructor Tests =====

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new VardWardenAbilityService(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ===== Runic Barrier Tests (29011) =====

    [Test]
    public void ExecuteRunicBarrier_NullPlayer_ThrowsArgumentNullException()
    {
        var act = () => _service.ExecuteRunicBarrier(null!, 5, 3);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ExecuteRunicBarrier_WrongSpecialization_ReturnsNull()
    {
        var player = new Player("Not Varð-Warden");
        player.SetSpecialization("rust-witch");
        player.CurrentAP = 10;

        var result = _service.ExecuteRunicBarrier(player, 5, 3);
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteRunicBarrier_AbilityNotUnlocked_ReturnsNull()
    {
        var player = CreateVardWarden();
        var result = _service.ExecuteRunicBarrier(player, 5, 3);
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteRunicBarrier_InsufficientAP_ReturnsNull()
    {
        var player = CreateVardWarden(VardWardenAbilityId.RunicBarrier);
        player.CurrentAP = 2;

        var result = _service.ExecuteRunicBarrier(player, 5, 3);
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteRunicBarrier_Rank1_Returns30HpBarrier()
    {
        var player = CreateVardWarden(VardWardenAbilityId.RunicBarrier);

        var result = _service.ExecuteRunicBarrier(player, 5, 3, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.BarrierHp.Should().Be(30);
        result.Duration.Should().Be(2);
        result.PositionX.Should().Be(5);
        result.PositionY.Should().Be(3);
        player.CurrentAP.Should().Be(17); // 20 - 3
    }

    [Test]
    public void ExecuteRunicBarrier_Rank2_Returns40HpBarrier()
    {
        var player = CreateVardWarden(VardWardenAbilityId.RunicBarrier);

        var result = _service.ExecuteRunicBarrier(player, 0, 0, rank: 2);

        result.Should().NotBeNull();
        result!.BarrierHp.Should().Be(40);
        result.Duration.Should().Be(3);
    }

    [Test]
    public void ExecuteRunicBarrier_Rank3_Returns50HpBarrier()
    {
        var player = CreateVardWarden(VardWardenAbilityId.RunicBarrier);

        var result = _service.ExecuteRunicBarrier(player, 0, 0, rank: 3);

        result.Should().NotBeNull();
        result!.BarrierHp.Should().Be(50);
        result.Duration.Should().Be(4);
    }

    // ===== Consecrate Ground Tests (29012) =====

    [Test]
    public void ExecuteConsecrateGround_Rank1_Returns1d6Healing()
    {
        var player = CreateVardWarden(VardWardenAbilityId.ConsecrateGround);
        _service.FixedD6 = 3;

        var result = _service.ExecuteConsecrateGround(player, 5, 5, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.HealPerTurn.Should().Be(3); // 1d6
        result.DamagePerTurn.Should().Be(3);
        result.Duration.Should().Be(3);
    }

    [Test]
    public void ExecuteConsecrateGround_Rank2_Returns1d6Plus2Healing()
    {
        var player = CreateVardWarden(VardWardenAbilityId.ConsecrateGround);
        _service.FixedD6 = 3;

        var result = _service.ExecuteConsecrateGround(player, 5, 5, rank: 2);

        result.Should().NotBeNull();
        result!.HealPerTurn.Should().Be(5); // 1d6 + 2
        result.Duration.Should().Be(4);
    }

    [Test]
    public void ExecuteConsecrateGround_Rank3_Returns2d6Healing()
    {
        var player = CreateVardWarden(VardWardenAbilityId.ConsecrateGround);
        _service.FixedD6 = 3;

        var result = _service.ExecuteConsecrateGround(player, 5, 5, rank: 3);

        result.Should().NotBeNull();
        result!.HealPerTurn.Should().Be(6); // 2d6 (3 + 3)
    }

    // ===== Rune of Shielding Tests (29013) =====

    [Test]
    public void ExecuteRuneOfShielding_WrongTier_ReturnsNull()
    {
        var player = CreateVardWarden(VardWardenAbilityId.RuneOfShielding);
        // No PP invested, so Tier 2 requirement not met

        var result = _service.ExecuteRuneOfShielding(player, Guid.NewGuid(), rank: 1);

        result.Should().BeNull();
    }

    [Test]
    public void ExecuteRuneOfShielding_Rank1_Returns3SoakAnd10PercentResist()
    {
        var player = CreateFullVardWarden();

        var result = _service.ExecuteRuneOfShielding(player, Guid.NewGuid(), rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.SoakBonus.Should().Be(3);
        result.CorruptionResistBonusPercent.Should().Be(10);
        result.Duration.Should().Be(4);
    }

    [Test]
    public void ExecuteRuneOfShielding_Rank2_Returns5SoakAnd15PercentResist()
    {
        var player = CreateFullVardWarden();

        var result = _service.ExecuteRuneOfShielding(player, Guid.NewGuid(), rank: 2);

        result.Should().NotBeNull();
        result!.SoakBonus.Should().Be(5);
        result.CorruptionResistBonusPercent.Should().Be(15);
    }

    [Test]
    public void ExecuteRuneOfShielding_Rank3_Returns7SoakAnd20PercentResist()
    {
        var player = CreateFullVardWarden();

        var result = _service.ExecuteRuneOfShielding(player, Guid.NewGuid(), rank: 3);

        result.Should().NotBeNull();
        result!.SoakBonus.Should().Be(7);
        result.CorruptionResistBonusPercent.Should().Be(20);
    }

    // ===== Reinforce Ward Tests (29014) =====

    [Test]
    public void ExecuteReinforceWard_BarrierMode_Rank1_Restores15Hp()
    {
        var player = CreateFullVardWarden();
        var barrierGuid = Guid.NewGuid();

        var result = _service.ExecuteReinforceWard(player, barrierGuid, isBarrier: true, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.IsBarrier.Should().BeTrue();
        result.HpRestored.Should().Be(15);
        result.ZoneBoostPercent.Should().BeNull();
    }

    [Test]
    public void ExecuteReinforceWard_BarrierMode_Rank3_Restores25Hp()
    {
        var player = CreateFullVardWarden();

        var result = _service.ExecuteReinforceWard(player, Guid.NewGuid(), isBarrier: true, rank: 3);

        result.Should().NotBeNull();
        result!.HpRestored.Should().Be(25);
    }

    [Test]
    public void ExecuteReinforceWard_ZoneMode_Rank1_Boosts50Percent()
    {
        var player = CreateFullVardWarden();

        var result = _service.ExecuteReinforceWard(player, Guid.NewGuid(), isBarrier: false, rank: 1);

        result.Should().NotBeNull();
        result!.IsBarrier.Should().BeFalse();
        result.ZoneBoostPercent.Should().Be(50);
        result.HpRestored.Should().BeNull();
    }

    [Test]
    public void ExecuteReinforceWard_ZoneMode_Rank3_Boosts100Percent()
    {
        var player = CreateFullVardWarden();

        var result = _service.ExecuteReinforceWard(player, Guid.NewGuid(), isBarrier: false, rank: 3);

        result.Should().NotBeNull();
        result!.ZoneBoostPercent.Should().Be(100);
    }

    // ===== Glyph of Sanctuary Tests (29016) =====

    [Test]
    public void ExecuteGlyphOfSanctuary_MultipleAllies_GrantsTempHpAndStressImmunity()
    {
        var player = CreateFullVardWarden();
        _service.FixedD10 = 5;
        var allyIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var result = _service.ExecuteGlyphOfSanctuary(player, allyIds, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.TempHpPerAlly.Should().Be(15); // 3d10 with fixed 5 = 5+5+5
        result.AlliesAffected.Should().Be(3);
        result.StressImmunityDuration.Should().Be(2);
        player.CurrentAP.Should().Be(16); // 20 - 4
    }

    [Test]
    public void ExecuteGlyphOfSanctuary_EmptyAllyList_StillSucceeds()
    {
        var player = CreateFullVardWarden();

        var result = _service.ExecuteGlyphOfSanctuary(player, Array.Empty<Guid>(), rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.AlliesAffected.Should().Be(0);
    }

    // ===== Indomitable Bastion Tests (29018) =====

    [Test]
    public void ExecuteIndomitableBastion_FirstUse_Succeeds()
    {
        var player = CreateFullVardWarden();

        var result = _service.ExecuteIndomitableBastion(player, Guid.NewGuid(), incomingDamage: 25, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.DamageNegated.Should().Be(25);
        result.BarrierCreated.Should().BeTrue();
        result.UsedExpeditionCharge.Should().BeTrue();
        player.HasUsedIndomitableBastionThisExpedition.Should().BeTrue();
    }

    [Test]
    public void ExecuteIndomitableBastion_SecondUse_ReturnsNull()
    {
        var player = CreateFullVardWarden();

        // First use
        var result1 = _service.ExecuteIndomitableBastion(player, Guid.NewGuid(), 25, rank: 1);
        result1.Should().NotBeNull();

        // Second use should fail
        var result2 = _service.ExecuteIndomitableBastion(player, Guid.NewGuid(), 25, rank: 1);
        result2.Should().BeNull();
    }

    [Test]
    public void ExecuteIndomitableBastion_ResetExpeditionCharge_AllowsReuse()
    {
        var player = CreateFullVardWarden();

        var result1 = _service.ExecuteIndomitableBastion(player, Guid.NewGuid(), 25, rank: 1);
        result1.Should().NotBeNull();

        // Simulate expedition end/reset
        player.HasUsedIndomitableBastionThisExpedition = false;

        var result2 = _service.ExecuteIndomitableBastion(player, Guid.NewGuid(), 25, rank: 1);
        result2.Should().NotBeNull();
    }

    // ===== Zone Tick Tests =====

    [Test]
    public void ProcessZoneTick_ConsecratedGround_Rank1_Heals1d6()
    {
        _service.FixedD6 = 4;
        var allies = new[] { Guid.NewGuid() };
        var enemies = Array.Empty<Guid>();

        var result = _service.ProcessZoneTick(allies, enemies, isConsecratedGround: true, rank: 1);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.HealAmount.Should().Be(4); // 1d6
        result.DamageAmount.Should().Be(4);
        result.TargetsAffected.Should().Be(1);
    }

    [Test]
    public void ProcessZoneTick_ConsecratedGround_Rank3_Heals2d6()
    {
        _service.FixedD6 = 4;
        var allies = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var enemies = new[] { Guid.NewGuid() };

        var result = _service.ProcessZoneTick(allies, enemies, isConsecratedGround: true, rank: 3);

        result.Should().NotBeNull();
        result.HealAmount.Should().Be(8); // 2d6 (4+4)
        result.TargetsAffected.Should().Be(3);
    }

    [Test]
    public void ProcessZoneTick_NonConsecratedZone_ReturnsNoEffect()
    {
        var allies = new[] { Guid.NewGuid() };
        var enemies = new[] { Guid.NewGuid() };

        var result = _service.ProcessZoneTick(allies, enemies, isConsecratedGround: false, rank: 1);

        result.IsSuccess.Should().BeTrue();
        result.HealAmount.Should().Be(0);
        result.DamageAmount.Should().Be(0);
        result.TargetsAffected.Should().Be(0);
    }

    // ===== GetAbilityReadiness Tests =====

    [Test]
    public void GetAbilityReadiness_ReadyAbility_ReturnsReady()
    {
        var player = CreateVardWarden(VardWardenAbilityId.RunicBarrier);

        var readiness = _service.GetAbilityReadiness(player, VardWardenAbilityId.RunicBarrier);

        readiness.Should().Be("Ready");
    }

    [Test]
    public void GetAbilityReadiness_UnlockedAbility_ReturnsNotUnlocked()
    {
        var player = CreateVardWarden();

        var readiness = _service.GetAbilityReadiness(player, VardWardenAbilityId.RunicBarrier);

        readiness.Should().Contain("not unlocked");
    }

    [Test]
    public void GetAbilityReadiness_InsufficientAP_ReturnsInsufficientAP()
    {
        var player = CreateVardWarden(VardWardenAbilityId.RunicBarrier);
        player.CurrentAP = 2;

        var readiness = _service.GetAbilityReadiness(player, VardWardenAbilityId.RunicBarrier);

        readiness.Should().Contain("Insufficient AP");
    }

    [Test]
    public void GetAbilityReadiness_Tier2WithoutPP_ReturnsInsufficientPP()
    {
        var player = CreateVardWarden(VardWardenAbilityId.RuneOfShielding);

        var readiness = _service.GetAbilityReadiness(player, VardWardenAbilityId.RuneOfShielding);

        readiness.Should().Contain("Insufficient PP");
    }

    [Test]
    public void GetAbilityReadiness_IndomitableBastionUsed_ReturnsAlreadyUsed()
    {
        var player = CreateFullVardWarden();
        player.HasUsedIndomitableBastionThisExpedition = true;

        var readiness = _service.GetAbilityReadiness(player, VardWardenAbilityId.IndomitableBastion);

        readiness.Should().Contain("already used this expedition");
    }

    // ===== RunicBarrierTracker Tests =====

    [Test]
    public void RunicBarrierTracker_Constructor_ValidValues_Succeeds()
    {
        var tracker = new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: 30, maxHp: 30, remainingTurns: 2,
            positionX: 5, positionY: 3, rank: 1);

        tracker.CurrentHp.Should().Be(30);
        tracker.MaxHp.Should().Be(30);
        tracker.RemainingTurns.Should().Be(2);
        tracker.IsDestroyed.Should().BeFalse();
    }

    [Test]
    public void RunicBarrierTracker_Constructor_NegativeHp_ThrowsArgumentException()
    {
        var act = () => new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: -1, maxHp: 30, remainingTurns: 2,
            positionX: 5, positionY: 3, rank: 1);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void RunicBarrierTracker_Constructor_InvalidRank_ThrowsArgumentException()
    {
        var act = () => new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: 30, maxHp: 30, remainingTurns: 2,
            positionX: 5, positionY: 3, rank: 4);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void RunicBarrierTracker_WithDamage_ReducesHP()
    {
        var tracker = new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: 30, maxHp: 30, remainingTurns: 2,
            positionX: 5, positionY: 3, rank: 1);

        var damaged = tracker.WithDamage(10);

        damaged.CurrentHp.Should().Be(20);
        damaged.IsDestroyed.Should().BeFalse();
    }

    [Test]
    public void RunicBarrierTracker_WithDamage_ToZero_MarkDestroyed()
    {
        var tracker = new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: 10, maxHp: 30, remainingTurns: 2,
            positionX: 5, positionY: 3, rank: 1);

        var destroyed = tracker.WithDamage(10);

        destroyed.CurrentHp.Should().Be(0);
        destroyed.IsDestroyed.Should().BeTrue();
    }

    [Test]
    public void RunicBarrierTracker_WithDamage_ExceedsMax_CapsAtZero()
    {
        var tracker = new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: 5, maxHp: 30, remainingTurns: 2,
            positionX: 5, positionY: 3, rank: 1);

        var damaged = tracker.WithDamage(100);

        damaged.CurrentHp.Should().Be(0);
    }

    [Test]
    public void RunicBarrierTracker_WithHealing_RestoresHP()
    {
        var tracker = new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: 10, maxHp: 30, remainingTurns: 2,
            positionX: 5, positionY: 3, rank: 1);

        var healed = tracker.WithHealing(15);

        healed.CurrentHp.Should().Be(25);
    }

    [Test]
    public void RunicBarrierTracker_WithHealing_CapsAtMaxHP()
    {
        var tracker = new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: 25, maxHp: 30, remainingTurns: 2,
            positionX: 5, positionY: 3, rank: 1);

        var healed = tracker.WithHealing(100);

        healed.CurrentHp.Should().Be(30);
    }

    [Test]
    public void RunicBarrierTracker_TickTurn_DecrementsTurns()
    {
        var tracker = new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: 30, maxHp: 30, remainingTurns: 2,
            positionX: 5, positionY: 3, rank: 1);

        var ticked = tracker.TickTurn();

        ticked.RemainingTurns.Should().Be(1);
        ticked.IsDestroyed.Should().BeFalse();
    }

    [Test]
    public void RunicBarrierTracker_TickTurn_ToZero_MarkDestroyed()
    {
        var tracker = new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: 30, maxHp: 30, remainingTurns: 1,
            positionX: 5, positionY: 3, rank: 1);

        var ticked = tracker.TickTurn();

        ticked.RemainingTurns.Should().Be(0);
        ticked.IsDestroyed.Should().BeTrue();
    }

    [Test]
    public void RunicBarrierTracker_Rank3_DestructionDamage_Returns7()
    {
        var tracker = new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: 50, maxHp: 50, remainingTurns: 4,
            positionX: 5, positionY: 3, rank: 3);

        tracker.DestructionDamage.Should().Be(7); // 2d6 average = 7
    }

    [Test]
    public void RunicBarrierTracker_Rank1_DestructionDamage_ReturnsZero()
    {
        var tracker = new RunicBarrierTracker(
            Guid.NewGuid(), currentHp: 30, maxHp: 30, remainingTurns: 2,
            positionX: 5, positionY: 3, rank: 1);

        tracker.DestructionDamage.Should().Be(0);
    }
}
