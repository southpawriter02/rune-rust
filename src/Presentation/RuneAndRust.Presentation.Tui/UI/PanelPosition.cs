namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Defines standard panel positions in the screen layout.
/// </summary>
public enum PanelPosition
{
    /// <summary>
    /// Primary content area (left/center).
    /// Used for room descriptions, combat log, and main game messages.
    /// </summary>
    MainContent,
    
    /// <summary>
    /// Right sidebar for status information.
    /// Used for player stats, location info.
    /// </summary>
    Sidebar,
    
    /// <summary>
    /// Bottom status bar.
    /// Used for HP, MP, XP, gold display.
    /// </summary>
    Footer,
    
    /// <summary>
    /// Command input area.
    /// Used for player command entry.
    /// </summary>
    Input,
    
    /// <summary>
    /// Modal/overlay panels.
    /// Used for dialogs, menus, popups.
    /// </summary>
    Popup
}
