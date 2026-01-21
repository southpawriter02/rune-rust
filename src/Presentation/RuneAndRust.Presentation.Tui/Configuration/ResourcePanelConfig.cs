// ═══════════════════════════════════════════════════════════════════════════════
// ResourcePanelConfig.cs
// Configuration for resource inventory panel display settings.
// Version: 0.13.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for resource inventory panel display settings.
/// </summary>
/// <remarks>
/// <para>Loaded from config/resource-panel.json.</para>
/// <para>Provides customization for panel dimensions, column layout,
/// and category-specific color assignments.</para>
/// </remarks>
public class ResourcePanelConfig
{
    /// <summary>
    /// Width of the resource panel in characters.
    /// </summary>
    /// <remarks>
    /// Default width of 70 characters provides space for multiple columns
    /// with adequate room for resource names and quantities.
    /// </remarks>
    public int PanelWidth { get; set; } = 70;

    /// <summary>
    /// Height of the resource panel in rows.
    /// </summary>
    /// <remarks>
    /// Default height of 20 rows accommodates header, filter tabs,
    /// and multiple resource rows per category.
    /// </remarks>
    public int PanelHeight { get; set; } = 20;

    /// <summary>
    /// Number of columns for category display.
    /// </summary>
    /// <remarks>
    /// Default of 3 columns allows efficient use of horizontal space
    /// while maintaining readability.
    /// </remarks>
    public int ColumnCount { get; set; } = 3;

    /// <summary>
    /// Maximum resources to show per column before overflow indicator.
    /// </summary>
    /// <remarks>
    /// When a category has more resources than this limit, an overflow
    /// indicator ("... +N more") is displayed.
    /// </remarks>
    public int MaxResourcesPerColumn { get; set; } = 6;

    /// <summary>
    /// Color for the active filter tab highlight.
    /// </summary>
    /// <remarks>
    /// Cyan provides high visibility for the currently selected filter.
    /// </remarks>
    public ConsoleColor ActiveFilterColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>
    /// Color for ore resources.
    /// </summary>
    /// <remarks>
    /// DarkYellow (brown/amber) represents mined mineral resources.
    /// </remarks>
    public ConsoleColor OreColor { get; set; } = ConsoleColor.DarkYellow;

    /// <summary>
    /// Color for herb resources.
    /// </summary>
    /// <remarks>
    /// Green represents plant-based gathered resources.
    /// </remarks>
    public ConsoleColor HerbColor { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// Color for leather resources.
    /// </summary>
    /// <remarks>
    /// DarkYellow (brown) represents animal hide resources.
    /// </remarks>
    public ConsoleColor LeatherColor { get; set; } = ConsoleColor.DarkYellow;

    /// <summary>
    /// Color for gem resources.
    /// </summary>
    /// <remarks>
    /// Cyan represents precious stone resources.
    /// </remarks>
    public ConsoleColor GemColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>
    /// Color for miscellaneous resources.
    /// </summary>
    /// <remarks>
    /// White provides neutral coloring for uncategorized resources.
    /// </remarks>
    public ConsoleColor MiscColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Creates a default configuration with standard values.
    /// </summary>
    /// <returns>A new <see cref="ResourcePanelConfig"/> with default values.</returns>
    public static ResourcePanelConfig CreateDefault() => new();
}
