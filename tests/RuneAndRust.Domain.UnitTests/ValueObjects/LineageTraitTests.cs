using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="LineageTrait"/> value object.
/// </summary>
/// <remarks>
/// Verifies correct behavior of the Create factory method, validation,
/// static trait properties, and helper properties.
/// </remarks>
[TestFixture]
public class LineageTraitTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE FACTORY METHOD TESTS - SUCCESSFUL CREATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create succeeds with valid BonusDiceToSkill effect parameters.
    /// </summary>
    [Test]
    public void Create_WithValidBonusDiceToSkillEffect_Succeeds()
    {
        // Arrange & Act
        var trait = LineageTrait.Create(
            traitId: "test_skill_trait",
            traitName: "[Test Skill Trait]",
            description: "Test description for skill bonus trait.",
            effectType: LineageTraitEffectType.BonusDiceToSkill,
            triggerCondition: "skill_check_initiated",
            bonusDice: 1,
            targetCheck: "rhetoric",
            targetCondition: "target.type == human");

        // Assert
        trait.TraitId.Should().Be("test_skill_trait");
        trait.TraitName.Should().Be("[Test Skill Trait]");
        trait.EffectType.Should().Be(LineageTraitEffectType.BonusDiceToSkill);
        trait.BonusDice.Should().Be(1);
        trait.UsesBonusDice.Should().BeTrue();
        trait.UsesPercentModifier.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create succeeds with valid PercentageModifier effect parameters.
    /// </summary>
    [Test]
    public void Create_WithValidPercentageModifierEffect_Succeeds()
    {
        // Arrange & Act
        var trait = LineageTrait.Create(
            traitId: "test_percent_trait",
            traitName: "[Test Percent Trait]",
            description: "Test description for percentage modifier trait.",
            effectType: LineageTraitEffectType.PercentageModifier,
            triggerCondition: "stress_gain",
            percentModifier: -0.10f);

        // Assert
        trait.TraitId.Should().Be("test_percent_trait");
        trait.PercentModifier.Should().Be(-0.10f);
        trait.UsesBonusDice.Should().BeFalse();
        trait.UsesPercentModifier.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE FACTORY METHOD TESTS - VALIDATION FAILURES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create throws when BonusDiceToSkill effect lacks BonusDice.
    /// </summary>
    [Test]
    public void Create_WithBonusDiceEffectMissingBonusDice_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => LineageTrait.Create(
            traitId: "invalid_trait",
            traitName: "[Invalid Trait]",
            description: "This trait is missing required BonusDice.",
            effectType: LineageTraitEffectType.BonusDiceToSkill);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("bonusDice");
    }

    /// <summary>
    /// Verifies that Create throws when PercentageModifier effect lacks PercentModifier.
    /// </summary>
    [Test]
    public void Create_WithPercentageEffectMissingPercentModifier_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => LineageTrait.Create(
            traitId: "invalid_trait",
            traitName: "[Invalid Trait]",
            description: "This trait is missing required PercentModifier.",
            effectType: LineageTraitEffectType.PercentageModifier);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("percentModifier");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC TRAIT PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that SurvivorsResolve has correct values for Clan-Born trait.
    /// </summary>
    [Test]
    public void SurvivorsResolve_HasCorrectValues()
    {
        // Arrange & Act
        var trait = LineageTrait.SurvivorsResolve;

        // Assert
        trait.TraitId.Should().Be("survivors_resolve");
        trait.TraitName.Should().Be("[Survivor's Resolve]");
        trait.EffectType.Should().Be(LineageTraitEffectType.BonusDiceToSkill);
        trait.BonusDice.Should().Be(1);
        trait.TargetCheck.Should().Be("rhetoric");
        trait.HasTargetCondition.Should().BeTrue();
        trait.UsesBonusDice.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that AetherTainted has correct values for Rune-Marked trait.
    /// </summary>
    [Test]
    public void AetherTainted_HasCorrectValues()
    {
        // Arrange & Act
        var trait = LineageTrait.AetherTainted;

        // Assert
        trait.TraitId.Should().Be("aether_tainted");
        trait.TraitName.Should().Be("[Aether-Tainted]");
        trait.EffectType.Should().Be(LineageTraitEffectType.PassiveAuraBonus);
        trait.PercentModifier.Should().Be(0.10f);
        trait.UsesPercentModifier.Should().BeTrue();
        trait.HasTargetCondition.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that helper properties correctly classify effect types.
    /// </summary>
    [Test]
    public void HelperProperties_CorrectlyClassifyEffectTypes()
    {
        // Arrange
        var bonusDiceSkill = LineageTrait.SurvivorsResolve;
        var bonusDiceResolve = LineageTrait.HazardAcclimation;
        var percentModifier = LineageTrait.PrimalClarity;
        var passiveAura = LineageTrait.AetherTainted;

        // Assert - BonusDice traits
        bonusDiceSkill.UsesBonusDice.Should().BeTrue();
        bonusDiceSkill.UsesPercentModifier.Should().BeFalse();
        bonusDiceResolve.UsesBonusDice.Should().BeTrue();
        bonusDiceResolve.UsesPercentModifier.Should().BeFalse();

        // Assert - Percent modifier traits
        percentModifier.UsesBonusDice.Should().BeFalse();
        percentModifier.UsesPercentModifier.Should().BeTrue();
        passiveAura.UsesBonusDice.Should().BeFalse();
        passiveAura.UsesPercentModifier.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that None returns an empty trait with no effect.
    /// </summary>
    [Test]
    public void None_ReturnsEmptyTrait()
    {
        // Arrange & Act
        var none = LineageTrait.None;

        // Assert
        none.TraitId.Should().BeEmpty();
        none.TraitName.Should().BeEmpty();
        none.Description.Should().BeEmpty();
        none.EffectiveBonusDice.Should().Be(0);
        none.EffectivePercentModifier.Should().Be(0f);
    }
}
