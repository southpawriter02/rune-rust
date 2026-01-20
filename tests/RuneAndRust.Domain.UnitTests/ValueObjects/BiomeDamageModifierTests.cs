using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for BiomeDamageModifier value object.
/// </summary>
[TestFixture]
public class BiomeDamageModifierTests
{
    [Test]
    public void ApplyModifier_WithFireInVolcanic_AppliesResistance()
    {
        // Arrange
        var modifier = BiomeDamageModifier.Volcanic;

        // Act
        var damage = modifier.ApplyModifier("fire", 20);

        // Assert
        damage.Should().Be(10); // 20 * 0.5 = 10
    }

    [Test]
    public void ApplyModifier_WithIceInVolcanic_AppliesVulnerability()
    {
        // Arrange
        var modifier = BiomeDamageModifier.Volcanic;

        // Act
        var damage = modifier.ApplyModifier("ice", 20);

        // Assert
        damage.Should().Be(30); // 20 * 1.5 = 30
    }

    [Test]
    public void ApplyModifier_UnknownType_ReturnsBaseDamage()
    {
        // Arrange
        var modifier = BiomeDamageModifier.Volcanic;

        // Act
        var damage = modifier.ApplyModifier("slashing", 20);

        // Assert
        damage.Should().Be(20); // No modifier = 1.0
    }

    [Test]
    public void GetModifier_KnownType_ReturnsCorrectValue()
    {
        // Arrange
        var modifier = BiomeDamageModifier.Flooded;

        // Act & Assert
        modifier.GetModifier("lightning").Should().Be(1.5f);
        modifier.GetModifier("ice").Should().Be(0.75f);
    }

    [Test]
    public void Default_HasNoModifiers()
    {
        // Arrange
        var modifier = BiomeDamageModifier.Default;

        // Act & Assert
        modifier.GetModifier("fire").Should().Be(1.0f);
        modifier.ApplyModifier("poison", 10).Should().Be(10);
    }
}
