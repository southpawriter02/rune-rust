using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the LootTables static data.
/// Validates table completeness, weight distributions, and template validity.
/// </summary>
public class LootTablesTests
{
    #region Quality Weight Tests

    [Fact]
    public void QualityWeightsByDanger_ContainsAllDangerLevels()
    {
        // Assert
        var allLevels = Enum.GetValues<DangerLevel>();
        foreach (var level in allLevels)
        {
            LootTables.QualityWeightsByDanger.Should().ContainKey(level,
                $"weights should be defined for {level}");
        }
    }

    [Theory]
    [InlineData(DangerLevel.Safe)]
    [InlineData(DangerLevel.Unstable)]
    [InlineData(DangerLevel.Hostile)]
    [InlineData(DangerLevel.Lethal)]
    public void QualityWeightsByDanger_EachLevel_SumsToOneHundred(DangerLevel level)
    {
        // Act
        var weights = LootTables.QualityWeightsByDanger[level];
        var sum = weights.Values.Sum();

        // Assert
        sum.Should().Be(100, $"weights for {level} should sum to 100, got {sum}");
    }

    [Theory]
    [InlineData(DangerLevel.Safe)]
    [InlineData(DangerLevel.Unstable)]
    [InlineData(DangerLevel.Hostile)]
    [InlineData(DangerLevel.Lethal)]
    public void QualityWeightsByDanger_EachLevel_ContainsAllTiers(DangerLevel level)
    {
        // Act
        var weights = LootTables.QualityWeightsByDanger[level];

        // Assert
        var allTiers = Enum.GetValues<QualityTier>();
        foreach (var tier in allTiers)
        {
            weights.Should().ContainKey(tier,
                $"{level} should have a weight for {tier}");
        }
    }

    [Fact]
    public void QualityWeightsByDanger_HigherDanger_BetterQualityChance()
    {
        // Get MythForged chances
        var safeMythForged = LootTables.QualityWeightsByDanger[DangerLevel.Safe][QualityTier.MythForged];
        var lethalMythForged = LootTables.QualityWeightsByDanger[DangerLevel.Lethal][QualityTier.MythForged];

        // Assert
        lethalMythForged.Should().BeGreaterThan(safeMythForged);
    }

    #endregion

    #region Item Count Range Tests

    [Fact]
    public void ItemCountsByDanger_ContainsAllDangerLevels()
    {
        var allLevels = Enum.GetValues<DangerLevel>();
        foreach (var level in allLevels)
        {
            LootTables.ItemCountsByDanger.Should().ContainKey(level);
        }
    }

    [Theory]
    [InlineData(DangerLevel.Safe, 1, 2)]
    [InlineData(DangerLevel.Unstable, 1, 3)]
    [InlineData(DangerLevel.Hostile, 2, 4)]
    [InlineData(DangerLevel.Lethal, 2, 5)]
    public void ItemCountsByDanger_HasCorrectRanges(DangerLevel level, int expectedMin, int expectedMax)
    {
        // Act
        var (min, max) = LootTables.ItemCountsByDanger[level];

        // Assert
        min.Should().Be(expectedMin);
        max.Should().Be(expectedMax);
    }

    [Fact]
    public void ItemCountsByDanger_RangesAreValid()
    {
        foreach (var (level, range) in LootTables.ItemCountsByDanger)
        {
            range.Min.Should().BePositive($"{level} min should be positive");
            range.Max.Should().BeGreaterThanOrEqualTo(range.Min,
                $"{level} max should be >= min");
        }
    }

    #endregion

    #region Weapon Template Tests

    [Fact]
    public void WeaponsByQuality_ContainsMultipleQualityTiers()
    {
        LootTables.WeaponsByQuality.Should().ContainKey(QualityTier.JuryRigged);
        LootTables.WeaponsByQuality.Should().ContainKey(QualityTier.Scavenged);
        LootTables.WeaponsByQuality.Should().ContainKey(QualityTier.ClanForged);
        LootTables.WeaponsByQuality.Should().ContainKey(QualityTier.Optimized);
        LootTables.WeaponsByQuality.Should().ContainKey(QualityTier.MythForged);
    }

    [Fact]
    public void WeaponsByQuality_AllTemplatesHaveValidData()
    {
        foreach (var (tier, templates) in LootTables.WeaponsByQuality)
        {
            templates.Should().NotBeEmpty($"{tier} should have weapon templates");

            foreach (var template in templates)
            {
                template.Name.Should().NotBeNullOrEmpty();
                template.Description.Should().NotBeNullOrEmpty();
                template.DamageDie.Should().BePositive($"{template.Name} damage die should be positive");
                template.Weight.Should().BePositive($"{template.Name} weight should be positive");
                template.Value.Should().BePositive($"{template.Name} value should be positive");
            }
        }
    }

