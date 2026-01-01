using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using Spectre.Console;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Static helper for rendering Flux-related UI elements in the combat HUD.
/// Provides visual feedback for environmental aetheric volatility.
/// </summary>
/// <remarks>
/// See: v0.4.3e (The Resonance) for implementation details.
/// Uses distinct bar character (▓) to differentiate from HP/Stamina bars (█).
/// </remarks>
public static class FluxWidget
{
    /// <summary>
    /// Renders a Flux progress bar with threshold indicator.
    /// </summary>
    /// <param name="flux">Current flux level (0-100).</param>
    /// <param name="themeService">Theme service for color lookup.</param>
    /// <param name="width">Width of the bar in characters (default 10).</param>
    /// <returns>Markup string with colored bar and threshold label.</returns>
    public static string RenderFluxBar(int flux, IThemeService themeService, int width = 10)
    {
        // Calculate fill
        var pct = Math.Clamp(flux / 100.0, 0, 1);
        var filled = (int)(pct * width);
        var empty = width - filled;

        // Build bar with distinct character for flux
        var bar = new string('▓', filled) + new string('░', empty);

        // Get color based on threshold
        var color = GetFluxColor(flux, themeService);

        // Get threshold label
        var (threshold, thresholdColor) = GetThresholdLabel(flux, themeService);

        return $"[{color.ToMarkup()}]{bar}[/] {flux} [{thresholdColor.ToMarkup()}]{threshold}[/]";
    }

    /// <summary>
    /// Gets the color for the Flux display based on current level.
    /// </summary>
    /// <param name="flux">Current flux level (0-100).</param>
    /// <param name="themeService">Theme service for color lookup.</param>
    /// <returns>The appropriate color for the flux level.</returns>
    public static Color GetFluxColor(int flux, IThemeService themeService)
    {
        var role = flux switch
        {
            >= 75 => "FluxOverload",   // Bright red
            >= 50 => "FluxCritical",   // Red
            >= 25 => "FluxElevated",   // Yellow
            _ => "FluxSafe"            // Green
        };

        return ParseColor(themeService.GetColor(role));
    }

    /// <summary>
    /// Gets the threshold label and its color.
    /// </summary>
    /// <param name="flux">Current flux level (0-100).</param>
    /// <param name="themeService">Theme service for color lookup.</param>
    /// <returns>A tuple of (label text, label color).</returns>
    public static (string Label, Color Color) GetThresholdLabel(int flux, IThemeService themeService)
    {
        return flux switch
        {
            >= 75 => ("[OVERLOAD]", ParseColor(themeService.GetColor("FluxOverload"))),
            >= 50 => ("[CRITICAL]", ParseColor(themeService.GetColor("FluxCritical"))),
            >= 25 => ("[ELEVATED]", ParseColor(themeService.GetColor("FluxElevated"))),
            _ => ("[SAFE]", ParseColor(themeService.GetColor("FluxSafe")))
        };
    }

    /// <summary>
    /// Gets the FluxThreshold enum value for a given flux level.
    /// </summary>
    /// <param name="flux">Current flux level (0-100).</param>
    /// <returns>The corresponding FluxThreshold enum value.</returns>
    public static FluxThreshold GetThreshold(int flux)
    {
        return flux switch
        {
            >= 75 => FluxThreshold.Overload,
            >= 50 => FluxThreshold.Critical,
            >= 25 => FluxThreshold.Elevated,
            _ => FluxThreshold.Safe
        };
    }

    /// <summary>
    /// Renders a compact Flux indicator for tight spaces.
    /// </summary>
    /// <param name="flux">Current flux level (0-100).</param>
    /// <param name="themeService">Theme service for color lookup.</param>
    /// <returns>Compact markup string like "Flux:55!!" for elevated levels.</returns>
    public static string RenderCompact(int flux, IThemeService themeService)
    {
        var color = GetFluxColor(flux, themeService);
        var threshold = GetThreshold(flux);
        var indicator = threshold switch
        {
            FluxThreshold.Overload => "!!!",
            FluxThreshold.Critical => "!!",
            FluxThreshold.Elevated => "!",
            _ => ""
        };

        return $"[{color.ToMarkup()}]Flux:{flux}{indicator}[/]";
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
            "red" => Color.Red,
            "red1" => Color.Red1,
            "green" => Color.Green,
            "lime" => Color.Lime,
            "limegreen" => Color.Green,
            "cyan" => Color.Cyan1,
            "cyan1" => Color.Cyan1,
            "yellow" => Color.Yellow,
            "gold" => Color.Gold1,
            "gold1" => Color.Gold1,
            "orange" => Color.Orange1,
            "orange1" => Color.Orange1,
            "tomato" => Color.Red,
            "grey" => Color.Grey,
            "gray" => Color.Grey,
            _ => Color.Grey
        };
    }
}
