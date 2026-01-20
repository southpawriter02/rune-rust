// ═══════════════════════════════════════════════════════════════════════════════
// DiceHistoryServiceTests.cs
// Unit tests for the DiceHistoryService, DiceRollRecord, and DiceRollHistory.
// Version: 0.12.0b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Models;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Records;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="DiceHistoryService"/> and related types.
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>DiceRollRecord factory method and critical detection</description></item>
///   <item><description>DiceRollHistory entity roll tracking and streak logic</description></item>
///   <item><description>DiceHistoryService constructor validation</description></item>
///   <item><description>Roll recording and history management</description></item>
///   <item><description>Statistics calculation and luck rating determination</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class DiceHistoryServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Mock logger for the service.
    /// </summary>
    private Mock<ILogger<DiceHistoryService>> _mockLogger = null!;

    /// <summary>
    /// The service under test.
    /// </summary>
    private DiceHistoryService _service = null!;

    /// <summary>
    /// Test player instance.
    /// </summary>
    private Player _player = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // Setup
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets up the test fixtures before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Initialize mocks
        _mockLogger = new Mock<ILogger<DiceHistoryService>>();

        // Create the service under test
        _service = new DiceHistoryService(_mockLogger.Object);

        // Create a test player
        _player = new Player("TestPlayer");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiceRollRecord Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create detects a natural 20 and sets WasCritical to true.
    /// </summary>
    [Test]
    public void DiceRollRecord_Create_WithNatural20_SetsWasCriticalTrue()
    {
        // Arrange & Act
        var record = DiceRollRecord.Create("1d20", 20, new[] { 20 }, "attack");

        // Assert
        record.WasCritical.Should().BeTrue();
        record.HasNatural20.Should().BeTrue();
        record.HasNatural1.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create detects a natural 1 and sets WasCritical to true.
    /// </summary>
    [Test]
    public void DiceRollRecord_Create_WithNatural1_SetsWasCriticalTrue()
    {
        // Arrange & Act
        var record = DiceRollRecord.Create("1d20", 1, new[] { 1 }, "attack");

        // Assert
        record.WasCritical.Should().BeTrue();
        record.HasNatural1.Should().BeTrue();
        record.HasNatural20.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create with a normal roll sets WasCritical to false.
    /// </summary>
    [Test]
    public void DiceRollRecord_Create_WithNormalRoll_SetsWasCriticalFalse()
    {
        // Arrange & Act
        var record = DiceRollRecord.Create("1d20", 15, new[] { 15 }, "attack");

        // Assert
        record.WasCritical.Should().BeFalse();
        record.HasNatural20.Should().BeFalse();
        record.HasNatural1.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiceRollHistory Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that RecordRoll for a d20 roll updates counts and sum.
    /// </summary>
    [Test]
    public void DiceRollHistory_RecordRoll_D20Roll_UpdatesCountsAndSum()
    {
        // Arrange
        var history = DiceRollHistory.Create(_player.Id);
        var roll = DiceRollRecord.Create("1d20", 15, new[] { 15 }, "attack");

        // Act
        history.RecordRoll(roll);

        // Assert
        history.TotalRolls.Should().Be(1);
        history.D20RollCount.Should().Be(1);
        history.D20RollSum.Should().Be(15);
        history.GetAverageD20Roll().Should().Be(15);
    }

    /// <summary>
    /// Verifies that RecordRoll increments natural 20 count when appropriate.
    /// </summary>
    [Test]
    public void DiceRollHistory_RecordRoll_Natural20_IncrementsNat20Count()
    {
        // Arrange
        var history = DiceRollHistory.Create(_player.Id);
        var roll = DiceRollRecord.Create("1d20", 20, new[] { 20 }, "attack");

        // Act
        history.RecordRoll(roll);

        // Assert
        history.TotalNaturalTwenties.Should().Be(1);
        history.TotalNaturalOnes.Should().Be(0);
    }

    /// <summary>
    /// Verifies that RecordRoll increments natural 1 count when appropriate.
    /// </summary>
    [Test]
    public void DiceRollHistory_RecordRoll_Natural1_IncrementsNat1Count()
    {
        // Arrange
        var history = DiceRollHistory.Create(_player.Id);
        var roll = DiceRollRecord.Create("1d20", 1, new[] { 1 }, "attack");

        // Act
        history.RecordRoll(roll);

        // Assert
        history.TotalNaturalOnes.Should().Be(1);
        history.TotalNaturalTwenties.Should().Be(0);
    }

    /// <summary>
    /// Verifies that RecordRoll increments the lucky streak for above-average rolls.
    /// </summary>
    [Test]
    public void DiceRollHistory_RecordRoll_AboveAverage_IncrementsLuckyStreak()
    {
        // Arrange
        var history = DiceRollHistory.Create(_player.Id);
        var roll1 = DiceRollRecord.Create("1d20", 15, new[] { 15 }, "attack"); // >= 11, lucky
        var roll2 = DiceRollRecord.Create("1d20", 18, new[] { 18 }, "attack"); // >= 11, lucky

        // Act
        history.RecordRoll(roll1);
        history.RecordRoll(roll2);

        // Assert
        history.CurrentStreak.Should().Be(2); // Positive for lucky
        history.LongestLuckyStreak.Should().Be(2);
    }

    /// <summary>
    /// Verifies that RecordRoll increments (decrements) the unlucky streak for below-average rolls.
    /// </summary>
    [Test]
    public void DiceRollHistory_RecordRoll_BelowAverage_IncrementsUnluckyStreak()
    {
        // Arrange
        var history = DiceRollHistory.Create(_player.Id);
        var roll1 = DiceRollRecord.Create("1d20", 5, new[] { 5 }, "attack"); // < 11, unlucky
        var roll2 = DiceRollRecord.Create("1d20", 3, new[] { 3 }, "attack"); // < 11, unlucky

        // Act
        history.RecordRoll(roll1);
        history.RecordRoll(roll2);

        // Assert
        history.CurrentStreak.Should().Be(-2); // Negative for unlucky
        history.LongestUnluckyStreak.Should().Be(2);
    }

    /// <summary>
    /// Verifies that RecordRoll resets streak when switching from lucky to unlucky.
    /// </summary>
    [Test]
    public void DiceRollHistory_RecordRoll_StreakReset_WhenSwitchingCategories()
    {
        // Arrange
        var history = DiceRollHistory.Create(_player.Id);
        var luckyRoll1 = DiceRollRecord.Create("1d20", 15, new[] { 15 }, "attack");
        var luckyRoll2 = DiceRollRecord.Create("1d20", 18, new[] { 18 }, "attack");
        var unluckyRoll = DiceRollRecord.Create("1d20", 5, new[] { 5 }, "attack");

        // Act
        history.RecordRoll(luckyRoll1);
        history.RecordRoll(luckyRoll2);
        history.RecordRoll(unluckyRoll);

        // Assert
        history.CurrentStreak.Should().Be(-1); // Reset to -1 for new unlucky streak
        history.LongestLuckyStreak.Should().Be(2); // Should preserve longest
    }

    /// <summary>
    /// Verifies that the recent rolls buffer maintains maximum size.
    /// </summary>
    [Test]
    public void DiceRollHistory_RecordRoll_RecentRolls_MaintainsMaximumSize()
    {
        // Arrange
        var history = DiceRollHistory.Create(_player.Id);

        // Act - Add more than MaxRecentRolls (20) rolls
        for (var i = 0; i < 25; i++)
        {
            var roll = DiceRollRecord.Create("1d20", i + 1, new[] { i + 1 }, "test");
            history.RecordRoll(roll);
        }

        // Assert
        history.RecentRolls.Count.Should().Be(DiceRollHistory.MaxRecentRolls);
        // First roll should be the 6th one added (rolls 1-5 removed)
        history.RecentRolls[0].Result.Should().Be(6);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiceHistoryService Constructor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the constructor throws when logger is null.
    /// </summary>
    [Test]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new DiceHistoryService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiceHistoryService RecordRoll Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that RecordRoll adds the roll to the player's history.
    /// </summary>
    [Test]
    public void RecordRoll_AddsToHistory()
    {
        // Arrange
        var roll = DiceRollRecord.Create("1d20", 15, new[] { 15 }, "attack");

        // Act
        _service.RecordRoll(_player, roll);

        // Assert
        var history = _service.GetHistory(_player);
        history.TotalRolls.Should().Be(1);
        history.D20RollCount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that RecordRoll initializes history if it doesn't exist.
    /// </summary>
    [Test]
    public void RecordRoll_InitializesHistoryIfNeeded()
    {
        // Arrange - Player has no history yet
        var roll = DiceRollRecord.Create("1d20", 15, new[] { 15 }, "attack");
        _player.DiceHistory.Should().BeNull();

        // Act
        _service.RecordRoll(_player, roll);

        // Assert
        _player.DiceHistory.Should().NotBeNull();
        _player.DiceHistory!.PlayerId.Should().Be(_player.Id);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiceHistoryService GetStatistics Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetStatistics calculates correct values.
    /// </summary>
    [Test]
    public void GetStatistics_CalculatesCorrectValues()
    {
        // Arrange - Record several d20 rolls
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 20, new[] { 20 }, "attack"));
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 15, new[] { 15 }, "attack"));
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 1, new[] { 1 }, "attack"));
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 10, new[] { 10 }, "attack"));
        // Average = (20 + 15 + 1 + 10) / 4 = 46 / 4 = 11.5

        // Act
        var stats = _service.GetStatistics(_player);

        // Assert
        stats.TotalRolls.Should().Be(4);
        stats.TotalNat20s.Should().Be(1);
        stats.TotalNat1s.Should().Be(1);
        stats.AverageD20.Should().BeApproximately(11.5, 0.01);
        stats.ExpectedD20.Should().Be(10.5);
        stats.Nat20Rate.Should().BeApproximately(0.25, 0.01); // 1/4
        stats.Nat1Rate.Should().BeApproximately(0.25, 0.01);  // 1/4
        // Luck % = ((11.5 - 10.5) / 10.5) * 100 ≈ 9.52%
        stats.LuckPercentage.Should().BeApproximately(9.52, 0.5);
    }

    /// <summary>
    /// Verifies that GetStatistics returns empty statistics for a new player.
    /// </summary>
    [Test]
    public void GetStatistics_NewPlayer_ReturnsDefaultValues()
    {
        // Act
        var stats = _service.GetStatistics(_player);

        // Assert
        stats.TotalRolls.Should().Be(0);
        stats.TotalNat20s.Should().Be(0);
        stats.TotalNat1s.Should().Be(0);
        stats.AverageD20.Should().Be(DiceRollHistory.D20ExpectedAverage);
        stats.LuckPercentage.Should().Be(0);
        stats.Rating.Should().Be(LuckRating.Average);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiceHistoryService GetRecentRolls Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetRecentRolls returns up to the requested count.
    /// </summary>
    [Test]
    public void GetRecentRolls_ReturnsUpToRequestedCount()
    {
        // Arrange - Record 15 rolls
        for (var i = 0; i < 15; i++)
        {
            var roll = DiceRollRecord.Create("1d20", i + 1, new[] { i + 1 }, "test");
            _service.RecordRoll(_player, roll);
        }

        // Act
        var recentRolls = _service.GetRecentRolls(_player, count: 5);

        // Assert
        recentRolls.Count.Should().Be(5);
        // Should return the last 5 rolls (11-15)
        recentRolls[0].Result.Should().Be(11);
        recentRolls[4].Result.Should().Be(15);
    }

    /// <summary>
    /// Verifies that GetRecentRolls returns all rolls when count exceeds available.
    /// </summary>
    [Test]
    public void GetRecentRolls_WhenCountExceedsAvailable_ReturnsAllAvailable()
    {
        // Arrange - Record 3 rolls
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 10, new[] { 10 }, "test"));
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 15, new[] { 15 }, "test"));
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 20, new[] { 20 }, "test"));

        // Act
        var recentRolls = _service.GetRecentRolls(_player, count: 10);

        // Assert
        recentRolls.Count.Should().Be(3);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LuckRating Calculation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CalculateLuckRating returns correct rating for various percentages.
    /// </summary>
    /// <param name="luckPercentage">The luck percentage to test.</param>
    /// <param name="expectedRating">The expected rating.</param>
    [Test]
    // The CalculateLuckRating method is internal, so we test through GetLuckRating behavior
    // by recording rolls that produce the target luck percentages
    [TestCase(3, LuckRating.Cursed)]       // Average of 3 = (3 - 10.5) / 10.5 * 100 = -71.4%
    [TestCase(7, LuckRating.Unlucky)]      // Average of 7 = (7 - 10.5) / 10.5 * 100 = -33.3%
    [TestCase(10, LuckRating.Average)]     // Average of 10 = (10 - 10.5) / 10.5 * 100 = -4.76%
    [TestCase(12, LuckRating.Lucky)]       // Average of 12 = (12 - 10.5) / 10.5 * 100 = +14.3%
    [TestCase(18, LuckRating.Blessed)]     // Average of 18 = (18 - 10.5) / 10.5 * 100 = +71.4%
    public void GetLuckRating_WithVariousAverages_ReturnsCorrectRating(int rollValue, LuckRating expectedRating)
    {
        // Arrange - Record a single roll with the target value
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", rollValue, new[] { rollValue }, "test"));

        // Act
        var rating = _service.GetLuckRating(_player);

        // Assert
        rating.Should().Be(expectedRating);
    }

    /// <summary>
    /// Verifies that GetLuckRating returns correct rating based on player history.
    /// </summary>
    [Test]
    public void GetLuckRating_BasedOnHistory_ReturnsCorrectRating()
    {
        // Arrange - Record rolls that produce a lucky average
        // Need average > 11.025 (10.5 * 1.05) for Lucky rating
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 15, new[] { 15 }, "test"));
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 16, new[] { 16 }, "test"));
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 14, new[] { 14 }, "test"));
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 15, new[] { 15 }, "test"));
        // Average = (15 + 16 + 14 + 15) / 4 = 60 / 4 = 15
        // Luck % = ((15 - 10.5) / 10.5) * 100 = 42.86% -> Blessed

        // Act
        var rating = _service.GetLuckRating(_player);

        // Assert
        rating.Should().Be(LuckRating.Blessed);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiceHistoryService GetCurrentStreak Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetCurrentStreak returns the correct streak value.
    /// </summary>
    [Test]
    public void GetCurrentStreak_ReturnsCorrectStreak()
    {
        // Arrange - Record 3 lucky rolls
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 15, new[] { 15 }, "test"));
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 18, new[] { 18 }, "test"));
        _service.RecordRoll(_player, DiceRollRecord.Create("1d20", 12, new[] { 12 }, "test"));

        // Act
        var streak = _service.GetCurrentStreak(_player);

        // Assert
        streak.Should().Be(3); // 3 consecutive lucky rolls (>= 11)
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiceStatistics Display Helper Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that DiceStatistics display helpers format values correctly.
    /// </summary>
    [Test]
    public void DiceStatistics_DisplayHelpers_FormatCorrectly()
    {
        // Arrange
        var stats = new DiceStatistics(
            TotalRolls: 100,
            TotalNat20s: 7,
            TotalNat1s: 3,
            AverageD20: 11.5,
            ExpectedD20: 10.5,
            Nat20Rate: 0.07,
            ExpectedNat20Rate: 0.05,
            Nat1Rate: 0.03,
            ExpectedNat1Rate: 0.05,
            LuckPercentage: 9.52,
            Rating: LuckRating.Lucky,
            CurrentStreak: 5,
            LongestLuckyStreak: 8,
            LongestUnluckyStreak: 4);

        // Assert
        stats.Nat20RateDisplay.Should().Be("7.00%");
        stats.Nat1RateDisplay.Should().Be("3.00%");
        stats.LuckPercentageDisplay.Should().Be("+9.52%");
        stats.AverageD20Display.Should().Be("11.50");
        stats.CurrentStreakDisplay.Should().Contain("+5"); // Contains fire emoji and +5
        stats.HasRollHistory.Should().BeTrue();
        stats.IsOnLuckyStreak.Should().BeTrue();
        stats.IsOnUnluckyStreak.Should().BeFalse();
        stats.CurrentStreakLength.Should().Be(5);
    }

    /// <summary>
    /// Verifies that DiceStatistics.Empty returns correct default values.
    /// </summary>
    [Test]
    public void DiceStatistics_Empty_ReturnsDefaultValues()
    {
        // Act
        var empty = DiceStatistics.Empty;

        // Assert
        empty.TotalRolls.Should().Be(0);
        empty.HasRollHistory.Should().BeFalse();
        empty.Rating.Should().Be(LuckRating.Average);
        empty.AverageD20.Should().Be(10.5);
        empty.LuckPercentage.Should().Be(0);
    }
}
