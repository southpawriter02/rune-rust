// ═══════════════════════════════════════════════════════════════════════════════
// LeaderboardEntryRenderer.cs
// Renderer for formatting leaderboard entries with rank indicators and colors.
// Version: 0.13.4c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renders individual leaderboard entries with proper formatting.
/// </summary>
/// <remarks>
/// <para>
/// Handles rank indicators (#1, #2, #3 for top three, &gt;N for current player),
/// column alignment, score/time/floor formatting, and date display.
/// </para>
/// <para>Rank formatting rules:</para>
/// <list type="bullet">
///   <item><description>Top 3: #1, #2, #3 with special colors</description></item>
///   <item><description>Other: Plain number (4, 5, etc.)</description></item>
///   <item><description>Current player: &gt;N (e.g., &gt;5)</description></item>
/// </list>
/// </remarks>
public class LeaderboardEntryRenderer
{
    private readonly ILogger<LeaderboardEntryRenderer>? _logger;

    /// <summary>
    /// Creates a new instance of the LeaderboardEntryRenderer.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public LeaderboardEntryRenderer(ILogger<LeaderboardEntryRenderer>? logger = null)
    {
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ENTRY FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a leaderboard entry for display.
    /// </summary>
    /// <param name="entry">The entry to format.</param>
    /// <param name="category">The leaderboard category.</param>
    /// <param name="config">The view configuration.</param>
    /// <returns>The formatted entry string with proper column alignment.</returns>
    public string FormatEntry(
        LeaderboardDisplayDto entry,
        LeaderboardCategory category,
        LeaderboardViewConfig config)
    {
        var rankStr = FormatRank(entry.Rank, entry.IsCurrentPlayer);
        var rankCol = rankStr.PadRight(config.RankColumnWidth);

        var nameStr = entry.IsCurrentPlayer
            ? $"{entry.PlayerName} (You)"
            : entry.PlayerName;
        nameStr = TruncateOrPad(nameStr, config.NameColumnWidth);

        var classCol = TruncateOrPad(entry.CharacterClass, config.ClassColumnWidth);
        var levelCol = entry.Level.ToString().PadLeft(config.LevelColumnWidth - 2).PadRight(config.LevelColumnWidth);
        var scoreCol = FormatScoreColumn(entry, category).PadLeft(config.ScoreColumnWidth - 2).PadRight(config.ScoreColumnWidth);
        var dateCol = FormatDate(entry.Date);

        return $"{rankCol}{nameStr}{classCol}{levelCol}{scoreCol}{dateCol}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RANK FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the rank indicator.
    /// </summary>
    /// <param name="rank">The rank number (1-based).</param>
    /// <param name="isCurrentPlayer">Whether this is the current player's entry.</param>
    /// <returns>The formatted rank string (e.g., "#1", "&gt;5", "4").</returns>
    /// <example>
    /// <code>
    /// FormatRank(1, false)  → "#1"
    /// FormatRank(5, true)   → ">5"
    /// FormatRank(4, false)  → "4"
    /// </code>
    /// </example>
    public string FormatRank(int rank, bool isCurrentPlayer)
    {
        if (isCurrentPlayer)
        {
            return $">{rank}";
        }

        return rank switch
        {
            1 => "#1",
            2 => "#2",
            3 => "#3",
            _ => rank.ToString()
        };
    }

    /// <summary>
    /// Gets the console color for a rank.
    /// </summary>
    /// <param name="rank">The rank number.</param>
    /// <param name="isCurrentPlayer">Whether this is the current player's entry.</param>
    /// <returns>The appropriate console color.</returns>
    /// <remarks>
    /// Color mapping:
    /// <list type="bullet">
    ///   <item><description>Current Player: Cyan</description></item>
    ///   <item><description>#1: Yellow (Gold)</description></item>
    ///   <item><description>#2: Gray (Silver)</description></item>
    ///   <item><description>#3: DarkYellow (Bronze)</description></item>
    ///   <item><description>4+: White</description></item>
    /// </list>
    /// </remarks>
    public ConsoleColor GetRankColor(int rank, bool isCurrentPlayer)
    {
        if (isCurrentPlayer)
        {
            return ConsoleColor.Cyan;
        }

        return rank switch
        {
            1 => ConsoleColor.Yellow,      // Gold
            2 => ConsoleColor.Gray,        // Silver
            3 => ConsoleColor.DarkYellow,  // Bronze
            _ => ConsoleColor.White
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALUE FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a score value with comma separators.
    /// </summary>
    /// <param name="score">The score to format.</param>
    /// <returns>The formatted score string (e.g., "125,430").</returns>
    public string FormatScore(long score)
    {
        return score.ToString("N0");
    }

    /// <summary>
    /// Formats a date in ISO format.
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <returns>The formatted date string (YYYY-MM-DD).</returns>
    public string FormatDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Formats a time span for speedrun display.
    /// </summary>
    /// <param name="time">The time span to format.</param>
    /// <returns>The formatted time string (H:MM:SS).</returns>
    /// <example>
    /// <code>
    /// FormatTime(TimeSpan.FromMinutes(83).Add(TimeSpan.FromSeconds(45)))
    /// // Returns "1:23:45"
    /// </code>
    /// </example>
    public string FormatTime(TimeSpan time)
    {
        return $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the score column based on the leaderboard category.
    /// </summary>
    private string FormatScoreColumn(LeaderboardDisplayDto entry, LeaderboardCategory category)
    {
        return category switch
        {
            LeaderboardCategory.Speedrun => FormatTime(entry.TimeElapsed ?? TimeSpan.Zero),
            LeaderboardCategory.NoDeath => entry.FloorsCleared?.ToString() ?? "0",
            LeaderboardCategory.BossSlayer => entry.BossesDefeated?.ToString() ?? "0",
            _ => FormatScore(entry.Score)
        };
    }

    /// <summary>
    /// Truncates or pads a string to fit a column width.
    /// </summary>
    private static string TruncateOrPad(string text, int width)
    {
        if (text.Length > width - 1)
        {
            return text.Substring(0, width - 4) + "...";
        }
        return text.PadRight(width);
    }
}
