using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Configuration;

namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Renders visual health and resource bars.
/// </summary>
/// <remarks>
/// Supports multiple display styles and color thresholds for danger indication.
/// Uses Unicode characters (█░) with ASCII fallback (#-).
/// </remarks>
public class HealthBar
{
    private readonly ITerminalService _terminal;
    private readonly HealthBarConfig _config;
    private readonly ILogger<HealthBar>? _logger;
    
    /// <summary>
    /// Initializes a new instance of <see cref="HealthBar"/>.
    /// </summary>
    /// <param name="terminal">Terminal service for output.</param>
    /// <param name="config">Optional configuration (uses defaults if null).</param>
    /// <param name="logger">Optional logger.</param>
    public HealthBar(
        ITerminalService terminal,
        HealthBarConfig? config = null,
        ILogger<HealthBar>? logger = null)
    {
        _terminal = terminal;
        _config = config ?? HealthBarConfig.CreateDefault();
        _logger = logger;
    }
    
    /// <summary>
    /// Renders a bar to a string.
    /// </summary>
    /// <param name="current">Current value.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="width">Bar width in characters.</param>
    /// <param name="style">Display style.</param>
    /// <returns>Rendered bar string.</returns>
    public string Render(int current, int max, int width, BarStyle style = BarStyle.Standard)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(current);
        ArgumentOutOfRangeException.ThrowIfLessThan(max, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 3);
        
        current = Math.Min(current, max);
        var percentage = (double)current / max;
        
        return style switch
        {
            BarStyle.Standard => RenderStandard(percentage, width),
            BarStyle.Detailed => RenderDetailed(percentage, width, current, max),
            BarStyle.Compact => RenderCompact(percentage, width, current),
            BarStyle.Numeric => $"{current}/{max}",
            _ => RenderStandard(percentage, width)
        };
    }
    
    /// <summary>
    /// Gets the color based on percentage and bar type.
    /// </summary>
    /// <param name="current">Current value.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="type">Bar type for threshold lookup.</param>
    /// <returns>ConsoleColor for the bar.</returns>
    public ConsoleColor GetThresholdColor(int current, int max, BarType type)
    {
        if (max <= 0) return ConsoleColor.Gray;
        
        var percentage = (int)((double)current / max * 100);
        var thresholds = _config.GetThresholds(type);
        
        // Iterate from lowest threshold up to find the first matching threshold
        // where the percentage is at or below that threshold
        foreach (var threshold in thresholds.OrderBy(t => t.Percent))
        {
            if (percentage <= threshold.Percent)
            {
                _logger?.LogDebug("Threshold color for {Type} at {Percent}%: {Color}", 
                    type, percentage, threshold.Color);
                return threshold.Color;
            }
        }
        
        // If above all thresholds, use the highest threshold color
        return thresholds.OrderByDescending(t => t.Percent).FirstOrDefault()?.Color ?? ConsoleColor.Gray;
    }
    
    /// <summary>
    /// Renders a labeled health bar.
    /// </summary>
    /// <param name="label">Label text (e.g., "HP").</param>
    /// <param name="current">Current value.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="totalWidth">Total width including label.</param>
    /// <returns>Formatted bar with label.</returns>
    public string RenderLabeled(string label, int current, int max, int totalWidth)
    {
        var labelPart = $"{label}: ";
        var valuePart = $" {current}/{max}";
        var barWidth = totalWidth - labelPart.Length - valuePart.Length;
        
        if (barWidth < 5)
        {
            // Not enough room for bar, show numeric only
            return $"{label}: {current}/{max}";
        }
        
        var bar = Render(current, max, barWidth, BarStyle.Standard);
        return $"{labelPart}{bar}{valuePart}";
    }
    
    /// <summary>
    /// Renders a colored health bar to the terminal.
    /// </summary>
    /// <param name="x">X position.</param>
    /// <param name="y">Y position.</param>
    /// <param name="current">Current value.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="width">Bar width.</param>
    /// <param name="type">Bar type for color.</param>
    public void RenderColored(int x, int y, int current, int max, int width, BarType type)
    {
        var bar = Render(current, max, width, BarStyle.Standard);
        var color = GetThresholdColor(current, max, type);
        
        _terminal.SetCursorPosition(x, y);
        
        var previousColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            _terminal.Write(bar);
        }
        finally
        {
            Console.ForegroundColor = previousColor;
        }
    }
    
    private string RenderStandard(double percentage, int width)
    {
        var filledWidth = (int)(percentage * width);
        var emptyWidth = width - filledWidth;
        
        var filled = GetFilledChar();
        var empty = GetEmptyChar();
        
        return new string(filled, filledWidth) + new string(empty, emptyWidth);
    }
    
    private string RenderDetailed(double percentage, int width, int current, int max)
    {
        var innerWidth = Math.Max(3, width - 2); // Account for brackets
        var bar = RenderStandard(percentage, innerWidth);
        return $"[{bar}] {current}/{max}";
    }
    
    private string RenderCompact(double percentage, int width, int current)
    {
        var valueStr = current.ToString();
        var barWidth = Math.Max(3, width - valueStr.Length);
        var bar = RenderStandard(percentage, barWidth);
        return $"{bar}{valueStr}";
    }
    
    private char GetFilledChar()
    {
        return _terminal.SupportsUnicode 
            ? _config.Characters.Filled 
            : _config.Characters.FilledFallback;
    }
    
    private char GetEmptyChar()
    {
        return _terminal.SupportsUnicode 
            ? _config.Characters.Empty 
            : _config.Characters.EmptyFallback;
    }
}
