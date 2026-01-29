namespace RuneAndRust.Domain.UnitTests.Enums;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for the <see cref="EffectTriggerType"/> enum.
/// </summary>
/// <remarks>
/// These tests verify:
/// <list type="bullet">
///   <item><description>The enum has the expected number of types (6)</description></item>
///   <item><description>Each type has the correct integer value</description></item>
///   <item><description>Passive is the default value (0)</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class EffectTriggerTypeTests
{
    /// <summary>
    /// Verifies the enum contains exactly 6 trigger types.
    /// </summary>
    [Test]
    public void EffectTriggerType_HasExpected6Types()
    {
        // Arrange & Act
        var types = Enum.GetValues<EffectTriggerType>();

        // Assert
        types.Should().HaveCount(6);
    }

    /// <summary>
    /// Verifies each trigger type has the correct integer value.
    /// </summary>
    /// <param name="type">The trigger type to test.</param>
    /// <param name="expectedValue">The expected integer value.</param>
    [Test]
    [TestCase(EffectTriggerType.Passive, 0)]
    [TestCase(EffectTriggerType.OnAttack, 1)]
    [TestCase(EffectTriggerType.OnHit, 2)]
    [TestCase(EffectTriggerType.OnDamageDealt, 3)]
    [TestCase(EffectTriggerType.OnDamageTaken, 4)]
    [TestCase(EffectTriggerType.OnKill, 5)]
    public void EffectTriggerType_HasCorrectValues(EffectTriggerType type, int expectedValue)
    {
        // Assert
        ((int)type).Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies Passive is the default value for an uninitialized EffectTriggerType.
    /// </summary>
    [Test]
    public void EffectTriggerType_Default_IsPassive()
    {
        // Arrange
        EffectTriggerType defaultType = default;

        // Assert
        defaultType.Should().Be(EffectTriggerType.Passive);
    }

    /// <summary>
    /// Verifies trigger types are ordered by combat timing.
    /// </summary>
    [Test]
    public void EffectTriggerType_OrderedByCombatTiming()
    {
        // Assert - Triggers should be ordered by combat sequence
        ((int)EffectTriggerType.Passive).Should().BeLessThan((int)EffectTriggerType.OnAttack);
        ((int)EffectTriggerType.OnAttack).Should().BeLessThan((int)EffectTriggerType.OnHit);
        ((int)EffectTriggerType.OnHit).Should().BeLessThan((int)EffectTriggerType.OnDamageDealt);
        ((int)EffectTriggerType.OnDamageDealt).Should().BeLessThan((int)EffectTriggerType.OnDamageTaken);
        ((int)EffectTriggerType.OnDamageTaken).Should().BeLessThan((int)EffectTriggerType.OnKill);
    }
}
