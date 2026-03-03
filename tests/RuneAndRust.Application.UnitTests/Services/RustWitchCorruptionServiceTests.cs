using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="RustWitchCorruptionService"/>.
/// Verifies deterministic self-Corruption amounts by ability and rank.
/// </summary>
[TestFixture]
public class RustWitchCorruptionServiceTests
{
    private RustWitchCorruptionService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new RustWitchCorruptionService(
            Mock.Of<ILogger<RustWitchCorruptionService>>());
    }

    // ===== Constructor Tests =====

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new RustWitchCorruptionService(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ===== Passive Ability Safety Tests =====

    [Test]
    public void EvaluateRisk_PhilosopherOfDust_AlwaysSafe()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.PhilosopherOfDust, 1);
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
    }

    [Test]
    public void EvaluateRisk_EntropicField_AlwaysSafe()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.EntropicField, 1);
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
    }

    [Test]
    public void EvaluateRisk_AcceleratedEntropy_AlwaysSafe()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.AcceleratedEntropy, 1);
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
    }

    [Test]
    public void EvaluateRisk_CascadeReaction_AlwaysSafe()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.CascadeReaction, 1);
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
    }

    // ===== Corrosive Curse Tests =====

    [Test]
    public void EvaluateRisk_CorrosiveCurse_Rank1_Returns2Corruption()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.CorrosiveCurse, 1);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(2);
        result.Trigger.Should().Be(RustWitchCorruptionTrigger.CorrosiveCurseCast);
        result.AbilityRank.Should().Be(1);
    }

    [Test]
    public void EvaluateRisk_CorrosiveCurse_Rank2_Returns2Corruption()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.CorrosiveCurse, 2);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(2);
    }

    [Test]
    public void EvaluateRisk_CorrosiveCurse_Rank3_Returns1Corruption()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.CorrosiveCurse, 3);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
    }

    // ===== System Shock Tests =====

    [Test]
    public void EvaluateRisk_SystemShock_Rank1_Returns3Corruption()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.SystemShock, 1);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(3);
        result.Trigger.Should().Be(RustWitchCorruptionTrigger.SystemShockCast);
    }

    [Test]
    public void EvaluateRisk_SystemShock_Rank3_Returns2Corruption()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.SystemShock, 3);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(2);
    }

    // ===== Flash Rust Tests =====

    [Test]
    public void EvaluateRisk_FlashRust_Rank1_Returns4Corruption()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.FlashRust, 1);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(4);
        result.Trigger.Should().Be(RustWitchCorruptionTrigger.FlashRustCast);
    }

    [Test]
    public void EvaluateRisk_FlashRust_Rank3_Returns3Corruption()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.FlashRust, 3);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(3);
    }

    // ===== Unmaking Word Tests (no rank reduction) =====

    [Test]
    public void EvaluateRisk_UnmakingWord_Rank1_Returns4Corruption()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.UnmakingWord, 1);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(4);
        result.Trigger.Should().Be(RustWitchCorruptionTrigger.UnmakingWordCast);
    }

    [Test]
    public void EvaluateRisk_UnmakingWord_Rank3_StillReturns4Corruption()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.UnmakingWord, 3);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(4);
    }

    // ===== Entropic Cascade Tests (no rank reduction) =====

    [Test]
    public void EvaluateRisk_EntropicCascade_Rank1_Returns6Corruption()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.EntropicCascade, 1);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(6);
        result.Trigger.Should().Be(RustWitchCorruptionTrigger.EntropicCascadeCast);
    }

    [Test]
    public void EvaluateRisk_EntropicCascade_Rank3_StillReturns6Corruption()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.EntropicCascade, 3);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(6);
    }

    // ===== GetCorruptionCost Tests =====

    [Test]
    public void GetCorruptionCost_PassiveAbility_ReturnsZero()
    {
        _service.GetCorruptionCost(RustWitchAbilityId.PhilosopherOfDust, 1).Should().Be(0);
        _service.GetCorruptionCost(RustWitchAbilityId.EntropicField, 1).Should().Be(0);
        _service.GetCorruptionCost(RustWitchAbilityId.AcceleratedEntropy, 1).Should().Be(0);
        _service.GetCorruptionCost(RustWitchAbilityId.CascadeReaction, 1).Should().Be(0);
    }

    [Test]
    public void GetCorruptionCost_ActiveAbility_MatchesDesignDoc()
    {
        // Tier 1-2: rank-dependent
        _service.GetCorruptionCost(RustWitchAbilityId.CorrosiveCurse, 1).Should().Be(2);
        _service.GetCorruptionCost(RustWitchAbilityId.CorrosiveCurse, 3).Should().Be(1);
        _service.GetCorruptionCost(RustWitchAbilityId.SystemShock, 1).Should().Be(3);
        _service.GetCorruptionCost(RustWitchAbilityId.SystemShock, 3).Should().Be(2);
        _service.GetCorruptionCost(RustWitchAbilityId.FlashRust, 1).Should().Be(4);
        _service.GetCorruptionCost(RustWitchAbilityId.FlashRust, 3).Should().Be(3);

        // Tier 3+: fixed (no rank reduction)
        _service.GetCorruptionCost(RustWitchAbilityId.UnmakingWord, 1).Should().Be(4);
        _service.GetCorruptionCost(RustWitchAbilityId.UnmakingWord, 3).Should().Be(4);
        _service.GetCorruptionCost(RustWitchAbilityId.EntropicCascade, 1).Should().Be(6);
        _service.GetCorruptionCost(RustWitchAbilityId.EntropicCascade, 3).Should().Be(6);
    }

    // ===== GetTriggerDescription Tests =====

    [Test]
    public void GetTriggerDescription_AllTriggers_ReturnNonEmpty()
    {
        foreach (RustWitchCorruptionTrigger trigger in Enum.GetValues<RustWitchCorruptionTrigger>())
        {
            var description = _service.GetTriggerDescription(trigger);
            description.Should().NotBeNullOrEmpty($"Trigger {trigger} should have a description");
        }
    }

    // ===== Result Object Tests =====

    [Test]
    public void EvaluateRisk_TriggeredResult_HasCorrectProperties()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.CorrosiveCurse, 2);

        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().BePositive();
        result.Trigger.Should().NotBeNull();
        result.Reason.Should().NotBeNullOrEmpty();
        result.AbilityRank.Should().Be(2);
        result.EvaluatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.GetWarning().Should().Contain("WARNING");
        result.GetDescriptionForPlayer().Should().Contain("Corruption");
    }

    [Test]
    public void EvaluateRisk_SafeResult_HasCorrectProperties()
    {
        var result = _service.EvaluateRisk(RustWitchAbilityId.PhilosopherOfDust, 1);

        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
        result.Trigger.Should().BeNull();
        result.Reason.Should().NotBeNullOrEmpty();
        result.GetWarning().Should().Contain("Safe");
        result.GetDescriptionForPlayer().Should().Contain("No self-Corruption");
    }
}
