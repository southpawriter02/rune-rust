using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Renders bars for various resource types with appropriate colors.
/// </summary>
/// <remarks>
/// Wraps <see cref="HealthBar"/> to provide resource-specific color mapping.
/// </remarks>
public class ResourceBar
{
    private readonly HealthBar _healthBar;
    private readonly ILogger<ResourceBar>? _logger;
    
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
    /// Initializes a new instance of <see cref="ResourceBar"/>.
    /// </summary>
    /// <param name="healthBar">Health bar component for rendering.</param>
    /// <param name="logger">Optional logger.</param>
    public ResourceBar(
        HealthBar healthBar,
        ILogger<ResourceBar>? logger = null)
    {
        _healthBar = healthBar;
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
        return _healthBar.Render(current, max, width, style);
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
            return _healthBar.GetThresholdColor(current, max, barType);
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
        return _healthBar.RenderLabeled(label, current, max, totalWidth);
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
