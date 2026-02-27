using RuneAndRust.Application.DTOs;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Loads quest definitions from JSON configuration (config/quests.json).
/// Registered as singleton in DI — definitions are immutable after initial load.
/// </summary>
/// <remarks>
/// <para>
/// The provider reads and parses quest definitions at application startup,
/// building indexed lookups for efficient retrieval by ID, faction, and NPC.
/// </para>
/// <para>
/// This interface lives in the Application layer. The implementation
/// (QuestDefinitionProvider) lives in Infrastructure and handles JSON I/O.
/// </para>
/// </remarks>
public interface IQuestDefinitionProvider
{
    /// <summary>Returns all loaded quest definitions.</summary>
    IReadOnlyList<QuestDefinitionDto> GetAllDefinitions();

    /// <summary>Returns a specific quest definition by ID, or null if not found.</summary>
    QuestDefinitionDto? GetDefinition(string questId);

    /// <summary>Returns all quests associated with a given faction.</summary>
    IReadOnlyList<QuestDefinitionDto> GetQuestsByFaction(string faction);

    /// <summary>Returns all quests that a specific NPC can give.</summary>
    IReadOnlyList<QuestDefinitionDto> GetQuestsByGiver(string npcId);

    /// <summary>
    /// Returns quests available to a player given their legend level and set of completed quests.
    /// Filters out quests where prerequisites aren't met or legend level is insufficient.
    /// </summary>
    IReadOnlyList<QuestDefinitionDto> GetAvailableQuests(int legendLevel, IReadOnlySet<string> completedQuestIds);

    /// <summary>Reloads definitions from disk. Used for hot-reload during development.</summary>
    void Reload();
}
