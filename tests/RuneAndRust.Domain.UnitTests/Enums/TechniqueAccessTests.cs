using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="TechniqueAccess"/> enum.
/// </summary>
[TestFixture]
public class TechniqueAccessTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Enum Value Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that all four expected technique access tiers are defined.
    /// </summary>
    [Test]
    public void TechniqueAccess_ContainsAllFourTiers()
    {
        // Assert
        Enum.GetValues<TechniqueAccess>().Should().HaveCount(4);
        Enum.IsDefined(TechniqueAccess.None).Should().BeTrue();
        Enum.IsDefined(TechniqueAccess.Basic).Should().BeTrue();
        Enum.IsDefined(TechniqueAccess.Advanced).Should().BeTrue();
        Enum.IsDefined(TechniqueAccess.Signature).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that each tier has the expected integer value per specification.
    /// </summary>
    [Test]
    [TestCase(TechniqueAccess.None, 0)]
    [TestCase(TechniqueAccess.Basic, 1)]
    [TestCase(TechniqueAccess.Advanced, 2)]
    [TestCase(TechniqueAccess.Signature, 3)]
    public void TechniqueAccess_HasExpectedIntegerValue(
        TechniqueAccess tier, int expectedValue)
    {
        // Assert
        ((int)tier).Should().Be(expectedValue);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Comparison Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that higher tiers compare as greater than lower tiers.
    /// </summary>
    [Test]
    public void TechniqueAccess_HigherTiersAreGreaterThanLowerTiers()
    {
        // Assert
        ((int)TechniqueAccess.Basic)
            .Should().BeGreaterThan((int)TechniqueAccess.None);
        ((int)TechniqueAccess.Advanced)
            .Should().BeGreaterThan((int)TechniqueAccess.Basic);
        ((int)TechniqueAccess.Signature)
            .Should().BeGreaterThan((int)TechniqueAccess.Advanced);
    }

    /// <summary>
    /// Verifies that technique tiers can be compared using comparison operators.
    /// This is critical for cumulative access checks (e.g., "has at least Basic access").
    /// </summary>
    [Test]
    public void TechniqueAccess_CanCompareUsingOperators()
    {
        // Assert - signature includes all
        (TechniqueAccess.Signature >= TechniqueAccess.Basic).Should().BeTrue();
        (TechniqueAccess.Signature >= TechniqueAccess.Advanced).Should().BeTrue();
        // Check that signature meets its own threshold (cast to int to avoid CS1718)
        ((int)TechniqueAccess.Signature >= (int)TechniqueAccess.Signature).Should().BeTrue();

        // Assert - advanced includes basic but not signature
        (TechniqueAccess.Advanced >= TechniqueAccess.Basic).Should().BeTrue();
        (TechniqueAccess.Advanced >= TechniqueAccess.Signature).Should().BeFalse();

        // Assert - none has nothing
        (TechniqueAccess.None >= TechniqueAccess.Basic).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Parsing Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that tier strings can be parsed case-insensitively.
    /// </summary>
    [Test]
    [TestCase("None", TechniqueAccess.None)]
    [TestCase("none", TechniqueAccess.None)]
    [TestCase("NONE", TechniqueAccess.None)]
    [TestCase("Basic", TechniqueAccess.Basic)]
    [TestCase("Advanced", TechniqueAccess.Advanced)]
    [TestCase("Signature", TechniqueAccess.Signature)]
    public void TryParse_ValidTierName_ReturnsTrue(
        string input, TechniqueAccess expected)
    {
        // Act
        var success = Enum.TryParse<TechniqueAccess>(input, ignoreCase: true, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that invalid tier names fail to parse.
    /// </summary>
    [Test]
    [TestCase("Invalid")]
    [TestCase("Expert")]
    [TestCase("Master")]
    [TestCase("")]
    public void TryParse_InvalidTierName_ReturnsFalse(string input)
    {
        // Act
        var success = Enum.TryParse<TechniqueAccess>(input, ignoreCase: true, out _);

        // Assert
        success.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Proficiency Level Mapping Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that technique access tiers map to expected proficiency levels.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.NonProficient, TechniqueAccess.None)]
    [TestCase(WeaponProficiencyLevel.Proficient, TechniqueAccess.Basic)]
    [TestCase(WeaponProficiencyLevel.Expert, TechniqueAccess.Advanced)]
    [TestCase(WeaponProficiencyLevel.Master, TechniqueAccess.Signature)]
    public void TechniqueAccess_MapsToExpectedProficiencyLevel(
        WeaponProficiencyLevel level, TechniqueAccess expectedAccess)
    {
        // Arrange - integer values should match
        var levelValue = (int)level;
        var accessValue = (int)expectedAccess;

        // Assert - values align for easy mapping
        levelValue.Should().Be(accessValue);
    }
}
