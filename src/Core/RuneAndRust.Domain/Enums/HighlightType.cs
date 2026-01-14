namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of cell highlighting for combat grid.
/// </summary>
public enum HighlightType
{
    /// <summary>Blue - Valid movement targets.</summary>
    Movement,
    
    /// <summary>Red - Valid attack targets.</summary>
    Attack,
    
    /// <summary>Purple - Ability effect range.</summary>
    Ability,
    
    /// <summary>Orange - Threatened squares (opportunity attacks).</summary>
    Threatened,
    
    /// <summary>Yellow - Current selection.</summary>
    Selected
}
