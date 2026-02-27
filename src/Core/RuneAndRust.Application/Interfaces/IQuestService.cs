using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Orchestrates quest lifecycle: acceptance, progression, completion, failure.
/// Works with Quest domain entities and QuestDefinitionProvider.
/// Registered as scoped in DI — one instance per game session.
/// </summary>
public interface IQuestService
{
    /// <summary>
    /// Creates and activates a quest from a definition.
    /// Returns the created Quest entity, or null if the definition wasn't found.
    /// </summary>
    Quest? AcceptQuest(string questId);

    /// <summary>
    /// Advances objectives matching the given type and target across all active quests.
    /// Returns true if any objective was progressed.
    /// </summary>
    bool AdvanceObjective(QuestObjectiveType type, string targetId, int amount = 1);

    /// <summary>
    /// Checks if all objectives are complete for the given quest.
    /// Returns true if the quest is ready for turn-in.
    /// </summary>
    bool CheckCompletion(Guid questId);

    /// <summary>
    /// Completes the quest and returns the reward result.
    /// Throws if the quest is not active or objectives aren't complete.
    /// </summary>
    QuestRewardResult CompleteQuest(Guid questId);

    /// <summary>Returns all currently active quests.</summary>
    IReadOnlyList<Quest> GetActiveQuests();

    /// <summary>Returns a specific active quest by instance ID, or null.</summary>
    Quest? GetActiveQuest(Guid questId);

    /// <summary>Returns all quests available from a given NPC, filtered by player state.</summary>
    IReadOnlyList<QuestDefinitionDto> GetAvailableQuestsFromNpc(
        string npcId,
        int legendLevel,
        IReadOnlySet<string> completedQuestIds);

    /// <summary>
    /// Ticks all timed quest timers by one turn.
    /// Returns failure results for any quests that expired.
    /// </summary>
    IReadOnlyList<QuestFailureResult> TickTimers();

    /// <summary>
    /// Evaluates failure conditions for all active quests against the current game state.
    /// Returns failure results for any quests whose conditions were triggered.
    /// </summary>
    IReadOnlyList<QuestFailureResult> EvaluateFailureConditions(FailureCheckContext context);

    /// <summary>Returns the set of completed quest definition IDs for prerequisite checking.</summary>
    IReadOnlySet<string> GetCompletedQuestIds();
}
