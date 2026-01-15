namespace RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// Types of messages that determine color coding.
/// </summary>
public enum MessageType
{
    /// <summary>Default text color.</summary>
    Default,
    
    /// <summary>Informational message (cyan).</summary>
    Info,
    
    /// <summary>Warning message (yellow).</summary>
    Warning,
    
    /// <summary>Error message (red).</summary>
    Error,
    
    /// <summary>Combat hit (orange-red).</summary>
    CombatHit,
    
    /// <summary>Combat miss (gray).</summary>
    CombatMiss,
    
    /// <summary>Combat heal (lime green).</summary>
    CombatHeal,
    
    /// <summary>Combat critical hit (magenta).</summary>
    CombatCritical,
    
    /// <summary>Common loot (white).</summary>
    LootCommon,
    
    /// <summary>Uncommon loot (lime green).</summary>
    LootUncommon,
    
    /// <summary>Rare loot (royal blue).</summary>
    LootRare,
    
    /// <summary>Epic loot (purple).</summary>
    LootEpic,
    
    /// <summary>Legendary loot (gold).</summary>
    LootLegendary,
    
    /// <summary>NPC dialogue (yellow).</summary>
    Dialogue,
    
    /// <summary>Success message (lime green).</summary>
    Success,
    
    /// <summary>Failure message (orange-red).</summary>
    Failure
}
