using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class CombatIntegrationValueObjectTests
{
    [Test]
    public void EffectInteraction_HasBonusDamage_WhenPercentGreaterThanZero()
    {
        // Arrange
        var interaction = new EffectInteraction(
            "wet-lightning", "wet", "lightning",
            50, "lightning", "stunned", null,
            "Electricity arcs through the water!");

        // Assert
        interaction.HasBonusDamage.Should().BeTrue();
        interaction.AppliesEffect.Should().BeTrue();
        interaction.RemovesEffect.Should().BeFalse();
    }

    [Test]
    public void TurnStartEffectResult_NoEffects_ReturnsCanAct()
    {
        // Act
        var result = TurnStartEffectResult.NoEffects();

        // Assert
        result.CanAct.Should().BeTrue();
        result.MustFlee.Should().BeFalse();
        result.TotalDamage.Should().Be(0);
        result.HadTickEffects.Should().BeFalse();
    }

    [Test]
    public void TurnStartEffectResult_ActionsPrevented_ReturnsCannotAct()
    {
        // Act
        var result = TurnStartEffectResult.ActionsPrevented(
            5, 0, new[] { "stunned" }, "Stunned!", false);

        // Assert
        result.CanAct.Should().BeFalse();
        result.PreventionReason.Should().Be("Stunned!");
        result.TotalDamage.Should().Be(5);
    }

    [Test]
    public void AttackEffectResult_None_ReturnsEmpty()
    {
        // Act
        var result = AttackEffectResult.None();

        // Assert
        result.HadAppliedEffects.Should().BeFalse();
        result.HadInteractions.Should().BeFalse();
        result.BonusDamage.Should().Be(0);
    }

    [Test]
    public void CleanseResult_Succeeded_BuildsMessage()
    {
        // Act
        var result = CleanseResult.Succeeded(new[] { "poisoned", "bleeding" });

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Cleansed 2 effects.");
    }

    [Test]
    public void CleanseItem_Antidote_HasCorrectProperties()
    {
        // Act
        var item = CleanseItem.Antidote();

        // Assert
        item.Id.Should().Be("antidote");
        item.CleanseType.Should().Be(CleanseType.Specific);
        item.SpecificEffect.Should().Be("poisoned");
    }

    [Test]
    public void CleanseItem_Panacea_CleansesAllNegative()
    {
        // Act
        var item = CleanseItem.Panacea();

        // Assert
        item.CleanseType.Should().Be(CleanseType.AllNegative);
        item.SpecificEffect.Should().BeNull();
    }

    [Test]
    public void EffectInteractionResult_WithBonusDamage_SetsCorrectly()
    {
        // Act
        var result = EffectInteractionResult.WithBonusDamage(
            "wet-lightning", "Electricity arcs!", 15);

        // Assert
        result.BonusDamage.Should().Be(15);
        result.AppliedEffects.Should().BeEmpty();
        result.RemovedEffects.Should().BeEmpty();
    }
}
