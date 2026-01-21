// ═══════════════════════════════════════════════════════════════════════════════
// ResourceStackRenderer.cs
// Renders resource stacks with type icons, names, and quantities.
// Version: 0.13.3a
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renders resource stacks with type icons, names, and quantities.
/// </summary>
/// <remarks>
/// <para>ResourceStackRenderer is a stateless renderer that provides methods for:</para>
/// <list type="bullet">
///   <item><description>Formatting resource stacks for display</description></item>
///   <item><description>Getting category-specific type icons ([O], [H], [L], [G], [M])</description></item>
///   <item><description>Getting category-specific colors</description></item>
///   <item><description>Formatting quantities (x## or x#k for large amounts)</description></item>
/// </list>
/// <para>Configuration is loaded from <c>config/resource-panel.json</c>.</para>
/// </remarks>
/// <example>
/// <code>
/// var renderer = new ResourceStackRenderer(config, logger);
/// var icon = renderer.GetTypeIcon(ResourceCategory.Ore);     // Returns "[O]"
/// var color = renderer.GetTypeColor(ResourceCategory.Herb);  // Returns Green
/// var qty = renderer.FormatQuantity(1500);                   // Returns "x1k"
/// </code>
/// </example>
public class ResourceStackRenderer
{
    private readonly ResourcePanelConfig _config;
    private readonly ILogger<ResourceStackRenderer> _logger;

