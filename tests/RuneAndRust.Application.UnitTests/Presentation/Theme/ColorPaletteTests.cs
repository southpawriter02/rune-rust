using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Presentation.Theme;

/// <summary>
/// Unit tests for the <see cref="ColorPalette"/> class.
/// </summary>
[TestFixture]
public class ColorPaletteTests
{
    private ColorPalette _palette = null!;

    [SetUp]
    public void SetUp()
    {
        _palette = ColorPalette.CreateDefault();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetColor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetColor_HealthFull_ReturnsForestGreen()
    {
        // Act
        var color = _palette.GetColor(ColorKey.HealthFull);

        // Assert
        color.Hex.Should().Be("#228B22");
        color.Name.Should().Be("Forest Green");
    }

    [Test]
    public void GetColor_HealthCritical_ReturnsCrimson()
    {
        // Act
        var color = _palette.GetColor(ColorKey.HealthCritical);

        // Assert
        color.Hex.Should().Be("#DC143C");
        color.Name.Should().Be("Crimson");
    }

    [Test]
    public void GetColor_Mana_ReturnsRoyalBlue()
    {
        // Act
        var color = _palette.GetColor(ColorKey.Mana);

        // Assert
        color.Hex.Should().Be("#4169E1");
        color.Name.Should().Be("Royal Blue");
    }

    [Test]
    public void GetColor_Player_ReturnsLime()
    {
        // Act
        var color = _palette.GetColor(ColorKey.Player);

        // Assert
        color.Hex.Should().Be("#00FF00");
        color.Name.Should().Be("Lime");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CreateDefault Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateDefault_ContainsAllColorKeys()
    {
        // Arrange
        var allColorKeys = Enum.GetValues<ColorKey>();

        // Act & Assert
        foreach (var key in allColorKeys)
        {
            _palette.ContainsColor(key).Should().BeTrue(
                $"ColorKey.{key} should be defined in the default palette");
        }
    }

    [Test]
    public void CreateDefault_HasExpectedColorCount()
    {
        // Act
        var count = _palette.Count;

        // Assert
        var expectedCount = Enum.GetValues<ColorKey>().Length;
        count.Should().Be(expectedCount);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Specific Color Category Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(ColorKey.HealthFull, "#228B22")]
    [TestCase(ColorKey.HealthGood, "#32CD32")]
    [TestCase(ColorKey.HealthLow, "#FFD700")]
    [TestCase(ColorKey.HealthCritical, "#DC143C")]
    public void GetColor_HealthColors_HaveCorrectValues(ColorKey key, string expectedHex)
    {
        // Act
        var color = _palette.GetColor(key);

        // Assert
        color.Hex.Should().Be(expectedHex);
    }

    [Test]
    [TestCase(ColorKey.Floor, "#696969")]
    [TestCase(ColorKey.Wall, "#2F4F4F")]
    [TestCase(ColorKey.Water, "#1E90FF")]
    [TestCase(ColorKey.Lava, "#FF4500")]
    public void GetColor_TerrainColors_HaveCorrectValues(ColorKey key, string expectedHex)
    {
        // Act
        var color = _palette.GetColor(key);

        // Assert
        color.Hex.Should().Be(expectedHex);
    }

    [Test]
    [TestCase(ColorKey.Player, "#00FF00")]
    [TestCase(ColorKey.Enemy, "#FF0000")]
    [TestCase(ColorKey.Npc, "#FFFF00")]
    [TestCase(ColorKey.Boss, "#FF00FF")]
    public void GetColor_EntityColors_HaveCorrectValues(ColorKey key, string expectedHex)
    {
        // Act
        var color = _palette.GetColor(key);

        // Assert
        color.Hex.Should().Be(expectedHex);
    }
}
