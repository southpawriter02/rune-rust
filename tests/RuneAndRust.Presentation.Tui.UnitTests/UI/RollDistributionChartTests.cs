// ═══════════════════════════════════════════════════════════════════════════════
// RollDistributionChartTests.cs
// Unit tests for RollDistributionChart UI component.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for the RollDistributionChart class.
/// </summary>
[TestFixture]
public class RollDistributionChartTests
{
    private Mock<ITerminalService> _terminalServiceMock = null!;
    private DiceHistoryPanelConfig _config = null!;
    private RollDistributionChart _chart = null!;

    [SetUp]
    public void SetUp()
    {
        _terminalServiceMock = new Mock<ITerminalService>();
        _config = new DiceHistoryPanelConfig();
        _chart = new RollDistributionChart(_terminalServiceMock.Object, _config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullTerminal_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new RollDistributionChart(null!, _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminal");
    }

    [Test]
    public void Constructor_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new RollDistributionChart(_terminalServiceMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("config");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FORMATBAR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatBar_FullBar_DisplaysAllFilled()
    {
        // Act
        var result = _chart.FormatBar(count: 100, maxCount: 100);

        // Assert
        result.Should().StartWith("[");
        result.Should().EndWith("]");
        result.Should().Contain(new string('#', 20));
        result.Should().NotContain(".");
    }

    [Test]
    public void FormatBar_HalfBar_DisplaysHalfFilled()
    {
        // Act
        var result = _chart.FormatBar(count: 50, maxCount: 100);

        // Assert
        result.Should().Contain("#");
        result.Should().Contain(".");
    }

    [Test]
    public void FormatBar_EmptyBar_DisplaysAllEmpty()
    {
        // Act
        var result = _chart.FormatBar(count: 0, maxCount: 100);

        // Assert
        result.Should().Be($"[{new string('.', 20)}]");
    }

    [Test]
    public void FormatBar_NonZeroCount_DisplaysAtLeastOneFilled()
    {
        // Act
        var result = _chart.FormatBar(count: 1, maxCount: 1000);

        // Assert
        result.Should().Contain("#");
    }

    [Test]
    public void FormatBar_ZeroMaxCount_ReturnsEmptyBar()
    {
        // Act
        var result = _chart.FormatBar(count: 0, maxCount: 0);

        // Assert
        result.Should().Be($"[{new string('.', 20)}]");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OUTLIER DETECTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetOutlierIndicator_AboveExpected_ShowsIndicator()
    {
        // Arrange - Create distribution where value 20 is well above expected
        var counts = new int[20];
        for (int i = 0; i < 20; i++) counts[i] = 10;
        counts[19] = 50; // Roll 20 is way above expected (should be ~10)
        var dto = new RollDistributionDto(counts, 200);

        // Act
        var result = RollDistributionChart.GetOutlierIndicator(dto, 20);

        // Assert
        result.Should().Contain("Above expected");
    }

    [Test]
    public void GetOutlierIndicator_BelowExpected_ShowsIndicator()
    {
        // Arrange - Create distribution where value 1 is well below expected
        var counts = new int[20];
        for (int i = 0; i < 20; i++) counts[i] = 100;
        counts[0] = 2; // Roll 1 is way below expected (should be ~100)
        var dto = new RollDistributionDto(counts, 2000);

        // Act
        var result = RollDistributionChart.GetOutlierIndicator(dto, 1);

        // Assert
        result.Should().Contain("Below expected");
    }

    [Test]
    public void GetOutlierIndicator_NormalValue_ReturnsEmpty()
    {
        // Arrange - Create uniform distribution
        var counts = new int[20];
        for (int i = 0; i < 20; i++) counts[i] = 25;
        var dto = new RollDistributionDto(counts, 500);

        // Act
        var result = RollDistributionChart.GetOutlierIndicator(dto, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void IsOutlier_HighValue_ReturnsTrue()
    {
        // Act - 80 is > 50 * 1.5 = 75
        var result = RollDistributionChart.IsOutlier(expectedCount: 50f, actualCount: 80);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsOutlier_LowValue_ReturnsTrue()
    {
        // Act - 20 is < 50 * 0.5 = 25
        var result = RollDistributionChart.IsOutlier(expectedCount: 50f, actualCount: 20);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsOutlier_NormalValue_ReturnsFalse()
    {
        // Act - 50 is within range [25, 75]
        var result = RollDistributionChart.IsOutlier(expectedCount: 50f, actualCount: 50);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void RenderDistribution_ValidData_DisplaysAllBars()
    {
        // Arrange
        var counts = new int[20];
        for (int i = 0; i < 20; i++) counts[i] = i + 1;
        var dto = new RollDistributionDto(counts, 210);

        // Act
        _chart.RenderDistribution(dto, x: 0, y: 0);

        // Assert - Should write 20 lines (one for each roll value)
        _terminalServiceMock.Verify(
            t => t.WriteColoredAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ConsoleColor>()),
            Times.Exactly(20));
    }

    [Test]
    public void RenderDistribution_NullDto_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => _chart.RenderDistribution((RollDistributionDto)null!, x: 0, y: 0);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ShowExpectedLine_WritesToTerminal()
    {
        // Act
        _chart.ShowExpectedLine(x: 10, y: 5);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteAt(10, 5, It.Is<string>(s => s.Contains("5.0%"))),
            Times.Once);
    }
}
