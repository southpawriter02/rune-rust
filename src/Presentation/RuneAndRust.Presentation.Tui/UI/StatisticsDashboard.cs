// ═══════════════════════════════════════════════════════════════════════════════
// StatisticsDashboard.cs
// Main UI component for displaying the player statistics dashboard.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays the player's statistics organized by category with session and all-time comparison.
/// </summary>
/// <remarks>
/// <para>
/// The statistics dashboard provides a comprehensive view of player statistics
/// organized into categories (Combat, Exploration, Progression, Time, Dice).
/// Each category can be selected via keyboard shortcuts (1-5).
/// </para>
/// <para>Dashboard sections:</para>
/// <list type="bullet">
///   <item><description>Header with title and playtime summary</description></item>
///   <item><description>Category tabs for filtering</description></item>
///   <item><description>Statistics comparison table (Session vs All-Time vs Change)</description></item>
///   <item><description>Distribution chart for certain categories (Combat damage types)</description></item>
/// </list>
/// </remarks>
public class StatisticsDashboard
{
    private readonly CategoryTabs _categoryTabs;
    private readonly StatComparisonView _comparisonView;
    private readonly SimpleChart _chart;
    private readonly PlaytimeRenderer _playtimeRenderer;
    private readonly ITerminalService _terminalService;
    private readonly StatisticsDashboardConfig _config;
    private readonly ILogger<StatisticsDashboard>? _logger;

    private StatisticCategory _activeCategory;
    private IReadOnlyList<StatisticDisplayDto> _statistics = Array.Empty<StatisticDisplayDto>();
    private TimeSpan _sessionTime;
    private TimeSpan _totalPlaytime;
    private (int X, int Y) _panelPosition;

    /// <summary>
    /// Gets the currently active category filter.
    /// </summary>
    public StatisticCategory ActiveCategory => _activeCategory;

