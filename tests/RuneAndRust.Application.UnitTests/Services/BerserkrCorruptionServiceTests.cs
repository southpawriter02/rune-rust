// ═══════════════════════════════════════════════════════════════════════════════
// BerserkrCorruptionServiceTests.cs
// Unit tests for the BerserkrCorruptionService covering risk evaluation,
// combat entry risk, and trigger descriptions.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class BerserkrCorruptionServiceTests
{
    private Mock<ILogger<BerserkrCorruptionService>> _loggerMock = null!;
    private BerserkrCorruptionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<BerserkrCorruptionService>>();
        _service = new BerserkrCorruptionService(_loggerMock.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EvaluateRisk — Passive Abilities
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void EvaluateRisk_BloodScent_AlwaysSafe()
    {
        // Arrange & Act
        var result = _service.EvaluateRisk(BerserkrAbilityId.BloodScent, currentRage: 100);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
    }

    [Test]
    public void EvaluateRisk_PainIsFuel_AlwaysSafe()
    {
        // Arrange & Act
        var result = _service.EvaluateRisk(BerserkrAbilityId.PainIsFuel, currentRage: 100);

        // Assert
        result.IsTriggered.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EvaluateRisk — Fury Strike
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void EvaluateRisk_FuryStrike_BelowThreshold_Safe()
    {
        // Arrange & Act
        var result = _service.EvaluateRisk(BerserkrAbilityId.FuryStrike, currentRage: 79);

        // Assert
        result.IsTriggered.Should().BeFalse();
        result.CorruptionAmount.Should().Be(0);
    }

    [Test]
    public void EvaluateRisk_FuryStrike_AtThreshold_TriggersCorruption()
    {
        // Arrange & Act
        var result = _service.EvaluateRisk(BerserkrAbilityId.FuryStrike, currentRage: 80);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
        result.Trigger.Should().Be(BerserkrCorruptionTrigger.FuryStrikeWhileEnraged);
    }

    [Test]
    public void EvaluateRisk_FuryStrike_AtMax_TriggersCorruption()
    {
        // Arrange & Act
        var result = _service.EvaluateRisk(BerserkrAbilityId.FuryStrike, currentRage: 100);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EvaluateRisk — Higher Tier Abilities (for completeness)
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void EvaluateRisk_AvatarOfDestruction_TriggersDouble()
    {
        // Arrange & Act
        var result = _service.EvaluateRisk(BerserkrAbilityId.AvatarOfDestruction, currentRage: 85);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(2);
        result.Trigger.Should().Be(BerserkrCorruptionTrigger.CapstoneActivation);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CheckCombatEntryRisk
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CheckCombatEntryRisk_BelowThreshold_Safe()
    {
        // Arrange & Act
        var result = _service.CheckCombatEntryRisk(currentRage: 50);

        // Assert
        result.IsTriggered.Should().BeFalse();
    }

    [Test]
    public void CheckCombatEntryRisk_AtThreshold_Triggers()
    {
        // Arrange & Act
        var result = _service.CheckCombatEntryRisk(currentRage: 80);

        // Assert
        result.IsTriggered.Should().BeTrue();
        result.CorruptionAmount.Should().Be(1);
        result.Trigger.Should().Be(BerserkrCorruptionTrigger.EnterCombatEnraged);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetTriggerDescription
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetTriggerDescription_FuryStrike_ReturnsDescription()
    {
        // Arrange & Act
        var desc = _service.GetTriggerDescription(BerserkrCorruptionTrigger.FuryStrikeWhileEnraged);

        // Assert
        desc.Should().Contain("Fury Strike");
        desc.Should().Contain("Enraged");
    }

    [Test]
    public void GetTriggerDescription_Capstone_ReturnsDescription()
    {
        // Arrange & Act
        var desc = _service.GetTriggerDescription(BerserkrCorruptionTrigger.CapstoneActivation);

        // Assert
        desc.Should().Contain("Avatar of Destruction");
        desc.Should().Contain("+2 Corruption");
    }
}
