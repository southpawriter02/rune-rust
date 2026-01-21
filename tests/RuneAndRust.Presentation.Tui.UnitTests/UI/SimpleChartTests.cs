// ═══════════════════════════════════════════════════════════════════════════════
// SimpleChartTests.cs
// Unit tests for the SimpleChart component.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for the <see cref="SimpleChart"/> component.
/// </summary>
/// <remarks>
/// Tests cover bar formatting and chart rendering.
/// </remarks>
[TestFixture]
public class SimpleChartTests
{
    private Mock<ITerminalService> _mockTerminalService = null!;
    private StatisticsDashboardConfig _config = null!;
    private SimpleChart _chart = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminalService = new Mock<ITerminalService>();
        _config = new StatisticsDashboardConfig();
        _chart = new SimpleChart(_mockTerminalService.Object, _config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_NullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new SimpleChart(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    [Test]
    public void Constructor_NullConfig_UsesDefaults()
    {
        // Arrange & Act
        var chart = new SimpleChart(_mockTerminalService.Object, null);

        // Assert
        chart.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FORMAT BAR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatBar_HalfValue_ReturnsHalfFilledBar()
    {
        // Arrange
        var value = 50;
        var maxValue = 100;
        var width = 20;

        // Act
        var result = _chart.FormatBar(value, maxValue, width);

        // Assert
        result.Should().HaveLength(width);
        result.Count(c => c == '#').Should().Be(10);
        result.Count(c => c == '.').Should().Be(10);
    }

    [Test]
    public void FormatBar_FullValue_ReturnsAllFilled()
    {
        // Arrange
        var value = 100;
        var maxValue = 100;
        var width = 20;

        // Act
        var result = _chart.FormatBar(value, maxValue, width);

        // Assert
        result.Should().Be(new string('#', 20));
    }

    [Test]
    public void FormatBar_ZeroValue_ReturnsAllEmpty()
    {
        // Arrange
        var value = 0;
        var maxValue = 100;
        var width = 20;

        // Act
        var result = _chart.FormatBar(value, maxValue, width);

        // Assert
        result.Should().Be(new string('.', 20));
    }

    [Test]
    public void FormatBar_ZeroMaxValue_ReturnsAllEmpty()
    {
        // Arrange
        var value = 50;
        var maxValue = 0;
        var width = 20;

        // Act
        var result = _chart.FormatBar(value, maxValue, width);

        // Assert
        result.Should().Be(new string('.', 20));
    }

    [Test]
    public void FormatBar_ThirtyPercent_ReturnsCorrectProportion()
    {
        // Arrange
        var value = 30;
        var maxValue = 100;
        var width = 10;

        // Act
        var result = _chart.FormatBar(value, maxValue, width);

        // Assert
        result.Should().HaveLength(10);
        result.Count(c => c == '#').Should().Be(3);
        result.Count(c => c == '.').Should().Be(7);
    }
}
