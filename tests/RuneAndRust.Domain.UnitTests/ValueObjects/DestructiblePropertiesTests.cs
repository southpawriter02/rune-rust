using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the DestructibleProperties value object.
/// </summary>
[TestFixture]
public class DestructiblePropertiesTests
{
    // ===== Factory Method Tests =====

    [Test]
    public void Create_WithValidHP_ReturnsProperties()
    {
        // Arrange & Act
        var props = DestructibleProperties.Create(maxHP: 25, defense: 2);

        // Assert
        props.MaxHP.Should().Be(25);
        props.CurrentHP.Should().Be(25);
        props.Defense.Should().Be(2);
        props.IsDestroyed.Should().BeFalse();
    }

    [Test]
    public void Create_WithZeroHP_ThrowsException()
    {
        // Arrange & Act
        var act = () => DestructibleProperties.Create(maxHP: 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("maxHP");
    }

    [Test]
    public void Create_WithNegativeHP_ThrowsException()
    {
        // Arrange & Act
        var act = () => DestructibleProperties.Create(maxHP: -5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("maxHP");
    }

    [Test]
    public void Weak_CreatesLowHPProperties()
    {
        // Arrange & Act
        var props = DestructibleProperties.Weak();

        // Assert
        props.MaxHP.Should().Be(10);
        props.Defense.Should().Be(0);
    }

    [Test]
    public void Sturdy_CreatesMediumHPPropertiesWithDefense()
    {
        // Arrange & Act
        var props = DestructibleProperties.Sturdy();

        // Assert
        props.MaxHP.Should().Be(25);
        props.Defense.Should().Be(2);
    }

    [Test]
    public void Armored_CreatesHighHPPropertiesWithHighDefense()
    {
        // Arrange & Act
        var props = DestructibleProperties.Armored();

        // Assert
        props.MaxHP.Should().Be(50);
        props.Defense.Should().Be(5);
    }

    // ===== Damage Type Modifier Tests =====

    [Test]
    public void Create_WithVulnerabilities_StoresNormalized()
    {
        // Arrange & Act
        var props = DestructibleProperties.Create(
            maxHP: 10,
            vulnerabilities: new[] { "Fire", "SLASHING" });

        // Assert
        props.Vulnerabilities.Should().Contain("fire");
        props.Vulnerabilities.Should().Contain("slashing");
    }

    [Test]
    public void Create_WithResistances_StoresNormalized()
    {
        // Arrange & Act
        var props = DestructibleProperties.Create(
            maxHP: 10,
            resistances: new[] { "Piercing" });

        // Assert
        props.Resistances.Should().Contain("piercing");
    }

    [Test]
    public void Create_WithImmunities_StoresNormalized()
    {
        // Arrange & Act
        var props = DestructibleProperties.Create(
            maxHP: 10,
            immunities: new[] { "POISON", "Necrotic" });

        // Assert
        props.Immunities.Should().Contain("poison");
        props.Immunities.Should().Contain("necrotic");
    }

    // ===== TakeDamage Tests =====

    [Test]
    public void TakeDamage_NormalDamage_ReducesHP()
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 20);

        // Act
        var dealt = props.TakeDamage(5);

        // Assert
        dealt.Should().Be(5);
        props.CurrentHP.Should().Be(15);
    }

