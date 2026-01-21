// ═══════════════════════════════════════════════════════════════════════════════
// ProgressBarRendererTests.cs
// Unit tests for ProgressBarRenderer.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for <see cref="ProgressBarRenderer"/>.
/// </summary>
[TestFixture]
public class ProgressBarRendererTests
{
    private ProgressBarRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _renderer = new ProgressBarRenderer();
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDER PROGRESS BAR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void RenderProgressBar_At50Percent_ShowsHalfFilled()
    {
        // Arrange
        var current = 50;
        var target = 100;
        var width = 20;

        // Act
        var result = _renderer.RenderProgressBar(current, target, width);

        // Assert
        result.Should().Be("[##########..........]");
        result.Should().HaveLength(width + 2); // brackets add 2
    }

    [Test]
    public void RenderProgressBar_At100Percent_ShowsFullyFilled()
    {
        // Arrange
        var current = 100;
        var target = 100;
        var width = 20;

        // Act
        var result = _renderer.RenderProgressBar(current, target, width);

        // Assert
        result.Should().Be("[####################]");
    }

    [Test]
    public void RenderProgressBar_At0Percent_ShowsEmpty()
    {
        // Arrange
        var current = 0;
        var target = 100;
        var width = 20;

        // Act
        var result = _renderer.RenderProgressBar(current, target, width);

        // Assert
        result.Should().Be("[....................]");
    }

    [Test]
    public void RenderProgressBar_WithZeroTarget_ShowsEmptyBar()
    {
        // Arrange
        var current = 50;
        var target = 0;
        var width = 10;

        // Act
        var result = _renderer.RenderProgressBar(current, target, width);

        // Assert
        result.Should().Be("[..........]");
    }

    // ═══════════════════════════════════════════════════════════════
    // FORMAT PERCENTAGE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FormatPercentage_At47Of100_Returns47Percent()
    {
        // Arrange & Act
        var result = _renderer.FormatPercentage(47, 100);

        // Assert
        result.Should().Be("47%");
    }

    [Test]
    public void FormatPercentage_At100Of100_Returns100Percent()
    {
        // Arrange & Act
        var result = _renderer.FormatPercentage(100, 100);

        // Assert
        result.Should().Be("100%");
    }

    [Test]
    public void FormatPercentage_ExceedsTarget_CapsAt100Percent()
    {
        // Arrange & Act
        var result = _renderer.FormatPercentage(150, 100);

        // Assert
        result.Should().Be("100%");
    }

    [Test]
    public void FormatPercentage_WithZeroTarget_Returns0Percent()
    {
        // Arrange & Act
        var result = _renderer.FormatPercentage(50, 0);

        // Assert
        result.Should().Be("0%");
    }

    // ═══════════════════════════════════════════════════════════════
    // GET PROGRESS COLOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [TestCase(1.0f, ConsoleColor.Green)]
    [TestCase(0.8f, ConsoleColor.Cyan)]
    [TestCase(0.6f, ConsoleColor.Yellow)]
    [TestCase(0.3f, ConsoleColor.DarkYellow)]
    [TestCase(0.1f, ConsoleColor.Red)]
    [TestCase(0.0f, ConsoleColor.Red)]
    public void GetProgressColor_ReturnsCorrectColor(float percentage, ConsoleColor expected)
    {
        // Arrange & Act
        var result = _renderer.GetProgressColor(percentage);

        // Assert
        result.Should().Be(expected);
    }
}
