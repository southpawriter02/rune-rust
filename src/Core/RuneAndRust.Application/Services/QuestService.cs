using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Orchestrates quest lifecycle: acceptance, objective progression, completion, and failure.
/// Scoped service — one instance per game session.
/// </summary>
/// <remarks>
/// <para>
/// QuestService bridges the gap between quest definitions (from IQuestDefinitionProvider)
/// and Quest domain entities. It creates Quest instances from definitions, tracks active
/// quests, advances objectives, and manages completion/failure.
/// </para>
/// <para>
/// The service does NOT own game event processing — that's handled by QuestEventBus,
/// which calls AdvanceObjective() when matching events arrive.
/// </para>
/// </remarks>
public class QuestService : IQuestService
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly IQuestDefinitionProvider _questProvider;
    private readonly IGameEventLogger? _eventLogger;
    private readonly ILogger<QuestService> _logger;

    /// <summary>Active quest instances keyed by runtime GUID.</summary>
    private readonly Dictionary<Guid, Quest> _activeQuests = [];

    /// <summary>Mapping from quest instance GUID to definition ID for quick lookup.</summary>
    private readonly Dictionary<Guid, string> _questDefinitionMap = [];

    /// <summary>
    /// Mapping from (objectiveType, targetId) to list of (questId, objectiveIndex)
    /// for efficient objective routing when events arrive.
    /// </summary>
    private readonly Dictionary<(QuestObjectiveType Type, string TargetId), List<(Guid QuestId, int ObjectiveIndex)>> _objectiveIndex = [];

    /// <summary>Set of completed quest definition IDs for prerequisite checking.</summary>
    private readonly HashSet<string> _completedQuestIds = new(StringComparer.OrdinalIgnoreCase);

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public QuestService(
        IQuestDefinitionProvider questProvider,
        ILogger<QuestService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _questProvider = questProvider ?? throw new ArgumentNullException(nameof(questProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;

        _logger.LogDebug("QuestService initialized");
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public Quest? AcceptQuest(string questId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questId);

        var definition = _questProvider.GetDefinition(questId);
        if (definition == null)
        {
            _logger.LogWarning("Quest definition not found: {QuestId}", questId);
            return null;
        }

        // Check if already active
        if (_activeQuests.Values.Any(q =>
            q.DefinitionId.Equals(questId, StringComparison.OrdinalIgnoreCase) &&
            q.Status == QuestStatus.Active))
        {
            _logger.LogWarning("Quest already active: {QuestId}", questId);
            return null;
        }

        // Create Quest domain entity from definition
        var quest = new Quest(definition.QuestId, definition.Name, definition.Description);

        // Set category
        if (Enum.TryParse<QuestCategory>(definition.Category, ignoreCase: true, out var category))
            quest.SetCategory(category);

        // Set chain
        if (!string.IsNullOrWhiteSpace(definition.ChainId))
            quest.SetChainId(definition.ChainId);

        // Set timing
        if (definition.IsTimed && definition.TimeLimit.HasValue)
            quest.SetTimeLimit(definition.TimeLimit.Value);

        // Set repeatability
        if (definition.IsRepeatable)
            quest.SetRepeatable(definition.RepeatCooldownHours);

        // Add objectives
        for (var i = 0; i < definition.Objectives.Count; i++)
        {
            var objDef = definition.Objectives[i];
            var objective = new QuestObjective(objDef.Description, objDef.RequiredCount);
            quest.AddObjective(objective);

            // Parse objective type and build index
            if (Enum.TryParse<QuestObjectiveType>(objDef.Type, ignoreCase: true, out var objType))
            {
                var key = (objType, objDef.TargetId.ToLowerInvariant());
                if (!_objectiveIndex.TryGetValue(key, out var entries))
                {
                    entries = [];
                    _objectiveIndex[key] = entries;
                }
                entries.Add((quest.Id, i));
            }
            else
            {
                _logger.LogWarning(
                    "Unknown objective type '{Type}' in quest {QuestId}, objective {Index}",
                    objDef.Type, questId, i);
            }
        }

        // Activate the quest
        quest.Activate();

        // Register tracking
        _activeQuests[quest.Id] = quest;
        _questDefinitionMap[quest.Id] = definition.QuestId;

        _eventLogger?.LogQuest("QuestAccepted",
            $"Quest accepted: {quest.Name}",
            data: new Dictionary<string, object>
            {
                ["QuestId"] = quest.DefinitionId,
                ["ObjectiveCount"] = quest.Objectives.Count
            });

        _logger.LogInformation(
            "Quest accepted: {QuestName} ({QuestId}) - {ObjectiveCount} objectives",
            quest.Name, quest.DefinitionId, quest.Objectives.Count);

        return quest;
    }

    /// <inheritdoc />
    public bool AdvanceObjective(QuestObjectiveType type, string targetId, int amount = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);

        var key = (type, targetId.ToLowerInvariant());
        if (!_objectiveIndex.TryGetValue(key, out var entries))
            return false;

        var anyAdvanced = false;

        foreach (var (questId, objectiveIndex) in entries)
        {
            if (!_activeQuests.TryGetValue(questId, out var quest))
                continue;

            if (quest.Status != QuestStatus.Active)
                continue;

            if (objectiveIndex >= quest.Objectives.Count)
                continue;

            var objective = quest.Objectives[objectiveIndex];
            if (objective.IsCompleted)
                continue;

            objective.AdvanceProgress(amount);
            anyAdvanced = true;

            _logger.LogDebug(
                "Quest {QuestName} objective '{Description}' advanced to {Current}/{Required}",
                quest.Name, objective.Description,
                objective.CurrentProgress, objective.RequiredCount);

            _eventLogger?.LogQuest("ObjectiveAdvanced",
                $"Objective advanced: {objective.Description} ({objective.CurrentProgress}/{objective.RequiredCount})",
                data: new Dictionary<string, object>
                {
                    ["QuestId"] = quest.DefinitionId,
                    ["ObjectiveDescription"] = objective.Description,
                    ["Progress"] = objective.CurrentProgress,
                    ["Required"] = objective.RequiredCount,
                    ["Completed"] = objective.IsCompleted
                });
        }

        return anyAdvanced;
    }

    /// <inheritdoc />
    public bool CheckCompletion(Guid questId)
    {
        if (!_activeQuests.TryGetValue(questId, out var quest))
            return false;

        return quest.AreAllObjectivesCompleted();
    }

    /// <inheritdoc />
    public QuestRewardResult CompleteQuest(Guid questId)
    {
        if (!_activeQuests.TryGetValue(questId, out var quest))
        {
            return new QuestRewardResult
            {
                Success = false,
                Message = "Quest not found."
            };
        }

        if (quest.Status != QuestStatus.Active)
        {
            return new QuestRewardResult
            {
                Success = false,
                Message = "Quest is not active."
            };
        }

        if (!quest.AreAllObjectivesCompleted())
        {
            return new QuestRewardResult
            {
                Success = false,
                Message = "Not all objectives are completed."
            };
        }

        // Get the definition for reward data
        var defId = _questDefinitionMap.GetValueOrDefault(questId, "");
        var definition = _questProvider.GetDefinition(defId);

        // Complete the quest entity
        quest.Complete();

        // Track completion
        _completedQuestIds.Add(defId);

        // Clean up objective index entries for this quest
        CleanupObjectiveIndex(questId);

        // Remove from active tracking (keep in completed tracking)
        _activeQuests.Remove(questId);
        _questDefinitionMap.Remove(questId);

        var rewards = definition?.Rewards ?? new QuestRewardDto();

        _eventLogger?.LogQuest("QuestCompleted",
            $"Quest completed: {quest.Name}",
            data: new Dictionary<string, object>
            {
                ["QuestId"] = defId,
                ["Experience"] = rewards.Experience,
                ["Currency"] = rewards.Currency
            });

        _logger.LogInformation(
            "Quest completed: {QuestName} ({QuestId}) - XP: {XP}, Gold: {Gold}",
            quest.Name, defId, rewards.Experience, rewards.Currency);

        return new QuestRewardResult
        {
            Success = true,
            Message = $"Quest '{quest.Name}' completed!",
            ExperienceGranted = rewards.Experience,
            CurrencyGranted = rewards.Currency,
            ItemsGranted = rewards.Items,
            UnlockedQuests = rewards.UnlockedQuestIds
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<Quest> GetActiveQuests()
    {
        return _activeQuests.Values
            .Where(q => q.Status == QuestStatus.Active)
            .ToList();
    }

    /// <inheritdoc />
    public Quest? GetActiveQuest(Guid questId)
    {
        _activeQuests.TryGetValue(questId, out var quest);
        return quest;
    }

    /// <inheritdoc />
    public IReadOnlyList<QuestDefinitionDto> GetAvailableQuestsFromNpc(
        string npcId,
        int legendLevel,
        IReadOnlySet<string> completedQuestIds)
    {
        var npcQuests = _questProvider.GetQuestsByGiver(npcId);

        return npcQuests
            .Where(q => q.MinimumLegend <= legendLevel)
            .Where(q => q.PrerequisiteQuestIds.All(prereq =>
                completedQuestIds.Contains(prereq)))
            .Where(q => !completedQuestIds.Contains(q.QuestId) || q.IsRepeatable)
            .Where(q => !_activeQuests.Values.Any(active =>
                active.DefinitionId.Equals(q.QuestId, StringComparison.OrdinalIgnoreCase) &&
                active.Status == QuestStatus.Active))
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<QuestFailureResult> TickTimers()
    {
        var failures = new List<QuestFailureResult>();

        foreach (var quest in _activeQuests.Values.Where(q => q.Status == QuestStatus.Active && q.IsTimed))
        {
            if (quest.TickTime())
            {
                var defId = _questDefinitionMap.GetValueOrDefault(quest.Id, quest.DefinitionId);
                quest.Fail("Time has run out.");

                var result = QuestFailureResult.TimeExpired(defId, quest.Name);
                failures.Add(result);

                CleanupObjectiveIndex(quest.Id);

                _eventLogger?.LogQuest("QuestExpired",
                    $"Quest expired: {quest.Name}",
                    data: new Dictionary<string, object> { ["QuestId"] = defId });

                _logger.LogInformation("Quest expired: {QuestName} ({QuestId})", quest.Name, defId);
            }
        }

        // Remove failed quests from active tracking
        foreach (var failure in failures)
        {
            var questToRemove = _activeQuests.Values
                .FirstOrDefault(q => q.DefinitionId.Equals(failure.QuestId, StringComparison.OrdinalIgnoreCase));

            if (questToRemove != null)
            {
                _activeQuests.Remove(questToRemove.Id);
                _questDefinitionMap.Remove(questToRemove.Id);
            }
        }

        return failures;
    }

    /// <inheritdoc />
    public IReadOnlyList<QuestFailureResult> EvaluateFailureConditions(FailureCheckContext context)
    {
        var failures = new List<QuestFailureResult>();

        foreach (var quest in _activeQuests.Values.Where(q => q.Status == QuestStatus.Active).ToList())
        {
            var defId = _questDefinitionMap.GetValueOrDefault(quest.Id, quest.DefinitionId);
            var definition = _questProvider.GetDefinition(defId);

            if (definition?.FailureConditions == null)
                continue;

            foreach (var condition in definition.FailureConditions)
            {
                var failed = EvaluateCondition(condition, context);
                if (failed)
                {
                    quest.Fail(condition.Message);
                    failures.Add(QuestFailureResult.Create(
                        defId,
                        quest.Name,
                        condition.Message,
                        ParseFailureType(condition.Type)));

                    CleanupObjectiveIndex(quest.Id);
                    _activeQuests.Remove(quest.Id);
                    _questDefinitionMap.Remove(quest.Id);

                    _logger.LogInformation(
                        "Quest failed: {QuestName} ({QuestId}) - {Reason}",
                        quest.Name, defId, condition.Message);

                    break; // Only need one failure per quest
                }
            }
        }

        return failures;
    }

    /// <inheritdoc />
    public IReadOnlySet<string> GetCompletedQuestIds()
    {
        return _completedQuestIds;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

    private void CleanupObjectiveIndex(Guid questId)
    {
        var keysToClean = _objectiveIndex
            .Where(kvp => kvp.Value.Any(e => e.QuestId == questId))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToClean)
        {
            _objectiveIndex[key].RemoveAll(e => e.QuestId == questId);
            if (_objectiveIndex[key].Count == 0)
                _objectiveIndex.Remove(key);
        }
    }

    private static bool EvaluateCondition(FailureConditionDto condition, FailureCheckContext context)
    {
        return condition.Type.ToLowerInvariant() switch
        {
            "npcdied" => !string.IsNullOrEmpty(condition.TargetId) &&
                         context.DeadNpcIds.Contains(condition.TargetId),
            "itemlost" => !string.IsNullOrEmpty(condition.TargetId) &&
                          !context.InventoryItemIds.Contains(condition.TargetId),
            "reputationdropped" => !string.IsNullOrEmpty(condition.TargetId) &&
                                  condition.Threshold.HasValue &&
                                  context.FactionReputations.TryGetValue(condition.TargetId, out var rep) &&
                                  rep < condition.Threshold.Value,
            "leftarea" => !string.IsNullOrEmpty(condition.TargetId) &&
                          context.CurrentAreaId != null &&
                          !context.CurrentAreaId.Equals(condition.TargetId, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static FailureType ParseFailureType(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "timeexpired" => FailureType.TimeExpired,
            "npcdied" => FailureType.NPCDied,
            "itemlost" => FailureType.ItemLost,
            "reputationdropped" => FailureType.ReputationDropped,
            "leftarea" => FailureType.LeftArea,
            _ => FailureType.TimeExpired
        };
    }
}
