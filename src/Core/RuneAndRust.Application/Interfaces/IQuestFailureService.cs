using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for quest failure handling.
/// </summary>
public interface IQuestFailureService
{
    /// <summary>
    /// Processes timed quests and returns any that expired.
    /// </summary>
    IReadOnlyList<QuestFailureResult> ProcessQuestTimers(IEnumerable<TimedQuestInfo> timedQuests);

    /// <summary>
    /// Checks failure conditions for a quest.
    /// </summary>
    FailureCondition? CheckFailureConditions(
        IEnumerable<FailureCondition> conditions,
        FailureCheckContext context);

    /// <summary>
    /// Creates a failure result for a quest.
    /// </summary>
    QuestFailureResult CreateFailureResult(
        string questId,
        string questName,
        FailureCondition condition);
}

/// <summary>
/// Information about a timed quest.
/// </summary>
public record TimedQuestInfo(
    string QuestId,
    string QuestName,
    int TurnsRemaining,
    bool IsExpired);

/// <summary>
/// Context for checking failure conditions.
/// </summary>
public record FailureCheckContext(
    IReadOnlySet<string> DeadNpcIds,
    IReadOnlySet<string> InventoryItemIds,
    IReadOnlyDictionary<string, int> FactionReputations,
    string? CurrentAreaId);
