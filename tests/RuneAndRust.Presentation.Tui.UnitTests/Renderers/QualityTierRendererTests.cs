// ═══════════════════════════════════════════════════════════════════════════════
// QualityTierRendererTests.cs
// Unit tests for the QualityTierRenderer class.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="QualityTierRenderer"/>.
/// </summary>
[TestFixture]
public class QualityTierRendererTests
{
    private QualityTierRenderer _renderer = null!;
    private RecipeBrowserConfig _config = null!;

    [SetUp]
    public void SetUp()
    {
        _config = RecipeBrowserConfig.CreateDefault();
        _renderer = new QualityTierRenderer(_config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetQualityStars Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetQualityStars_Common_ReturnsOneStar()
    {
        // Arrange & Act
        var stars = _renderer.GetQualityStars(ItemQuality.Common);

        // Assert
        stars.Should().Be("★");
    }

    [Test]
    public void GetQualityStars_Uncommon_ReturnsTwoStars()
    {
        // Arrange & Act
        var stars = _renderer.GetQualityStars(ItemQuality.Uncommon);

        // Assert
        stars.Should().Be("★★");
    }

    [Test]
    public void GetQualityStars_Rare_ReturnsThreeStars()
    {
        // Arrange & Act
        var stars = _renderer.GetQualityStars(ItemQuality.Rare);

        // Assert
        stars.Should().Be("★★★");
    }

    [Test]
    public void GetQualityStars_Epic_ReturnsFourStars()
    {
        // Arrange & Act
        var stars = _renderer.GetQualityStars(ItemQuality.Epic);

        // Assert
        stars.Should().Be("★★★★");
    }

    [Test]
    public void GetQualityStars_Legendary_ReturnsFiveStars()
    {
        // Arrange & Act
        var stars = _renderer.GetQualityStars(ItemQuality.Legendary);

        // Assert
        stars.Should().Be("★★★★★");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetQualityName Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ItemQuality.Common, "COMMON")]
    [TestCase(ItemQuality.Uncommon, "UNCOMMON")]
    [TestCase(ItemQuality.Rare, "RARE")]
    [TestCase(ItemQuality.Epic, "EPIC")]
    [TestCase(ItemQuality.Legendary, "LEGENDARY")]
    public void GetQualityName_ReturnsCorrectName(ItemQuality quality, string expectedName)
    {
        // Arrange & Act
        var name = _renderer.GetQualityName(quality);

        // Assert
        name.Should().Be(expectedName);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetQualityColor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ItemQuality.Common, ConsoleColor.Gray)]
    [TestCase(ItemQuality.Uncommon, ConsoleColor.Green)]
    [TestCase(ItemQuality.Rare, ConsoleColor.Blue)]
    [TestCase(ItemQuality.Epic, ConsoleColor.Magenta)]
    [TestCase(ItemQuality.Legendary, ConsoleColor.Yellow)]
    public void GetQualityColor_ReturnsCorrectColor(ItemQuality quality, ConsoleColor expectedColor)
    {
        // Arrange & Act
        var color = _renderer.GetQualityColor(quality);

        // Assert
        color.Should().Be(expectedColor);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FormatQuality Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatQuality_ReturnsNameAndStars()
    {
        // Arrange & Act
        var formatted = _renderer.FormatQuality(ItemQuality.Rare);

        // Assert
        formatted.Should().Be("RARE (★★★)");
    }

    [Test]
    public void FormatQuality_Legendary_ReturnsFiveStarsWithName()
    {
        // Arrange & Act
        var formatted = _renderer.FormatQuality(ItemQuality.Legendary);

        // Assert
        formatted.Should().Be("LEGENDARY (★★★★★)");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CreateQualityResult Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateQualityResult_ReturnsCompleteDto()
    {
        // Arrange & Act
        var result = _renderer.CreateQualityResult(ItemQuality.Epic);

        // Assert
        result.Quality.Should().Be(ItemQuality.Epic);
        result.QualityName.Should().Be("EPIC");
        result.Stars.Should().Be("★★★★");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullConfig_UsesDefault()
    {
        // Arrange & Act
        var renderer = new QualityTierRenderer(null);

        // Assert - should not throw and use default colors
        var color = renderer.GetQualityColor(ItemQuality.Rare);
        color.Should().Be(ConsoleColor.Blue);
    }
}
