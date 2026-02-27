using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Events;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Lightweight publish-subscribe system that routes game events to quest objective advancement.
/// Built as an observer layer — listens to game events and checks them against active quest objectives.
/// </summary>
/// <remarks>
/// <para>
/// The event bus maintains a registry of "objective watches" — tuples of (questId, objectiveType, targetId).
/// When a game event arrives, the bus checks it against all registered watches and advances matching objectives.
/// </para>
/// <para>
/// Watches are registered when a quest is accepted and unregistered when a quest completes or fails.
/// The bus does not own quest lifecycle — it delegates objective advancement to IQuestService.
/// </para>
/// </remarks>
public interface IQuestEventBus
{
    /// <summary>
    /// Processes a game event and advances any matching quest objectives.
    /// Returns progress results for any objectives that were advanced.
    /// </summary>
    IReadOnlyList<QuestProgressResult> ProcessEvent(GameEvent gameEvent);

    /// <summary>
    /// Registers interest in a specific event type for a quest objective.
    /// Called when a quest is accepted to set up event watching.
    /// </summary>
    void RegisterObjective(Guid questId, QuestObjectiveType type, string targetId);

    /// <summary>
    /// Registers all objectives for a quest at once.
    /// Convenience method called during quest acceptance.
    /// </summary>
    void RegisterQuestObjectives(Guid questId, IReadOnlyList<QuestObjectiveDto> objectives);

    /// <summary>
    /// Unregisters all objective watches for a quest.
    /// Called when a quest is completed, failed, or abandoned.
    /// </summary>
    void UnregisterQuest(Guid questId);

    /// <summary>Returns all currently registered objective watches.</summary>
    IReadOnlyList<ObjectiveWatch> GetActiveWatches();
}

/// <summary>
/// Represents a registered objective watch — the event bus checks incoming events against these.
/// </summary>
public record ObjectiveWatch
{
    /// <summary>The quest instance ID this watch belongs to.</summary>
    public Guid QuestId { get; init; }

    /// <summary>The type of objective being watched for.</summary>
    public QuestObjectiveType ObjectiveType { get; init; }

    /// <summary>The target ID to match against event data.</summary>
    public string TargetId { get; init; } = string.Empty;
}
