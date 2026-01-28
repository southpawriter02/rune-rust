// ═══════════════════════════════════════════════════════════════════════════════
// ProficiencyThresholdsTests.cs
// Unit tests for the ProficiencyThresholds value object.
// Version: 0.16.1d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ProficiencyThresholds"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify the proficiency thresholds functionality including:
/// </para>
/// <list type="bullet">
///   <item><description>Default values (10/25/50)</description></item>
///   <item><description>Factory method validation</description></item>
///   <item><description>Derived property calculations</description></item>
///   <item><description>ToString formatting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ProficiencyThresholdsTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Default Thresholds Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that default thresholds have expected values.
    /// </summary>
    [Test]
    public void Default_ReturnsCorrectThresholds()
    {
        // Arrange & Act
        var thresholds = ProficiencyThresholds.Default;

        // Assert
        thresholds.NonProficientToProficient.Should().Be(10);
        thresholds.ProficientToExpert.Should().Be(25);
        thresholds.ExpertToMaster.Should().Be(50);
    }

    /// <summary>
    /// Verifies that TotalToMaster calculates correctly.
    /// </summary>
    [Test]
    public void Default_TotalToMaster_Returns85()
    {
        // Arrange
        var thresholds = ProficiencyThresholds.Default;

        // Act
        var total = thresholds.TotalToMaster;

        // Assert
        total.Should().Be(85); // 10 + 25 + 50
    }

    /// <summary>
    /// Verifies that TotalToMasterFromProficient calculates correctly.
    /// </summary>
    [Test]
    public void Default_TotalToMasterFromProficient_Returns75()
    {
        // Arrange
        var thresholds = ProficiencyThresholds.Default;

        // Act
        var total = thresholds.TotalToMasterFromProficient;

        // Assert
        total.Should().Be(75); // 25 + 50
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests - Valid Input
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create with valid parameters creates thresholds.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesThresholds()
    {
        // Arrange & Act
        var thresholds = ProficiencyThresholds.Create(
            nonProficientToProficient: 5,
            proficientToExpert: 15,
            expertToMaster: 30);

        // Assert
        thresholds.NonProficientToProficient.Should().Be(5);
        thresholds.ProficientToExpert.Should().Be(15);
        thresholds.ExpertToMaster.Should().Be(30);
        thresholds.TotalToMaster.Should().Be(50);
    }

    /// <summary>
    /// Verifies that Create with minimum valid values succeeds.
    /// </summary>
    [Test]
    public void Create_WithMinimumValidValues_Succeeds()
    {
        // Arrange & Act
        var thresholds = ProficiencyThresholds.Create(
            nonProficientToProficient: 1,
            proficientToExpert: 1,
            expertToMaster: 1);

        // Assert
        thresholds.TotalToMaster.Should().Be(3);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests - Invalid Input
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create throws on zero NonProficientToProficient.
    /// </summary>
    [Test]
    public void Create_WithZeroNonProficientToProficient_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ProficiencyThresholds.Create(
            nonProficientToProficient: 0,
            proficientToExpert: 25,
            expertToMaster: 50);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("nonProficientToProficient");
    }

    /// <summary>
    /// Verifies that Create throws on negative ProficientToExpert.
    /// </summary>
    [Test]
    public void Create_WithNegativeProficientToExpert_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ProficiencyThresholds.Create(
            nonProficientToProficient: 10,
            proficientToExpert: -5,
            expertToMaster: 50);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("proficientToExpert");
    }

    /// <summary>
    /// Verifies that Create throws on zero ExpertToMaster.
    /// </summary>
    [Test]
    public void Create_WithZeroExpertToMaster_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ProficiencyThresholds.Create(
            nonProficientToProficient: 10,
            proficientToExpert: 25,
            expertToMaster: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("expertToMaster");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString returns expected format.
    /// </summary>
    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var thresholds = ProficiencyThresholds.Default;

        // Act
        var result = thresholds.ToString();

        // Assert
        result.Should().Be("Thresholds: 10/25/50 (Total: 85)");
    }

    /// <summary>
    /// Verifies that ToString with custom values shows correct total.
    /// </summary>
    [Test]
    public void ToString_WithCustomValues_ShowsCorrectTotal()
    {
        // Arrange
        var thresholds = ProficiencyThresholds.Create(5, 10, 20);

        // Act
        var result = thresholds.ToString();

        // Assert
        result.Should().Be("Thresholds: 5/10/20 (Total: 35)");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Record Equality Tests
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that identical thresholds are equal.
    /// </summary>
    [Test]
    public void Equality_IdenticalThresholds_AreEqual()
    {
        // Arrange
        var thresholds1 = ProficiencyThresholds.Default;
        var thresholds2 = ProficiencyThresholds.Default;

        // Assert
        thresholds1.Should().Be(thresholds2);
        (thresholds1 == thresholds2).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that different thresholds are not equal.
    /// </summary>
    [Test]
    public void Equality_DifferentThresholds_AreNotEqual()
    {
        // Arrange
        var thresholds1 = ProficiencyThresholds.Default;
        var thresholds2 = ProficiencyThresholds.Create(5, 15, 30);

        // Assert
        thresholds1.Should().NotBe(thresholds2);
        (thresholds1 != thresholds2).Should().BeTrue();
    }
}
