namespace RuneAndRust.Presentation.Gui.Services;

/// <summary>
/// Defines the regions available in the game window layout.
/// </summary>
/// <remarks>
/// Each region corresponds to a specific area of the game window
/// where panels can be registered and managed dynamically.
/// </remarks>
public enum PanelRegion
{
    /// <summary>
    /// Main content area for room display and primary game content.
    /// </summary>
    MainContent,
    
    /// <summary>
    /// Bottom content area for message log and game output.
    /// </summary>
    BottomContent,
    
    /// <summary>
    /// Right sidebar top area for player status display.
    /// </summary>
    RightSidebarTop,
    
    /// <summary>
    /// Right sidebar bottom area for inventory panel.
    /// </summary>
    RightSidebarBottom,
    
    /// <summary>
    /// Input area at bottom of window for command entry.
    /// </summary>
    Input,
    
    /// <summary>
    /// Combat overlay region that appears during combat.
    /// This overlays the main content area when active.
    /// </summary>
    CombatOverlay
}
