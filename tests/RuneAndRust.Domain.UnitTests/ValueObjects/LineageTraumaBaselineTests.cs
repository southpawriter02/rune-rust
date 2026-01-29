// ═══════════════════════════════════════════════════════════════════════════════
// LineageTraumaBaselineTests.cs
// Unit tests for LineageTraumaBaseline value object.
// Version: 0.17.0d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="LineageTraumaBaseline"/> value object.
/// </summary>
/// <remarks>
/// Verifies correct behavior of the Create factory method, validation,
/// static lineage baseline properties, helper properties, and the
/// CalculateCorruptionAfterCleanse method.
/// </remarks>
[TestFixture]
public class LineageTraumaBaselineTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE FACTORY METHOD TESTS - SUCCESSFUL CREATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create succeeds with valid baseline parameters.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesBaseline()
    {
        // Arrange & Act
        var baseline = LineageTraumaBaseline.Create(
            startingCorruption: 5,
            startingStress: 10,
            corruptionResistanceModifier: -1,
            stressResistanceModifier: 2);

        // Assert
        baseline.StartingCorruption.Should().Be(5);
        baseline.StartingStress.Should().Be(10);
        baseline.CorruptionResistanceModifier.Should().Be(-1);
        baseline.StressResistanceModifier.Should().Be(2);
    }

    /// <summary>
    /// Verifies that Create succeeds with all zeros (baseline humans).
    /// </summary>
    [Test]
    public void Create_WithAllZeros_CreatesNeutralBaseline()
    {
        // Arrange & Act
        var baseline = LineageTraumaBaseline.Create(0, 0, 0, 0);

        // Assert
        baseline.StartingCorruption.Should().Be(0);
        baseline.StartingStress.Should().Be(0);
        baseline.CorruptionResistanceModifier.Should().Be(0);
        baseline.StressResistanceModifier.Should().Be(0);
        baseline.HasPermanentCorruption.Should().BeFalse();
        baseline.HasAnyVulnerability.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CREATE FACTORY METHOD TESTS - VALIDATION FAILURES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create throws when StartingCorruption is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeStartingCorruption_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => LineageTraumaBaseline.Create(
            startingCorruption: -1,
            startingStress: 0,
            corruptionResistanceModifier: 0,
            stressResistanceModifier: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("startingCorruption");
    }

    /// <summary>
    /// Verifies that Create throws when StartingStress is negative.
    /// </summary>
    [Test]
    public void Create_WithNegativeStartingStress_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => LineageTraumaBaseline.Create(
            startingCorruption: 0,
            startingStress: -5,
            corruptionResistanceModifier: 0,
            stressResistanceModifier: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("startingStress");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC BASELINE PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ClanBorn baseline has all zeros (baseline humans).
    /// </summary>
    [Test]
    public void ClanBorn_HasBaselineValues()
    {
        // Arrange & Act
        var baseline = LineageTraumaBaseline.ClanBorn;

        // Assert
        baseline.StartingCorruption.Should().Be(0);
        baseline.StartingStress.Should().Be(0);
        baseline.CorruptionResistanceModifier.Should().Be(0);
        baseline.StressResistanceModifier.Should().Be(0);
        baseline.HasPermanentCorruption.Should().BeFalse();
        baseline.HasAnyVulnerability.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that RuneMarked has 5 permanent Corruption and -1 Corruption resistance.
    /// </summary>
    [Test]
    public void RuneMarked_HasPermanentCorruptionAndVulnerability()
    {
        // Arrange & Act
        var baseline = LineageTraumaBaseline.RuneMarked;

        // Assert
        baseline.StartingCorruption.Should().Be(5);
        baseline.StartingStress.Should().Be(0);
        baseline.CorruptionResistanceModifier.Should().Be(-1);
        baseline.StressResistanceModifier.Should().Be(0);
        baseline.HasPermanentCorruption.Should().BeTrue();
        baseline.HasCorruptionVulnerability.Should().BeTrue();
        baseline.HasStressVulnerability.Should().BeFalse();
        baseline.HasAnyVulnerability.Should().BeTrue();
        baseline.PermanentCorruptionFloor.Should().Be(5);
    }

    /// <summary>
    /// Verifies that IronBlooded has -1 Stress resistance.
    /// </summary>
    [Test]
    public void IronBlooded_HasStressVulnerability()
    {
        // Arrange & Act
        var baseline = LineageTraumaBaseline.IronBlooded;

        // Assert
        baseline.StartingCorruption.Should().Be(0);
        baseline.StartingStress.Should().Be(0);
        baseline.CorruptionResistanceModifier.Should().Be(0);
        baseline.StressResistanceModifier.Should().Be(-1);
        baseline.HasPermanentCorruption.Should().BeFalse();
        baseline.HasCorruptionVulnerability.Should().BeFalse();
        baseline.HasStressVulnerability.Should().BeTrue();
        baseline.HasAnyVulnerability.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that VargrKin has baseline values (Stress handled by Primal Clarity trait).
    /// </summary>
    [Test]
    public void VargrKin_HasBaselineValues()
    {
        // Arrange & Act
        var baseline = LineageTraumaBaseline.VargrKin;

        // Assert
        baseline.StartingCorruption.Should().Be(0);
        baseline.StartingStress.Should().Be(0);
        baseline.CorruptionResistanceModifier.Should().Be(0);
        baseline.StressResistanceModifier.Should().Be(0);
        baseline.HasPermanentCorruption.Should().BeFalse();
        baseline.HasAnyVulnerability.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that None returns a neutral baseline.
    /// </summary>
    [Test]
    public void None_ReturnsNeutralBaseline()
    {
        // Arrange & Act
        var baseline = LineageTraumaBaseline.None;

        // Assert
        baseline.StartingCorruption.Should().Be(0);
        baseline.StartingStress.Should().Be(0);
        baseline.CorruptionResistanceModifier.Should().Be(0);
        baseline.StressResistanceModifier.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CALCULATE CORRUPTION AFTER CLEANSE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CalculateCorruptionAfterCleanse respects the permanent floor.
    /// </summary>
    [Test]
    public void CalculateCorruptionAfterCleanse_RespectsFloor_WhenCleansingBelowFloor()
    {
        // Arrange
        var runeMarked = LineageTraumaBaseline.RuneMarked; // Floor = 5

        // Act - Try to cleanse 10 from 12 (would result in 2, below floor)
        var result = runeMarked.CalculateCorruptionAfterCleanse(
            currentCorruption: 12,
            amountCleansed: 10);

        // Assert - Should clamp to floor of 5
        result.Should().Be(5);
    }

    /// <summary>
    /// Verifies that CalculateCorruptionAfterCleanse allows cleansing above floor.
    /// </summary>
    [Test]
    public void CalculateCorruptionAfterCleanse_AllowsCleansingAboveFloor()
    {
        // Arrange
        var runeMarked = LineageTraumaBaseline.RuneMarked; // Floor = 5

        // Act - Cleanse 5 from 12 (results in 7, above floor)
        var result = runeMarked.CalculateCorruptionAfterCleanse(
            currentCorruption: 12,
            amountCleansed: 5);

        // Assert - Should be 7 (above floor, no clamping)
        result.Should().Be(7);
    }

    /// <summary>
    /// Verifies that CalculateCorruptionAfterCleanse allows cleansing to exactly the floor.
    /// </summary>
    [Test]
    public void CalculateCorruptionAfterCleanse_AllowsCleansingToExactFloor()
    {
        // Arrange
        var runeMarked = LineageTraumaBaseline.RuneMarked; // Floor = 5

        // Act - Cleanse 7 from 12 (results in exactly 5)
        var result = runeMarked.CalculateCorruptionAfterCleanse(
            currentCorruption: 12,
            amountCleansed: 7);

        // Assert - Should be exactly 5
        result.Should().Be(5);
    }

    /// <summary>
    /// Verifies that CalculateCorruptionAfterCleanse works for zero-floor lineages.
    /// </summary>
    [Test]
    public void CalculateCorruptionAfterCleanse_ZeroFloor_AllowsFullCleansing()
    {
        // Arrange
        var clanBorn = LineageTraumaBaseline.ClanBorn; // Floor = 0

        // Act - Cleanse 10 from 8 (results in -2, clamped to floor 0)
        var result = clanBorn.CalculateCorruptionAfterCleanse(
            currentCorruption: 8,
            amountCleansed: 10);

        // Assert - Should be 0 (floor)
        result.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString returns descriptive text for Rune-Marked baseline.
    /// </summary>
    [Test]
    public void ToString_ForRuneMarked_ReturnsDescriptiveText()
    {
        // Arrange
        var runeMarked = LineageTraumaBaseline.RuneMarked;

        // Act
        var result = runeMarked.ToString();

        // Assert
        result.Should().Contain("5 permanent Corruption");
        result.Should().Contain("Corruption Resist -1");
    }

    /// <summary>
    /// Verifies that ToString returns neutral text for baseline lineages.
    /// </summary>
    [Test]
    public void ToString_ForClanBorn_ReturnsNoModifiersText()
    {
        // Arrange
        var clanBorn = LineageTraumaBaseline.ClanBorn;

        // Act
        var result = clanBorn.ToString();

        // Assert
        result.Should().Be("No trauma modifiers");
    }
}
