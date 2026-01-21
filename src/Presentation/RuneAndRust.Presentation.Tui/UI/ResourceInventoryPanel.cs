// ═══════════════════════════════════════════════════════════════════════════════
// ResourceInventoryPanel.cs
// Displays the player's crafting resources organized by type.
// Version: 0.13.3a
// ═══════════════════════════════════════════════════════════════════════════════

using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Displays the player's crafting resources organized by type.
/// </summary>
/// <remarks>
/// <para>Resources are grouped into categories (Ore, Herb, Leather, Gem, Misc)
/// and displayed in a column layout with type icons and quantities.</para>
/// <para>Filter tabs allow viewing specific resource types.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item><description>Categorized column display with type headers</description></item>
///   <item><description>Filter tabs for All/Ore/Herb/Leather/Gem/Misc</description></item>
///   <item><description>Type-specific icons and colors</description></item>
///   <item><description>Overflow indicators for large collections</description></item>
///   <item><description>Resource detail popup</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var panel = new ResourceInventoryPanel(stackRenderer, terminalService, config, logger);
/// panel.SetPosition(10, 5);
/// panel.RenderResources(playerResources);
/// panel.SetFilter(ResourceCategory.Ore);  // Filter to ore only
/// </code>
/// </example>
public class ResourceInventoryPanel
{
    private readonly ResourceStackRenderer _stackRenderer;
    private readonly ITerminalService _terminalService;
    private readonly ResourcePanelConfig _config;
    private readonly ILogger<ResourceInventoryPanel> _logger;

    private ResourceCategory? _activeFilter;
    private IReadOnlyList<ResourceStackDisplayDto> _resources = Array.Empty<ResourceStackDisplayDto>();
    private (int X, int Y) _panelPosition;

    /// <summary>
    /// Gets the currently active filter category.
    /// </summary>
    /// <remarks>
    /// Null indicates "All" filter (show all categories).
    /// </remarks>
    public ResourceCategory? ActiveFilter => _activeFilter;