    [Fact]
    public void WeaponsByQuality_HigherTiers_HaveHigherDamageDice()
    {
        var juryRiggedAvg = LootTables.WeaponsByQuality[QualityTier.JuryRigged]
            .Average(w => w.DamageDie);
        var mythForgedAvg = LootTables.WeaponsByQuality[QualityTier.MythForged]
            .Average(w => w.DamageDie);

        mythForgedAvg.Should().BeGreaterThan(juryRiggedAvg);
    }

    #endregion

    #region Armor Template Tests

    [Fact]
    public void ArmorByQuality_ContainsMultipleQualityTiers()
    {
        LootTables.ArmorByQuality.Should().ContainKey(QualityTier.JuryRigged);
        LootTables.ArmorByQuality.Should().ContainKey(QualityTier.Scavenged);
        LootTables.ArmorByQuality.Should().ContainKey(QualityTier.ClanForged);
    }

    [Fact]
    public void ArmorByQuality_AllTemplatesHaveValidData()
    {
        foreach (var (tier, templates) in LootTables.ArmorByQuality)
        {
            templates.Should().NotBeEmpty($"{tier} should have armor templates");

            foreach (var template in templates)
            {
                template.Name.Should().NotBeNullOrEmpty();
                template.Description.Should().NotBeNullOrEmpty();
                template.SoakBonus.Should().BeGreaterThanOrEqualTo(0);
                template.Weight.Should().BePositive();
                template.Value.Should().BePositive();
            }
        }
    }

    #endregion

    #region Consumable Template Tests

    [Fact]
    public void ConsumablesByQuality_ContainsMultipleQualityTiers()
    {
        LootTables.ConsumablesByQuality.Should().ContainKey(QualityTier.JuryRigged);
        LootTables.ConsumablesByQuality.Should().ContainKey(QualityTier.Scavenged);
        LootTables.ConsumablesByQuality.Should().ContainKey(QualityTier.ClanForged);
    }

    [Fact]
    public void ConsumablesByQuality_AllTemplatesHaveValidData()
    {
        foreach (var (tier, templates) in LootTables.ConsumablesByQuality)
        {
            templates.Should().NotBeEmpty($"{tier} should have consumable templates");

            foreach (var template in templates)
            {
                template.Name.Should().NotBeNullOrEmpty();
                template.Description.Should().NotBeNullOrEmpty();
                template.Weight.Should().BePositive();
                template.Value.Should().BePositive();
            }
        }
    }

    #endregion

    #region Material Template Tests

    [Fact]
    public void MaterialsByQuality_ContainsMultipleQualityTiers()
    {
        LootTables.MaterialsByQuality.Should().NotBeEmpty();
        LootTables.MaterialsByQuality.Should().ContainKey(QualityTier.Scavenged);
    }

    [Fact]
    public void MaterialsByQuality_AllTemplatesHaveValidData()
    {
        foreach (var (tier, templates) in LootTables.MaterialsByQuality)
        {
            templates.Should().NotBeEmpty($"{tier} should have material templates");

            foreach (var template in templates)
            {
                template.Name.Should().NotBeNullOrEmpty();
                template.Description.Should().NotBeNullOrEmpty();
                template.Weight.Should().BePositive();
                template.Value.Should().BePositive();
            }
        }
    }

    #endregion

    #region Junk Template Tests

    [Fact]
    public void JunkItems_IsNotEmpty()
    {
        LootTables.JunkItems.Should().NotBeEmpty();
    }

    [Fact]
    public void JunkItems_AllTemplatesHaveValidData()
    {
        foreach (var template in LootTables.JunkItems)
        {
            template.Name.Should().NotBeNullOrEmpty();
            template.Description.Should().NotBeNullOrEmpty();
            template.Weight.Should().BePositive();
            template.Value.Should().BePositive();
        }
    }

    #endregion

    #region Biome Weighting Tests

    [Fact]
    public void ItemTypeByBiome_ContainsAllBiomes()
    {
        var allBiomes = Enum.GetValues<BiomeType>();
        foreach (var biome in allBiomes)
        {
            LootTables.ItemTypeByBiome.Should().ContainKey(biome);
        }
    }

    [Theory]
    [InlineData(BiomeType.Ruin)]
    [InlineData(BiomeType.Industrial)]
    [InlineData(BiomeType.Organic)]
    [InlineData(BiomeType.Void)]
    public void ItemTypeByBiome_EachBiome_SumsToOneHundred(BiomeType biome)
    {
        // Act
        var weights = LootTables.ItemTypeByBiome[biome];
        var sum = weights.Values.Sum();

        // Assert
        sum.Should().Be(100, $"weights for {biome} should sum to 100");
    }

