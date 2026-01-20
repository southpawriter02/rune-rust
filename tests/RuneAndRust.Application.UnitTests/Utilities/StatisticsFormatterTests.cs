// ═══════════════════════════════════════════════════════════════════════════════
// StatisticsFormatterTests.cs
// Unit tests for the StatisticsFormatter utility class.
// Version: 0.12.0c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Application.Models;
using RuneAndRust.Application.Utilities;
using RuneAndRust.Domain.Records;

namespace RuneAndRust.Application.UnitTests.Utilities;

/// <summary>
/// Unit tests for the <see cref="StatisticsFormatter"/> utility class.
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Number formatting with thousands separators</description></item>
///   <item><description>Percentage formatting</description></item>
///   <item><description>Duration formatting at various scales</description></item>
///   <item><description>Date formatting in ISO format</description></item>
///   <item><description>Recent rolls formatting with critical highlights</description></item>
///   <item><description>Combat rating star visualization</description></item>
///   <item><description>Luck rating emoji and percentage formatting</description></item>
///   <item><description>Streak formatting with emoji indicators</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class StatisticsFormatterTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Number Formatting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that numbers with thousands are formatted with commas.
    /// </summary>
    [Test]
    public void FormatNumber_WithThousands_AddsCommas()
    {
        // Act
        var result = StatisticsFormatter.FormatNumber(4532);

        // Assert
        result.Should().Be("4,532");
    }

    /// <summary>
    /// Verifies that numbers in the millions are formatted correctly.
    /// </summary>
    [Test]
    public void FormatNumber_WithMillions_AddsCommas()
    {
        // Act
        var result = StatisticsFormatter.FormatNumber(1000000);

        // Assert
        result.Should().Be("1,000,000");
    }

    /// <summary>
    /// Verifies that zero is formatted without commas.
    /// </summary>
    [Test]
    public void FormatNumber_WithZero_ReturnsZero()
    {
        // Act
        var result = StatisticsFormatter.FormatNumber(0);

        // Assert
        result.Should().Be("0");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Percentage Formatting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that decimal values are formatted as percentages correctly.
    /// </summary>
    [Test]
    public void FormatPercent_WithDecimal_FormatsCorrectly()
    {
        // Act
        var result = StatisticsFormatter.FormatPercent(0.092);

        // Assert
        // Note: P1 format in en-US locale produces "9.2%", but may vary by culture
        // We check that it contains the expected number
        result.Should().Contain("9.2").And.Contain("%");
    }

    /// <summary>
    /// Verifies that zero is formatted as "0.0%".
    /// </summary>
    [Test]
    public void FormatPercent_WithZero_ReturnsZeroPercent()
    {
        // Act
        var result = StatisticsFormatter.FormatPercent(0.0);

        // Assert
        result.Should().Contain("0.0").And.Contain("%");
    }

    /// <summary>
    /// Verifies that signed percentages include the correct sign.
    /// </summary>
    [Test]
    [TestCase(6.7, "+6.7%")]
    [TestCase(-3.5, "-3.5%")]
    [TestCase(0.0, "+0.0%")]
    public void FormatSignedPercent_IncludesCorrectSign(double value, string expected)
    {
        // Act
        var result = StatisticsFormatter.FormatSignedPercent(value);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Duration Formatting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that durations with hours include hours, minutes, and seconds.
    /// </summary>
    [Test]
    public void FormatDuration_WithHours_IncludesHoursMinutesSeconds()
    {
        // Arrange
        var duration = TimeSpan.FromHours(12) + TimeSpan.FromMinutes(30);

        // Act
        var result = StatisticsFormatter.FormatDuration(duration);

        // Assert
        result.Should().Be("12h 30m 0s");
    }

    /// <summary>
    /// Verifies that durations with days include days, hours, and minutes.
    /// </summary>
    [Test]
    public void FormatDuration_WithDays_IncludesDaysHoursMinutes()
    {
        // Arrange
        var duration = TimeSpan.FromDays(2) + TimeSpan.FromHours(5) + TimeSpan.FromMinutes(30);

        // Act
        var result = StatisticsFormatter.FormatDuration(duration);

        // Assert
        result.Should().Be("2d 5h 30m");
    }

    /// <summary>
    /// Verifies that durations less than an hour include minutes and seconds.
    /// </summary>
    [Test]
    public void FormatDuration_LessThanHour_IncludesMinutesSeconds()
    {
        // Arrange
        var duration = TimeSpan.FromMinutes(45) + TimeSpan.FromSeconds(30);

        // Act
        var result = StatisticsFormatter.FormatDuration(duration);

        // Assert
        result.Should().Be("45m 30s");
    }

    /// <summary>
    /// Verifies that average session is calculated correctly.
    /// </summary>
    [Test]
    public void FormatAverageSession_WithValidData_CalculatesCorrectly()
    {
        // Arrange
        var totalPlaytime = TimeSpan.FromHours(10);
        var sessionCount = 5;

        // Act
        var result = StatisticsFormatter.FormatAverageSession(totalPlaytime, sessionCount);

        // Assert
        result.Should().Be("2h 0m 0s");
    }

    /// <summary>
    /// Verifies that average session with zero sessions returns default.
    /// </summary>
    [Test]
    public void FormatAverageSession_WithZeroSessions_ReturnsDefault()
    {
        // Arrange
        var totalPlaytime = TimeSpan.FromHours(10);
        var sessionCount = 0;

        // Act
        var result = StatisticsFormatter.FormatAverageSession(totalPlaytime, sessionCount);

        // Assert
        result.Should().Be("0m 0s");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Date Formatting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that dates are formatted in ISO format.
    /// </summary>
    [Test]
    public void FormatDate_ReturnsIsoFormat()
    {
        // Arrange
        var date = new DateTime(2026, 1, 20);

        // Act
        var result = StatisticsFormatter.FormatDate(date);

        // Assert
        result.Should().Be("2026-01-20");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Recent Rolls Formatting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that natural 20s have exclamation appended.
    /// </summary>
    [Test]
    public void FormatRecentRolls_WithNat20_AppendsExclamation()
    {
        // Arrange
        var rolls = new List<DiceRollRecord>
        {
            DiceRollRecord.Create("1d20", 20, new[] { 20 }, "test")
        };

        // Act
        var result = StatisticsFormatter.FormatRecentRolls(rolls);

        // Assert
        result.Should().Be("20!");
    }

    /// <summary>
    /// Verifies that natural 1s are wrapped in parentheses.
    /// </summary>
    [Test]
    public void FormatRecentRolls_WithNat1_WrapsInParentheses()
    {
        // Arrange
        var rolls = new List<DiceRollRecord>
        {
            DiceRollRecord.Create("1d20", 1, new[] { 1 }, "test")
        };

        // Act
        var result = StatisticsFormatter.FormatRecentRolls(rolls);

        // Assert
        result.Should().Be("(1)");
    }

    /// <summary>
    /// Verifies that normal rolls are displayed as-is.
    /// </summary>
    [Test]
    public void FormatRecentRolls_WithNormalRoll_DisplaysAsIs()
    {
        // Arrange
        var rolls = new List<DiceRollRecord>
        {
            DiceRollRecord.Create("1d20", 14, new[] { 14 }, "test")
        };

        // Act
        var result = StatisticsFormatter.FormatRecentRolls(rolls);

        // Assert
        result.Should().Be("14");
    }

    /// <summary>
    /// Verifies that mixed rolls are formatted correctly.
    /// </summary>
    [Test]
    public void FormatRecentRolls_WithMixedRolls_FormatsCorrectly()
    {
        // Arrange
        var rolls = new List<DiceRollRecord>
        {
            DiceRollRecord.Create("1d20", 18, new[] { 18 }, "test"),
            DiceRollRecord.Create("1d20", 20, new[] { 20 }, "test"),
            DiceRollRecord.Create("1d20", 1, new[] { 1 }, "test"),
            DiceRollRecord.Create("1d20", 12, new[] { 12 }, "test")
        };

        // Act
        var result = StatisticsFormatter.FormatRecentRolls(rolls);

        // Assert
        result.Should().Be("18, 20!, (1), 12");
    }

    /// <summary>
    /// Verifies that empty rolls list returns dash.
    /// </summary>
    [Test]
    public void FormatRecentRolls_WithEmptyList_ReturnsDash()
    {
        // Arrange
        var rolls = new List<DiceRollRecord>();

        // Act
        var result = StatisticsFormatter.FormatRecentRolls(rolls);

        // Assert
        result.Should().Be("—");
    }

    /// <summary>
    /// Verifies that null rolls throws ArgumentNullException.
    /// </summary>
    [Test]
    public void FormatRecentRolls_WithNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => StatisticsFormatter.FormatRecentRolls(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("rolls");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Combat Rating Formatting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that combat ratings display correct stars and labels.
    /// </summary>
    [Test]
    [TestCase(CombatRating.Novice, "★☆☆☆☆ (Novice)")]
    [TestCase(CombatRating.Apprentice, "★★☆☆☆ (Apprentice)")]
    [TestCase(CombatRating.Journeyman, "★★★☆☆ (Journeyman)")]
    [TestCase(CombatRating.Skilled, "★★★★☆ (Skilled)")]
    [TestCase(CombatRating.Veteran, "★★★★★ (Veteran)")]
    [TestCase(CombatRating.Master, "★★★★★+ (Master)")]
    [TestCase(CombatRating.Legend, "⚔ LEGEND ⚔")]
    public void FormatCombatRating_ReturnsCorrectStarsAndLabel(CombatRating rating, string expected)
    {
        // Act
        var result = StatisticsFormatter.FormatCombatRating(rating);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Luck Rating Formatting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that luck ratings display correct emoji and percentage.
    /// </summary>
    [Test]
    [TestCase(LuckRating.Cursed, -15.2, "💀 Cursed (-15.2%)")]
    [TestCase(LuckRating.Unlucky, -7.0, "☁️ Unlucky (-7.0%)")]
    [TestCase(LuckRating.Average, 0.0, "⚖️ Average (+0.0%)")]
    [TestCase(LuckRating.Lucky, 6.7, "🍀 Lucky (+6.7%)")]
    [TestCase(LuckRating.Blessed, 15.0, "✨ Blessed (+15.0%)")]
    public void FormatLuckRating_ReturnsCorrectEmojiAndPercentage(
        LuckRating rating, double percentage, string expected)
    {
        // Act
        var result = StatisticsFormatter.FormatLuckRating(rating, percentage);

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Streak Formatting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that positive streaks show fire emoji.
    /// </summary>
    [Test]
    public void FormatStreak_WithPositive_ShowsFireEmoji()
    {
        // Act
        var result = StatisticsFormatter.FormatStreak(5);

        // Assert
        result.Should().Be("🔥 +5");
    }

    /// <summary>
    /// Verifies that negative streaks show ice emoji.
    /// </summary>
    [Test]
    public void FormatStreak_WithNegative_ShowsIceEmoji()
    {
        // Act
        var result = StatisticsFormatter.FormatStreak(-3);

        // Assert
        result.Should().Be("❄️ -3");
    }

    /// <summary>
    /// Verifies that zero streak shows dash.
    /// </summary>
    [Test]
    public void FormatStreak_WithZero_ShowsDash()
    {
        // Act
        var result = StatisticsFormatter.FormatStreak(0);

        // Assert
        result.Should().Be("— 0");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Monster Type Formatting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that top monsters are formatted correctly.
    /// </summary>
    [Test]
    public void FormatTopMonsters_WithData_FormatsCorrectly()
    {
        // Arrange
        var monsters = new Dictionary<string, int>
        {
            { "goblin", 45 },
            { "skeleton", 32 },
            { "orc", 12 }
        };

        // Act
        var result = StatisticsFormatter.FormatTopMonsters(monsters);

        // Assert
        result.Should().Contain("Goblin: 45");
        result.Should().Contain("Skeleton: 32");
        result.Should().Contain("Orc: 12");
    }

    /// <summary>
    /// Verifies that empty monster dictionary returns dash.
    /// </summary>
    [Test]
    public void FormatTopMonsters_WithEmpty_ReturnsDash()
    {
        // Arrange
        var monsters = new Dictionary<string, int>();

        // Act
        var result = StatisticsFormatter.FormatTopMonsters(monsters);

        // Assert
        result.Should().Be("—");
    }

    /// <summary>
    /// Verifies that monster count is limited by maxTypes parameter.
    /// </summary>
    [Test]
    public void FormatTopMonsters_WithMaxTypes_LimitsOutput()
    {
        // Arrange
        var monsters = new Dictionary<string, int>
        {
            { "goblin", 100 },
            { "skeleton", 80 },
            { "orc", 60 },
            { "troll", 40 },
            { "dragon", 20 }
        };

        // Act
        var result = StatisticsFormatter.FormatTopMonsters(monsters, maxTypes: 2);

        // Assert
        result.Should().Contain("Goblin: 100");
        result.Should().Contain("Skeleton: 80");
        result.Should().NotContain("Orc");
        result.Should().NotContain("Troll");
        result.Should().NotContain("Dragon");
    }
}