    /// <summary>
    /// Creates a new instance of the ResourceInventoryPanel component.
    /// </summary>
    /// <param name="stackRenderer">The renderer for resource stacks.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for panel display settings. If null, defaults are used.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stackRenderer"/> or <paramref name="terminalService"/> is null.
    /// </exception>
    public ResourceInventoryPanel(
        ResourceStackRenderer stackRenderer,
        ITerminalService terminalService,
        ResourcePanelConfig? config = null,
        ILogger<ResourceInventoryPanel>? logger = null)
    {
        _stackRenderer = stackRenderer ?? throw new ArgumentNullException(nameof(stackRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? ResourcePanelConfig.CreateDefault();
        _logger = logger ?? NullLogger<ResourceInventoryPanel>.Instance;

        _logger.LogDebug(
            "ResourceInventoryPanel initialized with {PanelWidth}x{PanelHeight} panel",
            _config.PanelWidth,
            _config.PanelHeight);
    }

    /// <summary>
    /// Sets the panel position on the terminal.
    /// </summary>
    /// <param name="x">The X coordinate (column).</param>
    /// <param name="y">The Y coordinate (row).</param>
    public void SetPosition(int x, int y)
    {
        _panelPosition = (x, y);

        _logger.LogDebug(
            "Panel position set to ({X}, {Y})",
            x, y);
    }

    /// <summary>
    /// Renders all resources in the panel.
    /// </summary>
    /// <param name="resources">The player's resources.</param>
    /// <remarks>
    /// <para>Resources are grouped by category and rendered in columns.</para>
    /// <para>If a filter is active, only matching categories are displayed.</para>
    /// </remarks>
    public void RenderResources(IEnumerable<ResourceStackDisplayDto> resources)
    {
        _resources = resources.ToList();

        _logger.LogInformation(
            "Rendering {Count} resources with filter {Filter}",
            _resources.Count,
            _activeFilter?.ToString() ?? "All");

        // Clear panel area
        ClearPanel();

        // Render panel header
        RenderHeader();

        // Render filter tabs
        RenderFilterTabs();

        // Handle empty state
        if (_resources.Count == 0)
        {
            RenderEmptyState();
            _logger.LogWarning("No resources to display");
            return;
        }

        // Group resources by category
        var grouped = GroupByCategory(_resources);

        // Apply active filter if set
        if (_activeFilter.HasValue)
        {
            grouped = grouped
                .Where(g => g.Key == _activeFilter.Value)
                .ToDictionary(g => g.Key, g => g.Value);
        }

        // Render resource categories in columns
        RenderCategories(grouped);

        // Render bottom border
        RenderBottomBorder();

        _logger.LogDebug(
            "Rendered {Count} resources across {CategoryCount} categories",
            _resources.Count,
            grouped.Count);
    }

    /// <summary>
    /// Renders resources filtered to a specific type.
    /// </summary>
    /// <param name="category">The resource category to display.</param>
    public void RenderResourcesByType(ResourceCategory category)
    {
        _logger.LogDebug("Rendering resources by type: {Category}", category);

        SetFilter(category);
        RenderResources(_resources);
    }

    /// <summary>
    /// Sets the active filter category.
    /// </summary>
    /// <param name="category">The category to filter by, or null for all.</param>
    public void SetFilter(ResourceCategory? category)
    {
        _activeFilter = category;

        _logger.LogDebug(
            "Set filter to {Category}",
            category?.ToString() ?? "All");
    }

    /// <summary>
    /// Shows details for a selected resource.
    /// </summary>
    /// <param name="resource">The resource to display details for.</param>
    /// <remarks>
    /// Displays a detail line at the bottom of the panel showing
    /// the resource name and description.
    /// </remarks>
    public void ShowResourceDetails(ResourceStackDisplayDto resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        // Render resource detail at the bottom of the panel
        var detailY = _panelPosition.Y + _config.PanelHeight - 3;
        var detailLine = $"{resource.DisplayName}: {resource.Description}";

        // Truncate if too long
        if (detailLine.Length > _config.PanelWidth - 4)
        {
            detailLine = detailLine.Substring(0, _config.PanelWidth - 7) + "...";
        }

        // Clear detail area
        var clearLine = new string(' ', _config.PanelWidth - 4);
        _terminalService.WriteAt(_panelPosition.X + 2, detailY, clearLine);

        // Write detail
        _terminalService.WriteAt(_panelPosition.X + 2, detailY, detailLine);

        _logger.LogDebug(
            "Showed details for resource {ResourceId}",
            resource.ResourceId);
    }

    /// <summary>
    /// Handles filter input from keyboard.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <returns>True if the input was handled.</returns>
    /// <remarks>
    /// <para>Key mappings:</para>
    /// <list type="bullet">
    ///   <item><description>1 or NumPad1: All (no filter)</description></item>
    ///   <item><description>2 or NumPad2: Ore</description></item>
    ///   <item><description>3 or NumPad3: Herb</description></item>
    ///   <item><description>4 or NumPad4: Leather</description></item>
    ///   <item><description>5 or NumPad5: Gem</description></item>
    ///   <item><description>6 or NumPad6: Misc</description></item>
    /// </list>
    /// </remarks>
    public bool HandleFilterInput(ConsoleKey key)
    {
        // Map key to filter
        var filter = key switch
        {
            ConsoleKey.D1 or ConsoleKey.NumPad1 => (ResourceCategory?)null,
            ConsoleKey.D2 or ConsoleKey.NumPad2 => ResourceCategory.Ore,
            ConsoleKey.D3 or ConsoleKey.NumPad3 => ResourceCategory.Herb,
            ConsoleKey.D4 or ConsoleKey.NumPad4 => ResourceCategory.Leather,
            ConsoleKey.D5 or ConsoleKey.NumPad5 => ResourceCategory.Gem,
            ConsoleKey.D6 or ConsoleKey.NumPad6 => ResourceCategory.Misc,
            _ => _activeFilter // No change for unrecognized keys
        };

        // Check if this is a filter key (1-6)
        var isFilterKey = key is >= ConsoleKey.D1 and <= ConsoleKey.D6 or
            >= ConsoleKey.NumPad1 and <= ConsoleKey.NumPad6;

        if (!isFilterKey)
        {
            return false;
        }

        // Apply filter and re-render
        _logger.LogDebug(
            "Filter input: {Key} -> {Filter}",
            key,
            filter?.ToString() ?? "All");

        SetFilter(filter);
        RenderResources(_resources);

        return true;
    }

    #region Private Methods

    /// <summary>
    /// Clears the entire panel area.
    /// </summary>
    private void ClearPanel()
    {
        var blankLine = new string(' ', _config.PanelWidth);

        for (var y = 0; y < _config.PanelHeight; y++)
        {
            _terminalService.WriteAt(_panelPosition.X, _panelPosition.Y + y, blankLine);
        }
    }

    /// <summary>
    /// Renders the panel header with title.
    /// </summary>
    private void RenderHeader()
    {
        var headerY = _panelPosition.Y;
        var width = _config.PanelWidth;

        // Top border: ┌─────────────────────────┐
        var topBorder = $"┌{new string('─', width - 2)}┐";
        _terminalService.WriteAt(_panelPosition.X, headerY, topBorder);

        // Title row: │       RESOURCES         │
        var title = "RESOURCES";
        var titlePadding = (width - 2 - title.Length) / 2;
        var rightPadding = width - 2 - titlePadding - title.Length;
        var titleLine = $"│{new string(' ', titlePadding)}{title}{new string(' ', rightPadding)}│";
        _terminalService.WriteAt(_panelPosition.X, headerY + 1, titleLine);

        // Separator: ├─────────────────────────┤
        var separator = $"├{new string('─', width - 2)}┤";
        _terminalService.WriteAt(_panelPosition.X, headerY + 2, separator);
    }

    /// <summary>
    /// Renders the filter tab bar.
    /// </summary>
    private void RenderFilterTabs()
    {
        var tabY = _panelPosition.Y + 3;
        var tabLine = new StringBuilder();
        tabLine.Append("│  Filter: ");

        // Define filter options
        var filters = new (string Name, ResourceCategory? Category)[]
        {
            ("All", null),
            ("Ore", ResourceCategory.Ore),
            ("Herb", ResourceCategory.Herb),
            ("Leather", ResourceCategory.Leather),
            ("Gem", ResourceCategory.Gem),
            ("Misc", ResourceCategory.Misc)
        };

        // Build tab string
        for (var i = 0; i < filters.Length; i++)
        {
            var (name, category) = filters[i];
            var isActive = _activeFilter == category;

            // Active tabs have brackets, inactive have spaces
            var bracket = isActive ? $"[{name}]" : $" {name} ";
            tabLine.Append(bracket);

            if (i < filters.Length - 1)
            {
                tabLine.Append(' ');
            }
        }

        // Pad to panel width
        var currentLength = tabLine.Length;
        var remainingWidth = _config.PanelWidth - currentLength - 1;
        if (remainingWidth > 0)
        {
            tabLine.Append(new string(' ', remainingWidth));
        }
        tabLine.Append('│');

        // Write the base tab line
        _terminalService.WriteAt(_panelPosition.X, tabY, tabLine.ToString());

        // Now highlight the active tab with color
        var tabX = _panelPosition.X + 11; // After "│  Filter: "

        for (var i = 0; i < filters.Length; i++)
        {
            var (name, category) = filters[i];
            var isActive = _activeFilter == category;

            if (isActive)
            {
                _terminalService.WriteColoredAt(
                    tabX,
                    tabY,
                    $"[{name}]",
                    _config.ActiveFilterColor);
            }

            // Move to next tab position
            tabX += name.Length + 3; // Name length + brackets + space
        }

        // Separator after filter tabs
        var separator = $"├{new string('─', _config.PanelWidth - 2)}┤";
        _terminalService.WriteAt(_panelPosition.X, tabY + 1, separator);
    }

    /// <summary>
    /// Renders the categorized resource columns.
    /// </summary>
    /// <param name="grouped">Resources grouped by category.</param>
    private void RenderCategories(Dictionary<ResourceCategory, List<ResourceStackDisplayDto>> grouped)
    {
        var contentY = _panelPosition.Y + 5;
        var columnWidth = (_config.PanelWidth - 4) / _config.ColumnCount;

        // Determine which categories to show
        var categories = _activeFilter.HasValue
            ? new[] { _activeFilter.Value }
            : Enum.GetValues<ResourceCategory>();

        var columnIndex = 0;

        foreach (var category in categories)
        {
            // Skip empty categories
            if (!grouped.TryGetValue(category, out var resources) || resources.Count == 0)
            {
                continue;
            }

            // Calculate column position
            var columnX = _panelPosition.X + 2 + (columnIndex * columnWidth);

            // Render the category column
            RenderCategoryColumn(columnX, contentY, category, resources, columnWidth);

            // Move to next column position
            columnIndex++;

            // Wrap to next row if we've exceeded column count
            if (columnIndex >= _config.ColumnCount)
            {
                columnIndex = 0;
                contentY += _config.MaxResourcesPerColumn + 2;
            }
        }
    }

    /// <summary>
    /// Renders a single category column with header and resources.
    /// </summary>
    /// <param name="x">X position of the column.</param>
    /// <param name="y">Y position of the column.</param>
    /// <param name="category">The resource category.</param>
    /// <param name="resources">The resources in this category.</param>
    /// <param name="columnWidth">Width of the column.</param>
    private void RenderCategoryColumn(
        int x,
        int y,
        ResourceCategory category,
        List<ResourceStackDisplayDto> resources,
        int columnWidth)
    {
        // Category header (uppercase)
        var header = category.ToString().ToUpperInvariant();
        var underline = new string('─', header.Length);

        // Write header with category color
        _terminalService.WriteColoredAt(x, y, header, _stackRenderer.GetTypeColor(category));
        _terminalService.WriteAt(x, y + 1, underline);

        // Resource stacks
        var resourceY = y + 2;
        var displayCount = Math.Min(resources.Count, _config.MaxResourcesPerColumn);

        for (var i = 0; i < displayCount; i++)
        {
            var resource = resources[i];
            var formattedStack = _stackRenderer.FormatStack(resource, columnWidth - 5); // -5 for icon + space + padding

            // Write type icon with color
            _terminalService.WriteColoredAt(
                x,
                resourceY + i,
                _stackRenderer.GetTypeIcon(category),
                _stackRenderer.GetTypeColor(category));

            // Write resource name and quantity
            _terminalService.WriteAt(
                x + 4,
                resourceY + i,
                formattedStack);
        }

        // Show overflow indicator if needed
        if (resources.Count > _config.MaxResourcesPerColumn)
        {
            var overflowCount = resources.Count - displayCount;
            var overflowText = $"... +{overflowCount} more";

            _terminalService.WriteColoredAt(
                x,
                resourceY + displayCount,
                overflowText,
                ConsoleColor.DarkGray);

            _logger.LogDebug(
                "Category {Category} has {OverflowCount} resources in overflow",
                category,
                overflowCount);
        }
    }

    /// <summary>
    /// Renders an empty state message when there are no resources.
    /// </summary>
    private void RenderEmptyState()
    {
        var contentY = _panelPosition.Y + 6;
        var message = "No resources collected yet.";
        var padding = (_config.PanelWidth - 2 - message.Length) / 2;

        var line = $"│{new string(' ', padding)}{message}{new string(' ', _config.PanelWidth - 2 - padding - message.Length)}│";
        _terminalService.WriteAt(_panelPosition.X, contentY, line);

        // Also render bottom border for empty state
        RenderBottomBorder();
    }

    /// <summary>
    /// Renders the bottom border of the panel.
    /// </summary>
    private void RenderBottomBorder()
    {
        var bottomY = _panelPosition.Y + _config.PanelHeight - 1;
        var bottomBorder = $"└{new string('─', _config.PanelWidth - 2)}┘";
        _terminalService.WriteAt(_panelPosition.X, bottomY, bottomBorder);
    }

    /// <summary>
    /// Groups resources by category.
    /// </summary>
    /// <param name="resources">The resources to group.</param>
    /// <returns>Dictionary of resources grouped by category.</returns>
    private static Dictionary<ResourceCategory, List<ResourceStackDisplayDto>> GroupByCategory(
        IReadOnlyList<ResourceStackDisplayDto> resources)
    {
        return resources
            .GroupBy(r => r.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    #endregion
}
