using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Renders bars for various resource types with appropriate colors (read-only, passive display).
/// </summary>
/// <remarks>
/// <para>Wraps <see cref="HealthBarDisplay"/> to provide resource-specific color mapping.</para>
/// <para>Renamed from ResourceBar to ResourceBarDisplay in v0.13.5a for naming convention alignment.</para>
/// </remarks>
public class ResourceBarDisplay
{
    private readonly HealthBarDisplay _healthBarDisplay;
    private readonly ILogger<ResourceBarDisplay>? _logger;
    
    private static readonly Dictionary<string, BarType> ResourceTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["health"] = BarType.Health,
        ["hp"] = BarType.Health,
        ["mana"] = BarType.Mana,
        ["mp"] = BarType.Mana,
        ["experience"] = BarType.Experience,
        ["xp"] = BarType.Experience,
        ["stamina"] = BarType.Stamina,
        ["energy"] = BarType.Stamina,
        ["rage"] = BarType.Custom,
        ["focus"] = BarType.Custom
    };
    
    private static readonly Dictionary<string, ConsoleColor> CustomResourceColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["rage"] = ConsoleColor.DarkRed,
        ["focus"] = ConsoleColor.Cyan
    };
    
    /// <summary>
    /// Initializes a new instance of <see cref="ResourceBarDisplay"/>.
    /// </summary>
    /// <param name="healthBarDisplay">Health bar display component for rendering.</param>
    /// <param name="logger">Optional logger.</param>
    public ResourceBarDisplay(
        HealthBarDisplay healthBarDisplay,
        ILogger<ResourceBarDisplay>? logger = null)
    {
        _healthBarDisplay = healthBarDisplay;
        _logger = logger;
    }
    
    /// <summary>
    /// Renders a resource bar for a named resource type.
    /// </summary>
    /// <param name="resourceName">Resource name (e.g., "Mana", "Rage").</param>
    /// <param name="current">Current value.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="width">Bar width.</param>
    /// <param name="style">Display style.</param>
    /// <returns>Rendered bar string.</returns>
    public string Render(string resourceName, int current, int max, int width, BarStyle style = BarStyle.Standard)
    {
        return _healthBarDisplay.Render(current, max, width, style);
    }
    
    /// <summary>
    /// Gets the color for a named resource type.
    /// </summary>
    /// <param name="resourceName">Resource name.</param>
    /// <param name="current">Current value.</param>
    /// <param name="max">Maximum value.</param>
    /// <returns>ConsoleColor for the resource.</returns>
    public ConsoleColor GetColor(string resourceName, int current, int max)
    {
        if (ResourceTypeMap.TryGetValue(resourceName, out var barType))
        {
            if (barType == BarType.Custom && CustomResourceColors.TryGetValue(resourceName, out var customColor))
            {
                return customColor;
            }
            return _healthBarDisplay.GetThresholdColor(current, max, barType);
        }
        
        _logger?.LogDebug("Unknown resource type '{Resource}', using default color", resourceName);
        return ConsoleColor.Gray;
    }
    
    /// <summary>
    /// Renders a labeled resource bar.
    /// </summary>
    /// <param name="label">Short label (e.g., "MP").</param>
    /// <param name="resourceName">Resource name for color lookup.</param>
    /// <param name="current">Current value.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="totalWidth">Total width including label.</param>
    /// <returns>Formatted bar with label.</returns>
    public string RenderLabeled(string label, string resourceName, int current, int max, int totalWidth)
    {
        return _healthBarDisplay.RenderLabeled(label, current, max, totalWidth);
    }
    
    /// <summary>
    /// Gets the bar type for a resource name.
    /// </summary>
    /// <param name="resourceName">Resource name.</param>
    /// <returns>Bar type or Custom if unknown.</returns>
    public static BarType GetBarTypeForResource(string resourceName)
    {
        return ResourceTypeMap.TryGetValue(resourceName, out var barType) 
            ? barType 
            : BarType.Custom;
    }
}
