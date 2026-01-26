using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="PerceptionAbility"/> related value objects.
/// </summary>
[TestFixture]
public class PerceptionAbilityTests
{
    [Test]
    public void AbilityCondition_Always_SetsAlwaysActive()
    {
        // Arrange & Act
        var condition = AbilityCondition.Always();

        // Assert
        condition.Type.Should().Be(ConditionType.Always);
        condition.AlwaysActive.Should().BeTrue();
    }

    [Test]
    public void AbilityCondition_ForCategories_SetsObjectCategories()
    {
        // Act
        var condition = AbilityCondition.ForCategories("Machinery", "Terminal");

        // Assert
        condition.Type.Should().Be(ConditionType.ObjectCategory);
        condition.ObjectCategories.Should().Contain("Machinery");
        condition.ObjectCategories.Should().Contain("Terminal");
    }

    [Test]
    public void PerceptionAbilityEffect_DiceBonusEffect_SetsCorrectValues()
    {
        // Act
        var effect = PerceptionAbilityEffect.DiceBonusEffect(2);

        // Assert
        effect.Type.Should().Be(PerceptionEffectType.DiceBonus);
        effect.BonusDice.Should().Be(2);
    }

    [Test]
    public void PerceptionAbilityEffect_AutoSuccessEffect_SetsLayer()
    {
        // Act
        var effect = PerceptionAbilityEffect.AutoSuccessEffect(3);

        // Assert
        effect.Type.Should().Be(PerceptionEffectType.AutoSuccessLayer);
        effect.AutoSuccessLayer.Should().Be(3);
    }

    [Test]
    public void PerceptionAbility_Create_SetsAllProperties()
    {
        // Arrange
        var condition = AbilityCondition.ForCategories("Machinery");
        var effect = PerceptionAbilityEffect.DiceBonusEffect(2);

        // Act
        var ability = PerceptionAbility.Create(
            "deep-scan",
            "[Deep Scan]",
            "jotun-reader",
            PerceptionAbilityType.Examination,
            "+2d10 to examine machinery/terminals",
            condition,
            effect);

        // Assert
        ability.AbilityId.Should().Be("deep-scan");
        ability.AbilityName.Should().Be("[Deep Scan]");
        ability.SpecializationId.Should().Be("jotun-reader");
        ability.AbilityType.Should().Be(PerceptionAbilityType.Examination);
    }

    [Test]
    public void PerceptionAbilityActivation_Create_RecordsActivation()
    {
        // Arrange
        var condition = AbilityCondition.Always();
        var effect = PerceptionAbilityEffect.PassiveBonusEffect(1);
        var ability = PerceptionAbility.Create(
            "keen-senses", "[Keen Senses]", "vei-madr",
            PerceptionAbilityType.PassivePerception,
            "+1 to passive perception",
            condition, effect);

        // Act
        var activation = PerceptionAbilityActivation.Create(
            ability, "player-1", "room-1", "+1 passive perception");

        // Assert
        activation.AbilityId.Should().Be("keen-senses");
        activation.CharacterId.Should().Be("player-1");
        activation.TargetId.Should().Be("room-1");
    }

    [Test]
    public void InvestigationModifiers_None_HasNoModifiers()
    {
        // Act
        var mods = InvestigationModifiers.None;

        // Assert
        mods.HasModifiers.Should().BeFalse();
        mods.TotalBonus.Should().Be(0);
    }

    [Test]
    public void InvestigationModifiers_WithDiceBonus_HasModifiers()
    {
        // Arrange
        var ability = PerceptionAbility.Create(
            "read-signs", "[Read the Signs]", "vei-madr",
            PerceptionAbilityType.Investigation,
            "Bonus to investigate creature remains",
            AbilityCondition.Always(),
            PerceptionAbilityEffect.DiceBonusEffect(2));

        // Act
        var mods = InvestigationModifiers.WithDiceBonus(2, ability);

        // Assert
        mods.HasModifiers.Should().BeTrue();
        mods.BonusDice.Should().Be(2);
        mods.HasContributingAbilities.Should().BeTrue();
    }
}
