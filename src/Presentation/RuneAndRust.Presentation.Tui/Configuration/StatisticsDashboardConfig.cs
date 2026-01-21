// ═══════════════════════════════════════════════════════════════════════════════
// StatisticsDashboardConfig.cs
// Configuration settings for the statistics dashboard display.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration settings for the statistics dashboard display.
/// </summary>
/// <remarks>
/// <para>
/// This configuration controls the layout and appearance of the statistics
/// dashboard panel, including column widths, chart dimensions, and color
/// settings for change indicators.
/// </para>
/// <para>
/// Configuration values can be loaded from config/statistics-dashboard.json
/// or use the defaults specified in this class.
/// </para>
/// </remarks>
public class StatisticsDashboardConfig
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PANEL DIMENSIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the total width of the dashboard panel in characters.
    /// </summary>
    public int PanelWidth { get; set; } = 72;

    /// <summary>
    /// Gets or sets the total height of the dashboard panel in characters.
    /// </summary>
    public int PanelHeight { get; set; } = 40;

    // ═══════════════════════════════════════════════════════════════════════════
    // COLUMN WIDTHS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the width of the statistic name column.
    /// </summary>
    public int NameColumnWidth { get; set; } = 24;

    /// <summary>
    /// Gets or sets the width of the value columns (Session, All-Time, Change).
    /// </summary>
    public int ValueColumnWidth { get; set; } = 12;

    // ═══════════════════════════════════════════════════════════════════════════
    // CHART SETTINGS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the width of the chart bars in characters.
    /// </summary>
    public int ChartWidth { get; set; } = 30;

    /// <summary>
    /// Gets or sets the width of chart labels.
    /// </summary>
    public int ChartLabelWidth { get; set; } = 10;

    /// <summary>
    /// Gets or sets the character used for filled chart bar sections.
    /// </summary>
    public char ChartFilledChar { get; set; } = '#';

    /// <summary>
    /// Gets or sets the character used for empty chart bar sections.
    /// </summary>
    public char ChartEmptyChar { get; set; } = '.';

    // ═══════════════════════════════════════════════════════════════════════════
    // CHANGE INDICATOR COLORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the color used for positive change values.
    /// </summary>
    public ConsoleColor PositiveChangeColor { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// Gets or sets the color used for negative change values.
    /// </summary>
    public ConsoleColor NegativeChangeColor { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// Gets or sets the color used for neutral/zero change values.
    /// </summary>
    public ConsoleColor NeutralChangeColor { get; set; } = ConsoleColor.Gray;

    // ═══════════════════════════════════════════════════════════════════════════
    // CATEGORY TAB SETTINGS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the color used for active category tabs.
    /// </summary>
    public ConsoleColor ActiveTabColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>
    /// Gets or sets the color used for inactive category tabs.
    /// </summary>
    public ConsoleColor InactiveTabColor { get; set; } = ConsoleColor.Gray;
}
