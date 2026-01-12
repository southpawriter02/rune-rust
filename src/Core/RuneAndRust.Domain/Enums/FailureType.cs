namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of quest failure conditions.
/// </summary>
public enum FailureType
{
    /// <summary>
    /// Quest time limit expired.
    /// </summary>
    TimeExpired,

    /// <summary>
    /// A protected NPC died.
    /// </summary>
    NPCDied,

    /// <summary>
    /// A required item was lost.
    /// </summary>
    ItemLost,

    /// <summary>
    /// Reputation dropped below threshold.
    /// </summary>
    ReputationDropped,

    /// <summary>
    /// Player left the required area.
    /// </summary>
    LeftArea,

    /// <summary>
    /// Custom scripted condition.
    /// </summary>
    Custom
}
