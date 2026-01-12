using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for quest failure handling.
/// </summary>
public class QuestFailureService : IQuestFailureService
{
    private readonly ILogger<QuestFailureService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    public QuestFailureService(
        ILogger<QuestFailureService>? logger = null,
        IGameEventLogger? eventLogger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<QuestFailureService>.Instance;
        _eventLogger = eventLogger;
    }

    /// <inheritdoc/>
    public IReadOnlyList<QuestFailureResult> ProcessQuestTimers(IEnumerable<TimedQuestInfo> timedQuests)
    {
        var failures = new List<QuestFailureResult>();

        foreach (var quest in timedQuests)
        {
            if (quest.IsExpired)
            {
                _logger.LogInformation("Quest {QuestId} expired", quest.QuestId);

                _eventLogger?.LogQuest("QuestExpired", $"Quest '{quest.QuestName}' expired",
                    data: new Dictionary<string, object>
                    {
                        ["questId"] = quest.QuestId,
                        ["questName"] = quest.QuestName
                    });

                failures.Add(QuestFailureResult.TimeExpired(quest.QuestId, quest.QuestName));
            }
        }

        return failures;
    }

    /// <inheritdoc/>
    public FailureCondition? CheckFailureConditions(
        IEnumerable<FailureCondition> conditions,
        FailureCheckContext context)
    {
        foreach (var condition in conditions)
        {
            if (EvaluateCondition(condition, context))
            {
                _logger.LogDebug("Failure condition triggered: {Type}", condition.Type);

                _eventLogger?.LogQuest("FailureCondition", $"Quest failure condition triggered: {condition.Type}",
                    data: new Dictionary<string, object>
                    {
                        ["conditionType"] = condition.Type.ToString(),
                        ["targetId"] = condition.TargetId ?? "none"
                    });

                return condition;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public QuestFailureResult CreateFailureResult(
        string questId,
        string questName,
        FailureCondition condition) =>
        QuestFailureResult.Create(questId, questName, condition.Message, condition.Type);

    private bool EvaluateCondition(FailureCondition condition, FailureCheckContext context)
    {
        return condition.Type switch
        {
            FailureType.NPCDied => EvaluateNPCDied(condition, context),
            FailureType.ItemLost => EvaluateItemLost(condition, context),
            FailureType.ReputationDropped => EvaluateReputationDropped(condition, context),
            FailureType.LeftArea => EvaluateLeftArea(condition, context),
            FailureType.TimeExpired => false, // Handled separately
            FailureType.Custom => false, // Not implemented
            _ => false
        };
    }

    private bool EvaluateNPCDied(FailureCondition condition, FailureCheckContext context)
    {
        if (string.IsNullOrEmpty(condition.TargetId)) return false;
        return context.DeadNpcIds.Contains(condition.TargetId);
    }

    private bool EvaluateItemLost(FailureCondition condition, FailureCheckContext context)
    {
        if (string.IsNullOrEmpty(condition.TargetId)) return false;
        return !context.InventoryItemIds.Contains(condition.TargetId);
    }

    private bool EvaluateReputationDropped(FailureCondition condition, FailureCheckContext context)
    {
        if (string.IsNullOrEmpty(condition.TargetId) || !condition.Threshold.HasValue)
            return false;

        if (context.FactionReputations.TryGetValue(condition.TargetId, out var rep))
            return rep < condition.Threshold.Value;

        return false;
    }

    private bool EvaluateLeftArea(FailureCondition condition, FailureCheckContext context)
    {
        if (string.IsNullOrEmpty(condition.TargetId)) return false;
        return !string.Equals(context.CurrentAreaId, condition.TargetId, StringComparison.OrdinalIgnoreCase);
    }
}
