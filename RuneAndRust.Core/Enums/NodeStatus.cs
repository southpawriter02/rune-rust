namespace RuneAndRust.Core.Enums;

/// <summary>
/// Visual status of a specialization node in the UI.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for Specialization UI implementation.</remarks>
public enum NodeStatus
{
    /// <summary>
    /// Node is locked - prerequisites not met.
    /// </summary>
    Locked = 0,

    /// <summary>
    /// Node is available for purchase - has PP and prerequisites met.
    /// </summary>
    Available = 1,

    /// <summary>
    /// Node is unlocked - already purchased.
    /// </summary>
    Unlocked = 2,

    /// <summary>
    /// Node is affordable but insufficient PP (prerequisites met).
    /// </summary>
    Affordable = 3
}
