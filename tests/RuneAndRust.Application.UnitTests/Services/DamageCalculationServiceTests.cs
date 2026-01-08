using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for DamageCalculationService (v0.0.9b).
/// </summary>
[TestFixture]
public class DamageCalculationServiceTests
{
    private DamageCalculationService _service = null!;
    private Mock<ILogger<DamageCalculationService>> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<DamageCalculationService>>();
        _service = new DamageCalculationService(_loggerMock.Object);
    }

    [Test]
    public void CalculateDamage_NoResistances_ReturnsBaseDamage()
    {
        // Arrange
        var resistances = DamageResistances.None;

        // Act
        var result = _service.CalculateDamage(100, "physical", resistances);

        // Assert
        result.BaseDamage.Should().Be(100);
        result.FinalDamage.Should().Be(100);
        result.WasResisted.Should().BeFalse();
        result.WasVulnerable.Should().BeFalse();
        result.WasImmune.Should().BeFalse();
    }

    [Test]
    public void CalculateDamage_50PercentResistance_HalvesDamage()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = 50
        });

        // Act
        var result = _service.CalculateDamage(100, "fire", resistances);

        // Assert
        result.BaseDamage.Should().Be(100);
        result.FinalDamage.Should().Be(50);
        result.ResistanceApplied.Should().Be(50);
        result.WasResisted.Should().BeTrue();
        result.WasVulnerable.Should().BeFalse();
    }

    [Test]
    public void CalculateDamage_Minus50PercentResistance_IncreasesDamage()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = -50
        });

        // Act
        var result = _service.CalculateDamage(100, "fire", resistances);

        // Assert
        result.BaseDamage.Should().Be(100);
        result.FinalDamage.Should().Be(150);
        result.ResistanceApplied.Should().Be(-50);
        result.WasResisted.Should().BeFalse();
        result.WasVulnerable.Should().BeTrue();
    }

    [Test]
    public void CalculateDamage_100PercentResistance_ReturnsImmune()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["poison"] = 100
        });

        // Act
        var result = _service.CalculateDamage(100, "poison", resistances);

        // Assert
        result.BaseDamage.Should().Be(100);
        result.FinalDamage.Should().Be(0);
        result.WasImmune.Should().BeTrue();
        result.WasResisted.Should().BeFalse();
        result.WasVulnerable.Should().BeFalse();
    }

    [Test]
    public void CalculateDamage_NullDamageType_DefaultsToPhysical()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["physical"] = 25
        });

        // Act
        var result = _service.CalculateDamage(100, null, resistances);

        // Assert
        result.DamageTypeId.Should().Be("physical");
        result.FinalDamage.Should().Be(75);
    }

    [Test]
    public void CalculateDamage_NegativeBaseDamage_ClampedToZero()
    {
        // Arrange
        var resistances = DamageResistances.None;

        // Act
        var result = _service.CalculateDamage(-50, "physical", resistances);

        // Assert
        result.BaseDamage.Should().Be(0);
        result.FinalDamage.Should().Be(0);
    }

    [Test]
    public void CalculateDamage_ResultNeverNegative()
    {
        // Arrange - even with extreme vulnerability, result should not be negative
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = -100  // Double damage
        });

        // Act
        var result = _service.CalculateDamage(0, "fire", resistances);

        // Assert
        result.FinalDamage.Should().BeGreaterOrEqualTo(0);
    }

    [Test]
    public void GetResistanceLabel_ReturnsImmuneAt100()
    {
        // Act & Assert
        _service.GetResistanceLabel(100).Should().Be("Immune");
    }

    [Test]
    public void GetResistanceLabel_ReturnsNormalAtZero()
    {
        // Act & Assert
        _service.GetResistanceLabel(0).Should().Be("Normal");
    }

    [Test]
    public void GetResistanceLabel_ReturnsResistantForPositive()
    {
        // Act & Assert
        _service.GetResistanceLabel(50).Should().Be("Resistant");
    }

    [Test]
    public void GetResistanceLabel_ReturnsVulnerableForNegative()
    {
        // Act & Assert
        _service.GetResistanceLabel(-50).Should().Be("Vulnerable");
    }

    [Test]
    public void GetResistanceDescription_IncludesPercentage()
    {
        // Arrange
        var resistances = new DamageResistances(new Dictionary<string, int>
        {
            ["fire"] = 50
        });

        // Act
        var description = _service.GetResistanceDescription("fire", resistances);

        // Assert
        description.Should().Contain("50%");
        description.Should().Contain("Resistant");
    }

    [Test]
    public void GetResistanceDescription_ReturnsNormalForZero()
    {
        // Arrange
        var resistances = DamageResistances.None;

        // Act
        var description = _service.GetResistanceDescription("fire", resistances);

        // Assert
        description.Should().Be("Normal");
    }
}
