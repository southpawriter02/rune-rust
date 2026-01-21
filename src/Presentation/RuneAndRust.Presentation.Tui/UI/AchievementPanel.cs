// ═══════════════════════════════════════════════════════════════════════════════
// AchievementPanel.cs
// Main panel for displaying all achievements organized by category.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Main panel for displaying all achievements organized by category.
/// </summary>
/// <remarks>
/// <para>
/// The achievement panel provides a comprehensive view of all achievements:
/// </para>
/// <list type="bullet">
///   <item><description>Panel header with title and summary (unlocked count, total points)</description></item>
///   <item><description>Filter tabs for category-based filtering (1-6 keys)</description></item>
///   <item><description>Achievement cards grouped by category</description></item>
/// </list>
/// <para>
/// Filter keys:
/// </para>
/// <list type="bullet">
///   <item><description>1 - All achievements</description></item>
///   <item><description>2 - Combat</description></item>
///   <item><description>3 - Exploration</description></item>
///   <item><description>4 - Progression</description></item>
///   <item><description>5 - Collection</description></item>
///   <item><description>6 - Challenge</description></item>
/// </list>
/// </remarks>
public class AchievementPanel
{
    private readonly AchievementCategoryView _categoryView;
    private readonly AchievementCard _achievementCard;
    private readonly ITerminalService _terminalService;
    private readonly AchievementPanelConfig _config;
    private readonly ILogger<AchievementPanel> _logger;

    // Current panel state
    private int _posX;
    private int _posY;

    /// <summary>
    /// Gets the currently active category filter.
    /// </summary>
    /// <value>
    /// The active <see cref="AchievementCategory"/> filter, 
    /// or <c>null</c> to show all achievements.
    /// </value>
    public AchievementCategory? ActiveFilter { get; private set; }

    /// <summary>
    /// Creates a new instance of the AchievementPanel.
    /// </summary>
    /// <param name="categoryView">The category view renderer.</param>
    /// <param name="achievementCard">The achievement card renderer.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for panel display settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when required parameters are null.
    /// </exception>
    public AchievementPanel(
        AchievementCategoryView categoryView,
        AchievementCard achievementCard,
        ITerminalService terminalService,
        AchievementPanelConfig? config = null,
        ILogger<AchievementPanel>? logger = null)
    {
        _categoryView = categoryView ?? throw new ArgumentNullException(nameof(categoryView));
        _achievementCard = achievementCard ?? throw new ArgumentNullException(nameof(achievementCard));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new AchievementPanelConfig();
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AchievementPanel>.Instance;
    }

    /// <summary>
    /// Sets the panel position on the terminal.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void SetPosition(int x, int y)
    {
        _posX = x;
        _posY = y;
    }

    /// <summary>
    /// Renders the achievement panel with all achievements.
    /// </summary>
    /// <param name="achievements">The list of achievements to display.</param>
    /// <param name="totalPoints">The total achievement points earned.</param>
    /// <param name="unlockedCount">The number of unlocked achievements.</param>
    /// <param name="totalCount">The total number of achievements.</param>
    public void RenderAchievements(
        IEnumerable<AchievementDisplayDto> achievements,
        int totalPoints,
        int unlockedCount,
        int totalCount)
    {
        ArgumentNullException.ThrowIfNull(achievements);

        var achievementList = achievements.ToList();
        var currentY = _posY;

        // Render panel header
        currentY = RenderHeader(totalPoints, unlockedCount, totalCount, currentY);

        // Render filter tabs
        currentY = RenderFilterTabs(currentY);

        // Filter achievements if a category is selected
        var filteredAchievements = ActiveFilter.HasValue
            ? achievementList.Where(a => a.Category == ActiveFilter.Value).ToList()
            : achievementList;

        // Group by category
        var groupedByCategory = filteredAchievements
            .GroupBy(a => a.Category)
            .OrderBy(g => g.Key);

        // Render each category
        foreach (var group in groupedByCategory)
        {
            currentY = _categoryView.RenderCategory(
                group.Key,
                group.ToList(),
                _posX,
                currentY,
                _achievementCard);
            currentY++; // Add spacing between categories
        }

        _logger.LogInformation(
            "Achievement panel rendered: {UnlockedCount}/{TotalCount} unlocked, {TotalPoints} points, filter: {Filter}",
            unlockedCount, totalCount, totalPoints, ActiveFilter?.ToString() ?? "All");
    }

    /// <summary>
    /// Sets the category filter for displaying achievements.
    /// </summary>
    /// <param name="category">The category to filter by, or null for all achievements.</param>
    public void FilterByCategory(AchievementCategory? category)
    {
        ActiveFilter = category;
        _logger.LogDebug("Achievement filter changed to: {Category}", category?.ToString() ?? "All");
    }

    /// <summary>
    /// Handles filter input from keyboard keys.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <returns>
    /// <c>true</c> if the key was handled as a filter command;
    /// <c>false</c> otherwise.
    /// </returns>
    public bool HandleFilterInput(ConsoleKey key)
    {
        var category = key switch
        {
            ConsoleKey.D1 or ConsoleKey.NumPad1 => (AchievementCategory?)null,
            ConsoleKey.D2 or ConsoleKey.NumPad2 => AchievementCategory.Combat,
            ConsoleKey.D3 or ConsoleKey.NumPad3 => AchievementCategory.Exploration,
            ConsoleKey.D4 or ConsoleKey.NumPad4 => AchievementCategory.Progression,
            ConsoleKey.D5 or ConsoleKey.NumPad5 => AchievementCategory.Collection,
            ConsoleKey.D6 or ConsoleKey.NumPad6 => AchievementCategory.Challenge,
            _ => (AchievementCategory?)(-1) // Sentinel for unrecognized
        };

        // Check if key was recognized
        if (category.HasValue && (int)category.Value == -1)
        {
            return false;
        }

        FilterByCategory(category);
        return true;
    }

    /// <summary>
    /// Renders the panel header with title and summary.
    /// </summary>
    private int RenderHeader(int totalPoints, int unlockedCount, int totalCount, int y)
    {
        var panelWidth = _config.PanelWidth;
        var currentY = y;

        // Top border
        var borderLine = new string('═', panelWidth - 2);
        _terminalService.WriteAt(_posX, currentY, $"╔{borderLine}╗");
        currentY++;

        // Title line
        var title = "ACHIEVEMENTS";
        var summary = $"{unlockedCount}/{totalCount} | {totalPoints} pts";
        var padding = panelWidth - 4 - title.Length - summary.Length;
        var titleLine = $"║ {title}{new string(' ', padding)}{summary} ║";
        _terminalService.WriteAt(_posX, currentY, titleLine);
        currentY++;

        // Header bottom border
        _terminalService.WriteAt(_posX, currentY, $"╠{borderLine}╣");
        currentY++;

        return currentY;
    }

    /// <summary>
    /// Renders the filter tabs for category selection.
    /// </summary>
    private int RenderFilterTabs(int y)
    {
        var currentY = y;

        // Filter tabs line
        var tabs = "[1] All  [2] Combat  [3] Explore  [4] Progress  [5] Collect  [6] Challenge";
        var tabsLine = $"║ {tabs.PadRight(_config.PanelWidth - 4)} ║";
        _terminalService.WriteAt(_posX, currentY, tabsLine);
        currentY++;

        // Separator
        var borderLine = new string('─', _config.PanelWidth - 2);
        _terminalService.WriteAt(_posX, currentY, $"╟{borderLine}╢");
        currentY++;

        return currentY;
    }
}
