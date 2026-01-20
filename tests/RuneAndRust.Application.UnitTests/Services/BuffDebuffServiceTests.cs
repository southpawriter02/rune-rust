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
/// Unit tests for BuffDebuffService.
/// </summary>
[TestFixture]
public class BuffDebuffServiceTests
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
    // APPLY EFFECT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ApplyEffect_WithValidEffect_ReturnsSuccess()
    {
        // Arrange
        var definition = CreateTestDefinition("regenerating", EffectCategory.Buff);
        var target = CreateMockTarget();
        _mockRepository.Setup(r => r.GetById("regenerating")).Returns(definition);

        // Act
        var result = _service.ApplyEffect(target.Object, "regenerating");

        // Assert
        result.WasApplied.Should().BeTrue();
        result.ResultType.Should().Be(ApplyResultType.Applied);
        result.ActiveEffect.Should().NotBeNull();
        target.Verify(t => t.AddEffect(It.IsAny<ActiveStatusEffect>()), Times.Once);
    }

    [Test]
    public void ApplyEffect_WithUnknownEffect_ReturnsFailed()
    {
        // Arrange
        var target = CreateMockTarget();
        _mockRepository.Setup(r => r.GetById("unknown")).Returns((StatusEffectDefinition?)null);

        // Act
        var result = _service.ApplyEffect(target.Object, "unknown");

        // Assert
        result.WasApplied.Should().BeFalse();
        result.ResultType.Should().Be(ApplyResultType.Failed);
        result.Message.Should().Contain("unknown");
    }

    [Test]
    public void ApplyEffect_WhenImmune_ReturnsImmune()
    {
        // Arrange
        var definition = CreateTestDefinition("stunned", EffectCategory.Debuff);
        var target = CreateMockTarget();
        target.Setup(t => t.IsImmuneToEffect("stunned")).Returns(true);
        _mockRepository.Setup(r => r.GetById("stunned")).Returns(definition);

        // Act
        var result = _service.ApplyEffect(target.Object, "stunned");

        // Assert
        result.WasApplied.Should().BeFalse();
        result.ResultType.Should().Be(ApplyResultType.Immune);
    }

    // ═══════════════════════════════════════════════════════════════
    // STACKING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ApplyEffect_WithRefreshDurationRule_RefreshesDuration()
    {
        // Arrange
        var definition = CreateTestDefinition("bleeding", EffectCategory.Debuff, StackingRule.RefreshDuration);
        var existingEffect = CreateMockActiveEffect(definition);
        var target = CreateMockTarget(existingEffect);
        _mockRepository.Setup(r => r.GetById("bleeding")).Returns(definition);

        // Act
        var result = _service.ApplyEffect(target.Object, "bleeding");

        // Assert
        result.WasApplied.Should().BeTrue();
        result.ResultType.Should().Be(ApplyResultType.Refreshed);
    }

    [Test]
    public void ApplyEffect_WithStackRule_AddsStacks()
    {
        // Arrange
        var definition = CreateTestDefinition("poison", EffectCategory.Debuff, StackingRule.Stack, maxStacks: 3);
        var existingEffect = CreateMockActiveEffect(definition, stacks: 1);
        var target = CreateMockTarget(existingEffect);
        _mockRepository.Setup(r => r.GetById("poison")).Returns(definition);

        // Act
        var result = _service.ApplyEffect(target.Object, "poison");

        // Assert
        result.WasApplied.Should().BeTrue();
        result.ResultType.Should().Be(ApplyResultType.Stacked);
    }

    [Test]
    public void ApplyEffect_WithStackRuleAtMax_ReturnsAtMaxStacks()
    {
        // Arrange
        var definition = CreateTestDefinition("poison", EffectCategory.Debuff, StackingRule.Stack, maxStacks: 3);
        var existingEffect = CreateMockActiveEffect(definition, stacks: 3);
        var target = CreateMockTarget(existingEffect);
        _mockRepository.Setup(r => r.GetById("poison")).Returns(definition);

        // Act
        var result = _service.ApplyEffect(target.Object, "poison");

        // Assert
        result.WasApplied.Should().BeTrue();
        result.ResultType.Should().Be(ApplyResultType.AtMaxStacks);
    }

    [Test]
    public void ApplyEffect_WithBlockRule_ReturnsBlocked()
    {
        // Arrange
        var definition = CreateTestDefinition("stunned", EffectCategory.Debuff, StackingRule.Block);
        var existingEffect = CreateMockActiveEffect(definition);
        var target = CreateMockTarget(existingEffect);
        _mockRepository.Setup(r => r.GetById("stunned")).Returns(definition);

        // Act
        var result = _service.ApplyEffect(target.Object, "stunned");

        // Assert
        result.WasApplied.Should().BeFalse();
        result.ResultType.Should().Be(ApplyResultType.Blocked);
    }

    // ═══════════════════════════════════════════════════════════════
    // REMOVE EFFECT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RemoveEffect_WhenEffectExists_ReturnsTrue()
    {
        // Arrange
        var target = CreateMockTarget();
        target.Setup(t => t.RemoveEffectsByDefinition("bleeding")).Returns(1);

        // Act
        var result = _service.RemoveEffect(target.Object, "bleeding");

        // Assert
        result.Should().BeTrue();
        target.Verify(t => t.RemoveEffectsByDefinition("bleeding"), Times.Once);
    }

    [Test]
    public void RemoveEffect_WhenEffectNotPresent_ReturnsFalse()
    {
        // Arrange
        var target = CreateMockTarget();
        target.Setup(t => t.RemoveEffectsByDefinition("unknown")).Returns(0);

        // Act
        var result = _service.RemoveEffect(target.Object, "unknown");

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR EFFECTS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ClearDebuffs_RemovesOnlyDebuffs()
    {
        // Arrange
        var debuffDef = CreateTestDefinition("poisoned", EffectCategory.Debuff);
        var buffDef = CreateTestDefinition("hasted", EffectCategory.Buff);
        var debuffEffect = CreateMockActiveEffect(debuffDef);
        var buffEffect = CreateMockActiveEffect(buffDef);

        var effects = new List<ActiveStatusEffect> { debuffEffect, buffEffect };
        var target = new Mock<IEffectTarget>();
        target.Setup(t => t.Name).Returns("TestTarget");
        target.Setup(t => t.ActiveEffects).Returns(effects.AsReadOnly());
        target.Setup(t => t.RemoveEffect(debuffEffect.Id)).Returns(true);
        target.Setup(t => t.RemoveEffect(buffEffect.Id)).Returns(true);

        // Act
        var result = _service.ClearDebuffs(target.Object);

        // Assert
        result.Should().Be(1);
        target.Verify(t => t.RemoveEffect(debuffEffect.Id), Times.Once);
        target.Verify(t => t.RemoveEffect(buffEffect.Id), Times.Never);
    }

    [Test]
    public void ClearBuffs_RemovesOnlyBuffs()
    {
        // Arrange
        var debuffDef = CreateTestDefinition("poisoned", EffectCategory.Debuff);
        var buffDef = CreateTestDefinition("hasted", EffectCategory.Buff);
        var debuffEffect = CreateMockActiveEffect(debuffDef);
        var buffEffect = CreateMockActiveEffect(buffDef);

        var effects = new List<ActiveStatusEffect> { debuffEffect, buffEffect };
        var target = new Mock<IEffectTarget>();
        target.Setup(t => t.Name).Returns("TestTarget");
        target.Setup(t => t.ActiveEffects).Returns(effects.AsReadOnly());
        target.Setup(t => t.RemoveEffect(debuffEffect.Id)).Returns(true);
        target.Setup(t => t.RemoveEffect(buffEffect.Id)).Returns(true);

        // Act
        var result = _service.ClearBuffs(target.Object);

        // Assert
        result.Should().Be(1);
        target.Verify(t => t.RemoveEffect(buffEffect.Id), Times.Once);
        target.Verify(t => t.RemoveEffect(debuffEffect.Id), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void HasEffect_WhenPresent_ReturnsTrue()
    {
        // Arrange
        var target = CreateMockTarget();
        target.Setup(t => t.HasEffect("bleeding")).Returns(true);

        // Act
        var result = _service.HasEffect(target.Object, "bleeding");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void GetStackCount_ReturnsStackCount()
    {
        // Arrange
        var definition = CreateTestDefinition("poison", EffectCategory.Debuff, maxStacks: 5);
        var effect = CreateMockActiveEffect(definition, stacks: 3);
        var target = CreateMockTarget();
        target.Setup(t => t.GetEffect("poison")).Returns(effect);

        // Act
        var result = _service.GetStackCount(target.Object, "poison");

        // Assert
        result.Should().Be(3);
    }

    [Test]
    public void GetRemainingDuration_ReturnsDuration()
    {
        // Arrange
        var definition = CreateTestDefinition("stunned", EffectCategory.Debuff, baseDuration: 5);
        var effect = CreateMockActiveEffect(definition);  // Duration comes from definition
        var target = CreateMockTarget();
        target.Setup(t => t.GetEffect("stunned")).Returns(effect);

        // Act
        var result = _service.GetRemainingDuration(target.Object, "stunned");

        // Assert
        result.Should().Be(5);  // Duration is the definition's BaseDuration
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private static StatusEffectDefinition CreateTestDefinition(
        string id,
        EffectCategory category,
        StackingRule stackingRule = StackingRule.RefreshDuration,
        int baseDuration = 3,
        int maxStacks = 1)
    {
        return StatusEffectDefinition.Create(id, id.ToUpperInvariant(), $"Test {id}", category, DurationType.Turns, baseDuration, stackingRule, maxStacks);
    }

    private static ActiveStatusEffect CreateMockActiveEffect(
        StatusEffectDefinition definition,
        int stacks = 1,
        int duration = 3)
    {
        var effect = ActiveStatusEffect.Create(definition);
        // Set stacks through reflection or use a factory that allows setting
        for (int i = 1; i < stacks; i++)
        {
            effect.AddStacks(1);
        }
        return effect;
    }

    private static Mock<IEffectTarget> CreateMockTarget(ActiveStatusEffect? existingEffect = null)
    {
        var mockTarget = new Mock<IEffectTarget>();
        mockTarget.Setup(t => t.Id).Returns(Guid.NewGuid());
        mockTarget.Setup(t => t.Name).Returns("TestTarget");
        mockTarget.Setup(t => t.IsImmuneToEffect(It.IsAny<string>())).Returns(false);

        if (existingEffect is not null)
        {
            mockTarget.Setup(t => t.GetEffect(existingEffect.Definition.Id)).Returns(existingEffect);
            mockTarget.Setup(t => t.ActiveEffects).Returns(new List<ActiveStatusEffect> { existingEffect }.AsReadOnly());
        }
        else
        {
            mockTarget.Setup(t => t.GetEffect(It.IsAny<string>())).Returns((ActiveStatusEffect?)null);
            mockTarget.Setup(t => t.ActiveEffects).Returns(new List<ActiveStatusEffect>().AsReadOnly());
        }

        return mockTarget;
    }
}
