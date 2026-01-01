using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Static helper for rendering Corruption-related UI elements in the combat HUD.
/// Provides visual feedback for the caster's soul degradation.
/// </summary>
/// <remarks>
/// See: v0.4.3e (The Resonance) for implementation details.
/// Corruption accumulates from Catastrophic backlash and represents permanent soul damage.
/// </remarks>
public static class CorruptionWidget
{
    /// <summary>
    /// Renders a corruption indicator with level label.
    /// </summary>
    /// <param name="corruption">Current corruption value (0-100).</param>
    /// <param name="level">The corruption level tier.</param>
    /// <param name="themeService">Theme service for color lookup.</param>
    /// <returns>Markup string with colored bar and level label.</returns>
    public static string RenderIndicator(int corruption, CorruptionLevel level, IThemeService themeService)
    {
        // Build bar (0-100 scale)
        var width = 10;
        var pct = Math.Clamp(corruption / 100.0, 0, 1);
        var filled = (int)(pct * width);
        var empty = width - filled;
        var bar = new string('█', filled) + new string('░', empty);

        // Get color
        var color = GetCorruptionColor(level, themeService);

        // Get label
        var label = GetCorruptionLabel(level);

        return $"[{color.ToMarkup()}]{bar}[/] {corruption} [{color.ToMarkup()}][{label}][/]";
    }

    /// <summary>
    /// Gets the color for corruption display based on level.
    /// </summary>
    /// <param name="level">The corruption level tier.</param>
    /// <param name="themeService">Theme service for color lookup.</param>
    /// <returns>The appropriate color for the corruption level.</returns>
    public static Color GetCorruptionColor(CorruptionLevel level, IThemeService themeService)
    {
        var role = level switch
        {
            CorruptionLevel.Lost => "CorruptionLost",
            CorruptionLevel.Blighted => "CorruptionBlighted",
            CorruptionLevel.Afflicted => "CorruptionAfflicted",
            CorruptionLevel.Tainted => "CorruptionTainted",
            _ => "CorruptionNone"
        };

        return ParseColor(themeService.GetColor(role));
    }

    /// <summary>
    /// Gets the display label for a corruption level.
    /// </summary>
    /// <param name="level">The corruption level tier.</param>
    /// <returns>Human-readable label for the corruption level.</returns>
    public static string GetCorruptionLabel(CorruptionLevel level)
    {
        return level switch
        {
            CorruptionLevel.Lost => "LOST",
            CorruptionLevel.Blighted => "Blighted",
            CorruptionLevel.Afflicted => "Afflicted",
            CorruptionLevel.Tainted => "Tainted",
            _ => "Untouched"
        };
    }

    /// <summary>
    /// Renders a compact corruption indicator for tight spaces.
    /// Returns empty string if corruption is at Untouched level.
    /// </summary>
    /// <param name="corruption">Current corruption value (0-100).</param>
    /// <param name="level">The corruption level tier.</param>
    /// <param name="themeService">Theme service for color lookup.</param>
    /// <returns>Compact markup string, or empty if Untouched.</returns>
    public static string RenderCompact(int corruption, CorruptionLevel level, IThemeService themeService)
    {
        // Don't show if pristine
        if (level == CorruptionLevel.Untouched)
            return string.Empty;

        var color = GetCorruptionColor(level, themeService);
        return $"[{color.ToMarkup()}]{GetCorruptionSymbol(level)} {corruption}[/]";
    }

    /// <summary>
    /// Gets the symbol used for compact corruption display.
    /// </summary>
    private static string GetCorruptionSymbol(CorruptionLevel level)
    {
        return level switch
        {
            CorruptionLevel.Lost => "☠☠",
            CorruptionLevel.Blighted => "☠",
            _ => "☣"
        };
    }

    /// <summary>
    /// Parses a color string to a Spectre.Console Color object.
    /// </summary>
    private static Color ParseColor(string colorString)
    {
        // Strip "bold " prefix if present
        var cleanColor = colorString.StartsWith("bold ", StringComparison.OrdinalIgnoreCase)
            ? colorString[5..]
            : colorString;

        return cleanColor.ToLowerInvariant() switch
        {
            "purple" => Color.Purple,
            "mediumpurple" => Color.MediumPurple,
            "darkmagenta" => Color.Magenta1,
            "magenta" => Color.Magenta1,
            "magenta1" => Color.Magenta1,
            "indigo" => Color.Purple4,
            "black" => Color.Black,
            "grey" => Color.Grey,
            "gray" => Color.Grey,
            _ => Color.Grey
        };
    }
}