    /// <summary>
    /// Creates a new instance of the ResourceStackRenderer.
    /// </summary>
    /// <param name="config">Configuration for display settings. If null, defaults are used.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public ResourceStackRenderer(
        ResourcePanelConfig? config = null,
        ILogger<ResourceStackRenderer>? logger = null)
    {
        _config = config ?? ResourcePanelConfig.CreateDefault();
        _logger = logger ?? NullLogger<ResourceStackRenderer>.Instance;

        _logger.LogDebug(
            "ResourceStackRenderer initialized with panel width {PanelWidth}",
            _config.PanelWidth);
    }

    /// <summary>
    /// Formats a resource stack for display within the specified width.
    /// </summary>
    /// <param name="resource">The resource to format.</param>
    /// <param name="maxWidth">Maximum width for the formatted string.</param>
    /// <returns>The formatted resource string with name and quantity.</returns>
    /// <remarks>
    /// <para>The format is: "ResourceName     x##" where the name is left-padded
    /// and the quantity is right-aligned.</para>
    /// <para>If the name exceeds available width, it is truncated with "..".</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new ResourceStackDisplayDto("iron-ore", "Iron Ore", "Common ore", ResourceCategory.Ore, 24);
    /// var formatted = renderer.FormatStack(dto, 20);
    /// // Returns: "Iron Ore         x24"
    /// </code>
    /// </example>
    public string FormatStack(ResourceStackDisplayDto resource, int maxWidth)
    {
        ArgumentNullException.ThrowIfNull(resource);

        // Format the quantity string first to know how much space it needs
        var quantity = FormatQuantity(resource.Quantity);

        // Calculate available width for name (subtract quantity length + 1 space)
        var availableWidth = maxWidth - quantity.Length - 1;

        // Truncate name if necessary
        var name = resource.DisplayName;
        if (name.Length > availableWidth)
        {
            // Truncate and add ".." indicator
            name = name.Substring(0, Math.Max(0, availableWidth - 2)) + "..";
        }

        // Build the formatted string with right-aligned quantity
        var result = $"{name.PadRight(availableWidth)} {quantity}";

        _logger.LogDebug(
            "Formatted stack: {ResourceId} as '{FormattedStack}'",
            resource.ResourceId,
            result);

        return result;
    }

    /// <summary>
    /// Gets the type icon for a resource category.
    /// </summary>
    /// <param name="category">The resource category.</param>
    /// <returns>The type icon string (e.g., "[O]", "[H]", "[L]", "[G]", "[M]").</returns>
    /// <remarks>
    /// <para>Icons are single-letter abbreviations in brackets:</para>
    /// <list type="bullet">
    ///   <item><description>[O] - Ore (mining resources)</description></item>
    ///   <item><description>[H] - Herb (plant resources)</description></item>
    ///   <item><description>[L] - Leather (animal hides)</description></item>
    ///   <item><description>[G] - Gem (precious stones)</description></item>
    ///   <item><description>[M] - Misc (other materials)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var icon = renderer.GetTypeIcon(ResourceCategory.Ore);
    /// // Returns: "[O]"
    /// </code>
    /// </example>
    public string GetTypeIcon(ResourceCategory category)
    {
        var icon = category switch
        {
            ResourceCategory.Ore => "[O]",
            ResourceCategory.Herb => "[H]",
            ResourceCategory.Leather => "[L]",
            ResourceCategory.Gem => "[G]",
            ResourceCategory.Misc => "[M]",
            _ => "[?]"
        };

        _logger.LogDebug(
            "GetTypeIcon for {Category} returned '{Icon}'",
            category,
            icon);

        return icon;
    }

    /// <summary>
    /// Gets the display color for a resource category.
    /// </summary>
    /// <param name="category">The resource category.</param>
    /// <returns>The console color for the category.</returns>
    /// <remarks>
    /// <para>Colors are configured in <c>config/resource-panel.json</c>.</para>
    /// <para>Default colors are:</para>
    /// <list type="bullet">
    ///   <item><description>Ore: DarkYellow (brown)</description></item>
    ///   <item><description>Herb: Green</description></item>
    ///   <item><description>Leather: DarkYellow (brown)</description></item>
    ///   <item><description>Gem: Cyan</description></item>
    ///   <item><description>Misc: White</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var color = renderer.GetTypeColor(ResourceCategory.Herb);
    /// // Returns: ConsoleColor.Green
    /// </code>
    /// </example>
    public ConsoleColor GetTypeColor(ResourceCategory category)
    {
        var color = category switch
        {
            ResourceCategory.Ore => _config.OreColor,
            ResourceCategory.Herb => _config.HerbColor,
            ResourceCategory.Leather => _config.LeatherColor,
            ResourceCategory.Gem => _config.GemColor,
            ResourceCategory.Misc => _config.MiscColor,
            _ => ConsoleColor.White
        };

        _logger.LogDebug(
            "GetTypeColor for {Category} returned {Color}",
            category,
            color);

        return color;
    }

    /// <summary>
    /// Formats a quantity for display.
    /// </summary>
    /// <param name="quantity">The quantity value.</param>
    /// <returns>The formatted quantity string (e.g., "x24", "x1k").</returns>
    /// <remarks>
    /// <para>Formatting rules:</para>
    /// <list type="bullet">
    ///   <item><description>Quantities less than 1000: "x##" (e.g., x24, x156)</description></item>
    ///   <item><description>Quantities 1000 or more: "x#k" (e.g., x1k, x25k)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var small = renderer.FormatQuantity(24);   // Returns: "x24"
    /// var large = renderer.FormatQuantity(2500); // Returns: "x2k"
    /// </code>
    /// </example>
    public string FormatQuantity(int quantity)
    {
        string formatted;

        if (quantity >= 1000)
        {
            // Format large quantities as "x#k"
            formatted = $"x{quantity / 1000}k";
        }
        else
        {
            // Format normal quantities as "x##"
            formatted = $"x{quantity}";
        }

        _logger.LogDebug(
            "FormatQuantity: {Quantity} -> '{Formatted}'",
            quantity,
            formatted);

        return formatted;
    }

    /// <summary>
    /// Formats a resource stack with the full icon prefix.
    /// </summary>
    /// <param name="resource">The resource to format.</param>
    /// <returns>The formatted string with icon, name, and quantity.</returns>
    /// <remarks>
    /// <para>Format: "[X] ResourceName x##"</para>
    /// <para>This is a convenience method combining icon, name, and quantity.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new ResourceStackDisplayDto("iron-ore", "Iron Ore", "Common ore", ResourceCategory.Ore, 24);
    /// var formatted = renderer.FormatStackWithIcon(dto);
    /// // Returns: "[O] Iron Ore x24"
    /// </code>
    /// </example>
    public string FormatStackWithIcon(ResourceStackDisplayDto resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var icon = GetTypeIcon(resource.Category);
        var name = resource.DisplayName;
        var quantity = FormatQuantity(resource.Quantity);

        var result = $"{icon} {name} {quantity}";

        _logger.LogDebug(
            "FormatStackWithIcon: {ResourceId} -> '{Result}'",
            resource.ResourceId,
            result);

        return result;
    }

    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    /// <returns>The resource panel configuration.</returns>
    public ResourcePanelConfig GetConfig() => _config;
}
