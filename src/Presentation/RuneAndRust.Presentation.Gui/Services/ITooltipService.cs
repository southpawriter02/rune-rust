namespace RuneAndRust.Presentation.Gui.Services;

using RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// Manages tooltip display throughout the application.
/// </summary>
public interface ITooltipService
{
    /// <summary>Gets or sets the delay in milliseconds before showing tooltips.</summary>
    int ShowDelayMs { get; set; }

    /// <summary>Gets or sets whether tooltips are enabled.</summary>
    bool IsEnabled { get; set; }

    /// <summary>Shows a simple text tooltip.</summary>
    void ShowTextTooltip(string text);

    /// <summary>Shows a rich content tooltip.</summary>
    void ShowTooltip(TooltipContent content);

    /// <summary>Hides the current tooltip.</summary>
    void HideTooltip();

    /// <summary>Gets the current tooltip content.</summary>
    TooltipContent? CurrentContent { get; }

    /// <summary>Event fired when tooltip content changes.</summary>
    event Action<TooltipContent?>? OnTooltipChanged;
}
