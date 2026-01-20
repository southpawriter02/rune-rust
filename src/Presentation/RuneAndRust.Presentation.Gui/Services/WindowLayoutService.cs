namespace RuneAndRust.Presentation.Gui.Services;

using Avalonia.Controls;
using Serilog;

/// <summary>
/// Implementation of window layout management.
/// </summary>
/// <remarks>
/// Manages panel registration, visibility, and layout change notifications
/// for the game window's defined regions.
/// </remarks>
public class WindowLayoutService : IWindowLayoutService
{
    private readonly Dictionary<PanelRegion, Control> _panels = new();

    /// <inheritdoc />
    public event Action<PanelRegion>? OnLayoutChanged;

    /// <inheritdoc />
    public void RegisterPanel(PanelRegion region, Control panel)
    {
        ArgumentNullException.ThrowIfNull(panel);
        
        _panels[region] = panel;
        Log.Debug("Registered panel in region {Region}", region);
        OnLayoutChanged?.Invoke(region);
    }

    /// <inheritdoc />
    public void UnregisterPanel(PanelRegion region)
    {
        if (_panels.Remove(region))
        {
            Log.Debug("Unregistered panel from region {Region}", region);
            OnLayoutChanged?.Invoke(region);
        }
    }

    /// <inheritdoc />
    public Control? GetPanel(PanelRegion region)
    {
        return _panels.TryGetValue(region, out var panel) ? panel : null;
    }

    /// <inheritdoc />
    public void SetPanelVisible(PanelRegion region, bool visible)
    {
        if (_panels.TryGetValue(region, out var panel))
        {
            panel.IsVisible = visible;
            Log.Debug("Set panel {Region} visibility to {Visible}", region, visible);
        }
        else
        {
            Log.Debug("Cannot set visibility for unregistered region {Region}", region);
        }
    }
}
