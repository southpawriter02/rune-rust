namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines how a status effect's duration is tracked.
/// </summary>
public enum DurationType
{
    /// <summary>
    /// Effect lasts a fixed number of turns.
    /// Duration decrements at the start of the affected entity's turn.
    /// </summary>
    Turns,

    /// <summary>
    /// Effect lasts until explicitly removed.
    /// Used for persistent conditions like On Fire.
    /// </summary>
    Permanent,

    /// <summary>
    /// Effect lasts until a specific trigger occurs.
    /// Example: Knocked Down removed when taking Stand action.
    /// </summary>
    Triggered,

    /// <summary>
    /// Effect lasts until its resource pool is depleted.
    /// Example: Shielded absorbs damage until shield HP is gone.
    /// </summary>
    ResourceBased
}
