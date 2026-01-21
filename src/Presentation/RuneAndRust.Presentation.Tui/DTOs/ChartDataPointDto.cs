// ═══════════════════════════════════════════════════════════════════════════════
// ChartDataPointDto.cs
// Data transfer object for chart data points used in bar charts.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for a chart data point used in ASCII bar charts.
/// </summary>
/// <remarks>
/// <para>
/// This DTO represents a single data point in a bar chart, containing
/// the label, numeric value, percentage of total, and display color.
/// </para>
/// <para>
/// Used by <see cref="UI.SimpleChart"/> to render distribution charts
/// such as damage type breakdowns.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var dataPoint = new ChartDataPointDto
/// {
///     Label = "Physical",
///     Value = 58000,
///     Percentage = 58.0f,
///     Color = ConsoleColor.White
/// };
/// </code>
/// </example>
public class ChartDataPointDto
{
    /// <summary>
    /// Gets or sets the label for this data point.
    /// </summary>
    /// <remarks>
    /// The label is displayed to the left of the bar in the chart.
    /// Should be kept reasonably short (typically 10 characters or less).
    /// </remarks>
    /// <example>"Physical", "Fire", "Ice", "Lightning"</example>
    public required string Label { get; set; }

    /// <summary>
    /// Gets or sets the numeric value for this data point.
    /// </summary>
    /// <remarks>
    /// This value is used to calculate the bar length relative to other
    /// data points in the chart.
    /// </remarks>
    public int Value { get; set; }

    /// <summary>
    /// Gets or sets the percentage this value represents of the total.
    /// </summary>
    /// <remarks>
    /// Expressed as a float from 0.0 to 100.0 (e.g., 58.0 for 58%).
    /// Displayed after the bar in the chart.
    /// </remarks>
    public float Percentage { get; set; }

    /// <summary>
    /// Gets or sets the display color for this data point.
    /// </summary>
    /// <remarks>
    /// Used to colorize the bar or label in the chart for visual
    /// differentiation between categories.
    /// </remarks>
    public ConsoleColor Color { get; set; }
}
