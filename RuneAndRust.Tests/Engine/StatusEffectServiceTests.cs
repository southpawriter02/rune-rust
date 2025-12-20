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
/// Tests for the StatusEffectService class.
/// Validates effect application, stacking, DoT damage, turn processing, and stat modifiers.
/// </summary>
public class StatusEffectServiceTests
{
    private readonly Mock<IDiceService> _mockDice;
    private readonly Mock<ILogger<StatusEffectService>> _mockLogger;
    private readonly StatusEffectService _sut;

    public StatusEffectServiceTests()
    {
        _mockDice = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<StatusEffectService>>();
        _sut = new StatusEffectService(_mockDice.Object, _mockLogger.Object);
    }

    #region ApplyEffect Tests

    [Fact]
    public void ApplyEffect_NewEffect_AddsToList()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var sourceId = Guid.NewGuid();

        // Act
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, sourceId);

        // Assert
        combatant.StatusEffects.Should().HaveCount(1);
        var effect = combatant.StatusEffects.First();
        effect.Type.Should().Be(StatusEffectType.Bleeding);
        effect.Stacks.Should().Be(1);
        effect.DurationRemaining.Should().Be(3);
        effect.SourceId.Should().Be(sourceId);
    }

    [Fact]
    public void ApplyEffect_StackableEffect_IncreasesStacks()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var sourceId = Guid.NewGuid();
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, sourceId);

        // Act
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, sourceId);

        // Assert
        combatant.StatusEffects.Should().HaveCount(1);
        combatant.StatusEffects.First().Stacks.Should().Be(2);
    }

    [Fact]
    public void ApplyEffect_AtMaxStacks_RefreshesDurationOnly()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var sourceId = Guid.NewGuid();

        // Apply 5 stacks of bleeding
        for (int i = 0; i < 5; i++)
        {
            _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 2, sourceId);
        }

        // Act - apply 6th stack with longer duration
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 5, sourceId);

        // Assert
        combatant.StatusEffects.Should().HaveCount(1);
        var effect = combatant.StatusEffects.First();
        effect.Stacks.Should().Be(5); // Capped at max
        effect.DurationRemaining.Should().Be(5); // Duration refreshed
    }

    [Fact]
    public void ApplyEffect_NonStackable_RefreshesDurationOnly()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var sourceId = Guid.NewGuid();
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 1, sourceId);

        // Act
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 3, sourceId);

        // Assert
        combatant.StatusEffects.Should().HaveCount(1);
        var effect = combatant.StatusEffects.First();
        effect.Stacks.Should().Be(1); // Cannot stack
        effect.DurationRemaining.Should().Be(3); // Duration refreshed
    }

    [Fact]
    public void ApplyEffect_DifferentTypes_AddsSeparateEffects()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var sourceId = Guid.NewGuid();

        // Act
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, sourceId);
        _sut.ApplyEffect(combatant, StatusEffectType.Poisoned, 2, sourceId);
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 1, sourceId);

        // Assert
        combatant.StatusEffects.Should().HaveCount(3);
        combatant.StatusEffects.Select(e => e.Type).Should()
            .Contain(new[] { StatusEffectType.Bleeding, StatusEffectType.Poisoned, StatusEffectType.Stunned });
    }

    [Fact]
    public void ApplyEffect_Fortified_StacksCorrectly()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var sourceId = Guid.NewGuid();

        // Act
        _sut.ApplyEffect(combatant, StatusEffectType.Fortified, 3, sourceId);
        _sut.ApplyEffect(combatant, StatusEffectType.Fortified, 3, sourceId);
        _sut.ApplyEffect(combatant, StatusEffectType.Fortified, 3, sourceId);

        // Assert
        combatant.StatusEffects.Should().HaveCount(1);
        combatant.StatusEffects.First().Stacks.Should().Be(3);
    }

    #endregion

    #region RemoveEffect Tests

    [Fact]
    public void RemoveEffect_ExistingEffect_RemovesFromList()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Poisoned, 2, Guid.NewGuid());

        // Act
        _sut.RemoveEffect(combatant, StatusEffectType.Bleeding);

        // Assert
        combatant.StatusEffects.Should().HaveCount(1);
        combatant.StatusEffects.First().Type.Should().Be(StatusEffectType.Poisoned);
    }

    [Fact]
    public void RemoveEffect_NonExistentEffect_NoException()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act & Assert - should not throw
        _sut.RemoveEffect(combatant, StatusEffectType.Stunned);
        combatant.StatusEffects.Should().BeEmpty();
    }

    [Fact]
    public void ClearAllEffects_RemovesAllEffects()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 1, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Fortified, 2, Guid.NewGuid());

        // Act
        _sut.ClearAllEffects(combatant);

        // Assert
        combatant.StatusEffects.Should().BeEmpty();
    }

    #endregion

    #region ProcessTurnStart (DoT) Tests

    [Fact]
    public void ProcessTurnStart_Bleeding_DealsDamage()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(4);

        // Act
        var damage = _sut.ProcessTurnStart(combatant);

        // Assert
        damage.Should().Be(4);
        _mockDice.Verify(d => d.RollSingle(6, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ProcessTurnStart_BleedingMultiStack_MultipliesDamage()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid()); // 3 stacks

        _mockDice.SetupSequence(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(3)
            .Returns(4)
            .Returns(5);

        // Act
        var damage = _sut.ProcessTurnStart(combatant);

        // Assert
        damage.Should().Be(12); // 3 + 4 + 5 = 12
        _mockDice.Verify(d => d.RollSingle(6, It.IsAny<string>()), Times.Exactly(3));
    }

    [Fact]
    public void ProcessTurnStart_Poisoned_AppliesSoak()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        combatant.ArmorSoak = 3;
        _sut.ApplyEffect(combatant, StatusEffectType.Poisoned, 3, Guid.NewGuid());
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(5);

        // Act
        var damage = _sut.ProcessTurnStart(combatant);

        // Assert
        damage.Should().Be(2); // 5 - 3 = 2, minimum 1 if any damage dealt
    }

    [Fact]
    public void ProcessTurnStart_PoisonedWithHighSoak_MinimumOneDamage()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        combatant.ArmorSoak = 10;
        _sut.ApplyEffect(combatant, StatusEffectType.Poisoned, 3, Guid.NewGuid());
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(2);

        // Act
        var damage = _sut.ProcessTurnStart(combatant);

        // Assert
        damage.Should().Be(1); // Minimum 1 damage on hit
    }

    [Fact]
    public void ProcessTurnStart_NoDoTEffects_ReturnsZero()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 1, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Vulnerable, 2, Guid.NewGuid());

        // Act
        var damage = _sut.ProcessTurnStart(combatant);

        // Assert
        damage.Should().Be(0);
        _mockDice.Verify(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ProcessTurnStart_BleedingAndPoisoned_DealsCombinedDamage()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        combatant.ArmorSoak = 2;
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid()); // 1 stack
        _sut.ApplyEffect(combatant, StatusEffectType.Poisoned, 3, Guid.NewGuid()); // 1 stack

        _mockDice.SetupSequence(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4)  // Bleeding: 4 damage (ignores soak)
            .Returns(5); // Poisoned: 5 - 2 = 3 damage

        // Act
        var damage = _sut.ProcessTurnStart(combatant);

        // Assert
        damage.Should().Be(7); // 4 + 3 = 7
    }

    #endregion

    #region CanAct Tests

    [Fact]
    public void CanAct_WhenStunned_ReturnsFalse()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 1, Guid.NewGuid());

        // Act
        var canAct = _sut.CanAct(combatant);

        // Assert
        canAct.Should().BeFalse();
    }

    [Fact]
    public void CanAct_WhenNotStunned_ReturnsTrue()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Vulnerable, 2, Guid.NewGuid());

        // Act
        var canAct = _sut.CanAct(combatant);

        // Assert
        canAct.Should().BeTrue();
    }

    [Fact]
    public void CanAct_NoEffects_ReturnsTrue()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act
        var canAct = _sut.CanAct(combatant);

        // Assert
        canAct.Should().BeTrue();
    }

    #endregion

    #region ProcessTurnEnd Tests

    [Fact]
    public void ProcessTurnEnd_DecrementsAllDurations()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 2, Guid.NewGuid());

        // Act
        _sut.ProcessTurnEnd(combatant);

        // Assert
        combatant.StatusEffects.Should().HaveCount(2);
        combatant.StatusEffects.First(e => e.Type == StatusEffectType.Bleeding)
            .DurationRemaining.Should().Be(2);
        combatant.StatusEffects.First(e => e.Type == StatusEffectType.Stunned)
            .DurationRemaining.Should().Be(1);
    }

    [Fact]
    public void ProcessTurnEnd_RemovesExpiredEffects()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 1, Guid.NewGuid()); // Will expire
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid()); // Will remain

        // Act
        _sut.ProcessTurnEnd(combatant);

        // Assert
        combatant.StatusEffects.Should().HaveCount(1);
        combatant.StatusEffects.First().Type.Should().Be(StatusEffectType.Bleeding);
    }

    [Fact]
    public void ProcessTurnEnd_AllEffectsExpire_ListBecomesEmpty()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 1, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Vulnerable, 1, Guid.NewGuid());

        // Act
        _sut.ProcessTurnEnd(combatant);

        // Assert
        combatant.StatusEffects.Should().BeEmpty();
    }

    [Fact]
    public void ProcessTurnEnd_NoEffects_NoException()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act & Assert - should not throw
        _sut.ProcessTurnEnd(combatant);
        combatant.StatusEffects.Should().BeEmpty();
    }

    #endregion

    #region GetSoakModifier Tests

    [Fact]
    public void GetSoakModifier_NoFortified_ReturnsZero()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());

        // Act
        var modifier = _sut.GetSoakModifier(combatant);

        // Assert
        modifier.Should().Be(0);
    }

    [Fact]
    public void GetSoakModifier_Fortified_ReturnsStackBonus()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Fortified, 3, Guid.NewGuid()); // 1 stack

        // Act
        var modifier = _sut.GetSoakModifier(combatant);

        // Assert
        modifier.Should().Be(2); // +2 per stack
    }

    [Fact]
    public void GetSoakModifier_FortifiedMultiStack_ReturnsScaledBonus()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Fortified, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Fortified, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Fortified, 3, Guid.NewGuid()); // 3 stacks

        // Act
        var modifier = _sut.GetSoakModifier(combatant);

        // Assert
        modifier.Should().Be(6); // +2 × 3 = +6
    }

    #endregion

    #region GetDamageMultiplier Tests

    [Fact]
    public void GetDamageMultiplier_NoVulnerable_ReturnsOne()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());

        // Act
        var multiplier = _sut.GetDamageMultiplier(combatant);

        // Assert
        multiplier.Should().Be(1.0f);
    }

    [Fact]
    public void GetDamageMultiplier_Vulnerable_ReturnsOnePointFive()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Vulnerable, 2, Guid.NewGuid());

        // Act
        var multiplier = _sut.GetDamageMultiplier(combatant);

        // Assert
        multiplier.Should().Be(1.5f);
    }

    #endregion

    #region HasEffect and GetEffectStacks Tests

    [Fact]
    public void HasEffect_WithEffect_ReturnsTrue()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 1, Guid.NewGuid());

        // Act
        var hasStunned = _sut.HasEffect(combatant, StatusEffectType.Stunned);

        // Assert
        hasStunned.Should().BeTrue();
    }

    [Fact]
    public void HasEffect_WithoutEffect_ReturnsFalse()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act
        var hasStunned = _sut.HasEffect(combatant, StatusEffectType.Stunned);

        // Assert
        hasStunned.Should().BeFalse();
    }

    [Fact]
    public void GetEffectStacks_ExistingEffect_ReturnsStackCount()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());

        // Act
        var stacks = _sut.GetEffectStacks(combatant, StatusEffectType.Bleeding);

        // Assert
        stacks.Should().Be(3);
    }

    [Fact]
    public void GetEffectStacks_NoEffect_ReturnsZero()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act
        var stacks = _sut.GetEffectStacks(combatant, StatusEffectType.Bleeding);

        // Assert
        stacks.Should().Be(0);
    }

    [Fact]
    public void GetActiveEffects_ReturnsReadOnlyList()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 1, Guid.NewGuid());

        // Act
        var effects = _sut.GetActiveEffects(combatant);

        // Assert
        effects.Should().HaveCount(2);
        effects.Should().BeAssignableTo<IReadOnlyList<ActiveStatusEffect>>();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_StunLoop_ProcessesCorrectly()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        _sut.ApplyEffect(combatant, StatusEffectType.Stunned, 2, Guid.NewGuid());

        // Round 1: Stunned, cannot act
        _sut.CanAct(combatant).Should().BeFalse();
        _sut.ProcessTurnEnd(combatant);
        combatant.StatusEffects.First().DurationRemaining.Should().Be(1);

        // Round 2: Still stunned
        _sut.CanAct(combatant).Should().BeFalse();
        _sut.ProcessTurnEnd(combatant);

        // Round 3: Stun expired, can act
        combatant.StatusEffects.Should().BeEmpty();
        _sut.CanAct(combatant).Should().BeTrue();
    }

    [Fact]
    public void Integration_BleedDeath_AccumulatesDamage()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        combatant.CurrentHp = 20;

        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 5, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 5, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Bleeding, 5, Guid.NewGuid()); // 3 stacks

        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(4);

        // Act - simulate 2 turns
        var damage1 = _sut.ProcessTurnStart(combatant);
        combatant.CurrentHp -= damage1;
        _sut.ProcessTurnEnd(combatant);

        var damage2 = _sut.ProcessTurnStart(combatant);
        combatant.CurrentHp -= damage2;

        // Assert
        damage1.Should().Be(12); // 4 × 3 stacks
        damage2.Should().Be(12);
        combatant.CurrentHp.Should().Be(-4); // 20 - 12 - 12 = -4 (dead)
    }

    [Fact]
    public void Integration_FortifiedPoisoned_SoakApplies()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        combatant.ArmorSoak = 2;

        _sut.ApplyEffect(combatant, StatusEffectType.Fortified, 3, Guid.NewGuid());
        _sut.ApplyEffect(combatant, StatusEffectType.Fortified, 3, Guid.NewGuid()); // +4 soak
        _sut.ApplyEffect(combatant, StatusEffectType.Poisoned, 3, Guid.NewGuid());

        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(6);

        // Act
        var damage = _sut.ProcessTurnStart(combatant);

        // Assert
        // Poison deals 6, soak = ArmorSoak(2) + Fortified(4) = 6
        // 6 - 6 = 0, but minimum 1 damage
        damage.Should().Be(1);
    }

    #endregion

    #region Helper Methods

    private static Combatant CreateTestCombatant(string name = "TestCombatant")
    {
        return new Combatant
        {
            Id = Guid.NewGuid(),
            Name = name,
            CurrentHp = 100,
            MaxHp = 100,
            CurrentStamina = 100,
            MaxStamina = 100,
            ArmorSoak = 0
        };
    }

    #endregion
}
