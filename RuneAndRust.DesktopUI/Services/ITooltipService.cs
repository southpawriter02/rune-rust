using System.Collections.Generic;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.20: Data model for tooltip content.
/// Provides structured tooltip information for UI elements.
/// </summary>
public class TooltipData
{
    /// <summary>
    /// Unique identifier key for the tooltip.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Short title for the tooltip.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Brief description shown in the tooltip.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Extended help text for the help browser.
    /// </summary>
    public string? DetailedHelp { get; set; }

    /// <summary>
    /// Category for grouping in help browser.
    /// </summary>
    public string Category { get; set; } = "General";

    /// <summary>
    /// Optional keyboard shortcut hint.
    /// </summary>
    public string? Shortcut { get; set; }

    /// <summary>
    /// Optional icon/emoji for visual identification.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Whether this tooltip is for a new feature (shows "NEW" badge).
    /// </summary>
    public bool IsNew { get; set; }

    /// <summary>
    /// Tags for enhanced search functionality.
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// v0.43.20: Service interface for tooltip and help system.
/// Provides contextual tooltips for all UI elements and searchable help.
/// </summary>
public interface ITooltipService
{
    /// <summary>
    /// Gets a tooltip by its key.
    /// </summary>
    /// <param name="key">The tooltip key.</param>
    /// <returns>The tooltip data, or a default if not found.</returns>
    TooltipData GetTooltip(string key);

    /// <summary>
    /// Registers a tooltip with the service.
    /// </summary>
    /// <param name="key">Unique key for the tooltip.</param>
    /// <param name="title">Short title.</param>
    /// <param name="description">Brief description.</param>
    /// <param name="detailedHelp">Optional extended help text.</param>
    /// <param name="category">Category for grouping.</param>
    void RegisterTooltip(string key, string title, string description,
        string? detailedHelp = null, string category = "General");

    /// <summary>
    /// Registers a tooltip data object directly.
    /// </summary>
    /// <param name="tooltip">The tooltip data to register.</param>
    void RegisterTooltip(TooltipData tooltip);

    /// <summary>
    /// Searches tooltips by query string.
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <returns>Matching tooltips ordered by relevance.</returns>
    IEnumerable<TooltipData> SearchTooltips(string query);

    /// <summary>
    /// Gets all tooltips in a category.
    /// </summary>
    /// <param name="category">Category name.</param>
    /// <returns>All tooltips in the category.</returns>
    IEnumerable<TooltipData> GetByCategory(string category);

    /// <summary>
    /// Gets all available categories.
    /// </summary>
    /// <returns>List of category names.</returns>
    IEnumerable<string> GetCategories();

    /// <summary>
    /// Gets all registered tooltips.
    /// </summary>
    /// <returns>All tooltips.</returns>
    IEnumerable<TooltipData> GetAllTooltips();

    /// <summary>
    /// Checks if a tooltip exists.
    /// </summary>
    /// <param name="key">The tooltip key.</param>
    /// <returns>True if the tooltip exists.</returns>
    bool HasTooltip(string key);
}
