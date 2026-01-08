using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class StatusEffectTests
{
    [Test]
    public void StatusEffectDefinition_Create_ShouldNormalizeId()
    {
        // Arrange & Act
        var definition = StatusEffectDefinition.Create(
            "BLEEDING",
            "Bleeding",
            "Taking damage over time",
            EffectCategory.Debuff,
            DurationType.Turns,
            baseDuration: 3);

        // Assert
        definition.Id.Should().Be("bleeding");
        definition.Name.Should().Be("Bleeding");
    }

    [Test]
    public void StatusEffectDefinition_WithDamageOverTime_ShouldSetProperties()
    {
        // Arrange
        var definition = StatusEffectDefinition.Create(
            "burning",
            "Burning",
            "On fire",
            EffectCategory.Debuff,
            DurationType.Turns,
            baseDuration: 2);

        // Act
        definition.WithDamageOverTime(4, "fire");

        // Assert
        definition.DamagePerTurn.Should().Be(4);
        definition.DamageType.Should().Be("fire");
    }

    [Test]
    public void StatusEffectDefinition_WithStatModifier_ShouldAddModifier()
    {
        // Arrange
        var definition = StatusEffectDefinition.Create(
            "weakened",
            "Weakened",
            "Reduced strength",
            EffectCategory.Debuff,
            DurationType.Turns,
            baseDuration: 3);

        // Act
        definition.WithStatModifier(StatModifier.Percentage("attack", -0.25f));

        // Assert
        definition.StatModifiers.Should().HaveCount(1);
        definition.StatModifiers[0].StatId.Should().Be("attack");
        definition.StatModifiers[0].Value.Should().Be(-0.25f);
    }

    [Test]
    public void ActiveStatusEffect_Create_ShouldSetInitialDuration()
    {
        // Arrange
        var definition = StatusEffectDefinition.Create(
            "stunned",
            "Stunned",
            "Cannot act",
            EffectCategory.Debuff,
            DurationType.Turns,
            baseDuration: 2);

        // Act
        var active = ActiveStatusEffect.Create(definition);

        // Assert
        active.RemainingDuration.Should().Be(2);
        active.IsActive.Should().BeTrue();
        active.Stacks.Should().Be(1);
    }

    [Test]
    public void ActiveStatusEffect_TickDuration_ShouldDecrementAndExpire()
    {
        // Arrange
        var definition = StatusEffectDefinition.Create(
            "test",
            "Test",
            "Test effect",
            EffectCategory.Debuff,
            DurationType.Turns,
            baseDuration: 1);
        var active = ActiveStatusEffect.Create(definition);

        // Act
        var stillActive = active.TickDuration();

        // Assert
        stillActive.Should().BeFalse();
        active.RemainingDuration.Should().Be(0);
        active.IsExpired.Should().BeTrue();
    }

    [Test]
    public void ActiveStatusEffect_RefreshDuration_ShouldResetToMax()
    {
        // Arrange
        var definition = StatusEffectDefinition.Create(
            "test",
            "Test",
            "Test effect",
            EffectCategory.Debuff,
            DurationType.Turns,
            baseDuration: 3);
        var active = ActiveStatusEffect.Create(definition);
        active.TickDuration();
        active.TickDuration();

        // Act
        active.RefreshDuration();

        // Assert
        active.RemainingDuration.Should().Be(3);
    }

    [Test]
    public void ActiveStatusEffect_AddStacks_ShouldRespectMaxStacks()
    {
        // Arrange
        var definition = StatusEffectDefinition.Create(
            "test",
            "Test",
            "Test effect",
            EffectCategory.Debuff,
            DurationType.Turns,
            baseDuration: 3,
            stackingRule: StackingRule.Stack,
            maxStacks: 3);
        var active = ActiveStatusEffect.Create(definition);

        // Act
        active.AddStacks(1);
        active.AddStacks(1);
        var addedBeyondMax = active.AddStacks(1);

        // Assert
        active.Stacks.Should().Be(3);
        addedBeyondMax.Should().BeFalse();
    }

    [Test]
    public void ActiveStatusEffect_CalculateDamagePerTurn_ShouldScaleWithStacks()
    {
        // Arrange
        var definition = StatusEffectDefinition.Create(
            "bleeding",
            "Bleeding",
            "DoT",
            EffectCategory.Debuff,
            DurationType.Turns,
            baseDuration: 3,
            stackingRule: StackingRule.Stack,
            maxStacks: 5);
        definition.WithDamageOverTime(3, "physical");
        var active = ActiveStatusEffect.Create(definition);
        active.AddStacks(2);

        // Act
        var damage = active.CalculateDamagePerTurn();

        // Assert
        damage.Should().Be(9); // 3 damage × 3 stacks
    }
}
