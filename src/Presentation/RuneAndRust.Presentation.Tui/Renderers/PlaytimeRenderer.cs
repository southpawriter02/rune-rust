// ═══════════════════════════════════════════════════════════════════════════════
// PlaytimeRenderer.cs
// Renderer for formatting playtime values in human-readable format.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renders playtime values in human-readable format with color-coded milestones.
/// </summary>
/// <remarks>
/// <para>
/// Formats playtime durations using the "#h ##m" format, with special handling
/// for short durations and long playtimes. Provides color coding based on
/// playtime milestones to reward dedicated players.
/// </para>
/// <para>Formatting rules:</para>
/// <list type="bullet">
///   <item><description>Less than 1 minute: "&lt;1m"</description></item>
///   <item><description>Less than 1 hour: "##m" (e.g., "45m")</description></item>
///   <item><description>1+ hours: "#h ##m" (e.g., "2h 15m")</description></item>
///   <item><description>100+ hours: "###h ##m" (e.g., "156h 20m")</description></item>
/// </list>
/// </remarks>
public class PlaytimeRenderer
{
    private readonly ILogger<PlaytimeRenderer>? _logger;

    /// <summary>
    /// Creates a new instance of the PlaytimeRenderer.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public PlaytimeRenderer(ILogger<PlaytimeRenderer>? logger = null)
    {
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SESSION TIME FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the current session time for display.
    /// </summary>
    /// <param name="sessionTime">The current session duration.</param>
    /// <returns>A human-readable session time string (e.g., "2h 15m").</returns>
    /// <example>
    /// <code>
    /// var renderer = new PlaytimeRenderer();
    /// var display = renderer.FormatSessionTime(TimeSpan.FromMinutes(135));
    /// // Returns "2h 15m"
    /// </code>
    /// </example>
    public string FormatSessionTime(TimeSpan sessionTime)
    {
        _logger?.LogDebug("Formatting session time: {SessionTime}", sessionTime);
        return FormatTimeSpan(sessionTime);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOTAL PLAYTIME FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the total cumulative playtime for display.
    /// </summary>
    /// <param name="totalPlaytime">The total playtime duration across all sessions.</param>
    /// <returns>A human-readable total playtime string (e.g., "47h 32m").</returns>
    /// <example>
    /// <code>
    /// var renderer = new PlaytimeRenderer();
    /// var display = renderer.FormatTotalPlaytime(TimeSpan.FromHours(47.5));
    /// // Returns "47h 30m"
    /// </code>
    /// </example>
    public string FormatTotalPlaytime(TimeSpan totalPlaytime)
    {
        _logger?.LogDebug("Formatting total playtime: {TotalPlaytime}", totalPlaytime);
        return FormatTimeSpan(totalPlaytime);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PLAYTIME MILESTONE COLORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the color for a playtime value based on milestone achievements.
    /// </summary>
    /// <param name="playtime">The playtime duration to evaluate.</param>
    /// <returns>The appropriate console color for the playtime milestone.</returns>
    /// <remarks>
    /// <para>Milestone color tiers:</para>
    /// <list type="bullet">
    ///   <item><description>0-9 hours: White (default)</description></item>
    ///   <item><description>10-49 hours: Green (milestone)</description></item>
    ///   <item><description>50-99 hours: Yellow (dedicated)</description></item>
    ///   <item><description>100+ hours: Cyan (veteran)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var color = renderer.GetPlaytimeColor(TimeSpan.FromHours(75));
    /// // Returns ConsoleColor.Yellow (dedicated tier)
    /// </code>
    /// </example>
    public ConsoleColor GetPlaytimeColor(TimeSpan playtime)
    {
        var hours = playtime.TotalHours;

        var color = hours switch
        {
            >= 100 => ConsoleColor.Cyan,      // Veteran (100+ hours)
            >= 50 => ConsoleColor.Yellow,     // Dedicated (50+ hours)
            >= 10 => ConsoleColor.Green,      // Milestone (10+ hours)
            _ => ConsoleColor.White           // Default
        };

        _logger?.LogDebug(
            "Playtime {Hours:F1}h mapped to color {Color}",
            hours, color);

        return color;
    }

    /// <summary>
    /// Renders the session time at a specific position with milestone coloring.
    /// </summary>
    /// <param name="sessionTime">The session duration to render.</param>
    /// <param name="terminalService">The terminal service for output.</param>
    /// <param name="x">The X coordinate for rendering.</param>
    /// <param name="y">The Y coordinate for rendering.</param>
    public void RenderSessionTimeAt(
        TimeSpan sessionTime,
        Application.Interfaces.ITerminalService terminalService,
        int x,
        int y)
    {
        ArgumentNullException.ThrowIfNull(terminalService);

        var formatted = FormatSessionTime(sessionTime);
        terminalService.WriteAt(x, y, formatted);

        _logger?.LogDebug(
            "Rendered session time at ({X}, {Y}): {Formatted}",
            x, y, formatted);
    }

    /// <summary>
    /// Renders the total playtime at a specific position with milestone coloring.
    /// </summary>
    /// <param name="totalPlaytime">The total playtime duration to render.</param>
    /// <param name="terminalService">The terminal service for output.</param>
    /// <param name="x">The X coordinate for rendering.</param>
    /// <param name="y">The Y coordinate for rendering.</param>
    public void RenderTotalPlaytimeAt(
        TimeSpan totalPlaytime,
        Application.Interfaces.ITerminalService terminalService,
        int x,
        int y)
    {
        ArgumentNullException.ThrowIfNull(terminalService);

        var formatted = FormatTotalPlaytime(totalPlaytime);
        var color = GetPlaytimeColor(totalPlaytime);
        terminalService.WriteColoredAt(x, y, formatted, color);

        _logger?.LogDebug(
            "Rendered total playtime at ({X}, {Y}): {Formatted} with color {Color}",
            x, y, formatted, color);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a TimeSpan into a human-readable string.
    /// </summary>
    /// <param name="time">The time span to format.</param>
    /// <returns>The formatted time string.</returns>
    private static string FormatTimeSpan(TimeSpan time)
    {
        var totalHours = (int)time.TotalHours;
        var minutes = time.Minutes;

        // Less than 1 minute
        if (totalHours == 0 && minutes == 0)
        {
            return "<1m";
        }

        // Less than 1 hour: just show minutes
        if (totalHours == 0)
        {
            return $"{minutes}m";
        }

        // 1 hour or more: show hours and minutes
        return $"{totalHours}h {minutes:D2}m";
    }
}
