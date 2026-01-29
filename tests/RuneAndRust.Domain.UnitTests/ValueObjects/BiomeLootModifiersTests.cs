using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="BiomeLootModifiers"/> value object.
/// </summary>
/// <remarks>
/// Tests cover modifier application methods, validation, edge cases, and the default modifier behavior.
/// </remarks>
[TestFixture]
public class BiomeLootModifiersTests
{
    #region ApplyToGold Tests

    /// <summary>
    /// Verifies that gold is correctly scaled by the multiplier.
    /// </summary>
    [Test]
    public void ApplyToGold_WithMultiplier_ScalesCorrectly()
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "alfheim",
            goldMultiplier: 1.5m,
            dropRateMultiplier: 1.0m,
            qualityBonus: 0,
            rareChanceBonusPercent: 0);

        // Act
        var result = modifiers.ApplyToGold(100);

        // Assert
        result.Should().Be(150, because: "100 * 1.5 = 150");
    }

    /// <summary>
    /// Verifies that gold scaling floors fractional results.
    /// </summary>
    [Test]
    public void ApplyToGold_WithFractionalResult_FloorsValue()
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "muspelheim",
            goldMultiplier: 1.2m,
            dropRateMultiplier: 1.0m,
            qualityBonus: 0,
            rareChanceBonusPercent: 0);

        // Act
        var result = modifiers.ApplyToGold(33);

        // Assert
        result.Should().Be(39, because: "33 * 1.2 = 39.6 → floors to 39");
    }

    /// <summary>
    /// Verifies that zero or negative gold returns zero.
    /// </summary>
    [TestCase(0)]
    [TestCase(-10)]
    public void ApplyToGold_WithZeroOrNegative_ReturnsZero(int baseGold)
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "jotunheim",
            goldMultiplier: 2.0m,
            dropRateMultiplier: 1.0m,
            qualityBonus: 0,
            rareChanceBonusPercent: 0);

        // Act
        var result = modifiers.ApplyToGold(baseGold);

        // Assert
        result.Should().Be(0, because: "zero or negative base gold should return 0");
    }

    #endregion

    #region ApplyToItemCount Tests

    /// <summary>
    /// Verifies that item counts are correctly reduced by drop rate multiplier.
    /// </summary>
    [Test]
    public void ApplyToItemCount_WithReducedDropRate_FloorsResult()
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "alfheim",
            goldMultiplier: 1.0m,
            dropRateMultiplier: 0.7m,
            qualityBonus: 0,
            rareChanceBonusPercent: 0);

        // Act
        var result = modifiers.ApplyToItemCount(baseCount: 5, minCount: 1);

        // Assert
        result.Should().Be(3, because: "5 * 0.7 = 3.5 → floors to 3");
    }

    /// <summary>
    /// Verifies that item count respects minimum value.
    /// </summary>
    [Test]
    public void ApplyToItemCount_BelowMinimum_ReturnsMinimum()
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "harsh-wasteland",
            goldMultiplier: 1.0m,
            dropRateMultiplier: 0.1m,
            qualityBonus: 0,
            rareChanceBonusPercent: 0);

        // Act
        var result = modifiers.ApplyToItemCount(baseCount: 3, minCount: 1);

        // Assert
        result.Should().Be(1, because: "3 * 0.1 = 0.3 → 0, but minimum is 1");
    }

    /// <summary>
    /// Verifies that zero or negative base count returns minimum.
    /// </summary>
    [TestCase(0)]
    [TestCase(-5)]
    public void ApplyToItemCount_WithZeroOrNegativeBase_ReturnsMinimum(int baseCount)
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "void",
            goldMultiplier: 1.0m,
            dropRateMultiplier: 1.5m,
            qualityBonus: 0,
            rareChanceBonusPercent: 0);

        // Act
        var result = modifiers.ApplyToItemCount(baseCount: baseCount, minCount: 2);

        // Assert
        result.Should().Be(2, because: "zero or negative base should return minimum");
    }

    #endregion

    #region ApplyToTier Tests

    /// <summary>
    /// Verifies that quality bonus correctly shifts tier upward.
    /// </summary>
    [Test]
    public void ApplyToTier_WithBonus_ShiftsTierUp()
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "alfheim",
            goldMultiplier: 1.0m,
            dropRateMultiplier: 1.0m,
            qualityBonus: 1,
            rareChanceBonusPercent: 0);

        // Act
        var result = modifiers.ApplyToTier(baseTier: 2, maxTier: 4);

        // Assert
        result.Should().Be(3, because: "2 + 1 = 3");
    }

    /// <summary>
    /// Verifies that tier bonus is capped at maximum tier.
    /// </summary>
    [Test]
    public void ApplyToTier_WithBonus_CapsAtMaxTier()
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "jotunheim",
            goldMultiplier: 1.0m,
            dropRateMultiplier: 1.0m,
            qualityBonus: 2,
            rareChanceBonusPercent: 0);

        // Act
        var result = modifiers.ApplyToTier(baseTier: 3, maxTier: 4);

        // Assert
        result.Should().Be(4, because: "3 + 2 = 5, but capped at maxTier 4");
    }

    /// <summary>
    /// Verifies that zero bonus does not change tier.
    /// </summary>
    [Test]
    public void ApplyToTier_WithZeroBonus_ReturnsBaseTier()
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "the-roots",
            goldMultiplier: 1.0m,
            dropRateMultiplier: 1.0m,
            qualityBonus: 0,
            rareChanceBonusPercent: 0);

        // Act
        var result = modifiers.ApplyToTier(baseTier: 2, maxTier: 4);

        // Assert
        result.Should().Be(2, because: "no bonus means no change");
    }

    #endregion

    #region GetRareChanceBonus Tests

    /// <summary>
    /// Verifies that rare chance bonus is converted to decimal correctly.
    /// </summary>
    [Test]
    public void GetRareChanceBonus_ReturnsDecimalFraction()
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "boss-room",
            goldMultiplier: 1.0m,
            dropRateMultiplier: 1.0m,
            qualityBonus: 0,
            rareChanceBonusPercent: 25);

        // Act
        var result = modifiers.GetRareChanceBonus();

        // Assert
        result.Should().Be(0.25m, because: "25% = 0.25");
    }

    /// <summary>
    /// Verifies that zero rare chance bonus returns zero.
    /// </summary>
    [Test]
    public void GetRareChanceBonus_WithZero_ReturnsZero()
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "the-roots",
            goldMultiplier: 1.0m,
            dropRateMultiplier: 1.0m,
            qualityBonus: 0,
            rareChanceBonusPercent: 0);

        // Act
        var result = modifiers.GetRareChanceBonus();

        // Assert
        result.Should().Be(0m, because: "0% = 0");
    }

    #endregion

    #region Default Tests

    /// <summary>
    /// Verifies that default modifiers apply no changes to any values.
    /// </summary>
    [Test]
    public void Default_ReturnsNoModification()
    {
        // Arrange
        var defaultMod = BiomeLootModifiers.Default;

        // Act & Assert
        defaultMod.ApplyToGold(100).Should().Be(100, because: "default gold multiplier is 1.0");
        defaultMod.ApplyToItemCount(5, 0).Should().Be(5, because: "default drop rate is 1.0");
        defaultMod.ApplyToTier(2, 4).Should().Be(2, because: "default quality bonus is 0");
        defaultMod.GetRareChanceBonus().Should().Be(0m, because: "default rare bonus is 0");
    }

    /// <summary>
    /// Verifies that default has correct property values.
    /// </summary>
    [Test]
    public void Default_HasCorrectPropertyValues()
    {
        // Arrange
        var defaultMod = BiomeLootModifiers.Default;

        // Assert
        defaultMod.BiomeId.Should().Be("default");
        defaultMod.GoldMultiplier.Should().Be(1.0m);
        defaultMod.DropRateMultiplier.Should().Be(1.0m);
        defaultMod.QualityBonus.Should().Be(0);
        defaultMod.RareChanceBonusPercent.Should().Be(0);
    }

    /// <summary>
    /// Verifies computed properties for default modifiers.
    /// </summary>
    [Test]
    public void Default_ComputedProperties_AllFalse()
    {
        // Arrange
        var defaultMod = BiomeLootModifiers.Default;

        // Assert
        defaultMod.IncreasesGold.Should().BeFalse();
        defaultMod.ReducesDropRate.Should().BeFalse();
        defaultMod.ImprovesQuality.Should().BeFalse();
        defaultMod.IncreasesRareChance.Should().BeFalse();
    }

    #endregion

    #region Computed Properties Tests

    /// <summary>
    /// Verifies that computed properties correctly reflect modifier values.
    /// </summary>
    [Test]
    public void ComputedProperties_WithActiveModifiers_ReturnTrue()
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "alfheim",
            goldMultiplier: 1.5m,
            dropRateMultiplier: 0.7m,
            qualityBonus: 1,
            rareChanceBonusPercent: 15);

        // Assert
        modifiers.IncreasesGold.Should().BeTrue(because: "1.5 > 1.0");
        modifiers.ReducesDropRate.Should().BeTrue(because: "0.7 < 1.0");
        modifiers.ImprovesQuality.Should().BeTrue(because: "1 > 0");
        modifiers.IncreasesRareChance.Should().BeTrue(because: "15 > 0");
    }

    #endregion

    #region Create Validation Tests

    /// <summary>
    /// Verifies that Create normalizes biome ID to lowercase.
    /// </summary>
    [Test]
    public void Create_WithMixedCaseBiomeId_NormalizesToLowercase()
    {
        // Act
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "Muspelheim",
            goldMultiplier: 1.0m,
            dropRateMultiplier: 1.0m,
            qualityBonus: 0,
            rareChanceBonusPercent: 0);

        // Assert
        modifiers.BiomeId.Should().Be("muspelheim");
    }

    /// <summary>
    /// Verifies that Create throws for null or whitespace biome ID.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithInvalidBiomeId_ThrowsArgumentException(string? biomeId)
    {
        // Act
        var act = () => BiomeLootModifiers.Create(
            biomeId: biomeId!,
            goldMultiplier: 1.0m,
            dropRateMultiplier: 1.0m,
            qualityBonus: 0,
            rareChanceBonusPercent: 0);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws for zero or negative gold multiplier.
    /// </summary>
    [TestCase(0)]
    [TestCase(-1.0)]
    public void Create_WithInvalidGoldMultiplier_ThrowsArgumentOutOfRangeException(decimal goldMultiplier)
    {
        // Act
        var act = () => BiomeLootModifiers.Create(
            biomeId: "test",
            goldMultiplier: goldMultiplier,
            dropRateMultiplier: 1.0m,
            qualityBonus: 0,
            rareChanceBonusPercent: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws for quality bonus outside 0-4 range.
    /// </summary>
    [TestCase(-1)]
    [TestCase(5)]
    public void Create_WithInvalidQualityBonus_ThrowsArgumentOutOfRangeException(int qualityBonus)
    {
        // Act
        var act = () => BiomeLootModifiers.Create(
            biomeId: "test",
            goldMultiplier: 1.0m,
            dropRateMultiplier: 1.0m,
            qualityBonus: qualityBonus,
            rareChanceBonusPercent: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create throws for rare chance bonus outside 0-100 range.
    /// </summary>
    [TestCase(-1)]
    [TestCase(101)]
    public void Create_WithInvalidRareChanceBonus_ThrowsArgumentOutOfRangeException(int rareChanceBonusPercent)
    {
        // Act
        var act = () => BiomeLootModifiers.Create(
            biomeId: "test",
            goldMultiplier: 1.0m,
            dropRateMultiplier: 1.0m,
            qualityBonus: 0,
            rareChanceBonusPercent: rareChanceBonusPercent);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region ToString Tests

    /// <summary>
    /// Verifies that ToString produces a readable format.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var modifiers = BiomeLootModifiers.Create(
            biomeId: "alfheim",
            goldMultiplier: 1.5m,
            dropRateMultiplier: 0.7m,
            qualityBonus: 1,
            rareChanceBonusPercent: 15);

        // Act
        var result = modifiers.ToString();

        // Assert
        result.Should().Contain("[alfheim]");
        result.Should().Contain("Gold: ×1.5");
        result.Should().Contain("Drop: ×0.7");
        result.Should().Contain("Tier: +1");
        result.Should().Contain("Rare: +15%");
    }

    #endregion
}
