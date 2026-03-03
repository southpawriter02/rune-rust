using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing faction quest chain progression and reputation-gated availability.
/// </summary>
/// <remarks>
/// <para>Extends quest availability beyond the base <see cref="IQuestDefinitionProvider"/>'s
/// legend-level and prerequisite filtering by adding faction reputation checks per
/// SPEC-FACTION-QUESTS-001.</para>
///
/// <para>Also provides chain-level views for quest journal display: how many quests
/// are complete in each chain, what's next, and whether the next quest is reputation-locked.</para>
///
/// <para><b>Dependencies:</b></para>
/// <list type="bullet">
///   <item><description><see cref="IQuestDefinitionProvider"/> — base quest data</description></item>
///   <item><description><see cref="IReputationService"/> — reputation tier/value lookups</description></item>
/// </list>
/// </remarks>
public interface IQuestChainService
{
    /// <summary>
    /// Gets all quests available to a player, including reputation-based filtering.
    /// </summary>
    /// <param name="player">The player to check availability for.</param>
    /// <param name="legendLevel">The player's current legend level.</param>
    /// <param name="completedQuestIds">Set of quest definition IDs the player has completed.</param>
    /// <returns>
    /// A list of quests the player can currently accept. Filters out quests where:
    /// legend level is insufficient, prerequisites aren't met, OR faction reputation
    /// is below the quest's <see cref="QuestDefinitionDto.MinimumReputation"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    IReadOnlyList<QuestDefinitionDto> GetAvailableQuestsForPlayer(
        Player player,
        int legendLevel,
        IReadOnlySet<string> completedQuestIds);

    /// <summary>
    /// Gets a summary of all quest chains and their progress for a player.
    /// </summary>
    /// <param name="player">The player to check progress for.</param>
    /// <param name="completedQuestIds">Set of quest definition IDs the player has completed.</param>
    /// <returns>A list of chain summaries, one per unique ChainId in the quest config.</returns>
    IReadOnlyList<QuestChainSummaryDto> GetAllChainSummaries(
        Player player,
        IReadOnlySet<string> completedQuestIds);

    /// <summary>
    /// Gets the summary for a specific quest chain.
    /// </summary>
    /// <param name="chainId">The chain ID to query.</param>
    /// <param name="player">The player to check progress for.</param>
    /// <param name="completedQuestIds">Set of completed quest IDs.</param>
    /// <returns>The chain summary, or null if no chain with that ID exists.</returns>
    QuestChainSummaryDto? GetChainSummary(
        string chainId,
        Player player,
        IReadOnlySet<string> completedQuestIds);

    /// <summary>
    /// Gets the next uncompleted quest in a chain, ordered by <see cref="QuestDefinitionDto.ChainOrder"/>.
    /// </summary>
    /// <param name="chainId">The chain ID to query.</param>
    /// <param name="completedQuestIds">Set of completed quest IDs.</param>
    /// <returns>
    /// The next uncompleted quest in the chain, or null if the chain is fully complete
    /// or no chain with that ID exists.
    /// </returns>
    QuestDefinitionDto? GetNextQuestInChain(
        string chainId,
        IReadOnlySet<string> completedQuestIds);

    /// <summary>
    /// Gets all quests in a chain, ordered by <see cref="QuestDefinitionDto.ChainOrder"/>.
    /// </summary>
    /// <param name="chainId">The chain ID to query.</param>
    /// <returns>An ordered list of quests in the chain. Empty if no chain with that ID exists.</returns>
    IReadOnlyList<QuestDefinitionDto> GetQuestsInChain(string chainId);

    /// <summary>
    /// Checks if a specific quest is available to a player (prerequisites + reputation).
    /// </summary>
    /// <param name="quest">The quest definition to check.</param>
    /// <param name="player">The player to check availability for.</param>
    /// <param name="legendLevel">The player's current legend level.</param>
    /// <param name="completedQuestIds">Set of completed quest IDs.</param>
    /// <returns><c>true</c> if the player meets all requirements to accept this quest.</returns>
    bool IsQuestAvailable(
        QuestDefinitionDto quest,
        Player player,
        int legendLevel,
        IReadOnlySet<string> completedQuestIds);
}
