namespace RuneAndRust.Presentation.Gui.Services;

using Avalonia.Controls;

/// <summary>
/// Manages panel regions in the game window.
/// </summary>
/// <remarks>
/// Provides methods for registering, unregistering, and controlling
/// the visibility of panels within the defined layout regions.
/// </remarks>
public interface IWindowLayoutService
{
    /// <summary>
    /// Registers a panel control in a specific region.
    /// </summary>
    /// <param name="region">The region to register the panel in.</param>
    /// <param name="panel">The control to register.</param>
    /// <remarks>
    /// If a panel is already registered in the region, it will be replaced.
    /// </remarks>
    void RegisterPanel(PanelRegion region, Control panel);

    /// <summary>
    /// Unregisters a panel from a specific region.
    /// </summary>
    /// <param name="region">The region to unregister.</param>
    void UnregisterPanel(PanelRegion region);

    /// <summary>
    /// Gets the panel registered in a specific region.
    /// </summary>
    /// <param name="region">The region to query.</param>
    /// <returns>The registered panel, or null if no panel is registered.</returns>
    Control? GetPanel(PanelRegion region);

    /// <summary>
    /// Sets whether a panel region is visible.
    /// </summary>
    /// <param name="region">The region to modify.</param>
    /// <param name="visible">True to show the panel, false to hide it.</param>
    void SetPanelVisible(PanelRegion region, bool visible);

    /// <summary>
    /// Event raised when layout changes occur.
    /// </summary>
    /// <remarks>
    /// Fired when a panel is registered, unregistered, or visibility changes.
    /// </remarks>
    event Action<PanelRegion>? OnLayoutChanged;
}
