namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of hazard triggers.
/// </summary>
public enum HazardTriggerType
{
    /// <summary>
    /// Triggers when a player enters the room.
    /// </summary>
    OnEnter,

    /// <summary>
    /// Triggers each turn while in the room.
    /// </summary>
    PerTurn,

    /// <summary>
    /// Triggers on specific player actions.
    /// </summary>
    OnAction,

    /// <summary>
    /// Triggers based on conditional requirements.
    /// </summary>
    Conditional,

    /// <summary>
    /// Always active environmental effect.
    /// </summary>
    Ambient
}
