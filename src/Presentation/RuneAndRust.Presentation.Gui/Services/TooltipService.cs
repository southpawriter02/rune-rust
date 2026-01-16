namespace RuneAndRust.Presentation.Gui.Services;

using RuneAndRust.Presentation.Gui.Models;
using Serilog;

/// <summary>
/// Implementation of the tooltip service with delayed appearance.
/// </summary>
public class TooltipService : ITooltipService
{
    private CancellationTokenSource? _showCts;

    /// <inheritdoc />
    public int ShowDelayMs { get; set; } = 500;

    /// <inheritdoc />
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc />
    public TooltipContent? CurrentContent { get; private set; }

    /// <inheritdoc />
    public event Action<TooltipContent?>? OnTooltipChanged;

    /// <inheritdoc />
    public void ShowTextTooltip(string text)
    {
        if (!IsEnabled) return;
        ShowTooltip(TooltipContent.Simple(text));
    }

    /// <inheritdoc />
    public void ShowTooltip(TooltipContent content)
    {
        if (!IsEnabled) return;
        ScheduleShow(content);
    }

    /// <inheritdoc />
    public void HideTooltip()
    {
        _showCts?.Cancel();
        _showCts = null;
        CurrentContent = null;
        OnTooltipChanged?.Invoke(null);
        Log.Debug("Tooltip hidden");
    }

    private async void ScheduleShow(TooltipContent content)
    {
        _showCts?.Cancel();
        _showCts = new CancellationTokenSource();

        try
        {
            if (ShowDelayMs > 0)
                await Task.Delay(ShowDelayMs, _showCts.Token);

            CurrentContent = content;
            OnTooltipChanged?.Invoke(content);
            Log.Debug("Showing tooltip: {Title}", content.Title);
        }
        catch (TaskCanceledException)
        {
            // Tooltip was cancelled before showing
        }
    }
}
