namespace RuneAndRust.Presentation.UI;

/// <summary>
/// Defines display styles for health/resource bars.
/// </summary>
public enum BarStyle
{
    /// <summary>
    /// Simple bar only: ████████░░░░
    /// </summary>
    Standard,
    
    /// <summary>
    /// Bar with brackets and value: [████████░░░░] 80/100
    /// </summary>
    Detailed,
    
    /// <summary>
    /// Short bar with value appended: ████80
    /// </summary>
    Compact,
    
    /// <summary>
    /// Value only, no bar: 80/100
    /// </summary>
    Numeric
}
