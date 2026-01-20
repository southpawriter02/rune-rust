using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for ResourceQuality enum and ResourceQualityExtensions (v0.11.0a).
/// </summary>
[TestFixture]
public class ResourceQualityExtensionsTests
{
    // ═══════════════════════════════════════════════════════════════
    // VALUE MULTIPLIER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ResourceQuality.Common, 1.0f)]
    [TestCase(ResourceQuality.Fine, 1.5f)]
    [TestCase(ResourceQuality.Rare, 3.0f)]
    [TestCase(ResourceQuality.Legendary, 10.0f)]
    public void GetValueMultiplier_ReturnsCorrectMultiplier(ResourceQuality quality, float expected)
    {
        // Act
        var result = quality.GetValueMultiplier();

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // CRAFTING BONUS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ResourceQuality.Common, 0)]
    [TestCase(ResourceQuality.Fine, 1)]
    [TestCase(ResourceQuality.Rare, 2)]
    [TestCase(ResourceQuality.Legendary, 3)]
    public void GetCraftingBonus_ReturnsCorrectBonus(ResourceQuality quality, int expected)
    {
        // Act
        var result = quality.GetCraftingBonus();

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ResourceQuality.Common, "#FFFFFF")]
    [TestCase(ResourceQuality.Fine, "#1EFF00")]
    [TestCase(ResourceQuality.Rare, "#0070DD")]
    [TestCase(ResourceQuality.Legendary, "#FF8000")]
    public void GetDisplayColor_ReturnsCorrectHexColor(ResourceQuality quality, string expected)
    {
        // Act
        var result = quality.GetDisplayColor();

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY NAME TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ResourceQuality.Common, "Common")]
    [TestCase(ResourceQuality.Fine, "Fine")]
    [TestCase(ResourceQuality.Rare, "Rare")]
    [TestCase(ResourceQuality.Legendary, "Legendary")]
    public void GetDisplayName_ReturnsCorrectName(ResourceQuality quality, string expected)
    {
        // Act
        var result = quality.GetDisplayName();

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // EDGE CASE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void AllQualityValues_HaveDefinedMultipliers()
    {
        // Arrange
        var qualities = Enum.GetValues<ResourceQuality>();

        // Act & Assert - Ensure no exceptions are thrown
        foreach (var quality in qualities)
        {
            var multiplier = quality.GetValueMultiplier();
            multiplier.Should().BeGreaterThan(0, $"Quality {quality} should have a positive multiplier");
        }
    }

    [Test]
    public void AllQualityValues_HaveDefinedCraftingBonuses()
    {
        // Arrange
        var qualities = Enum.GetValues<ResourceQuality>();

        // Act & Assert - Ensure no exceptions are thrown
        foreach (var quality in qualities)
        {
            var bonus = quality.GetCraftingBonus();
            bonus.Should().BeGreaterThanOrEqualTo(0, $"Quality {quality} should have a non-negative crafting bonus");
        }
    }

    [Test]
    public void AllQualityValues_HaveNonEmptyDisplayColors()
    {
        // Arrange
        var qualities = Enum.GetValues<ResourceQuality>();

        // Act & Assert
        foreach (var quality in qualities)
        {
            var color = quality.GetDisplayColor();
            color.Should().NotBeNullOrEmpty($"Quality {quality} should have a display color");
            color.Should().StartWith("#", $"Quality {quality} color should be a hex string");
        }
    }

    [Test]
    public void AllQualityValues_HaveNonEmptyDisplayNames()
    {
        // Arrange
        var qualities = Enum.GetValues<ResourceQuality>();

        // Act & Assert
        foreach (var quality in qualities)
        {
            var name = quality.GetDisplayName();
            name.Should().NotBeNullOrEmpty($"Quality {quality} should have a display name");
            name.Should().NotBe("Unknown", $"Quality {quality} should have a defined display name");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // VALUE PROGRESSION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ValueMultipliers_IncreaseWithQuality()
    {
        // Arrange
        var common = ResourceQuality.Common.GetValueMultiplier();
        var fine = ResourceQuality.Fine.GetValueMultiplier();
        var rare = ResourceQuality.Rare.GetValueMultiplier();
        var legendary = ResourceQuality.Legendary.GetValueMultiplier();

        // Assert - Each tier should be more valuable than the previous
        fine.Should().BeGreaterThan(common);
        rare.Should().BeGreaterThan(fine);
        legendary.Should().BeGreaterThan(rare);
    }

    [Test]
    public void CraftingBonuses_IncreaseWithQuality()
    {
        // Arrange
        var common = ResourceQuality.Common.GetCraftingBonus();
        var fine = ResourceQuality.Fine.GetCraftingBonus();
        var rare = ResourceQuality.Rare.GetCraftingBonus();
        var legendary = ResourceQuality.Legendary.GetCraftingBonus();

        // Assert - Each tier should provide a greater bonus than the previous
        fine.Should().BeGreaterThan(common);
        rare.Should().BeGreaterThan(fine);
        legendary.Should().BeGreaterThan(rare);
    }
}
