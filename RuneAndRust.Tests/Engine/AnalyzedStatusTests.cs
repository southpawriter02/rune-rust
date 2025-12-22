using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the Analyzed status effect introduced in v0.3.6c.
/// Validates the intel debuff behavior for revealing enemy intent.
/// </summary>
public class AnalyzedStatusTests
{
    private readonly Mock<IDiceService> _mockDice;
    private readonly Mock<ILogger<StatusEffectService>> _mockLogger;
    private readonly StatusEffectService _sut;

    public AnalyzedStatusTests()
    {
        _mockDice = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<StatusEffectService>>();
        _sut = new StatusEffectService(_mockDice.Object, _mockLogger.Object);
    }

    [Fact]
    public void ApplyEffect_Analyzed_DoesNotStack()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var sourceId = Guid.NewGuid();

        // Act - Apply Analyzed twice
        _sut.ApplyEffect(combatant, StatusEffectType.Analyzed, 3, sourceId);
        _sut.ApplyEffect(combatant, StatusEffectType.Analyzed, 3, sourceId);

        // Assert - Should have 1 stack only
        var effect = combatant.StatusEffects.FirstOrDefault(e => e.Type == StatusEffectType.Analyzed);
        effect.Should().NotBeNull();
        effect!.Stacks.Should().Be(1);
    }

    [Fact]
    public void ApplyEffect_Analyzed_RefreshesDuration_OnReapplication()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var sourceId = Guid.NewGuid();

        // Act - Apply with 2 turns, then reapply with 5 turns
        _sut.ApplyEffect(combatant, StatusEffectType.Analyzed, 2, sourceId);
        _sut.ApplyEffect(combatant, StatusEffectType.Analyzed, 5, sourceId);

        // Assert - Duration should be refreshed to 5
        var effect = combatant.StatusEffects.FirstOrDefault(e => e.Type == StatusEffectType.Analyzed);
        effect.Should().NotBeNull();
        effect!.DurationRemaining.Should().Be(5);
    }

    [Fact]
    public void HasEffect_Analyzed_ReturnsTrue_WhenApplied()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var sourceId = Guid.NewGuid();

        // Act
        _sut.ApplyEffect(combatant, StatusEffectType.Analyzed, 3, sourceId);

        // Assert
        _sut.HasEffect(combatant, StatusEffectType.Analyzed).Should().BeTrue();
    }

    [Fact]
    public void HasEffect_Analyzed_ReturnsFalse_WhenNotApplied()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Assert
        _sut.HasEffect(combatant, StatusEffectType.Analyzed).Should().BeFalse();
    }

    [Fact]
    public void ProcessTurnEnd_Analyzed_DecrementsDuration()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var sourceId = Guid.NewGuid();
        _sut.ApplyEffect(combatant, StatusEffectType.Analyzed, 3, sourceId);

        // Act
        _sut.ProcessTurnEnd(combatant);

        // Assert
        var effect = combatant.StatusEffects.FirstOrDefault(e => e.Type == StatusEffectType.Analyzed);
        effect.Should().NotBeNull();
        effect!.DurationRemaining.Should().Be(2);
    }

    [Fact]
    public void ProcessTurnEnd_Analyzed_RemovesEffect_WhenExpired()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var sourceId = Guid.NewGuid();
        _sut.ApplyEffect(combatant, StatusEffectType.Analyzed, 1, sourceId);

        // Act
        _sut.ProcessTurnEnd(combatant);

        // Assert
        _sut.HasEffect(combatant, StatusEffectType.Analyzed).Should().BeFalse();
    }

    [Fact]
    public void Analyzed_IsDebuff()
    {
        // Assert - Value 6 < 100 means it's a debuff
        ActiveStatusEffect.IsDebuff(StatusEffectType.Analyzed).Should().BeTrue();
    }

    [Fact]
    public void Analyzed_IsNotDamageOverTime()
    {
        // Assert - Analyzed is an intel effect, not DoT
        ActiveStatusEffect.IsDamageOverTime(StatusEffectType.Analyzed).Should().BeFalse();
    }

    [Fact]
    public void Analyzed_CannotStack()
    {
        // Assert
        ActiveStatusEffect.CanStack(StatusEffectType.Analyzed).Should().BeFalse();
    }

    [Fact]
    public void Analyzed_MaxStacks_IsOne()
    {
        // Assert
        ActiveStatusEffect.GetMaxStacks(StatusEffectType.Analyzed).Should().Be(1);
    }

    #region Helper Methods

    private static Combatant CreateTestCombatant()
    {
        return new Combatant
        {
            Id = Guid.NewGuid(),
            Name = "Test Combatant",
            IsPlayer = false,
            CurrentHp = 50,
            MaxHp = 50,
            CurrentStamina = 30,
            MaxStamina = 30,
            ArmorSoak = 2
        };
    }

    #endregion
}
