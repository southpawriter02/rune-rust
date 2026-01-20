using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for v0.10.0c Effect Triggers & Cleanse features.
/// </summary>
[TestFixture]
public class BuffDebuffServiceTickTests
{
    private Mock<IStatusEffectRepository> _mockRepository = null!;
    private Mock<ILogger<BuffDebuffService>> _mockLogger = null!;
    private BuffDebuffService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IStatusEffectRepository>();
        _mockLogger = new Mock<ILogger<BuffDebuffService>>();
        _service = new BuffDebuffService(_mockRepository.Object, _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // TICK EFFECTS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void TickEffects_WithDoTEffect_DealsDamage()
    {
        // Arrange
        var definition = CreateDoTDefinition("poisoned", damagePerTurn: 5);
        var effect = ActiveStatusEffect.Create(definition);
        var target = CreateMockTarget(effect, health: 50);

        // Act
        var result = _service.TickEffects(target.Object, TriggerTiming.OnTurnStart);

        // Assert
        result.DamageDealt.Should().Be(5);
        result.TriggeredEffects.Should().Contain("poisoned");
        target.Verify(t => t.TakeDamage(5), Times.Once);
    }

    [Test]
    public void TickEffects_WithHoTEffect_Heals()
    {
        // Arrange
        var definition = CreateHoTDefinition("regenerating", healingPerTurn: 10);
        var effect = ActiveStatusEffect.Create(definition);
        var target = CreateMockTarget(effect, health: 50);

        // Act
        var result = _service.TickEffects(target.Object, TriggerTiming.OnTurnStart);

        // Assert
        result.HealingDone.Should().Be(10);
        result.TriggeredEffects.Should().Contain("regenerating");
        target.Verify(t => t.Heal(10), Times.Once);
    }

    [Test]
    public void TickEffects_DecrementsDurationOnTurnStart()
    {
        // Arrange
        var definition = CreateTestDefinition("stunned", EffectCategory.Debuff, baseDuration: 2);
        var effect = ActiveStatusEffect.Create(definition);
        var target = CreateMockTarget(effect);

        // Act
        var result = _service.TickEffects(target.Object, TriggerTiming.OnTurnStart);

        // Assert
        effect.RemainingDuration.Should().Be(1);
        result.ExpiredEffects.Should().BeEmpty();
    }

    [Test]
    public void TickEffects_RemovesExpiredEffects()
    {
        // Arrange
        var definition = CreateTestDefinition("stunned", EffectCategory.Debuff, baseDuration: 1);
        var effect = ActiveStatusEffect.Create(definition);
        var target = CreateMockTarget(effect);

        // Act
        var result = _service.TickEffects(target.Object, TriggerTiming.OnTurnStart);

        // Assert
        result.ExpiredEffects.Should().Contain("stunned");
        target.Verify(t => t.RemoveEffect(effect.Id), Times.Once);
    }

    [Test]
    public void TickEffects_OnTurnEnd_DoesNotDecrementDuration()
    {
        // Arrange
        var definition = CreateTestDefinition("blessed", EffectCategory.Buff, baseDuration: 3);
        var effect = ActiveStatusEffect.Create(definition);
        var target = CreateMockTarget(effect);

        // Act
        var result = _service.TickEffects(target.Object, TriggerTiming.OnTurnEnd);

        // Assert
        effect.RemainingDuration.Should().Be(3);
        result.ExpiredEffects.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // IMMUNITY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsImmune_WithInnateImmunity_ReturnsTrue()
    {
        // Arrange
        var target = new Mock<IEffectTarget>();
        target.Setup(t => t.Name).Returns("TestTarget");
        target.Setup(t => t.IsImmuneToEffect("stunned")).Returns(true);
        target.Setup(t => t.ActiveEffects).Returns(new List<ActiveStatusEffect>().AsReadOnly());

        // Act
        var result = _service.IsImmune(target.Object, "stunned");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsImmune_WithoutImmunity_ReturnsFalse()
    {
        // Arrange
        var target = new Mock<IEffectTarget>();
        target.Setup(t => t.Name).Returns("TestTarget");
        target.Setup(t => t.IsImmuneToEffect(It.IsAny<string>())).Returns(false);
        target.Setup(t => t.ActiveEffects).Returns(new List<ActiveStatusEffect>().AsReadOnly());

        // Act
        var result = _service.IsImmune(target.Object, "stunned");

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEANSE / DISPEL TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Cleanse_RemovesOnlyDebuffs()
    {
        // Arrange
        var debuffDef = CreateTestDefinition("poisoned", EffectCategory.Debuff);
        var buffDef = CreateTestDefinition("blessed", EffectCategory.Buff);
        var debuffEffect = ActiveStatusEffect.Create(debuffDef);
        var buffEffect = ActiveStatusEffect.Create(buffDef);

        var effects = new List<ActiveStatusEffect> { debuffEffect, buffEffect };
        var target = new Mock<IEffectTarget>();
        target.Setup(t => t.Name).Returns("TestTarget");
        target.Setup(t => t.ActiveEffects).Returns(effects.AsReadOnly());
        target.Setup(t => t.RemoveEffect(debuffEffect.Id)).Returns(true);
        target.Setup(t => t.RemoveEffect(buffEffect.Id)).Returns(true);

        // Act
        var result = _service.Cleanse(target.Object);

        // Assert
        result.Should().Be(1);
        target.Verify(t => t.RemoveEffect(debuffEffect.Id), Times.Once);
        target.Verify(t => t.RemoveEffect(buffEffect.Id), Times.Never);
    }

    [Test]
    public void Cleanse_WithCount_RemovesLimitedDebuffs()
    {
        // Arrange
        var debuff1 = ActiveStatusEffect.Create(CreateTestDefinition("poison1", EffectCategory.Debuff));
        var debuff2 = ActiveStatusEffect.Create(CreateTestDefinition("poison2", EffectCategory.Debuff));
        var debuff3 = ActiveStatusEffect.Create(CreateTestDefinition("poison3", EffectCategory.Debuff));

        var effects = new List<ActiveStatusEffect> { debuff1, debuff2, debuff3 };
        var target = new Mock<IEffectTarget>();
        target.Setup(t => t.Name).Returns("TestTarget");
        target.Setup(t => t.ActiveEffects).Returns(effects.AsReadOnly());
        target.Setup(t => t.RemoveEffect(It.IsAny<Guid>())).Returns(true);

        // Act
        var result = _service.Cleanse(target.Object, count: 2);

        // Assert
        result.Should().Be(2);
    }

    [Test]
    public void Dispel_RemovesOnlyBuffs()
    {
        // Arrange
        var debuffDef = CreateTestDefinition("poisoned", EffectCategory.Debuff);
        var buffDef = CreateTestDefinition("blessed", EffectCategory.Buff);
        var debuffEffect = ActiveStatusEffect.Create(debuffDef);
        var buffEffect = ActiveStatusEffect.Create(buffDef);

        var effects = new List<ActiveStatusEffect> { debuffEffect, buffEffect };
        var target = new Mock<IEffectTarget>();
        target.Setup(t => t.Name).Returns("TestTarget");
        target.Setup(t => t.ActiveEffects).Returns(effects.AsReadOnly());
        target.Setup(t => t.RemoveEffect(debuffEffect.Id)).Returns(true);
        target.Setup(t => t.RemoveEffect(buffEffect.Id)).Returns(true);

        // Act
        var result = _service.Dispel(target.Object);

        // Assert
        result.Should().Be(1);
        target.Verify(t => t.RemoveEffect(buffEffect.Id), Times.Once);
        target.Verify(t => t.RemoveEffect(debuffEffect.Id), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private static StatusEffectDefinition CreateTestDefinition(
        string id,
        EffectCategory category,
        int baseDuration = 3)
    {
        return StatusEffectDefinition.Create(id, id.ToUpperInvariant(), $"Test {id}", category, DurationType.Turns, baseDuration, StackingRule.RefreshDuration, 1);
    }

    private static StatusEffectDefinition CreateDoTDefinition(string id, int damagePerTurn)
    {
        return StatusEffectDefinition.Create(id, id.ToUpperInvariant(), $"Test {id}", EffectCategory.Debuff, DurationType.Turns, 3, StackingRule.Stack, 3)
            .WithDamageOverTime(damagePerTurn, "poison");
    }

    private static StatusEffectDefinition CreateHoTDefinition(string id, int healingPerTurn)
    {
        return StatusEffectDefinition.Create(id, id.ToUpperInvariant(), $"Test {id}", EffectCategory.Buff, DurationType.Turns, 3, StackingRule.RefreshDuration, 1)
            .WithHealingOverTime(healingPerTurn);
    }

    private static Mock<IEffectTarget> CreateMockTarget(ActiveStatusEffect? existingEffect = null, int health = 100)
    {
        var mockTarget = new Mock<IEffectTarget>();
        mockTarget.Setup(t => t.Id).Returns(Guid.NewGuid());
        mockTarget.Setup(t => t.Name).Returns("TestTarget");
        mockTarget.Setup(t => t.Health).Returns(health);
        mockTarget.Setup(t => t.MaxHealth).Returns(100);
        mockTarget.Setup(t => t.IsImmuneToEffect(It.IsAny<string>())).Returns(false);

        if (existingEffect is not null)
        {
            mockTarget.Setup(t => t.ActiveEffects).Returns(new List<ActiveStatusEffect> { existingEffect }.AsReadOnly());
        }
        else
        {
            mockTarget.Setup(t => t.ActiveEffects).Returns(new List<ActiveStatusEffect>().AsReadOnly());
        }

        return mockTarget;
    }
}
