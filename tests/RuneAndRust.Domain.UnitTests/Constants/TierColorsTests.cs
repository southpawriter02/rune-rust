namespace RuneAndRust.Domain.UnitTests.Constants;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for the <see cref="TierColors"/> static class.
/// </summary>
/// <remarks>
/// These tests verify:
/// <list type="bullet">
///   <item><description>All tier color constants are defined correctly</description></item>
///   <item><description>GetColor returns correct hex for each tier</description></item>
///   <item><description>GetColorByIndex returns correct hex for indices 0-4</description></item>
///   <item><description>GetColorName returns human-readable color names</description></item>
///   <item><description>GetTierPrefix returns correctly formatted prefixes</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class TierColorsTests
{
    #region Color Constants Tests

    /// <summary>
    /// Verifies that the JuryRigged color constant is gray (#808080).
    /// </summary>
    [Test]
    public void JuryRiggedConstant_IsGrayColor()
    {
        // Assert
        TierColors.JuryRigged.Should().Be("#808080");
    }

    /// <summary>
    /// Verifies that the Scavenged color constant is white (#FFFFFF).
    /// </summary>
    [Test]
    public void ScavengedConstant_IsWhiteColor()
    {
        // Assert
        TierColors.Scavenged.Should().Be("#FFFFFF");
    }

    /// <summary>
    /// Verifies that the ClanForged color constant is green (#00FF00).
    /// </summary>
    [Test]
    public void ClanForgedConstant_IsGreenColor()
    {
        // Assert
        TierColors.ClanForged.Should().Be("#00FF00");
    }

    /// <summary>
    /// Verifies that the Optimized color constant is purple (#800080).
    /// </summary>
    [Test]
    public void OptimizedConstant_IsPurpleColor()
    {
        // Assert
        TierColors.Optimized.Should().Be("#800080");
    }

    /// <summary>
    /// Verifies that the MythForged color constant is gold (#FFD700).
    /// </summary>
    [Test]
    public void MythForgedConstant_IsGoldColor()
    {
        // Assert
        TierColors.MythForged.Should().Be("#FFD700");
    }

    /// <summary>
    /// Verifies that the LegendaryBorder color constant matches MythForged.
    /// </summary>
    [Test]
    public void LegendaryBorderConstant_MatchesMythForgedColor()
    {
        // Assert
        TierColors.LegendaryBorder.Should().Be(TierColors.MythForged);
    }

    /// <summary>
    /// Verifies that the EffectAccent color constant is orange-gold.
    /// </summary>
    [Test]
    public void EffectAccentConstant_IsOrangeGoldColor()
    {
        // Assert
        TierColors.EffectAccent.Should().Be("#FFAA00");
    }

    #endregion

    #region GetColor Tests

    /// <summary>
    /// Verifies that GetColor returns the correct hex color for each quality tier.
    /// </summary>
    [Test]
    [TestCase(QualityTier.JuryRigged, "#808080")]
    [TestCase(QualityTier.Scavenged, "#FFFFFF")]
    [TestCase(QualityTier.ClanForged, "#00FF00")]
    [TestCase(QualityTier.Optimized, "#800080")]
    [TestCase(QualityTier.MythForged, "#FFD700")]
    public void GetColor_ReturnCorrectHexForTier(QualityTier tier, string expected)
    {
        // Act
        var result = TierColors.GetColor(tier);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that GetColor returns the default (Scavenged) for unknown tiers.
    /// </summary>
    [Test]
    public void GetColor_ForUnknownTier_ReturnsScavenged()
    {
        // Arrange - Cast an invalid value
        var unknownTier = (QualityTier)99;

        // Act
        var result = TierColors.GetColor(unknownTier);

        // Assert
        result.Should().Be(TierColors.Scavenged);
    }

    #endregion

    #region GetColorByIndex Tests

    /// <summary>
    /// Verifies that GetColorByIndex returns correct colors for valid indices.
    /// </summary>
    [Test]
    [TestCase(0, "#808080")]
    [TestCase(1, "#FFFFFF")]
    [TestCase(2, "#00FF00")]
    [TestCase(3, "#800080")]
    [TestCase(4, "#FFD700")]
    public void GetColorByIndex_ReturnsCorrectColor(int index, string expected)
    {
        // Act
        var result = TierColors.GetColorByIndex(index);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that GetColorByIndex returns Scavenged for negative index.
    /// </summary>
    [Test]
    public void GetColorByIndex_NegativeIndex_ReturnsScavenged()
    {
        // Act
        var result = TierColors.GetColorByIndex(-1);

        // Assert
        result.Should().Be(TierColors.Scavenged);
    }

    /// <summary>
    /// Verifies that GetColorByIndex returns Scavenged for index above 4.
    /// </summary>
    [Test]
    public void GetColorByIndex_IndexAboveFour_ReturnsScavenged()
    {
        // Act
        var result = TierColors.GetColorByIndex(5);

        // Assert
        result.Should().Be(TierColors.Scavenged);
    }

    #endregion

    #region GetColorName Tests

    /// <summary>
    /// Verifies that GetColorName returns correct color names for each tier.
    /// </summary>
    [Test]
    [TestCase(QualityTier.JuryRigged, "Gray")]
    [TestCase(QualityTier.Scavenged, "White")]
    [TestCase(QualityTier.ClanForged, "Green")]
    [TestCase(QualityTier.Optimized, "Purple")]
    [TestCase(QualityTier.MythForged, "Gold")]
    public void GetColorName_ReturnsCorrectName(QualityTier tier, string expectedName)
    {
        // Act
        var result = TierColors.GetColorName(tier);

        // Assert
        result.Should().Be(expectedName);
    }

    /// <summary>
    /// Verifies that GetColorName returns "White" for unknown tiers.
    /// </summary>
    [Test]
    public void GetColorName_ForUnknownTier_ReturnsWhite()
    {
        // Arrange
        var unknownTier = (QualityTier)99;

        // Act
        var result = TierColors.GetColorName(unknownTier);

        // Assert
        result.Should().Be("White");
    }

    #endregion

    #region GetTierPrefix Tests

    /// <summary>
    /// Verifies that GetTierPrefix returns correct prefixes for each tier.
    /// </summary>
    [Test]
    [TestCase(QualityTier.JuryRigged, "[Jury-Rigged]")]
    [TestCase(QualityTier.Scavenged, "[Scavenged]")]
    [TestCase(QualityTier.ClanForged, "[Clan-Forged]")]
    [TestCase(QualityTier.Optimized, "[Optimized]")]
    [TestCase(QualityTier.MythForged, "[Myth-Forged]")]
    public void GetTierPrefix_ReturnsCorrectPrefix(QualityTier tier, string expectedPrefix)
    {
        // Act
        var result = TierColors.GetTierPrefix(tier);

        // Assert
        result.Should().Be(expectedPrefix);
    }

    /// <summary>
    /// Verifies that GetTierPrefix returns empty string for unknown tiers.
    /// </summary>
    [Test]
    public void GetTierPrefix_ForUnknownTier_ReturnsEmptyString()
    {
        // Arrange
        var unknownTier = (QualityTier)99;

        // Act
        var result = TierColors.GetTierPrefix(unknownTier);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetTierPrefix for MythForged starts with bracket.
    /// </summary>
    [Test]
    public void GetTierPrefix_ForMythForged_StartsWithBracket()
    {
        // Act
        var result = TierColors.GetTierPrefix(QualityTier.MythForged);

        // Assert
        result.Should().StartWith("[");
        result.Should().EndWith("]");
        result.Should().Contain("Myth");
    }

    #endregion
}
