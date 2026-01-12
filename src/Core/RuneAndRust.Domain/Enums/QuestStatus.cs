namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Status of a quest.
/// </summary>
public enum QuestStatus
{
    /// <summary>Quest is available to accept.</summary>
    Available = 0,

    /// <summary>Quest is currently active.</summary>
    Active = 1,

    /// <summary>Quest has been completed successfully.</summary>
    Completed = 2,

    /// <summary>Quest has failed.</summary>
    Failed = 3
}
