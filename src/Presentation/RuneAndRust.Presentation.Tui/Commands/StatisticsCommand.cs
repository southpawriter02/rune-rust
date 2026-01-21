// ═══════════════════════════════════════════════════════════════════════════════
// StatisticsCommand.cs
// Command handler for displaying the statistics dashboard.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.Commands;

/// <summary>
/// Command handler for displaying the statistics dashboard.
/// </summary>
/// <remarks>
/// <para>
/// Handles the "statistics" and "stats" commands to display the player's
/// statistics dashboard with session and all-time comparisons, organized
/// by category.
/// </para>
/// <para>
/// Uses the <see cref="IStatisticsService"/> to retrieve player statistics
/// and displays them using the <see cref="StatisticsDashboard"/> component.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var command = new StatisticsCommand(statisticsService, terminalService, logger);
/// command.Execute(player);
/// </code>
/// </example>
public class StatisticsCommand
{
    private readonly IStatisticsService _statisticsService;
    private readonly ITerminalService _terminalService;
    private readonly StatisticsDashboard _dashboard;
    private readonly StatisticsDashboardConfig _config;
    private readonly ILogger<StatisticsCommand>? _logger;

    /// <summary>
    /// Gets the command name.
    /// </summary>
    public string Name => "statistics";

    /// <summary>
    /// Gets the command aliases.
    /// </summary>
    public IReadOnlyList<string> Aliases => new[] { "stats" };

    /// <summary>
    /// Gets the command description.
    /// </summary>
    public string Description => "View player statistics organized by category";

    /// <summary>
    /// Gets the command usage help text.
    /// </summary>
    public string Usage => "statistics [category]";

