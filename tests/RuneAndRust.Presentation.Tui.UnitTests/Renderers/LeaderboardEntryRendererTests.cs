// ═══════════════════════════════════════════════════════════════════════════════
// LeaderboardEntryRendererTests.cs
// Unit tests for the LeaderboardEntryRenderer class.
// Version: 0.13.4c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UnitTests.Renderers;

/// <summary>
/// Unit tests for LeaderboardEntryRenderer functionality.
/// </summary>
[TestFixture]
public class LeaderboardEntryRendererTests
{
    private LeaderboardEntryRenderer _renderer = null!;
    private LeaderboardViewConfig _config = null!;

    [SetUp]
    public void SetUp()
    {
        _renderer = new LeaderboardEntryRenderer();
        _config = new LeaderboardViewConfig();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RANK FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatRank_FirstPlace_ReturnsHashOne()
    {
        // Arrange & Act
        var result = _renderer.FormatRank(1, false);

        // Assert
        result.Should().Be("#1");
    }

    [Test]
    public void FormatRank_SecondPlace_ReturnsHashTwo()
    {
        // Arrange & Act
        var result = _renderer.FormatRank(2, false);

        // Assert
        result.Should().Be("#2");
    }

    [Test]
    public void FormatRank_ThirdPlace_ReturnsHashThree()
    {
        // Arrange & Act
        var result = _renderer.FormatRank(3, false);

        // Assert
        result.Should().Be("#3");
    }

    [Test]
    public void FormatRank_FourthPlace_ReturnsPlainNumber()
    {
        // Arrange & Act
        var result = _renderer.FormatRank(4, false);

        // Assert
        result.Should().Be("4");
    }

    [Test]
    public void FormatRank_CurrentPlayer_ReturnsGreaterThanSymbol()
    {
        // Arrange & Act
        var result = _renderer.FormatRank(5, true);

        // Assert
        result.Should().Be(">5");
    }

    [Test]
    public void FormatRank_CurrentPlayerFirstPlace_ReturnsGreaterThanOne()
    {
        // Arrange & Act
        var result = _renderer.FormatRank(1, true);

        // Assert
        result.Should().Be(">1");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RANK COLOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetRankColor_FirstPlace_ReturnsYellow()
    {
        // Arrange & Act
        var result = _renderer.GetRankColor(1, false);

        // Assert
        result.Should().Be(ConsoleColor.Yellow);
    }

    [Test]
    public void GetRankColor_SecondPlace_ReturnsGray()
    {
        // Arrange & Act
        var result = _renderer.GetRankColor(2, false);

        // Assert
        result.Should().Be(ConsoleColor.Gray);
    }

    [Test]
    public void GetRankColor_ThirdPlace_ReturnsDarkYellow()
    {
        // Arrange & Act
        var result = _renderer.GetRankColor(3, false);

        // Assert
        result.Should().Be(ConsoleColor.DarkYellow);
    }

    [Test]
    public void GetRankColor_FourthPlace_ReturnsWhite()
    {
        // Arrange & Act
        var result = _renderer.GetRankColor(4, false);

        // Assert
        result.Should().Be(ConsoleColor.White);
    }

    [Test]
    public void GetRankColor_CurrentPlayer_ReturnsCyan()
    {
        // Arrange & Act
        var result = _renderer.GetRankColor(5, true);

        // Assert
        result.Should().Be(ConsoleColor.Cyan);
    }

    [Test]
    public void GetRankColor_CurrentPlayerFirstPlace_ReturnsCyan()
    {
        // Arrange: Current player highlight takes precedence
        // Act
        var result = _renderer.GetRankColor(1, true);

        // Assert
        result.Should().Be(ConsoleColor.Cyan);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SCORE FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatScore_LargeNumber_ReturnsCommaSeparated()
    {
        // Arrange & Act
        var result = _renderer.FormatScore(125430);

        // Assert
        result.Should().Be("125,430");
    }

    [Test]
    public void FormatScore_SmallNumber_ReturnsPlainNumber()
    {
        // Arrange & Act
        var result = _renderer.FormatScore(42);

        // Assert
        result.Should().Be("42");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TIME FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatTime_OneHourTwentyThreeMinutes_ReturnsCorrectFormat()
    {
        // Arrange
        var time = new TimeSpan(1, 23, 45);

        // Act
        var result = _renderer.FormatTime(time);

        // Assert
        result.Should().Be("1:23:45");
    }

    [Test]
    public void FormatTime_LessThanOneHour_ReturnsZeroHours()
    {
        // Arrange
        var time = new TimeSpan(0, 15, 30);

        // Act
        var result = _renderer.FormatTime(time);

        // Assert
        result.Should().Be("0:15:30");
    }

    [Test]
    public void FormatTime_TwoHours_ReturnsCorrectFormat()
    {
        // Arrange
        var time = new TimeSpan(2, 5, 7);

        // Act
        var result = _renderer.FormatTime(time);

        // Assert
        result.Should().Be("2:05:07");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DATE FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatDate_ReturnsIsoFormat()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15);

        // Act
        var result = _renderer.FormatDate(date);

        // Assert
        result.Should().Be("2024-03-15");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ENTRY FORMATTING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FormatEntry_HighScoreCategory_ContainsFormattedScore()
    {
        // Arrange
        var entry = new LeaderboardDisplayDto
        {
            Rank = 1,
            PlayerName = "TestPlayer",
            CharacterClass = "Warrior",
            Level = 10,
            Score = 52340,
            Date = new DateTime(2024, 3, 15),
            IsCurrentPlayer = false,
            Category = LeaderboardCategory.HighScore
        };

        // Act
        var result = _renderer.FormatEntry(entry, LeaderboardCategory.HighScore, _config);

        // Assert
        result.Should().Contain("52,340");
        result.Should().Contain("#1");
        result.Should().Contain("TestPlayer");
    }

    [Test]
    public void FormatEntry_CurrentPlayer_ContainsYouSuffix()
    {
        // Arrange
        var entry = new LeaderboardDisplayDto
        {
            Rank = 5,
            PlayerName = "Hero",
            CharacterClass = "Mage",
            Level = 7,
            Score = 12500,
            Date = DateTime.Today,
            IsCurrentPlayer = true,
            Category = LeaderboardCategory.HighScore
        };

        // Act
        var result = _renderer.FormatEntry(entry, LeaderboardCategory.HighScore, _config);

        // Assert
        result.Should().Contain("(You)");
        result.Should().Contain(">5");
    }

    [Test]
    public void FormatEntry_SpeedrunCategory_ContainsFormattedTime()
    {
        // Arrange
        var entry = new LeaderboardDisplayDto
        {
            Rank = 1,
            PlayerName = "Speedster",
            CharacterClass = "Rogue",
            Level = 15,
            Score = 0,
            TimeElapsed = new TimeSpan(0, 45, 30),
            Date = DateTime.Today,
            IsCurrentPlayer = false,
            Category = LeaderboardCategory.Speedrun
        };

        // Act
        var result = _renderer.FormatEntry(entry, LeaderboardCategory.Speedrun, _config);

        // Assert
        result.Should().Contain("0:45:30");
    }
}
