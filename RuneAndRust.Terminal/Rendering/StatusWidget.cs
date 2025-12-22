using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Static helper for rendering colored resource bars in the exploration HUD (v0.3.5a).
/// Provides color-coding logic and Unicode block-based progress bars.
/// </summary>
public static class StatusWidget
{
    /// <summary>
    /// Gets the color for HP based on percentage thresholds.
    /// </summary>
    /// <remarks>
    /// 4-tier system:
    /// - Green (>75%): Healthy
    /// - Yellow (50-75%): Wounded
    /// - Orange (25-49%): Danger
    /// - Red (≤25%): Critical
    /// </remarks>
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
    /// Gets the color for Stamina based on percentage thresholds.
    /// </summary>
    /// <remarks>
    /// 2-tier system:
    /// - Cyan (≥20%): Active
    /// - Grey (<20%): Exhausted
    /// </remarks>
    public static Color GetStaminaColor(int current, int max)
    {
        if (max == 0) return Color.Grey;

        var pct = (double)current / max * 100;
        return pct < 20 ? Color.Grey : Color.Cyan1;
    }

    /// <summary>
    /// Gets the color for Stress based on absolute value thresholds.
    /// </summary>
    /// <remarks>
    /// 5-tier system:
    /// - Grey (<20): Stable
    /// - Cyan (20-39): Unsettled
    /// - Yellow (40-59): Shaken
    /// - Magenta (60-79): Distressed
    /// - Purple (≥80): Fractured
    /// </remarks>
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
}
