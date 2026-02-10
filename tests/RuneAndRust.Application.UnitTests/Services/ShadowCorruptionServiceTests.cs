// ═══════════════════════════════════════════════════════════════════════════════
// ShadowCorruptionServiceTests.cs
// Unit tests for the ShadowCorruptionService, validating corruption risk
// evaluation across abilities and light levels.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Tests for <see cref="ShadowCorruptionService"/>.
/// </summary>
[TestFixture]
public class ShadowCorruptionServiceTests
{
    private ShadowCorruptionService _service = null!;
    private Mock<ILogger<ShadowCorruptionService>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<ShadowCorruptionService>>();
        _service = new ShadowCorruptionService(_mockLogger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Shadow Step Corruption Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void EvaluateRisk_ShadowStep_InBrightLight_ReturnsCorruption()
    {
        // Act
        var result = _service.EvaluateRisk(
            MyrkgengrAbilityId.ShadowStep,
            LightLevelType.BrightLight);

        // Assert
        result.RiskTriggered.Should().BeTrue();
        result.CorruptionGained.Should().Be(1);
    }

    [Test]
    public void EvaluateRisk_ShadowStep_InDarkness_ReturnsSafe()
    {
        // Act
        var result = _service.EvaluateRisk(
            MyrkgengrAbilityId.ShadowStep,
            LightLevelType.Darkness);

        // Assert
        result.RiskTriggered.Should().BeFalse();
        result.CorruptionGained.Should().Be(0);
    }

    [Test]
    public void EvaluateRisk_ShadowStep_InDimLight_ReturnsSafe()
    {
        // Act
        var result = _service.EvaluateRisk(
            MyrkgengrAbilityId.ShadowStep,
            LightLevelType.DimLight);

        // Assert
        result.RiskTriggered.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Cloak of Night Corruption Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void EvaluateRisk_CloakOfNight_InBrightLight_ReturnsCorruption()
    {
        // Act
        var result = _service.EvaluateRisk(
            MyrkgengrAbilityId.CloakOfNight,
            LightLevelType.BrightLight);

        // Assert
        result.RiskTriggered.Should().BeTrue();
        result.CorruptionGained.Should().Be(1);
    }

    [Test]
    public void EvaluateRisk_CloakOfNight_InDarkness_ReturnsSafe()
    {
        // Act
        var result = _service.EvaluateRisk(
            MyrkgengrAbilityId.CloakOfNight,
            LightLevelType.Darkness);

        // Assert
        result.RiskTriggered.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Dark-Adapted Corruption Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void EvaluateRisk_DarkAdapted_NeverTriggersCorruption()
    {
        // Act & Assert — Dark-Adapted should never trigger corruption
        foreach (var lightLevel in Enum.GetValues<LightLevelType>())
        {
            var result = _service.EvaluateRisk(MyrkgengrAbilityId.DarkAdapted, lightLevel);
            result.RiskTriggered.Should().BeFalse(
                $"Dark-Adapted should not trigger corruption in {lightLevel}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Coherent Target Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void EvaluateRisk_WithCoherentTarget_AddsOneCorruption()
    {
        // Act — bright light (base 1) + coherent target (+1) = 2
        var result = _service.EvaluateRisk(
            MyrkgengrAbilityId.ShadowStep,
            LightLevelType.BrightLight,
            targetIsCoherent: true);

        // Assert
        result.RiskTriggered.Should().BeTrue();
        result.CorruptionGained.Should().Be(2);
        result.TargetIsCoherent.Should().BeTrue();
    }

    [Test]
    public void EvaluateRisk_CoherentTarget_InDarkness_AddsOneCorruption()
    {
        // Act — darkness (base 0) + coherent target (+1) = 1
        var result = _service.EvaluateRisk(
            MyrkgengrAbilityId.ShadowStep,
            LightLevelType.Darkness,
            targetIsCoherent: true);

        // Assert
        result.RiskTriggered.Should().BeTrue();
        result.CorruptionGained.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Corruption Amount Lookup Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetCorruptionAmount_ReturnsCorrectValues()
    {
        // Act & Assert
        _service.GetCorruptionAmount(MyrkgengrAbilityId.ShadowStep, LightLevelType.BrightLight)
            .Should().Be(1);
        _service.GetCorruptionAmount(MyrkgengrAbilityId.ShadowStep, LightLevelType.Darkness)
            .Should().Be(0);
        _service.GetCorruptionAmount(MyrkgengrAbilityId.DarkAdapted, LightLevelType.BrightLight)
            .Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Triggers List Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetCorruptionTriggers_ShadowStep_ReturnsTwoTriggers()
    {
        // Act
        var triggers = _service.GetCorruptionTriggers(MyrkgengrAbilityId.ShadowStep);

        // Assert — BrightLight and Sunlight
        triggers.Should().HaveCount(2);
        triggers.Should().AllSatisfy(t => t.RiskTriggered.Should().BeTrue());
    }

    [Test]
    public void GetCorruptionTriggers_DarkAdapted_ReturnsEmpty()
    {
        // Act
        var triggers = _service.GetCorruptionTriggers(MyrkgengrAbilityId.DarkAdapted);

        // Assert — no triggers
        triggers.Should().BeEmpty();
    }
}
