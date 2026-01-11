using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for seed string conversion utilities.
/// </summary>
[TestFixture]
public class SeededRandomServiceSeedStringTests
{
    [Test]
    public void ToSeedString_ReturnsEightCharacters()
    {
        // Act
        var result = SeedStringUtility.ToSeedString(305419896);

        // Assert
        result.Should().HaveLength(8);
    }

    [Test]
    public void ToSeedString_UsesOnlyValidCharacters()
    {
        // Arrange
        const string validChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        // Act
        var result = SeedStringUtility.ToSeedString(12345678);

        // Assert
        result.ToUpperInvariant().All(c => validChars.Contains(c)).Should().BeTrue();
    }

    [Test]
    public void FromSeedString_RoundTrips_WithToSeedString()
    {
        // Arrange
        var testSeeds = new[] { 0, 1, 31, 32, 305419896, int.MaxValue, -1 };

        foreach (var seed in testSeeds)
        {
            // Act
            var seedString = SeedStringUtility.ToSeedString(seed);
            var recovered = SeedStringUtility.FromSeedString(seedString);

            // Assert
            recovered.Should().Be(seed, because: $"seed {seed} should round-trip through string '{seedString}'");
        }
    }

    [Test]
    public void FromSeedString_InvalidCharacter_ThrowsArgumentException()
    {
        // Arrange
        var invalidSeedString = "ABCD0123"; // Contains '0' and '1' which are invalid

        // Act
        var act = () => SeedStringUtility.FromSeedString(invalidSeedString);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void FromSeedString_WrongLength_ThrowsArgumentException()
    {
        // Arrange
        var shortString = "ABC";
        var longString = "ABCDEFGHIJ";

        // Act & Assert
        ((Action)(() => SeedStringUtility.FromSeedString(shortString))).Should().Throw<ArgumentException>();
        ((Action)(() => SeedStringUtility.FromSeedString(longString))).Should().Throw<ArgumentException>();
    }

    [Test]
    public void IsValidSeedString_ValidString_ReturnsTrue()
    {
        // Arrange
        var validString = SeedStringUtility.ToSeedString(12345678);

        // Act & Assert
        SeedStringUtility.IsValidSeedString(validString).Should().BeTrue();
    }

    [Test]
    public void IsValidSeedString_InvalidString_ReturnsFalse()
    {
        // Assert
        SeedStringUtility.IsValidSeedString("").Should().BeFalse();
        SeedStringUtility.IsValidSeedString("ABC").Should().BeFalse();
        SeedStringUtility.IsValidSeedString("ABCD0123").Should().BeFalse();
        SeedStringUtility.IsValidSeedString(null!).Should().BeFalse();
    }
}
