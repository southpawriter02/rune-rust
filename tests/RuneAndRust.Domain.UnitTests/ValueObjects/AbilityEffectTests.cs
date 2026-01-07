using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class AbilityEffectTests
{
    [Test]
    public void Damage_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = AbilityEffect.Damage(25, scalingStat: "attack", scalingMultiplier: 0.5f);

        // Assert
        effect.EffectType.Should().Be(AbilityEffectType.Damage);
        effect.Value.Should().Be(25);
        effect.Duration.Should().Be(0);
        effect.ScalingStat.Should().Be("attack");
        effect.ScalingMultiplier.Should().Be(0.5f);
        effect.IsInstant.Should().BeTrue();
    }

    [Test]
    public void Heal_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = AbilityEffect.Heal(30, scalingStat: "will", scalingMultiplier: 0.6f);

        // Assert
        effect.EffectType.Should().Be(AbilityEffectType.Heal);
        effect.Value.Should().Be(30);
        effect.IsInstant.Should().BeTrue();
        effect.HasScaling.Should().BeTrue();
    }

    [Test]
    public void DamageOverTime_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = AbilityEffect.DamageOverTime(valuePerTurn: 5, duration: 3, statusEffect: "poison");

        // Assert
        effect.EffectType.Should().Be(AbilityEffectType.DamageOverTime);
        effect.Value.Should().Be(5);
        effect.Duration.Should().Be(3);
        effect.StatusEffect.Should().Be("poison");
        effect.IsInstant.Should().BeFalse();
    }

    [Test]
    public void Buff_CreatesCorrectEffect()
    {
        // Arrange
        var modifiers = new StatModifiers { Defense = 5 };

        // Act
        var effect = AbilityEffect.Buff(modifiers, duration: 3, description: "+5 Defense");

        // Assert
        effect.EffectType.Should().Be(AbilityEffectType.Buff);
        effect.Duration.Should().Be(3);
        effect.StatModifier.Should().Be(modifiers);
        effect.Description.Should().Be("+5 Defense");
        effect.IsInstant.Should().BeFalse();
    }

    [Test]
    public void IsInstant_WhenDurationZero_ReturnsTrue()
    {
        // Arrange
        var effect = AbilityEffect.Damage(20);

        // Assert
        effect.Duration.Should().Be(0);
        effect.IsInstant.Should().BeTrue();
    }

    [Test]
    public void HasScaling_WhenScalingStatSet_ReturnsTrue()
    {
        // Arrange
        var effectWithScaling = AbilityEffect.Damage(10, scalingStat: "attack", scalingMultiplier: 0.5f);
        var effectWithoutScaling = AbilityEffect.Damage(10);

        // Assert
        effectWithScaling.HasScaling.Should().BeTrue();
        effectWithoutScaling.HasScaling.Should().BeFalse();
    }
}
