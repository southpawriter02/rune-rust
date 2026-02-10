// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionRiskResultTests.cs
// Unit tests for the CorruptionRiskResult value object, validating factory
// methods and player-facing descriptions.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tests for <see cref="CorruptionRiskResult"/>.
/// </summary>
[TestFixture]
public class CorruptionRiskResultTests
{
    [Test]
    public void CreateTriggered_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var result = CorruptionRiskResult.CreateTriggered(
            corruptionGained: 1,
            reason: "Shadow Step used in bright light",
            abilityUsed: "shadow-step",
            lightCondition: LightLevelType.BrightLight);

        // Assert
        result.RiskTriggered.Should().BeTrue();
        result.CorruptionGained.Should().Be(1);
        result.Reason.Should().Be("Shadow Step used in bright light");
        result.AbilityUsed.Should().Be("shadow-step");
        result.LightCondition.Should().Be(LightLevelType.BrightLight);
        result.TargetIsCoherent.Should().BeFalse();
        result.EvaluatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Test]
    public void CreateSafe_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var result = CorruptionRiskResult.CreateSafe(
            abilityUsed: "shadow-step",
            lightCondition: LightLevelType.Darkness);

        // Assert
        result.RiskTriggered.Should().BeFalse();
        result.CorruptionGained.Should().Be(0);
        result.AbilityUsed.Should().Be("shadow-step");
        result.LightCondition.Should().Be(LightLevelType.Darkness);
    }

    [Test]
    public void GetDescriptionForPlayer_Safe_ReturnsAcceptMessage()
    {
        // Arrange
        var result = CorruptionRiskResult.CreateSafe("shadow-step", LightLevelType.Darkness);

        // Act
        var description = result.GetDescriptionForPlayer();

        // Assert
        description.Should().Contain("without consequence");
    }

    [Test]
    public void GetDescriptionForPlayer_Triggered_ReturnsCorruptionMessage()
    {
        // Arrange
        var result = CorruptionRiskResult.CreateTriggered(
            corruptionGained: 1,
            reason: "test",
            abilityUsed: "shadow-step",
            lightCondition: LightLevelType.BrightLight);

        // Act
        var description = result.GetDescriptionForPlayer();

        // Assert
        description.Should().Contain("corruption");
    }

    [Test]
    public void CreateTriggered_WithCoherentTarget_SetsFlag()
    {
        // Arrange & Act
        var result = CorruptionRiskResult.CreateTriggered(
            corruptionGained: 2,
            reason: "Coherent target",
            abilityUsed: "shadow-step",
            lightCondition: LightLevelType.BrightLight,
            targetIsCoherent: true);

        // Assert
        result.TargetIsCoherent.Should().BeTrue();
        result.CorruptionGained.Should().Be(2);
    }
}