    /// <summary>
    /// Creates a new instance of the StatisticsDashboard component.
    /// </summary>
    /// <param name="categoryTabs">The category tabs component.</param>
    /// <param name="comparisonView">The comparison view component.</param>
    /// <param name="chart">The chart component for distribution display.</param>
    /// <param name="playtimeRenderer">The playtime formatting renderer.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Optional configuration for dashboard display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public StatisticsDashboard(
        CategoryTabs categoryTabs,
        StatComparisonView comparisonView,
        SimpleChart chart,
        PlaytimeRenderer playtimeRenderer,
        ITerminalService terminalService,
        StatisticsDashboardConfig? config = null,
        ILogger<StatisticsDashboard>? logger = null)
    {
        _categoryTabs = categoryTabs ?? throw new ArgumentNullException(nameof(categoryTabs));
        _comparisonView = comparisonView ?? throw new ArgumentNullException(nameof(comparisonView));
        _chart = chart ?? throw new ArgumentNullException(nameof(chart));
        _playtimeRenderer = playtimeRenderer ?? throw new ArgumentNullException(nameof(playtimeRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new StatisticsDashboardConfig();
        _logger = logger;

        _activeCategory = StatisticCategory.Combat;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // POSITION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the dashboard panel position.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void SetPosition(int x, int y)
    {
        _panelPosition = (x, y);
        _logger?.LogDebug("Dashboard position set to ({X}, {Y})", x, y);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders all statistics in the dashboard.
    /// </summary>
    /// <param name="statistics">The statistic display data.</param>
    /// <param name="sessionTime">The current session duration.</param>
    /// <param name="totalPlaytime">The total playtime across all sessions.</param>
    public void RenderStatistics(
        IEnumerable<StatisticDisplayDto> statistics,
        TimeSpan sessionTime,
        TimeSpan totalPlaytime)
    {
        _statistics = statistics.ToList();
        _sessionTime = sessionTime;
        _totalPlaytime = totalPlaytime;

        _logger?.LogDebug(
            "Rendering statistics dashboard: {StatCount} statistics, Session: {Session}, Total: {Total}",
            _statistics.Count, sessionTime, totalPlaytime);

        // Clear panel area
        ClearPanel();

        // Render panel header
        RenderHeader();

        // Render playtime summary line
        RenderPlaytimeSummary();

        // Render category tabs
        _categoryTabs.RenderTabs(_panelPosition.X, _panelPosition.Y + 5, _activeCategory);

        // Filter by active category
        var filteredStatistics = FilterStatistics(_statistics);

        // Render comparison view
        var lastY = _comparisonView.RenderComparison(
            filteredStatistics,
            _panelPosition.X,
            _panelPosition.Y + 8);

        // Render chart if applicable for category
        RenderCategoryChart(filteredStatistics, lastY + 2);

        _logger?.LogInformation(
            "Statistics dashboard rendered with {FilteredCount} statistics for {Category}",
            filteredStatistics.Count, _activeCategory);
    }

    /// <summary>
    /// Filters statistics by the specified category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    public void FilterByCategory(StatisticCategory category)
    {
        _activeCategory = category;
        _logger?.LogDebug("Statistics filter changed to: {Category}", category);

        // Re-render with new category
        RenderStatistics(_statistics, _sessionTime, _totalPlaytime);
    }

    /// <summary>
    /// Handles category input from user key press.
    /// </summary>
    /// <param name="key">The pressed key (1-5).</param>
    /// <returns>True if the key was handled, false otherwise.</returns>
    public bool HandleCategoryInput(int key)
    {
        var category = _categoryTabs.GetCategoryForKey(key);
        if (category.HasValue)
        {
            FilterByCategory(category.Value);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles category input from console key.
    /// </summary>
    /// <param name="key">The console key pressed.</param>
    /// <returns>True if the key was handled, false otherwise.</returns>
    public bool HandleCategoryInput(ConsoleKey key)
    {
        var keyNumber = key switch
        {
            ConsoleKey.D1 or ConsoleKey.NumPad1 => 1,
            ConsoleKey.D2 or ConsoleKey.NumPad2 => 2,
            ConsoleKey.D3 or ConsoleKey.NumPad3 => 3,
            ConsoleKey.D4 or ConsoleKey.NumPad4 => 4,
            ConsoleKey.D5 or ConsoleKey.NumPad5 => 5,
            _ => 0
        };

        if (keyNumber > 0)
        {
            return HandleCategoryInput(keyNumber);
        }

        return false;
    }

    /// <summary>
    /// Shows session statistics in the dashboard.
    /// </summary>
    /// <param name="sessionStats">The session statistics to display.</param>
    public void ShowSessionStats(IEnumerable<StatisticDisplayDto> sessionStats)
    {
        var filteredStats = sessionStats
            .Where(s => s.Category == _activeCategory)
            .ToList();

        _comparisonView.RenderSessionColumn(
            filteredStats,
            _panelPosition.X + _config.NameColumnWidth + 2,
            _panelPosition.Y + 8);
    }

    /// <summary>
    /// Shows all-time statistics in the dashboard.
    /// </summary>
    /// <param name="allTimeStats">The all-time statistics to display.</param>
    public void ShowAllTimeStats(IEnumerable<StatisticDisplayDto> allTimeStats)
    {
        var filteredStats = allTimeStats
            .Where(s => s.Category == _activeCategory)
            .ToList();

        _comparisonView.RenderAllTimeColumn(
            filteredStats,
            _panelPosition.X + _config.NameColumnWidth + _config.ValueColumnWidth + 4,
            _panelPosition.Y + 8);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Clears the panel area.
    /// </summary>
    private void ClearPanel()
    {
        for (int y = 0; y < _config.PanelHeight; y++)
        {
            _terminalService.WriteAt(
                _panelPosition.X,
                _panelPosition.Y + y,
                new string(' ', _config.PanelWidth));
        }
    }

    /// <summary>
    /// Renders the panel header.
    /// </summary>
    private void RenderHeader()
    {
        var headerLine = new string('=', _config.PanelWidth - 2);
        var title = "STATISTICS";
        var titlePadding = (_config.PanelWidth - title.Length - 2) / 2;

        _terminalService.WriteAt(_panelPosition.X, _panelPosition.Y, $"+{headerLine}+");
        _terminalService.WriteAt(
            _panelPosition.X,
            _panelPosition.Y + 1,
            $"|{new string(' ', titlePadding)}{title}{new string(' ', _config.PanelWidth - title.Length - titlePadding - 2)}|");
        _terminalService.WriteAt(_panelPosition.X, _panelPosition.Y + 2, $"+{headerLine}+");
    }

    /// <summary>
    /// Renders the playtime summary line.
    /// </summary>
    private void RenderPlaytimeSummary()
    {
        var sessionFormatted = _playtimeRenderer.FormatSessionTime(_sessionTime);
        var totalFormatted = _playtimeRenderer.FormatTotalPlaytime(_totalPlaytime);
        var summaryText = $"  Session: {sessionFormatted} | Total Playtime: {totalFormatted}";
        var paddedSummary = summaryText.PadRight(_config.PanelWidth - 2);

        _terminalService.WriteAt(
            _panelPosition.X,
            _panelPosition.Y + 3,
            $"|{paddedSummary}|");
        _terminalService.WriteAt(
            _panelPosition.X,
            _panelPosition.Y + 4,
            $"+{new string('-', _config.PanelWidth - 2)}+");
    }

    /// <summary>
    /// Filters statistics by the active category.
    /// </summary>
    private IReadOnlyList<StatisticDisplayDto> FilterStatistics(
        IReadOnlyList<StatisticDisplayDto> statistics)
    {
        return statistics
            .Where(s => s.Category == _activeCategory)
            .ToList();
    }

    /// <summary>
    /// Renders a category-specific chart if applicable.
    /// </summary>
    private void RenderCategoryChart(IReadOnlyList<StatisticDisplayDto> statistics, int chartY)
    {
        // Only render chart for Combat category (damage type distribution)
        if (_activeCategory != StatisticCategory.Combat)
        {
            return;
        }

        var chartData = GetDamageTypeDistribution(statistics);
        if (chartData.Count > 0)
        {
            _chart.RenderDistribution(
                chartData,
                _panelPosition.X + 2,
                chartY,
                "DAMAGE TYPE DISTRIBUTION");
        }
    }

    /// <summary>
    /// Extracts damage type distribution data for chart display.
    /// </summary>
    private IReadOnlyList<ChartDataPointDto> GetDamageTypeDistribution(
        IReadOnlyList<StatisticDisplayDto> statistics)
    {
        // Extract damage type statistics for chart display
        var damageStats = statistics
            .Where(s => s.Name.Contains("Damage") && s.Name.Contains("Dealt"))
            .ToList();

        if (damageStats.Count == 0)
        {
            return Array.Empty<ChartDataPointDto>();
        }

        var total = damageStats.Sum(s => s.AllTimeValue);
        if (total == 0)
        {
            return Array.Empty<ChartDataPointDto>();
        }

        return damageStats.Select(s => new ChartDataPointDto
        {
            Label = s.Name.Replace(" Damage Dealt", ""),
            Value = s.AllTimeValue,
            Percentage = (float)s.AllTimeValue / total * 100f,
            Color = GetDamageTypeColor(s.Name)
        }).ToList();
    }

    /// <summary>
    /// Gets the color for a damage type.
    /// </summary>
    private static ConsoleColor GetDamageTypeColor(string damageType)
    {
        return damageType.ToLowerInvariant() switch
        {
            string s when s.Contains("physical") => ConsoleColor.White,
            string s when s.Contains("fire") => ConsoleColor.Red,
            string s when s.Contains("ice") => ConsoleColor.Cyan,
            string s when s.Contains("lightning") => ConsoleColor.Yellow,
            string s when s.Contains("poison") => ConsoleColor.Green,
            string s when s.Contains("shadow") => ConsoleColor.DarkMagenta,
            _ => ConsoleColor.Gray
        };
    }
}
