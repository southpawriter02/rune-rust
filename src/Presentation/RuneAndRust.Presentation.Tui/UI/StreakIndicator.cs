// ═══════════════════════════════════════════════════════════════════════════════
// StreakIndicator.cs
// UI component that displays dice roll streak information.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Component that displays dice roll streak information including
/// current streak and longest historical streaks.
/// </summary>
/// <remarks>
/// <para>
/// A "streak" is defined as consecutive rolls that are all above or
/// all below the expected average (10.5 for d20). Positive streaks
/// indicate "lucky" runs, negative indicate "unlucky" runs.
/// </para>
/// <para>Features:</para>
/// <list type="bullet">
///   <item><description>Current streak display with direction indicator (+/-)</description></item>
///   <item><description>Human-readable streak descriptions</description></item>
///   <item><description>Longest lucky/unlucky streak history display</description></item>
///   <item><description>Color-coded streak values (green/red)</description></item>
/// </list>
/// </remarks>
public class StreakIndicator
{
    private readonly ITerminalService _terminal;
    private readonly DiceHistoryPanelConfig _config;
    private readonly ILogger<StreakIndicator>? _logger;

    /// <summary>
    /// Creates a new streak indicator with the specified dependencies.
    /// </summary>
    /// <param name="terminal">Terminal service for output.</param>
    /// <param name="config">Panel configuration settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public StreakIndicator(
        ITerminalService terminal,
        DiceHistoryPanelConfig config,
        ILogger<StreakIndicator>? logger = null)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;

        _logger?.LogDebug("StreakIndicator initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Rendering
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the current streak indicator at the specified position.
    /// </summary>
    /// <param name="streakCount">Number of consecutive rolls in streak (positive = lucky).</param>
    /// <param name="isLucky">True if streak is above average, false if below.</param>
    /// <param name="x">X coordinate for rendering.</param>
    /// <param name="y">Y coordinate for rendering.</param>
    /// <remarks>
    /// <para>Displays the streak as "+N" (green) or "-N" (red) with a description.</para>
    /// </remarks>
    public void RenderStreak(int streakCount, bool isLucky, int x, int y)
    {
        _logger?.LogDebug(
            "Rendering streak: Count={Count}, IsLucky={IsLucky} at ({X},{Y})",
            streakCount, isLucky, x, y);

        var streakText = FormatStreak(streakCount, isLucky);
        var color = GetStreakColor(streakCount);

        _terminal.WriteAt(x, y, "Current Streak: ");
        _terminal.WriteColoredAt(x + 16, y, streakText, color);

        // Show description on next line
        var description = GetStreakDescription(streakCount, isLucky);
        _terminal.WriteAt(x, y + 1, description);
    }

    /// <summary>
    /// Displays the longest lucky and unlucky streaks from history.
    /// </summary>
    /// <param name="longestLucky">Longest above-average streak.</param>
    /// <param name="longestUnlucky">Longest below-average streak.</param>
    /// <param name="x">X coordinate for rendering.</param>
    /// <param name="y">Y coordinate for rendering.</param>
    /// <remarks>
    /// <para>Displays "Longest Lucky: +N" and "Longest Unlucky: -N" on separate lines.</para>
    /// </remarks>
    public void ShowLongestStreaks(int longestLucky, int longestUnlucky, int x, int y)
    {
        _logger?.LogDebug(
            "Showing longest streaks: Lucky={Lucky}, Unlucky={Unlucky}",
            longestLucky, longestUnlucky);

        // Longest Lucky
        _terminal.WriteAt(x, y, "Longest Lucky: ");
        _terminal.WriteColoredAt(x + 15, y, $"+{longestLucky}", _config.LuckyStreakColor);

        // Longest Unlucky
        _terminal.WriteAt(x, y + 1, "Longest Unlucky: ");
        _terminal.WriteColoredAt(x + 17, y + 1, $"-{longestUnlucky}", _config.UnluckyStreakColor);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a streak count as a signed string.
    /// </summary>
    /// <param name="streakCount">The streak count (absolute value).</param>
    /// <param name="isLucky">True if lucky streak, false if unlucky.</param>
    /// <returns>Formatted string like "+3" or "-5".</returns>
    public static string FormatStreak(int streakCount, bool isLucky)
    {
        if (streakCount == 0)
        {
            return "0";
        }

        return isLucky ? $"+{streakCount}" : $"-{streakCount}";
    }

    /// <summary>
    /// Formats a signed streak value as a string.
    /// </summary>
    /// <param name="signedStreak">The streak value (positive = lucky, negative = unlucky).</param>
    /// <returns>Formatted string like "+3" or "-5".</returns>
    public static string FormatStreak(int signedStreak)
    {
        if (signedStreak == 0)
        {
            return "0";
        }

        return signedStreak > 0 ? $"+{signedStreak}" : signedStreak.ToString();
    }

    /// <summary>
    /// Gets a human-readable description of the streak.
    /// </summary>
    /// <param name="streakCount">Number of consecutive rolls in streak (absolute value).</param>
    /// <param name="isLucky">True if lucky streak, false if unlucky.</param>
    /// <returns>Description like "(3 above-average in a row)".</returns>
    /// <remarks>
    /// <para>Special cases:</para>
    /// <list type="bullet">
    ///   <item><description>0 streak: "(no streak)"</description></item>
    ///   <item><description>1 streak: "(1 roll above/below average)"</description></item>
    ///   <item><description>N streak: "(N above/below-average in a row)"</description></item>
    /// </list>
    /// </remarks>
    public string GetStreakDescription(int streakCount, bool isLucky)
    {
        if (streakCount == 0)
        {
            return "(no streak)";
        }

        var direction = isLucky ? "above" : "below";
        
        if (streakCount == 1)
        {
            return $"(1 roll {direction} average)";
        }

        return $"({streakCount} {direction}-average in a row)";
    }

    /// <summary>
    /// Gets a human-readable description of the streak using signed value.
    /// </summary>
    /// <param name="signedStreak">Signed streak value (positive = lucky).</param>
    /// <returns>Description like "(3 above-average in a row)".</returns>
    public string GetStreakDescription(int signedStreak)
    {
        var isLucky = signedStreak > 0;
        var count = Math.Abs(signedStreak);
        return GetStreakDescription(count, isLucky);
    }

    /// <summary>
    /// Gets the appropriate color for the streak display.
    /// </summary>
    /// <param name="streakCount">Streak count (positive or negative).</param>
    /// <returns>Console color for the streak value.</returns>
    /// <remarks>
    /// <para>Color mapping:</para>
    /// <list type="bullet">
    ///   <item><description>Positive streak: Green (lucky)</description></item>
    ///   <item><description>Negative streak: Red (unlucky)</description></item>
    ///   <item><description>Zero: White (neutral)</description></item>
    /// </list>
    /// </remarks>
    public ConsoleColor GetStreakColor(int streakCount)
    {
        return streakCount switch
        {
            > 0 => _config.LuckyStreakColor,
            < 0 => _config.UnluckyStreakColor,
            _ => ConsoleColor.White
        };
    }
}
