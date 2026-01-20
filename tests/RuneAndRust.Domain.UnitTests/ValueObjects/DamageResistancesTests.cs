using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for DamageResistances value object (v0.0.9b).
/// </summary>
[TestFixture]
public class DamageResistancesTests
{
    [Test]
    public void None_HasEmptyResistances()
    {
        // Arrange & Act
        var resistances = DamageResistances.None;

        // Assert
        resistances.Values.Should().BeEmpty();
        resistances.GetResistance("physical").Should().Be(0);
    }

    [Test]
    public void Constructor_WithNull_CreatesEmptyResistances()
    {
        // Arrange & Act
        var resistances = new DamageResistances(null);

        // Assert
        resistances.Values.Should().BeEmpty();
    }

    [Test]
    public void Constructor_ClampsValuesToValidRange()
    {
        // Arrange
        var input = new Dictionary<string, int>
        {
            ["fire"] = 150,  // Should clamp to 100
            ["ice"] = -200   // Should clamp to -100
        };

        // Act
        var resistances = new DamageResistances(input);

        // Assert
        resistances.GetResistance("fire").Should().Be(100);
        resistances.GetResistance("ice").Should().Be(-100);
    }

    [Test]
    public void GetResistance_ReturnsCorrectValueForExistingType()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = 50,
            ["ice"] = -25
        });

        // Act & Assert
        resistances.GetResistance("fire").Should().Be(50);
        resistances.GetResistance("ice").Should().Be(-25);
    }

    [Test]
    public void GetResistance_ReturnsZeroForUnknownType()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = 50
        });

        // Act & Assert
        resistances.GetResistance("lightning").Should().Be(0);
    }

    [Test]
    public void GetResistance_IsCaseInsensitive()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = 50
        });

        // Act & Assert
        resistances.GetResistance("FIRE").Should().Be(50);
        resistances.GetResistance("Fire").Should().Be(50);
    }

    [Test]
    public void GetMultiplier_PositiveResistance_ReducesMultiplier()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = 50  // 50% resistance = 0.5 multiplier
        });

        // Act & Assert
        resistances.GetMultiplier("fire").Should().BeApproximately(0.5f, 0.01f);
    }

    [Test]
    public void GetMultiplier_NegativeResistance_IncreasesMultiplier()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = -50  // -50% resistance = 1.5 multiplier
        });

        // Act & Assert
        resistances.GetMultiplier("fire").Should().BeApproximately(1.5f, 0.01f);
    }

    [Test]
    public void GetMultiplier_FullImmunity_ReturnsZero()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["poison"] = 100
        });

        // Act & Assert
        resistances.GetMultiplier("poison").Should().Be(0f);
    }

    [Test]
    public void IsImmune_ReturnsTrueAt100()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["poison"] = 100
        });

        // Act & Assert
        resistances.IsImmune("poison").Should().BeTrue();
        resistances.IsImmune("fire").Should().BeFalse();
    }

    [Test]
    public void IsVulnerable_ReturnsTrueForNegativeValues()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = -50,
            ["ice"] = 50
        });

        // Act & Assert
        resistances.IsVulnerable("fire").Should().BeTrue();
        resistances.IsVulnerable("ice").Should().BeFalse();
        resistances.IsVulnerable("lightning").Should().BeFalse();
    }

    [Test]
    public void IsResistant_ReturnsTrueForPositiveNonImmune()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = 50,
            ["poison"] = 100,
            ["ice"] = -25
        });

        // Act & Assert
        resistances.IsResistant("fire").Should().BeTrue();
        resistances.IsResistant("poison").Should().BeFalse();  // Immune, not resistant
        resistances.IsResistant("ice").Should().BeFalse();     // Vulnerable
    }

    [Test]
    public void CombineWith_TakesHigherResistance()
    {
        // Arrange
        var a = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = 30,
            ["ice"] = 50
        });
        var b = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = 50,
            ["lightning"] = 25
        });

        // Act
        var combined = a.CombineWith(b);

        // Assert
        combined.GetResistance("fire").Should().Be(50);  // Higher from b
        combined.GetResistance("ice").Should().Be(50);   // Only in a
        combined.GetResistance("lightning").Should().Be(25);  // Only in b
    }

    [Test]
    public void GetSignificantResistances_ReturnsNonZeroOnly()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = 50,
            ["ice"] = 0,
            ["lightning"] = -25
        });

        // Act
        var significant = resistances.GetSignificantResistances().ToList();

        // Assert
        significant.Should().HaveCount(2);
        significant.Should().Contain("fire");
        significant.Should().Contain("lightning");
        significant.Should().NotContain("ice");
    }

    [Test]
    public void FromDictionary_CreatesValidResistances()
    {
        // Arrange
        var dict = new Dictionary<string, int>
        {
            ["physical"] = 25,
            ["fire"] = -50
        };

        // Act
        var resistances = DamageResistances.FromDictionary(dict);

        // Assert
        resistances.GetResistance("physical").Should().Be(25);
        resistances.GetResistance("fire").Should().Be(-50);
    }
}
