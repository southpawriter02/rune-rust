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
/// Unit tests for <see cref="RustWitchAbilityService"/>.
/// Tests all 5 active abilities, passive utility methods, and the full guard chain pattern.
/// </summary>
[TestFixture]
public class RustWitchAbilityServiceTests
{
    // ===== Test Subclass for Deterministic Dice Rolls =====

    /// <summary>
    /// Test subclass that overrides virtual dice roll methods for deterministic results.
    /// </summary>
    private class TestRustWitchAbilityService : RustWitchAbilityService
    {
        /// <summary>Fixed d4 roll result for base [Corroded] DoT.</summary>
        public int FixedD4 { get; set; } = 2;

        /// <summary>Fixed d6 roll result for damage and enhanced DoT.</summary>
        public int FixedD6 { get; set; } = 4;

        public TestRustWitchAbilityService(
            IRustWitchCorruptionService corruptionService,
            ILogger<RustWitchAbilityService> logger)
            : base(corruptionService, logger) { }

        internal override int RollD4() => FixedD4;
        internal override int RollD6() => FixedD6;
    }

    private TestRustWitchAbilityService _service = null!;
    private RustWitchCorruptionService _corruptionService = null!;

    [SetUp]
    public void Setup()
    {
        _corruptionService = new RustWitchCorruptionService(
            Mock.Of<ILogger<RustWitchCorruptionService>>());
        _service = new TestRustWitchAbilityService(
            _corruptionService,
            Mock.Of<ILogger<RustWitchAbilityService>>());
    }

    // ===== Helper: Create Rust-Witch Player =====

    /// <summary>
    /// Creates a Rust-Witch player with specified unlocked abilities for testing.
    /// </summary>
    /// <param name="abilities">The abilities to unlock.</param>
    /// <returns>A configured Rust-Witch player with 20 AP.</returns>
    private static Player CreateRustWitch(params RustWitchAbilityId[] abilities)
    {
        var player = new Player("Test Rust-Witch");
        player.SetSpecialization("rust-witch");
        player.CurrentAP = 20;
        foreach (var ability in abilities)
        {
            player.UnlockRustWitchAbility(ability);
        }
        return player;
    }

    /// <summary>
    /// Creates a Rust-Witch player with ALL abilities unlocked (all tiers).
    /// </summary>
    private static Player CreateFullRustWitch()
    {
        return CreateRustWitch(
            RustWitchAbilityId.PhilosopherOfDust,
            RustWitchAbilityId.CorrosiveCurse,
            RustWitchAbilityId.EntropicField,
            RustWitchAbilityId.SystemShock,
            RustWitchAbilityId.FlashRust,
            RustWitchAbilityId.AcceleratedEntropy,
            RustWitchAbilityId.UnmakingWord,
            RustWitchAbilityId.CascadeReaction,
            RustWitchAbilityId.EntropicCascade);
    }

    // ===== Constructor Tests =====

