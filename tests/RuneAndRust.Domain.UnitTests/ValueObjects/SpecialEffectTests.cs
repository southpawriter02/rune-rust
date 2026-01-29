namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="SpecialEffect"/> value object.
/// </summary>
/// <remarks>
/// These tests verify:
/// <list type="bullet">
///   <item><description>Valid effects are created correctly</description></item>
///   <item><description>Effect IDs are normalized to lowercase</description></item>
///   <item><description>Validation rejects invalid inputs</description></item>
///   <item><description>Magnitude validation by effect type</description></item>
///   <item><description>Elemental effects require damage type</description></item>
///   <item><description>Trigger type validation matches expected triggers</description></item>
///   <item><description>Helper methods work correctly</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class SpecialEffectTests
{
    #region Create - Valid Cases

    /// <summary>
    /// Verifies that a valid SpecialEffect is created with all properties set correctly.
    /// </summary>
    [Test]
    public void Create_WithValidData_CreatesEffect()
    {
        // Arrange & Act
        var effect = SpecialEffect.Create(
            "life-steal",
            SpecialEffectType.LifeSteal,
            EffectTriggerType.OnDamageDealt,
            0.15m,
            description: "Heals 15% damage");

        // Assert
        effect.EffectId.Should().Be("life-steal");
        effect.EffectType.Should().Be(SpecialEffectType.LifeSteal);
        effect.TriggerType.Should().Be(EffectTriggerType.OnDamageDealt);
        effect.Magnitude.Should().Be(0.15m);
        effect.Description.Should().Be("Heals 15% damage");
    }

    /// <summary>
    /// Verifies that effect IDs are normalized to lowercase.
    /// </summary>
    [Test]
    public void Create_NormalizesEffectIdToLowercase()
    {
        // Arrange & Act
        var effect = SpecialEffect.Create(
            "LIFE-STEAL",
            SpecialEffectType.LifeSteal,
            EffectTriggerType.OnDamageDealt,
            0.15m);

        // Assert
        effect.EffectId.Should().Be("life-steal");
    }

    /// <summary>
    /// Verifies that damage type IDs are normalized to lowercase.
    /// </summary>
    [Test]
    public void Create_NormalizesDamageTypeIdToLowercase()
    {
        // Arrange & Act
        var effect = SpecialEffect.Create(
            "fire-damage",
            SpecialEffectType.FireDamage,
            EffectTriggerType.OnHit,
            10m,
            damageTypeId: "FIRE");

        // Assert
        effect.DamageTypeId.Should().Be("fire");
    }

    #endregion

    #region Create - Id Validation

    /// <summary>
    /// Verifies that null effect ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullEffectId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecialEffect.Create(
            null!,
            SpecialEffectType.LifeSteal,
            EffectTriggerType.OnDamageDealt,
            0.15m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that empty effect ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithEmptyEffectId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecialEffect.Create(
            "",
            SpecialEffectType.LifeSteal,
            EffectTriggerType.OnDamageDealt,
            0.15m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that whitespace effect ID throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceEffectId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecialEffect.Create(
            "   ",
            SpecialEffectType.LifeSteal,
            EffectTriggerType.OnDamageDealt,
            0.15m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Create - Effect Type Validation

    /// <summary>
    /// Verifies that None effect type throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNoneType_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => SpecialEffect.Create(
            "invalid",
            SpecialEffectType.None,
            EffectTriggerType.Passive,
            0m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*None type*");
    }

    #endregion

    #region Create - Magnitude Validation

    /// <summary>
    /// Verifies that percentage effects validate magnitude range (0-1).
    /// </summary>
    [Test]
    public void Create_PercentageEffect_RejectsMagnitudeOver100Percent()
    {
        // Arrange & Act
        var act = () => SpecialEffect.Create(
            "life-steal",
            SpecialEffectType.LifeSteal,
            EffectTriggerType.OnDamageDealt,
            1.5m); // Over 100%

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that percentage effects reject negative magnitude.
    /// </summary>
    [Test]
    public void Create_PercentageEffect_RejectsNegativeMagnitude()
    {
        // Arrange & Act
        var act = () => SpecialEffect.Create(
            "reflect",
            SpecialEffectType.Reflect,
            EffectTriggerType.OnDamageTaken,
            -0.1m);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that percentage effects accept boundary values (0 and 1).
    /// </summary>
    [Test]
    [TestCase(0.0)]
    [TestCase(0.5)]
    [TestCase(1.0)]
    public void Create_PercentageEffect_AcceptsValidRange(decimal magnitude)
    {
        // Arrange & Act
        var effect = SpecialEffect.Create(
            "critical-edge",
            SpecialEffectType.CriticalBonus,
            EffectTriggerType.Passive,
            magnitude);

        // Assert
        effect.Magnitude.Should().Be(magnitude);
    }

    /// <summary>
    /// Verifies that flat damage effects accept valid magnitude.
    /// </summary>
    [Test]
    public void Create_FlatDamageEffect_AcceptsValidMagnitude()
    {
        // Arrange & Act
        var effect = SpecialEffect.Create(
            "fire-damage",
            SpecialEffectType.FireDamage,
            EffectTriggerType.OnHit,
            25m,
            damageTypeId: "fire");

        // Assert
        effect.Magnitude.Should().Be(25m);
    }

    /// <summary>
    /// Verifies that flat damage effects reject negative magnitude.
    /// </summary>
    [Test]
    public void Create_FlatDamageEffect_RejectsNegativeMagnitude()
    {
        // Arrange & Act
        var act = () => SpecialEffect.Create(
            "fire-damage",
            SpecialEffectType.FireDamage,
            EffectTriggerType.OnHit,
            -10m,
            damageTypeId: "fire");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Create - Damage Type Validation

    /// <summary>
    /// Verifies that elemental effects require damage type ID.
    /// </summary>
    [Test]
    public void Create_ElementalEffect_RequiresDamageTypeId()
    {
        // Arrange & Act
        var act = () => SpecialEffect.Create(
            "fire-damage",
            SpecialEffectType.FireDamage,
            EffectTriggerType.OnHit,
            10m,
            damageTypeId: null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*DamageTypeId*");
    }

    /// <summary>
    /// Verifies that elemental effects are created correctly with damage type.
    /// </summary>
    [Test]
    public void Create_ElementalEffect_WithDamageType_Succeeds()
    {
        // Arrange & Act
        var effect = SpecialEffect.Create(
            "fire-damage",
            SpecialEffectType.FireDamage,
            EffectTriggerType.OnHit,
            10m,
            damageTypeId: "fire");

        // Assert
        effect.DamageTypeId.Should().Be("fire");
        effect.IsElemental.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ice damage effects require damage type.
    /// </summary>
    [Test]
    public void Create_IceDamageEffect_RequiresDamageTypeId()
    {
        // Arrange & Act
        var act = () => SpecialEffect.Create(
            "ice-damage",
            SpecialEffectType.IceDamage,
            EffectTriggerType.OnHit,
            8m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*DamageTypeId*");
    }

    /// <summary>
    /// Verifies that lightning damage effects require damage type.
    /// </summary>
    [Test]
    public void Create_LightningDamageEffect_RequiresDamageTypeId()
    {
        // Arrange & Act
        var act = () => SpecialEffect.Create(
            "lightning-damage",
            SpecialEffectType.LightningDamage,
            EffectTriggerType.OnHit,
            12m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*DamageTypeId*");
    }

    #endregion

    #region Create - Trigger Type Validation

    /// <summary>
    /// Verifies that wrong trigger type throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithWrongTriggerType_ThrowsArgumentException()
    {
        // Arrange - LifeSteal should be OnDamageDealt, not Passive
        var act = () => SpecialEffect.Create(
            "life-steal",
            SpecialEffectType.LifeSteal,
            EffectTriggerType.Passive, // Wrong trigger
            0.15m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*expects trigger*");
    }

    /// <summary>
    /// Verifies trigger validation for OnAttack effects.
    /// </summary>
    [Test]
    [TestCase(SpecialEffectType.IgnoreArmor)]
    [TestCase(SpecialEffectType.Cleave)]
    [TestCase(SpecialEffectType.Phase)]
    public void Create_OnAttackEffects_RequireOnAttackTrigger(SpecialEffectType effectType)
    {
        // Arrange & Act
        var act = () => SpecialEffect.Create(
            "test",
            effectType,
            EffectTriggerType.Passive, // Wrong trigger
            1.0m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*expects trigger*OnAttack*");
    }

    #endregion

    #region GetExpectedTriggerType

    /// <summary>
    /// Verifies GetExpectedTriggerType returns correct triggers for all effect types.
    /// </summary>
    [Test]
    public void GetExpectedTriggerType_ReturnsCorrectTriggers()
    {
        // Assert - OnAttack effects
        SpecialEffect.GetExpectedTriggerType(SpecialEffectType.IgnoreArmor)
            .Should().Be(EffectTriggerType.OnAttack);
        SpecialEffect.GetExpectedTriggerType(SpecialEffectType.Cleave)
            .Should().Be(EffectTriggerType.OnAttack);
        SpecialEffect.GetExpectedTriggerType(SpecialEffectType.Phase)
            .Should().Be(EffectTriggerType.OnAttack);

        // Assert - OnHit effects
        SpecialEffect.GetExpectedTriggerType(SpecialEffectType.FireDamage)
            .Should().Be(EffectTriggerType.OnHit);
        SpecialEffect.GetExpectedTriggerType(SpecialEffectType.Slow)
            .Should().Be(EffectTriggerType.OnHit);

        // Assert - Other triggers
        SpecialEffect.GetExpectedTriggerType(SpecialEffectType.LifeSteal)
            .Should().Be(EffectTriggerType.OnDamageDealt);
        SpecialEffect.GetExpectedTriggerType(SpecialEffectType.Reflect)
            .Should().Be(EffectTriggerType.OnDamageTaken);
        SpecialEffect.GetExpectedTriggerType(SpecialEffectType.AutoHide)
            .Should().Be(EffectTriggerType.OnKill);

        // Assert - Passive effects
        SpecialEffect.GetExpectedTriggerType(SpecialEffectType.Detection)
            .Should().Be(EffectTriggerType.Passive);
        SpecialEffect.GetExpectedTriggerType(SpecialEffectType.CriticalBonus)
            .Should().Be(EffectTriggerType.Passive);
    }

    #endregion

    #region Helper Properties

    /// <summary>
    /// Verifies IsPassive is true for passive effects.
    /// </summary>
    [Test]
    public void IsPassive_ForPassiveEffect_ReturnsTrue()
    {
        // Arrange
        var effect = SpecialEffect.Create(
            "detection",
            SpecialEffectType.Detection,
            EffectTriggerType.Passive,
            1.0m);

        // Assert
        effect.IsPassive.Should().BeTrue();
    }

    /// <summary>
    /// Verifies IsPassive is false for non-passive effects.
    /// </summary>
    [Test]
    public void IsPassive_ForNonPassiveEffect_ReturnsFalse()
    {
        // Arrange
        var effect = SpecialEffect.Create(
            "life-steal",
            SpecialEffectType.LifeSteal,
            EffectTriggerType.OnDamageDealt,
            0.15m);

        // Assert
        effect.IsPassive.Should().BeFalse();
    }

    /// <summary>
    /// Verifies IsElemental identifies all elemental effects.
    /// </summary>
    [Test]
    public void IsElemental_ForElementalEffects_ReturnsTrue()
    {
        // Arrange
        var fire = SpecialEffect.Create("fire", SpecialEffectType.FireDamage,
            EffectTriggerType.OnHit, 10m, "fire");
        var ice = SpecialEffect.Create("ice", SpecialEffectType.IceDamage,
            EffectTriggerType.OnHit, 10m, "ice");
        var lightning = SpecialEffect.Create("lightning", SpecialEffectType.LightningDamage,
            EffectTriggerType.OnHit, 10m, "lightning");

        // Assert
        fire.IsElemental.Should().BeTrue();
        ice.IsElemental.Should().BeTrue();
        lightning.IsElemental.Should().BeTrue();
    }

    /// <summary>
    /// Verifies IsElemental is false for non-elemental effects.
    /// </summary>
    [Test]
    public void IsElemental_ForNonElementalEffects_ReturnsFalse()
    {
        // Arrange
        var lifeSteal = SpecialEffect.Create("ls", SpecialEffectType.LifeSteal,
            EffectTriggerType.OnDamageDealt, 0.1m);

        // Assert
        lifeSteal.IsElemental.Should().BeFalse();
    }

    /// <summary>
    /// Verifies HasTrigger correctly identifies trigger types.
    /// </summary>
    [Test]
    public void HasTrigger_WithMatchingTrigger_ReturnsTrue()
    {
        // Arrange
        var effect = SpecialEffect.Create(
            "life-steal",
            SpecialEffectType.LifeSteal,
            EffectTriggerType.OnDamageDealt,
            0.15m);

        // Assert
        effect.HasTrigger(EffectTriggerType.OnDamageDealt).Should().BeTrue();
        effect.HasTrigger(EffectTriggerType.OnAttack).Should().BeFalse();
    }

    #endregion

    #region None and Default Description

    /// <summary>
    /// Verifies None returns a default empty effect.
    /// </summary>
    [Test]
    public void None_ReturnsDefaultEffect()
    {
        // Arrange & Act
        var none = SpecialEffect.None;

        // Assert
        none.EffectId.Should().Be("none");
        none.EffectType.Should().Be(SpecialEffectType.None);
        none.Magnitude.Should().Be(0m);
        none.Description.Should().Be("No effect");
    }

    /// <summary>
    /// Verifies that description is auto-generated when not provided.
    /// </summary>
    [Test]
    public void Create_GeneratesDefaultDescription_WhenNotProvided()
    {
        // Arrange & Act
        var effect = SpecialEffect.Create(
            "life-steal",
            SpecialEffectType.LifeSteal,
            EffectTriggerType.OnDamageDealt,
            0.15m);

        // Assert
        effect.Description.Should().Contain("15%");
        effect.Description.Should().Contain("damage dealt");
    }

    /// <summary>
    /// Verifies default description for damage reduction.
    /// </summary>
    [Test]
    public void Create_GeneratesCorrectDefaultDescription_ForDamageReduction()
    {
        // Arrange & Act
        var effect = SpecialEffect.Create(
            "damage-reduction",
            SpecialEffectType.DamageReduction,
            EffectTriggerType.Passive,
            5m);

        // Assert
        effect.Description.Should().Contain("5");
        effect.Description.Should().Contain("incoming damage");
    }

    #endregion

    #region ToString

    /// <summary>
    /// Verifies ToString provides a useful debug representation.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var effect = SpecialEffect.Create(
            "life-steal",
            SpecialEffectType.LifeSteal,
            EffectTriggerType.OnDamageDealt,
            0.15m);

        // Act
        var result = effect.ToString();

        // Assert
        result.Should().Contain("life-steal");
        result.Should().Contain("LifeSteal");
        result.Should().Contain("OnDamageDealt");
        result.Should().Contain("0.15");
    }

    #endregion
}
