// ═══════════════════════════════════════════════════════════════════════════════
// CraftingProgressRendererTests.cs
// Unit tests for the CraftingProgressRenderer class.
// Version: 0.13.3b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for CraftingProgressRenderer.
/// </summary>
[TestFixture]
public class CraftingProgressRendererTests
{
    private CraftingStationConfig _config = null!;
    private CraftingProgressRenderer _renderer = null!;

    [SetUp]
    public void Setup()
    {
        // Arrange: Use a consistent bar width for testable assertions
        _config = new CraftingStationConfig { ProgressBarWidth = 20 };
        _renderer = new CraftingProgressRenderer(_config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FormatProgressBar Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(0f, "[....................]")]
    [TestCase(0.5f, "[##########..........]")]
    [TestCase(1f, "[####################]")]
    public void FormatProgressBar_WithProgress_ReturnsCorrectFormat(
        float progress, string expected)
    {
        // Act
        var result = _renderer.FormatProgressBar(progress);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    [TestCase(-0.5f, "[....................]", Description = "Progress below 0 should clamp to 0")]
    [TestCase(1.5f, "[####################]", Description = "Progress above 1 should clamp to 1")]
    public void FormatProgressBar_WithOutOfRangeProgress_ClampsValue(
        float progress, string expected)
    {
        // Act
        var result = _renderer.FormatProgressBar(progress);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RenderProgress Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void RenderProgress_WithRecipeName_IncludesNameAndPercentage()
    {
        // Act
        var result = _renderer.RenderProgress(0.5f, "Steel Blade");

        // Assert
        result.Should().Contain("Crafting: Steel Blade");
        result.Should().Contain("50%");
        result.Should().Contain("[##########..........]");
    }

    [Test]
    [TestCase(0f, "0%")]
    [TestCase(0.25f, "25%")]
    [TestCase(0.75f, "75%")]
    [TestCase(1f, "100%")]
    public void RenderProgress_WithProgress_ShowsCorrectPercentage(
        float progress, string expectedPercent)
    {
        // Act
        var result = _renderer.RenderProgress(progress, "Test Item");

        // Assert
        result.Should().Contain(expectedPercent);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Configuration Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullConfig_UsesDefaultConfig()
    {
        // Arrange & Act
        var renderer = new CraftingProgressRenderer(config: null);

        // Assert
        renderer.GetConfig().Should().NotBeNull();
        renderer.GetConfig().ProgressBarWidth.Should().Be(30); // Default value
    }

    [Test]
    public void FormatProgressBar_WithCustomCharacters_UsesConfiguredChars()
    {
        // Arrange
        var customConfig = new CraftingStationConfig
        {
            ProgressBarWidth = 10,
            ProgressFilledChar = '=',
            ProgressEmptyChar = '-'
        };
        var renderer = new CraftingProgressRenderer(customConfig);

        // Act
        var result = renderer.FormatProgressBar(0.5f);

        // Assert
        result.Should().Be("[=====-----]");
    }
}
