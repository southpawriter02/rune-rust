namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Defines bar types for color threshold lookup.
/// </summary>
public enum BarType
{
    /// <summary>
    /// Health bar: Green → Yellow → Red → DarkRed
    /// </summary>
    Health,
    
    /// <summary>
    /// Mana bar: Always blue.
    /// </summary>
    Mana,
    
    /// <summary>
    /// Experience bar: Always magenta/purple.
    /// </summary>
    Experience,
    
    /// <summary>
    /// Stamina bar: Yellow → Orange.
    /// </summary>
    Stamina,
    
    /// <summary>
    /// Custom bar type with user-defined colors.
    /// </summary>
    Custom
}
