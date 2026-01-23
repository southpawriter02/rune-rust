namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="SpecializationSkillBonus"/> value object.
/// </summary>
[TestFixture]
public class SpecializationSkillBonusTests
{
    [Test]
    public void None_CreatesZeroEffectBonus()
    {
        // Act
        var bonus = SpecializationSkillBonus.None("test-spec", "test-skill");

        // Assert
        bonus.SpecializationId.Should().Be("test-spec");
        bonus.SkillId.Should().Be("test-skill");
        bonus.DiceBonus.Should().Be(0);
        bonus.HasEffect.Should().BeFalse();
        bonus.ShouldApply.Should().BeFalse();
    }

    [Test]
    public void HasEffect_TrueWhenDiceBonusNonZero()
    {
        // Arrange
        var bonus = new SpecializationSkillBonus(
            SpecializationId: "gantry-runner",
            SkillId: "climbing",
            DiceBonus: 2,
            Description: "Born to the Heights");

        // Assert
        bonus.HasEffect.Should().BeTrue();
        bonus.ShouldApply.Should().BeTrue();
    }

    [Test]
    public void HasEffect_TrueWhenSpecialAbilitySet()
    {
        // Arrange
        var bonus = new SpecializationSkillBonus(
            SpecializationId: "thul",
            SkillId: "persuasion",
            DiceBonus: 0,
            SpecialAbility: "no-reputation-loss-on-failure");

        // Assert
        bonus.HasEffect.Should().BeTrue();
        bonus.HasSpecialAbility.Should().BeTrue();
    }

    [Test]
    public void ConditionalBonus_ShouldApply_WhenConditionMet()
    {
        // Arrange
        var bonus = new SpecializationSkillBonus(
            SpecializationId: "myr-stalker",
            SkillId: "navigation",
            DiceBonus: 2,
            IsConditional: true,
            ConditionMet: true,
            Description: "Mire Walker");

        // Assert
        bonus.ShouldApply.Should().BeTrue();
    }

    [Test]
    public void ConditionalBonus_ShouldNotApply_WhenConditionNotMet()
    {
        // Arrange
        var bonus = new SpecializationSkillBonus(
            SpecializationId: "myr-stalker",
            SkillId: "navigation",
            DiceBonus: 2,
            IsConditional: true,
            ConditionMet: false,
            Description: "Mire Walker");

        // Assert
        bonus.ShouldApply.Should().BeFalse();
    }

    [Test]
    public void WithConditionResult_ReturnsUpdatedBonus()
    {
        // Arrange
        var bonus = new SpecializationSkillBonus(
            SpecializationId: "myr-stalker",
            SkillId: "navigation",
            DiceBonus: 2,
            IsConditional: true,
            ConditionMet: true);

        // Act
        var updated = bonus.WithConditionResult(false);

        // Assert
        updated.ConditionMet.Should().BeFalse();
        updated.ShouldApply.Should().BeFalse();
    }

    [Test]
    public void ToDisplayString_FormatsPositiveBonus()
    {
        // Arrange
        var bonus = new SpecializationSkillBonus(
            SpecializationId: "gantry-runner",
            SkillId: "climbing",
            DiceBonus: 2,
            Description: "Born to the Heights");

        // Act
        var display = bonus.ToDisplayString();

        // Assert
        display.Should().Contain("+2d10");
        display.Should().Contain("Born to the Heights");
    }

    [Test]
    public void ToDisplayString_ShowsConditionNotMet()
    {
        // Arrange
        var bonus = new SpecializationSkillBonus(
            SpecializationId: "myr-stalker",
            SkillId: "navigation",
            DiceBonus: 2,
            IsConditional: true,
            ConditionMet: false,
            Description: "Mire Walker");

        // Act
        var display = bonus.ToDisplayString();

        // Assert
        display.Should().Contain("condition not met");
    }
}
