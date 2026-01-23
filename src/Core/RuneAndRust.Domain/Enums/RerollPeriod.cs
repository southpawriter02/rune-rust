namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the time period after which a re-roll ability can be used again.
/// </summary>
/// <remarks>
/// Used by <see cref="MasterAbilityType.RerollFailure"/> abilities to track
/// how often the re-roll can be invoked.
/// </remarks>
public enum RerollPeriod
{
    /// <summary>
    /// Re-roll refreshes after the current conversation or dialogue ends.
    /// </summary>
    /// <example>Master Negotiator: Once per conversation.</example>
    Conversation = 0,

    /// <summary>
    /// Re-roll refreshes after the current scene or encounter ends.
    /// </summary>
    /// <example>A stealth re-roll that refreshes after leaving the area.</example>
    Scene = 1,

    /// <summary>
    /// Re-roll refreshes after an in-game day passes.
    /// </summary>
    /// <example>A powerful re-roll limited to once per day.</example>
    Day = 2,

    /// <summary>
    /// Re-roll refreshes after a long rest.
    /// </summary>
    /// <example>An exhausting ability requiring rest to recover.</example>
    Rest = 3
}