    [Fact]
    public void ItemTypeByBiome_OrganicBiome_FavorsConsumables()
    {
        var organicWeights = LootTables.ItemTypeByBiome[BiomeType.Organic];
        var ruinWeights = LootTables.ItemTypeByBiome[BiomeType.Ruin];

        organicWeights[ItemType.Consumable].Should().BeGreaterThan(ruinWeights[ItemType.Consumable]);
    }

    [Fact]
    public void ItemTypeByBiome_IndustrialBiome_FavorsMaterials()
    {
        var industrialWeights = LootTables.ItemTypeByBiome[BiomeType.Industrial];
        var organicWeights = LootTables.ItemTypeByBiome[BiomeType.Organic];

        industrialWeights[ItemType.Material].Should().BeGreaterThan(organicWeights[ItemType.Material]);
    }

    [Fact]
    public void ItemTypeByBiome_VoidBiome_FavorsCombatItems()
    {
        var voidWeights = LootTables.ItemTypeByBiome[BiomeType.Void];
        var combatWeight = voidWeights[ItemType.Weapon] + voidWeights[ItemType.Armor];

        combatWeight.Should().BeGreaterThanOrEqualTo(50);
    }

    #endregion

    #region AAM-VOICE Compliance Tests

    [Fact]
    public void AllDescriptions_DoNotContainModernTerminology()
    {
        // Note: "Glitch" as a proper noun (e.g., "pre-Glitch", "the Glitch") is allowed
        // per the lore - the Glitch is the in-universe cataclysm event
        // We only forbid the lowercase "glitch" as a technical debugging term
        var forbiddenTerms = new[] { "API", "debug", "meter", "kilogram", "celsius", "percent" };

        // Check weapon descriptions
        foreach (var (_, templates) in LootTables.WeaponsByQuality)
        {
            foreach (var template in templates)
            {
                foreach (var term in forbiddenTerms)
                {
                    template.Description.Should().NotContainEquivalentOf(term,
                        $"Weapon '{template.Name}' description should not contain '{term}'");
                    template.DetailedDescription.Should().NotContainEquivalentOf(term,
                        $"Weapon '{template.Name}' detailed description should not contain '{term}'");
                }

                // Check for lowercase "bug" or "glitch" used as tech terms, not as proper nouns
                // Allow: "the Glitch", "pre-Glitch", "POST-Glitch"
                // Forbid: "a bug in the system", "glitchy behavior"
                ContainsModernBugOrGlitch(template.Description)
                    .Should().BeFalse($"Weapon '{template.Name}' description uses 'bug' or 'glitch' inappropriately");
                ContainsModernBugOrGlitch(template.DetailedDescription)
                    .Should().BeFalse($"Weapon '{template.Name}' detailed description uses 'bug' or 'glitch' inappropriately");
            }
        }
    }

    /// <summary>
    /// Checks if text contains "bug" or "glitch" used as modern tech terms.
    /// Allows "Glitch" as proper noun (capitalized, or in compounds like pre-Glitch).
    /// </summary>
    private static bool ContainsModernBugOrGlitch(string text)
    {
        // Allow patterns like "pre-Glitch", "POST-Glitch", "the Glitch", "Glitch-era"
        // Disallow lowercase "glitch" or "bug" used as common nouns
        var lowered = text.ToLowerInvariant();

        // Check for "bug" as a standalone word (not debugging term)
        // Allow: insect bugs
        // Disallow: software bugs
        if (System.Text.RegularExpressions.Regex.IsMatch(lowered, @"\bsoftware\s+bug|\bbug\s+fix|\bbugg(y|ed)\b"))
            return true;

        // Check for "glitch" as a common noun (not the proper noun "Glitch")
        // The original text must have lowercase 'g' for it to be a violation
        if (System.Text.RegularExpressions.Regex.IsMatch(text, @"\bglitch\b"))
            return true;

        return false;
    }

    [Fact]
    public void AllDescriptions_DoNotContainPrecisionMeasurements()
    {
        // Pattern check for numbers followed by units
        var patterns = new[] { "\\d+\\s*(meter|kg|gram|second|degree|%)", "\\d+\\.\\d+" };

        foreach (var (_, templates) in LootTables.WeaponsByQuality)
        {
            foreach (var template in templates)
            {
                foreach (var pattern in patterns)
                {
                    System.Text.RegularExpressions.Regex.IsMatch(template.Description, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                        .Should().BeFalse($"Weapon '{template.Name}' description should not contain precision measurements");
                }
            }
        }
    }

    #endregion
}
