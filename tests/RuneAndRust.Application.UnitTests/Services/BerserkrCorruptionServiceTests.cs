using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="BerserkrCorruptionService"/>.
/// </summary>
[TestFixture]
public class BerserkrCorruptionServiceTests
{
    private BerserkrCorruptionService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new BerserkrCorruptionService(
            Mock.Of<ILogger<BerserkrCorruptionService>>());
    }

    // ===== Passive Ability Safety Tests =====

    [Test]
    public void EvaluateRisk_BloodScent_AlwaysSafe()
    {
        // Act — even at max Rage, passive is safe
        var result = _service.EvaluateRisk(BerserkrAbilityId.BloodScent, 100);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
    }

    [Test]
    public void EvaluateRisk_PainIsFuel_AlwaysSafe()
    {
        // Act
        var result = _service.EvaluateRisk(BerserkrAbilityId.PainIsFuel, 100);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
    }

    // ===== Fury Strike Threshold Tests =====

    [Test]
    public void EvaluateRisk_FuryStrike_BelowThreshold_Safe()
    {
        // Act
        var result = _service.EvaluateRisk(BerserkrAbilityId.FuryStrike, 79);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
    }

    [Test]
    public void EvaluateRisk_FuryStrike_AtThreshold_TriggersCorruption()
    {
        // Act
        var result = _service.EvaluateRisk(BerserkrAbilityId.FuryStrike, 80);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
        result.Trigger.Should().Be(BerserkrCorruptionTrigger.FuryStrikeWhileEnraged);
    }

    [Test]
    public void EvaluateRisk_FuryStrike_AtMaxRage_TriggersCorruption()
    {
        // Act
        var result = _service.EvaluateRisk(BerserkrAbilityId.FuryStrike, 100);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
    }

    // ===== Capstone Tests =====

    [Test]
    public void EvaluateRisk_Capstone_AlwaysTriggersTwo()
    {
        // Act — even at 0 Rage, capstone triggers +2
        var result = _service.EvaluateRisk(BerserkrAbilityId.AvatarOfDestruction, 0);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(2);
        result.Trigger.Should().Be(BerserkrCorruptionTrigger.CapstoneActivation);
    }

    // ===== Combat Entry Tests =====

    [Test]
    public void CheckCombatEntryRisk_BelowThreshold_Safe()
    {
        // Act
        var result = _service.CheckCombatEntryRisk(50);

        // Assert
        result.IsTriggered.Should().BeFalse();
    }

    [Test]
    public void CheckCombatEntryRisk_AtThreshold_TriggersCorruption()
    {
        // Act
        var result = _service.CheckCombatEntryRisk(80);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
        result.Trigger.Should().Be(BerserkrCorruptionTrigger.EnterCombatEnraged);
    }

    // ===== Trigger Description Tests =====

    [Test]
    public void GetTriggerDescription_ReturnsNonEmptyForAllTriggers()
    {
        // Act & Assert — every trigger should have a meaningful description
        foreach (BerserkrCorruptionTrigger trigger in Enum.GetValues(typeof(BerserkrCorruptionTrigger)))
        {
            var description = _service.GetTriggerDescription(trigger);
            description.Should().NotBeNullOrWhiteSpace(
                $"Trigger {trigger} should have a description");
        }
    }
}
