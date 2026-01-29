namespace RuneAndRust.Domain.UnitTests.Enums;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for the <see cref="SpecialEffectType"/> enum.
/// </summary>
/// <remarks>
/// These tests verify:
/// <list type="bullet">
///   <item><description>The enum has the expected number of types (14 + None = 15)</description></item>
///   <item><description>Each type has the correct integer value</description></item>
///   <item><description>None is the default value (0)</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class SpecialEffectTypeTests
{
    /// <summary>
    /// Verifies the enum contains exactly 15 types (14 effect types + None).
    /// </summary>
    [Test]
    public void SpecialEffectType_HasExpected15Types()
    {
        // Arrange & Act
        var types = Enum.GetValues<SpecialEffectType>();

        // Assert (14 types + None = 15)
        types.Should().HaveCount(15);
    }

    /// <summary>
    /// Verifies each combat effect has the correct integer value.
    /// </summary>
    /// <param name="type">The effect type to test.</param>
    /// <param name="expectedValue">The expected integer value.</param>
    [Test]
    [TestCase(SpecialEffectType.IgnoreArmor, 1)]
    [TestCase(SpecialEffectType.LifeSteal, 2)]
    [TestCase(SpecialEffectType.Cleave, 3)]
    [TestCase(SpecialEffectType.Phase, 4)]
    [TestCase(SpecialEffectType.Reflect, 5)]
    public void SpecialEffectType_CombatEffects_HaveCorrectValues(
        SpecialEffectType type,
        int expectedValue)
    {
        // Assert
        ((int)type).Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies each elemental effect has the correct integer value.
    /// </summary>
    /// <param name="type">The effect type to test.</param>
    /// <param name="expectedValue">The expected integer value.</param>
    [Test]
    [TestCase(SpecialEffectType.FireDamage, 6)]
    [TestCase(SpecialEffectType.IceDamage, 7)]
    [TestCase(SpecialEffectType.LightningDamage, 8)]
    public void SpecialEffectType_ElementalEffects_HaveCorrectValues(
        SpecialEffectType type,
        int expectedValue)
    {
        // Assert
        ((int)type).Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies each triggered effect has the correct integer value.
    /// </summary>
    /// <param name="type">The effect type to test.</param>
    /// <param name="expectedValue">The expected integer value.</param>
    [Test]
    [TestCase(SpecialEffectType.Slow, 9)]
    [TestCase(SpecialEffectType.AutoHide, 10)]
    public void SpecialEffectType_TriggeredEffects_HaveCorrectValues(
        SpecialEffectType type,
        int expectedValue)
    {
        // Assert
        ((int)type).Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies each passive effect has the correct integer value.
    /// </summary>
    /// <param name="type">The effect type to test.</param>
    /// <param name="expectedValue">The expected integer value.</param>
    [Test]
    [TestCase(SpecialEffectType.Detection, 11)]
    [TestCase(SpecialEffectType.CriticalBonus, 12)]
    [TestCase(SpecialEffectType.DamageReduction, 13)]
    [TestCase(SpecialEffectType.FearAura, 14)]
    public void SpecialEffectType_PassiveEffects_HaveCorrectValues(
        SpecialEffectType type,
        int expectedValue)
    {
        // Assert
        ((int)type).Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies None is the default value (0).
    /// </summary>
    [Test]
    public void SpecialEffectType_None_IsZero()
    {
        // Assert
        ((int)SpecialEffectType.None).Should().Be(0);
    }

    /// <summary>
    /// Verifies None is the default value for an uninitialized SpecialEffectType.
    /// </summary>
    [Test]
    public void SpecialEffectType_Default_IsNone()
    {
        // Arrange
        SpecialEffectType defaultType = default;

        // Assert
        defaultType.Should().Be(SpecialEffectType.None);
    }
}
