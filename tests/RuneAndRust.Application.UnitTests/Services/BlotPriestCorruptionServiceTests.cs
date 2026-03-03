using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="BlotPriestCorruptionService"/>.
/// Verifies deterministic self-Corruption amounts and Blight Transfer amounts
/// by ability and rank for the most Corruption-intensive specialization.
/// </summary>
[TestFixture]
public class BlotPriestCorruptionServiceTests
{
    private BlotPriestCorruptionService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new BlotPriestCorruptionService(
            Mock.Of<ILogger<BlotPriestCorruptionService>>());
    }

    // ===== Constructor Tests =====

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new BlotPriestCorruptionService(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ===== Passive Ability Safety Tests =====

    [Test]
    public void EvaluateRisk_SanguinePact_AlwaysSafe()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.SanguinePact, 1);
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
        result.CorruptionTransferred.Should().Be(0);
    }

    [Test]
    public void EvaluateRisk_CrimsonVigor_AlwaysSafe()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.CrimsonVigor, 1);
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
    }

    [Test]
    public void EvaluateRisk_MartyrsResolve_AlwaysSafe()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.MartyrsResolve, 1);
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
    }

    // ===== Standard +1 Corruption Ability Tests =====

    [Test]
    public void EvaluateRisk_BloodSiphon_Returns1Corruption()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.BloodSiphon, 1);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
        result.CorruptionTransferred.Should().Be(0);
        result.Trigger.Should().Be(BlotPriestCorruptionTrigger.BloodSiphonCast);
    }

    [Test]
    public void EvaluateRisk_GiftOfVitae_Rank1_Returns1SelfAnd2Transfer()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.GiftOfVitae, 1);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
        result.CorruptionTransferred.Should().Be(2);
        result.Trigger.Should().Be(BlotPriestCorruptionTrigger.GiftOfVitaeCast);
    }

    [Test]
    public void EvaluateRisk_GiftOfVitae_Rank2_TransferReducedTo1()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.GiftOfVitae, 2);
        result.CorruptionAmount.Should().Be(1);
        result.CorruptionTransferred.Should().Be(1);
    }

    [Test]
    public void EvaluateRisk_GiftOfVitae_Rank3_TransferStays1()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.GiftOfVitae, 3);
        result.CorruptionAmount.Should().Be(1);
        result.CorruptionTransferred.Should().Be(1);
    }

    [Test]
    public void EvaluateRisk_BloodWard_Returns1Corruption()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.BloodWard, 1);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
        result.Trigger.Should().Be(BlotPriestCorruptionTrigger.BloodWardCast);
    }

    [Test]
    public void EvaluateRisk_Exsanguinate_Returns1CorruptionPerTick()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.Exsanguinate, 1);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
        result.Trigger.Should().Be(BlotPriestCorruptionTrigger.ExsanguinateTick);
    }

    // ===== Higher Corruption Ability Tests =====

    [Test]
    public void EvaluateRisk_HemorrhagingCurse_Returns2Corruption()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.HemorrhagingCurse, 1);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(2);
        result.Trigger.Should().Be(BlotPriestCorruptionTrigger.HemorrhagingCurseCast);
    }

    [Test]
    public void EvaluateRisk_HemorrhagingCurse_Rank3_StillReturns2Corruption()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.HemorrhagingCurse, 3);
        result.CorruptionAmount.Should().Be(2, "Hemorrhaging Curse has fixed corruption, no rank reduction");
    }

    [Test]
    public void EvaluateRisk_Heartstopper_Returns10SelfCorruptionAnd5Transfer()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.Heartstopper, 1);
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(10);
        result.CorruptionTransferred.Should().Be(5);
        result.Trigger.Should().Be(BlotPriestCorruptionTrigger.CrimsonDelugeCast);
    }

    // ===== GetCorruptionCost Tests =====

    [Test]
    public void GetCorruptionCost_PassiveAbility_ReturnsZero()
    {
        _service.GetCorruptionCost(BlotPriestAbilityId.SanguinePact, 1).Should().Be(0);
        _service.GetCorruptionCost(BlotPriestAbilityId.CrimsonVigor, 1).Should().Be(0);
        _service.GetCorruptionCost(BlotPriestAbilityId.MartyrsResolve, 1).Should().Be(0);
    }

    [Test]
    public void GetCorruptionCost_StandardAbilities_Return1()
    {
        _service.GetCorruptionCost(BlotPriestAbilityId.BloodSiphon, 1).Should().Be(1);
        _service.GetCorruptionCost(BlotPriestAbilityId.GiftOfVitae, 1).Should().Be(1);
        _service.GetCorruptionCost(BlotPriestAbilityId.BloodWard, 1).Should().Be(1);
        _service.GetCorruptionCost(BlotPriestAbilityId.Exsanguinate, 1).Should().Be(1);
    }

    [Test]
    public void GetCorruptionCost_HemorrhagingCurse_Returns2()
    {
        _service.GetCorruptionCost(BlotPriestAbilityId.HemorrhagingCurse, 1).Should().Be(2);
    }

    // ===== GetTransferAmount Tests =====

    [Test]
    public void GetTransferAmount_GiftOfVitae_Rank1_Returns2()
    {
        _service.GetTransferAmount(BlotPriestAbilityId.GiftOfVitae, 1).Should().Be(2);
    }

    [Test]
    public void GetTransferAmount_GiftOfVitae_Rank2Plus_Returns1()
    {
        _service.GetTransferAmount(BlotPriestAbilityId.GiftOfVitae, 2).Should().Be(1);
        _service.GetTransferAmount(BlotPriestAbilityId.GiftOfVitae, 3).Should().Be(1);
    }

    [Test]
    public void GetTransferAmount_Heartstopper_Returns5PerAlly()
    {
        _service.GetTransferAmount(BlotPriestAbilityId.Heartstopper, 1).Should().Be(5);
    }

    [Test]
    public void GetTransferAmount_NonTransferAbility_ReturnsZero()
    {
        _service.GetTransferAmount(BlotPriestAbilityId.BloodSiphon, 1).Should().Be(0);
        _service.GetTransferAmount(BlotPriestAbilityId.HemorrhagingCurse, 1).Should().Be(0);
    }

    // ===== GetTriggerDescription Tests =====

    [Test]
    public void GetTriggerDescription_AllTriggers_ReturnNonEmpty()
    {
        foreach (BlotPriestCorruptionTrigger trigger in Enum.GetValues<BlotPriestCorruptionTrigger>())
        {
            var description = _service.GetTriggerDescription(trigger);
            description.Should().NotBeNullOrEmpty($"Trigger {trigger} should have a description");
        }
    }

    // ===== Result Object Tests =====

    [Test]
    public void EvaluateRisk_TriggeredResult_HasCorrectProperties()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.GiftOfVitae, 1);

        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().BePositive();
        result.CorruptionTransferred.Should().BePositive();
        result.Trigger.Should().NotBeNull();
        result.Reason.Should().NotBeNullOrEmpty();
        result.AbilityRank.Should().Be(1);
        result.EvaluatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.GetWarning().Should().Contain("WARNING");
        result.GetDescriptionForPlayer().Should().Contain("Corruption");
    }

    [Test]
    public void EvaluateRisk_SafeResult_HasCorrectProperties()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.SanguinePact, 1);

        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
        result.CorruptionTransferred.Should().Be(0);
        result.Trigger.Should().BeNull();
        result.Reason.Should().NotBeNullOrEmpty();
        result.GetWarning().Should().Contain("Safe");
        result.GetDescriptionForPlayer().Should().Contain("No self-Corruption");
    }

    [Test]
    public void EvaluateRisk_TransferResult_WarningContainsTransferNote()
    {
        var result = _service.EvaluateRisk(BlotPriestAbilityId.GiftOfVitae, 1);
        result.GetWarning().Should().Contain("Transferred");
        result.GetDescriptionForPlayer().Should().Contain("Blight Transfer");
    }
}
