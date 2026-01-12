using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of a quest failure.
/// </summary>
public readonly record struct QuestFailureResult
{
    /// <summary>
    /// Gets the quest ID that failed.
    /// </summary>
    public string QuestId { get; init; }

    /// <summary>
    /// Gets the quest name.
    /// </summary>
    public string QuestName { get; init; }

    /// <summary>
    /// Gets the failure reason.
    /// </summary>
    public string Reason { get; init; }

    /// <summary>
    /// Gets the failure type.
    /// </summary>
    public FailureType ConditionType { get; init; }

    /// <summary>
    /// Gets the time of failure.
    /// </summary>
    public DateTime FailedAt { get; init; }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    public static QuestFailureResult Create(
        string questId,
        string questName,
        string reason,
        FailureType conditionType) => new()
    {
        QuestId = questId,
        QuestName = questName,
        Reason = reason,
        ConditionType = conditionType,
        FailedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Creates a time expired failure result.
    /// </summary>
    public static QuestFailureResult TimeExpired(string questId, string questName) =>
        Create(questId, questName, "Time has run out.", FailureType.TimeExpired);
}
