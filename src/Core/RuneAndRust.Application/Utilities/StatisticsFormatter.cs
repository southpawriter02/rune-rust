// ═══════════════════════════════════════════════════════════════════════════════
// StatisticsFormatter.cs
// Utility class for formatting statistics values for display.
// Version: 0.12.0c
// ═══════════════════════════════════════════════════════════════════════════════

using System.Globalization;
using RuneAndRust.Application.Models;
using RuneAndRust.Domain.Records;

namespace RuneAndRust.Application.Utilities;

/// <summary>
/// Provides formatting utilities for displaying statistics values.
/// </summary>
/// <remarks>
/// <para>StatisticsFormatter provides consistent formatting across all statistics displays:</para>
/// <list type="bullet">
///   <item><description>Numbers with thousands separators (e.g., "4,532")</description></item>
///   <item><description>Percentages with one decimal place (e.g., "9.2%")</description></item>
///   <item><description>Durations in human-readable format (e.g., "12h 34m 56s")</description></item>
///   <item><description>Dates in ISO format (e.g., "2026-01-20")</description></item>
///   <item><description>Combat and luck ratings with visual indicators</description></item>
///   <item><description>Dice roll history with critical highlights</description></item>
/// </list>
/// <para>All methods are static and thread-safe.</para>
/// </remarks>
public static class StatisticsFormatter
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Number Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a number with thousands separators.
    /// </summary>
    /// <param name="value">The number to format.</param>
    /// <returns>Formatted string with thousands separators (e.g., "4,532").</returns>
    /// <example>
    /// <code>
    /// StatisticsFormatter.FormatNumber(4532);     // Returns "4,532"
    /// StatisticsFormatter.FormatNumber(1000000);  // Returns "1,000,000"
    /// StatisticsFormatter.FormatNumber(0);        // Returns "0"
    /// </code>
    /// </example>
    public static string FormatNumber(long value)
    {
        return value.ToString("N0", CultureInfo.CurrentCulture);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Percentage Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a decimal value as a percentage.
    /// </summary>
    /// <param name="value">The decimal value to format (e.g., 0.092 for 9.2%).</param>
    /// <returns>Formatted percentage string (e.g., "9.2%").</returns>
    /// <remarks>
    /// Uses the current culture's percentage format with one decimal place.
    /// </remarks>
    /// <example>
    /// <code>
    /// StatisticsFormatter.FormatPercent(0.092);  // Returns "9.2%"
    /// StatisticsFormatter.FormatPercent(0.5);    // Returns "50.0%"
    /// StatisticsFormatter.FormatPercent(0.0);    // Returns "0.0%"
    /// </code>
    /// </example>
    public static string FormatPercent(double value)
    {
        return value.ToString("P1", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Formats a percentage with a sign indicator.
    /// </summary>
    /// <param name="value">The percentage value (e.g., 6.7 for +6.7%).</param>
    /// <returns>Formatted percentage string with sign (e.g., "+6.7%" or "-3.5%").</returns>
    /// <example>
    /// <code>
    /// StatisticsFormatter.FormatSignedPercent(6.7);   // Returns "+6.7%"
    /// StatisticsFormatter.FormatSignedPercent(-3.5);  // Returns "-3.5%"
    /// StatisticsFormatter.FormatSignedPercent(0.0);   // Returns "+0.0%"
    /// </code>
    /// </example>
    public static string FormatSignedPercent(double value)
    {
        var sign = value >= 0 ? "+" : "";
        return $"{sign}{value:F1}%";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Duration Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a duration in human-readable format.
    /// </summary>
    /// <param name="duration">The time span to format.</param>
    /// <returns>
    /// Formatted duration string. Format depends on magnitude:
    /// <list type="bullet">
    ///   <item><description>Days or more: "Xd Yh Zm" (e.g., "2d 5h 30m")</description></item>
    ///   <item><description>Hours or more: "Xh Ym Zs" (e.g., "12h 34m 56s")</description></item>
    ///   <item><description>Less than an hour: "Xm Ys" (e.g., "45m 30s")</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// StatisticsFormatter.FormatDuration(TimeSpan.FromHours(12.5));   // Returns "12h 30m 0s"
    /// StatisticsFormatter.FormatDuration(TimeSpan.FromMinutes(45));   // Returns "45m 0s"
    /// StatisticsFormatter.FormatDuration(TimeSpan.FromDays(2.25));    // Returns "2d 6h 0m"
    /// </code>
    /// </example>
    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
        {
            return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
        }

        if (duration.TotalHours >= 1)
        {
            return $"{(int)duration.TotalHours}h {duration.Minutes}m {duration.Seconds}s";
        }

        return $"{duration.Minutes}m {duration.Seconds}s";
    }

    /// <summary>
    /// Formats the average session duration.
    /// </summary>
    /// <param name="totalPlaytime">Total playtime across all sessions.</param>
    /// <param name="sessionCount">Number of sessions played.</param>
    /// <returns>Formatted average session duration, or "0m 0s" if no sessions.</returns>
    /// <example>
    /// <code>
    /// var total = TimeSpan.FromHours(10);
    /// StatisticsFormatter.FormatAverageSession(total, 5);  // Returns "2h 0m 0s"
    /// StatisticsFormatter.FormatAverageSession(total, 0);  // Returns "0m 0s"
    /// </code>
    /// </example>
    public static string FormatAverageSession(TimeSpan totalPlaytime, int sessionCount)
    {
        if (sessionCount <= 0)
        {
            return "0m 0s";
        }

        var averageTicks = totalPlaytime.Ticks / sessionCount;
        var averageSession = TimeSpan.FromTicks(averageTicks);
        return FormatDuration(averageSession);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Date Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a date in ISO format (yyyy-MM-dd).
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <returns>Date string in "yyyy-MM-dd" format.</returns>
    /// <example>
    /// <code>
    /// StatisticsFormatter.FormatDate(new DateTime(2026, 1, 20));  // Returns "2026-01-20"
    /// </code>
    /// </example>
    public static string FormatDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Dice Roll Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats recent dice rolls with critical highlights.
    /// </summary>
    /// <param name="rolls">The recent rolls to format.</param>
    /// <returns>
    /// Comma-separated roll results where:
    /// <list type="bullet">
    ///   <item><description>Natural 20s are followed by "!" (e.g., "20!")</description></item>
    ///   <item><description>Natural 1s are wrapped in parentheses (e.g., "(1)")</description></item>
    ///   <item><description>Normal rolls are displayed as-is (e.g., "14")</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// // Given rolls with d20 results: 18, 20, 14, 1, 12
    /// // Returns: "18, 20!, 14, (1), 12"
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rolls"/> is null.</exception>
    public static string FormatRecentRolls(IReadOnlyList<DiceRollRecord> rolls)
    {
        ArgumentNullException.ThrowIfNull(rolls, nameof(rolls));

        if (rolls.Count == 0)
        {
            return "—";
        }

        return string.Join(", ", rolls.Select(FormatSingleRoll));
    }

    /// <summary>
    /// Formats a single dice roll with critical highlighting.
    /// </summary>
    /// <param name="roll">The roll to format.</param>
    /// <returns>Formatted roll result with appropriate highlighting.</returns>
    private static string FormatSingleRoll(DiceRollRecord roll)
    {
        // Check for d20 rolls specifically for nat20/nat1 highlighting
        if (roll.IndividualRolls.Length == 1)
        {
            var dieResult = roll.IndividualRolls[0];
            if (dieResult == 20)
            {
                return $"{roll.Result}!";
            }
            if (dieResult == 1)
            {
                return $"({roll.Result})";
            }
        }
        else
        {
            // Multi-die rolls - check for any nat20 or nat1
            if (roll.IndividualRolls.Any(r => r == 20))
            {
                return $"{roll.Result}!";
            }
            if (roll.IndividualRolls.Any(r => r == 1))
            {
                return $"({roll.Result})";
            }
        }

        return roll.Result.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Combat Rating Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a combat rating with stars and label.
    /// </summary>
    /// <param name="rating">The combat rating to format.</param>
    /// <returns>
    /// Formatted rating with star visualization:
    /// <list type="bullet">
    ///   <item><description>Novice: ★☆☆☆☆ (Novice)</description></item>
    ///   <item><description>Apprentice: ★★☆☆☆ (Apprentice)</description></item>
    ///   <item><description>Journeyman: ★★★☆☆ (Journeyman)</description></item>
    ///   <item><description>Skilled: ★★★★☆ (Skilled)</description></item>
    ///   <item><description>Veteran: ★★★★★ (Veteran)</description></item>
    ///   <item><description>Master: ★★★★★+ (Master)</description></item>
    ///   <item><description>Legend: ⚔ LEGEND ⚔</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// StatisticsFormatter.FormatCombatRating(CombatRating.Skilled);  // Returns "★★★★☆ (Skilled)"
    /// StatisticsFormatter.FormatCombatRating(CombatRating.Legend);   // Returns "⚔ LEGEND ⚔"
    /// </code>
    /// </example>
    public static string FormatCombatRating(CombatRating rating)
    {
        return rating switch
        {
            CombatRating.Novice => "★☆☆☆☆ (Novice)",
            CombatRating.Apprentice => "★★☆☆☆ (Apprentice)",
            CombatRating.Journeyman => "★★★☆☆ (Journeyman)",
            CombatRating.Skilled => "★★★★☆ (Skilled)",
            CombatRating.Veteran => "★★★★★ (Veteran)",
            CombatRating.Master => "★★★★★+ (Master)",
            CombatRating.Legend => "⚔ LEGEND ⚔",
            _ => $"☆☆☆☆☆ ({rating})"
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Luck Rating Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a luck rating with emoji and percentage.
    /// </summary>
    /// <param name="rating">The luck rating to format.</param>
    /// <param name="luckPercentage">The luck percentage deviation from expected average.</param>
    /// <returns>
    /// Formatted rating with emoji indicator and percentage:
    /// <list type="bullet">
    ///   <item><description>Cursed: 💀 Cursed (-X.X%)</description></item>
    ///   <item><description>Unlucky: ☁️ Unlucky (-X.X%)</description></item>
    ///   <item><description>Average: ⚖️ Average (+/-X.X%)</description></item>
    ///   <item><description>Lucky: 🍀 Lucky (+X.X%)</description></item>
    ///   <item><description>Blessed: ✨ Blessed (+X.X%)</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// StatisticsFormatter.FormatLuckRating(LuckRating.Lucky, 6.7);    // Returns "🍀 Lucky (+6.7%)"
    /// StatisticsFormatter.FormatLuckRating(LuckRating.Cursed, -15.2); // Returns "💀 Cursed (-15.2%)"
    /// </code>
    /// </example>
    public static string FormatLuckRating(LuckRating rating, double luckPercentage)
    {
        var (emoji, label) = rating switch
        {
            LuckRating.Cursed => ("💀", "Cursed"),
            LuckRating.Unlucky => ("☁️", "Unlucky"),
            LuckRating.Average => ("⚖️", "Average"),
            LuckRating.Lucky => ("🍀", "Lucky"),
            LuckRating.Blessed => ("✨", "Blessed"),
            _ => ("❓", rating.ToString())
        };

        return $"{emoji} {label} ({FormatSignedPercent(luckPercentage)})";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Streak Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a streak value with emoji indicator.
    /// </summary>
    /// <param name="streak">The current streak count (positive = lucky, negative = unlucky).</param>
    /// <returns>
    /// Formatted streak with emoji:
    /// <list type="bullet">
    ///   <item><description>Positive: 🔥 +N (lucky streak)</description></item>
    ///   <item><description>Negative: ❄️ N (unlucky streak)</description></item>
    ///   <item><description>Zero: — 0 (no streak)</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// StatisticsFormatter.FormatStreak(5);   // Returns "🔥 +5"
    /// StatisticsFormatter.FormatStreak(-3);  // Returns "❄️ -3"
    /// StatisticsFormatter.FormatStreak(0);   // Returns "— 0"
    /// </code>
    /// </example>
    public static string FormatStreak(int streak)
    {
        return streak switch
        {
            > 0 => $"🔥 +{streak}",
            < 0 => $"❄️ {streak}",
            _ => "— 0"
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Monster Type Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a dictionary of monster kills by type as a summary string.
    /// </summary>
    /// <param name="monstersByType">Dictionary mapping monster types to kill counts.</param>
    /// <param name="maxTypes">Maximum number of types to include (default 3).</param>
    /// <returns>
    /// Comma-separated list of top monster types with counts (e.g., "Goblins: 45, Skeletons: 32"),
    /// or "—" if no kills.
    /// </returns>
    /// <example>
    /// <code>
    /// var monsters = new Dictionary&lt;string, int&gt;
    /// {
    ///     { "goblin", 45 },
    ///     { "skeleton", 32 },
    ///     { "orc", 12 }
    /// };
    /// StatisticsFormatter.FormatTopMonsters(monsters);  // Returns "Goblin: 45, Skeleton: 32, Orc: 12"
    /// </code>
    /// </example>
    public static string FormatTopMonsters(IReadOnlyDictionary<string, int> monstersByType, int maxTypes = 3)
    {
        ArgumentNullException.ThrowIfNull(monstersByType, nameof(monstersByType));

        if (monstersByType.Count == 0)
        {
            return "—";
        }

        var topMonsters = monstersByType
            .OrderByDescending(kvp => kvp.Value)
            .Take(maxTypes)
            .Select(kvp => $"{FormatMonsterName(kvp.Key)}: {kvp.Value}");

        return string.Join(", ", topMonsters);
    }

    /// <summary>
    /// Formats a monster type name with proper capitalization.
    /// </summary>
    /// <param name="monsterType">The monster type identifier (usually lowercase).</param>
    /// <returns>Properly capitalized monster name.</returns>
    private static string FormatMonsterName(string monsterType)
    {
        if (string.IsNullOrEmpty(monsterType))
        {
            return "Unknown";
        }

        // Capitalize first letter of each word
        var words = monsterType.Split(' ', '_', '-');
        var formattedWords = words.Select(w =>
            string.IsNullOrEmpty(w) ? w : char.ToUpper(w[0]) + w[1..].ToLower());

        return string.Join(" ", formattedWords);
    }
}
