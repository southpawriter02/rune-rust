// ═══════════════════════════════════════════════════════════════════════════════
// StreakIndicatorTests.cs
// Unit tests for StreakIndicator UI component.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for the StreakIndicator class.
/// </summary>
[TestFixture]
public class StreakIndicatorTests
{
    private Mock<ITerminalService> _terminalServiceMock = null!;
    private DiceHistoryPanelConfig _config = null!;
    private StreakIndicator _indicator = null!;

    [SetUp]
    public void SetUp()
    {
        _terminalServiceMock = new Mock<ITerminalService>();
        _config = new DiceHistoryPanelConfig();
        _indicator = new StreakIndicator(_terminalServiceMock.Object, _config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullTerminal_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new StreakIndicator(null!, _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminal");
    }

    [Test]
    public void Constructor_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new StreakIndicator(_terminalServiceMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("config");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FORMATSTREAK TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatStreak_PositiveStreak_DisplaysLucky()
    {
        // Act
        var result = StreakIndicator.FormatStreak(3, isLucky: true);

        // Assert
        result.Should().Be("+3");
    }

    [Test]
    public void FormatStreak_NegativeStreak_DisplaysUnlucky()
    {
        // Act
        var result = StreakIndicator.FormatStreak(5, isLucky: false);

        // Assert
        result.Should().Be("-5");
    }

    [Test]
    public void FormatStreak_ZeroStreak_DisplaysZero()
    {
        // Act
        var result = StreakIndicator.FormatStreak(0, isLucky: true);

        // Assert
        result.Should().Be("0");
    }

    [Test]
    public void FormatStreak_SignedPositive_FormatsCorrectly()
    {
        // Act
        var result = StreakIndicator.FormatStreak(4);

        // Assert
        result.Should().Be("+4");
    }

    [Test]
    public void FormatStreak_SignedNegative_FormatsCorrectly()
    {
        // Act
        var result = StreakIndicator.FormatStreak(-3);

        // Assert
        result.Should().Be("-3");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GETSTREAKDESCRIPTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetStreakDescription_PositiveStreak_ReturnsAboveAverage()
    {
        // Act
        var result = _indicator.GetStreakDescription(3, isLucky: true);

        // Assert
        result.Should().Contain("above");
        result.Should().Contain("3");
        result.Should().Contain("in a row");
    }

    [Test]
    public void GetStreakDescription_NegativeStreak_ReturnsBelowAverage()
    {
        // Act
        var result = _indicator.GetStreakDescription(5, isLucky: false);

        // Assert
        result.Should().Contain("below");
        result.Should().Contain("5");
        result.Should().Contain("in a row");
    }

    [Test]
    public void GetStreakDescription_ZeroStreak_ReturnsNoStreak()
    {
        // Act
        var result = _indicator.GetStreakDescription(0, isLucky: true);

        // Assert
        result.Should().Be("(no streak)");
    }

    [Test]
    public void GetStreakDescription_SingleRoll_ReturnsSingularForm()
    {
        // Act
        var result = _indicator.GetStreakDescription(1, isLucky: true);

        // Assert
        result.Should().Contain("1 roll");
        result.Should().NotContain("in a row");
    }

    [Test]
    public void GetStreakDescription_SignedValue_ExtractsDirectionCorrectly()
    {
        // Act
        var result = _indicator.GetStreakDescription(-4);

        // Assert
        result.Should().Contain("below");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GETSTREAKCOLOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetStreakColor_PositiveStreak_ReturnsGreen()
    {
        // Act
        var result = _indicator.GetStreakColor(3);

        // Assert
        result.Should().Be(ConsoleColor.Green);
    }

    [Test]
    public void GetStreakColor_NegativeStreak_ReturnsRed()
    {
        // Act
        var result = _indicator.GetStreakColor(-3);

        // Assert
        result.Should().Be(ConsoleColor.Red);
    }

    [Test]
    public void GetStreakColor_ZeroStreak_ReturnsWhite()
    {
        // Act
        var result = _indicator.GetStreakColor(0);

        // Assert
        result.Should().Be(ConsoleColor.White);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void RenderStreak_ValidStreak_WritesToTerminal()
    {
        // Act
        _indicator.RenderStreak(3, isLucky: true, x: 10, y: 5);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Test]
    public void ShowLongestStreaks_ValidData_WritesToTerminal()
    {
        // Act
        _indicator.ShowLongestStreaks(longestLucky: 5, longestUnlucky: 3, x: 10, y: 5);

        // Assert
        _terminalServiceMock.Verify(
            t => t.WriteAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }
}
