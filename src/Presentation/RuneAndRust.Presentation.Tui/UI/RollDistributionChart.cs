// ═══════════════════════════════════════════════════════════════════════════════
// RollDistributionChart.cs
// UI component that displays a bar chart visualization of dice roll distribution.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Component that displays a bar chart visualization of dice roll
/// distribution across all possible values (1-20 for d20).
/// </summary>
/// <remarks>
/// <para>
/// Uses ASCII characters to render horizontal bars representing the
/// frequency of each roll value. Highlights values that deviate
/// significantly from the expected uniform distribution.
/// </para>
/// <para>Features:</para>
/// <list type="bullet">
///   <item><description>20 horizontal bars (one per roll value)</description></item>
///   <item><description>Scaled bar lengths based on maximum count</description></item>
///   <item><description>Outlier highlighting for above/below expected</description></item>
///   <item><description>Configurable bar characters (# for filled, . for empty)</description></item>
/// </list>
/// </remarks>
public class RollDistributionChart
{
    private readonly ITerminalService _terminal;
    private readonly DiceHistoryPanelConfig _config;
    private readonly ILogger<RollDistributionChart>? _logger;

    /// <summary>
    /// Creates a new roll distribution chart with the specified dependencies.
    /// </summary>
    /// <param name="terminal">Terminal service for output.</param>
    /// <param name="config">Panel configuration settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public RollDistributionChart(
        ITerminalService terminal,
        DiceHistoryPanelConfig config,
        ILogger<RollDistributionChart>? logger = null)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;

        _logger?.LogDebug("RollDistributionChart initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Rendering
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the distribution chart for all roll values.
    /// </summary>
    /// <param name="distribution">Roll distribution DTO with counts.</param>
    /// <param name="x">X coordinate for rendering.</param>
    /// <param name="y">Y coordinate for rendering.</param>
    /// <remarks>
    /// <para>
    /// Renders 20 lines, one for each roll value (1-20), with format:
    /// <c>1  [####................]  31 (5.9%)</c>
    /// </para>
    /// </remarks>
    public void RenderDistribution(RollDistributionDto distribution, int x, int y)
    {
        ArgumentNullException.ThrowIfNull(distribution, nameof(distribution));

        _logger?.LogDebug(
            "Rendering distribution chart with {TotalRolls} total rolls",
            distribution.TotalRolls);

        var maxCount = distribution.MaxCount;

        // Render each roll value 1-20
        for (int rollValue = 1; rollValue <= 20; rollValue++)
        {
            var count = distribution.GetCount(rollValue);
            var percentage = distribution.GetPercentage(rollValue);
            
            // Format and render the line
            RenderDistributionLine(rollValue, count, percentage, maxCount, distribution, x, y + rollValue - 1);
        }

        _logger?.LogInformation(
            "Distribution chart rendered: Max count = {MaxCount}, Total = {Total}",
            maxCount, distribution.TotalRolls);
    }

    /// <summary>
    /// Renders a single distribution line for a roll value.
    /// </summary>
    private void RenderDistributionLine(
        int rollValue,
        int count,
        float percentage,
        int maxCount,
        RollDistributionDto distribution,
        int x,
        int y)
    {
        // Roll value (2 chars, right-aligned)
        var valueStr = rollValue.ToString().PadLeft(2);

        // Bar
        var bar = FormatBar(count, maxCount);

        // Count and percentage
        var stats = $"{count,4} ({percentage:F1}%)";

        // Outlier indicator
        var outlierIndicator = GetOutlierIndicator(distribution, rollValue);

        // Build full line
        var line = $"{valueStr} {bar}  {stats}{outlierIndicator}";

        // Determine color based on outlier status
        var color = ConsoleColor.White;
        if (distribution.IsAboveExpected(rollValue))
        {
            color = _config.AboveExpectedColor;
        }
        else if (distribution.IsBelowExpected(rollValue))
        {
            color = _config.BelowExpectedColor;
        }

        // Handle critical values specially
        if (rollValue == 20)
        {
            color = _config.CriticalSuccessColor;
        }
        else if (rollValue == 1)
        {
            color = _config.CriticalFailureColor;
        }

        _terminal.WriteColoredAt(x, y, line, color);

        _logger?.LogDebug(
            "Rendered bar for roll {Value}: {Count}/{Max} ({Percentage:F1}%)",
            rollValue, count, maxCount, percentage);
    }

    /// <summary>
    /// Renders the distribution chart from a raw counts array.
    /// </summary>
    /// <param name="distribution">Array of counts for each roll value (index 0 = roll 1).</param>
    /// <param name="totalRolls">Total number of rolls for percentage calculation.</param>
    /// <param name="x">X coordinate for rendering.</param>
    /// <param name="y">Y coordinate for rendering.</param>
    public void RenderDistribution(int[] distribution, int totalRolls, int x, int y)
    {
        var dto = new RollDistributionDto(distribution, totalRolls);
        RenderDistribution(dto, x, y);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Bar Formatting
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a single bar for the distribution chart.
    /// </summary>
    /// <param name="count">Number of occurrences for this value.</param>
    /// <param name="maxCount">Maximum count across all values (for scaling).</param>
    /// <returns>Formatted bar string like "[####................]".</returns>
    /// <remarks>
    /// <para>
    /// Bar width is determined by <see cref="DiceHistoryPanelConfig.DistributionBarWidth"/>.
    /// Fill character is <see cref="DiceHistoryPanelConfig.BarFilledChar"/> ('#').
    /// Empty character is <see cref="DiceHistoryPanelConfig.BarEmptyChar"/> ('.').
    /// </para>
    /// </remarks>
    public string FormatBar(int count, int maxCount)
    {
        var barWidth = _config.DistributionBarWidth;
        var filledCount = maxCount > 0 
            ? (int)Math.Round((count / (float)maxCount) * barWidth)
            : 0;

        // Ensure at least 1 filled if count > 0
        if (count > 0 && filledCount == 0)
        {
            filledCount = 1;
        }

        var filled = new string(_config.BarFilledChar, filledCount);
        var empty = new string(_config.BarEmptyChar, barWidth - filledCount);

        return $"[{filled}{empty}]";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Outlier Detection
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the outlier indicator string for a roll value.
    /// </summary>
    /// <param name="distribution">The distribution DTO.</param>
    /// <param name="rollValue">The roll value to check.</param>
    /// <returns>Indicator string like "[arrow] Above expected!" or empty string.</returns>
    public static string GetOutlierIndicator(RollDistributionDto distribution, int rollValue)
    {
        if (distribution.IsAboveExpected(rollValue))
        {
            return " <- Above expected!";
        }
        if (distribution.IsBelowExpected(rollValue))
        {
            return " <- Below expected!";
        }
        return string.Empty;
    }

    /// <summary>
    /// Highlights values that are significantly above or below expected.
    /// </summary>
    /// <param name="expectedCount">Expected count per value (totalRolls / 20).</param>
    /// <param name="actualCount">Actual count for the value.</param>
    /// <returns>True if the value is an outlier.</returns>
    /// <remarks>
    /// <para>Outlier thresholds:</para>
    /// <list type="bullet">
    ///   <item><description>Above expected: count greater than expectedCount * 1.5</description></item>
    ///   <item><description>Below expected: count less than expectedCount * 0.5</description></item>
    /// </list>
    /// </remarks>
    public static bool IsOutlier(float expectedCount, int actualCount)
    {
        return actualCount > expectedCount * 1.5f || actualCount < expectedCount * 0.5f;
    }

    /// <summary>
    /// Shows the expected distribution line (5% for each value).
    /// </summary>
    /// <param name="x">X coordinate for rendering.</param>
    /// <param name="y">Y coordinate for rendering.</param>
    public void ShowExpectedLine(int x, int y)
    {
        _terminal.WriteAt(x, y, "Expected: 5.0% per value (uniform distribution)");
    }
}
