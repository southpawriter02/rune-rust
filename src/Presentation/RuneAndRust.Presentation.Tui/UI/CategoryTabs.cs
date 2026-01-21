// ═══════════════════════════════════════════════════════════════════════════════
// CategoryTabs.cs
// UI component for displaying and managing category filter tabs.
// Version: 0.13.4b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays category tabs for filtering statistics by category.
/// </summary>
/// <remarks>
/// <para>
/// Provides visual tab selection with highlighting for the active category.
/// Supports the five statistics categories: Combat, Exploration, Progression,
/// Time, and Dice.
/// </para>
/// <para>Tab visual states:</para>
/// <list type="bullet">
///   <item><description>Active: [TabName] - brackets around name</description></item>
///   <item><description>Inactive: TabName - plain text</description></item>
/// </list>
/// </remarks>
public class CategoryTabs
{
    private readonly ITerminalService _terminalService;
    private readonly StatisticsDashboardConfig _config;
    private readonly ILogger<CategoryTabs>? _logger;

    /// <summary>
    /// Tab definitions with name, category, and icon.
    /// </summary>
    private static readonly (string Name, StatisticCategory Category, string Icon)[] TabDefinitions =
    {
        ("Combat", StatisticCategory.Combat, "⚔"),
        ("Explore", StatisticCategory.Exploration, "🗺"),
        ("Progress", StatisticCategory.Progression, "📈"),
        ("Time", StatisticCategory.Time, "⏱"),
        ("Dice", StatisticCategory.Dice, "🎲")
    };

    /// <summary>
    /// Creates a new instance of the CategoryTabs component.
    /// </summary>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Optional configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when terminalService is null.</exception>
    public CategoryTabs(
        ITerminalService terminalService,
        StatisticsDashboardConfig? config = null,
        ILogger<CategoryTabs>? logger = null)
    {
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new StatisticsDashboardConfig();
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the category tabs at the specified position.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="activeCategory">The currently active category to highlight.</param>
    public void RenderTabs(int x, int y, StatisticCategory activeCategory)
    {
        _logger?.LogDebug(
            "Rendering category tabs at ({X}, {Y}) with active: {ActiveCategory}",
            x, y, activeCategory);

        var tabLine = "  ";
        var keyNum = 1;

        foreach (var (name, category, _) in TabDefinitions)
        {
            var isActive = category == activeCategory;
            var tabText = isActive ? $"[{keyNum}.{name}]" : $" {keyNum}.{name} ";
            tabLine += tabText + " ";
            keyNum++;
        }

        var paddedTabs = tabLine.PadRight(_config.PanelWidth - 2);
        _terminalService.WriteAt(x, y, $"|{paddedTabs}|");
        _terminalService.WriteAt(x, y + 1, $"+{new string('-', _config.PanelWidth - 2)}+");

        _logger?.LogDebug("Rendered category tabs with active: {ActiveCategory}", activeCategory);
    }

    /// <summary>
    /// Renders the tabs with color highlighting for active/inactive states.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="activeCategory">The currently active category.</param>
    public void RenderTabsColored(int x, int y, StatisticCategory activeCategory)
    {
        _logger?.LogDebug("Rendering colored tabs at ({X}, {Y})", x, y);

        var currentX = x + 3;
        var keyNum = 1;

        _terminalService.WriteAt(x, y, "|  ");

        foreach (var (name, category, _) in TabDefinitions)
        {
            var isActive = category == activeCategory;
            var tabText = isActive ? $"[{keyNum}.{name}]" : $" {keyNum}.{name} ";
            var color = isActive ? _config.ActiveTabColor : _config.InactiveTabColor;

            _terminalService.WriteColoredAt(currentX, y, tabText, color);
            currentX += tabText.Length + 1;
            keyNum++;
        }

        // Pad remainder of line
        var remaining = _config.PanelWidth - currentX + x - 1;
        if (remaining > 0)
        {
            _terminalService.WriteAt(currentX, y, new string(' ', remaining) + "|");
        }

        _terminalService.WriteAt(x, y + 1, $"+{new string('-', _config.PanelWidth - 2)}+");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TAB MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Selects a category tab.
    /// </summary>
    /// <param name="category">The category to select.</param>
    public void SelectTab(StatisticCategory category)
    {
        _logger?.LogDebug("Tab selected: {Category}", category);
    }

    /// <summary>
    /// Gets the icon for a category.
    /// </summary>
    /// <param name="category">The category to get the icon for.</param>
    /// <returns>The category icon string.</returns>
    public string GetTabIcon(StatisticCategory category)
    {
        return TabDefinitions.FirstOrDefault(t => t.Category == category).Icon ?? "📊";
    }

    /// <summary>
    /// Gets the display name for a category.
    /// </summary>
    /// <param name="category">The category to get the name for.</param>
    /// <returns>The category display name.</returns>
    public string GetTabName(StatisticCategory category)
    {
        return TabDefinitions.FirstOrDefault(t => t.Category == category).Name ?? category.ToString();
    }

    /// <summary>
    /// Highlights the active tab visually.
    /// </summary>
    /// <param name="category">The category to highlight.</param>
    public void HighlightActive(StatisticCategory category)
    {
        _logger?.LogDebug("Highlighting active tab: {Category}", category);
    }

    /// <summary>
    /// Gets the category for a given key number (1-5).
    /// </summary>
    /// <param name="keyNumber">The key number (1-5).</param>
    /// <returns>The corresponding category, or null if invalid key.</returns>
    public StatisticCategory? GetCategoryForKey(int keyNumber)
    {
        if (keyNumber < 1 || keyNumber > TabDefinitions.Length)
        {
            return null;
        }

        return TabDefinitions[keyNumber - 1].Category;
    }
}