    /// <summary>
    /// Creates a new instance of the StatisticsCommand.
    /// </summary>
    /// <param name="statisticsService">The statistics service for retrieving data.</param>
    /// <param name="terminalService">The terminal service for output.</param>
    /// <param name="config">Optional configuration for dashboard display.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public StatisticsCommand(
        IStatisticsService statisticsService,
        ITerminalService terminalService,
        StatisticsDashboardConfig? config = null,
        ILogger<StatisticsCommand>? logger = null)
    {
        _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new StatisticsDashboardConfig();
        _logger = logger;

        // Create dashboard components
        var playtimeRenderer = new PlaytimeRenderer(
            logger != null ? LoggerFactory.Create(b => b.AddConsole())
                .CreateLogger<PlaytimeRenderer>() : null);

        var categoryTabs = new CategoryTabs(
            _terminalService,
            _config,
            logger != null ? LoggerFactory.Create(b => b.AddConsole())
                .CreateLogger<CategoryTabs>() : null);

        var comparisonView = new StatComparisonView(
            _terminalService,
            _config,
            logger != null ? LoggerFactory.Create(b => b.AddConsole())
                .CreateLogger<StatComparisonView>() : null);

        var chart = new SimpleChart(
            _terminalService,
            _config,
            logger != null ? LoggerFactory.Create(b => b.AddConsole())
                .CreateLogger<SimpleChart>() : null);

        _dashboard = new StatisticsDashboard(
            categoryTabs,
            comparisonView,
            chart,
            playtimeRenderer,
            _terminalService,
            _config,
            logger != null ? LoggerFactory.Create(b => b.AddConsole())
                .CreateLogger<StatisticsDashboard>() : null);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXECUTION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Executes the statistics command for the given player.
    /// </summary>
    /// <param name="player">The player whose statistics to display.</param>
    /// <param name="args">Optional command arguments (category filter).</param>
    /// <returns>True if the command executed successfully, false otherwise.</returns>
    public bool Execute(Player player, string[]? args = null)
    {
        ArgumentNullException.ThrowIfNull(player);

        _logger?.LogInformation(
            "Executing statistics command for player {PlayerName}",
            player.Name);

        try
        {
            // Parse optional category argument
            var initialCategory = ParseCategoryArgument(args);

            // Gather statistics from service
            var statistics = GatherStatistics(player);

            // Calculate session time (placeholder - would come from session tracking)
            var sessionTime = TimeSpan.FromMinutes(45); // Placeholder

            // Get total playtime from statistics
            var totalPlaytime = GetTotalPlaytime(player);

            // Set dashboard position
            _dashboard.SetPosition(2, 2);

            // Apply category filter if specified
            if (initialCategory.HasValue)
            {
                _dashboard.FilterByCategory(initialCategory.Value);
            }

            // Render the dashboard
            _dashboard.RenderStatistics(statistics, sessionTime, totalPlaytime);

            _logger?.LogInformation(
                "Statistics dashboard rendered successfully for player {PlayerName}",
                player.Name);

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to execute statistics command");
            _terminalService.WriteLine("Error: Failed to display statistics.");
            return false;
        }
    }

    /// <summary>
    /// Handles interactive key input for the statistics dashboard.
    /// </summary>
    /// <param name="key">The key pressed by the user.</param>
    /// <returns>True if the key was handled, false otherwise.</returns>
    public bool HandleInput(ConsoleKey key)
    {
        return _dashboard.HandleCategoryInput(key);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Parses the category argument from command args.
    /// </summary>
    private static StatisticCategory? ParseCategoryArgument(string[]? args)
    {
        if (args == null || args.Length == 0)
        {
            return null;
        }

        var categoryArg = args[0].ToLowerInvariant();
        return categoryArg switch
        {
            "combat" or "c" => StatisticCategory.Combat,
            "exploration" or "explore" or "e" => StatisticCategory.Exploration,
            "progression" or "progress" or "p" => StatisticCategory.Progression,
            "time" or "t" => StatisticCategory.Time,
            "dice" or "d" => StatisticCategory.Dice,
            _ => null
        };
    }

    /// <summary>
    /// Gathers all statistics from the service and converts to display DTOs.
    /// </summary>
    private List<StatisticDisplayDto> GatherStatistics(Player player)
    {
        var displayStats = new List<StatisticDisplayDto>();

        // Gather statistics for all categories
        foreach (StatisticCategory category in Enum.GetValues<StatisticCategory>())
        {
            var categoryStats = _statisticsService.GetCategoryStatistics(player, category);

            foreach (var (name, value) in categoryStats)
            {
                displayStats.Add(CreateDisplayDto(name, value, category));
            }
        }

        _logger?.LogDebug("Gathered {Count} statistics for display", displayStats.Count);
        return displayStats;
    }

    /// <summary>
    /// Creates a display DTO from raw statistic data.
    /// </summary>
    private static StatisticDisplayDto CreateDisplayDto(
        string name,
        long value,
        StatisticCategory category)
    {
        // Determine if this is a percentage stat based on name
        var isPercentage = name.Contains("Rate") || name.Contains("Ratio");

        // For display purposes, session value = 0 (need session tracking) 
        // and all-time value is the stored value
        return new StatisticDisplayDto
        {
            Name = FormatStatName(name),
            SessionValue = 0, // Placeholder for session tracking
            AllTimeValue = (int)Math.Min(value, int.MaxValue),
            IsPercentage = isPercentage,
            Category = category
        };
    }

    /// <summary>
    /// Formats a statistic name for display.
    /// </summary>
    private static string FormatStatName(string name)
    {
        // Convert camelCase to Title Case with spaces
        var result = string.Concat(
            name.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));
        return char.ToUpper(result[0]) + result.Substring(1);
    }

    /// <summary>
    /// Gets the total playtime for the player.
    /// </summary>
    private TimeSpan GetTotalPlaytime(Player player)
    {
        // Get playtime from statistics
        var playtimeSeconds = _statisticsService.GetStatistic(player, "totalPlaytimeSeconds");
        return TimeSpan.FromSeconds(playtimeSeconds);
    }
}
