using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for DamageInstance value object (v0.0.9b).
/// </summary>
[TestFixture]
public class DamageInstanceTests
{
    [Test]
    public void Normal_CreatesInstanceWithNormalDamage()
    {
        // Arrange & Act
        var instance = DamageInstance.Normal(50, "physical");

        // Assert
        instance.BaseDamage.Should().Be(50);
        instance.FinalDamage.Should().Be(50);
        instance.DamageTypeId.Should().Be("physical");
        instance.ResistanceApplied.Should().Be(0);
        instance.WasResisted.Should().BeFalse();
        instance.WasVulnerable.Should().BeFalse();
        instance.WasImmune.Should().BeFalse();
        instance.HadResistanceEffect.Should().BeFalse();
    }

    [Test]
    public void Immune_CreatesInstanceWithZeroDamage()
    {
        // Arrange & Act
        var instance = DamageInstance.Immune(100, "poison");

        // Assert
        instance.BaseDamage.Should().Be(100);
        instance.FinalDamage.Should().Be(0);
        instance.DamageTypeId.Should().Be("poison");
        instance.ResistanceApplied.Should().Be(100);
        instance.WasImmune.Should().BeTrue();
        instance.HadResistanceEffect.Should().BeTrue();
    }

    [Test]
    public void GetResistanceDescription_ReturnsImmuneForImmunity()
    {
        // Arrange
        var instance = DamageInstance.Immune(50, "poison");

        // Act
        var description = instance.GetResistanceDescription();

        // Assert
        description.Should().Be("Immune!");
    }

    [Test]
    public void GetResistanceDescription_ReturnsResistedForPositiveResistance()
    {
        // Arrange
        var instance = new DamageInstance(100, "fire", 50, 50, true, false, false);

        // Act
        var description = instance.GetResistanceDescription();

        // Assert
        description.Should().Contain("Resisted");
        description.Should().Contain("50%");
    }

    [Test]
    public void GetResistanceDescription_ReturnsVulnerableForNegativeResistance()
    {
        // Arrange
        var instance = new DamageInstance(100, "fire", 150, -50, false, true, false);

        // Act
        var description = instance.GetResistanceDescription();

        // Assert
        description.Should().Contain("Vulnerable");
        description.Should().Contain("50%");
    }

    [Test]
    public void GetResistanceDescription_ReturnsEmptyForNoEffect()
    {
        // Arrange
        var instance = DamageInstance.Normal(50, "physical");

        // Act
        var description = instance.GetResistanceDescription();

        // Assert
        description.Should().BeEmpty();
    }

    [Test]
    public void HadResistanceEffect_TrueWhenResisted()
    {
        // Arrange
        var instance = new DamageInstance(100, "fire", 50, 50, true, false, false);

        // Assert
        instance.HadResistanceEffect.Should().BeTrue();
    }

    [Test]
    public void HadResistanceEffect_TrueWhenVulnerable()
    {
        // Arrange
        var instance = new DamageInstance(100, "fire", 150, -50, false, true, false);

        // Assert
        instance.HadResistanceEffect.Should().BeTrue();
    }
}
