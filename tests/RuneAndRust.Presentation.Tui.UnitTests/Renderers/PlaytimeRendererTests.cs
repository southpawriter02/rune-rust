// ═══════════════════════════════════════════════════════════════════════════════
// PlaytimeRendererTests.cs
// Unit tests for the PlaytimeRenderer component.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for the <see cref="PlaytimeRenderer"/> component.
/// </summary>
/// <remarks>
/// Tests cover playtime formatting rules and color milestone mappings.
/// </remarks>
[TestFixture]
public class PlaytimeRendererTests
{
    private PlaytimeRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _renderer = new PlaytimeRenderer();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FORMAT SESSION TIME TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatSessionTime_LessThanOneMinute_ReturnsLessThanOneMinute()
    {
        // Arrange
        var sessionTime = TimeSpan.FromSeconds(30);

        // Act
        var result = _renderer.FormatSessionTime(sessionTime);

        // Assert
        result.Should().Be("<1m");
    }

    [Test]
    public void FormatSessionTime_LessThanOneHour_ReturnsMinutesOnly()
    {
        // Arrange
        var sessionTime = TimeSpan.FromMinutes(45);

        // Act
        var result = _renderer.FormatSessionTime(sessionTime);

        // Assert
        result.Should().Be("45m");
    }

    [Test]
    public void FormatSessionTime_OneOrMoreHours_ReturnsHoursAndMinutes()
    {
        // Arrange
        var sessionTime = TimeSpan.FromHours(2) + TimeSpan.FromMinutes(15);

        // Act
        var result = _renderer.FormatSessionTime(sessionTime);

        // Assert
        result.Should().Be("2h 15m");
    }

    [Test]
    public void FormatTotalPlaytime_HundredPlusHours_ReturnsLongFormat()
    {
        // Arrange
        var totalPlaytime = TimeSpan.FromHours(156) + TimeSpan.FromMinutes(20);

        // Act
        var result = _renderer.FormatTotalPlaytime(totalPlaytime);

        // Assert
        result.Should().Be("156h 20m");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PLAYTIME COLOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetPlaytimeColor_LessThanTenHours_ReturnsWhite()
    {
        // Arrange
        var playtime = TimeSpan.FromHours(5);

        // Act
        var color = _renderer.GetPlaytimeColor(playtime);

        // Assert
        color.Should().Be(ConsoleColor.White);
    }

    [Test]
    public void GetPlaytimeColor_TenToFortyNineHours_ReturnsGreen()
    {
        // Arrange
        var playtime = TimeSpan.FromHours(25);

        // Act
        var color = _renderer.GetPlaytimeColor(playtime);

        // Assert
        color.Should().Be(ConsoleColor.Green);
    }

    [Test]
    public void GetPlaytimeColor_FiftyToNinetyNineHours_ReturnsYellow()
    {
        // Arrange
        var playtime = TimeSpan.FromHours(75);

        // Act
        var color = _renderer.GetPlaytimeColor(playtime);

        // Assert
        color.Should().Be(ConsoleColor.Yellow);
    }

    [Test]
    public void GetPlaytimeColor_HundredPlusHours_ReturnsCyan()
    {
        // Arrange
        var playtime = TimeSpan.FromHours(150);

        // Act
        var color = _renderer.GetPlaytimeColor(playtime);

        // Assert
        color.Should().Be(ConsoleColor.Cyan);
    }
}
