using RuneAndRust.Core.Interfaces;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Static helper for rendering colored resource bars in the exploration HUD (v0.3.9b).
/// Provides color-coding logic and Unicode block-based progress bars with theme support.
/// </summary>
public static class StatusWidget
{
    /// <summary>
    /// Gets the color for HP based on percentage thresholds using theme service.
    /// </summary>
    /// <remarks>
    /// 4-tier system:
    /// - HealthFull (>75%): Healthy
    /// - HealthHigh (50-75%): Wounded
    /// - HealthLow (25-49%): Danger
    /// - HealthCritical (≤25%): Critical
    /// </remarks>
    public static Color GetHpColor(int current, int max, IThemeService themeService)
    {
        if (max == 0) return ParseColor(themeService.GetColor("NeutralColor"));

        var pct = (double)current / max * 100;
        var role = pct switch
        {
            <= 25 => "HealthCritical",
            <= 50 => "HealthLow",
            <= 75 => "HealthHigh",
            _ => "HealthFull"
        };
        return ParseColor(themeService.GetColor(role));
    }

    /// <summary>
    /// Gets the color for HP based on percentage thresholds (legacy, non-themed).
    /// </summary>
    public static Color GetHpColor(int current, int max)
    {
        if (max == 0) return Color.Grey;

        var pct = (double)current / max * 100;
        return pct switch
        {
            <= 25 => Color.Red1,     // Critical
            <= 50 => Color.Orange1,  // Danger
            <= 75 => Color.Yellow,   // Wounded
            _ => Color.Green         // Healthy
        };
    }

    /// <summary>
    /// Gets the color for Stamina based on percentage thresholds using theme service.
    /// </summary>
    /// <remarks>
    /// 2-tier system:
    /// - StaminaColor (≥20%): Active
    /// - NeutralColor (<20%): Exhausted
    /// </remarks>
    public static Color GetStaminaColor(int current, int max, IThemeService themeService)
    {
        if (max == 0) return ParseColor(themeService.GetColor("NeutralColor"));

        var pct = (double)current / max * 100;
        return pct < 20
            ? ParseColor(themeService.GetColor("NeutralColor"))
            : ParseColor(themeService.GetColor("StaminaColor"));
    }

    /// <summary>
    /// Gets the color for Stamina based on percentage thresholds (legacy, non-themed).
    /// </summary>
    public static Color GetStaminaColor(int current, int max)
    {
        if (max == 0) return Color.Grey;

        var pct = (double)current / max * 100;
        return pct < 20 ? Color.Grey : Color.Cyan1;
    }

    /// <summary>
    /// Gets the color for Stress based on absolute value thresholds using theme service.
    /// </summary>
    /// <remarks>
    /// 5-tier system:
    /// - StressLow (<20): Stable
    /// - StressLow (20-39): Unsettled (using StaminaColor for distinction)
    /// - StressMid (40-59): Shaken
    /// - StressHigh (60-79): Distressed
    /// - StressHigh (≥80): Fractured
    /// </remarks>
    public static Color GetStressColor(int stress, IThemeService themeService)
    {
        var role = stress switch
        {
            >= 80 => "StressHigh",    // Fractured
            >= 60 => "StressHigh",    // Distressed (same color, different intensity handled by UI)
            >= 40 => "StressMid",     // Shaken
            >= 20 => "StaminaColor",  // Unsettled (cyan in standard)
            _ => "StressLow"          // Stable
        };
        return ParseColor(themeService.GetColor(role));
    }

    /// <summary>
    /// Gets the color for Stress based on absolute value thresholds (legacy, non-themed).
    /// </summary>
    public static Color GetStressColor(int stress)
    {
        return stress switch
        {
            >= 80 => Color.Purple,   // Fractured
            >= 60 => Color.Magenta1, // Distressed
            >= 40 => Color.Yellow,   // Shaken
            >= 20 => Color.Cyan1,    // Unsettled
            _ => Color.Grey          // Stable
        };
    }

    /// <summary>
    /// Renders a text-based progress bar using Unicode block characters.
    /// </summary>
    /// <param name="current">Current value.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="width">Width of the bar in characters (default 20).</param>
    /// <returns>A string of filled (█) and empty (░) blocks.</returns>
    public static string RenderBar(int current, int max, int width = 20)
    {
        if (max == 0) return new string('░', width);

        var pct = Math.Clamp((double)current / max, 0, 1);
        var filled = (int)(pct * width);
        var empty = width - filled;

        return new string('█', filled) + new string('░', empty);
    }

    /// <summary>
    /// Converts a Spectre.Console Color to its markup string representation.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The color name for use in Spectre markup.</returns>
    public static string ToMarkup(this Color color)
    {
        // Map common colors to their markup names
        return color.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Gets the color for AP (Aether Points) based on percentage thresholds using theme service.
    /// </summary>
    /// <remarks>
    /// 4-tier system:
    /// - ApFull (>60%): Healthy AP reserves
    /// - ApMedium (40-60%): Mid-range
    /// - ApLow (20-39%): Running low
    /// - ApCritical (≤20%): Nearly depleted
    /// </remarks>
    public static Color GetApColor(int current, int max, IThemeService themeService)
    {
        if (max == 0) return ParseColor(themeService.GetColor("NeutralColor"));

        var pct = (double)current / max * 100;
        var role = pct switch
        {
            <= 20 => "ApCritical",   // Red - nearly depleted
            <= 40 => "ApLow",        // Orange - running low
            <= 60 => "ApMedium",     // Yellow - mid-range
            _ => "ApFull"            // Cyan - healthy
        };

        return ParseColor(themeService.GetColor(role));
    }

    /// <summary>
    /// Renders an AP bar with appropriate styling.
    /// </summary>
    /// <param name="current">Current AP value.</param>
    /// <param name="max">Maximum AP value.</param>
    /// <param name="themeService">Theme service for color lookup.</param>
    /// <param name="width">Width of the bar in characters (default 10).</param>
    /// <returns>Markup string with colored bar and value display.</returns>
    public static string RenderApBar(int current, int max, IThemeService themeService, int width = 10)
    {
        var bar = RenderBar(current, max, width);
        var color = GetApColor(current, max, themeService);
        var dimColor = themeService.GetColor("DimColor");

        return $"[{color.ToMarkup()}]{bar}[/] [{dimColor}]{current}/{max}[/]";
    }

    /// <summary>
    /// Parses a color string to a Spectre.Console Color object.
    /// Handles "bold " prefix by stripping it (boldness is a style, not a color).
    /// </summary>
    private static Color ParseColor(string colorString)
    {
        // Strip "bold " prefix if present - boldness is handled separately in markup
        var cleanColor = colorString.StartsWith("bold ", StringComparison.OrdinalIgnoreCase)
            ? colorString[5..]
            : colorString;

        return cleanColor.ToLowerInvariant() switch
        {
            "red" => Color.Red,
            "red1" => Color.Red1,
            "green" => Color.Green,
            "blue" => Color.Blue,
            "cyan" => Color.Cyan1,
            "cyan1" => Color.Cyan1,
            "yellow" => Color.Yellow,
            "orange1" => Color.Orange1,
            "magenta1" => Color.Magenta1,
            "purple" => Color.Purple,
            "gold1" => Color.Gold1,
            "white" => Color.White,
            "grey" => Color.Grey,
            "gray" => Color.Grey,
            _ => Color.Grey
        };
    }
}
