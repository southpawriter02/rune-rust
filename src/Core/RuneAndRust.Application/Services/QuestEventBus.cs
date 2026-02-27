using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Events;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Lightweight event bus that routes game events to quest objective advancement.
/// Maintains a registry of objective watches and checks incoming events against them.
/// Scoped service — one instance per game session.
/// </summary>
public class QuestEventBus : IQuestEventBus
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly IQuestService _questService;
    private readonly ILogger<QuestEventBus> _logger;

    /// <summary>Active objective watches keyed by quest ID.</summary>
    private readonly Dictionary<Guid, List<ObjectiveWatch>> _watchesByQuest = [];

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public QuestEventBus(IQuestService questService, ILogger<QuestEventBus> logger)
    {
        _questService = questService ?? throw new ArgumentNullException(nameof(questService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("QuestEventBus initialized");
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<QuestProgressResult> ProcessEvent(GameEvent gameEvent)
    {
        ArgumentNullException.ThrowIfNull(gameEvent);

        var results = new List<QuestProgressResult>();

        // Map game event type to quest objective type and extract target ID
        var mapping = MapEventToObjective(gameEvent);
        if (mapping == null)
            return results;

        var (objectiveType, targetId) = mapping.Value;

        _logger.LogDebug(
            "Processing event: {EventType} -> {ObjectiveType}, target: {TargetId}",
            gameEvent.EventType, objectiveType, targetId);

        // Advance matching objectives
        var advanced = _questService.AdvanceObjective(objectiveType, targetId);

        if (advanced)
        {
            // Build progress results from active quests
            foreach (var quest in _questService.GetActiveQuests())
            {
                for (var i = 0; i < quest.Objectives.Count; i++)
                {
                    var obj = quest.Objectives[i];
                    // We can't perfectly track which specific objective just advanced
                    // without more state, but we report all non-complete objectives
                    // that are for this target type
                    results.Add(new QuestProgressResult
                    {
                        QuestId = quest.Id,
                        QuestDefinitionId = quest.DefinitionId,
                        QuestName = quest.Name,
                        ObjectiveDescription = obj.Description,
                        CurrentProgress = obj.CurrentProgress,
                        RequiredCount = obj.RequiredCount,
                        ObjectiveCompleted = obj.IsCompleted,
                        AllObjectivesComplete = quest.AreAllObjectivesCompleted()
                    });
                }
            }
        }

        return results;
    }

    /// <inheritdoc />
    public void RegisterObjective(Guid questId, QuestObjectiveType type, string targetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);

        var watch = new ObjectiveWatch
        {
            QuestId = questId,
            ObjectiveType = type,
            TargetId = targetId
        };

        if (!_watchesByQuest.TryGetValue(questId, out var watches))
        {
            watches = [];
            _watchesByQuest[questId] = watches;
        }
        watches.Add(watch);

        _logger.LogDebug(
            "Registered objective watch: quest={QuestId}, type={Type}, target={Target}",
            questId, type, targetId);
    }

    /// <inheritdoc />
    public void RegisterQuestObjectives(Guid questId, IReadOnlyList<QuestObjectiveDto> objectives)
    {
        ArgumentNullException.ThrowIfNull(objectives);

        foreach (var objective in objectives)
        {
            if (Enum.TryParse<QuestObjectiveType>(objective.Type, ignoreCase: true, out var objType))
            {
                RegisterObjective(questId, objType, objective.TargetId);
            }
            else
            {
                _logger.LogWarning(
                    "Unknown objective type '{Type}' for quest {QuestId}, skipping watch registration",
                    objective.Type, questId);
            }
        }
    }

    /// <inheritdoc />
    public void UnregisterQuest(Guid questId)
    {
        if (_watchesByQuest.Remove(questId))
        {
            _logger.LogDebug("Unregistered all watches for quest {QuestId}", questId);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<ObjectiveWatch> GetActiveWatches()
    {
        return _watchesByQuest.Values
            .SelectMany(w => w)
            .ToList();
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Maps a game event to a quest objective type and target ID.
    /// Returns null if the event type doesn't map to any objective type.
    /// </summary>
    private static (QuestObjectiveType Type, string TargetId)? MapEventToObjective(GameEvent gameEvent)
    {
        var data = gameEvent.Data;
        if (data == null)
            return null;

        return gameEvent.EventType switch
        {
            "MonsterDefeated" when data.TryGetValue("MonsterId", out var monsterId)
                => (QuestObjectiveType.KillEnemy, monsterId?.ToString() ?? ""),

            "ItemPickedUp" when data.TryGetValue("ItemId", out var itemId)
                => (QuestObjectiveType.CollectItem, itemId?.ToString() ?? ""),

            "RoomEntered" when data.TryGetValue("RoomId", out var roomId)
                => (QuestObjectiveType.ExploreRoom, roomId?.ToString() ?? ""),

            "InteractionCompleted" when data.TryGetValue("ObjectId", out var objectId)
                => (QuestObjectiveType.InteractWithObject, objectId?.ToString() ?? ""),

            "DialogueChoiceMade" when data.TryGetValue("ChoiceId", out var choiceId)
                => (QuestObjectiveType.MakeChoice, choiceId?.ToString() ?? ""),

            "DialogueStarted" when data.TryGetValue("NpcId", out var npcId)
                => (QuestObjectiveType.TalkToNpc, npcId?.ToString() ?? ""),

            "LevelUp" when data.TryGetValue("Level", out var level)
                => (QuestObjectiveType.ReachLevel, level?.ToString() ?? ""),

            _ => null
        };
    }
}
