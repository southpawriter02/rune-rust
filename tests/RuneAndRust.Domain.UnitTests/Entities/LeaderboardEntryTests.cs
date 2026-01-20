// ═══════════════════════════════════════════════════════════════════════════════
// LeaderboardEntryTests.cs
// Unit tests for the LeaderboardEntry entity.
// Version: 0.12.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.Entities;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for the <see cref="LeaderboardEntry"/> entity.
/// </summary>
[TestFixture]
public class LeaderboardEntryTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidParameters_CreatesEntryWithCorrectValues()
    {
        // Arrange & Act
        var entry = LeaderboardEntry.Create(
            playerName: "Thorin",
            className: "Warrior",
            level: 10,
            score: 50000,
            category: LeaderboardCategory.HighScore);

        // Assert
        entry.Id.Should().NotBe(Guid.Empty);
        entry.PlayerName.Should().Be("Thorin");
        entry.ClassName.Should().Be("Warrior");
        entry.Level.Should().Be(10);
        entry.Score.Should().Be(50000);
        entry.Category.Should().Be(LeaderboardCategory.HighScore);
        entry.AchievedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entry.CompletionTime.Should().BeNull();
    }

    [Test]
    public void Create_WithCompletionTime_SetsCompletionTime()
    {
        // Arrange
        var completionTime = TimeSpan.FromMinutes(45);

        // Act
        var entry = LeaderboardEntry.Create(
            playerName: "Speedster",
            className: "Rogue",
            level: 20,
            score: 2700,
            category: LeaderboardCategory.Speedrun,
            completionTime: completionTime);

        // Assert
        entry.CompletionTime.Should().Be(completionTime);
        entry.Category.Should().Be(LeaderboardCategory.Speedrun);
    }

    [Test]
    public void Create_WithNullPlayerName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => LeaderboardEntry.Create(
            playerName: null!,
            className: "Warrior",
            level: 10,
            score: 1000,
            category: LeaderboardCategory.HighScore);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithEmptyClassName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => LeaderboardEntry.Create(
            playerName: "Hero",
            className: "",
            level: 10,
            score: 1000,
            category: LeaderboardCategory.HighScore);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithNegativeLevel_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => LeaderboardEntry.Create(
            playerName: "Hero",
            className: "Warrior",
            level: -1,
            score: 1000,
            category: LeaderboardCategory.HighScore);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Create_WithNegativeScore_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => LeaderboardEntry.Create(
            playerName: "Hero",
            className: "Warrior",
            level: 10,
            score: -100,
            category: LeaderboardCategory.HighScore);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetScoreDisplay_ForHighScore_ReturnsFormattedNumber()
    {
        // Arrange
        var entry = LeaderboardEntry.Create(
            playerName: "Hero",
            className: "Warrior",
            level: 10,
            score: 125430,
            category: LeaderboardCategory.HighScore);

        // Act
        var display = entry.GetScoreDisplay();

        // Assert
        display.Should().Be("125,430");
    }

    [Test]
    public void GetScoreDisplay_ForSpeedrun_ReturnsFormattedTime()
    {
        // Arrange
        var completionTime = new TimeSpan(1, 23, 45); // 1:23:45
        var entry = LeaderboardEntry.Create(
            playerName: "Speedster",
            className: "Rogue",
            level: 20,
            score: (long)completionTime.TotalSeconds,
            category: LeaderboardCategory.Speedrun,
            completionTime: completionTime);

        // Act
        var display = entry.GetScoreDisplay();

        // Assert
        display.Should().Be("01:23:45");
    }

    [Test]
    public void GetScoreDisplay_ForOtherCategories_ReturnsFormattedNumber()
    {
        // Arrange
        var entry = LeaderboardEntry.Create(
            playerName: "Hunter",
            className: "Warrior",
            level: 15,
            score: 25,
            category: LeaderboardCategory.BossSlayer);

        // Act
        var display = entry.GetScoreDisplay();

        // Assert
        display.Should().Be("25");
    }

    [Test]
    public void ToString_ReturnsDescriptiveString()
    {
        // Arrange
        var entry = LeaderboardEntry.Create(
            playerName: "Thorin",
            className: "Warrior",
            level: 10,
            score: 50000,
            category: LeaderboardCategory.HighScore);

        // Act
        var result = entry.ToString();

        // Assert
        result.Should().Contain("Thorin");
        result.Should().Contain("Warrior");
        result.Should().Contain("Level 10");
        result.Should().Contain("HighScore");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CATEGORY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(LeaderboardCategory.HighScore)]
    [TestCase(LeaderboardCategory.Speedrun)]
    [TestCase(LeaderboardCategory.NoDeath)]
    [TestCase(LeaderboardCategory.AchievementPoints)]
    [TestCase(LeaderboardCategory.BossSlayer)]
    public void Create_WithAnyCategory_SetsCategory(LeaderboardCategory category)
    {
        // Arrange & Act
        var entry = LeaderboardEntry.Create(
            playerName: "Hero",
            className: "Warrior",
            level: 10,
            score: 1000,
            category: category);

        // Assert
        entry.Category.Should().Be(category);
    }
}