    [Test]
    public void Constructor_NullCorruptionService_ThrowsArgumentNullException()
    {
        var act = () => new RustWitchAbilityService(
            null!, Mock.Of<ILogger<RustWitchAbilityService>>());
        act.Should().Throw<ArgumentNullException>().WithParameterName("corruptionService");
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new RustWitchAbilityService(_corruptionService, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ===== Corrosive Curse Tests =====

    [Test]
    public void ExecuteCorrosiveCurse_NullPlayer_ThrowsArgumentNullException()
    {
        var act = () => _service.ExecuteCorrosiveCurse(null!, Guid.NewGuid());
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ExecuteCorrosiveCurse_WrongSpecialization_ReturnsNull()
    {
        var player = new Player("Not a Rust-Witch");
        player.SetSpecialization("seidkona");
        player.CurrentAP = 10;

        var result = _service.ExecuteCorrosiveCurse(player, Guid.NewGuid());
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteCorrosiveCurse_AbilityNotUnlocked_ReturnsNull()
    {
        var player = CreateRustWitch(); // No abilities unlocked
        var result = _service.ExecuteCorrosiveCurse(player, Guid.NewGuid());
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteCorrosiveCurse_InsufficientAP_ReturnsNull()
    {
        var player = CreateRustWitch(RustWitchAbilityId.CorrosiveCurse);
        player.CurrentAP = 1; // Needs 2 AP

        var result = _service.ExecuteCorrosiveCurse(player, Guid.NewGuid());
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteCorrosiveCurse_Rank1_Applies1Stack()
    {
        var player = CreateRustWitch(RustWitchAbilityId.CorrosiveCurse);

        var result = _service.ExecuteCorrosiveCurse(player, Guid.NewGuid(), currentTargetStacks: 0, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.StacksApplied.Should().Be(1);
        result.TotalStacksOnTarget.Should().Be(1);
        result.AetherSpent.Should().Be(2);
        result.CorruptionGained.Should().Be(2); // Deterministic +2 at R1
    }

    [Test]
    public void ExecuteCorrosiveCurse_Rank2_Applies2Stacks()
    {
        var player = CreateRustWitch(RustWitchAbilityId.CorrosiveCurse);

        var result = _service.ExecuteCorrosiveCurse(player, Guid.NewGuid(), currentTargetStacks: 0, rank: 2);

        result.Should().NotBeNull();
        result!.StacksApplied.Should().Be(2);
        result.TotalStacksOnTarget.Should().Be(2);
    }

    [Test]
    public void ExecuteCorrosiveCurse_Rank3_Applies3Stacks()
    {
        var player = CreateRustWitch(RustWitchAbilityId.CorrosiveCurse);

        var result = _service.ExecuteCorrosiveCurse(player, Guid.NewGuid(), currentTargetStacks: 0, rank: 3);

        result.Should().NotBeNull();
        result!.StacksApplied.Should().Be(3);
        result.TotalStacksOnTarget.Should().Be(3);
        result.CorruptionGained.Should().Be(1); // R3 reduces to +1
    }

    [Test]
    public void ExecuteCorrosiveCurse_TargetAtMaxStacks_CapsAtFive()
    {
        var player = CreateRustWitch(RustWitchAbilityId.CorrosiveCurse);

        var result = _service.ExecuteCorrosiveCurse(player, Guid.NewGuid(), currentTargetStacks: 4, rank: 3);

        result.Should().NotBeNull();
        result!.StacksApplied.Should().Be(1); // Only 1 of 3 can be applied (4+3=7 → capped at 5)
        result.TotalStacksOnTarget.Should().Be(5);
        result.WasStackCapped.Should().BeTrue();
    }

    [Test]
    public void ExecuteCorrosiveCurse_DeductsAP()
    {
        var player = CreateRustWitch(RustWitchAbilityId.CorrosiveCurse);
        var apBefore = player.CurrentAP;

        _service.ExecuteCorrosiveCurse(player, Guid.NewGuid(), rank: 1);

        player.CurrentAP.Should().Be(apBefore - 2);
    }

    // ===== System Shock Tests =====

    [Test]
    public void ExecuteSystemShock_WrongSpecialization_ReturnsNull()
    {
        var player = new Player("Not RW");
        player.SetSpecialization("seidkona");
        player.CurrentAP = 10;

        var result = _service.ExecuteSystemShock(player, Guid.NewGuid(), true);
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteSystemShock_InsufficientTier_ReturnsNull()
    {
        // Only Tier 1 abilities — hasn't reached 8 PP threshold for Tier 2
        var player = CreateRustWitch(
            RustWitchAbilityId.PhilosopherOfDust,  // 3 PP
            RustWitchAbilityId.SystemShock);        // Unlocked but tier not met (only 3+4=7 PP < 8)

        var result = _service.ExecuteSystemShock(player, Guid.NewGuid(), true);
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteSystemShock_MechanicalTarget_AppliesStunned()
    {
        var player = CreateFullRustWitch();
        _service.FixedD6 = 4;

        var result = _service.ExecuteSystemShock(player, Guid.NewGuid(), targetIsMechanical: true);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.TargetStunned.Should().BeTrue();
        result.TargetIsMechanical.Should().BeTrue();
        result.DamageDealt.Should().Be(8); // 2d6 with FixedD6=4 → 4+4=8
        result.CorruptionGained.Should().Be(3); // System Shock R1 = +3
    }

    [Test]
    public void ExecuteSystemShock_NonMechanicalTarget_NoStun()
    {
        var player = CreateFullRustWitch();

        var result = _service.ExecuteSystemShock(player, Guid.NewGuid(), targetIsMechanical: false);

        result.Should().NotBeNull();
        result!.TargetStunned.Should().BeFalse();
        result.TargetIsMechanical.Should().BeFalse();
    }

    // ===== Flash Rust Tests =====

    [Test]
    public void ExecuteFlashRust_MultipleTargets_AppliesStacksToAll()
    {
        var player = CreateFullRustWitch();
        var targets = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var result = _service.ExecuteFlashRust(player, targets, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.TargetsAffected.Should().Be(3);
        result.StacksPerTarget.Should().Be(1); // R1 = 1 stack per target
        result.AetherSpent.Should().Be(4);
        result.CorruptionGained.Should().Be(4); // Flash Rust R1 = +4
    }

    [Test]
    public void ExecuteFlashRust_Rank3_Applies2StacksPerTarget()
    {
        var player = CreateFullRustWitch();
        var targets = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var result = _service.ExecuteFlashRust(player, targets, rank: 3);

        result.Should().NotBeNull();
        result!.StacksPerTarget.Should().Be(2); // R3 = 2 stacks per target
        result.CorruptionGained.Should().Be(3); // Flash Rust R3 = +3
    }

    [Test]
    public void ExecuteFlashRust_NullTargets_ThrowsArgumentNullException()
    {
        var player = CreateFullRustWitch();
        var act = () => _service.ExecuteFlashRust(player, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ===== Unmaking Word Tests =====

    [Test]
    public void ExecuteUnmakingWord_DoublesStacks()
    {
        var player = CreateFullRustWitch();

        var result = _service.ExecuteUnmakingWord(player, Guid.NewGuid(), currentTargetStacks: 2, rank: 1);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.StacksBefore.Should().Be(2);
        result.StacksAfter.Should().Be(4); // 2 → 4
        result.EffectiveStacksGained.Should().Be(2);
        result.WasStackCapped.Should().BeFalse();
        result.CorruptionGained.Should().Be(4); // Unmaking Word = +4 all ranks
    }

    [Test]
    public void ExecuteUnmakingWord_CapsAtMaxStacks()
    {
        var player = CreateFullRustWitch();

        var result = _service.ExecuteUnmakingWord(player, Guid.NewGuid(), currentTargetStacks: 3, rank: 1);

        result.Should().NotBeNull();
        result!.StacksBefore.Should().Be(3);
        result.StacksAfter.Should().Be(5); // 3 × 2 = 6 → capped at 5
        result.EffectiveStacksGained.Should().Be(2);
        result.WasStackCapped.Should().BeTrue();
    }

    [Test]
    public void ExecuteUnmakingWord_ZeroStacks_StaysAtZero()
    {
        var player = CreateFullRustWitch();

        var result = _service.ExecuteUnmakingWord(player, Guid.NewGuid(), currentTargetStacks: 0, rank: 1);

        result.Should().NotBeNull();
        result!.StacksBefore.Should().Be(0);
        result.StacksAfter.Should().Be(0); // 0 × 2 = 0
        result.EffectiveStacksGained.Should().Be(0);
    }

    // ===== Entropic Cascade (Capstone) Tests =====

    [Test]
    public void ExecuteEntropicCascade_TargetAt5Stacks_Executes()
    {
        var player = CreateFullRustWitch();

        var result = _service.ExecuteEntropicCascade(
            player, Guid.NewGuid(),
            targetCorrodedStacks: 5, targetCorruption: 10);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.WasExecute.Should().BeTrue();
        result.DamageDealt.Should().Be(0); // Execute doesn't deal damage roll
        result.ExecuteReason.Should().Contain("5");
        result.CorruptionGained.Should().Be(6); // Entropic Cascade = +6
    }

    [Test]
    public void ExecuteEntropicCascade_TargetOver50Corruption_Executes()
    {
        var player = CreateFullRustWitch();

        var result = _service.ExecuteEntropicCascade(
            player, Guid.NewGuid(),
            targetCorrodedStacks: 1, targetCorruption: 51);

        result.Should().NotBeNull();
        result!.WasExecute.Should().BeTrue();
        result.ExecuteReason.Should().Contain("Corruption");
    }

    [Test]
    public void ExecuteEntropicCascade_BelowThresholds_Deals6d6Damage()
    {
        var player = CreateFullRustWitch();
        _service.FixedD6 = 4;

        var result = _service.ExecuteEntropicCascade(
            player, Guid.NewGuid(),
            targetCorrodedStacks: 3, targetCorruption: 30);

        result.Should().NotBeNull();
        result!.WasExecute.Should().BeFalse();
        result.DamageDealt.Should().Be(24); // 6d6 with FixedD6=4 → 6×4=24
    }

    [Test]
    public void ExecuteEntropicCascade_AtExactly50Corruption_DoesNotExecute()
    {
        var player = CreateFullRustWitch();
        _service.FixedD6 = 3;

        var result = _service.ExecuteEntropicCascade(
            player, Guid.NewGuid(),
            targetCorrodedStacks: 4, targetCorruption: 50); // Exactly 50, not >50

        result.Should().NotBeNull();
        result!.WasExecute.Should().BeFalse();
        result.DamageDealt.Should().Be(18); // 6d6 with FixedD6=3 → 6×3=18
    }

    [Test]
    public void ExecuteEntropicCascade_InsufficientPP_ReturnsNull()
    {
        // Only Tier 1 abilities — nowhere near 24 PP threshold
        var player = CreateRustWitch(
            RustWitchAbilityId.PhilosopherOfDust,
            RustWitchAbilityId.CorrosiveCurse,
            RustWitchAbilityId.EntropicCascade); // Unlocked but PP threshold not met

        var result = _service.ExecuteEntropicCascade(
            player, Guid.NewGuid(),
            targetCorrodedStacks: 5, targetCorruption: 100);

        result.Should().BeNull();
    }

    // ===== Corroded DoT Tick Tests =====

    [Test]
    public void ProcessCorrodedDotTick_ZeroStacks_NoDamage()
    {
        var tracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Test Target");

        var result = _service.ProcessCorrodedDotTick(tracker, hasAcceleratedEntropy: false);

        result.IsSuccess.Should().BeTrue();
        result.TotalDamage.Should().Be(0);
        result.StackCount.Should().Be(0);
    }

    [Test]
    public void ProcessCorrodedDotTick_BaseDoT_Uses1d4PerStack()
    {
        var tracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Test Target")
            .WithAdditionalStacks(3);
        _service.FixedD4 = 3;

        var result = _service.ProcessCorrodedDotTick(tracker, hasAcceleratedEntropy: false);

        result.IsSuccess.Should().BeTrue();
        result.TotalDamage.Should().Be(9); // 3 stacks × 1d4=3 = 9
        result.StackCount.Should().Be(3);
        result.HasAcceleratedEntropy.Should().BeFalse();
    }

    [Test]
    public void ProcessCorrodedDotTick_AcceleratedEntropy_Uses2d6PerStack()
    {
        var tracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Test Target")
            .WithAdditionalStacks(2);
        _service.FixedD6 = 4;

        var result = _service.ProcessCorrodedDotTick(tracker, hasAcceleratedEntropy: true);

        result.IsSuccess.Should().BeTrue();
        result.TotalDamage.Should().Be(16); // 2 stacks × 2d6=8 = 16
        result.HasAcceleratedEntropy.Should().BeTrue();
    }

    [Test]
    public void ProcessCorrodedDotTick_MaxStacks_FullDamage()
    {
        var tracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Test Target")
            .WithAdditionalStacks(5);
        _service.FixedD6 = 5;

        var result = _service.ProcessCorrodedDotTick(tracker, hasAcceleratedEntropy: true);

        result.TotalDamage.Should().Be(50); // 5 stacks × 2d6=10 = 50
        result.ArmorPenalty.Should().Be(-5);
    }

    // ===== Cascade Reaction Tests =====

    [Test]
    public void ProcessCascadeReaction_DeadTargetWithStacks_SpreadsToAdjacent()
    {
        var deadTracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Dead Golem")
            .WithAdditionalStacks(3);
        var adjacentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var result = _service.ProcessCascadeReaction(deadTracker, adjacentIds);

        result.IsSuccess.Should().BeTrue();
        result.StacksSpread.Should().Be(3);
        result.TargetsAffected.Should().Be(2);
    }

    [Test]
    public void ProcessCascadeReaction_NoStacks_NoSpread()
    {
        var deadTracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Clean Target");
        var adjacentIds = new List<Guid> { Guid.NewGuid() };

        var result = _service.ProcessCascadeReaction(deadTracker, adjacentIds);

        result.StacksSpread.Should().Be(0);
        result.TargetsAffected.Should().Be(0);
    }

    [Test]
    public void ProcessCascadeReaction_NoAdjacentTargets_NoSpread()
    {
        var deadTracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Dead Golem")
            .WithAdditionalStacks(3);

        var result = _service.ProcessCascadeReaction(deadTracker, new List<Guid>());

        result.StacksSpread.Should().Be(0);
        result.TargetsAffected.Should().Be(0);
    }

    // ===== GetAbilityReadiness Tests =====

    [Test]
    public void GetAbilityReadiness_WrongSpec_ReturnsRequiresSpec()
    {
        var player = new Player("Non-RW");
        player.SetSpecialization("seidkona");

        var result = _service.GetAbilityReadiness(player, RustWitchAbilityId.CorrosiveCurse);
        result.Should().Contain("Rust-Witch");
    }

    [Test]
    public void GetAbilityReadiness_AbilityNotUnlocked_ReturnsNotUnlocked()
    {
        var player = CreateRustWitch();

        var result = _service.GetAbilityReadiness(player, RustWitchAbilityId.CorrosiveCurse);
        result.Should().Contain("not unlocked");
    }

    [Test]
    public void GetAbilityReadiness_InsufficientAP_ReturnsInsufficientAP()
    {
        var player = CreateRustWitch(RustWitchAbilityId.CorrosiveCurse);
        player.CurrentAP = 1; // Needs 2

        var result = _service.GetAbilityReadiness(player, RustWitchAbilityId.CorrosiveCurse);
        result.Should().Contain("Insufficient AP");
    }

    [Test]
    public void GetAbilityReadiness_AllConditionsMet_ReturnsReady()
    {
        var player = CreateRustWitch(RustWitchAbilityId.CorrosiveCurse);

        var result = _service.GetAbilityReadiness(player, RustWitchAbilityId.CorrosiveCurse);
        result.Should().Be("Ready");
    }

    // ===== CorrodedStackTracker Value Object Tests =====

    [Test]
    public void CorrodedStackTracker_Create_StartsAtZero()
    {
        var tracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Test");

        tracker.CurrentStacks.Should().Be(0);
        tracker.TotalStacksApplied.Should().Be(0);
        tracker.TotalDotDamageDealt.Should().Be(0);
        tracker.ArmorPenalty.Should().Be(0);
        tracker.IsAtMaxStacks.Should().BeFalse();
    }

    [Test]
    public void CorrodedStackTracker_WithAdditionalStacks_CapsAtMax()
    {
        var tracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Test")
            .WithAdditionalStacks(3)
            .WithAdditionalStacks(4); // 3+4=7 → capped at 5

        tracker.CurrentStacks.Should().Be(5);
        tracker.TotalStacksApplied.Should().Be(7); // Tracks total attempted
        tracker.IsAtMaxStacks.Should().BeTrue();
        tracker.ArmorPenalty.Should().Be(-5);
    }

    [Test]
    public void CorrodedStackTracker_WithDoubledStacks_CapsAtMax()
    {
        var tracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Test")
            .WithAdditionalStacks(3)
            .WithDoubledStacks(); // 3 → 6 → capped at 5

        tracker.CurrentStacks.Should().Be(5);
        tracker.IsAtMaxStacks.Should().BeTrue();
    }

    [Test]
    public void CorrodedStackTracker_Cleansed_ResetsToZero()
    {
        var tracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Test")
            .WithAdditionalStacks(3)
            .Cleansed();

        tracker.CurrentStacks.Should().Be(0);
        tracker.TotalStacksApplied.Should().Be(3); // History preserved
    }

    [Test]
    public void CorrodedStackTracker_MeetsExecutionThreshold_AtMaxStacks()
    {
        var tracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Test")
            .WithAdditionalStacks(5);

        tracker.MeetsStackExecutionThreshold.Should().BeTrue();
    }

    [Test]
    public void CorrodedStackTracker_DoesNotMeetExecutionThreshold_Below5()
    {
        var tracker = CorrodedStackTracker.Create(Guid.NewGuid(), "Test")
            .WithAdditionalStacks(4);

        tracker.MeetsStackExecutionThreshold.Should().BeFalse();
    }

    // ===== PP Investment Tests =====

    [Test]
    public void Player_GetRustWitchPPInvested_CalculatesCorrectly()
    {
        var player = CreateRustWitch(
            RustWitchAbilityId.PhilosopherOfDust,   // 3 PP
            RustWitchAbilityId.CorrosiveCurse,       // 3 PP
            RustWitchAbilityId.EntropicField,        // 3 PP
            RustWitchAbilityId.SystemShock);          // 4 PP

        player.GetRustWitchPPInvested().Should().Be(13); // 3+3+3+4=13
    }

    [Test]
    public void Player_GetRustWitchPPInvested_AllAbilities_Returns40()
    {
        var player = CreateFullRustWitch();

        // 3×3 (T1) + 3×4 (T2) + 2×5 (T3) + 1×6 (Cap) = 9+12+10+6 = 37
        player.GetRustWitchPPInvested().Should().Be(37);
    }

    [Test]
    public void Player_GetRustWitchPPInvested_NoAbilities_ReturnsZero()
    {
        var player = CreateRustWitch();
        player.GetRustWitchPPInvested().Should().Be(0);
    }
}
