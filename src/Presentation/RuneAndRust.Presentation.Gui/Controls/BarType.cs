namespace RuneAndRust.Presentation.Gui.Controls;

/// <summary>
/// Types of resource bars for the HealthBarControl.
/// </summary>
public enum BarType
{
    /// <summary>Health/HP bar with color thresholds.</summary>
    Health,
    
    /// <summary>Mana/MP bar (blue).</summary>
    Mana,
    
    /// <summary>Experience/XP bar (purple).</summary>
    Experience,
    
    /// <summary>Stamina bar (orange).</summary>
    Stamina,
    
    /// <summary>Custom bar with default color.</summary>
    Custom
}
