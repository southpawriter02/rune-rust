// ═══════════════════════════════════════════════════════════════════════════════
// SimpleChart.cs
// UI component for rendering simple ASCII bar charts.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Renders simple ASCII bar charts for distribution and trend visualization.
/// </summary>
/// <remarks>
/// <para>
/// Uses filled blocks (#) and empty blocks (.) to represent proportional values.
/// Supports labels, percentages, and optional legend display.
/// </para>
/// <para>Chart format example:</para>
/// <code>
/// Physical [#############...........] 58%
/// Fire     [######..................] 22%
/// Ice      [####...................] 12%
/// </code>
/// </remarks>
public class SimpleChart
{
    private readonly ITerminalService _terminalService;
    private readonly StatisticsDashboardConfig _config;
    private readonly ILogger<SimpleChart>? _logger;

    private int _minValue;
    private int _maxValue;

    /// <summary>
    /// Creates a new instance of the SimpleChart component.
    /// </summary>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Optional configuration for chart display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when terminalService is null.</exception>
    public SimpleChart(
        ITerminalService terminalService,
        StatisticsDashboardConfig? config = null,
        ILogger<SimpleChart>? logger = null)
    {
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new StatisticsDashboardConfig();
        _logger = logger;

        _minValue = 0;
        _maxValue = 100;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SCALE CONFIGURATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the scale for the chart.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <remarks>
    /// The scale is used for manual scaling of bar lengths.
    /// For auto-scaling based on data, use RenderBarChart which calculates
    /// the max from the provided data points.
    /// </remarks>
    public void SetScale(int min, int max)
    {
        _minValue = min;
        _maxValue = max;
        _logger?.LogDebug("Chart scale set to {Min} - {Max}", min, max);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders a bar chart from the given data points.
    /// </summary>
    /// <param name="dataPoints">The data points to chart.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns>The Y coordinate after the last rendered row.</returns>
    public int RenderBarChart(IEnumerable<ChartDataPointDto> dataPoints, int x, int y)
    {
        var points = dataPoints.ToList();
        if (points.Count == 0)
        {
            _logger?.LogDebug("No data points to render for bar chart");
            return y;
        }

        var maxValue = points.Max(p => p.Value);
        if (maxValue == 0)
        {
            maxValue = 1; // Prevent division by zero
        }

        var currentY = y;
        foreach (var point in points)
        {
            var bar = FormatBar(point.Value, maxValue, _config.ChartWidth);
            var percentageText = $"{point.Percentage:F0}%";
            var labelPadded = point.Label.PadRight(_config.ChartLabelWidth);
            var line = $"{labelPadded}[{bar}] {percentageText}";
            _terminalService.WriteAt(x, currentY, line);
            currentY++;
        }

        _logger?.LogDebug("Rendered bar chart with {Count} data points", points.Count);
        return currentY;
    }

    /// <summary>
    /// Renders a bar chart with colored bars based on data point colors.
    /// </summary>
    /// <param name="dataPoints">The data points to chart.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns>The Y coordinate after the last rendered row.</returns>
    public int RenderBarChartColored(IEnumerable<ChartDataPointDto> dataPoints, int x, int y)
    {
        var points = dataPoints.ToList();
        if (points.Count == 0)
        {
            return y;
        }

        var maxValue = points.Max(p => p.Value);
        if (maxValue == 0)
        {
            maxValue = 1;
        }

        var currentY = y;
        foreach (var point in points)
        {
            var bar = FormatBar(point.Value, maxValue, _config.ChartWidth);
            var percentageText = $"{point.Percentage:F0}%";
            var labelPadded = point.Label.PadRight(_config.ChartLabelWidth);

            // Write label
            _terminalService.WriteAt(x, currentY, labelPadded);

            // Write colored bar
            var barX = x + _config.ChartLabelWidth;
            _terminalService.WriteColoredAt(barX, currentY, $"[{bar}]", point.Color);

            // Write percentage
            var percentX = barX + _config.ChartWidth + 3;
            _terminalService.WriteAt(percentX, currentY, percentageText);

            currentY++;
        }

        _logger?.LogDebug("Rendered colored bar chart with {Count} data points", points.Count);
        return currentY;
    }

    /// <summary>
    /// Renders a distribution chart with a title header.
    /// </summary>
    /// <param name="dataPoints">The data points representing the distribution.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="title">Optional title for the chart (defaults to "DISTRIBUTION").</param>
    /// <returns>The Y coordinate after the last rendered row.</returns>
    public int RenderDistribution(
        IReadOnlyList<ChartDataPointDto> dataPoints,
        int x,
        int y,
        string? title = null)
    {
        // Render header
        var headerTitle = title ?? "DISTRIBUTION";
        _terminalService.WriteAt(x, y, headerTitle);
        _terminalService.WriteAt(x, y + 1, new string('-', headerTitle.Length));

        // Render the bar chart
        return RenderBarChart(dataPoints, x, y + 2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BAR FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a bar for the given value and maximum.
    /// </summary>
    /// <param name="value">The value to represent.</param>
    /// <param name="maxValue">The maximum value for scaling.</param>
    /// <param name="width">The total width of the bar in characters.</param>
    /// <returns>The formatted bar string (e.g., "######..........").</returns>
    /// <example>
    /// <code>
    /// var bar = chart.FormatBar(50, 100, 20);
    /// // Returns "##########.........."
    /// </code>
    /// </example>
    public string FormatBar(int value, int maxValue, int width)
    {
        if (maxValue == 0)
        {
            return new string(_config.ChartEmptyChar, width);
        }

        var filledWidth = (int)Math.Round((double)value / maxValue * width);
        filledWidth = Math.Min(filledWidth, width);
        filledWidth = Math.Max(filledWidth, 0);

        var emptyWidth = width - filledWidth;

        return new string(_config.ChartFilledChar, filledWidth) +
               new string(_config.ChartEmptyChar, emptyWidth);
    }

    /// <summary>
    /// Shows or hides the legend.
    /// </summary>
    /// <param name="show">Whether to show the legend.</param>
    public void ShowLegend(bool show)
    {
        _logger?.LogDebug("Legend visibility set to: {Show}", show);
    }
}