    [Test]
    public void TakeDamage_WithDefense_ReducesDamage()
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 20, defense: 3);

        // Act
        var dealt = props.TakeDamage(5);

        // Assert
        dealt.Should().Be(2); // 5 - 3 defense
        props.CurrentHP.Should().Be(18);
    }

    [Test]
    public void TakeDamage_DefenseExceedsDamage_DealsMinimum1()
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 20, defense: 10);

        // Act
        var dealt = props.TakeDamage(3);

        // Assert
        dealt.Should().Be(1); // Minimum damage
        props.CurrentHP.Should().Be(19);
    }

    [Test]
    public void TakeDamage_WithVulnerability_DealsDoubleDamage()
    {
        // Arrange
        var props = DestructibleProperties.Create(
            maxHP: 20,
            vulnerabilities: new[] { "fire" });

        // Act
        var dealt = props.TakeDamage(5, "fire");

        // Assert
        dealt.Should().Be(10); // 5 * 2
        props.CurrentHP.Should().Be(10);
    }

    [Test]
    public void TakeDamage_WithVulnerabilityAndDefense_AppliesBoth()
    {
        // Arrange
        var props = DestructibleProperties.Create(
            maxHP: 20,
            defense: 3,
            vulnerabilities: new[] { "fire" });

        // Act
        var dealt = props.TakeDamage(5, "fire");

        // Assert
        dealt.Should().Be(7); // (5 * 2) - 3 defense
        props.CurrentHP.Should().Be(13);
    }

    [Test]
    public void TakeDamage_WithResistance_DealsHalfDamage()
    {
        // Arrange
        var props = DestructibleProperties.Create(
            maxHP: 20,
            resistances: new[] { "cold" });

        // Act
        var dealt = props.TakeDamage(10, "cold");

        // Assert
        dealt.Should().Be(5); // 10 / 2
        props.CurrentHP.Should().Be(15);
    }

    [Test]
    public void TakeDamage_WithImmunity_DealsZeroDamage()
    {
        // Arrange
        var props = DestructibleProperties.Create(
            maxHP: 20,
            immunities: new[] { "poison" });

        // Act
        var dealt = props.TakeDamage(10, "poison");

        // Assert
        dealt.Should().Be(0);
        props.CurrentHP.Should().Be(20);
    }

    [Test]
    public void TakeDamage_CaseInsensitiveDamageType()
    {
        // Arrange
        var props = DestructibleProperties.Create(
            maxHP: 20,
            vulnerabilities: new[] { "fire" });

        // Act
        var dealt = props.TakeDamage(5, "FIRE");

        // Assert
        dealt.Should().Be(10); // Vulnerability applied
    }

    [Test]
    public void TakeDamage_WhenAlreadyDestroyed_ReturnsZero()
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 5);
        props.TakeDamage(10); // Destroy it

        // Act
        var dealt = props.TakeDamage(5);

        // Assert
        dealt.Should().Be(0);
        props.CurrentHP.Should().Be(0);
    }

    [Test]
    public void TakeDamage_ZeroDamage_ReturnsZero()
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 10);

        // Act
        var dealt = props.TakeDamage(0);

        // Assert
        dealt.Should().Be(0);
        props.CurrentHP.Should().Be(10);
    }

    [Test]
    public void TakeDamage_NegativeDamage_ReturnsZero()
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 10);

        // Act
        var dealt = props.TakeDamage(-5);

        // Assert
        dealt.Should().Be(0);
        props.CurrentHP.Should().Be(10);
    }

    // ===== IsDestroyed Tests =====

    [Test]
    public void IsDestroyed_WhenHPIsZero_ReturnsTrue()
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 5);

        // Act
        props.TakeDamage(5);

        // Assert
        props.IsDestroyed.Should().BeTrue();
        props.CurrentHP.Should().Be(0);
    }

    [Test]
    public void IsDestroyed_WhenDamageExceedsHP_ReturnsTrue()
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 5);

        // Act
        props.TakeDamage(100);

        // Assert
        props.IsDestroyed.Should().BeTrue();
        props.CurrentHP.Should().Be(0); // Clamped to 0
    }

    // ===== Helper Method Tests =====

    [Test]
    public void IsImmuneTo_ReturnsCorrectly()
    {
        // Arrange
        var props = DestructibleProperties.Create(
            maxHP: 10,
            immunities: new[] { "poison" });

        // Act & Assert
        props.IsImmuneTo("poison").Should().BeTrue();
        props.IsImmuneTo("POISON").Should().BeTrue();
        props.IsImmuneTo("fire").Should().BeFalse();
    }

    [Test]
    public void IsResistantTo_ReturnsCorrectly()
    {
        // Arrange
        var props = DestructibleProperties.Create(
            maxHP: 10,
            resistances: new[] { "cold" });

        // Act & Assert
        props.IsResistantTo("cold").Should().BeTrue();
        props.IsResistantTo("fire").Should().BeFalse();
    }

    [Test]
    public void IsVulnerableTo_ReturnsCorrectly()
    {
        // Arrange
        var props = DestructibleProperties.Create(
            maxHP: 10,
            vulnerabilities: new[] { "fire" });

        // Act & Assert
        props.IsVulnerableTo("fire").Should().BeTrue();
        props.IsVulnerableTo("cold").Should().BeFalse();
    }

    [Test]
    public void Repair_ResetsHPToMax()
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 20);
        props.TakeDamage(15);

        // Act
        props.Repair();

        // Assert
        props.CurrentHP.Should().Be(20);
        props.IsDestroyed.Should().BeFalse();
    }

    [Test]
    public void HealthPercentage_ReturnsCorrectValue()
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 100);
        props.TakeDamage(25);

        // Act
        var percent = props.HealthPercentage;

        // Assert
        percent.Should().Be(75);
    }

    [Test]
    [TestCase(100, "pristine")]
    [TestCase(95, "pristine")]
    [TestCase(85, "slightly damaged")]
    [TestCase(60, "damaged")]
    [TestCase(30, "heavily damaged")]
    [TestCase(10, "nearly destroyed")]
    [TestCase(0, "destroyed")]
    public void GetConditionDescription_ReturnsCorrectDescription(int remainingHP, string expected)
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 100);
        props.TakeDamage(100 - remainingHP);

        // Act
        var description = props.GetConditionDescription();

        // Assert
        description.Should().Be(expected);
    }

    [Test]
    public void ToString_ReturnsDescriptiveString()
    {
        // Arrange
        var props = DestructibleProperties.Create(maxHP: 20, defense: 3);
        props.TakeDamage(5);

        // Act
        var result = props.ToString();

        // Assert
        result.Should().Contain("HP: 18/20"); // 5 damage - 3 defense = 2 actual damage
        result.Should().Contain("Defense: 3");
    }
}
