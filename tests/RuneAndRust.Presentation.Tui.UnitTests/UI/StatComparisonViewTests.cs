// ═══════════════════════════════════════════════════════════════════════════════
// StatComparisonViewTests.cs
// Unit tests for the StatComparisonView component.
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
/// Unit tests for the <see cref="StatComparisonView"/> component.
/// </summary>
/// <remarks>
/// Tests cover delta formatting, color coding, and comparison rendering.
/// </remarks>
[TestFixture]
public class StatComparisonViewTests
{
    private Mock<ITerminalService> _mockTerminalService = null!;
    private StatisticsDashboardConfig _config = null!;
    private StatComparisonView _view = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminalService = new Mock<ITerminalService>();
        _config = new StatisticsDashboardConfig();
        _view = new StatComparisonView(_mockTerminalService.Object, _config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_NullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new StatComparisonView(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    [Test]
    public void Constructor_NullConfig_UsesDefaults()
    {
        // Arrange & Act
        var view = new StatComparisonView(_mockTerminalService.Object, null);

        // Assert
        view.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FORMAT DELTA TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatDelta_PositiveValue_ReturnsPlusPrefix()
    {
        // Arrange & Act
        var result = _view.FormatDelta(47, 1284);

        // Assert
        result.Should().Be("+47");
    }

    [Test]
    public void FormatDelta_NegativeValue_ReturnsMinusPrefix()
    {
        // Arrange & Act
        var result = _view.FormatDelta(-3, 100);

        // Assert
        result.Should().Be("-3");
    }

    [Test]
    public void FormatDelta_ZeroValue_ReturnsDash()
    {
        // Arrange & Act
        var result = _view.FormatDelta(0, 100);

        // Assert
        result.Should().Be("-");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FORMAT PERCENTAGE DELTA TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatPercentageDelta_PositiveChange_ReturnsPlusPrefix()
    {
        // Arrange
        var sessionValue = 942;  // 94.2%
        var allTimeValue = 917;  // 91.7%

        // Act
        var result = _view.FormatPercentageDelta(sessionValue, allTimeValue);

        // Assert
        result.Should().Be("+2.5%");
    }

    [Test]
    public void FormatPercentageDelta_NegativeChange_ReturnsMinusPrefix()
    {
        // Arrange
        var sessionValue = 850;  // 85.0%
        var allTimeValue = 900;  // 90.0%

        // Act
        var result = _view.FormatPercentageDelta(sessionValue, allTimeValue);

        // Assert
        result.Should().Be("-5.0%");
    }

    [Test]
    public void FormatPercentageDelta_VerySmallChange_ReturnsDash()
    {
        // Arrange
        var sessionValue = 900;  // 90.0%
        var allTimeValue = 900;  // 90.0%

        // Act
        var result = _view.FormatPercentageDelta(sessionValue, allTimeValue);

        // Assert
        result.Should().Be("-");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GET DELTA COLOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetDeltaColor_PositiveDelta_ReturnsGreen()
    {
        // Arrange & Act
        var color = _view.GetDeltaColor(47);

        // Assert
        color.Should().Be(ConsoleColor.Green);
    }

    [Test]
    public void GetDeltaColor_NegativeDelta_ReturnsRed()
    {
        // Arrange & Act
        var color = _view.GetDeltaColor(-3);

        // Assert
        color.Should().Be(ConsoleColor.Red);
    }

    [Test]
    public void GetDeltaColor_ZeroDelta_ReturnsGray()
    {
        // Arrange & Act
        var color = _view.GetDeltaColor(0);

        // Assert
        color.Should().Be(ConsoleColor.Gray);
    }
}
