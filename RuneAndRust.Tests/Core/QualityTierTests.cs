using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the QualityTier enum.
/// Validates the five quality tiers: JuryRigged, Scavenged, ClanForged, Optimized, MythForged.
/// </summary>
public class QualityTierTests
{
    [Fact]
    public void QualityTier_ShouldHaveExactlyFiveValues()
    {
        // Arrange
        var values = Enum.GetValues<QualityTier>();

        // Assert
        values.Should().HaveCount(5, "Rune & Rust has exactly five quality tiers");
    }

    [Fact]
    public void QualityTier_ShouldContain_JuryRigged()
    {
        // Assert
        Enum.IsDefined(typeof(QualityTier), QualityTier.JuryRigged).Should().BeTrue();
    }

    [Fact]
    public void QualityTier_ShouldContain_Scavenged()
    {
        // Assert
        Enum.IsDefined(typeof(QualityTier), QualityTier.Scavenged).Should().BeTrue();
    }

    [Fact]
    public void QualityTier_ShouldContain_ClanForged()
    {
        // Assert
        Enum.IsDefined(typeof(QualityTier), QualityTier.ClanForged).Should().BeTrue();
    }

    [Fact]
    public void QualityTier_ShouldContain_Optimized()
    {
        // Assert
        Enum.IsDefined(typeof(QualityTier), QualityTier.Optimized).Should().BeTrue();
    }

    [Fact]
    public void QualityTier_ShouldContain_MythForged()
    {
        // Assert
        Enum.IsDefined(typeof(QualityTier), QualityTier.MythForged).Should().BeTrue();
    }

    [Fact]
    public void QualityTier_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)QualityTier.JuryRigged).Should().Be(0);
        ((int)QualityTier.Scavenged).Should().Be(1);
        ((int)QualityTier.ClanForged).Should().Be(2);
        ((int)QualityTier.Optimized).Should().Be(3);
        ((int)QualityTier.MythForged).Should().Be(4);
    }

    [Theory]
    [InlineData(QualityTier.JuryRigged, "JuryRigged")]
    [InlineData(QualityTier.Scavenged, "Scavenged")]
    [InlineData(QualityTier.ClanForged, "ClanForged")]
    [InlineData(QualityTier.Optimized, "Optimized")]
    [InlineData(QualityTier.MythForged, "MythForged")]
    public void QualityTier_ToString_ReturnsExpectedName(QualityTier qualityTier, string expectedName)
    {
        // Assert
        qualityTier.ToString().Should().Be(expectedName);
    }

    [Fact]
    public void QualityTier_Order_ShouldRepresentIncreasingQuality()
    {
        // Arrange & Act
        var tiers = new[] { QualityTier.JuryRigged, QualityTier.Scavenged, QualityTier.ClanForged, QualityTier.Optimized, QualityTier.MythForged };

        // Assert - each subsequent value should be higher than the previous
        for (int i = 1; i < tiers.Length; i++)
        {
            ((int)tiers[i]).Should().BeGreaterThan((int)tiers[i - 1]);
        }
    }
}
