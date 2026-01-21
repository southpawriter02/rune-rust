// ═══════════════════════════════════════════════════════════════════════════════
// StatComparisonView.cs
// UI component for displaying statistics in a three-column comparison format.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays statistics in a three-column comparison format with session, all-time, and delta values.
/// </summary>
/// <remarks>
/// <para>
/// Formats statistics with proper column alignment and colors positive/negative
/// changes appropriately. Supports both numeric and percentage-based statistics.
/// </para>
/// <para>Column layout:</para>
/// <code>
/// Statistic              Session      All-Time      Change
/// ================================================================
/// Monsters Defeated          47          1,284       +47
/// Combat Win Rate          94.2%          91.7%    +2.5%
/// </code>
/// </remarks>
public class StatComparisonView
{
    private readonly ITerminalService _terminalService;
    private readonly StatisticsDashboardConfig _config;
    private readonly ILogger<StatComparisonView>? _logger;

    /// <summary>
    /// Creates a new instance of the StatComparisonView component.
    /// </summary>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Optional configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when terminalService is null.</exception>
    public StatComparisonView(
        ITerminalService terminalService,
        StatisticsDashboardConfig? config = null,
        ILogger<StatComparisonView>? logger = null)
    {
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new StatisticsDashboardConfig();
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the comparison view for the given statistics.
    /// </summary>
    /// <param name="statistics">The statistics to display.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns>The Y coordinate after the last rendered row.</returns>
    public int RenderComparison(
        IReadOnlyList<StatisticDisplayDto> statistics,
        int x,
        int y)
    {
        _logger?.LogDebug(
            "Rendering comparison view at ({X}, {Y}) with {Count} statistics",
            x, y, statistics.Count);

        // Render category header if we have statistics
        if (statistics.Count > 0)
        {
            var categoryName = statistics[0].Category.ToString().ToUpperInvariant();
            var headerText = $"{categoryName} STATISTICS";
            _terminalService.WriteAt(x + 2, y, headerText);
            _terminalService.WriteAt(x + 2, y + 1, new string('-', headerText.Length));
        }

        // Render column headers
        var headerY = y + 3;
        RenderColumnHeaders(x + 2, headerY);
        _terminalService.WriteAt(x + 2, headerY + 1, new string('=', _config.PanelWidth - 6));

        // Render each statistic row
        var currentY = headerY + 2;
        foreach (var stat in statistics)
        {
            RenderStatisticRow(stat, x + 2, currentY);
            currentY++;
        }

        _logger?.LogDebug("Rendered {Count} statistics in comparison view", statistics.Count);
        return currentY;
    }

    /// <summary>
    /// Renders just the session column for the statistics.
    /// </summary>
    /// <param name="statistics">The statistics to display.</param>
    /// <param name="x">The X coordinate for the column.</param>
    /// <param name="y">The Y coordinate.</param>
    public void RenderSessionColumn(IReadOnlyList<StatisticDisplayDto> statistics, int x, int y)
    {
        var currentY = y;
        foreach (var stat in statistics)
        {
            var sessionValue = FormatValue(stat.SessionValue, stat.IsPercentage);
            _terminalService.WriteAt(x, currentY, sessionValue.PadLeft(_config.ValueColumnWidth));
            currentY++;
        }
    }

    /// <summary>
    /// Renders just the all-time column for the statistics.
    /// </summary>
    /// <param name="statistics">The statistics to display.</param>
    /// <param name="x">The X coordinate for the column.</param>
    /// <param name="y">The Y coordinate.</param>
    public void RenderAllTimeColumn(IReadOnlyList<StatisticDisplayDto> statistics, int x, int y)
    {
        var currentY = y;
        foreach (var stat in statistics)
        {
            var allTimeValue = FormatValue(stat.AllTimeValue, stat.IsPercentage);
            _terminalService.WriteAt(x, currentY, allTimeValue.PadLeft(_config.ValueColumnWidth));
            currentY++;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DELTA FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the delta value with appropriate prefix.
    /// </summary>
    /// <param name="sessionValue">The session value (which is the delta for this session).</param>
    /// <param name="allTimeValue">The all-time value (not used in current implementation).</param>
    /// <returns>The formatted delta string with +/- prefix.</returns>
    /// <remarks>
    /// Delta is the session value since it represents what was gained this session.
    /// Zero values are displayed as "-" to indicate no change.
    /// </remarks>
    public string FormatDelta(int sessionValue, int allTimeValue)
    {
        var delta = sessionValue;

        if (delta == 0)
        {
            return "-";
        }

        var prefix = delta > 0 ? "+" : "";
        return $"{prefix}{delta:N0}";
    }

    /// <summary>
    /// Formats the delta for percentage values.
    /// </summary>
    /// <param name="sessionValue">The session percentage value (as int, e.g., 942 for 94.2%).</param>
    /// <param name="allTimeValue">The all-time percentage value.</param>
    /// <returns>The formatted percentage delta string.</returns>
    /// <remarks>
    /// For percentages, the delta represents the difference between session and all-time rates.
    /// Very small differences (less than 0.1%) are displayed as "-".
    /// </remarks>
    public string FormatPercentageDelta(int sessionValue, int allTimeValue)
    {
        var delta = (sessionValue - allTimeValue) / 10.0;

        if (Math.Abs(delta) < 0.1)
        {
            return "-";
        }

        var prefix = delta > 0 ? "+" : "";
        return $"{prefix}{delta:F1}%";
    }

    /// <summary>
    /// Gets the console color for a delta value.
    /// </summary>
    /// <param name="delta">The delta value to evaluate.</param>
    /// <returns>Green for positive, Red for negative, Gray for zero.</returns>
    public ConsoleColor GetDeltaColor(int delta)
    {
        return delta switch
        {
            > 0 => _config.PositiveChangeColor,
            < 0 => _config.NegativeChangeColor,
            _ => _config.NeutralChangeColor
        };
    }

    /// <summary>
    /// Shows percentage change with formatting.
    /// </summary>
    /// <param name="percentageChange">The percentage change value.</param>
    public void ShowPercentageChange(float percentageChange)
    {
        var prefix = percentageChange > 0 ? "+" : "";
        var formatted = $"{prefix}{percentageChange:F1}%";
        _logger?.LogDebug("Percentage change: {Change}", formatted);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the column header row.
    /// </summary>
    private void RenderColumnHeaders(int x, int y)
    {
        var nameHeader = "Statistic".PadRight(_config.NameColumnWidth);
        var sessionHeader = "Session".PadLeft(_config.ValueColumnWidth);
        var allTimeHeader = "All-Time".PadLeft(_config.ValueColumnWidth);
        var changeHeader = "Change".PadLeft(_config.ValueColumnWidth);

        var headerLine = $"{nameHeader}{sessionHeader}{allTimeHeader}{changeHeader}";
        _terminalService.WriteAt(x, y, headerLine);
    }

    /// <summary>
    /// Renders a single statistic row.
    /// </summary>
    private void RenderStatisticRow(StatisticDisplayDto stat, int x, int y)
    {
        // Truncate long names with ellipsis
        var name = stat.Name.Length > _config.NameColumnWidth - 2
            ? stat.Name.Substring(0, _config.NameColumnWidth - 5) + "..."
            : stat.Name;

        var nameColumn = name.PadRight(_config.NameColumnWidth);
        var sessionColumn = FormatValue(stat.SessionValue, stat.IsPercentage).PadLeft(_config.ValueColumnWidth);
        var allTimeColumn = FormatValue(stat.AllTimeValue, stat.IsPercentage).PadLeft(_config.ValueColumnWidth);
        var changeColumn = stat.IsPercentage
            ? FormatPercentageDelta(stat.SessionValue, stat.AllTimeValue).PadLeft(_config.ValueColumnWidth)
            : FormatDelta(stat.SessionValue, stat.AllTimeValue).PadLeft(_config.ValueColumnWidth);

        var rowLine = $"{nameColumn}{sessionColumn}{allTimeColumn}{changeColumn}";
        _terminalService.WriteAt(x, y, rowLine);
    }

    /// <summary>
    /// Formats a value for display.
    /// </summary>
    private static string FormatValue(int value, bool isPercentage)
    {
        if (isPercentage)
        {
            // Value is stored as int * 10 (e.g., 942 = 94.2%)
            return $"{value / 10.0:F1}%";
        }

        return value.ToString("N0");
    }
}
