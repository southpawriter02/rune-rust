using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="QualityTier"/> enum.
/// </summary>
[TestFixture]
public class QualityTierTests
{
    /// <summary>
    /// Verifies that all five expected quality tiers are defined.
    /// </summary>
    [Test]
    public void QualityTier_ContainsAllFiveTiers()
    {
        // Assert
        Enum.GetValues<QualityTier>().Should().HaveCount(5);
        Enum.IsDefined(QualityTier.JuryRigged).Should().BeTrue();
        Enum.IsDefined(QualityTier.Scavenged).Should().BeTrue();
        Enum.IsDefined(QualityTier.ClanForged).Should().BeTrue();
        Enum.IsDefined(QualityTier.Optimized).Should().BeTrue();
        Enum.IsDefined(QualityTier.MythForged).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that each tier has the expected integer value per specification.
    /// </summary>
    [Test]
    [TestCase(QualityTier.JuryRigged, 0)]
    [TestCase(QualityTier.Scavenged, 1)]
    [TestCase(QualityTier.ClanForged, 2)]
    [TestCase(QualityTier.Optimized, 3)]
    [TestCase(QualityTier.MythForged, 4)]
    public void QualityTier_HasExpectedIntegerValue(QualityTier tier, int expectedValue)
    {
        // Assert
        ((int)tier).Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies that higher tiers compare as greater than lower tiers.
    /// </summary>
    [Test]
    public void QualityTier_HigherTiersAreGreaterThanLowerTiers()
    {
        // Assert - compare integer values since FluentAssertions doesn't support BeGreaterThan for enums
        ((int)QualityTier.Scavenged).Should().BeGreaterThan((int)QualityTier.JuryRigged);
        ((int)QualityTier.ClanForged).Should().BeGreaterThan((int)QualityTier.Scavenged);
        ((int)QualityTier.Optimized).Should().BeGreaterThan((int)QualityTier.ClanForged);
        ((int)QualityTier.MythForged).Should().BeGreaterThan((int)QualityTier.Optimized);
    }

    /// <summary>
    /// Verifies that tier strings can be parsed case-insensitively.
    /// </summary>
    [Test]
    [TestCase("JuryRigged", QualityTier.JuryRigged)]
    [TestCase("juryrigged", QualityTier.JuryRigged)]
    [TestCase("JURYRIGGED", QualityTier.JuryRigged)]
    [TestCase("Scavenged", QualityTier.Scavenged)]
    [TestCase("ClanForged", QualityTier.ClanForged)]
    [TestCase("Optimized", QualityTier.Optimized)]
    [TestCase("MythForged", QualityTier.MythForged)]
    public void TryParse_ValidTierName_ReturnsTrue(string input, QualityTier expected)
    {
        // Act
        var success = Enum.TryParse<QualityTier>(input, ignoreCase: true, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that invalid tier names fail to parse.
    /// </summary>
    [Test]
    [TestCase("Invalid")]
    [TestCase("Legendary")]
    [TestCase("Common")]
    [TestCase("")]
    public void TryParse_InvalidTierName_ReturnsFalse(string input)
    {
        // Act
        var success = Enum.TryParse<QualityTier>(input, ignoreCase: true, out _);

        // Assert
        success.Should().BeFalse();
    }
}
