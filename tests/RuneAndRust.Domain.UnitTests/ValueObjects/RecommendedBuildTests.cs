// ═══════════════════════════════════════════════════════════════════════════════
// RecommendedBuildTests.cs
// Unit tests for the RecommendedBuild value object verifying factory method
// validation, computed properties, display formatting, and edge cases.
// Version: 0.17.3e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="RecommendedBuild"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that RecommendedBuild correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Creates validated instances via the Create factory method</description></item>
///   <item><description>Validates attribute ranges [1, 10] and rejects out-of-range values</description></item>
///   <item><description>Validates name is not null or whitespace</description></item>
///   <item><description>Computes TotalAttributePoints as sum of all five attributes</description></item>
///   <item><description>Reports HasOptimalLineage correctly for builds with and without lineage</description></item>
///   <item><description>Formats display summaries and ToString output correctly</description></item>
///   <item><description>Supports optional lineage parameter for lineage-specific builds</description></item>
/// </list>
/// </remarks>
/// <seealso cref="RecommendedBuild"/>
[TestFixture]
public class RecommendedBuildTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — Valid Data
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.Create"/> creates a valid instance
    /// with correct properties when given valid data.
    /// </summary>
    [Test]
    public void Create_WithValidData_CreatesBuild()
    {
        // Arrange & Act
        var build = RecommendedBuild.Create(
            "Standard Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);

        // Assert
        build.Name.Should().Be("Standard Warrior");
        build.Might.Should().Be(4);
        build.Finesse.Should().Be(3);
        build.Wits.Should().Be(2);
        build.Will.Should().Be(2);
        build.Sturdiness.Should().Be(4);
        build.OptimalLineage.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.Create"/> correctly sets
    /// the OptimalLineage when provided.
    /// </summary>
    [Test]
    public void Create_WithOptimalLineage_SetsLineage()
    {
        // Arrange & Act
        var build = RecommendedBuild.Create(
            "IronBlooded Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4,
            optimalLineage: Lineage.IronBlooded);

        // Assert
        build.OptimalLineage.Should().Be(Lineage.IronBlooded);
        build.HasOptimalLineage.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.Create"/> trims whitespace
    /// from the build name.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceInName_TrimsName()
    {
        // Arrange & Act
        var build = RecommendedBuild.Create(
            "  Standard Warrior  ",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);

        // Assert
        build.Name.Should().Be("Standard Warrior");
    }

    /// <summary>
    /// Verifies that builds with minimum valid attribute values (all 1s) are accepted.
    /// </summary>
    [Test]
    public void Create_WithMinimumAttributes_Succeeds()
    {
        // Arrange & Act
        var build = RecommendedBuild.Create(
            "Minimum Build",
            might: 1, finesse: 1, wits: 1, will: 1, sturdiness: 1);

        // Assert
        build.TotalAttributePoints.Should().Be(5);
    }

    /// <summary>
    /// Verifies that builds with maximum valid attribute values (all 10s) are accepted.
    /// </summary>
    [Test]
    public void Create_WithMaximumAttributes_Succeeds()
    {
        // Arrange & Act
        var build = RecommendedBuild.Create(
            "Maximum Build",
            might: 10, finesse: 10, wits: 10, will: 10, sturdiness: 10);

        // Assert
        build.TotalAttributePoints.Should().Be(50);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS — Validation Failures
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.Create"/> throws
    /// <see cref="ArgumentException"/> when name is null.
    /// </summary>
    [Test]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => RecommendedBuild.Create(
            null!, might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.Create"/> throws
    /// <see cref="ArgumentException"/> when name is empty.
    /// </summary>
    [Test]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => RecommendedBuild.Create(
            "", might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.Create"/> throws
    /// <see cref="ArgumentException"/> when name is whitespace-only.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceOnlyName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => RecommendedBuild.Create(
            "   ", might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when Might is below minimum (0).
    /// </summary>
    [Test]
    public void Create_WithMightBelowMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => RecommendedBuild.Create(
            "Test", might: 0, finesse: 3, wits: 2, will: 2, sturdiness: 4);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("might");
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when Finesse exceeds maximum (11).
    /// </summary>
    [Test]
    public void Create_WithFinesseAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => RecommendedBuild.Create(
            "Test", might: 4, finesse: 11, wits: 2, will: 2, sturdiness: 4);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("finesse");
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when Wits is below minimum.
    /// </summary>
    [Test]
    public void Create_WithWitsBelowMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => RecommendedBuild.Create(
            "Test", might: 4, finesse: 3, wits: 0, will: 2, sturdiness: 4);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("wits");
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when Will exceeds maximum.
    /// </summary>
    [Test]
    public void Create_WithWillAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => RecommendedBuild.Create(
            "Test", might: 4, finesse: 3, wits: 2, will: 11, sturdiness: 4);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("will");
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.Create"/> throws
    /// <see cref="ArgumentOutOfRangeException"/> when Sturdiness is negative.
    /// </summary>
    [Test]
    public void Create_WithSturdinessBelowMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => RecommendedBuild.Create(
            "Test", might: 4, finesse: 3, wits: 2, will: 2, sturdiness: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("sturdiness");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS — TotalAttributePoints
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.TotalAttributePoints"/>
    /// returns 15 for the standard Warrior build (4+3+2+2+4).
    /// </summary>
    [Test]
    public void TotalAttributePoints_StandardWarrior_Returns15()
    {
        // Arrange
        var build = RecommendedBuild.Create(
            "Standard Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);

        // Act & Assert
        build.TotalAttributePoints.Should().Be(15);
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.TotalAttributePoints"/>
    /// returns 15 for the standard Skirmisher build (3+4+3+2+3).
    /// </summary>
    [Test]
    public void TotalAttributePoints_StandardSkirmisher_Returns15()
    {
        // Arrange
        var build = RecommendedBuild.Create(
            "Standard Skirmisher",
            might: 3, finesse: 4, wits: 3, will: 2, sturdiness: 3);

        // Act & Assert
        build.TotalAttributePoints.Should().Be(15);
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.TotalAttributePoints"/>
    /// returns 15 for the standard Mystic build (2+3+4+4+2).
    /// </summary>
    [Test]
    public void TotalAttributePoints_StandardMystic_Returns15()
    {
        // Arrange
        var build = RecommendedBuild.Create(
            "Standard Mystic",
            might: 2, finesse: 3, wits: 4, will: 4, sturdiness: 2);

        // Act & Assert
        build.TotalAttributePoints.Should().Be(15);
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.TotalAttributePoints"/>
    /// returns 14 for the standard Adept build (3+3+3+2+3).
    /// </summary>
    [Test]
    public void TotalAttributePoints_StandardAdept_Returns14()
    {
        // Arrange
        var build = RecommendedBuild.Create(
            "Standard Adept",
            might: 3, finesse: 3, wits: 3, will: 2, sturdiness: 3);

        // Act & Assert
        build.TotalAttributePoints.Should().Be(14);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS — HasOptimalLineage
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.HasOptimalLineage"/> returns
    /// false when no lineage is specified.
    /// </summary>
    [Test]
    public void HasOptimalLineage_WithNoLineage_ReturnsFalse()
    {
        // Arrange
        var build = RecommendedBuild.Create(
            "Standard Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);

        // Act & Assert
        build.HasOptimalLineage.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.HasOptimalLineage"/> returns
    /// true when a lineage is specified.
    /// </summary>
    [Test]
    public void HasOptimalLineage_WithLineage_ReturnsTrue()
    {
        // Arrange
        var build = RecommendedBuild.Create(
            "IronBlooded Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4,
            optimalLineage: Lineage.IronBlooded);

        // Act & Assert
        build.HasOptimalLineage.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that all four lineage values are accepted as OptimalLineage.
    /// </summary>
    [TestCase(Lineage.ClanBorn)]
    [TestCase(Lineage.RuneMarked)]
    [TestCase(Lineage.IronBlooded)]
    [TestCase(Lineage.VargrKin)]
    public void Create_WithEachLineage_SetsOptimalLineage(Lineage lineage)
    {
        // Arrange & Act
        var build = RecommendedBuild.Create(
            $"{lineage} Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4,
            optimalLineage: lineage);

        // Assert
        build.OptimalLineage.Should().Be(lineage);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY SUMMARY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.GetDisplaySummary"/> returns
    /// the expected format for a default (no lineage) build.
    /// </summary>
    [Test]
    public void GetDisplaySummary_DefaultBuild_ReturnsFormattedString()
    {
        // Arrange
        var build = RecommendedBuild.Create(
            "Standard Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);

        // Act
        var summary = build.GetDisplaySummary();

        // Assert
        summary.Should().Be("Standard Warrior: M4 F3 Wi2 Wl2 S4 (15 pts)");
    }

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.GetDisplaySummary"/> includes
    /// the optimal lineage information when present.
    /// </summary>
    [Test]
    public void GetDisplaySummary_WithLineage_IncludesLineageInfo()
    {
        // Arrange
        var build = RecommendedBuild.Create(
            "IronBlooded Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4,
            optimalLineage: Lineage.IronBlooded);

        // Act
        var summary = build.GetDisplaySummary();

        // Assert
        summary.Should().Contain("[Optimal: IronBlooded]");
        summary.Should().Contain("M4 F3 Wi2 Wl2 S4");
        summary.Should().Contain("15 pts");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="RecommendedBuild.ToString"/> returns
    /// the expected debug format.
    /// </summary>
    [Test]
    public void ToString_ReturnsDebugFormat()
    {
        // Arrange
        var build = RecommendedBuild.Create(
            "Standard Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);

        // Act
        var result = build.ToString();

        // Assert
        result.Should().Be("RecommendedBuild: Standard Warrior [M4 F3 Wi2 Wl2 S4]");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALUE EQUALITY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that two RecommendedBuild instances with identical values
    /// are considered equal (value equality from record struct).
    /// </summary>
    [Test]
    public void Equals_WithIdenticalValues_ReturnsTrue()
    {
        // Arrange
        var build1 = RecommendedBuild.Create(
            "Standard Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);
        var build2 = RecommendedBuild.Create(
            "Standard Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);

        // Act & Assert
        build1.Should().Be(build2);
    }

    /// <summary>
    /// Verifies that two RecommendedBuild instances with different values
    /// are not considered equal.
    /// </summary>
    [Test]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var build1 = RecommendedBuild.Create(
            "Standard Warrior",
            might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);
        var build2 = RecommendedBuild.Create(
            "Standard Mystic",
            might: 2, finesse: 3, wits: 4, will: 4, sturdiness: 2);

        // Act & Assert
        build1.Should().NotBe(build2);
    }
}
