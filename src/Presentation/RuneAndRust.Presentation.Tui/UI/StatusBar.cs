using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Renders the persistent status bar in the footer panel.
/// </summary>
/// <remarks>
/// The status bar displays player information (HP, MP, XP, Gold, Location)
/// and automatically refreshes when the underlying data changes.
/// Supports three format modes:
/// <list type="bullet">
/// <item><description>Compact: For narrow terminals (&lt;100 width)</description></item>
/// <item><description>Expanded: For wide terminals (≥100 width)</description></item>
/// <item><description>Combat: When in active combat</description></item>
/// </list>
/// </remarks>
public class StatusBar : IDisposable
{
    private readonly IStatusBarDataProvider _dataProvider;
    private readonly ScreenLayout _layout;
    private readonly ILogger<StatusBar> _logger;
    private bool _disposed;
    
    /// <summary>
    /// Width threshold for switching to expanded format.
    /// </summary>
    public const int ExpandedWidthThreshold = 100;
    
    /// <summary>
    /// HP percentage threshold for low HP warning.
    /// </summary>
    public const double LowHpThreshold = 0.25;
    
    /// <summary>
    /// Initializes a new instance of <see cref="StatusBar"/>.
    /// </summary>
    /// <param name="dataProvider">Data source for status bar content.</param>
    /// <param name="layout">Screen layout for rendering.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public StatusBar(
        IStatusBarDataProvider dataProvider,
        ScreenLayout layout,
        ILogger<StatusBar> logger)
    {
        _dataProvider = dataProvider;
        _layout = layout;
        _logger = logger;
        
        _dataProvider.OnDataChanged += Refresh;
        
        _logger.LogDebug("StatusBar initialized");
    }
    
    /// <summary>
    /// Renders the status bar to the footer panel.
    /// </summary>
    public void Render()
    {
        var panel = _layout.GetPanel(PanelPosition.Footer);
        if (panel == null)
        {
            _logger.LogWarning("Footer panel not found");
            return;
        }
        
        var content = FormatContent(panel.Value.ContentWidth);
        _layout.RenderToPanel(PanelPosition.Footer, new[] { content });
        
        _logger.LogDebug("StatusBar rendered: {Content}", content);
    }
    
    /// <summary>
    /// Refreshes the status bar content.
    /// </summary>
    public void Refresh()
    {
        if (_disposed) return;
        Render();
    }
    
    /// <summary>
    /// Formats the status bar content for the available width.
    /// </summary>
    /// <param name="availableWidth">Width in characters.</param>
    /// <returns>Formatted status string.</returns>
    public string FormatContent(int availableWidth)
    {
        var combat = _dataProvider.CombatInfo;
        
        if (combat.HasValue)
        {
            return FormatCombatMode(availableWidth, combat.Value);
        }
        
        return availableWidth >= ExpandedWidthThreshold
            ? FormatExpanded(availableWidth)
            : FormatCompact(availableWidth);
    }
    
    /// <summary>
    /// Formats status for compact display (&lt;100 width).
    /// </summary>
    private string FormatCompact(int width)
    {
        var health = _dataProvider.Health;
        var resource = _dataProvider.PrimaryResource;
        var xp = _dataProvider.Experience;
        var gold = _dataProvider.Gold;
        var location = _dataProvider.Location;
        
        var hpWarning = IsLowHp() ? " ⚠" : "";
        
        var parts = new List<string>
        {
            $"HP:{health.Current}/{health.Max}{hpWarning}"
        };
        
        if (resource.HasValue)
        {
            parts.Add($"{GetResourceAbbrev(resource.Value.Name)}:{resource.Value.Current}/{resource.Value.Max}");
        }
        
        parts.Add($"XP:{xp.Current}/{xp.ToNext}");
        parts.Add($"Gold:{gold}");
        
        // Calculate remaining space for location
        var used = string.Join(" | ", parts).Length;
        var locationSpace = Math.Max(0, width - used - 3);
        if (locationSpace > 0)
        {
            parts.Add(TruncateLocation(location, locationSpace));
        }
        
        return string.Join(" | ", parts);
    }
    
    /// <summary>
    /// Formats status for expanded display (≥100 width).
    /// </summary>
    private string FormatExpanded(int width)
    {
        var health = _dataProvider.Health;
        var resource = _dataProvider.PrimaryResource;
        var xp = _dataProvider.Experience;
        var gold = _dataProvider.Gold;
        var location = _dataProvider.Location;
        
        var hpWarning = IsLowHp() ? " ⚠" : "";
        
        var parts = new List<string>
        {
            $"Health: {health.Current}/{health.Max}{hpWarning}"
        };
        
        if (resource.HasValue)
        {
            parts.Add($"{resource.Value.Name}: {resource.Value.Current}/{resource.Value.Max}");
        }
        
        parts.Add($"XP: {xp.Current}/{xp.ToNext}");
        parts.Add($"Gold: {gold}");
        parts.Add($"Location: {location}");
        
        return string.Join(" | ", parts);
    }
    
    /// <summary>
    /// Formats status for combat mode.
    /// </summary>
    private string FormatCombatMode(int width, (int Round, bool IsPlayerTurn) combat)
    {
        var health = _dataProvider.Health;
        var resource = _dataProvider.PrimaryResource;
        var location = _dataProvider.Location;
        
        var hpWarning = IsLowHp() ? " ⚠" : "";
        var turnIndicator = combat.IsPlayerTurn ? "YOUR TURN" : "ENEMY TURN";
        
        var parts = new List<string>
        {
            $"HP:{health.Current}/{health.Max}{hpWarning}"
        };
        
        if (resource.HasValue)
        {
            parts.Add($"{GetResourceAbbrev(resource.Value.Name)}:{resource.Value.Current}/{resource.Value.Max}");
        }
        
        parts.Add($"Round {combat.Round}");
        parts.Add(turnIndicator);
        parts.Add(TruncateLocation(location, 20));
        
        return string.Join(" | ", parts);
    }
    
    /// <summary>
    /// Checks if player HP is below the low threshold.
    /// </summary>
    public bool IsLowHp()
    {
        var health = _dataProvider.Health;
        return health.Max > 0 && 
               (double)health.Current / health.Max <= LowHpThreshold;
    }
    
    /// <summary>
    /// Gets abbreviated resource name for compact display.
    /// </summary>
    public static string GetResourceAbbrev(string resourceName)
    {
        return resourceName.ToUpperInvariant() switch
        {
            "MANA" => "MP",
            "RAGE" => "RG",
            "ENERGY" => "EN",
            "STAMINA" => "ST",
            "FOCUS" => "FO",
            _ => resourceName.Length >= 2 
                ? resourceName[..Math.Min(2, resourceName.Length)].ToUpperInvariant()
                : resourceName.ToUpperInvariant()
        };
    }
    
    /// <summary>
    /// Truncates location name to fit available space.
    /// </summary>
    public static string TruncateLocation(string location, int maxLength)
    {
        if (maxLength <= 0) return "";
        if (location.Length <= maxLength) return location;
        if (maxLength <= 3) return location[..maxLength];
        return location[..(maxLength - 3)] + "...";
    }
    
    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _dataProvider.OnDataChanged -= Refresh;
        GC.SuppressFinalize(this);
    }
}
